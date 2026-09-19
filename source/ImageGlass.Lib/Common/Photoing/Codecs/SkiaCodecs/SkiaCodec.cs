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
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cysharp.Collections;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using ImageMagick;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

public static partial class SkiaCodec
{
    /// <summary>
    /// Skia refuses a single pixel buffer over this size, however much memory is free.
    /// </summary>
    private const long MAX_PIXEL_BUFFER_BYTES = int.MaxValue;

    /// <summary>
    /// JPEG scales natively in the IDCT at eighths; largest-first to lose the least detail.
    /// </summary>
    private static readonly float[] NATIVE_DECODE_SCALES =
        [7 / 8f, 6 / 8f, 5 / 8f, 4 / 8f, 3 / 8f, 2 / 8f, 1 / 8f];


    /// <summary>
    /// Loads photo metadata from file path.
    /// </summary>
    public static async Task<PhotoMetadata> LoadMetadataAsync(string? filePath,
        PhotoReadOptions? options = null, CancellationToken token = default)
    {
        filePath ??= string.Empty;
        var meta = new PhotoMetadata(filePath);

        // 0. get file info
        if (string.IsNullOrWhiteSpace(filePath)) return meta;

        // SVG: delegate to SvgCodec for metadata
        if (SvgCodec.IsSvgFile(filePath))
        {
            return await SvgCodec.LoadMetadataAsync(filePath, token);
        }

        // create Skia codec
        using var decoder = SKCodec.Create(filePath);
        if (decoder.IsDisposed()) return meta;


        // 1. calculate the specific frame index
        var frameIndex = options?.FrameIndex ?? 0;

        // make sure frame index is within range
        if (frameIndex >= decoder.FrameCount) frameIndex = 0;
        else if (frameIndex < 0) frameIndex = (int)decoder.FrameCount - 1;

        meta.FrameCount = (uint)decoder.FrameCount;
        if (token.IsCancellationRequested) return meta;


        // 2. read metadata of first frame only
        var readingTask = Task.Run(() =>
        {
            try
            {
                // get animation frames
                var skiaFramesInfo = SkiaCodec.GetFramesMetadata(meta.FilePath);
                meta.CanAnimate = skiaFramesInfo?.Count > 0;

                // get frame metadata
                meta.Frames = skiaFramesInfo?.Select(aniInfo => new FrameMetadata
                {
                    Animation = aniInfo,
                }).ToImmutableList() ?? [];

                // image size
                meta.OriginalWidth = meta.Width = (uint)decoder.Info.Width;
                meta.OriginalHeight = meta.Height = (uint)decoder.Info.Height;

                meta.HasAlpha = !decoder.Info.IsOpaque;
            }
            catch { }
            if (token.IsCancellationRequested) return;


            // get color profile
            try
            {
                // get embedded color profile
                meta.SkiaColorSpace = decoder.Info.ColorSpace;
                using var colorProfile = decoder.Info.ColorSpace.ToProfile();

                if (colorProfile.Size > 0)
                {
                    var bytes = new byte[colorProfile.Size];
                    unsafe
                    {
                        new ReadOnlySpan<byte>((void*)colorProfile.Buffer, (int)colorProfile.Size)
                            .CopyTo(bytes);
                    }

                    // create photo profile
                    var photoProfile = new PhotoColorProfile(bytes);

                    meta.ColorProfileName = photoProfile.GetIccDescription();
                    if (photoProfile.ProfileData is not null)
                    {
                        meta.MagickColorProfile = new ColorProfile(photoProfile.ProfileData);
                    }
                }

                // save to meta
                meta.ColorSpace = decoder.Info.ColorSpace.IsSrgb
                    ? ImageMagick.ColorSpace.sRGB
                    : ImageMagick.ColorSpace.Undefined;
            }
            catch { }


            // detect HDR transfer function, wide gamut, and bit depth
            meta.BitsPerChannel = GetBitsPerChannel(decoder.Info.ColorType);
            MagickCodec.DetectHdrInfo(meta);


            // detect motion/live photo
            if (!token.IsCancellationRequested)
            {
                var liveInfo = LivePhotoDetector.Detect(meta.FilePath);
                meta.EmbeddedVideoOffsetFromEnd = liveInfo.IsLivePhoto
                    ? liveInfo.EmbeddedVideoOffsetFromEnd
                    : 0;
            }
        }, token).ConfigureAwait(false);


        await readingTask;

        return meta;
    }


    /// <summary>
    /// Loads photo.
    /// </summary>
    public static async Task<SkiaDecoderOutput> LoadAsync(PhotoMetadata meta,
        PhotoReadOptions options, CancellationToken token)
    {
        return await Task.Run(() => Load(meta, options), token).ConfigureAwait(false);
    }


    /// <summary>
    /// Loads photo.
    /// </summary>
    public static SkiaDecoderOutput Load(PhotoMetadata meta, PhotoReadOptions options)
    {
        // A. SVG format: use SvgCodec for decoding
        if (meta.IsVector && SvgCodec.IsSvgFile(meta.FilePath))
        {
            var svgResult = new SkiaDecoderOutput();
            var svgDoc = SvgCodec.LoadSvg(meta.FilePath);
            var picture = svgDoc.Picture;

            if (picture is not null)
            {
                var intrinsicSize = SvgCodec.GetIntrinsicSize(picture);
                svgResult.Size = new Size(intrinsicSize.Width, intrinsicSize.Height);

                // pre-rasterize fallback for pixel operations (copy, export)
                // on background thread to avoid blocking UI
                var maxDim = (int)Math.Max(intrinsicSize.Width, intrinsicSize.Height);
                var rasterized = SvgCodec.RasterizeThumbnail(picture, Math.Min(maxDim, 1024));

                svgResult.VectorSource = new SkiaVectorSource(svgDoc, intrinsicSize, rasterized);
            }
            else
            {
                svgDoc.Dispose();
            }

            return svgResult;
        }


        // B. Rasterize format
        // 0. create Skia codec
        var codec = SKCodec.Create(meta.FilePath);
        var result = new SkiaDecoderOutput();
        result.Size = new Size(codec.Info.Width, codec.Info.Height);


        // 1. read animated formats
        if (codec.FrameCount > 0)
        {
            var frames = meta.Frames.Select(f => f.Animation ?? CreateUnknownFrameInfo()).ToArray();
            result.Animator = new SkiaAnimator(codec, frames);
            return result;
        }


        // 2. read single-frame formats
        // images past Skia's pixel-buffer ceiling decode at the largest native reduction that fits
        var decodeInfo = GetDecodableImageInfo(codec, out var decodeScale);

        // a caller that asked for a size gets a native reduced decode, like Magick's ApplySizeSettings
        if (TryGetRequestedDecodeInfo(codec, decodeInfo, options, out var requestedInfo))
        {
            decodeScale *= (double)requestedInfo.Width / decodeInfo.Width;
            decodeInfo = requestedInfo;
        }

        result.DecodeScale = decodeScale;
        result.Size = new Size(decodeInfo.Width, decodeInfo.Height);

        using var bmpFrame = new SKBitmap(decodeInfo);
        var frameIndex = Math.Min(0, options.FrameIndex);
        var codecOption = new SKCodecOptions(frameIndex);

        if (codec.GetPixels(decodeInfo, bmpFrame.GetPixels(), codecOption) == SKCodecResult.Success)
        {
            // piex reports no origin for most RAW containers, so fall back to the one the metadata Ping read
            var origin = codec.EncodedOrigin is SKEncodedOrigin.TopLeft or SKEncodedOrigin.Default
                ? meta.Orientation
                : codec.EncodedOrigin;

            // 2.1 correct rotation
            if (options.CorrectRotation)
            {
                if (TryApplyOrientation(bmpFrame, origin, out var bmpOriented))
                {
                    if (bmpOriented is not null)
                    {
                        using (bmpOriented)
                        {
                            result.Size = new Size(bmpOriented.Width, bmpOriented.Height);
                            result.SingleFrame = ToSKImageNoCopy(bmpOriented);
                        }
                        bmpFrame.Dispose();
                    }
                }

                if (bmpOriented is null)
                {
                    result.SingleFrame = ToSKImageNoCopy(bmpFrame);
                }
            }
            else
            {
                result.SingleFrame = ToSKImageNoCopy(bmpFrame);
            }
        }

        codec.Dispose();
        codec = null;

        return result;
    }


