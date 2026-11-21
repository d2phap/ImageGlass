/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace ImageGlass.Win32.Common;

/// <summary>
/// Reads the physical properties of each monitor.
/// </summary>
public static class Win32ScreenApi
{
    // the platform handle Avalonia gives a Windows screen
    private const string HANDLE_HMONITOR = "HMonitor";


    /// <summary>
    /// Gets the physical size of <paramref name="screen"/> in millimetres; <c>null</c> when the monitor reports none.
    /// </summary>
    public static Size? GetPhysicalSize(Screen screen)
    {
        var handle = screen.TryGetPlatformHandle();
        if (handle is not { HandleDescriptor: HANDLE_HMONITOR }) return null;

        // raw DPI is the panel's own density from its EDID, unlike the effective DPI the scale setting picks
        var hr = PInvoke.GetDpiForMonitor(new HMONITOR(handle.Handle), MONITOR_DPI_TYPE.MDT_RAW_DPI, out var dpiX, out var dpiY);
        if (hr.Failed || dpiX == 0 || dpiY == 0) return null;

        var widthMm = screen.Bounds.Width * 25.4 / dpiX;
        var heightMm = screen.Bounds.Height * 25.4 / dpiY;

        return new Size(widthMm, heightMm);
    }
}
