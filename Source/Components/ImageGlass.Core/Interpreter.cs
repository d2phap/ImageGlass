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
using System.Linq;
using System.Drawing.IconLib;
using System.Windows.Media.Imaging;
using ImageMagick;
using System.Threading.Tasks;

namespace ImageGlass.Core
{
    public class Interpreter
    {
        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="path">Full path  of image file</param>
        /// <param name="size">A custom size of image</param>
        /// <param name="colorProfileName">Name or Full path of color profile</param>
        /// <param name="isApplyColorProfileForAll">If FALSE, only the images with embedded profile will be applied</param>
        /// <returns></returns>
        public static async Task<Bitmap> Load(string path, Size size = new Size(), string colorProfileName = "sRGB", bool isApplyColorProfileForAll = false)
        {
            var ext = Path.GetExtension(path).ToLower();
            Bitmap bmp = null;

            await Task.Run(async () =>
            {
                switch (ext)
                {
                    case ".gif":
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            var ms = new MemoryStream();
                            fs.CopyTo(ms);
                            ms.Position = 0;

                            bmp = new Bitmap(ms, true);
                        }
                        break;

                    case ".ico":
                        bmp = ReadIconFile(path);
                        break;

                    default:
                        try
                        {
                            bmp = await GetBitmapFromFile();

                            if (bmp == null)
                            {
                                bmp = await GetBitmapFromWic();
                            }
                        }
                        catch (Exception)
                        {
                            bmp = await GetBitmapFromWic();
                        }
                        break;
                }

            }).ConfigureAwait(false);

            async Task<Bitmap> GetBitmapFromFile()
            {
                Bitmap bmpData = null;
                var settings = new MagickReadSettings();

                if (ext.CompareTo(".svg") == 0)
                {
                    settings.BackgroundColor = MagickColors.Transparent;
                }

                if (size.Width > 0 && size.Height > 0)
                {
                    settings.Width = size.Width;
                    settings.Height = size.Height;
                }

                //using (var magicColl = new MagickImageCollection())
                //{
                //    magicColl.Read(new FileInfo(path), settings);

                //    if (magicColl.Count > 0)
                //    {
                //        magicColl[0].Quality = 100;
                //        magicColl[0].AddProfile(ColorProfile.SRGB);

                //        bmp = BitmapBooster.BitmapFromSource(magicColl[0].ToBitmapSource());
                //    }
                //}

                await Task.Run(() =>
                {
                    using (var magicImg = new MagickImage(path, settings))
                    {
                        magicImg.Quality = 100;

                        //Get Exif information
                        var profile = magicImg.GetExifProfile();
                        if (profile != null)
                        {
                            //Get Orieantation Flag
                            var exifTag = profile.GetValue(ExifTag.Orientation);

                            if (exifTag != null)
                            {
                                int orientationFlag = int.Parse(profile.GetValue(ExifTag.Orientation).Value.ToString());

                                var orientationDegree = GetOrientationDegree(orientationFlag);
                                if (orientationDegree != 0)
                                {
                                    //Rotate image accordingly
                                    magicImg.Rotate(orientationDegree);
                                }
                            }
                        }


                        // get the color profile of image
                        var imgColorProfile = magicImg.GetColorProfile();


                        // if always apply color profile
                        // or only apply color profile if there is an embedded profile
                        if (isApplyColorProfileForAll || imgColorProfile != null)
                        {
                            if (imgColorProfile != null)
                            {
                                // correct the image color space
                                magicImg.ColorSpace = imgColorProfile.ColorSpace;
                            }
                            else
                            {
                                // set default color profile and color space
                                magicImg.AddProfile(ColorProfile.SRGB);
                                magicImg.ColorSpace = ColorProfile.SRGB.ColorSpace;
                            }

                            var colorProfile = GetColorProfileFromString(colorProfileName);
                            if (colorProfile != null)
                            {
                                magicImg.AddProfile(colorProfile);
                                magicImg.ColorSpace = colorProfile.ColorSpace;
                            }
                        }


                        //get bitmap
                        bmpData = magicImg.ToBitmap();
                    }
                }).ConfigureAwait(false);

                return bmpData;
            }

            async Task<Bitmap> GetBitmapFromWic()
            {
                Bitmap bmpData = null;

                await Task.Run(() =>
                {
                    var src = LoadImage(path);
                    bmpData = BitmapFromSource(src);
                }).ConfigureAwait(false);


                return bmpData;
            }

            return bmp;
        }

        /// <summary>
        /// Load image file using WIC
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        private static BitmapSource LoadImage(string filename, int frameIndex = 0)
        {
            using (var inFile = File.OpenRead(filename))
            {
                var decoder = BitmapDecoder.Create(inFile, BitmapCreateOptions.None, BitmapCacheOption.None);
                return Convert(decoder.Frames[frameIndex]);
            }
        }

