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

using ImageGlass.Base;

namespace ImageGlass.UI;

/// <summary>
/// A control for picking color with transparency.
/// </summary>
public partial class ModernColorPicker : UserControl
{
    private Color _colorValue = Color.White;
    private bool _darkMode = true;

    private HslColor _cacheHsl = HslColor.Empty;
    private bool _suspendColorInputsEvent = false;


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets, sets color value.
    /// </summary>
    public Color ColorValue
    {
        get => _colorValue;
        set
        {
            SetColorValue(value);

            // color preview
            ViewColors.Color2 = _colorValue;

            // color inputs
            LoadColorInputBoxValues();
        }
    }


    /// <summary>
    /// Gets, sets color mode.
    /// </summary>
    public ColorMode ColorMode
    {
        get => SliderRgb.ColorMode;
        set => SetColorMode(value, true);
    }


    /// <summary>
    /// Enables or disables control's dark mode.
    /// </summary>
    public bool DarkMode
    {
        get => _darkMode;
        set => _darkMode = value;
    }

    #endregion // Public properties


    public ModernColorPicker()
    {
        InitializeComponent();

        SetStyle(ControlStyles.SupportsTransparentBackColor |
             ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.ResizeRedraw |
             ControlStyles.UserPaint, true);
    }


    // Control events
    #region Control events

    private void BoxGradient_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (ColorMode == ColorMode.Hue)
        {
            // do nothing
        }
        else if (ColorMode == ColorMode.Saturation)
        {
            _cacheHsl = HslColor.FromAhsl(_colorValue.A, e.HslColor.H, _cacheHsl.S, e.HslColor.L);
            SliderRgb.ColorHSL = _cacheHsl;
        }
        else if (ColorMode == ColorMode.Luminance)
        {
            SliderRgb.ColorHSL = e.HslColor;
            _cacheHsl = e.HslColor;
        }
        else
        {
            SliderRgb.ColorRGB = e.RgbColor;
        }


        ViewColors.Color1 =
            SliderAlpha.ColorValue =
            _colorValue = Color.FromArgb(_colorValue.A, e.RgbColor);

