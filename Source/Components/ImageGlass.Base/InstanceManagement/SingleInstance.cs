/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.IO.Pipes;

namespace ImageGlass.Base.InstanceManagement;

/// <summary>
/// Enforces single instance for an application.
/// </summary>
public class SingleInstance : IDisposable
{

    #region IDisposable
    private bool disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (_mutex != null && _ownsMutex)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
            }
            disposed = true;
        }
    }

    ~SingleInstance() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion


    private Mutex? _mutex;
    private readonly bool _ownsMutex;
    private readonly string _id = string.Empty;


    /// <summary>
    /// Indicates whether this is the first instance of this application.
    /// </summary>
    public bool IsFirstInstance => _ownsMutex;


    /// <summary>
    /// Event raised when arguments are received from successive instances.
    /// </summary>
    public event EventHandler<ArgsReceivedEventArgs>? ArgsReceived;


    /// <summary>
    /// Enforces single instance for an application.
    /// </summary>
    /// <param name="id">An identifier unique to this application.</param>
    public SingleInstance(string id)
    {
        _id = id;
        _mutex = new Mutex(true, id, out _ownsMutex);
    }


    /// <summary>
    /// Passes the given arguments to the first running instance of the application.
    /// </summary>
    /// <param name="args">The arguments to pass.</param>
    /// <returns>Return true if the operation succeded, false otherwise.</returns>
    public async Task<bool> PassArgsToFirstInstanceAsync(string[] args)
    {
        // cannot pass to itself
        if (IsFirstInstance) return false;

        try
        {
            using var client = new NamedPipeClientStream(_id);
            using var writer = new StreamWriter(client);

            await client.ConnectAsync(200).ConfigureAwait(false);
            foreach (var arg in args)
            {
                await writer.WriteLineAsync(arg).ConfigureAwait(false);
            }

            return true;
        }
        catch (TimeoutException) { } // Couldn't connect to server
        catch (IOException) { } // Pipe was broken

        return false;
    }


    /// <summary>
    /// Listens for arguments being passed from successive instances of the application.
    /// </summary>
    public void ListenForArgsFromChildInstances()
    {
        // only the first instance can listen to its child instances
        if (!IsFirstInstance) return;

        ThreadPool.QueueUserWorkItem(new WaitCallback(ListenForArgs));
    }


    /// <summary>
    /// Listens for arguments on a named pipe.
    /// </summary>
    /// <param name="state">
    /// State object required by <see cref="WaitCallback"/> delegate.
    /// </param>
    private void ListenForArgs(object? state)
    {
        try
        {
            using var server = new NamedPipeServerStream(_id);
            using var reader = new StreamReader(server);
            server.WaitForConnection();

            var args = new List<string>();
            while (server.IsConnected)
            {
                var arg = reader.ReadLine();

                if (arg != null)
                {
                    args.Add(arg);
                }
            }

            ThreadPool.QueueUserWorkItem(CallOnArgsReceived, args.ToArray());
        }
        catch (IOException) { } // Pipe was broken
        finally
        {
            ListenForArgs(null);
        }
    }


    /// <summary>
    /// Calls the <see cref="OnArgumentsReceived"/> method casting
    /// the state <see cref="object"/> to <see cref="string[]"/>.
    /// </summary>
    /// <param name="state">The arguments to pass.</param>
    private void CallOnArgsReceived(object? state)
    {
        OnArgumentsReceived((string[]?)state);
    }


    /// <summary>
    /// Fires the <see cref="ArgsReceived"/> event.
    /// </summary>
    /// <param name="args">
    /// The arguments to pass with the <see cref="ArgsReceivedEventArgs"/>.
    /// </param>
    private void OnArgumentsReceived(string[]? args)
    {
        ArgsReceived?.Invoke(this, new()
        {
            Arguments = args ?? [],
        });
    }

}
