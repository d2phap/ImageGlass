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
    public partial class frmPageNav : ToolForm
    {
        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point((int)(20 * DPIScaling.GetDPIScaleFactor()), 
                                                               (int)(300 * DPIScaling.GetDPIScaleFactor()));
        public frmPageNav()
        {
            InitializeComponent();

            // TODO compartmentalize / move to UpdateUI
            var themeName = GlobalSetting.GetConfig("Theme", "default");
            Theme.Theme t = new Theme.Theme(GlobalSetting.ConfigDir(Dir.Themes, themeName));
            button2.Image = t.ToolbarIcons.ViewPreviousImage.Image;
            button3.Image = t.ToolbarIcons.ViewNextImage.Image;


            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm
            MouseDown += ToolForm_MouseDown;
            Move += ToolForm_Move;
            FormClosing += frmPageNav_FormClosing;
        }

        // TODO need an entity to send "NextPage" events to [first,prev,next,last]

        private void BtnClose_Click(object sender, EventArgs e)
        {
            // TODO move to ToolForm?
            Close();
        }


        #region Other Form Events
        private void frmPageNav_KeyDown(object sender, KeyEventArgs e)
        {
// TODO what is the shortcut key for this menu?
            #region ESC
            //ESC or CTRL + SHIFT + K --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)) 
                //|| //ESC 
                //(e.KeyCode == Keys.K && e.Control && e.Shift && !e.Alt))//CTRL + SHIFT + K
            {
                LocalSetting.IsShowColorPickerOnStartup = false;
                Close();
            }
            #endregion

        }


        private void frmPageNav_FormClosing(object sender, FormClosingEventArgs e)
        {
            LocalSetting.IsPageNavToolOpen = false;
            LocalSetting.ForceUpdateActions |= MainFormForceUpdateAction.PAGE_NAV_MENU;

            // TODO disconnect page events
        }


        /// <summary>
        /// Apply theme
        /// </summary>
        internal void UpdateUI()
        {
            //apply current theme ------------------------------------------------------
            BackColor =
                button1.BackColor =
                button2.BackColor =
                button3.BackColor =
                button4.BackColor =
                button1.FlatAppearance.BorderColor =
                button2.FlatAppearance.BorderColor =
                button3.FlatAppearance.BorderColor =
                button4.FlatAppearance.BorderColor =
                btnClose.FlatAppearance.BorderColor = 
                btnClose.BackColor =
                    LocalSetting.Theme.BackgroundColor;

            button1.ForeColor =
            button2.ForeColor =
            button3.ForeColor =
            button4.ForeColor =
                Theme.Theme.InvertBlackAndWhiteColor(LocalSetting.Theme.BackgroundColor);
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
        #endregion


    }
}
