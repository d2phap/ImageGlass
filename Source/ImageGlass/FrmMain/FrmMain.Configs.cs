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

using ImageGlass.Base;
using ImageGlass.Base.Actions;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Configs contains methods for dynamic binding   *
 * ****************************************************** */
public partial class FrmMain
{
    /// <summary>
    /// Hotkeys list of main menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> CurrentMenuHotkeys { get; set; } = new()
    {
	    // Open main menu
	    { nameof(MnuMain),                  [new(Keys.Alt | Keys.F)] },

	    // MnuFile
	    { nameof(MnuOpenFile),              [new(Keys.Control | Keys.O)] },
        { nameof(MnuPasteImage),            [new(Keys.Control | Keys.V)] },
        { nameof(MnuNewWindow),             [new(Keys.Control | Keys.N)] },
        { nameof(MnuSave),                  [new(Keys.Control | Keys.S)] },
        { nameof(MnuSaveAs),                [new(Keys.Control | Keys.Shift | Keys.S)] },
        { nameof(MnuOpenWith),              [new(Keys.D)] },
        { nameof(MnuEdit),                  [new(Keys.E)] },
        { nameof(MnuPrint),                 [new(Keys.Control | Keys.P)] },
        { nameof(MnuShare),                 [new(Keys.S)] },
        { nameof(MnuRefresh),               [new(Keys.R)] },
        { nameof(MnuReload),                [new(Keys.Control | Keys.R)] },
        { nameof(MnuReloadImageList),       [new(Keys.Control | Keys.Shift | Keys.R)] },
        { nameof(MnuUnload),                [new(Keys.U)] },

        // MnuNavigation
        { nameof(MnuViewNext),              [new(Keys.Right)] },
        { nameof(MnuViewPrevious),          [new(Keys.Left)] },
        { nameof(MnuGoTo),                  [new(Keys.F)] },
        { nameof(MnuGoToFirst),             [new(Keys.Home)] },
        { nameof(MnuGoToLast),              [new(Keys.End)] },
        { nameof(MnuViewNextFrame),         [new(Keys.Control | Keys.Right)] },
        { nameof(MnuViewPreviousFrame),     [new(Keys.Control | Keys.Left)] },
        { nameof(MnuViewFirstFrame),        [new(Keys.Control | Keys.Home)] },
        { nameof(MnuViewLastFrame),         [new(Keys.Control | Keys.End)] },

        // MnuPanning
        { nameof(MnuPanLeft),               [new(Keys.Alt | Keys.Left)] },
        { nameof(MnuPanRight),              [new(Keys.Alt | Keys.Right)] },
        { nameof(MnuPanUp),                 [new(Keys.Alt | Keys.Up)] },
        { nameof(MnuPanDown),               [new(Keys.Alt | Keys.Down)] },

        { nameof(MnuPanToLeftSide),         [new(Keys.Control | Keys.Alt | Keys.Left)] },
        { nameof(MnuPanToRightSide),        [new(Keys.Control | Keys.Alt | Keys.Right)] },
        { nameof(MnuPanToTop),              [new(Keys.Control | Keys.Alt | Keys.Up)] },
        { nameof(MnuPanToBottom),           [new(Keys.Control | Keys.Alt | Keys.Down)] },

        // MnuZoom
        { nameof(MnuZoomIn),                [new(Keys.Oemplus), new(Keys.Add)] }, // =
        { nameof(MnuZoomOut),               [new(Keys.OemMinus), new(Keys.Subtract)] }, // -
        { nameof(MnuCustomZoom),            [new(Keys.Z)] },
        { nameof(MnuActualSize),            [new(Keys.D0), new(Keys.NumPad0)] },
        { nameof(MnuAutoZoom),              [new(Keys.D1), new(Keys.NumPad1)] },
        { nameof(MnuLockZoom),              [new(Keys.D2), new(Keys.NumPad2)] },
        { nameof(MnuScaleToWidth),          [new(Keys.D3), new(Keys.NumPad3)] },
        { nameof(MnuScaleToHeight),         [new(Keys.D4), new(Keys.NumPad4)] },
        { nameof(MnuScaleToFit),            [new(Keys.D5), new(Keys.NumPad5)] },
        { nameof(MnuScaleToFill),           [new(Keys.D6), new(Keys.NumPad6)] },

        // MnuImage
        { nameof(MnuViewChannels),          [new(Keys.Shift | Keys.C)] },
        { nameof(MnuLoadingOrders),         [new(Keys.Shift | Keys.O)] },
        { nameof(MnuRotateLeft),            [new(Keys.Control | Keys.OemPeriod)] }, // Ctrl+.
        { nameof(MnuRotateRight),           [new(Keys.Control | Keys.OemQuestion)] }, // Ctrl+/
        { nameof(MnuFlipHorizontal),        [new(Keys.Control | Keys.Oem1)] }, // Ctrl+;
        { nameof(MnuFlipVertical),          [new(Keys.Control | Keys.Oem7)] }, // Ctrl+'
        { nameof(MnuRename),                [new(Keys.F2)] },
        { nameof(MnuMoveToRecycleBin),      [new(Keys.Delete)] },
        { nameof(MnuDeleteFromHardDisk),    [new(Keys.Shift | Keys.Delete)] },
        { nameof(MnuToggleImageAnimation),  [new(Keys.Control | Keys.Space)] },
        { nameof(MnuExportFrames),          [new(Keys.Control | Keys.J)] },
        { nameof(MnuOpenLocation),          [new(Keys.L)] },
        { nameof(MnuImageProperties),       [new(Keys.Control | Keys.I)] },

        // MnuClipboard
        { nameof(MnuCopyImageData),         [new(Keys.Control | Keys.C)] },
        { nameof(MnuCopyFile),              [new(Keys.Control | Keys.Shift | Keys.C)] },
        { nameof(MnuCutFile),               [new(Keys.Control | Keys.X)] },
        { nameof(MnuCopyPath),              [new(Keys.Control | Keys.L)] },
        { nameof(MnuClearClipboard),        [new(Keys.Control | Keys.Oemtilde)] }, // Ctrl+`

        { nameof(MnuWindowFit),             [new(Keys.F9)] },
        { nameof(MnuFrameless),             [new(Keys.F10)] },
        { nameof(MnuFullScreen),            [new(Keys.F11)] },
        { nameof(MnuSlideshow),             [new(Keys.F12)] },

        // MnuLayout
        { nameof(MnuToggleToolbar),         [new(Keys.T)] },
        { nameof(MnuToggleGallery),         [new(Keys.G)] },
        { nameof(MnuToggleCheckerboard),    [new(Keys.B)] },

        // MnuTools
        { nameof(MnuColorPicker),           [new(Keys.K)] },
        { nameof(MnuCropTool),              [new(Keys.C)] },
        { nameof(MnuFrameNav),              [new(Keys.P)] },
        { nameof(MnuLosslessCompression),   [new(Keys.Alt | Keys.C)] },
        { Const.IGTOOL_EXIFTOOL,            [new(Keys.X)] },

        // MnuHelp
        { nameof(MnuAbout),                 [new(Keys.F1)] },

        { nameof(MnuSettings),              [new(Keys.Control | Keys.Oemcomma)] }, // Ctrl+,
        { nameof(MnuExit),                  [new(Keys.Escape), new (Keys.Control | Keys.W)] },
    };


