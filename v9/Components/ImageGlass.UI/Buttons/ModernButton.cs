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
    // Private variables
    #region Private variables

    private ModernButtonStyle _style = ModernButtonStyle.Normal;
    private ModernControlState _buttonState = ModernControlState.Normal;

    private bool _isDefault;
    private bool _spacePressed;
    private bool _darkMode = false;

    private static readonly int _padding = 4;
    private int _imagePadding = 2;

    private SHSTOCKICONID? _systemIcon = null;
    private IconName _svgIcon = IconName.None;

    #endregion // Private variables


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
            UpdateSvgImage(_darkMode);
            Invalidate();
        }
    }


    /// <summary>
    /// Gets, sets the system icon.
    /// </summary>
    [DefaultValue(false)]
    public SHSTOCKICONID? SystemIcon
    {
        get => _systemIcon;
        set
        {
            _systemIcon = value;

            Image?.Dispose();
            Image = SystemIconApi.GetSystemIcon(_systemIcon, false);
            Invalidate();
        }
    }


    /// <summary>
    /// Gets, sets the SVG icon.
    /// </summary>
    [DefaultValue(IconName.None)]
    public IconName SvgIcon
    {
        get => _svgIcon;
        set
        {
            _svgIcon = value;
            Image?.Dispose();
            Image = null;

            if (_svgIcon != IconName.None)
            {
                _systemIcon = null;
            }

            UpdateSvgImage(DarkMode);
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

    public static new Padding DefaultPadding => new Padding(_padding);


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

    #endregion // Public properties


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


    // Override methods
    #region Override methods

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        UpdateSvgImage(_darkMode);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Image?.Dispose();
        Image = null;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;


        var colors = BHelper.GetThemeColorPalatte(DarkMode, DesignMode);
        var accentColor = colors.Accent;
        var isCTAStyle = _isDefault || ButtonStyle == ModernButtonStyle.CTA;


        var textColor = colors.AppText;
        var borderColor = colors.ControlBorder;
        var borderRadius = BHelper.IsOS(WindowsOS.Win11OrLater) ? this.ScaleToDpi(2.5f) : 0;
        var fillColor = isCTAStyle
            ? colors.ControlBgAccent
            : colors.ControlBg;


        if (Enabled)
        {
            // Normal style
            switch (ButtonState)
            {
                case ModernControlState.Hover:
                    fillColor = isCTAStyle
                        ? colors.ControlBgAccentHover
                        : colors.ControlBgHover;
                    borderColor = borderColor.WithBrightness(DarkMode ? 0.3f : -0.1f);
                    break;

                case ModernControlState.Pressed:
                    fillColor = colors.ControlBgPressed;
                    break;
            }

            if (Focused && TabStop)
                borderColor = colors.ControlBorderAccent;


            // Accent style
            if (ButtonStyle == ModernButtonStyle.Accent)
            {
                fillColor = fillColor.Blend(accentColor, DarkMode ? 0.7f : 0.8f);
                borderColor = borderColor.WithAlpha(120);
            }

            // Flat style
            else if (ButtonStyle == ModernButtonStyle.Flat)
            {
                if (ButtonState == ModernControlState.Pressed)
                {
                    fillColor = colors.ControlBgPressed2;
                }

                borderColor = fillColor;
            }
        }
        else
        {
            textColor = colors.AppTextDisabled;
            fillColor = colors.ControlBgDisabled;
        }


        // draw background
        var btnRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
        using var bgBrush = new SolidBrush(fillColor);
        g.FillRoundedRectangle(bgBrush, btnRect, borderRadius);


        // draw border
        var penWidth = this.ScaleToDpi(1.1f);
        using var pen = new Pen(borderColor, penWidth)
        {
            Alignment = PenAlignment.Center,
            LineJoin = LineJoin.Round,
        };

        var borderRect = new RectangleF(
            btnRect.X + penWidth / 3,
            btnRect.Y + penWidth / 3,
            btnRect.Width - penWidth, btnRect.Height - penWidth);

        g.DrawRoundedRectangle(pen, borderRect, borderRadius);


        // draw content
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

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);

        if (_svgIcon != IconName.None)
        {
            UpdateSvgImage(DarkMode);
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

    #endregion // Override methods


    private void SetButtonState(ModernControlState buttonState)
    {
        if (_buttonState != buttonState)
        {
            _buttonState = buttonState;
            Invalidate();
        }
    }


    /// <summary>
    /// Updates the button image using <see cref="SvgIcon"/>.
    /// </summary>
    public void UpdateSvgImage(bool darkMode)
    {
        var size = Height - Padding.Vertical * 2;
        if (DesignMode && _svgIcon != IconName.None)
        {
            Image = BHelper.CreateDefaultToolbarIcon(size, darkMode);
            return;
        }

        if (SystemIcon == null)
        {
            var svgPath = IconFile.GetFullPath(_svgIcon);
            Image = BHelper.ToGdiPlusBitmapFromSvg(svgPath, darkMode, size, size);
        }
    }

}
