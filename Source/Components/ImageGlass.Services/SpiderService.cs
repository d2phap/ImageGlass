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
        private const string SDK_DLL = "bs-sdk.dll";

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


        /// <summary>
        /// Initializes Blueswan service.
        /// </summary>
        public static void Initialize() {
            SetApp(new(APP_ID));
        }

        /// <summary>
        /// Starts Blueswan service.
        /// </summary>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Start();

        /// <summary>
        /// Stops Blueswan service.
        /// </summary>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Stop();

        /// <summary>
        /// Checks if Blueswan service is started.
        /// </summary>
        /// <returns></returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsStarted();

        /// <summary>
        /// Checks if Blueswan service is updated.
        /// </summary>
        /// <returns></returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsUpdated();

        /// <summary>
        /// Checks if Blueswan service is running.
        /// </summary>
        /// <returns></returns>
        [DllImport(SDK_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint IsRunning();

    }
}
