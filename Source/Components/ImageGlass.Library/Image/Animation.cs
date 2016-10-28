/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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

using System.IO;
using System.Drawing;

namespace ImageGlass.Library.Image
{
    public class Animation
    {
        private Bitmap _img;
        private bool _isAnimating = false;
        private int _i;
        private string _filename;
        private string _desFolder;

        /// <summary>
        /// Extract all frames of animation
        /// </summary>
        /// <param name="animationFile">File name</param>
        /// <param name="destinationFolder">Output folder</param>
        public void ExtractAllFrames(string animationFile, string destinationFolder)
        {
            //initiate class

            _isAnimating = false;
            _filename = animationFile;
            _desFolder = destinationFolder;

            _img = new Bitmap(animationFile);
            _i = 1;

            //begin extract
            AnimateImage();
        }

        /// <summary>
        /// This method begins the animation.
        /// </summary>
        private void AnimateImage()
        {
            if (!_isAnimating)
            {
                //Begin the animation only once.
                ImageAnimator.Animate(_img, new System.EventHandler(SaveFrames));
                _isAnimating = true;
            }

        }

        /// <summary>
        /// Save current frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFrames(object sender, System.EventArgs e)
        {
            //kiểm tra đã hết frame chưa
            if (_i > System.Drawing.Image.FromFile(_filename).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time))
            {
                _isAnimating = false;
                return;
            }

            //Begin the animation.
            AnimateImage();

            //Get the next frame ready for rendering.
            ImageAnimator.UpdateFrames();

            //Draw the next frame in the animation.
            _img.Save((_desFolder + "\\").Replace("\\\\", "\\") +
                    Path.GetFileNameWithoutExtension(_filename) + " - " +
                    _i.ToString() + ".png",
                    System.Drawing.Imaging.ImageFormat.Png);

            //go to next frmae
            _i = _i + 1;

        }

    }



}
