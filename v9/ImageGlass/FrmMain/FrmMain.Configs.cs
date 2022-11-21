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

using ImageGlass.Base;
using ImageGlass.Base.Actions;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Reflection;

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
        { nameof(MnuStartStopAnimating),    new() { new (Keys.Control | Keys.Space) } },
        { nameof(MnuExtractFrames),         new() { new (Keys.Control | Keys.J) } },
        { nameof(MnuOpenLocation),          new() { new (Keys.L) } },
        { nameof(MnuImageProperties),       new() { new (Keys.Control | Keys.I) } },

        // MnuClipboard
        { nameof(MnuCopyImageData),         new() { new (Keys.Control | Keys.C) } },
        { nameof(MnuCopy),                  new() { new (Keys.Control | Keys.Shift | Keys.C) } },
        { nameof(MnuCut),                   new() { new (Keys.Control | Keys.X) } },
        { nameof(MnuCopyPath),              new() { new (Keys.Control | Keys.L) } },
        { nameof(MnuClearClipboard),        new() { new (Keys.Control | Keys.Oemtilde) } }, // Ctrl+`

        { nameof(MnuWindowFit),             new() { new (Keys.F9) } },
        { nameof(MnuFrameless),             new() { new (Keys.F10) } },
        { nameof(MnuFullScreen),            new() { new (Keys.F11) } },

        // MnuSlideshow
        { nameof(MnuStartSlideshow),        new() { new (Keys.F12) } },
        { nameof(MnuCloseAllSlideshows),    new() { new (Keys.Control | Keys.F12) } },

        // MnuLayout
        { nameof(MnuToggleToolbar),         new() { new (Keys.T) } },
        { nameof(MnuToggleThumbnails),      new() { new (Keys.H) } },
        { nameof(MnuToggleCheckerboard),    new() { new (Keys.B) } },
        { nameof(MnuToggleTopMost),         new() { new (Keys.Oemtilde) } }, // `

        // MnuTools
        { nameof(MnuColorPicker),           new() { new (Keys.K) } },
        { nameof(MnuCropping),              new() { new (Keys.C) } },
        { nameof(MnuPageNav),               new() { new (Keys.P) } },
        { nameof(MnuExifTool),              new() { new (Keys.X) } },

        // MnuHelp
        { nameof(MnuAbout),                 new() { new (Keys.F1) } },

        { nameof(MnuSettings),              new() { new (Keys.Control | Keys.Oemcomma) } }, // Ctrl+,
        { nameof(MnuExit),                  new() { new (Keys.Escape) } },
    };


    private void SetUpFrmMainConfigs()
    {
        SuspendLayout();
        Sp1.TabStop = false;
        Sp2.Panel2Collapsed = true;
        Sp2.TabStop = false;


        // Toolbar
        Toolbar.Alignment = Config.CenterToolbar
            ? ToolbarAlignment.Center
            : ToolbarAlignment.Left;
        
        Toolbar.Items.Clear();
        Toolbar.AddItems(Config.ToolbarItems);
        IG_ToggleToolbar(Config.ShowToolbar);


        // Thumbnail bar
        Gallery.DoNotDeletePersistentCache = true;
        Gallery.PersistentCacheSize = 200;
        Gallery.PersistentCacheDirectory = App.ConfigDir(PathType.Dir, Dir.ThumbnailsCache);
        Gallery.EnableKeyNavigation = false;
        IG_ToggleGallery(Config.ShowThumbnails);


        // PicMain
        PicMain.EnableSelection = true;
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

        ResumeLayout(false);

        Load += FrmMainConfig_Load;
        FormClosing += FrmMainConfig_FormClosing;
        SizeChanged += FrmMainConfig_SizeChanged;
    }

    private void FrmMainConfig_Load(object? sender, EventArgs e)
    {
        Local.OnRequestUpdateFrmMain += Local_OnFrmMainUpdateRequested;

        // IsWindowAlwaysOnTop
        IG_ToggleTopMost(Config.EnableWindowTopMost, showInAppMessage: false);

        // load language pack
        Local.UpdateFrmMain(UpdateRequests.Language);

        // load menu hotkeys
        Config.MergeHotkeys(ref CurrentMenuHotkeys, Config.MenuHotkeys);
        Local.UpdateFrmMain(UpdateRequests.MenuHotkeys);

        // TODO: hide menu items that haven't implemented
        HideUnreadyMenuItems();


        // make sure all controls are painted before showing window
        Application.DoEvents();


        // load Full screen mode
        if (Config.EnableFullScreen)
        {
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
        }

        // start slideshow
        if (Config.EnableSlideshow)
        {
            IG_StartNewSlideshow();
        }

        // load context menu config
        Local.UpdateFrmMain(UpdateRequests.MouseActions);


        // make sure all other painting are done before showing the root layout
        Application.DoEvents();

        // display the root layout after the window shown
        Tb0.Visible = true;

    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _ = SaveConfigsOnClosing();
    }

    private async Task SaveConfigsOnClosing()
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

        // slideshow
        var serverCount = Local.SlideshowPipeServers.Count(s => s != null);
        Config.EnableSlideshow = serverCount > 0;


        await Config.WriteAsync();


        // cleaning
        try
        {
            // disconnect all slideshows
            FrmMain.DisconnectAllSlideshowServers();

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
    }


    /// <summary>
    /// Processes internal update requests
    /// </summary>
    private void Local_OnFrmMainUpdateRequested(UpdateRequests e)
    {
        if (e.HasFlag(UpdateRequests.Language))
        {
            LoadLanguage();
        }
        
        if (e.HasFlag(UpdateRequests.MenuHotkeys))
        {
            LoadMenuHotkeys();
            LoadToolbarItemsText();
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
    private void UpdateToolbarItemsState()
    {
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

                // get config name
                var configProp = Config.GetProp(tagModel.CheckableConfigBinding);
                if (configProp is null) continue;

                // get config value
                var propValue = configProp.GetValue(null);
                if (propValue is null) continue;

                // load check state:
                // Executable is menu item
                if (tagModel.OnClick.Executable.StartsWith("Mnu"))
                {
                    var field = GetType().GetField(tagModel.OnClick.Executable, BindingFlags.Instance | BindingFlags.NonPublic);
                    var mnu = field?.GetValue(this) as ToolStripMenuItem;

                    if (mnu is not null)
                    {
                        tItem.Checked = mnu.Checked;
                    }
                }
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
    }


    /// <summary>
    /// Load language.
    /// </summary>
    private void LoadLanguage()
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
        MnuScaleToFit.Text = lang[$"{Name}.{nameof(MnuScaleToFit)}"];
        MnuScaleToFill.Text = lang[$"{Name}.{nameof(MnuScaleToFill)}"];

        MnuActualSize.Text = lang[$"{Name}.{nameof(MnuActualSize)}"];
        MnuLockZoom.Text = lang[$"{Name}.{nameof(MnuLockZoom)}"];
        MnuAutoZoom.Text = lang[$"{Name}.{nameof(MnuAutoZoom)}"];
        MnuScaleToWidth.Text = lang[$"{Name}.{nameof(MnuScaleToWidth)}"];
        MnuScaleToHeight.Text = lang[$"{Name}.{nameof(MnuScaleToHeight)}"];
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
        MnuExtractFrames.Text = lang[$"{Name}.{nameof(MnuExtractFrames)}"];
        MnuStartStopAnimating.Text = lang[$"{Name}.{nameof(MnuStartStopAnimating)}"];
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

        MnuCopy.Text = lang[$"{Name}.{nameof(MnuCopy)}"];
        MnuCopyImageData.Text = lang[$"{Name}.{nameof(MnuCopyImageData)}"];
        MnuCut.Text = lang[$"{Name}.{nameof(MnuCut)}"];
        MnuCopyPath.Text = lang[$"{Name}.{nameof(MnuCopyPath)}"];
        MnuClearClipboard.Text = lang[$"{Name}.{nameof(MnuClearClipboard)}"];
        #endregion


        // Window modes & Menu Slideshow
        #region Window modes
        MnuWindowFit.Text = lang[$"{Name}.{nameof(MnuWindowFit)}"];
        MnuFullScreen.Text = lang[$"{Name}.{nameof(MnuFullScreen)}"];
        MnuFrameless.Text = lang[$"{Name}.{nameof(MnuFrameless)}"];

        // Menu Slideshow
        #region Menu Slideshow
        MnuSlideshow.Text = lang[$"{Name}.{nameof(MnuSlideshow)}"];

        MnuStartSlideshow.Text = lang[$"{Name}.{nameof(MnuStartSlideshow)}"];
        MnuCloseAllSlideshows.Text = lang[$"{Name}.{nameof(MnuCloseAllSlideshows)}"];
        #endregion

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
        MnuCropping.Text = lang[$"{Name}.{nameof(MnuCropping)}"];
        MnuExifTool.Text = lang[$"{Name}.{nameof(MnuExifTool)}"];
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
        LoadToolbarItemsText();

    }


    /// <summary>
    /// Load hotkeys of menu
    /// </summary>
    /// <param name="menu"></param>
    private void LoadMenuHotkeys(ToolStripDropDown? menu = null)
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
    private void LoadToolbarItemsText()
    {
        foreach (var item in Toolbar.Items)
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

        var newMenuIconHeight = DpiApi.Transform(Constants.MENU_ICON_HEIGHT);

        // add new items
        foreach (var channel in Enum.GetValues(typeof(ColorChannel)))
        {
            var channelName = Enum.GetName(typeof(ColorChannel), channel);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"{Name}.{nameof(MnuViewChannels)}._{channelName}"],
                Tag = channel,
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

        var selectedChannel = (ColorChannel)(int)mnu.Tag;

        if (selectedChannel != Local.ImageChannel)
        {
            Local.ImageChannel = selectedChannel;
            Local.Images.ImageChannel = selectedChannel;

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

        var newMenuIconHeight = DpiApi.Transform(Constants.MENU_ICON_HEIGHT);

        // add ImageOrderBy items
        foreach (var order in Enum.GetValues(typeof(ImageOrderBy)))
        {
            var orderName = Enum.GetName(typeof(ImageOrderBy), order);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"_.{nameof(ImageOrderBy)}._{orderName}"],
                Tag = order,
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
                Tag = orderType,
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

        if (mnu.Tag is ImageOrderBy selectedOrder
            && selectedOrder != Config.ImageLoadingOrder)
        {
            Config.ImageLoadingOrder = selectedOrder;

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

        if (mnu.Tag is ImageOrderType selectedType
            && selectedType != Config.ImageLoadingOrderType)
        {
            Config.ImageLoadingOrderType = selectedType;

            // reload image list
            IG_ReloadList();

            // reload the state
            LoadMnuLoadingOrdersSubItems();
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
        MnuEdit.Visible = false;
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
        toolStripMenuItem14.Visible = false;
        MnuViewNextFrame.Visible = false;
        MnuViewPreviousFrame.Visible = false;
        MnuViewFirstFrame.Visible = false;
        MnuViewLastFrame.Visible = false;

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
        MnuRotateLeft.Visible = false;
        MnuRotateRight.Visible = false;
        //MnuFlipHorizontal.Visible = false;
        //MnuFlipVertical.Visible = false;
        //toolStripMenuItem17.Visible = false;
        //MnuRename.Visible = false;
        //MnuMoveToRecycleBin.Visible = false;
        //MnuDeleteFromHardDisk.Visible = false;
        MnuStartStopAnimating.Visible = false;
        MnuExtractFrames.Visible = false;
        //MnuSetDesktopBackground.Visible = false;
        //MnuSetLockScreen.Visible = false;
        //MnuOpenLocation.Visible = false;
        //MnuImageProperties.Visible = false;

        // MnuClipboard
        //MnuPasteImage.Visible = false;
        //MnuCopyImageData.Visible = false;
        //MnuCopyPath.Visible = false;
        //MnuCopy.Visible = false;
        //MnuCut.Visible = false;
        //MnuClearClipboard.Visible = false;

        //toolStripMenuItem6.Visible = false;
        MnuWindowFit.Visible = false;
        MnuFrameless.Visible = false;
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
        MnuTools.Visible = false;
        MnuColorPicker.Visible = false;
        MnuCropping.Visible = false;
        MnuPageNav.Visible = false;
        MnuExifTool.Visible = false;

        // MnuHelp
        //MnuAbout.Visible = false;
        //MnuCheckForUpdate.Visible = false;
        //MnuReportIssue.Visible = false;
        MnuFirstLaunch.Visible = false;

        //MnuSettings.Visible = false;
        //MnuExit.Visible = false;
    }

}

