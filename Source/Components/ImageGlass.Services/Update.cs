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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;

namespace ImageGlass.Services {
    public class Update {
        #region Properties
        /// <summary>
        /// Get / set information of info update
        /// </summary>
        public InfoUpdate Info { get; set; }

        /// <summary>
        /// Gets value if checking for update is error
        /// </summary>
        public bool IsError { get; }
        #endregion

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update(Uri link, string savedPath) {
            Info = new InfoUpdate();

            //Get information update pack
            IsError = !GetUpdateConfig(link, savedPath);
        }

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update() {
            IsError = true;
            Info = new InfoUpdate();
        }

        /// <summary>
        /// Get update data from server
        /// </summary>
        /// <param name="link"></param>
        /// <param name="savedPath"></param>
        /// <returns></returns>
        private bool GetUpdateConfig(Uri link, string savedPath) {
            //Get config file
            try {
                if (File.Exists(savedPath)) { File.Delete(savedPath); }

                var w = new WebClient();
                w.DownloadFile(link, savedPath);
            }
            catch (Exception) { return false; }

            //return FALSE if config file is not exist
            if (!File.Exists(savedPath)) { return false; }

            //Init - checking for access error
            if (!LoadUpdateConfig(savedPath))
                return false;

            //error on downloading
            if (Info.NewVersion.ToString() == "1.0.0.0") {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load update data from XML file
        /// </summary>
        /// <param name="xmlFilename"></param>
        /// <returns>false on load failure</returns>
        public bool LoadUpdateConfig(string xmlFilename) {
            try {
                var xmlDoc = new XmlDocument();
                // Issue #520: the xml document was locked somehow. Open it read-only to prevent lock issues
                using (Stream s = File.OpenRead(xmlFilename)) {
                    xmlDoc.Load(s);
                }
                var root = xmlDoc.DocumentElement;// <ImageGlass>
                var nType = (XmlElement)root.SelectNodes("Update")[0]; //<Update>
                var n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

                //Get <Info> Attributes
                Info.NewVersion = new Version(n.GetAttribute("newVersion"));
                Info.VersionType = n.GetAttribute("versionType");
                Info.Level = n.GetAttribute("level");
                Info.Link = new Uri(n.GetAttribute("link"));
                Info.Size = n.GetAttribute("size");
                Info.PublishDate = DateTime.Parse(n.GetAttribute("pubDate"));
                Info.Description = n.GetAttribute("description");
                return true;
            }
            catch (Exception) {
                // access error; corrupted file
                return false;
            }
        }

        /// <summary>
        /// Load current ImageGlass.exe file and compare to the latest version.
        /// Equal is TRUE, else FALSE
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public bool CheckForUpdate(string exePath) {
            var fv = FileVersionInfo.GetVersionInfo(exePath);
            var currentVersion = new Version(fv.FileVersion);

            // Version = [Major.Minor.Build.Revision]

            // [6.1.12.3] > [5.1.12.3]
            if (Info.NewVersion.Major > currentVersion.Major) {
                return true;
            }
            // [6.1.12.3] > [6.0.12.3]
            else if (Info.NewVersion.Major == currentVersion.Major &&
                Info.NewVersion.Minor > currentVersion.Minor) {
                return true;
            }
            // [6.1.12.3] > [6.1.10.3]
            else if (Info.NewVersion.Major == currentVersion.Major &&
                Info.NewVersion.Minor == currentVersion.Minor &&
                Info.NewVersion.Build > currentVersion.Build) {
                return true;
            }
            // [6.1.12.3] > [6.1.12.0]
            else if (Info.NewVersion.Major == currentVersion.Major &&
                Info.NewVersion.Minor == currentVersion.Minor &&
                Info.NewVersion.Build == currentVersion.Build &&
                Info.NewVersion.Revision > currentVersion.Revision) {
                return true;
            }

            //default don't need to update
            return false;
        }
    }
}
