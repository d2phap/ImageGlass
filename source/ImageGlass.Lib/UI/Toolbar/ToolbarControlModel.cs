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
using Avalonia;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;

namespace ImageGlass.UI;

public class ToolbarControlModel : PhReactive
{
    public static Config Config => Core.Config;
    public static double ItemSpacing => Core.Config.ToolbarIconHeight / 6f; // 4
    public static Thickness ItemPadding => ToolbarItemModel.ItemPadding;


    // Text holds the language key, not the translated string, so it re-resolves on language change
    public ToolbarItemModel ButtonOverflowVM { get; } = new()
    {
        Text = nameof(LangId.Menu_MnuToolbarOverflow),
        Image = nameof(IgThemeIcon.MainMenu),
    };


    public ToolbarItemModel ButtonMenuVM
    {
        get; set
        {
            if (field == value) return;

            field = value;
            _ = OnPropertyChanged();
        }
    } = new()
    {
        Text = nameof(LangId.Menu_MnuMain),
        Image = nameof(IgThemeIcon.MainMenu),
    };


    /// <summary>
    /// Re-resolves the localized text of the menu buttons after the app language changed.
    /// </summary>
    public void RefreshLanguage()
    {
        ButtonMenuVM.RefreshLanguage();
        ButtonOverflowVM.RefreshLanguage();
    }

}
