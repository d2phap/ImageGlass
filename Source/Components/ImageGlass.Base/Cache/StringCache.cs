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

Author: Kevin Routley (aka fire-eggs)
*/

using System.Collections.Concurrent;

namespace ImageGlass.Base.Cache;


/// <summary>
/// A string cache.
/// </summary>
public class StringCache
{
    /// <summary>
    /// A comparator is necessary because just using vanilla char[] will result in
    /// a different hash for each *instance*. This comparator works on the contents.
    /// </summary>
    private class TagComparer : IEqualityComparer<char[]>
    {
        public bool Equals(char[]? x, char[]? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        public int GetHashCode(char[] obj)
        {
            if (obj.Length == 0) return 0;

            var hashCode = 0;
            for (var i = 0; i < obj.Length; i++)
            {
                var bytes = BitConverter.GetBytes(obj[i]);

                // Rotate by 3 bits and XOR the new value.
                for (var j = 0; j < bytes.Length; j++)
                {
                    hashCode = (hashCode << 3) | (hashCode >> (29)) ^ bytes[j];
                }
            }

            return hashCode;
        }
    }

    /// <summary>
    /// Concurrent dictionary used for parallelism
    /// </summary>
    private readonly ConcurrentDictionary<char[], string> _stringCache;

    /// <summary>
    /// Create a basic string cache.
    /// </summary>
    public StringCache()
    {
        _stringCache = new(new TagComparer());
    }

    /// <summary>
    /// Fetch or add a string from the cache.
    /// </summary>
    /// <param name="str">Input string to be cached.</param>
    /// <returns>The string from the cache.</returns>
    public string GetFromCache(string str)
    {
        return GetFromCache(str.ToCharArray());
    }

    /// <summary>
    /// Fetch or add a string from the cache.
    /// </summary>
    /// <param name="str">Input string to be cached.</param>
    /// <returns></returns>
    private string GetFromCache(char[] str)
    {
        if (!_stringCache.TryGetValue(str, out var result))
        {
            result = new string(str);

            try
            {
                _ = _stringCache.TryAdd(str, result);
            }
            catch { }
        }

        return result;
    }
}

