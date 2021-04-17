/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using ImageGlass.Base;

namespace ImageGlass.Library.WinAPI {
    public static class Explorer {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);


        /// <summary>
        /// Open folder and select item
        /// </summary>
        /// <param name="filePath"></param>
        public static void OpenFolderAndSelectItem(string filePath) {

            var folderPath = Path.GetDirectoryName(filePath);
            SHParseDisplayName(folderPath, IntPtr.Zero, out var nativeFolder, 0, out _);

            if (nativeFolder == IntPtr.Zero) {
                App.LogIt($"Explorer.OpenFolderAndSelectItem: Cannot find native folder for '{folderPath}'");
                throw new Exception($"Cannot find native folder for '{folderPath}'");
            }

            SHParseDisplayName(filePath, IntPtr.Zero, out var nativeFile, 0, out _);

            IntPtr[] fileArray;
            if (nativeFile == IntPtr.Zero) {
                // Open the folder without the file selected if we can't find the file
                fileArray = new IntPtr[0];
            }
            else {
                fileArray = new IntPtr[] { nativeFile };
            }

            SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

            Marshal.FreeCoTaskMem(nativeFolder);
            if (nativeFile != IntPtr.Zero) {
                Marshal.FreeCoTaskMem(nativeFile);
            }
        }
    }
}
