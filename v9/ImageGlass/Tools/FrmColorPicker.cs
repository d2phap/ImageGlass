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
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmColorPicker : ToolForm, IToolForm<ColorPickerConfig>
{

    public string ToolId => "ColorPicker";
    public ColorPickerConfig Settings { get; set; }


    public FrmColorPicker() : base()
    {
        InitializeComponent();
    }


    public FrmColorPicker(Form owner) : base()
    {
        InitializeComponent();
        if (DesignMode) return;

        Owner = owner;
        Settings = new(ToolId);

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }



    // Override methods
    #region Override methods

    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();


        // show backdrop effect for title and footer
        BackdropMargin = new Padding(0);

        if (!EnableTransparent)
        {
            BackColor = Config.Theme.ColorPalatte.AppBackground;
        }

        TableLayout.BackColor = Config.Theme.ColorPalatte.AppBackground;

        base.ApplyTheme(darkMode, style);

        TxtLocation.BackColor =
            TxtRgb.BackColor =
            TxtHex.BackColor =
            TxtCmyk.BackColor =
            TxtHsl.BackColor =
            TxtHsv.BackColor = Config.Theme.ColorPalatte.AppBackground;

        ResumeLayout();
    }


    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);

        OnUpdateHeight();
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void OnLoad(EventArgs e)
    {
        if (DesignMode)
        {
            base.OnLoad(e);
            return;
        }

        // load tool configs
        Settings.LoadFromAppConfig();
        ApplySettings();

        // add control events
        Local.FrmMain.PicMain.ImageMouseMove += PicMain_ImageMouseMove;
        Local.FrmMain.PicMain.ImageMouseClick += PicMain_ImageMouseClick;


        // set default location offset on the parent form
        var padding = DpiApi.Transform(10);
        var x = padding;
        var y = DpiApi.Transform(SystemInformation.CaptionHeight + Constants.TOOLBAR_ICON_HEIGHT * 2) + padding;
        InitLocation = new Point(x, y);

        base.OnLoad(e);

        ApplyLanguage();
    }

    private void PicMain_ImageMouseMove(object? sender, Views.ImageMouseEventArgs e)
    {
        if (e.Button != MouseButtons.None) return;

        if (e.ImageX < 0 || e.ImageY < 0
            || e.ImageX > Local.FrmMain.PicMain.SourceWidth
            || e.ImageY > Local.FrmMain.PicMain.SourceHeight)
        {
            LblCursorLocation.Text = string.Empty;
        }
        else
        {
            LblCursorLocation.Text = $"({(int)e.ImageX}, {(int)e.ImageY})";
        }
    }

    private void PicMain_ImageMouseClick(object? sender, Views.ImageMouseEventArgs e)
    {
        if (e.ImageX < 0 || e.ImageY < 0
            || e.ImageX > Local.FrmMain.PicMain.SourceWidth
            || e.ImageY > Local.FrmMain.PicMain.SourceHeight) return;

        ShowPickedColor((int)e.ImageX, (int)e.ImageY);
    }

    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);

        // calculate form height
        var contentHeight = TableLayout.Height + TableLayout.Padding.Vertical;
        var formHeight = contentHeight + baseHeight;

        if (performUpdate)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnToolFormClosing(ToolFormClosingEventArgs e)
    {
        base.OnToolFormClosing(e);

        // remove events
        Local.FrmMain.PicMain.ImageMouseMove -= PicMain_ImageMouseMove;
        Local.FrmMain.PicMain.ImageMouseClick -= PicMain_ImageMouseClick;


        // save settings
        Settings.SaveToAppConfig();
        Local.FrmMain.Activate();
    }



    #endregion // Override methods


    // Private methods
    #region Private methods

    private void ApplyLanguage()
    {
        Text = Config.Language[$"{nameof(Local.FrmMain)}.{nameof(Local.FrmMain.MnuColorPicker)}"];

        TooltipMain.SetToolTip(BtnSettings, Config.Language[$"{Name}.{nameof(BtnSettings)}._Tooltip"]);
        TooltipMain.SetToolTip(BtnCopyLocation, Config.Language[$"_.Copy"]);
        TooltipMain.SetToolTip(BtnCopyRgb, Config.Language[$"_.Copy"]);
        TooltipMain.SetToolTip(BtnCopyHex, Config.Language[$"_.Copy"]);
        TooltipMain.SetToolTip(BtnCopyCmyk, Config.Language[$"_.Copy"]);
        TooltipMain.SetToolTip(BtnCopyHsl, Config.Language[$"_.Copy"]);
        TooltipMain.SetToolTip(BtnCopyHsv, Config.Language[$"_.Copy"]);
    }


    private void ApplySettings()
    {
        LblRgb.Text = Settings.ShowRgbWithAlpha ? "RGBA:" : "RGB:";
        LblHex.Text = Settings.ShowHexWithAlpha ? "HEXA:" : "HEX:";
        LblHsl.Text = Settings.ShowHslWithAlpha ? "HSLA:" : "HSL:";
        LblHsv.Text = Settings.ShowHsvWithAlpha ? "HSVA:" : "HSV:";

        LblCmyk.Text = "CMYK:";
        LblLocation.Text = "X, Y:";
    }


    private void ShowPickedColor(int x, int y)
    {
        TxtLocation.Text = $"{x}, {y}";
    }

    #endregion // Private methods



}
