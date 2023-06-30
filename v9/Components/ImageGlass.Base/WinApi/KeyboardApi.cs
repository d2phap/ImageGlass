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
    static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern bool ToAsciiEx(int virtualKey, int scanCode, byte[] lpKeyState, ref uint lpChar, int uFlags, IntPtr dwhkl);

    [DllImport("user32.dll")]
    private static extern short VkKeyScanExA(char ch, IntPtr dwhkl);


    /// <summary>
    /// Converts virtual key to string.
    /// </summary>
    public static char KeyCodeToChar(Keys key, bool withShiftKey)
    {
        uint lpChar = 0;
        var lpKeyState = new byte[256];

        if (withShiftKey)
        {
            var mKey = Keys.ShiftKey;

            foreach (Keys sKey in Enum.GetValues(typeof(Keys)))
                if ((mKey & sKey) == sKey)
                    lpKeyState[(int)sKey] = 0x80;
        }

        var virtualKeyCode = (uint)key;
        var scanCode = MapVirtualKey(virtualKeyCode, 0);
        var keyboardLayoutPtr = GetKeyboardLayout(0);

        _ = ToAsciiEx((int)key, (int)scanCode, lpKeyState, ref lpChar, 0, keyboardLayoutPtr);

        return (char)lpChar;
    }


    /// <summary>
    /// Convert character to virtual key
    /// </summary>
    public static Keys CharToKeyCode(char c)
    {
        var keyboardLayoutPtr = GetKeyboardLayout(0);

        var vkey = VkKeyScanExA(c, keyboardLayoutPtr);
        var keys = (Keys)(vkey & 0xff);
        var modifiers = vkey >> 8;

        if ((modifiers & 1) != 0) keys |= Keys.Shift;
        if ((modifiers & 2) != 0) keys |= Keys.Control;
        if ((modifiers & 4) != 0) keys |= Keys.Alt;

        return keys;
    }

}
