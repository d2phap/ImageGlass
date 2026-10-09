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
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;


/// <summary>
/// Prints through CUPS as PDF jobs, and opens the desktop's print dialog through the XDG print portal.
/// </summary>
internal class LinuxPrintProvider : PrintProviderBase
{
    /// <inheritdoc/>
    public override string? SystemPrintersUnavailableReason => CupsApi.IsAvailable ? null : Core.Lang[LangId.Print_CupsMissing];


    /// <inheritdoc/>
    protected override Task<IReadOnlyList<PrinterInfo>> GetSystemPrintersAsync(CancellationToken token)
    {
        if (!CupsApi.IsAvailable) return Task.FromResult<IReadOnlyList<PrinterInfo>>([]);

        return Task.Run<IReadOnlyList<PrinterInfo>>(CupsApi.GetPrinters, token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterCapabilities> GetSystemCapabilitiesAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => CupsApi.GetCapabilities(printer.Id, token), token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterStatus> GetSystemStatusAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => CupsApi.GetStatus(printer.Id), token).WaitAsync(token);
    }


    /// <summary>
    /// Writes the job as a PDF of portrait pages, a landscape one turned onto its sheet, and sends it to CUPS as one job.
    /// </summary>
    protected override async Task PrintToSystemAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var path = await WriteTempPdfAsync(job, true, progress, token).ConfigureAwait(false);

        try
        {
            // past this point the job goes whole, so a cancel never leaves half a document on the printer
            token.ThrowIfCancellationRequested();

            var settings = job.Settings;
            var options = new CupsJobOptions(settings.Paper.Id, settings.Copies, settings.Collate, settings.Duplex, settings.ColorMode, settings.Dpi);
            await Task.Run(() => CupsApi.PrintFile(settings.Printer.Id, path, job.Title, options), CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            TryDelete(path);
        }
    }


    /// <inheritdoc/>
    public override bool CanShowSystemDialog => true;


    /// <summary>
    /// Hands the pages as a PDF to the desktop's print dialog, through the XDG print portal.
    /// </summary>
    public override async Task ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token)
    {
        var path = await WriteTempPdfAsync(job, true, null, token).ConfigureAwait(false);

        try
        {
            await XdgPrintPortal.PrintAsync(path, job.Title, token).ConfigureAwait(false);
        }
        finally
        {
            // the portal holds its own descriptor of the file, so its name can go
            TryDelete(path);
        }
    }
}
