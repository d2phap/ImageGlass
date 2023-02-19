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

using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using System.Security;

namespace igcmd;

public static class Functions
{
    /// <summary>
    /// Set desktop wallpaper
    /// </summary>
    /// <param name="bmpPath">Full path of BMP file</param>
    /// <param name="styleStr">Wallpaper style, see <see cref="WallpaperStyle"/>.</param>
    /// <returns></returns>
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


}
