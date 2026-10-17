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
using Avalonia.Input;
using Avalonia.Layout;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    // overlaid on the image, so showing them never resizes the drawing area or changes the zoom
    private readonly ScrollBar _hScrollbar = new() { Orientation = Orientation.Horizontal };
    private readonly ScrollBar _vScrollbar = new() { Orientation = Orientation.Vertical };
    private readonly Grid _scrollbarHost = new()
    {
        RowDefinitions = new("*, Auto"),
        ColumnDefinitions = new("*, Auto"),
        Cursor = Avalonia.Input.Cursor.Default,
        IsVisible = false,
    };


    /// <summary>
    /// Gets, sets how the scrollbars are shown when the image does not fit in the viewer.
    /// </summary>
    public ScrollbarMode ScrollbarMode
    {
        get => GetValue(ScrollbarModeProperty);
        set => SetValue(ScrollbarModeProperty, value);
    }
    public static readonly StyledProperty<ScrollbarMode> ScrollbarModeProperty =
        AvaloniaProperty.Register<ViewerControl, ScrollbarMode>(nameof(ScrollbarMode));


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // applying a template clears the visual children, so the scrollbars go back on top of it
        VisualChildren.Add(_scrollbarHost);
    }


    /// <summary>
    /// Builds the scrollbar layer; <see cref="OnApplyTemplate"/> adds it to the visual tree.
    /// </summary>
    private void InitScrollbars()
    {
        Grid.SetColumn(_vScrollbar, 1);
        Grid.SetRow(_hScrollbar, 1);
        _scrollbarHost.Children.Add(_vScrollbar);
        _scrollbarHost.Children.Add(_hScrollbar);
        _scrollbarHost.Bind(MarginProperty, this.GetObservable(PaddingProperty));
        LogicalChildren.Add(_scrollbarHost);

        _hScrollbar.Scroll += Scrollbar_Scroll;
        _vScrollbar.Scroll += Scrollbar_Scroll;
        _hScrollbar.PointerWheelChanged += Scrollbar_PointerWheelChanged;
        _vScrollbar.PointerWheelChanged += Scrollbar_PointerWheelChanged;

        // input on a scrollbar stays there, or the viewer would also pan, zoom or run a click action
        _scrollbarHost.AddHandler(PointerPressedEvent, ScrollbarHost_PointerPressed, handledEventsToo: true);
        _scrollbarHost.AddHandler(PointerMovedEvent, ScrollbarHost_PointerMoved);
        _scrollbarHost.AddHandler(PointerReleasedEvent, ScrollbarHost_PointerReleased);
        _scrollbarHost.AddHandler(TappedEvent, ScrollbarHost_Tapped);
        _scrollbarHost.AddHandler(DoubleTappedEvent, ScrollbarHost_Tapped);
        _scrollbarHost.AddHandler(RightTappedEvent, ScrollbarHost_Tapped);
    }


    /// <summary>
    /// Shows or hides the scrollbars, and keeps them expanded or lets them collapse, per <see cref="ScrollbarMode"/>.
    /// </summary>
    private void ApplyScrollbarMode()
    {
        var mode = ScrollbarMode;
        var isVisible = AreScrollbarsShown();
        var allowAutoHide = mode == ScrollbarMode.AutoHide;

        _scrollbarHost.IsVisible = isVisible;
        _hScrollbar.AllowAutoHide = allowAutoHide;
        _vScrollbar.AllowAutoHide = allowAutoHide;

        UpdateScrollbars();
    }


    /// <summary>
    /// Checks whether the scrollbars are shown at all; a display-only viewer takes no input to scroll with.
    /// </summary>
    private bool AreScrollbarsShown()
    {
        if (ScrollbarMode == ScrollbarMode.Never) return false;

        return IsInteractive;
    }


    /// <summary>
    /// Syncs the scrollbars with the current image size, zoom and pan position.
    /// </summary>
    private void UpdateScrollbars()
    {
        if (!AreScrollbarsShown()) return;

        // the pan position is in source pixels, so the viewport is measured in them too
        var zoomFactor = _zooming.Factor / Dpi;
        var viewport = DrawingArea.Size / zoomFactor;

        UpdateScrollbar(_hScrollbar, BitmapSize.Width, viewport.Width, _logicalSrcPoint.X);
        UpdateScrollbar(_vScrollbar, BitmapSize.Height, viewport.Height, _logicalSrcPoint.Y);
    }


    /// <summary>
    /// Syncs a scrollbar with the image extent, viewport and pan offset of its axis, in source pixels.
    /// </summary>
    private void UpdateScrollbar(ScrollBar bar, double extent, double viewport, double offset)
    {
        var logicalPixel = Dpi / _zooming.Factor;
        var hiddenExtent = extent - viewport;
        var hasViewport = viewport > 0;

        // less than a pixel out of view is an exact fit that rounding tipped over
        var hasHiddenImage = hiddenExtent >= logicalPixel;
        var isScrollable = hasViewport && hasHiddenImage;

        bar.Visibility = isScrollable ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        if (!isScrollable) return;

        // the range stretches to cover an over-pan past the image edges
        var minimum = Math.Min(0, offset);
        var maximum = Math.Max(hiddenExtent, offset);

        bar.Minimum = minimum;
        bar.Maximum = maximum;
        bar.ViewportSize = viewport;
        bar.LargeChange = viewport;
        bar.SmallChange = Const.MOUSE_WHEEL_SCROLL_DELTA * logicalPixel;
        bar.Value = offset;
    }


    /// <summary>
    /// Pans the image along the axis of the scrollbar, by a distance in logical pixels.
    /// </summary>
    private void PanAlong(ScrollBar bar, double distance)
    {
        if (bar.Orientation == Orientation.Horizontal)
        {
            _ = PanTo(distance, 0, null);
        }
        else
        {
            _ = PanTo(0, distance, null);
        }
    }


    private void Scrollbar_Scroll(object? sender, ScrollEventArgs e)
    {
        if (sender is not ScrollBar bar) return;

        // honor the pan feature lock (scrollbar pan skips the RunApiAsync lock gate)
        if (FeatureManager.IsPanLocked())
        {
            UpdateScrollbars();
            return;
        }

        var isHorizontal = bar.Orientation == Orientation.Horizontal;
        var offset = isHorizontal ? _logicalSrcPoint.X : _logicalSrcPoint.Y;

        // the scrollbar value is in source pixels, PanTo takes logical pixels
        var distance = (e.NewValue - offset) * _zooming.Factor / Dpi;

        PanAlong(bar, distance);
    }


    private void Scrollbar_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // the wheel scrolls the scrollbar under the pointer instead of zooming the image
        e.Handled = true;
        if (sender is not ScrollBar bar) return;
        if (FeatureManager.IsPanLocked()) return;

        var isHorizontalDelta = Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y);
        var delta = isHorizontalDelta ? e.Delta.X : e.Delta.Y;
        var distance = -delta * Const.MOUSE_WHEEL_SCROLL_DELTA;

        PanAlong(bar, distance);
    }


    private void ScrollbarHost_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // a touch drag on the track must not also pan the image through the viewer's gestures
        e.PreventGestureRecognition();

        // thumb buttons keep their viewer action, e.g. viewing the next image
        var p = e.GetCurrentPoint(this);
        var isThumbButton = IsThumbButtonPressed(p);
        if (isThumbButton) return;

        e.Handled = true;
    }


    private void ScrollbarHost_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var isThumbButton = IsThumbButton(e.InitialPressMouseButton);
        if (isThumbButton) return;

        e.Handled = true;
    }


    private static void ScrollbarHost_PointerMoved(object? sender, PointerEventArgs e) => e.Handled = true;


    private static void ScrollbarHost_Tapped(object? sender, TappedEventArgs e) => e.Handled = true;

}
