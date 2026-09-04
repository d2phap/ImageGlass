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
using ImageGlass.Common.Types;
using System;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public interface IShellProvider : IDisposable
{
    /// <summary>
    /// Gets, sets the Shell object of foreground window.
    /// </summary>
    object? ForegroundShell { get; set; }


    /// <summary>
    /// Check if we can use the foreground shell folder for loading images.
    /// </summary>
    bool CanUseForegroundShell();


    /// <summary>
    /// Gets the foreground shell object.
    /// </summary>
    object? GetForegroundWindowView();


    /// <summary>
    /// Gets the target path from shortcute file path
    /// </summary>
    string? GetTargetPathFromShortcut(string? lnkFilePath);


    /// <summary>
    /// Opens file explorer and selects the file.
    /// </summary>
    void OpenFilePath(string? filePath);


    /// <summary>
    /// Opens file explorer and selects the folder.
    /// </summary>
    void OpenFolderPath(string? dirPath);


    /// <summary>
    /// Deletes a file with option to move to recycle bin.
    /// </summary>
    void DeleteFile(string filePath, bool moveToRecycleBin = true);


    /// <summary>
    /// Shows the "Open with" dialog for the specified file.
    /// </summary>
    void ShowOpenWith(string filePath);


    /// <summary>
    /// Shows the file's Properties dialog.
    /// </summary>
    void ShowFileProperties(string filePath, nint windowHandle);


    /// <summary>
    /// Shows the OS Share dialog for the given files.
    /// </summary>
    void ShowShare(nint windowHandle, string[] filePaths);


    /// <summary>
    /// Sets the desktop wallpaper.
    /// </summary>
    void SetWallpaper(string filePath);


    /// <summary>
    /// Sets the lock screen image.
    /// </summary>
    Task SetLockScreenAsync(string filePath);


    /// <summary>
    /// Opens the specified file in the system's default editing application.
    /// </summary>
    Task OpenDefaultEditingAppAsync(string filePath, Action? callbackFn = null);


    /// <summary>
    /// Sets or removes this app as the default photo viewer for the specified file extensions.
    /// Returns the scope (per-user vs per-machine) used, or <c>null</c> when not supported.
    /// </summary>
    Task<DefaultAppScope?> SetDefaultPhotoViewerAsync(string[] extensions, bool enable);


    /// <summary>
    /// Rewrites a default-photo-viewer registration whose launch path an app update deleted.
    /// </summary>
    void RepairDefaultViewerRegistration() { }


    /// <summary>
    /// Gets the registry scope (per-user vs per-machine) that would be used to register
    /// the app as the default photo viewer, based on where the app is installed.
    /// </summary>
    DefaultAppScope GetDefaultViewerScope() => DefaultAppScope.CurrentUser;


    /// <summary>
    /// Whether the app can register file associations that the shell honors (false for a virtualized Store MSIX).
    /// </summary>
    bool IsDefaultViewerConfigurable => true;


    /// <summary>
    /// Whether the app runs from a packaged install (Windows MSIX), where the installer/OS owns
    /// file associations. Default (non-Windows / unpackaged): <c>false</c>.
    /// </summary>
    bool IsPackagedApp => false;


    /// <summary>
    /// Whether the app can add itself to the system's application menu. False wherever the
    /// installer already did it, so only a self-contained build that installs nothing says true.
    /// </summary>
    bool CanRegisterAppMenuEntry => false;


    /// <summary>
    /// Adds the app to the system's application menu, so it can be launched from there and picked
    /// as a default handler for image files.
    /// </summary>
    /// <returns><c>true</c> when an entry was written.</returns>
    Task<bool> RegisterAppMenuEntryAsync() => Task.FromResult(false);


    /// <summary>
    /// Removes the entry created by <see cref="RegisterAppMenuEntryAsync"/>.
    /// </summary>
    /// <returns><c>true</c> when an entry was removed.</returns>
    Task<bool> UnregisterAppMenuEntryAsync() => Task.FromResult(false);


    /// <summary>
    /// Coarse distribution channel reported by the anonymous usage statistics, e.g.
    /// <c>msstore</c>, <c>msix</c>, <c>msi</c>, <c>flatpak</c>, <c>appimage</c>, <c>dmg</c>, <c>zip</c>.
    /// Must stay a small closed set; never derive it from a filesystem path.
    /// </summary>
    string InstallChannelId => "zip";


    /// <summary>
    /// Resolves an app-owned file or dir path to where it physically lives. On a packaged (MSIX)
    /// Windows build, per-package write virtualization may redirect <c>%LocalAppData%</c> content to
    /// the package container; returns that real path when it exists. Default (non-Windows /
    /// unpackaged): returns <paramref name="path"/> unchanged.
    /// </summary>
    string GetActualPath(string path) => path;


    /// <summary>
    /// Returns <c>true</c> if the current scroll event originates from a trackpad
    /// (precise/continuous scrolling). Must be called during a scroll event handler.
    /// Returns <c>false</c> for mouse wheel (discrete) events.
    /// </summary>
    bool HasPreciseScrollingDeltas() => false;


    /// <summary>
    /// Detaches the OS IME from the window so it stops claiming keystrokes. Avalonia attaches its
    /// own context when a text field takes focus, so there is no attach counterpart.
    /// </summary>
    void DetachIme(nint windowHandle) { }


    /// <summary>
    /// Draws the window title bar in dark or light colors. Only platforms whose title bar follows
    /// the app theme implement this.
    /// </summary>
    void SetTitleBarDarkMode(nint windowHandle, bool isDark) { }


    /// <summary>
    /// Keeps the system and the display awake, e.g. while a slideshow plays. Best-effort.
    /// </summary>
    void PreventSleep(string reason) { }


    /// <summary>
    /// Releases the request held by <see cref="PreventSleep"/>.
    /// </summary>
    void AllowSleep() { }
}
