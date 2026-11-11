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
using Avalonia.Controls;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Devices.Display;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace ImageGlass.Win32.Common.ServiceProviders;

public partial class Win32ColorProfileProvider : PhDisposable, IWindowColorProfileProvider
{
    private Window? _window;
    private nint _windowHandle;
    private nint _currentMonitor;

    const int WM_COLORSPACECHANGED = 0x0320;
    const int WM_DISPLAYCHANGE = 0x007E;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event TEventHandler<IWindowColorProfileProvider, ColorProfileChangedEventArgs>? Changed;


    #region Public Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string ProfilePath { get; private set; } = string.Empty;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsHdr { get; private set; } = false;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsInitialized { get; private set; } = false;

    #endregion // Public Properties



    #region Instance Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();


        if (TopLevel.GetTopLevel(_window) is TopLevel top)
        {
            Win32Properties.RemoveWndProcHookCallback(top, WndProcHook);
        }

        if (_window != null)
        {
            _window.PositionChanged -= OnWindowMoved;
        }


        ProfilePath = string.Empty;
        IsHdr = false;
        IsInitialized = false;
    }



    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public void Initialize(Window window)
    {
        if (IsInitialized) return;

        IsInitialized = true;
        _window = window;
        _windowHandle = _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;

        if (_windowHandle == IntPtr.Zero) return;


        // get current monitor
        _currentMonitor = GetMonitorFromWindow(_windowHandle);

        // load profile of the monitor
        UpdateColorProfile();

        // listen to events
        _window.PositionChanged += OnWindowMoved;
        if (TopLevel.GetTopLevel(window) is TopLevel top)
        {
            Win32Properties.AddWndProcHookCallback(top, WndProcHook);
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<byte[]?> ReadColorProfileDataAsync()
    {
        if (string.IsNullOrEmpty(ProfilePath)) return null;

        var data = await File.ReadAllBytesAsync(ProfilePath);
        return data;
    }


    private IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_COLORSPACECHANGED || msg == WM_DISPLAYCHANGE)
        {
            // update profile of the monitor
            UpdateColorProfile();

            handled = true;
        }

        return IntPtr.Zero;
    }


    private void OnWindowMoved(object? sender, PixelPointEventArgs e)
    {
        if (_windowHandle == IntPtr.Zero) return;

        // check the current monitor of window
        var monitor = GetMonitorFromWindow(_windowHandle);
        if (monitor == _currentMonitor) return;
        _currentMonitor = monitor;


        // get the color profile of the current monitor
        UpdateColorProfile();
    }


    private void UpdateColorProfile()
    {
        // get profile of the monitor
        ProfilePath = GetColorProfilePath(_currentMonitor);
        IsHdr = IsHdrEnabled(_currentMonitor);

        Changed?.Invoke(this, new ColorProfileChangedEventArgs(ProfilePath, IsHdr));
    }


    #endregion // Instance Methods



    #region Public Static Methods

    /// <summary>
    /// Gets the monitor from window.
    /// </summary>
    public static nint GetMonitorFromWindow(nint windowHandle)
    {
        var monitor = PInvoke.MonitorFromWindow(new HWND(windowHandle),
            MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

        return monitor;
    }


    /// <summary>
    /// Gets color profile path of current monitor where the window is at.
    /// </summary>
    public static unsafe string GetColorProfilePath(nint hMonitor)
    {
        try
        {
            // get the monitor info
            var mi = new MONITORINFOEXW();
            mi.monitorInfo.cbSize = (uint)sizeof(MONITORINFOEXW);
            if (!PInvoke.GetMonitorInfo((HMONITOR)hMonitor, (MONITORINFO*)(void*)&mi)) return string.Empty;

            // get monitor name
            var deviceName = new string(mi.szDevice.AsSpan());
            var dc = PInvoke.CreateDCW(string.Empty, deviceName);
            if (dc.IsNull) return string.Empty;


            try
            {
                // get the length of profile path
                uint size = 0;
                _ = PInvoke.GetICMProfile(dc, ref size, null);

                // get the profile buffer
                var buffer = new char[size];
                _ = PInvoke.GetICMProfile(dc, ref size, buffer);

                // get the profile path
                var profilePath = new string(buffer, 0, (int)size - 1);
                if (!File.Exists(profilePath)) return string.Empty;

                return profilePath;
            }
            finally
            {
                PInvoke.DeleteDC(dc);
            }
        }
        catch { }

        return string.Empty;
    }


    /// <summary>
    /// Checks if HDR is enabled.
    /// </summary>
    public static unsafe bool IsHdrEnabled(nint hMonitor)
    {
        // get the monitor info
        var monitorInfo = new MONITORINFOEXW();
        monitorInfo.monitorInfo.cbSize = (uint)sizeof(MONITORINFOEXW);
        if (!PInvoke.GetMonitorInfo((HMONITOR)hMonitor, (MONITORINFO*)(void*)&monitorInfo)) return false;

        // get input monitor name
        var inputDeviceName = new string(monitorInfo.szDevice.AsSpan());


        uint pathCount, modeCount;
        if (PInvoke.GetDisplayConfigBufferSizes(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            &pathCount, &modeCount) != 0)
            return false;

        var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

        fixed (DISPLAYCONFIG_PATH_INFO* pPaths = paths)
        fixed (DISPLAYCONFIG_MODE_INFO* pModes = modes)
        {
            // query all monitor devices
            if (PInvoke.QueryDisplayConfig(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
                &pathCount, pPaths, &modeCount, pModes, null) != 0)
                return false;


            for (uint i = 0; i < pathCount; i++)
            {
                var sourceName = new DISPLAYCONFIG_SOURCE_DEVICE_NAME
                {
                    header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                    {
                        type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME,
                        size = (uint)sizeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME),
                        adapterId = paths[i].sourceInfo.adapterId,
                        id = paths[i].sourceInfo.id
                    }
                };

                // get monitor device info
                if (PInvoke.DisplayConfigGetDeviceInfo(ref sourceName.header) != 0)
                    continue;


                // get monitor device name
                var viewGdiDeviceName = new string(sourceName.viewGdiDeviceName.AsSpan());

                // check if this is the input monitor
                if (!viewGdiDeviceName.Equals(inputDeviceName, StringComparison.OrdinalIgnoreCase))
                    continue;


                // get the advanced color info
                var advancedColorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
                {
                    header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                    {
                        type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                        size = (uint)sizeof(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO),
                        adapterId = paths[i].targetInfo.adapterId,
                        id = paths[i].targetInfo.id
                    }
                };

                if (PInvoke.DisplayConfigGetDeviceInfo(ref advancedColorInfo.header) == 0)
                {
                    // check if HDR is enabled for this monitor
                    var isEnabled = advancedColorInfo.Anonymous.Anonymous.advancedColorEnabled;

                    return isEnabled;
                }
            }
        }

        return false;
    }

    #endregion // Public Static Methods


}




