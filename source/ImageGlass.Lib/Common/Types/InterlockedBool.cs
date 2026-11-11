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
using System.Threading;

namespace ImageGlass.Common.Types;


// Source:
// https://github.com/jiripolasek/PowerToys/blob/3bfa0a0cf8f98a6b5d8c753331c0b35dc3f2a41a/src/modules/cmdpal/Core/Microsoft.CmdPal.Core.Common/Helpers/InterlockedBoolean.cs

/// <summary>
/// Thread-safe boolean using atomic operations.
/// </summary>
public struct InterlockedBool(bool initialValue = false)
{
    private int _value = initialValue ? 1 : 0;


    /// <summary>
    /// Gets the boolean value atomically.
    /// </summary>
    public bool Value => Volatile.Read(ref _value) == 1;


    /// <summary>
    /// Atomically sets value.
    /// </summary>
    /// <returns><c>true</c> if the value was changed.</returns>
    public bool Set(bool value)
    {
        if (value) return SetTrue();
        else return SetFalse();
    }


    /// <summary>
    /// Atomically sets to <c>true</c>.
    /// </summary>
    /// <returns><c>true</c> if the value was changed (was previously <c>false</c>).</returns>
    public bool SetTrue() => Interlocked.Exchange(ref _value, 1) == 0;


    /// <summary>
    /// Atomically sets to <c>false</c>.
    /// </summary>
    /// <returns><c>true</c> if the value was changed (was previously <c>true</c>).</returns>
    public bool SetFalse() => Interlocked.Exchange(ref _value, 0) == 1;


    /// <summary>
    /// Reads as <see cref="bool"/>.
    /// </summary>
    public static implicit operator bool(InterlockedBool b) => b.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
