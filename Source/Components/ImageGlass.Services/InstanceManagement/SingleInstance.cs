/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Services.InstanceManagement {
    /// <summary>
    /// Enforces single instance for an application.
    /// </summary>
    public class SingleInstance: IDisposable {
        private Mutex mutex;
        private readonly bool ownsMutex;
        private Guid identifier = Guid.Empty;

        /// <summary>
        /// Enforces single instance for an application.
        /// </summary>
        /// <param name="identifier">An identifier unique to this application.</param>
        public SingleInstance(Guid identifier) {
            this.identifier = identifier;
            mutex = new Mutex(true, identifier.ToString(), out ownsMutex);
        }

        /// <summary>
        /// Indicates whether this is the first instance of this application.
        /// </summary>
        public bool IsFirstInstance => ownsMutex;

        /// <summary>
        /// Passes the given arguments to the first running instance of the application.
        /// </summary>
        /// <param name="arguments">The arguments to pass.</param>
        /// <returns>Return true if the operation succeded, false otherwise.</returns>
        public async Task<bool> PassArgumentsToFirstInstanceAsync(string[] arguments) {
            if (IsFirstInstance)
                throw new InvalidOperationException("This is the first instance.");

            try {
                using (var client = new NamedPipeClientStream(identifier.ToString()))
                using (var writer = new StreamWriter(client)) {
                    await client.ConnectAsync(200).ConfigureAwait(true);
                    foreach (var argument in arguments)
                        await writer.WriteLineAsync(argument).ConfigureAwait(true);
                }
                return true;
            }
            catch (TimeoutException) { } //Couldn't connect to server
            catch (IOException) { } //Pipe was broken

            return false;
        }

        /// <summary>
        /// Listens for arguments being passed from successive instances of the applicaiton.
        /// </summary>
        public void ListenForArgumentsFromSuccessiveInstances() {
            if (!IsFirstInstance)
                throw new InvalidOperationException("This is not the first instance.");
            ThreadPool.QueueUserWorkItem(new WaitCallback(ListenForArguments));
        }

        /// <summary>
        /// Listens for arguments on a named pipe.
        /// </summary>
        /// <param name="state">State object required by WaitCallback delegate.</param>
        private void ListenForArguments(object state) {
            try {
                using var server = new NamedPipeServerStream(identifier.ToString());
                using var reader = new StreamReader(server);
                server.WaitForConnection();

                var arguments = new List<string>();
                while (server.IsConnected)
                    arguments.Add(reader.ReadLine());

                ThreadPool.QueueUserWorkItem(new WaitCallback(CallOnArgumentsReceived), arguments.ToArray());
            }
            catch (IOException) { } //Pipe was broken
            finally {
                ListenForArguments(null);
            }
        }

        /// <summary>
        /// Calls the OnArgumentsReceived method casting the state Object to String[].
        /// </summary>
        /// <param name="state">The arguments to pass.</param>
        private void CallOnArgumentsReceived(object state) {
            OnArgumentsReceived((string[])state);
        }

        /// <summary>
        /// Event raised when arguments are received from successive instances.
        /// </summary>
        public event EventHandler<ArgumentsReceivedEventArgs> ArgumentsReceived;

        /// <summary>
        /// Fires the ArgumentsReceived event.
        /// </summary>
        /// <param name="arguments">The arguments to pass with the ArgumentsReceivedEventArgs.</param>
        private void OnArgumentsReceived(string[] arguments) {
            ArgumentsReceived?.Invoke(this, new ArgumentsReceivedEventArgs() { Args = arguments });
        }

        #region IDisposable
        private bool disposed;

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (mutex != null && ownsMutex) {
                    mutex.ReleaseMutex();
                    mutex = null;
                }
                disposed = true;
            }
        }

        ~SingleInstance() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
