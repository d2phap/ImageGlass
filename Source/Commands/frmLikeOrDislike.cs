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

namespace igcmd
{
    public partial class frmLikeOrDislike : Form
    {
        private string _expression = "";
        private string _version = "";

        public frmLikeOrDislike(string expression, string version)
        {
            InitializeComponent();

            _expression = expression.ToLower().Trim();
            _version = version.ToLower().Trim();
        }

        private void frmLikeOrDislike_Load(object sender, EventArgs e)
        {
            if (_expression == "iglike")
                this.Text = "Like ImageGlass";
            else if (_expression == "igdislike")
                this.Text = "Dislike ImageGlass";

            lblStatus.Text = "Sending your message, please wait...";
            Thread t = new Thread(new ThreadStart(Send));
            t.Priority = ThreadPriority.BelowNormal;
            t.IsBackground = true;
            t.Start();
        }

        private void Send()
        {
            if (_expression == "iglike")
            {
                if (ImageGlass.Feature.ImageGlass_DownloadFile.Send_Email(
                    "d2phap@gmail.com", "xeko.necromancer@gmail.com",
                    "xeko.necromancer", "nqstarlight",
                    "smtp.gmail.com", "[ImageGlass][" + _version + "][Like]",
                   System.Environment.UserName + " like ImageGlass") == 0)
                {
                    lblStatus.Text = "Can not connect to the internet";
                    picStatus.Image = igcmd.Properties.Resources._del_2;
                }
                else
                {
                    lblStatus.Text = "Thank you!";
                    picStatus.Image = igcmd.Properties.Resources.check;
                }
            }
            else if (_expression == "igdislike")
            {
                if (ImageGlass.Feature.ImageGlass_DownloadFile.Send_Email(
                    "d2phap@gmail.com", "xeko.necromancer@gmail.com",
                    "xeko.necromancer", "nqstarlight",
                    "smtp.gmail.com", "[ImageGlass][" + _version + "][Dislike]",
                   System.Environment.UserName + " dislike ImageGlass") == 0)
                {
                    lblStatus.Text = "Can not connect to the internet";
                    picStatus.Image = igcmd.Properties.Resources._del_2;
                }
                else
                {
                    lblStatus.Text = "Thank you!";
                    picStatus.Image = igcmd.Properties.Resources.check;
                }

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
