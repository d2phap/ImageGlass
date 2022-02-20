using ImageGlass.Base;
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
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;
        Sp1.SplitterDistance = 465;

        Sp2.Panel1Collapsed = true;
        Sp3.Panel2Collapsed = true;


        // PicBox
        PicBox.ZoomMode = Config.ZoomMode;


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
        // load window placement from settings
        WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

        TopMost = Config.IsWindowAlwaysOnTop;
    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // save FrmMain placement
        var wp = WindowSettings.GetPlacementFromWindow(this);
        WindowSettings.SetFrmMainPlacementConfig(wp);


        Config.Write();
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


}

