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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using ImageMagick;

namespace ImageGlass.Heart {
    public sealed class Img: IDisposable {
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets the error details
        /// </summary>
        public Exception Error { get; private set; } = null;

        /// <summary>
        /// Gets the value indicates that image loading is done
        /// </summary>
        public bool IsDone { get; set; } = false;

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
        public int PageCount { get; set; } = 0;

        /// <summary>
        /// Gets, sets the active page index
        /// </summary>
        public int ActivePageIndex { get; set; } = 0;

        /// <summary>
        /// Gets the Exif profile of image
        /// </summary>
        public IExifProfile Exif { get; set; } = null;

        /// <summary>
        /// Gets the color profile of image
        /// </summary>
        public IColorProfile ColorProfile { get; set; } = null;

        #endregion

        /// <summary>
        /// The Img class contain image data
        /// </summary>
        /// <param name="filename">Image filename</param>
        public Img(string filename) => Filename = filename;

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Release all resources of Img
        /// </summary>
        public void Dispose() {
            IsDone = false;
            Error = null;
            PageCount = 0;

            Exif = null;
            ColorProfile = null;

            Image?.Dispose();
        }

        /// <summary>
        /// Load the image
        /// </summary>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="channel">MagickImage.Channel value</param>
        /// <param name="useEmbeddedThumbnail">Use the embeded thumbnail if found</param>
        public async Task LoadAsync(Size size = new Size(), string colorProfileName = "", bool isApplyColorProfileForAll = false, int channel = -1, bool useEmbeddedThumbnail = false) {
            // reset done status
            IsDone = false;

            // reset error
            Error = null;

            try {
                // load image data
                var data = await Photo.LoadAsync(
                    filename: Filename,
                    size: size,
                    colorProfileName: colorProfileName,
                    isApplyColorProfileForAll: isApplyColorProfileForAll,
                    channel: channel,
                    useEmbeddedThumbnail: useEmbeddedThumbnail
                ).ConfigureAwait(true);

                Image = data.Image;
                Exif = data.Exif;
                ColorProfile = data.ColorProfile;

                if (Image != null) {
                    // Get page count
                    var dim = new FrameDimension(Image.FrameDimensionsList[0]);
                    PageCount = Image.GetFrameCount(dim);
                }
            }
            catch (Exception ex) {
                // save the error
                Error = ex;
            }

            // done loading
            IsDone = true;
        }

        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnail">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public async Task<Bitmap> GetThumbnailAsync(Size size, bool useEmbeddedThumbnail = true) {
            return await Photo.GetThumbnailAsync(Filename, size, useEmbeddedThumbnail).ConfigureAwait(true);
        }

        /// <summary>
        /// Sets active page index
        /// </summary>
        /// <param name="index">Page index</param>
        public void SetActivePage(int index) {
            if (Image == null) return;

            // Check if page index is greater than upper limit
            if (index >= PageCount)
                index = 0;

            // Check if page index is less than lower limit
            if (index < 0)
                index = PageCount - 1;

            ActivePageIndex = index;

            // Set active page index
            var dim = new FrameDimension(Image.FrameDimensionsList[0]);
            Image.SelectActiveFrame(dim, ActivePageIndex);
        }

        /// <summary>
        /// Save image pages to files
        /// </summary>
        /// <param name="destFolder">The destination folder to save to</param>
        /// <returns></returns>
        public async Task SaveImagePagesAsync(string destFolder) {
            await Photo.SavePagesAsync(Filename, destFolder).ConfigureAwait(true);
        }

        #endregion

    }
}
