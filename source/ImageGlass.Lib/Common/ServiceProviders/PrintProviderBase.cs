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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Printing;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// The part of every print provider the library owns: the built-in Save as PDF destination and the PDF and image helpers backends share.
/// </summary>
public abstract class PrintProviderBase : IPrintProvider
{
    /// <summary>
    /// The id of the built-in Save as PDF destination.
    /// </summary>
    public const string PDF_PRINTER_ID = "ig:pdf";


    /// <summary>
    /// Gets the built-in destination that writes a PDF file.
    /// </summary>
    public static PrinterInfo SaveAsPdfPrinter => new(PDF_PRINTER_ID, Core.Lang[LangId.Print_SaveAsPdf]) { IsVirtual = true, OutputFileExtension = ".pdf" };


    /// <summary>
    /// Gets what Save as PDF offers: the built-in papers and the usual resolutions.
    /// </summary>
    public static PrinterCapabilities PdfCapabilities { get; } = new()
    {
        Papers = PaperCatalog.Papers,
        DefaultPaperId = PaperCatalog.DefaultPaperId,
        SupportsColor = true,
        MaxCopies = 1,
        ResolutionsDpi = [150, 300, 600],
        DefaultDpi = 300,
    };


    /// <inheritdoc/>
    public async Task<IReadOnlyList<PrinterInfo>> GetPrintersAsync(CancellationToken token)
    {
        var printers = new List<PrinterInfo>();

        try
        {
            printers.AddRange(await GetSystemPrintersAsync(token).ConfigureAwait(false));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            // the app's own destination still works without the system's printers
            Debug.WriteLine($"❌❌❌ {nameof(PrintProviderBase)}.{nameof(GetPrintersAsync)}: {ex.Message}");
        }

        printers.Add(SaveAsPdfPrinter);
        return printers;
    }


    /// <inheritdoc/>
    public virtual string? SystemPrintersUnavailableReason => null;


    /// <inheritdoc/>
    public Task<PrinterCapabilities> GetCapabilitiesAsync(PrinterInfo printer, CancellationToken token)
    {
        return printer.Id == PDF_PRINTER_ID
            ? Task.FromResult(PdfCapabilities)
            : GetSystemCapabilitiesAsync(printer, token);
    }


    /// <inheritdoc/>
    public Task<PrinterStatus> GetStatusAsync(PrinterInfo printer, CancellationToken token)
    {
        return printer.Id == PDF_PRINTER_ID
            ? Task.FromResult(new PrinterStatus(PrinterState.Ready))
            : GetSystemStatusAsync(printer, token);
    }


    /// <inheritdoc/>
    public Task<PaperInfo> MeasurePaperAsync(PrinterInfo printer, PaperInfo paper, CancellationToken token)
    {
        return paper.IsPrintableAreaMeasured || printer.Id == PDF_PRINTER_ID
            ? Task.FromResult(paper)
            : MeasureSystemPaperAsync(printer, paper, token);
    }


    /// <inheritdoc/>
    public Task<bool> ShowPropertiesDialogAsync(PhWindow owner, PrintJobSettings settings)
    {
        return settings.Printer.Id == PDF_PRINTER_ID
            ? Task.FromResult(false)
            : ShowSystemPropertiesDialogAsync(owner, settings);
    }


