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
/// Opt-in, cross-platform startup profiler. Records wall-clock milestones from process start to the
/// first window paint and writes them to <c>ig_startup_trace.log</c> in the <see cref="Dir.Logs"/> folder of the config dir.
/// </summary>
/// <remarks>
/// <para>
/// Enable it by launching with the <see cref="AppCmds.STARTUP_TRACE"/> 
/// command-line flag; <see cref="EnableFromArgs"/> is called during app-instance initialization.
/// </para>
/// <para>
/// Marks are always buffered (a couple of cheap locked list adds during startup only), so marks
/// recorded before the flag is parsed - e.g. the very first <c>Main</c> mark - are still captured.
/// <see cref="Flush"/> only produces output when tracing is enabled, so a normal launch is silent.
/// </para>
/// </remarks>
public static class StartupTrace
{
    private static readonly Stopwatch _sw = Stopwatch.StartNew();
    private static readonly List<(long Ms, string Name, int Tid)> _marks = new(32);
    private static readonly Lock _lock = new();
    private static int _flushedCount;


    /// <summary>
    /// Whether trace output is enabled. Set by <see cref="EnableFromArgs"/> from the CLI flag.
    /// </summary>
    public static bool Enabled { get; private set; }


    /// <summary>
    /// Enables trace output if the given command-line args contain
    /// <see cref="AppCmds.STARTUP_TRACE"/>. Safe to call after marks have been recorded.
    /// </summary>
    public static void EnableFromArgs(string[]? args)
    {
        if (Enabled || args is null) return;

        foreach (var arg in args)
        {
            if (string.Equals(arg, AppCmds.STARTUP_TRACE, StringComparison.OrdinalIgnoreCase))
            {
                Enabled = true;
                return;
            }
        }
    }


    /// <summary>
    /// Records a named milestone with the current elapsed time. Always buffered so it survives being
    /// called before the trace flag is parsed; output is gated by <see cref="Enabled"/> in
    /// <see cref="Flush"/>.
    /// </summary>
    public static void Mark(string name)
    {
        lock (_lock)
        {
            _marks.Add((_sw.ElapsedMilliseconds, name, Environment.CurrentManagedThreadId));
        }
    }


    /// <summary>
    /// Writes marks recorded since the last flush; incremental so later calls capture new marks.
    /// </summary>
    public static void Flush()
    {
        if (!Enabled) return;

        lock (_lock)
        {
            if (_flushedCount >= _marks.Count) return;

            var lines = new List<string>(_marks.Count - _flushedCount + 2);

            // header + process-start estimate on the first flush only
            if (_flushedCount == 0)
            {
                lines.Add($"===== ImageGlass startup trace @ PID {Environment.ProcessId} =====");
                try
                {
                    var startToNow = DateTime.Now - Process.GetCurrentProcess().StartTime;
                    lines.Add($"   (process start -> first mark ~ {startToNow.TotalMilliseconds,7:0.0} ms)");
                }
                catch { }
            }

            // delta from the last already-flushed mark
            var prev = _flushedCount > 0 ? _marks[_flushedCount - 1].Ms : 0;
            for (var i = _flushedCount; i < _marks.Count; i++)
            {
                var (ms, name, tid) = _marks[i];
                lines.Add($"{ms,7} ms  (+{ms - prev,6} ms)  [t{tid}]  {name}");
                prev = ms;
            }
            _flushedCount = _marks.Count;

            foreach (var line in lines)
            {
                Debug.WriteLine($"[IG-STARTUP] {line}");
            }

            try
            {
                var logPath = BHelper.ConfigDir(Dir.Logs, "ig_startup_trace.log");
                File.AppendAllText(logPath, string.Join(Environment.NewLine, lines)
                    + Environment.NewLine + Environment.NewLine);
            }
            catch { }
        }
    }
}
