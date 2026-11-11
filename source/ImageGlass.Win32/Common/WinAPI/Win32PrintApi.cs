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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace ImageGlass.Win32.Common;

public static class Win32PrintApi
{
    private const uint SEE_MASK_NOASYNC = 0x00000100;
    private const uint SEE_MASK_FLAG_NO_UI = 0x00000400;
    private const uint SEE_MASK_FLAG_LOG_USAGE = 0x04000000;


    /// <summary>
    /// Open Print Pictures dialog.
    /// </summary>
    /// <param name="filePath">File to print.</param>
    /// <exception cref="Win32Exception">Thrown when <c>ShellExecuteEx</c> fails.</exception>
    public static unsafe void OpenPrintDialog(string filePath)
    {
        fixed (char* verbPtr = "print")
        fixed (char* filePtr = filePath)
        {
            var sei = new SHELLEXECUTEINFOW
            {
                cbSize = (uint)sizeof(SHELLEXECUTEINFOW),
                fMask = SEE_MASK_NOASYNC | SEE_MASK_FLAG_NO_UI | SEE_MASK_FLAG_LOG_USAGE,
                lpVerb = verbPtr,
                lpFile = filePtr,
                nShow = 1, // SW_SHOWNORMAL
            };

            if (!PInvoke.ShellExecuteEx(ref sei))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }


}
