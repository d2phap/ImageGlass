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
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders.Update;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;

public partial class LinuxUpdateProvider
{
    private const int BUFFER_SIZE = 128 * 1024;
    private const int F_SETFD = 2;
    private const int FD_CLOEXEC = 1;

    private static readonly Lazy<bool> _canReplaceAppImage = new(ProbeAppImageFolder);


    [LibraryImport("libc.so.6", EntryPoint = "fcntl", SetLastError = true)]
    private static partial int Fcntl(int fd, int cmd, int arg);


    /// <summary>
    /// Whether this user may swap files in the folder holding the running .AppImage.
    /// </summary>
    private static bool IsAppImageReplaceable => _canReplaceAppImage.Value;


    /// <summary>
    /// Whether this session runs extracted (<c>--appimage-extract-and-run</c>) rather than FUSE-mounted.
    /// </summary>
    private static bool IsExtractAndRun => Path.GetFileName(
        Environment.GetEnvironmentVariable("APPDIR")?.TrimEnd('/') ?? string.Empty)
        .StartsWith("appimage_extracted_", StringComparison.Ordinal);



    #region AppImage

    /// <summary>
    /// Swaps the verified image in by rename, relaunches it, and restores the old one if it cannot start.
    /// </summary>
    private async Task<UpdateOpResult> ApplyAppImageAsync(string packageFilePath, UpdateReleaseInfo release,
        CancellationToken ct)
    {
        var target = BHelper.AppRelaunchPath;
        var staged = GetSiblingPath(target, "new");
        var backup = GetSiblingPath(target, "old");

        // 1. stage beside the image: only a rename within one filesystem is atomic
        var stage = await StageAppImageAsync(packageFilePath, staged, target,
            ResolveArtifact(release)?.Sha256, ct).ConfigureAwait(false);
        if (!stage.IsSuccess) return stage;

        // 2. rename, never write in place: this session still reads its mount through the old inode
        File.Move(target, backup, true);
        try
        {
            File.Move(staged, target);
        }
        catch
        {
            File.Move(backup, target);
            TryDeleteFile(staged);
            throw;
        }

        UpdateTrace.Mark($"apply:swapped {target}");
        await ForgetInstalledUpdateAsync().ConfigureAwait(false);

        // 3. a build this host cannot run (a newer glibc floor, say) must not replace a working one
        SetInheritedFdsCloseOnExec();
        var relaunch = await RelaunchAsync(CreateAppImageStartInfo(target)).ConfigureAwait(false);
        if (relaunch.IsSuccess)
        {
            TryDeleteFile(backup);
            return relaunch;
        }

        return RollBackAppImage(target, backup, release, relaunch);
    }


