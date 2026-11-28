/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
    PrintCaption GetCaption(int index);

    /// <summary>
    /// Gets the pixel density of the photo for actual-size printing.
    /// </summary>
    float GetImageDpi(int index);

    /// <summary>
    /// Uses the picture of a vector print while it is held alive. Returns false for a raster print.
    /// </summary>
    bool UsePicture(int index, Action<SKPicture>? draw = null);

    /// <summary>
    /// Gets a ready image of a print at least <paramref name="longSide"/> pixels long, or <c>null</c> when none is ready yet; never waits.
    /// </summary>
    SKImageRef.ImageLease? TryGetImage(int index, int longSide);

    /// <summary>Gets a screen-sized preview and its original size in pixels for physical placement.</summary>
    SKImageRef.ImageLease? TryGetActualSizePreview(int index, int longSide, out SKSize originalSize);

    /// <summary>
    /// Gets an image of a print about <paramref name="longSide"/> pixels long, decoding it when needed.
    /// </summary>
    Task<SKImageRef.ImageLease?> GetImageAsync(int index, int longSide, CancellationToken token);
}


/// <summary>
/// One print of the session: a frame, and the part of it printed.
/// </summary>
public sealed record PrintItem(int FrameIndex, PrintRegion Region, PrintCaption Caption)
{
    internal PrintPhoto Source { get; init; } = null!;
}


/// <summary>
/// Captures metadata before navigation can unload the viewer's Photo. A running decode
/// keeps the pixels and reader alive when a photo is removed or its window is closed.
/// </summary>
internal sealed class PrintPhoto
{
    private int _references = 1;
    public ViewerImageState State { get; }
    public string FilePath { get; }
    public int FrameCount { get; }
    public float Dpi { get; }
    public float Aspect { get; }
    public Task<PhotoFrameReader?>? ReaderTask { get; set; }
    public SKSvg? Svg { get; }
    public Task<string?>? Snapshot { get; }
    public bool Removed { get; set; }
    public Dictionary<(int FrameIndex, PrintRegion Region), SKSize> OriginalSizes { get; } = [];


