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
namespace ImageGlass.Common.Localization;

public enum LangId
{
    #region General
    _OK,
    _Cancel,
    _Apply,
    _Close,
    _Yes,
    _No,
    _LearnMore,
    _Continue,
    _Quit,
    _Back,
    _Next,
    _Save,
    _Warning,
    _Copy,
    _Browse,
    _Reset,
    _ResetToDefault,
    _CheckForUpdate,
    _Update,
    _Download,
    _RestartNow,
    _Website,
    _TypeToFilter,
    _Delete,
    _Add,
    _Edit,
    _ID,
    _Name,
    _Hotkeys,
    _Executable,
    _Argument,
    _CommandPreview,
    _FileExtension,
    _Codec,
    _Decoder,
    _Encoder,
    _Empty,
    _Separator,
    _Icon,
    _Description,
    _Type,
    _Version,
    _Author,
    _View,
    _GetHelp,
    _GetMoreTools,
    _Start,
    _Register,
    _Unregister,
    _ClearCache,
    _ClearCache_Success,
    _ClearCache_Error,

    _UnhandledException,
    _UnhandledException_Description,
    _DoNotShowThisMessageAgain,
    _IncompatibleConfig,
    _IncompatibleConfig_Description,
    _IncompatibleConfig_BackupNote,
    _CreatingFile,
    _CreatingFileError,
    _NotSupported,

    _InvalidAction,
    _InvalidAction_Transformation,

    _UserAction_Win32ExeError,

    // Gallery tooltip
    _Metadata_FileSize,
    _Metadata_FileLastWriteTime,
    _Metadata_FrameCount,
    _Metadata_ExifRatingPercent,
    _Metadata_ColorSpace,
    _Metadata_ColorProfile,
    _Metadata_ExifDateTime,
    _Metadata_ExifDateTimeOriginal,

    // image info
    _ImageInfo_ListCount,
    _ImageInfo_FrameCount,

    // layout position
    _Position_Left,
    _Position_Right,
    _Position_Top,
    _Position_Bottom,

    _Validation_Required,
    _Validation_RegexPattern,
    _Validation_IntValueOnly,
    _Validation_UnsignedIntValueOnly,
    _Validation_FloatValueOnly,
    _Validation_UnsignedFloatValueOnly,
    _Validation_FileNameValueOnly,
    _Validation_FilePathValueOnly,
    _Validation_FileExtensionValueOnly,
    _Validation_FileExtensionsValueOnly,


    // main window
    _PicMain_ErrorText,
    _OpenFileDialog,
    _Loading,
    _ReachedFirstImage,
    _ReachedLastImage,
    _SwitchedToNextFolder,
    _SwitchedToPreviousFolder,
    _ClipboardImage,

    // about
    _Slogan,
    _AboutVersion,
    _License,
    _Privacy,
    _Homepage,
    _Credits,
    _Donate,

    // slideshow
    _PauseSlideshow,
    _ResumeSlideshow,
    _MnuPauseResumeSlideshow,
    _MnuToggleCountdown,
    _MnuExitSlideshow,

    // export frames
    _Title,
    _FolderPickerTitle,
    _Exporting,
    _ExportDone,
    _OpenOutputFolder,

    #endregion // General


    #region Enums

    ImageOrderBy_Name,
    ImageOrderBy_Random,
    ImageOrderBy_FileSize,
    ImageOrderBy_Extension,
    ImageOrderBy_DateCreated,
    ImageOrderBy_DateAccessed,
    ImageOrderBy_DateModified,
    ImageOrderBy_ExifDateTaken,
    ImageOrderBy_ExifRating,

    ImageOrderType_Asc,
    ImageOrderType_Desc,

    BrowsingMode_Turbo,
    BrowsingMode_Turbo_Description,
    BrowsingMode_Sequential,
    BrowsingMode_Sequential_Description,

    AfterEditAppAction_Nothing,
    AfterEditAppAction_Minimize,
    AfterEditAppAction_Close,

