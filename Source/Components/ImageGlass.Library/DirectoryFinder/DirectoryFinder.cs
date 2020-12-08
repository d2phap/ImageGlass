using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Library {
    public static class DirectoryFinder {
        #region DirectoryList
        /// <summary>
        /// Returns a list of directories under RootDirectory
        /// </summary>
        /// <param name="RootDirectory">starting directory</param>
        /// <param name="SearchAllDirectories">when true, all sub directories will be searched as well</param>
        /// <param name="Filter">filter to be done on directory. use null for no filtering</param>
        /// <returns></returns>
        public static ConcurrentBag<string> FindDirectories(string RootDirectory,
            bool SearchAllDirectories, Predicate<string> Filter) {
            var retList = new ConcurrentBag<string>();

            try {
                // create a directory info object 
                var di = new DirectoryInfo(RootDirectory);

                // loop through directories populating the list 
                Parallel.ForEach(di.GetDirectories(), folder => {
                    try {
                        // add the folder if it passes the filter 
                        if ((Filter == null) || (Filter(folder.FullName))) {
                            // add the folder 
                            retList.Add(folder.FullName);

                            // get its sub folders 
                            if (SearchAllDirectories) {
                                foreach (var dir in FindDirectories(folder.FullName, true, Filter)) {
                                    retList.Add(dir);
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) {
                        // don't really need to do anything 
                        // user just doesn't have access 
                    }

#pragma warning disable CS0168 // Variable is declared but never used
                    catch
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        // TODO: log the exception 
                    }
                });
            }

#pragma warning disable CS0168 // Variable is declared but never used
            catch
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // TODO: save exception 
            }

            // return the list 
            return retList;
        }

        // DirectoryList 
        #endregion

        #region FileList

        /// <summary>
        /// Returns a list of files under RootDirectory
        /// </summary>
        /// <param name="RootDirectory">>starting directory</param>
        /// <param name="SearchAllDirectories">>when true, all sub directories will be searched as well</param>
        /// <param name="Filter">filter to be done on files/directory. use null for no filtering</param>
        /// <returns></returns>
        public static ConcurrentBag<string> FindFiles(string RootDirectory,
            bool SearchAllDirectories, Predicate<FileInfo> Filter) {
            var retList = new ConcurrentBag<string>();

            try {
                // get the list of directories 
                var DirList = new List<string> { RootDirectory };

                // get sub directories if allowed 
                if (SearchAllDirectories) {
                    DirList.AddRange(FindDirectories(RootDirectory, true, null));
                }

                // loop through directories populating the list 
                Parallel.ForEach(DirList, folder => {
                    // get a directory object 
                    var di = new DirectoryInfo(folder);

                    try {
                        // loop through the files in this directory 
                        foreach (var file in di.GetFiles()) {
                            try {
                                // add the file if it passes the filter 
                                if ((Filter == null) || (Filter(file))) {
                                    retList.Add(file.FullName);
                                }
                            }

#pragma warning disable CS0168 // Variable is declared but never used
                            catch
#pragma warning restore CS0168 // Variable is declared but never used
                            {
                                // TODO: log the exception 
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) {
                        // don't really need to do anything 
                        // user just doesn't have access 
                    }

#pragma warning disable CS0168 // Variable is declared but never used
                    catch
#pragma warning restore CS0168 // Variable is declared but never used
                    {
                        // TODO: log the exception 
                    }
                });
            }

#pragma warning disable CS0168 // Variable is declared but never used
            catch
#pragma warning restore CS0168 // Variable is declared but never used
            {
                // TODO: save exception 
            }

            // return the list 
            return retList;
        }

        // FileList 
        #endregion 
    }
}
