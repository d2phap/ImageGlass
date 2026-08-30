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
using ImageGlass.Common.Loggers;
using ImageGlass.Common.Types;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Downloads an update artifact and verifies it against the manifest digest.
/// </summary>
internal static class AppUpdateDownloader
{
    private const int BUFFER_SIZE = 128 * 1024;


    /// <summary>
    /// Full path the artifact for <paramref name="version"/> is cached at.
    /// </summary>
    /// <remarks>
    /// Both parts come from the remote manifest, so unsanitized they are an arbitrary-file-write primitive.
    /// </remarks>
    public static string GetPackagePath(string version, string url)
    {
        var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
        var fileName = $"{BHelper.AppName}_{Sanitize(version, 40)}{SanitizeExtension(url)}";
        var fullPath = Path.GetFullPath(Path.Combine(dir, fileName));

        // last line of defence: the result must still be inside the cache folder
        var root = Path.GetFullPath(dir) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("IGE: Update package path escaped the cache folder.");

        return fullPath;
    }


    /// <summary>
    /// Keeps only characters that are safe in a file name.
    /// </summary>
    private static string Sanitize(string value, int maxLength)
    {
        var sb = new StringBuilder(Math.Min(value.Length, maxLength));

        foreach (var c in value)
        {
            if (sb.Length >= maxLength) break;
            if (char.IsAsciiLetterOrDigit(c) || c == '.' || c == '-' || c == '_') sb.Append(c);
        }

        return sb.Length > 0 ? sb.ToString() : "update";
    }


    /// <summary>
    /// Extension of the artifact URL, restricted to a short alphanumeric suffix.
    /// </summary>
    private static string SanitizeExtension(string url)
    {
        try
        {
            var ext = Path.GetExtension(new Uri(url).AbsolutePath);
            if (ext.Length is > 1 and <= 12 && ext[1..].All(char.IsAsciiLetterOrDigit)) return ext;
        }
        catch { }

        return ".pkg";
    }


    /// <summary>
    /// Finds the cached package for <paramref name="version"/>, without needing the manifest.
    /// </summary>
    public static string? FindCachedPackage(string version)
    {
        try
        {
            var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
            if (!Directory.Exists(dir)) return null;

            var prefix = $"{BHelper.AppName}_{Sanitize(version, 40)}.";
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var name = Path.GetFileName(file);
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && !name.EndsWith(".part", StringComparison.OrdinalIgnoreCase)) return file;
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Takes the cross-process download lock, or returns <c>null</c> when another instance holds it.
    /// </summary>
    /// <remarks>
    /// A file handle, not a Mutex: Mutex has thread affinity and cannot survive an await.
    /// </remarks>
    public static async Task<IDisposable?> AcquireDownloadLockAsync(CancellationToken ct)
    {
        var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
        Directory.CreateDirectory(dir);
        var lockPath = Path.Combine(dir, UpdateConstants.DownloadLockFile);

        // another instance may be mid-download, so wait it out rather than reporting a failure
        for (var i = 0; i < UpdateConstants.DownloadLockRetries; i++)
        {
            try
            {
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.None, 1, FileOptions.DeleteOnClose);
            }
            catch (IOException)
            {
                await Task.Delay(UpdateConstants.DownloadLockRetryDelayMs, ct).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(UpdateConstants.DownloadLockRetryDelayMs, ct).ConfigureAwait(false);
            }
        }

        return null;
    }


    /// <summary>
    /// Deletes every cached package except the one for <paramref name="keepVersion"/>.
    /// </summary>
    public static void PruneCacheExcept(string keepVersion)
    {
        try
        {
            var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
            if (!Directory.Exists(dir)) return;

            var keep = Path.GetFileName(FindCachedPackage(keepVersion) ?? string.Empty);

            foreach (var file in Directory.EnumerateFiles(dir))
            {
                var name = Path.GetFileName(file);
                if (name.Equals(keep, StringComparison.OrdinalIgnoreCase)) continue;
                if (name.Equals(UpdateConstants.DownloadLockFile, StringComparison.OrdinalIgnoreCase)) continue;
                if (name.Equals(UpdateConstants.ApplyAttemptFile, StringComparison.OrdinalIgnoreCase)) continue;

                TryDelete(file);
            }
        }
        catch { }
    }


