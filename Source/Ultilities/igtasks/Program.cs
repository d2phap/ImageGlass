﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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

using igtasks;
using ImageGlass.Library.Image;
using ImageGlass.Settings;
using System;
using System.Windows.Forms;

namespace adtasks
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string[] args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] argv)
        {
            // Windows Vista or later
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            args = argv;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load user configs
            Configs.Load();

            // Command
            string topcmd = args[0].ToLower().Trim();

            // Set desktop wallpaper

            #region setwallpaper <string imgPath> [int style]

            if (topcmd == "setwallpaper")
            {
                //Get image's path
                string imgPath = args[1];
                var style = DesktopWallapaper.Style.Current;

                if (args.Length > 2)
                {
                    //Get style
                    Enum.TryParse(args[2], out style);
                }

                //Apply changes and return exit code
                return (int)DesktopWallapaper.Set(imgPath, style);
            }

            #endregion setwallpaper <string imgPath> [int style]

            // Register file associations

            #region regassociations <string exts>

            else if (topcmd == "regassociations")
            {
                //get Extensions
                string exts = args[1];

                return Functions.SetRegistryAssociations(exts);
            }

            #endregion regassociations <string exts>

            // Delete all file associations

            #region delassociations

            else if (topcmd == "delassociations")
            {
                var formats = Configs.GetImageFormats(Configs.AllFormats);
                return Functions.DeleteRegistryAssociations(formats, true);
            }

            #endregion delassociations

            // Install new language packs

            #region iginstalllang

            else if (topcmd == "iginstalllang")
            {
                Functions.InstallLanguagePacks();
            }

            #endregion iginstalllang

            // Create new language packs

            #region ignewlang

            else if (topcmd == "ignewlang")
            {
                Functions.CreateNewLanguagePacks();
            }

            #endregion ignewlang

            // Edit language packs

            #region igeditlang <string filename>

            else if (topcmd == "igeditlang")
            {
                //get Executable file
                string filename = args[1];

                Functions.EditLanguagePacks(filename);
            }

            #endregion igeditlang <string filename>

            // Register URI Scheme for Web-to-App linking

            #region reg-uri-scheme

            else if (topcmd == "reg-uri-scheme")
            {
                return Functions.SetURIScheme();
            }

            #endregion reg-uri-scheme

            // Delete URI Scheme registry

            #region del-uri-scheme

            else if (topcmd == "del-uri-scheme")
            {
                return Functions.DeleteURIScheme();
            }

            #endregion del-uri-scheme

            return 0;
        }
    }
}