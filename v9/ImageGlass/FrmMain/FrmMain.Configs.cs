using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Settings;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Configs contains methods for dynamic binding   *
 * ****************************************************** */

public partial class FrmMain
{
    private void SetUpFrmMainConfigs()
    {
        // Toolbar
        Toolbar.Visible = Config.IsShowToolbar;
        Toolbar.IconHeight = Config.ToolbarIconHeight;
        LoadToolbarItemsConfig();

        // Thumbnail bar
        Gallery.Codec = Config.Codec;
        Gallery.DoNotDeletePersistentCache = true;
        Gallery.PersistentCacheSize = 100;
        Gallery.PersistentCacheDirectory = App.ConfigDir(PathType.Dir, Dir.ThumbnailsCache);
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;
        Sp1.SplitterDistance = 465;

        Sp2.Panel1Collapsed = true;
        Sp3.Panel2Collapsed = true;


        // PicBox
        PicMain.ZoomMode = Config.ZoomMode;
        PicMain.CheckerboardMode = Config.IsShowCheckerBoard
            ? (Config.IsShowCheckerboardOnlyImageRegion
                ? CheckerboardMode.Image
                : CheckerboardMode.Client)
            : CheckerboardMode.None;


        Load += FrmMainConfig_Load;
        FormClosing += FrmMainConfig_FormClosing;
        SizeChanged += FrmMainConfig_SizeChanged;
    }

    private void FrmMainConfig_SizeChanged(object? sender, EventArgs e)
    {
        Config.FrmMainPositionX = Location.X;
        Config.FrmMainPositionY = Location.Y;
        Config.FrmMainWidth = Size.Width;
        Config.FrmMainHeight = Size.Height;
    }

    private void FrmMainConfig_Load(object? sender, EventArgs e)
    {
        Local.OnRequestUpdateFrmMain += Local_OnFrmMainUpdateRequested;

        // load window placement from settings
        WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

        TopMost = Config.IsWindowAlwaysOnTop;

        // load language pack
        Local.UpdateFrmMain(ForceUpdateAction.LANGUAGE);
    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // save FrmMain placement
        var wp = WindowSettings.GetPlacementFromWindow(this);
        WindowSettings.SetFrmMainPlacementConfig(wp);


        Config.Write();
    }


    private void Local_OnFrmMainUpdateRequested(ForceUpdateAction e)
    {
        if (e.HasFlag(ForceUpdateAction.LANGUAGE))
        {
            LoadLanguages();
        }
    }


    private void LoadToolbarItemsConfig()
    {
        Toolbar.Items.Clear();

        // add other items
        Toolbar.AddItems(Config.ToolbarItems, (item) =>
        {
            if (item.GetType() != typeof(ToolStripButton))
                return;

            var bItem = item as ToolStripButton;
            if (bItem is null || !bItem.CheckOnClick) return;

            var tagModel = bItem.Tag as ToolbarItemTagModel;
            if (tagModel is null || string.IsNullOrEmpty(tagModel.CheckableConfigBinding))
                return;

            // get config name
            var configProp = Config.GetProp(tagModel.CheckableConfigBinding);
            if (configProp is null) return;

            // get config value
            var propValue = configProp.GetValue(null);
            if (propValue is null) return;

            // load check state based on config value
            if (configProp.PropertyType.IsEnum)
            {
                bItem.Checked = propValue.ToString() == tagModel.OnClick.Arguments;
            }
            else if (configProp.PropertyType == typeof(bool))
            {
                bItem.Checked = (bool)propValue;
            }
        });
    }


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
        MnuRefresh.Text = lang[$"{Name}.{nameof(MnuRefresh)}"];
        MnuReload.Text = lang[$"{Name}.{nameof(MnuReload)}"];
        MnuReloadImageList.Text = lang[$"{Name}.{nameof(MnuReloadImageList)}"];
        MnuOpenWith.Text = lang[$"{Name}.{nameof(MnuOpenWith)}"];
        MnuEdit.Text = lang[$"{Name}.{nameof(MnuEdit)}"];
        MnuPrint.Text = lang[$"{Name}.{nameof(MnuPrint)}"];
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

        #endregion
    }

}

