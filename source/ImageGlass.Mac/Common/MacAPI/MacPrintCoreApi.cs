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
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Mac.Common;


/// <summary>
/// The options of a PrintCore job, as the Print window chose them.
/// </summary>
internal sealed record MacPrintJobOptions(string PaperId, int Copies, bool Collate, PrintDuplex Duplex);


/// <summary>
/// The printers of macOS through PrintCore: the list, their papers and state, and jobs that print a PDF.
/// </summary>
internal static unsafe partial class MacPrintCoreApi
{
    private const string PRINT_CORE = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

    private const ushort PRINTER_PROCESSING = 4;
    private const ushort PRINTER_STOPPED = 5;
    private const ushort ORIENTATION_PORTRAIT = 1;
    private const uint DUPLEX_NONE = 1;
    private const uint DUPLEX_NO_TUMBLE = 2;
    private const uint DUPLEX_TUMBLE = 3;


    #region Printers

    /// <summary>
    /// Lists the printers of the local print server, the default one marked.
    /// </summary>
    public static List<PrinterInfo> GetPrinters()
    {
        nint list = 0;
        ThrowIfFailed(PMServerCreatePrinterList(0, &list), nameof(PMServerCreatePrinterList));

        try
        {
            var count = MacCoreFoundation.GetCount(list);
            var printers = new List<PrinterInfo>((int)count);
            for (nint i = 0; i < count; i++)
            {
                var printer = MacCoreFoundation.GetItem(list, i);
                var id = MacCoreFoundation.ReadString(PMPrinterGetID(printer));
                if (string.IsNullOrEmpty(id)) continue;

                printers.Add(new PrinterInfo(id, MacCoreFoundation.ReadString(PMPrinterGetName(printer)) ?? id)
                {
                    IsDefault = PMPrinterIsDefault(printer) != 0,
                    Location = MacCoreFoundation.ReadString(PMPrinterGetLocation(printer)),
                });
            }

            return printers;
        }
        finally
        {
            MacCoreFoundation.Release(list);
        }
    }


    /// <summary>
    /// Gets the state of a printer.
    /// </summary>
    public static PrinterStatus GetStatus(string id)
    {
        var printer = CreatePrinter(id);
        if (printer == 0) return new PrinterStatus(PrinterState.Unknown);

        try
        {
            ushort state;
            if (PMPrinterGetState(printer, &state) != 0) return new PrinterStatus(PrinterState.Unknown);

            return new PrinterStatus(state switch
            {
                PRINTER_STOPPED => PrinterState.Paused,
                PRINTER_PROCESSING => PrinterState.Busy,
                _ => PrinterState.Ready,
            });
        }
        finally
        {
            _ = PMRelease(printer);
        }
    }

    #endregion // Printers


    #region Capabilities

    /// <summary>
    /// Reads what a printer can do: its papers with their printable areas and resolutions, color and two-sided printing from its PPD.
    /// </summary>
    public static PrinterCapabilities GetCapabilities(string id, CancellationToken token)
    {
        var printer = CreatePrinter(id);
        if (printer == 0) throw new IOException($"The printer \"{id}\" was not found.");

        try
        {
            var papers = ReadPapers(printer);
            token.ThrowIfCancellationRequested();

            var resolutions = ReadResolutions(printer);
            var (supportsColor, supportsDuplex) = ReadPpdFeatures(printer);

            return new PrinterCapabilities
            {
                Papers = papers,
                DefaultPaperId = ReadDefaultPaperId(printer),
                SupportsColor = supportsColor,
                SupportsDuplex = supportsDuplex,
                SupportsCollate = true,
                MaxCopies = 999,
                ResolutionsDpi = resolutions,
                DefaultDpi = resolutions.Count > 0 ? resolutions[^1] : 300,
            };
        }
        finally
        {
            _ = PMRelease(printer);
        }
    }


    private static List<PaperInfo> ReadPapers(nint printer)
    {
        nint list = 0;
        if (PMPrinterGetPaperList(printer, &list) != 0 || list == 0) return [];

        var count = MacCoreFoundation.GetCount(list);
        var papers = new List<PaperInfo>((int)count);
        var seen = new HashSet<string>();

        for (nint i = 0; i < count; i++)
        {
            var paper = MacCoreFoundation.GetItem(list, i);

            nint idRef = 0;
            double width, height;
            if (PMPaperGetID(paper, &idRef) != 0 || PMPaperGetWidth(paper, &width) != 0 || PMPaperGetHeight(paper, &height) != 0) continue;

            var paperId = MacCoreFoundation.ReadString(idRef);
            if (string.IsNullOrEmpty(paperId) || width <= 0 || height <= 0 || !seen.Add(paperId)) continue;

            // sizes and margins are already in points
            var sizePt = new SKSize((float)Math.Min(width, height), (float)Math.Max(width, height));
            PaperMargins m;
            var margins = PMPaperGetMargins(paper, &m) == 0
                ? new PrintMargins((float)m.Left, (float)m.Top, (float)m.Right, (float)m.Bottom)
                : PrintMargins.Zero;

            var name = ReadLocalizedName(paper, printer) ?? paperId;
            papers.Add(new PaperInfo(paperId, PaperCatalog.GetDisplayName(name, sizePt), sizePt) { HardwareMarginsPt = margins });
        }

        return papers;
    }


