/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using System.Runtime.InteropServices;

namespace ImageGlass.Base.Photoing.Animators;

/// <summary>
/// Image animator
/// </summary>
public class ImgAnimator : IDisposable
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

            OnDisposing();
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ImgAnimator()
    {
        Dispose(false);
    }

    #endregion


    protected int _frameCount = 0;
    protected int _frameIndex = 0;
    protected int _maxLoopCount = 0; // 0 - infinite loop
    protected int _loopIndex = 0;

    protected bool _enable = false;
    protected TimeSpan _minTickTimeInMillisecond = TimeSpan.FromMilliseconds(20);


    /// <summary>
    /// Occurs when the image frame is changed. The <b><c>sender</c></b> object contains the frame resource.
    /// </summary>
    public event EventHandler FrameChanged;


    /// <summary>
    /// Initialize new instance of <see cref="ImgAnimator"/>.
    /// </summary>
    public ImgAnimator()
    {
        // request for high resolution gif animation
        if (!TimerApi.HasRequestedRateAtLeastAsFastAs(10) && TimerApi.TimeBeginPeriod(10))
        {
            SetTickTimeInMilliseconds(10);
        }
    }


    // Public methods
    #region Public methods

    /// <summary>
    /// Starts animating the image frames.
    /// </summary>
    public void Animate()
    {
        if (!CanAnimate()) return;

        _enable = true;

        var _thHeartBeat = new Thread(HandleThreadHeartBeatTicked);
        _thHeartBeat.IsBackground = true;
        _thHeartBeat.Name = "heartbeat - ImageAnimator";
        _thHeartBeat.Start();
    }


    /// <summary>
    /// Stops image animation.
    /// </summary>
    public void StopAnimate()
    {
        _enable = false;
    }

    #endregion // Public methods


    // Protected virtual methods
    #region Protected virtual methods

    /// <summary>
    /// Verifies the animated source before calling <see cref="Animate"/>.
    /// </summary>
    /// <returns>If <c>false</c>, does not proceed to call <see cref="Animate"/></returns>
    protected virtual bool CanAnimate()
    {
        return true;
    }


    /// <summary>
    /// Gets delay time of frame.
    /// <b>This function needs to be re-implemented in the inherited class.</b>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual TimeSpan GetFrameDelay(int frameIndex)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Occurs when the thread of image frames ticked.
    /// <b>This function needs to be re-implemented in the inherited class.</b>
    /// <para>
    /// This is where to get and update image frame.
    /// </para>
    /// </summary>
    /// <param name="frameIndex">The frame index</param>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual void UpdateFrame(int frameIndex)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Occurs when the <see cref="ImgAnimator"/> instance is being disposed.
    /// </summary>
    protected virtual void OnDisposing()
    {
        //
    }

    #endregion // Protected virtual methods


    // Private / Protected methods
    #region Private / Protected methods

    /// <summary>
    /// Process image frame tick.
    /// </summary>
    private void HandleThreadHeartBeatTicked()
    {
        var initSleepTime = GetSleepAmountInMilliseconds(GetFrameDelay(_frameIndex));
        Thread.Sleep(initSleepTime);

        while (_enable)
        {
            try
            {
                UpdateFrame(_frameIndex);

                var sleepTime = GetSleepAmountInMilliseconds(GetFrameDelay(_frameIndex));
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
        }
    }


    /// <summary>
    /// Emits the event <see cref="FrameChanged"/>.
    /// It passes <paramref name="frameBitmapObj"/> to <see cref="FrameChanged"/> as a sender.
    /// </summary>
    /// <param name="frameBitmapObj">Frame bitmap object</param>
    protected void TriggerFrameChangedEvent(object? frameBitmapObj)
    {
        FrameChanged?.Invoke(frameBitmapObj, EventArgs.Empty);
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
        _minTickTimeInMillisecond = TimeSpan.FromMilliseconds(newTickValue);
    }


    /// <summary>
    /// Given a delay amount, return either the minimum tick or delay, whichever is greater.
    /// </summary>
    /// <returns> the time to sleep during a tick in milliseconds </returns>
    private TimeSpan GetSleepAmountInMilliseconds(TimeSpan delay)
    {
        if (delay > _minTickTimeInMillisecond)
        {
            return delay;
        }

        return _minTickTimeInMillisecond;
    }

    #endregion // Private / Protected methods

}