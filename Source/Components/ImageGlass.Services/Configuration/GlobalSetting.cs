/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
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

    public static class GlobalSetting
    {
        private static ImgMan _imageList = new ImgMan();
        private static List<String> _imageFilenameList = new List<string>();
        private static string _facebookAccessToken = "";
        private static bool _isForcedActive = true;
        private static int _currentIndex = -1;
        private static bool _isRecursive = false;
        private static ImageOrderBy _imageOrderBy = ImageOrderBy.Name;
        private static string _supportedExtensions = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                     "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                                     "*.exif;*.wmf;*.emf;";
        private static string _supportedExtraExtensions = "*.tga;*.psd";
        private static string _contextMenuExtensions = "";
        private static bool _isPlaySlideShow = false;
        private static bool _isFullScreen = false;
        private static bool _isShowThumbnail = false;
        private static bool _isImageError = false;
        private static int _zoomLockValue = 100;
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;
        private static bool _isWelcomePicture = true;
        private static Color _backgroundColor = Color.White;
        private static bool _isShowToolBar = true;
        private static bool _isLoopBackSlideShow = false;
        private static bool _isImageBoosterBack = false;
        private static bool _isPressESCToQuit = true;
        private static int _thumbnailDimension = 48;
        private static StringCollection _stringClipboard = new StringCollection();
        private static bool _isAllowMultiInstances = true;
        private static bool _isShowCheckedBackground = false;
        private static bool _isTempMemoryData = false;


        private static Library.Language _langPack = new Library.Language();


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
        public static bool IsRecursive
        {
            get { return GlobalSetting._isRecursive; }
            set { GlobalSetting._isRecursive = value; }
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
        /// Gets, sets supported extension string
        /// </summary>
        public static string SupportedExtensions
        {
            get
            {
                return GlobalSetting._supportedExtensions + GlobalSetting.SupportedExtraExtensions;
            }
            set
            {
                GlobalSetting._supportedExtensions = value;
            }
        }

        /// <summary>
        /// Gets, sets supported extra extensions
        /// </summary>
        public static string SupportedExtraExtensions
        {
            get
            {
                return _supportedExtraExtensions;
            }
            set
            {
                _supportedExtraExtensions = value;
            }
        }

        /// <summary>
        /// Gets, sets the Context menu Extensions
        /// </summary>
        public static string ContextMenuExtensions
        {
            get
            {
                GlobalSetting._contextMenuExtensions = GlobalSetting.GetConfig("ContextMenuExtensions", "");
                return GlobalSetting._contextMenuExtensions;
            }
            set
            {
                GlobalSetting._contextMenuExtensions = value;
                GlobalSetting.SetConfig("ContextMenuExtensions", GlobalSetting._contextMenuExtensions);
            }
        }

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
                    GlobalSetting.SetConfig("ZoomOptimize", "1");
                }
                else if (value == ZoomOptimizationValue.ClearPixels)
                {
                    GlobalSetting.SetConfig("ZoomOptimize", "2");
                }
                else
                {
                    GlobalSetting.SetConfig("ZoomOptimize", "0");
                }
            }
        }

        /// <summary>
        /// Gets, sets welcome picture value
        /// </summary>
        public static bool IsWelcomePicture
        {
            get { return GlobalSetting._isWelcomePicture; }
            set
            {
                GlobalSetting._isWelcomePicture = value;
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
            get
            {
                return GlobalSetting._isShowToolBar;
            }
            set
            {
                GlobalSetting._isShowToolBar = value;
            }
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
            get
            {
                return _isFullScreen;
            }

            set
            {
                _isFullScreen = value;
            }
        }

        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension
        {
            get
            {
                return _thumbnailDimension;
            }

            set
            {
                _thumbnailDimension = value;
            }
        }

        /// <summary>
        /// Gets, sets value indicating that multi instances is allowed or not
        /// </summary>
        public static bool IsAllowMultiInstances
        {
            get
            {
                return _isAllowMultiInstances;
            }

            set
            {
                _isAllowMultiInstances = value;
            }
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

        

        #endregion


        #region "Public Method"


        /// <summary>
        /// Load image order from configuration file
        /// </summary>
        public static void LoadImageOrderConfig()
        {
            string s = GlobalSetting.GetConfig("ImageLoadingOrder", "0");

            int i = 0;

            if (int.TryParse(s, out i))
            {
                if (-1 < i && i < 7) //<=== Số lượng phần tử
                { }
                else
                {
                    i = 0;
                }
            }
            if (i == 1)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.Length;
            }
            else if (i == 2)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.CreationTime;
            }
            else if (i == 3)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.LastAccessTime;
            }
            else if (i == 4)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.LastWriteTime;
            }
            else if (i == 5)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.Extension;
            }
            else if (i == 6)
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.Random;
            }
            else
            {
                GlobalSetting.ImageOrderBy = ImageOrderBy.Name;
            }
        }

        /// <summary>
        /// Gets a specify config. Return "" if not found.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GlobalSetting.GetConfig(key, "");
        }

        /// <summary>
        /// Gets a specify config. Return @defaultValue if not found.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue)
        {
            try
            {
                // If the registry key doesn't exist, this would crash
                string hkey = @"HKEY_CURRENT_USER\Software\PhapSoftware\ImageGlass\";
                return Registry.GetValue(hkey, key, defaultValue).ToString();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a specify config.
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public static void SetConfig(string key, string value)
        {
            string hkey = @"HKEY_CURRENT_USER\Software\PhapSoftware\ImageGlass\";
            Registry.SetValue(hkey, key, value);
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
