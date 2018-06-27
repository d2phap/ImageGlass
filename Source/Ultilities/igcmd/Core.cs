/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
using System.IO;
using System.Windows.Forms;
using ImageGlass.Services;
using ImageGlass.Services.Configuration;
using ImageGlass.Library.Image;

namespace igcmd
{
    public static class Core
    {


        /// <summary>
        /// Check for update
        /// </summary>
        public static void AutoUpdate()
        {
            string updateXML = Path.Combine(GlobalSetting.StartUpDir, "update.xml");
            Update up = new Update(new Uri("http://www.imageglass.org/checkforupdate"), updateXML);

            if (File.Exists(updateXML))
            {
                File.Delete(updateXML);
            }

            if (!up.IsError &&
                up.CheckForUpdate(Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe")) &&
                up.Info.VersionType.ToLower() == "stable")
            {
                frmCheckForUpdate f = new frmCheckForUpdate();
                f.ShowDialog();
            }

            Application.Exit();
        }


        /// <summary>
        /// Check for update
        /// </summary>
        public static void CheckForUpdate()
        {
            Application.Run(new frmCheckForUpdate());
        }

        /// <summary>
        /// Set desktop wallpaper - without administrator rights.
        /// Not using admin allows using network shares which are not
        /// connected for the Admin account.
        /// If this fails, invoke igtasks to attempt again.
        /// </summary>
        internal static int SetWallpaper(string[] args)
        {
            if (args.Length < 2)
                return (int)DesktopWallapaper.Result.Success; // Failed to provide image path (but 'success')
            string imgPath = args[1];
            DesktopWallapaper.Style style = DesktopWallapaper.Style.Centered;
            if (args.Length > 2)
            {
                if (!Enum.TryParse(args[2], out style))
                    style = DesktopWallapaper.Style.Current;
            }
            var result = DesktopWallapaper.Set(imgPath, style);

            // TODO attempt to clear local policy settings

            return (int)result;
        }

    }
}
