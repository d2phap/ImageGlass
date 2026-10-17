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
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI;


/// <summary>
/// The side a <see cref="PhOverlay"/> slides in from when it shows.
/// </summary>
public enum PhOverlayDirection
{
    /// <summary>
    /// Fades in where it is, without sliding.
    /// </summary>
    Auto,
    Top,
    Bottom,
    Left,
    Right,
}


/// <summary>
/// A floating surface in the in-app message style, which animates its content in and out.
/// </summary>
public class PhOverlay : PhControl
{
    // the in-app message surface, lifted off what is under it by a hairline edge and a soft shadow
    private const int BORDER_ALPHA = 20;
    private const double SHADOW_OFFSET_Y = 4;
    private const double SHADOW_BLUR = 24;
    private const int SHADOW_ALPHA_DARK = 56;
    private const int SHADOW_ALPHA_LIGHT = 28;

    // it fades in growing out of the side it enters from, quicker on the way out
    private const double SHOW_MS = 200;
    private const double HIDE_MS = 150;
    private const double SLIDE_DISTANCE = 12;
    private const double START_SCALE = 0.95;

    private readonly MatrixTransform _transform = new();
    private IImmutableBrush? _fillBrush;
    private ImmutablePen? _borderPen;
    private BoxShadows _shadow;

    // 0 = hidden, 1 = shown
    private double _progress;
    private bool _isAnimating;
    private TimeSpan _lastFrameTime;



    #region Public Properties

    /// <summary>
    /// Gets, sets whether the overlay shows; it animates in and out, and leaves the layout once hidden.
    /// </summary>
    public bool IsShown
    {
        get => GetValue(IsShownProperty);
        set => SetValue(IsShownProperty, value);
    }
    public static readonly StyledProperty<bool> IsShownProperty =
        AvaloniaProperty.Register<PhOverlay, bool>(nameof(IsShown));


    /// <summary>
    /// Gets, sets the side the overlay slides in from; <see cref="PhOverlayDirection.Auto"/> fades it in place.
    /// </summary>
    public PhOverlayDirection TransitionDirection
    {
        get => GetValue(TransitionDirectionProperty);
        set => SetValue(TransitionDirectionProperty, value);
    }
    public static readonly StyledProperty<PhOverlayDirection> TransitionDirectionProperty =
        AvaloniaProperty.Register<PhOverlay, PhOverlayDirection>(nameof(TransitionDirection), PhOverlayDirection.Auto);


    /// <summary>
    /// Gets whether the overlay can be seen: shown, or still fading out.
    /// </summary>
    public bool IsOnScreen
    {
        get
        {
            if (IsShown) return true;

            var isFading = _progress > 0;
            return isFading;
        }
    }

    #endregion // Public Properties



    static PhOverlay()
    {
        // hidden until shown, so it takes no room and no input meanwhile
        IsVisibleProperty.OverrideDefaultValue<PhOverlay>(false);
        AffectsRender<PhOverlay>(CornerRadiusProperty);

        // a templated control clips to its bounds by default, which would cut the shadow off
        ClipToBoundsProperty.OverrideDefaultValue<PhOverlay>(false);
    }


    public PhOverlay()
    {
        RenderTransform = _transform;
        Opacity = 0;
        UpdateTransformOrigin();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateBrushes();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        UpdateBrushes();
        InvalidateVisual();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsShownProperty)
        {
            OnIsShownChanged();
        }
        else if (change.Property == TransitionDirectionProperty)
        {
            UpdateTransformOrigin();
            ApplyProgress();
        }
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var size = Bounds.Size;
        if (size.Width <= 0) return;
        if (size.Height <= 0) return;

