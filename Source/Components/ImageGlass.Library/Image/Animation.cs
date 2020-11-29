/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 - 2019 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Library.Image {
    public class Animation {
        private Bitmap _img;
        private bool _isAnimating = false;
        private int _i;
        private string _filename;
        private string _desFolder;
        private ExtractCallback _extractFinished;

        public delegate void ExtractCallback();

        /// <summary>
        /// Extract all frames of animation
        /// </summary>
        /// <param name="animationFile">File name</param>
        /// <param name="destinationFolder">Output folder</param>
        public async Task ExtractFramesAsync(string animationFile, string destinationFolder, ExtractCallback callback) {
            //initiate class

            _isAnimating = false;
            _filename = animationFile;
            _desFolder = destinationFolder;
            _extractFinished = callback;

            await Task.Run(() => {
                _img = new Bitmap(animationFile);
                _i = 1;

                //begin extract
                AnimateImage();
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// This method begins the animation.
        /// </summary>
        private void AnimateImage() {
            if (!_isAnimating) {
                //Begin the animation only once.
                ImageAnimator.Animate(_img, new EventHandler(SaveFrames));
                _isAnimating = true;
            }
        }

        /// <summary>
        /// Save current frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFrames(object sender, EventArgs e) {
            if (!_isAnimating) {
                return;
            }

            var frameCount = System.Drawing.Image.FromFile(_filename).GetFrameCount(FrameDimension.Time);
            var numberIndex = frameCount.ToString().Length;

            // Check current frame
            if (_i > frameCount) {
                _isAnimating = false;
                ImageAnimator.StopAnimate(_img, null);

                // Issue #565 callback to let the user know the extract has finished
                _extractFinished?.Invoke();
                _extractFinished = null;
                return;
            }

            //Begin the animation.
            AnimateImage();

            //Get the next frame ready for rendering.
            ImageAnimator.UpdateFrames();

            //Draw the next frame in the animation.
            _img.Save(
                Path.Combine(
                    _desFolder,
                    Path.GetFileNameWithoutExtension(_filename) + " - " +
                    _i.ToString($"D{numberIndex}") + ".png"
                    ),
                ImageFormat.Png
            );

            //go to next frame
            _i++;
        }
    }
}
