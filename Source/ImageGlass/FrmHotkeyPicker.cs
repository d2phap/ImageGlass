/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

public partial class FrmHotkeyPicker : DialogForm
{
    private Keys _inputKeys = Keys.None;


    /// <summary>
    /// Gets the return hotkey.
    /// </summary>
    public Hotkey HotkeyValue => new Hotkey(_inputKeys);


    public FrmHotkeyPicker()
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

        AcceptButton = null;
        TxtHotkey.Select();
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();

        LblHotkey.DarkMode = DarkMode;
        TableLayout.BackColor = Config.Theme.ColorPalatte.AppBg;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);
        var formHeight = TableLayout.Height + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // allow all keys to be captured
        e.SuppressKeyPress = false;
    }


    #endregion // Override / Virtual methods


    private void ApplyLanguage()
    {
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];

        LblHotkey.Text = Config.Language[$"{Name}.{nameof(LblHotkey)}"];
    }


    private void TxtHotkey_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Tab) e.SuppressKeyPress = true;

        _inputKeys = e.KeyData;
        TxtHotkey.Text = new Hotkey(_inputKeys).ToString();
    }
}
