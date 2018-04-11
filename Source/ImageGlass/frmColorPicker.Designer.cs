namespace ImageGlass
{
    partial class frmColorPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPixel = new System.Windows.Forms.Label();
            this.panelColor = new System.Windows.Forms.Panel();
            this.lblRgb = new System.Windows.Forms.Label();
            this.lblHex = new System.Windows.Forms.Label();
            this.txtRGB = new System.Windows.Forms.TextBox();
            this.txtHEX = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblPixel
            // 
            this.lblPixel.AutoSize = true;
            this.lblPixel.ForeColor = System.Drawing.Color.White;
            this.lblPixel.Location = new System.Drawing.Point(15, 70);
            this.lblPixel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPixel.Name = "lblPixel";
            this.lblPixel.Size = new System.Drawing.Size(47, 25);
            this.lblPixel.TabIndex = 0;
            this.lblPixel.Text = "Pixel";
            this.lblPixel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // panelColor
            // 
            this.panelColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(20, 20);
            this.panelColor.Margin = new System.Windows.Forms.Padding(4);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(224, 36);
            this.panelColor.TabIndex = 1;
            this.panelColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // lblRgb
            // 
            this.lblRgb.AutoSize = true;
            this.lblRgb.ForeColor = System.Drawing.Color.White;
            this.lblRgb.Location = new System.Drawing.Point(15, 108);
            this.lblRgb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRgb.Name = "lblRgb";
            this.lblRgb.Size = new System.Drawing.Size(49, 25);
            this.lblRgb.TabIndex = 2;
            this.lblRgb.Text = "RGB:";
            this.lblRgb.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // lblHex
            // 
            this.lblHex.AutoSize = true;
            this.lblHex.ForeColor = System.Drawing.Color.White;
            this.lblHex.Location = new System.Drawing.Point(15, 145);
            this.lblHex.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHex.Name = "lblHex";
            this.lblHex.Size = new System.Drawing.Size(49, 25);
            this.lblHex.TabIndex = 3;
            this.lblHex.Text = "HEX:";
            this.lblHex.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // txtRGB
            // 
            this.txtRGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRGB.Location = new System.Drawing.Point(71, 105);
            this.txtRGB.Name = "txtRGB";
            this.txtRGB.ReadOnly = true;
            this.txtRGB.Size = new System.Drawing.Size(173, 31);
            this.txtRGB.TabIndex = 4;
            this.txtRGB.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtHEX
            // 
            this.txtHEX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHEX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHEX.Location = new System.Drawing.Point(71, 142);
            this.txtHEX.Name = "txtHEX";
            this.txtHEX.ReadOnly = true;
            this.txtHEX.Size = new System.Drawing.Size(173, 31);
            this.txtHEX.TabIndex = 5;
            this.txtHEX.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // frmColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(264, 200);
            this.Controls.Add(this.txtHEX);
            this.Controls.Add(this.txtRGB);
            this.Controls.Add(this.lblHex);
            this.Controls.Add(this.lblRgb);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.lblPixel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmColorPicker";
            this.RightToLeftLayout = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Color Picker";
            this.TopMost = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            this.MouseEnter += new System.EventHandler(this.frmColorPicker_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.frmColorPicker_MouseLeave);
            this.Move += new System.EventHandler(this.frmColorPicker_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPixel;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lblRgb;
        private System.Windows.Forms.Label lblHex;
        private System.Windows.Forms.TextBox txtRGB;
        private System.Windows.Forms.TextBox txtHEX;
    }
}