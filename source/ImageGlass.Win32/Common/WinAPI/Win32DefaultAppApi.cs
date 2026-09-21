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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.UI.Shell;

namespace ImageGlass.Win32.Common;

public static class Win32DefaultAppApi
{
    /// <summary>
    /// Registers or unregisters the app as the default photo viewer for the specified file extensions.
    /// Returns the scope (per-user vs per-machine) that was used, or <c>null</c> when the operation
    /// is not supported (virtualized Store MSIX).
    /// </summary>
    public static async Task<DefaultAppScope?> SetDefaultPhotoViewerAsync(string[] extensions, bool enable)
    {
        // virtualized (Store) MSIX: writes never reach the shell; nothing we can do
        if (Win32AppIdentity.IsPackaged && !Win32AppIdentity.IsUnvirtualizedResources) return null;

        var scope = GetScope();
        var root = scope == DefaultAppScope.LocalMachine
            ? Registry.LocalMachine
            : Registry.CurrentUser;

        try
        {
            if (enable)
            {
                RegisterAppAndExtensions(root, extensions);
                PurgeShadowingPerUserRegistration(scope, extensions);
            }
            else
            {
                UnregisterAppAndExtensions(root, extensions);
            }

            NotifyShellAssocChanged();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            // don't self-relaunch when packaged (fast-fails) or already elevated (silent loop)
            if (Win32AppIdentity.IsPackaged || IsProcessElevated()) throw;

            // per-machine (HKLM) writes need admin; relaunch elevated to finish the job
            var exitCode = await RelaunchElevatedAsync(extensions, enable);

            // over-the-shoulder UAC gives the child another account's hive, not the shadowing one
            if (enable && exitCode == (int)IgExitCode.Done)
            {
                PurgeShadowingPerUserRegistration(scope, extensions);
                NotifyShellAssocChanged();
            }
        }

        return scope;
    }


    /// <summary>
    /// Drops a per-user registration, which outranks HKLM and keeps its own (dead) icon path.
    /// </summary>
    private static void PurgeShadowingPerUserRegistration(DefaultAppScope scope, string[] extensions)
    {
        if (scope != DefaultAppScope.LocalMachine) return;

        try
        {
            var root = Registry.CurrentUser;

            // its own recorded list too, for formats this version no longer registers
            var stale = new HashSet<string>(GetRegisteredExtensions(root), StringComparer.OrdinalIgnoreCase);
            foreach (var ext in extensions) stale.Add(ext);
            if (stale.Count == 0) return;

            UnregisterAppAndExtensions(root, [.. stale]);
        }
        catch { }
    }


