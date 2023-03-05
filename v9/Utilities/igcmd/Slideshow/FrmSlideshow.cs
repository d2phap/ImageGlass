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
using D2Phap;
using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.Services;
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.Views;
using System.Diagnostics;
using System.IO.Pipes;
using Timer = System.Windows.Forms.Timer;

namespace igcmd.Slideshow;

public partial class FrmSlideshow : ModernForm
{
    private PipeClient _client;
    private string _serverName;
    private string _initImagePath;

    private CancellationTokenSource _loadImageCancelToken = new();
    private ImageBooster _images = new();
    private int _currentIndex = -1;
    private IgMetadata? _currentMetadata = null;

    private Timer _slideshowTimer = new() { Enabled = false };
    private Stopwatch _slideshowStopwatch = new(); // slideshow stopwatch
    private float _slideshowCountdown = 5; // slideshow countdown interval
    private Rectangle _windowBound = new();
    private FormWindowState _windowState = FormWindowState.Normal;

    private CancellationTokenSource _hideCursorCancelToken = new();
    private bool _isCursorHidden = false;
    private bool _isColorPickerOpen = false;


    /// <summary>
    /// Hotkeys list of main menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> CurrentMenuHotkeys = new()
    {
        // Open context menu
	    { nameof(MnuContext),               new() { new(Keys.Alt | Keys.F) } },

        { nameof(MnuPauseResumeSlideshow),  new() { new(Keys.Space) } },

        // MnuNavigation
        { nameof(MnuViewNext),              new() { new (Keys.Right) } },
        { nameof(MnuViewPrevious),          new() { new (Keys.Left) } },
        { nameof(MnuGoToFirst),             new() { new (Keys.Home) } },
        { nameof(MnuGoToLast),              new() { new (Keys.End) } },

        { nameof(MnuFullScreen),            new() { new (Keys.F11) } },
        { nameof(MnuToggleCountdown),       new() { new (Keys.C) } },
        { nameof(MnuToggleCheckerboard),    new() { new (Keys.B) } },

        { nameof(MnuChangeBackgroundColor), new() { new (Keys.M) } },

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

        { nameof(MnuExitSlideshow),         new() { new(Keys.Escape) } },
    };


    public FrmSlideshow(string slideshowIndex, string initImagePath)
    {
        InitializeComponent();

        // update the DpiApi when DPI changed.
        EnableDpiApiUpdate = true;

        Config.Load();

        _serverName = $"{Constants.SLIDESHOW_PIPE_PREFIX}{slideshowIndex}";
        _initImagePath = initImagePath;


        // load configs
        _ = int.TryParse(slideshowIndex, out var indexNumber);
        Text = $"{Config.Language["FrmMain.MnuSlideshow"]} {indexNumber + 1} - {App.AppName}";
        SetUpFrmSlideshowConfigs();

        // update theme icons
        OnDpiChanged();

        ApplyTheme(Config.Theme.Settings.IsDarkMode, Config.WindowBackdrop);
    }


    private void SetUpFrmSlideshowConfigs()
    {
        SuspendLayout();


        PicMain.InterpolationScaleDown = Config.ImageInterpolationScaleDown;
        PicMain.InterpolationScaleUp = Config.ImageInterpolationScaleUp;

        Config.EnableSlideshow = true;
        MnuToggleCountdown.Checked = Config.ShowSlideshowCountdown;

        // zoom mode
        SetZoomMode(Config.ZoomMode);
        if (Config.ZoomMode == ZoomMode.LockZoom)
        {
            PicMain.ZoomFactor = Config.ZoomLockValue / 100f;
        }

        // menu
        MnuContext.CurrentDpi = DeviceDpi;

        ResumeLayout(false);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();
        MnuContext.Theme = Config.Theme;

        if (!EnableTransparent)
        {
            BackColor = Config.SlideshowBackgroundColor.NoAlpha();
        }


        // viewer
        PicMain.BackColor = Config.SlideshowBackgroundColor;
        PicMain.ForeColor = PicMain.BackColor.InvertBlackOrWhite(220);
        PicMain.AccentColor = WinColorsApi.GetAccentColor(true);


        // navigation buttons
        var navColor = Config.Theme.Colors.ToolbarBgColor;
        PicMain.NavHoveredColor = navColor.WithAlpha(200);
        PicMain.NavPressedColor = navColor.WithAlpha(240);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;


        ResumeLayout(false);
        base.ApplyTheme(darkMode, style);
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        Config.Theme.ReloadThemeColors();

        base.OnSystemAccentColorChanged(e);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // theme mode is changed, need to load the corresponding theme pack
        Config.LoadThemePack(e.IsDarkMode, true, true);

        // load the theme icons
        OnDpiChanged();

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);

