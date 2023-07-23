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
using System.Diagnostics;
using System.Security.Principal;

namespace ImageGlass.Base;

public class App
{
    /// <summary>
    /// Gets the application executable path
    /// </summary>
    public static string IGExePath => StartUpDir("ImageGlass.exe");


    /// <summary>
    /// Gets the application name
    /// </summary>
    public static string AppName => FileVersionInfo.GetVersionInfo(IGExePath).ProductName;


    /// <summary>
    /// Gets the application name after removing all space characters.
    /// Example: ImageGlass Kobe => ImageGlassKobe
    /// </summary>
    public static string AppNameCode => FileVersionInfo.GetVersionInfo(IGExePath).ProductName.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase);


    /// <summary>
    /// Gets the product version
    /// </summary>
    public static string Version => FileVersionInfo.GetVersionInfo(IGExePath).FileVersion ?? "";


    /// <summary>
    /// Checks if the current user is administator
    /// </summary>
    public static bool IsAdmin => new WindowsPrincipal(WindowsIdentity.GetCurrent())
       .IsInRole(WindowsBuiltInRole.Administrator);


    /// <summary>
    /// Gets value of Portable mode if the startup dir is writable
    /// </summary>
    public static bool IsPortable => BHelper.CheckPathWritable(PathType.Dir, StartUpDir());


    /// <summary>
    /// Gets the path based on the startup folder of ImageGlass.
    /// </summary>
    public static string StartUpDir(params string[] paths)
    {
        var newPaths = paths.ToList();
        newPaths.Insert(0, Application.StartupPath);

        return Path.Combine(newPaths.ToArray());
    }


    /// <summary>
    /// Returns the path based on the configuration folder of ImageGlass.
    /// For portable mode, ConfigDir = Installed Dir, else %LocalAppData%\ImageGlass
    /// </summary>
    /// <param name="type">Indicates if the given path is either file or directory</param>
    public static string ConfigDir(PathType type, params string[] paths)
    {
        // use StartUp dir if it's writable
        var startUpDir = StartUpDir(paths);

        if (BHelper.CheckPathWritable(type, startUpDir))
        {
            return startUpDir;
        }

        // else, use AppData dir
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppNameCode);

        // create the directory if not exists
        Directory.CreateDirectory(appDataDir);

        var newPaths = paths.ToList();
        newPaths.Insert(0, appDataDir);
        appDataDir = Path.Combine(newPaths.ToArray());

        return appDataDir;
    }


}
