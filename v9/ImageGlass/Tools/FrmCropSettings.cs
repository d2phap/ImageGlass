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

namespace ImageGlass;

public partial class FrmCropSettings : DialogForm
{
    public CropToolConfig Settings { get; init; }


    public FrmCropSettings(CropToolConfig settings) : base()
    {
        InitializeComponent();
        Settings = settings;

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // update theme here
        ApplyTheme(e.IsDarkMode);

        base.OnRequestUpdatingColorMode(e);
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var contentHeight = tableTop.Height + tableTop.Padding.Vertical;

        if (performUpdate)
        {
            Height = contentHeight + baseHeight;
        }

        return contentHeight;
    }

    #endregion // Override / Virtual methods



}
