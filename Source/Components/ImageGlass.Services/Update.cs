/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using System.Xml;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace ImageGlass.Services
{
    public class Update
    {
        private InfoUpdate _info;
        private bool _isError;

        #region Properties
        /// <summary>
        /// Get / set information of info update
        /// </summary>
        public InfoUpdate Info
        {
            get { return _info; }
            set { _info = value; }
        }

        /// <summary>
        /// Gets value if checking for update is error
        /// </summary>
        public bool IsError
        {
            get { return _isError; }
        }
        #endregion

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update(Uri link, string savedPath)
        {
            _info = new InfoUpdate();

            //Get information update pack
            _isError = !GetUpdateConfig(link, savedPath);
        }

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update()
        {
            _isError = true;
            _info = new InfoUpdate();
        }


        /// <summary>
        /// Get update data from server
        /// </summary>
        /// <param name="link"></param>
        /// <param name="savedPath"></param>
        /// <returns></returns>
        private bool GetUpdateConfig(Uri link, string savedPath)
        {
            //Get config file
            try
            {
                if (File.Exists(savedPath)) { File.Delete(savedPath); }

                System.Net.WebClient w = new WebClient();
                w.DownloadFile(link, savedPath);
            }
            catch (Exception ex) { return false; }

            //return FALSE if config file is not exist
            if (!File.Exists(savedPath)) { return false; }

            //Init - checking for access error
            if (!LoadUpdateConfig(savedPath))
                return false;

            //error on downloading
            if (_info.NewVersion.ToString() == "1.0.0.0")
            {
                return false;
            }

            return true;    
        }

        /// <summary>
        /// Load update data from XML file
        /// </summary>
        /// <param name="xmlFilename"></param>
        /// <returns>false on load failure</returns>
        public bool LoadUpdateConfig(string xmlFilename)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                // Issue #520: the xml document was locked somehow. Open it read-only to prevent lock issues
                using (Stream s = File.OpenRead(xmlFilename))
                {
                    xmlDoc.Load(s);
                }
                XmlElement root = xmlDoc.DocumentElement;// <ImageGlass>
                XmlElement nType = (XmlElement)root.SelectNodes("Update")[0]; //<Update>
                XmlElement n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

                //Get <Info> Attributes
                _info.NewVersion = new Version(n.GetAttribute("newVersion"));
                _info.VersionType = n.GetAttribute("versionType");
                _info.Level = n.GetAttribute("level");
                _info.Link = new Uri(n.GetAttribute("link"));
                _info.Size = n.GetAttribute("size");
                _info.PublishDate = DateTime.Parse(n.GetAttribute("pubDate"));
                _info.Decription = n.GetAttribute("decription");
                return true;
            }
            catch (Exception ex)
            {
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
        public bool CheckForUpdate(string exePath)
        {
            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(exePath);
            Version currentVersion = new Version(fv.FileVersion);

            // Version = [Major.Minor.Build.Revision]

            // [6.1.12.3] > [5.1.12.3]
            if (Info.NewVersion.Major > currentVersion.Major)
            {
                return true;
            }
            // [6.1.12.3] > [6.0.12.3]
            else if (Info.NewVersion.Major == currentVersion.Major && 
                Info.NewVersion.Minor > currentVersion.Minor)
            {
                return true;
            }
            // [6.1.12.3] > [6.1.10.3]
            else if (Info.NewVersion.Major == currentVersion.Major &&
                Info.NewVersion.Minor == currentVersion.Minor &&
                Info.NewVersion.Build > currentVersion.Build)
            {
                return true;
            }
            // [6.1.12.3] > [6.1.12.0]
            else if (Info.NewVersion.Major == currentVersion.Major &&
                Info.NewVersion.Minor == currentVersion.Minor &&
                Info.NewVersion.Build == currentVersion.Build &&
                Info.NewVersion.Revision > currentVersion.Revision)
            {
                return true;
            }


            //default don't need to update
            return false;
        }

        
    }

}