    public PrintPhoto(ViewerImageState state)
    {
        State = state;
        FilePath = state.Photo.FilePath;
        var metadata = state.Photo.Metadata;
        FrameCount = state.Photo.IsClipboard ? 1 : (int)Math.Max(1, metadata.FrameCount);
        Dpi = metadata.DpiX is >= 30 and <= 4800 ? (float)metadata.DpiX : 96;
        Aspect = metadata is { Width: > 0, Height: > 0 } ? metadata.Width / (float)metadata.Height : 1;
        if (state.Photo.IsClipboard) return;

        // Parse with the original base URI so SVG references keep their meaning after navigation.
        if (state.IsVector)
        {
            try { Svg = SvgCodec.LoadSvg(FilePath); }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        // Most ordinary photos can use the viewer's leased, full-resolution pixels directly.
        if (FrameCount > 1 || state.IsPreview || !state.HasExactColorTag || state.IsVector)
        {
            Snapshot = CaptureFileAsync(FilePath);
        }
    }


    // Open synchronously at selection, then copy off the UI thread. Every size and frame
    // is decoded from these same bytes, even if the original path is subsequently reused.
    private static async Task<string?> CaptureFileAsync(string path)
    {
        DirectoryInfo? directory = null;
        try
        {
            using var original = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            var stamp = File.GetLastWriteTimeUtc(original.SafeFileHandle);
            var length = original.Length;
            directory = Directory.CreateTempSubdirectory("ig-print-");
            var snapshot = Path.Combine(directory.FullName, "source" + Path.GetExtension(path));
            await Task.Run(() =>
            {
                using var copy = new FileStream(snapshot, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                original.CopyTo(copy);
                if (copy.Length != length || File.GetLastWriteTimeUtc(original.SafeFileHandle) != stamp)
                    throw new IOException("The source changed while it was being captured.");
            }).ConfigureAwait(false);
            return snapshot;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            if (directory is not null) DeleteSnapshot(directory.FullName);
            return null;
        }
    }


    private static void DeleteSnapshot(string directory)
    {
        try { Directory.Delete(directory, true); }
        catch (IOException ex) { Debug.WriteLine(ex.Message); }
        catch (UnauthorizedAccessException ex) { Debug.WriteLine(ex.Message); }
    }


    public PrintItem CreateItem(int frame, PrintRegion region)
    {
        var name = string.IsNullOrEmpty(FilePath) ? "ImageGlass" : Path.GetFileName(FilePath);
        var page = FrameCount > 1 ? $"({frame + 1}/{FrameCount})" : string.Empty;
        return new PrintItem(frame, region, new PrintCaption(name, page)) { Source = this };
    }


    // References are changed under the session lock.
    public void Retain() => _references++;

    public void Release()
    {
        if (--_references != 0) return;

        State.Dispose();
        Svg?.Dispose();
        _ = ReleaseReaderAsync();
    }


    private async Task ReleaseReaderAsync()
    {
        try
        {
            if (ReaderTask is { } reader) (await reader.ConfigureAwait(false))?.Dispose();
        }
        finally
        {
            if (Snapshot is { } pending && await pending.ConfigureAwait(false) is { } path)
                DeleteSnapshot(Path.GetDirectoryName(path)!);
        }
    }
}


/// <summary>
/// The caption under a print: the file name, which an ellipsis may shorten, and the page it shows, such as "(3/61)", which stays whole.
/// </summary>
public readonly record struct PrintCaption(string Name, string Page)
{
    public static PrintCaption Empty { get; } = new(string.Empty, string.Empty);

    public override string ToString() => string.IsNullOrEmpty(Page) ? Name : $"{Name} {Page}";
}


/// <summary>
/// The images of one Print window: the captured viewer state, the frames decoded apart from the viewer, and a bounded cache of sizes.
/// </summary>
public sealed class PrintSession : PhDisposable, IPrintImageSource
{
    // a bucket per power of two, so nearby sizes share one image
    private static readonly int[] _buckets = [256, 512, 1024, 2048, 4096, 8192, 16384];
    private const int FULL_SIZE = int.MaxValue;
    private const long CACHE_BUDGET_BYTES = 256L * 1024 * 1024;

    private readonly PrintPhoto _first;
    private readonly List<PrintPhoto> _added = [];
    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _cancel = new();
    private readonly SemaphoreSlim _decodeGate = new(2, 2);
    private readonly Dictionary<(PrintItem Item, int Bucket), SKImageRef> _cache = [];
    private readonly LinkedList<(PrintItem, int)> _lru = new();
    private readonly HashSet<(PrintItem, int)> _pending = [];
    private long _cacheBytes;

    private volatile IReadOnlyList<PrintItem> _items = [];


    /// <summary>
    /// Occurs on a worker thread when an image requested by <see cref="TryGetImage"/> is ready.
    /// </summary>
    public event Action? ImageReady;


    /// <summary>
    /// Gets the captured viewer state the session prints.
    /// </summary>
    public ViewerImageState State => _first.State;

    /// <summary>Gets the original file path captured before navigating the viewer.</summary>
    public string FilePath => _first.FilePath;

    /// <summary>Gets the number of photos added after the initial capture.</summary>
    public int AddedPhotoCount => _added.Count;


    /// <summary>
    /// Gets the number of frames or pages of the photo.
    /// </summary>
    public int FrameCount => _first.FrameCount;


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
        _first = new PrintPhoto(state);
        SetItems([state.FrameIndex], PrintRegion.WholeImage);
    }


    /// <summary>
    /// Sets the frames to print and the part of them printed; a region other than the whole image prints the current frame only.
    /// </summary>
    public void SetItems(IReadOnlyList<int> frameIndexes, PrintRegion region)
    {
        var frames = region == PrintRegion.WholeImage ? frameIndexes : [State.FrameIndex];
        _items = frames
            .Where(i => i >= 0 && i < FrameCount)
            .Select(i => _first.CreateItem(i, region))
            .Concat(_added.Select(photo => photo.CreateItem(photo.State.FrameIndex, PrintRegion.WholeImage)))
            .ToArray();
    }


    /// <summary>
    /// Adds the shown frame after the original photo's selected frames. Takes ownership
    /// of the capture even if the same photo is already in the session.
    /// </summary>
    public bool AddPhoto(ViewerImageState state)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var path = state.Photo.FilePath;
        bool Matches(PrintPhoto photo) => ReferenceEquals(photo.State.Photo, state.Photo)
            || (!string.IsNullOrEmpty(path) && path.Equals(
                string.IsNullOrEmpty(photo.State.Photo.FilePath) ? photo.FilePath : photo.State.Photo.FilePath, comparison));

        if (_cancel.IsCancellationRequested || Matches(_first) || _added.Any(Matches))
        {
            state.Dispose();
            return false;
        }

        var source = new PrintPhoto(state);
        _added.Add(source);
        _items = [.. _items, source.CreateItem(state.FrameIndex, PrintRegion.WholeImage)];
        return true;
    }


