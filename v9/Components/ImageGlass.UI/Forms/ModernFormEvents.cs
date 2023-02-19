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
*/

using ImageGlass.Base.WinApi;

namespace ImageGlass.UI;

public class SystemColorModeChangedEventArgs
{
    /// <summary>
    /// Checks if the app color mode is dark.
    /// </summary>
    public bool IsDarkMode => WinColorsApi.IsDarkMode;


    /// <summary>
    /// Gets, sets value indicates that the event is already handled.
    /// </summary>
    public bool Handled { get; set; } = false;

}


public class SystemAccentColorChangedEventArgs
{
    /// <summary>
    /// Gets system accent color.
    /// </summary>
    public Color AccentColor => WinColorsApi.GetAccentColor(true);


    /// <summary>
    /// Gets, sets value indicates that the event is already handled.
    /// </summary>
    public bool Handled { get; set; } = false;

}
