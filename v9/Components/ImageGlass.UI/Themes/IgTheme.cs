/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using WicNet;

namespace ImageGlass.UI;

public class IgTheme
{
    private int _iconHeight = Constants.TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Filename of theme configuration since v9.0
    /// </summary>
    public static string CONFIG_FILE { get; } = "igtheme.json";


    /// <summary>
    /// Gets the danger color.
    /// </summary>
    public Color DangerColor
    {
        get
        {
            var danger = Color.FromArgb(255, 255, 0, 0);
            var themedColor = ThemeUtils.AdjustLightness(danger, Info.IsDark ? -0.8f : 0.8f);

            return themedColor;
        }
    }



    /// <summary>
    /// Gets, sets the height of toolbar icons.
    /// You need to manually call <see cref="LoadTheme"/> to update toolbar icons.
    /// </summary>
    public int ToolbarActualIconHeight
    {
        get => DpiApi.Transform(_iconHeight);
        set => _iconHeight = value;
    }

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
    public bool IsValid { get; internal set; } = false;

    

    /// <summary>
    /// Theme information
    /// </summary>
    public IgThemeInfo Info { get; internal set; } = new();

    /// <summary>
    /// Theme colors
    /// </summary>
    public IgThemeSettings Settings { get; internal set; } = new();

    /// <summary>
    /// Theme toolbar icons
    /// </summary>
    public IgThemeToolbarIcons ToolbarIcons { get; internal set; } = new();



    /// <summary>
    /// Initialize theme pack
    /// </summary>
    /// <param name="themeFolderPath"></param>
    public IgTheme(
        string themeFolderPath = "",
        int iconHeight = Constants.TOOLBAR_ICON_HEIGHT)
    {
        ToolbarActualIconHeight = iconHeight;

        // read theme config
        _ = ReadThemeConfig(themeFolderPath);
    }


    /// <summary>
    /// Loads theme config file into <see cref="JsonModel"/>, and validates it.
    /// </summary>
    /// <param name="themeFolderPath"></param>
    /// <returns></returns>
    public bool ReadThemeConfig(string themeFolderPath)
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
            //parse theme config file
            JsonModel = BHelper.ReadJson<IgThemeJsonModel>(ConfigFilePath);
        }
        catch { }

        if (JsonModel == null)
        {
            IsValid = false;
            return IsValid;
        }

        // check required fields
        if (string.IsNullOrEmpty(JsonModel.Info.Name)
            || string.IsNullOrEmpty(JsonModel._Metadata.Version))
        {
            IsValid = false;
            return IsValid;
        }

        IsValid = true;
        return IsValid;
    }


    /// <summary>
    /// Loads theme from <see cref="JsonModel"/>
    /// </summary>
    /// <param name="iconHeight">Toolbar icon height</param>
    public void LoadTheme(int? iconHeight = null)
    {
        if (iconHeight is not null)
        {
            _iconHeight = iconHeight.Value;
        }

        if (IsValid is false || JsonModel is null) return;

        // import theme info
        Info = JsonModel.Info;
        

        #region import theme colors
        foreach (var item in JsonModel.Settings)
        {
            var value = (item.Value ?? "")?.ToString()?.Trim();
            if (string.IsNullOrEmpty(value))
                continue;
            
            var prop = Settings.GetType().GetProperty(item.Key);

            try
            {
                // property is Color
                if (prop?.PropertyType == typeof(Color))
                {
                    var colorItem = ThemeUtils.ColorFromHex(value);
                    prop.SetValue(Settings, colorItem);
                    continue;
                }

                // property is WicBitmapSource
                if (prop?.PropertyType == typeof(WicBitmapSource))
                {
                    var data = PhotoCodec.Load(Path.Combine(FolderPath, value), new()
                    {
                        Width = ToolbarActualIconHeight * 2,
                        Height = ToolbarActualIconHeight * 2,
                    });

                    prop.SetValue(Settings, data.Image);
                    continue;
                }

                // property is Bitmap
                if (prop?.PropertyType == typeof(Bitmap))
                {
                    var bmp = PhotoCodec.GetThumbnail(Path.Combine(FolderPath, value), ToolbarActualIconHeight, ToolbarActualIconHeight);

                    prop.SetValue(Settings, bmp);
                    continue;
                }


                // property is other types
                var typedValue = Convert.ChangeType(value, prop?.PropertyType ?? typeof(string));
                prop?.SetValue(Settings, typedValue);
            }
            catch { }
        }

        if (Settings.AppLogo is null)
        {
            Settings.AppLogo = Properties.Resources.DefaultLogo;
        }
        #endregion


        #region import toolbar icons
        foreach (var item in JsonModel.ToolbarIcons)
        {
            var value = (item.Value ?? "")?.ToString()?.Trim();
            if (string.IsNullOrEmpty(value))
                continue;

            try
            {
                var bmp = PhotoCodec.GetThumbnail(Path.Combine(FolderPath, value), ToolbarActualIconHeight, ToolbarActualIconHeight);

                ToolbarIcons.GetType().GetProperty(item.Key)?.SetValue(ToolbarIcons, bmp);
            }
            catch { }
        }
        #endregion
    }


    /// <summary>
    /// Saves current settings as a new config file
    /// </summary>
    /// <param name="filePath"></param>
    public async Task SaveConfigAsFileAsync(string filePath)
    {
        await BHelper.WriteJsonAsync(filePath, JsonModel);
    }


    /// <summary>
    /// Gets toolbar icon from theme property name or image file path.
    /// </summary>
    /// <param name="name">
    /// Theme pack's icon name or image file path.
    /// Example: <c>OpenFile</c>, or <c>.\Themes\Kobe\OpenFile.svg</c>
    /// </param>
    /// <returns></returns>
    public Bitmap? GetToolbarIcon(string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        // get icon from theme pack icon name
        var icon = ToolbarIcons.GetType().GetProperty(name ?? string.Empty)?.GetValue(ToolbarIcons);

        // get icon from file
        if (icon == null)
        {
            var fullPath = BHelper.ResolvePath(name);
            var data = PhotoCodec.Load(fullPath, new()
            {
                Width = ToolbarActualIconHeight,
                Height = ToolbarActualIconHeight,
            });

            icon = data.Image;
        }

        return icon as Bitmap;
    }
}
