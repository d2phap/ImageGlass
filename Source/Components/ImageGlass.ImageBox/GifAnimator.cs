using System;
using System.Drawing;

namespace ImageGlass {
    /// <summary>
    /// Used to animate gifs. 
    /// </summary>
    public interface GifAnimator
    {
        /// <summary>
        /// Updates the time frame for this image.
        /// </summary>
        void UpdateFrames(Image image);

        /// <summary>
        /// Stops updating frames for the given image. 
        /// </summary>
        void StopAnimate(Image image, EventHandler eventHandler);

        /// <summary>
        /// Animates the given image. 
        /// </summary>
        void Animate(Image image, EventHandler onFrameChangedHandler);

        /// <summary>
        /// Determines whether an image can be animated.
        /// </summary>
        /// <returns> true if the given image can be animated, otherwise false. </returns>
        bool CanAnimate(Image image);
    }
}
