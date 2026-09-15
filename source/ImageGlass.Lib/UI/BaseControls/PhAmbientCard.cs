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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using System;

namespace ImageGlass.UI;

/// <summary>
/// A card washed in soft accent light, with a highlight slowly travelling around its border.
/// </summary>
public class PhAmbientCard : PhControl
{
    // the flat tint under everything, plus the bloom that trails the travelling light
    private const double WASH_ALPHA = 0.055;
    private const int BLOOM_ALPHA = 40;
    private const double BLOOM_RADIUS = 0.85;

    // the border at rest, and the two strokes that carry the light over it
    private const int RESTING_BORDER_ALPHA = 64;
    private const double GLOW_OPACITY = 0.5;
    private const double GLOW_WIDTH_SCALE = 6.0;
    private const double LIGHT_ARC = 0.05;
    private const double GLOW_ARC = 0.2;
    private const int BAND_FALLOFF_ALPHA = 110;
    private const double BAND_FALLOFF_OFFSET = 0.4;

    // the light is only readable against the card once it is pushed away from the accent itself
    private const float LIGHT_BRIGHTNESS_DARK = 0.55f;
    private const float LIGHT_BRIGHTNESS_LIGHT = -0.2f;

    private TimeSpan? _startTimestamp;
    private double _angleDeg;
    private bool _animRunning;


    public PhAmbientCard()
    {
        // the card paints its own surface, so the presenter must not draw a second one over it
        Background = null;
        BorderBrush = null;
    }


    #region Public Properties

    /// <summary>
    /// Gets, sets the color the wash and the light are built from. Defaults to the app accent color.
    /// </summary>
    public Color? AmbientColor
    {
        get => GetValue(AmbientColorProperty);
        set => SetValue(AmbientColorProperty, value);
    }
    public static readonly StyledProperty<Color?> AmbientColorProperty =
        AvaloniaProperty.Register<PhAmbientCard, Color?>(nameof(AmbientColor));


    /// <summary>
    /// Gets, sets whether the border light travels. When off, it rests at the top of the card.
    /// </summary>
    public bool EnableLight
    {
        get => GetValue(EnableLightProperty);
        set => SetValue(EnableLightProperty, value);
    }
    public static readonly StyledProperty<bool> EnableLightProperty =
        AvaloniaProperty.Register<PhAmbientCard, bool>(nameof(EnableLight), true);


    /// <summary>
    /// Gets, sets how many seconds the light takes to travel once around the border.
    /// </summary>
    public double LightCycleSeconds
    {
        get => GetValue(LightCycleSecondsProperty);
        set => SetValue(LightCycleSecondsProperty, value);
    }
    public static readonly StyledProperty<double> LightCycleSecondsProperty =
        AvaloniaProperty.Register<PhAmbientCard, double>(nameof(LightCycleSeconds), 9.0);


    /// <summary>
    /// Gets, sets the stroke width of the border and of the light riding it.
    /// </summary>
    public double LightThickness
    {
        get => GetValue(LightThicknessProperty);
        set => SetValue(LightThicknessProperty, value);
    }
    public static readonly StyledProperty<double> LightThicknessProperty =
        AvaloniaProperty.Register<PhAmbientCard, double>(nameof(LightThickness), 0.5);

    #endregion // Public Properties



    #region Overrides

