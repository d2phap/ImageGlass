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
using ImageGlass.UI;

namespace ImageGlass.Settings;


/// <summary>
/// A modern color picker dialog with transparency support.
/// </summary>
public partial class ModernColorDialog : DialogForm
{
    /// <summary>
    /// Gets, sets the selected color value.
    /// </summary>
    public Color ColorValue
    {
        get => ColorPicker.ColorValue;
        set => ColorPicker.ColorValue = value;
    }


    /// <summary>
    /// Gets, sets color mode.
    /// </summary>
    public ColorMode ColorMode
    {
        get => ColorPicker.ColorMode;
        set => ColorPicker.ColorMode = value;
    }



    public ModernColorDialog()
    {
        InitializeComponent();
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        // must be before base.OnLoad()
        ApplyLanguage();

        // update form height
        base.OnLoad(e);

        // must be after base.OnLoad()
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();

        ColorPicker.BackColor = Config.Theme.ColorPalatte.AppBg;
        ColorPicker.DarkMode = darkMode;

        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = ColorPicker.Height + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    #endregion // Override / Virtual methods


    private void ApplyLanguage()
    {
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];

        Text = Config.Language["FrmMain.MnuColorPicker"];
    }

}
