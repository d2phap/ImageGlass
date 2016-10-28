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

namespace ImageGlass.Services
{
    public class HotfixItemUpdate
    {
        private string _level;

        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }
        private Version _newVersion;

        public Version NewVersion
        {
            get { return _newVersion; }
            set { _newVersion = value; }
        }
        private string _oriFile;

        public string OriginalFile
        {
            get { return _oriFile; }
            set { _oriFile = value; }
        }
        private Uri _link;

        public Uri Link
        {
            get { return _link; }
            set { _link = value; }
        }
        private string _desFile;

        public string DestinaionFile
        {
            get { return _desFile; }
            set { _desFile = value; }
        }

        /// <summary>
        /// Provides information of hotfix item
        /// </summary>
        public HotfixItemUpdate()
        {
            _level = "Recommended";
            _newVersion = new Version("1.0.0.0");
            _oriFile = string.Empty;
            _desFile = string.Empty;
            _link = new Uri("http://imageglass.org");
        }
    }
}
