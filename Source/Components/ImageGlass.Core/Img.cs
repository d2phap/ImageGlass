/*
 * imaeg - generic image utility in C#
 * Copyright (C) 2010  ed <tripflag@gmail.com>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License v2
 * (version 2) as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, refer to the following URL:
 * http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
 */
using System;
using System.Drawing;

namespace ImageGlass.Core
{
    public class Img
    {
        private Image bm;
        private string path;
        private bool _finished, _failed;

        public Img(string path)
        {
            this.path = path;
            Dispose();
        }

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="root">Parent folder for instance path</param>
        public void Load()
        {
            Image im = null;
            try
            {
                im = Interpreter.Load(path);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            { }
            Set(im);
        }

        /// <summary>
        /// True if (attempted) image loading finished.
        /// False if loading hasn't finished.
        /// </summary>
        public bool IsFinished
        {
            get { return _finished; }
            set { }
        }

        /// <summary>
        /// True if loading finished with errors.
        /// False if image loaded successfully.
        /// </summary>
        public bool IsFailed
        {
            get { return _failed; }
            set { }
        }

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public void Dispose()
        {
            if (bm != null)
            {
                bm.Dispose();
                bm = null;
            }
            // All cleared
            _finished = false;
            _failed = false;
        }

        /// <summary>
        /// Return the image (or null)
        /// </summary>
        /// <returns>HURR</returns>
        public Image Get()
        {
            return bm;
        }

        /// <summary>
        /// Manually set new image
        /// </summary>
        /// <param name="im">DURR</param>
        public void Set(Image im)
        {
            Dispose();
            bm = im;

            if (im != null)
            {
                // Set to valid image;
                // nothing wrong in here.
                _finished = true;
                _failed = false;
            }
            else
            {
                // Explicitly set to null;
                // assume externa failure
                _finished = true;
                _failed = true;
            }
        }

        /// <summary>
        /// Get full path of this image
        /// </summary>
        /// <returns>Full path of image</returns>
        public string GetFileName()
        {
            return path;
        }

        /// <summary>
        /// Set full path of this image
        /// </summary>
        /// <param name="s">New full path of image</param>
        public void SetFileName(string s)
        {
            path = s;
        }

        public Image GetThumbnail(Size size, bool useEmbeddedThumbnail)
        {
            return Interpreter.GetThumbnail(path, size, useEmbeddedThumbnail);
        }
    }
}