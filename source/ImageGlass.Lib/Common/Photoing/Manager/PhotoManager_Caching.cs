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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

public partial class PhotoManager
{
    private CancellationTokenSource? _cacheCts;
    private readonly Lock _cacheLock = new();

    /// <summary>
    /// Indexes of photos the caching logic decoded; the current photo is NOT among them.
    /// </summary>
    private readonly HashSet<int> _cachedIndexes = [];


    #region Debug properties

    /// <summary>
    /// Gets the number of photos currently held in cache.
    /// </summary>
    public int CachedCount
    {
        get
        {
            lock (_cacheLock) return _cachedIndexes.Count;
        }
    }

    /// <summary>
    /// Gets the estimated total cached memory in MB (includes the current photo).
    /// </summary>
    public double CachedMemoryMb
    {
        get
        {
            var bytes = EstimateCachedMemory(CurrentIndex);
            return Math.Round(bytes / (1024.0 * 1024.0), 1);
        }
    }

    /// <summary>
    /// Gets a snapshot of the currently cached indexes for debug display.
    /// </summary>
    public int[] CachedIndexSnapshot
    {
        get
        {
            lock (_cacheLock) return [.. _cachedIndexes];
        }
    }

    #endregion // Debug properties


    /// <summary>
    /// Starts a background cache pass around the given center index, cancelling any pass already running.
    /// </summary>
    public void RequestCacheAround(int centerIndex)
    {
        // skip caching during quick browsing (user is holding arrow keys)
        if (Core.API.IsQuickBrowsing) return;

        CancellationToken token;
        lock (_cacheLock)
        {
            _cacheCts?.Cancel();
            _cacheCts?.Dispose();
            _cacheCts = new CancellationTokenSource();
            token = _cacheCts.Token;
        }

        // a slideshow preloads its next image even when general caching is off (budget = 0)
        var isSlideshow = Core.Slideshow?.IsRunning == true;

        if (!isSlideshow
            && (Core.Config.CacheMaxMemoryInMb == 0
            || Core.Config.CacheMaxFileSizeInMb == 0
            || Core.Config.CacheMaxDimension == 0)) return;

        // run on a dedicated thread to avoid thread pool starvation
        _ = Task.Factory.StartNew(
            () => RunCacheAroundAsync(centerIndex, isSlideshow, token),
            token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }


    /// <summary>
    /// Cancels any in-progress caching operation.
    /// </summary>
    public void CancelCaching()
    {
        lock (_cacheLock)
        {
            _cacheCts?.Cancel();
            _cacheCts?.Dispose();
            _cacheCts = null;
        }
    }


    /// <summary>
    /// Unloads every decoded photo and resets tracking.
    /// </summary>
    /// <param name="excludePhoto">A photo to leave decoded, e.g. one carried over into a rebuilt list.</param>
    public void ClearCache(Photo? excludePhoto = null)
    {
        CancelCaching();

        lock (_cacheLock)
        {
            _cachedIndexes.Clear();
        }

        UnloadPhotosOutside([], excludePhoto);
    }


    /// <summary>
    /// Unloads every decoded photo whose index is not in <paramref name="keepIndexes"/>.
    /// </summary>
    /// <param name="keepIndexes">Indexes that must stay decoded.</param>
    /// <param name="excludePhoto">An extra photo to leave decoded, matched by reference.</param>
    /// <param name="keepCurrent">
    /// Re-reads <see cref="CurrentIndex"/> per item, so navigating mid-sweep cannot unload the viewer's photo.
    /// </param>
    /// <remarks>
    /// Walks the whole list: a pass cancelled mid-load leaves photos decoded but untracked, and nothing else frees them.
    /// </remarks>
    private void UnloadPhotosOutside(HashSet<int> keepIndexes, Photo? excludePhoto, bool keepCurrent = false)
    {
        Photo[] snapshot;
        lock (_lock)
        {
            snapshot = [.. Items];
        }

        for (var i = 0; i < snapshot.Length; i++)
        {
            if (keepIndexes.Contains(i)) continue;
            if (keepCurrent && i == CurrentIndex) continue;

            var photo = snapshot[i];
            if (photo.State != PhotoState.Loaded) continue;
            if (ReferenceEquals(photo, excludePhoto)) continue;

            photo.Unload();
        }
    }


    /// <summary>
    /// Marks a photo as cache-owned as soon as it is decoded.
    /// </summary>
    private void TrackCached(int index)
    {
        lock (_cacheLock)
        {
            _cachedIndexes.Add(index);
        }
    }


    /// <summary>
    /// Unloads the cached photo at the specified index if it was loaded by caching.
    /// </summary>
    public void InvalidateCacheAt(int index)
    {
        lock (_cacheLock)
        {
            if (!_cachedIndexes.Remove(index)) return;
        }

        Get(index)?.Unload();
    }


    /// <summary>
    /// Unloads the cached photo by file path if it was loaded by caching.
    /// </summary>
    public void InvalidateCacheAt(string filePath)
    {
        var index = IndexOf(filePath);
        if (index >= 0) InvalidateCacheAt(index);
    }


    /// <summary>
    /// Core caching loop: expands outward from the center until the memory budget is spent or the list runs out.
    /// </summary>
    private async Task RunCacheAroundAsync(int centerIndex, bool isSlideshow, CancellationToken token)
    {
        try
        {
            var maxMemoryBytes = (long)Core.Config.CacheMaxMemoryInMb * 1024L * 1024L;
            var maxFileSizeBytes = (long)(Core.Config.CacheMaxFileSizeInMb * 1024.0 * 1024.0);
            var maxDimension = Core.Config.CacheMaxDimension;
            var totalCount = (int)Count;

            if (totalCount == 0 || centerIndex < 0) return;

            // half the list per side is the whole list: past that the spiral wraps onto itself
            var maxRange = Math.Min(totalCount / 2 + 1, totalCount);

            // a slideshow only advances forward, so it looks ahead instead of spiralling
            var canLoop = !isSlideshow || Core.Config.EnableLoopSlideshow;
            var indexes = GenerateSpiralIndexes(centerIndex, maxRange, totalCount,
                primaryDirection: isSlideshow ? 1 : 0, canLoop);

            // the next slideshow image ignores the budget and the caps, to keep transitions seamless
            var guaranteedIndex = isSlideshow ? GetForwardIndex(centerIndex, totalCount, canLoop) : -1;

            // the spiral re-visits and re-counts every decoded photo, so seeding from the tracked set double-counts
            var currentPhoto = Get(centerIndex);
            var usedMemory = currentPhoto is null ? 0L : EstimatePhotoMemory(currentPhoto);

            var newCachedSet = new HashSet<int>();

            foreach (var idx in indexes)
            {
                if (token.IsCancellationRequested) return;

                // the user can start holding a navigation key mid-pass
                if (Core.API.IsQuickBrowsing) return;

                var photo = Get(idx);
                if (photo is null) continue;

                var isGuaranteed = idx == guaranteedIndex;

                // already decoded, by an earlier pass or by the viewer
                if (photo.State == PhotoState.Loaded)
                {
                    var photoMem = EstimatePhotoMemory(photo);
                    if (!isGuaranteed && usedMemory + photoMem > maxMemoryBytes) break;

                    usedMemory += photoMem;
                    newCachedSet.Add(idx);
                    continue;
                }

                // loading the viewer's own photo would cancel its in-flight load
                if (idx == CurrentIndex)
                {
                    newCachedSet.Add(idx);
                    continue;
                }

                if (!isSlideshow)
                {
                    if (maxFileSizeBytes > 0 && !SatisfiesFileSizeLimit(photo.FilePath, maxFileSizeBytes))
                    {
                        continue;
                    }

                    // the dimension cap needs real metadata, hence the load
                    if (maxDimension > 0)
                    {
                        await photo.LoadMetadataAsync(useCache: true, token: token);
                        if (token.IsCancellationRequested) return;

                        if (photo.Metadata.Width > maxDimension || photo.Metadata.Height > maxDimension)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    // the caps do not apply to a look-ahead, but the estimate below still needs metadata
                    await photo.LoadMetadataAsync(useCache: true, token: token);
                    if (token.IsCancellationRequested) return;
                }

                // only the guaranteed image may exceed the budget; anything else ends the pass
                var estimatedMem = EstimatePhotoMemoryFromMetadata(photo);
                if (!isGuaranteed && usedMemory + estimatedMem > maxMemoryBytes) break;

                await photo.LoadAsync(useCache: true, skipLoadingEvent: true);

                // track before the cancel check: an untracked decoded photo escapes eviction forever
                TrackCached(idx);
                if (token.IsCancellationRequested) return;

                // charge the real depth: the 8 bytes/px estimate lets an RgbaF32 decode overshoot the budget
                var decodedMem = EstimatePhotoMemory(photo);
                usedMemory += decodedMem > 0 ? decodedMem : estimatedMem;
                newCachedSet.Add(idx);
            }

            if (token.IsCancellationRequested) return;

            lock (_cacheLock)
            {
                _cachedIndexes.Clear();
                foreach (var idx in newCachedSet)
                {
                    _cachedIndexes.Add(idx);
                }
            }

            // evict whatever the budget did not keep, whichever codec decoded it; the viewer owns center/current
            var keepIndexes = new HashSet<int>(newCachedSet) { centerIndex, CurrentIndex };
            UnloadPhotosOutside(keepIndexes, null, keepCurrent: true);
        }
        catch (OperationCanceledException) { /* expected on navigation */ }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌❌❌ RunCacheAroundAsync: {ex.Message}");
        }
    }


    /// <summary>
    /// Generates an ordered list of indexes around <paramref name="centerIndex"/>.
    /// <para>
    /// <paramref name="primaryDirection"/> controls the ordering:
    /// <c>0</c> = balanced spiral (right-1, left-1, right-2, left-2, ...),
    /// <c>+1</c> = forward-first (right-1, right-2, ..., then left-1, left-2, ...),
    /// <c>-1</c> = backward-first. When <paramref name="canLoop"/> is <c>false</c>,
    /// offsets that fall outside the list are skipped instead of wrapping around.
    /// </para>
    /// This preserves insertion order, unlike <see cref="BHelper.GenerateWrappedIndexes"/>
    /// which uses an unordered HashSet.
    /// </summary>
    private static List<int> GenerateSpiralIndexes(int centerIndex, int maxRange, int totalCount,
        int primaryDirection = 0, bool canLoop = true)
    {
        var result = new List<int>(maxRange * 2);
        var seen = new HashSet<int>();

        // resolve a raw offset into a valid index, honoring wrap-around
        void TryAdd(int rawIndex)
        {
            int idx;
            if (canLoop)
            {
                idx = BHelper.ComputeIndexInRange(rawIndex, (uint)totalCount, true);
            }
            else
            {
                if (rawIndex < 0 || rawIndex >= totalCount) return;
                idx = rawIndex;
            }

            if (idx != centerIndex && seen.Add(idx))
            {
                result.Add(idx);
            }
        }

        if (primaryDirection > 0)
        {
            // forward-first (slideshow): all forward, then all backward
            for (var i = 1; i <= maxRange; i++) TryAdd(centerIndex + i);
            for (var i = 1; i <= maxRange; i++) TryAdd(centerIndex - i);
        }
        else if (primaryDirection < 0)
        {
            // backward-first: all backward, then all forward
            for (var i = 1; i <= maxRange; i++) TryAdd(centerIndex - i);
            for (var i = 1; i <= maxRange; i++) TryAdd(centerIndex + i);
        }
        else
        {
            // balanced spiral: right-1, left-1, right-2, left-2, ...
            for (var i = 1; i <= maxRange; i++)
            {
                TryAdd(centerIndex + i);
                TryAdd(centerIndex - i);
            }
        }

        return result;
    }


    /// <summary>
    /// Index of the next image forward, or <c>-1</c> at the end of a non-looping list.
    /// </summary>
    private static int GetForwardIndex(int centerIndex, int totalCount, bool canLoop)
    {
        var raw = centerIndex + 1;
        if (canLoop) return BHelper.ComputeIndexInRange(raw, (uint)totalCount, true);
        return raw < totalCount ? raw : -1;
    }


    /// <summary>
    /// Checks if the file size is within the allowed caching limit.
    /// </summary>
    private static bool SatisfiesFileSizeLimit(string filePath, long maxFileSizeBytes)
    {
        try
        {
            var fi = new FileInfo(filePath);
            return fi.Length <= maxFileSizeBytes;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Real memory footprint of a loaded photo from its decoded pixel depth; <c>0</c> when not loaded.
    /// </summary>
    private static long EstimatePhotoMemory(Photo photo)
    {
        if (photo.State != PhotoState.Loaded) return 0;

        return (long)photo.Width * photo.Height * photo.BytesPerPixel;
    }


    /// <summary>
    /// Estimates memory from metadata before the photo is fully loaded.
    /// </summary>
    private static long EstimatePhotoMemoryFromMetadata(Photo photo)
    {
        var w = photo.Metadata.Width;
        var h = photo.Metadata.Height;
        if (w == 0 || h == 0) return 8L * 1024 * 1024; // fallback estimate: 8 MB

        // a floor, not the truth: Magick's HDR path really lands on RgbaF32, corrected after the decode
        var bytesPerPixel = photo.Metadata.BitsPerChannel > 8 ? 8 : 4;

        return (long)w * h * bytesPerPixel;
    }


    /// <summary>
    /// Sums up the estimated memory of the current photo and all cached photos.
    /// </summary>
    private long EstimateCachedMemory(int centerIndex)
    {
        long total = 0;

        var current = Get(centerIndex);
        if (current is not null)
        {
            total += EstimatePhotoMemory(current);
        }

        lock (_cacheLock)
        {
            foreach (var idx in _cachedIndexes)
            {
                var photo = Get(idx);
                if (photo is not null)
                {
                    total += EstimatePhotoMemory(photo);
                }
            }
        }

        return total;
    }
}
