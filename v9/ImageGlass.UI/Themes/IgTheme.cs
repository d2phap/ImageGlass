using ImageGlass.Base;
using ImageGlass.UI.WinApi;

namespace ImageGlass.UI;

public class IgTheme
{
    private float _iconHeight = Constants.DEFAULT_TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Filename of theme configuration since v9.0
    /// </summary>
    public static string CONFIG_FILE { get; } = "igtheme.json";



    /// <summary>
    /// Gets the height of toolbar icons
    /// </summary>
    public float ToolbarIconHeight => DPIScaling.Transform<float>(_iconHeight);

    /// <summary>
    /// Theme API version, to check compatibility
    /// </summary>
    public string CONFIG_VERSION { get; } = "9";

    /// <summary>
    /// Gets the name of theme folder
    /// </summary>
    public string FolderName { get; internal set; } = "";

    /// <summary>
    /// Gets the name of theme folder
    /// </summary>
    public string FolderPath { get; internal set; } = "";

    /// <summary>
    /// Gets theme config file path (<see cref="CONFIG_FILE"/>)
    /// </summary>
    public string ConfigFilePath { get; internal set; } = "";

    /// <summary>
    /// Gets JSON model from config file (<see cref="CONFIG_FILE"/>)
    /// </summary>
    public IgThemeJsonModel? JsonModel { get; internal set; }

    /// <summary>
    /// Checks if this theme is valid
    /// </summary>
    public bool IsValid { get; internal set; } = true;



    /// <summary>
    /// Theme information
    /// </summary>
    public IgThemeInfo Info { get; set; } = new();

    /// <summary>
    /// Theme colors
    /// </summary>
    public IgThemeSettings Settings { get; set; } = new();

    /// <summary>
    /// Theme toolbar icons
    /// </summary>
    public IgThemeToolbarIcons ToolbarIcons { get; set; } = new();



    /// <summary>
    /// Initialize theme pack
    /// </summary>
    /// <param name="themeFolderPath"></param>
    public IgTheme(string themeFolderPath = "", float iconHeight = Constants.DEFAULT_TOOLBAR_ICON_HEIGHT)
    {
        _iconHeight = iconHeight;
        _ = LoadTheme(themeFolderPath);
    }


    /// <summary>
    /// Loads theme
    /// </summary>
    /// <param name="themeFolderPath"></param>
    /// <returns></returns>
    public async Task<bool> LoadTheme(string themeFolderPath)
    {
        // get full path of config file
        ConfigFilePath = Path.Combine(themeFolderPath, CONFIG_FILE);
        if (!File.Exists(ConfigFilePath))
        {
            IsValid = false;
            return IsValid;
        }

        // get theme folder name
        FolderName = Path.GetFileName(themeFolderPath);

        // get full path of theme folder
        FolderPath = Path.GetDirectoryName(ConfigFilePath) ?? string.Empty;

        try
        {
            // parse theme config file
            JsonModel = await Helpers.ReadJson<IgThemeJsonModel>(ConfigFilePath);
        }
        catch { }

        if (JsonModel == null)
        {
            IsValid = false;
            return IsValid;
        }

        // import theme info
        Info = JsonModel.Info;

        // import theme colors
        foreach (var item in JsonModel.Settings)
        {
            if (string.IsNullOrEmpty(item.Value))
                continue;

            var value = item.Value.Trim();
            var prop = Settings.GetType().GetProperty(item.Key);

            try
            {
                // property is Color
                if (prop?.GetType() == typeof(Color))
                {
                    var colorItem = ThemeUtils.ConvertHexStringToColor(value);
                    prop.SetValue(Settings, colorItem);
                    continue;
                }

                // property is Bitmap
                if (prop?.GetType() == typeof(Bitmap))
                {
                    // TODO: load image file
                    var bmp = new Bitmap(Path.Combine(FolderPath, value));
                    prop.SetValue(Settings, bmp);
                    continue;
                }

                // property is other types
                prop?.SetValue(Settings, value);
            }
            catch { }
        }

        // import toolbar icons
        foreach (var item in JsonModel.ToolbarIcons)
        {
            try
            {
                // TODO: load image file
                var icon = new Bitmap(item.Value);
                ToolbarIcons.GetType().GetProperty(item.Key)?.SetValue(ToolbarIcons, icon);
            }
            catch { }
        }


        IsValid = true;
        return IsValid;
    }


    /// <summary>
    /// Reloads the theme with new icon size
    /// </summary>
    /// <param name="iconHeight"></param>
    /// <returns></returns>
    public async Task<bool> ReloadTheme(float? iconHeight = null)
    {
        if (iconHeight is not null)
        {
            _iconHeight = iconHeight.Value;
        }

        return await LoadTheme(FolderPath);
    }


    /// <summary>
    /// Saves current settings as a new config file
    /// </summary>
    /// <param name="filePath"></param>
    public void SaveConfigAsFile(string filePath)
    {
        Helpers.WriteJson(filePath, JsonModel);
    }
}
