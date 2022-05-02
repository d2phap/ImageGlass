

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


        // toolbar
        Toolbar.Theme =
            MnuMain.Theme =
            MnuContext.Theme =
            MnuSubMenu.Theme = Config.Theme;

        // background
        BackColor = Sp1.BackColor = Sp2.BackColor = Config.BackgroundColor;
        PicMain.BackColor = Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Settings.TextColor;

        // Thumbnail bar
        Gallery.SetRenderer(new ModernGalleryRenderer(Config.Theme));
        Sp1.SplitterBackColor =
            Gallery.BackColor = Config.Theme.Settings.ThumbnailBarBgColor;

        // Side panels
        Sp2.SplitterBackColor = Config.Theme.Settings.ThumbnailBarBgColor;

        // navigation buttons
        PicMain.NavHoveredColor = Color.FromArgb(200, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavPressedColor = Color.FromArgb(240, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

    }

}

