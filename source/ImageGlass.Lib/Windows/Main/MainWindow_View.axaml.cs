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
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using D2Phap.FileWatcherEx;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using ImageGlass.Common.Types;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Viewer.ZoomAndPan;
using ImageGlass.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class MainWindowView : PhControl
{
    // how long to wait for the first paint before giving up (hidden window may never draw)
    private static readonly TimeSpan _sequentialRenderTimeout = TimeSpan.FromSeconds(2);

    // > 0 while a photo load + paint is in flight, Sequential browsing only
    private int _sequentialLoadDepth;

    // land on the last photo of the list being searched (backward folder switch)
    private bool _selectLastOnSearch;


    public MainWindowViewModel VM => (MainWindowViewModel)DataContext!;


    /// <summary>
    /// Gets whether a photo is currently loading and rendering under
    /// <see cref="BrowsingMode.Sequential"/>.
    /// </summary>
    public bool IsPhotoLoadInProgress => Volatile.Read(ref _sequentialLoadDepth) > 0;


    public MainWindowView()
    {
        InitializeComponent();

        PART_Viewer.ContextMenu = new();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        // apply app layout from settings
        ApplyAppLayout();

        base.OnLoaded(e);

        // drag-drop events
        DragDrop.SetAllowDrop(PART_Viewer, true);
        DragDrop.AddDragOverHandler(PART_Viewer, PART_Viewer_DragOver);
        DragDrop.AddDropHandler(PART_Viewer, PART_Viewer_Drop);

        Core.Config.PropertyChanged += Config_PropertyChanged;
        Core.Photos.FileWatcherChanged += Photos_FileWatcherChanged;
        PART_Viewer.PhotoLoading += PART_Viewer_PhotoLoading;
        PART_Viewer.ZoomChanged += PART_Viewer_ZoomChanged;
        PART_Viewer.NavButtonClicked += PART_Viewer_NavButtonClicked;
        PART_Viewer.ViewerPointerClicked += PART_Viewer_ViewerPointerClicked;
        PART_Viewer.ViewerMouseWheel += PART_Viewer_ViewerMouseWheel;
        PART_Viewer.ContextMenu?.Opened += PART_Viewer_ContextMenu_Opened;

        // motion/live photo overlay button
        PART_ToolHost.PropertyChanged += PART_ToolHost_PropertyChanged;
        PART_BtnMotionVideo.Click += PART_BtnMotionVideo_Click;
        UpdateMotionButtonTooltip();
        UpdateMotionButtonState();

        // hook viewer events for external tool broadcasting
        PART_Viewer.PhotoLoading += Core.Viewer_PhotoLoadingForPlugins;
        PART_Viewer.ViewerPointerMoved += Core.Viewer_PointerMovedForPlugins;
        PART_Viewer.ViewerPointerPressed += Core.Viewer_PointerPressedForPlugins;
        PART_Viewer.SelectionChanged += Core.Viewer_SelectionChangedForPlugins;
        PART_Viewer.PhotoFrameChanged += Core.Viewer_FrameChangedForPlugins;


        // load image from command line arguments
        LoadImagesFromCmdArgs();

        StartupTrace.Mark("MainView:loaded");
        StartupTrace.Flush();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // drag-drop events
        DragDrop.RemoveDragOverHandler(PART_Viewer, PART_Viewer_DragOver);
        DragDrop.RemoveDropHandler(PART_Viewer, PART_Viewer_Drop);

        Core.Config.PropertyChanged -= Config_PropertyChanged;
        Core.Photos.FileWatcherChanged -= Photos_FileWatcherChanged;
        PART_Viewer.PhotoLoading -= PART_Viewer_PhotoLoading;
        PART_Viewer.ZoomChanged -= PART_Viewer_ZoomChanged;
        PART_Viewer.NavButtonClicked -= PART_Viewer_NavButtonClicked;
        PART_Viewer.ViewerPointerClicked -= PART_Viewer_ViewerPointerClicked;
        PART_Viewer.ViewerMouseWheel -= PART_Viewer_ViewerMouseWheel;
        PART_Viewer.ContextMenu?.Opened -= PART_Viewer_ContextMenu_Opened;

        // motion/live photo overlay button
        PART_ToolHost.PropertyChanged -= PART_ToolHost_PropertyChanged;
        PART_BtnMotionVideo.Click -= PART_BtnMotionVideo_Click;

        // unhook viewer events for external tools
        PART_Viewer.PhotoLoading -= Core.Viewer_PhotoLoadingForPlugins;
        PART_Viewer.ViewerPointerMoved -= Core.Viewer_PointerMovedForPlugins;
        PART_Viewer.ViewerPointerPressed -= Core.Viewer_PointerPressedForPlugins;
        PART_Viewer.SelectionChanged -= Core.Viewer_SelectionChangedForPlugins;
        PART_Viewer.PhotoFrameChanged -= Core.Viewer_FrameChangedForPlugins;
    }


    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        UpdateGalleryWidth();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        UpdateMotionButtonTooltip();
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Config.Layout))
        {
            ApplyAppLayout();
        }
        else if (e.PropertyName == nameof(Config.EnableFileWatcher))
        {
            // set file watcher
            AppAPIProvider.SetFileWatcher(Core.Config.EnableFileWatcher);
        }
        else if (e.PropertyName is nameof(Config.BackgroundColor)
            or nameof(Config.SlideshowBackgroundColor)
            or nameof(Config.EnableSlideshow))
        {
            Core.UpdateViewerBackgroundBrushResource();
        }
    }


    private void PART_Viewer_DragOver(object? sender, DragEventArgs e)
    {
        // if drag data contains files or folders
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Link;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }


    private async void PART_Viewer_Drop(object? sender, DragEventArgs e)
    {
        // if drag data contains files or folders
        if (e.DataTransfer.TryGetFiles() is not IStorageItem[] sItems) return;

        // 1. get dropped paths
        var paths = sItems.Select(i => i.Path.LocalPath).ToArray();


        // 2. load multiple paths
        if (paths.Length > 1)
        {
            PrepareLoadPhotoList(paths,
                currentFilePath: null, disposeForegroundShell: true, reloadInitPhoto: true);
            return;
        }


        // 3. load single file path
        // 3.1 get foreground shell; always captured, since a search window is the only source of its own result list
        Core.ShellProvider?.ForegroundShell = Core.ShellProvider?.GetForegroundWindowView();
        Core.UpdateInitImagePath(paths[0]);

        // 3.2 open the path
        _ = await Core.API.RunApiAsync(API.IG_OpenPath, paths[0]);
    }


    private void Photos_FileWatcherChanged(PhotoManager sender, FileWatcherChangedEventArgs e)
    {
        if (e.ChangeType == ChangeType.LOG) return;

        Dispatcher.UIThread.Post(() =>
        {
            switch (e.ChangeType)
            {
                case ChangeType.CREATED:
                    HandleFileWatcher_FilesAdded(e);
                    break;

                case ChangeType.DELETED:
                    HandleFileWatcher_FilesDeleted(e);
                    break;

                case ChangeType.CHANGED:
                    HandleFileWatcher_FilesChanged(e);
                    break;

                case ChangeType.RENAMED:
                    HandleFileWatcher_FilesRenamed(e);
                    break;
            }
        });
    }


    private void HandleFileWatcher_FilesAdded(FileWatcherChangedEventArgs e)
    {
        // gallery is data-bound to Core.Photos.Items, so items are already added;
        // we only need to scroll/display the last added file if the user opted in.
        if (e.FilePaths.Count == 0) return;

        // scroll gallery to the newly added file if configured
        if (Core.Config.EnableAutoOpenNewAddedImage)
        {
            var lastAdded = e.FilePaths[^1];
            var newIndex = Core.Photos.IndexOf(lastAdded);

            if (newIndex >= 0)
            {
                var photo = Core.Photos.Select(newIndex);
                _ = ViewPhotoAsync(photo);
            }
        }
        else
        {
            // re-select current photo to update its index (list shifted by inserts)
            _ = Core.Photos.Select(Core.Photos.CurrentFilePath);
            PART_Gallery.ScrollToItem(Core.Photos.CurrentIndex);
        }
    }


    private void HandleFileWatcher_FilesDeleted(FileWatcherChangedEventArgs e)
    {
        if (e.FilePaths.Count == 0) return;

        // if the currently viewed photo was deleted
        if (!string.IsNullOrEmpty(e.AffectedCurrentFilePath))
        {
            AppAPIProvider.ViewPhotoAfterCurrentRemoved();
        }
        else
        {
            // re-select current photo to fix its index after list shifted
            _ = Core.Photos.Select(Core.Photos.CurrentFilePath);
            PART_Gallery.ScrollToItem(Core.Photos.CurrentIndex);
        }

        // update init photo path if it was deleted
        if (Core.Photos.InitPhoto is not null)
        {
            for (var i = 0; i < e.FilePaths.Count; i++)
            {
                if (string.Equals(e.FilePaths[i], Core.Photos.InitPhoto.FilePath, StringComparison.OrdinalIgnoreCase))
                {
                    var dirPath = Path.GetDirectoryName(e.FilePaths[i]) ?? string.Empty;
                    Core.Photos.InitPhoto = !string.IsNullOrEmpty(dirPath)
                        ? new Photo(dirPath)
                        : null;
                    break;
                }
            }
        }
    }


    private void HandleFileWatcher_FilesChanged(FileWatcherChangedEventArgs e)
    {
        if (e.FilePaths.Count == 0) return;

        for (var i = 0; i < e.FilePaths.Count; i++)
        {
            var filePath = e.FilePaths[i];
            var photoIndex = Core.Photos.IndexOf(filePath);
            var photo = Core.Photos.Get(photoIndex);
            if (photo is null) continue;

            // force thumbnail reload
            PART_Gallery.LoadThumbnail(photoIndex, false);

            // if it's the currently viewed photo, reload it
            if (Core.Photos.IsSelected(filePath))
            {
                _ = PART_Viewer.SetPhotoAsync(photo, new PhotoLoadingOptions
                {
                    ResetZoom = false,
                    UseCache = false,
                    Channels = Core.ColorChannels,
                });
            }
        }
    }


    private static void HandleFileWatcher_FilesRenamed(FileWatcherChangedEventArgs e)
    {
        if (e.FilePaths.Count == 0 || e.OldFilePaths is null) return;

        var currentNeedsRefresh = false;
        for (var i = 0; i < e.FilePaths.Count; i++)
        {
            var newPath = e.FilePaths[i];
            var oldPath = i < e.OldFilePaths.Count ? e.OldFilePaths[i] : null;

            // update init photo if it was renamed
            if (Core.Photos.InitPhoto is not null
                && oldPath is not null
                && string.Equals(oldPath, Core.Photos.InitPhoto.FilePath, StringComparison.OrdinalIgnoreCase))
            {
                Core.Photos.InitPhoto = new Photo(newPath);
            }

            // check if the currently viewed photo was renamed
            if (Core.Photos.IsSelected(newPath))
            {
                currentNeedsRefresh = true;
            }
        }

        // refresh the viewer title/info if the current photo was renamed
        if (currentNeedsRefresh)
        {
            // re-select to refresh bindings for current photo name/path
            _ = Core.Photos.Select(Core.Photos.CurrentIndex);
        }
    }


    private void PART_Viewer_PhotoLoading(ViewerControl sender, PhotoLoadingEventArgs e)
    {
        // Note: events are not fired in order

        // 1. handle loading error first
        if (e.Photo.Error is not null)
        {
            var heading = $"{Core.Lang[LangId._PicMain_ErrorText]} 😶";
            var err = BHelper.GetInAppError(e.Photo.Error);

            // show error message
            _ = PART_Message.ShowAsync(err.DebugInfo, heading, err.Details, 0);
        }

        // 2. handle photo loading
        else if (e.State == PhotoState.Preview)
        {
            // show loading message after 2s
            _ = PART_Message.ShowAsync(Core.Lang[LangId._Loading], durationMs: 0, delayMs: 2000);
        }

        // 3. handle photo loaded
        else if (e.State == PhotoState.Loaded)
        {
            // clear in-app message
            _ = PART_Message.ShowAsync(null);
        }

        UpdateMotionButtonState();
    }


    private void PART_ToolHost_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ToolHostControl.PluginContentProperty)
        {
            UpdateMotionButtonState();
        }
    }


    private async void PART_BtnMotionVideo_Click(object? sender, RoutedEventArgs e)
    {
        _ = await Core.API.RunApiAsync(API.IG_ToggleImageAnimation);
    }


    private void PART_Viewer_ZoomChanged(ViewerControl sender, ViewerZoomEventArgs e)
    {
        // update window fit
        if (Core.Config.EnableWindowFit
            && e.ChangeSource != ZoomChangeSource.SizeChanged
            && e.ChangeSource != ZoomChangeSource.FrameChanged
            && (e.IsManualZoom || e.IsZoomModeChange))
        {
            AppAPIProvider.ApplyWindowFitMode(e.ChangeSource == ZoomChangeSource.ZoomMode);
        }
    }


    private async void PART_Viewer_NavButtonClicked(ViewerControl sender, NavButtonClickedEventArgs e)
    {
        if (e.Direction == NavButtonDirection.Right)
        {
            _ = await Core.API.RunApiAsync(API.IG_ViewNext);
        }
        else
        {
            _ = await Core.API.RunApiAsync(API.IG_ViewPrevious);
        }
    }


    private async void PART_Viewer_ViewerPointerClicked(ViewerControl sender, ViewerPointerClickEventArgs e)
    {
        // get pointer click action from user settings
        var action = Core.Config.MouseClickActions.GetValueOrDefault(e.ClickEvent);

        // fallback to the default action
        if (action is null)
        {
            action = Config.DefaultMouseClickActions.GetValueOrDefault(e.ClickEvent);
        }

        await Core.API!.RunActionAsync(action);
    }


    private async void PART_Viewer_ViewerMouseWheel(ViewerControl sender, ViewerMouseWheelEventArgs e)
    {
        // get mouse wheel action from user settings
        if (!Core.Config.MouseWheelActions.TryGetValue(e.WheelEvent, out var wheelAction))
        {
            // fallback to the default action
            _ = Config.DefaultMouseWheelActions.TryGetValue(e.WheelEvent, out wheelAction);
        }

        switch (wheelAction)
        {
            case MouseWheelAction.Zoom:
                // respect a zoom feature lock (this path skips the RunApiAsync lock gate)
                if (!FeatureManager.IsZoomLocked())
                    _ = PART_Viewer.ZoomByDeltaToPoint(e.Delta, e.Position);
                break;

            case MouseWheelAction.PanVertically:
                if (!FeatureManager.IsPanLocked())
                    PART_Viewer.PanTo(0, -e.Delta, e.Position);
                break;

            case MouseWheelAction.PanHorizontally:
                if (!FeatureManager.IsPanLocked())
                    PART_Viewer.PanTo(-e.Delta, 0, e.Position);
                break;

            case MouseWheelAction.BrowseImages:
                if (e.Delta > 0) _ = await Core.API.RunApiAsync(API.IG_ViewPrevious);
                else _ = await Core.API.RunApiAsync(API.IG_ViewNext);
                break;

            case MouseWheelAction.DoNothing:
            default:
                break;
        }
    }


    private void PART_GalleryResizer_DragCompleted(object? sender, VectorEventArgs e)
    {
        var panelEl = PART_Gallery.FindVirtualPanel();
        if (panelEl is null) return;

        // save number of columns
        Core.Config.GalleryColumns = (uint)panelEl.ColumnsPerRow;
    }


    private void PART_Viewer_ContextMenu_Opened(object? sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu mnuContext) return;
        mnuContext.Items.Clear();

        var hasClipboardImage = Core.ClipboardImage != null;
        var imageNotFound = !File.Exists(Core.Photos.CurrentFilePath);


        #region Menu group: Slideshow
        if (Core.Slideshow?.IsRunning == true)
        {
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId._MnuPauseResumeSlideshow,
                Command = Core.API.GetApiCommand(API.IG_ToggleSlideshowPlayback),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId._MnuPauseResumeSlideshow),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId._MnuExitSlideshow,
                Command = Core.API.GetApiCommand(API.IG_ToggleSlideshow),
                CommandParameter = false,
                HotkeyText = string.Join(", ", [
                    new Hotkey(Key.Escape),
                    AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuSlideshow)
                ]),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                ToggleType = MenuItemToggleType.CheckBox,
                IsChecked = Core.Config.EnableSlideshowCountdown,
                LangKey = LangId._MnuToggleCountdown,
                Command = Core.API.GetApiCommand(API.IG_ToggleSlideshowCountdown),
                CommandParameter = !Core.Config.EnableSlideshowCountdown,
            });
            mnuContext.Items.Add("-"); //------------
        }
        #endregion // Menu group: Slideshow


        #region Menu group: Layout
        // menu toolbar
        mnuContext.Items.Add(new PhMenuItem
        {
            LangKey = LangId.Menu_MnuToggleToolbar,
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = Core.Config.ShowToolbar,
            Command = Core.API.GetApiCommand(API.IG_ToggleToolbar),
            HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuToggleToolbar),
        });
        // menu Top most
        mnuContext.Items.Add(new PhMenuItem
        {
            LangKey = LangId.Menu_MnuToggleTopMost,
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = Core.Config.EnableWindowTopMost,
            Command = Core.API.GetApiCommand(API.IG_ToggleWindowTopMost),
            HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuToggleTopMost),
        });
        #endregion // Menu group: Layout


        #region Menu group: Loading orders
        if (!imageNotFound) mnuContext.Items.Add("-");
        if (!imageNotFound && !hasClipboardImage)
        {
            var mnuLoadingOrders = new PhMenuItem
            {
                LangKey = LangId.Menu_MnuLoadingOrders,
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuLoadingOrders),
            };
            mnuContext.Items.Add(mnuLoadingOrders);


            // use Explorer sort order
            if (Core.ShellProvider is not null && BHelper.OS == OSType.Windows)
            {
                mnuLoadingOrders.Items.Add(new PhMenuItem
                {
                    LangKey = LangId.Settings_EnableExplorerSortOrder,
                    ToggleType = MenuItemToggleType.CheckBox,
                    IsChecked = Core.Config.EnableExplorerSortOrder,
                    Command = Core.API.GetApiCommand(API.IG_ToggleExplorerSortOrder),
                    HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Settings_EnableExplorerSortOrder),
                });
                mnuLoadingOrders.Items.Add("-");
            }


            // order by
            foreach (var orderBy in Enum.GetNames<ImageOrderBy>())
            {
                mnuLoadingOrders.Items.Add(new PhMenuItem
                {
                    LangKey = Lang.GetKey($"{nameof(ImageOrderBy)}_{orderBy}"),
                    ToggleType = MenuItemToggleType.Radio,
                    IsChecked = Core.Config.ImageLoadingOrder == Enum.Parse<ImageOrderBy>(orderBy),
                    Command = Core.API.GetApiCommand(API.IG_SetLoadingOrderBy),
                    CommandParameter = orderBy,
                });
            }

            // order type
            mnuLoadingOrders.Items.Add("-");
            foreach (var orderType in Enum.GetNames<ImageOrderType>())
            {
                mnuLoadingOrders.Items.Add(new PhMenuItem
                {
                    LangKey = Lang.GetKey($"{nameof(ImageOrderType)}_{orderType}"),
                    ToggleType = MenuItemToggleType.Radio,
                    IsChecked = Core.Config.ImageLoadingOrderType == Enum.Parse<ImageOrderType>(orderType),
                    Command = Core.API.GetApiCommand(API.IG_SetLoadingOrderType),
                    CommandParameter = orderType,
                });
            }
        }
        #endregion // Menu group: Loading orders


        #region Menu group: View channels
        if (!PART_Viewer.IsImageAnimating && (!imageNotFound || hasClipboardImage))
        {
            var mnuChannels = new PhMenuItem
            {
                LangKey = LangId.Menu_MnuViewChannels,
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuViewChannels),
            };
            mnuContext.Items.Add(mnuChannels);

            foreach (var item in PART_Toolbar.PART_MnuViewChannels.Items)
            {
                if (item is not PhMenuItem oriItem) continue;

                var mnuItem = new PhMenuItem
                {
                    Header = oriItem.Header,
                    ToggleType = oriItem.ToggleType,
                    HotkeyText = oriItem.HotkeyText,
                    CommandParameter = oriItem.CommandParameter,
                };
                if (Enum.TryParse<ColorChannels>((string)oriItem.CommandParameter!, true, out var val))
                {
                    mnuItem.IsChecked = Core.ColorChannels.HasFlag(val);
                }
                mnuItem.Click += PART_Toolbar.MainMenu_ViewChannelItem_Click;
                mnuChannels.Items.Add(mnuItem);
            }
        }
        #endregion // Menu group: View channels


        #region Menu group: Edit
        if (!imageNotFound) mnuContext.Items.Add("-");
        if (!imageNotFound || hasClipboardImage)
        {
            var mnuEdit = new PhMenuItem
            {
                LangKey = LangId.Menu_MnuEdit,
                Command = Core.API.GetApiCommand(API.IG_OpenEditingApp),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuEdit),
            };

            EditingApp.UpdateAppNameForMenuEdit(mnuEdit);
            mnuContext.Items.Add(mnuEdit);
        }
        #endregion // Menu group: Edit


        #region Menu group: Desktop wallpaper, lock screen
        if ((!imageNotFound && !Core.Photos.IsCurrentError) || hasClipboardImage)
        {
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuSetDesktopBackground,
                Command = Core.API.GetApiCommand(API.IG_SetDesktopBackground),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuSetDesktopBackground),
            });

            if (BHelper.OS == OSType.Windows)
            {
                mnuContext.Items.Add(new PhMenuItem
                {
                    LangKey = LangId.Menu_MnuSetLockScreen,
                    Command = Core.API.GetApiCommand(API.IG_SetLockScreenImage),
                    HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuSetLockScreen),
                });
            }
        }
        #endregion // Menu group: Desktop wallpaper, lock screen


        #region Menu group: Clipboard
        mnuContext.Items.Add(new PhMenuItem("-")); //------------

        mnuContext.Items.Add(new PhMenuItem
        {
            LangKey = LangId.Menu_MnuPasteImage,
            Command = Core.API.GetApiCommand(API.IG_PasteImage),
            HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuPasteImage),
        });
        mnuContext.Items.Add(new PhMenuItem
        {
            LangKey = LangId.Menu_MnuCopyImagePixels,
            Command = Core.API.GetApiCommand(API.IG_CopyImagePixels),
            HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuCopyImagePixels),
        });

        if (!imageNotFound && !hasClipboardImage)
        {
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuCopyPath,
                Command = Core.API.GetApiCommand(API.IG_CopyImagePath),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuCopyPath),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuCopyFile,
                Command = Core.API.GetApiCommand(API.IG_CopyFiles),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuCopyFile),
            });
            if (BHelper.OS != OSType.Mac) {
                mnuContext.Items.Add(new PhMenuItem
                {
                    LangKey = LangId.Menu_MnuCutFile,
                    Command = Core.API.GetApiCommand(API.IG_CutFiles),
                    HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuCutFile),
                });
            }
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuClearClipboard,
                Command = Core.API.GetApiCommand(API.IG_ClearClipboard),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuClearClipboard),
            });
        }
        #endregion // Menu group: Clipboard


        #region Menu group: File Operation
        if (!imageNotFound && !hasClipboardImage)
        {
            mnuContext.Items.Add(new PhMenuItem("-")); //------------
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuRename,
                Command = Core.API.GetApiCommand(API.IG_Rename),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuRename),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuMoveToRecycleBin,
                Command = Core.API.GetApiCommand(API.IG_Delete),
                CommandParameter = "true",
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuMoveToRecycleBin),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuOpenLocation,
                Command = Core.API.GetApiCommand(API.IG_OpenLocation),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuOpenLocation),
            });
            mnuContext.Items.Add(new PhMenuItem
            {
                LangKey = LangId.Menu_MnuImageProperties,
                Command = Core.API.GetApiCommand(API.IG_OpenProperties),
                HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuImageProperties),
            });
        }
        #endregion // Menu group: File Operation


        // menu Exit
        mnuContext.Items.Add("-"); //------------
        mnuContext.Items.Add(new PhMenuItem
        {
            LangKey = LangId.Menu_MnuExit,
            Command = Core.API.GetApiCommand(API.IG_Exit),
            HotkeyText = AppAPIProvider.GetMenuHotkeyText(LangId.Menu_MnuExit),
        });


        // Hide locked menu items
        FeatureManager.HideLockedMenuItems(mnuContext.Items);
    }


    #endregion // Control Events



    #region Control Methods

    private async void LoadImagesFromCmdArgs()
    {
        var pathToLoad = Core.InputImagePathFromArgs;

        // check for last seen image
        if (string.IsNullOrEmpty(pathToLoad)
            && Core.Config.EnableLastSeenImage
            && BHelper.CheckPath(Core.Config.LastSeenImagePath) == PathType.File)
        {
            pathToLoad = Core.Config.LastSeenImagePath;
        }

        // check for Welcome image
        if (string.IsNullOrEmpty(pathToLoad))
        {
            if (Core.Config.EnableWelcomeImage)
            {
                pathToLoad = BHelper.BaseDir("default.webp");
            }
            else
            {
                return;
            }
        }

        // a non-built-in type may need a codec plugin still loading; wait for discovery
        var ext = Path.GetExtension(pathToLoad);
        if (!string.IsNullOrEmpty(ext)
            && !Config.DefaultFileFormats.Contains(ext, StringComparer.OrdinalIgnoreCase))
        {
            StartupTrace.Mark("plugins:await:begin");
            try { await Core.PluginDiscoveryTask; } catch { }
            StartupTrace.Mark("plugins:await:end");
            StartupTrace.Flush();
        }

        // start loading path with the foreground shell
        PrepareLoadPhotoList([pathToLoad],
            currentFilePath: null, disposeForegroundShell: false, reloadInitPhoto: true);
    }


    /// <param name="selectLastPhoto">
    /// Lands on the last photo of the loaded list instead of the first. The list order is only
    /// known after the search runs (it may come from Explorer), so the choice can't be a path.
    /// </param>
    public void PrepareLoadPhotoList(ICollection<string> inputPaths, string? currentFilePath, bool disposeForegroundShell, bool reloadInitPhoto, bool selectLastPhoto = false)
    {
        _selectLastOnSearch = selectLastPhoto;

        // dispose the foreground shell if requested
        if (disposeForegroundShell) Core.ShellProvider?.ForegroundShell = null;


        // check if we should load images from foreground window
        var useForegroundWindow = Core.ShellProvider?.CanUseForegroundShell() ?? false;
        var foregroundShell = useForegroundWindow
            ? Core.ShellProvider?.ForegroundShell
            : null;


        Dispatcher.UIThread.Post(async () =>
        {
            // resolve first: a shortcut must contribute its target's type, never ".lnk"
            var resolvedPaths = inputPaths.Select(BHelper.ResolvePath).ToList();
            var resolvedCurrentPath = BHelper.ResolvePath(currentFilePath);

            // include the opened file's own type so it's listed even before its plugin loads
            var allowedExts = Core.GetSupportedFileExtensions();
            AddFileExtension(allowedExts, resolvedCurrentPath);
            foreach (var p in resolvedPaths) AddFileExtension(allowedExts, p);

            // start loading files
            var searchOptions = new FileSearchOptions()
            {
                AllowedExtensions = allowedExts,
                UseExplorerSortOrder = Core.Config.EnableExplorerSortOrder,
                ForegroundShell = foregroundShell,
                SearchSubDirectories = Core.Config.EnableSubfoldersLoading,
                GroupByDir = Core.Config.EnableImageFolderGrouping,
                IncludeHidden = Core.Config.EnableHiddenImagesLoading,
                OrderBy = Core.Config.ImageLoadingOrder,
                OrderType = Core.Config.ImageLoadingOrderType,
            };
            var initPhoto = Core.Photos.StartLoadingFiles(resolvedPaths, resolvedCurrentPath, searchOptions, Files_Searched, reloadInitPhoto);


            if (reloadInitPhoto && initPhoto is not null)
            {
                _ = ViewPhotoAsync(initPhoto);
            }
        });
    }


    /// <summary>
    /// Adds a file path's extension to the allowed-extensions set (no-op for a directory or empty path).
    /// </summary>
    private static void AddFileExtension(HashSet<string> exts, string? path)
    {
        if (string.IsNullOrEmpty(path)) return;

        var ext = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(ext)) exts.Add(ext);
    }


    private void Files_Searched(FileSearchingEventArgs e)
    {
        // The Win32 provider posts progress to the UI thread, the base one (Linux/macOS) calls
        // straight from its worker, so the whole body must marshal: Items is bound to the gallery.
        Dispatcher.UIThread.Invoke(() =>
        {
            try
            {
                Files_Searched__(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {nameof(Files_Searched)}: {ex.Message}");
            }
        });
    }


    private void Files_Searched__(FileSearchingEventArgs e)
    {
        var isEmptyList = Core.Photos.Count == 0;

        // add files to list
        Core.Photos.Add(e.Results);


        // display the last file in a folder; re-applied on every batch, so it settles on the
        // real last item once the search is done
        if (_selectLastOnSearch)
        {
            var lastIndex = (int)Core.Photos.Count - 1;
            Core.Photos.InitPhoto = Core.Photos.Select(lastIndex);
            _ = ViewPhotoAsync(Core.Photos.InitPhoto, true, false);
        }
        // the init photo owns the selection for every batch, incl. the later sub-dir ones
        else if (Core.Photos.InitPhoto is not null)
        {
            // if we haven't found current index for the init photo yet
            if (Core.Photos.CurrentIndex == -1)
            {
                // find index of the init photo and select it
                _ = Core.Photos.Select(Core.Photos.InitPhoto.FilePath);

                // save the init photo to the list
                var initIndex = Core.Photos.CurrentIndex;
                if (initIndex >= 0 && initIndex < Core.Photos.Items.Count)
                {
                    Core.Photos.Items[initIndex]?.Dispose();
                    Core.Photos.Items[initIndex] = Core.Photos.InitPhoto;
                    Core.Photos.Items[initIndex].IsCurrent = true;
                }
            }
        }
        // display the first file in a folder
        else
        {
            Core.Photos.InitPhoto = Core.Photos.Select(0);
            _ = ViewPhotoAsync(Core.Photos.InitPhoto, true, false);
        }


        // Gallery: scroll to the selected item
        if (isEmptyList)
        {
            // set file watcher
            AppAPIProvider.SetFileWatcher(Core.Config.EnableFileWatcher);

            Dispatcher.UIThread.Post(() =>
            {
                // set photo to the viewer
                PART_Gallery.ScrollToItem(Core.Photos.CurrentIndex);
            });
        }
    }


    public async Task ViewPhotoAsync(Photo? photo, bool useCache = true, bool scrollToThumbnail = true, bool resetZoom = true)
    {
        // Sequential mode: close the navigation gate here, not inside the post below,
        // so the next key repeat already sees a load in flight
        var isSequential = Core.Config.BrowsingMode == BrowsingMode.Sequential;
        if (isSequential) Interlocked.Increment(ref _sequentialLoadDepth);

        // clear the current in-app message
        _ = PART_Message.ClearAsync();

        Core.DisposeClipboardPhoto();
        Core.ImageTransform.Clear();

        // set read options for photo
        if (photo is not null)
        {
            photo.ReadOptions = PhotoReadOptions.FromConfig();
        }

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                // apply user settings to the viewer
                PART_Viewer.EnableImagePreview = Core.Config.EnableImagePreview;

                if (scrollToThumbnail)
                {
                    // set photo to the viewer
                    PART_Gallery.ScrollToItem(Core.Photos.CurrentIndex);
                }

                await PART_Viewer.SetPhotoAsync(photo, new PhotoLoadingOptions
                {
                    UseCache = useCache,
                    ResetZoom = resetZoom,
                    Channels = Core.ColorChannels,
                });

                // reopening the gate at decode time is too early: the next navigation would
                // dispose the image before it is ever painted
                if (isSequential)
                {
                    await PART_Viewer.WaitForPhotoRenderedAsync(_sequentialRenderTimeout);
                }
            }
            finally
            {
                if (isSequential) Interlocked.Decrement(ref _sequentialLoadDepth);
            }

            // trigger background caching of adjacent photos
            // after the current photo finishes loading
            Core.Photos.RequestCacheAround(Core.Photos.CurrentIndex);
        });
    }



    /// <summary>
    /// Updates app layout.
    /// </summary>
    public void ApplyAppLayout()
    {
        // 1. read control's layouts from setting
        var toolbarPos = Config.GetControlLayout(LayoutControl.Toolbar);
        var galleryPos = Config.GetControlLayout(LayoutControl.Gallery);


        // 2. create layout
        if (toolbarPos == LayoutPosition.Top)
        {
            PART_Toolbar.ItemTooltipPlacement = PlacementMode.Bottom;

            if (galleryPos == LayoutPosition.Top)
            {
                PART_Layout.RowDefinitions = new("Auto, Auto, *");
                PART_Layout.ColumnDefinitions = new("*");

                PART_GalleryResizer.IsVisible = false;
                PART_Gallery.MaxWidth = double.PositiveInfinity;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.FilmStrip;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Bottom;
                Grid.SetColumnSpan(PART_Toolbar, 1);

                Grid.SetRow(PART_Toolbar, 0);
                Grid.SetRow(PART_Gallery, 1);
                Grid.SetRow(PART_ViewerWrapper, 2);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
            else if (galleryPos == LayoutPosition.Bottom)
            {
                PART_Layout.RowDefinitions = new("Auto, *, Auto");
                PART_Layout.ColumnDefinitions = new("*");

                PART_GalleryResizer.IsVisible = false;
                PART_Gallery.MaxWidth = double.PositiveInfinity;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.FilmStrip;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Top;
                Grid.SetColumnSpan(PART_Toolbar, 1);

                Grid.SetRow(PART_Toolbar, 0);
                Grid.SetRow(PART_Gallery, 2);
                Grid.SetRow(PART_GalleryResizer, 0);
                Grid.SetRow(PART_ViewerWrapper, 1);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_GalleryResizer, 0);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
            else if (galleryPos == LayoutPosition.Left)
            {
                PART_Layout.RowDefinitions = new("Auto, *");
                PART_Layout.ColumnDefinitions = new("Auto, Auto, *");

                PART_GalleryResizer.IsVisible = true;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.Gallery;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Pointer;
                Grid.SetColumnSpan(PART_Toolbar, 3);

                Grid.SetRow(PART_Toolbar, 0);
                Grid.SetRow(PART_Gallery, 1);
                Grid.SetRow(PART_GalleryResizer, 1);
                Grid.SetRow(PART_ViewerWrapper, 1);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_GalleryResizer, 1);
                Grid.SetColumn(PART_ViewerWrapper, 2);
            }
            else if (galleryPos == LayoutPosition.Right)
            {
                PART_Layout.RowDefinitions = new("Auto, *");
                PART_Layout.ColumnDefinitions = new("*, Auto, Auto");

                PART_GalleryResizer.IsVisible = true;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.Gallery;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Pointer;
                Grid.SetColumnSpan(PART_Toolbar, 3);

                Grid.SetRow(PART_Toolbar, 0);
                Grid.SetRow(PART_Gallery, 1);
                Grid.SetRow(PART_GalleryResizer, 1);
                Grid.SetRow(PART_ViewerWrapper, 1);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 2);
                Grid.SetColumn(PART_GalleryResizer, 1);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
        }
        else if (toolbarPos == LayoutPosition.Bottom)
        {
            PART_Toolbar.ItemTooltipPlacement = PlacementMode.Top;

            if (galleryPos == LayoutPosition.Top)
            {
                PART_Layout.RowDefinitions = new("Auto, *, Auto");
                PART_Layout.ColumnDefinitions = new("*");

                PART_GalleryResizer.IsVisible = false;
                PART_Gallery.MaxWidth = double.PositiveInfinity;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.FilmStrip;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Bottom;
                Grid.SetColumnSpan(PART_Toolbar, 1);

                Grid.SetRow(PART_Toolbar, 2);
                Grid.SetRow(PART_Gallery, 0);
                Grid.SetRow(PART_GalleryResizer, 0);
                Grid.SetRow(PART_ViewerWrapper, 1);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_GalleryResizer, 0);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
            else if (galleryPos == LayoutPosition.Bottom)
            {
                PART_Layout.RowDefinitions = new("*, Auto, Auto");
                PART_Layout.ColumnDefinitions = new("*");

                PART_GalleryResizer.IsVisible = false;
                PART_Gallery.MaxWidth = double.PositiveInfinity;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.FilmStrip;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Top;
                Grid.SetColumnSpan(PART_Toolbar, 1);

                Grid.SetRow(PART_Toolbar, 2);
                Grid.SetRow(PART_Gallery, 1);
                Grid.SetRow(PART_GalleryResizer, 0);
                Grid.SetRow(PART_ViewerWrapper, 0);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_GalleryResizer, 0);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
            else if (galleryPos == LayoutPosition.Left)
            {
                PART_Layout.RowDefinitions = new("*, Auto");
                PART_Layout.ColumnDefinitions = new("Auto, Auto, *");

                PART_GalleryResizer.IsVisible = true;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.Gallery;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Pointer;
                Grid.SetColumnSpan(PART_Toolbar, 3);

                Grid.SetRow(PART_Toolbar, 1);
                Grid.SetRow(PART_Gallery, 0);
                Grid.SetRow(PART_GalleryResizer, 0);
                Grid.SetRow(PART_ViewerWrapper, 0);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 0);
                Grid.SetColumn(PART_GalleryResizer, 1);
                Grid.SetColumn(PART_ViewerWrapper, 2);
            }
            else if (galleryPos == LayoutPosition.Right)
            {
                PART_Layout.RowDefinitions = new("*, Auto");
                PART_Layout.ColumnDefinitions = new("*, Auto, Auto");

                PART_GalleryResizer.IsVisible = true;
                PART_Gallery.ViewMode = PhVirtualizingUniformPanelViewMode.Gallery;
                PART_Gallery.ItemTooltipPlacement = PlacementMode.Pointer;
                Grid.SetColumnSpan(PART_Toolbar, 3);

                Grid.SetRow(PART_Toolbar, 1);
                Grid.SetRow(PART_Gallery, 0);
                Grid.SetRow(PART_GalleryResizer, 0);
                Grid.SetRow(PART_ViewerWrapper, 0);
                Grid.SetColumn(PART_Toolbar, 0);
                Grid.SetColumn(PART_Gallery, 2);
                Grid.SetColumn(PART_GalleryResizer, 1);
                Grid.SetColumn(PART_ViewerWrapper, 0);
            }
        }


        // 3. update gallery initial width
        UpdateGalleryWidth();
    }


    /// <summary>
    /// Updates the width of the gallery and its resizer columns based on the current layout.
    /// </summary>
    private void UpdateGalleryWidth()
    {
        if (PART_Layout.ColumnDefinitions.Count == 0) return;

        // 1. get gallery position
        var galleryPos = Core.Config.Layout.GetValueOrDefault(LayoutControl.Gallery, LayoutPosition.Bottom);
        if (galleryPos is LayoutPosition.Top or LayoutPosition.Bottom) return;

        // 2. get column indexes
        var galleryColIndex = Grid.GetColumn(PART_Gallery);
        var galleryResizerColIndex = Grid.GetColumn(PART_GalleryResizer);


        // 3. hide gallery space
        if (!Core.Config.ShowGallery)
        {
            PART_Layout.ColumnDefinitions[galleryColIndex].Width = new(0);
            PART_Layout.ColumnDefinitions[galleryResizerColIndex].Width = new(0);
            PART_GalleryResizer.IsVisible = false;
        }

        // 4. update gallery size
        else
        {
            var galleryViewMinWidth = GalleryControl.CalculateWidthForGalleryView(1);
            var galleryViewWidth = GalleryControl.CalculateWidthForGalleryView(Core.Config.GalleryColumns);
            galleryViewWidth = Math.Min(galleryViewWidth, Bounds.Width * 0.8); // max width = 80% window width

            PART_Layout.ColumnDefinitions[galleryColIndex].Width = new(galleryViewWidth);
            PART_Layout.ColumnDefinitions[galleryColIndex].MinWidth = galleryViewMinWidth;
            PART_Layout.ColumnDefinitions[galleryResizerColIndex].Width = new(5);
            PART_GalleryResizer.IsVisible = true;
        }
    }


    /// <summary>
    /// Shows the motion-video button for a live photo while the Frame nav tool is closed.
    /// </summary>
    private void UpdateMotionButtonState()
    {
        var photo = PART_Viewer.Photo;
        var isLivePhoto = photo?.Error is null && (photo?.Metadata?.IsLivePhoto ?? false);
        var isFrameNavOpen = PART_ToolHost.Tool?.ToolId == FrameNavToolControl.TOOL_ID;

        PART_MotionButtonHost.IsVisible = isLivePhoto && !isFrameNavOpen;
    }


    /// <summary>
    /// Updates the motion-video button tooltip.
    /// </summary>
    private void UpdateMotionButtonTooltip()
    {
        ToolTip.SetTip(PART_BtnMotionVideo, AppAPIProvider.GetMenuTooltipText(
            LangId._PlayMotionVideo, LangId.Menu_MnuToggleImageAnimation));
    }


    #endregion // Control Methods


}