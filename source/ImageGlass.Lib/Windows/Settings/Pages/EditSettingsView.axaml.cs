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
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class EditSettingsView : SettingsPageView
{
    private static readonly FontFamily _codeFont = new(Const.FONT_CODE);

    // working copy of the editing apps (keyed by file-extension string); staged into the VM on change
    private readonly Dictionary<string, EditingApp> _apps = [];


    public EditSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public EditSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Saving
        BindToggle(PART_DeleteConfirmation, ConfigId.EnableDeleteConfirmation,
            LangId.Settings_EnableDeleteConfirmation, LangId.Settings_Edit_Saving, true);
        BindToggle(PART_SaveConfirmation, ConfigId.EnableSaveConfirmation,
            LangId.Settings_EnableSaveConfirmation, LangId.Settings_Edit_Saving, true);
        BindToggle(PART_PreserveModifiedDate, ConfigId.EnablePreserveModifiedDate,
            LangId.Settings_EnablePreserveModifiedDate, LangId.Settings_Edit_Saving);
        BindToggle(PART_OpenSaveAsInCurrentFolder, ConfigId.EnableOpenSaveAsInCurrentFolder,
            LangId.Settings_EnableOpenSaveAsInCurrentFolder, LangId.Settings_Edit_Saving, true);

        BindUIntSlider(PART_ImageEditQuality, ConfigId.ImageEditQuality,
            LangId.Settings_ImageEditQuality, LangId.Settings_Edit_Saving, 80u, PART_ImageEditQualityLabel);

        // Clipboard
        BindToggle(PART_CopyMultipleFiles, ConfigId.EnableCopyMultipleFiles,
            LangId.Settings_EnableCopyMultipleFiles, LangId.Settings_Clipboard, true);
        BindToggle(PART_CutMultipleFiles, ConfigId.EnableCutMultipleFiles,
            LangId.Settings_EnableCutMultipleFiles, LangId.Settings_Clipboard, true);

        // Image editing apps
        BindEnumDropdown(PART_AfterEditingAction, ConfigId.AfterEditingAction, AfterEditAppAction.Nothing,
            LangId.Settings_AfterEditingAction, LangId.Settings_EditApps);

        BuildEditApps();
    }


    /// <summary>
    /// Loads the working copy of the editing apps and wires the Add button + table.
    /// </summary>
    private void BuildEditApps()
    {
        // copy the staged/config value so edits don't mutate the live config before commit
        var stored = VM.GetValue(ConfigId.EditApps, new Dictionary<string, EditingApp?>());
        foreach (var (ext, app) in stored)
        {
            if (app is not null) _apps[ext] = app;
        }

        SetLocalizedText(PART_AddApp, LangId._Add);
        PART_AddApp.Click += async (_, _) => await AddOrEditAppAsync(null);

        // rebuild on language change (also performs the initial render)
        AddLangRefresher(RebuildAppsTable);

        RegisterSearchKey(PART_AddApp, LangId.Settings_EditApps, ConfigId.EditApps, LangId.Settings_EditApps);

        DisableIfLocked(ConfigId.EditApps, PART_AddApp, PART_AppsTable);
    }


    /// <summary>
    /// Stages the current working copy of editing apps into the view model.
    /// </summary>
    private void StageEditApps()
    {
        var value = _apps.ToDictionary(kv => kv.Key, kv => (EditingApp?)kv.Value);
        VM.SetValue(ConfigId.EditApps, value);
    }


    /// <summary>
    /// Opens <see cref="EditAppWindow"/> to add a new app (when <paramref name="extKey"/> is null)
    /// or edit an existing one, then updates the working copy and re-renders.
    /// </summary>
    private async Task AddOrEditAppAsync(string? extKey)
    {
        var existing = extKey is not null ? _apps.GetValueOrDefault(extKey) : null;
        var window = new EditAppWindow(extKey, existing);

        if (await window.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) != DialogExitCode.OK) return;
        if (string.IsNullOrEmpty(window.ResultExtKey)) return;

        // editing may rename the extension key -> drop the old entry first
        if (extKey is not null) _apps.Remove(extKey);
        _apps[window.ResultExtKey] = window.ResultApp;

        StageEditApps();
        RebuildAppsTable();
    }


    /// <summary>
    /// Removes an app from the working copy and re-renders.
    /// </summary>
    private void DeleteApp(string extKey)
    {
        if (!_apps.Remove(extKey)) return;

        StageEditApps();
        RebuildAppsTable();
    }


    /// <summary>
    /// Rebuilds the editing-apps table from the working copy (header + one row per app,
    /// each with Edit/Delete actions). Shows the empty note when there are no apps.
    /// </summary>
    private void RebuildAppsTable()
    {
        PhTableColumn[] columns =
        [
            new() { Header = Core.Lang[LangId._FileExtension] },
            new() { Header = Core.Lang[LangId.Settings_EditApps_AppName] },
            new() { Header = Core.Lang[LangId._Executable], Star = true },
            new() { Header = Core.Lang[LangId._Argument] },
        ];

        // sorted by extension; the ".*" catch-all is forced to the bottom
        var extKeys = _apps.Keys
            .OrderBy(IsWildcardKey) // false (specific) sorts before true (catch-all)
            .ThenBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rows = new List<PhTableRow>(extKeys.Count);
        foreach (var extKey in extKeys)
        {
            var app = _apps[extKey];
            var key = extKey; // capture for the action closures
            rows.Add(new PhTableRow
            {
                Cells =
                [
                    PhTableControl.TextCell(extKey, maxWidth: 160, selectable: true, font: _codeFont),
                    AppNameCell(key, app),
                    PhTableControl.TextCell(app.Executable, selectable: true),
                    string.IsNullOrEmpty(app.Argument)
                        ? PhTableControl.TextCell(Core.Lang[LangId._Empty], muted: true)
                        : PhTableControl.TextCell(app.Argument, maxWidth: 180, selectable: true),
                ],
                Actions =
                [
                    new() { Icon = ResxIconId.IconEdit, Tooltip = Core.Lang[LangId._Edit], Click = () => _ = AddOrEditAppAsync(key) },
                    new() { Icon = ResxIconId.IconClose, Tooltip = Core.Lang[LangId._Delete], Click = () => DeleteApp(key) },
                ],
            });
        }

        PART_AppsTable.EmptyText = Core.Lang[LangId._Empty];
        PART_AppsTable.Build(columns, rows);
    }


    /// <summary>
    /// The app-name cell: a link button showing the app name (capped); clicking it opens the edit dialog.
    /// </summary>
    private Control AppNameCell(string extKey, EditingApp app)
    {
        var displayName = string.IsNullOrWhiteSpace(app.AppName) ? extKey : app.AppName;

        var btn = new PhButton
        {
            Variant = PhButtonVariant.Link,
            Text = displayName,
            MaxWidth = 160,
            HorizontalAlignment = HorizontalAlignment.Left,
            ClipToBounds = true,
        };
        ToolTip.SetTip(btn, displayName);
        btn.Click += (_, _) => _ = AddOrEditAppAsync(extKey);

        return PhTableControl.WrapCell(btn);
    }


    /// <summary>
    /// Whether an extension key includes the <c>.*</c> catch-all segment.
    /// </summary>
    private static bool IsWildcardKey(string extKey) => extKey
        .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Contains(EditingApp.ALL_EXTENSIONS);

}
