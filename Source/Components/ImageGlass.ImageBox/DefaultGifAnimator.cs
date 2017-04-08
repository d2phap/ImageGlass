using System;
using System.Drawing;

namespace ImageGlass {
    /// <summary>
    /// This is a wrapper for the original System.Drawing animator. See <see cref="ImageAnimator"/>.
    /// </summary>
    public class DefaultGifAnimator : GifAnimator {
        public void UpdateFrames(Image image) {
            ImageAnimator.UpdateFrames(image);
        }

        public void StopAnimate(Image image, EventHandler eventHandler) {
            ImageAnimator.StopAnimate(image, eventHandler);
        }

        public void Animate(Image image, EventHandler eventHandler) {
            ImageAnimator.Animate(image, eventHandler);
        }

        public bool CanAnimate(Image image) {
            return ImageAnimator.CanAnimate(image);
        }
    }
}