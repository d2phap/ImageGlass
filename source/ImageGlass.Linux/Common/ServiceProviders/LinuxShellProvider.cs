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
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;

internal class LinuxShellProvider : PhDisposable, IShellProvider
{
    private static readonly string _desktopFileId = $"imageglass.desktop";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string InstallChannelId => BHelper.IsFlatpakSandbox ? "flatpak"
        : BHelper.IsAppImage ? "appimage"
        : "zip";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanRegisterAppMenuEntry => IntegrationHelperPath is not null;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<bool> RegisterAppMenuEntryAsync() => RunIntegrationHelperAsync("--install");


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<bool> UnregisterAppMenuEntryAsync() => RunIntegrationHelperAsync("--remove");


    /// <summary>
    /// Path of the bundled integration script, or <c>null</c> when not running from an AppImage.
    /// </summary>
    private static string? IntegrationHelperPath
    {
        get
        {
            if (!BHelper.IsAppImage) return null;

            var appDir = Environment.GetEnvironmentVariable("APPDIR");
            if (string.IsNullOrEmpty(appDir)) return null;

            var path = Path.Combine(appDir, "usr", "bin", "ig-appimage-integrate");
            return File.Exists(path) ? path : null;
        }
    }


    /// <summary>
    /// Runs the bundled integration script; never throws, so a failure cannot reach the caller.
    /// </summary>
    private static async Task<bool> RunIntegrationHelperAsync(string arg)
    {
        if (IntegrationHelperPath is not string helper) return false;

        try
        {
            return await BHelper.RunExeAsync(helper, [arg], waitForExit: true) == 0;
        }
        catch { return false; }
    }


    public object? ForegroundShell { get; set; }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();
        AllowSleep();
        ForegroundShell = null;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanUseForegroundShell()
    {
        // Linux does not support foreground shell integration
        return false;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void DeleteFile(string filePath, bool moveToRecycleBin = true)
    {
        if (moveToRecycleBin)
        {
            // Move to the user's real trash via the FreeDesktop spec. We don't use
            // the Trash portal (broken on some desktops) or 'gio trash' (targets
            // the sandbox trash inside Flatpak). Throw on failure so the caller
            // surfaces an error instead of silently "losing" the file.
            if (!FreeDesktopTrash.Trash(filePath))
            {
                throw new IOException($"IGE: Could not move the file to trash: {filePath}");
            }
        }
        else
        {
            File.Delete(filePath);
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public object? GetForegroundWindowView()
    {
        // Linux does not have a foreground shell object
        return null;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string? GetTargetPathFromShortcut(string? lnkFilePath)
    {
        if (string.IsNullOrWhiteSpace(lnkFilePath)) return null;

        // resolve symlinks
        try
        {
            var fileInfo = new FileInfo(lnkFilePath);
            if (fileInfo.LinkTarget is string target)
            {
                // resolve relative symlink targets to absolute paths
                var resolvedPath = Path.IsPathRooted(target)
                    ? target
                    : Path.GetFullPath(target, Path.GetDirectoryName(lnkFilePath)!);

                return resolvedPath;
            }
        }
        catch { }

        // parse .desktop files to extract the Exec= target
        if (lnkFilePath.EndsWith(".desktop", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                foreach (var line in File.ReadLines(lnkFilePath))
                {
                    if (line.StartsWith("Exec=", StringComparison.Ordinal))
                    {
                        // strip field codes like %f, %F, %u, %U
                        var exec = line["Exec=".Length..].Trim();
                        var spaceIndex = exec.IndexOf(' ');
                        return spaceIndex > 0 ? exec[..spaceIndex] : exec;
                    }
                }
            }
            catch { }
        }

        return null;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task OpenDefaultEditingAppAsync(string filePath, Action? callbackFn = null)
    {
        // Open the file in its associated application.
        XdgPortal.OpenPath(filePath);
        callbackFn?.Invoke();

        return Task.CompletedTask;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OpenFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        // Reveal & select the file in the default file manager.
        XdgPortal.ShowInFileManager(filePath);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OpenFolderPath(string? dirPath)
    {
        if (string.IsNullOrWhiteSpace(dirPath)) return;

        try
        {
            Directory.CreateDirectory(dirPath);
        }
        catch { }

        // Open the folder in the default file manager.
        XdgPortal.ShowFolders(dirPath);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<DefaultAppScope?> SetDefaultPhotoViewerAsync(string[] extensions, bool enable)
    {
        throw new NotSupportedException("IGE: This feature is not supported on Linux.");
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task SetLockScreenAsync(string filePath)
    {
        throw new NotSupportedException("IGE: This feature is not supported on Linux.");
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetWallpaper(string filePath)
    {
        // Route through the Wallpaper portal so it works inside the Flatpak
        // sandbox (gsettings would only change the sandbox's own dconf). The
        // portal shows the system wallpaper preview dialog for confirmation.
        XdgPortal.SetWallpaper(filePath);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowOpenWith(string filePath)
    {
        throw new NotSupportedException("IGE: This feature is not supported on Linux.");
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowFileProperties(string filePath, nint windowHandle)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        // Show the file manager's properties dialog for the file.
        XdgPortal.ShowInFileManager(filePath, showProperties: true);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowShare(nint windowHandle, string[] filePaths)
    {
        // Sharing is disabled on Linux for now (the Share button is hidden in the UI).
        // Linux has no general "share" sheet; the closest target is the XDG Email
        // portal, but it accepts attachments only as file descriptors (attachment_fds),
        // which the gdbus CLI cannot pass. Re-enable here once a managed D-Bus client
        // that can pass FDs is wired up.
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void PreventSleep(string reason) => LinuxPowerApi.PreventSleep(reason);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void AllowSleep() => LinuxPowerApi.AllowSleep();



    #region Private helpers

    /// <summary>
    /// Removes a MIME type association from the mimeapps.list file.
    /// </summary>
    private static void RemoveMimeAssociation(string mimeappsPath, string mimeType)
    {
        if (!File.Exists(mimeappsPath)) return;

        try
        {
            var lines = File.ReadAllLines(mimeappsPath);
            var prefix = $"{mimeType}=";

            using var writer = new StreamWriter(mimeappsPath);
            foreach (var line in lines)
            {
                // skip lines that assign this MIME type to our app
                if (line.StartsWith(prefix, StringComparison.Ordinal)
                    && line.Contains(_desktopFileId, StringComparison.Ordinal))
                {
                    continue;
                }

                writer.WriteLine(line);
            }
        }
        catch { }
    }

    #endregion // Private helpers

}
