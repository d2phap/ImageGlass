/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using System.Runtime.InteropServices;

namespace ImageGlass.UI;

/// <summary>
/// Represents an HSLA (hue, saturation, luminance, alpha) color.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct HslColor
{
    public static readonly HslColor Empty;
    private double _hue = 0;
    private double _saturation = 0;
    private double _luminance = 0;
    private int _alpha = 255;


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets, sets hue value.
    /// </summary>
    public double H
    {
        get => _hue;
        set => _hue = Math.Max(0, Math.Min(value, 1));
    }


    /// <summary>
    /// Gets, sets saturation value.
    /// </summary>
    public double S
    {
        get => _saturation;
        set => _saturation = Math.Max(0, Math.Min(value, 1));
    }


    /// <summary>
    /// Gets, sets luminance value.
    /// </summary>
    public double L
    {
        get => _luminance;
        set => _luminance = Math.Max(0, Math.Min(value, 1));
    }


    /// <summary>
    /// Gets, sets RGBA color.
    /// </summary>
    public Color RgbValue
    {
        get => HslToRgb();
        set => RgbtoHsl(value);
    }


    /// <summary>
    /// Gets, sets alpha value.
    /// </summary>
    public int A
    {
        get => _alpha;
        set => _alpha = Math.Max(0, Math.Min(value, 255));
    }


    /// <summary>
    /// Checks if <see cref="HslColor"/> is empty.
    /// </summary>
    public bool IsEmpty => _alpha == 0 && H == 0.0 && S == 0.0 && L == 0.0;

    #endregion // Public properties


    // Constructors
    #region Constructors
    public HslColor(int a, double h, double s, double l)
    {
        A = a;
        H = h;
        S = s;
        L = l;
    }


    public HslColor(double h, double s, double l)
    {
        _hue = h;
        _saturation = s;
        _luminance = l;
    }


    public HslColor(Color color)
    {
        _alpha = color.A;
        RgbtoHsl(color);
    }


    public static HslColor FromArgb(int a, int r, int g, int b)
    {
        return new HslColor(Color.FromArgb(a, r, g, b));
    }


    public static HslColor FromColor(Color color)
    {
        return new HslColor(color);
    }


    public static HslColor FromAhsl(int a)
    {
        return new HslColor(a, 0.0, 0.0, 0.0);
    }


    public static HslColor FromAhsl(int a, HslColor hsl)
    {
        return new HslColor(a, hsl._hue, hsl._saturation, hsl._luminance);
    }


    public static HslColor FromAhsl(double h, double s, double l)
    {
        return new HslColor(255, h, s, l);
    }


    public static HslColor FromAhsl(int a, double h, double s, double l)
    {
        return new HslColor(a, h, s, l);
    }

    #endregion // Constructors


    // Operators
    #region Operators
    public static bool operator ==(HslColor left, HslColor right)
    {
        return left.A == right.A
            && left.H == right.H
            && left.S == right.S
            && left.L == right.L;
    }

    public static bool operator !=(HslColor left, HslColor right)
    {
        return !(left == right);
    }

    #endregion // Operators


    // Override methods
    #region Override methods

    public override bool Equals(object? obj)
    {
        if (obj is not HslColor color) return false;

        return A == color.A
            && H == color.H
            && S == color.S
            && L == color.L;
    }


    public override string ToString()
    {
        return $"[H={H}, S={S}, L={L}, A={A}]";
    }


    public override readonly int GetHashCode()
    {
        return _alpha.GetHashCode()
            ^ _hue.GetHashCode()
            ^ _saturation.GetHashCode()
            ^ _luminance.GetHashCode();
    }

    #endregion // Override methods


    // Public methods
    #region Public methods

    /// <summary>
    /// Converts <see cref="HslColor"/> to <see cref="Color"/>.
    /// </summary>
    public Color ToRgba()
    {
        return ToRgba(A);
    }

    /// <summary>
    /// Converts <see cref="HslColor"/> to <see cref="Color"/>.
    /// </summary>
    public Color ToRgba(int alpha)
    {
        double q;
        if (L < 0.5) q = L * (1 + S);
        else q = L + S - (L * S);

        double p = 2 * L - q;
        double hk = H / 360;


        // r,g,b colors
        double[] tc = new[] { hk + (1d / 3d), hk, hk - (1d / 3d) };
        double[] colors = new[] { 0.0, 0.0, 0.0 };


        for (int color = 0; color < colors.Length; color++)
        {
            if (tc[color] < 0) tc[color] += 1;
            if (tc[color] > 1) tc[color] -= 1;

            if (tc[color] < (1d / 6d))
            {
                colors[color] = p + ((q - p) * 6 * tc[color]);
            }
            else if (tc[color] >= (1d / 6d) && tc[color] < (1d / 2d))
            {
                colors[color] = q;
            }
            else if (tc[color] >= (1d / 2d) && tc[color] < (2d / 3d))
            {
                colors[color] = p + ((q - p) * 6 * (2d / 3d - tc[color]));
            }
            else
            {
                colors[color] = p;
            }

            colors[color] *= 255;
        }

        var r = (int)Math.Max(0, Math.Min(colors[0], 255));
        var g = (int)Math.Max(0, Math.Min(colors[1], 255));
        var b = (int)Math.Max(0, Math.Min(colors[2], 255));

        return Color.FromArgb(alpha, r, g, b);
    }

    #endregion // Public methods


    // Private methods
    #region Private methods

    private Color HslToRgb()
    {
        var r = 0;
        var g = 0;
        var b = 0;

        int num1 = Round(_luminance * 255.0);
        int num2;
        int num3 = Round((1.0 - _saturation) * (_luminance / 1.0) * 255.0);
        double num4 = (num1 - num3) / 255.0;

        if (_hue >= 0.0 && _hue <= 0.16666666666666666)
        {
            num2 = Round((((_hue - 0.0) * num4) * 1530.0) + num3);

            r = num1;
            g = num2;
            b = num3;
        }

        else if (_hue <= 0.33333333333333331)
        {
            num2 = Round((-((_hue - 0.16666666666666666) * num4) * 1530.0) + num1);

            r = num2;
            g = num1;
            b = num3;
        }

        else if (_hue <= 0.5)
        {
            num2 = Round((((_hue - 0.33333333333333331) * num4) * 1530.0) + num3);

            r = num3;
            g = num1;
            b = num2;
        }

        else if (_hue <= 0.66666666666666663)
        {
            num2 = Round((-((_hue - 0.5) * num4) * 1530.0) + num1);

            r = num3;
            g = num2;
            b = num1;
        }

        else if (_hue <= 0.83333333333333337)
        {
            num2 = Round((((_hue - 0.66666666666666663) * num4) * 1530.0) + num3);

            r = num2;
            g = num3;
            b = num1;
        }

        else if (_hue <= 1.0)
        {
            num2 = Round((-((_hue - 0.83333333333333337) * num4) * 1530.0) + num1);

            r = num1;
            g = num3;
            b = num2;
        }


        r = Math.Max(0, Math.Min(r, 255));
        g = Math.Max(0, Math.Min(g, 255));
        b = Math.Max(0, Math.Min(b, 255));

        return Color.FromArgb(_alpha, r, g, b);
    }


    private void RgbtoHsl(Color color)
    {
        int r;
        int g;
        double num4;
        _alpha = color.A;

        if (color.R > color.G)
        {
            r = color.R;
            g = color.G;
        }
        else
        {
            r = color.G;
            g = color.R;
        }

        if (color.B > r) r = color.B;
        else if (color.B < g) g = color.B;

        int num3 = r - g;
        _luminance = r / 255.0;

        if (r == 0) _saturation = 0.0;
        else _saturation = num3 * 1f / r;

        if (num3 == 0) num4 = 0.0;
        else num4 = 60.0 / num3;


        if (r == color.R)
        {
            if (color.G < color.B)
            {
                _hue = (360.0 + (num4 * (color.G - color.B))) / 360.0;
            }
            else
            {
                _hue = (num4 * (color.G - color.B)) / 360.0;
            }
        }
        else if (r == color.G)
        {
            _hue = (120.0 + (num4 * (color.B - color.R))) / 360.0;
        }
        else if (r == color.B)
        {
            _hue = (240.0 + (num4 * (color.R - color.G))) / 360.0;
        }
        else
        {
            _hue = 0.0;
        }
    }


    private static int Round(double val)
    {
        return (int)(val + 0.5);
    }


    static HslColor()
    {
        Empty = new HslColor();
    }

    #endregion // Private methods

}
