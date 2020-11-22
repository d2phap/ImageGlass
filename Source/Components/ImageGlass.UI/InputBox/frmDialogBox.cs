/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.UI {
    public partial class frmDialogBox: Form {
        public string Content {
            get => txtValue.Text;
            set => txtValue.Text = value;
        }

        /// <summary>
        /// Limit the number of characters the user can enter
        /// </summary>
        public int MaxLimit {
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

        /// <summary>
        /// Apply Filter on key pressed event
        /// </summary>
        public bool FilterOnKeyPress { get; set; } = false;

        public frmDialogBox(string title, string message, Theme theme) {
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
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            const int WM_NCHITTEST = 0x84;
            if (m.Msg == WM_NCHITTEST) {
                const int HTCLIENT = 1;
                const int HTCAPTION = 2;
                if (m.Result.ToInt32() == HTCLIENT) {
                    m.Result = (IntPtr)HTCAPTION;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e) {
            if (ValidateInput()) {
                DialogResult = DialogResult.OK;
            }
        }

        private void txtValue_KeyPress(object sender, KeyPressEventArgs e) {
            if (!FilterOnKeyPress || Filter == null) {
                return;
            }

            var accept = Filter(e.KeyChar);
            if (!accept) {
                e.Handled = true;
            }
        }

        private void txtValue_TextChanged(object sender, EventArgs e) {
            _ = ValidateInput();
        }


        private void DialogBox_Load(object sender, EventArgs e) {
            txtValue.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            // disable parent form shotcuts
            return false;
        }

        private void frmDialogBox_KeyDown(object sender, KeyEventArgs e) {
            //close dialog
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) {
                DialogResult = DialogResult.Cancel;
            }
        }

        private bool ValidateInput() {
            var isValid = true;

            if (Filter != null) {
                foreach (var c in txtValue.Text) {
                    if (!Filter(c)) {
                        isValid = false;
                        break;
                    }
                }

                // invalid char
                if (!isValid) {
                    btnOK.Enabled = false;
                    txtValue.BackColor = Color.FromArgb(255, 255, 183, 183);
                }
                else {
                    btnOK.Enabled = true;
                    txtValue.BackColor = Color.White;
                }
            }

            return isValid;
        }

    }
}
