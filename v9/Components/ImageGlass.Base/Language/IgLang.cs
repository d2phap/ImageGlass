/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
namespace ImageGlass.Base;


/// <summary>
/// ImageGlass language pack (.iglang.json)
/// </summary>
public class IgLang : Dictionary<string, string>
{
    /// <summary>
    /// Language file path
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Language information
    /// </summary>
    public IgLangMetadata Metadata { get; set; } = new();


    /// <summary>
    /// Initializes default language
    /// </summary>
    public IgLang()
    {
        InitDefaultLanguage();
    }

    /// <summary>
    /// Initializes language with filename
    /// </summary>
    /// <param name="fileName">.iglang.json file</param>
    /// <param name="dirPath">The directory path contains language file (for relative filename)</param>
    public IgLang(string fileName, string dirPath = "")
    {
        InitDefaultLanguage();

        FileName = Path.Combine(dirPath, fileName);

        if (File.Exists(FileName))
        {
            ReadFromFile();
        }
    }


    /// <summary>
    /// Loads language strings from JSON file
    /// </summary>
    public void ReadFromFile()
    {
        var model = Helpers.ReadJson<IgLangJsonModel>(FileName);
        if (model == null) return;

        Metadata = model._Metadata;

        // import language items
        foreach (var item in model.Items)
        {
            if (!string.IsNullOrEmpty(item.Value))
            {
                this[item.Key] = item.Value;
            }
        }
    }


    /// <summary>
    /// Saves current language to JSON file
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveAsFile(string filePath)
    {
        var model = new IgLangJsonModel(Metadata, this);

        Helpers.WriteJson(filePath, model);
    }


