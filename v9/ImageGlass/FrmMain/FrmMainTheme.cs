using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.Toolbar;

namespace ImageGlass;

public partial class FrmMain
{
    public void SetUpFrmMainTheme()
    {
        Load += FrmMainTheme_Load;

        LoadToolbarIcons();
    }


    private void FrmMainTheme_Load(object? sender, EventArgs e)
    {
        toolBar.Renderer = new ModernToolbarRenderer(Config.Theme);
        toolBar.BackColor = Config.Theme.Settings.ToolbarBgColor;

        // Overflow button and Overflow dropdown
        toolBar.OverflowButton.DropDown.BackColor = toolBar.BackColor;
        toolBar.OverflowButton.AutoSize = false;
        toolBar.OverflowButton.Padding = new Padding(10);

        UpdateTheme();
    }


    private void UpdateTheme(SystemTheme theme = SystemTheme.Unknown)
    {
        var newTheme = theme;
        if (theme == SystemTheme.Unknown)
        {
            newTheme = ThemeUtils.GetSystemTheme();
        }

        if (newTheme == SystemTheme.Light)
        {
            BackColor = Color.FromArgb(255, 255, 255, 255);
        }
        else
        {
            BackColor = Color.FromArgb(255, 26, 34, 39);
        }
    }


    private void LoadToolbarIcons(bool forceReloadIcon = false)
    {
        if (forceReloadIcon)
        {
            //
        }

        var th = Config.Theme.ToolbarIcons;
        var DEFAULT_MARGIN = new Padding(0, 8, 2, 8);
        toolBar.Items.Clear();

        var mainMenuItem = new ToolStripButton()
        {
            Image = th.Crop,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Text = "Main menu",
            ToolTipText = "Main menu (Alf+F)",
            Padding = new Padding(12),
            CheckOnClick = true,
            ForeColor = Config.Theme.Settings.TextColor,
            Margin = DEFAULT_MARGIN,

            Alignment = ToolStripItemAlignment.Right,
            Overflow = ToolStripItemOverflow.Never,
        };

        toolBar.Items.Add(mainMenuItem);

        for (int i = 0; i < 20; i++)
        {
            var item = new ToolStripButton()
            {
                Image = th.Crop,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Text = "Button " + i.ToString(),
                ToolTipText = "Button " + i.ToString(),
                Alignment = ToolStripItemAlignment.Left,
                Padding = new Padding(12),
                CheckOnClick = true,
                ForeColor = Config.Theme.Settings.TextColor,
                Margin = DEFAULT_MARGIN,
            };

            toolBar.Items.Add(item);
        }

        
    }
}

