using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.Core
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
                lblTitle.Text = _title;
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

            this.Title = "";
            this.Message = "";
            this.IsNumberOnly = false;

            this.Text = _title;
            lblMessage.Text = _message;
        }

        public frmDialogBox(string title, string message)
        {
            InitializeComponent();

            this.Title = title;
            this.Message = message;
            this.IsNumberOnly = false;

            this.Text = _title;
            lblMessage.Text = _message;
        }

        /// <summary>
        /// Di chuyển form 
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
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        private void lblClose_MouseDown(object sender, MouseEventArgs e)
        {
            lblClose.BackColor = Color.FromArgb(0, 107, 179);
        }

        private void lblClose_MouseEnter(object sender, EventArgs e)
        {
            lblClose.BackColor = Color.FromArgb(0, 152, 253);
        }

        private void lblClose_MouseLeave(object sender, EventArgs e)
        {
            lblClose.BackColor = Color.FromArgb(0, 122, 204);
        }

        private void lblClose_MouseUp(object sender, MouseEventArgs e)
        {
            lblClose.BackColor = Color.FromArgb(0, 122, 204);
        }

        private void txtValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.IsNumberOnly)
            {
                if (char.IsDigit(e.KeyChar) || e.KeyChar == (char)8 || e.KeyChar == (char)9 ||
                    e.KeyChar == (char)46 || e.KeyChar == (char)37 || e.KeyChar == (char)39)
                { }
                else
                {
                    //Không cho nhập
                    e.Handled = true;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void DialogBox_Load(object sender, EventArgs e)
        {
            txtValue.Focus();
        }
    }
}
