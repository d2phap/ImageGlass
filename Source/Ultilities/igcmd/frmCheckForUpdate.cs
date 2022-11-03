/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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

using ImageGlass.Base;
using ImageGlass.Base.Update;
using ImageGlass.Library.WinAPI;
using ImageGlass.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace igcmd {
    public partial class frmCheckForUpdate: Form {
        private UpdateService updater = new UpdateService();

        public frmCheckForUpdate() {
            InitializeComponent();

            LoadTheme();
        }

        private void LoadTheme() {
            CornerApi.SetImmersiveDarkMode(Handle, Configs.Theme.IsDarkMode);

            // load theme colors
            lblStatus.ForeColor =
                lnkUpdateReadMore.LinkColor =
                lnkUpdateReadMore.VisitedLinkColor = Configs.Theme.AccentColor;

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
            try {
                await updater.GetUpdatesAsync();
            }
            catch (Exception ex) {

                lblStatus.Text = "Could not check for update";
                lblStatus.ForeColor = Color.FromArgb(241, 89, 58);

                picStatus.Image = Properties.Resources.warning;
                btnDownload.Visible = false;
                lnkUpdateReadMore.Visible = false;

                txtUpdates.Text = ex.Message + "\r\n\r\n" + 
                    $"Current version: " + App.Version;

                return;
            }


            Configs.IsNewVersionAvailable = updater.HasNewUpdate;

            // has a new update
            if (Configs.IsNewVersionAvailable) {
                lblStatus.Text = "A new update is available!";
                lblStatus.ForeColor = Color.FromArgb(241, 89, 58);

                picStatus.Image = Properties.Resources.warning;
                btnDownload.Visible = true;
            }

            // no update
            else {
                lblStatus.Text = "ImageGlass is up to date!";
                lblStatus.ForeColor = Configs.Theme.AccentColor;
                btnDownload.Visible = false;
                picStatus.Image = Properties.Resources.ok;
            }


            var sb = new StringBuilder();

            sb.AppendLine(updater.CurrentReleaseInfo.Title);
            sb.AppendLine();
            sb.AppendLine(updater.CurrentReleaseInfo.Description);
            sb.AppendLine();
            sb.AppendLine("Current version: " + App.Version);
            sb.AppendLine("Latest version: " + updater.CurrentReleaseInfo.Version);
            sb.AppendLine("Published date: " + updater.CurrentReleaseInfo.PublishedDate);

            txtUpdates.Text = sb.ToString();
        }


        #region Form events
        private void frmMain_Load(object sender, EventArgs e) {
            Directory.CreateDirectory(App.ConfigDir(PathType.Dir, Dir.Temporary));

            picStatus.Image = Properties.Resources.loading;
            _ = CheckForUpdateAsync();

            txtUpdates.Text = $"Current version: " + App.Version;
        }

        private void frmCheckForUpdate_FormClosing(object sender, FormClosingEventArgs e) {
        }

        private void lnkUpdateReadMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start(updater.CurrentReleaseInfo.ChangelogUrl + $"?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_update_read_more");
            }
            catch {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnDownload_Click(object sender, EventArgs e) {
            try {
                Process.Start($"https://imageglass.org/download?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_download");
            }
            catch {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        #endregion

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
                catch { }
            }
        }
    }
}
