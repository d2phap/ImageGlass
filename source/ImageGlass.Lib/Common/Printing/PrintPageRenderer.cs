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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

    // a raster print stays sharp from 150 dpi, and photo printers take no more than about 300
    private const float MIN_RASTER_DPI = 150;
    private const float MAX_PHOTO_RASTER_DPI = 300;

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
    /// Gets the resolution a raster backend draws a cell at: the image's own density, kept between a floor and <paramref name="maxDpi"/>.
    /// </summary>
    public static async Task<float> GetCellRasterDpiAsync(PrintCell cell, IPrintImageSource images,
        PrintRenderOptions options, float maxDpi, CancellationToken token)
    {
        // a vector print has no pixels of its own
        var picture = images.GetPicture(cell.ItemIndex);
        if (picture is not null && !picture.IsDisposed()) return maxDpi;

        using var lease = await images.GetImageAsync(cell.ItemIndex, GetRequestLongSide(cell, options), token).ConfigureAwait(false);
        var img = lease?.Image;
        var ceiling = Math.Min(maxDpi, MAX_PHOTO_RASTER_DPI);
        if (img.IsDisposed()) return ceiling;

        var (dest, _) = PrintLayoutEngine.PlaceImage(cell.ImageRectPt, new SKSize(img.Width, img.Height), options.Fit, options.AutoRotate, options.ImageDpi);
        var destLongSidePt = Math.Max(dest.Width, dest.Height);
        if (destLongSidePt <= 0) return ceiling;

        var imageDpi = Math.Max(img.Width, img.Height) / destLongSidePt * PrintUnits.POINTS_PER_INCH;
        return Math.Clamp(imageDpi, Math.Min(MIN_RASTER_DPI, ceiling), ceiling);
    }


    /// <summary>
    /// Gets the length of the long side of the image a cell asks for: the pixels it covers at the target resolution, every pixel for actual size.
    /// </summary>
    private static int GetRequestLongSide(PrintCell cell, PrintRenderOptions options)
    {
        if (options.Fit == PrintFitMode.ActualSize) return int.MaxValue;

        var longSidePt = Math.Max(cell.ImageRectPt.Width, cell.ImageRectPt.Height);
        return (int)Math.Ceiling(PrintUnits.PtToPx(longSidePt, options.TargetDpi) * (options.Fit == PrintFitMode.Fill ? 1.5f : 1f));
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

        var longSide = GetRequestLongSide(cell, options);
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
    /// Maps content of <paramref name="size"/> at the origin onto <paramref name="dest"/>, turned a quarter counter-clockwise when asked, as printing turns landscape.
    /// </summary>
    private static void ApplyPlacement(SKCanvas canvas, SKRect dest, bool rotate, SKSize size)
    {
        var drawW = rotate ? dest.Height : dest.Width;
        var drawH = rotate ? dest.Width : dest.Height;

        canvas.Translate(dest.MidX, dest.MidY);
        if (rotate) canvas.RotateDegrees(-90);
        canvas.Translate(-drawW / 2, -drawH / 2);
        canvas.Scale(drawW / size.Width, drawH / size.Height);
    }


    /// <summary>
    /// Draws the caption of a print centered in its band, each script in a font that has it, cut short with an ellipsis when too long.
    /// </summary>
    private static void DrawCaption(SKCanvas canvas, SKRect rect, string text)
    {
        if (string.IsNullOrEmpty(text) || rect.Width <= 0) return;

        using var fonts = new CaptionFonts();
        var runs = fonts.Split(Ellipsize(fonts, text, rect.Width - 4));
        var width = runs.Sum(i => i.Font.MeasureText(i.Text));

        using var paint = new SKPaint { Color = new SKColor(0x40, 0x40, 0x40), IsAntialias = true };
        var x = rect.MidX - width / 2;
        var y = rect.MidY + CAPTION_FONT_PT * 0.35f;

        // as outlines, a PDF embeds no font: the whole font would otherwise weigh more than the photos
        foreach (var (runText, font) in runs)
        {
            using var path = font.GetTextPath(runText, new SKPoint(x, y));
            canvas.DrawPath(path, paint);
            x += font.MeasureText(runText);
        }
    }


    /// <summary>
    /// Cuts the text at the longest start of whole characters that fits with an ellipsis.
    /// </summary>
    private static string Ellipsize(CaptionFonts fonts, string text, float maxWidth)
    {
        if (fonts.Measure(text) <= maxWidth) return text;

        // the starts of whole characters, so neither a surrogate pair nor a combining mark is cut
        var starts = StringInfo.ParseCombiningCharacters(text);
        var low = 0;
        var high = starts.Length - 1;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if (fonts.Measure(text[..starts[mid]] + "…") <= maxWidth) low = mid;
            else high = mid - 1;
        }

        return low == 0 ? "…" : text[..starts[low]] + "…";
    }


    /// <summary>
    /// The fonts of a caption: the default one, and for a character it lacks, a font of the system that has it.
    /// </summary>
    private sealed class CaptionFonts : IDisposable
    {
        private readonly SKFont _default = new(SKTypeface.Default, CAPTION_FONT_PT);
        private readonly List<(SKTypeface Typeface, SKFont Font)> _fallbacks = [];


        /// <summary>
        /// Splits the text into runs that each draw in one font.
        /// </summary>
        public List<(string Text, SKFont Font)> Split(string text)
        {
            var runs = new List<(string, SKFont)>();
            var run = new StringBuilder();
            SKFont? runFont = null;

            foreach (var rune in text.EnumerateRunes())
            {
                var font = GetFont(rune.Value);
                if (runFont is not null && !ReferenceEquals(font, runFont))
                {
                    runs.Add((run.ToString(), runFont));
                    run.Clear();
                }

                runFont = font;
                run.Append(rune.ToString());
            }

            if (runFont is not null && run.Length > 0) runs.Add((run.ToString(), runFont));
            return runs;
        }


        public float Measure(string text) => Split(text).Sum(i => i.Font.MeasureText(i.Text));


        private SKFont GetFont(int codepoint)
        {
            if (_default.GetGlyph(codepoint) != 0) return _default;

            foreach (var (_, font) in _fallbacks)
            {
                if (font.GetGlyph(codepoint) != 0) return font;
            }

            // a character no font has stays in the default one, as a box
            var typeface = SKFontManager.Default.MatchCharacter(codepoint);
            if (typeface is null) return _default;

            var fallback = new SKFont(typeface, CAPTION_FONT_PT);
            _fallbacks.Add((typeface, fallback));
            return fallback;
        }


        public void Dispose()
        {
            _default.Dispose();
            foreach (var (typeface, font) in _fallbacks)
            {
                font.Dispose();
                typeface.Dispose();
            }
        }
    }


    /// <summary>
    /// Hatches the edges of the paper outside the printable area.
    /// </summary>
    private static void DrawUnprintableBand(SKCanvas canvas, SKRect page, SKRect printable)
    {
        if (printable.Contains(page) || printable.IsEmpty) return;

        using var paint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 28),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        canvas.Save();
        canvas.ClipRect(page);
        canvas.ClipRect(printable, SKClipOperation.Difference);
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
