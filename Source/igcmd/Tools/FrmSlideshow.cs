/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Viewer;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Text;
using Timer = System.Windows.Forms.Timer;

namespace igcmd.Tools;

public partial class FrmSlideshow : ThemedForm
{
    private ImageGlassTool _igTool = new();
    private string _initImagePath;

    private CancellationTokenSource? _loadCancelTokenSrc = new();
    private MovableForm _movableForm;
    private ImageBooster _images = new();
    private int _currentIndex = -1;
    private IgMetadata? _currentMetadata = null;

    private Timer _slideshowTimer = new() { Enabled = false };
    private Stopwatch _slideshowStopwatch = new(); // slideshow stopwatch
    private float _slideshowCountdown = 5; // slideshow countdown interval
    private Rectangle _windowBound = new();
    private FormWindowState _windowState = FormWindowState.Normal;
    private int _numberImageChangeCount = 0;

    private CancellationTokenSource _hideCursorCancelToken = new();
    private bool _isCursorHidden = false;
    private bool _isColorPickerOpen = false;


    /// <summary>
    /// Hotkeys list of main menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> CurrentMenuHotkeys { get; set; } = new()
    {
        // Open context menu
	    { nameof(MnuContext),               new() { new(Keys.Alt | Keys.F) } },

        { nameof(MnuPauseResumeSlideshow),  new() { new(Keys.Space) } },

        // MnuNavigation
        { nameof(MnuViewNext),              new() { new (Keys.Right) } },
        { nameof(MnuViewPrevious),          new() { new (Keys.Left) } },
        { nameof(MnuGoToFirst),             new() { new (Keys.Home) } },
        { nameof(MnuGoToLast),              new() { new (Keys.End) } },

        { nameof(MnuWindowFit),             new() { new (Keys.F9) } },
        { nameof(MnuFrameless),             new() { new (Keys.F10) } },
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


    public FrmSlideshow(string initImagePath)
    {
        InitializeComponent();

        // update the DpiApi when DPI changed.
        EnableDpiApiUpdate = true;
        Config.Load();


        // load configs
        _initImagePath = initImagePath;

        Text = $"{Config.Language["FrmMain.MnuSlideshow"]} - {App.AppName}";
        SetUpFrmSlideshowConfigs();

        // initialize ImageGlassTool
        _ = ConnectToImageGlassAsync();


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


    // Protected methods
    #region Protected methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _slideshowTimer.Interval = 10; // support milliseconds
        _slideshowTimer.Tick += SlideshowTimer_Tick;


        // Initialize form movable
        #region Form movable
        _movableForm = new(this)
        {
            Key = Keys.ShiftKey | Keys.Shift,
        };

        // Enable form movable
        IG_SetWindowMoveable(true);
        #endregion // Form movable


        // full screen slideshow
        if (Config.EnableFullscreenSlideshow)
        {
            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit);


            // to hide the animation effect of window border
            FormBorderStyle = FormBorderStyle.None;

            // load window placement from settings here to save the initial
            // position of window so that when user exists the fullscreen mode,
            // it can be restore correctly
            WindowSettings.SetPlacementToWindow(this,
                WindowSettings.GetFrmMainPlacementFromConfig(SystemInformation.CaptionHeight, SystemInformation.CaptionHeight));

            IG_ToggleFullScreen(true);
        }
        // windowed slideshow
        else
        {
            // load window placement from settings
            WindowSettings.SetPlacementToWindow(this,
                WindowSettings.GetFrmMainPlacementFromConfig(SystemInformation.CaptionHeight, SystemInformation.CaptionHeight));

            // toggle frameless window
            IG_ToggleFrameless(Config.EnableFrameless, false);

            // toggle Window fit
            IG_ToggleWindowFit(Config.EnableWindowFit);
        }


        // load the init image
        _ = BHelper.RunAsThread(() => _ = LoadImageAsync(_initImagePath, _loadCancelTokenSrc));


        // load menu hotkeys
        CurrentMenuHotkeys = Config.GetAllHotkeys(CurrentMenuHotkeys);
        LoadMenuHotkeys();
        LoadMenuTagData();

        // load language
        LoadLanguage();

        // start slideshow
        SetSlideshowState(true);

        // focus on PicMain
        PicMain.Focus();
    }


    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        _igTool.Dispose();
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();
        style ??= Config.WindowBackdrop;
        var hasTransparency = EnableTransparent && style.Value != BackdropStyle.None;

