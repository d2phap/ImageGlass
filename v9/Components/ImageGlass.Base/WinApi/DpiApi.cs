/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
    /// Transform a number after applying <see cref="DpiScale"/>
    /// </summary>
    public static T Transform<T>(T num)
    {
        var type = typeof(T);
        var value = float.Parse(num.ToString()) * DpiScale;

        return (T)Convert.ChangeType(value, type);
    }


    /// <summary>
    /// Transform padding after applying <see cref="DpiScale"/>
    /// </summary>
    public static Padding Transform(Padding padding)
    {
        return new Padding(
            Transform(padding.Left),
            Transform(padding.Top),
            Transform(padding.Right),
            Transform(padding.Bottom));
    }


    /// <summary>
    /// Transform padding after applying <see cref="DpiScale"/>
    /// </summary>
    public static SizeF Transform(SizeF size)
    {
        return new SizeF(Transform(size.Width), Transform(size.Height));
    }

}

