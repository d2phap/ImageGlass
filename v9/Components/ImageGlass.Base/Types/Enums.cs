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
    /// <summary>
    /// Build 22621
    /// </summary>
    Win11_22H2,

    /// <summary>
    /// Build 22000
    /// </summary>
    Win11OrLater,
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
/// Flip options.
/// </summary>
[Flags]
public enum FlipOptions
{
    None = 0,
    Horizontal = 1 << 1,
    Vertical = 1 << 2,
}


/// <summary>
/// Selection aspect ratio.
/// </summary>
public enum SelectionAspectRatio
{
    Free = 0,
    Original = 1 << 1,
    Square = 1 << 2,
    _16_9 = 1 << 3,
    _9_16 = 1 << 4,
    _4_3 = 1 << 5,
    _3_4 = 1 << 6,
    _3_2 = 1 << 7,
    _2_3 = 1 << 8,
    _2_1 = 1 << 9,
    _1_2 = 1 << 10,
}