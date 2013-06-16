/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Text;
using System.Runtime.InteropServices;

namespace ThemeConfig
{
    public class RenderTheme
    {
        [DllImport("uxtheme.dll")]
        public static extern int SetWindowTheme(
            [In] IntPtr hwnd,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszSubAppName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszSubIdList
            );


        public int ApplyTheme(System.Windows.Forms.Control control)
        {
            return this.ApplyTheme(control, "Explorer");
        }

        public int ApplyTheme(System.Windows.Forms.Control control, string theme)
        {
            try
            {
                if (control != null)
                {
                    if (control.IsHandleCreated)
                    {
                        return SetWindowTheme(control.Handle, theme, null);
                    }
                }
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        public int ClearTheme(System.Windows.Forms.Control control)
        {
            return this.ApplyTheme(control, string.Empty);
        }

    }
}
