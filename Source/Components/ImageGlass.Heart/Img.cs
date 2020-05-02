﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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

using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageGlass.Heart
{
    public class Img : IDisposable
    {
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets the error details
        /// </summary>
        public Exception Error { get; private set; } = null;

        /// <summary>
        /// Gets the value indicates that image loading is done
        /// </summary>
        public bool IsDone { get; private set; } = false;

        /// <summary>
        /// Gets, sets filename of Img
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Gets, sets Bitmap data
        /// </summary>
        public Bitmap Image { get; set; } = null;

        /// <summary>
        /// Gets, sets number of image pages
        /// </summary>
        public int PageCount { get; private set; } = 0;

        /// <summary>
        /// Gets, sets the active page index
        /// </summary>
        public int ActivePageIndex { get; private set; } = 0;

        /// <summary>
        /// Gets the Exif profile of image
        /// </summary>
        public IExifProfile Exif { get; protected set; } = null;

        /// <summary>
        /// Gets the color profile of image
        /// </summary>
        public IColorProfile ColorProfile { get; protected set; } = null;

        #endregion PUBLIC PROPERTIES

        /// <summary>
        /// The Img class contain image data
        /// </summary>
        /// <param name="filename">Image filename</param>
        public Img(string filename)
        {
            this.Filename = filename;
        }

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Release all resources of Img
        /// </summary>
        public void Dispose()
        {
            this.IsDone = false;
            this.Error = null;
            this.PageCount = 0;

            this.Exif = null;
            this.ColorProfile = null;

            if (this.Image != null)
            {
                this.Image.Dispose();
            }
        }

        /// <summary>
        /// Load the image
        /// </summary>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="channel">MagickImage.Channel value</param>
        /// <param name="useEmbeddedThumbnail">Use the embeded thumbnail if found</param>
        public async Task LoadAsync(Size size = new Size(), string colorProfileName = "", bool isApplyColorProfileForAll = false, int channel = -1, bool useEmbeddedThumbnail = false)
        {
            // reset done status
            this.IsDone = false;

            // reset error
            this.Error = null;

            try
            {
                // load image data
                var data = await Photo.LoadAsync(
                    filename: this.Filename,
                    size: size,
                    colorProfileName: colorProfileName,
                    isApplyColorProfileForAll: isApplyColorProfileForAll,
                    channel: channel,
                    useEmbeddedThumbnail: useEmbeddedThumbnail
                );

                this.Image = data.Image;
                this.Exif = data.Exif;
                this.ColorProfile = data.ColorProfile;

                if (this.Image != null)
                {
                    // Get page count
                    var dim = new FrameDimension(this.Image.FrameDimensionsList[0]);
                    this.PageCount = this.Image.GetFrameCount(dim);
                }
            }
            catch (Exception ex)
            {
                // save the error
                this.Error = ex;
            }

            // done loading
            this.IsDone = true;
        }

        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnail">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public async Task<Bitmap> GetThumbnailAsync(Size size, bool useEmbeddedThumbnail = true)
        {
            return await Photo.GetThumbnailAsync(this.Filename, size, useEmbeddedThumbnail);
        }

        /// <summary>
        /// Sets active page index
        /// </summary>
        /// <param name="index">Page index</param>
        public void SetActivePage(int index)
        {
            if (this.Image == null) return;

            // Check if page index is greater than upper limit
            if (index >= this.PageCount)
                index = 0;

            // Check if page index is less than lower limit
            if (index < 0)
                index = this.PageCount - 1;

            this.ActivePageIndex = index;

            // Set active page index
            FrameDimension dim = new FrameDimension(this.Image.FrameDimensionsList[0]);
            this.Image.SelectActiveFrame(dim, this.ActivePageIndex);
        }

        /// <summary>
        /// Save image pages to files
        /// </summary>
        /// <param name="destFolder">The destination folder to save to</param>
        /// <returns></returns>
        public async Task SaveImagePages(string destFolder)
        {
            await Photo.SavePagesAsync(this.Filename, destFolder);
        }

        #endregion PUBLIC FUNCTIONS
    }
}