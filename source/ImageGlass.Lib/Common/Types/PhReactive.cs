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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace ImageGlass.Common.Types;


/// <summary>
/// Provides a base implementation of <see cref="INotifyPropertyChanged"/> interface.
/// </summary>
public partial class PhReactive : INotifyPropertyChanged
{
    #region INotifyPropertyChanged Implementation

    // to manage PropertyChanged events
    private readonly Lock _eventLock = new();
    private List<PropertyChangedEventHandler> _propertyChangedEvents = [];
    private event PropertyChangedEventHandler? _propertyChanged;


    #region IgReactive > Properties & Events

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            if (value is not null)
            {
                lock (_eventLock)
                {
                    _propertyChanged += value;
                    _propertyChangedEvents.Add(value);
                }
            }
        }
        remove
        {
            if (value is not null)
            {
                lock (_eventLock)
                {
                    _propertyChanged -= value;
                    _propertyChangedEvents.Remove(value);
                }
            }
        }
    }


    /// <summary>
    /// Suspends the <see cref="PropertyChanged"/> event.
    /// </summary>
    [JsonIgnore]
    public bool SuspendReactivity { get; set; } = false;

    #endregion // IgReactive > Properties & Events


    #region IgReactive > Methods

    /// <summary>
    /// Raises event <see cref="PropertyChanged"/>,
    /// returns <c>False</c> if the event is suspended.
    /// </summary>
    public bool OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        return OnPropertyChanged(null, null, propertyName);
    }


    /// <summary>
    /// Raises event <see cref="PropertyChanged"/>,
    /// returns <c>False</c> if the event is suspended.
    /// </summary>
    public bool OnPropertyChanged(object? value, object? oldValue, [CallerMemberName] string? propertyName = null)
    {
        if (SuspendReactivity) return false;

        _propertyChanged?.Invoke(this, new ReactiveEventArgs(propertyName, value, oldValue));
        return true;
    }


    /// <summary>
    /// Clears event handlers list of <see cref="PropertyChanged"/>.
    /// </summary>
    public void CleanUpPropertyChangedEvents()
    {
        lock (_eventLock)
        {
            // remove PropertyChanged events
            foreach (var eventHandler in _propertyChangedEvents)
            {
                _propertyChanged -= eventHandler;
            }
            _propertyChangedEvents.Clear();
        }
    }


    /// <summary>
    /// Runs an action without triggering <see cref="PropertyChanged"/> event.
    /// </summary>
    public void WithNoReactive(Action fn)
    {
        SuspendReactivity = true;
        fn();
        SuspendReactivity = false;
    }

    #endregion IgReactive > Methods


    #endregion // INotifyPropertyChanged Implementation

}


public class ReactiveEventArgs : PropertyChangedEventArgs
{
    /// <summary>
    /// Checks if both <see cref="Value"/>
    /// and <see cref="OldValue"/> are not <c>null</c>.
    /// </summary>
    public bool HasValues => Value is not null && OldValue is not null;

    public object? Value { get; set; }
    public object? OldValue { get; set; }


    public ReactiveEventArgs(
        string? propertyName,
        object? value = null,
        object? oldValue = null) : base(propertyName)
    {
        Value = value;
        OldValue = oldValue;
    }
}
