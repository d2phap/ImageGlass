/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013-2018 DUONG DIEU PHAP
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

using ImageGlass.Services.Configuration;
using System;
using System.Windows.Forms;


namespace igcmd
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            string topcmd = args[0].ToLower().Trim();
            if (topcmd == "setwallpaper")
            {
                return Core.SetWallpaper(args); // Note: no GUI
            }

            // Windows Vista or later
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if the start up directory writable
            GlobalSetting.IsStartUpDirWritable = GlobalSetting.CheckStartUpDirWritable();

            // Enable Portable mode as default if possible
            GlobalSetting.IsPortableMode = GlobalSetting.IsStartUpDirWritable;


            if (topcmd == "igupdate")// check for update
            {
                Core.CheckForUpdate();
            }
            else if (topcmd == "igautoupdate")// auto check for update
            {
                Core.AutoUpdate();
            }
            else if (topcmd == "firstlaunch")
            {
                Application.Run(new frmFirstLaunch());
            }
            return 0;
        }

        
    }
}
