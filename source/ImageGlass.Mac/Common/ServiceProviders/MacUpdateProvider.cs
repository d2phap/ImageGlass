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
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Update;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Mac.Common.ServiceProviders;


/// <summary>
/// Update provider for macOS; replaces the running .app bundle with the one in the release DMG.
/// </summary>
public partial class MacUpdateProvider : UpdateProvider
{
    private const string HDIUTIL = "/usr/bin/hdiutil";
    private const string DITTO = "/usr/bin/ditto";
    private const int W_OK = 2;

    private static readonly TimeSpan HDIUTIL_TIMEOUT = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan COPY_TIMEOUT = TimeSpan.FromMinutes(5);

    private static readonly Lazy<AppBundle?> _bundle = new(DetectReplaceableBundle);


    /// <summary>
    /// The running bundle, and its executable relative to it.
    /// </summary>
    private sealed record AppBundle(string Dir, string Executable);


    [LibraryImport("/usr/lib/libSystem.B.dylib", EntryPoint = "access", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Access(string path, int mode);



    #region Platform surface

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsInstallSupported => _bundle.Value is not null;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ArtifactKey => $"mac-{BHelper.ArchToken}-dmg";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async Task<UpdateOpResult> ApplyAndRestartAsync(string packageFilePath,
        UpdateReleaseInfo release, CancellationToken ct = default)
    {
        if (_bundle.Value is not { } bundle)
        {
            return UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates.");
        }

        if (!File.Exists(packageFilePath))
        {
            return UpdateOpResult.Fail($"IGE: The downloaded update package is missing: {packageFilePath}");
        }

        return await RunInstallThenExitAsync(release.Version,
            () => ApplyDmgAsync(bundle, packageFilePath, release, ct), ct).ConfigureAwait(false);
    }

    #endregion // Platform surface



    #region Private Methods

    /// <summary>
    /// Swaps the app from the image in by rename, relaunches it, and restores the old one if it cannot start.
    /// </summary>
    private async Task<UpdateOpResult> ApplyDmgAsync(AppBundle bundle, string dmgPath, UpdateReleaseInfo release,
        CancellationToken ct)
    {
        // 1. the cached image is re-read here, so it is what gets verified
        var hash = await VerifyImageAsync(dmgPath, ResolveArtifact(release)?.Sha256, ct).ConfigureAwait(false);
        if (!hash.IsSuccess) return hash;

        // 2. stage beside the bundle: only a rename within one volume is atomic
        var staged = GetSiblingPath(bundle.Dir, "new");
        var backup = GetSiblingPath(bundle.Dir, "old");

        var stage = await StageBundleAsync(dmgPath, staged, bundle, ct).ConfigureAwait(false);
        if (!stage.IsSuccess) return stage;

        // 3. rename, never copy over: this session keeps reading its files through the renamed bundle
        try
        {
            Directory.Move(bundle.Dir, backup);
            try
            {
                Directory.Move(staged, bundle.Dir);
            }
            catch
            {
                Directory.Move(backup, bundle.Dir);
                throw;
            }
        }
        catch (Exception ex)
        {
            TryDeleteDirectory(staged);
            return UpdateOpResult.Fail($"IGE: The update could not replace the app: {ex.Message}",
                $"App: {bundle.Dir}{Environment.NewLine}{BHelper.GetExceptionDetails(ex)}");
        }

        UpdateTrace.Mark($"apply:swapped {bundle.Dir}");
        await ForgetInstalledUpdateAsync().ConfigureAwait(false);

        // 4. a build this Mac cannot run (a newer macOS floor, say) must not replace a working one
        var relaunch = await RelaunchAsync(new ProcessStartInfo(Path.Combine(bundle.Dir, bundle.Executable))
        {
            UseShellExecute = false,
        }).ConfigureAwait(false);

        if (relaunch.IsSuccess)
        {
            TryDeleteDirectory(backup);
            return relaunch;
        }

        return RollBack(bundle.Dir, backup, release, relaunch);
    }


    /// <summary>
    /// Re-hashes the cached image; one that no longer matches is discarded, so the next check fetches it again.
    /// </summary>
    private async Task<UpdateOpResult> VerifyImageAsync(string dmgPath, string? expectedSha256, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256)) return UpdateOpResult.Ok();

        string actual;
        await using (var stream = File.OpenRead(dmgPath))
        {
            var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
            actual = Convert.ToHexString(hash).ToLowerInvariant();
        }

        if (string.Equals(actual, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return UpdateOpResult.Ok();
        }

        UpdateTrace.Mark($"apply:hashMismatch expected={expectedSha256} actual={actual}");
        await ForgetInstalledUpdateAsync().ConfigureAwait(false);

        return UpdateOpResult.Fail("IGE: The downloaded update failed its integrity check and was discarded.",
            $"Package: {dmgPath}{Environment.NewLine}"
            + $"Expected SHA-256: {expectedSha256}{Environment.NewLine}"
            + $"Actual SHA-256:   {actual}");
    }


    /// <summary>
    /// Copies the app out of the image to <paramref name="staged"/>, then checks it is signed as this app is.
    /// </summary>
    private async Task<UpdateOpResult> StageBundleAsync(string dmgPath, string staged, AppBundle bundle,
        CancellationToken ct)
    {
        TryDeleteDirectory(staged);

        // a private mount point, hidden from Finder; hdiutil creates it and removes it on detach
        var mountPoint = Path.Combine(Path.GetTempPath(), $"{BHelper.AppName}-update-{Environment.ProcessId}");
        var attach = await RunCommandAsync(CreateStartInfo(HDIUTIL, "attach", "-nobrowse", "-noautoopen",
            "-noverify", "-readonly", "-mountpoint", mountPoint, dmgPath), HDIUTIL_TIMEOUT).ConfigureAwait(false);

        if (attach.ExitCode != 0)
        {
            return UpdateOpResult.Fail(
                $"IGE: The update image could not be opened: {GetLastLine(attach.StdErr, attach.StdOut)}",
                FormatCommandDetails(dmgPath, attach));
        }

        UpdateTrace.Mark($"apply:mounted {mountPoint}");
        try
        {
            ct.ThrowIfCancellationRequested();

            if (FindAppBundle(mountPoint) is not { } source)
            {
                return UpdateOpResult.Fail("IGE: The update image does not contain an app.", $"Image: {dmgPath}");
            }

            // ditto keeps the symlinks, permissions and extended attributes a signed bundle relies on
            var copy = await RunCommandAsync(CreateStartInfo(DITTO, source, staged), COPY_TIMEOUT)
                .ConfigureAwait(false);

            if (copy.ExitCode != 0)
            {
                TryDeleteDirectory(staged);
                return UpdateOpResult.Fail(
                    $"IGE: The update could not be copied next to the app: {GetLastLine(copy.StdErr, copy.StdOut)}",
                    FormatCommandDetails(dmgPath, copy));
            }
        }
        finally
        {
            await DetachAsync(mountPoint).ConfigureAwait(false);
        }

        // the copy, not the image, is what gets installed, so it is what must pass
        var verify = await VerifyStagedBundleAsync(staged, bundle).ConfigureAwait(false);
        if (!verify.IsSuccess)
        {
            TryDeleteDirectory(staged);
            return verify;
        }

        UpdateTrace.Mark($"apply:staged {staged}");
        return UpdateOpResult.Ok();
    }


    /// <summary>
    /// Requires the executable this session relaunches, and a signature from this app's own team.
    /// </summary>
    private async Task<UpdateOpResult> VerifyStagedBundleAsync(string staged, AppBundle bundle)
    {
        string? teamId = null;
        var error = File.Exists(Path.Combine(staged, bundle.Executable))
            ? MacCodeSignApi.CheckSignedLikeThisApp(staged, out teamId)
            : $"The app has no {bundle.Executable}.";

        if (error is null)
        {
            // a team-less (ad-hoc) dev build can only demand a valid signature
            UpdateTrace.Mark($"apply:signatureOk team={teamId ?? "unpinned"}");
            return UpdateOpResult.Ok();
        }

        UpdateTrace.Mark($"apply:signatureRejected {error}");

        // the same package can never pass, so stop offering it
        await ForgetInstalledUpdateAsync().ConfigureAwait(false);

        return UpdateOpResult.Fail("IGE: The update failed its code signature check, so it was not installed.",
            $"App: {staged}{Environment.NewLine}{error}");
    }


    /// <summary>
    /// Puts the previous app back after the new one failed to start.
    /// </summary>
    private static UpdateOpResult RollBack(string bundleDir, string backup, UpdateReleaseInfo release,
        UpdateOpResult relaunch)
    {
        var heading = $"IGE: ImageGlass {release.Version} could not start on this Mac";
        var details = $"App: {bundleDir}{Environment.NewLine}"
            + $"{relaunch.ErrorMessage}{Environment.NewLine}{relaunch.ErrorDetails}";

        try
        {
            // a directory cannot be renamed over another, so the failed build steps aside first
            var failed = GetSiblingPath(bundleDir, "failed");
            Directory.Move(bundleDir, failed);
            Directory.Move(backup, bundleDir);
            TryDeleteDirectory(failed);

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
    /// Ejects the image; a volume kept busy (by Spotlight, say) is forced out.
    /// </summary>
    private static async Task DetachAsync(string mountPoint)
    {
        var detach = await RunCommandAsync(CreateStartInfo(HDIUTIL, "detach", mountPoint), HDIUTIL_TIMEOUT)
            .ConfigureAwait(false);

        if (detach.ExitCode != 0)
        {
            detach = await RunCommandAsync(CreateStartInfo(HDIUTIL, "detach", "-force", mountPoint),
                HDIUTIL_TIMEOUT).ConfigureAwait(false);
        }

        if (detach.ExitCode != 0)
        {
            UpdateTrace.Mark($"apply:detachFailed {GetLastLine(detach.StdErr, detach.StdOut)}");
        }
    }


    /// <summary>
    /// The app at the root of the mounted image; its Applications shortcut is a symlink, never a bundle.
    /// </summary>
    private static string? FindAppBundle(string mountPoint)
    {
        return new DirectoryInfo(mountPoint)
            .EnumerateDirectories("*.app")
            .FirstOrDefault(d => d.LinkTarget is null)?
            .FullName;
    }


    /// <summary>
    /// The running bundle when this user may replace it, otherwise <c>null</c>.
    /// </summary>
    private static AppBundle? DetectReplaceableBundle()
    {
        try
        {
            // <name>.app/Contents/MacOS/<exe>; a dev build runs loose from bin/
            var exe = BHelper.AppExePath;
            var macOsDir = Path.GetDirectoryName(exe);
            var contentsDir = Path.GetDirectoryName(macOsDir);
            var dir = Path.GetDirectoryName(contentsDir);

            if (dir is null
                || !string.Equals(Path.GetFileName(macOsDir), "MacOS", StringComparison.Ordinal)
                || !string.Equals(Path.GetFileName(contentsDir), "Contents", StringComparison.Ordinal)
                || !dir.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                UpdateTrace.Mark($"detect:notAppBundle {exe}");
                return null;
            }

            // Gatekeeper runs a quarantined app from a read-only copy, which cannot be replaced
            if (dir.Contains("/AppTranslocation/", StringComparison.Ordinal))
            {
                UpdateTrace.Mark($"detect:translocated {dir}");
                return null;
            }

            // the swap renames the bundle inside its folder, then deletes the old copy
            if (Access(Path.GetDirectoryName(dir) ?? string.Empty, W_OK) != 0 || Access(dir, W_OK) != 0)
            {
                UpdateTrace.Mark($"detect:bundleNotWritable {dir}");
                return null;
            }

            return new AppBundle(dir, Path.GetRelativePath(dir, exe));
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"detect:failed {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// A hidden, per-process path beside <paramref name="bundleDir"/>, so on the same volume.
    /// </summary>
    private static string GetSiblingPath(string bundleDir, string suffix)
    {
        return Path.Combine(Path.GetDirectoryName(bundleDir) ?? string.Empty,
            $".{Path.GetFileName(bundleDir)}.{Environment.ProcessId}.{suffix}");
    }


    private static ProcessStartInfo CreateStartInfo(string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in args) psi.ArgumentList.Add(arg);
        return psi;
    }


    /// <summary>
    /// Diagnostic detail of an image step, for the error dialog's expander.
    /// </summary>
    private static string FormatCommandDetails(string dmgPath, (int ExitCode, string StdOut, string StdErr) step)
    {
        var nl = Environment.NewLine;

        return $"Image: {dmgPath}{nl}"
            + $"Exit code: {step.ExitCode}{nl}"
            + $"{step.StdOut.Trim()}{nl}{step.StdErr.Trim()}";
    }


    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:deleteFailed {path} {ex.Message}");
        }
    }

    #endregion // Private Methods

}
