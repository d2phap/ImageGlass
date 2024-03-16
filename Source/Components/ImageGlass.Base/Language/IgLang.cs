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
using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace ImageGlass.Base;


/// <summary>
/// ImageGlass language pack (<c>*.iglang.json</c>)
/// </summary>
public class IgLang : IDictionary<string, string>
{
    private FrozenDictionary<string, string> _dict = FrozenDictionary<string, string>.Empty;


    // IDictionary interface implementation
    #region IDictionary interface implementation
    public string this[string key]
    {
        get => _dict[key];

        [Obsolete("Cannot change an item in a readonly Dictionary", true)]
        set { }
    }

    public ICollection<string> Keys => _dict.Keys;

    public ICollection<string> Values => _dict.Values;

    public int Count => _dict.Count;

    public bool IsReadOnly => true;

    public void Add(string key, string value)
    {
        _ = _dict.TryAdd(key, value);
    }

    public void Add(KeyValuePair<string, string> item)
    {
        _ = _dict.TryAdd(item.Key, item.Value);
    }

    public bool TryAdd(string key, string value)
    {
        return _dict.TryAdd(key, value);
    }

    public bool TryAdd(KeyValuePair<string, string> item)
    {
        return _dict.TryAdd(item.Key, item.Value);
    }

    public void Clear()
    {
        _dict = FrozenDictionary<string, string>.Empty;
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
        return _dict.Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return _dict.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        _dict.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _dict.GetEnumerator();
    }

    [Obsolete("Cannot remove an item in a readonly Dictionary", true)]
    public bool Remove(string key)
    {
        return false;
    }

