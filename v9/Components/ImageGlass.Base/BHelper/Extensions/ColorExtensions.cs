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

namespace ImageGlass.Base;

public static class ColorExtensions
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
        if (brightnessFactor == 0) return color;

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
    /// <param name="blendColor">Color to blend the other color onto.</param>
    /// <param name="amount">How much of the original color to keep,
    /// “on top of” <paramref name="blendColor"/>.</param>
    /// <returns>A new blended color.</returns>
    public static Color Blend(this Color c, Color blendColor, double amount = 0.5f, int alpha = 255)
    {
        byte r = (byte)(c.R * amount + blendColor.R * (1 - amount));
        byte g = (byte)(c.G * amount + blendColor.G * (1 - amount));
        byte b = (byte)(c.B * amount + blendColor.B * (1 - amount));

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
    /// Creates a <see cref="Color"/> from DWORD value.
    /// </summary>
    public static Color FromDWORD(this Color c, int dColor)
    {
        int a = (dColor >> 24) & 0xFF,
            r = (dColor >> 0) & 0xFF,
            g = (dColor >> 8) & 0xFF,
            b = (dColor >> 16) & 0xFF;

        return Color.FromArgb(a, r, g, b);
    }


    /// <summary>
    /// Converts this color <see cref="c"/> to CMYK values.
    /// </summary>
    public static int[] ToCmyk(this Color c)
    {
        if (c.R == 0 && c.G == 0 && c.B == 0)
        {
            return new[] { 0, 0, 0, 1 };
        }

        var black = Math.Min(1.0 - (c.R / 255.0), Math.Min(1.0 - (c.G / 255.0), 1.0 - (c.B / 255.0)));
        var cyan = (1.0 - (c.R / 255.0) - black) / (1.0 - black);
        var magenta = (1.0 - (c.G / 255.0) - black) / (1.0 - black);
        var yellow = (1.0 - (c.B / 255.0) - black) / (1.0 - black);

        return new[] {
            (int)Math.Round(cyan * 100),
            (int)Math.Round(magenta * 100),
            (int)Math.Round(yellow * 100),
            (int)Math.Round(black * 100)
        };
    }


    /// <summary>
    /// Converts this color <see cref="c"/> to HSLA values.
    /// </summary>
    public static float[] ToHsla(this Color c)
    {
        var h = (float)Math.Round(c.GetHue());
        var s = (float)Math.Round(c.GetSaturation() * 100);
        var l = (float)Math.Round(c.GetBrightness() * 100);
        var a = (float)Math.Round(c.A / 255.0, 3);

        return new[] { h, s, l, a };
    }


    /// <summary>
    /// Converts this color <see cref="c"/> to HSVA values.
    /// </summary>
    public static float[] ToHsva(this Color c)
    {
        int max = Math.Max(c.R, Math.Max(c.G, c.B));
        int min = Math.Min(c.R, Math.Min(c.G, c.B));

        var hue = (float)Math.Round(c.GetHue());
        var saturation = (float)Math.Round(100 * ((max == 0) ? 0 : 1f - (1f * min / max)));
        var value = (float)Math.Round(max * 100f / 255);
        var alpha = (float)Math.Round(c.A / 255.0, 3);

        return new[] { hue, saturation, value, alpha };
    }


    /// <summary>
    /// Converts this color <see cref="c"/> to HEXA values.
    /// </summary>
    /// <param name="skipAlpha"></param>
    public static string ToHex(this Color c, bool skipAlpha = false)
    {
        if (skipAlpha)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
    }


}
