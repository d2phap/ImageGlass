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
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;


/// <summary>
/// A control to pick alpha color.
/// </summary>
[DefaultEvent(nameof(ValueChanged))]
public partial class AlphaColorSlider : VSlider
{
    private Color _colorValue = Color.FromArgb(255, 0, 0, 0);
    private TextureBrush? _checkerboardBrush = null;


    // Public Properties / Events
    #region Public Properties / Events

    /// <summary>
    /// Gets, sets color value.
    /// </summary>
    [Browsable(false)]
    public Color ColorValue
    {
        get => _colorValue;
        set => SetColorValue(value, false);
    }


    /// <summary>
    /// Occurs when <see cref="ColorValue"/> is changed.
    /// </summary>
    public new event EventHandler<SliderColorValueChangedEventArgs>? ValueChanged;

    #endregion // Public Properties / Events



    public AlphaColorSlider()
    {
        
    }



    // Overridden Methods
    #region Overridden Methods

    protected override void Dispose(bool disposing)
    {
        _checkerboardBrush?.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // create checkerboard bitmap brush
        _checkerboardBrush ??= BHelper.CreateCheckerboardTileBrush(10, Color.FromArgb(25, Color.Black), Color.FromArgb(25, Color.White));

        // draw checkerboard
        e.Graphics.FillRectangle(_checkerboardBrush, ContentRect);


        // draw color
        using var gradientBrush = new LinearGradientBrush(ContentRect,
            Color.FromArgb(255, _colorValue), Color.Transparent, 90f);
        e.Graphics.FillRectangle(gradientBrush, ContentRect);


        base.OnPaint(e);
    }


    protected override void OnValueChanged(SliderValueChangedEventArgs e)
    {
        // calculate alpha using the position
        var alpha = (int)Math.Floor(255 - e.SliderValue * 255);
        SetColorValue(Color.FromArgb(alpha, ColorValue), false);

        ValueChanged?.Invoke(this, new SliderColorValueChangedEventArgs(e.SliderValue, ColorValue));
    }


    #endregion // Overridden Methods


    // Private Methods
    #region Private Methods

    /// <summary>
    /// Sets color value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="triggerEvent">Triggers <see cref="ValueChanged"/> event.</param>
    public void SetColorValue(Color value, bool triggerEvent)
    {
        _colorValue = value;
        SetValue((255 - _colorValue.A) / 255f, triggerEvent);

        Refresh();
    }


    #endregion // Private Methods

}
