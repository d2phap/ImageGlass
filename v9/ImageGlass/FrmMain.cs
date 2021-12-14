using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using ImageGlass.UI.WinApi;

namespace ImageGlass;

public partial class FrmMain : Form
{
    private ViewBox _viewer;

    public object Configs { get; private set; }

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
            _viewer.Image = new(args[1], true);
        }
        else
        {
            _viewer.Image = new(filename, true);
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
            _viewer.Image = new(of.FileName, true);
            _viewer.CurrentZoom = 0.5f;
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

    private void MnuMain_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Local.BtnMainMenu.Checked = true;
    }

    private void MnuMain_Closing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        Local.BtnMainMenu.Checked = false;
    }

    private void subMenu_DropDownOpening(object sender, EventArgs e)
    {
        var mnuItem = sender as ToolStripMenuItem;
        if (!mnuItem.HasDropDownItems)
        {
            return; // not a drop down item
        }

        // apply corner
        CornerApi.ApplyCorner(mnuItem.DropDown.Handle);

        mnuItem.DropDown.BackColor = Config.Theme.Settings.MenuBgColor;

        //get position of current menu item
        var pos = new Point(mnuItem.GetCurrentParent().Left, mnuItem.GetCurrentParent().Top);

        // Current bounds of the current monitor
        var currentScreen = Screen.FromPoint(pos);

        // Find the width of sub-menu
        var maxWidth = 0;
        foreach (var subItem in mnuItem.DropDownItems)
        {
            if (subItem is ToolStripMenuItem mnu)
            {
                maxWidth = Math.Max(mnu.Width, maxWidth);
            }
        }
        maxWidth += 10; // Add a little wiggle room

        var farRight = pos.X + MnuMain.Width + maxWidth;
        var farLeft = pos.X - maxWidth;

        // get left and right distance to compare
        var leftGap = farLeft - currentScreen.Bounds.Left;
        var rightGap = currentScreen.Bounds.Right - farRight;

        if (leftGap >= rightGap)
        {
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Left;
        }
        else
        {
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Right;
        }
    }

    private void MnuMain_DpiChangedBeforeParent(object sender, EventArgs e)
    {
        var iconHeight = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);

        MnuExit.Image = 
            new Bitmap(iconHeight, iconHeight);

        //if (mnuMainChannels.DropDownItems.Count > 0)
        //{
        //    mnuMainChannels.DropDownItems[0].Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
        //}

        //if (mnuLoadingOrder.DropDownItems.Count > 0)
        //{
        //    mnuLoadingOrder.DropDownItems[0].Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
        //}
    }
}