        base.OnRequestUpdatingColorMode(e);
    }


    protected override void OnDpiChanged()
    {
        base.OnDpiChanged();
        SuspendLayout();

        // scale toolbar icons corresponding to DPI
        var newIconHeight = DpiApi.Transform(Config.ToolbarIconHeight);

        // reload theme
        Config.Theme.LoadTheme(newIconHeight);

        // update picmain scaling
        PicMain.NavButtonSize = this.ScaleToDpi(new SizeF(60f, 60f));
        PicMain.CheckerboardCellSize = this.ScaleToDpi(Constants.VIEWER_GRID_SIZE);

        ResumeLayout(false);
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);

        MnuContext.CurrentDpi = e.DeviceDpiNew;
    }


    private void FrmSlideshow_Load(object sender, EventArgs e)
    {
        _slideshowTimer.Interval = 10; // support milliseconds
        _slideshowTimer.Tick += SlideshowTimer_Tick;

        PicMain.Render += PicMain_Render;
        PicMain.MouseWheel += PicMain_MouseWheel;

        // windowed slideshow
        if (Config.EnableWindowedSlideshow)
        {
            // load window placement from settings
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());
        }
        // full screen slideshow
        else
        {
            // to hide the animation effect of window border
            FormBorderStyle = FormBorderStyle.None;

            // load window placement from settings here to save the initial
            // position of window so that when user exists the fullscreen mode,
            // it can be restore correctly
            WindowSettings.SetPlacementToWindow(this, WindowSettings.GetFrmMainPlacementFromConfig());

            SetFullScreenMode(true);
        }


        // load the init image
        _ = BHelper.RunAsThread(() => _ = LoadImageAsync(_initImagePath, _loadImageCancelToken));

        _client = new PipeClient(_serverName, PipeDirection.InOut);
        _client.MessageReceived += Client_MessageReceived;
        _client.Disconnected += (_, _) => Application.Exit();
        _ = _client.ConnectAsync();

        // load menu hotkeys
        Config.MergeHotkeys(ref CurrentMenuHotkeys, Config.MenuHotkeys);
        LoadMenuHotkeys();

        LoadMenuTagData();

        // load language
        LoadLanguage();

        // start slideshow
        SetSlideshowState(true);
    }


    private void FrmSlideshow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _client.Dispose();
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var hotkey = new Hotkey(keyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // open context menu
        if (actions.Contains(nameof(MnuContext)))
        {
            MnuContext.Show(this, (PicMain.Width - MnuContext.Width) / 2, (PicMain.Height - MnuContext.Height) / 2);
            return true;
        }


        #region Register and run CONTEXT MENU shortcuts

        bool CheckMenuShortcut(ToolStripMenuItem mnu)
        {
            var menuHotkeyList = Config.GetHotkey(CurrentMenuHotkeys, mnu.Name);
            var menuHotkey = menuHotkeyList.SingleOrDefault(k => k.KeyData == keyData);

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
                return true;
            }
        }
        #endregion


        return base.ProcessCmdKey(ref msg, keyData);
    }


    private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.MessageName)) return;


        // terminate slideshow
        if (e.MessageName == ToolServerMsgs.TOOL_TERMINATE)
        {
            Application.Exit();
            return;
        }


        if (string.IsNullOrEmpty(e.MessageData)) return;

        // update image list
        if (e.MessageName.Equals(SlideshowPipeCommands.SET_IMAGE_LIST, StringComparison.InvariantCultureIgnoreCase))
        {
            var list = BHelper.ParseJson<List<string>>(e.MessageData);

            if (list != null && list.Count > 0)
            {
                _ = BHelper.RunAsThread(async () =>
                {
                    await LoadImageListAsync(list, _initImagePath);

                    // enable slideshow
                    SetSlideshowState(true, false);
                });
            }

            return;
        }


        // update language
        if (e.MessageName.Equals(SlideshowPipeCommands.SET_LANGUAGE, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Language = new IgLang(e.MessageData, App.StartUpDir(Dir.Languages));
            LoadLanguage();
            return;
        }


        // update theme
        if (e.MessageName.Equals(SlideshowPipeCommands.SET_THEME, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Theme = new IgTheme(e.MessageData, Config.ToolbarIconHeight);

            ApplyTheme(Config.Theme.Settings.IsDarkMode);
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
            if (_currentIndex == _images.Length - 1)
            {
                // loop the list
                if (!Config.ShouldLoopSlideshow)
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
        if (Config.ShowSlideshowCountdown && isSecond)
        {
            PicMain.Invalidate();
        }
    }


    // PicMain events
    #region PicMain events

    private void PicMain_Render(object? sender, RenderEventArgs e)
    {
        if (!_slideshowTimer.Enabled || !Config.ShowSlideshowCountdown) return;

        // draw countdown text ----------------------------------------------
        var countdownTime = TimeSpan.FromSeconds(_slideshowCountdown + 1);
        var text = (countdownTime - _slideshowStopwatch.Elapsed).ToString("mm'∶'ss");


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
        var borderRadius = BHelper.IsOS(WindowsOS.Win11OrLater) ? 10 : 1;
        var bgColor = Color.FromArgb(150, PicMain.BackColor);
        var bgRect = new RectangleF(bgX, bgY, bgSize.Width, bgSize.Height);

        e.Graphics.DrawRectangle(bgRect, borderRadius, bgColor, bgColor);


        // calculate text position
        var fontX = bgX + (bgSize.Width / 2) - (fontSize.Width / 2);
        var fontY = bgY + (bgSize.Height / 2) - (fontSize.Height / 2);

        // draw text
        var textColor = PicMain.BackColor.InvertBlackOrWhite(150);

        e.Graphics.DrawText(text, font.Name, font.Size, fontX, fontY, textColor, textDpi: DeviceDpi);
    }


    private void PicMain_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }
        DelayHideCursor();


        MouseWheelAction action;
        var eventType = ModifierKeys switch
        {
            Keys.Control => MouseWheelEvent.PressCtrlAndScroll,
            Keys.Shift => MouseWheelEvent.PressShiftAndScroll,
            Keys.Alt => MouseWheelEvent.PressAltAndScroll,
            _ => MouseWheelEvent.Scroll,
        };


        // Get mouse wheel action
        #region Get mouse wheel action

        // get user-defined mouse wheel action
        if (Config.MouseWheelActions.ContainsKey(eventType))
        {
            action = Config.MouseWheelActions[eventType];
        }
        // if not found, use the defaut mouse wheel action
        else
        {
            switch (eventType)
            {
                case MouseWheelEvent.Scroll:
                    action = MouseWheelAction.Zoom;
                    break;
                case MouseWheelEvent.PressCtrlAndScroll:
                    action = MouseWheelAction.PanVertically;
                    break;
                case MouseWheelEvent.PressShiftAndScroll:
                    action = MouseWheelAction.PanHorizontally;
                    break;
                case MouseWheelEvent.PressAltAndScroll:
                    action = MouseWheelAction.BrowseImages;
                    break;
                default:
                    action = MouseWheelAction.DoNothing;
                    break;
            }
        }
        #endregion


        // Run mouse wheel action
        #region Run mouse wheel action

        if (action == MouseWheelAction.Zoom)
        {
            PicMain.ZoomByDeltaToPoint(e.Delta, e.Location);
        }
        else if (action == MouseWheelAction.PanVertically)
        {
            if (e.Delta > 0)
            {
                PicMain.PanUp(e.Delta + PicMain.PanDistance / 4);
            }
            else
            {
                PicMain.PanDown(Math.Abs(e.Delta) + PicMain.PanDistance / 4);
            }
        }
        else if (action == MouseWheelAction.PanHorizontally)
        {
            if (e.Delta > 0)
            {
                PicMain.PanLeft(e.Delta + PicMain.PanDistance / 4);
            }
            else
            {
                PicMain.PanRight(Math.Abs(e.Delta) + PicMain.PanDistance / 4);
            }
        }
        else if (action == MouseWheelAction.BrowseImages)
        {
            if (e.Delta < 0)
            {
                _ = ViewNextImageAsync(1);
            }
            else
            {
                _ = ViewNextImageAsync(-1);
            }
        }
        #endregion
    }


    private void PicMain_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }

        DelayHideCursor();
    }


    private void PicMain_OnNavLeftClicked(object? sender, MouseEventArgs e)
    {
        _ = ViewNextImageAsync(-1);
    }

    private void PicMain_OnNavRightClicked(object? sender, MouseEventArgs e)
    {
        _ = ViewNextImageAsync(1);
    }

    private void PicMain_OnZoomChanged(object? sender, ZoomEventArgs e)
    {
        UpdateImageInfo(ImageInfoUpdateTypes.Zoom);
    }

    private void PicMain_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }

        DelayHideCursor();
    }

    #endregion // PicMain events


    // Load image
    #region Load image

    /// <summary>
    /// Loads image list
    /// </summary>
    /// <param name="initFilePath">The initial file path to find image index.</param>
    private async Task LoadImageListAsync(IEnumerable<string> fileList, string? initFilePath = null)
    {
        await Task.Run(() =>
        {
            var list = BHelper.SortImageList(fileList,
                Config.ImageLoadingOrder,
                Config.ImageLoadingOrderType,
                Config.ShouldGroupImagesByDirectory);
            _images = new ImageBooster(list)
            {
                MaxQueue = 1,
                MaxFileSizeInMbToCache = 100,
                MaxImageDimensionToCache = Constants.MAX_IMAGE_DIMENSION,
            };

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
            _currentIndex = _images.IndexOf(initFilePath);
        });
    }


    /// <summary>
    /// View the next image
    /// </summary>
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


        if (_images.Length > 0)
        {
            // Reach end of list
            if (imageIndex >= _images.Length)
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
        if (imageIndex >= _images.Length)
            imageIndex = 0;

        // Check if current index is less than lower limit
        if (imageIndex < 0)
            imageIndex = _images.Length - 1;


        // Update current index
        _currentIndex = imageIndex;

        #endregion // Validate image index


        await LoadImageAsync(null, _loadImageCancelToken);
    }


    /// <summary>
    /// Loads image to the viewer.
    /// </summary>
    /// <param name="filePath">Use <see cref="_currentIndex"/> if <paramref name="filePath"/> is <c>null</c>.</param>
    private async Task LoadImageAsync(string? filePath, CancellationTokenSource? tokenSrc = null)
    {
        if (InvokeRequired)
        {
            Invoke(LoadImageAsync, filePath, tokenSrc);
            return;
        }

        IgPhoto? photo;
        var readSettings = new CodecReadOptions()
        {
            ColorProfileName = Config.ColorProfile,
            ApplyColorProfileForAll = Config.ShouldUseColorProfileForAll,
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
        if (!string.IsNullOrEmpty(filePath))
        {
            _currentMetadata = PhotoCodec.LoadMetadata(filePath, readSettings);
        }
        else
        {
            var currentFilePath = _images.GetFilePath(_currentIndex);
            _currentMetadata = PhotoCodec.LoadMetadata(currentFilePath, readSettings);
        }


        // on image loading
        OnImageLoading();

        try
        {
            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();

            if (!string.IsNullOrEmpty(filePath))
            {
                photo = new(filePath);
                await photo.LoadAsync(readSettings, tokenSrc);
            }
            else
            {
                photo = await _images.GetAsync(_currentIndex, tokenSrc: tokenSrc);
            }

            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();

            // on image loaded
            OnImageLoaded(photo);
        }
        catch (OperationCanceledException)
        {
            _images.CancelLoading(_currentIndex);
        }
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

            var emoji = BHelper.IsOS(WindowsOS.Win11OrLater) ? "🥲" : "🙄";
            var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            var appVersion = App.Version + $" ({archInfo}, .NET {Environment.Version})";

            var debugInfo = $"ImageGlass {Constants.APP_CODE.CapitalizeFirst()} v{appVersion}" +
                $"\r\n{ImageMagick.MagickNET.Version}" +
                $"\r\n" +
                $"\r\nℹ️ Error details:" +
                $"\r\n";

            PicMain.ShowMessage(debugInfo +
                photo.Error.Source + ": " + photo.Error.Message,
                Config.Language[$"FrmMain.PicMain._ErrorText"] + $" {emoji}");
        }

        else if (!(photo?.ImgData.IsImageNull ?? true))
        {
            var isImageBigForFading = photo.Metadata.Width > 8000
                || photo.Metadata.Height > 8000;
            var enableFading = !isImageBigForFading;

            // set the main image
            PicMain.SetImage(photo.ImgData,
                enableFading: enableFading,
                initOpacity: 0.4f,
                opacityStep: 0.02f);

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
                ? _images.GetFilePath(_currentIndex)
                : BHelper.ResolvePath(filePath);

        // ListCount
        if (updateAll || types.HasFlag(ImageInfoUpdateTypes.ListCount))
        {
            if (Config.InfoItems.Contains(nameof(ImageInfo.ListCount))
                && _images.Length > 0)
            {
                ImageInfo.ListCount = $"{_currentIndex + 1}/{_images.Length} {Config.Language[$"FrmMain._Files"]}";

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


    private void LoadMenuTagData()
    {
        // Zoom mode
        MnuAutoZoom.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuLockZoom.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToWidth.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToHeight.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToFit.Tag = new ModernMenuItemTag() { SingleSelect = true };
        MnuScaleToFill.Tag = new ModernMenuItemTag() { SingleSelect = true };
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
                Tag = new ModernMenuItemTag()
                {
                    SingleSelect = true,
                    ImageOrderBy = (ImageOrderBy)order,
                },
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
                Tag = new ModernMenuItemTag()
                {
                    SingleSelect = true,
                    ImageOrderType = (ImageOrderType)orderType,
                },
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


        if (mnu.Tag is ImageOrderBy selectedOrder
            && selectedOrder != Config.ImageLoadingOrder)
        {
            Config.ImageLoadingOrder = selectedOrder;

            // reload image list
            _ = LoadImageListAsync(_images.FileNames, _images.GetFilePath(_currentIndex));

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }


    private void MnuLoadingOrderTypeItem_Click(object? sender, EventArgs e)
    {
        var mnu = sender as ToolStripMenuItem;
        if (mnu is null) return;

        if (mnu.Tag is ImageOrderType selectedType
            && selectedType != Config.ImageLoadingOrderType)
        {
            Config.ImageLoadingOrderType = selectedType;

            // reload image list
            _ = LoadImageListAsync(_images.FileNames, _images.GetFilePath(_currentIndex));

            // reload the state
            LoadMnuLoadingOrdersSubItems();
        }
    }


    /// <summary>
    /// Start counting a period of time to hide cursor.
    /// </summary>
    private void DelayHideCursor()
    {
        _hideCursorCancelToken.Cancel();
        _hideCursorCancelToken = new();
        _ = HideCursorAsync(_hideCursorCancelToken.Token);
    }


    /// <summary>
    /// Hides the cursor after 3 seconds
    /// </summary>
    private async Task HideCursorAsync(CancellationToken token = default)
    {
        try
        {
            await Task.Delay(3000, token);
            token.ThrowIfCancellationRequested();

            if (!MnuContext.IsOpen && !_isColorPickerOpen)
            {
                Cursor.Hide();
                _isCursorHidden = true;
            }
        }
        catch (OperationCanceledException) { }
    }


    // Menu
    #region Menu

    private void MnuContext_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }
    }

    private void MnuPauseResumeSlideshow_Click(object sender, EventArgs e)
    {
        Config.EnableSlideshow = !Config.EnableSlideshow;
        SetSlideshowState(Config.EnableSlideshow);
    }

    private void MnuExitSlideshow_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void MnuFullScreen_Click(object sender, EventArgs e)
    {
        Config.EnableWindowedSlideshow = !Config.EnableWindowedSlideshow;
        SetFullScreenMode(!Config.EnableWindowedSlideshow);
    }

    /// <summary>
    /// Enter or Exit Full screen mode
    /// </summary>
    private void SetFullScreenMode(bool enable = true)
    {
        MnuFullScreen.Checked = enable;

        // full screen
        if (enable)
        {
            Visible = false;

            // back up the last states of the window
            _windowBound = Bounds;
            _windowState = WindowState;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            Bounds = Screen.FromControl(this).Bounds;

            //// disable background colors
            //WindowApi.SetWindowFrame(Handle, new Padding(0));
            //PicMain.BackColor = Config.SlideshowBackgroundColor.NoAlpha();

            Visible = true;
        }

        // exit full screen
        else
        {
            // windows state
            if (_windowState == FormWindowState.Normal)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;

                // Windows Bound (Position + Size)
                Bounds = _windowBound;
            }
            else if (_windowState == FormWindowState.Maximized)
            {
                // Windows Bound (Position + Size)
                var wp = WindowSettings.GetPlacement(_windowBound, _windowState);
                WindowSettings.SetPlacementToWindow(this, wp);

                // to make sure the SizeChanged event is not triggered
                // before we set the window placement
                FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
            }

            //// re-enable background colors
            //WindowApi.SetWindowFrame(Handle, BackdropMargin);
            //PicMain.BackColor = Config.SlideshowBackgroundColor;

            Config.UpdateFormIcon(this);
        }
    }


    private void MnuToggleCountdown_Click(object sender, EventArgs e)
    {
        Config.ShowSlideshowCountdown = !Config.ShowSlideshowCountdown;
        MnuToggleCountdown.Checked = Config.ShowSlideshowCountdown;

        PicMain.Invalidate();
    }

    private void MnuToggleCheckerboard_Click(object sender, EventArgs e)
    {
        Config.ShowCheckerBoard = !Config.ShowCheckerBoard;
        MnuToggleCheckerboard.Checked = Config.ShowCheckerBoard;

        if (Config.ShowCheckerBoard)
        {
            if (Config.ShowCheckerboardOnlyImageRegion)
            {
                PicMain.CheckerboardMode = CheckerboardMode.Image;
            }
            else
            {
                PicMain.CheckerboardMode = CheckerboardMode.Client;
            }
        }
        else
        {
            PicMain.CheckerboardMode = CheckerboardMode.None;
        }
    }

    private void MnuChangeBackgroundColor_Click(object sender, EventArgs e)
    {
        _isColorPickerOpen = true;
        using var cd = new ColorDialog()
        {
            Color = Config.SlideshowBackgroundColor,
            FullOpen = true,
        };

        if (cd.ShowDialog() == DialogResult.OK)
        {
            BackColor = PicMain.BackColor = Config.SlideshowBackgroundColor = cd.Color;
            PicMain.ForeColor = PicMain.BackColor.InvertBlackOrWhite(220);

            _isColorPickerOpen = false;
        }
    }

    private void MnuViewNext_Click(object sender, EventArgs e)
    {
        _ = ViewNextImageAsync(1);
    }

    private void MnuViewPrevious_Click(object sender, EventArgs e)
    {
        _ = ViewNextImageAsync(-1);
    }

    private void MnuGoToFirst_Click(object sender, EventArgs e)
    {
        _currentIndex = 0;
        _ = ViewNextImageAsync(0);
    }

    private void MnuGoToLast_Click(object sender, EventArgs e)
    {
        _currentIndex = _images.Length - 1;
        _ = ViewNextImageAsync(0);
    }

    private void MnuActualSize_Click(object sender, EventArgs e)
    {
        PicMain.ZoomFactor = 1;
    }


    /// <summary>
    /// Sets the zoom mode value
    /// </summary>
    private void SetZoomMode(ZoomMode mode)
    {
        Config.ZoomMode = mode;

        if (PicMain.ZoomMode == Config.ZoomMode)
        {
            PicMain.Refresh();
        }
        else
        {
            PicMain.ZoomMode = Config.ZoomMode;
        }

        // update menu items state
        MnuAutoZoom.Checked = Config.ZoomMode == ZoomMode.AutoZoom;
        MnuLockZoom.Checked = Config.ZoomMode == ZoomMode.LockZoom;
        MnuScaleToWidth.Checked = Config.ZoomMode == ZoomMode.ScaleToWidth;
        MnuScaleToHeight.Checked = Config.ZoomMode == ZoomMode.ScaleToHeight;
        MnuScaleToFill.Checked = Config.ZoomMode == ZoomMode.ScaleToFill;
        MnuScaleToFit.Checked = Config.ZoomMode == ZoomMode.ScaleToFit;
    }

    private void MnuAutoZoom_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.AutoZoom);
    }

    private void MnuLockZoom_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.LockZoom);
    }

    private void MnuScaleToWidth_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.ScaleToWidth);
    }

    private void MnuScaleToHeight_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.ScaleToHeight);
    }

    private void MnuScaleToFit_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.ScaleToFit);
    }

    private void MnuScaleToFill_Click(object sender, EventArgs e)
    {
        SetZoomMode(ZoomMode.ScaleToFill);
    }

    private void MnuOpenWith_Click(object sender, EventArgs e)
    {
        if (PicMain.Source == ImageSource.Null) return;

        try
        {
            var filePath = _images.GetFilePath(_currentIndex);
            PicMain.ClearMessage();


            using var p = new Process();
            p.StartInfo.FileName = "openwith";

            // Build the arguments
            p.StartInfo.Arguments = $"\"{filePath}\"";

            // show error dialog
            p.StartInfo.ErrorDialog = true;

            p.Start();
        }
        catch { }
    }

    private void MnuOpenLocation_Click(object sender, EventArgs e)
    {
        try
        {
            var filePath = _images.GetFilePath(_currentIndex);

            try
            {
                ExplorerApi.OpenFolderAndSelectItem(filePath);
            }
            catch
            {
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }
        catch { }
    }

    private void MnuCopyPath_Click(object sender, EventArgs e)
    {
        try
        {
            Clipboard.SetText(_images.GetFilePath(_currentIndex));

            PicMain.ShowMessage(Config.Language[$"FrmMain.{nameof(MnuCopyPath)}._Success"], Config.InAppMessageDuration);
        }
        catch { }
    }



    #endregion // Menu


}
