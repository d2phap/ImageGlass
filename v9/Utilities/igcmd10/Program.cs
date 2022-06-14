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
using System.Diagnostics;

namespace igcmd10;

internal static class Program
{
    public static string[] Args = Array.Empty<string>();


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
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);


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
        Args = args;

        if (args.Length == 0)
        {
            return Config.ShowDefaultIgCommandError(nameof(igcmd10));
        }

        var topCmd = args[0].ToLower().Trim();


        #region SHARE <string[] imgPath>
        if (topCmd == IgCommands.SHARE)
        {
            if (args.Length < 2) return (int)IgExitCode.Error;

            Application.Run(new FrmShare());

            return (int)IgExitCode.Done;
        }
        #endregion


        #region SET_LOCK_SCREEN <string imgPath>
        if (topCmd == IgCommands.SET_LOCK_SCREEN)
        {
            if (args.Length < 2)
            {
                return Config.ShowDefaultIgCommandError(nameof(igcmd10));
            }

            return (int)Functions.SetLockScreenBackground(args[1]);
        }
        #endregion


        return (int)IgExitCode.Error;
    }
}