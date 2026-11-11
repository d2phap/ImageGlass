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
using Avalonia.Platform;
using ImageGlass.Common.Types;
using NetCoreAudio;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common;


public partial class BHelper
{
    private const string NOTIFICATION_SOUND_ASSET = "avares://ImageGlass.Lib/Assets/Sounds/notification.wav";
    private const int LINUX_PLAYBACK_TIMEOUT_MS = 10_000;
    private static readonly Lock _notificationSoundLock = new();
    private static string? _notificationSoundPath;
    private static Player? _notificationPlayer;

    /// <summary>
    /// A Linux command-line audio player: the executable plus its non-file arguments.
    /// </summary>
    private sealed record LinuxSoundPlayer(string Exe, string[] Args);

    /// <summary>
    /// Candidates in preference order, sound-server aware first. No single one covers every
    /// target: the Flatpak runtime ships no <c>aplay</c>, a bare ALSA host ships no <c>paplay</c>.
    /// </summary>
    private static readonly LinuxSoundPlayer[] LINUX_SOUND_PLAYERS =
    [
        new("paplay", []),
        new("pw-play", []),
        new("aplay", ["-q"]),
        new("ffplay", ["-nodisp", "-autoexit", "-loglevel", "quiet"]),
    ];
    private static volatile LinuxSoundPlayer? _linuxSoundPlayer;


    /// <summary>
    /// Plays the app notification sound. Failures are silent:
    /// a missed notification sound must never interrupt what triggered it.
    /// </summary>
    public static async Task PlayNotificationSoundAsync()
    {
        try
        {
            var filePath = GetNotificationSoundPath();
            if (filePath is null) return;

            // NetCoreAudio hardcodes `aplay` on Linux, so resolve a player that actually exists
            if (OS == OSType.Linux)
            {
                await PlayLinuxSoundAsync(filePath).ConfigureAwait(false);
                return;
            }

            var player = GetNotificationPlayer();
            if (player is null) return;

            // returns as soon as playback starts; a new call replaces the sound still playing
            await player.Play(filePath).ConfigureAwait(false);
        }
        catch { }
    }


    /// <summary>
    /// Plays <paramref name="filePath"/> with the first player that runs to a clean exit, then
    /// reuses it: a player can be installed yet fail for want of the sound server it talks to.
    /// </summary>
    private static async Task PlayLinuxSoundAsync(string filePath)
    {
        // a remembered player can be uninstalled mid-session, so re-probe rather than stay mute
        if (_linuxSoundPlayer is { } cached
            && await TryPlayLinuxSoundAsync(cached, filePath).ConfigureAwait(false))
        {
            return;
        }
        _linuxSoundPlayer = null;

        foreach (var player in LINUX_SOUND_PLAYERS)
        {
            if (await TryPlayLinuxSoundAsync(player, filePath).ConfigureAwait(false))
            {
                _linuxSoundPlayer = player;
                return;
            }
        }
    }


    /// <summary>
    /// Runs one player to completion.
    /// </summary>
    /// <returns><c>false</c> if it is missing or failed, so the caller should try the next one.</returns>
    private static async Task<bool> TryPlayLinuxSoundAsync(LinuxSoundPlayer player, string filePath)
    {
        var psi = new ProcessStartInfo(player.Exe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in player.Args) psi.ArgumentList.Add(arg);
        psi.ArgumentList.Add(filePath);

        // a missing executable throws Win32Exception
        Process? proc;
        try
        {
            proc = Process.Start(psi);
        }
        catch
        {
            return false;
        }
        if (proc is null) return false;

        using (proc)
        {
            using var timeoutCts = new CancellationTokenSource(LINUX_PLAYBACK_TIMEOUT_MS);
            try
            {
                await proc.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // hung on an unresponsive server: playback did start, so stop rather than stack another
                try { proc.Kill(true); } catch { }
                return true;
            }

            return proc.ExitCode == 0;
        }
    }


    /// <summary>
    /// Gets the shared player, built on first use because its constructor throws on an OS
    /// <see cref="Player"/> has no backend for, which a static initializer could not contain.
    /// </summary>
    private static Player? GetNotificationPlayer()
    {
        lock (_notificationSoundLock)
        {
            try
            {
                _notificationPlayer ??= new Player();
            }
            catch { }

            return _notificationPlayer;
        }
    }


    /// <summary>
    /// Extracts the bundled notification WAV into the temp dir once per session: every platform
    /// plays it by file path, but the asset only exists inside the assembly.
    /// </summary>
    private static string? GetNotificationSoundPath()
    {
        lock (_notificationSoundLock)
        {
            if (_notificationSoundPath is not null && File.Exists(_notificationSoundPath))
            {
                return _notificationSoundPath;
            }

            try
            {
                var filePath = ConfigDir(Dir.Temporary, "ig_notification.wav");

                using (var assetStream = AssetLoader.Open(new Uri(NOTIFICATION_SOUND_ASSET)))
                using (var fileStream = File.Create(filePath))
                {
                    assetStream.CopyTo(fileStream);
                }

                _notificationSoundPath = filePath;
            }
            catch
            {
                _notificationSoundPath = null;
            }

            return _notificationSoundPath;
        }
    }

}
