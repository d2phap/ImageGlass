using System.Globalization;
using System.Windows.Forms;

namespace ImageGlass.Settings
{
    public static class Constants
    {
        /// <summary>
        /// First launch version constant. 
        /// If the value read from config file is less than this value, 
        /// the First-Launch Configs screen will be launched.
        /// </summary>
        public const int FIRST_LAUNCH_VERSION = 5;


        /// <summary>
        /// The URI Scheme to register web-to-app linking
        /// </summary>
        public const string URI_SCHEME = "imageglass";


        /// <summary>
        /// Gets the application version
        /// </summary>
        public static string AppVersion { get => Application.ProductVersion; }


        /// <summary>
        /// Gets the application version
        /// </summary>
        public static string IGExePath { get => App.StartUpDir("ImageGlass.exe"); }


        /// <summary>
        /// Number format to use for save/restore ImageGlass settings
        /// </summary>
        public static NumberFormatInfo NumberFormat
        {
            get => new NumberFormatInfo
            {
                NegativeSign = "-"
            };
        }
    }
}
