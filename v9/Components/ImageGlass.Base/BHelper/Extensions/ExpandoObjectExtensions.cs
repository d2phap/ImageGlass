﻿/*
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

using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;

namespace ImageGlass.Base;

public static class ExpandoObjectExtensions
{
    /// <summary>
    /// Gets value from the <see cref="ExpandoObject"/>.
    /// </summary>
    public static T GetValue<T>(this ExpandoObject expObj, string keyName, T defaultValue)
    {
        var dict = (IDictionary<string, object>)(expObj as dynamic);

        try
        {
            _ = dict.TryGetValue(keyName, out var result);

            return BHelper.ConvertType<T>(result) ?? defaultValue;
        }
        catch (RuntimeBinderException) { }

        return defaultValue;
    }


    /// <summary>
    /// Gets value from the <see cref="ExpandoObject"/>.
    /// </summary>
    public static object? GetValue(this ExpandoObject expObj, string keyName)
    {
        var dict = (IDictionary<string, object>)(expObj as dynamic);

        try
        {
            _ = dict.TryGetValue(keyName, out var result);

            return result;
        }
        catch (RuntimeBinderException) { }

        return null;
    }


    /// <summary>
    /// Removes an item in <see cref="ExpandoObject"/>.
    /// </summary>
    public static bool Remove(this ExpandoObject expObj, string keyName)
    {
        var dict = expObj as dynamic as Dictionary<string, object>;

        return dict.Remove(keyName);
    }


    /// <summary>
    /// Sets value to <see cref="ExpandoObject"/>.
    /// </summary>
    public static void Set(this ExpandoObject expObj, string keyName, object value)
    {
        var dict = expObj as dynamic as IDictionary<string, object?>;

        if (dict.ContainsKey(keyName))
        {
            dict[keyName] = value;
        }
        else
        {
            dict.TryAdd(keyName, value);
        }
    }
}