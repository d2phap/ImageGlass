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
using System.Globalization;
using System.Linq;

namespace ImageGlass.Tools;


/// <summary>
/// A grid whose columns and rows take weighted shares of a screen; the projectors on it fill its cells in number order.
/// </summary>
public sealed class ProjectorLayout : IEquatable<ProjectorLayout>
{
    // the most columns or rows, and the largest share, a saved layout may have
    private const int MAX_DIVISIONS = 8;
    private const int MAX_WEIGHT = 12;

    // where each column and row starts, in shares, ending with the total
    private readonly int[] _columnEdges;
    private readonly int[] _rowEdges;


    #region Public Properties

    /// <summary>
    /// Gets the text that identifies and saves the layout: the column shares, then the row shares, e.g. <c>2:1|1</c>.
    /// </summary>
    public string Id { get; }


    /// <summary>
    /// Gets the number of columns.
    /// </summary>
    public int ColumnCount => _columnEdges.Length - 1;


    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int RowCount => _rowEdges.Length - 1;


    /// <summary>
    /// Gets the number of cells.
    /// </summary>
    public int CellCount => ColumnCount * RowCount;

    #endregion // Public Properties



    private ProjectorLayout(int[] columnWeights, int[] rowWeights)
    {
        _columnEdges = GetEdges(columnWeights);
        _rowEdges = GetEdges(rowWeights);
        Id = $"{string.Join(':', columnWeights)}|{string.Join(':', rowWeights)}";
    }



    #region Factory Methods

    /// <summary>
    /// Creates a grid of equal cells.
    /// </summary>
    public static ProjectorLayout Grid(int columns, int rows)
    {
        var columnWeights = Enumerable.Repeat(1, columns).ToArray();
        var rowWeights = Enumerable.Repeat(1, rows).ToArray();

        return new ProjectorLayout(columnWeights, rowWeights);
    }


    /// <summary>
    /// Creates columns side by side, each taking its share of the width.
    /// </summary>
    public static ProjectorLayout Columns(params int[] weights) => new(weights, [1]);


    /// <summary>
    /// Creates rows one above another, each taking its share of the height.
    /// </summary>
    public static ProjectorLayout Rows(params int[] weights) => new([1], weights);


    /// <summary>
    /// Reads a layout from its <see cref="Id"/>; <c>null</c> when the text is not one.
    /// </summary>
    public static ProjectorLayout? Parse(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        var parts = id.Split('|');
        if (parts.Length != 2) return null;

        var columnWeights = ParseWeights(parts[0]);
        var rowWeights = ParseWeights(parts[1]);
        if (columnWeights is null || rowWeights is null) return null;

        return new ProjectorLayout(columnWeights, rowWeights);
    }


    /// <summary>
    /// Gets the layouts offered for a screen of <paramref name="windowCount"/> projectors, in menu order; only ones they all fit in.
    /// </summary>
    public static IReadOnlyList<ProjectorLayout> GetLayouts(int windowCount, double aspectRatio)
    {
        if (windowCount < 1) return [];

        // 1. a lone projector can fill the whole screen
        var layouts = new List<ProjectorLayout>();
        if (windowCount == 1) layouts.Add(Grid(1, 1));

        // 2. halves, then two thirds and one third, side by side and stacked
        if (windowCount <= 2)
        {
            layouts.Add(Columns(1, 1));
            layouts.Add(Rows(1, 1));
            layouts.Add(Columns(2, 1));
            layouts.Add(Columns(1, 2));
            layouts.Add(Rows(2, 1));
            layouts.Add(Rows(1, 2));
        }

        // 3. a grid for any number
        layouts.Add(GetGrid(windowCount, aspectRatio));

        return layouts;
    }


    /// <summary>
    /// Gets the layout for <paramref name="windowCount"/> projectors set to tiled: the whole screen for one, halves for two, else a grid.
    /// </summary>
    public static ProjectorLayout GetDefault(int windowCount, double aspectRatio)
    {
        if (windowCount <= 1) return Grid(1, 1);
        if (windowCount == 2) return aspectRatio >= 1 ? Columns(1, 1) : Rows(1, 1);

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
        var columnTotal = _columnEdges[^1];
        var rowTotal = _rowEdges[^1];

        // neighbor cells share each rounded edge, so no pixel is left between them
        var left = area.X + area.Width * _columnEdges[column] / columnTotal;
        var right = area.X + area.Width * _columnEdges[column + 1] / columnTotal;
        var top = area.Y + area.Height * _rowEdges[row] / rowTotal;
        var bottom = area.Y + area.Height * _rowEdges[row + 1] / rowTotal;

        return new PixelRect(left, top, right - left, bottom - top);
    }


    /// <summary>
    /// Gets the bounds of <paramref name="cell"/> in <paramref name="area"/>, for drawing the layout.
    /// </summary>
    public Rect GetCellBounds(Rect area, int cell)
    {
        var column = cell % ColumnCount;
        var row = cell / ColumnCount;
        var columnTotal = (double)_columnEdges[^1];
        var rowTotal = (double)_rowEdges[^1];

        var left = area.X + area.Width * _columnEdges[column] / columnTotal;
        var right = area.X + area.Width * _columnEdges[column + 1] / columnTotal;
        var top = area.Y + area.Height * _rowEdges[row] / rowTotal;
        var bottom = area.Y + area.Height * _rowEdges[row + 1] / rowTotal;

        return new Rect(left, top, right - left, bottom - top);
    }


    public bool Equals(ProjectorLayout? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as ProjectorLayout);
    public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Id;

    public static bool operator ==(ProjectorLayout? left, ProjectorLayout? right) => Equals(left, right);
    public static bool operator !=(ProjectorLayout? left, ProjectorLayout? right) => !Equals(left, right);

    #endregion // Public Methods



    #region Private Methods

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

        var isLandscape = aspectRatio >= 1;
        if (isLandscape) return Grid(longSide, shortSide);

        return Grid(shortSide, longSide);
    }


    /// <summary>
    /// Gets where each share starts, ending with their total.
    /// </summary>
    private static int[] GetEdges(int[] weights)
    {
        var edges = new int[weights.Length + 1];
        for (var i = 0; i < weights.Length; i++)
        {
            edges[i + 1] = edges[i] + weights[i];
        }

        return edges;
    }


    /// <summary>
    /// Reads shares such as <c>2:1</c>; <c>null</c> when any is not a whole number a layout could use.
    /// </summary>
    private static int[]? ParseWeights(string text)
    {
        var items = text.Split(':');
        if (items.Length is < 1 or > MAX_DIVISIONS) return null;

        var weights = new int[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            var isNumber = int.TryParse(items[i], NumberStyles.None, CultureInfo.InvariantCulture, out var weight);
            if (!isNumber || weight is < 1 or > MAX_WEIGHT) return null;

            weights[i] = weight;
        }

        return weights;
    }

    #endregion // Private Methods

}



/// <summary>
/// The cell of a layout that a tiled projector fills.
/// </summary>
public readonly record struct ProjectorTile(ProjectorLayout Layout, int Cell);
