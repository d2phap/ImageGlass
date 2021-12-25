using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Heart;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using System.Diagnostics;
using System.Reflection;

namespace ImageGlass;

public partial class FrmMain : Form
{
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

    private void FrmMain_Load(object sender, EventArgs e)
    {
        Text = $"{PicBox.Width}x{PicBox.Height}";
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

    private void PicBox_OnZoomChanged(ZoomEventArgs e)
    {

    }


    private async void Prepare(string path = "")
    {
        var inputPath = path.Trim();

        if (string.IsNullOrEmpty(inputPath))
        {
            var args = Environment.GetCommandLineArgs()
                .Where(cmd => !cmd.StartsWith('-'))
                .ToArray();

            if (args.Length > 1)
            {
                inputPath = args[1];
            }
            else
            {
                inputPath = @"D:\_GITHUB\sample-images\Samples\GIF\unicorn.gif";
            }
        }

        if (string.IsNullOrEmpty(inputPath))
        {
            return;
        }

        Local.Metadata = Config.Codec.LoadMetadata(inputPath);

        PicBox.ShowMessage("Loading image... \n" + inputPath, 0, 1500);
        PicBox.Photo = await Config.Codec.LoadAsync(inputPath, new(Local.Metadata));
        PicBox.ClearMessage();
        PicBox.LoadImage(new Bitmap(inputPath));
    }



    private void FrmMain_Resize(object sender, EventArgs e)
    {
        Text = $"{PicBox.Width}x{PicBox.Height}";
    }


    private void Toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        var tagModel = e.ClickedItem.Tag as ToolbarItemTagModel;
        if (tagModel is null || string.IsNullOrEmpty(tagModel.OnClick.Executable)) return;

        // Find the private method in FrmMain
        var method = GetType().GetMethod(
            tagModel.OnClick.Executable,
            BindingFlags.Instance | BindingFlags.NonPublic);


        // run built-in method
        if (method is not null)
        {
            // method must be bool/void()
            var result = (bool?)method?.Invoke(this, null);

            var btn = e.ClickedItem as ToolStripButton;
            if (btn is not null)
            {
                btn.Checked = btn.CheckOnClick && result == true;
            }

            return;
        }


        // TODO: test file macro <file>
        var currentFilename = @"E:\WALLPAPER\NEW\dark2\horizon_by_t1na_den4yvj-fullview.jpg";


        // run external command line
        var proc = new Process
        {
            StartInfo = new(tagModel.OnClick.Executable)
            {
                Arguments = tagModel.OnClick.Arguments.Replace(Constants.FILE_MACRO, currentFilename),
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true,
            },
        };

        try
        {
            proc.Start();
        }
        catch { }
    }

}
