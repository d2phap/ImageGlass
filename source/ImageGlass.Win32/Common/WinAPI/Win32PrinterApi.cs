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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.Printing;
using Windows.Win32.Storage.Xps;

namespace ImageGlass.Win32.Common;


/// <summary>
/// What the spooler says about one printer.
/// </summary>
internal sealed record Win32PrinterDetails(string? Port, string? Driver, uint Status, uint Attributes);


/// <summary>
/// The printers of Windows through the spooler: the list, what each can do, its DEVMODE, and the driver's own settings dialog.
/// </summary>
internal static unsafe class Win32PrinterApi
{
    private const uint PRINTER_ENUM_LOCAL = 0x2;
    private const uint PRINTER_ENUM_CONNECTIONS = 0x4;
    private const uint PRINTER_ATTRIBUTE_NETWORK = 0x10;
    private const uint PRINTER_ATTRIBUTE_WORK_OFFLINE = 0x400;

    private const uint DM_OUT_BUFFER = 0x2;
    private const uint DM_IN_PROMPT = 0x4;
    private const uint DM_IN_BUFFER = 0x8;

    private const short DMORIENT_PORTRAIT = 1;
    private const short DMORIENT_LANDSCAPE = 2;
    private const int IDOK = 1;
    private const int PAPER_NAME_LENGTH = 64;

    // the spooler's status bits, grouped by what the window shows
    private const uint STATUS_PAUSED = 0x1;
    private const uint STATUS_OFFLINE = 0x80 | 0x1000 | 0x800000;
    private const uint STATUS_ERROR = 0x2 | 0x8 | 0x10 | 0x40 | 0x800 | 0x40000 | 0x100000 | 0x200000 | 0x400000;
    private const uint STATUS_BUSY = 0x200 | 0x400 | 0x4000 | 0x8000 | 0x10000;


    #region Printers

    /// <summary>
    /// Lists the printers and printer connections, the default one marked.
    /// </summary>
    public static List<PrinterInfo> GetPrinters()
    {
        var defaultName = GetDefaultPrinterName();
        var printers = new List<PrinterInfo>();

        foreach (var (name, attributes) in EnumPrinterNames())
        {
            // a connection's server may be slow, so only a local printer is asked for its port
            string? extension = null;
            if ((attributes & PRINTER_ATTRIBUTE_NETWORK) == 0)
            {
                var details = GetPrinterDetails(name);

                // a fax printer opens a wizard of its own while printing
                if (HasPort(details, "SHRFAX:")) continue;
                extension = GetOutputFileExtension(details);
            }

            printers.Add(new PrinterInfo(name, name)
            {
                IsDefault = string.Equals(name, defaultName, StringComparison.OrdinalIgnoreCase),
                OutputFileExtension = extension,
            });
        }

        return printers;
    }


    /// <summary>
    /// Gets the state of a printer.
    /// </summary>
    public static PrinterStatus GetStatus(string name)
    {
        var details = GetPrinterDetails(name);
        if (details is null) return new PrinterStatus(PrinterState.Unknown);

        return new PrinterStatus(ToState(details.Status, details.Attributes));
    }


    private static PrinterState ToState(uint status, uint attributes)
    {
        if ((status & STATUS_PAUSED) != 0) return PrinterState.Paused;
        if ((status & STATUS_OFFLINE) != 0 || (attributes & PRINTER_ATTRIBUTE_WORK_OFFLINE) != 0) return PrinterState.Offline;
        if ((status & STATUS_ERROR) != 0) return PrinterState.Error;
        if ((status & STATUS_BUSY) != 0) return PrinterState.Busy;

        return PrinterState.Ready;
    }


    /// <summary>
    /// Gets the port, driver and state of a printer; <c>null</c> when it cannot be opened.
    /// </summary>
    public static Win32PrinterDetails? GetPrinterDetails(string name)
    {
        if (!TryOpenPrinter(name, out var handle)) return null;

        try
        {
            uint needed = 0;
            PInvoke.GetPrinter(handle, 2, null, 0, &needed);
            if (needed == 0) return null;

            var buffer = new byte[needed];
            fixed (byte* p = buffer)
            {
                if (!PInvoke.GetPrinter(handle, 2, p, needed, &needed)) return null;

                var info = (PRINTER_INFO_2W*)p;
                return new Win32PrinterDetails(info->pPortName.ToString(), info->pDriverName.ToString(), info->Status, info->Attributes);
            }
        }
        finally
        {
            PInvoke.ClosePrinter(handle);
        }
    }


