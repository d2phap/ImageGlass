/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Platform.Storage;
using ImageGlass.Common.Localization;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class LanguageSettingsView : SettingsPageView
{
    private const string LANGUAGES_URL = "https://imageglass.org/languages";

    private List<Lang> _langs = [];
    private bool _isPopulating;

    private static FilePickerFileType LangPackFileType => new("ImageGlass language pack")
    {
        Patterns = ["*.iglang.json"],
    };


    public LanguageSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public LanguageSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // display-language list (each item shows the pack metadata)
        PART_LanguageList.SelectionChanged += (_, _) =>
        {
            ShowSelectedContributors();

            if (_isPopulating) return;
            if (PART_LanguageList.SelectedItem is Lang lang)
            {
                VM.SetValue(ConfigId.Language, ToConfigValue(lang));
            }
        };
        RegisterSearchKey(PART_LanguageList, LangId.Settings_DisplayLanguage, ConfigId.Language, null);

        SetLocalizedText(PART_RefreshLanguages, LangId.Settings_Refresh);
        PART_RefreshLanguages.Click += async (_, _) => await ReloadLanguagesAsync();
        RegisterSearchKey(PART_RefreshLanguages, LangId.Settings_Refresh, null, null);

        SetLocalizedText(PART_InstallLanguage, LangId.Settings_InstallNewLanguagePack);
        PART_InstallLanguage.Click += async (_, _) => await InstallLanguagesAsync();
        RegisterSearchKey(PART_InstallLanguage, LangId.Settings_InstallNewLanguagePack, null, null);

        BindLink(PART_GetMoreLanguages, LangId.Settings_GetMoreLanguagePacks, LANGUAGES_URL,
            () => _ = BHelper.OpenUrlAsync(this, LANGUAGES_URL, "from_setting_language"));

        SetLocalizedText(PART_ExportLanguage, LangId.Settings_ExportLanguagePack);
        PART_ExportLanguage.Click += async (_, _) => await ExportLanguageAsync();
        RegisterSearchKey(PART_ExportLanguage, LangId.Settings_ExportLanguagePack, null, null);

        _ = ReloadLanguagesAsync();
    }


    /// <summary>
    /// Reloads installed language packs and reselects the staged (or current) language.
    /// </summary>
    private async Task ReloadLanguagesAsync()
    {
        var packs = await Lang.LoadAllLanguagePacksAsync();

        // built-in English (no pack file, so its path line stays hidden) always first, to revert to
        _langs = [new Lang(string.Empty), .. packs];
        PopulateCombo();
    }


    /// <summary>
    /// Rebinds the combo items and selects the entry matching the staged language value.
    /// </summary>
    private void PopulateCombo()
    {
        _isPopulating = true;

        PART_LanguageList.ItemsSource = _langs;

        var current = VM.GetValue(ConfigId.Language, "English");
        PART_LanguageList.SelectedItem = _langs
            .FirstOrDefault(l => ToConfigValue(l).Equals(current, StringComparison.OrdinalIgnoreCase))
            ?? _langs[0];

        _isPopulating = false;
        ShowSelectedContributors();
    }


    /// <summary>
    /// Shows the contributors of the selected pack; hidden when the pack declares none.
    /// </summary>
    private void ShowSelectedContributors()
    {
        var author = (PART_LanguageList.SelectedItem as Lang)?.Metadata.Author ?? string.Empty;

        PART_Contributors.Text = author;
        PART_ContributorsSection.IsVisible = !string.IsNullOrWhiteSpace(author);
    }


    /// <summary>
    /// Opens a file picker for <c>*.iglang.json</c> packs, installs the compatible ones into the
    /// Config dir, reloads the list, and reports any rejected as incompatible.
    /// </summary>
    private async Task InstallLanguagesAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            FileTypeFilter = [LangPackFileType],
        });

        var paths = files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrEmpty(p))
            .Cast<string>()
            .ToList();
        if (paths.Count == 0) return;

        var result = await Lang.InstallLanguagePacksAsync(paths);
        await ReloadLanguagesAsync();

        // report packs rejected as incompatible
        if (result.IncompatiblePackNames.Count > 0)
        {
            var details = string.Join(Environment.NewLine, result.IncompatiblePackNames.Select(n => $"- {n}"));

            await ModalWindow.ShowErrorAsync(TopLevel.GetTopLevel(this) as PhWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Settings_InstallNewLanguagePack],
                Heading = Core.Lang[LangId._IncompatibleLanguage],
                Description = Core.Lang[LangId._IncompatibleLanguage_Description],
                Details = details,
            });
        }
    }


    /// <summary>
    /// Exports the selected pack as a <c>*.iglang.json</c> file. The built-in entry exports the
    /// default English strings as a translation template.
    /// </summary>
    private async Task ExportLanguageAsync()
    {
        if (PART_LanguageList.SelectedItem is not Lang selected) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = selected.IsBuiltIn ? "English.iglang.json" : selected.FileName,
            FileTypeChoices = [LangPackFileType],
        });

        var path = file?.TryGetLocalPath();
        if (string.IsNullOrEmpty(path)) return;

        // export from a fresh instance so SaveAsFileAsync's English->placeholder rewrite never
        // mutates the metadata of the entry still shown in the combo
        var toExport = selected.IsBuiltIn ? new Lang(string.Empty) { Items = Lang.DefaultLangMap } : selected;
        await toExport.SaveAsFileAsync(path);
    }


    private static string ToConfigValue(Lang lang) => lang.IsBuiltIn ? "English" : lang.FileName;


}
