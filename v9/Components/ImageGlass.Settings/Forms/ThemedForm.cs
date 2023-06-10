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
using ImageGlass.UI;

namespace ImageGlass.Settings;

/// <summary>
/// Modern form with theme support.
/// </summary>
public class ThemedForm : ModernForm
{
    /// <summary>
    /// Occurs when the system app color is changed and does not match the <see cref="DarkMode"/> value.
    /// </summary>
    public event RequestUpdatingColorModeHandler? RequestUpdatingColorMode;
    public delegate void RequestUpdatingColorModeHandler(SystemColorModeChangedEventArgs e);


    /// <summary>
    /// Occurs when the <see cref="Config.Theme"/> is requested to change.
    /// </summary>
    public static event RequestUpdatingThemeHandler? RequestUpdatingTheme;
    public delegate void RequestUpdatingThemeHandler(RequestUpdatingThemeEventArgs e);


    /// <summary>
    /// Occurs when the <see cref="Config.Language"/> is requested to change.
    /// </summary>
    public static event RequestUpdatingLanguageHandler? RequestUpdatingLanguage;
    public delegate void RequestUpdatingLanguageHandler();


    public ThemedForm() : base()
    {
        Config.RequestUpdatingColorMode += Config_RequestUpdatingColorMode;
        Config.RequestUpdatingTheme += Config_RequestUpdatingTheme;
        Config.RequestUpdatingLanguage += Config_RequestUpdatingLanguage;
    }


    private void Config_RequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(Config_RequestUpdatingColorMode, e);
            return;
        }

        OnRequestUpdatingColorMode(e);
    }

    private void Config_RequestUpdatingTheme(RequestUpdatingThemeEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(Config_RequestUpdatingTheme, e);
            return;
        }

        OnRequestUpdatingTheme(e);
    }


    private void Config_RequestUpdatingLanguage()
    {
        if (InvokeRequired)
        {
            Invoke(Config_RequestUpdatingLanguage);
            return;
        }

        OnRequestUpdatingLanguage();
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    protected virtual void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // theme mode is changed, need to load the corresponding theme pack
        Config.LoadThemePack(e.IsDarkMode, true, true);

        // emits the event
        RequestUpdatingColorMode?.Invoke(e);
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingTheme"/> event.
    /// </summary>
    protected virtual void OnRequestUpdatingTheme(RequestUpdatingThemeEventArgs e)
    {
        RequestUpdatingTheme?.Invoke(e);

        if (e.Handled) return;
        OnDpiChanged();
        ApplyTheme(e.Theme.Settings.IsDarkMode);
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingLanguage"/> event.
    /// </summary>
    protected virtual void OnRequestUpdatingLanguage()
    {
        RequestUpdatingLanguage?.Invoke();
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        Config.RequestUpdatingColorMode -= Config_RequestUpdatingColorMode;
    }

}
