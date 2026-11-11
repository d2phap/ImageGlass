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
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using ImageGlass.Common.Actions;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class ToolbarButtonEditWindowView : PhControl
{
    /// <summary>
    /// One entry in the icon picker: a theme icon, or <c>null</c> for the "Custom…" (SVG path) option.
    /// </summary>
    private sealed record IconOption(IgThemeIcon? Icon, string Display);


    // icon picker options (index 0 is the "Custom…" entry)
    private List<IconOption> _imageOptions = [];

    // button IDs already in use (the edited button's own ID excluded) for the uniqueness check
    private ISet<string> _takenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


    public ToolbarButtonEditWindowView()
    {
        InitializeComponent();

        PopulateImageOptions();
        PART_Image.SelectionChanged += (_, _) => UpdateCustomImageRow();
        PART_EnableConfigBinding.IsCheckedChanged += (_, _) => UpdateConfigBindingVisibility();
        PART_BrowseImage.Click += async (_, _) => await BrowseImageAsync();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        ToolTip.SetTip(PART_BrowseImage, Core.Lang[LangId._Browse]);
    }


    #region Load / build

    /// <summary>
    /// Loads the given button into the fields (a null model means "add new"), selecting its theme
    /// icon or falling back to the "Custom…" path. When <paramref name="isReadOnly"/> is set (built-in
    /// button) the fields are disabled and a note is shown. <paramref name="takenIds"/> drives the
    /// id-uniqueness check on submit.
    /// </summary>
    public void LoadData(ToolbarItemModel? model, bool isReadOnly, ISet<string> takenIds)
    {
        _takenIds = takenIds;

        PART_Id.Text = model?.Id ?? string.Empty;
        PART_Text.Text = model?.Text ?? string.Empty;
        PART_ShowText.IsChecked = model?.ShowText ?? false;
        PART_AlignRight.IsChecked = model?.Alignment == ToolbarItemAlignment.Right;
        PART_ConfigBinding.Text = model?.ConfigBinding ?? string.Empty;
        PART_ConfigBindingValue.Text = model?.ConfigBindingValue ?? string.Empty;
        PART_EnableConfigBinding.IsChecked = !string.IsNullOrWhiteSpace(model?.ConfigBinding);
        PART_Action.LoadAction(model?.OnClick);

        SelectImage(model?.Image);
        UpdateConfigBindingVisibility();

        // built-in buttons are read-only
        PART_Fields.IsEnabled = !isReadOnly;
        PART_ReadonlyNote.IsVisible = isReadOnly;

        // setting Text above eagerly raised the required-field errors; clear them so the dialog
        // opens clean (validation re-runs as the user edits and on submit)
        DataValidationErrors.ClearErrors(PART_Id);
        DataValidationErrors.ClearErrors(PART_CustomImagePath);
    }


    /// <summary>
    /// Validates the required + unique id, the required executable, and (when "Custom…" is selected)
    /// the icon path, showing inline errors.
    /// </summary>
    public bool Validate()
    {
        var idOk = PART_Id.ValidateAndShowError();
        if (idOk)
        {
            var id = PART_Id.Text?.Trim() ?? string.Empty;
            if (_takenIds.Contains(id))
            {
                DataValidationErrors.SetError(PART_Id,
                    new ValidationException(Core.Lang[LangId.Settings_Toolbar_Errors_ButtonIdDuplicated, id]));
                idOk = false;
            }
        }

        var exeOk = PART_Action.ValidateExecutable();

        // the icon path is only relevant for the "Custom…" option
        var imgOk = (PART_Image.SelectedItem as IconOption)?.Icon is not null
            || PART_CustomImagePath.ValidateAndShowError();

        return idOk & exeOk & imgOk;
    }


    /// <summary>
    /// Builds a toolbar button from the current (trimmed) field values.
    /// </summary>
    public ToolbarItemModel BuildModel()
    {
        var selected = PART_Image.SelectedItem as IconOption;
        var image = selected?.Icon is { } icon
            ? icon.ToString()
            : PART_CustomImagePath.Text?.Trim() ?? string.Empty;

        var text = PART_Text.Text?.Trim() ?? string.Empty;

        // the config (toggle) binding only applies when the checkbox is on
        var bindEnabled = PART_EnableConfigBinding.IsChecked == true;

        var action = new HotkeySingleAction
        {
            Executable = PART_Action.Executable?.Trim() ?? string.Empty,
            Argument = PART_Action.Argument?.Trim() ?? string.Empty,
            Hotkeys = [.. PART_Action.Hotkeys],
            LangKey = text,
        };

        return new ToolbarItemModel
        {
            Id = PART_Id.Text?.Trim() ?? string.Empty,
            Image = image,
            Text = text,
            ShowText = PART_ShowText.IsChecked == true,
            ConfigBinding = bindEnabled ? PART_ConfigBinding.Text?.Trim() ?? string.Empty : string.Empty,
            ConfigBindingValue = bindEnabled ? PART_ConfigBindingValue.Text?.Trim() ?? string.Empty : string.Empty,
            Alignment = PART_AlignRight.IsChecked == true
                ? ToolbarItemAlignment.Right
                : ToolbarItemAlignment.Left,
            OnClick = action,
        };
    }


    /// <summary>
    /// Shows the config name/value inputs only when the toggle-binding checkbox is on.
    /// </summary>
    private void UpdateConfigBindingVisibility()
        => PART_ConfigBindingInputs.IsVisible = PART_EnableConfigBinding.IsChecked == true;

    #endregion // Load / build


    #region Icon picker

    /// <summary>
    /// Populates the icon picker: the "Custom…" entry first, then every theme icon except the app logo.
    /// </summary>
    private void PopulateImageOptions()
    {
        _imageOptions = [new IconOption(null, Core.Lang[LangId.Settings_Toolbar_CustomIcon])];
        foreach (var icon in Enum.GetValues<IgThemeIcon>())
        {
            if (icon == IgThemeIcon.AppLogo) continue;
            _imageOptions.Add(new IconOption(icon, icon.ToString()));
        }

        PART_Image.ItemsSource = _imageOptions;
        PART_Image.ItemTemplate = new FuncDataTemplate<IconOption>((opt, _) =>
            opt is null ? null : BuildIconOptionVisual(opt));
    }


    /// <summary>
    /// Selects the picker entry matching the given image: a theme icon (enum name, app logo excluded),
    /// otherwise the "Custom…" entry with the raw value shown in the path box.
    /// </summary>
    private void SelectImage(string? image)
    {
        if (!string.IsNullOrWhiteSpace(image)
            && Enum.TryParse<IgThemeIcon>(image, out var icon)
            && icon != IgThemeIcon.AppLogo
            && _imageOptions.FirstOrDefault(o => o.Icon == icon) is { } match)
        {
            PART_Image.SelectedItem = match;
            return;
        }

        // custom path (or empty/new button): the "Custom…" entry reveals the path row
        PART_Image.SelectedIndex = 0;
        PART_CustomImagePath.Text = image ?? string.Empty;
    }


    private void UpdateCustomImageRow()
    {
        PART_CustomImageRow.IsVisible = (PART_Image.SelectedItem as IconOption)?.Icon is null;
    }


    /// <summary>
    /// Builds the icon-picker row visual: the SVG preview (when available) plus the display name.
    /// Uses the <c>Svg</c> control (not <c>Image</c>+<c>SvgImage</c>) so the disabled-state
    /// <c>Svg.CurrentCss</c> opacity reaches the glyph (SvgImage ignores composited Opacity).
    /// </summary>
    private static StackPanel BuildIconOptionVisual(IconOption opt)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (opt.Icon is { } icon
            && new ToolbarItemModel { Image = icon.ToString() }.ImagePath is { Length: > 0 } path)
        {
            // baseUri is unused for the rooted ImagePath, but the Svg ctor requires one
            panel.Children.Add(new Avalonia.Svg.Skia.Svg(new Uri("file:///"))
            {
                Width = 18,
                Height = 18,
                Path = path,
            });
        }

        panel.Children.Add(new TextBlock
        {
            Text = opt.Display,
            VerticalAlignment = VerticalAlignment.Center,
        });

        return panel;
    }


    /// <summary>
    /// Opens a file picker to choose a custom SVG icon.
    /// </summary>
    private async Task BrowseImageAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("SVG") { Patterns = ["*.svg"] }],
        });

        var path = (files.Count > 0 ? files[0] : null)?.TryGetLocalPath();
        if (string.IsNullOrEmpty(path)) return;

        PART_CustomImagePath.Text = path;
    }

    #endregion // Icon picker

}
