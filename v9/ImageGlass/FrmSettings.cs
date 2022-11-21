using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Diagnostics;

namespace ImageGlass;

public partial class FrmSettings : ModernForm
{
    public FrmSettings()
    {
        InitializeComponent();

        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        lblSettingsFilePath.Text = path;
    }

    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        base.ApplyTheme(darkMode, style);

        lblSettingsFilePath.ForeColor = Config.Theme.ColorPalatte.LightText;
        btnOpenSettingsFile.DarkMode = Config.Theme.Settings.IsDarkMode;

    }

    private void btnOpenSettingsFile_Click(object sender, EventArgs e)
    {
        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true,
        };

        using var proc = Process.Start(psi);
    }
}
