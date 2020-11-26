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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.ToolForms;

namespace ImageGlass {
    public partial class frmCrop: ToolForm {
        // default location offset on the parent form
        private static readonly Point DefaultLocationOffset = new Point(DPIScaling.Transform(20), DPIScaling.Transform(440));
        private ImageBoxEx _imgBox;

        /// <summary>
        /// The handler to send CropEvent to.
        /// </summary>
        public CropToolEvent CropEventHandler { get; set; }
        public delegate void CropToolEvent(CropActionEvent cropEvent);

        public enum CropActionEvent {
            Save,
            SaveAs,
            Copy
        }

        public frmCrop() {
            InitializeComponent();

            _locationOffset = DefaultLocationOffset; // TODO simplify and move logic to ToolForm

            RegisterToolFormEvents();
            FormClosing += frmCrop_FormClosing;
            btnSnapTo.Click += SnapButton_Click;
        }

        public void SetImageBox(ImageBoxEx imgBox) {
            if (_imgBox != null) {
                _imgBox.SelectionRegionChanged -= this._imgBox_SelectionRegionChanged;
                _imgBox.ImageChanged -= this._imgBox_ImageChanged;
            }

            _imgBox = imgBox;
            _imgBox.SelectionRegionChanged += this._imgBox_SelectionRegionChanged;
            _imgBox.ImageChanged += this._imgBox_ImageChanged;
        }

        private void _imgBox_ImageChanged(object sender, EventArgs e) {
            btnReset_Click(null, null);
        }

        private void _imgBox_SelectionRegionChanged(object sender, EventArgs e) {
            try {
                numX.Value = (decimal)_imgBox.SelectionRegion.X;
                numY.Value = (decimal)_imgBox.SelectionRegion.Y;
                numWidth.Value = (decimal)_imgBox.SelectionRegion.Width;
                numHeight.Value = (decimal)_imgBox.SelectionRegion.Height;
            }
            catch { }
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            if (_imgBox != null) {
                _imgBox.SelectionRegionChanged -= this._imgBox_SelectionRegionChanged;
                _imgBox.ImageChanged -= this._imgBox_ImageChanged;
            }
        }

        #region Private Methods

        /// <summary>
        /// Apply theme / language
        /// </summary>
        internal void UpdateUI() {
            // Apply current theme ------------------------------------------------------
            SetColors(Configs.Theme);
            tableActions.BackColor = Configs.Theme.ToolbarBackgroundColor;

            btnSnapTo.FlatAppearance.MouseOverBackColor = Theme.LightenColor(Configs.Theme.BackgroundColor, 0.1f);
            btnSnapTo.FlatAppearance.MouseDownBackColor = Theme.DarkenColor(Configs.Theme.BackgroundColor, 0.1f);

            // Upate language
            lblFormTitle.Text = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainCrop"];
            lblWidth.Text = Configs.Language.Items[$"{Name}.{nameof(lblWidth)}"];
            lblHeight.Text = Configs.Language.Items[$"{Name}.{nameof(lblHeight)}"];
            btnSave.Text = Configs.Language.Items[$"{Name}.{nameof(btnSave)}"];
            btnSaveAs.Text = Configs.Language.Items[$"{Name}.{nameof(btnSaveAs)}"];
            btnCopy.Text = Configs.Language.Items[$"{Name}.{nameof(btnCopy)}"];
            btnReset.Text = Configs.Language.Items[$"{Name}.{nameof(btnReset)}"];
        }
        #endregion

        #region Events
        private void frmCrop_Load(object sender, EventArgs e) {
            UpdateUI();

            // Windows Bound (Position + Size)-------------------------------------------
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

        private void frmCrop_KeyDown(object sender, KeyEventArgs e) {
            // ESC or C --------------------------------------------------------
            if (!e.Control && !e.Shift && !e.Alt
                && (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.C))) // C
            {
                Configs.IsShowPageNavOnStartup = false;
                this.Close();
            }
        }

        private void frmCrop_FormClosing(object sender, FormClosingEventArgs e) {
            btnReset_Click(null, null);

            CropEventHandler = null;
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            var frm = (frmMain)this._currentOwner;
            frm.ShowCropTool(false);
        }

        private void Numeric_Click(object sender, EventArgs e) {
            var num = (NumericUpDown)sender;
            num.Select(0, num.Text.Length);

            // fixed: cannot copy the text if Owner form is not activated
            this.Owner.Activate();
            this.Activate();
        }

        private void Numeric_ValueChanged(object sender, EventArgs e) {
            // manually set the selection region
            if (!_imgBox.IsSelecting
                && !_imgBox.IsResizingSelection
                && !_imgBox.IsMovingSelection) {
                _imgBox.SelectionRegion = new RectangleF(
                    (float)numX.Value,
                    (float)numY.Value,
                    (float)numWidth.Value,
                    (float)numHeight.Value);
            }
        }

        private void CropActionButton_Click(object sender, EventArgs e) {
            if (CropEventHandler == null)  // no handler established, do nothing
                return;

            if (sender == btnSave) {
                if (!Local.IsTempMemoryData) {
                    // Save the image path for saving
                    Local.ImageModifiedPath = Local.ImageList.GetFileName(Local.CurrentIndex);
                    CropEventHandler(CropActionEvent.Save);
                }
                else {
                    // for non-existing file, trigger SaveAs
                    CropEventHandler(CropActionEvent.SaveAs);
                }
            }
            else if (sender == btnSaveAs) {
                CropEventHandler(CropActionEvent.SaveAs);
            }
            else if (sender == btnCopy) {
                CropEventHandler(CropActionEvent.Copy);
            }
        }

        private void btnReset_Click(object sender, EventArgs e) {
            numX.Value = 0;
            numY.Value = 0;
            numWidth.Value = 0;
            numHeight.Value = 0;
        }

        #endregion

    }
}
