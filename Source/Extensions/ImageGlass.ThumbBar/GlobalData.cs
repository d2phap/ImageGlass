/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
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
using System.Drawing;
using System.Windows.Forms;


namespace ImageGlass.ThumbBar
{
    /// <summary>
    /// Global data class of the application.
    /// </summary>
    static class GlobalData
    {   

        /// <summary>
        /// The given thumbnail size.
        /// </summary>
        public static int ThumbnailSize = 64;

        /// <summary>
        /// The invalid image logo.
        /// </summary>
        public static Image InvalidImage = null;

        /// <summary>
        /// The thumbnail of the invalid image logo.
        /// </summary>
        public static Image InvalidImageThumbnail = null;

        /// <summary>
        /// The loading image logo.
        /// </summary>
        public static Image LoadingImage = null;

        /// <summary>
        /// The thumbnail of the loading image logo.
        /// </summary>
        public static Image LoadingImageThumbnail = null;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static GlobalData()
        {
            ReloadData();
        }

        /// <summary>
        /// Reload all data
        /// </summary>
        public static void ReloadData()
        {
            InvalidImage = new Bitmap(ThumbnailSize, ThumbnailSize);
            InvalidImageThumbnail = new Bitmap(ThumbnailSize, ThumbnailSize);

            LoadingImage = new Bitmap(ThumbnailSize, ThumbnailSize);
            LoadingImageThumbnail = new Bitmap(ThumbnailSize, ThumbnailSize);
        }

    }

}
