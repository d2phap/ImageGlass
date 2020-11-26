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
using System.Windows.Forms;

namespace ImageGlass.Library.WinAPI {
    public static class FormIcon {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        private const uint WM_SETICON = 0x80u;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        /// <summary>
        /// Sets icon to taskbar
        /// </summary>
        /// <param name="frm">Form</param>
        /// <param name="iconPointer">Icon handler</param>
        public static void SetTaskbarIcon(Form frm, IntPtr iconPointer) {
            SendMessage(frm.Handle, WM_SETICON, ICON_BIG, iconPointer);
        }


        /// <summary>
        /// Sets icon to window form
        /// </summary>
        /// <param name="frm">Form</param>
        /// <param name="iconPointer">Icon handler</param>
        public static void SetFormIcon(Form frm, IntPtr iconPointer) {
            SendMessage(frm.Handle, WM_SETICON, ICON_SMALL, iconPointer);
        }

    }
}
