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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Microsoft.Win32;
using System.Runtime.InteropServices;

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


public enum WallpaperResult
{
    /// <summary>
    /// Wallpaper successfully set
    /// </summary>
    Success = 0,

    /// <summary>
    /// Wallpaper failure due to privileges - can re-attempt with Admin privileges.
    /// </summary>
    PrivilegesFail,

    /// <summary>
    /// Wallpaper failure - no point in re-attempting
    /// </summary>
    Fail,
}


public static class DesktopApi
{
    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDWININICHANGE = 0x02;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);


    /// <summary>
    /// Set the desktop wallpaper.
    /// </summary>
    /// <param name="bmpPath">BMP image file path</param>
    /// <param name="style">Style of wallpaper</param>
    /// <returns>Success/failure indication.</returns>
    public static WallpaperResult SetWallpaper(string bmpPath, WallpaperStyle style)
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key == null)
                {
                    return WallpaperResult.Fail;
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
            }

            _ = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, bmpPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        catch (Exception ex)
        {
            if (ex is System.Security.SecurityException ||
                ex is UnauthorizedAccessException)
            {
                return WallpaperResult.PrivilegesFail;
            }

            return WallpaperResult.Fail;
        }

        return WallpaperResult.Success;
    }

}
