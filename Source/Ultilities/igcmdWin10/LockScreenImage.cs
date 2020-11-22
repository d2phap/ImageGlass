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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;

namespace igcmdWin10 {
    /// <summary>
    /// Set the Lock Screen image from an image path
    /// </summary>
    public static class LockScreenImage {
        public static async Task<int> SetAsync(string path) {
            //System.Diagnostics.Debugger.Break();

            var folder = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);

            try {
                var sf = await StorageFolder.GetFolderFromPathAsync(folder);
                var imgFile = await sf.GetFileAsync(file);

                using (var stream = await imgFile.OpenAsync(FileAccessMode.Read)) {
                    await LockScreen.SetImageStreamAsync(stream);
                }
            }
            catch (Exception) {
                return 1;
            }

            return 0;
        }
    }
}
