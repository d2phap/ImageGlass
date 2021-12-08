using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmMain
{
    private void SetUpFrmMainConfigs()
    {
        this.Load += FrmMainConfig_Load;
        this.FormClosing += FrmMainConfig_FormClosing;
        this.SizeChanged += FrmMainConfig_SizeChanged;
    }

    private void FrmMainConfig_SizeChanged(object? sender, EventArgs e)
    {
        Config.FrmMainPositionX = this.Location.X;
        Config.FrmMainPositionY = this.Location.Y;
        Config.FrmMainWidth = this.Size.Width;
        Config.FrmMainHeight = this.Size.Height;
    }

    private void FrmMainConfig_Load(object? sender, EventArgs e)
    {
        this.TopMost = Config.IsWindowAlwaysOnTop;

        // load window placement from settings
        WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());
    }

    private void FrmMainConfig_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // save FrmMain placement
        var wp = WindowSettings.GetPlacementFromWindow(this);
        WindowSettings.SetFrmMainPlacementConfig(wp);


        Config.Write();
    }
}

