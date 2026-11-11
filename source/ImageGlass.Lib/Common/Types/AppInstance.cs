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
using Avalonia.Threading;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Types;

public partial class AppInstance : PhDisposable
{
    private readonly string _id;
    private readonly string _pipeName;

    // Windows uses a named mutex; Unix cannot, so it locks a file instead (see TryLockFile)
    private readonly Mutex? _mutex;
    private readonly FileStream? _lockFile;


    /// <summary>
    /// Indicates whether this is the first instance of this app.
    /// </summary>
    public bool IsFirstInstance { get; }


    /// <summary>
    /// Event raised when arguments are received from successive instances.
    /// </summary>
    public event TEventHandler<AppInstance, InstanceInvokedEventArgs>? InstanceInvoked;


    public AppInstance(string id)
    {
        _id = id;

        // the Unix pipe path (/tmp/CoreFxPipe_*) is not user-scoped, so keep users apart by name
        _pipeName = OperatingSystem.IsWindows()
            ? id
            : $"{id}-{Environment.UserName}";

        if (OperatingSystem.IsWindows())
        {
            _mutex = new Mutex(true, id, out var isFirstInstance);
            IsFirstInstance = isFirstInstance;
        }
        else
        {
            IsFirstInstance = TryLockFile(_pipeName, out _lockFile);
        }

        // start the pipe server
        if (IsFirstInstance)
        {
            StartPipeServer();
        }
    }


    /// <summary>
    /// Takes an exclusive lock on a file next to the pipe socket, the Unix stand-in for the named
    /// mutex: NativeAOT has no cross-process named mutex there, so every launch would look first.
    /// </summary>
    /// <returns><c>true</c> if this process took the lock, i.e. it is the first instance.</returns>
    private static bool TryLockFile(string name, out FileStream? lockFile)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{name}.lock");

        try
        {
            // the OS drops this lock when the process dies, so a crash never blocks the next launch
            lockFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            lockFile = null;
            return false;
        }
        // an unusable lock path must not stop the app from starting, so claim the first instance
        catch
        {
            lockFile = null;
            return true;
        }
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        if (_mutex is not null && IsFirstInstance)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }

        // releases the lock; the empty file is left behind for the next instance to re-lock
        _lockFile?.Dispose();
    }


    /// <summary>
    /// Sends arguments to existing instances.
    /// </summary>
    /// <returns><c>false</c> if no existing instance took them, so the caller must start normally.</returns>
    public bool SendArgsToExistingInstances(string cmd, params string[] args)
    {
        // only send if this is NOT the first instance
        if (IsFirstInstance) return false;

        try
        {
            // create and try to connect to the first instance's pipe server,
            // timeout after 1000ms (1 second) if server isn't available
            using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
            client.Connect(1000);

            // send the argument count first so receiver knows how many lines to read
            using var writer = new StreamWriter(client);
            writer.WriteLine(args.Length + 1); // args count + cmd

            // send the command
            writer.WriteLine(cmd);

            // send each argument on a separate line
            foreach (var arg in args)
            {
                writer.WriteLine(arg);
            }

            writer.Flush();
            return true;
        }
        catch { return false; }
    }


    /// <summary>
    /// Starts a pipe server on a background thread.
    /// </summary>
    private void StartPipeServer()
    {
        _ = Task.Run(() => StartPipeServerAsync__());
    }


    /// <summary>
    /// Start a pipe server.
    /// </summary>
    private async Task StartPipeServerAsync__()
    {
        while (!IsDisposed)
        {
            try
            {
                // create pipe server & reader
                using var server = new NamedPipeServerStream(_pipeName,
                    PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                using var reader = new StreamReader(server);

                // wait for connection
                await server.WaitForConnectionAsync();


                // read first line which contains the number of arguments
                var countStr = await reader.ReadLineAsync();
                if (!int.TryParse(countStr, out var argsCount)) continue;


                // create array to hold the arguments
                var args = new string[argsCount - 1]; // exclude cmd (first args)
                var cmd = string.Empty;


                // read each argument line by line
                for (int i = 0; i < argsCount; i++)
                {
                    var text = await reader.ReadLineAsync() ?? string.Empty;

                    if (i == 0) cmd = text;
                    else args[i - 1] = text;
                }


                // trigger event
                Dispatcher.UIThread.Post(() =>
                {
                    InstanceInvoked?.Invoke(this, new InstanceInvokedEventArgs(_id, cmd, args));
                });
            }
            catch { }
        }
    }


}



/// <summary>
/// Holds a list of arguments given to an application at startup.
/// </summary>
public class InstanceInvokedEventArgs(string id, string cmd, string[] args) : EventArgs
{
    public string Id { get; set; } = id;
    public string Command { get; set; } = cmd;
    public string[] Arguments { get; set; } = args;
}

