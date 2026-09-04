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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Avalonia.Threading;
using ImageGlass.Common.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

/// <summary>
/// Manages the slideshow timer and countdown state.
/// </summary>
public sealed class SlideshowProvider : PhDisposable
{
    /// <summary>
    /// Interval floor. A configured <c>0</c> (or invalid value) must still wait once per cycle,
    /// otherwise the loop spins without yielding and floods the UI thread with advance requests.
    /// </summary>
    public const double MIN_INTERVAL_MS = 50;

    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _intervalCts;
    private SemaphoreSlim? _pauseGate;
    private readonly Lock _lock = new();

    // countdown state — read from UI thread for rendering
    private long _countdownSecondsBits;
    private volatile bool _isRunning;
    private volatile bool _isPaused;

    // remaining milliseconds saved when paused, so resume continues the countdown
    private double _remainingMsOnPause;

    // beep notification counter
    private int _beepImageCount;


    /// <summary>
    /// Raised on the UI thread when the slideshow should advance to the next photo.
    /// </summary>
    public event Action? NextPhotoRequested;



    #region Public Properties

    /// <summary>
    /// Gets whether the slideshow is currently running (may be paused).
    /// </summary>
    public bool IsRunning => _isRunning;


    /// <summary>
    /// Gets whether the slideshow is currently paused.
    /// </summary>
    public bool IsPaused => _isPaused;


