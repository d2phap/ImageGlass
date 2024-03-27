/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

---------------------
ImageGlass.WinTouch is based on WinTouch.NET
Url: https://github.com/rprouse/WinTouch.NET
License: MIT
---------------------
*/
namespace ImageGlass.WinTouch;


/// <summary>
/// Base class for all the gesture based events
/// </summary>
public abstract class GestureEventArgs : EventArgs
{
    #region Properties

    protected GestureInfo Info { get; set; }


    /// <summary>
    /// Gets the location of the gesture in Screen (not client) coordinates.
    /// </summary>
    public Point Location { get; private set; }


    /// <summary>
    /// Gets a value indicating whether this <see cref="GestureEventArgs"/> is beginning.
    /// </summary>
    public bool Begin => Info.Begin;


    /// <summary>
    /// Gets a value indicating whether this <see cref="GestureEventArgs"/> is ending
    /// </summary>
    public bool End => Info.End;


    /// <summary>
    /// Gets or sets a value indicating whether the window message was handled. Set this to false if you don't handle the message.
    /// </summary>
    public bool Handled { get; set; }

    #endregion


    internal GestureEventArgs(GestureInfo info)
    {
        Location = info.Location;
        Info = info;
        Handled = true;
    }


    // Helper Methods
    #region Helper Methods

    protected static int LoDWord(long l)
    {
        return (int)(l & 0xFFFFFFFF);
    }

    protected static int HiDWord(long l)
    {
        return (int)((l >> 32) & 0xFFFFFFFF);
    }

    protected static short LoWord(int i)
    {
        return (short)(i & 0xFFFF);
    }

    protected static short HiWord(int i)
    {
        return (short)((i >> 16) & 0xFFFF);
    }

    #endregion // Helper Methods

}



/// <summary>
/// Event for the Pan gesture
/// </summary>
public class PanEventArgs : GestureEventArgs
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="GestureEventArgs"/> has triggered inertia.
    /// </summary>
    public bool Inertia => Info.Inertia;


    public Point InertiaVector { get; private set; }


    /// <summary>
    /// Gets the pan offset since the last pan message.
    /// </summary>
    public Point PanOffset { get; private set; }


    internal PanEventArgs(GestureInfo info, Point lastPanPoint) : base(info)
    {
        var hiword = HiDWord((long)info.Arguments);

        InertiaVector = new Point(LoWord(hiword), HiWord(hiword));
        PanOffset = new Point(Location.X - lastPanPoint.X, Location.Y - lastPanPoint.Y);
    }
}



/// <summary>
/// Event for the Zoom gesture
/// </summary>
public class ZoomEventArgs : GestureEventArgs
{
    /// <summary>
    /// Gets the distance between the two points as they are being zoomed.
    /// </summary>
    public long Distance { get; private set; }


    /// <summary>
    /// Gets the percent changed since the last zoom message
    /// </summary>
    public double PercentChange { get; private set; }


    internal ZoomEventArgs(GestureInfo info, long lastZoomDistance) : base(info)
    {
        Distance = (long)info.Arguments;
        PercentChange = (double)Distance / lastZoomDistance;
    }

}



/// <summary>
/// Event for the Press and Tap gesture
/// </summary>
public class PressAndTapEventArgs : GestureEventArgs
{
    /// <summary>
    /// Gets the distance between the two points.
    /// </summary>
    public Point Distance { get; private set; }


    internal PressAndTapEventArgs(GestureInfo info) : base(info)
    {
        int pointsStruct = LoDWord((long)info.Arguments);
        Distance = new Point(LoWord(pointsStruct), HiWord(pointsStruct));
    }

}



/// <summary>
/// Event for the Rotate gesture
/// </summary>
public class RotateEventArgs : GestureEventArgs
{
    /// <summary>
    /// Gets the angle of rotation in Radians since the beginning of the gesture
    /// </summary>
    public double TotalAngle { get; private set; }


    /// <summary>
    /// Gets the angle of rotation in Degrees since the beginning of the gesture
    /// </summary>
    public double TotalDegrees => RadiandsToDegrees(TotalAngle);


    /// <summary>
    /// Gets the angle of rotation in Radians since the last rotation message
    /// </summary>
    public double Angle { get; private set; }


    /// <summary>
    /// Gets the angle of rotation in Degrees since the last rotation message
    /// </summary>
    public double Degrees => RadiandsToDegrees(Angle);


    internal RotateEventArgs(GestureInfo info, double lastRotation) : base(info)
    {
        var loword = LoDWord((long)info.Arguments);

        TotalAngle = RotateAngleFromArgument(loword);
        Angle = TotalAngle - lastRotation;
    }


    private static double RadiandsToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }


    /// <summary>
    /// Gesture argument helper that converts an argument to a rotation angle.
    /// </summary>
    /// <param name="arg">The argument to convert. Should be an unsigned 16-bit value.</param>
    /// <returns></returns>
    private static double RotateAngleFromArgument(int arg)
    {
        return ((arg / 65535.0) * 4.0 * Math.PI) - 2.0 * Math.PI;
    }

}



/// <summary>
/// Base class for the Two Finger Tap gesture
/// </summary>
public class TwoFingerTapEventArgs : GestureEventArgs
{
    /// <summary>
    /// Gets the distance between the two points.
    /// </summary>
    public long Distance { get; private set; }


    internal TwoFingerTapEventArgs(GestureInfo info) : base(info)
    {
        Distance = (long)info.Arguments;
    }
}

