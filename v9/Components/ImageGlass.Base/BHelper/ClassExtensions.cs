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

namespace ImageGlass.Base;

public static class ClassExtensions
{
    /// <summary>
    /// Creates a new color with corrected brightness.
    /// </summary>
    /// <param name="color">Color to correct.</param>
    /// <param name="brightnessFactor">The brightness correction factor.
    /// Must be between -1 and 1.
    /// Negative values produce darker colors.</param>
    public static Color WithBrightness(this Color color, float brightnessFactor)
    {
        float red = color.R;
        float green = color.G;
        float blue = color.B;

        if (brightnessFactor < 0)
        {
            brightnessFactor = 1 + brightnessFactor;
            red *= brightnessFactor;
            green *= brightnessFactor;
            blue *= brightnessFactor;
        }
        else
        {
            red = (255 - red) * brightnessFactor + red;
            green = (255 - green) * brightnessFactor + green;
            blue = (255 - blue) * brightnessFactor + blue;
        }

        return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
    }


    /// <summary>
    /// Creates a new color structure with the input alpha.
    /// </summary>
    public static Color WithAlpha(this Color c, int alpha = 255)
    {
        return Color.FromArgb(alpha, c);
    }


    /// <summary>
    /// Creates a new color structure without alpha value.
    /// </summary>
    public static Color NoAlpha(this Color c)
    {
        return c.WithAlpha(255);
    }


    /// <summary>Blends the specified colors together.</summary>
    /// <param name="c">Color to blend onto the background color.</param>
    /// <param name="backColor">Color to blend the other color onto.</param>
    /// <param name="amount">How much of <paramref name="c"/> to keep,
    /// “on top of” <paramref name="backColor"/>.</param>
    /// <returns>A new blended color.</returns>
    public static Color Blend(this Color c, Color backColor, double amount = 0.5f, int alpha = 255)
    {
        byte r = (byte)(c.R * amount + backColor.R * (1 - amount));
        byte g = (byte)(c.G * amount + backColor.G * (1 - amount));
        byte b = (byte)(c.B * amount + backColor.B * (1 - amount));

        return Color.FromArgb(alpha, r, g, b);
    }


    /// <summary>
    /// Returns <see cref="Color.White"/> if the input color's brightness is greater than <c>0.5</c>, otherwise returns <see cref="Color.Black"/>.
    /// </summary>
    public static Color BlackOrWhite(this Color c, int alpha = 255)
    {
        if (c.GetBrightness() >= 0.5f)
        {
            return Color.White.WithAlpha(alpha);
        }

        return Color.Black.WithAlpha(alpha);
    }


    /// <summary>
    /// Returns <see cref="Color.Black"/> if the input color's brightness is greater than <c>0.5</c>, otherwise returns <see cref="Color.White"/>.
    /// </summary>
    public static Color InvertBlackOrWhite(this Color c, int alpha = 255)
    {
        if (c.GetBrightness() > 0.5f)
        {
            return Color.Black.WithAlpha(alpha);
        }

        return Color.White.WithAlpha(alpha);
    }


    /// <summary>
    /// Fills rounded rectangle.
    /// </summary>
    public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF rect, float radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
    {
        using var path = BHelper.GetRoundRectanglePath(rect, radius, flatBottom, bottomOffset, flatTop);
        g.FillPath(brush, path);
    }


    /// <summary>
    /// Draws rounded rectangle.
    /// </summary>
    public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF rect, float radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
    {
        using var path = BHelper.GetRoundRectanglePath(rect, radius, flatBottom, bottomOffset, flatTop);
        g.DrawPath(pen, path);
    }

}
