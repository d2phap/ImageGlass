/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using ImageGlass.Library.WinAPI;
using ImageGlass.Services;
using ImageGlass.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace igcmd {
    public partial class frmCheckForUpdate: Form {
        private Update up = new Update();
        private string UpdateInfoFile { get => App.ConfigDir(PathType.File, Dir.Temporary, "update.xml"); }

        public frmCheckForUpdate() {
            InitializeComponent();

            LoadTheme();
        }

        private void LoadTheme() {
            // load theme colors
            lblStatus.ForeColor =
                lnkUpdateReadMore.LinkColor =
                lnkUpdateReadMore.VisitedLinkColor = Configs.Theme.AccentColor;

            // Icon theming
            if (!Configs.Theme.IsShowTitlebarLogo) {
                this.Icon = Icon.FromHandle(new Bitmap(48, 48).GetHicon());
                FormIcon.SetTaskbarIcon(this, Configs.Theme.Logo.Image.GetHicon());
            }
            else {
                this.Icon = Icon.FromHandle(Configs.Theme.Logo.Image.GetHicon());
            }
        }

        private void CheckForUpdate() {
            up = new Update(new Uri("https://imageglass.org/checkforupdate"), UpdateInfoFile);
            Configs.IsNewVersionAvailable = false;

            if (File.Exists(UpdateInfoFile)) {
                File.Delete(UpdateInfoFile);
            }

            var sb = new StringBuilder();

            if (up.IsError) {
                sb.Append("Please visit https://imageglass.org/download to check for updates.");

                lblStatus.Text = "Unable to check for current version online.";
                lblStatus.ForeColor = Color.FromArgb(241, 89, 58);
                picStatus.Image = igcmd.Properties.Resources.warning;
            }
            else {
                sb
                    .Append("The latest ImageGlass information:\r\n")
                    .Append("------------------------------\r\n")
                    .Append("Version: ")
                    .Append(up.Info.NewVersion)
                    .Append("\r\n")
                    .Append("Version type: ")
                    .Append(up.Info.VersionType)
                    .Append("\r\n")
                    .Append("Importance: ")
                    .Append(up.Info.Level)
                    .Append("\r\n")
                    .Append("Size: ")
                    .Append(up.Info.Size)
                    .Append("\r\n")
                    .Append("Publish date: ")
                    .AppendFormat("{0:MMM d, yyyy HH:mm:ss}", up.Info.PublishDate)
                    .AppendLine();

                if (up.CheckForUpdate(App.StartUpDir("ImageGlass.exe"))) {
                    if (string.Equals(up.Info.VersionType, "stable", StringComparison.CurrentCultureIgnoreCase)) {
                        lblStatus.Text = "ImageGlass is out of date!";
                        lblStatus.ForeColor = Color.FromArgb(241, 89, 58);
                    }
                    else {
                        lblStatus.Text = "ImageGlass is up to date!";
                        lblStatus.ForeColor = Configs.Theme.AccentColor;
                    }
                    picStatus.Image = igcmd.Properties.Resources.warning;
                    btnDownload.Enabled = true;

                    Configs.IsNewVersionAvailable = true;
                }
                else {
                    lblStatus.Text = "ImageGlass is up to date!";
                    lblStatus.ForeColor = Configs.Theme.AccentColor;
                    btnDownload.Enabled = false;
                    picStatus.Image = igcmd.Properties.Resources.ok;
                }
            }

            txtUpdates.Text += sb.ToString();
        }

        #region Form events
        private void frmMain_Load(object sender, EventArgs e) {
            Directory.CreateDirectory(App.ConfigDir(PathType.Dir, Dir.Temporary));

            picStatus.Image = igcmd.Properties.Resources.loading;
            var t = new Thread(new ThreadStart(CheckForUpdate)) {
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };
            t.Start();

            var fv = FileVersionInfo.GetVersionInfo(App.StartUpDir("ImageGlass.exe"));

            txtUpdates.Text = $"Current version: {fv.FileVersion}\r\n------------------------------\r\n\r\n";
        }

        private void frmCheckForUpdate_FormClosing(object sender, FormClosingEventArgs e) {
            if (File.Exists(UpdateInfoFile))
                File.Delete(UpdateInfoFile);
        }

        private void lnkUpdateReadMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            try {
                Process.Start(up.Info.Description + $"?utm_source=app_{App.Version}& utm_medium=app_click&utm_campaign=app_update_read_more");
            }
            catch {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnDownload_Click(object sender, EventArgs e) {
            try {
                Process.Start(up.Info.Link.ToString() + $"?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=app_update_read_more");
            }
            catch {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        #endregion

    }
}
