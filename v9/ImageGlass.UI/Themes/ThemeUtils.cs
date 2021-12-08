using Microsoft.Win32;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace ImageGlass.UI;

public enum SystemTheme
{
    Unknown,
    Light,
    Dark
}


public partial class ThemeUtils
{

    #region PUBLIC FUNCTIONS

    /// <summary>
    /// Gets system theme (Dark or Light)
    /// </summary>
    /// <returns></returns>
    public static SystemTheme GetSystemTheme()
    {
        const string regPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string regKeyAppTheme = "AppsUseLightTheme";

        using var key = Registry.CurrentUser.OpenSubKey(regPath);
        var regValue = key?.GetValue(regKeyAppTheme);
        var themeMode = SystemTheme.Dark;

        if (regValue != null)
        {
            var themeValue = (int)regValue;

            if (themeValue > 0)
            {
                themeMode = SystemTheme.Light;
            }
        }


        return themeMode;
    }


    /// <summary>
    /// Invert the color to black or white color
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static Color InvertBlackAndWhiteColor(Color c)
    {
        if (c.GetBrightness() > 0.5)
        {
            return Color.Black;
        }

        return Color.White;
    }


    /// <summary>
    /// Convert Color to CMYK
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
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
    /// <param name="c"></param>
    /// <returns></returns>
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
    /// <param name="c"></param>
    /// <returns></returns>
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
    /// <returns></returns>
    public static string ConvertColorToHEX(Color c, bool @skipAlpha = false)
    {
        if (skipAlpha)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
    }


    /// <summary>
    /// Convert Hex (with alpha) to Color
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color ConvertHexStringToColor(string hex, bool @skipAlpha = false)
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
    /// Validate if Hex string is a valid color
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
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
    /// Lightness of the color between black (-1) and white (+1).
    /// </summary>
    /// <param name="color">The color to change the lightness.</param>
    /// <param name="factor">The factor (-1 = black ... +1 = white) to change the lightness.</param>
    /// <returns>The color with the changed lightness.</returns>
    public static Color LightnessColor(Color color, float factor)
    {
        factor = MinMax(factor, -1f, 1f);
        return factor < 0f ? DarkenColor(color, -factor) : LightenColor(color, factor);
    }


    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    /// <returns></returns>
    public static GraphicsPath GetRoundRectanglePath(RectangleF bounds, int radius)
    {
        var diameter = radius * 2;
        var size = new Size(diameter, diameter);
        var arc = new RectangleF(bounds.Location, size);
        var path = new GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // top left arc  
        path.AddArc(arc, 180, 90);

        // top right arc  
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // bottom right arc  
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // bottom left arc 
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }


    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    /// <returns></returns>
    public static GraphicsPath GetRoundRectanglePath(Rectangle bounds, int radius)
    {
        return GetRoundRectanglePath(new RectangleF(bounds.Location, bounds.Size), radius);
    }

    #endregion


    #region PRIVITE FUNCTIONS

    private static float MinMax(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    #endregion
}
