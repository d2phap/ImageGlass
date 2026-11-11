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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using ImageGlass.SDK.Tools;
using ImageGlass.UI.Viewer;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Outcome of an attempt to launch an external tool. <see cref="Error"/> carries a
/// human-readable reason (a launch exception message or a connect-timeout note) when available.
/// </summary>
internal readonly record struct ToolLaunchResult(bool Success, string? Error)
{
    public static ToolLaunchResult Ok { get; } = new(true, null);
    public static ToolLaunchResult Fail(string? error) => new(false, error);
}


/// <summary>
/// Proxy that satisfies the <see cref="ITool"/> interface for a non-hosted
/// external (out-of-process) tool described by an <see cref="ExternalTool"/>
/// entry in <c>Config.Tools</c>.
///
/// When <see cref="ExternalTool.IsIntegrated"/> is <c>true</c>, the tool is launched
/// as an integrated child process connected to the host via
/// <see cref="ToolPipeServer"/>. Otherwise the executable is launched
/// detached with no IPC.
/// </summary>
internal sealed class ExternalToolProxy : ITool
{
    private readonly ExternalTool _tool;
    private readonly ToolProcessManager _processManager;

    /// <summary>
    /// Gets the stable ID of the proxied external tool.
    /// </summary>
    public string ToolId => _tool.ToolId;

    /// <summary>
    /// Gets whether the tool is hosted in-process.
    /// External tools always return <c>false</c>.
    /// </summary>
    public bool IsHosted => false;

    /// <summary>
    /// Gets the tool settings payload.
    /// External tools currently do not expose structured settings here.
    /// </summary>
    public object? Settings => null;

    /// <summary>
    /// Gets or sets the active viewer associated with the tool contract.
    /// </summary>
    public ViewerControl Viewer { get; set; } = null!;

    /// <summary>
    /// Gets the original registration entry used for menu building and launch metadata.
    /// </summary>
    internal ExternalTool Tool => _tool;


    /// <summary>
    /// Creates a proxy for one configured external tool entry.
    /// </summary>
    public ExternalToolProxy(ExternalTool tool, ToolProcessManager processManager)
    {
        _tool = tool;
        _processManager = processManager;
    }


    /// <summary>
    /// Launches the external tool (detached or integrated). On a real launch failure the result
    /// carries the reason (the launch exception message). A tool that starts but never connects
    /// to the host pipe is treated as launched, not a failure.
    /// </summary>
    public async Task<ToolLaunchResult> TryLaunchAsync(ToolExecutionContext context)
    {
        // Detached mode: just spawn the executable with arguments and walk away.
        if (!_tool.IsIntegrated)
        {
            return TryLaunchDetached(context);
        }

        // Integrated mode: reuse an existing process when possible.
        var info = _processManager.GetRunningTool(ToolId);
        if (info is null)
        {
            // Start the process, establish the pipe, and send the one-time init payload.
            var (started, error) = await _processManager.StartToolAsync(_tool);

            // a hard start failure (e.g. exe not found) carries a reason; a tool that started but
            // never connected returns no reason -> treat as launched rather than nag the user.
            if (started is null) return error is null ? ToolLaunchResult.Ok : ToolLaunchResult.Fail(error);
            info = started;

            info.PipeHandler.SendEvent(MessageTypes.INIT, new ToolInitPayload
            {
                ToolId = ToolId,
                DataDirectory = Path.GetDirectoryName(BHelper.ResolvePath(_tool.Executable)) ?? string.Empty,
                PipeName = info.PipeName,
                ThemeInfo = new ThemeInfo
                {
                    IsDarkMode = Core.Theme.Settings.IsDarkMode,
                    AccentColor = Core.AccentColor.ToString(),
                    BackgroundColor = Core.Config.BackgroundColor,
                },
            });

            // Keep the pipe reader alive for follow-up requests from the tool.
            _ = Task.Run(() => info.PipeHandler.RunMessageLoopAsync(CancellationToken.None));
        }

        // Trigger the tool's actual action once the process is ready.
        info.PipeHandler.SendEvent(MessageTypes.EXECUTE);
        return ToolLaunchResult.Ok;
    }


    /// <summary>
    /// Starts the external tool without IPC. On failure the result carries the reason
    /// (the launch exception message), or a null reason when no executable is configured.
    /// </summary>
    private ToolLaunchResult TryLaunchDetached(ToolExecutionContext context)
    {
        if (string.IsNullOrEmpty(_tool.Executable)) return ToolLaunchResult.Fail(null);

        // Resolve %VAR% tokens, quotes, and .lnk targets so a portable/relative path works.
        var exe = BHelper.ResolvePath(_tool.Executable).Trim();
        var filePath = Core.Photos?.Current?.FilePath ?? string.Empty;

        try
        {
            // App protocol (e.g. "ms-settings:"): the OS shell must resolve the scheme, so it
            // can't use ArgumentList. Reuse the canonical launcher, which builds the URI as
            // Executable + arguments and shell-executes it.
            if (exe.EndsWith(':'))
            {
                var built = BHelper.BuildExeArgList(exe, _tool.Arguments, filePath);
                _ = BHelper.RunExeAsync(built.Executable, built.Args);
                return ToolLaunchResult.Ok;
            }

            // Regular executable: no shell, each argument passed as its own element so a
            // crafted filename cannot inject extra arguments or shell syntax.
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(exe) ?? string.Empty,
            };
            foreach (var arg in BHelper.BuildArgumentList(_tool.Arguments, filePath))
            {
                psi.ArgumentList.Add(arg);
            }

            // Inside a Flatpak sandbox, route the launch through the host (no-op otherwise).
            BHelper.ApplyFlatpakHostSpawn(psi);

            // Process.Start throws Win32Exception when the executable/command can't be found.
            return Process.Start(psi) is not null ? ToolLaunchResult.Ok : ToolLaunchResult.Fail(null);
        }
        catch (Exception ex)
        {
            // launch failed (e.g. executable not found)
            return ToolLaunchResult.Fail(ex.Message);
        }
    }
}