    private static string? ReadLocalizedName(nint paper, nint printer)
    {
        nint name = 0;
        if (PMPaperCreateLocalizedName(paper, printer, &name) != 0) return null;

        try
        {
            return MacCoreFoundation.ReadString(name);
        }
        finally
        {
            MacCoreFoundation.Release(name);
        }
    }


    /// <summary>
    /// Gets the paper of the printer's default page format.
    /// </summary>
    private static string? ReadDefaultPaperId(nint printer)
    {
        nint session = 0, format = 0;
        try
        {
            if (PMCreateSession(&session) != 0 || PMSessionSetCurrentPMPrinter(session, printer) != 0) return null;
            if (PMCreatePageFormat(&format) != 0 || PMSessionDefaultPageFormat(session, format) != 0) return null;

            nint paper = 0, paperId = 0;
            if (PMGetPageFormatPaper(format, &paper) != 0 || PMPaperGetID(paper, &paperId) != 0) return null;

            return MacCoreFoundation.ReadString(paperId);
        }
        finally
        {
            if (format != 0) _ = PMRelease(format);
            if (session != 0) _ = PMRelease(session);
        }
    }


    /// <summary>
    /// Reads the resolutions the printer offers, in dots per inch, lowest first.
    /// </summary>
    private static List<int> ReadResolutions(nint printer)
    {
        uint count;
        if (PMPrinterGetPrinterResolutionCount(printer, &count) != 0) return [];

        var dpis = new SortedSet<int>();
        for (uint i = 1; i <= count; i++)
        {
            Resolution resolution;
            if (PMPrinterGetIndexedPrinterResolution(printer, i, &resolution) != 0) continue;

            // an uneven resolution prints at its lower half
            var dpi = (int)Math.Round(Math.Min(resolution.Horizontal, resolution.Vertical));
            if (dpi > 0) dpis.Add(dpi);
        }

        return [.. dpis];
    }


    /// <summary>
    /// Reads color and two-sided support from the printer's PPD, which PrintCore offers no other way; without one, color only.
    /// </summary>
    private static (bool SupportsColor, bool SupportsDuplex) ReadPpdFeatures(nint printer)
    {
        var type = MacCoreFoundation.CreateString("PPD");
        nint url = 0;

        try
        {
            if (PMPrinterCopyDescriptionURL(printer, type, &url) != 0) return (true, false);

            var path = MacCoreFoundation.GetPath(url);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return (true, false);

            var lines = File.ReadLines(path).Select(i => i.Trim()).ToList();
            var isColor = lines.Any(i => i.StartsWith("*ColorDevice:", StringComparison.Ordinal) && i.Contains("True", StringComparison.OrdinalIgnoreCase));
            var hasDuplex = lines.Any(i => i.StartsWith("*OpenUI *Duplex", StringComparison.Ordinal));

            return (isColor, hasDuplex);
        }
        catch (IOException)
        {
            return (true, false);
        }
        finally
        {
            MacCoreFoundation.Release(url);
            MacCoreFoundation.Release(type);
        }
    }

    #endregion // Capabilities


    #region Jobs

    /// <summary>
    /// Prints a PDF of portrait pages on a printer, its paper and options set, and returns once the print system has it.
    /// </summary>
    public static void PrintFile(string id, string pdfPath, string title, MacPrintJobOptions options)
    {
        nint printer = 0, session = 0, settings = 0, format = 0, url = 0, mimeType = 0, jobName = 0;

        try
        {
            printer = CreatePrinter(id);
            if (printer == 0) throw new IOException($"The printer \"{id}\" was not found.");

            // 1. the settings the printer starts from, then the window's choices
            ThrowIfFailed(PMCreateSession(&session), nameof(PMCreateSession));
            ThrowIfFailed(PMSessionSetCurrentPMPrinter(session, printer), nameof(PMSessionSetCurrentPMPrinter));
            ThrowIfFailed(PMCreatePrintSettings(&settings), nameof(PMCreatePrintSettings));
            ThrowIfFailed(PMSessionDefaultPrintSettings(session, settings), nameof(PMSessionDefaultPrintSettings));

            jobName = MacCoreFoundation.CreateString(title);
            _ = PMPrintSettingsSetJobName(settings, jobName);
            _ = PMSetCopies(settings, (uint)Math.Max(1, options.Copies), 0);
            _ = PMSetCollate(settings, (byte)(options.Collate ? 1 : 0));
            _ = PMSetDuplex(settings, options.Duplex switch
            {
                PrintDuplex.LongEdge => DUPLEX_NO_TUMBLE,
                PrintDuplex.ShortEdge => DUPLEX_TUMBLE,
                _ => DUPLEX_NONE,
            });

            // 2. the paper, portrait, since a landscape page is already turned onto it
            format = CreatePageFormat(printer, options.PaperId);
            _ = PMSetOrientation(format, ORIENTATION_PORTRAIT, 0);
            _ = PMSessionValidatePageFormat(session, format, null);
            _ = PMSessionValidatePrintSettings(session, settings, null);

            // 3. the document
            url = MacCoreFoundation.CreateFileUrl(pdfPath);
            mimeType = MacCoreFoundation.CreateString("application/pdf");
            ThrowIfFailed(PMPrinterPrintWithFile(printer, settings, format, mimeType, url), nameof(PMPrinterPrintWithFile));
        }
        finally
        {
            MacCoreFoundation.Release(jobName);
            MacCoreFoundation.Release(mimeType);
            MacCoreFoundation.Release(url);
            if (format != 0) _ = PMRelease(format);
            if (settings != 0) _ = PMRelease(settings);
            if (session != 0) _ = PMRelease(session);
            if (printer != 0) _ = PMRelease(printer);
        }
    }


