/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Core;
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
using System.Collections.Concurrent;
using FileWatcherEx;
using System.Reflection;
using SevenZip;
using System.Threading;

using Timer = System.Windows.Forms.Timer;

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
            
            //Remove white line under tool strip
            toolMain.Renderer = new Theme.ToolStripRenderer();

            //Load UI Configs
            LoadConfig(isLoadUI: true, isLoadOthers: false);
            Application.DoEvents();
            
            //Update form with new DPI
            OnDpiChanged();


            // KBR 20181009 - Fix observed bug. If picMain had input focus, CTRL+/CTRL- keys would zoom *twice*.
            // This is disabled by turning off ImageBox shortcuts. Done here rather than in designer so this bugfix
            // is visible.
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
            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];

            // Drag file from DESKTOP to APP
            if (GlobalSetting.ImageList.IndexOf(filePath) == -1)
            {
                e.Effect = DragDropEffects.Move;
            }
            // Drag file from APP to DESKTOP
            else
            {
                e.Effect = DragDropEffects.Copy;
            }

        }

        private void picMain_DragDrop(object sender, DragEventArgs e)
        {
            // Drag file from DESKTOP to APP
            string[] filepaths = ((string[])e.Data.GetData(DataFormats.FileDrop));
            if (filepaths.Length > 1)
            {
                PrepareMulti(filepaths);
                return;
            }

            string filePath = filepaths[0];

            if (Path.GetExtension(filePath).ToLower() == ".lnk")
                filePath = Shortcuts.FolderFromShortcut(filePath);

            int imageIndex = GlobalSetting.ImageList.IndexOf(filePath);

            // The file is located another folder, load the entire folder
            if (imageIndex == -1)
            {
                Prepare(filePath);
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
        /// Open an image or archive
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog o = new OpenFileDialog();

            o.Filter = string.Format("{0}|{1}|{2}|{3}",
                GlobalSetting.LangPack.Items["frmMain._OpenFileDialog"],
                GlobalSetting.AllImageFormats,
                GlobalSetting.LangPack.Items["frmMain._ArchiveFormats"],
                GlobalSetting.AllArchiveFormats);

            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName))
            {
                Prepare(o.FileName);
            }
            o.Dispose();
        }

        /// <summary>
        /// Prepare to load image
        /// </summary>
        /// <param name="path">Initial file/folder to load with</param>
        /// <param name="targetImgFile">When re-loading the image list, this is the desired image to remain visible</param>
        public void Prepare(string path, string targetImgFile = null)
        {
            if (File.Exists(path) == false && Directory.Exists(path) == false)
                return;

            //Reset current index
            GlobalSetting.CurrentIndex = 0;
            string filePath = "";
            string dirPath = "";

            // Reset search from subfolders
            LocalSetting.LoadFromSubfolders = GlobalSetting.IsRecursiveLoading;

            CancelAndCleanupArchiveExtract();

            //Check path is file or directory?
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLower();
                if (GlobalSetting.ArchiveFormatHashSet.Contains(ext))
                {
                    LoadFromArchive(path);
                    return;
                }
                if (ext == ".lnk")
                    dirPath = Shortcuts.FolderFromShortcut(path);
                else
                    dirPath = Path.GetDirectoryName(path);

                filePath = path;
            }
            else if (Directory.Exists(path))
            {
                dirPath = path;
            }

            // Issue #415: If the folder name ends in ALT+255 (alternate space), DirectoryInfo strips it.
            // By ensuring a terminating slash, the problem disappears. By doing that *here*,
            // the uses of DirectoryInfo in DirectoryFinder and FileWatcherEx are fixed as well.
            // https://stackoverflow.com/questions/5368054/getdirectories-fails-to-enumerate-subfolders-of-a-folder-with-255-name
            if (!dirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                dirPath += Path.DirectorySeparatorChar;


            LocalSetting.InitialInputImageFilename = string.IsNullOrEmpty(filePath) ? dirPath : filePath;
            
            
            // Not loading from an archive
            _FolderToDelete = null;
            LocalSetting.FilesFromArchive = false;


            //Get supported image extensions from directory
            var _imageFilenameList = LoadImageFilesFromDirectory(dirPath);

            LoadImages(_imageFilenameList, targetImgFile ?? filePath);
            
            WatchPath(dirPath);
        }
        

        /// <summary>
        /// Load the images.
        /// </summary>
        /// <param name="_imageFilenameList"></param>
        /// <param name="filePath"></param>
        private void LoadImages(List<string> _imageFilenameList, string filePath)
        { 
            //Dispose all garbage
            GlobalSetting.ImageList.Dispose();

            //Set filename to image list
            GlobalSetting.ImageList = new ImgMan(_imageFilenameList.ToArray());
            //Track image loading progress
            GlobalSetting.ImageList.OnFinishLoadingImage += ImageList_OnFinishLoadingImage;

            //Find the index of current image
            if (filePath.Length > 0)
            {
                GlobalSetting.CurrentIndex = GlobalSetting.ImageList.IndexOf(filePath);

                // KBR 20181009 Changing "include subfolder" setting could lose the "current" image.
                // Prefer not to report said image is "corrupt", merely reset the index in that case.
                // 1. Setting: "include subfolders: ON". Open image in folder with images in subfolders.
                // 2. Move to an image in a subfolder.
                // 3. Change setting "include subfolders: OFF".
                // Issue: the image in the subfolder is attempted to be shown, declared as corrupt/missing.
                if (GlobalSetting.CurrentIndex == -1 && !GlobalSetting.ImageList.HasFolder(filePath))
                    GlobalSetting.CurrentIndex = 0;
            }
            else
            {
                GlobalSetting.CurrentIndex = 0;
            }

            //Load thumnbnail
            LoadThumbnails();

            //Cannot find the index
            if (GlobalSetting.CurrentIndex == -1)
            {
                //Mark as Image Error
                GlobalSetting.IsImageError = true;
                this.Text = $"ImageGlass - {Path.GetFileName(filePath)} - {ImageInfo.GetFileSize(filePath)}";

                picMain.Text = GlobalSetting.LangPack.Items["frmMain.picMain._ErrorText"];
                picMain.Image = null;

                //Exit function
                return;
            }

            //Start loading image
            NextPic(0);
        }


        /// <summary>
        /// Watch a folder for changes.
        /// </summary>
        /// <param name="dirPath">The path to the folder to watch.</param>
        private void WatchPath(string dirPath)
        {
            //Watch all changes of current path
            this._fileWatcher.Stop();
            this._fileWatcher = new FileWatcherEx.FileWatcherEx()
            {
                FolderPath = dirPath,
                IncludeSubdirectories = LocalSetting.LoadFromSubfolders, // GlobalSetting.IsRecursiveLoading,

                // auto Invoke the form if required, no need to individually invoke in each event
                SynchronizingObject = this
            };

            this._fileWatcher.OnCreated += FileWatcher_OnCreated;
            this._fileWatcher.OnDeleted += FileWatcher_OnDeleted;
            this._fileWatcher.OnChanged += FileWatcher_OnChanged;
            this._fileWatcher.OnRenamed += FileWatcher_OnRenamed;

            this._fileWatcher.Start();
        }


        /// <summary>
        /// Prepare to load images. User has dragged multiple files / paths onto IG.
        /// </summary>
        /// <param name="paths"></param>
        private void PrepareMulti(string[] paths)
        {
            // TODO re-loading of the image list/folder currently does NOT invoke this code!
            // TODO don't have any 'memory' of initial paths; re-load of image list/folder will be confused

            HashSet<string> pathsLoaded = new HashSet<string>(); // track paths loaded to prevent duplicates
            List<string> allFilesToLoad = new List<string>();
            bool firstPath = true;
            foreach (var apath in paths)
            {
                string dirPath = "";
                if (File.Exists(apath))
                {
                    var ext = Path.GetExtension(apath).ToLower();
                    // If first path is an archive, load that one, and only that one
                    if (GlobalSetting.ArchiveFormatHashSet.Contains(ext) && firstPath)
                    {
                        LoadFromArchive(apath);
                        return;
                    }
                    if (ext == ".lnk")
                        dirPath = Shortcuts.FolderFromShortcut(apath);
                    else
                        dirPath = Path.GetDirectoryName(apath);
                }
                else if (Directory.Exists(apath))
                {
                    dirPath = apath;
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
                }

                // KBR 20181004 Fix observed bug: dropping multiple files from the same path
                // would load ALL files in said path multiple times! Prevent loading the same
                // path more than once.
                if (pathsLoaded.Contains(dirPath))
                    continue;
                pathsLoaded.Add(dirPath);

                var imageFilenameList = LoadImageFilesFromDirectory(dirPath);
                allFilesToLoad.AddRange(imageFilenameList);
            }

            LoadImages(allFilesToLoad, "");
        }


        #region Archive (ZIP) support

        // Remember the path created to extract the archive into; cleanup on next open, app exit
        private string _FolderToDelete;


        // This provides the means to cancel the background archive extraction.
        // Cancellation is necessary if the user opens/drops a new image before
        // the archive extraction is complete.
        private CancellationTokenSource _unzipCancelSource;

        private Task _unzipTask;

        /// <summary>
        /// Load all images from within an archive file.
        /// </summary>
        /// <param name="zippath">path to an archive file</param>
        private void LoadFromArchive(string zippath)
        {

            try
            {
                // Need to invoke 32 or 64 bit DLL as required for running OS (_not_ the build target!)
                if (Environment.Is64BitOperatingSystem)
                    SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z-64.dll"));
                else
                    SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7z-32.dll"));
            }
            catch
            {
                OnError("ArchiveSupportMissing");
                return;
            }

            string outpath; // location to extract to
            var filesToExtract = new List<ArchiveFileInfo>();

            // 1. Open the archive [if fail, done]
            using (SevenZipExtractor extr = new SevenZipExtractor(zippath))
            {

                IReadOnlyCollection<ArchiveFileInfo> zipentries;
                // 2. Get the list of archive entries [if fail, done]
                try
                {
                    zipentries = extr.ArchiveFileData;
                    if (zipentries.Count == 0)
                    {
                        OnError("ArchiveEmptyBad");
                        return;
                    }
                }
                catch
                {
                    OnError("ArchiveEmptyBad"); // extractor throws on empty file
                    return;
                }

                // 3. Scan for image files [if none, done]
                foreach (var entry in zipentries)
                {
                    if (entry.IsDirectory)
                        continue;
                    var extension = Path.GetExtension(entry.FileName).ToLower();
                    if (GlobalSetting.ImageFormatHashSet.Contains(extension))
                        filesToExtract.Add(entry);
                }

                if (filesToExtract.Count < 1)
                {
                    OnError("ArchiveEmptyBad");
                    return;
                }

                // 4. Create a folder in the user's temp directory [if fail, done]
                outpath = Path.Combine(Path.GetTempPath(), "IG_" + Path.GetFileName(zippath));
                try
                {
                    Directory.CreateDirectory(outpath);
                }
                catch
                {
                    OnError("ArchiveExtractFail");
                    return;
                }

                // Remember the created path and cleanup on next open, app exit
                _FolderToDelete = outpath;

                // 5. Extract the first image file with path to the created folder. If fail, don't do further steps.
                var file0 = filesToExtract[0].FileName;
                var path0 = Path.Combine(outpath, file0);
                var outfold = Path.GetDirectoryName(path0);
                try
                {
                    Directory.CreateDirectory(outfold);
                    using (FileStream fs = File.OpenWrite(path0))
                        extr.ExtractFile(filesToExtract[0].Index, fs);
                }
                catch
                {
                    OnError("ArchiveExtractFail");
                    return;
                }

            }

            // NOTE: Done with original extractor now. The background extract needs its own.


            // 6. Force "load from subfolders" for the image list
            LocalSetting.LoadFromSubfolders = true;

            // 7. Point FileWatcherEx at the created folder
            WatchPath(outpath);

            // 8. Initialize the image list
            var filepath = Path.Combine(outpath, filesToExtract[0].FileName); // TODO remove the first entry to not extract it twice
            LoadImages(new List<string> { filepath }, filepath);

            LocalSetting.FilesFromArchive = true; // Prevent attempts to modify images from an archive
            LocalSetting.ArchiveFilePath = zippath; // Display original archive path on title bar

            _unzipCancelSource = new CancellationTokenSource(); // Need a new one for each extract

            // 9. Extract image files (with paths) to the created folder ASYNCHRONOUSLY
            // The cancellation token allows the extract to be cancelled if interrupted by user
            // (e.g. dropping a different file onto IG).
            var token = _unzipCancelSource.Token;
            _unzipTask = Task.Run(() =>
            {
                var capturedToken = token;
                ExtractZipFiles(zippath, filesToExtract, outpath, capturedToken);
            }, token);

            void OnError(string msgEnd)
            {
                GlobalSetting.ImageList.Dispose();  // possibly draconian but previous images must be wiped
                LoadThumbnails();                   // clear thumbbar
                GlobalSetting.IsImageError = true;
                string msg = "frmMain.picMain." + msgEnd;
                this.Text = $"ImageGlass - {Path.GetFileName(zippath)}";
                picMain.Text = GlobalSetting.LangPack.Items[msg];
                picMain.Image = null;
            }
        }


        /// <summary>
        /// Extract image files from an archive. This is intended to be executed as a background task
        /// from the "load from archive" function.
        /// 
        /// By performing as a background task, the user can see the first image(s) from within the
        /// archive without having to wait for the entire archive to be extracted. The FileWatch
        /// mechanism is used to update the GUI when files have been extracted.
        /// </summary>
        /// <param name="inpath">path to archive</param>
        /// <param name="filesToExtract">the files to extract from the archive</param>
        /// <param name="outbase">the path to the base folder we're extracting to</param>
        /// <param name="token">check this for cancellation</param>
        private void ExtractZipFiles(string inpath, List<ArchiveFileInfo> filesToExtract, string outbase,
                                    CancellationToken token)
        {
            using (var extr = new SevenZipExtractor(inpath))
            {
                foreach (var entry in filesToExtract)
                {
                    if (token.IsCancellationRequested) // cancellation happened
                        return;

                    var outpath = Path.Combine(outbase, entry.FileName);
                    var outfold = Path.GetDirectoryName(outpath); // file might be in a sub-folder

                    try
                    {
                        Directory.CreateDirectory(outfold); // each file could possibly need a sub-folder

                        if (token.IsCancellationRequested) // cancellation happened
                            return;

                        using (FileStream fs = File.OpenWrite(outpath))
                            extr.ExtractFile(entry.FileName, fs);
                    }
                    catch
                    {
                        // TODO KBR should something happen here?
                        // Note no message: hoping we've successfully extracted one image
                    }

                }
            }
        }


        /// <summary>
        /// Archive extraction may still be going, and/or left a temp folder. Cancel and
        /// cleanup when necessary.
        /// </summary>
        private void CancelAndCleanupArchiveExtract()
        {
            // We're about to cancel the unpack: stop the watcher from seeing any more files
            // kbr 20181108 - the Stop() isn't fast enough/sufficient to prevent the
            // filewatcher from picking up any remnant files created by the archive
            // unpacker.
            this._fileWatcher.OnCreated -= FileWatcher_OnCreated;
            this._fileWatcher.OnDeleted -= FileWatcher_OnDeleted;
            this._fileWatcher.OnChanged -= FileWatcher_OnChanged;
            this._fileWatcher.OnRenamed -= FileWatcher_OnRenamed;
            _fileWatcher.Stop();

            // Cancel any existing archive extraction
            if (LocalSetting.FilesFromArchive)
            {
                // checking for null as the archive might have been invalid, and
                // not actually started the unarchive task
                if (_unzipCancelSource != null) 
                    _unzipCancelSource.Cancel();
                if (_unzipTask != null)
                    _unzipTask.Wait(); // wait for the unarchive background to stop
            }

            Thread.Sleep(10);

            // Clean up any previously opened archive
            Task.Run(() => DeleteUnzipFolder(_FolderToDelete));

            if (_unzipCancelSource != null)
            {
                _unzipCancelSource.Dispose();
                _unzipCancelSource = null;
            }
            if (_unzipTask != null)
            {
                _unzipTask.Dispose();
                _unzipTask = null;
            }
        }


        /// <summary>
        /// Delete the folder and contents we created to extract an archive into.
        /// </summary>
        private void DeleteUnzipFolder(string folderToDelete)
        {
            if (folderToDelete != null && Directory.Exists(folderToDelete))
            {
                Directory.Delete(folderToDelete, true);
            }
        }

        #endregion


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
        private List<string> LoadImageFilesFromDirectory(string path)
        {
            //Load image order from config
            GlobalSetting.LoadImageOrderConfig();

            //Get files from dir
            var fileList = DirectoryFinder.FindFiles(path,
                LocalSetting.LoadFromSubfolders, //GlobalSetting.IsRecursiveLoading,
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
            

            var list = SortImageList(fileList);

            return list;
        }


        private List<string> SortImageList(ConcurrentBag<string> fileList)
        {
            var list = new List<string>();

            //Sort image file
            if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.Name)
            {
                var arr = fileList.ToArray();
                Array.Sort(arr, new WindowsNaturalSort());
                list.AddRange(arr);

                //list.AddRange(FileLogicalComparer.Sort(dsFile.ToArray()));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.Length)
            {
                list.AddRange(fileList
                    .OrderBy(f => new FileInfo(f).Length));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.CreationTime)
            {
                list.AddRange(fileList
                    .OrderBy(f => new FileInfo(f).CreationTimeUtc));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.Extension)
            {
                list.AddRange(fileList
                    .OrderBy(f => new FileInfo(f).Extension));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.LastAccessTime)
            {
                list.AddRange(fileList
                    .OrderBy(f => new FileInfo(f).LastAccessTime));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.LastWriteTime)
            {
                list.AddRange(fileList
                    .OrderBy(f => new FileInfo(f).LastWriteTime));
            }
            else if (GlobalSetting.ImageLoadingOrder == ImageOrderBy.Random)
            {
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
        private void NextPic(int step)
        {
            NextPic(step, false);
        }

        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        /// <param name="configs">Configuration for the next load</param>
        /// <param name="isSkipCache"></param>
        private void NextPic(int step, bool isKeepZoomRatio, bool isSkipCache = false)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int, bool, bool>(NextPic), step, isKeepZoomRatio, isSkipCache);

                return;
            }

            if (picMain.IsAnimating)
            {
                picMain.StopAnimating();
            }

            // Save previous image if it was modified, except if from archive file
            if (File.Exists(LocalSetting.ImageModifiedPath) && GlobalSetting.IsSaveAfterRotating && !LocalSetting.FilesFromArchive)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SaveChanges"], 2000);

                Application.DoEvents();
                ImageSaveChange();

                //remove the old image data from cache
                GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);

                //update thumbnail
                thumbnailBar.Items[GlobalSetting.CurrentIndex].Update();
            }
            
            Application.DoEvents();

            picMain.Text = "";
            LocalSetting.IsTempMemoryData = false;

            if (GlobalSetting.ImageList.Length < 1)
            {
                Text = $"ImageGlass";

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
                    DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._LastItemOfList"], 1000);
                    return;
                }

                //Reach the first item of list
                if (tempIndex < 0)
                {
                    DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._FirstItemOfList"], 1000);
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


            //The image data will load
            Image im = null;

            try
            {
                //Read image data
                im = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex, isSkipCache);

                GlobalSetting.IsImageError = GlobalSetting.ImageList.IsErrorImage;

                //Show image
                picMain.Image = im;


                //Reset the zoom mode if isKeepZoomRatio = FALSE
                if (!isKeepZoomRatio)
                {
                    //reset zoom mode
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }

                //Run in another thread
                Parallel.Invoke(() =>
                {
                    //Release unused images
                    if (GlobalSetting.CurrentIndex - 2 >= 0)
                    {
                        GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex - 2);
                    }
                    if (!GlobalSetting.IsImageBoosterBack && GlobalSetting.CurrentIndex - 1 >= 0)
                    {
                        GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex - 1);
                    }
                });

            }
            catch
            {
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";

                Application.DoEvents();
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
                }
            }


            if (GlobalSetting.IsImageError)
            {
                picMain.Text = GlobalSetting.LangPack.Items["frmMain.picMain._ErrorText"];
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";
            }

            _isDraggingImage = false;

            //Select thumbnail item
            SelectCurrentThumbnail();

            //Collect system garbage
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
                var imgData = GlobalSetting.LangPack.Items["frmMain._ImageData"];
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

                indexTotal = $"{(GlobalSetting.CurrentIndex + 1)}/{GlobalSetting.ImageList.Length} {GlobalSetting.LangPack.Items["frmMain._Text"]}";

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
                    if (LocalSetting.FilesFromArchive)
                    {
                        filename = Ellipsis(LocalSetting.ArchiveFilePath);
                        filename += " : " + Path.GetFileName(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                    }
                    else
                    {
                        filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                        filename = Ellipsis(filename);
                    }
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

            //auto ellipsis the filename
            //the minimum text to show is Drive letter + basename.
            //ex: C:\...\example.jpg
            string Ellipsis(string fname)
            {
                var basename = Path.GetFileName(fname);

                var charWidth = this.CreateGraphics().MeasureString("A", this.Font).Width;
                var textMaxLength = (this.Width - DPIScaling.TransformNumber(400)) / charWidth;

                var maxLength = (int)Math.Max(basename.Length + 8, textMaxLength);

                return Helper.ShortenPath(fname, maxLength);
            }
        }
        #endregion



        #region Key event

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
            

            //Clear clipboard----------------------------------------------------------------
            #region CTRL + `
            if (e.KeyValue == 192 && e.Control && !e.Shift && !e.Alt)//CTRL + `
            {
                mnuMainClearClipboard_Click(null, null);
                return;
            }
            #endregion


            //Zoom + ------------------------------------------------------------------------
            #region Ctrl + = / = / + (numPad)
            if (((e.KeyValue == 187 && e.Control) || (e.KeyValue == 107 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + =
            {
                btnZoomIn_Click(null, null);
                return;
            }
            #endregion


            //Zoom - ------------------------------------------------------------------------
            #region Ctrl + - / - / - (numPad)
            if (((e.KeyValue == 189 && e.Control) || (e.KeyValue == 109 && !e.Control)) && !e.Shift && !e.Alt)// Ctrl + -
            {
                btnZoomOut_Click(null, null);
                return;
            }
            #endregion


            //Zoom to fit--------------------------------------------------------------------
            #region CTRL + `
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
            

            //Full screen--------------------------------------------------------------------
            #region ALT + ENTER
            if (e.Alt && e.KeyCode == Keys.Enter && !e.Control && !e.Shift)//Alt + Enter
            {
                btnFullScreen.PerformClick();
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
                //exit full screen
                else if (GlobalSetting.IsFullScreen)
                {
                    btnFullScreen.PerformClick();
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
            

            //Previous Image----------------------------------------------------------------
            #region LEFT ARROW / PAGE UP
            if (!_isWindowsKeyPressed && (e.KeyValue == 33 || e.KeyValue == 37) &&
                !e.Control && !e.Shift && !e.Alt)//Left arrow / PageUp
            {
                NextPic(-1);
                return;
            }
            #endregion


            //Next Image---------------------------------------------------------------------
            #region RIGHT ARROW / PAGE DOWN
            if (!_isWindowsKeyPressed && (e.KeyValue == 34 || e.KeyValue == 39) &&
                !e.Control && !e.Shift && !e.Alt)//Right arrow / Pagedown
            {
                NextPic(1);
                return;
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


            //Start / stop slideshow---------------------------------------------------------
            #region SPACE
            if (GlobalSetting.IsPlaySlideShow && e.KeyCode == Keys.Space && !e.Control && !e.Shift && !e.Alt)//SPACE
            {
                mnuMainSlideShowPause_Click(null, null);
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

            mnuMainEditImage.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"], appName);
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
                if (GlobalSetting.IsImageError || 
                    !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) ||
                    LocalSetting.FilesFromArchive // don't allow rename if archive
                    )
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
            string oldName  = Path.GetFileName(filepath);
            string ext = Path.GetExtension(filepath);
            string newName = Path.GetFileNameWithoutExtension(filepath);

            //Show input box
            string str = null;            

            if (InputBox.ShowDiaLog(GlobalSetting.LangPack.Items["frmMain._RenameDialogText"], GlobalSetting.LangPack.Items["frmMain._RenameDialog"], newName, false) == DialogResult.OK)
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            Timer tmsg = new Timer();
            tmsg.Enabled = false;
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


        private void tmsg_Tick(object sender, EventArgs e)
        {
            Timer tmsg = (Timer)sender;
            tmsg.Stop();

            picMain.TextBackColor = Color.Transparent;
            picMain.Font = Font;
            picMain.ForeColor = Color.Black;
            picMain.Text = string.Empty;
        }


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
                string.Format(GlobalSetting.LangPack.Items["frmMain._CopyFileText"],
                LocalSetting.StringClipboard.Count), 1000);
        }


        private void CutMultiFiles()
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
            

            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            var fileDropList = new StringCollection();
            fileDropList.AddRange(LocalSetting.StringClipboard.ToArray());

            DataObject data = new DataObject();
            data.SetFileDropList(fileDropList);
            data.SetData("Preferred DropEffect", dropEffect);
            

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);

            DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items["frmMain._CutFileText"],
                LocalSetting.StringClipboard.Count), 1000);
        }


        /// <summary>
        /// Save all change of image
        /// </summary>
        private void ImageSaveChange()
        {
            try
            {
                DateTime lastWriteTime = File.GetLastWriteTime(LocalSetting.ImageModifiedPath);
                Interpreter.SaveImage(picMain.Image, LocalSetting.ImageModifiedPath);
                // Issue #307: option to preserve the modified date/time
                if (GlobalSetting.PreserveModifiedDate)
                    File.SetLastWriteTime(LocalSetting.ImageModifiedPath, lastWriteTime);
            }
            catch (Exception ex)
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
            toolMain.Height = DPIScaling.TransformNumber((int) Constants.TOOLBAR_HEIGHT);

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

            //Tool bar separators
            foreach (var item in toolMain.Items.OfType<ToolStripSeparator>())
            {
                item.Size = new Size(5, newToolBarItemHeight);
            }

            //Update toolbar icon size
            var themeConfigFile = GlobalSetting.GetConfig("Theme", "default");
            if (!File.Exists(themeConfigFile))
            {
                themeConfigFile = Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml");
            }

            Theme.Theme t = new Theme.Theme(themeConfigFile);
            LoadToolbarIcons(t);

            #endregion

            #region change size of menu items
            int newMenuIconHeight = DPIScaling.TransformNumber((int)Constants.MENU_ICON_HEIGHT);

            mnuMainAbout.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            mnuMainViewNext.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            mnuMainSlideShowStart.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            mnuMainRotateCounterclockwise.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);

            mnuMainClearClipboard.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            mnuMainToolbar.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);
            mnuMainColorPicker.Image = new Bitmap(newMenuIconHeight, newMenuIconHeight);

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
        /// Save current loaded image to file and print it
        /// </summary>
        private string SaveTemporaryMemoryData()
        {
            if (!Directory.Exists(GlobalSetting.TempDir))
            {
                Directory.CreateDirectory(GlobalSetting.TempDir);
            }

            string filename = Path.Combine(GlobalSetting.TempDir, "temp_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".png");

            picMain.Image.Save(filename, ImageFormat.Png);

            return filename;
        }

        #endregion



        #region Configurations

        /// <summary>
        /// Apply ImageGlass theme
        /// </summary>
        /// <param name="themeConfigPath">config.xml path. By default, load default theme</param>
        private Theme.Theme ApplyTheme(string @themeConfigPath = "default")
        {
            if (File.Exists(themeConfigPath))
            {
                GlobalSetting.SetConfig("Theme", themeConfigPath);
            }


            Theme.Theme th = new Theme.Theme(themeConfigPath);
            LoadTheme(th);

            return th;

            void LoadTheme(Theme.Theme t)
            {
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
        private void LoadConfig(bool @isLoadUI = false, bool @isLoadOthers = true)
        {
            string configValue = string.Empty;

            

            #region UI SETTINGS
            if (isLoadUI)
            {

                #region Load theme
                thumbnailBar.SetRenderer(new ImageListView.ImageListViewRenderers.ThemeRenderer()); //ThumbnailBar Renderer must be done BEFORE loading theme            
                LocalSetting.Theme = ApplyTheme(GlobalSetting.GetConfig("Theme", "default"));
                Application.DoEvents();
                #endregion


                #region Show checkerboard
                GlobalSetting.IsShowCheckerBoard = bool.Parse(GlobalSetting.GetConfig("IsShowCheckedBackground", "False").ToString());
                GlobalSetting.IsShowCheckerBoard = !GlobalSetting.IsShowCheckerBoard;
                mnuMainCheckBackground_Click(null, EventArgs.Empty);
                #endregion


                #region Load background
                var bgValue = GlobalSetting.GetConfig("BackgroundColor", LocalSetting.Theme.BackgroundColor.ToArgb().ToString(GlobalSetting.NumberFormat));

                GlobalSetting.BackgroundColor = Color.FromArgb(int.Parse(bgValue, GlobalSetting.NumberFormat));
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


                #region Load Toolbar button centering state
                GlobalSetting.IsCenterToolbar = bool.Parse(GlobalSetting.GetConfig("IsCenterToolbar", "False"));
                // NOTE: no action necessary to force update, is performed via Form OnSize
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
                

                #region Load language pack
                configValue = GlobalSetting.GetConfig("Language", "English");
                if (File.Exists(configValue))
                {
                    GlobalSetting.LangPack = new Language(configValue);
                }

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

                // All the Archive File extensions settings
                GlobalSetting.BuildArchiveExtensions();

                #endregion


                #region Slideshow Interval
                int i = int.Parse(GlobalSetting.GetConfig("SlideShowInterval", "5"));

                if (!(0 < i && i < 61)) i = 5;//time limit [1; 60] seconds
                GlobalSetting.SlideShowInterval = i;
                timSlideShow.Interval = 1000 * GlobalSetting.SlideShowInterval;
                #endregion


                #region Recursive loading
                GlobalSetting.IsRecursiveLoading = bool.Parse(GlobalSetting.GetConfig("IsRecursiveLoading", "False"));
                #endregion


                #region Show hidden images
                GlobalSetting.IsShowingHiddenImages = bool.Parse(GlobalSetting.GetConfig("IsShowingHiddenImages", "False"));
                #endregion


                //Load image order config
                GlobalSetting.ImageLoadingOrder = GlobalSetting.LoadImageOrderConfig();

                //Load state of Image Booster
                GlobalSetting.IsImageBoosterBack = bool.Parse(GlobalSetting.GetConfig("IsImageBoosterBack", "True"));
                

                //Load IsDisplayBasenameOfImage value
                GlobalSetting.IsDisplayBasenameOfImage = bool.Parse(GlobalSetting.GetConfig("IsDisplayBasenameOfImage", "False"));
                

                #region Load Zoom Mode
                GlobalSetting.ZoomMode = (ZoomMode) Enum.Parse(typeof(ZoomMode), GlobalSetting.GetConfig("ZoomMode", "0"));


                // Load and Active Zoom Mode
                SelectUIZoomMode();
                

                // Load Zoom Lock Value
                int zoomLock = int.Parse(GlobalSetting.GetConfig("ZoomLockValue", "-1"), GlobalSetting.NumberFormat);
                GlobalSetting.ZoomLockValue = zoomLock > 0 ? zoomLock : 100;

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
                TopMost = mnuMainAlwaysOnTop.Checked = GlobalSetting.IsWindowAlwaysOnTop;
                #endregion


                #region Get welcome screen
                GlobalSetting.IsShowWelcome = bool.Parse(GlobalSetting.GetConfig("IsShowWelcome", "True"));
                if (GlobalSetting.IsShowWelcome)
                {
                    //Do not show welcome image if params exist.
                    if (Environment.GetCommandLineArgs().Count() < 2)
                    {
                        Prepare(Path.Combine(GlobalSetting.StartUpDir, "default.jpg"));
                    }
                }
                #endregion


                //load other configs in another thread
                Parallel.Invoke(() =>
                {
                    //Load IsLoopBackViewer
                    GlobalSetting.IsLoopBackViewer = bool.Parse(GlobalSetting.GetConfig("IsLoopBackViewer", "True"));

                    //Load IsLoopBackSlideShow
                    GlobalSetting.IsLoopBackSlideShow = bool.Parse(GlobalSetting.GetConfig("IsLoopBackSlideShow", "True"));

                    //Load IsPressESCToQuit
                    GlobalSetting.IsPressESCToQuit = bool.Parse(GlobalSetting.GetConfig("IsPressESCToQuit", "True"));

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

                        int mouseWheel;
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
                    GlobalSetting.IsConfirmationDelete = bool.Parse(GlobalSetting.GetConfig("IsConfirmationDelete", "False"));


                    //Get IsSaveAfterRotating value
                    GlobalSetting.IsSaveAfterRotating = bool.Parse(GlobalSetting.GetConfig("IsSaveAfterRotating", "False"));

                    // Fetch PreserveModifiedDate
                    GlobalSetting.PreserveModifiedDate = bool.Parse(GlobalSetting.GetConfig("PreserveModifiedDate", "False"));

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
                    GlobalSetting.IsNewVersionAvailable = bool.Parse(GlobalSetting.GetConfig("IsNewVersionAvailable", "False"));


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
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig($"{Name}.WindowsBound", GlobalSetting.RectToString(this.Bounds));
            }

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig($"{Name}.WindowsState", WindowState.ToString());

            //Checked background
            GlobalSetting.SetConfig("IsShowCheckedBackground", GlobalSetting.IsShowCheckerBoard.ToString());

            //Tool bar state
            GlobalSetting.SetConfig("IsShowToolBar", GlobalSetting.IsShowToolBar.ToString());
            //GlobalSetting.SetConfig("IsShowToolBarBottom", GlobalSetting.IsShowToolBarBottom.ToString());
            GlobalSetting.SetConfig("IsShowThumbnailScroll", GlobalSetting.IsShowThumbnailScrollbar.ToString());

            //Window always on top
            GlobalSetting.SetConfig("IsWindowAlwaysOnTop", GlobalSetting.IsWindowAlwaysOnTop.ToString());

            //Zoom Mode
            GlobalSetting.SetConfig("ZoomMode", GlobalSetting.ZoomMode.ToString());
            
            //Lock zoom ratio
            GlobalSetting.SetConfig("ZoomLockValue", (GlobalSetting.ZoomMode == ZoomMode.LockZoomRatio) ? GlobalSetting.ZoomLockValue.ToString(GlobalSetting.NumberFormat) : "-1");

            //Thumbnail panel
            GlobalSetting.SetConfig("IsShowThumbnail", GlobalSetting.IsShowThumbnail.ToString());
            
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
                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SaveChanges"], 1000);

                Application.DoEvents();
                ImageSaveChange();
            }

            //Save IsShowColorPickerOnStartup
            GlobalSetting.SetConfig("IsShowColorPickerOnStartup", LocalSetting.IsShowColorPickerOnStartup.ToString());

            //Save toolbar buttons
            GlobalSetting.SetConfig("ToolbarButtons", GlobalSetting.ToolbarButtons); // KBR

            // Save centering of toolbar buttons
            GlobalSetting.SetConfig("IsCenterToolbar", GlobalSetting.IsCenterToolbar.ToString()); // KBR
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
                    else
                    {
                        //reset picture position
                        picMain.ScrollTo(0, 0, 0, 0);
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
            System.Threading.Thread thDeleteWorker = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadWatcherDeleteFiles));
            thDeleteWorker.Priority = System.Threading.ThreadPriority.BelowNormal;
            thDeleteWorker.IsBackground = true;
            thDeleteWorker.Start();
        }

        public void LoadFromParams(string[] args)
        {
            //Load image from param
            if (args.Length >= 2)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    //only read the path, exclude configs parameter which starts with "--"
                    if(!args[i].StartsWith("--"))
                    {
                        string filename = args[i];

                        if (File.Exists(filename))
                        {
                            FileInfo f = new FileInfo(filename);
                            Prepare(f.FullName);
                        }
                        else if (Directory.Exists(filename))
                        {
                            // Issue #415: If the folder name ends in ALT+255 (), DirectoryInfo strips it.
                            // By ensuring a terminating slash, the problem disappears.
                            // https://stackoverflow.com/questions/5368054/getdirectories-fails-to-enumerate-subfolders-of-a-folder-with-255-name
                            if (!filename.EndsWith(Path.DirectorySeparatorChar.ToString()))
                                filename += Path.DirectorySeparatorChar;

                            DirectoryInfo d = new DirectoryInfo(filename);
                            Prepare(d.FullName);
                        }

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
                if (Directory.Exists(GlobalSetting.TempDir))
                {
                    Directory.Delete(GlobalSetting.TempDir, true);
                }

                CancelAndCleanupArchiveExtract(); // deal with archive extract

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
                //Toolbar

                //btnBack.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnBack"];
                btnBack.ToolTipText = string.Format("{0} ({1})", GlobalSetting.LangPack.Items["frmMain.mnuMainViewPrevious"],
                                                                 GlobalSetting.LangPack.Items["frmMain.mnuMainViewPrevious.Shortcut"]);

                //btnNext.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnNext"];
                btnNext.ToolTipText = string.Format("{0} ({1})", GlobalSetting.LangPack.Items["frmMain.mnuMainViewNext"],
                                                                 GlobalSetting.LangPack.Items["frmMain.mnuMainViewNext.Shortcut"]);

                btnRotateLeft.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRotateLeft"];
                btnRotateRight.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRotateRight"];
                btnFlipHorz.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFlipHorz"];
                btnFlipVert.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFlipVert"];
                btnDelete.ToolTipText = $"{GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"]} ({mnuMainMoveToRecycleBin.ShortcutKeys.ToString()})";
                btnZoomIn.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomIn"];
                btnZoomOut.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomOut"];
                btnAutoZoom.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnAutoZoom"];
                btnScaleToFit.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnScaleToFit"];
                btnActualSize.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnActualSize"];
                btnZoomLock.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomLock"];
                btnScaletoWidth.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnScaletoWidth"];
                btnScaletoHeight.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnScaletoHeight"];
                btnWindowAutosize.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnWindowAutosize"];
                btnOpen.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnOpen"];
                btnRefresh.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRefresh"];
                btnGoto.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnGoto"];
                btnThumb.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnThumb"];
                btnCheckedBackground.ToolTipText = $"{GlobalSetting.LangPack.Items["frmMain.mnuMainCheckBackground"]} (Ctrl + B)"; ;
                btnFullScreen.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFullScreen"];
                btnSlideShow.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnSlideShow"];
                btnConvert.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnConvert"];
                btnPrintImage.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnPrintImage"];
                btnMenu.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnMenu"];

                //Main menu
                mnuMainOpenFile.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainOpenFile"];
                mnuMainOpenImageData.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainOpenImageData"];
                mnuMainSaveAs.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSaveAs"];
                mnuMainRefresh.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRefresh"];
                mnuMainReloadImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainReloadImage"];
                mnuMainEditImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"];

                mnuMainNavigation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainNavigation"];
                mnuMainViewNext.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainViewNext"];
                mnuMainViewNext.ShortcutKeyDisplayString = GlobalSetting.LangPack.Items["frmMain.mnuMainViewNext.Shortcut"];

                mnuMainViewPrevious.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainViewPrevious"];
                mnuMainViewPrevious.ShortcutKeyDisplayString = GlobalSetting.LangPack.Items["frmMain.mnuMainViewPrevious.Shortcut"];

                mnuMainGoto.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGoto"];
                mnuMainGotoFirst.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGotoFirst"];
                mnuMainGotoLast.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGotoLast"];

                mnuMainFullScreen.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainFullScreen"];

                mnuMainSlideShow.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShow"];
                mnuMainSlideShowStart.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowStart"];
                mnuMainSlideShowPause.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowPause"];
                mnuMainSlideShowExit.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowExit"];

                mnuMainPrint.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainPrint"];

                mnuMainManipulation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainManipulation"];
                mnuMainRotateCounterclockwise.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRotateCounterclockwise"];
                mnuMainRotateClockwise.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRotateClockwise"];
                mnuMainZoomIn.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainZoomIn"];
                mnuMainZoomOut.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainZoomOut"];
                mnuMainScaleToFit.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainScaleToFit"];
                mnuMainActualSize.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainActualSize"];
                mnuMainLockZoomRatio.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainLockZoomRatio"];
                mnuMainAutoZoom.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainAutoZoom"];
                mnuMainScaleToWidth.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainScaleToWidth"];
                mnuMainScaleToHeight.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainScaleToHeight"];
                mnuMainWindowAdaptImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainWindowAdaptImage"];
                mnuMainRename.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRename"];
                mnuMainMoveToRecycleBin.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"];
                mnuMainDeleteFromHardDisk.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainDeleteFromHardDisk"];
                mnuMainExtractFrames.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"];
                mnuMainStartStopAnimating.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainStartStopAnimating"];
                mnuMainSetAsDesktop.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSetAsDesktop"];
                mnuMainSetAsLockImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSetAsLockImage"];
                mnuMainImageLocation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainImageLocation"];
                mnuMainImageProperties.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainImageProperties"];

                mnuMainClipboard.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainClipboard"];
                mnuMainCopy.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopy"];
                mnuMainCopyImageData.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopyImageData"];
                mnuMainCut.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCut"];
                mnuMainCopyImagePath.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopyImagePath"];
                mnuMainClearClipboard.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainClearClipboard"];

                mnuMainShare.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainShare"];

                mnuMainLayout.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainLayout"];
                mnuMainToolbar.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainToolbar"];
                mnuMainThumbnailBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainThumbnailBar"];
                mnuMainCheckBackground.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCheckBackground"];
                mnuMainAlwaysOnTop.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainAlwaysOnTop"];

                mnuMainTools.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainTools"];
                mnuMainColorPicker.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainColorPicker"];

                mnuMainSettings.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSettings"];
                mnuMainAbout.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainAbout"];

                mnuMainFirstLaunch.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainFirstLaunch"];
                mnuMainReportIssue.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainReportIssue"];
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
                ApplyTheme(LocalSetting.Theme.ThemeConfigFilePath);
                LocalSetting.FColorPicker.UpdateUI();
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

                //Update background---------------------
                picMain.BackColor = GlobalSetting.BackgroundColor;

                //Update slideshow interval value of timer
                timSlideShow.Interval = GlobalSetting.SlideShowInterval * 1000;

                #endregion

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
                toolMain_SizeChanged(null, null); // For centered toolbar buttons
            }
            #endregion


            #region IMAGE_FOLDER and IMAGE_LIST

            #region IMAGE_FOLDER
            if ((flags & MainFormForceUpdateAction.IMAGE_FOLDER) == MainFormForceUpdateAction.IMAGE_FOLDER)
            {
                Prepare(LocalSetting.InitialInputImageFilename,
                        GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            }
            #endregion

            #region IMAGE_LIST
            else
            {
                if ((flags & MainFormForceUpdateAction.IMAGE_LIST) == MainFormForceUpdateAction.IMAGE_LIST)
                {
                    //reload image list
                    // KBR 20180903 Fix observed issue: rebuild the list using the initial file, not the current,
                    // but keep the currently visible file in mind!
                    Prepare(LocalSetting.InitialInputImageFilename,
                            GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                }
            }
            #endregion

            #endregion


            #region Windows 10 Specific Actions
            bool enable = true;
            var vers = Environment.OSVersion;
            // Win7 == 6.1, Win Server 2008 == 6.1
            // Win10 == 10.0 [if app.manifest properly configured]
            if (vers.Version.Major < 6 ||
                vers.Version.Major == 6 && vers.Version.Minor < 2)
                enable = false; // Not running Windows 8 or later

            mnuMainSetAsLockImage.Enabled = enable;
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

        }

        private void thumbnailBar_ItemClick(object sender, ImageListView.ItemClickEventArgs e)
        {
            GlobalSetting.CurrentIndex = e.Item.Index;
            NextPic(0);
        }

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            NextPic(1);

            //stop playing slideshow at last image
            if (GlobalSetting.CurrentIndex == GlobalSetting.ImageList.Length - 1)
            {
                if (!GlobalSetting.IsLoopBackSlideShow)
                {
                    mnuMainSlideShowPause_Click(null, null);
                }
            }
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
                if (oldFilename == LocalSetting.InitialInputImageFilename)
                    LocalSetting.InitialInputImageFilename = newFilename;
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
            GlobalSetting.ImageList.AddItem(newFilename);

            //Add the new image to thumbnail bar
            ImageListView.ImageListViewItem lvi = new ImageListView.ImageListViewItem(newFilename)
            {
                Tag = newFilename
            };

            thumbnailBar.Items.Add(lvi);
            thumbnailBar.Refresh();


            UpdateStatusBar(); // File count has changed: update the title bar
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

                    DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._ImageNotExist"], 1300);

                    if (_queueListForDeleting.Count == 0)
                    {
                        NextPic(0);
                    }
                }


                // If user deletes the initially loaded image, use the path instead, in case
                // of list re-load.
                if (filename == LocalSetting.InitialInputImageFilename)
                    LocalSetting.InitialInputImageFilename = Path.GetDirectoryName(filename);


                UpdateStatusBar(); // File count has changed: update the title bar

            }
        }


        #endregion






        // Use mouse wheel to navigate, scroll, or zoom images
        private void picMain_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelActions action;
            switch(Control.ModifierKeys)
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
            switch(action)
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

        private void picMain_DoubleClick(object sender, EventArgs e)
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

                default:
                    break;
            }
        }

        private void toolMain_SizeChanged(object sender, EventArgs e)
        {
            if (toolMain.PreferredSize.Width > toolMain.Size.Width)
            {
                btnMenu.Alignment = ToolStripItemAlignment.Left;
            }
            else
            {
                btnMenu.Alignment = ToolStripItemAlignment.Right;

                // Issue 425: option to center the toolbar buttons horizontally [useful for wide screen]
                // I'm assuming the btnMenu stays to the right, in order to always be at a fixed location.
                var firstbtn = toolMain.Items[0];
                var marg = new Padding(2, firstbtn.Margin.Top, firstbtn.Margin.Right, firstbtn.Margin.Bottom);
                if (GlobalSetting.IsCenterToolbar)
                {

                    // NOTE: relies on the label control on the right of the menu button!
                    // NOTE: assumes at least one control to the left of the menu button in the toolbar!
                    var lastbut1btn = toolMain.Items[toolMain.Items.Count - 3]; 

                    int delta = btnMenu.Bounds.Right - lastbut1btn.Bounds.Right;
                    marg.Left = delta / 2;
                }
                firstbtn.Margin = marg;
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
            if (picMain.Image == null)
            {
                return;
            }

            if (picMain.CanAnimate)
            {
                DisplayTextMessage(GlobalSetting.LangPack.Items[$"{this.Name}._CannotRotateAnimatedFile"], 1000);
                return;
            }

            picMain.Image = Interpreter.Flip(picMain.Image, horz: true);

            try
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
            catch { }

        }

        private void btnFlipVert_Click(object sender, EventArgs e)
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

            picMain.Image = Interpreter.Flip(picMain.Image, horz: false);

            try
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
            catch { }
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

        //private void btnSetting_Click(object sender, EventArgs e)
        //{
        //    mnuMainSettings_Click(null, e);
        //}

        //private void btnHelp_Click(object sender, EventArgs e)
        //{
        //    mnuMainAbout_Click(null, e);
        //}

        private void btnConvert_Click(object sender, EventArgs e)
        {
            mnuMainSaveAs_Click(null, e);
        }

        //private void btnReport_Click(object sender, EventArgs e)
        //{
        //    mnuMainReportIssue_Click(null, e);
        //}

        private void btnMenu_Click(object sender, EventArgs e)
        {
            mnuMain.Show(toolMain, toolMain.Width - mnuMain.Width, toolMain.Height);
        }
        #endregion



        #region Context Menu
        private void mnuContext_Opening(object sender, CancelEventArgs e)
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
                mnuContext.Items.Add(new ToolStripSeparator());//---------------

                UpdateEditingAssocAppInfoForMenu();
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainEditImage));

                //check if image can animate (GIF)
                try
                {
                    Image img = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                    FrameDimension dim = new FrameDimension(img.FrameDimensionsList[0]);
                    int frameCount = img.GetFrameCount(dim);

                    if (frameCount > 1)
                    {
                        var mi = Library.Menu.Clone(mnuMainExtractFrames);
                        mi.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"], frameCount);
                        mi.Enabled = true;

                        mnuContext.Items.Add(mi);
                        var mi2 = Library.Menu.Clone(mnuMainStartStopAnimating);
                        mi2.Enabled = true;
                        mnuContext.Items.Add(mi2);
                    }

                }
                catch { }
            }


            if (!isImageError && !LocalSetting.IsTempMemoryData)
            {
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainSetAsDesktop));
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
                mnuContext.Items.Add(Library.Menu.Clone(mnuMainDeleteFromHardDisk));

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

        private void mnuMainOpenImageData_Click(object sender, EventArgs e)
        {
            //Is there a file in clipboard ?--------------------------------------------------
            if (Clipboard.ContainsFileDropList())
            {
                string[] sFile = (string[])Clipboard.GetData(DataFormats.FileDrop);
                int fileCount = 0;

                fileCount = sFile.Length;

                // load file
                Prepare(sFile[0]);
            }


            //Is there a image in clipboard ?-------------------------------------------------
            //CheckImageInClipboard: ;
            else if (Clipboard.ContainsImage())
            {
                LoadImageData(Clipboard.GetImage());
            }

            //Is there a filename in clipboard?-----------------------------------------------
            //CheckPathInClipboard: ;
            else if (Clipboard.ContainsText())
            {
                if (File.Exists(Clipboard.GetText()) || Directory.Exists(Clipboard.GetText()))
                {
                    Prepare(Clipboard.GetText());
                }
                //get image from Base64string 
                else
                {
                    try
                    {
                        // data:image/jpeg;base64,xxxxxxxx
                        string base64str = Clipboard.GetText().Substring(Clipboard.GetText().LastIndexOf(',') + 1);
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

        private void mnuMainSaveAs_Click(object sender, EventArgs e)
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


            var output = ImageInfo.ConvertImage(picMain.Image, filename);

            //display successful msg
            if (File.Exists(output))
            {
                DisplayTextMessage(string.Format(GlobalSetting.LangPack.Items["frmMain._SaveImage"], output), 2000);
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
            string s = "0";
            if (InputBox.ShowDiaLog("Message", GlobalSetting.LangPack.Items["frmMain._GotoDialogText"],
                                    "0", true, this.TopMost) == DialogResult.OK)
            {
                s = InputBox.Message;
            }

            if (int.TryParse(s, out n))
            {
                n--;

                if (-1 < n && n < GlobalSetting.ImageList.Length)
                {
                    GlobalSetting.CurrentIndex = n;
                    NextPic(0);
                }
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
            //full screen
            if (!GlobalSetting.IsFullScreen)
            {
                SaveConfig();

                //save last state of toolbar
                _isShowToolbar = GlobalSetting.IsShowToolBar;
                _isShowThumbnail = GlobalSetting.IsShowThumbnail;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                GlobalSetting.IsFullScreen = true;
                Application.DoEvents();
                Bounds = Screen.FromControl(this).Bounds;

                //Hide toolbar
                toolMain.Visible = false;
                GlobalSetting.IsShowToolBar = false;
                mnuMainToolbar.Checked = false;

                //hide thumbnail
                GlobalSetting.IsShowThumbnail = true;
                mnuMainThumbnailBar_Click(null, null);

                //realign image
                if (!_isManuallyZoomed)
                {
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }

                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._FullScreenMessage"]
                    , 2000);
            }
            //exit full screen
            else
            {
                //restore last state of toolbar
                GlobalSetting.IsShowToolBar = _isShowToolbar;
                GlobalSetting.IsShowThumbnail = _isShowThumbnail;

                FormBorderStyle = FormBorderStyle.Sizable;

                //windows state
                string state_str = GlobalSetting.GetConfig($"{Name}.WindowsState", "Normal");
                if (state_str == "Normal")
                {
                    WindowState = FormWindowState.Normal;
                }
                else if (state_str == "Maximized")
                {
                    WindowState = FormWindowState.Maximized;
                }

                //Windows Bound (Position + Size)
                Bounds = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", "280,125,750,545"));

                GlobalSetting.IsFullScreen = false;
                Application.DoEvents();
                
                if (GlobalSetting.IsShowToolBar)
                {
                    //Show toolbar
                    toolMain.Visible = true;
                    mnuMainToolbar.Checked = true;
                }

                if (GlobalSetting.IsShowThumbnail)
                {
                    //Show thumbnail
                    GlobalSetting.IsShowThumbnail = false;
                    mnuMainThumbnailBar_Click(null, null);
                }

                //realign image
                if (!_isManuallyZoomed)
                {
                    ApplyZoomMode(GlobalSetting.ZoomMode);
                }
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
                //perform slideshow
                picMain.BackColor = Color.Black;
                btnFullScreen.PerformClick();

                timSlideShow.Start();
                timSlideShow.Enabled = true;

                GlobalSetting.IsPlaySlideShow = true;
            }
            //performing
            else
            {
                mnuMainSlideShowExit_Click(null, null);
            }

            DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SlideshowMessage"], 2000);
        }

        private void mnuMainSlideShowPause_Click(object sender, EventArgs e)
        {
            //performing
            if (timSlideShow.Enabled)
            {
                timSlideShow.Enabled = false;
                timSlideShow.Stop();

                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SlideshowMessagePause"], 2000);
            }
            else
            {
                timSlideShow.Enabled = true;
                timSlideShow.Start();

                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SlideshowMessageResume"], 2000);
            }

        }

        private void mnuMainSlideShowExit_Click(object sender, EventArgs e)
        {
            timSlideShow.Stop();
            timSlideShow.Enabled = false;
            GlobalSetting.IsPlaySlideShow = false;

            picMain.BackColor = GlobalSetting.BackgroundColor;
            btnFullScreen.PerformClick();
        }
        
        private void mnuMainPrint_Click(object sender, EventArgs e)
        {
            //image error
            if (picMain.Image == null)
            {
                return;
            }

            //save image to temp file
            string temFile = "";
            temFile = SaveTemporaryMemoryData();
            

            Process p = new Process();
            p.StartInfo.FileName = temFile;
            p.StartInfo.Verb = "print";

            //show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();
            }
            catch (Exception)
            { }

        }

        private void mnuMainRotateCounterclockwise_Click(object sender, EventArgs e)
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

            picMain.Image = Interpreter.RotateImage(picMain.Image, 270);

            try
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
            catch { }
            ApplyZoomMode(GlobalSetting.ZoomMode);
        }

        private void mnuMainRotateClockwise_Click(object sender, EventArgs e)
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

            picMain.Image = Interpreter.RotateImage(picMain.Image, 90);

            try
            {
                // Save the image path for saving
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            }
            catch { }
            ApplyZoomMode(GlobalSetting.ZoomMode);
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
            picMain.CenterToImage();
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
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) ||
                    LocalSetting.FilesFromArchive) // disallow when image from archive
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = DialogResult.Yes;

            if (GlobalSetting.IsConfirmationDelete)
            {
                msg = MessageBox.Show(string.Format(GlobalSetting.LangPack.Items["frmMain._DeleteDialogText"], GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)), GlobalSetting.LangPack.Items["frmMain._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (msg == DialogResult.Yes)
            {
                string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                try
                {
                    ImageInfo.DeleteFile(filename, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainDeleteFromHardDisk_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) ||
                    LocalSetting.FilesFromArchive ) // Don't allow delete if from archive
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show(string.Format(GlobalSetting.LangPack.Items["frmMain._DeleteDialogText"], GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)), GlobalSetting.LangPack.Items["frmMain._DeleteDialogTitle"], MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (msg == DialogResult.Yes)
            {
                

                string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                try
                {
                    ImageInfo.DeleteFile(filename);                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainExtractFrames_Click(object sender, EventArgs e)
        {
            if (!(sender as ToolStripMenuItem).Enabled) // Shortcut keys still work even when menu is disabled!
                return;

            if (!GlobalSetting.IsImageError)
            {
                FolderBrowserDialog f = new FolderBrowserDialog
                {
                    Description = GlobalSetting.LangPack.Items["frmMain._ExtractFrameText"],
                    ShowNewFolderButton = true
                };

                DialogResult res = f.ShowDialog();

                if (res == DialogResult.OK && Directory.Exists(f.SelectedPath))
                {
                    Animation ani = new Animation();
                    ani.ExtractAllFrames(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex), f.SelectedPath);
                }

                f = null;
            }
        }

        // ReSharper disable once EmptyGeneralCatchClause
        private void mnuMainSetAsDesktop_Click(object sender, EventArgs e)
        {
            if (LocalSetting.IsTempMemoryData && !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                return;

            try
            {
                var args = string.Format("setwallpaper \"{0}\" {1}",
                    GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex),
                    (int)DesktopWallapaper.Style.Current);

                // Issue #326: first attempt to set wallpaper w/o privs. If that
                // fails _due_to_ privs error, re-attempt with admin privs.
                Process p = new Process();
                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igcmd.exe";
                p.StartInfo.Arguments = args;
                p.Start();
                p.WaitForExit();
                int result = p.ExitCode;
                if (result != (int)DesktopWallapaper.Result.PrivsFail)
                    return;

                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igtasks.exe";
                p.StartInfo.Arguments = args;
                p.Start();
            }
            catch
            { }
        }

        private void mnuMainSetAsLockImage_Click(object sender, EventArgs e)
        {
            if (LocalSetting.IsTempMemoryData && !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                return;

            var vers = Environment.OSVersion;
            // Win7 == 6.1, Win Server 2008 == 6.1
            // Win10 == 10.0 [if app.manifest properly configured]
            if (vers.Version.Major < 6 ||
                vers.Version.Major == 6 && vers.Version.Minor < 2)
                return; // Not running Windows 8 or later

            // TODO consider adding privilege check and retry?
            try
            {
                var args = string.Format("setlockimage \"{0}\"",
                    GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                Process p = new Process();
                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igcmdWin10.exe";
                p.StartInfo.Arguments = args;
                p.Start();
                return;
            }
            catch
            { }
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
                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._CopyImageData"], 1000);
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
                DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._ClearClipboard"], 1000);
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
            ApplyZoomMode(GlobalSetting.ZoomMode); // Resize image to adapt when thumbbar turned off
        }

        private void mnuMainCheckBackground_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowCheckerBoard = !GlobalSetting.IsShowCheckerBoard;
            btnCheckedBackground.Checked = GlobalSetting.IsShowCheckerBoard;

            if (btnCheckedBackground.Checked)
            {
                //show
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.Client;
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
            p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igcmd.exe");
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

        private void mnuMain_Opening(object sender, CancelEventArgs e)
        {
            btnMenu.Checked = true;

            try
            {
                // Alert user if there is a new version
                if (GlobalSetting.IsNewVersionAvailable)
                {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCheckForUpdate._NewVersion"];
                    mnuMainCheckForUpdate.BackColor = Color.FromArgb(35, 255, 165, 2);
                }
                else
                {
                    mnuMainCheckForUpdate.Text = mnuMainCheckForUpdate.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCheckForUpdate._NoUpdate"];
                }

                

                mnuMainExtractFrames.Enabled = false;
                mnuMainStartStopAnimating.Enabled = false;


                int frameCount = 0;
                if (GlobalSetting.CurrentIndex >= 0)
                {
                    Image img = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                    FrameDimension dim = new FrameDimension(img.FrameDimensionsList[0]);
                    frameCount = img.GetFrameCount(dim);
                }
                

                mnuMainExtractFrames.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"], frameCount);

                if (frameCount > 1)
                {
                    mnuMainExtractFrames.Enabled = true;
                    mnuMainStartStopAnimating.Enabled = true;
                }

                // Get association App for editing
                UpdateEditingAssocAppInfoForMenu();
                
            }
            catch (Exception ex) { }
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
            Rectangle bounds = Screen.GetWorkingArea(pos);
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
