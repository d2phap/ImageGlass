
using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.Settings
{
    public static class Configs
    {
        /// <summary>
        /// Configuration Source file
        /// </summary>
        private static ConfigSource Source { get; set; } = new ConfigSource();


        #region Public configs

        #region Boolean items
        /// <summary>
        /// Gets, sets value of slideshow state
        /// </summary>
        public static bool IsPlaySlideShow { get; set; } = false;


        /// <summary>
        /// Gets, sets value indicating that wheather the window is full screen or not
        /// </summary>
        public static bool IsFullScreen { get; set; } = false;


        /// <summary>
        /// Gets, sets value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail { get; set; } = false;


        /// <summary>
        /// Gets, sets the value that indicates if the default position of image in the viewer is center or top left
        /// </summary>
        public static bool IsCenterImage { get; set; } = false;


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
        public static bool IsLoopBackSlideShow { get; set; } = false;


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
        /// Is the thumbnail bar to be shown horizontal (down at the bottom) or vertical (on right side)?
        /// </summary>
        public static bool IsThumbnailHorizontal { get; set; } = false;


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
        /// Setting to control whether the image's original modified date value is preserved on save
        /// </summary>
        public static bool PreserveModifiedDate { get; set; } = true;


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
        /// Gets, sets the value indicates that to show or hide the Navigation Buttons on viewer
        /// </summary>
        public static bool IsShowNavigationButtons { get; set; } = false;


        /// <summary>
        /// Gets, sets the value indicates that to checkerboard in the image region only
        /// </summary>
        public static bool IsShowCheckerboardOnlyImageRegion { get; set; } = false;


        /// <summary>
        /// Gets, sets recursive value
        /// </summary>
        public static bool IsRecursiveLoading { get; set; } = false;


        /// <summary>
        /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
        /// </summary>
        public static bool IsUseFileExplorerSortOrder { get; set; } = false;


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

        #endregion


        #region Number items

        /// <summary>
        /// Gets, sets the version that requires to launch First-Launch Configs screen
        /// </summary>
        public static int FirstLaunchVersion { get; set; } = 0;


        /// <summary>
        /// Gets, sets slide show interval
        /// </summary>
        public static uint SlideShowInterval { get; set; } = 5;


        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static uint ThumbnailDimension { get; set; } = 64;


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


        #endregion


        #region String items

        /// <summary>
        /// Gets all supported extensions string
        /// </summary>
        public static string AllImageFormats { get => DefaultImageFormats + OptionalImageFormats; }


        /// <summary>
        /// Gets, sets default image formats
        /// </summary>
        public static string DefaultImageFormats { get; set; } = string.Empty;


        /// <summary>
        /// Gets, sets optional image formats
        /// </summary>
        public static string OptionalImageFormats { get; set; } = string.Empty;


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
        /// The toolbar button configuration: contents and order.
        /// </summary>
        public static string ToolbarButtons { get; set; } = $"" +
                $"{(int)Base.ToolbarButtons.btnBack}," +
                $"{(int)Base.ToolbarButtons.btnNext}," +
                $"{(int)Base.ToolbarButtons.Separator}," +

                $"{(int)Base.ToolbarButtons.btnRotateLeft}," +
                $"{(int)Base.ToolbarButtons.btnRotateRight}," +
                $"{(int)Base.ToolbarButtons.btnFlipHorz}," +
                $"{(int)Base.ToolbarButtons.btnFlipVert}," +
                $"{(int)Base.ToolbarButtons.Separator}," +

                $"{(int)Base.ToolbarButtons.btnAutoZoom}," +
                $"{(int)Base.ToolbarButtons.btnScaletoWidth}," +
                $"{(int)Base.ToolbarButtons.btnScaletoHeight}," +
                $"{(int)Base.ToolbarButtons.btnScaleToFit}," +
                $"{(int)Base.ToolbarButtons.btnScaleToFill}," +
                $"{(int)Base.ToolbarButtons.btnZoomLock}," +
                $"{(int)Base.ToolbarButtons.Separator}," +

                $"{(int)Base.ToolbarButtons.btnOpen}," +
                $"{(int)Base.ToolbarButtons.btnRefresh}," +
                $"{(int)Base.ToolbarButtons.btnGoto}," +
                $"{(int)Base.ToolbarButtons.Separator}," +

                $"{(int)Base.ToolbarButtons.btnThumb}," +
                $"{(int)Base.ToolbarButtons.btnCheckedBackground}," +
                $"{(int)Base.ToolbarButtons.btnFullScreen}," +
                $"{(int)Base.ToolbarButtons.btnSlideShow}," +
                $"{(int)Base.ToolbarButtons.btnDelete}," +
                $"{(int)Base.ToolbarButtons.btnEdit}";



        /// <summary>
        /// User-selected action tied to key pairings.
        /// E.g. Left/Right arrows: prev/next image
        /// </summary>
        public static string KeyboardActions { get; set; } = $"" +
            $"{(int)KeyCombos.LeftRight},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.UpDown},{(int)AssignableActions.PanUpDown};" +
            $"{(int)KeyCombos.PageUpDown},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.SpaceBack},{(int)AssignableActions.PauseSlideshow};";

        #endregion


        #region Array items

        /// <summary>
        /// Gets, sets zoom levels of the viewer
        /// </summary>
        public static int[] ZoomLevels { get; set; } = new int[] { 5, 10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100, 125, 150, 175, 200, 250, 300, 350, 400, 500, 600, 700, 800, 1000, 1200, 1500, 1800, 2100, 2500, 3000, 3500 };


        /// <summary>
        /// Gets, sets the list of Image Editing Association
        /// </summary>
        public static List<ImageEditingAssociation> ImageEditingAssociationList { get; set; } = new List<ImageEditingAssociation>();


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
        /// Gets, sets image loading order
        /// </summary>
        public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Extension;// ImageOrderBy.Name;


        /// <summary>
        /// Gets, sets image loading order type
        /// </summary>
        public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;


        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel
        /// </summary>
        public static MouseWheelActions MouseWheelAction { get; set; } = MouseWheelActions.ScrollVertically;


        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Ctrl key
        /// </summary>
        public static MouseWheelActions MouseWheelCtrlAction { get; set; } = MouseWheelActions.Zoom;


        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Shift key
        /// </summary>
        public static MouseWheelActions MouseWheelShiftAction { get; set; } = MouseWheelActions.ScrollHorizontally;


        /// <summary>
        /// Gets, sets action to be performed when user spins the mouse wheel while holding Alt key
        /// </summary>
        public static MouseWheelActions MouseWheelAltAction { get; set; } = MouseWheelActions.DoNothing;


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



        #endregion


        #region Other types items

        /// <summary>
        /// Gets, sets background color
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.Black;


        /// <summary>
        /// Gets, sets window bound of main form
        /// </summary>
        public static Rectangle FrmMainWindowsBound { get; set; } = new Rectangle(280, 125, 1000, 800);


        /// <summary>
        /// Gets, sets window bound of main form
        /// </summary>
        public static Rectangle FrmSettingsWindowsBound { get; set; } = new Rectangle(280, 125, 900, 700);



        /// <summary>
        /// Gets, sets language pack
        /// </summary>
        public static Language Language { get; set; } = new Language();


        /// <summary>
        /// Gets, sets theme
        /// </summary>
        public static Theme Theme { get; set; } = new Theme();


        #endregion

        #endregion



        #region Private methods
        /// <summary>
        /// Convert the given value to specific type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        /// <returns></returns>
        private static T ConvertType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }


        /// <summary>
        /// Gets config item from ConfigSource
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key of the config</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        private static T Get<T>(string key, object defaultValue)
        {
            try
            {
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
        private static void Set(string key, object value)
        {
            if (Source.ContainsKey(key))
            {
                Source[key] = value.ToString();
            }
            else
            {
                Source.Add(key, value.ToString());
            }
        }


        #endregion



        #region Public methods

        /// <summary>
        /// Load and parse configs from file
        /// </summary>
        public static void Load()
        {
            // load user configs from file
            Source.LoadUserConfigs();


            // load configs to public properties
            #region Boolean items

            IsPlaySlideShow = Get<bool>(nameof(IsPlaySlideShow), IsPlaySlideShow);
            IsFullScreen = Get<bool>(nameof(IsFullScreen), IsFullScreen);
            IsShowThumbnail = Get<bool>(nameof(IsShowThumbnail), IsShowThumbnail);
            IsCenterImage = Get<bool>(nameof(IsCenterImage), IsCenterImage);
            IsColorPickerRGBA = Get<bool>(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
            IsColorPickerHEXA = Get<bool>(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
            IsColorPickerHSLA = Get<bool>(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
            IsShowWelcome = Get<bool>(nameof(IsShowWelcome), IsShowWelcome);
            IsShowToolBar = Get<bool>(nameof(IsShowToolBar), IsShowToolBar);
            IsShowThumbnailScrollbar = Get<bool>(nameof(IsShowThumbnailScrollbar), IsShowThumbnailScrollbar);
            IsLoopBackSlideShow = Get<bool>(nameof(IsLoopBackSlideShow), IsLoopBackSlideShow);
            IsLoopBackViewer = Get<bool>(nameof(IsLoopBackViewer), IsLoopBackViewer);
            IsPressESCToQuit = Get<bool>(nameof(IsPressESCToQuit), IsPressESCToQuit);
            IsShowCheckerBoard = Get<bool>(nameof(IsShowCheckerBoard), IsShowCheckerBoard);
            IsAllowMultiInstances = Get<bool>(nameof(IsAllowMultiInstances), IsAllowMultiInstances);
            IsWindowAlwaysOnTop = Get<bool>(nameof(IsWindowAlwaysOnTop), IsWindowAlwaysOnTop);
            IsThumbnailHorizontal = Get<bool>(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
            IsConfirmationDelete = Get<bool>(nameof(IsConfirmationDelete), IsConfirmationDelete);
            IsScrollbarsVisible = Get<bool>(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
            IsSaveAfterRotating = Get<bool>(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
            PreserveModifiedDate = Get<bool>(nameof(PreserveModifiedDate), PreserveModifiedDate);
            IsNewVersionAvailable = Get<bool>(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
            IsDisplayBasenameOfImage = Get<bool>(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
            IsCenterToolbar = Get<bool>(nameof(IsCenterToolbar), IsCenterToolbar);
            IsOpenLastSeenImage = Get<bool>(nameof(IsOpenLastSeenImage), IsOpenLastSeenImage);
            IsApplyColorProfileForAll = Get<bool>(nameof(IsApplyColorProfileForAll), IsApplyColorProfileForAll);
            IsShowNavigationButtons = Get<bool>(nameof(IsShowNavigationButtons), IsShowNavigationButtons);
            IsShowCheckerboardOnlyImageRegion = Get<bool>(nameof(IsShowCheckerboardOnlyImageRegion), IsShowCheckerboardOnlyImageRegion);
            IsRecursiveLoading = Get<bool>(nameof(IsRecursiveLoading), IsRecursiveLoading);
            IsUseFileExplorerSortOrder = Get<bool>(nameof(IsUseFileExplorerSortOrder), IsUseFileExplorerSortOrder);
            IsShowingHiddenImages = Get<bool>(nameof(IsShowingHiddenImages), IsShowingHiddenImages);
            IsShowColorPickerOnStartup = Get<bool>(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
            IsShowPageNavOnStartup = Get<bool>(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);

            #endregion


            #region Number items

            FirstLaunchVersion = Get<int>(nameof(FirstLaunchVersion), FirstLaunchVersion);
            SlideShowInterval = Get<uint>(nameof(SlideShowInterval), SlideShowInterval);
            if (SlideShowInterval < 1) SlideShowInterval = 5;

            #region Load thumbnail bar width & position
            ThumbnailDimension = Get<uint>(nameof(ThumbnailDimension), ThumbnailDimension);
           
            if (IsThumbnailHorizontal)
            {
                // Get minimum width needed for thumbnail dimension
                var tbMinWidth = new ThumbnailItemInfo(ThumbnailDimension, true).GetTotalDimension();

                // Get the greater width value
                ThumbnailBarWidth = Math.Max(ThumbnailBarWidth, tbMinWidth);
            }
            else
            {
                ThumbnailBarWidth = Get<uint>(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
            }
            #endregion

            ImageBoosterCachedCount = Get<uint>(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
            ImageBoosterCachedCount = Math.Max(0, Math.Min(ImageBoosterCachedCount, 10));

            ZoomLockValue = Get<double>(nameof(ZoomLockValue), ZoomLockValue);
            if (ZoomLockValue < 0) ZoomLockValue = 100f;

            #endregion


            #region Enum items

            FrmMainWindowState = (FormWindowState)Get<int>(nameof(FrmMainWindowState), FrmMainWindowState);
            FrmSettingsWindowState = (FormWindowState)Get<int>(nameof(FrmSettingsWindowState), FrmSettingsWindowState);
            ImageLoadingOrder = (ImageOrderBy)Get<int>(nameof(ImageLoadingOrder), ImageLoadingOrder);
            ImageLoadingOrderType = (ImageOrderType)Get<int>(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
            MouseWheelAction = (MouseWheelActions)Get<int>(nameof(MouseWheelAction), MouseWheelAction);
            MouseWheelCtrlAction = (MouseWheelActions)Get<int>(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction);
            MouseWheelShiftAction = (MouseWheelActions)Get<int>(nameof(MouseWheelShiftAction), MouseWheelShiftAction);
            MouseWheelAltAction = (MouseWheelActions)Get<int>(nameof(MouseWheelAltAction), MouseWheelAltAction);
            ZoomMode = (ZoomMode)Get<int>(nameof(ZoomMode), ZoomMode);
            ZoomOptimizationMethod = (ZoomOptimizationMethods)Get<int>(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod);
            ToolbarPosition = (ToolbarPosition)Get<int>(nameof(ToolbarPosition), ToolbarPosition);

            #endregion


            #region String items

            // set default formats list from constant
            var exts = Constants.BuiltInImageFormats.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            DefaultImageFormats = exts[0];
            OptionalImageFormats = exts[1];

            // TODO: merge 2 list
            DefaultImageFormats = Get<string>(nameof(DefaultImageFormats), DefaultImageFormats);
            OptionalImageFormats = Get<string>(nameof(OptionalImageFormats), OptionalImageFormats);


            ColorProfile = Get<string>(nameof(ColorProfile), ColorProfile);
            ColorProfile = Heart.Helpers.GetCorrectColorProfileName(ColorProfile);

            ToolbarButtons = Get<string>(nameof(ToolbarButtons), ToolbarButtons);
            KeyboardActions = Get<string>(nameof(KeyboardActions), KeyboardActions);

            AutoUpdate = Get<string>(nameof(AutoUpdate), AutoUpdate);

            #endregion


            #region Array items

            #region ZoomLevels

            var zoomLevelStr = Get<string>(nameof(ZoomLevels), "");
            var zoomLevels = Helpers.StringToIntArray(zoomLevelStr, unsignedOnly: true, distinct: true);

            if (zoomLevels.Length > 0)
            {
                ZoomLevels = zoomLevels;
            }

            #endregion


            #region ImageEditingAssociationList

            var editAssocStr = Get<string>(nameof(ImageEditingAssociationList), "");
            var editAssocList = editAssocStr.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (editAssocList.Length > 0)
            {
                foreach (var configStr in editAssocList)
                {
                    try
                    {
                        var extAssoc = new ImageEditingAssociation(configStr);
                        ImageEditingAssociationList.Add(extAssoc);
                    }
                    catch (InvalidCastException) { }
                }
            }
            #endregion

            #endregion


            #region Other types items

            #region FrmMainWindowsBound
            var boundStr = Get<string>(nameof(FrmMainWindowsBound), "");
            if (!string.IsNullOrEmpty(boundStr))
            {
                var rc = Helpers.StringToRect(boundStr);

                if (!Helper.IsOnScreen(rc.Location))
                {
                    rc.Location = new Point(280, 125);
                }

                FrmMainWindowsBound = rc;
            }
            #endregion


            #region FrmSettingsWindowsBound
            boundStr = Get<string>(nameof(FrmSettingsWindowsBound), "");
            if (!string.IsNullOrEmpty(boundStr))
            {
                var rc = Helpers.StringToRect(boundStr);

                if (!Helper.IsOnScreen(rc.Location))
                {
                    rc.Location = new Point(280, 125);
                }

                FrmSettingsWindowsBound = rc;
            }
            #endregion


            #region Lang
            var langPath = Get<string>(nameof(Language), "English");
            Language = new Language(langPath, App.StartUpDir(Dir.Languages));
            #endregion


            #region Theme
            var themeFolderName = Get<string>(nameof(Theme), "default");
            var th = new Theme(App.ConfigDir(Dir.Themes, themeFolderName));

            if (th.IsValid)
            {
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
        public static void Write()
        {
            // save public properties to configs
            #region Boolean items

            Set(nameof(IsPlaySlideShow), IsPlaySlideShow);
            Set(nameof(IsFullScreen), IsFullScreen);
            Set(nameof(IsShowThumbnail), IsShowThumbnail);
            Set(nameof(IsCenterImage), IsCenterImage);
            Set(nameof(IsColorPickerRGBA), IsColorPickerRGBA);
            Set(nameof(IsColorPickerHEXA), IsColorPickerHEXA);
            Set(nameof(IsColorPickerHSLA), IsColorPickerHSLA);
            Set(nameof(IsShowWelcome), IsShowWelcome);
            Set(nameof(IsShowToolBar), IsShowToolBar);
            Set(nameof(IsShowThumbnailScrollbar), IsShowThumbnailScrollbar);
            Set(nameof(IsLoopBackSlideShow), IsLoopBackSlideShow);
            Set(nameof(IsLoopBackViewer), IsLoopBackViewer);
            Set(nameof(IsPressESCToQuit), IsPressESCToQuit);
            Set(nameof(IsShowCheckerBoard), IsShowCheckerBoard);
            Set(nameof(IsAllowMultiInstances), IsAllowMultiInstances);
            Set(nameof(IsWindowAlwaysOnTop), IsWindowAlwaysOnTop);
            Set(nameof(IsThumbnailHorizontal), IsThumbnailHorizontal);
            Set(nameof(IsConfirmationDelete), IsConfirmationDelete);
            Set(nameof(IsScrollbarsVisible), IsScrollbarsVisible);
            Set(nameof(IsSaveAfterRotating), IsSaveAfterRotating);
            Set(nameof(PreserveModifiedDate), PreserveModifiedDate);
            Set(nameof(IsNewVersionAvailable), IsNewVersionAvailable);
            Set(nameof(IsDisplayBasenameOfImage), IsDisplayBasenameOfImage);
            Set(nameof(IsCenterToolbar), IsCenterToolbar);
            Set(nameof(IsOpenLastSeenImage), IsOpenLastSeenImage);
            Set(nameof(IsApplyColorProfileForAll), IsApplyColorProfileForAll);
            Set(nameof(IsShowNavigationButtons), IsShowNavigationButtons);
            Set(nameof(IsShowCheckerboardOnlyImageRegion), IsShowCheckerboardOnlyImageRegion);
            Set(nameof(IsRecursiveLoading), IsRecursiveLoading);
            Set(nameof(IsUseFileExplorerSortOrder), IsUseFileExplorerSortOrder);
            Set(nameof(IsShowingHiddenImages), IsShowingHiddenImages);
            Set(nameof(IsShowColorPickerOnStartup), IsShowColorPickerOnStartup);
            Set(nameof(IsShowPageNavOnStartup), IsShowPageNavOnStartup);

            #endregion


            #region Number items

            Set(nameof(FirstLaunchVersion), FirstLaunchVersion);
            Set(nameof(SlideShowInterval), SlideShowInterval);
            Set(nameof(ThumbnailDimension), ThumbnailDimension);
            Set(nameof(ThumbnailBarWidth), ThumbnailBarWidth);
            Set(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
            Set(nameof(ZoomLockValue), ZoomLockValue);

            #endregion


            #region Enum items

            Set(nameof(FrmMainWindowState), FrmMainWindowState);
            Set(nameof(FrmSettingsWindowState), FrmSettingsWindowState);
            Set(nameof(ImageLoadingOrder), ImageLoadingOrder);
            Set(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
            Set(nameof(MouseWheelAction), MouseWheelAction);
            Set(nameof(MouseWheelCtrlAction), MouseWheelCtrlAction);
            Set(nameof(MouseWheelShiftAction), MouseWheelShiftAction);
            Set(nameof(MouseWheelAltAction), MouseWheelAltAction);
            Set(nameof(ZoomMode), ZoomMode);
            Set(nameof(ZoomOptimizationMethod), ZoomOptimizationMethod);
            Set(nameof(ToolbarPosition), ToolbarPosition);

            #endregion


            #region String items

            // TODO: merge 2 list
            Set(nameof(DefaultImageFormats), DefaultImageFormats);
            Set(nameof(OptionalImageFormats), OptionalImageFormats);

            Set(nameof(ColorProfile), ColorProfile);
            Set(nameof(ToolbarButtons), ToolbarButtons);
            Set(nameof(KeyboardActions), KeyboardActions);
            Set(nameof(AutoUpdate), AutoUpdate);

            #endregion


            #region Array items

            Set(nameof(ZoomLevels), Helpers.IntArrayToString(ZoomLevels));

            #region ImageEditingAssociationList

            var editingAssocString = new StringBuilder();

            foreach (var item in ImageEditingAssociationList)
            {
                editingAssocString.Append($"[{item.ToString()}]");
            }

            Set(nameof(ImageEditingAssociationList), editingAssocString);

            #endregion

            #endregion


            #region Other types items

            Set(nameof(BackgroundColor), Theme.ConvertColorToHEX(BackgroundColor, true));
            Set(nameof(FrmMainWindowsBound), Helpers.RectToString(FrmMainWindowsBound));
            Set(nameof(FrmSettingsWindowsBound), Helpers.RectToString(FrmSettingsWindowsBound));
            Set(nameof(Language), Path.GetFileName(Language.FileName));
            Set(nameof(Theme), Theme.FolderName);

            #endregion


            // write user configs to file
            Source.WriteUserConfigs();
        }

        #endregion




        /// <summary>
        /// Get ImageEditingAssociation from ImageEditingAssociationList
        /// </summary>
        /// <param name="ext">Extension to search. Ex: .png</param>
        /// <returns></returns>
        public static ImageEditingAssociation GetImageEditingAssociationFromList(string ext)
        {
            if (ImageEditingAssociationList.Count > 0)
            {
                try
                {
                    var assoc = ImageEditingAssociationList.FirstOrDefault(v => v.Extension.CompareTo(ext) == 0);

                    return assoc;
                }
                catch { }
            }

            return null;
        }

    }
}
