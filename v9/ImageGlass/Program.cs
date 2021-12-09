using ImageGlass.Settings;

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
}