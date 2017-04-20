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

            //Init
            LoadUpdateConfig(savedPath);

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
        /// <param name="filename"></param>
        public void LoadUpdateConfig(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
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

            //There is a new version
            if (Info.NewVersion.ToString().CompareTo(fv.FileVersion) != 0)
            {
                return true;
            }

            //default don't need to update
            return false;
        }

        
    }

}
