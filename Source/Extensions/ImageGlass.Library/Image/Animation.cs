using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
