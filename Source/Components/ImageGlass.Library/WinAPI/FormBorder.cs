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
using System.Runtime.InteropServices;

namespace ImageGlass.Library.WinAPI {
    /// <summary>
    /// Adjust client border of the form
    /// </summary>
    public static class FormBorder {
        /// <summary>
        /// Struct for box shadow
        /// </summary>
        private struct MARGINS {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int DWMWA_NCRENDERING_POLICY = 2;

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// Set window border
        /// </summary>
        /// <param name="handle">The pointer of the form</param>
        /// <param name="borderWidth">The border width</param>
        public static void Set(IntPtr handle, int borderWidth = 1) {
            var attrValue = DWMWA_NCRENDERING_POLICY;
            DwmSetWindowAttribute(handle, DWMWA_NCRENDERING_POLICY, ref attrValue, 4);

            var margins = new MARGINS() {
                bottomHeight = borderWidth,
                leftWidth = borderWidth,
                rightWidth = borderWidth,
                topHeight = borderWidth
            };

            DwmExtendFrameIntoClientArea(handle, ref margins);
        }
    }
}
