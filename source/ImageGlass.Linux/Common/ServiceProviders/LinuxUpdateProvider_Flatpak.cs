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
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;

public partial class LinuxUpdateProvider
{
    private const string SYSTEM_INSTALLATION = "/var/lib/flatpak";

    /// <summary>
    /// Longest wait for the startup probe when eligibility is asked before it finished.
    /// </summary>
    private const int FLATPAK_PROBE_WAIT_MS = 3_000;

    private static readonly TimeSpan FLATPAK_QUERY_TIMEOUT = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Covers a runtime download, and a polkit prompt the user may take a while to answer.
    /// </summary>
    private static readonly TimeSpan FLATPAK_INSTALL_TIMEOUT = TimeSpan.FromMinutes(15);

    private readonly Task<FlatpakInstall?> _flatpakProbe;


    /// <summary>
    /// The running app's installation for the host <c>flatpak</c> CLI; no user dir means the system one.
    /// </summary>
    private sealed record FlatpakInstall(string AppId, string Arch, string Branch, string Commit, string? UserDir);


    /// <summary>
    /// The running app's installation when it came from a bundle, otherwise <c>null</c>.
    /// </summary>
    private FlatpakInstall? FlatpakBundle
    {
        get
        {
            try
            {
                return _flatpakProbe.Wait(FLATPAK_PROBE_WAIT_MS) ? _flatpakProbe.Result : null;
            }
            catch { return null; }
        }
    }



    #region Flatpak

    /// <summary>
    /// Installs the verified bundle into this app's installation, checks it deployed, then relaunches.
    /// </summary>
    private async Task<UpdateOpResult> ApplyFlatpakBundleAsync(string packageFilePath, UpdateReleaseInfo release)
    {
        if (FlatpakBundle is not { } install)
        {
            return UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates.");
        }

        // the host flatpak opens the bundle itself, so it needs the path the host sees
        var bundlePath = BHelper.GetRealPlatformPath(packageFilePath);

        // no --noninteractive: it also suppresses the polkit prompt a system installation needs
        var add = await RunHostFlatpakAsync(install, FLATPAK_INSTALL_TIMEOUT,
            "install", "-y", "--bundle", bundlePath).ConfigureAwait(false);
        UpdateTrace.Mark($"apply:flatpakInstall exit={add.ExitCode}");

        if (add.ExitCode != 0)
        {
            var reason = GetLastLine(add.StdErr, add.StdOut);
            return UpdateOpResult.Fail($"IGE: Flatpak could not install the update: {reason}",
                FormatFlatpakDetails(install, bundlePath, add));
        }

        // flatpak also exits 0 with "nothing to do", so only a newly deployed commit proves the update
        var info = await RunHostFlatpakAsync(install, FLATPAK_QUERY_TIMEOUT,
            "info", "--show-commit", install.AppId, install.Branch).ConfigureAwait(false);
        var deployed = info.StdOut.Trim();

        if (info.ExitCode != 0 || deployed.Length == 0
            || string.Equals(deployed, install.Commit, StringComparison.Ordinal))
        {
            return UpdateOpResult.Fail("IGE: Flatpak finished, but the installed ImageGlass did not change.",
                FormatFlatpakDetails(install, bundlePath, add));
        }

        UpdateTrace.Mark($"apply:deployed {deployed}");
        await ForgetInstalledUpdateAsync().ConfigureAwait(false);

        // the new instance starts on the host, so it outlives this sandbox
        var relaunch = await RelaunchAsync(CreateHostFlatpakStartInfo(install, "run",
            $"--branch={install.Branch}", $"--arch={install.Arch}", install.AppId)).ConfigureAwait(false);
        if (relaunch.IsSuccess) return relaunch;

        return UpdateOpResult.Fail($"IGE: ImageGlass {release.Version} was installed, but it could not start.",
            $"{relaunch.ErrorMessage}{Environment.NewLine}{relaunch.ErrorDetails}");
    }


