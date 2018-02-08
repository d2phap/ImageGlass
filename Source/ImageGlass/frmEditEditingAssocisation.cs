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
using System.IO;
using System.Windows.Forms;

namespace ImageGlass
{
    public partial class frmEditEditingAssocisation : Form
    {
        private bool _isAllowFormClosed = false;
        public string FileExtension { get; set; }
        public string AppName { get; set; }
        public string AppPath { get; set; }
        public string AppArguments { get; set; }

        public frmEditEditingAssocisation()
        {
            InitializeComponent();

            lblFileExtension.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblFileExtension"];
            lblAppName.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblAppName"];
            lblAppPath.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblAppPath"];
            lblAppArguments.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblAppArguments"];

            btnReset.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnReset"];
            btnOK.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnOK"];
            btnClose.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnClose"];
        }

        private void frmEditEditingAssocisation_Load(object sender, EventArgs e)
        {
            txtFileExtension.Text = this.FileExtension;
            txtAppName.Text = this.AppName;
            txtAppPath.Text = this.AppPath;
            txtAppArguments.Text = this.AppArguments;

            txtAppName.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            _isAllowFormClosed = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            AppName = txtAppName.Text.Trim();
            AppPath = txtAppPath.Text.Trim();
            AppArguments = txtAppArguments.Text.Trim();

            if (AppPath.Length > 0 && !File.Exists(AppPath))
            {
                txtAppPath.Focus();
            }

            DialogResult = DialogResult.OK;
            _isAllowFormClosed = true;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtAppName.Text = txtAppPath.Text = txtAppArguments.Text = string.Empty;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.CheckFileExists = true;

            if (o.ShowDialog() == DialogResult.OK)
            {
                txtAppPath.Text = o.FileName;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // disable parent form shotcuts
            return false;
        }

        private void frmEditEditingAssocisation_KeyDown(object sender, KeyEventArgs e)
        {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                DialogResult = DialogResult.Cancel;
                _isAllowFormClosed = true;
            }
        }

        private void frmEditEditingAssocisation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isAllowFormClosed)
            {
                e.Cancel = true;
            }
        }
    }
}
