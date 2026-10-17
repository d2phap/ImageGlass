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
using ImageGlass.Common.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ImageGlass.Common.Loggers;


/// <summary>
/// Auto-update trace, written to <c>ig_update.log</c> in the <see cref="Dir.Logs"/> folder.
/// </summary>
/// <remarks>
/// Gated on <c>Config.EnableDebug</c>, not a CLI flag: a background failure cannot be reproduced on demand.
/// </remarks>
public static class UpdateTrace
{
    private static readonly Lock _lock = new();


    /// <summary>
    /// Whether trace output is enabled.
    /// </summary>
    public static bool Enabled => Core.Config?.EnableDebug == true;


    /// <summary>
    /// Appends one timestamped line; never throws.
    /// </summary>
    public static void Mark(string message)
    {
        Debug.WriteLine($"[IG-UPDATE] {message}");
        if (!Enabled) return;

        try
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  [{Environment.ProcessId}]  {message}";

            lock (_lock)
            {
                var logPath = BHelper.ConfigDir(Dir.Logs, "ig_update.log");
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
        }
        catch { }
    }
}
