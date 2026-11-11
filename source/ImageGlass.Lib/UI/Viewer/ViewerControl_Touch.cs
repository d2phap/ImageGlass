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
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using ImageGlass.Common;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer.ZoomAndPan;

namespace ImageGlass.UI.Viewer;

public partial class ViewerControl
{
    private PhPinchGestureRecognizer _pinchGesture = new();


    /// <summary>
    /// Registers touch gestures.
    /// </summary>
    private void RegisterTouchGestures()
    {
        // add support for panning gesture
        GestureRecognizers.Add(new ScrollGestureRecognizer
        {
            CanHorizontallyScroll = true,
            CanVerticallyScroll = true,
            IsScrollInertiaEnabled = true,
        });


        // touch screen gestures
        _pinchGesture.Pinch += OnTouchPinched; // pinch gesture
        GestureRecognizers.Add(_pinchGesture);

        // panning
        ScrollGesture += ViewerControl_ScrollGesture; // panning
        ScrollGestureEnded += ViewerControl_ScrollGestureEnded;

        // touchpad gestures
        PointerTouchPadGestureMagnify += ViewerControl_PointerTouchPadGestureMagnify; // pinch
    }


    /// <summary>
    /// Removes the touch gestures.
    /// </summary>
    private void UnregisterTouchGestures()
    {
        // touch screen gestures
        _pinchGesture.Pinch -= OnTouchPinched; // pinch

        // panning
        ScrollGesture -= ViewerControl_ScrollGesture;
        ScrollGestureEnded -= ViewerControl_ScrollGestureEnded;

        // touchpad gestures
        PointerTouchPadGestureMagnify -= ViewerControl_PointerTouchPadGestureMagnify; // pinch
    }


    /// <summary>
    /// Handles panning event for touch screen.
    /// </summary>
    private void ViewerControl_ScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        var args = (ScrollGestureEventArgs)e;

        if (EnableSelection && CurrentSelectionAction != SelectionAction.None)
        {
            // TODO:
            // OnSelectionUpdating(position);
        }
        else
        {
            if (!_enablePanningVelocity) args.ShouldEndScrollGesture = true;

            // perform panning (touch path skips the RunApiAsync lock gate, so honor the pan lock)
            if (!FeatureManager.IsPanLocked()) _ = PanTo(args.Delta.X, args.Delta.Y, null);
        }

        e.Handled = true;
    }


    private void ViewerControl_ScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
        _enablePanningVelocity = true;
    }


    /// <summary>
    /// Handles pinch event for touch screen.
    /// </summary>
    private void OnTouchPinched(object? sender, PhPinchEventArgs e)
    {
        // 1. normalize expansion value to delta
        var delta = e.Expansion * ZoomInfo.MAX_ZOOM_SPEED;


        // 2. check if the manipulation is a pinch gesture
        var isPintching = delta != 0;
        if (!isPintching) return;


        // 3. perform panning (honor the pan lock; this touch path skips the RunApiAsync gate)
        if (!FeatureManager.IsPanLocked())
            PanTo(-e.Translation.X, -e.Translation.Y, e.Position, !isPintching);


        // 4. perform zooming for Pinch gesture
        if (isPintching && !FeatureManager.IsZoomLocked())
        {
            _ = ZoomByDeltaToPoint(delta, e.Position);
        }

        e.Handled = true;
    }


    /// <summary>
    /// Handles pinch event for touchpad.
    /// </summary>
    private void ViewerControl_PointerTouchPadGestureMagnify(object? sender, PointerDeltaEventArgs e)
    {
        var position = e.GetPosition(this);
        var delta = e.Delta.X;

        // macOS touchpad pinch gestures report a much smaller delta compared to Windows/Linux,
        // so we need to scale it up for better responsiveness
        if (BHelper.OS == OSType.Mac)
        {
            delta *= 1000;
        }

        // touchpad pinch skips the RunApiAsync lock gate, so honor the zoom lock here
        if (!FeatureManager.IsZoomLocked()) ZoomByDeltaToPoint(delta, position);
        e.Handled = true;
    }




}
