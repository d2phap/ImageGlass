/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017-2018 DUONG DIEU PHAP
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
using System;
using System.Linq;
using System.Windows.Forms;
using igtasks;
using ImageGlass.Library.Image;
using ImageGlass.Services.Configuration;

namespace adtasks
{
    static class Program
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

            //Command
            string topcmd = args[0].ToLower().Trim();


            //Set desktop wallpaper
            #region setwallpaper <string imgPath> [int style]
            if (topcmd == "setwallpaper")
            {
                //Get image's path
                string imgPath = args[1];
                var result = 0;

                if (args.Length > 2)
                {
                    //Get style
                    string style = args[2];

                    //Apply changes
                    if (style == "1")
                    {
                        result = (int)DesktopWallapaper.Set(imgPath, DesktopWallapaper.Style.Stretched);
                    }
                    else if (style == "2")
                    {
                        result = (int)DesktopWallapaper.Set(imgPath, DesktopWallapaper.Style.Tiled);
                    }
                    else //style == "0"
                    {
                        result = (int)DesktopWallapaper.Set(imgPath, DesktopWallapaper.Style.Current);
                    }
                }
                else
                {
                    //Apply changes
                    result = (int)DesktopWallapaper.Set(imgPath, DesktopWallapaper.Style.Centered);
                }

                return result;
            }
            #endregion

            //Register file associations
            #region regassociations <string exts> [--no-ui]
            else if (topcmd == "regassociations")
            {
                //get Extensions
                string exts = args[1];
                bool isNoUI = args.FirstOrDefault(i => i == "--no-ui") == null ? false : true;

                Functions.SetRegistryAssociations(exts, isNoUI);

                Application.Exit();
            }
            #endregion

            //Delete all file associations
            #region delassociations
            else if (topcmd == "delassociations")
            {
                Functions.DeleteRegistryAssociations(GlobalSetting.AllImageFormats, true);

                Application.Exit();
            }
            #endregion

            //Install new language packs
            #region iginstalllang
            else if (topcmd == "iginstalllang")
            {
                Functions.InstallLanguagePacks();
                Application.Exit();
            }
            #endregion

            //Create new language packs
            #region ignewlang
            else if (topcmd == "ignewlang")
            {
                Functions.CreateNewLanguagePacks();
                Application.Exit();
            }
            #endregion

            //Edit language packs
            #region igeditlang <string filename>
            else if (topcmd == "igeditlang")
            {
                //get Executable file
                string filename = args[1];

                Functions.EditLanguagePacks(filename);
                Application.Exit();
            }
            #endregion

            //Install new extensions
            #region iginstallext
            else if (topcmd == "iginstallext")
            {
                Functions.InstallExtensions();
                Application.Exit();
            }
            #endregion



            return 0;
        }



    }
}
