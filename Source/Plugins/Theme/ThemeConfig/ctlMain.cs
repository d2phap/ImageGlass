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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using ImageGlass.Services.Configuration;
using System.Diagnostics;

namespace ThemeConfig
{
    /// <summary>
    /// Summary description for ctlMain.
    /// </summary>
    public class ctlMain : UserControl
    {
        private ColumnHeader clnThemeName;
        private Button btnRefresh;
        private Button btnUninstall;
        private Label lblCount;
        private TextBox txtInfo;
        private PictureBox picPreview;
        private Button btnApply;
        private ListView lvTheme;
        private Button btnSave;
        private Button btnEdit;
        private Button btnAdd;
        private Label label1;
        private LinkLabel lnkDownloadTheme;
        private Label lblVersion;
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.clnThemeName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.lvTheme = new System.Windows.Forms.ListView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lnkDownloadTheme = new System.Windows.Forms.LinkLabel();
            this.lblVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // clnThemeName
            // 
            this.clnThemeName.Text = "Theme name";
            this.clnThemeName.Width = 220;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.AutoSize = true;
            this.btnRefresh.Location = new System.Drawing.Point(258, 414);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 25);
            this.btnRefresh.TabIndex = 16;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUninstall.AutoSize = true;
            this.btnUninstall.Enabled = false;
            this.btnUninstall.Location = new System.Drawing.Point(98, 414);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(89, 25);
            this.btnUninstall.TabIndex = 14;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCount.Location = new System.Drawing.Point(19, 53);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(96, 15);
            this.lblCount.TabIndex = 20;
            this.lblCount.Text = "Installed themes:";
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.BackColor = System.Drawing.Color.White;
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfo.Location = new System.Drawing.Point(283, 304);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(515, 100);
            this.txtInfo.TabIndex = 12;
            // 
            // picPreview
            // 
            this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picPreview.Location = new System.Drawing.Point(22, 304);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(255, 100);
            this.picPreview.TabIndex = 18;
            this.picPreview.TabStop = false;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.AutoSize = true;
            this.btnApply.Location = new System.Drawing.Point(674, 414);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(124, 25);
            this.btnApply.TabIndex = 19;
            this.btnApply.Text = "Apply theme";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lvTheme
            // 
            this.lvTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvTheme.FullRowSelect = true;
            this.lvTheme.Location = new System.Drawing.Point(22, 74);
            this.lvTheme.MultiSelect = false;
            this.lvTheme.Name = "lvTheme";
            this.lvTheme.ShowItemToolTips = true;
            this.lvTheme.Size = new System.Drawing.Size(776, 224);
            this.lvTheme.TabIndex = 11;
            this.lvTheme.TileSize = new System.Drawing.Size(260, 35);
            this.lvTheme.UseCompatibleStateImageBehavior = false;
            this.lvTheme.View = System.Windows.Forms.View.Tile;
            this.lvTheme.SelectedIndexChanged += new System.EventHandler(this.lvTheme_SelectedIndexChanged);
            this.lvTheme.Click += new System.EventHandler(this.lvTheme_SelectedIndexChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.AutoSize = true;
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(193, 414);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(59, 25);
            this.btnSave.TabIndex = 15;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.AutoSize = true;
            this.btnEdit.Location = new System.Drawing.Point(504, 414);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(164, 25);
            this.btnEdit.TabIndex = 17;
            this.btnEdit.Text = "Create new theme";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.AutoSize = true;
            this.btnAdd.Location = new System.Drawing.Point(24, 414);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(68, 25);
            this.btnAdd.TabIndex = 13;
            this.btnAdd.Text = "Install";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(285, 25);
            this.label1.TabIndex = 22;
            this.label1.Text = "ImageGlass theme configuration";
            // 
            // lnkDownloadTheme
            // 
            this.lnkDownloadTheme.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkDownloadTheme.AutoSize = true;
            this.lnkDownloadTheme.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkDownloadTheme.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(131)))), ((int)(((byte)(244)))));
            this.lnkDownloadTheme.Location = new System.Drawing.Point(695, 53);
            this.lnkDownloadTheme.Name = "lnkDownloadTheme";
            this.lnkDownloadTheme.Size = new System.Drawing.Size(103, 15);
            this.lnkDownloadTheme.TabIndex = 23;
            this.lnkDownloadTheme.TabStop = true;
            this.lnkDownloadTheme.Text = "Download themes";
            this.lnkDownloadTheme.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDownloadTheme_LinkClicked);
            // 
            // lblVersion
            // 
            this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblVersion.Location = new System.Drawing.Point(698, 25);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(100, 15);
            this.lblVersion.TabIndex = 24;
            this.lblVersion.Text = "vx.x";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ctlMain
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lnkDownloadTheme);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnUninstall);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lvTheme);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(816, 453);
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(816, 453);
            this.Load += new System.EventHandler(this.ctlMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private void RefreshThemeList()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\");

            lvTheme.Items.Clear();
            lvTheme.Items.Add("2017 (Dark)").Tag = "Default";

            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string s = Path.Combine(d, "config.xml");

                    if (File.Exists(s))
                    {
                        ImageGlass.Theme.Theme t = new ImageGlass.Theme.Theme(s);
                        lvTheme.Items.Add(t.name).Tag = s;
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
            Plugin p = new Plugin();

            lblVersion.Text = $"v{p.Version}";
            RenderTheme r = new RenderTheme();
            r.ApplyTheme(lvTheme);
            RefreshThemeList();
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "ImageGlass theme (*.igtheme)|*.igtheme|All files (*.*)|*.*";

            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName))
            {
                //cmd: igcmd.exe iginstalltheme "srcFile"
                string exe = Path.Combine(GlobalSetting.StartUpDir, "igcmd.exe");
                string cmd = "iginstalltheme " + char.ConvertFromUtf32(34) + 
                                o.FileName + char.ConvertFromUtf32(34);

                Process.Start(exe, cmd);
                RefreshThemeList();
            }

        }

      
        private void lvTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvTheme.SelectedIndices.Count > 0)
            {
                string file = lvTheme.SelectedItems[0].Tag.ToString();
                btnSave.Enabled = true;
                btnUninstall.Enabled = true;

                if (file == "Default")
                {
                    file = Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml");
                    btnSave.Enabled = false;
                    btnUninstall.Enabled = false;
                }

                
                string dir = Path.GetDirectoryName(file) + "\\";

                ImageGlass.Theme.Theme t = new ImageGlass.Theme.Theme(file);
                picPreview.Image = t.PreviewImage.Image;

                txtInfo.Text = "Name: " + t.name + "\r\n" +
                                "Version: " + t.version + "\r\n" +
                                "Author: " + t.author + "\r\n" +
                                "Email: " + t.email + "\r\n" +
                                "Website: " + t.website + "\r\n" +
                                "Compatibility: " + t.compatibility + "\r\n" +
                                "Description: " + t.description;
                
                btnEdit.Text = "Edit selected theme";
            }
            else
            {
                picPreview.Image = null;
                txtInfo.Text = "";
                btnSave.Enabled = false;
                btnUninstall.Enabled = false;
                btnEdit.Text = "Create new theme";
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)
            {
                ImageGlass.Theme.Theme.ApplyTheme(lvTheme.SelectedItems[0].Tag.ToString());
               
                DialogResult msg = MessageBox.Show("Restart ImageGlass to complete the new theme applying ?", 
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
                    string exe = Path.Combine(GlobalSetting.StartUpDir, "igcmd.exe");
                    string cmd = "igpacktheme " + char.ConvertFromUtf32(34) + 
                        Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString()) +
                        char.ConvertFromUtf32(34) + " " +
                        char.ConvertFromUtf32(34) + s.FileName + char.ConvertFromUtf32(34);

                    Process.Start(exe, cmd);
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lvTheme.SelectedItems.Count > 0)//edit theme
            {
                frmEdit f = new frmEdit((Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString()) + "\\").Replace("\\\\", "\\"));
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
                string t = Microsoft.Win32.Registry.GetValue(hkey, "Theme", "(default)").ToString();

                if (t == lvTheme.SelectedItems[0].Tag.ToString())
                {
                    MessageBox.Show("Cannot uninstall the selected theme because ImageGlass is using.");
                    return;
                }
                else
                {
                    string dir = Path.GetDirectoryName(lvTheme.SelectedItems[0].Tag.ToString());
                    picPreview.Image = null;
                    GC.Collect();

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
        

        private void lnkDownloadTheme_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = GlobalSetting.GetConfig("AppVersion", "1.0").Replace(".", "_");
                Process.Start("http://www.imageglass.org/download/themes?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_download_theme");
            }
            catch { }
        }

        

      



	}
}
