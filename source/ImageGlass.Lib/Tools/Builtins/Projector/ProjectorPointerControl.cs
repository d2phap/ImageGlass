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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.UI;
using System;
using System.Collections.Generic;

namespace ImageGlass.Tools;


/// <summary>
/// Marks where the cursor is on the main viewer, on a projector: a see-through circle with a dot, which ripples on a click.
/// </summary>
public sealed class ProjectorPointerControl : PhControl
{
    // the circle takes this share of the shorter side of the projector, within these sizes
    private const double RADIUS_RATIO = 0.025;
    private const double MIN_RADIUS = 12;
    private const double MAX_RADIUS = 36;
    private const double DOT_RATIO = 0.2;

    // the circle lets the photo show through, while the dot marks the exact point
    private const int CIRCLE_ALPHA = 165;
    private const int DOT_ALPHA = 255;

    // a soft shadow keeps the circle apart from bright photos, where yellow alone fades
    private const double SHADOW_BLUR = 10;
    private const double SHADOW_OPACITY = 0.6;

    // a click sends a ring out to this many times the circle, while the circle dips by this share
    private const double RIPPLE_SCALE = 2.4;
    private const float RIPPLE_WIDTH = 3f;
    private const double PRESS_DEPTH = 0.2;

    private const double CLICK_MS = 450;
    private const double FADE_MS = 150;

    // moved without a layout pass, so following the cursor costs no more than a redraw of its spot
    private readonly TranslateTransform _offset = new();

    // the start of each click playing; null until its first frame
    private readonly List<TimeSpan?> _clicks = [];

    private double _radius = MIN_RADIUS;
    private double _opacity;
    private bool _isShown;
    private bool _isAnimating;
    private TimeSpan _lastFrameTime;
    private TimeSpan _frameTime;


    public ProjectorPointerControl()
    {
        IsHitTestVisible = false;
        RenderTransform = _offset;
        Width = GetExtent();
        Height = Width;

        Effect = new DropShadowEffect
        {
            Color = Colors.Black,
            BlurRadius = SHADOW_BLUR,
            Opacity = SHADOW_OPACITY,
            OffsetX = 0,
            OffsetY = 0,
        };
    }



    #region Public Methods

    /// <summary>
    /// Moves the pointer to <paramref name="center"/> in an area of <paramref name="areaSize"/>, showing it.
    /// </summary>
    public void MoveTo(Point center, Size areaSize)
    {
        // 1. sized for its projector, so a tile gets a smaller circle than a whole screen
        var shorterSide = Math.Min(areaSize.Width, areaSize.Height);
        var radius = Math.Clamp(shorterSide * RADIUS_RATIO, MIN_RADIUS, MAX_RADIUS);
        var isNewSize = Math.Abs(radius - _radius) > 0.01;
        if (isNewSize)
        {
            _radius = radius;
            Width = GetExtent();
            Height = Width;
            InvalidateVisual();
        }

        // 2. its middle on the point
        var halfExtent = GetExtent() / 2;
        _offset.X = center.X - halfExtent;
        _offset.Y = center.Y - halfExtent;

        // 3. fading in where it was hidden
        if (_isShown) return;

        _isShown = true;
        StartAnimation();
    }


    /// <summary>
    /// Fades the pointer out.
    /// </summary>
    public void Hide()
    {
        if (!_isShown) return;

        _isShown = false;
        StartAnimation();
    }


    /// <summary>
    /// Plays a click: a ring that spreads out from the circle while the circle dips.
    /// </summary>
    public void PlayClick()
    {
        // a click away from the photo has no point to show
        if (!_isShown) return;

        _clicks.Add(null);
        StartAnimation();
    }

    #endregion // Public Methods



    #region Render & Animation

    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var hasClicks = _clicks.Count > 0;
        var isFadedOut = _opacity <= 0;
        if (isFadedOut && !hasClicks) return;

        var circleColor = AppThemeColors.ProjectorPointerCircle;
        var dotColor = AppThemeColors.ProjectorPointerDot;
        var centerX = Bounds.Width / 2;
        var centerY = Bounds.Height / 2;
        var pressScale = 1d;


        // 1. each click sends a ring out, slowing as it fades
        foreach (var start in _clicks)
        {
            if (start is not { } startTime) continue;

            var progress = Math.Clamp((_frameTime - startTime).TotalMilliseconds / CLICK_MS, 0, 1);
            var eased = 1 - Math.Pow(1 - progress, 3);
            var ringRadius = _radius * (1 + (RIPPLE_SCALE - 1) * eased);
            var ringAlpha = (int)(DOT_ALPHA * (1 - progress) * _opacity);
            c.DrawEllipseEx(centerX, centerY, (float)ringRadius, circleColor.WithAlpha(ringAlpha), null, RIPPLE_WIDTH);

            // the latest click decides the dip of the circle
            pressScale = 1 - PRESS_DEPTH * Math.Sin(Math.PI * progress);
        }


        // 2. the see-through circle, and the dot on the exact point
        var circleRadius = _radius * pressScale;
        var dotRadius = _radius * DOT_RATIO;
        c.DrawEllipseEx(centerX, centerY, (float)circleRadius, null, circleColor.WithAlpha((int)(CIRCLE_ALPHA * _opacity)), 0);
        c.DrawEllipseEx(centerX, centerY, (float)dotRadius, null, dotColor.WithAlpha((int)(DOT_ALPHA * _opacity)), 0);
    }


    /// <summary>
    /// Runs the fade and the clicks until they settle.
    /// </summary>
    private void StartAnimation()
    {
        if (_isAnimating) return;

        // off screen there is nothing to see move, so it settles at once
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            _opacity = _isShown ? 1 : 0;
            _clicks.Clear();
            return;
        }

        _isAnimating = true;
        _lastFrameTime = TimeSpan.Zero;
        topLevel.RequestAnimationFrame(OnAnimationFrame);
    }


    private void OnAnimationFrame(TimeSpan time)
    {
        var elapsedMs = _lastFrameTime == TimeSpan.Zero ? 0 : (time - _lastFrameTime).TotalMilliseconds;
        _lastFrameTime = time;
        _frameTime = time;


        // 1. fade toward shown or hidden
        var targetOpacity = _isShown ? 1d : 0d;
        var fadeStep = elapsedMs / FADE_MS;
        _opacity = targetOpacity > _opacity
            ? Math.Min(targetOpacity, _opacity + fadeStep)
            : Math.Max(targetOpacity, _opacity - fadeStep);


        // 2. new clicks start on this frame, and finished ones go
        for (var i = 0; i < _clicks.Count; i++)
        {
            _clicks[i] ??= time;
        }
        _ = _clicks.RemoveAll(start => (time - start!.Value).TotalMilliseconds >= CLICK_MS);

        InvalidateVisual();


        // 3. on to the next frame until both settle
        var isFading = _opacity != targetOpacity;
        var hasClicks = _clicks.Count > 0;
        var isSettled = !isFading && !hasClicks;
        if (isSettled)
        {
            _isAnimating = false;
            return;
        }

        // taken off screen while playing, so the next show settles it
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            _isAnimating = false;
            return;
        }

        topLevel.RequestAnimationFrame(OnAnimationFrame);
    }


    /// <summary>
    /// Gets the width and height the pointer takes: room for the widest ring of a click.
    /// </summary>
    private double GetExtent() => (_radius * RIPPLE_SCALE + RIPPLE_WIDTH) * 2;

    #endregion // Render & Animation

}
