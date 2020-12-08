/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.Library.FileAssociations;
using ImageGlass.Library.WinAPI;
using ImageGlass.UI;
using Microsoft.Win32;

namespace ImageGlass.Settings {
    /// <summary>
    /// Provide all the settings of the app
    /// </summary>
    public static class Configs {
        /// <summary>
        /// Configuration Source file
        /// </summary>
        private static ConfigSource Source { get; } = new();

        /// <summary>
        /// Check if the config file is compatible with this ImageGlass version or not.
        /// </summary>
        public static bool IsCompatible => Source.IsCompatible;


        #region Public configs

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
        /// Gets, sets value indicating that frmMain is always on top or not.
        /// </summary>
        public static bool IsWindowAlwaysOnTop { get; set; } = false;

        /// <summary>
        /// Gets, sets value of frmMain's frameless mode.
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
        /// Gets, sets value indicates that to keep the title bar of frmMain empty
        /// </summary>
        public static bool IsUseEmptyTitleBar { get; set; } = false;

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


        #region String items

        /// <summary>
        /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
        /// </summary>
        public static string ColorProfile { get; set; } = "sRGB";

        /// <summary>
        /// Gets, sets the last time to check for update. Set it to "0" to disable auto-update.
        /// </summary>
        public static string AutoUpdate { get; set; } = "7/26/1991 12:13:08";

        /// <summary>
        /// Gets, sets the absolute file path of the last seen image
        /// </summary>
        public static string LastSeenImagePath { get; set; } = "";

        /// <summary>
        /// Gets, sets the absolute file path of the exiftool executable file
        /// </summary>
        public static string ExifToolExePath { get; set; } = "";

        #endregion


        #region Array items

        /// <summary>
        /// Gets, sets zoom levels of the viewer
        /// </summary>
        public static int[] ZoomLevels { get; set; } = new int[] { 5, 10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100, 125, 150, 175, 200, 250, 300, 350, 400, 500, 600, 700, 800, 1000, 1200, 1500, 1800, 2100, 2500, 3000, 3500 };

        /// <summary>
        /// Gets, sets the list of Image Editing Association
        /// </summary>
        public static List<EditApp> EditApps { get; set; } = new();

        /// <summary>
        /// Gets, sets the list of supported image formats
        /// </summary>
        public static HashSet<string> AllFormats { get; set; } = new();

        /// <summary>
        /// Gets, sets the list of keycombo actions
        /// </summary>
        public static Dictionary<KeyCombos, AssignableActions> KeyComboActions = Constants.DefaultKeycomboActions;

        /// <summary>
        /// Gets, sets the list of toolbar buttons
        /// </summary>
        public static List<ToolbarButton> ToolbarButtons { get; set; } = Constants.DefaultToolbarButtons;

        #endregion


        #region Enum items

        /// <summary>
        /// Gets, sets state of main window
        /// </summary>
        public static FormWindowState FrmMainWindowState { get; set; } = FormWindowState.Normal;

        /// <summary>
        /// Gets, sets state of settings window
        /// </summary>
        public static FormWindowState FrmSettingsWindowState { get; set; } = FormWindowState.Normal;

        /// <summary>
        /// Gets, sets state of exif tool window
        /// </summary>
        public static FormWindowState FrmExifToolWindowState { get; set; } = FormWindowState.Normal;

        /// <summary>
        /// Gets, sets image loading order
        /// </summary>
        public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;

        /// <summary>
        /// Gets, sets image loading order type
        /// </summary>
        public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;

        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel
        /// </summary>
        public static MouseWheelActions MouseWheelAction { get; set; } = MouseWheelActions.Zoom;

        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Ctrl key
        /// </summary>
        public static MouseWheelActions MouseWheelCtrlAction { get; set; } = MouseWheelActions.ScrollVertically;

        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Shift key
        /// </summary>
        public static MouseWheelActions MouseWheelShiftAction { get; set; } = MouseWheelActions.ScrollHorizontally;

        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Alt key
        /// </summary>
        public static MouseWheelActions MouseWheelAltAction { get; set; } = MouseWheelActions.BrowseImages;

