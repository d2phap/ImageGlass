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
using Avalonia.Media;
using System;

namespace ImageGlass.Common.Extensions;


public static class Color_Exts
{
    extension(Color c)
    {
        /// <summary>
        /// Checks if this color is empty (all values are <c>0</c>).
        /// </summary>
        public bool IsEmpty => c.R == 0 && c.G == 0 && c.B == 0 && c.A == 0;

        public Color A(byte a) => Color.FromArgb(a, c.R, c.G, c.B);
        public Color R(byte r) => Color.FromArgb(c.A, r, c.G, c.B);
        public Color G(byte g) => Color.FromArgb(c.A, c.R, g, c.B);
        public Color B(byte b) => Color.FromArgb(c.A, c.R, c.G, b);
    }


    /// <summary>
    /// Converts to <see cref="SolidColorBrush"/>.
    /// <c>NOTE:</c> Must call from UI thread.
    /// </summary>
    public static SolidColorBrush ToBrush(this Color c)
    {
        return new SolidColorBrush(c);
    }


    /// <summary>
    /// Get brightness value from the given color using Relative Luminance (W3C Standard),
    /// returns a value between 0.0 and 1.0
    /// </summary>
    public static double GetBrightness(this Color c)
    {
        double r = c.R / 255.0;
        double g = c.G / 255.0;
        double b = c.B / 255.0;

        r = (r <= 0.03928) ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        g = (g <= 0.03928) ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        b = (b <= 0.03928) ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

        // Returns a value between 0.0 and 1.0
        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
    }


    /// <summary>
    /// Creates a new color with corrected brightness.
    /// </summary>
    /// <param name="color">Color to correct.</param>
    /// <param name="factor">The brightness correction factor.
    /// Must be between -1 and 1.
    /// Negative values produce darker colors.</param>
    public static Color WithBrightness(this Color color, float factor)
    {
        if (factor == 0) return color;

        float r = color.R;
        float g = color.G;
        float b = color.B;

        // Perceptual Grayscale/Luma baseline
        float luma = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);

        if (factor < 0)
        {
            // --- DARKENING (Windows 11 Light Mode Accent Shades) ---
            // To prevent muddy tones, Windows boosts saturation up to 25% when darkening
            float saturationBoost = 1.0f + (Math.Abs(factor) * 0.25f);

            r = Math.Clamp(luma + (r - luma) * saturationBoost, 0f, 255f);
            g = Math.Clamp(luma + (g - luma) * saturationBoost, 0f, 255f);
            b = Math.Clamp(luma + (b - luma) * saturationBoost, 0f, 255f);

            // Apply darkness scaling 
            float darkenScale = 1.0f + factor;
            r *= darkenScale;
            g *= darkenScale;
            b *= darkenScale;
        }
        else
        {
            // --- LIGHTENING (Windows 11 Dark Mode Accent Shades) ---
            // To prevent washed-out chalkiness, Windows dampens the saturation shift 
            // and uses a smooth perceptual blend toward white
            float saturationDampen = 1.0f - (factor * 0.15f);

            r = Math.Clamp(luma + (r - luma) * saturationDampen, 0f, 255f);
            g = Math.Clamp(luma + (g - luma) * saturationDampen, 0f, 255f);
            b = Math.Clamp(luma + (b - luma) * saturationDampen, 0f, 255f);

            // Interpolate smoothly to pure white
            r += (255f - r) * factor;
            g += (255f - g) * factor;
            b += (255f - b) * factor;
        }

