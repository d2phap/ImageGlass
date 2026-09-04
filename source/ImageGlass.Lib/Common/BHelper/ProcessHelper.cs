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
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common;


public partial class BHelper
{
    private static readonly TaskFactory _taskFactory = new(
        CancellationToken.None, TaskCreationOptions.None,
        TaskContinuationOptions.None, TaskScheduler.Default);


    /// <summary>
    /// Whether running inside a Flatpak sandbox (host commands need <c>flatpak-spawn --host</c>).
    /// </summary>
    public static bool IsFlatpakSandbox { get; } = OS == OSType.Linux && File.Exists("/.flatpak-info");


    /// <summary>
    /// Whether running from an AppImage. The runtime exports <c>APPIMAGE</c> with the image path;
    /// only its presence is read, never its value.
    /// </summary>
    public static bool IsAppImage { get; } = OS == OSType.Linux
        && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPIMAGE"));


    /// <summary>
    /// In a Flatpak sandbox, rewrites <paramref name="psi"/> to launch via <c>flatpak-spawn --host</c>
    /// so host paths resolve (needs <c>--talk-name=org.freedesktop.Flatpak</c>); no-op otherwise.
    /// </summary>
    public static void ApplyFlatpakHostSpawn(ProcessStartInfo psi)
    {
        if (!IsFlatpakSandbox
            || psi.UseShellExecute
            || string.IsNullOrEmpty(psi.FileName)
            || psi.FileName == "flatpak-spawn")
        {
            return;
        }

        // the host cannot resolve our mounts, so an app-dir executable must be handed over as real;
        // arguments are passed through, so a tool sees the same paths whether it is integrated or not
        var hostArgs = new List<string>(psi.ArgumentList.Count + 2)
        {
            "--host",
            GetRealPlatformPath(psi.FileName),
        };
        hostArgs.AddRange(psi.ArgumentList);

        // Sandbox working dir is meaningless on the host; use the host default.
        psi.FileName = "flatpak-spawn";
        psi.WorkingDirectory = string.Empty;
        psi.ArgumentList.Clear();
        foreach (var arg in hostArgs) psi.ArgumentList.Add(arg);
    }


