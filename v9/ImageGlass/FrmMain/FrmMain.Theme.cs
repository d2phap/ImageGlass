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
using ImageGlass.UI;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Theme contains methods for dynamic binding     *
 * ****************************************************** */

public partial class FrmMain
{
    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();
        style ??= Config.WindowBackdrop;
        var hasTransparency = EnableTransparent && style.Value != BackdropStyle.None;


        if (hasTransparency)
        {
            BackdropMargin = new Padding(-1);
        }
        else
        {
            BackColor = Config.Theme.ColorPalatte.AppBg.NoAlpha();
        }

        // menu
        MnuMain.CurrentDpi =
            MnuContext.CurrentDpi =
            MnuSubMenu.CurrentDpi = this.DeviceDpi;

        MnuMain.Theme =
           MnuContext.Theme =
           MnuSubMenu.Theme = Config.Theme;


        // toolbar
        Toolbar.EnableTransparent = hasTransparency;
        Toolbar.Theme = Config.Theme;
        Toolbar.UpdateTheme(this.ScaleToDpi(Config.ToolbarIconHeight));


        // toolbar context
        ToolbarContext.EnableTransparent = hasTransparency;
        ToolbarContext.Theme = Config.Theme;
        ToolbarContext.UpdateTheme(this.ScaleToDpi(Config.ToolbarIconHeight));


        // viewer
        PicMain.EnableTransparent = hasTransparency;
        PicMain.BackColor = Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Colors.TextColor;
        PicMain.AccentColor = WinColorsApi.GetAccentColor(true);
        PicMain.NavButtonColor = Config.Theme.Colors.NavigationButtonColor;
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

        PicMain.Web2DarkMode = darkMode;
        PicMain.Web2NavLeftImagePath = Config.Theme.NavLeftImagePath;
        PicMain.Web2NavRightImagePath = Config.Theme.NavRightImagePath;


        // Thumbnail bar
        Gallery.EnableTransparent = hasTransparency;
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        Gallery.BackColor = Config.Theme.Colors.GalleryBgColor;
        Gallery.Tooltip = new ModernTooltip() { DarkMode = darkMode };


        // set app logo on titlebar
        _ = Config.UpdateFormIcon(this);

        // update webview2 styles
        if (PicMain.UseWebview2) PicMain.UpdateWeb2Styles(darkMode);

        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        Config.Theme.LoadThemeColors();
        PicMain.AccentColor = e.AccentColor;
        PicMain.NavButtonColor = Config.Theme.Colors.NavigationButtonColor;
        PicMain.Invalidate();

        Invalidate(true);

        // do not handle this event again in the parent class
        e.Handled = true;
        base.OnSystemAccentColorChanged(e);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        base.OnRequestUpdatingColorMode(e);

        // load the theme icons
        OnDpiChanged();

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }

}

