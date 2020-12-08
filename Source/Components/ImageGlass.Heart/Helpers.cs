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
using ImageMagick;

namespace ImageGlass.Heart {
    public static class Helpers {
        /// <summary>
        /// Get built-in color profiles
        /// </summary>
        /// <returns></returns>
        public static string[] GetBuiltInColorProfiles() {
            return new string[]
            {
                "AdobeRGB1998",
                "AppleRGB",
                "CoatedFOGRA39",
                "ColorMatchRGB",
                "sRGB",
                "USWebCoatedSWOP",
            };
        }

        /// <summary>
        /// Get the correct color profile name
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        public static string GetCorrectColorProfileName(string name) {
            var profileName = "";

            if (File.Exists(name)) {
                return name;
            }
            else {
                var builtInProfiles = GetBuiltInColorProfiles();
                var result = Array.Find(builtInProfiles, i => string.Equals(i, name, StringComparison.InvariantCultureIgnoreCase));

                if (result != null) {
                    profileName = result;
                }
                else {
                    return string.Empty;
                }
            }

            return profileName;
        }

        /// <summary>
        /// Get ColorProfile
        /// </summary>
        /// <param name="name">Name or Full path of color profile</param>
        /// <returns></returns>
        public static ColorProfile GetColorProfile(string name) {
            if (File.Exists(name)) {
                return new ColorProfile(name);
            }
            else {
                // get all profile names in Magick.NET
                var profiles = typeof(ColorProfile).GetProperties();
                var result = Array.Find(profiles, i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase));

                if (result != null) {
                    try {
                        return (ColorProfile)result.GetValue(result);
                    }
                    catch (Exception) {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns Exif rotation in degrees. Returns 0 if the metadata
        /// does not exist or could not be read. A negative value means
        /// the image needs to be mirrored about the vertical axis.
        /// </summary>
        /// <param name="orientationFlag">Orientation Flag</param>
        /// <returns></returns>
        public static int GetOrientationDegree(int orientationFlag) {
            if (orientationFlag == 1)
                return 0;
            else if (orientationFlag == 2)
                return -360;
            else if (orientationFlag == 3)
                return 180;
            else if (orientationFlag == 4)
                return -180;
            else if (orientationFlag == 5)
                return -90;
            else if (orientationFlag == 6)
                return 90;
            else if (orientationFlag == 7)
                return -270;
            else if (orientationFlag == 8)
                return 270;

            return 0;
        }

        private const string LONG_PATH_PREFIX = @"\\?\";

        /// <summary>
        /// Fallout from Issue #530. To handle a long path name (i.e. a file path
        /// longer than MAX_PATH), a magic prefix is sometimes necessary.
        /// </summary>
        public static string PrefixLongPath(string path) {
            if (path.Length > 255 && !path.StartsWith(LONG_PATH_PREFIX))
                return LONG_PATH_PREFIX + path;
            return path;
        }

        /// <summary>
        /// Fallout from Issue #530. Specific functions (currently FileWatch)
        /// fail if provided a prefixed file path. In this case, strip the prefix
        /// (see PrefixLongPath above).
        /// </summary>
        public static string DePrefixLongPath(string path) {
            if (path.StartsWith(LONG_PATH_PREFIX))
                return path.Substring(LONG_PATH_PREFIX.Length);
            return path;
        }
    }
}
