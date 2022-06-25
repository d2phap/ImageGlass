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
*/
namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Settings for loading image
/// </summary>
public record CodecReadOptions
{
    /// <summary>
    /// Gets, sets the requested width of the image
    /// </summary>
    public int Width = 0;

    /// <summary>
    /// Gets, sets the requested height of the image
    /// </summary>
    public int Height = 0;

    /// <summary>
    /// Gets, sets the value indicates whether the color profile should be ignored.
    /// </summary>
    public bool IgnoreColorProfile = false;

    /// <summary>
    /// Gets sets ColorProfile name of path
    /// </summary>
    public string ColorProfileName { get; set; } = Constants.CURRENT_MONITOR_PROFILE;

    /// <summary>
    /// Gets, sets the value indicates if the <see cref="ColorProfileName"/>
    /// should apply to all image files
    /// </summary>
    public bool ApplyColorProfileForAll { get; set; } = false;

    /// <summary>
    /// Gets, sets the value of ImageMagick.Channels to apply to the entire image list
    /// </summary>
    public ColorChannel ImageChannel { get; set; } = ColorChannel.All;

    /// <summary>
    /// Gets, sests the value indicates that the embedded thumbnail should be return (if found).
    /// </summary>
    public bool UseEmbeddedThumbnail { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates the RAW embedded thumbnail should be returned (if found).
    /// </summary>
    public bool UseRawThumbnail { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that the first frame of the image should be returned.
    /// If it's <c>null</c>, the coder will decide.
    /// </summary>
    public bool? FirstFrameOnly { get; set; } = null;


    /// <summary>
    /// Initializes <see cref="CodecReadOptions"/> instance.
    /// </summary>
    public CodecReadOptions()
    {
    }
}
