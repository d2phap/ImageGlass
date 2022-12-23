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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Runtime.InteropServices;


namespace ImageGlass.Library.WinAPI;

public static class PrintService
{
    [ComImport]
    [Guid("00000122-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDropTarget
    {
        int DragEnter(
            [In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObj,
            [In] int grfKeyState,
            [In] Point pt,
            [In, Out] ref int pdwEffect);

        int DragOver(
            [In] int grfKeyState,
            [In] Point pt,
            [In, Out] ref int pdwEffect);

        int DragLeave();

        int Drop(
            [In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObj,
            [In] int grfKeyState,
            [In] Point pt,
            [In, Out] ref int pdwEffect);
    }


    /// <summary>
    /// Open Print Pictures dialog
    /// </summary>
    /// <param name="filename">File to print</param>
    public static void OpenPrintPictures(string filename)
    {
        var dataObj = new DataObject(DataFormats.FileDrop, new string[] { filename });
        var memoryStream = new MemoryStream(4);
        var buffer = new byte[] { 5, 0, 0, 0 };

        memoryStream.Write(buffer, 0, buffer.Length);
        dataObj.SetData("Preferred DropEffect", memoryStream);

        var CLSID_PrintPhotosDropTarget = new Guid("60fd46de-f830-4894-a628-6fa81bc0190d");
        var dropTargetType = Type.GetTypeFromCLSID(CLSID_PrintPhotosDropTarget, true);
        var dropTarget = (IDropTarget?)Activator.CreateInstance(dropTargetType);

        dropTarget?.Drop(dataObj, 0, new Point(), 0);
    }
}
