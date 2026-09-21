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
using Avalonia.Threading;
using D2Phap;
using ImageGlass.Common;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Win32.Common.ServiceProviders;

public partial class Win32FileSearchProvider : FileSearchProvider
{

    /// <summary>
    /// Searches files from the provided directories, following these steps:
    /// 
    /// <list type="number">
    /// <item>
    ///   Get files from provided shell view of <see cref="FileSearchOptions.ForegroundShell"/>,
    ///   ignoring the param <c><paramref name="dirs"/></c>.
    /// </item>
    /// <item>
    ///   If not (or error), and <see cref="FileSearchOptions.UseExplorerSortOrder"/> is <c>true</c>,
    ///   try to get the shell view from each param <c><paramref name="dirs"/></c> provided, then do step 1.
    /// </item>
    /// <item>
    ///   If not shell view from step 2, use the normal searching process:
    /// </item>
    /// </list>
    /// 
    /// The shell view only decides <i>which</i> files are listed; their order still comes from
    /// <see cref="FileSearchOptions.OrderBy"/> unless <see cref="FileSearchOptions.UseExplorerSortOrder"/> is <c>true</c>.
    /// 
    /// <inheritdoc/>
    /// </summary>
    public override async Task SearchAsync(IEnumerable<string> dirs, FileSearchOptions options, Action<FileSearchingEventArgs>? progressFn = null)
    {
        Options = options;

        // cancel ongoing search
        CancelSearching();
        var token = _cancelSearching.Token;

        // Publishing on the UI thread serializes old-search cancellation with list updates.
        Action<FileSearchingEventArgs>? publish = progressFn is null
            ? null
            : e => Dispatcher.UIThread.Post(() =>
            {
                if (!token.IsCancellationRequested) progressFn(e);
            });


        // 1. get files from the foreground window; a search result has no other source,
        // so the shell is used whenever the caller supplies one
        if (options.ForegroundShell != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (token.IsCancellationRequested) return;

                try
                {
                    var folderShell = GetShellFolderView(null, (ExplorerView?)options.ForegroundShell);
                    FindFiles_WithShell(folderShell.View, folderShell.DirPath, options, publish, token);
                }
                catch (COMException) { }
            });

