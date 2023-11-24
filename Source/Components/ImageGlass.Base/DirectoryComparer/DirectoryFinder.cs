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
using System.Collections.Concurrent;

namespace ImageGlass.Base.DirectoryComparer;


public static class DirectoryFinder
{

    /// <summary>
    /// Returns a list of directories under RootDirectory
    /// </summary>
    /// <param name="rootDir">Starting directory</param>
    /// <param name="searchAllDirectories">When true, all sub directories will be searched as well</param>
    /// <param name="filterFn">Filter to be done on directory. use null for no filtering</param>
    public static ConcurrentBag<string> FindDirectories(string rootDir,
        bool searchAllDirectories, Predicate<string>? filterFn = null)
    {
        var retList = new ConcurrentBag<string>();

        try
        {
            // create a directory info object 
            var di = new DirectoryInfo(rootDir);

            // loop through directories populating the list 
            Parallel.ForEach(di.GetDirectories(), folder =>
            {
                try
                {
                    // add the folder if it passes the filter 
                    if ((filterFn == null) || filterFn(folder.FullName))
                    {
                        // add the folder 
                        retList.Add(folder.FullName);

                        // get its sub folders 
                        if (searchAllDirectories)
                        {
                            foreach (var dir in FindDirectories(folder.FullName, true, filterFn))
                            {
                                retList.Add(dir);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // don't really need to do anything 
                    // user just doesn't have access 
                }

                catch
                {
                    // TODO: log the exception 
                }
            });
        }

        catch
        {
            // TODO: save exception 
        }

        return retList;
    }



    /// <summary>
    /// Returns a list of files under RootDirectory
    /// </summary>
    /// <param name="rootDir">Starting directory</param>
    /// <param name="searchAllDirectories">When true, all sub directories will be searched as well</param>
    /// <param name="filterFn">Filter function to be done on files/directory. Use null for no filtering</param>
    public static ConcurrentBag<string> FindFiles(string rootDir,
        bool searchAllDirectories, Predicate<FileInfo>? filterFn = null)
    {
        var retList = new ConcurrentBag<string>();

        try
        {
            // get the list of directories 
            var DirList = new List<string> { rootDir };

            // get sub directories if allowed 
            if (searchAllDirectories)
            {
                DirList.AddRange(FindDirectories(rootDir, true, null));
            }

            // loop through directories populating the list 
            Parallel.ForEach(DirList, folder =>
            {
                // get a directory object 
                var di = new DirectoryInfo(folder);

                try
                {
                    // loop through the files in this directory 
                    foreach (var file in di.GetFiles())
                    {
                        try
                        {
                            // add the file if it passes the filter 
                            if ((filterFn == null) || filterFn(file))
                            {
                                retList.Add(file.FullName);
                            }
                        }

                        catch
                        {
                            // TODO: log the exception 
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // don't really need to do anything 
                    // user just doesn't have access 
                }
                catch
                {
                    // TODO: log the exception 
                }
            });
        }
        catch
        {
            // TODO: save exception 
        }

        // return the list 
        return retList;
    }

}
