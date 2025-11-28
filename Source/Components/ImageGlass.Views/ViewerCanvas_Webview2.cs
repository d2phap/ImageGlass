/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Dynamic;

namespace ImageGlass.Viewer;


public partial class ViewerCanvas
{
    private bool _isWeb2NavigationDone = false;
    private Web2? _web2 = null;
    private bool _web2DarkMode = true;
    private string _web2NavLeftImagePath = string.Empty;
    private string _web2NavRightImagePath = string.Empty;
    private string _web2CompareSliderHandlePath = string.Empty;
    private MouseEventArgs? _web2PointerDownEventArgs = null;
    private RectangleF _web2DestRect = RectangleF.Empty;
    private bool _isWeb2ComparisonModeActive = false;
    private Keys _web2LastMouseWheelModifiers = Keys.None;


    // Properties
    #region Properties

    /// <summary>
    /// Gets the <see cref="Web2"/> instance.
    /// </summary>
    private Web2 Web2 => _web2;


    /// <summary>
    /// Gets value indicates that <see cref="Web2"/> is ready to use.
    /// </summary>
    private bool IsWeb2Ready => Web2 != null && Web2.IsWeb2Ready;


    /// <summary>
    /// Gets, sets value indicates that the <see cref="ViewerCanvas"/>
    /// should use <see cref="Web2"/> to render the image.
    /// </summary>
    public bool UseWebview2 => _imageSource == ImageSource.Webview2;


    /// <summary>
    /// Gets whether WebView2 is handling comparison rendering (for SVG).
    /// </summary>
    public bool IsWeb2ComparisonModeActive => _isWeb2ComparisonModeActive;


    /// <summary>
    /// Gets modifier keys from the last WebView2 mouse wheel event.
    /// </summary>
    public Keys Web2LastMouseWheelModifiers => _web2LastMouseWheelModifiers;


    /// <summary>
    /// Gets, sets value of dark mode of <see cref="Web2"/>.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Web2DarkMode
    {
        get => _web2DarkMode;
        set
        {
            _web2DarkMode = value;
            if (Web2 != null)
            {
                Web2.DarkMode = value;
            }
        }
    }


    /// <summary>
    /// Gets, sets the left navigation image path for <see cref="Web2"/>.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Web2NavLeftImagePath
    {
        get => _web2NavLeftImagePath;
        set
        {
            _web2NavLeftImagePath = value;
            SetWeb2NavButtonStyles();
        }
    }


