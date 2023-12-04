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
using ImageGlass.Settings;
using System.Diagnostics;

namespace ImageGlass;

public partial class FrmAbout : WebForm
{
    public FrmAbout()
    {
        InitializeComponent();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        Web2.PageName = "about";
        Text = Config.Language[$"{nameof(FrmMain)}.{nameof(Local.FrmMain.MnuAbout)}"];
        CloseFormHotkey = Keys.Escape;
    }


    protected override async Task OnWeb2ReadyAsync()
    {
        await base.OnWeb2ReadyAsync();

        var htmlFilePath = App.StartUpDir(Dir.WebUI, $"{nameof(FrmAbout)}.html");
        Web2.Source = new Uri(htmlFilePath);
    }


    protected override async Task OnWeb2NavigationCompleted()
    {
        await base.OnWeb2NavigationCompleted();

        // load data
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";

        await Web2.ExecuteScriptAsync(@$"
            window._page.loadData({{
                AppLogo: 'data:image/png;base64,{base64Logo}',
                AppCode: '{Const.APP_CODE}',
                AppVersion: '{App.Version}',
                AppArchitecture: '{archInfo}',
                AppRuntime: '{Environment.Version.ToString()}',
                WebView2Runtime: '{Web2.Webview2Version?.ToString()}',
            }});
        ");
    }


    protected override async Task OnWeb2MessageReceivedAsync(Web2MessageReceivedEventArgs e)
    {
        await base.OnWeb2MessageReceivedAsync(e);

        if (e.Name.Equals("BtnImageGlassStore", StringComparison.OrdinalIgnoreCase))
        {
            BHelper.OpenImageGlassMsStore();
        }
        else if (e.Name.Equals("BtnCheckForUpdate", StringComparison.OrdinalIgnoreCase))
        {
            FrmMain.IG_CheckForUpdate(true);
        }
        else if (e.Name.Equals("BtnDonate", StringComparison.OrdinalIgnoreCase))
        {
            _ = BHelper.OpenUrlAsync("https://imageglass.org/support#donation", "from_about_donate");
        }
        else if (e.Name.Equals("BtnClose", StringComparison.OrdinalIgnoreCase))
        {
            Close();
        }
        else if (e.Name.Equals("ViewLicenseFile", StringComparison.OrdinalIgnoreCase))
        {
            var filePath = App.StartUpDir(Dir.License, e.Data);

            try
            {
                using var proc = Process.Start(new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true,
                });
            }
            catch { }
        }
    }

    #endregion // Protected / override methods

}
