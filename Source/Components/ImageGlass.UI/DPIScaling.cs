﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageGlass.UI
{
    public static class DPIScaling
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern void ReleaseDC(IntPtr hWnd);

        private enum DeviceCaps
        {
            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            LOGPIXELSX = 88,

            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            LOGPIXELSY = 90,

            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            HORZRES = 8,

            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            DESKTOPHORZRES = 118
        }

        public const int WM_DPICHANGED = 0x02E0;
        public const int DPI_DEFAULT = 96;

        /// <summary>
        /// Gets, sets current DPI scaling value
        /// </summary>
        public static int CurrentDPI { get; set; } = DPI_DEFAULT;

        public static short LOWORD(int number)
        {
            return (short)number;
        }

        /// <summary>
        /// Get system Dpi.
        /// NOTE: the this.DeviceDpi property is not accurate
        /// </summary>
        /// <returns></returns>
        public static int GetSystemDpi()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);

            var val = GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSX);

            ReleaseDC(hdc);
            return val;
        }

        /// <summary>
        /// Get DPI Scale factor
        /// </summary>
        /// <returns></returns>
        public static double GetDPIScaleFactor()
        {
            return (double)CurrentDPI / DPI_DEFAULT;
        }

        /// <summary>
        /// Transform a number to a new number after applying DPI Scale Factor
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double Transform(double num)
        {
            return num * GetDPIScaleFactor();
        }

        /// <summary>
        /// Transform a number to a new number after applying DPI Scale Factor
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static int Transform(int num)
        {
            return (int)Math.Round(num * GetDPIScaleFactor());
        }

        /// <summary>
        /// Apply DPI scale factor and transform toolbar
        /// </summary>
        /// <param name="toolbar">The toolbar to update</param>
        /// <param name="baseHeight">The base height of toolbar</param>
        public static void TransformToolbar(ref ToolStripToolTip toolbar, int baseHeight)
        {
            // Update size of toolbar
            toolbar.Height = Transform(baseHeight);

            // Get new toolbar item height
            int newBtnHeight = (int)Math.Floor(toolbar.Height * 0.8);

            // Update toolbar items size of Tool bar buttons
            foreach (var item in toolbar.Items.OfType<ToolStripButton>())
            {
                item.Size = new Size(newBtnHeight, newBtnHeight);
            }

            // Update toolbar items size of Tool bar menu buttons
            foreach (var item in toolbar.Items.OfType<ToolStripDropDownButton>())
            {
                item.Size = new Size(newBtnHeight, newBtnHeight);
            }

            // get correct icon height
            var hIcon = ThemeImage.GetCorrectBaseIconHeight();

            // Tool bar separators
            foreach (var item in toolbar.Items.OfType<ToolStripSeparator>())
            {
                item.Size = new Size(5, (int)(hIcon * 1.2));
                item.Margin = new Padding((int)(hIcon * 0.15), 0, (int)(hIcon * 0.15), 0);
            }
        }
    }
}