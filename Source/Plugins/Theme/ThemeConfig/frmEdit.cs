/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2016 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using ImageGlass.Services.Configuration;

namespace ThemeConfig
{
    public partial class frmEdit : Form
    {
        public frmEdit()
        {
            InitializeComponent();
        }
        public frmEdit(string dir)
        {
            InitializeComponent();
            themedir = dir;
        }

        private List<string> ds = new List<string>();
        private string themedir = string.Empty;

        private void btnBack_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog();
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg, *.svg)|*.png;*.jpg;*.jpeg;*.svg";

                if (o.ShowDialog() == DialogResult.OK)
                {
                    ToolStripButton b = (ToolStripButton)sender;
                    b.Image = Image.FromFile(o.FileName);
                    b.Tag = Path.GetFileName(o.FileName);

                    //add ds
                    int i = ds.IndexOf(b.Tag.ToString());
                    if (i == -1)
                    {
                        ds.Add(o.FileName);
                    }
                    else
                    {
                        ds[i] = o.FileName;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Selected image is not valid", "Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkToolBar_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog();
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg, *.svg)|*.png;*.jpg;*.jpeg;*.svg";

                if (o.ShowDialog() == DialogResult.OK)
                {
                    toolMain.BackgroundImage = Image.FromFile(o.FileName);
                    toolMain.Tag = Path.GetFileName(o.FileName);
                    //add ds
                    int i = ds.IndexOf(o.FileName);
                    if (i == -1)
                    {
                        ds.Add(o.FileName);
                    }
                    else
                    {
                        ds[i] = o.FileName;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Selected image is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkThumbnail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog();
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg, *.svg)|*.png;*.jpg;*.jpeg;*.svg";

                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    panThumbnail.BackgroundImage = Image.FromFile(o.FileName);
                    panThumbnail.Tag = Path.GetFileName(o.FileName);
                    //add ds
                    int i = ds.IndexOf(o.FileName);
                    if (i == -1)
                    {
                        ds.Add(o.FileName);
                    }
                    else
                    {
                        ds[i] = o.FileName;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Selected image is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkBackColor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            ColorDialog c = new ColorDialog();
            c.FullOpen = true;
            if (c.ShowDialog() == DialogResult.OK)
            {
                this.BackColor = c.Color;
            }

        }

        private void lnkPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog();
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg;";

                if (o.ShowDialog() == DialogResult.OK)
                {
                    picPreview.Image = Image.FromFile(o.FileName);
                    picPreview.Tag = Path.GetFileName(o.FileName);
                    //add ds
                    int i = ds.IndexOf(o.FileName);
                    if (i == -1)
                    {
                        ds.Add(o.FileName);
                    }
                    else
                    {
                        ds[i] = o.FileName;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Selected image is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lnkUpload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(lnkSave.Tag.ToString()))
            {
                //cmd: igcmd.exe igupload theme "srcFile"
                System.Diagnostics.Process.Start(
                    (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe" ,
                    "igupload theme " + 
                    char.ConvertFromUtf32(34) + lnkSave.Tag.ToString() + char.ConvertFromUtf32(34));
            }
        }

        private void lnkSave_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (txtName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter the name of theme to continue", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtVersion.Text.Trim().Length == 0) txtVersion.Text = " ";
            if (txtAuthor.Text.Trim().Length == 0) txtAuthor.Text = " ";
            if (txtEmail.Text.Trim().Length == 0) txtEmail.Text = " ";
            if (txtWebsite.Text.Trim().Length == 0) txtWebsite.Text = " ";
            if (txtMinVersion.Text.Trim().Length == 0) txtMinVersion.Text = " ";
            if (txtDescription.Text.Trim().Length == 0) txtDescription.Text = " ";

            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "ImageGlass theme (*.igtheme)|*.igtheme";

            if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("ImageGlass");//<ImageGlass>
                XmlElement nType = doc.CreateElement("Theme");//<Theme>

                XmlElement n = doc.CreateElement("Info");// <Info>
                n.SetAttribute("name", txtName.Text);
                n.SetAttribute("version", txtVersion.Text);
                n.SetAttribute("author", txtAuthor.Text);
                n.SetAttribute("email", txtEmail.Text);
                n.SetAttribute("website", txtWebsite.Text);
                n.SetAttribute("description", txtDescription.Text);
                n.SetAttribute("type", "ImageGlass Theme Configuration");
                n.SetAttribute("compatibility", txtMinVersion.Text);
                n.SetAttribute("preview", picPreview.Tag.ToString());
                nType.AppendChild(n);

                n = doc.CreateElement("main");// <main>
                n.SetAttribute("topbar", toolMain.Tag.ToString());
                n.SetAttribute("topbartransparent", "0");
                n.SetAttribute("bottombar", panThumbnail.Tag.ToString());
                n.SetAttribute("backcolor", this.BackColor.ToArgb().ToString(GlobalSetting.NumberFormat));
                n.SetAttribute("statuscolor", btnStatus.ForeColor.ToArgb().ToString(GlobalSetting.NumberFormat));
                nType.AppendChild(n);

                n = doc.CreateElement("toolbar_icon");// <toolbar_icon>
                n.SetAttribute("back", btnBack.Tag.ToString());
                n.SetAttribute("next", btnNext.Tag.ToString());
                n.SetAttribute("leftrotate", btnRotateLeft.Tag.ToString());
                n.SetAttribute("rightrotate", btnRotateRight.Tag.ToString());
                n.SetAttribute("zoomin", btnZoomIn.Tag.ToString());
                n.SetAttribute("zoomout", btnZoomOut.Tag.ToString());
                n.SetAttribute("zoomlock", btnZoomLock.Tag.ToString());
                n.SetAttribute("scaletofit", btnScale11.Tag.ToString());
                n.SetAttribute("scaletowidth", btnScaletoWidth.Tag.ToString());
                n.SetAttribute("scaletoheight", btnScaletoHeight.Tag.ToString());
                n.SetAttribute("autosizewindow", btnWindowAutosize.Tag.ToString());
                n.SetAttribute("open", btnOpen.Tag.ToString());
                n.SetAttribute("refresh", btnRefresh.Tag.ToString());
                n.SetAttribute("gotoimage", btnGoto.Tag.ToString());
                n.SetAttribute("thumbnail", btnThumb.Tag.ToString());
                n.SetAttribute("checkedbackground", btnCheckedBackground.Tag.ToString());
                n.SetAttribute("fullscreen", btnFullScreen.Tag.ToString());
                n.SetAttribute("slideshow", btnSlideShow.Tag.ToString());
                n.SetAttribute("convert", btnConvert.Tag.ToString());
                n.SetAttribute("print", btnPrintImage.Tag.ToString());
                n.SetAttribute("uploadfb", btnFacebook.Tag.ToString());
                n.SetAttribute("extension", btnExtension.Tag.ToString());
                n.SetAttribute("settings", btnSetting.Tag.ToString());
                n.SetAttribute("about", btnHelp.Tag.ToString());
                n.SetAttribute("menu", btnMenu.Tag.ToString());
                nType.AppendChild(n);

                root.AppendChild(nType);
                doc.AppendChild(root);

                //create temp directory of theme
                string raw_temp_dir = GlobalSetting.TempDir + txtName.Text.Trim();
                string temp_dir = raw_temp_dir + "\\";
                Directory.CreateDirectory(temp_dir);
                
                doc.Save(temp_dir + "config.xml");//save file
                //copy image
                foreach (string i in ds)
                {
                    if (File.Exists(i))
                    {
                        File.Copy(i, temp_dir + Path.GetFileName(i), true);
                    }
                }

                string exe = GlobalSetting.StartUpDir + "igcmd.exe";
                string cmd = "igpacktheme " + 
                    char.ConvertFromUtf32(34) + raw_temp_dir + char.ConvertFromUtf32(34) + " " +
                    char.ConvertFromUtf32(34) + s.FileName + char.ConvertFromUtf32(34);
                //Clipboard.SetText(exe + " " + cmd);
                //MessageBox.Show(exe + "__" + cmd);
                
                try
                {
                    System.Diagnostics.Process.Start(exe, cmd);

                    //store file for upload
                    lnkSave.Tag = s.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }     
            }


        }

        private void frmEdit_Load(object sender, EventArgs e)
        {
            if (File.Exists(themedir + "config.xml"))
            {
                this.Text = "ImageGlass theme editor (" + themedir + "config.xml)";
                ImageGlass.Theme.Theme t = new ImageGlass.Theme.Theme(themedir + "config.xml");
                lnkSave.Tag = themedir;

                //Info-----------------------------------------------------------------------------
                txtName.Text = t.name;
                txtVersion.Text = t.version;
                txtAuthor.Text = t.author;
                txtEmail.Text = t.email;
                txtWebsite.Text = t.website;
                txtMinVersion.Text = t.compatibility;
                txtDescription.Text = t.description;

                picPreview.Image = t.PreviewImage.Image;
                picPreview.Tag = t.PreviewImage.Filename;

                //main ----------------------------------------------------------------------------
                toolMain.BackColor = t.ToolbarBackgroundColor;
                toolMain.BackgroundImage = t.ToolbarBackgroundImage.Image;
                toolMain.Tag = t.ToolbarBackgroundImage.Filename;
                
                panThumbnail.BackColor = t.ThumbnailBackgroundColor;
                panThumbnail.BackgroundImage = t.ThumbnailBackgroundImage.Image;
                panThumbnail.Tag = t.ThumbnailBackgroundImage.Filename;

                this.BackColor = t.BackgroundColor;

                btnStatus.ForeColor = t.TextInfoColor;

                //toolbar_icon---------------------------------------------------------------------
                btnBack.Image = t.ToolbarIcons.ViewPreviousImage.Image;
                btnBack.Tag = t.ToolbarIcons.ViewPreviousImage.Filename;

                btnNext.Image = t.ToolbarIcons.ViewNextImage.Image;
                btnNext.Tag = t.ToolbarIcons.ViewNextImage.Filename;

                btnRotateLeft.Image = t.ToolbarIcons.RotateLeft.Image;
                btnRotateLeft.Tag = t.ToolbarIcons.RotateLeft.Filename;

                btnRotateRight.Image = t.ToolbarIcons.RotateRight.Image;
                btnRotateRight.Tag = t.ToolbarIcons.RotateRight.Filename;

                btnZoomIn.Image = t.ToolbarIcons.ZoomIn.Image;
                btnZoomIn.Tag = t.ToolbarIcons.ZoomIn.Filename;

                btnZoomOut.Image = t.ToolbarIcons.ZoomOut.Image;
                btnZoomOut.Tag = t.ToolbarIcons.ZoomOut.Filename;

                btnScale11.Image = t.ToolbarIcons.ActualSize.Image;
                btnScale11.Tag = t.ToolbarIcons.ActualSize.Filename;

                btnZoomLock.Image = t.ToolbarIcons.LockRatio.Image;
                btnZoomLock.Tag = t.ToolbarIcons.LockRatio.Filename;

                btnScaletoWidth.Image = t.ToolbarIcons.ScaleToWidth.Image;
                btnScaletoWidth.Tag = t.ToolbarIcons.ScaleToWidth.Filename;

                btnScaletoHeight.Image = t.ToolbarIcons.ScaleToHeight.Image;
                btnScaletoHeight.Tag = t.ToolbarIcons.ScaleToHeight.Filename;

                btnWindowAutosize.Image = t.ToolbarIcons.AdjustWindowSize.Image;
                btnWindowAutosize.Tag = t.ToolbarIcons.AdjustWindowSize.Filename;

                btnOpen.Image = t.ToolbarIcons.OpenFile.Image;
                btnOpen.Tag = t.ToolbarIcons.OpenFile.Filename;

                btnRefresh.Image = t.ToolbarIcons.Refresh.Image;
                btnRefresh.Tag = t.ToolbarIcons.Refresh.Filename;

                btnGoto.Image = t.ToolbarIcons.GoToImage.Image;
                btnGoto.Tag = t.ToolbarIcons.GoToImage.Filename;

                btnThumb.Image = t.ToolbarIcons.ThumbnailBar.Image;
                btnThumb.Tag = t.ToolbarIcons.ThumbnailBar.Filename;

                btnCheckedBackground.Image = t.ToolbarIcons.CheckedBackground.Image;
                btnCheckedBackground.Tag = t.ToolbarIcons.CheckedBackground.Filename;

                btnFullScreen.Image = t.ToolbarIcons.FullScreen.Image;
                btnFullScreen.Tag = t.ToolbarIcons.FullScreen.Filename;

                btnSlideShow.Image = t.ToolbarIcons.Slideshow.Image;
                btnSlideShow.Tag = t.ToolbarIcons.Slideshow.Filename;

                btnConvert.Image = t.ToolbarIcons.Convert.Image;
                btnConvert.Tag = t.ToolbarIcons.Convert.Filename;

                btnPrintImage.Image = t.ToolbarIcons.Print.Image;
                btnPrintImage.Tag = t.ToolbarIcons.Print.Filename;

                btnFacebook.Image = t.ToolbarIcons.Sharing.Image;
                btnFacebook.Tag = t.ToolbarIcons.Sharing.Filename;

                btnExtension.Image = t.ToolbarIcons.Plugins.Image;
                btnExtension.Tag = t.ToolbarIcons.Plugins.Filename;

                btnSetting.Image = t.ToolbarIcons.Settings.Image;
                btnSetting.Tag = t.ToolbarIcons.Settings.Filename;

                btnHelp.Image = t.ToolbarIcons.About.Image;
                btnHelp.Tag = t.ToolbarIcons.About.Filename;

                btnMenu.Image = t.ToolbarIcons.Menu.Image;
                btnMenu.Tag = t.ToolbarIcons.Menu.Filename;


                //add to list----------------------------------------------------------------------
                foreach (ToolStripButton b in toolMain.Items)
                {
                    ds.Add(themedir + b.Tag.ToString());
                }
                ds.Add(themedir + toolMain.Tag.ToString());
                ds.Add(themedir + panThumbnail.Tag.ToString());
                ds.Add(picPreview.Tag.ToString());
            }
            else
            {
                this.Text = "ImageGlass theme editor (new theme)";
            }
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            c.FullOpen = true;
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                btnStatus.ForeColor = c.Color;
            }
        }

    }
}
