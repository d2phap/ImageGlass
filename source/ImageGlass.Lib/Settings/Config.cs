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
using Avalonia;
using ImageGlass.Common.Actions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.Common.Types.JsonTypeConverters;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageGlass.Common;


[JsonSerializable(typeof(Config))]
public partial class ConfigJsonContext : JsonSerializerContext { }


public partial class Config : PhReactive
{
    [JsonIgnore]
    private readonly Dictionary<ConfigId, object> _values = [];

    public ConfigMetadata _Metadata { get; set; } = new();



    #region Setting items

    #region Boolean items

    /// <summary>
    /// Gets, sets value of visibility of toolbar on start up
    /// </summary>
    public bool ShowToolbar
    {
        get => Get(ConfigId.ShowToolbar, true);
        set => Set(ConfigId.ShowToolbar, value);
    }

    /// <summary>
    /// Gets, sets value indicates that the toolbar should be hidden in Full screen mode
    /// </summary>
    public bool ShowToolbarInFullscreen
    {
        get => Get(ConfigId.ShowToolbarInFullscreen, false);
        set => Set(ConfigId.ShowToolbarInFullscreen, value);
    }

    /// <summary>
    /// Gets, sets value of gallery visibility
    /// </summary>
    public bool ShowGallery
    {
        get => Get(ConfigId.ShowGallery, true);
        set => Set(ConfigId.ShowGallery, value);
    }

    /// <summary>
    /// Gets, sets value indicates that the gallery should be hidden in Full screen mode
    /// </summary>
    public bool ShowGalleryInFullscreen
    {
        get => Get(ConfigId.ShowGalleryInFullscreen, false);
        set => Set(ConfigId.ShowGalleryInFullscreen, value);
    }

    /// <summary>
    /// Gets, sets value indicates that showing image file name on gallery
    /// </summary>
    public bool ShowGalleryFileName
    {
        get => Get(ConfigId.ShowGalleryFileName, true);
        set => Set(ConfigId.ShowGalleryFileName, value);
    }

    /// <summary>
    /// Gets, sets value of visibility of app icon
    /// </summary>
    public bool ShowAppIcon
    {
        get => Get(ConfigId.ShowAppIcon, true);
        set => Set(ConfigId.ShowAppIcon, value);
    }

    /// <summary>
    /// Gets, sets maximized state of main window.
    /// </summary>
    public bool EnableMainWindowMaximized
    {
        get => Get(ConfigId.EnableMainWindowMaximized, false);
        set => Set(ConfigId.EnableMainWindowMaximized, value);
    }

    /// <summary>
    /// Gets, sets maximized state of the settings window.
    /// </summary>
    public bool EnableSettingsWindowMaximized
    {
        get => Get(ConfigId.EnableSettingsWindowMaximized, false);
        set => Set(ConfigId.EnableSettingsWindowMaximized, value);
    }

    /// <summary>
    /// Gets, sets value indicating whether the slideshow mode is enabled or not.
    /// </summary>
    [JsonIgnore]
    public bool EnableSlideshow
    {
        get => Get(ConfigId.EnableSlideshow, false);
        set => Set(ConfigId.EnableSlideshow, value);
    }

    /// <summary>
    /// Gets, sets value if the countdown timer is shown or not.
    /// </summary>
    public bool EnableSlideshowCountdown
    {
        get => Get(ConfigId.EnableSlideshowCountdown, true);
        set => Set(ConfigId.EnableSlideshowCountdown, value);
    }

    /// <summary>
    /// Gets, sets value indicates whether the slide show interval is random.
    /// </summary>
    public bool EnableSlideshowRandomInterval
    {
        get => Get(ConfigId.EnableSlideshowRandomInterval, false);
        set => Set(ConfigId.EnableSlideshowRandomInterval, value);
    }

    /// <summary>
    /// Gets, sets value indicates that slideshow will loop back to the first image when reaching the end of list.
    /// </summary>
    public bool EnableLoopSlideshow
    {
        get => Get(ConfigId.EnableLoopSlideshow, true);
        set => Set(ConfigId.EnableLoopSlideshow, value);
    }

    /// <summary>
    /// Gets, sets value indicates that slideshow is played in full screen, not window mode.
    /// </summary>
    public bool EnableFullscreenSlideshow
    {
        get => Get(ConfigId.EnableFullscreenSlideshow, true);
        set => Set(ConfigId.EnableFullscreenSlideshow, value);
    }

