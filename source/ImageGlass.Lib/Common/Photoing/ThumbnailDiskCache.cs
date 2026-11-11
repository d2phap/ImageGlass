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
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.IO;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Manages a persistent disk cache for gallery thumbnails.
/// </summary>
internal static class ThumbnailDiskCache
{
    private static string? _cacheDir;
    private static InterlockedBool _cleanupScheduled = new(false);
    private static readonly Lock _evictionLock = new();


    /// <summary>
    /// Gets the cache directory path, creating it on first access.
    /// </summary>
    private static string CacheDir => _cacheDir ??= BHelper.ConfigDir(Dir.Cache, Dir.Cache_Thumbnails);



    #region Public Methods

    /// <summary>
    /// Tries to load a cached thumbnail from disk.
    /// </summary>
    public static async Task<SKImage?> TryGetAsync(string filePath, int thumbSize, DateTime sourceLastWriteTimeUtc, CancellationToken token = default)
    {
        if (Core.Config.GalleryCacheSizeInMb == 0)
        {
            ScheduleCleanupOnce();
            return null;
        }
        if (string.IsNullOrEmpty(filePath) || sourceLastWriteTimeUtc == default) return null;

        var cachePath = GetCacheFilePath(filePath, thumbSize);

        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(cachePath)) return null;

                // validate: cache must be newer than source file
                var cacheTime = File.GetLastWriteTimeUtc(cachePath);
                if (cacheTime < sourceLastWriteTimeUtc) return null;

                // decode: force immediate rasterization so the image
                // does not depend on the file-mapped data after this block
                using var data = SKData.Create(cachePath);
                if (data is null) return null;

                using var bmp = SKBitmap.Decode(data);
                if (bmp is null) return null;

                var image = SKImage.FromBitmap(bmp);
                if (image.IsDisposed()) return null;

                // touch last access time for LRU eviction
                try
                {
                    File.SetLastAccessTimeUtc(cachePath, DateTime.UtcNow);
                }
                catch { }

                return image;
            }
            catch
            {
                return null;
            }
        }, token).ConfigureAwait(false);
    }


    /// <summary>
    /// Encodes and writes a thumbnail to the disk cache. Triggers async eviction if over budget.
    /// No-op if disk caching is disabled.
    /// </summary>
    /// <remarks>
    /// The caller must keep <paramref name="image"/> alive until this completes: encoding runs on a
    /// background thread so it never blocks the gallery pipeline, so this must be awaited rather
    /// than fired and forgotten.
    /// </remarks>
    public static async Task PutAsync(string filePath,
        int thumbSize, SKImage image, CancellationToken token = default)
    {
        if (Core.Config.GalleryCacheSizeInMb == 0)
        {
            ScheduleCleanupOnce();
            return;
        }
        if (string.IsNullOrEmpty(filePath)) return;
        if (image.IsDisposed()) return;

        var cachePath = GetCacheFilePath(filePath, thumbSize);

        // encode + write off the calling thread
        await Task.Run(() =>
        {
            try
            {
                if (image.IsDisposed()) return;

                using var encoded = image.Encode(SKEncodedImageFormat.Webp, 80);
                if (encoded is null || encoded.Size == 0) return;

                Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

                using var fs = File.Create(cachePath);
                encoded.SaveTo(fs);
            }
            catch { }
        }, token).ConfigureAwait(false);

        // fire-and-forget eviction check
        _ = Task.Run(TryEvict, CancellationToken.None);
    }


    /// <summary>
    /// Removes all cached thumbnails for a specific file (all sizes).
    /// </summary>
    public static void Invalidate(string filePath)
    {
        if (Core.Config.GalleryCacheSizeInMb == 0) return;
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            var pathHash = ComputePathHash(filePath);
            var dir = CacheDir;
            if (!Directory.Exists(dir)) return;

            foreach (var file in Directory.EnumerateFiles(dir, $"{pathHash}_*.webp"))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
        }
        catch { }
    }


    /// <summary>
    /// Clears the entire disk cache. Returns the failure, or <c>null</c> when the cache is gone
    /// (a thumbnail being written concurrently can keep the folder locked).
    /// </summary>
    public static Exception? Clear()
    {
        try
        {
            var dir = CacheDir;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            _cacheDir = null;
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    #endregion // Public Methods



    #region Private Methods

    /// <summary>
    /// Computes a hex hash of the file path for the cache key prefix.
    /// </summary>
    private static string ComputePathHash(string filePath)
    {
        Span<byte> hashBytes = stackalloc byte[16];
        XxHash128.Hash(MemoryMarshal.AsBytes(filePath.AsSpan()), hashBytes);

        return Convert.ToHexStringLower(hashBytes);
    }


    /// <summary>
    /// Gets the full cache file path for a given source file and thumbnail size.
    /// </summary>
    private static string GetCacheFilePath(string filePath, int thumbSize)
    {
        var pathHash = ComputePathHash(filePath);
        return Path.Combine(CacheDir, $"{pathHash}_{thumbSize}.webp");
    }


    /// <summary>
    /// Evicts oldest-accessed cache files if total size exceeds budget.
    /// Uses <see cref="Lock.TryEnter()"/> to ensure only one eviction runs at a time.
    /// </summary>
    private static void TryEvict()
    {
        if (!_evictionLock.TryEnter()) return;

        try
        {
            var maxBytes = (long)Core.Config.GalleryCacheSizeInMb * 1024L * 1024L;
            if (maxBytes <= 0) return;

            var dir = new DirectoryInfo(CacheDir);
            if (!dir.Exists) return;

            var files = dir.GetFiles("*.webp");
            var totalSize = 0L;
            for (var i = 0; i < files.Length; i++)
            {
                totalSize += files[i].Length;
            }

            if (totalSize <= maxBytes) return;

            // sort by last access time ascending (oldest first)
            Array.Sort(files, static (a, b) => a.LastAccessTimeUtc.CompareTo(b.LastAccessTimeUtc));

            for (var i = 0; i < files.Length && totalSize > maxBytes; i++)
            {
                totalSize -= files[i].Length;
                try
                {
                    files[i].Delete();
                }
                catch { }
            }
        }
        catch { }
        finally
        {
            _evictionLock.Exit();
        }
    }


    /// <summary>
    /// Schedules a one-time cleanup of old cache files when caching is disabled.
    /// </summary>
    private static void ScheduleCleanupOnce()
    {
        if (_cleanupScheduled.SetTrue())
        {
            _ = Task.Run(Clear, CancellationToken.None);
        }
    }

    #endregion // Private Methods


}
