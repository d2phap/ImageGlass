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

using ImageGlass.Base;
using ImageGlass.Base.Actions;
using ImageGlass.Base.PhotoBox;
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
    public static Dictionary<string, List<Hotkey>> CurrentMenuHotkeys = new()
    {
	    // Open main menu
	    { nameof(MnuMain),                  new() { new(Keys.Alt | Keys.F) } },

	    // MnuFile
	    { nameof(MnuOpenFile),              new() { new (Keys.Control | Keys.O) } },
        { nameof(MnuPasteImage),            new() { new (Keys.Control | Keys.V) } },
        { nameof(MnuNewWindow),             new() { new (Keys.Control | Keys.N) } },
        { nameof(MnuSave),                  new() { new (Keys.Control | Keys.S) } },
        { nameof(MnuSaveAs),                new() { new (Keys.Control | Keys.Shift | Keys.S) } },
        { nameof(MnuOpenWith),              new() { new (Keys.D) } },
        { nameof(MnuEdit),                  new() { new (Keys.E) } },
        { nameof(MnuPrint),                 new() { new (Keys.Control | Keys.P) } },
        { nameof(MnuShare),                 new() { new (Keys.S) } },
        { nameof(MnuRefresh),               new() { new (Keys.R) } },
        { nameof(MnuReload),                new() { new (Keys.Control | Keys.R) } },
        { nameof(MnuReloadImageList),       new() { new (Keys.Control | Keys.Shift | Keys.R) } },
        { nameof(MnuUnload),                new() { new (Keys.U) } },

        // MnuNavigation
        { nameof(MnuViewNext),              new() { new (Keys.Right) } },
        { nameof(MnuViewPrevious),          new() { new (Keys.Left) } },
        { nameof(MnuGoTo),                  new() { new (Keys.G) } },
        { nameof(MnuGoToFirst),             new() { new (Keys.Home) } },
        { nameof(MnuGoToLast),              new() { new (Keys.End) } },
        { nameof(MnuViewNextFrame),         new() { new (Keys.Control | Keys.Right) } },
        { nameof(MnuViewPreviousFrame),     new() { new (Keys.Control | Keys.Left) } },
        { nameof(MnuViewFirstFrame),        new() { new (Keys.Control | Keys.Home) } },
        { nameof(MnuViewLastFrame),         new() { new (Keys.Control | Keys.End) } },

        // MnuPanning
        { nameof(MnuPanLeft),               new() { new (Keys.Alt | Keys.Left) } },
        { nameof(MnuPanRight),              new() { new (Keys.Alt | Keys.Right) } },
        { nameof(MnuPanUp),                 new() { new (Keys.Alt | Keys.Up) } },
        { nameof(MnuPanDown),               new() { new (Keys.Alt | Keys.Down) } },

        { nameof(MnuPanToLeftSide),         new() { new (Keys.Control | Keys.Alt | Keys.Left) } },
        { nameof(MnuPanToRightSide),        new() { new (Keys.Control | Keys.Alt | Keys.Right) } },
        { nameof(MnuPanToTop),              new() { new (Keys.Control | Keys.Alt | Keys.Up) } },
        { nameof(MnuPanToBottom),           new() { new (Keys.Control | Keys.Alt | Keys.Down) } },

        // MnuZoom
        { nameof(MnuZoomIn),                new() { new (Keys.Oemplus) } }, // =
        { nameof(MnuZoomOut),               new() { new (Keys.OemMinus) } }, // -
        { nameof(MnuCustomZoom),            new() { new (Keys.Z) } },
        { nameof(MnuActualSize),            new() { new (Keys.D0), new(Keys.NumPad0) } },
        { nameof(MnuAutoZoom),              new() { new (Keys.D1), new(Keys.NumPad1) } },
        { nameof(MnuLockZoom),              new() { new (Keys.D2), new (Keys.NumPad2) } },
        { nameof(MnuScaleToWidth),          new() { new (Keys.D3), new (Keys.NumPad3) } },
        { nameof(MnuScaleToHeight),         new() { new (Keys.D4), new (Keys.NumPad4) } },
        { nameof(MnuScaleToFit),            new() { new (Keys.D5), new (Keys.NumPad5) } },
        { nameof(MnuScaleToFill),           new() { new (Keys.D6), new (Keys.NumPad6) } },

        // MnuImage
        { nameof(MnuViewChannels),          new() { new (Keys.Shift | Keys.C) } },
        { nameof(MnuLoadingOrders),         new() { new (Keys.Shift | Keys.O) } },
        { nameof(MnuRotateLeft),            new() { new (Keys.Control | Keys.OemPeriod) } }, // Ctrl+.
        { nameof(MnuRotateRight),           new() { new (Keys.Control | Keys.OemQuestion) } }, // Ctrl+/
        { nameof(MnuFlipHorizontal),        new() { new (Keys.Control | Keys.Oem1) } }, // Ctrl+;
        { nameof(MnuFlipVertical),          new() { new (Keys.Control | Keys.Oem7) } }, // Ctrl+'
        { nameof(MnuRename),                new() { new (Keys.F2) } },
        { nameof(MnuMoveToRecycleBin),      new() { new (Keys.Delete) } },
        { nameof(MnuDeleteFromHardDisk),    new() { new (Keys.Shift | Keys.Delete) } },
        { nameof(MnuToggleImageAnimation),  new() { new (Keys.Control | Keys.Space) } },
        { nameof(MnuExportFrames),          new() { new (Keys.Control | Keys.J) } },
        { nameof(MnuOpenLocation),          new() { new (Keys.L) } },
        { nameof(MnuImageProperties),       new() { new (Keys.Control | Keys.I) } },

        // MnuClipboard
        { nameof(MnuCopyImageData),         new() { new (Keys.Control | Keys.C) } },
        { nameof(MnuCopyFile),              new() { new (Keys.Control | Keys.Shift | Keys.C) } },
        { nameof(MnuCutFile),               new() { new (Keys.Control | Keys.X) } },
        { nameof(MnuCopyPath),              new() { new (Keys.Control | Keys.L) } },
        { nameof(MnuClearClipboard),        new() { new (Keys.Control | Keys.Oemtilde) } }, // Ctrl+`

        { nameof(MnuWindowFit),             new() { new (Keys.F9) } },
        { nameof(MnuFrameless),             new() { new (Keys.F10) } },
        { nameof(MnuFullScreen),            new() { new (Keys.F11) } },
        { nameof(MnuSlideshow),             new() { new (Keys.F12) } },

        // MnuLayout
        { nameof(MnuToggleToolbar),         new() { new (Keys.T) } },
        { nameof(MnuToggleThumbnails),      new() { new (Keys.H) } },
        { nameof(MnuToggleCheckerboard),    new() { new (Keys.B) } },
        { nameof(MnuToggleTopMost),         new() { new (Keys.Oemtilde) } }, // `

        // MnuTools
        { nameof(MnuColorPicker),           new() { new (Keys.K) } },
        { nameof(MnuCropTool),              new() { new (Keys.C) } },
        { nameof(MnuPageNav),               new() { new (Keys.P) } },
        { Constants.IGTOOL_EXIFTOOL,        new() { new (Keys.X) } },

        // MnuHelp
        { nameof(MnuAbout),                 new() { new (Keys.F1) } },

        { nameof(MnuSettings),              new() { new (Keys.Control | Keys.Oemcomma) } }, // Ctrl+,
        { nameof(MnuExit),                  new() { new (Keys.Escape) } },
    };


    private void SetUpFrmMainConfigs()
    {
        SuspendLayout();

        // Toolbar
        Toolbar.Alignment = Config.EnableCenterToolbar
            ? ToolbarAlignment.Center
            : ToolbarAlignment.Left;

        Toolbar.ClearItems();
        Toolbar.AddItems(Config.ToolbarItems);


        // Thumbnail bar
        Gallery.DoNotDeletePersistentCache = true;
        Gallery.PersistentCacheSize = Config.ThumbnailCacheSizeInMb;
        Gallery.PersistentCacheDirectory = App.ConfigDir(PathType.Dir, Dir.ThumbnailsCache);
        Gallery.EnableKeyNavigation = false;


        // PicMain
        //PicMain.DebugMode = true;
        PicMain.NavDisplay = Config.EnableNavigationButtons
            ? NavButtonDisplay.Both
            : NavButtonDisplay.None;
        PicMain.TabStop = false;
        PicMain.PanDistance = Config.PanSpeed;
        PicMain.ZoomSpeed = Config.ZoomSpeed;
        PicMain.InterpolationScaleDown = Config.ImageInterpolationScaleDown;
        PicMain.InterpolationScaleUp = Config.ImageInterpolationScaleUp;
        IG_SetZoomMode(Config.ZoomMode.ToString());

        if (Config.ZoomMode == ZoomMode.LockZoom)
        {
            PicMain.ZoomFactor = Config.ZoomLockValue / 100f;
        }

        IG_ToggleCheckerboard(Config.ShowCheckerBoard);

        // set up layout
        ApplyAppLayout();

        ResumeLayout(false);

        Load += FrmMainConfig_Load;
        FormClosing += FrmMainConfig_FormClosing;
        SizeChanged += FrmMainConfig_SizeChanged;
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
        Local.RequestUpdateFrmMain += Local_FrmMainUpdateRequested;

        // IsWindowAlwaysOnTop
        IG_ToggleTopMost(Config.EnableWindowTopMost, showInAppMessage: false);

        // load language pack
        Local.UpdateFrmMain(UpdateRequests.Language);

        // load menu
        LoadExternalTools();
        Config.MergeHotkeys(ref CurrentMenuHotkeys, Config.MenuHotkeys);
        Local.UpdateFrmMain(UpdateRequests.MenuHotkeys);

        // TODO: hide menu items that haven't implemented
        HideUnreadyMenuItems();

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
        IG_ToggleGallery(Config.ShowThumbnails);


        // load Full screen mode
        if (Config.EnableFullScreen)
        {
            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit, false);

            // to hide the animation effect of window border
            FormBorderStyle = FormBorderStyle.None;

            // load window placement from settings here to save the initial
            // position of window so that when user exists the fullscreen mode,
            // it can be restore correctly
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

            IG_ToggleFullScreen(true, showInAppMessage: false);
        }
        else
        {
            // load window placement from settings
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit, false);
        }

        // start slideshow
        if (Config.EnableSlideshow)
        {
            IG_ToggleSlideshow();
        }

        // load context menu config
        Local.UpdateFrmMain(UpdateRequests.MouseActions);


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
    }


    public async Task SaveConfigsOnClosing()
    {
        // save FrmMain placement
        if (!Config.EnableFullScreen)
        {
            var wp = WindowSettings.GetPlacementFromWindow(this);
            WindowSettings.SetFrmMainPlacementConfig(wp);
        }

        // correct the settings when Full screen mode is enabled
        if (Config.EnableFullScreen)
        {
            Config.ShowToolbar = _showToolbar;
            Config.ShowThumbnails = _showThumbnails;
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


    /// <summary>
    /// Processes internal update requests
    /// </summary>
    private void Local_FrmMainUpdateRequested(UpdateRequests e)
    {
        if (e.HasFlag(UpdateRequests.Language))
        {
            LoadLanguage();
        }

        if (e.HasFlag(UpdateRequests.MenuHotkeys))
        {
            LoadMenuHotkeys();
            LoadToolbarItemsText(Toolbar);
        }

        if (e.HasFlag(UpdateRequests.MouseActions))
        {
            var executable = nameof(IG_OpenContextMenu);
            var hasRightClick = Config.MouseClickActions.ContainsKey(MouseClickEvent.RightClick);

            // get the executable value of the right click action
            if (hasRightClick)
            {
                var toggleAction = Config.MouseClickActions[MouseClickEvent.RightClick];
                var isToggled = ToggleAction.IsToggled(toggleAction.Id);
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
    }


    /// <summary>
    /// Updates items state of <see cref="Toolbar"/>
    /// </summary>
    private void UpdateToolbarItemsState(bool performLayout = true)
    {
        Toolbar.SuspendLayout();

        // Toolbar itoms
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
                if (tagModel.OnClick.Executable.StartsWith("Mnu"))
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
                    tItem.Checked = tagModel.OnClick.Argument.Equals(propValue.ToString());
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
        MnuToggleThumbnails.Text = lang[$"{Name}.{nameof(MnuToggleThumbnails)}"];
        MnuToggleCheckerboard.Text = lang[$"{Name}.{nameof(MnuToggleCheckerboard)}"];
        MnuToggleTopMost.Text = lang[$"{Name}.{nameof(MnuToggleTopMost)}"];
        #endregion


        // Menu Tools
        #region Menu Tools
        MnuTools.Text = lang[$"{Name}.{nameof(MnuTools)}"];

        MnuColorPicker.Text = lang[$"{Name}.{nameof(MnuColorPicker)}"];
        MnuPageNav.Text = lang[$"{Name}.{nameof(MnuPageNav)}"];
        MnuCropTool.Text = lang[$"{Name}.{nameof(MnuCropTool)}"];

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
        MnuFirstLaunch.Text = lang[$"{Name}.{nameof(MnuFirstLaunch)}"];
        MnuReportIssue.Text = lang[$"{Name}.{nameof(MnuReportIssue)}"];

        MnuSetDefaultPhotoViewer.Text = lang[$"{Name}.{nameof(MnuSetDefaultPhotoViewer)}"];
        MnuUnsetDefaultPhotoViewer.Text = lang[$"{Name}.{nameof(MnuUnsetDefaultPhotoViewer)}"];
        #endregion


        #endregion


        // Toolbar
        LoadToolbarItemsText(Toolbar);

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

                var langKey = $"{Name}.{tagModel.OnClick.Executable}";
                if (Config.Language.ContainsKey(langKey))
                {
                    tItem.Text = tItem.ToolTipText = Config.Language[langKey];

                    var hotkey = Config.GetHotkeyString(CurrentMenuHotkeys, tagModel.OnClick.Executable);
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

        var newMenuIconHeight = this.ScaleToDpi(Constants.MENU_ICON_HEIGHT);

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

        var newMenuIconHeight = this.ScaleToDpi(Constants.MENU_ICON_HEIGHT);

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
    private void LoadExternalTools()
    {
        // clear external tools
        foreach (ToolStripItem item in MnuTools.DropDownItems)
        {
            if (item.Name.Equals(nameof(MnuColorPicker))
                || item.Name.Equals(nameof(MnuCropTool))
                || item.Name.Equals(nameof(MnuPageNav))) continue;

            item.Click -= MnuExternalTool_Click;
            MnuTools.DropDownItems.Remove(item);
        }

        // add separator to separate built-in and external tools
        if (Config.Tools.Count > 0)
        {
            MnuTools.DropDownItems.Add(new ToolStripSeparator());
        }

        var newMenuIconHeight = this.ScaleToDpi(Constants.MENU_ICON_HEIGHT);

        // add external tools
        foreach (var item in Config.Tools)
        {
            _ = Config.Language.TryGetValue($"_.Tools.{item.ToolId}", out var toolName);

            var mnu = new ToolStripMenuItem()
            {
                Name = item.ToolId,
                Text = toolName ?? item.ToolName ?? item.ToolId,
                CheckOnClick = item.CanToggle ?? false,
                Checked = false,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight),
            };

            mnu.Click += MnuExternalTool_Click;
            MnuTools.DropDownItems.Add(mnu);
        }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "<Pending>")]
    private void MnuExternalTool_Click(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem mnu
            || Config.Tools.SingleOrDefault(i => i.ToolId.Equals(mnu.Name)) is not IgTool tool) return;


        // Tool not found
        if (tool.IsEmpty)
        {
            using var frm = new FrmToolNotFound(tool.ToolId);
            var result = frm.ShowDialog(this);

            if (result != DialogResult.OK)
            {
                if (mnu.CheckOnClick) mnu.Checked = !mnu.Checked;
                return;
            }

            tool.Executable = frm.ExecutablePath;
        }


        var visible = !mnu.Checked;
        if (mnu.CheckOnClick)
        {
            visible = mnu.Checked;
        }

        if (visible)
        {
            _ = Local.OpenPipedToolAsync(tool);
        }
        else
        {
            _ = Local.ClosePipedToolAsync(tool);
        }
    }


    private void HideUnreadyMenuItems()
    {
        // MnuFile
        //MnuOpenFile.Visible = false;
        MnuNewWindow.Visible = false;
        //MnuSave.Visible = false;
        //MnuSaveAs.Visible = false;
        //MnuOpenWith.Visible = false;
        //MnuEdit.Visible = false;
        //MnuPrint.Visible = false;
        // MnuPrint.Visible = false;
        //MnuRefresh.Visible = false;
        //MnuReload.Visible = false;
        //MnuReloadImageList.Visible = false;

        // MnuNavigation
        //MnuViewNext.Visible = false;
        //MnuViewPrevious.Visible = false;
        //MnuGoTo.Visible = false;
        //MnuGoToFirst.Visible = false;
        //MnuGoToLast.Visible = false;
        //toolStripMenuItem14.Visible = false;
        //MnuViewNextFrame.Visible = false;
        //MnuViewPreviousFrame.Visible = false;
        //MnuViewFirstFrame.Visible = false;
        //MnuViewLastFrame.Visible = false;

        // MnuZoom
        //MnuZoomIn.Visible = false;
        //MnuZoomOut.Visible = false;
        //MnuCustomZoom.Visible = false;
        //MnuActualSize.Visible = false;
        //MnuAutoZoom.Visible = false;
        //MnuLockZoom.Visible = false;
        //MnuScaleToWidth.Visible = false;
        //MnuScaleToHeight.Visible = false;
        //MnuScaleToFit.Visible = false;
        //MnuScaleToFill.Visible = false;

        // MnuImage
        //MnuViewChannels.Visible = false;
        //MnuLoadingOrders.Visible = false;
        //toolStripMenuItem16.Visible = false;
        //MnuRotateLeft.Visible = false;
        //MnuRotateRight.Visible = false;
        //MnuFlipHorizontal.Visible = false;
        //MnuFlipVertical.Visible = false;
        //toolStripMenuItem17.Visible = false;
        //MnuRename.Visible = false;
        //MnuMoveToRecycleBin.Visible = false;
        //MnuDeleteFromHardDisk.Visible = false;
        //MnuToggleImageAnimation.Visible = false;
        //MnuExportFrames.Visible = false;
        //MnuSetDesktopBackground.Visible = false;
        //MnuSetLockScreen.Visible = false;
        //MnuOpenLocation.Visible = false;
        //MnuImageProperties.Visible = false;

        // MnuClipboard
        //MnuPasteImage.Visible = false;
        //MnuCopyImageData.Visible = false;
        //MnuCopyPath.Visible = false;
        //MnuCopyFile.Visible = false;
        //MnuCutFile.Visible = false;
        //MnuClearClipboard.Visible = false;

        //toolStripMenuItem6.Visible = false;
        //MnuWindowFit.Visible = false;
        //MnuFrameless.Visible = false;
        //MnuFullScreen.Visible = false;

        // MnuSlideshow
        //MnuSlideshow.Visible = false;
        //MnuStartSlideshow.Visible = false;
        //MnuCloseAllSlideshows.Visible = false;

        // MnuLayout
        //MnuToggleToolbar.Visible = false;
        //MnuToggleThumbnails.Visible = false;
        //MnuToggleCheckerboard.Visible = false;
        //MnuToggleTopMost.Visible = false;

        // MnuTools
        //MnuTools.Visible = false;
        //MnuColorPicker.Visible = false;
        //MnuCropTool.Visible = false;
        //MnuPageNav.Visible = false;
        //MnuExifTool.Visible = false;

        // MnuHelp
        //MnuAbout.Visible = false;
        //MnuCheckForUpdate.Visible = false;
        //MnuReportIssue.Visible = false;
        MnuFirstLaunch.Visible = false;

        //MnuSettings.Visible = false;
        //MnuExit.Visible = false;
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

            if (Config.EditApps.TryGetValue(ext, out var app) && app != null)
            {
                appName = $"({app.AppName})";

                try
                {
                    // update menu icon
                    using var ico = Icon.ExtractAssociatedIcon(app.Executable);
                    var iconWidth = this.ScaleToDpi(Constants.MENU_ICON_HEIGHT);

                    MnuEdit.Image = new Bitmap(ico.ToBitmap(), iconWidth, iconWidth);
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
    private void ApplyAppLayout()
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
        }
        else
        {
            Gallery.ScrollBars = Config.ShowThumbnailScrollbars || Gallery.View == ImageGlass.Gallery.View.Thumbnails;
        }
        UpdateGallerySize();


        // update docking order
        var orderedDict = dict.OrderBy(i => i.Value.Dock)
            .ThenByDescending(i => i.Value.DockOrder);

        foreach (var (control, _) in orderedDict)
        {
            control.SendToBack();
        }

        // make sure PicMain always on top
        PicMain.BringToFront();
        ResumeLayout(false);

    }

}

