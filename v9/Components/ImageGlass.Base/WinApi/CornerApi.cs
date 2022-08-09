/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

using Windows.Win32;
using Windows.Win32.Graphics.Dwm;

namespace ImageGlass.Base.WinApi;

public class CornerApi
{
    /// <summary>
    /// The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, 
    /// which tells the function what value of the enum to set.
    /// </summary>
    private enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3,
    }


    /// <summary>
    /// Apply rounded corners for Windows 11
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    public static void ApplyCorner(IntPtr handle)
    {
        if (!BHelper.IsOS(WindowsOS.Win11))
        {
            return;
        }

        unsafe
        {
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

            PInvoke.DwmSetWindowAttribute(new(handle),
                DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                &preference, sizeof(uint));
        }
    }
}
