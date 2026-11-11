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
using ImageGlass.Tools;
using ImageGlass.UI.Windowing;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Modal window to register a new external tool or to edit an existing one. The fields live in
/// <see cref="ToolEditWindowView"/>; this window adds the OK/Cancel buttons and exposes the result.
/// </summary>
internal sealed class ToolEditWindow : DialogWindow
{
    private readonly bool _isEditMode;
    private readonly ToolEditWindowView _view;


    // fixed dialog width so it doesn't grow/shrink with the entered text
    protected override int MIN_WIDTH => 500;
    protected override int MAX_WIDTH => 500;


    /// <summary>
    /// Gets the external tool built from the dialog inputs, or <c>null</c> if it wasn't submitted.
    /// </summary>
    public ExternalTool? ResultTool { get; private set; }


    /// <summary>
    /// Opens the window to register a new tool (when <paramref name="tool"/> is null) or edit an
    /// existing one. <paramref name="isIdLocked"/> makes the tool id read-only (pre-configured tools).
    /// </summary>
    public ToolEditWindow(ExternalTool? tool, ISet<string> takenIds, bool isIdLocked = false)
    {
        _isEditMode = tool is not null;

        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Default;

        _view = new ToolEditWindowView();
        _view.LoadData(tool, takenIds, isIdLocked);
        DialogContent = _view;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[_isEditMode
            ? LangId.Settings_Tools_EditTool
            : LangId.Settings_Tools_AddNewTool];
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

        ResultTool = _view.BuildTool();
        base.OnDialogSubmitted(e);
    }

}
