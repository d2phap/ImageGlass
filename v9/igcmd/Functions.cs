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

using igcmd.Tools;
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using System.IO.Compression;
using System.Security;
using Windows.Storage;
using Windows.System.UserProfile;

namespace igcmd;

public static class Functions
{
    /// <summary>
    /// Set desktop wallpaper
    /// </summary>
    /// <param name="bmpPath">Full path of BMP file</param>
    /// <param name="styleStr">Wallpaper style, see <see cref="WallpaperStyle"/>.</param>
    public static IgExitCode SetDesktopWallpaper(string bmpPath, string styleStr)
    {
        // Get style
        if (Enum.TryParse(styleStr, out WallpaperStyle style))
        {
            var result = DesktopApi.SetWallpaper(bmpPath, style);


            if (result == WallpaperResult.PrivilegesFail)
            {
                return IgExitCode.AdminRequired;
            }
            else if (result == WallpaperResult.Success)
            {
                return IgExitCode.Done;
            }
        }

        return IgExitCode.Error;
    }


    /// <summary>
    /// Sets or unsets app extensions
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="ext">Extensions to proceed. Example: <c>*.png;*.jpg;</c></param>
    public static IgExitCode SetAppExtensions(bool enable, string ext = "", bool showUi = false, bool hideAdminRequiredErrorUi = false)
    {
        var exitCode = IgExitCode.Done;

        if (string.IsNullOrEmpty(ext))
        {
            var allExts = Config.AllFormats;
            ext = Config.GetImageFormats(allExts);
        }


        var error = enable
            ? App.RegisterAppAndExtensions(ext)
            : App.UnregisterAppAndExtensions(ext);


        if (error == null)
        {
            exitCode = IgExitCode.Done;
        }
        else if (error is SecurityException && !App.IsAdmin)
        {
            exitCode = IgExitCode.AdminRequired;
        }
        else
        {
            exitCode = IgExitCode.Error;
        }


        // show result dialog
        if (showUi)
        {
            var langPath = enable
                ? "FrmMain.MnuSetDefaultPhotoViewer"
                : "FrmMain.MnuUnsetDefaultPhotoViewer";

            if (error == null)
            {
                var description = enable
                    ? Config.Language[$"{langPath}._SuccessDescription"]
                    : string.Empty;

                _ = Config.ShowInfo(null,
                    description: description,
                    title: Config.Language[langPath],
                    heading: Config.Language[$"{langPath}._Success"]);
            }
            else if (exitCode != IgExitCode.AdminRequired || !hideAdminRequiredErrorUi)
            {
                _ = Config.ShowError(null,
                    description: error.Message,
                    title: Config.Language[langPath],
                    heading: Config.Language[$"{langPath}._Error"],
                    details: error.ToString());
            }
        }


        return exitCode;
    }


    /// <summary>
    /// Opens folder picker and export image frames
    /// </summary>
    public static IgExitCode ExportImageFrames(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(null,
                filePath,
                Config.Language[$"{nameof(FrmExportFrames)}._Title"],
                Config.Language[$"{nameof(FrmExportFrames)}._FileNotExist"]);

            return IgExitCode.Error;
        }


        var destDirPath = Functions.OpenFolderPicker(Config.Language[$"{nameof(FrmExportFrames)}._FolderPickerTitle"]);

        if (!string.IsNullOrEmpty(destDirPath))
        {
            Application.Run(new FrmExportFrames(filePath, destDirPath));
        }


