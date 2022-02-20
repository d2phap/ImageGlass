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

namespace ImageGlass.Base.PhotoBox;


/// <summary>
/// Zoom modes
/// </summary>
public enum ZoomMode
{
    AutoZoom = 1,
    LockZoom = 2,
    ScaleToFit = 3,
    ScaleToWidth = 4,
    ScaleToHeight = 5,
    ScaleToFill = 6,
}

/// <summary>
/// Interpolation modes.
/// These values are based on <b>D2DBitmapInterpolationMode</b> of <b>D2DLib</b>.
/// </summary>
public enum ImageInterpolation : int
{
    NearestNeighbor = 0,
    Linear = 1,
}


/// <summary>
/// Specifies the display styles for the background texture grid
/// </summary>
public enum CheckerboardMode
{
    /// <summary>
    /// No background.
    /// </summary>
    None = 0,

    /// <summary>
    /// Background is displayed in the control's client area.
    /// </summary>
    Client = 1,

    /// <summary>
    /// Background is displayed only in the image region.
    /// </summary>
    Image = 2,
}


/// <summary>
/// Specifies the display styles for navigation button
/// </summary>
public enum NavButtonDisplay {
    None = 0,
    Both = 1,
    Left = 2,
    Right = 3,
}
