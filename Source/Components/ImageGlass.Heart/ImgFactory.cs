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

using System.Drawing;
using System.IO;
using ImageMagick;

namespace ImageGlass.Heart
{
    public class ImgFactory
    {
        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <returns></returns>
        public static MagickImageCollection Load(string filename, Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false)
        {
            var ext = Path.GetExtension(filename).ToUpperInvariant();
            var settings = new MagickReadSettings();
            var imgCollection = new MagickImageCollection();

            if (ext == ".SVG")
            {
                settings.BackgroundColor = MagickColors.Transparent;
            }

            if (size.Width > 0 && size.Height > 0)
            {
                settings.Width = size.Width;
                settings.Height = size.Height;
            }

            imgCollection.Read(filename, settings);

            // preprocess image data
            for (int i = 0; i < imgCollection.Count; i++)
            {
                imgCollection[i] = Helpers.PreprocessMagickImage(imgCollection[i], colorProfileName, isApplyColorProfileForAll);
            }

            return imgCollection;
        }



        /// <summary>
        /// Get thumbnail image
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public static IMagickImage GetThumbnail(string filename, Size size, bool useEmbeddedThumbnails = true)
        {
            var imgCollections = Load(filename, size);
            IMagickImage img = null;

            if (imgCollections.Count > 0)
            {
                var magicImg = imgCollections[0];

                // Try to read the exif thumbnail
                if (useEmbeddedThumbnails)
                {
                    var profile = magicImg.GetExifProfile();
                    if (profile != null)
                    {
                        // Fetch the embedded thumbnail
                        img = profile.CreateThumbnail();
                    }
                }

                // Revert to source image if an embedded thumbnail if required size was not found.
                if (img == null)
                {
                    img = magicImg;
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
