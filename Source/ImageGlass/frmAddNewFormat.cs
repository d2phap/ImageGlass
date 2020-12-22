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
using System.Windows.Forms;
using ImageGlass.Settings;

namespace ImageGlass {
    public partial class frmAddNewFormat: Form {
        private bool _isAllowFormClosed;
        public string FileFormat { get; set; }

        public frmAddNewFormat() {
            InitializeComponent();

            lblFileExtension.Text = Configs.Language.Items[$"{this.Name}.lblFileExtension"];
            btnOK.Text = Configs.Language.Items[$"{this.Name}.btnOK"];
            btnClose.Text = Configs.Language.Items[$"{this.Name}.btnClose"];
        }

        private void btnClose_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            _isAllowFormClosed = true;
        }

        private void btnOK_Click(object sender, EventArgs e) {
            FileFormat = txtFileExtension.Text.ToLower().Trim();

            if (FileFormat.Length < 2 || !FileFormat.StartsWith(".") || Configs.AllFormats.Contains(FileFormat)) {
                txtFileExtension.Focus();
                return;
            }

            // KBR 20191212 doing this causes the extension to be shown as (e.g.) "*.foo;" in the dialog,
            // and as (initially) saved to the config file. Seems to serve no purpose.
            //FileFormat = $"*{FileFormat};"; // standalize extension string

            DialogResult = DialogResult.OK;
            _isAllowFormClosed = true;
        }

        private void frmAddNewFormat_Load(object sender, EventArgs e) {
            txtFileExtension.Text = this.FileFormat;

            txtFileExtension.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // disable parent form shotcuts
            return false;
        }

        private void frmAddNewFormat_KeyDown(object sender, KeyEventArgs e) {
            // close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) {
                DialogResult = DialogResult.Cancel;
                _isAllowFormClosed = true;
            }
        }

        private void frmAddNewFormat_FormClosing(object sender, FormClosingEventArgs e) {
            if (!_isAllowFormClosed) {
                e.Cancel = true;
            }
        }
    }
}
