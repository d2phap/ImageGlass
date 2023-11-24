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
using System.Runtime.InteropServices;
using System.Text;

namespace ImageGlass.Base.WinApi;


public class DisplayApi
{
    [DllImport("mscms.dll", CharSet = CharSet.Unicode)]
    static extern bool GetColorDirectory(
        [MarshalAs(UnmanagedType.LPWStr)] string? pMachineName,
        StringBuilder pBuffer,
        ref uint pdwSize);

    [DllImport("mscms.dll", CharSet = CharSet.Auto)]
    static extern bool WcsGetUsePerUserProfiles(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceName,
        uint deviceClass,
        out bool usePerUserProfiles);

    [DllImport("mscms.dll", CharSet = CharSet.Auto)]
    static extern bool WcsGetDefaultColorProfileSize(
        WcsProfileManagementScope scope,
        [MarshalAs(UnmanagedType.LPWStr)] string deviceName,
        ColorProfileType colorProfileType,
        ColorProfileSubtype colorProfileSubType,
        uint dwProfileID,
        out uint cbProfileName);

    [DllImport("mscms.dll", CharSet = CharSet.Unicode)]
    static extern bool WcsGetDefaultColorProfile(
        WcsProfileManagementScope scope,
        [MarshalAs(UnmanagedType.LPWStr)] string deviceName,
        ColorProfileType colorProfileType,
        ColorProfileSubtype colorProfileSubType,
        uint dwProfileID,
        uint cbProfileName,
        StringBuilder pProfileName);

    enum WcsProfileManagementScope
    {
        SYSTEM_WIDE,
        CURRENT_USER
    }
    enum ColorProfileType
    {
        ICC,
        DMP,
        CAMP,
        GMMP
    };

    enum ColorProfileSubtype
    {
        PERCEPTUAL,
        RELATIVE_COLORIMETRIC,
        SATURATION,
        ABSOLUTE_COLORIMETRIC,
        NONE,
        RGB_WORKING_SPACE,
        CUSTOM_WORKING_SPACE,
        STANDARD_DISPLAY_COLOR_MODE,
        EXTENDED_DISPLAY_COLOR_MODE
    };

    const uint CLASS_MONITOR = 0x6d6e7472; //'mntr'
    const uint CLASS_PRINTER = 0x70727472; //'prtr'
    const uint CLASS_SCANNER = 0x73636e72; //'scnr'





    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorDefaults dwFlags);

    [DllImport("user32.dll")]
    extern static bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAYDEVICE lpDisplayDevice, uint dwFlags);

    enum MonitorDefaults
    {
        TONULL = 0,
        TOPRIMARY = 1,
        TONEAREST = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MONITORINFOEX
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct DISPLAYDEVICE
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }



    /// <summary>
    /// Gets color profile file path from window.
    /// </summary>
    /// <param name="windowHandle">Window handle</param>
    /// <returns>Full file path of the color profile file.</returns>
    public static string? GetMonitorColorProfileFromWindow(IntPtr windowHandle)
    {
        try
        {
            var hMonitor = MonitorFromWindow(windowHandle, MonitorDefaults.TONEAREST);

            return GetMonitorColorProfile(hMonitor);
        }
        catch { }

        return string.Empty;
    }


    private static string? GetMonitorColorProfile(IntPtr hMonitor)
    {
        var profileDir = new StringBuilder(255);
        var pDirSize = (uint)profileDir.Capacity;
        GetColorDirectory(null, profileDir, ref pDirSize);

        var mInfo = new MONITORINFOEX();
        mInfo.cbSize = (uint)Marshal.SizeOf(mInfo);
        if (!GetMonitorInfo(hMonitor, ref mInfo))
            return null;

        var dd = new DISPLAYDEVICE();
        dd.cb = (uint)Marshal.SizeOf(dd);
        if (!EnumDisplayDevices(mInfo.szDevice, 0, ref dd, 0))
            return null;

        WcsGetUsePerUserProfiles(dd.DeviceKey, CLASS_MONITOR, out var usePerUserProfiles);
        var scope = usePerUserProfiles ? WcsProfileManagementScope.CURRENT_USER : WcsProfileManagementScope.SYSTEM_WIDE;

        if (!WcsGetDefaultColorProfileSize(scope, dd.DeviceKey, ColorProfileType.ICC, ColorProfileSubtype.NONE, 0, out var size))
            return null;

        var profileName = new StringBuilder((int)size);
        if (!WcsGetDefaultColorProfile(scope, dd.DeviceKey, ColorProfileType.ICC, ColorProfileSubtype.NONE, 0, size, profileName))
            return null;

        return Path.Combine(profileDir.ToString(), profileName.ToString());
    }

}
