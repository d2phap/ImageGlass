using D2Phap;
using ImageGlass.Base;
using ImageGlass.Base.DirectoryComparer;
using ImageGlass.Base.NamedPipes;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.Views;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO.Pipes;
using System.Linq;
using System.Windows.Forms;
using WicNet;
using Timer = System.Windows.Forms.Timer;

namespace igcmd.Slideshow;

public partial class FrmSlideshow : Form
{
    private PipeClient _client;
    private string _serverName;
    private string _initImagePath;

    private CancellationTokenSource _loadImageCancelToken = new();
    private List<string> _imageList = new();
    private int _currentIndex = -1;
    private IgMetadata? _currentMetadata = null;

    private Timer _slideshowTimer = new() { Enabled = false };
    private Stopwatch _slideshowStopwatch = new(); // slideshow stopwatch
    private float _slideshowCountdown = 5; // slideshow countdown interval


    /// <summary>
    /// Hotkeys list of main menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> CurrentMenuHotkeys = new()
    {
        // Open context menu
	    { nameof(MnuContext),               new() { new(Keys.Alt | Keys.F) } },

        { nameof(MnuPauseResumeSlideshow),  new() { new(Keys.Space) } },
        { nameof(MnuExitSlideshow),         new() { new(Keys.Escape) } },
        { nameof(MnuShowMainWindow),        new() { new(Keys.F12) } },

        // MnuNavigation
        { nameof(MnuViewNext),              new() { new (Keys.Right) } },
        { nameof(MnuViewPrevious),          new() { new (Keys.Left) } },
        { nameof(MnuGoToFirst),             new() { new (Keys.Home) } },
        { nameof(MnuGoToLast),              new() { new (Keys.End) } },

        { nameof(MnuFullScreen),            new() { new (Keys.F11) } },
        { nameof(MnuToggleCheckerboard),    new() { new (Keys.B) } },
        
        { nameof(MnuActualSize),            new() { new (Keys.D0), new (Keys.NumPad0) } },
        { nameof(MnuAutoZoom),              new() { new (Keys.D1), new (Keys.NumPad1) } },
        { nameof(MnuLockZoom),              new() { new (Keys.D2), new (Keys.NumPad2) } },
        { nameof(MnuScaleToWidth),          new() { new (Keys.D3), new (Keys.NumPad3) } },
        { nameof(MnuScaleToHeight),         new() { new (Keys.D4), new (Keys.NumPad4) } },
        { nameof(MnuScaleToFit),            new() { new (Keys.D5), new (Keys.NumPad5) } },
        { nameof(MnuScaleToFill),           new() { new (Keys.D6), new (Keys.NumPad6) } },

        { nameof(MnuOpenWith),              new() { new (Keys.D) } },
        { nameof(MnuOpenLocation),          new() { new (Keys.L) } },
        { nameof(MnuCopyPath),              new() { new (Keys.Control | Keys.L) } },
    };


    public FrmSlideshow(string slideshowIndex, string initImagePath)
    {
        InitializeComponent();

        Config.Load();

        _serverName = $"{Constants.SLIDESHOW_PIPE_PREFIX}{slideshowIndex}"; ;
        _initImagePath = initImagePath;

        // Get the DPI of the current display,
        // and load theme icons
        DpiApi.CurrentDpi = DeviceDpi;

        // load configs
        _ = int.TryParse(slideshowIndex, out var indexNumber);
        Text = $"{Config.Language["FrmMain.MnuSlideshow"]} {indexNumber + 1} - {App.AppName}";

        PicMain.InterpolationScaleDown = Config.ImageInterpolationScaleDown;
        PicMain.InterpolationScaleUp = Config.ImageInterpolationScaleUp;


        LoadTheme();
    }


    private void FrmSlideshow_Load(object sender, EventArgs e)
    {
        _slideshowTimer.Interval = 10; // support milliseconds
        _slideshowTimer.Tick += SlideshowTimer_Tick;

        PicMain.Render += PicMain_Render;


        // load the init image
        _ = BHelper.RunAsThread(() => _ = LoadImageAsync(_initImagePath, _loadImageCancelToken));

        _client = new PipeClient(_serverName, PipeDirection.InOut);
        _client.MessageReceived += Client_MessageReceived;
        _client.Disconnected += (_, _) => Application.Exit();
        _ = _client.ConnectAsync();

        // load menu hotkeys
        Config.MergeHotkeys(ref CurrentMenuHotkeys, Config.MenuHotkeys);
        LoadMenuHotkeys();

        // load language
        LoadLanguage();
    }

