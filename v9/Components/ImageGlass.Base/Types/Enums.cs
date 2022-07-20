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
/// The loading order list.
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderBy
{
    Name = 0,
    Length = 1,
    CreationTime = 2,
    Extension = 3,
    LastAccessTime = 4,
    LastWriteTime = 5,
    Rating = 6,
    Random = 7,
}

/// <summary>
/// The loading order types list
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderType
{
    Asc = 0,
    Desc = 1
}

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
/// The list of mousewheel actions.
/// **If we need to rename, have to update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum MouseWheelActions
{
    DoNothing = 0,
    Zoom = 1,
    ScrollVertically = 2,
    ScrollHorizontally = 3,
    BrowseImages = 4
}

/// <summary>
/// Define the flags to tell frmMain update the UI
/// </summary>
[Flags]
public enum ForceUpdateAction
{
    None = 0,
    Language = 1 << 1,
    MenuHotkeys = 1 << 2,
    Toolbar = 1 << 3,
    ThumbnailBar = 1 << 4,
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
/// Actions the user can assign to keys
/// </summary>
public enum AssignableActions
{
    DoNothing = -1,    // error case
    PrevNextImage = 0, // previous/next image in list
    PanLeftRight,      // pan current image left/right
    PanUpDown,         // pan current image up/down
    ZoomInOut,         // zoom current image in/out
    PauseSlideshow,    // placeholder for V6 space key behavior

}

/// <summary>
/// User customizable key pairs
/// </summary>
public enum KeyCombos
{
    LeftRight = 0, // left/right arrow keys
    UpDown,      // up/down arrow keys
    PageUpDown,  // pageup/pagedown keys
    SpaceBack,   // space, backspace keys
}

/// <summary>
/// Supported actions which can be assigned to mouse click
/// </summary>
public enum MouseAction
{
    ToggleZoomFit = 0, // switch between 100% and fit-to-window
    ZoomIn,            // zoom in by zoom step
    ZoomOut,           // zoom out by zoom step
    NextImage,         // next image in list
    PrevImage,         // previous image in list
    ToggleFullScreen,  // toggle full-screen mode
    PopupMenu,         // bring up the popup menu
    ColorPick,         // select color under mouse cursor
    ZoomInToMouse,     // zoom in by zoom step, centered on mouse position
    ZoomOutToAuto,     // zoom out to Auto-zoom level
}

/// <summary>
/// Supported customizable mouse click
/// </summary>
public enum MouseClick
{
    Button1,    // Left single
    Button1Dbl,
    Button2,    // Right single
    Button2Dbl,
    Button3,    // Middle
    Button3Dbl,
    Button4,    // X1
    Button4Dbl,
    Button5,    // X2
    Button5Dbl,
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


/// <summary>
/// Types of image infomation update request
/// </summary>
[Flags]
public enum BasicInfoUpdate
{
    None = 0,
    All = 1 << 1,
    AppName = 1 << 2,
    Name = 1 << 3,
    Path = 1 << 4,
    FileSize = 1 << 5,
    Dimension = 1 << 6,
    ListCount = 1 << 7,
    Zoom = 1 << 8,
    FramesCount = 1 << 9,

    DateTimeAuto = 1 << 10,
    ModifiedDateTime = 1 << 11,
    ExifDateTime = 1 << 12,
    ExifDateTimeOriginal = 1 << 13,

    ExifRating = 1 << 14,
}