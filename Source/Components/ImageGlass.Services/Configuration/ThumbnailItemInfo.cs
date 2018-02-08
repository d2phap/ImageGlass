/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Windows.Forms;

namespace ImageGlass.Services.Configuration
{
    public class ThumbnailItemInfo
    {
        /// <summary>
        /// Gets actual thumbnail dimension
        /// </summary>
        public int Dimension { get; }

        /// <summary>
        /// Gets extra space to adapt minimum width / height of thumbnail bar
        /// </summary>
        public int ExtraSpace { get; }

        /// <summary>
        /// Get total dimension
        /// </summary>
        /// <returns></returns>
        public int GetTotalDimension()
        {
            return Dimension + ExtraSpace;
        }

        /// <summary>
        /// Thumbnail item information
        /// </summary>
        /// <param name="dimension">Thumbnail size</param>
        /// <param name="isHorizontalView">Horizontal or Verticle view</param>
        public ThumbnailItemInfo(int dimension, bool isHorizontalView)
        {
            if (isHorizontalView)
            {
                Dimension = dimension;
                //ExtraSpace = 58;
            }
            else
            {
                switch (dimension)
                {
                    case 32:
                        Dimension = 32;
                        //ExtraSpace = 48;
                        break;

                    case 48:
                        Dimension = 48;
                        //ExtraSpace = 52;
                        break;

                    case 64:
                        Dimension = 64;
                        //ExtraSpace = 57;
                        break;

                    case 96:
                        Dimension = 96;
                        //ExtraSpace = 69;
                        break;

                    case 128:
                        Dimension = 128;
                        //ExtraSpace = 79;
                        break;

                    default:
                        Dimension = 48;
                        //ExtraSpace = 57;
                        break;
                }
            }
            ExtraSpace = 0;
        }
    }
}
