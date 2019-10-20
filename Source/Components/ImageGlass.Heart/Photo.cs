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
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using System.Linq;

namespace ImageGlass.Heart
{
    public class Photo
    {

        #region Load image / thumbnail

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="quality">Image quality</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <param name="channel">MagickImage.Channel value</param>
        /// <returns>Bitmap</returns>
        public static Bitmap Load(
            string filename,
            Size size = new Size(),
            string colorProfileName = "sRGB",
            bool isApplyColorProfileForAll = false,
            int quality = 100,
            bool useEmbeddedThumbnails = false,
            int channel = -1
        )
        {
            Bitmap bitmap = null;
            var ext = Path.GetExtension(filename).ToUpperInvariant();
            var settings = new MagickReadSettings();

            #region Settings
            if (ext == ".SVG")
            {
                settings.BackgroundColor = MagickColors.Transparent;
            }

            if (size.Width > 0 && size.Height > 0)
            {
                settings.Width = size.Width;
                settings.Height = size.Height;
            }
            #endregion


            #region Read image data
            switch (ext)
            {
                case ".GIF":
                    // Note: Using FileStream is much faster than using MagickImageCollection
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        var ms = new MemoryStream();
                        fs.CopyTo(ms);
                        ms.Position = 0;

                        bitmap = new Bitmap(ms, true);
                    }
                    break;

                case ".ICO":
                    using (var imgColl = new MagickImageCollection(filename, settings))
                    {
                        if (imgColl.Count > 0)
                        {
                            // Get the biggest image in the collection
                            using (var imgM = imgColl.OrderByDescending(frame => frame.Width).First())
                            {
                                using (var channelImgM = ApplyColorChannel((MagickImage)imgM))
                                {
                                    bitmap = channelImgM.ToBitmap();
                                }
                            }
                        }
                    }
                    break;

                case ".TIF":
                    try
                    {
                        using (var imgColl = new MagickImageCollection(filename, settings))
                        {
                            bitmap = imgColl.ToBitmap();
                        }
                    }
                    catch
                    {
                        // Issue #637: MagickImageCollection falls over with certain images, fallback to MagickImage
                        ReadWithMagickImage();
                    }
                    break;

                default:
                    ReadWithMagickImage();

                    break;
            }
            #endregion


            #region Internal Functions 

            // Preprocess magick image
            void PreprocesMagickImage(MagickImage imgM, bool checkRotation = true)
            {
                imgM.Quality = quality;


                //Get Exif information
                var profile = imgM.GetExifProfile();

                // Use embedded thumbnails if specified
                if (profile != null && useEmbeddedThumbnails)
                {
                    // Fetch the embedded thumbnail
                    bitmap = profile.CreateThumbnail().ToBitmap();
                }


                // Revert to source image if an embedded thumbnail with required size was not found.
                if (bitmap == null)
                {
                    if (profile != null && checkRotation)
                    {
                        // Get Orientation Flag
                        var exifRotationTag = profile.GetValue(ExifTag.Orientation);

                        if (exifRotationTag != null)
                        {
                            if (int.TryParse(exifRotationTag.Value.ToString(), out var orientationFlag))
                            {
                                var orientationDegree = Helpers.GetOrientationDegree(orientationFlag);
                                if (orientationDegree != 0)
                                {
                                    //Rotate image accordingly
                                    imgM.Rotate(orientationDegree);
                                }
                            }
                        }
                    }


                    // get the color profile of image
                    var imgColorProfile = imgM.GetColorProfile();


                    // if always apply color profile
                    // or only apply color profile if there is an embedded profile
                    if (isApplyColorProfileForAll || imgColorProfile != null)
                    {
                        if (imgColorProfile != null)
                        {
                            // correct the image color space
                            imgM.ColorSpace = imgColorProfile.ColorSpace;
                        }
                        else
                        {
                            // set default color profile and color space
                            imgM.AddProfile(ColorProfile.SRGB);
                            imgM.ColorSpace = ColorProfile.SRGB.ColorSpace;
                        }

                        var colorProfile = Helpers.GetColorProfile(colorProfileName);
                        if (colorProfile != null)
                        {
                            imgM.AddProfile(colorProfile);
                            imgM.ColorSpace = colorProfile.ColorSpace;
                        }
                    }
                }

            }


