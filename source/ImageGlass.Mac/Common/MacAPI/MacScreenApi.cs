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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Avalonia;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;

namespace ImageGlass.Mac.Common;

/// <summary>
/// Reads the physical properties of each display from CoreGraphics.
/// </summary>
public static partial class MacScreenApi
{
    private const string CORE_GRAPHICS = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    // the platform handle Avalonia gives a macOS screen
    private const string HANDLE_DISPLAY_ID = "CGDirectDisplayID";

    // the density CoreGraphics assumes for a display without EDID: 72 DPI
    private const double ASSUMED_POINTS_PER_MM = 2.835;


    /// <summary>
    /// Gets the physical size of <paramref name="screen"/> in millimetres; <c>null</c> when the display reports none.
    /// </summary>
    public static Size? GetPhysicalSize(Screen screen)
    {
        var handle = screen.TryGetPlatformHandle();
        if (handle is not { HandleDescriptor: HANDLE_DISPLAY_ID }) return null;
        var displayId = (uint)handle.Handle;

        try
        {
            var size = CGDisplayScreenSize(displayId);
            if (size.Width <= 0 || size.Height <= 0) return null;

            // without EDID the size is made up from the point width at 72 DPI, which is no measurement
            var estimatedWidth = CGDisplayBounds(displayId).Width / ASSUMED_POINTS_PER_MM;
            if (Math.Abs(size.Width - estimatedWidth) < 1) return null;

            return new Size(size.Width, size.Height);
        }
        catch
        {
            return null;
        }
    }



    #region CoreGraphics interop

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double Width;
        public double Height;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }


    [LibraryImport(CORE_GRAPHICS)]
    private static partial CGSize CGDisplayScreenSize(uint display);

    [LibraryImport(CORE_GRAPHICS)]
    private static partial CGRect CGDisplayBounds(uint display);

    #endregion // CoreGraphics interop
}
