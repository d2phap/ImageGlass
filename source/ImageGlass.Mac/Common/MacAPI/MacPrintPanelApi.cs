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
/// Shows the macOS print panel for a PDF, through PDFKit and AppKit, which must run on the main thread.
/// </summary>
internal static unsafe partial class MacPrintPanelApi
{
    private const string LIB_OBJC = "/usr/lib/libobjc.dylib";
    private const string PDFKIT = "/System/Library/Frameworks/PDFKit.framework/PDFKit";

    // PDFPrintScalingMode: shrink a page only when the paper chosen in the panel is smaller
    private const nint SCALE_DOWN_TO_FIT = 2;

    private static readonly Lazy<bool> _isPdfKitLoaded = new(() => NativeLibrary.TryLoad(PDFKIT, out _));


    /// <summary>
    /// Shows the print panel for a PDF and prints it there; returns whether the user printed.
    /// </summary>
    public static bool ShowPrintPanel(string pdfPath)
    {
        if (!_isPdfKitLoaded.Value) throw new PlatformNotSupportedException("PDFKit is not available.");

        var url = Send(GetClass("NSURL"), "fileURLWithPath:", CreateNSString(pdfPath));
        var document = Send(Send(GetClass("PDFDocument"), "alloc"), "initWithURL:", url);
        if (document == 0) throw new IOException($"PDFKit cannot open {Path.GetFileName(pdfPath)}.");

        try
        {
            // pages turn to the paper chosen in the panel, never grow past it
            var printInfo = Send(GetClass("NSPrintInfo"), "sharedPrintInfo");
            var operation = objc_msgSend_operation(document, Selector("printOperationForPrintInfo:scalingMode:autoRotate:"), printInfo, SCALE_DOWN_TO_FIT, true);
            if (operation == 0) return false;

            objc_msgSend_setBool(operation, Selector("setShowsPrintPanel:"), true);
            objc_msgSend_setBool(operation, Selector("setShowsProgressPanel:"), true);

            return objc_msgSend_getBool(operation, Selector("runOperation"));
        }
        finally
        {
            _ = Send(document, "release");
        }
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
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool objc_msgSend_getBool(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint sel_registerName(string name);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint objc_getClass(string name);

    #endregion // ObjC runtime interop
}