    /// <summary>
    /// Removes the extra photos and releases their cached images.
    /// </summary>
    public void ClearAddedPhotos()
    {
        _items = _items.Where(i => i.Source == _first).ToArray();
        lock (_lock)
        {
            foreach (var photo in _added)
            {
                photo.Removed = true;
                photo.Release();
            }
            _added.Clear();

            foreach (var key in _cache.Keys.Where(k => k.Item.Source.Removed).ToArray())
            {
                var image = _cache[key];
                _cache.Remove(key);
                _lru.Remove(key);
                _cacheBytes -= image.Image?.Info.BytesSize64 ?? 0;
                image.RequestDispose();
            }
        }
    }


    private PrintItem? GetItem(int index)
    {
        var items = _items;
        return index >= 0 && index < items.Count ? items[index] : null;
    }


    /// <summary>
    /// Gets the width over height of a print as far as it is known, for the page orientation.
    /// </summary>
    public float GetAspect(int index)
    {
        if (GetItem(index) is not { } item) return 1;

        lock (_lock)
        {
            if (item.Source.Removed) return 1;

            // the shown frame is the one whose size is known before any decode
            var state = item.Source.State;
            if (item.FrameIndex == state.FrameIndex && !state.Image.IsDisposed())
            {
                var rect = state.GetRegionRect(ToViewerRegion(item.Region));
                if (rect.Width > 0 && rect.Height > 0) return rect.Width / (float)rect.Height;
            }

            foreach (var (key, img) in _cache)
            {
                if (key.Item != item || img.Image.IsDisposed()) continue;
                return img.Image.Width / (float)img.Image.Height;
            }
        }

        return item.Source.Aspect;
    }


    /// <inheritdoc/>
    public PrintCaption GetCaption(int index) => GetItem(index)?.Caption ?? PrintCaption.Empty;


    /// <inheritdoc/>
    public float GetImageDpi(int index) => GetItem(index)?.Source.Dpi ?? 96;


    /// <inheritdoc/>
    public bool UsePicture(int index, Action<SKPicture>? draw = null)
    {
        if (GetItem(index) is not { } item || !item.Source.State.IsVector) return false;
        var source = item.Source;

        SKPicture picture;
        lock (_lock)
        {
            if (source.Removed) return false;
            if (source.Svg?.Picture is not { } svgPicture || svgPicture.IsDisposed()) return false;
            if (draw is null) return true;
            picture = svgPicture;
            source.Retain();
        }

        try
        {
            if (item.Region == PrintRegion.WholeImage) draw(picture);
            else
            {
                var state = source.State;
                var rect = state.GetRegionRect(ToViewerRegion(item.Region));
                if (rect.IsEmpty || state.Image is not { } shown) return false;
                var bounds = picture.CullRect;
                var sx = bounds.Width / shown.Width;
                var sy = bounds.Height / shown.Height;
                var crop = SKRect.Create(rect.Width * sx, rect.Height * sy);
                using var recorder = new SKPictureRecorder();
                var canvas = recorder.BeginRecording(crop);
                canvas.ClipRect(crop);
                canvas.Translate(-bounds.Left - rect.Left * sx, -bounds.Top - rect.Top * sy);
                canvas.DrawPicture(picture);
                using var clipped = recorder.EndRecording();
                draw(clipped);
            }
            return true;
        }
        finally
        {
            lock (_lock) source.Release();
        }
    }


