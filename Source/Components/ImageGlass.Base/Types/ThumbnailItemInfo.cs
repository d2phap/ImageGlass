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

namespace ImageGlass.Base {
    /// <summary>
    /// Contains the information of thumbnail item in thumbnail bar
    /// </summary>
    public class ThumbnailItemInfo {
        /// <summary>
        /// Gets actual thumbnail dimension
        /// </summary>
        public uint Dimension { get; }

        /// <summary>
        /// Gets extra space to adapt minimum width / height of thumbnail bar
        /// </summary>
        public uint ExtraSpace { get; }

        /// <summary>
        /// Get total dimension
        /// </summary>
        /// <returns></returns>
        public uint GetTotalDimension() {
            return Dimension + ExtraSpace;
        }

        /// <summary>
        /// Thumbnail item information
        /// </summary>
        /// <param name="dimension">Thumbnail size</param>
        /// <param name="isHorizontalView">Horizontal or Verticle view</param>
        public ThumbnailItemInfo(uint dimension, bool isHorizontalView) {
            if (isHorizontalView) {
                Dimension = dimension;
            }
            else {
                Dimension = dimension switch {
                    32 => 32,
                    48 => 48,
                    64 => 64,
                    96 => 96,
                    128 => 128,
                    _ => 48,
                };
            }
            ExtraSpace = 0;
        }
    }
}
