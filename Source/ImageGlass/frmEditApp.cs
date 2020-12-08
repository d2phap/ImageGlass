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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass {
    public partial class frmEditApp: Form {
        private bool _isAllowFormClosed;
        public string FileExtension { get; set; }
        public string AppName { get; set; }
        public string AppPath { get; set; }
        public string AppArguments { get; set; }

        public frmEditApp() {
            InitializeComponent();

            lblFileExtension.Text = Configs.Language.Items[$"{this.Name}.lblFileExtension"];
            lblAppName.Text = Configs.Language.Items[$"{this.Name}.lblAppName"];
            lblAppPath.Text = Configs.Language.Items[$"{this.Name}.lblAppPath"];
            lblAppArguments.Text = Configs.Language.Items[$"{this.Name}.lblAppArguments"];
            lblPreviewLabel.Text = Configs.Language.Items[$"{this.Name}.lblPreviewLabel"];

            btnReset.Text = Configs.Language.Items[$"{this.Name}.btnReset"];
            btnOK.Text = Configs.Language.Items[$"{this.Name}.btnOK"];
            btnClose.Text = Configs.Language.Items[$"{this.Name}.btnClose"];
        }

        private void frmEditApp_Load(object sender, EventArgs e) {
            // Issue #543 Prevent usage of characters which cause problems for settings
            txtFileExtension.KeyPress += textBox_KeyPress;
            txtAppName.KeyPress += textBox_KeyPress;
            txtAppPath.KeyPress += textBox_KeyPress;
            txtAppArguments.KeyPress += textBox_KeyPress;

            txtFileExtension.Text = this.FileExtension;
            txtAppName.Text = this.AppName;
            txtAppPath.Text = this.AppPath;
            txtAppArguments.Text = this.AppArguments;

            InsureMacro();
            UpdateCommandPreview();

            txtAppName.Focus();

            txtAppPath.LostFocus += Option_LostFocus;
            txtAppArguments.LostFocus += Option_LostFocus;
        }

        private void btnClose_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            _isAllowFormClosed = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            AppName = txtAppName.Text.Trim();
            AppPath = txtAppPath.Text.Trim();
            AppArguments = txtAppArguments.Text.Trim();

            if (AppPath.Length > 0 && !File.Exists(AppPath)) {
                txtAppPath.Focus(); // TODO shouldn't this also prevent dialog close?
            }

            DialogResult = DialogResult.OK;
            _isAllowFormClosed = true;
        }

        private void btnReset_Click(object sender, EventArgs e) {
            txtAppName.Text = txtAppPath.Text = txtAppArguments.Text = string.Empty;
            InsureMacro();
            UpdateCommandPreview();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using (var o = new OpenFileDialog()) {
                o.CheckFileExists = true;

                if (o.ShowDialog() == DialogResult.OK) {
                    txtAppPath.Text = o.FileName;
                }
            }
            UpdateCommandPreview();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // disable parent form shortcuts
            return false;
        }

        private void frmEditApp_KeyDown(object sender, KeyEventArgs e) {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) {
                DialogResult = DialogResult.Cancel;
                _isAllowFormClosed = true;
            }
        }

        private void frmEditApp_FormClosing(object sender, FormClosingEventArgs e) {
            if (!_isAllowFormClosed) {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Insure the macro is put in the app arguments box
        /// </summary>
        private void InsureMacro() {
            // Make certain the app arguments has the file substitution string
            var txt = txtAppArguments.Text.Trim();
            if (txt.ToLower().Contains(EditApp.FileMacro))
                return;

            //use double quotes as default
            txtAppArguments.Text += (txt.Length > 0 ? " " : "") + $"\"{EditApp.FileMacro}\"";
        }

        /// <summary>
        /// Update command preview
        /// </summary>
        private void UpdateCommandPreview() {
            var appPath = txtAppPath.Text.Trim().Length > 0 ? $"\"{txtAppPath.Text.Trim()}\"" : "";

            var fileSample = Local.ImageList.GetFileName(Local.CurrentIndex);
            if (!File.Exists(fileSample)) {
                fileSample = @"C:\fake dir\sample photo.jpg";
            }

            // Something has changed; update the sample text
            txtCommandPreview.Text = $"{appPath} {txtAppArguments.Text.Replace(EditApp.FileMacro, fileSample)}";
        }

        private void Option_LostFocus(object sender, EventArgs e) {
            // Focus was lost from a user-edit control. Make sure the preview is updated.
            UpdateCommandPreview();
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e) {
            // Issue #543: use of square brackets breaks settings, don't let the user enter them
            if (e.KeyChar == '[' || e.KeyChar == ']' || e.KeyChar == '|')
                e.Handled = true;
        }
    }
}
