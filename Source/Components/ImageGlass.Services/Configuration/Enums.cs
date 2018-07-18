/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

namespace ImageGlass.Services.Configuration
{
    /// <summary>
    /// The loading order list.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
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

    /// <summary>
    /// The list of Zoom Optimization.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum ZoomOptimizationMethods
    {
        Auto = 0,
        SmoothPixels = 1,
        ClearPixels = 2
    }

    public enum ImageFormatGroup
    {
        Default = 0,
        Optional = 1
    }

    public enum Constants
    {
        MENU_ICON_HEIGHT = 21,
        TOOLBAR_ICON_HEIGHT = 20,
        TOOLBAR_HEIGHT = 40,
        VIEWER_GRID_SIZE = 8
    }

    /// <summary>
    /// The list of mousewheel actions.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum MouseWheelActions
    {
        DoNothing = 0,
        Zoom = 1,
        ScrollVertically = 2,
        ScrollHorizontally = 3,
        BrowseImages = 4
    }

    /// <summary>
    /// Define the flags to tell frmMain update the UI
    /// </summary>
    [Flags]
    public enum MainFormForceUpdateAction
    {
        NONE = 0,
        COLOR_PICKER_MENU = 1,
        THEME = 2,
        LANGUAGE = 4,
        THUMBNAIL_BAR = 8,
        THUMBNAIL_ITEMS = 16,
        TOOLBAR = 32,
        TOOLBAR_POSITION = 64,
        IMAGE_LIST = 128,
        IMAGE_FOLDER = 256,
        OTHER_SETTINGS = 512
    }


    /// <summary>
    /// The list of layout mode.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum LayoutMode
    {
        Standard = 0,
        Designer = 1
    }


    /// <summary>
    /// All the supported toolbar buttons. NOTE: the names here MUST match the field 
    /// name in frmMain! Reflection is used to fetch the image and string from the
    /// frmMain field.
    ///
    /// The integer value of the enum is used for storing the config info.
    /// </summary>
    public enum ToolbarButtons
    {
        Separator = -1,
        btnBack = 0,
        btnNext = 1,
        btnRotateLeft = 2,
        btnRotateRight = 3,
        btnZoomIn = 4,
        btnZoomOut = 5,
        btnScaleToFit = 6,
        btnActualSize = 7,
        btnZoomLock = 8,
        btnScaletoWidth = 9,
        btnScaletoHeight = 10,
        btnWindowAutosize = 11,
        btnOpen = 12,
        btnRefresh = 13,
        btnGoto = 14,
        btnThumb = 15,
        btnCheckedBackground = 16,
        btnFullScreen = 17,
        btnSlideShow = 18,
        btnConvert = 19,
        btnPrintImage = 20,
        btnDelete = 21,
        btnAutoZoom = 22,
        // NOTE: add new items here, must match order in _lstToolbarImg.Images list


        MAX // DO NOT ADD ANYTHING AFTER THIS
    }


    /// <summary>
    /// Zooming modes.
    /// </summary>
    public enum ZoomMode
    {
        AutoZoom = 0,
        ScaleToFit = 1,
        ScaleToWidth = 2,
        ScaleToHeight = 4,
        LockZoomRatio = 8
    }

    /// <summary>
    /// Toolbar position
    /// </summary>
    public enum ToolbarPosition
    {
        Top = 0,
        Bottom = 1
    }
}
