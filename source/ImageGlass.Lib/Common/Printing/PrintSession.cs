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
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using SkiaSharp;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Printing;


/// <summary>
/// Supplies the image of each print to the renderer.
/// </summary>
public interface IPrintImageSource
{
    /// <summary>
    /// Gets the number of prints.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the caption of a print.
    /// </summary>
    string GetCaption(int index);

    /// <summary>
    /// Gets the picture of a vector print, drawn as vectors; <c>null</c> for a raster print.
    /// </summary>
    SKPicture? GetPicture(int index);

    /// <summary>
    /// Gets a ready image of a print at least <paramref name="longSide"/> pixels long, or <c>null</c> when none is ready yet; never waits.
    /// </summary>
    SKImageRef.ImageLease? TryGetImage(int index, int longSide);

    /// <summary>
    /// Gets an image of a print about <paramref name="longSide"/> pixels long, decoding it when needed.
    /// </summary>
    Task<SKImageRef.ImageLease?> GetImageAsync(int index, int longSide, CancellationToken token);
}


/// <summary>
/// One print of the session: a frame, and the part of it printed.
/// </summary>
public sealed record PrintItem(int FrameIndex, PrintRegion Region, string Caption);


/// <summary>
/// The images of one Print window: the captured viewer state, the frames decoded apart from the viewer, and a bounded cache of sizes.
/// </summary>
public sealed class PrintSession : PhDisposable, IPrintImageSource
{
    // a bucket per power of two, so nearby sizes share one image
    private static readonly int[] _buckets = [256, 512, 1024, 2048, 4096, 8192, 16384];
    private const int FULL_SIZE = int.MaxValue;
    private const long CACHE_BUDGET_BYTES = 256L * 1024 * 1024;

    private readonly ViewerImageState _state;
    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _cancel = new();
    private readonly SemaphoreSlim _decodeGate = new(2, 2);
    private readonly Dictionary<(int Frame, PrintRegion Region, int Bucket), SKImageRef> _cache = [];
    private readonly LinkedList<(int, PrintRegion, int)> _lru = new();
    private readonly HashSet<(int, PrintRegion, int)> _pending = [];
    private long _cacheBytes;

    private Task<PhotoFrameReader?>? _readerTask;
    private SKSvg? _svg;
    private IReadOnlyList<PrintItem> _items = [];


    /// <summary>
    /// Occurs on a worker thread when an image requested by <see cref="TryGetImage"/> is ready.
    /// </summary>
    public event Action? ImageReady;


    /// <summary>
    /// Gets the captured viewer state the session prints.
    /// </summary>
    public ViewerImageState State => _state;


    /// <summary>
    /// Gets the number of frames or pages of the photo.
    /// </summary>
    public int FrameCount { get; }


    /// <summary>
    /// Gets the prints, in order.
    /// </summary>
    public IReadOnlyList<PrintItem> Items => _items;


    /// <inheritdoc/>
    public int Count => _items.Count;


    /// <summary>
    /// Starts a session over a captured viewer state, which it then owns.
    /// </summary>
    public PrintSession(ViewerImageState state)
    {
        _state = state;

        var meta = state.Photo.Metadata;
        FrameCount = state.Photo.IsClipboard ? 1 : (int)Math.Max(1, meta?.FrameCount ?? 1);
        SetItems([state.FrameIndex], PrintRegion.WholeImage);
    }


    /// <summary>
    /// Sets the frames to print and the part of them printed; a region other than the whole image prints the current frame only.
    /// </summary>
    public void SetItems(IReadOnlyList<int> frameIndexes, PrintRegion region)
    {
        var frames = region == PrintRegion.WholeImage ? frameIndexes : [_state.FrameIndex];
        var name = GetPhotoName();

        _items = frames
            .Where(i => i >= 0 && i < FrameCount)
            .Select(i => new PrintItem(i, region, FrameCount > 1 ? $"{name} ({i + 1}/{FrameCount})" : name))
            .ToList();
    }


