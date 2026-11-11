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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.UI.Windowing;

public partial class DialogWindow : PhWindow
{
    protected virtual int MIN_WIDTH => 400;
    protected virtual int MAX_WIDTH => 600;
    protected virtual Thickness ContentPadding => new(24, 14, 24, 20);


    protected Grid _contentEl;
    protected Border _footerEl;
    protected PhButton _btn1;
    protected PhButton _btn2;
    protected PhButton _btn3;

    protected TaskCompletionSource<DialogExitCode> _taskSourceExitCode = new(TaskCreationOptions.RunContinuationsAsynchronously);


    #region Control Properties

    /// <summary>
    /// Gets, sets the dialog content.
    /// </summary>
    public object DialogContent
    {
        get => GetValue(DialogContentProperty);
        set => SetValue(DialogContentProperty, value);
    }
    public static readonly StyledProperty<object> DialogContentProperty =
        AvaloniaProperty.Register<DialogWindow, object>(nameof(DialogContent));


    /// <summary>
    /// Gets, sets the content for left section of dialog footer.
    /// </summary>
    public object DialogFooterLeftContent
    {
        get => GetValue(DialogFooterLeftContentProperty);
        set => SetValue(DialogFooterLeftContentProperty, value);
    }
    public static readonly StyledProperty<object> DialogFooterLeftContentProperty =
        AvaloniaProperty.Register<DialogWindow, object>(nameof(DialogFooterLeftContent));


    /// <summary>
    /// Gets the visibility of title bar.
    /// </summary>
    public bool IsTitleVisible => !string.IsNullOrWhiteSpace(Title) && BHelper.OS != OSType.Linux;
    public static readonly DirectProperty<DialogWindow, bool> IsTitleVisibleProperty =
        AvaloniaProperty.RegisterDirect<DialogWindow, bool>(nameof(IsTitleVisible), i => i.IsTitleVisible);


    /// <summary>
    /// Gets, sets the button 1 text.
    /// </summary>
    public string Button1Text
    {
        get => GetValue(Button1TextProperty);
        set => SetValue(Button1TextProperty, value);
    }
    public static readonly StyledProperty<string> Button1TextProperty =
        AvaloniaProperty.Register<DialogWindow, string>(nameof(Button1Text), "[Button 1]");


    /// <summary>
    /// Gets, sets the button 2 text.
    /// </summary>
    public string Button2Text
    {
        get => GetValue(Button2TextProperty);
        set => SetValue(Button2TextProperty, value);
    }
    public static readonly StyledProperty<string> Button2TextProperty =
        AvaloniaProperty.Register<DialogWindow, string>(nameof(Button2Text), "[Button 2]");


    /// <summary>
    /// Gets, sets the button 3 text.
    /// </summary>
    public string Button3Text
    {
        get => GetValue(Button3TextProperty);
        set => SetValue(Button3TextProperty, value);
    }
    public static readonly StyledProperty<string> Button3TextProperty =
        AvaloniaProperty.Register<DialogWindow, string>(nameof(Button3Text), "[Button 3]");


    /// <summary>
    /// Gets, sets the icon of button 1; <c>null</c> shows text only.
    /// </summary>
    public Geometry? Button1Icon
    {
        get => GetValue(Button1IconProperty);
        set => SetValue(Button1IconProperty, value);
    }
    public static readonly StyledProperty<Geometry?> Button1IconProperty =
        AvaloniaProperty.Register<DialogWindow, Geometry?>(nameof(Button1Icon));


    /// <summary>
    /// Gets, sets the icon of button 2; <c>null</c> shows text only.
    /// </summary>
    public Geometry? Button2Icon
    {
        get => GetValue(Button2IconProperty);
        set => SetValue(Button2IconProperty, value);
    }
    public static readonly StyledProperty<Geometry?> Button2IconProperty =
        AvaloniaProperty.Register<DialogWindow, Geometry?>(nameof(Button2Icon));


    /// <summary>
    /// Gets, sets the icon of button 3; <c>null</c> shows text only.
    /// </summary>
    public Geometry? Button3Icon
    {
        get => GetValue(Button3IconProperty);
        set => SetValue(Button3IconProperty, value);
    }
    public static readonly StyledProperty<Geometry?> Button3IconProperty =
        AvaloniaProperty.Register<DialogWindow, Geometry?>(nameof(Button3Icon));


