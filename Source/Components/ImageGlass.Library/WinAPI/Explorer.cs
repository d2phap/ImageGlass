using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageGlass.Base;

namespace ImageGlass.Library.WinAPI {
    public static class Explorer {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

        public static void OpenFolderAndSelectItem(string filePath) {
            uint psfgaoOut;

            var folderPath = Path.GetDirectoryName(filePath);

            SHParseDisplayName(folderPath, IntPtr.Zero, out IntPtr nativeFolder, 0, out psfgaoOut);

            if (nativeFolder == IntPtr.Zero) {
                App.LogItAsync($"Explorer.OpenFolderAndSelectItem: Cannot find native folder for '{folderPath}'").RunSynchronously();
                throw new Exception($"Cannot find native folder for '{folderPath}'");
            }

            SHParseDisplayName(filePath, IntPtr.Zero, out IntPtr nativeFile, 0, out psfgaoOut);

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