    private static List<(string Name, uint Attributes)> EnumPrinterNames()
    {
        const uint flags = PRINTER_ENUM_LOCAL | PRINTER_ENUM_CONNECTIONS;

        // a printer added between the two calls needs a larger buffer, so ask again
        for (var attempt = 0; attempt < 3; attempt++)
        {
            uint needed = 0, returned = 0;
            PInvoke.EnumPrinters(flags, default, 4, null, 0, &needed, &returned);
            if (needed == 0) return [];

            var buffer = new byte[needed];
            fixed (byte* p = buffer)
            {
                if (!PInvoke.EnumPrinters(flags, default, 4, p, needed, &needed, &returned)) continue;

                var infos = (PRINTER_INFO_4W*)p;
                var list = new List<(string, uint)>((int)returned);
                for (var i = 0; i < returned; i++)
                {
                    var name = infos[i].pPrinterName.ToString();
                    if (!string.IsNullOrEmpty(name)) list.Add((name, infos[i].Attributes));
                }

                return list;
            }
        }

        throw new Win32Exception(Marshal.GetLastPInvokeError());
    }


    private static string? GetDefaultPrinterName()
    {
        uint length = 0;
        PInvoke.GetDefaultPrinter(default, &length);
        if (length == 0) return null;

        var buffer = new char[length];
        fixed (char* p = buffer)
        {
            return PInvoke.GetDefaultPrinter(p, &length) ? new string(p) : null;
        }
    }


    private static bool TryOpenPrinter(string name, out PRINTER_HANDLE handle)
    {
        PRINTER_HANDLE opened;
        fixed (char* pName = name)
        {
            var isOpen = PInvoke.OpenPrinter(pName, &opened, null);
            handle = isOpen ? opened : default;
            return isOpen;
        }
    }


    private static PRINTER_HANDLE OpenPrinter(string name)
    {
        if (TryOpenPrinter(name, out var handle)) return handle;

        throw new Win32Exception(Marshal.GetLastPInvokeError());
    }


    private static bool HasPort(Win32PrinterDetails? details, string port)
    {
        if (string.IsNullOrEmpty(details?.Port)) return false;

        return details.Port.Split(',', StringSplitOptions.TrimEntries).Any(i => i.Equals(port, StringComparison.OrdinalIgnoreCase));
    }


    /// <summary>
    /// Gets the extension of the file a printer writes instead of paper, by its driver; <c>null</c> for paper.
    /// </summary>
    private static string? GetOutputFileExtension(Win32PrinterDetails? details)
    {
        if (!HasPort(details, "PORTPROMPT:") && !HasPort(details, "FILE:")) return null;

        var driver = details?.Driver ?? string.Empty;
        if (driver.Contains("PDF", StringComparison.OrdinalIgnoreCase)) return ".pdf";
        if (driver.Contains("XPS", StringComparison.OrdinalIgnoreCase))
        {
            // the v4 XPS writer saves OpenXPS
            return driver.Contains("v4", StringComparison.OrdinalIgnoreCase) ? ".oxps" : ".xps";
        }

        return ".prn";
    }

    #endregion // Printers


    #region Capabilities