        private static BitmapSource Convert(BitmapFrame frame)
        {
            int stride = frame.PixelWidth * (frame.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[frame.PixelHeight * stride];

            frame.CopyPixels(pixels, stride, 0);

            var bmpSource = BitmapSource.Create(frame.PixelWidth, frame.PixelHeight,
                frame.DpiX, frame.DpiY, frame.Format, frame.Palette, pixels, stride);

            return bmpSource;
        }

        /// <summary>
        /// Load image file using WIC
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="frameIndex"></param>
        /// <returns></returns>
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // Use a PNG encoder to support transparency
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        /// <summary>
        /// Read icon *.ICO file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap ReadIconFile(string path)
        {
            MultiIcon mIcon = new MultiIcon();
            mIcon.Load(path);

            //Try to get the largest image of it
            SingleIcon sIcon = mIcon[0];
            IconImage iImage = sIcon.OrderByDescending(ico => ico.Size.Width).ToList()[0];

            //Convert to bitmap
            return iImage.Icon.ToBitmap();
        }

        /// <summary>
        /// Returns Exif rotation in degrees. Returns 0 if the metadata 
        /// does not exist or could not be read. A negative value means
        /// the image needs to be mirrored about the vertical axis.
        /// </summary>
        /// <param name="orientationFlag">Orientation Flag</param>
        /// <returns></returns>
        public static double GetOrientationDegree(int orientationFlag)
        {
            if (orientationFlag == 1)
                return 0;
            else if (orientationFlag == 2)
                return -360;
            else if (orientationFlag == 3)
                return 180;
            else if (orientationFlag == 4)
                return -180;
            else if (orientationFlag == 5)
                return -90;
            else if (orientationFlag == 6)
                return 90;
            else if (orientationFlag == 7)
                return -270;
            else if (orientationFlag == 8)
                return 270;

            return 0;
        }

        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="size"></param>
        /// <param name="useEmbeddedThumbnails"></param>
        /// <returns></returns>
        public static Image GetThumbnail(string filename, Size size, bool useEmbeddedThumbnails)
        {
            var ext = Path.GetExtension(filename).ToLower();

            Image source = null;
            try
            {
                var settings = new MagickReadSettings();

                if (ext.CompareTo(".svg") == 0)
                {
                    settings.BackgroundColor = MagickColors.Transparent;
                }

                if (size.Width > 0 && size.Height > 0)
                {
                    settings.Width = size.Width;
                    settings.Height = size.Height;
                }

                using (var magicImg = new MagickImage(filename, settings))
                {
                    // Try to read the exif thumbnail
                    if (useEmbeddedThumbnails)
                    {
                        var profile = magicImg.GetExifProfile();
                        if (profile != null)
                        {
                            // Fetch the embedded thumbnail
                            var magicThumb = profile.CreateThumbnail();
                            if (magicThumb != null)
                            {
                                source = magicThumb.ToBitmap();
                            }
                        }
                    }

                    // Revert to source image if an embedded thumbnail of required size was not found.
                    if (source == null)
                    {
                        source = magicImg.ToBitmap();
                    }

                }//END using MagickImage 
            }
            catch
            {
                source = null;
            }
            return source;
        }

        /// <summary>
        /// Save image
        /// </summary>
        /// <param name="pic">Image source</param>
        /// <param name="filename">New image file name</param>
        public static async void SaveImage(Image pic, string filename)
        {
            await Task.Run(() =>
            {
                string ext = Path.GetExtension(filename).Substring(1).ToLower();

                using (var img = new MagickImage(new Bitmap(pic)))
                {
                    img.Quality = 100;
                    img.Write(filename);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Load image
        /// </summary>
        /// <param name="Filename">path to image to load</param>
        /// <param name="width">Set width for Scalable Format</param>
        /// <param name="height">Set height for Scalable Format</param>
        public static Bitmap LoadIcon(string Filename, int @width = 0, int @height = 0)
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
                return magicImg.ToBitmap();
            }
        }

        public static Image RotateImage(Image input, int degrees)
        {
            var bmp = new Bitmap(input);
            using (var img = new MagickImage(bmp))
            {
                img.Rotate(degrees);
                img.Quality = 100;
                return img.ToBitmap();
            }
        }

        public static Image Flip(Image input, bool horz)
        {
            var bmp = new Bitmap(input);
            using (var img = new MagickImage(bmp))
            {
                if (horz)
                    img.Flop();
                else
                    img.Flip();
                img.Quality = 100;
                return img.ToBitmap();
            }

        }


        /// <summary>
        /// Get built-in color profiles
        /// </summary>
        /// <returns></returns>
        public static string[] GetBuiltInColorProfiles()
        {
            return new string[]
            {
                "AdobeRGB1998",
                "AppleRGB",
                "CoatedFOGRA39",
                "ColorMatchRGB",
                "sRGB",
                "USWebCoatedSWOP",
            };
        }


        /// <summary>
        /// Get the correct color profile name
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        public static string GetCorrectColorProfileName(string name)
        {
            var profileName = "";

            if (File.Exists(name))
            {
                return name;
            }
            else
            {
                var builtInProfiles = GetBuiltInColorProfiles();
                var result = builtInProfiles.FirstOrDefault(i => i.ToUpperInvariant() == name.ToUpperInvariant());

                if (result != null)
                {
                    profileName = result;
                }
                else
                {
                    return string.Empty;
                }
            }

            return profileName;
        }


        /// <summary>
        /// Get the ColorProfile
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        private static ColorProfile GetColorProfileFromString(string name)
        {
            if (File.Exists(name))
            {
                return new ColorProfile(name);
            }
            else
            {
                // get all profile names in Magick.NET
                var profiles = typeof(ColorProfile).GetProperties();
                var result = profiles.FirstOrDefault(i => i.Name.ToUpperInvariant() == name.ToUpperInvariant());
                
                if (result != null)
                {
                    try
                    {
                        return (ColorProfile)result.GetValue(result);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

    }
}
