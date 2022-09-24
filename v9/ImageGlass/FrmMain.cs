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
using ImageGlass.Base.Actions;
using ImageGlass.Base.DirectoryComparer;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Gallery;
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Media.Media3D;
using WicNet;

namespace ImageGlass;

public partial class FrmMain : Form
{
    // cancellation tokens of synchronious task
    private CancellationTokenSource _loadCancelToken = new();

    private readonly MovableForm _movableForm;


    // variable to back up / restore window layout when changing window mode
    private bool _showToolbar = true;
    private bool _showThumbnails = true;
    private Rectangle _windowBound = new();
    private FormWindowState _windowState = FormWindowState.Normal;


    public FrmMain()
    {
        InitializeComponent();

        // hide the root layout to avoid flickering render,
        // and show again when window is loaded
        Tb0.Visible = false;

        _movableForm = new(this);

        SetUpFrmMainConfigs();

        // Get the DPI of the current display,
        // and load theme icons
        DpiApi.OnDpiChanged += OnDpiChanged;
        DpiApi.CurrentDpi = DeviceDpi;

        SetUpFrmMainTheme();
    }

    private void FrmMain_Load(object sender, EventArgs e)
    {
        Local.OnImageLoading += Local_OnImageLoading;
        Local.OnImageListLoaded += Local_OnImageListLoaded;
        Local.OnImageLoaded += Local_OnImageLoaded;
        Local.OnFirstImageReached += Local_OnFirstImageReached;
        Local.OnLastImageReached += Local_OnLastImageReached;

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


    private void FrmMain_KeyDown(object sender, KeyEventArgs e)
    {
        //Text = new Hotkey(e.KeyData).ToString() + " - " + e.KeyValue.ToString();

        var hotkey = new Hotkey(e.KeyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // open main menu
        if (actions.Contains(nameof(MnuMain)))
        {
            Toolbar.ShowMainMenu();
        }
        // zoom in/out
        else if (actions.Contains(nameof(MnuZoomIn))
            || actions.Contains(nameof(IG_ZoomIn))
            || actions.Contains(nameof(MnuZoomOut))
            || actions.Contains(nameof(IG_ZoomOut)))
        {
            PicMain_KeyDown(PicMain, e);
            return;
        }


        #region Register and run MAIN MENU shortcuts

        bool CheckMenuShortcut(ToolStripMenuItem mnu)
        {
            var menuHotkey = Config.GetHotkey(CurrentMenuHotkeys, mnu.Name);

            if (menuHotkey?.KeyData == e.KeyData)
            {
                // ignore invisible menu
                if (mnu.Visible)
                {
                    return false;
                }

                if (mnu.HasDropDownItems)
                {
                    ShowSubMenu(mnu);
                }
                else
                {
                    mnu.PerformClick();
                }

                return true;
            }

            foreach (var child in mnu.DropDownItems.OfType<ToolStripMenuItem>())
            {
                CheckMenuShortcut(child);
            }

            return false;
        }


        // register context menu shortcuts
        foreach (var item in MnuMain.Items.OfType<ToolStripMenuItem>())
        {
            if (CheckMenuShortcut(item))
            {
                return;
            }
        }
        #endregion

    }


    private void FrmMain_KeyUp(object sender, KeyEventArgs e)
    {
        var hotkey = new Hotkey(e.KeyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // zoom in/out
        if (actions.Contains(nameof(MnuZoomIn))
            || actions.Contains(nameof(IG_ZoomIn))
            || actions.Contains(nameof(MnuZoomOut))
            || actions.Contains(nameof(IG_ZoomOut)))
        {
            PicMain_KeyUp(PicMain, e);
            return;
        }
    }


    private void Gallery_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.Item.Index == Local.CurrentIndex)
        {
            PicMain.Refresh();
        }
        else
        {
            GoToImage(e.Item.Index);
        }
    }

    private void Gallery_ItemTooltipShowing(object sender, ItemTooltipShowingEventArgs e)
    {
        var langPath = $"{nameof(FrmMain)}.{nameof(Gallery)}.Tooltip";

        // build tooltip content
        var sb = new StringBuilder();
        sb.AppendLine(e.Item.FileName);
        sb.AppendLine($"{Config.Language[$"{langPath}._{nameof(IgMetadata.FileSize)}"]}: {e.Item.Details.FileSizeFormated}");
        sb.AppendLine($"{Config.Language[$"{langPath}._{nameof(IgMetadata.FileLastWriteTime)}"]}: {e.Item.Details.FileLastWriteTimeFormated}");
        var tooltipLinesCount = 4;

        if (e.Item.Details.FramesCount > 1)
        {
            sb.AppendLine($"{Config.Language[$"{langPath}._{nameof(IgMetadata.FramesCount)}"]}: {e.Item.Details.FramesCount}");
            tooltipLinesCount++;
        }

        var rating = BHelper.FormatStarRatingText(e.Item.Details.ExifRatingPercent);
        if (!string.IsNullOrEmpty(rating))
        {
            sb.AppendLine($"{Config.Language[$"{langPath}._Rating"]}: {rating}");
            tooltipLinesCount++;
        }

        e.TooltipContent = sb.ToString();
        e.TooltipTitle = e.Item.Text + $" ({e.Item.Details.OriginalWidth} x {e.Item.Details.OriginalWidth})";
    }


    private void Toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        var tagModel = e.ClickedItem.Tag as ToolbarItemTagModel;
        if (tagModel == null) return;

        // execute action
        ExecuteUserAction(tagModel.OnClick);
    }


