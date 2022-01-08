/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.Runtime.InteropServices;


namespace ImageGlass.Services {
    public class SpiderService {

        private const string APP_ID = "org.imageglass.app";
        public const string SDK_DLL = "bs-sdk.dll";

        /// <summary>
        /// Golang string structure
        /// </summary>
        private struct GoString {
            public string p;
            public int n;

            public GoString(string id) {
                p = id;
                n = id.Length;
            }
        }


        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetApp(GoString appId);


        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Start();


        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Stop();



        /// <summary>
        /// Enables Blueswan service.
        /// </summary>
        public static void Enable() {
            try {
                SetApp(new(APP_ID));
                Start();

            }
            catch { }
        }

        /// <summary>
        /// Disables Blueswan service.
        /// </summary>
        public static void Disable() {
            try {
                Stop();
            }
            catch { }
        }

        /// <summary>
        /// Checks if Blueswan service is started.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><c>1</c> the service is started</item>
        ///   <item><c>0</c> the service is not started</item>
        /// </list>
        /// </returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsStarted();

        /// <summary>
        /// Checks if Blueswan service is updated.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><c>1</c> the service is updated</item>
        ///   <item><c>0</c> the service is not updated</item>
        /// </list>
        /// </returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsUpdated();

        /// <summary>
        /// Checks if Blueswan service is running.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><c>1</c> the service is running</item>
        ///   <item><c>0</c> the service is not running</item>
        /// </list>
        /// </returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsRunning();

    }
}
