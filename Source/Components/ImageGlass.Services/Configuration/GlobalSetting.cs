/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using Microsoft.Win32;
using System.Drawing;
using System.Text;
using ImageGlass.Library.FileAssociations;
using System.Linq;
using System.Globalization;
using ImageGlass.Heart;
using ImageGlass.Base;

namespace ImageGlass.Services.Configuration
{
    public static class GlobalSetting
    {
        // Configurations file: igconfig.xml
        private static ConfigurationFile _configFile = new ConfigurationFile();



        #region Private Properties

        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static Factory ImageList { get; set; } = new Factory();


        /// <summary>
        /// Gets, sets current index of image
        /// </summary>
        public static int CurrentIndex { get; set; } = -1;



        /// <summary>
        /// ~Gets, sets image loading order
        /// </summary>
        public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;


        /// <summary>
        /// ~Gets, sets image loading order type
        /// </summary>
        public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;



        /// <summary>
        /// Gets or sets the hash array of all supported formats. 
        /// **NOTE: this needs to be manually updated by calling GlobalSetting.MakeImageTypeSet()
        /// </summary>
        public static HashSet<string> ImageFormatHashSet { get; set; } = new HashSet<string>();



        #endregion



        #region Public Properties


        /// <summary>
        /// ~Gets all supported extensions string
        /// </summary>
        public static string AllImageFormats
        {
            get
            {
                return GlobalSetting.DefaultImageFormats + GlobalSetting.OptionalImageFormats;
            }
        }


        /// <summary>
        /// ~Gets, sets default image formats
        /// </summary>
        public static string DefaultImageFormats { get; set; } = string.Empty;


        /// <summary>
        /// ~Gets, sets optional image formats
        /// </summary>
        public static string OptionalImageFormats { get; set; } = string.Empty;


        /// <summary>
        /// ~Gets, sets value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail { get; set; } = false;


        /// <summary>
        /// Gets, sets image error value
        /// </summary>
        public static Exception ImageError { get; set; } = null;


        /// <summary>
        /// ~Gets, sets fixed width on zooming
        /// </summary>
        public static double ZoomLockValue { get; set; } = 100.0;


        /// <summary>
        /// ~Gets, sets zoom levels of the viewer
        /// </summary>
        public static int[] ZoomLevels { get; set; } = new int[] { 5, 10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100, 125, 150, 175, 200, 250, 300, 350, 400, 500, 600, 700, 800, 1000, 1200, 1500, 1800, 2100, 2500, 3000, 3500 };



        /// <summary>
        /// ~Check if user wants to display RGBA color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerRGBA { get; set; } = true;


        /// <summary>
        /// ~Check if user wants to display HEX with Alpha color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerHEXA { get; set; } = true;


        /// <summary>
        /// ~Check if user wants to display HSL with Alpha color code for Color Picker tool
        /// </summary>
        public static bool IsColorPickerHSLA { get; set; } = true;


        /// <summary>
        /// ~Gets, sets welcome picture value
        /// </summary>
        public static bool IsShowWelcome { get; set; } = true;


        /// <summary>
        /// ~Gets, sets background color
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.White;


        /// <summary>
        /// ~Gets, sets value of visibility of toolbar when start up
        /// </summary>
        public static bool IsShowToolBar { get; set; } = true;


        /// <summary>
        /// ~Gets, sets value whether thumbnail scrollbars visible
        /// </summary>
        public static bool IsShowThumbnailScrollbar { get; set; } = false;


        /// <summary>
        /// ~Gets, sets value that allows user to loop back to the first image when reaching the end of list
        /// </summary>
        public static bool IsLoopBackSlideShow { get; set; } = false;


        /// <summary>
        /// ~Gets, sets slide show interval
        /// </summary>
        public static int SlideShowInterval { get; set; } = 5;


        /// <summary>
        /// ~Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
        /// </summary>
        public static bool IsLoopBackViewer { get; set; } = true;


        /// <summary>
        /// ~Gets, sets value indicating that allow quit application by ESC
        /// </summary>
        public static bool IsPressESCToQuit { get; set; } = true;


