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
using ImageGlass.Common.Types;
using System;
using System.Globalization;


namespace ImageGlass.Common;

public partial class BHelper
{

    /// <summary>
    /// Creates a new <see cref="Color"/> from the given hex color string (with alpha),
    /// supports parsing <c>accent:opacity</c> sytax.
    /// </summary>
    /// <returns>
    /// <c>Color(0,0,0,0)</c> if the <paramref name="colorStr"/> is invalid.
    /// </returns>
    public static Color ColorFromHex(string colorStr, Color? accent = null, bool skipAlpha = false)
    {
        // not using accent color
        if (!colorStr.StartsWith(Const.THEME_SYSTEM_ACCENT, StringComparison.OrdinalIgnoreCase))
        {
            return ColorFromHex(colorStr, skipAlpha);
        }


        // example: accent:180
        var valueArr = colorStr.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        byte accentR = 0;
        byte accentG = 0;
        byte accentB = 0;
        byte accentA = 255;

        if (accent != null)
        {
            accentR = accent.Value.R;
            accentG = accent.Value.G;
            accentB = accent.Value.B;
            accentA = accent.Value.A;
        }

        // adjust accent color alpha
        if (!skipAlpha && valueArr.Length > 1)
        {
            _ = byte.TryParse(valueArr[1], out byte accentAlpha);
            accentA = accentAlpha;
        }

        return Color.FromArgb(accentA, accentR, accentG, accentB);

    }


    /// <summary>
    /// Creates a new <see cref="Color"/> from the given hex color string (with alpha).
    /// </summary>
    /// <returns>
    /// <c>Color(0,0,0,0)</c> if the <paramref name="hex"/> is invalid.
    /// </returns>
    public static Color ColorFromHex(string hex, bool skipAlpha)
    {
        if (string.IsNullOrWhiteSpace(hex)) return new Color();

        byte colorR = 0;
        byte colorG = 0;
        byte colorB = 0;
        byte colorA = 0;

        // remove # if present
        hex = hex.TrimStart('#');

        try
        {
            if (hex.Length == 8)
            {
                // #RRGGBBAA
                colorR = byte.Parse(hex.AsSpan(0, 2), NumberStyles.AllowHexSpecifier);
                colorG = byte.Parse(hex.AsSpan(2, 2), NumberStyles.AllowHexSpecifier);
                colorB = byte.Parse(hex.AsSpan(4, 2), NumberStyles.AllowHexSpecifier);
                colorA = byte.Parse(hex.AsSpan(6, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hex.Length == 6)
            {
                // #RRGGBB
                colorR = byte.Parse(hex.AsSpan(0, 2), NumberStyles.AllowHexSpecifier);
                colorG = byte.Parse(hex.AsSpan(2, 2), NumberStyles.AllowHexSpecifier);
                colorB = byte.Parse(hex.AsSpan(4, 2), NumberStyles.AllowHexSpecifier);
                colorA = 255;
            }
            else if (hex.Length == 4)
            {
                // #RGBA
                colorR = byte.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
                colorG = byte.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
                colorB = byte.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
                colorA = byte.Parse($"{hex[3]}{hex[3]}", NumberStyles.AllowHexSpecifier);
            }
            else if (hex.Length == 3)
            {
                // #RGB
                colorR = byte.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
                colorG = byte.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
                colorB = byte.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
                colorA = 255;
            }
            else
            {
                return new Color();
            }

            if (skipAlpha) colorA = 255;
        }
        catch { }

        return Color.FromArgb(colorA, colorR, colorG, colorB);
    }


}
