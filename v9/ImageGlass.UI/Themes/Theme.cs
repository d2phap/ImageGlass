using ImageGlass.UI.WinApi;

namespace ImageGlass.UI;

public class Theme
{
    /// <summary>
    /// Theme API version, to check compatibility
    /// </summary>
    public string CONFIG_VERSION { get; } = "9";

    /// <summary>
    /// Filename of theme configuration since v8.0
    /// </summary>
    public static string CONFIG_FILE { get; } = "igtheme.json";

    /// <summary>
    /// Dark / light mode
    /// </summary>
    public static bool IsDarkMode { get; } = true;


    #region PUBLIC PROPERTIES
    public Color AccentColor { get; private set; }

    public Color BackgroundColor { get; private set; }
    public Color BackgroundInactiveColor { get; private set; }


    // menu theme
    public Color MenuBackgroundColor { get; private set; }
    public Color MenuBackgroundHoverColor { get; private set; }

    public Color MenuTextColor { get; private set; }
    public Color MenuTextHoverColor { get; private set; }

    #endregion



    public Theme()
    {
        this.Update();
    }

    public void Update()
    {
        this.AccentColor = WinColors.AccentBrush;

        this.BackgroundColor = Color.FromArgb(255, 235, 246, 249);
        this.BackgroundInactiveColor = Color.FromArgb(255, 243, 243, 243);

        this.MenuBackgroundColor = this.BackgroundColor;
        this.MenuBackgroundHoverColor = WinColors.AccentBrush;
        this.MenuTextColor = Color.Black;
        this.MenuTextHoverColor = Color.White;
    }


}
