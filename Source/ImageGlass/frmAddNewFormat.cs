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
        public string ImageExtension { get; set; }
        public ImageExtensionGroup ExtensionGroup { get; set; }

        public frmAddNewFormat()
        {
            InitializeComponent();

            // Add group items
            cmbExtGroup.Items.Add(GlobalSetting.LangPack.Items["_.ImageFormatGroup.Default"]);
            cmbExtGroup.Items.Add(GlobalSetting.LangPack.Items["_.ImageFormatGroup.Optional"]);
            cmbExtGroup.SelectedIndex = 0;

            this.Text = GlobalSetting.LangPack.Items[$"{this.Name}._Text"];
            lblImageExtension.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblImageExtension"];
            lblExtGroup.Text = GlobalSetting.LangPack.Items[$"{this.Name}.lblExtGroup"];
            btnOK.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnOK"];
            btnClose.Text = GlobalSetting.LangPack.Items[$"{this.Name}.btnClose"];
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            ImageExtension = txtImageExtension.Text.ToLower().Trim();

            if (ImageExtension.Length == 0)
            {
                txtImageExtension.Focus();
                return;
            }

            ExtensionGroup = cmbExtGroup.SelectedIndex == 0 ? ImageExtensionGroup.Default : ImageExtensionGroup.Optional;

            DialogResult = DialogResult.OK;
        }

        private void frmAddNewFormat_Load(object sender, EventArgs e)
        {
            txtImageExtension.Focus();
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
            }
        }
    }
}
