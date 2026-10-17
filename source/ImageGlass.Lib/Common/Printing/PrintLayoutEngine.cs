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
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Common.Printing;


/// <summary>
/// What the layout engine paginates.
/// </summary>
public sealed record PrintLayoutInput
{
    public required PrintLayout Layout { get; init; }
    public required PaperInfo Paper { get; init; }
    public required int ItemCount { get; init; }

    /// <summary>
    /// Gets the width over height of the first print, which picks the page orientation of an automatic full page.
    /// </summary>
    public float FirstItemAspect { get; init; } = 1;

    public PrintOrientation Orientation { get; init; } = PrintOrientation.Auto;
    public PrintMarginPreset Margins { get; init; } = PrintMarginPreset.Normal;
    public int PrintsPerItem { get; init; } = 1;
    public bool FillPage { get; init; }
    public bool ShowCaptions { get; init; }
}


/// <summary>
/// One print on a page: the rectangle of its image and of its caption, in points.
/// </summary>
public sealed record PrintCell(SKRect RectPt, SKRect ImageRectPt, SKRect? CaptionRectPt, int ItemIndex);


/// <summary>
/// The prints of one page.
/// </summary>
public sealed record PrintPage(IReadOnlyList<PrintCell> Cells);


/// <summary>
/// The pages of a document, laid out in points on the oriented paper.
/// </summary>
public sealed record PrintDocumentLayout
{
    public required SKSize PageSizePt { get; init; }
    public required bool IsLandscape { get; init; }

    /// <summary>
    /// Gets the part of the page the printer can print on.
    /// </summary>
    public required SKRect PrintableRectPt { get; init; }

    /// <summary>
    /// Gets the part of the page inside the margins, where prints go.
    /// </summary>
    public required SKRect ContentRectPt { get; init; }

    public required IReadOnlyList<PrintPage> Pages { get; init; }

    /// <summary>
    /// Gets the rectangles of every cell of a page, used or not.
    /// </summary>
    public required IReadOnlyList<SKRect> CellRectsPt { get; init; }

    public int CellsPerPage => CellRectsPt.Count;

    /// <summary>
    /// Gets whether a fixed print size did not fit the page and was scaled down.
    /// </summary>
    public bool IsPrintShrunk { get; init; }

    /// <summary>
    /// Gets the number of prints across all pages.
    /// </summary>
    public int PrintCount => Pages.Sum(i => i.Cells.Count);
}


/// <summary>
/// Paginates prints onto pages; pure and deterministic, so the preview and the output agree.
/// </summary>
public static class PrintLayoutEngine
{
    /// <summary>
    /// The space between prints, 1/8 in.
    /// </summary>
    public const float GUTTER_PT = 9;

    /// <summary>
    /// The space between the prints of a contact sheet.
    /// </summary>
    public const float CONTACT_GUTTER_PT = 6;

    /// <summary>
    /// The height of the band a caption takes under its print.
    /// </summary>
    public const float CAPTION_BAND_PT = 13;


    /// <summary>
    /// Gets the margin of a preset, in points.
    /// </summary>
    public static float GetMarginPt(PrintMarginPreset preset) => preset switch
    {
        PrintMarginPreset.None => 0,
        PrintMarginPreset.Narrow => PrintUnits.InToPt(0.25),
        PrintMarginPreset.Wide => PrintUnits.InToPt(1),
        _ => PrintUnits.InToPt(0.5),
    };


    /// <summary>
    /// Lays the prints out on pages.
    /// </summary>
    public static PrintDocumentLayout Paginate(PrintLayoutInput input)
    {
        var isLandscape = ResolveLandscape(input);
        var (pageSize, printable, content) = GetPageRects(input.Paper, input.Margins, isLandscape);
        var cells = ComputeCellRects(input.Layout, content, input.FirstItemAspect, out var isShrunk);

        // a contact sheet always names its prints
        var showCaptions = input.ShowCaptions || input.Layout.Kind == PrintLayoutKind.ContactSheet;
        var pages = AssignItems(input, cells, showCaptions);

        return new PrintDocumentLayout
        {
            PageSizePt = pageSize,
            IsLandscape = isLandscape,
            PrintableRectPt = printable,
            ContentRectPt = content,
            Pages = pages,
            CellRectsPt = cells,
            IsPrintShrunk = isShrunk,
        };
    }


    /// <summary>
    /// Counts the prints of a layout on one page, as the layout picker shows them.
    /// </summary>
    public static int CountCellsPerPage(PrintLayout layout, PaperInfo paper, PrintMarginPreset margins, PrintOrientation orientation)
    {
        var input = new PrintLayoutInput { Layout = layout, Paper = paper, ItemCount = 1, Margins = margins, Orientation = orientation };
        var (_, _, content) = GetPageRects(paper, margins, ResolveLandscape(input));

        return ComputeCellRects(layout, content, 1, out _).Count;
    }


