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
using ImageGlass.Common.Printing;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;


public interface IPrintProvider
{
    /// <summary>
    /// Gets the printers to offer, the built-in Save as PDF last; a slow network printer must not stall it.
    /// </summary>
    Task<IReadOnlyList<PrinterInfo>> GetPrintersAsync(CancellationToken token);

    /// <summary>
    /// Gets what a printer can do: its papers, resolutions, color and two-sided support.
    /// </summary>
    Task<PrinterCapabilities> GetCapabilitiesAsync(PrinterInfo printer, CancellationToken token);

    /// <summary>
    /// Gets the state of a printer.
    /// </summary>
    Task<PrinterStatus> GetStatusAsync(PrinterInfo printer, CancellationToken token);

    /// <summary>
    /// Measures the printable area of a paper the capabilities only estimated, as it is chosen.
    /// </summary>
    Task<PaperInfo> MeasurePaperAsync(PrinterInfo printer, PaperInfo paper, CancellationToken token);

    /// <summary>
    /// Shows the printer's own settings dialog and reads its choices back into <paramref name="settings"/>; <c>false</c> when cancelled or there is none.
    /// </summary>
    Task<bool> ShowPropertiesDialogAsync(PhWindow owner, PrintJobSettings settings);

    /// <summary>
    /// Prints the job, reporting each page.
    /// </summary>
    Task PrintAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token);

    /// <summary>
    /// Gets whether the platform has a print dialog of its own to hand the job to.
    /// </summary>
    bool CanShowSystemDialog { get; }

    /// <summary>
    /// Hands the job to the platform's own print dialog.
    /// </summary>
    Task ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token);
}


/// <summary>
/// One print job: its settings, its pages, and the images to draw on them.
/// </summary>
public sealed class PrintJob
{
    public required PrintJobSettings Settings { get; init; }
    public required string Title { get; init; }
    public required PrintDocumentLayout Layout { get; init; }
    public required PrintSession Session { get; init; }
    public required PrintRenderOptions Render { get; init; }
}