    /// <inheritdoc/>
    public Task PrintAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token)
    {
        if (job.Settings.Printer.Id != PDF_PRINTER_ID) return PrintToSystemAsync(job, progress, token);

        var path = job.Settings.OutputPath;
        if (string.IsNullOrEmpty(path)) throw new InvalidOperationException("Save as PDF needs a file path.");

        return SaveAsPdfAsync(job, path, false, progress, token);
    }


    /// <inheritdoc/>
    public virtual bool CanShowSystemDialog => false;


    /// <inheritdoc/>
    public virtual Task ShowSystemDialogAsync(PhWindow owner, PrintJob job, CancellationToken token) => Task.CompletedTask;


    /// <inheritdoc/>
    public virtual bool CanAddPrinter => false;


    /// <inheritdoc/>
    public virtual Task OpenAddPrinterSettingsAsync(PhWindow owner) => Task.CompletedTask;


    #region Platform parts

    /// <summary>
    /// Gets the printers of the system.
    /// </summary>
    protected abstract Task<IReadOnlyList<PrinterInfo>> GetSystemPrintersAsync(CancellationToken token);

    /// <summary>
    /// Gets what a system printer can do.
    /// </summary>
    protected abstract Task<PrinterCapabilities> GetSystemCapabilitiesAsync(PrinterInfo printer, CancellationToken token);

    /// <summary>
    /// Gets the state of a system printer.
    /// </summary>
    protected virtual Task<PrinterStatus> GetSystemStatusAsync(PrinterInfo printer, CancellationToken token)
    {
        return Task.FromResult(new PrinterStatus(PrinterState.Unknown));
    }

    /// <summary>
    /// Measures the printable area of a system printer's paper; a backend that measures every paper up front keeps the estimate.
    /// </summary>
    protected virtual Task<PaperInfo> MeasureSystemPaperAsync(PrinterInfo printer, PaperInfo paper, CancellationToken token)
    {
        return Task.FromResult(paper with { IsPrintableAreaMeasured = true });
    }

    /// <summary>
    /// Shows a system printer's own settings dialog.
    /// </summary>
    protected virtual Task<bool> ShowSystemPropertiesDialogAsync(PhWindow owner, PrintJobSettings settings) => Task.FromResult(false);

    /// <summary>
    /// Prints the job on a system printer.
    /// </summary>
    protected abstract Task PrintToSystemAsync(PrintJob job, IProgress<PrintProgress>? progress, CancellationToken token);

    #endregion // Platform parts


    #region Shared helpers

    /// <summary>
    /// Writes the job as a PDF, staged beside the target and renamed over it, so a failure never leaves half a file.
    /// </summary>
    protected static async Task SaveAsPdfAsync(PrintJob job, string path, bool portraitPagesOnly,
        IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var tempPath = $"{path}.{Environment.ProcessId}.tmp";

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var options = new PdfWriteOptions
                {
                    Title = job.Title,
                    Dpi = job.Settings.Dpi,
                    PortraitPagesOnly = portraitPagesOnly,
                    Render = job.Render with { ColorMode = job.Settings.ColorMode },
                };

                await PdfPrintWriter.WriteAsync(stream, job.Layout, job.Session, options, progress, token).ConfigureAwait(false);
            }

            File.Move(tempPath, path, true);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }


    /// <summary>
    /// Writes the job as a PDF in the app's temporary folder, for a print system that takes PDF; the folder is emptied on exit.
    /// </summary>
    protected static async Task<string> WriteTempPdfAsync(PrintJob job, bool portraitPagesOnly,
        IProgress<PrintProgress>? progress, CancellationToken token)
    {
        var path = BHelper.ConfigDir(Dir.Temporary, $"ig_print_{Guid.NewGuid():N}.pdf");
        await SaveAsPdfAsync(job, path, portraitPagesOnly, progress, token).ConfigureAwait(false);

        return path;
    }


    /// <summary>
    /// Writes the shown frame with every edit as a PNG in the app's temporary folder, for a system dialog that takes one image.
    /// </summary>
    protected static async Task<string?> WriteShownImagePngAsync(PrintJob job, CancellationToken token)
    {
        var region = job.Session.Items.FirstOrDefault()?.Region ?? PrintRegion.WholeImage;
        using var img = await job.Session.BuildFullImageAsync(job.Session.State.FrameIndex, region, token).ConfigureAwait(false);
        if (img.IsDisposed()) return null;

        var path = BHelper.ConfigDir(Dir.Temporary, $"ig_print_{Guid.NewGuid():N}.png");
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        await using var stream = File.Create(path);
        data.SaveTo(stream);

        return path;
    }


    /// <summary>
    /// Deletes a file the print left behind, ignoring a failure.
    /// </summary>
    protected static void TryDelete(string? path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    #endregion // Shared helpers
}
