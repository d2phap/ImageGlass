/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using SkiaSharp;
using Svg.Skia;
using System;
using System.Threading;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Drives SMIL animation for SVG documents via <see cref="SKSvg.AdvanceAnimation"/>.
/// The <see cref="SKSvg"/> document is NOT owned by this animator; the viewer
/// manages its lifetime separately via <c>DisposeVectorResources()</c>.
/// </summary>
public sealed class SvgAnimator : AnimatorImpl
{
    private readonly SKSvg _svgDocument;
    private readonly Lock _viewerLock;
    private readonly Action<SKPicture?> _onPictureChanged;
    private readonly Action _invalidateVisual;
    private readonly Lock _syncLock = new();
    private DispatcherTimer _timer;

    /// <summary>
    /// Tracks the last known picture reference to detect changes after advancing.
    /// </summary>
    private SKPicture? _lastKnownPicture;

    /// <summary>
    /// Whether we have already fired the first <see cref="AnimatorImpl.FrameChanged"/>
    /// notification. SVG animation frame info is constant (always frame 0/1),
    /// so we only notify once to avoid per-tick GC pressure from delegate and
    /// event args allocations.
    /// </summary>
    private bool _hasNotifiedFirstFrame;


    /// <summary>
    /// Initializes a new instance of <see cref="SvgAnimator"/>.
    /// </summary>
    /// <param name="svgDocument">The SVG document with SMIL animations (not owned).</param>
    /// <param name="viewerLock">The viewer's <c>_lock</c> to synchronize picture updates
    /// with the render thread.</param>
    /// <param name="onPictureChanged">Callback invoked under <paramref name="viewerLock"/>
    /// when the animation produces a new <see cref="SKPicture"/>. Typically sets
    /// <c>_svgPicture</c> on the viewer.</param>
    /// <param name="invalidateVisual">Callback to trigger a visual refresh after a frame
    /// change. Called outside the lock.</param>
    public SvgAnimator(
        SKSvg svgDocument,
        Lock viewerLock,
        Action<SKPicture?> onPictureChanged,
        Action invalidateVisual)
        : base([new SKCodecFrameInfo()])
    {
        _svgDocument = svgDocument;
        _viewerLock = viewerLock;
        _onPictureChanged = onPictureChanged;
        _invalidateVisual = invalidateVisual;
        _loopCount = 0; // infinite (SMIL handles its own looping)
        _lastKnownPicture = svgDocument.Picture;

        _timer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher.UIThread);
        _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps polling
        _timer.Tick += Timer_Tick;
    }


    /// <inheritdoc/>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        lock (_syncLock)
        {
            StopTimer();
            _timer.Tick -= Timer_Tick;
        }

        _lastKnownPicture = null;
    }


    /// <inheritdoc/>
    protected override void StartTimer()
    {
        _timer.Start();
    }


    /// <inheritdoc/>
    protected override void StopTimer()
    {
        _timer.Stop();
    }


    private void Timer_Tick(object? sender, EventArgs e)
    {
        OnTimerTicked();
    }


    /// <summary>
    /// Advances the SVG animation clock and updates the viewer's picture reference
    /// if the animation produced a new frame.
    /// </summary>
    protected override void OnTimerTicked()
    {
        if (_isPaused || IsDisposed) return;

        var now = _stopwatch.Elapsed;
        var delta = now - _lastFrameTime;
        _lastFrameTime = now;

        bool changed;

        // Hold the viewer lock around AdvanceAnimation() so the render thread
        // cannot read a stale/disposed _svgPicture between the internal picture
        // rebuild and our update of the cached reference.
        lock (_viewerLock)
        {
            _svgDocument.AdvanceAnimation(delta);

            if (_svgDocument.HasPendingAnimationFrame)
            {
                _svgDocument.FlushPendingAnimationFrame();
            }

            var newPicture = _svgDocument.Picture;
            changed = !ReferenceEquals(_lastKnownPicture, newPicture);

            if (changed)
            {
                _lastKnownPicture = newPicture;
                _onPictureChanged(newPicture);
            }
        }

        if (changed)
        {
            _invalidateVisual();

            // Only notify frame change once - SVG animation frame info is constant
            // (always frame 0/1). Skipping subsequent notifications avoids per-tick
            // GC pressure from delegate and event args allocations in OnFrameChanged.
            if (!_hasNotifiedFirstFrame)
            {
                _hasNotifiedFirstFrame = true;
                OnFrameChanged(new AnimatorFrameChangedEventArgs
                {
                    CurrentFrame = 0,
                    CurrentLoop = (uint)_currentLoop,
                    FrameCount = 1,
                    LoopCount = (uint)_loopCount,
                });
            }
        }
    }


    /// <summary>
    /// Rasterizes the current SVG picture to an <see cref="SKImage"/> for pixel
    /// operations (copy, export, frame-nav thumbnail).
    /// </summary>
    public override SKImage? GetRenderedFrameBitmap(uint frameIndex)
    {
        lock (_viewerLock)
        {
            var picture = _svgDocument.Picture;
            if (picture is null) return null;

            return SvgCodec.RasterizeThumbnail(picture, 1024);
        }
    }


    /// <summary>
    /// Seeks the SVG animation to the specified time.
    /// </summary>
    public void SeekToTime(TimeSpan time)
    {
        lock (_viewerLock)
        {
            _svgDocument.SetAnimationTime(time);

            if (_svgDocument.HasPendingAnimationFrame)
            {
                _svgDocument.FlushPendingAnimationFrame();
            }

            var newPicture = _svgDocument.Picture;
            _lastKnownPicture = newPicture;
            _onPictureChanged(newPicture);
        }

        // Reset timing for accurate delta tracking after seek
        _lastFrameTime = _stopwatch.Elapsed;

        _invalidateVisual();

        OnFrameChanged(new AnimatorFrameChangedEventArgs
        {
            CurrentFrame = 0,
            CurrentLoop = (uint)_currentLoop,
            FrameCount = 1,
            LoopCount = (uint)_loopCount,
        });
    }


    /// <summary>
    /// Resets the SVG animation to the beginning.
    /// </summary>
    public void ResetAnimation()
    {
        lock (_viewerLock)
        {
            _svgDocument.ResetAnimation();

            if (_svgDocument.HasPendingAnimationFrame)
            {
                _svgDocument.FlushPendingAnimationFrame();
            }

            var newPicture = _svgDocument.Picture;
            _lastKnownPicture = newPicture;
            _onPictureChanged(newPicture);
        }

        ResetTimer();
        _invalidateVisual();
    }
}
