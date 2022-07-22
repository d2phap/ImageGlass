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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.WinApi;
using ImageGlass.UI;
using Microsoft.Extensions.Configuration;
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

    /// <summary>
    /// Gets the default set of toolbar buttons
    /// </summary>
    public static List<ToolbarItemModel> DefaultToolbarItems => new()
    {
        new()
        {
            Id = "btn_OpenFile",
            Alignment = ToolStripItemAlignment.Right,
            Image = "OpenFile",
            OnClick = new("MnuOpenFile"),
        },
        new()
        {
            Id = "btn_ViewPrevious",
            Image = "ViewPreviousImage",
            OnClick = new("MnuViewPrevious"),
        },
        new()
        {
            Id = "btn_ViewNext",
            Image = "ViewNextImage",
            OnClick = new("MnuViewNext"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "btn_AutoZoom",
            Image = "AutoZoom",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuAutoZoom"),
        },
        new()
        {
            Id = "btn_LockZoom",
            Image = "LockZoom",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuLockZoom"),
        },
        new()
        {
            Id = "btn_ScaleToWidth",
            Image = "ScaleToWidth",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuScaleToWidth"),
        },
        new()
        {
            Id = "btn_ScaleToHeight",
            Image = "ScaleToHeight",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuScaleToHeight"),
        },
        new()
        {
            Id = "btn_ScaleToFit",
            Image = "ScaleToFit",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuScaleToFit"),
        },
        new()
        {
            Id = "btn_ScaleToFill",
            Image = "ScaleToFill",
            CheckableConfigBinding = nameof(ZoomMode),
            OnClick = new("MnuScaleToFill"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "btn_Refresh",
            Image = "Refresh",
            OnClick = new("MnuRefresh"),
        },
        new()
        {
            Id = "btn_Thumbnail",
            Image = "ThumbnailBar",
            CheckableConfigBinding = nameof(ShowThumbnails),
            OnClick = new("MnuToggleThumbnails"),
        },
        new()
        {
            Id = "btn_Checkerboard",
            Image = "Checkerboard",
            CheckableConfigBinding = nameof(ShowCheckerBoard),
            OnClick = new("MnuToggleCheckerboard"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "btn_FullScreen",
            Image = "FullScreen",
            CheckableConfigBinding = nameof(EnableFullScreen),
            OnClick = new("MnuFullScreen"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        //new()
        //{
        //    Id = "btn_Edit",
        //    Image = "Edit",
        //},
        new()
        {
            Id = "btn_Print",
            Image = "Print",
            OnClick = new("MnuPrint"),
        },
        new()
        {
            Id = "btn_Delete",
            Image = "Delete",
            OnClick = new("MnuMoveToRecycleBin"),
        }
    };

    /// <summary>
    /// The default image info tags
    /// </summary>
    public static List<string> DefaultInfoItems => new()
    {
        nameof(ImageInfo.Name),
        nameof(ImageInfo.ListCount),
        nameof(ImageInfo.FramesCount),
        nameof(ImageInfo.Zoom),
        nameof(ImageInfo.Dimension),
        nameof(ImageInfo.FileSize),
        nameof(ImageInfo.ExifRating),
        nameof(ImageInfo.DateTimeAuto),
        nameof(ImageInfo.AppName),
    };


    #endregion


    #region Setting items

    #region Boolean items
    ///// <summary>
    ///// Gets, sets value of slideshow state
    ///// </summary>
    //public static bool IsSlideshow { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value if the countdown timer is shown or not
    ///// </summary>
    //public static bool IsShowSlideshowCountdown { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value indicating whether the slide show interval is random
    ///// </summary>
    //public static bool IsRandomSlideshowInterval { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating whether the full screen mode is enabled or not
    /// </summary>
    public static bool EnableFullScreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the toolbar should be hidden in Full screen mode
    /// </summary>
    public static bool HideToolbarInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the thumbnail bar should be hidden in Full screen mode
    /// </summary>
    public static bool HideThumbnailsInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value of thumbnail visibility
    /// </summary>
    public static bool ShowThumbnails { get; set; } = true;

    /// <summary>
    /// Gets, sets value whether thumbnail scrollbars visible
    /// </summary>
    public static bool ShowThumbnailScrollbars { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that showing image filename on thumbnail bar
    /// </summary>
    public static bool ShowThumbnailFilename { get; set; } = false;

    ///// <summary>
    ///// Gets, sets the value that indicates if the default position of image in the viewer is center or top left
    ///// </summary>
    //public static bool IsCenterImage { get; set; } = true;

    ///// <summary>
    ///// Check if user wants to display RGBA color code for Color Picker tool
    ///// </summary>
    //public static bool IsColorPickerRGBA { get; set; } = true;

    ///// <summary>
    ///// Check if user wants to display HEX with Alpha color code for Color Picker tool
    ///// </summary>
    //public static bool IsColorPickerHEXA { get; set; } = true;

    ///// <summary>
    ///// Check if user wants to display HSL with Alpha color code for Color Picker tool
    ///// </summary>
    //public static bool IsColorPickerHSLA { get; set; } = true;

    ///// <summary>
    ///// Check if user wants to display HSV with Alpha color code for Color Picker tool
    ///// </summary>
    //public static bool IsColorPickerHSVA { get; set; } = true;

    ///// <summary>
    ///// Gets, sets welcome picture value
    ///// </summary>
    //public static bool IsShowWelcome { get; set; } = true;

    /// <summary>
    /// Gets, sets value of visibility of toolbar when start up
    /// </summary>
    public static bool ShowToolbar { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value that allows user to loop back to the first image when reaching the end of list
    ///// </summary>
    //public static bool IsLoopBackSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
    /// </summary>
    public static bool EnableLoopBackNavigation { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value indicating that allow quit application by ESC
    ///// </summary>
    //public static bool EnablePressESCToQuit { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that checker board is shown or not
    /// </summary>
    public static bool ShowCheckerBoard { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating that multi instances is allowed or not
    /// </summary>
    public static bool EnableMultiInstances { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that FrmMain is always on top or not.
    /// </summary>
    public static bool EnableWindowTopMost { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value of FrmMain's frameless mode.
    ///// </summary>
    //public static bool IsWindowFrameless { get; set; } = false;

    ///// <summary>
    ///// Gets, sets the direction of thumbnail bar
    ///// </summary>
    //public static bool IsThumbnailHorizontal { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
    /// </summary>
    public static bool ShowDeleteConfirmation { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when overriding the viewing image
    /// </summary>
    public static bool ShowSaveOverrideConfirmation { get; set; } = true;

    ///// <summary>
    ///// Gets, sets the value indicates that viewer scrollbars are visible
    ///// </summary>
    //public static bool IsScrollbarsVisible { get; set; } = false;

    ///// <summary>
    ///// Gets, sets the value indicates that the viewing image is auto-saved after rotating
    ///// </summary>
    //public static bool IsSaveAfterRotating { get; set; } = false;

    /// <summary>
    /// Gets, sets the setting to control whether the image's original modified date value is preserved on save
    /// </summary>
    public static bool PreserveModifiedDate { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that there is a new version
    /// </summary>
    public static bool IsNewVersionAvailable { get; set; } = false;

    ///// <summary>
    ///// Gets, sets the value indicates that to show full image path or only base name
    ///// </summary>
    //public static bool IsDisplayBasenameOfImage { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that to toolbar buttons to be centered horizontally
    /// </summary>
    public static bool CenterToolbar { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that to show last seen image on startup
    /// </summary>
    public static bool OpenLastSeenImage { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that the ColorProfile will be applied for all or only the images with embedded profile
    /// </summary>
    public static bool ApplyColorProfileForAll { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates whether to show or hide the Navigation Buttons on viewer
    /// </summary>
    public static bool EnableNavigationButtons { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates whether to show checkerboard in the image region only
    /// </summary>
    public static bool ShowCheckerboardOnlyImageRegion { get; set; } = false;

    /// <summary>
    /// Gets, sets recursive value
    /// </summary>
    public static bool EnableRecursiveLoading { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
    /// </summary>
    public static bool UseFileExplorerSortOrder { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that images order should be grouped by directory
    /// </summary>
    public static bool GroupImagesByDirectory { get; set; } = false;

    /// <summary>
    /// Gets, sets showing/loading hidden images
    /// </summary>
    public static bool IncludeHiddenImages { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value that indicates frmColorPicker tool will be open on startup
    ///// </summary>
    //public static bool IsShowColorPickerOnStartup { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value that indicates frmPageNav tool will be open on startup
    ///// </summary>
    //public static bool IsShowPageNavOnStartup { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value that indicates page navigation tool auto-show on the multiple pages image
    ///// </summary>
    //public static bool IsShowPageNavAuto { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value specifying that Window Fit mode is on
    ///// </summary>
    //public static bool IsWindowFit { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value indicates the window should be always center in Window Fit mode
    ///// </summary>
    //public static bool IsCenterWindowFit { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value indicates that touch gesture support enabled
    ///// </summary>
    //public static bool IsUseTouchGesture { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value indicates that tooltips are to be hidden
    ///// </summary>
    //public static bool IsHideTooltips { get; set; } = false;

    ///// <summary>
    ///// Gets, sets value indicates that FrmExifTool always show on top
    ///// </summary>
    //public static bool IsExifToolAlwaysOnTop { get; set; } = true;

    ///// <summary>
    ///// Gets, sets value indicates that to keep the title bar of FrmMain empty
    ///// </summary>
    //public static bool IsUseEmptyTitleBar { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that RAW thumbnail will be use if found
    /// </summary>
    public static bool UseRawThumbnail { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the Image focus tool should be enable
    /// </summary>
    public static bool EnableImageFocusMode { get; set; } = false;


    #endregion


    #region Number items

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
    public static int FrmMainWidth { get; set; } = 1200;

    /// <summary>
    /// Gets, sets height of main window
    /// </summary>
    public static int FrmMainHeight { get; set; } = 800;

    /// <summary>
    /// Gets, sets the panning speed when <see cref="EnableImageFocusMode"/> is on.
    /// Value range is from 0 to 100.
    /// </summary>
    public static float PanSpeed { get; set; } = 20f;

    /// <summary>
    /// Gets, sets the zooming speed when <see cref="EnableImageFocusMode"/> is on.
    /// Value range is from -500 to 500.
    /// </summary>
    public static float ZoomSpeed { get; set; } = 0;


    ///// <summary>
    ///// Gets, sets 'Left' position of settings window
    ///// </summary>
    //public static int FrmSettingsPositionX { get; set; } = 200;

    ///// <summary>
    ///// Gets, sets 'Top' position of settings window
    ///// </summary>
    //public static int FrmSettingsPositionY { get; set; } = 200;

    ///// <summary>
    ///// Gets, sets width of settings window
    ///// </summary>
    //public static int FrmSettingsWidth { get; set; } = 1050;

    ///// <summary>
    ///// Gets, sets height of settings window
    ///// </summary>
    //public static int FrmSettingsHeight { get; set; } = 750;


    ///// <summary>
    ///// Gets, sets 'Left' position of ExifTool window
    ///// </summary>
    //public static int FrmExifToolPositionX { get; set; } = 200;

    ///// <summary>
    ///// Gets, sets 'Top' position of ExifTool window
    ///// </summary>
    //public static int FrmExifToolPositionY { get; set; } = 200;

    ///// <summary>
    ///// Gets, sets width of ExifTool window
    ///// </summary>
    //public static int FrmExifToolWidth { get; set; } = 800;

    ///// <summary>
    ///// Gets, sets height of ExifTool window
    ///// </summary>
    //public static int FrmExifToolHeight { get; set; } = 600;


    ///// <summary>
    ///// Gets, sets the version that requires to launch First-Launch Configs screen
    ///// </summary>
    //public static int FirstLaunchVersion { get; set; } = 0;

    ///// <summary>
    ///// Gets, sets slide show interval (minimum value if it's random)
    ///// </summary>
    //public static int SlideShowInterval { get; set; } = 5;

    ///// <summary>
    ///// Gets, sets the maximum slide show interval value
    ///// </summary>
    //public static int SlideShowIntervalTo { get; set; } = 5;

    /// <summary>
    /// Gets, sets value of thumbnail dimension in pixel
    /// </summary>
    public static int ThumbnailSize { get; set; } = 70;

    ///// <summary>
    ///// Gets, sets width of horizontal thumbnail bar
    ///// </summary>
    //public static int ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(ThumbnailDimension, true).GetTotalDimension();

    /// <summary>
    /// Gets, sets the number of images cached by Image
    /// </summary>
    public static int ImageBoosterCachedCount { get; set; } = 1;

    /// <summary>
    /// Gets, sets fixed width on zooming
    /// </summary>
    public static float ZoomLockValue { get; set; } = 100f;

    /// <summary>
    /// Gets, sets toolbar icon height
    /// </summary>
    public static int ToolbarIconHeight { get; set; } = Constants.TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Gets, sets value of image quality for editting
    /// </summary>
    public static int ImageEditQuality { get; set; } = 80;

    /// <summary>
    /// Gets, sets value of duration to display the in-app message
    /// </summary>
    public static int InAppMessageDuration { get; set; } = 2000;

    #endregion


    #region String items

    /// <summary>
    /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
    /// </summary>
    public static string ColorProfile { get; set; } = Constants.CURRENT_MONITOR_PROFILE;

    /// <summary>
    /// Gets, sets the last time to check for update. Set it to "0" to disable auto-update.
    /// </summary>
    public static string AutoUpdate { get; set; } = "7/20/2010 12:13:08";

    /// <summary>
    /// Gets, sets the absolute file path of the last seen image
    /// </summary>
    public static string LastSeenImagePath { get; set; } = "";

    ///// <summary>
    ///// Gets, sets the absolute file path of the exiftool executable file
    ///// </summary>
    //public static string ExifToolExePath { get; set; } = "";

    ///// <summary>
    ///// Gets, sets the custom arguments for Exif tool command
    ///// </summary>
    //public static string ExifToolCommandArgs { get; set; } = "";

    #endregion


    #region Array items

    ///// <summary>
    ///// Gets, sets the list of Image Editing Association
    ///// </summary>
    //public static List<EditApp> EditApps { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of supported image formats
    /// </summary>
    public static HashSet<string> AllFormats { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of formats that only load the first page forcefully
    /// </summary>
    public static HashSet<string> SinglePageFormats { get; set; } = new() { "*.heic;*.heif;*.psd;" };

    ///// <summary>
    ///// Gets, sets the list of keycombo actions
    ///// </summary>
    //public static Dictionary<KeyCombos, AssignableActions> KeyComboActions = Constants.DefaultKeycomboActions;

    /// <summary>
    /// Gets, sets the list of toolbar buttons
    /// </summary>
    public static List<ToolbarItemModel> ToolbarItems { get; set; } = DefaultToolbarItems;

    /// <summary>
    /// Gets, sets the items for displaying image info
    /// </summary>
    public static List<string> InfoItems { get; set; } = DefaultInfoItems;

    /// <summary>
    /// Gets, sets hotkeys list of menu
    /// </summary>
    public static Dictionary<string, Hotkey> MenuHotkeys = new();

    /// <summary>
    /// Gets, sets hotkeys list of image focus mode
    /// </summary>
    public static Dictionary<string, Hotkey> ImageFocusModeHotkeys = new();

    /// <summary>
    /// Gets, sets mouse click actions
    /// </summary>
    public static Dictionary<MouseClickEvent, UserAction> MouseClickActions = new();

    #endregion


    #region Enum items

    /// <summary>
    /// Gets, sets state of main window
    /// </summary>
    public static WindowState FrmMainState { get; set; } = WindowState.Normal;


    ///// <summary>
    ///// Gets, sets state of settings window
    ///// </summary>
    //public static WindowState FrmSettingsState { get; set; } = WindowState.Normal;


    ///// <summary>
    ///// Gets, sets state of exif tool window
    ///// </summary>
    //public static WindowState FrmExifToolState { get; set; } = WindowState.Normal;

    /// <summary>
    /// Gets, sets image loading order
    /// </summary>
    public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;

    /// <summary>
    /// Gets, sets image loading order type
    /// </summary>
    public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;

    ///// <summary>
    ///// Gets, sets action to be performed when user spins the mouse wheel
    ///// </summary>
    //public static MouseWheelActions MouseWheelAction { get; set; } = MouseWheelActions.Zoom;

    ///// <summary>
    ///// Gets, sets action to be performed when user spins the mouse wheel
    ///// while holding Ctrl key
    ///// </summary>
    //public static MouseWheelActions MouseWheelCtrlAction { get; set; } = MouseWheelActions.ScrollVertically;

    ///// <summary>
    ///// Gets, sets action to be performed when user spins the mouse wheel
    ///// while holding Shift key
    ///// </summary>
    //public static MouseWheelActions MouseWheelShiftAction { get; set; } = MouseWheelActions.ScrollHorizontally;

    ///// <summary>
    ///// Gets, sets action to be performed when user spins the mouse wheel
    ///// while holding Alt key
    ///// </summary>
    //public static MouseWheelActions MouseWheelAltAction { get; set; } = MouseWheelActions.BrowseImages;

    /// <summary>
    /// Gets, sets zoom mode value
    /// </summary>
    public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;

    ///// <summary>
    ///// Gets, sets zoom optimization value
    ///// </summary>
    //public static ZoomOptimizationMethods ZoomOptimizationMethod { get; set; } = ZoomOptimizationMethods.Auto;

    ///// <summary>
    ///// Gets, sets toolbar position
    ///// </summary>
    //public static ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Top;

    ///// <summary>
    ///// Gets, sets value indicates what happens after clicking Edit menu
    ///// </summary>
    //public static AfterOpeningEditAppAction AfterEditingAction { get; set; } = AfterOpeningEditAppAction.Nothing;

    #endregion


    #region Other types items

    /// <summary>
    /// Gets, sets background color
    /// </summary>
    public static Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets, sets language pack
    /// </summary>
    public static IgLang Language { get; set; }

    /// <summary>
    /// Gets, sets theme
    /// </summary>
    public static IgTheme Theme { get; set; }

    #endregion

    #endregion




    #region Public functions

    /// <summary>
    /// Loads and parsse configs from file
    /// </summary>
    public static void Load()
    {
        var items = Source.LoadUserConfigs();


        // Boolean values
        #region Boolean items

        //IsSlideshow = items.GetValue(nameof(IsSlideshow), IsSlideshow);
        //IsShowSlideshowCountdown = items.GetValue(nameof(IsShowSlideshowCountdown), IsShowSlideshowCountdown);
        //IsRandomSlideshowInterval = items.GetValue(nameof(IsRandomSlideshowInterval), IsRandomSlideshowInterval);
        EnableFullScreen = items.GetValue(nameof(EnableFullScreen), EnableFullScreen);
        ShowThumbnails = items.GetValue(nameof(ShowThumbnails), ShowThumbnails);
        ShowThumbnailScrollbars = items.GetValue(nameof(ShowThumbnailScrollbars), ShowThumbnailScrollbars);
        ShowThumbnailFilename = items.GetValue(nameof(ShowThumbnailFilename), ShowThumbnailFilename);
        //IsCenterImage = items.GetValue(nameof(IsCenterImage), IsCenterImage);
        //IsColorPickerRGBA = items.GetValue(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
        //IsColorPickerHEXA = items.GetValue(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
        //IsColorPickerHSLA = items.GetValue(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
        //IsColorPickerHSVA = items.GetValue(nameof(IsColorPickerHSVA), IsColorPickerHSVA);
        //IsShowWelcome = items.GetValue(nameof(IsShowWelcome), IsShowWelcome);
        ShowToolbar = items.GetValue(nameof(ShowToolbar), ShowToolbar);
        //IsLoopBackSlideshow = items.GetValue(nameof(IsLoopBackSlideshow), IsLoopBackSlideshow);
        EnableLoopBackNavigation = items.GetValue(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        //EnablePressESCToQuit = items.GetValue(nameof(EnablePressESCToQuit), EnablePressESCToQuit);
        ShowCheckerBoard = items.GetValue(nameof(ShowCheckerBoard), ShowCheckerBoard);
        EnableMultiInstances = items.GetValue(nameof(EnableMultiInstances), EnableMultiInstances);
        EnableWindowTopMost = items.GetValue(nameof(EnableWindowTopMost), EnableWindowTopMost);
        //IsWindowFrameless = items.GetValue(nameof(IsWindowFrameless), IsWindowFrameless);
        //IsThumbnailHorizontal = items.GetValue(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
        ShowDeleteConfirmation = items.GetValue(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        ShowSaveOverrideConfirmation = items.GetValue(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        //IsScrollbarsVisible = items.GetValue(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
        //IsSaveAfterRotating = items.GetValue(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
        PreserveModifiedDate = items.GetValue(nameof(PreserveModifiedDate), PreserveModifiedDate);
        IsNewVersionAvailable = items.GetValue(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
        //IsDisplayBasenameOfImage = items.GetValue(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
        CenterToolbar = items.GetValue(nameof(CenterToolbar), CenterToolbar);
        OpenLastSeenImage = items.GetValue(nameof(OpenLastSeenImage), OpenLastSeenImage);
        ApplyColorProfileForAll = items.GetValue(nameof(ApplyColorProfileForAll), ApplyColorProfileForAll);
        EnableNavigationButtons = items.GetValue(nameof(EnableNavigationButtons), EnableNavigationButtons);
        ShowCheckerboardOnlyImageRegion = items.GetValue(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        EnableRecursiveLoading = items.GetValue(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        UseFileExplorerSortOrder = items.GetValue(nameof(UseFileExplorerSortOrder), UseFileExplorerSortOrder);
        GroupImagesByDirectory = items.GetValue(nameof(GroupImagesByDirectory), GroupImagesByDirectory);
        IncludeHiddenImages = items.GetValue(nameof(IncludeHiddenImages), IncludeHiddenImages);
        //IsShowColorPickerOnStartup = items.GetValue(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
        //IsShowPageNavOnStartup = items.GetValue(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);
        //IsShowPageNavAuto = items.GetValue(nameof(IsShowPageNavAuto), IsShowPageNavAuto);
        //IsWindowFit = items.GetValue(nameof(IsWindowFit), IsWindowFit);
        //IsCenterWindowFit = items.GetValue(nameof(IsCenterWindowFit), IsCenterWindowFit);
        //IsUseTouchGesture = items.GetValue(nameof(IsUseTouchGesture), IsUseTouchGesture);
        //IsHideTooltips = items.GetValue(nameof(IsHideTooltips), IsHideTooltips);
        //IsExifToolAlwaysOnTop = items.GetValue(nameof(IsExifToolAlwaysOnTop), IsExifToolAlwaysOnTop);
        //IsUseEmptyTitleBar = items.GetValue(nameof(IsUseEmptyTitleBar), IsUseEmptyTitleBar);
        UseRawThumbnail = items.GetValue(nameof(UseRawThumbnail), UseRawThumbnail);
        HideToolbarInFullscreen = items.GetValue(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        HideThumbnailsInFullscreen = items.GetValue(nameof(HideThumbnailsInFullscreen), HideThumbnailsInFullscreen);
        EnableImageFocusMode = items.GetValue(nameof(EnableImageFocusMode), EnableImageFocusMode);

        #endregion


        // Number values
        #region Number items

        // FrmMain
        FrmMainPositionX = items.GetValue(nameof(FrmMainPositionX), FrmMainPositionX);
        FrmMainPositionY = items.GetValue(nameof(FrmMainPositionY), FrmMainPositionY);
        FrmMainWidth = items.GetValue(nameof(FrmMainWidth), FrmMainWidth);
        FrmMainHeight = items.GetValue(nameof(FrmMainHeight), FrmMainHeight);

        PanSpeed = items.GetValue(nameof(PanSpeed), PanSpeed);
        ZoomSpeed = items.GetValue(nameof(ZoomSpeed), ZoomSpeed);

        //// FrmSettings
        //FrmSettingsPositionX = items.GetValue(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        //FrmSettingsPositionY = items.GetValue(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        //FrmSettingsWidth = items.GetValue(nameof(FrmSettingsWidth), FrmSettingsWidth);
        //FrmSettingsHeight = items.GetValue(nameof(FrmSettingsHeight), FrmSettingsHeight);

        //// FrmExifTool
        //FrmExifToolPositionX = items.GetValue(nameof(FrmExifToolPositionX), FrmExifToolPositionX);
        //FrmExifToolPositionY = items.GetValue(nameof(FrmExifToolPositionY), FrmExifToolPositionY);
        //FrmExifToolWidth = items.GetValue(nameof(FrmExifToolWidth), FrmExifToolWidth);
        //FrmExifToolHeight = items.GetValue(nameof(FrmExifToolHeight), FrmExifToolHeight);


        //FirstLaunchVersion = items.GetValue(nameof(FirstLaunchVersion), FirstLaunchVersion);

        #region Slide show
        //SlideShowInterval = items.GetValue(nameof(SlideShowInterval), SlideShowInterval);
        //if (SlideShowInterval < 1) SlideShowInterval = 5;

        //SlideShowIntervalTo = items.GetValue(nameof(SlideShowIntervalTo), SlideShowIntervalTo);
        //SlideShowIntervalTo = Math.Max(SlideShowIntervalTo, SlideShowInterval);
        #endregion

        #region Load thumbnail bar width & position
        ThumbnailSize = items.GetValue(nameof(ThumbnailSize), ThumbnailSize);

        //if (IsThumbnailHorizontal)
        //{
        //    // Get minimum width needed for thumbnail dimension
        //    var tbMinWidth = new ThumbnailItemInfo(ThumbnailDimension, true).GetTotalDimension();

        //    // Get the greater width value
        //    ThumbnailBarWidth = Math.Max(ThumbnailBarWidth, tbMinWidth);
        //}
        //else
        //{
        //    ThumbnailBarWidth = items.GetValue(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
        //}
        #endregion

        ImageBoosterCachedCount = items.GetValue(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
        ImageBoosterCachedCount = Math.Max(0, Math.Min(ImageBoosterCachedCount, 10));

        ZoomLockValue = items.GetValue(nameof(ZoomLockValue), ZoomLockValue);
        if (ZoomLockValue < 0) ZoomLockValue = 100f;

        ToolbarIconHeight = items.GetValue(nameof(ToolbarIconHeight), ToolbarIconHeight);
        ImageEditQuality = items.GetValue(nameof(ImageEditQuality), ImageEditQuality);
        InAppMessageDuration = items.GetValue(nameof(InAppMessageDuration), InAppMessageDuration);

        #endregion


        // Enum values
        #region Enum items

        FrmMainState = items.GetValue(nameof(FrmMainState), FrmMainState);
        //FrmSettingsState = items.GetValue(nameof(FrmSettingsState), FrmSettingsState);
        //FrmExifToolState = items.GetValue(nameof(FrmExifToolState), FrmExifToolState);
        ImageLoadingOrder = items.GetValue(nameof(ImageLoadingOrder), ImageLoadingOrder);
        ImageLoadingOrderType = items.GetValue(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
        //MouseWheelAction = items.GetValue(nameof(MouseWheelAction), MouseWheelAction);
        //MouseWheelCtrlAction = items.GetValue(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction);
        //MouseWheelShiftAction = items.GetValue(nameof(MouseWheelShiftAction), MouseWheelShiftAction);
        //MouseWheelAltAction = items.GetValue(nameof(MouseWheelAltAction), MouseWheelAltAction);
        ZoomMode = items.GetValue(nameof(ZoomMode), ZoomMode);
        //ZoomOptimizationMethod = items.GetValue(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod);
        //ToolbarPosition = items.GetValue(nameof(ToolbarPosition), ToolbarPosition);
        //AfterEditingAction = items.GetValue(nameof(AfterEditingAction), AfterEditingAction);


        #endregion


        // String values
        #region String items

        ColorProfile = items.GetValue(nameof(ColorProfile), ColorProfile);
        ColorProfile = Helpers.GetCorrectColorProfileName(ColorProfile);

        AutoUpdate = items.GetValue(nameof(AutoUpdate), AutoUpdate);
        LastSeenImagePath = items.GetValue(nameof(LastSeenImagePath), LastSeenImagePath);
        //ExifToolExePath = items.GetValue(nameof(ExifToolExePath), ExifToolExePath);
        //ExifToolCommandArgs = items.GetValue(nameof(ExifToolCommandArgs), ExifToolCommandArgs);

        #endregion


        // Array values
        #region Array items

        #region EditApps

        //var appStr = items.GetValue(nameof(EditApps), "");
        //EditApps = GetEditApps(appStr);

        #endregion

        #region ImageFormats

        var formats = items.GetValue(nameof(AllFormats), Constants.IMAGE_FORMATS);
        AllFormats = GetImageFormats(formats);

        formats = items.GetValue(nameof(SinglePageFormats), string.Join(";", SinglePageFormats));
        SinglePageFormats = GetImageFormats(formats);

        #endregion

        #region KeyComboActions

        //var keyActionStr = items.GetValue(nameof(KeyComboActions), "");
        //if (!string.IsNullOrEmpty(keyActionStr))
        //{
        //    KeyComboActions = GetKeyComboActions(keyActionStr);
        //}

        #endregion


        // toolbar items
        var toolbarItems = items.GetSection(nameof(ToolbarItems))
            .GetChildren()
            .Select(i => i.Get<ToolbarItemModel>());
        ToolbarItems = toolbarItems.Any() ? toolbarItems.ToList() : DefaultToolbarItems;

        
        // info items
        var infoItems = items.GetSection(nameof(InfoItems))
            .GetChildren()
            .Select(i => i.Get<string>());
        InfoItems = infoItems.Any() ? infoItems.ToList() : DefaultInfoItems;


        // hotkeys for menu
        var stringDict = items.GetSection(nameof(MenuHotkeys))
            .GetChildren()
            .ToDictionary(i => i.Key, i => i.Value);
        MenuHotkeys = ParseHotkeys(stringDict);


        // hotkeys for menu
        stringDict = items.GetSection(nameof(ImageFocusModeHotkeys))
            .GetChildren()
            .ToDictionary(i => i.Key, i => i.Value);
        ImageFocusModeHotkeys = ParseHotkeys(stringDict);


        // mouse actions
        MouseClickActions = items.GetSection(nameof(MouseClickActions))
            .GetChildren()
            .ToDictionary(
                i => Helpers.ParseEnum<MouseClickEvent>(i.Key),
                i => i.Get<UserAction>());
        #endregion


        // Other types values
        #region Other types items

        #region Language
        var langPath = items.GetValue(nameof(Language), "English");
        Language = new IgLang(langPath, App.StartUpDir(Dir.Languages));
        #endregion


        #region Theme
        var themeFolderName = items.GetValue(nameof(Theme), Constants.DEFAULT_THEME);
        var th = new IgTheme(App.ConfigDir(PathType.Dir, Dir.Themes, themeFolderName));

        if (th.IsValid)
        {
            Theme = th;
        }
        else
        {
            // load default theme
            Theme = new(App.StartUpDir(Dir.Themes, Constants.DEFAULT_THEME));
        }

        if (!Theme.IsValid)
        {
            throw new InvalidDataException($"Unable to load '{th.FolderName}' theme pack. " +
                $"Please make sure '{th.FolderName}\\{IgTheme.CONFIG_FILE}' file is valid.");
        }
        #endregion


        #region BackgroundColor

        // must load after Theme
        var bgValue = items.GetValue(nameof(BackgroundColor), string.Empty);

        if (string.IsNullOrEmpty(bgValue))
        {
            BackgroundColor = Theme.Settings.BgColor;
        }
        else
        {
            BackgroundColor = ThemeUtils.ColorFromHex(bgValue, true);
        }
        #endregion

        #endregion

    }


    /// <summary>
    /// Parses and writes configs to file
    /// </summary>
    public static void Write()
    {
        var jsonFile = App.ConfigDir(PathType.File, Source.UserFilename);
        var jsonObj = PrepareJsonSettingObjects();

        Helpers.WriteJson(jsonFile, jsonObj);
    }


    /// <summary>
    /// Gets property by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
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
    /// Show the default error popup for igcmd/igcmd10.exe
    /// </summary>
    /// <returns><see cref="IgExitCode.Error"/></returns>
    public static int ShowDefaultIgCommandError(string igcmdExeName)
    {
        var url = "https://imageglass.org/docs/command-line-utilities";
        var langPath = $"_._IgCommandExe._DefaultError";

        var result = ShowError(
            title: Application.ProductName + " " + Application.ProductVersion,
            heading: Language[$"{langPath}._Heading"],
            description: string.Format(Language[$"{langPath}._Description"], url),
            buttons: PopupButtons.LearnMore_Close);


        if (result.ExitResult == PopupExitResult.OK)
        {
            Helpers.OpenUrl(url, $"{igcmdExeName}_invalid_command");
        }

        return (int)IgExitCode.Error;
    }


    /// <summary>
    /// Shows unhandled exception popup
    /// </summary>
    public static void HandleException(Exception ex)
    {
        var exeVersion = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;
        var appInfo = Application.ProductName + " v" + exeVersion;
        var osInfo = Environment.OSVersion.VersionString + " " + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");

        var langPath = $"_._UnhandledException";
        var description = Language[$"{langPath}._Description"];
        var details =
            $"Version: {appInfo}\r\n" +
            $"Release code: {Constants.APP_CODE}\r\n" +
            $"OS: {osInfo}\r\n\r\n" +

            $"-------------------------------------------------------------\r\n" +
            $"Error:\r\n\r\n" +
            $"{ex.Message}\r\n" +
            $"-------------------------------------------------------------\r\n\r\n" +

            ex.ToString();

        var result = ShowError(
            title: Application.ProductName + " - " + Language[langPath],
            heading: ex.Message,
            description: description,
            details: details,
            buttons: PopupButtons.Continue_Quit);

        if (result.ExitResult == PopupExitResult.OK)
        {
            Application.Exit();
        }
    }



    /// <summary>
    /// Shows information popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowInfo(
        string description = "",
        string title = "",
        string heading = "",
        string details = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_INFO,
        Image? thumbnail = null,
        PopupButtons buttons = PopupButtons.OK,
        string optionText = "")
    {
        SystemSounds.Question.Play();

        return Popup.ShowDialog(Theme, Language, description, title, heading, details, buttons, icon, thumbnail, optionText);
    }


    /// <summary>
    /// Shows warning popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowWarning(
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_WARNING,
        Image? thumbnail = null,
        PopupButtons buttons = PopupButtons.OK,
        string optionText = "")
    {
        heading ??= Language["_._Warning"];

        SystemSounds.Exclamation.Play();

        return Popup.ShowDialog(Theme, Language, description, title, heading, details, buttons, icon, thumbnail, optionText);
    }


    /// <summary>
    /// Shows error popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowError(
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_ERROR,
        Image? thumbnail = null,
        PopupButtons buttons = PopupButtons.OK,
        string optionText = "")
    {
        heading ??= Language["_._Error"];

        SystemSounds.Asterisk.Play();

        return Popup.ShowDialog(Theme, Language, description, title, heading, details, buttons, icon, thumbnail, optionText);
    }


    #endregion


    #region Private functions

    /// <summary>
    /// Converts all settings to ExpandoObject for parsing JSON
    /// </summary>
    /// <returns></returns>
    private static dynamic PrepareJsonSettingObjects()
    {
        var settings = new ExpandoObject();

        var metadata = new ConfigMetadata()
        {
            Description = _source.Description,
            Version = _source.Version,
        };

        settings.TryAdd("_Metadata", metadata);


        #region Boolean items

        //settings.TryAdd(nameof(IsSlideshow), IsSlideshow);
        //settings.TryAdd(nameof(IsShowSlideshowCountdown), IsShowSlideshowCountdown);
        //settings.TryAdd(nameof(IsRandomSlideshowInterval), IsRandomSlideshowInterval);
        settings.TryAdd(nameof(EnableFullScreen), EnableFullScreen);
        settings.TryAdd(nameof(ShowThumbnails), ShowThumbnails);
        settings.TryAdd(nameof(ShowThumbnailScrollbars), ShowThumbnailScrollbars);
        settings.TryAdd(nameof(ShowThumbnailFilename), ShowThumbnailFilename);
        //settings.TryAdd(nameof(IsCenterImage), IsCenterImage);
        //settings.TryAdd(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
        //settings.TryAdd(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
        //settings.TryAdd(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
        //settings.TryAdd(nameof(IsColorPickerHSVA), IsColorPickerHSVA);
        //settings.TryAdd(nameof(IsShowWelcome), IsShowWelcome);
        settings.TryAdd(nameof(ShowToolbar), ShowToolbar);
        //settings.TryAdd(nameof(IsLoopBackSlideshow), IsLoopBackSlideshow);
        settings.TryAdd(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        //settings.TryAdd(nameof(EnablePressESCToQuit), EnablePressESCToQuit);
        settings.TryAdd(nameof(ShowCheckerBoard), ShowCheckerBoard);
        settings.TryAdd(nameof(EnableMultiInstances), EnableMultiInstances);
        settings.TryAdd(nameof(EnableWindowTopMost), EnableWindowTopMost);
        //settings.TryAdd(nameof(IsWindowFrameless), IsWindowFrameless);
        //settings.TryAdd(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
        settings.TryAdd(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        settings.TryAdd(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        //settings.TryAdd(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
        //settings.TryAdd(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
        settings.TryAdd(nameof(PreserveModifiedDate), PreserveModifiedDate);
        settings.TryAdd(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
        //settings.TryAdd(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
        settings.TryAdd(nameof(CenterToolbar), CenterToolbar);
        settings.TryAdd(nameof(OpenLastSeenImage), OpenLastSeenImage);
        settings.TryAdd(nameof(ApplyColorProfileForAll), ApplyColorProfileForAll);
        settings.TryAdd(nameof(EnableNavigationButtons), EnableNavigationButtons);
        settings.TryAdd(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        settings.TryAdd(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        settings.TryAdd(nameof(UseFileExplorerSortOrder), UseFileExplorerSortOrder);
        settings.TryAdd(nameof(GroupImagesByDirectory), GroupImagesByDirectory);
        settings.TryAdd(nameof(IncludeHiddenImages), IncludeHiddenImages);
        //settings.TryAdd(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
        //settings.TryAdd(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);
        //settings.TryAdd(nameof(IsShowPageNavAuto), IsShowPageNavAuto);
        //settings.TryAdd(nameof(IsWindowFit), IsWindowFit);
        //settings.TryAdd(nameof(IsCenterWindowFit), IsCenterWindowFit);
        //settings.TryAdd(nameof(IsUseTouchGesture), IsUseTouchGesture);
        //settings.TryAdd(nameof(IsHideTooltips), IsHideTooltips);
        //settings.TryAdd(nameof(IsExifToolAlwaysOnTop), IsExifToolAlwaysOnTop);
        //settings.TryAdd(nameof(IsUseEmptyTitleBar), IsUseEmptyTitleBar);
        settings.TryAdd(nameof(UseRawThumbnail), UseRawThumbnail);
        settings.TryAdd(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        settings.TryAdd(nameof(HideThumbnailsInFullscreen), HideThumbnailsInFullscreen);
        settings.TryAdd(nameof(EnableImageFocusMode), EnableImageFocusMode);

        #endregion


        #region Number items

        // FrmMain
        settings.TryAdd(nameof(FrmMainPositionX), FrmMainPositionX);
        settings.TryAdd(nameof(FrmMainPositionY), FrmMainPositionY);
        settings.TryAdd(nameof(FrmMainWidth), FrmMainWidth);
        settings.TryAdd(nameof(FrmMainHeight), FrmMainHeight);

        settings.TryAdd(nameof(PanSpeed), PanSpeed);
        settings.TryAdd(nameof(ZoomSpeed), ZoomSpeed);

        //// FrmSettings
        //settings.TryAdd(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        //settings.TryAdd(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        //settings.TryAdd(nameof(FrmSettingsWidth), FrmSettingsWidth);
        //settings.TryAdd(nameof(FrmSettingsHeight), FrmSettingsHeight);

        //// FrmExifTool
        //settings.TryAdd(nameof(FrmExifToolPositionX), FrmExifToolPositionX);
        //settings.TryAdd(nameof(FrmExifToolPositionY), FrmExifToolPositionY);
        //settings.TryAdd(nameof(FrmExifToolWidth), FrmExifToolWidth);
        //settings.TryAdd(nameof(FrmExifToolHeight), FrmExifToolHeight);

        //settings.TryAdd(nameof(FirstLaunchVersion), FirstLaunchVersion);
        //settings.TryAdd(nameof(SlideShowInterval), SlideShowInterval);
        //settings.TryAdd(nameof(SlideShowIntervalTo), SlideShowIntervalTo);
        settings.TryAdd(nameof(ThumbnailSize), ThumbnailSize);
        //settings.TryAdd(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
        settings.TryAdd(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
        settings.TryAdd(nameof(ZoomLockValue), ZoomLockValue);
        settings.TryAdd(nameof(ToolbarIconHeight), ToolbarIconHeight);
        settings.TryAdd(nameof(ImageEditQuality), ImageEditQuality);
        settings.TryAdd(nameof(InAppMessageDuration), InAppMessageDuration);

        #endregion


        #region Enum items

        settings.TryAdd(nameof(FrmMainState), FrmMainState.ToString());
        //settings.TryAdd(nameof(FrmSettingsState), FrmSettingsState.ToString());
        //settings.TryAdd(nameof(FrmExifToolState), FrmExifToolState.ToString());
        settings.TryAdd(nameof(ImageLoadingOrder), ImageLoadingOrder.ToString());
        settings.TryAdd(nameof(ImageLoadingOrderType), ImageLoadingOrderType.ToString());
        //settings.TryAdd(nameof(MouseWheelAction), MouseWheelAction.ToString());
        //settings.TryAdd(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction.ToString());
        //settings.TryAdd(nameof(MouseWheelShiftAction), MouseWheelShiftAction.ToString());
        //settings.TryAdd(nameof(MouseWheelAltAction), MouseWheelAltAction.ToString());
        settings.TryAdd(nameof(ZoomMode), ZoomMode.ToString());
        //settings.TryAdd(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod.ToString());
        //settings.TryAdd(nameof(ToolbarPosition), ToolbarPosition.ToString());
        //settings.TryAdd(nameof(AfterEditingAction), AfterEditingAction.ToString());

        #endregion


        #region String items

        settings.TryAdd(nameof(ColorProfile), ColorProfile);
        settings.TryAdd(nameof(AutoUpdate), AutoUpdate);
        settings.TryAdd(nameof(LastSeenImagePath), LastSeenImagePath);
        //settings.TryAdd(nameof(ExifToolExePath), ExifToolExePath);
        //settings.TryAdd(nameof(ExifToolCommandArgs), ExifToolCommandArgs);

        #endregion


        #region Array items

        //settings.TryAdd(nameof(EditApps), GetEditApps(EditApps));
        settings.TryAdd(nameof(AllFormats), GetImageFormats(AllFormats));
        settings.TryAdd(nameof(SinglePageFormats), GetImageFormats(SinglePageFormats));
        //settings.TryAdd(nameof(KeyComboActions), GetKeyComboActions(KeyComboActions));
        settings.TryAdd(nameof(ToolbarItems), ToolbarItems);
        settings.TryAdd(nameof(InfoItems), InfoItems);
        settings.TryAdd(nameof(MenuHotkeys), ParseHotkeys(MenuHotkeys));
        settings.TryAdd(nameof(ImageFocusModeHotkeys), ParseHotkeys(ImageFocusModeHotkeys));
        settings.TryAdd(nameof(MouseClickActions), ParseMouseClickActions(MouseClickActions));
        #endregion


        #region Other types items

        settings.TryAdd(nameof(BackgroundColor), ThemeUtils.ColorToHex(BackgroundColor));
        settings.TryAdd(nameof(Language), Path.GetFileName(Language.FileName));
        settings.TryAdd(nameof(Theme), Theme.FolderName);

        #endregion


        return settings;
    }


    #region ImageFormats

    /// <summary>
    /// Returns distinc list of image formats.
    /// </summary>
    /// <param name="formats">The format string. E.g: *.bpm;*.jpg;</param>
    /// <returns></returns>
    public static HashSet<string> GetImageFormats(string formats)
    {
        var list = new HashSet<string>();
        var formatList = formats.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        char[] wildTrim = { '*' };

        foreach (var ext in formatList)
        {
            list.Add(ext.Trim(wildTrim));
        }

        return list;
    }

    /// <summary>
    /// Returns the image formats string. Example: <c>*.png;*.jpg;</c>
    /// </summary>
    /// <param name="list">The input HashSet</param>
    /// <returns></returns>
    public static string GetImageFormats(HashSet<string> list)
    {
        var sb = new StringBuilder(list.Count);
        foreach (var item in list)
        {
            sb.Append('*').Append(item).Append(';');
        }

        return sb.ToString();
    }

    #endregion


    #endregion


    #region Public static functions

    /// <summary>
    /// Apply theme colors and logo to form
    /// </summary>
    public static void ApplyFormTheme(Form frm, IgTheme th)
    {
        // load theme colors
        foreach (var ctr in Helpers.GetAllControls(frm, typeof(LinkLabel)))
        {
            if (ctr is LinkLabel lnk)
            {
                lnk.LinkColor = lnk.VisitedLinkColor = Theme.Settings.AccentColor;
            }
        }

        // Icon theming
        if (!th.Settings.IsShowTitlebarLogo)
        {
            frm.Icon = Icon.FromHandle(new Bitmap(64, 64).GetHicon());
            FormIconApi.SetTaskbarIcon(frm, th.Settings.AppLogo.GetHicon());
        }
        else
        {
            frm.Icon = Icon.FromHandle(th.Settings.AppLogo.GetHicon());
        }
    }


    /// <summary>
    /// Parses string dictionary to hotkey dictionary
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, Hotkey> ParseHotkeys(Dictionary<string, string> dict)
    {
        var result = new Dictionary<string, Hotkey>();

        foreach (var item in dict)
        {
            Hotkey keyCombo;
            try
            {
                // sample item: { "MnuOpen": "Ctrl+O" }
                keyCombo = new Hotkey(item.Value);
            }
            catch
            {
                keyCombo = new Hotkey();
            }

            if (result.ContainsKey(item.Key))
            {
                result[item.Key] = keyCombo;
            }
            else
            {
                result.Add(item.Key, keyCombo);
            }
        };

        return result;
    }


    /// <summary>
    /// Parses hotkey dictionary to string dictionary
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> ParseHotkeys(Dictionary<string, Hotkey> dict)
    {
        return dict.Select(i => new
        {
            Key = i.Key,
            Value = i.Value.ToString(),
        }).ToDictionary(i => i.Key, i => i.Value);
    }


    /// <summary>
    /// Merge <paramref name="srcDict"/> dictionary
    /// into <paramref name="destDict"/> dictionary
    /// </summary>
    /// <returns></returns>
    public static void MergeHotkeys(ref Dictionary<string, Hotkey> destDict, Dictionary<string, Hotkey> srcDict)
    {
        foreach (var item in srcDict)
        {
            if (destDict.ContainsKey(item.Key))
            {
                destDict[item.Key] = item.Value;
            }
            else
            {
                destDict.Add(item.Key, item.Value);
            }
        }
    }


    /// <summary>
    /// Gets hotkey string
    /// </summary>
    /// <returns></returns>
    public static string GetHotkeyString(Dictionary<string, Hotkey> dict, string dictKey)
    {
        dict.TryGetValue(dictKey, out var hotkey);

        return hotkey?.ToString() ?? string.Empty;
    }


    /// <summary>
    /// Gets hotkey
    /// </summary>
    public static Hotkey? GetHotkey(Dictionary<string, Hotkey> dict, string dictKey)
    {
        dict.TryGetValue(dictKey, out var hotkey);

        return hotkey;
    }

    
    /// <summary>
    /// Gets hotkey's KeyData
    /// </summary>
    /// <returns></returns>
    public static Keys GetHotkeyData(Dictionary<string, Hotkey> dict, string dictKey, Keys defaultValue)
    {
        var hotkey = GetHotkey(dict, dictKey);

        return hotkey?.KeyData ?? defaultValue;
    }

    
    /// <summary>
    /// Gets hotkey actions
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetHotkeyActions(Dictionary<string, Hotkey> dict, Hotkey dictValue)
    {
        return dict.Where(i => i.Value.ToString() == dictValue.ToString())
            .Select(i => i.Key);
    }



    /// <summary>
    /// Parses <see cref="MouseClickEvent"/> to string
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, UserAction> ParseMouseClickActions(Dictionary<MouseClickEvent, UserAction> dict)
    {
        return dict.ToDictionary(i => i.Key.ToString(), i => i.Value);
    }

    #endregion


}
