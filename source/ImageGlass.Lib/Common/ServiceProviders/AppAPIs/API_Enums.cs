/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
namespace ImageGlass.Common.ServiceProviders;

public enum API
{
    IG_OpenMainMenu,
    IG_OpenFolder,
    IG_OpenPath,
    IG_ViewByStep,
    IG_ViewByIndex,
    IG_OpenSettings,
    IG_ApplySettings,


    // Menu > File
    IG_OpenFile,
    IG_NewWindow,
    IG_Save,
    IG_SaveAs,
    IG_ExportImageFrames,
    IG_OpenWith,
    IG_Print,
    IG_Share,
    IG_OpenLocation,
    IG_Rename,
    IG_Delete,
    IG_OpenProperties,


    // Menu > Navigation
    IG_ViewNext,
    IG_ViewPrevious,
    IG_Goto,
    IG_GotoFirst,
    IG_GotoLast,
    IG_ViewFrame,
    IG_ViewNextFrame,
    IG_ViewPreviousFrame,
    IG_ViewFirstFrame,
    IG_ViewLastFrame,

    // Menu > Zoom
    IG_CustomZoom,
    IG_SetZoom,
    IG_SetZoomForMouseClick,
    IG_ZoomIn,
    IG_ZoomOut,
    IG_SetZoomMode,

    // Menu > Panning
    IG_PanLeft,
    IG_PanRight,
    IG_PanUp,
    IG_PanDown,
    IG_PanToLeft,
    IG_PanToRight,
    IG_PanToTop,
    IG_PanToBottom,

    // Menu > Image
    IG_Refresh,
    IG_Reload,
    IG_ReloadList,
    IG_Unload,
    IG_ToggleExplorerSortOrder,
    IG_SetLoadingOrderBy,
    IG_SetLoadingOrderType,
    IG_SetColorChannels,
    IG_OpenEditingApp,
    IG_InvertColors,
    IG_ToggleImageAnimation,
    IG_Rotate,
    IG_FlipImage,
    IG_SetDesktopBackground,
    IG_SetLockScreenImage,

    // Menu > Clipboard
    IG_PasteImage,
    IG_CopyImagePixels,
    IG_CopyImagePath,
    IG_CopyFiles,
    IG_CutFiles,
    IG_ClearClipboard,

    // Menu > Window mode
    IG_ToggleWindowFit,
    IG_ToggleFrameless,
    IG_ToggleFullScreen,
    IG_ToggleSlideshow,
    IG_ToggleSlideshowPlayback,
    IG_ToggleSlideshowCountdown,

    // Menu > Layout
    IG_ToggleToolbar,
    IG_ToggleGallery,
    IG_ToggleCheckerboard,
    IG_ToggleWindowTopMost,
    IG_SetBackgroundColor,

    // Menu > Tools
    IG_CloseCurrentTool,
    IG_ToggleTool,
    IG_OpenTool,
    IG_CloseTool,

    // Menu > Settings

    // Menu > Help
    IG_OpenAboutWindow,
    IG_ManageLicense,
    IG_CheckForUpdate,
    IG_InstallUpdate,
    IG_ReportIssue,
    IG_QuickSetup,
    IG_SetDefaultPhotoViewer,
    IG_RemoveDefaultPhotoViewer,

    // Menu > Exit
    IG_Exit,


    // Other APIs,
    IG_SetFileWatcher,
    IG_OpenContextMenu,

}

