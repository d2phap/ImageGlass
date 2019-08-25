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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Services.Configuration;
using ImageGlass.Theme;

namespace ImageGlass
{
    /// <summary>
    /// A "Page Navigation" dialog, to allow the user a GUI for moving between
    /// pages of a multi-page file via the mouse.
    /// </summary>
    public partial class frmPageNav : ToolForm
    {

        public enum NavEvent
        {
            PageFirst,
            PagePrevious,
            PageNext,
            PageLast
        }


        public delegate void PageNavEvent(NavEvent navEvent);


        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point((int)(20 * DPIScaling.GetDPIScaleFactor()), 
                                                               (int)(300 * DPIScaling.GetDPIScaleFactor()));

        
        public frmPageNav()
        {
            InitializeComponent();

            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm
            MouseDown += ToolForm_MouseDown;
            Move += ToolForm_Move;
            FormClosing += frmPageNav_FormClosing;

            btnFirstPage.Click += ButtonClick;
            btnPrevPage.Click += ButtonClick;
            btnNextPage.Click += ButtonClick;
            btnLastPage.Click += ButtonClick;
        }


        /// <summary>
        /// User has clicked on one of the navigation buttons. Fire off the
        /// appropriate event to our listener.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClick(object sender, EventArgs e)
        {
            if (NavEventHandler == null)  // no handler established, do nothing
                return;

            if (sender == btnFirstPage)
                NavEventHandler(NavEvent.PageFirst);
            if (sender == btnPrevPage)
                NavEventHandler(NavEvent.PagePrevious);
            if (sender == btnNextPage)
                NavEventHandler(NavEvent.PageNext);
            if (sender == btnLastPage)
                NavEventHandler(NavEvent.PageLast);
        }


        /// <summary>
        /// The handler to send NavEvents to.
        /// </summary>
        public PageNavEvent NavEventHandler { get; set; }


        /// <summary>
        /// User clicked the close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            // TODO move to ToolForm?
            Close();
        }


        private void frmPageNav_KeyDown(object sender, KeyEventArgs e)
        {
// TODO TIF what is the shortcut key for this menu?
            //ESC or ???? --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)) 
                //|| //ESC 
                //(e.KeyCode == Keys.K && e.Control && e.Shift && !e.Alt))//CTRL + SHIFT + K
            {
                Close();
            }
        }


        private void frmPageNav_FormClosing(object sender, FormClosingEventArgs e)
        {
            LocalSetting.IsPageNavToolOpen = false;
            LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.PAGE_NAV_MENU;
            NavEventHandler = null;
        }


        /// <summary>
        /// Apply theme / language
        /// </summary>
        internal void UpdateUI()
        {
            //apply current theme ------------------------------------------------------
            var themeName = GlobalSetting.GetConfig("Theme", "default");
            Theme.Theme t = new Theme.Theme(GlobalSetting.ConfigDir(Dir.Themes, themeName));
            btnFirstPage.Image = t.ToolbarIcons.First.Image;
            btnPrevPage.Image = t.ToolbarIcons.ViewPreviousImage.Image;
            btnNextPage.Image = t.ToolbarIcons.ViewNextImage.Image;
            btnLastPage.Image = t.ToolbarIcons.Last.Image;

            BackColor =
                btnFirstPage.BackColor =
                btnPrevPage.BackColor =
                btnNextPage.BackColor =
                btnLastPage.BackColor =
                btnFirstPage.FlatAppearance.BorderColor =
                btnPrevPage.FlatAppearance.BorderColor =
                btnNextPage.FlatAppearance.BorderColor =
                btnLastPage.FlatAppearance.BorderColor =
                btnClose.FlatAppearance.BorderColor = 
                btnClose.BackColor =
                    LocalSetting.Theme.BackgroundColor;

            btnFirstPage.ForeColor =
            btnPrevPage.ForeColor =
            btnNextPage.ForeColor =
            btnLastPage.ForeColor = 
            btnClose.ForeColor =
                Theme.Theme.InvertBlackAndWhiteColor(LocalSetting.Theme.BackgroundColor);

            toolTip1.SetToolTip(btnFirstPage, GlobalSetting.LangPack.Items["frmPageNav.button1Tooltip"]);
            toolTip1.SetToolTip(btnPrevPage, GlobalSetting.LangPack.Items["frmPageNav.button2Tooltip"]);
            toolTip1.SetToolTip(btnNextPage, GlobalSetting.LangPack.Items["frmPageNav.button3Tooltip"]);
            toolTip1.SetToolTip(btnLastPage, GlobalSetting.LangPack.Items["frmPageNav.button4Tooltip"]);
        }


        private void frmPageNav_Load(object sender, EventArgs e)
        {
            // TODO move to ToolForm?
            UpdateUI();

            //Windows Bound (Position + Size)-------------------------------------------
            // TODO must be different from Color Picker
            Rectangle rc = GlobalSetting.StringToRect("0,0,300,160");

            if (rc.X == 0 && rc.Y == 0) // TODO isn't this always true?
            {
                _locationOffset = DefaultLocationOffset;
                parentOffset = _locationOffset;

                _SetLocationBasedOnParent();
            }
            else
            {
                Location = rc.Location;
            }
        }


        /// <summary>
        /// User has reached the first page of the document - disable prev/first buttons
        /// </summary>
        public bool AtFirstPage
        {
            set => btnFirstPage.Enabled = btnPrevPage.Enabled = !value;
        }


        /// <summary>
        /// User has reached the last page of the document - disable next/last buttons
        /// </summary>
        public bool AtLastPage
        {
            set => btnNextPage.Enabled = btnLastPage.Enabled = !value;
        }

    }
}
