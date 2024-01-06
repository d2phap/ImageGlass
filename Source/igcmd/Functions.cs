/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Base.Photoing.Codecs;
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
    /// <param name="imgPath">Full path of image file</param>
    /// <param name="styleStr">Wallpaper style, see <see cref="WallpaperStyle"/>.</param>
    public static IgExitCode SetDesktopWallpaper(string imgPath, string styleStr)
    {
        return Run(() =>
        {
            if (Enum.TryParse(styleStr, out WallpaperStyle style))
            {
                var exception = DesktopApi.SetWallpaper(imgPath, style);

                if (exception != null)
                {
                    throw exception;
                }
            }
            else
            {
                throw new ArgumentException("Wallpaper style is not valid.", styleStr);
            }

        }, (error) =>
        {
            _ = Config.ShowError(null,
                description: error.Message,
                title: Config.Language["FrmSettings._Theme._UninstallTheme"]);
        });
    }


    /// <summary>
    /// Sets or unsets app extensions
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="exts">Extensions to proceed. Example: <c>.png;.jpg;</c></param>
    public static IgExitCode SetAppExtensions(bool enable, string exts = "", bool showUi = false, bool hideAdminRequiredErrorUi = false)
    {
        var exitCode = IgExitCode.Done;

        if (string.IsNullOrEmpty(exts))
        {
            exts = Config.GetImageFormats(Config.FileFormats);
        }


        var error = enable
            ? ExplorerApi.RegisterAppAndExtensions(exts)
            : ExplorerApi.UnregisterAppAndExtensions(exts);


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
                : "FrmMain.MnuRemoveDefaultPhotoViewer";

            if (error == null)
            {
                _ = Config.ShowInfo(null,
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
            ShowPinnedPlaces = true,
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
    public static IgExitCode InstallLanguagePacks(List<string> paths)
    {
        return Run(() =>
        {
            var langDir = App.StartUpDir(Dir.Language);
            Directory.CreateDirectory(langDir);

            foreach (var f in paths)
            {
                File.Copy(f, Path.Combine(langDir, Path.GetFileName(f)), true);
            }
        }, (error) =>
        {
            _ = Config.ShowError(null,
                description: error.Message,
                title: Config.Language["FrmSettings._InstallNewLanguagePack"]);
        });
    }


    /// <summary>
    /// Install new theme packs.
    /// </summary>
    public static IgExitCode InstallThemePacks(List<string> paths)
    {
        return Run(() =>
        {
            var igThemeDirPath = App.ConfigDir(PathType.Dir, Dir.Themes);
            Directory.CreateDirectory(igThemeDirPath);

            foreach (var f in paths)
            {
                if (!File.Exists(f)) continue;
                ZipFile.ExtractToDirectory(f, igThemeDirPath, true);
            }
        }, (error) =>
        {
            _ = Config.ShowError(null,
                description: error.Message,
                title: Config.Language["FrmSettings._Theme._InstallTheme"]);
        });
    }


    /// <summary>
    /// Uninstall a theme pack.
    /// </summary>
    public static IgExitCode UninstallThemePack(string themeDirPath)
    {
        return Run(() =>
        {
            var defaultThemeDir = App.ConfigDir(PathType.Dir, Dir.Themes, Const.DEFAULT_THEME);
            if (themeDirPath.Equals(defaultThemeDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Cannot remove the default theme pack.", themeDirPath);
            }

            Directory.Delete(themeDirPath, true);
        }, (error) =>
        {
            _ = Config.ShowError(null,
                description: error.Message,
                title: Config.Language["FrmSettings._Theme._UninstallTheme"]);
        });
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


    /// <summary>
    /// Runs action.
    /// </summary>
    private static IgExitCode Run(Action action, Action<Exception>? onError)
    {
        var exitCode = IgExitCode.Done;
        Exception? error = null;

        try
        {
            action();
        }
        catch (SecurityException ex)
        {
            error = ex;
            exitCode = IgExitCode.AdminRequired;
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


        if (Program.ShowUi && error != null)
        {
            if (exitCode != IgExitCode.AdminRequired || !Program.HideAdminRequiredErrorUi)
            {
                onError?.Invoke(error);
            }
        }

        return IgExitCode.Done;
    }


    /// <summary>
    /// Performs lossless compression.
    /// </summary>
    public static IgExitCode LosslessCompressImage(string imgPath)
    {
        return Run(() =>
        {
            _ = PhotoCodec.LosslessCompress(imgPath);
        }, (error) =>
        {
            _ = Config.ShowError(null,
                description: imgPath,
                title: Config.Language["FrmMain.MnuLosslessCompression"],
                heading: error.Message);
        });
    }
}
