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
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using ImageGlass.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;


public partial class MainWindow : PhWindow
{
    private readonly AppStatusInfo _status;
    private bool _isClosingHandled; // to handle closing for saving configs

    private const double DEFAULT_WIDTH = 800;
    private const double DEFAULT_HEIGHT = 500;
    private const double MIN_RESTORE_WIDTH = 200;
    private const double MIN_RESTORE_HEIGHT = 100;

    public MainWindowModel VM => (MainWindowModel)DataContext!;


    public MainWindow()
    {
        StartupTrace.Mark("MainWindow:InitComponent:begin");
        InitializeComponent();
        StartupTrace.Mark("MainWindow:InitComponent:end");
        _status = new AppStatusInfo(PART_MainView.PART_Viewer);


        // load window size & position
        RestoreWindowBounds(Core.Config.MainWindowBounds,
            new(DEFAULT_WIDTH, DEFAULT_HEIGHT),
            new(MIN_RESTORE_WIDTH, MIN_RESTORE_HEIGHT));

        if (!Core.Config.EnableWindowFit)
        {
            // load window state
            if (Core.Config.EnableMainWindowMaximized) WindowState = WindowState.Maximized;
        }

        // set zoom lock
        if (Core.Config.ZoomMode == UI.Viewer.ZoomMode.LockZoom)
        {
            PART_MainView.PART_Viewer.ZoomFactor = Core.Config.ZoomLockValue / 100f;
        }

        // events
        Core.AppInstance.InstanceInvoked += AppInstance_InstanceInvoked;
    }



    #region Override Methods


    protected override async void OnOpened(EventArgs e)
    {
        StartupTrace.Mark("MainWindow:opened");
        base.OnOpened(e);

        // load full screen; it wins over window fit, which the backup brings back on leaving it
        if (Core.Config.EnableFullScreen)
        {
            // through the API, so the saved windowed layout is backed up as the state to return to
            _ = await Core.API.RunApiAsync(API.IG_ToggleFullScreen, "true");
        }
        else if (Core.Config.EnableWindowFit)
        {
            // load Window fit
            _ = await Core.API.RunApiAsync(API.IG_ToggleWindowFit, "true");
        }

        // load color profile
        Core.UpdateDestColorProfile();

        // restore last opened tool
        _ = await Core.API.RunApiAsync(API.IG_OpenTool, Core.Config.LastOpenedTool);
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // register app hotkeys
        StartupTrace.Mark("Hotkeys:register:begin");
        Core.API.RegisterHotkeys();
        StartupTrace.Mark("Hotkeys:register:end");

        // build the macOS menu bar (window-level) from the existing main menu;
        // the application (⌘) menu is defined in App.axaml
        if (OperatingSystem.IsMacOS())
        {
            NativeMenu.SetMenu(this, PART_MainView.PART_Toolbar.BuildNativeWindowMenu());
        }

        // control events
        _status.Changed += Status_Changed;
        PART_MainView.PART_Toolbar.ItemClicked += PART_Toolbar_ItemClicked;
        PART_MainView.PART_Gallery.ItemClicked += PART_Gallery_ItemClicked;
    }


    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        // allow the close through on the second pass after async work is done
        if (_isClosingHandled)
        {
            base.OnClosing(e);
            return;
        }

        // cancel the close so async work can complete before the app tears down
        e.Cancel = true;
        _isClosingHandled = true;

        // let a save the user just triggered finish writing before anything is torn down
        if (Photo.PendingSaveCount > 0)
        {
            _ = PART_MainView.PART_Message.ShowAsync(Core.Photos.CurrentFilePath,
                Core.Lang[LangId.Menu_MnuSave_Saving], durationMs: 0);

            await Photo.WaitForPendingSavesAsync(TimeSpan.FromMinutes(2));
        }

        // control events
        _status.Changed -= Status_Changed;
        _status.Dispose();

        PART_MainView.PART_Toolbar.ItemClicked -= PART_Toolbar_ItemClicked;
        PART_MainView.PART_Gallery.ItemClicked -= PART_Gallery_ItemClicked;


