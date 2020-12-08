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

namespace ImageGlass.Services {
    public class InfoUpdate {
        #region Properties
        public Version NewVersion { get; set; }

        public string VersionType { get; set; }

        public string Level { get; set; }

        public Uri Link { get; set; }

        public string Size { get; set; }

        /// <summary>
        /// Get, set publish date
        /// </summary>
        public DateTime PublishDate { get; set; }

        public string Description { get; set; }
        #endregion

        /// <summary>
        /// Provides information of element 'Info> in 'Update>
        /// </summary>
        public InfoUpdate() {
            NewVersion = new Version("1.0.0.0");
            VersionType = "Stable";
            Level = "Recommended";
            Link = new Uri("https://imageglass.org");
            Size = "0 MB";
            PublishDate = DateTime.Now;
            Description = string.Empty;
        }
    }
}
