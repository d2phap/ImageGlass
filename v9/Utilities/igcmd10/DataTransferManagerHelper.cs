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
using Windows.ApplicationModel.DataTransfer;
using WinRT;

namespace igcmd10;

public static class DataTransferManagerHelper
{
    private static readonly Guid _dtm_iid = new(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

    private static IDataTransferManagerInterop DataTransferManagerInterop => DataTransferManager.As<IDataTransferManagerInterop>();


    /// <summary>
    /// Gets <see cref="DataTransferManager"/> from window handle
    /// </summary>
    /// <returns></returns>
    public static DataTransferManager GetForWindow(IntPtr hwnd)
    {
        var result = DataTransferManagerInterop.GetForWindow(hwnd, _dtm_iid);
        var dtm = MarshalInterface<DataTransferManager>.FromAbi(result);

        return dtm;
    }


    /// <summary>
    /// Shows Share dialog
    /// </summary>
    /// <param name="hwnd">Parent window handle</param>
    public static void ShowShareUIForWindow(IntPtr hwnd)
    {
        DataTransferManagerInterop.ShowShareUIForWindow(hwnd);
    }


    [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDataTransferManagerInterop
    {
        IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);

        void ShowShareUIForWindow(IntPtr appWindow);
    }
}
