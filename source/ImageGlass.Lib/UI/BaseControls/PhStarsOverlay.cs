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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ImageGlass.Common;
using System;

namespace ImageGlass.UI;

/// <summary>
/// A burst of star glyphs rising at 45deg behind a hero section, frozen on its last frame.
/// </summary>
public sealed class PhStarsOverlay : PhControl
{
    /// <summary>
    /// How long one burst lasts, in seconds.
    /// </summary>
    internal const double BURST_SECONDS = 3.0;

    private const double SQRT_HALF = 0.70710678118654752;

    // a hero is short, so at 45deg each star is a short diagonal streak
    private const int MIN_STARS = 20;
    private const int MAX_STARS = 30;
    private const double MIN_STAR_SIZE = 5.0;
    private const double MAX_STAR_SIZE = 18.0;
    private const double SIZE_BIAS = 1.7;
    private const double MIN_SPEED = 90.0;
    private const double MAX_SPEED = 200.0;
    private const double MIN_SPIN_DEG = 15.0;
    private const double MAX_SPIN_DEG = 55.0;
    private const double MAX_OPACITY = 0.80;
    private const double OPACITY_SIZE_FALLOFF = 0.40;
    private const double SPAWN_LEFT_MARGIN = 0.20;
    private const double SPAWN_RIGHT_LIMIT = 0.85;
    private const double SPAWN_TOP_LIMIT = 0.72;
    private const double STAR_FADE_IN = 0.20;
    private const double BURST_FADE_IN = 0.30;
    private const double LAST_SPAWN_DELAY = 2.3;
    private const double SEEDED_MIN_DELAY = -1.0;
    private const double SEEDED_MAX_DELAY = -0.15;
    private const int SEEDED_STARS = 5;

    // reserved late-and-slow stars, so the frozen frame is never an empty hero
    private const int RESERVED_TAIL = 6;
    private const double RESERVED_FIRST_DELAY = 1.75;
    private const double RESERVED_DELAY_STEP = 0.10;
    private const double RESERVED_MIN_SPEED = 90.0;
    private const double RESERVED_MAX_SPEED = 140.0;

    private const double BOB_AMPLITUDE = 4.0;
    private const double BOB_PERIOD = 1.5;

    private readonly TranslateTransform _bob = new();

    private Geometry? _geometry;
    private IBrush? _brush;
    private StarParticle[] _particles = [];
    private TimeSpan? _startTimestamp;
    private double _elapsed;
    private bool _animRunning;


    public PhStarsOverlay()
    {
        // the hero owns the replay click, and stars must not spill past it
        IsHitTestVisible = false;
        ClipToBounds = true;
    }


    #region Public Properties

    /// <summary>
    /// Gets, sets the element that floats along with the burst, driven off the same clock.
    /// </summary>
    public Control? BobTarget { get; set; }


    /// <summary>
    /// Gets, sets whether the stars fly; when off, <see cref="Play"/> only floats <see cref="BobTarget"/>.
    /// </summary>
    public bool EnableStars { get; set; } = true;

    #endregion // Public Properties



    #region Public Methods

    /// <summary>
    /// Restarts the burst from zero. Safe to call at any time, including mid-burst.
    /// </summary>
    public void Play()
    {
        try
        {
            // a detached view gets no frame, so the running flag would latch forever
            if (!IsLoaded || TopLevel.GetTopLevel(this) is null) return;

            // the very first Play can land before the hero has been arranged
            if (Bounds.Width <= 0 || Bounds.Height <= 0)
            {
                Dispatcher.UIThread.Post(Play, DispatcherPriority.Render);
                return;
            }

            // a missing icon resource costs the stars, never the float
            _particles = [];
            if (EnableStars)
            {
                EnsureResources();
                if (_geometry is not null) _particles = GenerateParticles(Bounds.Width, Bounds.Height);
            }

            _elapsed = 0;
            _startTimestamp = null;
            _bob.Y = 0;

            // never start a second loop, or the timeline advances at double speed
            if (!_animRunning)
            {
                _animRunning = true;
                TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
            }

            InvalidateVisual();
        }
        catch { }
    }

    #endregion // Public Methods



    #region Overrides

