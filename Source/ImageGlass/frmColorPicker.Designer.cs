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
            this.SuspendLayout();
            // 
            // lblPixel
            // 
            this.lblPixel.AutoSize = true;
            this.lblPixel.ForeColor = System.Drawing.Color.White;
            this.lblPixel.Location = new System.Drawing.Point(12, 41);
            this.lblPixel.Name = "lblPixel";
            this.lblPixel.Size = new System.Drawing.Size(31, 15);
            this.lblPixel.TabIndex = 0;
            this.lblPixel.Text = "pixel";
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(12, 13);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(110, 25);
            this.panelColor.TabIndex = 1;
            // 
            // lblRgb
            // 
            this.lblRgb.AutoSize = true;
            this.lblRgb.ForeColor = System.Drawing.Color.White;
            this.lblRgb.Location = new System.Drawing.Point(12, 56);
            this.lblRgb.Name = "lblRgb";
            this.lblRgb.Size = new System.Drawing.Size(25, 15);
            this.lblRgb.TabIndex = 2;
            this.lblRgb.Text = "rgb";
            // 
            // lblHex
            // 
            this.lblHex.AutoSize = true;
            this.lblHex.ForeColor = System.Drawing.Color.White;
            this.lblHex.Location = new System.Drawing.Point(12, 71);
            this.lblHex.Name = "lblHex";
            this.lblHex.Size = new System.Drawing.Size(25, 15);
            this.lblHex.TabIndex = 3;
            this.lblHex.Text = "hex";
            // 
            // frmColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(134, 111);
            this.Controls.Add(this.lblHex);
            this.Controls.Add(this.lblRgb);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.lblPixel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
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
            this.MouseEnter += new System.EventHandler(this.frmColorPicker_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.frmColorPicker_MouseLeave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPixel;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lblRgb;
        private System.Windows.Forms.Label lblHex;
    }
}