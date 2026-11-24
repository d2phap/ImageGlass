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
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;

namespace ImageGlass.UI.Viewer;

/// <summary>
/// One navigation button: a disc over a blurred backdrop, which <see cref="NavButtonsOverlay"/> slides, fades and presses.
/// </summary>
internal sealed class NavButton : PhControl
{
    private const double PRESSED_SCALE = 0.95;

    private readonly MatrixTransform _transform = new();
    private IImage? _icon;
    private bool _isPressed;


    public NavButton()
    {
        Width = NavButtonsInfo.NAV_BTN_SIZE.Width;
        Height = NavButtonsInfo.NAV_BTN_SIZE.Height;
        VerticalAlignment = VerticalAlignment.Center;

        // the overlay owns all input, so its hit areas stay as they were
        IsHitTestVisible = false;
        IsVisible = false;
        RenderTransform = _transform;

        // the clip rounds the blur, which would otherwise fill the square bounds
        Clip = new EllipseGeometry(new Rect(NavButtonsInfo.NAV_BTN_SIZE));
        BackdropEffect = new ImmutableBlurEffect(Const.BACKDROP_BLUR_RADIUS);
    }


    /// <summary>
    /// Shows the button at <paramref name="progress"/> (0 = hidden, 1 = shown), slid sideways by <paramref name="slideOffset"/>.
    /// </summary>
    public void Apply(double progress, double slideOffset, bool isPressed, IImage? icon)
    {
        IsVisible = progress > 0;
        Opacity = progress;

        var scale = isPressed ? PRESSED_SCALE : 1;
        _transform.Matrix = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(slideOffset, 0);

        var isLookChanged = _isPressed != isPressed || !ReferenceEquals(_icon, icon);
        if (!isLookChanged) return;

        _isPressed = isPressed;
        _icon = icon;
        InvalidateVisual();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var rect = new Rect(Bounds.Size);
        var strokeWidth = _isPressed ? 1.5f : 1f;
        var strokeColor = Core.AccentColor.WithAlpha(_isPressed ? 180 : 120);

        // a base layer for contrast, then the accent disc, its ring inset to stay inside the clip
        c.DrawEllipseEx(rect, null, Core.Theme.BaseColor.WithAlpha(120));
        c.DrawEllipseEx(rect.Deflate(strokeWidth / 2), strokeColor, Core.AccentColor.WithAlpha(100), strokeWidth);

        // the icon at half the button size, centered
        if (_icon is null) return;

        var iconSize = new Size(rect.Width * 0.5, rect.Height * 0.5);
        var iconRect = new Rect(
            (rect.Width - iconSize.Width) / 2,
            (rect.Height - iconSize.Height) / 2,
            iconSize.Width,
            iconSize.Height);

        c.DrawImage(_icon, iconRect);
    }
}
