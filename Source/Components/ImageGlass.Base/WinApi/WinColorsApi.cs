/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using Microsoft.Win32;
using Windows.UI.ViewManagement;

namespace ImageGlass.Base.WinApi;

public class WinColorsApi
{
    /// <summary>
    /// Gets system accent color.
    /// </summary>
    public static Color GetAccentColor(bool includeAlpha)
    {
        var ui = new UISettings();
        var accent = ui.GetColorValue(UIColorType.Accent);

        var alpha = includeAlpha ? accent.A : (byte)255;
        var color = Color.FromArgb(alpha, accent.R, accent.G, accent.B);

        return color;
    }


    /// <summary>
    /// Checks if the system transparency is enabled.
    /// </summary>
    public static bool IsTransparencyEnabled
    {
        get
        {
            const string regPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string regKey = "EnableTransparency";
            var enabled = true;

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(regPath);
                var regValue = key?.GetValue(regKey);

                if (regValue != null)
                {
                    var themeValue = (int)regValue;

                    if (themeValue == 0)
                    {
                        enabled = false;
                    }
                }
            }
            catch { }

            return enabled;
        }
    }


    /// <summary>
    /// Checks if the app color mode is dark (default).
    /// </summary>
    public static bool IsDarkMode
    {
        get
        {
            const string regPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string regKey = "AppsUseLightTheme";
            var darkMode = true;

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(regPath);
                var regValue = key?.GetValue(regKey);

                if (regValue != null)
                {
                    var themeValue = (int)regValue;

                    if (themeValue > 0)
                    {
                        darkMode = false;
                    }
                }
            }
            catch { }

            return darkMode;
        }
    }

}
