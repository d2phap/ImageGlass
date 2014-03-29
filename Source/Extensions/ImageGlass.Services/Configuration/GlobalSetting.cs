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
using Microsoft.Win32;
using ImageGlass.Core;
using System.Drawing;
using System.Windows.Forms;

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
        private static string _currentPath = "";
        private static bool _isRecursive = false;
        private static ImageOrderBy _imageOrderBy = ImageOrderBy.Name;
        private static string _supportedExtensions = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                     "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                                     "*.exif;*.wmf;*.emf;";
        private static string _contextMenuExtensions = "";
        //private static int _thumbnailMaxFileSize = 3;
        private static bool _isPlaySlideShow = false;
        //private static bool _isSmoothPanning = true;
        //private static bool _isLockWorkspaceEdges = true;
        //private static bool _isZooming = false;
        private static bool _isShowThumbnail = false;
        private static bool _isImageError = false;
        private static int _zoomLockValue = 100;
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;
        private static bool _isWelcomePicture = true;
        private static Color _backgroundColor = Color.White;
        private static bool _isHideToolBar = false;

        private static Library.Language _langPack = new Library.Language();


        #region "Properties"
        
        /// <summary>
        /// Get, set image list
        /// </summary>
        public static ImgMan ImageList
        {
            get { return GlobalSetting._imageList; }
            set { GlobalSetting._imageList = value; }
        }

        /// <summary>
        /// Get, set filename list
        /// </summary>
        public static List<String> ImageFilenameList
        {
            get { return GlobalSetting._imageFilenameList; }
            set { GlobalSetting._imageFilenameList = value; }
        }

        /// <summary>
        /// Get, set Access token of Facebook
        /// </summary>
        public static string FacebookAccessToken
        {
            get { return GlobalSetting._facebookAccessToken; }
            set { GlobalSetting._facebookAccessToken = value; }
        }

        /// <summary>
        /// Get, set active value whenever hover on picturebox
        /// </summary>
        public static bool IsForcedActive
        {
            get { return GlobalSetting._isForcedActive; }
            set { GlobalSetting._isForcedActive = value; }
        }

        /// <summary>
        /// Get, set current index of image
        /// </summary>
        public static int CurrentIndex
        {
            get { return GlobalSetting._currentIndex; }
            set { GlobalSetting._currentIndex = value; }
        }

        /// <summary>
        /// Get, set current path (directory only) of image
        /// </summary>
        public static string CurrentPath
        {
            get { return GlobalSetting._currentPath; }
            set { GlobalSetting._currentPath = value; }
        }

        /// <summary>
        /// Get, set recursive value
        /// </summary>
        public static bool IsRecursive
        {
            get { return GlobalSetting._isRecursive; }
            set { GlobalSetting._isRecursive = value; }
        }

        /// <summary>
        /// Get, set image order
        /// </summary>
        public static ImageOrderBy ImageOrderBy
        {
            get { return GlobalSetting._imageOrderBy; }
            set { GlobalSetting._imageOrderBy = value; }
        }

        /// <summary>
        /// Get, set supported extension string
        /// </summary>
        public static string SupportedExtensions
        {
            get
            {
                GlobalSetting._supportedExtensions = GlobalSetting.GetConfig("SupportedExtensions",
                                                        GlobalSetting._supportedExtensions);

                return GlobalSetting._supportedExtensions;
            }
            set
            {
                GlobalSetting._supportedExtensions = value;
                GlobalSetting.SetConfig("SupportedExtensions", GlobalSetting._supportedExtensions);
            }
        }

        /// <summary>
        /// Get, set the Context menu Extensions
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
        /// Get, set value of slideshow state
        /// </summary>
        public static bool IsPlaySlideShow
        {
            get { return GlobalSetting._isPlaySlideShow; }
            set { GlobalSetting._isPlaySlideShow = value; }
        }

        //Get, set value of smooth panning
        //public static bool IsSmoothPanning
        //{
        //    get { return GlobalSetting._isSmoothPanning; }
        //    set { GlobalSetting._isSmoothPanning = value; }
        //}

        /// <summary>
        /// Get, set the value of moving, lock onto screen edges
        /// </summary>
        //public static bool IsLockWorkspaceEdges
        //{
        //    get { return GlobalSetting._isLockWorkspaceEdges; }
        //    set
        //    {
        //        GlobalSetting._isLockWorkspaceEdges = value;
        //        GlobalSetting.SetConfig("LockToEdge", value.ToString());
        //    }
        //}

        /// <summary>
        /// Get, set value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail
        {
            get { return GlobalSetting._isShowThumbnail; }
            set { GlobalSetting._isShowThumbnail = value; }
        }

        /// <summary>
        /// Get, set image error value
        /// </summary>
        public static bool IsImageError
        {
            get { return GlobalSetting._isImageError; }
            set { GlobalSetting._isImageError = value; }
        }

        /// <summary>
        /// Get, set fixed width on zooming
        /// </summary>
        public static int ZoomLockValue
        {
            get { return GlobalSetting._zoomLockValue; }
            set { GlobalSetting._zoomLockValue = value; }
        }

        /// <summary>
        /// Get, set zoom optimization value
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
        /// Get, set welcome picture value
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
        /// Get, set background color
        /// </summary>
        public static Color BackgroundColor
        {
            get { return GlobalSetting._backgroundColor; }
            set { GlobalSetting._backgroundColor = value; }
        }

        /// <summary>
        /// Get, set value of visibility of toolbar when start up
        /// </summary>
        public static bool IsHideToolBar
        {
            get
            {
                return GlobalSetting._isHideToolBar;
            }
            set
            {
                GlobalSetting._isHideToolBar = value;
            }
        }


        /// <summary>
        /// Get, set language pack
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
        /// Lấy dữ liệu thứ tự sắp xếp ảnh
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
        /// Lấy thông tin cấu hình. Trả về "" nếu không tìm thấy.
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            return GlobalSetting.GetConfig(key, "");
        }

        /// <summary>
        /// Lấy thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="defaultValue">Giá trị mặc định nếu không tìm thấy</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue)
        {
            string hkey = @"HKEY_CURRENT_USER\Software\PhapSoftware\ImageGlass\";
            return Registry.GetValue(hkey, key, defaultValue).ToString();
        }

        /// <summary>
        /// Gán thông tin cấu hình
        /// </summary>
        /// <param name="key">Tên cấu hình</param>
        /// <param name="value">Giá trị cấu hình</param>
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
