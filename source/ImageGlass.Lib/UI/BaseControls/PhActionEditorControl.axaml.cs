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
using Avalonia.Platform.Storage;
using ImageGlass.Common;
using ImageGlass.Common.Actions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGlass.UI;

/// <summary>
/// Editor for <see cref="SingleAction"/> and <see cref="HotkeySingleAction"/>
/// </summary>
public partial class PhActionEditorControl : PhControl
{

    #region Public properties

    /// <summary>
    /// Gets, sets the executable (an <c>IG_</c> method, a menu item name, or a file path).
    /// </summary>
    public string? Executable
    {
        get => PART_Executable.Text;
        set => PART_Executable.Text = value;
    }


    /// <summary>
    /// Gets, sets the argument string (supports the <c>&lt;file&gt;</c> macro).
    /// </summary>
    public string? Argument
    {
        get => PART_Argument.Text;
        set => PART_Argument.Text = value;
    }


    /// <summary>
    /// Gets a copy of the recorded hotkeys, or replaces the whole set.
    /// </summary>
    public IReadOnlyList<Hotkey> Hotkeys
    {
        get => PART_Hotkeys.Hotkeys;
        set => PART_Hotkeys.Hotkeys = value;
    }


    /// <summary>
    /// Gets, sets whether the hotkey section is shown.
    /// </summary>
    public bool ShowHotkeys
    {
        get => PART_HotkeysSection.IsVisible;
        set => PART_HotkeysSection.IsVisible = value;
    }


    /// <summary>
    /// Gets, sets the placeholder shown in the executable field.
    /// </summary>
    public string? ExecutablePlaceholder
    {
        get => PART_Executable.PlaceholderText;
        set => PART_Executable.PlaceholderText = value;
    }


    /// <summary>
    /// Gets, sets the placeholder shown in the argument field.
    /// </summary>
    public string? ArgumentPlaceholder
    {
        get => PART_Argument.PlaceholderText;
        set => PART_Argument.PlaceholderText = value;
    }


    /// <summary>
    /// Gets, sets whether the executable field is required. Defaults to <c>true</c>.
    /// </summary>
    public bool IsExecutableRequired
    {
        get => PART_Executable.IsRequired;
        set => PART_Executable.IsRequired = value;
    }

    #endregion // Public properties


    public PhActionEditorControl()
    {
        InitializeComponent();

        // keep the command preview in sync with the executable + argument
        PART_Executable.TextChanged += (_, _) => UpdatePreview();
        PART_Argument.TextChanged += (_, _) => UpdatePreview();
        PART_Browse.Click += async (_, _) => await BrowseExecutableAsync();

        UpdatePreview();
    }



    #region Override Methods

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        ToolTip.SetTip(PART_Browse, Core.Lang[LangId._Browse]);
        PART_Hotkeys.PlaceholderText = Core.Lang[LangId.Settings_Toolbar_RecordHotkeyHint];
    }

    #endregion // Override Methods



    #region Public methods

    /// <summary>
    /// Loads the action's executable, argument and hotkeys; clears eager validation errors.
    /// </summary>
    public void LoadAction(SingleAction? action)
    {
        Executable = action?.Executable ?? string.Empty;
        Argument = action?.Argument ?? string.Empty;
        Hotkeys = (action as HotkeySingleAction)?.Hotkeys ?? [];

        UpdatePreview();
        ClearValidationErrors();
    }


    /// <summary>
    /// Validates the (optionally required) executable field and shows an inline error.
    /// </summary>
    public bool ValidateExecutable() => PART_Executable.ValidateAndShowError();


    /// <summary>
    /// Clears the eager validation error on the executable field.
    /// </summary>
    public void ClearValidationErrors() => DataValidationErrors.ClearErrors(PART_Executable);

    #endregion // Public methods



    #region Private methods

    private void UpdatePreview()
    {
        PART_CommandPreview.Executable = PART_Executable.Text;
        PART_CommandPreview.Argument = PART_Argument.Text;
    }


    /// <summary>
    /// Opens a file picker to choose the action executable.
    /// </summary>
    private async Task BrowseExecutableAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
        });

        var path = (files.Count > 0 ? files[0] : null)?.TryGetLocalPath();
        if (string.IsNullOrEmpty(path)) return;

        Executable = path;
    }

    #endregion // Private methods

}
