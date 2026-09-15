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
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI;

/// <summary>
/// The Pro star: a control that draws it once at rest, plus the shared glyph other stars draw with.
/// </summary>
public sealed class PhProStar : PhControl
{
    /// <summary>
    /// The <c>IconProStar</c> viewBox width; hardcoded, as the platform path bounds may differ.
    /// </summary>
    internal const double VIEWBOX_WIDTH = 94.0;

    /// <summary>
    /// The <c>IconProStar</c> viewBox height.
    /// </summary>
    internal const double VIEWBOX_HEIGHT = 90.0;

    // the SVG radial is isotropic, so its gradientTransform reduces to this center + radius
    private const double GRADIENT_CENTER_X = 61.2009;
    private const double GRADIENT_CENTER_Y = 29.1895;
    private const double GRADIENT_RADIUS = 72.6563;

    private const string DARK_CORE_COLOR = "#FFFFFFFF";
    private const string DARK_EDGE_COLOR = "#FFA7D5F7";

    // the badge sits on the blue logo, so it stays white in both themes
    private const string BADGE_EDGE_COLOR = "#FFE4F2FD";
    private const string LIGHT_CORE_FALLBACK = "#FF1C5E96";
    private const string LIGHT_EDGE_FALLBACK = "#FF004F90";

    private Geometry? _geometry;
    private IBrush? _brush;


    public PhProStar()
    {
        IsHitTestVisible = false;
    }


    #region Overrides

    public override void Render(DrawingContext c)
    {
        base.Render(c);

        // purely cosmetic, so a failure here must not reach the hosting window
        try
        {
            _geometry ??= TryGetGeometry();
            _brush ??= CreateWhiteBrush();
            if (_geometry is null || _brush is null) return;

            var size = Math.Min(Bounds.Width, Bounds.Height);
            if (size <= 0) return;

            Draw(c, _geometry, _brush, Bounds.Width / 2, Bounds.Height / 2, size, 0);
        }
        catch { }
    }

    #endregion // Overrides



    #region Shared glyph

    /// <summary>
    /// Resolves the shared star geometry; callers must degrade to drawing nothing on null.
    /// </summary>
    internal static Geometry? TryGetGeometry()
    {
        try
        {
            return Resx.GetIcon(ResxIconId.IconProStar);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Builds the star gradient for the current theme, with fully opaque stops.
    /// </summary>
    internal static IBrush CreateThemedBrush()
    {
        var core = Color.Parse(DARK_CORE_COLOR);
        var edge = Color.Parse(DARK_EDGE_COLOR);

        // a white core is invisible on a light background, so light mode uses the accent pair
        if (!Core.Theme.Settings.IsDarkMode)
        {
            try
            {
                core = Resx.Get<Color>(ResxId.IG_TextAccentColor);
                edge = Resx.Get<Color>(ResxId.SystemAccentColorDark3);
            }
            catch
            {
                core = Color.Parse(LIGHT_CORE_FALLBACK);
                edge = Color.Parse(LIGHT_EDGE_FALLBACK);
            }
        }

        return BuildBrush(core, edge);
    }


    /// <summary>
    /// Builds the authored white gradient, for a star sitting on the blue logo in either theme.
    /// </summary>
    internal static IBrush CreateWhiteBrush()
        => BuildBrush(Color.Parse(DARK_CORE_COLOR), Color.Parse(BADGE_EDGE_COLOR));


    /// <summary>
    /// Draws one star centered on the given point, scaled so its viewBox edge measures <paramref name="size"/>.
    /// </summary>
    internal static void Draw(DrawingContext c, Geometry geometry, IBrush brush,
        double centerX, double centerY, double size, double rotationRadians)
    {
        var scale = size / VIEWBOX_WIDTH;
        var pivot = new Point(VIEWBOX_WIDTH / 2, VIEWBOX_HEIGHT / 2);

        // Avalonia's Matrix is row-vector, so A * B applies A then B
        var matrix = Matrix.CreateRotation(rotationRadians, pivot)
            * Matrix.CreateTranslation(-pivot.X, -pivot.Y)
            * Matrix.CreateScale(scale, scale)
            * Matrix.CreateTranslation(centerX, centerY);

        using var _ = c.PushTransform(matrix);
        c.DrawGeometry(brush, null, geometry);
    }


    private static IBrush BuildBrush(Color core, Color edge)
    {
        try
        {
            // absolute units: the star is always drawn in viewBox space, so one brush fits all sizes
            var brush = new RadialGradientBrush
            {
                Center = new RelativePoint(GRADIENT_CENTER_X, GRADIENT_CENTER_Y, RelativeUnit.Absolute),
                GradientOrigin = new RelativePoint(GRADIENT_CENTER_X, GRADIENT_CENTER_Y, RelativeUnit.Absolute),
                RadiusX = new RelativeScalar(GRADIENT_RADIUS, RelativeUnit.Absolute),
                RadiusY = new RelativeScalar(GRADIENT_RADIUS, RelativeUnit.Absolute),
                GradientStops =
                {
                    new GradientStop(core, 0),
                    new GradientStop(edge, 0.5533),
                },
            };

            // immutable, or every DrawGeometry re-snapshots the brush into render data
            return brush.ToImmutable();
        }
        catch
        {
            return new SolidColorBrush(core);
        }
    }

    #endregion // Shared glyph

}
