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
using System;
using System.ComponentModel;

namespace ImageGlass.Common.Types;


/// <summary>
/// Provides a base implementation of <see cref="IDisposable"/>
/// and <see cref="INotifyPropertyChanged"/> interface,
/// including support for managed and unmanaged resource cleanup.
/// </summary>
public partial class PhDisposable : PhReactive, IDisposable
{
    #region IDisposable Disposing

    protected InterlockedBool _isDisposed = new(false);


    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;

        if (disposing)
        {
            // Free any other managed objects here.
            OnDisposing();

            // remove PropertyChanged events
            CleanUpPropertyChangedEvents();
        }

        // Free any unmanaged objects here.
        _isDisposed.SetTrue();
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PhDisposable()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Releases the managed objects.
    /// </summary>
    protected virtual void OnDisposing()
    {
        //
    }

}