    ColorProfileOption_None,
    ColorProfileOption_CurrentMonitorProfile,
    ColorProfileOption_Custom,

    BackdropStyle_None,

    MouseWheelEvent_Scroll,
    MouseWheelEvent_CtrlAndScroll,
    MouseWheelEvent_ShiftAndScroll,
    MouseWheelEvent_AltAndScroll,

    MouseWheelAction_DoNothing,
    MouseWheelAction_Zoom,
    MouseWheelAction_PanVertically,
    MouseWheelAction_PanHorizontally,
    MouseWheelAction_BrowseImages,

    MouseClickEvent_LeftClick,
    MouseClickEvent_LeftDoubleClick,
    MouseClickEvent_RightClick,
    MouseClickEvent_WheelClick,
    MouseClickEvent_XButton1Click,
    MouseClickEvent_XButton2Click,

    // values match the CheckerboardType enum members
    CheckerboardType_None,
    CheckerboardType_Client,
    CheckerboardType_Image,

    #endregion // Enums


    #region Main Window
    Menu_MnuMain,
    Menu_MnuToolbarOverflow,


    #region Main Window > Main Menu

    #region Main Menu > File
    Menu_MnuFile,
    Menu_MnuOpenFile,
    Menu_MnuNewWindow,
    Menu_MnuNewWindow_Error,

    Menu_MnuSave,
    Menu_MnuSave_Confirm,
    Menu_MnuSave_ConfirmDescription,
    Menu_MnuSave_Saving,
    Menu_MnuSave_Success,
    Menu_MnuSave_Error,
    Menu_MnuSaveAs,
    Menu_MnuExportFrames,

    Menu_MnuOpenWith,
    Menu_MnuEdit,
    Menu_MnuEdit_AppNotFound,
    Menu_MnuPrint,
    Menu_MnuPrint_Error,
    Menu_MnuShare,
    Menu_MnuShare_Error,
    Menu_MnuOpenLocation,

    Menu_MnuRename,
    Menu_MnuRename_Description,
    Menu_MnuMoveToRecycleBin,
    Menu_MnuMoveToRecycleBin_Description,
    Menu_MnuDeleteFromHardDisk,
    Menu_MnuDeleteFromHardDisk_Description,

    #endregion // Main Menu > File


    #region Main Menu > Navigation
    Menu_MnuNavigation,
    Menu_MnuViewNext,
    Menu_MnuViewPrevious,

    Menu_MnuGoTo,
    Menu_MnuGoTo_Description,
    Menu_MnuGoToFirst,
    Menu_MnuGoToLast,

    Menu_MnuViewNextFrame,
    Menu_MnuViewPreviousFrame,
    Menu_MnuViewFirstFrame,
    Menu_MnuViewLastFrame,
    #endregion // Main Menu > Navigation


    #region Main Menu > Zoom
    Menu_MnuZoom,
    Menu_MnuZoomIn,
    Menu_MnuZoomOut,
    Menu_MnuCustomZoom,
    Menu_MnuCustomZoom_Description,
    Menu_MnuScaleToFit,
    Menu_MnuScaleToFill,
    Menu_MnuActualSize,
    Menu_MnuLockZoom,
    Menu_MnuAutoZoom,
    Menu_MnuScaleToWidth,
    Menu_MnuScaleToHeight,
    #endregion // Main Menu > Zoom


    #region Main Menu > Panning
    Menu_MnuPanning,

    Menu_MnuPanLeft,
    Menu_MnuPanRight,
    Menu_MnuPanUp,
    Menu_MnuPanDown,

    Menu_MnuPanToLeftSide,
    Menu_MnuPanToRightSide,
    Menu_MnuPanToTop,
    Menu_MnuPanToBottom,
    #endregion // Main Menu > Panning


    #region Main Menu > Image
    Menu_MnuImage,

    Menu_MnuRefresh,
    Menu_MnuReload,
    Menu_MnuReloadImageList,
    Menu_MnuUnload,

