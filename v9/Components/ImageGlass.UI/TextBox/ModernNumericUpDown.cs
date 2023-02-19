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
using System.Reflection;
using System.Security;

namespace ImageGlass.UI;

public class ModernNumericUpDown : NumericUpDown
{
    private bool _mouseDown = false;
    private bool _mouseHover = false;
    private bool _darkMode = false;
    private IColors ColorPalatte => ThemeUtils.GetThemeColorPalatte(_darkMode);
    private float BorderRadius => BHelper.IsOS(WindowsOS.Win11OrLater) ? 1f : 0;


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

            Controls[1].BackColor = ColorPalatte.LightBackground;
            Controls[1].ForeColor = ColorPalatte.AppText;

            Invalidate();
        }
    }


    /// <summary>
    /// Gets, sets value indicates that the text should be selected if the control is focused or clicked.
    /// </summary>
    public bool SelectAllTextOnFocus { get; set; } = true;


    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color ForeColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color BackColor { get; set; }

    #endregion // Public properties


    public ModernNumericUpDown()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.UserPaint, true);

        base.ForeColor = ColorPalatte.AppText;
        base.BackColor = ColorPalatte.LightBackground;

        Controls[0].Paint += UpDownControls_Paint;
        Controls[0].MouseEnter += Control_MouseEnter;
        Controls[0].MouseLeave += Control_MouseLeave;
        Controls[1].MouseEnter += Control_MouseEnter;
        Controls[1].MouseLeave += Control_MouseLeave;

        try
        {
            // Prevent flickering, only if our assembly has reflection permission
            var type = Controls[0].GetType();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var method = type.GetMethod("SetStyle", flags);

            if (method != null)
            {
                object[] @params = { ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true };
                method.Invoke(Controls[0], @params);
            }
        }
        catch (SecurityException)
        {
            // Don't do anything, we are running in a trusted context
        }
    }

    private void Control_MouseEnter(object? sender, EventArgs e)
    {
        _mouseHover = true;
        this.Invalidate();
    }

    private void Control_MouseLeave(object? sender, EventArgs e)
    {
        _mouseHover = false;
        this.Invalidate();
    }

    private void UpDownControls_Paint(object? sender, PaintEventArgs e)
    {
        PaintUpDownControls(e);
    }


    private void PaintUpDownControls(PaintEventArgs e)
    {
        var g = e.Graphics;
        var rect = e.ClipRectangle;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;


        // up/down background
        using (var b = new SolidBrush(ColorPalatte.HeaderBackground))
        {
            var modRect = new Rectangle(rect.X - 2, rect.Y - 1, rect.Width + 2, rect.Height + 1);

            g.FillRectangle(new SolidBrush(ColorPalatte.LightBackground), modRect);
        }


        // up arrow
        var mousePos = Controls[0].PointToClient(Cursor.Position);
        var upArea = new Rectangle(0, 0, rect.Width, rect.Height / 2);
        var isUpHovered = upArea.Contains(mousePos);

        // arrow
        var arrowColor = isUpHovered
            ? ColorPalatte.GreySelection.WithBrightness(0.5f)
            : ColorPalatte.GreySelection.WithBrightness(0.3f);
        if (isUpHovered && _mouseDown)
        {
            arrowColor = ColorPalatte.AppText;
        }

        using (var p = new Pen(arrowColor, DpiApi.Transform(1.1f)))
        {
            var x = upArea.Width / 2 - 3;
            var y = upArea.Height / 2 - 2;

            p.LineJoin = LineJoin.Round;
            p.StartCap = LineCap.Round;
            p.EndCap = LineCap.Round;

            g.DrawLine(p, x, y + 3, x + 3, y);
            g.DrawLine(p, x + 3, y, x + 6, y + 3);
        }


        // down arrow
        var downArea = new Rectangle(0, rect.Height / 2, rect.Width, rect.Height / 2);
        var isDownHovered = downArea.Contains(mousePos);

        arrowColor = isDownHovered
            ? ColorPalatte.GreySelection.WithBrightness(0.5f)
            : ColorPalatte.GreySelection.WithBrightness(0.3f);
        if (isDownHovered && _mouseDown)
        {
            arrowColor = ColorPalatte.AppText;
        }

        using (var p = new Pen(arrowColor, DpiApi.Transform(1.1f)))
        {
            var x = downArea.Width / 2 - 3;
            var y = downArea.Top + downArea.Height / 2 - 2;

            p.LineJoin = LineJoin.Round;
            g.DrawLine(p, x, y, x + 3, y + 3);
            g.DrawLine(p, x + 3, y + 3, x + 6, y);
        }

        g.SmoothingMode = SmoothingMode.None;
        g.CompositingQuality = CompositingQuality.Default;
    }


    // Protected override methods
    #region Protected override methods

    protected override void Dispose(bool disposing)
    {
        Controls[0].Paint -= UpDownControls_Paint;
        Controls[0].MouseEnter -= Control_MouseEnter;
        Controls[0].MouseLeave -= Control_MouseLeave;
        Controls[1].MouseEnter -= Control_MouseEnter;
        Controls[1].MouseLeave -= Control_MouseLeave;

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var rect = e.ClipRectangle;
        var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);


        // fill background
        using (var brush = new SolidBrush(Controls[1].BackColor))
        {
            g.FillRoundedRectangle(brush, borderRect, BorderRadius);
        }


        // draw border
        var borderColor = ColorPalatte.GreySelection;
        if (Focused && TabStop)
        {
            borderColor = ColorPalatte.BlueHighlight;
        }
        else if (_mouseHover)
        {
            borderColor = borderColor.WithBrightness(0.3f);
        }

        using (var pen = new Pen(borderColor, DpiApi.Transform(1.1f)))
        {
            pen.Alignment = PenAlignment.Outset;
            pen.LineJoin = LineJoin.Round;
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawRoundedRectangle(pen, borderRect, BorderRadius);
            g.SmoothingMode = SmoothingMode.None;
        }
    }


    protected override void OnMouseMove(MouseEventArgs e)
    {
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _mouseDown = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _mouseDown = false;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseHover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouseHover = false;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if (SelectAllTextOnFocus)
        {
            Select(0, Text.Length);
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();

        if (SelectAllTextOnFocus)
        {
            Select(0, Text.Length);
        }
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnTextBoxLostFocus(object source, EventArgs e)
    {
        base.OnTextBoxLostFocus(source, e);
        Invalidate();
    }

    #endregion // Protected override methods

}
