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

namespace ImageGlass.Common.Printing;


public enum PrintOrientation
{
    Auto,
    Portrait,
    Landscape,
}


public enum PrintFitMode
{
    Fit,
    Fill,
    ActualSize,
}


public enum PrintMarginPreset
{
    None,
    Narrow,
    Normal,
    Wide,
}


public enum PrintColorMode
{
    Color,
    Grayscale,
}


public enum PrintDuplex
{
    None,
    LongEdge,
    ShortEdge,
}


public enum PrintPageScope
{
    Current,
    All,
    Range,
}


public enum PrintRegion
{
    WholeImage,
    Selection,
    VisibleArea,
}


public enum PrinterState
{
    Unknown,
    Ready,
    Busy,
    Offline,
    Paused,
    Error,
}


/// <summary>
/// The margins of a page, in points, as left, top, right and bottom.
/// </summary>
public readonly record struct PrintMargins(float Left, float Top, float Right, float Bottom)
{
    /// <summary>
    /// Gets the margins of zero.
    /// </summary>
    public static PrintMargins Zero { get; } = new(0, 0, 0, 0);


    /// <summary>
    /// Gets whether all four margins are zero, as on a borderless printer.
    /// </summary>
    public bool IsZero => Left <= 0 && Top <= 0 && Right <= 0 && Bottom <= 0;


    /// <summary>
    /// Returns the larger of each side of the two margins.
    /// </summary>
    public PrintMargins Max(PrintMargins other)
    {
        return new(Math.Max(Left, other.Left), Math.Max(Top, other.Top),
            Math.Max(Right, other.Right), Math.Max(Bottom, other.Bottom));
    }


    /// <summary>
    /// Returns margins safe on a landscape page, whichever way the printer turns it: each axis takes the larger side.
    /// </summary>
    public PrintMargins ToLandscape()
    {
        var horizontal = Math.Max(Top, Bottom);
        var vertical = Math.Max(Left, Right);
        return new(horizontal, vertical, horizontal, vertical);
    }
}


/// <summary>
/// A printer, or a virtual destination such as Save as PDF.
/// </summary>
public sealed record PrinterInfo(string Id, string DisplayName)
{
    /// <summary>
    /// Gets whether the OS uses this printer by default.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Gets whether this is a destination of the app itself, such as Save as PDF.
    /// </summary>
    public bool IsVirtual { get; init; }

    /// <summary>
    /// Gets where the printer is, when the OS knows.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Gets the extension of the file a printer writes instead of paper, such as ".pdf"; <c>null</c> for paper.
    /// </summary>
    public string? OutputFileExtension { get; init; }
}


/// <summary>
/// The state of a printer and a message describing it.
/// </summary>
public sealed record PrinterStatus(PrinterState State, string? Message = null);


/// <summary>
/// A paper size a printer takes; sizes are in points, portrait.
/// </summary>
public sealed record PaperInfo(string Id, string DisplayName, SKSize SizePt)
{
    /// <summary>
    /// Gets the margins the printer cannot print on, in points, portrait.
    /// </summary>
    public PrintMargins HardwareMarginsPt { get; init; } = PrintMargins.Zero;

    /// <summary>
    /// Gets whether <see cref="HardwareMarginsPt"/> was measured for this paper, not estimated from the printer's default paper.
    /// </summary>
    public bool IsPrintableAreaMeasured { get; init; } = true;
}


/// <summary>
/// What a printer can do.
/// </summary>
public sealed record PrinterCapabilities
{
    /// <summary>
    /// Gets the paper sizes, in the order the printer lists them.
    /// </summary>
    public IReadOnlyList<PaperInfo> Papers { get; init; } = [];

    /// <summary>
    /// Gets the id of the paper the printer uses by default.
    /// </summary>
    public string? DefaultPaperId { get; init; }

    /// <summary>
    /// Gets whether the printer prints in color.
    /// </summary>
    public bool SupportsColor { get; init; } = true;

    /// <summary>
    /// Gets whether the printer prints on both sides.
    /// </summary>
    public bool SupportsDuplex { get; init; }

    /// <summary>
    /// Gets whether the printer collates copies itself.
    /// </summary>
    public bool SupportsCollate { get; init; }

    /// <summary>
    /// Gets the most copies the printer makes of one job.
    /// </summary>
    public int MaxCopies { get; init; } = 999;

    /// <summary>
    /// Gets the resolutions the printer offers, in dots per inch; empty when it offers no choice.
    /// </summary>
    public IReadOnlyList<int> ResolutionsDpi { get; init; } = [];

    /// <summary>
    /// Gets the resolution the printer uses by default, in dots per inch.
    /// </summary>
    public int DefaultDpi { get; init; } = 300;

    /// <summary>
    /// Gets whether the printer has its own settings dialog.
    /// </summary>
    public bool HasPropertiesDialog { get; init; }
}


/// <summary>
/// The settings of one print job, as the backend receives them.
/// </summary>
public sealed class PrintJobSettings
{
    /// <summary>
    /// Gets, sets the printer to print on.
    /// </summary>
    public required PrinterInfo Printer { get; set; }

    /// <summary>
    /// Gets, sets the paper to print on.
    /// </summary>
    public required PaperInfo Paper { get; set; }

    /// <summary>
    /// Gets, sets whether the page is landscape.
    /// </summary>
    public bool IsLandscape { get; set; }

    /// <summary>
    /// Gets, sets the number of copies of the whole job.
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// Gets, sets whether copies come out in page order, one set after another.
    /// </summary>
    public bool Collate { get; set; } = true;

    /// <summary>
    /// Gets, sets the two-sided mode.
    /// </summary>
    public PrintDuplex Duplex { get; set; } = PrintDuplex.None;

    /// <summary>
    /// Gets, sets whether to print in color or grayscale.
    /// </summary>
    public PrintColorMode ColorMode { get; set; } = PrintColorMode.Color;

    /// <summary>
    /// Gets, sets the resolution in dots per inch.
    /// </summary>
    public int Dpi { get; set; } = 300;

    /// <summary>
    /// Gets, sets the file to write when the destination writes a file, such as Save as PDF.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets, sets settings only the backend reads, such as a Windows printer's DEVMODE.
    /// </summary>
    public byte[]? PlatformState { get; set; }
}


/// <summary>
/// The progress of a print job.
/// </summary>
public readonly record struct PrintProgress(int PageIndex, int PageCount);