    /// <summary>
    /// Creates the page format of a paper of the printer, the printer's default when the paper is not among its own.
    /// </summary>
    private static nint CreatePageFormat(nint printer, string paperId)
    {
        nint list = 0;
        if (PMPrinterGetPaperList(printer, &list) == 0 && list != 0)
        {
            var count = MacCoreFoundation.GetCount(list);
            for (nint i = 0; i < count; i++)
            {
                var paper = MacCoreFoundation.GetItem(list, i);
                nint idRef = 0;
                if (PMPaperGetID(paper, &idRef) != 0 || MacCoreFoundation.ReadString(idRef) != paperId) continue;

                nint format = 0;
                ThrowIfFailed(PMCreatePageFormatWithPMPaper(&format, paper), nameof(PMCreatePageFormatWithPMPaper));
                return format;
            }
        }

        nint fallback = 0;
        ThrowIfFailed(PMCreatePageFormat(&fallback), nameof(PMCreatePageFormat));
        return fallback;
    }

    #endregion // Jobs


    #region Helpers

    /// <summary>
    /// Creates a printer from its id; release it with <see cref="PMRelease"/>.
    /// </summary>
    private static nint CreatePrinter(string id)
    {
        var idRef = MacCoreFoundation.CreateString(id);
        try
        {
            return PMPrinterCreateFromPrinterID(idRef);
        }
        finally
        {
            MacCoreFoundation.Release(idRef);
        }
    }


    private static void ThrowIfFailed(int status, string step)
    {
        if (status != 0) throw new IOException($"{step} failed (OSStatus {status}).");
    }

    #endregion // Helpers


    #region PrintCore interop

    [StructLayout(LayoutKind.Sequential)]
    private struct PaperMargins
    {
        public double Top;
        public double Left;
        public double Bottom;
        public double Right;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct Resolution
    {
        public double Horizontal;
        public double Vertical;
    }


    [LibraryImport(PRINT_CORE)]
    private static partial int PMServerCreatePrinterList(nint server, nint* printerList);

    [LibraryImport(PRINT_CORE)]
    private static partial nint PMPrinterCreateFromPrinterID(nint printerId);

    [LibraryImport(PRINT_CORE)]
    private static partial nint PMPrinterGetID(nint printer);

    [LibraryImport(PRINT_CORE)]
    private static partial nint PMPrinterGetName(nint printer);

    [LibraryImport(PRINT_CORE)]
    private static partial nint PMPrinterGetLocation(nint printer);

    [LibraryImport(PRINT_CORE)]
    private static partial byte PMPrinterIsDefault(nint printer);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterGetState(nint printer, ushort* state);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterGetPaperList(nint printer, nint* paperList);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPaperGetID(nint paper, nint* paperId);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPaperGetWidth(nint paper, double* width);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPaperGetHeight(nint paper, double* height);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPaperGetMargins(nint paper, PaperMargins* margins);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPaperCreateLocalizedName(nint paper, nint printer, nint* paperName);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterGetPrinterResolutionCount(nint printer, uint* count);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterGetIndexedPrinterResolution(nint printer, uint index, Resolution* resolution);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterCopyDescriptionURL(nint printer, nint descriptionType, nint* fileUrl);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMCreateSession(nint* session);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSessionSetCurrentPMPrinter(nint session, nint printer);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMCreatePrintSettings(nint* settings);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSessionDefaultPrintSettings(nint session, nint settings);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSessionValidatePrintSettings(nint session, nint settings, byte* changed);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrintSettingsSetJobName(nint settings, nint name);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSetCopies(nint settings, uint copies, byte lockSetting);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSetCollate(nint settings, byte collate);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSetDuplex(nint settings, uint duplex);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMCreatePageFormat(nint* format);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMCreatePageFormatWithPMPaper(nint* format, nint paper);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSessionDefaultPageFormat(nint session, nint format);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSessionValidatePageFormat(nint session, nint format, byte* changed);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMGetPageFormatPaper(nint format, nint* paper);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMSetOrientation(nint format, ushort orientation, byte lockSetting);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMPrinterPrintWithFile(nint printer, nint settings, nint format, nint mimeType, nint fileUrl);

    [LibraryImport(PRINT_CORE)]
    private static partial int PMRelease(nint pmObject);

    #endregion // PrintCore interop
}