    /// <summary>
    /// Gets the width over height of a print as far as it is known, for the page orientation.
    /// </summary>
    public float GetAspect(int index)
    {
        if (index < 0 || index >= _items.Count) return 1;
        var item = _items[index];

        // the shown frame is the one whose size is known before any decode
        var shown = _state.Image;
        if (item.FrameIndex == _state.FrameIndex && !shown.IsDisposed())
        {
            var rect = _state.GetRegionRect(ToViewerRegion(item.Region));
            if (rect.Width > 0 && rect.Height > 0) return rect.Width / (float)rect.Height;
        }

        lock (_lock)
        {
            foreach (var (key, img) in _cache)
            {
                if (key.Frame != item.FrameIndex || key.Region != item.Region || img.Image.IsDisposed()) continue;
                return img.Image.Width / (float)img.Image.Height;
            }
        }

        var meta = _state.Photo.Metadata;
        return meta is { Width: > 0, Height: > 0 } ? meta.Width / (float)meta.Height : 1;
    }


    /// <inheritdoc/>
    public string GetCaption(int index) => index >= 0 && index < _items.Count ? _items[index].Caption : string.Empty;


    /// <inheritdoc/>
    public SKPicture? GetPicture(int index)
    {
        if (!_state.IsVector || index < 0 || index >= _items.Count) return null;

        lock (_lock)
        {
            if (_svg is null && !IsDisposed)
            {
                try
                {
                    _svg = SvgCodec.LoadSvg(_state.Photo.FilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌❌❌ {nameof(PrintSession)}.{nameof(GetPicture)}: {ex.Message}");
                }
            }

            return _svg?.Picture;
        }
    }


    /// <inheritdoc/>
    public SKImageRef.ImageLease? TryGetImage(int index, int longSide)
    {
        if (index < 0 || index >= _items.Count || IsDisposed) return null;

        var item = _items[index];
        var bucket = GetBucket(longSide);

        lock (_lock)
        {
            // the requested size, else any larger one, else a smaller stand-in while the right one decodes
            var exact = Acquire((item.FrameIndex, item.Region, bucket));
            if (exact is not null) return exact;

            RequestLoad(item, bucket);

            foreach (var b in _buckets.Where(b => b > bucket).Append(FULL_SIZE).Concat(_buckets.Where(b => b < bucket).Reverse()))
            {
                var other = Acquire((item.FrameIndex, item.Region, b));
                if (other is not null) return other;
            }
        }

        return null;
    }


    /// <inheritdoc/>
    public async Task<SKImageRef.ImageLease?> GetImageAsync(int index, int longSide, CancellationToken token)
    {
        if (index < 0 || index >= _items.Count || IsDisposed) return null;

        var item = _items[index];
        var bucket = GetBucket(longSide);
        var key = (item.FrameIndex, item.Region, bucket);

        lock (_lock)
        {
            var cached = Acquire(key);
            if (cached is not null) return cached;
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cancel.Token);
        await LoadAsync(item, bucket, linked.Token).ConfigureAwait(false);

        lock (_lock)
        {
            return Acquire(key);
        }
    }


    /// <summary>
    /// Builds the image of a frame at full resolution with every viewer edit, cropped to the printed part; the caller owns it.
    /// </summary>
    public async Task<SKImage?> BuildFullImageAsync(int frameIndex, PrintRegion region, CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cancel.Token);
        return await BuildSourceImageAsync(new PrintItem(frameIndex, region, string.Empty), FULL_SIZE, linked.Token).ConfigureAwait(false);
    }


    /// <summary>
    /// Takes a lease on a cached image and marks it recently used; the caller holds <see cref="_lock"/>.
    /// </summary>
    private SKImageRef.ImageLease? Acquire((int, PrintRegion, int) key)
    {
        if (!_cache.TryGetValue(key, out var imgRef)) return null;

        var lease = imgRef.Acquire();
        if (lease is null) return null;

        _lru.Remove(key);
        _lru.AddFirst(key);
        return lease;
    }


