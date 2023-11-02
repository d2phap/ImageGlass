/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

using ImageGlass.Base.Photoing.Codecs;

namespace ImageGlass.Gallery;

/// <summary>
/// Interface for thumbnail, metadata and shell info extractors.
/// </summary>
public interface IExtractor
{
    /// <summary>
    /// Gets the name of this extractor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Returns image metadata.
    /// </summary>
    /// <param name="path">Filepath of image</param>
    IgMetadata GetMetadata(string path);

    /// <summary>
    /// Creates a thumbnail from the given image file.
    /// </summary>
    /// <param name="filename">The filename pointing to an image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given file or null if an error occurs.</returns>
    Image? GetThumbnail(string filename, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation);

    /// <summary>
    /// Creates a thumbnail from the given image.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given image or null if an error occurs.</returns>
    Image? GetThumbnail(Image image, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation);

    /// <summary>
    /// Returns shell info.
    /// </summary>
    /// <param name="path">Filepath of image</param>
    ShellInfo GetShellInfo(string path);
}
