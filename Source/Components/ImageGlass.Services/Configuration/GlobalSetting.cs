/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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
using System.Collections.Specialized;
using System.Security;
using System.IO;

namespace ImageGlass.Services.Configuration
{
    public enum ImageOrderBy
    {
        Name = 0,
        Length = 1,
        CreationTime = 2,
        Extension = 3,
        LastAccessTime = 4,
        LastWriteTime = 5,
        Random = 6
    }

    public enum ZoomOptimizationValue
    {
        Auto,
        SmoothPixels,
        ClearPixels
    }

    public enum ImageExtensionGroup
    {
        Default = 0,
        Optional = 1
    }

    public class ThumbnailItemInfo
    {
        /// <summary>
        /// Gets actual thumbnail dimension
        /// </summary>
        public int Dimension { get; }

        /// <summary>
        /// Gets extra space to adapt minimum width / height of thumbnail bar
        /// </summary>
        public int ExtraSpace { get; }

        /// <summary>
        /// Gets total dimension needed for minimum width / height of thumbnail bar
        /// </summary>
        public int TotalDimension
        {
            get
            {
                return Dimension + ExtraSpace;
            }
        }

        /// <summary>
        /// Thumbnail item information
        /// </summary>
        /// <param name="dimension">Thumbnail size</param>
        /// <param name="isHorizontalView">Horizontal or Verticle view</param>
        public ThumbnailItemInfo(int dimension, bool isHorizontalView)
        {
            if (isHorizontalView)
            {
                Dimension = dimension;
                ExtraSpace = 58;
            }
            else {
                switch (dimension)
                {
                    case 32:
                        Dimension = 32;
                        ExtraSpace = 48;
                        break;

                    case 48:
                        Dimension = 48;
                        ExtraSpace = 52;
                        break;

                    case 64:
                        Dimension = 64;
                        ExtraSpace = 57;
                        break;

                    case 96:
                        Dimension = 96;
                        ExtraSpace = 69;
                        break;

                    case 128:
                        Dimension = 128;
                        ExtraSpace = 79;
                        break;

                    default:
                        Dimension = 48;
                        ExtraSpace = 57;
                        break;
                }
            }
        }
    }

    public static class GlobalSetting
    {
        // Private steeings --------------------------------------------------------------
        private static ImgMan _imageList = new ImgMan();
        private static List<String> _imageFilenameList = new List<string>();
        private static string _facebookAccessToken = "";
        private static bool _isForcedActive = true;
        private static int _currentIndex = -1;
        private static bool _isRecursiveLoading = false;
        private static StringCollection _stringClipboard = new StringCollection();
        private static string _tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Temp");
        private static Library.Language _langPack = new Library.Language();
        private static bool _isPortableMode = false;
        private static bool _isStartUpDirWritable = true;
        private static ConfigurationFile _configFile = new ConfigurationFile();
        private static string _builtInImageFormats = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;*.exif;*.wmf;*.emf;*.svg;*.webp;*.tga;|*.hdr;*.exr;*.tga;*.psd;";


        // Shared settings ----------------------------------------------------------------
        private static ImageOrderBy _imageOrderBy = ImageOrderBy.Name;
        private static string _defaultImageFormats = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                     "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                                     "*.exif;*.wmf;*.emf;*.svg;*.webp;*.tga;";
        private static string _optionalImageFormats = "*.hdr;*.exr;*.tga;*.psd;";
        private static bool _isPlaySlideShow = false;
        private static bool _isFullScreen = false;
        private static bool _isShowThumbnail = false;
        private static bool _isImageError = false;
        private static bool _isEnableZoomLock = false;
        private static bool _isZoomToFit = false;
        private static int _zoomLockValue = 100;
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;
        private static bool _isShowWelcome = true;
        private static Color _backgroundColor = Color.White;
        private static bool _isShowToolBar = true;
        private static bool _isLoopBackSlideShow = false;
        private static bool _isLoopBackViewer = true;
        private static bool _isImageBoosterBack = true;
        private static bool _isPressESCToQuit = true;
        private static int _thumbnailDimension = 48;
        private static int _thumbnailBarWidth = new ThumbnailItemInfo(48, true).TotalDimension;
        private static bool _isThumbnailHorizontal = false;
        
        private static bool _isAllowMultiInstances = true;
        private static bool _isShowCheckedBackground = false;
        private static bool _isTempMemoryData = false;
        private static bool _isMouseNavigation = false;
        
        private static bool _isWindowAlwaysOnTop = false;
        private static bool _isConfirmationDelete = false;
        

        


