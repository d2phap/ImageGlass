using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass.FileList
{
    public partial class FileListItem : UserControl
    {
        public FileListItem()
        {
            InitializeComponent();
        }

        #region Properties
        private Color _BACKGROUND_MOUSEENTER = Color.FromArgb(237, 245, 254);

        public Color BACKGROUND_MOUSEENTER
        {
            get { return _BACKGROUND_MOUSEENTER; }
            set { _BACKGROUND_MOUSEENTER = value; }
        }
        private Color _BACKGROUND_MOUSELEAVE = Color.FromArgb(255, 255, 255);

        public Color BACKGROUND_MOUSELEAVE
        {
            get { return _BACKGROUND_MOUSELEAVE; }
            set { _BACKGROUND_MOUSELEAVE = value; }
        }
        private Color _BACKGROUND_MOUSEDOWN = Color.FromArgb(219, 236, 253);

        public Color BACKGROUND_MOUSEDOWN
        {
            get { return _BACKGROUND_MOUSEDOWN; }
            set { _BACKGROUND_MOUSEDOWN = value; }
        }

        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
            }
        }
        private string _path = string.Empty;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
            }
        }
        private string _currenVersion = string.Empty;

        public string CurrenVersion
        {
            get { return _currenVersion; }
            set
            {
                _currenVersion = value;
            }
        }        
        private Bitmap _imgAvatar;

        public Bitmap ImgAvatar
        {
            get { return _imgAvatar; }
            set
            {
                _imgAvatar = value;
                picAvatar.Image = _imgAvatar;
            }
        }
        #endregion

        /// <summary>
        /// Mouse event:0 (normal), 1 (mouse enter), 2 (mouse down), 3 (mouse up), 4 (mouse leave)
        /// </summary>
        private int _mouseEvent = 0;

        #region Stylish
        
        private void FileListItem_MouseLeave(object sender, EventArgs e)
        {
            _mouseEvent = 4;
            this.Invalidate();
        }

        private void FileListItem_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseEvent = 2;
            this.Invalidate();
        }

        private void FileListItem_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseEvent = 3;
            this.Invalidate();
        }

        private void FileListItem_MouseEnter(object sender, EventArgs e)
        {
            _mouseEvent = 1;            
            this.Invalidate();
        }
        #endregion




        private void FileListItem_Paint(object sender, PaintEventArgs e)
        {
            if (_mouseEvent == 0)
            {
                DrawItem(BACKGROUND_MOUSELEAVE, e.Graphics);
            }
            else if (_mouseEvent == 1)
            {
                DrawItem(BACKGROUND_MOUSEENTER, e.Graphics);
            }
            else if (_mouseEvent == 2)
            {
                DrawItem(BACKGROUND_MOUSEDOWN, e.Graphics);
            }
            else if (_mouseEvent == 3)
            {
                DrawItem(BACKGROUND_MOUSEENTER, e.Graphics);
            }
            else if (_mouseEvent == 4)
            {
                DrawItem(BACKGROUND_MOUSELEAVE, e.Graphics);
            }
        }

        private void DrawItem(Color bgColor, Graphics g)
        {
            string str;

            if (_currenVersion == null)
            {
                str = _title + "\r\n" + _path;
            }
            else
            {
                str = _title + " - version: " + _currenVersion + "\r\n" + _path;
            }


            Font f = new System.Drawing.Font(new FontFamily("Segoe UI"), 9);
            Brush b = Brushes.Black;
            PointF p = new PointF(51, 9);

            g.Clear(bgColor);
            g.DrawString(str, f, b, p);
        }

        private void FileListItem_Load(object sender, EventArgs e)
        {
            string str;

            if (_currenVersion == null)
            {
                str = _title + "\r\n" + _path;
            }
            else
            {                
                str = _title + " - version: " + _currenVersion + "\r\n" + _path;
            }

            this.tip1.SetToolTip(this, str);
        }

        
    }
}
