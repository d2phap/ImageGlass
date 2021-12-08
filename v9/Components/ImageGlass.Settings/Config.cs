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

        #region Boolean items
        /// <summary>
        /// Gets, sets value of slideshow state
        /// </summary>
        public static bool IsSlideshow { get; set; } = false;

        /// <summary>
        /// Gets, sets value if the countdown timer is shown or not
        /// </summary>
        public static bool IsShowSlideshowCountdown { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating whether the slide show interval is random
        /// </summary>
        public static bool IsRandomSlideshowInterval { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicating whether the window is full screen or not
        /// </summary>
        public static bool IsFullScreen { get; set; } = false;

        /// <summary>
        /// Gets, sets value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail { get; set; } = false;

        /// <summary>
        /// Gets, sets the value that indicates if the default position of image in the viewer is center or top left
        /// </summary>
        public static bool IsCenterImage { get; set; } = true;

        /// <summary>
        /// Check if user wants to display RGBA color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerRGBA { get; set; } = true;

        /// <summary>
        /// Check if user wants to display HEX with Alpha color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerHEXA { get; set; } = true;

        /// <summary>
        /// Check if user wants to display HSL with Alpha color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerHSLA { get; set; } = true;

        /// <summary>
        /// Check if user wants to display HSV with Alpha color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerHSVA { get; set; } = true;

        /// <summary>
        /// Gets, sets welcome picture value
        /// </summary>
        public static bool IsShowWelcome { get; set; } = true;

        /// <summary>
        /// Gets, sets value of visibility of toolbar when start up
        /// </summary>
        public static bool IsShowToolBar { get; set; } = true;

        /// <summary>
        /// Gets, sets value whether thumbnail scrollbars visible
        /// </summary>
        public static bool IsShowThumbnailScrollbar { get; set; } = false;

        /// <summary>
        /// Gets, sets value that allows user to loop back to the first image when reaching the end of list
        /// </summary>
        public static bool IsLoopBackSlideshow { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
        /// </summary>
        public static bool IsLoopBackViewer { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that allow quit application by ESC
        /// </summary>
        public static bool IsPressESCToQuit { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that checker board is shown or not
        /// </summary>
        public static bool IsShowCheckerBoard { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicating that multi instances is allowed or not
        /// </summary>
        public static bool IsAllowMultiInstances { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that FrmMain is always on top or not.
        /// </summary>
        public static bool IsWindowAlwaysOnTop { get; set; } = false;

        /// <summary>
        /// Gets, sets value of FrmMain's frameless mode.
        /// </summary>
        public static bool IsWindowFrameless { get; set; } = false;

        /// <summary>
        /// Gets, sets the direction of thumbnail bar
        /// </summary>
        public static bool IsThumbnailHorizontal { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
        /// </summary>
        public static bool IsConfirmationDelete { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that viewer scrollbars are visible
        /// </summary>
        public static bool IsScrollbarsVisible { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that the viewing image is auto-saved after rotating
        /// </summary>
        public static bool IsSaveAfterRotating { get; set; } = false;

        /// <summary>
        /// Gets, sets the setting to control whether the image's original modified date value is preserved on save
        /// </summary>
        public static bool IsPreserveModifiedDate { get; set; } = true;

        /// <summary>
        /// Gets, sets the value indicates that there is a new version
        /// </summary>
        public static bool IsNewVersionAvailable { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that to show full image path or only base name
        /// </summary>
        public static bool IsDisplayBasenameOfImage { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that to toolbar buttons to be centered horizontally
        /// </summary>
        public static bool IsCenterToolbar { get; set; } = true;

        /// <summary>
        /// Gets, sets the value indicates that to show last seen image on startup
        /// </summary>
        public static bool IsOpenLastSeenImage { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that the ColorProfile will be applied for all or only the images with embedded profile
        /// </summary>
        public static bool IsApplyColorProfileForAll { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates whether to show or hide the Navigation Buttons on viewer
        /// </summary>
        public static bool IsShowNavigationButtons { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates whether to show checkerboard in the image region only
        /// </summary>
        public static bool IsShowCheckerboardOnlyImageRegion { get; set; } = false;

        /// <summary>
        /// Gets, sets recursive value
        /// </summary>
        public static bool IsRecursiveLoading { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
        /// </summary>
        public static bool IsUseFileExplorerSortOrder { get; set; } = true;

        /// <summary>
        /// Gets, sets the value indicates that images order should be grouped by directory
        /// </summary>
        public static bool IsGroupImagesByDirectory { get; set; } = false;

        /// <summary>
        /// Gets, sets showing/loading hidden images
        /// </summary>
        public static bool IsShowingHiddenImages { get; set; } = false;

        /// <summary>
        /// Gets, sets value that indicates frmColorPicker tool will be open on startup
        /// </summary>
        public static bool IsShowColorPickerOnStartup { get; set; } = false;

        /// <summary>
        /// Gets, sets value that indicates frmPageNav tool will be open on startup
        /// </summary>
        public static bool IsShowPageNavOnStartup { get; set; } = false;

        /// <summary>
        /// Gets, sets value that indicates page navigation tool auto-show on the multiple pages image
        /// </summary>
        public static bool IsShowPageNavAuto { get; set; } = false;

        /// <summary>
        /// Gets, sets value specifying that Window Fit mode is on
        /// </summary>
        public static bool IsWindowFit { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicates the window should be always center in Window Fit mode
        /// </summary>
        public static bool IsCenterWindowFit { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicates that toast messages will show
        /// </summary>
        public static bool IsShowToast { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicates that touch gesture support enabled
        /// </summary>
        public static bool IsUseTouchGesture { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicates that tooltips are to be hidden
        /// </summary>
        public static bool IsHideTooltips { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicates that FrmExifTool always show on top
        /// </summary>
        public static bool IsExifToolAlwaysOnTop { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicates that to keep the title bar of FrmMain empty
        /// </summary>
        public static bool IsUseEmptyTitleBar { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicates that RAW thumbnail will be use if found
        /// </summary>
        public static bool IsUseRawThumbnail { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicates that the toolbar should be hidden in Full screen mode
        /// </summary>
        public static bool IsHideToolbarInFullscreen { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicates that the thumbnail bar should be hidden in Full screen mode
        /// </summary>
        public static bool IsHideThumbnailBarInFullscreen { get; set; } = false;

        #endregion



        #region Number items

        /// <summary>
        /// Gets, sets the version that requires to launch First-Launch Configs screen
        /// </summary>
        public static int FirstLaunchVersion { get; set; } = 0;

        /// <summary>
        /// Gets, sets slide show interval (minimum value if it's random)
        /// </summary>
        public static uint SlideShowInterval { get; set; } = 5;

        /// <summary>
        /// Gets, sets the maximum slide show interval value
        /// </summary>
        public static uint SlideShowIntervalTo { get; set; } = 5;

        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static uint ThumbnailDimension { get; set; } = 96;

        /// <summary>
        /// Gets, sets width of horizontal thumbnail bar
        /// </summary>
        public static uint ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(ThumbnailDimension, true).GetTotalDimension();

        /// <summary>
        /// Gets, sets the number of images cached by Image
        /// </summary>
        public static uint ImageBoosterCachedCount { get; set; } = 1;

        /// <summary>
        /// Gets, sets fixed width on zooming
        /// </summary>
        public static double ZoomLockValue { get; set; } = 100f;

        /// <summary>
        /// Gets, sets toolbar icon height
        /// </summary>
        public static uint ToolbarIconHeight { get; set; } = Constants.DEFAULT_TOOLBAR_ICON_HEIGHT;

        /// <summary>
        /// Gets, sets value of image quality for editting
        /// </summary>
        public static int ImageEditQuality { get; set; } = 100;

        #endregion





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
            settings.TryAdd(nameof(IsWindowAlwaysOnTop), IsWindowAlwaysOnTop.ToString());
            settings.TryAdd(nameof(IsFullScreen), IsFullScreen.ToString());


            return settings;
        }

        #endregion


    }
}

