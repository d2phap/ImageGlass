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
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Linq;

namespace ImageGlass.Common.Windows;

/// <summary>
/// The content view of <see cref="EditAppWindow"/>: the file-extension, app-name, executable
/// (with a Browse button) and argument fields, plus a live command preview. Owns all field
/// behavior; the hosting window only collects the validated result.
/// </summary>
public partial class EditAppWindowView : PhControl
{
    public EditAppWindowView()
    {
        InitializeComponent();

        // the extension must be ".*" (all) or dot + alphanumeric (".jpg", ".jpg;.png")
        PART_Extension.AcceptValue = TextBoxAcceptValue.FileExtensionsValueOnly;
    }


    /// <summary>
    /// Gets the trimmed file-extension key entered by the user (e.g. <c>.jpg;.png</c>).
    /// </summary>
    public string ResultExtKey => PART_Extension.Text?.Trim() ?? string.Empty;


    /// <summary>
    /// Loads the given app into the fields (defaulting the argument to the <c>&lt;file&gt;</c> macro
    /// for a new app), then refreshes the command preview.
    /// </summary>
    public void LoadData(string? extKey, EditingApp? app)
    {
        PART_Extension.Text = extKey ?? string.Empty;
        PART_AppName.Text = app?.AppName ?? string.Empty;
        PART_Action.Executable = app?.Executable ?? string.Empty;
        PART_Action.Argument = app?.Argument ?? Const.FILE_MACRO;

        // setting Text above doesn't re-validate yet (handlers attach on load); clear the eager
        // errors raised when the required/regex rules were first applied so the window opens clean
        // (validation re-runs as the user edits and on submit)
        DataValidationErrors.ClearErrors(PART_Extension);
        DataValidationErrors.ClearErrors(PART_AppName);
        PART_Action.ClearValidationErrors();
    }


    /// <summary>
    /// Validates the required fields (extension, app name, executable) and shows inline errors.
    /// When the extension is valid it is normalized in place (trimmed, lowercased, de-duplicated).
    /// </summary>
    public bool Validate()
    {
        var extOk = PART_Extension.ValidateAndShowError();
        var nameOk = PART_AppName.ValidateAndShowError();
        var exeOk = PART_Action.ValidateExecutable();

        if (extOk) PART_Extension.Text = NormalizeExtensions(PART_Extension.Text);

        return extOk & nameOk & exeOk;
    }


    /// <summary>
    /// Normalizes an extension key: trims each segment, lowercases it, drops empties/duplicates,
    /// and rejoins with <c>;</c> (e.g. <c>"  .JPG ;  .svg "</c> -> <c>".jpg;.svg"</c>).
    /// </summary>
    private static string NormalizeExtensions(string? raw)
    {
        var segments = (raw ?? string.Empty)
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.ToLowerInvariant())
            .Distinct();

        return string.Join(';', segments);
    }


    /// <summary>
    /// Builds the editing app from the current (trimmed) field values.
    /// </summary>
    public EditingApp BuildApp() => new(
        PART_AppName.Text?.Trim() ?? string.Empty,
        PART_Action.Executable?.Trim() ?? string.Empty,
        PART_Action.Argument?.Trim() ?? string.Empty);

}
