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
using ImageGlass.Heart;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.UI.ToolForms;

namespace ImageGlass {
    public partial class frmColorPicker: ToolForm {
        // default location offset on the parent form
        private static Point DefaultLocationOffset = new Point(DPIScaling.Transform(20), DPIScaling.Transform(80));

        private ImageBoxEx _imgBox;
        private BitmapBooster _bmpBooster;
        private Point _cursorPos;

        public frmColorPicker() {
            InitializeComponent();
            RegisterToolFormEvents();

            btnSnapTo.Click += SnapButton_Click;
        }

        public void SetImageBox(ImageBoxEx imgBox) {
            if (_imgBox != null) {
                _imgBox.MouseMove -= _imgBox_MouseMove;
                _imgBox.Click -= _imgBox_Click;
            }

            _imgBox = imgBox;

            _imgBox.MouseMove += _imgBox_MouseMove;
            _imgBox.Click += _imgBox_Click;
        }

        #region Events to manage ImageBox

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            if (_imgBox != null) {
                _imgBox.MouseMove -= _imgBox_MouseMove;
                _imgBox.Click -= _imgBox_Click;
            }
        }

        private void _imgBox_MouseMove(object sender, MouseEventArgs e) {
            if (_imgBox.Image == null) {
                return;
            }
            _cursorPos = _imgBox.PointToImage(e.Location);

            //In case of opening a second image, 
            //there is a delay of loading image time which will cause error due to _imgBox is null.
            //Wrap try catch to skip this error
            try {
                if (_cursorPos.X >= 0 && _cursorPos.Y >= 0 && _cursorPos.X < _imgBox.Image.Width
                    && _cursorPos.Y < _imgBox.Image.Height) {
                    lblPixel.Text = string.Format("{0}, {1}", _cursorPos.X, _cursorPos.Y);
                }
            }
            catch { }
        }

        private void _imgBox_Click(object sender, EventArgs e) {
            if (_imgBox.Image == null) {
                return;
            }

            //In case of opening a second image, 
            //there is a delay of loading image time which will cause error due to _imgBox is null.
            //Wrap try catch to skip this error
            try {
                if (_cursorPos.X >= 0 && _cursorPos.Y >= 0 && _cursorPos.X < _imgBox.Image.Width && _cursorPos.Y < _imgBox.Image.Height) {
                    _bmpBooster?.Dispose();
                    using var bmp = new Bitmap(_imgBox.Image);
                    _bmpBooster = new BitmapBooster(bmp);

                    var color = _bmpBooster.Get(_cursorPos.X, _cursorPos.Y);
                    _DisplayColor(color);

                    _bmpBooster.Dispose();
                    _bmpBooster = null;
                }
            }
            catch { }
        }

        #endregion

        #region Display data

        private void _DisplayColor(Color color) {
            txtLocation.Text = lblPixel.Text;
            lblPixel.BackColor = Color.Transparent;
            panelColor.BackColor = color;

            // RGBA color -----------------------------------------------
            if (Configs.IsColorPickerRGBA) {
                lblRGB.Text = "RGBA:";
                txtRGB.Text = $"{color.R}, {color.G}, {color.B}, {Math.Round(color.A / 255.0, 3)}";
            }
            else {
                lblRGB.Text = "RGB:";
                txtRGB.Text = $"{color.R}, {color.G}, {color.B}";
            }

            // HEXA color -----------------------------------------------
            if (Configs.IsColorPickerHEXA) {
                lblHEX.Text = "HEXA:";
                txtHEX.Text = Theme.ConvertColorToHEX(color);
            }
            else {
                lblHEX.Text = "HEX:";
                txtHEX.Text = Theme.ConvertColorToHEX(color, true);
            }

            // CMYK color -----------------------------------------------
            var cmyk = Theme.ConvertColorToCMYK(color);
            txtCMYK.Text = $"{cmyk[0]}%, {cmyk[1]}%, {cmyk[2]}%, {cmyk[3]}%";

            // HSLA color -----------------------------------------------
            var hsla = Theme.ConvertColorToHSLA(color);
            if (Configs.IsColorPickerHSLA) {
                lblHSL.Text = "HSLA:";
                txtHSL.Text = $"{hsla[0]}, {hsla[1]}%, {hsla[2]}%, {hsla[3]}";
            }
            else {
                lblHSL.Text = "HSL:";
                txtHSL.Text = $"{hsla[0]}, {hsla[1]}%, {hsla[2]}%";
            }

            // HSVA color -----------------------------------------------
            var hsva = Theme.ConvertColorToHSVA(color);
            if (Configs.IsColorPickerHSVA) {
                lblHSV.Text = "HSVA:";
                txtHSV.Text = $"{hsva[0]}, {hsva[1]}%, {hsva[2]}%, {hsva[3]}";
            }
            else {
                lblHSV.Text = "HSV:";
                txtHSV.Text = $"{hsva[0]}, {hsva[1]}%, {hsva[2]}%";
            }


            lblPixel.ForeColor = Theme.InvertBlackAndWhiteColor(color);
        }

