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

using ImageGlass.Base;
using ImageGlass.Services;
using ImageGlass.Settings;
using System;
using System.IO;
using System.Windows.Forms;

namespace igcmd {
    public static class Core {
        /// <summary>
        /// Check for update
        /// </summary>
        public static bool AutoUpdate() {
            // Issue #520: intercept any possible exception and fail quietly
            try {
                Directory.CreateDirectory(App.ConfigDir(PathType.Dir, Dir.Temporary));

                var updateXML = App.ConfigDir(PathType.File, Dir.Temporary, "update.xml");
                var up = new Update(new Uri("https://imageglass.org/checkforupdate"), updateXML);

                if (File.Exists(updateXML)) {
                    File.Delete(updateXML);
                }

                if (!up.IsError &&
                    up.CheckForUpdate(App.StartUpDir("ImageGlass.exe")) &&
                    string.Equals(up.Info.VersionType, "stable", StringComparison.CurrentCultureIgnoreCase)) {
                    using (var f = new frmCheckForUpdate()) {
                        f.ShowDialog();
                    }
                }
            }
            catch { }

            return Configs.IsNewVersionAvailable;
        }

        /// <summary>
        /// Check for update
        /// </summary>
        public static bool CheckForUpdate() {
            Application.Run(new frmCheckForUpdate());

            return Configs.IsNewVersionAvailable;
        }
    }
}
