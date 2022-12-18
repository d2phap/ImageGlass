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
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

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
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string StartUpDir(params string[] paths)
    {
        var binaryPath = typeof(App).Assembly.Location;
        var path = Path.GetDirectoryName(binaryPath) ?? "";

        var newPaths = paths.ToList();
        newPaths.Insert(0, path);


        return Path.Combine(newPaths.ToArray());
    }


    /// <summary>
    /// Returns the path based on the configuration folder of ImageGlass.
    /// For portable mode, ConfigDir = Installed Dir, else %appdata%\ImageGlass
    /// </summary>
    /// <param name="type">Indicates if the given path is either file or directory</param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string ConfigDir(PathType type, params string[] paths)
    {
        // use StartUp dir if it's writable
        var startUpDir = StartUpDir(paths);

        if (BHelper.CheckPathWritable(type, startUpDir))
        {
            return startUpDir;
        }

        // else, use AppData dir
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppNameCode);

        // create the directory if not exists
        Directory.CreateDirectory(appDataDir);

        var newPaths = paths.ToList();
        newPaths.Insert(0, appDataDir);
        appDataDir = Path.Combine(newPaths.ToArray());

        return appDataDir;
    }


    /// <summary>
    /// Center the given form to the current screen.
    /// Note***: The method Form.CenterToScreen() contains a bug:
    /// https://stackoverflow.com/a/6837499/2856887
    /// </summary>
    /// <param name="form">The form to center</param>
    public static void CenterFormToScreen(Form form)
    {
        var screen = Screen.FromControl(form);

        var workingArea = screen.WorkingArea;
        form.Location = new Point()
        {
            X = Math.Max(workingArea.X, workingArea.X + ((workingArea.Width - form.Width) / 2)),
            Y = Math.Max(workingArea.Y, workingArea.Y + ((workingArea.Height - form.Height) / 2))
        };
    }


    /// <summary>
    /// Register file type associations and app capabilities to registry
    /// </summary>
    /// <param name="extensions">Extension string, ex: *.png;*.svg;</param>
    public static (bool IsSuccessful, StringBuilder Keys) RegisterAppAndExtensions(string extensions)
    {
        var appName = "ImageGlass";
        var keys = new StringBuilder();
        _ = UnregisterAppAndExtensions(extensions);

        var reg = new RegistryEx
        {
            ShowError = false,
            BaseRegistryKey = Registry.LocalMachine,

            // Register the application to Registry
            SubKey = @"SOFTWARE\RegisteredApplications"
        };
        
        if (!reg.Write(appName, $@"SOFTWARE\{appName}\Capabilities"))
        {
            keys.AppendLine($@"{reg.FullKey}\{appName}");
        }

        // Register Capabilities info
        reg.SubKey = $@"SOFTWARE\{appName}\Capabilities";
        if (!reg.Write("ApplicationName", App.AppName))
        {
            keys.AppendLine($@"{reg.FullKey}\ApplicationName");
        }

        if (!reg.Write("ApplicationIcon", $"\"{IGExePath}\", 0"))
        {
            keys.AppendLine($@"{reg.FullKey}\ApplicationIcon");
        }

        if (!reg.Write("ApplicationDescription", "A lightweight, versatile image viewer"))
        {
            keys.AppendLine($@"{reg.FullKey}\ApplicationDescription");
        }

        // Register File Associations
        var extList = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach (var ext in extList)
        {
            var keyname = $"{appName}.AssocFile" + ext.ToUpper();
            var extNoDot = ext[1..].ToUpperInvariant();

            reg.SubKey = $@"SOFTWARE\{appName}\Capabilities\FileAssociations";
            if (!reg.Write(ext, keyname))
            {
                keys.AppendLine($@"{reg.FullKey}\{ext}");
            }

            // File type description: ImageGlass <...> JPG file
            reg.SubKey = @"SOFTWARE\Classes\" + keyname;
            if (!reg.Write("", $"{appName} {extNoDot} file"))
            {
                keys.AppendLine($@"{reg.FullKey}");
            }

            // File type icon
            var iconPath = StartUpDir(@"Ext-Icons\" + extNoDot + ".ico");
            if (!File.Exists(iconPath))
            {
                iconPath = IGExePath;
            }

            reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\DefaultIcon";
            if (!reg.Write("", $"\"{iconPath}\", 0"))
            {
                keys.AppendLine($@"{reg.FullKey}");
            }

            // Friendly App Name
            reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open";
            if (!reg.Write("FriendlyAppName", App.AppName))
            {
                keys.AppendLine($@"{reg.FullKey}\FriendlyAppName");
            }

            // Execute command
            reg.SubKey = @"SOFTWARE\Classes\" + keyname + @"\shell\open\command";
            if (!reg.Write("", $"\"{IGExePath}\" \"%1\""))
            {
                keys.AppendLine($@"{reg.FullKey}");
            }
        }

        return (keys.Length == 0, keys);
    }


    /// <summary>
    /// Unregister file type associations and app information from registry
    /// </summary>
    /// <param name="exts">Extensions string to delete. Ex: *.png;*.bmp;</param>
    public static (bool IsSuccessful, StringBuilder Keys) UnregisterAppAndExtensions(string exts)
    {
        var appName = "ImageGlass";
        var keys = new StringBuilder();

        var reg = new RegistryEx
        {
            ShowError = false,
            BaseRegistryKey = Registry.LocalMachine,
        };

        var extList = exts.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach (var ext in extList)
        {
            reg.SubKey = $@"SOFTWARE\Classes\{appName}.AssocFile" + ext.ToUpper();
            reg.DeleteSubKeyTree();
        }

        reg.SubKey = $@"SOFTWARE\{appName}";
        if (!reg.DeleteSubKeyTree())
        {
            keys.AppendLine(reg.FullKey);
        }

        reg.SubKey = @"SOFTWARE\RegisteredApplications";
        if (!reg.DeleteKey(appName))
        {
            keys.AppendLine(reg.FullKey);
        }

        reg.SubKey = $@"SOFTWARE\{appName}\Capabilities\FileAssociations";
        if (!reg.DeleteSubKeyTree())
        {
            keys.AppendLine(reg.FullKey);
        }

        return (keys.Length == 0, keys);
    }


    /// <summary>
    /// Register app protocol to registry
    /// </summary>
    /// <returns></returns>
    public static bool RegisterAppProtocol()
    {
        _ = UnregisterAppProtocol();

        var baseKey = $@"SOFTWARE\Classes\{Constants.APP_PROTOCOL}";
        var reg = new RegistryEx
        {
            ShowError = false,
            BaseRegistryKey = Registry.CurrentUser,
            SubKey = baseKey,
        };

        if (!reg.Write("", $"URL: {App.AppName} Protocol"))
        {
            return false;
        }

        if (!reg.Write("URL Protocol", ""))
        {
            return false;
        }

        // DefaultIcon
        reg.SubKey = $@"{baseKey}\DefaultIcon";
        if (!reg.Write("", $"\"{IGExePath}\", 0"))
        {
            return false;
        }

        // shell\open\command
        reg.SubKey = $@"{baseKey}\shell\open\command";
        if (!reg.Write("", $"\"{IGExePath}\" \"%1\""))
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// Delete app protocol from registry
    /// </summary>
    /// <returns></returns>
    public static bool UnregisterAppProtocol()
    {
        var baseKey = $@"SOFTWARE\Classes\{Constants.APP_PROTOCOL}";

        var reg = new RegistryEx
        {
            ShowError = false,
            BaseRegistryKey = Registry.CurrentUser,
            SubKey = baseKey,
        };

        if (!reg.DeleteSubKeyTree()) return false;

        return true;
    }


}
