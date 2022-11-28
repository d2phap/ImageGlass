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


namespace ImageGlass.UI;

/// <summary>
/// Modern form with dark mode and backdrop support.
/// </summary>
public partial class ModernForm : Form
{
    private bool _isDarkMode = true;
    private BackdropStyle _backdropStyle = BackdropStyle.Mica;
    private Padding _backdropMargin = new(-1);
    private int _dpi = DpiApi.DPI_DEFAULT;


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
    public BackdropStyle BackdropStyle
    {
        get => _backdropStyle;
        set
        {
            _backdropStyle = value;
            SetBackdrop(value);
        }
    }


    /// <summary>
    /// Gets, sets the backdrop margin.
    /// </summary>
    public Padding BackdropMargin
    {
        get => _backdropMargin;
        set
        {
            _backdropMargin = value;
            _ = WindowApi.SetWindowFrame(Handle, _backdropMargin);
        }
    }


    /// <summary>
    /// Gets, sets the keys to close the <see cref="ModernForm"/>.
    /// </summary>
    public Keys CloseFormHotkey { get; set; } = Keys.None;


    /// <summary>
    /// Gets the current DPI. Default value is <c>96</c>.
    /// </summary>
    public int Dpi => _dpi;


    /// <summary>
    /// Gets the current DPI scaling. Default value is <c>1.0f</c>.
    /// </summary>
    public float DpiScale => _dpi / 96;


    /// <summary>
    /// Gets, sets the value indicates that <see cref="DpiApi.CurrentDpi"/> should be updated when form DPI is changed.
    /// </summary>
    public bool EnableDpiApiUpdate { get; set; } = false;


    /// <summary>
    /// Occurs when the window is being maximized.
    /// </summary>
    public event WindowMaximizingHandler? OnWindowMaximizing;
    public delegate void WindowMaximizingHandler(EventArgs e);


    /// <summary>
    /// Occurs when the window is being restored from maximize state.
    /// </summary>
    public event WindowRestoringHandler? OnWindowRestoring;
    public delegate void WindowRestoringHandler(EventArgs e);

    #endregion // Public properties


    /// <summary>
    /// Initializes the new instance of <see cref="ModernForm"/>.
    /// </summary>
    public ModernForm()
    {
        InitializeComponent();
        SizeGripStyle = SizeGripStyle.Hide;


        _dpi = DeviceDpi;
    }


    // Protected / virtual functions
    #region Protected / virtual functions

    protected override void OnHandleCreated(EventArgs e)
    {
        if (!DesignMode)
        {
            ApplyTheme(_isDarkMode);
        }

        base.OnHandleCreated(e);
    }


    protected override void WndProc(ref Message m)
    {
        // WM_SYSCOMMAND
        if (m.Msg == 0x0112)
        {
            // When user clicks on MAXIMIZE button on title bar
            if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
            {
                // The window is being maximized
                OnWindowMaximizing?.Invoke(EventArgs.Empty);
            }
            // When user clicks on the RESTORE button on title bar
            else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
            {
                // The window is being restored
                OnWindowRestoring?.Invoke(EventArgs.Empty);
            }
        }
        else if (m.Msg == DpiApi.WM_DPICHANGED)
        {
            // get new dpi value
            _dpi = (short)m.WParam;

            OnDpiChanged();
        }

        base.WndProc(ref m);
    }


    /// <summary>
    /// Occurs when window's DPI is changed.
    /// </summary>
    protected virtual void OnDpiChanged()
    {
        if (EnableDpiApiUpdate)
        {
            DpiApi.CurrentDpi = _dpi;
        }
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!DesignMode
            && _backdropStyle != BackdropStyle.None
            && BackdropMargin.Vertical == 0 && BackdropMargin.Horizontal == 0)
        {
            WindowApi.SetTransparentBlackBackground(e.Graphics, Bounds);
        }
    }


    /// <summary>
    /// Apply theme of the window.
    /// </summary>
    protected virtual void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        _isDarkMode = darkMode;
        _backdropStyle = style ?? _backdropStyle;

        SetDarkMode(IsDarkMode);
        SetBackdrop(BackdropStyle);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // disable parent form shotcuts
        return false;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (CloseFormHotkey != Keys.None && e.KeyData.Equals(CloseFormHotkey))
        {
            CloseFormByKeys();
        }
    }


    /// <summary>
    /// Closes the window when <see cref="CloseFormHotkey"/> is pressed.
    /// </summary>
    protected virtual void CloseFormByKeys()
    {
        Close();
    }

    #endregion // Protected / virtual functions


    // Private functions
    #region Private functions

    /// <summary>
    /// Sets window backdrop.
    /// </summary>
    private void SetBackdrop(BackdropStyle style)
    {
        var backupBgColor = BackColor;
        if (style != BackdropStyle.None)
        {
            // back color must be black
            BackColor = Color.Black;
        }

        // set backdrop style
        var succeeded = WindowApi.SetWindowBackdrop(Handle, (DWM_SYSTEMBACKDROP_TYPE)style);
        var margin = (succeeded && style != BackdropStyle.None)
            ? BackdropMargin
            : new Padding(0);

        if (!succeeded)
        {
            BackColor = backupBgColor;
        }

        // set window frame
        _ = WindowApi.SetWindowFrame(Handle, _backdropMargin);
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

