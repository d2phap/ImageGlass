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

---------------------
ImageGlass.WinTouch is based on WinTouch.NET
Url: https://github.com/rprouse/WinTouch.NET
License: MIT
---------------------
*/

namespace ImageGlass.WinTouch;

/// <summary>
/// Provides window gestures events
/// </summary>
public sealed class GestureListener : NativeWindow
{
    private const int WM_GESTURE = 0x0119;


    // Saved state
    private Point _lastPanPoint;
    private double _lastRotation;
    private long _lastZoom;
    private readonly GestureConfig[] m_configs;


    // Public Events
    public event EventHandler<PanEventArgs>? Pan;
    public event EventHandler<PressAndTapEventArgs>? PressAndTap;
    public event EventHandler<RotateEventArgs>? Rotate;
    public event EventHandler<TwoFingerTapEventArgs>? TwoFingerTap;
    public event EventHandler<ZoomEventArgs>? Zoom;


    /// <summary>
    /// Initializes a new instance of the <see cref="GestureListener"/> class to receive all gestures.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public GestureListener(Control parent) : this(parent, new[] {
        new GestureConfig(GestureConfigId.GID_PAN,
            GestureConfigFlags.GC_PAN_WITH_SINGLE_FINGER_VERTICALLY
            | GestureConfigFlags.GC_PAN_WITH_SINGLE_FINGER_VERTICALLY
            | GestureConfigFlags.GC_PAN_WITH_INERTIA,

            // block GC_PAN_WITH_GUTTER for proper panning:
            // https://stackoverflow.com/a/38367704/2856887
            GestureConfigFlags.GC_PAN_WITH_GUTTER),

        new GestureConfig(GestureConfigId.GID_ZOOM, GestureConfigFlags.GC_ZOOM, GestureConfigFlags.None),
    })
    { }


    /// <summary>
    /// Initializes a new instance of the <see cref="GestureListener"/> class to receive specific gestures.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="configs">The gesture configurations.</param>
    public GestureListener(Control parent, GestureConfig[] configs)
    {
        m_configs = configs;

        if (parent.IsHandleCreated)
        {
            Initialize(parent);
        }
        else
        {
            parent.HandleCreated += OnHandleCreated;
        }

        parent.HandleDestroyed += OnHandleDestroyed;
    }



    // Private Methods
    #region Private Methods

    private void Initialize(Control parent)
    {
        AssignHandle(parent.Handle);
        NativeMethods.SetGestureConfig(parent.Handle, m_configs);
    }


    private void OnHandleCreated(object? sender, EventArgs e)
    {
        // Window is now created, assign handle to NativeWindow.
        var control = sender as Control;
        if (control != null)
        {
            Initialize(control);
        }
    }


    private void OnHandleDestroyed(object? sender, EventArgs e)
    {
        // Window was destroyed, release hook.
        ReleaseHandle();
    }

    #endregion // Private Methods


    // WndProc
    #region WndProc

    /// <summary>
    /// Invokes the default window procedure associated with this window.
    /// </summary>
    /// <param name="m">A <see cref="T:System.Windows.Forms.Message"/> that is associated
    /// with the current Windows message.</param>
    protected override void WndProc(ref Message m)
    {
        bool handled = false;

        // Listen for operating system messages
        switch (m.Msg)
        {
            case WM_GESTURE:
                if (NativeMethods.GetGestureInfo(m.LParam) is GestureInfo info)
                {
                    switch (info.Id)
                    {
                        case GestureInfoId.GID_PAN:
                            handled = OnPan(info);
                            break;
                        case GestureInfoId.GID_PRESSANDTAP:
                            handled = OnPressAndTap(info);
                            break;
                        case GestureInfoId.GID_ROTATE:
                            handled = OnRotate(info);
                            break;
                        case GestureInfoId.GID_TWOFINGERTAP:
                            handled = OnTwoFingerTap(info);
                            break;
                        case GestureInfoId.GID_ZOOM:
                            handled = OnZoom(info);
                            break;
                    }

                    if (handled)
                    {
                        NativeMethods.CloseGestureInfoHandle(m.LParam);
                    }
                }
                break;
        }

        if (!handled)
        {
            base.WndProc(ref m);
        }
    }


    private bool OnPan(GestureInfo info)
    {
        if (Pan != null)
        {
            if (info.Begin)
            {
                _lastPanPoint = new Point(info.Location.X, info.Location.Y);
            }

            var args = new PanEventArgs(info, _lastPanPoint);
            _lastPanPoint = new Point(info.Location.X, info.Location.Y);

            Pan(this, args);
            return args.Handled;
        }

        return false;
    }


    private bool OnPressAndTap(GestureInfo info)
    {
        if (PressAndTap != null)
        {
            var args = new PressAndTapEventArgs(info);
            PressAndTap(this, args);

            return args.Handled;
        }

        return false;
    }


    private bool OnRotate(GestureInfo info)
    {
        if (Rotate != null)
        {
            if (info.Begin)
            {
                _lastRotation = 0;
            }

            var args = new RotateEventArgs(info, _lastRotation);
            if (!info.Begin)
            {
                // First rotation is the angle the fingers are at, so don't use it
                _lastRotation = args.TotalAngle;
            }

            Rotate(this, args);
            return args.Handled;
        }
        return false;
    }


    private bool OnTwoFingerTap(GestureInfo info)
    {
        if (TwoFingerTap != null)
        {
            var args = new TwoFingerTapEventArgs(info);
            TwoFingerTap(this, args);

            return args.Handled;
        }

        return false;
    }


    private bool OnZoom(GestureInfo info)
    {
        if (Zoom != null)
        {
            if (info.Begin)
            {
                _lastZoom = (long)info.Arguments;
            }

            var args = new ZoomEventArgs(info, _lastZoom);
            _lastZoom = args.Distance;

            Zoom(this, args);
            return args.Handled;
        }

        return false;
    }

    #endregion // WndProc

}

