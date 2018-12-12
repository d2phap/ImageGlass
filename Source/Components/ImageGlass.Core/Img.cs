/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;

namespace ImageGlass.Core
{
    public class Img
    {
        private Image bm;
        private string path;
        private bool _finished, _failed;

        public Img(string path)
        {
            this.path = path;
            Dispose();
        }

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        public async void Load(Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false)
        {
            Image im = null;
            try
            {
                im = await Interpreter.Load(path, size: size, colorProfileName: colorProfileName, isApplyColorProfileForAll: isApplyColorProfileForAll);
            }
            catch (Exception)
            { }

            Set(im);
        }

        /// <summary>
        /// True if (attempted) image loading finished.
        /// False if loading hasn't finished.
        /// </summary>
        public bool IsFinished
        {
            get { return _finished; }
            set { }
        }

        /// <summary>
        /// True if loading finished with errors.
        /// False if image loaded successfully.
        /// </summary>
        public bool IsFailed
        {
            get { return _failed; }
            set { }
        }

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public void Dispose()
        {
            if (bm != null)
            {
                bm.Dispose();
                bm = null;
            }
            // All cleared
            _finished = false;
            _failed = false;
        }

        /// <summary>
        /// Return the image (or null)
        /// </summary>
        /// <returns>HURR</returns>
        public Image Get()
        {
            return bm;
        }

        /// <summary>
        /// Manually set new image
        /// </summary>
        /// <param name="im">DURR</param>
        public void Set(Image im)
        {
            Dispose();
            bm = im;

            if (im != null)
            {
                // Set to valid image;
                // nothing wrong in here.
                _finished = true;
                _failed = false;
            }
            else
            {
                // Explicitly set to null;
                // assume externa failure
                _finished = true;
                _failed = true;
            }
        }

        /// <summary>
        /// Get full path of this image
        /// </summary>
        /// <returns>Full path of image</returns>
        public string GetFileName()
        {
            return path;
        }

        /// <summary>
        /// Set full path of this image
        /// </summary>
        /// <param name="s">New full path of image</param>
        public void SetFileName(string s)
        {
            path = s;
        }

        public Image GetThumbnail(Size size, bool useEmbeddedThumbnail)
        {
            return Interpreter.GetThumbnail(path, size, useEmbeddedThumbnail);
        }
    }
}