    Menu_MnuViewChannels,
    Menu_MnuLoadingOrders,

    Menu_MnuInvertColors,
    Menu_MnuToggleImageAnimation,

    Menu_MnuRotateLeft,
    Menu_MnuRotateRight,
    Menu_MnuFlipHorizontal,
    Menu_MnuFlipVertical,

    Menu_MnuSetDesktopBackground,
    Menu_MnuSetDesktopBackground_Error,
    Menu_MnuSetDesktopBackground_Success,
    Menu_MnuSetLockScreen,
    Menu_MnuSetLockScreen_Error,
    Menu_MnuSetLockScreen_Success,

    Menu_MnuImageProperties,
    #endregion // Main Menu > Image


    #region Main Menu > Clipboard
    Menu_MnuClipboard,
    Menu_MnuCopyFile,
    Menu_MnuCopyFile_Success,
    Menu_MnuCopyImagePixels,
    Menu_MnuCopyImagePixels_Copying,
    Menu_MnuCopyImagePixels_Success,
    Menu_MnuCutFile,
    Menu_MnuCutFile_Success,
    Menu_MnuCopyPath,
    Menu_MnuCopyPath_Success,
    Menu_MnuPasteImage,
    Menu_MnuClearClipboard,
    Menu_MnuClearClipboard_Success,
    #endregion // Main Menu > Clipboard


    Menu_MnuWindowFit,
    Menu_MnuFullScreen,
    Menu_MnuFrameless,
    Menu_MnuFrameless_EnableDescription,
    Menu_MnuSlideshow,


    #region Main Menu > Layout
    Menu_MnuLayout,
    Menu_MnuToggleToolbar,
    Menu_MnuToggleGallery,
    Menu_MnuToggleCheckerboard,
    Menu_MnuToggleTopMost,
    Menu_MnuToggleTopMost_Enable,
    Menu_MnuToggleTopMost_Disable,
    Menu_MnuChangeBackgroundColor,
    #endregion // Main Menu > Layout

    #region Main Menu > Tools
    Menu_MnuTools,
    Menu_MnuColorPicker,
    Menu_MnuCropTool,
    Menu_MnuResizeTool,
    Menu_MnuFrameNav,
    Menu_MnuHdrToneMapper,
    Menu_MnuToolsSettings,

    Menu_MnuLosslessCompression,
    Menu_MnuLosslessCompression_Confirm,
    Menu_MnuLosslessCompression_Description,
    Menu_MnuLosslessCompression_Compressing,
    Menu_MnuLosslessCompression_Done,
    Menu_MnuLosslessCompression_Error,
    #endregion // Main Menu > Tools

    Menu_MnuSettings,

    #region Main Menu > Help
    Menu_MnuHelp,
    Menu_MnuAbout,

    Menu_MnuUpgradeLicense,
    Menu_MnuUpgradeLicense_Description,
    Menu_MnuUpgradeLicense_ImportLicense,
    Menu_MnuUpgradeLicense_RetrieveFromEmail,
    Menu_MnuUpgradeLicense_BuyOnline,
    Menu_MnuUpgradeLicense_ViewFeatures,
    Menu_MnuUpgradeLicense_OutOfScope,

    Menu_MnuManageLicense,
    Menu_MnuManageLicense_ChangeLicense,
    Menu_MnuManageLicense_UpgradePlan,
    Menu_MnuManageLicense_LicensedTo,
    Menu_MnuManageLicense_Plan,
    Menu_MnuManageLicense_Seats,
    Menu_MnuManageLicense_Expires,
    Menu_MnuManageLicense_Perpetual,
    Menu_MnuManageLicense_LicenseId,
    Menu_MnuManageLicense_Source,
    Menu_MnuManageLicense_ImportSuccess,
    Menu_MnuManageLicense_ImportFailed,
    Menu_MnuManageLicense_ImportExpired,
    Menu_MnuManageLicense_LicenseFileType,
    Menu_MnuManageLicense_ExportLicense,
    Menu_MnuManageLicense_ExportNote,
    Menu_MnuManageLicense_ExportSuccess,
    Menu_MnuManageLicense_ExportFailed,

