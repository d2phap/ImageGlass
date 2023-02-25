/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

using System.Drawing.Imaging;

namespace ImageGlass.Base;

public static class BitmapExtensions
{
    /// <summary>
    /// Gets pixel color.
    /// </summary>
    /// <returns>
    /// <see cref="Color.Transparent"/> if the <paramref name="bmp"/> is <c>null</c>.
    /// </returns>
    public static Color GetPixelColor(this Bitmap? bmp, int x, int y)
    {
        if (bmp == null) return Color.Transparent;

        unsafe
        {
            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            var firstPixelPtr = (byte*)bitmapData.Scan0;
            var position = (bitmapData.Stride * y) + (x * 4);

            var color = Color.FromArgb(
                firstPixelPtr[position + 3],
                firstPixelPtr[position + 2],
                firstPixelPtr[position + 1],
                firstPixelPtr[position + 0]);

            bmp.UnlockBits(bitmapData);

            return color;
        }
    }
}
