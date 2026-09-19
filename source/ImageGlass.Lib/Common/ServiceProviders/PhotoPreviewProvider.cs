/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public class PhotoPreviewProvider : IPhotoPreviewProvider
{
    /// <summary>
    /// Fraction of the requested size a preview must reach to count as sharp enough.
    /// Callers request 2x the size they display (supersampling margin), so half of it is still
    /// drawn without any upscaling; below that the preview visibly blurs.
    /// </summary>
    internal const double MIN_PREVIEW_SIZE_RATIO = 0.5;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual Task<SKImage?> TryGetCachedThumbnailAsync(string filePath, int size,
        CancellationToken token = default) => Task.FromResult<SKImage?>(null);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual async Task<SKImage?> GetPreviewAsync(PhotoMetadata meta, double? minHeight,
        CancellationToken token = default, bool skipPlatformCache = false)
    {
        // 1. fast path: native scaled decode via SkiaSharp. Skipped for a plugin-owned format,
        // which Skia does not know but can still mis-sniff into a garbage frame.
        var size = (int)(minHeight ?? double.MinValue);
        var isPluginFormat = IsPluginOwnedFormat(meta);
        SKImage? imgPreview = null;

        if (!isPluginFormat)
        {
            imgPreview = await Task.Run(() => SkiaCodec.LoadThumbnail(meta.FilePath, size), token)
                .ConfigureAwait(false);
        }


        // 2. try embedded EXIF preview
        if (imgPreview.IsDisposed())
        {
            using var thumbM = meta.GetEmbeddedPreview();
            if (thumbM is not null && thumbM.Height >= minHeight)
            {
                imgPreview = SkiaCodec.FromMagick(thumbM, meta.SkiaColorSpace);
            }
        }


        // 3. process preview
        if (TryProcessImage(imgPreview, meta, out var imgProcessed))
        {
            imgPreview?.Dispose();
            imgPreview = imgProcessed;
        }


        if (imgPreview.IsDisposed()) imgPreview = null;
        return imgPreview;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual async Task<SKImage?> GetThumbnailAsync(PhotoMetadata meta, double minHeight,
        CancellationToken token = default, bool skipPlatformCache = false)
    {
        var minSize = (int)minHeight;
        var maxSize = minSize * 2;

        // 1. fast path: try to get the quick preview
        var imgPreview = await GetPreviewAsync(meta, minSize, token, skipPlatformCache);
        var isPreviewLargeEnough = IsPreviewLargeEnough(imgPreview, meta, minSize);
        PhotoTrace.Mark("thumb:preview", null, $"{meta.FilePath} -> {Describe(imgPreview)}, "
            + $"meta={meta.Width}x{meta.Height}, largeEnough={isPreviewLargeEnough}");


        // 2. slow paths: preview missing, or too small for the gallery cell (it would be scaled
        // up and look blurry). Selecting the codec probes the file, so do it once here.
        if (!isPreviewLargeEnough)
        {
            var codec = SelectDecodeCodec(meta);
            var isPluginCodec = CodecRegistry.IsPluginCodec(codec);
            PhotoTrace.Mark("thumb:codec", null,
                $"{meta.FilePath} -> {codec?.CodecId ?? "none"} (plugin={isPluginCodec})");

            // 2a. the owning plugin codec decodes it. Magick is skipped: it cannot identify the
            // format and may misread the bytes into a frame that wins KeepLarger.
            if (isPluginCodec)
            {
                var imgPlugin = await DecodeViaCodecRegistryAsync(meta, maxSize, token, codec);
                PhotoTrace.Mark("thumb:decode", null, $"{meta.FilePath} -> plugin {Describe(imgPlugin)}");
                _ = KeepLarger(ref imgPreview, imgPlugin);
            }

            // 2b. built-in formats: let ImageMagick read what SkiaSharp could not
            else
            {
                using var imgM = await MagickCodec.QuickDecodeAsync(meta.FilePath, maxSize, maxSize, token: token);
                var imgMagick = SkiaCodec.FromMagick(imgM, meta.SkiaColorSpace);
                PhotoTrace.Mark("thumb:decode", null, $"{meta.FilePath} -> magick {Describe(imgMagick)}");

                // an undersized preview is still better than nothing if Magick did no better
                _ = KeepLarger(ref imgPreview, imgMagick);


                // 2c. slowest path: decode through the codec registry, which reaches codecs
                // (e.g. the SVG renderer) that neither quick path above covers
                if (imgPreview.IsDisposed())
                {
                    imgPreview = await DecodeViaCodecRegistryAsync(meta, maxSize, token, codec);
                }
            }
        }


        // 3. shrink anything bigger than requested. The Shell hands back whole cache tiers and the
        // Magick/codec fallbacks decode at maxSize for headroom, so results are routinely oversized;
        // keeping them would cost several MB of gallery memory per photo. Skia-only: a Magick
        // round-trip here costs ~170ms per thumbnail and dominates gallery load time.
        if (minSize > 0 && (imgPreview?.Width > minSize || imgPreview?.Height > minSize))
        {
            var imgScaled = await Task.Run(() => SkiaCodec.ScaleDown(imgPreview, minSize), token)
                .ConfigureAwait(false);
            if (!imgScaled.IsDisposed())
            {
                imgPreview?.Dispose();
                imgPreview = imgScaled;
            }
        }


        if (imgPreview.IsDisposed()) imgPreview = null;
        return imgPreview;
    }


    /// <summary>
    /// Checks whether <paramref name="img"/> can fill a <paramref name="requestedSize"/> box
    /// without being scaled up. A source smaller than the request caps what any provider is able
    /// to return, so the source's own size becomes the target in that case.
    /// </summary>
    public static bool IsPreviewLargeEnough(SKImage? img, PhotoMetadata meta, double requestedSize)
    {
        if (img.IsDisposed()) return false;
        if (requestedSize <= 0) return true;

        // the supersampling margin is optional, the source size is not: when the source is
        // smaller than the request, its full size is the sharpest result obtainable
        var wantedSize = requestedSize * MIN_PREVIEW_SIZE_RATIO;
        var srcLongestSide = (double)Math.Max(meta.Width, meta.Height);
        if (srcLongestSide > 0) wantedSize = Math.Min(wantedSize, srcLongestSide);

        var imgLongestSide = (double)Math.Max(img.Width, img.Height);
        return imgLongestSide >= wantedSize;
    }


    /// <summary>
    /// Keeps whichever of <paramref name="current"/> and <paramref name="candidate"/> has the
    /// larger longest side and disposes the other. Returns <c>true</c> when
    /// <paramref name="candidate"/> won.
    /// </summary>
    protected static bool KeepLarger(ref SKImage? current, SKImage? candidate)
    {
        if (candidate.IsDisposed()) return false;

        var currentSide = GetLongestSide(current);
        var candidateSide = GetLongestSide(candidate);

        if (candidateSide <= currentSide)
        {
            candidate.Dispose();
            return false;
        }

        current?.Dispose();
        current = candidate;
        return true;
    }


    /// <summary>
    /// Formats an image's size for the trace log.
    /// </summary>
    protected static string Describe(SKImage? img)
        => img.IsDisposed() ? "none" : $"{img!.Width}x{img.Height}";


    /// <summary>
    /// Gets the longest side of the image, or <c>0</c> if it is null or disposed.
    /// </summary>
    private static int GetLongestSide(SKImage? img)
    {
        if (img.IsDisposed()) return 0;

        return Math.Max(img.Width, img.Height);
    }


    /// <summary>
    /// Whether a plugin codec claims this file's extension. The content-sniffing built-in decoders
    /// can return a garbage frame instead of failing on such a file, so they must not run first.
    /// </summary>
    protected static bool IsPluginOwnedFormat(PhotoMetadata? meta)
    {
        return Core.CodecRegistry.IsDecodingExtensionOwnedByPlugin(meta?.FileExtension);
    }


    /// <summary>
    /// Selects the codec that would decode <paramref name="meta"/>, or <c>null</c> when none
    /// claims it. Not free: the built-in codecs answer by probing the file header.
    /// </summary>
    protected static ICodec? SelectDecodeCodec(PhotoMetadata meta)
    {
        var context = CreateCodecContext();
        var codec = Core.CodecRegistry.SelectDecodeCodec(meta, context);

        return codec;
    }


    /// <summary>
    /// Builds the codec-selection context used by the thumbnail decode paths.
    /// </summary>
    private static CodecSelectionContext CreateCodecContext()
    {
        return new CodecSelectionContext
        {
            EnableVectorRenderer = Core.Config.EnableVectorRenderer,
            IsDestColorProfileSupported = Core.IsDestColorProfileSupported,
        };
    }


    /// <summary>
    /// Decodes the image through the codec registry and returns its raster frame.
    /// This lets custom/plugin codecs produce a thumbnail for formats that the
    /// built-in SkiaSharp/ImageMagick paths cannot decode. Orientation and color
    /// management are intentionally left to the codec (as in the full-image decode
    /// path), so no further processing is applied here.
    /// </summary>
    protected static async Task<SKImage?> DecodeViaCodecRegistryAsync(PhotoMetadata meta,
        int maxSize, CancellationToken token, ICodec? selectedCodec = null)
    {
        var context = CreateCodecContext();
        var codec = selectedCodec ?? Core.CodecRegistry.SelectDecodeCodec(meta, context);
        if (codec is null) return null;

        var options = new PhotoReadOptions
        {
            Width = (uint)Math.Max(0, maxSize),
            Height = (uint)Math.Max(0, maxSize),
        };

        using var result = await codec.DecodeAsync(meta, options, context, token).ConfigureAwait(false);

        // an animated codec hands back an animator instead of a frame, so take the first
        // frame from it. The animator owns its frame cache, hence the copy.
        if (result.SingleFrame is null && result.Animator is not null)
        {
            var animatorFrame = result.Animator.GetRenderedFrameBitmap(0);
            var ownedFrame = CopyImage(animatorFrame);
            return ownedFrame;
        }

        // detach the raster frame so disposing the result does not dispose it
        var imgFrame = result.SingleFrame;
        result.SingleFrame = null;
        return imgFrame;
    }


    /// <summary>
    /// Copies an image owned by someone else into one the caller owns.
    /// </summary>
    private static SKImage? CopyImage(SKImage? imgSrc)
    {
        if (imgSrc.IsDisposed()) return null;

        using var pixmap = imgSrc.PeekPixels();
        if (pixmap is null) return null;

        var imgCopy = SKImage.FromPixelCopy(pixmap);
        return imgCopy;
    }


    /// <summary>
    /// Processes the preview image by applying orientation and color management adjustments.
    /// </summary>
    protected static bool TryProcessImage(SKImage? imgPreview, PhotoMetadata meta, out SKImage? output)
    {
        output = null;
        if (imgPreview.IsDisposed()) return false;


        // 1. apply orientation
        if (SkiaCodec.TryApplyOrientation(imgPreview, meta.Orientation, out var imgOriented))
        {
            output?.Dispose();
            output = imgOriented;
        }


        // 2. apply color management
        if (SkiaCodec.TryApplyColorSpace(output ?? imgPreview, Core.DestColorProfile, out var imgFrameColored))
        {
            output?.Dispose();
            output = imgFrameColored;
        }

        // false keeps the caller's own image: neither step applied, so there is nothing to swap in
        return output is not null;
    }

}