        if (!hasTransparency)
        {
            BackColor = Config.SlideshowBackgroundColor.NoAlpha();
            WindowApi.SetTitleBar(Handle, Config.SlideshowBackgroundColor, Config.SlideshowBackgroundColor.InvertBlackOrWhite(220));
        }
        else
        {
            WindowApi.SetTitleBar(Handle, null, null);
        }


        // menu
        MnuContext.Theme = Config.Theme;


        // viewer
        PicMain.EnableTransparent = hasTransparency;
        PicMain.BackColor = Config.SlideshowBackgroundColor;
        PicMain.ForeColor = PicMain.BackColor.InvertBlackOrWhite(220);
        PicMain.AccentColor = WinColorsApi.GetAccentColor(true);
        PicMain.NavLeftImage = Config.Theme.Settings.NavButtonLeft;
        PicMain.NavRightImage = Config.Theme.Settings.NavButtonRight;

        PicMain.Web2DarkMode = darkMode;
        PicMain.Web2NavLeftImagePath = Config.Theme.NavLeftImagePath;
        PicMain.Web2NavRightImagePath = Config.Theme.NavRightImagePath;


        // set app logo on titlebar
        _ = Config.UpdateFormIcon(this);

        // update webview2 styles
        if (PicMain.UseWebview2) PicMain.UpdateWeb2Styles(darkMode);


        base.ApplyTheme(darkMode, style);
        ResumeLayout(false);
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        Config.Theme.LoadThemeColors();
        PicMain.AccentColor = SystemAccentColorChangedEventArgs.AccentColor;
        PicMain.Invalidate();

        // do not handle this event again in the parent class
        e.Handled = true;
        base.OnSystemAccentColorChanged(e);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        base.OnRequestUpdatingColorMode(e);

