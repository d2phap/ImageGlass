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
using Avalonia.Controls;
using ImageGlass.Common.Extensions;
using ImageGlass.UI.Viewer.Transitions;
using SkiaSharp;
using System;
using System.Threading;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    // the old frame stays on screen at most this long while the new photo decodes
    private const int MAX_TRANSITION_HOLD_MS = 3000;

    // read by PhotoRenderer under TransitionLock
    internal ViewerTransition? _transition;


    /// <summary>
    /// Gets the lock guarding <see cref="_transition"/>: that of the source for a mirror, whose renderer draws under it.
    /// </summary>
    private Lock TransitionLock => MirrorSource?._lock ?? _lock;


    /// <summary>
    /// Finishes the playing transition at once, showing the current photo.
    /// </summary>
    public void CompleteTransition()
    {
        CompleteTransition(TransitionLock);
    }


    /// <summary>
    /// Finishes the playing transition at once, taking it out from under <paramref name="transitionLock"/>.
    /// </summary>
    private void CompleteTransition(Lock transitionLock)
    {
        ViewerTransition? transition;
        lock (transitionLock)
        {
            transition = _transition;
            _transition = null;
        }

        if (transition is null) return;

        // safe outside the lock: the renderer only reads _transition while holding it
        transition.Dispose();
        InvalidateVisual();
    }


    /// <summary>
    /// Captures the frame on screen and starts <paramref name="request"/>; must run before the old photo unloads.
    /// </summary>
    private void BeginTransition(TransitionRequest? request)
    {
        // the mirrors record their own old frames now, as the old images go right after this
        TransitionStarting?.Invoke(this, request);

        // a transition still holding its old frame keeps showing it, so the next one starts from it too
        SKPicture? heldFrame = null;
        lock (TransitionLock)
        {
            if (request is not null && _transition is { StartTime: null } holding)
            {
                heldFrame = holding.TakeFromFrame();
            }
        }

        CompleteTransition();
        if (request is null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        var canPlay = topLevel is not null && IsEffectivelyVisible;
        var fromFrame = canPlay
            ? heldFrame ?? CaptureTransitionFrame()
            : null;

        if (!canPlay) heldFrame?.Dispose();

        if (fromFrame is null)
        {
            request.Complete();
            return;
        }

        var transition = new ViewerTransition(request, fromFrame);
        lock (TransitionLock)
        {
            _transition = transition;
        }

        topLevel!.RequestAnimationFrame(ts => OnTransitionFrame(ts, transition));
    }


    /// <summary>
    /// Records the photo on screen as a picture; <c>null</c> if there is nothing to transition from.
    /// </summary>
    private SKPicture? CaptureTransitionFrame()
    {
        if (MirrorSource is { } source) return CaptureMirrorFrame(source);

        lock (_lock)
        {
            var hasImage = _imgRender is not null || _imgSource is not null;
            var hasVector = _svgPicture is not null && !_svgPicture.IsDisposed();
            if (!hasImage && !hasVector) return null;
            if (DrawingArea.Width < 1 || DrawingArea.Height < 1) return null;

            using var renderer = new PhotoRenderer(this, null);
            return renderer.RecordFrame();
        }
    }


    /// <summary>
    /// Advances <paramref name="transition"/> by one animation frame.
    /// </summary>
    private void OnTransitionFrame(TimeSpan ts, ViewerTransition transition)
    {
        bool hasTarget;
        lock (TransitionLock)
        {
            // superseded or finished
            if (!ReferenceEquals(_transition, transition)) return;

            hasTarget = HasTransitionTarget();
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            CompleteTransition();
            return;
        }

        // hold the old frame until the new photo can be drawn, so no blank or preview frame flashes
        if (transition.StartTime is null)
        {
            var heldMs = Environment.TickCount64 - transition.HoldStartTick;
            if (!hasTarget && heldMs < MAX_TRANSITION_HOLD_MS)
            {
                topLevel.RequestAnimationFrame(t => OnTransitionFrame(t, transition));
                return;
            }

            transition.StartTime = ts;
        }

        var elapsedMs = (ts - transition.StartTime.Value).TotalMilliseconds;
        var linear = Math.Clamp(elapsedMs / transition.Request.DurationMs, 0, 1);

        lock (TransitionLock)
        {
            transition.Progress = (float)EaseInOutCubic(linear);
        }

        if (linear >= 1)
        {
            CompleteTransition();
            return;
        }

        InvalidateVisual();
        topLevel.RequestAnimationFrame(t => OnTransitionFrame(t, transition));
    }


    /// <summary>
    /// Whether the new photo has something to draw, or never will (no photo, or it failed to load).
    /// </summary>
    private bool HasTransitionTarget()
    {
        // a mirror draws the photo of its source
        if (MirrorSource is { } source) return source.HasTransitionTarget();

        if (Photo is null || Photo.Error is not null) return true;

        return _imgSource is not null || _svgPicture is not null;
    }


    private static double EaseInOutCubic(double t)
    {
        return t < 0.5
            ? 4 * t * t * t
            : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }

}
