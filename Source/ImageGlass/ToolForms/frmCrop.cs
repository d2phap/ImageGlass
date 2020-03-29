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
using System.ComponentModel;
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
        private static readonly Point DefaultLocationOffset = new Point(DPIScaling.Transform(20), DPIScaling.Transform(300));
        private ImageBoxEx _imgBox;

        public frmCrop()
        {
            InitializeComponent();

            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm

            RegisterToolFormEvents();
            FormClosing += frmCrop_FormClosing;
            btnSnapTo.Click += SnapButton_Click;
        }


        public void SetImageBox(ImageBoxEx imgBox)
        {
            if (_imgBox != null)
            {
                _imgBox.SelectionRegionChanged -= this._imgBox_SelectionRegionChanged;
            }

            _imgBox = imgBox;
            _imgBox.SelectionRegionChanged += this._imgBox_SelectionRegionChanged;
        }

        private void _imgBox_SelectionRegionChanged(object sender, EventArgs e)
        {
            try
            {
                numX.Value = (decimal)_imgBox.SelectionRegion.X;
                numY.Value = (decimal)_imgBox.SelectionRegion.Y;
                numWidth.Value = (decimal)_imgBox.SelectionRegion.Width;
                numHeight.Value = (decimal)_imgBox.SelectionRegion.Height;
            }
            catch { }
        }



        #region Private Methods

        /// <summary>
        /// Apply theme / language
        /// </summary>
        internal void UpdateUI()
        {
            // Apply current theme ------------------------------------------------------
            SetColors(Configs.Theme);

            tableActions.BackColor = Configs.Theme.ToolbarBackgroundColor;

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


        private void Numeric_Click(object sender, EventArgs e)
        {
            var num = (NumericUpDown)sender;
            num.Select(0, num.Text.Length);

            // fixed: cannot copy the text if Owner form is not activated
            this.Owner.Activate();
            this.Activate();
        }

        private void Numeric_ValueChanged(object sender, EventArgs e)
        {
            // manually set the selection region
            if (!_imgBox.IsSelecting 
                && !_imgBox.IsResizingSelection 
                && !_imgBox.IsMovingSelection)
            {
                _imgBox.SelectionRegion = new RectangleF(
                    (float)numX.Value,
                    (float)numY.Value,
                    (float)numWidth.Value,
                    (float)numHeight.Value);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            numX.Value = 0;
            numY.Value = 0;
            numWidth.Value = 0;
            numHeight.Value = 0;
        }

        #endregion

        
    }
}
