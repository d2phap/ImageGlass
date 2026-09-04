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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Stages every save into a sibling temp file and renames it over the destination, so a write that
/// is cancelled, fails, or dies with the process leaves the original file intact instead of 0 KB.
/// </summary>
internal static class SaveStaging
{
    // Marks the host's own staging file, so leftovers can be swept and the file watcher can skip them.
    private const string TEMP_PREFIX = ".ig-save-";

    // A younger leftover may belong to a save running right now, here or in another instance.
    private static readonly TimeSpan STALE_AGE = TimeSpan.FromMinutes(10);

    // An on-access scanner or the search indexer holds a brand-new file for a few ms.
    private const int PROMOTE_RETRIES = 4;
    private const int PROMOTE_RETRY_DELAY_MS = 60;


    /// <summary>
    /// Checks whether the path is one of our in-progress staging files.
    /// </summary>
    public static bool IsStagingFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        return Path.GetFileName(filePath.AsSpan()).StartsWith(TEMP_PREFIX, StringComparison.Ordinal);
    }


    /// <summary>
    /// Builds the staging path: same folder so the rename is cheap and atomic, and the real
    /// extension stays last because encoders read it to pick a container.
    /// </summary>
    public static string BuildTempPath(string destFilePath)
    {
        var dir = Path.GetDirectoryName(destFilePath);
        var ext = Path.GetExtension(destFilePath);
        var name = TEMP_PREFIX + Guid.NewGuid().ToString("N") + ext;

        return string.IsNullOrEmpty(dir) ? name : Path.Combine(dir, name);
    }


    /// <summary>
    /// Runs <paramref name="writeAsync"/> against a staging path, then promotes the result onto
    /// <paramref name="destFilePath"/>. The destination is never touched unless a complete file exists.
    /// </summary>
    public static async Task WriteThenPromoteAsync(string destFilePath,
        Func<string, Task> writeAsync, CancellationToken token = default)
    {
        var tempPath = BuildTempPath(destFilePath);
        SweepStaleTempFiles(destFilePath);

        try
        {
            await writeAsync(tempPath).ConfigureAwait(false);

            // a cancelled writer returns without writing; leave the destination alone and stay quiet
            token.ThrowIfCancellationRequested();

            if (!HasContent(tempPath))
            {
                throw new IOException($"IGE: nothing was written for '{destFilePath}'.");
            }
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }

        // a failed promote deliberately keeps the staged file: it holds the finished image
        var (promoted, promoteError) = await PromoteAsync(tempPath, destFilePath, token).ConfigureAwait(false);
        if (!promoted)
        {
            throw new IOException($"IGE: could not finalize '{destFilePath}'. {promoteError}".TrimEnd());
        }
    }


    /// <summary>
    /// Puts the staged file in place. Walks three strategies because no single one works against
    /// every kind of holder on the destination; reports failure instead of throwing, so the caller
    /// can keep the original file.
    /// </summary>
    public static async Task<(bool Ok, string Error)> PromoteAsync(string tempPath, string destFilePath,
        CancellationToken token = default)
    {
        var errors = new List<string>(3);

        // 1. Atomic replace, retried: an on-access scanner or the search indexer holds a brand-new
        //    file for a few ms. The destination is the whole old file or the whole new one.
        for (var attempt = 0; attempt <= PROMOTE_RETRIES; attempt++)
        {
            try
            {
                await Task.Run(() => File.Move(tempPath, destFilePath, overwrite: true), token).ConfigureAwait(false);
                return (true, string.Empty);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                if (attempt == PROMOTE_RETRIES) errors.Add($"replace: {Describe(ex)}");
                else await Task.Delay(PROMOTE_RETRY_DELAY_MS, token).ConfigureAwait(false);
            }
        }

        // 2. Unlink then rename. A memory-mapped destination (a shell/WIC thumbnail handler, or any
        //    decoder that maps the file) refuses both the replace and every truncating open, yet
        //    allows the unlink. The name is absent for an instant, but never holds partial content.
        try
        {
            await Task.Run(() =>
            {
                File.Delete(destFilePath);
                File.Move(tempPath, destFilePath);
            }, token).ConfigureAwait(false);

            return (true, string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            errors.Add($"unlink+rename: {Describe(ex)}");
        }

        // 3. Overwrite in place, which is what the app did before staging existed. Reaches the case
        //    a handle sharing write but not delete allows, where step 2 cannot unlink.
        try
        {
            await Task.Run(() => File.Copy(tempPath, destFilePath, overwrite: true), token).ConfigureAwait(false);
            TryDelete(tempPath);

            return (true, string.Empty);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            errors.Add($"overwrite: {Describe(ex)}");
        }

        // Nothing landed. Name the staged file: it holds the complete image the user just saved.
        errors.Add($"the finished image is at '{tempPath}'");

        return (false, string.Join("; ", errors));
    }


    /// <summary>
    /// Exception text plus the Win32 code, since .NET reports a mapped-section refusal as a bare
    /// "Access to the path is denied" that names neither the real cause nor the path.
    /// </summary>
    private static string Describe(Exception ex)
    {
        var code = Marshal.GetHRForException(ex) & 0xFFFF;

        return $"{ex.GetType().Name} (Win32 {code}): {ex.Message}";
    }


    /// <summary>
    /// Best-effort delete; a still-locked staging file is left for a later sweep.
    /// </summary>
    public static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (Exception ex) { Debug.WriteLine($"[SaveStaging] temp delete failed: {ex.Message}"); }
    }


    /// <summary>
    /// Removes staging files a previous save could not clean up (killed process, locked handle).
    /// </summary>
    public static void SweepStaleTempFiles(string destFilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(destFilePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

            var cutoff = DateTime.UtcNow - STALE_AGE;
            foreach (var stale in Directory.EnumerateFiles(dir, TEMP_PREFIX + "*"))
            {
                // never delete a staging file a concurrent save may still be writing
                try { if (File.GetLastWriteTimeUtc(stale) < cutoff) File.Delete(stale); }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SaveStaging] temp sweep failed: {ex.Message}");
        }
    }


    /// <summary>
    /// A staged file counts only when it exists and is not empty.
    /// </summary>
    private static bool HasContent(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            return fi.Exists && fi.Length > 0;
        }
        catch { return false; }
    }
}
