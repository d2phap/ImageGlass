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
    private CancellationTokenSource _busyCancelToken = new();


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
        // Load image from param
        LoadImagesFromCmdArgs(Environment.GetCommandLineArgs());
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
            _ = PrepareLoadingAsync(o.FileNames, o.FileNames[0]);
        }
    }


    /// <summary>
    /// Load images from command line arguments (<see cref="Environment.GetCommandLineArgs"/>)
    /// </summary>
    /// <param name="args"></param>
    public void LoadImagesFromCmdArgs(string[] args)
    {
        // Load image from param
        if (args.Length >= 2)
        {
            for (var i = 1; i < args.Length; i++)
            {
                // only read the path, exclude configs parameter which starts with "-"
                if (!args[i].StartsWith(Constants.CONFIG_CMD_PREFIX))
                {
                    PrepareLoading(args[i]);
                    break;
                }
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
        var path = App.ToAbsolutePath(inputPath);
        var currentFileName = File.Exists(path) ? path : "";

        // Start loading path
        _ = PrepareLoadingAsync(new string[] { inputPath }, currentFileName);
    }


    /// <summary>
    /// Prepare to load images
    /// </summary>
    /// <param name="inputPaths">Paths of image files or folders.
    /// It can be relative/absolute paths or URI scheme.</param>
    /// <param name="currentFileName">Current viewing filename.</param>
    private async Task PrepareLoadingAsync(IEnumerable<string> inputPaths, string currentFileName = "")
    {
        if (!inputPaths.Any()) return;

        var allFilesToLoad = new HashSet<string>();
        var currentFile = currentFileName;
        var hasInitFile = !string.IsNullOrEmpty(currentFile);

        // Dispose all garbage
        Local.Images.Dispose();
        Local.Images = new(Config.Codec)
        {
            MaxQueue = Config.ImageBoosterCachedCount,
            ImageChannel = Local.ImageChannel,
        };

        // Display currentFile while loading the full directory
        if (hasInitFile)
        {
            _ = NextImageCancellableAsync(0, filename: currentFile);
        }

        // Parse string to absolute path
        var paths = inputPaths.Select(item => App.ToAbsolutePath(item));

        // prepare the distinct dir list
        var distinctDirsList = Helpers.GetDistinctDirsFromPaths(paths);

        // track paths loaded to prevent duplicates
        var pathsLoaded = new HashSet<string>();
        var sortedFilesList = new List<string>();
        var firstPath = true;


        // Async load, filter and sort files in directories
        await Task.Run(() =>
        {
            foreach (var aPath in distinctDirsList)
            {
                var dirPath = aPath;
                if (File.Exists(aPath))
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
                else if (Directory.Exists(aPath))
                {
                    // Issue #415: If the folder name ends in ALT+255 (alternate space),
                    // DirectoryInfo strips it. By ensuring a terminating slash,
                    // the problem disappears. By doing that *here*, the uses of
                    // DirectoryInfo in DirectoryFinder and FileWatcherEx are fixed as well.
                    // https://stackoverflow.com/questions/5368054/getdirectories-fails-to-enumerate-subfolders-of-a-folder-with-255-name
                    if (!aPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        dirPath = aPath + Path.DirectorySeparatorChar;
                    }
                }
                else
                {
                    continue;
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
    private void LoadImages(
        List<string> imageFilenameList,
        string filePath,
        bool skipLoadingImage = false)
    {
        // Set filename to image list
        Local.Images.Clear();
        Local.Images.Add(imageFilenameList);

        // Find the index of current image
        if (filePath.Length > 0)
        {
            // this part of code fixes calls on legacy 8.3 filenames
            // (for example opening files from IBM Notes)
            var di = new DirectoryInfo(filePath);
            filePath = di.FullName;

            Local.CurrentIndex = Local.Images.IndexOf(filePath);

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
                && !Local.Images.ContainsDirPathOf(filePath))
            {
                Local.CurrentIndex = 0;
            }
        }
        else
        {
            Local.CurrentIndex = 0;
        }

        
        // Load thumnbnail
        LoadThumbnails();


        if (!skipLoadingImage)
        {
            // Start loading image
            _ = NextImageCancellableAsync(0);
        }

        // TODO:
        //SetStatusBar();
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
    private static List<string> SortImageList(IEnumerable<string> fileList)
    {
        // NOTE: relies on LocalSetting.ActiveImageLoadingOrder been updated first!
        var list = new List<string>();

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
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.Name)
        {
            list.AddRange(fileList
                .OrderBy(f => f, directorySortComparer)
                .ThenBy(f => f, naturalSortComparer));
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Length)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Length)
                    .ThenBy(f => f, naturalSortComparer));
            }
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.CreationTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).CreationTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Extension)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).Extension)
                    .ThenBy(f => f, naturalSortComparer));
            }
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastAccessTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).LastAccessTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.LastWriteTime)
        {
            if (Local.ActiveImageLoadingOrderType == ImageOrderType.Desc)
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
            else
            {
                list.AddRange(fileList
                    .OrderBy(f => f, directorySortComparer)
                    .ThenBy(f => new FileInfo(f).LastWriteTimeUtc)
                    .ThenBy(f => f, naturalSortComparer));
            }
        }
        else if (Local.ActiveImageLoadingOrder == ImageOrderBy.Random)
        {
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
    private void LoadThumbnails()
    {
        Gallery.SuspendLayout();
        Gallery.Items.Clear();
        Gallery.ThumbnailSize = new Size(Config.ThumbnailDimension, Config.ThumbnailDimension);

        for (var i = 0; i < Local.Images.Length; i++)
        {
            var lvi = new ImageGalleryItem(Local.Images.GetFileName(i));

            Gallery.Items.Add(lvi);
        }

        Gallery.ResumeLayout();

        SelectCurrentThumbnail();
    }


    /// <summary>
    /// Select current thumbnail
    /// </summary>
    private void SelectCurrentThumbnail()
    {
        if (Gallery.Items.Count > 0)
        {
            Gallery.ClearSelection();

            try
            {
                Gallery.Items[Local.CurrentIndex].Selected = true;
                Gallery.Items[Local.CurrentIndex].Focused = true;
                Gallery.ScrollToIndex(Local.CurrentIndex);
            }
            catch { }
        }
    }


    /// <summary>
    /// View the next image using jump step.
    /// </summary>
    private async Task ViewNextAsync(int step,
        bool isKeepZoomRatio = false,
        bool isSkipCache = false,
        int pageIndex = int.MinValue,
        string filename = "",
        CancellationTokenSource? token = null)
    {
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

        #region Validate image index

        // temp index
        var imageIndex = Local.CurrentIndex + step;

        // Issue #1019:
        // When showing the initial image, the ImageList is empty; don't show toast messages
        if (!Config.IsSlideshow && !Config.IsLoopBackViewer && Local.Images.Length > 0)
        {
            // Reach end of list
            if (imageIndex >= Local.Images.Length)
            {
                PicBox.ShowMessage(Config.Language[$"{Name}._LastItemOfList"], 1500);
                return;
            }

            // Reach the first item of list
            if (imageIndex < 0)
            {
                PicBox.ShowMessage(Config.Language[$"{Name}._FirstItemOfList"], 1500);
                return;
            }
        }

        // Check if current index is greater than upper limit
        if (imageIndex >= Local.Images.Length)
            imageIndex = 0;

        // Check if current index is less than lower limit
        if (imageIndex < 0)
            imageIndex = Local.Images.Length - 1;

        // Update current index
        Local.CurrentIndex = imageIndex;

        #endregion


        // Select thumbnail item
        SelectCurrentThumbnail();

        PicBox.ShowMessage("Loading image... \n" + filename, 0, 1500);

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
                IgPhoto? photo;

                // check if loading is cancelled
                token?.Token.ThrowIfCancellationRequested();

                // directly load the image file, skip image list
                if (filename.Length > 0)
                {
                    photo = new IgPhoto(filename);
                    await photo.LoadAsync(Config.Codec, new()
                    {
                        ColorProfileName = Config.ColorProfile,
                        IsApplyColorProfileForAll = Config.IsApplyColorProfileForAll,
                        ImageChannel = Local.ImageChannel,
                        UseRawThumbnail = Local.Images.ReadOptions.UseRawThumbnail,
                        //UseEmbeddedThumbnail = Local.Images.
                        FirstFrameOnly = Config.SinglePageFormats.Contains(photo.Extension)
                    }, token);
                }
                else
                {
                    photo = await Local.Images.GetAsync(
                        imageIndex,
                        useCache: !isSkipCache,
                        pageIndex: pageIndex,
                        tokenSrc: token
                    ).ConfigureAwait(true);
                }

                Local.ImageError = photo?.Error;

                if (photo?.Image != null)
                {
                    // check if loading is cancelled
                    token?.Token.ThrowIfCancellationRequested();

                    PicBox.SetImage(photo.Image);

                    // Reset the zoom mode if isKeepZoomRatio = FALSE
                    if (!isKeepZoomRatio)
                    {
                        if (Config.IsWindowFit)
                        {
                            //WindowFitMode();
                        }
                        else
                        {
                            // reset zoom mode
                            PicBox.ZoomMode = Config.ZoomMode;
                        }
                    }

                    PicBox.ClearMessage();
                }
            }
        }
        catch (OperationCanceledException)
        {
            Local.Images.CancelLoading(imageIndex);
        }
        catch (Exception ex)
        {
            Local.ImageError = ex;
        }


        // image error
        if (Local.ImageError != null)
        {
            PicBox.SetImage(null);
            Local.ImageModifiedPath = "";

            var currentFile = Local.Images.GetFileName(imageIndex);
            if (!string.IsNullOrEmpty(currentFile) && !File.Exists(currentFile))
            {
                Local.Images.Unload(imageIndex);
            }

            PicBox.ShowMessage(Config.Language[$"{Name}.picMain._ErrorText"] + "\r\n" + Local.ImageError.Source + ": " + Local.ImageError.Message);
        }


        // Collect system garbage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }


    /// <summary>
    /// View the next image using jump step
    /// </summary>
    /// <param name="step">The step to change image index. Use <c>0</c> to reload the viewing image.</param>
    /// <param name="isKeepZoomRatio"></param>
    /// <param name="isSkipCache"></param>
    /// <param name="pageIndex">Use <see cref="int.MinValue"/> to load the default page index.</param>
    /// <param name="filename"></param>
    public async Task NextImageCancellableAsync(int step,
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


    private void Gallery_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.Item.Index == Local.CurrentIndex)
        {
            PicBox.Refresh();
        }
        else
        {
            GoToImageAsync(e.Item.Index);
        }


        //e.Item.FetchItemDetails();
    }



    private void FrmMain_Resize(object sender, EventArgs e)
    {
        Text = $"{PicBox.Width}x{PicBox.Height}";
    }


    private void PicBox_OnImageChanged(EventArgs e)
    {
        PicBox.ClearMessage();
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


        // TODO: test file macro <file>
        var currentFilename = @"E:\WALLPAPER\NEW\dark2\horizon_by_t1na_den4yvj-fullview.jpg";


        // run external command line
        var proc = new Process
        {
            StartInfo = new(tagModel.OnClick.Executable)
            {
                Arguments = tagModel.OnClick.Arguments.Replace(Constants.FILE_MACRO, currentFilename),
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


    private void PicBox_Click(object sender, EventArgs e)
    {
        PicBox.Focus();
    }

    private void PicBox_DragOver(object sender, DragEventArgs e)
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

            // KBR 20190617 Fix observed issue: dragging from CD/DVD would fail because we set the
            // drag effect to Move, which is _not_allowed_
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

    private void PicBox_DragDrop(object sender, DragEventArgs e)
    {
        // Drag file from DESKTOP to APP
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var filepaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        if (filepaths.Length > 1)
        {
            _ = PrepareLoadingAsync(filepaths, Local.Images.GetFileName(Local.CurrentIndex));
            return;
        }

        var filePath = filepaths[0];

        if (string.Equals(Path.GetExtension(filePath), ".lnk", StringComparison.CurrentCultureIgnoreCase))
        {
            filePath = FileShortcutApi.GetTargetPathFromShortcut(filePath);
        }

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
            _ = NextImageCancellableAsync(0);
        }
    }

    private void PicBox_OnNavLeftClicked(MouseEventArgs e)
    {
        _ = NextImageCancellableAsync(-1);
    }

    private void PicBox_OnNavRightClicked(MouseEventArgs e)
    {
        _ = NextImageCancellableAsync(1);
    }

    private void PicBox_OnZoomChanged(PhotoBox.ZoomEventArgs e)
    {
        Text = Math.Round(e.ZoomFactor * 100, 2).ToString() + "%";
    }
}
