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
*/
using System.ComponentModel;

namespace ImageGlass.Base.QueuedWorker;


/// <summary>
/// Represents the method that will handle the RunWorkerCompleted event.
/// </summary>
/// <param name="sender">The object that is the source of the event.</param>
/// <param name="e">A <see cref="QueuedWorkerCompletedEventArgs"/> that contains event data.</param>
public delegate void RunQueuedWorkerCompletedEventHandler(object? sender, QueuedWorkerCompletedEventArgs e);



/// <summary>
/// Represents the event arguments of the RunWorkerCompleted event.
/// </summary>
public class QueuedWorkerCompletedEventArgs : AsyncCompletedEventArgs
{
    /// <summary>
    /// Gets a value that represents the result of an asynchronous operation.
    /// </summary>
    public object? Result { get; private set; }

    /// <summary>
    /// Gets the priority of this item.
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Initializes a new instance of the QueuedWorkerCompletedEventArgs class.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    /// <param name="result">The result of an asynchronous operation.</param>
    /// <param name="priority">A value between 0 and 5 indicating the priority of this item.</param>
    /// <param name="error">The error that occurred while loading the image.</param>
    /// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
    public QueuedWorkerCompletedEventArgs(object argument, object? result, int priority, Exception? error, bool cancelled)
        : base(error, cancelled, argument)
    {
        Result = result;
        Priority = priority;
    }
}