        #region "Properties"
        
        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static ImgMan ImageList
        {
            get { return GlobalSetting._imageList; }
            set { GlobalSetting._imageList = value; }
        }

        /// <summary>
        /// Gets, sets filename list
        /// </summary>
        public static List<String> ImageFilenameList
        {
            get { return GlobalSetting._imageFilenameList; }
            set { GlobalSetting._imageFilenameList = value; }
        }

        /// <summary>
        /// Gets, sets Access token of Facebook
        /// </summary>
        public static string FacebookAccessToken
        {
            get { return GlobalSetting._facebookAccessToken; }
            set { GlobalSetting._facebookAccessToken = value; }
        }

        /// <summary>
        /// Gets, sets active value whenever hover on picturebox
        /// </summary>
        public static bool IsForcedActive
        {
            get { return GlobalSetting._isForcedActive; }
            set { GlobalSetting._isForcedActive = value; }
        }

        /// <summary>
        /// Gets, sets current index of image
        /// </summary>
        public static int CurrentIndex
        {
            get { return GlobalSetting._currentIndex; }
            set { GlobalSetting._currentIndex = value; }
        }

        /// <summary>
        /// Gets, sets recursive value
        /// </summary>
        public static bool IsRecursiveLoading
        {
            get { return GlobalSetting._isRecursiveLoading; }
            set { GlobalSetting._isRecursiveLoading = value; }
        }

        /// <summary>
        /// Gets, sets image order
        /// </summary>
        public static ImageOrderBy ImageOrderBy
        {
            get { return GlobalSetting._imageOrderBy; }
            set { GlobalSetting._imageOrderBy = value; }
        }

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
        public static string DefaultImageFormats
        {
            get { return _defaultImageFormats; }
            set { _defaultImageFormats = value; }
        }

        /// <summary>
        /// Gets, sets optional image formats
        /// </summary>
        public static string OptionalImageFormats
        {
            get { return _optionalImageFormats; }
            set { _optionalImageFormats = value; }
        }

        /// <summary>
        /// Gets, sets the Context menu Extensions
        /// </summary>
        //public static string ContextMenuExtensions
        //{
        //    get
        //    {
        //        _contextMenuExtensions = GetConfig("ContextMenuExtensions", "");
        //        return _contextMenuExtensions;
        //    }
        //    set
        //    {
        //        _contextMenuExtensions = value;
        //        GlobalSetting.SetConfig("ContextMenuExtensions", _contextMenuExtensions);
        //    }
        //}

        /// <summary>
        /// Gets, sets value of slideshow state
        /// </summary>
        public static bool IsPlaySlideShow
        {
            get { return GlobalSetting._isPlaySlideShow; }
            set { GlobalSetting._isPlaySlideShow = value; }
        }


        /// <summary>
        /// Gets, sets value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail
        {
            get { return GlobalSetting._isShowThumbnail; }
            set { GlobalSetting._isShowThumbnail = value; }
        }

