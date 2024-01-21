/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
namespace ImageGlass.UI;

public class ColorChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets RGB color.
    /// </summary>
    public Color ColorRgb { get; private set; } = Color.Empty;


    /// <summary>
    /// Gets HSL color.
    /// </summary>
    public HslColor ColorHsl { get; private set; } = HslColor.Empty;


    public ColorChangedEventArgs(Color selectedColor)
    {
        ColorRgb = selectedColor;
        ColorHsl = HslColor.FromColor(selectedColor);
    }


    public ColorChangedEventArgs(HslColor selectedHslColor)
    {
        ColorRgb = selectedHslColor.RgbValue;
        ColorHsl = selectedHslColor;
    }

}


public class SliderValueChangedEventArgs(float sliderValue) : EventArgs
{
    /// <summary>
    /// Gets value of the slider.
    /// </summary>
    public float SliderValue { get; private set; } = sliderValue;
}


public class SliderColorValueChangedEventArgs : SliderValueChangedEventArgs
{
    /// <summary>
    /// Gets RGB color.
    /// </summary>
    public Color ColorRgb { get; private set; } = Color.Empty;


    /// <summary>
    /// Gets HSL color.
    /// </summary>
    public HslColor ColorHsl { get; private set; } = HslColor.Empty;


    public SliderColorValueChangedEventArgs(float value, object? colorValue) : base(value)
    {
        if (colorValue is Color rgbColor)
        {
            ColorRgb = rgbColor;
            ColorHsl = HslColor.FromColor(rgbColor);
        }
        else if (colorValue is HslColor hslColor)
        {
            ColorRgb = hslColor.RgbValue;
            ColorHsl = hslColor;
        }
    }
}
