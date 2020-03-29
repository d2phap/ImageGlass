/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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

namespace ImageGlass
{
    public partial class frmCrop : ToolForm
    {
        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point((int)(20 * DPIScaling.GetDPIScaleFactor()), (int)(300 * DPIScaling.GetDPIScaleFactor()));

        public frmCrop()
        {
            InitializeComponent();

            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm

            RegisterToolFormEvents();
            FormClosing += frmCrop_FormClosing;
            btnSnapTo.Click += SnapButton_Click;
        }




        #region Private Methods

        /// <summary>
        /// Apply theme / language
        /// </summary>
        internal void UpdateUI()
        {
            // Apply current theme ------------------------------------------------------
            SetColors(Configs.Theme);

            btnSnapTo.FlatAppearance.MouseOverBackColor = Theme.LightenColor(Configs.Theme.BackgroundColor, 0.1f);
            btnSnapTo.FlatAppearance.MouseDownBackColor = Theme.DarkenColor(Configs.Theme.BackgroundColor, 0.1f);
        }
        #endregion


        #region Events
        private void frmCrop_Load(object sender, EventArgs e)
        {
            UpdateUI();

            // Windows Bound (Position + Size)-------------------------------------------
            Rectangle rc = Helpers.StringToRect("0;0;300;160");

            if (rc.X == 0 && rc.Y == 0)
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

        private void frmCrop_KeyDown(object sender, KeyEventArgs e)
        {
            // ESC or C --------------------------------------------------------
            if (!e.Control && !e.Shift && !e.Alt
                && (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.C))) // C
            {
                Configs.IsShowPageNavOnStartup = false;
                this.Close();
            }
        }

        private void frmCrop_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Local.IsPageNavToolOpenning = false;

            //Local.ForceUpdateActions |= ForceUpdateActions.PAGE_NAV_MENU;
            //NavEventHandler = null;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            //Configs.IsShowPageNavOnStartup = false;
            this.Close();
        }
        #endregion
    }
}
