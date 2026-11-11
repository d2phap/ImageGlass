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
using Avalonia.Platform.Storage;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.UI;


/// <summary>
/// Drags the photo out of the gallery as a native file drag, droppable into any other app.
/// </summary>
public partial class GalleryItem
{
    // Windows uses SM_CXDRAG (4px); a bit more keeps a shaky click from becoming a drag
    private const double DRAG_THRESHOLD = 6d;

    private PointerPressedEventArgs? _dragTriggerEvent;
    private Point _dragStartPoint;
    private bool _isDraggingOut;


    #region Override Methods

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        ResetDragOutState();

        // touch owns filmstrip panning, so only mouse/pen may start a file drag
        if (e.Pointer.Type == PointerType.Touch) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        _dragTriggerEvent = e;
        _dragStartPoint = e.GetPosition(this);
    }


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDraggingOut || _dragTriggerEvent is null) return;

        var point = e.GetPosition(this);
        if (Math.Abs(point.X - _dragStartPoint.X) < DRAG_THRESHOLD
            && Math.Abs(point.Y - _dragStartPoint.Y) < DRAG_THRESHOLD) return;

        _isDraggingOut = true;
        _ = StartFileDragAsync(_dragTriggerEvent);
    }


    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        ResetDragOutState();
    }


    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        ResetDragOutState();
    }

    #endregion // Override Methods



    #region Private Methods

    /// <summary>
    /// Hands the file to the platform drag source; the drop target picks the operation.
    /// </summary>
    private async Task StartFileDragAsync(PointerPressedEventArgs triggerEvent)
    {
        try
        {
            var allowedEffects = GetAllowedDragEffects();
            if (allowedEffects == DragDropEffects.None) return;

            if (DataContext is not Photo photo) return;
            if (TopLevel.GetTopLevel(this) is not TopLevel topLevel) return;
            if (!File.Exists(photo.FilePath)) return;

            var file = await topLevel.StorageProvider.TryGetFileFromPathAsync(photo.FilePath);
            if (file is null) return;

            // one item with both formats: file for shells and upload fields, text for text targets
            var dtItem = new DataTransferItem();
            dtItem.SetFile(file);
            dtItem.SetText(photo.FilePath);

            // the platform owns the DataTransfer once the drag starts; never dispose it here
            var dataTransfer = new DataTransfer();
            dataTransfer.Add(dtItem);

            _ = await DragDrop.DoDragDropAsync(triggerEvent, dataTransfer, allowedEffects);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(StartFileDragAsync)}: {ex.Message}");
        }
        finally
        {
            ResetDragOutState();
        }
    }


    /// <summary>
    /// Maps the file-operation locks onto the allowed effects, since a drag-out skips the API gate.
    /// </summary>
    private static DragDropEffects GetAllowedDragEffects()
    {
        if (FeatureManager.IsLocked(API.IG_CopyFiles)) return DragDropEffects.None;

        var effects = DragDropEffects.Copy | DragDropEffects.Link;
        if (!FeatureManager.IsLocked(API.IG_CutFiles)) effects |= DragDropEffects.Move;

        return effects;
    }


    private void ResetDragOutState()
    {
        _dragTriggerEvent = null;
        _isDraggingOut = false;
    }

    #endregion // Private Methods

}
