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

---------------------
ModernButton is based on DarkUI
Url: https://github.com/RobinPerris/DarkUI
License: MIT, https://github.com/RobinPerris/DarkUI/blob/master/LICENSE
---------------------
*/

using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;

[ToolboxBitmap(typeof(Button))]
[DefaultEvent("Click")]
public class ModernButton : Button
{

    #region Field Region

    private const int DEFAULT_PADDING = 10;

    private ModernButtonStyle _style = ModernButtonStyle.Normal;
    private ModernControlState _buttonState = ModernControlState.Normal;

    private bool _isDefault;
    private bool _spacePressed;
    private bool _darkMode = false;

    private readonly int _padding = DEFAULT_PADDING / 2;
    private int _imagePadding = 2; // Consts.Padding / 2

    private SHSTOCKICONID? _systemIcon = null;

    #endregion


    #region Designer Property Region

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


    [DefaultValue(false)]
    public SHSTOCKICONID? SystemIcon
    {
        get => _systemIcon;
        set
        {
            _systemIcon = value;

            Image = SystemIconApi.GetSystemIcon(_systemIcon, false);
            Invalidate();
        }
    }


    public new string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    public new bool Enabled
    {
        get { return base.Enabled; }
        set
        {
            base.Enabled = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [Description("Determines the style of the button.")]
    [DefaultValue(ModernButtonStyle.Normal)]
    public ModernButtonStyle ButtonStyle
    {
        get { return _style; }
        set
        {
            _style = value;
            Invalidate();
        }
    }

    [Category("Appearance")]
    [Description("Determines the amount of padding between the image and text.")]
    [DefaultValue(5)]
    public int ImagePadding
    {
        get { return _imagePadding; }
        set
        {
            _imagePadding = value;
            Invalidate();
        }
    }

    #endregion


    #region Code Property Region

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AutoEllipsis
    {
        get { return false; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ModernControlState ButtonState
    {
        get { return _buttonState; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ContentAlignment ImageAlign
    {
        get { return base.ImageAlign; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool FlatAppearance
    {
        get { return false; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FlatStyle FlatStyle
    {
        get { return base.FlatStyle; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ContentAlignment TextAlign
    {
        get { return base.TextAlign; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseCompatibleTextRendering
    {
        get { return false; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseVisualStyleBackColor
    {
        get { return false; }
    }

    #endregion


    #region Constructor Region

    public ModernButton()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        base.UseVisualStyleBackColor = false;
        base.UseCompatibleTextRendering = false;

        SetButtonState(ModernControlState.Normal);
        Padding = new Padding(_padding);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
    }

    private void SetButtonState(ModernControlState buttonState)
    {
        if (_buttonState != buttonState)
        {
            _buttonState = buttonState;
            Invalidate();
        }
    }

    #endregion



    #region Event Handler Region

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

        var colors = ThemeUtils.GetThemeColorPalatte(DarkMode);
        var textColor = colors.LightText;
        var borderColor = colors.GreySelection;
        var fillColor = _isDefault
            ? colors.DarkBlueBackground
            : colors.LightBackground;

        var borderRadius = 0f;
        if (BHelper.IsOS(WindowsOS.Win11OrLater))
        {
            borderRadius = 4f;
        }

        if (Enabled)
        {
            if (ButtonStyle == ModernButtonStyle.Normal)
            {
                switch (ButtonState)
                {
                    case ModernControlState.Hover:
                        fillColor = _isDefault
                            ? colors.BlueBackground
                            : colors.LighterBackground;
                        borderColor = borderColor.WithBrightness(0.3f);
                        break;

                    case ModernControlState.Pressed:
                        fillColor = _isDefault
                            ? colors.DarkBackground
                            : colors.DarkBackground;
                        break;
                }

                if (Focused && TabStop)
                    borderColor = colors.BlueHighlight;
            }
            else if (ButtonStyle == ModernButtonStyle.Flat)
            {
                switch (ButtonState)
                {
                    case ModernControlState.Normal:
                        fillColor = colors.GreyBackground;
                        break;
                    case ModernControlState.Hover:
                        fillColor = colors.MediumBackground;
                        break;
                    case ModernControlState.Pressed:
                        fillColor = colors.DarkBackground;
                        break;
                }
            }
        }
        else
        {
            textColor = colors.DisabledText;
            fillColor = colors.DarkGreySelection;
        }


        var btnRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        using var btnRectPath = BHelper.GetRoundRectanglePath(btnRect, borderRadius);

        //// use default system style for light mode
        //if (!DarkMode)
        //{
        //    // draw border
        //    if (ButtonStyle == ModernButtonStyle.Normal)
        //    {
        //        using var pen = new Pen(borderColor, DpiApi.Transform(1f));
        //        pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

        //        var borderRect = new RectangleF(
        //            e.ClipRectangle.X + 2, e.ClipRectangle.Y + 2,
        //            e.ClipRectangle.Width - 2, e.ClipRectangle.Height - 2);
        //        using var borderPath = ThemeUtils.GetRoundRectanglePath(borderRect, borderRadius);

        //        g.DrawPath(pen, borderPath);
        //    }

        //    return;
        //}



        // draw background
        using var bgBrush = new SolidBrush(fillColor);
        g.FillPath(bgBrush, btnRectPath);
        

        // draw border
        if (ButtonStyle == ModernButtonStyle.Normal)
        {
            var penWidth = DpiApi.Transform(1f);
            using var pen = new Pen(borderColor, penWidth)
            {
                Alignment = PenAlignment.Outset,
                LineJoin = LineJoin.Round,
            };

            var borderRect = new RectangleF(
                btnRect.X, btnRect.Y,
                btnRect.Width - penWidth, btnRect.Height - penWidth);

            g.DrawRoundedRectangle(pen, borderRect, borderRadius);
        }

        
        var contentRect = new Rectangle(
            Padding.Left,
            Padding.Top,
            Width - Padding.Horizontal,
            Height - Padding.Vertical);

        var textOffsetX = contentRect.Left;
        var textOffsetY = contentRect.Top;

        // draw icon
        if (Image != null)
        {
            var stringSize = g.MeasureString(Text, Font, btnRect.Size);

            var x = (Width / 2) - (Image.Width / 2);
            var y = (Height / 2) - (Image.Height / 2);

            TextImageRelation = TextImageRelation.ImageBeforeText;

            switch (TextImageRelation)
            {
                case TextImageRelation.ImageAboveText:
                    y -= (int)(stringSize.Height / 2) + ImagePadding;
                    textOffsetY = (Image.Height / 2) + ImagePadding;
                    break;

                case TextImageRelation.TextAboveImage:
                    y += (int)(stringSize.Height / 2) + ImagePadding;
                    textOffsetY = ((Image.Height / 2) + ImagePadding) * -1;
                    break;

                case TextImageRelation.ImageBeforeText:
                    x -= (int)stringSize.Width / 2 + ImagePadding;
                    textOffsetX = Padding.Left + Image.Width / 2;
                    break;

                case TextImageRelation.TextBeforeImage:
                    x += (int)stringSize.Width / 2 + ImagePadding;
                    textOffsetX = Padding.Left - Image.Width / 2;
                    break;
            }

            g.DrawImageUnscaled(Image, x, y);
        }


        var textRect = new Rectangle(
            textOffsetX,
            textOffsetY,
            contentRect.Width,
            contentRect.Height);

        var stringFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
        };

        // draw text
        using var textBrush = new SolidBrush(textColor);
        g.DrawString(Text, Font, textBrush, textRect, stringFormat);
    }


    protected override void OnCreateControl()
    {
        base.OnCreateControl();

        var form = FindForm();
        if (form != null)
        {
            if (form.AcceptButton == this)
                _isDefault = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_spacePressed)
            return;

        if (e.Button == MouseButtons.Left)
        {
            if (ClientRectangle.Contains(e.Location))
                SetButtonState(ModernControlState.Pressed);
            else
                SetButtonState(ModernControlState.Hover);
        }
        else
        {
            SetButtonState(ModernControlState.Hover);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!ClientRectangle.Contains(e.Location))
            return;

        SetButtonState(ModernControlState.Pressed);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (_spacePressed)
            return;

        SetButtonState(ModernControlState.Normal);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (_spacePressed)
            return;

        SetButtonState(ModernControlState.Normal);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (_spacePressed)
            return;

        var location = Cursor.Position;

        if (!ClientRectangle.Contains(location))
            SetButtonState(ModernControlState.Normal);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);

        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        _spacePressed = false;

        var location = Cursor.Position;

        if (!ClientRectangle.Contains(location))
            SetButtonState(ModernControlState.Normal);
        else
            SetButtonState(ModernControlState.Hover);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = true;
            SetButtonState(ModernControlState.Pressed);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = false;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetButtonState(ModernControlState.Normal);
            else
                SetButtonState(ModernControlState.Hover);
        }
    }

    public override void NotifyDefault(bool value)
    {
        base.NotifyDefault(value);

        if (!DesignMode)
            return;

        _isDefault = value;
        Invalidate();
    }

    #endregion


}
