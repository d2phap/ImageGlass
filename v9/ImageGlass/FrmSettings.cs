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
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Diagnostics;

namespace ImageGlass;

public partial class FrmSettings : ModernForm
{
    public FrmSettings()
    {
        InitializeComponent();

        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        lblSettingsFilePath.Text = path;

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        EnableTransparent = darkMode;

        if (!EnableTransparent)
        {
            BackColor = Config.Theme.ColorPalatte.AppBackground;
        }

        base.ApplyTheme(darkMode, style);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);

        base.OnRequestUpdatingColorMode(e);
    }


    private void btnOpenSettingsFile_Click(object sender, EventArgs e)
    {
        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true,
        };

        using var proc = Process.Start(psi);
    }
}
