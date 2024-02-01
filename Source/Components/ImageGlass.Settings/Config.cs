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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using DirectN;
using ImageGlass.Base;
using ImageGlass.Base.Actions;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Dynamic;
using System.Media;
using System.Reflection;
using System.Text;

namespace ImageGlass.Settings;

/// <summary>
/// Provides app configuration
/// </summary>
public static class Config
{

    #region Internal properties
    private static readonly Source _source = new();
    private static CancellationTokenSource _requestUpdatingColorModeCancelToken = new();
    private static bool _isDarkMode = WinColorsApi.IsDarkMode;


    /// <summary>
    /// The default image info tags
    /// </summary>
    public static List<string> DefaultImageInfoTags => [
        nameof(ImageInfo.Name),
        nameof(ImageInfo.ListCount),
        nameof(ImageInfo.FrameCount),
        nameof(ImageInfo.Zoom),
        nameof(ImageInfo.Dimension),
        nameof(ImageInfo.FileSize),
        nameof(ImageInfo.ColorSpace),
        nameof(ImageInfo.ExifRating),
        nameof(ImageInfo.DateTimeAuto),
        nameof(ImageInfo.AppName),
    ];


    /// <summary>
    /// Gets, sets current theme.
    /// </summary>
    public static IgTheme Theme { get; set; } = new();


    /// <summary>
    /// Occurs when the system app color is changed and does not match the current <see cref="Theme"/>'s dark mode.
    /// </summary>
    public static event RequestUpdatingColorModeHandler? RequestUpdatingColorMode;
    public delegate void RequestUpdatingColorModeHandler(SystemColorModeChangedEventArgs e);


    /// <summary>
    /// Occurs when the <see cref="Config.Theme"/> is requested to change.
    /// </summary>
    public static event RequestUpdatingThemeHandler? RequestUpdatingTheme;
    public delegate void RequestUpdatingThemeHandler(RequestUpdatingThemeEventArgs e);


    /// <summary>
    /// Occurs when the <see cref="Config.Language"/> is requested to change.
    /// </summary>
    public static event RequestUpdatingLanguageHandler? RequestUpdatingLanguage;
    public delegate void RequestUpdatingLanguageHandler();


    #endregion


    #region Setting items

    /// <summary>
    /// Gets, sets the config section of tool settings.
    /// </summary>
    public static ExpandoObject ToolSettings { get; set; } = new();


    #region Boolean items

