/*
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageGlass.Heart {
    public static class Photo {

        #region Load image / thumbnail

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <param name="quality">Image quality</param>
        /// <param name="channel">MagickImage.Channel value</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns>Bitmap</returns>
        public static ImgData Load(
            string filename,
            Size size = new Size(),
            string colorProfileName = "sRGB",
            bool isApplyColorProfileForAll = false,
            int quality = 100,
            int channel = -1,
            bool useEmbeddedThumbnails = false
        ) {
            Bitmap bitmap = null;
            IExifProfile exif = null;
            IColorProfile colorProfile = null;

            var ext = Path.GetExtension(filename).ToUpperInvariant();
            var settings = new MagickReadSettings();

            #region Settings
            if (ext == ".SVG") {
                settings.BackgroundColor = MagickColors.Transparent;
            }

            if (size.Width > 0 && size.Height > 0) {
                settings.Width = size.Width;
                settings.Height = size.Height;
            }
            #endregion


            #region Read image data
            switch (ext) {
                case ".TXT": // base64 string
                case ".B64":
                    var base64Content = string.Empty;
                    using (var fs = new StreamReader(filename)) {
                        base64Content = fs.ReadToEnd();
                        fs.Close();
                    }

                    bitmap = ConvertBase64ToBitmap(base64Content);
                    break;

                case ".GIF":
                case ".TIF":
                    // Note: Using FileStream is much faster than using MagickImageCollection

                    try {
                        bitmap = ConvertFileToBitmap(filename);
                    }
                    catch {
                        // #637: falls over with certain images, fallback to MagickImage
                        ReadWithMagickImage();
                    }
                    break;

                case ".ICO":
                case ".WEBP":
                    using (var imgColl = new MagickImageCollection(filename, settings)) {
                        bitmap = imgColl.ToBitmap();
                    }
                    break;

                default:
                    ReadWithMagickImage();

                    break;
            }
            #endregion


            #region Internal Functions 

            // Preprocess magick image
            (IExifProfile, IColorProfile) PreprocesMagickImage(MagickImage imgM, bool checkRotation = true) {
                imgM.Quality = quality;

                
                IColorProfile imgColorProfile = null;
                IExifProfile profile = null;
                try {
                    // get the color profile of image
                    imgColorProfile = imgM.GetColorProfile();

                    // Get Exif information
                    profile = imgM.GetExifProfile();
                }
                catch { }
                

                // Use embedded thumbnails if specified
                if (profile != null && useEmbeddedThumbnails) {
                    // Fetch the embedded thumbnail
                    var thumbM = profile.CreateThumbnail();
                    if (thumbM != null) {
                        bitmap = thumbM.ToBitmap();
                    }
                }


                // Revert to source image if an embedded thumbnail with required size was not found.
                if (bitmap == null) {
                    if (profile != null && checkRotation) {
                        // Get Orientation Flag
                        var exifRotationTag = profile.GetValue(ExifTag.Orientation);

                        if (exifRotationTag != null) {
                            if (int.TryParse(exifRotationTag.Value.ToString(), out var orientationFlag)) {
                                var orientationDegree = Helpers.GetOrientationDegree(orientationFlag);
                                if (orientationDegree != 0) {
                                    //Rotate image accordingly
                                    imgM.Rotate(orientationDegree);
                                }
                            }
                        }
                    }


                    // if always apply color profile
                    // or only apply color profile if there is an embedded profile
                    if (isApplyColorProfileForAll || imgColorProfile != null) {
                        if (imgColorProfile != null) {
                            // correct the image color space
                            imgM.ColorSpace = imgColorProfile.ColorSpace;
                        }
                        else {
                            // set default color profile and color space
                            imgM.SetProfile(ColorProfile.SRGB);
                            imgM.ColorSpace = ColorProfile.SRGB.ColorSpace;
                        }

                        var imgColor = Helpers.GetColorProfile(colorProfileName);
                        if (imgColor != null) {
                            imgM.SetProfile(imgColor);
                            imgM.ColorSpace = imgColor.ColorSpace;
                        }
                    }
                }


                return (profile, imgColorProfile);
            }


            // Separate color channel
            MagickImage ApplyColorChannel(MagickImage imgM) {
                if (channel != -1) {
                    var magickChannel = (Channels)channel;
                    var channelImgM = (MagickImage)imgM.Separate(magickChannel).First();

                    if (imgM.HasAlpha && magickChannel != Channels.Alpha) {
                        using (var alpha = imgM.Separate(Channels.Alpha).First()) {
                            channelImgM.Composite(alpha, CompositeOperator.CopyAlpha);
                        }
                    }

                    return channelImgM;
                }

                return imgM;
            }


            void ReadWithMagickImage() {
                MagickImage imgM;

                // Issue #530: ImageMagick falls over if the file path is longer than the (old) windows limit of 260 characters. Workaround is to read the file bytes, but that requires using the "long path name" prefix to succeed.
                if (filename.Length > 260) {
                    var newFilename = Helpers.PrefixLongPath(filename);
                    var allBytes = File.ReadAllBytes(newFilename);

                    imgM = new MagickImage(allBytes, settings);
                } else {
                    imgM = new MagickImage(filename, settings);
                }


                // Issue #679: fix targa display with Magick.NET 7.15.x 
                if (ext == ".TGA") {
                    imgM.AutoOrient();
                }

                var checkRotation = ext != ".HEIC";
                (exif, colorProfile) = PreprocesMagickImage(imgM, checkRotation);

                using (var channelImgM = ApplyColorChannel(imgM)) {
                    bitmap = channelImgM.ToBitmap();
                }


                imgM.Dispose();
            }
            #endregion


            return new ImgData() {
                Image = bitmap,
                Exif = exif,
                ColorProfile = colorProfile,
            };
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
        /// <param name="useEmbeddedThumbnail">Use embeded thumbnail if found</param>
        /// <returns></returns>
        public static async Task<ImgData> LoadAsync(string filename, Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false, int quality = 100, int channel = -1, bool useEmbeddedThumbnail = false) {
            var data =  await Task.Run(() =>
            {
                return Load(
                    filename,
                    size,
                    colorProfileName,
                    isApplyColorProfileForAll,
                    quality,
                    channel: channel,
                    useEmbeddedThumbnail
                );
            }).ConfigureAwait(false);


            return data;
        }



        /// <summary>
        /// Get thumbnail image
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public static Bitmap GetThumbnail(string filename, Size size, bool useEmbeddedThumbnails = true) {
            var data = Load(filename,
                    size: size,
                    quality: 75,
                    useEmbeddedThumbnails: useEmbeddedThumbnails);

            return data.Image;
        }


        /// <summary>
        /// Get thumbnail image
        /// </summary>
        /// <param name="filename">Full path of image file</param>
        /// <param name="size">A custom size of thumbnail</param>
        /// <param name="useEmbeddedThumbnails">Return the embedded thumbnail if required size was not found.</param>
        /// <returns></returns>
        public static async Task<Bitmap> GetThumbnailAsync(string filename, Size size, bool useEmbeddedThumbnails = true) {
            var data = await Task.Run(() =>
            {
                return Load(filename,
                    size: size,
                    quality: 75,
                    useEmbeddedThumbnails: useEmbeddedThumbnails);

            }).ConfigureAwait(false);

            return data.Image;
        }



        /// <summary>
        /// Converts file to Bitmap
        /// </summary>
        /// <param name="filename">Full path of file</param>
        /// <returns></returns>
        public static Bitmap ConvertFileToBitmap(string filename) {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;

                return new Bitmap(ms, true);
            }
        }


        /// <summary>
        /// Converts base64 string to byte array, returns MIME type and raw data in byte array.
        /// </summary>
        /// <param name="content">Base64 string</param>
        /// <returns></returns>
        public static (string, byte[]) ConvertBase64ToBytes(string content) {
            var mimeType = string.Empty;
            var rawData = Array.Empty<byte>();

            if (string.IsNullOrWhiteSpace(content)) return (mimeType, rawData);

            // data:image/svg-xml;base64,xxxxxxxx
            var dataUriPattern = new Regex(@"^data\:(?<type>image\/[a-z\+\-]*);base64,(?<data>[a-zA-Z0-9\+\/\=]+)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);


            var match = dataUriPattern.Match(content);
            if (!match.Success) return (mimeType, rawData);
            var base64Data = match.Groups["data"].Value;
            

            try {
                mimeType = match.Groups["type"].Value.ToLower();
                rawData = Convert.FromBase64String(base64Data);
            }
            catch {}

            return (mimeType, rawData);
        }


        /// <summary>
        /// Converts base64 string to Bitmap.
        /// </summary>
        /// <param name="content">Base64 string</param>
        /// <returns></returns>
        public static Bitmap ConvertBase64ToBitmap(string content) {
            var (mimeType, rawData) = ConvertBase64ToBytes(content);
            if (string.IsNullOrEmpty(mimeType)) return null;


            #region Settings
            var settings = new MagickReadSettings();
            switch (mimeType) {
                case "image/bmp":
                    settings.Format = MagickFormat.Bmp;
                    break;
                case "image/gif":
                    settings.Format = MagickFormat.Gif;
                    break;
                case "image/tiff":
                    settings.Format = MagickFormat.Tiff;
                    break;
                case "image/jpeg":
                    settings.Format = MagickFormat.Jpeg;
                    break;
                case "image/svg+xml":
                    settings.BackgroundColor = MagickColors.Transparent;
                    settings.Format = MagickFormat.Svg;
                    break;
                case "image/x-icon":
                    settings.Format = MagickFormat.Ico;
                    break;
                case "image/x-portable-anymap":
                    settings.Format = MagickFormat.Pnm;
                    break;
                case "image/x-portable-bitmap":
                    settings.Format = MagickFormat.Pbm;
                    break;
                case "image/x-portable-graymap":
                    settings.Format = MagickFormat.Pgm;
                    break;
                case "image/x-portable-pixmap":
                    settings.Format = MagickFormat.Ppm;
                    break;
                case "image/x-xbitmap":
                    settings.Format = MagickFormat.Xbm;
                    break;
                case "image/x-xpixmap":
                    settings.Format = MagickFormat.Xpm;
                    break;
                case "image/x-cmu-raster":
                    settings.Format = MagickFormat.Ras;
                    break;

                default:
                    break;
            }
            #endregion


            Bitmap bmp = null;

            switch (settings.Format) {
                case MagickFormat.Gif:
                case MagickFormat.Gif87:
                case MagickFormat.Tif:
                case MagickFormat.Tiff64:
                case MagickFormat.Tiff:
                case MagickFormat.Ico:
                case MagickFormat.Icon:
                    bmp = new Bitmap(new MemoryStream(rawData) {
                        Position = 0
                    }, true);

                    break;

                default:
                    using (var imgM = new MagickImage(rawData, settings)) {
                        bmp = imgM.ToBitmap();
                    }
                    break;
            }

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
        public static async Task SaveImageAsync(string srcFileName, string destFileName, MagickFormat format = MagickFormat.Unknown, int quality = 100) {
            await Task.Run(() => {
                using (var imgM = new MagickImage(srcFileName)) {
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
        public static void SaveImage(Bitmap srcBitmap, string destFileName, int format = (int)MagickFormat.Unknown, int quality = 100) {
            using (var imgM = new MagickImage(srcBitmap)) {
                imgM.Quality = quality;

                if (format != (int)MagickFormat.Unknown) {
                    imgM.Write(destFileName, (MagickFormat)format);
                }
                else {
                    imgM.Write(destFileName);
                }
            }
        }


        /// <summary>
        /// Save image pages to files
        /// </summary>
        /// <param name="filename">The full path of source file</param>
        /// <param name="destFileName">The destination folder to save to</param>
        public static async Task SaveImagePagesAsync(string filename, string destFolder) {
            await Task.Run(() => {
                // create dirs unless it does not exist
                Directory.CreateDirectory(destFolder);

                using (var imgColl = new MagickImageCollection(filename)) {
                    var index = 0;
                    foreach (var imgM in imgColl) {
                        index++;
                        imgM.Quality = 100;

                        try {
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
        public static async Task<Bitmap> RotateImage(string srcFileName, int degrees) {
            Bitmap bitmap = null;

            await Task.Run(() => {
                using (var imgM = new MagickImage(srcFileName)) {
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
        public static async Task<Bitmap> RotateImage(Bitmap srcBitmap, int degrees) {
            Bitmap bitmap = null;

            await Task.Run(() => {
                using (var imgM = new MagickImage(srcBitmap)) {
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
        public static async Task<Bitmap> Flip(string srcFileName, bool isHorzontal) {
            Bitmap bitmap = null;

            await Task.Run(() => {
                using (var imgM = new MagickImage(srcFileName)) {
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
        public static async Task<Bitmap> Flip(Bitmap srcBitmap, bool isHorzontal) {
            Bitmap bitmap = null;

            await Task.Run(() => {
                using (var imgM = new MagickImage(srcBitmap)) {
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
        private static Bitmap Flip(MagickImage imgM, bool isHorzontal) {
            if (isHorzontal) {
                imgM.Flop();
            }
            else {
                imgM.Flip();
            }

            imgM.Quality = 100;

            return imgM.ToBitmap();
        }

        #endregion
    }

}