    private void SetUpFrmMainConfigs()
    {
        SuspendLayout();

        // Toolbar
        Toolbar.Alignment = Config.EnableCenterToolbar
            ? ToolbarAlignment.Center
            : ToolbarAlignment.Left;
        LoadToolbarButtons(Config.ToolbarButtons.Count == 0);


        // Thumbnail bar
        Gallery.PersistentCacheSize = Config.GalleryCacheSizeInMb;
        Gallery.DoNotDeletePersistentCache = Config.GalleryCacheSizeInMb > 0;
        Gallery.PersistentCacheDirectory = App.ConfigDir(PathType.Dir, Dir.ThumbnailsCache);
        Gallery.EnableKeyNavigation = false;
        Gallery.Padding = this.ScaleToDpi(new Padding(2));
        Gallery.ResizedByResizer += Gallery_ResizedByResizer;


        // PicMain
        PicMain.EnableDebug = Config.EnableDebug;
        PicMain.NavDisplay = Config.EnableNavigationButtons
            ? NavButtonDisplay.Both
            : NavButtonDisplay.None;
        PicMain.TabStop = false;
        PicMain.PanDistance = Config.PanSpeed;
        PicMain.ZoomSpeed = Config.ZoomSpeed;
        PicMain.ZoomLevels = Config.ZoomLevels;
        PicMain.InterpolationScaleDown = Config.ImageInterpolationScaleDown;
        PicMain.InterpolationScaleUp = Config.ImageInterpolationScaleUp;
        IG_SetZoomMode(Config.ZoomMode.ToString());

        if (Config.ZoomMode == ZoomMode.LockZoom)
        {
            PicMain.ZoomFactor = Config.ZoomLockValue / 100f;
        }

        IG_ToggleCheckerboard(Config.ShowCheckerboard);

        // set up layout
        LoadAppLayout();

        ResumeLayout(false);

        Load += FrmMainConfig_Load;
        FormClosing += FrmMainConfig_FormClosing;
        SizeChanged += FrmMainConfig_SizeChanged;


        // load default mouse actions
        LoadDefaultMouseClickActions();
        LoadDefaultMouseWheelActions();
    }


    /// <summary>
    /// Initialize context toolbar. Call once after <see cref="InitializeComponent"/> function.
    /// </summary>
    private void InitializeToolbarContext()
    {
        SuspendLayout();

        ToolbarContext.SuspendLayout();
        ToolbarContext.Alignment = UI.ToolbarAlignment.Center;
        ToolbarContext.AutoFocusOnHover = true;
        ToolbarContext.GripMargin = new Padding(0);
        ToolbarContext.Padding = new Padding(this.ScaleToDpi(4));
        ToolbarContext.GripStyle = ToolStripGripStyle.Hidden;
        ToolbarContext.Name = "ToolbarContext";
        ToolbarContext.ShowItemToolTips = false;
        ToolbarContext.ShowMainMenuButton = false;
        ToolbarContext.TabIndex = 3;
        ToolbarContext.Visible = false;
        ToolbarContext.ItemClicked += Toolbar_ItemClicked;

        Controls.Add(ToolbarContext);
        ToolbarContext.ResumeLayout(false);
        ResumeLayout(false);
    }


