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
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Theme contains methods for dynamic binding     *
 * ****************************************************** */

public partial class FrmMain
{
    protected override void ApplyTheme(bool darkMode, BackdropStyle? backDrop = null)
    {
        var isDarkMode = Config.Theme.Settings.IsDarkMode;
        var backdrop = BackdropStyle.None;

        if (Config.WindowBackdrop != BackdropStyle.None)
        {
            backdrop = Config.WindowBackdrop;
            BackdropMargin = new Padding(-1); // TODO: load from config / theme
        }


        // toolbar
        Toolbar.Theme =
            MnuMain.Theme =
            MnuContext.Theme =
            MnuSubMenu.Theme = Config.Theme;
        Toolbar.UpdateTheme(DpiApi.Transform(Config.ToolbarIconHeight));


        // viewer
        Sp1.BackColor = Sp2.BackColor = Color.Transparent;
        PicMain.BackColor = Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Settings.TextColor;
        PicMain.SelectionColor = Config.Theme.Settings.AccentColor;


        // Thumbnail bar
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        Sp1.SplitterBackColor =
            Sp2.SplitterBackColor =
            Gallery.BackColor = Config.Theme.Settings.ThumbnailBarBgColor;


        // navigation buttons
        var navColor = Config.Theme.Settings.ToolbarBgColor;
        PicMain.NavHoveredColor = navColor.WithAlpha(200);
        PicMain.NavPressedColor = navColor.WithAlpha(240);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

        ResumeLayout(false);

        base.ApplyTheme(isDarkMode, backdrop);
    }

}

