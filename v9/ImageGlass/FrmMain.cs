using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using System.Reflection;

namespace ImageGlass;

public partial class FrmMain : Form
{
    private ViewBox _viewer;


    public FrmMain()
    {
        InitializeComponent();

        // Get the DPI of the current display
        DpiApi.OnDpiChanged += OnDpiChanged;
        DpiApi.CurrentDpi = DeviceDpi;

        SetUpFrmMainConfigs();
        SetUpFrmMainTheme();

        // apply DPI changes
        OnDpiChanged();

        _viewer = new(PanCenter, null)
        {
            MinZoom = 0.01f,
            MaxZoom = 35,
        };
        _viewer.OnZoomChanged += _viewer_OnZoomChanged;

        Prepare();
    }

    protected override void WndProc(ref Message m)
    {
        // WM_SYSCOMMAND
        if (m.Msg == 0x0112)
        {
            // When user clicks on MAXIMIZE button on title bar
            if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
            {
                // The window is being maximized
            }
            // When user clicks on the RESTORE button on title bar
            else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
            {
                // The window is being restored
            }
        }
        else if (m.Msg == DpiApi.WM_DPICHANGED)
        {
            // get new dpi value
            DpiApi.CurrentDpi = (short)m.WParam;
        }

        base.WndProc(ref m);
    }

    private void OnDpiChanged()
    {
        Text = DpiApi.CurrentDpi.ToString();

        // scale toolbar icons corresponding to DPI
        var newIconHeight = DpiApi.Transform(Config.ToolbarIconHeight);

        // reload theme
        Config.Theme.LoadTheme(newIconHeight);

        // update toolbar theme
        Toolbar.UpdateTheme(newIconHeight);
    }

    private void _viewer_OnZoomChanged(ZoomEventArgs e)
    {
        Text = $"{e.ZoomFactor * 100}%";
    }


    private void Prepare(string filename = @"C:\Users\d2pha\Desktop\logo.png")
    {
        var args = Environment.GetCommandLineArgs()
            .Where(cmd => !cmd.StartsWith('-'))
            .ToArray();

        if (args.Length > 1)
        {
            _viewer.Image = Config.Codec.Load(args[1]);
        }
        else
        {
            _viewer.Image = Config.Codec.Load(filename);
        }
    }



    private bool IG_OpenFile()
    {
        var of = new OpenFileDialog()
        {
            Multiselect = false,
            CheckFileExists = true,
        };


        if (of.ShowDialog() == DialogResult.OK)
        {
            _viewer.Image = Config.Codec.Load(of.FileName);
            _viewer.CurrentZoom = 1f;


            return true;
        }

        return false;
    }

    private void FrmMain_Resize(object sender, EventArgs e)
    {

    }



    private void MnuMain_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Local.BtnMainMenu.Checked = true;
    }

    private void MnuMain_Closing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        Local.BtnMainMenu.Checked = false;
    }

    private void Toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem.Name == Local.BtnMainMenu.Name) return;

        var tagModel = e.ClickedItem.Tag as ToolbarItemTagModel;
        if (tagModel is null) return;

        // Find the private method in FrmMain
        var method = GetType().GetMethod(
            tagModel.OnClick,
            BindingFlags.Instance | BindingFlags.NonPublic);

        // method must be bool/void()
        var result = (bool?)method?.Invoke(this, null);

        var btn = e.ClickedItem as ToolStripButton;
        if (btn is not null)
        {
            btn.Checked = btn.CheckOnClick && result == true;
        }
    }
}
