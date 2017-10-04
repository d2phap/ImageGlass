using ImageGlass.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageGlass
{
    public partial class frmColorPicker : Form
    {
        // opacity when the mouse is out of this form
        private const double FormOpacity = 0.9;

        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point(30, 90);

        private Form _currentOwner = null;
        private ImageBox _imgBox = null;
        private BitmapBooster _bmpBooster = null;
        private Point _locationOffset = DefaultLocationOffset;

        public frmColorPicker()
        {
            InitializeComponent();

            // set the opacity and events to manage it
            Opacity = FormOpacity;
            foreach (var child in Controls)
            {
                ((Control)child).MouseEnter += frmColorPicker_MouseEnter;
            }
        }

        public void SetImageBox(ImageBox imgBox)
        {
            if (_imgBox != null)
            {
                _imgBox.MouseMove -= _imgBox_MouseMove;
                _imgBox.ImageChanged -= _imgBox_ImageChanged;
            }

            _imgBox = imgBox;
            _imgBox_ImageChanged(this, EventArgs.Empty);

            _imgBox.MouseMove += _imgBox_MouseMove;
            _imgBox.ImageChanged += _imgBox_ImageChanged;
        }

        #region Properties to make a tool window

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= 0x8000000 // WS_EX_NOACTIVATE
                    | 0x00000080;   // WS_EX_TOOLWINDOW
                return baseParams;
            }
        }

        #endregion

        #region Events to manage the form location

        private void _AttachEventsToParent(Form frmOwner)
        {
            if (frmOwner == null)
                return;

            frmOwner.Move += Owner_Move;
            frmOwner.SizeChanged += Owner_Move;
            frmOwner.VisibleChanged += Owner_Move;
        }

        private void _DetachEventsFromParent(Form frmOwner)
        {
            if (frmOwner == null)
                return;

            frmOwner.Move -= Owner_Move;
            frmOwner.SizeChanged -= Owner_Move;
            frmOwner.VisibleChanged -= Owner_Move;
        }

        private void Owner_Move(object sender, EventArgs e)
        {
            _SetLocationBasedOnParent();
        }

        protected override void OnShown(EventArgs e)
        {
            if (Owner != _currentOwner)
            {
                _DetachEventsFromParent(_currentOwner);
                _currentOwner = Owner;
                _AttachEventsToParent(_currentOwner);
            }

            _SetLocationBasedOnParent();

            _ResetColor();

            base.OnShown(e);
        }

        private void _SetLocationBasedOnParent()
        {
            if (Owner == null)
                return;

            if (Owner.WindowState == FormWindowState.Minimized
                || !Owner.Visible)
            {
                Visible = false;
                return;
            }

            // set location based on the main form
            Point ownerLocation = Owner.Location;
            ownerLocation.Offset(_locationOffset);
            Location = ownerLocation;
        }

        #endregion

        #region Events to manage ImageBox

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_imgBox != null)
            {
                _imgBox.MouseMove -= _imgBox_MouseMove;
                _imgBox.ImageChanged -= _imgBox_ImageChanged;
            }
        }

        private void _imgBox_ImageChanged(object sender, EventArgs e)
        {
            if (_bmpBooster != null)
            {
                _bmpBooster.Dispose();
            }

            if (_imgBox.Image != null)
            {
                _bmpBooster = new BitmapBooster(new Bitmap(_imgBox.Image));
            }
        }

        private void _imgBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_imgBox.Image == null || _bmpBooster == null)
            {
                return;
            }

            Point imgPixel = _imgBox.PointToImage(e.Location);
            if (imgPixel.X >= 0 && imgPixel.Y >= 0 && imgPixel.X < _imgBox.Image.Width
                && imgPixel.Y < _imgBox.Image.Height)
            {
                Color color = _bmpBooster.get(imgPixel.X, imgPixel.Y);
                _DisplayColor(color, imgPixel);
            }
        }

        #endregion

        #region Opacity events

        private void frmColorPicker_MouseLeave(object sender, EventArgs e)
        {
            Opacity = FormOpacity;
        }

        private void frmColorPicker_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        #endregion

        #region Display data

        private void _DisplayColor(Color color, Point pixel)
        {
            panelColor.BackColor = color;
            lblPixel.Text = string.Format("X:{0} Y:{1}", pixel.X, pixel.Y);
            lblRgb.Text = string.Format("R:{0} G:{1} B:{2}", color.R, color.G, color.B);
            lblHex.Text = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        private void _ResetColor()
        {
            lblPixel.Text = string.Empty;
            lblRgb.Text = string.Empty;
            lblHex.Text = string.Empty;
        }

        #endregion
    }
}
