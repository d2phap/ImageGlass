/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Base.DirectoryComparer;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Gallery;
using ImageGlass.Settings;
using System.Diagnostics;
using System.Reflection;

namespace ImageGlass;

public partial class FrmMain : Form
{
    // cancellation tokens of synchronious task
    private CancellationTokenSource _loadCancelToken = new();


    public FrmMain()
    {
        InitializeComponent();

        // Get the DPI of the current display
        DpiApi.OnDpiChanged += OnDpiChanged;
        DpiApi.CurrentDpi = DeviceDpi;

        SetUpFrmMainConfigs();
        SetUpFrmMainTheme();

        // apply DPI changes
        OnDpiChanged();
    }

    private void FrmMain_Load(object sender, EventArgs e)
    {
        Local.OnImageLoading += Local_OnImageLoading;
        Local.OnImageListLoaded += Local_OnImageListLoaded;
        Local.OnImageLoaded += Local_OnImageLoaded;

        LoadImagesFromCmdArgs();
    }

    protected override void WndProc(ref Message m)
    {
        // WM_SYSCOMMAND
        if (m.Msg == 0x0112)
        {
            // When user clicks on MAXIMIZE button on title bar
            if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
            {
                // The window is being maximized
            }
            // When user clicks on the RESTORE button on title bar
            else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
            {
                // The window is being restored
            }
        }
        else if (m.Msg == DpiApi.WM_DPICHANGED)
        {
            // get new dpi value
            DpiApi.CurrentDpi = (short)m.WParam;
        }

        base.WndProc(ref m);
    }


    private void FrmMain_Resize(object sender, EventArgs e)
    {
    }

