using ImageGlass.UI;

namespace ImageGlass;

public partial class FrmMain
{
    public void SetUpFrmMainTheme()
    {
        this.Load += FrmMainTheme_Load;
    }


    private void FrmMainTheme_Load(object? sender, EventArgs e)
    {
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
            this.BackColor = Color.FromArgb(255, 255, 255, 255);
        }
        else
        {
            this.BackColor = Color.FromArgb(255, 26, 34, 39);
        }
    }

}

