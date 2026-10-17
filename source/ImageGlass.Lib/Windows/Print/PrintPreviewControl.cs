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
    // room for the paper's shadow, which blurs past its edge and falls a little lower
    private const double PAPER_PADDING = 32;

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


    /// <summary>
    /// Gets the color of the desk the paper lies on, a little darker or lighter than the window; the preview's pane paints it.
    /// </summary>
    public static Color GetDeskColor()
    {
        var isDark = Core.Theme.Settings.IsDarkMode;
        return AppThemeColors.BgBrush.Color.NoAlpha().WithBrightness(isDark ? 0.06f : -0.06f);
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var paper = GetPaperRect(_pageSizePt);
        if (paper.Width <= 0 || paper.Height <= 0) return;

        // the paper lifted off the desk: a close contact shadow and a wide soft one
        var isDark = Core.Theme.Settings.IsDarkMode;
        var shadows = new BoxShadows(
            new BoxShadow { OffsetY = 1, Blur = 3, Color = Color.FromArgb((byte)(isDark ? 90 : 40), 0, 0, 0) },
            [new BoxShadow { OffsetY = 6, Blur = 20, Color = Color.FromArgb((byte)(isDark ? 110 : 45), 0, 0, 0) }]);

        c.DrawRectangle(Brushes.White, null, new RoundedRect(paper), shadows);
        if (_page is not null)
        {
            using (c.PushRenderOptions(new() { BitmapInterpolationMode = BitmapInterpolationMode.HighQuality }))
            {
                c.DrawImage(_page, new Rect(_page.Size), paper);
            }
        }
    }
}
