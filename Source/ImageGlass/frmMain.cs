/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Library.Image;
using ImageGlass.Library.Comparer;
using System.IO;
using System.Diagnostics;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using System.Collections.Specialized;
using ImageGlass.Services.InstanceManagement;
using System.Drawing.Imaging;
using ImageGlass.Theme;
using System.Threading.Tasks;
using ImageGlass.Library.WinAPI;
using FileWatcherEx;
using ImageGlass.Services;

namespace ImageGlass
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            //Get DPI Scaling ratio
            //NOTE: the this.DeviceDpi property is not accurate
            DPIScaling.CurrentDPI = DPIScaling.GetSystemDpi();

            //Load UI Configs
            LoadConfig(isLoadUI: true, isLoadOthers: false);

            //Update form with new DPI
            OnDpiChanged();
            Application.DoEvents();

            /* KBR 20181009 - Fix observed bug. 
             * If picMain had input focus, CTRL+/CTRL- keys would zoom *twice*. 
             * This is disabled by turning off ImageBox shortcuts. 
             * Done here rather than in designer so this bugfix is visible.
             */
            picMain.ShortcutsEnabled = false;

        }



        #region Local variables

        // window size value before resizing
        private Size _windowSize = new Size(1000, 800);

        // determine if the image is zoomed
        private bool _isManuallyZoomed = false;

        // determine if toolbar is shown (fullscreen / slideshow)
        private bool _isShowToolbar = true;

        // determine if thumbnail is shown  (fullscreen / slideshow)
        private bool _isShowThumbnail = true;

        // determine if Windows key is pressed
        private bool _isWindowsKeyPressed = false;

        // determine if user is dragging an image file
        private bool _isDraggingImage = false;

        // gets, sets wheather the app is busy or not
        private bool _isAppBusy = false;


        // gets, sets the CancellationTokenSource of synchronious image loading task
        private System.Threading.CancellationTokenSource _cancelToken = new System.Threading.CancellationTokenSource();


        /***********************************
         * Variables for FileWatcherEx
         ***********************************/
        // the list of local deleted files, need to be deleted in the memory list
        private List<string> _queueListForDeleting = new List<string>();

        // File system watcher
        private FileWatcherEx.FileWatcherEx _fileWatcher = new FileWatcherEx.FileWatcherEx();

        #endregion



        #region Drag - drop
        private void picMain_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return;
                var dataTest = e.Data.GetData(DataFormats.FileDrop, false);
                if (dataTest == null) // observed: null w/ long path and long path support not enabled
                    return;

                string filePath = ((string[])dataTest)[0];

                // KBR 20190617 Fix observed issue: dragging from CD/DVD would fail because we set the
                // drag effect to Move, which is _not_allowed_
                // Drag file from DESKTOP to APP
                if (GlobalSetting.ImageList.IndexOf(filePath) == -1 && 
                    (e.AllowedEffect & DragDropEffects.Move) != 0)
                {
                    e.Effect = DragDropEffects.Move;
                }
                // Drag file from APP to DESKTOP
                else
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            catch
            {
                // observed: exception with a long path and long path support enabled
            }
        }

        private void picMain_DragDrop(object sender, DragEventArgs e)
        {
            // Drag file from DESKTOP to APP
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            string[] filepaths = ((string[])e.Data.GetData(DataFormats.FileDrop, false));

            if (filepaths.Length > 1)
            {
                PrepareLoading(filepaths, GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                return;
            }

            string filePath = filepaths[0];

            if (Path.GetExtension(filePath).ToLower() == ".lnk")
                filePath = Shortcuts.GetTargetPathFromShortcut(filePath);

            int imageIndex = GlobalSetting.ImageList.IndexOf(filePath);

            // The file is located another folder, load the entire folder
            if (imageIndex == -1)
            {
                PrepareLoading(filePath);
            }
            // The file is in current folder AND it is the viewing image
            else if (GlobalSetting.CurrentIndex == imageIndex)
            {
                //do nothing
            }
            // The file is in current folder AND it is NOT the viewing image
            else
            {
                GlobalSetting.CurrentIndex = imageIndex;
                NextPic(0);
            }
        }

        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isDraggingImage)
            {
                string[] paths = new string[1];
                paths[0] = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

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
        private void OpenFile()
        {
            using (var o = new OpenFileDialog()
            {
                Filter = GlobalSetting.LangPack.Items[$"{Name}._OpenFileDialog"] + "|" +
                        GlobalSetting.AllImageFormats,
                CheckFileExists = true,
            })
            {
                if (o.ShowDialog() == DialogResult.OK)
                {
                    PrepareLoading(o.FileNames, o.FileNames[0]);
                }
            }
        }


        /// <summary>
        /// Prepare to load images. This method invoked for image on the command line,
        /// i.e. when double-clicking an image.
        /// </summary>
        /// <param name="inputPath">The relative/absolute path of file/folder; or a URI Scheme</param>
        private void PrepareLoading(string inputPath)
        {
            var path = GlobalSetting.ToAbsolutePath(inputPath);
            var currentFileName = File.Exists(path) ? path : "";

            // Start loading path
            PrepareLoading(new string[] { inputPath }, currentFileName);
        }

        /// <summary>
        /// Prepare to load images
        /// </summary>
        /// <param name="inputPaths">Paths of image files or folders. It can be relative/absolute paths or URI Scheme</param>
        /// <param name="currentFileName">Current viewing filename</param>
        private async void PrepareLoading(IEnumerable<string> inputPaths, string currentFileName = "")
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            if (inputPaths.Count() == 0) return;


            var allFilesToLoad = new HashSet<string>();
            var currentFile = currentFileName;


            // Parse string to absolute path
            var paths = inputPaths.Select(item => GlobalSetting.ToAbsolutePath(item));


            // prepare the distinct dir list
            var distinctDirsList = Helper.GetDistinctDirsFromPaths(paths);


            // track paths loaded to prevent duplicates
            HashSet<string> pathsLoaded = new HashSet<string>();
            bool firstPath = true;


            await Task.Run(() =>
            {
                foreach (var apath in distinctDirsList)
                {
                    string dirPath = apath;
                    if (File.Exists(apath))
                    {
                        if (Path.GetExtension(apath).ToLower() == ".lnk")
                        {
                            dirPath = Shortcuts.GetTargetPathFromShortcut(apath);
                        }
                        else
                        {
                            dirPath = Path.GetDirectoryName(apath);
                        }
                    }
                    else if (Directory.Exists(apath))
                    {
                        // Issue #415: If the folder name ends in ALT+255 (alternate space), DirectoryInfo strips it.
                        // By ensuring a terminating slash, the problem disappears. By doing that *here*,
                        // the uses of DirectoryInfo in DirectoryFinder and FileWatcherEx are fixed as well.
                        // https://stackoverflow.com/questions/5368054/getdirectories-fails-to-enumerate-subfolders-of-a-folder-with-255-name
                        if (!apath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            dirPath = apath + Path.DirectorySeparatorChar;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    // TODO Currently only have the ability to watch a single path for changes!
                    if (firstPath)
                    {
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

                LocalSetting.InitialInputPath = string.IsNullOrEmpty(currentFile) ? (distinctDirsList.Count > 0 ? distinctDirsList[0] : "") : currentFile;
            });

            // sort list
            var sortedFilesList = SortImageList(allFilesToLoad);

            LoadImages(sortedFilesList, currentFile);
        }


        /// <summary>
        /// Load the images.
        /// </summary>
        /// <param name="imageFilenameList">The list of files to load</param>
        /// <param name="filePath">The image file path to view first</param>
        private void LoadImages(List<string> imageFilenameList, string filePath)
        {
            // Dispose all garbage
            GlobalSetting.ImageList.Dispose();

            // Set filename to image list
            GlobalSetting.ImageList = new Heart.Factory(imageFilenameList)
            {
                MaxQueue = GlobalSetting.ImageBoosterCachedCount,
                Channels = (int)LocalSetting.Channels
            };


            // Track image loading progress
            GlobalSetting.ImageList.OnFinishLoadingImage += ImageList_OnFinishLoadingImage;

            // Find the index of current image
            if (filePath.Length > 0)
            {
                GlobalSetting.CurrentIndex = GlobalSetting.ImageList.IndexOf(filePath);

                // KBR 20181009 Changing "include subfolder" setting could lose the "current" image.
                // Prefer not to report said image is "corrupt", merely reset the index in that case.
                // 1. Setting: "include subfolders: ON". Open image in folder with images in subfolders.
                // 2. Move to an image in a subfolder.
                // 3. Change setting "include subfolders: OFF".
                // Issue: the image in the subfolder is attempted to be shown, declared as corrupt/missing.
                // Issue #481: the test is incorrect when imagelist is empty (i.e. attempt to open single, hidden image with 'show hidden' OFF)
                if (GlobalSetting.CurrentIndex == -1 &&
                    GlobalSetting.ImageList.Length > 0 &&
                    !GlobalSetting.ImageList.ContainsDirPathOf(filePath))
                {
                    GlobalSetting.CurrentIndex = 0;
                }
            }
            else
            {
                GlobalSetting.CurrentIndex = 0;
            }

            // Load thumnbnail
            LoadThumbnails();

            // Cannot find the index
            if (GlobalSetting.CurrentIndex == -1)
            {
                // Mark as Image Error
                GlobalSetting.IsImageError = true;
                this.Text = $"{Application.ProductName} - {Path.GetFileName(filePath)} - {ImageInfo.GetFileSize(filePath)}";

                picMain.Text = GlobalSetting.LangPack.Items[$"{Name}.picMain._ErrorText"];
                picMain.Image = null;

                // Exit function
                return;
            }

            // Start loading image
            NextPic(0);
        }


        /// <summary>
        /// Watch a folder for changes.
        /// </summary>
        /// <param name="dirPath">The path to the folder to watch.</param>
        private void WatchPath(string dirPath)
        {
            // From Issue #530: file watcher currently fails nastily if given a prefixed path
            var pathToWatch = Heart.Helpers.DePrefixLongPath(dirPath);

            //Watch all changes of current path
            this._fileWatcher.Stop();
            this._fileWatcher = new FileWatcherEx.FileWatcherEx()
            {
                FolderPath = pathToWatch,
                IncludeSubdirectories = GlobalSetting.IsRecursiveLoading,

                // auto Invoke the form if required, no need to invidiually invoke in each event
                SynchronizingObject = this
            };

            this._fileWatcher.OnCreated += FileWatcher_OnCreated;
            this._fileWatcher.OnDeleted += FileWatcher_OnDeleted;
            this._fileWatcher.OnChanged += FileWatcher_OnChanged;
            this._fileWatcher.OnRenamed += FileWatcher_OnRenamed;

            this._fileWatcher.Start();
        }


        private void ImageList_OnFinishLoadingImage(object sender, EventArgs e)
        {
            //clear text when finishing
            DisplayTextMessage("", 0);
        }

        /// <summary>
        /// Select current thumbnail
        /// </summary>
        private void SelectCurrentThumbnail()
        {
            if (thumbnailBar.Items.Count > 0)
            {
                thumbnailBar.ClearSelection();

                try
                {
                    thumbnailBar.Items[GlobalSetting.CurrentIndex].Selected = true;
                    thumbnailBar.Items[GlobalSetting.CurrentIndex].Focused = true;
                    thumbnailBar.EnsureVisible(GlobalSetting.CurrentIndex);
                }
                catch (Exception) { }
            }
        }


        /// <summary>
        /// Sort and find all supported image from directory
        /// </summary>
        /// <param name="path">Image folder path</param>
        private IEnumerable<string> LoadImageFilesFromDirectory(string path)
        {
            //Get files from dir
            var fileList = DirectoryFinder.FindFiles(path,
                GlobalSetting.IsRecursiveLoading,
                new Predicate<FileInfo>(delegate (FileInfo fi)
                {
                    // KBR 20180607 Rework predicate to use a FileInfo instead of the filename.
                    // By doing so, can use the attribute data already loaded into memory, 
                    // instead of fetching it again (via File.GetAttributes). A re-fetch is
                    // very slow across network paths. For me, improves image load from 4+ 
                    // seconds to 0.4 seconds for a specific network path.

                    if (fi.FullName == null) // KBR not sure why but occasionally getting null filename
                        return false;

                    string extension = fi.Extension ?? "";
                    extension = extension.ToLower(); // Path.GetExtension(f).ToLower() ?? ""; //remove blank extension

                    // checks if image is hidden and ignores it if so
                    if (GlobalSetting.IsShowingHiddenImages == false)
                    {
                        var attributes = fi.Attributes; // File.GetAttributes(f);
                        var isHidden = attributes.HasFlag(FileAttributes.Hidden);
                        if (isHidden)
                        {
                            return false;
                        }
                    }
                    if (extension.Length > 0 && GlobalSetting.ImageFormatHashSet.Contains(extension))
                    {
                        return true;
                    }

                    return false;
                }));

            return fileList;
        }


        private List<string> SortImageList(IEnumerable<string> fileList)
        {
            // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!

            var list = new List<string>();

            // KBR 20190605 Fix observed limitation: to more closely match the Windows Explorer's sort
            // order, we must sort by the target column, then by name.
            var naturalSortComparer = LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc
                                        ? (IComparer<string>)new ReverseWindowsNaturalSort()
                                        : new WindowsNaturalSort();

            // KBR 20190605 Fix observed discrepancy: using UTC for create, but not for write/access times

            // Sort image file
            if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.Name)
            {
                var arr = fileList.ToArray();
                Array.Sort(arr, naturalSortComparer);
                list.AddRange(arr);
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.Length)
            {
                if (LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc)
                {
                    list.AddRange(fileList
                        .OrderByDescending(f => new FileInfo(f).Length)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else
                {
                    list.AddRange(fileList
                        .OrderBy(f => new FileInfo(f).Length)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.CreationTime)
            {
                if (LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc)
                {
                    list.AddRange(fileList
                        .OrderByDescending(f => new FileInfo(f).CreationTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else
                {
                    list.AddRange(fileList
                        .OrderBy(f => new FileInfo(f).CreationTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.Extension)
            {
                if (LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc)
                {
                    list.AddRange(fileList
                        .OrderByDescending(f => new FileInfo(f).Extension)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else
                {
                    list.AddRange(fileList
                        .OrderBy(f => new FileInfo(f).Extension)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.LastAccessTime)
            {
                if (LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc)
                {
                    list.AddRange(fileList
                        .OrderByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else
                {
                    list.AddRange(fileList
                        .OrderBy(f => new FileInfo(f).LastAccessTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.LastWriteTime)
            {
                if (LocalSetting.ActiveImageLoadingOrderType == ImageOrderType.Desc)
                {
                    list.AddRange(fileList
                        .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
                else
                {
                    list.AddRange(fileList
                        .OrderBy(f => new FileInfo(f).LastWriteTimeUtc)
                        .ThenBy(f => f, naturalSortComparer));
                }
            }
            else if (LocalSetting.ActiveImageLoadingOrder == ImageOrderBy.Random)
            {
                // NOTE: ignoring the 'descending order' setting
                list.AddRange(fileList
                    .OrderBy(f => Guid.NewGuid()));
            }

            return list;
        }


        /// <summary>
        /// Clear and reload all thumbnail image
        /// </summary>
        private void LoadThumbnails()
        {
            thumbnailBar.SuspendLayout();
            thumbnailBar.Items.Clear();
            thumbnailBar.ThumbnailSize = new Size(GlobalSetting.ThumbnailDimension, GlobalSetting.ThumbnailDimension);

            for (int i = 0; i < GlobalSetting.ImageList.Length; i++)
            {
                ImageListView.ImageListViewItem lvi = new ImageListView.ImageListViewItem(GlobalSetting.ImageList.GetFileName(i));
                lvi.Tag = GlobalSetting.ImageList.GetFileName(i);

                thumbnailBar.Items.Add(lvi);
            }
            thumbnailBar.ResumeLayout();
        }


        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        public void NextPic(int step)
        {
            // KBR 20190302 Something which has bugged me for a long time: if I'm viewing a slideshow and
            // force a 'next image', the new image is NOT shown for the length of the slideshow timer.
            // This below fixes that.
            if (GlobalSetting.IsPlaySlideShow)
            {
                timSlideShow.Enabled = false;
                timSlideShow.Enabled = true;
            }
            NextPic(step, false);
        }

        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        /// <param name="configs">Configuration for the next load</param>
        /// <param name="isSkipCache"></param>
        public async void NextPic(int step, bool isKeepZoomRatio, bool isSkipCache = false)
        {
            Timer _loadingTimer = null; // busy state timer

            System.Threading.SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());


            // cancel the previous loading task
            _cancelToken.Cancel();
            var token = new System.Threading.CancellationTokenSource();
            _cancelToken = token;

            // stop the animation
            if (picMain.IsAnimating)
            {
                picMain.StopAnimating();
            }

            //Save previous image if it was modified
            if (File.Exists(LocalSetting.ImageModifiedPath) && GlobalSetting.IsSaveAfterRotating)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._SaveChanges"], 2000);

                Application.DoEvents();
                ImageSaveChange();

                //remove the old image data from cache
                GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);

                //update thumbnail
                thumbnailBar.Items[GlobalSetting.CurrentIndex].Update();
            }

            picMain.Text = "";
            LocalSetting.IsTempMemoryData = false;

            if (GlobalSetting.ImageList.Length < 1)
            {
                Text = Application.ProductName;

                GlobalSetting.IsImageError = true;
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";

                return;
            }

            //temp index
            int tempIndex = GlobalSetting.CurrentIndex + step;

            if (!GlobalSetting.IsPlaySlideShow && !GlobalSetting.IsLoopBackViewer)
            {
                //Reach end of list
                if (tempIndex >= GlobalSetting.ImageList.Length)
                {
                    DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._LastItemOfList"], 1000);
                    return;
                }

                //Reach the first item of list
                if (tempIndex < 0)
                {
                    DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._FirstItemOfList"], 1000);
                    return;
                }
            }

            //Check if current index is greater than upper limit
            if (tempIndex >= GlobalSetting.ImageList.Length)
                tempIndex = 0;

            //Check if current index is less than lower limit
            if (tempIndex < 0)
                tempIndex = GlobalSetting.ImageList.Length - 1;

            //Update current index
            GlobalSetting.CurrentIndex = tempIndex;

            //Select thumbnail item
            SelectCurrentThumbnail();


            // Update the basic info
            UpdateStatusBar();


            //The image data will load
            Bitmap im = null;

            try
            {
                // apply Color Management settings
                GlobalSetting.ImageList.IsApplyColorProfileForAll = GlobalSetting.IsApplyColorProfileForAll;
                GlobalSetting.ImageList.ColorProfileName = GlobalSetting.ColorProfile;

                // put app in a 'busy' state around image load: allows us to prevent the user from 
                // skipping past a slow-to-load image by processing too many arrow clicks
                SetAppBusy(true);


                var bmpImg = await GlobalSetting.ImageList.GetImgAsync(
                    GlobalSetting.CurrentIndex,
                    isSkipCache: isSkipCache
                   );
                im = bmpImg.Image;


                SetAppBusy(false); // KBR Issue #485: need to clear busy state ASAP so 'Loading...' message doesn't appear after image already loaded

                GlobalSetting.IsImageError = bmpImg.Error != null;

                if (!token.Token.IsCancellationRequested)
                {
                    //Show image
                    picMain.Image = im;
                    im = null;


                    //Reset the zoom mode if isKeepZoomRatio = FALSE
                    if (!isKeepZoomRatio)
                    {
                        //reset zoom mode
                        ApplyZoomMode(GlobalSetting.ZoomMode);
                    }
                }

            }
            catch (Exception ex)
            {
                SetAppBusy(false); // make sure busy state is off if exception during image load

                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";

                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
                }
            }


            if (GlobalSetting.IsImageError)
            {
                picMain.Text = GlobalSetting.LangPack.Items[$"{Name}.picMain._ErrorText"];
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";
            }

            _isDraggingImage = false;

            //Collect system garbage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            void SetAppBusy(bool isbusy)
            {
                _isAppBusy = isbusy;
                picMain.Cursor = isbusy ? Cursors.WaitCursor : Cursors.Default;

                // Part of Issue #485 fix: failure to disable timer after image load meant message 
                // could appear after image already loaded
                if (!isbusy && _loadingTimer != null)
                {
                    _loadingTimer.Enabled = false;
                    _loadingTimer.Dispose();
                    _loadingTimer = null;
                }
                if (isbusy)
                {
                    _loadingTimer = new Timer() // can't re-use timer, re-create each time
                    {
                        Interval = 2000
                    };
                    _loadingTimer.Tick += LoadingMessageTimer_Tick;
                    _loadingTimer.Enabled = true;
                }
            }

        }


        /// <summary>
        /// Update image information on status bar
        /// </summary>
        private void UpdateStatusBar()
        {
            string appName = Application.ProductName;
            string indexTotal = string.Empty;
            string filename = string.Empty;
            string zoom = string.Empty;
            string imgSize = string.Empty;
            string fileSize = string.Empty;
            string fileDate = string.Empty;


            if (LocalSetting.IsTempMemoryData)
            {
                var imgData = GlobalSetting.LangPack.Items[$"{Name}._ImageData"];
                zoom = $"{picMain.Zoom.ToString()}%";

                if (picMain.Image != null)
                {
                    try
                    {
                        imgSize = $"{picMain.Image.Width} x {picMain.Image.Height} px";
                    }
                    catch { }

                    //(Image data)  |  {zoom}  |  {image size} - ImageGlass
                    this.Text = $"{imgData}  |  {zoom}  |  {imgSize}  - {appName}";
                }
                else
                {
                    this.Text = $"{imgData}  |  {zoom}  - {appName}";
                }
            }
            else
            {
                if (GlobalSetting.ImageList.Length < 1)
                {
                    this.Text = appName;
                    return;
                }

                string currentFilePath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

                // when there is a problem with a file, don't try to show some info
                bool isShowMoreData = File.Exists(currentFilePath);

                indexTotal = $"{(GlobalSetting.CurrentIndex + 1)}/{GlobalSetting.ImageList.Length} {GlobalSetting.LangPack.Items[$"{Name}._Text"]}";

                if (isShowMoreData)
                {
                    fileSize = ImageInfo.GetFileSize(currentFilePath);
                    fileDate = File.GetCreationTime(currentFilePath).ToString("yyyy/MM/dd HH:mm:ss");
                }


                if (GlobalSetting.IsDisplayBasenameOfImage)
                {
                    filename = Path.GetFileName(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                }
                else
                {
                    //auto ellipsis the filename
                    //the minimum text to show is Drive letter + basename.
                    //ex: C:\...\example.jpg
                    filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                    var basename = Path.GetFileName(filename);

                    var charWidth = this.CreateGraphics().MeasureString("A", this.Font).Width;
                    var textMaxLength = (this.Width - DPIScaling.TransformNumber(400)) / charWidth;

                    var maxLength = (int)Math.Max(basename.Length + 8, textMaxLength);

                    filename = Helper.ShortenPath(filename, maxLength);
                }


                if (GlobalSetting.IsImageError)
                {
                    if (!isShowMoreData) // size and date not available
                        this.Text = $"{filename}  |  {indexTotal}  - {appName}";
                    else
                        this.Text = $"{filename}  |  {indexTotal}  |  {fileSize}  |  {fileDate}  - {appName}";
                }
                else
                {
                    zoom = $"{picMain.Zoom.ToString("F2")}%";

                    if (picMain.Image != null)
                    {
                        try
                        {
                            imgSize = $"{picMain.Image.Width} x {picMain.Image.Height} px";
                        }
                        catch { }


                        this.Text = $"{filename}  |  {indexTotal}  |  {zoom}  |  {imgSize}  |  {fileSize}  |  {fileDate}  - {appName}";
                    }
                    else
                    {
                        this.Text = $"{filename}  |  {indexTotal}  |  {zoom}  |  {fileSize}  |  {fileDate}  - {appName}";
                    }
                }
            }

        }

        #endregion



        #region Key event

        //Full screen--------------------------------------------------------------------
        // Alt+Enter is a system shortcut. If we attempt to handle it as a "normal" key,
        // Windows 10 issues an obnoxious sound. Issue #555.
        protected override bool ProcessCmdKey(ref Message message, Keys keys)
        {
            if (keys == (Keys.Enter | Keys.Alt))
            {
                mnuMainFullScreen_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref message, keys);
        }


        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //this.Text = e.KeyValue.ToString();

            #region Register MAIN MENU shortcuts
            bool checkMenuShortcut(ToolStripMenuItem mnu)
            {
                Keys pressed = e.KeyCode;
                if (e.Control) pressed = pressed | Keys.Control;
                if (e.Shift) pressed = pressed | Keys.Shift;
                if (e.Alt) pressed = pressed | Keys.Alt;

                if (mnu.ShortcutKeys == pressed)
                {
                    mnu.PerformClick();
                    return true;
                }
                foreach (ToolStripMenuItem child in mnu.DropDownItems.OfType<ToolStripMenuItem>())
                {
                    checkMenuShortcut(child);
                }

                return false;
            }

            //register context menu shortcuts
            foreach (ToolStripMenuItem item in mnuMain.Items.OfType<ToolStripMenuItem>())
            {
                if (checkMenuShortcut(item))
                {
                    return;
                }
            }
            #endregion


            #region Detect WIN logo key
            _isWindowsKeyPressed = false;
            if (e.KeyData == Keys.LWin || e.KeyData == Keys.RWin)
            {
                _isWindowsKeyPressed = true;
            }
            #endregion


            //Show main menu
            #region Ctrl + `
            if (e.KeyValue == 192 && !e.Control && !e.Shift && !e.Alt) // `
            {
                mnuMain.Show(toolMain, toolMain.Width - mnuMain.Width, toolMain.Height);
            }
            #endregion


            // Rotation Counterclockwise----------------------------------------------------
            #region Ctrl + ,
            if (e.KeyValue == 188 && e.Control && !e.Shift && !e.Alt)//Ctrl + ,
            {
                mnuMainRotateCounterclockwise_Click(null, null);
                return;
            }
            #endregion


            //Rotate Clockwise--------------------------------------------------------------
            #region Ctrl + .
            if (e.KeyValue == 190 && e.Control && !e.Shift && !e.Alt)//Ctrl + .
            {
                mnuMainRotateClockwise_Click(null, null);
                return;
            }
            #endregion


            //Flip Horizontally-----------------------------------------------------------
            #region Ctrl + ;
            if (e.KeyValue == 186 && e.Control && !e.Shift && !e.Alt)//Ctrl + ;
            {
                mnuMainFlipHorz_Click(null, null);
                return;
            }
            #endregion


            //Flip Vertically-----------------------------------------------------------
            #region Ctrl + '
            if (e.KeyValue == 222 && e.Control && !e.Shift && !e.Alt)//Ctrl + '
            {
                mnuMainFlipVert_Click(null, null);
                return;
            }
            #endregion


            //Clear clipboard----------------------------------------------------------------
            #region CTRL + `
            if (e.KeyValue == 192 && e.Control && !e.Shift && !e.Alt)//CTRL + `
            {
                mnuMainClearClipboard_Click(null, null);
                return;
            }
            #endregion


            //Zoom + ------------------------------------------------------------------------
            #region Ctrl + = or = or + (numPad)
            if ((e.KeyValue == 187 || (e.KeyValue == 107 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + =
            {
                btnZoomIn_Click(null, null);
                return;
            }
            #endregion


            //Zoom - ------------------------------------------------------------------------
            #region Ctrl + - or - or - (numPad)
            if ((e.KeyValue == 189 || (e.KeyValue == 109 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + -
            {
                btnZoomOut_Click(null, null);
                return;
            }
            #endregion


            //Zoom to fit--------------------------------------------------------------------
            #region CTRL + /
            if (e.KeyValue == 191 && e.Control && !e.Shift && !e.Alt)//CTRL + /
            {
                mnuMainScaleToFit_Click(null, null);
                return;
            }
            #endregion


            //Actual size image -------------------------------------------------------------
            #region Ctrl + 0 / Ctrl + Num0 / 0 / Num0
            if (!e.Shift && !e.Alt && (e.KeyValue == 48 || e.KeyValue == 96)) // 0 || Num0 || Ctrl + 0 || Ctrl + Num0
            {
                btnActualSize_Click(null, null);
                return;
            }
            #endregion


            //ESC ultility------------------------------------------------------------------
            #region ESC
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)//ESC
            {
                //exit slideshow
                if (GlobalSetting.IsPlaySlideShow)
                {
                    mnuMainSlideShowExit_Click(null, null);
                }
                //Quit ImageGlass
                else if (GlobalSetting.IsPressESCToQuit)
                {
                    Application.Exit();
                }
                return;
            }
            #endregion


            //Ctrl---------------------------------------------------------------------------
            #region CTRL (for Zooming)
            if (e.Control && !e.Alt && !e.Shift)//Ctrl
            {
                //Enable dragging viewing image to desktop feature---------------------------
                _isDraggingImage = true;

                return;
            }
            #endregion


            //Previous Image----------------------------------------------------------------
            #region LEFT ARROW / PAGE UP
            bool no_mods = !e.Control && !e.Shift && !e.Alt;
            bool ignore = _isAppBusy || _isWindowsKeyPressed;
            if (!ignore && e.KeyValue == (int)Keys.Left && no_mods)
            {
                if (GlobalSetting.GetKeyAction(KeyCombos.LeftRight) == AssignableActions.PrevNextImage)
                {
                    NextPic(-1);
                }
                return; // fall-through lets pan happen
            }
            if (!ignore && e.KeyValue == (int)Keys.PageUp && no_mods)
            {
                var action = GlobalSetting.GetKeyAction(KeyCombos.PageUpDown);
                if (action == AssignableActions.PrevNextImage)
                {
                    NextPic(-1);
                }
                else if (action == AssignableActions.ZoomInOut)
                {
                    mnuMainZoomIn_Click(null, null);
                }

                return;
            }
            #endregion


            //Next Image---------------------------------------------------------------------
            #region RIGHT ARROW / PAGE DOWN
            if (!ignore && e.KeyValue == (int)Keys.Right && no_mods)
            {
                if (GlobalSetting.GetKeyAction(KeyCombos.LeftRight) == AssignableActions.PrevNextImage)
                {
                    NextPic(1);
                }
                return; // fall-through lets pan happen
            }
            if (!ignore && e.KeyValue == (int)Keys.PageDown && no_mods)
            {
                var action = GlobalSetting.GetKeyAction(KeyCombos.PageUpDown);
                if (action == AssignableActions.PrevNextImage)
                {
                    NextPic(1);
                }
                else if (action == AssignableActions.ZoomInOut)
                {
                    mnuMainZoomOut_Click(null, null);
                }

                return;
            }
            #endregion

            #region UP ARROW
            if (!ignore && e.KeyValue == (int)Keys.Up && no_mods)
            {
                if (GlobalSetting.GetKeyAction(KeyCombos.UpDown) == AssignableActions.ZoomInOut)
                {
                    mnuMainZoomIn_Click(null, null);
                    e.Handled = true;
                }
                return; // fall-through lets pan happen
            }
            #endregion

            #region DOWN ARROW
            if (!ignore && e.KeyValue == (int)Keys.Down && no_mods)
            {
                if (GlobalSetting.GetKeyAction(KeyCombos.UpDown) == AssignableActions.ZoomInOut)
                {
                    mnuMainZoomOut_Click(null, null);
                    e.Handled = true;
                }
                return; // fall-through lets pan happen
            }
            #endregion


            //Goto the first Image---------------------------------------------------------------
            #region HOME
            if (!_isWindowsKeyPressed && e.KeyValue == 36 &&
                !e.Control && !e.Shift && !e.Alt)
            {
                mnuMainGotoFirst_Click(null, e);
                return;
            }
            #endregion


            //Goto the last Image---------------------------------------------------------------
            #region END
            if (!_isWindowsKeyPressed && e.KeyValue == 35 &&
                !e.Control && !e.Shift && !e.Alt)
            {
                mnuMainGotoLast_Click(null, e);
                return;
            }
            #endregion

        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
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
            bool no_mods = !e.Control && !e.Shift && !e.Alt;
            if (e.KeyCode == Keys.Space && no_mods)
            {
                if (GlobalSetting.IsPlaySlideShow) // Space always pauses slideshow if playing
                {
                    mnuMainSlideShowPause_Click(null, null);
                }
                else if (GlobalSetting.GetKeyAction(KeyCombos.SpaceBack) == AssignableActions.PrevNextImage)
                {
                    NextPic(1);
                }
                return;
            }
            #endregion
            #region Backspace
            if (e.KeyCode == Keys.Back && no_mods)
            {
                if (GlobalSetting.GetKeyAction(KeyCombos.SpaceBack) == AssignableActions.PrevNextImage)
                {
                    NextPic(-1);
                }
                return;
            }
            #endregion
        }
        #endregion



        #region Private functions

        /// <summary>
        /// Update editing association app info and icon for Edit Image menu
        /// </summary>
        private void UpdateEditingAssocAppInfoForMenu()
        {
            string appName = "";
            mnuMainEditImage.Image = null;

            //Temporary memory data
            if (LocalSetting.IsTempMemoryData)
            { }
            else
            {
                //Find file format
                var ext = Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToLower();
                var assoc = GlobalSetting.GetImageEditingAssociationFromList(ext);

                //Get App assoc info
                if (assoc != null && File.Exists(assoc.AppPath))
                {
                    appName = $"({ assoc.AppName})";

                    //Update icon
                    Icon ico = Icon.ExtractAssociatedIcon(assoc.AppPath);
                    double scaleFactor = DPIScaling.GetDPIScaleFactor();
                    int iconWidth = (int)((int)Constants.MENU_ICON_HEIGHT * scaleFactor);

                    mnuMainEditImage.Image = new Bitmap(ico.ToBitmap(), iconWidth, iconWidth);
                }
            }

            mnuMainEditImage.Text = string.Format(GlobalSetting.LangPack.Items[$"{Name}.mnuMainEditImage"], appName);
        }


        /// <summary>
        /// Select and Active Zoom Mode, use GlobalSetting.ZoomMode
        /// </summary>
        private void SelectUIZoomMode()
        {
            // Reset (Disable) Zoom Lock
            GlobalSetting.ZoomLockValue = 100.0;


            switch (GlobalSetting.ZoomMode)
            {
                case ZoomMode.ScaleToFit:
                    btnScaleToFit.Checked = mnuMainScaleToFit.Checked = true;

                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked =
                        btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked =
                        btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked =
                        btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = false;
                    break;

                case ZoomMode.ScaleToWidth:
                    btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked = true;

                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked =
                        btnScaleToFit.Checked = mnuMainScaleToFit.Checked =
                        btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked =
                        btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = false;
                    break;

                case ZoomMode.ScaleToHeight:
                    btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked = true;

                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked =
                        btnScaleToFit.Checked = mnuMainScaleToFit.Checked =
                        btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked =
                        btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = false;
                    break;

                case ZoomMode.LockZoomRatio:
                    btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = true;

                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked =
                        btnScaleToFit.Checked = mnuMainScaleToFit.Checked =
                        btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked =
                        btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked = false;

                    //Enable Zoom Lock
                    GlobalSetting.ZoomLockValue = picMain.Zoom;
                    break;

                case ZoomMode.AutoZoom:
                default:
                    btnAutoZoom.Checked = mnuMainAutoZoom.Checked = true;

                    btnScaleToFit.Checked = mnuMainScaleToFit.Checked =
                        btnScaletoWidth.Checked = mnuMainScaleToWidth.Checked =
                        btnScaletoHeight.Checked = mnuMainScaleToHeight.Checked =
                        btnZoomLock.Checked = mnuMainLockZoomRatio.Checked = false;

                    break;
            }
        }


        /// <summary>
        /// Apply zoom mode
        /// </summary>
        /// <param name="zoomMode"></param>
        /// <param name="isResetScrollPosition"></param>
        private void ApplyZoomMode(ZoomMode zoomMode, bool isResetScrollPosition = true)
        {
            // Reset scrollbar position
            if (isResetScrollPosition)
            {
                picMain.ScrollTo(0, 0, 0, 0);
            }


            if (zoomMode == ZoomMode.ScaleToWidth)
            {
                if (picMain.Image == null)
                {
                    return;
                }

                // Scale to Width
                double frac = picMain.Width / (1.0 * picMain.Image.Width);
                picMain.Zoom = frac * 100;
            }
            else if (zoomMode == ZoomMode.ScaleToHeight)
            {
                if (picMain.Image == null)
                {
                    return;
                }

                // Scale to Height
                double frac = picMain.Height / (1.0 * picMain.Image.Height);
                picMain.Zoom = frac * 100;
            }
            else if (zoomMode == ZoomMode.ScaleToFit)
            {
                picMain.ZoomToFit();
            }
            else if (zoomMode == ZoomMode.LockZoomRatio)
            {
                picMain.Zoom = GlobalSetting.ZoomLockValue;
            }
            else //zoomMode == ZoomMode.AutoZoom
            {
                picMain.ZoomAuto();
            }


            if (GlobalSetting.IsCenterImage)
            {
                // auto center the image
                picMain.CenterToImage();
            }
            

            //Tell the app that it's not zoomed by user
            _isManuallyZoomed = false;

            //Get image file information
            UpdateStatusBar();
        }


        /// <summary>
        /// Start Zoom optimization
        /// </summary>
        private void ZoomOptimization()
        {
            if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationMethods.Auto)
            {
                if (picMain.Zoom > 100)
                {
                    picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                }
                else if (picMain.Zoom < 100)
                {
                    picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                }
            }
            else if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationMethods.ClearPixels)
            {
                picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
            else if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationMethods.SmoothPixels)
            {
                picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            }
        }


        /// <summary>
        /// Rename image
        /// </summary>
        private void RenameImage()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            // Fix issue #397. Original logic didn't take network paths into account.
            // Replace original logic with the Path functions to access filename bits.

            // Extract the various bits of the image path
            var filepath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            string currentFolder = Path.GetDirectoryName(filepath);
            string oldName = Path.GetFileName(filepath);
            string ext = Path.GetExtension(filepath);
            string newName = Path.GetFileNameWithoutExtension(filepath);

            //Show input box
            string str = null;

            if (InputBox.ShowDiaLog(GlobalSetting.LangPack.Items[$"{Name}._RenameDialogText"], GlobalSetting.LangPack.Items[$"{Name}._RenameDialog"], newName, false) == DialogResult.OK)
            {
                str = InputBox.Message;
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                return;
            }

            newName = str + ext;

            //duplicated name
            if (oldName == newName)
            {
                return;
            }

            try
            {
                string newFilePath = Path.Combine(currentFolder, newName);
                //Rename file
                ImageInfo.RenameFile(filepath, newFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Check and display message if the image still being loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadingMessageTimer_Tick(object sender, EventArgs e)
        {
            var timer = (Timer)sender;
            timer.Enabled = false;
            timer.Stop();
            timer.Tick -= LoadingMessageTimer_Tick;

            if (_isAppBusy)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._Loading"], 10000);
            }

            timer.Dispose();
        }


        /// <summary>
        /// Display a message on picture box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="duration">Duration (milisecond)</param>
        private void DisplayTextMessage(string msg, int duration)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, int>(DisplayTextMessage), msg, duration);
                return;
            }

            if (duration == 0)
            {
                picMain.TextBackColor = Color.Transparent;
                picMain.Font = Font;
                picMain.ForeColor = LocalSetting.Theme.TextInfoColor;
                picMain.Text = string.Empty;
                return;
            }

            Timer tmsg = new Timer
            {
                Enabled = false
            };
            tmsg.Tick += tmsg_Tick;
            tmsg.Interval = duration; //display in xxx mili seconds

            picMain.TextBackColor = Color.Black;
            picMain.Font = new Font(Font.FontFamily, 12);
            picMain.ForeColor = Color.White;
            picMain.Text = msg;

            //Start timer
            tmsg.Enabled = true;
            tmsg.Start();
        }


        /// <summary>
        /// Timer Tick event: to display the message
        /// </summary>
        private void tmsg_Tick(object sender, EventArgs e)
        {
            Timer tmsg = (Timer)sender;
            tmsg.Stop();
            tmsg.Tick -= tmsg_Tick;
            tmsg.Dispose();

            picMain.TextBackColor = Color.Transparent;
            picMain.Font = Font;
            picMain.ForeColor = Color.Black;
            picMain.Text = string.Empty;
        }


        /// <summary>
        /// Copy multiple files
        /// </summary>
        private void CopyMultiFiles()
        {
            //get filename
            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(filename))
                {
                    return;
                }
            }
            catch { return; }


            //update the list
            var fileList = new List<string>();
            fileList.AddRange(LocalSetting.StringClipboard);

            for (int i = 0; i < fileList.Count; i++)
            {
                if (!File.Exists(fileList[i]))
                {
                    LocalSetting.StringClipboard.Remove(fileList[i]);
                }
            }


            //exit if duplicated filename
            if (LocalSetting.StringClipboard.IndexOf(filename) == -1)
            {
                //add filename to clipboard
                LocalSetting.StringClipboard.Add(filename);
            }

            var fileDropList = new StringCollection();
            fileDropList.AddRange(LocalSetting.StringClipboard.ToArray());

            Clipboard.Clear();
            Clipboard.SetFileDropList(fileDropList);

            DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items[$"{Name}._CopyFileText"],
                LocalSetting.StringClipboard.Count), 1000);
        }


        /// <summary>
        /// Cut multiple files
        /// </summary>
        private void CutMultiFiles()
        {
            //get filename
            var filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(filename))
                {
                    return;
                }
            }
            catch { return; }


            //update the list
            var fileList = new List<string>();
            fileList.AddRange(LocalSetting.StringClipboard);

            for (int i = 0; i < fileList.Count; i++)
            {
                if (!File.Exists(fileList[i]))
                {
                    LocalSetting.StringClipboard.Remove(fileList[i]);
                }
            }


            //exit if duplicated filename
            if (LocalSetting.StringClipboard.IndexOf(filename) == -1)
            {
                //add filename to clipboard
                LocalSetting.StringClipboard.Add(filename);
            }


            var moveEffect = new byte[] { 2, 0, 0, 0 };
            var dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            var fileDropList = new StringCollection();
            fileDropList.AddRange(LocalSetting.StringClipboard.ToArray());

            var data = new DataObject();
            data.SetFileDropList(fileDropList);
            data.SetData("Preferred DropEffect", dropEffect);


            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);

            DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items[$"{Name}._CutFileText"],
                LocalSetting.StringClipboard.Count), 1000);
        }


        /// <summary>
        /// Save all change of image
        /// </summary>
        private async void ImageSaveChange()
        {
            try
            {
                var lastWriteTime = File.GetLastWriteTime(LocalSetting.ImageModifiedPath);
                var newBitmap = new Bitmap(picMain.Image);

                // override the current image file
                Heart.Photo.SaveImage(newBitmap, LocalSetting.ImageModifiedPath);

                // Issue #307: option to preserve the modified date/time
                if (GlobalSetting.PreserveModifiedDate)
                {
                    File.SetLastWriteTime(LocalSetting.ImageModifiedPath, lastWriteTime);
                }

                // update cache of the modified item
                var img = await GlobalSetting.ImageList.GetImgAsync(GlobalSetting.CurrentIndex);
                img.Image = newBitmap;

            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(GlobalSetting.LangPack.Items[$"{this.Name}._SaveImageError"], LocalSetting.ImageModifiedPath), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LocalSetting.ImageModifiedPath = "";
        }


        /// <summary>
        /// Handle the event when Dpi changed
        /// </summary>
        private void OnDpiChanged()
        {
            //Change grid cell size
            picMain.GridCellSize = DPIScaling.TransformNumber((int)Constants.VIEWER_GRID_SIZE);


            #region change size of toolbar
            //Update size of toolbar
            toolMain.Height = DPIScaling.TransformNumber((int)Constants.TOOLBAR_HEIGHT);

            //Get new toolbar item height
            int currentToolbarHeight = toolMain.Height;
            int newToolBarItemHeight = int.Parse(Math.Floor((currentToolbarHeight * 0.8)).ToString());

            //Update toolbar items size
            //Tool bar buttons
            foreach (var item in toolMain.Items.OfType<ToolStripButton>())
            {
                item.Size = new Size(newToolBarItemHeight, newToolBarItemHeight);
            }

            //Tool bar menu buttons
            foreach (var item in toolMain.Items.OfType<ToolStripDropDownButton>())
            {
                item.Size = new Size(newToolBarItemHeight, newToolBarItemHeight);
            }

            // get correct icon height
            var hIcon = ThemeImage.GetCorrectIconHeight();

            //Tool bar separators
            foreach (var item in toolMain.Items.OfType<ToolStripSeparator>())
            {
                item.Size = new Size(5, (int)(hIcon * 1.2));
                item.Margin = new Padding((int)(hIcon * 0.15), 0, (int)(hIcon * 0.15), 0);
            }

            //Update toolbar icon size
            var themeName = GlobalSetting.GetConfig("Theme", "default");
            Theme.Theme t = new Theme.Theme(GlobalSetting.ConfigDir(Dir.Themes, themeName));
            LoadToolbarIcons(t);

            #endregion


            #region change size of menu items
            int newMenuIconHeight = DPIScaling.TransformNumber((int)Constants.MENU_ICON_HEIGHT);

            mnuMainOpenFile.Image = 
                mnuMainZoomIn.Image =
                mnuMainViewNext.Image = 
                mnuMainSlideShowStart.Image = 
                mnuMainRotateCounterclockwise.Image = 

                mnuMainClearClipboard.Image = 
                mnuMainToolbar.Image = 
                mnuMainColorPicker.Image = 
                mnuMainAbout.Image = 
                mnuMainSettings.Image =

                new Bitmap(newMenuIconHeight, newMenuIconHeight);

            if (mnuMainChannels.DropDownItems.Count > 0)
            {
                mnuMainChannels.DropDownItems[0].Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            }

            #endregion

        }


        /// <summary>
        /// Load image data
        /// </summary>
        /// <param name="img"></param>
        private void LoadImageData(Image img)
        {
            picMain.Image = img;
            picMain.Text = "";
            LocalSetting.IsTempMemoryData = true;

            UpdateStatusBar();
        }


        /// <summary>
        /// Save current loaded image to file
        /// </summary>
        private string SaveTemporaryMemoryData()
        {
            var tempDir = GlobalSetting.ConfigDir(Dir.Temporary);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string filename = Path.Combine(tempDir, "temp_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".png");

            picMain.Image.Save(filename, ImageFormat.Png);

            return filename;
        }


        /// <summary>
        /// Update toolbar buttons alignment
        /// </summary>
        private void UpdateToolbarButtonsAlignment()
        {
            // Issue 425: option to center the toolbar buttons horizontally 
            // [useful for wide screen]
            // I'm assuming the btnMenu stays to the right, 
            // in order to always be at a fixed location.

            var firstBtn = toolMain.Items[0];
            var defaultMargin = new Padding(3, firstBtn.Margin.Top, firstBtn.Margin.Right, firstBtn.Margin.Bottom);
            

            // reset the alignment to left
            firstBtn.Margin = defaultMargin;

            if (GlobalSetting.IsCenterToolbar)
            {
                // get the correct content width
                var toolbarContentWidth = btnMenu.Width;
                foreach (ToolStripItem item in toolMain.Items)
                {
                    toolbarContentWidth += item.Width;
                }

                // if the content cannot fit the toolbar size:
                // (toolbarContentWidth > toolMain.Size.Width)
                if (toolMain.OverflowButton.Visible)
                {
                    // align left
                    firstBtn.Margin = defaultMargin;
                }
                else
                {
                    // the default margin (left alignment)
                    var margin = defaultMargin;


                    // get the gap of content width and toolbar width
                    int gap = Math.Abs(toolMain.Width - toolbarContentWidth);

                    // update the left margin value
                    margin.Left = gap / 2;

                    // align the first item
                    firstBtn.Margin = margin;
                }
            }
        }


        /// <summary>
        /// Check and run an action if cursor position is the LEFT/CENTER/RIGHT side of picMain
        /// </summary>
        /// <param name="location">Cursor Location</param>
        /// <param name="onCursorLeftAction">Action to run if Cursor Position is LEFT</param>
        /// <param name="onCursorCenterAction">Action to run if Cursor Position is CENTER</param>
        /// <param name="onCursorRightAction">Action to run if Cursor Position is RIGHT</param>
        private void CheckCursorPositionOnViewer(Point location, Action onCursorLeftAction = null, Action onCursorCenterAction = null, Action onCursorRightAction = null)
        {
            if (GlobalSetting.ImageList.Length > 1)
            {
                // calculate icon height
                var iconHeight = DPIScaling.TransformNumber((int)Constants.TOOLBAR_ICON_HEIGHT * 3);

                // get the hotpot area width
                var hotpotWidth = Math.Max(iconHeight, picMain.Width / 7);

                // left side
                if (location.X < hotpotWidth)
                {
                    // The first image in the list
                    if (!GlobalSetting.IsLoopBackViewer && GlobalSetting.CurrentIndex == 0)
                    {
                        picMain.Cursor = _isAppBusy ? Cursors.WaitCursor : Cursors.Default;
                    }
                    else
                    {
                        onCursorLeftAction?.Invoke();
                    }
                }
                // right side
                else if (location.X > picMain.Width - hotpotWidth)
                {
                    // The last image in the list
                    if (!GlobalSetting.IsLoopBackViewer && GlobalSetting.CurrentIndex >= GlobalSetting.ImageList.Length - 1)
                    {
                        picMain.Cursor = _isAppBusy ? Cursors.WaitCursor : Cursors.Default;
                    }
                    else
                    {
                        onCursorRightAction?.Invoke();
                    }
                }
                // center
                else
                {
                    onCursorCenterAction?.Invoke();
                }
            }
        }


        /// <summary>
        /// Determine the image sort order/direction based on user settings
        /// or Windows Explorer sorting.
        /// <param name="fullPath">full path to file/folder (i.e. as comes in from drag-and-drop)</param>
        /// <side_effect>Updates GlobalSetting.ActiveImageLoadingOrder</side_effect>
        /// <side_effect>Updates LocalSetting.ActiveImageLoadingOrder</side_effect>
        /// <side_effect>Updates LocalSetting.ActiveImageLoadingOrderType</side_effect>
        /// </summary>
        private void DetermineSortOrder(string fullPath)
        {
            // Read the sorting order from config
            GlobalSetting.ImageLoadingOrder = GlobalSetting.GetImageOrderConfig();

            // Initialize to the user-configured sorting order. Fetching the Explorer sort
            // order may fail, or may be on an unsupported column.
            LocalSetting.ActiveImageLoadingOrder = GlobalSetting.ImageLoadingOrder;
            LocalSetting.ActiveImageLoadingOrderType = GlobalSetting.ImageLoadingOrderType;

            // Use File Explorer sort order if possible
            if (GlobalSetting.IsUseFileExplorerSortOrder)
            {
                if (ExplorerSortOrder.GetExplorerSortOrder(fullPath, out var explorerOrder, out var isAscending))
                {
                    if (explorerOrder != null)
                    {
                        LocalSetting.ActiveImageLoadingOrder = explorerOrder.Value;
                    }

                    if (isAscending != null)
                    {
                        LocalSetting.ActiveImageLoadingOrderType = isAscending.Value ? ImageOrderType.Asc : ImageOrderType.Desc;
                    }
                }
            }
        }


        /// <summary>
        /// Load View Channels menu items
        /// </summary>
        private void LoadViewChannelsMenuItems()
        {
            // clear items
            mnuMainChannels.DropDown.Items.Clear();

            var newMenuIconHeight = DPIScaling.TransformNumber((int)Constants.MENU_ICON_HEIGHT);

            // add new items
            var channelArr = Enum.GetValues(typeof(ColorChannels));
            foreach (var channel in channelArr)
            {
                var channelName = Enum.GetName(typeof(ColorChannels), channel);
                var mnu = new ToolStripMenuItem()
                {
                    Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainChannels._{channelName}"],
                    Tag = channel,
                    CheckOnClick = true,
                    Checked = (int)channel == (int)LocalSetting.Channels,
                    ImageScaling = ToolStripItemImageScaling.None,
                    Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
                };

                mnu.Click += this.MnuViewChannelsItem_Click;
                mnuMainChannels.DropDown.Items.Add(mnu);
            }
        }

        private void MnuViewChannelsItem_Click(object sender, EventArgs e)
        {
            var mnu = (ToolStripMenuItem)sender;
            var selectedChannel = (ColorChannels)(int)mnu.Tag;

            // uncheck all menu items
            foreach (ToolStripMenuItem item in mnuMainChannels.DropDown.Items)
            {
                item.Checked = false;
            }

            // select the clicked menu
            mnu.Checked = true;

            if (selectedChannel != LocalSetting.Channels)
            {
                LocalSetting.Channels = selectedChannel;
                GlobalSetting.ImageList.Channels = (int)selectedChannel;

                // update the viewing image
                NextPic(0, true, true);

                // update cached images
                GlobalSetting.ImageList.UpdateCache();
            }
        }

        #endregion



        #region Configurations

        /// <summary>
        /// Apply ImageGlass theme
        /// </summary>
        /// <param name="themeFolderName">The folder name of theme. By default, load default theme</param>
        private Theme.Theme ApplyTheme(string themeFolderName = "default")
        {
            Theme.Theme th = new Theme.Theme(GlobalSetting.ConfigDir(Dir.Themes, themeFolderName));

            if (th.IsThemeValid)
            {
                GlobalSetting.SetConfig("Theme", themeFolderName);
            }
            LoadTheme(th);

            return th;

            void LoadTheme(Theme.Theme t)
            {
                //Remove white line under tool strip
                toolMain.Renderer = new Theme.ToolStripRenderer(t.ToolbarBackgroundColor, t.TextInfoColor);

                // <main>
                picMain.BackColor = t.BackgroundColor;
                GlobalSetting.BackgroundColor = t.BackgroundColor;

                picMain.GridColor = Color.FromArgb(15, 0, 0, 0);
                picMain.GridColorAlternate = Color.FromArgb(20, 255, 255, 255);

                toolMain.BackgroundImage = t.ToolbarBackgroundImage.Image;
                toolMain.BackColor = t.ToolbarBackgroundColor;

                thumbnailBar.BackgroundImage = t.ThumbnailBackgroundImage.Image;
                thumbnailBar.BackColor = t.ThumbnailBackgroundColor;
                sp1.BackColor = t.ThumbnailBackgroundColor;

                lblInfo.ForeColor = t.TextInfoColor;
                picMain.ForeColor = Theme.Theme.InvertBlackAndWhiteColor(GlobalSetting.BackgroundColor);

                //Modern UI menu renderer
                mnuMain.Renderer = mnuContext.Renderer = new ModernMenuRenderer(t.MenuBackgroundColor, t.MenuTextColor);

                // <toolbar_icon>
                LoadToolbarIcons(t);

                // Overflow button and Overflow dropdown
                toolMain.OverflowButton.DropDown.BackColor = t.ToolbarBackgroundColor;
                toolMain.OverflowButton.AutoSize = false;
                toolMain.OverflowButton.Padding = new Padding(DPIScaling.TransformNumber(10));
            }
        }


        /// <summary>
        /// Load toolbar icons
        /// </summary>
        /// <param name="t">Theme</param>
        private void LoadToolbarIcons(Theme.Theme t)
        {
            // <toolbar_icon>
            btnBack.Image = t.ToolbarIcons.ViewPreviousImage.Image;
            btnNext.Image = t.ToolbarIcons.ViewNextImage.Image;

            btnRotateLeft.Image = t.ToolbarIcons.RotateLeft.Image;
            btnRotateRight.Image = t.ToolbarIcons.RotateRight.Image;
            btnFlipHorz.Image = t.ToolbarIcons.FlipHorz.Image;
            btnFlipVert.Image = t.ToolbarIcons.FlipVert.Image;
            btnDelete.Image = t.ToolbarIcons.Detele.Image;

            btnZoomIn.Image = t.ToolbarIcons.ZoomIn.Image;
            btnZoomOut.Image = t.ToolbarIcons.ZoomOut.Image;
            btnScaleToFit.Image = t.ToolbarIcons.ScaleToFit.Image;
            btnActualSize.Image = t.ToolbarIcons.ActualSize.Image;
            btnZoomLock.Image = t.ToolbarIcons.LockRatio.Image;
            btnAutoZoom.Image = t.ToolbarIcons.AutoZoom.Image;
            btnScaletoWidth.Image = t.ToolbarIcons.ScaleToWidth.Image;
            btnScaletoHeight.Image = t.ToolbarIcons.ScaleToHeight.Image;
            btnWindowAutosize.Image = t.ToolbarIcons.AdjustWindowSize.Image;

            btnOpen.Image = t.ToolbarIcons.OpenFile.Image;
            btnRefresh.Image = t.ToolbarIcons.Refresh.Image;
            btnGoto.Image = t.ToolbarIcons.GoToImage.Image;

            btnThumb.Image = t.ToolbarIcons.ThumbnailBar.Image;
            btnCheckedBackground.Image = t.ToolbarIcons.Checkerboard.Image;
            btnFullScreen.Image = t.ToolbarIcons.FullScreen.Image;
            btnSlideShow.Image = t.ToolbarIcons.Slideshow.Image;
            btnConvert.Image = t.ToolbarIcons.Convert.Image;
            btnPrintImage.Image = t.ToolbarIcons.Print.Image;

            btnMenu.Image = t.ToolbarIcons.Menu.Image;
        }

        /// <summary>
        /// If true is passed, try to use a 10ms system clock for animating GIFs, otherwise
        /// use the default animator.
        /// </summary>
        private void CheckAnimationClock(bool isUsingFasterClock)
        {
            if (isUsingFasterClock)
            {
                if (!TimerAPI.HasRequestedRateAtLeastAsFastAs(10) && TimerAPI.TimeBeginPeriod(10))
                    HighResolutionGifAnimator.SetTickTimeInMilliseconds(10);
                picMain.Animator = new HighResolutionGifAnimator();
            }
            else
            {
                if (TimerAPI.HasRequestedRateAlready(10))
                    TimerAPI.TimeEndPeriod(10);
                picMain.Animator = new DefaultGifAnimator();
            }
        }

        /// <summary>
        /// Load app configurations
        /// </summary>
        private void LoadConfig(bool @isLoadUI = false, bool @isLoadOthers = true)
        {
            string configValue = string.Empty;


            #region UI SETTINGS
            if (isLoadUI)
            {

                #region Load theme
                thumbnailBar.SetRenderer(new ImageListView.ImageListViewRenderers.ThemeRenderer()); //ThumbnailBar Renderer must be done BEFORE loading theme
                string themeFolderName = GlobalSetting.GetConfig("Theme", "default");

                LocalSetting.Theme = ApplyTheme(themeFolderName);
                Application.DoEvents();
                #endregion


                #region Load checkerboard display mode
                GlobalSetting.IsShowCheckerboardOnlyImageRegion = bool.Parse(GlobalSetting.GetConfig("IsShowCheckerboardOnlyImageRegion", "False"));
                #endregion


                #region Show checkerboard
                GlobalSetting.IsShowCheckerBoard = bool.Parse(GlobalSetting.GetConfig("IsShowCheckedBackground", "False").ToString());
                GlobalSetting.IsShowCheckerBoard = !GlobalSetting.IsShowCheckerBoard;
                mnuMainCheckBackground_Click(null, EventArgs.Empty);
                #endregion


                #region Load background
                var bgValue = GlobalSetting.GetConfig("BackgroundColor", Theme.Theme.ConvertColorToHEX(LocalSetting.Theme.BackgroundColor));

                GlobalSetting.BackgroundColor = Theme.Theme.ConvertHexStringToColor(bgValue, true);
                picMain.BackColor = GlobalSetting.BackgroundColor;
                #endregion


                #region Load Toolbar buttons

                GlobalSetting.ToolbarButtons = GlobalSetting.GetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons);
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.TOOLBAR;
                frmMain_Activated(null, null);

                #endregion


                #region Load state of Toolbar 
                GlobalSetting.IsShowToolBar = bool.Parse(GlobalSetting.GetConfig("IsShowToolBar", "True"));
                GlobalSetting.IsShowToolBar = !GlobalSetting.IsShowToolBar;
                mnuMainToolbar_Click(null, EventArgs.Empty);
                #endregion


                GlobalSetting.LoadKeyAssignments();


                #region Load state of Toolbar Below Image
                var vString = GlobalSetting.GetConfig("ToolbarPosition", ((int)GlobalSetting.ToolbarPosition).ToString());

                if (Enum.TryParse(vString, out ToolbarPosition toolbarPos))
                {
                    GlobalSetting.ToolbarPosition = toolbarPos;

                    //Request frmMain to update
                    LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.TOOLBAR_POSITION;
                    frmMain_Activated(null, EventArgs.Empty);
                }
                #endregion


                #region Load Thumbnail dimension
                if (int.TryParse(GlobalSetting.GetConfig("ThumbnailDimension", GlobalSetting.ThumbnailDimension.ToString()), out int thumbDimension))
                {
                    GlobalSetting.ThumbnailDimension = thumbDimension;
                }
                else
                {
                    GlobalSetting.ThumbnailDimension = 48;
                }
                #endregion


                #region Load thumbnail bar width & position
                if (!int.TryParse(GlobalSetting.GetConfig("ThumbnailBarWidth", "0"), out int tb_width))
                {
                    tb_width = 0;
                }

                //Get minimum width needed for thumbnail dimension
                var tb_minWidth = new ThumbnailItemInfo(GlobalSetting.ThumbnailDimension, true).GetTotalDimension();
                //Get the greater width value
                GlobalSetting.ThumbnailBarWidth = Math.Max(tb_width, tb_minWidth);

                //Load thumbnail orientation state: 
                //NOTE: needs to be done BEFORE the mnuMainThumbnailBar_Click invocation below!
                GlobalSetting.IsThumbnailHorizontal = bool.Parse(GlobalSetting.GetConfig("IsThumbnailHorizontal", "True"));

                //Load vertical thumbnail bar width
                if (GlobalSetting.IsThumbnailHorizontal == false)
                {
                    if (int.TryParse(GlobalSetting.GetConfig("ThumbnailBarWidth", "48"), out int vtb_width))
                    {
                        GlobalSetting.ThumbnailBarWidth = vtb_width;
                    }
                }
                #endregion


                #region Load Thumbnail scrollbar visibility
                if (bool.TryParse(GlobalSetting.GetConfig("IsShowThumbnailScrollbar", GlobalSetting.IsShowThumbnailScrollbar.ToString()), out bool showThumbScrollbar))
                {
                    GlobalSetting.IsShowThumbnailScrollbar = showThumbScrollbar;
                }
                #endregion


                // Load View Channels menu items
                LoadViewChannelsMenuItems();



                // NOTE: ***
                // Need to load the Windows state here to fix the issue:
                // https://github.com/d2phap/ImageGlass/issues/358
                // And to IMPROVE the startup loading speed.
                #region Windows Bound (Position + Size)
                Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,1000,800"));

                if (!Helper.IsOnScreen(rc.Location))
                {
                    rc.Location = new Point(280, 125);
                }
                this.Bounds = rc;
                #endregion


                // Issue #402: need to wait to load thumbnail size etc until after window bounds.
                // The splitter dimensions may be too small for the user's last splitter bar position.
                #region Load state of Thumbnail 
                GlobalSetting.IsShowThumbnail = bool.Parse(GlobalSetting.GetConfig("IsShowThumbnail", "False"));
                //GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;
                //mnuMainThumbnailBar_Click(null, EventArgs.Empty);
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.THUMBNAIL_BAR;
                frmMain_Activated(null, EventArgs.Empty);
                #endregion


                // Windows state must be loaded after Windows Bound!
                #region Windows state
                configValue = GlobalSetting.GetConfig($"{Name}.WindowsState", "Normal");
                if (configValue == "Normal")
                {
                    this.WindowState = FormWindowState.Normal;
                }
                else if (configValue == "Maximized")
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                #endregion


            }
            #endregion


            #region OTHER SETTINGS
            if (isLoadOthers)
            {
                // NOTE: ***
                // This is a 'UI' setting which isLoadUI had previously skipped. *However*,
                // the windows *Position* is the one UI setting which *must* be applied at
                // the OnLoad event in order to 'take'.
                #region Windows Bound (Position + Size)
                Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,1000,800"));

                if (!Helper.IsOnScreen(rc.Location))
                {
                    rc.Location = new Point(280, 125);
                }
                this.Bounds = rc;
                #endregion


                #region Load Toolbar button centering state
                GlobalSetting.IsCenterToolbar = bool.Parse(GlobalSetting.GetConfig("IsCenterToolbar", GlobalSetting.IsCenterToolbar.ToString()));
                #endregion


                #region Show NavigationButtons
                GlobalSetting.IsShowNavigationButtons = bool.Parse(GlobalSetting.GetConfig("IsShowNavigationButtons", "False").ToString());
                #endregion


                #region Load language pack
                configValue = GlobalSetting.GetConfig("Language", "English");
                GlobalSetting.LangPack = new Language(configValue, GlobalSetting.StartUpDir(Dir.Languages));

                //force update language pack
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.LANGUAGE;
                frmMain_Activated(null, null);
                #endregion


                #region Read supported image formats
                var extGroups = GlobalSetting.BuiltInImageFormats.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                //Load Default Image Formats
                GlobalSetting.DefaultImageFormats = GlobalSetting.GetConfig("DefaultImageFormats", extGroups[0]);

                //Load Optional Image Formats
                GlobalSetting.OptionalImageFormats = GlobalSetting.GetConfig("OptionalImageFormats", extGroups[1]);

                if (GlobalSetting.AllImageFormats.Length == 0)
                {
                    //If no formats from settings, we need to load from built-in configs
                    GlobalSetting.LoadBuiltInImageFormats();

                    //Write configs
                    GlobalSetting.SetConfig("DefaultImageFormats", GlobalSetting.DefaultImageFormats);
                    GlobalSetting.SetConfig("OptionalImageFormats", GlobalSetting.OptionalImageFormats);
                }

                // build the hashset GlobalSetting.ImageFormatHashSet
                GlobalSetting.BuildImageFormatHashSet();
                #endregion


                #region Recursive loading
                GlobalSetting.IsRecursiveLoading = bool.Parse(GlobalSetting.GetConfig("IsRecursiveLoading", "False"));
                #endregion


                #region Show hidden images
                GlobalSetting.IsShowingHiddenImages = bool.Parse(GlobalSetting.GetConfig("IsShowingHiddenImages", "False"));
                #endregion


                #region Load Color Management settings
                GlobalSetting.IsApplyColorProfileForAll = bool.Parse(GlobalSetting.GetConfig("IsApplyColorProfileForAll", "False"));

                // get color profile
                GlobalSetting.ColorProfile = GlobalSetting.GetConfig("ColorProfile", GlobalSetting.ColorProfile);
                GlobalSetting.ColorProfile = Heart.Helpers.GetCorrectColorProfileName(GlobalSetting.ColorProfile);
                #endregion


                // Load image order config
                GlobalSetting.ImageLoadingOrder = GlobalSetting.GetImageOrderConfig();


                // Load image order type config
                GlobalSetting.ImageLoadingOrderType = GlobalSetting.GetImageOrderTypeConfig();


                // Load state of Image Booster
                GlobalSetting.IsUseFileExplorerSortOrder = bool.Parse(GlobalSetting.GetConfig("IsUseFileExplorerSortOrder", "False"));


                // Load ImageBoosterCachedCount value
                {
                    // KBR 20190716 sanity check and range check the value. Prevents exception here or in settings dialog.
                    if (!int.TryParse(GlobalSetting.GetConfig("ImageBoosterCachedCount", "1"), out var boostValue))
                        boostValue = 1;
                    GlobalSetting.ImageBoosterCachedCount = Math.Max(0, Math.Min(boostValue, 10));
                }


                // Load IsDisplayBasenameOfImage value
                GlobalSetting.IsDisplayBasenameOfImage = bool.Parse(GlobalSetting.GetConfig("IsDisplayBasenameOfImage", "False"));


                // Load IsCenterImage value
                GlobalSetting.IsCenterImage = bool.Parse(GlobalSetting.GetConfig("IsCenterImage", "True"));


                #region Slideshow Interval
                int i = int.Parse(GlobalSetting.GetConfig("SlideShowInterval", "5"));

                if (!(0 < i && i < 61)) i = 5;//time limit [1; 60] seconds
                GlobalSetting.SlideShowInterval = i;
                timSlideShow.Interval = 1000 * GlobalSetting.SlideShowInterval;
                #endregion


                #region Load Zoom Mode
                GlobalSetting.ZoomMode = (ZoomMode)Enum.Parse(typeof(ZoomMode), GlobalSetting.GetConfig("ZoomMode", "0"));


                // Load and Active Zoom Mode
                SelectUIZoomMode();


                // Load Zoom Lock Value
                int zoomLock = int.Parse(GlobalSetting.GetConfig("ZoomLockValue", "-1"), GlobalSetting.NumberFormat);
                GlobalSetting.ZoomLockValue = zoomLock > 0 ? zoomLock : 100;


                // Load ZoomLevels
                var zoomLevelStr = GlobalSetting.GetConfig("ZoomLevels");
                var zoomLevels = GlobalSetting.StringToIntArray(zoomLevelStr, unsignedOnly: true, distinct: true);
                if (zoomLevels.Length > 0)
                {
                    GlobalSetting.ZoomLevels = zoomLevels;
                }
                picMain.ZoomLevels = new ImageBoxZoomLevelCollection(GlobalSetting.ZoomLevels);

                #endregion


                #region Load scrollbars visibility
                GlobalSetting.IsScrollbarsVisible = bool.Parse(GlobalSetting.GetConfig("IsScrollbarsVisible", "False"));
                if (GlobalSetting.IsScrollbarsVisible)
                {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                }
                #endregion


                #region Load state of IsWindowAlwaysOnTop value 
                GlobalSetting.IsWindowAlwaysOnTop = bool.Parse(GlobalSetting.GetConfig("IsWindowAlwaysOnTop", "False"));
                this.TopMost = mnuMainAlwaysOnTop.Checked = GlobalSetting.IsWindowAlwaysOnTop;
                #endregion


                #region Load Color picker configs 
                //Get Color code format
                GlobalSetting.IsColorPickerRGBA = bool.Parse(GlobalSetting.GetConfig("IsColorPickerRGBA", "True"));
                GlobalSetting.IsColorPickerHEXA = bool.Parse(GlobalSetting.GetConfig("IsColorPickerHEXA", "True"));
                GlobalSetting.IsColorPickerHSLA = bool.Parse(GlobalSetting.GetConfig("IsColorPickerHSLA", "True"));


                //Get IsShowColorPicker
                LocalSetting.IsShowColorPickerOnStartup = bool.Parse(GlobalSetting.GetConfig("IsShowColorPickerOnStartup", "False"));
                if (LocalSetting.IsShowColorPickerOnStartup)
                {
                    mnuMainColorPicker.PerformClick();
                }
                #endregion


                #region Load Full Screen mode
                GlobalSetting.IsFullScreen = bool.Parse(GlobalSetting.GetConfig("IsFullScreen", "False"));
                if (GlobalSetting.IsFullScreen)
                {
                    GlobalSetting.IsFullScreen = !GlobalSetting.IsFullScreen;
                    mnuMainFullScreen.PerformClick();
                }
                #endregion


                #region Get Last Seen Image Path & Welcome Image
                GlobalSetting.IsOpenLastSeenImage = bool.Parse(GlobalSetting.GetConfig("IsOpenLastSeenImage", "False"));
                GlobalSetting.IsShowWelcome = bool.Parse(GlobalSetting.GetConfig("IsShowWelcome", "True"));

                var startUpImg = "";

                if (GlobalSetting.IsOpenLastSeenImage)
                {
                    startUpImg = GlobalSetting.GetConfig("LastSeenImagePath");
                }

                if (!File.Exists(startUpImg) && GlobalSetting.IsShowWelcome)
                {
                    startUpImg = GlobalSetting.StartUpDir("default.jpg");
                }

                //Do not show welcome image if params exist.
                if (Environment.GetCommandLineArgs().Count() < 2)
                {
                    PrepareLoading(startUpImg);
                }
                #endregion


                //load other configs in another thread
                Task.Run(() =>
                {
                    //Load IsLoopBackViewer
                    GlobalSetting.IsLoopBackViewer = ValidatedBooleanSetting("IsLoopBackViewer", true);

                    //Load IsLoopBackSlideShow
                    GlobalSetting.IsLoopBackSlideShow = ValidatedBooleanSetting("IsLoopBackSlideShow", true);

                    //Load IsPressESCToQuit
                    GlobalSetting.IsPressESCToQuit = ValidatedBooleanSetting("IsPressESCToQuit", true);


                    #region Zoom optimization method 
                    string configValue2 = GlobalSetting.GetConfig("ZoomOptimization", "0");
                    if (int.TryParse(configValue2, out int zoomValue))
                    {
                        if (-1 < zoomValue && zoomValue < Enum.GetNames(typeof(ZoomOptimizationMethods)).Length)
                        { }
                        else
                        {
                            zoomValue = 0;
                        }
                    }
                    GlobalSetting.ZoomOptimizationMethod = (ZoomOptimizationMethods)zoomValue;
                    #endregion


                    #region Get mouse wheel settings 
                    configValue2 = GlobalSetting.GetConfig("MouseWheelAction", "1");

                    if (int.TryParse(configValue2, out var mouseWheel))
                    {
                        if (Enum.IsDefined(typeof(MouseWheelActions), mouseWheel))
                        { }
                        else
                        {
                            mouseWheel = 1; //MouseWheelActions.ZOOM
                        }
                    }
                    else
                    {
                        mouseWheel = 1;
                    }
                    GlobalSetting.MouseWheelAction = (MouseWheelActions)mouseWheel;

                    configValue2 = GlobalSetting.GetConfig("MouseWheelCtrlAction", "1");
                    if (int.TryParse(configValue2, out mouseWheel))
                    {
                        if (Enum.IsDefined(typeof(MouseWheelActions), mouseWheel))
                        { }
                        else
                        {
                            mouseWheel = 1; //MouseWheelActions.ZOOM
                        }
                    }
                    else
                    {
                        mouseWheel = 1;
                    }
                    GlobalSetting.MouseWheelCtrlAction = (MouseWheelActions)mouseWheel;

                    configValue2 = GlobalSetting.GetConfig("MouseWheelShiftAction", "1");
                    if (int.TryParse(configValue2, out mouseWheel))
                    {
                        if (Enum.IsDefined(typeof(MouseWheelActions), mouseWheel))
                        { }
                        else
                        {
                            mouseWheel = 1; //MouseWheelActions.ZOOM
                        }
                    }
                    else
                    {
                        mouseWheel = 1;
                    }
                    GlobalSetting.MouseWheelShiftAction = (MouseWheelActions)mouseWheel;

                    configValue2 = GlobalSetting.GetConfig("MouseWheelAltAction", "1");
                    if (int.TryParse(configValue2, out mouseWheel))
                    {
                        if (Enum.IsDefined(typeof(MouseWheelActions), mouseWheel))
                        { }
                        else
                        {
                            mouseWheel = 1; //MouseWheelActions.ZOOM
                        }
                    }
                    else
                    {
                        mouseWheel = 1;
                    }
                    GlobalSetting.MouseWheelAltAction = (MouseWheelActions)mouseWheel;
                    #endregion


                    //Get IsConfirmationDelete value
                    GlobalSetting.IsConfirmationDelete = ValidatedBooleanSetting("IsConfirmationDelete", false);


                    //Get IsSaveAfterRotating value
                    GlobalSetting.IsSaveAfterRotating = ValidatedBooleanSetting("IsSaveAfterRotating", false);

                    // Fetch PreserveModifiedDate
                    GlobalSetting.PreserveModifiedDate = ValidatedBooleanSetting("PreserveModifiedDate", false);


                    #region Get ImageEditingAssociationList
                    configValue2 = GlobalSetting.GetConfig("ImageEditingAssociationList", "");
                    string[] editingAssoclist = configValue2.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (editingAssoclist.Length > 0)
                    {
                        foreach (var configString in editingAssoclist)
                        {
                            try
                            {
                                var extAssoc = new ImageEditingAssociation(configString);
                                GlobalSetting.ImageEditingAssociationList.Add(extAssoc);
                            }
                            catch (InvalidCastException) { }
                        }
                    }
                    #endregion


                    //Get IsNewVersionAvailable
                    GlobalSetting.IsNewVersionAvailable = ValidatedBooleanSetting("IsNewVersionAvailable", false);


                    bool ValidatedBooleanSetting(string configSetting, bool defaultValue)
                    {
                        // KBR 20190716 handle possibly gibberish values in the config file.
                        // If we don't use TryParse, an exception would happen and other settings
                        // would not be read.

                        string boolConfigValue = GlobalSetting.GetConfig(configSetting, defaultValue.ToString());
                        if (!bool.TryParse(boolConfigValue, out var value))
                            value = defaultValue;
                        return value;
                    }


                });


            }
            #endregion

        }


        /// <summary>
        /// Save app configurations
        /// </summary>
        private void SaveConfig()
        {
            if (WindowState == FormWindowState.Normal)
            {
                // don't save Bound if in Full screen and SlideShow mode
                if (!GlobalSetting.IsFullScreen && !GlobalSetting.IsPlaySlideShow)
                {
                    //Windows Bound--------------------------------------------------------------
                    GlobalSetting.SetConfig($"{Name}.WindowsBound", GlobalSetting.RectToString(this.Bounds));
                }
            }

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig($"{Name}.WindowsState", WindowState.ToString());

            //Checked background
            GlobalSetting.SetConfig("IsShowCheckedBackground", GlobalSetting.IsShowCheckerBoard.ToString());


            #region  Toolbar state
            if (!GlobalSetting.IsPlaySlideShow)
            {
                GlobalSetting.SetConfig("IsShowToolBar", GlobalSetting.IsShowToolBar.ToString());
            }

            GlobalSetting.SetConfig("IsShowThumbnailScroll", GlobalSetting.IsShowThumbnailScrollbar.ToString());
            #endregion


            //Window always on top
            GlobalSetting.SetConfig("IsWindowAlwaysOnTop", GlobalSetting.IsWindowAlwaysOnTop.ToString());

            //Zoom Mode
            GlobalSetting.SetConfig("ZoomMode", GlobalSetting.ZoomMode.ToString());

            //Lock zoom ratio
            GlobalSetting.SetConfig("ZoomLockValue", (GlobalSetting.ZoomMode == ZoomMode.LockZoomRatio) ? GlobalSetting.ZoomLockValue.ToString(GlobalSetting.NumberFormat) : "-1");


            #region Thumbnail panel
            if (!GlobalSetting.IsPlaySlideShow)
            {
                GlobalSetting.SetConfig("IsShowThumbnail", GlobalSetting.IsShowThumbnail.ToString());
            }
            #endregion


            // Save thumbnail bar orientation state
            GlobalSetting.SetConfig("IsThumbnailHorizontal", GlobalSetting.IsThumbnailHorizontal.ToString());

            //Save thumbnail bar width
            GlobalSetting.ThumbnailBarWidth = sp1.Width - sp1.SplitterDistance;
            GlobalSetting.SetConfig("ThumbnailBarWidth", GlobalSetting.ThumbnailBarWidth.ToString(GlobalSetting.NumberFormat));

            // Save vertical thumbnail bar width
            if (GlobalSetting.IsThumbnailHorizontal == false)
            {
                GlobalSetting.SetConfig("ThumbnailBarWidth", (sp1.Width - sp1.SplitterDistance).ToString(GlobalSetting.NumberFormat));
            }

            //Save previous image if it was modified
            if (File.Exists(LocalSetting.ImageModifiedPath) && GlobalSetting.IsSaveAfterRotating)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._SaveChanges"], 1000);

                Application.DoEvents();
                ImageSaveChange();
            }

            //Save IsShowColorPickerOnStartup
            GlobalSetting.SetConfig("IsShowColorPickerOnStartup", LocalSetting.IsShowColorPickerOnStartup.ToString());

            //Save toolbar buttons
            GlobalSetting.SetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons); // KBR


            // Save last seen image path
            GlobalSetting.SetConfig("LastSeenImagePath", GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));

            // Save centering of toolbar buttons
            GlobalSetting.SetConfig("IsCenterToolbar", GlobalSetting.IsCenterToolbar.ToString()); // KBR

            // Save fullscreen state
            GlobalSetting.SetConfig("IsFullScreen", GlobalSetting.IsFullScreen.ToString());


            GlobalSetting.SaveKeyAssignments();
        }


        /// <summary>
        /// Enter or Exit Full screen mode
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="changeWindowState"></param>
        /// <param name="onlyShowViewer">Hide all layouts except main viewer</param>
        private void FullScreenMode(bool enabled = true, bool changeWindowState = true, bool onlyShowViewer = false)
        {
            //full screen
            if (enabled)
            {
                SaveConfig();

                //save last state of toolbar
                if (onlyShowViewer)
                {
                    _isShowToolbar = GlobalSetting.IsShowToolBar;
                    _isShowThumbnail = GlobalSetting.IsShowThumbnail;
                }

                if (changeWindowState)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Normal;
                    this.Bounds = Screen.FromControl(this).Bounds;
                }

                //Hide toolbar
                if (onlyShowViewer)
                {
                    toolMain.Visible = false;
                    GlobalSetting.IsShowToolBar = false;
                    mnuMainToolbar.Checked = false;

                    //hide thumbnail
                    GlobalSetting.IsShowThumbnail = true;
                    mnuMainThumbnailBar_Click(null, null);
                }

                Application.DoEvents();


                //realign image
                if (!_isManuallyZoomed)
                {
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }

            }

            //exit full screen
            else
            {
                //restore last state of toolbar
                if (onlyShowViewer)
                {
                    GlobalSetting.IsShowToolBar = _isShowToolbar;
                    GlobalSetting.IsShowThumbnail = _isShowThumbnail;
                }

                // restore background color in case of being overriden by SlideShow mode
                picMain.BackColor = GlobalSetting.BackgroundColor;

                if (changeWindowState)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;

                    //windows state
                    string state_str = GlobalSetting.GetConfig($"{Name}.WindowsState", "Normal");
                    if (state_str == "Normal")
                    {
                        this.WindowState = FormWindowState.Normal;
                    }
                    else if (state_str == "Maximized")
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }

                    //Windows Bound (Position + Size)
                    this.Bounds = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,750,545"));
                }


                if (onlyShowViewer)
                {
                    if (GlobalSetting.IsShowToolBar)
                    {
                        //Show toolbar
                        toolMain.Visible = true;
                        mnuMainToolbar.Checked = true;

                        UpdateToolbarButtonsAlignment();
                    }

                    if (GlobalSetting.IsShowThumbnail)
                    {
                        //Show thumbnail
                        GlobalSetting.IsShowThumbnail = false;
                        mnuMainThumbnailBar_Click(null, null);
                    }
                }

                Application.DoEvents();


                //realign image
                if (!_isManuallyZoomed)
                {
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }
            }
        }

        #endregion



        #region Form events
        protected override void WndProc(ref Message m)
        {
            //Check if the received message is WM_SHOWME
            if (m.Msg == NativeMethods.WM_SHOWME)
            {
                //Set frmMain of the first instance to TopMost
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }
                // get our current "TopMost" value (ours will always be false though)
                bool top = TopMost;
                // make our form jump to the top of everything
                TopMost = true;
                // set it back to whatever it was
                TopMost = top;
            }
            //This message is sent when the form is dragged to a different monitor i.e. when
            //the bigger part of its are is on the new monitor. 
            else if (m.Msg == DPIScaling.WM_DPICHANGED)
            {
                DPIScaling.CurrentDPI = DPIScaling.LOWORD((int)m.WParam);
                OnDpiChanged();
            }
            else if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                // When user clicks on MAXIMIZE button on title bar
                if (m.WParam == new IntPtr(0xF030)) // Maximize event - SC_MAXIMIZE from Winuser.h
                {
                    // The window is being maximized
                    if (!_isManuallyZoomed)
                    {
                        ApplyZoomMode(GlobalSetting.ZoomMode);
                    }
                }
                // When user clicks on the RESTORE button on title bar
                else if (m.WParam == new IntPtr(0xF120)) // Restore event - SC_RESTORE from Winuser.h
                {
                    // The window is being restored
                    if (!_isManuallyZoomed)
                    {
                        ApplyZoomMode(GlobalSetting.ZoomMode);
                    }
                }
            }


            base.WndProc(ref m);
        }



        private void frmMain_Load(object sender, EventArgs e)
        {
            //Load Other Configs
            LoadConfig(isLoadUI: false, isLoadOthers: true);

            //Trigger Mouse Wheel event
            picMain.MouseWheel += picMain_MouseWheel;


            //Try to use a faster image clock for animating GIFs
            CheckAnimationClock(true);

            //Load image from param
            LoadFromParams(Environment.GetCommandLineArgs());

            //Start thread to watching deleted files
            System.Threading.Thread thDeleteWorker = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadWatcherDeleteFiles))
            {
                Priority = System.Threading.ThreadPriority.BelowNormal,
                IsBackground = true
            };
            thDeleteWorker.Start();

            // update the alignment of toolbar buttons
            UpdateToolbarButtonsAlignment();

        }

        public void LoadFromParams(string[] args)
        {
            //Load image from param
            if (args.Length >= 2)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    //only read the path, exclude configs parameter which starts with "--"
                    if (!args[i].StartsWith("--"))
                    {
                        PrepareLoading(args[i]);
                        break;
                    }
                }

            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //stop the file system watcher
                this._fileWatcher.Stop();
                this._fileWatcher.Dispose();

                //clear temp files
                var tempDir = GlobalSetting.ConfigDir(Dir.Temporary);
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }

                SaveConfig();
            }
            catch { }
        }

        private void frmMain_Deactivate(object sender, EventArgs e)
        {
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            var flags = LocalSetting.ForceUpdateActions;

            //do nothing
            if (flags == MainFormForceUpdateAction.NONE) return;


            #region LANGUAGE
            if ((flags & MainFormForceUpdateAction.LANGUAGE) == MainFormForceUpdateAction.LANGUAGE)
            {
                #region Update language strings

                #region Toolbar
                btnBack.ToolTipText = string.Format("{0} ({1})", GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewPrevious"], GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewPrevious.Shortcut"]);
                btnNext.ToolTipText = string.Format("{0} ({1})", GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewNext"], GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewNext.Shortcut"]);

                btnRotateLeft.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnRotateLeft"];
                btnRotateRight.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnRotateRight"];
                btnFlipHorz.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnFlipHorz"];
                btnFlipVert.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnFlipVert"];
                btnDelete.ToolTipText = $"{GlobalSetting.LangPack.Items[$"{Name}.mnuMainMoveToRecycleBin"]} ({mnuMainMoveToRecycleBin.ShortcutKeys.ToString()})";
                btnZoomIn.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnZoomIn"];
                btnZoomOut.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnZoomOut"];
                btnAutoZoom.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnAutoZoom"];
                btnScaleToFit.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnScaleToFit"];
                btnActualSize.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnActualSize"];
                btnZoomLock.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnZoomLock"];
                btnScaletoWidth.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnScaletoWidth"];
                btnScaletoHeight.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnScaletoHeight"];
                btnWindowAutosize.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnWindowAutosize"];
                btnOpen.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnOpen"];
                btnRefresh.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnRefresh"];
                btnGoto.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnGoto"];
                btnThumb.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnThumb"];
                btnCheckedBackground.ToolTipText = $"{GlobalSetting.LangPack.Items[$"{Name}.mnuMainCheckBackground"]} (Ctrl + B)"; ;
                btnFullScreen.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnFullScreen"];
                btnSlideShow.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnSlideShow"];
                btnConvert.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnConvert"];
                btnPrintImage.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnPrintImage"];
                btnMenu.ToolTipText = GlobalSetting.LangPack.Items[$"{Name}.btnMenu"];
                #endregion


                #region Main menu

                #region Menu File
                mnuMainFile.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainFile"];
                mnuMainOpenFile.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainOpenFile"];
                mnuMainOpenImageData.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainOpenImageData"];
                mnuMainNewWindow.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainNewWindow"];
                mnuMainSaveAs.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSaveAs"];
                mnuMainRefresh.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainRefresh"];
                mnuMainReloadImage.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainReloadImage"];
                mnuMainReloadImageList.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainReloadImageList"];
                mnuMainEditImage.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainEditImage"];
                mnuMainPrint.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainPrint"];
                #endregion


                #region Menu Navigation
                mnuMainNavigation.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainNavigation"];
                mnuMainViewNext.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewNext"];
                mnuMainViewNext.ShortcutKeyDisplayString = GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewNext.Shortcut"];
                mnuMainViewPrevious.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewPrevious"];
                mnuMainViewPrevious.ShortcutKeyDisplayString = GlobalSetting.LangPack.Items[$"{Name}.mnuMainViewPrevious.Shortcut"];

                mnuMainGoto.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainGoto"];
                mnuMainGotoFirst.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainGotoFirst"];
                mnuMainGotoLast.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainGotoLast"];
                #endregion


                #region Menu Zoom
                mnuMainZoom.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainZoom"];
                mnuMainZoomIn.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainZoomIn"];
                mnuMainZoomOut.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainZoomOut"];
                mnuMainScaleToFit.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainScaleToFit"];
                mnuMainActualSize.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainActualSize"];
                mnuMainLockZoomRatio.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainLockZoomRatio"];
                mnuMainAutoZoom.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainAutoZoom"];
                mnuMainScaleToWidth.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainScaleToWidth"];
                mnuMainScaleToHeight.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainScaleToHeight"];
                mnuMainWindowAdaptImage.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainWindowAdaptImage"];
                #endregion


                #region Menu Image
                mnuMainImage.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainImage"];
                mnuMainRotateCounterclockwise.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainRotateCounterclockwise"];
                mnuMainRotateClockwise.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainRotateClockwise"];
                mnuMainFlipHorz.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainFlipHorz"];
                mnuMainFlipVert.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainFlipVert"];

                mnuMainRename.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainRename"];
                mnuMainMoveToRecycleBin.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainMoveToRecycleBin"];
                mnuMainDeleteFromHardDisk.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainDeleteFromHardDisk"];
                mnuMainExtractFrames.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainExtractFrames"];
                mnuMainStartStopAnimating.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainStartStopAnimating"];
                mnuMainSetAsDesktop.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSetAsDesktop"];
                mnuMainSetAsLockImage.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSetAsLockImage"];
                mnuMainImageLocation.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainImageLocation"];
                mnuMainImageProperties.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainImageProperties"];
                mnuMainChannels.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainChannels"];
                LoadViewChannelsMenuItems(); // update Channels menu items
                #endregion


                #region Menu CLipboard
                mnuMainClipboard.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainClipboard"];
                mnuMainCopy.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCopy"];
                mnuMainCopyImageData.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCopyImageData"];
                mnuMainCut.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCut"];
                mnuMainCopyImagePath.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCopyImagePath"];
                mnuMainClearClipboard.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainClearClipboard"];
                #endregion


                #region Menu Slideshow
                mnuMainSlideShow.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSlideShow"];
                mnuMainSlideShowStart.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSlideShowStart"];
                mnuMainSlideShowPause.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSlideShowPause"];
                mnuMainSlideShowExit.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSlideShowExit"];
                #endregion


                #region Menu Layout
                mnuMainLayout.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainLayout"];
                mnuMainToolbar.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainToolbar"];
                mnuMainThumbnailBar.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainThumbnailBar"];
                mnuMainCheckBackground.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCheckBackground"];
                mnuMainAlwaysOnTop.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainAlwaysOnTop"];
                #endregion


                #region Menu Tools
                mnuMainTools.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainTools"];
                mnuMainColorPicker.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainColorPicker"];
                #endregion


                #region Menu Help
                mnuMainHelp.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainHelp"];
                mnuMainAbout.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainAbout"];
                mnuMainFirstLaunch.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainFirstLaunch"];
                mnuMainReportIssue.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainReportIssue"];
                #endregion


                mnuMainFullScreen.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainFullScreen"];
                mnuMainShare.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainShare"];
                mnuMainSettings.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainSettings"];
                mnuMainExitApplication.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainExitApplication"];

                #endregion

                #endregion

                //Update language layout ------------------
                RightToLeft = GlobalSetting.LangPack.IsRightToLeftLayout;
            }
            #endregion


            #region THUMBNAIL_BAR or THUMBNAIL_ITEMS
            if ((flags & MainFormForceUpdateAction.THUMBNAIL_BAR) == MainFormForceUpdateAction.THUMBNAIL_BAR || (flags & MainFormForceUpdateAction.THUMBNAIL_ITEMS) == MainFormForceUpdateAction.THUMBNAIL_ITEMS)
            {
                //Update thumbnail bar position
                GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;
                mnuMainThumbnailBar_Click(null, null);

                //Update thumbnail bar scroll bar visibility
                thumbnailBar.ScrollBars = GlobalSetting.IsShowThumbnailScrollbar;
            }
            #endregion


            #region THUMBNAIL_ITEMS
            if ((flags & MainFormForceUpdateAction.THUMBNAIL_ITEMS) == MainFormForceUpdateAction.THUMBNAIL_ITEMS)
            {
                //Update thumbnail image size
                LoadThumbnails();
            }
            #endregion


            #region COLOR_PICKER_MENU
            if ((flags & MainFormForceUpdateAction.COLOR_PICKER_MENU) == MainFormForceUpdateAction.COLOR_PICKER_MENU)
            {
                mnuMainColorPicker.Checked = LocalSetting.IsColorPickerToolOpening;
            }
            #endregion


            #region THEME
            if ((flags & MainFormForceUpdateAction.THEME) == MainFormForceUpdateAction.THEME)
            {
                ApplyTheme(LocalSetting.Theme.ThemeFolderName);
                LocalSetting.FColorPicker.UpdateUI();
            }
            #endregion


            #region TOOLBAR
            if ((flags & MainFormForceUpdateAction.TOOLBAR) == MainFormForceUpdateAction.TOOLBAR)
            {
                frmSetting.UpdateToolbarButtons(toolMain, this);
                toolMain.Items.Add(btnMenu);
                toolMain.Items.Add(lblInfo);
            }
            #endregion


            #region TOOLBAR_POSITION
            if ((flags & MainFormForceUpdateAction.TOOLBAR_POSITION) == MainFormForceUpdateAction.TOOLBAR_POSITION)
            {
                if (GlobalSetting.ToolbarPosition == ToolbarPosition.Top)
                {
                    toolMain.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    toolMain.Dock = DockStyle.Top;
                }
                else if (GlobalSetting.ToolbarPosition == ToolbarPosition.Bottom)
                {
                    toolMain.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                    toolMain.Dock = DockStyle.Bottom;
                }


                // For centered toolbar buttons
                UpdateToolbarButtonsAlignment();
            }
            #endregion


            #region IMAGE_LIST
            if ((flags & MainFormForceUpdateAction.IMAGE_LIST) == MainFormForceUpdateAction.IMAGE_LIST)
            {
                // update image list
                MnuMainReloadImageList_Click(null, null);
            }
            #endregion


            #region IMAGE_LIST_NO_RECURSIVE
            if ((flags & MainFormForceUpdateAction.IMAGE_LIST_NO_RECURSIVE) == MainFormForceUpdateAction.IMAGE_LIST_NO_RECURSIVE)
            {
                // update image list with the initial input path
                PrepareLoading(new string[] { LocalSetting.InitialInputPath }, GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            }
            #endregion


            #region OTHER_SETTINGS
            if ((flags & MainFormForceUpdateAction.OTHER_SETTINGS) == MainFormForceUpdateAction.OTHER_SETTINGS)
            {
                #region Update Other Settings

                //Update scrollbars visibility
                if (GlobalSetting.IsScrollbarsVisible)
                {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Auto;
                }
                else
                {
                    picMain.HorizontalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
                    picMain.VerticalScrollBarStyle = ImageBoxScrollBarStyle.Hide;
                }

                // update checkerboard display mode
                if (GlobalSetting.IsShowCheckerBoard)
                {
                    GlobalSetting.IsShowCheckerBoard = false;
                    mnuMainCheckBackground_Click(null, null);
                }

                //Update background---------------------
                picMain.BackColor = GlobalSetting.BackgroundColor;

                //Update slideshow interval value of timer
                timSlideShow.Interval = GlobalSetting.SlideShowInterval * 1000;

                // Update ZoomLevels
                picMain.ZoomLevels = new ImageBoxZoomLevelCollection(GlobalSetting.ZoomLevels);

                #endregion

            }
            #endregion


            #region Windows 10 Specific Actions
            bool isWin81OrLater = true;
            var winVersion = Environment.OSVersion;

            // Win7 == 6.1, Win Server 2008 == 6.1
            // Win10 == 10.0 [if app.manifest properly configured]
            if (winVersion.Version.Major < 6 ||
                winVersion.Version.Major == 6 &&
                winVersion.Version.Minor < 2)
            {
                isWin81OrLater = false; // Not running Windows 8 or earlier
            }


            mnuMainSetAsLockImage.Enabled = isWin81OrLater;
            #endregion


            LocalSetting.ForceUpdateActions = MainFormForceUpdateAction.NONE;
        }

        private void frmMain_ResizeBegin(object sender, EventArgs e)
        {
            _windowSize = Size;
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            if (Size != _windowSize)
            {
                SaveConfig();
            }
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (!_isManuallyZoomed)
            {
                ApplyZoomMode(GlobalSetting.ZoomMode);
            }

            UpdateToolbarButtonsAlignment();
        }

        private void thumbnailBar_ItemClick(object sender, ImageListView.ItemClickEventArgs e)
        {
            GlobalSetting.CurrentIndex = e.Item.Index;
            NextPic(0);
        }

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            // KBR 20190302 perform this check first: if user hits 'End' during slideshow,
            // the slideshow would start over at beginning, even if IsLoopBackSlideShow was false
            //stop playing slideshow at last image
            if (GlobalSetting.CurrentIndex == GlobalSetting.ImageList.Length - 1)
            {
                if (!GlobalSetting.IsLoopBackSlideShow)
                {
                    mnuMainSlideShowPause_Click(null, null);
                    return;
                }
            }

            NextPic(1);

        }




        #region File System Watcher events

        private void FileWatcher_OnRenamed(object sender, FileChangedEvent e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<object, FileChangedEvent>(FileWatcher_OnRenamed), sender, e);
                return;
            }

            string newFilename = e.FullPath;
            string oldFilename = e.OldFullPath;


            var oldExt = Path.GetExtension(oldFilename).ToLower();
            var newExt = Path.GetExtension(newFilename).ToLower();

            // Only watch the supported file types
            if (!GlobalSetting.ImageFormatHashSet.Contains(oldExt) && !GlobalSetting.ImageFormatHashSet.Contains(newExt))
            {
                return;
            }


            //Get index of renamed image
            int imgIndex = GlobalSetting.ImageList.IndexOf(oldFilename);


            //if user changed file extension
            if (oldExt.CompareTo(newExt) != 0)
            {
                // [old] && [new]: update filename only
                if (GlobalSetting.ImageFormatHashSet.Contains(oldExt) && GlobalSetting.ImageFormatHashSet.Contains(newExt))
                {
                    if (imgIndex > -1)
                    {
                        RenameAction();
                    }
                }
                else
                {
                    // [old] && ![new]: remove from image list
                    if (GlobalSetting.ImageFormatHashSet.Contains(oldExt))
                    {
                        DoDeleteFiles(oldFilename);
                    }
                    // ![old] && [new]: add to image list
                    else if (GlobalSetting.ImageFormatHashSet.Contains(newExt))
                    {
                        FileWatcher_AddNewFileAction(newFilename);
                    }
                }
            }
            //if user changed filename only (not extension)
            else
            {
                if (imgIndex > -1)
                {
                    RenameAction();
                }
            }


            void RenameAction()
            {
                //Rename file in image list
                GlobalSetting.ImageList.SetFileName(imgIndex, newFilename);

                //Update status bar title
                UpdateStatusBar();

                try
                {
                    //Rename image in thumbnail bar
                    thumbnailBar.Items[imgIndex].Text = Path.GetFileName(e.FullPath);
                    thumbnailBar.Items[imgIndex].Tag = newFilename;
                }
                catch { }

                // User renamed the initial file - update in case of list reload
                if (oldFilename == LocalSetting.InitialInputPath)
                    LocalSetting.InitialInputPath = newFilename;
            }


        }


        private void FileWatcher_OnChanged(object sender, FileChangedEvent e)
        {
            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();
            if (!GlobalSetting.ImageFormatHashSet.Contains(ext))
            {
                return;
            }

            // update the viewing image
            var imgIndex = GlobalSetting.ImageList.IndexOf(e.FullPath);

            // KBR 20180827 When downloading using Chrome, the downloaded file quickly transits
            // from ".tmp" > ".jpg.crdownload" > ".jpg". The last is a "changed" event, and the
            // final ".jpg" cannot exist in the ImageList. Fire this off to the "rename" logic
            // so the new file is correctly added. [Could it be the "created" instead?]
            if (imgIndex == -1)
            {
                this.Invoke(new Action<object, FileChangedEvent>(FileWatcher_OnRenamed), sender, e);
                return;
            }

            if (imgIndex == GlobalSetting.CurrentIndex)
            {
                NextPic(0, true, true);
            }

            //update thumbnail
            thumbnailBar.Items[imgIndex].Update();
        }


        private void FileWatcher_OnCreated(object sender, FileChangedEvent e)
        {
            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();

            if (!GlobalSetting.ImageFormatHashSet.Contains(ext))
            {
                return;
            }

            if (GlobalSetting.ImageList.IndexOf(e.FullPath) == -1)
            {
                FileWatcher_AddNewFileAction(e.FullPath);
            }
        }


        private void FileWatcher_OnDeleted(object sender, FileChangedEvent e)
        {
            // Only watch the supported file types
            var ext = Path.GetExtension(e.FullPath).ToLower();
            if (!GlobalSetting.ImageFormatHashSet.Contains(ext))
            {
                return;
            }

            // add to queue list for deleting
            this._queueListForDeleting.Add(e.FullPath);
        }


        private void FileWatcher_AddNewFileAction(string newFilename)
        {
            //Add the new image to the list
            GlobalSetting.ImageList.Add(newFilename);

            //Add the new image to thumbnail bar
            ImageListView.ImageListViewItem lvi = new ImageListView.ImageListViewItem(newFilename)
            {
                Tag = newFilename
            };

            thumbnailBar.Items.Add(lvi);
            thumbnailBar.Refresh();

            UpdateStatusBar(); // File count has changed - update title bar
        }


        /// <summary>
        /// The queue thread to check the files needed to be deleted.
        /// </summary>
        private void ThreadWatcherDeleteFiles()
        {
            while (true)
            {
                if (_queueListForDeleting.Count > 0)
                {
                    var filename = _queueListForDeleting[0];
                    _queueListForDeleting.RemoveAt(0);

                    DoDeleteFiles(filename);
                }
                else
                {
                    System.Threading.Thread.Sleep(200);
                }
            }
        }


        /// <summary>
        /// Proceed deleting file in memory
        /// </summary>
        /// <param name="filename"></param>
        private void DoDeleteFiles(string filename)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(DoDeleteFiles), filename);
                return;
            }

            //Get index of deleted image
            int imgIndex = GlobalSetting.ImageList.IndexOf(filename);

            if (imgIndex > -1)
            {
                //delete image list
                GlobalSetting.ImageList.Remove(imgIndex);

                //delete thumbnail list
                thumbnailBar.Items.RemoveAt(imgIndex);

                // change the viewing image to memory data mode
                if (imgIndex == GlobalSetting.CurrentIndex)
                {
                    GlobalSetting.IsImageError = true;
                    LocalSetting.IsTempMemoryData = true;

                    DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._ImageNotExist"], 1300);

                    if (_queueListForDeleting.Count == 0)
                    {
                        NextPic(0);
                    }
                }

                // If user deletes the initially loaded image, use the path instead, in case
                // of list re-load.
                if (filename == LocalSetting.InitialInputPath)
                    LocalSetting.InitialInputPath = Path.GetDirectoryName(filename);
            }
        }


        #endregion






        // Use mouse wheel to navigate, scroll, or zoom images
        private void picMain_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelActions action;
            switch (Control.ModifierKeys)
            {
                case Keys.Control:
                    action = GlobalSetting.MouseWheelCtrlAction;
                    break;
                case Keys.Shift:
                    action = GlobalSetting.MouseWheelShiftAction;
                    break;
                case Keys.Alt:
                    action = GlobalSetting.MouseWheelAltAction;
                    break;
                case Keys.None:
                default:
                    action = GlobalSetting.MouseWheelAction;
                    break;
            }
            switch (action)
            {
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
                    if (e.Delta < 0)
                    {
                        //Next pic
                        mnuMainViewNext_Click(null, null);
                    }
                    else
                    {
                        //Previous pic
                        mnuMainViewPrevious_Click(null, null);
                    }
                    break;
                case MouseWheelActions.DoNothing:
                default:
                    break;
            }

        }

        private void picMain_Zoomed(object sender, ImageBoxZoomEventArgs e)
        {
            _isManuallyZoomed = true;

            // Set new zoom ratio if Zoom Mode LockZoomRatio is enabled
            if (GlobalSetting.ZoomMode == ZoomMode.LockZoomRatio)
            {
                GlobalSetting.ZoomLockValue = e.NewZoom;
            }

            // Zoom optimization
            ZoomOptimization();

            // Update zoom info
            UpdateStatusBar();
        }

        private void picMain_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle: //Reset zoom mode
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                    break;

                case MouseButtons.XButton1: //Back
                    mnuMainViewPrevious_Click(null, null);
                    break;

                case MouseButtons.XButton2: //Next
                    mnuMainViewNext_Click(null, null);
                    break;

                case MouseButtons.Left:
                    if (GlobalSetting.IsShowNavigationButtons && !picMain.IsPanning)
                    {
                        CheckCursorPositionOnViewer(e.Location, onCursorLeftAction: () =>
                        {
                            mnuMainViewPrevious_Click(null, null);
                        }, onCursorRightAction: () =>
                        {
                            mnuMainViewNext_Click(null, null);
                        });
                    }
                    break;

                default:
                    break;
            }


        }

        private void picMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            void ToggleActualSize()
            {
                if (picMain.Zoom < 100)
                {
                    mnuMainActualSize_Click(null, null);
                }
                else
                {
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }
            }


            if (GlobalSetting.IsShowNavigationButtons)
            {
                CheckCursorPositionOnViewer(e.Location, onCursorCenterAction: () =>
                {
                    ToggleActualSize();
                });
            }
            else
            {
                ToggleActualSize();
            }
        }


        private void picMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (!picMain.IsPanning)
            {
                void SetDefaultCursor()
                {
                    if (LocalSetting.IsColorPickerToolOpening)
                    {
                        picMain.Cursor = Cursors.Cross;
                    }
                    else
                    {
                        picMain.Cursor = _isAppBusy ? Cursors.WaitCursor : Cursors.Default;
                    }
                }

                // set the Arrow cursor
                if (GlobalSetting.IsShowNavigationButtons)
                {

                    CheckCursorPositionOnViewer(e.Location, onCursorLeftAction: () =>
                    {
                        picMain.Cursor = LocalSetting.Theme.PreviousArrowCursor ?? DefaultCursor;

                    }, onCursorRightAction: () =>
                    {
                        picMain.Cursor = LocalSetting.Theme.NextArrowCursor ?? DefaultCursor;

                    }, onCursorCenterAction: () =>
                    {
                        SetDefaultCursor();
                    });

                }

                //reset the cursor
                else
                {
                    SetDefaultCursor();
                }
            }
        }

        private void sp1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // User has moved the thumbnail splitter bar. Update image size.
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }


        #endregion



        #region Toolbar Buttons Events

        private void btnNext_Click(object sender, EventArgs e)
        {
            mnuMainViewNext_Click(null, e);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            mnuMainViewPrevious_Click(null, e);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            mnuMainRefresh_Click(null, null);
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            mnuMainRotateClockwise_Click(null, e);
        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            mnuMainRotateCounterclockwise_Click(null, e);
        }

        private void btnFlipHorz_Click(object sender, EventArgs e)
        {
            mnuMainFlipHorz_Click(null, null);
        }

        private void btnFlipVert_Click(object sender, EventArgs e)
        {
            mnuMainFlipVert_Click(null, null);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            mnuMainMoveToRecycleBin_Click(null, e);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            mnuMainOpenFile_Click(null, e);
        }

        private void btnThumb_Click(object sender, EventArgs e)
        {
            mnuMainThumbnailBar_Click(null, e);
        }

        private void btnActualSize_Click(object sender, EventArgs e)
        {
            mnuMainActualSize_Click(null, e);
        }

        private void btnAutoZoom_Click(object sender, EventArgs e)
        {
            mnuMainAutoZoom_Click(null, e);
        }

        private void btnScaletoWidth_Click(object sender, EventArgs e)
        {
            mnuMainScaleToWidth_Click(null, e);
        }

        private void btnScaletoHeight_Click(object sender, EventArgs e)
        {
            mnuMainScaleToHeight_Click(null, e);
        }

        private void btnWindowAutosize_Click(object sender, EventArgs e)
        {
            mnuMainWindowAdaptImage_Click(null, e);
        }

        private void btnGoto_Click(object sender, EventArgs e)
        {
            mnuMainGoto_Click(null, e);
        }

        private void btnCheckedBackground_Click(object sender, EventArgs e)
        {
            mnuMainCheckBackground_Click(null, e);
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            mnuMainZoomIn_Click(null, e);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            mnuMainZoomOut_Click(null, e);
        }

        private void btnScaleToFit_Click(object sender, EventArgs e)
        {
            mnuMainScaleToFit_Click(null, e);
        }

        private void btnZoomLock_Click(object sender, EventArgs e)
        {
            mnuMainLockZoomRatio_Click(null, e);
        }

        private void btnSlideShow_Click(object sender, EventArgs e)
        {
            mnuMainSlideShowStart_Click(null, null);
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            mnuMainFullScreen_Click(null, e);
        }

        private void btnPrintImage_Click(object sender, EventArgs e)
        {
            mnuMainPrint_Click(null, e);
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            mnuMainSaveAs_Click(null, e);
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            mnuMain.Show(toolMain, toolMain.Width - mnuMain.Width, toolMain.Height);
        }
        #endregion



        #region Context Menu
        private async void mnuContext_Opening(object sender, CancelEventArgs e)
        {
            bool isImageError = false;

            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) || GlobalSetting.IsImageError)
                {
                    isImageError = true;
                }
            }
            catch { e.Cancel = true; return; }

            //clear current items
            mnuContext.Items.Clear();

            if (GlobalSetting.IsPlaySlideShow && !isImageError)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainSlideShowPause));
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainSlideShowExit));
                mnuContext.Items.Add(new ToolStripSeparator());//---------------
            }

            //toolbar menu
            mnuContext.Items.Add(Library.Menu.Clone(mnuMainToolbar));
            mnuContext.Items.Add(Library.Menu.Clone(mnuMainAlwaysOnTop));


            //Get Editing Assoc App info
            if (!isImageError)
            {
                if (!LocalSetting.IsTempMemoryData)
                {
                    mnuContext.Items.Add(new ToolStripSeparator());//---------------
                    mnuContext.Items.Add(Library.Menu.Clone(mnuMainChannels));
                }

                mnuContext.Items.Add(new ToolStripSeparator());//---------------

                UpdateEditingAssocAppInfoForMenu();
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainEditImage));

                //check if image can animate (GIF)
                try
                {
                    var imgData = await GlobalSetting.ImageList.GetImgAsync(GlobalSetting.CurrentIndex);

                    if (imgData.FrameCount > 1)
                    {
                        var mnu1 = Library.Menu.Clone(mnuMainExtractFrames);
                        mnu1.Text = string.Format(GlobalSetting.LangPack.Items[$"{Name}.mnuMainExtractFrames"], imgData.FrameCount);
                        mnu1.Enabled = true;

                        var mnu2 = Library.Menu.Clone(mnuMainStartStopAnimating);
                        mnu2.Enabled = true;

                        mnuContext.Items.Add(mnu1);
                        mnuContext.Items.Add(mnu2);
                    }

                }
                catch { }
            }

            if (!isImageError || LocalSetting.IsTempMemoryData)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainSetAsDesktop));

                // check if igcmdWin10.exe exists!
                if (File.Exists(GlobalSetting.StartUpDir("igcmdWin10.exe")))
                {
                    mnuContext.Items.Add(Library.Menu.Clone(mnuMainSetAsLockImage));
                }
            }


            #region Menu group: CLIPBOARD
            mnuContext.Items.Add(new ToolStripSeparator());//------------
            mnuContext.Items.Add(Library.Menu.Clone(mnuMainOpenImageData));

            if (!isImageError && !LocalSetting.IsTempMemoryData)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainClearClipboard));
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainCopy));
            }

            if (picMain.Image != null)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainCopyImageData));
            }

            if (!isImageError && !LocalSetting.IsTempMemoryData)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainCut));
            }
            #endregion


            if (!isImageError && !LocalSetting.IsTempMemoryData)
            {
                mnuContext.Items.Add(new ToolStripSeparator());//------------
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainRename));
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainMoveToRecycleBin));

                mnuContext.Items.Add(new ToolStripSeparator());//------------
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainCopyImagePath));
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainImageLocation));
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainImageProperties));
            }

        }
        #endregion



        #region Main Menu (Main functions)

        private void mnuMainOpenFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void MnuMainNewWindow_Click(object sender, EventArgs e)
        {
            if (!GlobalSetting.IsAllowMultiInstances)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}.mnuMainNewWindow._Error"], 2000);

                return;
            }

            try
            {
                var filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

                Process.Start(new ProcessStartInfo()
                {
                    FileName = Application.ExecutablePath,
                    Arguments = $"\"{filename}\"",
                });
            }
            catch { }
        }

        private void mnuMainOpenImageData_Click(object sender, EventArgs e)
        {
            //Is there a file in clipboard ?--------------------------------------------------
            if (Clipboard.ContainsFileDropList())
            {
                string[] sFile = (string[])Clipboard.GetData(DataFormats.FileDrop);

                // load file
                PrepareLoading(sFile[0]);
            }


            //Is there a image in clipboard ?-------------------------------------------------
            //CheckImageInClipboard: ;
            else if (Clipboard.ContainsImage())
            {
                LoadImageData(Clipboard.GetImage());
            }

            // Is there a filename in clipboard?-----------------------------------------------
            // CheckPathInClipboard: ;
            else if (Clipboard.ContainsText())
            {
                // try to get absolute path
                var inputPath = GlobalSetting.ToAbsolutePath(Clipboard.GetText());

                if (File.Exists(inputPath) || Directory.Exists(inputPath))
                {
                    PrepareLoading(inputPath);
                }
                // get image from Base64string 
                else
                {
                    try
                    {
                        // data:image/jpeg;base64,xxxxxxxx
                        var base64str = inputPath.Substring(inputPath.LastIndexOf(',') + 1);

                        var file_bytes = Convert.FromBase64String(base64str);
                        var file_stream = new MemoryStream(file_bytes);
                        var file_image = Image.FromStream(file_stream);

                        picMain.Image = file_image;
                        LocalSetting.IsTempMemoryData = true;
                    }
                    catch { }
                }
            }
        }

        private async void mnuMainSaveAs_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            if (filename == "")
            {
                filename = "untitled.png";
            }
            string ext = Path.GetExtension(filename).Substring(1);


            SaveFileDialog s = new SaveFileDialog
            {
                Filter = "BMP|*.bmp|EMF|*.emf|EXIF|*.exif|GIF|*.gif|ICO|*.ico|JPG|*.jpg|PNG|*.png|TIFF|*.tiff|WMF|*.wmf|Base64String (*.txt)|*.txt",
                FileName = Path.GetFileNameWithoutExtension(filename)
            };

            // Use the last-selected file extension, if available.
            if (LocalSetting.SaveAsFilterIndex != 0)
                s.FilterIndex = LocalSetting.SaveAsFilterIndex;
            else
                switch (ext.ToLower())
                {
                    case "bmp":
                        s.FilterIndex = 1;
                        break;
                    case "emf":
                        s.FilterIndex = 2;
                        break;
                    case "exif":
                        s.FilterIndex = 3;
                        break;
                    case "gif":
                        s.FilterIndex = 4;
                        break;
                    case "ico":
                        s.FilterIndex = 5;
                        break;
                    case "jpg":
                    case "jpeg":
                    case "jpe":
                        s.FilterIndex = 6;
                        break;
                    case "png":
                        s.FilterIndex = 7;
                        break;
                    case "tiff":
                        s.FilterIndex = 8;
                        break;
                    case "wmf":
                        s.FilterIndex = 9;
                        break;
                }


            if (s.ShowDialog() == DialogResult.OK)
            {
                Bitmap clonedPic = (Bitmap)picMain.Image;

                LocalSetting.SaveAsFilterIndex = s.FilterIndex;
                switch (s.FilterIndex)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 7:
                        Heart.Photo.SaveImage(clonedPic, s.FileName);
                        break;
                    case 2:
                        clonedPic.Save(s.FileName, ImageFormat.Emf);
                        break;
                    case 3:
                        clonedPic.Save(s.FileName, ImageFormat.Exif);
                        break;
                    case 5:
                        clonedPic.Save(s.FileName, ImageFormat.Icon);
                        break;
                    case 8:
                        clonedPic.Save(s.FileName, ImageFormat.Tiff);
                        break;
                    case 9:
                        clonedPic.Save(s.FileName, ImageFormat.Wmf);
                        break;
                    case 10:
                        using (MemoryStream ms = new MemoryStream())
                        {
                            try
                            {
                                clonedPic.Save(ms, ImageFormat.Png);
                                string base64string = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());

                                using (StreamWriter fs = new StreamWriter(s.FileName))
                                {
                                    await fs.WriteAsync(base64string);
                                    fs.Flush();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Sorry, ImageGlass cannot convert this image because this error: \n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        break;
                }

                // release resources
                clonedPic.Dispose();

                //display successful msg
                if (File.Exists(s.FileName))
                {
                    DisplayTextMessage(string.Format(GlobalSetting.LangPack.Items[$"{Name}._SaveImage"], s.FileName), 2000);
                }
            }

        }

        private void mnuMainRefresh_Click(object sender, EventArgs e)
        {
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainReloadImage_Click(object sender, EventArgs e)
        {
            //Reload the viewing image
            NextPic(step: 0, isKeepZoomRatio: false, isSkipCache: true);
        }

        private void MnuMainReloadImageList_Click(object sender, EventArgs e)
        {
            // update image list
            PrepareLoading(GlobalSetting.ImageList.FileNames, GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
        }

        private void mnuMainEditImage_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.IsImageError)
            {
                return;
            }

            // Viewing image filename
            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            // If viewing image is temporary memory data
            if (LocalSetting.IsTempMemoryData)
            {
                // Save to temp file
                filename = SaveTemporaryMemoryData();

                EditByDefaultApp();
            }
            else
            {
                // Get extension
                var ext = Path.GetExtension(filename).ToLower();

                // Get association App for editing
                var assoc = GlobalSetting.GetImageEditingAssociationFromList(ext);

                if (assoc != null && File.Exists(assoc.AppPath))
                {
                    // Open configured app for editing
                    Process p = new Process();
                    p.StartInfo.FileName = assoc.AppPath;

                    //Build the arguments
                    var args = assoc.AppArguments.Replace(ImageEditingAssociation.FileMacro, filename);
                    p.StartInfo.Arguments = $"{args}";

                    //show error dialog
                    p.StartInfo.ErrorDialog = true;

                    try
                    {
                        p.Start();
                    }
                    catch (Exception)
                    { }
                }
                else // Edit by default associated app
                {
                    EditByDefaultApp();
                }
            }

            void EditByDefaultApp()
            {
                Process p = new Process();
                p.StartInfo.FileName = filename;
                p.StartInfo.Verb = "edit";

                //show error dialog
                p.StartInfo.ErrorDialog = true;

                try
                {
                    p.Start();
                }
                catch (Exception)
                { }
            }
        }

        private void mnuMainViewNext_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(1);
        }

        private void mnuMainViewPrevious_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(-1);
        }

        private void mnuMainGoto_Click(object sender, EventArgs e)
        {
            int n = GlobalSetting.CurrentIndex;
            // KBR 20190302 init to current index
            string s = (n + 1).ToString();

            if (InputBox.ShowDiaLog("", GlobalSetting.LangPack.Items[$"{Name}._GotoDialogText"], "0", true, this.TopMost) == DialogResult.OK)
            {
                s = InputBox.Message;
            }

            if (int.TryParse(s, out n))
            {
                n--;
                // KBR 20190302 have out-of-range values go to beginning/end as appropriate
                if (n < 1)
                    n = 0;
                else if (n >= GlobalSetting.ImageList.Length)
                    n = GlobalSetting.ImageList.Length - 1;

                GlobalSetting.CurrentIndex = n;
                NextPic(0);
            }
        }

        private void mnuMainGotoFirst_Click(object sender, EventArgs e)
        {
            GlobalSetting.CurrentIndex = 0;
            NextPic(0);
        }

        private void mnuMainGotoLast_Click(object sender, EventArgs e)
        {
            GlobalSetting.CurrentIndex = GlobalSetting.ImageList.Length - 1;
            NextPic(0);
        }

        private void mnuMainFullScreen_Click(object sender, EventArgs e)
        {
            //enter full screen
            if (!GlobalSetting.IsFullScreen)
            {
                mnuMainFullScreen.Checked =
                    btnFullScreen.Checked =
                    GlobalSetting.IsFullScreen = true;

                FullScreenMode(enabled: true);

                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._FullScreenMessage"]
                    , 2000);
            }
            //exit full screen
            else
            {
                mnuMainFullScreen.Checked =
                    btnFullScreen.Checked =
                    GlobalSetting.IsFullScreen = false;

                FullScreenMode(enabled: false);

            }
        }

        private void mnuMainSlideShowStart_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            //not performing
            if (!GlobalSetting.IsPlaySlideShow)
            {
                picMain.BackColor = Color.Black;

                // enter full screen
                FullScreenMode(enabled: true, changeWindowState: !GlobalSetting.IsFullScreen, onlyShowViewer: true);

                //perform slideshow
                timSlideShow.Enabled = true;

                GlobalSetting.IsPlaySlideShow = true;

                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._SlideshowMessage"], 2000);
            }
            //performing
            else
            {
                mnuMainSlideShowExit_Click(null, null);
            }
        }

        private void mnuMainSlideShowPause_Click(object sender, EventArgs e)
        {
            //performing
            if (timSlideShow.Enabled)
            {
                timSlideShow.Enabled = false;

                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._SlideshowMessagePause"], 2000);
            }
            else
            {
                timSlideShow.Enabled = true;

                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._SlideshowMessageResume"], 2000);
            }
        }

        private void mnuMainSlideShowExit_Click(object sender, EventArgs e)
        {
            timSlideShow.Enabled = false;
            GlobalSetting.IsPlaySlideShow = false;

            picMain.BackColor = GlobalSetting.BackgroundColor;

            // exit full screen
            FullScreenMode(enabled: false, changeWindowState: !GlobalSetting.IsFullScreen, onlyShowViewer: true);

        }

        private void mnuMainPrint_Click(object sender, EventArgs e)
        {
            //image error
            if (picMain.Image == null)
            {
                return;
            }

            //save image to temp file
            string temFile = SaveTemporaryMemoryData();


            Process p = new Process();
            p.StartInfo.FileName = temFile;
            p.StartInfo.Verb = "print";

            //show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();
            }
            catch (Exception) { }
        }


        private async void mnuMainRotateCounterclockwise_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            if (picMain.CanAnimate)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }


            picMain.Image = await Heart.Photo.RotateImage(new Bitmap(picMain.Image), 270);

            if (!LocalSetting.IsTempMemoryData)
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }

            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private async void mnuMainRotateClockwise_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            if (picMain.CanAnimate)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }


            picMain.Image = await Heart.Photo.RotateImage(new Bitmap(picMain.Image), 90);

            if (!LocalSetting.IsTempMemoryData)
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }

            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private async void mnuMainFlipHorz_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            if (picMain.CanAnimate)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }


            picMain.Image = await Heart.Photo.Flip(new Bitmap(picMain.Image), isHorzontal: true);

            if (!LocalSetting.IsTempMemoryData)
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
        }

        private async void mnuMainFlipVert_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            if (picMain.CanAnimate)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }


            picMain.Image = await Heart.Photo.Flip(new Bitmap(picMain.Image), isHorzontal: false);

            if (!LocalSetting.IsTempMemoryData)
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
        }

        private void mnuMainZoomIn_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomIn();
        }

        private void mnuMainZoomOut_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomOut();
        }

        private void mnuMainActualSize_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ActualSize();

            if (GlobalSetting.IsCenterImage)
            {
                picMain.CenterToImage();
            }
            else
            {
                picMain.ScrollTo(0, 0, 0, 0);
            }
        }

        private void mnuMainWindowAdaptImage_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            Rectangle screen = Screen.FromControl(this).WorkingArea;
            WindowState = FormWindowState.Normal;

            //if image size is bigger than screen
            if (picMain.Image.Width >= screen.Width || picMain.Height >= screen.Height)
            {
                Width = screen.Width;
                Height = screen.Height;
            }
            else
            {
                Size = new Size(Width += picMain.Image.Width - picMain.Width,
                                Height += picMain.Image.Height - picMain.Height);

                picMain.Bounds = new Rectangle(Point.Empty, picMain.Image.Size);
            }

            //reset zoom
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }


        private void mnuMainAutoZoom_Click(object sender, EventArgs e)
        {
            GlobalSetting.ZoomMode = ZoomMode.AutoZoom;

            SelectUIZoomMode();
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainScaleToWidth_Click(object sender, EventArgs e)
        {
            GlobalSetting.ZoomMode = ZoomMode.ScaleToWidth;

            SelectUIZoomMode();
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainScaleToHeight_Click(object sender, EventArgs e)
        {
            GlobalSetting.ZoomMode = ZoomMode.ScaleToHeight;

            SelectUIZoomMode();
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainScaleToFit_Click(object sender, EventArgs e)
        {
            GlobalSetting.ZoomMode = ZoomMode.ScaleToFit;

            SelectUIZoomMode();
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainLockZoomRatio_Click(object sender, EventArgs e)
        {
            GlobalSetting.ZoomMode = ZoomMode.LockZoomRatio;

            SelectUIZoomMode();
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }


        private void mnuMainRename_Click(object sender, EventArgs e)
        {
            RenameImage();
        }

        private void mnuMainMoveToRecycleBin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = DialogResult.Yes;

            if (GlobalSetting.IsConfirmationDelete)
            {
                msg = MessageBox.Show(string.Format(GlobalSetting.LangPack.Items[$"{Name}._DeleteDialogText"], GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)), GlobalSetting.LangPack.Items[$"{Name}._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (msg == DialogResult.Yes)
            {
                string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                try
                {
                    ImageInfo.DeleteFile(filename, isMoveToRecycleBin: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainDeleteFromHardDisk_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = DialogResult.Yes;

            if (GlobalSetting.IsConfirmationDelete)
            {
                msg = MessageBox.Show(string.Format(GlobalSetting.LangPack.Items[$"{Name}._DeleteDialogText"], GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)), GlobalSetting.LangPack.Items[$"{Name}._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (msg == DialogResult.Yes)
            {
                string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                try
                {
                    ImageInfo.DeleteFile(filename, isMoveToRecycleBin: false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainExtractFrames_Click(object sender, EventArgs e)
        {
            // Shortcut keys still work even when menu is disabled!
            if (!(sender as ToolStripMenuItem).Enabled)
                return;

            if (!GlobalSetting.IsImageError)
            {
                using (FolderBrowserDialog f = new FolderBrowserDialog()
                {
                    Description = GlobalSetting.LangPack.Items[$"{Name}._ExtractFrameText"],
                    ShowNewFolderButton = true
                })
                {
                    DialogResult res = f.ShowDialog();

                    if (res == DialogResult.OK && Directory.Exists(f.SelectedPath))
                    {
                        Animation ani = new Animation();
                        ani.ExtractAllFrames(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex), f.SelectedPath);
                    }
                }
            }
        }

        // ReSharper disable once EmptyGeneralCatchClause
        private void mnuMainSetAsDesktop_Click(object sender, EventArgs e)
        {
            var isError = false;

            try
            {
                // save the current image data to temp file
                var imgFile = SaveTemporaryMemoryData();

                using (Process p = new Process())
                {
                    var args = string.Format("setwallpaper \"{0}\" {1}", imgFile, (int)DesktopWallapaper.Style.Current);

                    // Issue #326: first attempt to set wallpaper w/o privs. 
                    p.StartInfo.FileName = GlobalSetting.StartUpDir("igcmd.exe");
                    p.StartInfo.Arguments = args;
                    p.Start();

                    p.WaitForExit();


                    // If that fails due to privs error, re-attempt with admin privs.
                    if (p.ExitCode == (int)DesktopWallapaper.Result.PrivsFail)
                    {
                        p.StartInfo.FileName = GlobalSetting.StartUpDir("igtasks.exe");
                        p.StartInfo.Arguments = args;
                        p.Start();

                        p.WaitForExit();

                        // success or error
                        isError = p.ExitCode != 0;
                    }
                    else
                    {
                        // success or error
                        isError = p.ExitCode != 0;
                    }
                }
            }
            catch { isError = true; }

            // show result message
            if (isError)
            {
                var msg = GlobalSetting.LangPack.Items[$"{Name}._SetBackground_Error"];
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var msg = GlobalSetting.LangPack.Items[$"{Name}._SetBackground_Success"];
                DisplayTextMessage(msg, 2000);
            }
        }


        private async void mnuMainSetAsLockImage_Click(object sender, EventArgs e)
        {
            var isError = false;

            try
            {
                await Task.Run(() =>
                {
                    // save the current image data to temp file
                    var imgFile = SaveTemporaryMemoryData();

                    using (Process p = new Process())
                    {
                        var args = string.Format("setlockimage \"{0}\"", imgFile);

                        p.StartInfo.FileName = GlobalSetting.StartUpDir("igcmdWin10.exe");
                        p.StartInfo.Arguments = args;
                        p.EnableRaisingEvents = true;
                        p.Start();

                        p.WaitForExit();

                        // success or error
                        isError = p.ExitCode != 0;
                    }
                });
            }
            catch { isError = true; }


            // show result message
            if (isError)
            {
                var msg = GlobalSetting.LangPack.Items[$"{Name}._SetLockImage_Error"];
                MessageBox.Show(msg, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var msg = GlobalSetting.LangPack.Items[$"{Name}._SetLockImage_Success"];
                DisplayTextMessage(msg, 2000);
            }
        }


        private void mnuMainImageLocation_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                Process.Start("explorer.exe", "/select,\"" +
                    GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + "\"");
            }
        }

        private void mnuMainImageProperties_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                ImageInfo.DisplayFileProperties(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex),
                                                Handle);
            }
        }

        private void mnuMainCopy_Click(object sender, EventArgs e)
        {
            CopyMultiFiles();
        }

        private void mnuMainCut_Click(object sender, EventArgs e)
        {
            CutMultiFiles();
        }

        private void mnuMainCopyImageData_Click(object sender, EventArgs e)
        {
            if (picMain.Image != null)
            {
                Clipboard.SetImage(picMain.Image);
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._CopyImageData"], 1000);
            }
        }

        private void mnuMainCopyImagePath_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            }
            catch { }
        }

        private void mnuMainClearClipboard_Click(object sender, EventArgs e)
        {
            //clear copied files in clipboard
            if (LocalSetting.StringClipboard.Count > 0)
            {
                LocalSetting.StringClipboard = new List<string>();
                Clipboard.Clear();
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{Name}._ClearClipboard"], 1000);
            }
        }


        private void mnuMainToolbar_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowToolBar = !GlobalSetting.IsShowToolBar;
            if (GlobalSetting.IsShowToolBar)
            {
                //Hien
                toolMain.Visible = true;
            }
            else
            {
                //An
                toolMain.Visible = false;
            }
            mnuMainToolbar.Checked = GlobalSetting.IsShowToolBar;

            // Issue #554 
            if (!_isManuallyZoomed)
            {
                ApplyZoomMode(GlobalSetting.ZoomMode); // Resize image to adapt when toolbar turned off
            }

        }

        private void mnuMainThumbnailBar_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;

            sp1.Panel2Collapsed = !GlobalSetting.IsShowThumbnail;
            btnThumb.Checked = GlobalSetting.IsShowThumbnail;

            if (GlobalSetting.IsShowThumbnail)
            {
                float scaleFactor = ((float)DPIScaling.CurrentDPI) / DPIScaling.DPI_DEFAULT;

                // calculate the gap
                int gap = 0;
                double hScrollHeight = 7 * scaleFactor - 1;

                if (GlobalSetting.IsShowThumbnailScrollbar)
                {
                    hScrollHeight = SystemInformation.HorizontalScrollBarHeight;
                }
                gap = (int)((hScrollHeight * scaleFactor) + (25 / scaleFactor * 1.05));

                //show
                var tb = new ThumbnailItemInfo(GlobalSetting.ThumbnailDimension, GlobalSetting.IsThumbnailHorizontal);
                int minSize = tb.GetTotalDimension() + gap;
                //sp1.Panel2MinSize = tb.GetTotalDimension() + gap;


                int splitterDistance = Math.Abs(sp1.Height - minSize);

                if (GlobalSetting.IsThumbnailHorizontal)
                {
                    // BOTTOM
                    sp1.SplitterWidth = 1;
                    sp1.Orientation = Orientation.Horizontal;
                    sp1.SplitterDistance = splitterDistance;
                    thumbnailBar.View = ImageListView.View.Gallery;
                }
                else
                {
                    // RIGHT
                    sp1.IsSplitterFixed = false; //Allow user to resize
                    sp1.SplitterWidth = (int)Math.Ceiling(3 * scaleFactor);
                    sp1.Orientation = Orientation.Vertical;

                    // KBR 20190302 Issue #483: reset splitter width if it gets out of whack somehow
                    if ((sp1.Width - GlobalSetting.ThumbnailBarWidth) < 1)
                    {
                        GlobalSetting.ThumbnailBarWidth = Math.Min(128, sp1.Width);
                        GlobalSetting.SetConfig("ThumbnailBarWidth", GlobalSetting.ThumbnailBarWidth.ToString(GlobalSetting.NumberFormat));
                    }

                    sp1.SplitterDistance = sp1.Width - GlobalSetting.ThumbnailBarWidth;
                    thumbnailBar.View = ImageListView.View.Thumbnails;
                }
            }
            else
            {
                //Save thumbnail bar width when closing
                if (!GlobalSetting.IsThumbnailHorizontal)
                {
                    GlobalSetting.ThumbnailBarWidth = sp1.Width - sp1.SplitterDistance;
                }
                sp1.SplitterWidth = 1; // right-side splitter will 'flash' unless width reset
            }
            mnuMainThumbnailBar.Checked = GlobalSetting.IsShowThumbnail;
            SelectCurrentThumbnail();

            if (!_isManuallyZoomed)
            {
                ApplyZoomMode(GlobalSetting.ZoomMode); // Resize image to adapt when thumbbar turned off
            }
        }

        private void mnuMainCheckBackground_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowCheckerBoard = !GlobalSetting.IsShowCheckerBoard;
            btnCheckedBackground.Checked = GlobalSetting.IsShowCheckerBoard;

            if (btnCheckedBackground.Checked)
            {
                //show
                if (GlobalSetting.IsShowCheckerboardOnlyImageRegion)
                {
                    picMain.GridDisplayMode = ImageBoxGridDisplayMode.Image;
                }
                else
                {
                    picMain.GridDisplayMode = ImageBoxGridDisplayMode.Client;
                }
            }
            else
            {
                //hide
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.None;
            }

            mnuMainCheckBackground.Checked = btnCheckedBackground.Checked;
        }

        private void mnuMainAlwaysOnTop_Click(object sender, EventArgs e)
        {
            TopMost =
                mnuMainAlwaysOnTop.Checked =
                GlobalSetting.IsWindowAlwaysOnTop = !GlobalSetting.IsWindowAlwaysOnTop;
        }


        private void mnuMainColorPicker_Click(object sender, EventArgs e)
        {
            LocalSetting.IsShowColorPickerOnStartup = LocalSetting.IsColorPickerToolOpening = mnuMainColorPicker.Checked;

            //open Color Picker tool
            if (mnuMainColorPicker.Checked)
            {
                if (LocalSetting.FColorPicker.IsDisposed)
                {
                    LocalSetting.FColorPicker = new frmColorPicker();
                }
                LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.COLOR_PICKER_MENU;

                LocalSetting.FColorPicker.SetImageBox(picMain);
                LocalSetting.FColorPicker.Show(this);

                this.Activate();
            }
            //Close Color picker tool
            else
            {
                if (LocalSetting.FColorPicker != null)
                {
                    LocalSetting.FColorPicker.Close();
                }
            }
        }

        private void mnuMainSettings_Click(object sender, EventArgs e)
        {
            if (LocalSetting.FSetting.IsDisposed)
            {
                LocalSetting.FSetting = new frmSetting();
            }

            LocalSetting.ForceUpdateActions = MainFormForceUpdateAction.NONE;
            LocalSetting.FSetting.MainInstance = this;
            LocalSetting.FSetting.TopMost = this.TopMost;
            LocalSetting.FSetting.Show();
            LocalSetting.FSetting.Activate();
        }

        private void mnuMainAbout_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.TopMost = this.TopMost;
            f.ShowDialog();
        }

        private void mnuMainFirstLaunch_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = GlobalSetting.StartUpDir("igcmd.exe");
            p.StartInfo.Arguments = "firstlaunch";

            try
            {
                p.Start();
            }
            catch { }
        }

        private void mnuMainCheckForUpdate_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(Application.StartupPath, "igcmd.exe");
            p.StartInfo.Arguments = "igupdate";
            p.Start();
        }

        private void mnuMainReportIssue_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/d2phap/ImageGlass/issues");
            }
            catch { }
        }

        private void mnuMainExitApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void mnuMainStartStopAnimating_Click(object sender, EventArgs e)
        {
            if (picMain.IsAnimating)
            {
                picMain.StopAnimating();
            }
            else
            {
                picMain.StartAnimating();
            }
        }

        private async void mnuMain_Opening(object sender, CancelEventArgs e)
        {
            btnMenu.Checked = true;

            try
            {
                // Alert user if there is a new version
                if (GlobalSetting.IsNewVersionAvailable)
                {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCheckForUpdate._NewVersion"];
                    mnuMainHelp.BackColor = mnuMainCheckForUpdate.BackColor = Color.FromArgb(35, 255, 165, 2);
                }
                else
                {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = GlobalSetting.LangPack.Items[$"{Name}.mnuMainCheckForUpdate._NoUpdate"];
                }


                mnuMainExtractFrames.Enabled = false;
                mnuMainStartStopAnimating.Enabled = false;


                int frameCount = 0;
                if (GlobalSetting.CurrentIndex >= 0)
                {
                    var imgData = await GlobalSetting.ImageList.GetImgAsync(GlobalSetting.CurrentIndex);
                    frameCount = imgData.FrameCount;
                }


                mnuMainExtractFrames.Text = string.Format(GlobalSetting.LangPack.Items[$"{Name}.mnuMainExtractFrames"], frameCount);

                if (frameCount > 1)
                {
                    mnuMainExtractFrames.Enabled = true;
                    mnuMainStartStopAnimating.Enabled = true;
                }

                // check if igcmdWin10.exe exists!
                if (!File.Exists(GlobalSetting.StartUpDir("igcmdWin10.exe")))
                {
                    mnuMainSetAsLockImage.Enabled = false;
                }
                else
                {
                    mnuMainSetAsLockImage.Enabled = true;
                }

                // add hotkey to Exit menu
                mnuMainExitApplication.ShortcutKeyDisplayString = GlobalSetting.IsPressESCToQuit ? "ESC" : "Alt+F4";

                // Get association App for editing
                UpdateEditingAssocAppInfoForMenu();

            }
            catch (Exception) { }
        }

        private void mnuMain_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            btnMenu.Checked = false;
        }


        private void subMenu_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem mnuItem = sender as ToolStripMenuItem;
            if (mnuItem.HasDropDownItems == false)
            {
                return; // not a drop down item
            }

            //get position of current menu item
            var pos = new Point(mnuItem.GetCurrentParent().Left, mnuItem.GetCurrentParent().Top);

            // Current bounds of the current monitor
            Screen currentScreen = Screen.FromPoint(pos);

            // Find the width of sub-menu
            int maxWidth = 0;
            foreach (var subItem in mnuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem mnu)
                {
                    maxWidth = Math.Max(mnu.Width, maxWidth);
                }
            }
            maxWidth += 10; // Add a little wiggle room

            int farRight = pos.X + mnuMain.Width + maxWidth;
            int farLeft = pos.X - maxWidth;

            //get left and right distance to compare
            int leftGap = farLeft - currentScreen.Bounds.Left;
            int rightGap = currentScreen.Bounds.Right - farRight;


            if (leftGap >= rightGap)
            {
                mnuItem.DropDownDirection = ToolStripDropDownDirection.Left;
            }
            else
            {
                mnuItem.DropDownDirection = ToolStripDropDownDirection.Right;
            }


            mnuItem.DropDown.BackColor = LocalSetting.Theme.MenuBackgroundColor;
        }



        #endregion

        
    }
}
