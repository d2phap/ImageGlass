namespace igcmd.Tools
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
            components = new System.ComponentModel.Container();
            PicMain = new ImageGlass.Viewer.DXCanvas();
            MnuContext = new ImageGlass.UI.ModernMenu(components);
            MnuPauseResumeSlideshow = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            MnuWindowFit = new ToolStripMenuItem();
            MnuFrameless = new ToolStripMenuItem();
            MnuFullScreen = new ToolStripMenuItem();
            MnuToggleCountdown = new ToolStripMenuItem();
            MnuToggleCheckerboard = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripSeparator();
            MnuChangeBackgroundColor = new ToolStripMenuItem();
            MnuNavigation = new ToolStripMenuItem();
            MnuViewNext = new ToolStripMenuItem();
            MnuViewPrevious = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripSeparator();
            MnuGoToFirst = new ToolStripMenuItem();
            MnuGoToLast = new ToolStripMenuItem();
            MnuActualSize = new ToolStripMenuItem();
            MnuZoomModes = new ToolStripMenuItem();
            MnuAutoZoom = new ToolStripMenuItem();
            MnuLockZoom = new ToolStripMenuItem();
            MnuScaleToWidth = new ToolStripMenuItem();
            MnuScaleToHeight = new ToolStripMenuItem();
            MnuScaleToFit = new ToolStripMenuItem();
            MnuScaleToFill = new ToolStripMenuItem();
            MnuLoadingOrders = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            MnuOpenWith = new ToolStripMenuItem();
            MnuOpenLocation = new ToolStripMenuItem();
            MnuCopyPath = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripSeparator();
            MnuExitSlideshow = new ToolStripMenuItem();
            MnuContext.SuspendLayout();
            SuspendLayout();
            // 
            // PicMain
            // 
            PicMain.BaseDpi = 96F;
            PicMain.CheckFPS = false;
            PicMain.ContextMenuStrip = MnuContext;
            PicMain.Dock = DockStyle.Fill;
            PicMain.Location = new Point(0, 0);
            PicMain.Margin = new Padding(0);
            PicMain.Name = "PicMain";
            PicMain.RequestUpdateFrame = false;
            PicMain.Size = new Size(628, 388);
            PicMain.TabIndex = 0;
            PicMain.OnZoomChanged += PicMain_OnZoomChanged;
            PicMain.OnNavLeftClicked += PicMain_OnNavLeftClicked;
            PicMain.OnNavRightClicked += PicMain_OnNavRightClicked;
            PicMain.Web2NavigationCompleted += PicMain_Web2NavigationCompleted;
            PicMain.Web2PointerDown += PicMain_Web2PointerDown;
            PicMain.Web2KeyDown += PicMain_Web2KeyDown;
            PicMain.Web2KeyUp += PicMain_Web2KeyUp;
            PicMain.Render += PicMain_Render;
            PicMain.MouseClick += PicMain_MouseClick;
            PicMain.MouseLeave += PicMain_MouseLeave;
            PicMain.MouseMove += PicMain_MouseMove;
            PicMain.MouseWheel += PicMain_MouseWheel;
            // 
            // MnuContext
            // 
            MnuContext.CurrentDpi = 96;
            MnuContext.ImageScalingSize = new Size(22, 22);
            MnuContext.Items.AddRange(new ToolStripItem[] { MnuPauseResumeSlideshow, toolStripMenuItem1, MnuWindowFit, MnuFrameless, MnuFullScreen, MnuToggleCountdown, MnuToggleCheckerboard, toolStripMenuItem3, MnuChangeBackgroundColor, MnuNavigation, MnuActualSize, MnuZoomModes, MnuLoadingOrders, toolStripMenuItem2, MnuOpenWith, MnuOpenLocation, MnuCopyPath, toolStripMenuItem5, MnuExitSlideshow });
            MnuContext.Name = "MnuContext";
            MnuContext.Size = new Size(246, 358);
            MnuContext.Opening += MnuContext_Opening;
            // 
            // MnuPauseResumeSlideshow
            // 
            MnuPauseResumeSlideshow.ImageAlign = ContentAlignment.MiddleLeft;
            MnuPauseResumeSlideshow.ImageScaling = ToolStripItemImageScaling.None;
            MnuPauseResumeSlideshow.Name = "MnuPauseResumeSlideshow";
            MnuPauseResumeSlideshow.Size = new Size(245, 22);
            MnuPauseResumeSlideshow.Text = "[Pause / resume slideshow]";
            MnuPauseResumeSlideshow.Click += MnuPauseResumeSlideshow_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(242, 6);
            // 
            // MnuWindowFit
            // 
            MnuWindowFit.CheckOnClick = true;
            MnuWindowFit.ImageAlign = ContentAlignment.MiddleLeft;
            MnuWindowFit.ImageScaling = ToolStripItemImageScaling.None;
            MnuWindowFit.Name = "MnuWindowFit";
            MnuWindowFit.Size = new Size(245, 22);
            MnuWindowFit.Text = "[Window fit]";
            MnuWindowFit.Click += MnuWindowFit_Click;
            // 
            // MnuFrameless
            // 
            MnuFrameless.CheckOnClick = true;
            MnuFrameless.ImageAlign = ContentAlignment.MiddleLeft;
            MnuFrameless.ImageScaling = ToolStripItemImageScaling.None;
            MnuFrameless.Name = "MnuFrameless";
            MnuFrameless.Size = new Size(245, 22);
            MnuFrameless.Text = "[Frameless]";
            MnuFrameless.Click += MnuFrameless_Click;
            // 
            // MnuFullScreen
            // 
            MnuFullScreen.CheckOnClick = true;
            MnuFullScreen.ImageAlign = ContentAlignment.MiddleLeft;
            MnuFullScreen.ImageScaling = ToolStripItemImageScaling.None;
            MnuFullScreen.Name = "MnuFullScreen";
            MnuFullScreen.Size = new Size(245, 22);
            MnuFullScreen.Text = "[Full screen]";
            MnuFullScreen.Click += MnuFullScreen_Click;
            // 
            // MnuToggleCountdown
            // 
            MnuToggleCountdown.CheckOnClick = true;
            MnuToggleCountdown.ImageAlign = ContentAlignment.MiddleLeft;
            MnuToggleCountdown.ImageScaling = ToolStripItemImageScaling.None;
            MnuToggleCountdown.Name = "MnuToggleCountdown";
            MnuToggleCountdown.Size = new Size(245, 22);
            MnuToggleCountdown.Text = "[Show countdown]";
            MnuToggleCountdown.Click += MnuToggleCountdown_Click;
            // 
            // MnuToggleCheckerboard
            // 
            MnuToggleCheckerboard.CheckOnClick = true;
            MnuToggleCheckerboard.ImageAlign = ContentAlignment.MiddleLeft;
            MnuToggleCheckerboard.ImageScaling = ToolStripItemImageScaling.None;
            MnuToggleCheckerboard.Name = "MnuToggleCheckerboard";
            MnuToggleCheckerboard.Size = new Size(245, 22);
            MnuToggleCheckerboard.Text = "[Checkerboard background]";
            MnuToggleCheckerboard.Click += MnuToggleCheckerboard_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(242, 6);
            // 
            // MnuChangeBackgroundColor
            // 
            MnuChangeBackgroundColor.ImageAlign = ContentAlignment.MiddleLeft;
            MnuChangeBackgroundColor.ImageScaling = ToolStripItemImageScaling.None;
            MnuChangeBackgroundColor.Name = "MnuChangeBackgroundColor";
            MnuChangeBackgroundColor.Size = new Size(245, 22);
            MnuChangeBackgroundColor.Text = "[Change background color...]";
            MnuChangeBackgroundColor.Click += MnuChangeBackgroundColor_Click;
            // 
            // MnuNavigation
            // 
            MnuNavigation.DropDownItems.AddRange(new ToolStripItem[] { MnuViewNext, MnuViewPrevious, toolStripMenuItem4, MnuGoToFirst, MnuGoToLast });
            MnuNavigation.ImageAlign = ContentAlignment.MiddleLeft;
            MnuNavigation.ImageScaling = ToolStripItemImageScaling.None;
            MnuNavigation.Name = "MnuNavigation";
            MnuNavigation.Size = new Size(245, 22);
            MnuNavigation.Text = "[Navigation]";
            // 
            // MnuViewNext
            // 
            MnuViewNext.Name = "MnuViewNext";
            MnuViewNext.Size = new Size(205, 22);
            MnuViewNext.Text = "[View next image]";
            MnuViewNext.Click += MnuViewNext_Click;
            // 
            // MnuViewPrevious
            // 
            MnuViewPrevious.Name = "MnuViewPrevious";
            MnuViewPrevious.Size = new Size(205, 22);
            MnuViewPrevious.Text = "[View previos image]";
            MnuViewPrevious.Click += MnuViewPrevious_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(202, 6);
            // 
            // MnuGoToFirst
            // 
            MnuGoToFirst.Name = "MnuGoToFirst";
            MnuGoToFirst.Size = new Size(205, 22);
            MnuGoToFirst.Text = "[Go to the first image]";
            MnuGoToFirst.Click += MnuGoToFirst_Click;
            // 
            // MnuGoToLast
            // 
            MnuGoToLast.Name = "MnuGoToLast";
            MnuGoToLast.Size = new Size(205, 22);
            MnuGoToLast.Text = "[Go to the last image]";
            MnuGoToLast.Click += MnuGoToLast_Click;
            // 
            // MnuActualSize
            // 
            MnuActualSize.ImageAlign = ContentAlignment.MiddleLeft;
            MnuActualSize.ImageScaling = ToolStripItemImageScaling.None;
            MnuActualSize.Name = "MnuActualSize";
            MnuActualSize.Size = new Size(245, 22);
            MnuActualSize.Text = "[View actual size]";
            MnuActualSize.Click += MnuActualSize_Click;
            // 
            // MnuZoomModes
            // 
            MnuZoomModes.DropDownItems.AddRange(new ToolStripItem[] { MnuAutoZoom, MnuLockZoom, MnuScaleToWidth, MnuScaleToHeight, MnuScaleToFit, MnuScaleToFill });
            MnuZoomModes.ImageAlign = ContentAlignment.MiddleLeft;
            MnuZoomModes.ImageScaling = ToolStripItemImageScaling.None;
            MnuZoomModes.Name = "MnuZoomModes";
            MnuZoomModes.Size = new Size(245, 22);
            MnuZoomModes.Text = "[Zoom modes]";
            // 
            // MnuAutoZoom
            // 
            MnuAutoZoom.CheckOnClick = true;
            MnuAutoZoom.Name = "MnuAutoZoom";
            MnuAutoZoom.Size = new Size(178, 22);
            MnuAutoZoom.Text = "[Auto Zoom]";
            MnuAutoZoom.Click += MnuAutoZoom_Click;
            // 
            // MnuLockZoom
            // 
            MnuLockZoom.CheckOnClick = true;
            MnuLockZoom.Name = "MnuLockZoom";
            MnuLockZoom.Size = new Size(178, 22);
            MnuLockZoom.Text = "[Lock zoom ratio]";
            MnuLockZoom.Click += MnuLockZoom_Click;
            // 
            // MnuScaleToWidth
            // 
            MnuScaleToWidth.CheckOnClick = true;
            MnuScaleToWidth.Name = "MnuScaleToWidth";
            MnuScaleToWidth.Size = new Size(178, 22);
            MnuScaleToWidth.Text = "[Scale to width]";
            MnuScaleToWidth.Click += MnuScaleToWidth_Click;
            // 
            // MnuScaleToHeight
            // 
            MnuScaleToHeight.CheckOnClick = true;
            MnuScaleToHeight.Name = "MnuScaleToHeight";
            MnuScaleToHeight.Size = new Size(178, 22);
            MnuScaleToHeight.Text = "[Scale to height]";
            MnuScaleToHeight.Click += MnuScaleToHeight_Click;
            // 
            // MnuScaleToFit
            // 
            MnuScaleToFit.CheckOnClick = true;
            MnuScaleToFit.Name = "MnuScaleToFit";
            MnuScaleToFit.Size = new Size(178, 22);
            MnuScaleToFit.Text = "[Scale to fit]";
            MnuScaleToFit.Click += MnuScaleToFit_Click;
            // 
            // MnuScaleToFill
            // 
            MnuScaleToFill.CheckOnClick = true;
            MnuScaleToFill.Name = "MnuScaleToFill";
            MnuScaleToFill.Size = new Size(178, 22);
            MnuScaleToFill.Text = "[Scale to fill]";
            MnuScaleToFill.Click += MnuScaleToFill_Click;
            // 
            // MnuLoadingOrders
            // 
            MnuLoadingOrders.ImageAlign = ContentAlignment.MiddleLeft;
            MnuLoadingOrders.ImageScaling = ToolStripItemImageScaling.None;
            MnuLoadingOrders.Name = "MnuLoadingOrders";
            MnuLoadingOrders.Size = new Size(245, 22);
            MnuLoadingOrders.Text = "[Loading orders]";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(242, 6);
            // 
            // MnuOpenWith
            // 
            MnuOpenWith.ImageAlign = ContentAlignment.MiddleLeft;
            MnuOpenWith.ImageScaling = ToolStripItemImageScaling.None;
            MnuOpenWith.Name = "MnuOpenWith";
            MnuOpenWith.Size = new Size(245, 22);
            MnuOpenWith.Text = "[Open with...]";
            MnuOpenWith.Click += MnuOpenWith_Click;
            // 
            // MnuOpenLocation
            // 
            MnuOpenLocation.ImageAlign = ContentAlignment.MiddleLeft;
            MnuOpenLocation.ImageScaling = ToolStripItemImageScaling.None;
            MnuOpenLocation.Name = "MnuOpenLocation";
            MnuOpenLocation.Size = new Size(245, 22);
            MnuOpenLocation.Text = "[Open image location]";
            MnuOpenLocation.Click += MnuOpenLocation_Click;
            // 
            // MnuCopyPath
            // 
            MnuCopyPath.ImageAlign = ContentAlignment.MiddleLeft;
            MnuCopyPath.ImageScaling = ToolStripItemImageScaling.None;
            MnuCopyPath.Name = "MnuCopyPath";
            MnuCopyPath.Size = new Size(245, 22);
            MnuCopyPath.Text = "[Copy image path]";
            MnuCopyPath.Click += MnuCopyPath_Click;
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(242, 6);
            // 
            // MnuExitSlideshow
            // 
            MnuExitSlideshow.ImageAlign = ContentAlignment.MiddleLeft;
            MnuExitSlideshow.ImageScaling = ToolStripItemImageScaling.None;
            MnuExitSlideshow.Name = "MnuExitSlideshow";
            MnuExitSlideshow.Size = new Size(245, 22);
            MnuExitSlideshow.Text = "[Exit slideshow]";
            MnuExitSlideshow.Click += MnuExitSlideshow_Click;
            // 
            // FrmSlideshow
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(628, 388);
            Controls.Add(PicMain);
            KeyPreview = true;
            Margin = new Padding(2, 2, 2, 2);
            Name = "FrmSlideshow";
            KeyDown += FrmSlideshow_KeyDown;
            MnuContext.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ImageGlass.Viewer.DXCanvas PicMain;
        private ImageGlass.UI.ModernMenu MnuContext;
        private ToolStripMenuItem MnuPauseResumeSlideshow;
        private ToolStripMenuItem MnuExitSlideshow;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem MnuZoomModes;
        private ToolStripMenuItem MnuLoadingOrders;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem MnuToggleCheckerboard;
        private ToolStripMenuItem MnuWindowFit;
        private ToolStripMenuItem MnuFrameless;
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