        private void _ResetColor() {
            lblPixel.Text = string.Empty;
            txtLocation.Text = string.Empty;
            txtRGB.Text = string.Empty;
            txtHEX.Text = string.Empty;
        }

        private void ColorTextbox_Click(object sender, EventArgs e) {
            var txt = (TextBox)sender;
            txt.SelectAll();

            // fixed: cannot copy the text if Owner form is not activated
            this.Owner.Activate();
            this.Activate();
        }

        private void BtnClose_Click(object sender, EventArgs e) {
            Configs.IsShowColorPickerOnStartup = false;
            this.Close();
        }

        #endregion

        #region Other Form Events
        private void frmColorPicker_KeyDown(object sender, KeyEventArgs e) {
            // lblPixel.Text = e.KeyCode.ToString();

            #region ESC or CTRL + SHIFT + K
            // ESC or CTRL + SHIFT + K --------------------------------------------------------
            if ((e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt) || //ESC 
                (e.KeyCode == Keys.K && e.Control && e.Shift && !e.Alt))//CTRL + SHIFT + K
            {
                Configs.IsShowColorPickerOnStartup = false;
                this.Close();
            }
            #endregion
        }

        private void frmColorPicker_FormClosing(object sender, FormClosingEventArgs e) {
            Local.IsColorPickerToolOpening = false;

            Local.ForceUpdateActions |= ForceUpdateActions.COLOR_PICKER_MENU;
        }

        /// <summary>
        /// Apply theme
        /// </summary>
        public void UpdateUI() {
            SetColors(Configs.Theme);

            btnSnapTo.FlatAppearance.MouseOverBackColor = Theme.LightenColor(Configs.Theme.BackgroundColor, 0.1f);
            btnSnapTo.FlatAppearance.MouseDownBackColor = Theme.DarkenColor(Configs.Theme.BackgroundColor, 0.1f);

            lblFormTitle.Text = Configs.Language.Items[$"{nameof(frmMain)}.mnuMainColorPicker"];
        }

        private void frmColorPicker_Load(object sender, EventArgs e) {
            UpdateUI();

            // Windows Bound (Position + Size)-------------------------------------------
            var rc = Base.Helpers.StringToRect("0;0;300;160");

            if (rc.X == 0 && rc.Y == 0) {
                _locationOffset = DefaultLocationOffset;
                parentOffset = _locationOffset;

                _SetLocationBasedOnParent();
            }
            else {
                this.Location = rc.Location;
            }

            _ResetColor();

            lblRGB.Text = "RGB:";
            lblHEX.Text = "HEX:";
            lblHSL.Text = "HSL:";

            if (Configs.IsColorPickerRGBA) {
                lblRGB.Text = "RGBA:";
            }
            if (Configs.IsColorPickerHEXA) {
                lblHEX.Text = "HEXA:";
            }
            if (Configs.IsColorPickerHSLA) {
                lblHSL.Text = "HSLA:";
            }
        }

        #endregion
    }
}