    /// <summary>
    /// Picks the largest info the codec can decode into one pixel buffer, stepping down
    /// through native scales when the full size does not fit. <paramref name="scale"/> is 1 if unreduced.
    /// </summary>
    /// <exception cref="NotSupportedException">Over the ceiling and the codec cannot scale.</exception>
    private static SKImageInfo GetDecodableImageInfo(SKCodec codec, out double scale)
    {
        scale = 1;
        var fullInfo = codec.Info;
        if (FitsInPixelBuffer(fullInfo)) return fullInfo;

        foreach (var candidate in NATIVE_DECODE_SCALES)
        {
            var size = codec.GetScaledDimensions(candidate);

            // codecs without native scaling just echo the full size back
            if (size.Width >= fullInfo.Width || size.Width <= 0 || size.Height <= 0) continue;

            var scaledInfo = fullInfo.WithSize(size.Width, size.Height);
            if (!FitsInPixelBuffer(scaledInfo)) continue;

            scale = (double)size.Width / fullInfo.Width;
            return scaledInfo;
        }

        var megaPixels = (double)fullInfo.Width * fullInfo.Height / 1_000_000;
        throw new NotSupportedException(
            $"The image is too large to open: {fullInfo.Width:n0}x{fullInfo.Height:n0} "
            + $"({megaPixels:n0} MP) needs {(double)fullInfo.Width * fullInfo.Height * fullInfo.BytesPerPixel / 1024 / 1024 / 1024:n2} GB "
            + $"in one buffer, but the renderer cannot address more than 2 GB per image. "
            + $"This format cannot be decoded at a reduced size.");
    }


    /// <summary>
    /// Checks whether a full pixel buffer for the info stays within Skia's byte-size ceiling.
    /// </summary>
    private static bool FitsInPixelBuffer(SKImageInfo info)
    {
        return (long)info.Width * info.Height * info.BytesPerPixel <= MAX_PIXEL_BUFFER_BYTES;
    }


    /// <summary>
    /// Returns the linear scale that brings a <c>width * height * bytesPerPixel</c> buffer
    /// under the ceiling, or 1 when it already fits.
    /// </summary>
    private static double GetPixelBufferFitScale(long width, long height, int bytesPerPixel)
    {
        var bytes = width * height * bytesPerPixel;
        if (bytes <= 0 || bytes <= MAX_PIXEL_BUFFER_BYTES) return 1;

        // area grows with the square of the linear scale; trim slightly to absorb rounding up
        return Math.Sqrt((double)MAX_PIXEL_BUFFER_BYTES / bytes) * 0.999;
    }


