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
using Avalonia.Threading;
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Mac.Common.ServiceProviders;


/// <summary>
/// Prints through PrintCore as PDF jobs, and opens the macOS print panel for the system dialog.
/// </summary>
internal class MacPrintProvider : PrintProviderBase
{
    /// <inheritdoc/>
    protected override Task<IReadOnlyList<PrinterInfo>> GetSystemPrintersAsync(CancellationToken token)
    {
        return Task.Run<IReadOnlyList<PrinterInfo>>(MacPrintCoreApi.GetPrinters, token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterCapabilities> GetSystemCapabilitiesAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => MacPrintCoreApi.GetCapabilities(printer.Id, token), token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterStatus> GetSystemStatusAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => MacPrintCoreApi.GetStatus(printer.Id), token).WaitAsync(token);
    }


    /// <summary>
    /// Writes the job as a PDF of portrait pages, a landscape one turned onto its sheet, and hands it to the print system.
    /// </summary>
    protected override async Task PrintToSystemAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var path = await WriteTempPdfAsync(job, true, progress, token).ConfigureAwait(false);

        try
        {
            // past this point the job goes whole, so a cancel never leaves half a document on the printer
            token.ThrowIfCancellationRequested();

            var settings = job.Settings;
            var options = new MacPrintJobOptions(settings.Paper.Id, settings.Copies, settings.Collate, settings.Duplex);
            await Task.Run(() => MacPrintCoreApi.PrintFile(settings.Printer.Id, path, job.Title, options), CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            TryDelete(path);
        }
    }


    /// <inheritdoc/>
    public override bool CanShowSystemDialog => true;


    /// <summary>
    /// Shows the macOS print panel for the pages as a PDF, landscape pages kept, since the panel turns them to its paper.
    /// </summary>
    public override async Task ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token)
    {
        var path = await WriteTempPdfAsync(job, false, null, token).ConfigureAwait(false);

        try
        {
            // AppKit runs the panel on the main thread
            await Dispatcher.UIThread.InvokeAsync(() => MacPrintPanelApi.ShowPrintPanel(path));
        }
        finally
        {
            TryDelete(path);
        }
    }
}
