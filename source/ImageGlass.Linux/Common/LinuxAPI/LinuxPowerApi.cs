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
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ImageGlass.Linux.Common;

/// <summary>
/// Inhibitor helpers to keep the system and the display awake.
/// A D-Bus inhibit lock only lives as long as the connection holding it, so helper processes take them.
/// </summary>
public static class LinuxPowerApi
{
    private const int HEARTBEAT_MS = 30_000;
    private const int EXIT_TIMEOUT_MS = 300;
    private const int CALL_TIMEOUT_MS = 5_000;

    private static readonly Lock _lock = new();
    private static readonly List<Process> _inhibitors = [];
    private static Timer? _idleResetTimer;
    private static InterlockedBool _useXdgReset;


    /// <summary>
    /// Starts the inhibitors and the idle-reset heartbeat until <see cref="AllowSleep"/>.
    /// </summary>
    public static void PreventSleep(string reason)
    {
        lock (_lock)
        {
            if (_idleResetTimer is not null) return;

            // logind: blocks idle actions and suspend
            StartInhibitor__("systemd-inhibit", [
                "--what=idle:sleep",
                $"--who={BHelper.AppDisplayName}",
                $"--why={reason}",
                "--mode=block",
                "cat",
            ]);

            // GNOME blanks the screen from its own session idle timer, not from logind
            StartInhibitor__("gnome-session-inhibit", [
                "--app-id", BHelper.AppDisplayName,
                "--reason", reason,
                "--inhibit", "idle",
                "--inhibit", "suspend",
                "cat",
            ]);

            _useXdgReset.SetFalse();
            _idleResetTimer = new Timer(_ => ResetIdleTimer__(), null, HEARTBEAT_MS, HEARTBEAT_MS);
        }
    }


    /// <summary>
    /// Stops everything started by <see cref="PreventSleep"/>.
    /// </summary>
    public static void AllowSleep()
    {
        lock (_lock)
        {
            _idleResetTimer?.Dispose();
            _idleResetTimer = null;

            foreach (var proc in _inhibitors)
            {
                try
                {
                    // closing the pipe ends `cat`, which ends the inhibitor holding the lock
                    proc.StandardInput.Close();
                    if (!proc.WaitForExit(EXIT_TIMEOUT_MS)) proc.Kill(true);
                }
                catch
                {
                    // best-effort: already exited
                }
                finally
                {
                    proc.Dispose();
                }
            }

            _inhibitors.Clear();
        }
    }


    #region Private methods

    /// <summary>
    /// Starts an inhibitor helper that holds its lock until its stdin pipe closes.
    /// </summary>
    private static void StartInhibitor__(string fileName, string[] args)
    {
        try
        {
            var proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardInput = true;
            foreach (var arg in args) proc.StartInfo.ArgumentList.Add(arg);

            // the helpers live on the host, not in the runtime
            BHelper.ApplyFlatpakHostSpawn(proc.StartInfo);

            if (proc.Start()) _inhibitors.Add(proc);
            else proc.Dispose();
        }
        catch
        {
            // best-effort: helper not installed
        }
    }


    /// <summary>
    /// Resets the session idle timer, for the desktops that honour no inhibitor above.
    /// </summary>
    private static void ResetIdleTimer__()
    {
        try
        {
            if (!_useXdgReset.Value)
            {
                var isDone = RunSilently__("gdbus", [
                    "call", "--session",
                    "--dest", "org.freedesktop.ScreenSaver",
                    "--object-path", "/org/freedesktop/ScreenSaver",
                    "--method", "org.freedesktop.ScreenSaver.SimulateUserActivity",
                ], hostSpawn: false);

                if (isDone) return;
                _useXdgReset.SetTrue();
            }

            _ = RunSilently__("xdg-screensaver", ["reset"], hostSpawn: true);
        }
        catch
        {
            // an exception escaping a timer callback would kill the process
        }
    }


    /// <summary>
    /// Runs a short command and reports whether it exited successfully.
    /// </summary>
    private static bool RunSilently__(string fileName, string[] args, bool hostSpawn)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            foreach (var arg in args) proc.StartInfo.ArgumentList.Add(arg);

            if (hostSpawn) BHelper.ApplyFlatpakHostSpawn(proc.StartInfo);
            if (!proc.Start()) return false;

            return proc.WaitForExit(CALL_TIMEOUT_MS) && proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    #endregion // Private methods
}
