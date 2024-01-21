
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

/******************************************
* THANKS [Meowski] FOR THIS CONTRIBUTION
*******************************************/

using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Media;

namespace ImageGlass.Base.WinApi;

/// <summary>
/// Used to make requests for obtaining and setting timer resolution.
/// This is a global request shared by all processes on the computer. All
/// requests are revoked when this process ends.
/// </summary>
public static class TimerApi
{
    // locks ourCurRequests
    private static readonly object _lock = new();

    private static readonly uint _minPeriod;
    private static readonly uint _maxPeriod;
    private static readonly List<int> _curRequests;


    static TimerApi()
    {
        var tc = new TIMECAPS();
        PInvoke.timeGetDevCaps(out tc, (uint)Marshal.SizeOf(tc));

        _minPeriod = tc.wPeriodMin;
        _maxPeriod = tc.wPeriodMax;
        _curRequests = [];
    }


    /// <summary>
    /// Request a rate from the system clock.
    /// </summary>
    /// <param name="timeInMilliseconds"> the time in milliseconds </param>
    /// <returns> true if we succesfully acquired a clock of
    /// the given rate, otherwise returns false. </returns>
    public static bool TimeBeginPeriod(int timeInMilliseconds)
    {
        if (timeInMilliseconds < _minPeriod || timeInMilliseconds > _maxPeriod)
        {
            return false;
        }

        bool successfullyRequestedPeriod;
        lock (_lock)
        {
            successfullyRequestedPeriod = PInvoke.timeBeginPeriod((uint)timeInMilliseconds) == 0;
            if (successfullyRequestedPeriod)
            {
                _curRequests.Add(timeInMilliseconds);
            }
        }

        return successfullyRequestedPeriod;
    }


    /// <summary>
    /// Revoke request for a rate from the system clock.
    /// </summary>
    /// <param name="timeInMilliseconds"> the time in milliseconds </param>
    /// <returns>true if we revoked a previous request, otherwise returns false</returns>
    public static bool TimeEndPeriod(int timeInMilliseconds)
    {
        bool successfullyEndedPeriod;
        lock (_lock)
        {
            successfullyEndedPeriod = _curRequests.Remove(timeInMilliseconds)
                && PInvoke.timeEndPeriod((uint)timeInMilliseconds) == 0;
        }

        return successfullyEndedPeriod;
    }


    /// <summary>
    /// Determines whether the current rate has already been requested.
    /// </summary>
    /// <param name="timeInMilliseconds"> the time in milliseconds </param>
    public static bool HasRequestedRateAlready(int timeInMilliseconds)
    {
        bool hasRequestedAlready;
        lock (_lock)
        {
            hasRequestedAlready = _curRequests.Contains(timeInMilliseconds);
        }

        return hasRequestedAlready;
    }


    /// <summary>
    /// Determines whether a rate at least as fast as the given has been requested
    /// </summary>
    /// <param name="timeInMilliseconds">the time in milliseconds</param>
    public static bool HasRequestedRateAtLeastAsFastAs(int timeInMilliseconds)
    {
        bool result;
        lock (_lock)
        {
            result = _curRequests.Exists(elt => elt <= timeInMilliseconds);
        }

        return result;
    }
}