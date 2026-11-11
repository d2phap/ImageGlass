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
using Microsoft.Win32;
using System;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ImageGlass.Win32.Common;

public enum WallpaperStyle : int
{
    /// <summary>
    /// Use current wallpaper style
    /// </summary>
    Current = -1,

    Fill,       // 10, 0
    Fit,        // 6, 0
    Stretch,    // 2, 0
    Tile,       // 0, 1
    Center,     // 0, 0
    Span,       // 22, 0 (for multi-monitor)
}


public static partial class Win32DesktopApi
{
    /// <summary>
    /// Sets the desktop wallpaper.
    /// </summary>
    /// <param name="filePath">Image file path</param>
    /// <param name="style">Style of wallpaper</param>
    /// <exception cref="Exception"></exception>
    public static unsafe void SetWallpaper(string filePath, WallpaperStyle style)
    {
        // 1. open registry
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true);
        if (key is null)
        {
            throw new InvalidOperationException($"IGE: Cannot open registry key: {key}");
        }


        // 2. get the wallpaper style
        (string bgStyle, string tileStyle) = style switch
        {
            WallpaperStyle.Fill => ("10", "0"),
            WallpaperStyle.Fit => ("6", "0"),
            WallpaperStyle.Stretch => ("2", "0"),
            WallpaperStyle.Tile => ("0", "1"),
            WallpaperStyle.Center => ("0", "0"),
            WallpaperStyle.Span => ("22", "0"),
            _ => ("-1", "0"),
        };

        // 3. check if we should use the current style
        if (bgStyle == "-1")
        {
            bgStyle = key.GetValue("WallpaperStyle")?.ToString() ?? "";
            tileStyle = key.GetValue("TileWallpaper")?.ToString() ?? "";
        }


        // 4. set wallpaper
        key.SetValue("WallpaperStyle", bgStyle);
        key.SetValue("TileWallpaper", tileStyle);
        key.SetValue("Wallpaper", filePath);

        fixed (char* pathPtr = filePath)
        {
            _ = PInvoke.SystemParametersInfo(
                SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETDESKWALLPAPER,
                0, pathPtr,
                SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_UPDATEINIFILE | SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_SENDWININICHANGE);
        }
    }

}
