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
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ImageGlass.UI.Viewer;

/// <summary>
/// Shows the image info in a floating bar on the viewer, where <see cref="Config.ImageInfoOverlayMode"/> allows it.
/// </summary>
public class ImageInfoOverlay : PhOverlay
{
    // auto-hide shows it while the pointer is within this distance of the bar, more once shown so it cannot flicker
    private const double REVEAL_ZONE_EXTRA = 10;
    private const double KEEP_ZONE_EXTRA = 48;
    private static readonly TimeSpan HIDE_DELAY = TimeSpan.FromMilliseconds(450);
    private static readonly TimeSpan PEEK_DURATION = TimeSpan.FromMilliseconds(2200);

    // tabular digits keep the bar from twitching while the zoom level or the frame number counts
    private static readonly FontFeatureCollection TABULAR_DIGITS = new() { new FontFeature { Tag = "tnum" } };

    private readonly TextBlock _textBlock;
    private readonly DispatcherTimer _hideTimer = new() { Interval = HIDE_DELAY };
    private readonly DispatcherTimer _peekTimer = new() { Interval = PEEK_DURATION };
    private TopLevel? _topLevel;

    private List<ImageInfoItem> _items = [];
    private bool _isTextDirty;
    private bool _isHovered;
    private bool _isPeeking;
    private double _coveredHeight;


    static ImageInfoOverlay()
    {
        // a bar a little below the top edge, centered and clear of the sides
        HorizontalAlignmentProperty.OverrideDefaultValue<ImageInfoOverlay>(HorizontalAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<ImageInfoOverlay>(VerticalAlignment.Top);
        MarginProperty.OverrideDefaultValue<ImageInfoOverlay>(new Thickness(20, 8, 20, 0));
        PaddingProperty.OverrideDefaultValue<ImageInfoOverlay>(new Thickness(12, 4));
        TransitionDirectionProperty.OverrideDefaultValue<ImageInfoOverlay>(PhOverlayDirection.Top);

        // never in the way of the viewer's own input
        IsHitTestVisibleProperty.OverrideDefaultValue<ImageInfoOverlay>(false);
    }


    public ImageInfoOverlay()
    {
        _textBlock = new TextBlock
        {
            FontSize = Const.FONT_SIZE_BODY,
            FontFeatures = TABULAR_DIGITS,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Content = _textBlock;

        _hideTimer.Tick += HideTimer_Tick;
        _peekTimer.Tick += PeekTimer_Tick;
    }



    #region Public Properties & Methods

    /// <summary>
    /// Gets whether the window hides its title bar, so the image info shows nowhere else.
    /// </summary>
    public static bool IsTitleBarHidden
    {
        get
        {
            var isFullScreen = Core.Config.EnableFullScreen;
            if (isFullScreen) return true;

            var isFrameless = Core.Config.EnableFrameless;
            return isFrameless;
        }
    }


    /// <summary>
    /// Gets how far into the viewer the bar may reach from the edge it sits on, which other overlays keep clear of; 0 while it cannot show.
    /// </summary>
    public double CoveredHeight
    {
        get => _coveredHeight;
        private set => SetAndRaise(CoveredHeightProperty, ref _coveredHeight, value);
    }
    public static readonly DirectProperty<ImageInfoOverlay, double> CoveredHeightProperty =
        AvaloniaProperty.RegisterDirect<ImageInfoOverlay, double>(nameof(CoveredHeight), o => o.CoveredHeight);


    /// <summary>
    /// Sets the image info to show, in the order of <see cref="Config.ImageInfoTags"/>.
    /// </summary>
    public void SetItems(IReadOnlyList<ImageInfoItem> items)
    {
        // the bar is about the image, while the app name only tells the window apart
        var imageItems = items.Where(item => item.Tag != nameof(AppStatusInfo.AppName)).ToList();

        var isSame = imageItems.SequenceEqual(_items);
        if (isSame) return;

        _items = imageItems;
        _isTextDirty = true;

        RefreshText();
        UpdateVisibility();
    }

    #endregion // Public Properties & Methods



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // the window sees the pointer everywhere, also beside the viewer, e.g. on a toolbar at the top
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(PointerMovedEvent, TopLevel_PointerMoved, RoutingStrategies.Tunnel, true);
        _topLevel?.AddHandler(PointerPressedEvent, TopLevel_PointerPressed, RoutingStrategies.Tunnel, true);
        _topLevel?.PointerExited += TopLevel_PointerExited;

        Core.Config.PropertyChanged += Config_PropertyChanged;

        UpdateVisibility();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _topLevel?.RemoveHandler(PointerMovedEvent, TopLevel_PointerMoved);
        _topLevel?.RemoveHandler(PointerPressedEvent, TopLevel_PointerPressed);
        _topLevel?.PointerExited -= TopLevel_PointerExited;
        _topLevel = null;

        Core.Config.PropertyChanged -= Config_PropertyChanged;

        _hideTimer.Stop();
        _peekTimer.Stop();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // the text is colored by the theme too
        _isTextDirty = true;
        RefreshText();
    }


    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        // the estimated height gives way to the laid out one
        if (e.HeightChanged) UpdateCoveredHeight();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var isPlacementChanged = change.Property == MarginProperty
            || change.Property == PaddingProperty
            || change.Property == VerticalAlignmentProperty;
        if (isPlacementChanged) UpdateCoveredHeight();
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var isRelevant = e.PropertyName is nameof(Config.ImageInfoOverlayMode)
            or nameof(Config.EnableFullScreen)
            or nameof(Config.EnableFrameless)
            or nameof(Config.EnableSlideshow);
        if (!isRelevant) return;

        // a mode switch starts over, so a stale hover cannot keep the bar up
        var isModeChanged = e.PropertyName is nameof(Config.ImageInfoOverlayMode)
            or nameof(Config.EnableSlideshow);

        Dispatcher.UIThread.Post(() =>
        {
            if (isModeChanged)
            {
                _isHovered = false;
                _hideTimer.Stop();
            }

            UpdateVisibility();
        });
    }


