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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace ImageGlass.Base.WinApi;
public static class ExplorerApi
{
    #region COM and WinAPI


    [DllImport("shell32.dll", SetLastError = true)]
    private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder,
        uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, uint dwFlags);

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SHParseDisplayName(
        [MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext,
        [Out] out IntPtr pidl, uint sfgaoIn, [Out] out uint psfgaoOut);

    [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);


    internal static readonly Guid SID_STopLevelBrowser = new(
        0x4C96BE40, 0x915C, 0x11CF, 0x99, 0xD3, 0x00, 0xAA, 0x00, 0x4A, 0xE8, 0x37);
    internal static readonly Guid SID_ShellWindows = new(
        0x9BA05972, 0xF6A8, 0x11CF, 0xA4, 0x42, 0x00, 0xA0, 0xC9, 0x0A, 0x8F, 0x39);

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    internal interface IServiceProvider
    {
        void QueryService(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
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
        void GetFolder(
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        // the rest is not defined
    }

    #endregion


    /// <summary>
    /// Open folder and select an item.
    /// </summary>
    /// <remarks>
    /// <see cref="SHParseDisplayName"/> will not always find the correct folder. If the user
    /// has a folder open that is rooted in their user folder (e.g. the desktop,
    /// Dropbox/Mega/Nextcloud folder), this won't match the folder reference
    /// returned by <see cref="SHParseDisplayName"/> if given the actual path
    /// of the same folder. This will result in opening a duplicate folder.
    /// 
    /// So instead, we iterate through open folder windows for a path match first,
    /// then use the old method if one is not found.
    /// </remarks>
    /// <param name="filePath">Full path to a file to highlight in Explorer</param>
    public static void OpenFolderAndSelectItem(string filePath)
    {
        var folderPath = Path.GetDirectoryName(filePath);
        bool opened = false;

        var shellWindowsType = Type.GetTypeFromCLSID(SID_ShellWindows);
        var shellWindows = (dynamic?)Activator.CreateInstance(shellWindowsType);

        try
        {
            foreach (IServiceProvider sp in shellWindows)
            {
                var pidl = IntPtr.Zero;
                var nativeFile = IntPtr.Zero;

                try
                {
                    sp.QueryService(SID_STopLevelBrowser, typeof(IShellBrowser).GUID, out object sb);
                    IShellBrowser shellBrowser = (IShellBrowser)sb;

                    shellBrowser.QueryActiveShellView(out object sv);

                    if (sv is IFolderView fv)
                    {
                        // only folder implementation support this (IE windows do not for example)
                        fv.GetFolder(typeof(IPersistFolder2).GUID, out object pf);
                        IPersistFolder2 persistFolder = (IPersistFolder2)pf;

                        // get current folder pidl
                        persistFolder.GetCurFolder(out pidl);

                        var path = new StringBuilder(1024);
                        if (SHGetPathFromIDList(pidl, path))
                        {
                            if (string.Equals(path.ToString(), folderPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                SHParseDisplayName(Path.Combine(folderPath, filePath), IntPtr.Zero, out nativeFile, 0, out _);

                                IntPtr[] fileArray;
                                if (nativeFile == IntPtr.Zero)
                                {
                                    // Open the folder without the file selected
                                    // if we can't find the file
                                    fileArray = [];
                                }
                                else
                                {
                                    fileArray = new IntPtr[] { nativeFile };
                                }

                                _ = SHOpenFolderAndSelectItems(pidl, (uint)fileArray.Length, fileArray, 0);
                                opened = true;

                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if (nativeFile != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(nativeFile);
                    }

                    if (pidl != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pidl);
                    }

                    Marshal.ReleaseComObject(sp);
                }
            }
        }
        finally
        {
            Marshal.FinalReleaseComObject(shellWindows);
        }

        if (!opened)
        {
            SHParseDisplayName(folderPath, IntPtr.Zero, out IntPtr nativeFolder, 0, out _);

            if (nativeFolder == IntPtr.Zero)
            {
                // Log error, can't find folder
                return;
            }

            SHParseDisplayName(Path.Combine(folderPath, filePath), IntPtr.Zero, out IntPtr nativeFile, 0, out _);

            IntPtr[] fileArray;
            if (nativeFile == IntPtr.Zero)
            {
                // Open the folder without the file selected if we can't find the file
                fileArray = [];
            }
            else
            {
                fileArray = new IntPtr[] { nativeFile };
            }

            _ = SHOpenFolderAndSelectItems(nativeFolder, (uint)fileArray.Length, fileArray, 0);

            Marshal.FreeCoTaskMem(nativeFolder);
            if (nativeFile != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(nativeFile);
            }
        }
    }


    /// <summary>
    /// Show file property dialog
    /// </summary>
    /// <param name="filePath">Full file path</param>
    /// <param name="hwnd">Form handle</param>
    public static unsafe void DisplayFileProperties(string filePath, IntPtr hwnd)
    {
        const int SEE_MASK_INVOKEIDLIST = 0xc;
        const int SW_SHOW = 5;
        var shInfo = new SHELLEXECUTEINFOW();

        fixed (char* pFilePath = filePath)
        {
            fixed (char* pVerb = "properties")
            {
                fixed (char* pParams = "Details")
                {
                    shInfo.cbSize = (uint)Marshal.SizeOf(shInfo);
                    shInfo.lpFile = pFilePath;
                    shInfo.nShow = SW_SHOW;
                    shInfo.fMask = SEE_MASK_INVOKEIDLIST;
                    shInfo.lpVerb = pVerb;
                    shInfo.lpParameters = pParams;
                    shInfo.hwnd = new HWND(hwnd);
                }
            }
        }

        _ = PInvoke.ShellExecuteEx(ref shInfo);
    }


    /// <summary>
    /// Apply system theme to control.
    /// </summary>
    public static bool SetWindowTheme(IntPtr handle, string subIdList = "", bool darkMode = true)
    {
        return SetWindowTheme(handle, darkMode ? "DarkMode_Explorer" : "Explorer", subIdList);
    }


    /// <summary>
    /// Apply system theme to control.
    /// </summary>
    public static bool SetWindowTheme(IntPtr handle, string subAppName, string subIdList = "")
    {
        var result = PInvoke.SetWindowTheme(new HWND(handle), subAppName, subIdList);

        return result.Succeeded;
    }


    // File type associations & App protocol
    #region File type associations & App protocol

    /// <summary>
    /// Register file type associations and app capabilities to registry
    /// </summary>
    /// <param name="extensions">Extension string, ex: <c>.png;.svg;</c></param>
    public static Exception? RegisterAppAndExtensions(string extensions)
    {
        const string APP_NAME = "ImageGlass";
        var capabilitiesPath = $@"Software\{APP_NAME}\Capabilities";

        _ = UnregisterAppAndExtensions(extensions);


        try
        {
            // register the application:
            // HKEY_CURRENT_USER\SOFTWARE\RegisteredApplications --------------------------------
            const string regAppPath = @"Software\RegisteredApplications";
            using (var key = Registry.CurrentUser.OpenSubKey(regAppPath, true))
            {
                key?.SetValue(APP_NAME, capabilitiesPath);
            }


            // register application information:
            // HKEY_CURRENT_USER\SOFTWARE\ImageGlass\Capabilities -------------------------------
            using (var key = Registry.CurrentUser.CreateSubKey(capabilitiesPath, true))
            {
                key?.SetValue("ApplicationName", App.AppName);
                key?.SetValue("ApplicationIcon", $"\"{App.IGExePath}\", 0");
                key?.SetValue("ApplicationDescription", "A lightweight, versatile image viewer");


                // Register application's file type associations:
                // HKEY_CURRENT_USER\SOFTWARE\ImageGlass\Capabilities\FileAssociations ----------
                using (var faKey = key?.CreateSubKey("FileAssociations", true))
                {
                    var exts = extensions.Split(";", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var ext in exts)
                    {
                        var extNoDot = ext[1..].ToUpperInvariant();
                        var extAssocKey = $"{APP_NAME}.AssocFile.{extNoDot}";
                        var extAssocPath = $@"Software\Classes\{extAssocKey}";

                        // register supported extension
                        faKey?.SetValue(ext, extAssocKey);


                        // write extension info
                        // HKEY_CURRENT_USER\SOFTWARE\Classes\ImageGlass.AssocFile.<EXT>
                        using (var extRootKey = Registry.CurrentUser.CreateSubKey(extAssocPath, true))
                        {
                            // DefaultIcon -------------------------------------------------------
                            // get extension icon
                            var extDir = App.ConfigDir(PathType.Dir, Dir.ExtIcons);
                            var iconPath = Path.Combine(extDir, $"{extNoDot}.ico");
                            if (!File.Exists(iconPath))
                            {
                                iconPath = App.StartUpDir(Dir.ExtIcons, $"{extNoDot}.ico");

                                if (!File.Exists(iconPath))
                                {
                                    iconPath = string.Empty;
                                }
                            }


                            // set extension icon
                            if (!string.IsNullOrEmpty(iconPath))
                            {
                                using (var faIconKey = extRootKey?.CreateSubKey("DefaultIcon", true))
                                {
                                    faIconKey?.SetValue("", iconPath);
                                }
                            }


                            // shell/open --------------------------------------------------------
                            using (var shellOpenKey = extRootKey?.CreateSubKey(@"shell\open", true))
                            {
                                shellOpenKey?.SetValue("FriendlyAppName", App.AppName);


                                // shell/open/command --------------------------------------------
                                using var shellOpenCmdKey = shellOpenKey?.CreateSubKey("command", true);
                                shellOpenCmdKey?.SetValue("", $"\"{App.IGExePath}\" \"%1\"");
                            }
                        }
                    }
                }
            }


            // register app protocol
            _ = RegisterAppProtocol();


            unsafe
            {
                // notify system change
                PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST, (byte*)IntPtr.Zero, (byte*)IntPtr.Zero);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Unregister file type associations and app information from registry
    /// </summary>
    /// <param name="extensions">Extensions string to delete. Ex: <c>.png;.svg;</c></param>
    public static Exception? UnregisterAppAndExtensions(string extensions)
    {
        const string APP_NAME = "ImageGlass";

        // remove registry of ImageGlass v8
        _ = UnregisterAppAndExtensionsLegacy(extensions);

        try
        {
            // Unregister the application:
            // HKEY_CURRENT_USER\SOFTWARE\RegisteredApplications --------------------------------
            const string regAppPath = @"Software\RegisteredApplications";
            using (var key = Registry.CurrentUser.OpenSubKey(regAppPath, true))
            {
                if (key.OpenSubKey(APP_NAME, true) != null)
                {
                    key?.DeleteValue(APP_NAME);
                }
            }


            // Delete application information:
            // HKEY_CURRENT_USER\SOFTWARE\ImageGlass --------------------------------------------
            using (var key = Registry.CurrentUser.OpenSubKey("Software", true))
            {
                key?.DeleteSubKeyTree(APP_NAME, false);
            }


            // Delete file type associations
            // HKEY_CURRENT_USER\Software\Classes\ImageGlass.AssocFile.<EXT>
            var exts = extensions.Split(";", StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in exts)
            {
                var extNoDot = ext[1..].ToUpperInvariant();
                var extAssocPath = $@"Software\Classes\{APP_NAME}.AssocFile.{extNoDot}";

                Registry.CurrentUser.DeleteSubKeyTree(extAssocPath, false);
            }


            // Delete app protocol
            _ = UnregisterAppProtocol();


            unsafe
            {
                // notify system change
                PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_ASSOCCHANGED, SHCNF_FLAGS.SHCNF_IDLIST, (byte*)IntPtr.Zero, (byte*)IntPtr.Zero);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Unregister file type associations and app information from registry for <b>ImageGlass v8</b>.
    /// </summary>
    /// <param name="extensions">Extensions string to delete. Ex: <c>.png;.svg;</c></param>
    public static Exception? UnregisterAppAndExtensionsLegacy(string extensions)
    {
        const string APP_NAME = "ImageGlass";

        try
        {
            // Unregister the application:
            // HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications --------------------------------
            const string regAppPath = @"Software\RegisteredApplications";
            using (var key = Registry.LocalMachine.OpenSubKey(regAppPath, true))
            {
                if (key.OpenSubKey(APP_NAME, true) != null)
                {
                    key?.DeleteValue(APP_NAME);
                }
            }


            // Delete application information:
            // HKEY_LOCAL_MACHINE\SOFTWARE\ImageGlass --------------------------------------------
            using (var key = Registry.LocalMachine.OpenSubKey("Software", true))
            {
                key?.DeleteSubKeyTree(APP_NAME, false);
            }


            // Delete file type associations
            // HKEY_CLASSES_ROOT\ImageGlass.AssocFile.<EXT>
            var exts = extensions.Split(";", StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in exts)
            {
                var extNoDot = ext[1..].ToUpperInvariant();
                var extAssocKey = $"{APP_NAME}.AssocFile.{extNoDot}";

                Registry.ClassesRoot.DeleteSubKeyTree(extAssocKey, false);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Register app protocol to registry.
    /// </summary>
    public static Exception? RegisterAppProtocol()
    {
        try
        {
            // HKEY_CURRENT_USER\Software\Classes\<APP_PROTOCOL> --------------------------------
            const string protocolPath = $@"Software\Classes\{Const.APP_PROTOCOL}";
            using (var key = Registry.CurrentUser.CreateSubKey(protocolPath, true))
            {
                key?.SetValue("", $"URL: {App.AppName} Protocol");
                key?.SetValue("URL Protocol", "");

                // set protocol icon
                // HKEY_CURRENT_USER\Software\Classes\<APP_PROTOCOL>\DefaultIcon ----------------
                using (var subKey = key.CreateSubKey(@"DefaultIcon", true))
                {
                    subKey?.SetValue("", $"\"{App.IGExePath}\", 0");
                }

                // set protocol command
                // HKEY_CURRENT_USER\Software\Classes\<APP_PROTOCOL>\shell\open\command ---------
                using (var subKey = key.CreateSubKey(@"shell\open\command", true))
                {
                    subKey?.SetValue("", $"\"{App.IGExePath}\" \"%1\"");
                }
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Delete app protocol from registry.
    /// </summary>
    public static Exception? UnregisterAppProtocol()
    {
        try
        {
            // HKEY_CURRENT_USER\Software\Classes -----------------------------------------------
            const string protocolPath = $@"Software\Classes";
            using (var key = Registry.CurrentUser.OpenSubKey(protocolPath, true))
            {
                // delete tree:
                // HKEY_CURRENT_USER\Software\Classes\<APP_PROTOCOL> ----------------------------
                key?.DeleteSubKeyTree(Const.APP_PROTOCOL, false);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }

    #endregion // File type associations & App protocol


}
