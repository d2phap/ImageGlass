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
using System.Diagnostics;

namespace igcmd;

internal static class Program
{
    /// <summary>
    /// Gets the command-line arguments after filtering out the configs.
    /// </summary>
    public static string[] CmdArgs
    {
        get
        {
            // remove the command lines begin with '/'
            // example: igcmd.exe /EnableWindowTopMost=True --ui
            // returns: --ui
            var Args = Environment.GetCommandLineArgs()
                .Skip(1) // skip the exe itself path
                .Where(cmd => !cmd.StartsWith(Const.CONFIG_CMD_PREFIX))
                .ToArray() ?? Array.Empty<string>();

            return Args;
        }
    }

    /// <summary>
    /// Show error UI
    /// </summary>
    public static bool ShowUi => CmdArgs.Contains(IgCommands.SHOW_UI);

    /// <summary>
    /// Hide admin error UI
    /// </summary>
    public static bool HideAdminRequiredErrorUi => CmdArgs.Contains(IgCommands.HIDE_ADMIN_REQUIRED_ERROR_UI);


    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static int Main(string[] args)
    {
        #region Form configs

        // Issue #360: IG periodically searching for dismounted device.
        WindowApi.SetAppErrorMode();

        // To customize application configuration such as set high DPI settings
        // or default font, see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();


        // App-level exception handler for non-debugger
        if (!Debugger.IsAttached)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => Config.HandleException((Exception)e.ExceptionObject);

            Application.ThreadException += (object sender, ThreadExceptionEventArgs e) => Config.HandleException(e.Exception);
        }

        #endregion


        // load application configs
        Config.Load();
        Config.Theme.LoadTheme();

        if (CmdArgs.Length == 0)
        {
            return Config.ShowDefaultIgCommandError(nameof(igcmd));
        }

        var topCmd = CmdArgs[0].ToLower().Trim();


        #region SET_WALLPAPER <string imgPath> [int style]
        if (topCmd == IgCommands.SET_WALLPAPER)
        {
            if (CmdArgs.Length < 3)
            {
                return Config.ShowDefaultIgCommandError(nameof(igcmd));
            }

            return (int)Functions.SetDesktopWallpaper(CmdArgs[1], CmdArgs[2]);
        }
        #endregion


        #region SET_DEFAULT_PHOTO_VIEWER [string exts]
        if (topCmd == IgCommands.SET_DEFAULT_PHOTO_VIEWER)
        {
            var exts = "";
            if (CmdArgs.Length > 1)
            {
                exts = CmdArgs[1];
            }

            return (int)Functions.SetAppExtensions(true, exts, ShowUi, HideAdminRequiredErrorUi);
        }
        #endregion


        #region REMOVE_DEFAULT_PHOTO_VIEWER [string exts]
        if (topCmd == IgCommands.REMOVE_DEFAULT_PHOTO_VIEWER)
        {
            var exts = "";
            if (CmdArgs.Length > 1)
            {
                exts = CmdArgs[1];
            }

            return (int)Functions.SetAppExtensions(false, exts, ShowUi, HideAdminRequiredErrorUi);
        }
        #endregion


        #region START_SLIDESHOW <string filePath>
        if (topCmd == IgCommands.START_SLIDESHOW)
        {
            if (CmdArgs.Length > 1)
            {
                Application.Run(new Tools.FrmSlideshow(CmdArgs[1]));

                return (int)IgExitCode.Done;
            }

            return (int)IgExitCode.Error;
        }
        #endregion


        #region EXPORT_FRAMES <string filePath>
        if (topCmd == IgCommands.EXPORT_FRAMES)
        {
            if (CmdArgs.Length > 1)
            {
                return (int)Functions.ExportImageFrames(CmdArgs[1]);
            }

            return (int)IgExitCode.Error;
        }
        #endregion


        #region CHECK_FOR_UPDATE
        if (topCmd == IgCommands.CHECK_FOR_UPDATE)
        {
            Application.Run(new Tools.FrmUpdate());

            return (int)IgExitCode.Done;
        }
        #endregion


        #region QUICK_SETUP
        if (topCmd == IgCommands.QUICK_SETUP)
        {
            Application.Run(new Tools.FrmQuickSetup());

            return (int)IgExitCode.Done;
        }
        #endregion


        #region INSTALL_LANGUAGES [string filePaths]
        if (topCmd == IgCommands.INSTALL_LANGUAGES)
        {
            var paths = CmdArgs.Where(cmd => File.Exists(cmd));
            if (paths.Any())
            {
                return (int)Functions.InstallLanguagePacks(paths.ToList());
            }
        }
        #endregion


        #region INSTALL_THEMES [string filePaths]
        if (topCmd == IgCommands.INSTALL_THEMES)
        {
            var paths = CmdArgs.Where(cmd => File.Exists(cmd));
            if (paths.Any())
            {
                return (int)Functions.InstallThemePacks(paths.ToList());
            }
        }
        #endregion


        #region UNINSTALL_THEME <string filePath>
        if (topCmd == IgCommands.UNINSTALL_THEME)
        {
            var paths = CmdArgs.Where(cmd => Directory.Exists(cmd));
            if (paths.Any())
            {
                return (int)Functions.UninstallThemePack(paths.FirstOrDefault());
            }

            return (int)IgExitCode.Error;
        }
        #endregion


        #region SET_LOCK_SCREEN <string imgPath>
        if (topCmd == IgCommands.SET_LOCK_SCREEN)
        {
            if (args.Length < 2)
            {
                return Config.ShowDefaultIgCommandError(nameof(igcmd));
            }

            return (int)Functions.SetLockScreenBackground(args[1]);
        }
        #endregion


        return (int)IgExitCode.Error;
    }


}