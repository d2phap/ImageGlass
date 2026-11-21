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

namespace ImageGlass.Common.Extensions;

public static class Screen_Exts
{
    extension(Screen screen)
    {
        /// <summary>
        /// Gets the work area of the screen. Read it here, never from <see cref="Screen.WorkingArea"/>,
        /// which Linux reports clipped by the panels of other monitors.
        /// </summary>
        public PixelRect GetActualWorkingArea()
        {
            return Core.ShellProvider?.GetScreenWorkingArea(screen) ?? screen.WorkingArea;
        }


        /// <summary>
        /// Gets the device pixels per physical inch, whatever the scale setting; <c>null</c> when the monitor reports no size.
        /// </summary>
        public double? GetPhysicalDpi()
        {
            var sizeMm = Core.ShellProvider?.GetScreenPhysicalSize(screen);
            if (sizeMm is not { Width: > 0, Height: > 0 } size) return null;

            // diagonals, so a rotated screen needs no axis matching
            var pixelDiagonal = double.Hypot(screen.Bounds.Width, screen.Bounds.Height);
            var inchDiagonal = double.Hypot(size.Width, size.Height) / 25.4;

            return pixelDiagonal / inchDiagonal;
        }
    }
}
