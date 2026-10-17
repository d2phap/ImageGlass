/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia;
using Avalonia.Controls;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace ImageGlass.Common;

public partial class BHelper
{
    private static string Win32ShortcutExtension => ".lnk";


    /// <summary>
    /// Where this process reads the app dir vs. where it really is; <c>null</c> when they are the same.
    /// </summary>
    private static readonly Lazy<(string Mounted, string Real)?> _appDirMount = new(ReadAppDirMount);


    /// <summary>
    /// Gets the full, real dir path of the app binary. Use <see cref="BaseDir(string[])"/> to reach a
    /// file in it: a sandboxed app reads its own dir through a mount, so this path is display-only.
    /// </summary>
    public static string BasePath => GetRealPlatformPath(AppDomain.CurrentDomain.BaseDirectory);


    /// <summary>
    /// Gets the config dir path: the base dir in portable mode (<see cref="ConfigMode"/>),
    /// else the per-user app data dir.
    /// </summary>
    public static string ConfigPath => ConfigMode.IsPortable
        ? BaseDir()
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);



    /// <summary>
    /// Computes the full path based on the installed folder, as this process must address it.
    /// </summary>
    public static string BaseDir(params string[] paths)
    {
        var newPaths = paths.ToList();
        newPaths.Insert(0, BasePath);
        var path = Path.Combine([.. newPaths]);

        return ToProcessPath(path);
    }


    /// <summary>
    /// Computes the full path based on the config folder.
    /// Also, auto-create the built-in directory if not exist.
    /// </summary>
    public static string ConfigDir(params string[] paths)
    {
        var isDirCreated = false;

        // 1. auto-create built-in directory if not exist
        if (paths.Length > 0)
        {
            var firstPath = paths[0];
            var isBuiltinDir = firstPath.Equals(Dir.Themes, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.ExtIcons, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Language, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Plugins, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Cache, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Temporary, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Logs, StringComparison.OrdinalIgnoreCase)
                || firstPath.Equals(Dir.Transitions, StringComparison.OrdinalIgnoreCase);

            // create the built-in directory if not exist
            if (isBuiltinDir)
            {
                var builtinConfigPath = Path.Combine(ConfigPath, firstPath);
                TryCreateDirectory(builtinConfigPath);
                isDirCreated = true;
            }
        }


        // 2. create the config directory if not exist
        if (!isDirCreated)
        {
            TryCreateDirectory(ConfigPath);
        }


        // 3. build the complete path
        var newPaths = paths.ToList();
        newPaths.Insert(0, ConfigPath);
        var path = Path.Combine([.. newPaths]);

        return path;
    }


    /// <summary>
    /// Creates the directory, ignoring failures: a read-only config dir must not throw out of a
    /// path getter, so the failing read/write reports it instead.
    /// </summary>
    private static void TryCreateDirectory(string dirPath)
    {
        try
        {
            Directory.CreateDirectory(dirPath);
        }
        catch { }
    }


    /// <summary>
    /// Like <see cref="ConfigDir(string[])"/> but resolves to the real physical path;
    /// use it to show/open config in the file explorer.
    /// </summary>
    public static string GetRealPlatformConfigDir(params string[] paths)
    {
        return GetRealPlatformPath(ConfigDir(paths));
    }


    /// <summary>
    /// Reads the Flatpak app-dir mount from <c>/.flatpak-info</c> (<c>app-path</c> is the real dir
    /// behind <c>/app</c>); <c>null</c> on every other platform and outside the sandbox.
    /// </summary>
    private static (string Mounted, string Real)? ReadAppDirMount()
    {
        if (!IsFlatpakSandbox) return null;

        try
        {
            const string key = "app-path=";

            foreach (var line in File.ReadLines("/.flatpak-info"))
            {
                if (!line.StartsWith(key, StringComparison.Ordinal)) continue;

                var realPath = line[key.Length..].Trim().TrimEnd('/');
                return string.IsNullOrEmpty(realPath) ? null : ("/app", realPath);
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Resolves an app-owned path to where it physically lives, as the OS and the user see it: this
    /// process may reach its own folders through a container mount or a redirected copy.
    /// </summary>
    /// <remarks>
    /// Two platform indirections, neither visible outside the process: a sandboxed Linux app (Flatpak)
    /// mounts its install dir elsewhere, and a packaged Windows app (MSIX) may redirect
    /// <c>%LocalAppData%</c> writes into the package container. Use this wherever a path is shown to
    /// the user or handed to something outside the process (file manager, another app), and NEVER to
    /// read or write - the real path is not reachable from in here. <see cref="ToProcessPath"/> is the
    /// inverse.
    /// </remarks>
    public static string GetRealPlatformPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path ?? string.Empty;

        var appDir = _appDirMount.Value;
        var realPath = appDir is null
            ? path
            : SwapPathPrefix(path, appDir.Value.Mounted, appDir.Value.Real);

        return Core.ShellProvider?.GetActualPath(realPath) ?? realPath;
    }


    /// <summary>
    /// Inverse of <see cref="GetRealPlatformPath"/>: maps a real app-dir path back to the mount this
    /// process reads through. Only the mount is undone; a redirected copy is already readable.
    /// </summary>
    private static string ToProcessPath(string path)
    {
        var appDir = _appDirMount.Value;

        return appDir is null
            ? path
            : SwapPathPrefix(path, appDir.Value.Real, appDir.Value.Mounted);
    }


    /// <summary>
    /// Replaces the <paramref name="from"/> dir prefix of <paramref name="path"/> with
    /// <paramref name="to"/>, matching whole segments only. Both prefixes carry no trailing separator.
    /// </summary>
    private static string SwapPathPrefix(string path, string from, string to)
    {
        if (!path.StartsWith(from, StringComparison.Ordinal)) return path;

        // a sibling such as "/appdata" must not match the "/app" prefix
        var rest = path[from.Length..];
        if (rest.Length > 0 && rest[0] != Path.DirectorySeparatorChar) return path;

        return to + rest;
    }


    /// <summary>
    /// Check if the given path (file or directory) is writable. 
    /// </summary>
    /// <param name="type">Indicates if the given path is either file or directory</param>
    /// <param name="path">Full path of file or directory</param>
    public static bool CheckPathWritable(PathType type, string path)
    {
        try
        {
            // If path is file
            if (type == PathType.File)
            {
                using (File.OpenWrite(path)) { }
            }

            // if path is directory
            else
            {
                var isDirExist = Directory.Exists(path);

                if (!isDirExist)
                {
                    Directory.CreateDirectory(path);
                }

                var sampleFile = Path.Combine(path, "test_write_file.temp");

                using (File.Create(sampleFile)) { }
                File.Delete(sampleFile);

                if (!isDirExist)
                {
                    Directory.Delete(path, true);
                }
            }


            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Checks type of the path.
    /// </summary>
    public static PathType CheckPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return PathType.Unknown;

        try
        {
            var attrs = File.GetAttributes(path);

            if (attrs.HasFlag(FileAttributes.Directory))
            {
                return PathType.Dir;
            }

            return PathType.File;
        }
        catch { }

        return PathType.Unknown;
    }


    /// <summary>
    /// Checks whether <paramref name="path"/> resolves to a location inside
    /// <paramref name="root"/> (or equals it). Both are resolved with
    /// <see cref="System.IO.Path.GetFullPath(string)"/> first, so <c>..</c>
    /// segments and absolute paths cannot escape the root.
    /// </summary>
    public static bool IsPathContainedIn(string? path, string? root)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(root)) return false;

        string fullPath, fullRoot;
        try
        {
            fullPath = Path.GetFullPath(path);
            fullRoot = Path.GetFullPath(root);
        }
        catch
        {
            return false;
        }

        // Trailing separator on root so a sibling like "_pluginsEvil" can't prefix-match "_plugins".
        var sep = Path.DirectorySeparatorChar;
        if (!fullRoot.EndsWith(sep)) fullRoot += sep;

        // Windows and macOS default to case-insensitive filesystems; Linux is case-sensitive.
        var comparison = OperatingSystem.IsLinux()
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        return (fullPath + sep).StartsWith(fullRoot, comparison);
    }


    /// <summary>
    /// Get distinct directories list from paths list.
    /// </summary>
    public static (List<string> DirPaths, List<string> FilePaths) GetDistinctDirsFromPaths(IEnumerable<string> pathList)
    {
        if (!pathList.Any()) return ([], []);

        var hashedDirsList = new HashSet<string>();
        var hashedFilesList = new HashSet<string>();

        foreach (var path in pathList)
        {
            var pathType = BHelper.CheckPath(path);
            if (pathType == PathType.Unknown) continue;

            if (pathType == PathType.Dir)
            {
                hashedDirsList.Add(path);
            }
            else
            {
                string? dir;

                if (string.Equals(Path.GetExtension(path), Win32ShortcutExtension, StringComparison.OrdinalIgnoreCase))
                {
                    if (Core.ShellProvider is null) continue;

                    var shortcutPath = Core.ShellProvider.GetTargetPathFromShortcut(path);
                    if (string.IsNullOrEmpty(shortcutPath)) continue;

                    var shortcutPathType = BHelper.CheckPath(shortcutPath);
                    if (shortcutPathType == PathType.Unknown) continue;

                    // get the DIR path of shortcut target
                    if (shortcutPathType == PathType.Dir)
                    {
                        dir = shortcutPath;
                    }
                    else
                    {
                        hashedFilesList.Add(shortcutPath);
                        dir = Path.GetDirectoryName(shortcutPath) ?? "";
                    }
                }
                else
                {
                    hashedFilesList.Add(path);
                    dir = Path.GetDirectoryName(path) ?? null;
                }


                if (string.IsNullOrEmpty(dir)) continue;
                hashedDirsList.Add(dir);
            }
        }

        return ([.. hashedDirsList], [.. hashedFilesList]);
    }


    /// <summary>
    /// Gets the next (<paramref name="direction"/> = <c>+1</c>) or previous (<c>-1</c>) sibling
    /// directory relative to <paramref name="currentPath"/> that directly contains at least one
    /// image with an allowed extension. Empty/unreadable siblings are skipped.
    /// </summary>
    /// <param name="currentPath">A directory path, or an image file path (its folder is used).</param>
    /// <param name="direction"><c>+1</c> for next, <c>-1</c> for previous.</param>
    /// <param name="allowedExtensions">Allowed extensions with a leading dot (e.g. <c>.jpg</c>).</param>
    /// <param name="includeHidden">Whether hidden folders/files are eligible.</param>
    /// <returns>Full path of the sibling directory, or <c>null</c> if none is found.</returns>
    public static string? GetSiblingDir(string? currentPath, int direction,
        ICollection<string> allowedExtensions, bool includeHidden)
    {
        if (string.IsNullOrEmpty(currentPath)) return null;

        // accept a file path too: use its containing folder
        var currentDir = CheckPath(currentPath) == PathType.File
            ? Path.GetDirectoryName(currentPath)
            : currentPath;
        if (string.IsNullOrEmpty(currentDir)) return null;

        currentDir = currentDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parentDir = Directory.GetParent(currentDir)?.FullName;
        if (string.IsNullOrEmpty(parentDir)) return null;

        try
        {
            // the current dir must stay in the list to locate itself, even when it is hidden
            var siblingDirs = Directory.GetDirectories(parentDir)
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var currentIndex = siblingDirs.FindIndex(d =>
                string.Equals(d.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                    currentDir, StringComparison.OrdinalIgnoreCase));
            if (currentIndex < 0) return null;

            for (var i = currentIndex + direction; i >= 0 && i < siblingDirs.Count; i += direction)
            {
                if (IsPathSkippedByAttributes(siblingDirs[i], includeHidden)) continue;
                if (DirContainsImage(siblingDirs[i], allowedExtensions, includeHidden)) return siblingDirs[i];
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }

        return null;
    }


    /// <summary>
    /// Checks whether <paramref name="dir"/> directly contains a file whose extension is in
    /// <paramref name="allowedExtensions"/> (extensions include the leading dot, e.g. <c>.jpg</c>).
    /// </summary>
    public static bool DirContainsImage(string? dir, ICollection<string> allowedExtensions, bool includeHidden)
    {
        if (string.IsNullOrEmpty(dir)) return false;

        try
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*", GetEnumerationOptions(includeHidden)))
            {
                if (allowedExtensions.Contains(Path.GetExtension(file))) return true;
            }
        }
        catch { }

        return false;
    }


    /// <summary>
    /// Gets the shared file/folder enumeration options: system items are always skipped,
    /// hidden ones only when <paramref name="includeHidden"/> is <c>false</c>.
    /// </summary>
    public static EnumerationOptions GetEnumerationOptions(bool includeHidden, bool recurse = false)
    {
        return new EnumerationOptions()
        {
            IgnoreInaccessible = true,
            AttributesToSkip = GetSkippedFileAttributes(includeHidden),
            RecurseSubdirectories = recurse,
        };
    }


    /// <summary>
    /// Gets the file attributes excluded from browsing (see <see cref="GetEnumerationOptions"/>).
    /// </summary>
    public static FileAttributes GetSkippedFileAttributes(bool includeHidden)
    {
        var attrs = FileAttributes.System;
        if (!includeHidden) attrs |= FileAttributes.Hidden;

        return attrs;
    }


    /// <summary>
    /// Checks whether <paramref name="path"/> carries an attribute excluded from browsing.
    /// Unreadable paths are treated as not skipped.
    /// </summary>
    public static bool IsPathSkippedByAttributes(string? path, bool includeHidden)
    {
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            return (File.GetAttributes(path) & GetSkippedFileAttributes(includeHidden)) != 0;
        }
        catch { return false; }
    }


    /// <summary>
    /// Checks whether <paramref name="path"/>, or any of its folders up to (but excluding)
    /// <paramref name="rootDir"/>, carries an attribute excluded from browsing. Used to keep
    /// items inside a hidden sub-folder out of the photo list.
    /// </summary>
    public static bool IsPathSkippedUnderRoot(string? path, string? rootDir, bool includeHidden)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (IsPathSkippedByAttributes(path, includeHidden)) return true;
        if (string.IsNullOrEmpty(rootDir)) return false;

        var root = rootDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var dir = Path.GetDirectoryName(path);

        // walk up to the watched root; the root itself may legitimately be hidden
        while (!string.IsNullOrEmpty(dir)
            && !string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                root, StringComparison.OrdinalIgnoreCase))
        {
            if (IsPathSkippedByAttributes(dir, includeHidden)) return true;
            dir = Path.GetDirectoryName(dir);
        }

        return false;
    }


    /// <summary>
    /// Resolves a relative/protocol/link path to absolute path,
    /// including <c>.app</c> bundle on macOS.
    /// </summary>
    public static string ResolvePath(string? inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
            return inputPath ?? string.Empty;

        var path = inputPath;
        const string protocol = Const.APP_PROTOCOL + ":";

        // if inputPath is URI Scheme
        if (path.StartsWith(protocol))
        {
            // Retrieve the real path
            path = Uri.UnescapeDataString(path)[protocol.Length..];
        }

        // if path is wrapped by quotes
        if (path.Length > 2 && path.StartsWith('"') && path.EndsWith('"'))
        {
            path = path[1..^1];
        }

        // skip expansion when the literal path exists: a filename containing "%TEMP%" must survive
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }

        if (string.Equals(Path.GetExtension(inputPath), Win32ShortcutExtension, StringComparison.OrdinalIgnoreCase))
        {
            path = Core.ShellProvider?.GetTargetPathFromShortcut(path) ?? path;
        }

        // macOS: a .app is a directory, so resolve to its inner executable for direct launching
        if (OS == OSType.Mac
            && path.EndsWith(".app", StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(path))
        {
            // Prefer CFBundleExecutable from Info.plist; fall back to the bundle name.
            var exeName = GetMacOsAppExecutableName(path)
                ?? Path.GetFileNameWithoutExtension(path.TrimEnd('/'));

            var innerExe = Path.Combine(path, "Contents", "MacOS", exeName);
            if (File.Exists(innerExe)) path = innerExe;
        }

        return ToAbsolutePath(path);
    }


    /// <summary>
    /// Expands a relative filesystem path against the current directory. Anything that is not an
    /// existing file or folder is returned unchanged, so shell URIs survive.
    /// </summary>
    private static string ToAbsolutePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path ?? string.Empty;
        if (Path.IsPathFullyQualified(path)) return path;

        try
        {
            var fullPath = Path.GetFullPath(path);

            // a shell URI expands into a nonsense path instead of throwing
            return File.Exists(fullPath) || Directory.Exists(fullPath) ? fullPath : path;
        }
        catch { return path; }
    }



    /// <summary>
    /// Reads <c>CFBundleExecutable</c> from a macOS app bundle's <c>Contents/Info.plist</c>.
    /// Returns <c>null</c> if the plist is missing or the key is absent.
    /// </summary>
    private static string? GetMacOsAppExecutableName(string appBundlePath)
    {
        var plistPath = Path.Combine(appBundlePath, "Contents", "Info.plist");
        if (!File.Exists(plistPath)) return null;

        try
        {
            // Info.plist is a <dict> of alternating <key>/<value> siblings.
            var doc = System.Xml.Linq.XDocument.Load(plistPath);
            var dict = doc.Root?.Element("dict");
            if (dict is null) return null;

            var elements = dict.Elements().ToList();
            for (var i = 0; i < elements.Count - 1; i++)
            {
                if (elements[i].Name.LocalName == "key"
                    && elements[i].Value == "CFBundleExecutable"
                    && elements[i + 1].Name.LocalName == "string")
                {
                    var name = elements[i + 1].Value.Trim();
                    return string.IsNullOrEmpty(name) ? null : name;
                }
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Builds the command line from config value.
    /// Example: <c>-p:EnableFullScreen=True</c>
    /// </summary>
    public static string BuildConfigCmdLine(string configName, object? configValue)
    {
        if (configValue == null) return string.Empty;

        return $"{Const.CONFIG_CMD_PREFIX}{configName}=\"{configValue}\"";
    }



    /// <summary>
    /// Open URL in the default browser.
    /// </summary>
    public static async Task OpenUrlAsync(Visual? visual, string? url, string campaign = "from_unknown")
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;

            // only tag our own http(s) pages with campaign params. Protocol URIs (ms-settings:,
            // mailto:, ...) break if a query is appended, and third-party hosts (github.com) must
            // never receive the app version or which button was clicked.
            if (uri.Scheme is "http" or "https" && IsImageGlassHost(uri.Host))
            {
                var ub = new UriBuilder(uri);
                var queries = HttpUtility.ParseQueryString(ub.Query);
                queries["utm_source"] = $"app_{Core.BuildInfo.FullVersion}";
                queries["utm_medium"] = "app_click";
                queries["utm_campaign"] = campaign;

                ub.Query = queries.ToString();
                uri = ub.Uri;
            }


            var launcher = TopLevel.GetTopLevel(visual)?.Launcher;
            if (launcher is not null)
            {
                await launcher.LaunchUriAsync(uri);
            }
        }
        catch { }
    }


    /// <summary>
    /// Whether <paramref name="host"/> is imageglass.org or one of its subdomains.
    /// </summary>
    private static bool IsImageGlassHost(string? host)
    {
        if (string.IsNullOrEmpty(host)) return false;

        return host.Equals(Const.WEBSITE_HOST, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith($".{Const.WEBSITE_HOST}", StringComparison.OrdinalIgnoreCase);
    }



    /// <summary>
    /// Opens file path in Explorer and selects it.
    /// </summary>
    public static void OpenFilePath(string? filePath)
    {
        if (Core.ShellProvider is null) return;

        Core.ShellProvider.OpenFilePath(filePath);
    }


    /// <summary>
    /// Opens the folder path in Explorer, creates the folder path if not existed.
    /// </summary>
    public static void OpenFolderPath(string? dirPath)
    {
        if (Core.ShellProvider is null) return;

        Core.ShellProvider.OpenFolderPath(dirPath);
    }


    /// <summary>
    /// Deletes a file with option to move to recycle bin.
    /// </summary>
    public static void DeleteFile(string filePath, bool moveToRecycleBin = true)
    {
        if (Core.ShellProvider is not null)
        {
            Core.ShellProvider.DeleteFile(filePath, moveToRecycleBin);
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch { }
    }

}