    /// <summary>
    /// Places an image of <paramref name="imageSize"/> pixels at <paramref name="imageDpi"/> in a cell: its rectangle in points, and whether it turns 90 degrees.
    /// </summary>
    public static (SKRect Dest, bool Rotate90) PlaceImage(SKRect cell, SKSize imageSize, PrintFitMode fit, bool autoRotate, float imageDpi)
    {
        if (cell.IsEmpty || imageSize.Width <= 0 || imageSize.Height <= 0) return (SKRect.Empty, false);

        // turn a landscape print in a portrait cell, and the reverse, when they are not square
        var rotate = autoRotate
            && imageSize.Width != imageSize.Height
            && cell.Width != cell.Height
            && imageSize.Width > imageSize.Height != cell.Width > cell.Height;
        var size = rotate ? new SKSize(imageSize.Height, imageSize.Width) : imageSize;

        var scale = fit switch
        {
            PrintFitMode.Fill => Math.Max(cell.Width / size.Width, cell.Height / size.Height),
            PrintFitMode.ActualSize => PrintUnits.POINTS_PER_INCH / Math.Max(1, imageDpi),
            _ => Math.Min(cell.Width / size.Width, cell.Height / size.Height),
        };

        var w = size.Width * scale;
        var h = size.Height * scale;
        var dest = SKRect.Create(cell.MidX - w / 2, cell.MidY - h / 2, w, h);

        return (dest, rotate);
    }


    /// <summary>
    /// Picks the page orientation: the user's choice, or for an automatic one what suits the prints best.
    /// </summary>
    private static bool ResolveLandscape(PrintLayoutInput input)
    {
        if (input.Orientation != PrintOrientation.Auto)
        {
            return input.Orientation == PrintOrientation.Landscape;
        }

        // a full page follows the print
        if (input.Layout.Kind == PrintLayoutKind.FullPage) return input.FirstItemAspect > 1;

        var (_, _, portraitContent) = GetPageRects(input.Paper, input.Margins, false);
        var (_, _, landscapeContent) = GetPageRects(input.Paper, input.Margins, true);
        var portraitCells = ComputeCellRects(input.Layout, portraitContent, input.FirstItemAspect, out _);
        var landscapeCells = ComputeCellRects(input.Layout, landscapeContent, input.FirstItemAspect, out _);

        // more prints per page wins, then more paper for each print, then more prints that need no turn
        if (portraitCells.Count != landscapeCells.Count) return landscapeCells.Count > portraitCells.Count;

        var landscapeArea = GetFittedArea(landscapeCells, input.FirstItemAspect);
        var portraitArea = GetFittedArea(portraitCells, input.FirstItemAspect);
        if (Math.Abs(landscapeArea - portraitArea) > Math.Max(landscapeArea, portraitArea) * 0.01f) return landscapeArea > portraitArea;

        return CountUpright(landscapeCells, input.FirstItemAspect) > CountUpright(portraitCells, input.FirstItemAspect);
    }


    /// <summary>
    /// Gets the oriented page size, its printable part and its content part inside the margins.
    /// </summary>
    private static (SKSize Page, SKRect Printable, SKRect Content) GetPageRects(PaperInfo paper, PrintMarginPreset preset, bool isLandscape)
    {
        var page = isLandscape
            ? new SKSize(Math.Max(paper.SizePt.Width, paper.SizePt.Height), Math.Min(paper.SizePt.Width, paper.SizePt.Height))
            : new SKSize(Math.Min(paper.SizePt.Width, paper.SizePt.Height), Math.Max(paper.SizePt.Width, paper.SizePt.Height));

        var hardware = isLandscape ? paper.HardwareMarginsPt.ToLandscape() : paper.HardwareMarginsPt;
        var userMargin = GetMarginPt(preset);
        var effective = hardware.Max(new PrintMargins(userMargin, userMargin, userMargin, userMargin));

        return (page, Inset(page, hardware), Inset(page, effective));
    }


    private static SKRect Inset(SKSize page, PrintMargins m)
    {
        var right = Math.Max(m.Left, page.Width - m.Right);
        var bottom = Math.Max(m.Top, page.Height - m.Bottom);

        return new SKRect(m.Left, m.Top, right, bottom);
    }


