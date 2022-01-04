using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Gallery;
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

        Prepare();
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
                inputPath = @"C:\Users\d2pha\Desktop\map50.jpg";
            }
        }

        if (string.IsNullOrEmpty(inputPath))
        {
            return;
        }


        NextPic(inputPath);

        LoadGallery(inputPath);
    }


    private async void NextPic(string filename)
    {
        Local.Metadata = Config.Codec.LoadMetadata(filename);

        PicBox.ShowMessage("Loading image... \n" + filename, 0, 1500);
        var bmp = await Config.Codec.LoadAsync(filename, new(Local.Metadata));
        PicBox.LoadImage(bmp);
    }


    private void LoadGallery(string path = "")
    {
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        if (InvokeRequired)
        {
            Invoke(LoadGallery, path);
        }

        var files = Directory.GetFiles(Path.GetDirectoryName(path) ?? "");

        Gallery.Items.Clear();
        Gallery.SuspendPaint();
        foreach (var file in files)
        {
            var item = new ImageListViewItem(file);
            Gallery.Items.Add(item);
        }
        Gallery.ResumePaint();
    }



    private void FrmMain_Resize(object sender, EventArgs e)
    {
        Text = $"{PicBox.Width}x{PicBox.Height}";
    }


    private void PicBox_OnImageChanged(EventArgs e)
    {
        PicBox.ClearMessage();
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

    private void Gallery_ItemClick(object sender, ItemClickEventArgs e)
    {
        NextPic(e.Item.FileName);
    }
}