    /// <summary>
    /// Gets the remaining countdown seconds for the current interval.
    /// </summary>
    public double CountdownSeconds => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _countdownSecondsBits));

    #endregion // Public Properties



    #region Public Methods

    /// <summary>
    /// Starts the slideshow loop.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            Stop_Locked();

            _cts = new CancellationTokenSource();
            _pauseGate = new SemaphoreSlim(0, 1);
            _isRunning = true;
            _isPaused = false;
            _beepImageCount = 1; // the initial image counts as #1
            SetCountdown(0);

            // playback is hands-off, so the OS must not blank the screen or suspend
            Core.ShellProvider?.PreventSleep($"{BHelper.AppDisplayName} slideshow");

            var token = _cts.Token;
            _ = RunLoopAsync(token);
        }
    }


    /// <summary>
    /// Stops the slideshow loop completely.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            Stop_Locked();
        }
    }


    /// <summary>
    /// Pauses the slideshow. The countdown freezes and no photos advance
    /// until <see cref="Resume"/> is called.
    /// </summary>
    public void Pause()
    {
        lock (_lock)
        {
            if (!_isRunning || _isPaused) return;

            // save the remaining countdown so Resume can continue from it
            _remainingMsOnPause = CountdownSeconds * 1000.0;
            _isPaused = true;
            _intervalCts?.Cancel();
        }
    }


    /// <summary>
    /// Resumes the slideshow from a paused state, starting a new countdown.
    /// </summary>
    public void Resume()
    {
        lock (_lock)
        {
            if (!_isRunning || !_isPaused) return;

            _isPaused = false;

            // release the gate if the loop is waiting
            if (_pauseGate is { CurrentCount: 0 })
            {
                _pauseGate.Release();
            }
        }
    }


    /// <summary>
    /// Resets the current interval countdown so a new cycle starts immediately.
    /// Also resets the beep notification counter.
    /// Called when the user manually navigates to another photo.
    /// </summary>
    public void ResetInterval()
    {
        lock (_lock)
        {
            _beepImageCount = 1; // the navigated-to image counts as #1
            _remainingMsOnPause = 0; // force a fresh interval
            _intervalCts?.Cancel();
        }
    }

    #endregion // Public Methods



    #region Private Methods

    protected override void OnDisposing()
    {
        base.OnDisposing();

        lock (_lock)
        {
            Stop_Locked();
        }
    }


    private void Stop_Locked()
    {
        Core.ShellProvider?.AllowSleep();

        _isRunning = false;
        _isPaused = false;
        _beepImageCount = 0;
        _remainingMsOnPause = 0;
        SetCountdown(0);

        _intervalCts?.Cancel();
        _intervalCts?.Dispose();
        _intervalCts = null;

        if (_cts is not null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        _pauseGate?.Dispose();
        _pauseGate = null;
    }


    private async Task RunLoopAsync(CancellationToken token)
    {
        const int tickMs = 50; // update countdown ~20 times/sec

        try
        {
            while (!token.IsCancellationRequested)
            {
                // 0. if paused, wait until resumed
                if (_isPaused)
                {
                    if (_pauseGate is not null)
                    {
                        await _pauseGate.WaitAsync(token).ConfigureAwait(false);
                    }
                    continue;
                }


                // 1. compute interval for this cycle
                // use remaining time from pause if available, otherwise a fresh interval
                double intervalMs;
                lock (_lock)
                {
                    if (_remainingMsOnPause > 0)
                    {
                        intervalMs = _remainingMsOnPause;
                        _remainingMsOnPause = 0;
                    }
                    else
                    {
                        intervalMs = GetNextIntervalMs();
                    }
                }
                SetCountdown(intervalMs / 1000.0);


                // 2. create a linked CTS for this interval (allows ResetInterval/Pause to cancel it)
                CancellationTokenSource intervalCts;
                lock (_lock)
                {
                    _intervalCts?.Dispose();
                    _intervalCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                    intervalCts = _intervalCts;
                }
                var intervalToken = intervalCts.Token;


                // 3. count down in small ticks; always wait at least once so the loop cannot spin
                try
                {
                    var remaining = intervalMs;
                    do
                    {
                        var delay = Math.Max(1, (int)Math.Ceiling(Math.Min(tickMs, remaining)));
                        await Task.Delay(delay, intervalToken).ConfigureAwait(false);

                        remaining -= delay;
                        SetCountdown(Math.Max(0, remaining / 1000.0));
                    }
                    while (remaining > 0 && !intervalToken.IsCancellationRequested);
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested)
                {
                    // interval was cancelled by ResetInterval or Pause, restart loop
                    continue;
                }

                if (token.IsCancellationRequested) break;


                // 4. check end of list without loop -> auto-pause
                var isLastPhoto = Core.Photos.CurrentIndex >= (int)Core.Photos.Count - 1;
                if (isLastPhoto && !Core.Config.EnableLoopSlideshow)
                {
                    _isPaused = true;
                    SetCountdown(0);
                    continue; // will wait on pause gate at top of loop
                }


                // 5. advance to next photo and update beep counter
                _beepImageCount++;
                Dispatcher.UIThread.Post(() => NextPhotoRequested?.Invoke());


                // 6. check beep notification
                var notifyCount = (int)Core.Config.SlideshowImagesToNotifySound;
                if (notifyCount > 0 && _beepImageCount >= notifyCount)
                {
                    _beepImageCount = 0;
                    _ = Task.Run(BHelper.PlayNotificationSoundAsync, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // expected on stop
        }
        finally
        {
            _isRunning = false;
            _isPaused = false;
            SetCountdown(0);
        }
    }


    private void SetCountdown(double value)
    {
        Interlocked.Exchange(ref _countdownSecondsBits, BitConverter.DoubleToInt64Bits(value));
    }


    /// <summary>
    /// Computes the next interval in milliseconds,
    /// respecting random interval and config boundaries.
    /// </summary>
    private static double GetNextIntervalMs()
    {
        var intervalFrom = Core.Config.SlideshowInterval;
        var intervalTo = Core.Config.SlideshowIntervalTo;
        var intervalMs = intervalFrom * 1000.0;

        if (Core.Config.EnableSlideshowRandomInterval && intervalTo > intervalFrom)
        {
            // Random.Shared is thread-safe
            var range = intervalTo - intervalFrom;
            var randomOffset = Random.Shared.NextDouble() * range;

            intervalMs = (intervalFrom + randomOffset) * 1000.0;
        }

        // Math.Max would propagate NaN, so reject it explicitly
        if (double.IsNaN(intervalMs)) return MIN_INTERVAL_MS;

        return Math.Max(MIN_INTERVAL_MS, intervalMs);
    }

    #endregion // Private Methods


}