    /// <summary>
    /// Queues a background decode of an image a preview asked for; the caller holds <see cref="_lock"/>.
    /// </summary>
    private void RequestLoad(PrintItem item, int bucket)
    {
        if (!_pending.Add((item.FrameIndex, item.Region, bucket))) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await LoadAsync(item, bucket, _cancel.Token).ConfigureAwait(false);
                if (!IsDisposed) ImageReady?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {nameof(PrintSession)}.{nameof(RequestLoad)}: {ex.Message}");
            }
            finally
            {
                lock (_lock) _pending.Remove((item.FrameIndex, item.Region, bucket));
            }
        });
    }


    /// <summary>
    /// Decodes an image at a size bucket into the cache, unless it is already there.
    /// </summary>
    private async Task LoadAsync(PrintItem item, int bucket, CancellationToken token)
    {
        var key = (item.FrameIndex, item.Region, bucket);

        await _decodeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            lock (_lock)
            {
                if (_cache.ContainsKey(key)) return;
            }

            var image = await BuildSourceImageAsync(item, bucket, token).ConfigureAwait(false);
            if (image is null) return;

            lock (_lock)
            {
                if (IsDisposed || _cache.ContainsKey(key))
                {
                    image.Dispose();
                    return;
                }

                _cache[key] = new SKImageRef(image);
                _lru.AddFirst(key);
                _cacheBytes += image.Info.BytesSize64;
                TrimCache(key);
            }
        }
        finally
        {
            _decodeGate.Release();
        }
    }


    /// <summary>
    /// Evicts the least recently used images over the budget, never the one just added; the caller holds <see cref="_lock"/>.
    /// </summary>
    private void TrimCache((int, PrintRegion, int) keep)
    {
        while (_cacheBytes > CACHE_BUDGET_BYTES && _lru.Last is { } node && !node.Value.Equals(keep))
        {
            _lru.RemoveLast();
            if (!_cache.Remove(node.Value, out var imgRef)) continue;

            _cacheBytes -= imgRef.Image?.Info.BytesSize64 ?? 0;

            // a lease still drawing keeps the image alive until it is done
            imgRef.RequestDispose();
        }
    }


    /// <summary>
    /// Builds the sRGB image of a print no longer than the bucket: the captured pixels when they are exact, else a fresh decode.
    /// </summary>
    private async Task<SKImage?> BuildSourceImageAsync(PrintItem item, int bucket, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        // 1. the shown frame, already edited, when its pixels say what color space they are in
        if (CanUseShownImage(item))
        {
            return await Task.Run(() => ScaleShownImage(item, bucket), token).ConfigureAwait(false);
        }

        // 2. any other frame, decoded apart from the viewer with true colors
        var reader = await GetReaderAsync(token).ConfigureAwait(false);
        if (reader is null) return null;

        var box = bucket == FULL_SIZE ? 0u : (uint)bucket;
        using var frame = await reader.ReadFrameAsync(item.FrameIndex, box, token).ConfigureAwait(false);
        if (frame.IsDisposed()) return null;

        return await Task.Run(() => EditDecodedFrame(frame, reader.Metadata, item, bucket), token).ConfigureAwait(false);
    }


    /// <summary>
    /// Checks whether a print can use the captured image instead of a decode.
    /// </summary>
    private bool CanUseShownImage(PrintItem item)
    {
        if (_state.Image.IsDisposed() || _state.IsVector) return false;
        if (_state.Photo.IsClipboard) return true;

        return item.FrameIndex == _state.FrameIndex && !_state.IsPreview && _state.HasExactColorTag;
    }


    /// <summary>
    /// Crops the captured image to the printed part and scales it into sRGB within the bucket.
    /// </summary>
    private SKImage? ScaleShownImage(PrintItem item, int bucket)
    {
        var shown = _state.Image;
        if (shown.IsDisposed()) return null;

        var rect = _state.GetRegionRect(ToViewerRegion(item.Region));
        if (rect.IsEmpty) rect = SKRectI.Create(shown.Width, shown.Height);

        return Resample(shown, rect, bucket);
    }


    /// <summary>
    /// Applies the captured tone mapping and edits to a decoded frame, crops it like the shown one, and scales it into sRGB.
    /// </summary>
    private SKImage? EditDecodedFrame(SKImage frame, PhotoMetadata meta, PrintItem item, int bucket)
    {
        // HDR is shown tone-mapped, so it prints tone-mapped
        SKImage? toneMapped = null;
        if (_state.HdrToneMapping is { } options && meta.IsHdr)
        {
            toneMapped = HdrToneMapper.ToneMapToSdr(frame, meta.HdrTransferFn, options, meta.ContentPeakNits);
        }

        try
        {
            var source = toneMapped ?? frame;
            using var edited = SkiaCodec.ApplyImageEdits(source, _state.Channels, _state.IsColorInverted, _state.Orientation);
            var shownFrame = edited ?? source;

            // the region was drawn on the shown image, so scale it to this decode's size
            var rect = SKRectI.Create(shownFrame.Width, shownFrame.Height);
            if (item.Region != PrintRegion.WholeImage && _state.Image is { } shown && !shown.IsDisposed())
            {
                var region = _state.GetRegionRect(ToViewerRegion(item.Region));
                var sx = shownFrame.Width / (float)shown.Width;
                var sy = shownFrame.Height / (float)shown.Height;
                var scaled = SKRectI.Round(new SKRect(region.Left * sx, region.Top * sy, region.Right * sx, region.Bottom * sy));
                if (scaled.Width > 0 && scaled.Height > 0) rect = scaled;
            }

            return Resample(shownFrame, rect, bucket);
        }
        finally
        {
            toneMapped?.Dispose();
        }
    }


    /// <summary>
    /// Draws part of an image into a new sRGB image no longer than the bucket, which converts its color space on the way.
    /// </summary>
    private static SKImage? Resample(SKImage src, SKRectI rect, int bucket)
    {
        var longSide = Math.Max(rect.Width, rect.Height);
        var scale = bucket == FULL_SIZE || longSide <= bucket ? 1f : bucket / (float)longSide;
        var w = Math.Max(1, (int)Math.Round(rect.Width * scale));
        var h = Math.Max(1, (int)Math.Round(rect.Height * scale));

        // 8-bit sRGB, opaque on the white of the paper so a PDF may store it as JPEG; int.MaxValue bytes caps any surface
        var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque, SKColorSpace.CreateSrgb());
        if (info.BytesSize64 > int.MaxValue) return null;

        using var surface = SKSurface.Create(info);
        if (surface is null) return null;

        surface.Canvas.Clear(SKColors.White);
        var sampling = scale < 1 ? new SKSamplingOptions(SKCubicResampler.Mitchell) : new SKSamplingOptions(SKFilterMode.Linear);
        surface.Canvas.DrawImage(src, SKRect.Create(rect.Left, rect.Top, rect.Width, rect.Height), SKRect.Create(w, h), sampling);

        return surface.Snapshot();
    }


    /// <summary>
    /// Opens the frame reader once, on first need.
    /// </summary>
    private Task<PhotoFrameReader?> GetReaderAsync(CancellationToken token)
    {
        lock (_lock)
        {
            _readerTask ??= OpenReaderAsync();
            return _readerTask.WaitAsync(token);
        }
    }


    private async Task<PhotoFrameReader?> OpenReaderAsync()
    {
        var path = _state.Photo.FilePath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

        try
        {
            // the viewer's own read settings, such as a RAW preview, so a print matches what is shown
            var options = PhotoReadOptions.FromConfig() with { SkipDisplayProfile = true };
            return await PhotoFrameReader.OpenAsync(path, options, _cancel.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return null; }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintSession)}.{nameof(OpenReaderAsync)}: {ex.Message}");
            return null;
        }
    }


    private static int GetBucket(int longSide)
    {
        if (longSide <= 0) return _buckets[0];

        foreach (var b in _buckets)
        {
            if (longSide <= b) return b;
        }

        return FULL_SIZE;
    }


    private static ViewerImageRegion ToViewerRegion(PrintRegion region) => region switch
    {
        PrintRegion.Selection => ViewerImageRegion.Selection,
        PrintRegion.VisibleArea => ViewerImageRegion.VisibleArea,
        _ => ViewerImageRegion.WholeImage,
    };


    private string GetPhotoName()
    {
        var path = _state.Photo.FilePath;
        return string.IsNullOrEmpty(path) ? "ImageGlass" : Path.GetFileName(path);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();
        _cancel.Cancel();

        lock (_lock)
        {
            foreach (var imgRef in _cache.Values) imgRef.RequestDispose();
            _cache.Clear();
            _lru.Clear();
            _cacheBytes = 0;

            _svg?.Dispose();
            _svg = null;
        }

        // a reader still opening disposes itself once it lands
        _ = _readerTask?.ContinueWith(t => t.Result?.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion);
        _state.Dispose();
    }
}
