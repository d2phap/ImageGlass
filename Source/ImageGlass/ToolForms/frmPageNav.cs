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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.ToolForms;

namespace ImageGlass {
    /// <summary>
    /// A "Page Navigation" dialog, to allow the user a GUI for moving between
    /// pages of a multi-page file via the mouse.
    /// </summary>
    public partial class frmPageNav: ToolForm {
        public enum NavEvent {
            PageFirst,
            PagePrevious,
            PageNext,
            PageLast
        }

        /// <summary>
        /// The handler to send NavEvents to.
        /// </summary>
        public PageNavEvent NavEventHandler { get; set; }


        public delegate void PageNavEvent(NavEvent navEvent);


        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point(DPIScaling.Transform(20), DPIScaling.Transform(320));



        public frmPageNav() {
            InitializeComponent();

            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm

            RegisterToolFormEvents();
            FormClosing += frmPageNav_FormClosing;

            btnFirstPage.Click += ButtonClick;
            btnPreviousPage.Click += ButtonClick;
            btnNextPage.Click += ButtonClick;
            btnLastPage.Click += ButtonClick;

            btnSnapTo.Click += SnapButton_Click;
        }



        #region Private Methods
        /// <summary>
        /// User has clicked on one of the navigation buttons. Fire off the appropriate event to our listener.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick(object sender, EventArgs e) {
            if (NavEventHandler == null)  // no handler established, do nothing
                return;

            if (sender == btnFirstPage)
                NavEventHandler(NavEvent.PageFirst);
            if (sender == btnPreviousPage)
                NavEventHandler(NavEvent.PagePrevious);
            if (sender == btnNextPage)
                NavEventHandler(NavEvent.PageNext);
            if (sender == btnLastPage)
                NavEventHandler(NavEvent.PageLast);
        }



        /// <summary>
        /// Apply theme / language
        /// </summary>
        internal void UpdateUI() {
            // Apply current theme ------------------------------------------------------
            OnDpiChanged();
            SetColors(Configs.Theme);

            // Remove white line under tool strip
            toolPageNav.Renderer = new UI.Renderers.ToolStripRenderer(Configs.Theme.ToolbarBackgroundColor, Configs.Theme.TextInfoColor);

            toolPageNav.BackgroundImage = Configs.Theme.ToolbarBackgroundImage.Image;
            toolPageNav.BackColor = Configs.Theme.ToolbarBackgroundColor;
            toolPageNav.Alignment = ToolbarAlignment.CENTER;
            toolPageNav.HideTooltips = Configs.IsHideTooltips;

            // Overflow button and Overflow dropdown
            toolPageNav.OverflowButton.DropDown.BackColor = Configs.Theme.ToolbarBackgroundColor;
            toolPageNav.OverflowButton.AutoSize = false;
            toolPageNav.OverflowButton.Padding = new Padding(DPIScaling.Transform(10));


            lblFormTitle.Text = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainPageNav"];
            btnNextPage.ToolTipText = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainNextPage"];
            btnPreviousPage.ToolTipText = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainPrevPage"];
            btnFirstPage.ToolTipText = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainFirstPage"];
            btnLastPage.ToolTipText = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainLastPage"];

            btnSnapTo.FlatAppearance.MouseOverBackColor = Theme.LightenColor(Configs.Theme.BackgroundColor, 0.1f);
            btnSnapTo.FlatAppearance.MouseDownBackColor = Theme.DarkenColor(Configs.Theme.BackgroundColor, 0.1f);
        }


        private void OnDpiChanged() {
            // Update size of toolbar
            DPIScaling.TransformToolbar(ref toolPageNav, (int)Configs.ToolbarIconHeight);

            // Update toolbar icon according to the new size
            LoadToolbarIcons(Configs.Theme);

            // Update window size
            this.Width = toolPageNav.PreferredSize.Width;
            this.Height = toolPageNav.PreferredSize.Height + lblPageInfo.Height + btnClose.Height + 30;
        }


        private void LoadToolbarIcons(Theme th) {
            btnFirstPage.Image = th.ToolbarIcons.ViewFirstImage.Image;
            btnPreviousPage.Image = th.ToolbarIcons.ViewPreviousImage.Image;
            btnNextPage.Image = th.ToolbarIcons.ViewNextImage.Image;
            btnLastPage.Image = th.ToolbarIcons.ViewLastImage.Image;
        }
        #endregion


        #region Events
        private void frmPageNav_Load(object sender, EventArgs e) {
            UpdateUI();

            //Windows Bound (Position + Size)-------------------------------------------
            // TODO must be different from Color Picker
            var rc = Helpers.StringToRect("0;0;300;160");

            if (rc.X == 0 && rc.Y == 0) {
                _locationOffset = DefaultLocationOffset;
                parentOffset = _locationOffset;

                _SetLocationBasedOnParent();
            }
            else {
                Location = rc.Location;
            }
        }

        private void frmPageNav_Activated(object sender, EventArgs e) {
            UpdateUI();
        }

        private void frmPageNav_KeyDown(object sender, KeyEventArgs e) {
            // ESC or Ctrl+Shift+J --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) ||
                (e.KeyCode == Keys.J && e.Control && e.Shift && !e.Alt)) // CTRL + SHIFT + J
            {
                Configs.IsShowPageNavOnStartup = false;
                this.Close();
            }
        }

        private void frmPageNav_FormClosing(object sender, FormClosingEventArgs e) {
            Local.IsPageNavToolOpenning = false;

            Local.ForceUpdateActions |= ForceUpdateActions.PAGE_NAV_MENU;
            NavEventHandler = null;
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            Configs.IsShowPageNavOnStartup = false;
            this.Close();
        }
        #endregion


    }
}
