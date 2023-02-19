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
using ImageGlass.Base.WinApi;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;

public class ModernComboBox : ComboBox
{

    private bool _autoExpanding = false;
    private Bitmap? _buffer = null;
    private bool _clicked = false;
    private bool _hover = false;
    private bool _darkMode = false;
    private int _padding = 10;
    private IColors ColorPalatte => ThemeUtils.GetThemeColorPalatte(_darkMode);
    private float BorderRadius => BHelper.IsOS(WindowsOS.Win11OrLater) ? 3f : 0;


    // Public properties
    #region Public properties

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


    [Category("Appearance")]
    [Description("Determines whether the drop down list should automatically expand to fit items.")]
    [DefaultValue(false)]
    public bool AutoExpanding
    {
        get { return _autoExpanding; }
        set
        {
            _autoExpanding = value;
            Invalidate();
        }
    }


    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color ForeColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color BackColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FlatStyle FlatStyle { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ComboBoxStyle DropDownStyle { get; set; }

    #endregion // Public properties


    public ModernComboBox() : base()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DrawMode = DrawMode.OwnerDrawVariable;
        DoubleBuffered = true;

        base.FlatStyle = FlatStyle.Flat;
        base.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_buffer == null)
            PaintComboboxImage();

        var g = e.Graphics;
        g.DrawImageUnscaled(_buffer, Point.Empty);
    }

    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        base.OnInvalidated(e);
        PaintComboboxImage();
    }

    private void PaintComboboxImage()
    {
        if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
            _buffer = new Bitmap(1, 1);
        if (_buffer == null)
            _buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

        using (var g = Graphics.FromImage(_buffer))
        {
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            var textColor = Enabled
                ? ColorPalatte.AppText
                : ColorPalatte.AppTextDisabled;

            var penWidth = DpiApi.Transform(1.1f);
            var borderColor = ColorPalatte.GreySelection;
            var fillColor = _hover ? ColorPalatte.LighterBackground : ColorPalatte.LightBackground;

            borderColor = _hover ? borderColor.WithBrightness(0.3f) : borderColor;
            if (Focused && TabStop)
                borderColor = ColorPalatte.BlueHighlight;

            using (var b = new SolidBrush(ColorPalatte.GreyBackground))
            {
                g.FillRectangle(b, rect);
            }

            // background
            using (var b = new SolidBrush(fillColor))
            {
                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRoundedRectangle(b, rect, BorderRadius, _clicked, 1);
                g.SmoothingMode = SmoothingMode.None;
            }

            // border
            using (var p = new Pen(borderColor, penWidth))
            {
                p.Alignment = PenAlignment.Outset;
                p.LineJoin = LineJoin.Round;
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;

                var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRoundedRectangle(p, modRect, BorderRadius, _clicked, 1);
                g.SmoothingMode = SmoothingMode.None;
            }

            // arrow
            var arrowColor = _hover
                ? ColorPalatte.AppText.WithAlpha(255)
                : ColorPalatte.AppText.WithAlpha(180);

            using (var arrowPen = new Pen(arrowColor, penWidth))
            {
                arrowPen.LineJoin = LineJoin.Round;
                arrowPen.StartCap = LineCap.Round;
                arrowPen.EndCap = LineCap.Round;


                var arrowSize = rect.Height;
                var initX = rect.Right - arrowSize;

                var p1 = new PointF(
                    initX + arrowSize * 0.35f,
                            arrowSize * 0.4f);

                var p2 = new PointF(
                    initX + arrowSize * 0.5f,
                            arrowSize * 0.55f);

                var p3 = new PointF(
                    initX + arrowSize * 0.65f,
                            arrowSize * 0.4f);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(arrowPen, p1, p2);
                g.DrawLine(arrowPen, p2, p3);
                g.SmoothingMode = SmoothingMode.None;
            }

            // text
            var text = SelectedItem != null ? SelectedItem.ToString() : Text;
            using (var textBrush = new SolidBrush(textColor))
            {
                var padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                                            rect.Top + padding,
                                            rect.Width - 8 - (_padding / 2) - (padding * 2),
                                            rect.Height - (padding * 2));

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter,
                };

                g.DrawString(text, Font, textBrush, modRect, stringFormat);
            }
        }
    }


    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        var g = e.Graphics;
        var rect = e.Bounds;

        var textColor = ColorPalatte.AppText;
        var fillColor = ColorPalatte.GreyBackground;

        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected ||
            (e.State & DrawItemState.Focus) == DrawItemState.Focus ||
            (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect && !TabStop)
        {
            fillColor = ColorPalatte.BlueSelection;
            if (!_darkMode)
                textColor = ColorPalatte.GreyBackground;
        }


        using (var b = new SolidBrush(fillColor))
        {
            g.FillRectangle(b, rect);
        }

        if (e.Index >= 0 && e.Index < Items.Count)
        {
            var text = Items[e.Index].ToString();

            using (var b = new SolidBrush(textColor))
            {
                var padding = 2;

                var modRect = new Rectangle(rect.Left + padding,
                    rect.Top + padding,
                    rect.Width - (padding * 2),
                    rect.Height - (padding * 2));

                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(text, Font, b, modRect, stringFormat);
            }
        }

    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _buffer = null;

        base.Dispose(disposing);
    }

    protected override void OnTabStopChanged(EventArgs e)
    {
        base.OnTabStopChanged(e);
        Invalidate();
    }

    protected override void OnTabIndexChanged(EventArgs e)
    {
        base.OnTabIndexChanged(e);
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnTextUpdate(EventArgs e)
    {
        base.OnTextUpdate(e);
        Invalidate();
    }

    protected override void OnSelectedValueChanged(EventArgs e)
    {
        base.OnSelectedValueChanged(e);
        Invalidate();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        _buffer = null;
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        _buffer = null;
        Invalidate();
    }

    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);
        if (_autoExpanding)
        {
            int width = DropDownWidth;
            Graphics g = CreateGraphics();
            Font font = Font;
            int newWidth;
            foreach (string s in Items)
            {
                newWidth = (int)g.MeasureString(s, font).Width + 25;
                if (newWidth > width)
                    width = newWidth;
            }
            DropDownWidth = width;
        }
        _clicked = true;
        Invalidate();
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        _clicked = false;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }


}
