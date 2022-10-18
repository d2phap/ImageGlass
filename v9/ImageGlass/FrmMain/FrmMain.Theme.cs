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
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Theme contains methods for dynamic binding     *
 * ****************************************************** */

public partial class FrmMain
{

    public void SetUpFrmMainTheme()
    {
        UpdateTheme();
    }


    private void UpdateTheme(SystemThemeMode mode = SystemThemeMode.Unknown)
    {
        var themMode = mode;
        
        if (mode == SystemThemeMode.Unknown)
        {
            themMode = ThemeUtils.GetSystemThemeMode();
        }

        // correct theme mode
        var isDarkMode = themMode != SystemThemeMode.Light;


        // toolbar
        Toolbar.Theme =
            MnuMain.Theme =
            MnuContext.Theme =
            MnuSubMenu.Theme = Config.Theme;
        Toolbar.UpdateTheme(DpiApi.Transform(Config.ToolbarIconHeight));

        // background
        BackColor = Sp1.BackColor = Sp2.BackColor = Config.BackgroundColor;
        PicMain.BackColor = Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Settings.TextColor;
        PicMain.SelectionColor = Config.Theme.Settings.AccentColor;

        // Thumbnail bar
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        Sp1.SplitterBackColor =
            Gallery.BackColor = Config.Theme.Settings.ThumbnailBarBgColor;

        // Side panels
        Sp2.SplitterBackColor = Config.Theme.Settings.ThumbnailBarBgColor;

        // navigation buttons
        PicMain.NavHoveredColor = Color.FromArgb(200, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavPressedColor = Color.FromArgb(240, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

        Config.ApplyFormTheme(this, Config.Theme);
    }

}

