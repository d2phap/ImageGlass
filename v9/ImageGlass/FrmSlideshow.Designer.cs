namespace ImageGlass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSlideshow));
            this.PicSlideshow = new ImageGlass.Views.DXCanvas();
            this.SuspendLayout();
            // 
            // PicSlideshow
            // 
            this.PicSlideshow.BaseDpi = 96F;
            this.PicSlideshow.CheckFPS = false;
            this.PicSlideshow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicSlideshow.InterpolationScaleDown = ImageGlass.Base.PhotoBox.ImageInterpolation.SampleLinear;
            this.PicSlideshow.Location = new System.Drawing.Point(0, 0);
            this.PicSlideshow.Margin = new System.Windows.Forms.Padding(0);
            this.PicSlideshow.Name = "PicSlideshow";
            this.PicSlideshow.NavBorderRadius = 45F;
            this.PicSlideshow.NavButtonSize = new System.Drawing.SizeF(90F, 90F);
            this.PicSlideshow.RequestUpdateFrame = false;
            this.PicSlideshow.Size = new System.Drawing.Size(956, 586);
            this.PicSlideshow.TabIndex = 0;
            // 
            // FrmSlideshow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 586);
            this.Controls.Add(this.PicSlideshow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmSlideshow";
            this.Text = "ImageGlass - Slideshow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSlideshow_FormClosing);
            this.Load += new System.EventHandler(this.FrmSlideshow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Views.DXCanvas PicSlideshow;
    }
}