        /// <summary>
        /// ~Gets, sets value indicating that checker board is shown or not
        /// </summary>
        public static bool IsShowCheckerBoard { get; set; } = false;


        /// <summary>
        /// ~Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension { get; set; } = 96;


        /// <summary>
        /// ~Gets, sets value indicating that multi instances is allowed or not
        /// </summary>
        public static bool IsAllowMultiInstances { get; set; } = true;


        /// <summary>
        /// ~Gets, sets value indicating that frmMain is always on top or not.
        /// </summary>
        public static bool IsWindowAlwaysOnTop { get; set; } = false;


        /// <summary>
        /// ~Is the thumbnail bar to be shown horizontal (down at the bottom) or vertical (on right side)?
        /// </summary>
        public static bool IsThumbnailHorizontal { get; set; } = false;


        /// <summary>
        /// ~Gets, sets width of horizontal thumbnail bar
        /// </summary>
        public static int ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(48, true).GetTotalDimension();


        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel
        /// </summary>
        public static MouseWheelActions MouseWheelAction { get; set; } = MouseWheelActions.ScrollVertically;


        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Ctrl key
        /// </summary>
        public static MouseWheelActions MouseWheelCtrlAction { get; set; } = MouseWheelActions.Zoom;

        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Shift key
        /// </summary>
        public static MouseWheelActions MouseWheelShiftAction { get; set; } = MouseWheelActions.ScrollHorizontally;

        /// <summary>
        /// ~Gets, sets action to be performed when user spins the mouse wheel while holding Alt key
        /// </summary>
        public static MouseWheelActions MouseWheelAltAction { get; set; } = MouseWheelActions.DoNothing;


        /// <summary>
        /// ~Gets, sets value indicating that Confirmation dialog is displayed when deleting image
        /// </summary>
        public static bool IsConfirmationDelete { get; set; } = false;


        /// <summary>
        /// ~Gets, sets the value indicates that viewer scrollbars are visible
        /// </summary>
        public static bool IsScrollbarsVisible { get; set; } = false;


        /// <summary>
        /// ~Gets, sets the list of Image Editing Association
        /// </summary>
        public static List<ImageEditingAssociation> ImageEditingAssociationList { get; set; } = new List<ImageEditingAssociation>();


        /// <summary>
        /// ~Gets, sets the value indicates that the viewing image is auto-saved after rotating
        /// </summary>
        public static bool IsSaveAfterRotating { get; set; } = false;

        /// <summary>
        /// ~Setting to control whether the image's original modified date value is preserved on save
        /// </summary>
        public static bool PreserveModifiedDate { get; set; } = true;

        /// <summary>
        /// ~Gets, sets the value indicates that there is a new version
        /// </summary>
        public static bool IsNewVersionAvailable { get; set; } = false;


        /// <summary>
        /// ~Gets, sets zoom mode value
        /// </summary>
        public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;


        /// <summary>
        /// ~Gets, sets zoom optimization value
        /// </summary>
        public static ZoomOptimizationMethods ZoomOptimizationMethod { get; set; } = ZoomOptimizationMethods.Auto;


        /// <summary>
        /// Number format to use for save/restore ImageGlass settings
        /// </summary>
        public static NumberFormatInfo NumberFormat
        {
            get
            {
                var newFormat = new NumberFormatInfo
                {
                    NegativeSign = "-"
                };
                return newFormat;
            }
        }


        /// <summary>
        /// ~Gets, sets the value indicates that to show full image path or only base name
        /// </summary>
        public static bool IsDisplayBasenameOfImage { get; set; } = false;


        /// <summary>
        /// ~Gets, sets toolbar position
        /// </summary>
        public static ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Top;


        /// <summary>
        /// ~Gets, sets the value indicates that to toolbar buttons to be centered horizontally
        /// </summary>
        public static bool IsCenterToolbar { get; set; } = true;


        /// <summary>
        /// ~Gets, sets the value indicates that to show last seen image on startup
        /// </summary>
        public static bool IsOpenLastSeenImage { get; set; } = false;


        /// <summary>
        /// ~Gets, sets color profile string. It can be a defined name or ICC/ICM file path
        /// </summary>
        public static string ColorProfile { get; set; } = "sRGB";


