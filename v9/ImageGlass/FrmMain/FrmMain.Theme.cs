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


        if (EnableTransparent)
        {
            BackdropMargin = new Padding(-1);
            PicMain.BackColor = Config.BackgroundColor;
        }
        else
        {
            BackColor =
                PicMain.BackColor = Config.BackgroundColor.WithAlpha(255);
        }

        // menu
        MnuMain.CurrentDpi =
            MnuContext.CurrentDpi =
            MnuSubMenu.CurrentDpi = this.DeviceDpi;

        MnuMain.Theme =
           MnuContext.Theme =
           MnuSubMenu.Theme = Config.Theme;

        // toolbar
        Toolbar.EnableTransparent = EnableTransparent;
        Toolbar.Theme = Config.Theme;
        Toolbar.UpdateTheme(this.ScaleToDpi(Config.ToolbarIconHeight));


        // viewer
        PicMain.ForeColor = Config.Theme.Colors.TextColor;
        PicMain.AccentColor = WinColorsApi.GetAccentColor(true);


        // Thumbnail bar
        Gallery.EnableTransparent = EnableTransparent;
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        Gallery.BackColor = Config.Theme.Colors.ThumbnailBarBgColor;


        // navigation buttons
        var navColor = Config.Theme.Colors.ToolbarBgColor;
        PicMain.NavHoveredColor = navColor.WithAlpha(200);
        PicMain.NavPressedColor = navColor.WithAlpha(240);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        Config.Theme.ReloadThemeColors();
        PicMain.AccentColor = e.AccentColor;
        PicMain.Invalidate();

        Invalidate(true);

        // do not handle this event again in the parent class
        e.Handled = true;
        base.OnSystemAccentColorChanged(e);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // theme mode is changed, need to load the corresponding theme pack
        Config.LoadThemePack(e.IsDarkMode, true, true);

        // load the theme icons
        OnDpiChanged();

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);

        base.OnRequestUpdatingColorMode(e);
    }

}

