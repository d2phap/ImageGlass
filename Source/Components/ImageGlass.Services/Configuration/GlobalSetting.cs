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
using ImageGlass.Core;
using System.Drawing;
using System.Windows.Forms;
using System.Security;
using System.IO;
using System.Text;
using ImageGlass.Library.FileAssociations;
using System.Linq;
using System.Globalization;

namespace ImageGlass.Services.Configuration
{
    public static class GlobalSetting
    {
        // Configurations file: igconfig.xml
        private static ConfigurationFile _configFile = new ConfigurationFile();



        /// <summary>
        /// First launch version constant. 
        /// If the value read from config file is less than this value, 
        /// the First-Launch Configs screen will be launched.
        /// </summary>
        public const int FIRST_LAUNCH_VERSION = 5;



        #region Private Properties

        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static ImgMan ImageList { get; set; } = new ImgMan();


        /// <summary>
        /// Gets, sets current index of image
        /// </summary>
        public static int CurrentIndex { get; set; } = -1;


        /// <summary>
        /// Gets, sets the value indicates that StartUpDir is writable. NOTE***: need to be manually update by calling GlobalSetting.CheckStartUpDirWritable()
        /// </summary>
        public static bool IsStartUpDirWritable { get; set; } = true;


        /// <summary>
        /// Gets, sets recursive value
        /// </summary>
        public static bool IsRecursiveLoading { get; set; } = false;


        /// <summary>
        /// Gets, sets image loading order
        /// </summary>
        public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;


        /// <summary>
        /// Gets, sets showing/loading hidden images
        /// </summary>
        public static bool IsShowingHiddenImages { get; set; } = false;


        /// <summary>
        /// Gets built-in image formats for both Default and Optional formats
        /// </summary>
        public static string BuiltInImageFormats { get; } = "*.bmp;*.cur;*.cut;*.dds;*.dib;*.emf;*.exif;*.gif;*.heic;*.ico;*.jfif;*.jpe;*.jpeg;*.jpg;*.pbm;*.pcx;*.pgm;*.png;*.ppm;*.psb;*.svg;*.tif;*.tiff;*.webp;*.wmf;*.wpg;*.xbm;*.xpm;|*.exr;*.hdr;*.psd;*.tga;" + "*.3fr;*.ari;*.arw;*.bay;*.crw;*.cr2;*.cap;*.dcs;*.dcr;*.dng;*.drf;*.eip;*.erf;*.fff;*.gpr;*.iiq;*.k25;*.kdc;*.mdc;*.mef;*.mos;*.mrw;*.nef;*.nrw;*.obm;*.orf;*.pef;*.ptx;*.pxn;*.r3d;*.raf;*.raw;*.rwl;*.rw2;*.rwz;*.sr2;*.srf;*.srw;*.tif;*.x3f;";


        /// <summary>
        /// Gets or sets the hash array of all supported formats. 
        /// **NOTE: this needs to be manually updated by calling GlobalSetting.MakeImageTypeSet()
        /// </summary>
        public static HashSet<string> ImageFormatHashSet { get; set; } = new HashSet<string>();

        

        #endregion

        


        #region Public Properties

        /// <summary>
        /// Gets, sets language pack
        /// </summary>
        public static Library.Language LangPack { get; set; } = new Library.Language();


        /// <summary>
        /// Gets all supported extensions string
        /// </summary>
        public static string AllImageFormats
        {
            get
            {
                return GlobalSetting.DefaultImageFormats + GlobalSetting.OptionalImageFormats;
            }
        }


        /// <summary>
        /// Gets, sets default image formats
        /// </summary>
        public static string DefaultImageFormats { get; set; } = string.Empty;


        /// <summary>
        /// Gets, sets optional image formats
        /// </summary>
        public static string OptionalImageFormats { get; set; } = string.Empty;


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
        /// Gets, sets image error value
        /// </summary>
        public static bool IsImageError { get; set; } = false;


        /// <summary>
        /// Gets, sets fixed width on zooming
        /// </summary>
        public static double ZoomLockValue { get; set; } = 100.0;


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
        /// Gets, sets background color
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.White;


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
        /// Gets, sets slide show interval
        /// </summary>
        public static int SlideShowInterval { get; set; } = 5;


