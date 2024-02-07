/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
    /// Gets all SVG icons.
    /// </summary>
    public static Dictionary<IconName, string>? SvgIcons { get; internal set; } = null;


    /// <summary>
    /// Gets all language packs.
    /// </summary>
    public static IEnumerable<ExpandoObject>? LangList { get; internal set; } = null;


    /// <summary>
    /// Gets all theme packs.
    /// </summary>
    public static IEnumerable<ExpandoObject>? ThemeList { get; internal set; } = null;


    /// <summary>
    /// Gets all enums.
    /// </summary>
    public static ExpandoObject Enums
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
                typeof(ImageInfoUpdateTypes),
            };

            foreach (var item in enums)
            {
                var keys = Enum.GetNames(item);
                _ = enumObj.TryAdd(item.Name, keys);
            }

            return enumObj;
        }
    }

    #endregion // Public properties



    /// <summary>
    /// Updates value of <see cref="SvgIcons"/>.
    /// </summary>
    public static async Task UpdateSvgIconsAsync(bool forced = false)
    {
        if (SvgIcons != null && !forced) return;

        var iconNames = new Dictionary<IconName, string>()
        {
            { IconName.Edit, string.Empty },
            { IconName.Delete, string.Empty },
            { IconName.ArrowUp, string.Empty },
            { IconName.ArrowDown, string.Empty },
            { IconName.ArrowLeft, string.Empty },
            { IconName.ArrowRight, string.Empty },
            { IconName.ArrowExchange, string.Empty },
            { IconName.Sun, string.Empty },
            { IconName.Moon, string.Empty },
        };


        await Parallel.ForEachAsync(iconNames,
            new ParallelOptions { MaxDegreeOfParallelism = 3 },
            async (item, _) => iconNames[item.Key] = await IconFile.ReadIconTextAsync(item.Key));

        SvgIcons = iconNames;
    }


    /// <summary>
    /// Updates value of <see cref="LangList"/>.
    /// </summary>
    public static void UpdateLangListJson(bool forced = false)
    {
        if (LangList != null && !forced) return;

        LangList = Config.LoadLanguageList().Select(i =>
        {
            var obj = new ExpandoObject();

            _ = obj.TryAdd(nameof(i.FileName), i.FileName);
            _ = obj.TryAdd(nameof(i.Metadata), i.Metadata);

            return obj;
        });
    }


    /// <summary>
    /// Updates value of <see cref="ThemeList"/>.
    /// </summary>
    public static void UpdateThemeListJson(bool forced = false)
    {
        if (ThemeList != null && !forced) return;

        var thList = Config.LoadThemeList();
        ThemeList = thList.Select(th =>
        {
            th.LoadThemeColors();
            var obj = new ExpandoObject();

            _ = obj.TryAdd(nameof(th.ConfigFilePath), th.ConfigFilePath);
            _ = obj.TryAdd(nameof(th.FolderName), th.FolderName);
            _ = obj.TryAdd(nameof(th.FolderPath), th.FolderPath);
            _ = obj.TryAdd(nameof(th.Info), th.JsonModel.Info);
            _ = obj.TryAdd(nameof(IgTheme.Colors.BgColor), th.Colors.BgColor.ToHex());


            // IsDarkMode
            var isDarkMode = true;
            if (th.JsonModel.Settings.TryGetValue(nameof(IgThemeSettings.IsDarkMode), out var darkMode))
            {
                isDarkMode = !darkMode.ToString().Equals("false", StringComparison.InvariantCultureIgnoreCase);
            }
            _ = obj.TryAdd(nameof(IgThemeSettings.IsDarkMode), isDarkMode);


            // PreviewImage
            if (th.JsonModel.Settings.TryGetValue(nameof(th.Settings.PreviewImage), out var previewImgName))
            {
                var previewImgPath = Path.Combine(th.FolderPath, previewImgName.ToString());
                var previewImgUri = new Uri(previewImgPath);

                _ = obj.TryAdd(nameof(IgThemeSettings.PreviewImage), previewImgUri.AbsoluteUri);
            }


            // Toolbar buttons icon path
            var buttons = new ExpandoObject();
            foreach (var item in th.JsonModel.ToolbarIcons)
            {
                var iconFileUrl = th.GetToolbarIconFilePath(item.Key);
                if (!string.IsNullOrWhiteSpace(iconFileUrl))
                {
                    iconFileUrl = new Uri(iconFileUrl).AbsoluteUri;
                }

                _ = buttons.TryAdd(item.Key, iconFileUrl);
            }
            _ = obj.TryAdd(nameof(th.JsonModel.ToolbarIcons), buttons);

            return obj;
        });
    }

}
