/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

using ImageGlass.Services.Configuration;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageGlass.Theme
{
    public class ThemeImage
    {
        public Bitmap Image { get; set; }
        public string Filename { get; set; }

        /// <summary>
        /// Icon image
        /// </summary>
        public ThemeImage()
        {
            Image = null;
            Filename = string.Empty;
        }

        /// <summary>
        /// Icon image
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="width">Set width for Scalable Format</param>
        /// <param name="height">Set height for Scalable Format</param>
        public ThemeImage(string filename, int @width = 0, int @height = 0)
        {
            Image = null;
            Filename = filename;

            //Load image
            LoadIcon(width, height);
        }

        /// <summary>
        /// Load image
        /// </summary>
        /// <param name="width">Set width for Scalable Format</param>
        /// <param name="height">Set height for Scalable Format</param>
        public void LoadIcon(int @width = 0, int @height = 0)
        {
            var settings = new MagickReadSettings();
            var ext = Path.GetExtension(Filename).ToLower();

            if (ext.CompareTo(".svg") == 0)
            {
                settings.BackgroundColor = MagickColors.Transparent;
            }

            if (width > 0 && height > 0)
            {
                settings.Width = width;
                settings.Height = height;
            }

            using (var magicImg = new MagickImage(Filename, settings))
            {
                Image = magicImg.ToBitmap();
            }
        }


        /// <summary>
        /// Get the height of toolbar icon after applying DPI calculation
        /// </summary>
        /// <returns></returns>
        public static int GetCorrectIconHeight()
        {
            //Get Scaling factor
            double scaleFactor = DPIScaling.GetDPIScaleFactor();
            int iconHeight = (int)((int)Constants.TOOLBAR_ICON_HEIGHT * scaleFactor);

            return iconHeight;
        }

    }
}
