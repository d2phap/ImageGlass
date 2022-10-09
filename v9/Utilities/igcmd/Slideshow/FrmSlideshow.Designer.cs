namespace igcmd.Slideshow
{
    partial class FrmSlideshow
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
            this.PicMain = new ImageGlass.Views.DXCanvas();
            this.SuspendLayout();
            // 
            // PicMain
            // 
            this.PicMain.BaseDpi = 96F;
            this.PicMain.CheckFPS = false;
            this.PicMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicMain.InterpolationScaleDown = ImageGlass.Base.PhotoBox.ImageInterpolation.SampleLinear;
            this.PicMain.Location = new System.Drawing.Point(0, 0);
            this.PicMain.Margin = new System.Windows.Forms.Padding(0);
            this.PicMain.Name = "PicMain";
            this.PicMain.NavBorderRadius = 45F;
            this.PicMain.NavButtonSize = new System.Drawing.SizeF(90F, 90F);
            this.PicMain.RequestUpdateFrame = false;
            this.PicMain.Size = new System.Drawing.Size(1112, 674);
            this.PicMain.TabIndex = 0;
            // 
            // FrmSlideshow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1112, 674);
            this.Controls.Add(this.PicMain);
            this.KeyPreview = true;
            this.Name = "FrmSlideshow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSlideshow_FormClosing);
            this.Load += new System.EventHandler(this.FrmSlideshow_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmSlideshow_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageGlass.Views.DXCanvas PicMain;
    }
}