    /// <summary>
    /// Records that an install of <paramref name="version"/> is starting.
    /// </summary>
    /// <remarks>
    /// Deployment kills this process before reporting, so the marker is the only failure evidence.
    /// </remarks>
    public static void MarkApplyAttempt(string version)
    {
        try
        {
            var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, UpdateConstants.ApplyAttemptFile), version);
        }
        catch { }
    }


    /// <summary>
    /// Version of the last install attempt, or <c>null</c> when none is recorded.
    /// </summary>
    public static string? ReadApplyAttempt()
    {
        try
        {
            var path = Path.Combine(BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir),
                UpdateConstants.ApplyAttemptFile);
            if (!File.Exists(path)) return null;

            var version = File.ReadAllText(path).Trim();
            return string.IsNullOrEmpty(version) ? null : version;
        }
        catch { return null; }
    }


    /// <summary>
    /// Deletes every cached update package.
    /// </summary>
    public static void ClearCache()
    {
        try
        {
            var dir = BHelper.ConfigDir(Dir.Cache, UpdateConstants.PackageCacheDir);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
        catch { }
    }


    /// <summary>
    /// Verifies a cached file against a lowercase hex SHA-256; deletes it when it does not match.
    /// </summary>
    public static async Task<bool> VerifyAsync(string filePath, string expectedSha256,
        CancellationToken ct = default)
    {
        try
        {
            if (!File.Exists(filePath)) return false;

            await using var stream = File.OpenRead(filePath);
            var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
            var actual = Convert.ToHexString(hash).ToLowerInvariant();

            if (string.Equals(actual, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            UpdateTrace.Mark($"verify:mismatch expected={expectedSha256} actual={actual}");
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"verify:error {ex.Message}");
        }

        TryDelete(filePath);
        return false;
    }


    /// <summary>
    /// Downloads and verifies the artifact, returning the cached path or <c>null</c> on failure.
    /// </summary>
    public static async Task<UpdateOpResult> DownloadAsync(HttpClient client, UpdateArtifactInfo artifact,
        string version, IProgress<double>? progress, CancellationToken ct)
    {
        var destPath = GetPackagePath(version, artifact.Url);

        // an already-verified download survives a restart, so never fetch the same bytes twice
        if (File.Exists(destPath) && await VerifyAsync(destPath, artifact.Sha256, ct).ConfigureAwait(false))
        {
            UpdateTrace.Mark($"download:cached {destPath}");
            return UpdateOpResult.Ok();
        }

        var partPath = destPath + ".part";

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(UpdateConstants.DownloadTimeout);
        var token = timeoutCts.Token;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            TryDelete(partPath);

            using var request = new HttpRequestMessage(HttpMethod.Get, artifact.Url);
            using var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var length = response.Content.Headers.ContentLength ?? artifact.Size;
            if (length > UpdateConstants.MaxPackageSize)
                throw new InvalidOperationException($"IGE: Update package too large: {length} bytes.");

            UpdateTrace.Mark($"download:begin {artifact.Url} ({length} bytes)");

            // hash while copying, so an 80 MB package is not read twice
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[BUFFER_SIZE];
            long total = 0;

            await using (var source = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false))
            await using (var dest = new FileStream(partPath, FileMode.Create, FileAccess.Write,
                FileShare.None, BUFFER_SIZE, useAsync: true))
            {
                int read;
                while ((read = await source.ReadAsync(buffer, token).ConfigureAwait(false)) > 0)
                {
                    hasher.AppendData(buffer, 0, read);
                    await dest.WriteAsync(buffer.AsMemory(0, read), token).ConfigureAwait(false);

                    total += read;
                    if (total > UpdateConstants.MaxPackageSize)
                        throw new InvalidOperationException("IGE: Update package exceeded the size limit.");

                    if (length > 0) progress?.Report(Math.Min(100d, total * 100d / length));
                }
            }

            var actual = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
            if (!string.Equals(actual, artifact.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                UpdateTrace.Mark($"download:hashMismatch expected={artifact.Sha256} actual={actual}");
                TryDelete(partPath);

                return UpdateOpResult.Fail(
                    "IGE: The downloaded update failed its integrity check and was discarded.",
                    $"URL: {artifact.Url}{Environment.NewLine}"
                    + $"Expected SHA-256: {artifact.Sha256}{Environment.NewLine}"
                    + $"Actual SHA-256:   {actual}");
            }

            // rename only after the digest matches, so a torn file is never installable
            TryDelete(destPath);
            File.Move(partPath, destPath);

            UpdateTrace.Mark($"download:ok {destPath}");
            return UpdateOpResult.Ok();
        }
        catch (OperationCanceledException)
        {
            TryDelete(partPath);
            return UpdateOpResult.Skip();
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"download:failed {ex}");
            TryDelete(partPath);
            return UpdateOpResult.Fail(ex);
        }
    }


    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }
}