        // the surface, under the content
        var surface = new RoundedRect(new Rect(size), GetCornerRadius());
        c.DrawRectangle(_fillBrush, _borderPen, surface, _shadow);
    }

    #endregion // Control Events



    #region Private Methods

    /// <summary>
    /// Builds the surface from the in-app message background of the current theme.
    /// </summary>
    private void UpdateBrushes()
    {
        var fillColor = Resx.GetBrushColor(ResxId.IG_OverlayBackgroundBrush, AppThemeColors.BgBrush.Color);
        var borderColor = Core.Theme.InvertedBaseColor.WithAlpha(BORDER_ALPHA);
        var shadowAlpha = Core.Theme.Settings.IsDarkMode ? SHADOW_ALPHA_DARK : SHADOW_ALPHA_LIGHT;

        _fillBrush = new ImmutableSolidColorBrush(fillColor);
        _borderPen = new ImmutablePen(new ImmutableSolidColorBrush(borderColor));
        _shadow = new BoxShadows(new BoxShadow
        {
            OffsetY = SHADOW_OFFSET_Y,
            Blur = SHADOW_BLUR,
            Color = Colors.Black.WithAlpha(shadowAlpha),
        });
    }


    /// <summary>
    /// Gets the corner radius set on the overlay, else the shared overlay one.
    /// </summary>
    private CornerRadius GetCornerRadius()
    {
        var isCustom = IsSet(CornerRadiusProperty);
        if (isCustom) return CornerRadius;

        var token = Resx.Get<object?>(ResxId.OverlayCornerRadius);
        if (token is CornerRadius radius) return radius;

        return CornerRadius;
    }


    /// <summary>
    /// Makes the overlay grow out of the side it enters from, or out of its middle when it fades in place.
    /// </summary>
    private void UpdateTransformOrigin()
    {
        RenderTransformOrigin = TransitionDirection switch
        {
            PhOverlayDirection.Top => new RelativePoint(0.5, 0, RelativeUnit.Relative),
            PhOverlayDirection.Bottom => new RelativePoint(0.5, 1, RelativeUnit.Relative),
            PhOverlayDirection.Left => new RelativePoint(0, 0.5, RelativeUnit.Relative),
            PhOverlayDirection.Right => new RelativePoint(1, 0.5, RelativeUnit.Relative),
            _ => new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
        };
    }


    private void OnIsShownChanged()
    {
        // it joins the layout at the start of the fade, so its first frame is still clear
        if (IsShown)
        {
            ApplyProgress();
            SetCurrentValue(IsVisibleProperty, true);
        }

        StartAnimation();
    }


    /// <summary>
    /// Runs the fade until it settles.
    /// </summary>
    private void StartAnimation()
    {
        if (_isAnimating) return;

        // with no window there is no frame to wait for, so it settles at once
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            _progress = IsShown ? 1 : 0;
            ApplyProgress();
            return;
        }

        _isAnimating = true;
        _lastFrameTime = TimeSpan.Zero;
        topLevel.RequestAnimationFrame(OnAnimationFrame);
    }


    private void OnAnimationFrame(TimeSpan time)
    {
        // taken off screen while playing, so it settles at once
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            _isAnimating = false;
            _progress = IsShown ? 1 : 0;
            ApplyProgress();
            return;
        }

        var isFirstFrame = _lastFrameTime == TimeSpan.Zero;
        var elapsedMs = isFirstFrame ? 0 : (time - _lastFrameTime).TotalMilliseconds;
        _lastFrameTime = time;

        var isShown = IsShown;
        var step = elapsedMs / (isShown ? SHOW_MS : HIDE_MS);
        _progress = isShown
            ? Math.Min(1, _progress + step)
            : Math.Max(0, _progress - step);

        ApplyProgress();

        // on to the next frame until it settles
        var targetProgress = isShown ? 1 : 0;
        var isSettled = _progress == targetProgress;
        if (isSettled)
        {
            _isAnimating = false;
            return;
        }

        topLevel.RequestAnimationFrame(OnAnimationFrame);
    }


    /// <summary>
    /// Fades, scales and slides the overlay to the current progress, and takes it out of the layout once hidden.
    /// </summary>
    private void ApplyProgress()
    {
        var eased = EaseOutCubic(_progress);
        var scale = START_SCALE + (1 - START_SCALE) * eased;
        var slide = SLIDE_DISTANCE * (1 - eased);
        var offset = TransitionDirection switch
        {
            PhOverlayDirection.Top => new Vector(0, -slide),
            PhOverlayDirection.Bottom => new Vector(0, slide),
            PhOverlayDirection.Left => new Vector(-slide, 0),
            PhOverlayDirection.Right => new Vector(slide, 0),
            _ => default,
        };

        Opacity = eased;
        _transform.Matrix = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset);

        // a showing overlay starts at 0 too, so only one on its way out leaves
        if (IsShown) return;

        var isFadedOut = _progress <= 0;
        if (isFadedOut) SetCurrentValue(IsVisibleProperty, false);
    }


    /// <summary>
    /// Fast at first, gentle at the end.
    /// </summary>
    private static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);

    #endregion // Private Methods

}
