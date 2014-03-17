namespace ImageGlass {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.picMain = new ImageGlass.ImageBox();
            this.mnuPopup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuStartSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStopSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExitSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPhanCach = new System.Windows.Forms.ToolStripSeparator();
            this.mnuShowToolBar = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditWithPaint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExtractFrames = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSetWallpaper = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMoveRecycle = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuUploadFacebook = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCopyImagePath = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImageProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.thumbBar = new ImageGlass.ThumbBar.ThumbnailFlowLayoutPanel();
            this.timSlideShow = new System.Windows.Forms.Timer(this.components);
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.sysWatch = new System.IO.FileSystemWatcher();
            this.spMain = new System.Windows.Forms.SplitContainer();
            this.toolMain = new System.Windows.Forms.ToolStrip();
            this.btnBack = new System.Windows.Forms.ToolStripButton();
            this.btnNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRotateLeft = new System.Windows.Forms.ToolStripButton();
            this.btnRotateRight = new System.Windows.Forms.ToolStripButton();
            this.btnZoomIn = new System.Windows.Forms.ToolStripButton();
            this.btnZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnActualSize = new System.Windows.Forms.ToolStripButton();
            this.btnZoomLock = new System.Windows.Forms.ToolStripButton();
            this.btnScaletoWidth = new System.Windows.Forms.ToolStripButton();
            this.btnScaletoHeight = new System.Windows.Forms.ToolStripButton();
            this.btnWindowAutosize = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnGoto = new System.Windows.Forms.ToolStripButton();
            this.btnThumb = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCaro = new System.Windows.Forms.ToolStripButton();
            this.btnFullScreen = new System.Windows.Forms.ToolStripButton();
            this.btnSlideShow = new System.Windows.Forms.ToolStripButton();
            this.btnConvert = new System.Windows.Forms.ToolStripButton();
            this.btnPrintImage = new System.Windows.Forms.ToolStripButton();
            this.btnFacebook = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExtension = new System.Windows.Forms.ToolStripButton();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.btnHelp = new System.Windows.Forms.ToolStripButton();
            this.btnReport = new System.Windows.Forms.ToolStripButton();
            this.btnFollow = new System.Windows.Forms.ToolStripButton();
            this.btnFacebookLike = new System.Windows.Forms.ToolStripButton();
            this.lblZoomRatio = new System.Windows.Forms.ToolStripLabel();
            this.lblImageType = new System.Windows.Forms.ToolStripLabel();
            this.lblImageSize = new System.Windows.Forms.ToolStripLabel();
            this.lblImageFileSize = new System.Windows.Forms.ToolStripLabel();
            this.lblImageDateCreate = new System.Windows.Forms.ToolStripLabel();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.Panel2.SuspendLayout();
            this.sp0.SuspendLayout();
            this.mnuPopup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sysWatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spMain)).BeginInit();
            this.spMain.Panel1.SuspendLayout();
            this.spMain.Panel2.SuspendLayout();
            this.spMain.SuspendLayout();
            this.toolMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // sp0
            // 
            this.sp0.AllowDrop = true;
            this.sp0.BackColor = System.Drawing.Color.White;
            this.sp0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp0.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.sp0.IsSplitterFixed = true;
            this.sp0.Location = new System.Drawing.Point(0, 0);
            this.sp0.Name = "sp0";
            this.sp0.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.AllowDrop = true;
            this.sp0.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.sp0.Panel1.Controls.Add(this.picMain);
            this.sp0.Panel1.MouseEnter += new System.EventHandler(this.sp0_Panel1_MouseEnter);
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.BackgroundImage = global::ImageGlass.Properties.Resources.bottombar;
            this.sp0.Panel2.Controls.Add(this.thumbBar);
            this.sp0.Panel2Collapsed = true;
            this.sp0.Size = new System.Drawing.Size(864, 437);
            this.sp0.SplitterDistance = 342;
            this.sp0.SplitterWidth = 1;
            this.sp0.TabIndex = 1;
            this.sp0.TabStop = false;
            // 
            // picMain
            // 
            this.picMain.AllowDrop = true;
            this.picMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.picMain.ContextMenuStrip = this.mnuPopup;
            this.picMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMain.GridDisplayMode = ImageGlass.ImageBoxGridDisplayMode.None;
            this.picMain.Location = new System.Drawing.Point(0, 0);
            this.picMain.Name = "picMain";
            this.picMain.Size = new System.Drawing.Size(864, 437);
            this.picMain.TabIndex = 1;
            this.picMain.Text = "Dương Diệu Pháp";
            this.picMain.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.picMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.picMain_DragDrop);
            this.picMain.DragOver += new System.Windows.Forms.DragEventHandler(this.picMain_DragOver);
            // 
            // mnuPopup
            // 
            this.mnuPopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStartSlideshow,
            this.mnuStopSlideshow,
            this.mnuExitSlideshow,
            this.mnuPhanCach,
            this.mnuShowToolBar,
            this.toolStripMenuItem4,
            this.mnuEditWithPaint,
            this.mnuExtractFrames,
            this.toolStripMenuItem1,
            this.mnuSetWallpaper,
            this.mnuMoveRecycle,
            this.mnuDelete,
            this.mnuRename,
            this.toolStripMenuItem2,
            this.mnuUploadFacebook,
            this.toolStripMenuItem3,
            this.mnuCopyImagePath,
            this.mnuOpenLocation,
            this.mnuImageProperties});
            this.mnuPopup.Name = "mnuPopup";
            this.mnuPopup.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mnuPopup.Size = new System.Drawing.Size(258, 342);
            this.mnuPopup.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.mnuPopup_Closing);
            this.mnuPopup.Opening += new System.ComponentModel.CancelEventHandler(this.mnuPopup_Opening);
            // 
            // mnuStartSlideshow
            // 
            this.mnuStartSlideshow.Name = "mnuStartSlideshow";
            this.mnuStartSlideshow.ShortcutKeyDisplayString = "Space";
            this.mnuStartSlideshow.Size = new System.Drawing.Size(257, 22);
            this.mnuStartSlideshow.Text = "Start slideshow";
            this.mnuStartSlideshow.Visible = false;
            this.mnuStartSlideshow.Click += new System.EventHandler(this.mnuStartSlideshow_Click);
            // 
            // mnuStopSlideshow
            // 
            this.mnuStopSlideshow.Name = "mnuStopSlideshow";
            this.mnuStopSlideshow.ShortcutKeyDisplayString = "Space";
            this.mnuStopSlideshow.Size = new System.Drawing.Size(257, 22);
            this.mnuStopSlideshow.Text = "Stop slideshow";
            this.mnuStopSlideshow.Visible = false;
            this.mnuStopSlideshow.Click += new System.EventHandler(this.mnuStopSlideshow_Click);
            // 
            // mnuExitSlideshow
            // 
            this.mnuExitSlideshow.Name = "mnuExitSlideshow";
            this.mnuExitSlideshow.ShortcutKeyDisplayString = "ESC";
            this.mnuExitSlideshow.Size = new System.Drawing.Size(257, 22);
            this.mnuExitSlideshow.Text = "Exit slideshow";
            this.mnuExitSlideshow.Visible = false;
            this.mnuExitSlideshow.Click += new System.EventHandler(this.mnuExitSlideshow_Click);
            // 
            // mnuPhanCach
            // 
            this.mnuPhanCach.Name = "mnuPhanCach";
            this.mnuPhanCach.Size = new System.Drawing.Size(254, 6);
            this.mnuPhanCach.Visible = false;
            // 
            // mnuShowToolBar
            // 
            this.mnuShowToolBar.Name = "mnuShowToolBar";
            this.mnuShowToolBar.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.mnuShowToolBar.Size = new System.Drawing.Size(257, 22);
            this.mnuShowToolBar.Text = "&Hide toolbar";
            this.mnuShowToolBar.Click += new System.EventHandler(this.mnuShowToolBar_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuEditWithPaint
            // 
            this.mnuEditWithPaint.Name = "mnuEditWithPaint";
            this.mnuEditWithPaint.Size = new System.Drawing.Size(257, 22);
            this.mnuEditWithPaint.Text = "&Edit with Paint";
            this.mnuEditWithPaint.Click += new System.EventHandler(this.mnuEditWithPaint_Click);
            // 
            // mnuExtractFrames
            // 
            this.mnuExtractFrames.Enabled = false;
            this.mnuExtractFrames.Name = "mnuExtractFrames";
            this.mnuExtractFrames.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.mnuExtractFrames.Size = new System.Drawing.Size(257, 22);
            this.mnuExtractFrames.Text = "E&xtract image frames";
            this.mnuExtractFrames.Click += new System.EventHandler(this.mnuExtractFrames_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuSetWallpaper
            // 
            this.mnuSetWallpaper.Name = "mnuSetWallpaper";
            this.mnuSetWallpaper.Size = new System.Drawing.Size(257, 22);
            this.mnuSetWallpaper.Text = "&Set as desktop background";
            this.mnuSetWallpaper.Click += new System.EventHandler(this.mnuWallpaper_Click);
            // 
            // mnuMoveRecycle
            // 
            this.mnuMoveRecycle.Name = "mnuMoveRecycle";
            this.mnuMoveRecycle.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.mnuMoveRecycle.Size = new System.Drawing.Size(257, 22);
            this.mnuMoveRecycle.Text = "&Move to recycle bin";
            this.mnuMoveRecycle.Click += new System.EventHandler(this.mnuRecycleBin_Click);
            // 
            // mnuDelete
            // 
            this.mnuDelete.Image = ((System.Drawing.Image)(resources.GetObject("mnuDelete.Image")));
            this.mnuDelete.Name = "mnuDelete";
            this.mnuDelete.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete)));
            this.mnuDelete.Size = new System.Drawing.Size(257, 22);
            this.mnuDelete.Text = "&Delete from hard disk";
            this.mnuDelete.Click += new System.EventHandler(this.mnuDelete_Click);
            // 
            // mnuRename
            // 
            this.mnuRename.Name = "mnuRename";
            this.mnuRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.mnuRename.Size = new System.Drawing.Size(257, 22);
            this.mnuRename.Text = "&Rename image";
            this.mnuRename.Click += new System.EventHandler(this.mnuRename_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuUploadFacebook
            // 
            this.mnuUploadFacebook.Image = ((System.Drawing.Image)(resources.GetObject("mnuUploadFacebook.Image")));
            this.mnuUploadFacebook.Name = "mnuUploadFacebook";
            this.mnuUploadFacebook.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.mnuUploadFacebook.Size = new System.Drawing.Size(257, 22);
            this.mnuUploadFacebook.Text = "&Upload to Facebook";
            this.mnuUploadFacebook.Click += new System.EventHandler(this.mnuUploadFacebook_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(254, 6);
            // 
            // mnuCopyImagePath
            // 
            this.mnuCopyImagePath.Name = "mnuCopyImagePath";
            this.mnuCopyImagePath.Size = new System.Drawing.Size(257, 22);
            this.mnuCopyImagePath.Text = "&Copy image path";
            this.mnuCopyImagePath.Click += new System.EventHandler(this.mnuCopyImagePath_Click);
            // 
            // mnuOpenLocation
            // 
            this.mnuOpenLocation.Name = "mnuOpenLocation";
            this.mnuOpenLocation.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.L)));
            this.mnuOpenLocation.Size = new System.Drawing.Size(257, 22);
            this.mnuOpenLocation.Text = "&Open image location";
            this.mnuOpenLocation.Click += new System.EventHandler(this.mnuImageLocation_Click);
            // 
            // mnuImageProperties
            // 
            this.mnuImageProperties.Name = "mnuImageProperties";
            this.mnuImageProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.mnuImageProperties.Size = new System.Drawing.Size(257, 22);
            this.mnuImageProperties.Text = "&Image properties";
            this.mnuImageProperties.Click += new System.EventHandler(this.mnuProperties_Click);
            // 
            // thumbBar
            // 
            this.thumbBar.AutoScroll = true;
            this.thumbBar.BackColor = System.Drawing.Color.Transparent;
            this.thumbBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thumbBar.Location = new System.Drawing.Point(0, 0);
            this.thumbBar.Name = "thumbBar";
            this.thumbBar.Size = new System.Drawing.Size(150, 46);
            this.thumbBar.TabIndex = 0;
            this.thumbBar.WrapContents = false;
            // 
            // timSlideShow
            // 
            this.timSlideShow.Interval = 2000;
            this.timSlideShow.Tick += new System.EventHandler(this.timSlideShow_Tick);
            // 
            // sysWatch
            // 
            this.sysWatch.EnableRaisingEvents = true;
            this.sysWatch.SynchronizingObject = this;
            this.sysWatch.Changed += new System.IO.FileSystemEventHandler(this.sysWatch_Changed);
            this.sysWatch.Deleted += new System.IO.FileSystemEventHandler(this.sysWatch_Deleted);
            this.sysWatch.Renamed += new System.IO.RenamedEventHandler(this.sysWatch_Renamed);
            // 
            // spMain
            // 
            this.spMain.BackColor = System.Drawing.Color.Transparent;
            this.spMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.spMain.IsSplitterFixed = true;
            this.spMain.Location = new System.Drawing.Point(0, 0);
            this.spMain.Name = "spMain";
            this.spMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spMain.Panel1
            // 
            this.spMain.Panel1.Controls.Add(this.toolMain);
            // 
            // spMain.Panel2
            // 
            this.spMain.Panel2.Controls.Add(this.sp0);
            this.spMain.Size = new System.Drawing.Size(864, 471);
            this.spMain.SplitterDistance = 33;
            this.spMain.SplitterWidth = 1;
            this.spMain.TabIndex = 2;
            this.spMain.TabStop = false;
            // 
            // toolMain
            // 
            this.toolMain.AllowMerge = false;
            this.toolMain.AutoSize = false;
            this.toolMain.BackColor = System.Drawing.Color.Transparent;
            this.toolMain.BackgroundImage = global::ImageGlass.Properties.Resources.topbar;
            this.toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBack,
            this.btnNext,
            this.toolStripSeparator1,
            this.btnRotateLeft,
            this.btnRotateRight,
            this.btnZoomIn,
            this.btnZoomOut,
            this.btnActualSize,
            this.btnZoomLock,
            this.btnScaletoWidth,
            this.btnScaletoHeight,
            this.btnWindowAutosize,
            this.toolStripSeparator2,
            this.btnOpen,
            this.btnRefresh,
            this.btnGoto,
            this.btnThumb,
            this.toolStripSeparator3,
            this.btnCaro,
            this.btnFullScreen,
            this.btnSlideShow,
            this.btnConvert,
            this.btnPrintImage,
            this.btnFacebook,
            this.toolStripSeparator4,
            this.btnExtension,
            this.btnSetting,
            this.btnHelp,
            this.btnReport,
            this.btnFollow,
            this.btnFacebookLike,
            this.lblZoomRatio,
            this.lblImageType,
            this.lblImageSize,
            this.lblImageFileSize,
            this.lblImageDateCreate});
            this.toolMain.Location = new System.Drawing.Point(0, 0);
            this.toolMain.Name = "toolMain";
            this.toolMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolMain.Size = new System.Drawing.Size(864, 33);
            this.toolMain.TabIndex = 1;
            // 
            // btnBack
            // 
            this.btnBack.AutoSize = false;
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            this.btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBack.Image = ((System.Drawing.Image)(resources.GetObject("btnBack.Image")));
            this.btnBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBack.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(25, 28);
            this.btnBack.ToolTipText = "Go to previous image (Left arrow / PageDown)";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnNext
            // 
            this.btnNext.AutoSize = false;
            this.btnNext.BackColor = System.Drawing.Color.Transparent;
            this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNext.Image")));
            this.btnNext.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNext.Margin = new System.Windows.Forms.Padding(0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(25, 28);
            this.btnNext.ToolTipText = "Go to next image (Right arrow / PageUp)";
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AutoSize = false;
            this.toolStripSeparator1.BackColor = System.Drawing.Color.Transparent;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(5, 28);
            // 
            // btnRotateLeft
            // 
            this.btnRotateLeft.AutoSize = false;
            this.btnRotateLeft.BackColor = System.Drawing.Color.Transparent;
            this.btnRotateLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRotateLeft.Image = global::ImageGlass.Properties.Resources.leftrotate;
            this.btnRotateLeft.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRotateLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRotateLeft.Margin = new System.Windows.Forms.Padding(0);
            this.btnRotateLeft.Name = "btnRotateLeft";
            this.btnRotateLeft.Size = new System.Drawing.Size(25, 28);
            this.btnRotateLeft.ToolTipText = "Rotate Counterclockwise (Ctrl + ,)";
            this.btnRotateLeft.Click += new System.EventHandler(this.btnRotateLeft_Click);
            // 
            // btnRotateRight
            // 
            this.btnRotateRight.AutoSize = false;
            this.btnRotateRight.BackColor = System.Drawing.Color.Transparent;
            this.btnRotateRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRotateRight.Image = ((System.Drawing.Image)(resources.GetObject("btnRotateRight.Image")));
            this.btnRotateRight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRotateRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRotateRight.Margin = new System.Windows.Forms.Padding(0);
            this.btnRotateRight.Name = "btnRotateRight";
            this.btnRotateRight.Size = new System.Drawing.Size(25, 28);
            this.btnRotateRight.Text = "Next";
            this.btnRotateRight.ToolTipText = "Rotate Clockwise (Ctrl + .)";
            this.btnRotateRight.Click += new System.EventHandler(this.btnRotateRight_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.AutoSize = false;
            this.btnZoomIn.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomIn.Image")));
            this.btnZoomIn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(0);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(25, 28);
            this.btnZoomIn.Tag = "0";
            this.btnZoomIn.ToolTipText = "Zoom in (Ctrl + =)";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.AutoSize = false;
            this.btnZoomOut.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomOut.Image")));
            this.btnZoomOut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(0);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(25, 28);
            this.btnZoomOut.ToolTipText = "Zoom out (Ctrl + -)";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnActualSize
            // 
            this.btnActualSize.AutoSize = false;
            this.btnActualSize.BackColor = System.Drawing.Color.Transparent;
            this.btnActualSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnActualSize.Image = ((System.Drawing.Image)(resources.GetObject("btnActualSize.Image")));
            this.btnActualSize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnActualSize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnActualSize.Margin = new System.Windows.Forms.Padding(0);
            this.btnActualSize.Name = "btnActualSize";
            this.btnActualSize.Size = new System.Drawing.Size(25, 28);
            this.btnActualSize.ToolTipText = "Actual size (Ctrl + 0)";
            this.btnActualSize.Click += new System.EventHandler(this.btnActualSize_Click);
            // 
            // btnZoomLock
            // 
            this.btnZoomLock.AutoSize = false;
            this.btnZoomLock.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomLock.CheckOnClick = true;
            this.btnZoomLock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomLock.Image = global::ImageGlass.Properties.Resources.zoomlock;
            this.btnZoomLock.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnZoomLock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomLock.Margin = new System.Windows.Forms.Padding(0);
            this.btnZoomLock.Name = "btnZoomLock";
            this.btnZoomLock.Size = new System.Drawing.Size(25, 28);
            this.btnZoomLock.Tag = "";
            this.btnZoomLock.ToolTipText = "Lock zoom ratio";
            this.btnZoomLock.Click += new System.EventHandler(this.btnZoomLock_Click);
            // 
            // btnScaletoWidth
            // 
            this.btnScaletoWidth.AutoSize = false;
            this.btnScaletoWidth.BackColor = System.Drawing.Color.Transparent;
            this.btnScaletoWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScaletoWidth.Image = ((System.Drawing.Image)(resources.GetObject("btnScaletoWidth.Image")));
            this.btnScaletoWidth.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnScaletoWidth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScaletoWidth.Margin = new System.Windows.Forms.Padding(0);
            this.btnScaletoWidth.Name = "btnScaletoWidth";
            this.btnScaletoWidth.Size = new System.Drawing.Size(25, 28);
            this.btnScaletoWidth.ToolTipText = "Scale to Width (Ctrl + W)";
            this.btnScaletoWidth.Click += new System.EventHandler(this.btnScaletoWidth_Click);
            // 
            // btnScaletoHeight
            // 
            this.btnScaletoHeight.AutoSize = false;
            this.btnScaletoHeight.BackColor = System.Drawing.Color.Transparent;
            this.btnScaletoHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScaletoHeight.Image = ((System.Drawing.Image)(resources.GetObject("btnScaletoHeight.Image")));
            this.btnScaletoHeight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnScaletoHeight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScaletoHeight.Margin = new System.Windows.Forms.Padding(0);
            this.btnScaletoHeight.Name = "btnScaletoHeight";
            this.btnScaletoHeight.Size = new System.Drawing.Size(25, 28);
            this.btnScaletoHeight.ToolTipText = "Scale to Height (Ctrl + H)";
            this.btnScaletoHeight.Click += new System.EventHandler(this.btnScaletoHeight_Click);
            // 
            // btnWindowAutosize
            // 
            this.btnWindowAutosize.AutoSize = false;
            this.btnWindowAutosize.BackColor = System.Drawing.Color.Transparent;
            this.btnWindowAutosize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWindowAutosize.Image = ((System.Drawing.Image)(resources.GetObject("btnWindowAutosize.Image")));
            this.btnWindowAutosize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnWindowAutosize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWindowAutosize.Margin = new System.Windows.Forms.Padding(0);
            this.btnWindowAutosize.Name = "btnWindowAutosize";
            this.btnWindowAutosize.Size = new System.Drawing.Size(25, 28);
            this.btnWindowAutosize.ToolTipText = "Window adapt to image (Ctrl + M)";
            this.btnWindowAutosize.Click += new System.EventHandler(this.btnWindowAutosize_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.AutoSize = false;
            this.toolStripSeparator2.BackColor = System.Drawing.Color.Transparent;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(5, 28);
            // 
            // btnOpen
            // 
            this.btnOpen.AutoSize = false;
            this.btnOpen.BackColor = System.Drawing.Color.Transparent;
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Margin = new System.Windows.Forms.Padding(0);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(25, 28);
            this.btnOpen.ToolTipText = "Open file (Ctrl + O)";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = false;
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(25, 28);
            this.btnRefresh.ToolTipText = "Refresh (F5)";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnGoto
            // 
            this.btnGoto.AutoSize = false;
            this.btnGoto.BackColor = System.Drawing.Color.Transparent;
            this.btnGoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnGoto.Image = ((System.Drawing.Image)(resources.GetObject("btnGoto.Image")));
            this.btnGoto.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnGoto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnGoto.Margin = new System.Windows.Forms.Padding(0);
            this.btnGoto.Name = "btnGoto";
            this.btnGoto.Size = new System.Drawing.Size(25, 28);
            this.btnGoto.ToolTipText = "Go to ... (Ctrl + G)";
            this.btnGoto.Click += new System.EventHandler(this.btnGoto_Click);
            // 
            // btnThumb
            // 
            this.btnThumb.AutoSize = false;
            this.btnThumb.BackColor = System.Drawing.Color.Transparent;
            this.btnThumb.CheckOnClick = true;
            this.btnThumb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnThumb.Image = ((System.Drawing.Image)(resources.GetObject("btnThumb.Image")));
            this.btnThumb.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnThumb.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnThumb.Margin = new System.Windows.Forms.Padding(0);
            this.btnThumb.Name = "btnThumb";
            this.btnThumb.Size = new System.Drawing.Size(25, 28);
            this.btnThumb.ToolTipText = "Show thumbnail (Ctrl + T)";
            this.btnThumb.Click += new System.EventHandler(this.btnThumb_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.AutoSize = false;
            this.toolStripSeparator3.BackColor = System.Drawing.Color.Transparent;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(5, 28);
            // 
            // btnCaro
            // 
            this.btnCaro.AutoSize = false;
            this.btnCaro.BackColor = System.Drawing.Color.Transparent;
            this.btnCaro.CheckOnClick = true;
            this.btnCaro.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCaro.Image = ((System.Drawing.Image)(resources.GetObject("btnCaro.Image")));
            this.btnCaro.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnCaro.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCaro.Margin = new System.Windows.Forms.Padding(0);
            this.btnCaro.Name = "btnCaro";
            this.btnCaro.Size = new System.Drawing.Size(25, 28);
            this.btnCaro.ToolTipText = "Show checked background (Ctrl + B)";
            this.btnCaro.Click += new System.EventHandler(this.btnCaro_Click);
            // 
            // btnFullScreen
            // 
            this.btnFullScreen.AutoSize = false;
            this.btnFullScreen.BackColor = System.Drawing.Color.Transparent;
            this.btnFullScreen.CheckOnClick = true;
            this.btnFullScreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFullScreen.Image = ((System.Drawing.Image)(resources.GetObject("btnFullScreen.Image")));
            this.btnFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFullScreen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFullScreen.Margin = new System.Windows.Forms.Padding(0);
            this.btnFullScreen.Name = "btnFullScreen";
            this.btnFullScreen.Size = new System.Drawing.Size(25, 28);
            this.btnFullScreen.ToolTipText = "Full sreen (Alt + Enter)";
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);
            // 
            // btnSlideShow
            // 
            this.btnSlideShow.AutoSize = false;
            this.btnSlideShow.BackColor = System.Drawing.Color.Transparent;
            this.btnSlideShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSlideShow.Image = ((System.Drawing.Image)(resources.GetObject("btnSlideShow.Image")));
            this.btnSlideShow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSlideShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSlideShow.Margin = new System.Windows.Forms.Padding(0);
            this.btnSlideShow.Name = "btnSlideShow";
            this.btnSlideShow.Size = new System.Drawing.Size(25, 28);
            this.btnSlideShow.ToolTipText = "Play slideshow (F11, ESC to exit)";
            this.btnSlideShow.Click += new System.EventHandler(this.btnSlideShow_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.AutoSize = false;
            this.btnConvert.BackColor = System.Drawing.Color.Transparent;
            this.btnConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConvert.Image = ((System.Drawing.Image)(resources.GetObject("btnConvert.Image")));
            this.btnConvert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConvert.Margin = new System.Windows.Forms.Padding(0);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(25, 28);
            this.btnConvert.ToolTipText = "Convert image (Ctrl + S)";
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnPrintImage
            // 
            this.btnPrintImage.AutoSize = false;
            this.btnPrintImage.BackColor = System.Drawing.Color.Transparent;
            this.btnPrintImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPrintImage.Image = global::ImageGlass.Properties.Resources.printer;
            this.btnPrintImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnPrintImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPrintImage.Margin = new System.Windows.Forms.Padding(0);
            this.btnPrintImage.Name = "btnPrintImage";
            this.btnPrintImage.Size = new System.Drawing.Size(25, 28);
            this.btnPrintImage.ToolTipText = "Print image (Ctrl + P)";
            this.btnPrintImage.Click += new System.EventHandler(this.btnPrintImage_Click);
            // 
            // btnFacebook
            // 
            this.btnFacebook.AutoSize = false;
            this.btnFacebook.BackColor = System.Drawing.Color.Transparent;
            this.btnFacebook.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFacebook.Image = global::ImageGlass.Properties.Resources.uploadfb;
            this.btnFacebook.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFacebook.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFacebook.Margin = new System.Windows.Forms.Padding(0);
            this.btnFacebook.Name = "btnFacebook";
            this.btnFacebook.Size = new System.Drawing.Size(25, 28);
            this.btnFacebook.ToolTipText = "Upload to Facebook (Ctrl + U)";
            this.btnFacebook.Click += new System.EventHandler(this.btnFacebook_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.AutoSize = false;
            this.toolStripSeparator4.BackColor = System.Drawing.Color.Transparent;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(5, 28);
            // 
            // btnExtension
            // 
            this.btnExtension.AutoSize = false;
            this.btnExtension.BackColor = System.Drawing.Color.Transparent;
            this.btnExtension.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExtension.Image = global::ImageGlass.Properties.Resources.extension;
            this.btnExtension.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnExtension.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExtension.Margin = new System.Windows.Forms.Padding(0);
            this.btnExtension.Name = "btnExtension";
            this.btnExtension.Size = new System.Drawing.Size(25, 28);
            this.btnExtension.ToolTipText = "Extension Manager (Ctrl + Shift + E)";
            this.btnExtension.Click += new System.EventHandler(this.btnExtension_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.AutoSize = false;
            this.btnSetting.BackColor = System.Drawing.Color.Transparent;
            this.btnSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetting.Image = ((System.Drawing.Image)(resources.GetObject("btnSetting.Image")));
            this.btnSetting.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(25, 28);
            this.btnSetting.ToolTipText = "ImageGlass Settings (Ctrl + Shift + P)";
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.AutoSize = false;
            this.btnHelp.BackColor = System.Drawing.Color.Transparent;
            this.btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHelp.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp.Image")));
            this.btnHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHelp.Margin = new System.Windows.Forms.Padding(0);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(25, 28);
            this.btnHelp.ToolTipText = "Help (F1)";
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnReport
            // 
            this.btnReport.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnReport.AutoSize = false;
            this.btnReport.BackColor = System.Drawing.Color.Transparent;
            this.btnReport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnReport.Image = ((System.Drawing.Image)(resources.GetObject("btnReport.Image")));
            this.btnReport.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnReport.Margin = new System.Windows.Forms.Padding(0);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(25, 28);
            this.btnReport.ToolTipText = "Report a bug or comment about ImageGlass";
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // btnFollow
            // 
            this.btnFollow.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnFollow.AutoSize = false;
            this.btnFollow.BackColor = System.Drawing.Color.Transparent;
            this.btnFollow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFollow.Image = ((System.Drawing.Image)(resources.GetObject("btnFollow.Image")));
            this.btnFollow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFollow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFollow.Margin = new System.Windows.Forms.Padding(0);
            this.btnFollow.Name = "btnFollow";
            this.btnFollow.Size = new System.Drawing.Size(25, 28);
            this.btnFollow.ToolTipText = "Follow ImageGlass by email";
            this.btnFollow.Click += new System.EventHandler(this.btnFollow_Click);
            // 
            // btnFacebookLike
            // 
            this.btnFacebookLike.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnFacebookLike.AutoSize = false;
            this.btnFacebookLike.BackColor = System.Drawing.Color.Transparent;
            this.btnFacebookLike.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFacebookLike.Image = ((System.Drawing.Image)(resources.GetObject("btnFacebookLike.Image")));
            this.btnFacebookLike.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFacebookLike.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFacebookLike.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.btnFacebookLike.Name = "btnFacebookLike";
            this.btnFacebookLike.Size = new System.Drawing.Size(25, 28);
            this.btnFacebookLike.ToolTipText = "Visit Facebook of ImageGlass";
            this.btnFacebookLike.Click += new System.EventHandler(this.btnFacebookLike_Click);
            // 
            // lblZoomRatio
            // 
            this.lblZoomRatio.BackColor = System.Drawing.Color.Transparent;
            this.lblZoomRatio.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblZoomRatio.Name = "lblZoomRatio";
            this.lblZoomRatio.Size = new System.Drawing.Size(0, 30);
            // 
            // lblImageType
            // 
            this.lblImageType.BackColor = System.Drawing.Color.Transparent;
            this.lblImageType.Margin = new System.Windows.Forms.Padding(0, 1, 5, 2);
            this.lblImageType.Name = "lblImageType";
            this.lblImageType.Size = new System.Drawing.Size(0, 30);
            // 
            // lblImageSize
            // 
            this.lblImageSize.BackColor = System.Drawing.Color.Transparent;
            this.lblImageSize.Margin = new System.Windows.Forms.Padding(0, 1, 5, 2);
            this.lblImageSize.Name = "lblImageSize";
            this.lblImageSize.Size = new System.Drawing.Size(0, 30);
            // 
            // lblImageFileSize
            // 
            this.lblImageFileSize.BackColor = System.Drawing.Color.Transparent;
            this.lblImageFileSize.Margin = new System.Windows.Forms.Padding(0, 1, 5, 2);
            this.lblImageFileSize.Name = "lblImageFileSize";
            this.lblImageFileSize.Size = new System.Drawing.Size(0, 30);
            // 
            // lblImageDateCreate
            // 
            this.lblImageDateCreate.BackColor = System.Drawing.Color.Transparent;
            this.lblImageDateCreate.Margin = new System.Windows.Forms.Padding(0, 1, 5, 2);
            this.lblImageDateCreate.Name = "lblImageDateCreate";
            this.lblImageDateCreate.Size = new System.Drawing.Size(0, 30);
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(864, 471);
            this.Controls.Add(this.spMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "frmMain";
            this.Text = "ImageGlass";
            this.Activated += new System.EventHandler(this.frmMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResizeEnd += new System.EventHandler(this.frmMain_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.sp0.Panel1.ResumeLayout(false);
            this.sp0.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).EndInit();
            this.sp0.ResumeLayout(false);
            this.mnuPopup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sysWatch)).EndInit();
            this.spMain.Panel1.ResumeLayout(false);
            this.spMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spMain)).EndInit();
            this.spMain.ResumeLayout(false);
            this.toolMain.ResumeLayout(false);
            this.toolMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolMain;
        private System.Windows.Forms.Timer timSlideShow;
        private System.Windows.Forms.ToolStripButton btnNext;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnRotateLeft;
        private System.Windows.Forms.ToolStripButton btnRotateRight;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnActualSize;
        private System.Windows.Forms.ToolStripButton btnScaletoWidth;
        private System.Windows.Forms.ToolStripButton btnScaletoHeight;
        private System.Windows.Forms.ToolStripButton btnWindowAutosize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripButton btnGoto;
        private System.Windows.Forms.ToolStripButton btnThumb;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnCaro;
        private System.Windows.Forms.ToolStripButton btnFullScreen;
        private System.Windows.Forms.ToolStripButton btnSlideShow;
        private System.Windows.Forms.ToolStripButton btnConvert;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnSetting;
        private System.Windows.Forms.ToolStripButton btnHelp;
        private System.Windows.Forms.ToolStripButton btnReport;
        private System.Windows.Forms.ToolStripButton btnFollow;
        private System.Windows.Forms.ToolStripButton btnFacebookLike;
        private System.Windows.Forms.ToolStripLabel lblZoomRatio;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ContextMenuStrip mnuPopup;
        private System.Windows.Forms.ToolStripMenuItem mnuEditWithPaint;
        private System.Windows.Forms.ToolStripMenuItem mnuExtractFrames;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuSetWallpaper;
        private System.Windows.Forms.ToolStripMenuItem mnuMoveRecycle;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuImageProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuStartSlideshow;
        private System.Windows.Forms.ToolStripMenuItem mnuStopSlideshow;
        private System.Windows.Forms.ToolStripMenuItem mnuExitSlideshow;
        private System.Windows.Forms.ToolStripSeparator mnuPhanCach;
        private ThumbBar.ThumbnailFlowLayoutPanel thumbBar;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.ToolStripMenuItem mnuRename;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyImagePath;
        private System.Windows.Forms.ToolStripLabel lblImageType;
        private System.Windows.Forms.ToolStripLabel lblImageDateCreate;
        private System.Windows.Forms.ToolStripLabel lblImageFileSize;
        private System.Windows.Forms.ToolStripLabel lblImageSize;
        private System.Windows.Forms.ToolStripButton btnPrintImage;
        private System.Windows.Forms.ToolStripButton btnFacebook;
        private System.Windows.Forms.ToolStripMenuItem mnuUploadFacebook;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripButton btnExtension;
        private System.Windows.Forms.ToolStripButton btnZoomLock;
        private System.IO.FileSystemWatcher sysWatch;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.SplitContainer spMain;
        private System.Windows.Forms.ToolStripMenuItem mnuShowToolBar;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private ImageBox picMain;
    }
}

