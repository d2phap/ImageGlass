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

using System.Drawing;

namespace ImageGlass.UI {
    public enum NavigationRegionType {
        Unknown,
        Left,
        Right,
    };

    /// <summary>
    /// Represent a navigation region
    /// </summary>
    public class NavigationRegion {
        /// <summary>
        /// Type of navigation region
        /// </summary>
        public NavigationRegionType Type { get; set; } = NavigationRegionType.Unknown;

        /// <summary>
        /// Rectangle of navigation region
        /// </summary>
        public Rectangle Region { get; set; } = new();
    }
}
