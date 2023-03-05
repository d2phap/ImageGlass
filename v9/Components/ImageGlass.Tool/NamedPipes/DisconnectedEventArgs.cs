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

namespace ImageGlass.Tools;


/// <summary>
/// Message received event arguments.
/// </summary>
public class DisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the pipe.
    /// </summary>
    public string PipeName { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="DisconnectedEventArgs"/> class.
    /// </summary>
    public DisconnectedEventArgs(string pipeName)
    {
        PipeName = pipeName;
    }
}