    /// <summary>
    /// Reads what a printer can do: its papers, resolutions, color, two-sided printing, copies and collation; one device context per paper is too slow, so only the default paper's printable area is measured.
    /// </summary>
    public static PrinterCapabilities GetCapabilities(string name, CancellationToken token)
    {
        var port = GetPrinterDetails(name)?.Port;
        var devMode = GetDefaultDevMode(name);

        fixed (char* pName = name)
        fixed (char* pPort = port)
        fixed (byte* pDevMode = devMode)
        {
            var dm = (DEVMODEW*)pDevMode;
            var papers = ReadPapers(pName, pPort, dm);
            token.ThrowIfCancellationRequested();

            var resolutions = ReadResolutions(pName, pPort, dm);
            var defaultPaperId = (dm->dmFields & DEVMODE_FIELD_FLAGS.DM_PAPERSIZE) != 0 && dm->dmPaperSize > 0
                ? dm->dmPaperSize.ToString(CultureInfo.InvariantCulture)
                : null;

            var defaultDpi = dm->dmPrintQuality > 0 ? dm->dmPrintQuality : dm->dmYResolution;
            if (defaultDpi <= 0) defaultDpi = (short)(resolutions.Count > 0 ? resolutions[^1] : 300);

            return new PrinterCapabilities
            {
                Papers = EstimatePrintableAreas(pName, devMode, papers, defaultPaperId),
                DefaultPaperId = defaultPaperId,
                SupportsColor = GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_COLORDEVICE, dm) == 1,
                SupportsDuplex = GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_DUPLEX, dm) == 1,
                SupportsCollate = GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_COLLATE, dm) == 1,
                MaxCopies = Math.Max(1, GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_COPIES, dm)),
                ResolutionsDpi = resolutions,
                DefaultDpi = defaultDpi,
                HasPropertiesDialog = true,
            };
        }
    }


    /// <summary>
    /// Gets how many copies the driver makes of one job and whether it collates them.
    /// </summary>
    public static (int MaxCopies, bool CanCollate) GetCopySupport(string name)
    {
        var port = GetPrinterDetails(name)?.Port;

        fixed (char* pName = name)
        fixed (char* pPort = port)
        {
            var maxCopies = GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_COPIES, null);
            var canCollate = GetCapability(pName, pPort, PRINTER_DEVICE_CAPABILITIES.DC_COLLATE, null) == 1;

            return (Math.Max(1, maxCopies), canCollate);
        }
    }


    private static int GetCapability(char* name, char* port, PRINTER_DEVICE_CAPABILITIES capability, DEVMODEW* dm)
    {
        return PInvoke.DeviceCapabilities(name, port, capability, default, dm);
    }


    /// <summary>
    /// Reads the papers of the driver in its own order and names, skipping sizeless and repeated entries; sizes are portrait.
    /// </summary>
    private static List<PaperInfo> ReadPapers(char* name, char* port, DEVMODEW* dm)
    {
        var count = GetCapability(name, port, PRINTER_DEVICE_CAPABILITIES.DC_PAPERS, dm);
        if (count <= 0) return [];

        var ids = new ushort[count];
        var names = new char[count * PAPER_NAME_LENGTH];
        var sizes = new int[count * 2];

        fixed (ushort* pIds = ids)
        fixed (char* pNames = names)
        fixed (int* pSizes = sizes)
        {
            if (PInvoke.DeviceCapabilities(name, port, PRINTER_DEVICE_CAPABILITIES.DC_PAPERS, (char*)pIds, dm) != count) return [];
            if (PInvoke.DeviceCapabilities(name, port, PRINTER_DEVICE_CAPABILITIES.DC_PAPERSIZE, (char*)pSizes, dm) != count) return [];
            var hasNames = PInvoke.DeviceCapabilities(name, port, PRINTER_DEVICE_CAPABILITIES.DC_PAPERNAMES, pNames, dm) == count;

            var papers = new List<PaperInfo>(count);
            var seen = new HashSet<ushort>();
            for (var i = 0; i < count; i++)
            {
                // sizes come in tenths of a millimeter
                var w = sizes[i * 2] / 10.0;
                var h = sizes[i * 2 + 1] / 10.0;
                if (ids[i] == 0 || w <= 0 || h <= 0 || !seen.Add(ids[i])) continue;

                var paperName = hasNames ? new string(names, i * PAPER_NAME_LENGTH, PAPER_NAME_LENGTH).Split('\0')[0].Trim() : string.Empty;
                var sizePt = new SKSize(PrintUnits.MmToPt(Math.Min(w, h)), PrintUnits.MmToPt(Math.Max(w, h)));
                if (string.IsNullOrEmpty(paperName)) paperName = PrintUnits.FormatSize(sizePt, PrintUnits.IsMetricRegion);

                papers.Add(new PaperInfo(ids[i].ToString(CultureInfo.InvariantCulture), PaperCatalog.GetDisplayName(paperName, sizePt), sizePt));
            }

            return papers;
        }
    }


    /// <summary>
    /// Reads the resolutions the driver offers, in dots per inch, lowest first.
    /// </summary>
    private static List<int> ReadResolutions(char* name, char* port, DEVMODEW* dm)
    {
        var count = GetCapability(name, port, PRINTER_DEVICE_CAPABILITIES.DC_ENUMRESOLUTIONS, dm);
        if (count <= 0) return [];

        var pairs = new int[count * 2];
        fixed (int* p = pairs)
        {
            if (PInvoke.DeviceCapabilities(name, port, PRINTER_DEVICE_CAPABILITIES.DC_ENUMRESOLUTIONS, (char*)p, dm) != count) return [];
        }

        // a resolution is a horizontal and vertical pair; an uneven one prints at its lower half
        var square = Enumerable.Range(0, count).Where(i => pairs[i * 2] == pairs[i * 2 + 1]).Select(i => pairs[i * 2]);
        var any = Enumerable.Range(0, count).Select(i => Math.Min(pairs[i * 2], pairs[i * 2 + 1]));
        var dpis = (square.Any() ? square : any).Where(i => i > 0).Distinct().Order().ToList();

        return dpis;
    }


    /// <summary>
    /// Measures the printable area of the default paper and lends it to the others, which are measured once chosen.
    /// </summary>
    private static List<PaperInfo> EstimatePrintableAreas(char* name, byte[] devMode, List<PaperInfo> papers, string? defaultPaperId)
    {
        var defaultPaper = papers.FirstOrDefault(i => i.Id == defaultPaperId) ?? papers.FirstOrDefault();
        var defaultMargins = defaultPaper is null ? null : MeasureMargins(name, devMode, defaultPaper);

        return papers.Select(paper => ReferenceEquals(paper, defaultPaper)
            ? paper with { HardwareMarginsPt = defaultMargins ?? PrintMargins.Zero, IsPrintableAreaMeasured = defaultMargins is not null }
            : paper with { HardwareMarginsPt = defaultMargins ?? PrintMargins.Zero, IsPrintableAreaMeasured = false })
            .ToList();
    }


    /// <summary>
    /// Measures the printable area of one paper; the estimate stays when the driver cannot tell.
    /// </summary>
    public static PaperInfo MeasurePaper(string name, PaperInfo paper)
    {
        var devMode = GetDefaultDevMode(name);

        fixed (char* pName = name)
        {
            var margins = MeasureMargins(pName, devMode, paper);
            return paper with { HardwareMarginsPt = margins ?? paper.HardwareMarginsPt, IsPrintableAreaMeasured = true };
        }
    }


    /// <summary>
    /// Measures the margins a printer cannot print on for a paper, in points, portrait.
    /// </summary>
    private static PrintMargins? MeasureMargins(char* name, byte[] devMode, PaperInfo paper)
    {
        if (!short.TryParse(paper.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var paperId)) return null;

        var copy = (byte[])devMode.Clone();
        fixed (byte* p = copy)
        {
            var dm = (DEVMODEW*)p;
            SetPaper(dm, paperId);
            dm->dmOrientation = DMORIENT_PORTRAIT;
            dm->dmFields |= DEVMODE_FIELD_FLAGS.DM_ORIENTATION;

            var hdc = PInvoke.CreateICW(default, name, default, dm);
            if (hdc.IsNull) return null;

            try
            {
                var dpiX = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
                var dpiY = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSY);
                var physicalW = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALWIDTH);
                var physicalH = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALHEIGHT);
                var offsetX = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALOFFSETX);
                var offsetY = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.PHYSICALOFFSETY);
                var printableW = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.HORZRES);
                var printableH = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.VERTRES);
                if (dpiX <= 0 || dpiY <= 0 || physicalW <= 0 || physicalH <= 0) return null;

                float ToPtX(int px) => Math.Max(0, px) * PrintUnits.POINTS_PER_INCH / dpiX;
                float ToPtY(int px) => Math.Max(0, px) * PrintUnits.POINTS_PER_INCH / dpiY;

                return new PrintMargins(
                    ToPtX(offsetX),
                    ToPtY(offsetY),
                    ToPtX(physicalW - offsetX - printableW),
                    ToPtY(physicalH - offsetY - printableH));
            }
            finally
            {
                PInvoke.DeleteDC(hdc);
            }
        }
    }

    #endregion // Capabilities


    #region DEVMODE

    /// <summary>
    /// Gets the printer's settings as the user last saved them.
    /// </summary>
    public static byte[] GetDefaultDevMode(string name)
    {
        var handle = OpenPrinter(name);
        try
        {
            fixed (char* pName = name)
            {
                var size = PInvoke.DocumentProperties(default, handle, pName, null, null, 0);
                if (size <= 0) throw new InvalidOperationException($"The driver of \"{name}\" returned no settings.");

                var devMode = new byte[size];
                fixed (byte* p = devMode)
                {
                    if (PInvoke.DocumentProperties(default, handle, pName, (DEVMODEW*)p, null, DM_OUT_BUFFER) < 0)
                    {
                        throw new InvalidOperationException($"The driver of \"{name}\" returned no settings.");
                    }
                }

                return devMode;
            }
        }
        finally
        {
            PInvoke.ClosePrinter(handle);
        }
    }


    /// <summary>
    /// Builds the DEVMODE of a job: what the driver's dialog returned, else its defaults, with the window's choices on top, as the driver validates them.
    /// </summary>
    public static byte[] BuildDevMode(string name, PrintJobSettings settings, int copies)
    {
        var input = settings.PlatformState is { Length: > 0 } state ? (byte[])state.Clone() : GetDefaultDevMode(name);
        fixed (byte* p = input)
        {
            ApplySettings((DEVMODEW*)p, settings, copies);
        }

        return MergeDevMode(name, input, IntPtr.Zero, false) ?? input;
    }


    /// <summary>
    /// Shows the driver's own settings dialog over the window, then reads its choices back into <paramref name="settings"/>.
    /// </summary>
    public static bool ShowPropertiesDialog(IntPtr owner, PrintJobSettings settings)
    {
        var name = settings.Printer.Id;
        var input = BuildDevMode(name, settings, settings.Copies);

        var output = MergeDevMode(name, input, owner, true);
        if (output is null) return false;

        settings.PlatformState = output;
        ReadSettings(output, settings);
        return true;
    }


    /// <summary>
    /// Passes a DEVMODE through the driver, which validates it and, with a prompt, lets the user change it; <c>null</c> when cancelled.
    /// </summary>
    private static byte[]? MergeDevMode(string name, byte[] input, IntPtr owner, bool prompt)
    {
        var handle = OpenPrinter(name);
        try
        {
            fixed (char* pName = name)
            {
                var size = PInvoke.DocumentProperties(default, handle, pName, null, null, 0);
                if (size <= 0) return null;

                var output = new byte[size];
                fixed (byte* pIn = input)
                fixed (byte* pOut = output)
                {
                    var mode = DM_IN_BUFFER | DM_OUT_BUFFER | (prompt ? DM_IN_PROMPT : 0);
                    var result = PInvoke.DocumentProperties(new HWND(owner), handle, pName, (DEVMODEW*)pOut, (DEVMODEW*)pIn, mode);

                    // with a prompt, anything but OK is a cancel
                    if (prompt ? result != IDOK : result < 0) return null;
                }

                return output;
            }
        }
        finally
        {
            PInvoke.ClosePrinter(handle);
        }
    }


    private static void ApplySettings(DEVMODEW* dm, PrintJobSettings settings, int copies)
    {
        if (short.TryParse(settings.Paper.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var paperId) && paperId > 0)
        {
            SetPaper(dm, paperId);
        }

        dm->dmOrientation = settings.IsLandscape ? DMORIENT_LANDSCAPE : DMORIENT_PORTRAIT;
        dm->dmCopies = (short)Math.Clamp(copies, 1, short.MaxValue);
        dm->dmCollate = settings.Collate ? DEVMODE_COLLATE.DMCOLLATE_TRUE : DEVMODE_COLLATE.DMCOLLATE_FALSE;
        dm->dmColor = settings.ColorMode == PrintColorMode.Grayscale ? DEVMODE_COLOR.DMCOLOR_MONOCHROME : DEVMODE_COLOR.DMCOLOR_COLOR;
        dm->dmDuplex = settings.Duplex switch
        {
            PrintDuplex.LongEdge => DEVMODE_DUPLEX.DMDUP_VERTICAL,
            PrintDuplex.ShortEdge => DEVMODE_DUPLEX.DMDUP_HORIZONTAL,
            _ => DEVMODE_DUPLEX.DMDUP_SIMPLEX,
        };
        dm->dmFields |= DEVMODE_FIELD_FLAGS.DM_ORIENTATION | DEVMODE_FIELD_FLAGS.DM_COPIES | DEVMODE_FIELD_FLAGS.DM_COLLATE
            | DEVMODE_FIELD_FLAGS.DM_COLOR | DEVMODE_FIELD_FLAGS.DM_DUPLEX;

        if (settings.Dpi > 0)
        {
            dm->dmPrintQuality = (short)Math.Min(settings.Dpi, short.MaxValue);
            dm->dmYResolution = dm->dmPrintQuality;
            dm->dmFields |= DEVMODE_FIELD_FLAGS.DM_PRINTQUALITY | DEVMODE_FIELD_FLAGS.DM_YRESOLUTION;
        }
    }


    /// <summary>
    /// Selects a paper by its number, which overrides a custom length and width and a form chosen by name.
    /// </summary>
    private static void SetPaper(DEVMODEW* dm, short paperId)
    {
        dm->dmPaperSize = paperId;
        dm->dmFields |= DEVMODE_FIELD_FLAGS.DM_PAPERSIZE;
        dm->dmFields &= ~(DEVMODE_FIELD_FLAGS.DM_PAPERLENGTH | DEVMODE_FIELD_FLAGS.DM_PAPERWIDTH | DEVMODE_FIELD_FLAGS.DM_FORMNAME);
    }


    /// <summary>
    /// Reads the choices of a DEVMODE into the settings, the paper by its id only.
    /// </summary>
    private static void ReadSettings(byte[] devMode, PrintJobSettings settings)
    {
        fixed (byte* p = devMode)
        {
            var dm = (DEVMODEW*)p;
            var fields = dm->dmFields;

            if ((fields & DEVMODE_FIELD_FLAGS.DM_PAPERSIZE) != 0 && dm->dmPaperSize > 0)
            {
                settings.Paper = new PaperInfo(dm->dmPaperSize.ToString(CultureInfo.InvariantCulture), string.Empty, SKSize.Empty);
            }

            if ((fields & DEVMODE_FIELD_FLAGS.DM_ORIENTATION) != 0) settings.IsLandscape = dm->dmOrientation == DMORIENT_LANDSCAPE;
            if ((fields & DEVMODE_FIELD_FLAGS.DM_COPIES) != 0 && dm->dmCopies > 0) settings.Copies = dm->dmCopies;
            if ((fields & DEVMODE_FIELD_FLAGS.DM_COLLATE) != 0) settings.Collate = dm->dmCollate == DEVMODE_COLLATE.DMCOLLATE_TRUE;

            if ((fields & DEVMODE_FIELD_FLAGS.DM_COLOR) != 0)
            {
                settings.ColorMode = dm->dmColor == DEVMODE_COLOR.DMCOLOR_MONOCHROME ? PrintColorMode.Grayscale : PrintColorMode.Color;
            }

            if ((fields & DEVMODE_FIELD_FLAGS.DM_DUPLEX) != 0)
            {
                settings.Duplex = dm->dmDuplex switch
                {
                    DEVMODE_DUPLEX.DMDUP_VERTICAL => PrintDuplex.LongEdge,
                    DEVMODE_DUPLEX.DMDUP_HORIZONTAL => PrintDuplex.ShortEdge,
                    _ => PrintDuplex.None,
                };
            }

            // a negative print quality is a draft or high setting, not a resolution
            if ((fields & DEVMODE_FIELD_FLAGS.DM_PRINTQUALITY) != 0 && dm->dmPrintQuality > 0) settings.Dpi = dm->dmPrintQuality;
            else if ((fields & DEVMODE_FIELD_FLAGS.DM_YRESOLUTION) != 0 && dm->dmYResolution > 0) settings.Dpi = dm->dmYResolution;
        }
    }

    #endregion // DEVMODE
}