        /// <summary>
        /// ~Gets, sets the value indicates that the ColorProfile will be applied for all or only the images with embedded profile
        /// </summary>
        public static bool IsApplyColorProfileForAll { get; set; } = false;


        /// <summary>
        /// ~Gets, sets the value indicates that to show or hide the Navigation Buttons on viewer
        /// </summary>
        public static bool IsShowNavigationButtons { get; set; } = false;


        /// <summary>
        /// ~Gets, sets the value indicates that to checkerboard in the image region only
        /// </summary>
        public static bool IsShowCheckerboardOnlyImageRegion { get; set; } = false;


        /// <summary>
        /// ~Gets, sets the number of images cached by Image
        /// </summary>
        public static int ImageBoosterCachedCount { get; set; } = 1;


        
        /// <summary>
        /// ~The toolbar button configuration: contents and order.
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
        /// ~User-selected action tied to key pairings. E.g. Left/Right arrows: prev/next image
        /// </summary>
        public static string KeyAssignments { get; set; } = $"" +
            $"{(int)KeyCombos.LeftRight},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.UpDown},{(int)AssignableActions.PanUpDown};" +
            $"{(int)KeyCombos.PageUpDown},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.SpaceBack},{(int)AssignableActions.PauseSlideshow};";

        #endregion



        #region Public Methods


        /// <summary>
        /// Load the default built-in image formats to the list
        /// </summary>
        public static void LoadBuiltInImageFormats()
        {
            var exts = Constants.BuiltInImageFormats.Split("|".ToCharArray());

            GlobalSetting.DefaultImageFormats = exts[0];
            GlobalSetting.OptionalImageFormats = exts[1];
        }


        /// <summary>
        /// Save ImageEditingAssociationList to Settings
        /// </summary>
        public static void SaveConfigOfImageEditingAssociationList()
        {
            StringBuilder editingAssocString = new StringBuilder();

            foreach (var assoc in GlobalSetting.ImageEditingAssociationList)
            {
                editingAssocString.Append($"[{assoc.ToString()}]");
            }

            GlobalSetting.SetConfig("ImageEditingAssociationList", editingAssocString.ToString());
        }


