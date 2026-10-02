/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.Threading;

namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// Shares the one system sleep request of <see cref="IShellProvider.PreventSleep"/> between its owners.
/// </summary>
public static class SleepGuard
{
    private static readonly Lock _lock = new();
    private static readonly HashSet<string> _owners = [];


    /// <summary>
    /// Gets whether any owner keeps the system awake.
    /// </summary>
    public static bool IsActive
    {
        get
        {
            lock (_lock)
            {
                return _owners.Count > 0;
            }
        }
    }


    /// <summary>
    /// Keeps the system and the display awake until <paramref name="owner"/> releases it.
    /// </summary>
    public static void Acquire(string owner, string reason)
    {
        lock (_lock)
        {
            var isFirstOwner = _owners.Count == 0;
            var isNewOwner = _owners.Add(owner);
            if (!isNewOwner) return;
            if (!isFirstOwner) return;

            Core.ShellProvider?.PreventSleep(reason);
        }
    }


    /// <summary>
    /// Releases the request of <paramref name="owner"/>; the system may sleep once no owner is left.
    /// </summary>
    public static void Release(string owner)
    {
        lock (_lock)
        {
            var isRemoved = _owners.Remove(owner);
            if (!isRemoved) return;

            var hasOtherOwners = _owners.Count > 0;
            if (hasOtherOwners) return;

            Core.ShellProvider?.AllowSleep();
        }
    }


    /// <summary>
    /// Releases the requests of every owner.
    /// </summary>
    public static void ReleaseAll()
    {
        lock (_lock)
        {
            var hasOwners = _owners.Count > 0;
            if (!hasOwners) return;

            _owners.Clear();
            Core.ShellProvider?.AllowSleep();
        }
    }
}
