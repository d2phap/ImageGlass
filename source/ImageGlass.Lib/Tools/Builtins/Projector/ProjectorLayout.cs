/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Tools;


/// <summary>
/// A grid of equal cells that the projectors sharing a screen are tiled into, in number order.
/// </summary>
public readonly record struct ProjectorLayout(int Rows, int Columns)
{
    /// <summary>
    /// Gets the number of cells.
    /// </summary>
    public int CellCount => Rows * Columns;


    /// <summary>
    /// Gets the grids for <paramref name="windowCount"/> windows that would lose one if a row or a column were dropped.
    /// </summary>
    public static IReadOnlyList<ProjectorLayout> GetLayouts(int windowCount)
    {
        if (windowCount < 2) return [];

        var layouts = new List<ProjectorLayout>();
        for (var rows = 1; rows <= windowCount; rows++)
        {
            for (var columns = 1; columns <= windowCount; columns++)
            {
                var layout = new ProjectorLayout(rows, columns);
                var isFitting = layout.CellCount >= windowCount;
                var hasSpareRow = (rows - 1) * columns >= windowCount;
                var hasSpareColumn = rows * (columns - 1) >= windowCount;

                if (isFitting && !hasSpareRow && !hasSpareColumn) layouts.Add(layout);
            }
        }

        // grids with no empty cell first, then the most even ones
        return layouts
            .OrderBy(l => l.CellCount - windowCount)
            .ThenBy(l => Math.Abs(l.Rows - l.Columns))
            .ThenBy(l => l.Rows)
            .ToArray();
    }


    /// <summary>
    /// Gets the bounds of <paramref name="cell"/>, counted left to right then top to bottom, so the cells tile <paramref name="area"/> exactly.
    /// </summary>
    public PixelRect GetCellBounds(PixelRect area, int cell)
    {
        var row = cell / Columns;
        var column = cell % Columns;

        // neighbor cells share each rounded edge, so no pixel is left between them
        var left = area.X + area.Width * column / Columns;
        var right = area.X + area.Width * (column + 1) / Columns;
        var top = area.Y + area.Height * row / Rows;
        var bottom = area.Y + area.Height * (row + 1) / Rows;

        return new PixelRect(left, top, right - left, bottom - top);
    }
}
