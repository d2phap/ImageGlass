/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
using ImageGlass.Theme;
using ImageGlass.Services.Configuration;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ImageGlass.ImageListView;

namespace ImageGlass
{
    public partial class frmMetadataView : Form
    {
        // width of the dataview (and therefore the form)
        private const int DATAVIEW_WIDE = 250;

        // default location offset on the parent form
        private static Point DefaultLocationOffset = new Point((int)(20 * DPIScaling.GetDPIScaleFactor()), (int)(DATAVIEW_WIDE * DPIScaling.GetDPIScaleFactor()));

        private Form _currentOwner = null;
        private Point _locationOffset = DefaultLocationOffset;

        private ListView _dataView;

        // The label strings - the lookup names from the localization dataset. The format is "frmMetadataView.<id>",
        // e.g. "frmMetadataView.DateTaken".
        //
        // This is ALSO the order in which the metadata will be displayed in the list! The metadata _values_ are 
        // difficult to fetch via code, so their position is hard coded: if you want to change the order of the
        // items here, you MUST change the row indices in which the _values_ are placed!
        //
        // TODO establish a better mechanism!
        private string[] _dataIds =
        {
            "DateTaken", "CameraMaker", "CameraModel", "Exposure", "FStop",
            "ISO", "FocalLen", "Software", "Artist", "Copyright",
            "Title", "Comment", "Description"
            //TODO not available via GDI, "Tags"
        };

        public frmMetadataView()
        {
            InitializeComponent();

            this.Font = new Font("Segoe UI", 9F);
            AutoSize = true; // size to fit contents

            BuildDataView();
        }

        #region Borderless form moving
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, Int32 lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();


        private void OnMouseDown(object sender, MouseEventArgs e)
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
            //frmOwner.Deactivate += FrmOwner_Deactivate;  // KBR disabled: color picker and this form fighting for focus
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
            //frmOwner.Deactivate -= FrmOwner_Deactivate; // KBR disabled: color picker and this form fighting for focus
            frmOwner.LocationChanged -= FrmOwner_LocationChanged;
        }

        // KBR disabled: doesn't need to be topmost
        //private void FrmOwner_Deactivate(object sender, EventArgs e)
        //{
        //    this.TopMost = false;
        //}

        private void Owner_Move(object sender, EventArgs e)
        {
            if (this.Owner == null) return;

            formOwnerMoving = true;

            var parentOffset = new Point(this.Owner.Left - parentLocation.X, this.Owner.Top - parentLocation.Y);

            _SetLocationBasedOnParent();
            parentLocation = this.Owner.Location;
        }

