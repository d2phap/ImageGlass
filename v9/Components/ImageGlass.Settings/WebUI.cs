/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Base.PhotoBox;
using ImageGlass.UI;
using System.Dynamic;

namespace ImageGlass.Settings;

public static class WebUI
{
    // Public properties 
    #region Public properties 

    /// <summary>
    /// Gets CSS content
    /// </summary>
    public static string Styles { get; set; } = string.Empty;


    /// <summary>
    /// Gets JavaScript content of FrmSettings
    /// </summary>
    public static string FrmSettingsJs { get; set; } = string.Empty;


    /// <summary>
    /// Gets current language as JSON
    /// </summary>
    public static string LangJson { get; set; } = string.Empty;


    /// <summary>
    /// Gets all SVG icons as JSON
    /// </summary>
    public static string SvgIconsJson { get; set; } = string.Empty;


    /// <summary>
    /// Gets all language packs as JSON
    /// </summary>
    public static  string LangListJson { get; set; } = string.Empty;


    /// <summary>
    /// Gets all theme packs as JSON
    /// </summary>
    public static  string ThemeListJson { get; set; } = string.Empty;


    /// <summary>
    /// Gets all tools as JSON
    /// </summary>
    public static  string ToolListJson { get; set; } = string.Empty;


    /// <summary>
    /// Gets all enums as JSON
    /// </summary>
    public static  string EnumsJson
    {
        get
        {
            var enumObj = new ExpandoObject();
            var enums = new Type[] {
                typeof(ImageOrderBy),
                typeof(ImageOrderType),
                typeof(ColorProfileOption),
                typeof(AfterEditAppAction),
                typeof(ImageInterpolation),
                typeof(MouseWheelAction),
                typeof(MouseWheelEvent),
                typeof(MouseClickEvent),
                typeof(Base.BackdropStyle),
                typeof(ToolbarItemModelType),
            };

            foreach (var item in enums)
            {
                var keys = Enum.GetNames(item);
                enumObj.TryAdd(item.Name, keys);
            }

            return BHelper.ToJson(enumObj);
        }
    }

    #endregion // Public properties


    // Public functions
    #region Public functions

    /// <summary>
    /// Updates value of <see cref="Styles"/>.
    /// </summary>
    public static async Task UpdateStylesAsync(bool forced = false)
    {
        if (!string.IsNullOrEmpty(Styles) && !forced) return;

        var cssPath = App.StartUpDir(Dir.WebUI, "styles.css");
        WebUI.Styles = await File.ReadAllTextAsync(cssPath);
    }


    /// <summary>
    /// Updates value of <see cref="FrmSettingsJs"/>.
    /// </summary>
    public static async Task UpdateFrmSettingsJsAsync(bool forced = false)
    {
        if (!string.IsNullOrEmpty(FrmSettingsJs) && !forced) return;

        var jsPath = App.StartUpDir(Dir.WebUI, "FrmSettings.js");
        WebUI.FrmSettingsJs = await File.ReadAllTextAsync(jsPath);
    }


    /// <summary>
    /// Updates value of <see cref="LangJson"/>.
    /// </summary>
    public static void UpdateLangJson(bool forced = false)
    {
        if (!string.IsNullOrEmpty(LangJson) && !forced) return;

        WebUI.LangJson = BHelper.ToJson(Config.Language);
    }


    /// <summary>
    /// Updates value of <see cref="SvgIconsJson"/>.
    /// </summary>
    public static async Task UpdateSvgIconsJsonAsync(bool forced = false)
    {
        if (!string.IsNullOrEmpty(SvgIconsJson) && !forced) return;

        var iconNames = new Dictionary<IconName, string>(4)
            {
                { IconName.Edit, string.Empty },
                { IconName.Delete, string.Empty },
                { IconName.Sun, string.Empty },
                { IconName.Moon, string.Empty },
            };
        await Parallel.ForEachAsync(iconNames,
            new ParallelOptions { MaxDegreeOfParallelism = 3 },
            async (item, _) => iconNames[item.Key] = await IconFile.ReadIconTextAsync(item.Key));

        WebUI.SvgIconsJson = BHelper.ToJson(iconNames);
    }


    /// <summary>
    /// Updates value of <see cref="LangListJson"/>.
    /// </summary>
    public static void UpdateLangListJson(bool forced = false)
    {
        if (!string.IsNullOrEmpty(LangListJson) && !forced) return;

        var langList = Config.LoadLanguageList();
        LangListJson = BHelper.ToJson(langList.Select(i =>
        {
            var obj = new ExpandoObject();

            var langName = Path.GetFileName(i.FileName);
            if (string.IsNullOrEmpty(langName))
            {
                langName = i.Metadata.EnglishName;
            }

            obj.TryAdd(nameof(i.FileName), langName);
            obj.TryAdd(nameof(i.Metadata), i.Metadata);

            return obj;
        }));
    }


    /// <summary>
    /// Updates value of <see cref="ThemeListJson"/>.
    /// </summary>
    public static void UpdateThemeListJson(bool forced = false)
    {
        if (!string.IsNullOrEmpty(ThemeListJson) && !forced) return;

        var themeList = Config.LoadThemeList();
        ThemeListJson = BHelper.ToJson(themeList.Select(th =>
        {
            th.ReloadThemeColors();
            var obj = new ExpandoObject();

            obj.TryAdd(nameof(th.ConfigFilePath), th.ConfigFilePath);
            obj.TryAdd(nameof(th.FolderName), th.FolderName);
            obj.TryAdd(nameof(th.FolderPath), th.FolderPath);
            obj.TryAdd(nameof(th.Info), th.JsonModel.Info);
            obj.TryAdd(nameof(IgTheme.Colors.BgColor), ThemeUtils.ColorToHex(th.Colors.BgColor));


            // IsDarkMode
            var isDarkMode = true;
            if (th.JsonModel.Settings.TryGetValue(nameof(IgThemeSettings.IsDarkMode), out var darkMode))
            {
                isDarkMode = darkMode.ToString().ToLowerInvariant() != "false";
            }
            obj.TryAdd(nameof(IgThemeSettings.IsDarkMode), isDarkMode);


            // PreviewImage
            var previewImgB64 = "";
            if (th.JsonModel.Settings.TryGetValue(nameof(th.Settings.PreviewImage), out var previewImgName))
            {
                var previewImgPath = Path.Combine(th.FolderPath, previewImgName.ToString());

                // get thumbnail
                using var bmp = ShellThumbnailApi.GetThumbnail(previewImgPath, 256, 256, ShellThumbnailOptions.ThumbnailOnly);

                // convert to base-64
                previewImgB64 = "data:image/png;charset=utf-8;base64," + BHelper.ToBase64Png(bmp);
            }
            obj.TryAdd(nameof(IgThemeSettings.PreviewImage), previewImgB64);

            return obj;
        }));
    }


    /// <summary>
    /// Updates value of <see cref="ToolListJson"/>.
    /// </summary>
    public static void UpdateToolListJson(bool forced = false)
    {
        if (!string.IsNullOrEmpty(ToolListJson) && !forced) return;

        ToolListJson = BHelper.ToJson(Config.Tools);
    }

    #endregion // Public functions

}
