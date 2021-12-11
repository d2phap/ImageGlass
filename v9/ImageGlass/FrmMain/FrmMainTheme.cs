using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.Toolbar;

namespace ImageGlass;

public partial class FrmMain
{
    public void SetUpFrmMainTheme()
    {
        Load += FrmMainTheme_Load;
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

}

