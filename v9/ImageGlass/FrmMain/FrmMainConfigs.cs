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
        LoadToolbarIcons();

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



    private void LoadToolbarIcons(bool forceReloadIcon = false)
    {
        if (forceReloadIcon)
        {
            //
        }

        var th = Config.Theme.ToolbarIcons;
        Toolbar.Items.Clear();

        // add Main Menu
        Local.BtnMainMenu.Click -= BtnMainMenu_Click;
        Local.BtnMainMenu.Click += BtnMainMenu_Click;
        Toolbar.Items.Add(Local.BtnMainMenu);


        var openBtnIndex = Toolbar.Items.Add(new ToolStripButton()
        {
            Name = "Btn_OpenFile",
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Open file...",
            ToolTipText = "Open file...",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
            Tag = nameof(th.OpenFile), // save icon name to load later
        });
        Toolbar.Items.Add(new ToolStripSeparator());
        Toolbar.Items.Add(new ToolStripButton()
        {
            Name = "Btn_LockZoom",
            Tag = nameof(th.LockZoom),
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Lock file",
            ToolTipText = "Lock file",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
        });
        Toolbar.Items.Add(new ToolStripButton()
        {
            Name = "Btn_Print",
            Tag = nameof(th.Print),
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Print image...",
            ToolTipText = "Print image...",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
        });
        Toolbar.Items.Add(new ToolStripButton()
        {
            Name = "Btn_Delete",
            Tag = nameof(th.Delete),
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Delete image...",
            ToolTipText = "Delete image...",
            CheckOnClick = true,

            Alignment = ToolStripItemAlignment.Right,
        });


        Toolbar.Items["Btn_OpenFile"].Click += FrmMain_Click;
    }

    private void BtnMainMenu_Click(object? sender, EventArgs e)
    {
        MnuMain.Show(Toolbar,
            Local.BtnMainMenu.Bounds.Left + Local.BtnMainMenu.Bounds.Width - MnuMain.Width,
            Toolbar.Height);
    }

    private void FrmMain_Click(object? sender, EventArgs e)
    {
        OpenFile();
    }
}

