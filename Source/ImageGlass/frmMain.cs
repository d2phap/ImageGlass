﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileWatcherEx;
using ImageGlass.Base;
using ImageGlass.Library;
using ImageGlass.Library.Comparer;
using ImageGlass.Library.Image;
using ImageGlass.Library.WinAPI;
using ImageGlass.Services;
using ImageGlass.Services.InstanceManagement;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.Renderers;
using ImageGlass.UI.ToolForms;
using ImageMagick;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace ImageGlass {
    public partial class frmMain: Form {
        public frmMain() {
            InitializeComponent();

            // Get DPI Scaling ratio
            // NOTE: the this.DeviceDpi property is not accurate
            DPIScaling.CurrentDPI = DPIScaling.GetSystemDpi();

            // Load UI Configs
            LoadConfig(isLoadUI: true, isLoadOthers: false);

            // Update form with new DPI
            OnDpiChanged();
            Application.DoEvents();

            // Disable built-in shortcuts
            picMain.ShortcutsEnabled = false;

            // Fix disk thrashing
            thumbnailBar.MetadataCacheEnabled = false;

            // apply Windows 11 Corner API
            CornerApi.ApplyCorner(mnuMain.Handle);
            CornerApi.ApplyCorner(mnuContext.Handle);
            CornerApi.ApplyCorner(mnuShortcut.Handle);
            CornerApi.ApplyCorner(mnuTray.Handle);
        }


        #region Local variables

        // window size value before resizing
        private Size _windowSize = new(1300, 800);

        // window state value before resizing
        private FormWindowState _windowState = FormWindowState.Minimized;

        // determine if the image is zoomed
        private bool _isManuallyZoomed;

        // determine if window is frameless (fullscreen / slideshow)
        private bool _isFrameless;

        // determine if is WindowFit (fullscreen / slideshow)
        private bool _isWindowFit;

        // determine if toolbar is shown (fullscreen / slideshow)
        private bool _isShowToolbar = true;

        // determine if thumbnail is shown  (fullscreen / slideshow)
        private bool _isShowThumbnail = true;

        // determine if Windows key is pressed
        private bool _isWindowsKeyPressed;

        // determine if user is dragging an image file
        private bool _isDraggingImage;

        // slideshow countdown interval
        private float _slideshowCountdown = 5;

        // slideshow stopwatch
        private Stopwatch _slideshowStopwatch = new();

        // force exiting app without checking reasons
        private bool _forceExitApp = false;

        // slideshow image alert counter
        private uint _numberImgsChangeCount = Configs.NumberImagesNotify;

        private bool _shouldPlayImgChangeAlert = Configs.IsPlayImageChangeSound;

        private readonly ToolFormManager _toolManager = new();

        private MovableForm _movableForm;

        // cancellation tokens of synchronious task
        private System.Threading.CancellationTokenSource _loadCancelToken = new();
        private System.Threading.CancellationTokenSource _busyCancelToken = new();

        /***********************************
         * Variables for FileWatcherEx
         ***********************************/
        // the list of local deleted files, need to be deleted in the memory list
        private readonly List<string> _queueListForDeleting = new();

        // File system watcher
        private FileWatcherEx.FileWatcherEx _fileWatcher = new();


        #endregion


        #region Drag - drop
        private void picMain_DragOver(object sender, DragEventArgs e) {
            try {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return;
                var dataTest = e.Data.GetData(DataFormats.FileDrop, false);
                if (dataTest == null) // observed: null w/ long path and long path support not enabled
                    return;

                var filePath = ((string[])dataTest)[0];

                // KBR 20190617 Fix observed issue: dragging from CD/DVD would fail because we set the
                // drag effect to Move, which is _not_allowed_
                // Drag file from DESKTOP to APP
                if (Local.ImageList.IndexOf(filePath) == -1 &&
                    (e.AllowedEffect & DragDropEffects.Move) != 0) {
                    e.Effect = DragDropEffects.Move;
                }
                // Drag file from APP to DESKTOP
                else {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            catch {
                // observed: exception with a long path and long path support enabled
            }
        }

        private void picMain_DragDrop(object sender, DragEventArgs e) {
            // Drag file from DESKTOP to APP
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            var filepaths = ((string[])e.Data.GetData(DataFormats.FileDrop, false));

            if (filepaths.Length > 1) {
                _ = PrepareLoadingAsync(filepaths, Local.ImageList.GetFileName(Local.CurrentIndex));
                return;
            }

            var filePath = filepaths[0];

            if (string.Equals(Path.GetExtension(filePath), ".lnk", StringComparison.CurrentCultureIgnoreCase))
                filePath = Shortcuts.GetTargetPathFromShortcut(filePath);

            var imageIndex = Local.ImageList.IndexOf(filePath);

            // The file is located another folder, load the entire folder
            if (imageIndex == -1) {
                PrepareLoading(filePath);
            }
            // The file is in current folder AND it is the viewing image
            else if (Local.CurrentIndex == imageIndex) {
                //do nothing
            }
            // The file is in current folder AND it is NOT the viewing image
            else {
                Local.CurrentIndex = imageIndex;
                _ = NextPicAsync(0);
            }
        }

        private void picMain_MouseDown(object sender, MouseEventArgs e) {
            if (_isDraggingImage) {
                var paths = new string[1];
                paths[0] = Local.ImageList.GetFileName(Local.CurrentIndex);

                var data = new DataObject(DataFormats.FileDrop, paths);
                picMain.DoDragDrop(data, DragDropEffects.Copy);

                _isDraggingImage = false;
            }
        }

        #endregion


        #region Preparing image
        /// <summary>
        /// Open an image
        /// </summary>
        private void OpenFile() {
            var formats = Configs.GetImageFormats(Configs.AllFormats);
            using var o = new OpenFileDialog() {
                Filter = Configs.Language.Items[$"{Name}._OpenFileDialog"] + "|" +
                        formats,
                CheckFileExists = true,
                RestoreDirectory = true,
            };
            if (o.ShowDialog() == DialogResult.OK) {
                _ = PrepareLoadingAsync(o.FileNames, o.FileNames[0]);
            }
        }

        /// <summary>
        /// Prepare to load images. This method invoked for image on the command line,
        /// i.e. when double-clicking an image.
        /// </summary>
        /// <param name="inputPath">The relative/absolute path of file/folder; or a URI Scheme</param>
        private void PrepareLoading(string inputPath) {
            var path = App.ToAbsolutePath(inputPath);
            var currentFileName = File.Exists(path) ? path : "";

            // Start loading path
            _ = PrepareLoadingAsync(new string[] { inputPath }, currentFileName);
        }

        /// <summary>
        /// Prepare to load images
        /// </summary>
        /// <param name="inputPaths">Paths of image files or folders. It can be relative/absolute paths or URI Scheme</param>
        /// <param name="currentFileName">Current viewing filename</param>
        private async Task PrepareLoadingAsync(IEnumerable<string> inputPaths, string currentFileName = "") {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            if (!inputPaths.Any()) return;

            var allFilesToLoad = new HashSet<string>();
            var currentFile = currentFileName;
            var hasInitFile = !string.IsNullOrEmpty(currentFile);

            // Display currentFile while loading the full directory
            if (hasInitFile) {
                _ = NextPicAsync(0, filename: currentFile);
            }

            // Parse string to absolute path
            var paths = inputPaths.Select(item => App.ToAbsolutePath(item));

            // prepare the distinct dir list
            var distinctDirsList = Helper.GetDistinctDirsFromPaths(paths);

            // track paths loaded to prevent duplicates
            var pathsLoaded = new HashSet<string>();
            var firstPath = true;

            var sortedFilesList = new List<string>();

            await Task.Run(() => {
                foreach (var apath in distinctDirsList) {
                    var dirPath = apath;
                    if (File.Exists(apath)) {
                        if (string.Equals(Path.GetExtension(apath), ".lnk", StringComparison.CurrentCultureIgnoreCase)) {
                            dirPath = Shortcuts.GetTargetPathFromShortcut(apath);
                        }
                        else {
                            dirPath = Path.GetDirectoryName(apath);
                        }
                    }
                    else if (Directory.Exists(apath)) {
                        // Issue #415: If the folder name ends in ALT+255 (alternate space), DirectoryInfo strips it.
                        // By ensuring a terminating slash, the problem disappears. By doing that *here*,
                        // the uses of DirectoryInfo in DirectoryFinder and FileWatcherEx are fixed as well.
                        // https://stackoverflow.com/questions/5368054/getdirectories-fails-to-enumerate-subfolders-of-a-folder-with-255-name
                        if (!apath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                            dirPath = apath + Path.DirectorySeparatorChar;
                        }
                    }
                    else {
                        continue;
                    }

                    // TODO: Currently only have the ability to watch a single path for changes!
                    if (firstPath) {
                        firstPath = false;
                        WatchPath(dirPath);

                        // Seek for explorer sort order
                        DetermineSortOrder(dirPath);
                    }

                    // KBR 20181004 Fix observed bug: dropping multiple files from the same path
                    // would load ALL files in said path multiple times! Prevent loading the same
                    // path more than once.
                    if (pathsLoaded.Contains(dirPath))
                        continue;

                    pathsLoaded.Add(dirPath);

                    var imageFilenameList = LoadImageFilesFromDirectory(dirPath);
                    allFilesToLoad.UnionWith(imageFilenameList);
                }

                Local.InitialInputPath = hasInitFile ? (distinctDirsList.Count > 0 ? distinctDirsList[0] : "") : currentFile;

                // sort list
                sortedFilesList = SortImageList(allFilesToLoad);

            }).ConfigureAwait(true);

            LoadImages(sortedFilesList, currentFile, skipLoadingImage: hasInitFile);
        }

        /// <summary>
        /// Load the images.
        /// </summary>
        /// <param name="imageFilenameList">The list of files to load</param>
        /// <param name="filePath">The image file path to view first</param>
        private void LoadImages(List<string> imageFilenameList, string filePath, bool skipLoadingImage = false) {
            // Dispose all garbage
            Local.ImageList.Dispose();

            // Set filename to image list
            Local.ImageList = new Heart.Factory(imageFilenameList) {
                MaxQueue = Configs.ImageBoosterCachedCount,
                Channels = (int)Local.Channels,
            };

            // Track image loading progress
            Local.ImageList.OnFinishLoadingImage -= ImageList_OnFinishLoadingImage;
            Local.ImageList.OnFinishLoadingImage += ImageList_OnFinishLoadingImage;

            // Find the index of current image
            if (filePath.Length > 0) {
                // this part of code fixes calls on legacy 8.3 filenames
                // (for example opening files from IBM Notes)
                var di = new DirectoryInfo(filePath);
                filePath = di.FullName;

                Local.CurrentIndex = Local.ImageList.IndexOf(filePath);

                // KBR 20181009 Changing "include subfolder" setting could lose the "current" image.
                // Prefer not to report said image is "corrupt", merely reset the index in that case.
                // 1. Setting: "include subfolders: ON". Open image in folder with images in subfolders.
                // 2. Move to an image in a subfolder.
                // 3. Change setting "include subfolders: OFF".
                // Issue: the image in the subfolder is attempted to be shown, declared as corrupt/missing.
                // Issue #481: the test is incorrect when imagelist is empty (i.e. attempt to open single, hidden image with 'show hidden' OFF)
                if (Local.CurrentIndex == -1 &&
                    Local.ImageList.Length > 0 &&
                    !Local.ImageList.ContainsDirPathOf(filePath)) {
                    Local.CurrentIndex = 0;
                }
            }
            else {
                Local.CurrentIndex = 0;
            }

            // Load thumnbnail
            LoadThumbnails();


            if (!skipLoadingImage) {
                // Start loading image
                _ = NextPicAsync(0);
            }

            SetStatusBar();
        }

        /// <summary>
        /// Watch a folder for changes.
        /// </summary>
        /// <param name="dirPath">The path to the folder to watch.</param>
        private void WatchPath(string dirPath) {
            // From Issue #530: file watcher currently fails nastily if given a prefixed path
            var pathToWatch = Heart.Helpers.DePrefixLongPath(dirPath);

            //Watch all changes of current path
            _fileWatcher.Stop();
            _fileWatcher = new FileWatcherEx.FileWatcherEx() {
                FolderPath = pathToWatch,
                IncludeSubdirectories = Configs.IsRecursiveLoading,

                // auto Invoke the form if required, no need to invidiually invoke in each event
                SynchronizingObject = this
            };

            _fileWatcher.OnCreated += FileWatcher_OnCreated;
            _fileWatcher.OnDeleted += FileWatcher_OnDeleted;
            _fileWatcher.OnChanged += FileWatcher_OnChanged;
            _fileWatcher.OnRenamed += FileWatcher_OnRenamed;

            _fileWatcher.Start();
        }

        private void ImageList_OnFinishLoadingImage(object sender, EventArgs e) {
            // clear text when finishing
            ShowToastMsg("", 0);

        }

        /// <summary>
        /// Select current thumbnail
        /// </summary>
        private void SelectCurrentThumbnail() {
            if (thumbnailBar.Items.Count > 0) {
                thumbnailBar.ClearSelection();

                try {
                    thumbnailBar.Items[Local.CurrentIndex].Selected = true;
                    thumbnailBar.Items[Local.CurrentIndex].Focused = true;
                    thumbnailBar.ScrollToIndex(Local.CurrentIndex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Sort and find all supported image from directory
        /// </summary>
        /// <param name="path">Image folder path</param>
        private static IEnumerable<string> LoadImageFilesFromDirectory(string path) {
            // Get files from dir
            return DirectoryFinder.FindFiles(path,
                Configs.IsRecursiveLoading,
                new Predicate<FileInfo>((FileInfo fi) => {
                    // KBR 20180607 Rework predicate to use a FileInfo instead of the filename.
                    // By doing so, can use the attribute data already loaded into memory, 
                    // instead of fetching it again (via File.GetAttributes). A re-fetch is
                    // very slow across network paths. For me, improves image load from 4+ 
                    // seconds to 0.4 seconds for a specific network path.

                    if (fi.FullName == null) // KBR not sure why but occasionally getting null filename
                        return false;

                    var extension = fi.Extension ?? "";
                    extension = extension.ToLower(); // Path.GetExtension(f).ToLower() ?? ""; //remove blank extension

                    // checks if image is hidden and ignores it if so
                    if (!Configs.IsShowingHiddenImages) {
                        var attributes = fi.Attributes; // File.GetAttributes(f);
                        var isHidden = (attributes & FileAttributes.Hidden) != 0;
                        if (isHidden) {
                            return false;
                        }
                    }

                    return extension.Length > 0 && Configs.AllFormats.Contains(extension);
                }));
        }

        /// <summary>
        /// Sort image list
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        private static List<string> SortImageList(IEnumerable<string> fileList) {
            // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!

            var list = new List<string>();

            // KBR 20190605 Fix observed limitation: to more closely match the Windows Explorer's sort
            // order, we must sort by the target column, then by name.
            var naturalSortComparer = Local.ActiveImageLoadingOrderType == ImageOrderType.Desc
                                        ? (IComparer<string>)new ReverseWindowsNaturalSort()
                                        : new WindowsNaturalSort();

            // initiate directory sorter to a comparer that does nothing
            // if user wants to group by directory, we initiate the real comparer
            var directorySortComparer = (IComparer<string>)new IdentityComparer();
            if (Configs.IsGroupImagesByDirectory) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    directorySortComparer = new ReverseWindowsDirectoryNaturalSort();
                }
                else {
                    directorySortComparer = new WindowsDirectoryNaturalSort();
                }
            }

            // KBR 20190605 Fix observed discrepancy: using UTC for create, but not for write/access times

            // Sort image file
            if (Local.ActiveImageLoadingOrder == ImageOrderBy.Name) {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Length) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenByDescending(f => new FileInfo(f).Length)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenBy(f => new FileInfo(f).Length)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.CreationTime) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Extension) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenByDescending(f => new FileInfo(f).Extension)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenBy(f => new FileInfo(f).Extension)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastAccessTime) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastWriteTime) {
                if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc) {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else {
                    list.AddRange(fileList
                        .OrderBy(f => f, directorySortComparer)
                        .ThenBy(f => new FileInfo(f).LastWriteTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Random) {
                // NOTE: ignoring the 'descending order' setting
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(_ => Guid.NewGuid()));
            }

            return list;
        }

        /// <summary>
        /// Clear and reload all thumbnail image
        /// </summary>
        private void LoadThumbnails() {
            thumbnailBar.SuspendLayout();
            thumbnailBar.Items.Clear();
            thumbnailBar.ThumbnailSize = new Size((int)Configs.ThumbnailDimension, (int)Configs.ThumbnailDimension);

            for (var i = 0; i < Local.ImageList.Length; i++) {
                var lvi = new ImageListView.ImageListViewItem(Local.ImageList.GetFileName(i)) {
                    Tag = Local.ImageList.GetFileName(i)
                };

                thumbnailBar.Items.Add(lvi);
            }
            thumbnailBar.ResumeLayout();

            SelectCurrentThumbnail();
        }

        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        /// <param name="isKeepZoomRatio"></param>
        /// <param name="isSkipCache"></param>
        /// <param name="pageIndex">Set pageIndex = int.MinValue to use default page index</param>
        public async Task NextPicAsync(int step, bool isKeepZoomRatio = false, bool isSkipCache = false, int pageIndex = int.MinValue, string filename = "") {

            System.Threading.SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            // cancel the previous loading task
            _loadCancelToken.Cancel();
            _loadCancelToken = new();

            if (Local.IsBusy) {
                return;
            }

            // Save previous image if it was modified
            if (File.Exists(Local.ImageModifiedPath) && Configs.IsSaveAfterRotating) {
                _ = SaveImageChangeAsync();

                // remove the old image data from cache
                Local.ImageList.Unload(Local.CurrentIndex);

                // update thumbnail
                thumbnailBar.Items[Local.CurrentIndex].Update();
            }
            else {
                // KBR 20190804 Fix obscure issue: 
                // 1. Rotate/flip image with "IsSaveAfterRotating" is OFF
                // 2. Move through images
                // 3. Turn "IsSaveAfterRotating" ON
                // 4. On navigate to another image, the change made at step 1 will be saved.
                Local.ImageModifiedPath = "";
            }

            SetStatusBar();
            picMain.Text = "";
            Local.IsTempMemoryData = false;

            if (filename.Length == 0 && Local.ImageList.Length == 0) {
                Local.ImageError = new Exception("File not found.");
                picMain.Image = null;
                Local.ImageModifiedPath = "";

                return;
            }


            // Issue #609: do not auto-reactivate slideshow if disabled
            if (Configs.IsSlideshow && timSlideShow.Enabled) {
                timSlideShow.Enabled = false;
                timSlideShow.Enabled = true;
                _slideshowStopwatch.Reset();
            }


            #region Validate image index

            // temp index
            var tempIndex = Local.CurrentIndex + step;


            // Issue #1019 : When showing the initial image, the ImageList is empty; don't show toast messages
            if (!Configs.IsSlideshow && !Configs.IsLoopBackViewer && Local.ImageList.Length > 0) {
                //Reach end of list
                if (tempIndex >= Local.ImageList.Length) {
                    ShowToastMsg(Configs.Language.Items[$"{Name}._LastItemOfList"], 1000);
                    return;
                }

                //Reach the first item of list
                if (tempIndex < 0) {
                    ShowToastMsg(Configs.Language.Items[$"{Name}._FirstItemOfList"], 1000);
                    return;
                }
            }

            // Check if current index is greater than upper limit
            if (tempIndex >= Local.ImageList.Length)
                tempIndex = 0;

            // Check if current index is less than lower limit
            if (tempIndex < 0)
                tempIndex = Local.ImageList.Length - 1;

            // Update current index
            Local.CurrentIndex = tempIndex;

            #endregion

            // Issue #1020: don't stop existing animation unless we're actually switching images
            // stop the animation
            if (picMain.IsAnimating) {
                picMain.StopAnimating();
            }


            // Select thumbnail item
            SelectCurrentThumbnail();

            // Raise image changed event
            Local.RaiseImageChangedEvent();
            try {
                // apply image list settings
                Local.ImageList.IsApplyColorProfileForAll = Configs.IsApplyColorProfileForAll;
                Local.ImageList.ColorProfileName = Configs.ColorProfile;
                Local.ImageList.UseRawThumbnail = Configs.IsUseRawThumbnail;
                Local.ImageList.SinglePageFormats = Configs.SinglePageFormats;

                // put app in a 'busy' state around image load: allows us to prevent the user
                // from skipping past a slow-to-load image by processing too many arrow clicks
                _ = SetAppBusyAsync(true, Configs.Language.Items[$"{Name}._Loading"], 2000, 2000);

                if (pageIndex != int.MinValue) {
                    UpdateActivePage();
                }
                else {
                    Heart.Img bmpImg;

                    // directly load the image file, skip image list
                    if (filename.Length > 0) {
                        bmpImg = new Heart.Img(filename);
                        await bmpImg.LoadAsync(
                            colorProfileName: Configs.ColorProfile,
                            isApplyColorProfileForAll: Configs.IsApplyColorProfileForAll,
                            channel: (int)Local.Channels,
                            useRawThumbnail: Local.ImageList.UseRawThumbnail,
                            forceLoadFirstPage: Configs.SinglePageFormats.Contains(bmpImg.Extension)
                            );
                    }
                    else {
                        bmpImg = await Local.ImageList.GetImgAsync(
                            Local.CurrentIndex,
                            isSkipCache: isSkipCache,
                            pageIndex: pageIndex
                           ).ConfigureAwait(true);
                    }

                    // Update current frame index
                    Local.CurrentPageIndex = bmpImg.ActivePageIndex;
                    Local.CurrentPageCount = bmpImg.PageCount;

                    Local.CurrentExif = bmpImg.Exif;
                    Local.CurrentColor = bmpImg.ColorProfile;

                    Local.ImageError = bmpImg.Error;


                    if (bmpImg.Image != null && !_loadCancelToken.Token.IsCancellationRequested) {
                        picMain.Image = bmpImg.Image;

                        // Reset the zoom mode if isKeepZoomRatio = FALSE
                        if (!isKeepZoomRatio) {
                            if (Configs.IsWindowFit) {
                                WindowFitMode();
                            }
                            else {
                                // reset zoom mode
                                ApplyZoomMode(Configs.ZoomMode);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Local.ImageError = ex;
            }

            // clear busy state
            _ = SetAppBusyAsync(false);

            // image error
            if (Local.ImageError != null) {
                picMain.Image = null;
                Local.ImageModifiedPath = "";
                Local.CurrentPageIndex = 0;
                Local.CurrentPageCount = 0;
                Local.CurrentExif = null;
                Local.CurrentColor = null;

                var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);
                if (!string.IsNullOrEmpty(currentFile) && !File.Exists(currentFile)) {
                    Local.ImageList.Unload(Local.CurrentIndex);
                }

                picMain.Text = Configs.Language.Items[$"{Name}.picMain._ErrorText"] + "\r\n" + Local.ImageError.Source + ": " + Local.ImageError.Message;
            }

            SetStatusBar();

            _isDraggingImage = false;

            // reset countdown timer value
            _slideshowCountdown = Configs.RandomizeSlideshowInterval();
            // since the UI does not print milliseconds,
            // this prevents the coutdown to flash the maximum value during the first tick
            if (_slideshowCountdown == Math.Ceiling(_slideshowCountdown))
                _slideshowCountdown -= 0.001f;

            // reset Cropping region
            ShowCropTool(mnuMainCrop.Checked);

            // auto-show Page Nav tool
            if (Local.CurrentPageCount > 1 && Configs.IsShowPageNavAuto) {
                ShowPageNavTool(true);
            }
            // hide the Page Nav tool
            else if (!Configs.IsShowPageNavOnStartup) {
                ShowPageNavTool(false);
            }

            // Collect system garbage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();


            void UpdateActivePage() {
                var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);
                Local.CurrentPageIndex = Heart.Img.SetActivePage((Bitmap)picMain.Image, pageIndex, currentFile);

                // Refresh picMain to update the active page
                picMain.Invalidate();
            }
        }

        /// <summary>
        /// Play sound on Image change
        /// </summary>
        private void Local_OnImageChanged(object sender, EventArgs e) {
            if (_shouldPlayImgChangeAlert == true) {
                if (_numberImgsChangeCount == 0) {
                    Local.PlaySound();
                    _numberImgsChangeCount = Configs.NumberImagesNotify;
                }
                else {
                    _numberImgsChangeCount -= 1;
                }
            }
            else {
                _shouldPlayImgChangeAlert = Configs.IsPlayImageChangeSound;
            }
        }


        /// <summary>
        /// Update image information on status bar
        /// </summary>
        private void SetStatusBar(string text = "") {
            if (Configs.IsUseEmptyTitleBar) {
                Text = "";
                return;
            }
            else if (!string.IsNullOrWhiteSpace(text)) {
                Text = text;
                return;
            }

            var appName = Application.ProductName;
            const string SEP = "  |  ";
            var imgSize = string.Empty;
            var fileSize = string.Empty;
            var exifInfo = string.Empty;

            string zoom;
            if (Local.IsTempMemoryData) {
                var imgData = Configs.Language.Items[$"{Name}._ImageData"];
                zoom = $"{picMain.Zoom}%";

                if (picMain.Image != null) {
                    try {
                        imgSize = $"{picMain.Image.Width} x {picMain.Image.Height} px";
                    }
                    catch { }

                    // (Image data)  |  {zoom}  |  {image size} - ImageGlass
                    Text = $"{imgData}  |  {zoom}  |  {imgSize}  - {appName}";
                }
                else {
                    Text = $"{imgData}  |  {zoom}  - {appName}";
                }
            }
            else {
                if (Local.ImageList.Length < 1) {
                    Text = appName;
                    return;
                }

                var filename = Local.ImageList.GetFileName(Local.CurrentIndex);

                // when there is a problem with a file, don't try to show more info
                var isShowMoreData = File.Exists(filename);

                var indexTotal = $"{Local.CurrentIndex + 1}/{Local.ImageList.Length} {Configs.Language.Items[$"{Name}._Files"]}";

                if (isShowMoreData) {
                    fileSize = ImageInfo.GetFileSize(filename);

                    // get color profile
                    var colorProfile = Local.CurrentColor?.ColorSpace.ToString();
                    exifInfo += colorProfile?.Length > 0 ? $"{SEP}{colorProfile}" : "";
                    // get comment
                    var ext = Path.GetExtension(filename).ToLower();
                    if (ext == ".jpg" | ext == ".jpeg") {
                        try {
                            var so = ShellObject.FromParsingName(filename);
                            var _usercomment = so.Properties.GetProperty(SystemProperties.System.Comment);
                            if(_usercomment.ValueAsObject != null) { 
                                string usercomment = _usercomment.ValueAsObject.ToString();
                                exifInfo = exifInfo + $"{SEP}" + usercomment;
                            }
                        }
                        catch { }

                    }
                    // get date info
                    exifInfo += $"{SEP}{GetImageDateInfo(filename)}";
                }

                if (Configs.IsDisplayBasenameOfImage) {
                    filename = Path.GetFileName(filename);
                }
                else {
                    // auto ellipsis the filename
                    // the minimum text to show is Drive letter + basename.
                    // ex: C:\...\example.jpg
                    var basename = Path.GetFileName(filename);

                    var charWidth = CreateGraphics().MeasureString("A", Font).Width;
                    var textMaxLength = (Width - DPIScaling.Transform(400)) / charWidth;
                    var maxLength = (int)Math.Max(basename.Length + 8, textMaxLength);

                    filename = Helper.ShortenPath(filename, maxLength);
                }

                // image error
                if (Local.ImageError != null) {
                    Local.FPageNav.lblPageInfo.Text = "";

                    if (!isShowMoreData) // size and date not available
                        Text = $"{filename}{SEP}{indexTotal}  - {appName}";
                    else
                        Text = $"{filename}{SEP}{indexTotal}{SEP}{fileSize}  - {appName}";
                }
                else {
                    zoom = $"{picMain.Zoom:F2}%";

                    // pages information
                    var pageInfo = $"{Local.CurrentPageIndex + 1}/{Local.CurrentPageCount}";
                    Local.FPageNav.lblPageInfo.Text = pageInfo;

                    if (Local.CurrentPageCount > 1) {
                        pageInfo = $"{pageInfo} {Configs.Language.Items[$"{Name}._Pages"]}{SEP}";
                    }
                    else {
                        pageInfo = "";
                    }

                    // image info
                    if (picMain.Image != null) {
                        try {
                            imgSize = $"{picMain.Image.Width} x {picMain.Image.Height} px";
                        }
                        catch { }

                        Text = $"{filename}{SEP}{indexTotal}{SEP}{pageInfo}{zoom}{SEP}{imgSize}{SEP}{fileSize}{exifInfo}  - {appName}";
                    }
                    else {
                        Text = $"{filename}{SEP}{indexTotal}{SEP}{pageInfo}{zoom}{SEP}{fileSize}{exifInfo}  - {appName}";
                    }
                }
            }
        }

        /// <summary>
        /// Get image datetime info, returns either Exif.DateTimeOriginal (o), Exif.DateTime, or File.LastWriteTime (m)
        /// </summary>
        /// <param name="filename">The full file path</param>
        /// <returns></returns>
        private static string GetImageDateInfo(string filename) {
            static string GetExifDateInfo(ExifTag<string> tag) {
                // get date
                var dateExif = Local.CurrentExif?.GetValue(tag)?.Value;

                if (DateTime.TryParseExact(dateExif,
                    "yyyy:MM:dd HH:mm:ss",
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var dateTaken)) {
                    return $"{dateTaken:yyyy/MM/dd HH:mm:ss}";
                }

                return string.Empty;
            }

            var exifDateOriginal = GetExifDateInfo(ExifTag.DateTimeOriginal);
            if (exifDateOriginal.Length == 0) {
                var exifDate = GetExifDateInfo(ExifTag.DateTime);
                if (exifDate.Length == 0) {
                    var fileDateModified = File.GetLastWriteTime(filename).ToString("yyyy/MM/dd HH:mm:ss");

                    // return LastWriteTime
                    return fileDateModified + " (m)";
                }

                // return DateTime
                return exifDate;
            }

            // return DateTimeOriginal
            return exifDateOriginal + " (o)";
        }

        #endregion


        #region Key event

        private void frmMain_KeyDown(object sender, KeyEventArgs e) {
            //this.Text = e.KeyValue.ToString();

            if (Local.IsBusy) {
                return;
            }

            #region Register MAIN MENU shortcuts
            bool checkMenuShortcut(ToolStripMenuItem mnu) {
                var pressed = e.KeyCode;
                if (e.Control) pressed |= Keys.Control;
                if (e.Shift) pressed |= Keys.Shift;
                if (e.Alt) pressed |= Keys.Alt;

                if (mnu.ShortcutKeys == pressed) {
                    mnu.PerformClick();
                    return true;
                }
                foreach (var child in mnu.DropDownItems.OfType<ToolStripMenuItem>()) {
                    checkMenuShortcut(child);
                }

                return false;
            }

            //register context menu shortcuts
            foreach (var item in mnuMain.Items.OfType<ToolStripMenuItem>()) {
                if (checkMenuShortcut(item)) {
                    return;
                }
            }
            #endregion


            #region Detect WIN logo key
            _isWindowsKeyPressed = false;
            if (e.KeyData == Keys.LWin || e.KeyData == Keys.RWin) {
                _isWindowsKeyPressed = true;
            }
            #endregion


            // KBR 20191210 Fix observed issue: when using WIN+down-arrow to minimize in
            // frameless mode, the first key code after restore would be ignored. Moved
            // these lines to _after_ WIN logo key check is complete.
            var hasNoMods = !e.Control && !e.Shift && !e.Alt;
            var ignore = Local.IsBusy || _isWindowsKeyPressed;
            _isDraggingImage = false;

            // Rotation Counterclockwise
            #region Ctrl + ,
            if (e.KeyValue == 188 && e.Control && !e.Shift && !e.Alt)//Ctrl + ,
            {
                mnuMainRotateCounterclockwise_Click(null, null);
                return;
            }
            #endregion


            // Rotate Clockwise
            #region Ctrl + .
            if (e.KeyValue == 190 && e.Control && !e.Shift && !e.Alt)//Ctrl + .
            {
                mnuMainRotateClockwise_Click(null, null);
                return;
            }
            #endregion


            // Flip Horizontally
            #region Ctrl + ;
            if (e.KeyValue == 186 && e.Control && !e.Shift && !e.Alt)//Ctrl + ;
            {
                mnuMainFlipHorz_Click(null, null);
                return;
            }
            #endregion


            // Flip Vertically
            #region Ctrl + '
            if (e.KeyValue == 222 && e.Control && !e.Shift && !e.Alt)//Ctrl + '
            {
                mnuMainFlipVert_Click(null, null);
                return;
            }
            #endregion


            // Clear clipboard
            #region CTRL + `
            if (e.KeyValue == 192 && e.Control && !e.Shift && !e.Alt)//CTRL + `
            {
                mnuMainClearClipboard_Click(null, null);
                return;
            }
            #endregion


            // Zoom in
            #region Ctrl + = / = / + (numPad)
            if ((e.KeyValue == 187 || (e.KeyValue == 107 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + =
            {
                btnZoomIn_Click(null, null);
                return;
            }
            #endregion


            // Zoom out
            #region Ctrl + - / - / - (numPad)
            if ((e.KeyValue == 189 || (e.KeyValue == 109 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + -
            {
                btnZoomOut_Click(null, null);
                return;
            }
            #endregion


            // Actual size image
            #region Ctrl + 0 / Ctrl + Num0 / 0 / Num0
            if (!e.Shift && !e.Alt && (e.KeyValue == 48 || e.KeyValue == 96)) // 0 || Num0 || Ctrl + 0 || Ctrl + Num0
            {
                btnActualSize_Click(null, null);
                return;
            }
            #endregion


            // ESC ultility
            #region ESC
            if (e.KeyCode == Keys.Escape) // ESC
            {
                if (!e.Control && !e.Alt) {
                    if (!e.Shift) {
                        // ESC: exit slideshow
                        if (Configs.IsSlideshow) {
                            mnuMainSlideShowExit_Click(null, null);
                        }
                        // ESC: Quit ImageGlass
                        else if (Configs.IsPressESCToQuit) {
                            Exit();
                        }
                    }
                    // Shift + ESC: Truly quit ImageGlass
                    else if (Configs.IsPressESCToQuit) {
                        Exit(true);
                    }
                }

                return;
            }
            #endregion


            // Previous Image
            #region LEFT ARROW / PAGE UP
            if (e.KeyValue == (int)Keys.Left && hasNoMods) {
                if (ignore) {
                    e.Handled = true; // Issue #963: leaning on the key will pan the image if IG is busy!
                }
                if (Configs.KeyComboActions[KeyCombos.LeftRight] == AssignableActions.PrevNextImage) {
                    e.Handled = true; // Issue #963: don't let ImageBox see the keystroke!
                    _ = NextPicAsync(-1);
                }
                else {
                    picMain.HandlePan(Configs.ImageHorizontalPanningSpeed, Configs.ImageVerticalPanningSpeed, e);
                    e.Handled = true;
                }
                return;
            }
            if (!ignore && e.KeyValue == (int)Keys.PageUp && hasNoMods) {
                var action = Configs.KeyComboActions[KeyCombos.PageUpDown];
                if (action == AssignableActions.PrevNextImage) {
                    _ = NextPicAsync(-1);
                }
                else if (action == AssignableActions.ZoomInOut) {
                    mnuMainZoomIn_Click(null, null);
                }

                return;
            }
            #endregion


            // Next Image
            #region RIGHT ARROW / PAGE DOWN
            if (e.KeyValue == (int)Keys.Right && hasNoMods) {
                if (ignore) {
                    e.Handled = true; // Issue #963: leaning on the key will pan the image if IG is busy!
                }
                if (Configs.KeyComboActions[KeyCombos.LeftRight] == AssignableActions.PrevNextImage) {
                    e.Handled = true; // Issue #963: don't let ImageBox see the keystroke!
                    _ = NextPicAsync(1);
                }
                else {
                    picMain.HandlePan(Configs.ImageHorizontalPanningSpeed, Configs.ImageVerticalPanningSpeed, e);
                    e.Handled = true;
                }
                return;
            }
            if (!ignore && e.KeyValue == (int)Keys.PageDown && hasNoMods) {
                var action = Configs.KeyComboActions[KeyCombos.PageUpDown];
                if (action == AssignableActions.PrevNextImage) {
                    _ = NextPicAsync(1);
                }
                else if (action == AssignableActions.ZoomInOut) {
                    mnuMainZoomOut_Click(null, null);
                }

                return;
            }
            #endregion


            // Pan up
            #region UP ARROW
            if (!ignore && e.KeyValue == (int)Keys.Up && hasNoMods) {
                if (Configs.KeyComboActions[KeyCombos.UpDown] == AssignableActions.ZoomInOut) {
                    mnuMainZoomIn_Click(null, null);
                    e.Handled = true;
                }
                else {
                    // Assume action is pan.
                    picMain.HandlePan(Configs.ImageHorizontalPanningSpeed, Configs.ImageVerticalPanningSpeed, e);
                    e.Handled = true;
                }
                return; // fall-through lets pan happen
            }
            #endregion


            // Pan down
            #region DOWN ARROW
            if (!ignore && e.KeyValue == (int)Keys.Down && hasNoMods) {
                if (Configs.KeyComboActions[KeyCombos.UpDown] == AssignableActions.ZoomInOut) {
                    mnuMainZoomOut_Click(null, null);
                    e.Handled = true;
                }
                else {
                    // Assume action is pan.
                    picMain.HandlePan(Configs.ImageHorizontalPanningSpeed, Configs.ImageVerticalPanningSpeed, e);
                    e.Handled = true;
                }
                // Handle pan events.
                return; // fall-through lets pan happen
            }
            #endregion


            // Goto the first Image
            #region HOME
            if (!_isWindowsKeyPressed && e.KeyValue == 36 &&
                !e.Control && !e.Shift && !e.Alt) {
                mnuMainGotoFirst_Click(null, e);
                return;
            }
            #endregion


            // Goto the last Image
            #region END
            if (!_isWindowsKeyPressed && e.KeyValue == 35 &&
                !e.Control && !e.Shift && !e.Alt) {
                mnuMainGotoLast_Click(null, e);
                return;
            }
            #endregion


            // Ctrl
            #region CTRL + ...
            if (e.Control && !e.Alt && !e.Shift) // Ctrl
            {
                // Enable dragging viewing image to desktop feature---------------------------
                _isDraggingImage = true;

                // View previous image page
                #region Ctrl + (previous)
                if ((e.KeyValue == (int)Keys.Left
                    && Configs.KeyComboActions[KeyCombos.LeftRight] == AssignableActions.PrevNextImage)
                    || (e.KeyValue == (int)Keys.PageUp
                    && Configs.KeyComboActions[KeyCombos.PageUpDown] == AssignableActions.PrevNextImage)
                    ) {
                    mnuMainPrevPage_Click(null, null);
                    return;
                }
                #endregion

                // View next image page
                #region Ctrl + (next)
                if ((e.KeyValue == (int)Keys.Right
                    && Configs.KeyComboActions[KeyCombos.LeftRight] == AssignableActions.PrevNextImage)
                    || (e.KeyValue == (int)Keys.PageDown
                    && Configs.KeyComboActions[KeyCombos.PageUpDown] == AssignableActions.PrevNextImage)
                    ) {
                    mnuMainNextPage_Click(null, null);
                    return;
                }
                #endregion

                // View first image page
                #region Ctrl + Home
                if (!_isWindowsKeyPressed && e.KeyValue == 36) {
                    mnuMainFirstPage_Click(null, null);
                }
                #endregion

                // View last image page
                #region Ctrl + End
                if (!_isWindowsKeyPressed && e.KeyValue == 35) {
                    mnuMainLastPage_Click(null, null);
                }
                #endregion

                // Exit app
                #region Ctrl + W
                if (!_isWindowsKeyPressed && e.KeyCode == Keys.W) {
                    Exit();
                }
                #endregion

                return;
            }
            #endregion


            // Shift
            #region Shift + ...
            if (e.Shift && !e.Control && !e.Alt) {
                // Shift+O: Loading order dropdown menu
                if (e.KeyCode == Keys.O) {
                    OpenShortcutMenu(mnuLoadingOrder);
                    return;
                }

                // Shift+C: Channels dropdown menu
                if (e.KeyCode == Keys.C) {
                    OpenShortcutMenu(mnuMainChannels);
                    return;
                }
            }
            #endregion


            // Alt
            #region Alt + ...
            if (e.Alt && !e.Shift && !e.Control) {

                // Alt+F: Open main menu
                if (e.KeyCode == Keys.F) {
                    mnuMain.Show(toolMain, toolMain.Width - mnuMain.Width, toolMain.Height);

                    return;
                }
            }
            #endregion


            // Without Modifiers keys 
            #region Without Modifiers keys
            if (hasNoMods) {
                // Toggle Window on Top
                if (e.KeyValue == 192) // `
                {
                    mnuMainAlwaysOnTop.PerformClick();
                    return;
                }

                // Checkerboard background
                if (e.KeyCode == Keys.B) {
                    mnuMainCheckBackground.PerformClick();
                    return;
                }

                // Crop tool
                if (e.KeyCode == Keys.C) {
                    mnuMainCrop.PerformClick();
                    return;
                }

                // Open with
                if (e.KeyCode == Keys.D) {
                    mnuOpenWith_Click(null, null);
                    return;
                }

                // Edit image
                if (e.KeyCode == Keys.E) {
                    mnuMainEditImage_Click(null, null);
                    return;
                }

                // Go to...
                if (e.KeyCode == Keys.G) {
                    mnuMainGoto.PerformClick();
                    return;
                }

                // Thumbnail bar
                if (e.KeyCode == Keys.H) {
                    mnuMainThumbnailBar.PerformClick();
                    return;
                }

                // Color picker tool
                if (e.KeyCode == Keys.K) {
                    mnuMainColorPicker.PerformClick();
                    return;
                }

                // Open image location
                if (e.KeyCode == Keys.L) {
                    mnuMainImageLocation_Click(null, null);
                    return;
                }

                // Page naviagtion tool
                if (e.KeyCode == Keys.P) {
                    mnuMainPageNav.PerformClick();
                    return;
                }

                // Refresh image
                if (e.KeyCode == Keys.R) {
                    mnuMainRefresh.PerformClick();
                    return;
                }

                // Share image
                if (e.KeyCode == Keys.S) {
                    mnuMainShare.PerformClick();
                    return;
                }

                // Toolbar
                if (e.KeyCode == Keys.T) {
                    mnuMainToolbar.PerformClick();

                    return;
                }

                // Exif tool
                if (e.KeyCode == Keys.X) {
                    mnuExifTool.PerformClick();
                    return;
                }

                // Custom zoom
                if (e.KeyCode == Keys.Z) {
                    mnuCustomZoom_Click(null, null);
                    return;
                }

                // Auto zoom
                if (e.KeyValue == 49 || e.KeyCode == Keys.NumPad1) // Num1 / NumPad1
                {
                    mnuMainAutoZoom_Click(null, null);
                    return;
                }

                // Lock zoom ratio
                if (e.KeyValue == 50 || e.KeyCode == Keys.NumPad2) // Num2 / NumPad2
                {
                    mnuMainLockZoomRatio_Click(null, null);
                    return;
                }

                // Scale to width
                if (e.KeyValue == 51 || e.KeyCode == Keys.NumPad3) // Num3 / NumPad3
                {
                    mnuMainScaleToWidth_Click(null, null);
                    return;
                }

                // Scale to height
                if (e.KeyValue == 52 || e.KeyCode == Keys.NumPad4) // Num4 / NumPad4
                {
                    mnuMainScaleToHeight_Click(null, null);
                    return;
                }

                // Scale to fit
                if (e.KeyValue == 53 || e.KeyCode == Keys.NumPad5) // Num5 / NumPad5
                {
                    mnuMainScaleToFit_Click(null, null);
                    return;
                }

                // Scale to fill
                if (e.KeyValue == 54 || e.KeyCode == Keys.NumPad6) // Num6 / NumPad6
                {
                    mnuMainScaleToFill_Click(null, null);
                    return;
                }
            }
            #endregion
        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e) {
            //this.Text = e.KeyValue.ToString();

            //Ctrl---------------------------------------------------------------------------
            #region CTRL (for Zooming)
            if (e.KeyData == Keys.ControlKey && !e.Alt && !e.Shift)//Ctrl
            {
                //Disable dragging viewing image to desktop feature--------------------------
                _isDraggingImage = false;

                return;
            }
            #endregion

            //Start / stop slideshow---------------------------------------------------------
            #region SPACE
            var no_mods = !e.Control && !e.Shift && !e.Alt;
            if (e.KeyCode == Keys.Space && no_mods) {
                if (Configs.IsSlideshow) // Space always pauses slideshow if playing
                {
                    mnuMainSlideShowPause_Click(null, null);
                }
                else if (Configs.KeyComboActions[KeyCombos.SpaceBack] == AssignableActions.PrevNextImage) {
                    _ = NextPicAsync(1);
                }
                return;
            }
            #endregion


            #region Backspace
            if (e.KeyCode == Keys.Back && no_mods) {
                if (Configs.KeyComboActions[KeyCombos.SpaceBack] == AssignableActions.PrevNextImage) {
                    _ = NextPicAsync(-1);
                }
                return;
            }
            #endregion
        }
        #endregion


        #region Private functions

        /// <summary>
        /// Handle the event when Dpi changed
        /// </summary>
        private void OnDpiChanged() {
            // Change grid cell size
            picMain.GridCellSize = DPIScaling.Transform(Constants.VIEWER_GRID_SIZE);

            // Change size of resize handlers
            picMain.DragHandleSize = DPIScaling.Transform(8);

            #region change size of toolbar
            // Update size of toolbar
            DPIScaling.TransformToolbar(ref toolMain, (int)Configs.ToolbarIconHeight);

            // Update toolbar icon according to the new size
            LoadToolbarIcons(forceReloadIcon: true);

            #endregion

            #region change size of menu items
            var newMenuIconHeight = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);

            mnuMainOpenFile.Image =
                mnuMainZoomIn.Image =
                mnuMainViewNext.Image =
                mnuMainSlideShowStart.Image =
                mnuMainRotateLeft.Image =

                mnuMainClearClipboard.Image =
                mnuMainToolbar.Image =
                mnuMainColorPicker.Image =
                mnuMainPageNav.Image =
                mnuMainAbout.Image =
                mnuMainSettings.Image =
                mnuMainExitApplication.Image =

                mnuMainExtractPages.Image =

                new Bitmap(newMenuIconHeight, newMenuIconHeight);

            if (mnuMainChannels.DropDownItems.Count > 0) {
                mnuMainChannels.DropDownItems[0].Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            }

            if (mnuLoadingOrder.DropDownItems.Count > 0) {
                mnuLoadingOrder.DropDownItems[0].Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            }

            #endregion

        }

        /// <summary>
        /// Update edit app info and icon for Edit Image menu
        /// </summary>
        private void UpdateEditAppInfoForMenu() {
            var appName = "";
            mnuMainEditImage.Image = null;

            // Temporary memory data
            if (!Local.IsTempMemoryData) {
                // Find file format
                var ext = Path.GetExtension(Local.ImageList.GetFileName(Local.CurrentIndex)).ToLower();
                var app = Configs.GetEditApp(ext);

                // Get EditApp info
                if (app != null) {
                    appName = $"({app.AppName})";

                    try {
                        // Update icon
                        var ico = Icon.ExtractAssociatedIcon(app.AppPath);
                        var iconWidth = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);

                        mnuMainEditImage.Image = new Bitmap(ico.ToBitmap(), iconWidth, iconWidth);
                    }
                    catch { }
                }
            }

            mnuMainEditImage.Text = string.Format(Configs.Language.Items[$"{Name}.mnuMainEditImage"], appName);
        }

        /// <summary>
        /// Select and Active Zoom Mode, use GlobalSetting.ZoomMode
        /// </summary>
        private void SelectUIZoomMode() {
            // Reset (Disable) Zoom Lock
            Configs.ZoomLockValue = 100.0;

            btnAutoZoom.Checked = mnuMainAutoZoom.Checked =
                btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked =
                btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked =
                btnZoomLock.Checked = mnuMainLockZoomRatio.Checked =
                btnScaleToFit.Checked = mnuMainScaleToFit.Checked =
                btnScaleToFill.Checked = mnuMainScaleToFill.Checked = false;

            switch (Configs.ZoomMode) {
                case ZoomMode.ScaleToFit:
                    btnScaleToFit.Checked = mnuMainScaleToFit.Checked = true;
                    break;

                case ZoomMode.ScaleToWidth:
                    btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked = true;
                    break;

                case ZoomMode.ScaleToHeight:
                    btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked = true;
                    break;

                case ZoomMode.LockZoomRatio:
                    btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = true;

                    // Enable Zoom Lock
                    Configs.ZoomLockValue = picMain.Zoom;
                    break;

                case ZoomMode.ScaleToFill:
                    mnuMainScaleToFill.Checked = btnScaleToFill.Checked = true;
                    break;

                case ZoomMode.AutoZoom:
                default:
                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// Apply zoom mode
        /// </summary>
        /// <param name="zoomMode"></param>
        /// <param name="isResetScrollPosition"></param>
        private void ApplyZoomMode(ZoomMode zoomMode, bool isResetScrollPosition = true) {
            if (picMain.Image == null) {
                return;
            }

            // Reset scrollbar position
            if (isResetScrollPosition) {
                picMain.ScrollTo(0, 0, 0, 0);
            }

            double frac;
            switch (zoomMode) {
                case ZoomMode.ScaleToWidth:
                    frac = picMain.Width / (1f * picMain.Image.Width);
                    picMain.Zoom = frac * 100;
                    break;

                case ZoomMode.ScaleToHeight:
                    frac = picMain.Height / (1f * picMain.Image.Height);
                    picMain.Zoom = frac * 100;
                    break;

                case ZoomMode.ScaleToFit:
                    picMain.ZoomToFit();
                    break;

                case ZoomMode.LockZoomRatio:
                    picMain.Zoom = Configs.ZoomLockValue;
                    break;

                case ZoomMode.ScaleToFill:
                    var widthRatio = picMain.Width / (1f * picMain.Image.Width);
                    var heightRatio = picMain.Height / (1f * picMain.Image.Height);

                    if (widthRatio > heightRatio) {
                        frac = picMain.Width / (1f * picMain.Image.Width);
                    }
                    else {
                        frac = picMain.Height / (1f * picMain.Image.Height);
                    }

                    picMain.Zoom = frac * 100;
                    break;

                case ZoomMode.AutoZoom:
                default:
                    picMain.ZoomAuto();
                    break;
            }

            if (Configs.IsCenterImage) {
                // auto center the image
                picMain.CenterToImage();
            }

            // Tell the app that it's not zoomed by user
            _isManuallyZoomed = false;

            // Get image file information
            SetStatusBar();
        }

        /// <summary>
        /// Start Zoom optimization
        /// </summary>
        private void ZoomOptimization() {
            if (Configs.ZoomOptimizationMethod == ZoomOptimizationMethods.Auto) {
                if (picMain.Zoom > 100) {
                    picMain.InterpolationMode = InterpolationMode.NearestNeighbor;
                }
                else if (picMain.Zoom < 100) {
                    picMain.InterpolationMode = InterpolationMode.Low;
                }
            }
            else {
                picMain.InterpolationMode = (InterpolationMode)Configs.ZoomOptimizationMethod;
            }
        }

        /// <summary>
        /// Rename image
        /// </summary>
        private void RenameImage() {
            try {
                if (Local.ImageError != null || !File.Exists(Local.ImageList.GetFileName(Local.CurrentIndex))) {
                    return;
                }
            }
            catch { return; }

            // Fix issue #397. Original logic didn't take network paths into account.
            // Replace original logic with the Path functions to access filename bits.

            // Extract the various bits of the image path
            var filepath = Local.ImageList.GetFileName(Local.CurrentIndex);
            var currentFolder = Path.GetDirectoryName(filepath);
            var oldName = Path.GetFileName(filepath);
            var ext = Path.GetExtension(filepath);
            var newName = Path.GetFileNameWithoutExtension(filepath);

            // Show input box
            string str = null;

            if (InputBox.ShowDialog(
                theme: Configs.Theme,
                message: Configs.Language.Items[$"{Name}._RenameDialog"],
                defaultValue: newName,
                title: Configs.Language.Items[$"{Name}._RenameDialogText"],
                topMost: TopMost,
                isFilename: true) == DialogResult.OK) {
                str = InputBox.Message;
            }

            if (string.IsNullOrWhiteSpace(str)) {
                return;
            }

            newName = str + ext;

            // duplicated name
            if (oldName == newName) {
                return;
            }

            try {
                var newFilePath = Path.Combine(currentFolder, newName);
                // Rename file
                ImageInfo.RenameFile(filepath, newFilePath);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Edit Comment
        /// </summary>
        private void EditComment() {
            try {
                if (Local.ImageError != null || !File.Exists(Local.ImageList.GetFileName(Local.CurrentIndex))) {
                    return;
                }
            }
            catch { return; }

            // Fix issue #397. Original logic didn't take network paths into account.
            // Replace original logic with the Path functions to access filename bits.

            // Extract the various bits of the image path
            var filepath = Local.ImageList.GetFileName(Local.CurrentIndex);
            var ext = Path.GetExtension(filepath).ToLower();
            string str = null;
            if (ext != ".jpg" & ext != ".jpeg") {
                if (InputBox.ShowDialog(
                    theme: Configs.Theme,
                    message: "Not jpg or jpeg file",
                    defaultValue: "",
                    title: "Alert",
                    topMost: TopMost,
                    isFilename: true) == DialogResult.OK) { }
                return;
            }

            string oldcomment = "";
            var so = ShellObject.FromParsingName(filepath);
            var _usercomment = so.Properties.GetProperty(SystemProperties.System.Comment);
            if (_usercomment.ValueAsObject != null) {
                oldcomment = _usercomment.ValueAsObject.ToString();
            }

                
            // Show input box

            if (InputBox.ShowDialog(
                theme: Configs.Theme,
                message: Configs.Language.Items[$"{Name}._EditComment"],
                defaultValue: oldcomment,
                title: Configs.Language.Items[$"{Name}._EditCommentText"],
                topMost: TopMost,
                isFilename: true) == DialogResult.OK) {
                str = InputBox.Message;
            }

            if (string.IsNullOrWhiteSpace(str)) {
                return;
            }

            string newComment = str;

            // duplicated name
            if (oldcomment == newComment) {
                return;
            }

            try {
                ShellPropertyWriter propertyWriter = so.Properties.GetPropertyWriter();
                propertyWriter.WriteProperty(SystemProperties.System.Comment, new string[] { InputBox.Message });
                propertyWriter.Close();
                Clipboard.SetText(oldcomment);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Display a toast message on picture box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="duration">Duration (milisecond)</param>
        private async void ShowToastMsg(string msg, int duration, int delay = 0) {
            if (!Configs.IsShowToast) return;

            if (InvokeRequired) {
                Invoke(new Action<string, int, int>(ShowToastMsg), msg, duration, delay);
                return;
            }

            if (duration == 0) {
                picMain.TextBackColor = Color.Transparent;
                picMain.Font = Font;
                picMain.ForeColor = Theme.InvertBlackAndWhiteColor(Configs.BackgroundColor);
                picMain.Text = string.Empty;
                return;
            }

            var timToast = new Timer {
                Enabled = false,
                Interval = duration, // display in xxx miliseconds
            };
            timToast.Tick += TimerToast_Tick;

            picMain.TextBackColor = Color.Black;
            picMain.Font = new Font(Font.FontFamily, 12);
            picMain.ForeColor = Color.White;
            picMain.Text = msg;

            await Task.Delay(delay);

            // Start timer
            timToast.Enabled = true;
            timToast.Start();
        }

        /// <summary>
        /// Timer Tick event: to display the message
        /// </summary>
        private void TimerToast_Tick(object sender, EventArgs e) {
            var timToast = (Timer)sender;
            timToast.Stop();
            timToast.Tick -= TimerToast_Tick;
            timToast.Dispose();

            picMain.TextBackColor = Color.Transparent;
            picMain.Font = Font;
            picMain.ForeColor = Color.Black;
            picMain.Text = string.Empty;
        }

        /// <summary>
        /// Copy multiple files
        /// </summary>
        private void CopyMultiFiles() {
            // get filename
            var filename = Local.ImageList.GetFileName(Local.CurrentIndex);

            try {
                if (Local.ImageError != null || !File.Exists(filename)) {
                    return;
                }
            }
            catch { return; }

            //update the list
            var fileList = new List<string>();
            fileList.AddRange(Local.StringClipboard);

            for (var i = 0; i < fileList.Count; i++) {
                if (!File.Exists(fileList[i])) {
                    Local.StringClipboard.Remove(fileList[i]);
                }
            }

            // exit if duplicated filename
            if (Local.StringClipboard.IndexOf(filename) == -1) {
                // add filename to clipboard
                Local.StringClipboard.Add(filename);
            }

            var fileDropList = new StringCollection();
            fileDropList.AddRange(Local.StringClipboard.ToArray());

            Clipboard.Clear();
            Clipboard.SetFileDropList(fileDropList);

            ShowToastMsg(
                string.Format(Configs.Language.Items[$"{Name}._CopyFileText"],
                Local.StringClipboard.Count), 1000);
        }

        /// <summary>
        /// Cut multiple files
        /// </summary>
        private async Task CutMultiFilesAsync() {
            // get filename
            var filename = Local.ImageList.GetFileName(Local.CurrentIndex);

            try {
                if (Local.ImageError != null || !File.Exists(filename)) {
                    return;
                }
            }
            catch { return; }

            // update the list
            var fileList = new List<string>();
            fileList.AddRange(Local.StringClipboard);

            for (var i = 0; i < fileList.Count; i++) {
                if (!File.Exists(fileList[i])) {
                    Local.StringClipboard.Remove(fileList[i]);
                }
            }

            // exit if duplicated filename
            if (Local.StringClipboard.IndexOf(filename) == -1) {
                // add filename to clipboard
                Local.StringClipboard.Add(filename);
            }

            var moveEffect = new byte[] { 2, 0, 0, 0 };
            using (var dropEffect = new MemoryStream()) {
                await dropEffect.WriteAsync(moveEffect, 0, moveEffect.Length).ConfigureAwait(true);
                var fileDropList = new StringCollection();
                fileDropList.AddRange(Local.StringClipboard.ToArray());
                var data = new DataObject();
                data.SetFileDropList(fileDropList);
                data.SetData("Preferred DropEffect", dropEffect);
                Clipboard.Clear();
                Clipboard.SetDataObject(data, true);
            }

            ShowToastMsg(
                string.Format(Configs.Language.Items[$"{Name}._CutFileText"],
                Local.StringClipboard.Count), 1000);
        }

        /// <summary>
        /// Save all change of image
        /// </summary>
        /// <param name="showError"></param>
        /// <returns></returns>
        private async Task SaveImageChangeAsync(bool showError = false) {
            // use backup name to avoid variable conflict
            var filename = Local.ImageModifiedPath;

            _ = SetAppBusyAsync(true, string.Format(Configs.Language.Items[$"{Name}._SavingImage"], filename));

            try {
                var lastWriteTime = File.GetLastWriteTime(filename);
                Bitmap newBitmap;

                if (!picMain.SelectionRegion.IsEmpty) {
                    newBitmap = new Bitmap(picMain.GetSelectedImage());
                }
                else {
                    newBitmap = new Bitmap(picMain.Image);
                }

                await Task.Run(() => {
                    // override the current image file
                    Heart.Photo.Save(newBitmap, filename, quality: Configs.ImageEditQuality);

                    // Issue #307: option to preserve the modified date/time
                    if (Configs.IsPreserveModifiedDate) {
                        File.SetLastWriteTime(filename, lastWriteTime);
                    }
                });

                // update cache of the modified item
                var img = await Local.ImageList.GetImgAsync(Local.CurrentIndex).ConfigureAwait(true);
                img.Image = newBitmap;
            }
            catch (Exception ex) {
                if (showError) {
                    MessageBox.Show(string.Format(Configs.Language.Items[$"{Name}._SaveImageError"], filename) + "\r\n\r\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Local.ImageModifiedPath = "";
            await SetAppBusyAsync(false);
        }

        /// <summary>
        /// Load image data
        /// </summary>
        /// <param name="img"></param>
        private void LoadImageData(Image img) {
            picMain.Image = img;
            picMain.Text = "";
            Local.IsTempMemoryData = true;

            ApplyZoomMode(Configs.ZoomMode);

            SetStatusBar();
        }

        /// <summary>
        /// Save current loaded image to file
        /// </summary>
        private string SaveTemporaryMemoryData() {
            var tempDir = App.ConfigDir(PathType.Dir, Dir.Temporary);
            if (!Directory.Exists(tempDir)) {
                Directory.CreateDirectory(tempDir);
            }

            var filename = Path.Combine(tempDir, "temp_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".png");

            picMain.Image.Save(filename, ImageFormat.Png);

            return filename;
        }

        /// <summary>
        /// Determine the image sort order/direction based on user settings
        /// or Windows Explorer sorting.
        /// <param name="fullPath">full path to file/folder (i.e. as comes in from drag-and-drop)</param>
        /// <side_effect>Updates GlobalSetting.ActiveImageLoadingOrder</side_effect>
        /// <side_effect>Updates LocalSetting.ActiveImageLoadingOrder</side_effect>
        /// <side_effect>Updates LocalSetting.ActiveImageLoadingOrderType</side_effect>
        /// </summary>
        private static void DetermineSortOrder(string fullPath) {
            // Initialize to the user-configured sorting order. Fetching the Explorer sort
            // order may fail, or may be on an unsupported column.
            Local.ActiveImageLoadingOrder = Configs.ImageLoadingOrder;
            Local.ActiveImageLoadingOrderType = Configs.ImageLoadingOrderType;

            // Use File Explorer sort order if possible
            if (Configs.IsUseFileExplorerSortOrder) {
                if (ExplorerSortOrder.GetExplorerSortOrder(fullPath, out var explorerOrder, out var isAscending)) {
                    if (explorerOrder != null) {
                        Local.ActiveImageLoadingOrder = explorerOrder.Value;
                    }

                    if (isAscending != null) {
                        Local.ActiveImageLoadingOrderType = isAscending.Value ? ImageOrderType.Asc : ImageOrderType.Desc;
                    }
                }
            }
        }

        /// <summary>
        /// Load View Channels menu items
        /// </summary>
        private void LoadViewChannelsMenuItems() {
            // clear items
            mnuMainChannels.DropDown.Items.Clear();

            var newMenuIconHeight = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);

            // add new items
            foreach (var channel in Enum.GetValues(typeof(ColorChannels))) {
                var channelName = Enum.GetName(typeof(ColorChannels), channel);
                var mnu = new ToolStripRadioButtonMenuItem() {
                    Text = Configs.Language.Items[$"{Name}.mnuMainChannels._{channelName}"],
                    Tag = channel,
                    CheckOnClick = true,
                    Checked = (int)channel == (int)Local.Channels,
                    ImageScaling = ToolStripItemImageScaling.None,
                    Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
                };

                mnu.Click += MnuViewChannelsItem_Click;
                mnuMainChannels.DropDown.Items.Add(mnu);
            }
        }

        private void MnuViewChannelsItem_Click(object sender, EventArgs e) {
            var mnu = sender as ToolStripMenuItem;
            var selectedChannel = (ColorChannels)(int)mnu.Tag;

            if (selectedChannel != Local.Channels) {
                Local.Channels = selectedChannel;
                Local.ImageList.Channels = (int)selectedChannel;

                // update the viewing image
                _ = NextPicAsync(0, true, true);

                // update cached images
                Local.ImageList.UpdateCache();

                // reload state
                LoadViewChannelsMenuItems();
            }
        }


        /// <summary>
        /// Load Loading order menu items
        /// </summary>
        private void LoadLoadingOrderMenuItems() {
            // clear items
            mnuLoadingOrder.DropDown.Items.Clear();

            var newMenuIconHeight = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);

            // add ImageOrderBy items
            foreach (var order in Enum.GetValues(typeof(ImageOrderBy))) {
                var orderName = Enum.GetName(typeof(ImageOrderBy), order);
                var mnu = new ToolStripRadioButtonMenuItem() {
                    Text = Configs.Language.Items[$"_.{nameof(ImageOrderBy)}._{orderName}"],
                    Tag = order,
                    CheckOnClick = true,
                    Checked = (int)order == (int)Configs.ImageLoadingOrder,
                    ImageScaling = ToolStripItemImageScaling.None,
                    Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
                };

                mnu.Click += MnuLoadingOrderItem_Click;
                mnuLoadingOrder.DropDown.Items.Add(mnu);
            }

            mnuLoadingOrder.DropDown.Items.Add(new ToolStripSeparator());

            // add ImageOrderType items
            foreach (var orderType in Enum.GetValues(typeof(ImageOrderType))) {
                var typeName = Enum.GetName(typeof(ImageOrderType), orderType);
                var mnu = new ToolStripRadioButtonMenuItem() {
                    Text = Configs.Language.Items[$"_.{nameof(ImageOrderType)}._{typeName}"],
                    Tag = orderType,
                    CheckOnClick = true,
                    Checked = (int)orderType == (int)Configs.ImageLoadingOrderType,
                    ImageScaling = ToolStripItemImageScaling.None,
                    Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
                };

                mnu.Click += MnuLoadingOrderTypeItem_Click;
                mnuLoadingOrder.DropDown.Items.Add(mnu);
            }
        }

        private void MnuLoadingOrderItem_Click(object sender, EventArgs e) {
            var mnu = sender as ToolStripMenuItem;
            var selectedOrder = (ImageOrderBy)(int)mnu.Tag;


            if (selectedOrder != Configs.ImageLoadingOrder) {
                Configs.ImageLoadingOrder = selectedOrder;

                // reload image list
                MnuMainReloadImageList_Click(null, null);

                // reload the state
                LoadLoadingOrderMenuItems();
            }
        }

        private void MnuLoadingOrderTypeItem_Click(object sender, EventArgs e) {
            var mnu = sender as ToolStripMenuItem;
            var selectedType = (ImageOrderType)(int)mnu.Tag;


            if (selectedType != Configs.ImageLoadingOrderType) {
                Configs.ImageLoadingOrderType = selectedType;

                // reload image list
                MnuMainReloadImageList_Click(null, null);

                // reload the state
                LoadLoadingOrderMenuItems();
            }
        }


        /// <summary>
        /// Load toolbar configs and update the buttons
        /// </summary>
        private void UpdateToolbarButtons() {
            toolMain.Items.Clear();

            // Update size of toolbar
            var newBtnHeight = (int)Math.Floor(toolMain.Height * 0.8);

            // get correct icon height
            var hIcon = DPIScaling.Transform<uint>(Configs.ToolbarIconHeight);


            foreach (var item in Configs.ToolbarButtons) {
                if (item == ToolbarButton.Separator) {
                    toolMain.Items.Add(new ToolStripSeparator {
                        AutoSize = false,
                        Height = (int)(hIcon * 1.2),
                    });
                }
                else {
                    try {
                        var info = typeof(frmMain).GetField(item.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
                        var btn = info.GetValue(this) as ToolStripItem;

                        // update the item size
                        btn.Size = new Size(newBtnHeight, newBtnHeight);

                        // add item to toolbar
                        toolMain.Items.Add(btn);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Adjust our window dimensions to fit the image size.
        /// </summary>
        private void WindowFitMode(bool reZoom = true) {
            if (!Configs.IsWindowFit || picMain.Image == null)
                return; // Nothing to do

            #region Set minimum size for window
            var minH = DPIScaling.Transform(100);
            if (Configs.IsShowToolBar) {
                minH += toolMain.Height;
            }

            if (Configs.IsShowThumbnail) {
                minH += (int)Configs.ThumbnailBarWidth;
            }

            MinimumSize = new() {
                Width = DPIScaling.Transform(200),
                Height = minH,
            };
            #endregion

            // If the user selects "Display view scrollbars" setting, the imageviewer will
            // try to take them into account [especially for tall images]. Override said
            // setting in this mode.
            var oldScrollSetting = picMain.HorizontalScrollBarStyle;
            picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
            picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Hide;

            WindowState = FormWindowState.Normal;

            // get current screen
            var screen = Screen.FromControl(this);

            // Check for early exits
            // This fixes issue(https://github.com/d2phap/ImageGlass/issues/1371)
            // If window size already reached max, then can't be expanded more larger
            if (picMain.Width * picMain.ZoomFactor > screen.WorkingArea.Width &&
                picMain.Height * picMain.ZoomFactor > screen.WorkingArea.Height &&
                Size.Width == screen.WorkingArea.Width &&
                Size.Height == screen.WorkingArea.Height) {
                return;
            }


            // First, adjust our main window to theoretically fit the entire
            // picture, but not larger than desktop working area.
            var fullW = Width + picMain.Image.Width - picMain.Width;
            var fullH = Height + picMain.Image.Height - picMain.Height;

            var maxWidth = Math.Min(fullW, screen.WorkingArea.Width);
            var maxHeight = Math.Min(fullH, screen.WorkingArea.Height);
            Size = new Size(Width = maxWidth, Height = maxHeight);

            // Let the image viewer control figure out the zoom value for
            // the full-size window
            if (reZoom) {
                ApplyZoomMode(Configs.ZoomMode);
            }

            // Now that we have the new zoom value, adjust our main window
            // to fit the *zoomed* image size
            var newW = (int)(picMain.Image.Width * picMain.ZoomFactor);
            var newH = (int)(picMain.Image.Height * picMain.ZoomFactor);

            // Adjust our main window to theoretically fit the entire
            // picture, but not larger than desktop working area.
            fullW = Width + newW - picMain.Width;
            fullH = Height + newH - picMain.Height;

            maxWidth = Math.Min(fullW, screen.WorkingArea.Width);
            maxHeight = Math.Min(fullH, screen.WorkingArea.Height);
            Size = new Size(Width = maxWidth, Height = maxHeight);

            // Scroll to last position
            if (!reZoom) {
                picMain.ScrollTo(picMain.PointToImage(picMain.CenterPoint), picMain.CenterPoint);
            }

            // center window to screen
            if (Configs.IsCenterWindowFit) {
                App.CenterFormToScreen(this);
            }

            picMain.Bounds = new Rectangle(0, 0, newW, newH);

            // Restore the user's "Display viewer scrollbars" setting.
            picMain.HorizontalScrollBarStyle = oldScrollSetting;
            picMain.VerticalScrollBarStyle = oldScrollSetting;
        }

        /// <summary>
        /// Paint countdown clock in Slideshow mode
        /// </summary>
        private void PaintSlideshowClock(PaintEventArgs e) {
            if (!timSlideShow.Enabled || !Configs.IsShowSlideshowCountdown) {
                return;
            }

            // draw countdown text ----------------------------------------------
            var countdownTime = TimeSpan.FromSeconds(_slideshowCountdown + 1);
            var text = (countdownTime - _slideshowStopwatch.Elapsed).ToString("mm'∶'ss");

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var textBrush = new SolidBrush(Color.FromArgb(150, Theme.InvertBlackAndWhiteColor(picMain.BackColor)));
            var font = new Font(Font.FontFamily, 30f);
            var fontSize = e.Graphics.MeasureString(text, font);

            // calculate background size
            var gap = DPIScaling.Transform(20);
            var bgSize = new SizeF(fontSize.Width + gap, fontSize.Height + gap);
            var bgX = picMain.Width - bgSize.Width - gap;
            var bgY = picMain.Height - bgSize.Height - gap;

            // calculate text size
            var fontX = bgX + (bgSize.Width / 2) - (fontSize.Width / 2);
            var fontY = bgY + (bgSize.Height / 2) - (fontSize.Height / 2);

            // draw background
            var borderRadius = Helpers.IsOS(WindowsOS.Win11) ? 10 : 1;
            using var bgBrush = new SolidBrush(Color.FromArgb(150, picMain.BackColor));
            using var path = Theme.GetRoundRectanglePath(new RectangleF(bgX, bgY, bgSize.Width, bgSize.Height), borderRadius);
            e.Graphics.FillPath(bgBrush, path);

            // draw countdown text
            e.Graphics.DrawString(text, font, textBrush, fontX, fontY);
        }

        /// <summary>
        /// Handle page navigation event
        /// </summary>
        /// <param name="navEvent"></param>
        private void PageNavigationEvent(frmPageNav.NavEvent navEvent) {
            switch (navEvent) {
                case frmPageNav.NavEvent.PageFirst:
                    mnuMainFirstPage_Click(null, null);
                    break;

                case frmPageNav.NavEvent.PageNext:
                    mnuMainNextPage_Click(null, null);
                    break;

                case frmPageNav.NavEvent.PagePrevious:
                    mnuMainPrevPage_Click(null, null);
                    break;

                case frmPageNav.NavEvent.PageLast:
                    mnuMainLastPage_Click(null, null);
                    break;
            }
        }

        /// <summary>
        /// Handle cropping tool event
        /// </summary>
        /// <param name="actionEvent"></param> 
        private void CropActionEvent(frmCrop.CropActionEvent actionEvent) {
            switch (actionEvent) {
                case frmCrop.CropActionEvent.Save:
                    _ = SaveImageChangeAsync();
                    break;
                case frmCrop.CropActionEvent.SaveAs:
                    mnuMainSaveAs_Click(null, null);
                    break;
                case frmCrop.CropActionEvent.Copy:
                    mnuMainCopyImageData_Click(null, null);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Show or hide Color picker tool
        /// </summary>
        /// <param name="show"></param>
        private void ShowColorPickerTool(bool show = true) {
            Local.IsColorPickerToolOpening =
                mnuMainColorPicker.Checked = show;

            // open Color Picker tool
            if (show) {
                if (Local.FColorPicker.IsDisposed) {
                    Local.FColorPicker = new frmColorPicker();
                }

                Local.FColorPicker.SetToolFormManager(_toolManager);
                Local.FColorPicker.Owner = this;
                Local.ForceUpdateActions |= ForceUpdateActions.COLOR_PICKER_MENU;
                Local.FColorPicker.SetImageBox(picMain);

                if (!Local.FColorPicker.Visible) {
                    Local.FColorPicker.Show(this);
                }

                Activate();
            }
            else {
                // Close Color picker tool
                Local.FColorPicker?.Close();
            }
        }

        /// <summary>
        /// Show or hide Page naviagtion tool
        /// </summary>
        /// <param name="show"></param>
        private void ShowPageNavTool(bool show = true) {
            Local.IsPageNavToolOpenning =
                mnuMainPageNav.Checked = show;

            if (!Configs.IsShowPageNavAuto) {
                Configs.IsShowPageNavOnStartup = show;
            }

            if (show) {
                // Open the page navigation tool
                if (Local.FPageNav?.IsDisposed != false) {
                    Local.FPageNav = new frmPageNav();
                }

                // register page event handler
                Local.FPageNav.NavEventHandler = PageNavigationEvent;
                Local.ForceUpdateActions |= ForceUpdateActions.PAGE_NAV_MENU;
                Local.FPageNav.SetToolFormManager(_toolManager);
                Local.FPageNav.Owner = this;

                if (!Local.FPageNav.Visible) {
                    Local.FPageNav.Show(this);
                    SetStatusBar();
                }

                Activate();
            }
            else {
                if (Local.FPageNav != null) {
                    // Close the page navigation tool
                    Local.FPageNav.Close();
                    Local.FPageNav.NavEventHandler = null;
                }
            }
        }

        /// <summary>
        /// Enable / disable Crop tool
        /// </summary>
        /// <param name="show"></param>
        public void ShowCropTool(bool show = true) {
            btnCrop.Checked = mnuMainCrop.Checked = show;
            picMain.SelectionMode = ImageBoxSelectionMode.None;
            picMain.SelectNone();

            // show Cropping mode
            if (show) {
                picMain.SelectionMode = ImageBoxSelectionMode.Rectangle;
                picMain.SelectionRegion = new RectangleF();

                // Open the page navigation tool
                if (Local.FCrop?.IsDisposed != false) {
                    Local.FCrop = new frmCrop();
                }

                // register page event handler
                Local.FCrop.CropEventHandler = CropActionEvent;
                Local.FCrop.SetToolFormManager(_toolManager);
                Local.FCrop.Owner = this;
                Local.FCrop.SetImageBox(picMain);

                if (!Local.FCrop.Visible) {
                    Local.FCrop.Show(this);
                }

                Activate();
            }
            else {
                if (Local.FCrop != null) {
                    // Close Crop tool
                    Local.FCrop.Close();
                    Local.FCrop.CropEventHandler = null;
                }
            }
        }

        /// <summary>
        /// Gets navigation regions
        /// </summary>
        /// <returns></returns>
        private List<NavigationRegion> GetNavigationRegions() {
            // get the nav region area width
            var width = Math.Max(Configs.Theme.NavArrowLeft.Height, picMain.Width / 10);

            return new List<NavigationRegion> {
                new NavigationRegion() {
                    Type = NavigationRegionType.Left,
                    Region = new Rectangle(0, 0, width, picMain.Height),
                },
                new NavigationRegion() {
                    Type = NavigationRegionType.Right,
                    Region = new Rectangle(picMain.Width - width, 0, width, picMain.Height),
                }
            };
        }

        /// <summary>
        /// Test if the given point is one of the left and right navigation regions
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private NavigationRegion TestCursorHitNavRegions(Point point) {
            if (!Configs.IsShowNavigationButtons || picMain.IsPanning)
                return null;

            var item = Local.NavRegions.Find(item => item.Region.Contains(point));

            // the given point is not in the hit regions
            if (item == null)
                return null;

            // no loopback
            if (!Configs.IsLoopBackViewer) {
                // disable left arrow on first image
                if (item.Type == NavigationRegionType.Left && Local.CurrentIndex == 0)
                    return null;

                // disable right arrow on last image
                if (item.Type == NavigationRegionType.Right && Local.CurrentIndex >= Local.ImageList.Length - 1)
                    return null;
            }

            return item;
        }

        /// <summary>
        /// Paint left-right navigation regions
        /// </summary>
        /// <param name="e"></param>
        private void PaintNavigationRegions(PaintEventArgs e) {
            // get current cursor position on frmMain
            var pos = PointToClient(MousePosition);
            var navRegion = TestCursorHitNavRegions(pos);

            // check if the hotspot hit
            if (navRegion == null || navRegion.Type == NavigationRegionType.Unknown) return;

            var region = navRegion.Region;
            Image icon;

            // expand rectangle by 1px to fit the drawable region
            region.Offset(-1, -1);
            region.Inflate(1, 1);

            if (navRegion.Type == NavigationRegionType.Left) {
                icon = Configs.Theme.NavArrowLeft;
            }
            else { // right
                icon = Configs.Theme.NavArrowRight;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.SetClip(region);

            var iconPosX = region.X + (region.Width / 2) - (icon.Width / 2);
            var iconPosY = (region.Height / 2) - (icon.Width / 2);

            // draw circle background for icon
            using var sBrush = new SolidBrush(Color.FromArgb(180, Configs.Theme.ToolbarBackgroundColor));
            e.Graphics.FillEllipse(sBrush, new RectangleF(iconPosX, iconPosY, icon.Width, icon.Height));

            // draw arrow icon
            e.Graphics.DrawImage(icon, iconPosX, iconPosY);

            e.Graphics.ResetClip();
        }

        /// <summary>
        /// Invoke action after opening editing app
        /// </summary>
        private void RunActionAfterEditing() {
            if (Configs.AfterEditingAction == AfterOpeningEditAppAction.Minimize) {
                foreach (var frm in Application.OpenForms) {
                    (frm as Form).WindowState = FormWindowState.Minimized;
                }
            }
            else if (Configs.AfterEditingAction == AfterOpeningEditAppAction.Close) {
                Exit();
            }
        }

        #endregion


        #region Configurations

        /// <summary>
        /// Apply ImageGlass theme
        /// </summary>
        private void ApplyTheme(bool changeBackground = false) {
            var th = Configs.Theme;

            // ThumbnailBar Renderer must be done BEFORE loading theme
            var thumbBarTheme = new ModernThumbnailRenderer(th);
            thumbnailBar.SetRenderer(thumbBarTheme);

            // Apply theme
            Configs.ApplyFormTheme(this, Configs.Theme);
            tray.Icon = Icon;

            // Remove white line under tool strip
            toolMain.Renderer = new ModernToolStripRenderer(th);

            if (changeBackground) {
                // User is changing theme. Override BackgroundColor setting.
                picMain.BackColor = th.BackgroundColor;
                Configs.BackgroundColor = th.BackgroundColor;
            }

            picMain.GridColor = Color.FromArgb(15, 0, 0, 0);
            picMain.GridColorAlternate = Color.FromArgb(20, 255, 255, 255);

            toolMain.BackgroundImage = th.ToolbarBackgroundImage.Image;
            toolMain.BackColor = th.ToolbarBackgroundColor;

            thumbnailBar.BackgroundImage = th.ThumbnailBackgroundImage.Image;
            thumbnailBar.BackColor = th.ThumbnailBackgroundColor;
            sp1.BackColor = th.ThumbnailBackgroundColor;

            lblInfo.ForeColor = th.TextInfoColor;
            picMain.ForeColor = Theme.InvertBlackAndWhiteColor(Configs.BackgroundColor);

            // Modern UI menu renderer
            mnuMain.Renderer =
                mnuShortcut.Renderer =
                mnuContext.Renderer =
                mnuTray.Renderer = new ModernMenuRenderer(th);


            // <toolbar_icon>
            LoadToolbarIcons();

            // Overflow button and Overflow dropdown
            toolMain.OverflowButton.DropDown.BackColor = th.ToolbarBackgroundColor;
            toolMain.OverflowButton.AutoSize = false;
            toolMain.OverflowButton.Padding = new Padding(DPIScaling.Transform(10));
        }

        /// <summary>
        /// Load toolbar icons
        /// </summary>
        private void LoadToolbarIcons(bool forceReloadIcon = false) {
            if (forceReloadIcon) {
                Configs.Theme.ReloadIcons((int)Configs.ToolbarIconHeight);
            }

            var th = Configs.Theme;

            // <toolbar_icon>
            btnBack.Image = th.ToolbarIcons.ViewPreviousImage.Image;
            btnNext.Image = th.ToolbarIcons.ViewNextImage.Image;

            btnRotateLeft.Image = th.ToolbarIcons.RotateLeft.Image;
            btnRotateRight.Image = th.ToolbarIcons.RotateRight.Image;
            btnFlipHorz.Image = th.ToolbarIcons.FlipHorz.Image;
            btnFlipVert.Image = th.ToolbarIcons.FlipVert.Image;
            btnDelete.Image = th.ToolbarIcons.Delete.Image;
            btnEdit.Image = th.ToolbarIcons.Edit.Image;
            btnCrop.Image = th.ToolbarIcons.Crop.Image;
            btnColorPicker.Image = th.ToolbarIcons.ColorPicker.Image;

            btnZoomIn.Image = th.ToolbarIcons.ZoomIn.Image;
            btnZoomOut.Image = th.ToolbarIcons.ZoomOut.Image;
            btnScaleToFit.Image = th.ToolbarIcons.ScaleToFit.Image;
            btnActualSize.Image = th.ToolbarIcons.ActualSize.Image;
            btnZoomLock.Image = th.ToolbarIcons.LockRatio.Image;
            btnAutoZoom.Image = th.ToolbarIcons.AutoZoom.Image;
            btnScaletoWidth.Image = th.ToolbarIcons.ScaleToWidth.Image;
            btnScaletoHeight.Image = th.ToolbarIcons.ScaleToHeight.Image;
            btnScaleToFill.Image = th.ToolbarIcons.ScaleToFill.Image;
            btnWindowFit.Image = th.ToolbarIcons.AdjustWindowSize.Image;

            btnOpen.Image = th.ToolbarIcons.OpenFile.Image;
            btnRefresh.Image = th.ToolbarIcons.Refresh.Image;
            btnGoto.Image = th.ToolbarIcons.GoToImage.Image;

            btnThumb.Image = th.ToolbarIcons.ThumbnailBar.Image;
            btnCheckedBackground.Image = th.ToolbarIcons.Checkerboard.Image;
            btnFullScreen.Image = th.ToolbarIcons.FullScreen.Image;
            btnSlideShow.Image = th.ToolbarIcons.Slideshow.Image;
            btnConvert.Image = th.ToolbarIcons.Convert.Image;
            btnPrintImage.Image = th.ToolbarIcons.Print.Image;

            btnMenu.Image = th.ToolbarIcons.Menu.Image;
        }

        /// <summary>
        /// If true is passed, try to use a 10ms system clock for animating GIFs, otherwise
        /// use the default animator.
        /// </summary>
        private void CheckAnimationClock(bool isUsingFasterClock) {
            if (isUsingFasterClock) {
                if (!TimerAPI.HasRequestedRateAtLeastAsFastAs(10) && TimerAPI.TimeBeginPeriod(10))
                    HighResolutionGifAnimator.SetTickTimeInMilliseconds(10);
                picMain.Animator = new HighResolutionGifAnimator();
            }
            else {
                if (TimerAPI.HasRequestedRateAlready(10))
                    TimerAPI.TimeEndPeriod(10);
                picMain.Animator = new DefaultGifAnimator();
            }
        }

        /// <summary>
        /// Load app configurations
        /// </summary>
        private void LoadConfig(bool @isLoadUI = false, bool @isLoadOthers = true) {
            #region UI SETTINGS
            if (isLoadUI) {
                ApplyTheme();

                // Show checkerboard
                Configs.IsShowCheckerBoard = !Configs.IsShowCheckerBoard;
                mnuMainCheckBackground_Click(null, EventArgs.Empty);

                // background color
                picMain.BackColor = Configs.BackgroundColor;

                // Load state of Toolbar 
                Configs.IsShowToolBar = !Configs.IsShowToolBar;
                mnuMainToolbar_Click(null, EventArgs.Empty);

                Application.DoEvents();

                // Load scrollbars visibility
                if (Configs.IsScrollbarsVisible) {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                }

                // Toolbar alignment and position
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR_POSITION;
                frmMain_Activated(null, EventArgs.Empty);

                // NOTE: ***
                // Need to load the Windows state here to fix the issue:
                // https://github.com/d2phap/ImageGlass/issues/358
                // And to IMPROVE the startup loading speed.
                var testWindowBound = Configs.FrmMainWindowBound;
                testWindowBound.Inflate(-10, -10);

                if (Helpers.IsVisibleOnAnyScreen(testWindowBound)) {
                    Bounds = Configs.FrmMainWindowBound;
                }
                else {
                    // The saved position no longer exists (e.g. 2d monitor removed).
                    // Prevent us from appearing off-screen.
                    StartPosition = FormStartPosition.WindowsDefaultLocation;
                }

                // Load state of Thumbnail
                Local.ForceUpdateActions |= ForceUpdateActions.THUMBNAIL_BAR;
                frmMain_Activated(null, EventArgs.Empty);


                #region Load Frameless mode
                _movableForm = new MovableForm(this) {
                    Key = Keys.ShiftKey | Keys.Shift,
                    FreeMoveControlNames = new HashSet<string>()
                    {
                        nameof(toolMain),
                        nameof(thumbnailBar),
                    },
                };

                if (Configs.IsWindowFrameless) {
                    Configs.IsWindowFrameless = !Configs.IsWindowFrameless;
                    mnuFrameless.PerformClick();
                }
                #endregion

            }
            #endregion

            #region OTHER SETTINGS
            if (isLoadOthers) {
                // NOTE: ***
                // This is a 'UI' setting which isLoadUI had previously skipped. *However*,
                // the windows *Position* is the one UI setting which *must* be applied at
                // the OnLoad event in order to 'take'.

                // Windows Bound (Position + Size)
                Bounds = Configs.FrmMainWindowBound;

                if (!Program.IsHideWindow) {
                    // Windows state must be loaded after Windows Bound!
                    WindowState = Configs.FrmMainWindowState;
                }

                // Load Toolbar buttons
                // *** Need to trigger after 'this.Bounds'
                Local.ForceUpdateActions |= ForceUpdateActions.TOOLBAR;

                // force update language pack
                Local.ForceUpdateActions |= ForceUpdateActions.LANGUAGE;
                frmMain_Activated(null, null);

                #region Load Zoom Mode

                // Load and Active Zoom Mode
                picMain.Zoom = Configs.ZoomLockValue;

                SelectUIZoomMode();

                // Load ZoomLevels
                picMain.ZoomLevels = new ImageBoxZoomLevelCollection(Configs.ZoomLevels);

                #endregion

                // Load Color picker configs
                if (Configs.IsShowColorPickerOnStartup) {
                    ShowColorPickerTool();
                }

                // Load Page navigation tool
                if (Configs.IsShowPageNavOnStartup) {
                    ShowPageNavTool();
                }

                // Load Full Screen mode
                if (Configs.IsFullScreen) {
                    Configs.IsFullScreen = !Configs.IsFullScreen;
                    mnuMainFullScreen.PerformClick();
                }

                // Load WindowFit mode
                if (Configs.IsWindowFit) {
                    Configs.IsWindowFit = !Configs.IsWindowFit;
                    mnuWindowFit.PerformClick();
                }

                #region Get Last Seen Image Path & Welcome Image
                var startUpImg = Configs.IsOpenLastSeenImage ? Configs.LastSeenImagePath : "";

                if (!File.Exists(startUpImg) && Configs.IsShowWelcome) {
                    startUpImg = App.StartUpDir("default.jpg");
                }

                // Do not show welcome image if params exist.
                var args = Environment.GetCommandLineArgs();
                var argCount = args.Where(a => !a.StartsWith("-")).Count();
                if (argCount < 2) {
                    PrepareLoading(startUpImg);
                }
                #endregion


                // Load state of IsWindowAlwaysOnTop value 
                TopMost = mnuMainAlwaysOnTop.Checked = Configs.IsWindowAlwaysOnTop;

                // Load state of WindowFit mode setting
                mnuWindowFit.Checked = Configs.IsWindowFit;
                WindowFitMode();

                // hide window
                if (Program.IsHideWindow) {
                    _ = ToggleAppVisibilityAsync(false);
                }

                // auto-focus on hover
                toolMain.AutoFocus = Configs.AutoFocusToolbarOnHover;
            }
            #endregion

        }

        /// <summary>
        /// Save app configurations
        /// </summary>
        /// <param name="windowStateOnly">Only save window state</param>
        private void SaveConfig(bool windowStateOnly = false) {
            #region Window state and bounds
            // Windows Bound---------------------------------------------
            // don't save Bound if in Full screen and SlideShow mode
            if (!Configs.IsFullScreen && !Configs.IsSlideshow) {
                if (WindowState == FormWindowState.Normal) {
                    Configs.FrmMainWindowBound = Bounds;
                }
                else if (WindowState == FormWindowState.Maximized) {
                    // if moving a maximized window from a screen to other screen
                    // and keep its maximized state, we need to save the "Normal"
                    // location so that it restores to correct screen in Normal state
                    // in the next run.
                    var newLocation = Location;
                    newLocation.Offset(110, 110);

                    Configs.FrmMainWindowBound = new Rectangle(
                        newLocation,
                        Configs.FrmMainWindowBound.Size);
                }
            }

            // Windows State----------------------------------------------
            Configs.FrmMainWindowState = WindowState != FormWindowState.Minimized ? WindowState : FormWindowState.Normal;

            if (windowStateOnly) {
                return;
            }
            #endregion


            // Save thumbnail bar width
            Configs.ThumbnailBarWidth = (uint)(sp1.Width - sp1.SplitterDistance);

            // Save last seen image path
            Configs.LastSeenImagePath = Local.ImageList.GetFileName(Local.CurrentIndex);
        }

        /// <summary>
        /// Enter or Exit Full screen mode
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="changeWindowState"></param>
        /// <param name="hideToolbar">Hide Toolbar</param>
        /// <param name="hideThumbnailBar">Hide Thumbnail bar</param>
        private void SetFullScreenMode(bool enabled = true, bool changeWindowState = true, bool hideToolbar = false, bool hideThumbnailBar = false) {
            // full screen
            if (enabled) {
                SaveConfig(windowStateOnly: true);

                _isFrameless = Configs.IsWindowFrameless;
                _isWindowFit = Configs.IsWindowFit;

                // exit WindowFit mode
                Configs.IsWindowFit = true;
                mnuWindowFit_Click(null, null);

                // exit frameless window
                Configs.IsWindowFrameless = true;
                mnuFrameless_Click(null, null);

                // save last state of layout
                if (hideToolbar) {
                    _isShowToolbar = Configs.IsShowToolBar;
                }
                if (hideThumbnailBar) {
                    _isShowThumbnail = Configs.IsShowThumbnail;
                }

                if (changeWindowState) {
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Normal;
                    Bounds = Screen.FromControl(this).Bounds;
                }

                // Hide toolbar
                if (hideToolbar) {
                    toolMain.Visible = false;
                    Configs.IsShowToolBar = false;
                    mnuMainToolbar.Checked = false;
                }
                // hide thumbnail
                if (hideThumbnailBar) {
                    Configs.IsShowThumbnail = true;
                    mnuMainThumbnailBar_Click(null, null);
                }

                Application.DoEvents();

                // realign image
                if (!_isManuallyZoomed) {
                    ApplyZoomMode(Configs.ZoomMode);
                }
            }

            // exit full screen
            else {
                // restore last state of toolbar
                if (hideToolbar) {
                    Configs.IsShowToolBar = _isShowToolbar;
                }
                if (hideThumbnailBar) {
                    Configs.IsShowThumbnail = _isShowThumbnail;
                }

                // restore background color in case of being overriden by SlideShow mode
                picMain.BackColor = Configs.BackgroundColor;

                if (changeWindowState) {
                    FormBorderStyle = FormBorderStyle.Sizable;

                    // windows state
                    if (Configs.FrmMainWindowState != FormWindowState.Minimized) {
                        WindowState = Configs.FrmMainWindowState;
                    }

                    // Windows Bound (Position + Size)
                    Bounds = Configs.FrmMainWindowBound;
                }

                // restore frameless state
                Configs.IsWindowFrameless = _isFrameless;
                if (Configs.IsWindowFrameless) {
                    // trigger frameless window
                    Configs.IsWindowFrameless = false;
                    mnuFrameless_Click(null, null);
                }

                if (hideToolbar && Configs.IsShowToolBar) {
                    // Show toolbar
                    toolMain.Visible = true;
                    mnuMainToolbar.Checked = true;
                }
                if (hideThumbnailBar && Configs.IsShowThumbnail) {
                    // Show thumbnail
                    Configs.IsShowThumbnail = false;
                    mnuMainThumbnailBar_Click(null, null);
                }

                // restore WindowFit mode state
                Configs.IsWindowFit = _isWindowFit;
                if (Configs.IsWindowFit) {
                    Configs.IsWindowFit = false;
                    mnuWindowFit_Click(null, null);
                }

                Application.DoEvents();

                // Update toolbar icon according to the new size
                LoadToolbarIcons(forceReloadIcon: true);

                toolMain.UpdateAlignment();

                // realign image
                if (!_isManuallyZoomed) {
                    ApplyZoomMode(Configs.ZoomMode);
                }
            }
        }

        /// <summary>
        /// Exits application
        /// </summary>
        /// <param name="force"></param>
        private void Exit(bool force = false) {
            _forceExitApp = force;
            Application.Exit();
        }

        #endregion


        #region Form events

        protected override CreateParams CreateParams {
            get {
                // minimizable borderless form
                const int WS_MINIMIZEBOX = 0x20000;

                var cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;

                return cp;
            }
        }

        protected override void WndProc(ref Message m) {
            var touchHandled = false;

            // Check if the received message is WM_SHOWME
            if (m.Msg == NativeMethods.WM_SHOWME) {
                // Set frmMain of the first instance to TopMost
                if (WindowState == FormWindowState.Minimized) {
                    WindowState = FormWindowState.Normal;
                }

                // Issue #620: using TopMost/Focus doesn't give focus
                BringToFront();
                Activate();
            }

            // This message is sent when the form is dragged to a different monitor i.e. when
            // the bigger part of its are is on the new monitor. 
            else if (m.Msg == DPIScaling.WM_DPICHANGED) {
                DPIScaling.CurrentDPI = DPIScaling.LOWORD((int)m.WParam);
                OnDpiChanged();
            }

            // WM_SYSCOMMAND
            else if (m.Msg == 0x0112) {
                // When user clicks on MAXIMIZE button on title bar
                if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
                {
                    // The window is being maximized
                    if (!_isManuallyZoomed) {
                        ApplyZoomMode(Configs.ZoomMode);
                    }
                }
                // When user clicks on the RESTORE button on title bar
                else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
                {
                    // The window is being restored
                    if (!_isManuallyZoomed) {
                        ApplyZoomMode(Configs.ZoomMode);
                    }
                }
            }

            // Touch support
            else if (m.Msg == Touch.WM_GESTURENOTIFY && Configs.IsUseTouchGesture) {
                touchHandled = Touch.AcceptTouch(this);
            }

            // Touch support
            else if (m.Msg == Touch.WM_GESTURE && Configs.IsUseTouchGesture) {
                touchHandled = Touch.DecodeTouch(m, out var act);

                switch (act) {
                    case Touch.Action.SwipeLeft:
                        _ = NextPicAsync(1);
                        break;
                    case Touch.Action.SwipeRight:
                        _ = NextPicAsync(-1);
                        break;
                    case Touch.Action.RotateCCW:
                        mnuMainRotateCounterclockwise_Click(null, null);
                        break;
                    case Touch.Action.RotateCW:
                        mnuMainRotateClockwise_Click(null, null);
                        break;
                    case Touch.Action.ZoomIn:
                        // Zoom in to a specific position
                        for (var i = 0; i < Touch.ZoomFactor; i++)
                            picMain.ProcessMouseZoom(true, Touch.ZoomLocation);
                        break;
                    case Touch.Action.ZoomOut:
                        // Zoom out to a specific position
                        for (var i = 0; i < Touch.ZoomFactor; i++)
                            picMain.ProcessMouseZoom(false, Touch.ZoomLocation);
                        break;
                    case Touch.Action.SwipeUp:
                        btnZoomOut_Click(null, null);
                        break;
                    case Touch.Action.SwipeDown:
                        btnZoomIn_Click(null, null);
                        break;
                }
            }

            // Window frameless resizing
            else if (m.Msg == 0x0084 && Configs.IsWindowFrameless) {
                base.WndProc(ref m);

                if ((int)m.Result == 0x01) // HTCLIENT
                {
                    var screenPoint = new Point(m.LParam.ToInt32());
                    var clientPoint = PointToClient(screenPoint);

                    const int RESIZE_HANDLE_SIZE = 10;

                    if (clientPoint.Y <= RESIZE_HANDLE_SIZE) {
                        if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                            m.Result = (IntPtr)13; // HTTOPLEFT
                        else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                            m.Result = (IntPtr)12; // HTTOP
                        else
                            m.Result = (IntPtr)14; // HTTOPRIGHT
                    }
                    else if (clientPoint.Y <= (Size.Height - RESIZE_HANDLE_SIZE)) {
                        if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                            m.Result = (IntPtr)10; // HTLEFT
                        else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                            m.Result = (IntPtr)2; // HTCAPTION
                        else
                            m.Result = (IntPtr)11; // HTRIGHT
                    }
                    else {
                        if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                            m.Result = (IntPtr)16; // HTBOTTOMLEFT
                        else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                            m.Result = (IntPtr)15; // HTBOTTOM
                        else
                            m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    }
                }
                return;
            }

            // State changed
            else if (m.Msg == 0x0005) // WM_SIZE
            {
            }

            base.WndProc(ref m);

            if (touchHandled)
                m.Result = new IntPtr(1);
        }

        private void frmMain_Load(object sender, EventArgs e) {
            // Listening to image change event
            Local.OnImageChanged += Local_OnImageChanged;

            // Load Other Configs
            LoadConfig(isLoadUI: false, isLoadOthers: true);

            // Trigger Mouse Wheel event
            picMain.MouseWheel += picMain_MouseWheel;

            // Try to use a faster image clock for animating GIFs
            CheckAnimationClock(true);

            // Load image from param
            LoadFromParams(Environment.GetCommandLineArgs());

            // Start thread to watching deleted files
            var thDeleteWorker = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadWatcherDeleteFiles)) {
                Priority = System.Threading.ThreadPriority.BelowNormal,
                IsBackground = true,
            };
            thDeleteWorker.Start();
        }

        public void LoadFromParams(string[] args) {
            // Load image from param
            if (args.Length >= 2) {
                for (var i = 1; i < args.Length; i++) {
                    // only read the path, exclude configs parameter which starts with "--"
                    if (!args[i].StartsWith("-")) {
                        PrepareLoading(args[i]);
                        break;
                    }
                }
            }
        }

        private async void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            // wait for task done
            if (Local.IsBusy) {
                e.Cancel = true;
                return;
            }

            // continue running background
            if (!_forceExitApp
                && Configs.IsContinueRunningBackground
                && e.CloseReason != CloseReason.WindowsShutDown
                && e.CloseReason != CloseReason.TaskManagerClosing) {

                // allow to exit if there are multiple instances running
                if (Configs.IsAllowMultiInstances) {
                    var processCount = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;

                    if (processCount > 1) {
                        await PrepareToExitAppAsync();

                        return;
                    }
                }

                // cancel closing requests by user
                e.Cancel = true;

                // hide the app
                _ = ToggleAppVisibilityAsync(false);
            }

            // close the app
            else {
                await PrepareToExitAppAsync();
            }
        }

        public async Task ToggleAppVisibilityAsync(bool show) {
            tray.Visible = !show;

            if (show) {
                ShowInTaskbar = true;
                Visible = true;
                WindowState = Configs.FrmMainWindowState;
            }
            else {
                SaveConfig();
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Visible = false;

                // Write user configs file
                Configs.Write();

                // Save image if it was modified
                if (File.Exists(Local.ImageModifiedPath) && Configs.IsSaveAfterRotating) {
                    await SaveImageChangeAsync(true);
                }
                Application.DoEvents();

                if (!Visible) {
                    // Dispose all garbage
                    Local.ImageList.Dispose();
                    Local.CurrentIndex = -1;
                    Local.CurrentPageCount = 0;
                    Local.CurrentPageIndex = 0;
                    Local.CurrentExif = null;
                    Local.CurrentColor = null;

                    thumbnailBar.Items.Clear();
                    picMain.Image = null;
                    picMain.Text = "";
                    SetStatusBar();

                    // Collect system garbage
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
        }

        private async Task PrepareToExitAppAsync() {
            try {
                // release resource of the file system watcher
                _fileWatcher.Dispose();

                // Save image if it was modified
                if (File.Exists(Local.ImageModifiedPath) && Configs.IsSaveAfterRotating) {
                    await SaveImageChangeAsync(true);
                }

                // clear temp files
                var tempDir = App.ConfigDir(PathType.Dir, Dir.Temporary);
                if (Directory.Exists(tempDir)) {
                    Directory.Delete(tempDir, true);
                }

                SaveConfig();

                // start with os
                Helper.SetStartWithOS(Configs.IsStartWithOs);

                // Write user configs file
                Configs.Write();
            }
            catch { }

            tray.Visible = false;
        }

        private void frmMain_Activated(object sender, EventArgs e) {
            var flags = Local.ForceUpdateActions;

            // do nothing
            if (flags == ForceUpdateActions.NONE) return;

            #region LANGUAGE
            if ((flags & ForceUpdateActions.LANGUAGE) == ForceUpdateActions.LANGUAGE) {
                var lang = Configs.Language.Items;

                #region Update language strings

                #region Main menu

                #region Menu File
                mnuMainFile.Text = lang[$"{Name}.{nameof(mnuMainFile)}"];
                mnuMainOpenFile.Text = lang[$"{Name}.{nameof(mnuMainOpenFile)}"];
                mnuMainOpenImageData.Text = lang[$"{Name}.{nameof(mnuMainOpenImageData)}"];
                mnuMainNewWindow.Text = lang[$"{Name}.{nameof(mnuMainNewWindow)}"];
                mnuSaveImage.Text = lang[$"{Name}.{nameof(mnuSaveImage)}"];
                mnuMainSaveAs.Text = lang[$"{Name}.{nameof(mnuMainSaveAs)}"];
                mnuMainRefresh.Text = lang[$"{Name}.{nameof(mnuMainRefresh)}"];
                mnuMainReloadImage.Text = lang[$"{Name}.{nameof(mnuMainReloadImage)}"];
                mnuMainReloadImageList.Text = lang[$"{Name}.{nameof(mnuMainReloadImageList)}"];
                mnuOpenWith.Text = lang[$"{Name}.{nameof(mnuOpenWith)}"];
                mnuMainEditImage.Text = lang[$"{Name}.{nameof(mnuMainEditImage)}"];
                mnuMainPrint.Text = lang[$"{Name}.{nameof(mnuMainPrint)}"];
                #endregion

                #region Menu Navigation
                mnuMainNavigation.Text = lang[$"{Name}.{nameof(mnuMainNavigation)}"];
                mnuMainViewNext.Text = lang[$"{Name}.{nameof(mnuMainViewNext)}"];
                mnuMainViewNext.ShortcutKeyDisplayString = lang[$"{Name}.{nameof(mnuMainViewNext)}.Shortcut"];
                mnuMainViewPrevious.Text = lang[$"{Name}.{nameof(mnuMainViewPrevious)}"];
                mnuMainViewPrevious.ShortcutKeyDisplayString = lang[$"{Name}.{nameof(mnuMainViewPrevious)}.Shortcut"];

                mnuMainGoto.Text = lang[$"{Name}.{nameof(mnuMainGoto)}"];
                mnuMainGotoFirst.Text = lang[$"{Name}.{nameof(mnuMainGotoFirst)}"];
                mnuMainGotoLast.Text = lang[$"{Name}.{nameof(mnuMainGotoLast)}"];

                mnuMainNextPage.Text = lang[$"{Name}.{nameof(mnuMainNextPage)}"];
                mnuMainPrevPage.Text = lang[$"{Name}.{nameof(mnuMainPrevPage)}"];
                mnuMainFirstPage.Text = lang[$"{Name}.{nameof(mnuMainFirstPage)}"];
                mnuMainLastPage.Text = lang[$"{Name}.{nameof(mnuMainLastPage)}"];
                #endregion

                #region Menu Zoom
                mnuMainZoom.Text = lang[$"{Name}.{nameof(mnuMainZoom)}"];
                mnuMainZoomIn.Text = lang[$"{Name}.{nameof(mnuMainZoomIn)}"];
                mnuMainZoomOut.Text = lang[$"{Name}.{nameof(mnuMainZoomOut)}"];
                mnuCustomZoom.Text = lang[$"{Name}.{nameof(mnuCustomZoom)}"];
                mnuMainScaleToFit.Text = lang[$"{Name}.{nameof(mnuMainScaleToFit)}"];
                mnuMainScaleToFill.Text = lang[$"{Name}.{nameof(mnuMainScaleToFill)}"];
                mnuMainActualSize.Text = lang[$"{Name}.{nameof(mnuMainActualSize)}"];
                mnuMainLockZoomRatio.Text = lang[$"{Name}.{nameof(mnuMainLockZoomRatio)}"];
                mnuMainAutoZoom.Text = lang[$"{Name}.{nameof(mnuMainAutoZoom)}"];
                mnuMainScaleToWidth.Text = lang[$"{Name}.{nameof(mnuMainScaleToWidth)}"];
                mnuMainScaleToHeight.Text = lang[$"{Name}.{nameof(mnuMainScaleToHeight)}"];
                mnuWindowFit.Text = lang[$"{Name}.{nameof(mnuWindowFit)}"];
                #endregion

                #region Menu Image
                mnuMainImage.Text = lang[$"{Name}.{nameof(mnuMainImage)}"];
                mnuMainRotateLeft.Text = lang[$"{Name}.{nameof(mnuMainRotateLeft)}"];
                mnuMainRotateRight.Text = lang[$"{Name}.{nameof(mnuMainRotateRight)}"];
                mnuMainFlipHorz.Text = lang[$"{Name}.{nameof(mnuMainFlipHorz)}"];
                mnuMainFlipVert.Text = lang[$"{Name}.{nameof(mnuMainFlipVert)}"];

                mnuMainRename.Text = lang[$"{Name}.{nameof(mnuMainRename)}"];
                mnuMainComment.Text = lang[$"{Name}.{nameof(mnuMainComment)}"];
                mnuMainMoveToRecycleBin.Text = lang[$"{Name}.{nameof(mnuMainMoveToRecycleBin)}"];
                mnuMainDeleteFromHardDisk.Text = lang[$"{Name}.{nameof(mnuMainDeleteFromHardDisk)}"];
                mnuMainExtractPages.Text = lang[$"{Name}.{nameof(mnuMainExtractPages)}"];
                mnuMainStartStopAnimating.Text = lang[$"{Name}.{nameof(mnuMainStartStopAnimating)}"];
                mnuMainSetAsDesktop.Text = lang[$"{Name}.{nameof(mnuMainSetAsDesktop)}"];
                mnuMainSetAsLockImage.Text = lang[$"{Name}.{nameof(mnuMainSetAsLockImage)}"];
                mnuMainImageLocation.Text = lang[$"{Name}.{nameof(mnuMainImageLocation)}"];
                mnuMainImageProperties.Text = lang[$"{Name}.{nameof(mnuMainImageProperties)}"];

                mnuMainChannels.Text = lang[$"{Name}.{nameof(mnuMainChannels)}"];
                LoadViewChannelsMenuItems(); // update Channels menu items

                mnuLoadingOrder.Text = lang[$"{Name}.{nameof(mnuLoadingOrder)}"];
                LoadLoadingOrderMenuItems(); // update Loading order items
                #endregion

                #region Menu CLipboard
                mnuMainClipboard.Text = lang[$"{Name}.{nameof(mnuMainClipboard)}"];
                mnuMainCopy.Text = lang[$"{Name}.{nameof(mnuMainCopy)}"];
                mnuMainCopyImageData.Text = lang[$"{Name}.{nameof(mnuMainCopyImageData)}"];
                mnuMainCut.Text = lang[$"{Name}.{nameof(mnuMainCut)}"];
                mnuMainCopyImagePath.Text = lang[$"{Name}.{nameof(mnuMainCopyImagePath)}"];
                mnuMainClearClipboard.Text = lang[$"{Name}.{nameof(mnuMainClearClipboard)}"];
                #endregion

                #region Menu Slideshow
                mnuMainSlideShow.Text = lang[$"{Name}.{nameof(mnuMainSlideShow)}"];
                mnuMainSlideShowStart.Text = lang[$"{Name}.{nameof(mnuMainSlideShowStart)}"];
                mnuMainSlideShowPause.Text = lang[$"{Name}.{nameof(mnuMainSlideShowPause)}"];
                mnuMainSlideShowExit.Text = lang[$"{Name}.{nameof(mnuMainSlideShowExit)}"];
                #endregion

                #region Menu Layout
                mnuMainLayout.Text = lang[$"{Name}.{nameof(mnuMainLayout)}"];
                mnuMainToolbar.Text = lang[$"{Name}.{nameof(mnuMainToolbar)}"];
                mnuMainThumbnailBar.Text = lang[$"{Name}.{nameof(mnuMainThumbnailBar)}"];
                mnuMainCheckBackground.Text = lang[$"{Name}.{nameof(mnuMainCheckBackground)}"];
                mnuMainAlwaysOnTop.Text = lang[$"{Name}.{nameof(mnuMainAlwaysOnTop)}"];
                #endregion

                #region Menu Tools
                mnuMainTools.Text = lang[$"{Name}.{nameof(mnuMainTools)}"];
                mnuMainColorPicker.Text = lang[$"{Name}.{nameof(mnuMainColorPicker)}"];
                mnuMainPageNav.Text = lang[$"{Name}.{nameof(mnuMainPageNav)}"];
                mnuMainCrop.Text = lang[$"{Name}.{nameof(mnuMainCrop)}"];
                mnuExifTool.Text = lang[$"{Name}.{nameof(mnuExifTool)}"];
                #endregion

                #region Menu Help
                mnuMainHelp.Text = lang[$"{Name}.{nameof(mnuMainHelp)}"];
                mnuMainAbout.Text = lang[$"{Name}.{nameof(mnuMainAbout)}"];
                mnuMainFirstLaunch.Text = lang[$"{Name}.{nameof(mnuMainFirstLaunch)}"];
                mnuMainReportIssue.Text = lang[$"{Name}.{nameof(mnuMainReportIssue)}"];
                #endregion

                mnuMainFullScreen.Text = lang[$"{Name}.{nameof(mnuMainFullScreen)}"];
                mnuFrameless.Text = lang[$"{Name}.{nameof(mnuFrameless)}"];
                mnuMainShare.Text = lang[$"{Name}.{nameof(mnuMainShare)}"];

                mnuMainSettings.Text = lang[$"{Name}.{nameof(mnuMainSettings)}"];
                mnuMainExitApplication.Text = lang[$"{Name}.{nameof(mnuMainExitApplication)}"];

                #endregion

                #region Toolbar

                btnBack.ToolTipText = mnuMainViewPrevious.Text + $" ({mnuMainViewPrevious.ShortcutKeyDisplayString})";
                btnNext.ToolTipText = mnuMainViewNext.Text + $" ({mnuMainViewNext.ShortcutKeyDisplayString})";

                // Edit
                btnRotateLeft.ToolTipText = mnuMainRotateLeft.Text + $" ({mnuMainRotateLeft.ShortcutKeyDisplayString})";
                btnRotateRight.ToolTipText = mnuMainRotateRight.Text + $" ({mnuMainRotateRight.ShortcutKeyDisplayString})";
                btnFlipHorz.ToolTipText = mnuMainFlipHorz.Text + $" ({mnuMainFlipHorz.ShortcutKeyDisplayString})";
                btnFlipVert.ToolTipText = mnuMainFlipVert.Text + $" ({mnuMainFlipVert.ShortcutKeyDisplayString})";
                btnDelete.ToolTipText = mnuMainMoveToRecycleBin.Text + $" ({mnuMainMoveToRecycleBin.ShortcutKeyDisplayString})";
                btnEdit.ToolTipText = string.Format(mnuMainEditImage.Text, "") + $" ({mnuMainEditImage.ShortcutKeyDisplayString})";
                btnCrop.ToolTipText = string.Format(mnuMainCrop.Text, "") + $" ({mnuMainCrop.ShortcutKeyDisplayString})";
                btnColorPicker.ToolTipText = string.Format(mnuMainColorPicker.Text, "") + $" ({mnuMainColorPicker.ShortcutKeyDisplayString})";

                // Zooming
                btnZoomIn.ToolTipText = mnuMainZoomIn.Text + $" ({mnuMainZoomIn.ShortcutKeyDisplayString})";
                btnZoomOut.ToolTipText = mnuMainZoomOut.Text + $" ({mnuMainZoomOut.ShortcutKeyDisplayString})";
                btnActualSize.ToolTipText = mnuMainActualSize.Text + $" ({mnuMainActualSize.ShortcutKeyDisplayString})";

                // Zoom modes
                btnAutoZoom.ToolTipText = mnuMainAutoZoom.Text + $" ({mnuMainAutoZoom.ShortcutKeyDisplayString})";
                btnScaletoWidth.ToolTipText = mnuMainScaleToWidth.Text + $" ({mnuMainScaleToWidth.ShortcutKeyDisplayString})";
                btnScaletoHeight.ToolTipText = mnuMainScaleToHeight.Text + $" ({mnuMainScaleToHeight.ShortcutKeyDisplayString})";
                btnScaleToFit.ToolTipText = mnuMainScaleToFit.Text + $" ({mnuMainScaleToFit.ShortcutKeyDisplayString})";
                btnScaleToFill.ToolTipText = mnuMainScaleToFill.Text + $" ({mnuMainScaleToFill.ShortcutKeyDisplayString})";
                btnZoomLock.ToolTipText = mnuMainLockZoomRatio.Text + $" ({mnuMainLockZoomRatio.ShortcutKeyDisplayString})";

                // Window modes
                btnWindowFit.ToolTipText = mnuWindowFit.Text + $" ({mnuWindowFit.ShortcutKeyDisplayString})";
                btnFullScreen.ToolTipText = mnuMainFullScreen.Text + $" ({mnuMainFullScreen.ShortcutKeyDisplayString})";
                btnSlideShow.ToolTipText = mnuMainSlideShowStart.Text + $" ({mnuMainSlideShowStart.ShortcutKeyDisplayString})";

                // File
                btnOpen.ToolTipText = mnuMainOpenFile.Text + $" ({mnuMainOpenFile.ShortcutKeyDisplayString})";
                btnRefresh.ToolTipText = mnuMainRefresh.Text + $" ({mnuMainRefresh.ShortcutKeyDisplayString})";
                btnGoto.ToolTipText = mnuMainGoto.Text + $" ({mnuMainGoto.ShortcutKeyDisplayString})";

                // Layout
                btnThumb.ToolTipText = mnuMainThumbnailBar.Text + $" ({mnuMainThumbnailBar.ShortcutKeyDisplayString})";
                btnCheckedBackground.ToolTipText = mnuMainCheckBackground.Text + $" ({mnuMainCheckBackground.ShortcutKeyDisplayString})";
                btnConvert.ToolTipText = mnuMainSaveAs.Text + $" ({mnuMainSaveAs.ShortcutKeyDisplayString})";
                btnPrintImage.ToolTipText = mnuMainPrint.Text + $" ({mnuMainPrint.ShortcutKeyDisplayString})";
                btnMenu.ToolTipText = lang[$"{Name}.{nameof(btnMenu)}"] + " (Alt+F)";


                #endregion

                #endregion

                //Update language layout ------------------
                RightToLeft = Configs.Language.IsRightToLeftLayout;
            }
            #endregion

            #region THUMBNAIL_BAR or THUMBNAIL_ITEMS
            if ((flags & ForceUpdateActions.THUMBNAIL_BAR) == ForceUpdateActions.THUMBNAIL_BAR || (flags & ForceUpdateActions.THUMBNAIL_ITEMS) == ForceUpdateActions.THUMBNAIL_ITEMS) {
                // Update thumbnail bar position
                Configs.IsShowThumbnail = !Configs.IsShowThumbnail;
                mnuMainThumbnailBar_Click(null, null);

                // Update thumbnail bar scroll bar visibility
                thumbnailBar.ScrollBars = Configs.IsShowThumbnailScrollbar;
            }
            #endregion

            #region THUMBNAIL_ITEMS
            if ((flags & ForceUpdateActions.THUMBNAIL_ITEMS) == ForceUpdateActions.THUMBNAIL_ITEMS) {
                //Update thumbnail image size
                LoadThumbnails();
            }
            #endregion

            #region COLOR_PICKER_MENU
            if ((flags & ForceUpdateActions.COLOR_PICKER_MENU) == ForceUpdateActions.COLOR_PICKER_MENU) {
                btnColorPicker.Checked =
                    mnuMainColorPicker.Checked =
                    Local.IsColorPickerToolOpening;
            }
            #endregion

            #region PAGE_NAV_MENU
            if ((flags & ForceUpdateActions.PAGE_NAV_MENU) == ForceUpdateActions.PAGE_NAV_MENU) {
                mnuMainPageNav.Checked = Local.IsPageNavToolOpenning;
            }
            #endregion

            #region THEME
            if ((flags & ForceUpdateActions.THEME) == ForceUpdateActions.THEME) {
                ApplyTheme(changeBackground: true);
                Local.FColorPicker.UpdateUI();
                Local.FPageNav.UpdateUI();
                Local.FCrop.UpdateUI();
            }
            #endregion

            #region TOOLBAR
            if ((flags & ForceUpdateActions.TOOLBAR) == ForceUpdateActions.TOOLBAR) {
                UpdateToolbarButtons();
                toolMain.Items.Add(btnMenu);
                toolMain.Items.Add(lblInfo);

                toolMain.UpdateAlignment();
            }
            #endregion

            #region TOOLBAR_POSITION
            if ((flags & ForceUpdateActions.TOOLBAR_POSITION) == ForceUpdateActions.TOOLBAR_POSITION) {
                if (Configs.ToolbarPosition == ToolbarPosition.Top) {
                    toolMain.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    toolMain.Dock = DockStyle.Top;
                    toolMain.ToolTipShowUp = false;
                }
                else if (Configs.ToolbarPosition == ToolbarPosition.Bottom) {
                    toolMain.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                    toolMain.Dock = DockStyle.Bottom;
                    toolMain.ToolTipShowUp = true;
                }

                // update toolbar items alignment
                toolMain.Alignment = Configs.IsCenterToolbar ? ToolbarAlignment.CENTER : ToolbarAlignment.LEFT;

                // Hide toolbar tooltips
                toolMain.HideTooltips = Configs.IsHideTooltips;
            }
            #endregion

            #region TOOLBAR_ICON_HEIGHT
            if ((flags & ForceUpdateActions.TOOLBAR_ICON_HEIGHT) == ForceUpdateActions.TOOLBAR_ICON_HEIGHT) {
                // Update size of toolbar
                DPIScaling.TransformToolbar(ref toolMain, (int)Configs.ToolbarIconHeight);

                // Update toolbar icon according to the new size
                LoadToolbarIcons(forceReloadIcon: true);

                toolMain.UpdateAlignment();
            }
            #endregion

            #region IMAGE_LIST
            if ((flags & ForceUpdateActions.IMAGE_LIST) == ForceUpdateActions.IMAGE_LIST) {
                // update image loading order value
                LoadLoadingOrderMenuItems();

                // update image list
                MnuMainReloadImageList_Click(null, null);
            }
            #endregion

            #region IMAGE_LIST_NO_RECURSIVE
            if ((flags & ForceUpdateActions.IMAGE_LIST_NO_RECURSIVE) == ForceUpdateActions.IMAGE_LIST_NO_RECURSIVE) {
                // update image list with the initial input path
                _ = PrepareLoadingAsync(new string[] { Local.InitialInputPath }, Local.ImageList.GetFileName(Local.CurrentIndex));
            }
            #endregion

            #region OTHER_SETTINGS
            if ((flags & ForceUpdateActions.OTHER_SETTINGS) == ForceUpdateActions.OTHER_SETTINGS) {
                #region Update Other Settings

                // Update scrollbars visibility
                if (Configs.IsScrollbarsVisible) {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                }
                else {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
                }

                // update checkerboard display mode
                if (Configs.IsShowCheckerBoard) {
                    Configs.IsShowCheckerBoard = false;
                    mnuMainCheckBackground_Click(null, null);
                }

                // update navigation regions
                if (Configs.IsShowNavigationButtons) {
                    Local.NavRegions = GetNavigationRegions();
                }

                // Update background
                picMain.BackColor = Configs.BackgroundColor;

                // Update ZoomLevels
                picMain.ZoomLevels = new ImageBoxZoomLevelCollection(Configs.ZoomLevels);

                ApplyZoomMode(Configs.ZoomMode);

                #endregion

            }
            #endregion

            #region Windows 10 Specific Actions
            var isWin81OrLater = true;
            var winVersion = Environment.OSVersion;

            // Win7 == 6.1, Win Server 2008 == 6.1
            // Win10 == 10.0 [if app.manifest properly configured]
            if (winVersion.Version.Major < 6 ||
                (winVersion.Version.Major == 6 &&
                winVersion.Version.Minor < 2)) {
                isWin81OrLater = false; // Not running Windows 8 or earlier
            }

            mnuMainSetAsLockImage.Enabled = isWin81OrLater;
            #endregion

            Local.ForceUpdateActions = ForceUpdateActions.NONE;
        }


        private void frmMain_Resize(object sender, EventArgs e) {
            if (WindowState != _windowState) {
                _windowState = WindowState;
                if (WindowState == FormWindowState.Normal) {
                    // Restored

                    // Update toolbar icon according to the new size
                    LoadToolbarIcons(forceReloadIcon: true);

                    toolMain.UpdateAlignment();
                }
            }
        }

        private void frmMain_ResizeBegin(object sender, EventArgs e) {
            _windowSize = Size;
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e) {
            if (Size != _windowSize) {
                SaveConfig(windowStateOnly: true);
            }
        }

        private void frmMain_SizeChanged(object sender, EventArgs e) {
            if (!_isManuallyZoomed) {
                ApplyZoomMode(Configs.ZoomMode);
            }
        }

        private void thumbnailBar_ItemClick(object sender, ImageListView.ItemClickEventArgs e) {
            if (e.Buttons == MouseButtons.Left) {
                Local.CurrentIndex = e.Item.Index;
                _ = NextPicAsync(0);
            }
        }

        private void timSlideShow_Tick(object sender, EventArgs e) {
            if (!_slideshowStopwatch.IsRunning)
                _slideshowStopwatch.Restart();

            if (_slideshowStopwatch.Elapsed.TotalMilliseconds >= TimeSpan.FromSeconds(_slideshowCountdown).TotalMilliseconds) {
                // end of image list
                if (Local.CurrentIndex == Local.ImageList.Length - 1) {
                    // loop the list
                    if (!Configs.IsLoopBackSlideshow) {
                        // pause slideshow
                        mnuMainSlideShowPause_Click(null, null);
                        return;
                    }
                }

                _ = NextPicAsync(1);
            }


            // only update the countdown text if it's a full second number
            var isSecond = _slideshowStopwatch.Elapsed.Milliseconds <= 100;
            if (Configs.IsShowSlideshowCountdown && isSecond) {
                picMain.Invalidate();
            }
        }

        private void PicMain_Paint(object sender, PaintEventArgs e) {
            // draw slideshow clock
            PaintSlideshowClock(e);

            // draw navigation regions
            PaintNavigationRegions(e);
        }

        #region File System Watcher events

        private void FileWatcher_OnRenamed(object sender, FileChangedEvent e) {
            if (InvokeRequired) {
                Invoke(new Action<object, FileChangedEvent>(FileWatcher_OnRenamed), sender, e);
                return;
            }

            var newFilename = e.FullPath;
            var oldFilename = e.OldFullPath;

            var oldExt = Path.GetExtension(oldFilename).ToLower();
            var newExt = Path.GetExtension(newFilename).ToLower();

            // Only watch the supported file types
            if (!Configs.AllFormats.Contains(oldExt) && !Configs.AllFormats.Contains(newExt)) {
                return;
            }

            // Get index of renamed image
            var imgIndex = Local.ImageList.IndexOf(oldFilename);

            // if user changed file extension
            if (oldExt.CompareTo(newExt) != 0) {
                // [old] && [new]: update filename only
                if (Configs.AllFormats.Contains(oldExt) && Configs.AllFormats.Contains(newExt)) {
                    if (imgIndex > -1) {
                        RenameAction();
                    }
                }
                else {
                    // [old] && ![new]: remove from image list
                    if (Configs.AllFormats.Contains(oldExt)) {
                        DoDeleteFiles(oldFilename);
                    }
                    // ![old] && [new]: add to image list
                    else if (Configs.AllFormats.Contains(newExt)) {
                        FileWatcher_AddNewFileAction(newFilename);
                    }
                }
            }
            //if user changed filename only (not extension)
            else {
                if (imgIndex > -1) {
                    RenameAction();
                }
            }

            void RenameAction() {
                //Rename file in image list
                Local.ImageList.SetFileName(imgIndex, newFilename);

                //Update status bar title
                SetStatusBar();

                try {
                    //Rename image in thumbnail bar
                    thumbnailBar.Items[imgIndex].Text = Path.GetFileName(e.FullPath);
                    thumbnailBar.Items[imgIndex].Tag = newFilename;
                }
                catch { }

                // User renamed the initial file - update in case of list reload
                if (oldFilename == Local.InitialInputPath)
                    Local.InitialInputPath = newFilename;
            }
        }

        private void FileWatcher_OnChanged(object sender, FileChangedEvent e) {
            if (Local.IsBusy) {
                return;
            }

            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();
            if (!Configs.AllFormats.Contains(ext)) {
                return;
            }

            // update the viewing image
            var imgIndex = Local.ImageList.IndexOf(e.FullPath);

            // KBR 20180827 When downloading using Chrome, the downloaded file quickly transits
            // from ".tmp" > ".jpg.crdownload" > ".jpg". The last is a "changed" event, and the
            // final ".jpg" cannot exist in the ImageList. Fire this off to the "rename" logic
            // so the new file is correctly added. [Could it be the "created" instead?]
            if (imgIndex == -1) {
                Invoke(new Action<object, FileChangedEvent>(FileWatcher_OnRenamed), sender, e);
                return;
            }

            if (imgIndex == Local.CurrentIndex && string.IsNullOrEmpty(Local.ImageModifiedPath)) {
                _ = NextPicAsync(0, true, true);
            }

            //update thumbnail
            thumbnailBar.Items[imgIndex].Update();
        }

        private void FileWatcher_OnCreated(object sender, FileChangedEvent e) {
            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();

            if (!Configs.AllFormats.Contains(ext)) {
                return;
            }

            if (Local.ImageList.IndexOf(e.FullPath) == -1) {
                FileWatcher_AddNewFileAction(e.FullPath);
            }
        }

        private void FileWatcher_OnDeleted(object sender, FileChangedEvent e) {
            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();
            if (!Configs.AllFormats.Contains(ext)) {
                return;
            }

            // add to queue list for deleting
            _queueListForDeleting.Add(e.FullPath);
        }

        private void FileWatcher_AddNewFileAction(string newFilename) {
            //Add the new image to the list
            Local.ImageList.Add(newFilename);

            //Add the new image to thumbnail bar
            var lvi = new ImageListView.ImageListViewItem(newFilename) {
                Tag = newFilename
            };

            thumbnailBar.Items.Add(lvi);
            thumbnailBar.Refresh();

            SetStatusBar(); // File count has changed - update title bar

            // display the file just added
            if (Configs.AutoDisplayNewImageInFolder) {
                Local.CurrentIndex = Local.ImageList.Length - 1;
                _ = NextPicAsync(0);
            }
        }

        /// <summary>
        /// The queue thread to check the files needed to be deleted.
        /// </summary>
        private void ThreadWatcherDeleteFiles() {
            while (true) {
                if (_queueListForDeleting.Count > 0) {
                    var filename = _queueListForDeleting[0];
                    _queueListForDeleting.RemoveAt(0);

                    DoDeleteFiles(filename);
                }
                else {
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// Proceed deleting file in memory
        /// </summary>
        /// <param name="filename"></param>
        private void DoDeleteFiles(string filename) {
            if (InvokeRequired) {
                Invoke(new Action<string>(DoDeleteFiles), filename);
                return;
            }

            // Get index of deleted image
            var imgIndex = Local.ImageList.IndexOf(filename);

            if (imgIndex > -1) {
                // delete image list
                Local.ImageList.Remove(imgIndex);

                // delete thumbnail list
                thumbnailBar.Items.RemoveAt(imgIndex);

                // change the viewing image to memory data mode
                if (imgIndex == Local.CurrentIndex) {
                    Local.ImageError = new Exception("File not found.");
                    Local.IsTempMemoryData = true;

                    ShowToastMsg(Configs.Language.Items[$"{Name}._ImageNotExist"], 1300);

                    if (_queueListForDeleting.Count == 0) {
                        _ = NextPicAsync(0);
                    }
                }

                // If user deletes the initially loaded image, use the path instead, in case
                // of list re-load.
                if (filename == Local.InitialInputPath)
                    Local.InitialInputPath = Path.GetDirectoryName(filename);
            }
        }

        #endregion

        // Use mouse wheel to navigate, scroll, or zoom images
        private void picMain_MouseWheel(object sender, MouseEventArgs e) {
            var action = Control.ModifierKeys switch {
                Keys.Control => Configs.MouseWheelCtrlAction,
                Keys.Shift => Configs.MouseWheelShiftAction,
                Keys.Alt => Configs.MouseWheelAltAction,
                _ => Configs.MouseWheelAction,
            };

            switch (action) {
                case MouseWheelActions.Zoom:
                    picMain.ZoomWithMouseWheel(e.Delta, e.Location);
                    break;
                case MouseWheelActions.ScrollVertically:
                    picMain.ScrollWithMouseWheel(e.Delta);
                    break;
                case MouseWheelActions.ScrollHorizontally:
                    picMain.ScrollWithMouseWheel(e.Delta, true);
                    break;
                case MouseWheelActions.BrowseImages:
                    if (e.Delta < 0) {
                        // Next pic
                        mnuMainViewNext_Click(null, null);
                    }
                    else {
                        // Previous pic
                        mnuMainViewPrevious_Click(null, null);
                    }
                    break;
                case MouseWheelActions.DoNothing:
                default:
                    break;
            }
        }

        private void picMain_Zoomed(object sender, ImageBoxZoomEventArgs e) {
            _isManuallyZoomed = true;

            // Handle window fit after zoom change
            if (Configs.IsWindowFit) {
                WindowFitMode(false);
            }

            // Set new zoom ratio if Zoom Mode LockZoomRatio is enabled
            if (Configs.ZoomMode == ZoomMode.LockZoomRatio) {
                Configs.ZoomLockValue = e.NewZoom;
            }

            // Zoom optimization
            ZoomOptimization();

            // Update zoom info
            SetStatusBar();
        }

        private void picMain_MouseClick(object sender, MouseEventArgs e) {
            switch (e.Button) {
                case MouseButtons.Middle: //Reset zoom mode
                    ApplyZoomMode(Configs.ZoomMode);
                    break;

                case MouseButtons.XButton1: //Back
                    mnuMainViewPrevious_Click(null, null);
                    break;

                case MouseButtons.XButton2: //Next
                    mnuMainViewNext_Click(null, null);
                    break;

                case MouseButtons.Left:
                    var navRegion = TestCursorHitNavRegions(e.Location);

                    if (navRegion?.Type == NavigationRegionType.Left) {
                        mnuMainViewPrevious_Click(null, null);
                    }
                    else if (navRegion?.Type == NavigationRegionType.Right) {
                        mnuMainViewNext_Click(null, null);
                    }
                    break;

                default:
                    break;
            }
        }

        private void picMain_MouseDoubleClick(object sender, MouseEventArgs e) {
            switch (e.Button) {
                case MouseButtons.XButton1:
                    mnuMainViewPrevious_Click(null, null);
                    break;

                case MouseButtons.XButton2:
                    mnuMainViewNext_Click(null, null);
                    break;

                case MouseButtons.Left:
                    // check double-click in Navigation regions
                    var navRegion = TestCursorHitNavRegions(e.Location);
                    if (navRegion?.Type == NavigationRegionType.Left) {
                        _ = NextPicAsync(-1);
                    }
                    else if (navRegion?.Type == NavigationRegionType.Right) {
                        _ = NextPicAsync(1);
                    }
                    else {
                        if (picMain.Zoom < 100) {
                            mnuMainActualSize_Click(null, null);
                        }
                        else {
                            ApplyZoomMode(Configs.ZoomMode);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void picMain_MouseMove(object sender, MouseEventArgs e) {
            #region Navigation regions
            // get current cursor position on frmMain
            var pos = PointToClient(MousePosition);
            var navRegion = TestCursorHitNavRegions(pos);

            // get the current nav type
            var navType = navRegion?.Type ?? NavigationRegionType.Unknown;

            // only draw if nav type is different
            if (Local.NavRegionType != navType) {
                Local.NavRegionType = navType;

                // draw navigation regions
                picMain.Invalidate();
            }
            #endregion
        }

        private void picMain_MouseLeave(object sender, EventArgs e) {
            if (Local.NavRegionType != NavigationRegionType.Unknown) {
                Local.NavRegionType = NavigationRegionType.Unknown;

                // draw navigation regions
                picMain.Invalidate();
            }
        }

        private void picMain_SizeChanged(object sender, EventArgs e) {
            // update navigation regions list
            if (Configs.IsShowNavigationButtons) {
                Local.NavRegions = GetNavigationRegions();
            }
        }

        private void sp1_SplitterMoved(object sender, SplitterEventArgs e) {
            // User has moved the thumbnail splitter bar. Update image size.
            if (!_isManuallyZoomed) {
                ApplyZoomMode(Configs.ZoomMode);
            }
        }

        private void Tray_MouseDoubleClick(object sender, MouseEventArgs e) {
            // show app
            _ = ToggleAppVisibilityAsync(true);
        }

        #endregion


        #region Toolbar Buttons Events

        private void btnNext_Click(object sender, EventArgs e) {
            mnuMainViewNext_Click(null, e);
        }

        private void btnBack_Click(object sender, EventArgs e) {
            mnuMainViewPrevious_Click(null, e);
        }

        private void btnRefresh_Click(object sender, EventArgs e) {
            mnuMainRefresh_Click(null, null);
        }

        private void btnRotateRight_Click(object sender, EventArgs e) {
            mnuMainRotateClockwise_Click(null, e);
        }

        private void btnRotateLeft_Click(object sender, EventArgs e) {
            mnuMainRotateCounterclockwise_Click(null, e);
        }

        private void btnFlipHorz_Click(object sender, EventArgs e) {
            mnuMainFlipHorz_Click(null, null);
        }

        private void btnFlipVert_Click(object sender, EventArgs e) {
            mnuMainFlipVert_Click(null, null);
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            mnuMainMoveToRecycleBin_Click(null, e);
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            mnuMainEditImage_Click(null, null);
        }

        private void btnCropping_Click(object sender, EventArgs e) {
            mnuMainCrop.PerformClick();
        }

        private void btnColorPicker_Click(object sender, EventArgs e) {
            mnuMainColorPicker.PerformClick();
        }

        private void btnOpen_Click(object sender, EventArgs e) {
            mnuMainOpenFile_Click(null, e);
        }

        private void btnThumb_Click(object sender, EventArgs e) {
            mnuMainThumbnailBar_Click(null, e);
        }

        private void btnActualSize_Click(object sender, EventArgs e) {
            mnuMainActualSize_Click(null, e);
        }

        private void btnAutoZoom_Click(object sender, EventArgs e) {
            mnuMainAutoZoom_Click(null, e);
        }

        private void btnScaletoWidth_Click(object sender, EventArgs e) {
            mnuMainScaleToWidth_Click(null, e);
        }

        private void btnScaletoHeight_Click(object sender, EventArgs e) {
            mnuMainScaleToHeight_Click(null, e);
        }

        private void btnWindowFit_Click(object sender, EventArgs e) {
            mnuWindowFit_Click(null, e);
        }

        private void btnGoto_Click(object sender, EventArgs e) {
            mnuMainGoto_Click(null, e);
        }

        private void btnCheckedBackground_Click(object sender, EventArgs e) {
            mnuMainCheckBackground_Click(null, e);
        }

        private void btnZoomIn_Click(object sender, EventArgs e) {
            mnuMainZoomIn_Click(null, e);
        }

        private void btnZoomOut_Click(object sender, EventArgs e) {
            mnuMainZoomOut_Click(null, e);
        }

        private void btnScaleToFit_Click(object sender, EventArgs e) {
            mnuMainScaleToFit_Click(null, e);
        }

        private void btnScaleToFill_Click(object sender, EventArgs e) {
            mnuMainScaleToFill_Click(null, e);
        }

        private void btnZoomLock_Click(object sender, EventArgs e) {
            mnuMainLockZoomRatio_Click(null, e);
        }

        private void btnSlideShow_Click(object sender, EventArgs e) {
            mnuMainSlideShowStart_Click(null, null);
        }

        private void btnFullScreen_Click(object sender, EventArgs e) {
            mnuMainFullScreen_Click(null, e);
        }

        private void btnPrintImage_Click(object sender, EventArgs e) {
            mnuMainPrint_Click(null, e);
        }

        private void btnConvert_Click(object sender, EventArgs e) {
            mnuMainSaveAs_Click(null, e);
        }

        private void btnMenu_Click(object sender, EventArgs e) {
            mnuMain.Show(toolMain, toolMain.Width - mnuMain.Width, toolMain.Height);
        }
        #endregion

        #region Menu Common
        private void SetShortcutExit() {
            if (Configs.IsContinueRunningBackground) {
                mnuMainExitApplication.ShortcutKeyDisplayString = "Shift+ESC";
            }
            else {
                mnuMainExitApplication.ShortcutKeyDisplayString = Configs.IsPressESCToQuit ? "ESC" : "Alt+F4";
            }
        }
        #endregion

        #region Context Menu
        private void OpenShortcutMenu(ToolStripMenuItem parentMenu) {
            mnuShortcut.Items.Clear();

            foreach (ToolStripItem item in parentMenu.DropDownItems) {
                if (item.GetType() == typeof(ToolStripSeparator)) {
                    mnuShortcut.Items.Add(new ToolStripSeparator());
                }
                else {
                    mnuShortcut.Items.Add(UI.Menu.Clone(item as ToolStripMenuItem));
                }
            }

            mnuShortcut.Show(Cursor.Position);
        }


        private void mnuContext_Opening(object sender, CancelEventArgs e) {
            var imageNotFound = !File.Exists(Local.ImageList.GetFileName(Local.CurrentIndex));
            var imageError = Local.ImageError != null;

            // clear current items
            mnuContext.Items.Clear();

            if (Configs.IsSlideshow && !imageNotFound) {
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainSlideShowPause));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainSlideShowExit));
                mnuContext.Items.Add(new ToolStripSeparator());
            }

            // toolbar menu
            mnuContext.Items.Add(UI.Menu.Clone(mnuMainToolbar));
            mnuContext.Items.Add(UI.Menu.Clone(mnuMainAlwaysOnTop));

            mnuContext.Items.Add(new ToolStripSeparator());
            mnuContext.Items.Add(UI.Menu.Clone(mnuLoadingOrder));

            // Get Edit App info
            if (!imageNotFound) {
                if (!imageError && !Local.IsTempMemoryData && Local.CurrentPageCount <= 1) {
                    mnuContext.Items.Add(UI.Menu.Clone(mnuMainChannels));
                }

                mnuContext.Items.Add(new ToolStripSeparator());
                if (!Helpers.IsOS(WindowsOS.Win7)) {
                    mnuContext.Items.Add(UI.Menu.Clone(mnuOpenWith));
                }

                UpdateEditAppInfoForMenu();
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainEditImage));

                #region Check if image can animate (GIF)
                try {
                    if (!imageError && Local.CurrentPageCount > 1) {
                        var mnu1 = UI.Menu.Clone(mnuMainExtractPages);
                        mnu1.Text = string.Format(Configs.Language.Items[$"{Name}.mnuMainExtractPages"], Local.CurrentPageCount);
                        mnu1.Enabled = true;

                        var mnu2 = UI.Menu.Clone(mnuMainStartStopAnimating);
                        mnu2.Enabled = true;

                        mnuContext.Items.Add(mnu1);
                        mnuContext.Items.Add(mnu2);
                    }
                }
                catch { }
                #endregion
            }

            if (!imageNotFound && !imageError || Local.IsTempMemoryData) {
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainSetAsDesktop));

                // check if igcmdWin10.exe exists!
                if (Helpers.IsOS(WindowsOS.Win10OrLater) && File.Exists(App.StartUpDir("igcmdWin10.exe"))) {
                    mnuContext.Items.Add(UI.Menu.Clone(mnuMainSetAsLockImage));
                }
            }

            #region Menu group: CLIPBOARD
            mnuContext.Items.Add(new ToolStripSeparator());//------------

            if (picMain.Image != null) {
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainCopyImageData));
            }

            if (!imageNotFound && !Local.IsTempMemoryData) {
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainCopy));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainCut));
            }

            mnuContext.Items.Add(UI.Menu.Clone(mnuMainOpenImageData));
            if (!imageNotFound && !Local.IsTempMemoryData) {
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainClearClipboard));
            }
            #endregion

            if (!imageNotFound && !Local.IsTempMemoryData) {
                mnuContext.Items.Add(new ToolStripSeparator());//------------
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainRename));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainComment));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainMoveToRecycleBin));

                mnuContext.Items.Add(new ToolStripSeparator());//------------
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainCopyImagePath));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainImageLocation));
                mnuContext.Items.Add(UI.Menu.Clone(mnuMainImageProperties));
            }

            SetShortcutExit();
            mnuContext.Items.Add(new ToolStripSeparator());
            mnuContext.Items.Add(UI.Menu.Clone(mnuMainExitApplication));
        }

        private void MnuTray_Opening(object sender, CancelEventArgs e) {
            // menu item size
            var iconH = DPIScaling.Transform(Constants.MENU_ICON_HEIGHT);
            mnuTrayExit.Image = new Bitmap(iconH, iconH);

            // language
            mnuTrayShowWindow.Text = Configs.Language.Items[$"{Name}.{nameof(mnuTrayShowWindow)}"];
            mnuTrayExit.Text = Configs.Language.Items[$"{Name}.{nameof(mnuMainExitApplication)}"];

            // info
            mnuTrayInfo.Enabled = false;
            mnuTrayInfo.Text = $"{Application.ProductName}" +
                $" v{Application.ProductVersion}" +
                $" {(Environment.Is64BitProcess ? "x64" : "x86")}";
        }

        private void mnuTrayShowWindow_Click(object sender, EventArgs e) {
            _ = ToggleAppVisibilityAsync(true);
        }

        private void MnuTrayExit_Click(object sender, EventArgs e) {
            Exit(true);
        }

        #endregion


        #region Main Menu (Main functions)

        private void mnuMainOpenFile_Click(object sender, EventArgs e) {
            OpenFile();
        }

        private void MnuMainNewWindow_Click(object sender, EventArgs e) {
            if (!Configs.IsAllowMultiInstances) {
                ShowToastMsg(Configs.Language.Items[$"{Name}.mnuMainNewWindow._Error"], 2000);

                return;
            }

            try {
                var filename = Local.ImageList.GetFileName(Local.CurrentIndex);

                Process.Start(new ProcessStartInfo() {
                    FileName = Application.ExecutablePath,
                    Arguments = $"\"{filename}\"",
                });
            }
            catch { }
        }

        private void mnuMainOpenImageData_Click(object sender, EventArgs e) {
            picMain.Text = string.Empty;

            // Is there a file in clipboard ?
            if (Clipboard.ContainsFileDropList()) {
                var sFile = (string[])Clipboard.GetData(DataFormats.FileDrop);

                // load file
                PrepareLoading(sFile[0]);
            }

            // Is there a image in clipboard ?
            // CheckImageInClipboard: ;
            else if (Clipboard.ContainsImage()) {
                var bmp = ClipboardEx.GetClipboardImage((DataObject)Clipboard.GetDataObject());

                LoadImageData(bmp);
            }

            // Is there a filename in clipboard?
            // CheckPathInClipboard: ;
            else if (Clipboard.ContainsText()) {
                // try to get absolute path
                var text = App.ToAbsolutePath(Clipboard.GetText());

                if (File.Exists(text) || Directory.Exists(text)) {
                    PrepareLoading(text);
                }
                // get image from Base64string 
                else {
                    try {
                        var img = Heart.Photo.ConvertBase64ToBitmap(text);
                        LoadImageData(img);
                    }
                    catch (Exception ex) {
                        var msg = Configs.Language.Items[$"{Name}._InvalidImageClipboardData"];
                        ShowToastMsg($"{msg}\r\n{ex.Source}: {ex.Message}", 3000);
                    }
                }
            }
        }

        /// <summary>
        /// Sets app's busy state. UI interaction is blocked while the app is busy
        /// </summary>
        /// <param name="isBusy"></param>
        /// <param name="msg"></param>
        private async Task SetAppBusyAsync(bool isBusy, string msg = "", int disableDelay = 0, int msgDelay = 0, int msgDuration = 30_000) {
            _busyCancelToken.Cancel();
            _busyCancelToken = new();

            Local.IsBusy = isBusy;

            if (isBusy) {
                sp0.Cursor = Cursors.WaitCursor;
                KeyPreview = false;

                if (!string.IsNullOrEmpty(msg)) {
                    ShowToastMsg(msg, msgDuration, msgDelay);
                }
            }
            else {
                sp0.Cursor = Cursors.Default;
                ShowToastMsg(msg, 0);
            }

            if (disableDelay > 0) {
                try {
                    await Task.Delay(disableDelay, _busyCancelToken.Token);
                }
                catch { }
            }

            sp0.Enabled = !Local.IsBusy;
            KeyPreview = true;
            picMain.Focus();
        }


        /// <summary>
        /// Save image to file
        /// </summary>
        /// <param name="destFilename">Destination file</param>
        /// <param name="destExt">Destination file extension. E.g. "png"</param>
        /// <returns></returns>
        private async Task SaveImageAsAsync(string destFilename, string destExt) {
            if (picMain.Image == null) {
                return;
            }

            var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);
            if (string.IsNullOrEmpty(currentFile)) currentFile = "untitled.png";

            // set app busy
            _ = SetAppBusyAsync(true, string.Format(Configs.Language.Items[$"{Name}._SavingImage"], destFilename), 1000);

            Bitmap clonedPic;

            if (!picMain.SelectionRegion.IsEmpty) {
                clonedPic = (Bitmap)picMain.GetSelectedImage();
            }
            else {
                clonedPic = (Bitmap)picMain.Image;
            }

            switch (destExt) {
                case "bmp":
                case "gif":
                case "png":
                case "jpg" or "jpeg" or "jpe":
                    Heart.Photo.Save(clonedPic, destFilename, quality: Configs.ImageEditQuality);
                    break;
                case "emf":
                    clonedPic.Save(destFilename, ImageFormat.Emf);
                    break;
                case "exif":
                    clonedPic.Save(destFilename, ImageFormat.Exif);
                    break;
                case "ico":
                    clonedPic.Save(destFilename, ImageFormat.Icon);
                    break;
                case "jxl":
                    Heart.Photo.Save(clonedPic, destFilename, (int)MagickFormat.Jxl, quality: Configs.ImageEditQuality);
                    break;
                case "tiff":
                    clonedPic.Save(destFilename, ImageFormat.Tiff);
                    break;
                case "wmf":
                    clonedPic.Save(destFilename, ImageFormat.Wmf);
                    break;
                default:
                    using (var ms = new MemoryStream()) {
                        try {
                            // temporary data or selected region
                            if (Local.IsTempMemoryData || !picMain.SelectionRegion.IsEmpty) {
                                await Heart.Photo.SaveAsBase64Async(clonedPic, destFilename, ImageFormat.Png).ConfigureAwait(true);
                            }
                            else {
                                await Heart.Photo.SaveAsBase64Async(
                                    currentFile,
                                    destFilename,
                                    clonedPic.RawFormat).ConfigureAwait(true);
                            }
                        }
                        catch (Exception ex) {
                            MessageBox.Show(Configs.Language.Items[$"{Name}._SaveImageError"] + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    break;
            }

            // display successful msg
            if (File.Exists(destFilename)) {
                ShowToastMsg(string.Format(Configs.Language.Items[$"{Name}._SaveImage"], destFilename), 2000);
            }

            // release busy state
            _ = SetAppBusyAsync(false);
        }


        private void mnuSaveImage_Click(object sender, EventArgs e) {
            var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);

            // trigger "Save image as"
            if (Local.IsTempMemoryData || !picMain.SelectionRegion.IsEmpty || string.IsNullOrEmpty(currentFile)) {
                mnuMainSaveAs_Click(null, null);
                return;
            }

            Local.ImageModifiedPath = currentFile;
            _ = SaveImageChangeAsync(true);
        }

        private void mnuMainSaveAs_Click(object sender, EventArgs e) {
            var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);
            var ext = Path.GetExtension(currentFile);

            // Report at Google groups - no extension or problem getting extension, take reasonable default
            // Otherwise, crashes when ext is null or empty.
            if (ext == null || ext.Length < 2)
                ext = "jpg";
            else
                ext = ext.Substring(1);

            var saveDialog = new SaveFileDialog {
                Filter = "BMP|*.bmp|EMF|*.emf|EXIF|*.exif|GIF|*.gif|ICO|*.ico|JPG|*.jpg|PNG|*.png|JPEG-XL|*.jxl|TIFF|*.tiff|WMF|*.wmf|Base64String (*.b64)|*.b64|Base64String (*.txt)|*.txt",
                FileName = Path.GetFileNameWithoutExtension(currentFile),
                RestoreDirectory = true,
            };

            // When saving image from clipboard, there is no path (issue #1075)
            // In the window of time while IG is populating the image list, there is no path (issue #1055)
            var path2 = string.IsNullOrEmpty(currentFile) ? currentFile : Path.GetDirectoryName(currentFile);
            saveDialog.CustomPlaces.Add(path2);


            // Use the last-selected file extension, if available.
            if (Local.SaveAsFilterIndex != 0) {
                saveDialog.FilterIndex = Local.SaveAsFilterIndex;
            }
            else {
                saveDialog.FilterIndex = ext.ToLower() switch {
                    "bmp" => 1,
                    "emf" => 2,
                    "exif" => 3,
                    "gif" => 4,
                    "ico" => 5,
                    "jpg" or "jpeg" or "jpe" => 6,
                    "jxl" => 8,
                    "tiff" => 9,
                    "wmf" => 10,
                    _ => 7, // png
                };
            }

            if (saveDialog.ShowDialog() == DialogResult.OK) {
                Local.SaveAsFilterIndex = saveDialog.FilterIndex;

                var destExt = Path.GetExtension(saveDialog.FileName).Substring(1);

                _ = SaveImageAsAsync(saveDialog.FileName, destExt);
            }
        }

        private void mnuMainRefresh_Click(object sender, EventArgs e) {
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainReloadImage_Click(object sender, EventArgs e) {
            //Reload the viewing image
            _ = NextPicAsync(step: 0, isKeepZoomRatio: false, isSkipCache: true);
        }

        private void MnuMainReloadImageList_Click(object sender, EventArgs e) {
            // update image list
            _ = PrepareLoadingAsync(Local.ImageList.FileNames, Local.ImageList.GetFileName(Local.CurrentIndex));
        }

        private void mnuOpenWith_Click(object sender, EventArgs e) {
            using var p = new Process();
            p.StartInfo.FileName = "openwith";

            // Build the arguments
            var filename = Local.ImageList.GetFileName(Local.CurrentIndex);
            p.StartInfo.Arguments = $"\"{filename}\"";

            // show error dialog
            p.StartInfo.ErrorDialog = true;

            try {
                p.Start();
            }
            catch { }
        }

        private void mnuMainEditImage_Click(object sender, EventArgs e) {
            string filename;

            // If viewing image is temporary memory data
            if (Local.IsTempMemoryData) {
                // Save to temp file
                filename = SaveTemporaryMemoryData();
            }
            else {
                // Viewing image filename
                filename = Local.ImageList.GetFileName(Local.CurrentIndex);
            }

            // Get extension
            var ext = Path.GetExtension(filename).ToLower();

            // Get EditApp for editing
            var app = Configs.GetEditApp(ext);

            if (app != null) {
                // Open configured app for editing
                using var p = new Process();
                p.StartInfo.FileName = Environment.ExpandEnvironmentVariables(app.AppPath);

                // Build the arguments
                var args = app.AppArguments.Replace(EditApp.FileMacro, filename);
                p.StartInfo.Arguments = $"{args}";

                // show error dialog
                p.StartInfo.ErrorDialog = true;

                try {
                    p.Start();

                    RunActionAfterEditing();
                }
                catch { }
            }
            else // Edit by default associated app
            {
                EditByDefaultApp(filename);
            }
        }

        private void EditByDefaultApp(string filename) {
            // windows 11 sucks the verb 'edit'
            if (Helpers.IsOS(WindowsOS.Win11)) {
                var mspaint11 = @"%LocalAppData%\Microsoft\WindowsApps\mspaint.exe";
                var fullPath = Environment.ExpandEnvironmentVariables(mspaint11);

                if (!File.Exists(fullPath)) {
                    MessageBox.Show("Could not find the default app for editing. Please associate your app in ImageGlass Settings > Edit.", filename, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                using var p11 = new Process();
                p11.StartInfo.FileName = fullPath;
                p11.StartInfo.Arguments = $"\"{filename}\"";
                p11.StartInfo.UseShellExecute = true;

                try {
                    p11.Start();

                    RunActionAfterEditing();
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, filename, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return;
            }


            // windows 10 or earlier ------------------------------
            var win32ErrorMsg = string.Empty;

            using var p10 = new Process();
            p10.StartInfo.FileName = $"\"{filename}\"";
            p10.StartInfo.Verb = "edit";

            // first try: launch the associated app for editing
            try {
                p10.Start();

                RunActionAfterEditing();
            }
            catch (Win32Exception ex) {
                // file does not have associated app
                win32ErrorMsg = ex.Message;
            }
            catch { }

            if (string.IsNullOrEmpty(win32ErrorMsg)) return;


            // second try: use MS Paint to edit the file
            using var p = new Process();
            p.StartInfo.FileName = Environment.ExpandEnvironmentVariables("mspaint.exe");
            p.StartInfo.Arguments = $"\"{filename}\"";
            p.StartInfo.UseShellExecute = true;


            try {
                p.Start();

                RunActionAfterEditing();
            }
            catch (Win32Exception) {
                // show error: file does not have associated app
                MessageBox.Show(win32ErrorMsg, filename, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }

        }


        private void mnuMainShare_Click(object sender, EventArgs e) {
            var filename = "";

            if (Local.IsTempMemoryData) {
                filename = SaveTemporaryMemoryData();
            }
            else {
                filename = Local.ImageList.GetFileName(Local.CurrentIndex);

                if (!File.Exists(filename)) return;
            }

            try {
                using var p = new Process();
                var args = string.Format("share \"{0}\"", filename);

                p.StartInfo.FileName = App.StartUpDir("igcmdWin10.exe");
                p.StartInfo.Arguments = args;
                p.EnableRaisingEvents = true;
                p.Start();

                p.WaitForExit();
            }
            catch { }
        }


        private void mnuMainViewNext_Click(object sender, EventArgs e) {
            _ = NextPicAsync(1);
        }

        private void mnuMainViewPrevious_Click(object sender, EventArgs e) {
            _ = NextPicAsync(-1);
        }

        private void mnuMainGoto_Click(object sender, EventArgs e) {
            var newIndex = Local.CurrentIndex;
            var value = (newIndex + 1).ToString();

            if (InputBox.ShowDialog(
                theme: Configs.Theme,
                message: Configs.Language.Items[$"{Name}._GotoDialogText"],
                defaultValue: value,
                isNumberOnly: true,
                topMost: TopMost,
                filterOnKeyPressed: true) == DialogResult.OK) {
                value = InputBox.Message;
            }

            if (int.TryParse(value, out newIndex)) {
                newIndex--;
                // KBR 20190302 have out-of-range values go to beginning/end as appropriate
                if (newIndex < 1)
                    newIndex = 0;
                else if (newIndex >= Local.ImageList.Length)
                    newIndex = Local.ImageList.Length - 1;

                Local.CurrentIndex = newIndex;
                _ = NextPicAsync(0);
            }
        }

        private void mnuMainGotoFirst_Click(object sender, EventArgs e) {
            Local.CurrentIndex = 0;
            _ = NextPicAsync(0);
        }

        private void mnuMainGotoLast_Click(object sender, EventArgs e) {
            Local.CurrentIndex = Local.ImageList.Length - 1;
            _ = NextPicAsync(0);
        }

        private void mnuMainPrevPage_Click(object sender, EventArgs e) {
            Local.CurrentPageIndex--;
            _ = NextPicAsync(0, pageIndex: Local.CurrentPageIndex);
        }

        private void mnuMainNextPage_Click(object sender, EventArgs e) {
            Local.CurrentPageIndex++;
            _ = NextPicAsync(0, pageIndex: Local.CurrentPageIndex);
        }

        private void mnuMainFirstPage_Click(object sender, EventArgs e) {
            Local.CurrentPageIndex = 0;
            _ = NextPicAsync(0, pageIndex: Local.CurrentPageIndex);
        }

        private void mnuMainLastPage_Click(object sender, EventArgs e) {
            Local.CurrentPageIndex = Local.CurrentPageCount - 1;
            _ = NextPicAsync(0, pageIndex: Local.CurrentPageIndex);
        }

        private void mnuMainFullScreen_Click(object sender, EventArgs e) {
            // enter full screen
            if (!Configs.IsFullScreen) {
                mnuMainFullScreen.Checked =
                    btnFullScreen.Checked =
                    Configs.IsFullScreen = true;

                SetFullScreenMode(
                    enabled: true,
                    hideToolbar: Configs.IsHideToolbarInFullscreen,
                    hideThumbnailBar: Configs.IsHideThumbnailBarInFullscreen);

                ShowToastMsg(
                    string.Format(
                        Configs.Language.Items[$"{Name}._FullScreenMessage"],
                        mnuMainFullScreen.ShortcutKeyDisplayString),
                    2000);
            }
            // exit full screen
            else {
                mnuMainFullScreen.Checked =
                    btnFullScreen.Checked =
                    Configs.IsFullScreen = false;

                SetFullScreenMode(
                    enabled: false,
                    hideToolbar: Configs.IsHideToolbarInFullscreen,
                    hideThumbnailBar: Configs.IsHideThumbnailBarInFullscreen);
            }
        }

        private void mnuMainSlideShowStart_Click(object sender, EventArgs e) {
            if (Local.ImageList.Length < 1) {
                return;
            }

            // not performing
            if (!Configs.IsSlideshow) {
                picMain.BackColor = Color.Black;

                // enter full screen
                SetFullScreenMode(
                    enabled: true,
                    changeWindowState: !Configs.IsFullScreen,
                    hideToolbar: true,
                    hideThumbnailBar: true);

                //perform slideshow
                timSlideShow.Enabled = true;
                _slideshowStopwatch.Reset();

                Configs.IsSlideshow = true;
                SysExecutionState.PreventSleep();

                ShowToastMsg(Configs.Language.Items[$"{Name}._SlideshowMessage"], 2000);
            }
            // performing
            else {
                mnuMainSlideShowExit_Click(null, null);
            }
        }

        private void mnuMainSlideShowPause_Click(object sender, EventArgs e) {
            // performing
            if (timSlideShow.Enabled) {
                timSlideShow.Enabled = false;
                _slideshowStopwatch.Stop();

                ShowToastMsg(Configs.Language.Items[$"{Name}._SlideshowMessagePause"], 2000);
            }
            else {
                timSlideShow.Enabled = true;
                _slideshowStopwatch.Start();

                ShowToastMsg(Configs.Language.Items[$"{Name}._SlideshowMessageResume"], 2000);
            }
        }

        private void mnuMainSlideShowExit_Click(object sender, EventArgs e) {
            timSlideShow.Enabled = false;
            Configs.IsSlideshow = false;
            SysExecutionState.AllowSleep();

            picMain.BackColor = Configs.BackgroundColor;

            // exit full screen
            SetFullScreenMode(
                enabled: false,
                changeWindowState: !Configs.IsFullScreen,
                hideToolbar: true,
                hideThumbnailBar: true);
        }

        private void mnuMainPrint_Click(object sender, EventArgs e) {
            // image error
            if (picMain.Image == null) {
                return;
            }

            var currentFile = Local.ImageList.GetFileName(Local.CurrentIndex);
            var fileToPrint = currentFile;

            if (Local.IsTempMemoryData || Local.CurrentPageCount == 1) {
                // save image to temp file
                fileToPrint = SaveTemporaryMemoryData();
            }
            // rename ext FAX -> TIFF to multipage printing
            else if (Path.GetExtension(currentFile).ToUpperInvariant() == ".FAX") {
                fileToPrint = App.ConfigDir(PathType.File, Dir.Temporary, Path.GetFileNameWithoutExtension(currentFile) + ".tiff");
                File.Copy(currentFile, fileToPrint, true);
            }


            try {
                PrintService.OpenPrintPictures(fileToPrint);
            }
            catch {
                fileToPrint = SaveTemporaryMemoryData();
                PrintService.OpenPrintPictures(fileToPrint);
            }
        }

        private async void mnuMainRotateCounterclockwise_Click(object sender, EventArgs e) {
            if (picMain.Image == null) return;

            if (picMain.CanAnimate) {
                ShowToastMsg(Configs.Language.Items[$"{Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }

            _ = SetAppBusyAsync(true, "", 1000);
            picMain.Image = await Heart.Photo.RotateImageAsync(new Bitmap(picMain.Image), 270).ConfigureAwait(true);

            if (!Local.IsTempMemoryData) {
                // Save the image path for saving
                Local.ImageModifiedPath = Local.ImageList.GetFileName(Local.CurrentIndex);
            }

            ApplyZoomMode(Configs.ZoomMode);
            await SetAppBusyAsync(false);
        }

        private async void mnuMainRotateClockwise_Click(object sender, EventArgs e) {
            if (picMain.Image == null) return;

            if (picMain.CanAnimate) {
                ShowToastMsg(Configs.Language.Items[$"{Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }

            _ = SetAppBusyAsync(true, "", 1000);
            picMain.Image = await Heart.Photo.RotateImageAsync(new Bitmap(picMain.Image), 90).ConfigureAwait(true);

            if (!Local.IsTempMemoryData) {
                // Save the image path for saving
                Local.ImageModifiedPath = Local.ImageList.GetFileName(Local.CurrentIndex);
            }

            ApplyZoomMode(Configs.ZoomMode);
            await SetAppBusyAsync(false);
        }

        private async void mnuMainFlipHorz_Click(object sender, EventArgs e) {
            if (picMain.Image == null) return;

            if (picMain.CanAnimate) {
                ShowToastMsg(Configs.Language.Items[$"{Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }

            _ = SetAppBusyAsync(true, "", 1000);
            picMain.Image = await Heart.Photo.FlipAsync(new Bitmap(picMain.Image), isHorzontal: true).ConfigureAwait(true);

            if (!Local.IsTempMemoryData) {
                // Save the image path for saving
                Local.ImageModifiedPath = Local.ImageList.GetFileName(Local.CurrentIndex);
            }
            await SetAppBusyAsync(false);
        }

        private async void mnuMainFlipVert_Click(object sender, EventArgs e) {
            if (picMain.Image == null) return;

            if (picMain.CanAnimate) {
                ShowToastMsg(Configs.Language.Items[$"{Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }

            _ = SetAppBusyAsync(true, "", 1000);
            picMain.Image = await Heart.Photo.FlipAsync(new Bitmap(picMain.Image), isHorzontal: false).ConfigureAwait(true);

            if (!Local.IsTempMemoryData) {
                // Save the image path for saving
                Local.ImageModifiedPath = Local.ImageList.GetFileName(Local.CurrentIndex);
            }
            await SetAppBusyAsync(false);
        }

        private void mnuMainZoomIn_Click(object sender, EventArgs e) {
            if (picMain.Image == null) {
                return;
            }

            picMain.ZoomIn();
        }

        private void mnuMainZoomOut_Click(object sender, EventArgs e) {
            if (picMain.Image == null) {
                return;
            }

            picMain.ZoomOut();
        }

        private void mnuCustomZoom_Click(object sender, EventArgs e) {
            if (picMain.Image == null) {
                return;
            }

            if (InputBox.ShowDialog(
                theme: Configs.Theme,
                title: Configs.Language.Items[$"{Name}.{nameof(mnuCustomZoom)}"],
                message: Configs.Language.Items[$"{Name}.{nameof(mnuCustomZoom)}._Text"],
                defaultValue: picMain.Zoom.ToString(),
                isNumberOnly: true,
                topMost: TopMost,
                filterOnKeyPressed: true) == DialogResult.OK) {
                picMain.Zoom = Convert.ToSingle(InputBox.Message);
                picMain.CenterToImage();
            }
        }

        private void mnuMainActualSize_Click(object sender, EventArgs e) {
            if (picMain.Image == null) {
                return;
            }

            picMain.ActualSize();

            if (Configs.IsCenterImage) {
                picMain.CenterToImage();
            }
            else {
                picMain.ScrollTo(0, 0, 0, 0);
            }
        }

        private void mnuWindowFit_Click(object sender, EventArgs e) {
            Configs.IsWindowFit = !Configs.IsWindowFit;
            mnuWindowFit.Checked =
                btnWindowFit.Checked = Configs.IsWindowFit;

            if (picMain.Image == null) {
                return;
            }

            if (Configs.IsWindowFit) {
                WindowFitMode(false);
            }
        }

        private void mnuMainAutoZoom_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.AutoZoom;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainScaleToWidth_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.ScaleToWidth;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainScaleToHeight_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.ScaleToHeight;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainScaleToFit_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.ScaleToFit;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainScaleToFill_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.ScaleToFill;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainLockZoomRatio_Click(object sender, EventArgs e) {
            Configs.ZoomMode = ZoomMode.LockZoomRatio;

            SelectUIZoomMode();
            ApplyZoomMode(Configs.ZoomMode);
        }

        private void mnuMainRename_Click(object sender, EventArgs e) {
            RenameImage();
        }
        private void mnuMainComment_Click(object sender, EventArgs e) {
            EditComment();
        }
        private void mnuMainMoveToRecycleBin_Click(object sender, EventArgs e) {
            try {
                if (!File.Exists(Local.ImageList.GetFileName(Local.CurrentIndex))) {
                    return;
                }
            }
            catch { return; }

            var msg = DialogResult.Yes;

            if (Configs.IsConfirmationDelete) {
                msg = MessageBox.Show(string.Format(Configs.Language.Items[$"{Name}._DeleteDialogText"], Local.ImageList.GetFileName(Local.CurrentIndex)), Configs.Language.Items[$"{Name}._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (msg == DialogResult.Yes) {
                var filename = Local.ImageList.GetFileName(Local.CurrentIndex);
                try {
                    ImageInfo.DeleteFile(filename, isMoveToRecycleBin: true);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainDeleteFromHardDisk_Click(object sender, EventArgs e) {
            try {
                if (!File.Exists(Local.ImageList.GetFileName(Local.CurrentIndex))) {
                    return;
                }
            }
            catch { return; }

            var msg = DialogResult.Yes;

            if (Configs.IsConfirmationDelete) {
                msg = MessageBox.Show(string.Format(Configs.Language.Items[$"{Name}._DeleteDialogText"], Local.ImageList.GetFileName(Local.CurrentIndex)), Configs.Language.Items[$"{Name}._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (msg == DialogResult.Yes) {
                var filename = Local.ImageList.GetFileName(Local.CurrentIndex);
                try {
                    ImageInfo.DeleteFile(filename, isMoveToRecycleBin: false);
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void mnuMainExtractPages_Click(object sender, EventArgs e) {
            // Shortcut keys still work even when menu is disabled!
            if (!(sender as ToolStripMenuItem).Enabled || Local.ImageError != null)
                return;

            using var fb = new FolderBrowserDialog() {
                Description = Configs.Language.Items[$"{Name}._ExtractPageText"],
                ShowNewFolderButton = true,
            };
            var result = fb.ShowDialog();

            if (result == DialogResult.OK && Directory.Exists(fb.SelectedPath)) {
                // set app busy
                var img = await Local.ImageList.GetImgAsync(Local.CurrentIndex);

                picMain.StopAnimating();
                _ = SetAppBusyAsync(true, Configs.Language.Items[$"{Name}._PageExtracting"]);

                await img.SaveImagePagesAsync(fb.SelectedPath);
                picMain.StartAnimating();

                // release app busy
                await SetAppBusyAsync(false);
                ShowToastMsg(Configs.Language.Items[$"{Name}._PageExtractComplete"], 2000);
            }
        }


        private void mnuMainSetAsDesktop_Click(object sender, EventArgs e) {
            _ = Task.Run(() => {
                var isError = false;

                try {
                    // save the current image data to temp file
                    var imgFile = SaveTemporaryMemoryData();

                    using var p = new Process();
                    var args = string.Format("setwallpaper \"{0}\" {1}", imgFile, (int)DesktopWallapaper.Style.Current);

                    // Issue #326: first attempt to set wallpaper w/o privs. 
                    p.StartInfo.FileName = App.StartUpDir("igcmd.exe");
                    p.StartInfo.Arguments = args;
                    p.Start();

                    p.WaitForExit();

                    // If that fails due to privs error, re-attempt with admin privs.
                    if (p.ExitCode == (int)DesktopWallapaper.Result.PrivsFail) {
                        p.StartInfo.FileName = App.StartUpDir("igtasks.exe");
                        p.StartInfo.Arguments = args;
                        p.Start();

                        p.WaitForExit();

                        // success or error
                        isError = p.ExitCode != 0;
                    }
                    else {
                        // success or error
                        isError = p.ExitCode != 0;
                    }
                }
                catch { isError = true; }

                // show result message
                if (isError) {
                    var msg = Configs.Language.Items[$"{Name}._SetBackground_Error"];
                    MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else {
                    var msg = Configs.Language.Items[$"{Name}._SetBackground_Success"];
                    ShowToastMsg(msg, 2000);
                }
            });
        }

        private async void mnuMainSetAsLockImage_Click(object sender, EventArgs e) {
            if (!Helpers.IsOS(WindowsOS.Win10OrLater))
                return; // Do nothing - running Windows 8 or earlier

            var isError = false;

            try {
                await Task.Run(() => {
                    // save the current image data to temp file
                    var imgFile = SaveTemporaryMemoryData();

                    using var p = new Process();
                    var args = string.Format("setlockimage \"{0}\"", imgFile);

                    p.StartInfo.FileName = App.StartUpDir("igcmdWin10.exe");
                    p.StartInfo.Arguments = args;
                    p.EnableRaisingEvents = true;
                    p.Start();

                    p.WaitForExit();

                    // success or error
                    isError = p.ExitCode != 0;
                }).ConfigureAwait(true);
            }
            catch { isError = true; }

            // show result message
            if (isError) {
                var msg = Configs.Language.Items[$"{Name}._SetLockImage_Error"];
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                var msg = Configs.Language.Items[$"{Name}._SetLockImage_Success"];
                ShowToastMsg(msg, 2000);
            }
        }

        private void mnuMainImageLocation_Click(object sender, EventArgs e) {
            if (Local.ImageList.Length > 0) {
                try {
                    Explorer.OpenFolderAndSelectItem(Local.ImageList.GetFileName(Local.CurrentIndex));
                }
                catch {
                    Process.Start("explorer.exe", "/select,\"" +
                        Local.ImageList.GetFileName(Local.CurrentIndex) + "\"");
                }
            }
        }

        private void mnuMainImageProperties_Click(object sender, EventArgs e) {
            if (Local.ImageList.Length > 0) {
                ImageInfo.DisplayFileProperties(Local.ImageList.GetFileName(Local.CurrentIndex), Handle);
            }
        }

        private void mnuMainCopy_Click(object sender, EventArgs e) {
            CopyMultiFiles();
        }

        private void mnuMainCut_Click(object sender, EventArgs e) {
            _ = CutMultiFilesAsync();
        }

        private void mnuMainCopyImageData_Click(object sender, EventArgs e) {
            Image img;
            if (!picMain.SelectionRegion.IsEmpty) {
                img = picMain.GetSelectedImage();
            }
            else {
                img = picMain.Image;
            }

            if (img != null) {
                ClipboardEx.SetClipboardImage(new Bitmap(img), null, null);

                ShowToastMsg(Configs.Language.Items[$"{Name}._CopyImageData"], 1000);
            }
        }

        private void mnuMainCopyImagePath_Click(object sender, EventArgs e) {
            try {
                Clipboard.SetText(Local.ImageList.GetFileName(Local.CurrentIndex));
            }
            catch { }
        }

        private void mnuMainClearClipboard_Click(object sender, EventArgs e) {
            // clear copied files in clipboard
            if (Local.StringClipboard.Count > 0) {
                Local.StringClipboard = new List<string>();
                Clipboard.Clear();
                ShowToastMsg(Configs.Language.Items[$"{Name}._ClearClipboard"], 1000);
            }
        }

        private void mnuMainToolbar_Click(object sender, EventArgs e) {
            Configs.IsShowToolBar = !Configs.IsShowToolBar;

            toolMain.Visible = Configs.IsShowToolBar;
            mnuMainToolbar.Checked = Configs.IsShowToolBar;

            // Issue #554 
            if (!_isManuallyZoomed) {
                // Resize image to adapt when toolbar turned off
                ApplyZoomMode(Configs.ZoomMode);
            }
        }

        private void mnuMainThumbnailBar_Click(object sender, EventArgs e) {
            Configs.IsShowThumbnail = !Configs.IsShowThumbnail;

            sp1.Panel2Collapsed = !Configs.IsShowThumbnail;
            btnThumb.Checked = Configs.IsShowThumbnail;

            if (Configs.IsShowThumbnail) {
                var scaleFactor = ((float)DPIScaling.CurrentDPI) / DPIScaling.DPI_DEFAULT;
                var hScrollHeight = (7 * scaleFactor) - 1;

                if (Configs.IsShowThumbnailScrollbar) {
                    hScrollHeight = SystemInformation.HorizontalScrollBarHeight;
                }

                var gap = (uint)((hScrollHeight * scaleFactor) + (25 / scaleFactor * 1.05));
                var thumbItem = new ThumbnailItemInfo(Configs.ThumbnailDimension, Configs.IsThumbnailHorizontal);
                var minSize = thumbItem.GetTotalDimension() + gap;

                if (Configs.IsThumbnailHorizontal) {
                    // BOTTOM
                    sp1.SplitterWidth = 1;
                    sp1.Orientation = Orientation.Horizontal;
                    sp1.SplitterDistance = Math.Abs(sp1.Height - (int)minSize);
                    thumbnailBar.View = ImageListView.View.Gallery;
                }
                else {
                    // RIGHT
                    sp1.IsSplitterFixed = false; // Allow user to resize
                    sp1.SplitterWidth = (int)Math.Ceiling(3 * scaleFactor);
                    sp1.Orientation = Orientation.Vertical;

                    // KBR 20190302 Issue #483: reset splitter width if it gets out of whack somehow
                    if ((sp1.Width - Configs.ThumbnailBarWidth) < 1) {
                        minSize = (uint)Math.Min(128, sp1.Width);
                        Configs.ThumbnailBarWidth = minSize;
                    }

                    // KBR 20200110 Issue #678 : restore saved thumbnail panel size
                    sp1.SplitterDistance = sp1.Width - (int)Configs.ThumbnailBarWidth;
                    thumbnailBar.View = ImageListView.View.Thumbnails;
                }
            }
            else {
                // Save thumbnail bar width when closing
                if (!Configs.IsThumbnailHorizontal) {
                    Configs.ThumbnailBarWidth = (uint)(sp1.Width - sp1.SplitterDistance);
                }
                sp1.SplitterWidth = 1; // right-side splitter will 'flash' unless width reset
            }
            mnuMainThumbnailBar.Checked = Configs.IsShowThumbnail;
            SelectCurrentThumbnail();

            if (!_isManuallyZoomed) {
                // Resize image to adapt when thumbbar turned off
                ApplyZoomMode(Configs.ZoomMode);
            }
        }

        private void mnuMainCheckBackground_Click(object sender, EventArgs e) {
            Configs.IsShowCheckerBoard = !Configs.IsShowCheckerBoard;
            btnCheckedBackground.Checked = Configs.IsShowCheckerBoard;

            if (btnCheckedBackground.Checked) {
                //show
                if (Configs.IsShowCheckerboardOnlyImageRegion) {
                    picMain.GridDisplayMode = ImageBoxGridDisplayMode.Image;
                }
                else {
                    picMain.GridDisplayMode = ImageBoxGridDisplayMode.Client;
                }
            }
            else {
                //hide
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.None;
            }

            mnuMainCheckBackground.Checked = btnCheckedBackground.Checked;
        }

        private void mnuFrameless_Click(object sender, EventArgs e) {
            Configs.IsWindowFrameless = !Configs.IsWindowFrameless;
            Control[] frameLessMovers = { picMain, toolMain };

            if (Configs.IsWindowFrameless) {
                FormBorderStyle = FormBorderStyle.None;

                // Enable frameless movable
                _movableForm.Enable();
                _movableForm.Enable(frameLessMovers);

                ShowToastMsg(Configs.Language.Items[$"{Name}._Frameless"], 3000);
            }
            else {
                // Disable frameless movable
                _movableForm.Disable();
                _movableForm.Disable(frameLessMovers);

                FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        private void mnuMainAlwaysOnTop_Click(object sender, EventArgs e) {
            TopMost =
                mnuMainAlwaysOnTop.Checked =
                Configs.IsWindowAlwaysOnTop = !Configs.IsWindowAlwaysOnTop;
        }

        private void mnuMainColorPicker_Click(object sender, EventArgs e) {
            Configs.IsShowColorPickerOnStartup =
                btnColorPicker.Checked =
                mnuMainColorPicker.Checked;

            ShowColorPickerTool(mnuMainColorPicker.Checked);
        }

        private void mnuMainPageNav_Click(object sender, EventArgs e) {
            Configs.IsShowPageNavOnStartup = mnuMainPageNav.Checked;

            ShowPageNavTool(mnuMainPageNav.Checked);
        }

        private void mnuMainCrop_Click(object sender, EventArgs e) {
            ShowCropTool(mnuMainCrop.Checked);
        }

        private void mnuExifTool_Click(object sender, EventArgs e) {
            if (Local.FExifTool?.IsDisposed != false) {
                Local.FExifTool = new FrmExifTool();
            }

            if (!Local.FExifTool.Visible) {
                Local.FExifTool.TopMost = TopMost;
                Local.FExifTool.Show();
            }

            if (Configs.IsExifToolAlwaysOnTop) {
                Local.FExifTool.Owner = this;
                Activate();
            }
        }

        private void mnuMainSettings_Click(object sender, EventArgs e) {
            if (Local.FSetting.IsDisposed) {
                Local.FSetting = new frmSetting();
            }

            Local.ForceUpdateActions = ForceUpdateActions.NONE;
            Local.FSetting.MainInstance = this;
            Local.FSetting.TopMost = TopMost;
            Local.FSetting.Show();
            Local.FSetting.Activate();
        }

        private void mnuMainAbout_Click(object sender, EventArgs e) {
            var f = new frmAbout {
                TopMost = TopMost
            };
            f.ShowDialog();
        }

        private void mnuMainFirstLaunch_Click(object sender, EventArgs e) {
            var p = new Process();
            p.StartInfo.FileName = App.StartUpDir("igcmd.exe");
            p.StartInfo.Arguments = "firstlaunch";

            try {
                p.Start();
            }
            catch { }
        }

        private void mnuMainCheckForUpdate_Click(object sender, EventArgs e) {
            Program.CheckForUpdate();
        }

        private void mnuMainReportIssue_Click(object sender, EventArgs e) {
            try {
                Process.Start("https://github.com/d2phap/ImageGlass/issues");
            }
            catch { }
        }

        private void mnuMainExitApplication_Click(object sender, EventArgs e) {
            // make sure app is truly exitted
            Exit(true);
        }

        private void mnuMainStartStopAnimating_Click(object sender, EventArgs e) {
            if (picMain.IsAnimating) {
                picMain.StopAnimating();
            }
            else {
                picMain.StartAnimating();
            }
        }

        private void mnuMain_Opening(object sender, CancelEventArgs e) {
            btnMenu.Checked = true;

            try {
                // Alert user if there is a new version
                if (Configs.IsNewVersionAvailable) {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = Configs.Language.Items[$"{Name}.mnuMainCheckForUpdate._NewVersion"];
                    mnuMainHelp.BackColor = mnuMainCheckForUpdate.BackColor = Color.FromArgb(35, 255, 165, 2);
                }
                else {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = Configs.Language.Items[$"{Name}.mnuMainCheckForUpdate._NoUpdate"];
                    mnuMainHelp.BackColor = mnuMainCheckForUpdate.BackColor = Color.Transparent;
                }

                mnuMainChannels.Enabled = true;
                mnuMainExtractPages.Enabled =
                    mnuMainStartStopAnimating.Enabled =
                    mnuMainPrevPage.Enabled =
                    mnuMainNextPage.Enabled =
                    mnuMainFirstPage.Enabled =
                    mnuMainLastPage.Enabled = false;

                mnuMainSetAsLockImage.Enabled = true;

                if (Local.CurrentPageCount > 1) {
                    mnuMainChannels.Enabled = false;

                    mnuMainExtractPages.Enabled =
                        mnuMainStartStopAnimating.Enabled =
                        mnuMainPrevPage.Enabled =
                        mnuMainNextPage.Enabled =
                        mnuMainFirstPage.Enabled =
                        mnuMainLastPage.Enabled = true;
                }

                mnuMainExtractPages.Text = string.Format(Configs.Language.Items[$"{Name}.mnuMainExtractPages"], Local.CurrentPageCount);

                // check if igcmdWin10.exe exists!
                if (!Helpers.IsOS(WindowsOS.Win10OrLater) || !File.Exists(App.StartUpDir("igcmdWin10.exe"))) {
                    mnuMainSetAsLockImage.Enabled = false;
                    mnuMainShare.Enabled = false;
                }

                if (Helpers.IsOS(WindowsOS.Win7)) {
                    mnuOpenWith.Enabled = false;
                }

                // add hotkey to Exit menu
                SetShortcutExit();

                // Get EditApp for editing
                UpdateEditAppInfoForMenu();
            }
            catch { }
        }

        private void mnuMain_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            btnMenu.Checked = false;
        }

        private void subMenu_DropDownOpening(object sender, EventArgs e) {
            var mnuItem = sender as ToolStripMenuItem;
            if (!mnuItem.HasDropDownItems) {
                return; // not a drop down item
            }

            // apply corner
            CornerApi.ApplyCorner(mnuItem.DropDown.Handle);

            mnuItem.DropDown.BackColor = Configs.Theme.MenuBackgroundColor;

            //get position of current menu item
            var pos = new Point(mnuItem.GetCurrentParent().Left, mnuItem.GetCurrentParent().Top);

            // Current bounds of the current monitor
            var currentScreen = Screen.FromPoint(pos);

            // Find the width of sub-menu
            var maxWidth = 0;
            foreach (var subItem in mnuItem.DropDownItems) {
                if (subItem is ToolStripMenuItem mnu) {
                    maxWidth = Math.Max(mnu.Width, maxWidth);
                }
            }
            maxWidth += 10; // Add a little wiggle room

            var farRight = pos.X + mnuMain.Width + maxWidth;
            var farLeft = pos.X - maxWidth;

            // get left and right distance to compare
            var leftGap = farLeft - currentScreen.Bounds.Left;
            var rightGap = currentScreen.Bounds.Right - farRight;

            if (leftGap >= rightGap) {
                mnuItem.DropDownDirection = ToolStripDropDownDirection.Left;
            }
            else {
                mnuItem.DropDownDirection = ToolStripDropDownDirection.Right;
            }
        }


        #endregion
    }

}
