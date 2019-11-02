
using ImageGlass.Library;
using ImageGlass.Services.Configuration;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// Gets, sets slide show interval
        /// </summary>
        public static int SlideShowInterval { get; set; } = 5;


        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension { get; set; } = 96;


        /// <summary>
        /// Gets, sets width of horizontal thumbnail bar
        /// </summary>
        public static int ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(48, true).GetTotalDimension();


        /// <summary>
        /// Gets, sets the number of images cached by Image
        /// </summary>
        public static int ImageBoosterCachedCount { get; set; } = 1;


        /// <summary>
        /// Gets, sets fixed width on zooming
        /// </summary>
        public static double ZoomLockValue { get; set; } = 100.0;


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
        /// The toolbar button configuration: contents and order.
        /// </summary>
        public static string ToolbarButtons { get; set; } = $"" +
                $"{(int)Services.Configuration.ToolbarButtons.btnBack}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnNext}," +
                $"{(int)Services.Configuration.ToolbarButtons.Separator}," +

                $"{(int)Services.Configuration.ToolbarButtons.btnRotateLeft}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnRotateRight}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnFlipHorz}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnFlipVert}," +
                $"{(int)Services.Configuration.ToolbarButtons.Separator}," +

                $"{(int)Services.Configuration.ToolbarButtons.btnAutoZoom}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnScaletoWidth}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnScaletoHeight}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnScaleToFit}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnScaleToFill}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnZoomLock}," +
                $"{(int)Services.Configuration.ToolbarButtons.Separator}," +

                $"{(int)Services.Configuration.ToolbarButtons.btnOpen}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnRefresh}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnGoto}," +
                $"{(int)Services.Configuration.ToolbarButtons.Separator}," +

                $"{(int)Services.Configuration.ToolbarButtons.btnThumb}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnCheckedBackground}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnFullScreen}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnSlideShow}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnDelete}," +
                $"{(int)Services.Configuration.ToolbarButtons.btnEdit}";



        /// <summary>
        /// User-selected action tied to key pairings.
        /// E.g. Left/Right arrows: prev/next image
        /// </summary>
        public static string KeyAssignments { get; set; } = $"" +
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
        public static Language Lang { get; set; } = new Language();


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

            SlideShowInterval = Get<int>(nameof(SlideShowInterval), SlideShowInterval);
            ThumbnailDimension = Get<int>(nameof(ThumbnailDimension), ThumbnailDimension);
            ThumbnailBarWidth = Get<int>(nameof(ThumbnailBarWidth), ThumbnailBarWidth);

            ImageBoosterCachedCount = Get<int>(nameof(ImageBoosterCachedCount), ImageBoosterCachedCount);
            ImageBoosterCachedCount = Math.Max(0, Math.Min(ImageBoosterCachedCount, 10));

            ZoomLockValue = Get<double>(nameof(ZoomLockValue), ZoomLockValue);

            #endregion


            #region String items
            // TODO: merge 2 list
            DefaultImageFormats = Get<string>(nameof(DefaultImageFormats), DefaultImageFormats);
            OptionalImageFormats = Get<string>(nameof(OptionalImageFormats), OptionalImageFormats);

            ColorProfile = Get<string>(nameof(ColorProfile), ColorProfile);
            ColorProfile = Heart.Helpers.GetCorrectColorProfileName(ColorProfile);

            ToolbarButtons = Get<string>(nameof(ToolbarButtons), ToolbarButtons);
            KeyAssignments = Get<string>(nameof(KeyAssignments), KeyAssignments);

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


            #region Other types items

            #region BackgroundColor
            var bgValue = Get<string>(nameof(BackgroundColor), "#000");
            BackgroundColor = Theme.ConvertHexStringToColor(bgValue, true);
            #endregion


            #region FrmMainWindowsBound
            var boundStr = Get<string>(nameof(FrmMainWindowsBound), "");
            if (string.IsNullOrEmpty(boundStr))
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
            if (string.IsNullOrEmpty(boundStr))
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
            var langPath = Get<string>(nameof(Lang), "English");
            Lang = new Language(langPath, App.StartUpDir(Dir.Languages));
            #endregion


            #region Theme
            var themeFolderName = Get<string>(nameof(Theme), "default");
            var th = new Theme(App.ConfigDir(Dir.Themes, themeFolderName));

            if (th.IsValid)
            {
                Theme = th;
            }
            #endregion

            #endregion

        }


        /// <summary>
        /// Parse and write configs to file
        /// </summary>
        public static void Write()
        {
            // save public properties to configs


            // write user configs to file
            Source.WriteUserConfigs();
        }

        #endregion


    }
}