    private void TopLevel_PointerMoved(object? sender, PointerEventArgs e)
    {
        // a finger has no hover to end, so its taps peek instead
        var isTouch = e.Pointer.Type == PointerType.Touch;
        if (isTouch) return;

        var isWaitingForPointer = IsAutoHideActive();
        if (!isWaitingForPointer) return;

        var isInZone = IsInRevealZone(e);
        if (isInZone)
        {
            _hideTimer.Stop();
            if (_isHovered) return;

            _isHovered = true;
            UpdateVisibility();
            return;
        }

        // leaving the zone hides it a moment later, so a brief slip does not flicker it
        if (!_isHovered) return;
        if (_hideTimer.IsEnabled) return;

        _hideTimer.Start();
    }


    private void TopLevel_PointerExited(object? sender, PointerEventArgs e)
    {
        // e.g. leaving a frameless window over its top edge
        if (!_isHovered) return;

        _hideTimer.Start();
    }


    private void TopLevel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // a touch has no hover, so a tap near the bar shows it for a moment
        var isTouch = e.Pointer.Type == PointerType.Touch;
        if (!isTouch) return;

        var isWaitingForPointer = IsAutoHideActive();
        if (!isWaitingForPointer) return;

        var isInZone = IsInRevealZone(e);
        if (!isInZone) return;

