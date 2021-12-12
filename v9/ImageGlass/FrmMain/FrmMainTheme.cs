using ImageGlass.Base;
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
        BackColor = Config.Theme.Settings.BgColor;

        toolBar.Renderer = new ModernToolbarRenderer(Config.Theme);
        toolBar.BackColor = Config.Theme.Settings.ToolbarBgColor;

        // Overflow button and Overflow dropdown
        toolBar.OverflowButton.DropDown.BackColor = toolBar.BackColor;
        toolBar.OverflowButton.ForeColor = Config.Theme.Settings.TextColor;

        UpdateTheme();
    }


    private void UpdateTheme(SystemThemeMode theme = SystemThemeMode.Unknown)
    {
        //var newTheme = theme;
        //if (theme == SystemThemeMode.Unknown)
        //{
        //    newTheme = ThemeUtils.GetSystemThemeMode();
        //}

        //if (newTheme == SystemThemeMode.Light)
        //{
        //    BackColor = Color.FromArgb(255, 255, 255, 255);
        //}
        //else
        //{
        //    BackColor = Color.FromArgb(255, 26, 34, 39);
        //}
    }


    private void LoadToolbarIcons(bool forceReloadIcon = false)
    {
        if (forceReloadIcon)
        {
            //
        }

        var th = Config.Theme.ToolbarIcons;
        toolBar.Items.Clear();

        var mainMenuItem = new ToolStripButton()
        {
            Image = th.Crop,
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
                CheckOnClick = true,
                Padding = Constants.TOOLBAR_BTN_PADDING,
                Margin = Constants.TOOLBAR_BTN_MARGIN,
                ForeColor = Config.Theme.Settings.TextColor,
            };

            toolBar.Items.Add(item);
        }
    }
}

