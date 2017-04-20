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
using System.Windows.Forms;

namespace igcmd
{
    public enum FormMessageIcons
    {
        Loading = 1,
        OK = 2,
        Warning = 3
    }
    public partial class frmMsg : Form
    {
        public string _title = string.Empty;
        public string _msg = string.Empty;
        public FormMessageIcons _icon = FormMessageIcons.OK;
        public string _btnClose_Text = string.Empty;


        public frmMsg(string title, string msg, FormMessageIcons icon, string btnClose_Text)
        {
            InitializeComponent();

            Text = _title = title;
            lblMessage.Text = _msg = msg;
            _icon = icon;
            btnClose.Text = _btnClose_Text = btnClose_Text;

            if (_icon == FormMessageIcons.Loading)
            {
                picStatus.Image = igcmd.Properties.Resources.loading;
            }
            else if (_icon == FormMessageIcons.Warning)
            {
                picStatus.Image = igcmd.Properties.Resources.warning;
            }
            else
            {
                picStatus.Image = igcmd.Properties.Resources.ok;
            }
        }

        private void frmMsg_Load(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
