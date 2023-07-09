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
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ImageGlass.Viewer;

public partial class DXCanvas
{
    private bool _useWebview2 = false;
    private WebView2? _webView2 = null;


    // Properties
    #region Properties

    /// <summary>
    /// Gets the <see cref="WebView2"/> instance.
    /// </summary>
    private WebView2 Web2 => _webView2;


    /// <summary>
    /// Gets value indicates that <see cref="Web2"/> is ready to use.
    /// </summary>
    private bool IsWeb2Ready => Web2 != null && Web2.CoreWebView2 != null;


    /// <summary>
    /// Gets, sets value indicates that the <see cref="DXCanvas"/>
    /// should use <see cref="WebView2"/> to render the image.
    /// </summary>
    public bool UseWebview2
    {
        get => _useWebview2;
        set
        {
            if (_useWebview2 != value)
            {
                if (value && _webView2 == null)
                {
                    _ = InitializeWebview2ControlAsync();
                }
                else
                {
                    _ = SetWebview2ControlVisibilityAsync(value);
                }
            }

            _useWebview2 = value;
        }
    }

    #endregion // Properties


    // Private methods
    #region Private methods

    /// <summary>
    /// Initializes <see cref="Web2"/> control, adds it into the <see cref="DXCanvas"/> control.
    /// </summary>
    private async Task InitializeWebview2ControlAsync()
    {
        if (InvokeRequired)
        {
            await Invoke(InitializeWebview2ControlAsync);
            return;
        }

        _webView2 = new WebView2();

        ((System.ComponentModel.ISupportInitialize)Web2).BeginInit();
        SuspendLayout();

        Web2.DefaultBackgroundColor = Color.Transparent;
        Web2.AllowExternalDrop = true;
        Web2.CreationProperties = null;
        Web2.Name = nameof(Web2);
        Web2.Size = Size;
        Web2.Dock = DockStyle.Fill;
        Web2.ZoomFactor = 1D;
        Web2.Visible = true;
        Web2.WebMessageReceived += Web2_WebMessageReceived;


        try
        {
            Controls.Add(Web2);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}\r\n\r\n at InitializeWebview2ControlAsync() method", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ((System.ComponentModel.ISupportInitialize)Web2).EndInit();
        ResumeLayout(false);

        await EnsureCoreWebview2Async();
    }


