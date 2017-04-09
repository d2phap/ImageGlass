using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.WinAPI {
    /// <summary>
    /// Used to make requests for obtaining and setting timer resolution. 
    /// This is a global request shared by all processes on the computer. All
    /// requests are revoked when this process ends.
    /// </summary>
    public static class TimerAPI {
        // locks ourCurRequests
        //
        private static readonly object ourLock;

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TIMECAPS ptc, int cbtc);

        private static readonly int ourMinPeriod;
        private static readonly int ourMaxPeriod;
        private static readonly List<int> ourCurRequests;

        [StructLayout(LayoutKind.Sequential)]
        private struct TIMECAPS {
            public int periodMin;
            public int periodMax;
        }

        static TimerAPI() {
            ourLock = new object();

            TIMECAPS tc = new TIMECAPS();
            timeGetDevCaps(ref tc, Marshal.SizeOf(tc));
            ourMinPeriod = tc.periodMin;
            ourMaxPeriod = tc.periodMax;
            ourCurRequests = new List<int>();
        }

        /// <summary>
        /// Request a rate from the system clock.
        /// </summary>
        /// <param name="timeInMilliseconds"> the time in milliseconds </param>
        /// <returns> true if we succesfully acquired a clock of
        /// the given rate, otherwise returns false. </returns>
        public static bool TimeBeginPeriod(int timeInMilliseconds) {
            if (timeInMilliseconds < ourMinPeriod || timeInMilliseconds > ourMaxPeriod)
                return false;

            bool successfullyRequestedPeriod;
            lock (ourLock) {
                successfullyRequestedPeriod = timeBeginPeriod(timeInMilliseconds) == 0;
                if (successfullyRequestedPeriod)
                    ourCurRequests.Add(timeInMilliseconds);
            }

            return successfullyRequestedPeriod;
        }

        /// <summary>
        /// Revoke request for a rate from the system clock.
        /// </summary>
        /// <param name="timeInMilliseconds"> the time in milliseconds </param>
        /// <returns>true if we revoked a previous request, otherwise returns false</returns>
        public static bool TimeEndPeriod(int timeInMilliseconds) {
            bool successfullyEndedPeriod;
            lock (ourLock) {
                successfullyEndedPeriod = ourCurRequests.Remove(timeInMilliseconds) && timeEndPeriod(timeInMilliseconds) == 0;
            }

            return successfullyEndedPeriod;
        }

        /// <summary>
        /// Determines whether the current rate has already been requested.
        /// </summary>
        /// <param name="timeInMilliseconds"> the time in milliseconds </param>
        public static bool HasRequestedRateAlready(int timeInMilliseconds) {
            bool hasRequestedAlready;
            lock (ourLock) {
                hasRequestedAlready = ourCurRequests.Contains(timeInMilliseconds);
            }

            return hasRequestedAlready;
        }

        /// <summary>
        /// Determines whether a rate at least as fast as the given has been requested
        /// </summary>
        /// <param name="timeInMilliseconds">the time in milliseconds</param>
        public static bool HasRequestedRateAtLeastAsFastAs(int timeInMilliseconds) {
            bool result;
            lock (ourLock) {
                result = ourCurRequests.Exists(elt => elt <= timeInMilliseconds);
            }

            return result;
        }
    }
}