    /// <summary>
    /// Resolves the running installation, and keeps it only when its origin is a bundle's.
    /// </summary>
    private static async Task<FlatpakInstall?> DetectFlatpakBundleAsync()
    {
        try
        {
            var install = ReadFlatpakInfo();
            if (install is null) return null;

            var origin = await RunHostFlatpakAsync(install, FLATPAK_QUERY_TIMEOUT,
                "info", "--show-origin", install.AppId, install.Branch).ConfigureAwait(false);
            var originName = origin.StdOut.Trim();

            if (origin.ExitCode != 0 || originName.Length == 0)
            {
                UpdateTrace.Mark($"detect:flatpakOriginUnknown exit={origin.ExitCode} {origin.StdErr.Trim()}");
                return null;
            }

            // a bundle's origin is disabled, so it is listed only with --show-disabled
            var remotes = await RunHostFlatpakAsync(install, FLATPAK_QUERY_TIMEOUT,
                "remotes", "--show-disabled", "--columns=name,url").ConfigureAwait(false);

            foreach (var line in remotes.StdOut.Split('\n'))
            {
                var columns = line.Split('\t');
                if (!string.Equals(columns[0].Trim(), originName, StringComparison.Ordinal)) continue;

                // a remote with a URL updates through flatpak; a bundle over it would fork the app off it
                var url = columns.Length > 1 ? columns[1].Trim() : string.Empty;
                UpdateTrace.Mark($"detect:flatpak origin={originName} url={url}");

                return url.Length == 0 ? install : null;
            }

            UpdateTrace.Mark($"detect:flatpakOriginNotListed {originName}");
            return null;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"detect:flatpakFailed {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Reads the running instance from <c>/.flatpak-info</c>; <c>null</c> when host commands are not allowed.
    /// </summary>
    private static FlatpakInstall? ReadFlatpakInfo()
    {
        string? section = null, name = null, arch = null, branch = null, commit = null, appPath = null;
        var canSpawnHost = false;

        foreach (var rawLine in File.ReadLines("/.flatpak-info"))
        {
            var line = rawLine.Trim();
            if (line.StartsWith('['))
            {
                section = line;
                continue;
            }

            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0) continue;

            var key = line[..eqIndex];
            var value = line[(eqIndex + 1)..];

            switch (section)
            {
                case "[Application]" when key == "name": name = value; break;
                case "[Instance]" when key == "arch": arch = value; break;
                case "[Instance]" when key == "branch": branch = value; break;
                case "[Instance]" when key == "app-commit": commit = value; break;
                case "[Instance]" when key == "app-path": appPath = value; break;

                // flatpak-spawn --host goes through this bus name
                case "[Session Bus Policy]" when key == "org.freedesktop.Flatpak":
                    canSpawnHost = value is "talk" or "own";
                    break;
            }
        }

        if (!canSpawnHost)
        {
            UpdateTrace.Mark("detect:flatpakNoHostAccess");
            return null;
        }

        if (name is null || arch is null || branch is null || commit is null || appPath is null) return null;

        // app-path is <installation>/app/<id>/<arch>/<branch>/<commit>/files
        var suffix = $"/app/{name}/{arch}/{branch}/{commit}/files";
        if (!appPath.EndsWith(suffix, StringComparison.Ordinal))
        {
            UpdateTrace.Mark($"detect:flatpakUnknownLayout {appPath}");
            return null;
        }

        var root = appPath[..^suffix.Length];
        return new FlatpakInstall(name, arch, branch, commit,
            string.Equals(root, SYSTEM_INSTALLATION, StringComparison.Ordinal) ? null : root);
    }


    /// <summary>
    /// Runs the host <c>flatpak</c> CLI against the running app's installation; never throws.
    /// </summary>
    private static Task<(int ExitCode, string StdOut, string StdErr)> RunHostFlatpakAsync(
        FlatpakInstall install, TimeSpan timeout, string command, params string[] args)
    {
        return RunCommandAsync(CreateHostFlatpakStartInfo(install, command, args), timeout);
    }


    /// <summary>
    /// Builds a host <c>flatpak</c> command pinned to the installation this instance runs from.
    /// </summary>
    private static ProcessStartInfo CreateHostFlatpakStartInfo(FlatpakInstall install, string command,
        params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // a user installation outside flatpak's default path is only reachable through this variable
        if (install.UserDir is { } userDir)
        {
            psi.FileName = "env";
            psi.ArgumentList.Add($"FLATPAK_USER_DIR={userDir}");
            psi.ArgumentList.Add("flatpak");
        }
        else
        {
            psi.FileName = "flatpak";
        }

        psi.ArgumentList.Add(command);
        psi.ArgumentList.Add(install.UserDir is null ? "--system" : "--user");
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        BHelper.ApplyFlatpakHostSpawn(psi);
        return psi;
    }


    /// <summary>
    /// Diagnostic detail of a host flatpak step, for the error dialog's expander.
    /// </summary>
    private static string FormatFlatpakDetails(FlatpakInstall install, string bundlePath,
        (int ExitCode, string StdOut, string StdErr) step)
    {
        var nl = Environment.NewLine;

        return $"Bundle: {bundlePath}{nl}"
            + $"Installation: {install.UserDir ?? SYSTEM_INSTALLATION}{nl}"
            + $"Installed commit: {install.Commit}{nl}"
            + $"Exit code: {step.ExitCode}{nl}"
            + $"{step.StdOut.Trim()}{nl}{step.StdErr.Trim()}";
    }

    #endregion // Flatpak

}