    Menu_MnuQuickSetup,
    Menu_MnuReportIssue,
    Menu_MnuCheckForUpdate_NewVersion,
    Menu_MnuCheckForUpdate_NoUpdate,
    Menu_MnuCheckForUpdate_Checking,
    Menu_MnuCheckForUpdate_Failed,
    Menu_MnuCheckForUpdate_SkipVersion,
    Menu_MnuCheckForUpdate_CurrentVersion,
    Menu_MnuCheckForUpdate_LatestVersion,
    Menu_MnuCheckForUpdate_PublishedDate,
    Menu_MnuCheckForUpdate_ReadyToInstall,
    Menu_MnuCheckForUpdate_Downloading,
    Menu_MnuInstallUpdate,
    Menu_MnuInstallUpdate_Confirm,

    Menu_MnuSetDefaultPhotoViewer,
    Menu_MnuSetDefaultPhotoViewer_Success,
    Menu_MnuSetDefaultPhotoViewer_Error,

    Menu_MnuRemoveDefaultPhotoViewer,
    Menu_MnuRemoveDefaultPhotoViewer_Success,
    Menu_MnuRemoveDefaultPhotoViewer_Error,
    #endregion // Main Menu > Help

    Menu_MnuExit,


    #endregion // Main Window > Main Menu

    #endregion // Main Window


    #region Settings

    Settings_ResetSettings,
    Settings_UnmanagedSettingReminder,
    Settings_SearchPlaceholder,
    Settings_AdminLockWarning,
    Settings_ProFeatureHint,

    #region Settings > Navbar
    Settings_Nav_General,
    Settings_Nav_Image,
    Settings_Nav_Slideshow,
    Settings_Nav_Edit,
    Settings_Nav_Viewer,
    Settings_Nav_Toolbar,
    Settings_Nav_Gallery,
    Settings_Nav_Layout,
    Settings_Nav_Mouse,
    Settings_Nav_Keyboard,
    Settings_Nav_FileTypeAssociations,
    Settings_Nav_Tools,
    Settings_Nav_Plugins,
    Settings_Nav_Language,
    Settings_Nav_Appearance,
    #endregion // Settings > Navbar


    #region Settings > Tab General
    // General > General
    Settings_StartupDir,
    Settings_ConfigDir,
    Settings_UserConfigFile,

    // General > Startup
    Settings_Startup,
    Settings_EnableWelcomeImage,
    Settings_EnableLastSeenImage,


    // General > Real-time update
    Settings_EnableFileWatcher,
    Settings_EnableAutoOpenNewAddedImage,

    // General > App update
    Settings_AppUpdate,
    Settings_AutoUpdate_Description,
    Settings_SeeWhatIsSent,
    Settings_EnableAutoInstallUpdate,

    // General > Others
    Settings_Others,
    Settings_AutoUpdate,
    Settings_EnableMultiInstances,
    Settings_ShowAppIcon,
    Settings_InAppMessageDuration,
    Settings_ImageInfoTags,
    Settings_AvailableImageInfoTags,
    #endregion // Settings > Tab General


    #region Settings > Tab Image
    // Image > Browsing
    Settings_Browsing,
    Settings_ImageLoadingOrder,
    Settings_EnableExplorerSortOrder,
    Settings_EnableSubfoldersLoading,
    Settings_EnableImageFolderGrouping,
    Settings_EnableHiddenImagesLoading,
    Settings_EnableLoopBackNavigation,
    Settings_EnableAutoSwitchSiblingDir,
    Settings_BrowsingMode,
    Settings_EnableImagePreview,

    Settings_ImagePreview,
    Settings_EnableOnlyLoadRawPreview,
    Settings_EnableOnlyLoadNonRawPreview,
    Settings_MinEmbeddedThumbnailSize,
    Settings_MinEmbeddedThumbnailSize_Width,
    Settings_MinEmbeddedThumbnailSize_Height,

