

using ImageGlass.Base;
using ImageGlass.Settings;
using Microsoft.Web.WebView2.Core;

namespace ImageGlass;

public partial class FrmAbout : ThemedForm
{
    private bool IsWeb2Ready { get; set; } = false;


    public FrmAbout()
    {
        InitializeComponent();
        CloseFormHotkey = Keys.Escape;

        Web2.CoreWebView2InitializationCompleted += Web2_CoreWebView2InitializationCompleted;
        Web2.NavigationCompleted += Web2_NavigationCompleted;
        Web2.WebMessageReceived += Web2_WebMessageReceived;

        _ = InitWebview2();

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }

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
        //Web2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

        Web2.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

        Web2.Source = new Uri(App.StartUpDir(@"Html\about.html"));
        Web2.Focus();
    }

    private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        throw new NotImplementedException();
    }


    private void Web2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        //
    }


    private void Web2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        //var msg = e.WebMessageAsJson;
        //var json = WebMessage.FromJson<object>(msg);
        //var code = json?.Code ?? string.Empty;
        //var data = json?.Data ?? null;

        //if (code == WebMessageCodes.UI_SystemThemeChanged)
        //{
        //    bool isLightTheme = data?.ToString() == "light";

        //    if (isLightTheme)
        //    {
        //        UpdateTheme(SystemTheme.Light);
        //    }
        //    else
        //    {
        //        UpdateTheme(SystemTheme.Dark);
        //    }
        //}
    }


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
    }

}
