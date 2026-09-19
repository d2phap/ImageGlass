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
namespace ImageGlass.Common;

public record ConfigMetadata
{
    public string Description { get; set; } = "ImageGlass Configuration File";
    public float Version { get; set; } = 10.0f;
}



public enum ConfigId
{

    #region Boolean settings

    ShowToolbar,
    ShowToolbarInFullscreen,
    ShowGallery,
    ShowGalleryInFullscreen,
    ShowGalleryFileName,
    ShowAppIcon,
    EnableMainWindowMaximized,
    EnableSettingsWindowMaximized,
    EnableSlideshow,
    EnableSlideshowCountdown,
    EnableSlideshowRandomInterval,
    EnableLoopSlideshow,
    EnableFullscreenSlideshow,
    EnableFrameless,
    EnableFullScreen,
    EnableLoopBackNavigation,
    EnableAutoSwitchSiblingDir,
    EnableMultiInstances,
    EnableWindowTopMost,
    EnableFreePan,
    EnableDeleteConfirmation,
    EnableSaveConfirmation,
    EnablePreserveModifiedDate,
    EnableOpenSaveAsInCurrentFolder,
    EnableWelcomeImage,
    EnableLastSeenImage,
    EnableVectorRenderer,
    EnableHdrToneMapping,
    EnableAlwaysApplyColorProfile,
    EnableNavigationButtons,
    EnableSubfoldersLoading,
    EnableExplorerSortOrder,
    EnableImageFolderGrouping,
    EnableHiddenImagesLoading,
    EnableWindowFit,
    EnableCenterWindowFit,
    EnableOnlyLoadRawPreview,
    EnableOnlyLoadNonRawPreview,
    EnableImagePreview,
    EnableGalleryShellThumbnail,
    EnableCopyMultipleFiles,
    EnableCutMultipleFiles,
    EnableFileWatcher,
    EnableAutoOpenNewAddedImage,
    EnableAutoInstallUpdate,
    EnableDebug,

    #endregion // Boolean settings


    #region Number settings

    QuickSetupVersion,
    PanMargin,
    PanSpeed,
    ZoomSpeed,
    SlideshowInterval,
    SlideshowIntervalTo,
    SlideshowImagesToNotifySound,
    ThumbnailSize,
    GalleryCacheSizeInMb,
    GalleryColumns,
    CacheMaxFiles,
    CacheMaxMemoryInMb,
    CacheMaxDimension,
    CacheMaxFileSizeInMb,
    ZoomLockValue,
    ToolbarIconHeight,
    ImageEditQuality,
    InAppMessageDuration,
    PreviewMinWidth,
    PreviewMinHeight,

    #endregion // Number settings


    #region String settings

    AutoUpdate,
    UpdateSkippedVersion,
    UpdatePendingVersion,
    ColorProfile,
    LastSeenImagePath,
    LastOpenedTool,
    LastOpenedSetting,
    BackgroundColor,
    SlideshowBackgroundColor,
    DarkTheme,
    LightTheme,
    Language,

    #endregion // String settings


    #region Enum settings

    CheckerboardMode,
    BrowsingMode,
    ImageLoadingOrder,
    ImageLoadingOrderType,
    ZoomMode,
    ImageInterpolationScaleDown,
    ImageInterpolationScaleUp,
    AfterEditingAction,
    WindowBackdrop,

    #endregion // Enum settings


    #region Array settings

    MainWindowBounds,
    SettingsWindowBounds,
    ZoomLevels,
    EditApps,
    SingleFrameFormats,
    FileFormats,
    ImageInfoTags,
    MenuHotkeys,
    MouseClickActions,
    MouseWheelActions,
    Layout,
    PluginTrust,
    Tools,
    ToolSettings,
    ToolbarButtons,

    #endregion // Array settings

}


