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
using Avalonia.Media;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    /// <summary>
    /// Gets, sets whether HDR tone mapping is rendered. The owner sets this from the HDR setting.
    /// </summary>
    public bool EnableHdrRendering
    {
        get => GetValue(EnableHdrRenderingProperty);
        set => SetValue(EnableHdrRenderingProperty, value);
    }
    public static readonly StyledProperty<bool> EnableHdrRenderingProperty =
        AvaloniaProperty.Register<ViewerControl, bool>(nameof(EnableHdrRendering));


    #region Control Methods

    /// <summary>
    /// Gets a rendered bitmap of the current image or the selected region.
    /// </summary>
    public SKBitmap? GetRenderedBitmap(bool selectionOnly = false)
    {
        SKImageRef.ImageLease? imgLease = null;
        Rect selectionRect;

        try
        {
            lock (_lock)
            {
                var imageRef = _imgRender ?? _imgSource;
                if (imageRef is null) return null;

                // Acquire a lease to keep the image alive while we copy pixels.
                imgLease = imageRef.Acquire();
                var leaseImage = imgLease?.Image;
                if (leaseImage is null || leaseImage.IsDisposed()) return null;
                if (selectionOnly && SourceSelection.IsEmpty) return null;

                // Determine the source rectangle to copy (in source image coords).
                selectionRect = selectionOnly
                    ? SourceSelection.Normalize()
                    : new Rect(0, 0, leaseImage.Width, leaseImage.Height);
            }

            // Validate the leased image again after exiting the lock.
            var img = imgLease?.Image;
            if (img is null || img.IsDisposed()) return null;

            // Intersect selection with actual image bounds to avoid out-of-range
            // reads and to handle partially out-of-bounds selections.
            var bounds = new Rect(0, 0, img.Width, img.Height);
            selectionRect = selectionRect.GetIntersection(bounds);
            if (selectionRect.IsEmpty) return null;

            // prepare output bitmap
            var rect = selectionRect.ToSKRectI();
            var info = new SKImageInfo(rect.Width, rect.Height, img.ColorType, img.AlphaType, img.ColorSpace);
            var bmpOutput = new SKBitmap(info);

            // copy the image pixels to the output bitmap
            if (!img.ReadPixels(info, bmpOutput.GetPixels(), bmpOutput.RowBytes, rect.Left, rect.Top))
            {
                bmpOutput.Dispose();
                return null;
            }

            return bmpOutput;
        }
        finally
        {
            imgLease?.Dispose();
        }
    }


    /// <summary>
    /// Gets the color of the pixel at the specified coordinates from the image source.
    /// </summary>
    /// <returns>
    /// Empty color (<c>#0000</c>) if the photo source is none.
    /// </returns>
    public Color GetColorAt(int x, int y)
    {
        SKImageRef.ImageLease? imgLease = null;

        try
        {
            lock (_lock)
            {
                var imageRef = _imgRender ?? _imgSource;
                if (imageRef is null) return Const.COLOR_EMPTY;

                imgLease = imageRef.Acquire();
            }

            var img = imgLease?.Image;
            if (img.IsDisposed()) return Const.COLOR_EMPTY;

            if (x < 0 || x >= img.Width || y < 0 || y >= img.Height)
                return Const.COLOR_EMPTY;

            // read a single pixel using a 1x1 bitmap to avoid allocating a full copy
            var info = new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            using var pixel = new SKBitmap(info);
            if (!img.ReadPixels(info, pixel.GetPixels(), info.RowBytes, x, y))
                return Const.COLOR_EMPTY;

            var skColor = pixel.GetPixel(0, 0);
            return new Color(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
        }
        finally
        {
            imgLease?.Dispose();
        }
    }


    /// <summary>
    /// Applies HDR tone mapping and/or the destination Skia color profile to a decoded frame,
    /// off the UI thread. Both are full-image pixel passes: run inline they freeze the app for
    /// over a second on a large HDR photo. Returns <see langword="null"/> if nothing applied.
    /// </summary>
    /// <remarks>
    /// Must be called on the UI thread (it reads <see cref="EnableHdrRendering"/>). The caller
    /// owns <paramref name="srcImage"/> and must keep it alive for the whole pass
    /// (see <see cref="Photo.PinBitmap"/>).
    /// </remarks>
    private async Task<SKImage?> ApplySkiaColorSpaceAsync(SKImage? srcImage, PhotoMetadata? meta)
    {
        if (srcImage.IsDisposed()) return null;

        // snapshot UI-thread-affine state: the pass must not touch Photo or styled properties
        var isHdr = EnableHdrRendering && meta?.IsHdr == true;
        var transferFn = meta?.HdrTransferFn ?? HdrTransferFunction.None;
        var contentPeakNits = meta?.ContentPeakNits ?? 0d;

        // tuning the options is Pro-only
        var options = Core.IsProEnabled ? Core.HdrToneMappingConfig : new HdrToneMappingOptions();
        var destProfile = CanApplySkiaColorSpace() ? Core.DestColorProfile : null;

        // nothing to do: skip the thread hop entirely
        if (!isHdr && destProfile is null) return null;

        return await Task.Run(()
            => ApplySkiaColorSpace(srcImage, isHdr, transferFn, options, contentPeakNits, destProfile));
    }


    /// <summary>
    /// The color-management pixel work itself, with every input already snapshotted so it is
    /// safe to run on a background thread.
    /// </summary>
    private static SKImage? ApplySkiaColorSpace(SKImage srcImage, bool isHdr,
        HdrTransferFunction transferFn, HdrToneMappingOptions options, double contentPeakNits,
        SKColorSpace? destProfile)
    {
        // 1. HDR: tone-map to standard sRGB first, then the monitor profile below (same as SDR)
        if (isHdr)
        {
            var toneMapped = HdrToneMapper.ToneMapToSdr(srcImage, transferFn, options, contentPeakNits);

            if (!toneMapped.IsDisposed())
            {
                if (destProfile is not null
                    && SkiaCodec.TryApplyColorSpace(toneMapped, destProfile, out var profiled))
                {
                    toneMapped.Dispose();
                    return profiled;
                }

                return toneMapped;
            }
        }

        // 2. monitor color profile only
        if (destProfile is not null
            && SkiaCodec.TryApplyColorSpace(srcImage, destProfile, out var colored))
        {
            return colored;
        }

        return null;
    }


    /// <summary>
    /// Enables live HDR re-tone-mapping: the pre-tone-map HDR frame is retained in memory so
    /// slider changes re-apply instantly without a disk re-decode. Called when the HDR tool opens.
    /// Captures the current photo's raw frame once (via a single re-decode) if it wasn't retained.
    /// </summary>
    public void BeginLiveHdrToneMapping()
    {
        _liveHdrToneMapping.SetTrue();

        // capture the raw frame in the background WITHOUT touching the display, so opening the tool
        // never disturbs the current image (no reload -> no blank) and slider changes are instant
        _ = EnsureHdrSourceCapturedAsync();
    }


    /// <summary>
    /// Disables live HDR re-tone-mapping and releases the retained raw HDR frame.
    /// Called when the HDR tool closes.
    /// </summary>
    public void EndLiveHdrToneMapping()
    {
        _liveHdrToneMapping.SetFalse();
        lock (_lock)
        {
            SKImageRef.Set(ref _imgHdrSource, null);
        }
    }


    /// <summary>
    /// Whether the current photo can be tone-mapped, i.e. <see cref="ReapplyHdrToneMapping"/> has
    /// any effect on it. Mirrors the guards in <see cref="DoOneHdrToneMapPassAsync"/>.
    /// </summary>
    public bool CanReapplyHdrToneMapping()
    {
        if (!Core.IsProEnabled) return false;

        lock (_lock)
        {
            if (_animator is not null || IsVectorSource()) return false;
            if (Photo is not { State: PhotoState.Loaded }) return false;
            if (Photo.Metadata?.IsHdr != true) return false;

            // a gain-map base layer is already SDR, so tone mapping is a no-op
            return Photo.Metadata.HdrTransferFn != HdrTransferFunction.GainMap;
        }
    }


    /// <summary>
    /// Requests a live HDR re-tone-map with the latest <see cref="Core.HdrToneMappingConfig"/>,
    /// keeping zoom and pan. Coalesced and run on a background thread: rapid slider changes collapse
    /// to back-to-back passes over the retained raw HDR frame (no disk decode), always using the
    /// newest settings. No-op when tone mapping is disabled or the current photo is not HDR.
    /// </summary>
    public void ReapplyHdrToneMapping()
    {
        _hdrDirty = true;
        if (Interlocked.CompareExchange(ref _hdrActive, 1, 0) != 0) return;

        // the pump reads UI-thread-affine state, so it must start on the UI thread
        if (Dispatcher.UIThread.CheckAccess())
        {
            _ = HdrToneMapPumpAsync();
        }
        else
        {
            Dispatcher.UIThread.Post(() => _ = HdrToneMapPumpAsync());
        }
    }


    /// <summary>
    /// Serialized pump that drains re-tone-map requests one pass at a time (latest-wins),
    /// re-kicking if a request slips in during shutdown.
    /// </summary>
    /// <remarks>
    /// Runs on the UI thread and must stay there between passes: each pass reads viewer state that
    /// is UI-thread-affine. The expensive work inside a pass is already offloaded via
    /// <c>Task.Run</c>, so the loop itself costs nothing on the UI thread.
    /// </remarks>
    private async Task HdrToneMapPumpAsync()
    {
        try
        {
            // _hdrDirty is volatile: direct reads/writes are already ordered
            while (_hdrDirty)
            {
                _hdrDirty = false;
                await DoOneHdrToneMapPassAsync();
            }
        }
        catch (Exception ex)
        {
            // nothing awaits this pump, so an escaping fault would resurface on the finalizer
            // thread as an unobserved task exception and take the app down
            Debug.WriteLine($"❌❌❌ {nameof(HdrToneMapPumpAsync)}: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _hdrActive, 0);
        }

        // a request may have arrived after the last check but before we released the pump
        if (_hdrDirty && Interlocked.CompareExchange(ref _hdrActive, 1, 0) == 0)
        {
            _ = HdrToneMapPumpAsync();
        }
    }


    /// <summary>
    /// Runs one re-tone-map pass off the UI thread and swaps the result into <c>_imgSource</c>.
    /// If the raw frame isn't captured yet it captures it once (in the background) and retries.
    /// </summary>
    private async Task DoOneHdrToneMapPassAsync()
    {
        if (!Core.IsProEnabled) return;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            SKImageRef.ImageLease? lease;
            Photo? photoAtStart;
            HdrTransferFunction transferFn;
            double contentPeakNits;
            bool applyProfile;
            var destProfile = Core.DestColorProfile;

            lock (_lock)
            {
                if (_animator is not null || IsVectorSource()) return;
                if (Photo is not { State: PhotoState.Loaded }) return;
                // Mode drives tone-map vs pass-through (Mode=None => EnableHdrToneMapping is off and
                // ToneMapToSdr returns null => raw pass-through); only HDR photos are handled here
                if (Photo.Metadata?.IsHdr != true) return;

                lease = _imgHdrSource?.Acquire();
                photoAtStart = Photo;
                transferFn = Photo.Metadata.HdrTransferFn;
                contentPeakNits = Photo.Metadata.ContentPeakNits;
                applyProfile = CanApplySkiaColorSpace();
            }

            // raw frame not captured yet: capture it once (background, no display change), then retry
            if (lease is null)
            {
                if (attempt == 0)
                {
                    await EnsureHdrSourceCapturedAsync().ConfigureAwait(false);
                    continue;
                }
                return;
            }

            // heavy work off the UI thread; the lease keeps the raw frame alive across a photo change
            SKImage? result = null;
            var passthrough = false;
            try
            {
                (result, passthrough) = await Task.Run(() =>
                {
                    var toneMapped = HdrToneMapper.ToneMapToSdr(lease.Image, transferFn,
                        Core.HdrToneMappingConfig, contentPeakNits);

                    // None / gain-map => pass-through (show the raw frame, optionally monitor-profiled)
                    if (toneMapped.IsDisposed())
                    {
                        if (applyProfile && SkiaCodec.TryApplyColorSpace(lease.Image, destProfile, out var profiledRaw))
                            return (profiledRaw, false);
                        return ((SKImage?)null, true);
                    }

                    if (applyProfile && SkiaCodec.TryApplyColorSpace(toneMapped, destProfile, out var profiled))
                    {
                        toneMapped.Dispose();
                        return (profiled, false);
                    }
                    return (toneMapped, false);
                }).ConfigureAwait(false);
            }
            finally
            {
                lease.Dispose();
            }

            // swap in on the UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                lock (_lock)
                {
                    // drop a stale result if the photo changed or the retained frame is gone
                    if (!ReferenceEquals(Photo, photoAtStart)
                        || _imgHdrSource is null || _imgHdrSource.Image.IsDisposed())
                    {
                        result?.Dispose();
                        return;
                    }

                    _isFirstDraw.SetTrue();
                    if (passthrough)
                    {
                        // share the raw frame (no extra copy); ref-counting keeps both refs valid
                        SKImageRef.Set(ref _imgSource, _imgHdrSource.Image, _imgHdrSource);
                    }
                    else if (!result.IsDisposed())
                    {
                        SKImageRef.Set(ref _imgSource, result);
                    }
                    else
                    {
                        return;
                    }

                    _mipmapCache?.Dispose();
                    _mipmapCache = null;
                }

                Refresh(false);
            });

            return;
        }
    }


    /// <summary>
    /// Ensures the pre-tone-map HDR frame is available in <c>_imgHdrSource</c> for live
    /// re-tone-mapping. Decodes a fresh copy in the background and does NOT touch the current
    /// display (<c>_imgSource</c>), zoom, or pan — so opening the tool causes no reload/blank.
    /// No-op if already captured, the tool is closed, or the photo isn't a static HDR image.
    /// </summary>
    private async Task EnsureHdrSourceCapturedAsync()
    {
        Photo? photo;
        lock (_lock)
        {
            if (!_liveHdrToneMapping) return;
            if (_imgHdrSource?.Image.IsDisposed() == false) return; // already captured
            if (_animator is not null || IsVectorSource()) return;
            if (Photo is not { State: PhotoState.Loaded } || Photo.Metadata?.IsHdr != true) return;
            photo = Photo;
        }

        SKImage? raw = null;
        try
        {
            raw = await photo.DecodeStaticFrameAsync(0).ConfigureAwait(false);
        }
        catch
        {
            // decode failure: leave uncaptured; a later slider change retries
        }

        if (raw.IsDisposed()) return;

        lock (_lock)
        {
            // drop if the photo changed, the tool closed, or another capture already won
            if (!_liveHdrToneMapping || !ReferenceEquals(Photo, photo)
                || _imgHdrSource?.Image.IsDisposed() == false)
            {
                raw.Dispose();
                return;
            }

            SKImageRef.Set(ref _imgHdrSource, raw);
        }
    }


    /// <summary>
    /// Checks if Skia color space profile can be applied to the current photo.
    /// </summary>
    private bool CanApplySkiaColorSpace()
    {
        // 1. check if the destination profile is supported
        if (!Core.IsDestColorProfileSupported) return false;

        // 2. check user configs
        if (Core.Config.EnableAlwaysApplyColorProfile || Photo?.Metadata?.SkiaColorSpace is not null)
        {
            return true;
        }

        return false;
    }


    /// <summary>
    /// Inverts image colors.
    /// </summary>
    public bool InvertColor(bool requestRerender = true)
    {
        lock (_lock)
        {
            // do nothing for animated/vector images or when there is no source
            if (_animator is not null || IsVectorSource()) return false;

            var srcImage = (_imgRender ?? _imgSource)?.Image;
            var invertedImage = SkiaCodec.InvertImageColors(srcImage);
            if (invertedImage.IsDisposed()) return false;

            // update the render cache, keep _imgSource intact
            SKImageRef.Set(ref _imgRender, invertedImage);
            _mipmapCache?.Dispose();
            _mipmapCache = null;

            IsColorInverted = !IsColorInverted;
        }


        // render the transformation
        if (requestRerender)
        {
            Refresh(resetZoom: false);
        }

        return true;
    }


    /// <summary>
    /// Rotates the image.
    /// </summary>
    public bool RotateImage(double degree, bool requestRerender = true)
    {
        lock (_lock)
        {
            // do nothing for animated images or when there is no source
            if (_animator is not null || IsVectorSource()) return false;

            var srcImage = (_imgRender ?? _imgSource)?.Image;
            var rotatedImage = SkiaCodec.RotateImage(srcImage, degree);
            if (rotatedImage.IsDisposed()) return false;

            // update the render cache, keep _imgSource intact
            SKImageRef.Set(ref _imgRender, rotatedImage);
            _mipmapCache?.Dispose();
            _mipmapCache = null;

            // update source size
            BitmapSize = new(rotatedImage.Width, rotatedImage.Height);
        }

        // render the transformation
        if (requestRerender)
        {
            Refresh();
        }

        return true;
    }


    /// <summary>
    /// Flips the image.
    /// </summary>
    public bool FlipImage(FlipOptions options, bool requestRerender = true)
    {
        lock (_lock)
        {
            // do nothing for animated images or when there is no source
            if (_animator is not null || IsVectorSource()) return false;

            var srcImage = (_imgRender ?? _imgSource)?.Image;
            var flippedImage = SkiaCodec.FlipImage(srcImage, options);
            if (flippedImage.IsDisposed()) return false;

            // update the render cache, keep _imgSource intact
            SKImageRef.Set(ref _imgRender, flippedImage);
            _mipmapCache?.Dispose();
            _mipmapCache = null;
        }

        // render the transformation
        if (requestRerender)
        {
            Refresh(resetZoom: false);
        }

        return true;
    }


    /// <summary>
    /// Filters image color channels.
    /// </summary>
    public bool FilterColorChannels(ColorChannels colors, bool requestRerender = true)
    {
        lock (_lock)
        {
            // 1. do nothing for animated/vector images or when there is no source
            if (_animator is not null || IsVectorSource()) return false;

            var srcImage = _imgSource?.Image;
            if (srcImage.IsDisposed()) return false;


            // 2. reset render cache to start from original source
            SKImageRef.Set(ref _imgRender, null);
            _mipmapCache?.Dispose();
            _mipmapCache = null;
            _loadingOptions.Channels = colors;


            // 3. skip filtering when all channels (RGBA) are selected
            if (!colors.HasFlag(ColorChannels.RGBA))
            {
                var filteredImage = SkiaCodec.FilterImageColorChannels(srcImage, colors);
                if (filteredImage.IsDisposed()) return false;

                SKImageRef.Set(ref _imgRender, filteredImage);
            }
        }


        // 4. render the transformation
        if (requestRerender)
        {
            Refresh(false);
        }

        return true;
    }


    #endregion // Control Methods


}