        private void OnMove(object sender, EventArgs e)
        {
            if (!formOwnerMoving)
            {
                _locationOffset = new Point(this.Left - this.Owner.Left, this.Top - this.Owner.Top);
                parentOffset = _locationOffset;
            }
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            #region ESC or CTRL + SHIFT + K
            //ESC or CTRL + SHIFT + K --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) || //ESC 
                (e.KeyCode == Keys.K && e.Control && e.Shift && !e.Alt))//CTRL + SHIFT + K
            {
                LocalSetting.IsShowMetadataViewOnStartup = false;
                this.Close();
            }
            #endregion
        }


        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            LocalSetting.IsMetadataViewOpening = false;
            LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.METADATA_VIEW_MENU;
        }
		
		public void UpdateUI()
		{
			// Apply current theme
			this.BackColor = LocalSetting.Theme.BackgroundColor;
            _dataView.BackColor = BackColor;

            ForeColor = Theme.Theme.InvertBlackAndWhiteColor(LocalSetting.Theme.BackgroundColor);
            _dataView.ForeColor = ForeColor;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            UpdateUI();

            //Windows Bound (Position + Size)-------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect("0,0,100,100");

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
		}

        #region Tooltip for listview items
        private ToolTip mTooltip;
        private Point mLastPos = new Point(-1, -1);

        private void _dataView_MouseMove(object sender, MouseEventArgs e)
        {
            // Display a tooltip for list view items. This is useful when the metadata value
            // is longer than can be viewed. NOTE: the viewer must have focus before tooltips
            // will show!

            // TODO width limit on tooltip? Currently sizes to width of screen.

            ListViewHitTestInfo info = _dataView.HitTest(e.X, e.Y);

            if (mTooltip == null)
                mTooltip = new ToolTip();

            if (mLastPos != e.Location)
            {
                if (info.Item != null && info.SubItem != null)
                {
                    mTooltip.ToolTipTitle = info.Item.Text;
                    mTooltip.Show(info.SubItem.Text, info.Item.ListView, e.X, e.Y, 4000);
                }
                else
                {
                    mTooltip.SetToolTip(_dataView, string.Empty);
                }
            }

            mLastPos = e.Location;
        }

        #endregion

        private void BuildDataView()
        {
            // Create the control containing the labels and values. 

            _dataView = new ListView();
            Font dvFont = new Font("Segoe UI", 9F);
            _dataView.Font = dvFont;
            _dataView.Scrollable = false; // window will be sized to fit height
            _dataView.BorderStyle = BorderStyle.None;
            _dataView.View = System.Windows.Forms.View.Details; // force list style
            _dataView.Columns.Add("Id");
            _dataView.Columns.Add("Data");
            _dataView.HeaderStyle = ColumnHeaderStyle.None; // don't need header
            _dataView.Columns[0].TextAlign = HorizontalAlignment.Right;
            _dataView.MultiSelect = false; // prevent select rect appearing on mouse down

            // 1. Set the label in the left column. Label is pulled from localized strings.
            // 2. Determine the maximum width of the label text in order to size the first
            //    column to fit.
            int maxW = int.MinValue;
            using (Graphics g = this.CreateGraphics())
                foreach (var id in _dataIds)
                {
                    string lookup = "frmMetadataView." + id;
                    string label = GlobalSetting.LangPack.Items[lookup];

                    SizeF s = g.MeasureString(label, dvFont);
                    maxW = Math.Max(maxW, (int)(Math.Ceiling(s.Width)) + 10); // extra 10 pixel buffer seems necessary
                    _dataView.Items.Add(new ListViewItem(new string[] { label, "" }));
                }

            // First column as wide as longest label.
            // Second column as wide as we have space left
            // TODO does this need DPIScaling?
            _dataView.Columns[0].Width = maxW; 
            _dataView.Columns[1].Width = DATAVIEW_WIDE - maxW - 10;

            // Size the form to fit.
            // TODO might not be tall enough depending on font size? (i.e. when user changes scale)
            _dataView.Height = (int)(_dataIds.Length * 20 * DPIScaling.GetDPIScaleFactor());
            _dataView.Width = (int)(DATAVIEW_WIDE * DPIScaling.GetDPIScaleFactor());
            this.Height = _dataView.Height + 10;

            Controls.Add(_dataView);

            _dataView.ItemSelectionChanged += _dataView_ItemSelectionChanged;
            _dataView.MouseDown += OnMouseDown; // let the form have the event
            _dataView.MouseMove += _dataView_MouseMove;
        }

        private void _dataView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // Prevent any side-effects from selection
            if (e.IsSelected) e.Item.Selected = false;
        }

        #region Populate the metadata from an image
        private void SetValueString(int rowdex, string val)
        {
            // vanilla string
            try
            {
                var lvi = _dataView.Items[rowdex];
                if (string.IsNullOrEmpty(val))
                    lvi.SubItems[1].Text = "";
                else
                    lvi.SubItems[1].Text = val.Trim();
            }
            catch { }
        }

        private void SetFloatString(int rowdex, float val, string format)
        {
            // floating point value
            try
            {
                if (val == 0.0)
                    SetValueString(rowdex, "");
                else
                    SetValueString(rowdex, string.Format(format, val));
            }
            catch { }
        }

        private void SetIntString(int rowdex, int val, string format)
        {
            // integer value
            // TODO don't really need this, use SetFloatString?
            try
            {
                if (val == 0)
                    SetValueString(rowdex, "");
                else
                    SetValueString(rowdex, string.Format(format, val));
            }
            catch { }
        }

        public ImageListViewItem Image
        {
            set
            {
                try
                {
                    // active image has been changed
                    // update metadata controls
                    var local = value;

                    if (local.DateTaken == DateTime.MinValue)
                        SetValueString(0, "");
                    else
                        SetValueString(0, local.DateTaken.ToString("yyyy / MM / dd HH:mm:ss").Trim());

                    SetValueString(1, local.EquipmentMaker);
                    SetValueString(2, local.EquipmentModel);
                    SetFloatString(3, local.ExposureTime, "{0} sec.");
                    SetFloatString(4, local.FNumber, "f/{0}");
                    SetIntString  (5, local.ISOSpeed, "ISO-{0}");
                    SetIntString  (6, (int)Math.Round(local.FocalLength), "{0} mm"); // technically a float but we want it forced to int
                    SetValueString(7, local.Software);
                    SetValueString(8, local.Artist);
                    SetValueString(9, local.Copyright);
                    SetValueString(10, local.Title);
                    SetValueString(11, local.UserComment);
                    SetValueString(12, local.ImageDescription);
                    // TODO not available via GDI SetValueString(13, local.Tags);
                }
                catch { }
            }
        }
        #endregion
    }
}
