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

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Base.Update;
using ImageGlass.Library.WinAPI;
using ImageGlass.Settings;
using ImageGlass.UI;

namespace igcmd {
    public partial class frmCheckForUpdate: Form {
        private UpdateService updater = new UpdateService();

        public frmCheckForUpdate() {
            InitializeComponent();

            LoadTheme();
        }


        private void LoadTheme() {
            CornerApi.SetImmersiveDarkMode(Handle, Configs.Theme.IsDarkMode);

            var iconPtr = Configs.Theme.Logo.Image.GetHicon();

            // Icon theming
            if (!Configs.Theme.IsShowTitlebarLogo) {
                using var icon = Icon.FromHandle(new Bitmap(48, 48).GetHicon());
                this.Icon = icon;
                FormIcon.SetTaskbarIcon(this, iconPtr);
            }
            else {
                using var icon = Icon.FromHandle(iconPtr);
                this.Icon = icon;
            }
        }

        private async Task CheckForUpdateAsync() {
            ShowHtmlContent($@"
<h1 class=""text-accent"">Checking for an update...</h1>
<div>Current version: {App.Version}</div>
");

            try {
                await updater.GetUpdatesAsync();
            }
            catch (Exception ex) {

                btnDownload.Visible = false;


                ShowHtmlContent($@"
<h1 class=""text-warning"">Could not check for update</h1>
<div>Current version: {App.Version}</div>
<br/>
<div><b>Error message:</b></div>
<div><pre><code>{ex.Message}</code></pre></div>
");

                return;
            }


            // get requirements of the new update
            var updateRequirements = await updater.CheckRequirementsAsync();
            var canUpdate = !updateRequirements.ContainsValue(false);


            string h1Text;
            // has a new update
            if (updater.HasNewUpdate) {
                h1Text = "🚀 A new update is available!";
                btnDownload.Visible = true;
            }

            // no update
            else {
                h1Text = "ImageGlass is up to date! 😉";
                btnDownload.Visible = false;
            }

            var sb = new StringBuilder();
            sb.Append($"<h1 class=\"text-accent\">{h1Text}</h1>");

            // update version info
            sb.Append($"<div>Current version: {App.Version}</div>");
            sb.Append($"<div>Latest version: {updater.CurrentReleaseInfo.Version}</div>");
            sb.Append($"<div>Published date: {updater.CurrentReleaseInfo.PublishedDate}</div>");

            // update requirements
            if (updateRequirements.Count > 0) {
                sb.Append("<hr/>");
                sb.Append($"<div class=\"box {(canUpdate ? "box-success" : "box-danger")}\">");
                if (canUpdate) {
                    sb.Append($"<div>😊 Your system meet all the requirements!</div>");
                }
                else {
                    sb.Append($"<div>Your system <b><u>does not meet</u></b> the new version's requirements!</div>");
                }

                sb.Append("<div style=\"margin-left: 1rem;\">");
                foreach (var item in updateRequirements) {
                    if (item.Value) {
                        sb.Append($"<div><span class=\"text-success\">✔️</span> {item.Key}</div>");
                    }
                    else {
                        sb.Append($"<div><span class=\"text-danger\">❌</span> {item.Key}</div>");
                    }
                }
                sb.Append("</div>");
                sb.Append("</div>");
            }

            // update details
            sb.Append("<hr/>");
            sb.Append($"<h2 class=\"text-accent\">🌟 <a href=\"{updater.CurrentReleaseInfo.ChangelogUrl}?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_update_read_more\" target=\"_blank\">{updater.CurrentReleaseInfo.Title}</a></h2>");
            sb.Append($"<div>{updater.CurrentReleaseInfo.Description.Replace("\r\n", "<br/>")}</div>");


            ShowHtmlContent(sb.ToString());

            Configs.IsNewVersionAvailable = updater.HasNewUpdate && canUpdate;
        }


        private string WebStyles => @"
*,
*::before,
*::after {
    box-sizing: border-box;
}
html, body {
    font-family: 'Segoe UI Variant', 'Segoe UI';
    margin: 0;
    background-color: rgb(198, 203, 204);
}
body {
    margin: 1rem;
    font-size: 0.85rem;
}
pre {
    overflow-x: hidden;
    white-space: pre-wrap;
    word-wrap: break-word;
}
code {
    font-family: Consolas, 'Segoe UI';
    font-weight: 500;
}
b, strong {
    font-weight: 600;
}
p {
    margin-bottom: 0;
}
p:last-child {
    margin-bottom: 0;
}
hr {
    margin: 1rem 0;
    height: 0.05rem;
    background-color: rgba(0,0,0,0.2);
    border: 0;
}
ul {
    padding-left: 1.5rem;
}
h1 {
    font-size: 1.25rem;
    font-weight: 600;
}
h2 {
    font-size: 1rem;
    font-weight: 600;
}
a,
a:visited {
    transition: all ease 300ms;
    color: " + Theme.ConvertColorToHEX(Configs.Theme.AccentColor, true) + @"
}
a:hover {
    color: rgb(0, 102, 212);
    transform: translateY(-1px);
}
a:active {
    color: rgb(0, 0, 0);
    transform: translateY(1px);
    transition: all ease 70ms;
}

.text-accent {
    color: " + Theme.ConvertColorToHEX(Configs.Theme.AccentColor, true) + @"
}
.text-danger {
    color: rgb(219, 19, 24);
}
.text-warning {
    color: rgb(205, 98, 0);
}
.text-success {
    color: rgb(31, 146, 84);
}
.text-info {
    color: rgb(0, 102, 212);
}

.box {
    padding: 0.5rem;
    border-radius: 0.25rem;
}
.box-danger {
    background-color: rgba(219, 19, 24, 0.1);
}
.box-warning {
    background-color: rgba(205, 98, 0, 0.1);
}
.box-success {
    background-color: rgba(31, 146, 84, 0.1);
}
.box-info {
    background-color: rgba(0, 102, 212, 0.1);
}

";

        private void ShowHtmlContent(string htmlContent) {
            web1.Navigate("about:blank");
            web1.Document.OpenNew(false);
            web1.Document.Write($@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>{WebStyles}</style>
</head>
<body>{htmlContent}</body>
</html>
");
            web1.Refresh(WebBrowserRefreshOption.Completely);
        }


        #region Form events
        private void frmMain_Load(object sender, EventArgs e) {
            Directory.CreateDirectory(App.ConfigDir(PathType.Dir, Dir.Temporary));

            _ = CheckForUpdateAsync();

            web1.NewWindow += Web1_NewWindow;
        }

        private void frmCheckForUpdate_FormClosing(object sender, FormClosingEventArgs e) {
        }

        private void Web1_NewWindow(object sender, System.ComponentModel.CancelEventArgs e) {
            var pattern = new Regex("href=\\\"(.+?)\\\"");
            var match = pattern.Match(web1.Document.ActiveElement.OuterHtml);
            var link = match.Groups[1].Value;

            try {
                Process.Start($"{link}?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_update");
            }
            catch { }
        }

        private void btnDownload_Click(object sender, EventArgs e) {
            try {
                Process.Start($"https://imageglass.org/download?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_download");
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void picStoreApp_Click(object sender, EventArgs e) {
            var campaignId = $"IgInAppBadgeV{App.Version}";
            var source = "UpdaterWindow";

            try {
                var url = $"ms-windows-store://pdp/?productid={Constants.MS_APPSTORE_ID}&cid={campaignId}&referrer=appbadge&source={source}";

                Process.Start(url);
            }
            catch {
                try {
                    Process.Start($"https://www.microsoft.com/store/productId/{Constants.MS_APPSTORE_ID}?cid={campaignId}&referrer=appbadge&source={source}");
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        #endregion

    }
}
