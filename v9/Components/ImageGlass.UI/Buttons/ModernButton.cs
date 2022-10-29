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
        // use default system style for light mode
        if (!DarkMode)
        {
            base.OnPaint(e);
            return;
        }

        
        var g = e.Graphics;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;


        var textColor = DarkColors.LightText;
        var borderColor = DarkColors.GreySelection;
        var fillColor = _isDefault
            ? DarkColors.DarkBlueBackground
            : DarkColors.LightBackground;

        var borderRadius = 0f;
        if (BHelper.IsOS(WindowsOS.Win11))
        {
            borderRadius = Height / 6f;
        }

        if (Enabled)
        {
            if (ButtonStyle == ModernButtonStyle.Normal)
            {
                if (Focused && TabStop)
                    borderColor = DarkColors.BlueHighlight;

                switch (ButtonState)
                {
                    case ModernControlState.Hover:
                        fillColor = _isDefault
                            ? DarkColors.BlueBackground
                            : DarkColors.LighterBackground;
                        break;
                    case ModernControlState.Pressed:
                        fillColor = _isDefault
                            ? DarkColors.DarkBackground
                            : DarkColors.DarkBackground;
                        break;
                }
            }
            else if (ButtonStyle == ModernButtonStyle.Flat)
            {
                switch (ButtonState)
                {
                    case ModernControlState.Normal:
                        fillColor = DarkColors.GreyBackground;
                        break;
                    case ModernControlState.Hover:
                        fillColor = DarkColors.MediumBackground;
                        break;
                    case ModernControlState.Pressed:
                        fillColor = DarkColors.DarkBackground;
                        break;
                }
            }
        }
        else
        {
            textColor = DarkColors.DisabledText;
            fillColor = DarkColors.DarkGreySelection;
        }


        // draw background
        var parentBgRect = new Rectangle(-1, -1, ClientSize.Width + 1, ClientSize.Height + 1);
        using var parentBgBrush = new SolidBrush(Parent.BackColor);
        g.FillRectangle(parentBgBrush, parentBgRect);

        var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        using var bgPath = ThemeUtils.GetRoundRectanglePath(rect, borderRadius);
        using var bgBrush = new SolidBrush(fillColor);
        g.FillPath(bgBrush, bgPath);
        

        // draw border
        if (ButtonStyle == ModernButtonStyle.Normal)
        {
            var borderRect = new Rectangle(
                rect.Left, rect.Top,
                rect.Width - 1, rect.Height - 1);

            using var borderPath = ThemeUtils.GetRoundRectanglePath(borderRect, borderRadius);
            using var pen = new Pen(borderColor, 1);

            g.DrawPath(pen, borderPath);
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
            var stringSize = g.MeasureString(Text, Font, rect.Size);

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
