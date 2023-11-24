/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using System.Drawing.Drawing2D;

namespace ImageGlass.Gallery;


/// <summary>
/// Extracts thumbnails from images.
/// </summary>
public partial class GDIExtractor : IExtractor
{
    // Exif Tag IDs
    private const int TagOrientation = 0x0112;


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the GDIExtractor class.
    /// </summary>
    public GDIExtractor()
    {
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Creates a thumbnail from the given image.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given image or null if an error occurs.</returns>
    public virtual Image? GetThumbnail(Image image, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        if (size.Width <= 0 || size.Height <= 0) return null;

        return GetThumbnailBmp(image, size, useExifOrientation ? GetRotation(image) : 0);
    }


    /// <summary>
    /// Creates a thumbnail from the given image file.
    /// </summary>
    /// <param name="filename">The filename pointing to an image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given file or null if an error occurs.</returns>
    public virtual Image? GetThumbnail(string filename, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        if (string.IsNullOrEmpty(filename)) return null;
        if (size.Width <= 0 || size.Height <= 0) return null;

        return GetThumbnailBmp(filename, size, useEmbeddedThumbnails,
            useExifOrientation ? GetRotation(filename) : 0);
    }

    #endregion


    #region Helper Methods

    /// <summary>
    /// Creates a thumbnail from the given image.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="rotate">Rotation angle.</param>
    /// <returns>The image from the given file or null if an error occurs.</returns>
    internal static Image? GetThumbnailBmp(Image image, Size size, int rotate)
    {
        if (size.Width <= 0 || size.Height <= 0) return null;

        Image? thumb = null;
        try
        {
            double scale;
            if (rotate % 180 != 0)
            {
                scale = Math.Min(size.Height / (double)image.Width,
                    size.Width / (double)image.Height);
            }
            else
            {
                scale = Math.Min(size.Width / (double)image.Width,
                    size.Height / (double)image.Height);
            }

            thumb = ScaleDownRotateBitmap(image, scale, rotate);
        }
        catch
        {
            thumb?.Dispose();
            thumb = null;
        }

        return thumb;
    }


    /// <summary>
    /// Creates a thumbnail from the given image file.
    /// </summary>
    /// <param name="filename">The filename pointing to an image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="rotate">Rotation angle.</param>
    /// <returns>The image from the given file or null if an error occurs.</returns>
    internal Image? GetThumbnailBmp(string filename, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, int rotate)
    {
        if (size.Width <= 0 || size.Height <= 0) return null;

        Image? thumb = null;

        // extrack thumbnail from shell
        if (useEmbeddedThumbnails == UseEmbeddedThumbnails.Always)
        {
            thumb = ShellThumbnailApi.GetThumbnail(filename, size.Width, size.Height, ShellThumbnailOptions.ThumbnailOnly);
        }

        // get thumbnail from source file
        else if (useEmbeddedThumbnails == UseEmbeddedThumbnails.Never)
        {
            thumb = PhotoCodec.GetThumbnail(filename, size.Width, size.Height);
        }
        else
        {
            thumb = ShellThumbnailApi.GetThumbnail(filename, size.Width, size.Height, ShellThumbnailOptions.ThumbnailOnly);

            if (thumb == null)
            {
                thumb = PhotoCodec.GetThumbnail(filename, size.Width, size.Height);
            }
        }

        return thumb;
    }


    /// <summary>
    /// Returns Exif rotation in degrees. Returns 0 if the metadata 
    /// does not exist or could not be read. A negative value means
    /// the image needs to be mirrored about the vertical axis.
    /// </summary>
    /// <param name="img">Image.</param>
    private static int GetRotation(Image img)
    {
        try
        {
            foreach (var prop in img.PropertyItems)
            {
                if (prop.Id == TagOrientation && prop.Value != null)
                {
                    var orientationFlag = BitConverter.ToUInt16(prop.Value, 0);
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
        catch { }

        return 0;
    }


    /// <summary>
    /// Returns Exif rotation in degrees. Returns 0 if the metadata 
    /// does not exist or could not be read. A negative value means
    /// the image needs to be mirrored about the vertical axis.
    /// </summary>
    /// <param name="filename">Image.</param>
    private static int GetRotation(string filename)
    {
        try
        {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var img = Image.FromStream(stream, false, false);

            return GetRotation(img);
        }
        catch { }

        return 0;
    }


    /// <summary>
    /// Scales down and rotates an image.
    /// </summary>
    /// <param name="source">Original image</param>
    /// <param name="scale">Uniform scaling factor</param>
    /// <param name="angle">Rotation angle</param>
    /// <returns>Scaled and rotated image</returns>
    private static Image? ScaleDownRotateBitmap(Image source, double scale, int angle)
    {
        if (angle % 90 != 0)
        {
            throw new ArgumentException("Rotation angle should be a multiple of 90 degrees.", nameof(angle));
        }

        // Do not upscale and no rotation.
        if ((float)scale >= 1.0f && angle == 0)
        {
            return new Bitmap(source);
        }

        var sourceWidth = source.Width;
        var sourceHeight = source.Height;

        // Scale
        var xScale = Math.Min(1.0, Math.Max(1.0 / sourceWidth, scale));
        var yScale = Math.Min(1.0, Math.Max(1.0 / sourceHeight, scale));

        var width = (int)(sourceWidth * xScale);
        var height = (int)(sourceHeight * yScale);
        var thumbWidth = Math.Abs(angle) % 180 == 0 ? width : height;
        var thumbHeight = Math.Abs(angle) % 180 == 0 ? height : width;

        var thumb = new Bitmap(thumbWidth, thumbHeight);
        thumb.SetResolution(source.HorizontalResolution, source.VerticalResolution);

        using var g = Graphics.FromImage(thumb);
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.Clear(Color.Transparent);

        g.TranslateTransform(-sourceWidth / 2, -sourceHeight / 2, MatrixOrder.Append);
        if (Math.Abs(angle) % 360 != 0)
            g.RotateTransform(Math.Abs(angle), MatrixOrder.Append);
        if (angle < 0)
            xScale = -xScale;
        g.ScaleTransform((float)xScale, (float)yScale, MatrixOrder.Append);
        g.TranslateTransform(thumbWidth / 2, thumbHeight / 2, MatrixOrder.Append);

        g.DrawImage(source, 0, 0);


        return thumb;
    }


    #endregion
}
