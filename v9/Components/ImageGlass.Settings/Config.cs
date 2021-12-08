using ImageGlass.Base;
using Microsoft.Extensions.Configuration;
using System.Dynamic;


namespace ImageGlass.Settings
{
    /// <summary>
    /// Provides app configuration
    /// </summary>
    public class Config
    {

        #region Internal properties
        private static readonly Source _source = new();


        #endregion


        #region Setting items

        /// <summary>
        /// Gets, sets 'Left' position of WinMain
        /// </summary>
        public static int FrmMainPositionX { get; set; } = 200;

        /// <summary>
        /// Gets, sets 'Top' position of WinMain
        /// </summary>
        public static int FrmMainPositionY { get; set; } = 200;

        /// <summary>
        /// Gets, sets width of WinMain
        /// </summary>
        public static int FrmMainWidth { get; set; } = 1200;

        /// <summary>
        /// Gets, sets height of WinMain
        /// </summary>
        public static int FrmMainHeight { get; set; } = 800;

        /// <summary>
        /// Gets, sets window state of WinMain
        /// </summary>
        public static WindowState FrmMainState { get; set; } = WindowState.Normal;

        /// <summary>
        /// Gets, sets window top most state
        /// </summary>
        public static bool IsAlwaysOnTop { get; set; } = false;

        /// <summary>
        /// Gets, sets window Full screen mode
        /// </summary>
        public static bool IsFullScreen { get; set; } = false;


        #endregion




        #region Public functions

        /// <summary>
        /// Loads and parsse configs from file
        /// </summary>
        public static void Load()
        {
            var items = _source.LoadUserConfigs();

            // Number values
            FrmMainPositionX = items.GetValue(nameof(FrmMainPositionX), FrmMainPositionX);
            FrmMainPositionY = items.GetValue(nameof(FrmMainPositionY), FrmMainPositionY);
            FrmMainWidth = items.GetValue(nameof(FrmMainWidth), FrmMainWidth);
            FrmMainHeight = items.GetValue(nameof(FrmMainHeight), FrmMainHeight);

            // Boolean values
            IsAlwaysOnTop = items.GetValue(nameof(IsAlwaysOnTop), IsAlwaysOnTop);
            IsFullScreen = items.GetValue(nameof(IsFullScreen), IsFullScreen);

            // String values


            // Enum values
            FrmMainState = items.GetValue(nameof(FrmMainState), FrmMainState);

        }


        /// <summary>
        /// Parses and writes configs to file
        /// </summary>
        public static void Write()
        {
            var jsonFile = App.ConfigDir(PathType.File, Source.UserFilename);
            Helpers.WriteJson(jsonFile, GetSettingObjects());
        }

        #endregion


        #region Private functions

        /// <summary>
        /// Converts all settings to ExpandoObject for Json parsing
        /// </summary>
        /// <returns></returns>
        private static dynamic GetSettingObjects()
        {
            var settings = new ExpandoObject();

            var infoJson = new
            {
                _source.Description,
                _source.Version
            };

            settings.TryAdd("Info", infoJson);


            // Number values
            settings.TryAdd(nameof(FrmMainPositionX), FrmMainPositionX);
            settings.TryAdd(nameof(FrmMainPositionY), FrmMainPositionY);
            settings.TryAdd(nameof(FrmMainWidth), FrmMainWidth);
            settings.TryAdd(nameof(FrmMainHeight), FrmMainHeight);

            // Enum values
            settings.TryAdd(nameof(FrmMainState), FrmMainState.ToString());

            // String values

            // Boolean values
            settings.TryAdd(nameof(IsAlwaysOnTop), IsAlwaysOnTop.ToString());
            settings.TryAdd(nameof(IsFullScreen), IsFullScreen.ToString());


            return settings;
        }

        #endregion


    }
}