        /// <summary>
        /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
        /// </summary>
        public static bool IsLoopBackViewer { get; set; } = true;


        /// <summary>
        /// Gets, sets value that allow user speed up image loading when navigate back
        /// </summary>
        public static bool IsImageBoosterBack { get; set; } = true;


        /// <summary>
        /// Gets, sets value indicating that allow quit application by ESC
        /// </summary>
        public static bool IsPressESCToQuit { get; set; } = true;


        /// <summary>
        /// Gets, sets value indicating that checker board is shown or not
        /// </summary>
        public static bool IsShowCheckerBoard { get; set; } = false;


        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension { get; set; } = 96;


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
        /// Gets, sets width of horizontal thumbnail bar
        /// </summary>
        public static int ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(48, true).GetTotalDimension();


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
        /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
        /// </summary>
        public static bool IsConfirmationDelete { get; set; } = false;


        /// <summary>
        /// Gets, sets the value indicates that viewer scrollbars are visible
        /// </summary>
        public static bool IsScrollbarsVisible { get; set; } = false;


        /// <summary>
        /// Gets, sets the list of Image Editing Association
        /// </summary>
        public static List<ImageEditingAssociation> ImageEditingAssociationList { get; set; } = new List<ImageEditingAssociation>();


        /// <summary>
        /// Gets, sets the value indicates that the viewing image is auto-saved after rotating
        /// </summary>
        public static bool IsSaveAfterRotating { get; set; } = false;

        /// <summary>
        /// Setting to control whether the image's original modified date value is preserved on save
        /// </summary>
        public static bool PreserveModifiedDate { get; set; } = false;

        /// <summary>
        /// Gets, sets the value indicates that there is a new version
        /// </summary>
        public static bool IsNewVersionAvailable { get; set; } = false;
        

        /// <summary>
        /// Gets, sets zoom mode value
        /// </summary>
        public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;

        
        /// <summary>
        /// Gets, sets zoom optimization value
        /// </summary>
        public static ZoomOptimizationMethods ZoomOptimizationMethod { get; set; } = ZoomOptimizationMethods.Auto;


        private static NumberFormatInfo _numFormat = new NumberFormatInfo();
        /// <summary>
        /// Default number format for ImageGlass
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
            set => _numFormat = value;
        }


        /// <summary>
        /// Gets, sets the value indicates that to show full image path or only base name
        /// </summary>
        public static bool IsDisplayBasenameOfImage { get; set; } = false;


        /// <summary>
        /// Gets, sets toolbar position
        /// </summary>
        public static ToolbarPosition ToolbarPosition { get; set; } = ToolbarPosition.Top;


        /// <summary>
        /// Gets, sets the value indicates that to toolbar buttons to be centered horizontally
        /// </summary>
        public static bool IsCenterToolbar { get; set; } = true;


        /// <summary>
        /// Gets, sets the value indicates that to show last seen image on startup
        /// </summary>
        public static bool IsOpenLastSeenImage { get; set; } = false;


        /// <summary>
        /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
        /// </summary>
        public static string ColorProfile { get; set; } = "sRGB";


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
        /// Gets, sets the value indicates that a message is displayed if the program is busy
        /// </summary>
        public static bool IsDisplayLoadMessage { get; set; } = true;


        /// <summary>
        /// The toolbar button configuration: contents and order.
        /// </summary>
        public static string ToolbarButtons { get; set; } = $"" +
            $"{(int)Configuration.ToolbarButtons.btnBack}," +
            $"{(int)Configuration.ToolbarButtons.btnNext}," +
            $"{(int)Configuration.ToolbarButtons.Separator}," +

            $"{(int)Configuration.ToolbarButtons.btnRotateLeft}," +
            $"{(int)Configuration.ToolbarButtons.btnRotateRight}," +
            $"{(int)Configuration.ToolbarButtons.btnFlipHorz}," +
            $"{(int)Configuration.ToolbarButtons.btnFlipVert}," +
            $"{(int)Configuration.ToolbarButtons.btnZoomIn}," +
            $"{(int)Configuration.ToolbarButtons.btnZoomOut}," +
            $"{(int)Configuration.ToolbarButtons.btnActualSize}," +
            $"{(int)Configuration.ToolbarButtons.btnWindowAutosize}," +
            $"{(int)Configuration.ToolbarButtons.Separator}," +