    /// <inheritdoc/>
    public SKImageRef.ImageLease? TryGetImage(int index, int longSide)
    {
        if (GetItem(index) is not { } item || IsDisposed) return null;

        var bucket = GetBucket(longSide);

        lock (_lock)
        {
            if (item.Source.Removed) return null;

            // the requested size, else any larger one, else a smaller stand-in while the right one decodes
            var exact = Acquire((item, bucket));
            if (exact is not null) return exact;

            RequestLoad(item, bucket);

            foreach (var b in _buckets.Where(b => b > bucket).Append(FULL_SIZE).Concat(_buckets.Where(b => b < bucket).Reverse()))
            {
                var other = Acquire((item, b));
                if (other is not null) return other;
            }
        }

        return null;
    }


    /// <inheritdoc/>
    public SKImageRef.ImageLease? TryGetActualSizePreview(int index, int longSide, out SKSize originalSize)
    {
        originalSize = SKSize.Empty;
        if (GetItem(index) is not { } item) return null;

        lock (_lock)
        {
            if (item.Source.Removed) return null;
            if (CanUseShownImage(item))
            {
                var rect = item.Source.State.GetRegionRect(ToViewerRegion(item.Region));
                originalSize = new SKSize(rect.Width, rect.Height);
                return TryGetImage(index, longSide);
            }

            // This preview needs the dimensions of a full decode, even when a smaller
            // cached rendering exists. A separate bucket keeps it at screen resolution.
            var bucket = -GetBucket(longSide);
            if (item.Source.OriginalSizes.TryGetValue((item.FrameIndex, item.Region), out originalSize)
                && Acquire((item, bucket)) is { } ready) return ready;
            RequestLoad(item, bucket);
            return null;
        }
    }


    /// <inheritdoc/>
    public async Task<SKImageRef.ImageLease?> GetImageAsync(int index, int longSide, CancellationToken token)
    {
        if (GetItem(index) is not { } item || IsDisposed) return null;

        var bucket = GetBucket(longSide);
        var key = (item, bucket);

        lock (_lock)
        {
            var cached = Acquire(key);
            if (cached is not null) return cached;
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cancel.Token);
        return await LoadAsync(item, bucket, linked.Token).ConfigureAwait(false);
    }


