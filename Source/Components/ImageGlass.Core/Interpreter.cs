using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Drawing.IconLib;
using Paloma;
using System.Drawing.Drawing2D;
using FreeImageAPI;
using Svg;

namespace ImageGlass.Core
{
    public class Interpreter
    {
        private const int TAG_ORIENTATION = 0x0112;

        public static Bitmap load(string path)
        {
            Bitmap bmp = null;
            
            //file *.hdr
            if (path.ToLower().EndsWith(".hdr"))
            {
                FIBITMAP hdr = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_HDR, path, FREE_IMAGE_LOAD_FLAGS.RAW_DISPLAY);
                bmp = FreeImage.GetBitmap(FreeImage.ToneMapping(hdr, FREE_IMAGE_TMO.FITMO_DRAGO03, 2.2, 0));
                FreeImage.Unload(hdr);
            }
            //file *.exr
            else if (path.ToLower().EndsWith(".exr"))
            {
                FIBITMAP exr = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_EXR, path, FREE_IMAGE_LOAD_FLAGS.RAW_DISPLAY);
                bmp = FreeImage.GetBitmap(FreeImage.ToneMapping(exr, FREE_IMAGE_TMO.FITMO_DRAGO03, 2.2, 0));
                FreeImage.Unload(exr);
            }
            //file *.svg
            else if (path.ToLower().EndsWith(".svg"))
            {
                SvgDocument svg = SvgDocument.Open(path);
                bmp = svg.Draw();
            }
            //TARGA file *.tga
            else if (path.ToLower().EndsWith(".tga"))
            {
                using (Paloma.TargaImage tar = new Paloma.TargaImage(path))
                {
                    bmp = new Bitmap(tar.Image);
                }
            }
            //PHOTOSHOP file *.PSD
            else if (path.ToLower().EndsWith(".psd"))
            {
                System.Drawing.PSD.PsdFile psd = (new System.Drawing.PSD.PsdFile()).Load(path);
                bmp = System.Drawing.PSD.ImageDecoder.DecodeImage(psd);
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bmp = new Bitmap(fs, true);

                    //GIF file *.gif
                    if (bmp.RawFormat.Equals(ImageFormat.Gif))
                    {
                        bmp = new Bitmap(path);
                    }
                    //ICON file *.ico
                    else if (bmp.RawFormat.Equals(ImageFormat.Icon))
                    {
                        bmp = ReadIconFile(path);
                    }
                    else if (bmp.RawFormat.Equals(ImageFormat.Jpeg))
                    {
                        //read Exif rotation
                        int rotation = GetRotation(bmp);
                        if (rotation != 0)
                        {
                            bmp = ScaleDownRotateBitmap(bmp, 1.0f, rotation);
                        }
                    }
                }
            }

            

            return bmp;
        }


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
        /// <param name="img">Image.</param>
        public static int GetRotation(Bitmap img)
        {
            try
            {
                foreach (PropertyItem prop in img.PropertyItems)
                {
                    if (prop.Id == TAG_ORIENTATION)
                    {
                        ushort orientationFlag = BitConverter.ToUInt16(prop.Value, 0);
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
                    }
                }
            }
            catch
            {
                ;
            }

            return 0;
        }


        /// <summary>
        /// Scales down and rotates an image.
        /// </summary>
        /// <param name="source">Original image</param>
        /// <param name="scale">Uniform scaling factor</param>
        /// <param name="angle">Rotation angle</param>
        /// <returns>Scaled and rotated image</returns>
        public static Bitmap ScaleDownRotateBitmap(Bitmap source, double scale, int angle)
        {
            if (angle % 90 != 0)
            {
                throw new ArgumentException("Rotation angle should be a multiple of 90 degrees.", "angle");
            }

            // Do not upscale and no rotation.
            if ((float)scale >= 1.0f && angle == 0)
            {
                return new Bitmap(source);
            }

            int sourceWidth = source.Width;
            int sourceHeight = source.Height;

            // Scale
            double xScale = Math.Min(1.0, Math.Max(1.0 / (double)sourceWidth, scale));
            double yScale = Math.Min(1.0, Math.Max(1.0 / (double)sourceHeight, scale));

            int width = (int)((double)sourceWidth * xScale);
            int height = (int)((double)sourceHeight * yScale);
            int thumbWidth = Math.Abs(angle) % 180 == 0 ? width : height;
            int thumbHeight = Math.Abs(angle) % 180 == 0 ? height : width;

            Bitmap thumb = new Bitmap(thumbWidth, thumbHeight);
            thumb.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(System.Drawing.Color.Transparent);

                g.TranslateTransform(-sourceWidth / 2, -sourceHeight / 2, MatrixOrder.Append);
                if (Math.Abs(angle) % 360 != 0)
                    g.RotateTransform(Math.Abs(angle), MatrixOrder.Append);
                if (angle < 0)
                    xScale = -xScale;
                g.ScaleTransform((float)xScale, (float)yScale, MatrixOrder.Append);
                g.TranslateTransform(thumbWidth / 2, thumbHeight / 2, MatrixOrder.Append);

                g.DrawImage(source, 0, 0);
            }

            return thumb;
        }



    }
}
