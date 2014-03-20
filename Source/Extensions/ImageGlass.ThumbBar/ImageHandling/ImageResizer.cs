/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Drawing;
using System.Windows.Forms;


namespace ImageGlass.ThumbBar
{
    
    /// <summary>
    /// Static class containing image resizing operations.
    /// </summary>
    public static class ImageResizer
    {

        /// <summary>
        /// Returns the thumbnail resized proportionally to the given maximum dimension.
        /// </summary>
        /// <param name="fileName">The name of the image file</param>
        /// <param name="newSize">The maximum dimension to resize the image to</param>
        /// <returns>The thumbnail resized proportionally to the given maximum dimension</returns>
        public static Image CreateThumbnailFromFile(string fileName, int newSize)
        {
            Image tempImage;

            try
            {
                tempImage = new Bitmap(fileName);
            }
            catch
            {
                tempImage = GlobalData.InvalidImage;
            }

            try
            {
                Image thumbnail = CreateThumbnailFromImage(tempImage, newSize);
                return thumbnail;
            }
            finally
            {
                if (tempImage != GlobalData.InvalidImage)
                    tempImage.Dispose();
            }
        }


        /// <summary>
        /// Returns the thumbnail resized proportionally to the given maximum dimension.
        /// </summary>
        /// <param name="image">The image to be resized</param>
        /// <param name="newSize">The maximum dimension to resize the image to</param>
        /// <returns>The thumbnail resized proportionally to the given maximum dimension</returns>
        public static Image CreateThumbnailFromImage(Image image, int newSize)
        {
            int width = image.Width;
            int height = image.Height;

            if (width >= height)
            {
                float aspectRatio = (float)width / (float)height;
                width = newSize;
                height = (int)((float)width / aspectRatio);
            }
            else
            {
                float aspectRatio = (float)height / (float)width;
                height = newSize;
                width = (int)((float)height / aspectRatio);
            }

            Image thumbnail = new Bitmap(width, height);

            try
            {
                using (Graphics graphics = Graphics.FromImage(thumbnail))
                    graphics.DrawImage(image, 0, 0, width, height);
            }
            catch
            {
                using (Graphics graphics = Graphics.FromImage(thumbnail))
                    graphics.DrawImage(GlobalData.InvalidImage, 0, 0, width, height);
            }

            return thumbnail;
        }


        /// <summary>
        /// Returns the full-screen image resized proportionally to the screen size.
        /// </summary>
        /// <param name="image">The image to be resized</param>
        /// <returns>The image resized proportionally to the screen size</returns>
        public static Image CreateResizedFullScreenImageFromImage(Image image)
        {
            int width = image.Width;
            int height = image.Height;

            if (width >= height)
            {
                float aspectRatio = (float)width / (float)height;

                width = Screen.PrimaryScreen.Bounds.Width;
                height = (int)((float)width / aspectRatio);

                if (height > Screen.PrimaryScreen.Bounds.Height)
                {
                    height = Screen.PrimaryScreen.Bounds.Height;
                    width = (int)(height * aspectRatio);
                }
            }
            else
            {
                float aspectRatio = (float)height / (float)width;

                height = Screen.PrimaryScreen.Bounds.Height;
                width = (int)((float)height / aspectRatio);

                if (width > Screen.PrimaryScreen.Bounds.Width)
                {
                    width = Screen.PrimaryScreen.Bounds.Width;
                    height = (int)(width * aspectRatio);
                }
            }

            Image fullScreenImage = new Bitmap(width, height);

            try
            {
                using (Graphics graphics = Graphics.FromImage(fullScreenImage))
                    graphics.DrawImage(image, 0, 0, width, height);
            }
            catch
            {
                using (Graphics graphics = Graphics.FromImage(fullScreenImage))
                    graphics.DrawImage(GlobalData.InvalidImage, 0, 0, width, height);
            }

            return fullScreenImage;
        }

    }

}

