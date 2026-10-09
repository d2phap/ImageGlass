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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Linux.Common;


/// <summary>
/// The options of a CUPS print job, as the Print window chose them.
/// </summary>
internal sealed record CupsJobOptions(string Media, int Copies, bool Collate, PrintDuplex Duplex, PrintColorMode ColorMode, int Dpi);


/// <summary>
/// The printers of CUPS through the host's libcups, whose connection is per thread, so each call runs start to end on one thread.
/// </summary>
internal static unsafe partial class CupsApi
{
    private const string LIB_CUPS = "libcups.so.2";

    // CUPS_HTTP_DEFAULT: the calling thread's connection to the default scheduler
    private const nint HTTP_DEFAULT = 0;

    private const int HTTP_STATUS_CONTINUE = 100;
    private const int IPP_STATUS_OK_EVENTS_COMPLETE = 0x0007;
    private const int IPP_RES_PER_CM = 4;
    private const int IPP_PRINTER_PROCESSING = 4;
    private const int IPP_PRINTER_STOPPED = 5;
    private const int MEDIA_NAME_LENGTH = 128;
    private const int WRITE_CHUNK_BYTES = 64 * 1024;

    private static readonly Lazy<bool> _isAvailable = new(() => NativeLibrary.TryLoad(LIB_CUPS, out _));


    /// <summary>
    /// Gets whether libcups is installed, so the system's printers can be listed.
    /// </summary>
    public static bool IsAvailable => _isAvailable.Value;


    #region Printers

    /// <summary>
    /// Lists the destinations of CUPS, each instance apart, the default one marked.
    /// </summary>
    public static List<PrinterInfo> GetPrinters()
    {
        CupsDest* dests = null;
        var count = cupsGetDests2(HTTP_DEFAULT, &dests);

        try
        {
            var printers = new List<PrinterInfo>(Math.Max(0, count));
            for (var i = 0; i < count; i++)
            {
                var dest = &dests[i];
                var id = GetDestId(dest);

                // the description the administrator gave the queue reads better than its name
                var description = GetOption(dest, "printer-info");
                var name = string.IsNullOrWhiteSpace(description) ? ToString(dest->Name) ?? id : description;
                var instance = ToString(dest->Instance);

                printers.Add(new PrinterInfo(id, string.IsNullOrEmpty(instance) ? name : $"{name} ({instance})")
                {
                    IsDefault = dest->IsDefault != 0,
                    Location = GetOption(dest, "printer-location"),
                });
            }

            return printers;
        }
        finally
        {
            if (dests != null) cupsFreeDests(count, dests);
        }
    }