    /// <summary>
    /// Gets, sets value of FrmMain's frameless mode.
    /// </summary>
    public bool EnableFrameless
    {
        get => Get(ConfigId.EnableFrameless, false);
        set => Set(ConfigId.EnableFrameless, value);
    }

    /// <summary>
    /// Gets, sets value indicating whether the full screen mode is enabled or not.
    /// </summary>
    public bool EnableFullScreen
    {
        get => Get(ConfigId.EnableFullScreen, false);
        set => Set(ConfigId.EnableFullScreen, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether loop-back navigation is enabled when browsing images.
    /// </summary>
    public bool EnableLoopBackNavigation
    {
        get => Get(ConfigId.EnableLoopBackNavigation, true);
        set => Set(ConfigId.EnableLoopBackNavigation, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether navigation automatically switches to the next/previous
    /// sibling directory (that contains images) when reaching the end/start of the current image list.
    /// Takes precedence over <see cref="EnableLoopBackNavigation"/> at the list boundary.
    /// </summary>
    public bool EnableAutoSwitchSiblingDir
    {
        get => Get(ConfigId.EnableAutoSwitchSiblingDir, false);
        set => Set(ConfigId.EnableAutoSwitchSiblingDir, value);
    }

    /// <summary>
    /// Gets, sets value indicating that multi instances is allowed.
    /// </summary>
    public bool EnableMultiInstances
    {
        get => Get(ConfigId.EnableMultiInstances, true);
        set => Set(ConfigId.EnableMultiInstances, value);
    }

    /// <summary>
    /// Gets, sets value indicating that app window is always on top..
    /// </summary>
    public bool EnableWindowTopMost
    {
        get => Get(ConfigId.EnableWindowTopMost, false);
        set => Set(ConfigId.EnableWindowTopMost, value);
    }

    /// <summary>
    /// Gets, sets value indicating whether free panning is allowed.
    /// </summary>
    public bool EnableFreePan
    {
        get => Get(ConfigId.EnableFreePan, false);
        set => Set(ConfigId.EnableFreePan, value);
    }

    /// <summary>
    /// Gets, sets value indicates that Confirmation dialog is displayed when deleting image
    /// </summary>
    public bool EnableDeleteConfirmation
    {
        get => Get(ConfigId.EnableDeleteConfirmation, true);
        set => Set(ConfigId.EnableDeleteConfirmation, value);
    }

    /// <summary>
    /// Gets, sets value indicates that Confirmation dialog is displayed when overriding the viewing image
    /// </summary>
    public bool EnableSaveConfirmation
    {
        get => Get(ConfigId.EnableSaveConfirmation, true);
        set => Set(ConfigId.EnableSaveConfirmation, value);
    }

    /// <summary>
    /// Gets, sets the setting to control whether the image's original modified date value is preserved on save
    /// </summary>
    public bool EnablePreserveModifiedDate
    {
        get => Get(ConfigId.EnablePreserveModifiedDate, false);
        set => Set(ConfigId.EnablePreserveModifiedDate, value);
    }

    /// <summary>
    /// Gets, sets value indicates that Save dialog should use the current image folder as initial directory.
    /// </summary>
    public bool EnableOpenSaveAsInCurrentFolder
    {
        get => Get(ConfigId.EnableOpenSaveAsInCurrentFolder, true);
        set => Set(ConfigId.EnableOpenSaveAsInCurrentFolder, value);
    }

    /// <summary>
    /// Gets, sets welcome picture value
    /// </summary>
    public bool EnableWelcomeImage
    {
        get => Get(ConfigId.EnableWelcomeImage, true);
        set => Set(ConfigId.EnableWelcomeImage, value);
    }

    /// <summary>
    /// Gets, sets the value indicates that the last seen image will be re-opened on startup.
    /// </summary>
    public bool EnableLastSeenImage
    {
        get => Get(ConfigId.EnableLastSeenImage, true);
        set => Set(ConfigId.EnableLastSeenImage, value);
    }

    /// <summary>
    /// Gets, sets the value indicating whether to use vector renderer (Svg.Skia)
    /// instead of rasterizing through Magick.NET.
    /// </summary>
    public bool EnableVectorRenderer
    {
        get => Get(ConfigId.EnableVectorRenderer, true);
        set => Set(ConfigId.EnableVectorRenderer, value);
    }

    /// <summary>
    /// Gets, sets the value indicates that HDR tone mapping is enabled when rendering image.
    /// </summary>
    public bool EnableHdrToneMapping
    {
        get => Get(ConfigId.EnableHdrToneMapping, true);
        set => Set(ConfigId.EnableHdrToneMapping, value);
    }

    /// <summary>
    /// Gets, sets the value indicates that the color profile will be always applied for all images.
    /// </summary>
    public bool EnableAlwaysApplyColorProfile
    {
        get => Get(ConfigId.EnableAlwaysApplyColorProfile, false);
        set => Set(ConfigId.EnableAlwaysApplyColorProfile, value);
    }

    /// <summary>
    /// Gets, sets the value indicates whether to show or hide the navigation buttons on viewer.
    /// </summary>
    public bool EnableNavigationButtons
    {
        get => Get(ConfigId.EnableNavigationButtons, true);
        set => Set(ConfigId.EnableNavigationButtons, value);
    }

    /// <summary>
    /// Gets, sets the value indicates whether to load photos in sub folders.
    /// </summary>
    public bool EnableSubfoldersLoading
    {
        get => Get(ConfigId.EnableSubfoldersLoading, false);
        set => Set(ConfigId.EnableSubfoldersLoading, value);
    }

    /// <summary>
    /// Gets, sets showing/loading hidden images
    /// </summary>
    public bool EnableHiddenImagesLoading
    {
        get => Get(ConfigId.EnableHiddenImagesLoading, false);
        set => Set(ConfigId.EnableHiddenImagesLoading, value);
    }

    /// <summary>
    /// Gets, sets the value indicates that images order should be grouped by directory
    /// </summary>
    public bool EnableImageFolderGrouping
    {
        get => Get(ConfigId.EnableImageFolderGrouping, false);
        set => Set(ConfigId.EnableImageFolderGrouping, value);
    }

    /// <summary>
    /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
    /// </summary>
    public bool EnableExplorerSortOrder
    {
        get => Get(ConfigId.EnableExplorerSortOrder, true);
        set => Set(ConfigId.EnableExplorerSortOrder, value);
    }

    /// <summary>
    /// Gets, sets value specifying that Window Fit mode is on
    /// </summary>
    public bool EnableWindowFit
    {
        get => Get(ConfigId.EnableWindowFit, false);
        set => Set(ConfigId.EnableWindowFit, value);
    }

    /// <summary>
    /// Gets, sets value indicates the window should be always center in Window Fit mode
    /// </summary>
    public bool EnableCenterWindowFit
    {
        get => Get(ConfigId.EnableCenterWindowFit, true);
        set => Set(ConfigId.EnableCenterWindowFit, value);
    }

    /// <summary>
    /// Displays the embedded thumbnail for RAW formats if found.
    /// </summary>
    public bool EnableOnlyLoadRawPreview
    {
        get => Get(ConfigId.EnableOnlyLoadRawPreview, false);
        set => Set(ConfigId.EnableOnlyLoadRawPreview, value);
    }

    /// <summary>
    /// Displays the embedded thumbnail for other formats if found.
    /// </summary>
    public bool EnableOnlyLoadNonRawPreview
    {
        get => Get(ConfigId.EnableOnlyLoadNonRawPreview, false);
        set => Set(ConfigId.EnableOnlyLoadNonRawPreview, value);
    }

    /// <summary>
    /// Gets, sets value indicates that image preview is shown while the image is being loaded.
    /// </summary>
    public bool EnableImagePreview
    {
        get => Get(ConfigId.EnableImagePreview, true);
        set => Set(ConfigId.EnableImagePreview, value);
    }

    /// <summary>
    /// Gets, sets value indicates that gallery can use shell for thumbnails.
    /// </summary>
    public bool EnableGalleryShellThumbnail
    {
        get => Get(ConfigId.EnableGalleryShellThumbnail, true);
        set => Set(ConfigId.EnableGalleryShellThumbnail, value);
    }

    /// <summary>
    /// Enables / Disables copy multiple files.
    /// </summary>
    public bool EnableCopyMultipleFiles
    {
        get => Get(ConfigId.EnableCopyMultipleFiles, true);
        set => Set(ConfigId.EnableCopyMultipleFiles, value);
    }

    /// <summary>
    /// Enables / Disables cut multiple files.
    /// </summary>
    public bool EnableCutMultipleFiles
    {
        get => Get(ConfigId.EnableCutMultipleFiles, true);
        set => Set(ConfigId.EnableCutMultipleFiles, value);
    }

    /// <summary>
    /// Enables / Disables the file system watcher.
    /// </summary>
    public bool EnableFileWatcher
    {
        get => Get(ConfigId.EnableFileWatcher, true);
        set => Set(ConfigId.EnableFileWatcher, value);
    }

    /// <summary>
    /// Gets, sets value indicates that ImageGlass should open the new image file added in the viewing folder.
    /// </summary>
    public bool EnableAutoOpenNewAddedImage
    {
        get => Get(ConfigId.EnableAutoOpenNewAddedImage, false);
        set => Set(ConfigId.EnableAutoOpenNewAddedImage, value);
    }

    /// <summary>
    /// Enables / Disables downloading app updates in the background so they are ready to install.
    /// </summary>
    public bool EnableAutoInstallUpdate
    {
        get => Get(ConfigId.EnableAutoInstallUpdate, false);
        set => Set(ConfigId.EnableAutoInstallUpdate, value);
    }

    /// <summary>
    /// Enables, disables debug mode.
    /// </summary>
    public bool EnableDebug
    {
        get => Get(ConfigId.EnableDebug, false);
        set => Set(ConfigId.EnableDebug, value);
    }

    #endregion // Boolean items


    #region Number items

    /// <summary>
    /// Gets, sets the version that requires to open Quick setup ImageGlass dialog.
    /// </summary>
    public double QuickSetupVersion
    {
        get => Get(ConfigId.QuickSetupVersion, 0d);
        set => Set(ConfigId.QuickSetupVersion, value);
    }

    /// <summary>
    /// Gets, sets the maximum panning margin in screen pixels beyond the image edge.
    /// </summary>
    public double PanMargin
    {
        get => Get(ConfigId.PanMargin, 0d);
        set => Set(ConfigId.PanMargin, value);
    }

    /// <summary>
    /// Gets, sets the panning speed.
    /// Value range is from 0 to 100.
    /// </summary>
    public double PanSpeed
    {
        get => Get(ConfigId.PanSpeed, 20d);
        set => Set(ConfigId.PanSpeed, value);
    }

    /// <summary>
    /// Gets, sets the zooming speed.
    /// Value range is from -500 to 500.
    /// </summary>
    public double ZoomSpeed
    {
        get => Get(ConfigId.ZoomSpeed, 0d);
        set => Set(ConfigId.ZoomSpeed, value);
    }

    /// <summary>
    /// Gets, sets slide show interval (minimum value if it's random)
    /// </summary>
    public double SlideshowInterval
    {
        get => Get(ConfigId.SlideshowInterval, 5d);
        set => Set(ConfigId.SlideshowInterval, value);
    }

    /// <summary>
    /// Gets, sets the maximum slide show interval value
    /// </summary>
    public double SlideshowIntervalTo
    {
        get => Get(ConfigId.SlideshowIntervalTo, 5d);
        set => Set(ConfigId.SlideshowIntervalTo, value);
    }

    /// <summary>
    /// Gets, sets the number of image changes to play a beep sound in slideshow mode.
    /// </summary>
    public uint SlideshowImagesToNotifySound
    {
        get => Get(ConfigId.SlideshowImagesToNotifySound, 0u);
        set => Set(ConfigId.SlideshowImagesToNotifySound, value);
    }

    /// <summary>
    /// Gets, sets value of thumbnail dimension in pixel
    /// </summary>
    public uint ThumbnailSize
    {
        get => Get(ConfigId.ThumbnailSize, 70u);
        set => Set(ConfigId.ThumbnailSize, value);
    }

    /// <summary>
    /// Gets, sets the maximum size in MB of thumbnail persistent cache.
    /// </summary>
    public uint GalleryCacheSizeInMb
    {
        get => Get(ConfigId.GalleryCacheSizeInMb, 100u);
        set => Set(ConfigId.GalleryCacheSizeInMb, value);
    }

    /// <summary>
    /// Gets, sets number of thumbnail columns displayed in vertical gallery.
    /// </summary>
    public uint GalleryColumns
    {
        get => Get(ConfigId.GalleryColumns, 3u);
        set => Set(ConfigId.GalleryColumns, value);
    }

    /// <summary>
    /// Gets, sets the maximum memory for image caching (in MB).
    /// </summary>
    public uint CacheMaxMemoryInMb
    {
        get => Get(ConfigId.CacheMaxMemoryInMb, 1000u);
        set => Set(ConfigId.CacheMaxMemoryInMb, value);
    }

    /// <summary>
    /// Gets, sets the maximum image file size (in MB) for caching.
    /// If value is <c>0</c>, the option will be ignored.
    /// </summary>
    public double CacheMaxFileSizeInMb
    {
        get => Get(ConfigId.CacheMaxFileSizeInMb, 100d);
        set => Set(ConfigId.CacheMaxFileSizeInMb, value);
    }

    /// <summary>
    /// Gets, sets the maximum image dimension for caching.
    /// If value is <c>0</c>, the option will be ignored.
    /// </summary>
    public uint CacheMaxDimension
    {
        get => Get(ConfigId.CacheMaxDimension, 8_000u);
        set => Set(ConfigId.CacheMaxDimension, value);
    }

    /// <summary>
    /// Gets, sets fixed width on zooming
    /// </summary>
    public double ZoomLockValue
    {
        get => Get(ConfigId.ZoomLockValue, 100d);
        set => Set(ConfigId.ZoomLockValue, value);
    }

    /// <summary>
    /// Gets, sets toolbar icon height
    /// </summary>
    public uint ToolbarIconHeight
    {
        get => Get(ConfigId.ToolbarIconHeight, (uint)Const.TOOLBAR_ICON_HEIGHT);
        set => Set(ConfigId.ToolbarIconHeight, value);
    }

    /// <summary>
    /// Gets, sets value of image quality for editting
    /// </summary>
    public uint ImageEditQuality
    {
        get => Get(ConfigId.ImageEditQuality, 80u);
        set => Set(ConfigId.ImageEditQuality, value);
    }

    /// <summary>
    /// Gets, sets value of duration to display the in-app message
    /// </summary>
    public int InAppMessageDuration
    {
        get => Get(ConfigId.InAppMessageDuration, 2000);
        set => Set(ConfigId.InAppMessageDuration, value);
    }

    /// <summary>
    /// Gets, sets the minimum width of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="EnableOnlyLoadRawPreview"/> or <see cref="EnableOnlyLoadNonRawPreview"/> is <c>true</c>.
    /// </summary>
    public int PreviewMinWidth
    {
        get => Get(ConfigId.PreviewMinWidth, 0);
        set => Set(ConfigId.PreviewMinWidth, value);
    }

    /// <summary>
    /// Gets, sets the minimum height of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="EnableOnlyLoadRawPreview"/>
    /// or <see cref="EnableOnlyLoadNonRawPreview"/> is <c>true</c>.
    /// </summary>
    public int PreviewMinHeight
    {
        get => Get(ConfigId.PreviewMinHeight, 0);
        set => Set(ConfigId.PreviewMinHeight, value);
    }

    #endregion // Number items


    #region String items

    /// <summary>
    /// Gets, sets the last time to check for update. Set it to <c>0</c> to disable auto-update.
    /// </summary>
    public string AutoUpdate
    {
        get => Get(ConfigId.AutoUpdate, DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).ToString());
        set => Set(ConfigId.AutoUpdate, value);
    }

    /// <summary>
    /// Gets, sets the version string the user chose to skip (e.g. "10.1.0.500").
    /// </summary>
    public string UpdateSkippedVersion
    {
        get => Get(ConfigId.UpdateSkippedVersion, string.Empty);
        set => Set(ConfigId.UpdateSkippedVersion, value);
    }


    /// <summary>
    /// Gets, sets the version of the verified update package waiting to be installed.
    /// </summary>
    public string UpdatePendingVersion
    {
        get => Get(ConfigId.UpdatePendingVersion, string.Empty);
        set => Set(ConfigId.UpdatePendingVersion, value);
    }

    /// <summary>
    /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
    /// </summary>
    public string ColorProfile
    {
        get => Get(ConfigId.ColorProfile, nameof(ColorProfileOption.CurrentMonitorProfile));
        set => Set(ConfigId.ColorProfile, value);
    }

    /// <summary>
    /// Gets, sets the absolute file path of the last seen image
    /// </summary>
    public string LastSeenImagePath
    {
        get => Get(ConfigId.LastSeenImagePath, string.Empty);
        set => Set(ConfigId.LastSeenImagePath, value);
    }

    /// <summary>
    /// Gets, sets the plugin ID to open on startup.
    /// </summary>
    public string LastOpenedTool
    {
        get => Get(ConfigId.LastOpenedTool, string.Empty);
        set => Set(ConfigId.LastOpenedTool, value);
    }

    /// <summary>
    /// Gets, sets the last view of settings window.
    /// </summary>
    public string LastOpenedSetting
    {
        get => Get(ConfigId.LastOpenedSetting, string.Empty);
        set => Set(ConfigId.LastOpenedSetting, value);
    }

    /// <summary>
    /// Gets, sets background color of of the main window
    /// </summary>
    public string BackgroundColor
    {
        get => Get(ConfigId.BackgroundColor, "#00000000");
        set => Set(ConfigId.BackgroundColor, value);
    }

    /// <summary>
    /// Gets, sets background color of slideshow
    /// </summary>
    public string SlideshowBackgroundColor
    {
        get => Get(ConfigId.SlideshowBackgroundColor, "#000000");
        set => Set(ConfigId.SlideshowBackgroundColor, value);
    }

    /// <summary>
    /// Gets, sets the theme name for dark mode.
    /// </summary>
    public string DarkTheme
    {
        get => Get(ConfigId.DarkTheme, Const.DEFAULT_THEME);
        set => Set(ConfigId.DarkTheme, value);
    }

    /// <summary>
    /// Gets, sets the theme name for light mode.
    /// </summary>
    public string LightTheme
    {
        get => Get(ConfigId.LightTheme, Const.DEFAULT_LIGHT_THEME);
        set => Set(ConfigId.LightTheme, value);
    }

    /// <summary>
    /// Gets, sets app language.
    /// </summary>
    public string Language
    {
        get => Get(ConfigId.Language, "English");
        set => Set(ConfigId.Language, value);
    }

    #endregion


    #region Enum items

    /// <summary>
    /// Gets, sets checkerboard mode of the viewer.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<CheckerboardType>))]
    public CheckerboardType CheckerboardMode
    {
        get => Get(ConfigId.CheckerboardMode, CheckerboardType.None);
        set => Set(ConfigId.CheckerboardMode, value);
    }

    /// <summary>
    /// Gets, sets how the app behaves while browsing photos.
    /// <see cref="BrowsingMode.Sequential"/> ignores navigation until the current photo
    /// is fully loaded and rendered.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<BrowsingMode>))]
    public BrowsingMode BrowsingMode
    {
        get => Get(ConfigId.BrowsingMode, BrowsingMode.Turbo);
        set => Set(ConfigId.BrowsingMode, value);
    }

    /// <summary>
    /// Gets, sets image loading order
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ImageOrderBy>))]
    public ImageOrderBy ImageLoadingOrder
    {
        get => Get(ConfigId.ImageLoadingOrder, ImageOrderBy.Name);
        set => Set(ConfigId.ImageLoadingOrder, value);
    }

