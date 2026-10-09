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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Printing;


/// <summary>
/// How a print job is written as a PDF.
/// </summary>
public sealed record PdfWriteOptions
{
    public string Title { get; init; } = "ImageGlass";

    /// <summary>
    /// Gets the resolution images are embedded at, at most, in pixels per inch of paper.
    /// </summary>
    public float Dpi { get; init; } = 300;

    /// <summary>
    /// Gets the JPEG quality for opaque images, above 100 for lossless; 95 keeps a photo's PDF near its own file size, not four times it.
    /// </summary>
    public int EncodingQuality { get; init; } = 95;

    /// <summary>
    /// Gets whether a landscape page is turned onto portrait paper, for print systems that would otherwise rotate or scale it on their own.
    /// </summary>
    public bool PortraitPagesOnly { get; init; }

    public required PrintRenderOptions Render { get; init; }
}


/// <summary>
/// Writes a print job as a PDF with the same renderer as the preview.
/// </summary>
public static class PdfPrintWriter
{
    /// <summary>
    /// Writes every page of the layout to the stream.
    /// </summary>
    public static async Task WriteAsync(Stream stream, PrintDocumentLayout layout, IPrintImageSource images,
        PdfWriteOptions options, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var now = DateTime.Now;
        var metadata = new SKDocumentPdfMetadata
        {
            Title = options.Title,
            Creator = "ImageGlass",
            Producer = "ImageGlass",
            Creation = now,
            Modified = now,
            RasterDpi = options.Dpi,
            EncodingQuality = options.EncodingQuality,
        };

        using var doc = SKDocument.CreatePdf(stream, metadata)
            ?? throw new InvalidOperationException("The PDF backend of SkiaSharp is not available.");

        var render = options.Render with { TargetDpi = options.Dpi, IsPreview = false };
        var size = layout.PageSizePt;
        var turn = options.PortraitPagesOnly && layout.IsLandscape;

        for (var i = 0; i < layout.Pages.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            progress?.Report(new PrintProgress(i, layout.Pages.Count));

            var canvas = doc.BeginPage(turn ? size.Height : size.Width, turn ? size.Width : size.Height);
            try
            {
                // landscape content turned a quarter counter-clockwise onto the portrait sheet, as IPP's "landscape" does
                if (turn)
                {
                    canvas.Translate(0, size.Width);
                    canvas.RotateDegrees(-90);
                }

                await PrintPageRenderer.DrawPageAsync(canvas, layout, i, images, render, token).ConfigureAwait(false);
            }
            finally
            {
                doc.EndPage();
            }
        }

        doc.Close();
        progress?.Report(new PrintProgress(layout.Pages.Count, layout.Pages.Count));
    }
}