        /// <summary>
        /// Gets, sets zoom mode value
        /// </summary>
        public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;

        /// <summary>
        /// Gets, sets zoom optimization value
        /// </summary>
        public static ZoomOptimizationMethods ZoomOptimizationMethod { get; set; } = ZoomOptimizationMethods.Auto;

        /// <summary>
        /// Gets, sets toolbar position
        /// </summary>
        public static ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Top;

        /// <summary>
        /// Gets, sets value indicates what happens after clicking Edit menu
        /// </summary>
        public static AfterOpeningEditAppAction AfterEditingAction { get; set; } = AfterOpeningEditAppAction.Nothing;

        #endregion


        #region Other types items

        /// <summary>
        /// Gets, sets background color
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets, sets window bound of main form
        /// </summary>
        public static Rectangle FrmMainWindowBound { get; set; } = new(280, 125, 1300, 800);

        /// <summary>
        /// Gets, sets window bound of main form
        /// </summary>
        public static Rectangle FrmSettingsWindowBound { get; set; } = new(280, 125, 1050, 750);

        /// <summary>
        /// Gets, sets window bound of exif tool form
        /// </summary>
        public static Rectangle FrmExifToolWindowBound { get; set; } = new(280, 125, 800, 600);

        /// <summary>
        /// Gets, sets language pack
        /// </summary>
        public static Language Language { get; set; } = new();

        /// <summary>
        /// Gets, sets theme
        /// </summary>
        public static Theme Theme { get; set; }

        #endregion

        #endregion


        #region Private methods

        /// <summary>
        /// Convert the given object to Enum type
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private static T ParseEnum<T>(object value) {
            return (T)Enum.Parse(typeof(T), value.ToString(), true);
        }

        /// <summary>
        /// Gets config item from ConfigSource
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key of the config</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        private static T Get<T>(string key, object defaultValue) {
            try {
                return ConvertType<T>(Source[key]);
            }
            catch { }

            // return default value
            return ConvertType<T>(defaultValue);
        }

