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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.Common.Windows;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using ImageGlass.Windows;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public partial class AppAPIProvider
{
    // wallpaper formats
    private static FrozenSet<string> _desktopNativeFormats => [".bmp", ".jpg", ".jpeg", ".png", ".gif"];

    // clipboard formats carrying the cut-vs-copy hint of a file operation
    private static readonly DataFormat<byte[]> _winDropEffectFormat = DataFormat.CreateBytesPlatformFormat("Preferred DropEffect");
    private static readonly DataFormat<byte[]> _gnomeCopiedFilesFormat = DataFormat.CreateBytesPlatformFormat("x-special/gnome-copied-files");
    private static readonly DataFormat<byte[]> _kdeCutSelectionFormat = DataFormat.CreateBytesPlatformFormat("application/x-kde-cutselection");

    // windowed layout captured on entering full screen; null when not in full screen
    private WindowLayoutSnapshot? _preFullScreenLayout;

    // slideshow state backup
    private bool _isFullScreenBeforeSlideshow;
    private bool _isFramelessBeforeSlideshow;
    private bool _isWindowFitBeforeSlideshow;
    private bool _showToolbarBeforeSlideshow = true;
    private bool _showGalleryBeforeSlideshow = true;
    private bool _windowMaximizedBeforeSlideshow;
    private DispatcherTimer? _slideshowCountdownTimer;
    private bool _slideshowIsAdvancing;

    // set once the user runs a check-for-update with UI; the startup silent check then completes
    // its work without popping its own window on top of what the user already asked for
    private static InterlockedBool _hasManualUpdateCheck;


    /// <summary>
    /// Gets the windowed layout captured on entering full screen, so a session that ends in full
    /// screen still persists the layout to return to; <c>null</c> when not in full screen.
    /// </summary>
    public WindowLayoutSnapshot? PreFullScreenLayout => _preFullScreenLayout;


    private static ViewerControl Viewer => App.MainWindow.PART_MainView.PART_Viewer;
    private static ToolbarControl Toolbar => App.MainWindow.PART_MainView.PART_Toolbar;
    private static GalleryControl Gallery => App.MainWindow.PART_MainView.PART_Gallery;
    private static PhGridSplitter GalleryResizer => App.MainWindow.PART_MainView.PART_GalleryResizer;
    private static MessageControl Message => App.MainWindow.PART_MainView.PART_Message;
    private static ToolHostControl ToolHost => App.MainWindow.PART_MainView.PART_ToolHost;
    private static SlideshowCountdownOverlay SlideshowCountdown => App.MainWindow.PART_MainView.PART_SlideshowCountdown;


    /// <summary>
    /// Gets the <see cref="ViewerControl"/> for external plugin access.
    /// </summary>
    internal static ViewerControl? GetViewer() => Viewer;


    public AppAPIProvider()
    {
        // Register built-in hosted plugins (via ToolControlAdapter)
        Core.ToolRegistry.Register(ColorPickerToolControl.TOOL_ID,
            new ToolControlAdapter(ColorPickerToolControl.TOOL_ID, v => new ColorPickerToolControl { Viewer = v }));
        Core.ToolRegistry.Register(CropImageToolControl.TOOL_ID,
            new ToolControlAdapter(CropImageToolControl.TOOL_ID, v => new CropImageToolControl { Viewer = v }));
        Core.ToolRegistry.Register(FrameNavToolControl.TOOL_ID,
            new ToolControlAdapter(FrameNavToolControl.TOOL_ID, v => new FrameNavToolControl { Viewer = v }));
        Core.ToolRegistry.Register(HdrToneMapperToolControl.TOOL_ID,
            new ToolControlAdapter(HdrToneMapperToolControl.TOOL_ID, v => new HdrToneMapperToolControl { Viewer = v }));

        // Register built-in non-hosted plugins
        Core.ToolRegistry.Register(ImageResizerTool.TOOL_ID, new ImageResizerTool());
        Core.ToolRegistry.Register(LosslessCompressionTool.TOOL_ID, new LosslessCompressionTool());
    }




    #region Main Menu APIs

    /// <summary>
    /// Shows main menu.
    /// </summary>
    public static void IG_OpenMainMenu()
    {
        App.MainWindow.PART_MainView.PART_Toolbar.PART_BtnMainMenu.OpenDropdownMenu();
    }


    /// <summary>
    /// Opens the app settings window.
    /// </summary>
    public static async Task IG_OpenSettingsAsync(string? configId = null)
    {
        // no explicit target -> restore the last opened settings page
        if (string.IsNullOrWhiteSpace(configId)) configId = Core.Config.LastOpenedSetting;

        // reuse the existing window if it is already open
        if (App.SettingsWindow is not null)
        {
            if (!string.IsNullOrWhiteSpace(configId))
            {
                App.SettingsWindow.NavigateToConfig(configId);
            }

            App.SettingsWindow.RestoreAndActivate();
            return;
        }

        App.SettingsWindow = new SettingsWindow(configId);

        try
        {
            await App.SettingsWindow.ShowAsync(null);
        }
        finally
        {
            App.SettingsWindow = null;
        }
    }


    /// <summary>
    /// Exit the app.
    /// </summary>
    public static void IG_Exit()
    {
        BHelper.ExitApp(false);
    }



    /// <summary>
    /// Applies settings from a JSON object.
    /// </summary>
    public static async Task IG_ApplySettingsAsync(string? jsonStr = null)
    {
        if (string.IsNullOrWhiteSpace(jsonStr))
        {
            throw new ArgumentException($$"""
                A JSON object of settings is required, e.g. { "Language": "Vietnamese.iglang.json" }.

                ----------
                👉🏼 Method: {{nameof(IG_ApplySettingsAsync)}}
                """,
                nameof(jsonStr));
        }

        var changedIds = Config.ApplyJsonOverrides(Core.Config, jsonStr);
        if (changedIds.Count == 0) return;

        // same order as SettingsViewModel.CommitAsync
        var isSaved = await Core.Config.SaveAsync();
        SettingsViewModel.RunApplyActions(changedIds);

        // the values are live either way; the caller still has to hear that the file was not written
        if (!isSaved && Config.SavingException is Exception ex) throw ex;
    }


    #endregion // Main Menu APIs



    #region File APIs

    /// <summary>
    /// Shows file picker to open a photo.
    /// </summary>
    public async Task IG_OpenFileAsync()
    {
        var supportFileExtPatterns = Core.GetSupportedFileExtensions().Select(ext => $"*{ext}")
            .ToImmutableList();

        var files = await App.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Core.Lang[LangId.Menu_MnuOpenFile],
            FileTypeFilter = [
                new FilePickerFileType(Core.Lang[LangId._OpenFileDialog]) {
                    Patterns = supportFileExtPatterns,
                },
            ],
        });

        var file = files?.ElementAtOrDefault(0);
        var filePath = file?.TryGetLocalPath();

        IG_OpenPath(filePath);
    }


    /// <summary>
    /// Shows folder picker to open a photo folder.
    /// </summary>
    public async Task IG_OpenFolderAsync()
    {
        var dirs = await App.MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

        var dir = dirs?.ElementAtOrDefault(0);
        var dirPath = dir?.TryGetLocalPath();

        IG_OpenPath(dirPath);
    }


    /// <summary>
    /// Opens photo by the given path, shortcut for method <see cref="PrepareLoadPhotoList"/> with params:
    /// <list type="bullet">
    ///   <item><c>currentFilePath</c> = <c>null</c></item>
    ///   <item><c>disposeForegroundShell</c> = <c>true</c> when loading from a different folder</item>
    ///   <item><c>loadInitPhoto</c> = <c>true</c></item>
    /// </list>
    /// </summary>
    public void IG_OpenPath(string? path)
    {
        var fullPath = BHelper.ResolvePath(path);
        if (string.IsNullOrWhiteSpace(fullPath)) return;

        // 1. check if the path is being opened
        var imageIndex = Core.Photos.IndexOf(fullPath);

        // 2.1 The file is located another folder, load the entire folder
        if (imageIndex == -1 || (Core.ShellProvider?.CanUseForegroundShell() ?? false))
        {
            // dispose the foreground shell when loading from a different folder,
            // so that file enumeration uses the new folder instead of the stale shell
            var disposeForegroundShell = imageIndex == -1;

            App.MainWindow.PART_MainView.PrepareLoadPhotoList([fullPath],
                currentFilePath: null, disposeForegroundShell, reloadInitPhoto: true);
        }
        // 2.2 The file is in current folder AND it is the viewing image
        else if (Core.Photos.CurrentIndex == imageIndex)
        {
            //do nothing
        }
        // 2.3 The file is in current folder AND it is NOT the viewing image
        else
        {
            IG_ViewByIndex(imageIndex);
        }
    }


    /// <summary>
    /// Open the current image in a new window.
    /// </summary>
    public static void IG_NewWindow()
    {
        if (!Core.Config.EnableMultiInstances)
        {
            _ = ModalWindow.ShowInfoAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuNewWindow],
                Heading = Core.Lang[LangId.Menu_MnuNewWindow],
                Description = Core.Lang[LangId.Menu_MnuNewWindow_Error],
            });
            return;
        }

        var filePath = Core.Photos.CurrentFilePath;

        // get position for new window
        var posDiff = App.MainWindow.DpiScale(10f);
        var newBounds = Core.Config.MainWindowBounds.WithX(Core.Config.MainWindowBounds.X + posDiff);
        newBounds = newBounds.WithY(Core.Config.MainWindowBounds.Y + posDiff);
        var boundStr = newBounds.ToStringDelimiter();
        var boundCmd = BHelper.BuildConfigCmdLine(nameof(Config.MainWindowBounds), boundStr);

        _ = BHelper.RunExeAsync(BHelper.AppExePath, [boundCmd, filePath]);
    }


    /// <summary>
    /// Saves and overrides the current photo.
    /// </summary>
    public static async Task IG_SaveAsync()
    {
        var srcFilePath = Core.Photos.CurrentFilePath;
        var isOpeningImageList = Core.Photos.CurrentIndex > -1;

        // use Save As if no image is opened
        if (!isOpeningImageList && Core.ClipboardImage is not null)
        {
            await IG_SaveAsAsync();
            return;
        }


        // show override warning
        if (Core.Config.EnableSaveConfirmation)
        {
            var modal = await ModalWindow.ShowWarningAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuSave],
                Heading = Core.Lang[LangId.Menu_MnuSave_Confirm],
                Description = srcFilePath,
                Note = Core.Lang[LangId.Menu_MnuSave_ConfirmDescription],
                IsRememberOptionVisible = true,
                Thumbnail = Core.Photos.Current?.GalleryThumbnail,
            }, ModalWindowButton.Yes_No);

            // update EnableSaveConfirmation setting
            Core.Config.EnableSaveConfirmation = !modal.IsRememberOptionChecked;

            if (modal.ExitCode != DialogExitCode.OK) return;
        }

        await SaveImageAsync(srcFilePath);
    }


    /// <summary>
    /// Shows save file dialog to save photo to file.
    /// </summary>
    public static async Task IG_SaveAsAsync()
    {
        var srcFilePath = string.Empty;
        var srcExt = ".png";
        IStorageFolder? initSaveDir = null;


        // 1. get dest file name
        if (Core.ClipboardImage is null)
        {
            srcFilePath = Core.Photos.CurrentFilePath;
            srcExt = Core.Photos.Current?.Extension?.ToLowerInvariant() ?? srcExt;


            if (Core.Config.EnableOpenSaveAsInCurrentFolder)
            {
                var initSaveDirPath = Path.GetDirectoryName(srcFilePath);

                if (!string.IsNullOrWhiteSpace(initSaveDirPath))
                {
                    initSaveDir = await App.MainWindow.StorageProvider.TryGetFolderFromPathAsync(initSaveDirPath);
                }
            }
        }

        var destFileName = string.IsNullOrEmpty(srcFilePath)
            ? $"untitle{srcExt}"
            : Path.GetFileNameWithoutExtension(srcFilePath);


        // 2. create file save picker
        var result = await App.MainWindow.StorageProvider.SaveFilePickerWithResultAsync(new FilePickerSaveOptions
        {
            Title = Core.Lang[LangId.Menu_MnuSaveAs],
            FileTypeChoices = SavingExts.FilePickerFileTypeChoices,
            ShowOverwritePrompt = !Core.Config.EnableSaveConfirmation, // only show 1 prompt
            SuggestedStartLocation = initSaveDir,
            SuggestedFileName = destFileName,
            SuggestedFileType = SavingExts.LastSavedFileType,
        });

        SavingExts.LastSavedFileType = result.SelectedFileType;

        var destFilePath = result.File?.TryGetLocalPath() ?? string.Empty;
        if (string.IsNullOrEmpty(destFilePath)) return;


        // 3. show override warning
        if (File.Exists(destFilePath) && Core.Config.EnableSaveConfirmation)
        {
            var fi = new FileInfo(destFilePath);

            // show confirm dialog
            var modal = await ModalWindow.ShowWarningAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuSaveAs],
                Heading = Core.Lang[LangId.Menu_MnuSave_Confirm],
                Description = $"""
                {destFilePath}
                {BHelper.FormatSize(fi.Length)}
                """,
                Note = Core.Lang[LangId.Menu_MnuSave_ConfirmDescription],
                IsRememberOptionVisible = true,
            }, ModalWindowButton.Yes_No);

            // update EnableSaveConfirmation setting
            Core.Config.EnableSaveConfirmation = !modal.IsRememberOptionChecked;

            if (modal.ExitCode != DialogExitCode.OK) return;
        }


        // save file
        await SaveImageAsync(destFilePath);
    }


    /// <summary>
    /// Save the viewing image to file.
    ///   <para>
    ///     The source image is checked by this order:
    ///     <list type="number">
    ///       <item>Selected image area.</item>
    ///       <item><see cref="Core.ClipboardImage"/>.</item>
    ///       <item>Source <paramref name="destFilePath"/> file.</item>
    ///     </list>
    ///   </para>
    /// </summary>
    public static async Task<bool> SaveImageAsync(string destFilePath)
    {
        var saveSource = ImageSaveSource.Undefined;
        var hasSrcPath = !string.IsNullOrEmpty(Core.Photos.CurrentFilePath);
        Exception? error = null;

        _ = Message.ShowAsync(destFilePath, Core.Lang[LangId.Menu_MnuSave_Saving]);


        // 1. save photo
        // 1.1 save the selection
        var hasSelection = Viewer.EnableSelection && !Viewer.SourceSelection.IsEmpty;
        if (hasSelection)
        {
            try
            {
                using var selectedBmp = Viewer.GetRenderedBitmap(true);
                var selectedImg = SkiaCodec.ToSKImage(selectedBmp)!;
                using var photo = new Photo(selectedImg);

                await photo.SaveAsAsync(destFilePath, new PhotoTransform(),
                    Core.Config.ImageEditQuality, Core.Config.EnablePreserveModifiedDate);
                saveSource = ImageSaveSource.SelectedArea;
            }
            catch (Exception ex) { error = ex; }
        }

        // 1.2 save the clipboard image
        else if (Core.ClipboardImage is not null)
        {
            try
            {
                await Core.ClipboardImage.SaveAsAsync(destFilePath, Core.ImageTransform,
                    Core.Config.ImageEditQuality, Core.Config.EnablePreserveModifiedDate);
                saveSource = ImageSaveSource.Clipboard;
            }
            catch (Exception ex) { error = ex; }
        }

        // 1.3 save the image in the list
        else if (Core.Photos.Current is not null)
        {
            try
            {
                await Core.Photos.Current.SaveAsAsync(destFilePath, Core.ImageTransform,
                    Core.Config.ImageEditQuality, Core.Config.EnablePreserveModifiedDate);
                saveSource = ImageSaveSource.CurrentFile;
            }
            catch (Exception ex) { error = ex; }
        }

        // 1.4 image is empty
        else
        {
            return false;
        }


        // 2. check for error
        if (error is not null)
        {
            await Message.ClearAsync();

            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuSave],
                Heading = Core.Lang[LangId.Menu_MnuSave_Error],
                Description = $"""
                {error.Source}:
                {error.Message}

                {destFilePath}
                """
            });

            return false;
        }


        // 3. check for success
        var newPhotoIndex = Core.Photos.IndexOf(destFilePath);
        if (newPhotoIndex == Core.Photos.CurrentIndex)
        {
            if (saveSource == ImageSaveSource.SelectedArea)
            {
                // reload to view the updated image
                IG_Reload();
            }
            else if (saveSource == ImageSaveSource.Clipboard)
            {
                // clear the clipboard image
                await LoadClipboardPhotoAsync(null);

                // reload to view the updated image
                IG_Reload();
            }

            // reset transformations
            Core.ImageTransform.Clear();
            Viewer.ClearPhotoTransforms();
        }


        _ = Message.ShowAsync(destFilePath, Core.Lang[LangId.Menu_MnuSave_Success]);


        // 4. emits saved event
        Core.OnPhotoSaved(new(Core.Photos.CurrentFilePath, destFilePath, saveSource));


        // 5. update thumbnail & metadata if file in the list was overriden
        Gallery.LoadThumbnail(newPhotoIndex, false);

        return true;
    }


    /// <summary>
    /// Exports image frames from the current photo source.
    /// </summary>
    public static async Task IG_ExportImageFrames()
    {
        if (Viewer.SourceKind == PhotoSource.None) return;
        var frameCount = Core.Photos.CurrentMetadata?.FrameCount ?? 0;
        if (frameCount < 2) return;

        // 1. open folder picker
        var results = await App.MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Core.Lang[LangId._FolderPickerTitle],
        });

        var destDirPath = results.ToArray().FirstOrDefault()?.TryGetLocalPath();
        if (string.IsNullOrEmpty(destDirPath)) return;


        // 2. get source file path
        var srcFilePath = Core.Photos.CurrentFilePath;

        // clipboard image
        if (Core.ClipboardImage is not null)
        {
            // save image to temp file
            srcFilePath = await Core.SavePhotoAsTempFileAsync();
        }
        if (string.IsNullOrEmpty(srcFilePath)) return;


        // 3. export frames
        Core.IsBusy = true;

        var exportWindow = new ExportFramesWindow(srcFilePath, destDirPath);
        await exportWindow.ShowAsync(App.MainWindow);

        Core.IsBusy = false;
    }


    /// <summary>
    /// Shows Open With window.
    /// </summary>
    public static async Task IG_OpenWithAsync()
    {
        if (BHelper.OS != OSType.Windows)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }


        string? filePath = null;
        var isClipboardPhoto = Core.ClipboardImage is not null;


        if (isClipboardPhoto)
        {
            _ = Message.ShowAsync(Core.Lang[LangId._CreatingFile], delayMs: 500);

            // save clipboard photo as temp PNG file
            filePath = await Core.SavePhotoAsTempFileAsync();
        }
        else
        {
            filePath = Core.Photos.CurrentFilePath;
        }


        await Message.ClearAsync();
        if (!File.Exists(filePath))
        {
            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuOpenWith],
                Description = Core.Lang[LangId._CreatingFileError],
            });
        }
        else
        {
            Core.ShellProvider?.ShowOpenWith(filePath);
        }
    }


    /// <summary>
    /// Opens Print dialog to print the current photo.
    /// </summary>
    public static async Task IG_PrintAsync()
    {
        if (BHelper.OS != OSType.Windows)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }

        var fileToPrint = Core.Photos.CurrentFilePath;
        _ = Message.ShowAsync(Core.Lang[LangId._CreatingFile], delayMs: 500);


        if (string.IsNullOrEmpty(fileToPrint))
        {
            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuOpenWith],
                Description = Core.Lang[LangId._CreatingFileError],
            });
        }
        else
        {
            try
            {
                await Core.PrintProvider.OpenPrintAsync(fileToPrint,
                    Core.Photos.CurrentMetadata, Core.ClipboardImage != null);
            }
            catch (Exception ex)
            {
                _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
                {
                    Title = Core.Lang[LangId.Menu_MnuPrint],
                    Heading = Core.Lang[LangId.Menu_MnuPrint_Error],
                    Description = ex.Message,
                });
            }
        }

        _ = Message.ClearAsync();
    }


    /// <summary>
    /// Shows Share dialog.
    /// </summary>
    public static async Task IG_ShareAsync()
    {
        if (BHelper.OS != OSType.Windows)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }

        var filePath = Core.Photos.CurrentFilePath;

        // print clipboard image
        if (Core.ClipboardImage is not null)
        {
            _ = Message.ShowAsync(Core.Lang[LangId._CreatingFile], delayMs: 500);

            // save image to temp file
            filePath = await Core.SavePhotoAsTempFileAsync();
        }

        await Message.ClearAsync();

        if (!File.Exists(filePath))
        {
            _ = ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuShare],
                Description = Core.Lang[LangId._CreatingFileError],
            });
        }
        else
        {
            try
            {
                Core.ShellProvider?.ShowShare(App.MainWindow.Handle, [filePath]);
            }
            catch (Exception ex)
            {
                _ = ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
                {
                    Title = Core.Lang[LangId.Menu_MnuShare],
                    Description = $"{Core.Lang[LangId.Menu_MnuShare_Error]}\r\n\r\n{ex.Message}",
                });
            }
        }
    }


    /// <summary>
    /// Opens photo file location.
    /// </summary>
    public static void IG_OpenLocation()
    {
        BHelper.OpenFilePath(Core.Photos.CurrentFilePath);
    }


    /// <summary>
    /// Opens a popup to rename the current photo.
    /// </summary>
    public static async Task IG_RenameAsync()
    {
        var oldFilePath = Core.Photos.CurrentFilePath;
        if (!File.Exists(oldFilePath)) return;

        var currentFolder = Path.GetDirectoryName(oldFilePath) ?? string.Empty;
        var ext = Path.GetExtension(oldFilePath);
        var newName = Path.GetFileNameWithoutExtension(oldFilePath);
        var title = Core.Lang[LangId.Menu_MnuRename];

        // 2. show popup
        var result = await ModalWindow.ShowInputAsync(App.MainWindow, new ModalWindowOptions
        {
            Title = title,
            Description = $"""
            {oldFilePath}

            {Core.Lang[LangId.Menu_MnuRename_Description]}
            """,
            InputValue = newName,
            AcceptValue = TextBoxAcceptValue.FileNameValueOnly,
            ThumbnailIcon = StockIconId.Rename,
            Thumbnail = Core.Photos.Current?.GalleryThumbnail,
        });

        if (result.ExitCode != DialogExitCode.OK || string.IsNullOrWhiteSpace(result.InputValue)) return;

        // 3. get new photo name
        newName = $"{result.InputValue.Trim()}{ext}";
        var newFilePath = Path.Combine(currentFolder, newName);


        // 4. perform renaming
        try
        {
            // Issue 73: Windows ignores case-only changes
            if (string.Equals(oldFilePath, newFilePath, StringComparison.OrdinalIgnoreCase))
            {
                // user changing only the case of the filename. Need to perform a trick.
                File.Move(oldFilePath, oldFilePath + "_temp");
                File.Move(oldFilePath + "_temp", newFilePath);
            }
            else
            {
                File.Move(oldFilePath, newFilePath);
            }


            // manually update the change if FileWatcher is not enabled
            if (!Core.Config.EnableFileWatcher)
            {
                Core.Photos.SetFilePath(Core.Photos.CurrentIndex, newFilePath);
            }
        }
        catch (Exception ex)
        {
            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Description = ex.Message,
            });
        }
    }


    /// <summary>
    /// Sends or permenantly deletes the current image.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static async Task IG_DeleteAsync(string? moveToRecycleBinStr = "true")
    {
        var moveToRecycleBin = BHelper.ConvertStringToBool(moveToRecycleBinStr) ?? true;
        await IG_DeleteAsync(moveToRecycleBin);
    }


    /// <summary>
    /// Sends or permenantly deletes the current image.
    /// </summary>
    public static async Task IG_DeleteAsync(bool moveToRecycleBin = true)
    {
        var filePath = Core.Photos.CurrentFilePath;
        if (!File.Exists(filePath)) return;

        var canDelete = true;
        var title = moveToRecycleBin
            ? Core.Lang[LangId.Menu_MnuMoveToRecycleBin]
            : Core.Lang[LangId.Menu_MnuDeleteFromHardDisk];


        // 1. show confirm dialog
        if (Core.Config.EnableDeleteConfirmation)
        {
            var heading = moveToRecycleBin
                ? Core.Lang[LangId.Menu_MnuMoveToRecycleBin_Description]
                : Core.Lang[LangId.Menu_MnuDeleteFromHardDisk_Description];
            var thumbnailIcon = moveToRecycleBin
                ? StockIconId.RecycleBin
                : StockIconId.Delete;

            var modal = await ModalWindow.ShowWarningAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Heading = heading,
                Description = $"""
                {filePath}
                {Core.Photos.Current?.Metadata.FileSizeFormatted}
                """,
                IsRememberOptionVisible = true,
                ThumbnailIcon = thumbnailIcon,
                Thumbnail = Core.Photos.Current?.GalleryThumbnail,
            }, ModalWindowButton.Yes_No);

            // update remember confirm setting
            Core.Config.EnableDeleteConfirmation = !modal.IsRememberOptionChecked;

            canDelete = modal.ExitCode == DialogExitCode.OK;
        }


        // 2. delete photo
        if (!canDelete) return;
        Core.IsBusy = true;

        try
        {
            await IG_UnloadAsync();
            BHelper.DeleteFile(filePath, moveToRecycleBin);

            // manually update the change because FileWatcher is disabled when IsBusy = true
            Core.Photos.Remove(Core.Photos.CurrentFilePath);
            var nextIndex = (int)Math.Min(Core.Photos.Count - 1, Core.Photos.CurrentIndex);
            var nextPhoto = Core.Photos.Select(nextIndex);
            _ = App.MainWindow.PART_MainView.ViewPhotoAsync(nextPhoto);
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Description = ex.Message,
            });
        }

        Core.IsBusy = false;
    }


    /// <summary>
    /// Opens photo's Properties dialog.
    /// </summary>
    public static void IG_OpenProperties()
    {
        Core.ShellProvider?.ShowFileProperties(Core.Photos.CurrentFilePath, App.MainWindow.Handle);
    }

    #endregion // File APIs



    #region Navigation APIs

    /// <summary>
    /// View a photo in the list by the given index.
    /// </summary>
    public void IG_ViewByIndex(string? photoIndexStr)
    {
        if (!int.TryParse(photoIndexStr, out var photoIndex)) return;

        IG_ViewByIndex(photoIndex);
    }


    /// <summary>
    /// View a photo in the list by the given index.
    /// </summary>
    public void IG_ViewByIndex(int photoIndex)
    {
        if (photoIndex < 0) return;

        var step = photoIndex - Core.Photos.CurrentIndex;
        IG_ViewByStep(step);
    }


    /// <summary>
    /// View a photo in the list by the given step.
    /// </summary>
    public void IG_ViewByStep(string? stepStr)
    {
        if (!int.TryParse(stepStr, out var step))
        {
            throw new ArgumentException($"""
                Step '{stepStr}' is not a valid integer.

                ----------
                👉🏼 Method: {nameof(IG_ViewByStep)}
                """,
                nameof(stepStr));
        }

        IG_ViewByStep(step);
    }


    /// <summary>
    /// View a photo in the list by the given step.
    /// </summary>
    public void IG_ViewByStep(int step)
    {
        // Sequential mode: ignore navigation until the current photo is painted, so holding
        // an arrow key advances one fully-drawn image at a time.
        // must run before GetByStep below, which advances CurrentIndex right away
        var isSequential = Core.Config.BrowsingMode == BrowsingMode.Sequential;
        if (isSequential)
        {
            var isLoading = App.MainWindow.PART_MainView.IsPhotoLoadInProgress;
            if (isLoading)
            {
                PhotoTrace.Mark("nav:blocked-sequential", Core.Photos.CurrentFilePath);
                return;
            }
        }

        // check if can navigate to the image
        var canLoopBack = Core.Config.EnableSlideshow
            ? Core.Config.EnableLoopSlideshow
            : Core.Config.EnableLoopBackNavigation;

        // jumping to a sibling folder at the boundary takes precedence over loop-back (not in slideshow),
        // so detect the true boundary first by disabling loop-back for the probe
        var autoSwitchDir = Core.Config.EnableAutoSwitchSiblingDir && !Core.Config.EnableSlideshow;

        var reachedBoundary = !Core.Photos.GetByStep(step, autoSwitchDir ? false : canLoopBack, out var photo);
        if (reachedBoundary && autoSwitchDir)
        {
            // try the sibling directory; if none found, fall back to normal loop-back handling
            if (TryOpenSiblingDir(step)) return;
            reachedBoundary = !Core.Photos.GetByStep(step, canLoopBack, out photo);
        }

        if (reachedBoundary)
        {
            var isFirst = Core.Photos.CurrentIndex == 0;

            _ = Message.ShowAsync(Core.Lang[isFirst
                ? LangId._ReachedFirstImage
                : LangId._ReachedLastImage]);
            return;
        }


        _ = App.MainWindow.PART_MainView.ViewPhotoAsync(photo);

        // reset slideshow interval on manual navigation
        if (Core.Config.EnableSlideshow && !_slideshowIsAdvancing)
        {
            if (Core.Slideshow is { } slideshow)
            {
                // resume if auto-paused (e.g. at end of list)
                if (slideshow.IsPaused)
                {
                    slideshow.Resume();
                }

                slideshow.ResetInterval();
            }
        }
    }


    /// <summary>
    /// At a navigation boundary, opens the next/previous sibling directory that contains images.
    /// Forward navigation lands on the first image; backward navigation lands on the last image.
    /// </summary>
    /// <returns><c>false</c> if no suitable sibling directory is found.</returns>
    private static bool TryOpenSiblingDir(int step)
    {
        var direction = step >= 0 ? 1 : -1;
        var currentDir = Path.GetDirectoryName(Core.Photos.CurrentFilePath);
        var supportedExts = Core.GetSupportedFileExtensions();
        var includeHidden = Core.Config.EnableHiddenImagesLoading;

        var siblingDir = BHelper.GetSiblingDir(currentDir, direction, supportedExts, includeHidden);
        if (string.IsNullOrEmpty(siblingDir)) return false;

        // announce the switch once the new photo has loaded; showing it earlier would be wiped
        // by the load pipeline, which clears the in-app message on PhotoState.Loaded
        void OnPhotoLoaded(ViewerControl s, PhotoLoadingEventArgs e)
        {
            if (e.State != PhotoState.Loaded && e.Photo.Error is null) return;

            Viewer.PhotoLoading -= OnPhotoLoaded;
            if (e.Photo.Error is not null) return;

            _ = Message.ShowAsync(Core.Lang[direction < 0
                ? LangId._SwitchedToPreviousFolder
                : LangId._SwitchedToNextFolder, siblingDir]);
        }
        Viewer.PhotoLoading += OnPhotoLoaded;

        // forward -> first image, backward -> last image; resolved after the list is built, so it
        // follows whatever order the search produced (Explorer view order included)
        App.MainWindow.PART_MainView.PrepareLoadPhotoList([siblingDir],
            currentFilePath: null, disposeForegroundShell: true, reloadInitPhoto: true,
            selectLastPhoto: direction < 0);

        return true;
    }


    /// <summary>
    /// View the next photo.
    /// </summary>
    public void IG_ViewNext()
    {
        IG_ViewByStep(1);
    }


    /// <summary>
    /// View the previous photo.
    /// </summary>
    public void IG_ViewPrevious()
    {
        IG_ViewByStep(-1);
    }


    /// <summary>
    /// Shows an input dialog, and opens the user-input photo.
    /// </summary>
    public async Task IG_GoToAsync()
    {
        if (Core.Photos.Count == 0) return;

        var oldIndex = Core.Photos.CurrentIndex + 1;
        var result = await ModalWindow.ShowInputAsync(App.MainWindow, new ModalWindowOptions
        {
            Title = Core.Lang[LangId.Menu_MnuGoTo],
            Description = Core.Lang[LangId.Menu_MnuGoTo_Description],
            InputValue = oldIndex.ToString(),
            AcceptValue = TextBoxAcceptValue.UnsignedIntValueOnly,
        });

        if (result.ExitCode != DialogExitCode.OK) return;


        if (int.TryParse(result.InputValue, out var newIndex))
        {
            newIndex--;

            if (newIndex != Core.Photos.CurrentIndex
                && 0 <= newIndex && newIndex < Core.Photos.Count)
            {
                IG_ViewByIndex(newIndex);
            }
        }
    }


    /// <summary>
    /// Views the first photo in the list.
    /// </summary>
    public void IG_GoToFirst()
    {
        IG_ViewByIndex(0);
    }


    /// <summary>
    /// Views the last photo in the list.
    /// </summary>
    public void IG_GoToLast()
    {
        IG_ViewByIndex((int)Core.Photos.Count - 1);
    }


    /// <summary>
    /// View a frame of the current photo.
    /// </summary>
    public void IG_ViewFrame(string? frameIndexStr)
    {
        if (!int.TryParse(frameIndexStr, out var frameIndex))
        {
            throw new ArgumentException($"""
                Frame index '{frameIndexStr}' is not a valid integer.

                ----------
                👉🏼 Method: {nameof(IG_ViewFrame)}
                """,
                nameof(frameIndexStr));
        }

        IG_ViewFrame(frameIndex);
    }


    /// <summary>
    /// View a frame of the current photo.
    /// If the frame index is out of range, it will be looped.
    /// </summary>
    public static void IG_ViewFrame(int frameIndex)
    {
        var frameCount = Core.Photos.CurrentMetadata?.FrameCount ?? 0;
        if (frameCount < 2) return;

        var safeFrameIndex = BHelper.ComputeIndexInRange(frameIndex, frameCount, true);

        Dispatcher.UIThread.Post(async () =>
        {
            await Viewer.ViewFrameAsync((uint)safeFrameIndex);
        });
    }


    /// <summary>
    /// View the next frame of the current photo.
    /// </summary>
    public static void IG_ViewNextFrame()
    {
        var newFrameIndex = (Core.Photos.Current?.FrameIndex ?? 0) + 1;
        IG_ViewFrame(newFrameIndex);
    }


    /// <summary>
    /// View the previous frame of the current photo.
    /// </summary>
    public static void IG_ViewPreviousFrame()
    {
        var newFrameIndex = (Core.Photos.Current?.FrameIndex ?? 1) - 1;
        IG_ViewFrame(newFrameIndex);
    }


    /// <summary>
    /// View the first frame of the current photo.
    /// </summary>
    public static void IG_ViewFirstFrame()
    {
        IG_ViewFrame(0);
    }


    /// <summary>
    /// View the last frame of the current photo.
    /// </summary>
    public static void IG_ViewLastFrame()
    {
        var lastFrameIndex = (int)(Core.Photos.CurrentMetadata?.FrameCount ?? 1) - 1;
        IG_ViewFrame(lastFrameIndex);
    }

    #endregion // Navigation APIs



    #region Zoom APIs

    /// <summary>
    /// Shows input dialog for custom zoom.
    /// </summary>
    public static async Task IG_CustomZoomAsync()
    {
        var oldZoom = Math.Round(Viewer.ZoomFactor * 100f, 3);

        var result = await ModalWindow.ShowInputAsync(App.MainWindow, new ModalWindowOptions
        {
            Title = Core.Lang[LangId.Menu_MnuCustomZoom],
            Description = Core.Lang[LangId.Menu_MnuCustomZoom_Description],
            InputValue = oldZoom.ToString(),
            AcceptValue = TextBoxAcceptValue.UnsignedFloatValueOnly,
            ThumbnailIcon = StockIconId.Find,
        });

        if (result.ExitCode != DialogExitCode.OK) return;

        if (float.TryParse(result.InputValue.Trim(), out var newZoom))
        {
            Viewer.ZoomFactor = newZoom / 100f;
        }
    }


    /// <summary>
    /// Zoom to the current cursor location by the given factor.
    /// </summary>
    public void IG_SetZoom(string? factorStr)
    {
        if (!float.TryParse(factorStr, out var factor))
        {
            throw new ArgumentException($"""
                Zoom factor '{factorStr}' is not a valid float.

                ----------
                👉🏼 Method: {nameof(IG_SetZoom)}
                """,
                nameof(factorStr));
        }

        IG_SetZoom(factor);
    }

    /// <summary>
    /// Zoom to the current cursor location by the given factor.
    /// </summary>
    public static void IG_SetZoom(float factor)
    {
        _ = Viewer.ZoomToPoint(factor);
    }


    /// <summary>
    /// Sets zoom = 100% if zoom value is less than 100%.
    /// Otherwise, refresh the image with the current zoom mode.
    /// </summary>
    public static void IG_SetZoomForMouseClick()
    {
        if (Viewer.ZoomFactor < 1)
        {
            IG_SetZoom(1);
        }
        else
        {
            IG_Refresh();
        }
    }


    /// <summary>
    /// Sets the zoom mode value.
    /// </summary>
    public void IG_SetZoomMode(string? modeStr)
    {
        if (!Enum.TryParse<ZoomMode>(modeStr, out var mode))
        {
            throw new ArgumentException($"""
                '{modeStr}' is not a valid zoom mode.

                ----------
                👉🏼 Method: {nameof(IG_SetZoomMode)}
                """,
                nameof(modeStr));
        }

        IG_SetZoomMode(mode);
    }

    /// <summary>
    /// Sets the zoom mode value.
    /// </summary>
    public static void IG_SetZoomMode(ZoomMode mode)
    {
        if (mode == Core.Config.ZoomMode)
        {
            IG_Refresh();
        }
        else
        {
            Core.Config.ZoomMode = mode;
        }
    }


    /// <summary>
    /// Zooms into the image.
    /// </summary>
    public static void IG_ZoomIn()
    {
        if (Viewer.ZoomLevels.Length > 0)
        {
            Viewer.ZoomIn();
            return;
        }

        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.ZoomIn, 100);
    }


    /// <summary>
    /// Zooms out of the image.
    /// </summary>
    public static void IG_ZoomOut()
    {
        if (Viewer.ZoomLevels.Length > 0)
        {
            Viewer.ZoomOut();
            return;
        }

        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.ZoomOut, 100);
    }

    #endregion // Zoom APIs



    #region Panning APIs

    /// <summary>
    /// Pans the viewing image to left.
    /// </summary>
    public static void IG_PanLeft()
    {
        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.PanLeft, 100);
    }


    /// <summary>
    /// Pans the viewing image to right.
    /// </summary>
    public static void IG_PanRight()
    {
        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.PanRight, 100);
    }


    /// <summary>
    /// Pans the viewing image to top.
    /// </summary>
    public static void IG_PanUp()
    {
        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.PanUp, 100);
    }


    /// <summary>
    /// Pans the viewing image to bottom.
    /// </summary>
    public static void IG_PanDown()
    {
        // smooth zooming
        Viewer.StartDrawingAnimation(AnimationSources.PanDown, 100);
    }


    /// <summary>
    /// Pans the viewing image to left side.
    /// </summary>
    public static void IG_PanToLeft()
    {
        var distanceX = Viewer.SrcRect.X * Viewer.ZoomFactor;
        var duration = 1000;
        Viewer.PanSpeed = distanceX / duration * 60;

        Viewer.StartDrawingAnimation(AnimationSources.PanLeft, duration, () =>
        {
            Viewer.PanSpeed = Core.Config.PanSpeed;
        });
    }


    /// <summary>
    /// Pans the viewing image to right side.
    /// </summary>
    public static void IG_PanToRight()
    {
        var x = Viewer.BitmapSize.Width - Viewer.SrcRect.Width;
        var distanceX = (x + Viewer.SrcRect.X) * Viewer.ZoomFactor;
        var duration = 1000;
        Viewer.PanSpeed = distanceX / duration * 60;

        Viewer.StartDrawingAnimation(AnimationSources.PanRight, duration, () =>
        {
            Viewer.PanSpeed = Core.Config.PanSpeed;
        });
    }


    /// <summary>
    /// Pans the viewing image to top.
    /// </summary>
    public static void IG_PanToTop()
    {
        var distanceY = Viewer.SrcRect.Y * Viewer.ZoomFactor;
        var duration = 1000;
        Viewer.PanSpeed = distanceY / duration * 60;

        Viewer.StartDrawingAnimation(AnimationSources.PanUp, duration, () =>
        {
            Viewer.PanSpeed = Core.Config.PanSpeed;
        });
    }


    /// <summary>
    /// Pans the viewing image to bottom.
    /// </summary>
    public static void IG_PanToBottom()
    {
        var y = Viewer.BitmapSize.Height - Viewer.SrcRect.Height;
        var distanceY = (y + Viewer.SrcRect.Y) * Viewer.ZoomFactor;
        var duration = 1000;
        Viewer.PanSpeed = distanceY / duration * 60;

        Viewer.StartDrawingAnimation(AnimationSources.PanDown, duration, () =>
        {
            Viewer.PanSpeed = Core.Config.PanSpeed;
        });
    }

    #endregion // Panning APIs



    #region Image APIs

    /// <summary>
    /// Refreshes image viewport.
    /// </summary>
    public static void IG_Refresh()
    {
        Viewer.Refresh(true, false, Core.Config.EnableWindowFit);
    }


    /// <summary>
    /// Reloads image file.
    /// </summary>
    /// <param name="resetZoomBoolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static void IG_Reload(string? resetZoomBoolStr)
    {
        var resetZoom = BHelper.ConvertStringToBool(resetZoomBoolStr) ?? true;
        IG_Reload(resetZoom);
    }


    /// <summary>
    /// Reloads image file.
    /// </summary>
    public static void IG_Reload(bool resetZoom = true)
    {
        _ = App.MainWindow.PART_MainView.ViewPhotoAsync(Core.Photos.Current, useCache: false, resetZoom: resetZoom);

        // reload thumbnail
        Gallery.LoadThumbnail(Core.Photos.CurrentIndex, false);
    }


    /// <summary>
    /// Reloads images list.
    /// </summary>
    public static void IG_ReloadList()
    {
        App.MainWindow.PART_MainView.PrepareLoadPhotoList(Core.Photos.DistinctDirs,
            Core.Photos.CurrentFilePath, disposeForegroundShell: false, reloadInitPhoto: false);
    }


    /// <summary>
    /// Unloads the current photo.
    /// </summary>
    public static async Task IG_UnloadAsync()
    {
        var args = new PhotoUnloadedEventArgs()
        {
            IsClipboardPhoto = Core.ClipboardImage is not null,
            Index = Core.Photos.CurrentIndex,
            FilePath = Core.Photos.CurrentFilePath,
        };


        // 1. unload clipboard photo
        if (args.IsClipboardPhoto)
        {
            await LoadClipboardPhotoAsync(null);

            // show the current photo in the list
            await App.MainWindow.PART_MainView.ViewPhotoAsync(Core.Photos.Current);
        }

        // 2. unload photo from the list
        else
        {
            // cancel loading the current image
            Core.Photos.Current?.CancelLoading();

            await App.MainWindow.PART_MainView.ViewPhotoAsync(null, false);
            Core.Photos.Current?.Unload();
        }

        // raise unloaded event
        Core.OnPhotoUnloaded(args);
    }


    /// <summary>
    /// Sets whether to use the Explorer sort order.
    /// </summary>
    public static void IG_ToggleExplorerSortOrder(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleExplorerSortOrder(enabled);
    }


    /// <summary>
    /// Sets whether to use the Explorer sort order.
    /// </summary>
    public static void IG_ToggleExplorerSortOrder(bool? enabled)
    {
        enabled ??= !Core.Config.EnableExplorerSortOrder;
        Core.Config.EnableExplorerSortOrder = enabled.Value;

        IG_ReloadList();
    }


    /// <summary>
    /// Sets the image loading order value.
    /// </summary>
    public void IG_SetLoadingOrderBy(string? orderByStr)
    {
        if (!Enum.TryParse<ImageOrderBy>(orderByStr, out var orderBy))
        {
            throw new ArgumentException($"""
                '{orderByStr}' is not a valid loading order.

                ----------
                👉🏼 Method: {nameof(IG_SetLoadingOrderBy)}
                """,
                nameof(orderByStr));
        }

        IG_SetLoadingOrderBy(orderBy);
    }


    /// <summary>
    /// Sets the image loading order value.
    /// </summary>
    public static void IG_SetLoadingOrderBy(ImageOrderBy orderBy)
    {
        if (orderBy == Core.Config.ImageLoadingOrder) return;

        Core.Config.ImageLoadingOrder = orderBy;
        IG_ReloadList();
    }


    /// <summary>
    /// Sets the image loading order value.
    /// </summary>
    public void IG_SetLoadingOrderType(string? orderTypeStr)
    {
        if (!Enum.TryParse<ImageOrderType>(orderTypeStr, out var orderType))
        {
            throw new ArgumentException($"""
                '{orderTypeStr}' is not a valid loading order.

                ----------
                👉🏼 Method: {nameof(IG_SetLoadingOrderType)}
                """,
                nameof(orderTypeStr));
        }

        IG_SetLoadingOrderType(orderType);
    }


    /// <summary>
    /// Sets the image loading order value.
    /// </summary>
    public static void IG_SetLoadingOrderType(ImageOrderType orderType)
    {
        if (orderType == Core.Config.ImageLoadingOrderType) return;

        Core.Config.ImageLoadingOrderType = orderType;
        IG_ReloadList();
    }


    /// <summary>
    /// Sets the image color channels.
    /// </summary>
    public void IG_SetColorChannels(string? channelsStr)
    {
        if (!Enum.TryParse<ColorChannels>(channelsStr, out var channels))
        {
            throw new ArgumentException($"""
                '{channelsStr}' is not a valid color channel.

                ----------
                👉🏼 Method: {nameof(IG_SetColorChannels)}
                """,
                nameof(channelsStr));
        }

        IG_SetColorChannels(channels);
    }


    /// <summary>
    /// Sets the image color channels.
    /// </summary>
    public static void IG_SetColorChannels(ColorChannels channels)
    {
        if (Viewer.SourceKind == PhotoSource.None || Core.IsBusy) return;

        // apply color channel filter
        if (Viewer.FilterColorChannels(channels, false))
        {
            Core.ColorChannels = channels;

            // apply transforms
            if (Core.ImageTransform.HasChanges)
            {
                _ = Viewer.RotateImage(Core.ImageTransform.Rotation, false);
                _ = Viewer.FlipImage(Core.ImageTransform.Flips, false);
            }

            Viewer.Refresh(resetZoom: false);
        }
        else
        {
            _ = Message.ShowAsync(
                Core.Lang[LangId._InvalidAction],
                Core.Lang[LangId.Menu_MnuViewChannels]);
        }
    }


    /// <summary>
    /// Open app for edit action.
    /// </summary>
    public async Task IG_OpenEditingAppAsync()
    {
        // get file path to edit
        string? filePath;
        if (Core.ClipboardImage != null)
        {
            _ = Message.ShowAsync(Core.Lang[LangId._CreatingFile], delayMs: 500);
            filePath = await Core.SavePhotoAsTempFileAsync();
        }
        else
        {
            filePath = Core.Photos.CurrentFilePath;
        }
        await Message.ClearAsync();


        if (!File.Exists(filePath))
        {
            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuOpenWith],
                Description = Core.Lang[LangId._CreatingFileError],
            });
            return;
        }


        // get extension
        var ext = Path.GetExtension(filePath).ToLowerInvariant();


        // get app from the extension
        if (EditingApp.GetFromExtension(ext) is EditingApp app)
        {
            try
            {
                var args = BHelper.BuildExeArgList(app.Executable, app.Argument, filePath);

                var result = await BHelper.RunExeCmd(args.Executable, args.Args, false, true);
                if (result == IgExitCode.Done)
                {
                    RunActionAfterEditing__();
                }
            }
            catch { }
        }
        else // edit by default associated app
        {
            Core.ShellProvider?.OpenDefaultEditingAppAsync(filePath, RunActionAfterEditing__);
        }
    }


    /// <summary>
    /// Runs the <see cref="Config.AfterEditingAction"/> action after done editing.
    /// </summary>
    private void RunActionAfterEditing__()
    {
        if (Core.Config.AfterEditingAction == AfterEditAppAction.Minimize)
        {
            App.MainWindow.WindowState = WindowState.Minimized;
        }
        else if (Core.Config.AfterEditingAction == AfterEditAppAction.Close)
        {
            IG_Exit();
        }
    }


    /// <summary>
    /// Invert image colors.
    /// </summary>
    public static void IG_InvertColors()
    {
        if (Viewer.SourceKind == PhotoSource.None || Core.IsBusy) return;

        // invert image colors
        if (Viewer.InvertColor(true))
        {
            Core.ImageTransform.IsColorInverted = Viewer.IsColorInverted;
        }
        else
        {
            _ = Message.ShowAsync(
                Core.Lang[LangId._InvalidAction],
                Core.Lang[LangId.Menu_MnuInvertColors]);
        }
    }


    /// <summary>
    /// Sets whether to play or pause the image animation.
    /// If the photo is a live/motion photo, opens the embedded video instead.
    /// </summary>
    public static async Task IG_ToggleImageAnimationAsync(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        await IG_ToggleImageAnimationAsync(enabled);
    }


    /// <summary>
    /// Sets whether to play or pause the image animation.
    /// If the photo is a live/motion photo, opens the embedded video instead.
    /// </summary>
    public static async Task IG_ToggleImageAnimationAsync(bool? enabled)
    {
        // if this is a motion/live photo, extract and play the embedded video
        var meta = Viewer.Photo?.Metadata;
        if (meta?.IsLivePhoto == true && meta.EmbeddedVideoOffsetFromEnd > 0)
        {
            await meta.OpenLivePhotoVideoAsync().ConfigureAwait(false);
            return;
        }

        enabled ??= !Viewer.IsImageAnimating;

        if (enabled.Value)
        {
            Viewer.StartAnimator();
        }
        else
        {
            Viewer.StopAnimator();
        }
    }


    /// <summary>
    /// Rotate the current image according to the rotation options.
    /// </summary>
    public void IG_Rotate(string? optionStr)
    {
        if (!Enum.TryParse<RotateOption>(optionStr, out var options))
        {
            throw new ArgumentException($"""
                '{optionStr}' is not a valid rotation option.

                ----------
                👉🏼 Method: {nameof(IG_Rotate)}
                """,
                nameof(optionStr));
        }

        IG_Rotate(options);
    }


    /// <summary>
    /// Rotate the current image according to the rotation options.
    /// </summary>
    public static void IG_Rotate(RotateOption options)
    {
        if (Viewer.SourceKind == PhotoSource.None || Core.IsBusy) return;

        var degree = options == RotateOption.Left ? -90 : 90;

        // update rotation changes
        if (Viewer.RotateImage(degree))
        {
            var currentRotation = Core.ImageTransform.Rotation + degree;
            if (Math.Abs(currentRotation) >= 360)
            {
                currentRotation %= 360;
            }

            Core.ImageTransform.Rotation = currentRotation;
        }
        else
        {
            _ = Message.ShowAsync(
                Core.Lang[LangId._InvalidAction_Transformation],
                Core.Lang[LangId._InvalidAction]);
        }
    }


    /// <summary>
    /// Flips the current image according to the flip options.
    /// </summary>
    public void IG_FlipImage(string? optionStr)
    {
        if (!Enum.TryParse<FlipOptions>(optionStr, out var options))
        {
            throw new ArgumentException($"""
                '{optionStr}' is not a valid flip option.

                ----------
                👉🏼 Method: {nameof(IG_FlipImage)}
                """,
                nameof(optionStr));
        }

        IG_FlipImage(options);
    }


    /// <summary>
    /// Flips the current image according to the flip options.
    /// </summary>
    public static void IG_FlipImage(FlipOptions options)
    {
        if (Viewer.SourceKind == PhotoSource.None || Core.IsBusy) return;

        // update flip changes
        if (Viewer.FlipImage(options))
        {
            if (options.HasFlag(FlipOptions.Horizontal))
            {
                if (Core.ImageTransform.Flips.HasFlag(FlipOptions.Horizontal))
                {
                    Core.ImageTransform.Flips ^= FlipOptions.Horizontal;
                }
                else
                {
                    Core.ImageTransform.Flips |= FlipOptions.Horizontal;
                }
            }

            if (options.HasFlag(FlipOptions.Vertical))
            {
                if (Core.ImageTransform.Flips.HasFlag(FlipOptions.Vertical))
                {
                    Core.ImageTransform.Flips ^= FlipOptions.Vertical;
                }
                else
                {
                    Core.ImageTransform.Flips |= FlipOptions.Vertical;
                }
            }
        }
        else
        {
            _ = Message.ShowAsync(
                Core.Lang[LangId._InvalidAction_Transformation],
                Core.Lang[LangId._InvalidAction]);
        }
    }


    /// <summary>
    /// Sets the viewing photo as desktop wallpaper.
    /// </summary>
    public static async Task IG_SetDesktopBackgroundAsync()
    {
        await SetSystemBackgroundAsync(false);
    }


    /// <summary>
    /// Sets the viewing photo as lock screen image.
    /// </summary>
    public static async Task IG_SetLockScreenImageAsync()
    {
        if (BHelper.OS != OSType.Windows)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }

        await SetSystemBackgroundAsync(true);
    }


    /// <summary>
    /// Sets the current photo as system background.
    /// </summary>
    /// <param name="forLockScreen">
    /// <c>true</c>: For lock screen image, <c>false</c>: for desktop wallpaper
    /// </param>
    private static async Task SetSystemBackgroundAsync(bool forLockScreen)
    {
        if (Viewer.SourceKind == PhotoSource.None || Core.ShellProvider is null) return;

        var filePath = Core.Photos.CurrentFilePath;
        var ext = Core.Photos.Current?.Extension.ToLowerInvariant() ?? string.Empty;
        _ = Message.ShowAsync(Core.Lang[LangId._CreatingFile], delayMs: 500);

        var title = forLockScreen
            ? Core.Lang[LangId.Menu_MnuSetLockScreen]
            : Core.Lang[LangId.Menu_MnuSetDesktopBackground];


        // 1. create temp image if needed
        if (Core.ClipboardImage is not null || !_desktopNativeFormats.Contains(ext))
        {
            // save image to temp file
            filePath = await Core.SavePhotoAsTempFileAsync(".jpg");
        }
        await Message.ClearAsync();


        // 2. check if file path is valid
        if (!File.Exists(filePath))
        {
            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Description = Core.Lang[LangId._CreatingFileError],
            });

            return;
        }


        // 3. set background
        try
        {
            if (forLockScreen)
            {
                await Core.ShellProvider.SetLockScreenAsync(filePath);
            }
            else
            {
                Core.ShellProvider.SetWallpaper(filePath);
            }


            var successMsg = forLockScreen
                ? Core.Lang[LangId.Menu_MnuSetLockScreen_Success]
                : Core.Lang[LangId.Menu_MnuSetDesktopBackground_Success];
            _ = Message.ShowAsync(successMsg);
        }
        catch (Exception ex)
        {
            var heading = forLockScreen
                ? Core.Lang[LangId.Menu_MnuSetLockScreen_Error]
                : Core.Lang[LangId.Menu_MnuSetDesktopBackground_Error];

            _ = await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Heading = heading,
                Description = ex.Message,
            });
        }
    }


    #endregion // Image APIs



    #region Clipboard APIs

    /// <summary>
    /// Opens image from clipboard.
    /// </summary>
    private async Task IG_PasteImageAsync()
    {
        if (App.MainWindow.Clipboard is null) return;

        using var data = await App.MainWindow.Clipboard.TryGetDataAsync();
        if (data is null) return;


        // 1. if clipboard contains a file
        if (data.Contains(DataFormat.File))
        {
            var fileItem = await data.TryGetFileAsync();
            var filePath = fileItem?.TryGetLocalPath();

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                IG_OpenPath(filePath);
            }
            return;
        }


        // 2. if clipboard contains image pixels
        if (data.Contains(DataFormat.Bitmap))
        {
            var abmp = await data.TryGetBitmapAsync();
            var skBmp = SkiaCodec.FromBitmap(abmp);
            if (skBmp is not null)
            {
                var photo = new Photo(skBmp);
                await LoadClipboardPhotoAsync(photo);
            }

            return;
        }


        // 3. if clipboard contains file path
        if (data.Contains(DataFormat.Text))
        {
            var text = await data.TryGetTextAsync();
            var path = BHelper.ResolvePath(text);

            // 3.1 try to get absolute path
            if (File.Exists(path) || Directory.Exists(path))
            {
                IG_OpenPath(path);
                return;
            }


            // 3.2 get photo from base64 string
            try
            {
                var photo = await MagickCodec.DecodeBase64Async(text);
                if (photo is not null)
                {
                    await LoadClipboardPhotoAsync(photo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ IG_PasteImageAsync: {ex.Message}");
            }
        }

    }


    public static async Task LoadClipboardPhotoAsync(Photo? photo)
    {
        // cancel the current loading image
        Core.Photos.Current?.CancelLoading();

        await App.MainWindow.PART_MainView.ViewPhotoAsync(photo, true, false);

        Core.ClipboardImage = photo;
    }


    /// <summary>
    /// Copies image pixels.
    /// </summary>
    public static async Task IG_CopyImagePixelsAsync()
    {
        if (Viewer.SourceKind == PhotoSource.None || App.MainWindow.Clipboard is null) return;

        // 1. get rendered bitmap
        var bmp = Viewer.GetRenderedBitmap(!Viewer.SourceSelection.IsEmpty);
        if (bmp.IsDisposed()) return;


        // 2. show message
        await Message.ClearAsync();
        _ = Message.ShowAsync(Core.Lang[LangId.Menu_MnuCopyImagePixels_Copying], delayMs: 1000);


        // 3. copy to clipboard
        try
        {
            var abmp = SkiaCodec.ToWritableBitmap(bmp);
            await App.MainWindow.Clipboard.SetBitmapAsync(abmp);

            _ = Message.ShowAsync(Core.Lang[LangId.Menu_MnuCopyImagePixels_Success]);
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Menu_MnuCopyImagePixels],
                Description = ex.Message,
            });
        }
    }


    /// <summary>
    /// Copy the current image path.
    /// </summary>
    public static async Task IG_CopyImagePathAsync()
    {
        if (string.IsNullOrWhiteSpace(Core.Photos.CurrentFilePath) || App.MainWindow.Clipboard is null) return;

        try
        {
            await App.MainWindow.Clipboard.SetTextAsync(Core.Photos.CurrentFilePath);

            // show message
            _ = Message.ShowAsync(Core.Lang[LangId.Menu_MnuCopyPath_Success]);
        }
        catch { }
    }


    /// <summary>
    /// Copies the current photo file.
    /// </summary>
    public static async Task IG_CopyFilesAsync()
    {
        await SetFileToClipboardAsync(Core.Photos.CurrentFilePath, false);
    }


    /// <summary>
    /// Cuts the current photo file.
    /// </summary>
    public static async Task IG_CutFilesAsync()
    {
        if (BHelper.OS == OSType.Mac)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }

        await SetFileToClipboardAsync(Core.Photos.CurrentFilePath, true);
    }


    /// <summary>
    /// Sets file to clipboard
    /// </summary>
    public static async Task SetFileToClipboardAsync(string? filePath, bool forCutting)
    {
        if (App.MainWindow.Clipboard is null || !File.Exists(filePath)) return;

        // 1. cut/copy single file
        if (forCutting)
        {
            if (!Core.Config.EnableCutMultipleFiles)
            {
                Core.StringClipboard.Clear();
            }
        }
        else
        {
            if (!Core.Config.EnableCopyMultipleFiles)
            {
                Core.StringClipboard.Clear();
            }
        }


        // 2. try adding current photo path to clipboard paths
        _ = Core.StringClipboard.Add(filePath);


        // 3. set files to clipboard
        try
        {
            var dt = new DataTransfer();
            var clipboardPaths = new List<string>(Core.StringClipboard.Count);

            foreach (var path in Core.StringClipboard)
            {
                var fi = await App.MainWindow.StorageProvider.TryGetFileFromPathAsync(path);
                if (fi is null) continue;

                var dti = new DataTransferItem();
                dti.SetFile(fi);
                dt.Add(dti);

                clipboardPaths.Add(path);
            }


            // 4. tell the paste target whether to move (cut) or copy the files
            AddFileOperationHint__(dt, clipboardPaths, forCutting);


            // 5. perform copy/cut
            await App.MainWindow.Clipboard.SetDataAsync(dt);

            _ = Message.ShowAsync(Core.Lang[forCutting
                    ? LangId.Menu_MnuCutFile_Success
                    : LangId.Menu_MnuCopyFile_Success,
                Core.StringClipboard.Count]);
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = Core.Lang[forCutting ? LangId.Menu_MnuCutFile : LangId.Menu_MnuCopyFile],
                Description = ex.Message,
            });
        }
    }


    /// <summary>
    /// Adds the platform-specific hint telling the paste target
    /// whether the files were cut (moved) or copied.
    /// </summary>
    private static void AddFileOperationHint__(DataTransfer dt, IReadOnlyList<string> filePaths, bool forCutting)
    {
        if (filePaths.Count == 0) return;
        var dti = new DataTransferItem();

        if (BHelper.OS == OSType.Windows)
        {
            // File Explorer's convention: DROPEFFECT_MOVE = 2, DROPEFFECT_COPY | DROPEFFECT_LINK = 5
            var dropEffect = forCutting ? 2u : 5u;
            dti.Set(_winDropEffectFormat, BitConverter.GetBytes(dropEffect));
        }
        else if (BHelper.OS == OSType.Linux)
        {
            var uris = filePaths.Select(path => new Uri(path).AbsoluteUri);
            var opName = forCutting ? "cut" : "copy";

            dti.Set(_gnomeCopiedFilesFormat, Encoding.UTF8.GetBytes($"{opName}\n{string.Join('\n', uris)}"));
            dti.Set(_kdeCutSelectionFormat, Encoding.UTF8.GetBytes(forCutting ? "1" : "0"));
        }
        // macOS Finder has no cut-and-paste for files
        else return;

        dt.Add(dti);
    }


    /// <summary>
    /// Clears clipboard.
    /// </summary>
    public static async Task IG_ClearClipboardAsync()
    {
        // clear clipboard
        Core.StringClipboard.Clear();

        if (App.MainWindow.Clipboard is not null)
        {
            await App.MainWindow.Clipboard.ClearAsync();
        }


        // show message
        _ = Message.ShowAsync(Core.Lang[LangId.Menu_MnuClearClipboard_Success]);
    }

    #endregion // Clipboard APIs



    #region Window Mode APIs

    /// <summary>
    /// Toggles window fit mode.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public void IG_ToggleWindowFit(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleWindowFit(enabled);
    }


    /// <summary>
    /// Toggles window fit mode.
    /// </summary>
    public void IG_ToggleWindowFit(bool? enabled = null)
    {
        enabled ??= !Core.Config.EnableWindowFit;
        Core.Config.EnableWindowFit = enabled.Value;

        if (Core.Config.EnableWindowFit)
        {
            // exit full screen
            if (Core.Config.EnableFullScreen)
            {
                IG_ToggleFullScreen(false);
            }
        }


        // set Window Fit mode
        ApplyWindowFitMode(true);
    }


    /// <summary>
    /// Adjusts the main window size and position to fit the displayed image within the available screen area.
    /// </summary>
    public static void ApplyWindowFitMode(bool resetZoomMode = true)
    {
        if (!Core.Config.EnableWindowFit || Viewer.SourceKind == PhotoSource.None) return;

        // 1. reset window state
        App.MainWindow.WindowState = WindowState.Normal;


        // 2. get the size
        var dpi = App.MainWindow.Dpi;
        var toolbarPos = Config.GetControlLayout(LayoutControl.Toolbar);
        var galleryPos = Config.GetControlLayout(LayoutControl.Gallery);

        var frameSize = Core.Config.EnableFrameless ? new() : new Size(2, 32);
        var gapW = 0d;
        var gapH = Toolbar.Bounds.Height;

        if (galleryPos is LayoutPosition.Left or LayoutPosition.Right)
        {
            gapW += Gallery.Bounds.Width + GalleryResizer.Bounds.Width;
        }
        else
        {
            gapH += Gallery.Bounds.Height + GalleryResizer.Bounds.Height;
        }


        // get current screen workarea
        var screen = App.MainWindow.Screens.ScreenFromWindow(App.MainWindow)!;
        var workArea = screen.WorkingArea.ToRect(dpi);

        // get source image size
        var srcImgW = Viewer.BitmapSize.Width;
        var srcImgH = Viewer.BitmapSize.Height;


        // 3. calculate zoom factor for the new size
        var zoomFactor = Viewer.ZoomFactor;
        if (resetZoomMode)
        {
            if (Core.Config.ZoomMode == ZoomMode.LockZoom)
            {
                Viewer.SetZoomFactor(Core.Config.ZoomLockValue / 100f, false);
            }
            else
            {
                var maxViewerWidth = workArea.Width - gapW;
                var maxViewerHeight = workArea.Height - gapH;

                // recalculate zoom factor for the new size
                zoomFactor = Viewer.CalculateZoomFactor(Core.Config.ZoomMode, srcImgW, srcImgH, maxViewerWidth, maxViewerHeight);
            }
        }


        // 4. apply zoom factor to the image size
        var zoomImgW = (int)(srcImgW * zoomFactor / dpi);
        var zoomImgH = (int)(srcImgH * zoomFactor / dpi);


        // 5. adjust the viewer size to fit the entire image
        // but not larger than desktop working area.
        var viewerBounds = new Rect(0, 0,
            Math.Min(zoomImgW, workArea.Width - gapW),
            Math.Min(zoomImgH, workArea.Height - gapH));

        var workAreaWithoutFrame = new Rect(
            workArea.X + gapW / 2 + frameSize.Width / 2,
            workArea.Y + gapH / 2 + frameSize.Height / 2,
            workArea.Width - gapW - frameSize.Width,
            workArea.Height - gapH - frameSize.Height);

        // adjust viewer size and position to the desktop working area
        viewerBounds = workAreaWithoutFrame.CenterRectEx(viewerBounds, true);

        // add the gaps to make window bound
        var winBounds = new Rect(
            viewerBounds.X - gapW / 2 - frameSize.Width / 2,
            viewerBounds.Y - gapH / 2 - frameSize.Height / 2,
            viewerBounds.Width + gapW,
            viewerBounds.Height + gapH);


        // check center window to screen option
        if (!Core.Config.EnableCenterWindowFit)
        {
            winBounds = winBounds.WithX(App.MainWindow.Position.X / dpi);
            winBounds = winBounds.WithY(App.MainWindow.Position.Y / dpi);
        }


        // 6. set min size for window
        App.MainWindow.MinWidth = gapW + 50;
        App.MainWindow.MinHeight = gapH + 50;

        // update window position and size
        App.MainWindow.Position = new((int)(winBounds.X * dpi), (int)(winBounds.Y * dpi));
        App.MainWindow.Width = winBounds.Width;
        App.MainWindow.Height = winBounds.Height;

        if (resetZoomMode)
        {
            Viewer.SetZoomFactor(zoomFactor, false);
        }

    }


    /// <summary>
    /// Toggles frameless mode.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public void IG_ToggleFrameless(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleFrameless(enabled);
    }


    /// <summary>
    /// Toggles frameless mode.
    /// </summary>
    public void IG_ToggleFrameless(bool? enabled = null)
    {
        if (BHelper.OS == OSType.Linux)
        {
            throw new NotSupportedException($"IGE: This feature is not supported on {BHelper.OS}.");
        }


        enabled ??= !Core.Config.EnableFrameless;

        // set frameless mode
        SetFramelessMode__(enabled.Value, true);
    }
    private void SetFramelessMode__(bool enabled, bool showMessage)
    {
        Core.Config.EnableFrameless = enabled;


        // set frameless mode
        if (enabled)
        {
            // exit full screen
            if (Core.Config.EnableFullScreen) IG_ToggleFullScreen(false);

            App.MainWindow.IsFrameless = true;
        }

        // restore frame
        else
        {
            App.MainWindow.IsFrameless = false;
        }


        // update window fit
        if (Core.Config.EnableWindowFit)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApplyWindowFitMode(!Viewer.IsManualZoom);
            });
        }


        // show message
        if (showMessage && enabled)
        {
            _ = Message.ShowAsync(
                Core.Lang[LangId.Menu_MnuFrameless_EnableDescription],
                Core.Lang[LangId.Menu_MnuFrameless]);
        }
    }


    /// <summary>
    /// Toggles fullscreen mode.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public void IG_ToggleFullScreen(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleFullScreen(enabled);
    }


    /// <summary>
    /// Toggles fullscreen mode.
    /// </summary>
    public void IG_ToggleFullScreen(bool? enabled = null)
    {
        enabled ??= !Core.Config.EnableFullScreen;
        SetFullScreenMode__(enabled.Value,
            !Core.Config.ShowToolbarInFullscreen, !Core.Config.ShowGalleryInFullscreen);
    }
    private void SetFullScreenMode__(bool enabled, bool hideToolbar = false, bool hideThumbnails = false)
    {
        Core.Config.EnableFullScreen = enabled;

        // enable full screen mode
        if (enabled)
        {
            // back up the windowed layout before anything below writes over it in Config
            _preFullScreenLayout ??= CapturePreFullScreenLayout__();

            // exit window fit & frameless; the backup brings them back
            if (Core.Config.EnableWindowFit) IG_ToggleWindowFit(false);
            if (Core.Config.EnableFrameless) SetFramelessMode__(false, false);


            // enable fullscreen
            App.MainWindow.WindowState = WindowState.FullScreen;


            // hide toolbar & gallery
            if (hideToolbar) IG_ToggleToolbar(false);
            if (hideThumbnails) _ = IG_ToggleGalleryAsync(false);
        }

        // disable full screen mode
        else
        {
            // with no backup, full screen was never entered in this process, so the saved config
            // is still the windowed layout
            var layout = _preFullScreenLayout ?? ReadWindowLayoutFromConfig__();
            _preFullScreenLayout = null;

            // restore layout
            IG_ToggleToolbar(layout.ShowToolbar);
            _ = IG_ToggleGalleryAsync(layout.ShowGallery);


            // restore window state, size, position
            Core.Config.EnableMainWindowMaximized = layout.IsMaximized;
            App.MainWindow.WindowState = layout.IsMaximized
                ? WindowState.Maximized
                : WindowState.Normal;


            // restore frameless, window fit mode when exiting full screen
            if (layout.IsFrameless) SetFramelessMode__(true, false);

            // window fit measures the toolbar & gallery, whose visibility change is still queued
            if (layout.IsWindowFit) Dispatcher.UIThread.Post(() => IG_ToggleWindowFit(true));
        }
    }


    /// <summary>
    /// Captures the layout to return to when full screen mode is turned off. A running slideshow
    /// has already hidden the toolbar &amp; gallery in Config, so take its backup of them instead.
    /// </summary>
    private WindowLayoutSnapshot CapturePreFullScreenLayout__()
    {
        var layout = App.MainWindow.CaptureWindowLayout();
        if (!Core.Config.EnableSlideshow) return layout;

        return layout with
        {
            ShowToolbar = _showToolbarBeforeSlideshow,
            ShowGallery = _showGalleryBeforeSlideshow,
        };
    }


    /// <summary>
    /// Reads the windowed layout from the saved config, which is where it survives between sessions.
    /// </summary>
    private static WindowLayoutSnapshot ReadWindowLayoutFromConfig__() => new()
    {
        IsMaximized = Core.Config.EnableMainWindowMaximized,
        Bounds = Core.Config.MainWindowBounds,
        ShowToolbar = Core.Config.ShowToolbar,
        ShowGallery = Core.Config.ShowGallery,
        IsFrameless = Core.Config.EnableFrameless,
        IsWindowFit = Core.Config.EnableWindowFit,
    };


    /// <summary>
    /// Toggles slideshow mode.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public void IG_ToggleSlideshow(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleSlideshow(enabled);
    }


    /// <summary>
    /// Toggles slideshow mode.
    /// </summary>
    public void IG_ToggleSlideshow(bool? enabled = null)
    {
        enabled ??= !Core.Config.EnableSlideshow;

        if (enabled.Value)
        {
            StartSlideshow__();
        }
        else
        {
            StopSlideshow__();
        }
    }
    private void StartSlideshow__()
    {
        if (Core.Config.EnableSlideshow) return;
        if (Core.Photos.Count == 0) return;

        // 1. back up current window state
        _isFullScreenBeforeSlideshow = Core.Config.EnableFullScreen;
        _isFramelessBeforeSlideshow = Core.Config.EnableFrameless;
        _isWindowFitBeforeSlideshow = Core.Config.EnableWindowFit;
        _showToolbarBeforeSlideshow = Core.Config.ShowToolbar;
        _showGalleryBeforeSlideshow = Core.Config.ShowGallery;
        _windowMaximizedBeforeSlideshow = App.MainWindow.WindowState == WindowState.Maximized;


        // 2. enter full screen if configured
        if (Core.Config.EnableFullscreenSlideshow && !Core.Config.EnableFullScreen)
        {
            // back it up like a normal full screen entry, so leaving full screen mid-slideshow
            // (or quitting from it) still has a windowed layout to return to
            _preFullScreenLayout = CapturePreFullScreenLayout__();

            // exit window fit and frameless first
            if (Core.Config.EnableWindowFit) IG_ToggleWindowFit(false);
            if (Core.Config.EnableFrameless) SetFramelessMode__(false, false);

            App.MainWindow.WindowState = WindowState.FullScreen;
            Core.Config.EnableFullScreen = true;
        }

        // hide toolbar and gallery
        IG_ToggleToolbar(false);
        _ = IG_ToggleGalleryAsync(false);


        // 3. create and start the slideshow service
        var slideshow = new SlideshowProvider();
        slideshow.NextPhotoRequested += OnSlideshowNextPhoto__;
        Core.Slideshow = slideshow;

        Core.Config.EnableSlideshow = true;
        slideshow.Start();


        // 4. start countdown refresh timer for the viewer overlay
        SetSlideshowCountdown(Core.Config.EnableSlideshowCountdown);
    }

    private void StopSlideshow__()
    {
        if (!Core.Config.EnableSlideshow) return;

        Core.Config.EnableSlideshow = false;

        // 1. stop countdown timer
        SetSlideshowCountdown(false);


        // 2. stop and dispose the slideshow service
        if (Core.Slideshow is { } service)
        {
            service.NextPhotoRequested -= OnSlideshowNextPhoto__;
            service.Dispose();
            Core.Slideshow = null;
        }

        // release the forced look-ahead image the slideshow preloaded; when no memory
        // budget is set, a normal cache pass would early-return without unloading it
        if (Core.Config.CacheMaxMemoryInMb == 0)
        {
            Core.Photos.ClearCache();
        }


        // 3. restore window state
        if (Core.Config.EnableFullscreenSlideshow && !_isFullScreenBeforeSlideshow)
        {
            Core.Config.EnableFullScreen = false;
            _preFullScreenLayout = null;

            Core.Config.EnableMainWindowMaximized = _windowMaximizedBeforeSlideshow;
            App.MainWindow.WindowState = _windowMaximizedBeforeSlideshow
                ? WindowState.Maximized
                : WindowState.Normal;
        }

        // restore toolbar and gallery
        IG_ToggleToolbar(_showToolbarBeforeSlideshow);
        _ = IG_ToggleGalleryAsync(_showGalleryBeforeSlideshow);

        // restore frameless and window fit
        if (_isFramelessBeforeSlideshow) SetFramelessMode__(true, false);
        if (_isWindowFitBeforeSlideshow) IG_ToggleWindowFit(true);
    }

    private void OnSlideshowNextPhoto__()
    {
        _slideshowIsAdvancing = true;
        IG_ViewNext();
        _slideshowIsAdvancing = false;
    }



    /// <summary>
    /// Toggle slideshow countdown.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public void IG_ToggleSlideshowCountdown(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleSlideshowCountdown(enabled);
    }


    /// <summary>
    /// Toggle slideshow countdown.
    /// </summary>
    public void IG_ToggleSlideshowCountdown(bool? enabled = null)
    {
        enabled ??= !Core.Config.EnableSlideshowCountdown;
        Core.Config.EnableSlideshowCountdown = enabled.Value;

        SetSlideshowCountdown(enabled.Value);
    }
    private void SetSlideshowCountdown(bool enabled)
    {
        if (enabled)
        {
            // start countdown timer
            _slideshowCountdownTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Render,
                (_, _) => SlideshowCountdown.InvalidateVisual());
            _slideshowCountdownTimer.Start();
        }
        else
        {
            // stop countdown timer
            _slideshowCountdownTimer?.Stop();
            _slideshowCountdownTimer = null;
            SlideshowCountdown.InvalidateVisual();
        }
    }


    /// <summary>
    /// Plays or pauses the current slideshow.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static void IG_ToggleSlideshowPlayback(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleSlideshowPlayback(enabled);
    }


    /// <summary>
    /// Plays or pauses the current slideshow.
    /// </summary>
    public static void IG_ToggleSlideshowPlayback(bool? enabled = null)
    {
        if (Core.Slideshow?.IsRunning != true) return;
        var isPaused = Core.Slideshow?.IsPaused ?? false;

        if (isPaused) Core.Slideshow?.Resume();
        else Core.Slideshow?.Pause();

        _ = Message.ShowAsync(Core.Lang[isPaused
            ? LangId._ResumeSlideshow
            : LangId._PauseSlideshow]);
    }

    #endregion // Window Modes APIs



    #region Layout APIs

    /// <summary>
    /// Toggles visibility of toolbar.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static void IG_ToggleToolbar(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleToolbar(enabled);
    }


    /// <summary>
    /// Toggles visibility of toolbar.
    /// </summary>
    public static void IG_ToggleToolbar(bool? enabled = null)
    {
        enabled ??= !Core.Config.ShowToolbar;
        Core.Config.ShowToolbar = enabled.Value;

        // update window fit
        if (Core.Config.EnableWindowFit)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApplyWindowFitMode(!Viewer.IsManualZoom);
            });
        }
    }


    /// <summary>
    /// Toggles visibility of gallery.
    /// </summary>
    public static async Task IG_ToggleGalleryAsync(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        await IG_ToggleGalleryAsync(enabled);
    }


    /// <summary>
    /// Toggles visibility of gallery
    /// </summary>
    public static async Task IG_ToggleGalleryAsync(bool? enabled = null)
    {
        enabled ??= !Core.Config.ShowGallery;
        Core.Config.ShowGallery = enabled.Value;

        if (enabled.Value)
        {
            Gallery.ScrollToItem(Core.Photos.CurrentIndex);
        }

        App.MainWindow.PART_MainView.ApplyAppLayout();

        // update window fit
        if (Core.Config.EnableWindowFit)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApplyWindowFitMode(!Viewer.IsManualZoom);
            });
        }
    }


    /// <summary>
    /// Toggles the viewer's checkerboard mode.
    /// </summary>
    public static void IG_ToggleCheckerboard(string? mode = null)
    {
        var isValid = Enum.TryParse<CheckerboardType>(mode, out var wantedMode);

        IG_ToggleCheckerboard(isValid ? wantedMode : null);
    }


    /// <summary>
    /// Toggles the viewer's checkerboard mode.
    /// </summary>
    public static void IG_ToggleCheckerboard(CheckerboardType? mode = null)
    {
        var wantedMode = Core.Config.CheckerboardMode;

        if (mode is not null)
        {
            wantedMode = mode.Value;
        }
        else
        {
            var hasAlpha = Core.Photos.CurrentMetadata?.HasAlpha ?? false;

            if (Core.Config.CheckerboardMode == CheckerboardType.None)
            {
                wantedMode = hasAlpha
                    ? CheckerboardType.Image
                    : CheckerboardType.Client;
            }
            else if (Core.Config.CheckerboardMode == CheckerboardType.Image)
            {
                wantedMode = CheckerboardType.Client;
            }
            else if (Core.Config.CheckerboardMode == CheckerboardType.Client)
            {
                wantedMode = CheckerboardType.None;
            }
        }

        Core.Config.CheckerboardMode = wantedMode;
    }


    /// <summary>
    /// Toggles window top most.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static void IG_ToggleWindowTopMost(string? boolStr = null)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr);
        IG_ToggleWindowTopMost(enabled);
    }


    /// <summary>
    /// Toggles window top most.
    /// </summary>
    public static void IG_ToggleWindowTopMost(bool? enabled = null)
    {
        enabled ??= !Core.Config.EnableWindowTopMost;
        Core.Config.EnableWindowTopMost = enabled.Value;

        _ = Message.ShowAsync(Core.Lang[enabled.Value
            ? LangId.Menu_MnuToggleTopMost_Enable
            : LangId.Menu_MnuToggleTopMost_Disable]);
    }


    /// <summary>
    /// Sets background color,
    /// opens Color Picker dialog if the <paramref name="hexColor"/> is <c>null</c>.
    /// </summary>
    public static async Task IG_SetBackgroundColorAsync(string? hexColor = null)
    {
        Color? newColor = null;

        // 1. open Color picker to select color if hexColor not defined
        if (string.IsNullOrEmpty(hexColor))
        {
            var oldColor = Resx.Get<SolidColorBrush>(ResxId.IG_ViewerBackgroundBrush).Color;
            var defaultColor = Core.Config.EnableSlideshow
                ? Colors.Black
                : Const.COLOR_EMPTY;

            var cp = new PhColorPickerDialog(oldColor, defaultColor)
            {
                Title = Core.Lang[LangId.Menu_MnuChangeBackgroundColor],
            };
            var result = await cp.ShowAsync(App.MainWindow);

            if (result != DialogExitCode.OK) return;
            newColor = cp.SelectedColor;
        }

        // 2. convert the input hex color string
        else
        {
            newColor = BHelper.ColorFromHex(hexColor);
        }

        if (newColor == null) return;


        // 3. set background color
        if (Core.Config.EnableSlideshow)
        {
            Core.Config.SlideshowBackgroundColor = newColor.Value.ToHex();
        }
        else
        {
            Core.Config.BackgroundColor = newColor.Value.ToHex();
        }
    }

    #endregion // Layout APIs



    #region Tools APIs

    /// <summary>
    /// Toggles a tool by ID. Non-hosted tools only support open (toggle = open).
    /// </summary>
    public static void IG_ToggleTool(string? toolId)
    {
        if (string.IsNullOrEmpty(toolId)) return;
        if (Core.ToolRegistry.Get(toolId) is not { } tool) return;

        if (tool.IsHosted)
        {
            var currentToolId = ToolHost.Tool?.ToolId;

            if (string.Equals(currentToolId, toolId, StringComparison.Ordinal))
            {
                // Close: save settings first, then close UI
                if (ToolHost.Tool is { } currentTool)
                {
                    ToolRegistry.SaveToolSettings(currentTool);
                }

                ToolHost.CloseCurrentTool();
                Core.Config.LastOpenedTool = "";
            }
            else
            {
                // Open: close current (with save), then open new
                IG_CloseTool(currentToolId);

                if (tool is ToolControlAdapter adapter)
                {
                    var control = adapter.CreateToolControl(Viewer);
                    ToolRegistry.LoadToolSettings(control);
                    ToolHost.OpenTool(control);

                    // save current tool setting if it's open
                    Core.Config.LastOpenedTool = control.ToolId;
                }
            }
        }
        else
        {
            // Non-hosted tools can only be opened, not toggled closed
            IG_OpenTool(toolId);
        }
    }


    /// <summary>
    /// Opens a tool by ID. Handles both hosted and non-hosted plugins.
    /// Settings are loaded before the tool is opened/executed.
    /// </summary>
    public static void IG_OpenTool(string? toolId)
    {
        if (string.IsNullOrEmpty(toolId)) return;
        if (Core.ToolRegistry.Get(toolId) is not { } tool) return;

        if (tool.IsHosted)
        {
            // Hosted tool: create via adapter, load settings, host in ToolHostControl
            var currentToolId = ToolHost.Tool?.ToolId;
            if (string.Equals(currentToolId, toolId, StringComparison.Ordinal)) return;

            // Close current tool (with save) before opening new one
            IG_CloseTool(currentToolId);

            if (tool is ToolControlAdapter adapter)
            {
                var control = adapter.CreateToolControl(Viewer);
                ToolRegistry.LoadToolSettings(control);
                ToolHost.OpenTool(control);

                // save current tool setting if it's open
                Core.Config.LastOpenedTool = control.ToolId;
            }
        }
        else
        {
            if (tool is ExternalToolProxy extProxy)
            {
                // Only allow one instance of a non-hosted external tool
                if (Core.ToolRegistry.ExternalTools.IsRunning(toolId)) return;

                // launch out-of-process; show a fix-it dialog if it can't be started
                _ = LaunchExternalToolAsync(extProxy);
            }
            else
            {
                // Built-in non-hosted tool: set viewer, load settings, execute
                tool.Viewer = Viewer;
                ToolRegistry.LoadToolSettings(tool);

                var context = new ToolExecutionContext { Window = App.MainWindow };
                _ = ToolRegistry.ExecuteNonHostedToolAsync(tool, context);
            }
        }
    }


    /// <summary>
    /// Closes a tool by ID. Only applicable to hosted tools.
    /// Saves settings before closing.
    /// </summary>
    public static void IG_CloseTool(string? toolId)
    {
        if (string.IsNullOrEmpty(toolId)) return;

        // Save settings for the current hosted tool before closing
        if (ToolHost.Tool is ITool currentTool
            && string.Equals(currentTool.ToolId, toolId, StringComparison.Ordinal))
        {
            ToolRegistry.SaveToolSettings(currentTool);
        }

        ToolHost.CloseTool(toolId);
        Core.Config.LastOpenedTool = "";
    }


    /// <summary>
    /// Closes the currently active tool in the tool host, if one is open.
    /// </summary>
    public static void IG_CloseCurrentTool()
    {
        if (ToolHost.Tool is ITool currentTool)
        {
            ToolRegistry.SaveToolSettings(currentTool);
        }
        ToolHost.CloseCurrentTool();
    }


    /// <summary>
    /// Launches an external tool; if it can't be started, offers to fix it in Settings > Tools.
    /// </summary>
    private static async Task LaunchExternalToolAsync(ExternalToolProxy proxy)
    {
        var context = new ToolExecutionContext { Window = App.MainWindow };
        var result = await proxy.TryLaunchAsync(context);
        if (!result.Success)
        {
            await ShowToolLaunchFailedAsync(proxy.Tool, result.Error);
        }
    }


    /// <summary>
    /// Shows a dialog when an external tool fails to launch, surfacing the failure reason
    /// and offering to fix the tool in Settings > Tools.
    /// </summary>
    private static async Task ShowToolLaunchFailedAsync(ExternalTool tool, string? error)
    {
        var toolName = string.IsNullOrWhiteSpace(tool.ToolName) ? tool.ToolId : tool.ToolName;

        var result = await ModalWindow.ShowAsync(App.MainWindow, new ModalWindowOptions
        {
            Title = toolName,
            Heading = Core.Lang[LangId.Settings_Tools_ToolLaunchFailed, toolName],
            Description = Core.Lang[LangId.Settings_Tools_ToolLaunchFailed_Description],
            Details = error,
            ThumbnailIcon = StockIconId.Warning,
        }, ModalWindowButton.Yes_No);

        // Yes: open Settings > Tools to edit the tool
        if (result.ExitCode == DialogExitCode.OK)
        {
            await OpenToolSettingsAsync(tool.ToolId);
        }
    }


    /// <summary>
    /// Opens Settings > Tools and the edit dialog for the given tool id, reusing the open window if any.
    /// </summary>
    private static async Task OpenToolSettingsAsync(string toolId)
    {
        // reuse the already-open settings window
        if (App.SettingsWindow is not null)
        {
            App.SettingsWindow.RestoreAndActivate();
            App.SettingsWindow.NavigateToTool(toolId);
            return;
        }

        App.SettingsWindow = new SettingsWindow(SettingsNavId.Tools.ToString(), toolId);
        try
        {
            await App.SettingsWindow.ShowAsync(null);
        }
        finally
        {
            App.SettingsWindow = null;
        }
    }

    #endregion // Tools APIs



    #region Help APIs

    /// <summary>
    /// Open About window.
    /// </summary>
    public static async Task IG_OpenAboutWindowAsync()
    {
        var dialog = new AboutWindow();
        _ = await dialog.ShowAsync(App.MainWindow);
    }


    /// <summary>
    /// Open the Pro license window (Upgrade when unlicensed, Manage when licensed).
    /// </summary>
    public static async Task IG_ManageLicenseAsync()
    {
        var dialog = new ManageLicenseWindow();
        _ = await dialog.ShowAsync(App.MainWindow);
    }


    /// <summary>
    /// Checks for new update asynchronously with option to shows UI feedback.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static async Task IG_CheckForUpdateAsync(string? boolStr = null)
    {
        var showUI = BHelper.ConvertStringToBool(boolStr);
        await IG_CheckForUpdateAsync(showUI ?? true);
    }


    /// <summary>
    /// Checks for new update asynchronously with option to shows UI feedback.
    /// </summary>
    public static async Task IG_CheckForUpdateAsync(bool showUI = true)
    {
        if (showUI) _hasManualUpdateCheck.SetTrue();

        // silent mode: skip if disabled or checked recently
        if (!showUI)
        {
            if (string.Equals(Core.Config.AutoUpdate, "0", StringComparison.Ordinal)) return;
            if (!UpdateProvider.ShouldCheck) return;

            // delay to let the app finish starting
            await Task.Delay(TimeSpan.FromSeconds(10));
        }


        UpdateWindow? updateWindow = null;

        if (showUI)
        {
            // open UpdateWindow immediately in "checking" state
            updateWindow = new UpdateWindow();
            updateWindow.SetCheckingState();

            // show the window non-blocking, then perform the check
            _ = updateWindow.ShowAsync(App.MainWindow);
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var result = await Core.UpdateProvider.CheckForUpdateAsync(cts.Token, isScheduled: !showUI);

        if (showUI)
        {
            // show check progress
            await Task.Delay(1000);

            // transition the already-open window to result state
            updateWindow!.SetResultState(result);
        }
        else
        {
            // auto-download when enabled; the ready package shows the menu affordance, not a popup
            var download = await Core.UpdateProvider.TryDownloadUpdateAsync(result, CancellationToken.None);
            if (download.IsSuccess) return;

            // silent mode: only show window for an update the user has not already checked for
            if (result.Status == Update.UpdateCheckStatus.UpdateAvailable && !_hasManualUpdateCheck)
            {
                updateWindow = new UpdateWindow();
                updateWindow.SetResultState(result);
                _ = await updateWindow.ShowAsync(App.MainWindow);
            }
        }
    }


    /// <summary>
    /// Installs the downloaded update and relaunches the app.
    /// </summary>
    public static async Task IG_InstallUpdateAsync(string? boolStr = null)
    {
        var needsConfirm = BHelper.ConvertStringToBool(boolStr) ?? true;
        await IG_InstallUpdateAsync(needsConfirm);
    }


    /// <summary>
    /// Installs the downloaded update and relaunches the app.
    /// </summary>
    public static async Task IG_InstallUpdateAsync(bool needsConfirm)
    {
        var provider = Core.UpdateProvider;
        var version = Core.Config.UpdatePendingVersion;
        var title = Core.Lang[LangId._CheckForUpdate];

        // self-guard: reachable without RunApiAsync, so the admin lock is enforced here too
        if (FeatureManager.IsLocked(API.IG_InstallUpdate)) return;
        if (string.IsNullOrWhiteSpace(version)) return;

        var pkgPath = provider.GetPendingPackagePath();
        if (pkgPath is null)
        {
            provider.DiscardPendingUpdate();
            _ = await Core.Config.SaveAsync();

            var cacheDir = BHelper.ConfigDir(Dir.Cache, Update.UpdateConstants.PackageCacheDir);
            var realCacheDir = BHelper.GetRealPlatformPath(cacheDir);

            await ShowUpdateFailedAsync(Update.UpdateOpResult.Fail(
                $"IGE: The downloaded update package for {version} is no longer in the cache folder.", realCacheDir));
            return;
        }

        // the menu entry is one click from killing the app; the dialog's [Restart now] already confirmed
        if (needsConfirm)
        {
            var confirm = await ModalWindow.ShowAsync(App.MainWindow, new ModalWindowOptions
            {
                Title = title,
                Heading = Core.Lang[LangId.Menu_MnuCheckForUpdate_ReadyToInstall],
                Description = Core.Lang[LangId.Menu_MnuInstallUpdate_Confirm],
                Thumbnail = Resx.GetSvg(ResxSvgId.StarStruck),
            }, ModalWindowButton.OK_Cancel);
            if (confirm.ExitCode != DialogExitCode.OK) return;
        }

        // deployment terminates the process, so OnClosing never runs: persist window state now
        if (App.MainWindow is { } mainWindow)
        {
            _ = await mainWindow.CaptureAndSaveConfigAsync();
        }

        var release = new Update.UpdateReleaseInfo { Version = version };
        var apply = await provider.ApplyAndRestartAsync(pkgPath, release);

        // on success the process is already being torn down by the installer
        if (!apply.IsSuccess && !apply.IsSkipped)
        {
            await ShowUpdateFailedAsync(apply);
        }
    }


    /// <summary>
    /// Reports the real reason a self-update failed and offers the download page instead.
    /// </summary>
    public static async Task ShowUpdateFailedAsync(Update.UpdateOpResult opResult, PhWindow? owner = null)
    {
        owner ??= App.MainWindow;

        var result = await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
        {
            Title = Core.Lang[LangId._CheckForUpdate],
            Heading = opResult.ErrorMessage ?? Core.Lang[LangId.Menu_MnuCheckForUpdate_Failed],
            Details = opResult.ErrorDetails,
        });
    }


    /// <summary>
    /// Opens the "ImageGlass Quick Setup" wizard window.
    /// </summary>
    public static async Task IG_QuickSetupAsync()
    {
        var dialog = new QuickSetupWindow();
        _ = await dialog.ShowAsync(App.MainWindow);
    }


    /// <summary>
    /// Opens website to report issue.
    /// </summary>
    public static void IG_ReportIssue()
    {
        _ = BHelper.OpenUrlAsync(App.MainWindow,
            "https://github.com/d2phap/ImageGlass/issues?q=is%3Aissue+",
            "from_report_issue");
    }


    /// <summary>
    /// Registers the app as the default photo viewer.
    /// </summary>
    public static async Task IG_SetDefaultPhotoViewerAsync()
    {
        await SetDefaultPhotoViewerAsync(true);
    }


    /// <summary>
    /// Unregisters the app from the default photo viewer.
    /// </summary>
    public static async Task IG_RemoveDefaultPhotoViewerAsync()
    {
        await SetDefaultPhotoViewerAsync(false);
    }


    /// <summary>
    /// Sets or removes the app as the default photo viewer for supported file formats.
    /// </summary>
    /// <param name="owner">Modal owner</param>
    /// <param name="lang">Language for the result dialog</param>
    public static async Task SetDefaultPhotoViewerAsync(bool enable, PhWindow? owner = null, Lang? lang = null)
    {
        if (Core.ShellProvider is null) return;

        owner ??= App.MainWindow;
        lang ??= Core.Lang;

        var extensions = Core.GetSupportedFileExtensions().ToArray();

        try
        {
            var scope = await Core.ShellProvider.SetDefaultPhotoViewerAsync(extensions, enable);

            // null = the provider can't do it (virtualized Store MSIX); UI is hidden, so just stop
            if (scope is null) return;

            // let the user know whether the change is per-machine (all users) or per-user
            var scopeText = lang[scope == DefaultAppScope.LocalMachine
                ? LangId.Settings_DefaultPhotoViewer_ScopePerMachine
                : LangId.Settings_DefaultPhotoViewer_ScopePerUser];

            await ModalWindow.ShowInfoAsync(owner, new ModalWindowOptions
            {
                Title = lang[enable
                    ? LangId.Menu_MnuSetDefaultPhotoViewer
                    : LangId.Menu_MnuRemoveDefaultPhotoViewer],
                Heading = lang[enable
                    ? LangId.Menu_MnuSetDefaultPhotoViewer_Success
                    : LangId.Menu_MnuRemoveDefaultPhotoViewer_Success],
                Description = scopeText,
                Note = enable ? lang[LangId.Settings_UnmanagedSettingReminder] : null,
                NoteStyle = InfoBarSeverity.Warning,
            });
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
            {
                Title = lang[enable
                    ? LangId.Menu_MnuSetDefaultPhotoViewer
                    : LangId.Menu_MnuRemoveDefaultPhotoViewer],
                Heading = lang[enable
                    ? LangId.Menu_MnuSetDefaultPhotoViewer_Error
                    : LangId.Menu_MnuRemoveDefaultPhotoViewer_Error],
                Description = ex.Message,
                Details = ex.ToString(),
            });
        }
    }


    #endregion // Help APIs



    #region Other APIs

    /// <summary>
    /// Sets the real-time file update engine.
    /// </summary>
    /// <param name="boolStr">Values: <c>"true"</c>, <c>"false"</c> or empty.</param>
    public static void IG_SetFileWatcher(string? boolStr)
    {
        var enabled = BHelper.ConvertStringToBool(boolStr) ?? false;
        IG_SetFileWatcher(enabled);
    }


    /// <summary>
    /// Sets the real-time file update engine.
    /// </summary>
    public static void IG_SetFileWatcher(bool enabled)
    {
        Core.Config.EnableFileWatcher = enabled;
    }


    /// <summary>
    /// Sets the real-time file update engine without updating the setting <see cref="Config.EnableFileWatcher"/>.
    /// </summary>
    public static void SetFileWatcher(bool enabled)
    {
        // file watcher is a Pro feature
        if (enabled && !Core.IsProEnabled) enabled = false;

        if (enabled)
        {
            // always prefer the current photo list's directory so the watcher
            // follows folder changes (e.g. opening a photo in a new folder)
            var dirPath = Core.Photos.DistinctDirs.FirstOrDefault();

            if (string.IsNullOrEmpty(dirPath))
            {
                dirPath = Core.Photos.FileWatcherFolderPath;
            }

            if (!string.IsNullOrEmpty(dirPath))
            {
                Core.Photos.StartFileWatcher(dirPath);
            }
        }
        else
        {
            Core.Photos.StopFileWatcher();
        }
    }


    /// <summary>
    /// Opens the context menu associated with the viewer.
    /// </summary>
    public static void IG_OpenContextMenu()
    {
        Viewer.ContextMenu?.Open();
    }


    #endregion // Other APIs

}
