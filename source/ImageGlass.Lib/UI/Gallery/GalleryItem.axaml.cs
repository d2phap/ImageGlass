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
using Avalonia.Interactivity;
using ImageGlass.Common;
using ImageGlass.Common.Photoing;
using System;
using System.Diagnostics;
using System.Threading;

namespace ImageGlass.UI;

public partial class GalleryItem : PhToolButton
{
    private CancellationTokenSource? _thumbnailCts;
    private Photo? _tooltipReadyPhoto;
    public Photo VM => (Photo)DataContext!;



    public GalleryItem()
    {
        InitializeComponent();
        ToolTip.AddToolTipOpeningHandler(this, ToolTip_Opening);
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        LoadThumbnail();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        CancelThumbnailLoading();
        base.OnUnloaded(e);
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == DataContextProperty)
        {
            _tooltipReadyPhoto = null;

            // a new container is still detached here (Dpi would be 1); OnLoaded covers it
            if (DataContext is Photo && IsLoaded) LoadThumbnail();
            else CancelThumbnailLoading();
        }
    }


    internal async void LoadThumbnail(bool useCache = true)
    {
        CancelThumbnailLoading();
        if (DataContext is not Photo photo) return;

        _thumbnailCts = new CancellationTokenSource();
        try
        {
            var thumbSize = Core.Config.ThumbnailSize * Dpi * 2;
            await photo.LoadThumbnailAsync(thumbSize, useCache, _thumbnailCts.Token);
        }
        catch (OperationCanceledException) { }
        // async void: a thumbnail failure must never FailFast the process from a worker thread
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(LoadThumbnail)}: {ex.Message}");
        }
    }


    internal void CancelThumbnailLoading()
    {
        _thumbnailCts?.Cancel();
        _thumbnailCts?.Dispose();
        _thumbnailCts = null;
    }


    private async void ToolTip_Opening(object? sender, CancelRoutedEventArgs e)
    {
        if (DataContext is not Photo photo) return;
        if (ReferenceEquals(_tooltipReadyPhoto, photo) || photo.Metadata.FrameCount > 0) return;

        e.Cancel = true;
        await photo.LoadMetadataAsync(true);

        // the gallery suspends the tooltip service while scrolling
        if (!ReferenceEquals(DataContext, photo) || !IsPointerOver || !ToolTip.GetServiceEnabled(this)) return;

        _tooltipReadyPhoto = photo;
        ToolTip.SetIsOpen(this, true);
    }

}