    private void FrmSlideshow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _client.Dispose();
    }

    private void FrmSlideshow_KeyDown(object sender, KeyEventArgs e)
    {
        #region Register and run CONTEXT MENU shortcuts

        bool CheckMenuShortcut(ToolStripMenuItem mnu)
        {
            var menuHotkeyList = Config.GetHotkey(CurrentMenuHotkeys, mnu.Name);
            var menuHotkey = menuHotkeyList.SingleOrDefault(k => k.KeyData == e.KeyData);

            if (menuHotkey != null)
            {
                // ignore invisible menu
                if (mnu.Visible)
                {
                    return false;
                }

                mnu.PerformClick();

                return true;
            }

            foreach (var child in mnu.DropDownItems.OfType<ToolStripMenuItem>())
            {
                CheckMenuShortcut(child);
            }

            return false;
        }


        // register context menu shortcuts
        foreach (var item in MnuContext.Items.OfType<ToolStripMenuItem>())
        {
            if (CheckMenuShortcut(item))
            {
                return;
            }
        }
        #endregion
    }

    private void FrmSlideshow_MouseDown(object sender, MouseEventArgs e)
    {
        
    }


    private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Message)) return;

        if (e.Message == Constants.SLIDESHOW_PIPE_CMD_TERMINATE)
        {
            Application.Exit();
            return;
        }

        var firstEqualCharPosition = e.Message.IndexOf("=");
        if (firstEqualCharPosition == -1) return;

        var cmd = e.Message.Substring(0, firstEqualCharPosition);
        if (string.IsNullOrEmpty(cmd)) return;

        var arg = e.Message.Substring(++firstEqualCharPosition);
        if (string.IsNullOrEmpty(arg)) return;
        

        // update image list
        if (cmd.Equals(SlideshowPipeCommands.SET_IMAGE_LIST, StringComparison.InvariantCultureIgnoreCase))
        {
            var list = BHelper.ParseJson<List<string>>(arg);

            if (list != null && list.Count > 0)
            {
                _ = BHelper.RunAsThread(() => LoadImageList(list, _initImagePath));
            }

            return;
        }


        // update language
        if (cmd.Equals(SlideshowPipeCommands.SET_LANGUAGE, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Language = new IgLang(arg, App.StartUpDir(Dir.Languages));
            LoadLanguage();
            return;
        }


        // update theme
        if (cmd.Equals(SlideshowPipeCommands.SET_THEME, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Theme = new IgTheme(arg, Config.ToolbarIconHeight);
            LoadTheme();
            return;
        }
    }


    private void SlideshowTimer_Tick(object? sender, EventArgs e)
    {
        if (!_slideshowStopwatch.IsRunning)
            _slideshowStopwatch.Restart();

        if (_slideshowStopwatch.Elapsed.TotalMilliseconds >= TimeSpan.FromSeconds(_slideshowCountdown).TotalMilliseconds)
        {
            // end of image list
            if (_currentIndex == _imageList.Count - 1)
            {
                // loop the list
                if (!Config.LoopSlideshow)
                {
                    // pause slideshow
                    SetSlideshowState(false, false);
                    return;
                }
            }

            _ = ViewNextImageAsync(1);
        }


        // only update the countdown text if it's a full second number
        var isSecond = _slideshowStopwatch.Elapsed.Milliseconds <= 100;
        if (isSecond)
        {
            PicMain.Invalidate();
        }
        
    }


    private void PicMain_Render(object? sender, RenderEventArgs e)
    {
        if (!_slideshowTimer.Enabled || !Config.ShowSlideshowCountdown) return;

        // draw countdown text ----------------------------------------------
        var countdownTime = TimeSpan.FromSeconds(_slideshowCountdown + 1);
        var text = (countdownTime - _slideshowStopwatch.Elapsed).ToString("mm':'ss");

        
        var font = new Font(Font.FontFamily, 30f);
        var fontSize = e.Graphics.MeasureText(text, font.Name, font.Size, textDpi: DeviceDpi);

        // calculate background size
        var gapX = fontSize.Width / 4;
        var gapY = fontSize.Height / 4;
        var padding = DpiApi.Transform(30);
        var bgSize = new SizeF(fontSize.Width + gapX, fontSize.Height + gapY);
        var bgX = PicMain.Width - bgSize.Width - padding;
        var bgY = PicMain.Height - bgSize.Height - padding;

        // draw background
        var borderRadius = BHelper.IsOS(WindowsOS.Win11) ? 10 : 1;
        var bgColor = Color.FromArgb(150, PicMain.BackColor);
        var bgRect = new RectangleF(bgX, bgY, bgSize.Width, bgSize.Height);

        e.Graphics.DrawRectangle(bgRect, borderRadius, bgColor, bgColor);


        // calculate text position
        var fontX = bgX + (bgSize.Width / 2) - (fontSize.Width / 2);
        var fontY = bgY + (bgSize.Height / 2) - (fontSize.Height / 2);

        // draw text
        var textColor = Color.FromArgb(150, ThemeUtils.InvertBlackAndWhiteColor(PicMain.BackColor));

        e.Graphics.DrawText(text, font.Name, font.Size, fontX, fontY, textColor, textDpi: DeviceDpi);
    }


    // Load image
    #region Load image

    private void LoadImageList(IEnumerable<string> fileList, string? initFilePath = null)
    {
        _imageList = BHelper.SortImageList(fileList,
            Config.ImageLoadingOrder,
            Config.ImageLoadingOrderType, 
            Config.GroupImagesByDirectory).ToList();

        if (string.IsNullOrEmpty(initFilePath))
        {
            _currentIndex = 0;
            return;
        }

        // this part of code fixes calls on legacy 8.3 filenames
        // (for example opening files from IBM Notes)
        var di = new DirectoryInfo(initFilePath);
        initFilePath = di.FullName;

        // Find the index of current image
        _currentIndex = _imageList.IndexOf(initFilePath);


        // enable slideshow
        SetSlideshowState(true, false);
    }


    private async Task ViewNextImageAsync(int step = 0)
    {
        _loadImageCancelToken?.Cancel();
        _loadImageCancelToken = new();


        // Issue #609: do not auto-reactivate slideshow if disabled
        if (_slideshowTimer.Enabled)
        {
            _slideshowTimer.Enabled = false;
            _slideshowTimer.Enabled = true;
            _slideshowStopwatch.Reset();
        }


        // Validate image index
        #region Validate image index

        // temp index
        var imageIndex = _currentIndex + step;


        if (_imageList.Count > 0)
        {
            // Reach end of list
            if (imageIndex >= _imageList.Count)
            {
                if (!Config.EnableLoopBackNavigation)
                {
                    PicMain.ShowMessage(Config.Language[$"{Name}._ReachedFirstImage"],
                        Config.InAppMessageDuration);

                    return;
                }
            }

            // Reach the first image of list
            if (imageIndex < 0)
            {
                if (!Config.EnableLoopBackNavigation)
                {
                    PicMain.ShowMessage(Config.Language[$"{Name}._ReachedLastLast"],
                        Config.InAppMessageDuration);

                    return;
                }
            }
        }


        // Check if current index is greater than upper limit
        if (imageIndex >= _imageList.Count)
            imageIndex = 0;

        // Check if current index is less than lower limit
        if (imageIndex < 0)
            imageIndex = _imageList.Count - 1;


        // Update current index
        _currentIndex = imageIndex;

        #endregion // Validate image index


        var filePath = _imageList[_currentIndex];

        await LoadImageAsync(filePath, _loadImageCancelToken);
    }


    private async Task LoadImageAsync(string? filePath, CancellationTokenSource? tokenSrc = null)
    {
        if (InvokeRequired)
        {
            Invoke(LoadImageAsync, filePath, tokenSrc);
            return;
        }

        var photo = new IgPhoto(filePath);

        var readSettings = new CodecReadOptions()
        {
            ColorProfileName = Config.ColorProfile,
            ApplyColorProfileForAll = Config.ApplyColorProfileForAll,
            ImageChannel = ColorChannel.All,
            AutoScaleDownLargeImage = true,
            UseEmbeddedThumbnailRawFormats = Config.UseEmbeddedThumbnailRawFormats,
            UseEmbeddedThumbnailOtherFormats = Config.UseEmbeddedThumbnailOtherFormats,
            EmbeddedThumbnailMinWidth = Config.EmbeddedThumbnailMinWidth,
            EmbeddedThumbnailMinHeight = Config.EmbeddedThumbnailMinHeight,
            FirstFrameOnly = true,
            CorrectRotation = true,
        };

        // get metadata
        _currentMetadata = PhotoCodec.LoadMetadata(filePath, readSettings);

        // on image loading
        OnImageLoading();

        try
        {
            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();

            await photo.LoadAsync(readSettings, tokenSrc);

            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) { }

        // on image loaded
        OnImageLoaded(photo);
    }


    private void OnImageLoading()
    {
        if (InvokeRequired)
        {
            Invoke(OnImageLoading);
            return;
        }

        PicMain.ClearMessage();
        PicMain.ShowMessage(Config.Language[$"FrmMain._Loading"], "", delayMs: 1500);


        _ = BHelper.RunAsThread(() => UpdateImageInfo(ImageInfoUpdateTypes.All, _currentMetadata.FilePath));
    }


    private void OnImageLoaded(IgPhoto photo)
    {
        if (InvokeRequired)
        {
            OnImageLoaded(photo);
            return;
        }


        // image error
        if (photo.Error != null)
        {
            PicMain.SetImage(null);

            PicMain.ShowMessage(photo.Error.Source + ": " + photo.Error.Message,
                Config.Language[$"FrmMain.PicMain._ErrorText"]);
        }

        else if (!(photo?.ImgData.IsImageNull ?? true))
        {
            var isImageBigForFading = photo.Metadata.Width > 8000
                || photo.Metadata.Height > 8000;
            var enableFading = !isImageBigForFading;

            // set the main image
            PicMain.SetImage(photo.ImgData, enableFading, 0.4f, 0.02f);

            PicMain.ClearMessage();


            // reset countdown timer value
            _slideshowCountdown = RandomizeSlideshowInterval();

            // since the UI does not print milliseconds,
            // this prevents the coutdown to flash the maximum value during the first tick
            if (_slideshowCountdown == Math.Ceiling(_slideshowCountdown))
            {
                _slideshowCountdown -= 0.001f;
            }
        }


        UpdateImageInfo(ImageInfoUpdateTypes.Dimension | ImageInfoUpdateTypes.FramesCount);

        // Collect system garbage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    #endregion // Load image



    /// <summary>
    /// Sets slideshow state.
    /// </summary>
    private void SetSlideshowState(bool? enable = null, bool displayMessage = true)
    {
        if (InvokeRequired)
        {
            Invoke(SetSlideshowState, enable, displayMessage);
            return;
        }

        enable ??= !_slideshowTimer.Enabled;
        var msg = "";

        // play
        if (enable.Value)
        {
            _slideshowTimer.Enabled = true;
            _slideshowStopwatch.Start();

            msg = Config.Language[$"{Name}._ResumeSlideshow"];
        }
        // pause
        else
        {
            _slideshowTimer.Enabled = false;
            _slideshowStopwatch.Stop();

            msg = Config.Language[$"{Name}._PauseSlideshow"];
        }

        if (displayMessage)
        {
            PicMain.ShowMessage(msg, Config.InAppMessageDuration);
        }
    }


    /// <summary>
    /// Gets image info in status bar.
    /// </summary>
    private void UpdateImageInfo(ImageInfoUpdateTypes types = ImageInfoUpdateTypes.All, string? filePath = null)
    {
        if (InvokeRequired)
        {
            Invoke(UpdateImageInfo, types, filePath);
            return;
        }

        var updateAll = ImageInfo.IsNull || types.HasFlag(ImageInfoUpdateTypes.All);


        // AppName
        if (Config.InfoItems.Contains(nameof(ImageInfo.AppName)))
        {
            ImageInfo.AppName = App.AppName;
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


        // the viewing image is from the image list
        var fullPath = string.IsNullOrEmpty(filePath)
                ? _imageList[_currentIndex]
                : BHelper.ResolvePath(filePath);

        // ListCount
        if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ListCount))
        {
            if (Config.InfoItems.Contains(nameof(ImageInfo.ListCount))
                && _imageList.Count > 0)
            {
                ImageInfo.ListCount = $"{_currentIndex + 1}/{_imageList.Count} {Config.Language[$"FrmMain._Files"]}";

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
                ImageInfo.Name = Path.GetFileName(filePath);
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
                ImageInfo.Path = filePath;
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
                && _currentMetadata != null)
            {
                ImageInfo.FileSize = _currentMetadata.FileSizeFormated;
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
                && _currentMetadata != null
                && _currentMetadata.FramesCount > 1)
            {
                ImageInfo.FramesCount = $"{_currentMetadata.FramesCount} frames";
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
                && _currentMetadata != null)
            {
                ImageInfo.Dimension = $"{_currentMetadata.Width} x {_currentMetadata.Height} px";
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
                && _currentMetadata != null)
            {
                ImageInfo.ModifiedDateTime = _currentMetadata.FileLastWriteTimeFormated + " (m)";
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
                && _currentMetadata != null)
            {
                ImageInfo.ExifRating = BHelper.FormatStarRatingText(_currentMetadata.ExifRatingPercent);
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
                && _currentMetadata != null
                && _currentMetadata.ExifDateTime != null)
            {
                ImageInfo.ExifDateTime = BHelper.FormatDateTime(_currentMetadata.ExifDateTime) + " (e)";
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
                && _currentMetadata != null
                && _currentMetadata.ExifDateTimeOriginal != null)
            {
                ImageInfo.ExifDateTimeOriginal = BHelper.FormatDateTime(_currentMetadata.ExifDateTimeOriginal) + " (o)";
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
                && _currentMetadata != null)
            {
                if (_currentMetadata.ExifDateTimeOriginal != null)
                {
                    dtStr = BHelper.FormatDateTime(_currentMetadata.ExifDateTimeOriginal) + " (o)";
                }
                else if (_currentMetadata.ExifDateTime != null)
                {
                    dtStr = BHelper.FormatDateTime(_currentMetadata.ExifDateTime) + " (e)";
                }
                else
                {
                    dtStr = _currentMetadata.FileLastWriteTimeFormated + " (m)";
                }
            }

            ImageInfo.DateTimeAuto = dtStr;
        }


        Text = ImageInfo.ToString(Config.InfoItems, false);
    }


    /// <summary>
    /// Randomize slideshow interval in seconds
    /// </summary>
    private static float RandomizeSlideshowInterval()
    {
        var intervalTo = Config.UseRandomIntervalForSlideshow ? Config.SlideshowIntervalTo : Config.SlideshowInterval;

        var ran = new Random();
        var interval = (float)(ran.NextDouble() * (intervalTo - Config.SlideshowInterval) + Config.SlideshowInterval);

        return interval;
    }


    /// <summary>
    /// Loads hotkeys of menu
    /// </summary>
    /// <param name="menu"></param>
    private void LoadMenuHotkeys(ToolStripDropDown? menu = null)
    {
        if (InvokeRequired)
        {
            Invoke(LoadMenuHotkeys, menu);
            return;
        }

        // default: context menu
        menu ??= MnuContext;


        var allItems = MenuUtils.GetActualItems(menu.Items);
        foreach (ToolStripMenuItem item in allItems)
        {
            item.ShortcutKeyDisplayString = Config.GetHotkeyString(CurrentMenuHotkeys, item.Name);

            if (item.HasDropDownItems)
            {
                LoadMenuHotkeys(item.DropDown);
            }
        }
    }


    /// <summary>
    /// Loads theme pack
    /// </summary>
    private void LoadTheme(SystemThemeMode mode = SystemThemeMode.Unknown)
    {
        var themMode = mode;

        if (mode == SystemThemeMode.Unknown)
        {
            themMode = ThemeUtils.GetSystemThemeMode();
        }

        // correct theme mode
        var isDarkMode = themMode != SystemThemeMode.Light;

        MnuContext.Theme = Config.Theme;

        // background
        BackColor = Config.BackgroundColor;
        PicMain.BackColor = Config.BackgroundColor;
        PicMain.ForeColor = Config.Theme.Settings.TextColor;

        // navigation buttons
        PicMain.NavHoveredColor = Color.FromArgb(200, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavPressedColor = Color.FromArgb(240, Config.Theme.Settings.ToolbarBgColor);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;


        Config.ApplyFormTheme(this, Config.Theme);
    }


    /// <summary>
    /// Loads language
    /// </summary>
    private void LoadLanguage()
    {
        var lang = Config.Language;

        // slideshow controls
        MnuPauseResumeSlideshow.Text = lang[$"{Name}.{nameof(MnuPauseResumeSlideshow)}"];
        MnuExitSlideshow.Text = lang[$"{Name}.{nameof(MnuExitSlideshow)}"];
        MnuShowMainWindow.Text = lang[$"{Name}.{nameof(MnuShowMainWindow)}"];

        // toggle menus
        MnuFullScreen.Text = lang[$"FrmMain.{nameof(MnuFullScreen)}"];
        MnuToggleCountdown.Text = lang[$"{Name}.{nameof(MnuToggleCountdown)}"];
        MnuToggleCheckerboard.Text = lang[$"FrmMain.{nameof(MnuToggleCheckerboard)}"];

        // navigation
        MnuChangeBackgroundColor.Text = lang[$"{Name}.{nameof(MnuChangeBackgroundColor)}"];
        MnuNavigation.Text = lang[$"FrmMain.{nameof(MnuNavigation)}"];
        MnuViewNext.Text = lang[$"FrmMain.{nameof(MnuViewNext)}"];
        MnuViewPrevious.Text = lang[$"FrmMain.{nameof(MnuViewPrevious)}"];
        MnuGoToFirst.Text = lang[$"FrmMain.{nameof(MnuGoToFirst)}"];
        MnuGoToLast.Text = lang[$"FrmMain.{nameof(MnuGoToLast)}"];

        // zoom
        MnuActualSize.Text = lang[$"FrmMain.{nameof(MnuActualSize)}"];
        MnuZoomModes.Text = lang[$"{Name}.{nameof(MnuZoomModes)}"];

        MnuAutoZoom.Text = lang[$"FrmMain.{nameof(MnuAutoZoom)}"];
        MnuLockZoom.Text = lang[$"FrmMain.{nameof(MnuLockZoom)}"];
        MnuScaleToWidth.Text = lang[$"FrmMain.{nameof(MnuScaleToWidth)}"];
        MnuScaleToHeight.Text = lang[$"FrmMain.{nameof(MnuScaleToHeight)}"];
        MnuScaleToFit.Text = lang[$"FrmMain.{nameof(MnuScaleToFit)}"];
        MnuScaleToFill.Text = lang[$"FrmMain.{nameof(MnuScaleToFill)}"];


        MnuLoadingOrders.Text = lang[$"FrmMain.{nameof(MnuLoadingOrders)}"];
        LoadMnuLoadingOrdersSubItems(); // update Loading order items

        // viewing image actions
        MnuOpenWith.Text = lang[$"FrmMain.{nameof(MnuOpenWith)}"];
        MnuOpenLocation.Text = lang[$"FrmMain.{nameof(MnuOpenLocation)}"];
        MnuCopyPath.Text = lang[$"FrmMain.{nameof(MnuCopyPath)}"];

    }


    /// <summary>
    /// Load Loading order menu items
    /// </summary>
    private void LoadMnuLoadingOrdersSubItems()
    {
        // clear items
        MnuLoadingOrders.DropDown.Items.Clear();

        var newMenuIconHeight = DpiApi.Transform(Constants.MENU_ICON_HEIGHT);

        // add ImageOrderBy items
        foreach (var order in Enum.GetValues(typeof(ImageOrderBy)))
        {
            var orderName = Enum.GetName(typeof(ImageOrderBy), order);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"_.{nameof(ImageOrderBy)}._{orderName}"],
                Tag = order,
                CheckOnClick = true,
                Checked = (int)order == (int)Config.ImageLoadingOrder,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
            };

            mnu.Click += MnuLoadingOrderItem_Click;
            MnuLoadingOrders.DropDown.Items.Add(mnu);
        }

        MnuLoadingOrders.DropDown.Items.Add(new ToolStripSeparator());

        // add ImageOrderType items
        foreach (var orderType in Enum.GetValues(typeof(ImageOrderType)))
        {
            var typeName = Enum.GetName(typeof(ImageOrderType), orderType);
            var mnu = new ToolStripRadioButtonMenuItem()
            {
                Text = Config.Language[$"_.{nameof(ImageOrderType)}._{typeName}"],
                Tag = orderType,
                CheckOnClick = true,
                Checked = (int)orderType == (int)Config.ImageLoadingOrderType,
                ImageScaling = ToolStripItemImageScaling.None,
                Image = new Bitmap(newMenuIconHeight, newMenuIconHeight)
            };

            mnu.Click += MnuLoadingOrderTypeItem_Click;
            MnuLoadingOrders.DropDown.Items.Add(mnu);
        }
    }


    private void MnuLoadingOrderItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        var selectedOrder = (ImageOrderBy)(int)mnu.Tag;

        if (selectedOrder != Config.ImageLoadingOrder)
        {
            Config.ImageLoadingOrder = selectedOrder;

            //// reload image list
            //IG_ReloadList();

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }

    private void MnuLoadingOrderTypeItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        var selectedType = (ImageOrderType)(int)mnu.Tag;

        if (selectedType != Config.ImageLoadingOrderType)
        {
            Config.ImageLoadingOrderType = selectedType;

            //// reload image list
            //IG_ReloadList();

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }


}