    // Image > File watcher
    Settings_FileWatcher,

    // Image > Caching
    Settings_Caching,
    Settings_ImageBoosterCacheMaxMemoryInMb,
    Settings_ImageBoosterCacheMaxDimension,
    Settings_ImageBoosterCacheMaxFileSizeInMb,

    // Image > Color management
    Settings_ColorManagement,
    Settings_EnableHdrToneMapping,
    Settings_EnableAlwaysApplyColorProfile,
    Settings_ColorProfile,
    Settings_CurrentMonitorProfile_Description,
    #endregion // Settings > Tab Image


    #region Settings > Tab Slideshow
    // Slideshow > Appearance
    Settings_Slideshow_Appearance,
    Settings_EnableSlideshowCountdown,
    Settings_EnableFullscreenSlideshow,
    Settings_SlideshowBackgroundColor,

    // Slideshow > Playback
    Settings_Slideshow_Playback,
    Settings_EnableLoopSlideshow,
    Settings_EnableSlideshowRandomInterval,
    Settings_SlideshowInterval,
    Settings_SlideshowInterval_From,
    Settings_SlideshowInterval_To,

    Settings_SlideshowImagesToNotifySound,
    #endregion // Settings > Tab Slideshow


    #region Settings > Tab Edit
    // Edit > Saving
    Settings_Edit_Saving,
    Settings_EnableDeleteConfirmation,
    Settings_EnableSaveConfirmation,
    Settings_EnablePreserveModifiedDate,
    Settings_EnableOpenSaveAsInCurrentFolder,
    Settings_ImageEditQuality,

    // Edit > Clipboard
    Settings_Clipboard,
    Settings_EnableCopyMultipleFiles,
    Settings_EnableCutMultipleFiles,

    // Edit > Image editing apps
    Settings_AfterEditingAction,
    Settings_EditApps,
    Settings_EditApps_AppName,
    Settings_EditAppDialog_AddApp,
    Settings_EditAppDialog_EditApp,
    #endregion // Settings > Tab Edit


    #region Settings > Tab Layout
    // Layout > Window
    Settings_Window,

    // Layout > Controls
    Settings_Controls,
    Settings_Layout_ArrangeHint,
    Settings_Layout_Viewer,

    // Layout > Layout
    Settings_Layout_Toolbar,
    Settings_Layout_Gallery,
    Settings_Layout_ToolbarPosition,
    Settings_Layout_GalleryPosition,
    #endregion // Settings > Tab Layout


    #region Settings > Tab Viewer
    // Viewer > Appearance
    Settings_Appearance,
    Settings_EnableNavigationButtons,
    Settings_EnableCenterWindowFit,
    Settings_EnableVectorRenderer,
    Settings_CheckerboardMode,

    // Viewer > Panning
    Settings_Panning,
    Settings_EnableFreePan,
    Settings_PanMargin,
    Settings_PanSpeed,

    // Viewer > Zooming
    Settings_Zooming,
    Settings_ImageInterpolation,
    Settings_ImageInterpolation_ScaleDown,
    Settings_ImageInterpolation_ScaleUp,
    Settings_ZoomSpeed,
    Settings_ZoomLevels,
    Settings_UseSmoothZooming,
    Settings_LoadDefaultZoomLevels,
    #endregion // Settings > Tab Viewer


    #region Settings > Tab Toolbar
    // Toolbar > Toolbar
    Settings_Toolbar_ShowToolbarInFullscreen,
    Settings_Toolbar_ToolbarIconHeight,

    Settings_Toolbar_AddNewButton,
    Settings_Toolbar_EditButton,

    Settings_Toolbar_ToolbarButtons,
    Settings_Toolbar_AddCustomButton,
    Settings_Toolbar_AvailableButtons,
    Settings_Toolbar_CurrentButtons,
    Settings_Toolbar_Errors_ButtonIdDuplicated,

