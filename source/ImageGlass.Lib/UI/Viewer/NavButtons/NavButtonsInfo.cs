/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia;
using Avalonia.Media;

namespace ImageGlass.UI.Viewer;

internal class NavButtonsInfo
{
    // Size of the navigation button
    public static readonly Size NAV_BTN_SIZE = new Size(50, 50);

    // Margin from the control edge to the button
    public static readonly double NAV_BTN_MARGIN = 10;


    /// <summary>
    /// Indicates whether the navigation buttons feature are enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;


    /// <summary>
    /// Gets, sets the SVG file path for the right arrow icon.
    /// </summary>
    public string RightArrowSvgPath { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets the SVG file path for the left arrow icon.
    /// </summary>
    public string LeftArrowSvgPath { get; set; } = string.Empty;


    /// <summary>
    /// Indicates whether the pointer is in the left hit area.
    /// </summary>
    public bool IsLeftHovered { get; set; } = false;


    /// <summary>
    /// Indicates whether the pointer is in the right hit area.
    /// </summary>
    public bool IsRightHovered { get; set; } = false;


    /// <summary>
    /// Indicates whether the pointer is down in the left hit area.
    /// </summary>
    public bool IsLeftPressed { get; set; } = false;


    /// <summary>
    /// Indicates whether the pointer is down in the right hit area.
    /// </summary>
    public bool IsRightPressed { get; set; } = false;


    /// <summary>
    /// Represents the initial press position for click detection.
    /// </summary>
    public Point? PointerDownPoint { get; set; } = null;


    /// <summary>
    /// Indicates whether the pointer moved beyond the click threshold after press.
    /// </summary>
    public bool IsDragging { get; set; } = false;


    /// <summary>
    /// Cached SVG icon for the left arrow.
    /// </summary>
    public IImage? LeftIcon { get; set; } = null;


    /// <summary>
    /// Cached SVG icon for the right arrow.
    /// </summary>
    public IImage? RightIcon { get; set; } = null;


    /// <summary>
    /// Resets all hover/press/drag state to defaults.
    /// </summary>
    public void ResetState()
    {
        IsLeftHovered = false;
        IsRightHovered = false;
        IsLeftPressed = false;
        IsRightPressed = false;
        PointerDownPoint = null;
        IsDragging = false;
    }
}
