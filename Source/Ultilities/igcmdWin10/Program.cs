/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using System;
using System.Threading.Tasks;

/// <summary>
/// Application to perform Windows 8/10 specific operations.
/// These require using Windows 8/10 DLLs which aren't available
/// on Windows 7. [... and would cause igcmd to crash on Win7 if
/// the references were added to that project]
/// </summary>
namespace igcmdWin10 {
    internal static class Program {
        [STAThread]
        private static int Main(string[] args) {
            var topcmd = args[0].ToLower().Trim();

            if (topcmd == "setlockimage") {
                var task = SetLockScreenImageAsync(args);
                task.Wait();

                return task.Result;
            }

            return 0;
        }

        /// <summary>
        /// Set lock screen image
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static async Task<int> SetLockScreenImageAsync(string[] args) {
            var imgPath = args[1];
            var result = await LockScreenImage.SetAsync(imgPath).ConfigureAwait(true);

            return result;
        }
    }
}
