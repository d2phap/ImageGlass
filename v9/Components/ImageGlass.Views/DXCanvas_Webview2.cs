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

using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using Microsoft.Web.WebView2.Core;
using System.Dynamic;

namespace ImageGlass.Viewer;


public partial class DXCanvas
{
    private bool _isWeb2NavigationDone = false;
    private Web2? _web2 = null;
    private MouseEventArgs? _web2PointerDownEventArgs = null;


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
    /// Gets, sets value indicates that the <see cref="DXCanvas"/>
    /// should use <see cref="Web2"/> to render the image.
    /// </summary>
    public bool UseWebview2 => _imageSource == ImageSource.Webview2;

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
            _web2PointerDownEventArgs = DXCanvas.ParseMouseEventJson(e.Data);
            Web2PointerDown?.Invoke(this, _web2PointerDownEventArgs);
        }
        else if (e.Name == Web2FrontendMsgNames.ON_MOUSE_WHEEL)
        {
            var mouseWheelEventArgs = DXCanvas.ParseMouseEventJson(e.Data);
            this.OnMouseWheel(mouseWheelEventArgs);
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

            // if image file is SVG, we read its content
            if (!string.IsNullOrWhiteSpace(data.Filename)
                && data.Filename.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase))
            {
                var textContent = await File.ReadAllTextAsync(data.Filename, token);
                _ = obj.TryAdd("Html", textContent);
                msgName = Web2BackendMsgNames.SET_HTML;
            }
            else
            {
                _ = obj.TryAdd("Url", data.Filename);
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


    #endregion // Public methods



    // Private methods
    #region Private methods

    /// <summary>
    /// Initializes <see cref="Web2"/> control, adds it into the <see cref="DXCanvas"/> control.
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
        SourceWidth = imgData?.Width ?? 0;
        SourceHeight = imgData?.Height ?? 0;
        CanImageAnimate = false;
        HasAlphaPixels = true;
        UseHardwareAcceleration = true;
    }


    /// <summary>
    /// Parses JSON string to <see cref="ZoomEventArgs"/>.
    /// </summary>
    private ZoomEventArgs ParseZoomEventJson(string json)
    {
        var isZoomModeChanged = false;
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

        if (dict.TryGetValue("ZoomFactor", out var zoomFactor))
        {
            _ = float.TryParse(zoomFactor, out _zoomFactor);
        }
        if (dict.TryGetValue("IsManualZoom", out var isManualZoom))
        {
            _isManualZoom = isManualZoom.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
        if (dict.TryGetValue("IsZoomModeChanged", out var zoomModeChanged))
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

    #endregion // Private methods


}
