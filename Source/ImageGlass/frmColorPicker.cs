using ImageGlass.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImageGlass.Theme;
using System.Runtime.InteropServices;
using ImageGlass.Services.Configuration;

namespace ImageGlass
{
    public partial class frmColorPicker : Form
    {
        
        // default location offset on the parent form
        private static Point DefaultLocationOffset = new Point((int)(20 * DPIScaling.GetDPIScaleFactor()), (int)(80 * DPIScaling.GetDPIScaleFactor()));

        private Form _currentOwner = null;
        private ImageBox _imgBox = null;
        private BitmapBooster _bmpBooster = null;
        private Point _locationOffset = DefaultLocationOffset;
        private Point _cursorPos = new Point();


        public frmColorPicker()
        {
            InitializeComponent();

            //apply current theme
            this.BackColor = txtRGB.BackColor = txtHEX.BackColor = LocalSetting.Theme.BackgroundColor;
            lblPixel.ForeColor = lblRgb.ForeColor = lblHex.ForeColor = txtRGB.ForeColor = txtHEX.ForeColor = LocalSetting.Theme.TextInfoColor;
            
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
            _imgBox.Click += _imgBox_Click;
        }


        #region Borderless form moving
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        private void frmColorPicker_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion


        #region Create shadow for borderless form

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled = false;              // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT: // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);

                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };

                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default:
                    break;
            }

            base.WndProc(ref m);
        }
        #endregion
        

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


                #region Shadow for Borderless form
                m_aeroEnabled = CheckAeroEnabled();

                if (!m_aeroEnabled)
                    baseParams.ClassStyle |= CS_DROPSHADOW;
                #endregion


                return baseParams;
            }
        }

        #endregion


        #region Events to manage the form location
        
        private Point parentLocation = Point.Empty;
        private Point parentOffset = Point.Empty;
        private bool formOwnerMoving = false;



        private void _AttachEventsToParent(Form frmOwner)
        {
            if (frmOwner == null)
                return;

            parentLocation = this.Owner.Location;

            frmOwner.Move += Owner_Move;
            frmOwner.SizeChanged += Owner_Move;
            frmOwner.VisibleChanged += Owner_Move;
            frmOwner.Deactivate += FrmOwner_Deactivate;
            frmOwner.LocationChanged += FrmOwner_LocationChanged;
        }

        private void FrmOwner_LocationChanged(object sender, EventArgs e)
        {
            formOwnerMoving = false;
        }

        private void _DetachEventsFromParent(Form frmOwner)
        {
            if (frmOwner == null)
                return;

            frmOwner.Move -= Owner_Move;
            frmOwner.SizeChanged -= Owner_Move;
            frmOwner.VisibleChanged -= Owner_Move;
            frmOwner.Deactivate -= FrmOwner_Deactivate;
            frmOwner.LocationChanged -= FrmOwner_LocationChanged;
        }

        private void FrmOwner_Deactivate(object sender, EventArgs e)
        {
            this.TopMost = false;
        }

        private void frmColorPicker_Move(object sender, EventArgs e)
        {
            if (!formOwnerMoving)
            {
                _locationOffset = new Point(this.Left - this.Owner.Left, this.Top - this.Owner.Top);
                parentOffset = _locationOffset;
            }
        }

        private void Owner_Move(object sender, EventArgs e)
        {
            if (this.Owner == null) return;

            formOwnerMoving = true;

            var parentOffset = new Point(this.Owner.Left - parentLocation.X, this.Owner.Top - parentLocation.Y);

            _SetLocationBasedOnParent();
            parentLocation = this.Owner.Location;
        }

        protected override void OnShown(EventArgs e)
        {
            if (Owner != _currentOwner)
            {
                _DetachEventsFromParent(_currentOwner);
                _currentOwner = Owner;
                _AttachEventsToParent(_currentOwner);
            }
            
            base.OnShown(e);
        }


        private void _SetLocationBasedOnParent()
        {
            if (Owner == null)
                return;

            if (Owner.WindowState == FormWindowState.Minimized || !Owner.Visible)
            {
                Visible = false;
                return;
            }

            // set location based on the main form
            Point ownerLocation = Owner.Location;
            ownerLocation.Offset(parentOffset);

            this.Location = ownerLocation;
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
            _imgBox.Cursor = Cursors.Cross;

            _cursorPos = _imgBox.PointToImage(e.Location);
            if (_cursorPos.X >= 0 && _cursorPos.Y >= 0 && _cursorPos.X < _imgBox.Image.Width
                && _cursorPos.Y < _imgBox.Image.Height)
            {
                lblPixel.Text = string.Format("({0}, {1})", _cursorPos.X, _cursorPos.Y);
            }
        }

        private void _imgBox_Click(object sender, EventArgs e)
        {
            if (_imgBox.Image == null || _bmpBooster == null)
            {
                return;
            }

            if (_cursorPos.X >= 0 && _cursorPos.Y >= 0 && _cursorPos.X < _imgBox.Image.Width
                && _cursorPos.Y < _imgBox.Image.Height)
            {
                Color color = _bmpBooster.get(_cursorPos.X, _cursorPos.Y);
                _DisplayColor(color);
            }
        }

        #endregion
        

        #region Display data

        private void _DisplayColor(Color color)
        {
            panelColor.BackColor = color;

            if (GlobalSetting.IsColorPickerRGBA)
            {
                txtRGB.Text = string.Format("{0}, {1}, {2}, {3}", color.R, color.G, color.B, color.A);
            }
            else
            {
                txtRGB.Text = string.Format("{0}, {1}, {2}", color.R, color.G, color.B);
            }

            if (GlobalSetting.IsColorPickerHEXA)
            {
                txtHEX.Text = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.R, color.G, color.B, color.A);
            }
            else
            {
                txtHEX.Text = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
            }
            
            
            lblPixel.ForeColor = InvertColor(color);
        }

        private void _ResetColor()
        {
            lblPixel.Text = string.Empty;
            txtRGB.Text = string.Empty;
            txtHEX.Text = string.Empty;
        }

        private Color InvertColor(Color c)
        {
            var avgValue = 255 / 2;
            var brightColorCounts = 0;
            var list = new List<int>();

            list.Add(c.R);
            list.Add(c.G);
            list.Add(c.B);

            list.ForEach(li =>
            {
                if (li > avgValue)
                {
                    brightColorCounts++;
                }
            });


            if (brightColorCounts > 1)
            {
                return Color.Black;
            }

            return Color.White;
        }


        private void ColorTextbox_Click(object sender, EventArgs e)
        {
            var txt = (TextBox)sender;
            txt.SelectAll();
        }




        #endregion


        #region Other Form Events
        private void frmColorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            //lblPixel.Text = e.KeyCode.ToString();


            #region ESC or CTRL + SHIFT + K
            //ESC or CTRL + SHIFT + K --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) || //ESC 
                (e.KeyCode == Keys.K && e.Control && e.Shift && !e.Alt))//CTRL + SHIFT + K
            {
                this.Close();
            }
            #endregion
        }


        private void frmColorPicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            LocalSetting.IsColorPickerToolOpening = false;
            GlobalSetting.IsForcedActive = true;

            //Windows Bound-------------------------------------------------------------------
            GlobalSetting.SetConfig($"{Name}.WindowsBound", GlobalSetting.RectToString(Bounds));
        }


        private void frmColorPicker_Load(object sender, EventArgs e)
        {
            //Windows Bound (Position + Size)-------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig($"{Name}.WindowsBound", $"0,0,300,160"));

            if (rc.X == 0 && rc.Y == 0)
            {
                _locationOffset = DefaultLocationOffset;
                parentOffset = _locationOffset;

                _SetLocationBasedOnParent();
            }
            else
            {
                this.Location = rc.Location;
            }

            _ResetColor();



            //Color code ----------------------------------------------------------------
            GlobalSetting.IsColorPickerRGBA = Boolean.Parse(GlobalSetting.GetConfig("IsColorPickerRGBA", "True"));
            GlobalSetting.IsColorPickerHEXA = Boolean.Parse(GlobalSetting.GetConfig("IsColorPickerHEXA", "True"));

            lblRgb.Text = "RGB:";
            lblHex.Text = "HEX:";

            if (GlobalSetting.IsColorPickerRGBA)
            {
                lblRgb.Text = "RGBA:";
            }

            if (GlobalSetting.IsColorPickerHEXA)
            {
                lblHex.Text = "HEXA:";
            }


        }
        #endregion

    }
}
