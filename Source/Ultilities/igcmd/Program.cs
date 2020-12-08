/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using ImageGlass.Library.Image;
using ImageGlass.Settings;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace igcmd {
    internal static class Program {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        // Issue #360: IG periodically searching for dismounted device
        [DllImport("kernel32.dll")]
        private static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes: uint {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOGPFAULTERRORBOX = 1 << 1,
            SEM_NOALIGNMENTFAULTEXCEPT = 1 << 2,
            SEM_NOOPENFILEERRORBOX = 1 << 15
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args) {
            // Issue #360: IG periodically searching for dismounted device
            // This _must_ be executed first!
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);

            var topcmd = args[0].ToLower().Trim();

            // Windows Vista or later
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load user configs
            Configs.Load();

            // Set desktop wallpaper
            #region setwallpaper <string imgPath> [int style]
            if (topcmd == "setwallpaper") {
                // Get image's path
                var imgPath = args[1];
                var style = DesktopWallapaper.Style.Current;

                if (args.Length > 2) {
                    // Get style
                    Enum.TryParse(args[2], out style);
                }

                // Apply changes and return exit code
                return (int)DesktopWallapaper.Set(imgPath, style);
            }
            #endregion

            // check for update
            else if (topcmd == "igupdate") {
                return Core.CheckForUpdate() ? 1 : 0;
            }

            // auto check for update
            else if (topcmd == "igautoupdate") {
                return Core.AutoUpdate() ? 1 : 0;
            }

            // run first launch configs
            else if (topcmd == "firstlaunch") {
                Application.Run(new frmFirstLaunch());
            }

            return 0;
        }
    }
}
