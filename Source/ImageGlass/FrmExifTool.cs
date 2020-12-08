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

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImageGlass.Library.Image;
using ImageGlass.Settings;
using ImageGlass.UI.Renderers;

namespace ImageGlass {
    public partial class FrmExifTool: Form {
        public FrmExifTool() {
            InitializeComponent();

            // Apply theme
            Configs.ApplyFormTheme(this, Configs.Theme);
        }


        private readonly ExifToolWrapper exifTool = new(Configs.ExifToolExePath);


        #region Form events
        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void FrmExif_Load(object sender, EventArgs e) {
            SystemRenderer.ApplyTheme(lvExifItems);
            Local_OnImageChanged(null, null);


            // Load config
            // Windows Bound (Position + Size)
            Bounds = Configs.FrmExifToolWindowBound;

            // windows state
            WindowState = Configs.FrmExifToolWindowState;

            Local.OnImageChanged += Local_OnImageChanged;
        }

        private void FrmExif_FormClosing(object sender, FormClosingEventArgs e) {
            Local.OnImageChanged -= Local_OnImageChanged;

            // Save config
            if (WindowState == FormWindowState.Normal) {
                // Windows Bound
                Configs.FrmExifToolWindowBound = Bounds;
            }

            Configs.FrmExifToolWindowState = WindowState;
        }

        private void FrmExifTool_Activated(object sender, EventArgs e) {
            LoadLanguage();
        }

        private void FrmExif_KeyDown(object sender, KeyEventArgs e) {
            // close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) {
                Close();

                return;
            }

            // copy value
            if (e.KeyCode == Keys.C && e.Control && !e.Shift && !e.Alt) {
                btnCopyValue.PerformClick();
            }
        }

        private void btnCopyValue_Click(object sender, EventArgs e) {
            if (lvExifItems.SelectedItems.Count > 0) {
                Clipboard.SetText(lvExifItems.SelectedItems[0].SubItems[2].Text);
            }
        }

        private void lnkSelectExifTool_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            var ofd = new OpenFileDialog() {
                CheckFileExists = true,
                Filter = "exiftool.exe file|*.exe",
            };

            if (ofd.ShowDialog() == DialogResult.OK) {
                var exif = new ExifToolWrapper(ofd.FileName);

                if (!exif.CheckExists()) {
                    lblNotFound.Text = string.Format(
                        Configs.Language.Items[$"{nameof(frmSetting)}.lnkSelectExifTool._NotFound"],
                        ofd.FileName);
                }
                else {
                    Configs.ExifToolExePath = ofd.FileName;
                    Local_OnImageChanged(null, null);
                }
            }
        }

        private async void BtnExport_Click(object sender, EventArgs e) {
            SetFormState(false);

            var sfd = new SaveFileDialog() {
                Filter = "Text file (*.txt)|*.txt",
            };

            if (sfd.ShowDialog() == DialogResult.OK) {
                await this.exifTool.ExportToFileAsync(sfd.FileName);
            }

            SetFormState(true);
        }

        #endregion


        #region Private functions

        /// <summary>
        /// Load language
        /// </summary>
        private void LoadLanguage() {
            var _lang = Configs.Language.Items;

            lnkSelectExifTool.Text = _lang[$"{nameof(frmSetting)}.lnkSelectExifTool"];
            lblNotFound.Text = string.Format(
                    _lang[$"{nameof(frmSetting)}.lnkSelectExifTool._NotFound"],
                    Configs.ExifToolExePath);

            clnProperty.Text = _lang[$"{Name}.{nameof(clnProperty)}"];
            clnValue.Text = _lang[$"{Name}.{nameof(clnValue)}"];

            btnCopyValue.Text = _lang[$"{Name}.{nameof(btnCopyValue)}"];
            btnExport.Text = _lang[$"{Name}.{nameof(btnExport)}"];
            btnClose.Text = _lang[$"{Name}.{nameof(btnClose)}"];
        }


        /// <summary>
        /// Change UI according to existence of Exif tool
        /// </summary>
        /// <param name="isNotFound"></param>
        private void SetUIVisibility(bool isNotFound) {
            if (isNotFound) {
                this.Text = Configs.Language.Items["frmMain.mnuExifTool"];

                panNotFound.Visible = true;
                lvExifItems.Visible = false;
            }
            else {
                this.Text = Path.GetFileName(Configs.ExifToolExePath);
                this.Icon = Icon.ExtractAssociatedIcon(Configs.ExifToolExePath);

                panNotFound.Visible = false;
                lvExifItems.Visible = true;
            }
        }


        private async void Local_OnImageChanged(object sender, EventArgs e) {
            SetFormState(false);

            // check if exif tool exists
            this.exifTool.ToolPath = Configs.ExifToolExePath;
            if (!this.exifTool.CheckExists()) {
                SetUIVisibility(true);

                return;
            }

            SetUIVisibility(false);
            var filename = Local.ImageList.GetFileName(Local.CurrentIndex);

            // preprocess unicode filename and load exif data
            await this.exifTool.LoadAndProcessExifDataAsync(filename);

            lvExifItems.Items.Clear();
            lvExifItems.Groups.Clear();

            // get groups
            var groups = this.exifTool.GroupBy(i => i.Group)
                .Select(group => new { Group = group.Key })
                .Distinct()
                .ToList();

            foreach (var item in groups) {
                lvExifItems.Groups.Add(item.Group, item.Group);
            }

            // count total items
            clnNo.Text = $"({this.exifTool.Count})";


            // load items
            for (var i = 0; i < this.exifTool.Count; i++) {
                var item = this.exifTool[i];
                var li = new ListViewItem((i + 1).ToString()) {
                    Group = lvExifItems.Groups[item.Group],
                };

                // highlight File Name
                if (item.Name == "File Name") {
                    li.Font = new Font(this.Font, FontStyle.Bold);
                }

                _ = li.SubItems.Add(item.Name);
                _ = li.SubItems.Add(item.Value);
                _ = lvExifItems.Items.Add(li);

            }

            SetFormState(true);
        }


        /// <summary>
        /// Set form state to disabled or enabled
        /// </summary>
        /// <param name="enabled"></param>
        private void SetFormState(bool enabled = true) {
            btnCopyValue.Enabled =
                btnExport.Enabled =
                lblNotFound.Enabled = enabled;
        }

        #endregion

    }
}
