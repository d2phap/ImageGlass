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
using ImageGlass.Common.Localization;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Modal window to create a new custom toolbar button or to view/edit an existing one.
/// </summary>
internal sealed class ToolbarButtonEditWindow : DialogWindow
{
    private readonly bool _isEditMode;
    private readonly bool _isReadOnly;
    private readonly ToolbarButtonEditWindowView _view;


    // fixed dialog width so it doesn't grow/shrink with the entered text
    protected override int MIN_WIDTH => 500;
    protected override int MAX_WIDTH => 500;
    protected override Thickness ContentPadding => new(0);


    /// <summary>
    /// Gets the toolbar button built from the dialog inputs, or <c>null</c> if it wasn't submitted
    /// (cancelled, or a read-only built-in button).
    /// </summary>
    public ToolbarItemModel? ResultModel { get; private set; }


    /// <summary>
    /// Opens the window to create a new custom button or to edit/view an existing one.
    /// </summary>
    public ToolbarButtonEditWindow(ToolbarItemModel? model, ISet<string> takenIds, bool isBuiltIn)
    {
        _isEditMode = model is not null;
        _isReadOnly = isBuiltIn;

        IsButton2Visible = true;
        IsButton3Visible = false;

        // a built-in button is view-only: show a single Close button instead of OK/Cancel
        IsButton1Visible = !_isReadOnly;
        DefaultButton = _isReadOnly ? DialogButton.Button2 : DialogButton.Button1;
        DefaultFocus = _isReadOnly ? DialogFocus.Button2 : DialogFocus.Default;

        _view = new ToolbarButtonEditWindowView();
        _view.LoadData(model, _isReadOnly, takenIds);
        DialogContent = _view;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[_isEditMode
            ? LangId.Settings_Toolbar_EditButton
            : LangId.Settings_Toolbar_AddNewButton];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[_isReadOnly ? LangId._Close : LangId._Cancel];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        // never commit a read-only built-in button
        if (_isReadOnly)
        {
            e.CanProceed = false;
            return;
        }

        if (!_view.Validate())
        {
            e.CanProceed = false;
            return;
        }

        ResultModel = _view.BuildModel();
        base.OnDialogSubmitted(e);
    }

}
