/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

---------------------
This source code is based on Christopher Morgan's NamedPipes project:
Url: https://www.codeproject.com/Articles/810030/IPC-with-Named-Pipes
License: CPOL, http://www.codeproject.com/info/cpol10.aspx
---------------------
*/
using System.IO.Pipes;
using System.Text;

namespace ImageGlass.Tools;


/// <summary>
/// Asynchronous state for the pipe server.
/// </summary>
internal class PipeServerState
{

    private const int BufferSize = 8125;


    #region Public Properties

    /// <summary>
    /// Gets the byte buffer.
    /// </summary>
    public byte[] Buffer { get; private set; }

    /// <summary>
    /// Gets the pipe server.
    /// </summary>
    public NamedPipeServerStream PipeServer { get; private set; }

    /// <summary>
    /// The external cancellation token.
    /// </summary>
    public CancellationToken ExternalCancellationToken { get; private set; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public StringBuilder Message { get; private set; }

    #endregion


    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeServerState"/> class.
    /// </summary>
    /// <param name="pipeServer">The pipe server instance.</param>
    /// <param name="token">A token referenced by and external cancellation token.</param>
    public PipeServerState(NamedPipeServerStream pipeServer, CancellationToken token)
        : this(pipeServer, new byte[BufferSize], token)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeServerState"/> class.
    /// </summary>
    /// <param name="pipeServer">The pipe server instance.</param>
    /// <param name="buffer">The byte buffer.</param>
    /// <param name="token">A token referenced by and external cancellation token.</param>
    public PipeServerState(NamedPipeServerStream pipeServer, byte[] buffer, CancellationToken token)
    {
        this.PipeServer = pipeServer;
        this.Buffer = buffer;
        this.ExternalCancellationToken = token;
        this.Message = new StringBuilder();
    }

    #endregion

}