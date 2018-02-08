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
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;
using System.Threading;

namespace igcmd
{
    public partial class frmInstallTheme : Form
    {
        private string _filename = "";
        public frmInstallTheme(string filename)
        {
            InitializeComponent();
            _filename = filename.Trim();
        }

        private void frmInstallTheme_Load(object sender, EventArgs e)
        {
            //cmd: iginstalltheme "srcFile"

            lblStatus.Text = "Installing ...";
            //Thread t = new Thread(new ThreadStart(InstallTheme));
            //t.Priority = ThreadPriority.BelowNormal;
            //t.IsBackground = true;
            //t.Start();
            InstallTheme();
        }

        /// <summary>
        /// Install theme
        /// </summary>
        private void InstallTheme()
        {
            string file = _filename;
            if (!File.Exists(file))
            {
                return;
            }

            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\");
            Directory.CreateDirectory(dir);

            using (ZipFile z = new ZipFile(file, Encoding.UTF8))
            {
                z.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(z_ExtractProgress);
                z.ZipError += new EventHandler<ZipErrorEventArgs>(z_ZipError);

                z.ExtractAll(dir, ExtractExistingFileAction.OverwriteSilently);
            };

        }

        private void z_ZipError(object sender, ZipErrorEventArgs e)
        {
            picStatus.Image = igcmd.Properties.Resources.warning;
            lblStatus.Text = "Invalid theme!";
        }

        private void z_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EntriesExtracted == e.EntriesTotal)
            {
                picStatus.Image = igcmd.Properties.Resources.ok;
                lblStatus.Text = "Installed successfully!";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\"));
        }

       
    }
}
