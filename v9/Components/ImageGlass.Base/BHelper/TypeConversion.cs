/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
namespace ImageGlass.Base;

public partial class BHelper
{
    /// <summary>
    /// Convert the given object to Enum type
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    /// <param name="value">Value</param>
    /// <returns></returns>
    public static T ParseEnum<T>(object value)
    {
        return (T)Enum.Parse(typeof(T), value.ToString(), true);
    }


    /// <summary>
    /// Convert the given value to specific type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="value">Value</param>
    /// <returns>
    /// Returns <c>default</c> value of the type if the <paramref name="value"/>
    /// is not convertable or null.
    /// </returns>
    public static T? ConvertType<T>(object? value, T? defaultValue = default)
    {
        if (value == null) return defaultValue;
        var type = typeof(T);

        try
        {
            if (type.IsEnum)
            {
                return ParseEnum<T>(value);
            }

            return (T)Convert.ChangeType(value, type);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }
}
