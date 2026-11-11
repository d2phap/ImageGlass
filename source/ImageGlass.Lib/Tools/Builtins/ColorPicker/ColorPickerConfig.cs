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
using ImageGlass.Common.Types;
using System.Text.Json.Serialization;

namespace ImageGlass.Tools;


[JsonSerializable(typeof(ColorPickerConfig))]
public partial class ColorPickerConfigJsonContext : JsonSerializerContext { }


/// <summary>
/// Provides settings for Color Picker tool.
/// </summary>
public class ColorPickerConfig() : PhReactive
{

    /// <summary>
    /// Gets, sets option to show alpha value of RGB code.
    /// </summary>
    public bool ShowRgbWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = true;


    /// <summary>
    /// Gets, sets option to show alpha value of HEX code.
    /// </summary>
    public bool ShowHexWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = true;


    /// <summary>
    /// Gets, sets option to show alpha value of HSL code.
    /// </summary>
    public bool ShowHslWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = true;


    /// <summary>
    /// Gets, sets option to show alpha value of HSV code.
    /// </summary>
    public bool ShowHsvWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = true;


    /// <summary>
    /// Gets, sets option to show alpha value of HSV code.
    /// </summary>
    public bool ShowCmykWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = false;


    /// <summary>
    /// Gets, sets option to show alpha value of CIELAB code.
    /// </summary>
    public bool ShowCIELabWithAlpha
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = false;

}
