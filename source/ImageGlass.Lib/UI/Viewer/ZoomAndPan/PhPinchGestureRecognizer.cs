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
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using System;

namespace ImageGlass.UI.Viewer.ZoomAndPan;

public class PhPinchGestureRecognizer : GestureRecognizer
{
    private float _initialDistance;

    private IPointer? _firstContact;
    private Point _firstPoint;
    private IPointer? _secondContact;
    private Point _secondPoint;

    private Point _origin;
    private Point _previousCenter;
    private double _previousAngle;
    private double _previousScale = 1.0;


    // events
    public event EventHandler<PhPinchEventArgs>? Pinch;
    public event EventHandler<PinchEndedEventArgs>? PinchEnded;



    protected override void PointerPressed(PointerPressedEventArgs e)
    {
        if (Target is not Visual visual) return;
        if (e.Pointer.Type == PointerType.Mouse) return;

        // 1. update the first contact point
        if (_firstContact == null)
        {
            _firstContact = e.Pointer;
            _firstPoint = e.GetPosition(visual);
            return;
        }

        // 2. update the second contact point
        if (_secondContact == null && _firstContact != e.Pointer)
        {
            _secondContact = e.Pointer;
            _secondPoint = e.GetPosition(visual);
        }
        else
        {
            return;
        }


        // 3. get metadata
        if (_firstContact != null && _secondContact != null)
        {
            _initialDistance = GetDistance(_firstPoint, _secondPoint);
            _origin = new Point((_firstPoint.X + _secondPoint.X) / 2.0, (_firstPoint.Y + _secondPoint.Y) / 2.0);
            _previousCenter = _origin;
            _previousAngle = GetAngleDegreeFromPoints(_firstPoint, _secondPoint);

            Capture(_firstContact);
            Capture(_secondContact);
            e.PreventGestureRecognition();
        }
    }


    protected override void PointerReleased(PointerReleasedEventArgs e)
    {
        if (RemoveContact(e.Pointer))
        {
            e.PreventGestureRecognition();
        }
    }


    protected override void PointerMoved(PointerEventArgs e)
    {
        if (Target is not Visual visual) return;


        // update contact points
        if (_firstContact == e.Pointer)
        {
            _firstPoint = e.GetPosition(visual);
        }
        else if (_secondContact == e.Pointer)
        {
            _secondPoint = e.GetPosition(visual);
        }
        else
        {
            return;
        }


        // get metadata
        if (_firstContact != null && _secondContact != null)
        {
            var distance = GetDistance(_firstPoint, _secondPoint);
            var degree = GetAngleDegreeFromPoints(_firstPoint, _secondPoint);

            // normalize scale value as expansion value
            var scale = distance / _initialDistance;
            var scaleDiff = scale / _previousScale;
            var expansion = scaleDiff - 1.0;
            _previousScale = scale;

            // get translation
            var currentCenter = new Point(
                (_firstPoint.X + _secondPoint.X) / 2.0,
                (_firstPoint.Y + _secondPoint.Y) / 2.0);

            var translation = currentCenter - _previousCenter;
            _previousCenter = currentCenter;


            // raise event
            var pinchEventArgs = new PhPinchEventArgs(expansion, scale, _origin, degree, _previousAngle - degree, translation, currentCenter);
            _previousAngle = degree;
            Pinch?.Invoke(Target, pinchEventArgs);


            e.Handled = pinchEventArgs.Handled;
            e.PreventGestureRecognition();
        }

    }


    protected override void PointerCaptureLost(IPointer pointer)
    {
        RemoveContact(pointer);
    }


    private bool RemoveContact(IPointer pointer)
    {
        if (_firstContact == pointer || _secondContact == pointer)
        {
            if (_secondContact == pointer)
            {
                _secondContact = null;
            }

            if (_firstContact == pointer)
            {
                _firstContact = _secondContact;
                _secondContact = null;
            }

            _previousScale = 1.0;
            PinchEnded?.Invoke(Target, new PinchEndedEventArgs());
            return true;
        }

        return false;
    }


    private static float GetDistance(Point a, Point b)
    {
        var length = b - a;
        return (float)new Vector(length.X, length.Y).Length;
    }


    private static double GetAngleDegreeFromPoints(Point a, Point b)
    {
        var deltaX = a.X - b.X;
        var deltaY = -(a.Y - b.Y);
        var rad = System.Math.Atan2(deltaX, deltaY);
        var degree = ((rad * (180 / System.Math.PI))) + 180;

        return degree;
    }

}



public class PhPinchEventArgs : PinchEventArgs
{
    /// <summary>
    /// Gets the change of pinch scale value
    /// </summary>
    public double Expansion { get; }

    /// <summary>
    /// Gets the change in x-y screen coordinates.
    /// </summary>
    public Vector Translation { get; }

    /// <summary>
    /// Gets the current position of gesture.
    /// </summary>
    public Point Position { get; }


    public PhPinchEventArgs(double expansion,
        double scale,
        Point scaleOrigin,
        double angle,
        double angleDelta,
        Vector translation,
        Point position) : base(scale, scaleOrigin, angle, angleDelta)
    {
        Expansion = expansion;
        Translation = translation;
        Position = position;
    }

}