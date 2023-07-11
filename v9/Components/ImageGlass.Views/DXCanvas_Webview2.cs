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

        ((System.ComponentModel.ISupportInitialize)Web2).BeginInit();
        SuspendLayout();

        Web2.AllowExternalDrop = true;
        Web2.CreationProperties = null;
        Web2.Name = nameof(Web2);
        Web2.Size = Size;
        Web2.Dock = DockStyle.Fill;
        Web2.ZoomFactor = 1D;
        Web2.Visible = true;
        Web2.Web2KeyDown += Web2_Web2KeyDown;
        Web2.Web2MessageReceived += Web2_Web2MessageReceived;
        

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

        _web2.Web2KeyDown -= Web2_Web2KeyDown;
        _web2.Web2MessageReceived -= Web2_Web2MessageReceived;
        _web2?.Dispose();
        _web2 = null;
    }


    #endregion // Private methods


    // Web2 events
    #region Web2 events

    private void Web2_Web2KeyDown(object? sender, KeyEventArgs e)
    {
        this.OnKeyDown(e);
    }


    private void Web2_Web2MessageReceived(object? sender, Web2MessageReceivedEventArgs e)
    {
        
    }

    #endregion // Web2 events




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
