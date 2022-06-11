/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using ImageGlass.UI;

namespace igcmd;

internal static class Program
{
    public static string[] Args = Array.Empty<string>();


    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        // Issue #360: IG periodically searching for dismounted device.
        WindowApi.SetAppErrorMode();

        // To customize application configuration such as set high DPI settings
        // or default font, see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // load application configs
        Config.Load();

        Args = args;

        if (args.Length == 0)
        {
            return ShowDefaultError();
        }

        var topCmd = args[0].ToLower().Trim();


        #region SET_WALLPAPER <string imgPath> [int style]
        if (topCmd == Commands.SET_WALLPAPER)
        {
            if (args.Length < 3)
            {
                return ShowDefaultError();
            }

            return (int)Functions.SetDesktopWallpaper(args[1], args[2]);
        }
        #endregion


        #region SET_DEFAULT_PHOTO_VIEWER [string ext]
        if (topCmd == Commands.SET_DEFAULT_PHOTO_VIEWER)
        {
            var ext = "";
            if (args.Length > 1)
            {
                ext = args[1];
            }

            return (int)Functions.SetAppExtensions(true, ext);
        }
        #endregion


        #region UNSET_DEFAULT_PHOTO_VIEWER [string ext]
        if (topCmd == Commands.UNSET_DEFAULT_PHOTO_VIEWER)
        {
            var ext = "";
            if (args.Length > 1)
            {
                ext = args[1];
            }

            return (int)Functions.SetAppExtensions(false, ext);
        }
        #endregion



        return (int)IgExitCode.Error;
    }


    /// <summary>
    /// Show error popup.
    /// </summary>
    /// <returns><see cref="IgExitCode.Error"/></returns>
    private static int ShowDefaultError()
    {
        var url = "https://imageglass.org/docs/command-line-utilities";
        var langPath = $"{nameof(igcmd)}._DefaultError";

        var result = Popup.ShowError(Config.Theme, Config.Language,
            title: Application.ProductName + " " + Application.ProductVersion,
            heading: Config.Language[$"{langPath}._Heading"],
            description: string.Format(Config.Language[$"{langPath}._Description"], url),
            buttons: PopupButtons.LearnMore_Close);


        if (result == DialogResult.OK)
        {
            Helpers.OpenUrl(url, "igcmd_invalid_command");
        }

        return (int)IgExitCode.Error;
    }
}