    /// <summary>
    /// Gets, sets the visibility of button 1.
    /// </summary>
    public bool IsButton1Visible
    {
        get => GetValue(IsButton1VisibleProperty);
        set => SetValue(IsButton1VisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsButton1VisibleProperty =
        AvaloniaProperty.Register<DialogWindow, bool>(nameof(IsButton1Visible), true);


    /// <summary>
    /// Gets, sets the visibility of button 2.
    /// </summary>
    public bool IsButton2Visible
    {
        get => GetValue(IsButton2VisibleProperty);
        set => SetValue(IsButton2VisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsButton2VisibleProperty =
        AvaloniaProperty.Register<DialogWindow, bool>(nameof(IsButton2Visible), false);


    /// <summary>
    /// Gets, sets the visibility of button 3.
    /// </summary>
    public bool IsButton3Visible
    {
        get => GetValue(IsButton3VisibleProperty);
        set => SetValue(IsButton3VisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsButton3VisibleProperty =
        AvaloniaProperty.Register<DialogWindow, bool>(nameof(IsButton3Visible), false);


    /// <summary>
    /// Gets, sets the default button of dialog.
    /// </summary>
    public DialogButton DefaultButton { get; set; } = DialogButton.Button1;


    /// <summary>
    /// Gets, sets the default focus of dialog.
    /// </summary>
    public DialogFocus DefaultFocus { get; set; } = DialogFocus.Default;


    /// <summary>
    /// Gets, sets the value indicates that pression ENTER key to submit the window.
    /// </summary>
    public bool PressEnterToSubmit { get; set; } = true;


    /// <summary>
    /// Gets or sets the result for the dialog.
    /// </summary>
    public DialogExitCode DialogResult { get; set; } = DialogExitCode.None;

    #endregion // Control Properties



    public DialogWindow()
    {
        CanResize = false;
        ShowInTaskbar = false;
        CanMinimize = false;

        SizeToContent = SizeToContent.WidthAndHeight;
        BackdropStyle = BHelper.OS == OSType.Windows ? BackdropStyle.MicaAlt : BackdropStyle.None;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CloseWindowHotkeys = [new(Avalonia.Input.Key.Escape)];

        Content = CreateContentElement();
        _ = UpdateWindowIconAsync();
    }



    #region Window Events

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ApplyTheme();
        SetDefaultButton(DefaultButton);

        // Note: need a delay so that pressing Space key won't hit the focused button
        await Task.Delay(200);
        SetDefaultFocus();
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // if the dialog is closed unexpected, returns Abort code to break the while loop.
        if (DialogResult == DialogExitCode.None) DialogResult = DialogExitCode.Abort;

        // set the result to complete the task
        _ = _taskSourceExitCode.TrySetResult(DialogResult);

        // reactivate the owner window
        Owner?.Activate();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        ApplyTheme();
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!PressEnterToSubmit) return;

        // press Enter to submit
        var hk = new Hotkey(e.KeyModifiers, e.Key);
        if (hk.IsSame(Key.Enter))
        {
            e.Handled = true;

            // execute the action of the current default button
            if (DefaultButton == DialogButton.Button1)
            {
                Button1_Click(_btn1, e);
            }
            else if (DefaultButton == DialogButton.Button2)
            {
                Button2_Click(_btn2, e);
            }
            else if (DefaultButton == DialogButton.Button3)
            {
                Button3_Click(_btn3, e);
            }
            else
            {
                OnDialogSubmitted(new DialogEventArgs(DialogAction.Submit));
            }
        }
    }


    protected override void OnIgCloseWindowHotkeyPressed(KeyEventArgs e)
    {
        base.OnIgCloseWindowHotkeyPressed(e);

        e.Handled = true;
        OnDialogAborted();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == TitleProperty)
        {
            RaisePropertyChanged(IsTitleVisibleProperty, default, IsTitleVisible);
        }
    }


    /// <summary>
    /// Creates layout and content for dialog window.
    /// </summary>
    [MemberNotNull(
        nameof(_contentEl),
        nameof(_footerEl),
        nameof(_btn1),
        nameof(_btn2),
        nameof(_btn3))]
    protected Grid CreateContentElement()
    {
        // 1. create content slot
        // stretch the content so resizable dialogs (e.g. Settings) can fill/shrink and
        // let their own scrollers take over instead of overflowing and centering.
        var dialogContentSlot = new ContentControl
        {
            Padding = ContentPadding,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            [!ContentControl.ContentProperty] = this[!DialogContentProperty],
        };
        _contentEl = new Grid();
        _contentEl.Children.Add(dialogContentSlot);



        // 2. create footer
        // 2.1 left footer
        var footerLeftSlot = new ContentControl
        {
            [!ContentControl.ContentProperty] = this[!DialogFooterLeftContentProperty],
        };


        // 2.2 right footer
        _btn1 = new PhButton
        {
            MinWidth = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            [!PhButton.TextProperty] = this[!Button1TextProperty],
            [!PhButton.IconDataProperty] = this[!Button1IconProperty],
            [!PhButton.IsVisibleProperty] = this[!IsButton1VisibleProperty],
        };
        _btn2 = new PhButton
        {
            MinWidth = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            [!PhButton.TextProperty] = this[!Button2TextProperty],
            [!PhButton.IconDataProperty] = this[!Button2IconProperty],
            [!PhButton.IsVisibleProperty] = this[!IsButton2VisibleProperty],
        };
        _btn3 = new PhButton
        {
            MinWidth = 100,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            [!PhButton.TextProperty] = this[!Button3TextProperty],
            [!PhButton.IconDataProperty] = this[!Button3IconProperty],
            [!PhButton.IsVisibleProperty] = this[!IsButton3VisibleProperty],
        };
        _btn1.Click += Button1_Click;
        _btn2.Click += Button2_Click;
        _btn3.Click += Button3_Click;
        var footerRightPanel = new StackPanel
        {
            Spacing = 8,
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        footerRightPanel.KeyDown += FooterContent_KeyDown;
        footerRightPanel.Children.AddRange([_btn1, _btn2, _btn3]);

        var footerWrapper = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            Margin = new Thickness(24, 17, 24, 18),
            ColumnSpacing = 20,
        };
        Grid.SetColumn(footerLeftSlot, 0);
        Grid.SetColumn(footerRightPanel, 1);
        footerWrapper.Children.AddRange([footerLeftSlot, footerRightPanel]);

        _footerEl = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            Child = footerWrapper,
        };


        // 3. create root content
        var root = new Grid
        {
            MinWidth = MIN_WIDTH,
            MaxWidth = MAX_WIDTH,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*, Auto"),
        };
        Grid.SetRow(_contentEl, 0);
        Grid.SetRow(_footerEl, 1);

        root.Children.Add(_contentEl);
        root.Children.Add(_footerEl);

        return root;
    }


    private void FooterContent_KeyDown(object? sender, KeyEventArgs e)
    {
        var isMoveNext = e.Key == Key.Up || e.Key == Key.Right;
        var isMoveBack = e.Key == Key.Down || e.Key == Key.Left;
        if (!isMoveNext && !isMoveBack) return;
        if (sender is not StackPanel panel) return;


        // get visible footer buttons
        var visibleButtons = panel.Children
            .OfType<Button>()
            .Where(i => i.IsVisible)
            .ToList();
        if (visibleButtons.Count == 0) return;

        // get focused button and its index
        var focusedIndex = -1;
        if (FocusManager?.GetFocusedElement() is Button focusedButton)
        {
            focusedIndex = visibleButtons.IndexOf(focusedButton);
        }


        // if no button has focus yet => focus the first one
        if (focusedIndex == -1)
        {
            visibleButtons[0].Focus(NavigationMethod.Tab);
        }
        else
        {
            if (isMoveNext)
            {
                // wrap around
                focusedIndex = (focusedIndex + 1) % visibleButtons.Count;
            }
            else if (isMoveBack)
            {
                // wrap around
                focusedIndex = (focusedIndex - 1 + visibleButtons.Count) % visibleButtons.Count;
            }

            visibleButtons[focusedIndex].Focus(NavigationMethod.Tab);
        }

        e.Handled = true;
    }


    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        OnDialogSubmitted(new DialogEventArgs(DialogAction.Submit));
    }


    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        OnDialogCancelled(new DialogEventArgs(DialogAction.Cancel));
    }


    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        OnDialogApplied(new DialogEventArgs(DialogAction.Apply));
    }


    #endregion // Window Events



    #region Virtual methods

    /// <summary>
    /// Closes the form and returns <see cref="DialogExitCode.Abort"/> code.
    /// </summary>
    protected virtual void OnDialogAborted()
    {
        DialogResult = DialogExitCode.Abort;
        Close(DialogResult);
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogExitCode.OK"/> code.
    /// </summary>
    protected virtual void OnDialogSubmitted(DialogEventArgs e)
    {
        if (!e.CanProceed) return;
        DialogResult = DialogExitCode.OK;
        Close(DialogResult);
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogExitCode.Cancel"/> code.
    /// </summary>
    protected virtual void OnDialogCancelled(DialogEventArgs e)
    {
        if (!e.CanProceed) return;
        DialogResult = DialogExitCode.Cancel;
        Close(DialogResult);
    }


    /// <summary>
    /// Sets the dialog result to <see cref="DialogExitCode.None"/>
    /// and does nothing.
    /// </summary>
    protected virtual void OnDialogApplied(DialogEventArgs e)
    {
        if (!e.CanProceed) return;
        DialogResult = DialogExitCode.None;
    }


    #endregion // Virtual methods



    #region Methods

    /// <summary>
    /// Updates background according to theme color.
    /// </summary>
    protected void ApplyTheme()
    {
        var isDarkMode = Core.Theme.Settings.IsDarkMode;
        var bg = AppThemeColors.BgBrush.Color.NoAlpha();

        // content bg
        var contentAlpha = Math.Max(isDarkMode ? 180 : 220, bg.A);
        var contentBg = bg.WithAlpha(contentAlpha);
        _contentEl.Background = contentBg.ToBrush();

        // footer bg
        var footerAlpha = _canUseBackdrop ? Math.Max(180, contentAlpha / 2) : contentAlpha;
        var footerBg = bg
            .WithBrightness(isDarkMode ? 0.075f : -0.075f)
            .WithAlpha(footerAlpha);
        _footerEl.Background = footerBg.ToBrush();

        // footer border
        _footerEl.BorderBrush = footerBg
            .WithBrightness(isDarkMode ? 0.075f : -0.075f)
            .ToBrush();
    }


    /// <summary>
    /// Sets the default button style.
    /// </summary>
    protected void SetDefaultButton(DialogButton btn)
    {
        // Only make the button the Enter-key default when submission via Enter is allowed.
        // Avalonia routes Enter to any IsDefault button regardless of OnKeyDown, so this
        // is what actually honors PressEnterToSubmit = false.
        if (btn == DialogButton.Button1)
        {
            _btn1.IsDefault = PressEnterToSubmit;
            _btn1.Variant = PhButtonVariant.Accent;
        }
        else if (btn == DialogButton.Button2)
        {
            _btn2.IsDefault = PressEnterToSubmit;
            _btn2.Variant = PhButtonVariant.Accent;
        }
        else if (btn == DialogButton.Button3)
        {
            _btn3.IsDefault = PressEnterToSubmit;
            _btn3.Variant = PhButtonVariant.Accent;
        }
    }


    /// <summary>
    /// Sets the default focused button.
    /// </summary>
    protected void SetDefaultFocus()
    {
        if (DefaultFocus == DialogFocus.Button1)
        {
            _btn1.Focus(Avalonia.Input.NavigationMethod.Tab);
        }
        else if (DefaultFocus == DialogFocus.Button2)
        {
            _btn2.Focus(Avalonia.Input.NavigationMethod.Tab);
        }
        else if (DefaultFocus == DialogFocus.Button3)
        {
            _btn3.Focus(Avalonia.Input.NavigationMethod.Tab);
        }
    }


    /// <summary>
    /// Shows dialog.
    /// </summary>
    public async Task<DialogExitCode> ShowAsync(PhWindow? owner)
    {
        _taskSourceExitCode = new TaskCompletionSource<DialogExitCode>();

        // Only show as a modal child when the owner is actually visible.
        // During early startup the owner may not be shown yet on some backends.
        if (owner is not null && owner.IsVisible)
        {
            await ShowDialog(owner);
        }
        else
        {
            Show();
        }


        // wait for exit code
        var exitCode = await _taskSourceExitCode.Task;

        return exitCode;
    }

    #endregion // Methods


}