    /// <summary>
    /// Gets the state of a destination from the scheduler.
    /// </summary>
    public static PrinterStatus GetStatus(string id)
    {
        var dest = GetNamedDest(id);
        if (dest == null) return new PrinterStatus(PrinterState.Unknown);

        try
        {
            var reasons = GetOption(dest, "printer-state-reasons") ?? string.Empty;
            var message = GetOption(dest, "printer-state-message");
            _ = int.TryParse(GetOption(dest, "printer-state"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var state);

            PrinterState result;
            if (reasons.Contains("offline", StringComparison.OrdinalIgnoreCase)) result = PrinterState.Offline;
            else if (reasons.Contains("paused", StringComparison.OrdinalIgnoreCase)) result = PrinterState.Paused;
            else if (state == IPP_PRINTER_STOPPED) result = PrinterState.Error;
            else if (state == IPP_PRINTER_PROCESSING) result = PrinterState.Busy;
            else result = state > 0 ? PrinterState.Ready : PrinterState.Unknown;

            return new PrinterStatus(result, string.IsNullOrWhiteSpace(message) ? null : message);
        }
        finally
        {
            cupsFreeDests(1, dest);
        }
    }

    #endregion // Printers


    #region Capabilities

    /// <summary>
    /// Reads what a destination can do: its media with their printable areas, resolutions, color and two-sided printing.
    /// </summary>
    public static PrinterCapabilities GetCapabilities(string id, CancellationToken token)
    {
        var dest = GetNamedDest(id);
        if (dest == null) throw new IOException(GetLastError());

        var info = cupsCopyDestInfo(HTTP_DEFAULT, dest);

        try
        {
            if (info == 0) throw new IOException(GetLastError());

            var papers = ReadMedia(dest, info);
            token.ThrowIfCancellationRequested();

            CupsSize defaultSize;
            var defaultPaperId = cupsGetDestMediaDefault(HTTP_DEFAULT, dest, info, 0, &defaultSize) != 0
                ? ToString(defaultSize.Media)
                : null;

            var resolutions = ReadResolutions(cupsFindDestSupported(HTTP_DEFAULT, dest, info, "printer-resolution"));
            var defaultDpi = ReadResolutions(cupsFindDestDefault(HTTP_DEFAULT, dest, info, "printer-resolution")).FirstOrDefault();

            return new PrinterCapabilities
            {
                Papers = papers,
                DefaultPaperId = defaultPaperId,
                SupportsColor = cupsCheckDestSupported(HTTP_DEFAULT, dest, info, "print-color-mode", "color") == 1,
                SupportsDuplex = cupsCheckDestSupported(HTTP_DEFAULT, dest, info, "sides", "two-sided-long-edge") == 1,

                // the scheduler makes and collates copies itself when the printer cannot
                SupportsCollate = true,
                MaxCopies = 999,
                ResolutionsDpi = resolutions,
                DefaultDpi = defaultDpi > 0 ? defaultDpi : resolutions.LastOrDefault(300),
            };
        }
        finally
        {
            if (info != 0) cupsFreeDestInfo(info);
            cupsFreeDests(1, dest);
        }
    }


    /// <summary>
    /// Reads the media of a destination with their printable areas, named by CUPS or, on an old libcups, from their PWG names.
    /// </summary>
    private static List<PaperInfo> ReadMedia(CupsDest* dest, nint info)
    {
        var count = cupsGetDestMediaCount(HTTP_DEFAULT, dest, info, 0);
        var papers = new List<PaperInfo>(Math.Max(0, count));
        var seen = new HashSet<string>();

        for (var i = 0; i < count; i++)
        {
            CupsSize size;
            if (cupsGetDestMediaByIndex(HTTP_DEFAULT, dest, info, i, 0, &size) == 0) continue;

            var id = ToString(size.Media);
            if (string.IsNullOrEmpty(id) || size.Width <= 0 || size.Length <= 0 || !seen.Add(id)) continue;

            // sizes and margins come in hundredths of a millimeter
            static float ToPt(int value) => PrintUnits.MmToPt(Math.Max(0, value) / 100.0);
            var sizePt = new SKSize(ToPt(Math.Min(size.Width, size.Length)), ToPt(Math.Max(size.Width, size.Length)));
            var margins = new PrintMargins(ToPt(size.Left), ToPt(size.Top), ToPt(size.Right), ToPt(size.Bottom));

            var name = LocalizeMedia(dest, info, &size) ?? GetNameFromPwg(id);
            papers.Add(new PaperInfo(id, PaperCatalog.GetDisplayName(name, sizePt), sizePt) { HardwareMarginsPt = margins });
        }

        return papers;
    }


    /// <summary>
    /// Gets the localized name of a medium, which libcups has offered since 1.6; <c>null</c> when it cannot.
    /// </summary>
    private static string? LocalizeMedia(CupsDest* dest, nint info, CupsSize* size)
    {
        try
        {
            var name = ToString(cupsLocalizeDestMedia(HTTP_DEFAULT, dest, info, 0, size));
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
    }


    /// <summary>
    /// Builds a name from a self-describing PWG media name, such as "A4" from "iso_a4_210x297mm".
    /// </summary>
    private static string GetNameFromPwg(string pwgName)
    {
        var parts = pwgName.Split('_');
        if (parts.Length < 3) return pwgName;

        var name = string.Join(' ', parts[1..^1]).Replace('-', ' ');
        if (name.Length == 0) return pwgName;

        // ISO and JIS sizes are written in capitals, others as a word
        var isSeriesName = name.Length <= 4 && char.IsLetter(name[0]) && name.Skip(1).All(char.IsDigit);
        return isSeriesName
            ? name.ToUpperInvariant()
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);
    }


    /// <summary>
    /// Reads the resolutions of a resolution attribute, in dots per inch, lowest first.
    /// </summary>
    private static List<int> ReadResolutions(nint attribute)
    {
        if (attribute == 0) return [];

        var dpis = new SortedSet<int>();
        var count = ippGetCount(attribute);
        for (var i = 0; i < count; i++)
        {
            int y, units;
            var x = ippGetResolution(attribute, i, &y, &units);

            // a resolution is a horizontal and vertical pair; an uneven one prints at its lower half
            var dpi = Math.Min(x, y);
            if (units == IPP_RES_PER_CM) dpi = (int)Math.Round(dpi * 2.54);
            if (dpi > 0) dpis.Add(dpi);
        }

        return [.. dpis];
    }

    #endregion // Capabilities


    #region Jobs

    /// <summary>
    /// Sends a PDF to a destination as one job; the scheduler places nothing, since the pages already carry the layout.
    /// </summary>
    public static void PrintFile(string id, string pdfPath, string title, CupsJobOptions options)
    {
        var dest = GetNamedDest(id);
        if (dest == null) throw new IOException(GetLastError());

        var info = cupsCopyDestInfo(HTTP_DEFAULT, dest);

        CupsOption* jobOptions = null;
        var optionCount = 0;

        try
        {
            if (info == 0) throw new IOException(GetLastError());

            foreach (var (name, value) in GetJobOptions(options))
            {
                optionCount = cupsAddOption(name, value, optionCount, &jobOptions);
            }

            // 1. the job, with every option
            var jobId = 0;
            var status = cupsCreateDestJob(HTTP_DEFAULT, dest, info, &jobId, title, optionCount, jobOptions);
            if (status > IPP_STATUS_OK_EVENTS_COMPLETE || jobId == 0) throw new IOException(GetLastError());

            // 2. its one document, sent whole, so a cancel never leaves the printer half a file
            try
            {
                if (cupsStartDestDocument(HTTP_DEFAULT, dest, info, jobId, title, "application/pdf", 0, null, 1) != HTTP_STATUS_CONTINUE)
                {
                    throw new IOException(GetLastError());
                }

                WriteDocument(pdfPath);

                if (cupsFinishDestDocument(HTTP_DEFAULT, dest, info) > IPP_STATUS_OK_EVENTS_COMPLETE)
                {
                    throw new IOException(GetLastError());
                }
            }
            catch
            {
                _ = cupsCancelDestJob(HTTP_DEFAULT, dest, jobId);
                throw;
            }
        }
        finally
        {
            if (jobOptions != null) cupsFreeOptions(optionCount, jobOptions);
            if (info != 0) cupsFreeDestInfo(info);
            cupsFreeDests(1, dest);
        }
    }


    /// <summary>
    /// Builds the IPP job options: pages as they are, never scaled or turned, and the window's choices.
    /// </summary>
    private static List<(string Name, string Value)> GetJobOptions(CupsJobOptions options)
    {
        var list = new List<(string, string)>
        {
            ("media", options.Media),
            ("print-scaling", "none"),

            // every page is portrait, a landscape one already turned onto it
            ("orientation-requested", "3"),
            ("print-color-mode", options.ColorMode == PrintColorMode.Grayscale ? "monochrome" : "color"),
            ("sides", options.Duplex switch
            {
                PrintDuplex.LongEdge => "two-sided-long-edge",
                PrintDuplex.ShortEdge => "two-sided-short-edge",
                _ => "one-sided",
            }),
        };

        if (options.Copies > 1)
        {
            list.Add(("copies", options.Copies.ToString(CultureInfo.InvariantCulture)));
            list.Add(("multiple-document-handling", options.Collate ? "separate-documents-collated-copies" : "separate-documents-uncollated-copies"));
        }

        if (options.Dpi > 0)
        {
            list.Add(("printer-resolution", options.Dpi.ToString(CultureInfo.InvariantCulture) + "dpi"));
        }

        return list;
    }


    private static void WriteDocument(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[WRITE_CHUNK_BYTES];

        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            fixed (byte* p = buffer)
            {
                if (cupsWriteRequestData(HTTP_DEFAULT, p, (nuint)read) != HTTP_STATUS_CONTINUE) throw new IOException(GetLastError());
            }
        }
    }

