using ImageGlass.Services.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            ImageExtension = txtImageExtension.Text.Trim();
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
