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
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageGlass.Linux.Common;

/// <summary>
/// Moves files to the user's trash following the FreeDesktop.org Trash
/// specification (<see href="https://specifications.freedesktop.org/trash-spec/"/>).
/// </summary>
/// <remarks>
/// The Trash portal is unreliable on some desktops (it returns "failed" on
/// xdg-desktop-portal 1.18.x), and <c>gio trash</c> inside a Flatpak either
/// delegates to that portal or targets the sandbox's own trash. So we write the
/// home trash directly. Inside Flatpak, <c>$HOME</c> is the user's real home and
/// <c>--filesystem=host</c> grants access, so this reaches the real trash; the
/// sandboxed <c>$XDG_DATA_HOME</c> is deliberately ignored.
/// </remarks>
internal static class FreeDesktopTrash
{
    /// <summary>
    /// Moves <paramref name="filePath"/> to the home trash. Returns
    /// <see langword="true"/> on success.
    /// </summary>
    public static bool Trash(string filePath)
    {
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath) && !Directory.Exists(fullPath)) return false;

            // Home trash = $HOME/.local/share/Trash (spec default). Use $HOME, not
            // $XDG_DATA_HOME, which Flatpak redirects into the sandbox.
            var home = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(home)) return false;

            var filesDir = Path.Combine(home, ".local", "share", "Trash", "files");
            var infoDir = Path.Combine(home, ".local", "share", "Trash", "info");
            Directory.CreateDirectory(filesDir);
            Directory.CreateDirectory(infoDir);

            var name = Path.GetFileName(fullPath);
            var stem = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);

            // Claim a unique name by creating its .trashinfo exclusively (the spec
            // requires the info file to be created atomically to avoid races).
            string trashName;
            for (var i = 1; ; i++)
            {
                trashName = i == 1 ? name : $"{stem}.{i}{ext}";
                var infoPath = Path.Combine(infoDir, trashName + ".trashinfo");
                try
                {
                    using var fs = new FileStream(infoPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    var info = "[Trash Info]\n"
                        + $"Path={EncodePath(fullPath)}\n"
                        + $"DeletionDate={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}\n";
                    var bytes = Encoding.UTF8.GetBytes(info);
                    fs.Write(bytes, 0, bytes.Length);
                    break;
                }
                catch (IOException) when (File.Exists(infoPath))
                {
                    if (i > 10000) return false;   // give up on pathological collisions
                }
            }

            var dest = Path.Combine(filesDir, trashName);
            try
            {
                File.Move(fullPath, dest);
            }
            catch (IOException)
            {
                // Different filesystem than the trash: fall back to copy + delete
                // so the file still ends up in the trash (restorable via Path=).
                File.Copy(fullPath, dest, overwrite: false);
                File.Delete(fullPath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Percent-encodes a path for the <c>Path=</c> key of a .trashinfo file,
    /// preserving the <c>/</c> separators per the spec.
    /// </summary>
    private static string EncodePath(string path)
        => string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
}
