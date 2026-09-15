/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Threading;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides functionality for animating image frames with customizable frame delays and looping behavior.
/// </summary>
public abstract class AnimatorImpl : PhDisposable
{
    protected SKCodecFrameInfo[] _frames;
    protected int _frameCount = 0;
    protected int _loopCount = 0; // 0 - infinite loop
    protected int _currentFrame = 0;
    protected int _currentLoop = 0;

    protected bool _isDecoded = false; // check if frames are decoded
    protected bool _isPaused = true;
    protected Stopwatch _stopwatch = new();
    protected TimeSpan _lastFrameTime = TimeSpan.Zero;
    protected TimeSpan _pauseStartTime = TimeSpan.Zero;
    private bool _needsInitialFrame = true;


    /// <summary>
    /// How far playback may fall behind before the clock resyncs instead of catching up.
    /// </summary>
    private static readonly TimeSpan MAX_CATCH_UP = TimeSpan.FromSeconds(1);


    /// <summary>
    /// Occurs when the image frame is changed.
    /// </summary>
    public event TEventHandler<AnimatorImpl, AnimatorFrameChangedEventArgs>? FrameChanged;


    /// <summary>
    /// Occurs when the animation stopped or the loop has completed.
    /// </summary>
    public event TEventHandler<AnimatorImpl, EventArgs>? Stopped;


    #region Public Properties

    /// <summary>
    /// Gets frames.
    /// </summary>
    public SKCodecFrameInfo[] Frames => _frames;


    /// <summary>
    /// Gets the current frame.
    /// </summary>
    public uint CurrentFrame => (uint)_currentFrame;


    /// <summary>
    /// Gets the current loop.
    /// </summary>
    public uint CurrentLoop => (uint)_currentLoop;


    /// <summary>
    /// Gets the loop count.
    /// </summary>
    public uint LoopCount => (uint)_loopCount;


    /// <summary>
    /// Gets the playback status.
    /// </summary>
    public bool IsPlaying => !_isPaused;

    #endregion // Public Properties





    /// <summary>
    /// Initialize new instance of <see cref="AnimatorImpl"/>.
    /// </summary>
    public AnimatorImpl(SKCodecFrameInfo[] frames)
    {
        _frames = frames;
        _frameCount = frames.Length;
    }


    /// <summary>
    /// Start the animator timer.
    /// </summary>
    protected abstract void StartTimer();


    /// <summary>
    /// Stop the animator timer.
    /// </summary>
    protected abstract void StopTimer();


    /// <summary>
    /// Renders the current frame of the animation and returns the resulting bitmap.
    /// </summary>
    public abstract SKImage? GetRenderedFrameBitmap(uint frameIndex);



    // Public methods
    #region Public methods


    /// <summary>
    /// Restarts animation.
    /// </summary>
    public void Restart()
    {
        Pause();

        // reset timer
        ResetTimer();
        _stopwatch.Restart();

        Play();
    }


    /// <summary>
    /// Starts or resumes the animation.
    /// </summary>
    public void Play()
    {
        if (!_isDecoded)
        {
            _isDecoded = true;

            _stopwatch.Restart();
            _lastFrameTime = TimeSpan.Zero;
        }

        if (_isPaused)
        {
            // shift lastFrameTime forward by pause duration to preserve correct timing
            var pausedDuration = _stopwatch.Elapsed - _pauseStartTime;
            _lastFrameTime += pausedDuration;
        }

        _isPaused = false;
        StartTimer();

        // frame N owns delay[N], so the first frame is shown now and held for its own delay
        if (_needsInitialFrame)
        {
            _needsInitialFrame = false;
            _lastFrameTime = _stopwatch.Elapsed;

            RaiseFrameChanged();
        }
    }


    /// <summary>
    /// Pauses the animation.
    /// </summary>
    public void Pause()
    {
        if (!_isDecoded || _isPaused) return;

        StopTimer();
        _isPaused = true;
        _pauseStartTime = _stopwatch.Elapsed;
    }


    /// <summary>
    /// Seeks the animation to the specified frame index.
    /// </summary>
    public void SeekToFrame(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= _frameCount) return;

        _currentFrame = frameIndex;
        _needsInitialFrame = false;

        // the seeked frame is on screen now, so it gets its full delay from here
        _lastFrameTime = _stopwatch.Elapsed;

