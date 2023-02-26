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
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuFullScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuToggleCountdown = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuToggleCheckerboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuChangeBackgroundColor = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuNavigation = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewNext = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuGoToFirst = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuGoToLast = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuZoomModes = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuAutoZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLockZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToWidth = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToHeight = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToFit = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToFill = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLoadingOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuOpenWith = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuOpenLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuExitSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // PicMain
            // 
            this.PicMain.BaseDpi = 96F;
            this.PicMain.CheckFPS = false;
            this.PicMain.ContextMenuStrip = this.MnuContext;
            this.PicMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicMain.EnableSelection = false;
            this.PicMain.InterpolationScaleDown = ImageGlass.Base.PhotoBox.ImageInterpolation.SampleLinear;
            this.PicMain.Location = new System.Drawing.Point(0, 0);
            this.PicMain.Margin = new System.Windows.Forms.Padding(0);
            this.PicMain.Name = "PicMain";
            this.PicMain.RequestUpdateFrame = false;
            this.PicMain.SelectionAspectRatio = new System.Drawing.SizeF(0F, 0F);
            this.PicMain.AccentColor = System.Drawing.Color.Black;
            this.PicMain.Size = new System.Drawing.Size(1112, 674);
            this.PicMain.TabIndex = 0;
            this.PicMain.OnZoomChanged += this.PicMain_OnZoomChanged;
            this.PicMain.OnNavLeftClicked += this.PicMain_OnNavLeftClicked;
            this.PicMain.OnNavRightClicked += this.PicMain_OnNavRightClicked;
            this.PicMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PicMain_MouseClick);
            this.PicMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PicMain_MouseMove);
            // 
            // MnuContext
            // 
            this.MnuContext.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.MnuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuPauseResumeSlideshow,
            this.toolStripMenuItem1,
            this.MnuFullScreen,
            this.MnuToggleCountdown,
            this.MnuToggleCheckerboard,
            this.toolStripMenuItem3,
            this.MnuChangeBackgroundColor,
            this.MnuNavigation,
            this.MnuActualSize,
            this.MnuZoomModes,
            this.MnuLoadingOrders,
            this.toolStripMenuItem2,
            this.MnuOpenWith,
            this.MnuOpenLocation,
            this.MnuCopyPath,
            this.toolStripMenuItem5,
            this.MnuExitSlideshow});
            this.MnuContext.Name = "MnuContext";
            this.MnuContext.Size = new System.Drawing.Size(301, 392);
            this.MnuContext.Opening += new System.ComponentModel.CancelEventHandler(this.MnuContext_Opening);
            // 
            // MnuPauseResumeSlideshow
            // 
            this.MnuPauseResumeSlideshow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuPauseResumeSlideshow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuPauseResumeSlideshow.Name = "MnuPauseResumeSlideshow";
            this.MnuPauseResumeSlideshow.Size = new System.Drawing.Size(300, 28);
            this.MnuPauseResumeSlideshow.Text = "[Pause / resume slideshow]";
            this.MnuPauseResumeSlideshow.Click += new System.EventHandler(this.MnuPauseResumeSlideshow_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(297, 6);
            // 
            // MnuFullScreen
            // 
            this.MnuFullScreen.CheckOnClick = true;
            this.MnuFullScreen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuFullScreen.Name = "MnuFullScreen";
            this.MnuFullScreen.Size = new System.Drawing.Size(300, 28);
            this.MnuFullScreen.Text = "[Full screen]";
            this.MnuFullScreen.Click += new System.EventHandler(this.MnuFullScreen_Click);
            // 
            // MnuToggleCountdown
            // 
            this.MnuToggleCountdown.CheckOnClick = true;
            this.MnuToggleCountdown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuToggleCountdown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuToggleCountdown.Name = "MnuToggleCountdown";
            this.MnuToggleCountdown.Size = new System.Drawing.Size(300, 28);
            this.MnuToggleCountdown.Text = "[Show countdown]";
            this.MnuToggleCountdown.Click += new System.EventHandler(this.MnuToggleCountdown_Click);
            // 
            // MnuToggleCheckerboard
            // 
            this.MnuToggleCheckerboard.CheckOnClick = true;
            this.MnuToggleCheckerboard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuToggleCheckerboard.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuToggleCheckerboard.Name = "MnuToggleCheckerboard";
            this.MnuToggleCheckerboard.Size = new System.Drawing.Size(300, 28);
            this.MnuToggleCheckerboard.Text = "[Checkerboard background]";
            this.MnuToggleCheckerboard.Click += new System.EventHandler(this.MnuToggleCheckerboard_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(297, 6);
            // 
            // MnuChangeBackgroundColor
            // 
            this.MnuChangeBackgroundColor.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuChangeBackgroundColor.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuChangeBackgroundColor.Name = "MnuChangeBackgroundColor";
            this.MnuChangeBackgroundColor.Size = new System.Drawing.Size(300, 28);
            this.MnuChangeBackgroundColor.Text = "[Change background color...]";
            this.MnuChangeBackgroundColor.Click += new System.EventHandler(this.MnuChangeBackgroundColor_Click);
            // 
            // MnuNavigation
            // 
            this.MnuNavigation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuViewNext,
            this.MnuViewPrevious,
            this.toolStripMenuItem4,
            this.MnuGoToFirst,
            this.MnuGoToLast});
            this.MnuNavigation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuNavigation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuNavigation.Name = "MnuNavigation";
            this.MnuNavigation.Size = new System.Drawing.Size(300, 28);
            this.MnuNavigation.Text = "[Navigation]";
            // 
            // MnuViewNext
            // 
            this.MnuViewNext.Name = "MnuViewNext";
            this.MnuViewNext.Size = new System.Drawing.Size(272, 30);
            this.MnuViewNext.Text = "[View next image]";
            this.MnuViewNext.Click += new System.EventHandler(this.MnuViewNext_Click);
            // 
            // MnuViewPrevious
            // 
            this.MnuViewPrevious.Name = "MnuViewPrevious";
            this.MnuViewPrevious.Size = new System.Drawing.Size(272, 30);
            this.MnuViewPrevious.Text = "[View previos image]";
            this.MnuViewPrevious.Click += new System.EventHandler(this.MnuViewPrevious_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(269, 6);
            // 
            // MnuGoToFirst
            // 
            this.MnuGoToFirst.Name = "MnuGoToFirst";
            this.MnuGoToFirst.Size = new System.Drawing.Size(272, 30);
            this.MnuGoToFirst.Text = "[Go to the first image]";
            this.MnuGoToFirst.Click += new System.EventHandler(this.MnuGoToFirst_Click);
            // 
            // MnuGoToLast
            // 
            this.MnuGoToLast.Name = "MnuGoToLast";
            this.MnuGoToLast.Size = new System.Drawing.Size(272, 30);
            this.MnuGoToLast.Text = "[Go to the last image]";
            this.MnuGoToLast.Click += new System.EventHandler(this.MnuGoToLast_Click);
            // 
            // MnuActualSize
            // 
            this.MnuActualSize.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuActualSize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuActualSize.Name = "MnuActualSize";
            this.MnuActualSize.Size = new System.Drawing.Size(300, 28);
            this.MnuActualSize.Text = "[View actual size]";
            this.MnuActualSize.Click += new System.EventHandler(this.MnuActualSize_Click);
            // 
            // MnuZoomModes
            // 
            this.MnuZoomModes.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuAutoZoom,
            this.MnuLockZoom,
            this.MnuScaleToWidth,
            this.MnuScaleToHeight,
            this.MnuScaleToFit,
            this.MnuScaleToFill});
            this.MnuZoomModes.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuZoomModes.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuZoomModes.Name = "MnuZoomModes";
            this.MnuZoomModes.Size = new System.Drawing.Size(300, 28);
            this.MnuZoomModes.Text = "[Zoom modes]";
            // 
            // MnuAutoZoom
            // 
            this.MnuAutoZoom.CheckOnClick = true;
            this.MnuAutoZoom.Name = "MnuAutoZoom";
            this.MnuAutoZoom.Size = new System.Drawing.Size(236, 30);
            this.MnuAutoZoom.Text = "[Auto Zoom]";
            this.MnuAutoZoom.Click += new System.EventHandler(this.MnuAutoZoom_Click);
            // 
            // MnuLockZoom
            // 
            this.MnuLockZoom.CheckOnClick = true;
            this.MnuLockZoom.Name = "MnuLockZoom";
            this.MnuLockZoom.Size = new System.Drawing.Size(236, 30);
            this.MnuLockZoom.Text = "[Lock zoom ratio]";
            this.MnuLockZoom.Click += new System.EventHandler(this.MnuLockZoom_Click);
            // 
            // MnuScaleToWidth
            // 
            this.MnuScaleToWidth.CheckOnClick = true;
            this.MnuScaleToWidth.Name = "MnuScaleToWidth";
            this.MnuScaleToWidth.Size = new System.Drawing.Size(236, 30);
            this.MnuScaleToWidth.Text = "[Scale to width]";
            this.MnuScaleToWidth.Click += new System.EventHandler(this.MnuScaleToWidth_Click);
            // 
            // MnuScaleToHeight
            // 
            this.MnuScaleToHeight.CheckOnClick = true;
            this.MnuScaleToHeight.Name = "MnuScaleToHeight";
            this.MnuScaleToHeight.Size = new System.Drawing.Size(236, 30);
            this.MnuScaleToHeight.Text = "[Scale to height]";
            this.MnuScaleToHeight.Click += new System.EventHandler(this.MnuScaleToHeight_Click);
            // 
            // MnuScaleToFit
            // 
            this.MnuScaleToFit.CheckOnClick = true;
            this.MnuScaleToFit.Name = "MnuScaleToFit";
            this.MnuScaleToFit.Size = new System.Drawing.Size(236, 30);
            this.MnuScaleToFit.Text = "[Scale to fit]";
            this.MnuScaleToFit.Click += new System.EventHandler(this.MnuScaleToFit_Click);
            // 
            // MnuScaleToFill
            // 
            this.MnuScaleToFill.CheckOnClick = true;
            this.MnuScaleToFill.Name = "MnuScaleToFill";
            this.MnuScaleToFill.Size = new System.Drawing.Size(236, 30);
            this.MnuScaleToFill.Text = "[Scale to fill]";
            this.MnuScaleToFill.Click += new System.EventHandler(this.MnuScaleToFill_Click);
            // 
            // MnuLoadingOrders
            // 
            this.MnuLoadingOrders.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuLoadingOrders.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuLoadingOrders.Name = "MnuLoadingOrders";
            this.MnuLoadingOrders.Size = new System.Drawing.Size(300, 28);
            this.MnuLoadingOrders.Text = "[Loading orders]";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(297, 6);
            // 
            // MnuOpenWith
            // 
            this.MnuOpenWith.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuOpenWith.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuOpenWith.Name = "MnuOpenWith";
            this.MnuOpenWith.Size = new System.Drawing.Size(300, 28);
            this.MnuOpenWith.Text = "[Open with...]";
            this.MnuOpenWith.Click += new System.EventHandler(this.MnuOpenWith_Click);
            // 
            // MnuOpenLocation
            // 
            this.MnuOpenLocation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuOpenLocation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuOpenLocation.Name = "MnuOpenLocation";
            this.MnuOpenLocation.Size = new System.Drawing.Size(300, 28);
            this.MnuOpenLocation.Text = "[Open image location]";
            this.MnuOpenLocation.Click += new System.EventHandler(this.MnuOpenLocation_Click);
            // 
            // MnuCopyPath
            // 
            this.MnuCopyPath.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuCopyPath.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuCopyPath.Name = "MnuCopyPath";
            this.MnuCopyPath.Size = new System.Drawing.Size(300, 28);
            this.MnuCopyPath.Text = "[Copy image path]";
            this.MnuCopyPath.Click += new System.EventHandler(this.MnuCopyPath_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(297, 6);
            // 
            // MnuExitSlideshow
            // 
            this.MnuExitSlideshow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuExitSlideshow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuExitSlideshow.Name = "MnuExitSlideshow";
            this.MnuExitSlideshow.Size = new System.Drawing.Size(300, 28);
            this.MnuExitSlideshow.Text = "[Exit slideshow]";
            this.MnuExitSlideshow.Click += new System.EventHandler(this.MnuExitSlideshow_Click);
            // 
            // FrmSlideshow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackdropMargin = new System.Windows.Forms.Padding(-1);
            this.ClientSize = new System.Drawing.Size(1112, 674);
            this.Controls.Add(this.PicMain);
            this.KeyPreview = true;
            this.Name = "FrmSlideshow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSlideshow_FormClosing);
            this.Load += new System.EventHandler(this.FrmSlideshow_Load);
            this.MnuContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageGlass.Views.DXCanvas PicMain;
        private ImageGlass.UI.ModernMenu MnuContext;
        private ToolStripMenuItem MnuPauseResumeSlideshow;
        private ToolStripMenuItem MnuExitSlideshow;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem MnuZoomModes;
        private ToolStripMenuItem MnuLoadingOrders;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem MnuToggleCheckerboard;
        private ToolStripMenuItem MnuFullScreen;
        private ToolStripMenuItem MnuOpenLocation;
        private ToolStripMenuItem MnuCopyPath;
        private ToolStripMenuItem MnuOpenWith;
        private ToolStripMenuItem MnuActualSize;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripMenuItem MnuNavigation;
        private ToolStripMenuItem MnuViewNext;
        private ToolStripMenuItem MnuViewPrevious;
        private ToolStripSeparator toolStripMenuItem4;
        private ToolStripMenuItem MnuGoToFirst;
        private ToolStripMenuItem MnuGoToLast;
        private ToolStripMenuItem MnuAutoZoom;
        private ToolStripMenuItem MnuLockZoom;
        private ToolStripMenuItem MnuScaleToWidth;
        private ToolStripMenuItem MnuScaleToHeight;
        private ToolStripMenuItem MnuScaleToFit;
        private ToolStripMenuItem MnuScaleToFill;
        private ToolStripMenuItem MnuToggleCountdown;
        private ToolStripMenuItem MnuChangeBackgroundColor;
        private ToolStripSeparator toolStripMenuItem5;
    }
}