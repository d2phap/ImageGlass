/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.UI;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types;


[JsonSerializable(typeof(EditingApp))]
public partial class EditingAppJsonContext : JsonSerializerContext { }


/// <summary>
/// Contains app information for editing the viewing image.
/// </summary>
/// <param name="appName">Friendly app name.</param>
/// <param name="executable">Executable command. Ex: <c>C:\app\app.exe</c></param>
/// <param name="argument">Argument to pass to the <paramref name="executable"/>. Ex: <c>--help</c></param>
public class EditingApp
{
    /// <summary>
    /// The extension key that matches all file extensions (catch-all).
    /// </summary>
    public const string ALL_EXTENSIONS = ".*";


    /// <summary>
    /// Gets, sets friendly app name.
    /// </summary>
    public string AppName { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets full path of app.
    /// </summary>
    public string Executable { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets argument of app.
    /// </summary>
    public string Argument { get; set; } = string.Empty;


    public EditingApp() { }


    public EditingApp(string appName = "", string executable = "", string argument = "")
    {
        AppName = appName;
        Executable = executable;
        Argument = argument;
    }



    /// <summary>
    /// Gets an editing app from the given extension. An exact extension match wins; a
    /// <c>.*</c> (all-extensions) entry is used as a fallback.
    /// </summary>
    /// <param name="ext">An extension. E.g. <c>.jpg</c></param>
    public static EditingApp? GetFromExtension(string? ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return null;

        EditingApp? wildcardApp = null;

        foreach (var (key, app) in Core.Config.EditApps)
        {
            if (app is null) continue;

            var exts = key.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (exts.Contains(ext)) return app;                  // exact extension match wins
            if (exts.Contains(ALL_EXTENSIONS)) wildcardApp = app; // remember the catch-all as a fallback
        }

        return wildcardApp;
    }


    /// <summary>
    /// Updates the app name of menu edit.
    /// </summary>
    public static void UpdateAppNameForMenuEdit(PhMenuItem mnuEdit)
    {
        var appName = string.Empty;
        if (Core.ClipboardImage is null)
        {
            var ext = Core.Photos.Current?.Extension;
            if (GetFromExtension(ext) is EditingApp app)
            {
                appName = $"({app.AppName})";
            }
            else if (BHelper.OS == OSType.Windows)
            {
                appName = "(Microsoft Paint)";
            }
        }

        mnuEdit.LangParams = appName;
    }

}



/// <summary>
/// Actions after opening editing app.
/// </summary>
public enum AfterEditAppAction
{
    Nothing = 0,
    Minimize = 1,
    Close = 2,
}
