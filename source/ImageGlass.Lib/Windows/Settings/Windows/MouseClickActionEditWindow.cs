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
using ImageGlass.Common.Actions;
using ImageGlass.Common.Localization;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Modal window to edit the <see cref="SingleAction"/> bound to a single mouse click event,
/// using a <see cref="PhActionEditorControl"/>. An empty executable means "do nothing".
/// </summary>
internal sealed class MouseClickActionEditWindow : DialogWindow
{
    private readonly PhActionEditorControl _editor;
    private readonly string _eventLabel;
    private readonly bool _isReadOnly;

    protected override int MIN_WIDTH => 460;
    protected override int MAX_WIDTH => 460;


    /// <summary>
    /// Gets the action built from the dialog inputs, or <c>null</c> if cancelled or read-only.
    /// </summary>
    public SingleAction? ResultAction { get; private set; }


    /// <summary>
    /// Opens the editor for <paramref name="action"/> (null = empty), titled by <paramref name="eventLabel"/>.
    /// </summary>
    public MouseClickActionEditWindow(string eventLabel, SingleAction? action, bool isReadOnly = false)
    {
        _eventLabel = eventLabel;
        _isReadOnly = isReadOnly;

        IsButton2Visible = true;
        IsButton3Visible = false;

        // view-only: show a single Close button instead of OK/Cancel
        IsButton1Visible = !_isReadOnly;
        DefaultButton = _isReadOnly ? DialogButton.Button2 : DialogButton.Button1;
        DefaultFocus = _isReadOnly ? DialogFocus.Button2 : DialogFocus.Default;

        // optional action: no hotkeys, allow an empty executable
        _editor = new PhActionEditorControl
        {
            ShowHotkeys = false,
            IsExecutableRequired = false,
            IsEnabled = !_isReadOnly,
        };
        _editor.LoadAction(action);
        DialogContent = _editor;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = $"{_eventLabel} – {Core.Lang[LangId.Settings_MouseClickAction]}";
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[_isReadOnly ? LangId._Close : LangId._Cancel];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        // never commit a view-only action
        if (_isReadOnly)
        {
            e.CanProceed = false;
            return;
        }

        if (!_editor.ValidateExecutable())
        {
            e.CanProceed = false;
            return;
        }

        ResultAction = new SingleAction(
            _editor.Executable?.Trim() ?? string.Empty,
            _editor.Argument?.Trim() ?? string.Empty);

        base.OnDialogSubmitted(e);
    }

}
