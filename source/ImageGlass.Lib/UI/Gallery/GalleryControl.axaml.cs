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
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ImageGlass.Common;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.UI;

public partial class GalleryControl : PhControl
{
    private CancellationTokenSource? _cancelScrollAnimation;
    private ScrollViewer? _cachedScrollViewerEl;
    public static readonly Thickness GalleryItemMargin = new(1);
    public static readonly Thickness GalleryPadding = new(4, 4, 4, 8);


    // events
    public event TEventHandler<GalleryItem, GalleryItemClickEventArgs>? ItemClicked;



    #region Public Properties

    /// <summary>
    /// Gets, sets the items source.
    /// </summary>
    public IEnumerable<Photo> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public static readonly StyledProperty<IEnumerable<Photo>> ItemsSourceProperty =
        AvaloniaProperty.Register<GalleryControl, IEnumerable<Photo>>(nameof(ItemsSource), []);


    /// <summary>
    /// Gets, sets the gallery view mode.
    /// </summary>
    public PhVirtualizingUniformPanelViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }
    public static readonly StyledProperty<PhVirtualizingUniformPanelViewMode> ViewModeProperty =
        AvaloniaProperty.Register<GalleryControl, PhVirtualizingUniformPanelViewMode>(nameof(ViewMode), PhVirtualizingUniformPanelViewMode.FilmStrip);


    /// <summary>
    /// Gets, sets tooltip placement for gallery item.
    /// </summary>
    public PlacementMode ItemTooltipPlacement
    {
        get => GetValue(ItemTooltipPlacementProperty);
        set => SetValue(ItemTooltipPlacementProperty, value);
    }
    public static readonly StyledProperty<PlacementMode> ItemTooltipPlacementProperty =
        AvaloniaProperty.Register<GalleryControl, PlacementMode>(nameof(ItemTooltipPlacement), PlacementMode.Pointer);


    /// <summary>
    /// Gets the minimum content size.
    /// </summary>
    public Size MinContentSize
    {
        get
        {
            var totalItemWidth = CalculateWidthForGalleryView(1);
            var totalItemHeight = Core.Config.ThumbnailSize
                + GalleryItemMargin.Top + GalleryItemMargin.Bottom
                + GalleryPadding.Top + GalleryPadding.Bottom;

            if (ViewMode == PhVirtualizingUniformPanelViewMode.FilmStrip)
            {
                return new(0, totalItemHeight);
            }

            return new(totalItemWidth, 0);
        }
    }
    public static readonly DirectProperty<GalleryControl, Size> MinContentSizeProperty =
        AvaloniaProperty.RegisterDirect<GalleryControl, Size>(nameof(MinContentSize), i => i.MinContentSize);


    #endregion // Public Properties



    public GalleryControl()
    {
        InitializeComponent();

        // tunneling: ScrollContentPresenter consumes the bubbling wheel event with its own fixed 50px step
        AddHandler(PointerWheelChangedEvent, GalleryControl_PointerWheelChanged, RoutingStrategies.Tunnel);
    }



    #region Override Methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        UpdateReservedSize();
        Core.Config.PropertyChanged += Config_PropertyChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _cachedScrollViewerEl = null;
        Core.Config.PropertyChanged -= Config_PropertyChanged;
    }


    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        ScrollToItem(Core.Photos.CurrentIndex, false);
    }


    protected override void OnIgDpiChanged()
    {
        base.OnIgDpiChanged();
        RefreshRealizedThumbnails();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ViewModeProperty)
        {
            RaisePropertyChanged(MinContentSizeProperty, default, MinContentSize);
            UpdateReservedSize();
        }
        else if (e.Property == IsContentVisibleProperty)
        {
            RefreshRealizedThumbnails();
        }
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Config.ThumbnailSize))
        {
            RaisePropertyChanged(MinContentSizeProperty, default, MinContentSize);
            UpdateReservedSize();
            RefreshRealizedThumbnails();
        }
    }


    #endregion // Override Methods



    #region Control Events

    private void GalleryControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.X == 0 && e.Delta.Y == 0) return;

        var svEl = FindScrollViewer();
        if (svEl is null) return;

        var cellSize = GetCellSize();

        if (ViewMode == PhVirtualizingUniformPanelViewMode.FilmStrip)
        {
            if (svEl.Extent.Width <= svEl.Viewport.Width) return;

            // touchpad horizontal swipe reports the delta on X, mouse wheel on Y
            var delta = e.Delta.Y != 0 ? e.Delta.Y : e.Delta.X;
            var step = GetWheelScrollStep(cellSize.Width, svEl.Viewport.Width);
            var maxOffset = Math.Max(0, svEl.Extent.Width - svEl.Viewport.Width);

            // subtract to match natural scroll direction (wheel down -> scroll right)
            var targetX = svEl.Offset.X - (delta * step);
            svEl.Offset = new Vector(Math.Clamp(targetX, 0, maxOffset), 0);
        }
        else
        {
            if (svEl.Extent.Height <= svEl.Viewport.Height) return;

            var step = GetWheelScrollStep(cellSize.Height, svEl.Viewport.Height);
            var maxOffset = Math.Max(0, svEl.Extent.Height - svEl.Viewport.Height);

            var targetY = svEl.Offset.Y - (e.Delta.Y * step);
            svEl.Offset = new Vector(svEl.Offset.X, Math.Clamp(targetY, 0, maxOffset));
        }

        e.Handled = true;
    }


    private void GalleryItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not GalleryItem itemEl) return;
        if (itemEl.VM.IsCurrent) return;

        // raise event
        ItemClicked?.Invoke(itemEl, new GalleryItemClickEventArgs(itemEl.VM));
    }

    #endregion // Control Events



    #region Control Methods


    /// <summary>
    /// Loads the thumbnail.
    /// </summary>
    public void LoadThumbnail(int index, bool useCache)
    {
        var photo = Core.Photos.Get(index);
        if (photo is null) return;

        // Cancel in-flight thumbnail load so it exits early
        if (!useCache) photo.CancelThumbnailLoading();

        var thumbSize = Core.Config.ThumbnailSize * Dpi * 2;
        _ = photo.LoadThumbnailAsync(thumbSize, useCache);
    }


    private void RefreshRealizedThumbnails()
    {
        foreach (var item in PART_ItemsControl.GetVisualDescendants().OfType<GalleryItem>())
        {
            if (IsContentVisible) item.LoadThumbnail();
            else item.CancelThumbnailLoading();
        }
    }


    /// <summary>
    /// Scrolls the gallery to bring the specified item into the center of the view.
    /// </summary>
    public void ScrollToItem(int index, bool enableAnimation = false)
    {
        var svEl = FindScrollViewer();
        if (svEl is null || index < 0) return;

        var targetOffset = GetCenteredScrollOffset(svEl, index);
        if (enableAnimation)
        {
            _ = AnimateScrollAsync(svEl, targetOffset);
        }
        else
        {
            _cancelScrollAnimation?.Cancel();
            svEl.Offset = targetOffset;
        }
    }


    /// <summary>
    /// Gets the size of a single item cell: item size + margin on each side (must match panel layout).
    /// </summary>
    private static Size GetCellSize()
    {
        var itemSize = (double)Core.Config.ThumbnailSize;
        var cellWidth = itemSize + GalleryItemMargin.Left + GalleryItemMargin.Right;
        var cellHeight = itemSize + GalleryItemMargin.Top + GalleryItemMargin.Bottom;

        return new Size(cellWidth, cellHeight);
    }


    /// <summary>
    /// Gets the distance to scroll for one wheel notch: one item cell, capped to the viewport
    /// so a thumbnail larger than the viewport never scrolls past a full page.
    /// </summary>
    private static double GetWheelScrollStep(double cellLength, double viewportLength)
    {
        if (viewportLength <= 0) return cellLength;

        return Math.Min(cellLength, viewportLength);
    }


    /// <summary>
    /// Calculates the scroll offset that centers the item at <paramref name="index"/>.
    /// </summary>
    private Vector GetCenteredScrollOffset(ScrollViewer svEl, int index)
    {
        // Border padding offsets the panel within the scroll content.
        var cellSize = GetCellSize();
        var cellWidth = cellSize.Width;
        var cellHeight = cellSize.Height;
        var padLeft = GalleryPadding.Left;
        var padTop = GalleryPadding.Top;

        if (ViewMode == PhVirtualizingUniformPanelViewMode.FilmStrip)
        {
            var itemCenter = padLeft + (index * cellWidth) + (cellWidth / 2);
            var target = itemCenter - (svEl.Viewport.Width / 2);
            var maxOffset = Math.Max(0, svEl.Extent.Width - svEl.Viewport.Width);

            return new Vector(Math.Clamp(target, 0, maxOffset), svEl.Offset.Y);
        }
        else
        {
            // Gallery mode: calculate row position
            var availWidth = svEl.Viewport.Width - padLeft - GalleryPadding.Right;
            var columnsPerRow = Math.Max(1, (int)(availWidth / cellWidth));
            var row = index / columnsPerRow;

            var itemCenter = padTop + (row * cellHeight) + (cellHeight / 2);
            var target = itemCenter - (svEl.Viewport.Height / 2);
            var maxOffset = Math.Max(0, svEl.Extent.Height - svEl.Viewport.Height);

            return new Vector(svEl.Offset.X, Math.Clamp(target, 0, maxOffset));
        }
    }


    /// <summary>
    /// Smoothly animates the <see cref="ScrollViewer"/> offset
    /// from its current position to <paramref name="targetOffset"/> using ease-out cubic.
    /// </summary>
    private async Task AnimateScrollAsync(ScrollViewer svEl, Vector targetOffset, int durationMs = 300)
    {
        _cancelScrollAnimation?.Cancel();
        _cancelScrollAnimation = new CancellationTokenSource();
        var token = _cancelScrollAnimation.Token;

        var startOffset = svEl.Offset;
        var startTime = Environment.TickCount64;

        while (!token.IsCancellationRequested)
        {
            var elapsed = Environment.TickCount64 - startTime;
            var progress = Math.Clamp((double)elapsed / durationMs, 0, 1);

            // ease-out cubic: f(t) = 1 - (1 - t)^3
            var eased = 1 - Math.Pow(1 - progress, 3);

            var x = startOffset.X + (targetOffset.X - startOffset.X) * eased;
            var y = startOffset.Y + (targetOffset.Y - startOffset.Y) * eased;
            svEl.Offset = new Vector(x, y);

            if (progress >= 1) break;

            await Task.Delay(16, token); // ~60 fps
        }
    }


    /// <summary>
    /// Finds the <see cref="ScrollViewer"/> inside the <see cref="ItemsControl"/> template.
    /// </summary>
    public ScrollViewer? FindScrollViewer()
    {
        _cachedScrollViewerEl ??= PART_ItemsControl
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();

        return _cachedScrollViewerEl;
    }


    /// <summary>
    /// Finds the virtual panel inside ItemsControl template.
    /// </summary>
    public PhVirtualizingUniformPanel? FindVirtualPanel()
    {
        return PART_ItemsControl
            .GetVisualDescendants()
            .OfType<PhVirtualizingUniformPanel>()
            .FirstOrDefault();
    }


    /// <summary>
    /// Updates the minimum size of gallery control and scroll bar visibility.
    /// </summary>
    private void UpdateReservedSize()
    {
        PART_ItemsControl.MinWidth = MinContentSize.Width;
        PART_ItemsControl.MinHeight = MinContentSize.Height;

        // Update ScrollViewer visibility so the panel receives correct constraints:
        // - FilmStrip: horizontal scroll, no vertical
        // - Gallery: vertical scroll, no horizontal (gives finite width to panel)
        var svEl = FindScrollViewer();
        if (svEl is not null)
        {
            if (ViewMode == PhVirtualizingUniformPanelViewMode.FilmStrip)
            {
                svEl.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                svEl.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                svEl.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                svEl.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }


    /// <summary>
    /// Calculates the total width required to display a gallery view
    /// with the specified number of item columns.
    /// </summary>
    public static double CalculateWidthForGalleryView(uint itemColumns)
    {
        var itemWidth = Core.Config.ThumbnailSize
            + GalleryItemMargin.Left + GalleryItemMargin.Right
            + GalleryPadding.Left + GalleryPadding.Right;

        return itemWidth * itemColumns;
    }


    #endregion // Control Methods


}


public class GalleryItemClickEventArgs(Photo vm) : RoutedEventArgs
{
    public Photo VM => vm;
}
