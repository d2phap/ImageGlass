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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
namespace ImageMagick.IgCodec;

internal class Utils
{
    /// <summary>
    /// Get built-in color profiles
    /// </summary>
    /// <returns></returns>
    public static string[] GetBuiltInColorProfiles()
    {
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
    public static string GetCorrectColorProfileName(string name)
    {
        var profileName = "";

        if (File.Exists(name))
        {
            return name;
        }
        else
        {
            var builtInProfiles = GetBuiltInColorProfiles();
            var result = Array.Find(builtInProfiles, i => string.Equals(i, name, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                profileName = result;
            }
            else
            {
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
    public static ColorProfile? GetColorProfile(string name)
    {
        if (File.Exists(name))
        {
            return new ColorProfile(name);
        }
        else
        {
            // get all profile names in Magick.NET
            var profiles = typeof(ColorProfile).GetProperties();
            var result = Array.Find(profiles, i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                try
                {
                    return (ColorProfile?)result.GetValue(result);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        return null;
    }
}
