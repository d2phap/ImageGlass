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
namespace ImageGlass.UI.Viewer;


/// <summary>
/// Defines different types of selection resizers for a user interface.
/// Each value represents a specific corner or edge of a selection area.
/// </summary>
public enum SelectionResizerType
{
    TopLeft = 0,
    Top = 1 << 1,
    TopRight = 1 << 2,
    Right = 1 << 3,
    BottomRight = 1 << 4,
    Bottom = 1 << 5,
    BottomLeft = 1 << 6,
    Left = 1 << 7,
}


public enum SelectionAction
{
    None,

    /// <summary>
    /// User is dragging to draw the selection.
    /// </summary>
    Drawing,

    /// <summary>
    /// User is resizing the selection.
    /// </summary>
    Resizing,

    // User is moving the selection.
    Moving,
}
