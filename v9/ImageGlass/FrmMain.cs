using ImageGlass.Base.WinApi;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmMain : Form
{
    private ViewBox _viewer;


    public FrmMain()
    {
        InitializeComponent();

        SetUpFrmMainConfigs();
        SetUpFrmMainTheme();

        _viewer = new(PanCenter, null)
        {
            MinZoom = 0.01f,
            MaxZoom = 35,
        };
        _viewer.OnZoomChanged += _viewer_OnZoomChanged;

        Prepare();
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



    private void OpenFile()
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
        }
    }

    private void FrmMain_Resize(object sender, EventArgs e)
    {

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
            DpiApi.CurrentDpi = (short)m.WParam;
            
            Text = DpiApi.CurrentDpi.ToString();
        }

        base.WndProc(ref m);
    }

    private void MnuMain_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Local.BtnMainMenu.Checked = true;
    }

    private void MnuMain_Closing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        Local.BtnMainMenu.Checked = false;
    }
}