    /// <summary>
    /// Starts a process with the given command and arguments.
    /// </summary>
    public static void RunProcess(string fileName, string arguments)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = fileName;
        proc.StartInfo.Arguments = arguments;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
    }


    /// <summary>
    /// Runs a process and reads its standard output.
    /// </summary>
    public static string RunProcessAndReadOutput(string fileName, string arguments)
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return output;
        }
        catch
        {
            return string.Empty;
        }
    }


    /// <summary>
    /// Builds correct file path for executable and app protocol.
    /// </summary>
    public static (string Executable, string Args) BuildExeArgs(string executable, string arguments, string currentFilePath = "")
    {
        var exe = executable.Trim();
        var isAppProtocol = exe.EndsWith(':');

        // exclude the double quotes if the executable is app protocol
        var filePath = isAppProtocol ? currentFilePath : $"\"{currentFilePath}\"";

        var args = arguments.Replace(Const.FILE_MACRO, filePath);

        return (Executable: exe, Args: args);
    }


    /// <summary>
    /// Builds the executable and its argument list (tokenized, macro-substituted per-token);
    /// for an app protocol the args are the single URI tail appended to the scheme.
    /// </summary>
    public static (string Executable, List<string> Args) BuildExeArgList(string executable, string? arguments, string currentFilePath = "")
    {
        var exe = executable.Trim();

        // app protocol: the tail is an opaque URI remainder, not argv
        if (exe.EndsWith(':'))
        {
            var tail = (arguments ?? string.Empty).Replace(Const.FILE_MACRO, currentFilePath);
            var protocolArgs = new List<string>();
            if (tail.Length > 0) protocolArgs.Add(tail);
            return (exe, protocolArgs);
        }

        return (exe, BuildArgumentList(arguments, currentFilePath));
    }


    /// <summary>
    /// Tokenizes an args template (respecting double quotes) into individual arguments, then
    /// substitutes <see cref="Const.FILE_MACRO"/> per-token so a file path can't inject arguments.
    /// </summary>
    public static List<string> BuildArgumentList(string? argsTemplate, string filePath)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(argsTemplate)) return result;

        var token = new StringBuilder();
        var inQuotes = false;
        var hasToken = false;

        foreach (var ch in argsTemplate)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                hasToken = true; // an explicit "" is a real (empty) token
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(ch))
            {
                if (hasToken)
                {
                    result.Add(token.ToString().Replace(Const.FILE_MACRO, filePath));
                    token.Clear();
                    hasToken = false;
                }
                continue;
            }

            token.Append(ch);
            hasToken = true;
        }

        if (hasToken)
        {
            result.Add(token.ToString().Replace(Const.FILE_MACRO, filePath));
        }

        return result;
    }


    /// <summary>
    /// Runs an executable, auto-relaunching it elevated if it reports admin is required.
    /// </summary>
    public static async Task<IgExitCode> RunExeCmd(string exePath, IReadOnlyList<string>? args = null, bool waitForExit = true, bool showError = false)
    {
        try
        {
            var code = (IgExitCode)await RunExeAsync(exePath, args, asAdmin: false, waitForExit, showError);

            // elevation required -> retry as admin
            if (code == IgExitCode.AdminRequired)
            {
                code = (IgExitCode)await RunExeAsync(exePath, args, asAdmin: true, waitForExit);
            }

            return code;
        }
        catch
        {
            return IgExitCode.Error;
        }
    }


    /// <summary>
    /// Runs an executable or app protocol, with optional cross-platform elevation. Args go through
    /// <see cref="ProcessStartInfo.ArgumentList"/> so a crafted argument can't inject tokens.
    /// </summary>
    /// <param name="path">Executable path, or app protocol ending with <c>:</c>.</param>
    /// <param name="args">Individual arguments; for a protocol, concatenated onto the scheme.</param>
    /// <param name="asAdmin">Run elevated: UAC / osascript / pkexec.</param>
    /// <param name="waitForExit">Wait for exit and return the exit code.</param>
    /// <param name="showError">Show the OS error dialog on launch failure (shell-execute only).</param>
    public static async Task<int> RunExeAsync(string path, IReadOnlyList<string>? args = null, bool asAdmin = false, bool waitForExit = false, bool showError = false)
    {
        using var proc = new Process();
        ConfigureExeStart(proc.StartInfo, path.Trim(), args ?? [], asAdmin);
        proc.StartInfo.ErrorDialog = showError && proc.StartInfo.UseShellExecute;

        try
        {
            proc.Start();

            if (waitForExit)
            {
                await proc.WaitForExitAsync();
                return proc.ExitCode;
            }

            return (int)IgExitCode.Done;
        }
        catch (Win32Exception ex)
        {
            return (int)(ex.NativeErrorCode switch
            {
                2 => IgExitCode.Error_FileNotFound,     // ERROR_FILE_NOT_FOUND
                740 => IgExitCode.AdminRequired,        // ERROR_ELEVATION_REQUIRED
                _ => IgExitCode.Error,
            });
        }
        catch
        {
            return (int)IgExitCode.Error;
        }
    }


    /// <summary>
    /// Configures <paramref name="psi"/> for a protocol, normal, or elevated launch per platform.
    /// </summary>
    private static void ConfigureExeStart(ProcessStartInfo psi, string path, IReadOnlyList<string> args, bool asAdmin)
    {
        // app protocol: the whole URI is the FileName; never elevated
        if (path.EndsWith(':'))
        {
            psi.FileName = $"{path}{string.Concat(args)}";
            psi.UseShellExecute = true;
            return;
        }

        // non-elevated: shell-execute so associated apps and file verbs resolve
        if (!asAdmin)
        {
            psi.FileName = path;
            psi.UseShellExecute = true;
            AddArgs(psi, args);
            return;
        }

        // elevated launch via each platform's native admin prompt
        switch (BHelper.OS)
        {
            case OSType.Mac:
                // `do shell script` runs one /bin/sh string; single-quote each token
                var macCmd = string.Join(" ", args.Prepend(path).Select(ShellQuote));
                psi.FileName = "osascript";
                psi.UseShellExecute = false;
                psi.ArgumentList.Add("-e");
                psi.ArgumentList.Add($"do shell script \"{EscapeAppleScript(macCmd)}\" with administrator privileges");
                break;

            case OSType.Linux:
                // PolicyKit runs the program directly (no shell); each arg is its own token
                psi.FileName = "pkexec";
                psi.UseShellExecute = false;
                psi.ArgumentList.Add(path);
                AddArgs(psi, args);
                break;

            default: // Windows
                psi.FileName = path;
                psi.UseShellExecute = true;
                psi.Verb = "runas"; // triggers the UAC prompt
                AddArgs(psi, args);
                break;
        }
    }


    /// <summary>
    /// Appends each argument to the process <see cref="ProcessStartInfo.ArgumentList"/>.
    /// </summary>
    private static void AddArgs(ProcessStartInfo psi, IReadOnlyList<string> args)
    {
        foreach (var arg in args) psi.ArgumentList.Add(arg);
    }


    /// <summary>
    /// Wraps <paramref name="s"/> in POSIX single quotes for /bin/sh.
    /// </summary>
    private static string ShellQuote(string s) => $"'{s.Replace("'", "'\\''")}'";


    /// <summary>
    /// Escapes <paramref name="s"/> for embedding inside an AppleScript double-quoted literal.
    /// </summary>
    private static string EscapeAppleScript(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");


    /// <summary>
    /// Runs an async function synchronous in a new thread.
    /// Source: <see href="https://github.com/aspnet/AspNetIdentity/blob/b7826741279450c58b230ece98bd04b4815beabf/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs" />
    /// </summary>
    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;

        return _taskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Runs an async function synchronous in a new thread.
    /// Source: <see href="https://github.com/aspnet/AspNetIdentity/blob/b7826741279450c58b230ece98bd04b4815beabf/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs" />
    /// </summary>
    public static void RunSync(Func<Task> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;

        _taskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Returns <c>true</c> if another instance of this app (besides the current process) is running.
    /// </summary>
    public static bool HasOtherInstances()
    {
        try
        {
            using var current = Process.GetCurrentProcess();
            var procs = Process.GetProcessesByName(current.ProcessName);

            var hasOther = false;
            foreach (var proc in procs)
            {
                if (proc.Id != current.Id) hasOther = true;
                proc.Dispose();
            }

            return hasOther;
        }
        catch { return false; }
    }


    /// <summary>
    /// Terminates all other running instances of this app, keeping the current process alive.
    /// </summary>
    public static void CloseOtherInstances()
    {
        try
        {
            using var current = Process.GetCurrentProcess();
            foreach (var proc in Process.GetProcessesByName(current.ProcessName))
            {
                try
                {
                    if (proc.Id != current.Id) proc.Kill();
                }
                catch { }
                finally { proc.Dispose(); }
            }
        }
        catch { }
    }


    /// <summary>
    /// Restarts the app: releases the single-instance mutex so a fresh instance can take ownership,
    /// launches it, then exits the current process.
    /// </summary>
    /// <param name="suppressQuickSetup">
    /// Pass <c>true</c> when restarting out of the Quick Setup wizard so the fresh instance skips
    /// the forced wizard for that launch (prevents an admin-locked version from looping).
    /// </param>
    public static void RestartApp(bool suppressQuickSetup = false)
    {
        // release the single-instance lock; otherwise the new instance would just forward to this
        // (exiting) one and quit, leaving no window
        Core.AppInstance.Dispose();

        IReadOnlyList<string> args = suppressQuickSetup ? [AppCmds.NO_QUICK_SETUP] : [];
        _ = RunExeAsync(AppRelaunchPath, args);
        ExitApp(false);
    }


    /// <summary>
    /// Exits the app.
    /// </summary>
    public static void ExitApp(bool forced, int exitCode = 0)
    {
        // force exit
        if (forced)
        {
            Environment.Exit(exitCode);
            return;
        }

        var appLf = Application.Current?.ApplicationLifetime;

        // try to exit the app
        if (appLf is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _ = desktop.TryShutdown(exitCode);
        }
    }

}