    /// <summary>
    /// Ensures <see cref="WebView2"/> is ready to work.
    /// </summary>
    private async Task EnsureCoreWebview2Async()
    {
        var options = new CoreWebView2EnvironmentOptions
        {
            AdditionalBrowserArguments = "--disable-web-security --allow-file-access-from-files --allow-file-access",
        };

        try
        {
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: App.ConfigDir(PathType.Dir, "WebUIData"),
                options: options);

            await Web2.EnsureCoreWebView2Async(env);

            Web2.CoreWebView2.Settings.IsZoomControlEnabled = false;
            Web2.CoreWebView2.Settings.IsStatusBarEnabled = false;
            Web2.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
            Web2.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            Web2.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            Web2.CoreWebView2.Settings.IsPinchZoomEnabled = false;
            Web2.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            Web2.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            Web2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            Web2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

            // DevTools
            Web2.CoreWebView2.Settings.AreDevToolsEnabled = false;
#if DEBUG
            Web2.CoreWebView2.Settings.AreDevToolsEnabled = true;
#endif
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize Webview2!\r\n\r\n" +
                $"{ex.Message}\r\n\r\n" +
                $"at EnsureCoreWebview2Async() method", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    /// <summary>
    /// Starts listening to keydown event.
    /// </summary>
    private async Task StartListeningToKeyDownWebEventAsync()
    {
        await Web2.ExecuteScriptAsync("""
            window.onkeydown = (e) => {
                e.preventDefault();
        
                const ctrl = e.ctrlKey ? 'ctrl' : '';
                const shift = e.shiftKey ? 'shift' : '';
                const alt = e.altKey ? 'alt' : '';
        
                let key = e.key.toLowerCase();
                const keyMaps = {
                    control: '',
                    shift: '',
                    alt: '',
                    arrowleft: 'left',
                    arrowright: 'right',
                    arrowup: 'up',
                    arrowdown: 'down',
                    backspace: 'back',
                };
                if (keyMaps[key] !== undefined) {
                    key = keyMaps[key];
                }
        
                const keyCombo = [ctrl, shift, alt, key].filter(Boolean).join('+');
        
                console.log('KEYDOWN', keyCombo);
                window.chrome.webview?.postMessage({ Name: 'KEYDOWN', Data: keyCombo });
            }
        """);
    }


    /// <summary>
    /// Dispose <see cref="WebView2"/> resources.
    /// </summary>
    private void DisposeWebview2Control()
    {
        Controls.Remove(_webView2);
        _webView2?.Dispose();
        _webView2 = null;
    }


    /// <summary>
    /// Sets the visibility of <see cref="Web2"/>.
    /// If the <paramref name="visible"/> is <c>false</c>,
    /// the <see cref="Web2"/> will be also suspended to have the <see cref="WebView2"/> consume less memory.
    /// </summary>
    public async Task SetWebview2ControlVisibilityAsync(bool visible)
    {
        Web2.Visible = visible;

        if (!visible)
        {
            await Task.Delay(1000);
            if (Web2.CoreWebView2 != null)
            {
                try
                {
                    _ = Web2.CoreWebView2.TrySuspendAsync();
                }
                catch (InvalidOperationException) { }
            }
        }
    }


    #endregion // Private methods





    private void Web2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var msg = BHelper.ParseJson<WebMessageModel>(e.WebMessageAsJson);

        if (msg.Name.Equals("KEYDOWN") && !string.IsNullOrWhiteSpace(msg.Data))
        {
#if DEBUG
            if (msg.Data == "ctrl+shift+i")
            {
                Web2.CoreWebView2.OpenDevToolsWindow();
                return;
            }
#endif

            var hotkey = new Hotkey(msg.Data);
            this.OnKeyDown(new(hotkey.KeyData));
        }
    }




    /// <summary>
    /// Loads the file into <see cref="WebView2"/>.
    /// </summary>
    public async Task SetImageUsingWebview2Async(string filePath, CancellationToken token)
    {
        var textContent = await File.ReadAllTextAsync(filePath, token);

        try
        {
            while (!IsWeb2Ready)
            {
                await Task.Delay(100, token);
            }

            token.ThrowIfCancellationRequested();


            var html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                </head>
                <body>
                    {{textContent}}

                    <script>
                        window.onkeydown = (e) => {
                            e.preventDefault();
                
                            const ctrl = e.ctrlKey ? 'ctrl' : '';
                            const shift = e.shiftKey ? 'shift' : '';
                            const alt = e.altKey ? 'alt' : '';

                            let key = e.key.toLowerCase();
                            const keyMaps = {
                                control: '',
                                shift: '',
                                alt: '',
                                arrowleft: 'left',
                                arrowright: 'right',
                                arrowup: 'up',
                                arrowdown: 'down',
                                backspace: 'back',
                            };
                            if (keyMaps[key] !== undefined) {
                                key = keyMaps[key];
                            }

                            const keyCombo = [ctrl, shift, alt, key].filter(Boolean).join('+');

                            console.log('KEYDOWN', keyCombo);
                            window.chrome.webview?.postMessage({ Name: 'KEYDOWN', Data: keyCombo });
                        }
                    </script>
                </body>
                </html>
                """;

            Web2.CoreWebView2.NavigateToString(html);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException) { }
        catch (Exception ex)
        {
            Web2.CoreWebView2.Navigate("about:blank");
            UseWebview2 = false;

            if (ex.Message.Contains("does not fall within the expected range"))
            {
                throw new InvalidDataException($"The content of '{filePath}' file is not valid", ex);
            }
            else throw ex;
        }
    }

}
