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
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ImageGlass.Base;

/// <summary>
/// A wrapper of <see cref="WebView2"/> control.
/// </summary>
public class Web2 : WebView2
{
    private bool _darkMode = true;
    private Color _accentColor = Color.FromArgb(28, 146, 255);


    // Properties
    #region Properties

    /// <summary>
    /// Enables or disables debug mode.
    /// </summary>
    public bool EnableDebug { get; set; } = false;

    /// <summary>
    /// Gets, sets dark mode of <see cref="Web2"/>.
    /// </summary>
    public bool DarkMode
    {
        get => _darkMode;
        set
        {
            _darkMode = value;
            _ = SetWeb2DarkModeAsync(value);
        }
    }

    /// <summary>
    /// Gets, sets accent color of <see cref="Web2"/>.
    /// </summary>
    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            _accentColor = value;
            _ = SetWeb2AccentColorAsync(value);
        }
    }

    /// <summary>
    /// Gets, sets campaign for hyperlink url.
    /// </summary>
    public string PageName { get; set; } = "unknown";

    /// <summary>
    /// Gets value indicates that <see cref="Web2"/> is ready to use.
    /// </summary>
    public bool IsWeb2Ready => this.CoreWebView2 != null;


    /// <summary>
    /// Gets the path of WebView2 Runtime fixed version.
    /// If not found, return <c>null</c>.
    /// </summary>
    public static string? WebView2RuntimeFixedVersionDirPath
    {
        get
        {
            var dir = App.StartUpDir(Dir.WebView2Runtime);

            if (Directory.Exists(dir)) return dir;

            return null;
        }
    }


    /// <summary>
    /// Gets version of <see cref="WebView2"/> runtime,
    /// returns <c>null</c> if <see cref="WebView2"/> runtime is not installed.
    /// </summary>
    public static Version? Webview2Version
    {
        get
        {
            try
            {
                var version = CoreWebView2Environment.GetAvailableBrowserVersionString(WebView2RuntimeFixedVersionDirPath);
                return new Version(version);
            }
            catch (WebView2RuntimeNotFoundException) { }

            return null;
        }
    }


    #endregion // Properties



    // Public events
    #region Public events

    /// <summary>
    /// Occurs when <see cref="Web2"/> is ready to use.
    /// </summary>
    public event EventHandler<EventArgs> Web2Ready;

    /// <summary>
    /// Occurs when <see cref="Web2"/> navigation is completed
    /// </summary>
    public event EventHandler<EventArgs> Web2NavigationCompleted;

    /// <summary>
    /// Occurs when <see cref="Web2"/> receives a message from web view.
    /// </summary>
    public event EventHandler<Web2MessageReceivedEventArgs> Web2MessageReceived;

    /// <summary>
    /// Occurs when <see cref="Web2"/> receives keydown.
    /// </summary>
    public event EventHandler<KeyEventArgs> Web2KeyDown;

    /// <summary>
    /// Occurs when <see cref="Web2"/> receives keyup.
    /// </summary>
    public event EventHandler<KeyEventArgs> Web2KeyUp;

    /// <summary>
    /// Occurs when <see cref="Web2"/> is opening context menu.
    /// </summary>
    public event EventHandler<CoreWebView2ContextMenuRequestedEventArgs> Web2ContextMenuRequested;

    #endregion // Public events



    /// <summary>
    /// Initializes new instance of <see cref="Web2"/>.
    /// </summary>
    public Web2()
    {
        this.DefaultBackgroundColor = Color.Transparent;

        this.WebMessageReceived += Web2_WebMessageReceived;
        this.NavigationCompleted += Web2_NavigationCompleted;
    }



    // Override/ virtual methods
    #region Override/ virtual methods

    protected override void Dispose(bool disposing)
    {
        if (this.CoreWebView2 != null)
        {
            this.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
        }

        this.WebMessageReceived -= Web2_WebMessageReceived;
        this.NavigationCompleted -= Web2_NavigationCompleted;

        base.Dispose(disposing);
    }


    /// <summary>
    /// Occurs when the <see cref="Web2"/> control is ready.
    /// </summary>
    protected virtual async Task OnWeb2ReadyAsync()
    {
        if (InvokeRequired)
        {
            await Invoke(OnWeb2ReadyAsync);
            return;
        }

        Web2Ready?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Occurs when the <see cref="Web2"/> navigation is completed.
    /// </summary>
    protected virtual async Task OnWeb2NavigationCompleted()
    {
        if (InvokeRequired)
        {
            await Invoke(OnWeb2NavigationCompleted);
            return;
        }

        Web2NavigationCompleted?.Invoke(this, EventArgs.Empty);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Triggers <see cref="Web2MessageReceived"/> event.
    /// </summary>
    protected virtual async Task OnWeb2MessageReceivedAsync(Web2MessageReceivedEventArgs e)
    {
        if (InvokeRequired)
        {
            await Invoke(async delegate
            {
                await OnWeb2MessageReceivedAsync(e);
            });
            return;
        }

        Web2MessageReceived?.Invoke(this, e);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Triggers <see cref="Web2KeyDown"/> event.
    /// </summary>
    protected virtual async Task OnWeb2KeyDownAsync(KeyEventArgs e)
    {
        if (InvokeRequired)
        {
            await Invoke(async delegate
            {
                await OnWeb2KeyDownAsync(e);
            });
            return;
        }

        Web2KeyDown?.Invoke(this, e);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Triggers <see cref="Web2KeyUp"/> event.
    /// </summary>
    protected virtual async Task OnWeb2KeyUpAsync(KeyEventArgs e)
    {
        if (InvokeRequired)
        {
            await Invoke(async delegate
            {
                await OnWeb2KeyUpAsync(e);
            });
            return;
        }

        Web2KeyUp?.Invoke(this, e);
        await Task.CompletedTask;
    }


    /// <summary>
    /// Triggers <see cref="Web2ContextMenuRequested"/> event.
    /// </summary>
    protected virtual async Task OnWeb2ContextMenuRequested(CoreWebView2ContextMenuRequestedEventArgs e)
    {
        if (InvokeRequired)
        {
            await Invoke(async delegate
            {
                await OnWeb2ContextMenuRequested(e);
            });
            return;
        }

        Web2ContextMenuRequested?.Invoke(this, e);
        await Task.CompletedTask;
    }

    #endregion // Override/ virtual methods



    // Public methods
    #region Public methods

    /// <summary>
    /// Ensures <see cref="Web2"/> is ready to work.
    /// </summary>
    public async Task EnsureWeb2Async()
    {
        // if WebView2 runtime is not installed
        if (!Web2.CheckWebview2Installed())
        {
            MessageBox.Show($"{nameof(Web2)}: WebView2 Runtime 64-bit is not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }


        var options = new CoreWebView2EnvironmentOptions
        {
            AdditionalBrowserArguments = "--disable-web-security --allow-file-access-from-files --allow-file-access",
        };

        try
        {
            // use AppData dir
            // %LocalAppData%\ImageGlass\WebView2_Data
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.AppName,
                "WebView2_Data");

            // create the directory if not exists
            Directory.CreateDirectory(appDataDir);

            var env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: WebView2RuntimeFixedVersionDirPath,
                userDataFolder: appDataDir,
                options: options);

            await this.EnsureCoreWebView2Async(env).ConfigureAwait(true);

            this.CoreWebView2.Settings.IsZoomControlEnabled = false;
            this.CoreWebView2.Settings.IsStatusBarEnabled = false;
            this.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
            this.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            this.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            this.CoreWebView2.Settings.IsPinchZoomEnabled = false;
            this.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            this.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            this.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            this.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

            // DevTools
            this.CoreWebView2.Settings.AreDevToolsEnabled = true;

            this.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            this.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;


            await OnWeb2ReadyAsync();
        }
        catch (Exception ex)
        {
            // Operation aborted (0x80004004 (E_ABORT))
            if (ex.HResult == -2147467260)
            {
                // ignore
            }
            else
            {
                MessageBox.Show($"{nameof(Web2)}: Failed to initialize Webview2!\r\n\r\n" +
                    $"{ex.Message}\r\n\r\n" +
                    $"at {nameof(EnsureWeb2Async)}() method", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }


    /// <summary>
    /// Checks if <see cref="WebView2"/> runtime is installed.
    /// </summary>
    public static bool CheckWebview2Installed()
    {
        return Web2.Webview2Version != null;
    }


    /// <summary>
    /// Sets accent color to <see cref="Web2"/> content. Example:
    /// <code>
    /// html style="--Accent: 255 0 0"
    /// </code>
    /// </summary>
    public async Task SetWeb2AccentColorAsync(Color accent)
    {
        if (!this.IsWeb2Ready) return;

        var rgb = $"{accent.R} {accent.G} {accent.B}";
        await this.ExecuteScriptAsync($"""
            document.documentElement.style.setProperty('--Accent', '{rgb}');
            """);
    }


    /// <summary>
    /// Sets dark mode for <see cref="Web2"/> content. Example:
    /// <code>
    /// html color-mode="light"
    /// </code>
    /// </summary>
    public async Task SetWeb2DarkModeAsync(bool darkMode)
    {
        if (!this.IsWeb2Ready) return;

        var colorMode = darkMode ? "dark" : "light";
        await this.ExecuteScriptAsync($"""
            document.documentElement.setAttribute('color-mode', '{colorMode}');
            """);
    }


    /// <summary>
    /// Sets focus mode for <see cref="Web2"/> content. Example:
    /// <code>
    /// html window-focus="true"
    /// </code>
    /// </summary>
    public async Task SetWeb2WindowFocusAsync(bool focus)
    {
        if (!this.IsWeb2Ready) return;

        await this.ExecuteScriptAsync($"""
            document.documentElement.setAttribute('window-focus', '{focus.ToString().ToLowerInvariant()}');
            """);
    }


    /// <summary>
    /// Sets the visibility of <see cref="Web2"/>.
    /// If the <paramref name="visible"/> is <c>false</c>,
    /// the <see cref="Web2"/> will be also suspended to consume less memory.
    /// </summary>
    public async Task SetWeb2VisibilityAsync(bool visible)
    {
        this.Visible = visible;

        if (!visible)
        {
            await TrySuspendWeb2Async();
        }
    }


    /// <summary>
    /// Tries to suspend the <see cref="Web2"/> instance to consume less memory.
    /// </summary>
    public async Task TrySuspendWeb2Async(int delayMs = 1000)
    {
        await Task.Delay(delayMs);
        if (!this.IsWeb2Ready) return;

        try
        {
            _ = this.CoreWebView2.TrySuspendAsync();
        }
        catch (InvalidOperationException) { }
    }


    /// <summary>
    /// Post a message to web view.
    /// </summary>
    /// <param name="name">Name of the message</param>
    /// <param name="dataJson">Message data</param>
    /// <exception cref="ArgumentException"></exception>
    public void PostWeb2Message(string name, string dataJson)
    {
        if (!this.IsWeb2Ready) return;


        var json = @$"
{{
    ""Name"": ""{name}"",
    ""Data"": {dataJson}
}}";

        try
        {
            this.CoreWebView2.PostWebMessageAsJson(json);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"{ex}\r\n" +
                $"JSON data:\r\n{json}\r\n\r\n" +
                $"Error detail:\r\n", ex);
        }
    }

    #endregion // Public methods



    // Webview2 Events
    #region Webview2 Events

    private void Web2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var msg = BHelper.ParseJson<Web2MessageReceivedEventArgs>(e.WebMessageAsJson);

        if (msg.Name.Equals("KEYUP") && !string.IsNullOrWhiteSpace(msg.Data))
        {
            var hotkey = new Hotkey(msg.Data);
            SafeRunUi(async () =>
            {
                await OnWeb2KeyUpAsync(new KeyEventArgs(hotkey.KeyData));
            });
        }
        else if (msg.Name.Equals("KEYDOWN") && !string.IsNullOrWhiteSpace(msg.Data))
        {
            if (EnableDebug && msg.Data == "ctrl+shift+i")
            {
                this.CoreWebView2.OpenDevToolsWindow();
                return;
            }

            var hotkey = new Hotkey(msg.Data);
            SafeRunUi(async () =>
            {
                await OnWeb2KeyDownAsync(new KeyEventArgs(hotkey.KeyData));
            });
        }
        else
        {
            SafeRunUi(async () =>
            {
                var args = new Web2MessageReceivedEventArgs(
                    msg.Name.Trim(),
                    msg.Data?.Trim(),
                    e.AdditionalObjects
                );
                await OnWeb2MessageReceivedAsync(args);
            });
        }
    }


    private async void Web2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (this.Source.AbsoluteUri == "about:blank") return;

        var logTask = Task.CompletedTask;

        // disable console.log if not debug mode
        if (!EnableDebug)
        {
            logTask = this.ExecuteScriptAsync($"""
                console.log = () => undefined;
                console.info = () => undefined;
            """);
        }


        var borderRadiusTask = Task.CompletedTask;
        if (BHelper.IsOS(WindowsOS.Win10))
        {
            borderRadiusTask = this.ExecuteScriptAsync($"""
                document.documentElement.style.setProperty('--baseBorderRadius', '0');
            """);
        }

        var darkModeTask = SetWeb2DarkModeAsync(DarkMode);
        var accentColorTask = SetWeb2AccentColorAsync(AccentColor);

        await Task.WhenAll(logTask, borderRadiusTask, darkModeTask, accentColorTask);
        await OnWeb2NavigationCompleted();
    }


    private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        _ = BHelper.OpenUrlAsync(e.Uri, $"app_{PageName}");
    }


    private void CoreWebView2_ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        // block the default context menu
        e.Handled = true;

        _ = OnWeb2ContextMenuRequested(e);
    }


    /// <summary>
    /// Safely run the action after the current event handler is completed,
    /// to avoid potential reentrancy caused by running a nested message loop
    /// in the WebView2 event handler.
    /// Source: <see href="https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model#reentrancy"/>
    /// </summary>
    private static void SafeRunUi(Action action)
    {
        SynchronizationContext.Current.Post((_) =>
        {
            action();
        }, null);
    }

    #endregion // Webview2 Events

}
