using ImageGlass.Base;
using ImageGlass.Base.Update;
using ImageGlass.Settings;
using System.Globalization;

namespace ImageGlass;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
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

        Application.Run(new FrmMain());
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
    /// <param name="useAutoCheck">If TRUE, use "igautoupdate"; else "igupdate" for argument</param>
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
                });
            }

        });
    }

}