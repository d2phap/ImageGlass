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

namespace ImageGlass.Base;

public static class GraphicsExtensions
{
    /// <summary>
    /// Fills rounded rectangle.
    /// </summary>
    public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
    {
        using var path = BHelper.GetRoundRectanglePath(rect, radius, flatBottom, bottomOffset, flatTop);
        g.FillPath(brush, path);
    }


    /// <summary>
    /// Draws rounded rectangle.
    /// </summary>
    public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
    {
        using var path = BHelper.GetRoundRectanglePath(rect, radius, flatBottom, bottomOffset, flatTop);
        g.DrawPath(pen, path);
    }

}