    private void FrmMainConfig_Load(object? sender, EventArgs e)
    {
        Local.FrmMainUpdateRequested += Local_FrmMainUpdateRequested;

        // IsWindowAlwaysOnTop
        IG_ToggleTopMost(Config.EnableWindowTopMost, showInAppMessage: false);

        // load language pack
        Local.UpdateFrmMain(UpdateRequests.Language);

        // load menu
        LoadExternalTools();
        CurrentMenuHotkeys = Config.GetAllHotkeys(CurrentMenuHotkeys);
        Local.UpdateFrmMain(UpdateRequests.MenuHotkeys);

        // Initialize form movable
        #region Form movable
        _movableForm = new(this)
        {
            Key = Keys.ShiftKey | Keys.Shift,
            FreeMoveControlNames = new HashSet<string>()
            {
                nameof(Toolbar),
                nameof(ToolbarContext),
            },
        };

        // Enable form movable
        IG_SetWindowMoveable(true);
        #endregion // Form movable

        // make sure all controls are painted before showing window
        Application.DoEvents();

        // toggle toolbar
        IG_ToggleToolbar(Config.ShowToolbar);

        // toggle gallery
        IG_ToggleGallery(Config.ShowGallery);


        // load Full screen mode
        if (Config.EnableFullScreen)
        {
            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit);

            // to hide the animation effect of window border
            FormBorderStyle = FormBorderStyle.None;

            // load window placement from settings here to save the initial
            // position of window so that when user exists the fullscreen mode,
            // it can be restore correctly
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

            IG_ToggleFullScreen(true);
        }
        else
        {
            // load window placement from settings
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit);
        }

        // start slideshow
        if (Config.EnableSlideshow)
        {
            IG_ToggleSlideshow();
        }

        // load context menu config
        Local.UpdateFrmMain(UpdateRequests.MouseActions);


        // focus on PicMain
        PicMain.Focus();


        // update tag data for zoom mode menus
        MnuAutoZoom.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuLockZoom.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToWidth.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToHeight.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToFit.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToFill.Tag = new ModernMenuItemTag() { SingleSelect = true };
    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _ = SaveConfigsOnClosing();

        ToolbarContext.Dispose();
    }


    public async Task SaveConfigsOnClosing()
    {
        // save FrmMain placement
        if (!Config.EnableFullScreen)
        {
            var wp = WindowSettings.GetPlacementFromWindow(this);
            WindowSettings.SetFrmMainPlacementConfig(wp);
        }


        Config.LastSeenImagePath = Local.Images.GetFilePath(Local.CurrentIndex);
        Config.ZoomLockValue = PicMain.ZoomFactor * 100f;


        // save config to file
        await Config.WriteAsync();


        // cleaning
        try
        {
            // delete trash
            Directory.Delete(App.ConfigDir(PathType.Dir, Dir.Temporary), true);
        }
        catch { }
    }


    private void FrmMainConfig_SizeChanged(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Normal
            && !Config.EnableFullScreen)
        {
            Config.FrmMainPositionX = Location.X;
            Config.FrmMainPositionY = Location.Y;
            Config.FrmMainWidth = Size.Width;
            Config.FrmMainHeight = Size.Height;
        }

        UpdateGallerySize();
    }


    private void Gallery_ResizedByResizer(object? sender, EventArgs e)
    {
        if (Gallery.View == ImageGlass.Gallery.View.Thumbnails)
        {
            Config.GalleryColumns = Gallery.MaxColumns;
        }
    }


    /// <summary>
    /// Processes internal update requests
    /// </summary>
    private void Local_FrmMainUpdateRequested(UpdateRequestEventArgs e)
    {
        if (e.Requests.HasFlag(UpdateRequests.ReloadImage))
        {
            IG_Reload();
        }

        if (e.Requests.HasFlag(UpdateRequests.ReloadImageList))
        {
            IG_ReloadList();
        }

        if (e.Requests.HasFlag(UpdateRequests.Slideshow))
        {
            // TODO:
        }

        if (e.Requests.HasFlag(UpdateRequests.Layout))
        {
            LoadAppLayout(true);
        }

        if (e.Requests.HasFlag(UpdateRequests.ToolbarButtons))
        {
            LoadToolbarButtons();
            LoadToolbarItemsText(Toolbar);
        }

        if (e.Requests.HasFlag(UpdateRequests.ToolbarIcons)
            || e.Requests.HasFlag(UpdateRequests.ToolbarButtons))
        {
            Toolbar.UpdateTheme(this.ScaleToDpi(Config.ToolbarIconHeight));
            ToolbarContext.UpdateTheme(this.ScaleToDpi(Config.ToolbarIconHeight));
        }

        if (e.Requests.HasFlag(UpdateRequests.ToolbarIcons)
            || e.Requests.HasFlag(UpdateRequests.ToolbarAlignment))
        {
            Toolbar.Alignment = Config.EnableCenterToolbar
                ? ToolbarAlignment.Center
                : ToolbarAlignment.Left;
        }

        if (e.Requests.HasFlag(UpdateRequests.Gallery))
        {
            Gallery.ScrollBars = Config.ShowGalleryScrollbars || Gallery.View == ImageGlass.Gallery.View.Thumbnails;
            Gallery.ShowItemText = Config.ShowGalleryFileName;
            Gallery.PersistentCacheSize = Config.GalleryCacheSizeInMb;
            Gallery.DoNotDeletePersistentCache = Config.GalleryCacheSizeInMb > 0;

            // disable thumbnail cache
            if (Config.GalleryCacheSizeInMb == 0)
            {
                Gallery.ClearThumbnailCache();
            }

            // update gallery size
            UpdateGallerySize();
        }

        if (e.Requests.HasFlag(UpdateRequests.Language))
        {
            Config.TriggerRequestUpdatingLanguage();
        }

        if (e.Requests.HasFlag(UpdateRequests.Theme))
        {
            Config.LoadThemePack(WinColorsApi.IsDarkMode, true, true, true);
            Config.TriggerRequestUpdatingTheme();
        }

        if (e.Requests.HasFlag(UpdateRequests.Appearance))
        {
            Config.TriggerRequestUpdatingTheme();
        }

        if (e.Requests.HasFlag(UpdateRequests.MenuHotkeys))
        {
            LoadMenuHotkeys();
            LoadToolbarItemsText(Toolbar);
        }

        if (e.Requests.HasFlag(UpdateRequests.MouseActions))
        {
            var executable = nameof(IG_OpenContextMenu);
            var hasRightClick = Config.MouseClickActions.ContainsKey(MouseClickEvent.RightClick);

            // get the executable value of the right click action
            if (hasRightClick)
            {
                var toggleAction = Config.MouseClickActions[MouseClickEvent.RightClick];
                var isToggled = ToggleAction.IsToggleOff(toggleAction.Id);
                var action = isToggled
                    ? toggleAction.ToggleOff
                    : toggleAction.ToggleOn;

                if (action != null)
                {
                    executable = action.Executable.Trim();
                }
            }


            // set context menu = MnuContext
            if (executable == nameof(IG_OpenContextMenu))
            {
                PicMain.ContextMenuStrip = MnuContext;
            }
            // set context menu = MnuMain
            else if (executable == nameof(IG_OpenMainMenu))
            {
                PicMain.ContextMenuStrip = MnuMain;
            }
            // remove context menu
            else
            {
                PicMain.ContextMenuStrip = null;
            }
        }


        e.OnUpdated?.Invoke(e.Requests);
    }


    protected override void OnRequestUpdatingLanguage()
    {
        LoadLanguage();
        LoadImageInfo();
    }


    /// <summary>
    /// Load toolbar buttons according to <see cref="Config.ToolbarButtons"/>.
    /// </summary>
    private void LoadToolbarButtons(bool forcedReset = false)
    {
        if (forcedReset)
        {
            Config.ToolbarButtons = new();
            foreach (var btnId in Local.DefaultToolbarItemIds)
            {
                if (btnId == nameof(ToolbarItemModelType.Separator))
                {
                    Config.ToolbarButtons.Add(new()
                    {
                        Type = ToolbarItemModelType.Separator,
                    });
                }
                else if (Local.BuiltInToolbarItems.FirstOrDefault(i => i.Id == btnId) is ToolbarItemModel btn)
                {
                    Config.ToolbarButtons.Add(btn);
                }
            }
        }


        Toolbar.ClearItems();
        Toolbar.AddItems(Config.ToolbarButtons);
    }


    /// <summary>
    /// Updates items state of <see cref="Toolbar"/>
    /// </summary>
    private void UpdateToolbarItemsState(bool performLayout = true)
    {
        Toolbar.SuspendLayout();

        // Toolbar buttons
        foreach (var item in Toolbar.Items)
        {
            if (item.GetType() == typeof(ToolStripButton))
            {
                var tItem = item as ToolStripButton;
                if (tItem is null) continue;

                // update item from metadata
                var tagModel = tItem.Tag as ToolbarItemTagModel;
                if (tagModel is null) continue;

                // load check state:
                // Executable is menu item
                if (tagModel.OnClick.Executable.StartsWith("Mnu", StringComparison.Ordinal))
                {
                    var field = GetType().GetField(tagModel.OnClick.Executable);
                    var mnu = field?.GetValue(this) as ToolStripMenuItem;

                    if (mnu is not null)
                    {
                        tItem.Checked = mnu.Checked;
                    }
                }

                // get config name
                var configProp = Config.GetProp(tagModel.CheckableConfigBinding);
                var propValue = configProp?.GetValue(null);
                if (propValue is null) continue;

                // Executable is IGMethod
                // Example: OnClick = new("IG_SetZoomMode", ZoomMode.AutoZoom.ToString())
                else if (configProp.PropertyType.IsEnum)
                {
                    tItem.Checked = tagModel.OnClick.Arguments.FirstOrDefault().Equals(propValue.ToString());
                }
                // Executable is IGMethod
                // Example: OnClick = new("IG_ToggleToolbar", false)
                else if (configProp.PropertyType.Equals(typeof(bool)))
                {
                    if (bool.TryParse(propValue.ToString(), out bool value))
                    {
                        tItem.Checked = value;
                    }
                }
            }
        }

        Toolbar.ResumeLayout(performLayout);
    }


    /// <summary>
    /// Load language.
    /// </summary>
    public void LoadLanguage()
    {
        var lang = Config.Language;

        #region Main menu

        // Menu File
        #region Menu File
        MnuFile.Text = lang[$"{Name}.{nameof(MnuFile)}"];

        MnuOpenFile.Text = lang[$"{Name}.{nameof(MnuOpenFile)}"];
        MnuPasteImage.Text = lang[$"{Name}.{nameof(MnuPasteImage)}"];
        MnuNewWindow.Text = lang[$"{Name}.{nameof(MnuNewWindow)}"];
        MnuSave.Text = lang[$"{Name}.{nameof(MnuSave)}"];
        MnuSaveAs.Text = lang[$"{Name}.{nameof(MnuSaveAs)}"];

        MnuOpenWith.Text = lang[$"{Name}.{nameof(MnuOpenWith)}"];
        MnuEdit.Text = lang[$"{Name}.{nameof(MnuEdit)}"];
        MnuPrint.Text = lang[$"{Name}.{nameof(MnuPrint)}"];
        MnuShare.Text = lang[$"{Name}.{nameof(MnuShare)}"];

        MnuRefresh.Text = lang[$"{Name}.{nameof(MnuRefresh)}"];
        MnuReload.Text = lang[$"{Name}.{nameof(MnuReload)}"];
        MnuReloadImageList.Text = lang[$"{Name}.{nameof(MnuReloadImageList)}"];
        MnuUnload.Text = lang[$"{Name}.{nameof(MnuUnload)}"];

        #endregion


        // Menu Navigation
        #region Menu Navigation
        MnuNavigation.Text = lang[$"{Name}.{nameof(MnuNavigation)}"];

        MnuViewNext.Text = lang[$"{Name}.{nameof(MnuViewNext)}"];
        MnuViewPrevious.Text = lang[$"{Name}.{nameof(MnuViewPrevious)}"];

        MnuGoTo.Text = lang[$"{Name}.{nameof(MnuGoTo)}"];
        MnuGoToFirst.Text = lang[$"{Name}.{nameof(MnuGoToFirst)}"];
        MnuGoToLast.Text = lang[$"{Name}.{nameof(MnuGoToLast)}"];

        MnuViewNextFrame.Text = lang[$"{Name}.{nameof(MnuViewNextFrame)}"];
        MnuViewPreviousFrame.Text = lang[$"{Name}.{nameof(MnuViewPreviousFrame)}"];
        MnuViewFirstFrame.Text = lang[$"{Name}.{nameof(MnuViewFirstFrame)}"];
        MnuViewLastFrame.Text = lang[$"{Name}.{nameof(MnuViewLastFrame)}"];

        #endregion // Menu Navigation


        // Menu Zoom
        #region Menu Zoom
        MnuZoom.Text = lang[$"{Name}.{nameof(MnuZoom)}"];

        MnuZoomIn.Text = lang[$"{Name}.{nameof(MnuZoomIn)}"];
        MnuZoomOut.Text = lang[$"{Name}.{nameof(MnuZoomOut)}"];
        MnuCustomZoom.Text = lang[$"{Name}.{nameof(MnuCustomZoom)}"];
        MnuActualSize.Text = lang[$"{Name}.{nameof(MnuActualSize)}"];

        MnuAutoZoom.Text = lang[$"{Name}.{nameof(MnuAutoZoom)}"];
        MnuLockZoom.Text = lang[$"{Name}.{nameof(MnuLockZoom)}"];
        MnuScaleToWidth.Text = lang[$"{Name}.{nameof(MnuScaleToWidth)}"];
        MnuScaleToHeight.Text = lang[$"{Name}.{nameof(MnuScaleToHeight)}"];
        MnuScaleToFit.Text = lang[$"{Name}.{nameof(MnuScaleToFit)}"];
        MnuScaleToFill.Text = lang[$"{Name}.{nameof(MnuScaleToFill)}"];

        #endregion // Menu Zoom


        // Menu Panning
        #region Panning
        MnuPanning.Text = lang[$"{Name}.{nameof(MnuPanning)}"];

        MnuPanLeft.Text = lang[$"{Name}.{nameof(MnuPanLeft)}"];
        MnuPanRight.Text = lang[$"{Name}.{nameof(MnuPanRight)}"];
        MnuPanUp.Text = lang[$"{Name}.{nameof(MnuPanUp)}"];
        MnuPanDown.Text = lang[$"{Name}.{nameof(MnuPanDown)}"];

        MnuPanToLeftSide.Text = lang[$"{Name}.{nameof(MnuPanToLeftSide)}"];
        MnuPanToRightSide.Text = lang[$"{Name}.{nameof(MnuPanToRightSide)}"];
        MnuPanToTop.Text = lang[$"{Name}.{nameof(MnuPanToTop)}"];
        MnuPanToBottom.Text = lang[$"{Name}.{nameof(MnuPanToBottom)}"];
        #endregion // Menu Panning


        // Menu Image
        #region Menu Image
        MnuImage.Text = lang[$"{Name}.{nameof(MnuImage)}"];
        MnuRotateLeft.Text = lang[$"{Name}.{nameof(MnuRotateLeft)}"];
        MnuRotateRight.Text = lang[$"{Name}.{nameof(MnuRotateRight)}"];
        MnuFlipHorizontal.Text = lang[$"{Name}.{nameof(MnuFlipHorizontal)}"];
        MnuFlipVertical.Text = lang[$"{Name}.{nameof(MnuFlipVertical)}"];

        MnuRename.Text = lang[$"{Name}.{nameof(MnuRename)}"];
        MnuMoveToRecycleBin.Text = lang[$"{Name}.{nameof(MnuMoveToRecycleBin)}"];
        MnuDeleteFromHardDisk.Text = lang[$"{Name}.{nameof(MnuDeleteFromHardDisk)}"];
        MnuExportFrames.Text = lang[$"{Name}.{nameof(MnuExportFrames)}"];
        MnuToggleImageAnimation.Text = lang[$"{Name}.{nameof(MnuToggleImageAnimation)}"];
        MnuSetDesktopBackground.Text = lang[$"{Name}.{nameof(MnuSetDesktopBackground)}"];
        MnuSetLockScreen.Text = lang[$"{Name}.{nameof(MnuSetLockScreen)}"];
        MnuOpenLocation.Text = lang[$"{Name}.{nameof(MnuOpenLocation)}"];
        MnuImageProperties.Text = lang[$"{Name}.{nameof(MnuImageProperties)}"];

        MnuViewChannels.Text = lang[$"{Name}.{nameof(MnuViewChannels)}"];
        LoadMnuViewChannelsSubItems(); // update Channels menu items

        MnuLoadingOrders.Text = lang[$"{Name}.{nameof(MnuLoadingOrders)}"];
        LoadMnuLoadingOrdersSubItems(); // update Loading order items
        #endregion


        // Menu Clipboard
        #region Menu Clipboard
        MnuClipboard.Text = lang[$"{Name}.{nameof(MnuClipboard)}"];

        MnuCopyFile.Text = lang[$"{Name}.{nameof(MnuCopyFile)}"];
        MnuCopyImageData.Text = lang[$"{Name}.{nameof(MnuCopyImageData)}"];
        MnuCutFile.Text = lang[$"{Name}.{nameof(MnuCutFile)}"];
        MnuCopyPath.Text = lang[$"{Name}.{nameof(MnuCopyPath)}"];
        MnuClearClipboard.Text = lang[$"{Name}.{nameof(MnuClearClipboard)}"];
        #endregion


        // Window modes & Menu Slideshow
        #region Window modes
        MnuWindowFit.Text = lang[$"{Name}.{nameof(MnuWindowFit)}"];
        MnuFullScreen.Text = lang[$"{Name}.{nameof(MnuFullScreen)}"];
        MnuFrameless.Text = lang[$"{Name}.{nameof(MnuFrameless)}"];
        MnuSlideshow.Text = lang[$"{Name}.{nameof(MnuSlideshow)}"];
        #endregion


        // Menu Layout
        #region Menu Layout
        MnuLayout.Text = lang[$"{Name}.{nameof(MnuLayout)}"];

        MnuToggleToolbar.Text = lang[$"{Name}.{nameof(MnuToggleToolbar)}"];
        MnuToggleGallery.Text = lang[$"{Name}.{nameof(MnuToggleGallery)}"];
        MnuToggleCheckerboard.Text = lang[$"{Name}.{nameof(MnuToggleCheckerboard)}"];
        MnuToggleTopMost.Text = lang[$"{Name}.{nameof(MnuToggleTopMost)}"];
        #endregion


        // Menu Tools
        #region Menu Tools
        MnuTools.Text = lang[$"{Name}.{nameof(MnuTools)}"];

        MnuColorPicker.Text = lang[$"{Name}.{nameof(MnuColorPicker)}"];
        MnuFrameNav.Text = lang[$"{Name}.{nameof(MnuFrameNav)}"];
        MnuCropTool.Text = lang[$"{Name}.{nameof(MnuCropTool)}"];
        MnuLosslessCompression.Text = lang[$"{Name}.{nameof(MnuLosslessCompression)}"];
        MnuGetMoreTools.Text = lang[$"{Name}.{nameof(MnuGetMoreTools)}"];

        foreach (var item in MnuTools.DropDownItems)
        {
            if (item is not ToolStripMenuItem mnuItem) continue;
            if (!Config.Language.TryGetValue($"_.Tools.{mnuItem.Name}", out var toolDisplayName)) continue;

            mnuItem.Text = toolDisplayName;
        }
        #endregion


        MnuSettings.Text = lang[$"{Name}.{nameof(MnuSettings)}"];
        MnuExit.Text = lang[$"{Name}.{nameof(MnuExit)}"];


        // Menu Help
        #region Menu Help
        MnuHelp.Text = lang[$"{Name}.{nameof(MnuHelp)}"];

        MnuAbout.Text = lang[$"{Name}.{nameof(MnuAbout)}"];
        MnuQuickSetup.Text = lang[$"{Name}.{nameof(MnuQuickSetup)}"];
        MnuReportIssue.Text = lang[$"{Name}.{nameof(MnuReportIssue)}"];

        MnuSetDefaultPhotoViewer.Text = lang[$"{Name}.{nameof(MnuSetDefaultPhotoViewer)}"];
        MnuRemoveDefaultPhotoViewer.Text = lang[$"{Name}.{nameof(MnuRemoveDefaultPhotoViewer)}"];
        #endregion


        #endregion


        // Toolbar
        LoadToolbarItemsText(Toolbar);

        // load disabled state of menu
        LoadMenuDisabledState();
    }


    /// <summary>
    /// Load hotkeys of menu
    /// </summary>
    /// <param name="menu"></param>
    public void LoadMenuHotkeys(ToolStripDropDown? menu = null)
    {
        if (InvokeRequired)
        {
            Invoke(LoadMenuHotkeys, menu);
            return;
        }

        // default: main menu
        menu ??= MnuMain;


        var allItems = MenuUtils.GetActualItems(menu.Items);
        foreach (ToolStripMenuItem item in allItems)
        {
            item.ShortcutKeyDisplayString = Config.GetHotkeyString(CurrentMenuHotkeys, item.Name);

            if (item.HasDropDownItems)
            {
                LoadMenuHotkeys(item.DropDown);
            }
        }
    }


    /// <summary>
    /// Loads disables state of menu
    /// </summary>
    private void LoadMenuDisabledState()
    {
        // load DisabledMenus
        foreach (var mnuName in Config.DisabledMenus)
        {
            var field = GetType().GetField(mnuName);
            if (field?.GetValue(this) is not ToolStripMenuItem mnu) continue;

            mnu.Enabled = false;

            if (!mnu.Text.EndsWith("🔒", StringComparison.OrdinalIgnoreCase))
            {
                mnu.Text += " 🔒";
            }
        }
    }


    /// <summary>
    /// Loads text and tooltip for toolbar items
    /// </summary>
    public void LoadToolbarItemsText(ModernToolbar modernToolbar)
    {
        foreach (var item in modernToolbar.Items)
        {
            if (item.GetType() == typeof(ToolStripButton))
            {
                var tItem = item as ToolStripButton;
                if (tItem is null) continue;

                var tagModel = tItem.Tag as ToolbarItemTagModel;
                if (tagModel is null) continue;

                string langKey;
                string hotkey;
                if (tItem.Name == Toolbar.MainMenuButton.Name)
                {
                    langKey = $"{Name}.MnuMain";
                    hotkey = Config.GetHotkeyString(CurrentMenuHotkeys, nameof(MnuMain));
                }
                else
                {
                    langKey = $"{Name}.{tagModel.OnClick.Executable}";
                    hotkey = Config.GetHotkeyString(CurrentMenuHotkeys, tagModel.OnClick.Executable);
                }

                if (Config.Language.TryGetValue(langKey, out string? value))
                {
                    tItem.Text = tItem.ToolTipText = value;

                    if (!string.IsNullOrEmpty(hotkey))
                    {
                        tItem.ToolTipText += $" ({hotkey})";
                    }
                }
            }
        }
    }


    /// <summary>
    /// Load View Channels menu items
    /// </summary>
    private void LoadMnuViewChannelsSubItems()
    {
        // clear items
        MnuViewChannels.DropDown.Items.Clear();

        var newMenuIconHeight = this.ScaleToDpi(Const.MENU_ICON_HEIGHT);

        // add new items
        foreach (var channel in Enum.GetValues(typeof(ColorChannel)))
        {
            var channelName = Enum.GetName(typeof(ColorChannel), channel);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"{Name}.{nameof(MnuViewChannels)}._{channelName}"],
                Tag = new ModernMenuItemTag()
                {
                    SingleSelect = true,
                    ColorChannel = (ColorChannel)channel,
                },
                CheckOnClick = true,
                Checked = (int)channel == (int)Local.ImageChannel,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight),
            };

            mnu.Click += MnuViewChannelsItem_Click;
            MnuViewChannels.DropDown.Items.Add(mnu);
        }
    }


    private void MnuViewChannelsItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        // get color channel from tag
        if (mnu.Tag is ModernMenuItemTag tag
            && tag.ColorChannel != null
            && tag.ColorChannel.Value != Local.ImageChannel)
        {
            Local.ImageChannel = tag.ColorChannel.Value;
            Local.Images.ImageChannel = tag.ColorChannel.Value;

            // update the viewing image
            _ = ViewNextCancellableAsync(0, true, true);

            // update cached images
            Local.Images.UpdateCache();

            // reload state
            LoadMnuViewChannelsSubItems();
        }
    }


    /// <summary>
    /// Load Loading order menu items
    /// </summary>
    private void LoadMnuLoadingOrdersSubItems()
    {
        // clear items
        MnuLoadingOrders.DropDown.Items.Clear();

        var newMenuIconHeight = this.ScaleToDpi(Const.MENU_ICON_HEIGHT);

        // add ImageOrderBy items
        foreach (var order in Enum.GetValues(typeof(ImageOrderBy)))
        {
            var orderName = Enum.GetName(typeof(ImageOrderBy), order);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"_.{nameof(ImageOrderBy)}._{orderName}"],
                Tag = new ModernMenuItemTag()
                {
                    SingleSelect = true,
                    ImageOrderBy = (ImageOrderBy)order,
                },
                CheckOnClick = true,
                Checked = (int)order == (int)Config.ImageLoadingOrder,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
            };

            mnu.Click += MnuLoadingOrderItem_Click;
            MnuLoadingOrders.DropDown.Items.Add(mnu);
        }

        MnuLoadingOrders.DropDown.Items.Add(new ToolStripSeparator());

        // add ImageOrderType items
        foreach (var orderType in Enum.GetValues(typeof(ImageOrderType)))
        {
            var typeName = Enum.GetName(typeof(ImageOrderType), orderType);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"_.{nameof(ImageOrderType)}._{typeName}"],
                Tag = new ModernMenuItemTag()
                {
                    SingleSelect = true,
                    ImageOrderType = (ImageOrderType)orderType,
                },
                CheckOnClick = true,
                Checked = (int)orderType == (int)Config.ImageLoadingOrderType,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
            };

            mnu.Click += MnuLoadingOrderTypeItem_Click;
            MnuLoadingOrders.DropDown.Items.Add(mnu);
        }
    }


    private void MnuLoadingOrderItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        if (mnu.Tag is ModernMenuItemTag tag
            && tag.ImageOrderBy != null
            && tag.ImageOrderBy.Value != Config.ImageLoadingOrder)
        {
            Config.ImageLoadingOrder = tag.ImageOrderBy.Value;

            // reload image list
            IG_ReloadList();

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }


    private void MnuLoadingOrderTypeItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        if (mnu.Tag is ModernMenuItemTag tag
            && tag.ImageOrderType != null
            && tag.ImageOrderType.Value != Config.ImageLoadingOrderType)
        {
            Config.ImageLoadingOrderType = tag.ImageOrderType.Value;

            // reload image list
            IG_ReloadList();

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }


    /// <summary>
    /// Load external tools to MnuTools.
    /// </summary>
    public void LoadExternalTools()
    {
        var builtinToolMenuNames = new List<string>()
        {
            nameof(MnuColorPicker),
            nameof(MnuCropTool),
            nameof(MnuFrameNav),
            nameof(MnuLosslessCompression),
            nameof(MnuExternalToolsSeparator),
            nameof(MnuGetMoreTools),
        };
        var allToolMenuNames = new List<string>(MnuTools.DropDownItems.Count);
        foreach (ToolStripItem item in MnuTools.DropDownItems)
        {
            allToolMenuNames.Add(item.Name);
        }

        // clear external tools
        foreach (var menuName in allToolMenuNames)
        {
            if (builtinToolMenuNames.Contains(menuName)) continue;

            MnuTools.DropDownItems[menuName].Click -= MnuExternalTool_Click;
            MnuTools.DropDownItems.RemoveByKey(menuName);
        }

        var mnuGetMoreToolsIndex = MnuTools.DropDownItems.IndexOf(MnuGetMoreTools);

        // add separator to separate built-in and external tools
        if (Config.Tools.Count > 0)
        {
            MnuTools.DropDownItems.Insert(mnuGetMoreToolsIndex, new ToolStripSeparator()
            {
                Name = "MnuExternalToolsSeparator_End",
            });
        }

        var newMenuIconHeight = this.ScaleToDpi(Const.MENU_ICON_HEIGHT);
        var i = 0;

        // add external tools
        foreach (var item in Config.Tools)
        {
            _ = Config.Language.TryGetValue($"_.Tools.{item.ToolId}", out var toolName);

            var mnu = new ToolStripMenuItem()
            {
                Name = item.ToolId,
                Text = toolName ?? item.ToolName ?? item.ToolId,
                CheckOnClick = item.IsIntegrated ?? false,
                Checked = false,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight),
                ShortcutKeyDisplayString = Config.GetHotkeyString(CurrentMenuHotkeys, item.ToolId),
            };

            mnu.Click += MnuExternalTool_Click;
            MnuTools.DropDownItems.Insert(mnuGetMoreToolsIndex + i, mnu);
            i++;
        }
    }


    private void MnuExternalTool_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem mnu) return;

        _ = OpenExternalToolAsync(mnu);
    }


    private async Task OpenExternalToolAsync(ToolStripMenuItem mnu)
    {
        if (Config.Tools.SingleOrDefault(i => i.ToolId.Equals(mnu.Name, StringComparison.Ordinal)) is not IgTool tool) return;


        var visible = !mnu.Checked;
        if (mnu.CheckOnClick)
        {
            visible = mnu.Checked;
        }

        if (visible)
        {
            try
            {
                _ = await Local.OpenPipedToolAsync(tool);
            }
            catch (FileNotFoundException)
            {
                using var frm = new FrmToolNotFound(tool.ToolId);
                var result = frm.ShowDialog(this);

                if (result != DialogResult.OK)
                {
                    if (mnu.CheckOnClick) mnu.Checked = !mnu.Checked;
                    return;
                }


                // fix the tool's path
                tool.Executable = frm.ExecutablePath;

                // run again
                _ = await Local.OpenPipedToolAsync(tool);
            }
        }
        else
        {
            _ = Local.ClosePipedToolAsync(tool);
        }
    }


    private void UpdateEditAppInfoForMenu()
    {
        var appName = string.Empty;
        MnuEdit.Image = null;

        // not clipboard image
        if (Local.ClipboardImage == null)
        {
            // Find file format
            var ext = Path.GetExtension(Local.Images.GetFilePath(Local.CurrentIndex)).ToLowerInvariant();

            if (Config.GetEditAppFromExtension(ext) is EditApp app)
            {
                appName = $"({app.AppName})";

                try
                {
                    // update menu icon
                    using var ico = Icon.ExtractAssociatedIcon(app.Executable);
                    var iconWidth = this.ScaleToDpi(Const.MENU_ICON_HEIGHT);

                    MnuEdit.Image = new Bitmap(ico?.ToBitmap(), iconWidth, iconWidth);
                }
                catch { }
            }
            else if (BHelper.IsOS(WindowsOS.Win11OrLater))
            {
                appName = "(MS Paint)";
            }
        }

        MnuEdit.Text = string.Format(Config.Language[$"{Name}.{nameof(MnuEdit)}"], appName);
    }


    /// <summary>
    /// Makes app layout changes according to <see cref="Config.Layout"/>.
    /// </summary>
    private void LoadAppLayout(bool forcedUpdateLayout = false)
    {
        const string SEPARATOR = ";";
        var dict = new Dictionary<Control, (DockStyle Dock, int DockOrder)>()
        {
            { Toolbar, (DockStyle.Top, 0) },
            { Gallery, (DockStyle.Bottom, 0) },
            { ToolbarContext, (DockStyle.Bottom, 1) },
        };


        // load layout setting
        SuspendLayout();
        foreach (var (control, info) in dict)
        {
            var (dockStyle, dockOrder) = info;

            if (Config.Layout.TryGetValue(control.Name, out var layoutStr))
            {
                // Bottom;0
                var options = layoutStr?.Split(SEPARATOR,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (options?.Length > 1)
                {
                    _ = int.TryParse(options[1], out dockOrder);
                }

                if (options?.Length > 0)
                {
                    dockStyle = BHelper.ConvertType<DockStyle>(options[0], dockStyle);
                }

                dict[control] = (dockStyle, dockOrder);
            }

            // update control dock style
            control.Dock = dockStyle;
        }


        // update Gallery bar layout
        if (Gallery.Dock == DockStyle.Left || Gallery.Dock == DockStyle.Right)
        {
            Gallery.View = ImageGlass.Gallery.View.Thumbnails;
            Gallery.ScrollBars = true;
            Gallery.TooltipDirection = ImageGlass.Gallery.TooltipDirection.Bottom;

            Gallery.Resizer = Gallery.Dock == DockStyle.Left
                ? ResizerType.HTRIGHT
                : ResizerType.HTLEFT;

            if (Gallery.Dock == DockStyle.Left)
            {
                Gallery.Resizer = ResizerType.HTRIGHT;
                PicMain.Padding = new Padding();
            }
            else
            {
                Gallery.Resizer = ResizerType.HTLEFT;
                PicMain.Padding = this.ScaleToDpi(new Padding(0, 0, 2, 0));
            }
        }
        else
        {
            Gallery.View = ImageGlass.Gallery.View.HorizontalStrip;
            Gallery.ScrollBars = Config.ShowGalleryScrollbars;

            Gallery.Resizer = ResizerType.None;
            Gallery.TooltipDirection = Gallery.Dock == DockStyle.Bottom
                ? ImageGlass.Gallery.TooltipDirection.Top
                : ImageGlass.Gallery.TooltipDirection.Bottom;

            PicMain.Padding = new Padding();
        }
        UpdateGallerySize();


        // update docking order
        var orderedDict = dict.OrderBy(i => i.Value.Dock)
            .ThenByDescending(i => i.Value.DockOrder);

        foreach (var (control, _) in orderedDict)
        {
            control.SendToBack();
        }


        // make sure Gallery does not cover toolbar in vertical layout
        if (Gallery.Dock == DockStyle.Left || Gallery.Dock == DockStyle.Right)
        {
            Gallery.BringToFront();
        }

        // make sure PicMain always on top
        PicMain.BringToFront();
        ResumeLayout(false);

        // perform layout update
        if (forcedUpdateLayout)
        {
            PerformLayout();
            PicMain.Focus();
        }
    }


    /// <summary>
    /// Loads the default mouse click actions.
    /// </summary>
    public void LoadDefaultMouseClickActions(bool forced = false)
    {
        if (forced || Config.MouseClickActions.Count == 0)
        {
            Config.MouseClickActions.Add(MouseClickEvent.LeftDoubleClick,
                new ToggleAction(new(nameof(IG_AutoSetActualSize))));

            Config.MouseClickActions.Add(MouseClickEvent.WheelClick,
                new ToggleAction(new(nameof(IG_Refresh))));

            Config.MouseClickActions.Add(MouseClickEvent.XButton1Click,
                new ToggleAction(new(nameof(IG_ViewPreviousImage))));

            Config.MouseClickActions.Add(MouseClickEvent.XButton2Click,
                new ToggleAction(new(nameof(IG_ViewNextImage))));
        }
    }


    /// <summary>
    /// Loads the default mouse wheel actions.
    /// </summary>
    public static void LoadDefaultMouseWheelActions(bool forced = false)
    {
        if (forced || Config.MouseWheelActions.Count == 0)
        {
            Config.MouseWheelActions.Add(MouseWheelEvent.Scroll, MouseWheelAction.Zoom);
            Config.MouseWheelActions.Add(MouseWheelEvent.CtrlAndScroll, MouseWheelAction.PanVertically);
            Config.MouseWheelActions.Add(MouseWheelEvent.ShiftAndScroll, MouseWheelAction.PanHorizontally);
            Config.MouseWheelActions.Add(MouseWheelEvent.AltAndScroll, MouseWheelAction.BrowseImages);
        }
    }


}

