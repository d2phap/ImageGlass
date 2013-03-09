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
        private Color _BACKGROUND_MOUSEENTER = Color.FromArgb(20, 23, 131, 244);

        public Color BACKGROUND_MOUSEENTER
        {
            get { return _BACKGROUND_MOUSEENTER; }
            set { _BACKGROUND_MOUSEENTER = value; }
        }
        private Color _BACKGROUND_MOUSELEAVE = Color.FromArgb(0, 23, 131, 244);

        public Color BACKGROUND_MOUSELEAVE
        {
            get { return _BACKGROUND_MOUSELEAVE; }
            set { _BACKGROUND_MOUSELEAVE = value; }
        }
        private Color _BACKGROUND_MOUSEDOWN = Color.FromArgb(40, 23, 131, 244);

        public Color BACKGROUND_MOUSEDOWN
        {
            get { return _BACKGROUND_MOUSEDOWN; }
            set { _BACKGROUND_MOUSEDOWN = value; }
        }
        private Color _BACKGROUND_NEWVERSION = Color.FromArgb(50, 244, 193, 19);

        public Color BACKGROUND_NEWVERSION
        {
            get { return _BACKGROUND_NEWVERSION; }
            set { _BACKGROUND_NEWVERSION = value; }
        }
        private bool _isNeedUpdate = false;

        public bool IsNeedUpdate
        {
            get { return _isNeedUpdate; }
            set
            {
                _isNeedUpdate = value;
                if (_isNeedUpdate)
                {
                    this.BackColor = _BACKGROUND_NEWVERSION;
                }
                else
                {
                    this.BackColor = Color.White;
                }
            }
        }
        private string _title = string.Empty;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                lblTitle.Text = _title;
            }
        }
        private string _path = string.Empty;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                lblPath.Text = _path;
            }
        }
        private string _currenVersion = string.Empty;

        public string CurrenVersion
        {
            get { return _currenVersion; }
            set
            {
                _currenVersion = value;
                lblVersion.Text = "Current version: " + _currenVersion + " Latest version: " + _newVersion;
                lblVersion.LinkArea = new LinkArea(("Current version: " + _currenVersion + " Latest version: ").Length, lblVersion.Text.Length);
            }
        }
        private string _newVersion = string.Empty;

        public string NewVersion
        {
            get { return _newVersion; }
            set
            {
                _newVersion = value;
                lblVersion.Text = "Current version: " + _currenVersion + "     Latest version: " + _newVersion;
                lblVersion.LinkArea = new LinkArea(("Current version: " + _currenVersion + "     Latest version: ").Length, lblVersion.Text.Length);
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

        #region Stylish
        private void FileListItem_MouseEnter(object sender, EventArgs e)
        {            
            if (_isNeedUpdate)
            {
                this.BackColor = _BACKGROUND_NEWVERSION;
            }
            else
            {
                this.BackColor = _BACKGROUND_MOUSEENTER;
            }
        }

        private void FileListItem_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = _BACKGROUND_MOUSELEAVE;
        }

        private void FileListItem_MouseDown(object sender, MouseEventArgs e)
        {
            this.BackColor = _BACKGROUND_MOUSEDOWN;
        }

        private void FileListItem_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isNeedUpdate)
            {
                this.BackColor = _BACKGROUND_NEWVERSION;
            }
            else
            {
                this.BackColor = _BACKGROUND_MOUSEENTER;
            }
        }
        #endregion
    }
}
