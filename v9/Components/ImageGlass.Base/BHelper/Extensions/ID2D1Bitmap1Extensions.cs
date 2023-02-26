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
using DirectN;
using System.Runtime.InteropServices;

namespace ImageGlass.Base;

public static class ID2D1Bitmap1Extensions
{
    /// <summary>
    /// Gets pixel color.
    /// </summary>
    /// <returns>
    /// <see cref="Color.Transparent"/> if
    /// <paramref name="srcBitmap1"/> or <paramref name="dc"/> is <c>null</c>.
    /// </returns>
    public static Color GetPixelColor(this IComObject<ID2D1Bitmap1>? srcBitmap1, IComObject<ID2D1DeviceContext6>? dc, int x, int y)
    {
        if (srcBitmap1 == null || dc == null) return Color.Transparent;

        var bmpProps = new D2D1_BITMAP_PROPERTIES1()
        {
            bitmapOptions = D2D1_BITMAP_OPTIONS.D2D1_BITMAP_OPTIONS_CANNOT_DRAW | D2D1_BITMAP_OPTIONS.D2D1_BITMAP_OPTIONS_CPU_READ,
            pixelFormat = new D2D1_PIXEL_FORMAT()
            {
                alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED,
                format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            },
            dpiX = 96.0f,
            dpiY = 96.0f,
        };

        var size = srcBitmap1.GetSize().ToD2D_SIZE_U();
        using var bitmap1 = dc.CreateBitmap<ID2D1Bitmap1>(size, bmpProps);
        bitmap1.CopyFromBitmap(srcBitmap1);

        var map = bitmap1.Map(D2D1_MAP_OPTIONS.D2D1_MAP_OPTIONS_READ);
        //var totalDataSize = (int)(size.height * map.pitch);
        var startIndex = (y * map.pitch) + (x * 4);
        var lastIndex = (int)(startIndex + 4);

        var bytes = new byte[lastIndex];
        Marshal.Copy(map.bits, bytes, 0, lastIndex);
        bitmap1.Unmap();


        // since pixel data is D2D1_ALPHA_MODE_PREMULTIPLIED,
        // we need to re-calculate the color values
        var a = bytes[startIndex + 3];
        var alphaPremultiplied = a / 255f;

        var r = (byte)(bytes[startIndex + 2] / alphaPremultiplied);
        var g = (byte)(bytes[startIndex + 1] / alphaPremultiplied);
        var b = (byte)(bytes[startIndex] / alphaPremultiplied);


        var color = Color.FromArgb(a, r, g, b);

        return color;
    }

}
