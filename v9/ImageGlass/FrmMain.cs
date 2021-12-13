using ImageGlass.PhotoBox;

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
            MaxZoom = 3500,
        };
        _viewer.MoveOver += Viewer_MoveOver;
        _viewer.ZoomChanged += Viewer_ZoomChanged;

        Prepare();
    }

    private void Prepare()
    {
        Task.Run(() =>
        {
            var args = Environment.GetCommandLineArgs()
            .Where(cmd => !cmd.StartsWith('-'))
            .ToArray();

            if (args.Length > 1)
            {
                _viewer.Image = new(args[1], true);
            }
        });
    }

    private void Viewer_ZoomChanged(float CurZoom)
    {
        Text = $"{CurZoom * 100}%";
    }

    private void Viewer_MoveOver(float Px, float Py)
    {
        //Text = $"X={Px}, Y={Py}";
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
            _viewer.Image = new(of.OpenFile(), true);
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

        base.WndProc(ref m);
    }

}
