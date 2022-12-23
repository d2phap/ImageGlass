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
using System.Security.Cryptography;
using System.Text;

namespace ImageGlass.Base.Cache;


/// <summary>
/// Represents a collection of items on disk that can be read 
/// and written by multiple threads.
/// </summary>
public class DiskCache
{
    #region Member Variables

    private string _dirName = string.Empty;
    private long _cacheSize = 0;
    private long _currentCacheSize = 0;
    private readonly object _lockObject = new();

    #endregion


    #region Properties

    /// <summary>
    /// Gets or sets the cache directory.
    /// </summary>
    public string DirectoryName
    {
        get
        {
            lock (_lockObject)
            {
                return _dirName;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _dirName = value;
                CalculateSize();
            }
        }
    }

    /// <summary>
    /// Gets or sets the cache size in bytes.
    /// </summary>
    public long CacheSize
    {
        get
        {
            lock (_lockObject)
            {
                return _cacheSize;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _cacheSize = value;
            }
        }
    }

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskCache"/> class.
    /// </summary>
    /// <param name="directoryName">The path to the cache file.</param>
    /// <param name="size">Cache size in bytes.</param>
    public DiskCache(string directoryName, long size)
    {
        _cacheSize = size;
        _dirName = directoryName.Trim();

        if (!string.IsNullOrEmpty(_dirName))
        {
            if (!Directory.Exists(_dirName))
            {
                Directory.CreateDirectory(_dirName);
            }

            lock (_lockObject)
            {
                CalculateSize();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiskCache"/> class.
    /// </summary>
    public DiskCache() : this(string.Empty, 0) { }

    #endregion


    #region Instance Methods

    /// <summary>
    /// Reads an item from the cache.
    /// </summary>
    /// <param name="id">Item identifier.</param>
    /// <returns>A stream holding item data.</returns>
    public Stream Read(string id)
    {
        lock (_lockObject)
        {
            var ms = new MemoryStream();
            if (string.IsNullOrEmpty(_dirName)) return ms;

            id = MakeKey(id);
            var filename = Path.Combine(_dirName, id);
            if (!File.Exists(filename)) return ms;

            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var read = 0;
            var buffer = new byte[4096];

            while ((read = fs.Read(buffer, 0, 4096)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms;
        }
    }

    /// <summary>
    /// Writes an item to the cache.
    /// </summary>
    /// <param name="id">Item identifier. If an item with this identifier already 
    /// exists, it will be overwritten.</param>
    /// <param name="data">Item data.</param>
    public void Write(string id, Stream data)
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(_dirName)) return;

            id = MakeKey(id);
            var filename = Path.Combine(_dirName, id);
            var bytesWritten = 0L;
            var read = 0;
            var buffer = new byte[4096];

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            data.Seek(0, SeekOrigin.Begin);

            while ((read = data.Read(buffer, 0, 4096)) > 0)
            {
                fs.Write(buffer, 0, read);
                bytesWritten += read;
            }

            _currentCacheSize += bytesWritten;

            if (_currentCacheSize > _cacheSize / 2)
            {
                PurgeCache();
            }
        }
    }

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="id">Item identifier.</param>
    public void Remove(string id)
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(_dirName)) return;

            id = MakeKey(id);

            string filename = Path.Combine(_dirName, id);
            if (!File.Exists(filename)) return;

            var fi = new FileInfo(filename);
            _currentCacheSize -= fi.Length;

            if (_currentCacheSize < 0) _currentCacheSize = 0;
            File.Delete(filename);
        }
    }

    /// <summary>
    /// Removes all items from the cache.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(_dirName)) return;

            foreach (var file in Directory.GetFiles(_dirName))
            {
                File.Delete(file);
            }

            _currentCacheSize = 0;
        }
    }

    /// <summary>
    /// Converts the given string to an item key.
    /// </summary>
    /// <param name="key">Input string.</param>
    /// <returns>Item key.</returns>
    private static string MakeKey(string key)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(key));

        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Calculates the size of the cache.
    /// </summary>
    private void CalculateSize()
    {
        lock (_lockObject)
        {
            _currentCacheSize = 0;

            if (string.IsNullOrEmpty(_dirName)) return;

            Directory.CreateDirectory(_dirName);

            foreach (FileInfo file in new DirectoryInfo(_dirName).GetFiles())
            {
                _currentCacheSize += file.Length;
            }
        }
    }

    /// <summary>
    /// Removes old items from the cache.
    /// </summary>
    private void PurgeCache()
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(_dirName)) return;
            if (_cacheSize == 0) return;

            var files = new DirectoryInfo(_dirName).GetFiles();
            var indexList = new List<FileInfo>();

            foreach (var file in files)
            {
                indexList.Add(file);
            }

            indexList.Sort((f1, f2) =>
            {
                var d1 = f1.CreationTime;
                var d2 = f2.CreationTime;
                return (d1 < d2 ? -1 : (d2 > d1 ? 1 : 0));
            });

            while (indexList.Count > 0 && _currentCacheSize > _cacheSize / 2)
            {
                var i = indexList.Count - 1;
                _currentCacheSize -= indexList[i].Length;

                indexList.RemoveAt(i);
                File.Delete(indexList[i].FullName);
            }

            if (_currentCacheSize < 0) _currentCacheSize = 0;
        }
    }

    #endregion

}

