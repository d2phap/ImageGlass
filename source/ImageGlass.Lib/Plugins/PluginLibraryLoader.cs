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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Plugins;


/// <summary>
/// Maps native plugin libraries into the process and keeps them mapped for its whole lifetime.
/// </summary>
/// <remarks>
/// A plugin library embeds its own runtime (NativeAOT, or a C runtime), which installs
/// process-global state on load: signal handlers / a vectored exception handler, TLS destructors
/// and atexit records. Unloading the library unmaps that code while the OS still points at it, so
/// the next fault the host handles itself - a plain <see cref="NullReferenceException"/> is one -
/// jumps into freed memory and kills the process with no managed exception. Hence: load once,
/// never free. Disabling a plugin stops calling into it; it does not unmap it.
/// </remarks>
internal static class PluginLibraryLoader
{
    /// <summary>
    /// Subfolder under <c>_plugins</c> holding the private copies described in <see cref="Load"/>.
    /// </summary>
    internal const string SHADOW_DIR_NAME = "_shadow";

    private static readonly Lock _lock = new();

    // library path -> the handle it is mapped under + the SHA-256 of the bytes that were mapped
    private static readonly Dictionary<string, (nint Handle, string Hash)> _mapped =
        new(StringComparer.Ordinal);


    /// <summary>
    /// Maps the plugin library and returns its handle, or <c>0</c> with the reason in
    /// <paramref name="error"/>. <paramref name="hash"/> is the SHA-256 of
    /// <paramref name="libraryPath"/> as the caller verified it against the trust pin.
    /// </summary>
    /// <remarks>
    /// A path already mapped with the same bytes is reused. Mapped with different bytes means the
    /// plugin was replaced in this session: the loader would hand back the copy it already holds
    /// (glibc resolves by name first, and the file is still mapped because we never free), so the
    /// update would silently not take effect. Such a load goes through a private copy instead.
    /// </remarks>
    public static nint Load(string libraryPath, string hash, string pluginDir, out string? error)
    {
        error = null;

        lock (_lock)
        {
            var isMapped = _mapped.TryGetValue(libraryPath, out var existing);
            if (isMapped && string.Equals(existing.Hash, hash, StringComparison.OrdinalIgnoreCase))
            {
                return existing.Handle;
            }

            try
            {
                var loadPath = isMapped ? CreateShadowCopy(pluginDir, libraryPath, hash) : libraryPath;
                var handle = NativeLibrary.Load(loadPath);

                _mapped[libraryPath] = (handle, hash);
                return handle;
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}";
                return 0;
            }
        }
    }


    /// <summary>
    /// Copies the whole plugin folder into a fresh <c>_shadow</c> subfolder and returns the library
    /// path inside it. The folder, not just the library, so a sibling data file still resolves.
    /// </summary>
    private static string CreateShadowCopy(string pluginDir, string libraryPath, string hash)
    {
        var shadowRoot = Path.Combine(BHelper.ConfigDir(Dir.Plugins), SHADOW_DIR_NAME);
        var destDir = Path.Combine(shadowRoot, Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(destDir);
        foreach (var dir in Directory.GetDirectories(pluginDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(destDir, Path.GetRelativePath(pluginDir, dir)));
        }
        foreach (var file in Directory.GetFiles(pluginDir, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetRelativePath(pluginDir, file)), overwrite: true);
        }

        // the copy, not the original, is what runs, so it is what the trust pin has to cover
        var copiedLibrary = Path.Combine(destDir, Path.GetRelativePath(pluginDir, libraryPath));
        var copiedHash = PluginTrustPolicy.ComputeSha256(copiedLibrary);
        if (!string.Equals(copiedHash, hash, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("the plugin library changed while it was being copied.");
        }

        return copiedLibrary;
    }
}