    /// <summary>
    /// Handle DPI changes
    /// </summary>
    private void OnDpiChanged()
    {
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
    public void LoadImagesFromCmdArgs(string[] args)
    {
        var pathToLoad = "";

        if (args.Length >= 2)
        {
            // get path from params
            var cmdPath = args
                .Skip(1)
                .FirstOrDefault(i => !i.StartsWith(Constants.CONFIG_CMD_PREFIX));

            if (!string.IsNullOrEmpty(cmdPath))
            {
                pathToLoad = cmdPath;
            }
        }

        if (string.IsNullOrEmpty(pathToLoad)
            && Config.OpenLastSeenImage
            && File.Exists(Config.LastSeenImagePath))
        {
            pathToLoad = Config.LastSeenImagePath;
        }

        if (string.IsNullOrEmpty(pathToLoad)) return;


        // Start loading path
        _ = BHelper.RunAsThread(() => PrepareLoading(pathToLoad));
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
        var path = BHelper.ResolvePath(inputPath);

        if (string.IsNullOrEmpty(path)) return;

        if (BHelper.IsDirectory(path))
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
            filePath = paths.AsParallel().FirstOrDefault(i => !BHelper.IsDirectory(i));
            filePath = BHelper.ResolvePath(filePath);
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
            var paths = inputPaths.Select(item => BHelper.ResolvePath(item));

            // prepare the distinct dir list
            var distinctDirsList = BHelper.GetDistinctDirsFromPaths(paths);

            foreach (var aPath in distinctDirsList)
            {
                var dirPath = aPath;
                var isDir = false;

                try
                {
                    isDir = BHelper.IsDirectory(aPath);
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
            Local.InitImageList(sortedFilesList, distinctDirsList);

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
        if (Config.UseFileExplorerSortOrder)
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
            Config.EnableRecursiveLoading,
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
                if (!Config.IncludeHiddenImages)
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
        if (Config.GroupImagesByDirectory)
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
        if (Local.ActiveImageLoadingOrder == ImageOrderBy.FileSize)
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
        Gallery.ThumbnailSize = new Size(Config.ThumbnailSize, Config.ThumbnailSize);


        foreach (string filename in Local.Images.FileNames)
        {
            Gallery.Items.Add(filename);
        }

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
    private async Task ViewNextAsync(int step,
        bool isKeepZoomRatio = false,
        bool isSkipCache = false,
        int pageIndex = int.MinValue,
        string filename = "",
        CancellationTokenSource? token = null)
    {
        Local.CurrentChanges.Clear();

        if (Local.Images.Length == 0 && string.IsNullOrEmpty(filename))
        {
            Local.CurrentIndex = -1;
            Local.Metadata = null;
            
            UpdateImageInfo(ImageInfoUpdateTypes.All);

            return;
        }


        IgPhoto? photo = null;
        var readSettings = new CodecReadOptions()
        {
            ColorProfileName = Config.ColorProfile,
            ApplyColorProfileForAll = Config.ApplyColorProfileForAll,
            ImageChannel = Local.ImageChannel,
            UseRawThumbnail = Config.UseRawThumbnail,
        };


        #region Validate image index & load image metadata

        // temp index
        var imageIndex = Local.CurrentIndex + step;


        if (Local.Images.Length > 0)
        {
            // Reach end of list
            if (imageIndex >= Local.Images.Length)
            {
                Local.RaiseLastImageReachedEvent();

                if (!Config.EnableLoopBackNavigation)
                {
                    return;
                }
            }

            // Reach the first image of list
            if (imageIndex < 0)
            {
                Local.RaiseFirstImageReachedEvent();

                if (!Config.EnableLoopBackNavigation)
                {
                    return;
                }
            }
        }


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
            readSettings.FirstFrameOnly = Config.SinglePageFormats.Contains(photo.Extension);

            Local.Metadata = PhotoCodec.LoadMetadata(filename, readSettings);
        }
        else
        {
            Local.Metadata = Local.Images.GetMetadata(imageIndex);

            // Update current index
            Local.CurrentIndex = imageIndex;
        }


        Local.RaiseImageLoadingEvent(new()
        {
            CurrentIndex = Local.CurrentIndex,
            NewIndex = imageIndex,
            FilePath = string.IsNullOrEmpty(filename) ? Local.Images.GetFileName(Local.CurrentIndex) : filename,
        });

        #endregion


        try
        {
            // apply image list settings
            Local.Images.SinglePageFormats = Config.SinglePageFormats;
            Local.Images.ReadOptions = readSettings;


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
                    await photo.LoadAsync(readSettings, token);
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
        //catch (Exception ex)
        //{
        //    Local.RaiseImageLoadedEvent(new()
        //    {
        //        Index = imageIndex,
        //        Error = ex,
        //        KeepZoomRatio = isKeepZoomRatio,
        //    });
        //}
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
    private void GoToImage(int index)
    {
        Local.CurrentIndex = index;
        _ = ViewNextCancellableAsync(0);
    }

    #endregion


    #region Local.Images event

    private void Local_OnImageLoading(ImageLoadingEventArgs e)
    {
        Local.IsImageError = false;

        PicMain.ClearMessage();
        if (e.CurrentIndex >= 0 || !string.IsNullOrEmpty(e.FilePath))
        {
            PicMain.ShowMessage(Config.Language[$"{Name}._Loading"], "", delayMs: 1500);
        }

        // Select thumbnail item
        _ = BHelper.RunAsThread(SelectCurrentThumbnail);

        ShowImagePreview(e.FilePath);

        _ = Task.Run(() => UpdateImageInfo(ImageInfoUpdateTypes.All, e.FilePath));
    }


    /// <summary>
    /// Show image preview using the thumbnail
    /// </summary>
    private void ShowImagePreview(string filePath)
    {
        if (InvokeRequired)
        {
            Invoke(ShowImagePreview, filePath);
            return;
        }

        if (Local.Metadata == null || !Config.ShowImagePreview) return;

        WicBitmapSource? wicSrc = null;
        Size previewSize;

        // get preview size
        if (Config.ZoomMode == ZoomMode.LockZoom)
        {
            previewSize = new(Local.Metadata.Width, Local.Metadata.Height);
        }
        else
        {
            var zoomFactor = PicMain.CalculateZoomFactor(Config.ZoomMode, Local.Metadata.Width, Local.Metadata.Height);

            previewSize = new((int)(Local.Metadata.Width * zoomFactor), (int)(Local.Metadata.Height * zoomFactor));
        }

        // get embedded thumbnail for preview
        wicSrc = PhotoCodec.GetEmbeddedThumbnail(filePath);

        // use thumbnail image for preview
        if (wicSrc == null)
        {
            if (Local.CurrentIndex >= 0 && Local.CurrentIndex < Gallery.Items.Count)
            {
                var thumbnailPath = Gallery.Items[Local.CurrentIndex].FileName;
                var thumb = Gallery.Items[Local.CurrentIndex].ThumbnailImage;

                if (thumb != null
                    && thumbnailPath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    wicSrc = BHelper.ToWicBitmapSource(thumb);
                }
            }
        }


        if (wicSrc != null)
        {
            // scale the preview image
            if (wicSrc.Width < previewSize.Width || wicSrc.Height < previewSize.Height)
            {
                wicSrc.Scale(previewSize.Width, previewSize.Height, DirectN.WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor);
            }


            PicMain.SetImage(new()
            {
                Image = wicSrc,
                CanAnimate = false,
                FrameCount = 1,
            });
        }
    }

    private void Local_OnImageLoaded(ImageLoadedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(Local_OnImageLoaded, e);
            return;
        }

        // image error
        if (e.Error != null)
        {
            PicMain.SetImage(null);
            Local.IsImageError = true;
            Local.ImageModifiedPath = "";

            var currentFile = Local.Images.GetFileName(e.Index);
            if (!string.IsNullOrEmpty(currentFile) && !File.Exists(currentFile))
            {
                Local.Images.Unload(e.Index);
            }

            PicMain.ShowMessage(e.Error.Source + ": " + e.Error.Message,
                Config.Language[$"{Name}.{nameof(PicMain)}._ErrorText"]);
        }

        else if (!(e.Data?.ImgData.IsImageNull ?? true))
        {
            // delete clipboard image
            Local.ClipboardImage?.Dispose();
            Local.ClipboardImage = null;
            Local.TempImagePath = null;

            // set the main image
            PicMain.SetImage(e.Data.ImgData);

            // Reset the zoom mode if KeepZoomRatio = FALSE
            if (!e.KeepZoomRatio)
            {
                //TODO:
                //if (Config.IsWindowFit)
                //{
                //    //WindowFitMode();
                //}
                //else
                //{
                // reset zoom mode
                IG_SetZoomMode(Config.ZoomMode.ToString());
                //}
            }

            PicMain.ClearMessage();
        }


        if (Local.CurrentIndex >= 0)
        {
            SelectCurrentThumbnail();
        }

        UpdateImageInfo(ImageInfoUpdateTypes.Dimension | ImageInfoUpdateTypes.FramesCount);

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

        UpdateImageInfo(ImageInfoUpdateTypes.ListCount);

        // Load thumnbnail
        _ = BHelper.RunAsThread(LoadThumbnails);
    }

    private void Local_OnFirstImageReached()
    {
        if (!Config.EnableLoopBackNavigation)
        {
            PicMain.ShowMessage(Config.Language[$"{Name}._ReachedFirstImage"],
                Config.InAppMessageDuration);
        }
    }

    private void Local_OnLastImageReached()
    {
        if (!Config.EnableLoopBackNavigation)
        {
            PicMain.ShowMessage(Config.Language[$"{Name}._ReachedLastLast"],
                Config.InAppMessageDuration);
        }
    }

    #endregion


    /// <summary>
    /// Update image info in status bar
    /// </summary>
    private void UpdateImageInfo(ImageInfoUpdateTypes types = ImageInfoUpdateTypes.All,
        string? filename = null)
    {
        if (InvokeRequired)
        {
            Invoke(UpdateImageInfo, types, filename);
            return;
        }

        var updateAll = ImageInfo.IsNull || types.HasFlag(ImageInfoUpdateTypes.All);
        var clipboardImageText = string.Empty;


        // AppName
        if (Config.InfoItems.Contains(nameof(ImageInfo.AppName)))
        {
            ImageInfo.AppName = Application.ProductName;
        }
        else
        {
            ImageInfo.AppName = string.Empty;
        }

        // Zoom
        if (updateAll || types.HasFlag(ImageInfoUpdateTypes.Zoom))
        {
            if (Config.InfoItems.Contains(nameof(ImageInfo.Zoom)))
            {
                ImageInfo.Zoom = $"{Math.Round(PicMain.ZoomFactor * 100, 2)}%";
            }
            else
            {
                ImageInfo.Zoom = string.Empty;
            }
        }


        // the viewing image is a clipboard image
        if (Local.ClipboardImage != null)
        {
            clipboardImageText = Config.Language[$"{Name}._ClipboardImage"];

            // Dimension
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.Dimension))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.Dimension)))
                {
                    ImageInfo.Dimension = $"{Local.ClipboardImage.Width} x {Local.ClipboardImage.Height} px";
                }
                else
                {
                    ImageInfo.Dimension = string.Empty;
                }
            }
        }
        // the viewing image is from the image list
        else
        {
            var fullPath = string.IsNullOrEmpty(filename)
                ? Local.Images.GetFileName(Local.CurrentIndex)
                : BHelper.ResolvePath(filename);

            // ListCount
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ListCount))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.ListCount))
                    && Local.Images.Length > 0)
                {
                    ImageInfo.ListCount = $"{Local.CurrentIndex + 1}/{Local.Images.Length} {Config.Language[$"{Name}._Files"]}";

                }
                else
                {
                    ImageInfo.ListCount = string.Empty;
                }
            }

            // Name
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.Name))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.Name)))
                {
                    ImageInfo.Name = Path.GetFileName(fullPath);
                }
                else
                {
                    ImageInfo.Name = string.Empty;
                }
            }

            // Path
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.Path))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.Path)))
                {
                    ImageInfo.Path = fullPath;
                }
                else
                {
                    ImageInfo.Path = string.Empty;
                }
            }

            // FileSize
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.FileSize))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.FileSize))
                    && Local.Metadata != null)
                {
                    ImageInfo.FileSize = Local.Metadata.FileSizeFormated;
                }
                else
                {
                    ImageInfo.FileSize = string.Empty;
                }
            }

            // FramesCount
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.FramesCount))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.FramesCount))
                    && Local.Metadata != null
                    && Local.Metadata.FramesCount > 1)
                {
                    ImageInfo.FramesCount = $"{Local.Metadata.FramesCount} frames";
                }
                else
                {
                    ImageInfo.FramesCount = string.Empty;
                }
            }

            // Dimension
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.Dimension))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.Dimension))
                    && Local.Metadata != null)
                {
                    ImageInfo.Dimension = $"{Local.Metadata.OriginalWidth} x {Local.Metadata.OriginalHeight} px";
                }
                else
                {
                    ImageInfo.Dimension = string.Empty;
                }
            }

            // ModifiedDateTime
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ModifiedDateTime))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.ModifiedDateTime))
                    && Local.Metadata != null)
                {
                    ImageInfo.ModifiedDateTime = Local.Metadata.FileLastWriteTimeFormated + " (m)";
                }
                else
                {
                    ImageInfo.ModifiedDateTime = string.Empty;
                }
            }

            // ExifRating
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ExifRating))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.ExifRating))
                    && Local.Metadata != null)
                {
                    ImageInfo.ExifRating = BHelper.FormatStarRatingText(Local.Metadata.ExifRatingPercent);
                }
                else
                {
                    ImageInfo.ExifRating = string.Empty;
                }
            }

            // ExifDateTime
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ExifDateTime))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.ExifDateTime))
                    && Local.Metadata != null
                    && Local.Metadata.ExifDateTime != null)
                {
                    ImageInfo.ExifDateTime = BHelper.FormatDateTime(Local.Metadata.ExifDateTime) + " (e)";
                }
                else
                {
                    ImageInfo.ExifDateTime = string.Empty;
                }
            }

            // ExifDateTimeOriginal
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ExifDateTimeOriginal))
            {
                if (Config.InfoItems.Contains(nameof(ImageInfo.ExifDateTimeOriginal))
                    && Local.Metadata != null
                    && Local.Metadata.ExifDateTimeOriginal != null)
                {
                    ImageInfo.ExifDateTimeOriginal = BHelper.FormatDateTime(Local.Metadata.ExifDateTimeOriginal) + " (o)";
                }
                else
                {
                    ImageInfo.ExifDateTimeOriginal = string.Empty;
                }
            }

            // DateTimeAuto
            if (updateAll || types.HasFlag(ImageInfoUpdateTypes.DateTimeAuto))
            {
                var dtStr = string.Empty;

                if (Config.InfoItems.Contains(nameof(ImageInfo.DateTimeAuto))
                    && Local.Metadata != null)
                {
                    if (Local.Metadata.ExifDateTimeOriginal != null)
                    {
                        dtStr = BHelper.FormatDateTime(Local.Metadata.ExifDateTimeOriginal) + " (o)";
                    }
                    else if (Local.Metadata.ExifDateTime != null)
                    {
                        dtStr = BHelper.FormatDateTime(Local.Metadata.ExifDateTime) + " (e)";
                    }
                    else
                    {
                        dtStr = Local.Metadata.FileLastWriteTimeFormated + " (m)";
                    }
                }

                ImageInfo.DateTimeAuto = dtStr;
            }

        }


        Text = ImageInfo.ToString(Config.InfoItems, Local.ClipboardImage != null, clipboardImageText);
    }


    /// <summary>
    /// Executes user action
    /// </summary>
    private void ExecuteUserAction(SingleAction? ac)
    {
        if (ac == null) return;
        if (string.IsNullOrEmpty(ac.Executable)) return;

        var langPath = $"_._UserAction";
        Exception? error = null;


        // Executable is name of main menu item
        #region Main menu item executable
        if (ac.Executable.StartsWith("Mnu"))
        {
            var field = GetType().GetField(ac.Executable,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var mnu = field?.GetValue(this) as ToolStripMenuItem;

            if (mnu is not null)
            {
                mnu.PerformClick();
            }
            else
            {
                error = new MissingFieldException(string.Format(Config.Language[$"{langPath}._MenuNotFound"], ac.Executable));
            }
        }
        #endregion


        // Executable is a predefined function in FrmMain.IGMethods
        #region IGMethods executable
        else if (ac.Executable.StartsWith("IG_"))
        {
            // Find the private method in FrmMain
            var method = GetType().GetMethod(ac.Executable,
                BindingFlags.Instance | BindingFlags.NonPublic);


            // run built-in method
            if (method is not null)
            {
                // check method's params
                var paramItems = method.GetParameters();
                var paramters = new List<object>();

                if (paramItems.Length == 1)
                {
                    object? methodArg = null;
                    var type = Nullable.GetUnderlyingType(paramItems[0].ParameterType) ?? paramItems[0].ParameterType;

                    if (type.IsPrimitive || type.Equals(typeof(string)))
                    {
                        if (string.IsNullOrEmpty(ac.Argument.ToString()))
                        {
                            methodArg = null;
                        }
                        else
                        {
                            try
                            {
                                methodArg = Convert.ChangeType(ac.Argument, type);
                            }
                            catch (Exception ex) { error = ex; }
                        }
                    }
                    else
                    {
                        error = new ArgumentException(
                            string.Format(Config.Language[$"{langPath}._MethodArgumentNotSupported"], ac.Executable),
                            nameof(ac.Argument));
                    }


                    if (methodArg != null && methodArg.GetType().IsArray)
                    {
                        paramters.AddRange((object[])methodArg);
                    }
                    else
                    {
                        paramters.Add(methodArg);
                    }
                }


                // method must be bool/void()
                try
                {
                    method.Invoke(this, paramters.ToArray());
                }
                catch (Exception ex) { error = ex; }
            }
            else
            {
                error = new MissingMethodException(
                    string.Format(Config.Language[$"{langPath}._MethodNotFound"], ac.Executable));
            }
        }

        #endregion


        // Executable is a free path
        #region Free path executable
        else
        {
            var currentFilePath = Local.Images.GetFileName(Local.CurrentIndex);
            var procArgs = $"{ac.Argument}".Replace(Constants.FILE_MACRO, currentFilePath);

            // run external command line
            var proc = new Process
            {
                StartInfo = new(ac.Executable)
                {
                    Arguments = procArgs,
                    UseShellExecute = true,
                    ErrorDialog = true,
                },
            };

            try
            {
                proc.Start();
            }
            catch { }
        }

        #endregion


        // run next action
        if (error == null)
        {
            ExecuteUserAction(ac.NextAction);
        }
        // show error if any
        else
        {
            Config.ShowError(error.ToString(), Config.Language["_._Error"], error.Message);
        }
    }


    /// <summary>
    /// Executes action from mouse event
    /// </summary>
    private void ExecuteMouseAction(MouseClickEvent e)
    {
        if (Config.MouseClickActions.ContainsKey(e))
        {
            var toggleAction = Config.MouseClickActions[e];
            var isToggled = ToggleAction.IsToggled(toggleAction.Id);
            var action = isToggled
                ? toggleAction.ToggleOff
                : toggleAction.ToggleOn;

            var executable = action?.Executable.Trim();


            if (e == MouseClickEvent.RightClick)
            {
                // update PicMain's context menu
                Local.UpdateFrmMain(UpdateRequests.MouseActions);


                if (executable != nameof(IG_OpenContextMenu)
                    && executable != nameof(IG_OpenMainMenu))
                {
                    ExecuteUserAction(action);
                }
            }
            else
            {
                ExecuteUserAction(action);
            }

            // update toggling value
            ToggleAction.SetToggleValue(toggleAction.Id, !isToggled);
        }
    }



    #region Main Menu component

    /// <summary>
    /// Shows submenu items
    /// </summary>
    /// <param name="parentMenu"></param>
    private void ShowSubMenu(ToolStripMenuItem parentMenu)
    {
        MnuSubMenu.Items.Clear();

        foreach (ToolStripItem item in parentMenu.DropDownItems)
        {
            if (item.GetType() == typeof(ToolStripSeparator))
            {
                MnuSubMenu.Items.Add(new ToolStripSeparator());
            }
            else
            {
                MnuSubMenu.Items.Add(MenuUtils.Clone((ToolStripMenuItem)item));
            }
        }

        MnuSubMenu.Show(Cursor.Position);
    }

    private void MnuMain_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            // Alert user if there is a new version
            if (Config.IsNewVersionAvailable)
            {
                MnuCheckForUpdate.Text = MnuCheckForUpdate.Text = Config.Language[$"{Name}.{nameof(MnuCheckForUpdate)}._NewVersion"];
                MnuHelp.BackColor = MnuCheckForUpdate.BackColor = Color.FromArgb(35, 255, 165, 2);
            }
            else
            {
                MnuCheckForUpdate.Text = MnuCheckForUpdate.Text = Config.Language[$"{Name}.{nameof(MnuCheckForUpdate)}._NoUpdate"];
                MnuHelp.BackColor = MnuCheckForUpdate.BackColor = Color.Transparent;
            }

            MnuViewChannels.Enabled = true;
            MnuExtractFrames.Enabled =
                MnuStartStopAnimating.Enabled =
                MnuViewPreviousFrame.Enabled =
                MnuViewNextFrame.Enabled =
                MnuViewFirstFrame.Enabled =
                MnuViewLastFrame.Enabled = false;

            MnuSetLockScreen.Enabled = true;

            if (Local.Metadata?.FramesCount > 1)
            {
                MnuViewChannels.Enabled = false;

                MnuExtractFrames.Enabled =
                    MnuStartStopAnimating.Enabled =
                    MnuViewPreviousFrame.Enabled =
                    MnuViewNextFrame.Enabled =
                    MnuViewFirstFrame.Enabled =
                    MnuViewLastFrame.Enabled = true;
            }

            MnuExtractFrames.Text = string.Format(Config.Language[$"{Name}.{nameof(MnuExtractFrames)}"], Local.Metadata?.FramesCount);

            // check if igcmdWin10.exe exists!
            if (!BHelper.IsOS(WindowsOS.Win10OrLater)
                || !File.Exists(App.StartUpDir("igcmd10.exe")))
            {
                MnuSetLockScreen.Enabled = false;
            }

            if (BHelper.IsOS(WindowsOS.Win7))
            {
                MnuOpenWith.Enabled = false;
            }

            //// add hotkey to Exit menu
            //if (false) // Config.IsContinueRunningBackground)
            //{
            //    MnuExit.ShortcutKeyDisplayString = "Shift+ESC";
            //}
            //else
            //{
            //    MnuExit.ShortcutKeyDisplayString = Config.EnablePressESCToQuit ? "ESC" : "Alt+F4";
            //}

            //// Get EditApp for editing
            //UpdateEditAppInfoForMenu();
        }
        catch { }

    }


    private void MnuContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // clear current items
        MnuContext.Items.Clear();

        var hasClipboardImage = Local.ClipboardImage != null;
        var imageNotFound = !File.Exists(Local.Images.GetFileName(Local.CurrentIndex));

        //if (Config.IsSlideshow && !imageNotFound)
        //{
        //    MnuContext.Items.Add(MenuUtils.Clone(MnuPauseResumeSlideshow));
        //    MnuContext.Items.Add(MenuUtils.Clone(MnuExitSlideshow));
        //    MnuContext.Items.Add(new ToolStripSeparator());
        //}

        // toolbar menu
        MnuContext.Items.Add(MenuUtils.Clone(MnuToggleToolbar));
        MnuContext.Items.Add(MenuUtils.Clone(MnuToggleTopMost));

        if (!hasClipboardImage)
        {
            MnuContext.Items.Add(new ToolStripSeparator());
            MnuContext.Items.Add(MenuUtils.Clone(MnuLoadingOrders));
        }

        // Get Edit App info
        if (!imageNotFound)
        {
            if (!Local.IsImageError
                && !hasClipboardImage
                && Local.Metadata?.FramesCount <= 1)
            {
                MnuContext.Items.Add(MenuUtils.Clone(MnuViewChannels));
            }

            MnuContext.Items.Add(new ToolStripSeparator());
            if (!BHelper.IsOS(WindowsOS.Win7))
            {
                MnuContext.Items.Add(MenuUtils.Clone(MnuOpenWith));
            }

            //UpdateEditAppInfoForMenu();
            //MnuContext.Items.Add(MenuUtils.Clone(MnuEdit));

            //#region Check if image can animate (GIF)
            //try
            //{
            //    if (!Local.IsImageError && Local.Metadata?.FramesCount > 1)
            //    {
            //        var mnu1 = MenuUtils.Clone(MnuExtractFrames);
            //        mnu1.Text = string.Format(Config.Language[$"{Name}.{nameof(MnuExtractFrames)}"], Local.Metadata?.FramesCount);
            //        mnu1.Enabled = true;

            //        var mnu2 = MenuUtils.Clone(MnuStartStopAnimating);
            //        mnu2.Enabled = true;

            //        MnuContext.Items.Add(mnu1);
            //        MnuContext.Items.Add(mnu2);
            //    }
            //}
            //catch { }
            //#endregion
        }


        if ((!imageNotFound && !Local.IsImageError) || Local.ClipboardImage != null)
        {
            MnuContext.Items.Add(MenuUtils.Clone(MnuSetDesktopBackground));

            // check if igcmd10.exe exists!
            if (BHelper.IsOS(WindowsOS.Win10OrLater) && File.Exists(App.StartUpDir("igcmd10.exe")))
            {
                MnuContext.Items.Add(MenuUtils.Clone(MnuSetLockScreen));
            }
        }


        // Menu group: CLIPBOARD
        #region Menu group: CLIPBOARD
        MnuContext.Items.Add(new ToolStripSeparator());//------------

        MnuContext.Items.Add(MenuUtils.Clone(MnuPasteImage));
        MnuContext.Items.Add(MenuUtils.Clone(MnuCopyImageData));

        if (!imageNotFound && Local.ClipboardImage == null)
        {
            MnuContext.Items.Add(MenuUtils.Clone(MnuCopy));
            MnuContext.Items.Add(MenuUtils.Clone(MnuCut));
        }

        if (!imageNotFound && Local.ClipboardImage == null)
        {
            MnuContext.Items.Add(MenuUtils.Clone(MnuClearClipboard));
        }
        #endregion


        if (!imageNotFound && Local.ClipboardImage == null)
        {
            MnuContext.Items.Add(new ToolStripSeparator());//------------
            MnuContext.Items.Add(MenuUtils.Clone(MnuRename));
            MnuContext.Items.Add(MenuUtils.Clone(MnuMoveToRecycleBin));

            MnuContext.Items.Add(new ToolStripSeparator());//------------
            MnuContext.Items.Add(MenuUtils.Clone(MnuCopyPath));
            MnuContext.Items.Add(MenuUtils.Clone(MnuOpenLocation));
            MnuContext.Items.Add(MenuUtils.Clone(MnuImageProperties));
        }
    }



    // Menu File
    #region Menu File

    private void MnuOpenFile_Click(object sender, EventArgs e)
    {
        IG_OpenFile();
    }

    private void MnuNewWindow_Click(object sender, EventArgs e)
    {
        // TODO
    }

    private void MnuSave_Click(object sender, EventArgs e)
    {
        IG_Save();
    }

    private void MnuSaveAs_Click(object sender, EventArgs e)
    {
        IG_SaveAs();
    }

    private void MnuOpenWith_Click(object sender, EventArgs e)
    {
        IG_OpenWith();
    }

    private void MnuEdit_Click(object sender, EventArgs e)
    {
        // TODO
    }

    private void MnuPrint_Click(object sender, EventArgs e)
    {
        IG_Print();
    }

    private void MnuShare_Click(object sender, EventArgs e)
    {
        IG_Share();
    }

    private void MnuRefresh_Click(object sender, EventArgs e)
    {
        IG_Refresh();
    }

    private void MnuReload_Click(object sender, EventArgs e)
    {
        IG_Reload();
    }

    private void MnuReloadImageList_Click(object sender, EventArgs e)
    {
        IG_ReloadList();
    }

    #endregion


    // Menu Navigation
    #region Menu Navigation
    private void MnuViewNext_Click(object sender, EventArgs e)
    {
        IG_ViewNextImage();
    }

    private void MnuViewPrevious_Click(object sender, EventArgs e)
    {
        IG_ViewPreviousImage();
    }

    private void MnuGoTo_Click(object sender, EventArgs e)
    {
        IG_GoTo();
    }

    private void MnuGoToFirst_Click(object sender, EventArgs e)
    {
        IG_GoToFirst();
    }

    private void MnuGoToLast_Click(object sender, EventArgs e)
    {
        IG_GoToLast();
    }

    private void MnuViewNextFrame_Click(object sender, EventArgs e)
    {
        // TODO
    }

    private void MnuViewPreviousFrame_Click(object sender, EventArgs e)
    {
        // TODO
    }

    private void MnuViewFirstFrame_Click(object sender, EventArgs e)
    {
        // TODO
    }

    private void MnuViewLastFrame_Click(object sender, EventArgs e)
    {
        // TODO
    }



    #endregion


    // Menu Zoom
    #region Menu Zoom
    private void MnuZoomIn_Click(object sender, EventArgs e)
    {
        IG_ZoomIn();
    }

    private void MnuZoomOut_Click(object sender, EventArgs e)
    {
        IG_ZoomOut();
    }

    private void MnuCustomZoom_Click(object sender, EventArgs e)
    {
        IG_CustomZoom();
    }

    private void MnuActualSize_Click(object sender, EventArgs e)
    {
        IG_SetZoom(1f);
    }

    private void MnuAutoZoom_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.AutoZoom));
    }

    private void MnuLockZoom_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.LockZoom));
    }

    private void MnuScaleToWidth_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.ScaleToWidth));
    }

    private void MnuScaleToHeight_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.ScaleToHeight));
    }

    private void MnuScaleToFit_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.ScaleToFit));
    }

    private void MnuScaleToFill_Click(object sender, EventArgs e)
    {
        IG_SetZoomMode(nameof(ZoomMode.ScaleToFill));
    }



    #endregion


    // Menu Layout
    #region Menu Layout
    private void MnuToggleToolbar_Click(object sender, EventArgs e)
    {
        IG_ToggleToolbar();
    }

    private void MnuToggleThumbnails_Click(object sender, EventArgs e)
    {
        IG_ToggleGallery();
    }

    private void MnuToggleCheckerboard_Click(object sender, EventArgs e)
    {
        IG_ToggleCheckerboard();
    }

    private void MnuToggleTopMost_Click(object sender, EventArgs e)
    {
        IG_ToggleTopMost();
    }
    #endregion


    // Menu Image
    #region Menu Image

    private void MnuToggleImageFocus_Click(object sender, EventArgs e)
    {
        IG_ToggleImageFocus();
    }

    private void MnuRotateLeft_Click(object sender, EventArgs e)
    {

    }

    private void MnuRotateRight_Click(object sender, EventArgs e)
    {

    }

    private void MnuFlipHorizontal_Click(object sender, EventArgs e)
    {
        IG_FlipImage(FlipOptions.Horizontal);
    }

    private void MnuFlipVertical_Click(object sender, EventArgs e)
    {
        IG_FlipImage(FlipOptions.Vertical);
    }

    private void MnuRename_Click(object sender, EventArgs e)
    {
        IG_Rename();
    }

    private void MnuMoveToRecycleBin_Click(object sender, EventArgs e)
    {
        IG_Delete(true);
    }

    private void MnuDeleteFromHardDisk_Click(object sender, EventArgs e)
    {
        IG_Delete(false);
    }

    private void MnuStartStopAnimating_Click(object sender, EventArgs e)
    {

    }

    private void MnuExtractFrames_Click(object sender, EventArgs e)
    {

    }

    private void MnuSetDesktopBackground_Click(object sender, EventArgs e)
    {
        IG_SetDesktopBackground();
    }

    private void MnuSetLockScreen_Click(object sender, EventArgs e)
    {
        IG_SetLockScreenBackground();
    }

    private void MnuOpenLocation_Click(object sender, EventArgs e)
    {
        IG_OpenLocation();
    }

    private void MnuImageProperties_Click(object sender, EventArgs e)
    {
        IG_OpenProperties();
    }


    #endregion


    // Window modes menu
    #region Window modes menu
    private void MnuWindowFit_Click(object sender, EventArgs e)
    {

    }

    private void MnuFrameless_Click(object sender, EventArgs e)
    {

    }

    private void MnuFullScreen_Click(object sender, EventArgs e)
    {
        IG_ToggleFullScreen();
    }
    #endregion


    // Menu Slideshow
    #region Menu Slideshow
    private void MnuStartSlideshow_Click(object sender, EventArgs e)
    {

    }

    private void MnuPauseResumeSlideshow_Click(object sender, EventArgs e)
    {

    }

    private void MnuExitSlideshow_Click(object sender, EventArgs e)
    {

    }
    #endregion


    // Menu Clipboard
    #region Menu Clipboard

    private void MnuPasteImage_Click(object sender, EventArgs e)
    {
        IG_PasteImage();
    }

    private void MnuCopyImageData_Click(object sender, EventArgs e)
    {
        IG_CopyImageData();
    }

    private void MnuCopy_Click(object sender, EventArgs e)
    {
        IG_CopyMultiFiles();
    }

    private void MnuCut_Click(object sender, EventArgs e)
    {
        IG_CutMultiFiles();
    }

    private void MnuCopyPath_Click(object sender, EventArgs e)
    {
        IG_CopyImagePath();
    }

    private void MnuClearClipboard_Click(object sender, EventArgs e)
    {
        IG_ClearClipboard();
    }


    #endregion


    // Menu Tools
    #region Menu Tools

    private void MnuColorPicker_Click(object sender, EventArgs e)
    {

    }

    private void MnuCropping_Click(object sender, EventArgs e)
    {

    }

    private void MnuPageNav_Click(object sender, EventArgs e)
    {

    }

    private void MnuExifTool_Click(object sender, EventArgs e)
    {

    }



    #endregion


    // Menu Help
    #region Menu Help
    private void MnuAbout_Click(object sender, EventArgs e)
    {
        IG_About();
    }

    private void MnuCheckForUpdate_Click(object sender, EventArgs e)
    {
        IG_CheckForUpdate(true);
    }

    private void MnuReportIssue_Click(object sender, EventArgs e)
    {
        IG_ReportIssue();
    }

    private void MnuFirstLaunch_Click(object sender, EventArgs e)
    {

    }

    private void MnuSetDefaultPhotoViewer_Click(object sender, EventArgs e)
    {
        IG_SetDefaultPhotoViewer();
    }

    private void MnuUnsetDefaultPhotoViewer_Click(object sender, EventArgs e)
    {
        IG_UnsetDefaultPhotoViewer();
    }

    #endregion


    // Others
    #region Other menu
    private void MnuSettings_Click(object sender, EventArgs e)
    {
        IG_Settings();
    }

    private void MnuExit_Click(object sender, EventArgs e)
    {
        IG_Exit();
    }









    #endregion

    #endregion

}
