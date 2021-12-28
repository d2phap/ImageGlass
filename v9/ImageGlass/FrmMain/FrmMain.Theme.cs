

using ImageGlass.Settings;
using ImageGlass.UI;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.Theme contains methods for dynamic binding     *
 * ****************************************************** */

public partial class FrmMain
{

    public void SetUpFrmMainTheme()
    {
        UpdateTheme();
    }


    private void UpdateTheme(SystemThemeMode mode = SystemThemeMode.Unknown)
    {
        var themMode = mode;
        
        if (mode == SystemThemeMode.Unknown)
        {
            themMode = ThemeUtils.GetSystemThemeMode();
        }

        // correct theme mode
        var isDarkMode = themMode != SystemThemeMode.Light;


        // set theme
        Toolbar.Theme =
            MnuMain.Theme = Config.Theme;

        // background
        BackColor = Config.BackgroundColor;
        PicBox.BackColor = Config.BackgroundColor;
        PicBox.ForeColor = Config.Theme.Settings.TextColor;

        // navigation buttons
        PicBox.NavHoveredColor = Color.FromArgb(200, Config.Theme.Settings.ToolbarBgColor);
        PicBox.NavPressedColor = Color.FromArgb(240, Config.Theme.Settings.ToolbarBgColor);
        PicBox.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicBox.NavRightImage = Config.Theme.Settings.NavButtonRight;

        // Thumbnail bar
        Sp1.SplitterBackColor =
            PanBot.BackColor = Config.Theme.Settings.ThumbnailBarBgColor;

        // Side panels
        Sp2.SplitterBackColor =
            Sp3.SplitterBackColor =
            PanLeft.BackColor =
            PanRight.BackColor = Config.Theme.Settings.ThumbnailBarBgColor;

    }

}

