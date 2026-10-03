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
using Avalonia;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Linux.Common;

/// <summary>
/// Reads the work area of each monitor from the X11 window manager. Avalonia clips every screen by
/// <c>_NET_WORKAREA</c>, one rectangle for the whole desktop, which a panel on another monitor shrinks.
/// </summary>
public static partial class LinuxScreenApi
{
    private const string LIB_X11 = "libX11.so.6";
    private const int SUCCESS = 0;
    private const nint ANY_PROPERTY_TYPE = 0;

    private static readonly Lock _lock = new();

    // Avalonia does not share its connection, so this is a second one, opened on first use
    private static nint _display;
    private static bool _isDisplayUnavailable;


    /// <summary>
    /// Gets the work area of the monitor at <paramref name="screenBounds"/>, in desktop pixels;
    /// <c>null</c> when the window manager does not publish one per monitor.
    /// </summary>
    public static PixelRect? GetWorkingArea(PixelRect screenBounds)
    {
        lock (_lock)
        {
            try
            {
                var display = GetDisplay__();
                if (display == 0) return null;

                var workAreas = ReadWorkAreas__(display);
                if (workAreas is null) return null;

                // as GTK does: narrow the monitor by each work area that overlaps it
                PixelRect? area = null;
                foreach (var workArea in workAreas)
                {
                    var current = area ?? screenBounds;
                    if (current.Intersects(workArea)) area = current.Intersect(workArea);
                }

                return area;
            }
            catch
            {
                // no libX11 to call, so the work area the framework reports stays
                _isDisplayUnavailable = true;
                return null;
            }
        }
    }


    /// <summary>
    /// Closes the connection <see cref="GetWorkingArea"/> opened.
    /// </summary>
    public static void Close()
    {
        lock (_lock)
        {
            if (_display == 0) return;

            _ = XCloseDisplay(_display);
            _display = 0;
        }
    }



    #region Private methods

    /// <summary>
    /// Gets the connection to the X server, opened on first use; <c>0</c> when there is none.
    /// </summary>
    private static nint GetDisplay__()
    {
        if (_display != 0 || _isDisplayUnavailable) return _display;

        // the server of $DISPLAY, the one Avalonia draws on
        _display = XOpenDisplay(0);
        _isDisplayUnavailable = _display == 0;

        return _display;
    }


    /// <summary>
    /// Reads the work areas of the current desktop, one per monitor, from <c>_GTK_WORKAREAS_D&lt;n&gt;</c>.
    /// </summary>
    private static PixelRect[]? ReadWorkAreas__(nint display)
    {
        var root = XDefaultRootWindow(display);

        // 1. a property left by an earlier window manager counts only while the running one advertises it
        var workAreasAtom = XInternAtom(display, "_GTK_WORKAREAS", onlyIfExists: 1);
        if (workAreasAtom == 0) return null;

        var supportedAtoms = ReadProperty__(display, root, XInternAtom(display, "_NET_SUPPORTED", onlyIfExists: 1));
        if (supportedAtoms is null || Array.IndexOf(supportedAtoms, workAreasAtom) < 0) return null;


        // 2. one property per desktop, the first when the current one is unknown
        var currentDesktop = ReadProperty__(display, root, XInternAtom(display, "_NET_CURRENT_DESKTOP", onlyIfExists: 1));
        var desktop = currentDesktop is [var index, ..] ? index : 0;

        var desktopAtom = XInternAtom(display, $"_GTK_WORKAREAS_D{desktop}", onlyIfExists: 1);
        var values = ReadProperty__(display, root, desktopAtom);
        if (values is null || values.Length == 0 || values.Length % 4 != 0) return null;


        // 3. x, y, width, height of each
        var workAreas = new PixelRect[values.Length / 4];
        for (var i = 0; i < workAreas.Length; i++)
        {
            var offset = i * 4;
            workAreas[i] = new PixelRect((int)values[offset], (int)values[offset + 1],
                (int)values[offset + 2], (int)values[offset + 3]);
        }

        return workAreas;
    }


    /// <summary>
    /// Reads a 32-bit property of <paramref name="window"/>, which Xlib hands back as C longs; <c>null</c> when unset.
    /// </summary>
    private static unsafe nint[]? ReadProperty__(nint display, nint window, nint atom)
    {
        // asking for no atom is a protocol error
        if (atom == 0) return null;

        var result = XGetWindowProperty(display, window, atom, 0, int.MaxValue, delete: 0, ANY_PROPERTY_TYPE,
            out var actualType, out var actualFormat, out var itemCount, out var bytesAfter, out var data);

        // a failed request sets none of the outputs, so there is nothing to free either
        if (result != SUCCESS) return null;

        try
        {
            if (data == 0 || actualType == 0 || actualFormat != 32 || bytesAfter != 0) return null;

            return new ReadOnlySpan<nint>((void*)data, (int)itemCount).ToArray();
        }
        finally
        {
            if (data != 0) _ = XFree(data);
        }
    }

    #endregion // Private methods



    #region Native methods

    [LibraryImport(LIB_X11)]
    private static partial nint XOpenDisplay(nint displayName);

    [LibraryImport(LIB_X11)]
    private static partial int XCloseDisplay(nint display);

    [LibraryImport(LIB_X11)]
    private static partial nint XDefaultRootWindow(nint display);

    [LibraryImport(LIB_X11, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint XInternAtom(nint display, string atomName, int onlyIfExists);

    [LibraryImport(LIB_X11)]
    private static partial int XGetWindowProperty(nint display, nint window, nint property, nint longOffset,
        nint longLength, int delete, nint reqType, out nint actualType, out int actualFormat,
        out nint itemCount, out nint bytesAfter, out nint data);

    [LibraryImport(LIB_X11)]
    private static partial int XFree(nint data);

    #endregion // Native methods

}
