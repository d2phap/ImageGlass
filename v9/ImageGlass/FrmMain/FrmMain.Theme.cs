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
    public override void ApplyTheme(bool darkMode, WindowBackdrop? backDrop = null)
    {
        //var themMode = mode;

        //if (mode == SystemThemeMode.Unknown)
        //{
        //    themMode = ThemeUtils.GetSystemThemeMode();
        //}

        //// correct theme mode
        //var isDarkMode = themMode != SystemThemeMode.Light;

        var isDarkMode = Config.Theme.Settings.IsDarkMode;
        var backdrop = WindowBackdrop.Default;

        var isViewerTransparent = Config.BackgroundColor.A != 255;
        var isToolbarTransparent = Config.Theme.Settings.ToolbarBgColor.A != 255;
        var isGalleryTransparent = Config.Theme.Settings.ThumbnailBarBgColor.A != 255;
        var isTransparent = isViewerTransparent || isToolbarTransparent || isGalleryTransparent;
        if (isTransparent)
        {
            backdrop = WindowBackdrop.Mica;
        }


        // toolbar
        Toolbar.Theme =
            MnuMain.Theme =
            MnuContext.Theme =
            MnuSubMenu.Theme = Config.Theme;
        Toolbar.UpdateTheme(DpiApi.Transform(Config.ToolbarIconHeight));


        // viewer
        BackColor = Sp1.BackColor = Sp2.BackColor = isTransparent
            ? Config.Theme.ColorPalatte.GreyBackground : Config.BackgroundColor;

        PicMain.BackColor = isViewerTransparent ? BackColor : Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Settings.TextColor;
        PicMain.SelectionColor = Config.Theme.Settings.AccentColor;


        // Thumbnail bar
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        var galleryBackColor = Config.Theme.Settings.ThumbnailBarBgColor;
        if (isGalleryTransparent)
        {
            galleryBackColor = BackColor;
        }
        Sp1.SplitterBackColor = Sp2.SplitterBackColor = Gallery.BackColor = galleryBackColor;


        // navigation buttons
        var navColor = Config.Theme.Settings.ToolbarBgColor;
        if (isToolbarTransparent)
        {
            navColor = Config.Theme.ColorPalatte.DarkBackground;
        }
        PicMain.NavHoveredColor = navColor.WithAlpha(200);
        PicMain.NavPressedColor = navColor.WithAlpha(240);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

        Config.ApplyFormTheme(this, Config.Theme);

        ResumeLayout(false);

        base.ApplyTheme(isDarkMode, backdrop);
    }

}

