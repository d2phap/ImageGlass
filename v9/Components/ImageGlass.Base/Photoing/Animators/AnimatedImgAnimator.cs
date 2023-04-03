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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base.Photoing.Codecs;

namespace ImageGlass.Base.Photoing.Animators;

public class AnimatedImgAnimator : ImgAnimator
{
    private AnimatedImg _frames;


    /// <summary>
    /// Initialize new instance of <see cref="AnimatedImgAnimator"/>.
    /// </summary>
    public AnimatedImgAnimator(AnimatedImg frames)
    {
        lock (frames)
        {
            _frames = frames;
            _frameCount = frames.FramesCount;
        }
    }


    // Protected virtual methods
    #region Protected virtual methods

    protected override bool CanAnimate()
    {
        return _frames != null;
    }


    protected override int GetFrameDelay(int frameIndex)
    {
        return GetFrame(_frameIndex)?.Duration ?? 0;
    }


    protected override void UpdateFrame(int frameIndex)
    {
        lock (_frames)
        {
            var frame = GetFrame(_frameIndex);
            if (frame != null)
            {
                TriggerFrameChangedEvent(frame.Bitmap);
            }
        }
    }

    #endregion // Protected virtual methods


    /// <summary>
    /// Gets image frame data.
    /// </summary>
    private AnimatedImgFrame? GetFrame(int frameIndex)
    {
        return _frames.GetFrame(frameIndex);
    }
}
