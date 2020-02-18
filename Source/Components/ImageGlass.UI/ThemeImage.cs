/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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

using ImageGlass.Base;
using ImageGlass.Heart;
using System.Drawing;

namespace ImageGlass.UI
{
    public class ThemeImage
    {
        private int _height = Constants.TOOLBAR_ICON_HEIGHT;


        public Bitmap Image { get; set; } = null;

        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Sets the height of icon image. Gets the height with DPI correction.
        /// </summary>
        public int Height
        {
            get => GetCorrectHeight(_height);
            set => _height = value;
        }


        /// <summary>
        /// Icon image
        /// </summary>
        public ThemeImage() { }

        /// <summary>
        /// Icon image
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="height">Height of the icon</param>
        public ThemeImage(string filename, int height = Constants.TOOLBAR_ICON_HEIGHT)
        {
            Height = height;
            Filename = filename;

            // load icon
            Refresh();
        }


        private int GetCorrectHeight(int height)
        {
            // Get Scaling factor
            var scaleFactor = DPIScaling.GetDPIScaleFactor();
            var iconHeight = (int)(height * scaleFactor);

            return iconHeight;
        }


        /// <summary>
        /// Reload theme image
        /// </summary>
        public void Refresh()
        {
            if (string.IsNullOrWhiteSpace(Filename))
                return;

            try
            {
                var data = Photo.Load(Filename, new Size(Height, Height));
                data.OriginalImage.Dispose();
                Image = data.ProcessedBitmap;
            }
            catch { }
        }


        /// <summary>
        /// Get the height of toolbar icon after applying DPI calculation
        /// </summary>
        /// <returns></returns>
        public static int GetCorrectBaseIconHeight()
        {
            // Get Scaling factor
            var scaleFactor = DPIScaling.GetDPIScaleFactor();
            var iconHeight = (int)(Constants.TOOLBAR_ICON_HEIGHT * scaleFactor);

            return iconHeight;
        }

    }
}
