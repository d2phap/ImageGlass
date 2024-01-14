/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using Microsoft.Extensions.Configuration;
using System.Dynamic;

namespace ImageGlass;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets config section as dynamic object.
    /// </summary>
    public static ExpandoObject? GetValueObj(this IConfiguration config, string keyName)
    {
        var result = new ExpandoObject();

        // retrieve all keys from your settings
        var configs = config.AsEnumerable().Where(_ => _.Key.StartsWith(keyName));
        if (!configs.Any()) return null;

        foreach (var kvp in configs)
        {
            var parent = result as IDictionary<string, object>;
            var path = kvp.Key.Split(':');

            // create or retrieve the hierarchy (keep last path item for later)
            var i = 0;
            for (i = 0; i < path.Length - 1; i++)
            {
                if (!parent.ContainsKey(path[i]))
                {
                    parent.Add(path[i], new ExpandoObject());
                }

                parent = parent[path[i]] as IDictionary<string, object>;
            }

            if (kvp.Value == null)
                continue;

            // add the value to the parent
            // note: in case of an array, key will be an integer and will be dealt with later
            var key = path[i];
            parent.Add(key, kvp.Value);
        }

        // at this stage, all arrays are seen as dictionaries with integer keys
        ReplaceWithArray(null, null, result);

        return result;
    }


    /// <summary>
    /// Gets config value, returns <paramref name="defaultValue"/> on error.
    /// </summary>
    public static T GetValueEx<T>(this IConfiguration config, string keyName, T defaultValue)
    {
        try
        {
            return config.GetValue(keyName, defaultValue);
        }
        catch { }

        return defaultValue;
    }


    private static void ReplaceWithArray(ExpandoObject? parent, string? key, ExpandoObject input)
    {
        if (input == null)
            return;

        var dict = input as IDictionary<string, object>;
        var keys = dict.Keys.ToArray();

        // it's an array if all keys are integers
        if (keys.All(k => int.TryParse(k, out var dummy)))
        {
            var array = new object[keys.Length];
            foreach (var kvp in dict)
            {
                array[int.Parse(kvp.Key)] = kvp.Value;

                // Edit: If structure is nested deeper we need this next line 
                ReplaceWithArray(input, kvp.Key, kvp.Value as ExpandoObject);
            }

            if (parent != null)
            {
                var parentDict = parent as IDictionary<string, object>;
                parentDict.Remove(key);
                parentDict.Add(key, array);
            }
        }
        else
        {
            foreach (var childKey in dict.Keys.ToList())
            {
                ReplaceWithArray(input, childKey, dict[childKey] as ExpandoObject);
            }
        }
    }

}
