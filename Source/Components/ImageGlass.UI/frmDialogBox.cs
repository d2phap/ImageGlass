﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
Project homepage: https://imageglass.org

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

namespace ImageGlass.UI
{
    public partial class frmDialogBox : Form
    {
        public string Content
        {
            get => txtValue.Text;
            set => txtValue.Text = value;
        }

        /// <summary>
        /// Limit the number of characters the user can enter
        /// </summary>
        public int MaxLimit
        {
            set => txtValue.MaxLength = value;
        }

        /// <summary>
        /// Provide a character filter. Used either to 1) prevent non-numeric characters;
        /// 2) prevent invalid filename characters. If not provided, any character will
        /// be accepted.
        /// </summary>
        /// <returns>true if character is acceptable</returns>
        public delegate bool KeyFilter(char key);

        public KeyFilter Filter { get; set; } = null;

        public frmDialogBox(string title, string message, Theme theme)
        {
            InitializeComponent();

            Text = title;
            lblMessage.Text = message;

            // apply theme
            this.BackColor = theme.BackgroundColor;
            lblMessage.ForeColor = theme.TextInfoColor;
            panel1.BackColor = theme.ToolbarBackgroundColor;
        }

        /// <summary>
        /// Moving form
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            int WM_NCHITTEST = 0x84;
            if (m.Msg == WM_NCHITTEST)
            {
                int HTCLIENT = 1;
                int HTCAPTION = 2;
                if (m.Result.ToInt32() == HTCLIENT)
                    m.Result = (IntPtr)HTCAPTION;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void txtValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Filter == null)
                return;

            bool accept = Filter(e.KeyChar);
            if (!accept)
                e.Handled = true;
        }

        private void DialogBox_Load(object sender, EventArgs e)
        {
            txtValue.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // disable parent form shotcuts
            return false;
        }

        private void frmDialogBox_KeyDown(object sender, KeyEventArgs e)
        {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}