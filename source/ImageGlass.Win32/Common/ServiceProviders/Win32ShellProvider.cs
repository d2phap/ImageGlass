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
using D2Phap;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;

namespace ImageGlass.Win32.Common.ServiceProviders;

public class Win32ShellProvider : PhDisposable, IShellProvider
{
    private readonly EggShell _shell = new();
    private static ExplorerView? _foregroundShell = null;
    private static string _foregroundShellPath = string.Empty;

    private static string Win32SearchFileExtension => ".search-ms";

    /// <summary>
    /// Package container that <c>%LocalAppData%</c> writes are redirected into,
    /// or <c>null</c> when unpackaged (<c>LocalCacheFolder</c> throws without MSIX identity).
    /// </summary>
    private static readonly Lazy<string?> _localCacheDir = new(() =>
    {
        try { return ApplicationData.Current.LocalCacheFolder.Path; }
        catch { return null; }
    });



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public object? ForegroundShell
    {
        get => _foregroundShell;
        set
        {
            _foregroundShell = null;
            _foregroundShell = (ExplorerView?)value;

            try
            {
                _foregroundShellPath = _foregroundShell?.GetTabViewPath() ?? string.Empty;
            }
            catch
            {
                _foregroundShellPath = string.Empty;
                _foregroundShell = null;
            }
        }
    }



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();
        AllowSleep();
        ForegroundShell = null;
    }



    #region Public Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool CanUseForegroundShell()
    {
        // check if we should load images from foreground window
        var inputImageDirPath = Path.GetDirectoryName(Core.InputImagePathFromArgs) ?? string.Empty;
        var isFromSearchWindow = _foregroundShellPath.StartsWith(EggShell.SEARCH_MS_PROTOCOL, StringComparison.OrdinalIgnoreCase);
        var isFromSavedSearch = _foregroundShellPath.EndsWith(Win32SearchFileExtension, StringComparison.OrdinalIgnoreCase);
        var isFromSameDir = inputImageDirPath.Equals(_foregroundShellPath, StringComparison.OrdinalIgnoreCase);

        var useForegroundWindow = _foregroundShell is not null
            && !string.IsNullOrEmpty(Core.InputImagePathFromArgs)
            && (isFromSearchWindow || isFromSavedSearch || isFromSameDir);

        return useForegroundWindow;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public object? GetForegroundWindowView()
    {
        var ev = _shell.GetForegroundWindowView();

        return ev;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string? GetTargetPathFromShortcut(string? lnkFilePath)
    {
        if (string.IsNullOrWhiteSpace(lnkFilePath)) return null;

        return EggShell.GetTargetPathFromShortcut(lnkFilePath);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void OpenFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;

        try
        {
            _shell.SelectFileFromExplorer(filePath);
        }
        catch
        {
            using var proc = Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
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

        try
        {
            using var proc = Process.Start("explorer.exe", $"\"{dirPath}\"");
        }
        catch { }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void DeleteFile(string filePath, bool moveToRecycleBin = true)
    {
        var option = moveToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently;

        try
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, option);
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowOpenWith(string filePath)
    {
        // Uses the system shell32.dll 'OpenAs_RunDLL' entry point
        var args = $"shell32.dll,OpenAs_RunDLL {filePath}";

        _ = Process.Start(new ProcessStartInfo
        {
            FileName = "rundll32.exe",
            Arguments = args,
            UseShellExecute = true,
        });
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowFileProperties(string filePath, nint windowHandle)
    {
        EggShell.DisplayFileProperties(filePath, windowHandle);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void ShowShare(nint windowHandle, string[] filePaths)
    {
        Win32ShareApi.ShowShare(windowHandle, filePaths);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetWallpaper(string filePath)
    {
        Win32DesktopApi.SetWallpaper(filePath, WallpaperStyle.Current);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task SetLockScreenAsync(string filePath)
    {
        var sFile = await StorageFile.GetFileFromPathAsync(filePath);
        await LockScreen.SetImageFileAsync(sFile);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task OpenDefaultEditingAppAsync(string filePath, Action? callbackFn = null)
    {
        #region Windows 11
        if (Environment.OSVersion.Version.Major == 10
            && Environment.OSVersion.Version.Build >= 22000)
        {
            var mspaint11 = @"%LocalAppData%\Microsoft\WindowsApps\mspaint.exe";
            var mspaint11Path = BHelper.ResolvePath(mspaint11);

            if (!File.Exists(mspaint11Path))
            {
                _ = await ModalWindow.ShowInfoAsync(null, new ModalWindowOptions
                {
                    Title = Core.Lang[LangId.Menu_MnuEdit],
                    Heading = Core.Lang[LangId.Menu_MnuEdit_AppNotFound],
                    Description = filePath,
                });
                return;
            }

            using var p11 = new Process();
            p11.StartInfo.FileName = mspaint11Path;
            p11.StartInfo.Arguments = $"\"{filePath}\"";
            p11.StartInfo.UseShellExecute = true;

            try
            {
                p11.Start();
                callbackFn?.Invoke();
            }
            catch (Exception ex)
            {
                _ = await ModalWindow.ShowErrorAsync(null, new ModalWindowOptions
                {
                    Title = string.Format(Core.Lang[LangId.Menu_MnuEdit], "(Microsoft Paint)"),
                    Description = ex.Message + $"\r\n\r\n{filePath}",
                });
            }

            return;
        }
        #endregion // Windows 11


        #region Windows 10 or earlier
        var win32ErrorMsg = string.Empty;

        using var p10 = new Process();
        p10.StartInfo.FileName = $"\"{filePath}\"";
        p10.StartInfo.Verb = "edit";

        // first try: launch the associated app for editing
        try
        {
            p10.Start();
            callbackFn?.Invoke();
        }
        catch (Win32Exception ex)
        {
            // file does not have associated app
            win32ErrorMsg = ex.Message;
        }
        catch { }

        if (string.IsNullOrEmpty(win32ErrorMsg)) return;


        // second try: use MS Paint to edit the file
        using var p = new Process();
        p.StartInfo.FileName = BHelper.ResolvePath("mspaint.exe");
        p.StartInfo.Arguments = $"\"{filePath}\"";
        p.StartInfo.UseShellExecute = true;


        try
        {
            p.Start();
            callbackFn?.Invoke();
        }
        catch (Win32Exception)
        {
            // show error: file does not have associated app
            _ = await ModalWindow.ShowErrorAsync(null, new ModalWindowOptions
            {
                Title = string.Format(Core.Lang[LangId.Menu_MnuEdit]),
                Description = win32ErrorMsg + $"\r\n\r\n{filePath}",
            });
        }
        catch { }
        #endregion // Windows 10 or earlier
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<DefaultAppScope?> SetDefaultPhotoViewerAsync(string[] extensions, bool enable)
    {
        return await Win32DefaultAppApi.SetDefaultPhotoViewerAsync(extensions, enable);
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void RepairDefaultViewerRegistration() => Win32DefaultAppApi.RepairDefaultViewerRegistration();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public DefaultAppScope GetDefaultViewerScope() => Win32DefaultAppApi.GetScope();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsDefaultViewerConfigurable => !Win32AppIdentity.IsPackaged || Win32AppIdentity.IsUnvirtualizedResources;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsPackagedApp => Win32AppIdentity.IsPackaged;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string InstallChannelId => Win32AppIdentity.IsMsStorePackage
        ? "msstore"
        : Win32AppIdentity.IsPackaged ? "msix"
        : ConfigMode.IsPortable ? "zip" : "msi";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string GetActualPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // BHelper.BasePath resolves through here on every call, so keep the WinRT probe cached
        var localCache = _localCacheDir.Value;
        if (localCache is null) return path;

        // only %LocalAppData% is virtualized
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!BHelper.IsPathContainedIn(path, localAppData)) return path;

        // MSIX redirects newly-created AppData\Local writes to <pkg>\LocalCache\Local and reads it
        // first; point at that copy when it physically exists, else the real (write-through) path
        var rel = Path.GetRelativePath(localAppData, path);
        var container = Path.Combine(localCache, "Local", rel);

        return Directory.Exists(container) || File.Exists(container) ? container : path;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void DetachIme(nint windowHandle) => Win32ImeApi.DetachIme(windowHandle);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetTitleBarDarkMode(nint windowHandle, bool isDark) => Win32WindowApi.SetTitleBarDarkMode(windowHandle, isDark);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void PreventSleep(string reason) => Win32PowerApi.PreventSleep();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void AllowSleep() => Win32PowerApi.AllowSleep();

    #endregion // Public Methods


}