        // this pass already accepted the close, so nothing below may escape and keep the window open
        try
        {
            // stop slideshow so pre-slideshow config values are restored before saving
            Core.API?.IG_ToggleSlideshow(false);

            // stop all external tool processes before saving config
            await Core.ToolRegistry.ExternalTools.StopAllAsync();

            // only save config here, do NOT dispose resources yet
            await SaveConfigOnClosingAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] Closing failed: {ex}");
        }

        // now close for real — _isClosingHandled lets the second pass through
        Close();
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Dispose after the window (and its render loop) is fully closed
        Core.Dispose();
    }


    protected override async void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        if (e.Source is TextBox
            or NumericUpDown
            or MaskedTextBox
            or AutoCompleteBox) return;

        // process app hotkeys
        // press ESC: exit slideshow if it is running
        var hk = new Hotkey(e);
        if (hk.IsSame(Key.Escape) && Core.Slideshow?.IsRunning == true)
        {
            _ = await Core.API.RunApiAsync(API.IG_ToggleSlideshow, "false");
            e.Handled = true;
            return;
        }


        await Core.API.HandleKeyDownAsync(e);
        if (e.Handled) return;
    }


    protected override async void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Handled) return;

        // process app hotkeys
        await Core.API.HandleKeyUpAsync(e);
        if (e.Handled) return;
    }


    #endregion // Override Methods



    #region Control Events

    private async void AppInstance_InstanceInvoked(AppInstance sender, InstanceInvokedEventArgs e)
    {
        // handle single instance command
        if (e.Command.Equals(AppCmds.SINGLE_INSTANCE))
        {
            // un-minimize back to the pre-minimize state, so a maximized window stays maximized
            RestoreAndActivate();

            // set instance arguments
            var modulePath = Core.Args.ElementAtOrDefault(0) ?? string.Empty;
            Core.Args = [modulePath, .. e.Arguments];
            Core.UpdateInitImagePath();

            // apply any -p: config overrides from the forwarded args
            Config.ApplyCliOverrides(Core.Config, e.Arguments);

            // Refresh lock manager after CLI overrides
            ServiceProviders.FeatureManager.Refresh();

            // load image path
            _ = await Core.API.RunApiAsync(API.IG_OpenPath, Core.InputImagePathFromArgs);

            Activate();
            Topmost = true;
            Topmost = Core.Config.EnableWindowTopMost;
        }
    }


    private void Status_Changed(object? sender, EventArgs e)
    {
        VM.Title = _status.Text;
    }


    private void PART_Toolbar_ItemClicked(object sender, ToolbarItemClickEventArgs e)
    {
        _ = Core.API.RunActionAsync(e.VM.OnClick);
    }


    private async void PART_Gallery_ItemClicked(GalleryItem sender, GalleryItemClickEventArgs e)
    {
        var photoIndex = Core.Photos.IndexOf(sender.VM.FilePath);
        _ = await Core.API.RunApiAsync(API.IG_ViewByIndex, photoIndex.ToString());
    }


    #endregion Control Events




    /// <summary>
    /// Captures the current windowed layout, i.e. the state full screen mode returns to.
    /// </summary>
    public WindowLayoutSnapshot CaptureWindowLayout()
    {
        // the tracked windowed bounds, never the live ones: a maximized, full screen or minimized
        // window reports the bounds of that layout, which is not the one to come back to
        Rect? bounds = WindowedBounds is { } b
            && b.Width >= MIN_RESTORE_WIDTH
            && b.Height >= MIN_RESTORE_HEIGHT ? b : null;

        return new WindowLayoutSnapshot
        {
            IsMaximized = RestorableWindowState == WindowState.Maximized,
            Bounds = bounds,
            ShowToolbar = Core.Config.ShowToolbar,
            ShowGallery = Core.Config.ShowGallery,
            IsFrameless = Core.Config.EnableFrameless,
            IsWindowFit = Core.Config.EnableWindowFit,
        };
    }


    /// <summary>
    /// Captures live window/viewer state into config and saves it; also used before a self-update kills us.
    /// </summary>
    internal async Task<bool> CaptureAndSaveConfigAsync()
    {
        // 1. save full screen mode; a minimized window reports the state it will restore to
        Core.Config.EnableFullScreen = RestorableWindowState == WindowState.FullScreen;

        // 2. save the windowed layout: while in full screen that is the backup taken on entry,
        // never the live values, which full screen has already overwritten in Config
        var layout = Core.API.PreFullScreenLayout ?? CaptureWindowLayout();

        Core.Config.EnableMainWindowMaximized = layout.IsMaximized;
        Core.Config.ShowToolbar = layout.ShowToolbar;
        Core.Config.ShowGallery = layout.ShowGallery;
        Core.Config.EnableFrameless = layout.IsFrameless;
        Core.Config.EnableWindowFit = layout.IsWindowFit;
        if (layout.Bounds is { } bounds) Core.Config.MainWindowBounds = bounds;


        Core.Config.ZoomLockValue = PART_MainView.PART_Viewer.ZoomFactor * 100f;
        Core.Config.LastSeenImagePath = Core.Config.EnableLastSeenImage
            ? Core.Photos.CurrentFilePath
            : string.Empty;


        // persist the current hosted tool's settings, but keep LastOpenedTool intact
        // (IG_CloseTool clears it) so the tool is re-opened on next launch
        if (PART_MainView.PART_ToolHost.Tool is ITool toolToSave)
        {
            ToolRegistry.SaveToolSettings(toolToSave);
        }

        return await Core.Config.SaveAsync();
    }


    private async Task SaveConfigOnClosingAsync()
    {
        // hide the open windows
        Hide();
        App.SettingsWindow?.Hide();


        // save config to file
        var taskConfig = CaptureAndSaveConfigAsync();


        // permanently adds the data that is on the Clipboard so that it is available
        // after the data's original application closes.
        if (Clipboard is not null)
        {
            try
            {
                await Clipboard.FlushAsync();
            }
            catch { }
        }

        await taskConfig;

        // cleaning
        try
        {
            // delete trash
            var tempDir = BHelper.ConfigDir(Dir.Temporary);
            Directory.Delete(tempDir, true);
        }
        catch { }
    }



}