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
using Avalonia.Input;

namespace ImageGlass.UI.Viewer;


/// <summary>
/// Initialize a new <see cref="SelectionResizer"/> instance.
/// </summary>
public class SelectionResizer(SelectionResizerType position, Rect indicatorRegion, Rect hitRegion)
{
    /// <summary>
    /// Gets, sets the type of the resizer
    /// </summary>
    public SelectionResizerType Type { get; set; } = position;


    /// <summary>
    /// Gets, sets the region to resize.
    /// </summary>
    public Rect HitRegion { get; set; } = hitRegion;


    /// <summary>
    /// Gets, sets the region to draw resizer.
    /// </summary>
    public Rect IndicatorRegion { get; set; } = indicatorRegion;


    /// <summary>
    /// Gets cursor for current resizer type.
    /// </summary>
    public Cursor GetCursor()
    {
        return Type switch
        {
            SelectionResizerType.TopLeft => new Cursor(StandardCursorType.TopLeftCorner),
            SelectionResizerType.Top => new Cursor(StandardCursorType.SizeNorthSouth),
            SelectionResizerType.TopRight => new Cursor(StandardCursorType.TopRightCorner),
            SelectionResizerType.Right => new Cursor(StandardCursorType.SizeWestEast),
            SelectionResizerType.BottomRight => new Cursor(StandardCursorType.BottomRightCorner),
            SelectionResizerType.Bottom => new Cursor(StandardCursorType.SizeNorthSouth),
            SelectionResizerType.BottomLeft => new Cursor(StandardCursorType.BottomLeftCorner),
            SelectionResizerType.Left => new Cursor(StandardCursorType.SizeWestEast),
            _ => Cursor.Default,
        };
    }


}

