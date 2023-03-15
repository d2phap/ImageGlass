/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Base.Photoing.Codecs;

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
        var model = BHelper.ReadJson<IgLangJsonModel>(FileName);
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
    public async Task SaveAsFileAsync(string filePath)
    {
        var model = new IgLangJsonModel(Metadata, this);

        await BHelper.WriteJsonAsync(filePath, model);
    }


    /// <summary>
    /// Initialize default language
    /// </summary>
    public void InitDefaultLanguage()
    {
        //Add("_IncompatibleConfigs", "Some settings are not compatible with your ImageGlass {0}. It's recommended to update them before continuing.\r\n\n- Click Yes to learn about the changes.\r\n- Click No to launch ImageGlass with default settings."); //v7.5

        Add("_._OK", "OK"); // v9.0
        Add("_._Cancel", "Cancel"); // v9.0
        Add("_._Apply", "Apply"); // v9.0
        Add("_._Close", "Close"); // v9.0
        Add("_._Yes", "Yes"); // v9.0
        Add("_._No", "No"); // v9.0
        Add("_._LearnMore", "Learn more…"); // v9.0
        Add("_._Continue", "Continue"); // v9.0
        Add("_._Quit", "Quit"); // v9.0
        Add("_._Error", "Error"); // v9.0
        Add("_._Warning", "Warning"); // v9.0
        Add("_.Copy", "Copy"); //v9.0

        Add("_._UnhandledException", "Unhandled exception"); // v9.0
        Add("_._UnhandledException._Description", "Unhandled exception has occurred. If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately."); // v9.0
        Add("_._DoNotShowThisMessageAgain", "Do not show this message again"); // v9.0
        Add($"_._CreatingFile", "Create temporary image file..."); //v9.0
        Add($"_._CreatingFileError", "Could not create temporary image file"); //v9.0


        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Name)}", "Name (default)"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.FileSize)}", "File size"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.CreationTime)}", "Date created"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.LastAccessTime)}", "Last access time"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.LastWriteTime)}", "Last write time"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Extension)}", "Extension"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Rating)}", "Rating"); //v8.0
        Add($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Random)}", "Random"); //v8.0

        Add($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Asc)}", "Ascending");  //v8.0
        Add($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Desc)}", "Descending");  //v8.0

        Add($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Nothing)}", "Nothing"); //v8.0
        Add($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Minimize)}", "Minimize"); //v8.0
        Add($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Close)}", "Close"); //v8.0


        Add("_._UserAction._MenuNotFound", "Cannot find menu '{0}' to invoke its action."); // v9.0
        Add("_._UserAction._MethodNotFound", "Cannot find method '{0}' to invoke its action."); // v9.0
        Add("_._UserAction._MethodArgumentNotSupported", "The argument type of method '{0}' is not supported."); // v9.0

        // Gallery tooltip
        Add($"_.Metadata._{nameof(IgMetadata.FileSize)}", "File size"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.FileCreationTime)}", "Date created"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.FileLastAccessTime)}", "Date accessed"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.FileLastWriteTime)}", "Date modified"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.FramesCount)}", "Frames"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.ExifRatingPercent)}", "Rating"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.ColorSpace)}", "Color space"); //v9.0
        Add($"_.Metadata._{nameof(IgMetadata.ColorProfile)}", "Color profile"); //v9.0


        #region FrmMain

        #region Main menu

        #region File
        Add("FrmMain.MnuFile", "File"); //v7.0
        Add("FrmMain.MnuOpenFile", "Open file…"); //v3.0
        Add("FrmMain.MnuNewWindow", "Open new window"); //v7.0
        Add("FrmMain.MnuNewWindow._Error", "Cannot open new window because only one instance allowed"); //v7.0
        Add("FrmMain.MnuSave", "Save image"); //v8.1
        Add("FrmMain.MnuSave._Confirm", "Are you sure you want to override this image?"); //v9.0
        Add("FrmMain.MnuSave._ConfirmDescription", "ImageGlass is not a professional photo editor, please be aware of losing the quality, metadata, layers,... when saving your image."); //v9.0
        Add("FrmMain.MnuSave._Saving", "Saving image..."); //v9.0
        Add("FrmMain.MnuSave._Success", "Image is saved"); //v9.0
        Add("FrmMain.MnuSave._Error", "Could not save the image"); //v9.0
        Add("FrmMain.MnuSaveAs", "Save image as…"); //v3.0
        Add("FrmMain.MnuRefresh", "Refresh"); //v3.0
        Add("FrmMain.MnuReload", "Reload image"); //v5.5
        Add("FrmMain.MnuReloadImageList", "Reload image list"); //v7.0
        Add("FrmMain.MnuUnload", "Unload image"); //v9.0
        Add("FrmMain.MnuOpenWith", "Open with…"); //v7.6
        Add("FrmMain.MnuEdit", "Edit image {0}…"); //v3.0,
        Add("FrmMain.MnuEdit._AppNotFound", "Could not find the associated app for editing. You can assign an app for editing this format in ImageGlass Settings > Edit."); //v9.0
        Add("FrmMain.MnuPrint", "Print…"); //v3.0
        Add("FrmMain.MnuPrint._Error", "Could not print the viewing image"); //v9.0
        Add("FrmMain.MnuShare", "Share…"); //v8.6
        Add("FrmMain.MnuShare._Error", "Could not open Share dialog."); //v9.0
        #endregion

        #region Navigation
        Add("FrmMain.MnuNavigation", "Navigation"); //v3.0
        Add("FrmMain.MnuViewNext", "View next image"); //v3.0
        Add("FrmMain.MnuViewPrevious", "View previous image"); //v3.0

        Add("FrmMain.MnuGoTo", "Go to…"); //v3.0
        Add("FrmMain.MnuGoTo._Description", "Enter the image index to view, and then press ENTER");
        Add("FrmMain.MnuGoToFirst", "Go to the first image"); //v3.0
        Add("FrmMain.MnuGoToLast", "Go to the last image"); //v3.0

        Add("FrmMain.MnuViewNextFrame", "View next frame"); //v7.5
        Add("FrmMain.MnuViewPreviousFrame", "View previous frame"); //v7.5
        Add("FrmMain.MnuViewFirstFrame", "View the first frame"); //v7.5
        Add("FrmMain.MnuViewLastFrame", "View the last last"); //v7.5
        #endregion // Navigation

        #region Zoom
        Add("FrmMain.MnuZoom", "Zoom"); //v7.0
        Add("FrmMain.MnuZoomIn", "Zoom in"); //v3.0
        Add("FrmMain.MnuZoomOut", "Zoom out"); //v3.0
        Add("FrmMain.MnuCustomZoom", "Custom zoom…"); // v8.3
        Add("FrmMain.MnuCustomZoom._Description", "Enter a new zoom value"); // v8.3
        Add("FrmMain.MnuScaleToFit", "Scale to fit"); //v3.5
        Add("FrmMain.MnuScaleToFill", "Scale to fill"); //v7.5
        Add("FrmMain.MnuActualSize", "Actual size"); //v3.0
        Add("FrmMain.MnuLockZoom", "Lock zoom ratio"); //v3.0
        Add("FrmMain.MnuAutoZoom", "Auto zoom"); //v5.5
        Add("FrmMain.MnuScaleToWidth", "Scale to width"); //v3.0
        Add("FrmMain.MnuScaleToHeight", "Scale to height"); //v3.0
        #endregion

        #region Panning
        Add("FrmMain.MnuPanning", "Panning"); //v9.0

        Add("FrmMain.MnuPanLeft", "Pan image left"); //v9.0
        Add("FrmMain.MnuPanRight", "Pan image right"); //v9.0
        Add("FrmMain.MnuPanUp", "Pan image up"); //v9.0
        Add("FrmMain.MnuPanDown", "Pan image down"); //v9.0

        Add("FrmMain.MnuPanToLeftSide", "Pan image to the left side"); //v9.0
        Add("FrmMain.MnuPanToRightSide", "Pan image to the right side"); //v9.0
        Add("FrmMain.MnuPanToTop", "Pan image to the top"); //v9.0
        Add("FrmMain.MnuPanToBottom", "Pan image to the bottom"); //v9.0
        #endregion // Panning

        #region Image
        Add("FrmMain.MnuImage", "Image"); //v7.0

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
        Add("FrmMain.MnuRename._Description", "Enter a new filename:"); // v9.0
        Add("FrmMain.MnuMoveToRecycleBin", "Move to the Recycle Bin"); //v3.0
        Add("FrmMain.MnuMoveToRecycleBin._Description", "Do you want to move this file to the Recycle bin?"); //v3.0
        Add("FrmMain.MnuDeleteFromHardDisk", "Delete from hard disk"); //v3.0
        Add("FrmMain.MnuDeleteFromHardDisk._Description", "Are you sure you want to permanently delete this file?"); //v3.0
        Add("FrmMain.MnuExportFrames", "Export image frames ({0})…"); //v7.5
        Add("FrmMain.MnuToggleImageAnimation", "Start / stop animating image"); //v3.0
        Add("FrmMain.MnuSetDesktopBackground", "Set as Desktop background"); //v3.0
        Add("FrmMain.MnuSetDesktopBackground._Error", "Could not set the viewing image as desktop background"); // v6.0
        Add("FrmMain.MnuSetDesktopBackground._Success", "Desktop background is updated"); // v6.0
        Add("FrmMain.MnuSetLockScreen", "Set as Lock screen image"); // V6.0
        Add("FrmMain.MnuSetLockScreen._Error", "Could not set the viewing image as lock screen image"); // v6.0
        Add("FrmMain.MnuSetLockScreen._Success", "Lock screen image is updated"); // v6.0
        Add("FrmMain.MnuOpenLocation", "Open image location"); //v3.0
        Add("FrmMain.MnuImageProperties", "Image properties"); //v3.0
        #endregion // Image

        #region Clipboard
        Add("FrmMain.MnuClipboard", "Clipboard"); //v3.0
        Add("FrmMain.MnuCopyFile", "Copy file"); //v3.0
        Add("FrmMain.MnuCopyFile._Success", "Copied {0} file(s)."); // v2.0 final
        Add("FrmMain.MnuCopyImageData", "Copy image data"); //v5.0
        Add("FrmMain.MnuCopyImageData._Copying", "Copying the image data. It's going to take a while..."); // v9.0
        Add("FrmMain.MnuCopyImageData._Success", "Copied the current image data."); // v5.0
        Add("FrmMain.MnuCutFile", "Cut file"); //v3.0
        Add("FrmMain.MnuCutFile._Success", "Cut {0} file(s)."); // v2.0 final
        Add("FrmMain.MnuCopyPath", "Copy image path"); //v3.0
        Add("FrmMain.MnuCopyPath._Success", "Copied the current image path."); // v9.0
        Add("FrmMain.MnuPasteImage", "Paste image"); //v3.0
        Add("FrmMain.MnuPasteImage._Error", "Could not find image data in the Clipboard"); // v8.0
        Add("FrmMain.MnuClearClipboard", "Clear clipboard"); //v3.0
        Add("FrmMain.MnuClearClipboard._Success", "Cleared clipboard."); // v2.0 final

        #endregion

        Add("FrmMain.MnuWindowFit", "Window fit"); //v7.5
        Add("FrmMain.MnuWindowFit._Enable", "Window fit is enabled"); // v9.0
        Add("FrmMain.MnuWindowFit._Disable", "Window fit is disabled"); // v9.0

        Add("FrmMain.MnuFullScreen", "Full screen"); //v3.0
        Add("FrmMain.MnuFullScreen._Enable", "Full screen window is enabled"); // v9.0
        Add("FrmMain.MnuFullScreen._EnableDescription", "Press {0} to exit full screen mode."); // v2.0
        Add("FrmMain.MnuFullScreen._Disable", "Full screen window is disabled"); // v9.0

        Add("FrmMain.MnuFrameless", "Frameless"); //v7.5
        Add("FrmMain.MnuFrameless._Enable", "Frameless window is enabled"); // v9.0
        Add("FrmMain.MnuFrameless._EnableDescription", "Hold Shift key to move the window."); // v7.5
        Add("FrmMain.MnuFrameless._Disable", "Frameless window is disabled"); // v9.0

        #region Slideshow
        Add("FrmMain.MnuSlideshow", "Slideshow"); //v3.0
        Add("FrmMain.MnuStartSlideshow", "Start a new slideshow"); //v9.0
        Add("FrmMain.MnuCloseAllSlideshows", "Close all slideshows"); //v9.0
        #endregion

        #region Layout
        Add("FrmMain.MnuLayout", "Layout"); //v3.0
        Add("FrmMain.MnuToggleToolbar", "Toolbar"); //v3.0
        Add("FrmMain.MnuToggleThumbnails", "Thumbnail panel"); //v3.0
        Add("FrmMain.MnuToggleCheckerboard", "Checkerboard background"); //v3.0, updated v5.0
        Add("FrmMain.MnuToggleTopMost", "Keep window always on top"); //v3.2
        Add("FrmMain.MnuToggleTopMost._Enable", "Enabled window always on top"); // v9.0
        Add("FrmMain.MnuToggleTopMost._Disable", "Disabled window always on top"); // v9.0
        #endregion // Layout

        #region Tools
        Add("FrmMain.MnuTools", "Tools"); //v3.0
        Add("FrmMain.MnuColorPicker", "Color picker"); //v5.0
        Add("FrmMain.MnuPageNav", "Page navigation"); // v7.5
        Add("FrmMain.MnuCropTool", "Crop image"); // v7.6
        #endregion

        Add("FrmMain.MnuSettings", "Settings…"); // v3.0

        #region Help
        Add("FrmMain.MnuHelp", "Help"); //v7.0
        Add("FrmMain.MnuAbout", "About"); //v3.0
        Add("FrmMain.MnuFirstLaunch", "First-launch configurations…"); //v5.0
        Add("FrmMain.MnuCheckForUpdate._NoUpdate", "Check for update…"); //v5.0
        Add("FrmMain.MnuCheckForUpdate._NewVersion", "A new version is available!"); //v5.0
        Add("FrmMain.MnuReportIssue", "Report an issue…"); //v3.0

        Add("FrmMain.MnuSetDefaultPhotoViewer", "Set as default photo viewer"); //v9.0
        Add("FrmMain.MnuSetDefaultPhotoViewer._Success", "You have successfully registered ImageGlass as default photo viewer."); //v9.0
        Add("FrmMain.MnuSetDefaultPhotoViewer._SuccessDescription", "Next, please open Windows Settings > Default apps, and select ImageGlass under the Photo viewer section."); //v9.0
        Add("FrmMain.MnuSetDefaultPhotoViewer._Error", "Could not set ImageGlass as default photo viewer."); //v9.0

        Add("FrmMain.MnuUnsetDefaultPhotoViewer", "Unset default photo viewer"); //v9.0
        Add("FrmMain.MnuUnsetDefaultPhotoViewer._Success", "ImageGlass is now not the default photo viewer anymore."); //v9.0
        Add("FrmMain.MnuUnsetDefaultPhotoViewer._Error", "Could not remove ImageGlass from the default photo viewer setting."); //v9.0

        #endregion

        Add("FrmMain.MnuExit", "Exit"); //v7.0

        #endregion

        #region Form message texts
        Add("FrmMain.PicMain._ErrorText", "Could not open this image"); // v2.0 beta, updated 4.0, 9.0

        //Add("FrmMain._ImageNotExist", "The viewing image doesn't exist."); // v4.5
        Add("FrmMain.MnuMain", "Main menu"); // v3.0

        Add("FrmMain._OpenFileDialog", "All supported files");
        Add("FrmMain._Files", "file(s)"); // v7.5
        Add("FrmMain._Loading", "Loading..."); // v3.0
                                               //Add("FrmMain._Pages", "pages"); // v7.5

        //Add("FrmMain._SaveChanges", "Saving change..."); // v2.0 final

        Add("FrmMain._ReachedFirstImage", "Reached the first image"); // v4.0
        Add("FrmMain._ReachedLastLast", "Reached the last image"); // v4.0
        //Add("FrmMain._CannotRotateAnimatedFile", "Modification for animated format is not supported"); // Added V5.0; Modified V6.0
        //Add("FrmMain._SetLockImage_Error", "There was an error while setting lock screen image"); // v6.0
        //Add("FrmMain._SetLockImage_Success", "Lock screen image was set successfully"); //v6.0

        //Add("FrmMain._PageExtractComplete", "Page extraction completed."); // v7.5


        Add("FrmMain._ClipboardImage", "Clipboard image"); //v9.0


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
        //Add("FrmSetting.btnDeleteExt", "Delete"); // 4.0
        //Add("FrmSetting.btnResetExt", "Reset to default"); // 4.0

        #endregion

        #region TAB Toolbar
        //Add("FrmSetting.lblToolbarPosition", "Toolbar position:"); // v5.5
        //Add("FrmSetting.lblToolbarIconHeight", "Toolbar icon size:");

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
        Add("FrmCrop.LblAspectRatio", "Aspect ratio:"); //v9.0
        Add("FrmCrop.LblLocation", "Location:"); //v9.0
        Add("FrmCrop.LblSize", "Size:"); //v9.0

        Add("FrmCrop.SelectionAspectRatio._FreeRatio", "Free ratio"); //v9.0
        Add("FrmCrop.SelectionAspectRatio._Custom", "Custom…"); //v9.0
        Add("FrmCrop.SelectionAspectRatio._Original", "Original"); //v9.0

        Add("FrmCrop.BtnQuickSelect._Tooltip", "Quick select…"); //v9.0
        Add("FrmCrop.BtnReset._Tooltip", "Reset selection"); //v9.0
        Add("FrmCrop.BtnSettings._Tooltip", "Open Crop tool settings…"); //v9.0

        Add("FrmCrop.BtnSave", "Save"); //v9.0
        Add("FrmCrop.BtnSave._Tooltip", "Save image"); //v9.0
        Add("FrmCrop.BtnSaveAs", "Save as…"); //v9.0
        Add("FrmCrop.BtnSaveAs._Tooltip", "Save as a copy…"); //v9.0
        Add("FrmCrop.BtnCrop", "Crop"); //v9.0
        Add("FrmCrop.BtnCrop._Tooltip", "Crop the image only"); //v9.0
        Add("FrmCrop.BtnCopy", "Copy"); //v9.0
        Add("FrmCrop.BtnCopy._Tooltip", "Copy the selection to clipboard"); //v9.0


        // Crop settings
        Add("FrmCropSettings._Title", "Crop settings"); //v9.0
        Add("FrmCropSettings.ChkCloseToolAfterSaving", "Close Crop tool after saving"); //v9.0
        Add("FrmCropSettings.LblDefaultSelection", "Default selection settings"); //v9.0
        Add("FrmCropSettings.ChkAutoCenterSelection", "Auto-center selection"); //v9.0

        Add("FrmCropSettings.DefaultSelectionType._UseTheLastSelection", "Use the last selection"); //v9.0
        Add("FrmCropSettings.DefaultSelectionType._SelectNone", "Select none"); //v9.0
        Add("FrmCropSettings.DefaultSelectionType._SelectX", "Select {0}"); //v9.0
        Add("FrmCropSettings.DefaultSelectionType._SelectAll", "Select all"); //v9.0
        Add("FrmCropSettings.DefaultSelectionType._CustomArea", "Custom area…"); //v9.0

        #endregion

        #region FrmColorPicker

        Add("FrmColorPicker.BtnSettings._Tooltip", "Open Color picker settings…"); //v9.0

        // Color picker settings
        Add("FrmColorPickerSettings._Title", "Color picker settings"); //v9.0
        Add("FrmColorPickerSettings.ChkShowRgbA", "Use RGB format with alpha value"); //v5.0
        Add("FrmColorPickerSettings.ChkShowHexA", "Use HEX format with alpha value"); //v5.0
        Add("FrmColorPickerSettings.ChkShowHslA", "Use HSL format with alpha value"); //v5.0
        Add("FrmColorPickerSettings.ChkShowHsvA", "Use HSV format with alpha value"); //v8.0

        #endregion


        // External tools ----------------------------------------------------
        Add("FrmToolNotFound._Title", "Tool not found"); // v9.0
        Add("FrmToolNotFound.BtnSelectExecutable", "Select…"); // v9.0
        Add("FrmToolNotFound.LblHeading", "{0} is not found!"); // v9.0
        Add("FrmToolNotFound.LblDescription", "ImageGlass could not find executable file of {0}. Please provide the executable path of the tool."); // v9.0
        Add("FrmToolNotFound.LblDownloadToolText", "You can download more tools for ImageGlass at:"); // v9.0


        #region Tool_ExifTool
        Add($"_.Tools.{Constants.IGTOOL_EXIFTOOL}.ClnProperty", "Property"); // v8.0
        Add($"_.Tools.{Constants.IGTOOL_EXIFTOOL}.ClnValue", "Value"); // v8.0

        Add($"_.Tools.{Constants.IGTOOL_EXIFTOOL}.BtnCopyValue", "Copy value"); // v8.0
        Add($"_.Tools.{Constants.IGTOOL_EXIFTOOL}.BtnExport", "Export…"); // v8.0
        Add($"_.Tools.{Constants.IGTOOL_EXIFTOOL}.BtnClose", "Close"); // v8.0

        #endregion


        #region igcmd.exe

        Add("_._IgCommandExe._DefaultError._Heading", "Invalid commands"); //v9.0
        Add("_._IgCommandExe._DefaultError._Description", "Make sure you pass the correct commands!\r\nThis executable file contains command-line functions for ImageGlass software.\r\n\r\nTo explore all command lines, please visit:\r\n{0}"); //v9.0


        #region FrmSlideshow

        Add("FrmSlideshow._PauseSlideshow", "Slideshow is paused."); // v9.0
        Add("FrmSlideshow._ResumeSlideshow", "Slideshow is resumed."); // v9.0

        // menu
        Add("FrmSlideshow.MnuPauseResumeSlideshow", "Pause/resume slideshow"); // v9.0
        Add("FrmSlideshow.MnuExitSlideshow", "Exit slideshow"); // v9.0
        Add("FrmSlideshow.MnuChangeBackgroundColor", "Change background color…"); // v9.0

        Add("FrmSlideshow.MnuToggleCountdown", "Show slideshow countdown"); // v9.0
        Add("FrmSlideshow.MnuZoomModes", "Zoom modes"); // v9.0

        #endregion


        #region FrmExportFrames
        Add("FrmExportFrames._Title", "Export image frames"); //v9.0
        Add("FrmExportFrames._FileNotExist", "Image file does not exist"); //v7.5
        Add("FrmExportFrames._FolderPickerTitle", "Select output folder for exporting image frames"); //v9.0
        Add("FrmExportFrames._Exporting", "Exporting {0}/{1} frames \r\n{2}..."); //v9.0
        Add("FrmExportFrames._ExportDone", "Exported {0} frames successfully to \r\n{1}"); //v9.0
        Add("FrmExportFrames._OpenOutputFolder", "Open output folder"); //v9.0
        #endregion


        #endregion

    }

}