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

using ImageMagick;
using System;
using System.Drawing;

namespace ImageGlass.Heart
{
    public class BitmapImg : IDisposable
    {
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets bitmap
        /// </summary>
        public Bitmap Image { get; }

        /// <summary>
        /// Gets the exif profile from the image.
        /// </summary>
        public ExifProfile ExifProfile { get; }

        /// <summary>
        /// Gets the time in 1/100ths of a second which must expire before splaying the next image in an animated sequence.
        /// </summary>
        public int AnimationDelay { get; }

        /// <summary>
        /// Gets the number of iterations to loop an animation (e.g. Netscape loop extension) for.
        /// </summary>
        public int AnimationIterations { get; }

        /// <summary>
        /// Gets the ticks per seconds for the animation delay.
        /// </summary>
        public int AnimationTicksPerSecond { get; }

        /// <summary>
        /// Gets the JPEG/MIFF/PNG compression level (default 75).
        /// </summary>
        public int Quality { get; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the comment text of the image.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Gets a value indicating whether the image supports transparency (alpha channel).
        /// </summary>
        public bool HasAlpha { get; }

        /// <summary>
        /// Gets the label of the image.
        /// </summary>
        public string Label { get; }

        #endregion


        public BitmapImg(IMagickImage img)
        {
            this.AnimationDelay = img.AnimationDelay;
            this.AnimationIterations = img.AnimationIterations;
            this.AnimationTicksPerSecond = img.AnimationTicksPerSecond;
            this.Comment = img.Comment;
            this.HasAlpha = img.HasAlpha;
            this.Height = img.Height;
            this.Width = img.Width;
            this.Quality = img.Quality;
            this.Label = img.Label;

            this.Image = img.ToBitmap();
            this.ExifProfile = img.GetExifProfile();
        }

        public void Dispose()
        {
            this.Image.Dispose();
        }
    }
}
