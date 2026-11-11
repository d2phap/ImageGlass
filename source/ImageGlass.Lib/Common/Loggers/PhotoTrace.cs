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
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ImageGlass.Common.Loggers;


/// <summary>
/// Opt-in, cross-platform photo-loading profiler. Records the full per-photo pipeline
/// (photo list -> metadata -> codec -> preview -> decode -> color management -> render)
/// and appends it to <c>ig_photo_trace.log</c> in the <see cref="Dir.Logs"/> folder of the config dir.
/// </summary>
/// <remarks>
/// <para>
/// Enable it by launching with the <see cref="AppCmds.PHOTO_TRACE"/> command-line flag;
/// <see cref="EnableFromArgs"/> is called during app-instance initialization. A normal
/// launch is completely silent (every entry point returns early when disabled).
/// </para>
/// <para>
/// Unlike <see cref="StartupTrace"/> (a one-shot startup timeline), photos load repeatedly and
/// concurrently, so marks are grouped into per-file <em>sessions</em>. Call <see cref="Begin"/>
/// when a photo starts loading and <see cref="End"/> when it finishes; <see cref="Mark"/> calls in
/// between are tagged with that session id and carry a per-step delta. Marks with no matching
/// session (e.g. gallery thumbnails, which are traced without a session) still log with the global
/// elapsed time. This is a debugging aid, so same-file concurrency is disambiguated by thread id
/// rather than strict correlation.
/// </para>
/// </remarks>
public static class PhotoTrace
{
    private static readonly Stopwatch _sw = Stopwatch.StartNew();
    private static readonly Lock _lock = new();
    private static readonly Dictionary<string, Session> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private static StreamWriter? _writer;
    private static int _sessionCounter;

    private const string MEMORY_KEY = "<memory>";


    private sealed class Session(int id, long beginMs)
    {
        public int Id { get; } = id;
        public long BeginMs { get; } = beginMs;
        public long LastMs { get; set; } = beginMs;
    }


    /// <summary>
    /// Whether trace output is enabled. Set by <see cref="EnableFromArgs"/> from the CLI flag.
    /// </summary>
    public static bool Enabled { get; private set; }


    /// <summary>
    /// Enables trace output if the given command-line args contain
    /// <see cref="AppCmds.PHOTO_TRACE"/>.
    /// </summary>
    public static void EnableFromArgs(string[]? args)
    {
        if (Enabled || args is null) return;

        foreach (var arg in args)
        {
            if (string.Equals(arg, AppCmds.PHOTO_TRACE, StringComparison.OrdinalIgnoreCase))
            {
                Enabled = true;
                return;
            }
        }
    }


    /// <summary>
    /// Opens a new trace session for a photo and logs a BEGIN banner. Pair with <see cref="End"/>.
    /// </summary>
    public static void Begin(string? file, string? detail = null)
    {
        if (!Enabled) return;

        var key = NormalizeKey(file);
        lock (_lock)
        {
            var ms = _sw.ElapsedMilliseconds;
            var id = ++_sessionCounter;
            _sessions[key] = new Session(id, ms);

            Write($"{Environment.NewLine}======== [#{id}] BEGIN  {key} ========"
                + (detail is null ? "" : $"  ({detail})"));
        }
    }


    /// <summary>
    /// Records a pipeline milestone. <paramref name="file"/> associates the mark with a session
    /// started by <see cref="Begin"/>; pass <c>null</c> for session-less marks (e.g. thumbnails).
    /// </summary>
    public static void Mark(string stage, string? file = null, string? detail = null)
    {
        if (!Enabled) return;

        lock (_lock)
        {
            var ms = _sw.ElapsedMilliseconds;
            var tid = Environment.CurrentManagedThreadId;

            // resolve the owning session for a per-step delta
            var id = "-";
            var delta = string.Empty;
            if (file is not null && _sessions.TryGetValue(NormalizeKey(file), out var s))
            {
                id = s.Id.ToString();
                delta = $"(+{ms - s.LastMs,5})";
                s.LastMs = ms;
            }

            var line = $"{ms,7} ms  {delta,-8}  [t{tid}]  [#{id}]  {stage}"
                + (detail is null ? "" : $" = {detail}");
            Write(line);
        }
    }


    /// <summary>
    /// Logs an END banner and closes the session opened by <see cref="Begin"/>.
    /// </summary>
    public static void End(string? file, string? detail = null)
    {
        if (!Enabled) return;

        var key = NormalizeKey(file);
        lock (_lock)
        {
            var ms = _sw.ElapsedMilliseconds;
            if (_sessions.Remove(key, out var s))
            {
                var elapsed = $"elapsed={ms - s.BeginMs} ms";
                Write($"======== [#{s.Id}] END    {key} ========  "
                    + $"({elapsed}{(detail is null ? "" : $", {detail}")})");
            }
        }
    }


    private static string NormalizeKey(string? file)
    {
        return string.IsNullOrEmpty(file) ? MEMORY_KEY : file;
    }


    /// <summary>
    /// Writes one line to the debug output and to the log file. Caller must hold <see cref="_lock"/>.
    /// </summary>
    private static void Write(string line)
    {
        Debug.WriteLine($"[IG-PHOTO] {line}");

        try
        {
            _writer ??= new StreamWriter(BHelper.ConfigDir(Dir.Logs, "ig_photo_trace.log"), append: true)
            {
                AutoFlush = true,
            };
            _writer.WriteLine(line);
        }
        catch { }
    }
}
