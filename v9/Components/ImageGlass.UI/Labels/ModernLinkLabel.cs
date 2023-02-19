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

public class ModernLinkLabel : LinkLabel
{

    private bool _autoUpdateHeight;
    private bool _isGrowing;
    private bool _darkMode = false;

    private ModernControlState _controlState = ModernControlState.Normal;
    private IColors ColorPalatte => ThemeUtils.GetThemeColorPalatte(_darkMode);


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


    public ModernLinkLabel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        // draw parent background
        ButtonRenderer.DrawParentBackground(e.Graphics, Bounds, this);

        var g = e.Graphics;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;

        var textColor = ColorPalatte.BlueHighlight;
        var backColor = DarkMode
            ? Color.White.WithAlpha(0)
            : Color.Black.WithAlpha(0);

        if (Enabled)
        {
            if (_controlState == ModernControlState.Hover)
            {
                backColor = backColor.WithAlpha(DarkMode ? 5 : 20);
            }
            else if (_controlState == ModernControlState.Pressed)
            {
                textColor = textColor.WithBrightness(0.15f);
                backColor = backColor.WithAlpha(DarkMode ? 10 : 35);
            }
        }
        // disabled style
        else
        {
            backColor = Color.Transparent;
            textColor = ColorPalatte.AppTextDisabled;
        }

        // draw hover background
        using (var brush = new SolidBrush(backColor))
        {
            var bgRect = new Rectangle(0, 0, Bounds.Width - 1, Bounds.Height - 1);
            g.FillRoundedRectangle(brush, bgRect, 2f);
        }


        // draw text
        using var font = new Font(Font.FontFamily, Font.Size, FontStyle.Underline);
        using (var brush = new SolidBrush(textColor))
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

            g.DrawString(Text, font, brush, modRect, textFormat);
        }
    }


    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        SetControlState(ModernControlState.Hover);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        SetControlState(ModernControlState.Normal);
    }


    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        SetControlState(ModernControlState.Pressed);
    }


    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        SetControlState(ModernControlState.Normal);
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


    private void SetControlState(ModernControlState controlState)
    {
        if (_controlState != controlState)
        {
            _controlState = controlState;
            Invalidate();
        }
    }


}
