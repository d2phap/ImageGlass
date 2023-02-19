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

using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace ImageGlass.UI;


public partial class ThemeUtils
{

    #region PUBLIC FUNCTIONS

    /// <summary>
    /// Gets the background color for the input status.
    /// </summary>
    public static Color GetBackgroundColorForStatus(StatusType status, bool darkMode = true, int alpha = 255)
    {
        if (darkMode)
        {
            // dark color palette
            return status switch
            {
                StatusType.Info => Color.FromArgb(alpha, 20, 44, 59),
                StatusType.Success => Color.FromArgb(alpha, 34, 59, 42),
                StatusType.Warning => Color.FromArgb(alpha, 59, 40, 10),
                StatusType.Danger => Color.FromArgb(alpha, 59, 20, 19),
                _ => Color.FromArgb(alpha, 32, 38, 43),
            };
        }

        // light color palette
        return status switch
        {
            StatusType.Info => Color.FromArgb(alpha, 199, 238, 255),
            StatusType.Success => Color.FromArgb(alpha, 219, 255, 242),
            StatusType.Warning => Color.FromArgb(alpha, 255, 239, 219),
            StatusType.Danger => Color.FromArgb(alpha, 255, 222, 222),
            _ => Color.FromArgb(alpha, 242, 242, 242),
        };
    }


    /// <summary>
    /// Gets theme color palatte
    /// </summary>
    public static IColors GetThemeColorPalatte(bool darkMode)
    {
        return darkMode
            ? new DarkColors()
            : new LightColors();
    }


    /// <summary>
    /// Parses DWord color to <see cref="Color"/>.
    /// </summary>
    /// <param name="dColor">DWord color</param>
    public static Color ParseDWordColor(int dColor)
    {
        int a = (dColor >> 24) & 0xFF,
            r = (dColor >> 0) & 0xFF,
            g = (dColor >> 8) & 0xFF,
            b = (dColor >> 16) & 0xFF;

        return Color.FromArgb(a, r, g, b);
    }


    /// <summary>
    /// Convert Color to CMYK
    /// </summary>
    public static int[] ConvertColorToCMYK(Color c)
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
                (int) Math.Round(cyan*100),
                (int) Math.Round(magenta*100),
                (int) Math.Round(yellow*100),
                (int) Math.Round(black*100)
            };
    }


    /// <summary>
    /// Convert Color to HSLA
    /// </summary>
    public static float[] ConvertColorToHSLA(Color c)
    {
        var h = (float)Math.Round(c.GetHue());
        var s = (float)Math.Round(c.GetSaturation() * 100);
        var l = (float)Math.Round(c.GetBrightness() * 100);
        var a = (float)Math.Round(c.A / 255.0, 3);

        return new[] { h, s, l, a };
    }


    /// <summary>
    /// Convert Color to HSVA
    /// </summary>
    public static float[] ConvertColorToHSVA(Color c)
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
    /// Convert Color to HEX (with alpha)
    /// </summary>
    /// <param name="c"></param>
    /// <param name="skipAlpha"></param>
    public static string ColorToHex(Color c, bool @skipAlpha = false)
    {
        if (skipAlpha)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
    }


    /// <summary>
    /// Creates a new <see cref="Color"/> from the given hex color string (with alpha).
    /// </summary>
    public static Color ColorFromHex(string hex, bool skipAlpha = false)
    {
        // Remove # if present
        if (hex.IndexOf('#') != -1)
        {
            hex = hex.Replace("#", "");
        }

        var red = 0;
        var green = 0;
        var blue = 0;
        var alpha = 255;

        if (hex.Length == 8)
        {
            // #RRGGBBAA
            red = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            green = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            alpha = int.Parse(hex.Substring(6, 2), NumberStyles.AllowHexSpecifier);
        }
        else if (hex.Length == 6)
        {
            // #RRGGBB
            red = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            green = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            blue = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
        }
        else if (hex.Length == 4)
        {
            // #RGBA
            red = int.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
            green = int.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
            blue = int.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
            alpha = int.Parse($"{hex[3]}{hex[3]}", NumberStyles.AllowHexSpecifier);
        }
        else if (hex.Length == 3)
        {
            // #RGB
            red = int.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
            green = int.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
            blue = int.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
        }

        if (skipAlpha)
        {
            alpha = 255;
        }

        return Color.FromArgb(alpha, red, green, blue);
    }


    /// <summary>
    /// Validates if the input hex color string is a valid color.
    /// </summary>
    public static bool IsValidHex(string hex)
    {
        if (hex.StartsWith("#"))
        {
            return hex.Length == 9 || hex.Length == 7 || hex.Length == 5 || hex.Length == 4;
        }

        return false;
    }


    /// <summary>
    /// Makes the color lighter by the given factor (0 = no change, 1 = white).
    /// </summary>
    /// <param name="color">The color to make lighter.</param>
    /// <param name="factor">The factor to make the color lighter (0 = no change, 1 = white).</param>
    /// <returns>The lighter color.</returns>
    public static Color LightenColor(Color color, float factor)
    {
        const float min = 0.001f;
        const float max = 1.999f;

        return ControlPaint.Light(color, min + (MinMax(factor, 0f, 1f) * (max - min)));
    }


    /// <summary>
    /// Makes the color darker by the given factor (0 = no change, 1 = black).
    /// </summary>
    /// <param name="color">The color to make darker.</param>
    /// <param name="factor">The factor to make the color darker (0 = no change, 1 = black).</param>
    /// <returns>The darker color.</returns>
    public static Color DarkenColor(Color color, float factor)
    {
        const float min = -0.5f;
        const float max = 1f;

        return ControlPaint.Dark(color, min + (MinMax(factor, 0f, 1f) * (max - min)));
    }


    /// <summary>
    /// Creates image from text.
    /// </summary>
    public static Image CreateImageFromText(string text, Font font, Color textColor, Color? backColor = null)
    {
        // fix dpi scaling
        var fontSize = FontPixelToPoint(font.Height);

        var path = new GraphicsPath();
        path.AddString(text, font.FontFamily, (int)font.Style, fontSize, new Point(0, 0), StringFormat.GenericTypographic);

        var pathRect = path.GetBounds();
        var br = new RectangleF(pathRect.X, pathRect.Y, pathRect.Width, pathRect.Height);
        var img = new Bitmap((int)Math.Ceiling(br.Width) + 1, (int)Math.Ceiling(br.Height));

        using var textBrush = new SolidBrush(textColor);
        using var g = Graphics.FromImage(img);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.Clear(backColor ?? Color.Transparent);

        g.TranslateTransform(
            (img.Width - br.Width) / 2 - br.X,
            (float)Math.Floor((img.Height - br.Height) / 2 - br.Y));
        g.FillPath(textBrush, path);
        g.Save();


        return img;
    }


    /// <summary>
    /// Converts font size in point to pixel.
    /// </summary>
    /// <param name="pt">Font size in point</param>
    public static float FontPointToPixel(int pt)
    {
        // 16px = 12pt
        return pt * 16f / 12f;
    }


    /// <summary>
    /// Converts font size in pixel to point.
    /// </summary>
    /// <param name="px">Font size in pixel</param>
    public static float FontPixelToPoint(float px)
    {
        // 16px = 12pt
        return px * 12f / 16f;
    }

    #endregion


    #region PRIVITE FUNCTIONS

    private static float MinMax(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    #endregion
}


public enum StatusType
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger,
}
