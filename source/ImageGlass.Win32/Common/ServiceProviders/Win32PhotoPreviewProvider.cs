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
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using SkiaSharp;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Win32.Common.ServiceProviders;

public class Win32PhotoPreviewProvider : PhotoPreviewProvider
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async Task<SKImage?> TryGetCachedThumbnailAsync(string filePath, int size,
        CancellationToken token = default)
    {
        if (!Core.Config.EnableGalleryShellThumbnail) return null;

        // no gate: the caller already holds a throttle slot, so this would serialize everything
        return await Task.Run(
            () => Win32ShellThumbnailApi.GetThumbnail(filePath, size, size, true), token)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// Tries to use native platform API to get the shell thumbnail if allowed.
    /// </summary>
    public override async Task<SKImage?> GetPreviewAsync(PhotoMetadata meta, double? minHeight,
        CancellationToken token = default, bool skipPlatformCache = false)
    {
        // 0. if don't use shell thumbnail if not allowed
        if (!Core.Config.EnableGalleryShellThumbnail)
        {
            return await base.GetPreviewAsync(meta, minHeight, token, skipPlatformCache);
        }


        var size = (int)(minHeight ?? double.MinValue);
        SKImage? imgPreview = null;
        var needPreprocess = false;


        // 1. fast path: try Shell cache only (instant, no decoding).
        // The Shell cache only holds discrete size tiers (96/256/768/1920), so a hit can be far
        // smaller than requested; keep it as a fallback but keep looking for a sharper source.
        if (!skipPlatformCache)
        {
            var imgShellCached = await Task.Run(() => Win32ShellThumbnailApi.GetThumbnail(meta.FilePath, size, size, true))
                .ConfigureAwait(false);
            PhotoTrace.Mark("preview:shell-cache", null, $"{meta.FilePath} -> {Describe(imgShellCached)}");
            _ = KeepLarger(ref imgPreview, imgShellCached); // Shell output needs no post-processing
        }


        // 2. fast path: native scaled decode via SkiaSharp. Skipped for a plugin-owned format,
        // which Skia does not know but can still mis-sniff into a garbage frame.
        var isLargeEnough = IsPreviewLargeEnough(imgPreview, meta, size);
        var isPluginFormat = IsPluginOwnedFormat(meta);
        if (!isLargeEnough && !isPluginFormat)
        {
            var imgDecoded = await Task.Run(() => SkiaCodec.LoadThumbnail(meta.FilePath, size), token)
                .ConfigureAwait(false);
            PhotoTrace.Mark("preview:skia", null, $"{meta.FilePath} -> {Describe(imgDecoded)}");
            var useDecoded = KeepLarger(ref imgPreview, imgDecoded);
            if (useDecoded) needPreprocess = true;
        }


        // 3. try getting thumbnail from Shell; this one hits the disk, so the Shell can extract a
        // bigger tier than the cache had
        isLargeEnough = IsPreviewLargeEnough(imgPreview, meta, size);
        if (!isLargeEnough)
        {
            var imgShell = await Task.Run(() => Win32ShellThumbnailApi.GetThumbnail(meta.FilePath, size, size, false))
                .ConfigureAwait(false);
            PhotoTrace.Mark("preview:shell-disk", null, $"{meta.FilePath} -> {Describe(imgShell)}");
            var useShell = KeepLarger(ref imgPreview, imgShell);
            if (useShell) needPreprocess = false;
        }


        // 4. try embedded EXIF preview
        isLargeEnough = IsPreviewLargeEnough(imgPreview, meta, size);
        if (!isLargeEnough)
        {
            using var thumbM = meta.GetEmbeddedPreview();
            if (thumbM is not null && thumbM.Height >= minHeight)
            {
                var imgEmbedded = SkiaCodec.FromMagick(thumbM, meta.SkiaColorSpace);
                var useEmbedded = KeepLarger(ref imgPreview, imgEmbedded);
                if (useEmbedded) needPreprocess = true;
            }
        }


        // 5. process preview
        if (needPreprocess && TryProcessImage(imgPreview, meta, out var imgProcessed))
        {
            imgPreview?.Dispose();
            imgPreview = imgProcessed;
        }


        if (imgPreview.IsDisposed()) imgPreview = null;
        return imgPreview;
    }


}