    [Obsolete("Cannot remove an item in a readonly Dictionary", true)]
    public bool Remove(KeyValuePair<string, string> item)
    {
        return false;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return _dict.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict.GetEnumerator();
    }
    #endregion // IDictionary interface implementation



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
        _dict = InitDefaultLanguage().ToFrozenDictionary();
    }


    /// <summary>
    /// Initializes language with filename
    /// </summary>
    /// <param name="fileName">E.g. <c>Vietnamese.iglang.json</c></param>
    /// <param name="dirPath">The directory path contains language file (for relative filename)</param>
    public IgLang(string fileName, string dirPath = "")
    {
        var defaultLang = InitDefaultLanguage();
        var filePath = Path.Combine(dirPath, fileName);

        if (File.Exists(filePath))
        {
            FilePath = filePath;
            ReadFromFile(ref defaultLang);
        }

        _dict = defaultLang.ToFrozenDictionary();
    }


    /// <summary>
    /// Loads language strings from JSON file
    /// </summary>
    public void ReadFromFile(ref Dictionary<string, string> defaultLang)
    {
        var model = BHelper.ReadJson<IgLangJsonModel>(FilePath);
        if (model == null) return;

        Metadata = model._Metadata;

        // import language items
        foreach (var item in model.Items)
        {
            if (string.IsNullOrEmpty(item.Value)) continue;

            if (defaultLang.ContainsKey(item.Key))
            {
                defaultLang[item.Key] = item.Value;
            }
            else
            {
                _ = defaultLang.TryAdd(item.Key, item.Value);
            }
        }
    }


    /// <summary>
    /// Saves current language to JSON file
    /// </summary>
    public async Task SaveAsFileAsync(string filePath)
    {
        var model = new IgLangJsonModel(Metadata, _dict);

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
    public Dictionary<string, string> InitDefaultLanguage()
    {
        return new()
        {
            #region General
            { "_._OK", "OK" }, // v9.0
            { "_._Cancel", "Cancel" }, // v9.0
            { "_._Apply", "Apply" }, // v9.0
            { "_._Close", "Close" }, // v9.0
            { "_._Yes", "Yes" }, // v9.0
            { "_._No", "No" }, // v9.0
            { "_._LearnMore", "Learn more…" }, // v9.0
            { "_._Continue", "Continue" }, // v9.0
            { "_._Quit", "Quit" }, // v9.0
            { "_._Back", "Back" }, // v9.0
            { "_._Next", "Next" }, // v9.0
            { "_._Save", "Save" }, // v9.0
            { "_._Error", "Error" }, // v9.0
            { "_._Warning", "Warning" }, // v9.0
            { "_._Copy", "Copy" }, //v9.0
            { "_._Browse", "Browse…" }, //v9.0
            { "_._Reset", "Reset" }, //v9.0
            { "_._ResetToDefault", "Reset to default" }, //v9.0
            { "_._CheckForUpdate", "Check for update…" }, //v5.0
            { "_._Download", "Download" }, //v9.0
            { "_._Update", "Update" }, //v9.0
            { "_._Website", "Website" }, //v9.0
            { "_._Email", "Email" }, //v9.0
            { "_._Install", "Install…" },
            { "_._Refresh", "Refresh" },
            { "_._Delete", "Delete" },
            { "_._Add", "Add" },
            { "_._Add+", "Add…" },
            { "_._Edit", "Edit" },
            { "_._ID", "ID" },
            { "_._Name", "Name" },
            { "_._Hotkeys", "Hotkeys" },
            { "_._AddHotkey", "Add hotkey…" },
            { "_._Executable", "Executable" },
            { "_._Argument", "Argument" },
            { "_._CommandPreview", "Command preview" },
            { "_._FileExtension", "File extension" },
            { "_._Empty", "(empty)" },
            { "_._MoveUp", "Move up" },
            { "_._MoveDown", "Move down" },
            { "_._Separator", "Separator" },
            { "_._Icon", "Icon" },
            { "_._Description", "Description" },
            { "_._GetHelp", "Get help" },

            { "_._UnhandledException", "Unhandled exception" }, // v9.0
            { "_._UnhandledException._Description", "Unhandled exception has occurred. If you click Continue, the application will ignore this error and attempt to continue. If you click Quit, the application will close immediately." }, // v9.0
            { "_._DoNotShowThisMessageAgain", "Do not show this message again" }, // v9.0
            { $"_._CreatingFile", "Creating a temporary image file…" }, //v9.0
            { $"_._CreatingFileError", "Could not create temporary image file" }, //v9.0
            { $"_._NotSupported", "Unsupported format" }, //v9.0

            { $"_._InvalidAction", "Invalid action" }, //v9.0
            { $"_._InvalidAction._Transformation", "ImageGlass does not support rotation, flipping for this image." }, //v9.0


            { "_._UserAction._MenuNotFound", "Cannot find menu '{0}' to invoke the action" }, // v9.0
            { "_._UserAction._MethodNotFound", "Cannot find method '{0}' to invoke the action" }, // v9.0
            { "_._UserAction._MethodArgumentNotSupported", "The argument type of method '{0}' is not supported" }, // v9.0
            { "_._UserAction._Win32ExeError", "Cannot execute command '{0}'. Make sure the name is correct." }, // v9.0

            { "_._Webview2._NotFound", "Please install the latest version of WebView2 Runtime." }, // 9.1

            // Gallery tooltip
            { $"_.Metadata._{nameof(IgMetadata.FileSize)}", "File size" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.FileCreationTime)}", "Date created" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.FileLastAccessTime)}", "Date accessed" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.FileLastWriteTime)}", "Date modified" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.FrameCount)}", "Frames" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.ExifRatingPercent)}", "Rating" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.ColorSpace)}", "Color space" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.ColorProfile)}", "Color profile" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.ExifDateTime)}", "EXIF DateTime" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.ExifDateTimeOriginal)}", "EXIF DateTimeOriginal" }, //v9.0
            { $"_.Metadata._{nameof(IgMetadata.Date)}", "Date" }, //v9.0

            // image info
            { $"_.{nameof(ImageInfo)}._{nameof(ImageInfo.ListCount)}", "{0} file(s)" }, //v9.0
            { $"_.{nameof(ImageInfo)}._{nameof(ImageInfo.FrameCount)}", "{0} frame(s)" }, //v9.0

            // layout position
            { $"_.Position._Left", "Left" },
            { $"_.Position._Right", "Right" },
            { $"_.Position._Top", "Top" },
            { $"_.Position._Bottom", "Bottom" },

            #endregion // General


            #region Enums

            // ImageOrderBy
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Name)}", "Name (default)" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Random)}", "Random" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.FileSize)}", "File size" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Extension)}", "Extension" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.Date)}", "Date" }, //v9.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateCreated)}", "Date created" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateAccessed)}", "Date accessed" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.DateModified)}", "Date modified" }, //v8.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.ExifDateTaken)}", "EXIF: Date taken" }, //v9.0
            { $"_.{nameof(ImageOrderBy)}._{nameof(ImageOrderBy.ExifRating)}", "EXIF: Rating" }, //v9.0


            // ImageOrderType
            { $"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Asc)}", "Ascending" },  //v8.0
            { $"_.{nameof(ImageOrderType)}._{nameof(ImageOrderType.Desc)}", "Descending" },  //v8.0

            // AfterEditAppAction
            { $"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Nothing)}", "Nothing" }, //v8.0
            { $"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Minimize)}", "Minimize" }, //v8.0
            { $"_.{nameof(AfterEditAppAction)}._{nameof(AfterEditAppAction.Close)}", "Close" }, //v8.0

            // ColorProfileOption
            { $"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.None)}", "None" },
            { $"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.CurrentMonitorProfile)}", "Current monitor profile" },
            { $"_.{nameof(ColorProfileOption)}._{nameof(ColorProfileOption.Custom)}", "Custom…" },

            // BackdropStyle
            { $"_.{nameof(BackdropStyle)}._{nameof(BackdropStyle.None)}", "None" },

            // MouseWheelEvent
            { $"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.Scroll)}", "Scroll" },
            { $"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.CtrlAndScroll)}", "Hold Ctrl and scroll" },
            { $"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.ShiftAndScroll)}", "Hold Shift and scroll" },
            { $"_.{nameof(MouseWheelEvent)}._{nameof(MouseWheelEvent.AltAndScroll)}", "Hold Alt and scroll" },

            // MouseWheelAction
            { $"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.DoNothing)}", "Do nothing" },
            { $"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.Zoom)}", "Zoom in / out" },
            { $"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.PanVertically)}", "Pan up / down" },
            { $"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.PanHorizontally)}", "Pan left / right" },
            { $"_.{nameof(MouseWheelAction)}._{nameof(MouseWheelAction.BrowseImages)}", "View next / previous Image" },

            // ImageInterpolation
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.NearestNeighbor)}", "Nearest neighbor" },
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Linear)}", "Linear" },
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Cubic)}", "Cubic" },
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.MultiSampleLinear)}", "Multi-sample linear" },
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.Antisotropic)}", "Antisotropic" },
            { $"_.{nameof(ImageInterpolation)}._{nameof(ImageInterpolation.HighQualityBicubic)}", "High quality bicubic" },

            #endregion // Enums


            #region FrmMain

            #region Main menu

            #region File
            { "FrmMain.MnuFile", "File" }, //v7.0
            { "FrmMain.MnuOpenFile", "Open file…" }, //v3.0
            { "FrmMain.MnuNewWindow", "Open new window" }, //v7.0
            { "FrmMain.MnuNewWindow._Error", "Cannot open new window because only one instance is allowed" }, //v7.0
            { "FrmMain.MnuSave", "Save" }, //v8.1
            { "FrmMain.MnuSave._Confirm", "Are you sure you want to override this image?" }, //v9.0
            { "FrmMain.MnuSave._ConfirmDescription", "ImageGlass is not a professional photo editor, please be aware of losing the quality, metadata, layers,… when saving your image." }, //v9.0
            { "FrmMain.MnuSave._Saving", "Saving image…" }, //v9.0
            { "FrmMain.MnuSave._Success", "Image is saved" }, //v9.0
            { "FrmMain.MnuSave._Error", "Could not save the image" }, //v9.0
            { "FrmMain.MnuSaveAs", "Save as…" }, //v3.0
            { "FrmMain.MnuRefresh", "Refresh" }, //v3.0
            { "FrmMain.MnuReload", "Reload image" }, //v5.5
            { "FrmMain.MnuReloadImageList", "Reload image list" }, //v7.0
            { "FrmMain.MnuUnload", "Unload image" }, //v9.0
            { "FrmMain.MnuOpenWith", "Open with…" }, //v7.6
            { "FrmMain.MnuEdit", "Edit image {0}…" }, //v3.0,
            { "FrmMain.MnuEdit._AppNotFound", "Could not find the associated app for editing. You can assign an app for editing this format in ImageGlass Settings > Edit." }, //v9.0
            { "FrmMain.MnuPrint", "Print…" }, //v3.0
            { "FrmMain.MnuPrint._Error", "Could not print the viewing image" }, //v9.0
            { "FrmMain.MnuShare", "Share…" }, //v8.6
            { "FrmMain.MnuShare._Error", "Could not open Share dialog." }, //v9.0
            #endregion

            #region Navigation
            { "FrmMain.MnuNavigation", "Navigation" }, //v3.0
            { "FrmMain.MnuViewNext", "View next image" }, //v3.0
            { "FrmMain.MnuViewPrevious", "View previous image" }, //v3.0

            { "FrmMain.MnuGoTo", "Go to…" }, //v3.0
            { "FrmMain.MnuGoTo._Description", "Enter the image index to view, and then press ENTER" },
            { "FrmMain.MnuGoToFirst", "Go to first image" }, //v3.0
            { "FrmMain.MnuGoToLast", "Go to last image" }, //v3.0

            { "FrmMain.MnuViewNextFrame", "View next frame" }, //v7.5
            { "FrmMain.MnuViewPreviousFrame", "View previous frame" }, //v7.5
            { "FrmMain.MnuViewFirstFrame", "View first frame" }, //v7.5
            { "FrmMain.MnuViewLastFrame", "View last frame" }, //v7.5
            #endregion // Navigation

            #region Zoom
            { "FrmMain.MnuZoom", "Zoom" }, //v7.0
            { "FrmMain.MnuZoomIn", "Zoom in" }, //v3.0
            { "FrmMain.MnuZoomOut", "Zoom out" }, //v3.0
            { "FrmMain.MnuCustomZoom", "Custom zoom…" }, // v8.3
            { "FrmMain.MnuCustomZoom._Description", "Enter a new zoom value" }, // v8.3
            { "FrmMain.MnuScaleToFit", "Scale to fit" }, //v3.5
            { "FrmMain.MnuScaleToFill", "Scale to fill" }, //v7.5
            { "FrmMain.MnuActualSize", "Actual size" }, //v3.0
            { "FrmMain.MnuLockZoom", "Lock zoom ratio" }, //v3.0
            { "FrmMain.MnuAutoZoom", "Auto zoom" }, //v5.5
            { "FrmMain.MnuScaleToWidth", "Scale to width" }, //v3.0
            { "FrmMain.MnuScaleToHeight", "Scale to height" }, //v3.0
            #endregion

            #region Panning
            { "FrmMain.MnuPanning", "Panning" }, //v9.0

            { "FrmMain.MnuPanLeft", "Pan image left" }, //v9.0
            { "FrmMain.MnuPanRight", "Pan image right" }, //v9.0
            { "FrmMain.MnuPanUp", "Pan image up" }, //v9.0
            { "FrmMain.MnuPanDown", "Pan image down" }, //v9.0

            { "FrmMain.MnuPanToLeftSide", "Pan image to left edge" }, //v9.0
            { "FrmMain.MnuPanToRightSide", "Pan image to right edge" }, //v9.0
            { "FrmMain.MnuPanToTop", "Pan image to top" }, //v9.0
            { "FrmMain.MnuPanToBottom", "Pan image to bottom" }, //v9.0
            #endregion // Panning

            #region Image
            { "FrmMain.MnuImage", "Image" }, //v7.0

            { "FrmMain.MnuViewChannels", "View channels" }, //v7.0
            { "FrmMain.MnuViewChannels._All", "All" }, //v7.0
            { "FrmMain.MnuViewChannels._Red", "Red" }, //v7.0
            { "FrmMain.MnuViewChannels._Green", "Green" }, //v7.0
            { "FrmMain.MnuViewChannels._Blue", "Blue" }, //v7.0
            { "FrmMain.MnuViewChannels._Black", "Black" }, //v7.0
            { "FrmMain.MnuViewChannels._Alpha", "Alpha" }, //v7.0

            { "FrmMain.MnuLoadingOrders", "Loading orders" }, //v8.0

            { "FrmMain.MnuRotateLeft", "Rotate left" }, //v7.5
            { "FrmMain.MnuRotateRight", "Rotate right" }, //v7.5
            { "FrmMain.MnuFlipHorizontal", "Flip Horizontal" }, // V6.0
            { "FrmMain.MnuFlipVertical", "Flip Vertical" }, // V6.0
            { "FrmMain.MnuRename", "Rename image…" }, //v3.0
            { "FrmMain.MnuRename._Description", "Enter a new filename:" }, // v9.0
            { "FrmMain.MnuMoveToRecycleBin", "Move to the Recycle Bin" }, //v3.0
            { "FrmMain.MnuMoveToRecycleBin._Description", "Do you want to move this file to the Recycle bin?" }, //v3.0
            { "FrmMain.MnuDeleteFromHardDisk", "Delete permanently" }, //v3.0
            { "FrmMain.MnuDeleteFromHardDisk._Description", "Are you sure you want to permanently delete this file?" }, //v3.0
            { "FrmMain.MnuExportFrames", "Export image frames…" }, //v7.5
            { "FrmMain.MnuToggleImageAnimation", "Start / stop animating image" }, //v3.0
            { "FrmMain.MnuSetDesktopBackground", "Set as Desktop background" }, //v3.0
            { "FrmMain.MnuSetDesktopBackground._Error", "Could not set the viewing image as desktop background" }, // v6.0
            { "FrmMain.MnuSetDesktopBackground._Success", "Desktop background is updated" }, // v6.0
            { "FrmMain.MnuSetLockScreen", "Set as Lock screen image" }, // V6.0
            { "FrmMain.MnuSetLockScreen._Error", "Could not set the viewing image as lock screen image" }, // v6.0
            { "FrmMain.MnuSetLockScreen._Success", "Lock screen image is updated" }, // v6.0
            { "FrmMain.MnuOpenLocation", "Open image location" }, //v3.0
            { "FrmMain.MnuImageProperties", "Image properties" }, //v3.0
            #endregion // Image

            #region Clipboard
            { "FrmMain.MnuClipboard", "Clipboard" }, //v3.0
            { "FrmMain.MnuCopyFile", "Copy file" }, //v3.0
            { "FrmMain.MnuCopyFile._Success", "Copied {0} file(s)." }, // v2.0 final
            { "FrmMain.MnuCopyImageData", "Copy image data" }, //v5.0
            { "FrmMain.MnuCopyImageData._Copying", "Copying the image data. It's going to take a while…" }, // v9.0
            { "FrmMain.MnuCopyImageData._Success", "Copied the current image data." }, // v5.0
            { "FrmMain.MnuCutFile", "Cut file" }, //v3.0
            { "FrmMain.MnuCutFile._Success", "Cut {0} file(s)." }, // v2.0 final
            { "FrmMain.MnuCopyPath", "Copy image path" }, //v3.0
            { "FrmMain.MnuCopyPath._Success", "Copied the current image path." }, // v9.0
            { "FrmMain.MnuPasteImage", "Paste image" }, //v3.0
            { "FrmMain.MnuPasteImage._Error", "Could not find image data in the Clipboard" }, // v8.0
            { "FrmMain.MnuClearClipboard", "Clear clipboard" }, //v3.0
            { "FrmMain.MnuClearClipboard._Success", "Cleared clipboard." }, // v2.0 final

            #endregion

            { "FrmMain.MnuWindowFit", "Window Fit" }, //v7.5
            { "FrmMain.MnuFullScreen", "Full Screen" }, //v3.0

            { "FrmMain.MnuFrameless", "Frameless" }, //v7.5
            { "FrmMain.MnuFrameless._EnableDescription", "Hold Shift key to move the window." }, // v7.5

            { "FrmMain.MnuSlideshow", "Slideshow" }, //v3.0

            #region Layout
            { "FrmMain.MnuLayout", "Layout" }, //v3.0
            { "FrmMain.MnuToggleToolbar", "Toolbar" }, //v3.0
            { "FrmMain.MnuToggleGallery", "Gallery panel" }, //v3.0
            { "FrmMain.MnuToggleCheckerboard", "Checkerboard background" }, //v3.0, updated v5.0
            { "FrmMain.MnuToggleTopMost", "Keep window always on top" }, //v3.2
            { "FrmMain.MnuToggleTopMost._Enable", "Enabled window always on top" }, // v9.0
            { "FrmMain.MnuToggleTopMost._Disable", "Disabled window always on top" }, // v9.0
            #endregion // Layout

            #region Tools
            { "FrmMain.MnuTools", "Tools" }, //v3.0
            { "FrmMain.MnuColorPicker", "Color picker" }, //v5.0
            { "FrmMain.MnuFrameNav", "Frame navigation" }, // v7.5
            { "FrmMain.MnuCropTool", "Crop image" }, // v7.6
            { "FrmMain.MnuGetMoreTools", "Get more tools…" }, // v9.0

            { "FrmMain.MnuLosslessCompression", "Magick.NET Lossless Compression" }, // v9.1
            { "FrmMain.MnuLosslessCompression._Confirm", "Are you sure you want to proceed?" }, // v9.1
            { "FrmMain.MnuLosslessCompression._Description", "This tool uses Magick.NET library for lossless compression, optimizing file size. Overwrites only if the compressed file is smaller than the original." }, // v9.1
            { "FrmMain.MnuLosslessCompression._Compressing", "Performing lossless compression…" }, // v9.1
            { "FrmMain.MnuLosslessCompression._Done", "Done lossless compression.\r\nThe new file size is {0}, saved {1}." }, // v9.1

            #endregion

            { "FrmMain.MnuSettings", "Settings" }, // v3.0

            #region Help
            { "FrmMain.MnuHelp", "Help" }, //v7.0
            { "FrmMain.MnuAbout", "About" }, //v3.0
            { "FrmMain.MnuQuickSetup", "Open ImageGlass Quick Setup" }, //v9.0
            { "FrmMain.MnuCheckForUpdate._NewVersion", "A new version is available!" }, //v5.0
            { "FrmMain.MnuReportIssue", "Report an issue…" }, //v3.0

            { "FrmMain.MnuSetDefaultPhotoViewer", "Set default photo viewer" }, //v9.0
            { "FrmMain.MnuSetDefaultPhotoViewer._Success", "You have successfully set ImageGlass as default photo viewer." }, //v9.0
            { "FrmMain.MnuSetDefaultPhotoViewer._Error", "Could not set ImageGlass as default photo viewer." }, //v9.0

            { "FrmMain.MnuRemoveDefaultPhotoViewer", "Remove default photo viewer" }, //v9.0
            { "FrmMain.MnuRemoveDefaultPhotoViewer._Success", "ImageGlass is no longer the default photo viewer." }, //v9.0
            { "FrmMain.MnuRemoveDefaultPhotoViewer._Error", "Could not remove ImageGlass as the default photo viewer." }, //v9.0

            #endregion

            { "FrmMain.MnuExit", "Exit" }, //v7.0

            #endregion


            #region Form message texts
            { "FrmMain.PicMain._ErrorText", "Could not open this image" }, // v2.0 beta, updated 4.0, 9.0
            { "FrmMain.MnuMain", "Main menu" }, // v3.0

            { "FrmMain._OpenFileDialog", "All supported files" },
            { "FrmMain._Loading", "Loading…" }, // v3.0
            { "FrmMain._OpenWith", "Open with {0}" }, //v9.0
            { "FrmMain._ReachedFirstImage", "Reached the first image" }, // v4.0
            { "FrmMain._ReachedLastLast", "Reached the last image" }, // v4.0
            { "FrmMain._ClipboardImage", "Clipboard image" }, //v9.0

            #endregion


            #endregion


            #region FrmAbout
            { "FrmAbout._Slogan", "A lightweight, versatile image viewer" },
            { "FrmAbout._Version", "Version:" },
            { "FrmAbout._License", "Software license" },
            { "FrmAbout._Privacy", "Privacy policy" },
            { "FrmAbout._Thanks", "Special thanks to:" },
            { "FrmAbout._LogoDesigner", "Logo designer:" },
            { "FrmAbout._Collaborator", "Collaborator:" },
            { "FrmAbout._Contact", "Contact" },
            { "FrmAbout._Homepage", "Homepage:" },
            { "FrmAbout._Email", "Email:" },
            { "FrmAbout._Credits", "Credits" },
            { "FrmAbout._Donate", "Donate" },
            #endregion


            #region FrmSettings

            { "FrmSettings._ResetSettings", "Reset settings" }, // v9.1

            #region Nav bar
            { "FrmSettings.Nav._General", "General" },
            { "FrmSettings.Nav._Image", "Image" },
            { "FrmSettings.Nav._Slideshow", "Slideshow" },
            { "FrmSettings.Nav._Edit", "Edit" },
            { "FrmSettings.Nav._Viewer", "Viewer" },
            { "FrmSettings.Nav._Toolbar", "Toolbar" },
            { "FrmSettings.Nav._Gallery", "Gallery" },
            { "FrmSettings.Nav._Layout", "Layout" },
            { "FrmSettings.Nav._Mouse", "Mouse" },
            { "FrmSettings.Nav._Keyboard", "Keyboard" },
            { "FrmSettings.Nav._FileTypeAssociations", "File type associations" },
            { "FrmSettings.Nav._Tools", "Tools" },
            { "FrmSettings.Nav._Language", "Language" },
            { "FrmSettings.Nav._Appearance", "Appearance" },
            #endregion // Nav bar


            #region Tab General
            // General > General
            { "FrmSettings._StartupDir", "Startup location" },
            { "FrmSettings._ConfigDir", "Configuration location" },
            { "FrmSettings._UserConfigFile", "User settings file (igconfig.json)" },

            // General > Startup
            { "FrmSettings._Startup", "Startup" },
            { "FrmSettings._ShowWelcomeImage", "Show welcome image" },
            { "FrmSettings._ShouldOpenLastSeenImage", "Open the last seen image" },

            // General > Real-time update
            { "FrmSettings._RealTimeFileUpdate", "Real-time file update" },
            { "FrmSettings._EnableRealTimeFileUpdate", "Monitor file changes in the viewing folder and update in realtime" },
            { "FrmSettings._ShouldAutoOpenNewAddedImage", "Open the new added image automatically" },

            // General > Others
            { "FrmSettings._Others", "Others" },
            { "FrmSettings._AutoUpdate", "Check for update automatically" },
            { "FrmSettings._EnableMultiInstances", "Allow multiple instances of the program" },
            { "FrmSettings._ShowAppIcon", "Show app icon on the title bar" },
            { "FrmSettings._InAppMessageDuration", "In-app message duration (milliseconds)" },
            { "FrmSettings._ImageInfoTags", "Image information tags" },
            { "FrmSettings._AvailableImageInfoTags", "Available tags:" },
            #endregion // Tab General


            
            #region Tab Image
            // Image > Image loading
            { "FrmSettings._ImageLoading", "Image loading" },
            { "FrmSettings._ImageLoadingOrder", "Image loading order" },
            { "FrmSettings._ShouldUseExplorerSortOrder", "Use Windows File Explorer sort order if possible" },
            { "FrmSettings._EnableRecursiveLoading", "Load images in subfolders" },
            { "FrmSettings._ShouldGroupImagesByDirectory", "Group images by directory" },
            { "FrmSettings._ShouldLoadHiddenImages", "Load hidden images" },
            { "FrmSettings._EnableLoopBackNavigation", "Loop back to the first image when reaching the end of the image list" },
            { "FrmSettings._ShowImagePreview", "Display image preview while it's being loaded" },
            { "FrmSettings._EnableImageTransition", "Enable image transition effect" },
            { "FrmSettings._EnableImageAsyncLoading", "Enable image asynchronous loading" },

            { "FrmSettings._EmbeddedThumbnail", "Embedded thumbnail" },
            { "FrmSettings._UseEmbeddedThumbnailRawFormats", "Load only the embedded thumbnail for RAW formats" },
            { "FrmSettings._UseEmbeddedThumbnailOtherFormats", "Load only the embedded thumbnail for other formats" },
            { "FrmSettings._MinEmbeddedThumbnailSize", "Minimum size of the embedded thumbnail to be loaded" },
            { "FrmSettings._MinEmbeddedThumbnailSize._Width", "Width" },
            { "FrmSettings._MinEmbeddedThumbnailSize._Height", "Height" },

            // Image > Image Booster
            { "FrmSettings._ImageBooster", "Image Booster" },
            { "FrmSettings._ImageBoosterCacheCount", "Number of images cached by Image Booster (one direction)" },
            { "FrmSettings._ImageBoosterCacheMaxDimension", "Maximum image dimension to be cached (in pixels)" },
            { "FrmSettings._ImageBoosterCacheMaxFileSizeInMb", "Maximum image file size to be cached (in megabytes)" },

            // Image > Color management
            { "FrmSettings._ColorManagement", "Color management" },
            { "FrmSettings._ShouldUseColorProfileForAll", "Apply also for images without embedded color profile" },
            { "FrmSettings._ColorProfile", "Color profile" },
            { "FrmSettings._CurrentMonitorProfile._Description", "ImageGlass does not auto-update the color when moving its window between monitors" },
            #endregion // Tab Image


            #region Tab Slideshow
            // Slideshow > Slideshow
            { "FrmSettings._HideMainWindowInSlideshow", "Automatically hide main window" },
            { "FrmSettings._ShowSlideshowCountdown", "Show slideshow countdown" },
            { "FrmSettings._EnableFullscreenSlideshow", "Start slideshow in Full Screen mode" },
            { "FrmSettings._UseRandomIntervalForSlideshow", "Use random interval" },
            { "FrmSettings._SlideshowInterval", "Slideshow interval:" },
            { "FrmSettings._SlideshowInterval._From", "From" },
            { "FrmSettings._SlideshowInterval._To", "To" },
            { "FrmSettings._SlideshowBackgroundColor", "Slideshow background color" },

            // Slideshow > Slideshow notification
            { "FrmSettings._SlideshowNotification", "Slideshow notification" },
            { "FrmSettings._SlideshowImagesToNotifySound", "Number of images to trigger a notification sound" },
            #endregion // Tab Slideshow


            #region Tab Edit
            // Edit > Edit
            { "FrmSettings._ShowDeleteConfirmation", "Show confirmation dialog when deleting file" },
            { "FrmSettings._ShowSaveOverrideConfirmation", "Show confirmation dialog when overriding file" },
            { "FrmSettings._ShouldPreserveModifiedDate", "Preserve the image's modified date on save" },
            { "FrmSettings._OpenSaveAsDialogInTheCurrentImageDir", "Open the Save As dialog in the current image directory" }, // v9.1
            { "FrmSettings._ImageEditQuality", "Image quality" },
            { "FrmSettings._AfterEditingAction", "After opening editing app" },

            // Edit > Clipboard
            { "FrmSettings._Clipboard", "Clipboard" },
            { "FrmSettings._EnableCopyMultipleFiles", "Enable the copying of multiple files at once" },
            { "FrmSettings._EnableCutMultipleFiles", "Enable the cutting of multiple files at once" },

            // Edit > Image editing apps
            { "FrmSettings._EditApps", "Image editing apps" },
            { "FrmSettings._EditApps._AppName", "App name" },
            { "FrmSettings.EditAppDialog._AddApp", "Add an app for editing" },
            { "FrmSettings.EditAppDialog._EditApp", "Edit app" },

            #endregion // Tab Edit


            #region Tab Layout
            // Layout > Layout
            { "FrmSettings.Layout._Order", "Order" },
            { "FrmSettings.Layout._Toolbar", "Toolbar" },
            { "FrmSettings.Layout._ToolbarContext", "Contextual toolbar" },
            { "FrmSettings.Layout._Gallery", "Gallery" },
            { "FrmSettings.Layout._ToolbarPosition", "Toolbar position" },
            { "FrmSettings.Layout._ToolbarContextPosition", "Contextual toolbar position" },
            { "FrmSettings.Layout._GalleryPosition", "Gallery position" },
            #endregion // Tab Layout


            #region Tab Viewer
            // Viewer > Viewer
            { "FrmSettings._ShowCheckerboardOnlyImageRegion", "Show checkerboard only within the image region" },
            { "FrmSettings._EnableNavigationButtons", "Show navigation arrow buttons" },
            { "FrmSettings._CenterWindowFit", "Automatically center the window in Window Fit mode" },
            { "FrmSettings._UseWebview2ForSvg", "Use Webview2 for viewing SVG format" },
            { "FrmSettings._PanSpeed", "Panning speed" },

            // Viewer > Zooming
            { "FrmSettings._Zooming", "Zooming" },
            { "FrmSettings._ImageInterpolation", "Image interpolation" },
            { "FrmSettings._ImageInterpolation._ScaleDown", "When zoom < 100%" },
            { "FrmSettings._ImageInterpolation._ScaleUp", "When zoom > 100%" },
            { "FrmSettings._ZoomSpeed", "Zoom speed" },
            { "FrmSettings._ZoomLevels", "Zoom levels" },
            { "FrmSettings._UseSmoothZooming", "Use smooth zooming" },
            { "FrmSettings._LoadDefaultZoomLevels", "Load default zoom levels" },
            #endregion // Tab Viewer


            #region Tab Toolbar
            // Toolbar > Toolbar
            { "FrmSettings.Toolbar._HideToolbarInFullscreen", "Hide toolbar in Full Screen mode" },
            { "FrmSettings.Toolbar._EnableCenterToolbar", "Use center alignment for toolbar" },
            { "FrmSettings.Toolbar._ToolbarIconHeight", "Toolbar icon size" },

            { "FrmSettings.Toolbar._AddNewButton", "Add a custom toolbar button" },
            { "FrmSettings.Toolbar._EditButton", "Edit toolbar button" },
            { "FrmSettings.Toolbar._ButtonJson", "Button JSON" },


            { "FrmSettings.Toolbar._ToolbarButtons", "Toolbar buttons" },
            { "FrmSettings.Toolbar._AddCustomButton", "Add a custom button…" },
            { "FrmSettings.Toolbar._AvailableButtons", "Available buttons:" },
            { "FrmSettings.Toolbar._CurrentButtons", "Current buttons:" },
            { "FrmSettings.Toolbar._Errors._ButtonIdRequired", "Button ID required." },
            { "FrmSettings.Toolbar._Errors._ButtonIdDuplicated", "A button with the ID '{0}' has already been defined. Please choose a different and unique ID for your button to avoid conflicts." },
            { "FrmSettings.Toolbar._Errors._ButtonExecutableRequired", "Button executable required." },

            #endregion // TAB Toolbar


            #region Tab Gallery
            // Gallery > Gallery
            { "FrmSettings._HideGalleryInFullscreen", "Hide gallery in Full Screen mode" },
            { "FrmSettings._ShowGalleryScrollbars", "Show gallery scrollbars" },
            { "FrmSettings._ShowGalleryFileName", "Show thumbnail filename" },
            { "FrmSettings._ThumbnailSize", "Thumbnail size (in pixels)" },
            { "FrmSettings._GalleryCacheSizeInMb", "Maximum gallery cache size (in megabytes)" },
            { "FrmSettings._GalleryColumns", "Number of thumbnail columns in vertical gallery layout" },
            #endregion // Tab Gallery


            #region Tab Mouse
            // Mouse > Mouse wheel action
            { "FrmSettings._MouseWheelAction", "Mouse wheel action" },
            #endregion // Tab Mouse


            #region Tab Keyboard

            #endregion // Tab Mouse & Keyboard


            #region Tab File type associations
            // File type associations > File extension icons
            { "FrmSettings._FileExtensionIcons", "File extension icons" },
            { "FrmSettings._FileExtensionIcons._Description", "For customizing file extension icons, download an icon pack, place all .ICO files in the extension icon folder, and click the '{0}' button. This will also set ImageGlass as default photo viewer." },
            { "FrmSettings._OpenExtensionIconFolder", "Open extension icon folder" },
            { "FrmSettings._GetExtensionIconPacks", "Get extension icon packs…" },

            // File type associations > Default photo viewer
            { "FrmSettings._DefaultPhotoViewer", "Default photo viewer" },
            { "FrmSettings._DefaultPhotoViewer._Description", "You can set ImageGlass as your default photo viewer using the buttons below. Remember to manually reset it if you uninstall ImageGlass, as the installer does not handle this task automatically." },
            { "FrmSettings._MakeDefault", "Make default" },
            { "FrmSettings._RemoveDefault", "Remove default" },
            { "FrmSettings._OpenDefaultAppsSetting", "Open Default apps setting" },

            // File type associations > File formats
            { "FrmSettings._FileFormats", "File formats" },
            { "FrmSettings._TotalSupportedFormats", "Total supported formats: {0}" },
            { "FrmSettings._AddNewFileExtension", "Add new file extension" },

            #endregion // Tab File type associations


            #region Tab Tools
            // Tools > Tools
            { "FrmSettings.Tools._AddNewTool", "Add an external tool" },
            { "FrmSettings.Tools._EditTool", "Edit external tool" },
            { "FrmSettings.Tools._Integrated", "Integrated" },
            { "FrmSettings.Tools._IntegratedWith", "Integrated with {0}" },
            #endregion // Tab Tools


            #region Tab Language
            // Language > Language
            { "FrmSettings._DisplayLanguage", "Display language" },
            { "FrmSettings._Refresh", "Refresh" },
            { "FrmSettings._InstallNewLanguagePack", "Install new language packs…" },
            { "FrmSettings._GetMoreLanguagePacks", "Get more language packs…" },
            { "FrmSettings._ExportLanguagePack", "Export language pack…" },
            { "FrmSettings._Contributors", "Contributors" },
            #endregion // Tab Language


            #region Tab Appearance
            // Appearance > Appearance
            { "FrmSettings._WindowBackdrop", "Window backdrop" },
            { "FrmSettings._BackgroundColor", "Viewer background color" },

            // Appearance > Theme
            { "FrmSettings._Theme", "Theme" },
            { "FrmSettings._DarkTheme", "Dark" },
            { "FrmSettings._LightTheme", "Light" },
            { "FrmSettings._Author", "Author" },
            { "FrmSettings._Theme._OpenThemeFolder", "Open theme folder" },
            { "FrmSettings._Theme._GetMoreThemes", "Get more theme packs…" },
            { "FrmSettings._Theme._InstallTheme", "Install theme packs" },
            { "FrmSettings._Theme._UninstallTheme", "Uninstall a theme pack" },

            { "FrmSettings._UseThemeForDarkMode", "Use this theme for dark mode" },
            { "FrmSettings._UseThemeForLightMode", "Use this theme for light mode" },
            #endregion // Tab Appearance

            #endregion // FrmSettings
        
            
            #region FrmCrop
            { "FrmCrop.LblAspectRatio", "Aspect ratio:" }, //v9.0
            { "FrmCrop.LblLocation", "Location:" }, //v9.0
            { "FrmCrop.LblSize", "Size:" }, //v9.0

            { "FrmCrop.SelectionAspectRatio._FreeRatio", "Free ratio" }, //v9.0
            { "FrmCrop.SelectionAspectRatio._Custom", "Custom…" }, //v9.0
            { "FrmCrop.SelectionAspectRatio._Original", "Original" }, //v9.0

            { "FrmCrop.BtnQuickSelect._Tooltip", "Quick select…" }, //v9.0
            { "FrmCrop.BtnReset._Tooltip", "Reset selection" }, //v9.0
            { "FrmCrop.BtnSettings._Tooltip", "Open Crop tool settings" }, //v9.0

            { "FrmCrop.BtnSave", "Save" }, //v9.0
            { "FrmCrop.BtnSave._Tooltip", "Save image" }, //v9.0
            { "FrmCrop.BtnSaveAs", "Save as…" }, //v9.0
            { "FrmCrop.BtnSaveAs._Tooltip", "Save as a copy…" }, //v9.0
            { "FrmCrop.BtnCrop", "Crop" }, //v9.0
            { "FrmCrop.BtnCrop._Tooltip", "Crop the image only" }, //v9.0
            { "FrmCrop.BtnCopy", "Copy" }, //v9.0
            { "FrmCrop.BtnCopy._Tooltip", "Copy the selection to clipboard" }, //v9.0


            // Crop settings
            { "FrmCropSettings._Title", "Crop settings" }, //v9.0
            { "FrmCropSettings.ChkCloseToolAfterSaving", "Close Crop tool after saving" }, //v9.0
            { "FrmCropSettings.LblDefaultSelection", "Default selection" }, //v9.0
            { "FrmCropSettings.ChkAutoCenterSelection", "Auto-center selection" }, //v9.0

            { "FrmCropSettings.DefaultSelectionType._UseTheLastSelection", "Use the last selection" }, //v9.0
            { "FrmCropSettings.DefaultSelectionType._SelectNone", "Select none" }, //v9.0
            { "FrmCropSettings.DefaultSelectionType._SelectX", "Select {0}" }, //v9.0
            { "FrmCropSettings.DefaultSelectionType._SelectAll", "Select all" }, //v9.0
            { "FrmCropSettings.DefaultSelectionType._CustomArea", "Custom area…" }, //v9.0

            #endregion // FrmCrop


            #region FrmColorPicker

            { "FrmColorPicker.BtnSettings._Tooltip", "Open Color picker settings…" }, //v9.0

            // Color picker settings
            { "FrmColorPickerSettings._Title", "Color picker settings" }, //v9.0
            { "FrmColorPickerSettings.ChkShowRgbA", "Use RGB format with alpha value" }, //v5.0
            { "FrmColorPickerSettings.ChkShowHexA", "Use HEX format with alpha value" }, //v5.0
            { "FrmColorPickerSettings.ChkShowHslA", "Use HSL format with alpha value" }, //v5.0
            { "FrmColorPickerSettings.ChkShowHsvA", "Use HSV format with alpha value" }, //v8.0
            { "FrmColorPickerSettings.ChkShowCIELabA", "Use CIELAB format with alpha value" }, //v9.0

            #endregion


            #region FrmToolNotFound
            { "FrmToolNotFound._Title", "Tool not found" }, // v9.0
            { "FrmToolNotFound.BtnSelectExecutable", "Select…" }, // v9.0
            { "FrmToolNotFound.LblHeading", "'{0}' is not found!" }, // v9.0
            { "FrmToolNotFound.LblDescription", "ImageGlass was unable to locate the path to the '{0}' executable. To resolve this issue, please update the path to the '{0}' as necessary." }, // v9.0
            { "FrmToolNotFound.LblDownloadToolText", "You can download more tools for ImageGlass at:" }, // v9.0
            #endregion // FrmToolNotFound


            #region FrmHotkeyPicker
            { "FrmHotkeyPicker.LblHotkey", "Press hotkeys" }, // v9.0
            #endregion // FrmHotkeyPicker


            #region igcmd.exe

            { "_._IgCommandExe._DefaultError._Heading", "Invalid commands" }, //v9.0
            { "_._IgCommandExe._DefaultError._Description", "Make sure you pass the correct commands!\r\nThis executable file contains command-line functions for ImageGlass software.\r\n\r\nTo explore all command lines, please visit:\r\n{0}" }, //v9.0


            #region FrmSlideshow

            { "FrmSlideshow._PauseSlideshow", "Slideshow is paused." }, // v9.0
            { "FrmSlideshow._ResumeSlideshow", "Slideshow is resumed." }, // v9.0

            // menu
            { "FrmSlideshow.MnuPauseResumeSlideshow", "Pause/resume slideshow" }, // v9.0
            { "FrmSlideshow.MnuExitSlideshow", "Exit slideshow" }, // v9.0
            { "FrmSlideshow.MnuChangeBackgroundColor", "Change background color…" }, // v9.0

            { "FrmSlideshow.MnuToggleCountdown", "Show slideshow countdown" }, // v9.0
            { "FrmSlideshow.MnuZoomModes", "Zoom modes" }, // v9.0

            #endregion


            #region FrmExportFrames
            { "FrmExportFrames._Title", "Export image frames" }, //v9.0
            { "FrmExportFrames._FileNotExist", "Image file does not exist" }, //v7.5
            { "FrmExportFrames._FolderPickerTitle", "Select output folder for exporting image frames" }, //v9.0
            { "FrmExportFrames._Exporting", "Exporting {0}/{1} frames \r\n{2}…" }, //v9.0
            { "FrmExportFrames._ExportDone", "Exported {0} frames successfully to \r\n{1}" }, //v9.0
            { "FrmExportFrames._OpenOutputFolder", "Open output folder" }, //v9.0
            #endregion


            #region FrmUpdate
            { "FrmUpdate._StatusChecking", "Checking for update…" }, //v9.0
            { "FrmUpdate._StatusUpdated", "You are using the latest version!" }, //v9.0
            { "FrmUpdate._StatusOutdated", "A new update is available!" }, //v9.0
            { "FrmUpdate._CurrentVersion", "Current version: {0}" }, //v9.0
            { "FrmUpdate._LatestVersion", "The latest version: {0}" }, //v9.0
            { "FrmUpdate._PublishedDate", "Published date: {0}" }, //v9.0
            #endregion


            #region FrmQuickSetup

            { "FrmQuickSetup._Text", "ImageGlass Quick Setup" }, //v9.0
            { "FrmQuickSetup._StepInfo", "Step {0}" }, //v9.0
            { "FrmQuickSetup._SkipQuickSetup", "Skip this and launch ImageGlass" }, //v9.0

            { "FrmQuickSetup._SeeWhatNew", "See what's new in this version…" }, // v9.0
            { "FrmQuickSetup._SelectProfile", "Select a profile" }, //v9.0
            { "FrmQuickSetup._StandardUser", "Standard user" }, //v9.0
            { "FrmQuickSetup._ProfessionalUser", "Professional user" }, //v9.0
            { "FrmQuickSetup._SettingProfileDescription", "To modify these settings, simply access app settings." }, // v9.0

            { "FrmQuickSetup._SettingsWillBeApplied", "Settings will be applied:" }, //v9.0
            { "FrmQuickSetup._SetDefaultViewer", "Do you want to set ImageGlass as the default photo viewer?" }, //v9.0
            { "FrmQuickSetup._SetDefaultViewer._Description", "You can reset it in the app settings > File type associations tab." }, //v9.0

            { "FrmQuickSetup._ConfirmCloseProcess", "Before applying the new settings, it's essential to close all ImageGlass processes. Are you ready to proceed?" }, //v7.5

            #endregion

            #endregion // igcmd.exe

        };
    }

}