        // load color input values
        LoadColorInputBoxValues();
    }

    private void SliderRgb_ValueChanged(object sender, SliderColorValueChangedEventArgs e)
    {
        _cacheHsl = e.HslColor;

        if (ColorMode == ColorMode.Hue
            || ColorMode == ColorMode.Saturation
            || ColorMode == ColorMode.Luminance)
        {
            BoxGradient.ColorHSL = e.HslColor;
        }
        else
        {
            BoxGradient.ColorRGB = e.RgbColor;
        }


        ViewColors.Color1 =
            SliderAlpha.ColorValue =
            _colorValue = Color.FromArgb(_colorValue.A, e.RgbColor);

        // load color input values
        LoadColorInputBoxValues();
    }

    private void SliderAlpha_ValueChanged(object sender, SliderColorValueChangedEventArgs e)
    {
        ViewColors.Color1 = _colorValue = e.RgbColor;

        // load color input values
        LoadColorInputBoxValues();
    }

    private void ChkColorMode_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is not RadioButton rad) return;
        var colorMode = ColorMode;

        // RGB color modes
        if (rad.Name == nameof(RadR))
        {
            colorMode = ColorMode.Red;
        }
        else if (rad.Name == nameof(RadG))
        {
            colorMode = ColorMode.Green;
        }
        else if (rad.Name == nameof(RadB))
        {
            colorMode = ColorMode.Blue;
        }


        // HSV color modes
        else if (rad.Name == nameof(RadH))
        {
            colorMode = ColorMode.Hue;
        }
        else if (rad.Name == nameof(RadS))
        {
            colorMode = ColorMode.Saturation;
        }
        else if (rad.Name == nameof(RadV))
        {
            colorMode = ColorMode.Luminance;
        }


        SetColorMode(colorMode, false);
    }

    private void NumH_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newHsl = HslColor.FromColor(_colorValue);
        newHsl.H = (double)NumH.Value / 360d;

        SetColorValue(newHsl.RgbValue);
    }

    private void NumS_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newHsl = HslColor.FromColor(_colorValue);
        newHsl.S = (double)NumS.Value / 100d;

        SetColorValue(newHsl.RgbValue);
    }

    private void NumL_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newHsl = HslColor.FromColor(_colorValue);
        newHsl.L = (double)NumV.Value / 100d;

        SetColorValue(newHsl.RgbValue);
    }

    private void NumR_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newColor = Color.FromArgb(_colorValue.A, (byte)NumR.Value, _colorValue.G, _colorValue.B);
        SetColorValue(newColor);
    }

    private void NumG_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newColor = Color.FromArgb(_colorValue.A, _colorValue.R, (byte)NumG.Value, _colorValue.B);
        SetColorValue(newColor);
    }

    private void NumB_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newColor = Color.FromArgb(_colorValue.A, _colorValue.R, _colorValue.G, (byte)NumB.Value);
        SetColorValue(newColor);
    }

    private void NumA_ValueChanged(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newColor = _colorValue.WithAlpha((int)NumA.Value);
        SetColorValue(newColor);
    }

    private void TxtHex_LostFocus(object sender, EventArgs e)
    {
        if (_suspendColorInputsEvent) return;

        var newColor = ThemeUtils.ColorFromHex(TxtHex.Text.Trim());
        if (newColor == Color.Empty)
        {
            // restore the original hex value
            _suspendColorInputsEvent = true;
            TxtHex.Text = _colorValue.ToHex();
            _suspendColorInputsEvent = false;
        }
        else
        {
            SetColorValue(newColor);
        }
    }

    #endregion // Control events



    // Private methods
    #region Private methods

    private void SetColorMode(ColorMode mode, bool updateControls)
    {
        SliderRgb.ColorMode = BoxGradient.ColorMode = mode;

        // color controls
        BoxGradient.ColorRGB = _colorValue;
        SliderRgb.ColorRGB = _colorValue;
        SliderAlpha.ColorValue = _colorValue;

        if (updateControls)
        {
            RadR.Checked = mode == ColorMode.Red;
            RadG.Checked = mode == ColorMode.Green;
            RadB.Checked = mode == ColorMode.Blue;

            RadH.Checked = mode == ColorMode.Hue;
            RadS.Checked = mode == ColorMode.Saturation;
            RadV.Checked = mode == ColorMode.Luminance;
        }
    }


    private void SetColorValue(Color value)
    {
        _colorValue = value;

        // color controls
        BoxGradient.ColorRGB = _colorValue;
        SliderRgb.ColorRGB = _colorValue;
        SliderAlpha.ColorValue = _colorValue;

        // color preview
        ViewColors.Color1 = _colorValue;


        _suspendColorInputsEvent = true;
        TxtHex.Text = _colorValue.ToHex();
        _suspendColorInputsEvent = false;
        TxtHex.Refresh();
    }


    private void LoadColorInputBoxValues()
    {
        _suspendColorInputsEvent = true;

        // RGBA
        NumR.Value = _colorValue.R;
        NumR.Refresh();
        NumG.Value = _colorValue.G;
        NumG.Refresh();
        NumB.Value = _colorValue.B;
        NumB.Refresh();
        NumA.Value = _colorValue.A;
        NumA.Refresh();

        // HSV
        NumH.Value = (decimal)Math.Round(SliderRgb.ColorHSL.H * 360f);
        NumH.Refresh();
        NumS.Value = (decimal)Math.Round(SliderRgb.ColorHSL.S * 100f);
        NumS.Refresh();
        NumV.Value = (decimal)Math.Round(SliderRgb.ColorHSL.L * 100f);
        NumV.Refresh();

        // HEX
        TxtHex.Text = _colorValue.ToHex();
        TxtHex.Refresh();

        _suspendColorInputsEvent = false;
    }

    #endregion // Private methods

}
