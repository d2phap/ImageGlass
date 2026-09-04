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
using ImageGlass.SDK.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Information about a running external tool process.
/// </summary>
internal sealed class ToolProcessInfo
{
    /// <summary>
    /// Gets the tool registration associated with the running process.
    /// </summary>
    public required ExternalTool Tool { get; init; }

    /// <summary>
    /// Gets the spawned external process.
    /// </summary>
    public required Process Process { get; init; }

    /// <summary>
    /// Gets the named-pipe server stream used for host-tool IPC.
    /// </summary>
    public required NamedPipeServerStream PipeServer { get; init; }

    /// <summary>
    /// Gets the generated pipe name passed to the tool process.
    /// </summary>
    public required string PipeName { get; init; }

    /// <summary>
    /// Gets the host-side pipe handler bound to the process connection.
    /// </summary>
    public required ToolPipeServer PipeHandler { get; init; }
}


/// <summary>
/// Manages external tool processes: spawning, pipe server, and lifecycle.
/// External tools are registered explicitly in <c>Config.Tools</c>; this manager
/// does not perform any folder discovery.
/// </summary>
public sealed class ToolProcessManager : PhDisposable
{
    private readonly Dictionary<string, ToolProcessInfo> _processes = [];
    private readonly Lock _lock = new();


    /// <summary>
    /// Starts a tool process: creates named pipe, spawns process, and waits for connection.
    /// A hard start failure returns null info plus the exception message; a start-but-no-connect
    /// returns null info with no reason (the caller treats that as a launched tool).
    /// </summary>
    internal async Task<(ToolProcessInfo? Info, string? Error)> StartToolAsync(ExternalTool tool)
    {
        if (string.IsNullOrEmpty(tool.Executable)) return (null, null);

        var pipeName = CreatePipeName();

        // CurrentUserOnly restricts the pipe to this user (SID/UID); the SDK client sets the same flag.
        var pipeServer = new NamedPipeServerStream(
            pipeName, PipeDirection.InOut, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

        // Expand %VAR% tokens so a portable/relative executable path works cross-platform.
        var exe = BHelper.ResolvePath(tool.Executable);
        var workingDir = Path.GetDirectoryName(exe) ?? string.Empty;

        // Launch the tool process with the generated pipe name in its arguments.
        Process? process;
        try
        {
            var filePath = Core.Photos?.Current?.FilePath ?? string.Empty;
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                WorkingDirectory = workingDir,
            };

            // Configured args first (each as its own element), then the pipe handshake.
            foreach (var arg in BHelper.BuildArgumentList(tool.Arguments, filePath))
            {
                psi.ArgumentList.Add(arg);
            }
            psi.ArgumentList.Add("--pipe");
            psi.ArgumentList.Add(pipeName);

            // Inside a Flatpak sandbox, route the launch through the host (no-op otherwise).
            BHelper.ApplyFlatpakHostSpawn(psi);

            process = Process.Start(psi);
        }
        catch (Exception ex)
        {
            // e.g. the executable/command was not found
            await pipeServer.DisposeAsync();
            return (null, ex.Message);
        }

        if (process is null)
        {
            await pipeServer.DisposeAsync();
            return (null, null);
        }

        // Wait for the integrated tool to attach to the pipe before exposing it.
        // Wait for tool to connect (10 second timeout)
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await pipeServer.WaitForConnectionAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Started but didn't connect in time (e.g. not an integrated tool). The process did
            // start, so this isn't a launch failure: return null info with no reason (caller ignores).
            try { process.Kill(); } catch { }
            await pipeServer.DisposeAsync();
            return (null, null);
        }

        // Windows only: reject if the connected client isn't the child we spawned (pipe squatting).
        // Provider is null on Linux/macOS, so this is skipped there.
        if (Core.PipeSecurityProvider is { } pipeSec
            && !pipeSec.VerifyClientProcess(pipeServer.SafePipeHandle, process.Id))
        {
            try { process.Kill(); } catch { }
            await pipeServer.DisposeAsync();
            return (null, null);
        }

        // Register the fully-connected process so later calls can reuse it.
        var pipeHandler = new ToolPipeServer(pipeServer, tool.ToolId);

        var info = new ToolProcessInfo
        {
            Tool = tool,
            Process = process,
            PipeServer = pipeServer,
            PipeName = pipeName,
            PipeHandler = pipeHandler,
        };

        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => CleanupExitedProcess(tool.ToolId, info);

        lock (_lock)
        {
            _processes[tool.ToolId] = info;
        }

