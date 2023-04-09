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
using System.Text;

namespace ImageGlass;

public partial class FrmAbout : WebForm
{
    public FrmAbout()
    {
        InitializeComponent();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override string OnLoadingWebSource()
    {
        return Resources.Page_About;
    }


    protected override IEnumerable<(string Variable, string Value)> OnWebTemplateParsing()
    {
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        var msStoreBadge = Encoding.UTF8.GetString(Settings.Properties.Resources.MsStoreBadge);

        return new List<(string Variable, string Value)>
        {
            ("{{AppLogo}}", $"data:image/png;base64,{base64Logo}"),
            ("{{AppCode}}", Constants.APP_CODE),
            ("{{AppVersion}}", App.Version),
            ("{{AppArchitecture}}", archInfo),
            ("{{AppRuntime}}", Environment.Version.ToString()),
            ("{{CopyrightsYear}}", DateTime.UtcNow.Year.ToString()),
            ("{{MsStoreBadge}}", msStoreBadge),
        };
    }

    protected override void OnWeb2NavigationCompleted()
    {
        _ = Web2.ExecuteScriptAsync("""
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


    protected override void OnWeb2MessageReceived(string name, string data)
    {
        if (name == "Button_Clicked")
        {
            if (data.Equals("BtnImageGlassStore", StringComparison.InvariantCultureIgnoreCase))
            {
                Local.FrmMain.IG_OpenMsStore();
            }
            else if (data.Equals("BtnCheckForUpdate", StringComparison.InvariantCultureIgnoreCase))
            {
                Local.FrmMain.IG_CheckForUpdate(true);
            }
            else if (data.Equals("BtnDonate", StringComparison.InvariantCultureIgnoreCase))
            {
                BHelper.OpenUrl("https://imageglass.org/support#donation", "app_about_donate");
            }
            else if (data.Equals("BtnClose", StringComparison.InvariantCultureIgnoreCase))
            {
                Close();
            }
        }
    }

    #endregion // Protected / override methods

}