        return Color.FromArgb(
            color.A,
            (byte)Math.Round(r),
            (byte)Math.Round(g),
            (byte)Math.Round(b)
        );
    }


    /// <summary>
    /// Creates a new color structure with the input alpha.
    /// </summary>
    public static Color WithAlpha(this Color c, int alpha = 255)
    {
        return Color.FromArgb((byte)alpha, c.R, c.G, c.B);
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

        return Color.FromArgb((byte)alpha, r, g, b);
    }


    /// <summary>
    /// Returns <see cref="Color.White"/> if the input color's brightness is greater than <c>0.5</c>, otherwise returns <see cref="Color.Black"/>.
    /// </summary>
    public static Color BlackOrWhite(this Color c, int alpha = 255)
    {
        if (c.GetBrightness() >= 0.5f)
        {
            return Colors.White.WithAlpha(alpha);
        }

        return Colors.Black.WithAlpha(alpha);
    }


    /// <summary>
    /// Returns <see cref="Color.Black"/> if the input color's brightness is greater than <c>0.5</c>, otherwise returns <see cref="Color.White"/>.
    /// </summary>
    public static Color InvertBlackOrWhite(this Color c, int alpha = 255)
    {
        if (c.GetBrightness() > 0.5f)
        {
            return Colors.Black.WithAlpha(alpha);
        }

        return Colors.White.WithAlpha(alpha);
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

        return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
    }


    /// <summary>
    /// Converts this color to RGBA array.
    /// </summary>
    public static byte[] ToRgbaArray(this Color c)
    {
        return [c.R, c.G, c.B, c.A];
    }


    /// <summary>
    /// Converts this color to RGBA values.
    /// </summary>
    public static string ToRgbaString(this Color c, bool skipAlpha = false)
    {
        var arr = c.ToRgbaArray();
        var alpha = (double)Math.Round(c.A / 255f, 2);

        var str = $"{arr[0]}, {arr[1]}, {arr[2]}";
        if (!skipAlpha) str += $", {alpha}";

        return str;
    }


    /// <summary>
    /// Converts this color to CMYK values.
    /// </summary>
    public static double[] ToCmyk(this Color c)
    {
        if (c.R == 0 && c.G == 0 && c.B == 0) return [0, 0, 0, 1, 0];

        var black = Math.Min(1.0 - (c.R / 255.0), Math.Min(1.0 - (c.G / 255.0), 1.0 - (c.B / 255.0)));
        var cyan = (1.0 - (c.R / 255.0) - black) / (1.0 - black);
        var magenta = (1.0 - (c.G / 255.0) - black) / (1.0 - black);
        var yellow = (1.0 - (c.B / 255.0) - black) / (1.0 - black);

        var _c = (int)Math.Round(cyan * 100);
        var _m = (int)Math.Round(magenta * 100);
        var _y = (int)Math.Round(yellow * 100);
        var _k = (int)Math.Round(black * 100);
        var alpha = (double)Math.Round(c.A / 255f, 2);

        return [_c, _m, _y, _k, alpha];
    }


    /// <summary>
    /// Converts this color to CMYK string.
    /// </summary>
    public static string ToCmykString(this Color c, bool skipAlpha = false)
    {
        var arr = c.ToCmyk();
        var str = $"{arr[0]}%, {arr[1]}%, {arr[2]}%, {arr[3]}%";

        if (!skipAlpha) str += $", {arr[4]}";

        return str;
    }


    /// <summary>
    /// Converts this color to HSL string.
    /// </summary>
    public static string ToHslString(this Color c, bool skipAlpha = false)
    {
        var c2 = c.ToHsl();
        var h = Math.Round(c2.H);
        var s = Math.Round(c2.S * 100);
        var l = Math.Round(c2.L * 100);
        var alpha = Math.Round(c2.A, 2);

        var str = $"{h}, {s}%, {l}%";
        if (!skipAlpha) str += $", {alpha}";

        return str;
    }


    /// <summary>
    /// Converts this color to HSV string.
    /// </summary>
    public static string ToHsvString(this Color c, bool skipAlpha = false)
    {
        var c2 = c.ToHsv();
        var h = Math.Round(c2.H);
        var s = Math.Round(c2.S * 100);
        var v = Math.Round(c2.V * 100);
        var alpha = Math.Round(c2.A, 2);

        var str = $"{h}, {s}%, {v}%";
        if (!skipAlpha) str += $", {alpha}";

        return str;
    }


    /// <summary>
    /// Converts this color to CIELAB string.
    /// </summary>
    public static string ToCIELABString(this Color c, bool skipAlpha = false)
    {
        var arr = c.ToCIELAB(skipAlpha);

        return string.Join(", ", arr);
    }


    /// <summary>
    /// Converts this color to HEXA values.
    /// </summary>
    public static string ToHex(this Color c, bool skipAlpha = false)
    {
        if (skipAlpha)
        {
            return String.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
    }


    /// <summary>
    /// Converts this color to CIELAB.
    /// </summary>
    public static double[] ToCIELAB(this Color c, bool skipAlpha = false)
    {
        var xyz = new double[3];
        var rgb = new double[3] { c.R / 255d, c.G / 255d, c.B / 255d };

        if (rgb[0] > .04045f) rgb[0] = Math.Pow((rgb[0] + 0.055) / 1.055, 2.4);
        else rgb[0] = rgb[0] / 12.92f;

        if (rgb[1] > .04045f) rgb[1] = Math.Pow((rgb[1] + 0.055) / 1.055, 2.4);
        else rgb[1] = rgb[1] / 12.92f;

        if (rgb[2] > .04045f) rgb[2] = Math.Pow((rgb[2] + 0.055) / 1.055, 2.4);
        else rgb[2] = rgb[2] / 12.92f;


        rgb[0] = rgb[0] * 100.0f;
        rgb[1] = rgb[1] * 100.0f;
        rgb[2] = rgb[2] * 100.0f;


        xyz[0] = ((rgb[0] * 0.412453f) + (rgb[1] * 0.357580f) + (rgb[2] * 0.180423f));
        xyz[1] = ((rgb[0] * 0.212671f) + (rgb[1] * 0.715160f) + (rgb[2] * 0.072169f));
        xyz[2] = ((rgb[0] * 0.019334f) + (rgb[1] * 0.119193f) + (rgb[2] * 0.950227f));


        xyz[0] = xyz[0] / 95.047f;
        xyz[1] = xyz[1] / 100.0f;
        xyz[2] = xyz[2] / 108.883f;


        if (xyz[0] > .008856f) xyz[0] = (float)Math.Pow(xyz[0], (1.0 / 3.0));
        else xyz[0] = (xyz[0] * 7.787f) + (16.0f / 116.0f);

        if (xyz[1] > .008856f) xyz[1] = (float)Math.Pow(xyz[1], 1.0 / 3.0);
        else xyz[1] = (xyz[1] * 7.787f) + (16.0f / 116.0f);

        if (xyz[2] > .008856f) xyz[2] = (float)Math.Pow(xyz[2], 1.0 / 3.0);
        else xyz[2] = (xyz[2] * 7.787f) + (16.0f / 116.0f);


        var l = (double)Math.Round((116.0f * xyz[1]) - 16.0f, 2);
        var a = (double)Math.Round(500.0f * (xyz[0] - xyz[1]), 2);
        var b = (double)Math.Round(200.0f * (xyz[1] - xyz[2]), 2);


        if (skipAlpha) return [l, a, b];
        else
        {
            var alpha = (double)Math.Round(c.A / 255f, 2);
            return [l, a, b, alpha];
        }
    }

}
