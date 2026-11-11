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
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImageGlass.Common.Localization;
using ImageGlass.UI.Windowing;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class SettingsWindow : DialogWindow
{
    private readonly SettingsViewModel _vm = new();
    private readonly SettingsWindowView _viewEl;
    private readonly string? _targetConfigId;
    private readonly string? _editToolId;
    private bool _editToolShown;


    protected override int MIN_WIDTH => 0;
    protected override int MAX_WIDTH => int.MaxValue;
    protected override Thickness ContentPadding => new(0);


    private const double DEFAULT_WIDTH = 900;
    private const double DEFAULT_HEIGHT = 580;
    private const double MIN_RESTORE_WIDTH = 200;
    private const double MIN_RESTORE_HEIGHT = 100;



    public SettingsWindow(string? targetConfigId = null, string? editToolId = null)
    {
        _targetConfigId = targetConfigId;
        _editToolId = editToolId;

        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = true;

        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Default;
        ShowInTaskbar = true;
        PressEnterToSubmit = false;

        // resizable window
        CanResize = true;
        CanMinimize = true;
        CanMaximize = true;
        SizeToContent = SizeToContent.Manual;
        // no min size: let the window be resized as small as the OS allows

        // restore window size & position
        RestoreWindowBounds(Core.Config.SettingsWindowBounds,
            new(DEFAULT_WIDTH, DEFAULT_HEIGHT),
            new(MIN_RESTORE_WIDTH, MIN_RESTORE_HEIGHT));

        // restore window maximized state
        if (Core.Config.EnableSettingsWindowMaximized) WindowState = WindowState.Maximized;

        _viewEl = new SettingsWindowView(_vm);
        DialogContent = _viewEl;
    }



    #region Override Methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // a specific setting was requested -> jump to it (overrides the restored page)
        if (!string.IsNullOrWhiteSpace(_targetConfigId))
        {
            _viewEl.NavigateToConfig(_targetConfigId);
        }

        // a tool edit was requested -> open it once (NavigateToTool defers the child dialog itself)
        if (!_editToolShown && !string.IsNullOrWhiteSpace(_editToolId))
        {
            _editToolShown = true;
            _viewEl.NavigateToTool(_editToolId);
        }

        // focus the search box on open
        _viewEl.FocusSearch();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Menu_MnuSettings];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];
        Button3Text = Core.Lang[LangId._Apply];
    }


    protected override async void OnDialogSubmitted(DialogEventArgs e)
    {
        if (!e.CanProceed) return;

        await CommitAndReportAsync();
        base.OnDialogSubmitted(e);
    }


    protected override async void OnDialogApplied(DialogEventArgs e)
    {
        if (!e.CanProceed) return;

        await CommitAndReportAsync();
        base.OnDialogApplied(e);
    }


    /// <summary>
    /// Commits the staged edits, reporting a failed write; the values stay applied for this session.
    /// </summary>
    private async Task CommitAndReportAsync()
    {
        if (await _vm.CommitAsync()) return;

        await ModalWindow.ShowSettingsSaveErrorAsync(this, Core.Lang[LangId.Menu_MnuSettings]);
    }


    protected override void OnDialogCancelled(DialogEventArgs e)
    {
        _vm.Discard();
        base.OnDialogCancelled(e);
    }


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // capture bounds here (not OnClosed) — the native window is still alive,
        // so Position/ClientSize are valid; by OnClosed they read (0,0).

        // save state regardless of how the dialog was closed; the pre-minimize state keeps it maximized
        Core.Config.EnableSettingsWindowMaximized = RestorableWindowState == WindowState.Maximized;

        // the tracked windowed bounds, so a maximized size is never stored as the restore size,
        // while a session that ends maximized still records the monitor it was on
        if (WindowedBounds is { } bounds
            && bounds.Width >= MIN_RESTORE_WIDTH
            && bounds.Height >= MIN_RESTORE_HEIGHT)
        {
            Core.Config.SettingsWindowBounds = bounds;
        }

        _ = Core.Config.SaveAsync();

        base.OnClosing(e);
    }


    protected override void OnDialogAborted()
    {
        _vm.Discard();
        base.OnDialogAborted();
    }

    #endregion // Override Methods



    #region Public Methods

    /// <summary>
    /// Navigates the settings window to the page hosting the given config id and scrolls to it.
    /// </summary>
    public void NavigateToConfig(string? configId) => _viewEl.NavigateToConfig(configId);


    /// <summary>
    /// Navigates to the Tools page and opens the add/edit dialog for the given tool id.
    /// </summary>
    public void NavigateToTool(string? toolId) => _viewEl.NavigateToTool(toolId);

    #endregion // Public Methods


}