        Peek();
    }


    private void HideTimer_Tick(object? sender, EventArgs e)
    {
        _hideTimer.Stop();
        _isHovered = false;

        UpdateVisibility();
    }


    private void PeekTimer_Tick(object? sender, EventArgs e)
    {
        _peekTimer.Stop();
        _isPeeking = false;

        UpdateVisibility();
    }

    #endregion // Control Events



    #region Visibility

    /// <summary>
    /// Gets whether the overlay mode lets the bar show in the window as it is now.
    /// </summary>
    private static bool IsModeActive
    {
        get
        {
            var mode = Core.Config.ImageInfoOverlayMode;
            if (mode == ImageInfoOverlayMode.Hidden) return false;

            var isInEveryWindow = mode is ImageInfoOverlayMode.Always or ImageInfoOverlayMode.AlwaysAutoHide;
            if (isInEveryWindow) return true;

            var isTitleBarHidden = IsTitleBarHidden;
            return isTitleBarHidden;
        }
    }


    /// <summary>
    /// Gets whether the bar waits for the pointer to show, which every mode does during a slideshow.
    /// </summary>
    private static bool IsAutoHideMode
    {
        get
        {
            var isSlideshow = Core.Config.EnableSlideshow;
            if (isSlideshow) return true;

            var mode = Core.Config.ImageInfoOverlayMode;
            var isAutoHide = mode is ImageInfoOverlayMode.AlwaysAutoHide or ImageInfoOverlayMode.BorderlessAutoHide;

            return isAutoHide;
        }
    }


    /// <summary>
    /// Checks if the bar has something to show, in a window its mode lets it show in.
    /// </summary>
    private bool IsAvailable()
    {
        var hasItems = _items.Count > 0;
        if (!hasItems) return false;

        var isModeActive = IsModeActive;
        return isModeActive;
    }


    /// <summary>
    /// Checks if the bar waits for the pointer to show.
    /// </summary>
    private bool IsAutoHideActive()
    {
        var isAutoHide = IsAutoHideMode;
        if (!isAutoHide) return false;

        var isAvailable = IsAvailable();
        return isAvailable;
    }


    /// <summary>
    /// Checks if the bar should be on screen now.
    /// </summary>
    private bool ShouldShow()
    {
        var isAvailable = IsAvailable();
        if (!isAvailable) return false;

        var isAutoHide = IsAutoHideMode;
        if (!isAutoHide) return true;

        if (_isHovered) return true;
        return _isPeeking;
    }


    /// <summary>
    /// Works out whether the bar should show, and animates toward it.
    /// </summary>
    private void UpdateVisibility()
    {
        // with nothing to reveal, a pending reveal must not show the bar later
        var isAvailable = IsAvailable();
        if (!isAvailable) ResetReveal();

        UpdateCoveredHeight();

        var isShown = ShouldShow();
        if (isShown == IsShown) return;

        IsShown = isShown;
        RefreshText();
    }


    /// <summary>
    /// Shows the bar for a moment in auto-hide mode, e.g. after a tap near it.
    /// </summary>
    private void Peek()
    {
        var isAutoHide = IsAutoHideMode;
        if (!isAutoHide) return;

        _isPeeking = true;
        _peekTimer.Stop();
        _peekTimer.Start();

        UpdateVisibility();
    }


    /// <summary>
    /// Forgets the pointer hover and any peek, with their timers.
    /// </summary>
    private void ResetReveal()
    {
        _isHovered = false;
        _isPeeking = false;
        _hideTimer.Stop();
        _peekTimer.Stop();
    }


    /// <summary>
    /// Checks if the pointer is at the edge of the viewer the bar sits on, where it shows in auto-hide mode.
    /// </summary>
    private bool IsInRevealZone(PointerEventArgs e)
    {
        if (this.GetVisualParent() is not Visual host) return false;
        var point = e.GetPosition(host);

        // a gallery on the side is not the edge of the viewer
        var isLeftOfViewer = point.X < 0;
        if (isLeftOfViewer) return false;

        var isRightOfViewer = point.X > host.Bounds.Width;
        if (isRightOfViewer) return false;

        var zoneExtra = _isHovered ? KEEP_ZONE_EXTRA : REVEAL_ZONE_EXTRA;
        var zoneDepth = GetBarHeight() + zoneExtra;

        // the bar may be placed at the bottom instead
        var isAtBottom = VerticalAlignment == VerticalAlignment.Bottom;
        if (isAtBottom)
        {
            var zoneTop = host.Bounds.Height - Margin.Bottom - zoneDepth;
            var isBelowZoneTop = point.Y >= zoneTop;
            return isBelowZoneTop;
        }

        var zoneBottom = Margin.Top + zoneDepth;
        var isAboveZoneBottom = point.Y <= zoneBottom;
        return isAboveZoneBottom;
    }


    /// <summary>
    /// Works out how far into the viewer the bar may reach, whether or not it shows right now.
    /// </summary>
    private void UpdateCoveredHeight()
    {
        // kept while the bar only waits for the pointer, so a hover does not push other overlays around
        var isAvailable = IsAvailable();
        if (!isAvailable)
        {
            CoveredHeight = 0;
            return;
        }

        var isAtBottom = VerticalAlignment == VerticalAlignment.Bottom;
        var edgeMargin = isAtBottom ? Margin.Bottom : Margin.Top;
        CoveredHeight = edgeMargin + GetBarHeight();
    }


    /// <summary>
    /// Gets the bar's height, estimated from the font size until it is first laid out.
    /// </summary>
    private double GetBarHeight()
    {
        var isLaidOut = Bounds.Height > 0;
        if (isLaidOut) return Bounds.Height;

        return Const.FONT_SIZE_BODY * 1.4 + Padding.Top + Padding.Bottom;
    }

    #endregion // Visibility



    #region Text

    /// <summary>
    /// Rebuilds a changed text while the bar is on screen.
    /// </summary>
    private void RefreshText()
    {
        if (!_isTextDirty) return;

        // a hidden bar catches up when it shows, so the updates of a playing animation cost nothing meanwhile
        if (!IsOnScreen) return;

        // with nothing left to show, the last text stays for the bar to fade out with
        if (_items.Count == 0) return;

        _isTextDirty = false;
        _textBlock.Inlines = BuildInlines();
    }


    /// <summary>
    /// Lays out the items in one line, the titles in bold, with accent separators.
    /// </summary>
    private InlineCollection BuildInlines()
    {
        var textBrush = new ImmutableSolidColorBrush(AppThemeColors.TextColorBrush.Color);
        var separatorBrush = new ImmutableSolidColorBrush(GetSeparatorColor());

        var inlines = new InlineCollection();
        foreach (var item in _items)
        {
            if (inlines.Count > 0)
            {
                inlines.Add(new Run(AppStatusInfo.TEXT_SEPARATOR) { Foreground = separatorBrush });
            }

            var isTitle = IsTitleItem(item);
            inlines.Add(new Run(item.Value)
            {
                Foreground = textBrush,
                FontWeight = isTitle ? FontWeight.SemiBold : FontWeight.Normal,
            });
        }

        return inlines;
    }


    /// <summary>
    /// Checks if the item names what is shown, e.g. the file name the details are about.
    /// </summary>
    private static bool IsTitleItem(ImageInfoItem item)
    {
        var isTitle = item.Tag is nameof(AppStatusInfo.Name)
            or nameof(AppStatusInfo.Path)
            or AppStatusInfo.CLIPBOARD_TAG;

        return isTitle;
    }


    /// <summary>
    /// Gets the accent tuned for text on the current theme, which the separators are drawn in.
    /// </summary>
    private static Color GetSeparatorColor()
    {
        var textAccent = Resx.Get<object?>(ResxId.IG_TextAccentColor);
        if (textAccent is Color color) return color;

        return Core.AccentColor;
    }

    #endregion // Text

}