    /// <summary>
    /// Builds the image of a frame at full resolution with every viewer edit, cropped to the printed part; the caller owns it.
    /// </summary>
    public async Task<SKImage?> BuildFullImageAsync(int frameIndex, PrintRegion region, CancellationToken token)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cancel.Token);
        return await BuildSourceImageAsync(_first.CreateItem(frameIndex, region), FULL_SIZE, linked.Token).ConfigureAwait(false);
    }


    /// <summary>
    /// Takes a lease on a cached image and marks it recently used; the caller holds <see cref="_lock"/>.
    /// </summary>
    private SKImageRef.ImageLease? Acquire((PrintItem, int) key)
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
        if (!_pending.Add((item, bucket))) return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var lease = await LoadAsync(item, bucket, _cancel.Token).ConfigureAwait(false);
                if (lease is not null && !_cancel.IsCancellationRequested) ImageReady?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {nameof(PrintSession)}.{nameof(RequestLoad)}: {ex.Message}");
            }
            finally
            {
                lock (_lock) _pending.Remove((item, bucket));
            }
        });
    }


    /// <summary>
    /// Decodes an image at a size bucket into the cache, unless it is already there.
    /// </summary>
    private async Task<SKImageRef.ImageLease?> LoadAsync(PrintItem item, int bucket, CancellationToken token)
    {
        var key = (item, bucket);

        await _decodeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            lock (_lock)
            {
                if (item.Source.Removed) return null;
                if (Acquire(key) is { } cached) return cached;
            }

            SKImage? image;
            if (bucket < 0)
            {
                using var full = await BuildSourceImageAsync(item, FULL_SIZE, token).ConfigureAwait(false);
                if (full is null) return null;
                var rect = SKRectI.Create(full.Width, full.Height);
                lock (_lock) item.Source.OriginalSizes[(item.FrameIndex, item.Region)] = new SKSize(rect.Width, rect.Height);
                image = Resample(full, rect, -bucket);
            }
            else image = await BuildSourceImageAsync(item, bucket, token).ConfigureAwait(false);

            if (image is null) return null;

            lock (_lock)
            {
                if (item.Source.Removed || _cache.ContainsKey(key))
                {
                    image.Dispose();
                    return Acquire(key);
                }

                _cache[key] = new SKImageRef(image);
                _lru.AddFirst(key);
                _cacheBytes += image.Info.BytesSize64;
                TrimCache(key);
                // Pin this result before another photo's decode can evict it.
                return _cache[key].Acquire();
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
    private void TrimCache((PrintItem, int) keep)
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

        var source = item.Source;
        lock (_lock)
        {
            if (source.Removed) return null;
            source.Retain();
        }

        try
        {
            // 1. the shown frame, already edited, when its pixels say what color space they are in
            if (CanUseShownImage(item))
            {
                return await Task.Run(() => ScaleShownImage(item, bucket), token).ConfigureAwait(false);
            }

            // 2. any other frame, decoded apart from the viewer with true colors
            var reader = await GetReaderAsync(item.Source, token).ConfigureAwait(false);
            if (reader is null) return null;

            var box = bucket == FULL_SIZE ? 0u : (uint)bucket;
            using var frame = await reader.ReadFrameAsync(item.FrameIndex, box, token).ConfigureAwait(false);
            if (frame.IsDisposed()) return null;

            return await Task.Run(() => EditDecodedFrame(frame, reader.Metadata, item, bucket), token).ConfigureAwait(false);
        }
        finally
        {
            lock (_lock) source.Release();
        }
    }


    /// <summary>
    /// Checks whether a print can use the captured image instead of a decode.
    /// </summary>
    private static bool CanUseShownImage(PrintItem item)
    {
        var state = item.Source.State;
        if (state.Image.IsDisposed() || state.IsVector) return false;
        if (string.IsNullOrEmpty(item.Source.FilePath)) return true;

        return item.FrameIndex == state.FrameIndex && !state.IsPreview && state.HasExactColorTag;
    }


    /// <summary>
    /// Crops the captured image to the printed part and scales it into sRGB within the bucket.
    /// </summary>
    private static SKImage? ScaleShownImage(PrintItem item, int bucket)
    {
        var state = item.Source.State;
        var shown = state.Image;
        if (shown.IsDisposed()) return null;

        var rect = state.GetRegionRect(ToViewerRegion(item.Region));
        if (rect.IsEmpty) rect = SKRectI.Create(shown.Width, shown.Height);

        return Resample(shown, rect, bucket);
    }


    /// <summary>
    /// Applies the captured tone mapping and edits to a decoded frame, crops it like the shown one, and scales it into sRGB.
    /// </summary>
    private static SKImage? EditDecodedFrame(SKImage frame, PhotoMetadata meta, PrintItem item, int bucket)
    {
        var state = item.Source.State;

        // HDR is shown tone-mapped, so it prints tone-mapped
        SKImage? toneMapped = null;
        if (state.HdrToneMapping is { } options && meta.IsHdr)
        {
            toneMapped = HdrToneMapper.ToneMapToSdr(frame, meta.HdrTransferFn, options, meta.ContentPeakNits);
        }

        try
        {
            var source = toneMapped ?? frame;
            using var edited = SkiaCodec.ApplyImageEdits(source, state.Channels, state.IsColorInverted, state.Orientation);
            var shownFrame = edited ?? source;

            // the region was drawn on the shown image, so scale it to this decode's size
            var rect = SKRectI.Create(shownFrame.Width, shownFrame.Height);
            if (item.Region != PrintRegion.WholeImage && state.Image is { } shown && !shown.IsDisposed())
            {
                var region = state.GetRegionRect(ToViewerRegion(item.Region));
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
    private Task<PhotoFrameReader?> GetReaderAsync(PrintPhoto source, CancellationToken token)
    {
        lock (_lock)
        {
            // A file restored after a failed preview can open on the next request.
            if (source.ReaderTask is null || source.ReaderTask.IsCompletedSuccessfully && source.ReaderTask.Result is null)
            {
                source.ReaderTask = OpenReaderAsync(source);
            }
            return source.ReaderTask.WaitAsync(token);
        }
    }


    private async Task<PhotoFrameReader?> OpenReaderAsync(PrintPhoto source)
    {
        if (source.Snapshot is not { } snapshot || await snapshot.ConfigureAwait(false) is not { } path) return null;

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

            foreach (var photo in _added.Prepend(_first))
            {
                photo.Removed = true;
                photo.Release();
            }
            _added.Clear();
            _items = [];
        }
    }
}
