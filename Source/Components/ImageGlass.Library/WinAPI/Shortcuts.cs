/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

/*********************************
 * Access the target path from a Windows Shortcut.
 * Based on code from
 * https://astoundingprogramming.wordpress.com/2012/12/17/how-to-get-the-target-of-a-windows-shortcut-c/
 ********************************/

namespace ImageGlass.Library.WinAPI {
    public static class Shortcuts {
        /// <summary>
        /// Get the target path from shortcut (*.lnk)
        /// </summary>
        /// <param name="shortcutPath">Path of shortcut (*.lnk)</param>
        /// <returns></returns>
        public static string GetTargetPathFromShortcut(string shortcutPath) {
            var shell = new IWshRuntimeLibrary.WshShell();

            try {
                var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortcut.TargetPath;
            }
            catch //(COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
            }
        }
    }
}