    /// <summary>
    /// Gets, sets the right navigation image path for <see cref="Web2"/>.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Web2NavRightImagePath
    {
        get => _web2NavRightImagePath;
        set
        {
            _web2NavRightImagePath = value;
            SetWeb2NavButtonStyles();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Web2CompareSliderHandlePath
    {
        get => _web2CompareSliderHandlePath;
        set => _web2CompareSliderHandlePath = value;
    }

    #endregion // Properties



    // Public events
    #region Public events

    /// <summary>
    /// Occurs when <see cref="Web2"/> navigation is completed.
    /// </summary>
    public event EventHandler<EventArgs> Web2NavigationCompleted;

    /// <summary>
    /// Occurs when <see cref="Web2"/> received <c>pointerdown</c> event.
    /// </summary>
    public event EventHandler<MouseEventArgs> Web2PointerDown;

    /// <summary>
    /// Occurs when <see cref="Web2"/> received <c>keydown</c> event.
    /// </summary>
    public event EventHandler<KeyEventArgs> Web2KeyDown;

    /// <summary>
    /// Occurs when <see cref="Web2"/> received <c>keyup</c> event.
    /// </summary>
    public event EventHandler<KeyEventArgs> Web2KeyUp;

    #endregion // Public events



    // Web2 events
    #region Web2 events

    private void Web2_Web2Ready(object? sender, EventArgs e)
    {
        var htmlFilePath = App.StartUpDir(Dir.WebUI, "DXCanvas_Webview2.html");
        Web2.Source = new Uri(htmlFilePath);
    }


    private void Web2_Web2NavigationCompleted(object? sender, EventArgs e)
    {
        _isWeb2NavigationDone = true;
        Web2.DarkMode = Web2DarkMode;
        Web2.AccentColor = AccentColor;
        SetWeb2NavButtonStyles();

        Web2NavigationCompleted?.Invoke(this, EventArgs.Empty);
    }


    private void Web2_Web2KeyDown(object? sender, KeyEventArgs e)
    {
        Web2KeyDown?.Invoke(this, e);
    }


    private void Web2_Web2KeyUp(object? sender, KeyEventArgs e)
    {
        Web2KeyUp?.Invoke(this, e);
    }


    private void Web2_Web2ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        if (_web2PointerDownEventArgs == null)
        {
            _web2PointerDownEventArgs ??= new(MouseButtons.Right, 1, e.Location.X, e.Location.Y, 0);
        }
        else
        {
            _web2PointerDownEventArgs = new(
                MouseButtons.Right,
                _web2PointerDownEventArgs.Clicks,
                _web2PointerDownEventArgs.X,
                _web2PointerDownEventArgs.Y,
                _web2PointerDownEventArgs.Delta
            );
        }

        base.OnMouseClick(_web2PointerDownEventArgs);

        _web2PointerDownEventArgs = null;
    }


    private void Web2_Web2MessageReceived(object? sender, Web2MessageReceivedEventArgs e)
    {
        if (e.Name == Web2FrontendMsgNames.ON_ZOOM_CHANGED)
        {
            var zoomEventArgs = ParseZoomEventJson(e.Data);
            OnZoomChanged?.Invoke(this, zoomEventArgs);
        }
        else if (e.Name == Web2FrontendMsgNames.ON_POINTER_DOWN)
        {
            _web2PointerDownEventArgs = ViewerCanvas.ParseMouseEventJson(e.Data);
            Web2PointerDown?.Invoke(this, _web2PointerDownEventArgs);
        }
        else if (e.Name == Web2FrontendMsgNames.ON_MOUSE_WHEEL)
        {
            var mouseWheelEventArgs = ViewerCanvas.ParseMouseEventJson(e.Data);
            _web2LastMouseWheelModifiers = ViewerCanvas.ParseModifierKeysFromJson(e.Data);
            this.OnMouseWheel(mouseWheelEventArgs);
        }
        else if (e.Name == Web2FrontendMsgNames.ON_CONTENT_SIZE_CHANGED)
        {
            _web2DestRect = ViewerCanvas.ParseContentSizeChangedEventJson(e.Data);
            if (!_web2DestRect.IsEmpty)
            {
                this.Invalidate();
            }
        }
        else if (e.Name == Web2FrontendMsgNames.ON_FILE_DROP)
        {
            var filePaths = e.AdditionalObjects.Where(i => i is CoreWebView2File)
                .Select(i => (i as CoreWebView2File).Path)
                .ToArray();

            var dataObj = new DataObject(DataFormats.FileDrop, filePaths);
            var args = new DragEventArgs(dataObj, 0, 0, 0, DragDropEffects.All, DragDropEffects.Link);
            this.OnDragDrop(args);
        }
        else if (e.Name == Web2FrontendMsgNames.ON_NAV_CLICK)
        {
            var pointerEventArgs = ViewerCanvas.ParseMouseEventJson(e.Data);
            var dict = BHelper.ParseJson<ExpandoObject>(e.Data)
                .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

            if (dict.TryGetValue("NavigationButton", out var navBtn))
            {
                if (navBtn.Equals("left", StringComparison.InvariantCultureIgnoreCase))
                {
                    OnNavLeftClicked?.Invoke(this, pointerEventArgs);
                }
                else if (navBtn.Equals("right", StringComparison.InvariantCultureIgnoreCase))
                {
                    OnNavRightClicked?.Invoke(this, pointerEventArgs);
                }
            }
        }
        // Handle WebView2 comparison slider changes
        else if (e.Name == Web2FrontendMsgNames.ON_COMPARISON_SLIDER_CHANGED)
        {
            var dict = BHelper.ParseJson<ExpandoObject>(e.Data)
                .ToDictionary(i => i.Key, i => i.Value?.ToString() ?? string.Empty);

            if (dict.TryGetValue("SliderPosition", out var posStr)
                && float.TryParse(posStr, out var pos))
            {
                // Update D2D slider position to stay in sync (without triggering another event)
                _comparisonSliderPos = Math.Clamp(pos, 0f, 1f);
                Invalidate();
            }
        }
        // Handle WebView2 comparison pane file drops
        else if (e.Name == Web2FrontendMsgNames.ON_COMPARISON_PANE_DROP)
        {
            var dict = BHelper.ParseJson<ExpandoObject>(e.Data)
                .ToDictionary(i => i.Key, i => i.Value?.ToString() ?? string.Empty);

            var filePaths = e.AdditionalObjects.Where(i => i is CoreWebView2File)
                .Select(i => (i as CoreWebView2File).Path)
                .ToArray();

            if (dict.TryGetValue("Pane", out var paneStr) && filePaths.Length > 0)
            {
                var pane = paneStr.Equals("left", StringComparison.OrdinalIgnoreCase)
                    ? ComparisonPaneHover.LeftPane
                    : ComparisonPaneHover.RightPane;

                ComparisonPaneFileDrop?.Invoke(this, new ComparisonPaneDropEventArgs(pane, filePaths[0]));
            }
        }
    }

    #endregion // Web2 events



    // Public methods
    #region Public methods

    /// <summary>
    /// Loads the image file into <see cref="Web2"/>.
    /// </summary>
    public async Task SetImageWeb2Async(IgPhoto? data, CancellationToken token = default)
    {
        if (data == null) return;

        try
        {
            await InitializeWeb2Async();

            // release native resources
            SetImage(null);

            // load image data for web2
            LoadImageDataWeb2(data?.Metadata);

            string msgName;
            var obj = new ExpandoObject();
            _ = obj.TryAdd("ZoomMode", ZoomMode.ToString());
            _ = obj.TryAdd("ZoomFactor", ZoomFactor);
            _ = obj.TryAdd("FilePath", data.FilePath);

            // make sure dir path contains separator
            var dirPath = Path.GetDirectoryName(data.FilePath);
            if (!Path.EndsInDirectorySeparator(dirPath.AsSpan()))
            {
                dirPath += Path.DirectorySeparatorChar;
            }
            _ = obj.TryAdd("DirPath", dirPath);


            // if image file is SVG, we read its content
            if (!string.IsNullOrWhiteSpace(data.FilePath)
                && data.FilePath.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase))
            {
                var textContent = await File.ReadAllTextAsync(data.FilePath, token);
                _ = obj.TryAdd("Html", textContent);
                msgName = Web2BackendMsgNames.SET_HTML;
            }
            else
            {
                _ = obj.TryAdd("Url", data.FilePath);
                msgName = Web2BackendMsgNames.SET_IMAGE;
            }


            // wait for the Web2 navigation is completed
            while (!_isWeb2NavigationDone)
            {
                await Task.Delay(10, token);
            }

            token.ThrowIfCancellationRequested();

            var json = BHelper.ToJson(obj);
            Web2.PostWeb2Message(msgName, json);

            // make sure it's focus to receive keydown events
            this.Focus();
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException) { }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    /// <summary>
    /// Updates language of <see cref="Web2"/>.
    /// </summary>
    public async Task LoadWeb2LanguageAsync(string langJson)
    {
        await Web2.ExecuteScriptAsync($"""
            window._page.lang = {langJson};
            window._page.loadLanguage();
        """);
    }


    /// <summary>
    /// Sets styles for Web2.
    /// </summary>
    public void UpdateWeb2Styles(bool isDarkMode)
    {
        Web2.AccentColor = AccentColor;
        Web2.DarkMode = isDarkMode;
        SetWeb2NavButtonStyles();
    }

    #endregion // Public methods



    // Private methods
    #region Private methods

    /// <summary>
    /// Initializes <see cref="Web2"/> control, adds it into the <see cref="ViewerCanvas"/> control.
    /// </summary>
    private async Task InitializeWeb2Async()
    {
        if (InvokeRequired)
        {
            await Invoke(InitializeWeb2Async);
            return;
        }

        if (_web2 != null) return;

        _web2 = new Web2();
        _isWeb2NavigationDone = false;

        ((System.ComponentModel.ISupportInitialize)Web2).BeginInit();
        SuspendLayout();

        Web2.AllowExternalDrop = true;
        Web2.CreationProperties = null;
        Web2.Name = nameof(Web2);
        Web2.Size = Size;
        Web2.Dock = DockStyle.Fill;
        Web2.ZoomFactor = 1D;
        Web2.Visible = true;
        Web2.EnableDebug = EnableDebug;

        Web2.Web2Ready += Web2_Web2Ready;
        Web2.Web2NavigationCompleted += Web2_Web2NavigationCompleted;
        Web2.Web2MessageReceived += Web2_Web2MessageReceived;
        Web2.Web2KeyDown += Web2_Web2KeyDown;
        Web2.Web2KeyUp += Web2_Web2KeyUp;
        Web2.Web2ContextMenuRequested += Web2_Web2ContextMenuRequested;


        try
        {
            Controls.Add(Web2);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}\r\n\r\n at {nameof(InitializeWeb2Async)}() method", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ((System.ComponentModel.ISupportInitialize)Web2).EndInit();
        ResumeLayout(false);

        await Web2.EnsureWeb2Async();
    }


    /// <summary>
    /// Dispose <see cref="Web2"/> resources.
    /// </summary>
    private void DisposeWeb2Control()
    {
        Controls.Remove(_web2);

        if (_web2 != null)
        {
            _web2.Web2Ready -= Web2_Web2Ready;
            _web2.Web2NavigationCompleted -= Web2_Web2NavigationCompleted;
            _web2.Web2KeyDown -= Web2_Web2KeyDown;
            _web2.Web2MessageReceived -= Web2_Web2MessageReceived;
            _web2.Dispose();
            _web2 = null;
        }
    }


    /// <summary>
    /// Loads image data for Webview2.
    /// </summary>
    private void LoadImageDataWeb2(IgMetadata? imgData)
    {
        Source = ImageSource.Webview2;
        SourceWidth = imgData?.RenderedWidth ?? 0;
        SourceHeight = imgData?.RenderedHeight ?? 0;
        CanImageAnimate = false;
        HasAlphaPixels = true;
    }


    /// <summary>
    /// Parses JSON string to <see cref="ZoomEventArgs"/>.
    /// </summary>
    private ZoomEventArgs ParseZoomEventJson(string json)
    {
        var isZoomModeChanged = false;
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

        if (dict.TryGetValue(nameof(ZoomEventArgs.ZoomFactor), out var zoomFactor))
        {
            _ = float.TryParse(zoomFactor, out _zoomFactor);
        }
        if (dict.TryGetValue(nameof(ZoomEventArgs.IsManualZoom), out var isManualZoom))
        {
            _isManualZoom = isManualZoom.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
        if (dict.TryGetValue(nameof(ZoomEventArgs.IsZoomModeChange), out var zoomModeChanged))
        {
            isZoomModeChanged = zoomModeChanged.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        return new ZoomEventArgs()
        {
            ZoomFactor = _zoomFactor,
            IsManualZoom = _isManualZoom,
            IsZoomModeChange = isZoomModeChanged,
            IsPreviewingImage = false,
            ChangeSource = ZoomChangeSource.Unknown,
        };
    }


    /// <summary>
    /// Parses JSON string to <see cref="RectangleF"/>.
    /// </summary>
    private static RectangleF ParseContentSizeChangedEventJson(string json)
    {
        var rect = new RectangleF();
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

        // save the dest rect of Web2
        var dpi = DpiApi.DpiScale;
        if (dict.TryGetValue("Dpi", out var dpiStr))
        {
            _ = float.TryParse(dpiStr, out dpi);
        }
        if (dict.TryGetValue("X", out var xStr))
        {
            _ = float.TryParse(xStr, out var x);
            rect.X = x * dpi;
        }
        if (dict.TryGetValue("Y", out var yStr))
        {
            _ = float.TryParse(yStr, out var y);
            rect.Y = y * dpi;
        }
        if (dict.TryGetValue("Width", out var widthStr))
        {
            _ = float.TryParse(widthStr, out var width);
            rect.Width = width * dpi;
        }
        if (dict.TryGetValue("Height", out var heightStr))
        {
            _ = float.TryParse(heightStr, out var height);
            rect.Height = height * dpi;
        }

        return rect;
    }


    /// <summary>
    /// Parses JSON string to <see cref="MouseEventArgs"/>.
    /// </summary>
    private static MouseEventArgs ParseMouseEventJson(string json)
    {
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

        var dpi = DpiApi.DpiScale;
        var x = 0f;
        var y = 0f;
        var delta = 0d;
        var button = MouseButtons.Left;

        if (dict.TryGetValue("Dpi", out var dpiStr))
        {
            _ = float.TryParse(dpiStr, out dpi);
        }
        if (dict.TryGetValue("X", out var xStr))
        {
            _ = float.TryParse(xStr, out x);
        }
        if (dict.TryGetValue("Y", out var yStr))
        {
            _ = float.TryParse(yStr, out y);
        }
        if (dict.TryGetValue("Delta", out var deltaStr))
        {
            if (double.TryParse(deltaStr, out delta))
            {
                // delta direction is opposite
                delta *= -1;
                if (delta <= -100 || delta >= 100)
                {
                    delta = Math.CopySign(SystemInformation.MouseWheelScrollDelta, delta);
                }
            }
        }
        if (dict.TryGetValue("Button", out var buttonStr))
        {
            _ = int.TryParse(buttonStr, out var btnIndex);

            if (btnIndex == 1) button = MouseButtons.Middle;
            else if (btnIndex == 2) button = MouseButtons.Right;
            else if (btnIndex == 3) button = MouseButtons.XButton1;
            else if (btnIndex == 4) button = MouseButtons.XButton2;
            else button = MouseButtons.Left;
        }

        var point = new Point((int)(x * dpi), (int)(y * dpi));

        return new MouseEventArgs(button, 1, point.X, point.Y, (int)delta);
    }


    /// <summary>
    /// Parses modifier keys from JSON string.
    /// </summary>
    private static Keys ParseModifierKeysFromJson(string json)
    {
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value?.ToString() ?? string.Empty);

        var modifiers = Keys.None;

        if (dict.TryGetValue("AltKey", out var altStr) &&
            bool.TryParse(altStr, out var altKey) && altKey)
        {
            modifiers |= Keys.Alt;
        }
        if (dict.TryGetValue("CtrlKey", out var ctrlStr) &&
            bool.TryParse(ctrlStr, out var ctrlKey) && ctrlKey)
        {
            modifiers |= Keys.Control;
        }
        if (dict.TryGetValue("ShiftKey", out var shiftStr) &&
            bool.TryParse(shiftStr, out var shiftKey) && shiftKey)
        {
            modifiers |= Keys.Shift;
        }

        return modifiers;
    }


    /// <summary>
    /// Sets zoom factor for <see cref="Web2"/>.
    /// </summary>
    /// <param name="zoomFactor"></param>
    /// <param name="isManualZoom"></param>
    /// <param name="zoomDelta">
    /// <list type="bullet">
    ///   <item>If <c>zoomFactor equals 1</c>, use <paramref name="zoomFactor"/> to zoom.</item>
    ///   <item>If <c>zoomFactor is greater than 1</c>, performs zoom in.</item>
    ///   <item>If <c>zoomFactor is less than 1</c>, performs zoom out.</item>
    /// </list>
    /// </param>
    private void SetZoomFactorWeb2(float zoomFactor, bool isManualZoom, float zoomDelta = 1f)
    {
        var obj = new ExpandoObject();
        _ = obj.TryAdd("ZoomFactor", zoomFactor);
        _ = obj.TryAdd("IsManualZoom", isManualZoom);
        _ = obj.TryAdd("ZoomDelta", zoomDelta);

        Web2.PostWeb2Message(Web2BackendMsgNames.SET_ZOOM_FACTOR, BHelper.ToJson(obj));
    }


    /// <summary>
    /// Starts animation for <see cref="Web2"/>.
    /// </summary>
    private void StartWeb2Animation(AnimationSource sources)
    {
        // Panning
        if (sources.HasFlag(AnimationSource.PanLeft))
        {
            StartWeb2PanningAnimation("left");
        }
        else if (sources.HasFlag(AnimationSource.PanRight))
        {
            StartWeb2PanningAnimation("right");
        }
        else if (sources.HasFlag(AnimationSource.PanUp))
        {
            StartWeb2PanningAnimation("up");
        }
        else if (sources.HasFlag(AnimationSource.PanDown))
        {
            StartWeb2PanningAnimation("down");
        }

        // Zooming
        else if (sources.HasFlag(AnimationSource.ZoomIn))
        {
            StartWeb2ZoomingAnimation(false);
        }
        else if (sources.HasFlag(AnimationSource.ZoomOut))
        {
            StartWeb2ZoomingAnimation(true);
        }
    }


    /// <summary>
    /// Starts panning animation for <see cref="Web2"/>.
    /// </summary>
    /// <param name="direction">Direction is either <c>left</c>, <c>right</c>, <c>up</c> or <c>down</c>.</param>.
    private void StartWeb2PanningAnimation(string direction)
    {
        var obj = new ExpandoObject();
        _ = obj.TryAdd("PanSpeed", PanDistance);
        _ = obj.TryAdd("Direction", direction);

        Web2.PostWeb2Message(Web2BackendMsgNames.START_PANNING_ANIMATION, BHelper.ToJson(obj));
    }


    /// <summary>
    /// Starts zooming animation for <see cref="Web2"/>.
    /// </summary>
    private void StartWeb2ZoomingAnimation(bool isZoomOut)
    {
        var obj = new ExpandoObject();
        _ = obj.TryAdd("IsZoomOut", isZoomOut);
        _ = obj.TryAdd("ZoomSpeed", ZoomSpeed);

        Web2.PostWeb2Message(Web2BackendMsgNames.START_ZOOMING_ANIMATION, BHelper.ToJson(obj));
    }


    /// <summary>
    /// Stops animations of <see cref="Web2"/>.
    /// </summary>
    private void StopWeb2Animations()
    {
        Web2.PostWeb2Message(Web2BackendMsgNames.STOP_ANIMATIONS, "null");
    }


    /// <summary>
    /// Sets message of <see cref="Web2"/>.
    /// </summary>
    private void ShowWeb2Message(string text, string? heading = null)
    {
        var obj = new ExpandoObject();
        _ = obj.TryAdd("Text", text ?? string.Empty);
        _ = obj.TryAdd("Heading", heading ?? string.Empty);

        Web2.PostWeb2Message(Web2BackendMsgNames.SET_MESSAGE, BHelper.ToJson(obj));
    }


    /// <summary>
    /// Sets navigation button image for <see cref="Web2"/>.
    /// </summary>
    private void SetWeb2NavButtonStyles()
    {
        if (Web2 == null) return;

        var leftImageUrl = string.Empty;
        var rightImageUrl = string.Empty;

        if (!string.IsNullOrWhiteSpace(Web2NavLeftImagePath))
        {
            leftImageUrl = new Uri(Web2NavLeftImagePath).AbsoluteUri;
        }
        if (!string.IsNullOrWhiteSpace(Web2NavRightImagePath))
        {
            rightImageUrl = new Uri(Web2NavRightImagePath).AbsoluteUri;
        }


        var obj = new ExpandoObject();
        _ = obj.TryAdd("Visible", NavDisplay != Base.PhotoBox.NavButtonDisplay.None);
        _ = obj.TryAdd("LeftImageUrl", leftImageUrl);
        _ = obj.TryAdd("RightImageUrl", rightImageUrl);
        _ = obj.TryAdd(nameof(NavButtonColor), NavButtonColor.ToRgbaArray().ToList());


        Web2.PostWeb2Message(Web2BackendMsgNames.SET_NAVIGATION, BHelper.ToJson(obj));
    }

    #endregion // Private methods


    // WebView2 Comparison mode methods
    #region WebView2 Comparison mode methods

    /// <summary>
    /// Enables or disables WebView2 comparison mode.
    /// </summary>
    public async Task SetWeb2ComparisonModeAsync(bool enabled, CancellationToken token = default)
    {
        // Track the WebView2 comparison mode state
        _isWeb2ComparisonModeActive = enabled;

        try
        {
            // If disabling and WebView2 not needed, skip initialization
            if (!enabled && !IsWeb2Ready)
            {
                return;
            }

            if (!IsWeb2Ready)
            {
                await InitializeWeb2Async();
            }

            // Wait for Web2 navigation to complete before sending messages
            while (!_isWeb2NavigationDone)
            {
                await Task.Delay(10, token);
            }

            token.ThrowIfCancellationRequested();

            // Ensure Web2 is still valid after waiting
            if (Web2 == null) return;

            // Bring WebView2 to front when enabling comparison mode
            // so it renders on top of D2D canvas
            if (enabled)
            {
                Web2.BringToFront();
                Web2.Visible = true;
            }
            else if (!UseWebview2)
            {
                // Hide WebView2 when disabling comparison and not using WebView2 for main image
                Web2.SendToBack();
                await Web2.SetWeb2VisibilityAsync(false);
            }

            var sliderHandleUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(Web2CompareSliderHandlePath))
            {
                sliderHandleUrl = new Uri(Web2CompareSliderHandlePath).AbsoluteUri;
            }

            var obj = new ExpandoObject();
            _ = obj.TryAdd("Enabled", enabled);
            _ = obj.TryAdd("AccentColor", AccentColor.ToRgbaArray().ToList());
            _ = obj.TryAdd("SliderHandleUrl", sliderHandleUrl);

            Web2.PostWeb2Message(Web2BackendMsgNames.SET_COMPARISON_MODE, BHelper.ToJson(obj));
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException) { }
    }

    /// <summary>
    /// Sets the comparison images in WebView2.
    /// </summary>
    public async Task SetWeb2ComparisonImagesAsync(
        string? leftImagePath,
        string? leftImageHtml,
        string? rightImagePath,
        string? rightImageHtml,
        CancellationToken token = default)
    {
        // Store comparison image path for cycling to work correctly
        _compareImagePath = rightImagePath ?? string.Empty;

        try
        {
            if (!IsWeb2Ready)
            {
                await InitializeWeb2Async();
            }

            // Wait for Web2 navigation to complete before sending messages
            while (!_isWeb2NavigationDone)
            {
                await Task.Delay(10, token);
            }

            token.ThrowIfCancellationRequested();

            // Ensure Web2 is still valid after waiting
            if (Web2 == null) return;

            var obj = new ExpandoObject();

            // Left image
            if (!string.IsNullOrEmpty(leftImageHtml))
            {
                _ = obj.TryAdd("LeftImageHtml", leftImageHtml);
                _ = obj.TryAdd("LeftImageUrl", string.Empty);
            }
            else if (!string.IsNullOrEmpty(leftImagePath))
            {
                _ = obj.TryAdd("LeftImageUrl", new Uri(leftImagePath).AbsoluteUri);
                _ = obj.TryAdd("LeftImageHtml", string.Empty);
            }

            // Right image
            if (!string.IsNullOrEmpty(rightImageHtml))
            {
                _ = obj.TryAdd("RightImageHtml", rightImageHtml);
                _ = obj.TryAdd("RightImageUrl", string.Empty);
            }
            else if (!string.IsNullOrEmpty(rightImagePath))
            {
                _ = obj.TryAdd("RightImageUrl", new Uri(rightImagePath).AbsoluteUri);
                _ = obj.TryAdd("RightImageHtml", string.Empty);
            }

            Web2.PostWeb2Message(Web2BackendMsgNames.SET_COMPARISON_IMAGES, BHelper.ToJson(obj));
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException) { }
    }

    /// <summary>
    /// Sets the comparison slider position in WebView2.
    /// </summary>
    public void SetWeb2ComparisonSlider(float position)
    {
        if (!IsWeb2Ready) return;

        var obj = new ExpandoObject();
        _ = obj.TryAdd("Position", Math.Clamp(position, 0f, 1f));

        Web2.PostWeb2Message(Web2BackendMsgNames.SET_COMPARISON_SLIDER, BHelper.ToJson(obj));
    }

    /// <summary>
    /// Reads SVG file content for use in WebView2 comparison.
    /// </summary>
    public static async Task<string?> ReadSvgContentAsync(string filePath, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        if (!filePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) return null;
        if (!File.Exists(filePath)) return null;

        try
        {
            return await File.ReadAllTextAsync(filePath, token);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Reads image file content as HTML for WebView2 comparison.
    /// Returns SVG content for SVG files, or base64 img tag for raster images.
    /// </summary>
    public static async Task<string?> ReadImageAsHtmlAsync(string filePath, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        if (!File.Exists(filePath)) return null;

        try
        {
            // For SVG files, return the SVG content directly
            if (filePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                return await File.ReadAllTextAsync(filePath, token);
            }

            // For raster images, convert to base64 img tag
            var bytes = await File.ReadAllBytesAsync(filePath, token);
            var base64 = Convert.ToBase64String(bytes);
            var mimeType = GetMimeType(filePath);

            return $"<img src=\"data:{mimeType};base64,{base64}\" style=\"max-width:none;max-height:none;\" />";
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Gets the MIME type for an image file.
    /// </summary>
    private static string GetMimeType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" or ".jpe" or ".jfif" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" or ".dib" => "image/bmp",
            ".ico" => "image/x-icon",
            ".tif" or ".tiff" => "image/tiff",
            ".avif" => "image/avif",
            ".heic" or ".heif" => "image/heic",
            ".jxl" => "image/jxl",
            _ => "image/png", // fallback
        };
    }

    #endregion // WebView2 Comparison mode methods


}
