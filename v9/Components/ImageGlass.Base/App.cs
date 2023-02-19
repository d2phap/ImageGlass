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
using Microsoft.Win32;
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
    public static Exception? RegisterAppAndExtensions(string extensions)
    {
        const string APP_NAME = "ImageGlass";
        var capabilitiesPath = $@"Software\{APP_NAME}\Capabilities";

        _ = UnregisterAppAndExtensions(extensions);


        try
        {
            // Register the application:
            // HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications --------------------------------
            const string regAppPath = @"Software\RegisteredApplications";
            using (var key = Registry.LocalMachine.OpenSubKey(regAppPath, true))
            {
                key?.SetValue(APP_NAME, capabilitiesPath);
            }


            // Register application information:
            // HKEY_LOCAL_MACHINE\SOFTWARE\ImageGlass\Capabilities -------------------------------
            using (var key = Registry.LocalMachine.CreateSubKey(capabilitiesPath, true))
            {
                key?.SetValue("ApplicationName", App.AppName);
                key?.SetValue("ApplicationIcon", $"\"{IGExePath}\", 0");
                key?.SetValue("ApplicationDescription", "A lightweight, versatile image viewer");


                // Register application's file type associations:
                // HKEY_LOCAL_MACHINE\SOFTWARE\ImageGlass\Capabilities\FileAssociations ----------
                using (var faKey = key?.CreateSubKey("FileAssociations", true))
                {
                    var exts = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (var ext in exts)
                    {
                        var extNoDot = ext[1..].ToUpperInvariant();
                        var extAssocKey = $"{APP_NAME}.AssocFile.{extNoDot}";

                        // register supported extension
                        faKey?.SetValue(ext, extAssocKey);


                        // write extension info
                        // HKEY_CLASSES_ROOT\ImageGlass.AssocFile.<EXT>
                        using (var extRootKey = Registry.ClassesRoot.CreateSubKey(extAssocKey, true))
                        {
                            // ImageGlass <EXT> file
                            extRootKey?.SetValue("", $"{APP_NAME} {extNoDot} file");


                            // DefaultIcon -------------------------------------------------------
                            // get extension icon
                            var iconPath = ConfigDir(PathType.File, Dir.ExtIcons, $"{extNoDot}.ico");
                            if (!File.Exists(iconPath))
                            {
                                iconPath = StartUpDir(Dir.ExtIcons, $"{extNoDot}.ico");

                                if (!File.Exists(iconPath))
                                {
                                    iconPath = string.Empty;
                                }
                            }

                            // set extension icon
                            if (!string.IsNullOrEmpty(iconPath))
                            {
                                using (var faIconKey = extRootKey?.CreateSubKey("DefaultIcon", true))
                                {
                                    faIconKey?.SetValue("", iconPath);
                                }
                            }


                            // shell/open --------------------------------------------------------
                            using (var shellOpenKey = extRootKey?.CreateSubKey(@"shell\open", true))
                            {
                                shellOpenKey?.SetValue("FriendlyAppName", App.AppName);


                                // shell/open/command --------------------------------------------
                                using var shellOpenCmdKey = shellOpenKey?.CreateSubKey("command", true);
                                shellOpenCmdKey?.SetValue("", $"\"{IGExePath}\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Unregister file type associations and app information from registry
    /// </summary>
    /// <param name="extensions">Extensions string to delete. Ex: *.png;*.bmp;</param>
    public static Exception? UnregisterAppAndExtensions(string extensions)
    {
        const string APP_NAME = "ImageGlass";

        try
        {
            // Unregister the application:
            // HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications --------------------------------
            const string regAppPath = @"Software\RegisteredApplications";
            using (var key = Registry.LocalMachine.OpenSubKey(regAppPath, true))
            {
                if (key.OpenSubKey(APP_NAME, true) != null)
                {
                    key?.DeleteValue(APP_NAME);
                }
            }


            // Delete application information:
            // HKEY_LOCAL_MACHINE\SOFTWARE\ImageGlass --------------------------------------------
            using (var key = Registry.LocalMachine.OpenSubKey("Software", true))
            {
                key?.DeleteSubKeyTree(APP_NAME, false);
            }


            // Delete file type associations
            // HKEY_CLASSES_ROOT\ImageGlass.AssocFile.<EXT>
            var exts = extensions.Split("*;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in exts)
            {
                var extNoDot = ext[1..].ToUpperInvariant();
                var extAssocKey = $"{APP_NAME}.AssocFile.{extNoDot}";

                Registry.ClassesRoot.DeleteSubKeyTree(extAssocKey, false);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
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
