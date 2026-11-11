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
using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ImageGlass.Win32.Common;

public static class Win32WindowApi
{
    // WM_SETICON / WM_GETICON wParam; the caption bar draws the small icon
    private const int ICON_SMALL = 0;
    private const int ICON_BIG = 1;
    private const int BLANK_ICON_SIZE = 16;
    private const int BLANK_ICON_BYTES = BLANK_ICON_SIZE * BLANK_ICON_SIZE / 8; // 1bpp

    // DWMWA_USE_IMMERSIVE_DARK_MODE, before it moved to 20 in Windows 10 build 18985
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

    // the first Windows 10 build that can draw a dark title bar, and the one the attribute moved on
    private const int BUILD_DARK_TITLE_BAR = 17763;
    private const int BUILD_DARK_TITLE_BAR_MOVED = 18985;

    private static HICON _blankIcon;


    /// <summary>
    /// Sets window backdrop.
    /// </summary>
    public static void SetWindowBackdrop(nint wndHandle, SystemBackdropType type = SystemBackdropType.Auto)
    {
        unsafe
        {
            _ = PInvoke.DwmSetWindowAttribute(new HWND(wndHandle),
               DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
               &type, sizeof(uint));
        }
    }


    /// <summary>
    /// Draws the window title bar in dark or light colors. Avalonia only applies this on
    /// Windows 11, so a Windows 10 title bar stays light without it.
    /// </summary>
    public unsafe static void SetTitleBarDarkMode(nint wndHandle, bool isDark)
    {
        var hWnd = new HWND(wndHandle);
        if (hWnd.IsNull) return;

        var build = Environment.OSVersion.Version.Build;
        if (build < BUILD_DARK_TITLE_BAR) return;

        var attribute = build >= BUILD_DARK_TITLE_BAR_MOVED
            ? DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE
            : (DWMWINDOWATTRIBUTE)DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

        var value = isDark ? 1 : 0;
        var result = PInvoke.DwmSetWindowAttribute(hWnd, attribute, &value, sizeof(int));
        if (result.Failed) return;

        RedrawTitleBar(hWnd);
    }


    /// <summary>
    /// Repaints the title bar: on Windows 10 a dark mode change only reaches the non-client area on
    /// the next activation change, so re-assert the current one.
    /// </summary>
    private static void RedrawTitleBar(HWND hWnd)
    {
        nuint isActive = PInvoke.GetForegroundWindow() == hWnd ? 1u : 0u;

        _ = PInvoke.SendMessage(hWnd, PInvoke.WM_NCACTIVATE, isActive ^ 1u, 0);
        _ = PInvoke.SendMessage(hWnd, PInvoke.WM_NCACTIVATE, isActive, 0);
    }


    /// <summary>
    /// Shows / hides the app icon on the window title bar. Only the caption (small) icon is
    /// swapped for a transparent one, so the taskbar and Alt+Tab icons are kept.
    /// </summary>
    public unsafe static void SetTitleBarIconVisible(nint wndHandle, bool visible)
    {
        var hWnd = new HWND(wndHandle);
        if (hWnd.IsNull) return;

        var smallIcon = PInvoke.SendMessage(hWnd, PInvoke.WM_GETICON, ICON_SMALL, 0).Value;
        var isHidden = !_blankIcon.IsNull && smallIcon == (nint)_blankIcon.Value;

        // showing: keep the icon Avalonia set, or take the big (taskbar) one - which Windows
        // scales down - when coming back from hidden
        var newIcon = visible
            ? (isHidden ? PInvoke.SendMessage(hWnd, PInvoke.WM_GETICON, ICON_BIG, 0).Value : smallIcon)
            : (nint)GetBlankIcon().Value;

        // the caption redraws on WM_SETICON only when the handle changes, so clear it first:
        // a re-assert is what brings the icon back after the window regains its decorations
        if (newIcon == smallIcon)
        {
            _ = PInvoke.SendMessage(hWnd, PInvoke.WM_SETICON, ICON_SMALL, 0);
        }

        _ = PInvoke.SendMessage(hWnd, PInvoke.WM_SETICON, ICON_SMALL, newIcon);
    }


    /// <summary>
    /// Gets the process-wide 16x16 fully transparent icon, creating it on first use.
    /// It is never destroyed: the windows using it live as long as the process.
    /// </summary>
    private static HICON GetBlankIcon()
    {
        if (!_blankIcon.IsNull) return _blankIcon;

        unsafe
        {
            // all-1s AND mask + all-0s XOR bitmap = fully transparent
            var maskBits = stackalloc byte[BLANK_ICON_BYTES];
            var colorBits = stackalloc byte[BLANK_ICON_BYTES];
            for (var i = 0; i < BLANK_ICON_BYTES; i++)
            {
                maskBits[i] = 0xFF;
                colorBits[i] = 0x00;
            }

            var hMask = PInvoke.CreateBitmap(BLANK_ICON_SIZE, BLANK_ICON_SIZE, 1, 1, maskBits);
            var hColor = PInvoke.CreateBitmap(BLANK_ICON_SIZE, BLANK_ICON_SIZE, 1, 1, colorBits);

            try
            {
                var iconInfo = new ICONINFO
                {
                    fIcon = true,
                    hbmMask = hMask,
                    hbmColor = hColor,
                };

                // CreateIconIndirect copies the bitmaps, so they can be freed right after
                _blankIcon = PInvoke.CreateIconIndirect(&iconInfo);
            }
            finally
            {
                _ = PInvoke.DeleteObject(hMask);
                _ = PInvoke.DeleteObject(hColor);
            }
        }

        return _blankIcon;
    }


}


/// <summary>
/// <c>DWM_SYSTEMBACKDROP_TYPE</c>
/// </summary>
public enum SystemBackdropType
{
    /// <summary>
    /// <c>DWMSBT_AUTO</c>:
    /// Let OS decides.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// <c>DWMSBT_NONE</c>:
    /// No effect.
    /// </summary>
    None = 1,

    /// <summary>
    /// <c>DWMSBT_MAINWINDOW</c>:
    /// Mica effect.
    /// </summary>
    Mica = 2,

    /// <summary>
    /// <c>DWMSBT_TRANSIENTWINDOW</c>:
    /// Acrylic effect.
    /// </summary>
    Acrylic = 3,

    /// <summary>
    /// <c>DWMSBT_TABBEDWINDOW</c>:
    /// Draw the backdrop material effect corresponding to a window with a tabbed title bar.
    /// </summary>
    MicaAlt = 4,
}