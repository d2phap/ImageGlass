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
using ImageGlass.Common.Extensions;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Printing;


/// <summary>
/// How the renderer draws a page.
/// </summary>
public sealed record PrintRenderOptions
{
    public PrintFitMode Fit { get; init; } = PrintFitMode.Fit;
    public bool AutoRotate { get; init; } = true;
    public PrintColorMode ColorMode { get; init; } = PrintColorMode.Color;

    /// <summary>
    /// Gets the resolution images are requested at, in pixels per inch of paper.
    /// </summary>
    public float TargetDpi { get; init; } = 300;

    /// <summary>
    /// Gets the resolution of the image itself, which <see cref="PrintFitMode.ActualSize"/> prints at.
    /// </summary>
    public float ImageDpi { get; init; } = 96;

    /// <summary>
    /// Gets whether this draws the on-screen preview: cached images and placeholders, never a wait, plus the printable band.
    /// </summary>
    public bool IsPreview { get; init; }
}


/// <summary>
/// Draws a page of a layout in points onto any canvas: the preview, a PDF page, or a band of a printer's page.
/// </summary>
public static class PrintPageRenderer
{
    private const float CAPTION_FONT_PT = 7.5f;

    // Rec. 709 luminance, the same weights the HDR code uses
    private static readonly float[] _grayscaleMatrix =
    [
        0.2126f, 0.7152f, 0.0722f, 0, 0,
        0.2126f, 0.7152f, 0.0722f, 0, 0,
        0.2126f, 0.7152f, 0.0722f, 0, 0,
        0, 0, 0, 1, 0,
    ];


    /// <summary>
    /// Draws one page; the caller sets the canvas matrix and clip, and prints outside the clip are skipped.
    /// </summary>
    public static async Task DrawPageAsync(SKCanvas canvas, PrintDocumentLayout layout, int pageIndex,
        IPrintImageSource images, PrintRenderOptions options, CancellationToken token)
    {
        if (pageIndex < 0 || pageIndex >= layout.Pages.Count) return;

        var page = layout.Pages[pageIndex];
        var pageRect = SKRect.Create(layout.PageSizePt);

        // 1. paper; printers composite transparent images on it too
        using (var paper = new SKPaint { Color = SKColors.White })
        {
            canvas.DrawRect(pageRect, paper);
        }

        // 2. what only the preview shows: the band the printer cannot print on, and the empty cells
        if (options.IsPreview)
        {
            DrawUnprintableBand(canvas, pageRect, layout.PrintableRectPt);
            DrawEmptyCells(canvas, layout, page);
        }

        // 3. the prints
        using var filter = options.ColorMode == PrintColorMode.Grayscale ? SKColorFilter.CreateColorMatrix(_grayscaleMatrix) : null;
        var clip = canvas.LocalClipBounds;

        foreach (var cell in page.Cells)
        {
            token.ThrowIfCancellationRequested();
            if (!cell.RectPt.IntersectsWith(clip)) continue;

            await DrawCellAsync(canvas, cell, images, options, filter, token).ConfigureAwait(false);
            if (cell.CaptionRectPt is { } captionRect) DrawCaption(canvas, captionRect, images.GetCaption(cell.ItemIndex));
        }
    }


    /// <summary>
    /// Draws the image of one print, placed and clipped to its cell.
    /// </summary>
    private static async Task DrawCellAsync(SKCanvas canvas, PrintCell cell, IPrintImageSource images,
        PrintRenderOptions options, SKColorFilter? filter, CancellationToken token)
    {
        using var paint = new SKPaint { ColorFilter = filter };

        // a vector print stays vector, crisp at any resolution
        var picture = images.GetPicture(cell.ItemIndex);
        if (picture is not null && !picture.IsDisposed())
        {
            var size = picture.CullRect.Size;
            var (dest, rotate) = PrintLayoutEngine.PlaceImage(cell.ImageRectPt, size, options.Fit, options.AutoRotate, 96);

            canvas.Save();
            canvas.ClipRect(cell.ImageRectPt);
            ApplyPlacement(canvas, dest, rotate, size);
            canvas.Translate(-picture.CullRect.Left, -picture.CullRect.Top);
            canvas.DrawPicture(picture, paint);
            canvas.Restore();
            return;
        }

        // the pixels the print covers at the target resolution; actual size asks for every pixel
        var longSidePt = Math.Max(cell.ImageRectPt.Width, cell.ImageRectPt.Height);
        var longSide = options.Fit == PrintFitMode.ActualSize
            ? int.MaxValue
            : (int)Math.Ceiling(PrintUnits.PtToPx(longSidePt, options.TargetDpi) * (options.Fit == PrintFitMode.Fill ? 1.5f : 1f));

        using var lease = options.IsPreview
            ? images.TryGetImage(cell.ItemIndex, longSide)
            : await images.GetImageAsync(cell.ItemIndex, longSide, token).ConfigureAwait(false);

        var img = lease?.Image;
        if (img.IsDisposed())
        {
            if (options.IsPreview) DrawPlaceholder(canvas, cell.ImageRectPt);
            return;
        }

        var imageSize = new SKSize(img.Width, img.Height);
        var (imageDest, turn) = PrintLayoutEngine.PlaceImage(cell.ImageRectPt, imageSize, options.Fit, options.AutoRotate, options.ImageDpi);

        canvas.Save();
        canvas.ClipRect(cell.ImageRectPt);
        ApplyPlacement(canvas, imageDest, turn, imageSize);
        canvas.DrawImage(img, SKRect.Create(imageSize), new SKSamplingOptions(SKCubicResampler.Mitchell), paint);
        canvas.Restore();
    }


