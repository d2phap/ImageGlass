/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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

using System;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.Image
{
    public class DesktopWallapaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            /// <summary>
            /// Current windows wallpaper style
            /// </summary>
            Current = -1,
            /// <summary>
            /// 0
            /// </summary>
            Centered = 0,
            /// <summary>
            /// 1
            /// </summary>
            Stretched = 1,
            /// <summary>
            /// 2
            /// </summary>
            Tiled = 2
        }

        /// <summary>
        /// Set desktop wallpaper
        /// </summary>
        /// <param name="uri">Image filename</param>
        /// <param name="style">Style of wallpaper</param>
        public static void Set(Uri uri, Style style)
        {
            Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            Set(img, style);
        }

        /// <summary>
        /// Set desktop wallpaper
        /// </summary>
        /// <param name="img">Image data</param>
        /// <param name="style">Style of wallpaper</param>
        public static void Set(System.Drawing.Image img, Style style)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "imageglass.jpg");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (key == null)
                    return;

                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "0");
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", "1");
                    key.SetValue(@"TileWallpaper", "1");
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch (Exception) { }
        }
    }
}
