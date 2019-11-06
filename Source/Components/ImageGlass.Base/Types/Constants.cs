using System.Globalization;
using System.Windows.Forms;

namespace ImageGlass.Base
{
    public static class Constants
    {
        public const int MENU_ICON_HEIGHT = 21;
        public const int TOOLBAR_ICON_HEIGHT = 20;
        public const int TOOLBAR_HEIGHT = 40;
        public const int VIEWER_GRID_SIZE = 8;

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