            return;
        }


        // 2. get files from the given directories
        var fvMap = new ConcurrentDictionary<string, ExplorerFolderView?>();
        var dirList = dirs.ToList();

        if (options.UseExplorerSortOrder)
        {
            // find and save all shell folder view
            foreach (var dirPath in dirList)
            {
                if (token.IsCancellationRequested) return;

                var folderShell = await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var dirShell = GetShellFolderView(dirPath, null);
                    return dirShell;
                });

                var fv = folderShell.View;
                _ = fvMap.TryAdd(dirPath, fv);
            }
        }


        // 3. start searching in a separate thread
        await Task.Run(() =>
        {
            foreach (var dirPath in dirList)
            {
                if (token.IsCancellationRequested) break;
                var folderShellView = fvMap.GetValueOrDefault(dirPath);

                // with shell
                if (folderShellView != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            FindFiles_WithShell(folderShellView, dirPath, options, publish, token);
                        }
                    });
                }

                // without shell
                else
                {
                    FindFiles(dirPath, options, publish, token);
                }
            }
        });


        // dipose shell objects
        fvMap.Clear();
    }


    /// <summary>
    /// Finds files in the given <see cref="ExplorerFolderView"/>.
    /// Use the <see cref="FilesEnumerated"/> event to get results.
    /// </summary>
    private void FindFiles_WithShell(ExplorerFolderView? fv, string? rootDir,
        FileSearchOptions options, Action<FileSearchingEventArgs>? progressFn,
        CancellationToken token)
    {
        // if no folder view
        if (fv is null)
        {
            // use .NET
            if (!string.IsNullOrWhiteSpace(rootDir))
            {
                FindFiles(rootDir, options, progressFn, token);
            }
            return;
        }


        // shell determines which items are shown and in what order.
        var shellPaths = fv.GetItems(FolderItemViewOptions.SVGIO_FLAG_VIEWORDER).ToArray();

        // directory enumeration provides their filesystem metadata.
        var isFileSystemDirectory = !string.IsNullOrWhiteSpace(rootDir)
            && Path.IsPathFullyQualified(rootDir)
            && Directory.Exists(rootDir);
        var entriesByPath = isFileSystemDirectory
            ? EnumerateFileEntries(rootDir!, options, false, token).ToDictionary(entry => entry.FilePath, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, FileSearchEntry>(StringComparer.OrdinalIgnoreCase);

        if (token.IsCancellationRequested) return;

        var entries = shellPaths
            .Select(path =>
            {
                // ignore special folders
                if (path.StartsWith(EggShell.SPECIAL_DIR_PREFIX, StringComparison.InvariantCultureIgnoreCase)) return null;

                // filter unsupported Shell items
                if (options.AllowedExtensions is not null)
                {
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    if (!options.AllowedExtensions.Contains(ext)) return null;
                }

                try
                {
                    var entry = entriesByPath.GetValueOrDefault(path) ?? FileSearchEntry.FromPath(path);
                    var attrs = entry.Attributes;

                    // path is dir
                    if (attrs.HasFlag(FileAttributes.Directory)) return null;

                    // path is hidden/system
                    if ((attrs & BHelper.GetSkippedFileAttributes(options.IncludeHidden)) != 0) return null;

                    return entry;
                }
                catch
                {
                    return null;
                }
            })
            .Where(entry => entry is not null)
            .Cast<FileSearchEntry>()
            .ToList();

        // cancel if requested
        if (token.IsCancellationRequested) return;

        // the shell only decided which files are listed; the user's own order still applies
        var results = options.UseExplorerSortOrder
            ? entries
            : OnSorting(entries, options).ToList();

        // emit results
        progressFn?.Invoke(new FileSearchingEventArgs(results));


        // cancel if requested
        if (token.IsCancellationRequested) return;


        // search all sub-directories if root dir is a real filesystem directory.
        // Skip for shell URIs like `search-ms:` or `shell:` which are not valid paths
        // for Directory.EnumerateDirectories and would throw IOException.
        // https://github.com/d2phap/ImageGlass/issues/2189
        if (options.SearchSubDirectories && isFileSystemDirectory)
        {
            // search files for the sub dirs
            // get sub folders
            var subDirList = Directory.EnumerateDirectories(rootDir!, "*",
                BHelper.GetEnumerationOptions(options.IncludeHidden));

            // find files in sub-folders
            foreach (var dirPath in subDirList)
            {
                FindFiles(dirPath, options, progressFn, token);
            }
        }
    }


    /// <summary>
    /// Gets the <see cref="ExplorerFolderView"/> from the given dir path.
    /// </summary>
    /// <remarks>🔴 NOTE: Must run on UI thread.</remarks>
    /// <exception cref="COMException"></exception>
    private static (ExplorerFolderView? View, string DirPath) GetShellFolderView(string? rootDir, ExplorerView? foregroundShell)
    {
        var folderPath = string.Empty;
        var shell = new EggShell();
        ExplorerFolderView? folderView = null;


        // if no dir path, get the explorer's folder view where the application opened from
        if (string.IsNullOrWhiteSpace(rootDir))
        {
            if (foregroundShell?.GetTabFolderView() is ExplorerFolderView fv)
            {
                folderPath = foregroundShell.GetTabViewPath();
                folderView = fv;
            }
        }
        else if (!Path.EndsInDirectorySeparator(rootDir))
        {
            rootDir += Path.DirectorySeparatorChar;
        }


        rootDir ??= "";

        // find the folder view from the opening explorer windows
        if (folderView == null)
        {
            // find the explorer's folder view for each directory
            shell.WithOpeningWindows(ev =>
            {
                var windowPath = ev.GetTabViewPath();
                if (!Path.EndsInDirectorySeparator(windowPath))
                {
                    windowPath += Path.DirectorySeparatorChar;
                }

                // get the folder view for the input dir
                if (rootDir.Equals(windowPath, StringComparison.InvariantCultureIgnoreCase)
                    && ev.GetTabFolderView() is ExplorerFolderView fv)
                {
                    folderPath = windowPath;
                    folderView = fv;
                    return true;
                }

                return false;
            }, true);
        }


        return (folderView, folderPath);
    }

}
