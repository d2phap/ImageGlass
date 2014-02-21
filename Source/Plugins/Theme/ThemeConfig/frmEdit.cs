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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

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
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

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
            if (c.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.BackColor = c.Color;
            }

        }

        private void lnkPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                OpenFileDialog o = new OpenFileDialog();
                o.Filter = "Supported image formats (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

                if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
                MessageBox.Show("Enter the name of theme to continous", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                n.SetAttribute("backcolor", this.BackColor.ToArgb().ToString());
                n.SetAttribute("statuscolor", btnStatus.ForeColor.ToArgb().ToString());
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
                n.SetAttribute("caro", btnCaro.Tag.ToString());
                n.SetAttribute("fullscreen", btnFullScreen.Tag.ToString());
                n.SetAttribute("slideshow", btnSlideShow.Tag.ToString());
                n.SetAttribute("convert", btnConvert.Tag.ToString());
                n.SetAttribute("print", btnPrintImage.Tag.ToString());
                n.SetAttribute("uploadfb", btnFacebook.Tag.ToString());
                n.SetAttribute("extension", btnExtension.Tag.ToString());
                n.SetAttribute("settings", btnSetting.Tag.ToString());
                n.SetAttribute("about", btnHelp.Tag.ToString());
                n.SetAttribute("like", btnLike.Tag.ToString());
                n.SetAttribute("dislike", btnDisLike.Tag.ToString());
                n.SetAttribute("report", btnReport.Tag.ToString());
                nType.AppendChild(n);

                root.AppendChild(nType);
                doc.AppendChild(root);

                //create temp directory of theme
                string dir = (Application.StartupPath + "\\").Replace("\\\\", "\\") + 
                                "Temp\\" + txtName.Text.Trim(); 
                Directory.CreateDirectory(dir);

                doc.Save(dir + "\\config.xml");//save file
                //copy image
                foreach (string i in ds)
                {
                    if (File.Exists(i))
                    {
                        File.Copy(i, dir + "\\" + Path.GetFileName(i), true);
                    }
                }

                string exe = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe";
                string cmd = "igpacktheme " + char.ConvertFromUtf32(34) + dir +
                    char.ConvertFromUtf32(34) + " " +
                    char.ConvertFromUtf32(34) + s.FileName + char.ConvertFromUtf32(34);
                
                System.Diagnostics.Process.Start(exe, cmd);
                               
                //store file for upload
                lnkSave.Tag = s.FileName;
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

                try
                {
                    picPreview.Image = Image.FromFile(themedir + t.preview);
                    picPreview.Tag = t.preview;
                }
                catch { picPreview.Image = null; }

                //main---------------------------------------------------------------------------------
                try { toolMain.BackgroundImage = Image.FromFile(themedir + t.topbar); toolMain.Tag = t.topbar; }
                catch { toolMain.BackgroundImage = null; }

                try { panThumbnail.BackgroundImage = Image.FromFile(themedir + t.bottombar); panThumbnail.Tag = t.bottombar; }
                catch { panThumbnail.BackgroundImage = null; }

                try { this.BackColor = t.backcolor; }
                catch { this.BackColor = Color.White; }

                try { btnStatus.ForeColor = t.statuscolor; }
                catch { btnStatus.ForeColor = Color.Black; }

                //toolbar_icon--------------------------------------------------------------------------
                try { btnBack.Image = Image.FromFile(themedir + t.back); btnBack.Tag = t.back; }
                catch { btnBack.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnNext.Image = Image.FromFile(themedir + t.next); btnNext.Tag = t.next; }
                catch { btnNext.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnRotateLeft.Image = Image.FromFile(themedir + t.leftrotate); btnRotateLeft.Tag = t.leftrotate; }
                catch { btnRotateLeft.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnRotateRight.Image = Image.FromFile(themedir + t.rightrotate); btnRotateRight.Tag = t.rightrotate; }
                catch { btnRotateRight.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnZoomIn.Image = Image.FromFile(themedir + t.zoomin); btnZoomIn.Tag = t.zoomin; }
                catch { btnZoomIn.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnZoomOut.Image = Image.FromFile(themedir + t.zoomout); btnZoomOut.Tag = t.zoomout; }
                catch { btnZoomOut.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnScale11.Image = Image.FromFile(themedir + t.scaletofit); btnScale11.Tag = t.scaletofit; }
                catch { btnScale11.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnZoomLock.Image = Image.FromFile(themedir + t.zoomlock); btnZoomLock.Tag = t.zoomlock; }
                catch { btnZoomLock.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnScaletoWidth.Image = Image.FromFile(themedir + t.scaletowidth); btnScaletoWidth.Tag = t.scaletowidth; }
                catch { btnScaletoWidth.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnScaletoHeight.Image = Image.FromFile(themedir + t.scaletoheight); btnScaletoHeight.Tag = t.scaletoheight; }
                catch { btnScaletoHeight.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnWindowAutosize.Image = Image.FromFile(themedir + t.autosizewindow); btnWindowAutosize.Tag = t.autosizewindow; }
                catch { btnWindowAutosize.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnOpen.Image = Image.FromFile(themedir + t.open); btnOpen.Tag = t.open; }
                catch { btnOpen.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnRefresh.Image = Image.FromFile(themedir + t.refresh); btnRefresh.Tag = t.refresh; }
                catch { btnRefresh.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnGoto.Image = Image.FromFile(themedir + t.gotoimage); btnGoto.Tag = t.gotoimage; }
                catch { btnGoto.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnThumb.Image = Image.FromFile(themedir + t.thumbnail); btnThumb.Tag = t.thumbnail; }
                catch { btnThumb.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnCaro.Image = Image.FromFile(themedir + t.caro); btnCaro.Tag = t.caro; }
                catch { btnCaro.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnFullScreen.Image = Image.FromFile(themedir + t.fullscreen); btnFullScreen.Tag = t.fullscreen; }
                catch { btnFullScreen.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnSlideShow.Image = Image.FromFile(themedir + t.slideshow); btnSlideShow.Tag = t.slideshow; }
                catch { btnSlideShow.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnConvert.Image = Image.FromFile(themedir + t.convert); btnConvert.Tag = t.convert; }
                catch { btnConvert.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnPrintImage.Image = Image.FromFile(themedir + t.print); btnPrintImage.Tag = t.print; }
                catch { btnPrintImage.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnFacebook.Image = Image.FromFile(themedir + t.uploadfb); btnFacebook.Tag = t.uploadfb; }
                catch { btnFacebook.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnExtension.Image = Image.FromFile(themedir + t.extension); btnExtension.Tag = t.extension; }
                catch { btnExtension.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnSetting.Image = Image.FromFile(themedir + t.settings); btnSetting.Tag = t.settings; }
                catch { btnSetting.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnHelp.Image = Image.FromFile(themedir + t.about); btnHelp.Tag = t.about; }
                catch { btnHelp.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnLike.Image = Image.FromFile(themedir + t.like); btnLike.Tag = t.like; }
                catch { btnLike.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnDisLike.Image = Image.FromFile(themedir + t.dislike); btnDisLike.Tag = t.dislike; }
                catch { btnDisLike.Image = ThemeConfig.Properties.Resources.noimg; }

                try { btnReport.Image = Image.FromFile(themedir + t.report); btnReport.Tag = t.report; }
                catch { btnReport.Image = ThemeConfig.Properties.Resources.noimg; }

                //add ds
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