    Settings_Toolbar_ButtonText,
    Settings_Toolbar_ShowButtonText,
    Settings_Toolbar_AlignRight,
    Settings_Toolbar_CustomIcon,
    Settings_Toolbar_ConfigBinding,
    Settings_Toolbar_ConfigBindingName,
    Settings_Toolbar_ConfigBindingValue,
    Settings_Toolbar_RecordHotkeyHint,
    Settings_Toolbar_BuiltInReadonly,

    Settings_Toolbar_ArrangeHint,
    #endregion // Settings > Tab Toolbar


    #region Settings > Tab Gallery
    // Gallery > Gallery
    Settings_ShowGalleryInFullscreen,
    Settings_ShowGalleryFileName,
    Settings_EnableGalleryShellThumbnail,
    Settings_ThumbnailSize,
    Settings_GalleryCacheSizeInMb,
    Settings_GalleryColumns,
    #endregion // Settings > Tab Gallery


    #region Settings > Tab Mouse
    // Mouse > Mouse wheel action
    Settings_MouseWheelAction,
    // Mouse > Mouse click action
    Settings_MouseClickAction,
    #endregion // Settings > Tab Mouse


    #region Settings > Tab Keyboard
    Settings_Keyboard_MenuHotkeys,
    Settings_Keyboard_Action,
    Settings_Keyboard_NoResults,
    Settings_Keyboard_EditTitle,
    Settings_Keyboard_Conflict,
    #endregion // Settings > Tab Mouse & Keyboard


    #region Settings > Tab File type associations
    // File type associations > File extension icons
    Settings_FileExtensionIcons,
    Settings_FileExtensionIcons_Description,
    Settings_OpenExtensionIconFolder,
    Settings_GetExtensionIconPacks,

    // File type associations > Default photo viewer
    Settings_DefaultPhotoViewer,
    Settings_DefaultPhotoViewer_Description,
    Settings_DefaultPhotoViewer_ScopePerMachine,
    Settings_DefaultPhotoViewer_ScopePerUser,
    Settings_OpenDefaultAppsSetting,

    // File type associations > File formats
    Settings_FileFormats,
    Settings_TotalSupportedFormats,
    Settings_AddNewFileExtension,

    #endregion // Settings > Tab File type associations


    #region Settings > Tab Tools
    // Tools > Tools
    Settings_Tools_AddNewTool,
    Settings_Tools_EditTool,
    Settings_Tools_ToolLaunchFailed,
    Settings_Tools_ToolLaunchFailed_Description,
    Settings_Tools_Integrated,
    Settings_Tools_IntegratedWith,
    Settings_Tools_Errors_ToolIdDuplicated,
    #endregion // Settings > Tab Tools


    #region Settings > Tab Plugins
    Settings_Plugins_OpenPluginFolder,
    Settings_Plugins_GetMorePlugins,
    Settings_Plugins_SupportedExtensions,
    Settings_Plugins_ViewMetadata,
    Settings_Plugins_FolderPath,
    Settings_Plugins_InstallSuccess,
    Settings_Plugins_DeleteConfirm,
    Settings_Plugins_Status,
    Settings_Plugins_Enable,
    Settings_Plugins_TrustAndEnable,
    Settings_Plugins_StatusEnabled,
    Settings_Plugins_StatusDisabled,
    Settings_Plugins_StatusUntrusted,
    Settings_Plugins_StatusChanged,
    Settings_Plugins_StatusNotLoaded,
    Settings_Plugins_TrustTitle,
    Settings_Plugins_TrustPrompt,
    Settings_Plugins_TrustChangedWarning,
    Settings_Plugins_EnableToLoad,
    Settings_Plugins_ExtensionsHint,
    Settings_Plugins_FormatsAfterEnable,
    Settings_Plugins_NoFormats,
    #endregion // Settings > Tab Plugins


