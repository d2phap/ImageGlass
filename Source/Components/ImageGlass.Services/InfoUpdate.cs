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

namespace ImageGlass.Services
{


    public class InfoUpdate
    {
        private Version _newVersion;
        private string _versionType;
        private string _level;
        private Uri _link;
        private string _size;
        private DateTime _pubDate;
        private string _decription;

        #region Properties
        public Version NewVersion
        {
            get { return _newVersion; }
            set { _newVersion = value; }
        }        

        public string VersionType
        {
            get { return _versionType; }
            set { _versionType = value; }
        }        

        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }        

        public Uri Link
        {
            get { return _link; }
            set { _link = value; }
        }        

        public string Size
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Get, set publish date
        /// </summary>
        public DateTime PublishDate
        {
            get { return _pubDate; }
            set { _pubDate = value; }
        }

        public string Decription
        {
            get { return _decription; }
            set { _decription = value; }
        }
        #endregion

        /// <summary>
        /// Provides information of element 'Info> in 'Update>
        /// </summary>
        public InfoUpdate()
        {
            _newVersion = new System.Version("1.0.0.0");
            _versionType = "Stable";
            _level = "Recommended";
            _link = new Uri("https://imageglass.org");
            _size = "0 MB";
            _pubDate = DateTime.Now;
            _decription = string.Empty;           
        }

    }
}
