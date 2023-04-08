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
using ImageGlass.Properties;
using ImageGlass.Settings;
using Microsoft.Web.WebView2.Core;
using System.Dynamic;
using System.Text;

namespace ImageGlass;

public partial class FrmAbout : ThemedForm
{
    private bool IsWeb2Ready { get; set; } = false;


    public FrmAbout()
    {
        InitializeComponent();

        Web2.CoreWebView2InitializationCompleted += Web2_CoreWebView2InitializationCompleted;
        Web2.NavigationCompleted += Web2_NavigationCompleted;
        Web2.WebMessageReceived += Web2_WebMessageReceived;

        _ = InitWebview2();

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }



    // Protected / override methods
    #region Protected / override methods

    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        if (!EnableTransparent)
        {
            BackColor = Config.Theme.ColorPalatte.AppBg;
        }

        base.ApplyTheme(darkMode, style);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        base.OnRequestUpdatingColorMode(e);

        // apply theme to controls
        ApplyTheme(Config.Theme.Settings.IsDarkMode);

        _ = SetWeb2AccentColor();
    }


    #endregion // Protected / override methods


    private async Task InitWebview2()
    {
        var options = new CoreWebView2EnvironmentOptions
        {
            AdditionalBrowserArguments = "--disable-web-security --allow-file-access-from-files --allow-file-access",
        };

        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: App.ConfigDir(PathType.Dir, "ViewerData"),
            options: options);

        await Web2.EnsureCoreWebView2Async(env);
    }


    private void Web2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        this.Text = Application.ProductName + " [" + Web2.CoreWebView2.Environment.BrowserVersionString + "]";
        IsWeb2Ready = true;

        Web2.DefaultBackgroundColor = Color.Transparent;
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
        Web2.CoreWebView2.Settings.AreDevToolsEnabled = false;
#if DEBUG
        Web2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        Web2.CoreWebView2.Settings.AreDevToolsEnabled = true;
#endif

        Web2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

        LoadWebPage();
        Web2.Focus();
    }

    private void LoadWebPage()
    {
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        var msStoreBadge = Encoding.UTF8.GetString(Resources.MsStoreBadge);

        var pageHtml = Resources.Layout.ReplaceMultiple(new[]
        {
            Tuple.Create("{{styles.css}}", Resources.Styles),
            Tuple.Create("{{body.html}}", Resources.Page_About),
            Tuple.Create("{{AppLogo}}", $"data:image/png;base64,{base64Logo}"),
            Tuple.Create("{{AppCode}}", Constants.APP_CODE),
            Tuple.Create("{{AppVersion}}", App.Version),
            Tuple.Create("{{AppArchitecture}}", archInfo),
            Tuple.Create("{{AppRuntime}}", Environment.Version.ToString()),
            Tuple.Create("{{CopyrightsYear}}", DateTime.UtcNow.Year.ToString()),
            Tuple.Create("{{MsStoreBadge}}", $"{Encoding.UTF8.GetString(Resources.MsStoreBadge)}"),
        });

        Web2.CoreWebView2.NavigateToString(pageHtml);
    }


    private void Web2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        _ = SetWeb2AccentColor();

        _ = Web2.ExecuteScriptAsync("""
            window.onkeydown = (e) => {
                e.preventDefault();
                console.log(e);
                window.chrome.webview?.postMessage({ Name: 'KeyDown', Data: e.key });
            }

            function Button_Clicked(e) {
                e.preventDefault();
                e.stopPropagation();
                console.log(e);
                window.chrome.webview?.postMessage({ Name: 'Button_Clicked', Data: e.target.id });
            };

            document.getElementById('BtnImageGlassStore').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnCheckForUpdate').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnDonate').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnClose').addEventListener('click', Button_Clicked, false);
        """);
    }


    private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;

        BHelper.OpenUrl(e.Uri, "app_about");
    }


    private async Task SetWeb2AccentColor()
    {
        var accent = Config.Theme.ColorPalatte.Accent.WithBrightness(0.2f);
        var rgb = $"{accent.R} {accent.G} {accent.B}";
        await Web2.ExecuteScriptAsync($"""
            document.documentElement.style.setProperty('--Accent', '{rgb}');
            document.documentElement.setAttribute('color-mode', '{(Config.Theme.Settings.IsDarkMode ? "dark" : "light")}');
            """);
    }


    private void Web2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        var msg = BHelper.ParseJson<ExpandoObject>(e.WebMessageAsJson) as dynamic;
        var name = msg.Name.ToString();

        if (name == "KeyDown")
        {
            var key = msg.Data.ToString();
            if (key == "Escape")
            {
                Close();
            }
        }
        else if (name == "Button_Clicked")
        {
            var btnId = msg.Data.ToString();

            if (btnId.Equals("BtnImageGlassStore", StringComparison.InvariantCultureIgnoreCase))
            {
                Local.FrmMain.IG_OpenMsStore();
            }
            else if (btnId.Equals("BtnCheckForUpdate", StringComparison.InvariantCultureIgnoreCase))
            {
                Local.FrmMain.IG_CheckForUpdate(true);
            }
            else if (btnId.Equals("BtnDonate", StringComparison.InvariantCultureIgnoreCase))
            {
                BHelper.OpenUrl("https://imageglass.org/support#donation", "app_about_donate");
            }
            else if (btnId.Equals("BtnClose", StringComparison.InvariantCultureIgnoreCase))
            {
                Close();
            }
        }
    }


}
