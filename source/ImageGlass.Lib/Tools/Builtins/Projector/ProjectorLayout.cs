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
/// A grid of equal cells on a screen, and the cells the projectors on it fill, in number order.
/// </summary>
public sealed class ProjectorLayout : IEquatable<ProjectorLayout>
{
    private readonly int[] _usedCells;


    #region Public Properties

    /// <summary>
    /// Gets the text that tells layouts apart, e.g. <c>2x1:1</c> for the right half.
    /// </summary>
    public string Id { get; }


    /// <summary>
    /// Gets the number of columns.
    /// </summary>
    public int ColumnCount { get; }


    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int RowCount { get; }


    /// <summary>
    /// Gets the number of cells.
    /// </summary>
    public int CellCount => ColumnCount * RowCount;


    /// <summary>
    /// Gets the cells the projectors fill, counted left to right then top to bottom, in number order.
    /// </summary>
    public IReadOnlyList<int> UsedCells => _usedCells;

    #endregion // Public Properties



    private ProjectorLayout(int columns, int rows, int[] usedCells)
    {
        ColumnCount = columns;
        RowCount = rows;
        _usedCells = usedCells;
        Id = $"{columns}x{rows}:{string.Join(',', usedCells)}";
    }



    #region Factory Methods

    /// <summary>
    /// Gets the layouts offered for a screen of <paramref name="windowCount"/> projectors, in menu order.
    /// </summary>
    public static IReadOnlyList<ProjectorLayout> GetLayouts(int windowCount, double aspectRatio)
    {
        // a lone projector takes a half, side by side or stacked, or a quarter
        if (windowCount == 1)
        {
            return
            [
                Halves(true, 0),
                Halves(true, 1),
                Halves(false, 0),
                Halves(false, 1),
                Quarter(0),
                Quarter(1),
                Quarter(2),
                Quarter(3),
            ];
        }

        // two share the halves
        if (windowCount == 2)
        {
            return [Halves(true, 0, 1), Halves(false, 0, 1)];
        }

        if (windowCount > 2) return [GetGrid(windowCount, aspectRatio)];

        return [];
    }


    /// <summary>
    /// Gets the layout for <paramref name="windowCount"/> projectors set to tiled: the first half for one, both for two, else a grid.
    /// </summary>
    public static ProjectorLayout GetDefault(int windowCount, double aspectRatio)
    {
        var isLandscape = aspectRatio >= 1;
        if (windowCount <= 1) return Halves(isLandscape, 0);
        if (windowCount == 2) return Halves(isLandscape, 0, 1);

        return GetGrid(windowCount, aspectRatio);
    }


    /// <summary>
    /// Gets the width to height ratio of <paramref name="area"/>.
    /// </summary>
    public static double GetAspectRatio(PixelRect area)
    {
        if (area.Width <= 0 || area.Height <= 0) return 16d / 9;

        return (double)area.Width / area.Height;
    }

    #endregion // Factory Methods



    #region Public Methods

    /// <summary>
    /// Gets the bounds of <paramref name="cell"/>, counted left to right then top to bottom, so the cells tile <paramref name="area"/> exactly.
    /// </summary>
    public PixelRect GetCellBounds(PixelRect area, int cell)
    {
        var column = cell % ColumnCount;
        var row = cell / ColumnCount;

        // neighbor cells share each rounded edge, so no pixel is left between them
        var left = area.X + area.Width * column / ColumnCount;
        var right = area.X + area.Width * (column + 1) / ColumnCount;
        var top = area.Y + area.Height * row / RowCount;
        var bottom = area.Y + area.Height * (row + 1) / RowCount;

        return new PixelRect(left, top, right - left, bottom - top);
    }


    /// <summary>
    /// Gets the bounds of <paramref name="cell"/> in <paramref name="area"/>, for drawing the layout.
    /// </summary>
    public Rect GetCellBounds(Rect area, int cell)
    {
        var column = cell % ColumnCount;
        var row = cell / ColumnCount;
        var cellWidth = area.Width / ColumnCount;
        var cellHeight = area.Height / RowCount;

        return new Rect(area.X + column * cellWidth, area.Y + row * cellHeight, cellWidth, cellHeight);
    }


    /// <summary>
    /// Gets which projector, by its place in number order, fills <paramref name="cell"/>; <c>-1</c> when none does.
    /// </summary>
    public int GetProjectorIndex(int cell) => Array.IndexOf(_usedCells, cell);


    public bool Equals(ProjectorLayout? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as ProjectorLayout);
    public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Id;

    public static bool operator ==(ProjectorLayout? left, ProjectorLayout? right) => Equals(left, right);
    public static bool operator !=(ProjectorLayout? left, ProjectorLayout? right) => !Equals(left, right);

    #endregion // Public Methods



    #region Private Methods

    /// <summary>
    /// Creates the two halves of a screen, side by side or stacked, with projectors in <paramref name="usedCells"/>.
    /// </summary>
    private static ProjectorLayout Halves(bool isSideBySide, params int[] usedCells)
    {
        return isSideBySide
            ? new ProjectorLayout(2, 1, usedCells)
            : new ProjectorLayout(1, 2, usedCells);
    }


    /// <summary>
    /// Creates the four quarters of a screen, with a projector in <paramref name="cell"/>.
    /// </summary>
    private static ProjectorLayout Quarter(int cell) => new(2, 2, [cell]);


    /// <summary>
    /// Gets the smallest grid of at least 2 × 2 for <paramref name="windowCount"/> projectors; it grows along the longer side of the screen first.
    /// </summary>
    private static ProjectorLayout GetGrid(int windowCount, double aspectRatio)
    {
        var longSide = 2;
        var shortSide = 2;
        while (longSide * shortSide < windowCount)
        {
            if (longSide == shortSide) longSide++;
            else shortSide++;
        }

        var usedCells = Enumerable.Range(0, windowCount).ToArray();
        var isLandscape = aspectRatio >= 1;
        if (isLandscape) return new ProjectorLayout(longSide, shortSide, usedCells);

        return new ProjectorLayout(shortSide, longSide, usedCells);
    }

    #endregion // Private Methods

}



/// <summary>
/// The cell of a layout that a tiled projector fills.
/// </summary>
public readonly record struct ProjectorTile(ProjectorLayout Layout, int Cell);
