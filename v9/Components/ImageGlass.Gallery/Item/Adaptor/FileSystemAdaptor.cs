/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using ImageGlass.Base;
using ImageGlass.Base.Cache;
using System.Text;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents a file system adaptor.
/// </summary>
public class FileSystemAdaptor : IAdaptor
{
    // Use a cache for commonly repeated strings
    private static readonly StringCache _stringCache = new();

    private bool _isDisposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemAdaptor"/> class.
    /// </summary>
    public FileSystemAdaptor() { }

    /// <summary>
    /// Returns the thumbnail image for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
    public override Image? GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        if (_isDisposed) return null;

        // Issue #530: thumbnails not built if long file path
        var filename = Helpers.PrefixLongPath((string)key);

        if (File.Exists(filename))
            return Extractor.Instance.GetThumbnail(filename, size, useEmbeddedThumbnails, useExifOrientation);
        else
            return null;
    }

    /// <summary>
    /// Returns a unique identifier for this thumbnail to be used in persistent
    /// caching.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>A unique identifier string for the thumnail.</returns>
    public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        var sb = new StringBuilder();
        sb.Append((string)key); // Filename
        sb.Append(':');
        sb.Append(size.Width); // Thumbnail size
        sb.Append(',');
        sb.Append(size.Height);
        sb.Append(':');
        sb.Append(useEmbeddedThumbnails);
        sb.Append(':');
        sb.Append(useExifOrientation);

        return sb.ToString();
    }

    /// <summary>
    /// Returns the path to the source image for use in drag operations.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>The path to the source image.</returns>
    public override string GetSourceImage(object key)
    {
        if (_isDisposed) return string.Empty;

        return (string)key;
    }

    /// <summary>
    /// Returns the details for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
    public override ImageDetails GetDetails(object key)
    {
        var details = new ImageDetails();
        if (_isDisposed) return details;

        var filename = (string)key;
        

        // Get file info
        if (File.Exists(filename))
        {
            var info = new FileInfo(filename);

            details.DateCreated = info.CreationTime;
            details.DateAccessed = info.LastAccessTime;
            details.DateModified = info.LastWriteTime;
            details.FileSize = info.Length;
            details.FilePath = filename;
            details.FileName = Path.GetFileName(filename);
            details.FileExtension = Path.GetExtension(filename).ToUpperInvariant();
            details.FolderPath = _stringCache.GetFromCache(info.DirectoryName ?? "");
            details.FolderName = info.Directory?.Name ?? "";

            // Get metadata
            var metadata = Extractor.Instance.GetMetadata(filename);
            details.Width = metadata.Width;
            details.Height = metadata.Height;
            details.DPIX = metadata.DPIX;
            details.DPIY = metadata.DPIY;
            details.ImageDescription = metadata.ImageDescription ?? "";
            details.EquipmentModel = metadata.EquipmentModel ?? "";
            details.DateTaken = metadata.DateTaken;
            details.Artist = metadata.Artist ?? "";
            details.Copyright = metadata.Copyright ?? "";
            details.ExposureTime = (float)metadata.ExposureTime;
            details.FNumber = (float)metadata.FNumber;
            details.ISOSpeed = (ushort)metadata.ISOSpeed;
            details.Comment = metadata.Comment ?? "";
            details.Rating = (ushort)metadata.Rating;
            details.Software = metadata.Software ?? "";
            details.FocalLength = (float)metadata.FocalLength;
        }

        return details;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() => _isDisposed = true;

}

