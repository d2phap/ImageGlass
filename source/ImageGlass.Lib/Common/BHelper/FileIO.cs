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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common;

public partial class BHelper
{
    // one gate per target path: two windows of this process can reach the same file at once
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _sharedFileGates = new(StringComparer.OrdinalIgnoreCase);

    // a staged write only holds the target for one rename, so retries stay short
    private const int SHARED_FILE_MAX_ATTEMPTS = 12;
    private const int SHARED_FILE_RETRY_DELAY_MS = 60;


    /// <summary>
    /// Opens a file for reading, waiting out another process replacing it at that moment.
    /// </summary>
    public static FileStream OpenReadShared(string filePath)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                // Delete lets a writer swap the file in under this handle; withholding Write is what
                // keeps that swap a rename, as a shared writer degrades it into an in-place copy
                return new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.Read | FileShare.Delete);
            }
            // the last attempt lets the real exception through
            catch (Exception ex) when (IsTransientFileError__(ex) && attempt < SHARED_FILE_MAX_ATTEMPTS)
            {
                Debug.WriteLine($"[BHelper] Read attempt {attempt} on '{filePath}': {ex.Message}");
                Thread.Sleep(SHARED_FILE_RETRY_DELAY_MS);
            }
        }
    }


    /// <summary>
    /// Writes text to a file other threads and app instances may write at the same moment.
    /// </summary>
    public static async Task WriteAllTextSharedAsync(string filePath, string content,
        CancellationToken token = default)
    {
        var gate = _sharedFileGates.GetOrAdd(filePath, static _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await Task.Run(() => WriteAllTextShared__(filePath, content, token), token)
                .ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }


    /// <summary>
    /// Stages the content next to the target and replaces it.
    /// </summary>
    private static void WriteAllTextShared__(string filePath, string content, CancellationToken token)
    {
        if (TryWriteViaTempFile__(filePath, content, token)) return;

        // a filter driver or a redirected store can refuse the rename, so write the target directly
        WriteAllTextDirect__(filePath, content, token);
    }


    /// <summary>
    /// Writes the content to a per-process temp file, then moves it onto the target.
    /// </summary>
    /// <returns><c>false</c> if the target was not replaced, so the caller must fall back.</returns>
    private static bool TryWriteViaTempFile__(string filePath, string content, CancellationToken token)
    {
        // the pid keeps two instances from staging into the same file
        var tempPath = $"{filePath}.{Environment.ProcessId}.tmp";

        try
        {
            File.WriteAllText(tempPath, content, Encoding.UTF8);

            for (var attempt = 1; attempt <= SHARED_FILE_MAX_ATTEMPTS; attempt++)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    // a plain rename, so the target is never half-written nor briefly absent
                    File.Move(tempPath, filePath, true);
                    return true;
                }
                catch (Exception ex) when (IsTransientFileError__(ex))
                {
                    Debug.WriteLine($"[BHelper] Replace attempt {attempt} on '{filePath}': {ex.Message}");
                    Thread.Sleep(SHARED_FILE_RETRY_DELAY_MS);
                }
            }
        }
        catch (Exception ex) when (IsTransientFileError__(ex))
        {
            Debug.WriteLine($"[BHelper] Could not stage '{tempPath}': {ex.Message}");
        }
        finally
        {
            // clears an abandoned staging file
            try { File.Delete(tempPath); } catch { }
        }

        return false;
    }


    /// <summary>
    /// Writes straight to the target, retrying while another process holds it.
    /// </summary>
    private static void WriteAllTextDirect__(string filePath, string content, CancellationToken token)
    {
        for (var attempt = 1; ; attempt++)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
                return;
            }
            // the last attempt lets the real exception through
            catch (Exception ex) when (IsTransientFileError__(ex) && attempt < SHARED_FILE_MAX_ATTEMPTS)
            {
                Debug.WriteLine($"[BHelper] Write attempt {attempt} on '{filePath}': {ex.Message}");
                Thread.Sleep(SHARED_FILE_RETRY_DELAY_MS);
            }
        }
    }


    /// <summary>
    /// Whether a later attempt can still get past the error, i.e. the file is merely held.
    /// </summary>
    private static bool IsTransientFileError__(Exception ex) => ex switch
    {
        DirectoryNotFoundException or FileNotFoundException or PathTooLongException => false,
        IOException or UnauthorizedAccessException => true,
        _ => false,
    };
}
