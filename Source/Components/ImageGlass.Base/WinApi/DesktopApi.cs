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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ImageGlass.Base.WinApi;


public enum WallpaperStyle : int
{
    /// <summary>
    /// Current windows wallpaper style
    /// </summary>
    Current = -1,
    Centered = 0,
    Stretched = 1,
    Tiled = 2,
}


public static partial class DesktopApi
{
    /// <summary>
    /// Set the desktop wallpaper.
    /// </summary>
    /// <param name="bmpPath">BMP image file path</param>
    /// <param name="style">Style of wallpaper</param>
    /// <returns>Success/failure indication.</returns>
    public static unsafe Exception? SetWallpaper(string bmpPath, WallpaperStyle style)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            if (key == null)
            {
                throw new Exception($"Cannot open registry key: {key}");
            }

            var tileVal = "0"; // default not-tiled
            var winStyle = "1"; // default centered

            switch (style)
            {
                case WallpaperStyle.Tiled:
                    tileVal = "1";
                    break;

                case WallpaperStyle.Stretched:
                    winStyle = "2";
                    break;

                case WallpaperStyle.Current:
                    tileVal = key.GetValue("TileWallpaper")?.ToString() ?? "";
                    winStyle = key.GetValue("WallpaperStyle")?.ToString() ?? "";
                    break;
            }

            key.SetValue("TileWallpaper", tileVal);
            key.SetValue("WallpaperStyle", winStyle);
            key.SetValue("Wallpaper", bmpPath);


            fixed (char* pathPtr = bmpPath)
            {
                _ = PInvoke.SystemParametersInfo(
                    SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETDESKWALLPAPER,
                    0, pathPtr,
                    SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_UPDATEINIFILE | SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_SENDWININICHANGE);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }

}
