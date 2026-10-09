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
using Avalonia.Media.Imaging;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using SkiaSharp;
using System;

namespace ImageGlass.Common.Windows;


/// <summary>
/// Shows one page of the print preview: the paper with a soft shadow, the rendered page on it.
/// </summary>
public sealed class PrintPreviewControl : PhControl
{
    private const double PAPER_PADDING = 18;

    private Bitmap? _page;
    private SKSize _pageSizePt = new(595, 842);


    public PrintPreviewControl()
    {
        ClipToBounds = true;
    }


    /// <summary>
    /// Gets where the paper of a page of <paramref name="pageSizePt"/> is drawn, in this control's coordinates.
    /// </summary>
    public Rect GetPaperRect(SKSize pageSizePt)
    {
        var area = new Rect(Bounds.Size).Deflate(PAPER_PADDING);
        if (area.Width <= 0 || area.Height <= 0 || pageSizePt.Width <= 0 || pageSizePt.Height <= 0) return default;

        var scale = Math.Min(area.Width / pageSizePt.Width, area.Height / pageSizePt.Height);
        var w = pageSizePt.Width * scale;
        var h = pageSizePt.Height * scale;

        return new Rect(area.Center.X - w / 2, area.Center.Y - h / 2, w, h);
    }


    /// <summary>
    /// Shows a rendered page, which this control then owns; <c>null</c> keeps showing the blank paper.
    /// </summary>
    public void SetPage(Bitmap? page, SKSize pageSizePt)
    {
        var old = _page;
        _page = page;
        _pageSizePt = pageSizePt;

        InvalidateVisual();
        old?.Dispose();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        // the desk the paper lies on, a little darker or lighter than the window
        var isDark = Core.Theme.Settings.IsDarkMode;
        var desk = AppThemeColors.BgBrush.Color.NoAlpha().WithBrightness(isDark ? 0.06f : -0.06f);
        c.FillRectangle(desk.ToBrush(), new Rect(Bounds.Size));

        var paper = GetPaperRect(_pageSizePt);
        if (paper.Width <= 0 || paper.Height <= 0) return;

        // a soft shadow under the paper
        for (var i = 1; i <= 4; i++)
        {
            var shadow = paper.Translate(new Vector(0, i)).Inflate(i);
            c.FillRectangle(new SolidColorBrush(Colors.Black, isDark ? 0.10 : 0.05), shadow);
        }

        c.FillRectangle(Brushes.White, paper);
        if (_page is not null)
        {
            using (c.PushRenderOptions(new() { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality }))
            {
                c.DrawImage(_page, new Rect(_page.Size), paper);
            }
        }
    }
}
