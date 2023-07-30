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

using ImageGlass.Base;
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
        var colors = GetThemeColorPalatte(darkMode);

        return status switch
        {
            StatusType.Info => colors.BgInfo.WithAlpha(alpha),
            StatusType.Success => colors.BgSuccess.WithAlpha(alpha),
            StatusType.Warning => colors.BgWarning.WithAlpha(alpha),
            StatusType.Danger => colors.BgDanger.WithAlpha(alpha),
            _ => colors.BgNeutral.WithAlpha(alpha),
        };
    }


    /// <summary>
    /// Gets theme color palatte
    /// </summary>
    public static IColors GetThemeColorPalatte(bool darkMode, bool designMode = false)
    {
        return darkMode
            ? new DarkColors(designMode)
            : new LightColors(designMode);
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
        if (br.IsEmpty)
        {
            br.Width = 1;
            br.Height = 1;
        }

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

}


public enum StatusType
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger,
}
