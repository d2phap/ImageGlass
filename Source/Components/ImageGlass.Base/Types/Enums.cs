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

namespace ImageGlass.Base {
    /// <summary>
    /// The loading order list.
    /// **If we need to rename, we MUST update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum ImageOrderBy {
        Name = 0,
        Length = 1,
        CreationTime = 2,
        Extension = 3,
        LastAccessTime = 4,
        LastWriteTime = 5,
        Random = 6
    }

    /// <summary>
    /// The loading order types list
    /// **If we need to rename, we MUST update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum ImageOrderType {
        Asc = 0,
        Desc = 1
    }

    /// <summary>
    /// The list of Zoom Optimization.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum ZoomOptimizationMethods {
        Auto = 0,
        SmoothPixels = 1,
        ClearPixels = 2
    }

    /// <summary>
    /// The list of mousewheel actions.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum MouseWheelActions {
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
    public enum ForceUpdateActions {
        NONE = 0,
        OTHER_SETTINGS = 1,
        THEME = 2,
        LANGUAGE = 4,
        THUMBNAIL_BAR = 8,
        THUMBNAIL_ITEMS = 16,
        TOOLBAR = 32,
        TOOLBAR_POSITION = 64,
        TOOLBAR_ICON_HEIGHT = 128,
        IMAGE_LIST = 256,
        IMAGE_LIST_NO_RECURSIVE = 512,
        COLOR_PICKER_MENU = 1024,
        PAGE_NAV_MENU = 2048
    }

    /// <summary>
    /// The list of layout mode.
    /// **If we need to rename, have to update the language string too.
    /// Because the name is also language keyword!
    /// </summary>
    public enum LayoutMode {
        Standard = 0,
        Designer = 1
    }

    /// <summary>
    /// <para>
    /// All the supported toolbar buttons. NOTE: the names here MUST match the field
    /// name in frmMain! Reflection is used to fetch the image and string from the
    /// frmMain field.
    /// </para>
    /// <para>The integer value of the enum is used for storing the config info.</para>
    /// </summary>
    public enum ToolbarButton {
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
        btnWindowFit = 11,
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
        btnFlipHorz = 23,
        btnFlipVert = 24,
        btnScaleToFill = 25,
        btnEdit = 26,
        btnCrop = 27,
        btnColorPicker = 28,

        MAX // DO NOT ADD ANYTHING AFTER THIS
    }

    /// <summary>
    /// Zooming modes.
    /// </summary>
    [Flags]
    public enum ZoomMode {
        AutoZoom = 0,
        ScaleToFit = 1,
        ScaleToWidth = 2,
        ScaleToHeight = 4,
        LockZoomRatio = 8,
        ScaleToFill = 16,
    }

    /// <summary>
    /// Toolbar position
    /// </summary>
    public enum ToolbarPosition {
        Top = 0,
        Bottom = 1
    }

    /// <summary>
    /// Color channels of image, the value should be same as MagickImage.Channels enum
    /// </summary>
    public enum ColorChannels {
        All = -1, // not applicable

        Red = 1,
        Green = 2,
        Blue = 4,
        Black = 8,
        Alpha = 16,
    }

    /// <summary>
    /// Actions the user can assign to keys
    /// </summary>
    public enum AssignableActions {
        DoNothing = -1,    // error case
        PrevNextImage = 0, // previous/next image in list
        PanLeftRight,      // pan current image left/right
        PanUpDown,         // pan current image up/down
        ZoomInOut,         // zoom current image in/out
        PauseSlideshow,    // placeholder for V6 space key behavior

    }

    /// <summary>
    /// User customizable key pairs
    /// </summary>
    public enum KeyCombos {
        LeftRight = 0, // left/right arrow keys
        UpDown,      // up/down arrow keys
        PageUpDown,  // pageup/pagedown keys
        SpaceBack,   // space, backspace keys
    }

    /// <summary>
    /// Supported actions which can be assigned to mouse click
    /// </summary>
    public enum MouseAction {
        ToggleZoomFit = 0, // switch between 100% and fit-to-window
        ZoomIn,            // zoom in by zoom step
        ZoomOut,           // zoom out by zoom step
        NextImage,         // next image in list
        PrevImage,         // previous image in list
        ToggleFullScreen,  // toggle full-screen mode
        PopupMenu,         // bring up the popup menu
        ColorPick,         // select color under mouse cursor
        ZoomInToMouse,     // zoom in by zoom step, centered on mouse position
        ZoomOutToAuto,     // zoom out to Auto-zoom level
    }

    /// <summary>
    /// Supported customizable mouse click
    /// </summary>
    public enum MouseClick {
        Button1,    // Left single
        Button1Dbl,
        Button2,    // Right single
        Button2Dbl,
        Button3,    // Middle
        Button3Dbl,
        Button4,    // X1
        Button4Dbl,
        Button5,    // X2
        Button5Dbl,
    }

    /// <summary>
    /// Types of path
    /// </summary>
    public enum PathType {
        File,
        Dir,
    }

    /// <summary>
    /// Actions after opening editing app
    /// </summary>
    public enum AfterOpeningEditAppAction {
        Nothing = 0,
        Minimize = 1,
        Close = 2,
    }
}
