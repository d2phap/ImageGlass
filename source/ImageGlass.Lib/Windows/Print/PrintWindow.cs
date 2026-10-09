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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;

namespace ImageGlass.Common.Windows;


/// <summary>
/// The Print window: printer, paper and layout on the left, the live preview of every page on the right.
/// </summary>
public sealed class PrintWindow : DialogWindow
{
    protected override int MIN_WIDTH => 0;
    protected override int MAX_WIDTH => int.MaxValue;
    protected override Thickness ContentPadding => new(0);

    private const double DEFAULT_WIDTH = 1000;
    private const double DEFAULT_HEIGHT = 680;
    private const double MIN_RESTORE_WIDTH = 640;
    private const double MIN_RESTORE_HEIGHT = 420;

    private readonly PrintWindowView _view;
    private readonly PhButton _systemDialogLink;
    private readonly TextBlock _summary;


    /// <summary>
    /// Gets the message the main window shows once the job is done; <c>null</c> when nothing was printed.
    /// </summary>
    public string? CompletedMessage { get; private set; }


    public PrintWindow(PrintSession session)
    {
        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;

        // the window has text inputs, so Enter must not print
        PressEnterToSubmit = false;

        CanResize = true;
        CanMaximize = true;
        CanMinimize = false;
        SizeToContent = SizeToContent.Manual;
        MinWidth = MIN_RESTORE_WIDTH;
        MinHeight = MIN_RESTORE_HEIGHT;

        RestoreWindowBounds(Core.Config.PrintWindowBounds,
            new(DEFAULT_WIDTH, DEFAULT_HEIGHT),
            new(MIN_RESTORE_WIDTH, MIN_RESTORE_HEIGHT));
        if (Core.Config.EnablePrintWindowMaximized) WindowState = WindowState.Maximized;

        _view = new PrintWindowView(session);
        _view.StateChanged += View_StateChanged;
        DialogContent = _view;

        // the footer: the platform's own dialog, and how many pages and prints
        _systemDialogLink = new PhButton
        {
            Variant = PhButtonVariant.Link,
            Padding = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = Core.PrintProvider.CanShowSystemDialog,
        };
        _systemDialogLink.Click += SystemDialogLink_Click;

        _summary = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
        };

        DialogFooterLeftContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24,
            Children = { _systemDialogLink, _summary },
        };
    }


    #region Overrides

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Print_Title];
        Button2Text = Core.Lang[LangId._Cancel];
        _systemDialogLink.Text = Core.Lang[LangId.Print_BtnSystemDialog];
        View_StateChanged();
    }


    protected override async void OnDialogSubmitted(DialogEventArgs e)
    {
        try
        {
            if (!e.CanProceed || _view.IsPrinting) return;

            var printer = _view.Printer;
            if (!await _view.PrintAsync(this)) return;

            // a printer's driver writes even a file after the job is handed over, so only the app's own PDF is already saved
            CompletedMessage = printer?.Id == PrintProviderBase.PDF_PRINTER_ID
                ? Core.Lang[LangId.Print_Saved]
                : Core.Lang[LangId.Print_Sent, printer?.DisplayName];

            base.OnDialogSubmitted(e);
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(this, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Print_Title],
                Description = ex.Message,
                Details = BHelper.GetExceptionDetails(ex),
            });
        }
    }


    protected override void OnDialogCancelled(DialogEventArgs e)
    {
        // Cancel stops a running job first; the next one closes
        if (_view.IsPrinting)
        {
            _view.CancelPrinting();
            return;
        }

        base.OnDialogCancelled(e);
    }


    protected override void OnDialogAborted()
    {
        if (_view.IsPrinting)
        {
            _view.CancelPrinting();
            return;
        }

        base.OnDialogAborted();
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        // turn the pages of the preview
        if (e.Key == Key.PageDown || e.Key == Key.PageUp)
        {
            _view.GoToPage(e.Key == Key.PageDown ? 1 : -1);
            e.Handled = true;
        }
    }


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // a closing window cannot leave a job behind
        if (_view.IsPrinting)
        {
            _view.CancelPrinting();
            e.Cancel = true;
            return;
        }

        // bounds while the native window still lives; a maximized size is never the restore size
        Core.Config.EnablePrintWindowMaximized = RestorableWindowState == WindowState.Maximized;
        if (WindowedBounds is { } bounds
            && bounds.Width >= MIN_RESTORE_WIDTH
            && bounds.Height >= MIN_RESTORE_HEIGHT)
        {
            Core.Config.PrintWindowBounds = bounds;
        }

        _view.SaveConfig();
        _view.Close();

        base.OnClosing(e);
    }

    #endregion // Overrides


    private void View_StateChanged()
    {
        Button1Text = _view.Printer?.OutputFileExtension is not null
            ? Core.Lang[LangId.Print_BtnSave]
            : Core.Lang[LangId.Print_BtnPrint];

        _btn1.IsEnabled = _view.CanPrint;
        _systemDialogLink.IsEnabled = !_view.IsPrinting;
        _summary.Text = _view.SummaryText;
    }


    private async void SystemDialogLink_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (_view.IsPrinting) return;
            if (!await _view.ShowSystemDialogAsync(this)) return;

            // the platform's dialog takes it from here
            DialogResult = DialogExitCode.Cancel;
            Close(DialogResult);
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(this, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Print_Title],
                Description = ex.Message,
                Details = BHelper.GetExceptionDetails(ex),
            });
        }
    }
}
