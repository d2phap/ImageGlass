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
using System.Threading;
using Ionic.Zip;
using System.IO;

namespace igcmd
{
    public partial class frmUpload : Form
    {
        public frmUpload(string cmd, string filename)
        {
            InitializeComponent();

            _cmd = cmd.ToLower().Trim();
            _filename = filename.Trim();
        }

        private string _cmd = "";
        private string _filename = "";

        private void frmUpload_Load(object sender, EventArgs e)
        {
            if(_cmd == "theme")
            {
                //cmd: igcmd.exe igupload theme "srcFile"

                lblStatus.Text = "Uploading theme, please wait...";
                Thread t = new Thread(new ThreadStart(Upload));
                t.Priority = ThreadPriority.BelowNormal;
                t.IsBackground = true;
                t.Start();
            }
        }


        private void Upload()
        {
            if (ImageGlass.Feature.ImageGlass_DownloadFile.Send_Email("d2phap@gmail.com",
                "xeko.necromancer@gmail.com", "xeko.necromancer", "nqstarlight",
                "smtp.gmail.com", "[ImageGlass][Theme]", System.Environment.UserName + " (" +
                System.Environment.OSVersion.VersionString + ")",
                _filename) == 0)//loi
            {
                lblStatus.Text = "Cannot connect to the internet";
                picStatus.Image = igcmd.Properties.Resources._del_2;
            }
            else
            {
                lblStatus.Text = "Upload successful. Thank you for sharing!";
                picStatus.Image = igcmd.Properties.Resources.check;
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
