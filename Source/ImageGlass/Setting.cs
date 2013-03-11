/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2012 DUONG DIEU PHAP
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

namespace ImageGlass
{
    public enum ImageOrderBy
    {
        Name,
        Length,
        CreationTime,
        Extension,
        LastAccessTime,
        LastWriteTime
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
        private static int _thumbnailMaxFileSize = 3;
        private static bool _isPlaySlideShow = false;
        private static bool _isSmoothPanning = true;
        private static bool _isLockWorkspaceEdges = true;
        private static bool _isZooming = false;
        private static bool _isShowThumbnail = false;
        private static bool _isImageError = false;
        private static double _zoomLockValue = 1;
        private static ZoomOptimizationValue _zoomOptimizationMethod = ZoomOptimizationValue.Auto;

        
        


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
        /// Get, set support extension string
        /// </summary>
        public static string SupportedExtensions
        {
            get { return Setting._supportedExtensions; }
            set { Setting._supportedExtensions = value; }
        }

        //Get, set max file size of thumbnail image file
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
            set { Setting._isLockWorkspaceEdges = value; }
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
            set { Setting._zoomOptimizationMethod = value; }
        }

    }
}
