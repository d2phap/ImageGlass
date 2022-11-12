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
using System.Windows.Forms;


namespace ImageGlass.UI;


/// <summary>
/// Modern form with dark mode and backdrop support.
/// </summary>
public partial class ModernForm : Form
{
    private bool _isDarkMode = true;
    private WindowBackdrop _backdrop = WindowBackdrop.Mica;


    #region Public properties

    /// <summary>
    /// Enables or disables form dark mode.
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _isDarkMode = value;
            SetDarkMode(value);
        }
    }


    /// <summary>
    /// Gets, sets window backdrop.
    /// </summary>
    public WindowBackdrop WindowBackdrop
    {
        get => _backdrop;
        set
        {
            _backdrop = value;
            SetBackdrop(value);
        }
    }

    #endregion // Public properties


    public ModernForm()
    {
        InitializeComponent();
    }


    protected override void OnHandleCreated(EventArgs e)
    {
        ApplyTheme(_isDarkMode);

        base.OnHandleCreated(e);
    }


    /// <summary>
    /// Apply theme of the window.
    /// </summary>
    public virtual void ApplyTheme(bool darkMode, WindowBackdrop? backDrop = null)
    {
        _isDarkMode = darkMode;
        _backdrop = backDrop ?? _backdrop;

        SetDarkMode(IsDarkMode);
        SetBackdrop(WindowBackdrop);
    }


    // Private functions
    #region Private functions

    /// <summary>
    /// Sets window backdrop.
    /// </summary>
    private void SetBackdrop(WindowBackdrop type)
    {
        if (!BHelper.IsOS(WindowsOS.Win11OrLater)) return;
        var useTransparency = type != WindowBackdrop.None;

        if (useTransparency)
        {
            WindowApi.SetWindowsBackdrop(Handle, (DWM_SYSTEMBACKDROP_TYPE)type);

            TransparencyKey = BackColor;
        }

        AllowTransparency = useTransparency;
    }


    /// <summary>
    /// Sets window dark mode.
    /// </summary>
    private void SetDarkMode(bool enable)
    {
        WindowApi.SetImmersiveDarkMode(Handle, enable);
    }

    #endregion // Private functions

}