        /// <summary>
        /// Gets, sets image error value
        /// </summary>
        public static bool IsImageError
        {
            get { return GlobalSetting._isImageError; }
            set { GlobalSetting._isImageError = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that Zoom Lock enabled
        /// </summary>
        public static bool IsEnabledZoomLock
        {
            get { return _isEnableZoomLock; }
            set { _isEnableZoomLock = value; }
        }

        /// <summary>
        /// Gets, sets fixed width on zooming
        /// </summary>
        public static int ZoomLockValue
        {
            get { return GlobalSetting._zoomLockValue; }
            set { GlobalSetting._zoomLockValue = value; }
        }

        /// <summary>
        /// Gets, sets zoom optimization value
        /// </summary>
        public static ZoomOptimizationValue ZoomOptimizationMethod
        {
            get { return GlobalSetting._zoomOptimizationMethod; }
            set
            {
                GlobalSetting._zoomOptimizationMethod = value;

                if (value == ZoomOptimizationValue.SmoothPixels)
                {
                    GlobalSetting.SetConfig("ZoomOptimization", "1");
                }
                else if (value == ZoomOptimizationValue.ClearPixels)
                {
                    GlobalSetting.SetConfig("ZoomOptimization", "2");
                }
                else
                {
                    GlobalSetting.SetConfig("ZoomOptimization", "0");
                }
            }
        }

        /// <summary>
        /// Gets, sets welcome picture value
        /// </summary>
        public static bool IsShowWelcome
        {
            get { return GlobalSetting._isShowWelcome; }
            set
            {
                GlobalSetting._isShowWelcome = value;
                GlobalSetting.SetConfig("Welcome", value.ToString());
            }
        }

        /// <summary>
        /// Gets, sets background color
        /// </summary>
        public static Color BackgroundColor
        {
            get { return GlobalSetting._backgroundColor; }
            set { GlobalSetting._backgroundColor = value; }
        }

        /// <summary>
        /// Gets, sets value of visibility of toolbar when start up
        /// </summary>
        public static bool IsShowToolBar
        {
            get { return GlobalSetting._isShowToolBar; }
            set { GlobalSetting._isShowToolBar = value; }
        }

        /// <summary>
        /// Gets, sets value that allows user to loop back to the first image when reaching the end of list
        /// </summary>
        public static bool IsLoopBackSlideShow
        {
            get { 
                return GlobalSetting._isLoopBackSlideShow; 
            }
            set { 
                GlobalSetting._isLoopBackSlideShow = value; 
            }
        }

        /// <summary>
        /// Gets, sets value that allow user speed up image loading when navigate back
        /// </summary>
        public static bool IsImageBoosterBack
        {
            get { return GlobalSetting._isImageBoosterBack; }
            set { GlobalSetting._isImageBoosterBack = value; }
        }

        /// <summary>
        /// Gets, sets copied filename collection (multi-copy)
        /// </summary>
        public static StringCollection StringClipboard
        {
            get { return GlobalSetting._stringClipboard; }
            set { GlobalSetting._stringClipboard = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that allow quit application by ESC
        /// </summary>
        public static bool IsPressESCToQuit
        {
            get { return GlobalSetting._isPressESCToQuit; }
            set { GlobalSetting._isPressESCToQuit = value; }
        }

        /// <summary>
        /// Gets, sets language pack
        /// </summary>
        public static Library.Language LangPack
        {
            get { return GlobalSetting._langPack; }
            set
            {
                GlobalSetting._langPack = value;
                GlobalSetting.SetConfig("Language", _langPack.FileName);
            }

        }

        /// <summary>
        /// Gets start up directory of ImageGlass
        /// </summary>
        public static string StartUpDir
        {
            get
            {
                return (Application.StartupPath + "\\").Replace("\\\\", "\\");
            }
        }

        /// <summary>
        /// Gets, sets value indicating that wheather the window is full screen or not
        /// </summary>
        public static bool IsFullScreen
        {
            get { return _isFullScreen; }
            set { _isFullScreen = value; }
        }

        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension
        {
            get { return _thumbnailDimension; }
            set { _thumbnailDimension = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that multi instances is allowed or not
        /// </summary>
        public static bool IsAllowMultiInstances
        {
            get { return _isAllowMultiInstances; }
            set { _isAllowMultiInstances = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that checked background is shown or not
        /// </summary>
        public static bool IsShowCheckedBackground
        {
            get { return GlobalSetting._isShowCheckedBackground; }
            set { GlobalSetting._isShowCheckedBackground = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that the image we are processing is memory data (clipboard / screenshot,...) or not
        /// </summary>
        public static bool IsTempMemoryData
        {
            get { return GlobalSetting._isTempMemoryData; }
            set { GlobalSetting._isTempMemoryData = value; }
        }

        /// <summary>
        /// Gets temporary directory of ImageGlass, e.g. C:\Users\xxx\AppData\Roaming\ImageGlass\
        /// </summary>
        public static string TempDir
        {
            get { return _tempDir; }
        }

        /// <summary>
        /// Gets, sets value indicating that frmMain is always on top or not.
        /// </summary>
        public static bool IsWindowAlwaysOnTop
        {
            get { return _isWindowAlwaysOnTop; }
            set { _isWindowAlwaysOnTop = value; }
        }

        /// <summary>
        /// Is the thumbnail bar to be shown horizontal (down at the bottom) or vertical (on right side)?
        /// </summary>
        public static bool IsThumbnailHorizontal
        {
            get { return _isThumbnailHorizontal; }
            set { _isThumbnailHorizontal = value; }
        }

        /// <summary>
        /// Gets, sets width of horizontal thumbnail bar
        /// </summary>
        public static int ThumbnailBarWidth
        {
            get { return _thumbnailBarWidth; }
            set { _thumbnailBarWidth = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that 'Zoom to Fit' is enabled or not
        /// </summary>
        public static bool IsZoomToFit
        {
            get { return _isZoomToFit; }
            set { _isZoomToFit = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that using mouse wheel to navigate image or not
        /// </summary>
        public static bool IsMouseNavigation
        {
            get { return _isMouseNavigation; }
            set { _isMouseNavigation = value; }
        }

        /// <summary>
        /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
        /// </summary>
        public static bool IsConfirmationDelete
        {
            get => _isConfirmationDelete;
            set => _isConfirmationDelete = value;
        }

        /// <summary>
        /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list
        /// </summary>
        public static bool IsLoopBackViewer { get => _isLoopBackViewer; set => _isLoopBackViewer = value; }

        /// <summary>
        /// Gets, sets value indicating that ImageGlass will run in portable mode
        /// All configurations will be written to XML file instead of registry
        /// </summary>
        public static bool IsPortableMode
        {
            get => _isPortableMode;
            set
            {
                //check if we have write access to write config file for portable mode
                if (value && GlobalSetting.IsStartUpDirWritable == false)
                {
                    //we dont have permission
                    _isPortableMode = false;
                }
                else
                {
                    _isPortableMode = value;
                }
            }
        }

        /// <summary>
        /// Gets, sets the value indicates that StartUpDir is writable
        /// </summary>
        public static bool IsStartUpDirWritable
        {
            get => _isStartUpDirWritable;
            set => _isStartUpDirWritable = value;
        }

        /// <summary>
        /// Gets built-in image formats for both Default and Optional formats
        /// </summary>
        public static string BuiltInImageFormats { get => _builtInImageFormats; }




        #endregion


        #region "Public Method"

        /// <summary>
        /// Check is ImageGlass can write config file in the startup folder
        /// </summary>
        /// <returns></returns>
        public static bool CheckStartUpDirWritable()
        {
            return _configFile.IsWritable();
        }

        /// <summary>
        /// Load image order from configuration file
        /// </summary>
        public static void LoadImageOrderConfig()
        {
            string s = GetConfig("ImageLoadingOrder", "0");

            if (int.TryParse(s, out int i))
            {
                if (-1 < i && i < 7) //<=== Number of items in array
                { }
                else
                {
                    i = 0;
                }
            }
            if (i == 1)
            {
                ImageOrderBy = ImageOrderBy.Length;
            }
            else if (i == 2)
            {
                ImageOrderBy = ImageOrderBy.CreationTime;
            }
            else if (i == 3)
            {
                ImageOrderBy = ImageOrderBy.LastAccessTime;
            }
            else if (i == 4)
            {
                ImageOrderBy = ImageOrderBy.LastWriteTime;
            }
            else if (i == 5)
            {
                ImageOrderBy = ImageOrderBy.Extension;
            }
            else if (i == 6)
            {
                ImageOrderBy = ImageOrderBy.Random;
            }
            else
            {
                ImageOrderBy = ImageOrderBy.Name;
            }
            
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
        /// <param name="defaultValue">Default value</param>
        /// <param name="forceGetConfigsFromRegistry">True: always read configs from Registry</param>
        /// <returns></returns>
        public static string GetConfig(string configKey, string @defaultValue = "", bool forceGetConfigsFromRegistry = false)
        {
            // Portable mode: retrieve config from file -----------------------------
            if (GlobalSetting.IsPortableMode && !forceGetConfigsFromRegistry)
            {
                return _configFile.GetConfig(configKey, defaultValue);
            }


            // Read configs from Registry --------------------------------------------
            try
            {
                RegistryKey workKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PhapSoftware\ImageGlass"); // Work path in the registry.
                object configValue = null; // Value of the configuration obtained.

                if (workKey != null)
                {
                    configValue = workKey.GetValue(configKey, defaultValue); // Get the config value.
                }
                else
                {
                    throw new Exception(); // Force catch.
                }
                return configValue.ToString();
            }
            catch (SecurityException)
            {
                return defaultValue;
            }
            catch (IOException)
            {
                return defaultValue;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
            catch (Exception)
            {
                // Need a repair function.
                return defaultValue;
            }
            
        }

        /// <summary>
        /// Sets a specify config.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="value">Configuration value</param>
        /// <param name="forceWriteConfigsToRegistry">True: always write configs to Registry</param>
        public static void SetConfig(string configKey, string value, bool @forceWriteConfigsToRegistry = false)
        {
            // Portable mode: retrieve config from file -----------------------------
            if (GlobalSetting.IsPortableMode && !@forceWriteConfigsToRegistry)
            {
                _configFile.SetConfig(configKey, value);
                return;
            }


            // Read configs from Registry --------------------------------------------
            RegistryKey workKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\PhapSoftware\ImageGlass", RegistryKeyPermissionCheck.ReadWriteSubTree); // Work path in the registry.

            if (workKey == null)
            {
                workKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\PhapSoftware\ImageGlass", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            try
            {
                workKey.SetValue(configKey, value);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex) { }
#pragma warning restore CS0168 // Variable is declared but never used
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

        #endregion

    }
}
