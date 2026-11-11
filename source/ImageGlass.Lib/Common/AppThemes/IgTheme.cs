/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Media;
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ImageGlass.Common.AppThemes;


[JsonSerializable(typeof(IgTheme))]
public partial class IgThemeJsonContext : JsonSerializerContext { }


/// <summary>
/// Represents a theme pack for the app.
/// </summary>
public partial class IgTheme : PhReactive
{

    // Serializable properties
    public IgThemeMetadata _Metadata { get; set; } = new();
    public IgThemeInfo Info { get; set; } = new();
    public IgThemeSettings Settings { get; set; } = new();
    public IgThemeColors Colors { get; set; } = new();
    public Dictionary<string, string> ToolbarIcons { get; set; } = [];



    #region Static Properties

    /// <summary>
    /// Theme specs version, to check for compatibility.
    /// </summary>
    public static float SPEC_VERSION => 10;

    /// <summary>
    /// Filename of theme configuration since v9.0.
    /// </summary>
    public static string CONFIG_FILE => "igtheme.json";


    #endregion // Static Properties



    #region Instance Properties

    /// <summary>
    /// Gets the full path of theme folder.
    /// </summary>
    [JsonIgnore]
    public string FolderPath { get; set; } = "";

    /// <summary>
    /// Gets <see cref="FolderPath"/> as the OS sees it, for display: a pack ships in the app dir or
    /// is installed into the config dir, and either can be reached through a platform indirection.
    /// </summary>
    [JsonIgnore]
    public string DisplayFolderPath => BHelper.GetRealPlatformPath(FolderPath);

    /// <summary>
    /// Gets the name of theme folder.
    /// </summary>
    [JsonIgnore]
    public string FolderName => Path.GetFileName(FolderPath);

    /// <summary>
    /// Gets the full path of theme config file (<see cref="IgTheme.CONFIG_FILE"/>).
    /// </summary>
    [JsonIgnore]
    public string ConfigFilePath => Path.Combine(FolderPath, IgTheme.CONFIG_FILE);

    /// <summary>
    /// Indicates if this theme is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsValid { get; set; } = false;

    /// <summary>
    /// Gets the accent color parsed from theme pack.
    /// </summary>
    [JsonIgnore]
    public Color AccentColor => BHelper.ColorFromHex(Colors.AccentColor);

    /// <summary>
    /// Gets the value indicates that the theme pack accent color follow system accent color.
    /// </summary>
    [JsonIgnore]
    public bool UseSystemAccent => string.IsNullOrWhiteSpace(Colors.AccentColor);

    /// <summary>
    /// Gets the base color according to <see cref="Settings.IsDarkMode"/>.
    /// </summary>
    [JsonIgnore]
    public Color BaseColor => Settings.IsDarkMode
        ? Avalonia.Media.Colors.Black
        : Avalonia.Media.Colors.White;

    /// <summary>
    /// Gets the inverted base color according to <see cref="Settings.IsDarkMode"/>.
    /// </summary>
    [JsonIgnore]
    public Color InvertedBaseColor => Settings.IsDarkMode
        ? Avalonia.Media.Colors.White
        : Avalonia.Media.Colors.Black;

    #endregion // Instance Properties



    #region Public methods

    /// <summary>
    /// Reads theme config file and loads the theme properties.
    /// </summary>
    /// <returns>The current instance of this theme pack.</returns>
    public IgTheme Load(string themeFolderPath)
    {
        return BHelper.RunSync(() => LoadAsync(themeFolderPath));
    }


    /// <summary>
    /// Reads theme config file and loads the theme properties.
    /// </summary>
    /// <returns>The current instance of this theme pack.</returns>
    public async Task<IgTheme> LoadAsync(string themeFolderPath)
    {
        FolderPath = themeFolderPath;

        var th = await Task.Run(async () =>
        {
            if (!File.Exists(ConfigFilePath)) return null;

            try
            {
                // 1. create json context
                var jsonOptions = BHelper.CreateJsonOptions();
                var jsonContext = new IgThemeJsonContext(jsonOptions);

                // 2. parse theme config
                var th = await BHelper.ReadJsonFromFileAsync(ConfigFilePath, jsonContext.IgTheme);

                return th;
            }
            catch { }

            return null;
        }).ConfigureAwait(false);


        IsValid = th != null;

        // 3. load theme properties
        if (th != null)
        {
            _Metadata = th._Metadata;
            Info = th.Info;
            Settings = th.Settings;
            Colors = th.Colors;
            ToolbarIcons = th.ToolbarIcons;
        }

        return this;
    }


    /// <summary>
    /// Gets the full path of theme icon.
    /// </summary>
    public string GetIconPath(IgThemeIcon iconName)
    {
        var svgPath = "";

        try
        {
            var themeIconName = "";

            // icon from Settings
            if (iconName == IgThemeIcon.AppLogo)
            {
                themeIconName = Settings.AppLogo;
            }
            // icon from ToolbarIcons
            else if (!ToolbarIcons.TryGetValue(iconName.ToString(), out themeIconName))
            {
                return svgPath;
            }


            // get full path
            svgPath = Path.Combine(FolderPath, themeIconName);
            if (!File.Exists(svgPath)) return "";
        }
        catch { }

        return svgPath;
    }

    #endregion // Public methods

}

