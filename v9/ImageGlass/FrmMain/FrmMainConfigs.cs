using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmMain
{
    private void SetUpFrmMainConfigs()
    {
        // Toolbar
        Toolbar.Visible = Config.IsShowToolbar;
        Toolbar.IconHeight = Config.ToolbarIconHeight;
        LoadToolbarItems();

        // Thumbnail bar
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;


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



    private void LoadToolbarItems()
    {
        Toolbar.Items.Clear();

        // add other items
        Toolbar.AddItems(Config.ToolbarItems);
    }


}

