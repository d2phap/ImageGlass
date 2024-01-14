/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;

namespace ImageGlass.Base;


/// <summary>
/// ImageGlass language pack (<c>*.iglang.json</c>)
/// </summary>
public class IgLang : Dictionary<string, string>
{
    /// <summary>
    /// Gets the path of language file.
    /// Example: <c>C:\ImageGlass\Languages\Vietnameses.iglang.json</c>
    /// </summary>
    private string FilePath { get; set; } = "English";

    /// <summary>
    /// Gets the name of language file.
    /// Example: <c>Vietnameses.iglang.json</c>
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

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
    /// <param name="fileName">E.g. <c>Vietnamese.iglang.json</c></param>
    /// <param name="dirPath">The directory path contains language file (for relative filename)</param>
    public IgLang(string fileName, string dirPath = "")
    {
        InitDefaultLanguage();
        var filePath = Path.Combine(dirPath, fileName);

        if (File.Exists(filePath))
        {
            FilePath = filePath;
            ReadFromFile();
        }
    }


    /// <summary>
    /// Loads language strings from JSON file
    /// </summary>
    public void ReadFromFile()
    {
        var model = BHelper.ReadJson<IgLangJsonModel>(FilePath);
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
    public async Task SaveAsFileAsync(string filePath)
    {
        var model = new IgLangJsonModel(Metadata, this);

        if (Metadata.EnglishName.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            model._Metadata.EnglishName = "<Your language name in English>";
            model._Metadata.LocalName = "<Local name of your language>";
            model._Metadata.Author = "<Your name here>";
        }

        await BHelper.WriteJsonAsync(filePath, model);
    }


    /// <summary>
    /// Initialize default language
    /// </summary>
    public void InitDefaultLanguage()
    {
        //_ = TryAdd("_IncompatibleConfigs", "Some settings are not compatible with your ImageGlass {0}. It's recommended to update them before continuing.\r\n\n- Click Yes to learn about the changes.\r\n- Click No to launch ImageGlass with default settings."); //v7.5

        _ = TryAdd("_._OK", "OK"); // v9.0
        _ = TryAdd("_._Cancel", "Cancel"); // v9.0
        _ = TryAdd("_._Apply", "Apply"); // v9.0
        _ = TryAdd("_._Close", "Close"); // v9.0
        _ = TryAdd("_._Yes", "Yes"); // v9.0
        _ = TryAdd("_._No", "No"); // v9.0
        _ = TryAdd("_._LearnMore", "Learn more…"); // v9.0
        _ = TryAdd("_._Continue", "Continue"); // v9.0
        _ = TryAdd("_._Quit", "Quit"); // v9.0
        _ = TryAdd("_._Back", "Back"); // v9.0
        _ = TryAdd("_._Next", "Next"); // v9.0
        _ = TryAdd("_._Save", "Save"); // v9.0
        _ = TryAdd("_._Error", "Error"); // v9.0
        _ = TryAdd("_._Warning", "Warning"); // v9.0
        _ = TryAdd("_._Copy", "Copy"); //v9.0
        _ = TryAdd("_._Browse", "Browse…"); //v9.0
        _ = TryAdd("_._Reset", "Reset"); //v9.0
        _ = TryAdd("_._ResetToDefault", "Reset to default"); //v9.0
        _ = TryAdd("_._CheckForUpdate", "Check for update…"); //v5.0
        _ = TryAdd("_._Download", "Download"); //v9.0
        _ = TryAdd("_._Website", "Website"); //v9.0
        _ = TryAdd("_._Email", "Email"); //v9.0
        _ = TryAdd("_._Install", "Install…");
        _ = TryAdd("_._Refresh", "Refresh");
        _ = TryAdd("_._Delete", "Delete");
        _ = TryAdd("_._Add", "Add");
        _ = TryAdd("_._Add+", "Add…");
        _ = TryAdd("_._Edit", "Edit");
        _ = TryAdd("_._ID", "ID");
        _ = TryAdd("_._Name", "Name");
        _ = TryAdd("_._Hotkeys", "Hotkeys");
        _ = TryAdd("_._AddHotkey", "Add hotkey…");
        _ = TryAdd("_._Executable", "Executable");
        _ = TryAdd("_._Argument", "Argument");
        _ = TryAdd("_._CommandPreview", "Command preview");
        _ = TryAdd("_._FileExtension", "File extension");
        _ = TryAdd("_._Empty", "(empty)");
        _ = TryAdd("_._MoveUp", "Move up");
        _ = TryAdd("_._MoveDown", "Move down");
        _ = TryAdd("_._Separator", "Separator");
        _ = TryAdd("_._Icon", "Icon");
        _ = TryAdd("_._Description", "Description");
        _ = TryAdd("_._GetHelp", "Get help");

        _ = TryAdd("_._UnhandledException", "Unhandled exception"); // v9.0
        _ = TryAdd("_._UnhandledException._Description", "Unhandled exception has occurred. If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately."); // v9.0
        _ = TryAdd("_._DoNotShowThisMessageAgain", "Do not show this message again"); // v9.0
        _ = TryAdd($"_._CreatingFile", "Creating a temporary image file…"); //v9.0
        _ = TryAdd($"_._CreatingFileError", "Could not create temporary image file"); //v9.0
        _ = TryAdd($"_._NotSupported", "Unsupported format"); //v9.0

        _ = TryAdd($"_._InvalidAction", "Invalid action"); //v9.0
        _ = TryAdd($"_._InvalidAction._Transformation", "ImageGlass does not support rotation, flipping for this image."); //v9.0


        _ = TryAdd("_._UserAction._MenuNotFound", "Cannot find menu '{0}' to invoke the action"); // v9.0
        _ = TryAdd("_._UserAction._MethodNotFound", "Cannot find method '{0}' to invoke the action"); // v9.0
        _ = TryAdd("_._UserAction._MethodArgumentNotSupported", "The argument type of method '{0}' is not supported"); // v9.0
        _ = TryAdd("_._UserAction._Win32ExeError", "Cannot execute command '{0}'. Make sure the name is correct."); // v9.0

        _ = TryAdd("_._Webview2._NotFound", "ImageGlass could not detect WebView2 Runtime 64-bit on your machine."); // 9.1

        // Gallery tooltip
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.FileSize)}", "File size"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.FileCreationTime)}", "Date created"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.FileLastAccessTime)}", "Date accessed"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.FileLastWriteTime)}", "Date modified"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.FrameCount)}", "Frames"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.ExifRatingPercent)}", "Rating"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.ColorSpace)}", "Color space"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.ColorProfile)}", "Color profile"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.ExifDateTime)}", "EXIF DateTime"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.ExifDateTimeOriginal)}", "EXIF DateTimeOriginal"); //v9.0
        _ = TryAdd($"_.Metadata._{nameof(IgMetadata.Date)}", "Date"); //v9.0

        // image info
        _ = TryAdd($"_.{nameof(ImageInfo)}._{nameof(ImageInfo.ListCount)}", "{0} file(s)"); //v9.0
        _ = TryAdd($"_.{nameof(ImageInfo)}._{nameof(ImageInfo.FrameCount)}", "{0} frame(s)"); //v9.0

        // layout position
        _ = TryAdd($"_.Position._Left", "Left");
        _ = TryAdd($"_.Position._Right", "Right");
        _ = TryAdd($"_.Position._Top", "Top");
        _ = TryAdd($"_.Position._Bottom", "Bottom");


        #region Enums

        // ImageOrderBy
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Name)}", "Name (default)"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Random)}", "Random"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.FileSize)}", "File size"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Extension)}", "Extension"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Date)}", "Date"); //v9.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateCreated)}", "Date created"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateAccessed)}", "Date accessed"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateModified)}", "Date modified"); //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.ExifDateTaken)}", "EXIF: Date taken"); //v9.0
        _ = TryAdd($"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.ExifRating)}", "EXIF: Rating"); //v9.0


        // ImageOrderType
        _ = TryAdd($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Asc)}", "Ascending");  //v8.0
        _ = TryAdd($"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Desc)}", "Descending");  //v8.0

        // AfterEditAppAction
        _ = TryAdd($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Nothing)}", "Nothing"); //v8.0
        _ = TryAdd($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Minimize)}", "Minimize"); //v8.0
        _ = TryAdd($"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Close)}", "Close"); //v8.0

        // ColorProfileOption
        _ = TryAdd($"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.None)}", "None");
        _ = TryAdd($"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.CurrentMonitorProfile)}", "Current monitor profile");
        _ = TryAdd($"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.Custom)}", "Custom…");

        // BackdropStyle
        _ = TryAdd($"_.{nameof(BackdropStyle)}._{nameof(BackdropStyle.None)}", "None");

        // MouseWheelEvent
        _ = TryAdd($"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.Scroll)}", "Scroll");
        _ = TryAdd($"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.CtrlAndScroll)}", "Hold Ctrl and scroll");
        _ = TryAdd($"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.ShiftAndScroll)}", "Hold Shift and scroll");
        _ = TryAdd($"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.AltAndScroll)}", "Hold Alt and scroll");

        // MouseWheelAction
        _ = TryAdd($"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.DoNothing)}", "Do nothing");
        _ = TryAdd($"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.Zoom)}", "Zoom in / out");
        _ = TryAdd($"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.PanVertically)}", "Pan up / down");
        _ = TryAdd($"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.PanHorizontally)}", "Pan left / right");
        _ = TryAdd($"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.BrowseImages)}", "View next / previous Image");

        // ImageInterpolation
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.NearestNeighbor)}", "Nearest neighbor");
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Linear)}", "Linear");
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Cubic)}", "Cubic");
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.MultiSampleLinear)}", "Multi-sample linear");
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Antisotropic)}", "Antisotropic");
        _ = TryAdd($"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.HighQualityBicubic)}", "High quality bicubic");

        #endregion // Enums


        #region FrmMain

        #region Main menu

        #region File
        _ = TryAdd("FrmMain.MnuFile", "File"); //v7.0
        _ = TryAdd("FrmMain.MnuOpenFile", "Open file…"); //v3.0
        _ = TryAdd("FrmMain.MnuNewWindow", "Open new window"); //v7.0
        _ = TryAdd("FrmMain.MnuNewWindow._Error", "Cannot open new window because only one instance is allowed"); //v7.0
        _ = TryAdd("FrmMain.MnuSave", "Save"); //v8.1
        _ = TryAdd("FrmMain.MnuSave._Confirm", "Are you sure you want to override this image?"); //v9.0
        _ = TryAdd("FrmMain.MnuSave._ConfirmDescription", "ImageGlass is not a professional photo editor, please be aware of losing the quality, metadata, layers,… when saving your image."); //v9.0
        _ = TryAdd("FrmMain.MnuSave._Saving", "Saving image…"); //v9.0
        _ = TryAdd("FrmMain.MnuSave._Success", "Image is saved"); //v9.0
        _ = TryAdd("FrmMain.MnuSave._Error", "Could not save the image"); //v9.0
        _ = TryAdd("FrmMain.MnuSaveAs", "Save as…"); //v3.0
        _ = TryAdd("FrmMain.MnuRefresh", "Refresh"); //v3.0
        _ = TryAdd("FrmMain.MnuReload", "Reload image"); //v5.5
        _ = TryAdd("FrmMain.MnuReloadImageList", "Reload image list"); //v7.0
        _ = TryAdd("FrmMain.MnuUnload", "Unload image"); //v9.0
        _ = TryAdd("FrmMain.MnuOpenWith", "Open with…"); //v7.6
        _ = TryAdd("FrmMain.MnuEdit", "Edit image {0}…"); //v3.0,
        _ = TryAdd("FrmMain.MnuEdit._AppNotFound", "Could not find the associated app for editing. You can assign an app for editing this format in ImageGlass Settings > Edit."); //v9.0
        _ = TryAdd("FrmMain.MnuPrint", "Print…"); //v3.0
        _ = TryAdd("FrmMain.MnuPrint._Error", "Could not print the viewing image"); //v9.0
        _ = TryAdd("FrmMain.MnuShare", "Share…"); //v8.6
        _ = TryAdd("FrmMain.MnuShare._Error", "Could not open Share dialog."); //v9.0
        #endregion

        #region Navigation
        _ = TryAdd("FrmMain.MnuNavigation", "Navigation"); //v3.0
        _ = TryAdd("FrmMain.MnuViewNext", "View next image"); //v3.0
        _ = TryAdd("FrmMain.MnuViewPrevious", "View previous image"); //v3.0

        _ = TryAdd("FrmMain.MnuGoTo", "Go to…"); //v3.0
        _ = TryAdd("FrmMain.MnuGoTo._Description", "Enter the image index to view, and then press ENTER");
        _ = TryAdd("FrmMain.MnuGoToFirst", "Go to first image"); //v3.0
        _ = TryAdd("FrmMain.MnuGoToLast", "Go to last image"); //v3.0

        _ = TryAdd("FrmMain.MnuViewNextFrame", "View next frame"); //v7.5
        _ = TryAdd("FrmMain.MnuViewPreviousFrame", "View previous frame"); //v7.5
        _ = TryAdd("FrmMain.MnuViewFirstFrame", "View first frame"); //v7.5
        _ = TryAdd("FrmMain.MnuViewLastFrame", "View last frame"); //v7.5
        #endregion // Navigation

        #region Zoom
        _ = TryAdd("FrmMain.MnuZoom", "Zoom"); //v7.0
        _ = TryAdd("FrmMain.MnuZoomIn", "Zoom in"); //v3.0
        _ = TryAdd("FrmMain.MnuZoomOut", "Zoom out"); //v3.0
        _ = TryAdd("FrmMain.MnuCustomZoom", "Custom zoom…"); // v8.3
        _ = TryAdd("FrmMain.MnuCustomZoom._Description", "Enter a new zoom value"); // v8.3
        _ = TryAdd("FrmMain.MnuScaleToFit", "Scale to fit"); //v3.5
        _ = TryAdd("FrmMain.MnuScaleToFill", "Scale to fill"); //v7.5
        _ = TryAdd("FrmMain.MnuActualSize", "Actual size"); //v3.0
        _ = TryAdd("FrmMain.MnuLockZoom", "Lock zoom ratio"); //v3.0
        _ = TryAdd("FrmMain.MnuAutoZoom", "Auto zoom"); //v5.5
        _ = TryAdd("FrmMain.MnuScaleToWidth", "Scale to width"); //v3.0
        _ = TryAdd("FrmMain.MnuScaleToHeight", "Scale to height"); //v3.0
        #endregion

        #region Panning
        _ = TryAdd("FrmMain.MnuPanning", "Panning"); //v9.0

        _ = TryAdd("FrmMain.MnuPanLeft", "Pan image left"); //v9.0
        _ = TryAdd("FrmMain.MnuPanRight", "Pan image right"); //v9.0
        _ = TryAdd("FrmMain.MnuPanUp", "Pan image up"); //v9.0
        _ = TryAdd("FrmMain.MnuPanDown", "Pan image down"); //v9.0

        _ = TryAdd("FrmMain.MnuPanToLeftSide", "Pan image to left edge"); //v9.0
        _ = TryAdd("FrmMain.MnuPanToRightSide", "Pan image to right edge"); //v9.0
        _ = TryAdd("FrmMain.MnuPanToTop", "Pan image to top"); //v9.0
        _ = TryAdd("FrmMain.MnuPanToBottom", "Pan image to bottom"); //v9.0
        #endregion // Panning

        #region Image
        _ = TryAdd("FrmMain.MnuImage", "Image"); //v7.0

        _ = TryAdd("FrmMain.MnuViewChannels", "View channels"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._All", "All"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._Red", "Red"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._Green", "Green"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._Blue", "Blue"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._Black", "Black"); //v7.0
        _ = TryAdd("FrmMain.MnuViewChannels._Alpha", "Alpha"); //v7.0

        _ = TryAdd("FrmMain.MnuLoadingOrders", "Loading orders"); //v8.0

        _ = TryAdd("FrmMain.MnuRotateLeft", "Rotate left"); //v7.5
        _ = TryAdd("FrmMain.MnuRotateRight", "Rotate right"); //v7.5
        _ = TryAdd("FrmMain.MnuFlipHorizontal", "Flip Horizontal"); // V6.0
        _ = TryAdd("FrmMain.MnuFlipVertical", "Flip Vertical"); // V6.0
        _ = TryAdd("FrmMain.MnuRename", "Rename image…"); //v3.0
        _ = TryAdd("FrmMain.MnuRename._Description", "Enter a new filename:"); // v9.0
        _ = TryAdd("FrmMain.MnuMoveToRecycleBin", "Move to the Recycle Bin"); //v3.0
        _ = TryAdd("FrmMain.MnuMoveToRecycleBin._Description", "Do you want to move this file to the Recycle bin?"); //v3.0
        _ = TryAdd("FrmMain.MnuDeleteFromHardDisk", "Delete permanently"); //v3.0
        _ = TryAdd("FrmMain.MnuDeleteFromHardDisk._Description", "Are you sure you want to permanently delete this file?"); //v3.0
        _ = TryAdd("FrmMain.MnuExportFrames", "Export image frames…"); //v7.5
        _ = TryAdd("FrmMain.MnuToggleImageAnimation", "Start / stop animating image"); //v3.0
        _ = TryAdd("FrmMain.MnuSetDesktopBackground", "Set as Desktop background"); //v3.0
        _ = TryAdd("FrmMain.MnuSetDesktopBackground._Error", "Could not set the viewing image as desktop background"); // v6.0
        _ = TryAdd("FrmMain.MnuSetDesktopBackground._Success", "Desktop background is updated"); // v6.0
        _ = TryAdd("FrmMain.MnuSetLockScreen", "Set as Lock screen image"); // V6.0
        _ = TryAdd("FrmMain.MnuSetLockScreen._Error", "Could not set the viewing image as lock screen image"); // v6.0
        _ = TryAdd("FrmMain.MnuSetLockScreen._Success", "Lock screen image is updated"); // v6.0
        _ = TryAdd("FrmMain.MnuOpenLocation", "Open image location"); //v3.0
        _ = TryAdd("FrmMain.MnuImageProperties", "Image properties"); //v3.0
        #endregion // Image

        #region Clipboard
        _ = TryAdd("FrmMain.MnuClipboard", "Clipboard"); //v3.0
        _ = TryAdd("FrmMain.MnuCopyFile", "Copy file"); //v3.0
        _ = TryAdd("FrmMain.MnuCopyFile._Success", "Copied {0} file(s)."); // v2.0 final
        _ = TryAdd("FrmMain.MnuCopyImageData", "Copy image data"); //v5.0
        _ = TryAdd("FrmMain.MnuCopyImageData._Copying", "Copying the image data. It's going to take a while…"); // v9.0
        _ = TryAdd("FrmMain.MnuCopyImageData._Success", "Copied the current image data."); // v5.0
        _ = TryAdd("FrmMain.MnuCutFile", "Cut file"); //v3.0
        _ = TryAdd("FrmMain.MnuCutFile._Success", "Cut {0} file(s)."); // v2.0 final
        _ = TryAdd("FrmMain.MnuCopyPath", "Copy image path"); //v3.0
        _ = TryAdd("FrmMain.MnuCopyPath._Success", "Copied the current image path."); // v9.0
        _ = TryAdd("FrmMain.MnuPasteImage", "Paste image"); //v3.0
        _ = TryAdd("FrmMain.MnuPasteImage._Error", "Could not find image data in the Clipboard"); // v8.0
        _ = TryAdd("FrmMain.MnuClearClipboard", "Clear clipboard"); //v3.0
        _ = TryAdd("FrmMain.MnuClearClipboard._Success", "Cleared clipboard."); // v2.0 final

        #endregion

        _ = TryAdd("FrmMain.MnuWindowFit", "Window Fit"); //v7.5
        _ = TryAdd("FrmMain.MnuFullScreen", "Full Screen"); //v3.0

        _ = TryAdd("FrmMain.MnuFrameless", "Frameless"); //v7.5
        _ = TryAdd("FrmMain.MnuFrameless._EnableDescription", "Hold Shift key to move the window."); // v7.5

        _ = TryAdd("FrmMain.MnuSlideshow", "Slideshow"); //v3.0

        #region Layout
        _ = TryAdd("FrmMain.MnuLayout", "Layout"); //v3.0
        _ = TryAdd("FrmMain.MnuToggleToolbar", "Toolbar"); //v3.0
        _ = TryAdd("FrmMain.MnuToggleGallery", "Gallery panel"); //v3.0
        _ = TryAdd("FrmMain.MnuToggleCheckerboard", "Checkerboard background"); //v3.0, updated v5.0
        _ = TryAdd("FrmMain.MnuToggleTopMost", "Keep window always on top"); //v3.2
        _ = TryAdd("FrmMain.MnuToggleTopMost._Enable", "Enabled window always on top"); // v9.0
        _ = TryAdd("FrmMain.MnuToggleTopMost._Disable", "Disabled window always on top"); // v9.0
        #endregion // Layout

        #region Tools
        _ = TryAdd("FrmMain.MnuTools", "Tools"); //v3.0
        _ = TryAdd("FrmMain.MnuColorPicker", "Color picker"); //v5.0
        _ = TryAdd("FrmMain.MnuFrameNav", "Frame navigation"); // v7.5
        _ = TryAdd("FrmMain.MnuCropTool", "Crop image"); // v7.6
        _ = TryAdd("FrmMain.MnuGetMoreTools", "Get more tools…"); // v9.0

        _ = TryAdd("FrmMain.MnuLosslessCompression", "Lossless compression"); // v9.0
        _ = TryAdd("FrmMain.MnuLosslessCompression._Description", "This tool uses Magick.NET library for lossless compression, optimizing file size. Overwrites only if the compressed file is smaller than the original.\r\n\r\nDo you want to proceed?"); // v9.0
        _ = TryAdd("FrmMain.MnuLosslessCompression._Compressing", "Performing lossless compression…"); // v9.0
        _ = TryAdd("FrmMain.MnuLosslessCompression._Done", "Done lossless compression.\r\nThe new file size is {0}, saved {1}."); // v9.0
        
        #endregion

        _ = TryAdd("FrmMain.MnuSettings", "Settings"); // v3.0

        #region Help
        _ = TryAdd("FrmMain.MnuHelp", "Help"); //v7.0
        _ = TryAdd("FrmMain.MnuAbout", "About"); //v3.0
        _ = TryAdd("FrmMain.MnuQuickSetup", "Open ImageGlass Quick Setup"); //v9.0
        _ = TryAdd("FrmMain.MnuCheckForUpdate._NewVersion", "A new version is available!"); //v5.0
        _ = TryAdd("FrmMain.MnuReportIssue", "Report an issue…"); //v3.0

        _ = TryAdd("FrmMain.MnuSetDefaultPhotoViewer", "Set default photo viewer"); //v9.0
        _ = TryAdd("FrmMain.MnuSetDefaultPhotoViewer._Success", "You have successfully set ImageGlass as default photo viewer."); //v9.0
        _ = TryAdd("FrmMain.MnuSetDefaultPhotoViewer._Error", "Could not set ImageGlass as default photo viewer."); //v9.0

        _ = TryAdd("FrmMain.MnuRemoveDefaultPhotoViewer", "Remove default photo viewer"); //v9.0
        _ = TryAdd("FrmMain.MnuRemoveDefaultPhotoViewer._Success", "ImageGlass is no longer the default photo viewer."); //v9.0
        _ = TryAdd("FrmMain.MnuRemoveDefaultPhotoViewer._Error", "Could not remove ImageGlass as the default photo viewer."); //v9.0

        #endregion

        _ = TryAdd("FrmMain.MnuExit", "Exit"); //v7.0

        #endregion

        #region Form message texts
        _ = TryAdd("FrmMain.PicMain._ErrorText", "Could not open this image"); // v2.0 beta, updated 4.0, 9.0
        _ = TryAdd("FrmMain.MnuMain", "Main menu"); // v3.0

        _ = TryAdd("FrmMain._OpenFileDialog", "All supported files");
        _ = TryAdd("FrmMain._Loading", "Loading…"); // v3.0
        _ = TryAdd("FrmMain._OpenWith", "Open with {0}"); //v9.0
        _ = TryAdd("FrmMain._ReachedFirstImage", "Reached the first image"); // v4.0
        _ = TryAdd("FrmMain._ReachedLastLast", "Reached the last image"); // v4.0
        _ = TryAdd("FrmMain._ClipboardImage", "Clipboard image"); //v9.0


        #endregion


        #endregion


        #region FrmAbout
        _ = TryAdd("FrmAbout._Slogan", "A lightweight, versatile image viewer");
        _ = TryAdd("FrmAbout._Version", "Version:");
        _ = TryAdd("FrmAbout._License", "Software license");
        _ = TryAdd("FrmAbout._Privacy", "Privacy policy");
        _ = TryAdd("FrmAbout._Thanks", "Special thanks to:");
        _ = TryAdd("FrmAbout._LogoDesigner", "Logo designer:");
        _ = TryAdd("FrmAbout._Collaborator", "Collaborator:");
        _ = TryAdd("FrmAbout._Contact", "Contact");
        _ = TryAdd("FrmAbout._Homepage", "Homepage:");
        _ = TryAdd("FrmAbout._Email", "Email:");
        _ = TryAdd("FrmAbout._Credits", "Credits");
        _ = TryAdd("FrmAbout._Donate", "Donate");
        #endregion


        #region FrmSettings

        #region Nav bar
        _ = TryAdd("FrmSettings.Nav._General", "General");
        _ = TryAdd("FrmSettings.Nav._Image", "Image");
        _ = TryAdd("FrmSettings.Nav._Slideshow", "Slideshow");
        _ = TryAdd("FrmSettings.Nav._Edit", "Edit");
        _ = TryAdd("FrmSettings.Nav._Viewer", "Viewer");
        _ = TryAdd("FrmSettings.Nav._Toolbar", "Toolbar");
        _ = TryAdd("FrmSettings.Nav._Gallery", "Gallery");
        _ = TryAdd("FrmSettings.Nav._Layout", "Layout");
        _ = TryAdd("FrmSettings.Nav._Mouse", "Mouse");
        _ = TryAdd("FrmSettings.Nav._Keyboard", "Keyboard");
        _ = TryAdd("FrmSettings.Nav._FileTypeAssociations", "File type associations");
        _ = TryAdd("FrmSettings.Nav._Tools", "Tools");
        _ = TryAdd("FrmSettings.Nav._Language", "Language");
        _ = TryAdd("FrmSettings.Nav._Appearance", "Appearance");
        #endregion // Nav bar


        #region Tab General
        // General > General
        _ = TryAdd("FrmSettings._StartupDir", "Startup location");
        _ = TryAdd("FrmSettings._ConfigDir", "Configuration location");
        _ = TryAdd("FrmSettings._UserConfigFile", "User settings file (igconfig.json)");

        // General > Startup
        _ = TryAdd("FrmSettings._Startup", "Startup");
        _ = TryAdd("FrmSettings._ShowWelcomeImage", "Show welcome image");
        _ = TryAdd("FrmSettings._ShouldOpenLastSeenImage", "Open the last seen image");

        // General > Real-time update
        _ = TryAdd("FrmSettings._RealTimeFileUpdate", "Real-time file update");
        _ = TryAdd("FrmSettings._EnableRealTimeFileUpdate", "Monitor file changes in the viewing folder and update in realtime");
        _ = TryAdd("FrmSettings._ShouldAutoOpenNewAddedImage", "Open the new added image automatically");

        // General > Others
        _ = TryAdd("FrmSettings._Others", "Others");
        _ = TryAdd("FrmSettings._AutoUpdate", "Check for update automatically");
        _ = TryAdd("FrmSettings._EnableMultiInstances", "Allow multiple instances of the program");
        _ = TryAdd("FrmSettings._ShowAppIcon", "Show app icon on the title bar");
        _ = TryAdd("FrmSettings._InAppMessageDuration", "In-app message duration (milliseconds)");
        _ = TryAdd("FrmSettings._ImageInfoTags", "Image information tags");
        _ = TryAdd("FrmSettings._AvailableImageInfoTags", "Available tags:");
        #endregion // Tab General


        #region Tab Image
        // Image > Image loading
        _ = TryAdd("FrmSettings._ImageLoading", "Image loading");
        _ = TryAdd("FrmSettings._ImageLoadingOrder", "Image loading order");
        _ = TryAdd("FrmSettings._ShouldUseExplorerSortOrder", "Use Windows File Explorer sort order if possible");
        _ = TryAdd("FrmSettings._EnableRecursiveLoading", "Load images in subfolders");
        _ = TryAdd("FrmSettings._ShouldGroupImagesByDirectory", "Group images by directory");
        _ = TryAdd("FrmSettings._ShouldLoadHiddenImages", "Load hidden images");
        _ = TryAdd("FrmSettings._EnableLoopBackNavigation", "Loop back to the first image when reaching the end of the image list");
        _ = TryAdd("FrmSettings._ShowImagePreview", "Display image preview while it's being loaded");
        _ = TryAdd("FrmSettings._EnableImageTransition", "Enable image transition effect");
        _ = TryAdd("FrmSettings._EnableImageAsyncLoading", "Enable image asynchronous loading");

        _ = TryAdd("FrmSettings._EmbeddedThumbnail", "Embedded thumbnail");
        _ = TryAdd("FrmSettings._UseEmbeddedThumbnailRawFormats", "Load only the embedded thumbnail for RAW formats");
        _ = TryAdd("FrmSettings._UseEmbeddedThumbnailOtherFormats", "Load only the embedded thumbnail for other formats");
        _ = TryAdd("FrmSettings._MinEmbeddedThumbnailSize", "Minimum size of the embedded thumbnail to be loaded");
        _ = TryAdd("FrmSettings._MinEmbeddedThumbnailSize._Width", "Width");
        _ = TryAdd("FrmSettings._MinEmbeddedThumbnailSize._Height", "Height");

        // Image > Image Booster
        _ = TryAdd("FrmSettings._ImageBooster", "Image Booster");
        _ = TryAdd("FrmSettings._ImageBoosterCacheCount", "Number of images cached by Image Booster (one direction)");
        _ = TryAdd("FrmSettings._ImageBoosterCacheMaxDimension", "Maximum image dimension to be cached (in pixels)");
        _ = TryAdd("FrmSettings._ImageBoosterCacheMaxFileSizeInMb", "Maximum image file size to be cached (in megabytes)");

        // Image > Color management
        _ = TryAdd("FrmSettings._ColorManagement", "Color management");
        _ = TryAdd("FrmSettings._ShouldUseColorProfileForAll", "Apply also for images without embedded color profile");
        _ = TryAdd("FrmSettings._ColorProfile", "Color profile");
        _ = TryAdd("FrmSettings._CurrentMonitorProfile._Description", "ImageGlass does not auto-update the color when moving its window between monitors");
        #endregion // Tab Image


        #region Tab Slideshow
        // Slideshow > Slideshow
        _ = TryAdd("FrmSettings._HideMainWindowInSlideshow", "Automatically hide main window");
        _ = TryAdd("FrmSettings._ShowSlideshowCountdown", "Show slideshow countdown");
        _ = TryAdd("FrmSettings._EnableFullscreenSlideshow", "Start slideshow in Full Screen mode");
        _ = TryAdd("FrmSettings._UseRandomIntervalForSlideshow", "Use random interval");
        _ = TryAdd("FrmSettings._SlideshowInterval", "Slideshow interval:");
        _ = TryAdd("FrmSettings._SlideshowInterval._From", "From");
        _ = TryAdd("FrmSettings._SlideshowInterval._To", "To");
        _ = TryAdd("FrmSettings._SlideshowBackgroundColor", "Slideshow background color");

        // Slideshow > Slideshow notification
        _ = TryAdd("FrmSettings._SlideshowNotification", "Slideshow notification");
        _ = TryAdd("FrmSettings._SlideshowImagesToNotifySound", "Number of images to trigger a notification sound");
        #endregion // Tab Slideshow


        #region Tab Edit
        // Edit > Edit
        _ = TryAdd("FrmSettings._ShowDeleteConfirmation", "Show confirmation dialog when deleting file");
        _ = TryAdd("FrmSettings._ShowSaveOverrideConfirmation", "Show confirmation dialog when overriding file");
        _ = TryAdd("FrmSettings._ShouldPreserveModifiedDate", "Preserve the image's modified date on save");
        _ = TryAdd("FrmSettings._ImageEditQuality", "Image quality");
        _ = TryAdd("FrmSettings._AfterEditingAction", "After opening editing app");

        // Edit > Clipboard
        _ = TryAdd("FrmSettings._Clipboard", "Clipboard");
        _ = TryAdd("FrmSettings._EnableCopyMultipleFiles", "Enable the copying of multiple files at once");
        _ = TryAdd("FrmSettings._EnableCutMultipleFiles", "Enable the cutting of multiple files at once");

        // Edit > Image editing apps
        _ = TryAdd("FrmSettings._EditApps", "Image editing apps");
        _ = TryAdd("FrmSettings._EditApps._AppName", "App name");
        _ = TryAdd("FrmSettings.EditAppDialog._AddApp", "Add an app for editing");
        _ = TryAdd("FrmSettings.EditAppDialog._EditApp", "Edit app");

        #endregion // Tab Edit


        #region Tab Layout
        // Layout > Layout
        _ = TryAdd("FrmSettings.Layout._Order", "Order");
        _ = TryAdd("FrmSettings.Layout._Toolbar", "Toolbar");
        _ = TryAdd("FrmSettings.Layout._ToolbarContext", "Contextual toolbar");
        _ = TryAdd("FrmSettings.Layout._Gallery", "Gallery");
        _ = TryAdd("FrmSettings.Layout._ToolbarPosition", "Toolbar position");
        _ = TryAdd("FrmSettings.Layout._ToolbarContextPosition", "Contextual toolbar position");
        _ = TryAdd("FrmSettings.Layout._GalleryPosition", "Gallery position");
        #endregion // Tab Layout


        #region Tab Viewer
        // Viewer > Viewer
        _ = TryAdd("FrmSettings._ShowCheckerboardOnlyImageRegion", "Show checkerboard only within the image region");
        _ = TryAdd("FrmSettings._EnableNavigationButtons", "Show navigation arrow buttons");
        _ = TryAdd("FrmSettings._CenterWindowFit", "Automatically center the window in Window Fit mode");
        _ = TryAdd("FrmSettings._UseWebview2ForSvg", "Use Webview2 for viewing SVG format");
        _ = TryAdd("FrmSettings._PanSpeed", "Panning speed");

        // Viewer > Zooming
        _ = TryAdd("FrmSettings._Zooming", "Zooming");
        _ = TryAdd("FrmSettings._ImageInterpolation", "Image interpolation");
        _ = TryAdd("FrmSettings._ImageInterpolation._ScaleDown", "When zoom < 100%");
        _ = TryAdd("FrmSettings._ImageInterpolation._ScaleUp", "When zoom > 100%");
        _ = TryAdd("FrmSettings._ZoomSpeed", "Zoom speed");
        _ = TryAdd("FrmSettings._ZoomLevels", "Zoom levels");
        _ = TryAdd("FrmSettings._UseSmoothZooming", "Use smooth zooming");
        _ = TryAdd("FrmSettings._LoadDefaultZoomLevels", "Load default zoom levels");
        #endregion // Tab Viewer


        #region Tab Toolbar
        // Toolbar > Toolbar
        _ = TryAdd("FrmSettings.Toolbar._HideToolbarInFullscreen", "Hide toolbar in Full Screen mode");
        _ = TryAdd("FrmSettings.Toolbar._EnableCenterToolbar", "Use center alignment for toolbar");
        _ = TryAdd("FrmSettings.Toolbar._ToolbarIconHeight", "Toolbar icon size");

        _ = TryAdd("FrmSettings.Toolbar._AddNewButton", "Add a custom toolbar button");
        _ = TryAdd("FrmSettings.Toolbar._EditButton", "Edit toolbar button");
        _ = TryAdd("FrmSettings.Toolbar._ButtonJson", "Button JSON");


        _ = TryAdd("FrmSettings.Toolbar._ToolbarButtons", "Toolbar buttons");
        _ = TryAdd("FrmSettings.Toolbar._AddCustomButton", "Add a custom button…");
        _ = TryAdd("FrmSettings.Toolbar._AvailableButtons", "Available buttons:");
        _ = TryAdd("FrmSettings.Toolbar._CurrentButtons", "Current buttons:");
        _ = TryAdd("FrmSettings.Toolbar._Errors._ButtonIdRequired", "Button ID required.");
        _ = TryAdd("FrmSettings.Toolbar._Errors._ButtonIdDuplicated", "A button with the ID '{0}' has already been defined. Please choose a different and unique ID for your button to avoid conflicts.");
        _ = TryAdd("FrmSettings.Toolbar._Errors._ButtonExecutableRequired", "Button executable required.");

        #endregion // TAB Toolbar


        #region Tab Gallery
        // Gallery > Gallery
        _ = TryAdd("FrmSettings._HideGalleryInFullscreen", "Hide gallery in Full Screen mode");
        _ = TryAdd("FrmSettings._ShowGalleryScrollbars", "Show gallery scrollbars");
        _ = TryAdd("FrmSettings._ShowGalleryFileName", "Show thumbnail filename");
        _ = TryAdd("FrmSettings._ThumbnailSize", "Thumbnail size (in pixels)");
        _ = TryAdd("FrmSettings._GalleryCacheSizeInMb", "Maximum gallery cache size (in megabytes)");
        _ = TryAdd("FrmSettings._GalleryColumns", "Number of thumbnail columns in vertical gallery layout");
        #endregion // Tab Gallery


        #region Tab Mouse
        // Mouse > Mouse wheel action
        _ = TryAdd("FrmSettings._MouseWheelAction", "Mouse wheel action");
        #endregion // Tab Mouse


        #region Tab Keyboard

        #endregion // Tab Mouse & Keyboard


        #region Tab File type associations
        // File type associations > File extension icons
        _ = TryAdd("FrmSettings._FileExtensionIcons", "File extension icons");
        _ = TryAdd("FrmSettings._FileExtensionIcons._Description", "For customizing file extension icons, download an icon pack, place all .ICO files in the extension icon folder, and click the '{0}' button. This will also set ImageGlass as default photo viewer.");
        _ = TryAdd("FrmSettings._OpenExtensionIconFolder", "Open extension icon folder");
        _ = TryAdd("FrmSettings._GetExtensionIconPacks", "Get extension icon packs…");

        // File type associations > Default photo viewer
        _ = TryAdd("FrmSettings._DefaultPhotoViewer", "Default photo viewer");
        _ = TryAdd("FrmSettings._DefaultPhotoViewer._Description", "You can set ImageGlass as your default photo viewer using the buttons below. Remember to manually reset it if you uninstall ImageGlass, as the installer does not handle this task automatically.");
        _ = TryAdd("FrmSettings._MakeDefault", "Make default");
        _ = TryAdd("FrmSettings._RemoveDefault", "Remove default");
        _ = TryAdd("FrmSettings._OpenDefaultAppsSetting", "Open Default apps setting");

        // File type associations > File formats
        _ = TryAdd("FrmSettings._FileFormats", "File formats");
        _ = TryAdd("FrmSettings._TotalSupportedFormats", "Total supported formats: {0}");
        _ = TryAdd("FrmSettings._AddNewFileExtension", "Add new file extension");

        #endregion // Tab File type associations


        #region Tab Tools
        // Tools > Tools
        _ = TryAdd("FrmSettings.Tools._AddNewTool", "Add an external tool");
        _ = TryAdd("FrmSettings.Tools._EditTool", "Edit external tool");
        _ = TryAdd("FrmSettings.Tools._Integrated", "Integrated");
        _ = TryAdd("FrmSettings.Tools._IntegratedWith", "Integrated with {0}");
        #endregion // Tab Tools


        #region Tab Language
        // Language > Language
        _ = TryAdd("FrmSettings._DisplayLanguage", "Display language");
        _ = TryAdd("FrmSettings._Refresh", "Refresh");
        _ = TryAdd("FrmSettings._InstallNewLanguagePack", "Install new language packs…");
        _ = TryAdd("FrmSettings._GetMoreLanguagePacks", "Get more language packs…");
        _ = TryAdd("FrmSettings._ExportLanguagePack", "Export language pack…");
        _ = TryAdd("FrmSettings._Contributors", "Contributors");
        #endregion // Tab Language


        #region Tab Appearance
        // Appearance > Appearance
        _ = TryAdd("FrmSettings._WindowBackdrop", "Window backdrop");
        _ = TryAdd("FrmSettings._BackgroundColor", "Viewer background color");

        // Appearance > Theme
        _ = TryAdd("FrmSettings._Theme", "Theme");
        _ = TryAdd("FrmSettings._DarkTheme", "Dark");
        _ = TryAdd("FrmSettings._LightTheme", "Light");
        _ = TryAdd("FrmSettings._Author", "Author");
        _ = TryAdd("FrmSettings._Theme._OpenThemeFolder", "Open theme folder");
        _ = TryAdd("FrmSettings._Theme._GetMoreThemes", "Get more theme packs…");
        _ = TryAdd("FrmSettings._Theme._InstallTheme", "Install theme packs");
        _ = TryAdd("FrmSettings._Theme._UninstallTheme", "Uninstall a theme pack");

        _ = TryAdd("FrmSettings._UseThemeForDarkMode", "Use this theme for dark mode");
        _ = TryAdd("FrmSettings._UseThemeForLightMode", "Use this theme for light mode");
        #endregion // Tab Appearance


        #endregion // FrmSettings


        #region FrmCrop
        _ = TryAdd("FrmCrop.LblAspectRatio", "Aspect ratio:"); //v9.0
        _ = TryAdd("FrmCrop.LblLocation", "Location:"); //v9.0
        _ = TryAdd("FrmCrop.LblSize", "Size:"); //v9.0

        _ = TryAdd("FrmCrop.SelectionAspectRatio._FreeRatio", "Free ratio"); //v9.0
        _ = TryAdd("FrmCrop.SelectionAspectRatio._Custom", "Custom…"); //v9.0
        _ = TryAdd("FrmCrop.SelectionAspectRatio._Original", "Original"); //v9.0

        _ = TryAdd("FrmCrop.BtnQuickSelect._Tooltip", "Quick select…"); //v9.0
        _ = TryAdd("FrmCrop.BtnReset._Tooltip", "Reset selection"); //v9.0
        _ = TryAdd("FrmCrop.BtnSettings._Tooltip", "Open Crop tool settings"); //v9.0

        _ = TryAdd("FrmCrop.BtnSave", "Save"); //v9.0
        _ = TryAdd("FrmCrop.BtnSave._Tooltip", "Save image"); //v9.0
        _ = TryAdd("FrmCrop.BtnSaveAs", "Save as…"); //v9.0
        _ = TryAdd("FrmCrop.BtnSaveAs._Tooltip", "Save as a copy…"); //v9.0
        _ = TryAdd("FrmCrop.BtnCrop", "Crop"); //v9.0
        _ = TryAdd("FrmCrop.BtnCrop._Tooltip", "Crop the image only"); //v9.0
        _ = TryAdd("FrmCrop.BtnCopy", "Copy"); //v9.0
        _ = TryAdd("FrmCrop.BtnCopy._Tooltip", "Copy the selection to clipboard"); //v9.0


        // Crop settings
        _ = TryAdd("FrmCropSettings._Title", "Crop settings"); //v9.0
        _ = TryAdd("FrmCropSettings.ChkCloseToolAfterSaving", "Close Crop tool after saving"); //v9.0
        _ = TryAdd("FrmCropSettings.LblDefaultSelection", "Default selection"); //v9.0
        _ = TryAdd("FrmCropSettings.ChkAutoCenterSelection", "Auto-center selection"); //v9.0

        _ = TryAdd("FrmCropSettings.DefaultSelectionType._UseTheLastSelection", "Use the last selection"); //v9.0
        _ = TryAdd("FrmCropSettings.DefaultSelectionType._SelectNone", "Select none"); //v9.0
        _ = TryAdd("FrmCropSettings.DefaultSelectionType._SelectX", "Select {0}"); //v9.0
        _ = TryAdd("FrmCropSettings.DefaultSelectionType._SelectAll", "Select all"); //v9.0
        _ = TryAdd("FrmCropSettings.DefaultSelectionType._CustomArea", "Custom area…"); //v9.0

        #endregion


        #region FrmColorPicker

        _ = TryAdd("FrmColorPicker.BtnSettings._Tooltip", "Open Color picker settings…"); //v9.0

        // Color picker settings
        _ = TryAdd("FrmColorPickerSettings._Title", "Color picker settings"); //v9.0
        _ = TryAdd("FrmColorPickerSettings.ChkShowRgbA", "Use RGB format with alpha value"); //v5.0
        _ = TryAdd("FrmColorPickerSettings.ChkShowHexA", "Use HEX format with alpha value"); //v5.0
        _ = TryAdd("FrmColorPickerSettings.ChkShowHslA", "Use HSL format with alpha value"); //v5.0
        _ = TryAdd("FrmColorPickerSettings.ChkShowHsvA", "Use HSV format with alpha value"); //v8.0
        _ = TryAdd("FrmColorPickerSettings.ChkShowCIELabA", "Use CIELAB format with alpha value"); //v9.0

        #endregion


        #region FrmToolNotFound
        _ = TryAdd("FrmToolNotFound._Title", "Tool not found"); // v9.0
        _ = TryAdd("FrmToolNotFound.BtnSelectExecutable", "Select…"); // v9.0
        _ = TryAdd("FrmToolNotFound.LblHeading", "'{0}' is not found!"); // v9.0
        _ = TryAdd("FrmToolNotFound.LblDescription", "ImageGlass was unable to locate the path to the '{0}' executable. To resolve this issue, please update the path to the '{0}' as necessary."); // v9.0
        _ = TryAdd("FrmToolNotFound.LblDownloadToolText", "You can download more tools for ImageGlass at:"); // v9.0
        #endregion // FrmToolNotFound


        #region FrmHotkeyPicker
        _ = TryAdd("FrmHotkeyPicker.LblHotkey", "Press hotkeys"); // v9.0
        #endregion // FrmHotkeyPicker


        #region igcmd.exe

        _ = TryAdd("_._IgCommandExe._DefaultError._Heading", "Invalid commands"); //v9.0
        _ = TryAdd("_._IgCommandExe._DefaultError._Description", "Make sure you pass the correct commands!\r\nThis executable file contains command-line functions for ImageGlass software.\r\n\r\nTo explore all command lines, please visit:\r\n{0}"); //v9.0


        #region FrmSlideshow

        _ = TryAdd("FrmSlideshow._PauseSlideshow", "Slideshow is paused."); // v9.0
        _ = TryAdd("FrmSlideshow._ResumeSlideshow", "Slideshow is resumed."); // v9.0

        // menu
        _ = TryAdd("FrmSlideshow.MnuPauseResumeSlideshow", "Pause/resume slideshow"); // v9.0
        _ = TryAdd("FrmSlideshow.MnuExitSlideshow", "Exit slideshow"); // v9.0
        _ = TryAdd("FrmSlideshow.MnuChangeBackgroundColor", "Change background color…"); // v9.0

        _ = TryAdd("FrmSlideshow.MnuToggleCountdown", "Show slideshow countdown"); // v9.0
        _ = TryAdd("FrmSlideshow.MnuZoomModes", "Zoom modes"); // v9.0

        #endregion


        #region FrmExportFrames
        _ = TryAdd("FrmExportFrames._Title", "Export image frames"); //v9.0
        _ = TryAdd("FrmExportFrames._FileNotExist", "Image file does not exist"); //v7.5
        _ = TryAdd("FrmExportFrames._FolderPickerTitle", "Select output folder for exporting image frames"); //v9.0
        _ = TryAdd("FrmExportFrames._Exporting", "Exporting {0}/{1} frames \r\n{2}…"); //v9.0
        _ = TryAdd("FrmExportFrames._ExportDone", "Exported {0} frames successfully to \r\n{1}"); //v9.0
        _ = TryAdd("FrmExportFrames._OpenOutputFolder", "Open output folder"); //v9.0
        #endregion


        #region FrmUpdate
        _ = TryAdd("FrmUpdate._StatusChecking", "Checking for update…"); //v9.0
        _ = TryAdd("FrmUpdate._StatusUpdated", "You are using the latest version!"); //v9.0
        _ = TryAdd("FrmUpdate._StatusOutdated", "A new update is available!"); //v9.0
        _ = TryAdd("FrmUpdate._CurrentVersion", "Current version: {0}"); //v9.0
        _ = TryAdd("FrmUpdate._LatestVersion", "The latest version: {0}"); //v9.0
        _ = TryAdd("FrmUpdate._PublishedDate", "Published date: {0}"); //v9.0
        #endregion


        #region FrmQuickSetup

        _ = TryAdd("FrmQuickSetup._Text", "ImageGlass Quick Setup"); //v9.0
        _ = TryAdd("FrmQuickSetup._StepInfo", "Step {0}"); //v9.0
        _ = TryAdd("FrmQuickSetup._SkipQuickSetup", "Skip this and launch ImageGlass"); //v9.0

        _ = TryAdd("FrmQuickSetup._SeeWhatNew", "See what's new in this version…"); // v9.0
        _ = TryAdd("FrmQuickSetup._SelectProfile", "Select a profile"); //v9.0
        _ = TryAdd("FrmQuickSetup._StandardUser", "Standard user"); //v9.0
        _ = TryAdd("FrmQuickSetup._ProfessionalUser", "Professional user"); //v9.0
        _ = TryAdd("FrmQuickSetup._SettingProfileDescription", "To modify these settings, simply access app settings."); // v9.0

        _ = TryAdd("FrmQuickSetup._SettingsWillBeApplied", "Settings will be applied:"); //v9.0
        _ = TryAdd("FrmQuickSetup._SetDefaultViewer", "Do you want to set ImageGlass as the default photo viewer?"); //v9.0
        _ = TryAdd("FrmQuickSetup._SetDefaultViewer._Description", "You can reset it in the app settings > File type associations tab."); //v9.0

        _ = TryAdd("FrmQuickSetup._ConfirmCloseProcess", "Before applying the new settings, it's essential to close all ImageGlass processes. Are you ready to proceed?"); //v7.5

        #endregion

        #endregion

    }

}