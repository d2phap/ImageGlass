/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017-2019 DUONG DIEU PHAP
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
using System.Drawing.Imaging;

namespace ImageGlass.Base.Photoing.Animators;

public class GifAnimator : ImgAnimator
{

    private readonly Bitmap? _bitmap;
    private int[] _frameDelays; // in millisecond

    private readonly int FRAME_DELAY_TAG = 0x5100;
    private readonly int LOOP_COUNT_TAG = 20737;


    /// <summary>
    /// Initialize new instance of <see cref="GifAnimator"/>.
    /// </summary>
    public GifAnimator(Bitmap bmp)
    {
        lock (bmp)
        {
            _bitmap = bmp;
            LoadGifMetadata();
        }
    }



    // Protected virtual methods
    #region Protected virtual methods

    protected override bool CanAnimate()
    {
        return _bitmap != null;
    }


    protected override TimeSpan GetFrameDelay(int frameIndex)
    {
        return TimeSpan.FromMilliseconds(_frameDelays[_frameIndex]);
    }


    protected override void UpdateFrame(int frameIndex)
    {
        lock (_bitmap)
        {
            // update frame
            _bitmap.SetActiveTimeFrame(_frameIndex);

            TriggerFrameChangedEvent(_bitmap);
        }
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        Array.Clear(_frameDelays);
    }

    #endregion // Protected virtual methods


    /// <summary>
    /// Loads GIF metadata.
    /// </summary>
    private void LoadGifMetadata()
    {
        if (_bitmap == null) return;

        _frameCount = _bitmap.GetFrameCount(FrameDimension.Time);
        _maxLoopCount = BitConverter.ToInt16(_bitmap.GetPropertyItem(LOOP_COUNT_TAG).Value, 0);

        var frameDelayData = _bitmap.GetPropertyItem(FRAME_DELAY_TAG)?.Value;
        _frameDelays = new int[_frameCount];

        for (int i = 0; i < _frameCount; i++)
        {
            // in millisecond
            _frameDelays[i] = BitConverter.ToInt32(frameDelayData, i * 4) * 10;

            // Sometimes gifs have a zero frame delay, erroneously?
            // These gifs seem to play differently depending on the program.
            // I'll give them a default delay, as most gifs with 0 delay seem
            // wayyyy to fast compared to other programs.
            //
            // 0.1 seconds appears to be chromes default setting... I'll use that
            // 
            // KBR 20181009 Older GIF editors could set the delay to 0, relying on the behavior
            // of Netscape Navigator to provide the default minimum of 10ms. On Windows 7, it
            // appears necessary to enforce this same default minimum, with no negative impact
            // on Windows 10.
            // KBR 20181127 10ms is only if the image has a delay of 0. Other delays should not
            // be modified (issue #458).
            if (_frameDelays[i] < 1)
                _frameDelays[i] = 10;
        }
    }

}