    /// <summary>
    /// Gets, sets image loading order type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ImageOrderType>))]
    public ImageOrderType ImageLoadingOrderType
    {
        get => Get(ConfigId.ImageLoadingOrderType, ImageOrderType.Asc);
        set => Set(ConfigId.ImageLoadingOrderType, value);
    }

    /// <summary>
    /// Gets, sets zoom mode value
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ZoomMode>))]
    public ZoomMode ZoomMode
    {
        get => Get(ConfigId.ZoomMode, ZoomMode.AutoZoom);
        set => Set(ConfigId.ZoomMode, value);
    }

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image
    /// when the zoom factor is <c>less than or equals 100%</c>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ImageInterpolation>))]
    public ImageInterpolation ImageInterpolationScaleDown
    {
        get => Get(ConfigId.ImageInterpolationScaleDown, ImageInterpolation.LinearMipmapNearest);
        set => Set(ConfigId.ImageInterpolationScaleDown, value);
    }

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image
    /// when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ImageInterpolation>))]
    public ImageInterpolation ImageInterpolationScaleUp
    {
        get => Get(ConfigId.ImageInterpolationScaleUp, ImageInterpolation.Nearest);
        set => Set(ConfigId.ImageInterpolationScaleUp, value);
    }

    /// <summary>
    /// Gets, sets value indicates what happens after clicking Edit menu.
    /// </summary>
    public AfterEditAppAction AfterEditingAction
    {
        get => Get(ConfigId.AfterEditingAction, AfterEditAppAction.Nothing);
        set => Set(ConfigId.AfterEditingAction, value);
    }

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<BackdropStyle>))]
    public BackdropStyle WindowBackdrop
    {
        get => Get(ConfigId.WindowBackdrop, BackdropStyle.Mica);
        set => Set(ConfigId.WindowBackdrop, value);
    }

    #endregion // Enum items


    #region Array items

    /// <summary>
    /// Gets, sets the size and position of main window.
    /// </summary>
    [JsonConverter(typeof(JsonArrayToRectConverter))]
    public Rect MainWindowBounds
    {
        get => Get(ConfigId.MainWindowBounds, new Rect(200, 200, 800, 500));
        set => Set(ConfigId.MainWindowBounds, value);
    }

    /// <summary>
    /// Gets, sets the size and position of the settings window.
    /// </summary>
    [JsonConverter(typeof(JsonArrayToRectConverter))]
    public Rect SettingsWindowBounds
    {
        get => Get(ConfigId.SettingsWindowBounds, new Rect(200, 200, 900, 580));
        set => Set(ConfigId.SettingsWindowBounds, value);
    }

    /// <summary>
    /// Gets, sets zoom levels of the viewer
    /// </summary>
    [JsonConverter(typeof(JsonArrayToZoomFactorConverter))]
    public double[] ZoomLevels
    {
        get => Get(ConfigId.ZoomLevels, Array.Empty<double>());
        set => Set(ConfigId.ZoomLevels, value);
    }

    /// <summary>
    /// Gets, sets the list of apps for edit action.
    /// </summary>
    public Dictionary<string, EditingApp?> EditApps
    {
        get => Get(ConfigId.EditApps, new Dictionary<string, EditingApp?>());
        set => Set(ConfigId.EditApps, value);
    }

    /// <summary>
    /// Gets, sets the list of formats that always use native codec to decode.
    /// </summary>

    /// <summary>
    /// Gets, sets the list of supported image formats
    /// </summary>
    [JsonConverter(typeof(JsonHashSetToStringConverter))]
    public HashSet<string> FileFormats
    {
        get => Get(ConfigId.FileFormats, new HashSet<string>(DefaultFileFormats, StringComparer.OrdinalIgnoreCase));
        set => Set(ConfigId.FileFormats, value);
    }

    /// <summary>
    /// Gets, sets the tags for displaying image info.
    /// </summary>
    [JsonConverter(typeof(JsonObservableCollectionToStringConverter))]
    public ObservableCollection<string> ImageInfoTags
    {
        get => Get(ConfigId.ImageInfoTags, new ObservableCollection<string>(DefaultImageInfoTags));
        set => Set(ConfigId.ImageInfoTags, value);
    }

    /// <summary>
    /// Gets, sets hotkeys list of menu
    /// </summary>
    public Dictionary<LangId, Hotkey[]> MenuHotkeys
    {
        get => Get(ConfigId.MenuHotkeys, new Dictionary<LangId, Hotkey[]>());
        set => Set(ConfigId.MenuHotkeys, value);
    }

    /// <summary>
    /// Gets, sets mouse click actions
    /// </summary>
    public Dictionary<MouseClickEvent, SingleAction> MouseClickActions
    {
        get => Get(ConfigId.MouseClickActions, new Dictionary<MouseClickEvent, SingleAction>());
        set => Set(ConfigId.MouseClickActions, value);
    }

    /// <summary>
    /// Gets, sets mouse wheel actions
    /// </summary>
    public Dictionary<MouseWheelEvent, MouseWheelAction> MouseWheelActions
    {
        get => Get(ConfigId.MouseWheelActions, new Dictionary<MouseWheelEvent, MouseWheelAction>());
        set => Set(ConfigId.MouseWheelActions, value);
    }

    /// <summary>
    /// Gets, sets layout for FrmMain. Syntax:
    /// <c>Dictionary["ControlName", "LayoutPosition"]</c>
    /// </summary>
    public Dictionary<LayoutControl, LayoutPosition> Layout
    {
        get => Get(ConfigId.Layout, new Dictionary<LayoutControl, LayoutPosition>());
        set => Set(ConfigId.Layout, value);
    }

    /// <summary>
    /// Gets, sets per-plugin trust decisions (enabled state + pinned library hash), keyed by
    /// plugin id. A native plugin loads only when its entry is enabled and its pinned SHA-256
    /// still matches the on-disk library. See <see cref="PluginTrustInfo"/>.
    /// </summary>
    public Dictionary<string, PluginTrustInfo> PluginTrust
    {
        get => Get(ConfigId.PluginTrust, new Dictionary<string, PluginTrustInfo>(StringComparer.Ordinal));
        set => Set(ConfigId.PluginTrust, value);
    }

    /// <summary>
    /// Gets, sets the list of registered external tools.
    /// </summary>
    public ObservableCollection<ExternalTool> Tools
    {
        get => Get(ConfigId.Tools, new ObservableCollection<ExternalTool>());
        set => Set(ConfigId.Tools, value);
    }

    /// <summary>
    /// Gets, sets the config section of plugin settings.
    /// Each plugin serializes/deserializes its own <see cref="JsonElement"/> using its source-generated <see cref="JsonSerializerContext"/>.
    /// </summary>
    public Dictionary<string, JsonElement> ToolSettings
    {
        get => Get(ConfigId.ToolSettings, new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase));
        set => Set(ConfigId.ToolSettings, value);
    }

    /// <summary>
    /// Gets, sets the list of toolbar buttons
    /// </summary>
    public ObservableCollection<ToolbarItemModel> ToolbarButtons
    {
        get => Get(ConfigId.ToolbarButtons, new ObservableCollection<ToolbarItemModel>(DefaultToolbarItems));
        set => Set(ConfigId.ToolbarButtons, value);
    }

    #endregion // Array items


    #endregion // Setting items



    #region Public Methods

    /// <summary>
    /// Resets all settings to their built-in defaults by clearing the stored values
    /// (getters then fall back to their hardcoded defaults).
    /// </summary>
    public void ResetToDefault()
    {
        _values.Clear();
    }


    /// <summary>
    /// Whether a value is stored for <paramref name="configName"/>. <c>false</c> means the getter
    /// falls back to its built-in default, which is how a never-written setting is detected.
    /// </summary>
    public bool HasValue(ConfigId configName) => _values.ContainsKey(configName);


    /// <summary>
    /// Sets setting value.
    /// </summary>
    public void Set(ConfigId configName, object value)
    {
        var oldValue = Get<object?>(configName, null);
        if (Equals(value, oldValue)) return;

        _values[configName] = value;
        _ = OnPropertyChanged(value, oldValue, configName.ToString());
    }


    /// <summary>
    /// Gets setting value.
    /// </summary>
    public T Get<T>(string configName, T defaultValue)
    {
        if (!Enum.TryParse<ConfigId>(configName, out var configId)) return defaultValue;

        return Get<T>(configId, defaultValue);
    }


    /// <summary>
    /// Gets setting value as string.
    /// </summary>
    public string GetAsString(string configName)
    {
        if (!Enum.TryParse<ConfigId>(configName, out var configId)) return string.Empty;

        var value = _values.GetValueOrDefault(configId)?.ToString() ?? string.Empty;
        return value;
    }


    /// <summary>
    /// Gets setting value.
    /// </summary>
    public T Get<T>(ConfigId configName, T defaultValue)
    {
        var value = _values.GetValueOrDefault(configName) ?? defaultValue;
        return (T)value!;
    }


    /// <summary>
    /// Removes plugin-contributed <paramref name="extensions"/> from the persisted
    /// <see cref="FileFormats"/> so a plugin's formats are not left behind after it is disabled or
    /// removed. Built-in default formats are never removed (a plugin may also claim a format the app
    /// supports natively). Plugin formats stay browsable while the plugin is loaded via
    /// <see cref="Core.GetSupportedFileExtensions"/>. Returns the removed extensions; no-op if none
    /// were present.
    /// </summary>
    public IReadOnlyList<string> PurgePluginFileFormats(IEnumerable<string> extensions)
    {
        var current = FileFormats;
        var defaults = new HashSet<string>(DefaultFileFormats, StringComparer.OrdinalIgnoreCase);
        var removed = new List<string>();

        foreach (var ext in extensions)
        {
            if (string.IsNullOrWhiteSpace(ext)) continue;
            var normalized = ext.StartsWith('.') ? ext : "." + ext;
            if (defaults.Contains(normalized)) continue; // keep built-in formats
            if (current.Contains(normalized)) removed.Add(normalized);
        }

        if (removed.Count == 0) return [];

        var next = new HashSet<string>(current, StringComparer.OrdinalIgnoreCase);
        foreach (var ext in removed) next.Remove(ext);

        FileFormats = next;
        return removed;
    }

    #endregion // Public Methods



}