            // Separate color channel
            MagickImage ApplyColorChannel(MagickImage imgM)
            {
                // separate color channel
                if (channel != -1)
                {
                    var magickChannel = (Channels)channel;
                    var channelImgM = (MagickImage)imgM.Separate(magickChannel).First();

                    if (imgM.HasAlpha && magickChannel != Channels.Alpha)
                    {
                        using (var alpha = imgM.Separate(Channels.Alpha).First())
                        {
                            channelImgM.Composite(alpha, CompositeOperator.CopyAlpha);
                        }
                    }
                    

                    return channelImgM;
                }

                return imgM;
            }

            void ReadWithMagickImage()
            {
                // Issue #530: ImageMagick falls over if the file path is longer than the (old)
                // windows limit of 260 characters. Workaround is to read the file bytes, but 
                // that requires using the "long path name" prefix to succeed.
                filename = Helpers.PrefixLongPath(filename);
                var allBytes = File.ReadAllBytes(filename);

                using (var imgM = new MagickImage(allBytes, settings))
                {
                    var checkRotation = ext != ".HEIC";
                    PreprocesMagickImage(imgM, checkRotation);

                    using (var channelImgM = ApplyColorChannel(imgM))
                    {
                        bitmap = channelImgM.ToBitmap();
                    }
                }

            }
            #endregion