    public override void Render(DrawingContext c)
    {
        base.Render(c);

        // purely cosmetic, so a failure here must not reach the hosting window
        try
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

            var accent = AmbientColor ?? Core.AccentColor;
            var lightColor = accent.WithBrightness(Core.Theme.Settings.IsDarkMode
                ? LIGHT_BRIGHTNESS_DARK
                : LIGHT_BRIGHTNESS_LIGHT);

            var lightWidth = Math.Max(0.5, LightThickness);
            var cardRect = new RoundedRect(new Rect(Bounds.Size), CornerRadius);

            // the wash sits under the content, the bloom marks where the light currently is
            c.DrawRectangle(new ImmutableSolidColorBrush(accent, WASH_ALPHA), null, cardRect);
            c.DrawRectangle(CreateBloomBrush(accent, _angleDeg), null, cardRect);

            // the strokes ride the card edge, so clip their outer half instead of letting it spill
            using var _ = c.PushClip(cardRect);

            var strokeRect = cardRect.Deflate(lightWidth / 2, lightWidth / 2);
            var glowPen = new ImmutablePen(
                CreateSweepBrush(lightColor, GLOW_ARC, GLOW_OPACITY, _angleDeg),
                lightWidth * GLOW_WIDTH_SCALE);
            var lightPen = new ImmutablePen(
                CreateSweepBrush(lightColor, LIGHT_ARC, 1.0, _angleDeg), lightWidth);

            c.DrawRectangle(null, glowPen, strokeRect);
            c.DrawRectangle(null, lightPen, strokeRect);
        }
        catch { }
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        StartLight();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // the pending frame self-terminates on the flag; there is no handle to cancel
        _animRunning = false;
        _startTimestamp = null;
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == EnableLightProperty)
        {
            if (EnableLight) StartLight();
            else
            {
                _animRunning = false;
                _angleDeg = 0;
                InvalidateVisual();
            }
        }
        else if (e.Property == AmbientColorProperty || e.Property == LightThicknessProperty)
        {
            InvalidateVisual();
        }
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }

    #endregion // Overrides



    #region Private Methods

    /// <summary>
    /// Starts the travel loop, unless it is already running or turned off.
    /// </summary>
    private void StartLight()
    {
        try
        {
            if (_animRunning || !EnableLight) return;

            // a detached view gets no frame, so the running flag would latch forever
            var topLevel = TopLevel.GetTopLevel(this);
            if (!IsLoaded || topLevel is null) return;

            _startTimestamp = null;
            _animRunning = true;
            topLevel.RequestAnimationFrame(OnAnimationFrame);
        }
        catch { }
    }


    /// <summary>
    /// Advances the light off an absolute start timestamp, so a stalled compositor cannot skip it.
    /// </summary>
    private void OnAnimationFrame(TimeSpan timestamp)
    {
        try
        {
            if (!_animRunning) return;

            _startTimestamp ??= timestamp;

            var cycle = Math.Max(0.5, LightCycleSeconds);
            var elapsed = (timestamp - _startTimestamp.Value).TotalSeconds;
            _angleDeg = elapsed / cycle % 1.0 * 360.0;

            // a collapsed card still holds its frame loop, but must not cost a repaint
            if (IsEffectivelyVisible) InvalidateVisual();

            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
        }
        catch
        {
            _animRunning = false;
        }
    }


    /// <summary>
    /// Builds the swept band carrying the light, biased so its peak lands at <paramref name="angleDeg"/>.
    /// </summary>
    private static ImmutableConicGradientBrush CreateSweepBrush(Color color, double arc,
        double opacity, double angleDeg)
    {
        var halfArc = Math.Clamp(arc, 0.005, 0.49);
        var falloffArc = halfArc * BAND_FALLOFF_OFFSET;
        var fadeColor = color.WithAlpha(0);
        var falloffColor = color.WithAlpha(BAND_FALLOFF_ALPHA);

        // the band is centered on offset 0.5 so its two shoulders cannot wrap past the sweep seam
        ImmutableGradientStop[] stops =
        [
            new(0, fadeColor),
            new(0.5 - halfArc, fadeColor),
            new(0.5 - falloffArc, falloffColor),
            new(0.5, color),
            new(0.5 + falloffArc, falloffColor),
            new(0.5 + halfArc, fadeColor),
            new(1, fadeColor),
        ];

        return new ImmutableConicGradientBrush(stops, opacity, angle: angleDeg + 180.0);
    }


    /// <summary>
    /// Builds the interior bloom, centered where the light currently sits on the border.
    /// </summary>
    private static ImmutableRadialGradientBrush CreateBloomBrush(Color color, double angleDeg)
    {
        // the sweep starts above the center and turns clockwise, matching the conic band
        var radian = angleDeg * Math.PI / 180.0;
        var center = new RelativePoint(
            0.5 + 0.5 * Math.Sin(radian),
            0.5 - 0.5 * Math.Cos(radian),
            RelativeUnit.Relative);

        ImmutableGradientStop[] stops =
        [
            new(0, color.WithAlpha(BLOOM_ALPHA)),
            new(1, color.WithAlpha(0)),
        ];

        return new ImmutableRadialGradientBrush(stops,
            center: center, gradientOrigin: center, radius: BLOOM_RADIUS);
    }

    #endregion // Private Methods

}