    /// <summary>
    /// Whether the current process is running with an elevated (administrator) token.
    /// </summary>
    private static bool IsProcessElevated()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch { return false; }
    }


    /// <summary>
    /// Determines the registry scope from the install location: per-machine when the app runs
    /// from a system location (e.g. Program Files), otherwise per-user (portable install).
    /// </summary>
    public static DefaultAppScope GetScope()
    {
        // packaged: always per-user (HKCU); a packaged exe can't be relaunched elevated for HKLM
        if (Win32AppIdentity.IsPackaged) return DefaultAppScope.CurrentUser;

        var exeDir = Path.GetDirectoryName(BHelper.AppExePath);
        if (string.IsNullOrEmpty(exeDir)) return DefaultAppScope.CurrentUser;

        string[] machineRoots =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        ];

        foreach (var machineRoot in machineRoots)
        {
            if (!string.IsNullOrEmpty(machineRoot)
                && exeDir.StartsWith(machineRoot, StringComparison.OrdinalIgnoreCase))
            {
                return DefaultAppScope.LocalMachine;
            }
        }

        return DefaultAppScope.CurrentUser;
    }


    /// <summary>
    /// Gets the exe to register as the launch command; never the MSIX alias, which has no icon.
    /// </summary>
    private static string LaunchCommandExe => BHelper.AppExePath;


    /// <summary>
    /// Rewrites a registration whose launch or icon path an app update deleted (an MSIX dir is versioned).
    /// </summary>
    public static void RepairDefaultViewerRegistration()
    {
        try
        {
            // virtualized (Store) MSIX: writes never reach the shell
            if (Win32AppIdentity.IsPackaged && !Win32AppIdentity.IsUnvirtualizedResources) return;

            // a per-machine hive needs elevation, and a silent UAC prompt at startup is worse
            if (GetScope() != DefaultAppScope.CurrentUser) return;

            var root = Registry.CurrentUser;
            var extensions = GetRegisteredExtensions(root);
            if (extensions.Length == 0) return;

            var registeredExe = GetRegisteredCommandExe(root, extensions[0]);
            var isCommandAlive = registeredExe.Length > 0 && File.Exists(registeredExe);

            if (isCommandAlive)
            {
                // only our own live registration is ours to rewrite for a dead icon
                var isOurCommand = registeredExe.Equals(LaunchCommandExe, StringComparison.OrdinalIgnoreCase);
                if (!isOurCommand || IsRegisteredIconAlive(root, extensions)) return;
            }

            RefreshRegisteredPaths(root, extensions);
            NotifyShellAssocChanged();
        }
        catch { }
    }


    /// <summary>
    /// Rewrites the registered paths only, never the extension defaults or the user's UserChoice.
    /// </summary>
    private static void RefreshRegisteredPaths(RegistryKey root, string[] extensions)
    {
        using (var key = root.OpenSubKey($@"Software\{BHelper.AppName}\Capabilities", writable: true))
        {
            key?.SetValue("ApplicationIcon", $"\"{BHelper.AppExePath}\", 0");
        }

        using var classesKey = root.OpenSubKey(@"Software\Classes", writable: true);
        if (classesKey is null) return;

        foreach (var ext in extensions)
        {
            var extNoDot = ext.TrimStart('.').ToUpperInvariant();
            RegisterProgId(classesKey, GetProgId(extNoDot), extNoDot);
        }
    }


    /// <summary>
    /// Reads the extensions a previous registration recorded under Capabilities.
    /// </summary>
    private static string[] GetRegisteredExtensions(RegistryKey root)
    {
        using var faKey = root.OpenSubKey($@"Software\{BHelper.AppName}\Capabilities\FileAssociations");
        if (faKey is null) return [];

        return faKey.GetValueNames().Where(name => name.StartsWith('.')).ToArray();
    }


    /// <summary>
    /// Gets the exe recorded in an extension's registered open command.
    /// </summary>
    private static string GetRegisteredCommandExe(RegistryKey root, string ext)
    {
        var extNoDot = ext.TrimStart('.').ToUpperInvariant();
        var progId = GetProgId(extNoDot);

        using var cmdKey = root.OpenSubKey($@"Software\Classes\{progId}\shell\open\command");
        if (cmdKey?.GetValue("") is not string command) return string.Empty;

        return ExtractCommandExePath(command);
    }


    /// <summary>
    /// Whether the first registered <c>DefaultIcon</c> still resolves to a file on disk.
    /// </summary>
    private static bool IsRegisteredIconAlive(RegistryKey root, string[] extensions)
    {
        foreach (var ext in extensions)
        {
            var extNoDot = ext.TrimStart('.').ToUpperInvariant();
            var progId = GetProgId(extNoDot);

            using var iconKey = root.OpenSubKey($@"Software\Classes\{progId}\DefaultIcon");

            // no DefaultIcon falls back to the app icon, never to blank, so it is not a defect
            if (iconKey?.GetValue("") is not string iconPath || iconPath.Length == 0) continue;

            return File.Exists(iconPath);
        }

        return true;
    }


    /// <summary>
    /// Pulls the executable out of a registered open command (<c>"&lt;exe&gt;" "%1"</c>).
    /// </summary>
    private static string ExtractCommandExePath(string command)
    {
        if (!command.StartsWith('"')) return string.Empty;

        var closingQuote = command.IndexOf('"', 1);

        return closingQuote > 1 ? command[1..closingQuote] : string.Empty;
    }


    /// <summary>
    /// Registers file type associations and app capabilities to the registry
    /// under the given <paramref name="root"/> hive (HKCU or HKLM).
    /// </summary>
    private static void RegisterAppAndExtensions(RegistryKey root, string[] extensions)
    {
        var capabilitiesPath = $@"Software\{BHelper.AppName}\Capabilities";
        var classesKey = root.OpenSubKey(@"Software\Classes", writable: true);

        // 0. formats an earlier registration claimed but this one drops: nothing else rewrites them
        var dropped = GetRegisteredExtensions(root).Except(extensions, StringComparer.OrdinalIgnoreCase).ToArray();
        UnregisterExtensions(classesKey, dropped);


        // 1. register the application:
        // <root>\Software\RegisteredApplications
        using (var key = root.OpenSubKey(@"Software\RegisteredApplications", writable: true))
        {
            key?.SetValue(BHelper.AppName, capabilitiesPath);
        }


        // 2. register application information:
        // <root>\Software\ImageGlass\Capabilities
        using (var key = root.CreateSubKey(capabilitiesPath, writable: true))
        {
            key.SetValue("ApplicationName", BHelper.AppName);
            // the real exe has the embedded icon; the execution alias is a 0-byte reparse point (blank)
            key.SetValue("ApplicationIcon", $"\"{BHelper.AppExePath}\", 0");
            key.SetValue("ApplicationDescription", "A Fast, Seamless Photo Viewer");

            // register file type associations:
            // HKCU\Software\ImageGlass\Capabilities\FileAssociations
            using var faKey = key.CreateSubKey("FileAssociations", writable: true);

            // an entry naming a ProgId that no longer exists invalidates the whole Capabilities key
            foreach (var ext in dropped) faKey.DeleteValue(ext, throwOnMissingValue: false);

            foreach (var ext in extensions)
            {
                var extNoDot = ext.TrimStart('.').ToUpperInvariant();
                var progId = GetProgId(extNoDot);
                faKey.SetValue(ext, progId);

                // HKCU\Software\Classes\...
                RegisterProgId(classesKey, progId, extNoDot);
                AssociateExtensionDefault(classesKey, ext, progId);
                ClearUserChoice(ext);
            }
        }

        classesKey?.Dispose();
    }


    /// <summary>
    /// Builds the ProgId of an extension, e.g. <c>ImageGlass.AssocFile.JPG</c>.
    /// </summary>
    private static string GetProgId(string extNoDot) => $"{BHelper.AppName}.AssocFile.{extNoDot}";


    /// <summary>
    /// Builds the file type name Explorer shows for an extension, e.g. <c>ImageGlass JPG File</c>.
    /// </summary>
    private static string GetFriendlyTypeName(string extNoDot) => $"{BHelper.AppName} {extNoDot} File";


    /// <summary>
    /// Registers a ProgId under the <c>Software\Classes</c> subkey of the active hive.
    /// </summary>
    private static void RegisterProgId(RegistryKey? classesKey, string progId, string extNoDot)
    {
        if (classesKey is null) return;

        // <root>\Software\Classes\ImageGlass.AssocFile.<EXT>
        using var progIdKey = classesKey.CreateSubKey(progId, writable: true);

        // Explorer's Type column: per-format, since a bare app name labels every format the same
        var friendlyTypeName = GetFriendlyTypeName(extNoDot);
        progIdKey.SetValue("", friendlyTypeName);

        // shell verbs resolve ASSOCSTR_FRIENDLYDOCNAME from here first, the default value second
        progIdKey.SetValue("FriendlyTypeName", friendlyTypeName);

        // 1. HKCU\Software\Classes\ImageGlass.AssocFile.<EXT>\DefaultIcon
        var iconPath = ResolveExtIconPath(extNoDot);
        if (!string.IsNullOrEmpty(iconPath))
        {
            using var iconKey = progIdKey.CreateSubKey("DefaultIcon", writable: true);
            iconKey.SetValue("", iconPath);
        }
        else
        {
            // no icon on disk any more: drop the one a previous registration left behind
            progIdKey.DeleteSubKeyTree("DefaultIcon", throwOnMissingSubKey: false);
        }


        // 2. HKCU\Software\Classes\ImageGlass.AssocFile.<EXT>\shell\open
        using var shellKey = progIdKey.CreateSubKey("shell", writable: true);
        using var openKey = shellKey.CreateSubKey("open", writable: true);
        openKey.SetValue("FriendlyAppName", BHelper.AppName);


        // 3. HKCU\Software\Classes\ImageGlass.AssocFile.<EXT>\shell\open\command
        using var commandKey = openKey.CreateSubKey("command", writable: true);
        commandKey.SetValue("", $"\"{LaunchCommandExe}\" \"%1\"");
    }


    /// <summary>
    /// Resolves an extension's icon: a user icon in the config dir wins, else the bundled one.
    /// </summary>
    private static string? ResolveExtIconPath(string extNoDot)
    {
        // MSIX may redirect the config dir, so resolve the real physical path
        var userIcon = BHelper.GetRealPlatformConfigDir(Dir.ExtIcons, $"{extNoDot}.ico");
        if (File.Exists(userIcon)) return userIcon;

        // bundled fallback; a packaged install dir is versioned, so an update needs re-registering
        var bundledIcon = BHelper.BaseDir(Dir.ExtIcons, $"{extNoDot}.ico");
        return File.Exists(bundledIcon) ? bundledIcon : null;
    }


    /// <summary>
    /// Sets our ProgId as the extension's classic default and adds it to <c>OpenWithProgids</c>.
    /// </summary>
    private static void AssociateExtensionDefault(RegistryKey? classesKey, string ext, string progId)
    {
        if (classesKey is null) return;

        // <root>\Software\Classes\.<EXT>  (default + OpenWithProgids)
        using var extKey = classesKey.CreateSubKey(ext, writable: true);
        extKey.SetValue("", progId);

        using var openWith = extKey.CreateSubKey("OpenWithProgids", writable: true);
        openWith.SetValue(progId, string.Empty);
    }


    /// <summary>
    /// Removes the current user's hash-protected UserChoice (always HKCU, both scopes) so the
    /// classic default applies; optionally only when it points to <paramref name="onlyIfProgId"/>.
    /// </summary>
    private static void ClearUserChoice(string ext, string? onlyIfProgId = null)
    {
        try
        {
            using var fileExts = Registry.CurrentUser.OpenSubKey(
                $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}", writable: true);
            if (fileExts is null) return;

            if (onlyIfProgId is not null)
            {
                using var uc = fileExts.OpenSubKey("UserChoice");
                if (uc?.GetValue("ProgId") as string != onlyIfProgId) return;
            }

            // reg.exe / Remove-Item are denied on UserChoice; DeleteSubKey via the parent is allowed
            fileExts.DeleteSubKey("UserChoice", throwOnMissingSubKey: false);
        }
        catch { }
    }


    /// <summary>
    /// Unregisters file type associations and app information from the registry
    /// under the given <paramref name="root"/> hive (HKCU or HKLM).
    /// </summary>
    private static void UnregisterAppAndExtensions(RegistryKey root, string[] extensions)
    {
        // 1. unregister the application:
        // <root>\Software\RegisteredApplications\ImageGlass
        using (var key = root.OpenSubKey(@"Software\RegisteredApplications", writable: true))
        {
            key?.DeleteValue(BHelper.AppName, throwOnMissingValue: false);
        }

        // 2. delete application information:
        // <root>\Software\ImageGlass\*
        using (var key = root.OpenSubKey("Software", writable: true))
        {
            key?.DeleteSubKeyTree(BHelper.AppName, throwOnMissingSubKey: false);
        }

        // 3. delete ProgIds and OpenWithProgids entries:
        // <root>\Software\Classes\...
        using var classesKey = root.OpenSubKey(@"Software\Classes", writable: true);
        UnregisterExtensions(classesKey, extensions);
    }


    /// <summary>
    /// Removes our ProgId, extension default and <c>OpenWithProgids</c> entry for each extension.
    /// </summary>
    private static void UnregisterExtensions(RegistryKey? classesKey, IEnumerable<string> extensions)
    {
        if (classesKey is null) return;

        foreach (var ext in extensions)
        {
            var extNoDot = ext.TrimStart('.').ToUpperInvariant();
            var progId = GetProgId(extNoDot);

            // remove HKCU\Software\Classes\ImageGlass.AssocFile.<EXT>\*
            classesKey.DeleteSubKeyTree(progId, throwOnMissingSubKey: false);

            using var extKey = classesKey.OpenSubKey(ext, writable: true);
            if (extKey is not null)
            {
                // clear the default if it points to us
                if (extKey.GetValue("") as string == progId)
                {
                    extKey.DeleteValue("", throwOnMissingValue: false);
                }

                using var openWith = extKey.OpenSubKey("OpenWithProgids", writable: true);
                openWith?.DeleteValue(progId, throwOnMissingValue: false);
            }

            // drop our UserChoice so Explorer re-picks a default
            ClearUserChoice(ext, progId);
        }
    }


    /// <summary>
    /// Notifies the shell that file associations have changed.
    /// </summary>
    private static unsafe void NotifyShellAssocChanged()
    {
        PInvoke.SHChangeNotify(
            SHCNE_ID.SHCNE_ASSOCCHANGED,
            SHCNF_FLAGS.SHCNF_IDLIST,
            null, null);
    }


    /// <summary>
    /// Re-launches the current process with admin elevation to perform the file association change,
    /// then waits for it to finish and returns its <see cref="IgExitCode"/>.
    /// </summary>
    private static async Task<int> RelaunchElevatedAsync(string[] extensions, bool enable)
    {
        var cmd = enable
            ? AppCmds.SET_DEFAULT_PHOTO_VIEWER
            : AppCmds.REMOVE_DEFAULT_PHOTO_VIEWER;
        var extArg = string.Join(";", extensions);

        // reuse the shared elevating launcher (UAC prompt + cancellation handled there)
        return await BHelper.RunExeAsync(BHelper.AppExePath, [cmd, extArg], asAdmin: true, waitForExit: true);
    }

}
