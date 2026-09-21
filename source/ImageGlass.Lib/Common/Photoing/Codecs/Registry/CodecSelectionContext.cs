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
namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides runtime selection data used to choose the appropriate codec.
/// </summary>
public sealed class CodecSelectionContext
{
    /// <summary>
    /// Gets or sets a value indicating whether vector rendering is enabled.
    /// </summary>
    public bool EnableVectorRenderer { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the destination color profile is supported.
    /// </summary>
    public bool IsDestColorProfileSupported { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether a raw embedded preview should be used instead of the full image.
    /// </summary>
    public bool LoadRawThumbnailOnly { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether a non-raw embedded preview should be used instead of the full image.
    /// </summary>
    public bool LoadOtherThumbnailOnly { get; init; }

    /// <summary>
    /// Gets or sets the minimum width an embedded preview must have to be used instead of the full image.
    /// </summary>
    public int PreviewMinWidth { get; init; }

    /// <summary>
    /// Gets or sets the minimum height an embedded preview must have to be used instead of the full image.
    /// </summary>
    public int PreviewMinHeight { get; init; }
}