    /// <summary>
    /// Copies the package beside the image, hashing it on the way, with the image's own permissions.
    /// </summary>
    private async Task<UpdateOpResult> StageAppImageAsync(string source, string staged, string target,
        string? expectedSha256, CancellationToken ct)
    {
        string actual;
        try
        {
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[BUFFER_SIZE];

            await using (var src = new FileStream(source, FileMode.Open, FileAccess.Read,
                FileShare.Read, BUFFER_SIZE, useAsync: true))
            await using (var dest = new FileStream(staged, FileMode.Create, FileAccess.Write,
                FileShare.None, BUFFER_SIZE, useAsync: true))
            {
                int read;
                while ((read = await src.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
                {
                    hasher.AppendData(buffer, 0, read);
                    await dest.WriteAsync(buffer.AsMemory(0, read), ct).ConfigureAwait(false);
                }

                // on disk before the rename, or a crash could leave the image's name on a torn file
                dest.Flush(true);
            }

            if (OperatingSystem.IsLinux())
            {
                // keep the owner's permission bits, but the image must stay runnable
                File.SetUnixFileMode(staged, File.GetUnixFileMode(target)
                    | UnixFileMode.UserRead | UnixFileMode.UserExecute);
            }

            actual = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            TryDeleteFile(staged);
            return UpdateOpResult.Fail($"IGE: The update could not be written next to the AppImage: {ex.Message}",
                $"AppImage: {target}{Environment.NewLine}{BHelper.GetExceptionDetails(ex)}");
        }

        // last check before the swap: the cached copy is re-read here, so it is what gets verified
        if (!string.IsNullOrWhiteSpace(expectedSha256)
            && !string.Equals(actual, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            TryDeleteFile(staged);
            UpdateTrace.Mark($"apply:hashMismatch expected={expectedSha256} actual={actual}");

            // the cached package can never pass, so let the next check download it again
            await ForgetInstalledUpdateAsync().ConfigureAwait(false);

            return UpdateOpResult.Fail("IGE: The downloaded update failed its integrity check and was discarded.",
                $"Package: {source}{Environment.NewLine}"
                + $"Expected SHA-256: {expectedSha256}{Environment.NewLine}"
                + $"Actual SHA-256:   {actual}");
        }

        UpdateTrace.Mark($"apply:staged {staged}");
        return UpdateOpResult.Ok();
    }


    /// <summary>
    /// Puts the previous image back after the new one failed to start.
    /// </summary>
    private static UpdateOpResult RollBackAppImage(string target, string backup, UpdateReleaseInfo release,
        UpdateOpResult relaunch)
    {
        var heading = $"IGE: ImageGlass {release.Version} could not start on this system";
        var details = $"AppImage: {target}{Environment.NewLine}"
            + $"{relaunch.ErrorMessage}{Environment.NewLine}{relaunch.ErrorDetails}";

        try
        {
            File.Move(backup, target, true);
            UpdateTrace.Mark("apply:rolledBack");

            return UpdateOpResult.Fail($"{heading}, so the previous version was restored.", details);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:rollbackFailed {ex.Message}");

            return UpdateOpResult.Fail($"{heading}, and the previous version could not be restored.",
                $"{details}{Environment.NewLine}Previous version: {backup}{Environment.NewLine}"
                + BHelper.GetExceptionDetails(ex));
        }
    }


    /// <summary>
    /// Starts the image the way this session was started, or a FUSE-less host fails a healthy build.
    /// </summary>
    private static ProcessStartInfo CreateAppImageStartInfo(string target)
    {
        var psi = new ProcessStartInfo(target) { UseShellExecute = false };
        if (IsExtractAndRun) psi.Environment["APPIMAGE_EXTRACT_AND_RUN"] = "1";

        return psi;
    }


    /// <summary>
    /// Keeps fds past stdio out of children: the runtime's inheritable mount fds would pin this mount.
    /// </summary>
    private static void SetInheritedFdsCloseOnExec()
    {
        try
        {
            foreach (var entry in Directory.GetFileSystemEntries("/proc/self/fd"))
            {
                if (int.TryParse(Path.GetFileName(entry), out var fd) && fd > 2)
                {
                    _ = Fcntl(fd, F_SETFD, FD_CLOEXEC);
                }
            }
        }
        catch (Exception ex)
        {
            // the new image still starts; it only keeps the old mount until it exits
            UpdateTrace.Mark($"apply:cloexecFailed {ex.Message}");
        }
    }


    /// <summary>
    /// Creates, then deletes, a file beside the .AppImage, which is the exact right the swap needs.
    /// </summary>
    private static bool ProbeAppImageFolder()
    {
        try
        {
            var target = BHelper.AppRelaunchPath;
            if (!File.Exists(target)) return false;

            using (new FileStream(GetSiblingPath(target, "probe"), FileMode.Create, FileAccess.Write,
                FileShare.None, 1, FileOptions.DeleteOnClose)) { }

            return true;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"detect:appImageNotWritable {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// A hidden, per-process file name beside <paramref name="target"/>, so on the same filesystem.
    /// </summary>
    private static string GetSiblingPath(string target, string suffix)
    {
        return Path.Combine(Path.GetDirectoryName(target) ?? string.Empty,
            $".{Path.GetFileName(target)}.{Environment.ProcessId}.{suffix}");
    }

    #endregion // AppImage

}
