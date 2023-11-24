/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ImageGlass.Base;

namespace ImageGlass.Library.WinAPI {
    public static class Explorer {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);


        internal static readonly Guid SID_STopLevelBrowser = new Guid(0x4C96BE40, 0x915C, 0x11CF, 0x99, 0xD3, 0x00, 0xAA, 0x00, 0x4A, 0xE8, 0x37);
        internal static readonly Guid SID_ShellWindows = new Guid(0x9BA05972, 0xF6A8, 0x11CF, 0xA4, 0x42, 0x00, 0xA0, 0xC9, 0x0A, 0x8F, 0x39);

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
        internal interface IServiceProvider
        {
            void QueryService([MarshalAs(UnmanagedType.LPStruct)] Guid guidService, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("1AC3D9F0-175C-11d1-95BE-00609797EA4F")]
        internal interface IPersistFolder2
        {
            void GetClassID(out Guid pClassID);
            void Initialize(IntPtr pidl);
            void GetCurFolder(out IntPtr pidl);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E2-0000-0000-C000-000000000046")]
        internal interface IShellBrowser
        {
            void _VtblGap0_12(); // skip 12 members
            void QueryActiveShellView([MarshalAs(UnmanagedType.IUnknown)] out object ppshv);
            // the rest is not defined
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("cde725b0-ccc9-4519-917e-325d72fab4ce")]
        internal interface IFolderView
        {
            void _VtblGap0_2(); // skip 2 members
            void GetFolder([MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
            // the rest is not defined
        }


        /// <summary>
        /// Open folder and select an item.
        /// </summary>
        /// <remarks>
        /// SHParseDisplayName will not always find the correct folder. If the user has a folder open that is rooted in their user folder (e.g. the desktop, Dropbox/Mega/Nextcloud folder),
        /// this won't match the folder reference returned by SHParseDisplayName if given the actual path of the same folder. This will result in opening a duplicate folder.
        /// So instead, we iterate through open folder windows for a path match first, then use the old method if one is not found.
        /// </remarks>
        /// <param name="filePath">Full path to a file to highlight in Windows Explorer</param>
        public static void OpenFolderAndSelectItem(string filePath) {
            var folderPath = Path.GetDirectoryName(filePath);
            bool opened = false;

            var shellWindowsType = Type.GetTypeFromCLSID(SID_ShellWindows);
            var shellWindows = (dynamic)Activator.CreateInstance(shellWindowsType);

            try {
                foreach (IServiceProvider sp in shellWindows) {
                    var pidl = IntPtr.Zero;
                    var nativeFile = IntPtr.Zero;

                    try {
                        sp.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID, out object sb);
                        IShellBrowser shellBrowser = (IShellBrowser)sb;

                        shellBrowser.QueryActiveShellView(out object sv);
                        IFolderView fv = sv as IFolderView;

                        if (fv != null) {
                            // only folder implementation support this (IE windows do not for example)
                            fv.GetFolder(typeof(IPersistFolder2).GUID, out object pf);
                            IPersistFolder2 persistFolder = (IPersistFolder2)pf;

                            // get current folder pidl
                            persistFolder.GetCurFolder(out pidl);

                            var path = new StringBuilder(1024);
                            if (SHGetPathFromIDList(pidl, path)) {
                                if (string.Equals(path.ToString(), folderPath, StringComparison.InvariantCultureIgnoreCase)) {
                                    SHParseDisplayName(Path.Combine(folderPath, filePath), IntPtr.Zero, out nativeFile, 0, out _);

                                    IntPtr[] fileArray;
                                    if (nativeFile == IntPtr.Zero) {
                                        // Open the folder without the file selected if we can't find the file
                                        fileArray = new IntPtr[0];
                                    }
                                    else {
                                        fileArray = new IntPtr[] { nativeFile };
                                    }

                                    SHOpenFolderAndSelectItems(pidl, (uint)fileArray.Length, fileArray, 0);
                                    opened = true;

                                    break;
                                }
                            }
                        }
                    }
                    finally {
                        if (nativeFile != IntPtr.Zero) {
                            Marshal.FreeCoTaskMem(nativeFile);
                        }

                        if (pidl != IntPtr.Zero) {
                            Marshal.FreeCoTaskMem(pidl);
                        }

                        Marshal.ReleaseComObject(sp);
                    }
                }
            }
            finally {
                Marshal.FinalReleaseComObject(shellWindows);
            }

            if (!opened) {
                IntPtr nativeFolder;
                uint psfgaoOut;
                SHParseDisplayName(folderPath, IntPtr.Zero, out nativeFolder, 0, out psfgaoOut);

                if (nativeFolder == IntPtr.Zero) {
                    // Log error, can't find folder
                    return;
                }

                IntPtr nativeFile;
                SHParseDisplayName(Path.Combine(folderPath, filePath), IntPtr.Zero, out nativeFile, 0, out psfgaoOut);

                IntPtr[] fileArray;
                if (nativeFile == IntPtr.Zero) {
                    // Open the folder without the file selected if we can't find the file
                    fileArray = new IntPtr[0];
                }
                else {
                    fileArray = new IntPtr[] { nativeFile };
                }

                SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

                Marshal.FreeCoTaskMem(nativeFolder);
                if (nativeFile != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(nativeFile);
                }
            }
        }
    }
}
