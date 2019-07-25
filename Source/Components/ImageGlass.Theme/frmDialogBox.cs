/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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

namespace ImageGlass.Theme
{
    public partial class frmDialogBox : Form
    {
        private string _title = "";
        private string _message = "";
        private string _content = "";
        private bool _isNumberOnly = false;

        #region Properties
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Text = _title;
            }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public string Content
        {
            get
            {
                _content = txtValue.Text;
                return _content;
            }
            set
            {
                _content = value;
                txtValue.Text = _content;
            }
        }

        public bool IsNumberOnly
        {
            get { return _isNumberOnly; }
            set { _isNumberOnly = value; }
        }
        #endregion

        public frmDialogBox()
        {
            InitializeComponent();

            Title = "";
            Message = "";
            IsNumberOnly = false;

            Text = _title;
            lblMessage.Text = _message;
        }

        public frmDialogBox(string title, string message)
        {
            InitializeComponent();

            Title = title;
            Message = message;
            IsNumberOnly = false;

            Text = _title;
            lblMessage.Text = _message;
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

        
        private void lblClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void txtValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (IsNumberOnly)
            {
                if (char.IsDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back)
                    return;
                e.Handled = true;
                /* KBR 20190302 With the exception of backspace, all these others are handled by the dialog at a higher level.
                 * As a result, percent (%), single quote (') and period (.) would all pass the filter.
                if (e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Tab ||
                    e.KeyChar == (char)Keys.Delete || e.KeyChar == (char)Keys.Left || e.KeyChar == (char)Keys.Right)
                { }
                else
                {
                    //Prevent input char
                    e.Handled = true;
                }
                */
            }
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
