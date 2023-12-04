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
*/
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.Touch;

namespace ImageGlass.WinTouch;

internal static class NativeMethods
{
    /// <summary>
    /// Registers the HWND to receive all gestures.
    /// </summary>
    /// <remarks>
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setgestureconfig"/>
    /// </remarks>
    /// <param name="hwnd">The HWND.</param>
    public static bool SetGestureConfig(IntPtr hwnd)
    {
        var configs = new GestureConfig[] {
            new GestureConfig
            {
                Id = 0,
                Want = GestureConfigFlags.GC_ALLGESTURES,
                Block = 0,
            },
        };

        return SetGestureConfig(hwnd, configs);
    }


    /// <summary>
    /// Registers the HWND to receive specific gestures.
    /// </summary>
    /// <remarks>
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setgestureconfig"/>
    /// </remarks>
    /// <param name="hwnd">The HWND.</param>
    /// <param name="configs">The gesture configurations</param>
    public static bool SetGestureConfig(IntPtr hwnd, IEnumerable<GestureConfig> configs)
    {
        var allConfigs = configs.Select(i => new GESTURECONFIG()
        {
            dwID = (GESTURECONFIG_ID)i.Id,
            dwWant = (uint)i.Want,
            dwBlock = (uint)i.Block,
        }).ToArray().AsSpan();

        var result = PInvoke.SetGestureConfig(new HWND(hwnd), (uint)allConfigs.Length, allConfigs,
            (uint)Marshal.SizeOf<GESTURECONFIG>());

        return result.Value == new BOOL(true);
    }


    /// <summary>
    /// Gets the gesture info.
    /// </summary>
    /// <param name="gestureInfoHandle">The gesture info handle.</param>
    public static GestureInfo? GetGestureInfo(IntPtr gestureInfoHandle)
    {
        var pGestureInfo = new GESTUREINFO();
        pGestureInfo.cbSize = (uint)Marshal.SizeOf(pGestureInfo);

        var result = PInvoke.GetGestureInfo(new HGESTUREINFO(gestureInfoHandle), out pGestureInfo);
        if (result.Value == new BOOL(false)) return null;

        return new GestureInfo()
        {
            Size = pGestureInfo.cbSize,
            Flags = (GestureInfoFlags)pGestureInfo.dwFlags,
            Id = (GestureInfoId)pGestureInfo.dwID,
            Hwnd = pGestureInfo.hwndTarget,
            Location = new Point(pGestureInfo.ptsLocation.x, pGestureInfo.ptsLocation.y),
            InstanceId = pGestureInfo.dwInstanceID,
            SequenceId = pGestureInfo.dwSequenceID,
            Arguments = pGestureInfo.ullArguments,
            ExtraArguments = pGestureInfo.cbExtraArgs,
        };
    }


    /// <summary>
    /// Closes the gesture info handle.
    /// </summary>
    /// <param name="gestureInfoHandle">The gesture info handle.</param>
    /// <returns></returns>
    public static bool CloseGestureInfoHandle(IntPtr gestureInfoHandle)
    {
        var result = PInvoke.CloseGestureInfoHandle(new HGESTUREINFO(gestureInfoHandle));

        return result.Value == new BOOL(true);
    }


}

