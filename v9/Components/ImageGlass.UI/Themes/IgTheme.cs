/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using ImageGlass.Base.WinApi;
using ImageGlass.Heart;

namespace ImageGlass.UI;

public class IgTheme
{
    private int _iconHeight = Constants.TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Filename of theme configuration since v9.0
    /// </summary>
    public static string CONFIG_FILE { get; } = "igtheme.json";



    /// <summary>
    /// Gets the height of toolbar icons
    /// </summary>
    public int ToolbarIconHeight => DpiApi.Transform(_iconHeight);

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
    /// Gets, sets codec to load theme icons
    /// </summary>
    public IIgCodec? Codec { get; set; }



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
    public IgTheme(IIgCodec? codec = null, string themeFolderPath = "", int iconHeight = Constants.TOOLBAR_ICON_HEIGHT)
    {
        Codec = codec;
        _iconHeight = iconHeight;
        _ = LoadTheme(themeFolderPath);
    }


    /// <summary>
    /// Loads theme
    /// </summary>
    /// <param name="themeFolderPath"></param>
    /// <returns></returns>
    public bool LoadTheme(string themeFolderPath)
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
            JsonModel = Helpers.ReadJson<IgThemeJsonModel>(ConfigFilePath);
        }
        catch { }

        if (JsonModel == null)
        {
            IsValid = false;
            return IsValid;
        }

        // import theme info
        Info = JsonModel.Info;

        // check required fields
        if (string.IsNullOrEmpty(Info.Name)
            || string.IsNullOrEmpty(Info.ConfigVersion))
        {
            IsValid = false;
            return IsValid;
        }


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

                // property is Bitmap
                if (prop?.PropertyType == typeof(Bitmap))
                {
                    var bmp = Codec.Load(Path.Combine(FolderPath, value), new());
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
                var icon = Codec.Load(Path.Combine(FolderPath, value), new()
                {
                    Width = ToolbarIconHeight,
                    Height = ToolbarIconHeight,
                });

                ToolbarIcons.GetType().GetProperty(item.Key)?.SetValue(ToolbarIcons, icon);
            }
            catch { }
        }
        #endregion


        IsValid = true;
        return IsValid;
    }


    /// <summary>
    /// Reloads the theme with new icon size
    /// </summary>
    /// <param name="iconHeight"></param>
    /// <returns></returns>
    public bool ReloadTheme(int? iconHeight = null)
    {
        if (iconHeight is not null)
        {
            _iconHeight = iconHeight.Value;
        }

        return LoadTheme(FolderPath);
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
