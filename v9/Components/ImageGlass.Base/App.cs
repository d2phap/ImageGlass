
using System.Diagnostics;

namespace ImageGlass.Base;

public class App
{
    /// <summary>
    /// Gets the application executable path
    /// </summary>
    public static string ExePath { get => StartUpDir("HapplaBox.exe"); }


    /// <summary>
    /// Gets the application version
    /// </summary>
    public static string Version { get => FileVersionInfo.GetVersionInfo(ExePath).FileVersion; }


    /// <summary>
    /// Gets the path based on the startup folder of HapplaBox.
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
    /// Returns the path based on the configuration folder of HapplaBox.
    /// For portable mode, ConfigDir = Installed Dir, else %appdata%\HapplaBox
    /// </summary>
    /// <param name="type">Indicates if the given path is either file or directory</param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string ConfigDir(PathType type, params string[] paths)
    {
        // use StartUp dir if it's writable
        var startUpDir = StartUpDir(paths);

        if (Helpers.CheckPathWritable(type, startUpDir))
        {
            return startUpDir;
        }

        // else, use AppData dir
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"HapplaBox");

        var newPaths = paths.ToList();
        newPaths.Insert(0, appDataDir);
        appDataDir = Path.Combine(newPaths.ToArray());

        return appDataDir;
    }
}