            return bitmap;
        }


        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="quality">Image quality</param>
        /// <param name="channel">MagickImage.Channel value</param>
        /// <returns></returns>
        public static async Task<Bitmap> LoadAsync(string filename, Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false, int quality = 100, int channel = -1)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                bitmap = Load(filename, size, colorProfileName, isApplyColorProfileForAll, quality, useEmbeddedThumbnails: false, channel: channel);
            }).ConfigureAwait(false);


            return bitmap;
        }



        /// <summary>
        /// Get thumbnail image
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public static Bitmap GetThumbnail(string filename, Size size, bool useEmbeddedThumbnails = true)
        {
            Bitmap bmp = Load(filename,
                    size: size,
                    quality: 75,
                    useEmbeddedThumbnails: useEmbeddedThumbnails);

            return bmp;
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
            Bitmap bmp = null;

            await Task.Run(() =>
            {
                bmp = Load(filename,
                    size: size,
                    quality: 75,
                    useEmbeddedThumbnails: useEmbeddedThumbnails);

            }).ConfigureAwait(false);

            return bmp;
        }

        #endregion


        #region Save image as file

        /// <summary>
        /// Save as image file
        /// </summary>
        /// <param name="srcFileName">Source filename to save</param>
        /// <param name="destFileName">Destination filename</param>
        /// <param name="format">New image format</param>
        /// <param name="quality">JPEG/MIFF/PNG compression level</param>
        public static async Task SaveImageAsync(string srcFileName, string destFileName, MagickFormat format = MagickFormat.Unknown, int quality = 100)
        {
            await Task.Run(() =>
            {
                using (var imgM = new MagickImage(srcFileName))
                {
                    imgM.Quality = quality;
                    imgM.Write(destFileName, format);
                }
            }).ConfigureAwait(false);
        }


        /// <summary>
        /// Save as image file
        /// </summary>
        /// <param name="srcBitmap">Source bitmap to save</param>
        /// <param name="destFileName">Destination filename</param>
        /// <param name="format">New image format</param>
        /// <param name="quality">JPEG/MIFF/PNG compression level</param>
        public static void SaveImage(Bitmap srcBitmap, string destFileName, int format = (int)MagickFormat.Unknown, int quality = 100)
        {
            using (var imgM = new MagickImage(srcBitmap))
            {
                imgM.Quality = quality;

                if (format != (int)MagickFormat.Unknown)
                {
                    imgM.Write(destFileName, (MagickFormat)format);
                }
                else
                {
                    imgM.Write(destFileName);
                }
            }
        }


        /// <summary>
        /// Save image pages to files
        /// </summary>
        /// <param name="filename">The full path of source file</param>
        /// <param name="destFileName">The destination folder to save to</param>
        public static async Task SaveImagePagesAsync(string filename, string destFolder)
        {
            await Task.Run(() =>
            {
                // create dirs unless it does not exist
                Directory.CreateDirectory(destFolder);

                using (var imgColl = new MagickImageCollection(filename))
                {
                    var index = 0;
                    foreach (var imgM in imgColl)
                    {
                        index++;
                        imgM.Quality = 100;

                        try
                        {
                            var newFilename = Path.GetFileNameWithoutExtension(filename) + " - " +
                    index.ToString($"D{imgColl.Count.ToString().Length}") + ".png";
                            var destFilePath = Path.Combine(destFolder, newFilename);

                            imgM.Write(destFilePath, MagickFormat.Png);
                        }
                        catch { }
                    }
                }
            });
        }


        #endregion


        #region Rotate image

        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="srcFileName">Source filename</param>
        /// <param name="degrees">Degrees to rotate</param>
        /// <returns></returns>
        public static async Task<Bitmap> RotateImage(string srcFileName, int degrees)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                using (var imgM = new MagickImage(srcFileName))
                {
                    imgM.Rotate(degrees);
                    imgM.Quality = 100;

                    bitmap = imgM.ToBitmap();
                }
            });

            return bitmap;
        }

        /// <summary>
        /// Rotate image
        /// </summary>
        /// <param name="srcBitmap">Source bitmap</param>
        /// <param name="degrees">Degrees to rotate</param>
        /// <returns></returns>
        public static async Task<Bitmap> RotateImage(Bitmap srcBitmap, int degrees)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                using (var imgM = new MagickImage(srcBitmap))
                {
                    imgM.Rotate(degrees);
                    imgM.Quality = 100;

                    bitmap = imgM.ToBitmap();
                }
            });

            return bitmap;
        }

        #endregion


        #region Flip / flop

        /// <summary>
        /// Flip / flop an image
        /// </summary>
        /// <param name="srcFileName">Source filename</param>
        /// <param name="isHorzontal">Reflect each scanline in the horizontal/vertical direction</param>
        /// <returns></returns>
        public static async Task<Bitmap> Flip(string srcFileName, bool isHorzontal)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                using (var imgM = new MagickImage(srcFileName))
                {
                    bitmap = Flip(imgM, isHorzontal);
                }
            });

            return bitmap;
        }


        /// <summary>
        /// Flip / flop an image
        /// </summary>
        /// <param name="srcBitmap">Source bitmap</param>
        /// <param name="isHorzontal">Reflect each scanline in the horizontal/vertical direction</param>
        /// <returns></returns>
        public static async Task<Bitmap> Flip(Bitmap srcBitmap, bool isHorzontal)
        {
            Bitmap bitmap = null;

            await Task.Run(() =>
            {
                using (var imgM = new MagickImage(srcBitmap))
                {
                    bitmap = Flip(imgM, isHorzontal);
                }
            });

            return bitmap;
        }

        #endregion






        #region PRIVATE FUCTIONS

        /// <summary>
        /// Flip / flop MagickImage
        /// </summary>
        /// <param name="imgM"></param>
        /// <param name="isHorzontal"></param>
        /// <returns></returns>
        private static Bitmap Flip(MagickImage imgM, bool isHorzontal)
        {
            if (isHorzontal)
            {
                imgM.Flop();
            }
            else
            {
                imgM.Flip();
            }

            imgM.Quality = 100;

            return imgM.ToBitmap();
        }

        #endregion
    }
}
