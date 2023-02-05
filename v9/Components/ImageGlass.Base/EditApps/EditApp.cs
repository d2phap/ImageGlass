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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace ImageGlass.Base;

/// <summary>
/// Contains app information for editing the viewing image.
/// </summary>
public class EditApp
{
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


    /// <summary>
    /// Initialize new <see cref="EditApp"/> instance.
    /// </summary>
    public EditApp()
    {
    }


    /// <summary>
    /// Initialize new <see cref="EditApp"/> instance.
    /// </summary>
    /// <param name="appName">Friendly app name.</param>
    /// <param name="executable">Executable command. Ex: <c>C:\app\app.exe</c></param>
    /// <param name="argument">Argument to pass to the <paramref name="executable"/>. Ex: <c>--help</c></param>
    public EditApp(string appName, string executable, string argument = "")
    {
        AppName = appName;
        Executable = executable;
        Argument = argument;
    }

}
