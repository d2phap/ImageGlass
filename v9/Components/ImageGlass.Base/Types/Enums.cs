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

namespace ImageGlass.Base;


/// <summary>
/// The list of Zoom Optimization.
/// **If we need to rename, have to update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ZoomOptimizationMethods
{
    /// <summary>
    /// Combination of NearestNeighbor
    /// </summary>
    Auto = 0,
    /// <summary>
    /// Specifies low quality interpolation.
    /// </summary>
    Low = 1,
    /// <summary>
    /// Specifies high quality interpolation.
    /// </summary>
    High = 2,
    /// <summary>
    /// Specifies bilinear interpolation. No prefiltering is done. This mode is not suitable
    /// for shrinking an image below 50 percent of its original size.
    /// </summary>
    Bilinear = 3,
    /// <summary>
    /// Specifies bicubic interpolation. No prefiltering is done. This mode is not suitable
    /// for shrinking an image below 25 percent of its original size.
    /// </summary>
    Bicubic = 4,
    /// <summary>
    /// Specifies nearest-neighbor interpolation.
    /// </summary>
    NearestNeighbor = 5,
    /// <summary>
    /// Specifies high-quality, bilinear interpolation. Prefiltering is performed to
    /// ensure high-quality shrinking.
    /// </summary>
    HighQualityBilinear = 6,
    /// <summary>
    /// Specifies high-quality, bicubic interpolation. Prefiltering is performed to ensure
    /// high-quality shrinking. This mode produces the highest quality transformed images.
    /// </summary>
    HighQualityBicubic = 7,
}


/// <summary>
/// Define the flags to tell FrmMain update the UI
/// </summary>
[Flags]
public enum UpdateRequests
{
    None = 0,
    Language = 1 << 1,
    MenuHotkeys = 1 << 2,
    Toolbar = 1 << 3,
    ThumbnailBar = 1 << 4,
    MouseActions = 1 << 5,
}


/// <summary>
/// Toolbar position
/// </summary>
public enum ToolbarPosition
{
    Top = 0,
    Bottom = 1,
}


/// <summary>
/// Color channels of image, the value should be same as MagickImage.Channels enum
/// </summary>
public enum ColorChannel
{
    All = -1, // not applicable

    Red = 1,
    Green = 2,
    Blue = 4,
    Black = 8,
    Alpha = 16,
}


/// <summary>
/// Types of path
/// </summary>
public enum PathType
{
    File,
    Dir,
}


/// <summary>
/// Actions after opening editing app
/// </summary>
public enum AfterOpeningEditAppAction
{
    Nothing = 0,
    Minimize = 1,
    Close = 2,
}


/// <summary>
/// Determines Windows OS requirement
/// </summary>
public enum WindowsOS
{
    Win11,
    Win10,
    Win10OrLater,
    Win7,
}


/// <summary>
/// Exit codes of ImageGlass ultilities
/// </summary>
public enum IgExitCode : int
{
    Done = 0,
    Error = 1,
    AdminRequired = 2,
}

