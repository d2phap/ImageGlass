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
using Avalonia.Layout;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.UI;

public class MessageOverlayControl : PhOverlay
{
    private CancellationTokenSource? _cancelMessage;
    private readonly Lock _lock = new();

    private readonly TextBlock _lblHeading;
    private readonly TextBlock _lblDescription;
    private readonly SelectableTextBlock _lblDetails;
    private readonly Border _detailsBox;

    // set while a message replaces its three parts, so they show together
    private bool _isSettingMessage;


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
        AvaloniaProperty.Register<MessageOverlayControl, string?>(nameof(Heading));


    /// <summary>
    /// Gets, sets the text of modal's description.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<MessageOverlayControl, string?>(nameof(Description));


    /// <summary>
    /// Gets, sets the text of modal's details.
    /// </summary>
    public string? Details
    {
        get => GetValue(DetailsProperty);
        set => SetValue(DetailsProperty, value);
    }
    public static readonly StyledProperty<string?> DetailsProperty =
        AvaloniaProperty.Register<MessageOverlayControl, string?>(nameof(Details));


    /// <summary>
    /// Gets the visibility of heading control.
    /// </summary>
    public bool IsHeadingVisible => !string.IsNullOrWhiteSpace(Heading);
    public static readonly DirectProperty<MessageOverlayControl, bool> IsHeadingVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageOverlayControl, bool>(nameof(IsHeadingVisible), i => i.IsHeadingVisible);


    /// <summary>
    /// Gets the visibility of description control.
    /// </summary>
    public bool IsDescriptionVisible => !string.IsNullOrWhiteSpace(Description);
    public static readonly DirectProperty<MessageOverlayControl, bool> IsDescriptionVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageOverlayControl, bool>(nameof(IsDescriptionVisible), i => i.IsDescriptionVisible);


    /// <summary>
    /// Gets the visibility of details control.
    /// </summary>
    public bool IsDetailsVisible => !string.IsNullOrWhiteSpace(Details);
    public static readonly DirectProperty<MessageOverlayControl, bool> IsDetailsVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageOverlayControl, bool>(nameof(IsDetailsVisible), i => i.IsDetailsVisible);


    /// <summary>
    /// Gets the visibility of this control.
    /// </summary>
    public bool IsMessageVisible => IsHeadingVisible || IsDescriptionVisible || IsDetailsVisible;
    public static readonly DirectProperty<MessageOverlayControl, bool> IsMessageVisibleProperty =
        AvaloniaProperty.RegisterDirect<MessageOverlayControl, bool>(nameof(IsMessageVisible), i => i.IsMessageVisible);

    #endregion // Public Properties



    static MessageOverlayControl()
    {
        // a box in the middle of the viewer, fading in place
        HorizontalAlignmentProperty.OverrideDefaultValue<MessageOverlayControl>(HorizontalAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<MessageOverlayControl>(VerticalAlignment.Center);
        MarginProperty.OverrideDefaultValue<MessageOverlayControl>(new Thickness(20));
        PaddingProperty.OverrideDefaultValue<MessageOverlayControl>(new Thickness(10));
    }


    public MessageOverlayControl()
    {
        _lblHeading = new TextBlock
        {
            FontSize = Const.FONT_SIZE_SUBTITLE,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            [!TextBlock.ForegroundProperty] = Resx.CreateBinding(ResxId.SystemAccentColor),
        };
        _lblDescription = new TextBlock
        {
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
        };
        _lblDetails = new SelectableTextBlock
        {
            Padding = new Thickness(5),
            FontSize = Const.FONT_SIZE_SMALL,
            FontFamily = Const.FONT_CODE,
            FontWeight = Avalonia.Media.FontWeight.SemiLight,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
        _detailsBox = new Border
        {
            Margin = new Thickness(0, 12, 0, 0),
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            [!Border.CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius),
            [!Border.BorderBrushProperty] = Resx.CreateBinding(ResxId.TextControlForeground),
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = _lblDetails,
            },
        };

        Content = CreateContentElement();
    }



    #region Override Methods

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
        else
        {
            return;
        }

        if (_isSettingMessage) return;
        ApplyMessage();
    }

    #endregion // Override Methods



    #region Private methods

    /// <summary>
    /// Creates content element: the heading and description above the scrollable details.
    /// </summary>
    private Grid CreateContentElement()
    {
        // top section
        var topEl = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Top,
            Orientation = Orientation.Vertical,
            Spacing = 12,
        };
        topEl.Children.Add(_lblHeading);
        topEl.Children.Add(_lblDescription);


        // root grid
        Grid.SetRow(topEl, 0);
        Grid.SetRow(_detailsBox, 1);

        var rootEl = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
        };
        rootEl.Children.Add(topEl);
        rootEl.Children.Add(_detailsBox);

        return rootEl;
    }


    /// <summary>
    /// Shows the current message, or fades the box out with its last one, so it never empties while visible.
    /// </summary>
    private void ApplyMessage()
    {
        var isVisible = IsMessageVisible;
        if (isVisible)
        {
            _lblHeading.Text = Heading;
            _lblHeading.IsVisible = IsHeadingVisible;
            _lblDescription.Text = Description;
            _lblDescription.IsVisible = IsDescriptionVisible;
            _lblDetails.Text = Details;
            _detailsBox.IsVisible = IsDetailsVisible;
        }

        IsShown = isVisible;
    }


    /// <summary>
    /// Sets the message.
    /// </summary>
    private void SetMessage(string? message, string? heading = null, string? details = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _isSettingMessage = true;
            try
            {
                Heading = heading;
                Description = message;
                Details = details;
            }
            finally
            {
                _isSettingMessage = false;
            }

            ApplyMessage();
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
