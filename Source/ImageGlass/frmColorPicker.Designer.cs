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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblPixel
            // 
            this.lblPixel.AutoSize = true;
            this.lblPixel.ForeColor = System.Drawing.Color.White;
            this.lblPixel.Location = new System.Drawing.Point(15, 26);
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
            this.panelColor.Size = new System.Drawing.Size(260, 36);
            this.panelColor.TabIndex = 1;
            this.panelColor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // lblRgb
            // 
            this.lblRgb.AutoSize = true;
            this.lblRgb.ForeColor = System.Drawing.Color.White;
            this.lblRgb.Location = new System.Drawing.Point(15, 75);
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
            this.lblHex.Location = new System.Drawing.Point(15, 112);
            this.lblHex.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHex.Name = "lblHex";
            this.lblHex.Size = new System.Drawing.Size(49, 25);
            this.lblHex.TabIndex = 3;
            this.lblHex.Text = "HEX:";
            this.lblHex.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // txtRGB
            // 
            this.txtRGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRGB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtRGB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRGB.Location = new System.Drawing.Point(71, 72);
            this.txtRGB.Name = "txtRGB";
            this.txtRGB.ReadOnly = true;
            this.txtRGB.Size = new System.Drawing.Size(209, 31);
            this.txtRGB.TabIndex = 4;
            this.txtRGB.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // txtHEX
            // 
            this.txtHEX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHEX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.txtHEX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHEX.Location = new System.Drawing.Point(71, 109);
            this.txtHEX.Name = "txtHEX";
            this.txtHEX.ReadOnly = true;
            this.txtHEX.Size = new System.Drawing.Size(209, 31);
            this.txtHEX.TabIndex = 5;
            this.txtHEX.Click += new System.EventHandler(this.ColorTextbox_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.lblPixel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 165);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(300, 75);
            this.panel1.TabIndex = 6;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmColorPicker_MouseDown);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(172, 18);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(108, 40);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(300, 240);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtHEX);
            this.Controls.Add(this.txtRGB);
            this.Controls.Add(this.lblHex);
            this.Controls.Add(this.lblRgb);
            this.Controls.Add(this.panelColor);
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
            this.Move += new System.EventHandler(this.frmColorPicker_Move);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
    }
}