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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.InstanceManagement;
using ImageGlass.Base.Update;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using System.Globalization;

namespace ImageGlass;

internal static class Program
{
    public const string APP_GUID = "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";
    private static FrmMain? FormMain;


    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Issue #360: IG periodically searching for dismounted device.
        WindowApi.SetAppErrorMode();

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // App-level exception handler
        AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => HandleException((Exception)e.ExceptionObject);
        Application.ThreadException += (object sender, ThreadExceptionEventArgs e) => HandleException(e.Exception);

        // load application configs
        Config.Load();

        // check and run auto-update
        CheckAndRunAutoUpdate();

        // checks and runs app instance(s)
        RunAppInstances();
    }


    static void HandleException(Exception ex)
    {
        var appInfo = Application.ProductName + " v" + Application.ProductVersion.ToString();
        var osInfo = Environment.OSVersion.VersionString + " " + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");

        var btnContinue = new TaskDialogButton("Continue", allowCloseDialog: true);
        var btnCopy = new TaskDialogButton("Copy", allowCloseDialog: false);
        var btnQuit = new TaskDialogButton("Quit", allowCloseDialog: true);


        btnCopy.Click += (object? sender, EventArgs e) => Clipboard.SetText(
            ex.Message + "\r\n" +
            "\r\n" +
            appInfo + "\r\n" +
            osInfo + "\r\n" +
            "\r\n" +
            ex.ToString()
        );
        btnQuit.Click += (object? sender, EventArgs e) => Application.Exit();


        TaskDialog.ShowDialog(new()
        {
            Icon = TaskDialogIcon.Error,
            Caption = "Error",

            Heading = ex.Message,
            Text = "Unhandled exception has occurred.\r\n" +
                "Click" + "\r\n" +
                "- 'Continue' to ignore this error," + "\r\n" +
                "- 'Copy' to copy the error details for bug report, or " + "\r\n" +
                "- 'Quit' to exit the application.\r\n\r\n" +
                appInfo + "\r\n" +
                osInfo,

            Expander = new(ex.ToString()),
            Buttons = new TaskDialogButtonCollection { btnContinue, btnCopy, btnQuit },
            AllowCancel = true,
        });
    }


    /// <summary>
    /// Checks and runs auto-update.
    /// </summary>
    private static void CheckAndRunAutoUpdate()
    {
        if (Config.AutoUpdate != "0")
        {
            if (DateTime.TryParseExact(
                Config.AutoUpdate,
                Constants.DATETIME_FORMAT,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var lastUpdate))
            {
                // Check for update every 5 days
                if (DateTime.Now.Subtract(lastUpdate).TotalDays > 5)
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
            await updater.GetUpdates();


            // There is a newer version
            Config.IsNewVersionAvailable = updater.HasNewUpdate;

            // save last update
            Config.AutoUpdate = DateTime.Now.ToString(Constants.DATETIME_FORMAT);


            if (updater.HasNewUpdate || showIfNewUpdate.Value)
            {
                var archInfo = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                var appVersion = App.Version + $" ({archInfo})";

                var btnWhatNew = new TaskDialogButton(updater.HasNewUpdate ? "What's new?" : "Read changelog", allowCloseDialog: true);
                var btnClose = new TaskDialogButton("Close", allowCloseDialog: true);

                btnWhatNew.Click += (object? sender, EventArgs e) =>
                {
                    Helpers.OpenUrl(updater.CurrentReleaseInfo?.ChangelogUrl.ToString(), "app_update");
                };


                _ = TaskDialog.ShowDialog(new()
                {
                    Caption = $"Check for update",
                    Icon = TaskDialogIcon.Information,

                    Heading = updater.HasNewUpdate
                            ? "There is a new update! 🙂"
                            : "You are using the latest version! 🙃",

                    Text = $"{updater.CurrentReleaseInfo?.Title}\r\n" +
                        $"----------------------\r\n" +
                        $"{updater.CurrentReleaseInfo?.Description}\r\n" +
                        $"\r\n" +
                        $"Version: {appVersion}\r\n" +
                        $"Published date: {updater.CurrentReleaseInfo?.PublishedDate.ToString(Constants.DATETIME_FORMAT)}",

                    Buttons = new TaskDialogButtonCollection { btnWhatNew, btnClose },
                    AllowCancel = true,
                });
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
            Application.Run(FormMain = new FrmMain());
        }
        else
        {
            var guid = new Guid(APP_GUID);

            // single instance is required
            using var instance = new SingleInstance(guid);
            if (instance.IsFirstInstance)
            {
                instance.ArgsReceived += Instance_ArgumentsReceived; ;
                instance.ListenForArgsFromChildInstances();

                Application.Run(FormMain = new FrmMain());
            }
            else
            {
                _ = instance.PassArgsToFirstInstanceAsync(Environment.GetCommandLineArgs());
            }
        }
    }

    private static void Instance_ArgumentsReceived(object? sender, ArgsReceivedEventArgs e)
    {
        if (FormMain == null) return;


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
        FormMain.Invoke(ActivateWindow, (object)realArgs);
    }


    /// <summary>
    /// Pass arguments and activate the main window
    /// </summary>
    /// <param name="args"></param>
    private static void ActivateWindow(string[] args)
    {
        if (FormMain == null) return;

        // load image file from arg
        FormMain.LoadImagesFromCmdArgs(args);

        // Issues #774, #855 : if IG is normal or maximized, do nothing. If IG is minimized,
        // restore it to previous state.
        if (FormMain.WindowState == FormWindowState.Minimized)
        {
            WindowApi.ShowAppWindow(FormMain.Handle, WindowApi.ShowWindowCommands.SW_RESTORE);
        }
        else
        {
            // Hack for issue #620: IG does not activate in normal / maximized window state
            FormMain.TopMost = true;
            WindowApi.ClickOnWindow(FormMain.Handle, new(0, 0));
            FormMain.TopMost = Config.EnableWindowAlwaysOnTop;
        }
    }

}