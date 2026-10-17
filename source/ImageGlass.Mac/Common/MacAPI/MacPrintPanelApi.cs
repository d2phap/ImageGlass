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
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageGlass.Mac.Common;


/// <summary>
/// What the print panel starts from: the Print window's printer (its display name), paper, orientation and copies.
/// </summary>
internal sealed record MacPrintPanelOptions(string? PrinterName, string? PaperId, bool IsLandscape, int Copies);


/// <summary>
/// Shows the macOS print panel for a PDF, through PDFKit and AppKit, which must run on the main thread.
/// </summary>
internal static unsafe partial class MacPrintPanelApi
{
    private const string LIB_OBJC = "/usr/lib/libobjc.dylib";
    private const string PDFKIT = "/System/Library/Frameworks/PDFKit.framework/PDFKit";

    // PDFPrintScalingMode: shrink a page only when the paper chosen in the panel is smaller
    private const nint SCALE_DOWN_TO_FIT = 2;
    private const nint ORIENTATION_LANDSCAPE = 1;

    // NSPrintPanelOptions: the panel keeps a paper and orientation only while it shows them
    private const nint SHOWS_PAPER_SIZE = 1 << 2;
    private const nint SHOWS_ORIENTATION = 1 << 3;

    private static readonly Lazy<bool> _isPdfKitLoaded = new(() => NativeLibrary.TryLoad(PDFKIT, out _));


    /// <summary>
    /// Shows the print panel for a PDF and prints it there; returns whether the user printed.
    /// </summary>
    public static bool ShowPrintPanel(string pdfPath, MacPrintPanelOptions options)
    {
        if (!_isPdfKitLoaded.Value) throw new PlatformNotSupportedException("PDFKit is not available.");

        var url = Send(GetClass("NSURL"), "fileURLWithPath:", CreateNSString(pdfPath));
        var document = Send(Send(GetClass("PDFDocument"), "alloc"), "initWithURL:", url);
        if (document == 0) throw new IOException($"PDFKit cannot open {Path.GetFileName(pdfPath)}.");

        // a copy, so the app's shared print info keeps the system's defaults
        var printInfo = Send(Send(GetClass("NSPrintInfo"), "sharedPrintInfo"), "copy");

        try
        {
            ApplyOptions(printInfo, options);

            // pages turn to the paper chosen in the panel, never grow past it
            var operation = objc_msgSend_operation(document, Selector("printOperationForPrintInfo:scalingMode:autoRotate:"), printInfo, SCALE_DOWN_TO_FIT, true);
            if (operation == 0) return false;

            objc_msgSend_setBool(operation, Selector("setShowsPrintPanel:"), true);
            objc_msgSend_setBool(operation, Selector("setShowsProgressPanel:"), true);

            var panel = Send(operation, "printPanel");
            objc_msgSend_setLong(panel, Selector("setOptions:"), objc_msgSend_getLong(panel, Selector("options")) | SHOWS_PAPER_SIZE | SHOWS_ORIENTATION);

            return objc_msgSend_getBool(operation, Selector("runOperation"));
        }
        finally
        {
            _ = Send(printInfo, "release");
            _ = Send(document, "release");
        }
    }


    /// <summary>
    /// Starts the panel from the Print window's choices; a printer or paper the panel does not know keeps its default.
    /// </summary>
    private static void ApplyOptions(nint printInfo, MacPrintPanelOptions options)
    {
        var printer = options.PrinterName is { } name ? Send(GetClass("NSPrinter"), "printerWithName:", CreateNSString(name)) : 0;
        if (printer != 0)
        {
            _ = Send(printInfo, "setPrinter:", printer);

            // PrintCore's paper ids are NSPrintInfo's paper names
            if (options.PaperId is { } paper) _ = Send(printInfo, "setPaperName:", CreateNSString(paper));
        }

        objc_msgSend_setLong(printInfo, Selector("setOrientation:"), options.IsLandscape ? ORIENTATION_LANDSCAPE : 0);

        var copies = objc_msgSend_long(GetClass("NSNumber"), Selector("numberWithInteger:"), Math.Max(1, options.Copies));
        objc_msgSend_setObject(Send(printInfo, "dictionary"), Selector("setObject:forKey:"), copies, CreateNSString("NSCopies"));
    }


    /// <summary>
    /// Creates an autoreleased <c>NSString</c>.
    /// </summary>
    private static nint CreateNSString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value + '\0');
        fixed (byte* p = bytes)
        {
            return objc_msgSend_utf8(GetClass("NSString"), Selector("stringWithUTF8String:"), p);
        }
    }


    private static nint Send(nint receiver, string selector) => objc_msgSend(receiver, Selector(selector));


    private static nint Send(nint receiver, string selector, nint argument) => objc_msgSend_id(receiver, Selector(selector), argument);


    private static nint GetClass(string name) => objc_getClass(name);


    private static nint Selector(string name) => sel_registerName(name);


    #region ObjC runtime interop

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend_id(nint receiver, nint selector, nint argument);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend_utf8(nint receiver, nint selector, byte* utf8);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend_operation(nint receiver, nint selector, nint printInfo, nint scalingMode, [MarshalAs(UnmanagedType.U1)] bool autoRotate);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_setBool(nint receiver, nint selector, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_setLong(nint receiver, nint selector, nint value);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend_long(nint receiver, nint selector, nint value);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend_getLong(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_setObject(nint receiver, nint selector, nint value, nint key);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool objc_msgSend_getBool(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint sel_registerName(string name);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint objc_getClass(string name);

    #endregion // ObjC runtime interop
}