        return (info, null);
    }


    /// <summary>
    /// Builds the pipe name. A bare GUID on Windows and macOS; on Linux an absolute socket path
    /// under the shared host home.
    /// </summary>
    private static string CreatePipeName()
    {
        var id = $"ig_{Guid.NewGuid():N}";
        if (BHelper.OS != OSType.Linux) return id;

        // ~/.cache stays visible through --filesystem=host, unlike /tmp.
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache", "ImageGlass", "tools");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, id);
    }


    /// <summary>
    /// Sends shutdown to tool and kills the process if it doesn't exit gracefully.
    /// </summary>
    public async Task StopToolAsync(string toolId)
    {
        ToolProcessInfo? info;
        lock (_lock)
        {
            if (!_processes.Remove(toolId, out info)) return;
        }

        try
        {
            // Ask the tool to shut down cleanly before escalating to process kill.
            if (!info.Process.HasExited)
            {
                info.PipeHandler.SendEvent(MessageTypes.SHUTDOWN);
            }

            // Give the process a short grace period to tear itself down.
            // Wait a short time for graceful exit
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                if (!info.Process.HasExited)
                {
                    await info.Process.WaitForExitAsync(cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                try { info.Process.Kill(); } catch { }
            }
        }
        catch { }
        finally
        {
            DisposeProcessInfo(info);
        }
    }


    /// <summary>
    /// Stops all running tool processes.
    /// </summary>
    public async Task StopAllAsync()
    {
        string[] ids;
        lock (_lock)
        {
            ids = [.. _processes.Keys];
        }

        foreach (var id in ids)
        {
            await StopToolAsync(id);
        }
    }


    /// <summary>
    /// Gets information about a running tool, or <c>null</c> if it is not active.
    /// </summary>
    internal ToolProcessInfo? GetRunningTool(string toolId)
    {
        ToolProcessInfo? exitedInfo = null;

        lock (_lock)
        {
            _processes.TryGetValue(toolId, out var info);
            if (info is not null && info.Process.HasExited)
            {
                _processes.Remove(toolId);
                exitedInfo = info;
                info = null;
            }

            if (exitedInfo is null)
            {
                return info;
            }
        }

        DisposeProcessInfo(exitedInfo);
        return null;
    }


    /// <summary>
    /// Gets whether a tool process is currently running.
    /// </summary>
    public bool IsRunning(string toolId)
    {
        return GetRunningTool(toolId) is not null;
    }


    /// <summary>
    /// Broadcasts an event to all running tool processes.
    /// </summary>
    public void BroadcastToAll(string type, object? payload = null)
    {
        ToolProcessInfo[] infos;
        lock (_lock)
        {
            infos = [.. _processes.Values];
        }

        // Send the event optimistically and prune dead/broken processes on failure.
        foreach (var info in infos)
        {
            if (info.Process.HasExited)
            {
                CleanupExitedProcess(info.Tool.ToolId, info);
                continue;
            }

            try
            {
                info.PipeHandler.SendEvent(type, payload);
            }
            catch
            {
                CleanupExitedProcess(info.Tool.ToolId, info);
            }
        }
    }


    /// <summary>
    /// Broadcasts an event only to tools that have subscribed to it.
    /// </summary>
    public void BroadcastToSubscribed(string type, object? payload, Func<ToolEventSubscriptions, bool> filter)
    {
        ToolProcessInfo[] infos;
        lock (_lock)
        {
            infos = [.. _processes.Values];
        }

        // Apply the caller-provided subscription filter before sending.
        foreach (var info in infos)
        {
            if (info.Process.HasExited)
            {
                CleanupExitedProcess(info.Tool.ToolId, info);
                continue;
            }

            try
            {
                if (filter(info.PipeHandler.Subscriptions))
                {
                    info.PipeHandler.SendEvent(type, payload);
                }
            }
            catch
            {
                CleanupExitedProcess(info.Tool.ToolId, info);
            }
        }
    }


    /// <summary>
    /// Removes an exited process from the registry and disposes its resources.
    /// </summary>
    private void CleanupExitedProcess(string toolId, ToolProcessInfo info)
    {
        var shouldDispose = false;

        lock (_lock)
        {
            if (_processes.TryGetValue(toolId, out var current) && ReferenceEquals(current, info))
            {
                _processes.Remove(toolId);
                shouldDispose = true;
            }
        }

        if (shouldDispose)
        {
            DisposeProcessInfo(info);
        }
    }


    /// <summary>
    /// Disposes the IPC and process resources associated with one tool.
    /// </summary>
    private static void DisposeProcessInfo(ToolProcessInfo info)
    {
        try { info.PipeHandler.Dispose(); } catch { }
        try { info.PipeServer.Dispose(); } catch { }
        try { info.Process.Dispose(); } catch { }
    }


    /// <summary>
    /// Disposes any remaining tracked tool processes during manager shutdown.
    /// </summary>
    protected override void OnDisposing()
    {
        // Synchronously dispose — StopAllAsync should have been called before
        lock (_lock)
        {
            foreach (var info in _processes.Values)
            {
                try
                {
                    info.PipeHandler.Dispose();
                    info.PipeServer.Dispose();
                    try { info.Process.Kill(); } catch { }
                    info.Process.Dispose();
                }
                catch { }
            }
            _processes.Clear();
        }
    }
}
