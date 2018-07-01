using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Drawing.IconLib;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace ImageGlass.Core
{
    public class Interpreter
    {
        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="path">Full path  of image file</param>
        /// <param name="width">Width value of scalable image format</param>
        /// <param name="height">Height value of scalable image format</param>
        /// <returns></returns>
        public static Bitmap Load(string path, int @width = 0, int @height = 0)
        {
            var ext = Path.GetExtension(path).ToLower();

            Bitmap bmp = null;

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
                        GetBitmapFromFile();

                        if (bmp == null)
                        {
                            GetBitmapFromWic();
                        }
                    }
                    catch (Exception)
                    {
                        GetBitmapFromWic();
                    }
                    break;
            }

            void GetBitmapFromFile()
            {
                var settings = new MagickReadSettings();

                if (ext.CompareTo(".svg") == 0)
                {
                    settings.BackgroundColor = MagickColors.Transparent;
                }

                if (width > 0 && height > 0)
                {
                    settings.Width = width;
                    settings.Height = height;
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

                    //corect the image color
                    magicImg.AddProfile(ColorProfile.SRGB);

                    //get bitmap
                    bmp = magicImg.ToBitmap();
                    
                }
            }

            void GetBitmapFromWic()
            {
                var src = LoadImage(path);
                bmp = BitmapFromSource(src);
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
        public static void SaveImage(Image pic, string filename)
        {
            string ext = Path.GetExtension(filename).Substring(1).ToLower();

            using (var img = new MagickImage(new Bitmap(pic)))
            {
                img.Quality = 100;
                img.Write(filename);
            }
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

    }
}
