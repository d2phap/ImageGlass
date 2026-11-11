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
using ImageGlass.UI;
using System.Collections.ObjectModel;

namespace ImageGlass.Common.Windows;

public partial class ToolbarSettingsView : SettingsPageView
{
    public ToolbarSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public ToolbarSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Appearance
        BindToggle(PART_ShowToolbarInFullscreen, ConfigId.ShowToolbarInFullscreen,
            LangId.Settings_Toolbar_ShowToolbarInFullscreen, LangId.Settings_Appearance);
        BindUIntSlider(PART_ToolbarIconHeight, ConfigId.ToolbarIconHeight,
            LangId.Settings_Toolbar_ToolbarIconHeight, LangId.Settings_Appearance,
            (uint)Const.TOOLBAR_ICON_HEIGHT, PART_IconHeightLabel);

        // Toolbar buttons
        BuildToolbarButtons();
    }


    /// <summary>
    /// Wires the toolbar button arranger to the staged <see cref="ConfigId.ToolbarButtons"/> value:
    /// loads the current buttons and re-stages on every edit.
    /// </summary>
    private void BuildToolbarButtons()
    {
        var current = VM.GetValue(ConfigId.ToolbarButtons,
            new ObservableCollection<ToolbarItemModel>(Config.DefaultToolbarItems));

        PART_Editor.LoadButtons(current);
        PART_Editor.ButtonsChanged += (_, _) =>
            VM.SetValue(ConfigId.ToolbarButtons, PART_Editor.CurrentButtons);

        RegisterSearchKey(PART_Editor, LangId.Settings_Toolbar_ToolbarButtons,
            ConfigId.ToolbarButtons, LangId.Settings_Toolbar_ToolbarButtons);

        DisableIfLocked(ConfigId.ToolbarButtons, PART_Editor);
    }

}
