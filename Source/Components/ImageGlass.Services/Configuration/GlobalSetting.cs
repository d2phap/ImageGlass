/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2016 DUONG DIEU PHAP
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
        /// <param name="isHorizontalView">Horizontal or Vertical view</param>
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
        private static string _supportedDefaultExtensions = "*.jpg;*.jpe;*.jfif;*.jpeg;*.png;" +
                                                     "*.gif;*.ico;*.bmp;*.dib;*.tif;*.tiff;" +
                                                     "*.exif;*.wmf;*.emf;*.svg;*.webp;";
        private static string _supportedExtraExtensions = "*.hdr;*.exr;*.tga;*.psd;";
        private static string _contextMenuExtensions = "";
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;
        private static bool _isWelcomePicture = true;

        private static Library.Language _langPack = new Library.Language();


        #region "Properties"
        
        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static ImgMan ImageList { get; set; } = new ImgMan();

        /// <summary>
        /// Gets, sets filename list
        /// </summary>
        public static List<String> ImageFilenameList { get; set; } = new List<string>();

        /// <summary>
        /// Gets, sets Access token of Facebook
        /// </summary>
        public static string FacebookAccessToken { get; set; } = "";

        /// <summary>
        /// Gets, sets active value whenever hover on picturebox
        /// </summary>
        public static bool IsForcedActive { get; set; } = true;

        /// <summary>
        /// Gets, sets current index of image
        /// </summary>
        public static int CurrentIndex { get; set; } = -1;

        /// <summary>
        /// Gets, sets recursive value
        /// </summary>
        public static bool IsRecursive { get; set; } = false;

        /// <summary>
        /// Gets, sets image order
        /// </summary>
        public static ImageOrderBy ImageOrderBy { get; set; } = ImageOrderBy.Name;

        /// <summary>
        /// Gets all supported extensions string
        /// </summary>
        public static string SupportedExtensions
        {
            get
            {
                return _supportedDefaultExtensions + SupportedExtraExtensions;
            }
        }

        /// <summary>
        /// Gets default supported extension string
        /// </summary>
        public static string SupportedDefaultExtensions
        {
            get { return _supportedDefaultExtensions; }
        }

        /// <summary>
        /// Gets, sets supported extra extensions
        /// </summary>
        public static string SupportedExtraExtensions
        {
            get { return _supportedExtraExtensions; }
            set { _supportedExtraExtensions = value; }
        }

        /// <summary>
        /// Gets, sets the Context menu Extensions
        /// </summary>
        public static string ContextMenuExtensions
        {
            get
            {
                _contextMenuExtensions = GetConfig("ContextMenuExtensions", "");
                return _contextMenuExtensions;
            }
            set
            {
                _contextMenuExtensions = value;
                SetConfig("ContextMenuExtensions", _contextMenuExtensions);
            }
        }

        /// <summary>
        /// Gets, sets value of slideshow state
        /// </summary>
        public static bool IsPlaySlideShow { get; set; } = false;

        /// <summary>
        /// Gets, sets value of thumbnail visibility
        /// </summary>
        public static bool IsShowThumbnail { get; set; } = false;

        /// <summary>
        /// Gets, sets image error value
        /// </summary>
        public static bool IsImageError { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicating that Zoom Lock enabled
        /// </summary>
        public static bool IsEnabledZoomLock { get; set; }

        /// <summary>
        /// Gets, sets fixed width on zooming
        /// </summary>
        public static int ZoomLockValue { get; set; } = 100;

        /// <summary>
        /// Gets, sets zoom optimization value
        /// </summary>
        public static ZoomOptimizationValue ZoomOptimizationMethod
        {
            get { return _zoomOptimizationMethod; }
            set
            {
                _zoomOptimizationMethod = value;

                if (value == ZoomOptimizationValue.SmoothPixels)
                {
                    SetConfig("ZoomOptimization", "1");
                }
                else if (value == ZoomOptimizationValue.ClearPixels)
                {
                    SetConfig("ZoomOptimization", "2");
                }
                else
                {
                    SetConfig("ZoomOptimization", "0");
                }
            }
        }

        /// <summary>
        /// Gets, sets welcome picture value
        /// </summary>
        public static bool IsWelcomePicture
        {
            get { return _isWelcomePicture; }
            set
            {
                _isWelcomePicture = value;
                SetConfig("Welcome", value.ToString());
            }
        }

        /// <summary>
        /// Gets, sets background color
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.White;

        /// <summary>
        /// Gets, sets value of visibility of toolbar when start up
        /// </summary>
        public static bool IsShowToolBar { get; set; } = true;

        /// <summary>
        /// Gets, sets value that allows user to loop back to the first image when reaching the end of list
        /// </summary>
        public static bool IsLoopBackSlideShow { get; set; } = false;

        /// <summary>
        /// Gets, sets value that allow user speed up image loading when navigate back
        /// </summary>
        public static bool IsImageBoosterBack { get; set; } = true;

        /// <summary>
        /// Gets, sets copied filename collection (multi-copy)
        /// </summary>
        public static StringCollection StringClipboard { get; set; } = new StringCollection();

        /// <summary>
        /// Gets, sets value indicating that allow quit application by ESC
        /// </summary>
        public static bool IsPressESCToQuit { get; set; } = true;

        /// <summary>
        /// Gets, sets language pack
        /// </summary>
        public static Library.Language LangPack
        {
            get { return _langPack; }
            set
            {
                _langPack = value;
                SetConfig("Language", _langPack.FileName);
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
        public static bool IsFullScreen { get; set; } = false;

        /// <summary>
        /// Gets, sets value of thumbnail dimension in pixel
        /// </summary>
        public static int ThumbnailDimension { get; set; } = 48;

        /// <summary>
        /// Gets, sets value indicating that multi instances is allowed or not
        /// </summary>
        public static bool IsAllowMultiInstances { get; set; } = true;

        /// <summary>
        /// Gets, sets value indicating that checked background is shown or not
        /// </summary>
        public static bool IsShowCheckedBackground { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicating that the image we are processing is memory data (clipboard / screenshot,...) or not
        /// </summary>
        public static bool IsTempMemoryData { get; set; } = false;

        /// <summary>
        /// Gets temporary directory of ImageGlass, e.g. C:\Users\xxx\AppData\Roaming\ImageGlass\
        /// </summary>
        public static string TempDir { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ImageGlass\\Temp\\";

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
        public static int ThumbnailBarWidth { get; set; } = new ThumbnailItemInfo(48, true).TotalDimension;

        /// <summary>
        /// Gets, sets value indicating that 'Zoom to Fit' is enabled or not
        /// </summary>
        public static bool IsZoomToFit { get; set; } = false;

        /// <summary>
        /// Gets, sets value indicating that using mouse wheel to navigate image or not
        /// </summary>
        public static bool IsMouseNavigation { get; set; } = false;

        #endregion


        #region "Public Method"


        /// <summary>
        /// Load image order from configuration file
        /// </summary>
        public static void LoadImageOrderConfig()
        {
            string s = GetConfig("ImageLoadingOrder", "0");

            int i = 0;

            if (int.TryParse(s, out i))
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
