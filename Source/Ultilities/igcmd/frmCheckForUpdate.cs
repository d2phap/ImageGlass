/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Threading;
using System.Diagnostics;
using ImageGlass.Services;
using ImageGlass.Services.Configuration;

namespace igcmd
{
    public partial class frmCheckForUpdate : Form
    {
        public frmCheckForUpdate()
        {
            InitializeComponent();
        }

        Update up = new Update();
        string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";

        private void btnClose_Click(object sender, EventArgs e)
        {            
            if (File.Exists(tempDir + "update.xml"))
                File.Delete(tempDir + "update.xml");
            Application.Exit();
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = "Connecting to server, please wait...";
            picStatus.Image = igcmd.Properties.Resources.loading;
            Thread t = new Thread(new ThreadStart(CheckForUpdate));
            t.Priority = ThreadPriority.BelowNormal;
            t.IsBackground = true;
            t.Start();

            //CheckForUpdate();

            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(GlobalSetting.StartUpDir + "ImageGlass.exe");
            lblCurentVersion.Text = "Version: " + fv.FileVersion;

        }

        private void CheckForUpdate()
        {
            up = new Update(new Uri("http://www.imageglass.org/checkforupdate"), tempDir + "update.xml");

            if (File.Exists(tempDir + "update.xml"))
            {
                File.Delete(tempDir + "update.xml");
            }

            if (up.IsError)
            {
                lblUpdateVersion.Text = "Not available.";
                lblUpdateVersionType.Text =
                    lblUpdateImportance.Text =
                    lblUpdateSize.Text =
                    lblUpdatePubDate.Text =
                    String.Empty;

                this.Text = "Cannot connect to server.";
                picStatus.Image = igcmd.Properties.Resources.warning;
            }
            else
            {
                lblUpdateVersion.Text = "Version: " + up.Info.NewVersion.ToString();
                lblUpdateVersionType.Text = "Version type: " + up.Info.VersionType;
                lblUpdateImportance.Text = "Importance: " + up.Info.Level;
                lblUpdateSize.Text = "Size: " + up.Info.Size;
                lblUpdatePubDate.Text = "Publish date: " + up.Info.PublishDate.ToString("MMM d, yyyy");

                this.Text = "";

                if (up.CheckForUpdate(GlobalSetting.StartUpDir + "ImageGlass.exe"))
                {
                    if (up.Info.VersionType.ToLower() == "stable")
                    {
                        this.Text = "Your ImageGlass is outdate!";
                    }

                    picStatus.Image = igcmd.Properties.Resources.warning;
                    btnDownload.Enabled = true;
                }
                else
                {
                    btnDownload.Enabled = false;
                    picStatus.Image = igcmd.Properties.Resources.ok;
                }
            }

            //save last update
            GlobalSetting.SetConfig("AutoUpdate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        private void lnkUpdateReadMore_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string version = GlobalSetting.GetConfig("igVersion", "1.0").Replace(".", "_");
                Process.Start(up.Info.Decription + "?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_update_read_more");
            }
            catch
            {
                MessageBox.Show("Check your Internet connection!");
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string version = GlobalSetting.GetConfig("igVersion", "1.0").Replace(".", "_");
                Process.Start(up.Info.Link.ToString() + "?utm_source=app_" + version + "&utm_medium=app_click&utm_campaign=app_update_read_more");
            }
            catch
            {
                MessageBox.Show("Check your Internet connection!");
            }
        }
    }
}
