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
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Dynamic;

namespace ImageGlass.Viewer;


public partial class DXCanvas
{
    private bool _isWeb2NavigationDone = false;
    private bool _useWebview2 = false;
    private Web2? _web2 = null;


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
    public bool UseWebview2
    {
        get => _useWebview2;
        set
        {
            if (_useWebview2 != value)
            {
                if (value && _web2 == null)
                {
                    _ = InitializeWeb2Async();
                }
                else
                {
                    _ = Web2.SetWeb2VisibilityAsync(value);
                }
            }

            _useWebview2 = value;
        }
    }

    #endregion // Properties


    /// <summary>
    /// Occurs when <see cref="Web2"/> navigation is completed.
    /// </summary>
    public event EventHandler<EventArgs> Web2NavigationCompleted;

    /// <summary>
    /// Occurs when <see cref="Web2"/> received <c>pointerdown</c> event.
    /// </summary>
    public event EventHandler<MouseEventArgs> Web2PointerDown;


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
    /// Dispose <see cref="WebView2"/> resources.
    /// </summary>
    private void DisposeWebview2Control()
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


    #endregion // Private methods


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
        this.OnKeyDown(e);
    }


    private MouseEventArgs? _web2PointerDownEventArgs = null;

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
            var isZoomModeChanged = false;
            var dict = BHelper.ParseJson<ExpandoObject>(e.Data)
                .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

            if (dict.TryGetValue("zoomFactor", out var zoomFactor))
            {
                _ = float.TryParse(zoomFactor, out _zoomFactor);
            }
            if (dict.TryGetValue("isManualZoom", out var isManualZoom))
            {
                _isManualZoom = isManualZoom.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }
            if (dict.TryGetValue("isZoomModeChanged", out var zoomModeChanged))
            {
                isZoomModeChanged = zoomModeChanged.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }


            OnZoomChanged?.Invoke(this, new ZoomEventArgs()
            {
                ZoomFactor = _zoomFactor,
                IsManualZoom = _isManualZoom,
                IsZoomModeChange = isZoomModeChanged,
                IsPreviewingImage = false,
                ChangeSource = ZoomChangeSource.Unknown,
            });
        }
        else if (e.Name == Web2FrontendMsgNames.ON_POINTER_DOWN)
        {
            _web2PointerDownEventArgs = ParseMouseEventJson(e.Data);
            Web2PointerDown?.Invoke(this, _web2PointerDownEventArgs);
        }
    }

    #endregion // Web2 events





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
    /// Loads the image file into <see cref="Web2"/>.
    /// </summary>
    public async Task SetImageWeb2Async(IgPhoto? data, CancellationToken token = default)
    {
        if (data == null) return;

        try
        {
            LoadImageDataWeb2(data?.Metadata);

            string msgName;
            var obj = new ExpandoObject();
            _ = obj.TryAdd("ZoomMode", ZoomMode.ToString());

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
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException) { }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    /// <summary>
    /// Loads image data for Webview2.
    /// </summary>
    private void LoadImageDataWeb2(IgMetadata? imgData)
    {
        SourceWidth = imgData?.Width ?? 0;
        SourceHeight = imgData?.Height ?? 0;
        CanImageAnimate = false;
        HasAlphaPixels = true;
        UseHardwareAcceleration = true;
    }


    /// <summary>
    /// Parses JSON string to <see cref="MouseEventArgs"/>.
    /// </summary>
    private MouseEventArgs ParseMouseEventJson(string json)
    {
        var dict = BHelper.ParseJson<ExpandoObject>(json)
            .ToDictionary(i => i.Key, i => i.Value.ToString() ?? string.Empty);

        var x = 0f;
        var y = 0f;
        var button = MouseButtons.Left;

        if (dict.TryGetValue("x", out var xStr))
        {
            _ = float.TryParse(xStr, out x);
        }
        if (dict.TryGetValue("y", out var yStr))
        {
            _ = float.TryParse(yStr, out y);
        }
        if (dict.TryGetValue("button", out var buttonStr))
        {
            _ = int.TryParse(buttonStr, out var btnIndex);

            if (btnIndex == 1) button = MouseButtons.Middle;
            else if (btnIndex == 2) button = MouseButtons.Right;
            else if (btnIndex == 3) button = MouseButtons.XButton1;
            else if (btnIndex == 4) button = MouseButtons.XButton2;
            else button = MouseButtons.Left;
        }

        var point = this.ScaleToDpi(new Point((int)x, (int)y));

        return new MouseEventArgs(button, 1, point.X, point.Y, 0);
    }

}
