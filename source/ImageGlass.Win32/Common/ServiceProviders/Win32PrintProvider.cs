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
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Win32.Common.ServiceProviders;


/// <summary>
/// Prints through the Windows spooler with GDI, and opens Print Pictures as the system dialog.
/// </summary>
public class Win32PrintProvider : PrintProviderBase
{
    /// <inheritdoc/>
    protected override Task<IReadOnlyList<PrinterInfo>> GetSystemPrintersAsync(CancellationToken token)
    {
        return Task.Run<IReadOnlyList<PrinterInfo>>(Win32PrinterApi.GetPrinters, token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterCapabilities> GetSystemCapabilitiesAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => Win32PrinterApi.GetCapabilities(printer.Id, token), token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PrinterStatus> GetSystemStatusAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.Run(() => Win32PrinterApi.GetStatus(printer.Id), token).WaitAsync(token);
    }


    /// <inheritdoc/>
    protected override Task<PaperInfo> MeasureSystemPaperAsync(PrinterInfo printer, PaperInfo paper, CancellationToken token)
    {
        return Task.Run(() => Win32PrinterApi.MeasurePaper(printer.Id, paper), token).WaitAsync(token);
    }


    /// <summary>
    /// Shows the driver's own settings dialog, owned by the Print window, on the UI thread.
    /// </summary>
    protected override Task<bool> ShowSystemPropertiesDialogAsync(PhWindow owner, PrintJobSettings settings)
    {
        var handle = owner.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

        return Task.FromResult(Win32PrinterApi.ShowPropertiesDialog(handle, settings));
    }


    /// <inheritdoc/>
    protected override async Task PrintToSystemAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var settings = job.Settings;
        var name = settings.Printer.Id;

        // copies go to the driver when it can make them in the order asked, else the app prints each one
        var copies = Math.Max(1, settings.Copies);
        var (maxCopies, canCollate) = Win32PrinterApi.GetCopySupport(name);
        var isDriverCopies = copies <= maxCopies && (!settings.Collate || canCollate || job.Layout.Pages.Count == 1);

        var devMode = Win32PrinterApi.BuildDevMode(name, settings, isDriverCopies ? copies : 1);
        await Win32GdiPrintJob.RunAsync(job, devMode, isDriverCopies ? 1 : copies, progress, token).ConfigureAwait(false);
    }


    /// <inheritdoc/>
    public override bool CanShowSystemDialog => true;


    /// <summary>
    /// Opens Print Pictures with the shown frame, every edit applied.
    /// </summary>
    public override async Task<bool> ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token)
    {
        var path = await WriteShownImagePngAsync(job, token);
        if (string.IsNullOrEmpty(path)) return false;

        Win32PrintApi.OpenPrintDialog(BHelper.GetRealPlatformPath(path));
        return true;
    }


    /// <inheritdoc/>
    public override bool CanAddPrinter => true;


    /// <summary>
    /// Opens Settings at Printers &amp; scanners, where "Add device" adds a printer.
    /// </summary>
    public override Task OpenAddPrinterSettingsAsync(PhWindow owner)
    {
        return BHelper.OpenUrlAsync(owner, "ms-settings:printers");
    }
}
