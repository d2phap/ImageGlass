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
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Downloads an update artifact to the temp dir and verifies it against the manifest digest.
/// </summary>
internal static class AppUpdateDownloader
{
    private const int BUFFER_SIZE = 128 * 1024;


    /// <summary>
    /// Full path the artifact for <paramref name="version"/> is cached at.
    /// </summary>
    public static string GetPackagePath(string version, string url)
    {
        var ext = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".pkg";

        return BHelper.ConfigDir(Dir.Temporary, UpdateConstants.PackageCacheDir,
            $"{BHelper.AppName}_{version}{ext}");
    }


    /// <summary>
    /// Deletes every cached update package.
    /// </summary>
    public static void ClearCache()
    {
        try
        {
            var dir = BHelper.ConfigDir(Dir.Temporary, UpdateConstants.PackageCacheDir);
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
    public static async Task<string?> DownloadAsync(HttpClient client, UpdateArtifactInfo artifact,
        string version, IProgress<double>? progress, CancellationToken ct)
    {
        var destPath = GetPackagePath(version, artifact.Url);

        // an already-verified download survives a restart, so never fetch the same bytes twice
        if (File.Exists(destPath) && await VerifyAsync(destPath, artifact.Sha256, ct).ConfigureAwait(false))
        {
            UpdateTrace.Mark($"download:cached {destPath}");
            return destPath;
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
                return null;
            }

            // rename only after the digest matches, so a torn file is never installable
            TryDelete(destPath);
            File.Move(partPath, destPath);

            UpdateTrace.Mark($"download:ok {destPath}");
            return destPath;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"download:failed {ex.Message}");
            TryDelete(partPath);
            return null;
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
