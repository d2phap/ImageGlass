
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI.BuiltInForms;

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
            return ShowErrorPopup();
        }

        var topCmd = args[0].ToLower().Trim();

        // Set desktop wallpaper: setwallpaper <string imgPath> [int style]
        if (topCmd == "setwallpaper")
        {
            if (args.Length < 3)
            {
                return ShowErrorPopup();
            }

            return (int)Functions.SetDesktopWallpaper(args[1], args[2]);
        }

        return (int)IgExitCode.Error;
    }


    private static int ShowErrorPopup()
    {
        var frm = new Popup(Config.Theme, Config.Language)
        {
            Title = Application.ProductName + " " + Application.ProductVersion,
            Heading = "Invalid commands",
            Description = "This executable file contains command-line functions for ImageGlass software. Please make sure you pass the commands correctly." +
                    "\r\n\r\n" +
                    "To explore all command lines, please visit: \r\n" +
                    "https://imageglass.org/docs/command-line-utilities",

            Thumbnail = SystemIconApi.GetSystemIcon(SHSTOCKICONID.SIID_ERROR),
            ShowTextInput = false,

            AcceptButtonText = "Learn more...",
            CancelButtonText = "Close",
            ShowInTaskbar = true,
        };

        if (frm.ShowDialog() == DialogResult.OK)
        {
            Helpers.OpenUrl("https://imageglass.org/docs/command-line-utilities", "igcmd_invalid_command");
        }

        return (int)IgExitCode.Error;
    }
}