            $"{(int)Configuration.ToolbarButtons.btnAutoZoom}," +
            $"{(int)Configuration.ToolbarButtons.btnScaletoWidth}," +
            $"{(int)Configuration.ToolbarButtons.btnScaletoHeight}," +
            $"{(int)Configuration.ToolbarButtons.btnScaleToFit}," +
            $"{(int)Configuration.ToolbarButtons.btnZoomLock}," +
            $"{(int)Configuration.ToolbarButtons.Separator}," +

            $"{(int)Configuration.ToolbarButtons.btnOpen}," +
            $"{(int)Configuration.ToolbarButtons.btnRefresh}," +
            $"{(int)Configuration.ToolbarButtons.btnGoto}," +
            $"{(int)Configuration.ToolbarButtons.Separator}," +

            $"{(int)Configuration.ToolbarButtons.btnThumb}," +
            $"{(int)Configuration.ToolbarButtons.btnCheckedBackground}," +
            $"{(int)Configuration.ToolbarButtons.btnFullScreen}," +
            $"{(int)Configuration.ToolbarButtons.btnSlideShow}," +
            $"{(int)Configuration.ToolbarButtons.btnConvert}," +
            $"{(int)Configuration.ToolbarButtons.btnPrintImage}," +
            $"{(int)Configuration.ToolbarButtons.btnDelete},";







        #endregion



        #region Public Methods

        /// <summary>
        /// Get the path based on the startup folder of ImageGlass.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string StartUpDir(params string[] paths)
        {
            var path = Application.StartupPath;

            var newPaths = paths.ToList();
            newPaths.Insert(0, path);

            return Path.Combine(newPaths.ToArray());
        }


        /// <summary>
        /// Get the path based on the configuration folder of ImageGlass.
        /// For portable mode, ConfigDir = Installed Dir, else %appdata%\ImageGlass
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string ConfigDir(params string[] paths)
        {
            if (IsStartUpDirWritable)
            {
                return GlobalSetting.StartUpDir(paths);
            }

            var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass");

            var newPaths = paths.ToList();
            newPaths.Insert(0, configDir);
            configDir = Path.Combine(newPaths.ToArray());

            return configDir;
        }


        /// <summary>
        /// Load the default built-in image formats to the list
        /// </summary>
        public static void LoadBuiltInImageFormats()
        {
            var exts = GlobalSetting.BuiltInImageFormats.Split("|".ToCharArray());

            GlobalSetting.DefaultImageFormats = exts[0];
            GlobalSetting.OptionalImageFormats = exts[1];
        }


        /// <summary>
        /// Save ImageEditingAssociationList to Settings
        /// </summary>
        public static void SaveConfigOfImageEditingAssociationList()
        {
            StringBuilder editingAssocString = new StringBuilder();

            foreach(var assoc in GlobalSetting.ImageEditingAssociationList)
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

            foreach(var ext in extList)
            {
                exts.Append($"*{ext};");
            }

            return exts.ToString();
        }


        /// <summary>
        /// Load image order from configuration file
        /// </summary>
        public static ImageOrderBy LoadImageOrderConfig()
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

            //ImageLoadingOrder = (ImageOrderBy)i;

            return (ImageOrderBy)i;
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
        /// Convert String to Rectangle
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Rectangle StringToRect(string str)
        {
            string[] args = str.Split(',');
            int[] arg = new int[args.Length];
            for (int a = 0; a < arg.Length; a++)
            {
                arg[a] = Convert.ToInt32(args[a]);
            }
            return new Rectangle(arg[0], arg[1], arg[2], arg[3]);
        }


        /// <summary>
        /// Convert Rectangle to String
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static string RectToString(Rectangle rc)
        {
            return rc.Left + "," + rc.Top + "," + rc.Width + "," + rc.Height;
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


        /// <summary>
        /// Check if startup folder is writable
        /// </summary>
        /// <returns></returns>
        public static bool CheckStartUpDirWritable()
        {
            try
            {
                var filePath = GlobalSetting.StartUpDir("test_write_file.temp");

                using (File.Create(filePath)) { }
                File.Delete(filePath);

                return true;
            }
            catch// (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }


        #endregion

    }
}
