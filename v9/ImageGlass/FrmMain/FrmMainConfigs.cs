using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmMain
{
    private void SetUpFrmMainConfigs()
    {
        // Toolbar
        Toolbar.Visible = Config.IsShowToolbar;
        Toolbar.ImageScalingSize = new(Config.ToolbarIconHeight, Config.ToolbarIconHeight);
        LoadToolbarIcons();

        // Thumbnail bar
        //PanBottom.Visible = Config.IsShowThumbnail;


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

        var mainMenuItem = new ToolStripButton()
        {
            Name = "_mainMenu",
            Image = th.ThumbnailBar,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Main menu",
            ToolTipText = "Main menu (Alf+F)",
            CheckOnClick = true,
            ForeColor = Config.Theme.Settings.TextColor,
            Padding = Constants.TOOLBAR_BTN_PADDING,
            Margin = Constants.TOOLBAR_BTN_MARGIN,

            Alignment = ToolStripItemAlignment.Right,
            Overflow = ToolStripItemOverflow.Never,
        };

        Toolbar.Items.Add(mainMenuItem);


        var openBtnIndex = Toolbar.Items.Add(new ToolStripButton()
        {
            Name = "_openFile",
            Image = th.OpenFile,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Open file...",
            ToolTipText = "Open file...",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
            Padding = Constants.TOOLBAR_BTN_PADDING,
            Margin = Constants.TOOLBAR_BTN_MARGIN,
            ForeColor = Config.Theme.Settings.TextColor,
        });
        Toolbar.Items.Add(new ToolStripSeparator()
        {
            AutoSize = false,
            Height = (int)(Toolbar.Height * 0.55),
            Width = 8,
        });
        Toolbar.Items.Add(new ToolStripButton()
        {
            Image = th.LockZoom,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Lock file",
            ToolTipText = "Lock file",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
            Padding = Constants.TOOLBAR_BTN_PADDING,
            Margin = Constants.TOOLBAR_BTN_MARGIN,
            ForeColor = Config.Theme.Settings.TextColor,
        });
        Toolbar.Items.Add(new ToolStripButton()
        {
            Image = th.Print,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Print image...",
            ToolTipText = "Print image...",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
            Padding = Constants.TOOLBAR_BTN_PADDING,
            Margin = Constants.TOOLBAR_BTN_MARGIN,
            ForeColor = Config.Theme.Settings.TextColor,
        });
        Toolbar.Items.Add(new ToolStripButton()
        {
            Image = th.Delete,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Delete image...",
            ToolTipText = "Delete image...",
            Alignment = ToolStripItemAlignment.Left,
            CheckOnClick = true,
            Padding = Constants.TOOLBAR_BTN_PADDING,
            Margin = Constants.TOOLBAR_BTN_MARGIN,
            ForeColor = Config.Theme.Settings.TextColor,

            Enabled = false,
        });


        Toolbar.Items["_openFile"].Click += FrmMain_Click;
    }

    private void FrmMain_Click(object? sender, EventArgs e)
    {
        OpenFile();
    }
}

