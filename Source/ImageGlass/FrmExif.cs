/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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
using System.Windows.Forms;
using ImageGlass.Settings;

namespace ImageGlass {
    public partial class FrmExif: Form {
        public FrmExif() {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void FrmExif_Load(object sender, EventArgs e) {
            this.TopMost =
                chkExifToolTopMost.Checked =
                Configs.IsExifToolAlwaysOnTop;

            if (File.Exists(Configs.ExifToolExePath)) {
                this.Text = Path.GetFileName(Configs.ExifToolExePath);
                this.Icon = Icon.ExtractAssociatedIcon(Configs.ExifToolExePath);
            }
        }

        private void chkExifToolTopMost_CheckedChanged(object sender, EventArgs e) {
            this.TopMost =
                Configs.IsExifToolAlwaysOnTop =
                chkExifToolTopMost.Checked;
        }
    }
}