        /// <summary>
        /// Get ImageEditingAssociation from ImageEditingAssociationList
        /// </summary>
        /// <param name="ext">Extension to search. Ex: .png</param>
        /// <returns></returns>
        public static ImageEditingAssociation GetImageEditingAssociationFromList(string ext)
        {
            if (GlobalSetting.ImageEditingAssociationList.Count > 0)
            {
                try
                {
                    var assoc = GlobalSetting.ImageEditingAssociationList.FirstOrDefault(v => v.Extension.CompareTo(ext) == 0);

                    return assoc;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }


        /// <summary>
        /// Get file extensions from registry
        /// Ex: *.svg;*.png;
        /// </summary>
        /// <returns></returns>
        public static string GetFileExtensionsFromRegistry()
        {
            StringBuilder exts = new StringBuilder();

            RegistryHelper reg = new RegistryHelper()
            {
                BaseRegistryKey = Registry.LocalMachine,
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations"
            };
            var extList = reg.GetValueNames();

            foreach (var ext in extList)
            {
                exts.Append($"*{ext};");
            }

            return exts.ToString();
        }


        /// <summary>
        /// Get image order from configuration file
        /// </summary>
        /// <returns></returns>
        public static ImageOrderBy GetImageOrderConfig()
        {
            string s = GetConfig("ImageLoadingOrder", "0");

            if (int.TryParse(s, out int i))
            {
                if (-1 < i && i < Enum.GetNames(typeof(ImageOrderBy)).Length) //<=== Number of items in enum
                { }
                else
                {
                    i = 0;
                }
            }

            return (ImageOrderBy)i;
        }


        /// <summary>
        /// Get image order type from configuration file
        /// </summary>
        /// <returns></returns>
        public static ImageOrderType GetImageOrderTypeConfig()
        {
            string s = GetConfig("ImageLoadingOrderType", "0");

            if (int.TryParse(s, out int i))
            {
                if (-1 < i && i < Enum.GetNames(typeof(ImageOrderType)).Length) //<=== Number of items in enum
                { }
                else
                {
                    i = 0;
                }
            }

            return (ImageOrderType)i;
        }


        /// <summary>
        /// Gets a specify config. Return "" if not found.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GetConfig(key, "");
        }


        /// <summary>
        /// Gets a specify config. Return @defaultValue if not found.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="defaultValue">Default value</param>=
        /// <returns></returns>
        public static string GetConfig(string configKey, string @defaultValue = "")
        {
            // Read configs from file
            return _configFile.GetConfig(configKey, defaultValue);
        }


        /// <summary>
        /// Sets a specify config.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public static void SetConfig(string configKey, string value)
        {
            // Write configs to file
            _configFile.SetConfig(configKey, value);
        }


        /// <summary>
        /// Take the supported extensions string from GlobalSetting and convert it 
        /// to a faster lookup mechanism and with wildcard removed.
        /// 
        /// Intended to fix the observed issue where "string.Contains" would cause
        /// unsupported extensions such as ".c", ".h", ".md", etc to pass.
        /// </summary>
        public static void BuildImageFormatHashSet()
        {
            char[] wildtrim = { '*' };
            var allTypes = GlobalSetting.AllImageFormats;

            var typesArray = allTypes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            GlobalSetting.ImageFormatHashSet = new HashSet<string>();

            foreach (var aType in typesArray)
            {
                string wildRemoved = aType.Trim(wildtrim);
                GlobalSetting.ImageFormatHashSet.Add(wildRemoved);
            }
        }


        #endregion



        #region Keyboard customization
        // The user is permitted to choose what action to associate to a key-pairing.
        // E.g. PageUp/PageDown to next/previous image.

        // The KeyPair -> action lookup
        private static Dictionary<KeyCombos, AssignableActions> KeyActionLookup;

        // Note: default value matches the IGV6 behavior
        private static string DEFAULT_KEY_ASSIGNMENTS = "0,0;1,2;2,0;3,4;";

        /// <summary>
        /// Load the KeyPair -> action values from the config file into the lookup
        /// dictionary.
        /// </summary>
        public static void LoadKeyAssignments()
        {
            try
            {
                KeyAssignments = GetConfig("KeyboardActions", DEFAULT_KEY_ASSIGNMENTS);
                SetKeyAssignments();
            }
            catch (Exception e)
            {
                ResetKeyActionsToDefault();
            }
        }

        private static void ResetKeyActionsToDefault()
        {
            KeyAssignments = DEFAULT_KEY_ASSIGNMENTS;
            SetKeyAssignments();
        }

        private static void SetKeyAssignments()
        {
            var part_sep = new [] { ',' };
            var pairs = KeyAssignments.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            KeyActionLookup = new Dictionary<KeyCombos, AssignableActions>();
            foreach (var pair in pairs)
            {
                var parts = pair.Split(part_sep);
                int part1 = int.Parse(parts[0]);
                int part2 = int.Parse(parts[1]);
                KeyActionLookup.Add((KeyCombos)part1, (AssignableActions)part2);
            }
        }

        /// <summary>
        /// For a given key-pair, return the user-chosen action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AssignableActions GetKeyAction(KeyCombos key)
        {
            try
            {
                return KeyActionLookup[key];
            }
            catch
            {
                // KBR 20170716 not quite sure how we might get here, but
                // don't blow up nastily if something went wrong loading 
                // the key assignments from the config file
                ResetKeyActionsToDefault();
                return KeyActionLookup[key];
            }
        }

        public static void SetKeyAction(KeyCombos which, int newval)
        {
            KeyActionLookup[which] = (AssignableActions)newval;
        }

        /// <summary>
        /// Write the key-pair customizations to the config file.
        /// </summary>
        public static void SaveKeyAssignments()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in KeyActionLookup.Keys)
            {
                sb.Append((int)key);
                sb.Append(',');
                sb.Append((int)KeyActionLookup[key]);
                sb.Append(';');
            }
            KeyAssignments = sb.ToString();
            SetConfig("KeyboardActions", KeyAssignments);
        }
        #endregion

    }
}
