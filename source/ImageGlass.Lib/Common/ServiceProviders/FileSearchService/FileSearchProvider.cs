/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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

using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders.FileSearchService;


/// <summary>
/// Handles file searching, filtering, and sorting based on specified criteria.
/// </summary>
public partial class FileSearchProvider() : PhDisposable, IFileSearchProvider
{
    protected CancellationTokenSource? _cancelSearching;

    // Dolphin and Nautilus rank the base name above the extension; Explorer and Finder do not
    private static readonly bool _compareExtensionLast = BHelper.OS == OSType.Linux;


    // Public Properties
    #region Public Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public FileSearchOptions Options { get; protected set; } = new();


    #endregion // Public Properties



    #region Public Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async virtual Task SearchAsync(IEnumerable<string> dirs, FileSearchOptions options, Action<FileSearchingEventArgs>? progressFn = null)
    {
        Options = options;

        // cancel ongoing search
        CancelSearching();
        var token = _cancelSearching.Token;

        // snapshot the collection to avoid modification during enumeration
        var dirList = dirs.ToList();

        // Re-check the token at delivery, like the Win32 override does: a batch computed before
        // the caller's Clear() must not repopulate the fresh list it raced.
        Action<FileSearchingEventArgs>? publishFn = progressFn is null
            ? null
            : e =>
            {
                if (!token.IsCancellationRequested) progressFn(e);
            };

        // get files from the given directories
        try
        {
            await Task.Run(() =>
            {
                foreach (var dirPath in dirList)
                {
                    if (token.IsCancellationRequested) break;
                    FindFiles(dirPath, options, publishFn, token);
                }
            }, token);
        }
        catch { }
    }


    /// <summary>
    /// Cancels an ongoing file searching operation.
    /// </summary>
    [MemberNotNull(nameof(_cancelSearching))]
    public virtual void CancelSearching()
    {
        _cancelSearching?.Cancel();
        _cancelSearching?.Dispose();
        _cancelSearching = new();
    }


    #endregion // Public Methods



