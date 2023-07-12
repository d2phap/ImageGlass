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
        Web2.PageName = "update";
        Text = Config.Language[$"_._CheckForUpdate"];

        // center window in screen
        var workingArea = Screen.FromControl(this).WorkingArea;
        var rect = BHelper.CenterRectToRect(Bounds, workingArea);
        Location = rect.Location;
    }


    protected override async Task OnWeb2ReadyAsync()
    {
        await base.OnWeb2ReadyAsync();

        var htmlFilePath = App.StartUpDir(Dir.WebUI, $"{nameof(FrmUpdate)}.html");
        Web2.CoreWebView2.Navigate(htmlFilePath);
    }


    protected override async Task OnWeb2NavigationCompleted()
    {
        await base.OnWeb2NavigationCompleted();


        // show loading status
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        await Web2.ExecuteScriptAsync(@$"
            window._page.loadData({{
                CurrentVersion: '{App.Version} ({archInfo})',
            }});

            document.documentElement.setAttribute('app-status', 'checking');
        ");


        // check for update
        await _updater.GetUpdatesAsync();
        await Task.Delay(1000);


        // show result
        var status = _updater.HasNewUpdate ? "outdated" : "updated";
        var newVersion = _updater.CurrentReleaseInfo?.Version ?? "";
        var releasedDate = _updater.CurrentReleaseInfo?.PublishedDate.ToString(Constants.DATETIME_FORMAT) ?? "";
        var releaseTitle = _updater.CurrentReleaseInfo?.Title ?? "";
        var releaseLink = _updater.CurrentReleaseInfo?.ChangelogUrl.ToString() ?? "";
        var releaseDetails = _updater.CurrentReleaseInfo?.Description?.Replace("\r\n", "<br/>") ?? "";

        await Web2.ExecuteScriptAsync(@$"
            window._page.loadData({{
                CurrentVersion: '{App.Version} ({archInfo})',
                LatestVersion: '{newVersion}',
                PublishedDate: '{releasedDate}',
                ReleaseTitle: '{releaseTitle}',
                ReleaseLink: '{releaseLink}',
                ReleaseDetails: `{releaseDetails}`,
            }});

            document.documentElement.setAttribute('app-status', '{status}');
        ");
    }


    protected override async Task OnWeb2MessageReceivedAsync(Web2MessageReceivedEventArgs e)
    {
        await base.OnWeb2MessageReceivedAsync(e);

        if (e.Name.Equals("BtnImageGlassStore", StringComparison.InvariantCultureIgnoreCase))
        {
            BHelper.OpenImageGlassMsStore();
        }
        else if (e.Name.Equals("BtnUpdate", StringComparison.InvariantCultureIgnoreCase))
        {
            BHelper.OpenUrl(_updater.CurrentReleaseInfo?.ChangelogUrl.ToString(), $"app_{Web2.PageName}");
        }
        else if (e.Name.Equals("BtnClose", StringComparison.InvariantCultureIgnoreCase))
        {
            Close();
        }
    }

    #endregion // Protected / override methods


}
