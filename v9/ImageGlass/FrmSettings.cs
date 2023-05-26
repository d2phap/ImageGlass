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
using ImageGlass.UI;
using System.Diagnostics;
using System.Text;

namespace ImageGlass;

public partial class FrmSettings : WebForm
{
    public FrmSettings()
    {
        InitializeComponent();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        PageName = "settings";
        Text = Config.Language[$"{nameof(FrmMain)}.{nameof(Local.FrmMain.MnuSettings)}"];
    }


    protected override void OnWeb2Ready()
    {
        base.OnWeb2Ready();

        _ = LoadWeb2ContentAsync(Settings.Properties.Resources.Page_Settings +
            $"<script>{Settings.Properties.Resources.Script_Settings}</script>");
    }


    protected override IEnumerable<(string Variable, string Value)> OnWebTemplateParsing()
    {
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        var msStoreBadge = Encoding.UTF8.GetString(Settings.Properties.Resources.MsStoreBadge);

        return new List<(string Variable, string Value)>
        {
            ("{{_OK}}", Config.Language[$"_._OK"]),
            ("{{_Cancel}}", Config.Language[$"_._Cancel"]),
            ("{{_Apply}}", Config.Language[$"_._Apply"]),
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

            document.getElementById('BtnOK').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnCancel').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnApply').addEventListener('click', Button_Clicked, false);

            document.getElementById('BtnOK').focus();
        """);
    }


    protected override void OnWeb2MessageReceived(string name, string data)
    {
        if (name == "Button_Clicked")
        {
            if (data.Equals("BtnOK", StringComparison.InvariantCultureIgnoreCase))
            {
                //
            }
            else if (data.Equals("BtnApply", StringComparison.InvariantCultureIgnoreCase))
            {
                //
            }
            else if (data.Equals("BtnCancel", StringComparison.InvariantCultureIgnoreCase))
            {
                Close();
            }
        }
    }

    #endregion // Protected / override methods


}
