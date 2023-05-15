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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using igcmd.Properties;
using ImageGlass;
using ImageGlass.Base;
using ImageGlass.Base.Update;
using ImageGlass.Settings;
using System.Text;

namespace igcmd.Tools;

public partial class FrmUpdate : WebForm
{
    private readonly UpdateService _updater = new();

    public FrmUpdate()
    {
        InitializeComponent();
    }


    // Protected / override methods
    #region Protected / override methods

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        Config.UpdateFormIcon(this);
        PageName = "update";
        Text = Config.Language[$"_._CheckForUpdate"];

        // center window in screen
        var workingArea = Screen.FromControl(this).WorkingArea;
        var rect = BHelper.CenterRectToRect(Bounds, workingArea);
        Location = rect.Location;
    }


    protected override void OnWeb2Ready()
    {
        base.OnWeb2Ready();

        _ = CheckForUpdateAsync();
    }


    protected override IEnumerable<(string Variable, string Value)> OnWebTemplateParsing()
    {
        var base64Logo = BHelper.ToBase64Png(Config.Theme.Settings.AppLogo);
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        var msStoreBadge = Encoding.UTF8.GetString(ImageGlass.Settings.Properties.Resources.MsStoreBadge);

        var newVersion = _updater.CurrentReleaseInfo?.Version ?? "";
        var releasedDate = _updater.CurrentReleaseInfo?.PublishedDate.ToString(Constants.DATETIME_FORMAT) ?? "";
        var releaseTitle = _updater.CurrentReleaseInfo?.Title ?? "";
        var releaseLink = _updater.CurrentReleaseInfo?.ChangelogUrl.ToString() ?? "";
        var releaseDetails = _updater.CurrentReleaseInfo?.Description?.Replace("\r\n", "<br/>") ?? "";


        return new List<(string Variable, string Value)>
        {
            ("{{AppLogo}}", $"data:image/png;base64,{base64Logo}"),
            ("{{AppCode}}", Constants.APP_CODE),
            ("{{MsStoreBadge}}", msStoreBadge),

            ("{{ReleaseTitle}}", releaseTitle),
            ("{{ReleaseLink}}", releaseLink),
            ("{{ReleaseDetails}}", releaseDetails),

            // language
            ("{{_StatusChecking}}", Config.Language[$"{nameof(FrmUpdate)}._StatusChecking"]),
            ("{{_StatusUpdated}}", Config.Language[$"{nameof(FrmUpdate)}._StatusUpdated"]),
            ("{{_StatusOutdated}}", Config.Language[$"{nameof(FrmUpdate)}._StatusOutdated"]),

            ("{{_CurrentVersion}}", string.Format(
                Config.Language[$"{nameof(FrmUpdate)}._CurrentVersion"],
                $"{App.Version} ({archInfo})")),

            ("{{_LatestVersion}}", string.Format(
                Config.Language[$"{nameof(FrmUpdate)}._LatestVersion"],
                $"{newVersion} ({archInfo})")),

            ("{{_PublishedDate}}", string.Format(
                Config.Language[$"{nameof(FrmUpdate)}._PublishedDate"],
                releasedDate)),

            ("{{_Update}}", Config.Language[$"_._Update"]),
            ("{{_Close}}", Config.Language[$"_._Close"]),
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
            document.getElementById('BtnUpdate').addEventListener('click', Button_Clicked, false);
            document.getElementById('BtnClose').addEventListener('click', Button_Clicked, false);

            document.getElementById('BtnUpdate').focus();
        """);
    }


    protected override void OnWeb2MessageReceived(string name, string data)
    {
        if (name == "Button_Clicked")
        {
            if (data.Equals("BtnImageGlassStore", StringComparison.InvariantCultureIgnoreCase))
            {
                BHelper.OpenImageGlassMsStore();
            }
            else if (data.Equals("BtnUpdate", StringComparison.InvariantCultureIgnoreCase))
            {
                BHelper.OpenUrl(_updater.CurrentReleaseInfo?.ChangelogUrl.ToString(), $"app_{PageName}");
            }
            else if (data.Equals("BtnClose", StringComparison.InvariantCultureIgnoreCase))
            {
                Close();
            }
        }
    }

    #endregion // Protected / override methods


    private async Task CheckForUpdateAsync()
    {
        // show checking progress
        await LoadWeb2ContentAsync(Resources.Page_Update);
        var status = "checking";
        await Web2.ExecuteScriptAsync($"""
            document.documentElement.setAttribute('app-status', '{status}');
        """);
        await Task.Delay(1000);


        // update the status
        await _updater.GetUpdatesAsync();
        status = _updater.HasNewUpdate ? "outdated" : "updated";
        await LoadWeb2ContentAsync(Resources.Page_Update);

        await Web2.ExecuteScriptAsync($"""
            document.documentElement.setAttribute('app-status', '{status}');
        """);
    }

}