    /// <summary>
    /// Computes the cell rectangles of one page of a layout.
    /// </summary>
    private static List<SKRect> ComputeCellRects(PrintLayout layout, SKRect content, float itemAspect, out bool isShrunk)
    {
        isShrunk = false;
        if (content.Width <= 0 || content.Height <= 0) return [];

        switch (layout.Kind)
        {
            case PrintLayoutKind.Grid:
                var (rows, cols) = PickGridShape(layout.CellCount, content, itemAspect);
                return MakeGrid(content, cols, rows, GUTTER_PT, null);

            case PrintLayoutKind.FixedSize:
                return MakeFixedSizeCells(layout.PrintSizePt, content, GUTTER_PT, out isShrunk);

            case PrintLayoutKind.Wallet:
            case PrintLayoutKind.Passport:
                // small prints that get cut apart butt together, 9 wallets on Letter like the classic wizard
                return MakeFixedSizeCells(layout.PrintSizePt, content, 0, out isShrunk);

            case PrintLayoutKind.ContactSheet:
                // the longer side of the page takes the larger count
                var (c, r) = layout.Grid;
                var shape = content.Width > content.Height ? (Math.Max(c, r), Math.Min(c, r)) : (Math.Min(c, r), Math.Max(c, r));
                return MakeGrid(content, shape.Item1, shape.Item2, CONTACT_GUTTER_PT, null);

            default:
                return [content];
        }
    }


    /// <summary>
    /// Picks the rows and columns of a grid whose cells hold a print of the given aspect largest.
    /// </summary>
    private static (int Rows, int Cols) PickGridShape(int count, SKRect content, float itemAspect)
    {
        var best = (Rows: 1, Cols: count);
        var bestArea = -1f;

        for (var rows = 1; rows <= count; rows++)
        {
            if (count % rows != 0) continue;

            var cols = count / rows;
            var area = GetFittedArea(MakeGrid(content, cols, rows, GUTTER_PT, 1), itemAspect);
            if (area > bestArea)
            {
                bestArea = area;
                best = (rows, cols);
            }
        }

        return best;
    }


