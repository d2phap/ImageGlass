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
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.Ime;

namespace ImageGlass.Win32.Common;

/// <summary>
/// IMM32 helpers to keep the IME off windows that are not editing text.
/// </summary>
public static class Win32ImeApi
{
    /// <summary>
    /// Detaches the IME input context from the window, so the IME stops claiming keystrokes
    /// before the app sees them (they would arrive as <c>VK_PROCESSKEY</c>).
    /// </summary>
    public static void DetachIme(nint wndHandle)
    {
        var hWnd = new HWND(wndHandle);
        if (hWnd.IsNull) return;

        _ = PInvoke.ImmAssociateContext(hWnd, HIMC.Null);
    }
}
