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
using ImageGlass.Common.Photoing;
using SkiaSharp;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public interface IPhotoPreviewProvider
{
    /// <summary>
    /// Tries to retrieve a thumbnail that is already present in the platform cache.
    /// This operation must not decode the source image.
    /// </summary>
    Task<SKImage?> TryGetCachedThumbnailAsync(string filePath, int size,
        CancellationToken token = default);


    /// <summary>
    /// Retrieves an embedded thumbnail from either a RAW format or an EXIF profile if exists.
    /// </summary>
    /// <param name="skipPlatformCache">The caller already probed the platform cache.</param>
    Task<SKImage?> GetPreviewAsync(PhotoMetadata meta, double? minHeight,
        CancellationToken token = default, bool skipPlatformCache = false);


    /// <summary>
    /// Gets thumbnail of the photo.
    /// </summary>
    /// <param name="skipPlatformCache">The caller already probed the platform cache.</param>
    Task<SKImage?> GetThumbnailAsync(PhotoMetadata meta, double minHeight,
        CancellationToken token = default, bool skipPlatformCache = false);

}