    private void Gallery_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.Item.Index == Local.CurrentIndex)
        {
            PicMain.Refresh();
        }
        else
        {
            GoToImageAsync(e.Item.Index);
        }
    }


    private void Toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        var tagModel = e.ClickedItem.Tag as ToolbarItemTagModel;
        if (tagModel is null || string.IsNullOrEmpty(tagModel.OnClick.Executable)) return;

        // Find the private method in FrmMain
        var method = GetType().GetMethod(
            tagModel.OnClick.Executable,
            BindingFlags.Instance | BindingFlags.NonPublic);


        // run built-in method
        if (method is not null)
        {
            // method must be bool/void()
            method?.Invoke(this, new[] { tagModel.OnClick.Arguments });

            return;
        }


        var currentFilePath = Local.Images.GetFileName(Local.CurrentIndex);

        // run external command line
        var proc = new Process
        {
            StartInfo = new(tagModel.OnClick.Executable)
            {
                Arguments = tagModel.OnClick.Arguments.Replace(Constants.FILE_MACRO, currentFilePath),
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true,
            },
        };

        try
        {
            proc.Start();
        }
        catch { }
    }


    /// <summary>
    /// Handle DPI changes
    /// </summary>
    private void OnDpiChanged()
    {
        Text = DpiApi.CurrentDpi.ToString();

        // scale toolbar icons corresponding to DPI
        var newIconHeight = DpiApi.Transform(Config.ToolbarIconHeight);

        // reload theme
        Config.Theme.LoadTheme(newIconHeight);

        // update toolbar theme
        Toolbar.UpdateTheme(newIconHeight);
    }


    #region Image Loading functions

    /// <summary>
    /// Load images from command line arguments
    /// (<see cref="Environment.GetCommandLineArgs"/>)
    /// </summary>
    /// <param name="args"></param>
    public void LoadImagesFromCmdArgs()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length < 2) return;

        // get path from params
        var path = args.Skip(1).FirstOrDefault(i => !i.StartsWith(Constants.CONFIG_CMD_PREFIX));

        if (path == null) return;

        // Start loading path
        Helpers.RunAsThread(() => PrepareLoading(path));
    }


    /// <summary>
    /// Open file picker and load the selected image
    /// </summary>
    private void OpenFilePicker()
    {
        var formats = Config.GetImageFormats(Config.AllFormats);
        using var o = new OpenFileDialog()
        {
            Filter = Config.Language[$"{Name}._OpenFileDialog"] + "|" + formats,
            CheckFileExists = true,
            RestoreDirectory = true,
        };

        if (o.ShowDialog() == DialogResult.OK)
        {
            PrepareLoading(o.FileName);
        }
    }
    

    /// <summary>
    /// Prepare and loads images from the input path
    /// </summary>
    /// <param name="inputPath">
    /// The relative/absolute path of file/folder;
    /// or a URI Scheme
    /// </param>
    private void PrepareLoading(string inputPath)
    {
        var path = Helpers.ResolvePath(inputPath);

        if (Helpers.IsDirectory(path))
        {
            _ = PrepareLoadingAsync(new string[] { inputPath }, "");
        }
        else
        {
            // load images list
            _ = LoadImageListAsync(new string[] { inputPath }, path);

            // load the current image
            _ = ViewNextCancellableAsync(0, filename: path);
        }
    }


    /// <summary>
    /// Prepares and load images from the input paths
    /// </summary>
    /// <param name="paths"></param>
    /// <param name="currentFile"></param>
    /// <returns></returns>
    private async Task PrepareLoadingAsync(string[] paths, string? currentFile = null)
    {
        var filePath = currentFile;

        if (string.IsNullOrEmpty(currentFile))
        {
            filePath = paths.AsParallel().FirstOrDefault(i => !Helpers.IsDirectory(i));
            filePath = Helpers.ResolvePath(filePath);
        }

        if (string.IsNullOrEmpty(filePath))
        {
            // load images list
            await LoadImageListAsync(paths, currentFile ?? filePath ?? "");

            // load the current image
            await ViewNextCancellableAsync(0);
        }
        else
        {
            // load the current image
            _ = ViewNextCancellableAsync(0, filename: filePath);

            // load images list
            _ = LoadImageListAsync(paths, currentFile ?? filePath ?? "");
        }
    }


    /// <summary>
    /// Load the images list.
    /// </summary>
    /// <param name="inputPaths">The list of files to load</param>
    /// <param name="currentFilePath">The image file path to view first</param>
    private async Task LoadImageListAsync(
        IEnumerable<string> inputPaths,
        string currentFilePath)
    {
        if (!inputPaths.Any()) return;

        await Task.Run(() =>
        {
            var allFilesToLoad = new HashSet<string>();
            var currentFile = currentFilePath;
            var hasInitFile = !string.IsNullOrEmpty(currentFile);


            // track paths loaded to prevent duplicates
            var pathsLoaded = new HashSet<string>();
            var firstPath = true;

            // Parse string to absolute path
            var paths = inputPaths.Select(item => Helpers.ResolvePath(item));

            // prepare the distinct dir list
            var distinctDirsList = Helpers.GetDistinctDirsFromPaths(paths);

            foreach (var aPath in distinctDirsList)
            {
                var dirPath = aPath;
                var isDir = false;

                try
                {
                    isDir = Helpers.IsDirectory(aPath);
                }
                catch { continue; }

                // path is directory
                if (isDir)
                {
                    // Issue #415: If the folder name ends in ALT+255 (alternate space),
                    // DirectoryInfo strips it.
                    if (!aPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        dirPath = aPath + Path.DirectorySeparatorChar;
                    }
                }
                // path is file
                else
                {
                    if (string.Equals(Path.GetExtension(aPath), ".lnk", StringComparison.CurrentCultureIgnoreCase))
                    {
                        dirPath = FileShortcutApi.GetTargetPathFromShortcut(aPath);
                    }
                    else
                    {
                        dirPath = Path.GetDirectoryName(aPath) ?? "";
                    }
                }


                // TODO: Currently only have the ability to watch a single path for changes!
                if (firstPath)
                {
                    firstPath = false;

                    // TODO:
                    //WatchPath(dirPath);

                    // Seek for explorer sort order
                    DetermineSortOrder(dirPath);
                }

                // KBR 20181004 Fix observed bug: dropping multiple files from the same path
                // would load ALL files in said path multiple times! Prevent loading the same
                // path more than once.
                if (!pathsLoaded.Contains(dirPath))
                {
                    pathsLoaded.Add(dirPath);

                    var imageFilenameList = LoadImageFilesFromDirectory(dirPath);
                    allFilesToLoad.UnionWith(imageFilenameList);
                }
            }


            Local.InitialInputPath = hasInitFile ? (distinctDirsList.Count > 0 ? distinctDirsList[0] : "") : currentFile;


            // sort list
            var sortedFilesList = SortImageList(allFilesToLoad);

            // add to image list
            Local.InitImageList(sortedFilesList);

            // Find the index of current image
            UpdateCurrentIndex(currentFilePath);

            Local.RaiseImageListLoadedEvent(new()
            {
                FilePath = currentFilePath,
            });
        });
    }


    /// <summary>
    /// Updates <see cref="Local.CurrentIndex"/> according to the context.
    /// </summary>
    /// <param name="currentFilePath"></param>
    private static void UpdateCurrentIndex(string? currentFilePath)
    {
        if (string.IsNullOrEmpty(currentFilePath))
        {
            Local.CurrentIndex = 0;
            return;
        }

        // this part of code fixes calls on legacy 8.3 filenames
        // (for example opening files from IBM Notes)
        var di = new DirectoryInfo(currentFilePath);
        currentFilePath = di.FullName;

        // Find the index of current image
        Local.CurrentIndex = Local.Images.IndexOf(currentFilePath);

        // KBR 20181009
        // Changing "include subfolder" setting could lose the "current" image. Prefer
        // not to report said image is "corrupt", merely reset the index in that case.
        // 1. Setting: "include subfolders: ON".
        //    Open image in folder with images in subfolders.
        // 2. Move to an image in a subfolder.
        // 3. Change setting "include subfolders: OFF".
        // Issue: the image in the subfolder is attempted to be shown,
        // declared as corrupt/missing.
        // Issue #481: the test is incorrect when imagelist is empty (i.e. attempt to
        // open single, hidden image with 'show hidden' OFF)
        if (Local.CurrentIndex == -1
            && Local.Images.Length > 0
            && !Local.Images.ContainsDirPathOf(currentFilePath))
        {
            Local.CurrentIndex = 0;
        }
    }


    /// <summary>
    /// Determine the image sort order/direction based on user settings
    /// or Windows Explorer sorting.
    /// <para>
    /// Side effects:
    /// </para>
    /// <list type="bullet">
    ///     <item>Updates <see cref="Local.ActiveImageLoadingOrder"/></item>
    ///     <item>Updates <see cref="Local.ActiveImageLoadingOrderType"/></item>
    /// </list>
    /// </summary>
    /// <param name="fullPath">
    /// Full path to file/folder(i.e. as comes in from drag-and-drop)
    /// </param>
    private static void DetermineSortOrder(string fullPath)
    {
        // Initialize to the user-configured sorting order. Fetching the Explorer sort
        // order may fail, or may be on an unsupported column.
        Local.ActiveImageLoadingOrder = Config.ImageLoadingOrder;
        Local.ActiveImageLoadingOrderType = Config.ImageLoadingOrderType;

        // Use File Explorer sort order if possible
        if (Config.IsUseFileExplorerSortOrder)
        {
            if (ExplorerSortOrder.GetExplorerSortOrder(fullPath, out var explorerOrder, out var isAscending))
            {
                if (explorerOrder != null)
                {
                    Local.ActiveImageLoadingOrder = explorerOrder.Value;
                }

                if (isAscending != null)
                {
                    Local.ActiveImageLoadingOrderType = isAscending.Value ? ImageOrderType.Asc : ImageOrderType.Desc;
                }
            }
        }
    }


    /// <summary>
    /// Sort and find all supported image from directory
    /// </summary>
    /// <param name="path">Image folder path</param>
    private static IEnumerable<string> LoadImageFilesFromDirectory(string path)
    {
        // Get files from dir
        return DirectoryFinder.FindFiles(path,
            Config.IsRecursiveLoading,
            new Predicate<FileInfo>((FileInfo fi) =>
            {
                // KBR 20180607 Rework predicate to use a FileInfo instead of the filename.
                // By doing so, can use the attribute data already loaded into memory, 
                // instead of fetching it again (via File.GetAttributes). A re-fetch is
                // very slow across network paths. For me, improves image load from 4+ 
                // seconds to 0.4 seconds for a specific network path.
                if (fi.FullName == null)
                    return false;

                var extension = fi.Extension.ToLower();

                // checks if image is hidden and ignores it if so
                if (!Config.IsShowingHiddenImages)
                {
                    var attributes = fi.Attributes;
                    var isHidden = (attributes & FileAttributes.Hidden) != 0;
                    if (isHidden)
                    {
                        return false;
                    }
                }

                return extension.Length > 0 && Config.AllFormats.Contains(extension);
            }));
    }


    /// <summary>
    /// Sort image list
    /// </summary>
    /// <param name="fileList"></param>
    /// <returns></returns>
    private static IEnumerable<string> SortImageList(IEnumerable<string> fileList)
    {
        // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!

        // KBR 20190605
        // Fix observed limitation: to more closely match the Windows Explorer's sort
        // order, we must sort by the target column, then by name.
        var naturalSortComparer = Local.ActiveImageLoadingOrderType == ImageOrderType.Desc
                                    ? (IComparer<string>)new ReverseWindowsNaturalSort()
                                    : new WindowsNaturalSort();

        // initiate directory sorter to a comparer that does nothing
        // if user wants to group by directory, we initiate the real comparer
        var directorySortComparer = (IComparer<string>)new IdentityComparer();
        if (Config.IsGroupImagesByDirectory)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                directorySortComparer = new ReverseWindowsDirectoryNaturalSort();
            }
            else
            {
                directorySortComparer = new WindowsDirectoryNaturalSort();
            }
        }

        // KBR 20190605 Fix observed discrepancy: using UTC for create,
        // but not for write/access times

        // Sort image file
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.Length)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by CreationTime
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.CreationTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by Extension
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.Extension)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by LastAccessTime
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastAccessTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
        }

        // sort by LastWriteTime
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastWriteTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => f, naturalSortComparer);
            }
            else
            {
                return fileList.AsParallel()
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => f, naturalSortComparer)
                    .ThenBy(f => new FileInfo(f).LastWriteTimeUtc);
            }
        }

        // sort by Random
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.Random)
        {
            // NOTE: ignoring the 'descending order' setting
            return fileList.AsParallel()
                .OrderBy(f => f, directorySortComparer)
                .ThenBy(_ => Guid.NewGuid());
        }

        // sort by Name (default)
        return fileList.AsParallel()
            .OrderBy(f => f, directorySortComparer)
            .ThenBy(f => f, naturalSortComparer);
    }


    /// <summary>
    /// Clear and reload all thumbnail image
    /// </summary>
    private void LoadThumbnails()
    {
        if (InvokeRequired)
        {
            Invoke(LoadThumbnails);
            return;
        }

        Gallery.SuspendLayout();
        Gallery.Items.Clear();
        Gallery.ThumbnailSize = new Size(Config.ThumbnailDimension, Config.ThumbnailDimension);


        //var watch = new Stopwatch();
        //watch.Start();

        for (var i = 0; i < Local.Images.Length; i++)
        {
            var lvi = new ImageGalleryItem(Local.Images.GetFileName(i));
            Gallery.Items.Add(lvi);
        }

        //watch.Stop();
        //MessageBox.Show($"{watch.ElapsedMilliseconds} ms for {Gallery.Items.Count} items.");

        Gallery.ResumeLayout();

        SelectCurrentThumbnail();
    }


    /// <summary>
    /// Select current thumbnail
    /// </summary>
    private void SelectCurrentThumbnail()
    {
        if (InvokeRequired)
        {
            Invoke(new(SelectCurrentThumbnail));
            return;
        }

        if (Gallery.Items.Count > 0)
        {
            Gallery.ClearSelection();

            try
            {
                Gallery.Items[Local.CurrentIndex].Selected = true;
                Gallery.Items[Local.CurrentIndex].Focused = true;
                Gallery.ScrollToIndex(Local.CurrentIndex);
            }
            catch (ArgumentOutOfRangeException) { }
        }
    }


    /// <summary>
    /// View the next image using jump step.
    /// </summary>
    private static async Task ViewNextAsync(int step,
        bool isKeepZoomRatio = false,
        bool isSkipCache = false,
        int pageIndex = int.MinValue,
        string filename = "",
        CancellationTokenSource? token = null)
    {
        IgPhoto? photo = null;
        var directReadSettings = new CodecReadOptions()
        {
            ColorProfileName = Config.ColorProfile,
            IsApplyColorProfileForAll = Config.IsApplyColorProfileForAll,
            ImageChannel = Local.ImageChannel,
            UseRawThumbnail = Config.IsUseRawThumbnail,
            //UseEmbeddedThumbnail = Local.Images.use
            Metadata = Local.Metadata,
        };


        #region Validate image index & load image metadata

        // temp index
        var imageIndex = Local.CurrentIndex + step;

        // Check if current index is greater than upper limit
        if (imageIndex >= Local.Images.Length)
            imageIndex = 0;

        // Check if current index is less than lower limit
        if (imageIndex < 0)
            imageIndex = Local.Images.Length - 1;


        // load image metadata
        if (!string.IsNullOrEmpty(filename))
        {
            photo = new IgPhoto(filename);
            directReadSettings.FirstFrameOnly = Config.SinglePageFormats.Contains(photo.Extension);

            Local.Metadata = Config.Codec.LoadMetadata(filename, directReadSettings);
        }
        else
        {
            Local.Metadata = Local.Images.GetMetadata(imageIndex);

            // Update current index
            Local.CurrentIndex = imageIndex;
        }

        directReadSettings.Metadata = Local.Metadata;


        Local.RaiseImageLoadingEvent(new()
        {
            CurrentIndex = Local.CurrentIndex,
            NewIndex = imageIndex,
            FilePath = filename,
        });

        #endregion


        try
        {
            // apply image list settings
            Local.Images.ReadOptions.IsApplyColorProfileForAll = Config.IsApplyColorProfileForAll;
            Local.Images.ReadOptions.ColorProfileName = Config.ColorProfile;
            Local.Images.ReadOptions.UseRawThumbnail = Config.IsUseRawThumbnail;
            Local.Images.SinglePageFormats = Config.SinglePageFormats;

            if (pageIndex != int.MinValue)
            {
                //UpdateActivePage();
            }
            else
            {
                // check if loading is cancelled
                token?.Token.ThrowIfCancellationRequested();

                // directly load the image file, skip image list
                if (photo != null)
                {
                    await photo.LoadAsync(Config.Codec, directReadSettings, token);
                }
                else
                {
                    photo = await Local.Images.GetAsync(
                        imageIndex,
                        useCache: !isSkipCache,
                        pageIndex: pageIndex,
                        tokenSrc: token
                    );
                }

                // check if loading is cancelled
                token?.Token.ThrowIfCancellationRequested();

                Local.RaiseImageLoadedEvent(new()
                {
                    Index = imageIndex,
                    Data = photo,
                    Error = photo?.Error,
                    KeepZoomRatio = isKeepZoomRatio,
                });
            }
        }
        catch (OperationCanceledException)
        {
            Local.Images.CancelLoading(imageIndex);
        }
        catch (Exception ex)
        {
            Local.RaiseImageLoadedEvent(new()
            {
                Index = imageIndex,
                Error = ex,
                KeepZoomRatio = isKeepZoomRatio,
            });
        }
    }


    /// <summary>
    /// View the next image using jump step
    /// </summary>
    /// <param name="step">The step to change image index. Use <c>0</c> to reload the viewing image.</param>
    /// <param name="isKeepZoomRatio"></param>
    /// <param name="isSkipCache"></param>
    /// <param name="pageIndex">Use <see cref="int.MinValue"/> to load the default page index.</param>
    /// <param name="filename"></param>
    public async Task ViewNextCancellableAsync(int step,
        bool isKeepZoomRatio = false,
        bool isSkipCache = false,
        int pageIndex = int.MinValue,
        string filename = "")
    {
        _loadCancelToken?.Cancel();
        _loadCancelToken = new();

        await ViewNextAsync(step, isKeepZoomRatio, isSkipCache, pageIndex, filename, _loadCancelToken);
    }


    /// <summary>
    /// View image using index
    /// </summary>
    /// <param name="index">Image index</param>
    private async void GoToImageAsync(int index)
    {
        _loadCancelToken?.Cancel();
        _loadCancelToken = new();

        Local.CurrentIndex = index;
        await ViewNextAsync(0, token: _loadCancelToken);
    }

    #endregion


    #region Local.Images event

    private void Local_OnImageLoading(ImageLoadingEventArgs e)
    {
        // Select thumbnail item
        _ = Helpers.RunAsThread(SelectCurrentThumbnail);

        UpdateImageInfo(BasicInfoUpdate.All, e.FilePath);
    }

    private void Local_OnImageLoaded(ImageLoadedEventArgs e)
    {
        // image error
        if (e.Error != null)
        {
            PicMain.SetImage(null);
            Local.ImageModifiedPath = "";

            var currentFile = Local.Images.GetFileName(e.Index);
            if (!string.IsNullOrEmpty(currentFile) && !File.Exists(currentFile))
            {
                Local.Images.Unload(e.Index);
            }

            PicMain.ShowMessage(Config.Language[$"{Name}.picMain._ErrorText"] + "\r\n" + e.Error.Source + ": " + e.Error.Message);
        }

        else if (e.Data?.ImgData.Image != null)
        {
            PicMain.SetImage(e.Data.ImgData.Image);

            // Reset the zoom mode if KeepZoomRatio = FALSE
            if (!e.KeepZoomRatio)
            {
                if (Config.IsWindowFit)
                {
                    //WindowFitMode();
                }
                else
                {
                    // reset zoom mode
                    PicMain.ZoomMode = Config.ZoomMode;
                }
            }

            PicMain.ClearMessage();
        }


        if (Local.CurrentIndex >= 0)
        {
            SelectCurrentThumbnail();
        }

        UpdateImageInfo(BasicInfoUpdate.Dimension | BasicInfoUpdate.FramesCount);

        // Collect system garbage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private void Local_OnImageListLoaded(ImageListLoadedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.FilePath))
        {
            UpdateCurrentIndex(e.FilePath);
        }

        UpdateImageInfo(BasicInfoUpdate.ListCount);

        // Load thumnbnail
        _ = Helpers.RunAsThread(LoadThumbnails);
    }

    #endregion


    #region PicMain events

    private void PicMain_DragOver(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var dataTest = e.Data.GetData(DataFormats.FileDrop, false);

            // observed: null w/ long path and long path support not enabled
            if (dataTest == null)
                return;

            var filePath = ((string[])dataTest)[0];

            // KBR 20190617 Fix observed issue: dragging from CD/DVD would fail because
            // we set the drag effect to Move, which is _not_allowed_
            // Drag file from DESKTOP to APP
            if (Local.Images.IndexOf(filePath) == -1
                && (e.AllowedEffect & DragDropEffects.Move) != 0)
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


    private async void PicMain_DragDrop(object sender, DragEventArgs e)
    {
        // Drag file from DESKTOP to APP
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        if (filePaths.Length > 1)
        {
            await PrepareLoadingAsync(filePaths);

            return;
        }

        var filePath = Helpers.ResolvePath(filePaths[0]);
        var imageIndex = Local.Images.IndexOf(filePath);

        // The file is located another folder, load the entire folder
        if (imageIndex == -1)
        {
            PrepareLoading(filePath);
        }
        // The file is in current folder AND it is the viewing image
        else if (Local.CurrentIndex == imageIndex)
        {
            //do nothing
        }
        // The file is in current folder AND it is NOT the viewing image
        else
        {
            Local.CurrentIndex = imageIndex;
            _ = ViewNextCancellableAsync(0);
        }
    }

    private void PicMain_Click(object sender, EventArgs e)
    {
        PicMain.Focus();
    }

    private void PicMain_OnNavLeftClicked(MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(-1);
    }

    private void PicMain_OnNavRightClicked(MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(1);
    }

    private void PicMain_OnZoomChanged(PhotoBox.ZoomEventArgs e)
    {
        UpdateImageInfo(BasicInfoUpdate.Zoom);
    }

    #endregion


    /// <summary>
    /// Update image info in status bar
    /// </summary>
    private void UpdateImageInfo(BasicInfoUpdate types = BasicInfoUpdate.All,
        string? filename = null)
    {
        if (InvokeRequired)
        {
            Invoke(UpdateImageInfo, types, filename);
            return;
        }

        FileInfo? fi = null;

        var fullPath = string.IsNullOrEmpty(filename)
            ? Local.Images.GetFileName(Local.CurrentIndex)
            : Helpers.ResolvePath(filename);

        var updateAll = BasicInfo.IsNull || types.HasFlag(BasicInfoUpdate.All);
        var isFileUpdate = updateAll && !string.IsNullOrEmpty(fullPath)
            || types.HasFlag(BasicInfoUpdate.FileSize)
            || types.HasFlag(BasicInfoUpdate.ModifiedDateTime);

        try
        {
            if (isFileUpdate)
            {
                fi = new FileInfo(fullPath);
            }
        }
        catch { }


        // AppName
        if (Config.InfoItems.Contains(nameof(BasicInfo.AppName)))
        {
            BasicInfo.AppName = Application.ProductName;
        }
        else
        {
            BasicInfo.AppName = string.Empty;
        }

        // ListCount
        if (updateAll || types.HasFlag(BasicInfoUpdate.ListCount))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.ListCount)))
            {
                if (Local.Images.Length == 0)
                {
                    BasicInfo.ListCount = "0/0 file(s)";
                }
                else
                {
                    BasicInfo.ListCount = $"{Local.CurrentIndex + 1}/{Local.Images.Length} file(s)";
                }
            }
            else
            {
                BasicInfo.ListCount = string.Empty;
            }
        }

        // Name
        if (updateAll || types.HasFlag(BasicInfoUpdate.Name))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.Name)))
            {
                BasicInfo.Name = Path.GetFileName(fullPath);
            }
            else
            {
                BasicInfo.Name = string.Empty;
            }
        }

        // Path
        if (updateAll || types.HasFlag(BasicInfoUpdate.Path))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.Path)))
            {
                BasicInfo.Path = fullPath;
            }
            else
            {
                BasicInfo.Path = string.Empty;
            }
        }

        // FileSize
        if (updateAll || types.HasFlag(BasicInfoUpdate.FileSize))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.FileSize))
                && fi != null)
            {
                BasicInfo.FileSize = Helpers.FormatSize(fi.Length);
            }
            else
            {
                BasicInfo.FileSize = string.Empty;
            }
        }

        // Dimension
        if (updateAll || types.HasFlag(BasicInfoUpdate.Dimension))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.Dimension))
                && Local.Metadata != null)
            {
                BasicInfo.Dimension = $"{Local.Metadata.Width} x {Local.Metadata.Height} px";
            }
            else
            {
                BasicInfo.Dimension = string.Empty;
            }
        }

        // Zoom
        if (updateAll || types.HasFlag(BasicInfoUpdate.Zoom))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.Zoom))
                && Local.Metadata != null)
            {
                BasicInfo.Zoom = $"{Math.Round(PicMain.ZoomFactor * 100, 2)}%";
            }
            else
            {
                BasicInfo.Zoom = string.Empty;
            }
        }

        // FramesCount
        if (updateAll || types.HasFlag(BasicInfoUpdate.FramesCount))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.FramesCount))
                && Local.Metadata != null
                && Local.Metadata.FramesCount > 1)
            {
                BasicInfo.FramesCount = $"{Local.Metadata.FramesCount} frames";
            }
            else
            {
                BasicInfo.FramesCount = string.Empty;
            }
        }

        // ModifiedDateTime
        if (updateAll || types.HasFlag(BasicInfoUpdate.ModifiedDateTime))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.ModifiedDateTime))
                && fi != null)
            {
                BasicInfo.ModifiedDateTime = Helpers.FormatDateTime(fi.LastWriteTime) + " (m)";
            }
            else
            {
                BasicInfo.ModifiedDateTime = string.Empty;
            }
        }

        // ExifRating
        if (updateAll || types.HasFlag(BasicInfoUpdate.ExifRating))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.ExifRating))
                && Local.Metadata != null)
            {
                var star = Helpers.FormatStarRating(Local.Metadata.ExifRatingPercent);
                BasicInfo.ExifRating = star > 0
                    ? "".PadRight(star, '⭐').PadRight(5, '-').Replace("-", " -")
                    : string.Empty;
            }
            else
            {
                BasicInfo.ExifRating = string.Empty;
            }
        }

        // ExifDateTime
        if (updateAll || types.HasFlag(BasicInfoUpdate.ExifDateTime))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.ExifDateTime))
                && Local.Metadata != null
                && Local.Metadata.ExifDateTime != null)
            {
                BasicInfo.ExifDateTime = Helpers.FormatDateTime(Local.Metadata.ExifDateTime) + " (e)";
            }
            else
            {
                BasicInfo.ExifDateTime = string.Empty;
            }
        }

        // ExifDateTimeOriginal
        if (updateAll || types.HasFlag(BasicInfoUpdate.ExifDateTimeOriginal))
        {
            if (Config.InfoItems.Contains(nameof(BasicInfo.ExifDateTimeOriginal))
                && Local.Metadata != null
                && Local.Metadata.ExifDateTimeOriginal != null)
            {
                BasicInfo.ExifDateTimeOriginal = Helpers.FormatDateTime(Local.Metadata.ExifDateTimeOriginal) + " (o)";
            }
            else
            {
                BasicInfo.ExifDateTimeOriginal = string.Empty;
            }
        }

        // DateTimeAuto
        if (updateAll || types.HasFlag(BasicInfoUpdate.DateTimeAuto))
        {
            var dtStr = string.Empty;

            if (Config.InfoItems.Contains(nameof(BasicInfo.DateTimeAuto)))
            {
                if (Local.Metadata != null)
                {
                    if (Local.Metadata.ExifDateTimeOriginal != null)
                    {
                        dtStr = Helpers.FormatDateTime(Local.Metadata.ExifDateTimeOriginal) + " (o)";
                    }
                    else if (Local.Metadata.ExifDateTime != null)
                    {
                        dtStr = Helpers.FormatDateTime(Local.Metadata.ExifDateTime) + " (e)";
                    }
                }

                if (fi != null && string.IsNullOrEmpty(dtStr))
                {
                    dtStr = Helpers.FormatDateTime(fi.LastWriteTime) + " (m)";
                }
            }

            BasicInfo.DateTimeAuto = dtStr;
        }


        Text = BasicInfo.ToString(Config.InfoItems);
        Application.DoEvents();
    }

}