    /// <summary>
    /// Maps content of <paramref name="size"/> at the origin onto <paramref name="dest"/>, turned 90 degrees when asked.
    /// </summary>
    private static void ApplyPlacement(SKCanvas canvas, SKRect dest, bool rotate, SKSize size)
    {
        var drawW = rotate ? dest.Height : dest.Width;
        var drawH = rotate ? dest.Width : dest.Height;

        canvas.Translate(dest.MidX, dest.MidY);
        if (rotate) canvas.RotateDegrees(90);
        canvas.Translate(-drawW / 2, -drawH / 2);
        canvas.Scale(drawW / size.Width, drawH / size.Height);
    }


    /// <summary>
    /// Draws the caption of a print centered in its band, cut short with an ellipsis when too long.
    /// </summary>
    private static void DrawCaption(SKCanvas canvas, SKRect rect, string text)
    {
        if (string.IsNullOrEmpty(text) || rect.Width <= 0) return;

        // a name in a script the default font lacks falls back to a font that has it
        var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, CAPTION_FONT_PT);
        if (!font.ContainsGlyphs(text))
        {
            var missing = FindMissingCodepoint(font, text);
            if (missing > 0 && SKFontManager.Default.MatchCharacter(missing) is { } fallback) font.Typeface = fallback;
        }

        var caption = Ellipsize(font, text, rect.Width - 4);
        using var paint = new SKPaint { Color = new SKColor(0x40, 0x40, 0x40), IsAntialias = true };
        var origin = new SKPoint(rect.MidX - font.MeasureText(caption) / 2, rect.MidY + CAPTION_FONT_PT * 0.35f);

        // as outlines, a PDF embeds no font: the whole font would otherwise weigh more than the photos
        using var path = font.GetTextPath(caption, origin);
        canvas.DrawPath(path, paint);
    }


    private static int FindMissingCodepoint(SKFont font, string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var codepoint = char.ConvertToUtf32(text, i);
            if (char.IsSurrogatePair(text, i)) i++;
            if (font.GetGlyph(codepoint) == 0) return codepoint;
        }

        return 0;
    }


    private static string Ellipsize(SKFont font, string text, float maxWidth)
    {
        if (font.MeasureText(text) <= maxWidth) return text;

        // the longest start that still fits with the ellipsis
        var low = 0;
        var high = text.Length;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if (font.MeasureText(text[..mid] + "…") <= maxWidth) low = mid;
            else high = mid - 1;
        }

        return low == 0 ? "…" : text[..low] + "…";
    }


    /// <summary>
    /// Hatches the edges of the paper outside the printable area.
    /// </summary>
    private static void DrawUnprintableBand(SKCanvas canvas, SKRect page, SKRect printable)
    {
        if (printable.Contains(page) || printable.IsEmpty) return;

        using var path = new SKPath { FillType = SKPathFillType.EvenOdd };
        path.AddRect(page);
        path.AddRect(printable);

        using var paint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 28),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        canvas.Save();
        canvas.ClipPath(path);
        for (var x = page.Left - page.Height; x < page.Right; x += 6)
        {
            canvas.DrawLine(x, page.Bottom, x + page.Height, page.Top, paint);
        }
        canvas.Restore();
    }


    /// <summary>
    /// Outlines the cells of a page that hold no print, so the layout reads at a glance.
    /// </summary>
    private static void DrawEmptyCells(SKCanvas canvas, PrintDocumentLayout layout, PrintPage page)
    {
        if (page.Cells.Count >= layout.CellRectsPt.Count) return;

        using var dash = SKPathEffect.CreateDash([4, 3], 0);
        using var paint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 60),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 0.75f,
            PathEffect = dash,
            IsAntialias = true,
        };

        for (var i = page.Cells.Count; i < layout.CellRectsPt.Count; i++)
        {
            canvas.DrawRect(layout.CellRectsPt[i], paint);
        }
    }


    /// <summary>
    /// Draws the stand-in of an image still decoding.
    /// </summary>
    private static void DrawPlaceholder(SKCanvas canvas, SKRect rect)
    {
        using var paint = new SKPaint { Color = new SKColor(0xE8, 0xE8, 0xE8) };
        canvas.DrawRect(rect, paint);
    }
}