    #region Protected Functions

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        CancelSearching();
    }


    /// <summary>
    /// Filters a collection of strings and returns the filtered results.
    /// </summary>
    protected virtual IEnumerable<FileSearchEntry> OnFiltering(IEnumerable<FileSearchEntry> fileList,
        FileSearchOptions options)
    {
        if (options.AllowedExtensions is null) return fileList;

        return fileList.Where(entry =>
        {
            var ext = Path.GetExtension(entry.FilePath).ToLowerInvariant();

            return options.AllowedExtensions.Contains(ext);
        });
    }


    /// <summary>
    /// Sorts a collection of image file paths based on provided criteria.
    /// </summary>
    protected virtual IOrderedEnumerable<FileSearchEntry> OnSorting(IEnumerable<FileSearchEntry> fileList,
        FileSearchOptions options)
    {
        return SortEntries(fileList, options);
    }


    /// <summary>
    /// Finds files in the given directory, emits <see cref="FileSearching"/> event.
    /// </summary>
    protected void FindFiles(string dirPath, FileSearchOptions options,
        Action<FileSearchingEventArgs>? progressFn, CancellationToken token)
    {
        // cancel if requested
        if (token.IsCancellationRequested) return;

        // search files; hidden/system sub-folders are pruned from the recursion too
        var entries = EnumerateFileEntries(dirPath, options, options.SearchSubDirectories, token);


        // cancel if requested
        if (token.IsCancellationRequested) return;

        // filter list
        var filePaths = OnFiltering(entries, options);


        // cancel if requested
        if (token.IsCancellationRequested) return;

        // sort list
        filePaths = OnSorting(filePaths, options);


        // cancel if requested
        if (token.IsCancellationRequested) return;

        // emits results
        progressFn?.Invoke(new FileSearchingEventArgs(filePaths.ToList()));
    }


    /// <summary>
    /// Enumerates files and captures their filesystem metadata.
    /// </summary>
    protected static List<FileSearchEntry> EnumerateFileEntries(string dirPath,
        FileSearchOptions options, bool searchSubDirectories, CancellationToken token)
    {
        var entries = new List<FileSearchEntry>();
        try
        {
            foreach (var file in new DirectoryInfo(dirPath).EnumerateFiles("*",
                BHelper.GetEnumerationOptions(options.IncludeHidden, searchSubDirectories)))
            {
                if (token.IsCancellationRequested) break;

                var ext = file.Extension.ToLowerInvariant();
                if (options.AllowedExtensions is not null
                    && !options.AllowedExtensions.Contains(ext)) continue;

                entries.Add(FileSearchEntry.FromFileInfo(file));
            }
        }
        catch { }

        return entries;
    }


    /// <summary>
    /// Sorts a collection of filesystem entries based on provided criteria.
    /// </summary>
    private static IOrderedEnumerable<FileSearchEntry> SortEntries(IEnumerable<FileSearchEntry> fileList, FileSearchOptions options)
    {
        var filePathComparer = new StringNaturalComparer(options.OrderType == ImageOrderType.Asc, StringComparison.OrdinalIgnoreCase, _compareExtensionLast);
        var dirPathComparer = options.GroupByDir
            ? new StringNaturalComparer(options.OrderType == ImageOrderType.Asc, StringComparison.OrdinalIgnoreCase)
            : (IComparer<string?>)Comparer<string>.Create((a, b) => 0);
        var query = fileList.OrderBy(f => Path.GetDirectoryName(f.FilePath), dirPathComparer);

        if (options.OrderBy == ImageOrderBy.Random)
        {
            return query.ThenBy(_ => Guid.NewGuid());
        }

        var sorted = (options.OrderBy, options.OrderType) switch
        {
            (ImageOrderBy.FileSize, ImageOrderType.Desc) => query.ThenByDescending(f => f.FileSizeInBytes),
            (ImageOrderBy.FileSize, _) => query.ThenBy(f => f.FileSizeInBytes),
            (ImageOrderBy.DateCreated, ImageOrderType.Desc) => query.ThenByDescending(f => f.FileCreationTimeUtc),
            (ImageOrderBy.DateCreated, _) => query.ThenBy(f => f.FileCreationTimeUtc),
            (ImageOrderBy.Extension, ImageOrderType.Desc) => query.ThenByDescending(f => Path.GetExtension(f.FilePath), StringComparer.OrdinalIgnoreCase),
            (ImageOrderBy.Extension, _) => query.ThenBy(f => Path.GetExtension(f.FilePath), StringComparer.OrdinalIgnoreCase),
            (ImageOrderBy.DateAccessed, ImageOrderType.Desc) => query.ThenByDescending(f => f.FileLastAccessTimeUtc),
            (ImageOrderBy.DateAccessed, _) => query.ThenBy(f => f.FileLastAccessTimeUtc),
            (ImageOrderBy.DateModified, ImageOrderType.Desc) => query.ThenByDescending(f => f.FileLastWriteTimeUtc),
            (ImageOrderBy.DateModified, _) => query.ThenBy(f => f.FileLastWriteTimeUtc),
            _ => query,
        };

        return sorted.ThenBy(f => Path.GetFileName(f.FilePath), filePathComparer);
    }


    /// <summary>
    /// Sorts a collection of image file paths based on provided criteria.
    /// </summary>
    public static IOrderedEnumerable<string> SortFiles(IEnumerable<string> fileList, FileSearchOptions options)
    {
        var query = fileList;


        // Gets the file path comparer.
        var filePathComparer = new StringNaturalComparer(options.OrderType == ImageOrderType.Asc, StringComparison.OrdinalIgnoreCase, _compareExtensionLast);

        // Gets the directory path comparer.
        var dirPathComparer = options.GroupByDir
            ? new StringNaturalComparer(options.OrderType == ImageOrderType.Asc, StringComparison.OrdinalIgnoreCase)
            : (IComparer<string?>)Comparer<string>.Create((a, b) => 0);


        // sort by FileSize
        if (options.OrderBy == ImageOrderBy.FileSize)
        {
            if (options.OrderType == ImageOrderType.Desc)
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).Length)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).Length)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by DateCreated
        if (options.OrderBy == ImageOrderBy.DateCreated)
        {
            if (options.OrderType == ImageOrderType.Desc)
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by Extension
        if (options.OrderBy == ImageOrderBy.Extension)
        {
            if (options.OrderType == ImageOrderType.Desc)
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by DateAccessed
        if (options.OrderBy == ImageOrderBy.DateAccessed)
        {
            if (options.OrderType == ImageOrderType.Desc)
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by DateModified
        if (options.OrderBy == ImageOrderBy.DateModified)
        {
            if (options.OrderType == ImageOrderType.Desc)
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
            else
            {
                return query
                    .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                    .ThenBy(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => Path.GetFileName(f), filePathComparer);
            }
        }

        // sort by Random
        if (options.OrderBy == ImageOrderBy.Random)
        {
            // NOTE: ignoring the 'descending order' setting
            return query
                .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
                .ThenBy(_ => Guid.NewGuid());
        }


        // sort by Name (default)
        return query
            .OrderBy(f => Path.GetDirectoryName(f), dirPathComparer)
            .ThenBy(f => Path.GetFileName(f), filePathComparer);
    }


    #endregion // Protected Functions


}
