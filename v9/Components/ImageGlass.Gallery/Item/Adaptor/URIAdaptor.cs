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
using System.Net;
using System.Text;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents a URI adaptor.
/// </summary>
public class UriAdaptor : IAdaptor
{
    private bool _isDisposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriAdaptor"/> class.
    /// </summary>
    public UriAdaptor() { }

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

        var uri = (string)key;
        try
        {
            using var client = new WebClient();
            var imageData = client.DownloadData(uri);
            using var stream = new MemoryStream(imageData);
            using var sourceImage = Image.FromStream(stream);

            return Extractor.Current.GetThumbnail(sourceImage, size, useEmbeddedThumbnails, useExifOrientation);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a unique identifier for this thumbnail to be used in persistent
    /// caching.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>A unique identifier string for the thumbnail.</returns>
    public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        var sb = new StringBuilder();
        sb.Append((string)key); // Uri
        sb.Append(':');
        sb.Append(size.Width); // Thumbnail size
        sb.Append(',');
        sb.Append(size.Height);
        sb.Append(':');
        sb.Append(useEmbeddedThumbnails);
        sb.Append(':');
        sb.Append(useExifOrientation);
        sb.Append(':');

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

        var uri = (string)key;
        try
        {
            var filename = Path.GetTempFileName();
            using var client = new WebClient();
            client.DownloadFile(uri, filename);

            return filename;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns the details for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>An array of 2-tuples containing item details or null if an error occurs.</returns>
    public override ImageDetails GetDetails(object key)
    {
        return new();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        _isDisposed = true;
    }
}

