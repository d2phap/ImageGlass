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
using System.Runtime.InteropServices;

namespace ImageGlass.Mac.Common;

/// <summary>
/// AppKit event helpers, called through the ObjC runtime.
/// </summary>
public static partial class MacEventApi
{
    private const string LIB_OBJC = "/usr/lib/libobjc.dylib";

    private static readonly Lazy<nint> _sharedAppSel = new(() => sel_registerName("sharedApplication"));
    private static readonly Lazy<nint> _currentEventSel = new(() => sel_registerName("currentEvent"));
    private static readonly Lazy<nint> _hasPreciseSel = new(() => sel_registerName("hasPreciseScrollingDeltas"));
    private static readonly Lazy<nint> _nsAppClass = new(() => objc_getClass("NSApplication"));


    /// <summary>
    /// Whether the current event scrolls with precise (trackpad) deltas.
    /// </summary>
    public static bool HasPreciseScrollingDeltas()
    {
        var app = objc_msgSend(_nsAppClass.Value, _sharedAppSel.Value);
        var currentEvent = objc_msgSend(app, _currentEventSel.Value);
        if (currentEvent == 0) return false;

        return objc_msgSend_bool(currentEvent, _hasPreciseSel.Value);
    }


    #region ObjC runtime interop

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    private static partial nint objc_msgSend(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool objc_msgSend_bool(nint receiver, nint selector);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint sel_registerName(string name);

    [LibraryImport(LIB_OBJC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint objc_getClass(string name);

    #endregion // ObjC runtime interop
}
