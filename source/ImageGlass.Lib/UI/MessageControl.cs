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
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.UI;

public class MessageControl : PhControl
{
    private CancellationTokenSource? _cancelMessage;
    private readonly Lock _lock = new();
    private PhButton? _btnCopy;


    #region Public Properties

    /// <summary>
    /// Gets, sets the text of modal's heading.
    /// </summary>
    public string? Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }
    public static readonly StyledProperty<string?> HeadingProperty =
        AvaloniaProperty.Register<MessageControl, string?>(nameof(Heading));


    /// <summary>
    /// Gets, sets the text of modal's description.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<MessageControl, string?>(nameof(Description));


    /// <summary>
    /// Gets, sets the text of modal's details.
    /// </summary>
    public string? Details
    {
        get => GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }
    public static readonly StyledProperty<string?> DetailsProperty =
        AvaloniaProperty.Register<MessageControl, string?>(nameof(Details));


    /// <summary>
    /// Gets the visibility of heading control.
    /// </summary>
    public bool IsHeadingVisible => !string.IsNullOrWhiteSpace(Heading);
    public static readonly DirectProperty<MessageControl, bool> IsHeadingVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageControl, bool>(nameof(IsHeadingVisible), i => i.IsHeadingVisible);


    /// <summary>
    /// Gets the visibility of description control.
    /// </summary>
    public bool IsDescriptionVisible => !string.IsNullOrWhiteSpace(Description);
    public static readonly DirectProperty<MessageControl, bool> IsDescriptionVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageControl, bool>(nameof(IsDescriptionVisible), i => i.IsDescriptionVisible);


    /// <summary>
    /// Gets the visibility of details control.
    /// </summary>
    public bool IsDetailsVisible => !string.IsNullOrWhiteSpace(Details);
    public static readonly DirectProperty<MessageControl, bool> IsDetailsVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageControl, bool>(nameof(IsDetailsVisible), i => i.IsDetailsVisible);


    /// <summary>
    /// Gets the visibility of this control.
    /// </summary>
    public bool IsMessageVisible => IsHeadingVisible || IsDescriptionVisible || IsDetailsVisible;
    public static readonly DirectProperty<MessageControl, bool> IsMessageVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageControl, bool>(nameof(IsMessageVisible), i => i.IsMessageVisible);

    #endregion // Public Properties



    public MessageControl()
    {
        Content = CreateContentElement();
    }



    #region Override Methods

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        _btnCopy?.Text = Core.Lang[LangId._Copy];
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == HeadingProperty)
        {
            RaisePropertyChanged(IsHeadingVisibleProperty, default, IsHeadingVisible);
            RaisePropertyChanged(IsMessageVisibleProperty, default, IsMessageVisible);
        }
        else if (e.Property == DescriptionProperty)
        {
            RaisePropertyChanged(IsDescriptionVisibleProperty, default, IsDescriptionVisible);
            RaisePropertyChanged(IsMessageVisibleProperty, default, IsMessageVisible);
        }
        else if (e.Property == DetailsProperty)
        {
            RaisePropertyChanged(IsDetailsVisibleProperty, default, IsDetailsVisible);
            RaisePropertyChanged(IsMessageVisibleProperty, default, IsMessageVisible);
        }
    }

    #endregion // Override Methods



    #region Private methods

    /// <summary>
    /// Creates content element.
    /// </summary>
    private Border CreateContentElement()
    {
        // top section
        var topEl = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Vertical,
            Spacing = 12,
        };
        var lblHeading = new TextBlock
        {
            FontSize = Const.FONT_SIZE_SUBTITLE,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            [!TextBlock.ForegroundProperty] = Resx.CreateBinding(ResxId.SystemAccentColor),
            [!TextBlock.TextProperty] = this[!HeadingProperty],
            [!TextBlock.IsVisibleProperty] = this[!IsHeadingVisibleProperty],
        };
        var lblDescription = new SelectableTextBlock
        {
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            [!TextBlock.TextProperty] = this[!DescriptionProperty],
            [!TextBlock.IsVisibleProperty] = this[!IsDescriptionVisibleProperty],
        };
        _btnCopy = new PhButton
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Focusable = false,
            IconData = Resx.GetIcon(ResxIconId.IconCopy),
            [!PhButton.IsVisibleProperty] = this[!IsDetailsVisibleProperty],
        };
        _btnCopy.Click += BtnCopy_Click;

        topEl.Children.Add(lblHeading);
        topEl.Children.Add(lblDescription);
        topEl.Children.Add(_btnCopy);


        // bottom section
        var bottomEl = new Border
        {
            Margin = new Thickness(0, 12, 0, 0),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            [!ScrollViewer.CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius),
            [!ScrollViewer.BorderBrushProperty] = Resx.CreateBinding(ResxId.TextControlForeground),
            [!ScrollViewer.IsVisibleProperty] = this[!IsDetailsVisibleProperty],
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = new SelectableTextBlock
                {
                    Padding = new Thickness(5),
                    FontSize = Const.FONT_SIZE_SMALL,
                    FontFamily = Const.FONT_CODE,
                    FontWeight = Avalonia.Media.FontWeight.SemiLight,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = this[!DetailsProperty],
                },
            },
        };


        // root grid
        Grid.SetRow(topEl, 0);
        Grid.SetRow(bottomEl, 1);

        var rootWrapperEl = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
        };
        rootWrapperEl.Children.Add(topEl);
        rootWrapperEl.Children.Add(bottomEl);

        var rootEl = new Border
        {
            Margin = new Thickness(20),
            Padding = new Thickness(10),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = rootWrapperEl,
            [!Border.CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius),
            [!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_MessageBackgroundBrush),
            [!Border.IsVisibleProperty] = this[!IsMessageVisibleProperty],

            // short messages (e.g. loading) must not block clicks on the viewer
            [!Border.IsHitTestVisibleProperty] = this[!IsDetailsVisibleProperty],
        };

        // stop text selection from also panning or triggering viewer click actions
        rootEl.PointerPressed += (_, e) => e.Handled = true;
        rootEl.PointerReleased += (_, e) => e.Handled = true;

        return rootEl;
    }


    private void BtnCopy_Click(object? sender, RoutedEventArgs e)
    {
        var text = string.Join(Environment.NewLine + Environment.NewLine,
            new[] { Heading, Description, Details }.Where(i => !string.IsNullOrWhiteSpace(i)));

        _ = TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
    }


    /// <summary>
    /// Sets the message.
    /// </summary>
    private void SetMessage(string? message, string? heading = null, string? details = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Heading = heading;
            Description = message;
            Details = details;
        }, DispatcherPriority.Render);
    }

    #endregion // Private methods



    #region Public Methods

    /// <summary>
    /// Shows message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="heading"></param>
    /// <param name="details"></param>
    /// <param name="durationMs">The duration to display (ms). <c>0</c> = permanent.</param>
    /// <param name="delayMs">The delay time before showing (ms). Default = <c>0</c>.</param>
    public async Task ShowAsync(string? message, string? heading = null, string? details = null,
        int? durationMs = null, int delayMs = 0)
    {
        // 1. if intent to clear message
        var clearMessage = message == null && heading == null && details == null;
        if (clearMessage)
        {
            lock (_lock)
            {
                _cancelMessage?.Cancel();
                _cancelMessage?.Dispose();
                _cancelMessage = null; // do not allocate new CTS
            }

            SetMessage(null);
            return;
        }


        // 2. if intent to show message
        CancellationTokenSource localCancelMessage;
        lock (_lock)
        {
            _cancelMessage?.Cancel();
            _cancelMessage?.Dispose();

            _cancelMessage = new CancellationTokenSource();
            localCancelMessage = _cancelMessage;
        }

        var token = localCancelMessage.Token;


        try
        {
            // wait for the delay
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, token);
            }

            // check if this message was superseded by a newer ShowAsync call
            if (token.IsCancellationRequested) return;

            SetMessage(message, heading, details);


            // clear text after duration
            if (!string.IsNullOrWhiteSpace(message))
            {
                durationMs ??= Core.Config.InAppMessageDuration;

                if (durationMs > 0)
                {
                    await Task.Delay(durationMs.Value, token);
                    SetMessage(null);
                }
            }
        }
        catch { }
    }


    /// <summary>
    /// Clears current message.
    /// </summary>
    public async Task ClearAsync()
    {
        await ShowAsync(null);
    }

    #endregion // Public Methods


}
