/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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

namespace ImageGlass.Base {
    /// <summary>
    /// The directory name constants
    /// </summary>
    public static class Dir {
        /// <summary>
        /// Gets the Themes folder name
        /// </summary>
        public static string Themes { get; } = "Themes";

        /// <summary>
        /// Gets the Languages folder name
        /// </summary>
        public static string Languages { get; } = "Languages";

        /// <summary>
        /// Gets the temporary folder name
        /// </summary>
        public static string Temporary { get; } = "Temp";

#if DEBUG
        /// <summary>
        /// Logging should not be to the temporary folder, as it is deleted on shutdown
        /// </summary>
        public static string Log { get; } = "Log";
#endif

    }
}
