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
using System.ComponentModel;

namespace ImageGlass.UI;


/// <summary>
/// A control to pick non-alpha color.
/// </summary>
[DefaultEvent(nameof(ValueChanged))]
public partial class RgbColorSlider : VSlider
{
    private ColorMode _colorMode = ColorMode.Luminance;
    private HslColor _colorHSL = HslColor.FromAhsl(255);
    private Color _colorRGB = Color.Empty;


    /// <summary>
    /// Occurs when <see cref="ColorValue"/> is changed.
    /// </summary>
    public new event EventHandler<SliderColorValueChangedEventArgs>? ValueChanged;


    // Public Properties
    #region Public Properties

    /// <summary>
    /// Gets, sets RGB color value.
    /// </summary>
    [Browsable(false)]
    public Color ColorRGB
    {
        get => _colorRGB;
        set => SetColorValue(value, false);
    }


    /// <summary>
    /// Gets, sets, HSL color value.
    /// </summary>
    [Browsable(false)]
    public HslColor ColorHSL
    {
        get => _colorHSL;
        set => SetColorValue(value, false);
    }


    /// <summary>
    /// Gets, set color mode
    /// </summary>
    public ColorMode ColorMode
    {
        get => _colorMode;
        set => SetColorMode(value, false);
    }

    #endregion // Public Properties



    public RgbColorSlider()
    {
        _colorHSL = HslColor.FromAhsl(1, 1, 1);
        _colorRGB = _colorHSL.RgbValue;
    }



    // Overridden Methods
    #region Overridden Methods

    protected override void OnPaint(PaintEventArgs e)
    {
        var h = NubHeight * DpiScale;
        var color = HslColor.FromAhsl(255);

        switch (ColorMode)
        {
            case ColorMode.Hue:
                color.L = color.S = 1.0;
                break;

            case ColorMode.Saturation:
                color.H = ColorHSL.H;
                color.L = ColorHSL.L;
                break;

            case ColorMode.Luminance:
                color.H = ColorHSL.H;
                color.S = ColorHSL.S;
                break;
        }


        for (int i = 0; i < ContentRect.Height; i++)
        {
            var lineColor = Color.Empty;
            double num2;

            if (ColorMode < ColorMode.Hue)
            {
                num2 = 255f - Math.Round(255f * i / ContentRect.Height);
            }
            else
            {
                num2 = 1f - i / ContentRect.Height;
            }

            switch (ColorMode)
            {
                case ColorMode.Red:
                    lineColor = Color.FromArgb((int)num2, ColorRGB.G, ColorRGB.B);
                    break;

                case ColorMode.Green:
                    lineColor = Color.FromArgb(ColorRGB.R, (int)num2, ColorRGB.B);
                    break;

                case ColorMode.Blue:
                    lineColor = Color.FromArgb(ColorRGB.R, ColorRGB.G, (int)num2);
                    break;

                case ColorMode.Hue:
                    color.H = num2;
                    lineColor = color.RgbValue;
                    break;

                case ColorMode.Saturation:
                    color.S = num2;
                    lineColor = color.RgbValue;
                    break;

                case ColorMode.Luminance:
                    color.L = num2;
                    lineColor = color.RgbValue;
                    break;
            }

            using var pen = new Pen(lineColor);
            e.Graphics.DrawLine(pen,
                ContentRect.X,
                i + h / 2,
                ContentRect.Right,
                i + h / 2
            );
        }

        base.OnPaint(e);
    }


    protected override void OnValueChanged(SliderValueChangedEventArgs e)
    {
        var selectedColor = GetColorFromSliderValue(e.SliderValue, ColorMode);
        SetColorValue(selectedColor, false);

        ValueChanged?.Invoke(this, new SliderColorValueChangedEventArgs(e.SliderValue, selectedColor));
    }

    #endregion // Overridden Methods



    // Private Methods
    #region Private Methods

    public void SetColorValue(object? colorValue, bool triggerEvent)
    {
        if (colorValue == null) return;
        var sliderValue = GetSliderValueFromColor(colorValue);

        if (colorValue is Color rgb)
        {
            _colorRGB = rgb;
            _colorHSL = HslColor.FromColor(rgb);
        }
        else if (colorValue is HslColor hsl)
        {
            _colorRGB = hsl.RgbValue;
            _colorHSL = hsl;
        }

        SetValue(sliderValue, triggerEvent);

        // repaint the control because SetValue() does not do it when value is unchanged.
        if (sliderValue == Value)
        {
            Refresh();
        }
    }


    private float GetSliderValueFromColor(object colorValue)
    {
        var sliderValue = Value;
        HslColor hsl;
        if (colorValue is Color rgb)
        {
            hsl = HslColor.FromColor(rgb);
        }
        else
        {
            hsl = (HslColor)colorValue;
        }


        if (ColorMode == ColorMode.Red)
        {
            sliderValue = hsl.RgbValue.R / 255f;
        }
        else if (ColorMode == ColorMode.Green)
        {
            sliderValue = hsl.RgbValue.G / 255f;
        }
        else if (ColorMode == ColorMode.Blue)
        {
            sliderValue = hsl.RgbValue.B / 255f;
        }
        else if (ColorMode == ColorMode.Hue)
        {
            sliderValue = (float)hsl.H;
        }
        else if (ColorMode == ColorMode.Saturation)
        {
            sliderValue = (float)hsl.S;
        }
        else if (ColorMode == ColorMode.Luminance)
        {
            sliderValue = (float)hsl.L;
        }


        return 1 - sliderValue;
    }


    private void SetColorMode(ColorMode mode, bool triggerEvent)
    {
        _colorMode = mode;
        var color = GetColorFromSliderValue(Value, _colorMode);

        SetColorValue(color, triggerEvent);
    }


    private object? GetColorFromSliderValue(float sliderValue, ColorMode mode)
    {
        // RGB color modes
        if (mode == ColorMode.Red)
        {
            return Color.FromArgb(255 - (int)Math.Round(255.0 * sliderValue), ColorRGB.G, ColorRGB.B);
        }

        if (mode == ColorMode.Green)
        {
            return Color.FromArgb(ColorRGB.R, 255 - (int)Math.Round(255.0 * sliderValue), ColorRGB.B);
        }

        if (mode == ColorMode.Blue)
        {
            return Color.FromArgb(ColorRGB.R, ColorRGB.G, 255 - (int)Math.Round(255.0 * sliderValue));
        }


        // HSL color modes
        if (mode == ColorMode.Hue)
        {
            var hsl = ColorHSL;
            hsl.H = 1 - sliderValue;

            return hsl;
        }

        if (mode == ColorMode.Saturation)
        {
            var hsl = ColorHSL;
            hsl.S = 1 - sliderValue;

            return hsl;
        }

        if (mode == ColorMode.Luminance)
        {
            var hsl = ColorHSL;
            hsl.L = 1 - sliderValue;

            return hsl;
        }

        return null;
    }


    #endregion // Private Methods

}
