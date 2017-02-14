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
                if (char.IsDigit(e.KeyChar) || e.KeyChar == (char)8 || e.KeyChar == (char)9 ||
                    e.KeyChar == (char)46 || e.KeyChar == (char)37 || e.KeyChar == (char)39)
                { }
                else
                {
                    //Prevent input char
                    e.Handled = true;
                }
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
