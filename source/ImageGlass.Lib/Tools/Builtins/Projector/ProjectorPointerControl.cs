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
using ImageGlass.Common.Extensions;
using ImageGlass.UI;
using System;
using System.Collections.Generic;

namespace ImageGlass.Tools;


/// <summary>
/// Marks where the cursor is on the main viewer, on a projector: a glow like a torch beam with a dot, which ripples on a click.
/// </summary>
public sealed class ProjectorPointerControl : PhControl
{
    // the same size on every projector and through a click, so the audience always knows it
    private const double RADIUS = 30;
    private const double DOT_RATIO = 0.15;

    // the glow is brightest in the middle and fades toward its rim, where it still shows so it keeps its round shape
    private const double GLOW_CENTER_OPACITY = 0.6;
    private const double GLOW_EDGE_OPACITY = 0.2;

    // a pale core warming to yellow, like a torch beam, and a red dot, alike in every theme as few photos blend with them
    private static readonly Color GLOW_CORE_COLOR = Color.FromRgb(0xFF, 0xF5, 0xC2);
    private static readonly Color GLOW_COLOR = Color.FromRgb(0xFF, 0xCC, 0x00);
    private static readonly Color DOT_COLOR = Color.FromRgb(0xFF, 0x3B, 0x30);

    // a click sends a ring out to this many times the circle, which the extent of the control makes room for
    private const double RIPPLE_SCALE = 2.4;
    private const float RIPPLE_WIDTH = 3f;
    private const double EXTENT = (RADIUS * RIPPLE_SCALE + RIPPLE_WIDTH) * 2;

    private const double CLICK_MS = 450;
    private const double FADE_MS = 150;

    // moved without a layout pass, so following the cursor costs no more than a redraw of its spot
    private readonly TranslateTransform _offset = new();

    // the start of each click playing; null until its first frame
    private readonly List<TimeSpan?> _clicks = [];

    private double _opacity;
    private bool _isShown;
    private bool _isAnimating;
    private TimeSpan _lastFrameTime;
    private TimeSpan _frameTime;


    public ProjectorPointerControl()
    {
        IsHitTestVisible = false;
        RenderTransform = _offset;
        Width = EXTENT;
        Height = EXTENT;
    }



    #region Public Methods

    /// <summary>
    /// Moves the pointer to <paramref name="center"/>, showing it.
    /// </summary>
    public void MoveTo(Point center)
    {
        // its middle on the point
        _offset.X = center.X - EXTENT / 2;
        _offset.Y = center.Y - EXTENT / 2;

        // fading in where it was hidden
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
    /// Plays a click: a ring that spreads out from the circle.
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

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);


        // 1. each click sends a ring out, slowing as it fades
        foreach (var start in _clicks)
        {
            if (start is not { } startTime) continue;

            var progress = Math.Clamp((_frameTime - startTime).TotalMilliseconds / CLICK_MS, 0, 1);
            var eased = 1 - Math.Pow(1 - progress, 3);
            var ringRadius = RADIUS * (1 + (RIPPLE_SCALE - 1) * eased);
            var ringAlpha = (int)(DOT_COLOR.A * (1 - progress) * _opacity);
            c.DrawEllipseEx(center.X, center.Y, (float)ringRadius, DOT_COLOR.WithAlpha(ringAlpha), null, RIPPLE_WIDTH);
        }


        // 2. the glow, and the dot on the exact point
        c.DrawEllipse(CreateGlowBrush(_opacity), null, center, RADIUS, RADIUS);

        var dotRadius = RADIUS * DOT_RATIO;
        c.DrawEllipseEx(center.X, center.Y, (float)dotRadius, null, DOT_COLOR.WithAlpha((int)(DOT_COLOR.A * _opacity)), 0);
    }


    /// <summary>
    /// Creates the glow at <paramref name="opacity"/>: brightest in the middle, fading toward the rim.
    /// </summary>
    private static RadialGradientBrush CreateGlowBrush(double opacity)
    {
        var centerAlpha = (int)(255 * GLOW_CENTER_OPACITY * opacity);
        var edgeAlpha = (int)(255 * GLOW_EDGE_OPACITY * opacity);

        return new RadialGradientBrush
        {
            GradientStops =
            {
                new GradientStop(GLOW_CORE_COLOR.WithAlpha(centerAlpha), 0),
                new GradientStop(GLOW_COLOR.WithAlpha(edgeAlpha), 1),
            },
        };
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

    #endregion // Render & Animation

}
