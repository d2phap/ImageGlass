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
    public static Dictionary<string, Hotkey> MenuHotkeys = new()
    {
	    // Open main menu
	    { nameof(MnuMain),                  new(Keys.Alt | Keys.F) },

	    // MnuFile
	    { nameof(MnuOpenFile),              new(Keys.Control | Keys.O) },
        { nameof(MnuOpenImageData),         new(Keys.Control | Keys.V) },
        { nameof(MnuNewWindow),             new(Keys.Control | Keys.N) },
        { nameof(MnuSave),                  new(Keys.Control | Keys.S) },
        { nameof(MnuSaveAs),                new(Keys.Control | Keys.Shift | Keys.S) },
        { nameof(MnuOpenWith),              new(Keys.D) },
        { nameof(MnuEdit),                  new(Keys.E) },
        { nameof(MnuPrint),                 new(Keys.Control | Keys.P) },
        { nameof(MnuRefresh),               new(Keys.R) },
        { nameof(MnuReload),                new(Keys.Control | Keys.R) },
        { nameof(MnuReloadImageList),       new(Keys.Control | Keys.Shift | Keys.R) },

        // MnuNavigation
        { nameof(MnuViewNext),              new(Keys.Right) },
        { nameof(MnuViewPrevious),          new(Keys.Left) },
        { nameof(MnuGoTo),                  new(Keys.G) },
        { nameof(MnuGoToFirst),             new(Keys.Home) },
        { nameof(MnuGoToLast),              new(Keys.End) },
        { nameof(MnuViewNextFrame),         new(Keys.Control | Keys.Right) },
        { nameof(MnuViewPreviousFrame),     new(Keys.Control | Keys.Left) },
        { nameof(MnuViewFirstFrame),        new(Keys.Control | Keys.Home) },
        { nameof(MnuViewLastFrame),         new(Keys.Control | Keys.End) },

        // MnuZoom
        { nameof(MnuZoomIn),                new(Keys.Oemplus) }, // =
        { nameof(MnuZoomOut),               new(Keys.OemMinus) }, // -
        { nameof(MnuCustomZoom),            new(Keys.Z) },
        { nameof(MnuActualSize),            new(Keys.D0) },
        { nameof(MnuAutoZoom),              new(Keys.D1) },
        { nameof(MnuLockZoom),              new(Keys.D2) },
        { nameof(MnuScaleToWidth),          new(Keys.D3) },
        { nameof(MnuScaleToHeight),         new(Keys.D4) },
        { nameof(MnuScaleToFit),            new(Keys.D5) },
        { nameof(MnuScaleToFill),           new(Keys.D6) },

        // MnuImage
        { nameof(MnuViewChannels),          new(Keys.Shift | Keys.C) },
        { nameof(MnuLoadingOrders),         new(Keys.Shift | Keys.O) },
        { nameof(MnuRotateLeft),            new(Keys.Control | Keys.OemPeriod) }, // Ctrl+.
        { nameof(MnuRotateRight),           new(Keys.Control | Keys.OemQuestion) }, // Ctrl+/
        { nameof(MnuFlipHorizontal),        new(Keys.Control | Keys.Oem1) }, // Ctrl+;
        { nameof(MnuFlipVertical),          new(Keys.Control | Keys.Oem7) }, // Ctrl+'
        { nameof(MnuRename),                new(Keys.F2) },
        { nameof(MnuMoveToRecycleBin),      new(Keys.Delete) },
        { nameof(MnuDeleteFromHardDisk),    new(Keys.Shift | Keys.Delete) },
        { nameof(MnuStartStopAnimating),    new(Keys.Control | Keys.Space) },
        { nameof(MnuExtractFrames),         new(Keys.Control | Keys.J) },
        { nameof(MnuOpenLocation),          new(Keys.L) },
        { nameof(MnuImageProperties),       new(Keys.Control | Keys.I) },

        // MnuClipboard
        { nameof(MnuCopyImageData),         new(Keys.Control | Keys.C) },
        { nameof(MnuCopy),                  new(Keys.Control | Keys.Shift | Keys.C) },
        { nameof(MnuCut),                   new(Keys.Control | Keys.X) },
        { nameof(MnuCopyPath),              new(Keys.Control | Keys.L) },
        { nameof(MnuClearClipboard),        new(Keys.Control | Keys.Oemtilde) }, // Ctrl+`

        { nameof(MnuWindowFit),             new(Keys.F9) },
        { nameof(MnuFrameless),             new(Keys.F10) },
        { nameof(MnuFullScreen),            new(Keys.F11) },

        // MnuSlideshow
        { nameof(MnuStartSlideshow),        new(Keys.F12) },
        { nameof(MnuPauseResumeSlideshow),  new(Keys.Space) },
        { nameof(MnuExitSlideshow),         new(Keys.Escape) },

        // MnuLayout
        { nameof(MnuToggleToolbar),         new(Keys.T) },
        { nameof(MnuToggleThumbnails),      new(Keys.H) },
        { nameof(MnuToggleCheckerboard),    new(Keys.B) },
        { nameof(MnuToggleTopMost),         new(Keys.Oemtilde) }, // `

        // MnuTools
        { nameof(MnuColorPicker),           new(Keys.K) },
        { nameof(MnuCropping),              new(Keys.C) },
        { nameof(MnuPageNav),               new(Keys.P) },
        { nameof(MnuExifTool),              new(Keys.X) },

        // MnuHelp
        { nameof(MnuAbout),                 new(Keys.F1) },

        { nameof(MnuSettings),              new(Keys.Control | Keys.Oemcomma) }, // Ctrl+,
        { nameof(MnuExit),                  new(Keys.Escape) },
    };

    private void SetUpFrmMainConfigs()
    {
        // Toolbar
        Toolbar.IconHeight = Config.ToolbarIconHeight;
        IG_ToggleToolbar(Config.IsShowToolbar);
        Toolbar.Items.Clear();
        Toolbar.AddItems(Config.ToolbarItems);


        // Thumbnail bar
        Gallery.Codec = Config.Codec;
        Gallery.DoNotDeletePersistentCache = true;
        Gallery.PersistentCacheSize = 100;
        Gallery.PersistentCacheDirectory = App.ConfigDir(PathType.Dir, Dir.ThumbnailsCache);
        IG_ToggleGallery(Config.IsShowThumbnail);


        Sp2.Panel1Collapsed = true;
        Sp3.Panel2Collapsed = true;


        // PicBox
        IG_SetZoomMode(Config.ZoomMode.ToString());
        IG_ToggleCheckerboard(Config.IsShowCheckerBoard);


        Load += FrmMainConfig_Load;
        FormClosing += FrmMainConfig_FormClosing;
        SizeChanged += FrmMainConfig_SizeChanged;
    }

    private void FrmMainConfig_Load(object? sender, EventArgs e)
    {
        Local.OnRequestUpdateFrmMain += Local_OnFrmMainUpdateRequested;

        // load window placement from settings
        WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

        // IsWindowAlwaysOnTop
        IG_ToggleTopMost(Config.IsWindowAlwaysOnTop);

        // load language pack
        Local.UpdateFrmMain(ForceUpdateAction.Language);

        // load menu hotkeys
        Config.MergeHotkeys(ref MenuHotkeys, Config.MenuHotkeysOverride);
        Local.UpdateFrmMain(ForceUpdateAction.MenuHotkeys);


        // TODO: hide menu items that haven't implemented
        HideUnreadyMenuItems();
    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // save FrmMain placement
        var wp = WindowSettings.GetPlacementFromWindow(this);
        WindowSettings.SetFrmMainPlacementConfig(wp);


        Config.Write();
    }

    private void FrmMainConfig_SizeChanged(object? sender, EventArgs e)
    {
        Config.FrmMainPositionX = Location.X;
        Config.FrmMainPositionY = Location.Y;
        Config.FrmMainWidth = Size.Width;
        Config.FrmMainHeight = Size.Height;
    }


    /// <summary>
    /// Processes internal update requests
    /// </summary>
    /// <param name="e"></param>
    private void Local_OnFrmMainUpdateRequested(ForceUpdateAction e)
    {
        if (e.HasFlag(ForceUpdateAction.Language))
        {
            LoadLanguages();
        }
        
        if (e.HasFlag(ForceUpdateAction.MenuHotkeys))
        {
            LoadMenuHotkeys();
            LoadToolbarItemsText();
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
    /// Load <see cref="MnuMain"/> language.
    /// </summary>
    private void LoadLanguages()
    {
        var lang = Config.Language;

        #region Main menu

        // Menu File
        #region Menu File
        MnuFile.Text = lang[$"{Name}.{nameof(MnuFile)}"];

        MnuOpenFile.Text = lang[$"{Name}.{nameof(MnuOpenFile)}"];
        MnuOpenImageData.Text = lang[$"{Name}.{nameof(MnuOpenImageData)}"];
        MnuNewWindow.Text = lang[$"{Name}.{nameof(MnuNewWindow)}"];
        MnuSave.Text = lang[$"{Name}.{nameof(MnuSave)}"];
        MnuSaveAs.Text = lang[$"{Name}.{nameof(MnuSaveAs)}"];

        MnuOpenWith.Text = lang[$"{Name}.{nameof(MnuOpenWith)}"];
        MnuEdit.Text = lang[$"{Name}.{nameof(MnuEdit)}"];
        MnuPrint.Text = lang[$"{Name}.{nameof(MnuPrint)}"];

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
        #endregion


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
        #endregion


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
        #region Menu CLipboard
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
        MnuPauseResumeSlideshow.Text = lang[$"{Name}.{nameof(MnuPauseResumeSlideshow)}"];
        MnuExitSlideshow.Text = lang[$"{Name}.{nameof(MnuExitSlideshow)}"];
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
            item.ShortcutKeyDisplayString = Config.GetHotkeyString(MenuHotkeys, item.Name);

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

                    var hotkey = Config.GetHotkeyString(MenuHotkeys, tagModel.OnClick.Executable);
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

        var selectedOrder = (ImageOrderBy)(int)mnu.Tag;

        if (selectedOrder != Config.ImageLoadingOrder)
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

        var selectedType = (ImageOrderType)(int)mnu.Tag;

        if (selectedType != Config.ImageLoadingOrderType)
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
        MnuOpenImageData.Visible = false;
        MnuNewWindow.Visible = false;
        MnuSave.Visible = false;
        MnuSaveAs.Visible = false;
        //MnuOpenWith.Visible = false;
        MnuEdit.Visible = false;
        //MnuPrint.Visible = false;
        //MnuRefresh.Visible = false;
        //MnuReload.Visible = false;
        //MnuReloadImageList.Visible = false;

        // MnuNavigation
        //MnuViewNext.Visible = false;
        //MnuViewPrevious.Visible = false;
        MnuGoTo.Visible = false;
        //MnuGoToFirst.Visible = false;
        //MnuGoToLast.Visible = false;
        MnuViewNextFrame.Visible = false;
        MnuViewPreviousFrame.Visible = false;
        MnuViewFirstFrame.Visible = false;
        MnuViewLastFrame.Visible = false;

        // MnuZoom
        //MnuZoomIn.Visible = false;
        //MnuZoomOut.Visible = false;
        MnuCustomZoom.Visible = false;
        //MnuActualSize.Visible = false;
        //MnuAutoZoom.Visible = false;
        //MnuLockZoom.Visible = false;
        //MnuScaleToWidth.Visible = false;
        //MnuScaleToHeight.Visible = false;
        //MnuScaleToFit.Visible = false;
        //MnuScaleToFill.Visible = false;

        // MnuImage
        MnuViewChannels.Visible = false;
        //MnuLoadingOrders.Visible = false;
        MnuRotateLeft.Visible = false;
        MnuRotateRight.Visible = false;
        MnuFlipHorizontal.Visible = false;
        MnuFlipVertical.Visible = false;
        MnuRename.Visible = false;
        MnuMoveToRecycleBin.Visible = false;
        MnuDeleteFromHardDisk.Visible = false;
        MnuStartStopAnimating.Visible = false;
        MnuExtractFrames.Visible = false;
        MnuSetDesktopBackground.Visible = false;
        MnuSetLockScreen.Visible = false;
        //MnuOpenLocation.Visible = false;
        //MnuImageProperties.Visible = false;

        // MnuClipboard
        //MnuCopyImageData.Visible = false;
        //MnuCopy.Visible = false;
        //MnuCut.Visible = false;
        //MnuCopyPath.Visible = false;
        //MnuClearClipboard.Visible = false;

        MnuWindowFit.Visible = false;
        MnuFrameless.Visible = false;
        MnuFullScreen.Visible = false;

        // MnuSlideshow
        MnuSlideshow.Visible = false;
        MnuStartSlideshow.Visible = false;
        MnuPauseResumeSlideshow.Visible = false;
        MnuExitSlideshow.Visible = false;

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
        MnuCheckForUpdate.Visible = false;
        //MnuReportIssue.Visible = false;
        MnuFirstLaunch.Visible = false;

        //MnuSettings.Visible = false;
        //MnuExit.Visible = false;
    }
}

