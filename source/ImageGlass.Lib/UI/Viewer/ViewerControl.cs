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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer.ZoomAndPan;
using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl : PhControl
{
    private CancellationTokenSource? _cancelPreview;
    internal InterlockedBool _isPreviewing = new(false);
    internal InterlockedBool _isFirstDraw = new(false);
    private InterlockedBool _isRecoveringPhoto = new(false);
    internal PhotoLoadingOptions _loadingOptions = new();

    // completes once the current photo is painted; null when nothing is pending
    internal TaskCompletionSource? _firstDrawTcs;

    private Point? _lastMousePanPoint = null; // mouse panning
    private Point? _lockZoomSavedSrcPoint; // saved pan position for LockZoom
    private Point? _mouseClickDownPoint = null; // track press point for click action


    // events
    public event TEventHandler<ViewerControl, PhotoLoadingEventArgs>? PhotoLoading;
    public event TEventHandler<ViewerControl, PhotoFrameChangedEventArgs>? PhotoFrameChanged;
    public event TEventHandler<ViewerControl, ViewerPointerEventArgs>? ViewerPointerMoved;
    public event TEventHandler<ViewerControl, ViewerPointerEventArgs>? ViewerPointerPressed;
    public event TEventHandler<ViewerControl, ViewerPointerClickEventArgs>? ViewerPointerClicked;
    public event TEventHandler<ViewerControl, ViewerMouseWheelEventArgs>? ViewerMouseWheel;


    #region Public Properties

    /// <summary>
    /// Gets the drawing area.
    /// </summary>
    public Rect DrawingArea { get; private set; }


    /// <summary>
    /// Gets the current photo.
    /// </summary>
    public Photo? Photo { get; private set; }


    /// <summary>
    /// Gets the bitmap size.
    /// </summary>
    public Size BitmapSize { get; private set; }


    /// <summary>
    /// Gets the photo source for renderer.
    /// </summary>
    public PhotoSource SourceKind { get; private set; } = PhotoSource.None;


    /// <summary>
    /// Gets, sets value indicates whether full resolution is loaded or not.
    /// </summary>
    public InterlockedBool ShouldLoadFullResolution = new(true);


    /// <summary>
    /// Checks if the photo is animating.
    /// </summary>
    public bool IsImageAnimating => _animator?.IsPlaying ?? false;


    /// <summary>
    /// Gets the value indicates whether the color is inverted by <see cref="InvertColor(bool)"/>.
    /// </summary>
    public bool IsColorInverted { get; protected set; } = false;


    #endregion // Public Properties



    #region Override Methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Core.ColorProfileChanged += Core_ColorProfileChanged;

        // suppress the default way to open context menu
        AddHandler(ContextRequestedEvent, OnContextMenuRequested, RoutingStrategies.Tunnel);
        RegisterTouchGestures();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.ColorProfileChanged -= Core_ColorProfileChanged;

        UnregisterTouchGestures();

        // suppress the default way to open context menu
        RemoveHandler(ContextRequestedEvent, OnContextMenuRequested);

        DisposeCheckerboard();
        DisposeNativePhotoResources();
    }


    private void Core_ColorProfileChanged(object? sender, DestColorProfileChangedEventArgs e)
    {
        // a monitor change only re-targets later loads; reloading here would flash the viewer
        if (!e.RequiresPhotoReload) return;

        Photo? photo;
        lock (_lock)
        {
            // skip animated/vector; skip an in-progress load (its own load applies the current
            // profile); skip when there's no color management to re-apply
            if (_animator is not null || IsVectorSource()) return;
            if (Photo is not { State: PhotoState.Loaded } || !HasColorManagement()) return;
            photo = Photo;
        }

        // Re-decode from source, keeping zoom + pan (ResetZoom: false). A fresh decode
        // re-applies color management cleanly and re-selects the right codec, instead of
        // compounding the conversion on the already color-managed _imgSource.
        _ = SetPhotoAsync(photo, new PhotoLoadingOptions
        {
            ResetZoom = false,
            UseCache = false,
            Channels = Core.ColorChannels,
        });
    }


    /// <summary>
    /// Whether the current photo has color management to re-apply (HDR, always-apply, or an
    /// embedded profile), used to skip redundant re-decodes on color-profile changes.
    /// </summary>
    private bool HasColorManagement()
    {
        var meta = Photo?.Metadata;
        if (meta is null) return false;

        return meta.IsHdr
            || Core.Config.EnableAlwaysApplyColorProfile
            || meta.SkiaColorSpace is not null
            || meta.MagickColorProfile is not null;
    }


    protected override void OnIgDpiChanged()
    {
        base.OnIgDpiChanged();

        DisposeCheckerboard();
        InvalidateVisual();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // update drawing area
        if (e.Property == PaddingProperty || e.Property == BoundsProperty)
        {
            Dispatcher.UIThread.Post(() =>
            {
                DrawingArea = Bounds.Deflate(Padding);
                CalculateDrawingRegion();

                // redraw the control on resizing
                var shouldResetZoom = Photo is not null && !_zooming.IsManual;
                Refresh(shouldResetZoom, false, true);
            });
        }
        else if (e.Property == EnableNavButtonsProperty)
        {
            _navButtons.IsEnabled = (bool)e.NewValue!;
        }
    }


    private void OnContextMenuRequested(object? sender, ContextRequestedEventArgs e)
    {
        // disable context menu for custom mouse click actions
        e.Handled = true;
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var p = e.GetCurrentPoint(this);
        var requestRerender = false;

        // set the init point for panning
        if (p.Pointer.Type == PointerType.Mouse && !IsThumbButtonPressed(p))
        {
            var canPanByMouse = !EnableSelection || e.Properties.IsMiddleButtonPressed;

            if (canPanByMouse)
            {
                _lastMousePanPoint = p.Position;
            }
            else if (EnableSelection)
            {
                requestRerender = OnSelectionBegin(p);
            }

            // track press point and button for mouse click action dispatch
            _mouseClickDownPoint = p.Position;
        }

        // request re-render control
        if (requestRerender) InvalidateVisual();


        // trigger event
        OnViewerPointerPressed(e, p);
    }


    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (CurrentSelectionAction != SelectionAction.None)
        {
            _ = OnSelectionEnd(true);
        }
    }


    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var isThumbButton = IsThumbButton(e.InitialPressMouseButton);

        // a thumb button owns no drag gesture, so releasing one must not end a pan or a
        // selection that another button still has in progress
        if (!isThumbButton)
        {
            // reset the panning point
            _lastMousePanPoint = null;

            var requestRerender = OnSelectionEnd(false);
            if (requestRerender) InvalidateVisual();
        }

        // dispatch mouse click action (single clicks only)
        DispatchMouseClickAction(e);
        if (!isThumbButton) _mouseClickDownPoint = null;

        base.OnPointerReleased(e);
    }


    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        // reset the panning point
        _lastMousePanPoint = null;


        var requestRerender = OnSelectionEnd(false);
        if (requestRerender) InvalidateVisual();

        base.OnPointerCaptureLost(e);
    }


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var p = e.GetCurrentPoint(this);
        var requestRerender = false;

        _zooming.ZoomedPoint = p.Position;


        // if user is panning with mouse
        if (_lastMousePanPoint is not null)
        {
            var hDistance = _lastMousePanPoint.Value.X - p.Position.X;
            var vDistance = _lastMousePanPoint.Value.Y - p.Position.Y;
            _lastMousePanPoint = p.Position;

            // honor the pan feature lock (drag-pan skips the RunApiAsync lock gate)
            if (!FeatureManager.IsPanLocked())
                requestRerender = PanTo(hDistance, vDistance, p.Position);
        }
        else
        {
            requestRerender = OnSelectionUpdating(p.Position);
        }


        // request re-render control
        if (requestRerender) InvalidateVisual();


        // trigger event
        OnViewerPointerMoved(e, p);
    }


    protected override void OnRightTapped(TappedEventArgs e)
    {
        base.OnRightTapped(e);

        ViewerPointerClicked?.Invoke(this, new ViewerPointerClickEventArgs(e, MouseClickEvent.RightClick));
    }


    protected override void OnDoubleTapped(TappedEventArgs e)
    {
        base.OnDoubleTapped(e);
        if (EnableSelection) return;


        // exclude nav button regions
        var pos = e.GetPosition(this);
        if (IsInNavButtonHitArea(pos)) return;

        ViewerPointerClicked?.Invoke(this, new ViewerPointerClickEventArgs(e, MouseClickEvent.LeftDoubleClick));
    }


    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = e.Delta.Y;
        var deltaAbs = Math.Abs(delta);
        var position = e.GetPosition(this);
        var isPreciseScrolling = Core.ShellProvider?.HasPreciseScrollingDeltas() ?? false;
        var isUsingTouchpad = isPreciseScrolling || deltaAbs != 1;

        // Touchpad scrolling — keep existing behavior
        if (isUsingTouchpad)
        {
            // horizontal component dominates -> horizontal pan gesture
            var isScrollingHorz = Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y) * 1.5;

            // Scroll Left/Right: Pan horizontally
            if (isScrollingHorz)
            {
                // this touchpad path skips the RunApiAsync lock gate, so check the pan lock here
                if (!FeatureManager.IsPanLocked()) PanTo(e.Delta.X * -50, e.Delta.Y * -50, position);
                return;
            }

            // Scroll Up/Down: Zoom
            delta *= 70;
            if (!FeatureManager.IsZoomLocked()) _ = ZoomByDeltaToPoint(delta, position);
            return;
        }

        // Mouse wheel — raise event for external dispatch
        delta *= Const.MOUSE_WHEEL_SCROLL_DELTA;
        var wheelEvent = GetMouseWheelEvent(e.KeyModifiers);

        ViewerMouseWheel?.Invoke(this, new ViewerMouseWheelEventArgs(e, wheelEvent, delta, position));
    }


    /// <summary>
    /// Raises <see cref="PhotoLoading"/> event.
    /// </summary>
    protected virtual void OnPhotoLoading(PhotoLoadingEventArgs e)
    {
        PhotoLoading?.Invoke(this, e);
    }


    /// <summary>
    /// Raises <see cref="PhotoFrameChanged"/> event.
    /// </summary>
    protected virtual void OnPhotoFrameChanged(AnimatorFrameChangedEventArgs? e = null)
    {
        if (SourceKind == PhotoSource.None) return;

        var canAnimate = _animator != null;
        var isLivePhoto = Photo?.Metadata?.IsLivePhoto ?? false;
        var args = new PhotoFrameChangedEventArgs(canAnimate, IsImageAnimating, isLivePhoto)
        {
            CurrentFrame = e?.CurrentFrame ?? (uint)Math.Max(0, Photo?.FrameIndex ?? 0),
            FrameCount = e?.FrameCount ?? Photo?.Metadata.FrameCount ?? 0,
            CurrentLoop = e?.CurrentLoop ?? _animator?.CurrentLoop ?? 0,
            LoopCount = e?.LoopCount ?? _animator?.LoopCount ?? 0,
        };

        PhotoFrameChanged?.Invoke(this, args);
    }


    /// <summary>
    /// Raises <see cref="ViewerPointerPressed"/> event.
    /// </summary>
    protected virtual void OnViewerPointerPressed(PointerEventArgs e, PointerPoint p)
    {
        if (ViewerPointerPressed is not null)
        {
            var srcPoint = PointClientToSource(p.Position).ToPixelPoint();
            ViewerPointerPressed.Invoke(this, new ViewerPointerEventArgs(e, p, srcPoint));
        }
    }


    /// <summary>
    /// Raises <see cref="ViewerPointerMoved"/> event.
    /// </summary>
    protected virtual void OnViewerPointerMoved(PointerEventArgs e, PointerPoint p)
    {
        if (ViewerPointerMoved is not null)
        {
            var srcPoint = PointClientToSource(p.Position).ToPixelPoint();
            ViewerPointerMoved.Invoke(this, new ViewerPointerEventArgs(e, p, srcPoint));
        }
    }


    /// <summary>
    /// Determines the <see cref="MouseWheelEvent"/> from keyboard modifiers.
    /// </summary>
    private static MouseWheelEvent GetMouseWheelEvent(KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Control)) return MouseWheelEvent.CtrlAndScroll;
        if (modifiers.HasFlag(KeyModifiers.Shift)) return MouseWheelEvent.ShiftAndScroll;
        if (modifiers.HasFlag(KeyModifiers.Alt)) return MouseWheelEvent.AltAndScroll;
        return MouseWheelEvent.Scroll;
    }


    /// <summary>
    /// Checks if the given position is within the nav button hit area.
    /// </summary>
    private bool IsInNavButtonHitArea(Point pos)
    {
        if (!EnableNavButtons || !_navButtons.IsEnabled) return false;

        var hitWidth = NavButtonsInfo.NAV_BTN_SIZE.Width + NavButtonsInfo.NAV_BTN_MARGIN;
        var leftHitArea = new Rect(0, 0, hitWidth, Bounds.Height);
        var rightHitArea = new Rect(Bounds.Width - hitWidth, 0, hitWidth, Bounds.Height);

        return leftHitArea.Contains(pos) || rightHitArea.Contains(pos);
    }


    /// <summary>
    /// Dispatches a single-click action based on the pointer release event.
    /// Double-click is handled by <see cref="OnDoubleTapped"/>.
    /// </summary>
    private void DispatchMouseClickAction(PointerReleasedEventArgs e)
    {
        // determine the single-click event
        var clickEvent = e.InitialPressMouseButton switch
        {
            MouseButton.Left => MouseClickEvent.LeftClick,
            MouseButton.Middle => MouseClickEvent.WheelClick,
            MouseButton.XButton1 => MouseClickEvent.XButton1Click,
            MouseButton.XButton2 => MouseClickEvent.XButton2Click,
            _ => (MouseClickEvent?)null,
        };
        if (clickEvent is null) return;


        // the guards below only protect the left/middle drag gestures (pan, selection, nav
        // buttons); a thumb button drives none of them, so it always dispatches
        if (!IsThumbButton(e.InitialPressMouseButton))
        {
            if (_mouseClickDownPoint is null) return;
            if (EnableSelection) return;

            var pos = e.GetPosition(this);

            // don't fire if the user dragged (threshold 5px)
            var dragDistance = Math.Sqrt(
                Math.Pow(pos.X - _mouseClickDownPoint.Value.X, 2)
                + Math.Pow(pos.Y - _mouseClickDownPoint.Value.Y, 2));
            if (dragDistance > 5) return;

            // exclude nav button regions
            if (IsInNavButtonHitArea(pos)) return;
        }

        ViewerPointerClicked?.Invoke(this, new ViewerPointerClickEventArgs(e, clickEvent.Value));
    }


    /// <summary>
    /// Checks if the press that raised the event came from a thumb button (XButton1/XButton2).
    /// </summary>
    private static bool IsThumbButtonPressed(PointerPoint p)
    {
        return p.Properties.PointerUpdateKind
            is PointerUpdateKind.XButton1Pressed
            or PointerUpdateKind.XButton2Pressed;
    }


    /// <summary>
    /// Checks if the given mouse button is a thumb button (XButton1/XButton2).
    /// </summary>
    private static bool IsThumbButton(MouseButton button)
    {
        return button is MouseButton.XButton1 or MouseButton.XButton2;
    }

    #endregion // Override Methods



    #region Control Methods

    /// <summary>
    /// Forces the control to reset zoom mode and invalidate itself.
    /// </summary>
    public void Refresh()
    {
        Refresh(true);
    }


    /// <summary>
    /// Forces the control to invalidate itself.
    /// </summary>
    public void Refresh(bool resetZoom = true, bool isManualZoom = false, bool zoomedByResizing = false)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (resetZoom)
            {
                SetZoomMode(null, isManualZoom, zoomedByResizing);
            }
            else if (DestRect.IsEmpty && !BitmapSize.IsEmpty)
            {
                // the viewport was emptied while the source was unloaded (a DPI change mid-load,
                // e.g. dragging the window to another monitor); rebuild it
                if (_zooming.IsManual)
                {
                    // clean recompute: keeps zoom + pan, skips the zoom-to-cursor path
                    _zooming.ZoomedPoint = new();
                    _zooming.OldFactor = _zooming.Factor;
                    CalculateDrawingRegion();
                }
                else
                {
                    SetZoomMode(null, false, zoomedByResizing);
                }
            }

            InvalidateVisual();
        });
    }


    /// <summary>
    /// Cancels any ongoing photo previewing operation.
    /// </summary>
    [MemberNotNull(nameof(_cancelPreview))]
    private void CancelPreview()
    {
        lock (_lock)
        {
            _cancelPreview?.Cancel();
            _cancelPreview?.Dispose();
            _cancelPreview = new();
        }
    }


    /// <summary>
    /// Disposes the native photo resources.
    /// </summary>
    private void DisposeNativePhotoResources()
    {
        lock (_lock)
        {
            // dispose tile cache first (it holds a KeepAlive on the source)
            _mipmapCache?.Dispose();
            _mipmapCache = null;

            _animator?.Dispose();
            _animator = null;

            // dispose vector resources
            DisposeVectorResources();

            // dispose native bitmap
            SKImageRef.Set(ref _imgSource, null);
            SKImageRef.Set(ref _imgRender, null);
            SKImageRef.Set(ref _imgHdrSource, null);
        }
    }


    /// <summary>
    /// Unloads the photo and cancels any ongoing loading operation.
    /// </summary>
    [MemberNotNull(nameof(_cancelPreview))]
    public void UnloadPhoto()
    {
        lock (_lock)
        {
            CancelPreview();
            StopAnimator();
            SourceKind = PhotoSource.None;
            Photo?.CancelLoading();
            Photo?.Unload();

            // release a waiter whose load is being superseded
            _firstDrawTcs?.TrySetResult();
            _firstDrawTcs = null;

            // reset
            AnimationSource = AnimationSources.None;
            ClearPhotoTransforms();
            _loadingOptions = new();
            _enablePanningVelocity = false;
            SetSourceSelection(new Rect(), false);
            BitmapSize = new();

            // dispose native resources of photo
            DisposeNativePhotoResources();
        }
    }


    /// <summary>
    /// Resets all photo transformation settings to their default values.
    /// </summary>
    public void ClearPhotoTransforms()
    {
        IsColorInverted = false;
    }


    /// <summary>
    /// Sets a photo to render.
    /// </summary>
    public async Task SetPhotoAsync(Photo? inputPhoto, PhotoLoadingOptions? options = null)
    {
        PhotoTrace.Mark("viewer:set-photo", inputPhoto?.FilePath,
            $"cache={options?.UseCache ?? true}, state={inputPhoto?.State}, shouldLoadFull={ShouldLoadFullResolution}");

        lock (_lock)
        {
            // save pan position for LockZoom before unloading
            _lockZoomSavedSrcPoint = ZoomMode == ZoomMode.LockZoom ? _logicalSrcPoint : null;

            // unload current photo resources
            UnloadPhoto();

            if (inputPhoto is null)
            {
                Photo = null;
                Refresh(true);
                return;
            }


            SourceKind = PhotoSource.Native;
            _loadingOptions = options ?? new();
            _enablePanningVelocity = true;
            Photo = inputPhoto;
        }


        // photo is loaded, render full resolution directly from cache
        if (_loadingOptions.UseCache && inputPhoto.State == PhotoState.Loaded && ShouldLoadFullResolution)
        {
            var token = inputPhoto.CancelToken ?? default;
            await HandlePhotoLoadedAsync(new(PhotoState.Loaded, inputPhoto, token));
        }
        else
        {
            // force reload when quick browsing to render preview instead of full photo
            var useCache = _loadingOptions.UseCache && ShouldLoadFullResolution;
            await LoadPhotoAsync(
                useCache: useCache,
                skipLoadingEvent: !_loadingOptions.ResetZoom);
        }

        await RecoverIfNothingRenderedAsync(inputPhoto);
    }


    /// <summary>
    /// Recovers a photo whose load ended without a frame, which would leave the viewer blank.
    /// </summary>
    private async Task RecoverIfNothingRenderedAsync(Photo photo)
    {
        // the reload below comes back through LoadPhotoAsync, so recover only once
        if (_isRecoveringPhoto) return;
        if (!NeedsRecovery(photo)) return;

        PhotoTrace.Mark("viewer:recover", photo.FilePath, $"state={photo.State}");

        // decoded meanwhile (a concurrent loader won the race): render what is already in memory
        if (photo.State == PhotoState.Loaded)
        {
            await HandlePhotoLoadedAsync(new(PhotoState.Loaded, photo, CancellationToken.None));
            if (!NeedsRecovery(photo)) return;
        }

        // nothing usable in memory: decode again, ignoring the cache
        PhotoTrace.Mark("viewer:recover-reload", photo.FilePath);
        _isRecoveringPhoto.SetTrue();
        try
        {
            await LoadPhotoAsync(useCache: false, skipLoadingEvent: true);
        }
        finally
        {
            _isRecoveringPhoto.SetFalse();
        }
    }


    /// <summary>
    /// Whether <paramref name="photo"/> is the photo on screen yet has nothing renderable.
    /// </summary>
    private bool NeedsRecovery(Photo photo)
    {
        // a newer navigation owns the viewer now, and its own load will paint
        if (!ReferenceEquals(photo, Photo)) return false;

        // quick browsing renders the preview on purpose, and an error is reported by its own path
        if (!ShouldLoadFullResolution || photo.Error is not null) return false;

        lock (_lock)
        {
            if (IsVectorSource() || _animator is not null) return false;

            return _imgSource is null || _imgSource.Image.IsDisposed();
        }
    }


    /// <summary>
    /// Loads photo data and renders to the viewer
    /// </summary>
    public async Task LoadPhotoAsync(bool useCache, bool skipLoadingEvent)
    {
        if (Photo is null) return;
        var photo = Photo;

        await photo.LoadAsync(useCache, OnPhotoLoadingProgressAsync, skipLoadingEvent);

        await RecoverIfNothingRenderedAsync(photo);
    }


    /// <summary>
    /// Waits until the current photo has actually been painted at least once.
    /// Returns immediately when there is nothing pending to draw.
    /// </summary>
    public async Task WaitForPhotoRenderedAsync(TimeSpan timeout)
    {
        Task? drawTask;
        lock (_lock)
        {
            drawTask = _firstDrawTcs?.Task;
        }

        if (drawTask is null || drawTask.IsCompleted) return;

        // a minimized or occluded window may never produce a frame, so cap the wait
        _ = await Task.WhenAny(drawTask, Task.Delay(timeout));
    }


    /// <summary>
    /// Processes photo loading progress.
    /// </summary>
    private async Task OnPhotoLoadingProgressAsync(PhotoLoadingEventArgs e)
    {
        // previewing
        if (e.State == PhotoState.Preview)
        {
            await HandlePhotoPreviewAsync(e);
        }
        // load full resolution photo
        else
        {
            await HandlePhotoLoadedAsync(e);
        }
    }


    /// <summary>
    /// Handles previewing the current photo.
    /// </summary>
    private async Task HandlePhotoPreviewAsync(PhotoLoadingEventArgs e)
    {
        if (!ShouldLoadFullResolution) e.Photo.CancelLoading();

        // 1. skip the preview if it's not enable or in zoom lock mode
        if (!EnableImagePreview || ZoomMode == ZoomMode.LockZoom)
        {
            PhotoTrace.Mark("viewer:preview-skip", e.Photo.FilePath,
                $"enablePreview={EnableImagePreview}, zoomMode={ZoomMode}");

            // raise event
            _isPreviewing.SetFalse();
            OnPhotoLoading(e);
            return;
        }


        SKImage? imgPreview = null;
        var token = _cancelPreview?.Token ?? default;
        var hasPreview = false;


        // 2. try to get the preview bitmap
        try
        {
            // try to get photo preview
            if (e.Photo.GalleryThumbnail is not null)
            {
                PhotoTrace.Mark("viewer:preview-source", e.Photo.FilePath, "gallery-thumbnail");
                using var skBmp = SkiaCodec.FromBitmap(e.Photo.GalleryThumbnail);
                imgPreview = SkiaCodec.ToSKImage(skBmp);
            }
            else
            {
                PhotoTrace.Mark("viewer:preview-source", e.Photo.FilePath, "preview-provider");
                var previewHeight = Math.Min(Math.Min(DrawingArea.Height, e.Metadata.Height), 700);
                imgPreview = await Core.PreviewProvider.GetPreviewAsync(e.Metadata, previewHeight, token);
            }
            hasPreview = imgPreview is not null;

            // cancel if requested
            if (token.IsCancellationRequested)
            {
                HandleCancelLoading();
                return;
            }

            if (hasPreview)
            {
                // get thumbnail size
                var prevWidth = imgPreview?.Width ?? (int)e.Metadata.Width;
                var prevHeight = imgPreview?.Height ?? (int)e.Metadata.Height;
                BitmapSize = new(prevWidth, prevHeight);

                // cancel if requested
                if (token.IsCancellationRequested)
                {
                    HandleCancelLoading();
                    return;
                }
            }
        }
        catch
        {
            HandleCancelLoading();
        }


        // 3. calculate viewport of preview
        if (hasPreview)
        {
            lock (_lock)
            {
                // set preview source
                SKImageRef.Set(ref _imgSource, imgPreview);

                var desiredSrcZoomFactor = CalculateZoomFactor(ZoomMode, e.Metadata.Width, e.Metadata.Height);

                // the preview is a shrunken copy of the source, so scale it back up by however
                // much it was shrunk; it then covers the exact area the full image will
                var previewToSrcScale = GetPreviewToSourceScale(BitmapSize, e.Metadata);
                var previewZoomFactor = desiredSrcZoomFactor * previewToSrcScale;

                SetZoomFactor(previewZoomFactor, false);
            }
        }


        PhotoTrace.Mark("viewer:preview-done", e.Photo.FilePath,
            hasPreview ? $"{BitmapSize.Width}x{BitmapSize.Height}" : "no-preview");

        // raise event
        _isPreviewing.Set(hasPreview);
        OnPhotoLoading(e);


        void HandleCancelLoading()
        {
            hasPreview = false;
            imgPreview?.Dispose();
            imgPreview = null;
        }
    }


    /// <summary>
    /// Gets how many times the preview bitmap was shrunk from the full-size photo, so the preview
    /// can be drawn over the exact area the full image is going to occupy.
    /// </summary>
    private static double GetPreviewToSourceScale(Size previewSize, PhotoMetadata meta)
    {
        if (previewSize.Width <= 0 || previewSize.Height <= 0) return 1;
        if (meta.Width == 0 || meta.Height == 0) return 1;

        var widthScale = meta.Width / previewSize.Width;
        var heightScale = meta.Height / previewSize.Height;

        // Min keeps the preview inside the final footprint when the aspect ratios differ slightly
        return Math.Max(1, Math.Min(widthScale, heightScale));
    }


    /// <summary>
    /// Handles rendering the current photo.
    /// </summary>
    private async Task HandlePhotoLoadedAsync(PhotoLoadingEventArgs e)
    {
        if (!ShouldLoadFullResolution) return;


        // 1. back up size of preview image
        var prevSize = BitmapSize;

        // source
        SKImage? imgFrame = null;
        AnimatorImpl? animator = null;
        var hasSource = false;

        // raw pre-tone-map HDR frame to keep for live re-tone-mapping (null = don't retain)
        SKImage? hdrRawToRetain = null;

        try
        {
            // 2. check if photo error
            if (e.Photo.Error is not null)
            {
                HandleCancelLoaded(false);
                OnPhotoLoading(e);
                return;
            }

            // cancel if requested
            if (e.CancelToken.IsCancellationRequested)
            {
                HandleCancelLoaded(true);
                return;
            }


            // 3. create the native bitmap
            if (e.Photo.Bitmap is not null)
            {
                // vector (SVG) source
                if (e.Photo.Bitmap is SkiaVectorSource)
                {
                    PhotoTrace.Mark("viewer:loaded-source", e.Photo.FilePath, "vector");
                    hasSource = true;
                }
                // native bitmap is an animated bitmap
                else if (e.Photo.Bitmap is AnimatorImpl skAnimator)
                {
                    PhotoTrace.Mark("viewer:loaded-source", e.Photo.FilePath,
                        $"animator ({skAnimator.GetType().Name})");

                    // update bitmap size
                    BitmapSize = e.Photo.Size;

                    animator = skAnimator;
                    hasSource = true;
                }
                // native bitmap is a single-frame bitmap
                else
                {
                    PhotoTrace.Mark("viewer:loaded-source", e.Photo.FilePath, "single-frame");

                    // update bitmap size
                    BitmapSize = e.Photo.Size;

                    var frameToLoad = (uint)Math.Max(0, e.Photo.FrameIndex);
                    imgFrame = await e.Photo.GetFrameAsync(frameToLoad);

                    // apply color space off the UI thread; the pin keeps the frame alive if the
                    // user navigates away mid-pass
                    SKImage? imgFrameColored;
                    using (e.Photo.PinBitmap())
                    {
                        imgFrameColored = await ApplySkiaColorSpaceAsync(imgFrame, e.Photo.Metadata);
                    }

                    if (imgFrameColored is not null)
                    {
                        PhotoTrace.Mark("viewer:color-managed", e.Photo.FilePath,
                            $"applied (hdrToneMap={Core.Config.EnableHdrToneMapping && e.Photo.Metadata.IsHdr}, srcProfile={(string.IsNullOrEmpty(e.Photo.Metadata.ColorProfileName) ? "none" : e.Photo.Metadata.ColorProfileName)})");

                        // retain the pre-tone-map HDR frame for live re-tone-mapping, else free it
                        // (never dispose the clipboard photo's frame)
                        if (_liveHdrToneMapping && e.Photo.Metadata.IsHdr && !e.Photo.IsClipboard)
                        {
                            hdrRawToRetain = imgFrame;
                        }
                        else if (!e.Photo.IsClipboard)
                        {
                            imgFrame?.Dispose();
                        }

                        imgFrame = imgFrameColored;
                    }
                    else
                    {
                        PhotoTrace.Mark("viewer:color-managed", e.Photo.FilePath, "skipped");
                    }

                    // IsDisposed, not null: a dead frame renders nothing yet claims the photo is shown
                    hasSource = !imgFrame.IsDisposed();
                }
            }


            // cancel if requested
            if (e.CancelToken.IsCancellationRequested)
            {
                HandleCancelLoaded(true);
                return;
            }


            // 4. cancel the preview process
            CancelPreview();


            lock (_lock)
            {
                // update bitmap size after the preview is cancelled
                BitmapSize = e.Photo.Size;

                // 5. calculate the source viewport to match with the preview
                if (hasSource)
                {
                    // arm the render-completion signal before the draw is queued
                    _firstDrawTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                    // 5.1 set source based on type
                    if (e.Photo.Bitmap is SkiaVectorSource vectorSource)
                    {
                        HandleVectorPhotoLoaded(vectorSource);
                    }
                    else
                    {
                        _isFirstDraw.SetTrue();
                        SKImageRef.Set(ref _imgSource, imgFrame);

                        // keep (or clear) the retained raw HDR frame for live re-tone-mapping
                        SKImageRef.Set(ref _imgHdrSource, hdrRawToRetain);
                    }


                    // 5.2 set animator
                    if (animator is not null)
                    {
                        _animator?.FrameChanged -= Animator_FrameChanged;
                        _animator = animator;
                        _animator.FrameChanged += Animator_FrameChanged;
                        StartAnimator();
                    }


                    // 5.3 if user zoomed and panned the preview
                    if (_isPreviewing
                        && _zooming.IsManual
                        && ZoomMode != ZoomMode.LockZoom)
                    {
                        _isPreviewing.SetFalse();

                        var diffRatio = new Size(
                            prevSize.Width / BitmapSize.Width,
                            prevSize.Height / BitmapSize.Height);

                        // calculate new source rect
                        var newSrcRect = SrcRect;
                        newSrcRect = newSrcRect.WithX(newSrcRect.X / diffRatio.Width);
                        newSrcRect = newSrcRect.WithY(newSrcRect.Y / diffRatio.Height);
                        newSrcRect = newSrcRect.WithWidth(newSrcRect.Width / diffRatio.Width);
                        newSrcRect = newSrcRect.WithHeight(newSrcRect.Height / diffRatio.Height);

                        // update zoom source
                        SrcRect = newSrcRect.Normalize();
                        _logicalSrcPoint = SrcRect.Position;
                        _zooming.Factor *= diffRatio.Width;
                        _zooming.ZoomedPoint = new();

                        // trigger zoom changed event
                        ZoomChanged?.Invoke(this, new ViewerZoomEventArgs()
                        {
                            ZoomFactor = _zooming.Factor,
                            IsManualZoom = false,
                            IsZoomModeChange = false,
                            IsPreviewingImage = _isPreviewing,
                            ChangeSource = ZoomChangeSource.Unknown,
                        });

                        Refresh(false);
                    }
                    else
                    {
                        _isPreviewing.SetFalse();

                        // restore saved pan position for LockZoom
                        if (ZoomMode == ZoomMode.LockZoom && _lockZoomSavedSrcPoint is not null)
                        {
                            _logicalSrcPoint = _lockZoomSavedSrcPoint.Value;
                            _lockZoomSavedSrcPoint = null;
                        }

                        Refresh(_loadingOptions.ResetZoom);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            e.Photo.Error = ex; // the rendering error
            HandleCancelLoaded(false);
        }
        finally
        {
            // don't override VectorRenderer set by HandleVectorPhotoLoaded
            if (SourceKind != PhotoSource.VectorRenderer)
            {
                SourceKind = hasSource ? PhotoSource.Native : PhotoSource.None;
            }

            // raise events
            OnPhotoLoading(e);
            OnPhotoFrameChanged();
        }


        void HandleCancelLoaded(bool userCancelled)
        {
            // no draw is coming, release any waiter
            lock (_lock)
            {
                _firstDrawTcs?.TrySetResult();
            }

            if (userCancelled) e.Photo.Unload();

            imgFrame?.Dispose();
            imgFrame = null;

            // free the retained raw HDR frame if we hadn't stored it yet
            hdrRawToRetain?.Dispose();
            hdrRawToRetain = null;

            animator?.Dispose();
            animator = null;

            Refresh(userCancelled);
        }
    }


    /// <summary>
    /// Handles frame animation.
    /// </summary>
    private void Animator_FrameChanged(AnimatorImpl sender, AnimatorFrameChangedEventArgs e)
    {
        // pause animation when app is busy
        if (Core.IsBusy) return;

        // SVG animation: _svgPicture and InvalidateVisual() are already
        // handled by SvgAnimator.OnTimerTicked() under _lock.
        if (sender is SvgAnimator)
        {
            OnPhotoFrameChanged(e);
            return;
        }

        // Raster animation: update the frame bitmap
        var renderedFrame = sender.GetRenderedFrameBitmap(e.CurrentFrame);

        lock (_lock)
        {
            SourceKind = PhotoSource.Native;
            SKImageRef.Set(ref _imgSource, renderedFrame);
            SKImageRef.Set(ref _imgRender, renderedFrame, _imgSource);

            // animated sources have no image at load time, so the first render pass consumes
            // _isFirstDraw without drawing; the first frame is the real render signal
            _firstDrawTcs?.TrySetResult();
        }

        // variable-size frames: each frame is its own canvas
        if (!renderedFrame.IsDisposed()
            && (renderedFrame.Width != BitmapSize.Width || renderedFrame.Height != BitmapSize.Height))
        {
            RefitForFrameSize(new Size(renderedFrame.Width, renderedFrame.Height));
        }

        InvalidateVisual();
        OnPhotoFrameChanged(e);
    }


    /// <summary>
    /// Start animating the image if it can animate.
    /// </summary>
    public void StartAnimator()
    {
        if (IsImageAnimating || _animator is null || SourceKind == PhotoSource.None)
            return;

        lock (_lock)
        {
            _animator.Play();

            // emit frame changed event
            OnPhotoFrameChanged();
        }
    }


    /// <summary>
    /// Stop animating the image.
    /// </summary>
    public void StopAnimator()
    {
        lock (_lock)
        {
            if (_animator is null) return;
            _animator.Pause();

            // emit frame changed event
            OnPhotoFrameChanged();
        }
    }


    /// <summary>
    /// Views a single frame for multi-frame images.
    /// </summary>
    public async Task ViewFrameAsync(uint frameIndex)
    {
        if (Photo is null) return;

        // pause the animator if it's running
        if (IsImageAnimating) StopAnimator();

        var imgFrame = await Photo.GetFrameAsync(frameIndex);
        if (imgFrame is null) return;

        // apply color space off the UI thread
        SKImage? colored;
        using (Photo.PinBitmap())
        {
            colored = await ApplySkiaColorSpaceAsync(imgFrame, Photo.Metadata);
        }

        var sourceImg = colored ?? imgFrame;

        lock (_lock)
        {
            _mipmapCache?.Dispose();
            _mipmapCache = null;

            SKImageRef.Set(ref _imgRender, null);
            SKImageRef.Set(ref _imgSource, sourceImg);
            _isFirstDraw.SetTrue();
        }

        // variable-size frames: each frame is its own canvas. Photo.Size still reports
        // the metadata size for animated sources, so measure the decoded frame itself.
        var frameSize = sourceImg.IsDisposed()
            ? Photo.Size
            : new Size(sourceImg.Width, sourceImg.Height);

        if (frameSize != BitmapSize)
        {
            RefitForFrameSize(frameSize);
            InvalidateVisual();
        }
        else
        {
            Refresh(resetZoom: _loadingOptions.ResetZoom);
        }

        // emit frame changed event
        OnPhotoFrameChanged();
    }


    #endregion // Control Methods


}