    /// <summary>
    /// Loads a thumbnail using native scaled decoding.
    /// For JPEG, this uses libjpeg-turbo's IDCT scaling to decode at
    /// a reduced resolution (1/2, 1/4, 1/8) without processing the full image data.
    /// </summary>
    public static SKImage? LoadThumbnail(string filePath, int maxDimension)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        try
        {
            using var codec = SKCodec.Create(filePath);
            if (codec.IsDisposed()) return null;

            var origW = codec.Info.Width;
            var origH = codec.Info.Height;
            if (origW <= 0 || origH <= 0) return null;

            // calculate the scale needed to fit within maxDimension
            var scale = Math.Min((float)maxDimension / origW, (float)maxDimension / origH);
            scale = Math.Min(scale, 1f); // don't upscale

            // get the native scaled dimensions (JPEG supports 1/2, 1/4, 1/8 natively)
            var scaledSize = codec.GetScaledDimensions(scale);

            var info = new SKImageInfo(scaledSize.Width, scaledSize.Height,
                SKColorType.Bgra8888, SKAlphaType.Premul);
            using var bmp = new SKBitmap(info);

            var result = codec.GetPixels(info, bmp.GetPixels());
            if (result is not SKCodecResult.Success and not SKCodecResult.IncompleteInput)
            {
                return null;
            }

            return ToSKImage(bmp);
        }
        catch { return null; }
    }


    /// <summary>
    /// Saves photo to file using Magick codec.
    /// </summary>
    /// <returns><c>true</c> if file is saved.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task SaveAsync(SKImage? srcImg, string destFilePath,
        PhotoTransform? transform = null, uint quality = 100, CancellationToken token = default)
    {
        if (srcImg.IsDisposed()) return;

        try
        {
            // 1. apply transforms
            using var transformedImg = TransformImage(srcImg, transform);
            var dstImg = transformedImg ?? srcImg;
            if (dstImg.IsDisposed() || token.IsCancellationRequested) return;


            // 2. use Magick to save
            if (MagickCodec.CanWrite(destFilePath))
            {
                using var imgM = ToMagick(dstImg);
                if (imgM is null) return;

                // write image data to file
                imgM.Quality = quality;
                await imgM.WriteAsync(destFilePath, token);
            }
            else
            {
                throw new FormatException("IGE: Unsupported image format.");
            }

        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Checks if the photo can be pinged to get metadata.
    /// </summary>
    public static bool CanPing(string srcFilePath)
    {
        using var codec = SKCodec.Create(srcFilePath);
        if (codec.IsDisposed()) return false;

        return true;
    }


    /// <summary>
    /// Checks if the photo can be decoded.
    /// </summary>
    public static bool CanRead(string srcFilePath)
    {
        using var meta = new PhotoMetadata()
        {
            FilePath = srcFilePath,
        };

        return CanRead(meta);
    }


    /// <summary>
    /// Checks if the photo can be decoded.
    /// </summary>
    public static bool CanRead(PhotoMetadata meta)
    {
        using var codec = SKCodec.Create(meta.FilePath);
        if (codec.IsDisposed()) return false;

        // multiple frames
        var isMultiFrames = meta.FrameCount > 1;
        if (isMultiFrames) return codec.FrameCount > 1;

        return true;
    }


    /// <summary>
    /// Narrows the decode to the size the caller asked for, using the codec's native scales.
    /// </summary>
    private static bool TryGetRequestedDecodeInfo(SKCodec codec, SKImageInfo currentInfo,
        PhotoReadOptions options, out SKImageInfo output)
    {
        output = currentInfo;
        if (options.Width == 0 || options.Height == 0) return false;
        if (currentInfo.Width <= options.Width && currentInfo.Height <= options.Height) return false;

        var scale = Math.Min((float)options.Width / currentInfo.Width,
            (float)options.Height / currentInfo.Height);
        var size = codec.GetScaledDimensions(scale);
        if (size.Width <= 0 || size.Height <= 0 || size.Width >= currentInfo.Width) return false;

        output = currentInfo.WithSize(size.Width, size.Height);
        return true;
    }


    /// <summary>
    /// Checks if the RAW embedded preview can be decoded and is at least the requested size.
    /// </summary>
    public static bool CanReadRawPreview(PhotoMetadata meta, int minWidth, int minHeight)
    {
        using var codec = SKCodec.Create(meta.FilePath);
        if (codec.IsDisposed()) return false;

        return codec.Info.Width >= minWidth && codec.Info.Height >= minHeight;
    }


    /// <summary>
    /// Derives bits per channel from a SkiaSharp color type.
    /// </summary>
    private static int GetBitsPerChannel(SKColorType colorType) => colorType switch
    {
        SKColorType.Rgba8888 or SKColorType.Bgra8888
            or SKColorType.Rgb888x or SKColorType.Gray8
            or SKColorType.Bgr101010xXR => 8,
        SKColorType.Rgba1010102 or SKColorType.Bgra1010102
            or SKColorType.Rgb101010x or SKColorType.Bgr101010x => 10,
        SKColorType.Rgba16161616
            or SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped => 16,
        SKColorType.RgbaF32 => 32,
        _ => 8,
    };


    /// <summary>
    /// Returns <c>true</c> when the color type stores more than 8 bits per channel,
    /// indicating a high-bit-depth or HDR pixel format.
    /// </summary>
    public static bool IsHighBitDepthColorType(SKColorType colorType) => colorType is
        SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped
        or SKColorType.RgbaF32
        or SKColorType.Rgba16161616
        or SKColorType.Rgba1010102 or SKColorType.Bgra1010102
        or SKColorType.Rgb101010x or SKColorType.Bgr101010x;


    /// <summary>
    /// Extracts metadata for all frames.
    /// </summary>
    public static List<SKCodecFrameInfo>? GetFramesMetadata(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;


        using var data = SKData.Create(filePath);
        using var codec = SKCodec.Create(data);
        if (codec == null) return null;

        int frameCount = codec.FrameCount;
        var metadataList = new List<SKCodecFrameInfo>(frameCount);
        var readCount = 0;

        for (int i = 0; i < frameCount; i++)
        {
            // GetFrameInfo provides duration and alpha info
            if (codec.GetFrameInfo(i, out var info))
            {
                metadataList.Add(info);
                readCount++;
            }
            else
            {
                metadataList.Add(CreateUnknownFrameInfo());
            }
        }

        // an animation whose every frame is unreadable is not usable; a still image keeps its empty list
        if (frameCount > 0 && readCount == 0) return null;

        return metadataList;
    }


    /// <summary>
    /// Creates the stand-in for a frame whose info the codec cannot read.
    /// </summary>
    private static SKCodecFrameInfo CreateUnknownFrameInfo() => new()
    {
        // independent, so nothing composes itself onto a frame we know nothing about
        RequiredFrame = -1,

        // AnimatorImpl.GetFrameDelay substitutes its default for a zero delay
        Duration = 0,

        DisposalMethod = SKCodecAnimationDisposalMethod.Keep,
    };


    /// <summary>
    /// Returns the new transformed image by applying flip, rotation, and color inversion.
    /// </summary>
    public static SKImage? TransformImage(SKImage? imgSrc, PhotoTransform? transform)
    {
        if (imgSrc.IsDisposed() || transform is null || !transform.HasChanges) return null;

        SKImage? result = null;

        // flip
        if (transform.Flips != FlipOptions.None)
        {
            var flipped = FlipImage(result ?? imgSrc, transform.Flips);
            result?.Dispose();
            result = flipped;
        }

        // rotation
        if (transform.Rotation != 0)
        {
            var rotated = RotateImage(result ?? imgSrc, transform.Rotation);
            result?.Dispose();
            result = rotated;
        }

        // invert color
        if (transform.IsColorInverted)
        {
            var inverted = InvertImageColors(result ?? imgSrc);
            result?.Dispose();
            result = inverted;
        }

        return result;
    }


    /// <summary>
    /// Returns a new image with inverted RGB colors, preserving alpha.
    /// </summary>
    public static SKImage? InvertImageColors(SKImage? imgSrc)
    {
        if (imgSrc.IsDisposed()) return null;

        float[] invertMatrix =
        [
            -1,  0,  0,  0,  1,
             0, -1,  0,  0,  1,
             0,  0, -1,  0,  1,
             0,  0,  0,  1,  0,
        ];

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(invertMatrix),
        };

        var info = new SKImageInfo(imgSrc.Width, imgSrc.Height, imgSrc.ColorType, imgSrc.AlphaType, imgSrc.ColorSpace);
        using var surface = SKSurface.Create(info);
        if (surface.IsDisposed()) return null;

        surface.Canvas.DrawImage(imgSrc, 0, 0, paint);
        return surface.Snapshot();
    }


    /// <summary>
    /// Returns a new image with the specified color channels filtered.
    /// Single-channel views (R, G, or B only) are shown as grayscale.
    /// Alpha-only shows the alpha channel as grayscale with opaque output.
    /// </summary>
    public static SKImage? FilterImageColorChannels(SKImage? imgSrc, ColorChannels colors)
    {
        if (imgSrc.IsDisposed()) return null;

        var redOnly = colors.HasFlag(ColorChannels.R)
            && !colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.B);
        var greenOnly = colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.R)
            && !colors.HasFlag(ColorChannels.B);
        var blueOnly = colors.HasFlag(ColorChannels.B)
            && !colors.HasFlag(ColorChannels.G)
            && !colors.HasFlag(ColorChannels.R);
        var alphaOnly = colors == ColorChannels.A;
        var keepAlpha = colors.HasFlag(ColorChannels.A) && !alphaOnly;

        var red = !alphaOnly && colors.HasFlag(ColorChannels.R) ? 1f : 0f;
        var green = !alphaOnly && colors.HasFlag(ColorChannels.G) ? 1f : 0f;
        var blue = !alphaOnly && colors.HasFlag(ColorChannels.B) ? 1f : 0f;

        // for single-channel views, spread the channel value to all RGB outputs
        var mRed = redOnly ? 1f : 0f;
        var mGreen = greenOnly ? 1f : 0f;
        var mBlue = blueOnly ? 1f : 0f;

        // for alpha-only, map alpha to RGB for grayscale visualization
        var fromAlpha = alphaOnly ? 1f : 0f;
        var alphaScale = alphaOnly ? 0f : 1f;
        var alphaOffset = alphaOnly ? 1f : 0f;

        // when alpha channel is not selected and not alpha-only,
        // blend with black background instead of forcing opaque
        var blendWithBlack = !keepAlpha && !alphaOnly;

        #pragma warning disable format
        float[] matrix =
        [
            red,    mGreen, mBlue,  fromAlpha,      0,
            mRed,   green,  mBlue,  fromAlpha,      0,
            mRed,   mGreen, blue,   fromAlpha,      0,
            0,      0,      0,      alphaScale,     alphaOffset,
        ];
        #pragma warning restore format

        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(matrix),
        };

        var info = new SKImageInfo(imgSrc.Width, imgSrc.Height, imgSrc.ColorType, imgSrc.AlphaType, imgSrc.ColorSpace);
        using var surface = SKSurface.Create(info);
        if (surface.IsDisposed()) return null;

        // blend transparent pixels with black background
        if (blendWithBlack)
        {
            surface.Canvas.Clear(SKColors.Black);
        }

        surface.Canvas.DrawImage(imgSrc, 0, 0, paint);
        return surface.Snapshot();
    }


    /// <summary>
    /// Returns a new image rotated by the specified degree.
    /// Handles arbitrary angles by computing the bounding box and centering.
    /// </summary>
    public static SKImage? RotateImage(SKImage? imgSrc, double degree)
    {
        if (imgSrc.IsDisposed()) return null;

        // normalize to [0, 360)
        degree = ((degree % 360) + 360) % 360;
        if (degree == 0) return null;

        var w = imgSrc.Width;
        var h = imgSrc.Height;

        // compute the bounding box of the rotated image
        var rad = degree * Math.PI / 180.0;
        var cos = Math.Abs(Math.Cos(rad));
        var sin = Math.Abs(Math.Sin(rad));
        var outW = (int)Math.Ceiling(w * cos + h * sin);
        var outH = (int)Math.Ceiling(w * sin + h * cos);

        var info = new SKImageInfo(outW, outH, imgSrc.ColorType, imgSrc.AlphaType, imgSrc.ColorSpace);
        using var surface = SKSurface.Create(info);
        if (surface.IsDisposed()) return null;

        var canvas = surface.Canvas;

        // move origin to the center of the output, rotate, then draw centered
        canvas.Translate(outW / 2f, outH / 2f);
        canvas.RotateDegrees((float)degree);
        canvas.Translate(-w / 2f, -h / 2f);
        canvas.DrawImage(imgSrc, 0, 0);

        return surface.Snapshot();
    }


    /// <summary>
    /// Returns a new image flipped according to the specified options.
    /// </summary>
    public static SKImage? FlipImage(SKImage? imgSrc, FlipOptions options)
    {
        if (imgSrc.IsDisposed() || options == FlipOptions.None) return null;

        var w = imgSrc.Width;
        var h = imgSrc.Height;
        var info = new SKImageInfo(w, h, imgSrc.ColorType, imgSrc.AlphaType, imgSrc.ColorSpace);
        using var surface = SKSurface.Create(info);
        if (surface.IsDisposed()) return null;

        var canvas = surface.Canvas;

        if (options.HasFlag(FlipOptions.Horizontal))
        {
            canvas.Scale(-1, 1, w / 2f, 0);
        }
        if (options.HasFlag(FlipOptions.Vertical))
        {
            canvas.Scale(1, -1, 0, h / 2f);
        }

        canvas.DrawImage(imgSrc, 0, 0);
        return surface.Snapshot();
    }


    /// <summary>
    /// Tries to apply the specified orientation to the source bitmap and outputs the transformed bitmap if
    /// successful.
    /// </summary>
    public static bool TryApplyOrientation(SKImage? imgSrc, SKEncodedOrigin? orientation, out SKImage? output)
    {
        output = null;
        if (imgSrc.IsDisposed()) return false;

        var bmpSrc = SKBitmap.FromImage(imgSrc);

        if (TryApplyOrientation(bmpSrc, orientation, out var bmpOriented))
        {
            bmpSrc?.Dispose();
            bmpSrc = null;

            output = SKImage.FromBitmap(bmpOriented);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Tries to apply the specified orientation to the source bitmap and outputs the transformed bitmap if
    /// successful.
    /// </summary>
    public static bool TryApplyOrientation(SKBitmap? bmpSrc, SKEncodedOrigin? orientation, out SKBitmap? output)
    {
        output = null;
        if (bmpSrc.IsDisposed()) return false;

        var origin = orientation ?? SKEncodedOrigin.TopLeft;
        if (origin is SKEncodedOrigin.TopLeft or SKEncodedOrigin.Default) return false;


        // 1. determine if the result dimensions are swapped (90°/270° rotations)
        var swapDims = origin is SKEncodedOrigin.LeftTop
            or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom
            or SKEncodedOrigin.LeftBottom;

        var w = swapDims ? bmpSrc.Height : bmpSrc.Width;
        var h = swapDims ? bmpSrc.Width : bmpSrc.Height;

        var info = bmpSrc.Info.WithSize(w, h);
        var bmpOutput = new SKBitmap(info);

        try
        {
            // 2. fix the orientation
            using (var surface = SKSurface.Create(info, bmpOutput.GetPixels(), info.RowBytes))
            {
                var canvas = surface.Canvas;
                switch (origin)
                {
                    case SKEncodedOrigin.TopRight:
                        canvas.Translate(w, 0);
                        canvas.Scale(-1, 1);
                        break;

                    case SKEncodedOrigin.BottomRight:
                        canvas.RotateDegrees(180, w * 0.5f, h * 0.5f);
                        break;

                    case SKEncodedOrigin.BottomLeft:
                        canvas.Translate(0, h);
                        canvas.Scale(1, -1);
                        break;

                    case SKEncodedOrigin.LeftTop:
                        canvas.RotateDegrees(90);
                        canvas.Scale(1, -1);
                        break;

                    case SKEncodedOrigin.RightTop:
                        canvas.Translate(w, 0);
                        canvas.RotateDegrees(90);
                        break;

                    case SKEncodedOrigin.RightBottom:
                        canvas.Translate(w, h);
                        canvas.RotateDegrees(-90);
                        canvas.Scale(1, -1);
                        break;

                    case SKEncodedOrigin.LeftBottom:
                        canvas.Translate(0, h);
                        canvas.RotateDegrees(-90);
                        break;
                }

                canvas.DrawBitmap(bmpSrc, 0, 0);
                canvas.Flush();
            }


            output = bmpOutput;
            return true;
        }
        catch
        {
            bmpOutput.Dispose();
        }

        return false;
    }


    /// <summary>
    /// Converts the Skia's <see cref="SKEncodedOrigin"/> value to Magick's <see cref="OrientationType"/> value.
    /// </summary>
    public static OrientationType ToMagickOrientation(SKEncodedOrigin orientation)
    {
        return orientation switch
        {
            SKEncodedOrigin.TopLeft => OrientationType.TopLeft,
            SKEncodedOrigin.TopRight => OrientationType.TopRight,
            SKEncodedOrigin.BottomRight => OrientationType.BottomRight,
            SKEncodedOrigin.BottomLeft => OrientationType.BottomLeft,
            SKEncodedOrigin.LeftTop => OrientationType.LeftTop,
            SKEncodedOrigin.RightTop => OrientationType.RightTop,
            SKEncodedOrigin.RightBottom => OrientationType.RightBottom,
            SKEncodedOrigin.LeftBottom => OrientationType.LeftBottom,
            _ => OrientationType.Undefined,
        };
    }


    /// <summary>
    /// Converts the enum to an <see cref="SKSamplingOptions"/> struct.
    /// </summary>
    public static SKSamplingOptions ToSamplingOptions(this ImageInterpolation interpolation, int maxAniso = 16)
    {
        return interpolation switch
        {
            ImageInterpolation.Nearest => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None),
            ImageInterpolation.NearestMipmapNearest => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Nearest),
            ImageInterpolation.NearestMipmapLinear => new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.Linear),

            ImageInterpolation.Linear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None),
            ImageInterpolation.LinearMipmapNearest => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Nearest),
            ImageInterpolation.LinearMipmapLinear => new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),

            ImageInterpolation.CubicMitchell => new SKSamplingOptions(SKCubicResampler.Mitchell),
            ImageInterpolation.CubicCatmullRom => new SKSamplingOptions(SKCubicResampler.CatmullRom),

            ImageInterpolation.Anisotropic => new SKSamplingOptions(maxAniso),

            _ => SKSamplingOptions.Default,
        };
    }


    /// <summary>
    /// Attempts to apply the given color space to the source image and outputs a new image.
    /// </summary>
    public static bool TryApplyColorSpace(SKImage? imgSrc, SKColorSpace? destColorSpace, out SKImage? output)
    {
        output = null;
        if (imgSrc.IsDisposed()) return false;

        // convert color space
        if (destColorSpace is not null)
        {
            // Use SKSurface (not SKCanvas from SKBitmap) so that Skia's color
            // management pipeline performs the ICC gamut conversion during draw.
            var info = new SKImageInfo(imgSrc.Width, imgSrc.Height,
                imgSrc.ColorType, imgSrc.AlphaType, destColorSpace);
            using var surface = SKSurface.Create(info);
            if (surface is null) return false;

            surface.Canvas.DrawImage(imgSrc, 0, 0);
            output = surface.Snapshot();
            return true;
        }

        return false;
    }


    /// <summary>
    /// Shrinks the image so its longest side is <paramref name="maxSize"/>, entirely inside Skia.
    /// Unlike <see cref="ResizeAsync(SKImage?, double, ImageResamplingMethod, CancellationToken)"/>
    /// this skips the Magick round-trip (4 full pixel copies), which is far too costly for the
    /// thumbnail pipeline. Returns <c>null</c> when the image already fits (it never upscales).
    /// </summary>
    public static SKImage? ScaleDown(SKImage? imgSrc, double maxSize)
    {
        if (imgSrc.IsDisposed() || maxSize <= 0) return null;

        var newSize = BHelper.ResizeRatio(new Size(imgSrc.Width, imgSrc.Height), maxSize);
        var newWidth = Math.Max(1, (int)newSize.Width);
        var newHeight = Math.Max(1, (int)newSize.Height);
        if (newWidth >= imgSrc.Width && newHeight >= imgSrc.Height) return null;

        // no color space: matches the Magick path, which also read raw pixels into an unmanaged info
        var info = new SKImageInfo(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var bmpDest = new SKBitmap(info);
        using var pixmap = bmpDest.PeekPixels();
        if (pixmap is null) return null;

        // Mitchell keeps downscaled thumbnails smooth without ringing
        var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
        if (!imgSrc.ScalePixels(pixmap, sampling)) return null;

        return ToSKImage(bmpDest);
    }


    /// <summary>
    /// Resizes the specified bitmap to the given dimensions using the selected resampling method.
    /// </summary>
    public static async Task<SKBitmap?> ResizeAsync(SKImage? imgSrc,
        double size, ImageResamplingMethod resample = ImageResamplingMethod.Auto,
        CancellationToken token = default)
    {
        if (imgSrc.IsDisposed()) return null;

        var oldSize = new Size(imgSrc.Width, imgSrc.Height);
        var newSize = BHelper.ResizeRatio(oldSize, size);

        using var bmpSrc = SKBitmap.FromImage(imgSrc);
        return await ResizeAsync(bmpSrc, (int)newSize.Width, (int)newSize.Height, resample, token);
    }


    /// <summary>
    /// Resizes the specified bitmap to the given dimensions using the selected resampling method.
    /// </summary>
    public static async Task<SKBitmap?> ResizeAsync(SKBitmap? bmpSrc,
        int width, int height, ImageResamplingMethod resample = ImageResamplingMethod.Auto,
        CancellationToken token = default)
    {
        if (bmpSrc.IsDisposed()) return null;


        try
        {
            if (token.IsCancellationRequested) return null;

            // the enum mirrors FilterType (Auto = Undefined), so it casts directly
            var filter = (FilterType)resample;

            // resize with Magick.NET on a background thread
            return await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                using var imgM = ToMagick(bmpSrc);
                if (imgM is null) return null;

                if (filter != FilterType.Undefined) imgM.FilterType = filter;

                // resize to the exact target size (the caller already computed it)
                imgM.Resize(new MagickGeometry((uint)width, (uint)height) { IgnoreAspectRatio = true });
                token.ThrowIfCancellationRequested();

                using var resizedImg = FromMagick(imgM);
                return resizedImg is null ? null : SKBitmap.FromImage(resizedImg);
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch { }

        return null;
    }


    /// <summary>
    /// Creates an <see cref="SKColorSpace"/> from ICC data; <see langword="null"/> if Skia cannot parse it.
    /// </summary>
    public static SKColorSpace? CreateIccColorSpace(SKData? iccData)
    {
        if (iccData is null || iccData.IsEmpty) return null;

        try
        {
            var profile = SKColorSpaceIccProfile.Create(iccData);
            if (profile is null) return null;

            var colorSpace = SKColorSpace.CreateIcc(profile);
            if (colorSpace is null) profile.Dispose();

            return colorSpace;
        }
        catch { return null; }
    }


    /// <summary>
    /// Creates an <see cref="SKColorSpace"/> from raw ICC bytes.
    /// </summary>
    public static SKColorSpace? CreateIccColorSpace(ReadOnlySpan<byte> iccData)
    {
        if (iccData.IsEmpty) return null;

        var data = SKData.CreateCopy(iccData);
        var colorSpace = CreateIccColorSpace(data);
        if (colorSpace is null) data.Dispose();

        return colorSpace;
    }


    /// <summary>
    /// Gets Skia color profile.
    /// </summary>
    /// <param name="name">Name or Full path of color profile</param>
    public static (SKColorSpace? ColorSpace, bool IsSupported) GetColorProfileByName(string name)
    {
        (SKColorSpace? ColorSpace, bool IsSupported) results = new(null, true);


        // 1. don't use color profile
        if (name.Equals(nameof(ColorProfileOption.None))) return results;


        // 2. use current monitor profile
        if (name.Equals(nameof(ColorProfileOption.CurrentMonitorProfile)))
        {
            if (string.IsNullOrEmpty(Core.ColorProfileProvider?.ProfilePath))
                return results;

            using var data = SKData.Create(Core.ColorProfileProvider.ProfilePath);
            results.ColorSpace = CreateIccColorSpace(data);
            results.IsSupported = results.ColorSpace is not null; // Skia does not support all profiles

            return results;
        }


        // 3. use built-in color profile
        var magickProfile = MagickCodec.GetBuiltinColorProfile(name);
        if (magickProfile is not null)
        {
            results.ColorSpace = CreateIccColorSpace(magickProfile.ToReadOnlySpan());
            results.IsSupported = results.ColorSpace is not null;

            return results;
        }


        // 4. use custom color profile
        if (Path.Exists(name))
        {
            using var data = SKData.Create(name);
            results.ColorSpace = CreateIccColorSpace(data);
            results.IsSupported = results.ColorSpace is not null;

            return results;
        }


        return results;
    }


    /// <summary>
    /// Converts Magick image to SKImage.
    /// When <paramref name="isHdr"/> is <c>true</c>, exports at full float precision
    /// (<see cref="SKColorType.RgbaF32"/>) to preserve super-white HDR values from Q16-HDRI.
    /// </summary>
    public static unsafe SKImage? FromMagick(MagickImage? imgM, SKColorSpace? srcColorSpace = null, bool isHdr = false)
        => FromMagick(imgM, srcColorSpace, isHdr, out _);


    /// <summary>
    /// Converts Magick image to SKImage and reports any reduction in <paramref name="decodeScale"/>.
    /// An image past the pixel ceiling is resized down IN PLACE first, because no
    /// <see cref="SKImage"/> can exceed it regardless of how the pixels are supplied.
    /// </summary>
    public static unsafe SKImage? FromMagick(MagickImage? imgM, SKColorSpace? srcColorSpace,
        bool isHdr, out double decodeScale)
    {
        decodeScale = 1;
        if (imgM is null) return null;

        // prepare image info
        var alphaType = imgM.HasAlpha ? SKAlphaType.Unpremul : SKAlphaType.Opaque;
        var colorType = isHdr ? SKColorType.RgbaF32 : SKColorType.Rgba8888;

        // shrink to fit before converting; callers hand over images they are about to discard
        var srcWidth = imgM.Width;
        var bpp = new SKImageInfo(1, 1, colorType).BytesPerPixel;
        var fitScale = GetPixelBufferFitScale(srcWidth, imgM.Height, bpp);
        if (fitScale < 1)
        {
            var fitW = Math.Max(1u, (uint)(srcWidth * fitScale));
            var fitH = Math.Max(1u, (uint)(imgM.Height * fitScale));
            imgM.Resize(fitW, fitH);
            decodeScale = (double)imgM.Width / srcWidth;
        }

        var info = new SKImageInfo((int)imgM.Width, (int)imgM.Height, colorType, alphaType);
        if (srcColorSpace is not null)
        {
            info = info.WithColorSpace(srcColorSpace);
        }

        using var pixels = imgM.GetPixelsUnsafe();
        NativeMemoryArray<byte> nativeBuffer;

        if (isHdr)
        {
            var hdrBuffer = MagickCodec.ExportHdrPixels(imgM, pixels);
            if (hdrBuffer is null) return null;

            nativeBuffer = hdrBuffer;
        }
        else
        {
            // SDR path: 8-bit RGBA to save memory
            var bytes = pixels.ToByteArray(PixelMapping.RGBA);
            if (bytes is null) return null;

            nativeBuffer = new NativeMemoryArray<byte>(bytes.Length, skipZeroClear: true, addMemoryPressure: true);
            bytes.CopyTo(nativeBuffer.AsSpan());
        }

        // create SKData backed by native memory; release callback disposes nativeBuffer
        fixed (byte* ptr = nativeBuffer)
        {
            var data = SKData.Create(
                (nint)ptr,
                (int)nativeBuffer.Length,
                (addr, ctx) => ((NativeMemoryArray<byte>)ctx!).Dispose(),
                nativeBuffer
            );

            return SKImage.FromPixels(info, data);
        }
    }


    /// <summary>
    /// Wraps a raw native pixel buffer (typically returned by an external/native codec
    /// plugin in <see cref="IGPixelBuffer"/>) in an <see cref="SKImage"/>.
    /// </summary>
    /// <remarks>
    /// The bytes are copied into a host-owned <see cref="NativeMemoryArray{T}"/> so the
    /// caller may free the original buffer immediately after this returns. The resulting
    /// SKImage retains <paramref name="srcColorSpace"/> via its <see cref="SKImageInfo"/>;
    /// the caller should NOT dispose <paramref name="srcColorSpace"/> right after this
    /// call - SkiaSharp's managed wrapper participates in the underlying refcount and an
    /// early dispose has been observed to surface the image as "untagged" downstream.
    /// </remarks>
    public static unsafe SKImage? FromPixelBuffer(byte* data, int width, int height, int stride,
        SKColorType colorType, SKAlphaType alphaType, SKColorSpace? srcColorSpace)
    {
        if (data == null || width <= 0 || height <= 0 || stride <= 0) return null;
        if (colorType == SKColorType.Unknown) return null;

        var info = new SKImageInfo(width, height, colorType, alphaType);
        if (srcColorSpace is not null)
        {
            info = info.WithColorSpace(srcColorSpace);
        }

        // Allocate a host-owned native buffer and copy the source rows. Using a tight
        // (rowBytes == width * bytesPerPixel) layout simplifies SKImage.FromPixels and
        // avoids row-padding mismatches when the host re-encodes the image later.
        var rowBytes = info.RowBytes;
        var totalBytes = checked((long)rowBytes * height);
        var nativeBuffer = new NativeMemoryArray<byte>(totalBytes, skipZeroClear: true, addMemoryPressure: true);

        var srcSpan = new ReadOnlySpan<byte>(data, stride * (height - 1) + rowBytes);
        var dstSpan = nativeBuffer.AsSpan();
        if (stride == rowBytes)
        {
            // tight pack -> single bulk copy
            srcSpan.Slice(0, (int)totalBytes).CopyTo(dstSpan);
        }
        else
        {
            // row-by-row to strip the source stride padding
            for (var y = 0; y < height; y++)
            {
                var src = new ReadOnlySpan<byte>(data + (long)y * stride, rowBytes);
                src.CopyTo(dstSpan.Slice(y * rowBytes, rowBytes));
            }
        }
        fixed (byte* ptr = nativeBuffer)
        {
            var skData = SKData.Create(
                (nint)ptr,
                (int)nativeBuffer.Length,
                (addr, ctx) => ((NativeMemoryArray<byte>)ctx!).Dispose(),
                nativeBuffer
            );

            return SKImage.FromPixels(info, skData);
        }
    }


    /// <summary>
    /// Converts <see cref="Bitmap"/> to <see cref="SKBitmap"/>.
    /// </summary>
    public static SKBitmap? FromBitmap(Bitmap? abmp)
    {
        if (abmp is null) return null;

        var width = abmp.PixelSize.Width;
        var height = abmp.PixelSize.Height;

        // 1. Detect pixel format and alpha from the Avalonia bitmap
        var srcFormat = abmp.Format;
        var srcAlpha = abmp.AlphaFormat;
        var (colorType, alphaType) = MapAvaloniaFormat(srcFormat, srcAlpha);

        // 2. If the format maps directly to a Skia color type, do a fast direct copy
        if (colorType != SKColorType.Unknown)
        {
            var info = new SKImageInfo(width, height, colorType, alphaType);
            var skBmp = new SKBitmap(info);

            try
            {
                abmp.CopyPixels(
                    new PixelRect(0, 0, width, height),
                    skBmp.GetPixels(),
                    info.RowBytes * height,
                    info.RowBytes);
            }
            catch
            {
                skBmp.Dispose();
                return null;
            }

            return skBmp;
        }

        // 3. Fallback: use a WriteableBitmap in Bgra8888 as an intermediate
        //    to let Avalonia transcode unsupported formats (Rgb24, Bgr24, BlackWhite, etc.)
        var dstInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var dstBmp = new SKBitmap(dstInfo);

        try
        {
            using var wbmp = new WriteableBitmap(
                new PixelSize(width, height),
                abmp.Dpi,
                PixelFormat.Bgra8888,
                AlphaFormat.Premul);

            using (var fb = wbmp.Lock())
            {
                abmp.CopyPixels(fb);
            }

            wbmp.CopyPixels(
                new PixelRect(0, 0, width, height),
                dstBmp.GetPixels(),
                dstInfo.RowBytes * height,
                dstInfo.RowBytes);
        }
        catch
        {
            dstBmp.Dispose();
            return null;
        }

        return dstBmp;
    }


    /// <summary>
    /// Maps Avalonia pixel format and alpha to the corresponding SkiaSharp types.
    /// Returns <see cref="SKColorType.Unknown"/> if no direct mapping exists.
    /// </summary>
    private static (SKColorType ColorType, SKAlphaType AlphaType) MapAvaloniaFormat(
        PixelFormat? format, AlphaFormat? alpha)
    {
        var alphaType = alpha switch
        {
            AlphaFormat.Premul => SKAlphaType.Premul,
            AlphaFormat.Unpremul => SKAlphaType.Unpremul,
            AlphaFormat.Opaque => SKAlphaType.Opaque,
            _ => SKAlphaType.Premul,
        };

        if (format is null) return (SKColorType.Unknown, alphaType);

        var colorType = format.Value switch
        {
            var f when f == PixelFormat.Bgra8888 => SKColorType.Bgra8888,
            var f when f == PixelFormat.Rgba8888 => SKColorType.Rgba8888,
            var f when f == PixelFormat.Rgb565 => SKColorType.Rgb565,
            _ => SKColorType.Unknown,
        };

        return (colorType, alphaType);
    }


    /// <summary>
    /// Converts bitmap to image with optional source color space.
    /// </summary>
    public static SKImage? ToSKImage(SKBitmap? bmp)
    {
        if (bmp.IsDisposed()) return null;

        var img = SKImage.FromBitmap(bmp);
        return img;
    }


    /// <summary>
    /// Converts a finished bitmap to an image without copying its pixels; the image keeps
    /// the buffer alive. Only call once nothing will draw into <paramref name="bmp"/> again.
    /// </summary>
    public static SKImage? ToSKImageNoCopy(SKBitmap? bmp)
    {
        if (bmp.IsDisposed()) return null;

        bmp.SetImmutable();
        return SKImage.FromBitmap(bmp);
    }


    /// <summary>
    /// Converts <see cref="SKBitmap"/> to <see cref="MagickImage"/>.
    /// </summary>
    public static MagickImage? ToMagick(SKBitmap? skBmp)
    {
        using var skImg = ToSKImage(skBmp);
        return ToMagick(skImg);
    }


    /// <summary>
    /// Converts <see cref="SKImage"/> to <see cref="MagickImage"/>.
    /// Images are read as tightly packed Bgra8888 pixels so Skia handles
    /// color type conversion before Magick imports the pixel buffer.
    /// </summary>
    public static unsafe MagickImage? ToMagick(SKImage? skImg)
    {
        if (skImg.IsDisposed()) return null;

        var info = new SKImageInfo(skImg.Width, skImg.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var rowBytes = info.Width * info.BytesPerPixel;
        var pixelBuffer = new byte[rowBytes * info.Height];

        fixed (byte* ptr = pixelBuffer)
        {
            if (!skImg.ReadPixels(info, (nint)ptr, rowBytes, 0, 0)) return null;
        }

        // convert to MagickImage
        var imgM = new MagickImage();
        var settings = new PixelReadSettings((uint)info.Width, (uint)info.Height,
            StorageType.Char, PixelMapping.BGRA);
        imgM.ReadPixels(pixelBuffer, settings);

        return imgM;
    }


    /// <summary>
    /// Converts <see cref="SKImage"/> to <see cref="WriteableBitmap"/>.
    /// </summary>
    public static WriteableBitmap? ToWritableBitmap(SKImage? img)
    {
        if (img.IsDisposed()) return null;

        // PeekPixels returns null if the image is on the GPU (Texture-backed)
        using var pixmap = img.PeekPixels();
        return ToWritableBitmap(pixmap);
    }


    /// <summary>
    /// Converts <see cref="SKBitmap"/> to <see cref="WriteableBitmap"/>.
    /// </summary>
    public static WriteableBitmap? ToWritableBitmap(SKBitmap? bmp)
    {
        if (bmp.IsDisposed()) return null;

        using var pixmap = bmp.PeekPixels();
        return ToWritableBitmap(pixmap);
    }


    /// <summary>
    /// Converts <see cref="SKPixmap"/> to <see cref="WriteableBitmap"/>.
    /// </summary>
    public static unsafe WriteableBitmap? ToWritableBitmap(SKPixmap? pixmap)
    {
        if (pixmap.IsDisposed()) return null;

        var info = pixmap.Info;
        var wbmp = new WriteableBitmap(
            new PixelSize(info.Width, info.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using (ILockedFramebuffer fb = wbmp.Lock())
        {
            // If the source is already Bgra8888, we can do a super-fast memcpy.
            // We also check matching AlphaType to ensure we don't copy Premul into Unpremul blindly.
            bool isFastPath = info.ColorType == SKColorType.Bgra8888 &&
                              (info.AlphaType == SKAlphaType.Premul || info.AlphaType == SKAlphaType.Opaque);

            if (isFastPath)
            {
                byte* srcPtr = (byte*)pixmap.GetPixels();
                byte* dstPtr = (byte*)fb.Address;
                long srcRowBytes = pixmap.RowBytes;
                long dstRowBytes = fb.RowBytes;
                long bytesToCopy = info.Width * 4;

                if (srcRowBytes == dstRowBytes)
                {
                    // Single block copy if strides match (most common)
                    Unsafe.CopyBlock(dstPtr, srcPtr, (uint)(srcRowBytes * info.Height));
                }
                else
                {
                    // Row-by-row copy if strides differ
                    for (int y = 0; y < info.Height; y++)
                    {
                        Unsafe.CopyBlock(dstPtr, srcPtr, (uint)bytesToCopy);
                        srcPtr += srcRowBytes;
                        dstPtr += dstRowBytes;
                    }
                }
            }
            else
            {
                // Slow path: Source is not Bgra8888 (e.g., Rgba8888, Gray8).
                // We let Skia convert the pixels into the destination buffer.
                var dstInfo = new SKImageInfo(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                pixmap.ReadPixels(dstInfo, fb.Address, fb.RowBytes);
            }
        }

        return wbmp;
    }


    /// <summary>
    /// Converts SKImage to memory stream using uncompressed 32-bit BMPv4 format for transparency support.
    /// </summary>
    public static MemoryStream? ToBitmapV4Stream(SKImage? imgSrc)
    {
        if (imgSrc.IsDisposed()) return null;

        var bmpSrc = SKBitmap.FromImage(imgSrc);
        return ToBitmapV4Stream(bmpSrc);
    }


    /// <summary>
    /// Converts SKBitmap to memory stream using uncompressed 32-bit BMPv4 format for transparency support.
    /// </summary>
    public static MemoryStream? ToBitmapV4Stream(SKBitmap? bmpSrc)
    {
        if (bmpSrc.IsDisposed()) return null;


        SKBitmap? converted = null;
        try
        {
            var bmp = bmpSrc;

            // BMP uses BGRA byte order; convert if the source uses a different color type
            if (bmp.ColorType != SKColorType.Bgra8888)
            {
                var info = new SKImageInfo(bmp.Width, bmp.Height, SKColorType.Bgra8888, bmp.AlphaType);
                converted = new SKBitmap(info);
                if (!bmpSrc.CopyTo(converted, SKColorType.Bgra8888)) return null;
                bmp = converted;
            }

            var w = bmp.Width;
            var h = bmp.Height;
            var bmpRowBytes = w * 4;

            // header = 14 (file header) + 108 (BITMAPV4HEADER) = 122 bytes
            const int headerSize = 122;
            long pixelBytesL = (long)bmpRowBytes * h;

            // guard against int overflow for extremely large images
            if (pixelBytesL > int.MaxValue - headerSize) return null;

            var pixelBytes = (int)pixelBytesL;
            var fileSize = headerSize + pixelBytes;
            var ms = new MemoryStream(fileSize);

            // BMP File Header (14 bytes) + BITMAPV4HEADER (108 bytes, full alpha + color space support)
            using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                // -- BMP File Header (14 bytes) --
                bw.Write((byte)'B');
                bw.Write((byte)'M');
                bw.Write(fileSize);       // total file size in bytes
                bw.Write(0);              // reserved
                bw.Write(headerSize);     // pixel data offset

                // -- BITMAPV4HEADER (108 bytes) --
                bw.Write(108);            // biSize: header size identifies this as V4
                bw.Write(w);              // width in pixels
                bw.Write(-h);             // negative height = top-down row order
                bw.Write((short)1);       // color planes (always 1)
                bw.Write((short)32);      // bits per pixel
                bw.Write(3);              // biCompression: BI_BITFIELDS (required for channel masks)
                bw.Write(pixelBytes);     // raw pixel data size
                bw.Write(0);              // X pixels per meter (ignored)
                bw.Write(0);              // Y pixels per meter (ignored)
                bw.Write(0);              // palette colors used (0 = none for 32bpp)
                bw.Write(0);              // important colors (0 = all)

                // BGRA channel masks (BI_BITFIELDS layout, BGRA byte order)
                bw.Write(0x00FF0000);                     // red mask
                bw.Write(0x0000FF00);                     // green mask
                bw.Write(0x000000FF);                     // blue mask
                bw.Write(unchecked((int)0xFF000000));     // alpha mask

                // V4 color space block (52 bytes)
                bw.Write(0x73524742);                     // bV4CSType = 'sRGB' signals alpha-aware readers
                for (int i = 0; i < 12; i++) bw.Write(0); // CIEXYZTRIPLE endpoints (36 bytes, zeroed = default)
                                                          // bV4GammaRed/Green/Blue (12 bytes, zeroed = default)
            }

            // write raw pixel data (zero-copy fast path if row stride matches)
            var pixels = bmp.GetPixelSpan();
            var srcRowBytes = bmp.RowBytes;

            if (srcRowBytes == bmpRowBytes)
            {
                ms.Write(pixels);
            }
            else
            {
                for (int y = 0; y < h; y++)
                {
                    ms.Write(pixels.Slice(y * srcRowBytes, bmpRowBytes));
                }
            }

            ms.Position = 0;
            return ms;
        }
        finally
        {
            converted?.Dispose();
        }
    }
}
