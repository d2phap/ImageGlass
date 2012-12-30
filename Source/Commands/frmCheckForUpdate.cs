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
using ImageGlass.Feature;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace igcmd
{
    public partial class frmCheckForUpdate : Form
    {
        public frmCheckForUpdate()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (File.Exists("C:\\ImageGlass-Update.txt")) File.Delete("C:\\ImageGlass-Update.txt");
            Application.Exit();
        }


        private void frmMain_Load(object sender, EventArgs e)
        {
            ThemeConfig.RenderTheme r = new ThemeConfig.RenderTheme();
            r.ApplyTheme(lv);

            lblStatus.Text = "Connecting to server, please wait...";
            Thread t = new Thread(new ThreadStart(CheckForUpdate));
            t.Priority = ThreadPriority.BelowNormal;
            t.IsBackground = true;
            t.Start();
        }
            

        //download failed
        void d_FileDownloadFailed(Exception ex)
        {
            lblStatus.Text = "Cannot connect to server";
            picStatus.Image = igcmd.Properties.Resources._del_2;
            lv.Enabled = false;
        }

        //download complete
        void d_FileDownloadComplete()
        {
            if (File.Exists("C:\\ImageGlass-Update.txt"))
            {
                FileVersionInfo f;
                StreamReader r = new StreamReader("C:\\ImageGlass-Update.txt");
                List<string> ds = new List<string>();
                int c = 0;//dem so luong can update

                while (!r.EndOfStream)
                {
                    ds.Add(r.ReadLine());
                }

                string dir = (Application.StartupPath + "\\").Replace("\\\\", "\\");//duong dan cai dat

                //Kiem tra phien ban cua chuong trinh--------------------------------------------------------
                f = FileVersionInfo.GetVersionInfo(dir + "ImageGlass.exe");
                string v = ds[0].Split('#')[0];//lay phien ban update
                ListViewItem i = lv.Items.Add("ImageGlass.exe");
                i.Tag = "x";//khong co link
                i.SubItems.Add(f.FileVersion);//phien ban hien tai
                i.SubItems.Add(v);//phien ban moi
                if (v != f.FileVersion)
                {
                    i.Tag = ds[0].Split('#')[1];//them link download      
                    c++;
                }

                

                //bao cao
                if (c == 0)
                {
                    lblStatus.Text = "ImageGlass is up to date";
                    picStatus.Image = igcmd.Properties.Resources.check;
                    btnDownload.Enabled = false;
                }
                else
                {
                    lblStatus.Text = "Need to update: " + c.ToString();
                    picStatus.Image = igcmd.Properties.Resources.ques;
                    btnDownload.Enabled = true;
                }

                r.Close();
                File.Delete("C:\\ImageGlass-Update.txt");
                lv.Enabled = true;
            }
        }


        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (lv.SelectedItems.Count > 0)
            {
                foreach (ListViewItem i in lv.SelectedItems)
                {
                    if (i.Tag.ToString() != "x")
                    {
                        Process.Start(i.Tag.ToString());
                    }
                }
            }
        }

        private void CheckForUpdate()
        {
            
            ImageGlass.Feature.ImageGlass_DownloadFile d = new ImageGlass.Feature.ImageGlass_DownloadFile();
            d.FileDownloadComplete += new ImageGlass_DownloadFile.FileDownloadCompleteEventHandler(d_FileDownloadComplete);
            d.FileDownloadFailed += new ImageGlass_DownloadFile.FileDownloadFailedEventHandler(d_FileDownloadFailed);

            try
            {
                if (File.Exists("C:\\ImageGlass-Update.txt"))
                {
                    File.Delete("C:\\ImageGlass-Update.txt");
                }

                d.DownloadFile("http://imageglass.googlecode.com/files/ImageGlass-Update.txt", "C:\\ImageGlass-Update.txt");

            }
            catch
            {
                lblStatus.Text = "Can not connect to server";
                picStatus.Image = igcmd.Properties.Resources._del_2;
            }

        }

        private void lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lv.SelectedItems[0].Tag.ToString() == "x")
                {
                    btnDownload.Enabled = false;
                }
                else
                {
                    btnDownload.Enabled = true;
                }
            }
            catch { }
        }

    }
}