    /// <summary>
    /// Builds a grid of equal cells filling the content box, row by row; <paramref name="limit"/> stops after that many.
    /// </summary>
    private static List<SKRect> MakeGrid(SKRect content, int cols, int rows, float gutter, int? limit)
    {
        var cellW = (content.Width - (cols - 1) * gutter) / cols;
        var cellH = (content.Height - (rows - 1) * gutter) / rows;
        var cells = new List<SKRect>();
        if (cellW <= 0 || cellH <= 0) return cells;

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                cells.Add(SKRect.Create(content.Left + c * (cellW + gutter), content.Top + r * (cellH + gutter), cellW, cellH));
                if (cells.Count == limit) return cells;
            }
        }

        return cells;
    }


    /// <summary>
    /// Fits as many prints of a fixed size as the content holds, either way round, centered; a print larger than the page is scaled down.
    /// </summary>
    private static List<SKRect> MakeFixedSizeCells(SKSize printSize, SKRect content, float gutter, out bool isShrunk)
    {
        isShrunk = false;
        var portrait = new SKSize(Math.Min(printSize.Width, printSize.Height), Math.Max(printSize.Width, printSize.Height));
        var landscape = new SKSize(portrait.Height, portrait.Width);

        var (pCols, pRows) = CountFixed(portrait, content, gutter);
        var (lCols, lRows) = CountFixed(landscape, content, gutter);
        var pCount = pCols * pRows;
        var lCount = lCols * lRows;

        // more prints wins; at a tie the cell takes the content's own orientation
        var useLandscape = lCount > pCount || (lCount == pCount && content.Width > content.Height);
        var (size, cols, rows) = useLandscape ? (landscape, lCols, lRows) : (portrait, pCols, pRows);

        if (cols * rows == 0)
        {
            isShrunk = true;
            var fitSize = content.Width > content.Height ? landscape : portrait;
            var scale = Math.Min(content.Width / fitSize.Width, content.Height / fitSize.Height);
            size = new SKSize(fitSize.Width * scale, fitSize.Height * scale);
            (cols, rows) = (1, 1);
        }

        // center the block of prints in the content
        var blockW = cols * size.Width + (cols - 1) * gutter;
        var blockH = rows * size.Height + (rows - 1) * gutter;
        var left = content.MidX - blockW / 2;
        var top = content.MidY - blockH / 2;

        var cells = new List<SKRect>();
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                cells.Add(SKRect.Create(left + c * (size.Width + gutter), top + r * (size.Height + gutter), size.Width, size.Height));
            }
        }

        return cells;
    }


    private static (int Cols, int Rows) CountFixed(SKSize size, SKRect content, float gutter)
    {
        // a hair of tolerance, so a print exactly as wide as the content still fits
        var cols = (int)Math.Floor((content.Width + gutter + 0.01f) / (size.Width + gutter));
        var rows = (int)Math.Floor((content.Height + gutter + 0.01f) / (size.Height + gutter));

        return (Math.Max(0, cols), Math.Max(0, rows));
    }


    /// <summary>
    /// Sums the area a print of the given aspect covers when fitted into each cell, either way round.
    /// </summary>
    private static float GetFittedArea(IReadOnlyList<SKRect> cells, float aspect)
    {
        if (aspect <= 0) aspect = 1;

        var total = 0f;
        foreach (var cell in cells)
        {
            var upright = FitArea(cell, aspect);
            var turned = FitArea(cell, 1 / aspect);
            total += Math.Max(upright, turned);
        }

        return total;
    }


    private static float FitArea(SKRect cell, float aspect)
    {
        var w = Math.Min(cell.Width, cell.Height * aspect);
        return w * (w / aspect);
    }


    /// <summary>
    /// Counts the cells a print of the given aspect fits without turning, as <see cref="PlaceImage"/> decides it.
    /// </summary>
    private static int CountUpright(IReadOnlyList<SKRect> cells, float aspect)
    {
        var isLandscapePrint = aspect > 1;
        return cells.Count(cell => aspect == 1 || cell.Width == cell.Height || cell.Width > cell.Height == isLandscapePrint);
    }


    /// <summary>
    /// Fills the cells with the prints, page after page.
    /// </summary>
    private static List<PrintPage> AssignItems(PrintLayoutInput input, List<SKRect> cells, bool showCaptions)
    {
        var pages = new List<PrintPage>();
        if (cells.Count == 0 || input.ItemCount <= 0) return pages;

        // the item of each print, in order; a filled page starts each item on a page of its own
        var perPage = cells.Count;
        var prints = new List<int?>();
        var copies = Math.Max(1, input.PrintsPerItem);

        for (var item = 0; item < input.ItemCount; item++)
        {
            if (input.FillPage && perPage > 1)
            {
                var pageCount = (int)Math.Ceiling(copies / (double)perPage);
                for (var i = 0; i < pageCount * perPage; i++) prints.Add(item);
            }
            else
            {
                for (var i = 0; i < copies; i++) prints.Add(item);
            }
        }

        for (var start = 0; start < prints.Count; start += perPage)
        {
            var pageCells = new List<PrintCell>();
            for (var i = 0; i < perPage && start + i < prints.Count; i++)
            {
                if (prints[start + i] is not int item) continue;
                pageCells.Add(MakeCell(cells[i], item, showCaptions));
            }

            pages.Add(new PrintPage(pageCells));
        }

        return pages;
    }


    /// <summary>
    /// Splits a cell into its image and, when it is tall enough, the caption band under it.
    /// </summary>
    private static PrintCell MakeCell(SKRect cell, int item, bool showCaption)
    {
        if (!showCaption || cell.Height < CAPTION_BAND_PT * 3)
        {
            return new PrintCell(cell, cell, null, item);
        }

        var image = new SKRect(cell.Left, cell.Top, cell.Right, cell.Bottom - CAPTION_BAND_PT);
        var caption = new SKRect(cell.Left, cell.Bottom - CAPTION_BAND_PT, cell.Right, cell.Bottom);

        return new PrintCell(cell, image, caption, item);
    }
}


/// <summary>
/// Parses a frame range such as "1-3, 5, 8-".
/// </summary>
public static class PrintPageRange
{
    /// <summary>
    /// Parses 1-based numbers and ranges into sorted zero-based indexes below <paramref name="count"/>; <c>null</c> when the text is not a range.
    /// </summary>
    public static IReadOnlyList<int>? Parse(string? text, int count)
    {
        if (string.IsNullOrWhiteSpace(text) || count <= 0) return null;

        var result = new SortedSet<int>();
        var parts = text.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var dash = part.IndexOf('-');

            // a single number
            if (dash < 0)
            {
                if (!int.TryParse(part, out var n) || n < 1) return null;
                if (n <= count) result.Add(n - 1);
                continue;
            }

            // "a-b", "a-" to the end, "-b" from the start
            var fromText = part[..dash].Trim();
            var toText = part[(dash + 1)..].Trim();
            var from = fromText.Length == 0 ? 1 : int.TryParse(fromText, out var f) ? f : -1;
            var to = toText.Length == 0 ? count : int.TryParse(toText, out var t) ? t : -1;
            if (from < 1 || to < 1 || from > to) return null;

            for (var i = from; i <= Math.Min(to, count); i++) result.Add(i - 1);
        }

        return result.Count == 0 ? null : result.ToList();
    }
}
