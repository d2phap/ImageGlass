namespace WindowsFormsApplication1
{
    partial class Form1
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
            this.ctlMain1 = new ThemeConfig.ctlMain();
            this.SuspendLayout();
            // 
            // ctlMain1
            // 
            this.ctlMain1.BackColor = System.Drawing.Color.White;
            this.ctlMain1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlMain1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlMain1.Location = new System.Drawing.Point(0, 0);
            this.ctlMain1.MinimumSize = new System.Drawing.Size(723, 396);
            this.ctlMain1.Name = "ctlMain1";
            this.ctlMain1.Size = new System.Drawing.Size(810, 495);
            this.ctlMain1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 495);
            this.Controls.Add(this.ctlMain1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ThemeConfig.ctlMain ctlMain1;
    }
}

