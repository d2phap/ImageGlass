/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.Theme
{
    public static class DPIScaling
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        const int LOGPIXELSX = 88;

        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        const int LOGPIXELSY = 90;

        public const int WM_DPICHANGED = 0x02E0;

        public static short LOWORD(int number)
        {
            return (short)number;
        }

        public static int CalculateCurrentDPI()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);

            return GetDeviceCaps(hdc, LOGPIXELSX);
        }

        public static void HandleDpiChanged(int oldDpi, int currentDpi, Form f)
        {
            if (oldDpi != 0)
            {
                float scaleFactor = (float)currentDpi / oldDpi;

                //the default scaling method of the framework
                f.Scale(new SizeF(scaleFactor, scaleFactor));


                //fonts are not scaled automatically so we need to handle this manually
                //ScaleFontForControl(f, scaleFactor);
            }
        }

        private static void ScaleFontForControl(Control control, float factor)
        {
            control.Font = new Font(control.Font.FontFamily,
                   control.Font.Size * factor,
                   control.Font.Style);

            foreach (Control child in control.Controls)
            {
                ScaleFontForControl(child, factor);
            }
        }

    }
}
