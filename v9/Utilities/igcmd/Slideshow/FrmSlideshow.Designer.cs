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
            this.components = new System.ComponentModel.Container();
            this.PicMain = new ImageGlass.Views.DXCanvas();
            this.MnuContext = new ImageGlass.UI.ModernMenu(this.components);
            this.MnuPauseResumeSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuExitSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuOpenMainWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuFullScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCheckerboardBackground = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuZoomModes = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLoadingOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuOpenWith = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuOpenLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // PicMain
            // 
            this.PicMain.BaseDpi = 96F;
            this.PicMain.CheckFPS = false;
            this.PicMain.ContextMenuStrip = this.MnuContext;
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
            // MnuContext
            // 
            this.MnuContext.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.MnuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuPauseResumeSlideshow,
            this.MnuExitSlideshow,
            this.MnuOpenMainWindow,
            this.toolStripMenuItem1,
            this.MnuFullScreen,
            this.MnuCheckerboardBackground,
            this.toolStripMenuItem3,
            this.MnuViewActualSize,
            this.MnuZoomModes,
            this.MnuLoadingOrders,
            this.toolStripMenuItem2,
            this.MnuOpenWith,
            this.MnuOpenLocation,
            this.MnuCopyPath});
            this.MnuContext.Name = "MnuContext";
            this.MnuContext.Size = new System.Drawing.Size(293, 361);
            // 
            // MnuPauseResumeSlideshow
            // 
            this.MnuPauseResumeSlideshow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuPauseResumeSlideshow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuPauseResumeSlideshow.Name = "MnuPauseResumeSlideshow";
            this.MnuPauseResumeSlideshow.Size = new System.Drawing.Size(292, 28);
            this.MnuPauseResumeSlideshow.Text = "[Pause / resume slideshow]";
            // 
            // MnuExitSlideshow
            // 
            this.MnuExitSlideshow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuExitSlideshow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuExitSlideshow.Name = "MnuExitSlideshow";
            this.MnuExitSlideshow.Size = new System.Drawing.Size(292, 28);
            this.MnuExitSlideshow.Text = "[Exit slideshow]";
            // 
            // MnuOpenMainWindow
            // 
            this.MnuOpenMainWindow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuOpenMainWindow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuOpenMainWindow.Name = "MnuOpenMainWindow";
            this.MnuOpenMainWindow.Size = new System.Drawing.Size(292, 28);
            this.MnuOpenMainWindow.Text = "[Open main window]";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(289, 6);
            // 
            // MnuFullScreen
            // 
            this.MnuFullScreen.CheckOnClick = true;
            this.MnuFullScreen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuFullScreen.Name = "MnuFullScreen";
            this.MnuFullScreen.Size = new System.Drawing.Size(292, 28);
            this.MnuFullScreen.Text = "[Full screen]";
            // 
            // MnuCheckerboardBackground
            // 
            this.MnuCheckerboardBackground.CheckOnClick = true;
            this.MnuCheckerboardBackground.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuCheckerboardBackground.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuCheckerboardBackground.Name = "MnuCheckerboardBackground";
            this.MnuCheckerboardBackground.Size = new System.Drawing.Size(292, 28);
            this.MnuCheckerboardBackground.Text = "[Checkerboard background]";
            // 
            // MnuViewActualSize
            // 
            this.MnuViewActualSize.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuViewActualSize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuViewActualSize.Name = "MnuViewActualSize";
            this.MnuViewActualSize.Size = new System.Drawing.Size(292, 28);
            this.MnuViewActualSize.Text = "[View actual size]";
            // 
            // MnuZoomModes
            // 
            this.MnuZoomModes.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuZoomModes.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuZoomModes.Name = "MnuZoomModes";
            this.MnuZoomModes.Size = new System.Drawing.Size(292, 28);
            this.MnuZoomModes.Text = "[Zoom modes]";
            // 
            // MnuLoadingOrders
            // 
            this.MnuLoadingOrders.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuLoadingOrders.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuLoadingOrders.Name = "MnuLoadingOrders";
            this.MnuLoadingOrders.Size = new System.Drawing.Size(292, 28);
            this.MnuLoadingOrders.Text = "[Loading orders]";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(289, 6);
            // 
            // MnuOpenWith
            // 
            this.MnuOpenWith.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuOpenWith.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuOpenWith.Name = "MnuOpenWith";
            this.MnuOpenWith.Size = new System.Drawing.Size(292, 28);
            this.MnuOpenWith.Text = "[Open with...]";
            // 
            // MnuOpenLocation
            // 
            this.MnuOpenLocation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuOpenLocation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuOpenLocation.Name = "MnuOpenLocation";
            this.MnuOpenLocation.Size = new System.Drawing.Size(292, 28);
            this.MnuOpenLocation.Text = "[Open image location]";
            // 
            // MnuCopyPath
            // 
            this.MnuCopyPath.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuCopyPath.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuCopyPath.Name = "MnuCopyPath";
            this.MnuCopyPath.Size = new System.Drawing.Size(292, 28);
            this.MnuCopyPath.Text = "[Copy image path]";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(289, 6);
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
            this.MnuContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageGlass.Views.DXCanvas PicMain;
        private ImageGlass.UI.ModernMenu MnuContext;
        private ToolStripMenuItem MnuPauseResumeSlideshow;
        private ToolStripMenuItem MnuExitSlideshow;
        private ToolStripMenuItem MnuOpenMainWindow;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem MnuZoomModes;
        private ToolStripMenuItem MnuLoadingOrders;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem MnuCheckerboardBackground;
        private ToolStripMenuItem MnuFullScreen;
        private ToolStripMenuItem MnuOpenLocation;
        private ToolStripMenuItem MnuCopyPath;
        private ToolStripMenuItem MnuOpenWith;
        private ToolStripMenuItem MnuViewActualSize;
        private ToolStripSeparator toolStripMenuItem3;
    }
}