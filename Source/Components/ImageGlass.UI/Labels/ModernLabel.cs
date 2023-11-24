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

namespace ImageGlass.UI;

public class ModernLabel : Label
{
    private bool _autoUpdateHeight;
    private bool _isGrowing;
    private bool _darkMode = false;

    private IColors ColorPalatte => BHelper.GetThemeColorPalatte(_darkMode);


    #region Property Region

    /// <summary>
    /// Toggles dark mode for this <see cref="ModernButton"/> control.
    /// </summary>
    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            _darkMode = value;

            Invalidate();
        }
    }


    [Category("Layout")]
    [Description("Enables automatic height sizing based on the contents of the label.")]
    [DefaultValue(false)]
    public bool AutoUpdateHeight
    {
        get { return _autoUpdateHeight; }
        set
        {
            _autoUpdateHeight = value;

            if (_autoUpdateHeight)
            {
                AutoSize = false;
                ResizeLabel();
            }
        }
    }

    #endregion


    public ModernLabel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        BackColor = Color.Transparent;
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        var textColor = ForeColor == DefaultForeColor
            ? ColorPalatte.AppText
            : ForeColor;

        if (!Enabled)
        {
            textColor = ColorPalatte.AppTextDisabled;
        }

        using (var b = new SolidBrush(BackColor))
        {
            g.FillRectangle(b, Bounds);
        }

        using (var b = new SolidBrush(textColor))
        {
            var textFormat = new StringFormat();
            textFormat.Trimming = StringTrimming.EllipsisCharacter;

            if (TextAlign == ContentAlignment.TopLeft)
            {
                textFormat.LineAlignment = StringAlignment.Near;
                textFormat.Alignment = StringAlignment.Near;
            }
            else if (TextAlign == ContentAlignment.TopCenter)
            {
                textFormat.LineAlignment = StringAlignment.Near;
                textFormat.Alignment = StringAlignment.Center;
            }
            else if (TextAlign == ContentAlignment.TopRight)
            {
                textFormat.LineAlignment = StringAlignment.Near;
                textFormat.Alignment = StringAlignment.Far;
            }


            else if (TextAlign == ContentAlignment.MiddleLeft)
            {
                textFormat.LineAlignment = StringAlignment.Center;
                textFormat.Alignment = StringAlignment.Near;
            }
            else if (TextAlign == ContentAlignment.MiddleCenter)
            {
                textFormat.LineAlignment = StringAlignment.Center;
                textFormat.Alignment = StringAlignment.Center;
            }
            else if (TextAlign == ContentAlignment.MiddleRight)
            {
                textFormat.LineAlignment = StringAlignment.Center;
                textFormat.Alignment = StringAlignment.Far;
            }


            else if (TextAlign == ContentAlignment.BottomLeft)
            {
                textFormat.LineAlignment = StringAlignment.Far;
                textFormat.Alignment = StringAlignment.Near;
            }
            else if (TextAlign == ContentAlignment.BottomCenter)
            {
                textFormat.LineAlignment = StringAlignment.Far;
                textFormat.Alignment = StringAlignment.Center;
            }
            else if (TextAlign == ContentAlignment.BottomRight)
            {
                textFormat.LineAlignment = StringAlignment.Far;
                textFormat.Alignment = StringAlignment.Far;
            }

            var gapX = this.ScaleToDpi(-2f);
            var missingWidth = g.MeasureString("W", Font).Width;

            var modRect = new RectangleF(
                gapX + Padding.Left,
                Padding.Top,
                Bounds.Width + missingWidth - Padding.Horizontal * 1.5f,
                Bounds.Height - Padding.Vertical);

            g.DrawString(Text, Font, b, modRect, textFormat);
        }
    }


    private void ResizeLabel()
    {
        if (!_autoUpdateHeight || _isGrowing)
            return;

        try
        {
            _isGrowing = true;
            var sz = new Size(Width, int.MaxValue);
            sz = TextRenderer.MeasureText(Text, Font, sz, TextFormatFlags.WordBreak);
            Height = sz.Height;
        }
        finally
        {
            _isGrowing = false;
        }
    }

}