        return IgExitCode.Done;
    }


    /// <summary>
    /// Opens folder picker and exports image frames.
    /// </summary>
    public static string OpenFolderPicker(string title = "")
    {
        using var fb = new FolderBrowserDialog()
        {
            Description = title,
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true,
            AutoUpgradeEnabled = true,

#if NET7_0_OR_GREATER
            ShowPinnedPlaces = true,
#endif
        };
        var result = fb.ShowDialog();


        if (result == DialogResult.OK && Directory.Exists(fb.SelectedPath))
        {
            return fb.SelectedPath;
        }

        return string.Empty;
    }


    /// <summary>
    /// Install new language packs.
    /// </summary>
    public static IgExitCode InstallLanguagePacks(List<string> paths, bool showUi = false, bool hideAdminRequiredErrorUi = false)
    {
        var exitCode = IgExitCode.Done;
        Exception? error = null;

        // create directory if not exist
        if (!Directory.Exists(App.StartUpDir(Dir.Language)))
        {
            Directory.CreateDirectory(App.StartUpDir(Dir.Language));
        }


        foreach (var f in paths)
        {
            try
            {
                File.Copy(f, App.StartUpDir(Dir.Language, Path.GetFileName(f)), true);
            }
            catch (UnauthorizedAccessException ex)
            {
                error = ex;
                exitCode = IgExitCode.AdminRequired;
                break;
            }
            catch (Exception ex)
            {
                error = ex;

                exitCode = IgExitCode.Error;
                break;
            }
        }


        if (showUi && error != null)
        {
            if (exitCode != IgExitCode.AdminRequired || !hideAdminRequiredErrorUi)
            {
                _ = Config.ShowError(null,
                    description: error.Message,
                    title: Config.Language["FrmSettings.Tab.Language._InstallNewLanguagePack"]);
            }
        }

        return IgExitCode.Done;
    }


    /// <summary>
    /// Install new theme packs.
    /// </summary>
    public static IgExitCode InstallThemePacks(List<string> paths, bool showUi = false, bool hideAdminRequiredErrorUi = false)
    {
        var exitCode = IgExitCode.Done;
        Exception? error = null;

        var igThemeDirPath = App.ConfigDir(PathType.Dir, Dir.Themes);
        Directory.CreateDirectory(igThemeDirPath);

        foreach (var f in paths)
        {
            try
            {
                if (!File.Exists(f)) continue;

                ZipFile.ExtractToDirectory(f, igThemeDirPath, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                error = ex;
                exitCode = IgExitCode.AdminRequired;
                break;
            }
            catch (Exception ex)
            {
                error = ex;

                exitCode = IgExitCode.Error;
                break;
            }
        }


        if (showUi && error != null)
        {
            if (exitCode != IgExitCode.AdminRequired || !hideAdminRequiredErrorUi)
            {
                _ = Config.ShowError(null,
                    description: error.Message,
                    title: Config.Language["FrmSettings.Tab.Appearance._Theme._InstallTheme"]);
            }
        }

        return IgExitCode.Done;
    }


    /// <summary>
    /// Uninstall a theme pack.
    /// </summary>
    public static IgExitCode UninstallThemePack(string themeDirPath, bool showUi = false, bool hideAdminRequiredErrorUi = false)
    {
        var exitCode = IgExitCode.Done;
        Exception? error = null;

        try
        {
            var defaultThemeDir = App.ConfigDir(PathType.Dir, Dir.Themes, Constants.DEFAULT_THEME);
            if (themeDirPath.Equals(defaultThemeDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Cannot remove the default theme pack.", themeDirPath);
            }

            Directory.Delete(themeDirPath, true);
        }
        catch (UnauthorizedAccessException ex)
        {
            error = ex;
            exitCode = IgExitCode.AdminRequired;
        }
        catch (Exception ex)
        {
            error = ex;
            exitCode = IgExitCode.Error;
        }


        if (showUi && error != null)
        {
            if (exitCode != IgExitCode.AdminRequired || !hideAdminRequiredErrorUi)
            {
                _ = Config.ShowError(null,
                    description: error.Message,
                    title: Config.Language["FrmSettings.Tab.Appearance._Theme._UninstallTheme"]);
            }
        }

        return IgExitCode.Done;
    }



    /// <summary>
    /// Sets the Lock Screen background
    /// </summary>
    public static IgExitCode SetLockScreenBackground(string imgPath)
    {
        if (string.IsNullOrEmpty(imgPath))
        {
            return IgExitCode.Error;
        }


        var result = BHelper.RunSync(() => SetLockScreenBackgroundAsync(imgPath));

        return result;
    }


    /// <summary>
    /// Sets the Lock Screen background
    /// </summary>
    private static async Task<IgExitCode> SetLockScreenBackgroundAsync(string imgPath)
    {
        try
        {
            var imgFile = await StorageFile.GetFileFromPathAsync(imgPath);

            using var stream = await imgFile.OpenAsync(FileAccessMode.Read);
            await LockScreen.SetImageStreamAsync(stream);
        }
        catch (Exception ex)
        {
            if (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                return IgExitCode.AdminRequired;
            }

            return IgExitCode.Error;
        }

        return IgExitCode.Done;
    }

}
