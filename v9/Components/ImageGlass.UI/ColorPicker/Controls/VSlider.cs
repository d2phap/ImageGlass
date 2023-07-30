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
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;


/// <summary>
/// A vertical slider control
/// </summary>
[DefaultEvent(nameof(ValueChanged))]
public class VSlider : Control
{
    private float _nubHeight = 8f;
    private float _value = 0;
    private bool _darkMode = false;
    private bool _isHover = false;
    private bool _isPressed = false;


    // Internal properties
    #region Internal properties

    /// <summary>
    /// Gets the bounding rectangle of slider bar.
    /// </summary>
    internal RectangleF ContentRect
    {
        get
        {
            var gap = _nubHeight * DpiScale;
            var x = 7 * gap / 8;
            var y = gap / 2;

            var rect = new RectangleF(x, y,
                Width - x - gap,
                Height - y * 2);

            return rect;
        }
    }


    /// <summary>
    /// Gets the color according to the <see cref="DarkMode"/> value.
    /// </summary>
    internal Color ThemeColor => DarkMode ? Color.White : Color.Black;


    /// <summary>
    /// Gets the maximum position corresponding to the <see cref="Value"/> of <see cref="VSlider"/>.
    /// </summary>
    internal float PositionMax => ContentRect.Height - DpiScale;


    /// <summary>
    /// Gets the position corresponding to the <see cref="Value"/> of <see cref="VSlider"/>.
    /// </summary>
    internal float Position => Value * PositionMax;


    /// <summary>
    /// Gets the height of slider nubs
    /// </summary>
    internal float NubHeight => _nubHeight;

    #endregion // Internal properties



    // Public Properties / Events
    #region Public Properties / Events

    /// <summary>
    /// Gets, sets the value of <see cref="VSlider"/> scaled in range [0; 1].
    /// </summary>
    public float Value
    {
        get => _value;
        set => SetValue(value, true);
    }


    /// <summary>
    /// Gets, sets dark mode for <see cref="VSlider"/>.
    /// </summary>
    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            if (_darkMode != value)
            {
                Invalidate();
            }

            _darkMode = value;
        }
    }


    /// <summary>
    /// Gets DPI scale value.
    /// </summary>
    public float DpiScale => DeviceDpi / 96f;


    /// <summary>
    /// Occurs when slider's <see cref="Value"/> is changed.
    /// </summary>
    public event EventHandler<SliderValueChangedEventArgs>? ValueChanged;

    #endregion // Public Properties / Events



    /// <summary>
    /// Initialize new instance of <see cref="VSlider"/>.
    /// </summary>
    public VSlider()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint, true);
    }



    // Override / Virtual Methods
    #region Override / Virtual Methods

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _nubHeight = Width / 10f;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHover = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _isPressed = true;

        var pos = e.Y - _nubHeight / 2 * DpiScale;
        Value = pos / PositionMax;

        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isPressed)
        {
            var pos = e.Y - _nubHeight / 2 * DpiScale;
            Value = pos / PositionMax;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // draw border
        using var borderPen = new Pen(Color.FromArgb(50, ThemeColor), DpiScale) {
            Alignment = PenAlignment.Inset,
        };
        e.Graphics.DrawRectangle(borderPen, ContentRect.X, ContentRect.Y, ContentRect.Width, ContentRect.Height);

        // draw slider nubs
        DrawSliderNubs(e.Graphics);
    }


    /// <summary>
    /// Emits event <see cref="ValueChanged"/>.
    /// </summary>
    protected virtual void OnValueChanged(SliderValueChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    #endregion // Override / Virtual Methods



    // Internal virtual Methods
    #region Internal virtual Methods

    /// <summary>
    /// Sets value for <see cref="Value"/>.
    /// </summary>
    /// <param name="sliderValue"></param>
    /// <param name="triggerEvent">Triggers <see cref="ValueChanged"/> event.</param>
    internal virtual void SetValue(float sliderValue, bool triggerEvent)
    {
        var val = Math.Clamp(sliderValue, 0, 1);
        if (val == _value) return;

        _value = val;
        Refresh();

        if (triggerEvent)
        {
            OnValueChanged(new SliderValueChangedEventArgs(_value));
        }
    }


    /// <summary>
    /// Draw slider nubs
    /// </summary>
    internal virtual void DrawSliderNubs(Graphics g)
    {
        var h = _nubHeight * DpiScale;
        var smoothMode = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var borderOpacity = DarkMode ? 150 : 200;
        using var borderPen = new Pen(Color.FromArgb(borderOpacity, ThemeColor), DpiScale);

        var fillOpacity = 40;
        if (_isHover) fillOpacity = fillOpacity = 20;
        if (_isPressed) fillOpacity = fillOpacity = 60;

        using var fillBrush = new SolidBrush(Color.FromArgb(fillOpacity, ThemeColor));
        var points = new PointF[] {
            new(1*h/8, Position),
            new(3*h/8, Position),
            new(7*h/8, Position + 4*h/8),
            new(3*h/8, Position + h),
            new(1*h/8, Position + h),
            new(0, Position + 7*h/8),
            new(0, Position + 1*h/8)
        };

        g.FillPolygon(fillBrush, points);
        g.DrawPolygon(borderPen, points);

        points[0] = new(Width - (2 * h / 8), Position);
        points[1] = new(Width - (4 * h / 8), Position);
        points[2] = new(Width - h, Position + 4 * h / 8);
        points[3] = new(Width - (4 * h / 8), Position + h);
        points[4] = new(Width - (2 * h / 8), Position + h);
        points[5] = new(Width - (1 * h / 8), Position + 7 * h / 8);
        points[6] = new(Width - (1 * h / 8), Position + 1 * h / 8);

        g.FillPolygon(fillBrush, points);
        g.DrawPolygon(borderPen, points);

        g.SmoothingMode = smoothMode;
    }

    #endregion // Internal virtual Methods

}
