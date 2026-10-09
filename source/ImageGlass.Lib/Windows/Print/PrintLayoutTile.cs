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
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Printing;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;


/// <summary>
/// Draws a page of the current paper with the cells of a print layout on it, as the layout picker shows it.
/// </summary>
public sealed class PrintLayoutTile : PhControl
{
    private const double BOX_SIZE = 44;
    private const float CELL_RADIUS = 1.5f;

    // a layout's gutter shrinks to a hairline in a tile, so the cells get a gap of their own
    private const double CELL_GAP = 2;

    private SKSize _pageSizePt = new(595, 842);
    private IReadOnlyList<SKRect> _cellRectsPt = [];


    /// <summary>
    /// Gets the layout the tile shows.
    /// </summary>
    public PrintLayout Layout { get; }


    public PrintLayoutTile(PrintLayout layout)
    {
        Layout = layout;
        IsHitTestVisible = false;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
    }


    /// <summary>
    /// Sets the page and the cells to draw, in points.
    /// </summary>
    public void SetPage(SKSize pageSizePt, IReadOnlyList<SKRect> cellRectsPt)
    {
        _pageSizePt = pageSizePt;
        _cellRectsPt = cellRectsPt;

        InvalidateMeasure();
        InvalidateVisual();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        // a square whatever the page's orientation, so the labels of a row line up
        return new Size(BOX_SIZE, BOX_SIZE);
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var foreground = Resx.GetBrushColor(ResxId.IG_ThemeForegroundBrush, Core.Theme.InvertedBaseColor);
        var accent = Core.AccentColor;
        var border = Resx.GetBrushColor(ResxId.IG_BorderControlBrush, foreground.WithAlpha(90));

        // the shape of the page, centered in the square
        var scale = Math.Min(Bounds.Width, Bounds.Height) / Math.Max(_pageSizePt.Width, _pageSizePt.Height);
        var pageW = _pageSizePt.Width * scale;
        var pageH = _pageSizePt.Height * scale;
        var page = new Rect((Bounds.Width - pageW) / 2, (Bounds.Height - pageH) / 2, pageW, pageH).Deflate(0.5);

        // the paper, white like the preview so the tile reads as a page
        c.DrawRectangleEx(page, 2, border, Colors.White);

        var scaleX = page.Width / _pageSizePt.Width;
        var scaleY = page.Height / _pageSizePt.Height;
        foreach (var cell in _cellRectsPt)
        {
            var w = cell.Width * scaleX;
            var h = cell.Height * scaleY;
            var gapX = Math.Min(CELL_GAP, w - 1) / 2;
            var gapY = Math.Min(CELL_GAP, h - 1) / 2;

            var rect = new Rect(
                page.X + cell.Left * scaleX + Math.Max(0, gapX),
                page.Y + cell.Top * scaleY + Math.Max(0, gapY),
                Math.Max(1, w - 2 * Math.Max(0, gapX)),
                Math.Max(1, h - 2 * Math.Max(0, gapY)));

            c.DrawRectangleEx(rect, CELL_RADIUS, null, accent.WithAlpha(150));
        }
    }
}