    #region Settings > Tab Language
    // Language > Language
    Settings_DisplayLanguage,
    Settings_Refresh,
    Settings_InstallNewLanguagePack,
    Settings_GetMoreLanguagePacks,
    Settings_ExportLanguagePack,
    Settings_Contributors,
    #endregion // Settings > Tab Language


    #region Settings > Tab Appearance
    // Appearance > Appearance
    Settings_WindowBackdrop,
    Settings_BackgroundColor,

    // Appearance > Theme
    Settings_Theme,
    Settings_DarkTheme,
    Settings_LightTheme,
    Settings_Theme_OpenThemeFolder,
    Settings_Theme_GetMoreThemes,
    Settings_Theme_InstallTheme,
    _IncompatibleTheme,
    _IncompatibleTheme_Description,
    _IncompatibleLanguage,
    _IncompatibleLanguage_Description,

    Settings_UseThemeForDarkMode,
    Settings_UseThemeForLightMode,
    #endregion // Settings > Tab Appearance

    #endregion // Settings


    #region Tool: Crop
    Tool_Crop_LblAspectRatio,
    Tool_Crop_LblLocation,
    Tool_Crop_LblSize,

    Tool_Crop_SelectionAspectRatio_FreeRatio,
    Tool_Crop_SelectionAspectRatio_Custom,
    Tool_Crop_SelectionAspectRatio_Original,

    Tool_Crop_BtnReset,
    Tool_Crop_BtnSave,
    Tool_Crop_BtnSaveAs,
    Tool_Crop_BtnCrop,
    Tool_Crop_BtnCopy,


    // Crop settings
    Tool_Crop_Title,
    Tool_Crop_ChkCloseToolAfterSaving,
    Tool_Crop_LblDefaultSelection,
    Tool_Crop_ChkAutoCenterSelection,

    Tool_Crop_DefaultSelectionType_UseTheLastSelection,
    Tool_Crop_DefaultSelectionType_SelectNone,
    Tool_Crop_DefaultSelectionType_SelectX,
    Tool_Crop_DefaultSelectionType_SelectAll,
    Tool_Crop_DefaultSelectionType_CustomArea,
    #endregion // Tool: Crop


    #region Tool: HDR tone mapper
    Tool_Hdr_LblMode,
    Tool_Hdr_LblExposure,
    Tool_Hdr_LblWhitePoint,
    Tool_Hdr_LblHighlightCompression,
    Tool_Hdr_LblSaturation,
    Tool_Hdr_BtnReset,
    #endregion // Tool: HDR tone mapper


    #region Tool: Color picker

    // Color picker settings
    Tool_ColorPicker_Title,
    Tool_ColorPicker_ChkShowRgbA,
    Tool_ColorPicker_ChkShowHexA,
    Tool_ColorPicker_ChkShowHslA,
    Tool_ColorPicker_ChkShowHsvA,
    Tool_ColorPicker_ChkShowCmykA,
    Tool_ColorPicker_ChkShowCIELabA,
    #endregion // Tool: Color picker


    #region Tool: Resizer
    Tool_Resizer_RadResizeByPixels,
    Tool_Resizer_RadResizeByPercentage,
    Tool_Resizer_ChkKeepRatio,
    Tool_Resizer_LblResample,
    Tool_Resizer_LblCurrentSize,
    Tool_Resizer_LblNewSize,
    #endregion // Tool: Resizer


    #region Quick setup
    QuickSetup_Title,
    QuickSetup_StepInfo,
    QuickSetup_SkipAndLaunch,

    QuickSetup_SelectLanguage,
    QuickSetup_SeeWhatNew,
    QuickSetup_SelectProfile,
    QuickSetup_StandardUser,
    QuickSetup_ProfessionalUser,
    QuickSetup_SettingsWillBeApplied,
    QuickSetup_SettingProfileDescription,
    QuickSetup_RegisterImageFormats,

    QuickSetup_ConfirmCloseProcess,
    QuickSetup_ConfirmCloseProcess_Description,
    #endregion // Quick setup


}