    #endregion // Jobs


    #region Helpers

    /// <summary>
    /// Gets a destination by the id <see cref="GetPrinters"/> gave it, "name" or "name/instance"; the caller frees it.
    /// </summary>
    private static CupsDest* GetNamedDest(string id)
    {
        var slash = id.IndexOf('/');
        var name = slash < 0 ? id : id[..slash];
        var instance = slash < 0 ? null : id[(slash + 1)..];

        return cupsGetNamedDest(HTTP_DEFAULT, name, instance);
    }


    private static string GetDestId(CupsDest* dest)
    {
        var name = ToString(dest->Name) ?? string.Empty;
        var instance = ToString(dest->Instance);

        return string.IsNullOrEmpty(instance) ? name : $"{name}/{instance}";
    }


    private static string? GetOption(CupsDest* dest, string name)
    {
        return ToString(cupsGetOption(name, dest->NumOptions, dest->Options));
    }


    private static string GetLastError()
    {
        return ToString(cupsLastErrorString()) ?? "CUPS did not say what went wrong.";
    }


    private static string? ToString(byte* value) => value == null ? null : Marshal.PtrToStringUTF8((nint)value);

    #endregion // Helpers


    #region Native

    [StructLayout(LayoutKind.Sequential)]
    private struct CupsOption
    {
        public byte* Name;
        public byte* Value;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct CupsDest
    {
        public byte* Name;
        public byte* Instance;
        public int IsDefault;
        public int NumOptions;
        public CupsOption* Options;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct CupsSize
    {
        public fixed byte Media[MEDIA_NAME_LENGTH];
        public int Width;
        public int Length;
        public int Bottom;
        public int Left;
        public int Right;
        public int Top;
    }


    [LibraryImport(LIB_CUPS)]
    private static partial int cupsGetDests2(nint http, CupsDest** dests);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial CupsDest* cupsGetNamedDest(nint http, string name, string? instance);

    [LibraryImport(LIB_CUPS)]
    private static partial void cupsFreeDests(int numDests, CupsDest* dests);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial byte* cupsGetOption(string name, int numOptions, CupsOption* options);

    [LibraryImport(LIB_CUPS)]
    private static partial nint cupsCopyDestInfo(nint http, CupsDest* dest);

    [LibraryImport(LIB_CUPS)]
    private static partial void cupsFreeDestInfo(nint info);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsGetDestMediaCount(nint http, CupsDest* dest, nint info, uint flags);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsGetDestMediaByIndex(nint http, CupsDest* dest, nint info, int index, uint flags, CupsSize* size);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsGetDestMediaDefault(nint http, CupsDest* dest, nint info, uint flags, CupsSize* size);

    [LibraryImport(LIB_CUPS)]
    private static partial byte* cupsLocalizeDestMedia(nint http, CupsDest* dest, nint info, uint flags, CupsSize* size);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int cupsCheckDestSupported(nint http, CupsDest* dest, nint info, string option, string? value);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint cupsFindDestSupported(nint http, CupsDest* dest, nint info, string option);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint cupsFindDestDefault(nint http, CupsDest* dest, nint info, string option);

    [LibraryImport(LIB_CUPS)]
    private static partial int ippGetCount(nint attribute);

    [LibraryImport(LIB_CUPS)]
    private static partial int ippGetResolution(nint attribute, int element, int* yres, int* units);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int cupsAddOption(string name, string value, int numOptions, CupsOption** options);

    [LibraryImport(LIB_CUPS)]
    private static partial void cupsFreeOptions(int numOptions, CupsOption* options);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int cupsCreateDestJob(nint http, CupsDest* dest, nint info, int* jobId, string title, int numOptions, CupsOption* options);

    [LibraryImport(LIB_CUPS, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int cupsStartDestDocument(nint http, CupsDest* dest, nint info, int jobId, string docName, string format, int numOptions, CupsOption* options, int lastDocument);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsWriteRequestData(nint http, byte* buffer, nuint length);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsFinishDestDocument(nint http, CupsDest* dest, nint info);

    [LibraryImport(LIB_CUPS)]
    private static partial int cupsCancelDestJob(nint http, CupsDest* dest, int jobId);

    [LibraryImport(LIB_CUPS)]
    private static partial byte* cupsLastErrorString();

    #endregion // Native
}
