/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using System.Runtime.InteropServices;

namespace ImageGlass.UI.WinApi;

public static class DPIScaling
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern void ReleaseDC(IntPtr hWnd);


    private enum DeviceCaps
    {
        /// <summary>
        /// Horizontal width in pixels
        /// </summary>
        HORZRES = 8,

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,

        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90,

        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118
    }

    public const int WM_DPICHANGED = 0x02E0;
    public const int DPI_DEFAULT = 96;


    /// <summary>
    /// Gets, sets current DPI scaling value
    /// </summary>
    public static int CurrentDpi { get; set; } = DPI_DEFAULT;

    /// <summary>
    /// Get DPI Scale factor
    /// </summary>
    /// <returns></returns>
    public static double DpiScale => (double)CurrentDpi / DPI_DEFAULT;


    /// <summary>
    /// Transform a number to a new number after applying DPI Scale Factor
    /// </summary>
    /// <param name="num">A float number</param>
    /// <returns></returns>
    public static T Transform<T>(float num)
    {
        var type = typeof(T);
        var value = num * DpiScale;

        return (T)Convert.ChangeType(value, type);
    }

    /// <summary>
    /// Transform a number to a new number after applying DPI Scale Factor
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static int Transform(int num)
    {
        return (int)Math.Round(num * DpiScale);
    }

}

