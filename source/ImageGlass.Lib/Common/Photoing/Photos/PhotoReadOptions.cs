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
/// Settings for loading image
/// </summary>
public record PhotoReadOptions
{

    /// <summary>
    /// Gets, sets the requested width of the image.
    /// </summary>
    public uint Width { get; set; } = 0;


    /// <summary>
    /// Gets, sets the requested height of the image.
    /// </summary>
    public uint Height { get; set; } = 0;


    /// <summary>
    /// Gets, sets the value indicates the embedded thumbnail of the RAW formats should be returned (if found).
    /// </summary>
    public bool OnlyLoadRawPreview { get; set; } = false;


    /// <summary>
    /// Gets, sets the value indicates the embedded thumbnail of the non-RAW formats should be returned (if found).
    /// </summary>
    public bool OnlyLoadNonRawPreview { get; set; } = false;


    /// <summary>
    /// Gets, sets the minimum width of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="OnlyLoadRawPreview"/> or <see cref="OnlyLoadNonRawPreview"/> is <c>true</c>.
    /// </summary>
    public int PreviewMinWidth { get; set; } = 0;


    /// <summary>
    /// Gets, sets the minimum height of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="OnlyLoadRawPreview"/> or <see cref="OnlyLoadNonRawPreview"/> is <c>true</c>.
    /// </summary>
    public int PreviewMinHeight { get; set; } = 0;


    /// <summary>
    /// Gets, sets the value indicates that the incorrect rotation should be fixed
    /// </summary>
    public bool CorrectRotation { get; set; } = true;


    /// <summary>
    /// Gets, sets the requested image frame index for metadata reading.
    /// If the frame index is <c> less than 0</c>, it will attempt to decode all frames.
    /// </summary>
    public int FrameIndex { get; set; } = 0;


    /// <summary>
    /// Initializes <see cref="PhotoReadOptions"/> instance.
    /// </summary>
    public PhotoReadOptions()
    {
    }


    /// <summary>
    /// Builds the options from the current user settings; every decode of a listed photo must use these.
    /// </summary>
    public static PhotoReadOptions FromConfig(int frameIndex = 0) => new()
    {
        FrameIndex = frameIndex,
        OnlyLoadRawPreview = Core.Config.EnableOnlyLoadRawPreview,
        OnlyLoadNonRawPreview = Core.Config.EnableOnlyLoadNonRawPreview,
        PreviewMinWidth = Core.Config.PreviewMinWidth,
        PreviewMinHeight = Core.Config.PreviewMinHeight,
    };
}
