﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013-2018 DUONG DIEU PHAP
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
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.Image {
    public static class DesktopWallapaper {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style: int {
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
        public static void Set(Uri uri, Style style) {
            var s = new System.Net.WebClient().OpenRead(uri.ToString());

            var img = System.Drawing.Image.FromStream(s);
            Set(img, style);
        }

        /// <summary>
        /// Set desktop wallpaper
        /// </summary>
        /// <param name="img">Image data</param>
        /// <param name="style">Style of wallpaper</param>
        public static void Set(System.Drawing.Image img, Style style) {
            try {
                var tempPath = Path.Combine(Path.GetTempPath(), "imageglass.jpg");
                img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (key == null) {
                    return;
                }

                if (style == Style.Stretched) {
                    key.SetValue("WallpaperStyle", "2");
                    key.SetValue("TileWallpaper", "0");
                }

                if (style == Style.Centered) {
                    key.SetValue("WallpaperStyle", "1");
                    key.SetValue("TileWallpaper", "0");
                }

                if (style == Style.Tiled) {
                    key.SetValue("WallpaperStyle", "1");
                    key.SetValue("TileWallpaper", "1");
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch { }
        }

        public enum Result {
            Success = 0, // Wallpaper successfully set
            PrivsFail, // Wallpaper failure due to privileges - can re-attempt with Admin privs.
            Fail       // Wallpaper failure - no point in re-attempting
        }

        /// <summary>
        /// Set the desktop wallpaper.
        /// </summary>
        /// <param name="path">File system path to the image</param>
        /// <param name="style">Style of wallpaper</param>
        /// <returns>Success/failure indication.</returns>
        public static Result Set(string path, Style style) {
            //System.Diagnostics.Debugger.Break();

            // TODO use ImageMagick to load image
            var tempPath = Path.Combine(Path.GetTempPath(), "imageglass.bmp");
            try {
                using (var img = System.Drawing.Image.FromFile(path)) {
                    // SPI_SETDESKWALLPAPER will *only* work consistently if image is a Bitmap! (Issue #327)
                    img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
            catch {
                // Couldn't open/save image file: Fail, and don't re-try
                return Result.Fail;
            }

            try {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true)) {
                    if (key == null) {
                        return Result.Fail;
                    }

                    var tileVal = "0"; // default not-tiled
                    var winStyle = "1"; // default centered
                    switch (style) {
                        case Style.Tiled:
                            tileVal = "1";
                            break;
                        case Style.Stretched:
                            winStyle = "2";
                            break;
                        case Style.Current:
                            tileVal = (string)key.GetValue("TileWallpaper");
                            winStyle = (string)key.GetValue("WallpaperStyle");
                            break;
                    }
                    key.SetValue("TileWallpaper", tileVal);
                    key.SetValue("WallpaperStyle", winStyle);
                    key.SetValue("Wallpaper", tempPath);
                }
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch (Exception ex) {
                if (ex is System.Security.SecurityException ||
                    ex is UnauthorizedAccessException) {
                    return Result.PrivsFail;
                }

                return Result.Fail;
            }
            return Result.Success;
        }
    }
}
