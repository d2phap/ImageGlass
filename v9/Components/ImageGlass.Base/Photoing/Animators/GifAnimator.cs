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
using ImageGlass.Base.WinApi;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageGlass.Base.Photoing.Animators;

public class GifAnimator : IDisposable
{

    #region IDisposable Disposing

    public bool IsDisposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            _enable = false;

            _frameIndex = 0;
            _frameCount = 0;
            _maxLoopCount = 0;
            _loopIndex = 0;

            Array.Clear(_frameDelays);
            // don't dispose _bitmap here
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GifAnimator()
    {
        Dispose(false);
    }

    #endregion



    private readonly Bitmap? _bitmap;
    private int[] _frameDelays; // in millisecond
    private int _minTickTimeInMillisecond = 20;

    private int _frameCount = 0;
    private int _frameIndex = 0;
    private int _maxLoopCount = 0;
    private int _loopIndex = 0;
    private bool _enable = false;

    private readonly int FRAME_DELAY_TAG = 0x5100;
    private readonly int LOOP_COUNT_TAG = 20737;


    /// <summary>
    /// Gets the bitmap.
    /// </summary>
    public Bitmap? Bitmap => _bitmap;

    /// <summary>
    /// Occurs when the image frame is changed.
    /// </summary>
    public event EventHandler<EventArgs> FrameChanged;


    /// <summary>
    /// Initialize new instance of <see cref="GifAnimator"/>.
    /// </summary>
    public GifAnimator(Bitmap bmp)
    {
        // request for high resolution gif animation
        if (!TimerApi.HasRequestedRateAtLeastAsFastAs(10) && TimerApi.TimeBeginPeriod(10))
        {
            SetTickTimeInMilliseconds(10);
        }

        lock (bmp)
        {
            _bitmap = bmp;
            LoadGifMetadata();
        }
    }


    /// <summary>
    /// Starts animating the image frames.
    /// </summary>
    public void Animate()
    {
        if (_bitmap == null) return;

        _enable = true;

        var _thHeartBeat = new Thread(OnThreadHeartBeatTicked);
        _thHeartBeat.IsBackground = true;
        _thHeartBeat.Name = "heartbeat - GifAnimator";
        _thHeartBeat.Start();
    }


    /// <summary>
    /// Stops image animation.
    /// </summary>
    public void StopAnimate()
    {
        _enable = false;
    }


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


    /// <summary>
    /// Process image frame tick
    /// </summary>
    private void OnThreadHeartBeatTicked()
    {
        var initSleepTime = GetSleepAmountInMilliseconds(_frameDelays[_frameIndex]);
        Thread.Sleep(initSleepTime);

        while (_enable)
        {
            _frameIndex++;
            if (_frameIndex >= _frameCount)
            {
                _frameIndex = 0;
                _loopIndex++;

                if (_maxLoopCount > 0 && _loopIndex >= _maxLoopCount)
                {
                    _enable = false;
                    return;
                }
            }


            try
            {
                lock (_bitmap)
                {
                    // update frame
                    _bitmap.SetActiveTimeFrame(_frameIndex);
                }

                FrameChanged?.Invoke(_bitmap, EventArgs.Empty);


                var sleepTime = GetSleepAmountInMilliseconds(_frameDelays[_frameIndex]);
                Thread.Sleep(sleepTime);
            }
            catch (ArgumentException)
            {
                // ignore errors that occur due to the image being disposed
            }
            catch (OutOfMemoryException)
            {
                // also ignore errors that occur due to running out of memory
            }
            catch (ExternalException)
            {
                // ignore
            }
            catch (InvalidOperationException)
            {
                // issue #373: a race condition caused this exception: deleting
                // the image from underneath us could cause a collision in
                // HighResolutionGif_animator. I've not been able to repro;
                // hopefully this is the correct response.

                // ignore
            }
        }
    }


    /// <summary>
    /// Sets the tick for the animation thread. The thread may use a lower tick to ensure
    /// the passed value is divisible by 10 (the gif format operates in units of 10 ms).
    /// </summary>
    /// <param name="value"> Ideally should be a multiple of 10. </param>
    /// <returns>The actual tick value that will be used</returns>
    private void SetTickTimeInMilliseconds(int value)
    {
        // 10 is the minimum value, as a GIF's lowest tick rate is 10ms 
        //
        var newTickValue = Math.Max(10, (value / 10) * 10);
        _minTickTimeInMillisecond = newTickValue;
    }


    /// <summary>
    /// Given a delay amount, return either the minimum tick or delay, whichever is greater.
    /// </summary>
    /// <returns> the time to sleep during a tick in milliseconds </returns>
    private int GetSleepAmountInMilliseconds(int delayInMilliseconds)
    {
        return Math.Max(_minTickTimeInMillisecond, delayInMilliseconds);
    }

}


