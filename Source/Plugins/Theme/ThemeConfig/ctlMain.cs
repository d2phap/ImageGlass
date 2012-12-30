/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2012 DUONG DIEU PHAP
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace ThemeConfig
{
	/// <summary>
	/// Summary description for ctlMain.
	/// </summary>
	public class ctlMain : System.Windows.Forms.UserControl
    {
        private ListView lvTheme;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnSave;
        private TabControl tabControl1;
        private TabPage tpgInstalledTheme;
        private TabPage tpgOnlineTheme;
        private LinkLabel lnkGotoStore;
        private Button btnApply;
        private PictureBox picPreview;
        private TextBox txtInfo;
        private ColumnHeader clnThemeName;
        private Label lblCount;
        private Button btnUninstall;
        private Button btnRefresh;
        private PictureBox pictureBox1;
        private LinkLabel lnkHelp;
        private IContainer components;

		public ctlMain()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctlMain));
            this.lvTheme = new System.Windows.Forms.ListView();
            this.clnThemeName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpgInstalledTheme = new System.Windows.Forms.TabPage();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.tpgOnlineTheme = new System.Windows.Forms.TabPage();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lnkGotoStore = new System.Windows.Forms.LinkLabel();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.tabControl1.SuspendLayout();
            this.tpgInstalledTheme.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tpgOnlineTheme.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lvTheme
            // 
            this.lvTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTheme.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clnThemeName});
            this.lvTheme.FullRowSelect = true;
            this.lvTheme.Location = new System.Drawing.Point(6, 29);
            this.lvTheme.MultiSelect = false;
            this.lvTheme.Name = "lvTheme";
            this.lvTheme.ShowItemToolTips = true;
            this.lvTheme.Size = new System.Drawing.Size(253, 270);
            this.lvTheme.TabIndex = 0;
            this.lvTheme.UseCompatibleStateImageBehavior = false;
            this.lvTheme.View = System.Windows.Forms.View.Details;
            this.lvTheme.SelectedIndexChanged += new System.EventHandler(this.lvTheme_SelectedIndexChanged);
            // 
            // clnThemeName
            // 
            this.clnThemeName.Text = "Theme name";
            this.clnThemeName.Width = 220;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(6, 305);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(58, 26);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Install";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Location = new System.Drawing.Point(297, 305);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(127, 26);
            this.btnEdit.TabIndex = 6;
            this.btnEdit.Text = "Create new or edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(142, 305);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(49, 26);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpgInstalledTheme);
            this.tabControl1.Controls.Add(this.tpgOnlineTheme);
            this.tabControl1.Location = new System.Drawing.Point(9, 16);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(535, 365);
            this.tabControl1.TabIndex = 8;
            // 
            // tpgInstalledTheme
            // 
            this.tpgInstalledTheme.Controls.Add(this.lnkHelp);
            this.tpgInstalledTheme.Controls.Add(this.btnRefresh);
            this.tpgInstalledTheme.Controls.Add(this.btnUninstall);
            this.tpgInstalledTheme.Controls.Add(this.lblCount);
            this.tpgInstalledTheme.Controls.Add(this.txtInfo);
            this.tpgInstalledTheme.Controls.Add(this.picPreview);
            this.tpgInstalledTheme.Controls.Add(this.btnApply);
            this.tpgInstalledTheme.Controls.Add(this.lvTheme);
            this.tpgInstalledTheme.Controls.Add(this.btnSave);
            this.tpgInstalledTheme.Controls.Add(this.btnEdit);
            this.tpgInstalledTheme.Controls.Add(this.btnAdd);
            this.tpgInstalledTheme.Location = new System.Drawing.Point(4, 22);
            this.tpgInstalledTheme.Name = "tpgInstalledTheme";
            this.tpgInstalledTheme.Padding = new System.Windows.Forms.Padding(3);
            this.tpgInstalledTheme.Size = new System.Drawing.Size(527, 339);
            this.tpgInstalledTheme.TabIndex = 0;
            this.tpgInstalledTheme.Text = "Installed theme";
            this.tpgInstalledTheme.UseVisualStyleBackColor = true;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(197, 305);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(62, 26);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUninstall.Location = new System.Drawing.Point(70, 305);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(66, 26);
            this.btnUninstall.TabIndex = 3;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCount.Location = new System.Drawing.Point(6, 6);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(102, 15);
            this.lblCount.TabIndex = 9;
            this.lblCount.Text = "Installed themes:";
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.BackColor = System.Drawing.Color.White;
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfo.Location = new System.Drawing.Point(265, 138);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfo.Size = new System.Drawing.Size(253, 157);
            this.txtInfo.TabIndex = 1;
            // 
            // picPreview
            // 
            this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picPreview.BackgroundImage")));
            this.picPreview.Location = new System.Drawing.Point(265, 29);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(253, 100);
            this.picPreview.TabIndex = 7;
            this.picPreview.TabStop = false;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(430, 305);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(88, 26);
            this.btnApply.TabIndex = 7;
            this.btnApply.Text = "Apply theme";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // tpgOnlineTheme
            // 
            this.tpgOnlineTheme.Controls.Add(this.pictureBox1);
            this.tpgOnlineTheme.Controls.Add(this.lnkGotoStore);
            this.tpgOnlineTheme.Location = new System.Drawing.Point(4, 22);
            this.tpgOnlineTheme.Name = "tpgOnlineTheme";
            this.tpgOnlineTheme.Padding = new System.Windows.Forms.Padding(3);
            this.tpgOnlineTheme.Size = new System.Drawing.Size(527, 339);
            this.tpgOnlineTheme.TabIndex = 1;
            this.tpgOnlineTheme.Text = "Get more themes";
            this.tpgOnlineTheme.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(233, 122);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lnkGotoStore
            // 
            this.lnkGotoStore.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lnkGotoStore.AutoSize = true;
            this.lnkGotoStore.Location = new System.Drawing.Point(194, 173);
            this.lnkGotoStore.Name = "lnkGotoStore";
            this.lnkGotoStore.Size = new System.Drawing.Size(128, 15);
            this.lnkGotoStore.TabIndex = 0;
            this.lnkGotoStore.TabStop = true;
            this.lnkGotoStore.Text = "Go to ImageGlass store";
            this.lnkGotoStore.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGotoStore_LinkClicked);
            // 
            // lnkHelp
            // 
            this.lnkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkHelp.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkHelp.Location = new System.Drawing.Point(368, 6);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(150, 15);
            this.lnkHelp.TabIndex = 10;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "How to create your theme?";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // ctlMain
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(554, 396);
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(554, 396);
            this.Load += new System.EventHandler(this.ctlMain_Load);
            this.tabControl1.ResumeLayout(false);
            this.tpgInstalledTheme.ResumeLayout(false);
            this.tpgInstalledTheme.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tpgOnlineTheme.ResumeLayout(false);
            this.tpgOnlineTheme.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

        private void RefreshThemeList()
        {
            string dir = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "Themes";
            lvTheme.Items.Clear();
            lvTheme.Items.Add("Frost Phoenix (default)").Tag = "Frost Phoenix";

            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string s = (d + "\\").Replace("\\\\", "\\");
                    if (File.Exists(s + "config.xml"))
                    {
                        ImageGlass.Theme.Theme t = new ImageGlass.Theme.Theme(s + "config.xml");
                        lvTheme.Items.Add(t.name).Tag = s + "config.xml";
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
            }

            lblCount.Text = "Installed themes: " + lvTheme.Items.Count.ToString();
        }

        private void ctlMain_Load(object sender, EventArgs e)
        {
            ThemeConfig.RenderTheme r = new ThemeConfig.RenderTheme();
            r.ApplyTheme(lvTheme);
            RefreshThemeList();
        }

        private void lnkGotoStore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.imageglass.org/themes.php");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.imageglass.org/themes.php");
        }

        private void CopyFile(string file, string themeName)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Copy(file, (Application.StartupPath + "\\").Replace("\\\\", "\\") + "Themes\\" +
                        themeName + "\\" + Path.GetFileName(file));
                }
            }
            catch { }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass theme (*.igtheme)|*.igtheme|All files (*.*)|*.*";

            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName))
            {
                //cmd: igcmd.exe iginstalltheme "srcFile"
                string exe = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe";
                string cmd = "iginstalltheme " + char.ConvertFromUtf32(34) + 
                                o.FileName + char.ConvertFromUtf32(34);

                System.Diagnostics.Process.Start(exe, cmd);
                RefreshThemeList();
            }

        }

      
        private void lvTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lvTheme.SelectedItems[0].Tag.ToString() == "Frost Phoenix")
                {
                    picPreview.Image = ThemeConfig.Properties.Resources.preview;
                    txtInfo.Text = "Name: Frost Phoenix\r\n" +
                                "Version: 1.0\r\n" +
                                "Author: Duong Dieu Phap\r\n" +
                                "Email: d2phap@gmail.com\r\n" +
                                "Website: phapsoftware.wordpress.com\r\n" +
                                "Compatibility: 1.4\r\n" +
                                "Description: This is default theme.\r\n" +
                                "Download more themes at sites.google.com/site/psimageglass/downloads/theme-store/";
                    btnSave.Enabled = false;
                    return;
                }

                string file = lvTheme.SelectedItems[0].Tag.ToString();
                string dir = Path.GetDirectoryName(file) + "\\";
                ImageGlass.Theme.Theme t = new ImageGlass.Theme.Theme(file);
                try
                {
                    picPreview.Image = Image.FromFile(dir + t.preview);
                }
                catch { picPreview.Image = null; }
                txtInfo.Text = "Name: " + t.name + "\r\n" +
                                "Version: " + t.version + "\r\n" +
                                "Author: " + t.author + "\r\n" +
                                "Email: " + t.email + "\r\n" +
                                "Website: " + t.website + "\r\n" +
                                "Compatibility: " + t.compatibility + "\r\n" +
                                "Description: " + t.description;
                btnSave.Enabled = true;
            }
            catch { }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
                Microsoft.Win32.Registry.SetValue(hkey, "Theme", lvTheme.SelectedItems[0].Tag.ToString());

                DialogResult msg = MessageBox.Show("Restart ImageGlass to complete new theme applying ?", 
                    "Restart", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (msg == DialogResult.Yes)
                {
                    Application.Restart();
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                SaveFileDialog s = new SaveFileDialog();
                s.Filter = "ImageGlass theme (*.igtheme)|*.igtheme";

                if (s.ShowDialog() == DialogResult.OK)
                {                    
                    string exe = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe";
                    string cmd = "igpacktheme " + char.ConvertFromUtf32(34) + 
                        Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString()) +
                        char.ConvertFromUtf32(34) + " " +
                        char.ConvertFromUtf32(34) + s.FileName + char.ConvertFromUtf32(34);

                    System.Diagnostics.Process.Start(exe, cmd);

                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)//edit theme
            {
                frmEdit f = new frmEdit((Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString()) +
                                "\\").Replace("\\\\", "\\"));
                f.Show();
            }
            else //make new theme
            {
                frmEdit f = new frmEdit();
                f.Show();
            }
            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshThemeList();
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                string hkey = "HKEY_CURRENT_USER\\Software\\PhapSoftware\\ImageGlass\\";
                string t = Microsoft.Win32.Registry.GetValue(hkey, "Theme", "Frost Phoenix").ToString();

                if (t == lvTheme.SelectedItems[0].Tag.ToString())
                {
                    MessageBox.Show("Cannot uninstall theme because ImageGlass is using");
                    return;
                }
                else
                {
                    string dir = Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString());
                    picPreview.Image = null;
                    System.GC.Collect();

                    if (Directory.Exists(dir))
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    RefreshThemeList();
                }
            }


        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.imageglass.org/support.php?type=personalize" + 
                    "&utm_source=imageglass&utm_medium=personalize_click&utm_campaign=from_app_" +
                    Application.ProductVersion.Replace(".", "_"));
            }
            catch { }
        }

        

      



	}
}
