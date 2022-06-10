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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageGlass.Base.WinApi;

public enum SystemIconType
{
    Warning = 101,
    Help = 102,
    Error = 103,
    Info = 104,
    Shield = 106,
}

public static class ImageApi
{

    internal enum GDI_IMAGE_TYPE : uint
    {
        IMAGE_BITMAP = 0U,
        IMAGE_CURSOR = 2U,
        IMAGE_ICON = 1U,
    }

    [Flags]
    internal enum IMAGE_FLAGS : uint
    {
        LR_CREATEDIBSECTION = 0x00002000,
        LR_DEFAULTCOLOR = 0x00000000,
        LR_DEFAULTSIZE = 0x00000040,
        LR_LOADFROMFILE = 0x00000010,
        LR_LOADMAP3DCOLORS = 0x00001000,
        LR_LOADTRANSPARENT = 0x00000020,
        LR_MONOCHROME = 0x00000001,
        LR_SHARED = 0x00008000,
        LR_VGACOLOR = 0x00000080,
        LR_COPYDELETEORG = 0x00000008,
        LR_COPYFROMRESOURCE = 0x00004000,
        LR_COPYRETURNORG = 0x00000004,
    }


    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);


    /// <summary>
    /// Loads the system icon.
    /// </summary>
    /// <param name="type">The type of icon.</param>
    /// <param name="size">The size.</param>
    /// <returns>The icon.</returns>
    /// <exception cref="System.PlatformNotSupportedException"></exception>
    public static Icon? LoadSystemIcon(SystemIconType type, Size size)
    {
        var hIcon = LoadImage(IntPtr.Zero,
            $"#{(int)type}",
            (uint)GDI_IMAGE_TYPE.IMAGE_ICON,
            size.Width, size.Height,
            (uint)IMAGE_FLAGS.LR_DEFAULTCOLOR);

        return hIcon == IntPtr.Zero ? null : Icon.FromHandle(hIcon);
    }

}
