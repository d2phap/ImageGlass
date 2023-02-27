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

namespace ImageGlass;

public partial class FrmColorPickerSettings : DialogForm
{
    public ColorPickerConfig Settings { get; init; }


    public FrmColorPickerSettings(ColorPickerConfig settings) : base()
    {
        InitializeComponent();
        Settings = settings;
    }


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
        ApplyLanguage();
        LoadSettings();


        var workingArea = Screen.FromControl(this).WorkingArea;
        if (Bottom > workingArea.Bottom) Top = workingArea.Bottom - Height;
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();


        TableTop.BackColor = Config.Theme.ColorPalatte.AppBackground;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = TableTop.Height + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnAcceptButtonClicked()
    {
        base.OnAcceptButtonClicked();
        ApplySettings();
    }


    #endregion // Override / Virtual methods



    // Private methods
    #region Private methods

    private void ApplyLanguage()
    {
        Text = Config.Language[$"{Name}._Title"];
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];
        BtnApply.Text = Config.Language["_._Apply"];


        ChkShowRgbA.Text = Config.Language[$"{Name}.{nameof(ChkShowRgbA)}"];
        ChkShowHexA.Text = Config.Language[$"{Name}.{nameof(ChkShowHexA)}"];
        ChkShowHslA.Text = Config.Language[$"{Name}.{nameof(ChkShowHslA)}"];
        ChkShowHsvA.Text = Config.Language[$"{Name}.{nameof(ChkShowHsvA)}"];
    }


    private void LoadSettings()
    {
        ChkShowRgbA.Checked = Settings.ShowRgbWithAlpha;
        ChkShowHexA.Checked = Settings.ShowHexWithAlpha;
        ChkShowHslA.Checked = Settings.ShowHslWithAlpha;
        ChkShowHsvA.Checked = Settings.ShowHsvWithAlpha;
    }


    private void ApplySettings()
    {
        Settings.ShowRgbWithAlpha = ChkShowRgbA.Checked;
        Settings.ShowHexWithAlpha = ChkShowHexA.Checked;
        Settings.ShowHslWithAlpha = ChkShowHslA.Checked;
        Settings.ShowHsvWithAlpha = ChkShowHsvA.Checked;
    }


    #endregion // Private methods


}
