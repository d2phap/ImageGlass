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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.InstanceManagement;
using ImageGlass.Base.Update;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using System.Diagnostics;

namespace ImageGlass;

internal static class Program
{
    public static string APP_SINGLE_INSTANCE_ID => "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";


    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        #region App configs

        // Issue #360: IG periodically searching for dismounted device.
        WindowApi.SetAppErrorMode();

        ApplicationConfiguration.Initialize();


        // App-level exception handler for non-debugger
        if (!Debugger.IsAttached)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => Config.HandleException((Exception)e.ExceptionObject);

            Application.ThreadException += (object sender, ThreadExceptionEventArgs e) => Config.HandleException(e.Exception);
        }

        #endregion


        try
        {
            // load application configs
            Config.Load();
        }
        catch (Exception ex)
        {
            Config.Language = [];

            Config.ShowError(null, title: Config.Language["_._Error"],
                heading: "Could not load user settings",
                description: $"The user configuration file:\r\n{App.ConfigDir(PathType.File, Source.UserFilename)}\r\n\r\nappears to be corrupted or contains an invalid value. Please review the details below to address the issue before proceeding.",
                details: ex.ToString(), buttons: PopupButton.Close);

            return;
        }

        // check and run Quick setup
        if (CheckAndRunQuickSetup()) return;

        // check and run auto-update
        CheckAndRunAutoUpdate();

        // checks and runs app instance(s)
        RunAppInstances();
    }


    /// <summary>
    /// Checks if the Quick setup dialog should be opened.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    ///   <item><c>true</c> if the Quick setup is required.</item>
    ///   <item><c>false</c> if the Quick setup is not required.</item>
    /// </list>
    /// </returns>
    public static bool CheckAndRunQuickSetup()
    {
        var requiredQuickSetup = false;

        if (Config.QuickSetupVersion < Const.QUICK_SETUP_VERSION)
        {
            FrmMain.IG_OpenQuickSetupDialog();

            requiredQuickSetup = true;
            Application.Exit();
        }

        return requiredQuickSetup;
    }



    /// <summary>
    /// Checks and runs auto-update.
    /// </summary>
    private static void CheckAndRunAutoUpdate()
    {
        if (Config.AutoUpdate != "0")
        {
            if (DateTime.TryParse(Config.AutoUpdate, out var lastUpdate))
            {
                // Check for update every 5 days
                if (DateTime.UtcNow.Subtract(lastUpdate).TotalDays > 5)
                {
                    CheckForUpdate(false);
                }
            }
            else
            {
                CheckForUpdate(false);
            }
        }
    }


    /// <summary>
    /// Check for updatae
    /// </summary>
    /// <param name="showIfNewUpdate">
    /// Set to <c>true</c> if you want to show the Update dialog
    /// when there is a new version. Default value is <c>false</c>.
    /// </param>
    public static void CheckForUpdate(bool? showIfNewUpdate = null)
    {
        _ = Task.Run(async () =>
        {
            showIfNewUpdate ??= false;

            var updater = new UpdateService();
            await updater.GetUpdatesAsync();


            // There is a newer version
            Config.ShowNewVersionIndicator = updater.HasNewUpdate;

            // save last update
            Config.AutoUpdate = DateTime.UtcNow.ToISO8601String();


            if (updater.HasNewUpdate || showIfNewUpdate.Value)
            {
                _ = Config.RunIgcmd(IgCommands.CHECK_FOR_UPDATE);
            }
        });
    }


    /// <summary>
    /// Checks and runs app instance(s)
    /// </summary>
    private static void RunAppInstances()
    {
        if (Config.EnableMultiInstances)
        {
            Local.FrmMain?.Dispose();
            Application.Run(Local.FrmMain = new FrmMain());
        }
        else
        {
            // single instance is required
            using var instance = new SingleInstance(APP_SINGLE_INSTANCE_ID);
            if (instance.IsFirstInstance)
            {
                instance.ArgsReceived += Instance_ArgumentsReceived;
                instance.ListenForArgsFromChildInstances();

                Local.FrmMain?.Dispose();
                Application.Run(Local.FrmMain = new FrmMain());
            }
            else
            {
                _ = instance.PassArgsToFirstInstanceAsync(Environment.GetCommandLineArgs());
            }
        }
    }

    private static void Instance_ArgumentsReceived(object? sender, ArgsReceivedEventArgs e)
    {
        if (Local.FrmMain == null) return;


        // Attempt to run a 2nd instance of IG when multi-instance turned off.
        // The primary instance will crash if no file provided
        // (e.g. by double-clicking on .EXE in explorer).
        var realCount = 0;
        foreach (var arg in e.Arguments)
        {
            if (arg != null)
            {
                realCount++;
            }
        }

        var realArgs = new string[realCount];
        Array.Copy(e.Arguments, realArgs, realCount);

        // Execute our delegate on the forms thread!
        Local.FrmMain.Invoke(ActivateWindow, (object)realArgs);
    }


    /// <summary>
    /// Pass arguments and activate the main window
    /// </summary>
    private static void ActivateWindow(string[] args)
    {
        if (Local.FrmMain == null) return;

        // load image file from arg
        Local.FrmMain.LoadImagesFromCmdArgs(args);

        // Issues #774, #855: if IG is normal or maximized, do nothing. If IG is minimized,
        // restore it to previous state.
        if (Local.FrmMain.WindowState == FormWindowState.Minimized)
        {
            WindowApi.ShowAppWindow(Local.FrmMain.Handle, SHOW_WINDOW_CMD.SW_RESTORE);
        }
        else
        {
            // Hack for issue #620: IG does not activate in normal / maximized window state
            Local.FrmMain.TopMost = true;
            WindowApi.ClickOnWindow(Local.FrmMain.Handle, Local.FrmMain.PointToScreen(new(0, 0)));
            Local.FrmMain.TopMost = Config.EnableWindowTopMost;
        }
    }
}