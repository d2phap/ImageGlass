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
using ImageGlass.Base;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;

public class ColorView : Control
{
    private Color _color1 = Color.Empty;
    private Color _color2 = Color.Empty;
    private bool _darkMode = false;
    private TextureBrush? _checkerboardBrush = null;


    /// <summary>
    /// Gets the color according to the <see cref="DarkMode"/> value.
    /// </summary>
    internal Color ThemeColor => DarkMode ? Color.White : Color.Black;


    /// <summary>
    /// Gets, sets left color value.
    /// </summary>
    public Color Color1
    {
        get => _color1;
        set
        {
            var oldColor = _color1;
            _color1 = value;

            if (_color1 != oldColor)
            {
                Refresh();
            }
        }
    }


    /// <summary>
    /// Gets, sets right color value.
    /// </summary>
    public Color Color2
    {
        get => _color2;
        set
        {
            var oldColor = _color2;
            _color2 = value;

            if (_color2 != oldColor)
            {
                Refresh();
            }
        }
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


    public ColorView()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint, true);
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // create checkerboard bitmap brush
        _checkerboardBrush ??= BHelper.CreateCheckerboardTileBrush(10, Color.FromArgb(25, Color.Black), Color.FromArgb(25, Color.White));

        // draw checkerboard
        e.Graphics.FillRectangle(_checkerboardBrush, e.ClipRectangle);


        // draw color 1 (left side)
        using var colorBrush = new SolidBrush(Color1);
        e.Graphics.FillRectangle(colorBrush, new RectangleF(
            e.ClipRectangle.X,
            e.ClipRectangle.Y,
            e.ClipRectangle.Width / 2,
            e.ClipRectangle.Height));

        // draw color 2 (right side)
        colorBrush.Color = Color2;
        e.Graphics.FillRectangle(colorBrush, new RectangleF(
            e.ClipRectangle.X + e.ClipRectangle.Width / 2,
            e.ClipRectangle.Y,
            e.ClipRectangle.Width / 2,
            e.ClipRectangle.Height));


        // draw border
        using var borderPen = new Pen(Color.FromArgb(50, ThemeColor), DpiScale)
        {
            Alignment = PenAlignment.Inset,
        };
        e.Graphics.DrawRectangle(borderPen, e.ClipRectangle);
    }

}