    public override void Render(DrawingContext c)
    {
        base.Render(c);

        try
        {
            if (_particles.Length == 0) return;

            EnsureResources();
            if (_geometry is null || _brush is null) return;

            // masks the seeded stars popping in at t=0
            var burstFade = Math.Min(1.0, _elapsed / BURST_FADE_IN);

            foreach (var p in _particles)
            {
                var age = _elapsed - p.Delay;
                if (age <= 0) continue;

                var opacity = p.PeakOpacity * burstFade * Math.Min(1.0, age / STAR_FADE_IN);
                if (opacity <= 0.004) continue;

                using var _ = c.PushOpacity(opacity);
                PhProStar.Draw(c, _geometry, _brush,
                    p.StartX + p.VelX * age,
                    p.StartY + p.VelY * age,
                    p.Size,
                    p.Rotation + p.SpinRate * age);
            }
        }
        catch { }
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // the pending frame self-terminates on the flag; there is no handle to cancel
        _animRunning = false;
        _bob.Y = 0;
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        _brush = null;
        _geometry = null;
        InvalidateVisual();
    }

    #endregion // Overrides



    #region Private Methods

    private void EnsureResources()
    {
        _geometry ??= PhProStar.TryGetGeometry();
        _brush ??= PhProStar.CreateThemedBrush();
    }


    /// <summary>
    /// Advances the burst off an absolute start timestamp, so a stalled compositor cannot skip it.
    /// </summary>
    private void OnAnimationFrame(TimeSpan timestamp)
    {
        try
        {
            if (!_animRunning) return;

            _startTimestamp ??= timestamp;
            _elapsed = Math.Min(BURST_SECONDS, (timestamp - _startTimestamp.Value).TotalSeconds);

            UpdateBob(_elapsed);
            InvalidateVisual();

            if (_elapsed >= BURST_SECONDS)
            {
                // no frame lands on exactly BURST_SECONDS, so rest has to be written here
                _bob.Y = 0;
                _animRunning = false;
                InvalidateVisual();
                return;
            }

            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
        }
        catch
        {
            _animRunning = false;
        }
    }


    /// <summary>
    /// Floats <see cref="BobTarget"/> under an envelope that is zero at both ends of the burst.
    /// </summary>
    private void UpdateBob(double elapsed)
    {
        if (BobTarget is null) return;
        BobTarget.RenderTransform ??= _bob;

        var progress = elapsed / BURST_SECONDS;
        _bob.Y = -BOB_AMPLITUDE
            * Math.Sin(Math.PI * progress)
            * Math.Sin(2 * Math.PI * elapsed / BOB_PERIOD);
    }


    private static StarParticle[] GenerateParticles(double width, double height)
    {
        var rnd = Random.Shared;
        var count = rnd.Next(MIN_STARS, MAX_STARS + 1);
        var items = new StarParticle[count];
        var reservedFrom = Math.Max(count - RESERVED_TAIL, 0);

        for (var i = 0; i < count; i++)
        {
            var sizeNorm = Math.Pow(rnd.NextDouble(), SIZE_BIAS);
            var starSize = MIN_STAR_SIZE + sizeNorm * (MAX_STAR_SIZE - MIN_STAR_SIZE);
            var isReserved = i >= reservedFrom;

            var speed = isReserved
                ? Lerp(RESERVED_MIN_SPEED, RESERVED_MAX_SPEED, rnd.NextDouble())
                : Lerp(MIN_SPEED, MAX_SPEED, rnd.NextDouble());

            double delay;
            if (isReserved) delay = RESERVED_FIRST_DELAY + (i - reservedFrom) * RESERVED_DELAY_STEP;
            else if (i < SEEDED_STARS) delay = Lerp(SEEDED_MIN_DELAY, SEEDED_MAX_DELAY, rnd.NextDouble());
            else delay = rnd.NextDouble() * LAST_SPAWN_DELAY;

            var spinDeg = Lerp(MIN_SPIN_DEG, MAX_SPIN_DEG, rnd.NextDouble());
            if (rnd.Next(2) == 0) spinDeg = -spinDeg;

            items[i] = new StarParticle(
                StartX: Lerp(-SPAWN_LEFT_MARGIN * width, SPAWN_RIGHT_LIMIT * width, rnd.NextDouble()),
                StartY: Lerp(SPAWN_TOP_LIMIT * height, height + starSize, rnd.NextDouble()),
                VelX: speed * SQRT_HALF,
                VelY: -speed * SQRT_HALF,
                Size: starSize,
                PeakOpacity: MAX_OPACITY - sizeNorm * OPACITY_SIZE_FALLOFF,
                Rotation: rnd.NextDouble() * Math.Tau,
                SpinRate: spinDeg * Math.PI / 180.0,
                Delay: delay);
        }

        return items;
    }


    private static double Lerp(double from, double to, double amount) => from + (to - from) * amount;

    #endregion // Private Methods



    /// <summary>
    /// One star of the burst. A negative <c>Delay</c> seeds it already in flight.
    /// </summary>
    private readonly record struct StarParticle(
        double StartX, double StartY,
        double VelX, double VelY,
        double Size,
        double PeakOpacity,
        double Rotation, double SpinRate,
        double Delay);
}