    /// <summary>
    /// Initialize default language
    /// </summary>
    public void InitDefaultLanguage()
    {
        //Add("_IncompatibleConfigs", "Some settings are not compatible with your ImageGlass {0}. It's recommended to update them before continuing.\r\n\n- Click Yes to learn about the changes.\r\n- Click No to launch ImageGlass with default settings."); //v7.5

        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Name)}", "Name (default)"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Length)}", "Length"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.CreationTime)}", "Creation time"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.LastAccessTime)}", "Last access time"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.LastWriteTime)}", "Last write time"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Extension)}", "Extension"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Rating)}", "Rating"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Random)}", "Random"); //v8.0

        Add($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Asc)}", "Ascending");  //v8.0
        Add($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Desc)}", "Descending");  //v8.0

        //Add("_.AfterOpeningEditAppAction._Nothing", "Nothing"); //v8.0
        //Add("_.AfterOpeningEditAppAction._Minimize", "Minimize"); //v8.0
        //Add("_.AfterOpeningEditAppAction._Close", "Close"); //v8.0

        #region FrmMain

        #region Main menu

        #region File
        Add("FrmMain.MnuFile", "File"); //v7.0
        Add("FrmMain.MnuOpenFile", "Open file…"); //v3.0
        Add("FrmMain.MnuOpenImageData", "Open image data from clipboard"); //v3.0
        Add("FrmMain.MnuNewWindow", "Open new window"); //v7.0
        Add("FrmMain.MnuNewWindow._Error", "Cannot open new window because only one instance allowed"); //v7.0
        Add("FrmMain.MnuSave", "Save image"); //v8.1
        Add("FrmMain.MnuSaveAs", "Save image as…"); //v3.0
        Add("FrmMain.MnuRefresh", "Refresh"); //v3.0
        Add("FrmMain.MnuReload", "Reload image"); //v5.5
        Add("FrmMain.MnuReloadImageList", "Reload image list"); //v7.0
        Add("FrmMain.MnuOpenWith", "Open with…"); //v7.6
        Add("FrmMain.MnuEdit", "Edit image {0}…"); //v3.0, updated 4.0
        Add("FrmMain.MnuPrint", "Print…"); //v3.0
        #endregion

        #region Navigation
        Add("FrmMain.MnuNavigation", "Navigation"); //v3.0
        Add("FrmMain.MnuViewNext", "View next image"); //v3.0
        Add("FrmMain.MnuViewPrevious", "View previous image"); //v3.0

        Add("FrmMain.MnuGoTo", "Go to…"); //v3.0
        Add("FrmMain.MnuGoToFirst", "Go to the first image"); //v3.0
        Add("FrmMain.MnuGoToLast", "Go to the last image"); //v3.0

        Add("FrmMain.MnuViewNextFrame", "View next frame"); //v7.5
        Add("FrmMain.MnuViewPreviousFrame", "View previous frame"); //v7.5
        Add("FrmMain.MnuViewFirstFrame", "View the first frame"); //v7.5
        Add("FrmMain.MnuViewLastFrame", "View the last last"); //v7.5
        #endregion

        #region Zoom
        Add("FrmMain.MnuZoom", "Zoom"); //v7.0
        Add("FrmMain.MnuZoomIn", "Zoom in"); //v3.0
        Add("FrmMain.MnuZoomOut", "Zoom out"); //v3.0
        Add("FrmMain.MnuCustomZoom", "Custom zoom…"); // v8.3
        Add("FrmMain.MnuCustomZoom._Text", "Enter a new zoom value"); // v8.3
        Add("FrmMain.MnuScaleToFit", "Scale to fit"); //v3.5
        Add("FrmMain.MnuScaleToFill", "Scale to fill"); //v7.5
        Add("FrmMain.MnuActualSize", "Actual size"); //v3.0
        Add("FrmMain.MnuLockZoom", "Lock zoom ratio"); //v3.0
        Add("FrmMain.MnuAutoZoom", "Auto zoom"); //v5.5
        Add("FrmMain.MnuScaleToWidth", "Scale to width"); //v3.0
        Add("FrmMain.MnuScaleToHeight", "Scale to height"); //v3.0
        #endregion

        #region Image
        Add("FrmMain.MnuImage", "Image"); //v7.0
        Add("FrmMain.MnuToggleImageFocus", "Toggle Image focus mode"); // v9.0
        Add("FrmMain.MnuToggleImageFocus._Enable", "Enabled Image focus mode."); // v9.0
        Add("FrmMain.MnuToggleImageFocus._Disable", "Disabled Image focus mode."); // v9.0

        Add("FrmMain.MnuViewChannels", "View channels"); //v7.0
        Add("FrmMain.MnuViewChannels._All", "All"); //v7.0
        Add("FrmMain.MnuViewChannels._Red", "Red"); //v7.0
        Add("FrmMain.MnuViewChannels._Green", "Green"); //v7.0
        Add("FrmMain.MnuViewChannels._Blue", "Blue"); //v7.0
        Add("FrmMain.MnuViewChannels._Black", "Black"); //v7.0
        Add("FrmMain.MnuViewChannels._Alpha", "Alpha"); //v7.0

        Add("FrmMain.MnuLoadingOrders", "Loading orders"); //v8.0

        Add("FrmMain.MnuRotateLeft", "Rotate left"); //v7.5
        Add("FrmMain.MnuRotateRight", "Rotate right"); //v7.5
        Add("FrmMain.MnuFlipHorizontal", "Flip Horizontal"); // V6.0
        Add("FrmMain.MnuFlipVertical", "Flip Vertical"); // V6.0
        Add("FrmMain.MnuRename", "Rename image…"); //v3.0
        Add("FrmMain.MnuMoveToRecycleBin", "Move to recycle bin"); //v3.0
        Add("FrmMain.MnuDeleteFromHardDisk", "Delete from hard disk"); //v3.0
        Add("FrmMain.MnuExtractFrames", "Extract image frames ({0})…"); //v7.5
        Add("FrmMain.MnuStartStopAnimating", "Start / Stop animating image"); //v3.0
        Add("FrmMain.MnuSetDesktopBackground", "Set as Desktop background"); //v3.0
        Add("FrmMain.MnuSetLockScreen", "Set as Lock screen image"); // V6.0
        Add("FrmMain.MnuOpenLocation", "Open image location"); //v3.0
        Add("FrmMain.MnuImageProperties", "Image properties"); //v3.0
        #endregion

        #region Clipboard
        Add("FrmMain.MnuClipboard", "Clipboard"); //v3.0
        Add("FrmMain.MnuCopy", "Copy"); //v3.0
        Add("FrmMain.MnuCopyImageData", "Copy image data"); //v5.0
        Add("FrmMain.MnuCut", "Cut"); //v3.0
        Add("FrmMain.MnuCopyPath", "Copy image path"); //v3.0
        Add("FrmMain.MnuClearClipboard", "Clear clipboard"); //v3.0
        #endregion

        Add("FrmMain.MnuWindowFit", "Window fit"); //v7.5
        Add("FrmMain.MnuFullScreen", "Full screen"); //v3.0
        Add("FrmMain.MnuFrameless", "Frameless"); //v7.5

        #region Slideshow
        Add("FrmMain.MnuSlideshow", "Slideshow"); //v3.0
        Add("FrmMain.MnuStartSlideshow", "Start slideshow"); //v3.0
        Add("FrmMain.MnuPauseResumeSlideshow", "Pause / Resume slideshow"); //v3.0
        Add("FrmMain.MnuExitSlideshow", "Exit slideshow"); //v3.0
        #endregion

        #region Layout
        Add("FrmMain.MnuLayout", "Layout"); //v3.0
        Add("FrmMain.MnuToggleToolbar", "Toolbar"); //v3.0
        Add("FrmMain.MnuToggleThumbnails", "Thumbnail panel"); //v3.0
        Add("FrmMain.MnuToggleCheckerboard", "Checkerboard background"); //v3.0, updated v5.0
        Add("FrmMain.MnuToggleTopMost", "Keep window always on top"); //v3.2
        #endregion

        #region Tools
        Add("FrmMain.MnuTools", "Tools"); //v3.0
        Add("FrmMain.MnuColorPicker", "Color picker"); //v5.0
        Add("FrmMain.MnuPageNav", "Page navigation"); // v7.5
        Add("FrmMain.MnuCropping", "Cropping"); // v7.6
        Add("FrmMain.MnuExifTool", "Exif tool"); // v8.0
        
        #endregion

        Add("FrmMain.MnuSettings", "Settings…"); // v3.0

        #region Help
        Add("FrmMain.MnuHelp", "Help"); //v7.0
        Add("FrmMain.MnuAbout", "About"); //v3.0
        Add("FrmMain.MnuFirstLaunch", "First-launch configurations…"); //v5.0
        Add("FrmMain.MnuCheckForUpdate._NoUpdate", "Check for update…"); //v5.0
        Add("FrmMain.MnuCheckForUpdate._NewVersion", "A new version is available!"); //v5.0
        Add("FrmMain.MnuReportIssue", "Report an issue…"); //v3.0
        #endregion

        Add("FrmMain.MnuExit", "Exit"); //v7.0

        #endregion

        #region Form message texts
        Add("FrmMain.PicMain._ErrorText", "ImageGlass cannot open this picture because the file appears to be damaged, corrupted or not supported."); // v2.0 beta, updated 4.0

        //Add("FrmMain._ImageNotExist", "The viewing image doesn't exist."); // v4.5
        Add("FrmMain.MnuMain", "Main menu"); // v3.0

        Add("FrmMain._OpenFileDialog", "All supported files");
        Add("FrmMain._Files", "file(s)"); // v7.5
        Add("FrmMain._Loading", "Loading..."); // v3.0
        //Add("FrmMain._Pages", "pages"); // v7.5
        //Add("FrmMain._ImageData", "Image data"); // v5.0
        //Add("FrmMain._RenameDialogText", "Rename"); // v3.5
        //Add("FrmMain._RenameDialog", "Enter new filename");
        //Add("FrmMain._GotoDialogText", "Enter the image index to view it. Press ENTER");
        //Add("FrmMain._DeleteDialogText", "Delete file '{0}' ?");
        //Add("FrmMain._DeleteDialogTitle", "Confirm");

        //Add("FrmMain._ExtractPageText", "Extracting image pages. Please select output folder.");
        //Add("FrmMain._FullScreenMessage", "Press {0} to exit full screen mode.");// v2.0 beta, v6.0, v8.0
        //Add("FrmMain._SlideshowMessage", "Press ESC to exit slideshow.\n Right click to open context menu."); // v2.0 beta
        //Add("FrmMain._SlideshowMessagePause", "Slideshow is paused"); // v4.0
        //Add("FrmMain._SlideshowMessageResume", "Slideshow is resumed"); // v4.0
        Add("FrmMain._CopyFileText", "Copied {0} file(s)."); // v2.0 final
        Add("FrmMain._CutFileText", "Cut {0} file(s)."); // v2.0 final
        Add("FrmMain._CopyImageData", "Copied the current image data."); // v5.0
        Add("FrmMain._ImagePathCopied", "Copied the current image path."); // v9.0
        Add("FrmMain._ClearClipboard", "Cleared clipboard."); // v2.0 final
        //Add("FrmMain._SaveChanges", "Saving change..."); // v2.0 final
        //Add("FrmMain._SaveImage", "Image was saved to\r\n{0}"); // v5.0
        //Add("FrmMain._SavingImage", "Saving image...\r\n{0}"); // v7.6
        //Add("FrmMain._SaveImageError", "Unable to save image\r\n{0}."); // v5.0

        Add("FrmMain._ReachedFirstImage", "Reached the first image"); // v4.0
        Add("FrmMain._ReachedLastLast", "Reached the last image"); // v4.0
        //Add("FrmMain._CannotRotateAnimatedFile", "Modification for animated format is not supported"); // Added V5.0; Modified V6.0
        //Add("FrmMain._SetLockImage_Error", "There was an error while setting lock screen image"); // v6.0
        //Add("FrmMain._SetLockImage_Success", "Lock screen image was set successfully"); //v6.0
        //Add("FrmMain._SetBackground_Error", "There was an error while setting desktop background"); // v6.0
        //Add("FrmMain._SetBackground_Success", "Desktop background was set successfully"); // v6.0

        //Add("FrmMain._PageExtractComplete", "Page extraction completed."); // v7.5
        //Add("FrmMain._Frameless", "Hold SHIFT to move the window."); // v7.5
        //Add("FrmMain._InvalidImageClipboardData", "Clipboard does not contain image data."); // v8.0

        Add("FrmMain._ToolbarItemClick._CannotFindMenu", "Cannot find the menu {0}"); // 9.0
        Add("FrmMain._ToolbarItemClick._CannotFindMethod", "Cannot find the method {0}"); // 9.0

        #endregion

        #endregion

        #region FrmAbout
        //Add("FrmAbout.lblSlogant", "A lightweight, versatile image viewer"); //changed 4.0
        //Add("FrmAbout.lblInfo", "Information");
        //Add("FrmAbout.lblComponent", "Components");
        //Add("FrmAbout.lblReferences", "References");
        //Add("FrmAbout.lblVersion", "Version: {0}");
        //Add("FrmAbout.lblInfoContact", "Contact");
        //Add("FrmAbout.lblSoftwareUpdate", "Software updates");
        //Add("FrmAbout.lnkCheckUpdate", "» Check for update…");
        //Add("FrmAbout._Text", "About");
        //Add("FrmAbout._PortableText", "[Portable]"); //v4.0
        #endregion

        #region FrmSetting
        //Add("FrmSetting._Text", "Settings");

        //Add("FrmSetting.btnSave", "Save"); //v4.1
        //Add("FrmSetting.btnCancel", "Cancel"); //v4.1
        //Add("FrmSetting.btnApply", "Apply"); //v4.1

        #region Tab names
        //Add("FrmSetting.lblGeneral", "General");
        //Add("FrmSetting.lblImage", "Image"); //v4.0
        //Add("FrmSetting.lblEdit", "Edit"); //v6.0
        //Add("FrmSetting.lblFileTypeAssoc", "File Type Associations"); //v2.0 final
        //Add("FrmSetting.lblToolbar", "Toolbar"); //v5.0
        //Add("FrmSetting.lblLanguage", "Language");
        //Add("FrmSetting.lblTheme", "Theme"); //v5.0
        //Add("FrmSetting.lblKeyboard", "Keyboard"); // v7.0
        #endregion

        #region TAB General
        #region Start up
        //Add("FrmSetting.lblHeadStartup", "Start up"); //v4.0
        //Add("FrmSetting.chkWelcomePicture", "Show welcome picture");
        //Add("FrmSetting.chkLastSeenImage", "Open last seen image"); //v6.0
        //Add("FrmSetting.chkShowToolBar", "Show toolbar when starting up"); //v4.0

        #endregion

        #region Configuration dir
        //Add("FrmSetting.lblHeadConfigDir", "Configuration directory"); // 5.5.x
        #endregion

        #region Viewer
        //Add("FrmSetting.lblHeadViewer", "Viewer"); // v7.6
        //Add("FrmSetting.chkShowScrollbar", "Display viewer scrollbars"); //v4.1
        //Add("FrmSetting.chkShowNavButtons", "Display navigation arrow buttons"); //v6.0
        //Add("FrmSetting.chkDisplayBasename", "Display basename of the viewing image on title bar"); //v5.0
        //Add("FrmSetting.chkShowCheckerboardOnlyImage", "Display checkerboard only in the image region"); //v6.0
        //Add("FrmSetting.chkUseTouchGesture", "Enable touch gesture support"); // v7.6
        //Add("FrmSetting.lblBackGroundColor", "Background color");
        //Add("FrmSetting.lnkResetBackgroundColor", "Reset"); // v4.0
        #endregion

        #region Others
        //Add("FrmSetting.lblHeadOthers", "Others"); //v4.0
        //Add("FrmSetting.chkAutoUpdate", "Check for update automatically");
        //Add("FrmSetting.chkAllowMultiInstances", "Allow multiple instances of the program"); //v3.0
        //Add("FrmSetting.chkESCToQuit", "Allow to press ESC to quit application"); //v2.0 final
        //Add("FrmSetting.chkConfirmationDelete", "Display Delete confirmation dialog"); //v4.0
        //Add("FrmSetting.chkCenterWindowFit", "Auto-center the window in Window Fit mode"); //v7.5
        //Add("FrmSetting.chkShowToast", "Show toast message"); //v7.5

        #endregion
        #endregion

        #region TAB Image
        #region Image loading
        //Add("FrmSetting.lblHeadImageLoading", "Image loading"); //v4.0
        //Add("FrmSetting.chkFindChildFolder", "Find images in child folder");
        //Add("FrmSetting.chkShowHiddenImages", "Show hidden images"); //v4.5
        //Add("FrmSetting.chkLoopViewer", "Loop back viewer to the first image when reaching the end of the list"); //v4.0
        //Add("FrmSetting.chkIsCenterImage", "Center image on viewer"); //v7.0
        //Add("FrmSetting.chkIsUseRawThumbnail", "Use embedded thumbnail for RAW formats"); //v8.3

        //Add("FrmSetting.lblImageLoadingOrder", "Image loading order");
        //Add("FrmSetting.chkUseFileExplorerSortOrder", "Use Windows File Explorer sort order if possible"); //v7.0
        //Add("FrmSetting.chkGroupByDirectory", "Group images by directory"); //v8.0
        //Add("FrmSetting.lblImageBoosterCachedCount", "Number of images cached by ImageBooster (one direction)"); //v7.0
        #endregion

        #region Color Management
        //Add("FrmSetting.lblColorManagement", "Color management"); //v6.0
        //Add("FrmSetting.chkApplyColorProfile", "Apply also for images without embedded color profile"); //v6.0
        //Add("FrmSetting.lblColorProfile", "Color profile:"); //v6.0
        //Add("FrmSetting.lnkColorProfileBrowse", "Browse…"); //v6.0
        //Add("FrmSetting.cmbColorProfile._None", "None"); //v6.0
        //Add("FrmSetting.cmbColorProfile._CustomProfileFile", "Custom…"); //v6.0

        #endregion

        #region Mouse wheel actions
        //Add("FrmSetting.lblHeadMouseWheelActions", "Mouse wheel actions");
        //Add("FrmSetting.lblMouseWheel", "Mouse wheel");
        //Add("FrmSetting.lblMouseWheelAlt", "Mouse wheel + Alt");
        //Add("FrmSetting.lblMouseWheelCtrl", "Mouse wheel + Ctrl");
        //Add("FrmSetting.lblMouseWheelShift", "Mouse wheel + Shift");
        //Add("FrmSetting.cmbMouseWheel._DoNothing", "Do nothing");
        //Add("FrmSetting.cmbMouseWheel._Zoom", "Zoom");
        //Add("FrmSetting.cmbMouseWheel._ScrollVertically", "Scroll vertically");
        //Add("FrmSetting.cmbMouseWheel._ScrollHorizontally", "Scroll horizontally");
        //Add("FrmSetting.cmbMouseWheel._BrowseImages", "Previous/next image");
        #endregion

        #region Zooming
        //Add("FrmSetting.lblHeadZooming", "Zooming"); //v4.0
        //Add("FrmSetting.lblGeneral_ZoomOptimization", "Zoom optimization"); //-3.0, +3.5
        //Add("FrmSetting.cmbZoomOptimization._Auto", "Auto (Low quality/Nearest-neighbor)"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._Low", "Low quality"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._High", "High quality"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._Bilinear", "Bilinear"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._Bicubic", "Bicubic"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._NearestNeighbor", "Nearest-neighbor"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._HighQualityBilinear", "High-quality, bilinear"); // v8.1
        //Add("FrmSetting.cmbZoomOptimization._HighQualityBicubic", "High-quality, bicubic"); // v8.1

        //Add("FrmSetting.lblZoomLevels", "Zoom levels"); //v7.0
        //Add("FrmSetting.txtZoomLevels._Error", "There was error updating Zoom levels. Error message:\r\n\n{0}"); //v7.0
        #endregion

        #region Thumbnail bar
        //Add("FrmSetting.lblHeadThumbnailBar", "Thumbnail bar"); //v4.0
        //Add("FrmSetting.chkThumbnailVertical", "Show thumbnails on right side");
        //Add("FrmSetting.chkShowThumbnailScrollbar", "Show thumbnails scroll bar"); //v5.5
        //Add("FrmSetting.lblGeneral_ThumbnailSize", "Thumbnail dimension (pixel)"); // v3.0
        #endregion

        #region Slideshow
        //Add("FrmSetting.lblHeadSlideshow", "Slideshow"); // v4.0
        //Add("FrmSetting.chkLoopSlideshow", "Loop back slideshow to the first image when reaching the end of the list"); // v2.0 final
        //Add("FrmSetting.chkShowSlideshowCountdown", "Show slideshow countdown"); // v7.5
        //Add("FrmSetting.chkRandomSlideshowInterval", "Use random interval"); // v7.6
        //Add("FrmSetting.lblSlideshowInterval", "Slideshow interval: {0}");
        //Add("FrmSetting.lblSlideshowIntervalTo", "to"); // v7.6
        #endregion

        #region Full screen
        //Add("FrmSetting.lblHeadFullScreen", "Full screen"); // v8.3
        //Add("FrmSetting.chkHideToolbarInFullScreen", "Hide toolbar"); // v8.3
        //Add("FrmSetting.chkHideThumbnailBarInFullScreen", "Hide thumbnail bar"); // v8.3
        #endregion

        #endregion

        #region TAB Edit
        //Add("FrmSetting.chkSaveOnRotate", "Save the viewing image after rotating"); //v4.5
        //Add("FrmSetting.lblSelectAppForEdit", "Select application for image editing"); //v4.5
        //Add("FrmSetting.lblAfterEditingApp", "After opening editing app:"); // v8.0
        //Add("FrmSetting.lblImageQuality", "Image quality:"); // v8.0

        //Add("FrmSetting.btnEditEditExt", "Edit…"); //v4.0
        //Add("FrmSetting.btnEditResetExt", "Reset to default"); //v4.0
        //Add("FrmSetting.btnEditEditAllExt", "Edit all extensions…"); //v4.1
        //Add("FrmSetting._allExtensions", "all extensions"); //v4.1
        //Add("FrmSetting.lvImageEditing.clnFileExtension", "File extension"); //v4.0
        //Add("FrmSetting.lvImageEditing.clnAppName", "App name"); //v4.0
        //Add("FrmSetting.lvImageEditing.clnAppPath", "App path"); //v4.0
        //Add("FrmSetting.lvImageEditing.clnAppArguments", "App arguments"); //v4.0

        //Add("FrmSetting.chkSaveModifyDate", "Preserve the image's modified date on save"); //v5.5, v8.0
        #endregion

        #region TAB File Associations
        //Add("FrmSetting.lblSupportedExtension", "Supported formats: {0}"); // v3.0, updated v4.0
        //Add("FrmSetting.lnkOpenFileAssoc", "Open File Type Associations"); // 4.0

        //Add("FrmSetting.btnAddNewExt", "Add…"); // 4.0
        //Add("FrmSetting.btnRegisterExt", "Set as Default photo viewer…"); // 4.0, updated v5.0
        //Add("FrmSetting.btnUnregisterExt", "Unregister extensions"); // 8.0
        //Add("FrmSetting.btnDeleteExt", "Delete"); // 4.0
        //Add("FrmSetting.btnResetExt", "Reset to default"); // 4.0
        //Add("FrmSetting._RegisterWebToApp_Error", "Unable to register Web-to-App linking"); // 7.0
        //Add("FrmSetting._RegisterAppExtensions_Error", "Unable to register file extensions for ImageGlass app"); // 6.0
        //Add("FrmSetting._RegisterAppExtensions_Success", "All file extensions are registered successfully! To set ImageGlass as Default photo viewer, please open Windows Settings > Default Apps, and manually select ImageGlass app under Photo Viewer section."); // 6.0

        //Add("FrmSetting._UnregisterAppExtensions_Error", "Unable to delete registered file extensions of ImageGlass app"); // 8.0
        //Add("FrmSetting._UnregisterAppExtensions_Success", "All file extensions are unregistered successfully!"); // 8.0
        #endregion

        #region TAB Toolbar
        //Add("FrmSetting.lblToolbarPosition", "Toolbar position:"); // v5.5
        //Add("FrmSetting.lblToolbarIconHeight", "Toolbar icon size:");
        //Add("FrmSetting.cmbToolbarPosition._Top", "Top"); // v5.5
        //Add("FrmSetting.cmbToolbarPosition._Bottom", "Bottom"); // v5.5

        //// V5.0
        //Add("FrmSetting._separator", "Separator"); // i.e. 'toolbar separator'
        //Add("FrmSetting.lblToolbar._Tooltip", "Configure toolbar buttons"); // tooltip
        //Add("FrmSetting.lblUsedBtns", "Current Buttons:");
        //Add("FrmSetting.lblAvailBtns", "Available Buttons:");
        //Add("FrmSetting.btnMoveDown._Tooltip", "Move selected button down"); // tooltip
        //Add("FrmSetting.btnMoveLeft._Tooltip", "Remove selected button(s) from the toolbar"); // tooltip
        //Add("FrmSetting.btnMoveRight._Tooltip", "Add selected button(s) to the toolbar"); // tooltip
        //Add("FrmSetting.btnMoveUp._Tooltip", "Move selected button up"); // tooltip

        //Add("FrmSetting.chkHorzCenterToolbarBtns", "Center toolbar buttons horizontally in window"); // V6.0
        //Add("FrmSetting.chkHideTooltips", "Hide toolbar tooltips"); // v8.0
        #endregion

        #region TAB Tools
        //Add("FrmSetting.chkColorUseRGBA", "Use RGB format with Alpha value"); //v5.0
        //Add("FrmSetting.chkColorUseHEXA", "Use HEX format with Alpha value"); //v5.0
        //Add("FrmSetting.chkColorUseHSLA", "Use HSL format with Alpha value"); //v5.0
        //Add("FrmSetting.chkColorUseHSVA", "Use HSV format with Alpha value"); //v8.0
        //Add("FrmSetting.lblDefaultColorCode", "Default color code format when copying"); //v5.0

        //Add("FrmSetting.chkShowPageNavAuto", "Auto-show Page navigation tool for multi-page image"); //v7.5

        //Add("FrmSetting.chkExifToolAlwaysOnTop", "Keep Exif tool always on top"); // v8.0
        //Add("FrmSetting.lnkSelectExifTool", "Select Exif tool file"); // v8.0
        //Add("FrmSetting.lnkSelectExifTool._NotFound", "The Exif tool does not exist or invalid: \n{0}"); // v8.0

        //Add("FrmSetting.lblExifToolCommandArgs", "Command arguments:"); // v8.1
        //Add("FrmSetting.lblExifToolCommandPreview", "Command preview:"); // v8.1
        #endregion

        #region TAB Language
        //Add("FrmSetting.lblLanguageText", "Installed languages");
        //Add("FrmSetting.lnkRefresh", "> Refresh");
        //Add("FrmSetting.lblLanguageWarning", "This language pack may be not compatible with {0}"); //v3.2

        //Add("FrmSetting.lnkInstallLanguage", "> Install new language pack (*.iglang)…"); //v2.0 final
        //Add("FrmSetting.lnkCreateNew", "> Create new language pack…");
        //Add("FrmSetting.lnkEdit", "> Edit selected language pack…");
        //Add("FrmSetting.lnkGetMoreLanguage", "> Get more language packs…");
        #endregion

        #region TAB Theme

        //Add("FrmSetting.lblInstalledThemes", "Installed themes: {0}"); //v5.0
        //Add("FrmSetting.lnkThemeDownload", "Download themes…"); //v5.0
        //Add("FrmSetting.btnThemeRefresh", "Refresh"); //v5.0
        //Add("FrmSetting.btnThemeInstall", "Install…"); //v5.0
        //Add("FrmSetting.btnThemeUninstall", "Uninstall…"); //v5.0
        //Add("FrmSetting.btnThemeSaveAs", "Save as…"); //v5.0
        //Add("FrmSetting.btnThemeFolderOpen", "Open theme folder"); //v5.0
        //Add("FrmSetting.btnThemeApply", "Apply theme"); //v5.0

        //Add("FrmSetting.txtThemeInfo._Name", "Name"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Version", "Version"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Author", "Author"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Email", "Email"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Website", "Website"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Compatibility", "Compatibility"); //v5.0
        //Add("FrmSetting.txtThemeInfo._Description", "Description"); //v5.0

        //Add("FrmSetting.btnThemeInstall._Success", "Your theme was installed successfully!"); //v5.0
        //Add("FrmSetting.btnThemeInstall._Error", "Unable to install your theme."); //v5.0
        //Add("FrmSetting.btnThemeUninstall._Error", "Unable to uninstall the selected theme."); //v5.0
        //Add("FrmSetting.btnThemeSaveAs._Success", "Your selected theme has been saved in {0}"); //v5.0
        //Add("FrmSetting.btnThemeSaveAs._Error", "Unable to save your selected theme."); //v5.0
        //Add("FrmSetting.btnThemeApply._Success", "The selected theme was applied successfully!"); //v5.0
        //Add("FrmSetting.btnThemeApply._Error", "Unable to apply the selected theme."); //v5.0

        #endregion

        #region TAB Keyboard
        //Add("FrmSetting.btnKeyReset", "Reset to default"); // v7.0
        //Add("FrmSetting.lblKeysSpaceBack", "Space / Backspace"); // v7.0
        //Add("FrmSetting.lblKeysPageUpDown", "PageUp / PageDown"); // v7.0
        //Add("FrmSetting.lblKeysUpDown", "Up / Down arrows"); // v7.0
        //Add("FrmSetting.lblKeysLeftRight", "Left / Right arrows"); // v7.0

        #region Actions Combo Values
        //Add("FrmSetting.KeyActions._PrevNextImage", "Previous / Next Image"); // v7.0
        //Add("FrmSetting.KeyActions._PanLeftRight", "Pan Left / Right"); // v7.0
        //Add("FrmSetting.KeyActions._PanUpDown", "Pan Up / Down"); // v7.0
        //Add("FrmSetting.KeyActions._ZoomInOut", "Zoom In / Out"); // v7.0
        //Add("FrmSetting.KeyActions._PauseSlideshow", "Pause slideshow"); // v7.0
        //Add("FrmSetting.KeyActions._DoNothing", "Do nothing"); // v7.0
        #endregion

        #endregion

        #endregion

        #region FrmAddNewFormat
        Add("FrmAddNewFormat.lblFileExtension", "File extension"); // 4.0
        Add("FrmAddNewFormat.btnOK", "OK"); // 4.0
        Add("FrmAddNewFormat.btnClose", "Close"); // 4.0
        #endregion

        #region FrmEditApp
        Add("FrmEditApp.lblFileExtension", "File extension"); // 4.0
        Add("FrmEditApp.lblAppName", "App name"); // 4.0
        Add("FrmEditApp.lblAppPath", "App path"); // 4.0
        Add("FrmEditApp.lblAppArguments", "App arguments"); // 4.0
        Add("FrmEditApp.btnReset", "Reset"); // 4.0
        Add("FrmEditApp.btnOK", "OK"); // 4.0
        Add("FrmEditApp.btnClose", "Close"); // 4.0
        Add("FrmEditApp.lblPreviewLabel", "Preview"); // 5.0
        #endregion

        #region FrmFirstLaunch
        Add("FrmFirstLaunch._Text", "First-Launch Configurations"); //v5.0
        Add("FrmFirstLaunch._ConfirmCloseProcess", "ImageGlass needs to close all its processes to apply the new settings, do you want to continue?"); //v7.5
        Add("FrmFirstLaunch.lblStepNumber", "Step {0}/{1}"); //v5.0
        Add("FrmFirstLaunch.btnNextStep", "Next"); //v5.0
        Add("FrmFirstLaunch.btnNextStep._Done", "Done!"); //v5.0
        Add("FrmFirstLaunch.lnkSkip", "Skip this and Launch ImageGlass"); //v5.0

        Add("FrmFirstLaunch.lblLanguage", "Select Language"); //v5.0
        Add("FrmFirstLaunch.lblLayout", "Select Layout"); //v5.0
        Add("FrmFirstLaunch.cmbLayout._Standard", "Standard"); //v5.0
        Add("FrmFirstLaunch.cmbLayout._Designer", "Designer"); //v5.0
        Add("FrmFirstLaunch.lblTheme", "Select Theme"); //v5.0
        Add("FrmFirstLaunch.lblDefaultApp", "Set ImageGlass as Default Photo Viewer?"); //v5.0
        Add("FrmFirstLaunch.btnSetDefaultApp", "Yes"); //v5.0
        #endregion

        #region FrmCrop
        Add("FrmCrop.lblWidth", "Width:"); //v7.6
        Add("FrmCrop.lblHeight", "Height:"); //v7.6
        Add("FrmCrop.btnSave", "Save"); //v7.6
        Add("FrmCrop.btnSaveAs", "Save as…"); //v7.6
        Add("FrmCrop.btnCopy", "Copy"); //v7.6
        Add("FrmCrop.btnReset", "Reset"); //v8.0

        #endregion

        #region FrmExifTool
        Add("FrmExifTool.clnProperty", "Property"); // v8.0
        Add("FrmExifTool.clnValue", "Value"); // v8.0

        Add("FrmExifTool.btnCopyValue", "Copy value"); // v8.0
        Add("FrmExifTool.btnExport", "Export…"); // v8.0
        Add("FrmExifTool.btnClose", "Close"); // v8.0

        #endregion

    }

}