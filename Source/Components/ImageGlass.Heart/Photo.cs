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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;

namespace ImageGlass.Heart
{
    public class Photo
    {
        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="quality">Image quality</param>
        /// <returns></returns>
        public static async Task<List<BitmapImg>> LoadAsync(string filename, Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false, int quality = 100)
        {
            var ext = Path.GetExtension(filename).ToUpperInvariant();
            var settings = new MagickReadSettings();
            var imgList = new List<BitmapImg>();

            if (ext == ".SVG")
            {
                settings.BackgroundColor = MagickColors.Transparent;
            }

            if (size.Width > 0 && size.Height > 0)
            {
                settings.Width = size.Width;
                settings.Height = size.Height;
            }

            await Task.Run(() =>
            {
                using(var imgColl = new MagickImageCollection(filename, settings))
                {
                    // preprocess image data
                    for (int i = 0; i < imgColl.Count; i++)
                    {
                        imgList.Add(Helpers.PreprocessMagickImage(imgColl[i], colorProfileName, isApplyColorProfileForAll, quality));
                    }
                }
                
            }).ConfigureAwait(false);

            return imgList;
        }



        /// <summary>
        /// Get thumbnail image
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public static async Task<Bitmap> GetThumbnailAsync(string filename, Size size, bool useEmbeddedThumbnails = true)
        {
            var imgList = await LoadAsync(filename, size, quality: 75);
            Bitmap img = null;

            if (imgList.Count > 0)
            {
                var bmp = imgList[0];

                // Try to read the exif thumbnail
                if (useEmbeddedThumbnails)
                {
                    if (bmp.ExifProfile != null)
                    {
                        // Fetch the embedded thumbnail
                        img = bmp.ExifProfile.CreateThumbnail().ToBitmap();
                    }
                }

                // Revert to source image if an embedded thumbnail if required size was not found.
                if (img == null)
                {
                    img = bmp.Image;
                }
            }

            return img;
        }



        /// <summary>
        /// Save as image file
        /// </summary>
        /// <param name="magicImg">IMagickImage data</param>
        /// <param name="filename">Full path of image file</param>
        /// <param name="quality">JPEG/MIFF/PNG compression level</param>
        public static void SaveImage(IMagickImage magicImg, string filename, int quality = 100)
        {
            magicImg.Quality = quality;
            magicImg.Write(filename);
        }


        /// <summary>
        /// Rotate IMagickImage
        /// </summary>
        /// <param name="magicImg">IMagickImage data</param>
        /// <param name="degrees">Degrees to rotate</param>
        /// <returns></returns>
        public static IMagickImage RotateImage(IMagickImage magicImg, int degrees)
        {
            magicImg.Rotate(degrees);
            magicImg.Quality = 100;

            return magicImg;
        }


        /// <summary>
        /// Flip/flop IMagickImage
        /// </summary>
        /// <param name="magicImg">IMagickImage data</param>
        /// <param name="isHorzontal">Reflect each scanline in the horizontal/vertical direction</param>
        /// <returns></returns>
        public static IMagickImage Flip(IMagickImage magicImg, bool isHorzontal)
        {
            if (isHorzontal)
            {
                magicImg.Flop();
            }
            else
            {
                magicImg.Flip();
            }
            magicImg.Quality = 100;

            return magicImg;
        }


    }
}