        /// <summary>
        /// Set the given config to ConfigSource
        /// </summary>
        /// <param name="key">Key of the config</param>
        /// <param name="value">Value</param>
        private static void Set(string key, object value) {
            Source[key] = value.ToString();
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Load and parse configs from file
        /// </summary>
        public static void Load() {
            // load user configs from file
            Source.LoadUserConfigs();

            // load configs to public properties
            #region bool items

            IsSlideshow = Get<bool>(nameof(IsSlideshow), IsSlideshow);
            IsShowSlideshowCountdown = Get<bool>(nameof(IsShowSlideshowCountdown), IsShowSlideshowCountdown);
            IsRandomSlideshowInterval = Get<bool>(nameof(IsRandomSlideshowInterval), IsRandomSlideshowInterval);
            IsFullScreen = Get<bool>(nameof(IsFullScreen), IsFullScreen);
            IsShowThumbnail = Get<bool>(nameof(IsShowThumbnail), IsShowThumbnail);
            IsCenterImage = Get<bool>(nameof(IsCenterImage), IsCenterImage);
            IsColorPickerRGBA = Get<bool>(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
            IsColorPickerHEXA = Get<bool>(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
            IsColorPickerHSLA = Get<bool>(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
            IsColorPickerHSVA = Get<bool>(nameof(IsColorPickerHSVA), IsColorPickerHSVA);
            IsShowWelcome = Get<bool>(nameof(IsShowWelcome), IsShowWelcome);
            IsShowToolBar = Get<bool>(nameof(IsShowToolBar), IsShowToolBar);
            IsShowThumbnailScrollbar = Get<bool>(nameof(IsShowThumbnailScrollbar), IsShowThumbnailScrollbar);
            IsLoopBackSlideshow = Get<bool>(nameof(IsLoopBackSlideshow), IsLoopBackSlideshow);
            IsLoopBackViewer = Get<bool>(nameof(IsLoopBackViewer), IsLoopBackViewer);
            IsPressESCToQuit = Get<bool>(nameof(IsPressESCToQuit), IsPressESCToQuit);
            IsShowCheckerBoard = Get<bool>(nameof(IsShowCheckerBoard), IsShowCheckerBoard);
            IsAllowMultiInstances = Get<bool>(nameof(IsAllowMultiInstances), IsAllowMultiInstances);
            IsWindowAlwaysOnTop = Get<bool>(nameof(IsWindowAlwaysOnTop), IsWindowAlwaysOnTop);
            IsWindowFrameless = Get<bool>(nameof(IsWindowFrameless), IsWindowFrameless);
            IsThumbnailHorizontal = Get<bool>(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
            IsConfirmationDelete = Get<bool>(nameof(IsConfirmationDelete), IsConfirmationDelete);
            IsScrollbarsVisible = Get<bool>(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
            IsSaveAfterRotating = Get<bool>(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
            IsPreserveModifiedDate = Get<bool>(nameof(IsPreserveModifiedDate), IsPreserveModifiedDate);
            IsNewVersionAvailable = Get<bool>(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
            IsDisplayBasenameOfImage = Get<bool>(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
            IsCenterToolbar = Get<bool>(nameof(IsCenterToolbar), IsCenterToolbar);
            IsOpenLastSeenImage = Get<bool>(nameof(IsOpenLastSeenImage), IsOpenLastSeenImage);
            IsApplyColorProfileForAll = Get<bool>(nameof(IsApplyColorProfileForAll), IsApplyColorProfileForAll);
            IsShowNavigationButtons = Get<bool>(nameof(IsShowNavigationButtons), IsShowNavigationButtons);
            IsShowCheckerboardOnlyImageRegion = Get<bool>(nameof(IsShowCheckerboardOnlyImageRegion), IsShowCheckerboardOnlyImageRegion);
            IsRecursiveLoading = Get<bool>(nameof(IsRecursiveLoading), IsRecursiveLoading);
            IsUseFileExplorerSortOrder = Get<bool>(nameof(IsUseFileExplorerSortOrder), IsUseFileExplorerSortOrder);
            IsGroupImagesByDirectory = Get<bool>(nameof(IsGroupImagesByDirectory), IsGroupImagesByDirectory);
            IsShowingHiddenImages = Get<bool>(nameof(IsShowingHiddenImages), IsShowingHiddenImages);
            IsShowColorPickerOnStartup = Get<bool>(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
            IsShowPageNavOnStartup = Get<bool>(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);
            IsShowPageNavAuto = Get<bool>(nameof(IsShowPageNavAuto), IsShowPageNavAuto);
            IsWindowFit = Get<bool>(nameof(IsWindowFit), IsWindowFit);
            IsCenterWindowFit = Get<bool>(nameof(IsCenterWindowFit), IsCenterWindowFit);
            IsShowToast = Get<bool>(nameof(IsShowToast), IsShowToast);
            IsUseTouchGesture = Get<bool>(nameof(IsUseTouchGesture), IsUseTouchGesture);
            IsHideTooltips = Get<bool>(nameof(IsHideTooltips), IsHideTooltips);
            IsExifToolAlwaysOnTop = Get<bool>(nameof(IsExifToolAlwaysOnTop), IsExifToolAlwaysOnTop);
            IsUseEmptyTitleBar = Get<bool>(nameof(IsUseEmptyTitleBar), IsUseEmptyTitleBar);

            #endregion

            #region Number items

            FirstLaunchVersion = Get<int>(nameof(FirstLaunchVersion), FirstLaunchVersion);

            #region Slide show
            SlideShowInterval = Get<uint>(nameof(SlideShowInterval), SlideShowInterval);
            if (SlideShowInterval < 1) SlideShowInterval = 5;

            SlideShowIntervalTo = Get<uint>(nameof(SlideShowIntervalTo), SlideShowIntervalTo);
            SlideShowIntervalTo = Math.Max(SlideShowIntervalTo, SlideShowInterval);
            #endregion

            #region Load thumbnail bar width & position
            ThumbnailDimension = Get<uint>(nameof(ThumbnailDimension), ThumbnailDimension);

            if (IsThumbnailHorizontal) {
                // Get minimum width needed for thumbnail dimension
                var tbMinWidth = new ThumbnailItemInfo(ThumbnailDimension, true).GetTotalDimension();

                // Get the greater width value
                ThumbnailBarWidth = Math.Max(ThumbnailBarWidth, tbMinWidth);
            }
            else {
                ThumbnailBarWidth = Get<uint>(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
            }
            #endregion

            ImageBoosterCachedCount = Get<uint>(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
            ImageBoosterCachedCount = Math.Max(0, Math.Min(ImageBoosterCachedCount, 10));

            ZoomLockValue = Get<double>(nameof(ZoomLockValue), ZoomLockValue);
            if (ZoomLockValue < 0) ZoomLockValue = 100f;

            ToolbarIconHeight = Get<uint>(nameof(ToolbarIconHeight), ToolbarIconHeight);
            ImageEditQuality = Get<int>(nameof(ImageEditQuality), ImageEditQuality);

            #endregion

            #region Enum items

            FrmMainWindowState = Get<FormWindowState>(nameof(FrmMainWindowState), FrmMainWindowState);
            FrmSettingsWindowState = Get<FormWindowState>(nameof(FrmSettingsWindowState), FrmSettingsWindowState);
            FrmExifToolWindowState = Get<FormWindowState>(nameof(FrmExifToolWindowState), FrmExifToolWindowState);
            ImageLoadingOrder = Get<ImageOrderBy>(nameof(ImageLoadingOrder), ImageLoadingOrder);
            ImageLoadingOrderType = Get<ImageOrderType>(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
            MouseWheelAction = Get<MouseWheelActions>(nameof(MouseWheelAction), MouseWheelAction);
            MouseWheelCtrlAction = Get<MouseWheelActions>(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction);
            MouseWheelShiftAction = Get<MouseWheelActions>(nameof(MouseWheelShiftAction), MouseWheelShiftAction);
            MouseWheelAltAction = Get<MouseWheelActions>(nameof(MouseWheelAltAction), MouseWheelAltAction);
            ZoomMode = Get<ZoomMode>(nameof(ZoomMode), ZoomMode);
            ZoomOptimizationMethod = Get<ZoomOptimizationMethods>(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod);
            ToolbarPosition = Get<ToolbarPosition>(nameof(ToolbarPosition), ToolbarPosition);
            AfterEditingAction = Get<AfterOpeningEditAppAction>(nameof(AfterEditingAction), AfterEditingAction);


            #endregion

            #region String items

            ColorProfile = Get<string>(nameof(ColorProfile), ColorProfile);
            ColorProfile = Heart.Helpers.GetCorrectColorProfileName(ColorProfile);

            AutoUpdate = Get<string>(nameof(AutoUpdate), AutoUpdate);
            LastSeenImagePath = Get<string>(nameof(LastSeenImagePath), LastSeenImagePath);
            ExifToolExePath = Get<string>(nameof(ExifToolExePath), ExifToolExePath);

            #endregion

            #region Array items

            #region ZoomLevels

            var zoomLevelStr = Get<string>(nameof(ZoomLevels), "");
            var zoomLevels = Helpers.StringToIntArray(zoomLevelStr, unsignedOnly: true, distinct: true);

            if (zoomLevels.Length > 0) ZoomLevels = zoomLevels;

            #endregion

            #region EditApps

            var appStr = Get<string>(nameof(EditApps), "");
            EditApps = GetEditApps(appStr);

            #endregion

            #region ImageFormats

            var formats = Get<string>(nameof(AllFormats), Constants.IMAGE_FORMATS);
            AllFormats = GetImageFormats(formats);

            #endregion

            #region KeyComboActions

            var keyActionStr = Get<string>(nameof(KeyComboActions), "");
            if (!string.IsNullOrEmpty(keyActionStr)) {
                KeyComboActions = GetKeyComboActions(keyActionStr);
            }

            #endregion

            #region ToolbarButtons

            var buttonStr = Get<string>(nameof(ToolbarButtons), "");
            var btnList = GetToolbarButtons(buttonStr);
            if (btnList.Count > 0) ToolbarButtons = btnList;

            #endregion

            #endregion

            #region Other types items

            #region FrmMainWindowBound
            var boundStr = Get<string>(nameof(FrmMainWindowBound), "");
            if (!string.IsNullOrEmpty(boundStr)) {
                var rc = Helpers.StringToRect(boundStr);
                if (!Helper.IsAnyPartOnScreen(rc)) {
                    rc = new Rectangle(280, 125, 1000, 800);
                }

                FrmMainWindowBound = rc;
            }
            #endregion

            #region FrmSettingsWindowBound
            boundStr = Get<string>(nameof(FrmSettingsWindowBound), "");
            if (!string.IsNullOrEmpty(boundStr)) {
                var rc = Helpers.StringToRect(boundStr);

                if (!Helper.IsOnScreen(rc.Location)) {
                    rc.Location = new Point(280, 125);
                }

                FrmSettingsWindowBound = rc;
            }
            #endregion

            #region FrmExifToolWindowBound
            boundStr = Get<string>(nameof(FrmExifToolWindowBound), "");
            if (!string.IsNullOrEmpty(boundStr)) {
                var rc = Helpers.StringToRect(boundStr);

                if (!Helper.IsOnScreen(rc.Location)) {
                    rc.Location = new Point(280, 125);
                }

                FrmExifToolWindowBound = rc;
            }
            #endregion

            #region Lang
            var langPath = Get<string>(nameof(Language), "English");
            Language = new Language(langPath, App.StartUpDir(Dir.Languages));
            #endregion

            #region Theme
            var themeFolderName = Get<string>(nameof(Theme), Constants.DEFAULT_THEME);
            var th = new Theme((int)ToolbarIconHeight, App.ConfigDir(PathType.Dir, Dir.Themes, themeFolderName));

            if (th.IsValid) {
                Theme = th;
            }
            #endregion

            #region BackgroundColor
            // must load after Theme
            var bgValue = Get<string>(nameof(BackgroundColor), Theme.ConvertColorToHEX(Theme.BackgroundColor, true));
            BackgroundColor = Theme.ConvertHexStringToColor(bgValue, true);
            #endregion

            #endregion

        }

        /// <summary>
        /// Parse and write configs to file
        /// </summary>
        public static void Write() {
            // save public properties to configs
            #region bool items

            Set(nameof(IsSlideshow), IsSlideshow);
            Set(nameof(IsShowSlideshowCountdown), IsShowSlideshowCountdown);
            Set(nameof(IsRandomSlideshowInterval), IsRandomSlideshowInterval);
            Set(nameof(IsFullScreen), IsFullScreen);
            Set(nameof(IsShowThumbnail), IsShowThumbnail);
            Set(nameof(IsCenterImage), IsCenterImage);
            Set(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
            Set(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
            Set(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
            Set(nameof(IsColorPickerHSVA), IsColorPickerHSVA);
            Set(nameof(IsShowWelcome), IsShowWelcome);
            Set(nameof(IsShowToolBar), IsShowToolBar);
            Set(nameof(IsShowThumbnailScrollbar), IsShowThumbnailScrollbar);
            Set(nameof(IsLoopBackSlideshow), IsLoopBackSlideshow);
            Set(nameof(IsLoopBackViewer), IsLoopBackViewer);
            Set(nameof(IsPressESCToQuit), IsPressESCToQuit);
            Set(nameof(IsShowCheckerBoard), IsShowCheckerBoard);
            Set(nameof(IsAllowMultiInstances), IsAllowMultiInstances);
            Set(nameof(IsWindowAlwaysOnTop), IsWindowAlwaysOnTop);
            Set(nameof(IsWindowFrameless), IsWindowFrameless);
            Set(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
            Set(nameof(IsConfirmationDelete), IsConfirmationDelete);
            Set(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
            Set(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
            Set(nameof(IsPreserveModifiedDate), IsPreserveModifiedDate);
            Set(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
            Set(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
            Set(nameof(IsCenterToolbar), IsCenterToolbar);
            Set(nameof(IsOpenLastSeenImage), IsOpenLastSeenImage);
            Set(nameof(IsApplyColorProfileForAll), IsApplyColorProfileForAll);
            Set(nameof(IsShowNavigationButtons), IsShowNavigationButtons);
            Set(nameof(IsShowCheckerboardOnlyImageRegion), IsShowCheckerboardOnlyImageRegion);
            Set(nameof(IsRecursiveLoading), IsRecursiveLoading);
            Set(nameof(IsUseFileExplorerSortOrder), IsUseFileExplorerSortOrder);
            Set(nameof(IsGroupImagesByDirectory), IsGroupImagesByDirectory);
            Set(nameof(IsShowingHiddenImages), IsShowingHiddenImages);
            Set(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
            Set(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);
            Set(nameof(IsShowPageNavAuto), IsShowPageNavAuto);
            Set(nameof(IsWindowFit), IsWindowFit);
            Set(nameof(IsCenterWindowFit), IsCenterWindowFit);
            Set(nameof(IsShowToast), IsShowToast);
            Set(nameof(IsUseTouchGesture), IsUseTouchGesture);
            Set(nameof(IsHideTooltips), IsHideTooltips);
            Set(nameof(IsExifToolAlwaysOnTop), IsExifToolAlwaysOnTop);
            Set(nameof(IsUseEmptyTitleBar), IsUseEmptyTitleBar);

            #endregion

            #region Number items

            Set(nameof(FirstLaunchVersion), FirstLaunchVersion);
            Set(nameof(SlideShowInterval), SlideShowInterval);
            Set(nameof(SlideShowIntervalTo), SlideShowIntervalTo);
            Set(nameof(ThumbnailDimension), ThumbnailDimension);
            Set(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
            Set(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
            Set(nameof(ZoomLockValue), ZoomLockValue);
            Set(nameof(ToolbarIconHeight), ToolbarIconHeight);
            Set(nameof(ImageEditQuality), ImageEditQuality);

            #endregion

            #region Enum items

            Set(nameof(FrmMainWindowState), FrmMainWindowState);
            Set(nameof(FrmSettingsWindowState), FrmSettingsWindowState);
            Set(nameof(FrmExifToolWindowState), FrmExifToolWindowState);
            Set(nameof(ImageLoadingOrder), ImageLoadingOrder);
            Set(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
            Set(nameof(MouseWheelAction), MouseWheelAction);
            Set(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction);
            Set(nameof(MouseWheelShiftAction), MouseWheelShiftAction);
            Set(nameof(MouseWheelAltAction), MouseWheelAltAction);
            Set(nameof(ZoomMode), ZoomMode);
            Set(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod);
            Set(nameof(ToolbarPosition), ToolbarPosition);
            Set(nameof(AfterEditingAction), AfterEditingAction);

            #endregion

            #region String items

            Set(nameof(ColorProfile), ColorProfile);
            Set(nameof(ToolbarButtons), ToolbarButtons);
            Set(nameof(AutoUpdate), AutoUpdate);
            Set(nameof(LastSeenImagePath), LastSeenImagePath);
            Set(nameof(ExifToolExePath), ExifToolExePath);

            #endregion

            #region Array items

            Set(nameof(ZoomLevels), Helpers.IntArrayToString(ZoomLevels));
            Set(nameof(EditApps), GetEditApps(EditApps));
            Set(nameof(AllFormats), GetImageFormats(AllFormats));
            Set(nameof(KeyComboActions), GetKeyComboActions(KeyComboActions));
            Set(nameof(ToolbarButtons), GetToolbarButtons(ToolbarButtons));

            #endregion

            #region Other types items

            Set(nameof(BackgroundColor), Theme.ConvertColorToHEX(BackgroundColor, true));
            Set(nameof(FrmMainWindowBound), Helpers.RectToString(FrmMainWindowBound));
            Set(nameof(FrmSettingsWindowBound), Helpers.RectToString(FrmSettingsWindowBound));
            Set(nameof(FrmExifToolWindowBound), Helpers.RectToString(FrmExifToolWindowBound));
            Set(nameof(Language), Path.GetFileName(Language.FileName));
            Set(nameof(Theme), Theme.FolderName);

            #endregion


            // write user configs to file
            Source.WriteUserConfigs();
        }


        #region Helper functions

        /// <summary>
        /// Convert the given value to specific type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public static T ConvertType<T>(object value) {
            var type = typeof(T);

            if (type.IsEnum) {
                return ParseEnum<T>(value);
            }
            else {
                return (T)Convert.ChangeType(value, type);
            }
        }

        /// <summary>
        /// Get the registered file extensions from registry
        /// Ex: *.svg;*.png;
        /// </summary>
        /// <returns></returns>
        public static string GetRegisteredExtensions() {
            var reg = new RegistryHelper() {
                BaseRegistryKey = Registry.LocalMachine,
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations"
            };

            var extList = reg.GetValueNames();
            var exts = new StringBuilder();

            foreach (var ext in extList) {
                exts.Append('*').Append(ext).Append(';');
            }

            return exts.ToString();
        }

        /// <summary>
        /// Randomize slideshow interval in seconds
        /// </summary>
        /// <returns></returns>
        public static uint RandomizeSlideshowInterval() {
            var intervalTo = (int)(IsRandomSlideshowInterval ? SlideShowIntervalTo : SlideShowInterval);

            var ran = new Random();
            var interval = (uint)ran.Next((int)SlideShowInterval, intervalTo);

            return interval;
        }

        #endregion


        #region Config functions

        #region EditApp

        /// <summary>
        /// Get ImageEditingAssociation from ImageEditingAssociationList
        /// </summary>
        /// <param name="ext">An extension to search. Ex: .png</param>
        /// <returns></returns>
        public static EditApp GetEditApp(string ext) {
            if (EditApps.Count > 0) {
                return EditApps.Find(v => v.Extension.CompareTo(ext) == 0);
            }

            return null;
        }

        /// <summary>
        /// Returns string from the given apps
        /// </summary>
        /// <param name="apps"></param>
        /// <returns></returns>
        public static List<EditApp> GetEditApps(string apps) {
            var appStr = apps.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var list = new List<EditApp>();

            if (appStr.Length > 0) {
                foreach (var item in appStr) {
                    try {
                        var extAssoc = new EditApp(item);
                        list.Add(extAssoc);
                    }
                    catch (InvalidCastException) { }
                }
            }

            return list;
        }

        /// <summary>
        /// Returns string from the given apps
        /// </summary>
        /// <param name="apps"></param>
        /// <returns></returns>
        public static string GetEditApps(List<EditApp> apps) {
            var appStr = new StringBuilder();
            foreach (var item in apps) {
                appStr.Append('[').Append(item).Append(']');
            }

            return appStr.ToString();
        }

        #endregion

        #region ImageFormats

        /// <summary>
        /// Returns distinc list of image formats
        /// </summary>
        /// <param name="formats">The format string. E.g: *.bpm;*.jpg;</param>
        /// <returns></returns>
        public static HashSet<string> GetImageFormats(string formats) {
            var list = new HashSet<string>();
            var formatList = formats.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            char[] wildTrim = { '*' };

            foreach (var ext in formatList) {
                list.Add(ext.Trim(wildTrim));
            }

            return list;
        }

        /// <summary>
        /// Returns the image formats string
        /// </summary>
        /// <param name="list">The input HashSet</param>
        /// <returns></returns>
        public static string GetImageFormats(HashSet<string> list) {
            var sb = new StringBuilder(list.Count);
            foreach (var item in list) {
                sb.Append('*').Append(item).Append(';');
            }

            return sb.ToString();
        }

        #endregion

        #region KeyComboActions

        /// <summary>
        /// Returns the keycombo actions from string
        /// </summary>
        /// <param name="keyActions">The input string. E.g. "combo1:action1;combo2:action2"</param>
        /// <returns></returns>
        public static Dictionary<KeyCombos, AssignableActions> GetKeyComboActions(string keyActions) {
            var pairs = keyActions.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var dic = new Dictionary<KeyCombos, AssignableActions>();

            try {
                foreach (var pair in pairs) {
                    var parts = pair.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    var keyCombo = ParseEnum<KeyCombos>(parts[0]);
                    var action = ParseEnum<AssignableActions>(parts[1]);

                    dic.Add(keyCombo, action);
                }
            }
            catch {
                // reset to default set on error
                dic = Constants.DefaultKeycomboActions;
            }

            return dic;
        }

        /// <summary>
        /// Returns the string from keycombo actions
        /// </summary>
        /// <param name="keyActions">The input keycombo actions</param>
        /// <returns></returns>
        public static string GetKeyComboActions(Dictionary<KeyCombos, AssignableActions> keyActions) {
            var sb = new StringBuilder();

            foreach (var key in keyActions.Keys) {
                sb.Append(key.ToString());
                sb.Append(':');
                sb.Append(keyActions[key].ToString());
                sb.Append(';');
            }

            return sb.ToString();
        }

        #endregion

        #region ToolbarButtons

        /// <summary>
        /// Returns list of toolbar buttons from string
        /// </summary>
        /// <param name="buttons">The input string</param>
        /// <returns></returns>
        public static List<ToolbarButton> GetToolbarButtons(string buttons) {
            var list = new List<ToolbarButton>();
            var splitvals = buttons.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in splitvals) {
                try {
                    var btn = ParseEnum<ToolbarButton>(item);
                    list.Add(btn);
                }
                // ignore invalid values
                catch { }
            }

            return list;
        }

        /// <summary>
        /// Returns string from toolbar buttons list
        /// </summary>
        /// <param name="list">The input toolbar buttons</param>
        /// <returns></returns>
        public static string GetToolbarButtons(List<ToolbarButton> list) {
            var sb = new StringBuilder(list.Count);
            foreach (var item in list) {
                sb.Append(item).Append(';');
            }

            return sb.ToString();
        }

        #endregion


        /// <summary>
        /// Apply theme colors and logo to form
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="th"></param>
        public static void ApplyFormTheme(Form frm, Theme th) {
            // load theme colors
            foreach (var ctr in Helpers.GetAllControls(frm, typeof(LinkLabel))) {
                if (ctr is LinkLabel lnk) {
                    lnk.LinkColor = lnk.VisitedLinkColor = th.AccentColor;
                }
            }

            // Icon theming
            if (!th.IsShowTitlebarLogo) {
                frm.Icon = Icon.FromHandle(new Bitmap(64, 64).GetHicon());
                FormIcon.SetTaskbarIcon(frm, th.Logo.Image.GetHicon());
            }
            else {
                frm.Icon = Icon.FromHandle(th.Logo.Image.GetHicon());
            }
        }

        #endregion


        #endregion

    }
}
