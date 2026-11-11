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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.Tools;
using ImageGlass.UI;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;

public partial class ToolEditWindowView : PhControl
{
    // tool ids already in use (the edited tool's own id excluded) for the uniqueness check
    private ISet<string> _takenIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);


    public ToolEditWindowView()
    {
        InitializeComponent();

        PART_SdkLink.Click += (_, _) =>
            _ = BHelper.OpenUrlAsync(this, "https://github.com/ImageGlass/SDK", "from_tool_settings");
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // "Integrated with {0}" -> label text + "ImageGlass.SDK" link (+ any trailing text)
        var template = Core.Lang[LangId.Settings_Tools_IntegratedWith];
        var idx = template.IndexOf("{0}", System.StringComparison.Ordinal);

        PART_IntegratedBefore.Text = idx < 0 ? template : template[..idx];
        PART_IntegratedAfter.Text = idx < 0 ? string.Empty : template[(idx + 3)..];
        PART_IntegratedAfter.IsVisible = !string.IsNullOrEmpty(PART_IntegratedAfter.Text);
        PART_SdkLink.Text = "ImageGlass.SDK";
    }


    /// <summary>
    /// Loads the given tool into the fields (null means "add new", defaulting the argument to
    /// <see cref="Const.FILE_MACRO"/>). <paramref name="isIdLocked"/> disables the id field.
    /// </summary>
    public void LoadData(ExternalTool? tool, ISet<string> takenIds, bool isIdLocked = false)
    {
        _takenIds = takenIds;

        PART_Id.Text = tool?.ToolId ?? string.Empty;
        PART_Id.IsEnabled = !isIdLocked;
        PART_Name.Text = tool?.ToolName ?? string.Empty;
        PART_Integrated.IsChecked = tool?.IsIntegrated ?? false;

        PART_Action.Executable = tool?.Executable ?? string.Empty;
        PART_Action.Argument = tool?.Arguments ?? Const.FILE_MACRO;
        PART_Action.Hotkeys = tool?.Hotkeys ?? [];

        // clear the eager required-field errors raised by setting Text above so the dialog opens clean
        DataValidationErrors.ClearErrors(PART_Id);
        PART_Action.ClearValidationErrors();
    }


    /// <summary>
    /// Validates the required + unique tool id and the required executable, showing inline errors.
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
                    new ValidationException(Core.Lang[LangId.Settings_Tools_Errors_ToolIdDuplicated, id]));
                idOk = false;
            }
        }

        var exeOk = PART_Action.ValidateExecutable();

        return idOk & exeOk;
    }


    /// <summary>
    /// Builds an external tool from the current (trimmed) field values.
    /// </summary>
    public ExternalTool BuildTool() => new()
    {
        ToolId = PART_Id.Text?.Trim() ?? string.Empty,
        ToolName = PART_Name.Text?.Trim() ?? string.Empty,
        Executable = PART_Action.Executable?.Trim() ?? string.Empty,
        Arguments = PART_Action.Argument?.Trim() ?? string.Empty,
        IsIntegrated = PART_Integrated.IsChecked == true,
        Hotkeys = [.. PART_Action.Hotkeys],
    };

}
