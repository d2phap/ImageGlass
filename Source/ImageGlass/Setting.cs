/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Linq;
using System.Text;
using ImageGlass.Core;
using System.Drawing;
using Microsoft.Win32;
using System.Configuration;
using System.Windows.Forms;

namespace ImageGlass
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

    public static class Setting
    {
        private static frmFacebook _fFacebook = new frmFacebook();
        private static frmSetting _fSetting = new frmSetting();
        private static frmExtension _fExtension = new frmExtension();

        private static ImgMan _imageList = new ImgMan();
        private static List<String> _imageFilenameList = new List<string>();
        private static string _facebookAccessToken = "";
        private static bool _isForcedActive = true;
        private static int _currentIndex = 0;
        private static string _currentPath = "";
        private static bool _isRecursive = false;
        private static ImageOrderBy _imageOrderBy = ImageOrderBy.Name;
        private static string _supportedExtensions = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                     "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" + 
                                                     "*.exif;*.wmf;*.emf;";
        private static string _contextMenuExtensions = "";
        private static int _thumbnailMaxFileSize = 3;
        private static bool _isPlaySlideShow = false;
        private static bool _isSmoothPanning = true;
        private static bool _isLockWorkspaceEdges = true;
        private static bool _isZooming = false;
        private static bool _isShowThumbnail = false;
        private static bool _isImageError = false;
        private static double _zoomLockValue = 1;
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;
        private static bool _isWelcomePicture = true;
        private static Color _backgroundColor = Color.White;
        private static bool _isHideToolBar = false;

        private static string _languageFile = "English";


        #region "Properties"
        /// <summary>
        /// Form frmFacebook
        /// </summary>
        public static frmFacebook FFacebook
        {
            get { return Setting._fFacebook; }
            set { Setting._fFacebook = value; }
        }

        /// <summary>
        /// Form frmSetting
        /// </summary>
        public static frmSetting FSetting
        {
            get { return Setting._fSetting; }
            set { Setting._fSetting = value; }
        }

        /// <summary>
        /// Form frmExtension
        /// </summary>
        public static frmExtension FExtension
        {
            get { return Setting._fExtension; }
            set { Setting._fExtension = value; }
        }

        /// <summary>
        /// Get, set image list
        /// </summary>
        public static ImgMan ImageList
        {
            get { return Setting._imageList; }
            set { Setting._imageList = value; }
        }

        /// <summary>
        /// Get, set filename list
        /// </summary>
        public static List<String> ImageFilenameList
        {
            get { return Setting._imageFilenameList; }
            set { Setting._imageFilenameList = value; }
        }

        /// <summary>
        /// Get, set Access token of Facebook
        /// </summary>
        public static string FacebookAccessToken
        {
            get { return Setting._facebookAccessToken; }
            set { Setting._facebookAccessToken = value; }
        }

        /// <summary>
        /// Get, set active value whenever hover on picturebox
        /// </summary>
        public static bool IsForcedActive
        {
            get { return Setting._isForcedActive; }
            set { Setting._isForcedActive = value; }
        }

        /// <summary>
        /// Get, set current index of image
        /// </summary>
        public static int CurrentIndex
        {
            get { return Setting._currentIndex; }
            set { Setting._currentIndex = value; }
        }

        /// <summary>
        /// Get, set current path (full filename) of image
        /// </summary>
        public static string CurrentPath
        {
            get { return Setting._currentPath; }
            set { Setting._currentPath = value; }
        }

        /// <summary>
        /// Get, set recursive value
        /// </summary>
        public static bool IsRecursive
        {
            get { return Setting._isRecursive; }
            set { Setting._isRecursive = value; }
        }

        /// <summary>
        /// Get, set image order
        /// </summary>
        public static ImageOrderBy ImageOrderBy
        {
            get { return Setting._imageOrderBy; }
            set { Setting._imageOrderBy = value; }
        }

        /// <summary>
        /// Get, set supported extension string
        /// </summary>
        public static string SupportedExtensions
        {
            get
            {
                Setting._supportedExtensions = Setting.GetConfig("SupportedExtensions", 
                                                        Setting._supportedExtensions);

                return Setting._supportedExtensions;
            }
            set
            {
                Setting._supportedExtensions = value;
                Setting.SetConfig("SupportedExtensions", Setting._supportedExtensions);
            }
        }

        /// <summary>
        /// Get, set the Context menu Extensions
        /// </summary>
        public static string ContextMenuExtensions
        {
            get
            {
                Setting._contextMenuExtensions = Setting.GetConfig("ContextMenuExtensions", "");                                                        
                return Setting._contextMenuExtensions;
            }
            set
            {
                Setting._contextMenuExtensions = value;
                Setting.SetConfig("ContextMenuExtensions", Setting._contextMenuExtensions);
            }
        }


        /// <summary>
        /// Get, set max file size of thumbnail image file
        /// </summary>
        public static int ThumbnailMaxFileSize
        {
            get { return Setting._thumbnailMaxFileSize; }
            set { Setting._thumbnailMaxFileSize = value; }
        }

        /// <summary>
        /// Get, set value of slideshow state
        /// </summary>
        public static bool IsPlaySlideShow
        {
            get { return Setting._isPlaySlideShow; }
            set { Setting._isPlaySlideShow = value; }
        }

        //Get, set value of smooth panning
        public static bool IsSmoothPanning
        {
            get { return Setting._isSmoothPanning; }
            set { Setting._isSmoothPanning = value; }
        }

        /// <summary>
        /// Get, set the value of moving, lock onto screen edges
        /// </summary>
        public static bool IsLockWorkspaceEdges
        {
            get { return Setting._isLockWorkspaceEdges; }
            set
            {
                Setting._isLockWorkspaceEdges = value;
                Setting.SetConfig("LockToEdge", value.ToString());
            }
        }

        /// <summary>
        /// Get, set the value of zoom event
        /// </summary>
        public static bool IsZooming
        {
            get { return Setting._isZooming; }
            set { Setting._isZooming = value; }
        }

        /// <summary>
        /// Get, set value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail
        {
            get { return Setting._isShowThumbnail; }
            set { Setting._isShowThumbnail = value; }
        }

        /// <summary>
        /// Get, set image error value
        /// </summary>
        public static bool IsImageError
        {
            get { return Setting._isImageError; }
            set { Setting._isImageError = value; }
        }

        /// <summary>
        /// Get, set fixed width on zooming
        /// </summary>
        public static double ZoomLockValue
        {
            get { return Setting._zoomLockValue; }
            set { Setting._zoomLockValue = value; }
        }

        /// <summary>
        /// Get, set zoom optimization value
        /// </summary>
        public static ZoomOptimizationValue ZoomOptimizationMethod
        {
            get { return Setting._zoomOptimizationMethod; }
            set
            {
                Setting._zoomOptimizationMethod = value;

                if (value == ZoomOptimizationValue.SmoothPixels)
                {
                    Setting.SetConfig("ZoomOptimize", "1");
                }
                else if (value == ZoomOptimizationValue.ClearPixels)
                {
                    Setting.SetConfig("ZoomOptimize", "2");
                }
                else
                {
                    Setting.SetConfig("ZoomOptimize", "0");
                }
            }
        }

        /// <summary>
        /// Get, set welcome picture value
        /// </summary>
        public static bool IsWelcomePicture
        {
            get { return Setting._isWelcomePicture; }
            set
            {
                Setting._isWelcomePicture = value;
                Setting.SetConfig("Welcome", value.ToString());
            }
        }

        /// <summary>
        /// Get, set background color
        /// </summary>
        public static Color BackgroundColor
        {
            get { return Setting._backgroundColor; }
            set { Setting._backgroundColor = value; }
        }

        /// <summary>
        /// Get, set value of visibility of toolbar when start up
        /// </summary>
        public static bool IsHideToolBar
        {
            get
            {
                return Setting._isHideToolBar;
            }
            set
            {
                Setting._isHideToolBar = value;
            }
        }

        /// <summary>
        /// Get, set language
        /// </summary>
        public static string LanguageFile
        {
            get
            {
                Setting._languageFile = Setting.GetConfig("Language", "English");
                return Setting._languageFile;
            }
            set
            {
                Setting._languageFile = value;
                Setting.SetConfig("Language", Setting._languageFile);
            }
        }

        /// <summary>
        /// Get start up directory of ImageGlass
        /// </summary>
        public static string StartUpDir
        {
            get
            {
                return (Application.StartupPath + "\\").Replace("\\\\", "\\");
            }
        }

        #endregion


        #region "Public Method"

        /// <summary>
        /// Lấy kích thước tối đa sẽ nạp file ảnh thu nhỏ
        /// </summary>
        public static void LoadMaxThumbnailFileSizeConfig()
        {
            string s = Setting.GetConfig("MaxThumbnailFileSize", "1");

            int i = 1;
            int.TryParse(s, out i);
            Setting.ThumbnailMaxFileSize = i;
        }

        /// <summary>
        /// Lấy dữ liệu thứ tự sắp xếp ảnh
        /// </summary>
        public static void LoadImageOrderConfig()
        {
            string s = Setting.GetConfig("ImageLoadingOrder", "0");

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
                Setting.ImageOrderBy = ImageOrderBy.Length;
            }
            else if (i == 2)
            {
                Setting.ImageOrderBy = ImageOrderBy.CreationTime;
            }
            else if (i == 3)
            {
                Setting.ImageOrderBy = ImageOrderBy.LastAccessTime;
            }
            else if (i == 4)
            {
                Setting.ImageOrderBy = ImageOrderBy.LastWriteTime;
            }
            else if (i == 5)
            {
                Setting.ImageOrderBy = ImageOrderBy.Extension;
            }
            else if (i == 6)
            {
                Setting.ImageOrderBy = ImageOrderBy.Random;
            }
            else
            {
                Setting.ImageOrderBy = ImageOrderBy.Name;
            }
        }

        /// <summary>
        /// Lấy thông tin cấu hình. Trả về "" nếu không tìm thấy.
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GetConfig(key, "");
        }

        /// <summary>
        /// Lấy thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="defaultValue">Giá trị mặc định nếu không tìm thấy</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue)
        {
            // Open App.Config of executable
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration
                                                        (ConfigurationUserLevel.None);

            //Kiểm tra sự tồn tại của Key
            int index = config.AppSettings.Settings.AllKeys.ToList().IndexOf(key);

            //Nếu tồn tại
            if (index != -1)
            {
                //Thì lấy giá trị
                return config.AppSettings.Settings[key].Value;
            }
            else //Nếu không tồn tại
            {
                //Trả về giá trị mặc định
                return defaultValue;
            }
            
        }
        
        /// <summary>
        /// Gán thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="value">Giá trị cấu hình</param>
        public static void SetConfig(string key, string value)
        {
            // Open App.Config of executable
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration
                                                        (ConfigurationUserLevel.None);
            
            //Kiểm tra sự tồn tại của Key
            int index = config.AppSettings.Settings.AllKeys.ToList().IndexOf(key);

            //Nếu tồn tại
            if (index != -1)
            {
                //Thì cập nhật
                config.AppSettings.Settings[key].Value = value;
            }
            else //Nếu không tồn tại
            {
                //Tạo Key mới
                config.AppSettings.Settings.Add(key, value);
            }

            // Save the changes in App.config file.
            config.Save(ConfigurationSaveMode.Modified);
            

            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");

        }

        /// <summary>
        /// Áp dụng gói ngôn ngữ
        /// </summary>
        /// <param name="l">Gói ngôn ngữ</param>
        public static void LoadLanguagePack(ImageGlass.Library.Language l)
        {
            
        }

        #endregion

    }
}
