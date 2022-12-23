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
using System.Runtime.InteropServices;
using System.Text;

namespace ImageGlass.Base.WinApi;

public class KeyboardApi
{
    [DllImport("user32.dll")]
    static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);


    /// <summary>
    /// Convert key code to unicode string
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string KeyCodeToUnicode(Keys key)
    {
        var keyboardState = new byte[255];
        var keyboardStateStatus = GetKeyboardState(keyboardState);

        if (!keyboardStateStatus)
        {
            return "";
        }

        var virtualKeyCode = (uint)key;
        var scanCode = MapVirtualKey(virtualKeyCode, 0);
        var inputLocaleIdentifier = GetKeyboardLayout(0);

        var result = new StringBuilder();
        _ = ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, 5, 0, inputLocaleIdentifier);

        return result.ToString();
    }
}
