/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.Reflection;
using System.Security;

namespace ImageGlass.UI;

public class ModernNumericUpDown : NumericUpDown
{
    private bool _mouseDown = false;
    private bool _darkMode = false;


    private IColors ColorPalatte => ThemeUtils.GetThemeColorPalatte(_darkMode);

    private float BorderRadius => BHelper.IsOS(WindowsOS.Win11OrLater) ? 4f : 0;


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


    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color ForeColor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color BackColor { get; set; }


    public ModernNumericUpDown()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.UserPaint, true);

        base.ForeColor = ColorPalatte.LightText;
        base.BackColor = ColorPalatte.LightBackground;

        Controls[0].Paint += ModernNumericUpDown_Paint;

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
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
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

    protected override void OnTextBoxLostFocus(object source, EventArgs e)
    {
        base.OnTextBoxLostFocus(source, e);
        Invalidate();
    }

    private void ModernNumericUpDown_Paint(object? sender, PaintEventArgs e)
    {
        PaintUpDownControls(e);
    }

    private void PaintUpDownControls(PaintEventArgs e)
    {
        if (!_darkMode) return;

        var g = e.Graphics;
        var rect = e.ClipRectangle;
        var fillColor = ColorPalatte.HeaderBackground;

        // draw up/down controls
        using (var b = new SolidBrush(fillColor))
        {
            var modRect = rect with { Y = rect.Y - 1, Height = rect.Height + 1 };
            var modRect2 = new Rectangle(rect.X - 2, rect.Y - 1, rect.Width + 2, rect.Height + 1);

            g.FillRectangle(new SolidBrush(BackColor), modRect2);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.DrawRoundedRectangle(new Pen(ColorPalatte.GreySelection, 1), modRect, BorderRadius);
            g.FillRoundedRectangle(b, modRect, BorderRadius);
           
            g.SmoothingMode = SmoothingMode.None;
        }

        var mousePos = Controls[0].PointToClient(Cursor.Position);

        var upArea = new Rectangle(0, 0, rect.Width, rect.Height / 2);
        var upHot = upArea.Contains(mousePos);

        var arrowColor = upHot ? ColorPalatte.ActiveControl : ColorPalatte.GreyHighlight;
        if (upHot && _mouseDown)
            arrowColor = ColorPalatte.LightText;

        using (var p = new Pen(arrowColor, 1))
        {
            var x = upArea.Width / 2 - 3;
            var y = upArea.Height / 2 - 2;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawLine(p, x, y + 3, x + 3, y);
            g.DrawLine(p, x + 3, y, x + 6, y + 3);
        }

        var downArea = new Rectangle(0, rect.Height / 2, rect.Width, rect.Height / 2);
        var downHot = downArea.Contains(mousePos);

        arrowColor = downHot ? ColorPalatte.ActiveControl : ColorPalatte.GreyHighlight;
        if (downHot && _mouseDown)
        {
            arrowColor = ColorPalatte.LightText;
        }

        using (var p = new Pen(arrowColor, 1))
        {
            var x = downArea.Width / 2 - 3;
            var y = downArea.Top + downArea.Height / 2 - 2;
            g.DrawLine(p, x, y, x + 3, y + 3);
            g.DrawLine(p, x + 3, y + 3, x + 6, y);
            g.SmoothingMode = SmoothingMode.None;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        PaintUpDownControls(e);

        if (!_darkMode) return;

        var g = e.Graphics;
        var rect = e.ClipRectangle; // new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        var borderColor = ColorPalatte.GreySelection;

        if (Focused && TabStop)
            borderColor = ColorPalatte.BlueHighlight;

        using var p = new Pen(borderColor, 1);
        var modRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
        g.DrawRectangle(new Pen(BackColor, 2), rect);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(ColorPalatte.LightBackground);

        g.DrawRoundedRectangle(p, modRect, BorderRadius);
        g.FillRoundedRectangle(brush, modRect, BorderRadius);

        g.SmoothingMode = SmoothingMode.None;
    }
}
