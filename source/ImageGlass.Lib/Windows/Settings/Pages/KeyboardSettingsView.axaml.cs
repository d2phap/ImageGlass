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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;

public partial class KeyboardSettingsView : SettingsPageView
{
    public KeyboardSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public KeyboardSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        var current = VM.GetValue(ConfigId.MenuHotkeys, new Dictionary<LangId, Hotkey[]>());

        PART_Editor.LoadHotkeys(current);
        PART_Editor.HotkeysChanged += (_, _) =>
            VM.SetValue(ConfigId.MenuHotkeys, PART_Editor.CurrentHotkeys);

        RegisterSearchKey(PART_Editor, LangId.Settings_Keyboard_MenuHotkeys,
            ConfigId.MenuHotkeys, LangId.Settings_Keyboard_MenuHotkeys);

        DisableIfLocked(ConfigId.MenuHotkeys, PART_Editor);
    }

}
