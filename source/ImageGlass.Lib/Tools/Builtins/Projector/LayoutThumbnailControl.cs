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
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Globalization;

namespace ImageGlass.Tools;


/// <summary>
/// Draws a screen split into the cells of a <see cref="ProjectorLayout"/>, with the projectors that would fill them.
/// </summary>
public sealed class LayoutThumbnailControl : PhControl
{
    private const double THUMBNAIL_HEIGHT = 34;
    private const double MIN_THUMBNAIL_WIDTH = 24;
    private const double MAX_THUMBNAIL_WIDTH = 80;

    private const float FRAME_RADIUS = 4;
    private const float CELL_RADIUS = 2;
    private const double FRAME_PADDING = 3;
    private const double CELL_GAP = 2;

    // a cell this small shows its fill only
    private const double MIN_LABEL_CELL_SIZE = 11;


    #region Public Properties

    /// <summary>
    /// Gets the grid to draw.
    /// </summary>
    public ProjectorLayout Layout { get; }


    /// <summary>
    /// Gets the numbers of the projectors that would fill the cells, in cell order.
    /// </summary>
    public int[] ProjectorNumbers { get; }


    /// <summary>
    /// Gets the width to height ratio of the screen.
    /// </summary>
    public double AspectRatio { get; }


    /// <summary>
    /// Gets, sets whether the projectors are tiled in this grid now.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<LayoutThumbnailControl, bool>(nameof(IsActive));

    #endregion // Public Properties



    static LayoutThumbnailControl()
    {
        AffectsRender<LayoutThumbnailControl>(IsActiveProperty);
    }


    public LayoutThumbnailControl(ProjectorLayout layout, int[] projectorNumbers, double aspectRatio)
    {
        Layout = layout;
        ProjectorNumbers = projectorNumbers;
        AspectRatio = double.IsFinite(aspectRatio) && aspectRatio > 0 ? aspectRatio : 16d / 9;
        IsHitTestVisible = false;
    }



    #region Layout & Render

    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        // take the shape of the screen
        var width = Math.Clamp(THUMBNAIL_HEIGHT * AspectRatio, MIN_THUMBNAIL_WIDTH, MAX_THUMBNAIL_WIDTH);

        return new Size(width, THUMBNAIL_HEIGHT);
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var accent = Core.AccentColor;
        var foreground = Core.Theme.InvertedBaseColor;


        // 1. the screen
        var frame = new Rect(Bounds.Size).Deflate(0.5);
        var frameBorder = IsActive ? accent.WithAlpha(230) : foreground.WithAlpha(90);
        c.DrawRectangleEx(frame, FRAME_RADIUS, frameBorder, foreground.WithAlpha(14));


        // 2. the cells, filled in the order the projectors are tiled
        var area = frame.Deflate(FRAME_PADDING);
        var cellWidth = (area.Width - CELL_GAP * (Layout.Columns - 1)) / Layout.Columns;
        var cellHeight = (area.Height - CELL_GAP * (Layout.Rows - 1)) / Layout.Rows;
        if (cellWidth <= 0 || cellHeight <= 0) return;

        for (var cell = 0; cell < Layout.CellCount; cell++)
        {
            var row = cell / Layout.Columns;
            var column = cell % Layout.Columns;
            var cellRect = new Rect(
                area.X + column * (cellWidth + CELL_GAP),
                area.Y + row * (cellHeight + CELL_GAP),
                cellWidth,
                cellHeight);

            if (cell < ProjectorNumbers.Length)
            {
                DrawFilledCell(c, cellRect, ProjectorNumbers[cell], accent, foreground);
            }
            else
            {
                DrawEmptyCell(c, cellRect, foreground);
            }
        }
    }


    /// <summary>
    /// Draws a cell a projector would fill, with its number where it fits.
    /// </summary>
    private void DrawFilledCell(DrawingContext c, Rect cellRect, int projectorNumber, Color accent, Color foreground)
    {
        var fill = IsActive ? accent : foreground.WithAlpha(70);
        c.DrawRectangleEx(cellRect, CELL_RADIUS, null, fill);

        var hasRoom = cellRect.Width >= MIN_LABEL_CELL_SIZE && cellRect.Height >= MIN_LABEL_CELL_SIZE;
        if (!hasRoom) return;

        var label = projectorNumber.ToString(CultureInfo.InvariantCulture);
        var fontSize = Math.Min(Const.FONT_SIZE_SMALL - 3, cellRect.Height - 1);
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize, true);
        var labelColor = IsActive ? accent.InvertBlackOrWhite() : foreground.WithAlpha(230);

        c.DrawTextEx(label, FontFamily, fontSize,
            cellRect.Center.X - labelSize.Width / 2,
            cellRect.Center.Y - labelSize.Height / 2,
            labelColor, isBold: true);
    }


    /// <summary>
    /// Draws a cell no projector would fill, as a dashed outline.
    /// </summary>
    private static void DrawEmptyCell(DrawingContext c, Rect cellRect, Color foreground)
    {
        var pen = new Pen(foreground.WithAlpha(80).ToBrush(), 1, DashStyle.Dash);
        var outline = new RoundedRect(cellRect.Deflate(0.5), CELL_RADIUS);

        c.DrawRectangle(null, pen, outline);
    }

    #endregion // Layout & Render

}
