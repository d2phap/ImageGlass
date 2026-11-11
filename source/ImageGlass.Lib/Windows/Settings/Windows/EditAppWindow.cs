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
using ImageGlass.UI.Windowing;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Modal window to create or edit a single <see cref="EditingApp"/> entry (the file extensions it
/// handles plus the app name, executable and argument). The fields live in
/// <see cref="EditAppWindowView"/>; this window adds the OK/Cancel buttons and exposes the result.
/// </summary>
internal sealed class EditAppWindow : DialogWindow
{
    private readonly bool _isEditMode;
    private readonly EditAppWindowView _view;


    // fixed dialog width so it doesn't grow/shrink with the executable path or preview text
    protected override int MIN_WIDTH => 500;
    protected override int MAX_WIDTH => 500;


    /// <summary>
    /// Gets the file-extension key entered by the user (e.g. <c>.jpg;.png</c>).
    /// </summary>
    public string ResultExtKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the editing app built from the dialog inputs.
    /// </summary>
    public EditingApp ResultApp { get; private set; } = new();


    /// <summary>
    /// Opens the window to create a new app, or to edit an existing one when
    /// <paramref name="extKey"/> / <paramref name="app"/> are supplied.
    /// </summary>
    public EditAppWindow(string? extKey = null, EditingApp? app = null)
    {
        _isEditMode = !string.IsNullOrEmpty(extKey);

        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Default;

        _view = new EditAppWindowView();
        _view.LoadData(extKey, app);
        DialogContent = _view;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[_isEditMode
            ? LangId.Settings_EditAppDialog_EditApp
            : LangId.Settings_EditAppDialog_AddApp];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        if (!_view.Validate())
        {
            e.CanProceed = false;
            return;
        }

        ResultExtKey = _view.ResultExtKey;
        ResultApp = _view.BuildApp();

        base.OnDialogSubmitted(e);
    }

}
