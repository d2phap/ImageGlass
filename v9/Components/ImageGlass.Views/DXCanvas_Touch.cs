/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Base.WinApi;
using ImageGlass.WinTouch;

namespace ImageGlass.Viewer;

public partial class DXCanvas
{
    // touch support
    private GestureListener? _gesture;


    private void InitializeTouchGesture()
    {
        // initialize touch support
        if (WindowApi.IsTouchDevice())
        {
            _gesture = new GestureListener(this);

            _gesture.Pan += Gesture_Pan;
            _gesture.Zoom += Gesture_Zoom;
        }
    }


    private void Gesture_Zoom(object? sender, WinTouch.ZoomEventArgs e)
    {
        if (e.Begin || e.PercentChange == 0 || UseWebview2) return;


        var zoomPoint = PointToClient(e.Location);
        var newZoomFactor = _zoomFactor * (float)e.PercentChange;


        ZoomToPoint(newZoomFactor, zoomPoint);
    }


    private void Gesture_Pan(object? sender, PanEventArgs e)
    {
        if (UseWebview2) return;

        var clientLoc = PointToClient(e.Location);

        if (e.Begin)
        {
            _panHostFromPoint.X = clientLoc.X;
            _panHostFromPoint.Y = clientLoc.Y;
            _panHostToPoint.X = clientLoc.X;
            _panHostToPoint.Y = clientLoc.Y;
        }
        else
        {
            PanTo(-e.PanOffset.X, -e.PanOffset.Y);
        }
    }

}