    /// <summary>
    /// Gets, sets value indicating whether the slideshow mode is enabled or not.
    /// </summary>
    public static bool EnableSlideshow { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating whether the FrmMain should be hidden when <see cref="EnableSlideshow"/> is on.
    /// </summary>
    public static bool HideMainWindowInSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value if the countdown timer is shown or not.
    /// </summary>
    public static bool ShowSlideshowCountdown { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates whether the slide show interval is random.
    /// </summary>
    public static bool UseRandomIntervalForSlideshow { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that slideshow will loop back to the first image when reaching the end of list.
    /// </summary>
    public static bool EnableLoopSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that slideshow is played in full screen, not window mode.
    /// </summary>
    public static bool EnableFullscreenSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value of FrmMain's frameless mode.
    /// </summary>
    public static bool EnableFrameless { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating whether the full screen mode is enabled or not.
    /// </summary>
    public static bool EnableFullScreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the toolbar should be hidden in Full screen mode
    /// </summary>
    public static bool HideToolbarInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the gallery should be hidden in Full screen mode
    /// </summary>
    public static bool HideGalleryInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value of gallery visibility
    /// </summary>
    public static bool ShowGallery { get; set; } = true;

    /// <summary>
    /// Gets, sets value whether gallery scrollbars visible
    /// </summary>
    public static bool ShowGalleryScrollbars { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that showing image file name on gallery
    /// </summary>
    public static bool ShowGalleryFileName { get; set; } = true;

    /// <summary>
    /// Gets, sets welcome picture value
    /// </summary>
    public static bool ShowWelcomeImage { get; set; } = true;

    /// <summary>
    /// Gets, sets value of visibility of toolbar when start up
    /// </summary>
    public static bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets, sets value of visibility of app icon
    /// </summary>
    public static bool ShowAppIcon { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
    /// </summary>
    public static bool EnableLoopBackNavigation { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that checker board is shown or not
    /// </summary>
    public static bool ShowCheckerboard { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates whether to show checkerboard in the image region only
    /// </summary>
    public static bool ShowCheckerboardOnlyImageRegion { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating that multi instances is allowed or not
    /// </summary>
    public static bool EnableMultiInstances { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that FrmMain is always on top or not.
    /// </summary>
    public static bool EnableWindowTopMost { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
    /// </summary>
    public static bool ShowDeleteConfirmation { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when overriding the viewing image
    /// </summary>
    public static bool ShowSaveOverrideConfirmation { get; set; } = true;

    /// <summary>
    /// Gets, sets the setting to control whether the image's original modified date value is preserved on save
    /// </summary>
    public static bool ShouldPreserveModifiedDate { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that there is a new version
    /// </summary>
    public static bool ShowNewVersionIndicator { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that to toolbar buttons to be centered horizontally
    /// </summary>
    public static bool EnableCenterToolbar { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that to show last seen image on startup
    /// </summary>
    public static bool ShouldOpenLastSeenImage { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that the ColorProfile will be applied for all or only the images with embedded profile
    /// </summary>
    public static bool ShouldUseColorProfileForAll { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates whether to show or hide the Navigation Buttons on viewer
    /// </summary>
    public static bool EnableNavigationButtons { get; set; } = true;

    /// <summary>
    /// Gets, sets recursive value
    /// </summary>
    public static bool EnableRecursiveLoading { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
    /// </summary>
    public static bool ShouldUseExplorerSortOrder { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that images order should be grouped by directory
    /// </summary>
    public static bool ShouldGroupImagesByDirectory { get; set; } = false;

    /// <summary>
    /// Gets, sets showing/loading hidden images
    /// </summary>
    public static bool ShouldLoadHiddenImages { get; set; } = false;

    /// <summary>
    /// Gets, sets value specifying that Window Fit mode is on
    /// </summary>
    public static bool EnableWindowFit { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates the window should be always center in Window Fit mode
    /// </summary>
    public static bool CenterWindowFit { get; set; } = true;

    /// <summary>
    /// Displays the embedded thumbnail for RAW formats if found.
    /// </summary>
    public static bool UseEmbeddedThumbnailRawFormats { get; set; } = false;

    /// <summary>
    /// Displays the embedded thumbnail for other formats if found.
    /// </summary>
    public static bool UseEmbeddedThumbnailOtherFormats { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that image preview is shown while the image is being loaded.
    /// </summary>
    public static bool ShowImagePreview { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that image fading transition is used while it's being loaded.
    /// </summary>
    public static bool EnableImageTransition { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that images should be loaded asynchronously.
    /// </summary>
    public static bool EnableImageAsyncLoading { get; set; } = true;

    /// <summary>
    /// Enables / Disables copy multiple files.
    /// </summary>
    public static bool EnableCopyMultipleFiles { get; set; } = true;

    /// <summary>
    /// Enables / Disables cut multiple files.
    /// </summary>
    public static bool EnableCutMultipleFiles { get; set; } = true;

    /// <summary>
    /// Enables / Disables the file system watcher.
    /// </summary>
    public static bool EnableRealTimeFileUpdate { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that ImageGlass should open the new image file added in the viewing folder.
    /// </summary>
    public static bool ShouldAutoOpenNewAddedImage { get; set; } = false;

    /// <summary>
    /// Uses Webview2 for viewing SVG format.
    /// </summary>
    public static bool UseWebview2ForSvg { get; set; } = true;

    /// <summary>
    /// Enables, disables debug mode.
    /// </summary>
    public static bool EnableDebug { get; set; } = false;

    #endregion // Boolean items


    #region Number items

    /// <summary>
    /// Gets, sets the version that requires to open Quick setup ImageGlass dialog.
    /// </summary>
    public static int QuickSetupVersion { get; set; } = 0;

    /// <summary>
    /// Gets, sets 'Left' position of main window
    /// </summary>
    public static int FrmMainPositionX { get; set; } = 200;

    /// <summary>
    /// Gets, sets 'Top' position of main window
    /// </summary>
    public static int FrmMainPositionY { get; set; } = 200;

    /// <summary>
    /// Gets, sets width of main window
    /// </summary>
    public static int FrmMainWidth { get; set; } = 1300;

    /// <summary>
    /// Gets, sets height of main window
    /// </summary>
    public static int FrmMainHeight { get; set; } = 800;

    /// <summary>
    /// Gets, sets 'Left' position of settings window
    /// </summary>
    public static int FrmSettingsPositionX { get; set; } = 200;

    /// <summary>
    /// Gets, sets 'Top' position of settings window
    /// </summary>
    public static int FrmSettingsPositionY { get; set; } = 200;

    /// <summary>
    /// Gets, sets width of settings window
    /// </summary>
    public static int FrmSettingsWidth { get; set; } = 1300;

    /// <summary>
    /// Gets, sets height of settings window
    /// </summary>
    public static int FrmSettingsHeight { get; set; } = 800;

    /// <summary>
    /// Gets, sets the panning speed.
    /// Value range is from 0 to 100.
    /// </summary>
    public static float PanSpeed { get; set; } = 20f;

    /// <summary>
    /// Gets, sets the zooming speed.
    /// Value range is from -500 to 500.
    /// </summary>
    public static float ZoomSpeed { get; set; } = 0;

    /// <summary>
    /// Gets, sets slide show interval (minimum value if it's random)
    /// </summary>
    public static float SlideshowInterval { get; set; } = 5f;

    /// <summary>
    /// Gets, sets the maximum slide show interval value
    /// </summary>
    public static float SlideshowIntervalTo { get; set; } = 5f;

    /// <summary>
    /// Gets, sets the number of image changes to notify <see cref="SlideshowNotificationSound"/> sound in slideshow mode.
    /// </summary>
    public static int SlideshowImagesToNotifySound { get; set; } = 0;

    /// <summary>
    /// Gets, sets value of thumbnail dimension in pixel
    /// </summary>
    public static int ThumbnailSize { get; set; } = 50;

    /// <summary>
    /// Gets, sets the maximum size in MB of thumbnail persistent cache.
    /// </summary>
    public static int GalleryCacheSizeInMb { get; set; } = 400;

    /// <summary>
    /// Gets, sets number of thumbnail columns displayed in vertical gallery.
    /// </summary>
    public static int GalleryColumns { get; set; } = 3;

    /// <summary>
    /// Gets, sets the number of images cached by <see cref="Base.Services.ImageBooster"/>.
    /// </summary>
    public static int ImageBoosterCacheCount { get; set; } = 1;

    /// <summary>
    /// Gets, sets the maximum image dimension when caching by <see cref="Base.Services.ImageBooster"/>.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public static int ImageBoosterCacheMaxDimension { get; set; } = 8_000;

    /// <summary>
    /// Gets, sets the maximum image file size (in MB) when caching by <see cref="Base.Services.ImageBooster"/>.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public static float ImageBoosterCacheMaxFileSizeInMb { get; set; } = 100f;

    /// <summary>
    /// Gets, sets fixed width on zooming
    /// </summary>
    public static float ZoomLockValue { get; set; } = 100f;

    /// <summary>
    /// Gets, sets toolbar icon height
    /// </summary>
    public static int ToolbarIconHeight { get; set; } = Const.TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Gets, sets value of image quality for editting
    /// </summary>
    public static int ImageEditQuality { get; set; } = 80;

    /// <summary>
    /// Gets, sets value of duration to display the in-app message
    /// </summary>
    public static int InAppMessageDuration { get; set; } = 2000;

    /// <summary>
    /// Gets, sets the minimum width of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="UseEmbeddedThumbnailRawFormats"/> or <see cref="UseEmbeddedThumbnailOtherFormats"/> is <c>true</c>.
    /// </summary>
    public static int EmbeddedThumbnailMinWidth { get; set; } = 0;

    /// <summary>
    /// Gets, sets the minimum height of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="UseEmbeddedThumbnailRawFormats"/> or <see cref="UseEmbeddedThumbnailOtherFormats"/> is <c>true</c>.
    /// </summary>
    public static int EmbeddedThumbnailMinHeight { get; set; } = 0;

    #endregion // Number items


    #region String items

    /// <summary>
    /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
    /// </summary>
    public static string ColorProfile { get; set; } = nameof(ColorProfileOption.CurrentMonitorProfile);

    /// <summary>
    /// Gets, sets the last time to check for update. Set it to <c>0</c> to disable auto-update.
    /// </summary>
    public static string AutoUpdate { get; set; } = DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToISO8601String();

    /// <summary>
    /// Gets, sets the absolute file path of the last seen image
    /// </summary>
    public static string LastSeenImagePath { get; set; } = "";

    /// <summary>
    /// Gets, sets the last view of settings window.
    /// </summary>
    public static string LastOpenedSetting { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets the theme name for dark mode.
    /// </summary>
    public static string DarkTheme { get; set; } = Const.DEFAULT_THEME;

    /// <summary>
    /// Gets, sets the theme name for light mode.
    /// </summary>
    public static string LightTheme { get; set; } = "Kobe-Light";

    #endregion


    #region Array items

    /// <summary>
    /// Gets, sets zoom levels of the viewer
    /// </summary>
    public static float[] ZoomLevels { get; set; } = [];

    /// <summary>
    /// Gets, sets the list of apps for edit action.
    /// </summary>
    public static Dictionary<string, EditApp?> EditApps { get; set; } = [];

    /// <summary>
    /// Gets, sets the list of supported image formats
    /// </summary>
    public static HashSet<string> FileFormats { get; set; } = [];

    /// <summary>
    /// Gets, sets the list of formats that only load the first frame forcefully
    /// </summary>
    public static HashSet<string> SingleFrameFormats { get; set; } = [".avif", ".heic", ".heif", ".psd", ".jxl"];

    /// <summary>
    /// Gets, sets the list of toolbar buttons
    /// </summary>
    public static List<ToolbarItemModel> ToolbarButtons { get; set; } = [];

    /// <summary>
    /// Gets, sets the tags for displaying image info
    /// </summary>
    public static List<string> ImageInfoTags { get; set; } = DefaultImageInfoTags;

    /// <summary>
    /// Gets, sets hotkeys list of menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> MenuHotkeys { get; set; } = [];

    /// <summary>
    /// Gets, sets mouse click actions
    /// </summary>
    public static Dictionary<MouseClickEvent, ToggleAction> MouseClickActions { get; set; } = [];

    /// <summary>
    /// Gets, sets mouse wheel actions
    /// </summary>
    public static Dictionary<MouseWheelEvent, MouseWheelAction> MouseWheelActions { get; set; } = [];

    /// <summary>
    /// Gets, sets layout for FrmMain. Syntax:
    /// <c>Dictionary["ControlName", "DockStyle;order"]</c>
    /// </summary>
    public static Dictionary<string, string?> Layout { get; set; } = [];

    /// <summary>
    /// Gets, sets tools.
    /// </summary>
    public static List<IgTool?> Tools { get; set; } = [
        new IgTool()
        {
            ToolId = Const.IGTOOL_EXIFTOOL,
            ToolName = "ExifGlass - EXIF metadata viewer",
            Executable = "exifglass",
            Argument = Const.FILE_MACRO,
            IsIntegrated = true,
            Hotkeys = [new Hotkey(Keys.X)],
        },
    ];

    /// <summary>
    /// Gets, sets the list of disabled menus
    /// </summary>
    public static List<string> DisabledMenus { get; set; } = [];

    #endregion // Array items


    #region Enum items

    /// <summary>
    /// Gets, sets state of main window
    /// </summary>
    public static WindowState FrmMainState { get; set; } = WindowState.Normal;

    /// <summary>
    /// Gets, sets state of settings window
    /// </summary>
    public static WindowState FrmSettingsState { get; set; } = WindowState.Normal;

    /// <summary>
    /// Gets, sets image loading order
    /// </summary>
    public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;

    /// <summary>
    /// Gets, sets image loading order type
    /// </summary>
    public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;

    /// <summary>
    /// Gets, sets zoom mode value
    /// </summary>
    public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>less than or equals 100%</c>.
    /// </summary>
    public static ImageInterpolation ImageInterpolationScaleDown { get; set; } = ImageInterpolation.MultiSampleLinear;

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    public static ImageInterpolation ImageInterpolationScaleUp { get; set; } = ImageInterpolation.NearestNeighbor;

    /// <summary>
    /// Gets, sets value indicates what happens after clicking Edit menu
    /// </summary>
    public static AfterEditAppAction AfterEditingAction { get; set; } = AfterEditAppAction.Nothing;

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    public static BackdropStyle WindowBackdrop { get; set; } = BackdropStyle.Mica;

    #endregion // Enum items


    #region Other types items

    /// <summary>
    /// Gets, sets background color of of the main window
    /// </summary>
    public static Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets, sets background color of slideshow
    /// </summary>
    public static Color SlideshowBackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets, sets language pack
    /// </summary>
    public static IgLang Language { get; set; }

    #endregion // Other types items

    #endregion // Setting items



    #region Public static functions

    /// <summary>
    /// Loads and parsse configs from file
    /// </summary>
    public static void Load()
    {
#nullable disable
        var items = Source.LoadUserConfigs();

        // save the config for all tools
        ToolSettings = items.GetValueObj(nameof(ToolSettings)).GetValue(nameof(ToolSettings), new ExpandoObject());


        // Boolean values
        #region Boolean items

        EnableSlideshow = items.GetValueEx(nameof(EnableSlideshow), EnableSlideshow);
        HideMainWindowInSlideshow = items.GetValueEx(nameof(HideMainWindowInSlideshow), HideMainWindowInSlideshow);
        ShowSlideshowCountdown = items.GetValueEx(nameof(ShowSlideshowCountdown), ShowSlideshowCountdown);
        UseRandomIntervalForSlideshow = items.GetValueEx(nameof(UseRandomIntervalForSlideshow), UseRandomIntervalForSlideshow);
        EnableLoopSlideshow = items.GetValueEx(nameof(EnableLoopSlideshow), EnableLoopSlideshow);
        EnableFullscreenSlideshow = items.GetValueEx(nameof(EnableFullscreenSlideshow), EnableFullscreenSlideshow);
        EnableFrameless = items.GetValueEx(nameof(EnableFrameless), EnableFrameless);
        EnableFullScreen = items.GetValueEx(nameof(EnableFullScreen), EnableFullScreen);
        ShowGallery = items.GetValueEx(nameof(ShowGallery), ShowGallery);
        ShowGalleryScrollbars = items.GetValueEx(nameof(ShowGalleryScrollbars), ShowGalleryScrollbars);
        ShowGalleryFileName = items.GetValueEx(nameof(ShowGalleryFileName), ShowGalleryFileName);
        ShowWelcomeImage = items.GetValueEx(nameof(ShowWelcomeImage), ShowWelcomeImage);
        ShowToolbar = items.GetValueEx(nameof(ShowToolbar), ShowToolbar);
        ShowAppIcon = items.GetValueEx(nameof(ShowAppIcon), ShowAppIcon);
        EnableLoopBackNavigation = items.GetValueEx(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        ShowCheckerboard = items.GetValueEx(nameof(ShowCheckerboard), ShowCheckerboard);
        ShowCheckerboardOnlyImageRegion = items.GetValueEx(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        EnableMultiInstances = items.GetValueEx(nameof(EnableMultiInstances), EnableMultiInstances);
        EnableWindowTopMost = items.GetValueEx(nameof(EnableWindowTopMost), EnableWindowTopMost);
        ShowDeleteConfirmation = items.GetValueEx(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        ShowSaveOverrideConfirmation = items.GetValueEx(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        ShouldPreserveModifiedDate = items.GetValueEx(nameof(ShouldPreserveModifiedDate), ShouldPreserveModifiedDate);
        ShowNewVersionIndicator = items.GetValueEx(nameof(ShowNewVersionIndicator), ShowNewVersionIndicator);
        EnableCenterToolbar = items.GetValueEx(nameof(EnableCenterToolbar), EnableCenterToolbar);
        ShouldOpenLastSeenImage = items.GetValueEx(nameof(ShouldOpenLastSeenImage), ShouldOpenLastSeenImage);
        ShouldUseColorProfileForAll = items.GetValueEx(nameof(ShouldUseColorProfileForAll), ShouldUseColorProfileForAll);
        EnableNavigationButtons = items.GetValueEx(nameof(EnableNavigationButtons), EnableNavigationButtons);
        EnableRecursiveLoading = items.GetValueEx(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        ShouldUseExplorerSortOrder = items.GetValueEx(nameof(ShouldUseExplorerSortOrder), ShouldUseExplorerSortOrder);
        ShouldGroupImagesByDirectory = items.GetValueEx(nameof(ShouldGroupImagesByDirectory), ShouldGroupImagesByDirectory);
        ShouldLoadHiddenImages = items.GetValueEx(nameof(ShouldLoadHiddenImages), ShouldLoadHiddenImages);
        EnableWindowFit = items.GetValueEx(nameof(EnableWindowFit), EnableWindowFit);
        CenterWindowFit = items.GetValueEx(nameof(CenterWindowFit), CenterWindowFit);
        UseEmbeddedThumbnailRawFormats = items.GetValueEx(nameof(UseEmbeddedThumbnailRawFormats), UseEmbeddedThumbnailRawFormats);
        UseEmbeddedThumbnailOtherFormats = items.GetValueEx(nameof(UseEmbeddedThumbnailOtherFormats), UseEmbeddedThumbnailOtherFormats);
        ShowImagePreview = items.GetValueEx(nameof(ShowImagePreview), ShowImagePreview);
        EnableImageTransition = items.GetValueEx(nameof(EnableImageTransition), EnableImageTransition);
        EnableImageAsyncLoading = items.GetValueEx(nameof(EnableImageAsyncLoading), EnableImageAsyncLoading);
        EnableCopyMultipleFiles = items.GetValueEx(nameof(EnableCopyMultipleFiles), EnableCopyMultipleFiles);
        EnableCutMultipleFiles = items.GetValueEx(nameof(EnableCutMultipleFiles), EnableCutMultipleFiles);
        EnableRealTimeFileUpdate = items.GetValueEx(nameof(EnableRealTimeFileUpdate), EnableRealTimeFileUpdate);
        ShouldAutoOpenNewAddedImage = items.GetValueEx(nameof(ShouldAutoOpenNewAddedImage), ShouldAutoOpenNewAddedImage);
        UseWebview2ForSvg = items.GetValueEx(nameof(UseWebview2ForSvg), UseWebview2ForSvg);
        EnableDebug = items.GetValueEx(nameof(EnableDebug), EnableDebug);

        HideToolbarInFullscreen = items.GetValueEx(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        HideGalleryInFullscreen = items.GetValueEx(nameof(HideGalleryInFullscreen), HideGalleryInFullscreen);

        #endregion


        // Number values
        #region Number items

        QuickSetupVersion = items.GetValueEx(nameof(QuickSetupVersion), QuickSetupVersion);

        // FrmMain
        FrmMainPositionX = items.GetValueEx(nameof(FrmMainPositionX), FrmMainPositionX);
        FrmMainPositionY = items.GetValueEx(nameof(FrmMainPositionY), FrmMainPositionY);
        FrmMainWidth = items.GetValueEx(nameof(FrmMainWidth), FrmMainWidth);
        FrmMainHeight = items.GetValueEx(nameof(FrmMainHeight), FrmMainHeight);

        // FrmSettings
        FrmSettingsPositionX = items.GetValueEx(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        FrmSettingsPositionY = items.GetValueEx(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        FrmSettingsWidth = items.GetValueEx(nameof(FrmSettingsWidth), FrmSettingsWidth);
        FrmSettingsHeight = items.GetValueEx(nameof(FrmSettingsHeight), FrmSettingsHeight);

        PanSpeed = items.GetValueEx(nameof(PanSpeed), PanSpeed);
        ZoomSpeed = items.GetValueEx(nameof(ZoomSpeed), ZoomSpeed);

        #region Slideshow
        SlideshowInterval = items.GetValueEx(nameof(SlideshowInterval), SlideshowInterval);
        if (SlideshowInterval < 1) SlideshowInterval = 5f;

        SlideshowIntervalTo = items.GetValueEx(nameof(SlideshowIntervalTo), SlideshowIntervalTo);
        SlideshowIntervalTo = Math.Max(SlideshowIntervalTo, SlideshowInterval);

        SlideshowImagesToNotifySound = items.GetValueEx(nameof(SlideshowImagesToNotifySound), SlideshowImagesToNotifySound);
        #endregion

        #region Load gallery thumbnail width & position
        ThumbnailSize = items.GetValueEx(nameof(ThumbnailSize), ThumbnailSize);
        ThumbnailSize = Math.Max(20, ThumbnailSize);
        GalleryCacheSizeInMb = items.GetValueEx(nameof(GalleryCacheSizeInMb), GalleryCacheSizeInMb);

        GalleryColumns = items.GetValueEx(nameof(GalleryColumns), GalleryColumns);
        GalleryColumns = Math.Max(1, GalleryColumns);
        #endregion

        ImageBoosterCacheCount = items.GetValueEx(nameof(ImageBoosterCacheCount), ImageBoosterCacheCount);
        ImageBoosterCacheCount = Math.Max(0, Math.Min(ImageBoosterCacheCount, 10));

        ImageBoosterCacheMaxDimension = items.GetValueEx(nameof(ImageBoosterCacheMaxDimension), ImageBoosterCacheMaxDimension);
        ImageBoosterCacheMaxFileSizeInMb = items.GetValueEx(nameof(ImageBoosterCacheMaxFileSizeInMb), ImageBoosterCacheMaxFileSizeInMb);

        ZoomLockValue = items.GetValueEx(nameof(ZoomLockValue), ZoomLockValue);
        if (ZoomLockValue < 0) ZoomLockValue = 100f;

        ToolbarIconHeight = items.GetValueEx(nameof(ToolbarIconHeight), ToolbarIconHeight);
        ImageEditQuality = items.GetValueEx(nameof(ImageEditQuality), ImageEditQuality);
        InAppMessageDuration = items.GetValueEx(nameof(InAppMessageDuration), InAppMessageDuration);
        EmbeddedThumbnailMinWidth = items.GetValueEx(nameof(EmbeddedThumbnailMinWidth), EmbeddedThumbnailMinWidth);
        EmbeddedThumbnailMinHeight = items.GetValueEx(nameof(EmbeddedThumbnailMinHeight), EmbeddedThumbnailMinHeight);

        #endregion


        // Enum values
        #region Enum items

        FrmMainState = items.GetValueEx(nameof(FrmMainState), FrmMainState);
        FrmSettingsState = items.GetValueEx(nameof(FrmSettingsState), FrmSettingsState);
        ImageLoadingOrder = items.GetValueEx(nameof(ImageLoadingOrder), ImageLoadingOrder);
        ImageLoadingOrderType = items.GetValueEx(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
        ZoomMode = items.GetValueEx(nameof(ZoomMode), ZoomMode);
        ImageInterpolationScaleDown = items.GetValueEx(nameof(ImageInterpolationScaleDown), ImageInterpolationScaleDown);
        ImageInterpolationScaleUp = items.GetValueEx(nameof(ImageInterpolationScaleUp), ImageInterpolationScaleUp);
        AfterEditingAction = items.GetValueEx(nameof(AfterEditingAction), AfterEditingAction);
        WindowBackdrop = items.GetValueEx(nameof(WindowBackdrop), WindowBackdrop);

        #endregion


        // String values
        #region String items

        ColorProfile = items.GetValueEx(nameof(ColorProfile), ColorProfile);
        ColorProfile = BHelper.GetCorrectColorProfileName(ColorProfile);

        AutoUpdate = items.GetValueEx(nameof(AutoUpdate), AutoUpdate);
        LastSeenImagePath = items.GetValueEx(nameof(LastSeenImagePath), LastSeenImagePath);
        LastOpenedSetting = items.GetValueEx(nameof(LastOpenedSetting), LastOpenedSetting);
        DarkTheme = items.GetValueEx(nameof(DarkTheme), DarkTheme);
        LightTheme = items.GetValueEx(nameof(LightTheme), LightTheme);

        #endregion


        // Array values
        #region Array items

        // ZoomLevels
        ZoomLevels = items.GetSection(nameof(ZoomLevels))
            .GetChildren()
            .Select(i =>
            {
                // convert % to float
                try { return i.Get<float>() / 100f; } catch { }
                return -1;
            })
            .OrderBy(i => i)
            .Where(i => i > 0)
            .Distinct()
            .ToArray();

        #region EditApps

        EditApps = items.GetSection(nameof(EditApps))
            .GetChildren()
            .ToDictionary(
                i => i.Key.ToLowerInvariant(),
                i => i.Get<EditApp>()
            );

        #endregion

        #region ImageFormats

        var formats = items.GetValueEx(nameof(FileFormats), Const.IMAGE_FORMATS);
        FileFormats = GetImageFormats(formats);

        formats = items.GetValueEx(nameof(SingleFrameFormats), string.Join(";", SingleFrameFormats));
        SingleFrameFormats = GetImageFormats(formats);

        #endregion


        // toolbar buttons
        var toolbarItems = items.GetSection(nameof(ToolbarButtons))
            .GetChildren()
            .Select(i => i.Get<ToolbarItemModel>());
        ToolbarButtons = toolbarItems.ToList();


        // info items
        var infoTagsHasValue = items.GetValueObj(nameof(ImageInfoTags)) is not null;
        if (infoTagsHasValue)
        {
            ImageInfoTags = items.GetSection(nameof(ImageInfoTags))
                .GetChildren()
                .Select(i => i.Get<string>())
                .ToList();
        }


        // hotkeys for menu
        var stringArrDict = items.GetSection(nameof(MenuHotkeys))
            .GetChildren()
            .ToDictionary(
                i => i.Key,
                i => i.GetChildren().Select(i => i.Value).ToArray()
            );
        MenuHotkeys = ParseHotkeys(stringArrDict);


        // MouseClickActions
        MouseClickActions = items.GetSection(nameof(MouseClickActions))
            .GetChildren()
            .ToDictionary(
                i => BHelper.ParseEnum<MouseClickEvent>(i.Key),
                i => i.Get<ToggleAction>());


        // MouseWheelActions
        MouseWheelActions = items.GetSection(nameof(MouseWheelActions))
            .GetChildren()
            .ToDictionary(
                i => BHelper.ParseEnum<MouseWheelEvent>(i.Key),
                i => BHelper.ParseEnum<MouseWheelAction>(i.Value));

        // Layout
        Layout = items.GetSection(nameof(Layout))
            .GetChildren()
            .ToDictionary(
                i => i.Key,
                i => i.Get<string>()
            );


        // Tools
        var toolList = items.GetSection(nameof(Tools))
            .GetChildren()
            .Select(i =>
            {
                var tool = i.Get<IgTool>();
                var hotkeysArr = i.GetChildren()
                    .FirstOrDefault(i => i.Key == "Hotkeys")
                    ?.Get<string[]>() ?? [];

                tool.Hotkeys = hotkeysArr.Distinct()
                    .Where(i => !string.IsNullOrEmpty(i))
                    .Select(i => new Hotkey(i))
                    .ToList();

                return tool;
            })
            .Where(i => i != null && !i.IsEmpty);
        if (toolList != null && toolList.Any())
        {
            Tools.Clear();
            Tools = toolList.ToList();
        }


        // DisabledMenus
        DisabledMenus = items.GetSection(nameof(DisabledMenus))
            .GetChildren()
            .Select(i => i.Get<string>())
            .ToList();

        #endregion // Array items


        // Other types values
        #region Other types items

        #region Language
        var langPath = items.GetValueEx(nameof(Language), "English");
        Language = new IgLang(langPath, App.StartUpDir(Dir.Language));
        #endregion


        // must load before Theme
        #region BackgroundColor

        var bgValue = items.GetValueEx(nameof(BackgroundColor), string.Empty);

        if (string.IsNullOrEmpty(bgValue))
        {
            BackgroundColor = Theme.Colors.BgColor;
        }
        else
        {
            BackgroundColor = BHelper.ColorFromHex(bgValue);
        }
        #endregion


        // load theme
        LoadThemePack(WinColorsApi.IsDarkMode, true, true, false);


        #region SlideshowBackgroundColor

        bgValue = items.GetValueEx(nameof(SlideshowBackgroundColor), string.Empty);
        if (!string.IsNullOrWhiteSpace(bgValue))
        {
            SlideshowBackgroundColor = BHelper.ColorFromHex(bgValue);
        }

        #endregion

        #endregion // Other types items


        // initialize Magick.NET
        PhotoCodec.InitMagickNET();

        // listen to system events
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

#nullable enable 
    }


    /// <summary>
    /// Parses and writes configs to file
    /// </summary>
    public static async Task WriteAsync()
    {
        var jsonFile = App.ConfigDir(PathType.File, Source.UserFilename);
        var jsonObj = PrepareJsonSettingsObject();

        await BHelper.WriteJsonAsync(jsonFile, jsonObj);
    }


    /// <summary>
    /// Converts all settings to ExpandoObject for parsing JSON.
    /// </summary>
    public static ExpandoObject PrepareJsonSettingsObject(bool useAbsoluteFileUrl = false)
    {
        var settings = new ExpandoObject();

        var metadata = new ConfigMetadata()
        {
            Description = _source.Description,
            Version = _source.Version,
        };

        settings.TryAdd("_Metadata", metadata);


        #region Boolean items

        settings.TryAdd(nameof(EnableSlideshow), EnableSlideshow);
        settings.TryAdd(nameof(HideMainWindowInSlideshow), HideMainWindowInSlideshow);
        settings.TryAdd(nameof(ShowSlideshowCountdown), ShowSlideshowCountdown);
        settings.TryAdd(nameof(UseRandomIntervalForSlideshow), UseRandomIntervalForSlideshow);
        settings.TryAdd(nameof(EnableLoopSlideshow), EnableLoopSlideshow);
        settings.TryAdd(nameof(EnableFullscreenSlideshow), EnableFullscreenSlideshow);
        settings.TryAdd(nameof(EnableFrameless), EnableFrameless);
        settings.TryAdd(nameof(EnableFullScreen), EnableFullScreen);
        settings.TryAdd(nameof(ShowGallery), ShowGallery);
        settings.TryAdd(nameof(ShowGalleryScrollbars), ShowGalleryScrollbars);
        settings.TryAdd(nameof(ShowGalleryFileName), ShowGalleryFileName);
        settings.TryAdd(nameof(ShowWelcomeImage), ShowWelcomeImage);
        settings.TryAdd(nameof(ShowToolbar), ShowToolbar);
        settings.TryAdd(nameof(ShowAppIcon), ShowAppIcon);
        settings.TryAdd(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        settings.TryAdd(nameof(ShowCheckerboard), ShowCheckerboard);
        settings.TryAdd(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        settings.TryAdd(nameof(EnableMultiInstances), EnableMultiInstances);
        settings.TryAdd(nameof(EnableWindowTopMost), EnableWindowTopMost);
        settings.TryAdd(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        settings.TryAdd(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        settings.TryAdd(nameof(ShouldPreserveModifiedDate), ShouldPreserveModifiedDate);
        settings.TryAdd(nameof(ShowNewVersionIndicator), ShowNewVersionIndicator);
        settings.TryAdd(nameof(EnableCenterToolbar), EnableCenterToolbar);
        settings.TryAdd(nameof(ShouldOpenLastSeenImage), ShouldOpenLastSeenImage);
        settings.TryAdd(nameof(ShouldUseColorProfileForAll), ShouldUseColorProfileForAll);
        settings.TryAdd(nameof(EnableNavigationButtons), EnableNavigationButtons);
        settings.TryAdd(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        settings.TryAdd(nameof(ShouldUseExplorerSortOrder), ShouldUseExplorerSortOrder);
        settings.TryAdd(nameof(ShouldGroupImagesByDirectory), ShouldGroupImagesByDirectory);
        settings.TryAdd(nameof(ShouldLoadHiddenImages), ShouldLoadHiddenImages);
        settings.TryAdd(nameof(EnableWindowFit), EnableWindowFit);
        settings.TryAdd(nameof(CenterWindowFit), CenterWindowFit);
        settings.TryAdd(nameof(UseEmbeddedThumbnailRawFormats), UseEmbeddedThumbnailRawFormats);
        settings.TryAdd(nameof(UseEmbeddedThumbnailOtherFormats), UseEmbeddedThumbnailOtherFormats);
        settings.TryAdd(nameof(ShowImagePreview), ShowImagePreview);
        settings.TryAdd(nameof(EnableImageTransition), EnableImageTransition);
        settings.TryAdd(nameof(EnableImageAsyncLoading), EnableImageAsyncLoading);
        settings.TryAdd(nameof(EnableCopyMultipleFiles), EnableCopyMultipleFiles);
        settings.TryAdd(nameof(EnableCutMultipleFiles), EnableCutMultipleFiles);
        settings.TryAdd(nameof(EnableRealTimeFileUpdate), EnableRealTimeFileUpdate);
        settings.TryAdd(nameof(ShouldAutoOpenNewAddedImage), ShouldAutoOpenNewAddedImage);
        settings.TryAdd(nameof(UseWebview2ForSvg), UseWebview2ForSvg);
        settings.TryAdd(nameof(EnableDebug), EnableDebug);

        settings.TryAdd(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        settings.TryAdd(nameof(HideGalleryInFullscreen), HideGalleryInFullscreen);

        #endregion


        #region Number items

        settings.TryAdd(nameof(QuickSetupVersion), QuickSetupVersion);

        // FrmMain
        settings.TryAdd(nameof(FrmMainPositionX), FrmMainPositionX);
        settings.TryAdd(nameof(FrmMainPositionY), FrmMainPositionY);
        settings.TryAdd(nameof(FrmMainWidth), FrmMainWidth);
        settings.TryAdd(nameof(FrmMainHeight), FrmMainHeight);

        // FrmSettings
        settings.TryAdd(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        settings.TryAdd(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        settings.TryAdd(nameof(FrmSettingsWidth), FrmSettingsWidth);
        settings.TryAdd(nameof(FrmSettingsHeight), FrmSettingsHeight);

        settings.TryAdd(nameof(PanSpeed), PanSpeed);
        settings.TryAdd(nameof(ZoomSpeed), ZoomSpeed);
        settings.TryAdd(nameof(SlideshowInterval), SlideshowInterval);
        settings.TryAdd(nameof(SlideshowIntervalTo), SlideshowIntervalTo);
        settings.TryAdd(nameof(SlideshowImagesToNotifySound), SlideshowImagesToNotifySound);
        settings.TryAdd(nameof(ThumbnailSize), ThumbnailSize);
        settings.TryAdd(nameof(GalleryCacheSizeInMb), GalleryCacheSizeInMb);
        settings.TryAdd(nameof(GalleryColumns), GalleryColumns);
        settings.TryAdd(nameof(ImageBoosterCacheCount), ImageBoosterCacheCount);
        settings.TryAdd(nameof(ImageBoosterCacheMaxDimension), ImageBoosterCacheMaxDimension);
        settings.TryAdd(nameof(ImageBoosterCacheMaxFileSizeInMb), ImageBoosterCacheMaxFileSizeInMb);
        settings.TryAdd(nameof(ZoomLockValue), ZoomLockValue);
        settings.TryAdd(nameof(ToolbarIconHeight), ToolbarIconHeight);
        settings.TryAdd(nameof(ImageEditQuality), ImageEditQuality);
        settings.TryAdd(nameof(InAppMessageDuration), InAppMessageDuration);
        settings.TryAdd(nameof(EmbeddedThumbnailMinWidth), EmbeddedThumbnailMinWidth);
        settings.TryAdd(nameof(EmbeddedThumbnailMinHeight), EmbeddedThumbnailMinHeight);

        #endregion


        #region Enum items

        settings.TryAdd(nameof(FrmMainState), FrmMainState.ToString());
        settings.TryAdd(nameof(FrmSettingsState), FrmSettingsState.ToString());
        settings.TryAdd(nameof(ImageLoadingOrder), ImageLoadingOrder.ToString());
        settings.TryAdd(nameof(ImageLoadingOrderType), ImageLoadingOrderType.ToString());
        settings.TryAdd(nameof(ZoomMode), ZoomMode.ToString());
        settings.TryAdd(nameof(ImageInterpolationScaleDown), ImageInterpolationScaleDown);
        settings.TryAdd(nameof(ImageInterpolationScaleUp), ImageInterpolationScaleUp);
        settings.TryAdd(nameof(AfterEditingAction), AfterEditingAction.ToString());
        settings.TryAdd(nameof(WindowBackdrop), WindowBackdrop);

        #endregion


        #region String items

        settings.TryAdd(nameof(ColorProfile), ColorProfile);
        settings.TryAdd(nameof(AutoUpdate), AutoUpdate);
        settings.TryAdd(nameof(LastSeenImagePath), LastSeenImagePath);
        settings.TryAdd(nameof(LastOpenedSetting), LastOpenedSetting);
        settings.TryAdd(nameof(DarkTheme), DarkTheme);
        settings.TryAdd(nameof(LightTheme), LightTheme);

        #endregion


        #region Other types items
        if (BackgroundColor == Theme.Colors.BgColor)
        {
            settings.TryAdd(nameof(BackgroundColor), ""); // follow theme background
        }
        else
        {
            settings.TryAdd(nameof(BackgroundColor), BackgroundColor.ToHex());
        }

        settings.TryAdd(nameof(SlideshowBackgroundColor), SlideshowBackgroundColor.ToHex());
        settings.TryAdd(nameof(Language), Language.FileName);

        #endregion


        #region Array items

        settings.TryAdd(nameof(ZoomLevels), ZoomLevels.Select(i => Math.Round(i * 100, 2)));
        settings.TryAdd(nameof(EditApps), EditApps);
        settings.TryAdd(nameof(FileFormats), GetImageFormats(FileFormats));
        settings.TryAdd(nameof(SingleFrameFormats), GetImageFormats(SingleFrameFormats));
        settings.TryAdd(nameof(ImageInfoTags), ImageInfoTags);
        settings.TryAdd(nameof(MenuHotkeys), ParseHotkeys(MenuHotkeys));
        settings.TryAdd(nameof(MouseClickActions), MouseClickActions);
        settings.TryAdd(nameof(MouseWheelActions), MouseWheelActions);
        settings.TryAdd(nameof(Layout), Layout);
        settings.TryAdd(nameof(Tools), Tools);
        settings.TryAdd(nameof(DisabledMenus), DisabledMenus);

        // ToolbarButtons
        settings.TryAdd(nameof(ToolbarButtons), useAbsoluteFileUrl
            ? ConvertToolbarButtonsToExpandoObjList(ToolbarButtons)
            : ToolbarButtons);

        #endregion


        // Tools' settings
        settings.TryAdd(nameof(ToolSettings), (object)ToolSettings);

        return settings;
    }


    /// <summary>
    /// Converts <c>List{ToolbarItemModel}</c> to <c>List{ExpandoObject}</c>.
    /// Also adds <c>ImageUrl</c> property to the item.
    /// </summary>
    public static List<ExpandoObject> ConvertToolbarButtonsToExpandoObjList(List<ToolbarItemModel> list)
    {
        var items = list.Select(i =>
        {
            var obj = i.ToExpandoObject();

            if (string.IsNullOrWhiteSpace(i.Image)) return obj;

            var filePath = Theme.GetToolbarIconFilePath(i.Image);
            if (string.IsNullOrWhiteSpace(filePath)) filePath = i.Image;

            try
            {
                var fileUrl = new Uri(filePath).AbsoluteUri;
                if (!string.IsNullOrWhiteSpace(fileUrl))
                {
                    obj.TryAdd("ImageUrl", fileUrl);
                    return obj;
                }
            }
            catch { }

            return obj;
        }).ToList();

        return items;
    }


    /// <summary>
    /// Gets property by name.
    /// </summary>
    public static PropertyInfo? GetProp(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        // Find the public property in Config
        var prop = typeof(Config)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(i => i.Name == name);

        return prop;
    }


    /// <summary>
    /// Set user settings from JSON dictionary.
    /// </summary>
    /// <param name="settings">JSON dictionary</param>
    /// <param name="configName">Config name</param>
    public static (bool Done, bool Unsupported) SetFromJson(IDictionary<string, string> settings, string configName)
    {
        var Done = false;
        var Unsupported = false;

        if (!settings.ContainsKey(configName)) return (Done, Unsupported);

        var prop = Config.GetProp(configName);
        if (prop == null) return (Done, Unsupported);

        var propValue = prop?.GetValue(null);
        if (!settings.TryGetValue(configName, out var newValue)) return (Done, Unsupported);
        newValue ??= string.Empty;


        // BackgroundColor
        if (configName == nameof(Config.BackgroundColor))
        {
            BackgroundColor = BHelper.ColorFromHex(newValue);
            Done = true;
        }

        // SlideshowBackgroundColor
        else if (configName == nameof(Config.SlideshowBackgroundColor))
        {
            SlideshowBackgroundColor = BHelper.ColorFromHex(newValue);
            Done = true;
        }

        // Zoom levels
        else if (configName == nameof(Config.ZoomLevels))
        {
            try
            {
                var floats = BHelper.ParseJson<float[]>(newValue);
                if (floats != null)
                {
                    Config.ZoomLevels = floats.Select(i =>
                    {
                        // convert % to float
                        try { return i / 100f; } catch { }
                        return -1;
                    })
                    .OrderBy(i => i)
                    .Where(i => i > 0)
                    .Distinct()
                    .ToArray();

                    Done = true;
                }
            }
            catch { }
        }

        // Language
        else if (configName == nameof(Config.Language))
        {
            Config.Language = new IgLang(newValue, App.StartUpDir(Dir.Language));
            Done = true;
        }

        // MouseWheelActions
        else if (configName == nameof(Config.MouseWheelActions))
        {
            var dict = BHelper.ParseJson<Dictionary<MouseWheelEvent, MouseWheelAction>>(newValue);
            if (dict != null)
            {
                foreach (var key in dict.Keys)
                {
                    if (Config.MouseWheelActions.ContainsKey(key))
                    {
                        Config.MouseWheelActions[key] = dict[key];
                    }
                    else
                    {
                        Config.MouseWheelActions.TryAdd(key, dict[key]);
                    }
                }
                Done = true;
            }
        }

        // MouseClickActions
        else if (configName == nameof(Config.MouseClickActions))
        {
            var dict = BHelper.ParseJson<Dictionary<MouseClickEvent, ToggleAction>>(newValue);
            if (dict != null)
            {
                foreach (var key in dict.Keys)
                {
                    if (Config.MouseClickActions.ContainsKey(key))
                    {
                        Config.MouseClickActions[key] = dict[key];
                    }
                    else
                    {
                        Config.MouseClickActions.TryAdd(key, dict[key]);
                    }
                }
                Done = true;
            }
        }

        // MenuHotkeys
        else if (configName == nameof(Config.MenuHotkeys))
        {
            var dict = BHelper.ParseJson<Dictionary<string, string[]>>(newValue);
            if (dict != null)
            {
                foreach (var key in dict.Keys)
                {
                    // Config.MenuHotkeys[key] = 
                }
                Done = true;
            }
        }

        // Tools
        else if (configName == nameof(Config.Tools))
        {
            var toolList = BHelper.ParseJson<IgTool?[]>(newValue);
            if (toolList != null)
            {
                Config.Tools.Clear();
                Config.Tools = [.. toolList];
            }
            Done = true;
        }

        // Layout
        else if (configName == nameof(Config.Layout))
        {
            var dict = BHelper.ParseJson<Dictionary<string, string>>(newValue);
            if (dict != null)
            {
                Config.Layout.Clear();
                foreach (var item in dict)
                {
                    _ = Config.Layout.TryAdd(item.Key, item.Value);
                }

                Done = true;
            }
        }

        // ToolbarButtons
        else if (configName == nameof(Config.ToolbarButtons))
        {
            var btnList = BHelper.ParseJson<ToolbarItemModel[]>(newValue);
            if (btnList != null)
            {
                Config.ToolbarButtons.Clear();
                Config.ToolbarButtons = [.. btnList];
            }
            Done = true;
        }

        // ImageInfoTags
        else if (configName == nameof(Config.ImageInfoTags))
        {
            try
            {
                var strArr = BHelper.ParseJson<string[]>(newValue);
                if (strArr != null)
                {
                    Config.ImageInfoTags.Clear();
                    Config.ImageInfoTags.AddRange(strArr);

                    Done = true;
                }
            }
            catch { }
        }

        // EditApps
        else if (configName == nameof(Config.EditApps))
        {
            var dict = BHelper.ParseJson<Dictionary<string, EditApp>>(newValue);
            if (dict != null)
            {
                Config.EditApps.Clear();
                foreach (var item in dict)
                {
                    _ = Config.EditApps.TryAdd(item.Key, item.Value);
                }

                Done = true;
            }
        }

        // FileFormats
        else if (configName == nameof(Config.FileFormats))
        {
            FileFormats = GetImageFormats(newValue);
            Done = true;
        }

        // bool
        else if (prop.PropertyType.Equals(typeof(bool)))
        {
            if (bool.TryParse(newValue, out var value))
            {
                prop.SetValue(null, value);
                Done = true;
            }
        }
        // int
        else if (prop.PropertyType.Equals(typeof(int)))
        {
            if (int.TryParse(newValue, out var value))
            {
                prop.SetValue(null, value);
                Done = true;
            }
        }
        // float
        else if (prop.PropertyType.Equals(typeof(float)))
        {
            if (int.TryParse(newValue, out var value))
            {
                prop.SetValue(null, value);
                Done = true;
            }
        }
        // string
        else if (prop.PropertyType.Equals(typeof(string)))
        {
            prop.SetValue(null, newValue);
            Done = true;
        }
        // enum
        else if (prop.PropertyType.IsEnum)
        {
            if (Enum.TryParse(prop.PropertyType, newValue, true, out var value))
            {
                prop.SetValue(null, value);
                Done = true;
            }
        }
        else
        {
            // unsupported type
            Unsupported = true;
        }

        return (Done, Unsupported);
    }


    /// <summary>
    /// Loads theme pack <see cref="Config.Theme"/> and only theme colors.
    /// </summary>
    /// <param name="darkMode">
    /// Determine which theme should be loaded: <see cref="DarkTheme"/> or <see cref="LightTheme"/>.
    /// </param>
    /// <param name="useFallBackTheme">
    /// If theme pack is invalid, should load the default theme pack <see cref="Const.DEFAULT_THEME"/>?
    /// </param>
    /// <param name="throwIfThemeInvalid">
    /// If theme pack is invalid, should throw exception?
    /// </param>
    /// <param name="forceUpdateBackground">Force updating background according to theme value</param>
    /// <exception cref="InvalidDataException"></exception>
    public static void LoadThemePack(bool darkMode, bool useFallBackTheme, bool throwIfThemeInvalid, bool forceUpdateBackground)
    {
        var themeFolderName = darkMode ? DarkTheme : LightTheme;
        if (string.IsNullOrEmpty(themeFolderName))
        {
            themeFolderName = Const.DEFAULT_THEME;
        }


        // theme pack is already updated
        if (themeFolderName.Equals(Theme.FolderName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        // load theme pack
        var th = FindAndLoadThemePack(themeFolderName, useFallBackTheme, throwIfThemeInvalid);

        // update the name of dark/light theme
        if (darkMode) DarkTheme = th.FolderName;
        else LightTheme = th.FolderName;

        // load theme settings
        BHelper.RunSync(th.LoadThemeSettingsAsync);

        // load theme colors
        th.LoadThemeColors();


        // load background color
        if (Config.BackgroundColor == Theme.Colors.BgColor || forceUpdateBackground)
        {
            Config.BackgroundColor = th.Colors.BgColor;
        }


        // set to the current theme
        Theme?.Dispose();
        Theme = th;
    }


    /// <summary>
    /// Finds the correct location of theme name and loads it.
    /// </summary>
    /// <exception cref="InvalidDataException"></exception>
    private static IgTheme? FindAndLoadThemePack(string themeFolderName, bool useFallBackTheme, bool throwIfThemeInvalid)
    {
        // look for theme pack in the Config dir
        var th = new IgTheme(App.ConfigDir(PathType.Dir, Dir.Themes, themeFolderName));
        if (!th.IsValid)
        {
            // look for theme pack in the Startup dir
            th.Dispose();
            th = null;
            th = new(App.StartUpDir(Dir.Themes, themeFolderName));

            // cannot find theme, use fall back theme
            if (!th.IsValid && useFallBackTheme)
            {
                th.Dispose();
                th = null;

                // load default theme
                th = new(App.StartUpDir(Dir.Themes, Const.DEFAULT_THEME));
            }
        }


        if (!th.IsValid && throwIfThemeInvalid)
        {
            var themeName = th.FolderName;

            th.Dispose();
            th = null;

            throw new InvalidDataException($"Unable to load '{themeName}' theme pack. " +
                $"Please make sure '{Path.Combine(themeName, IgTheme.CONFIG_FILE)}' file is valid.");
        }

        return th;
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingTheme"/> event.
    /// </summary>
    public static void TriggerRequestUpdatingTheme()
    {
        RequestUpdatingTheme?.Invoke(new RequestUpdatingThemeEventArgs(Config.Theme));
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingLanguage"/> event.
    /// </summary>
    public static void TriggerRequestUpdatingLanguage()
    {
        RequestUpdatingLanguage?.Invoke();
    }


    /// <summary>
    /// Updates form icon using theme setting.
    /// </summary>
    public static async Task UpdateFormIcon(Form frm)
    {
        // Icon theming
        if (Config.Theme.Settings.AppLogo?.GetHicon() is not IntPtr hIcon) return;
        frm.Icon = Icon.FromHandle(hIcon);

        await Task.Delay(200);
        frm.ShowIcon = Config.ShowAppIcon;
    }


    /// <summary>
    /// Parses string dictionary to hotkey dictionary
    /// </summary>
    public static Dictionary<string, List<Hotkey>> ParseHotkeys(Dictionary<string, string?[]>? dict)
    {
        var result = new Dictionary<string, List<Hotkey>>();
        if (dict == null) return result;

        foreach (var item in dict)
        {
            var keyList = new List<Hotkey>();

            // sample item: { "MnuOpen": ["O", "Ctrl+O"] }
            foreach (var strKey in item.Value)
            {
                try
                {
                    keyList.Add(new Hotkey(strKey));
                }
                catch { }
            }


            if (result.TryGetValue(item.Key, out List<Hotkey>? value))
            {
                value.Clear();
                value.AddRange(keyList);
            }
            else
            {
                result.Add(item.Key, keyList);
            }
        };

        return result;
    }


    /// <summary>
    /// Parses hotkey dictionary to string dictionary.
    /// </summary>
    public static Dictionary<string, string[]> ParseHotkeys(Dictionary<string, List<Hotkey>> dict)
    {
        var result = dict.Select(i => new
        {
            i.Key,
            Value = i.Value.Select(k => k.ToString()),
        }).ToDictionary(i => i.Key, i => i.Value.ToArray());

        return result;
    }


    /// <summary>
    /// Gets all hotkeys by merging <see cref="Config.MenuHotkeys"/>
    /// and <see cref="Config.Tools"/> into <c><paramref name="defaultDict"/></c>.
    /// </summary>
    public static Dictionary<string, List<Hotkey>> GetAllHotkeys(Dictionary<string, List<Hotkey>> defaultDict)
    {
        var result = defaultDict;

        // merge menu hotkeys
        foreach (var item in Config.MenuHotkeys)
        {
            if (result.ContainsKey(item.Key))
            {
                result[item.Key] = item.Value;
            }
            else
            {
                result.Add(item.Key, item.Value);
            }
        }


        var toolHotkeys = Config.Tools
            .ToDictionary(i => i.ToolId, i => i.Hotkeys);

        // merge tool hotkeys
        foreach (var item in toolHotkeys)
        {
            if (result.ContainsKey(item.Key))
            {
                result[item.Key] = item.Value;
            }
            else
            {
                result.Add(item.Key, item.Value);
            }
        }

        return result;
    }


    /// <summary>
    /// Gets hotkeys string from the action.
    /// </summary>
    public static string GetHotkeyString(Dictionary<string, List<Hotkey>> dict, string action)
    {
        dict.TryGetValue(action, out var hotkeyList);

        if (hotkeyList != null)
        {
            var str = string.Join(", ", hotkeyList.Select(k => k.ToString()));

            return str ?? string.Empty;
        }


        return string.Empty;
    }


    /// <summary>
    /// Gets hotkeys from the action.
    /// </summary>
    public static List<Hotkey> GetHotkey(Dictionary<string, List<Hotkey>> dict, string action)
    {
        dict.TryGetValue(action, out var hotkeyList);

        return hotkeyList ?? [];
    }


    /// <summary>
    /// Gets hotkey's KeyData.
    /// </summary>
    public static List<Keys> GetHotkeyData(Dictionary<string, List<Hotkey>> dict, string action, Keys defaultValue)
    {
        var keyDataList = GetHotkey(dict, action)
            .Select(k => k.KeyData).ToList();

        return keyDataList ?? [defaultValue];
    }


    /// <summary>
    /// Gets actions from the input hotkey.
    /// </summary>
    public static List<string> GetHotkeyActions(Dictionary<string, List<Hotkey>> dict, Hotkey hotkey)
    {
        var actions = new HashSet<string>();

        foreach (var item in dict)
        {
            foreach (var key in item.Value)
            {
                var result = string.Compare(key.ToString(), hotkey.ToString(), StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                {
                    actions.Add(item.Key);
                }
            }
        }

        return [.. actions];
    }


    /// <summary>
    /// Loads language list. It auto-adds the first item as the default language pack.
    /// </summary>
    public static List<IgLang> LoadLanguageList()
    {
        var list = new List<IgLang>() { new IgLang(), };
        var langDir = App.StartUpDir(Dir.Language);

        if (!Directory.Exists(langDir)) return list;

        var files = Directory.EnumerateFiles(langDir, "*.iglang.json")
            .Select(path => new IgLang(path));
        list.AddRange(files);

        return list;
    }


    /// <summary>
    /// Loads all theme packs from default folder and user folder.
    /// </summary>
    public static List<IgTheme> LoadThemeList()
    {
        var defaultThemeFolder = App.StartUpDir(Dir.Themes);
        var userThemeFolder = App.ConfigDir(PathType.Dir, Dir.Themes);

        // Create theme folder if not exist
        Directory.CreateDirectory(userThemeFolder);

        var userThemeNames = Directory.EnumerateDirectories(userThemeFolder);
        var defaultThemeNames = Directory.EnumerateDirectories(defaultThemeFolder);

        // merge and distinct all themes
        var allThemeNames = defaultThemeNames.ToList();
        allThemeNames.AddRange(userThemeNames);
        allThemeNames = allThemeNames.Distinct().ToList();

        var allThemes = new List<IgTheme>(allThemeNames.Count);

        Parallel.ForEach(allThemeNames, dir =>
        {
            var configFile = Path.Combine(dir, IgTheme.CONFIG_FILE);
            var th = new IgTheme(themeFolderPath: dir);

            // valid theme
            if (th.IsValid)
            {
                allThemes.Add(th);
            }
        });


        // get default theme dir
        var defaultThemePath = App.StartUpDir(Dir.Themes, Const.DEFAULT_THEME, IgTheme.CONFIG_FILE);

        allThemes = [.. allThemes.OrderBy(i => i.ConfigFilePath != defaultThemePath)];

        return allThemes;
    }


    /// <summary>
    /// Builds the command line from config value.
    /// Example: <c>/EnableFullScreen=True</c>
    /// </summary>
    public static string BuildConfigCmdLine(string configName, object? configValue)
    {
        if (configValue == null) return string.Empty;

        return $"{Const.CONFIG_CMD_PREFIX}{configName}=\"{configValue}\"";
    }


    /// <summary>
    /// Sets default photo viewer.
    /// </summary>
    public static async Task SetDefaultPhotoViewerAsync(bool enable)
    {
        var extensions = Config.GetImageFormats(Config.FileFormats);

        var cmd = enable
            ? IgCommands.SET_DEFAULT_PHOTO_VIEWER
            : IgCommands.REMOVE_DEFAULT_PHOTO_VIEWER;

        // run command and show the results
        _ = await Config.RunIgcmd($"{cmd} {extensions} {IgCommands.SHOW_UI}");
    }


    /// <summary>
    /// Gets <see cref="EditApp"/> from the given extension.
    /// </summary>
    /// <param name="ext">An extension. E.g. <c>.jpg</c></param>
    public static EditApp? GetEditAppFromExtension(string ext)
    {
        var appItem = Config.EditApps.FirstOrDefault(i =>
        {
            var exts = i.Key.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return exts.Contains(ext);
        });

        return appItem.Value;
    }


    /// <summary>
    /// Runs a command from <c>igcmd.exe</c>, supports auto-elevating process privilege
    /// if admin permission is required.
    /// </summary>
    public static async Task<IgExitCode> RunIgcmd(string args, bool waitForExit = true)
    {
        var exePath = App.StartUpDir("igcmd.exe");

        // create command-lines for the current settings
        var lightThemeCmd = Config.BuildConfigCmdLine(nameof(Config.LightTheme), Config.LightTheme);
        var darkThemeCmd = Config.BuildConfigCmdLine(nameof(Config.DarkTheme), Config.DarkTheme);
        var langCmd = Config.BuildConfigCmdLine(nameof(Config.Language), Config.Language.FileName);

        var allArgs = $"{args} {lightThemeCmd} {darkThemeCmd} {langCmd}";

        return await BHelper.RunExeCmd(exePath, allArgs, waitForExit);
    }


    // Popup functions
    #region Popup functions

    /// <summary>
    /// Shows information popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowInfo(
        Form? formOwner,
        string description = "",
        string title = "",
        string heading = "",
        string details = "",
        string note = "",
        ShellStockIcon? icon = ShellStockIcon.SIID_INFO,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        SystemSounds.Question.Play();

        return Popup.ShowDialog(description, title, heading, details, note, ColorStatusType.Info, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Shows warning popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowWarning(
        Form? formOwner,
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        string note = "",
        ShellStockIcon? icon = ShellStockIcon.SIID_WARNING,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        heading ??= Language["_._Warning"];
        SystemSounds.Exclamation.Play();

        return Popup.ShowDialog(description, title, heading, details, note, ColorStatusType.Warning, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Shows error popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowError(
        Form? formOwner,
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        string note = "",
        ShellStockIcon? icon = ShellStockIcon.SIID_ERROR,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        heading ??= Language["_._Error"];
        SystemSounds.Asterisk.Play();

        return Popup.ShowDialog(description, title, heading, details, note, ColorStatusType.Danger, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Show the default error popup for igcmd.exe
    /// </summary>
    /// <returns><see cref="IgExitCode.Error"/></returns>
    public static int ShowDefaultIgCommandError(string igcmdExeName)
    {
        var url = "https://imageglass.org/docs/command-line-utilities";
        var langPath = $"_._IgCommandExe._DefaultError";

        var result = ShowError(null,
            title: Application.ProductName + " v" + Application.ProductVersion,
            heading: Language[$"{langPath}._Heading"],
            description: string.Format(Language[$"{langPath}._Description"], url),
            buttons: PopupButton.LearnMore_Close);


        if (result.ExitResult == PopupExitResult.OK)
        {
            _ = BHelper.OpenUrlAsync(url, $"from_{igcmdExeName}_invalid_command");
        }

        return (int)IgExitCode.Error;
    }


    /// <summary>
    /// Shows unhandled exception popup
    /// </summary>
    public static void HandleException(Exception ex)
    {
        var osInfo = Environment.OSVersion.VersionString + " " + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");
        var exeVersion = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;
        var appInfo = Application.ProductName + " v" + exeVersion;
        if (BHelper.IsRunningAsUwp()) appInfo += " (Store)";

        var langPath = $"_._UnhandledException";
        var description = Language[$"{langPath}._Description"];
        var details =
            $"Version: {appInfo}\r\n" +
            $"Release code: {Const.APP_CODE}\r\n" +
            $"Magick.NET: {ImageMagick.MagickNET.Version}\r\n" +
            $"WebView2 Runtime: {Web2.Webview2Version?.ToString()}\r\n" +
            $"OS: {osInfo}\r\n\r\n" +

            $"----------------------------------------------------\r\n" +
            $"Error:\r\n\r\n" +
            $"{ex.Message}\r\n" +
            $"----------------------------------------------------\r\n\r\n" +

            ex.ToString();

        var result = ShowError(null,
            title: Application.ProductName + " - " + Language[langPath],
            heading: ex.Message,
            description: description,
            details: details,
            buttons: PopupButton.Continue_Quit);

        if (result.ExitResult == PopupExitResult.Cancel)
        {
            Application.Exit();
        }
    }


    #endregion // Popup functions


    #endregion // Public static functions




    #region Private functions

    // ImageFormats
    #region ImageFormats

    /// <summary>
    /// Returns distinc list of image formats.
    /// </summary>
    /// <param name="formats">The format string. E.g: <c>.bpm;.jpg;</c></param>
    public static HashSet<string> GetImageFormats(string formats)
    {
        var formatList = formats
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet();

        return formatList;
    }

    /// <summary>
    /// Returns the image formats string. Example: <c>.png;.jpg;</c>
    /// </summary>
    /// <param name="list">The input HashSet</param>
    public static string GetImageFormats(HashSet<string> list)
    {
        var sb = new StringBuilder(list.Count);
        foreach (var item in list)
        {
            sb.Append(item).Append(';');
        }

        return sb.ToString();
    }

    #endregion // ImageFormats


    // System events
    #region System events

    private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // User settings changed:
        // - Color mode: dark / light
        // - Transparency
        // - Accent color
        // - others...
        if (e.Category == UserPreferenceCategory.General)
        {
            DelayTriggerRequestUpdatingColorModeEvent();
        }
    }


    /// <summary>
    /// Delays triggering <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    private static void DelayTriggerRequestUpdatingColorModeEvent()
    {
        _requestUpdatingColorModeCancelToken.Cancel();
        _requestUpdatingColorModeCancelToken = new();

        _ = TriggerRequestUpdatingColorModeEventAsync(_requestUpdatingColorModeCancelToken.Token);
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    private static async Task TriggerRequestUpdatingColorModeEventAsync(CancellationToken token = default)
    {
        try
        {
            // since the message is triggered multiple times (3 - 5 times)
            await Task.Delay(10, token);
            token.ThrowIfCancellationRequested();


            var isDarkMode = WinColorsApi.IsDarkMode;
            if (_isDarkMode != isDarkMode)
            {
                _isDarkMode = isDarkMode;

                // emit event here
                RequestUpdatingColorMode?.Invoke(new SystemColorModeChangedEventArgs(_isDarkMode));
            }
        }
        catch (OperationCanceledException) { }
    }
    #endregion // System events

    #endregion // Private functions


}