        // load the theme icons
        OnDpiChanged();

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void OnDpiChanged()
    {
        base.OnDpiChanged();
        SuspendLayout();

        // scale toolbar icons corresponding to DPI
        var newIconHeight = DpiApi.Scale(Config.ToolbarIconHeight);

        // reload theme
        Config.Theme.LoadToolbarIcons(newIconHeight);

        // update picmain scaling
        PicMain.NavButtonSize = this.ScaleToDpi(new SizeF(50f, 50f));
        PicMain.CheckerboardCellSize = this.ScaleToDpi(Const.VIEWER_GRID_SIZE);

        ResumeLayout(false);
    }


    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);

        MnuContext.CurrentDpi = e.DeviceDpiNew;
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // to fix arrow keys sometimes does not regconize
        if (keyData == Keys.Up
            || keyData == Keys.Down
            || keyData == Keys.Left
            || keyData == Keys.Right)
        {
            FrmSlideshow_KeyDown(this, new KeyEventArgs(keyData));

            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    #endregion // Protected methods



    // ImageGlassTool connection
    #region ImageGlassTool connection

    private async Task ConnectToImageGlassAsync()
    {
        _igTool.ToolMessageReceived += IgTool_ToolMessageReceived;
        _igTool.ToolClosingRequest += IgTool_ToolClosingRequest;
        await _igTool.ConnectAsync();
    }


    private void IgTool_ToolClosingRequest(object? sender, DisconnectedEventArgs e)
    {
        Close();
    }


    private void IgTool_ToolMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.MessageData)) return;


        // update image list
        if (e.MessageName.Equals(ImageGlassEvents.IMAGE_LIST_UPDATED, StringComparison.InvariantCultureIgnoreCase))
        {
            var data = IgImageListUpdatedEventArgs.Deserialize(e.MessageData);
            var newInitFile = !_initImagePath.Equals(data.InitFilePath, StringComparison.InvariantCultureIgnoreCase);

            _initImagePath = data.InitFilePath ?? _initImagePath;

            if (data != null && data.Files.Count > 0)
            {
                _ = BHelper.RunAsThread(async () =>
                {
                    // update the current image if it's not same
                    if (!string.IsNullOrEmpty(data.InitFilePath) && newInitFile)
                    {
                        _ = LoadImageAsync(_initImagePath, _loadCancelTokenSrc);
                    }

                    await LoadImageListAsync(data.Files, _initImagePath);

                    // enable slideshow
                    SetSlideshowState(true, false);
                });
            }

            return;
        }


        // update language
        if (e.MessageName.Equals(ImageGlassEvents.LANG_UPDATED, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Language = new IgLang(e.MessageData, App.StartUpDir(Dir.Language));
            LoadLanguage();
            return;
        }


        // update theme
        if (e.MessageName.Equals(ImageGlassEvents.THEME_UPDATED, StringComparison.InvariantCultureIgnoreCase))
        {
            Config.Theme = new IgTheme(e.MessageData, Config.ToolbarIconHeight);

            ApplyTheme(Config.Theme.Settings.IsDarkMode);
            return;
        }
    }

    #endregion // ImageGlass server connection


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
        var padding = DpiApi.Scale(30);
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
            Keys.Control => MouseWheelEvent.CtrlAndScroll,
            Keys.Shift => MouseWheelEvent.ShiftAndScroll,
            Keys.Alt => MouseWheelEvent.AltAndScroll,
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
                case MouseWheelEvent.CtrlAndScroll:
                    action = MouseWheelAction.PanVertically;
                    break;
                case MouseWheelEvent.ShiftAndScroll:
                    action = MouseWheelAction.PanHorizontally;
                    break;
                case MouseWheelEvent.AltAndScroll:
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
        if (e.Button == MouseButtons.Right)
        {
            // handle right-click action for webview2
            if (PicMain.UseWebview2)
            {
                var point = this.PointToScreen(e.Location);
                point.X += PicMain.Left;
                point.Y += PicMain.Top;

                MnuContext.Show(point);
            }
        }


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
        // Handle window fit after zoom change
        if (Config.EnableWindowFit && !e.IsPreviewingImage && (e.IsManualZoom || e.IsZoomModeChange))
        {
            FitWindowToImage(e.ChangeSource == ZoomChangeSource.ZoomMode);
        }

        LoadImageInfo(ImageInfoUpdateTypes.Zoom);
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


    private void PicMain_MouseLeave(object sender, EventArgs e)
    {
        Cursor.Show();
        _isCursorHidden = false;
    }


    private void PicMain_Web2NavigationCompleted(object sender, EventArgs e)
    {
        var langJson = BHelper.ToJson(Config.Language);
        _ = PicMain.LoadWeb2LanguageAsync(langJson);
    }


    private void PicMain_Web2PointerDown(object sender, MouseEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }

        // make sure all menus closed when mouse clicked
        MnuContext.Close();
    }


    private void PicMain_Web2KeyDown(object sender, KeyEventArgs e)
    {
        // pass keydown to FrmMain
        this.OnKeyDown(e);
    }


    private void PicMain_Web2KeyUp(object sender, KeyEventArgs e)
    {
        // pass keyup to FrmMain
        this.OnKeyUp(e);
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
                MaxImageDimensionToCache = Const.MAX_IMAGE_DIMENSION,
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

            LoadImageInfo();
        });
    }


    /// <summary>
    /// View the next image
    /// </summary>
    private async Task ViewNextImageAsync(int step = 0)
    {
        _loadCancelTokenSrc?.Cancel();
        _loadCancelTokenSrc = new();


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


        await LoadImageAsync(null, _loadCancelTokenSrc);
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

        IgPhoto? photo = null;
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

        var imgFilePath = filePath;
        if (string.IsNullOrWhiteSpace(imgFilePath))
        {
            imgFilePath = _images.GetFilePath(_currentIndex);
        }
        else
        {
            photo = new IgPhoto(imgFilePath);
        }


        try
        {
            // get metadata
            _currentMetadata = PhotoCodec.LoadMetadata(imgFilePath, readSettings);

            // check if we should use Webview2 viewer
            var useWebview2 = Config.UseWebview2ForSvg
                && imgFilePath.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase);

            // on image loading
            OnImageLoading();


            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();


            // if we are using Webview2
            if (useWebview2)
            {
                photo = new IgPhoto(imgFilePath)
                {
                    Metadata = _currentMetadata,
                };
            }
            else
            {
                // directly load the image file, skip image list
                if (photo != null)
                {
                    await photo.LoadAsync(readSettings, tokenSrc);
                }
                else
                {
                    photo = await _images.GetAsync(_currentIndex, tokenSrc: tokenSrc);
                }
            }


            // check if loading is cancelled
            tokenSrc?.Token.ThrowIfCancellationRequested();

            // on image loaded
            OnImageLoaded(photo, useWebview2);
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
        PicMain.ShowMessage(Config.Language[$"FrmMain._Loading"], null, delayMs: 1500);


        LoadImageInfo(null, _currentMetadata.FilePath);
    }


    private void OnImageLoaded(IgPhoto photo, bool useWebview2)
    {
        if (InvokeRequired)
        {
            OnImageLoaded(photo, useWebview2);
            return;
        }

        var error = photo.Error;

        // if image needs to display in Webview2 viweer
        if (useWebview2)
        {
            PicMain.ClearMessage();

            try
            {
                _ = PicMain.SetImageWeb2Async(photo, _loadCancelTokenSrc.Token);
            }
            catch (Exception ex) { error = ex; }
        }


        // image error
        if (error != null)
        {
            PicMain.SetImage(null);

            var emoji = BHelper.IsOS(WindowsOS.Win11OrLater) ? "🥲" : "🙄";
            var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            var appVersion = App.Version + $" ({archInfo}, .NET {Environment.Version})";

            var debugInfo = $"ImageGlass {Const.APP_CODE.CapitalizeFirst()} v{appVersion}" +
                $"\r\n{ImageMagick.MagickNET.Version}" +
                $"\r\n" +
                $"\r\nℹ️ Error details:" +
                $"\r\n";

            var errorLines = error.StackTrace?.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Take(2) ?? [];
            var errDetails = error.Message + "\r\n\r\n" + string.Join("\r\n", errorLines);

            PicMain.ShowMessage(debugInfo +
                error.Source + ": " + errDetails,
                Config.Language[$"FrmMain.PicMain._ErrorText"] + $" {emoji}");
        }
        // use native viewer to display image
        else if (!(photo?.ImgData.IsImageNull ?? true))
        {
            var isImageBigForFading = photo.Metadata.RenderedWidth > 8000
                || photo.Metadata.RenderedHeight > 8000;
            var enableFading = !isImageBigForFading && Config.EnableImageTransition;

            // set the main image
            PicMain.SetImage(photo.ImgData,
                resetZoom: true,
                enableFading: enableFading,
                initOpacity: 0.4f,
                opacityStep: 0.02f);

            // update window fit
            if (Config.EnableWindowFit)
            {
                FitWindowToImage(false);
            }

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


        // load image info
        LoadImageInfo(ImageInfoUpdateTypes.Dimension | ImageInfoUpdateTypes.FrameCount);


        // notify image changes
        if (Config.SlideshowImagesToNotifySound > 0)
        {
            if (_numberImageChangeCount >= Config.SlideshowImagesToNotifySound - 1)
            {
                SystemSounds.Asterisk.Play();
                _numberImageChangeCount = 0;
            }
            else
            {
                _numberImageChangeCount++;
            }
        }

        // Collect system garbage
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    #endregion // Load image


    // Slideshow methods
    #region Slideshow methods

    private void FrmSlideshow_KeyDown(object sender, KeyEventArgs e)
    {
        if (_isCursorHidden)
        {
            Cursor.Show();
            _isCursorHidden = false;
        }


        var hotkey = new Hotkey(e.KeyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // open context menu
        if (actions.Contains(nameof(MnuContext)))
        {
            MnuContext.Show(this, (PicMain.Width - MnuContext.Width) / 2, (PicMain.Height - MnuContext.Height) / 2);
            return;
        }


        #region Register and run CONTEXT MENU shortcuts

        bool CheckMenuShortcut(ToolStripMenuItem mnu)
        {
            var menuHotkeyList = Config.GetHotkey(CurrentMenuHotkeys, mnu.Name);
            var menuHotkey = menuHotkeyList.SingleOrDefault(k => k.KeyData == e.KeyData);

            if (menuHotkey != null)
            {
                // ignore invisible menu
                if (mnu.Visible) return false;

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
            if (CheckMenuShortcut(item)) return;
        }
        #endregion
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
                if (!Config.EnableLoopSlideshow)
                {
                    // pause slideshow
                    SetSlideshowState(false);
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
    /// Randomize slideshow interval in seconds
    /// </summary>
    private static float RandomizeSlideshowInterval()
    {
        var intervalTo = Config.UseRandomIntervalForSlideshow ? Config.SlideshowIntervalTo : Config.SlideshowInterval;

        var interval = (Random.Shared.NextSingle() * (intervalTo - Config.SlideshowInterval) + Config.SlideshowInterval);

        return interval;
    }

    #endregion Slideshow methods


    /// <summary>
    /// Gets image info in status bar.
    /// </summary>
    private void LoadImageInfo(ImageInfoUpdateTypes? types = null, string? filePath = null)
    {
        if (InvokeRequired)
        {
            Invoke(LoadImageInfo, types, filePath);
            return;
        }

        var updateAll = ImageInfo.IsNull || types == null;


        // AppName
        if (Config.ImageInfoTags.Contains(nameof(ImageInfo.AppName)))
        {
            ImageInfo.AppName = App.AppName;
        }
        else
        {
            ImageInfo.AppName = string.Empty;
        }

        // Zoom
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.Zoom))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.Zoom)))
            {
                ImageInfo.Zoom = $"{Math.Round(PicMain.ZoomFactor * 100, 2):n0}%";
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
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ListCount))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ListCount))
                && _images.Length > 0)
            {
                var listInfo = new StringBuilder(3);
                listInfo.Append(_currentIndex + 1);
                listInfo.Append('/');
                listInfo.Append(_images.Length);

                ImageInfo.ListCount = string.Format(
                    Config.Language[$"_.{nameof(ImageInfo)}._{nameof(ImageInfo.ListCount)}"],
                    listInfo.ToString());
            }
            else
            {
                ImageInfo.ListCount = string.Empty;
            }
        }

        // Name
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.Name))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.Name)))
            {
                ImageInfo.Name = Path.GetFileName(filePath);
            }
            else
            {
                ImageInfo.Name = string.Empty;
            }
        }

        // Path
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.Path))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.Path)))
            {
                ImageInfo.Path = filePath;
            }
            else
            {
                ImageInfo.Path = string.Empty;
            }
        }

        // FileSize
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.FileSize))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.FileSize))
                && _currentMetadata != null)
            {
                ImageInfo.FileSize = _currentMetadata.FileSizeFormated;
            }
            else
            {
                ImageInfo.FileSize = string.Empty;
            }
        }

        // FrameCount
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.FrameCount))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.FrameCount))
                && _currentMetadata != null
                && _currentMetadata.FrameCount > 1)
            {
                ImageInfo.FrameCount = string.Format(
                    Config.Language[$"_.{nameof(ImageInfo)}._{nameof(ImageInfo.FrameCount)}"],
                    _currentMetadata.FrameCount);
            }
            else
            {
                ImageInfo.FrameCount = string.Empty;
            }
        }

        // Dimension
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.Dimension))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.Dimension))
                && _currentMetadata != null)
            {
                ImageInfo.Dimension = $"{_currentMetadata.RenderedWidth} x {_currentMetadata.RenderedHeight} px";
            }
            else
            {
                ImageInfo.Dimension = string.Empty;
            }
        }

        // ModifiedDateTime
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ModifiedDateTime))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ModifiedDateTime))
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
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ExifRating))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ExifRating))
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
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ExifDateTime))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ExifDateTime))
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
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ExifDateTimeOriginal))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ExifDateTimeOriginal))
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
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.DateTimeAuto))
        {
            var dtStr = string.Empty;

            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.DateTimeAuto))
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

        // ColorSpace
        if (updateAll || types!.Value.HasFlag(ImageInfoUpdateTypes.ColorSpace))
        {
            if (Config.ImageInfoTags.Contains(nameof(ImageInfo.ColorSpace))
                && _currentMetadata != null
                && !string.IsNullOrEmpty(_currentMetadata.ColorSpace))
            {
                var colorProfile = !string.IsNullOrEmpty(_currentMetadata.ColorProfile)
                    ? _currentMetadata.ColorProfile
                    : "-";

                if (_currentMetadata.ColorSpace.Equals(colorProfile, StringComparison.InvariantCultureIgnoreCase))
                {
                    ImageInfo.ColorSpace = _currentMetadata.ColorSpace;
                }
                else
                {
                    ImageInfo.ColorSpace = $"{_currentMetadata.ColorSpace}/{colorProfile}";
                }
            }
            else
            {
                ImageInfo.ColorSpace = string.Empty;
            }
        }



        Text = ImageInfo.ToString(Config.ImageInfoTags, false);
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
        MnuWindowFit.Text = lang[$"FrmMain.{nameof(MnuWindowFit)}"];
        MnuFrameless.Text = lang[$"FrmMain.{nameof(MnuFrameless)}"];
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


    // Load menu settings
    #region Load menu settings

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
    /// Load Loading order menu items
    /// </summary>
    private void LoadMnuLoadingOrdersSubItems()
    {
        // clear items
        MnuLoadingOrders.DropDown.Items.Clear();

        var newMenuIconHeight = DpiApi.Scale(Const.MENU_ICON_HEIGHT);

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

    #endregion // Load menu settings


    // Menu events
    #region Menu events

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

    private void MnuWindowFit_Click(object sender, EventArgs e)
    {
        IG_ToggleWindowFit();
    }

    /// <summary>
    /// Toggles Window fit.
    /// </summary>
    public bool IG_ToggleWindowFit(bool? enable = null)
    {
        enable ??= !Config.EnableWindowFit;
        Config.EnableWindowFit = enable.Value;

        if (Config.EnableWindowFit)
        {
            // exit full screen
            if (Config.EnableFullScreen)
            {
                IG_ToggleFullScreen(false);
            }
        }

        // set Window Fit mode
        FitWindowToImage();

        // update menu item state
        MnuWindowFit.Checked = Config.EnableWindowFit;


        return Config.EnableWindowFit;
    }

    public void FitWindowToImage(bool resetZoomMode = true)
    {
        if (!Config.EnableWindowFit || PicMain.Source == ImageSource.Null)
            return; // Nothing to do

        WindowState = FormWindowState.Normal;

        // get the gap of the window to the viewer control
        // it includes boder, titlebar, toolbar, gallery,...
        var horzGap = Width - ClientSize.Width;
        var vertGap = Height - ClientSize.Height;

        // get current screen
        var workingArea = Screen.FromControl(this).WorkingArea;

        // get source image size
        var srcImgW = PicMain.SourceWidth;
        var srcImgH = PicMain.SourceHeight;

        // get zoom factor
        var zoomFactor = PicMain.ZoomFactor;
        if (resetZoomMode)
        {
            if (Config.ZoomMode == ZoomMode.LockZoom)
            {
                PicMain.SetZoomFactor(Config.ZoomLockValue / 100f, false);
            }
            else
            {
                var maxViewerWidth = workingArea.Width - horzGap;
                var maxViewerHeight = workingArea.Height - vertGap;

                // recalculate zoom factor for the new size
                zoomFactor = PicMain.CalculateZoomFactor(Config.ZoomMode, srcImgW, srcImgH, maxViewerWidth, maxViewerHeight);
            }
        }

        // get image size after zoomed
        var zoomImgW = (int)(srcImgW * zoomFactor);
        var zoomImgH = (int)(srcImgH * zoomFactor);

        // adjust the viewer size to fit the entire image
        // but not larger than desktop working area.
        var viewerBound = new Rectangle()
        {
            Width = Math.Min(zoomImgW, workingArea.Width - horzGap),
            Height = Math.Min(zoomImgH, workingArea.Height - vertGap),
        };

        // adjust viewer size and position to the desktop working area
        viewerBound = BHelper.CenterRectToRect(viewerBound,
            new Rectangle(
                workingArea.X + horzGap / 2,
                workingArea.Y + vertGap / 2,
                workingArea.Width - horzGap,
                workingArea.Height - vertGap),
            true);

        // add the gaps to make window bound
        var winBound = new Rectangle(
            viewerBound.X - horzGap / 2,
            viewerBound.Y - vertGap / 2,
            viewerBound.Width + horzGap,
            viewerBound.Height + vertGap);

        // check center window to screen option
        if (!Config.CenterWindowFit)
        {
            winBound.X = Left;
            winBound.Y = Top;
        }


        // set min size for window
        MinimumSize = new()
        {
            Width = horzGap + this.ScaleToDpi(50),
            Height = vertGap + this.ScaleToDpi(50),
        };

        // update window position and size
        SetBounds(winBound.X, winBound.Y, winBound.Width, winBound.Height, BoundsSpecified.All);

        if (resetZoomMode)
        {
            PicMain.SetZoomFactor(zoomFactor, false);
        }
    }


    private void MnuFrameless_Click(object sender, EventArgs e)
    {
        IG_ToggleFrameless();
    }

    /// <summary>
    /// Toggle framless mode
    /// </summary>
    private bool IG_ToggleFrameless(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableFrameless;
        Config.EnableFrameless = enable.Value;

        if (Config.EnableWindowFit)
        {
            // exit full screen
            if (Config.EnableFullScreen)
            {
                IG_ToggleFullScreen(false);
            }
        }

        // set frameless mode
        FormBorderStyle = Config.EnableFrameless ? FormBorderStyle.None : FormBorderStyle.Sizable;

        // update menu item state
        MnuFrameless.Checked = Config.EnableFrameless;

        if (showInAppMessage)
        {
            var langPath = $"FrmMain.{nameof(MnuFrameless)}";

            if (Config.EnableFrameless)
            {
                WindowApi.SetRoundCorner(Handle);

                PicMain.ShowMessage(
                    string.Format(Config.Language[$"{langPath}._EnableDescription"], MnuFrameless.ShortcutKeyDisplayString),
                    Config.InAppMessageDuration);
            }
        }

        return Config.EnableFrameless;
    }

    /// <summary>
    /// Sets <see cref="FrmSlideshow"/> movable state
    /// </summary>
    public void IG_SetWindowMoveable(bool? enable = null)
    {
        enable ??= true;
        _movableForm.Key = Keys.ShiftKey | Keys.Shift;

        if (enable.Value)
        {
            _movableForm.Enable();
            _movableForm.Enable(PicMain);
        }
        else
        {
            _movableForm.Disable();
            _movableForm.Disable(PicMain);
        }

    }

    private void MnuFullScreen_Click(object sender, EventArgs e)
    {
        IG_ToggleFullScreen();
    }

    /// <summary>
    /// Toggles full screen mode.
    /// </summary>
    public bool IG_ToggleFullScreen(bool? enable = null)
    {
        enable ??= !Config.EnableFullscreenSlideshow;
        Config.EnableFullscreenSlideshow = enable.Value;

        if (Config.EnableFullscreenSlideshow)
        {
            // exit full screen
            if (Config.EnableWindowFit)
            {
                IG_ToggleWindowFit(false);
            }

            // exit frameless
            if (Config.EnableFrameless)
            {
                IG_ToggleFrameless(false);
            }
        }

        // disable round corner in full screen
        WindowApi.SetRoundCorner(Handle, Config.EnableFullscreenSlideshow ? WindowCorner.DoNotRound : WindowCorner.Round);

        SetFullScreenMode(enable.Value);

        // update menu item state
        MnuFullScreen.Checked = Config.EnableFullscreenSlideshow;

        return Config.EnableFullscreenSlideshow;
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

            Visible = true;

            // disable free moving
            IG_SetWindowMoveable(false);
        }

        // exit full screen
        else
        {
            // renable free moving
            IG_SetWindowMoveable(true);


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

            _ = Config.UpdateFormIcon(this);
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
        Config.ShowCheckerboard = !Config.ShowCheckerboard;
        MnuToggleCheckerboard.Checked = Config.ShowCheckerboard;

        if (Config.ShowCheckerboard)
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

        using var cd = new ModernColorDialog()
        {
            StartPosition = FormStartPosition.CenterParent,
            ColorValue = Config.SlideshowBackgroundColor,
        };

        if (cd.ShowDialog() == DialogResult.OK)
        {
            PicMain.BackColor = Config.SlideshowBackgroundColor = cd.ColorValue;
            PicMain.ForeColor = PicMain.BackColor.InvertBlackOrWhite(220);


            if (EnableTransparent && BackdropStyle != BackdropStyle.None)
            {
                WindowApi.SetTitleBar(Handle, null, null);
            }
            else
            {
                BackColor = PicMain.BackColor.NoAlpha(); ;
                WindowApi.SetTitleBar(Handle, PicMain.BackColor, PicMain.ForeColor);
            }

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

    #endregion // Menu events

}
