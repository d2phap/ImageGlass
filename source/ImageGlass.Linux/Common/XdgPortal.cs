/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.Common;
using System;
using System.Diagnostics;
using System.IO;

namespace ImageGlass.Linux.Common;

/// <summary>
/// Thin wrapper over the XDG desktop portals (<c>org.freedesktop.portal.*</c>) and
/// the file-manager D-Bus interface (<c>org.freedesktop.FileManager1</c>), invoked
/// through the <c>gdbus</c> CLI that ships in the runtime.
/// </summary>
/// <remarks>
/// Using <c>gdbus</c> avoids a managed D-Bus dependency. Crucially, <c>gdbus call</c>
/// is SYNCHRONOUS — it waits for the method reply before exiting — so the message is
/// reliably delivered through the Flatpak D-Bus proxy even though the calling app
/// does not wait for the gdbus process. (Fire-and-forget <c>dbus-send</c> without
/// <c>--print-reply</c> proved unreliable from inside the sandbox.) These calls take
/// string/URI arguments only — no file-descriptor passing — and the app is granted
/// <c>--filesystem=host</c> plus <c>--talk-name=org.freedesktop.FileManager1</c>, so
/// the host backend can resolve the real paths. Trashing is handled separately
/// (see <see cref="FreeDesktopTrash"/>) because the Trash portal is unreliable on
/// some desktops; the OpenURI, Email and Print portals are not used because they
/// require FD passing for local files (see <see cref="OpenPath"/>).
/// </remarks>
internal static class XdgPortal
{
    private const string PORTAL_DEST = "org.freedesktop.portal.Desktop";
    private const string PORTAL_PATH = "/org/freedesktop/portal/desktop";
    private const string FM_DEST = "org.freedesktop.FileManager1";
    private const string FM_PATH = "/org/freedesktop/FileManager1";
    private const int QUERY_TIMEOUT_MS = 3_000;


    /// <summary>
    /// Sets the desktop wallpaper to <paramref name="filePath"/> via
    /// <c>org.freedesktop.portal.Wallpaper.SetWallpaperURI</c>. The portal shows
    /// its preview/confirm dialog. Best-effort and non-blocking.
    /// </summary>
    public static void SetWallpaper(string filePath)
    {
        Call(PORTAL_DEST, PORTAL_PATH, "org.freedesktop.portal.Wallpaper.SetWallpaperURI",
            "",     // parent_window
            ToFileUri(filePath),
            "{'show-preview': <true>, 'set-on': <'both'>}");
    }


    /// <summary>
    /// Opens a file or folder in its default handler with <c>xdg-open</c>, run on
    /// the host when sandboxed. Best-effort and non-blocking.
    /// </summary>
    /// <remarks>
    /// The OpenURI portal cannot be used here: it answers <c>file://</c> URIs with
    /// <c>Response(2)</c> (error) — local paths must go through <c>OpenFile</c> /
    /// <c>OpenDirectory</c>, which take a file descriptor that <c>gdbus</c> cannot pass.
    /// </remarks>
    public static void OpenPath(string path)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = "xdg-open";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.ArgumentList.Add(BHelper.GetRealPlatformPath(Path.GetFullPath(path)));

            // the sandbox's own xdg-open would just call the broken OpenURI portal
            BHelper.ApplyFlatpakHostSpawn(proc.StartInfo);
            proc.Start();
        }
        catch
        {
            // best-effort: xdg-open missing
        }
    }


    /// <summary>
    /// Opens <paramref name="dirPath"/> itself in the default file manager via
    /// <c>org.freedesktop.FileManager1.ShowFolders</c>. Best-effort and non-blocking.
    /// </summary>
    public static void ShowFolders(string dirPath)
    {
        Call(FM_DEST, FM_PATH, "org.freedesktop.FileManager1.ShowFolders",
            $"['{ToGVariantUri(dirPath)}']",    // URIs
            "");                                // startup_id
    }


    /// <summary>
    /// Reveals <paramref name="filePath"/> in the default file manager, selecting
    /// it (or showing its properties dialog when <paramref name="showProperties"/>
    /// is <c>true</c>) via the <c>org.freedesktop.FileManager1</c> interface.
    /// Best-effort and non-blocking.
    /// </summary>
    public static void ShowInFileManager(string filePath, bool showProperties = false)
    {
        var method = showProperties
            ? "org.freedesktop.FileManager1.ShowItemProperties"
            : "org.freedesktop.FileManager1.ShowItems";

        Call(FM_DEST, FM_PATH, method,
            $"['{ToGVariantUri(filePath)}']",   // URIs
            "");                                // startup_id
    }


    /// <summary>
    /// Whether the connection is metered, via <c>org.freedesktop.portal.NetworkMonitor</c>;
    /// <c>false</c> when the portal cannot tell, so an unknown cost never blocks an update.
    /// </summary>
    public static bool IsNetworkMetered()
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = "gdbus";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            var args = proc.StartInfo.ArgumentList;
            args.Add("call");
            args.Add("--session");
            args.Add("--dest"); args.Add(PORTAL_DEST);
            args.Add("--object-path"); args.Add(PORTAL_PATH);
            args.Add("--method"); args.Add("org.freedesktop.portal.NetworkMonitor.GetMetered");

            proc.Start();
            var output = proc.StandardOutput.ReadToEndAsync();
            _ = proc.StandardError.ReadToEndAsync();

            if (!proc.WaitForExit(QUERY_TIMEOUT_MS))
            {
                proc.Kill();
                return false;
            }

            // the reply is "(true,)" or "(false,)"
            return proc.ExitCode == 0 && output.Result.Contains("true", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Builds a percent-encoded <c>file://</c> URI for <paramref name="path"/>, mapped to its host
    /// location first because the backends run outside the sandbox.
    /// </summary>
    private static string ToFileUri(string path)
    {
        return new Uri(BHelper.GetRealPlatformPath(Path.GetFullPath(path))).AbsoluteUri;
    }


    /// <summary>
    /// Like <see cref="ToFileUri"/>, but also percent-encodes the apostrophe so a name
    /// containing one cannot break out of a single-quoted GVariant string literal.
    /// </summary>
    private static string ToGVariantUri(string path)
    {
        return ToFileUri(path).Replace("'", "%27");
    }


    /// <summary>
    /// Invokes a D-Bus method via <c>gdbus call</c>. Arguments are passed through
    /// <see cref="ProcessStartInfo.ArgumentList"/> so no manual shell quoting is
    /// needed. Fire-and-forget at the process level; gdbus itself completes the
    /// (synchronous) bus call before exiting. Failures are swallowed.
    /// </summary>
    private static void Call(string dest, string objectPath, string method, params string[] methodArgs)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = "gdbus";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            var args = proc.StartInfo.ArgumentList;
            args.Add("call");
            args.Add("--session");
            args.Add("--dest"); args.Add(dest);
            args.Add("--object-path"); args.Add(objectPath);
            args.Add("--method"); args.Add(method);
            foreach (var arg in methodArgs) args.Add(arg);

            proc.Start();
        }
        catch
        {
            // best-effort: gdbus missing or no session bus
        }
    }
}
