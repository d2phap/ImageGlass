/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

// Source:
// https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs

using System.Diagnostics;
using System.Globalization;

namespace ImageGlass.Base;

public partial class Helpers
{
    private static readonly TaskFactory _myTaskFactory = new(
        CancellationToken.None, TaskCreationOptions.None,
        TaskContinuationOptions.None, TaskScheduler.Default);

    /// <summary>
    /// Runs an async function synchronous
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;

        return _myTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Runs an async function synchronous
    /// </summary>
    /// <param name="func"></param>
    public static void RunSync(Func<Task> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;

        _myTaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Runs a function in a new thread
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Thread RunAsThread(ThreadStart func, ApartmentState thState = ApartmentState.Unknown)
    {
        var th = new Thread(func)
        {
            IsBackground = true,
        };

        th.SetApartmentState(thState);
        th.Start();

        return th;
    }


    /// <summary>
    /// Runs as admin
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="args"></param>
    public static async Task<int> RunExeAsync(string filename, string args, bool asAdmin = false, bool waitForExit = false)
    {
        var proc = new Process();
        proc.StartInfo.FileName = filename;
        proc.StartInfo.Arguments = args;

        proc.StartInfo.Verb = asAdmin ? "runas" : "";
        proc.StartInfo.UseShellExecute = true;

        try
        {
            proc.Start();

            if (waitForExit)
            {
                await proc.WaitForExitAsync();
            }

            return proc.ExitCode;
        }
        catch
        {
            return (int)IgExitCode.Error;
        }
    }


    /// <summary>
    /// Run a command, supports auto-elevating process privilege
    /// if admin permission is required.
    /// </summary>
    /// <returns></returns>
    public static async Task<IgExitCode> RunExeCmd(string exePath, string args)
    {
        IgExitCode code;

        try
        {
            code = (IgExitCode)await RunExeAsync(exePath, args, waitForExit: true);


            // If that fails due to privs error, re-attempt with admin privs.
            if (code == IgExitCode.AdminRequired)
            {
                code = (IgExitCode)await RunExeAsync(exePath, args, asAdmin: true, waitForExit: true);
            }
        }
        catch
        {
            code = IgExitCode.Error;
        }

        return code;
    }


    /// <summary>
    /// Runs a command from <c>igcmd10.exe</c>, supports auto-elevating process privilege
    /// if admin permission is required.
    /// </summary>
    /// <returns></returns>
    public static async Task<IgExitCode> RunIgcmd(string args)
    {
        var exePath = App.StartUpDir("igcmd.exe");

        return await RunExeCmd(exePath, args);
    }


    /// <summary>
    /// Runs a command from <c>igcmd10.exe</c>, supports auto-elevating process privilege
    /// if admin permission is required.
    /// </summary>
    /// <returns></returns>
    public static async Task<IgExitCode> RunIgcmd10(string args)
    {
        var exePath = App.StartUpDir("igcmd10.exe");

        return await RunExeCmd(exePath, args);
    }
}