        RaiseFrameChanged();
    }

    #endregion // Public methods



    // Protected virtual methods
    #region Protected virtual methods


    /// <summary>
    /// Occurs when the <see cref="AnimatorImpl"/> instance is being disposed.
    /// </summary>
    protected override void OnDisposing()
    {
        StopTimer();

        _currentFrame = 0;
        _frameCount = 0;
        _loopCount = 0;
        _currentLoop = 0;

        foreach (var eventDelegate in FrameChanged?.GetInvocationList() ?? [])
        {
            FrameChanged -= (TEventHandler<AnimatorImpl, AnimatorFrameChangedEventArgs>)eventDelegate;
        }

        foreach (var eventDelegate in Stopped?.GetInvocationList() ?? [])
        {
            Stopped -= (TEventHandler<AnimatorImpl, EventArgs>)eventDelegate;
        }

        base.OnDisposing();
    }


    /// <summary>
    /// Resets the internal timer and frame, loop index to 0.
    /// </summary>
    protected void ResetTimer()
    {
        _stopwatch.Reset();

        _lastFrameTime = TimeSpan.Zero;
        _pauseStartTime = TimeSpan.Zero;

        _currentFrame = 0;
        _currentLoop = 0;
        _needsInitialFrame = true;
    }


    /// <summary>
    /// Handles the timer tick event to update the animation state.
    /// </summary>
    /// <remarks>
    /// If the maximum loop count is reached, the animation stops,
    /// and the <see cref="Stopped"/> event is raised.
    /// </remarks>
    protected virtual void OnTimerTicked()
    {
        if (_isPaused || _frameCount == 0) return;

        var now = _stopwatch.Elapsed;
        var frameDelay = GetFrameDelay(_currentFrame);

        // the frame on screen has not reached its delay yet
        if ((now - _lastFrameTime) < frameDelay) return;

        // advance by the frame's own delay, never to the tick time: snapping there discards the
        // sub-tick remainder, which rounds every frame up to a whole poll interval
        _lastFrameTime += frameDelay;

        // a long UI stall would otherwise fast-forward a frame per tick until the backlog cleared
        if (now - _lastFrameTime > MAX_CATCH_UP)
        {
            _lastFrameTime = now;
        }

        _currentFrame++;

        // check for loop
        if (_currentFrame >= _frameCount)
        {
            _currentFrame = 0;
            _currentLoop++;

            if (_loopCount > 0 && _currentLoop >= _loopCount)
            {
                // hold the last frame rather than snapping back to the first one
                _currentFrame = _frameCount - 1;
                Pause();

                // loop ended
                OnStopped(EventArgs.Empty);
                return;
            }
        }

        RaiseFrameChanged();
    }


    /// <summary>
    /// Gets the delay duration for a specific animation frame.
    /// Includes compatibility fix for 0 or very small delays.
    /// </summary>
    protected virtual TimeSpan GetFrameDelay(int frameIndex)
    {
        var frameMeta = _frames[frameIndex];
        var delayMs = frameMeta.Duration;

        // 2. Browser/Legacy Compatibility Hack
        // If delay is <= 10ms (<= 1 GIF tick), it is likely invalid or intended to be default speed.
        // Most browsers force this to 100ms (10fps).
        if (delayMs <= 10)
        {
            delayMs = 100;
        }

        return TimeSpan.FromMilliseconds(delayMs);
    }


    /// <summary>
    /// Raises <see cref="FrameChanged"/> for the frame that is now on screen.
    /// </summary>
    private void RaiseFrameChanged()
    {
        OnFrameChanged(new AnimatorFrameChangedEventArgs()
        {
            CurrentFrame = (uint)_currentFrame,
            CurrentLoop = (uint)_currentLoop,
            FrameCount = (uint)_frameCount,
            LoopCount = (uint)_loopCount,
        });
    }


    /// <summary>
    /// Raises <c><see cref="FrameChanged"/></c> event.
    /// </summary>
    protected virtual void OnFrameChanged(AnimatorFrameChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            FrameChanged?.Invoke(this, e);
        });
    }


    /// <summary>
    /// Raises <c><see cref="Stopped"/></c> event.
    /// </summary>
    protected virtual void OnStopped(EventArgs e)
    {
        Stopped?.Invoke(this, e);
    }


    #endregion // Protected virtual methods



}
