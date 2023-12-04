/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace ImageGlass.Base.WinApi;

public static class DpiApi
{
    private static int _currentDpi = DPI_DEFAULT;

    public const int WM_DPICHANGED = 0x02E0;
    public const int DPI_DEFAULT = 96;

    /// <summary>
    /// Occurs when the <see cref="CurrentDpi"/> is changed.
    /// </summary>
    public static event DpiChangedHandler? OnDpiChanged = null;
    public delegate void DpiChangedHandler();


    /// <summary>
    /// Gets, sets current DPI scaling value
    /// </summary>
    public static int CurrentDpi
    {
        get => _currentDpi;
        set
        {
            _currentDpi = value;
            OnDpiChanged?.Invoke();
        }
    }


    /// <summary>
    /// Get DPI Scale factor.
    /// </summary>
    public static float DpiScale => (float)CurrentDpi / DPI_DEFAULT;


    /// <summary>
    /// Scales a number after applying <see cref="DpiScale"/>
    /// </summary>
    public static T Scale<T>(T num, float? dpiScale = null)
    {
        dpiScale ??= DpiScale;

        var type = typeof(T);
        var value = float.Parse(num.ToString()) * dpiScale;

        return (T)Convert.ChangeType(value, type);
    }


    /// <summary>
    /// Scales padding after applying <see cref="DpiScale"/>
    /// </summary>
    public static Padding Scale(Padding padding, float? dpiScale = null)
    {
        return new Padding(
            Scale(padding.Left, dpiScale),
            Scale(padding.Top, dpiScale),
            Scale(padding.Right, dpiScale),
            Scale(padding.Bottom, dpiScale));
    }


    /// <summary>
    /// Scales padding after applying <see cref="DpiScale"/>
    /// </summary>
    public static SizeF Scale(SizeF size, float? dpiScale = null)
    {
        return new SizeF(Scale(size.Width, dpiScale), Scale(size.Height, dpiScale));
    }



    /// <summary>
    /// Gets DPI scale from a screen.
    /// </summary>
    public static float GetDpiScale(this Screen screen)
    {
        var point = new Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);

        return GetDpiScale(point);
    }


    /// <summary>
    /// Gets DPI scale from a point on the screen.
    /// </summary>
    public static float GetDpiScale(Point screenPoint)
    {
        var hMonitor = PInvoke.MonitorFromPoint(screenPoint, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        _ = PInvoke.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);


        return (float)dpiX / DPI_DEFAULT;
    }

}

