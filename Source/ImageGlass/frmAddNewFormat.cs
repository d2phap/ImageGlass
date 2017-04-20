/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using ImageGlass.Services.Configuration;
using System;
using System.Windows.Forms;

namespace ImageGlass
{
    public partial class frmAddNewFormat : Form
    {
        private bool _isAllowFormClosed = false;
        public string FileFormat { get; set; }
        public ImageFormatGroup FormatGroup { get; set; }

        public frmAddNewFormat()
        {
            InitializeComponent();

            // Add group items
            cmbFormatGroup.Items.Add(GlobalSetting.LangPack.Items["_.ImageFormatGroup.Default"]);
            cmbFormatGroup.Items.Add(GlobalSetting.LangPack.Items["_.ImageFormatGroup.Optional"]);
            cmbFormatGroup.SelectedIndex = 0;
            
            lblFileExtension.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblFileExtension"];
            lblFormatGroup.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblFormatGroup"];
            btnOK.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnOK"];
            btnClose.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnClose"];
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            _isAllowFormClosed = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FileFormat = txtFileExtension.Text.ToLower().Trim();
            FormatGroup = (ImageFormatGroup)cmbFormatGroup.SelectedIndex;

            if (FileFormat.Length < 2 || !FileFormat.StartsWith(".") || GlobalSetting.AllImageFormats.Contains(FileFormat))
            {
                txtFileExtension.Focus();
                return;
            }

            FileFormat = $"*{FileFormat};"; //standalize extension string
            DialogResult = DialogResult.OK;
            _isAllowFormClosed = true;
        }

        private void frmAddNewFormat_Load(object sender, EventArgs e)
        {
            txtFileExtension.Text = this.FileFormat;
            cmbFormatGroup.SelectedIndex = (int) this.FormatGroup;

            txtFileExtension.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // disable parent form shotcuts
            return false;
        }

        private void frmAddNewFormat_KeyDown(object sender, KeyEventArgs e)
        {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                DialogResult = DialogResult.Cancel;
                _isAllowFormClosed = true;
            }
        }

        private void frmAddNewFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!_isAllowFormClosed)
            {
                e.Cancel = true;
            }
        }
    }
}
