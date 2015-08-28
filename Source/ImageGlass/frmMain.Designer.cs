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
            this.mnuPopup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sampleMenuItemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timSlideShow = new System.Windows.Forms.Timer(this.components);
            this.tip1 = new System.Windows.Forms.ToolTip(this.components);
            this.sysWatch = new System.IO.FileSystemWatcher();
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
            this.btnCheckedBackground = new System.Windows.Forms.ToolStripButton();
            this.btnFullScreen = new System.Windows.Forms.ToolStripButton();
            this.btnSlideShow = new System.Windows.Forms.ToolStripButton();
            this.btnConvert = new System.Windows.Forms.ToolStripButton();
            this.btnPrintImage = new System.Windows.Forms.ToolStripButton();
            this.btnFacebook = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExtension = new System.Windows.Forms.ToolStripButton();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.btnHelp = new System.Windows.Forms.ToolStripButton();
            this.btnMenu = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuMainOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainOpenImageData = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainNavigation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainViewNext = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainViewPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainGoto = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainGotoFirst = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainGotoLast = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainFullScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSlideShow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSlideShowStart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSlideShowPause = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSlideShowExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainManipulation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainRotateCounterclockwise = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainRotateClockwise = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainLockZoomRatio = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem27 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainScaleToWidth = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainScaleToHeight = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainWindowAdaptImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainRename = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainMoveToRecycleBin = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainDeleteFromHardDisk = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainExtractFrames = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainStartStopAnimating = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSetAsDesktop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainImageLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainImageProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainCopyMulti = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainCutMulti = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem28 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainCopyImagePath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainClearClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainShare = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainShareFacebook = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainLayout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainToolbar = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainThumbnailBar = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainCheckBackground = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainTools = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainExtensionManager = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainReportIssue = new System.Windows.Forms.ToolStripMenuItem();
            this.lblInfo = new System.Windows.Forms.ToolStripLabel();
            this.sp0 = new System.Windows.Forms.SplitContainer();
            this.sp1 = new System.Windows.Forms.SplitContainer();
            this.picMain = new ImageGlass.ImageBox();
            this.thumbnailBar = new ImageGlass.ImageListView.ImageListView();
            this.mnuPopup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sysWatch)).BeginInit();
            this.toolMain.SuspendLayout();
            this.mnuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).BeginInit();
            this.sp0.Panel1.SuspendLayout();
            this.sp0.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).BeginInit();
            this.sp1.Panel1.SuspendLayout();
            this.sp1.Panel2.SuspendLayout();
            this.sp1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuPopup
            // 
            this.mnuPopup.BackColor = System.Drawing.Color.White;
            this.mnuPopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sampleMenuItemToolStripMenuItem});
            this.mnuPopup.Name = "mnuPopup";
            this.mnuPopup.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mnuPopup.Size = new System.Drawing.Size(174, 26);
            this.mnuPopup.Opening += new System.ComponentModel.CancelEventHandler(this.mnuPopup_Opening);
            // 
            // sampleMenuItemToolStripMenuItem
            // 
            this.sampleMenuItemToolStripMenuItem.Name = "sampleMenuItemToolStripMenuItem";
            this.sampleMenuItemToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.sampleMenuItemToolStripMenuItem.Text = "sample menu item";
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
            this.btnCheckedBackground,
            this.btnFullScreen,
            this.btnSlideShow,
            this.btnConvert,
            this.btnPrintImage,
            this.btnFacebook,
            this.toolStripSeparator4,
            this.btnExtension,
            this.btnSetting,
            this.btnHelp,
            this.btnMenu,
            this.lblInfo});
            this.toolMain.Location = new System.Drawing.Point(0, 0);
            this.toolMain.Name = "toolMain";
            this.toolMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolMain.Size = new System.Drawing.Size(836, 33);
            this.toolMain.TabIndex = 1;
            // 
            // btnBack
            // 
            this.btnBack.AutoSize = false;
            this.btnBack.BackColor = System.Drawing.Color.Transparent;
            this.btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnBack.Image = global::ImageGlass.Properties.Resources.back;
            this.btnBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnBack.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(28, 28);
            this.btnBack.ToolTipText = "Go to previous image (Left arrow / PageDown)";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnNext
            // 
            this.btnNext.AutoSize = false;
            this.btnNext.BackColor = System.Drawing.Color.Transparent;
            this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNext.Image = global::ImageGlass.Properties.Resources.next;
            this.btnNext.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNext.Margin = new System.Windows.Forms.Padding(0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(28, 28);
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
            this.btnRotateLeft.Size = new System.Drawing.Size(28, 28);
            this.btnRotateLeft.ToolTipText = "Rotate Counterclockwise (Ctrl + ,)";
            this.btnRotateLeft.Click += new System.EventHandler(this.btnRotateLeft_Click);
            // 
            // btnRotateRight
            // 
            this.btnRotateRight.AutoSize = false;
            this.btnRotateRight.BackColor = System.Drawing.Color.Transparent;
            this.btnRotateRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRotateRight.Image = global::ImageGlass.Properties.Resources.rightrotate;
            this.btnRotateRight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRotateRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRotateRight.Margin = new System.Windows.Forms.Padding(0);
            this.btnRotateRight.Name = "btnRotateRight";
            this.btnRotateRight.Size = new System.Drawing.Size(28, 28);
            this.btnRotateRight.Text = "Next";
            this.btnRotateRight.ToolTipText = "Rotate Clockwise (Ctrl + .)";
            this.btnRotateRight.Click += new System.EventHandler(this.btnRotateRight_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.AutoSize = false;
            this.btnZoomIn.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomIn.Image = global::ImageGlass.Properties.Resources.zoomin;
            this.btnZoomIn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(0);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(28, 28);
            this.btnZoomIn.Tag = "0";
            this.btnZoomIn.ToolTipText = "Zoom in (Ctrl + =)";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.AutoSize = false;
            this.btnZoomOut.BackColor = System.Drawing.Color.Transparent;
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = global::ImageGlass.Properties.Resources.zoomout;
            this.btnZoomOut.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(0);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(28, 28);
            this.btnZoomOut.ToolTipText = "Zoom out (Ctrl + -)";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnActualSize
            // 
            this.btnActualSize.AutoSize = false;
            this.btnActualSize.BackColor = System.Drawing.Color.Transparent;
            this.btnActualSize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnActualSize.Image = global::ImageGlass.Properties.Resources.scaletofit;
            this.btnActualSize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnActualSize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnActualSize.Margin = new System.Windows.Forms.Padding(0);
            this.btnActualSize.Name = "btnActualSize";
            this.btnActualSize.Size = new System.Drawing.Size(28, 28);
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
            this.btnZoomLock.Size = new System.Drawing.Size(28, 28);
            this.btnZoomLock.Tag = "";
            this.btnZoomLock.ToolTipText = "Lock zoom ratio";
            this.btnZoomLock.Click += new System.EventHandler(this.btnZoomLock_Click);
            // 
            // btnScaletoWidth
            // 
            this.btnScaletoWidth.AutoSize = false;
            this.btnScaletoWidth.BackColor = System.Drawing.Color.Transparent;
            this.btnScaletoWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScaletoWidth.Image = global::ImageGlass.Properties.Resources.scaletowidth;
            this.btnScaletoWidth.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnScaletoWidth.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScaletoWidth.Margin = new System.Windows.Forms.Padding(0);
            this.btnScaletoWidth.Name = "btnScaletoWidth";
            this.btnScaletoWidth.Size = new System.Drawing.Size(28, 28);
            this.btnScaletoWidth.ToolTipText = "Scale to Width (Ctrl + W)";
            this.btnScaletoWidth.Click += new System.EventHandler(this.btnScaletoWidth_Click);
            // 
            // btnScaletoHeight
            // 
            this.btnScaletoHeight.AutoSize = false;
            this.btnScaletoHeight.BackColor = System.Drawing.Color.Transparent;
            this.btnScaletoHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnScaletoHeight.Image = global::ImageGlass.Properties.Resources.scaletoheight;
            this.btnScaletoHeight.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnScaletoHeight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnScaletoHeight.Margin = new System.Windows.Forms.Padding(0);
            this.btnScaletoHeight.Name = "btnScaletoHeight";
            this.btnScaletoHeight.Size = new System.Drawing.Size(28, 28);
            this.btnScaletoHeight.ToolTipText = "Scale to Height (Ctrl + H)";
            this.btnScaletoHeight.Click += new System.EventHandler(this.btnScaletoHeight_Click);
            // 
            // btnWindowAutosize
            // 
            this.btnWindowAutosize.AutoSize = false;
            this.btnWindowAutosize.BackColor = System.Drawing.Color.Transparent;
            this.btnWindowAutosize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWindowAutosize.Image = global::ImageGlass.Properties.Resources.autosizewindow;
            this.btnWindowAutosize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnWindowAutosize.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWindowAutosize.Margin = new System.Windows.Forms.Padding(0);
            this.btnWindowAutosize.Name = "btnWindowAutosize";
            this.btnWindowAutosize.Size = new System.Drawing.Size(28, 28);
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
            this.btnOpen.Image = global::ImageGlass.Properties.Resources.open;
            this.btnOpen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Margin = new System.Windows.Forms.Padding(0);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(28, 28);
            this.btnOpen.ToolTipText = "Open file (Ctrl + O)";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.AutoSize = false;
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = global::ImageGlass.Properties.Resources.refresh;
            this.btnRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(28, 28);
            this.btnRefresh.ToolTipText = "Refresh (F5)";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnGoto
            // 
            this.btnGoto.AutoSize = false;
            this.btnGoto.BackColor = System.Drawing.Color.Transparent;
            this.btnGoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnGoto.Image = global::ImageGlass.Properties.Resources.gotoimage;
            this.btnGoto.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnGoto.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnGoto.Margin = new System.Windows.Forms.Padding(0);
            this.btnGoto.Name = "btnGoto";
            this.btnGoto.Size = new System.Drawing.Size(28, 28);
            this.btnGoto.ToolTipText = "Go to ... (Ctrl + G)";
            this.btnGoto.Click += new System.EventHandler(this.btnGoto_Click);
            // 
            // btnThumb
            // 
            this.btnThumb.AutoSize = false;
            this.btnThumb.BackColor = System.Drawing.Color.Transparent;
            this.btnThumb.CheckOnClick = true;
            this.btnThumb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnThumb.Image = global::ImageGlass.Properties.Resources.thumbnail;
            this.btnThumb.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnThumb.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnThumb.Margin = new System.Windows.Forms.Padding(0);
            this.btnThumb.Name = "btnThumb";
            this.btnThumb.Size = new System.Drawing.Size(28, 28);
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
            // btnCheckedBackground
            // 
            this.btnCheckedBackground.AutoSize = false;
            this.btnCheckedBackground.BackColor = System.Drawing.Color.Transparent;
            this.btnCheckedBackground.CheckOnClick = true;
            this.btnCheckedBackground.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCheckedBackground.Image = global::ImageGlass.Properties.Resources.background;
            this.btnCheckedBackground.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnCheckedBackground.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCheckedBackground.Margin = new System.Windows.Forms.Padding(0);
            this.btnCheckedBackground.Name = "btnCheckedBackground";
            this.btnCheckedBackground.Size = new System.Drawing.Size(28, 28);
            this.btnCheckedBackground.ToolTipText = "Show checked background (Ctrl + B)";
            this.btnCheckedBackground.Click += new System.EventHandler(this.btnCheckedBackground_Click);
            // 
            // btnFullScreen
            // 
            this.btnFullScreen.AutoSize = false;
            this.btnFullScreen.BackColor = System.Drawing.Color.Transparent;
            this.btnFullScreen.CheckOnClick = true;
            this.btnFullScreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFullScreen.Image = global::ImageGlass.Properties.Resources.fullscreen;
            this.btnFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnFullScreen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFullScreen.Margin = new System.Windows.Forms.Padding(0);
            this.btnFullScreen.Name = "btnFullScreen";
            this.btnFullScreen.Size = new System.Drawing.Size(28, 28);
            this.btnFullScreen.ToolTipText = "Full sreen (Alt + Enter)";
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);
            // 
            // btnSlideShow
            // 
            this.btnSlideShow.AutoSize = false;
            this.btnSlideShow.BackColor = System.Drawing.Color.Transparent;
            this.btnSlideShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSlideShow.Image = global::ImageGlass.Properties.Resources.slideshow;
            this.btnSlideShow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSlideShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSlideShow.Margin = new System.Windows.Forms.Padding(0);
            this.btnSlideShow.Name = "btnSlideShow";
            this.btnSlideShow.Size = new System.Drawing.Size(28, 28);
            this.btnSlideShow.ToolTipText = "Play slideshow (F11, ESC to exit)";
            this.btnSlideShow.Click += new System.EventHandler(this.btnSlideShow_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.AutoSize = false;
            this.btnConvert.BackColor = System.Drawing.Color.Transparent;
            this.btnConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConvert.Image = global::ImageGlass.Properties.Resources.convert;
            this.btnConvert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConvert.Margin = new System.Windows.Forms.Padding(0);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(28, 28);
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
            this.btnPrintImage.Size = new System.Drawing.Size(28, 28);
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
            this.btnFacebook.Size = new System.Drawing.Size(28, 28);
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
            this.btnExtension.Size = new System.Drawing.Size(28, 28);
            this.btnExtension.ToolTipText = "Extension Manager (Ctrl + Shift + E)";
            this.btnExtension.Click += new System.EventHandler(this.btnExtension_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.AutoSize = false;
            this.btnSetting.BackColor = System.Drawing.Color.Transparent;
            this.btnSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetting.Image = global::ImageGlass.Properties.Resources.settings;
            this.btnSetting.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(28, 28);
            this.btnSetting.ToolTipText = "ImageGlass Settings (Ctrl + Shift + P)";
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.AutoSize = false;
            this.btnHelp.BackColor = System.Drawing.Color.Transparent;
            this.btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHelp.Image = global::ImageGlass.Properties.Resources.about;
            this.btnHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHelp.Margin = new System.Windows.Forms.Padding(0);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(28, 28);
            this.btnHelp.ToolTipText = "Help (F1)";
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnMenu
            // 
            this.btnMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnMenu.AutoSize = false;
            this.btnMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMenu.DropDown = this.mnuMain;
            this.btnMenu.Image = ((System.Drawing.Image)(resources.GetObject("btnMenu.Image")));
            this.btnMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMenu.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.ShowDropDownArrow = false;
            this.btnMenu.Size = new System.Drawing.Size(28, 28);
            this.btnMenu.Text = "Menu (Hotkey: `)";
            // 
            // mnuMain
            // 
            this.mnuMain.BackColor = System.Drawing.Color.White;
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainOpenFile,
            this.mnuMainOpenImageData,
            this.mnuMainSaveAs,
            this.mnuMainEditImage,
            this.mnuMainRefresh,
            this.toolStripSeparator6,
            this.mnuMainNavigation,
            this.mnuMainFullScreen,
            this.mnuMainSlideShow,
            this.mnuMainPrint,
            this.toolStripSeparator5,
            this.mnuMainManipulation,
            this.mnuMainClipboard,
            this.mnuMainShare,
            this.toolStripSeparator9,
            this.mnuMainLayout,
            this.mnuMainTools,
            this.mnuMainSettings,
            this.mnuMainAbout,
            this.toolStripMenuItem21,
            this.mnuMainReportIssue});
            this.mnuMain.Name = "mnuPopup";
            this.mnuMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.mnuMain.Size = new System.Drawing.Size(289, 418);
            this.mnuMain.Opening += new System.ComponentModel.CancelEventHandler(this.mnuMain_Opening);
            // 
            // mnuMainOpenFile
            // 
            this.mnuMainOpenFile.ForeColor = System.Drawing.Color.Black;
            this.mnuMainOpenFile.Name = "mnuMainOpenFile";
            this.mnuMainOpenFile.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainOpenFile.ShortcutKeyDisplayString = "";
            this.mnuMainOpenFile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuMainOpenFile.Size = new System.Drawing.Size(288, 23);
            this.mnuMainOpenFile.Text = "&Open file";
            this.mnuMainOpenFile.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.mnuMainOpenFile.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.mnuMainOpenFile.Click += new System.EventHandler(this.mnuMainOpenFile_Click);
            // 
            // mnuMainOpenImageData
            // 
            this.mnuMainOpenImageData.ForeColor = System.Drawing.Color.Black;
            this.mnuMainOpenImageData.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainOpenImageData.Image")));
            this.mnuMainOpenImageData.Name = "mnuMainOpenImageData";
            this.mnuMainOpenImageData.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainOpenImageData.ShortcutKeyDisplayString = "";
            this.mnuMainOpenImageData.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.mnuMainOpenImageData.Size = new System.Drawing.Size(288, 23);
            this.mnuMainOpenImageData.Text = "Open image &data from clipboard";
            this.mnuMainOpenImageData.Click += new System.EventHandler(this.mnuMainOpenImageData_Click);
            // 
            // mnuMainSaveAs
            // 
            this.mnuMainSaveAs.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSaveAs.Name = "mnuMainSaveAs";
            this.mnuMainSaveAs.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSaveAs.ShortcutKeyDisplayString = "";
            this.mnuMainSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuMainSaveAs.Size = new System.Drawing.Size(288, 23);
            this.mnuMainSaveAs.Text = "&Save image as ...";
            this.mnuMainSaveAs.Click += new System.EventHandler(this.mnuMainSaveAs_Click);
            // 
            // mnuMainRefresh
            // 
            this.mnuMainRefresh.ForeColor = System.Drawing.Color.Black;
            this.mnuMainRefresh.Name = "mnuMainRefresh";
            this.mnuMainRefresh.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainRefresh.ShortcutKeyDisplayString = "";
            this.mnuMainRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuMainRefresh.Size = new System.Drawing.Size(288, 23);
            this.mnuMainRefresh.Text = "&Refresh";
            this.mnuMainRefresh.Click += new System.EventHandler(this.mnuMainRefresh_Click);
            // 
            // mnuMainEditImage
            // 
            this.mnuMainEditImage.ForeColor = System.Drawing.Color.Black;
            this.mnuMainEditImage.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainEditImage.Image")));
            this.mnuMainEditImage.Name = "mnuMainEditImage";
            this.mnuMainEditImage.Size = new System.Drawing.Size(288, 22);
            this.mnuMainEditImage.Text = "&Edit image";
            this.mnuMainEditImage.Click += new System.EventHandler(this.mnuMainEditImage_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(285, 6);
            // 
            // mnuMainNavigation
            // 
            this.mnuMainNavigation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainViewNext,
            this.mnuMainViewPrevious,
            this.toolStripMenuItem24,
            this.mnuMainGoto,
            this.mnuMainGotoLast,
            this.mnuMainGotoFirst});
            this.mnuMainNavigation.ForeColor = System.Drawing.Color.Black;
            this.mnuMainNavigation.Name = "mnuMainNavigation";
            this.mnuMainNavigation.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainNavigation.Size = new System.Drawing.Size(288, 23);
            this.mnuMainNavigation.Text = "&Navigation";
            // 
            // mnuMainViewNext
            // 
            this.mnuMainViewNext.ForeColor = System.Drawing.Color.Black;
            this.mnuMainViewNext.Name = "mnuMainViewNext";
            this.mnuMainViewNext.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainViewNext.ShortcutKeyDisplayString = "Next / PageUp";
            this.mnuMainViewNext.Size = new System.Drawing.Size(283, 23);
            this.mnuMainViewNext.Text = "View &next image";
            this.mnuMainViewNext.Click += new System.EventHandler(this.mnuMainViewNext_Click);
            // 
            // mnuMainViewPrevious
            // 
            this.mnuMainViewPrevious.ForeColor = System.Drawing.Color.Black;
            this.mnuMainViewPrevious.Name = "mnuMainViewPrevious";
            this.mnuMainViewPrevious.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainViewPrevious.ShortcutKeyDisplayString = "Back / PageDown";
            this.mnuMainViewPrevious.Size = new System.Drawing.Size(283, 23);
            this.mnuMainViewPrevious.Text = "View &previous image";
            this.mnuMainViewPrevious.Click += new System.EventHandler(this.mnuMainViewPrevious_Click);
            // 
            // toolStripMenuItem24
            // 
            this.toolStripMenuItem24.Name = "toolStripMenuItem24";
            this.toolStripMenuItem24.Size = new System.Drawing.Size(280, 6);
            // 
            // mnuMainGoto
            // 
            this.mnuMainGoto.ForeColor = System.Drawing.Color.Black;
            this.mnuMainGoto.Name = "mnuMainGoto";
            this.mnuMainGoto.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainGoto.ShortcutKeyDisplayString = "";
            this.mnuMainGoto.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.mnuMainGoto.Size = new System.Drawing.Size(283, 23);
            this.mnuMainGoto.Text = "&Go to ...";
            this.mnuMainGoto.Click += new System.EventHandler(this.mnuMainGoto_Click);
            // 
            // mnuMainGotoFirst
            // 
            this.mnuMainGotoFirst.ForeColor = System.Drawing.Color.Black;
            this.mnuMainGotoFirst.Name = "mnuMainGotoFirst";
            this.mnuMainGotoFirst.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainGotoFirst.ShortcutKeyDisplayString = "Home";
            this.mnuMainGotoFirst.Size = new System.Drawing.Size(283, 23);
            this.mnuMainGotoFirst.Text = "Go to the &first image";
            this.mnuMainGotoFirst.Click += new System.EventHandler(this.mnuMainGotoFirst_Click);
            // 
            // mnuMainGotoLast
            // 
            this.mnuMainGotoLast.ForeColor = System.Drawing.Color.Black;
            this.mnuMainGotoLast.Name = "mnuMainGotoLast";
            this.mnuMainGotoLast.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainGotoLast.ShortcutKeyDisplayString = "End";
            this.mnuMainGotoLast.Size = new System.Drawing.Size(283, 23);
            this.mnuMainGotoLast.Text = "Go to the &last image";
            this.mnuMainGotoLast.Click += new System.EventHandler(this.mnuMainGotoLast_Click);
            // 
            // mnuMainFullScreen
            // 
            this.mnuMainFullScreen.ForeColor = System.Drawing.Color.Black;
            this.mnuMainFullScreen.Name = "mnuMainFullScreen";
            this.mnuMainFullScreen.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainFullScreen.ShortcutKeyDisplayString = "Alt+Enter";
            this.mnuMainFullScreen.Size = new System.Drawing.Size(288, 23);
            this.mnuMainFullScreen.Text = "&Full screen";
            this.mnuMainFullScreen.Click += new System.EventHandler(this.mnuMainFullScreen_Click);
            // 
            // mnuMainSlideShow
            // 
            this.mnuMainSlideShow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainSlideShowStart,
            this.mnuMainSlideShowPause,
            this.mnuMainSlideShowExit});
            this.mnuMainSlideShow.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSlideShow.Name = "mnuMainSlideShow";
            this.mnuMainSlideShow.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSlideShow.ShortcutKeyDisplayString = "";
            this.mnuMainSlideShow.Size = new System.Drawing.Size(288, 23);
            this.mnuMainSlideShow.Text = "Sl&ideshow";
            // 
            // mnuMainSlideShowStart
            // 
            this.mnuMainSlideShowStart.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSlideShowStart.Name = "mnuMainSlideShowStart";
            this.mnuMainSlideShowStart.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSlideShowStart.ShortcutKeyDisplayString = "";
            this.mnuMainSlideShowStart.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.mnuMainSlideShowStart.Size = new System.Drawing.Size(251, 23);
            this.mnuMainSlideShowStart.Text = "&Start slideshow";
            this.mnuMainSlideShowStart.Click += new System.EventHandler(this.mnuMainSlideShowStart_Click);
            // 
            // mnuMainSlideShowPause
            // 
            this.mnuMainSlideShowPause.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSlideShowPause.Name = "mnuMainSlideShowPause";
            this.mnuMainSlideShowPause.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSlideShowPause.ShortcutKeyDisplayString = "Space";
            this.mnuMainSlideShowPause.Size = new System.Drawing.Size(251, 23);
            this.mnuMainSlideShowPause.Text = "&Pause / Resume slideshow";
            this.mnuMainSlideShowPause.Click += new System.EventHandler(this.mnuMainSlideShowPause_Click);
            // 
            // mnuMainSlideShowExit
            // 
            this.mnuMainSlideShowExit.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSlideShowExit.Name = "mnuMainSlideShowExit";
            this.mnuMainSlideShowExit.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSlideShowExit.ShortcutKeyDisplayString = "ESC";
            this.mnuMainSlideShowExit.Size = new System.Drawing.Size(251, 23);
            this.mnuMainSlideShowExit.Text = "E&xit slideshow";
            this.mnuMainSlideShowExit.Click += new System.EventHandler(this.mnuMainSlideShowExit_Click);
            // 
            // mnuMainPrint
            // 
            this.mnuMainPrint.ForeColor = System.Drawing.Color.Black;
            this.mnuMainPrint.Name = "mnuMainPrint";
            this.mnuMainPrint.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainPrint.ShortcutKeyDisplayString = "";
            this.mnuMainPrint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.mnuMainPrint.Size = new System.Drawing.Size(288, 23);
            this.mnuMainPrint.Text = "&Print";
            this.mnuMainPrint.Click += new System.EventHandler(this.mnuMainPrint_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(285, 6);
            // 
            // mnuMainManipulation
            // 
            this.mnuMainManipulation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainRotateCounterclockwise,
            this.mnuMainRotateClockwise,
            this.toolStripMenuItem6,
            this.mnuMainZoomIn,
            this.mnuMainZoomOut,
            this.mnuMainActualSize,
            this.mnuMainLockZoomRatio,
            this.toolStripMenuItem27,
            this.mnuMainScaleToWidth,
            this.mnuMainScaleToHeight,
            this.mnuMainWindowAdaptImage,
            this.toolStripMenuItem15,
            this.mnuMainRename,
            this.mnuMainMoveToRecycleBin,
            this.mnuMainDeleteFromHardDisk,
            this.toolStripMenuItem13,
            this.mnuMainExtractFrames,
            this.mnuMainStartStopAnimating,
            this.mnuMainSetAsDesktop,
            this.mnuMainImageLocation,
            this.mnuMainImageProperties});
            this.mnuMainManipulation.ForeColor = System.Drawing.Color.Black;
            this.mnuMainManipulation.Name = "mnuMainManipulation";
            this.mnuMainManipulation.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainManipulation.Size = new System.Drawing.Size(288, 23);
            this.mnuMainManipulation.Text = "&Manipulation";
            // 
            // mnuMainRotateCounterclockwise
            // 
            this.mnuMainRotateCounterclockwise.ForeColor = System.Drawing.Color.Black;
            this.mnuMainRotateCounterclockwise.Name = "mnuMainRotateCounterclockwise";
            this.mnuMainRotateCounterclockwise.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainRotateCounterclockwise.ShortcutKeyDisplayString = "Ctrl+,";
            this.mnuMainRotateCounterclockwise.Size = new System.Drawing.Size(291, 23);
            this.mnuMainRotateCounterclockwise.Text = "&Rotate Counterclockwise";
            this.mnuMainRotateCounterclockwise.Click += new System.EventHandler(this.mnuMainRotateCounterclockwise_Click);
            // 
            // mnuMainRotateClockwise
            // 
            this.mnuMainRotateClockwise.ForeColor = System.Drawing.Color.Black;
            this.mnuMainRotateClockwise.Name = "mnuMainRotateClockwise";
            this.mnuMainRotateClockwise.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainRotateClockwise.ShortcutKeyDisplayString = "Ctrl+.";
            this.mnuMainRotateClockwise.Size = new System.Drawing.Size(291, 23);
            this.mnuMainRotateClockwise.Text = "R&otate Clockwise";
            this.mnuMainRotateClockwise.Click += new System.EventHandler(this.mnuMainRotateClockwise_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(288, 6);
            // 
            // mnuMainZoomIn
            // 
            this.mnuMainZoomIn.ForeColor = System.Drawing.Color.Black;
            this.mnuMainZoomIn.Name = "mnuMainZoomIn";
            this.mnuMainZoomIn.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainZoomIn.ShortcutKeyDisplayString = "Ctrl+=";
            this.mnuMainZoomIn.Size = new System.Drawing.Size(291, 23);
            this.mnuMainZoomIn.Text = "&Zoom in";
            this.mnuMainZoomIn.Click += new System.EventHandler(this.mnuMainZoomIn_Click);
            // 
            // mnuMainZoomOut
            // 
            this.mnuMainZoomOut.ForeColor = System.Drawing.Color.Black;
            this.mnuMainZoomOut.Name = "mnuMainZoomOut";
            this.mnuMainZoomOut.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainZoomOut.ShortcutKeyDisplayString = "Ctrl+-";
            this.mnuMainZoomOut.Size = new System.Drawing.Size(291, 23);
            this.mnuMainZoomOut.Text = "Zoo&m out";
            this.mnuMainZoomOut.Click += new System.EventHandler(this.mnuMainZoomOut_Click);
            // 
            // mnuMainActualSize
            // 
            this.mnuMainActualSize.ForeColor = System.Drawing.Color.Black;
            this.mnuMainActualSize.Name = "mnuMainActualSize";
            this.mnuMainActualSize.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainActualSize.ShortcutKeyDisplayString = "Ctrl+0";
            this.mnuMainActualSize.Size = new System.Drawing.Size(291, 23);
            this.mnuMainActualSize.Text = "&Actual size";
            this.mnuMainActualSize.Click += new System.EventHandler(this.mnuMainActualSize_Click);
            // 
            // mnuMainLockZoomRatio
            // 
            this.mnuMainLockZoomRatio.ForeColor = System.Drawing.Color.Black;
            this.mnuMainLockZoomRatio.Name = "mnuMainLockZoomRatio";
            this.mnuMainLockZoomRatio.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainLockZoomRatio.ShortcutKeyDisplayString = "";
            this.mnuMainLockZoomRatio.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.mnuMainLockZoomRatio.Size = new System.Drawing.Size(291, 23);
            this.mnuMainLockZoomRatio.Text = "&Lock zoom ratio";
            this.mnuMainLockZoomRatio.Click += new System.EventHandler(this.mnuMainLockZoomRatio_Click);
            // 
            // toolStripMenuItem27
            // 
            this.toolStripMenuItem27.Name = "toolStripMenuItem27";
            this.toolStripMenuItem27.Size = new System.Drawing.Size(288, 6);
            // 
            // mnuMainScaleToWidth
            // 
            this.mnuMainScaleToWidth.ForeColor = System.Drawing.Color.Black;
            this.mnuMainScaleToWidth.Name = "mnuMainScaleToWidth";
            this.mnuMainScaleToWidth.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainScaleToWidth.ShortcutKeyDisplayString = "";
            this.mnuMainScaleToWidth.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.mnuMainScaleToWidth.Size = new System.Drawing.Size(291, 23);
            this.mnuMainScaleToWidth.Text = "Scale to &Width";
            this.mnuMainScaleToWidth.Click += new System.EventHandler(this.mnuMainScaleToWidth_Click);
            // 
            // mnuMainScaleToHeight
            // 
            this.mnuMainScaleToHeight.ForeColor = System.Drawing.Color.Black;
            this.mnuMainScaleToHeight.Name = "mnuMainScaleToHeight";
            this.mnuMainScaleToHeight.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainScaleToHeight.ShortcutKeyDisplayString = "";
            this.mnuMainScaleToHeight.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.mnuMainScaleToHeight.Size = new System.Drawing.Size(291, 23);
            this.mnuMainScaleToHeight.Text = "Scale to &Height";
            this.mnuMainScaleToHeight.Click += new System.EventHandler(this.mnuMainScaleToHeight_Click);
            // 
            // mnuMainWindowAdaptImage
            // 
            this.mnuMainWindowAdaptImage.ForeColor = System.Drawing.Color.Black;
            this.mnuMainWindowAdaptImage.Name = "mnuMainWindowAdaptImage";
            this.mnuMainWindowAdaptImage.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainWindowAdaptImage.ShortcutKeyDisplayString = "";
            this.mnuMainWindowAdaptImage.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.mnuMainWindowAdaptImage.Size = new System.Drawing.Size(291, 23);
            this.mnuMainWindowAdaptImage.Text = "&Window adapt to image";
            this.mnuMainWindowAdaptImage.Click += new System.EventHandler(this.mnuMainWindowAdaptImage_Click);
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(288, 6);
            // 
            // mnuMainRename
            // 
            this.mnuMainRename.ForeColor = System.Drawing.Color.Black;
            this.mnuMainRename.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainRename.Image")));
            this.mnuMainRename.Name = "mnuMainRename";
            this.mnuMainRename.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainRename.ShortcutKeyDisplayString = "";
            this.mnuMainRename.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.mnuMainRename.Size = new System.Drawing.Size(291, 23);
            this.mnuMainRename.Text = "Re&name image";
            this.mnuMainRename.Click += new System.EventHandler(this.mnuMainRename_Click);
            // 
            // mnuMainMoveToRecycleBin
            // 
            this.mnuMainMoveToRecycleBin.ForeColor = System.Drawing.Color.Black;
            this.mnuMainMoveToRecycleBin.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainMoveToRecycleBin.Image")));
            this.mnuMainMoveToRecycleBin.Name = "mnuMainMoveToRecycleBin";
            this.mnuMainMoveToRecycleBin.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainMoveToRecycleBin.ShortcutKeyDisplayString = "";
            this.mnuMainMoveToRecycleBin.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.mnuMainMoveToRecycleBin.Size = new System.Drawing.Size(291, 23);
            this.mnuMainMoveToRecycleBin.Text = "&Move to recycle bin";
            this.mnuMainMoveToRecycleBin.Click += new System.EventHandler(this.mnuMainMoveToRecycleBin_Click);
            // 
            // mnuMainDeleteFromHardDisk
            // 
            this.mnuMainDeleteFromHardDisk.ForeColor = System.Drawing.Color.Black;
            this.mnuMainDeleteFromHardDisk.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainDeleteFromHardDisk.Image")));
            this.mnuMainDeleteFromHardDisk.Name = "mnuMainDeleteFromHardDisk";
            this.mnuMainDeleteFromHardDisk.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainDeleteFromHardDisk.ShortcutKeyDisplayString = "";
            this.mnuMainDeleteFromHardDisk.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete)));
            this.mnuMainDeleteFromHardDisk.Size = new System.Drawing.Size(291, 23);
            this.mnuMainDeleteFromHardDisk.Text = "&Delete from hard disk";
            this.mnuMainDeleteFromHardDisk.Click += new System.EventHandler(this.mnuMainDeleteFromHardDisk_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(288, 6);
            // 
            // mnuMainExtractFrames
            // 
            this.mnuMainExtractFrames.ForeColor = System.Drawing.Color.Black;
            this.mnuMainExtractFrames.Name = "mnuMainExtractFrames";
            this.mnuMainExtractFrames.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainExtractFrames.ShortcutKeyDisplayString = "";
            this.mnuMainExtractFrames.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.mnuMainExtractFrames.Size = new System.Drawing.Size(291, 23);
            this.mnuMainExtractFrames.Text = "&Extract image frames";
            this.mnuMainExtractFrames.Click += new System.EventHandler(this.mnuMainExtractFrames_Click);
            // 
            // mnuMainStartStopAnimating
            // 
            this.mnuMainStartStopAnimating.ForeColor = System.Drawing.Color.Black;
            this.mnuMainStartStopAnimating.Name = "mnuMainStartStopAnimating";
            this.mnuMainStartStopAnimating.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainStartStopAnimating.ShortcutKeyDisplayString = "";
            this.mnuMainStartStopAnimating.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
            this.mnuMainStartStopAnimating.Size = new System.Drawing.Size(291, 23);
            this.mnuMainStartStopAnimating.Text = "Start / Stop &animating image";
            this.mnuMainStartStopAnimating.Click += new System.EventHandler(this.mnuMainStartStopAnimating_Click);
            // 
            // mnuMainSetAsDesktop
            // 
            this.mnuMainSetAsDesktop.Name = "mnuMainSetAsDesktop";
            this.mnuMainSetAsDesktop.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSetAsDesktop.Size = new System.Drawing.Size(291, 23);
            this.mnuMainSetAsDesktop.Text = "&Set as desktop background";
            this.mnuMainSetAsDesktop.Click += new System.EventHandler(this.mnuMainSetAsDesktop_Click);
            // 
            // mnuMainImageLocation
            // 
            this.mnuMainImageLocation.Name = "mnuMainImageLocation";
            this.mnuMainImageLocation.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainImageLocation.ShortcutKeyDisplayString = "";
            this.mnuMainImageLocation.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.L)));
            this.mnuMainImageLocation.Size = new System.Drawing.Size(291, 23);
            this.mnuMainImageLocation.Text = "Open image &location";
            this.mnuMainImageLocation.Click += new System.EventHandler(this.mnuMainImageLocation_Click);
            // 
            // mnuMainImageProperties
            // 
            this.mnuMainImageProperties.Name = "mnuMainImageProperties";
            this.mnuMainImageProperties.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainImageProperties.ShortcutKeyDisplayString = "";
            this.mnuMainImageProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.mnuMainImageProperties.Size = new System.Drawing.Size(291, 23);
            this.mnuMainImageProperties.Text = "Ima&ge properties";
            this.mnuMainImageProperties.Click += new System.EventHandler(this.mnuMainImageProperties_Click);
            // 
            // mnuMainClipboard
            // 
            this.mnuMainClipboard.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainCopy,
            this.mnuMainCopyMulti,
            this.mnuMainCut,
            this.mnuMainCutMulti,
            this.toolStripMenuItem28,
            this.mnuMainCopyImagePath,
            this.toolStripMenuItem14,
            this.mnuMainClearClipboard});
            this.mnuMainClipboard.ForeColor = System.Drawing.Color.Black;
            this.mnuMainClipboard.Name = "mnuMainClipboard";
            this.mnuMainClipboard.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainClipboard.Size = new System.Drawing.Size(288, 23);
            this.mnuMainClipboard.Text = "&Clipboard";
            // 
            // mnuMainCopy
            // 
            this.mnuMainCopy.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCopy.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainCopy.Image")));
            this.mnuMainCopy.Name = "mnuMainCopy";
            this.mnuMainCopy.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCopy.ShortcutKeyDisplayString = "";
            this.mnuMainCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuMainCopy.Size = new System.Drawing.Size(247, 23);
            this.mnuMainCopy.Text = "&Copy";
            this.mnuMainCopy.Click += new System.EventHandler(this.mnuMainCopy_Click);
            // 
            // mnuMainCopyMulti
            // 
            this.mnuMainCopyMulti.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCopyMulti.Name = "mnuMainCopyMulti";
            this.mnuMainCopyMulti.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCopyMulti.ShortcutKeyDisplayString = "";
            this.mnuMainCopyMulti.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.mnuMainCopyMulti.Size = new System.Drawing.Size(247, 23);
            this.mnuMainCopyMulti.Text = "Copy &multiple files";
            this.mnuMainCopyMulti.Click += new System.EventHandler(this.mnuMainCopyMulti_Click);
            // 
            // mnuMainCut
            // 
            this.mnuMainCut.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCut.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainCut.Image")));
            this.mnuMainCut.Name = "mnuMainCut";
            this.mnuMainCut.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCut.ShortcutKeyDisplayString = "";
            this.mnuMainCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.mnuMainCut.Size = new System.Drawing.Size(247, 23);
            this.mnuMainCut.Text = "Cu&t";
            this.mnuMainCut.Click += new System.EventHandler(this.mnuMainCut_Click);
            // 
            // mnuMainCutMulti
            // 
            this.mnuMainCutMulti.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCutMulti.Name = "mnuMainCutMulti";
            this.mnuMainCutMulti.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCutMulti.ShortcutKeyDisplayString = "";
            this.mnuMainCutMulti.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.X)));
            this.mnuMainCutMulti.Size = new System.Drawing.Size(247, 23);
            this.mnuMainCutMulti.Text = "C&ut multifile files";
            this.mnuMainCutMulti.Click += new System.EventHandler(this.mnuMainCutMulti_Click);
            // 
            // toolStripMenuItem28
            // 
            this.toolStripMenuItem28.Name = "toolStripMenuItem28";
            this.toolStripMenuItem28.Size = new System.Drawing.Size(244, 6);
            // 
            // mnuMainCopyImagePath
            // 
            this.mnuMainCopyImagePath.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCopyImagePath.Name = "mnuMainCopyImagePath";
            this.mnuMainCopyImagePath.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCopyImagePath.Size = new System.Drawing.Size(247, 23);
            this.mnuMainCopyImagePath.Text = "Copy image path";
            this.mnuMainCopyImagePath.Click += new System.EventHandler(this.mnuMainCopyImagePath_Click);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(244, 6);
            // 
            // mnuMainClearClipboard
            // 
            this.mnuMainClearClipboard.ForeColor = System.Drawing.Color.Black;
            this.mnuMainClearClipboard.Name = "mnuMainClearClipboard";
            this.mnuMainClearClipboard.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainClearClipboard.ShortcutKeyDisplayString = "Ctrl+`";
            this.mnuMainClearClipboard.Size = new System.Drawing.Size(247, 23);
            this.mnuMainClearClipboard.Text = "Clear clipboard";
            this.mnuMainClearClipboard.Click += new System.EventHandler(this.mnuMainClearClipboard_Click);
            // 
            // mnuMainShare
            // 
            this.mnuMainShare.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainShareFacebook});
            this.mnuMainShare.ForeColor = System.Drawing.Color.Black;
            this.mnuMainShare.Name = "mnuMainShare";
            this.mnuMainShare.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainShare.Size = new System.Drawing.Size(288, 23);
            this.mnuMainShare.Text = "S&hare ...";
            // 
            // mnuMainShareFacebook
            // 
            this.mnuMainShareFacebook.ForeColor = System.Drawing.Color.Black;
            this.mnuMainShareFacebook.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainShareFacebook.Image")));
            this.mnuMainShareFacebook.Name = "mnuMainShareFacebook";
            this.mnuMainShareFacebook.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainShareFacebook.ShortcutKeyDisplayString = "";
            this.mnuMainShareFacebook.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.mnuMainShareFacebook.Size = new System.Drawing.Size(222, 23);
            this.mnuMainShareFacebook.Text = "Upload to &Facebook";
            this.mnuMainShareFacebook.Click += new System.EventHandler(this.mnuMainShareFacebook_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(285, 6);
            // 
            // mnuMainLayout
            // 
            this.mnuMainLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainToolbar,
            this.mnuMainThumbnailBar,
            this.mnuMainCheckBackground});
            this.mnuMainLayout.ForeColor = System.Drawing.Color.Black;
            this.mnuMainLayout.Name = "mnuMainLayout";
            this.mnuMainLayout.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainLayout.Size = new System.Drawing.Size(288, 23);
            this.mnuMainLayout.Text = "&Layout";
            // 
            // mnuMainToolbar
            // 
            this.mnuMainToolbar.BackColor = System.Drawing.Color.Transparent;
            this.mnuMainToolbar.Checked = true;
            this.mnuMainToolbar.CheckOnClick = true;
            this.mnuMainToolbar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuMainToolbar.ForeColor = System.Drawing.Color.Black;
            this.mnuMainToolbar.Name = "mnuMainToolbar";
            this.mnuMainToolbar.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainToolbar.ShortcutKeyDisplayString = "";
            this.mnuMainToolbar.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.mnuMainToolbar.Size = new System.Drawing.Size(228, 23);
            this.mnuMainToolbar.Text = "Toolbar";
            this.mnuMainToolbar.Click += new System.EventHandler(this.mnuMainToolbar_Click);
            // 
            // mnuMainThumbnailBar
            // 
            this.mnuMainThumbnailBar.BackColor = System.Drawing.Color.Transparent;
            this.mnuMainThumbnailBar.CheckOnClick = true;
            this.mnuMainThumbnailBar.ForeColor = System.Drawing.Color.Black;
            this.mnuMainThumbnailBar.Name = "mnuMainThumbnailBar";
            this.mnuMainThumbnailBar.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainThumbnailBar.ShortcutKeyDisplayString = "";
            this.mnuMainThumbnailBar.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.mnuMainThumbnailBar.Size = new System.Drawing.Size(228, 23);
            this.mnuMainThumbnailBar.Text = "Thumbnail panel";
            this.mnuMainThumbnailBar.Click += new System.EventHandler(this.mnuMainThumbnailBar_Click);
            // 
            // mnuMainCheckBackground
            // 
            this.mnuMainCheckBackground.BackColor = System.Drawing.Color.Transparent;
            this.mnuMainCheckBackground.CheckOnClick = true;
            this.mnuMainCheckBackground.ForeColor = System.Drawing.Color.Black;
            this.mnuMainCheckBackground.Name = "mnuMainCheckBackground";
            this.mnuMainCheckBackground.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainCheckBackground.ShortcutKeyDisplayString = "";
            this.mnuMainCheckBackground.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.mnuMainCheckBackground.Size = new System.Drawing.Size(228, 23);
            this.mnuMainCheckBackground.Text = "Checked background";
            this.mnuMainCheckBackground.Click += new System.EventHandler(this.mnuMainCheckBackground_Click);
            // 
            // mnuMainTools
            // 
            this.mnuMainTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainExtensionManager});
            this.mnuMainTools.ForeColor = System.Drawing.Color.Black;
            this.mnuMainTools.Name = "mnuMainTools";
            this.mnuMainTools.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainTools.Size = new System.Drawing.Size(288, 23);
            this.mnuMainTools.Text = "&Tools";
            // 
            // mnuMainExtensionManager
            // 
            this.mnuMainExtensionManager.ForeColor = System.Drawing.Color.Black;
            this.mnuMainExtensionManager.Name = "mnuMainExtensionManager";
            this.mnuMainExtensionManager.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainExtensionManager.ShortcutKeyDisplayString = "";
            this.mnuMainExtensionManager.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.E)));
            this.mnuMainExtensionManager.Size = new System.Drawing.Size(246, 23);
            this.mnuMainExtensionManager.Text = "&Extension manager";
            this.mnuMainExtensionManager.Click += new System.EventHandler(this.mnuMainExtensionManager_Click);
            // 
            // mnuMainSettings
            // 
            this.mnuMainSettings.ForeColor = System.Drawing.Color.Black;
            this.mnuMainSettings.Name = "mnuMainSettings";
            this.mnuMainSettings.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainSettings.ShortcutKeyDisplayString = "";
            this.mnuMainSettings.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.mnuMainSettings.Size = new System.Drawing.Size(288, 23);
            this.mnuMainSettings.Text = "S&ettings";
            this.mnuMainSettings.Click += new System.EventHandler(this.mnuMainSettings_Click);
            // 
            // mnuMainAbout
            // 
            this.mnuMainAbout.ForeColor = System.Drawing.Color.Black;
            this.mnuMainAbout.Name = "mnuMainAbout";
            this.mnuMainAbout.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainAbout.ShortcutKeyDisplayString = "";
            this.mnuMainAbout.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.mnuMainAbout.Size = new System.Drawing.Size(288, 23);
            this.mnuMainAbout.Text = "&About";
            this.mnuMainAbout.Click += new System.EventHandler(this.mnuMainAbout_Click);
            // 
            // toolStripMenuItem21
            // 
            this.toolStripMenuItem21.Name = "toolStripMenuItem21";
            this.toolStripMenuItem21.Size = new System.Drawing.Size(285, 6);
            // 
            // mnuMainReportIssue
            // 
            this.mnuMainReportIssue.ForeColor = System.Drawing.Color.Black;
            this.mnuMainReportIssue.Image = ((System.Drawing.Image)(resources.GetObject("mnuMainReportIssue.Image")));
            this.mnuMainReportIssue.Name = "mnuMainReportIssue";
            this.mnuMainReportIssue.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
            this.mnuMainReportIssue.Size = new System.Drawing.Size(288, 23);
            this.mnuMainReportIssue.Text = "Report an iss&ue";
            this.mnuMainReportIssue.Click += new System.EventHandler(this.mnuMainReportIssue_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo.Margin = new System.Windows.Forms.Padding(10, 1, 5, 2);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 30);
            // 
            // sp0
            // 
            this.sp0.AllowDrop = true;
            this.sp0.BackColor = System.Drawing.Color.Transparent;
            this.sp0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp0.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.sp0.Location = new System.Drawing.Point(0, 0);
            this.sp0.Name = "sp0";
            // 
            // sp0.Panel1
            // 
            this.sp0.Panel1.AllowDrop = true;
            this.sp0.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.sp0.Panel1.Controls.Add(this.sp1);
            this.sp0.Panel1.Controls.Add(this.toolMain);
            // 
            // sp0.Panel2
            // 
            this.sp0.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.sp0.Panel2Collapsed = true;
            this.sp0.Size = new System.Drawing.Size(836, 450);
            this.sp0.SplitterDistance = 595;
            this.sp0.SplitterWidth = 1;
            this.sp0.TabIndex = 1;
            this.sp0.TabStop = false;
            // 
            // sp1
            // 
            this.sp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sp1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.sp1.IsSplitterFixed = true;
            this.sp1.Location = new System.Drawing.Point(0, 33);
            this.sp1.Name = "sp1";
            this.sp1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sp1.Panel1
            // 
            this.sp1.Panel1.Controls.Add(this.picMain);
            // 
            // sp1.Panel2
            // 
            this.sp1.Panel2.Controls.Add(this.thumbnailBar);
            this.sp1.Panel2Collapsed = true;
            this.sp1.Size = new System.Drawing.Size(836, 417);
            this.sp1.SplitterDistance = 349;
            this.sp1.SplitterWidth = 1;
            this.sp1.TabIndex = 2;
            this.sp1.TabStop = false;
            // 
            // picMain
            // 
            this.picMain.AllowDrop = true;
            this.picMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.picMain.ContextMenuStrip = this.mnuPopup;
            this.picMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMain.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.picMain.GridDisplayMode = ImageGlass.ImageBoxGridDisplayMode.None;
            this.picMain.HorizontalScrollBarStyle = ImageGlass.ImageBoxScrollBarStyle.Hide;
            this.picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            this.picMain.Location = new System.Drawing.Point(0, 0);
            this.picMain.Name = "picMain";
            this.picMain.Size = new System.Drawing.Size(836, 417);
            this.picMain.TabIndex = 1;
            this.picMain.VerticalScrollBarStyle = ImageGlass.ImageBoxScrollBarStyle.Hide;
            this.picMain.Zoomed += new System.EventHandler<ImageGlass.ImageBoxZoomEventArgs>(this.picMain_Zoomed);
            this.picMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.picMain_DragDrop);
            this.picMain.DragOver += new System.Windows.Forms.DragEventHandler(this.picMain_DragOver);
            this.picMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picMain_MouseClick);
            // 
            // thumbnailBar
            // 
            this.thumbnailBar.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.thumbnailBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.thumbnailBar.Colors = new ImageGlass.ImageListView.ImageListViewColor(resources.GetString("thumbnailBar.Colors"));
            this.thumbnailBar.ColumnHeaderFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.thumbnailBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thumbnailBar.EnableKeyNavigation = false;
            this.thumbnailBar.GroupHeaderFont = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.thumbnailBar.Location = new System.Drawing.Point(0, 0);
            this.thumbnailBar.MultiSelect = false;
            this.thumbnailBar.Name = "thumbnailBar";
            this.thumbnailBar.PersistentCacheFile = "";
            this.thumbnailBar.PersistentCacheSize = ((long)(100));
            this.thumbnailBar.Size = new System.Drawing.Size(150, 46);
            this.thumbnailBar.TabIndex = 0;
            this.thumbnailBar.ThumbnailSize = new System.Drawing.Size(48, 48);
            this.thumbnailBar.ItemClick += new ImageGlass.ImageListView.ItemClickEventHandler(this.thumbnailBar_ItemClick);
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(836, 450);
            this.Controls.Add(this.sp0);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "frmMain";
            this.Text = "ImageGlass 3";
            this.Activated += new System.EventHandler(this.frmMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResizeBegin += new System.EventHandler(this.frmMain_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.frmMain_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.mnuPopup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sysWatch)).EndInit();
            this.toolMain.ResumeLayout(false);
            this.toolMain.PerformLayout();
            this.mnuMain.ResumeLayout(false);
            this.sp0.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp0)).EndInit();
            this.sp0.ResumeLayout(false);
            this.sp1.Panel1.ResumeLayout(false);
            this.sp1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).EndInit();
            this.sp1.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripButton btnCheckedBackground;
        private System.Windows.Forms.ToolStripButton btnFullScreen;
        private System.Windows.Forms.ToolStripButton btnSlideShow;
        private System.Windows.Forms.ToolStripButton btnConvert;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnSetting;
        private System.Windows.Forms.ToolStripButton btnHelp;
        private System.Windows.Forms.ToolStripLabel lblInfo;
        private System.Windows.Forms.ToolStripButton btnBack;
        private System.Windows.Forms.ContextMenuStrip mnuPopup;
        private System.Windows.Forms.ToolTip tip1;
        private System.Windows.Forms.ToolStripButton btnPrintImage;
        private System.Windows.Forms.ToolStripButton btnFacebook;
        private System.Windows.Forms.ToolStripButton btnExtension;
        private System.Windows.Forms.ToolStripButton btnZoomLock;
        private System.IO.FileSystemWatcher sysWatch;
        private ImageBox picMain;
        private System.Windows.Forms.SplitContainer sp0;
        private System.Windows.Forms.ToolStripDropDownButton btnMenu;
        private System.Windows.Forms.ContextMenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuMainOpenFile;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuMainRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mnuMainManipulation;
        private System.Windows.Forms.ToolStripMenuItem mnuMainReportIssue;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem mnuMainTools;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSettings;
        private System.Windows.Forms.ToolStripMenuItem mnuMainAbout;
        private System.Windows.Forms.ToolStripMenuItem mnuMainRotateCounterclockwise;
        private System.Windows.Forms.ToolStripMenuItem mnuMainRotateClockwise;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem mnuMainZoomIn;
        private System.Windows.Forms.ToolStripMenuItem mnuMainZoomOut;
        private System.Windows.Forms.ToolStripMenuItem mnuMainActualSize;
        private System.Windows.Forms.ToolStripMenuItem mnuMainLockZoomRatio;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem27;
        private System.Windows.Forms.ToolStripMenuItem mnuMainScaleToWidth;
        private System.Windows.Forms.ToolStripMenuItem mnuMainScaleToHeight;
        private System.Windows.Forms.ToolStripMenuItem mnuMainWindowAdaptImage;
        private System.Windows.Forms.ToolStripMenuItem mnuMainClipboard;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCopyMulti;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCut;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCutMulti;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem28;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCopyImagePath;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem14;
        private System.Windows.Forms.ToolStripMenuItem mnuMainClearClipboard;
        private System.Windows.Forms.ToolStripMenuItem mnuMainExtensionManager;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem15;
        private System.Windows.Forms.ToolStripMenuItem mnuMainRename;
        private System.Windows.Forms.ToolStripMenuItem mnuMainMoveToRecycleBin;
        private System.Windows.Forms.ToolStripMenuItem mnuMainDeleteFromHardDisk;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem21;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem mnuMainExtractFrames;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSetAsDesktop;
        private System.Windows.Forms.ToolStripMenuItem mnuMainShare;
        private System.Windows.Forms.ToolStripMenuItem mnuMainShareFacebook;
        private System.Windows.Forms.ToolStripMenuItem mnuMainLayout;
        private System.Windows.Forms.ToolStripMenuItem mnuMainToolbar;
        private System.Windows.Forms.ToolStripMenuItem mnuMainThumbnailBar;
        private System.Windows.Forms.ToolStripMenuItem mnuMainCheckBackground;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem mnuMainFullScreen;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSlideShow;
        private System.Windows.Forms.ToolStripMenuItem mnuMainPrint;
        private System.Windows.Forms.ToolStripMenuItem mnuMainNavigation;
        private System.Windows.Forms.ToolStripMenuItem mnuMainImageLocation;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSlideShowStart;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSlideShowPause;
        private System.Windows.Forms.ToolStripMenuItem mnuMainSlideShowExit;
        private System.Windows.Forms.ToolStripMenuItem mnuMainViewNext;
        private System.Windows.Forms.ToolStripMenuItem mnuMainViewPrevious;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem24;
        private System.Windows.Forms.ToolStripMenuItem mnuMainGoto;
        private System.Windows.Forms.ToolStripMenuItem mnuMainGotoFirst;
        private System.Windows.Forms.ToolStripMenuItem mnuMainGotoLast;
        private System.Windows.Forms.ToolStripMenuItem mnuMainImageProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuMainOpenImageData;
        private System.Windows.Forms.SplitContainer sp1;
        private ImageListView.ImageListView thumbnailBar;
        private System.Windows.Forms.ToolStripMenuItem mnuMainStartStopAnimating;
        private System.Windows.Forms.ToolStripMenuItem mnuMainEditImage;
        private System.Windows.Forms.ToolStripMenuItem sampleMenuItemToolStripMenuItem;
    }
}

