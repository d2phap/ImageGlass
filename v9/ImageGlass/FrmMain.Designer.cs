using ImageGlass.Base;

namespace ImageGlass
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.Toolbar = new ImageGlass.UI.ModernToolbar();
            this.MnuMain = new ImageGlass.UI.ModernMenu(this.components);
            this.MnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuNewWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuOpenWith = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuShare = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuReload = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuReloadImageList = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuUnload = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuNavigation = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewNext = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuGoToFirst = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuGoToLast = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuViewNextFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewPreviousFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewFirstFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewLastFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCustomZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuActualSize = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuAutoZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLockZoom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToWidth = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToHeight = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToFit = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuScaleToFill = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanning = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanRight = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanUp = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuPanToLeftSide = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanToRightSide = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanToTop = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPanToBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuImage = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuViewChannels = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLoadingOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuRotateRight = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuFlipHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuFlipVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuRename = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuMoveToRecycleBin = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuDeleteFromHardDisk = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuToggleImageAnimation = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuExtractFrames = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSetDesktopBackground = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSetLockScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuOpenLocation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuImageProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPasteImage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuCopyImageData = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuClearClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuWindowFit = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuFrameless = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuFullScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuStartSlideshow = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCloseAllSlideshows = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuLayout = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuToggleToolbar = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuToggleThumbnails = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuToggleCheckerboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuToggleTopMost = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuColorPicker = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCropTool = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuPageNav = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuExifTool = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuCheckForUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuReportIssue = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuFirstLaunch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuSetDefaultPhotoViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuUnsetDefaultPhotoViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.Tb0 = new System.Windows.Forms.TableLayoutPanel();
            this.Sp1 = new ImageGlass.UI.ModernSplitContainer();
            this.Sp2 = new ImageGlass.UI.ModernSplitContainer();
            this.PicMain = new ImageGlass.Views.DXCanvas();
            this.Gallery = new ImageGlass.Gallery.ImageGallery();
            this.MnuContext = new ImageGlass.UI.ModernMenu(this.components);
            this.itemToPreserveTheSpaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.MnuSubMenu = new ImageGlass.UI.ModernMenu(this.components);
            this.itemToPreserveSpaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuMain.SuspendLayout();
            this.Tb0.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Sp1)).BeginInit();
            this.Sp1.Panel1.SuspendLayout();
            this.Sp1.Panel2.SuspendLayout();
            this.Sp1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Sp2)).BeginInit();
            this.Sp2.Panel1.SuspendLayout();
            this.Sp2.SuspendLayout();
            this.MnuContext.SuspendLayout();
            this.MnuSubMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // Toolbar
            // 
            this.Toolbar.Alignment = ImageGlass.UI.ToolbarAlignment.Center;
            this.Toolbar.AutoFocusOnHover = true;
            this.Toolbar.BackColor = System.Drawing.Color.Transparent;
            this.Tb0.SetColumnSpan(this.Toolbar, 3);
            this.Toolbar.EnableTransparent = true;
            this.Toolbar.GripMargin = new System.Windows.Forms.Padding(0);
            this.Toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.Toolbar.HideTooltips = false;
            this.Toolbar.IconHeight = 22;
            this.Toolbar.ImageScalingSize = new System.Drawing.Size(30, 30);
            this.Toolbar.Location = new System.Drawing.Point(0, 0);
            this.Toolbar.MainMenu = this.MnuMain;
            this.Toolbar.Name = "Toolbar";
            this.Toolbar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.Toolbar.ShowItemToolTips = false;
            this.Toolbar.ShowMainMenuButton = true;
            this.Toolbar.Size = new System.Drawing.Size(1266, 25);
            this.Toolbar.TabIndex = 1;
            this.Toolbar.Theme = null;
            this.Toolbar.ToolTipDirection = ImageGlass.UI.TooltipDirection.Bottom;
            this.Toolbar.ToolTipText = "";
            this.Toolbar.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Toolbar_ItemClicked);
            // 
            // MnuMain
            // 
            this.MnuMain.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.MnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuFile,
            this.MnuNavigation,
            this.MnuZoom,
            this.MnuPanning,
            this.MnuImage,
            this.MnuClipboard,
            this.toolStripMenuItem6,
            this.MnuWindowFit,
            this.MnuFrameless,
            this.MnuFullScreen,
            this.MnuSlideshow,
            this.toolStripMenuItem7,
            this.MnuLayout,
            this.MnuTools,
            this.toolStripMenuItem8,
            this.MnuSettings,
            this.MnuHelp,
            this.toolStripMenuItem9,
            this.MnuExit});
            this.MnuMain.Name = "MnuContext";
            this.MnuMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.MnuMain.Size = new System.Drawing.Size(277, 748);
            this.MnuMain.Opening += new System.ComponentModel.CancelEventHandler(this.MnuMain_Opening);
            // 
            // MnuFile
            // 
            this.MnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuOpenFile,
            this.MnuNewWindow,
            this.MnuSave,
            this.MnuSaveAs,
            this.toolStripMenuItem10,
            this.MnuOpenWith,
            this.MnuEdit,
            this.MnuPrint,
            this.MnuShare,
            this.toolStripMenuItem12,
            this.MnuRefresh,
            this.MnuReload,
            this.MnuReloadImageList,
            this.MnuUnload});
            this.MnuFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuFile.Name = "MnuFile";
            this.MnuFile.Size = new System.Drawing.Size(276, 48);
            this.MnuFile.Text = "[File]";
            // 
            // MnuOpenFile
            // 
            this.MnuOpenFile.Name = "MnuOpenFile";
            this.MnuOpenFile.Size = new System.Drawing.Size(451, 54);
            this.MnuOpenFile.Text = "[Open file...]";
            this.MnuOpenFile.Click += new System.EventHandler(this.MnuOpenFile_Click);
            // 
            // MnuNewWindow
            // 
            this.MnuNewWindow.Name = "MnuNewWindow";
            this.MnuNewWindow.Size = new System.Drawing.Size(451, 54);
            this.MnuNewWindow.Text = "[Open new window]";
            this.MnuNewWindow.Click += new System.EventHandler(this.MnuNewWindow_Click);
            // 
            // MnuSave
            // 
            this.MnuSave.Name = "MnuSave";
            this.MnuSave.Size = new System.Drawing.Size(451, 54);
            this.MnuSave.Text = "[Save image]";
            this.MnuSave.Click += new System.EventHandler(this.MnuSave_Click);
            // 
            // MnuSaveAs
            // 
            this.MnuSaveAs.Name = "MnuSaveAs";
            this.MnuSaveAs.Size = new System.Drawing.Size(451, 54);
            this.MnuSaveAs.Text = "[Save image as...]";
            this.MnuSaveAs.Click += new System.EventHandler(this.MnuSaveAs_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(448, 6);
            // 
            // MnuOpenWith
            // 
            this.MnuOpenWith.Name = "MnuOpenWith";
            this.MnuOpenWith.Size = new System.Drawing.Size(451, 54);
            this.MnuOpenWith.Text = "[Open with...]";
            this.MnuOpenWith.Click += new System.EventHandler(this.MnuOpenWith_Click);
            // 
            // MnuEdit
            // 
            this.MnuEdit.Name = "MnuEdit";
            this.MnuEdit.Size = new System.Drawing.Size(451, 54);
            this.MnuEdit.Text = "[Edit image...]";
            this.MnuEdit.Click += new System.EventHandler(this.MnuEdit_Click);
            // 
            // MnuPrint
            // 
            this.MnuPrint.Name = "MnuPrint";
            this.MnuPrint.Size = new System.Drawing.Size(451, 54);
            this.MnuPrint.Text = "[Print...]";
            this.MnuPrint.Click += new System.EventHandler(this.MnuPrint_Click);
            // 
            // MnuShare
            // 
            this.MnuShare.Name = "MnuShare";
            this.MnuShare.Size = new System.Drawing.Size(451, 54);
            this.MnuShare.Text = "[Share...]";
            this.MnuShare.Click += new System.EventHandler(this.MnuShare_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(448, 6);
            // 
            // MnuRefresh
            // 
            this.MnuRefresh.Name = "MnuRefresh";
            this.MnuRefresh.Size = new System.Drawing.Size(451, 54);
            this.MnuRefresh.Text = "[Refresh]";
            this.MnuRefresh.Click += new System.EventHandler(this.MnuRefresh_Click);
            // 
            // MnuReload
            // 
            this.MnuReload.Name = "MnuReload";
            this.MnuReload.Size = new System.Drawing.Size(451, 54);
            this.MnuReload.Text = "[Reload image]";
            this.MnuReload.Click += new System.EventHandler(this.MnuReload_Click);
            // 
            // MnuReloadImageList
            // 
            this.MnuReloadImageList.Name = "MnuReloadImageList";
            this.MnuReloadImageList.Size = new System.Drawing.Size(451, 54);
            this.MnuReloadImageList.Text = "[Reload image list]";
            this.MnuReloadImageList.Click += new System.EventHandler(this.MnuReloadImageList_Click);
            // 
            // MnuUnload
            // 
            this.MnuUnload.Name = "MnuUnload";
            this.MnuUnload.Size = new System.Drawing.Size(451, 54);
            this.MnuUnload.Text = "[Unload image]";
            this.MnuUnload.Click += new System.EventHandler(this.MnuUnload_Click);
            // 
            // MnuNavigation
            // 
            this.MnuNavigation.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuViewNext,
            this.MnuViewPrevious,
            this.toolStripMenuItem13,
            this.MnuGoTo,
            this.MnuGoToFirst,
            this.MnuGoToLast,
            this.toolStripMenuItem14,
            this.MnuViewNextFrame,
            this.MnuViewPreviousFrame,
            this.MnuViewFirstFrame,
            this.MnuViewLastFrame});
            this.MnuNavigation.Image = ((System.Drawing.Image)(resources.GetObject("MnuNavigation.Image")));
            this.MnuNavigation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuNavigation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuNavigation.Name = "MnuNavigation";
            this.MnuNavigation.Size = new System.Drawing.Size(276, 48);
            this.MnuNavigation.Text = "[Navigation]";
            // 
            // MnuViewNext
            // 
            this.MnuViewNext.Name = "MnuViewNext";
            this.MnuViewNext.Size = new System.Drawing.Size(477, 54);
            this.MnuViewNext.Text = "[View next image]";
            this.MnuViewNext.Click += new System.EventHandler(this.MnuViewNext_Click);
            // 
            // MnuViewPrevious
            // 
            this.MnuViewPrevious.Name = "MnuViewPrevious";
            this.MnuViewPrevious.Size = new System.Drawing.Size(477, 54);
            this.MnuViewPrevious.Text = "[View previous image]";
            this.MnuViewPrevious.Click += new System.EventHandler(this.MnuViewPrevious_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(474, 6);
            // 
            // MnuGoTo
            // 
            this.MnuGoTo.Name = "MnuGoTo";
            this.MnuGoTo.Size = new System.Drawing.Size(477, 54);
            this.MnuGoTo.Text = "[Go to...]";
            this.MnuGoTo.Click += new System.EventHandler(this.MnuGoTo_Click);
            // 
            // MnuGoToFirst
            // 
            this.MnuGoToFirst.Name = "MnuGoToFirst";
            this.MnuGoToFirst.Size = new System.Drawing.Size(477, 54);
            this.MnuGoToFirst.Text = "[Go to the first image]";
            this.MnuGoToFirst.Click += new System.EventHandler(this.MnuGoToFirst_Click);
            // 
            // MnuGoToLast
            // 
            this.MnuGoToLast.Name = "MnuGoToLast";
            this.MnuGoToLast.Size = new System.Drawing.Size(477, 54);
            this.MnuGoToLast.Text = "[Go to the last image]";
            this.MnuGoToLast.Click += new System.EventHandler(this.MnuGoToLast_Click);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(474, 6);
            // 
            // MnuViewNextFrame
            // 
            this.MnuViewNextFrame.Name = "MnuViewNextFrame";
            this.MnuViewNextFrame.Size = new System.Drawing.Size(477, 54);
            this.MnuViewNextFrame.Text = "[View next frame]";
            this.MnuViewNextFrame.Click += new System.EventHandler(this.MnuViewNextFrame_Click);
            // 
            // MnuViewPreviousFrame
            // 
            this.MnuViewPreviousFrame.Name = "MnuViewPreviousFrame";
            this.MnuViewPreviousFrame.Size = new System.Drawing.Size(477, 54);
            this.MnuViewPreviousFrame.Text = "[View previous frame]";
            this.MnuViewPreviousFrame.Click += new System.EventHandler(this.MnuViewPreviousFrame_Click);
            // 
            // MnuViewFirstFrame
            // 
            this.MnuViewFirstFrame.Name = "MnuViewFirstFrame";
            this.MnuViewFirstFrame.Size = new System.Drawing.Size(477, 54);
            this.MnuViewFirstFrame.Text = "[View the first frame]";
            this.MnuViewFirstFrame.Click += new System.EventHandler(this.MnuViewFirstFrame_Click);
            // 
            // MnuViewLastFrame
            // 
            this.MnuViewLastFrame.Name = "MnuViewLastFrame";
            this.MnuViewLastFrame.Size = new System.Drawing.Size(477, 54);
            this.MnuViewLastFrame.Text = "[View the last frame]";
            this.MnuViewLastFrame.Click += new System.EventHandler(this.MnuViewLastFrame_Click);
            // 
            // MnuZoom
            // 
            this.MnuZoom.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuZoomIn,
            this.MnuZoomOut,
            this.MnuCustomZoom,
            this.MnuActualSize,
            this.toolStripMenuItem15,
            this.MnuAutoZoom,
            this.MnuLockZoom,
            this.MnuScaleToWidth,
            this.MnuScaleToHeight,
            this.MnuScaleToFit,
            this.MnuScaleToFill});
            this.MnuZoom.Image = ((System.Drawing.Image)(resources.GetObject("MnuZoom.Image")));
            this.MnuZoom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuZoom.Name = "MnuZoom";
            this.MnuZoom.Size = new System.Drawing.Size(276, 48);
            this.MnuZoom.Text = "[Zoom]";
            // 
            // MnuZoomIn
            // 
            this.MnuZoomIn.Name = "MnuZoomIn";
            this.MnuZoomIn.Size = new System.Drawing.Size(415, 54);
            this.MnuZoomIn.Text = "[Zoom in]";
            this.MnuZoomIn.Click += new System.EventHandler(this.MnuZoomIn_Click);
            // 
            // MnuZoomOut
            // 
            this.MnuZoomOut.Name = "MnuZoomOut";
            this.MnuZoomOut.Size = new System.Drawing.Size(415, 54);
            this.MnuZoomOut.Text = "[Zoom out]";
            this.MnuZoomOut.Click += new System.EventHandler(this.MnuZoomOut_Click);
            // 
            // MnuCustomZoom
            // 
            this.MnuCustomZoom.Name = "MnuCustomZoom";
            this.MnuCustomZoom.Size = new System.Drawing.Size(415, 54);
            this.MnuCustomZoom.Text = "[Custom zoom...]";
            this.MnuCustomZoom.Click += new System.EventHandler(this.MnuCustomZoom_Click);
            // 
            // MnuActualSize
            // 
            this.MnuActualSize.Name = "MnuActualSize";
            this.MnuActualSize.Size = new System.Drawing.Size(415, 54);
            this.MnuActualSize.Text = "[View actual size]";
            this.MnuActualSize.Click += new System.EventHandler(this.MnuActualSize_Click);
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(412, 6);
            // 
            // MnuAutoZoom
            // 
            this.MnuAutoZoom.CheckOnClick = true;
            this.MnuAutoZoom.Name = "MnuAutoZoom";
            this.MnuAutoZoom.Size = new System.Drawing.Size(415, 54);
            this.MnuAutoZoom.Tag = "";
            this.MnuAutoZoom.Text = "[Auto Zoom]";
            this.MnuAutoZoom.Click += new System.EventHandler(this.MnuAutoZoom_Click);
            // 
            // MnuLockZoom
            // 
            this.MnuLockZoom.CheckOnClick = true;
            this.MnuLockZoom.Name = "MnuLockZoom";
            this.MnuLockZoom.Size = new System.Drawing.Size(415, 54);
            this.MnuLockZoom.Tag = "";
            this.MnuLockZoom.Text = "[Lock zoom ratio]";
            this.MnuLockZoom.Click += new System.EventHandler(this.MnuLockZoom_Click);
            // 
            // MnuScaleToWidth
            // 
            this.MnuScaleToWidth.CheckOnClick = true;
            this.MnuScaleToWidth.Name = "MnuScaleToWidth";
            this.MnuScaleToWidth.Size = new System.Drawing.Size(415, 54);
            this.MnuScaleToWidth.Tag = "";
            this.MnuScaleToWidth.Text = "[Scale to width]";
            this.MnuScaleToWidth.Click += new System.EventHandler(this.MnuScaleToWidth_Click);
            // 
            // MnuScaleToHeight
            // 
            this.MnuScaleToHeight.CheckOnClick = true;
            this.MnuScaleToHeight.Name = "MnuScaleToHeight";
            this.MnuScaleToHeight.Size = new System.Drawing.Size(415, 54);
            this.MnuScaleToHeight.Tag = "";
            this.MnuScaleToHeight.Text = "[Scale to height]";
            this.MnuScaleToHeight.Click += new System.EventHandler(this.MnuScaleToHeight_Click);
            // 
            // MnuScaleToFit
            // 
            this.MnuScaleToFit.CheckOnClick = true;
            this.MnuScaleToFit.Name = "MnuScaleToFit";
            this.MnuScaleToFit.Size = new System.Drawing.Size(415, 54);
            this.MnuScaleToFit.Tag = "";
            this.MnuScaleToFit.Text = "[Scale to fit]";
            this.MnuScaleToFit.Click += new System.EventHandler(this.MnuScaleToFit_Click);
            // 
            // MnuScaleToFill
            // 
            this.MnuScaleToFill.CheckOnClick = true;
            this.MnuScaleToFill.Name = "MnuScaleToFill";
            this.MnuScaleToFill.Size = new System.Drawing.Size(415, 54);
            this.MnuScaleToFill.Tag = "";
            this.MnuScaleToFill.Text = "[Scale to fill]";
            this.MnuScaleToFill.Click += new System.EventHandler(this.MnuScaleToFill_Click);
            // 
            // MnuPanning
            // 
            this.MnuPanning.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuPanLeft,
            this.MnuPanRight,
            this.MnuPanUp,
            this.MnuPanDown,
            this.toolStripSeparator3,
            this.MnuPanToLeftSide,
            this.MnuPanToRightSide,
            this.MnuPanToTop,
            this.MnuPanToBottom});
            this.MnuPanning.Image = ((System.Drawing.Image)(resources.GetObject("MnuPanning.Image")));
            this.MnuPanning.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuPanning.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuPanning.Name = "MnuPanning";
            this.MnuPanning.Size = new System.Drawing.Size(276, 48);
            this.MnuPanning.Text = "[Panning]";
            // 
            // MnuPanLeft
            // 
            this.MnuPanLeft.Name = "MnuPanLeft";
            this.MnuPanLeft.Size = new System.Drawing.Size(559, 54);
            this.MnuPanLeft.Text = "[Pan image left]";
            this.MnuPanLeft.Click += new System.EventHandler(this.MnuPanLeft_Click);
            // 
            // MnuPanRight
            // 
            this.MnuPanRight.Name = "MnuPanRight";
            this.MnuPanRight.Size = new System.Drawing.Size(559, 54);
            this.MnuPanRight.Text = "[Pan image right]";
            this.MnuPanRight.Click += new System.EventHandler(this.MnuPanRight_Click);
            // 
            // MnuPanUp
            // 
            this.MnuPanUp.Name = "MnuPanUp";
            this.MnuPanUp.Size = new System.Drawing.Size(559, 54);
            this.MnuPanUp.Text = "[Pan image up]";
            this.MnuPanUp.Click += new System.EventHandler(this.MnuPanUp_Click);
            // 
            // MnuPanDown
            // 
            this.MnuPanDown.Name = "MnuPanDown";
            this.MnuPanDown.Size = new System.Drawing.Size(559, 54);
            this.MnuPanDown.Text = "[Pan image down]";
            this.MnuPanDown.Click += new System.EventHandler(this.MnuPanDown_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(556, 6);
            // 
            // MnuPanToLeftSide
            // 
            this.MnuPanToLeftSide.Name = "MnuPanToLeftSide";
            this.MnuPanToLeftSide.Size = new System.Drawing.Size(559, 54);
            this.MnuPanToLeftSide.Text = "[Pan image to the left side]";
            this.MnuPanToLeftSide.Click += new System.EventHandler(this.MnuPanToLeftSide_Click);
            // 
            // MnuPanToRightSide
            // 
            this.MnuPanToRightSide.Name = "MnuPanToRightSide";
            this.MnuPanToRightSide.Size = new System.Drawing.Size(559, 54);
            this.MnuPanToRightSide.Text = "[Pan image to the right side]";
            this.MnuPanToRightSide.Click += new System.EventHandler(this.MnuPanToRightSide_Click);
            // 
            // MnuPanToTop
            // 
            this.MnuPanToTop.Name = "MnuPanToTop";
            this.MnuPanToTop.Size = new System.Drawing.Size(559, 54);
            this.MnuPanToTop.Text = "[Pan image to the top]";
            this.MnuPanToTop.Click += new System.EventHandler(this.MnuPanToTop_Click);
            // 
            // MnuPanToBottom
            // 
            this.MnuPanToBottom.Name = "MnuPanToBottom";
            this.MnuPanToBottom.Size = new System.Drawing.Size(559, 54);
            this.MnuPanToBottom.Text = "[Pan image to the bottom]";
            this.MnuPanToBottom.Click += new System.EventHandler(this.MnuPanToBottom_Click);
            // 
            // MnuImage
            // 
            this.MnuImage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuViewChannels,
            this.MnuLoadingOrders,
            this.toolStripMenuItem16,
            this.MnuRotateLeft,
            this.MnuRotateRight,
            this.MnuFlipHorizontal,
            this.MnuFlipVertical,
            this.toolStripMenuItem17,
            this.MnuRename,
            this.MnuMoveToRecycleBin,
            this.MnuDeleteFromHardDisk,
            this.toolStripMenuItem18,
            this.MnuToggleImageAnimation,
            this.MnuExtractFrames,
            this.MnuSetDesktopBackground,
            this.MnuSetLockScreen,
            this.MnuOpenLocation,
            this.toolStripSeparator1,
            this.MnuImageProperties});
            this.MnuImage.Image = ((System.Drawing.Image)(resources.GetObject("MnuImage.Image")));
            this.MnuImage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuImage.Name = "MnuImage";
            this.MnuImage.Size = new System.Drawing.Size(276, 48);
            this.MnuImage.Text = "[Image]";
            // 
            // MnuViewChannels
            // 
            this.MnuViewChannels.Name = "MnuViewChannels";
            this.MnuViewChannels.Size = new System.Drawing.Size(581, 54);
            this.MnuViewChannels.Text = "[Channels]";
            // 
            // MnuLoadingOrders
            // 
            this.MnuLoadingOrders.Name = "MnuLoadingOrders";
            this.MnuLoadingOrders.Size = new System.Drawing.Size(581, 54);
            this.MnuLoadingOrders.Text = "[Loading orders]";
            // 
            // toolStripMenuItem16
            // 
            this.toolStripMenuItem16.Name = "toolStripMenuItem16";
            this.toolStripMenuItem16.Size = new System.Drawing.Size(578, 6);
            // 
            // MnuRotateLeft
            // 
            this.MnuRotateLeft.Name = "MnuRotateLeft";
            this.MnuRotateLeft.Size = new System.Drawing.Size(581, 54);
            this.MnuRotateLeft.Text = "[Rotate counterclockwise]";
            this.MnuRotateLeft.Click += new System.EventHandler(this.MnuRotateLeft_Click);
            // 
            // MnuRotateRight
            // 
            this.MnuRotateRight.Name = "MnuRotateRight";
            this.MnuRotateRight.Size = new System.Drawing.Size(581, 54);
            this.MnuRotateRight.Text = "[Rotate Clockwise]";
            this.MnuRotateRight.Click += new System.EventHandler(this.MnuRotateRight_Click);
            // 
            // MnuFlipHorizontal
            // 
            this.MnuFlipHorizontal.Name = "MnuFlipHorizontal";
            this.MnuFlipHorizontal.Size = new System.Drawing.Size(581, 54);
            this.MnuFlipHorizontal.Text = "[Flip Horizontal]";
            this.MnuFlipHorizontal.Click += new System.EventHandler(this.MnuFlipHorizontal_Click);
            // 
            // MnuFlipVertical
            // 
            this.MnuFlipVertical.Name = "MnuFlipVertical";
            this.MnuFlipVertical.Size = new System.Drawing.Size(581, 54);
            this.MnuFlipVertical.Text = "[Flip Vertical]";
            this.MnuFlipVertical.Click += new System.EventHandler(this.MnuFlipVertical_Click);
            // 
            // toolStripMenuItem17
            // 
            this.toolStripMenuItem17.Name = "toolStripMenuItem17";
            this.toolStripMenuItem17.Size = new System.Drawing.Size(578, 6);
            // 
            // MnuRename
            // 
            this.MnuRename.Name = "MnuRename";
            this.MnuRename.Size = new System.Drawing.Size(581, 54);
            this.MnuRename.Text = "[Rename image]";
            this.MnuRename.Click += new System.EventHandler(this.MnuRename_Click);
            // 
            // MnuMoveToRecycleBin
            // 
            this.MnuMoveToRecycleBin.Name = "MnuMoveToRecycleBin";
            this.MnuMoveToRecycleBin.Size = new System.Drawing.Size(581, 54);
            this.MnuMoveToRecycleBin.Text = "[Move to recycle bin]";
            this.MnuMoveToRecycleBin.Click += new System.EventHandler(this.MnuMoveToRecycleBin_Click);
            // 
            // MnuDeleteFromHardDisk
            // 
            this.MnuDeleteFromHardDisk.Name = "MnuDeleteFromHardDisk";
            this.MnuDeleteFromHardDisk.Size = new System.Drawing.Size(581, 54);
            this.MnuDeleteFromHardDisk.Text = "[Delete from hard disk]";
            this.MnuDeleteFromHardDisk.Click += new System.EventHandler(this.MnuDeleteFromHardDisk_Click);
            // 
            // toolStripMenuItem18
            // 
            this.toolStripMenuItem18.Name = "toolStripMenuItem18";
            this.toolStripMenuItem18.Size = new System.Drawing.Size(578, 6);
            // 
            // MnuToggleImageAnimation
            // 
            this.MnuToggleImageAnimation.Name = "MnuToggleImageAnimation";
            this.MnuToggleImageAnimation.Size = new System.Drawing.Size(581, 54);
            this.MnuToggleImageAnimation.Text = "[Start / stop animating image]";
            this.MnuToggleImageAnimation.Click += new System.EventHandler(this.MnuToggleImageAnimation_Click);
            // 
            // MnuExtractFrames
            // 
            this.MnuExtractFrames.Name = "MnuExtractFrames";
            this.MnuExtractFrames.Size = new System.Drawing.Size(581, 54);
            this.MnuExtractFrames.Text = "[Extract image frames]";
            this.MnuExtractFrames.Click += new System.EventHandler(this.MnuExtractFrames_Click);
            // 
            // MnuSetDesktopBackground
            // 
            this.MnuSetDesktopBackground.Name = "MnuSetDesktopBackground";
            this.MnuSetDesktopBackground.Size = new System.Drawing.Size(581, 54);
            this.MnuSetDesktopBackground.Text = "[Set as desktop background]";
            this.MnuSetDesktopBackground.Click += new System.EventHandler(this.MnuSetDesktopBackground_Click);
            // 
            // MnuSetLockScreen
            // 
            this.MnuSetLockScreen.Name = "MnuSetLockScreen";
            this.MnuSetLockScreen.Size = new System.Drawing.Size(581, 54);
            this.MnuSetLockScreen.Text = "[Set as Lock Screen image]";
            this.MnuSetLockScreen.Click += new System.EventHandler(this.MnuSetLockScreen_Click);
            // 
            // MnuOpenLocation
            // 
            this.MnuOpenLocation.Name = "MnuOpenLocation";
            this.MnuOpenLocation.Size = new System.Drawing.Size(581, 54);
            this.MnuOpenLocation.Text = "[Open image location]";
            this.MnuOpenLocation.Click += new System.EventHandler(this.MnuOpenLocation_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(578, 6);
            // 
            // MnuImageProperties
            // 
            this.MnuImageProperties.Name = "MnuImageProperties";
            this.MnuImageProperties.Size = new System.Drawing.Size(581, 54);
            this.MnuImageProperties.Text = "[Image properties]";
            this.MnuImageProperties.Click += new System.EventHandler(this.MnuImageProperties_Click);
            // 
            // MnuClipboard
            // 
            this.MnuClipboard.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuPasteImage,
            this.toolStripMenuItem19,
            this.MnuCopyImageData,
            this.MnuCopyPath,
            this.MnuCopy,
            this.MnuCut,
            this.toolStripMenuItem23,
            this.MnuClearClipboard});
            this.MnuClipboard.Image = ((System.Drawing.Image)(resources.GetObject("MnuClipboard.Image")));
            this.MnuClipboard.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuClipboard.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuClipboard.Name = "MnuClipboard";
            this.MnuClipboard.Size = new System.Drawing.Size(276, 48);
            this.MnuClipboard.Text = "[Clipboard]";
            // 
            // MnuPasteImage
            // 
            this.MnuPasteImage.Name = "MnuPasteImage";
            this.MnuPasteImage.Size = new System.Drawing.Size(445, 54);
            this.MnuPasteImage.Text = "[Paste image]";
            this.MnuPasteImage.Click += new System.EventHandler(this.MnuPasteImage_Click);
            // 
            // toolStripMenuItem19
            // 
            this.toolStripMenuItem19.Name = "toolStripMenuItem19";
            this.toolStripMenuItem19.Size = new System.Drawing.Size(442, 6);
            // 
            // MnuCopyImageData
            // 
            this.MnuCopyImageData.Name = "MnuCopyImageData";
            this.MnuCopyImageData.Size = new System.Drawing.Size(445, 54);
            this.MnuCopyImageData.Text = "[Copy image pixels]";
            this.MnuCopyImageData.Click += new System.EventHandler(this.MnuCopyImageData_Click);
            // 
            // MnuCopyPath
            // 
            this.MnuCopyPath.Name = "MnuCopyPath";
            this.MnuCopyPath.Size = new System.Drawing.Size(445, 54);
            this.MnuCopyPath.Text = "[Copy image path]";
            this.MnuCopyPath.Click += new System.EventHandler(this.MnuCopyPath_Click);
            // 
            // MnuCopy
            // 
            this.MnuCopy.Name = "MnuCopy";
            this.MnuCopy.Size = new System.Drawing.Size(445, 54);
            this.MnuCopy.Text = "[Copy]";
            this.MnuCopy.Click += new System.EventHandler(this.MnuCopy_Click);
            // 
            // MnuCut
            // 
            this.MnuCut.Name = "MnuCut";
            this.MnuCut.Size = new System.Drawing.Size(445, 54);
            this.MnuCut.Text = "[Cut]";
            this.MnuCut.Click += new System.EventHandler(this.MnuCut_Click);
            // 
            // toolStripMenuItem23
            // 
            this.toolStripMenuItem23.Name = "toolStripMenuItem23";
            this.toolStripMenuItem23.Size = new System.Drawing.Size(442, 6);
            // 
            // MnuClearClipboard
            // 
            this.MnuClearClipboard.Name = "MnuClearClipboard";
            this.MnuClearClipboard.Size = new System.Drawing.Size(445, 54);
            this.MnuClearClipboard.Text = "[Clear clipboard]";
            this.MnuClearClipboard.Click += new System.EventHandler(this.MnuClearClipboard_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(273, 6);
            // 
            // MnuWindowFit
            // 
            this.MnuWindowFit.CheckOnClick = true;
            this.MnuWindowFit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuWindowFit.Name = "MnuWindowFit";
            this.MnuWindowFit.Size = new System.Drawing.Size(276, 48);
            this.MnuWindowFit.Text = "[Window fit]";
            this.MnuWindowFit.Click += new System.EventHandler(this.MnuWindowFit_Click);
            // 
            // MnuFrameless
            // 
            this.MnuFrameless.CheckOnClick = true;
            this.MnuFrameless.Image = ((System.Drawing.Image)(resources.GetObject("MnuFrameless.Image")));
            this.MnuFrameless.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuFrameless.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuFrameless.Name = "MnuFrameless";
            this.MnuFrameless.Size = new System.Drawing.Size(276, 48);
            this.MnuFrameless.Text = "[Frameless]";
            this.MnuFrameless.Click += new System.EventHandler(this.MnuFrameless_Click);
            // 
            // MnuFullScreen
            // 
            this.MnuFullScreen.CheckOnClick = true;
            this.MnuFullScreen.Image = ((System.Drawing.Image)(resources.GetObject("MnuFullScreen.Image")));
            this.MnuFullScreen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuFullScreen.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuFullScreen.Name = "MnuFullScreen";
            this.MnuFullScreen.Size = new System.Drawing.Size(276, 48);
            this.MnuFullScreen.Text = "[Full screen]";
            this.MnuFullScreen.Click += new System.EventHandler(this.MnuFullScreen_Click);
            // 
            // MnuSlideshow
            // 
            this.MnuSlideshow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuStartSlideshow,
            this.MnuCloseAllSlideshows});
            this.MnuSlideshow.Image = ((System.Drawing.Image)(resources.GetObject("MnuSlideshow.Image")));
            this.MnuSlideshow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuSlideshow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuSlideshow.Name = "MnuSlideshow";
            this.MnuSlideshow.Size = new System.Drawing.Size(276, 48);
            this.MnuSlideshow.Text = "Slideshow";
            // 
            // MnuStartSlideshow
            // 
            this.MnuStartSlideshow.Name = "MnuStartSlideshow";
            this.MnuStartSlideshow.Size = new System.Drawing.Size(464, 54);
            this.MnuStartSlideshow.Text = "[Start new slideshow]";
            this.MnuStartSlideshow.Click += new System.EventHandler(this.MnuStartSlideshow_Click);
            // 
            // MnuCloseAllSlideshows
            // 
            this.MnuCloseAllSlideshows.Name = "MnuCloseAllSlideshows";
            this.MnuCloseAllSlideshows.Size = new System.Drawing.Size(464, 54);
            this.MnuCloseAllSlideshows.Text = "[Close all slideshows]";
            this.MnuCloseAllSlideshows.Click += new System.EventHandler(this.MnuCloseAllSlideshows_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(273, 6);
            // 
            // MnuLayout
            // 
            this.MnuLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuToggleToolbar,
            this.MnuToggleThumbnails,
            this.MnuToggleCheckerboard,
            this.toolStripMenuItem20,
            this.MnuToggleTopMost});
            this.MnuLayout.Image = ((System.Drawing.Image)(resources.GetObject("MnuLayout.Image")));
            this.MnuLayout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuLayout.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuLayout.Name = "MnuLayout";
            this.MnuLayout.Size = new System.Drawing.Size(276, 48);
            this.MnuLayout.Text = "[Layout]";
            // 
            // MnuToggleToolbar
            // 
            this.MnuToggleToolbar.CheckOnClick = true;
            this.MnuToggleToolbar.Name = "MnuToggleToolbar";
            this.MnuToggleToolbar.Size = new System.Drawing.Size(573, 54);
            this.MnuToggleToolbar.Text = "[Toolbar]";
            this.MnuToggleToolbar.Click += new System.EventHandler(this.MnuToggleToolbar_Click);
            // 
            // MnuToggleThumbnails
            // 
            this.MnuToggleThumbnails.CheckOnClick = true;
            this.MnuToggleThumbnails.Name = "MnuToggleThumbnails";
            this.MnuToggleThumbnails.Size = new System.Drawing.Size(573, 54);
            this.MnuToggleThumbnails.Text = "[Thumbnail panel]";
            this.MnuToggleThumbnails.Click += new System.EventHandler(this.MnuToggleThumbnails_Click);
            // 
            // MnuToggleCheckerboard
            // 
            this.MnuToggleCheckerboard.CheckOnClick = true;
            this.MnuToggleCheckerboard.Name = "MnuToggleCheckerboard";
            this.MnuToggleCheckerboard.Size = new System.Drawing.Size(573, 54);
            this.MnuToggleCheckerboard.Text = "[Checkerboard]";
            this.MnuToggleCheckerboard.Click += new System.EventHandler(this.MnuToggleCheckerboard_Click);
            // 
            // toolStripMenuItem20
            // 
            this.toolStripMenuItem20.Name = "toolStripMenuItem20";
            this.toolStripMenuItem20.Size = new System.Drawing.Size(570, 6);
            // 
            // MnuToggleTopMost
            // 
            this.MnuToggleTopMost.CheckOnClick = true;
            this.MnuToggleTopMost.Name = "MnuToggleTopMost";
            this.MnuToggleTopMost.Size = new System.Drawing.Size(573, 54);
            this.MnuToggleTopMost.Text = "[Keep window always on top]";
            this.MnuToggleTopMost.Click += new System.EventHandler(this.MnuToggleTopMost_Click);
            // 
            // MnuTools
            // 
            this.MnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuColorPicker,
            this.MnuCropTool,
            this.MnuPageNav,
            this.MnuExifTool});
            this.MnuTools.Image = ((System.Drawing.Image)(resources.GetObject("MnuTools.Image")));
            this.MnuTools.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuTools.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuTools.Name = "MnuTools";
            this.MnuTools.Size = new System.Drawing.Size(276, 48);
            this.MnuTools.Text = "[Tools]";
            // 
            // MnuColorPicker
            // 
            this.MnuColorPicker.Name = "MnuColorPicker";
            this.MnuColorPicker.Size = new System.Drawing.Size(413, 54);
            this.MnuColorPicker.Text = "[Color picker]";
            this.MnuColorPicker.Click += new System.EventHandler(this.MnuColorPicker_Click);
            // 
            // MnuCropTool
            // 
            this.MnuCropTool.Name = "MnuCropTool";
            this.MnuCropTool.Size = new System.Drawing.Size(413, 54);
            this.MnuCropTool.Text = "[Cropping]";
            this.MnuCropTool.Click += new System.EventHandler(this.MnuCropTool_Click);
            // 
            // MnuPageNav
            // 
            this.MnuPageNav.Name = "MnuPageNav";
            this.MnuPageNav.Size = new System.Drawing.Size(413, 54);
            this.MnuPageNav.Text = "[Page navigation]";
            this.MnuPageNav.Click += new System.EventHandler(this.MnuPageNav_Click);
            // 
            // MnuExifTool
            // 
            this.MnuExifTool.Name = "MnuExifTool";
            this.MnuExifTool.Size = new System.Drawing.Size(413, 54);
            this.MnuExifTool.Text = "[Exif tool]";
            this.MnuExifTool.Click += new System.EventHandler(this.MnuExifTool_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(273, 6);
            // 
            // MnuSettings
            // 
            this.MnuSettings.Image = ((System.Drawing.Image)(resources.GetObject("MnuSettings.Image")));
            this.MnuSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuSettings.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuSettings.Name = "MnuSettings";
            this.MnuSettings.Size = new System.Drawing.Size(276, 48);
            this.MnuSettings.Text = "[Settings]";
            this.MnuSettings.Click += new System.EventHandler(this.MnuSettings_Click);
            // 
            // MnuHelp
            // 
            this.MnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuAbout,
            this.MnuCheckForUpdate,
            this.MnuReportIssue,
            this.MnuFirstLaunch,
            this.toolStripMenuItem22,
            this.MnuSetDefaultPhotoViewer,
            this.MnuUnsetDefaultPhotoViewer});
            this.MnuHelp.Image = ((System.Drawing.Image)(resources.GetObject("MnuHelp.Image")));
            this.MnuHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuHelp.Name = "MnuHelp";
            this.MnuHelp.Size = new System.Drawing.Size(276, 48);
            this.MnuHelp.Text = "[Help]";
            // 
            // MnuAbout
            // 
            this.MnuAbout.Name = "MnuAbout";
            this.MnuAbout.Size = new System.Drawing.Size(562, 54);
            this.MnuAbout.Text = "[About]";
            this.MnuAbout.Click += new System.EventHandler(this.MnuAbout_Click);
            // 
            // MnuCheckForUpdate
            // 
            this.MnuCheckForUpdate.Name = "MnuCheckForUpdate";
            this.MnuCheckForUpdate.Size = new System.Drawing.Size(562, 54);
            this.MnuCheckForUpdate.Text = "[A new version is available]";
            this.MnuCheckForUpdate.Click += new System.EventHandler(this.MnuCheckForUpdate_Click);
            // 
            // MnuReportIssue
            // 
            this.MnuReportIssue.Name = "MnuReportIssue";
            this.MnuReportIssue.Size = new System.Drawing.Size(562, 54);
            this.MnuReportIssue.Text = "[Report an issue]";
            this.MnuReportIssue.Click += new System.EventHandler(this.MnuReportIssue_Click);
            // 
            // MnuFirstLaunch
            // 
            this.MnuFirstLaunch.Name = "MnuFirstLaunch";
            this.MnuFirstLaunch.Size = new System.Drawing.Size(562, 54);
            this.MnuFirstLaunch.Text = "[First-launch configurations]";
            this.MnuFirstLaunch.Click += new System.EventHandler(this.MnuFirstLaunch_Click);
            // 
            // toolStripMenuItem22
            // 
            this.toolStripMenuItem22.Name = "toolStripMenuItem22";
            this.toolStripMenuItem22.Size = new System.Drawing.Size(559, 6);
            // 
            // MnuSetDefaultPhotoViewer
            // 
            this.MnuSetDefaultPhotoViewer.Name = "MnuSetDefaultPhotoViewer";
            this.MnuSetDefaultPhotoViewer.Size = new System.Drawing.Size(562, 54);
            this.MnuSetDefaultPhotoViewer.Text = "[Set as default photo viewer]";
            this.MnuSetDefaultPhotoViewer.Click += new System.EventHandler(this.MnuSetDefaultPhotoViewer_Click);
            // 
            // MnuUnsetDefaultPhotoViewer
            // 
            this.MnuUnsetDefaultPhotoViewer.Name = "MnuUnsetDefaultPhotoViewer";
            this.MnuUnsetDefaultPhotoViewer.Size = new System.Drawing.Size(562, 54);
            this.MnuUnsetDefaultPhotoViewer.Text = "[Unset default photo viewer]";
            this.MnuUnsetDefaultPhotoViewer.Click += new System.EventHandler(this.MnuUnsetDefaultPhotoViewer_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(273, 6);
            // 
            // MnuExit
            // 
            this.MnuExit.Image = ((System.Drawing.Image)(resources.GetObject("MnuExit.Image")));
            this.MnuExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MnuExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MnuExit.Name = "MnuExit";
            this.MnuExit.Size = new System.Drawing.Size(276, 48);
            this.MnuExit.Text = "[Exit]";
            this.MnuExit.Click += new System.EventHandler(this.MnuExit_Click);
            // 
            // Tb0
            // 
            this.Tb0.ColumnCount = 1;
            this.Tb0.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tb0.Controls.Add(this.Toolbar, 0, 0);
            this.Tb0.Controls.Add(this.Sp1, 0, 1);
            this.Tb0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tb0.Location = new System.Drawing.Point(0, 0);
            this.Tb0.Margin = new System.Windows.Forms.Padding(2);
            this.Tb0.Name = "Tb0";
            this.Tb0.RowCount = 2;
            this.Tb0.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.Tb0.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.Tb0.Size = new System.Drawing.Size(1266, 723);
            this.Tb0.TabIndex = 2;
            // 
            // Sp1
            // 
            this.Sp1.BackColor = System.Drawing.Color.Transparent;
            this.Sp1.Cursor = System.Windows.Forms.Cursors.SizeNS;
            this.Sp1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Sp1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.Sp1.IsSplitterFixed = true;
            this.Sp1.Location = new System.Drawing.Point(0, 25);
            this.Sp1.Margin = new System.Windows.Forms.Padding(0);
            this.Sp1.Name = "Sp1";
            this.Sp1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // Sp1.Panel1
            // 
            this.Sp1.Panel1.Controls.Add(this.Sp2);
            this.Sp1.Panel1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Sp1.Panel1MinSize = 10;
            // 
            // Sp1.Panel2
            // 
            this.Sp1.Panel2.BackColor = System.Drawing.Color.RosyBrown;
            this.Sp1.Panel2.Controls.Add(this.Gallery);
            this.Sp1.Panel2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Sp1.Panel2MinSize = 0;
            this.Sp1.Size = new System.Drawing.Size(1266, 698);
            this.Sp1.SplitterBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.Sp1.SplitterDistance = 672;
            this.Sp1.SplitterWidth = 1;
            this.Sp1.TabIndex = 98;
            this.Sp1.TabStop = false;
            // 
            // Sp2
            // 
            this.Sp2.BackColor = System.Drawing.Color.Transparent;
            this.Sp2.Cursor = System.Windows.Forms.Cursors.SizeWE;
            this.Sp2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Sp2.IsSplitterFixed = true;
            this.Sp2.Location = new System.Drawing.Point(0, 0);
            this.Sp2.Margin = new System.Windows.Forms.Padding(0);
            this.Sp2.Name = "Sp2";
            // 
            // Sp2.Panel1
            // 
            this.Sp2.Panel1.Controls.Add(this.PicMain);
            this.Sp2.Panel1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Sp2.Panel1MinSize = 0;
            // 
            // Sp2.Panel2
            // 
            this.Sp2.Panel2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Sp2.Panel2MinSize = 10;
            this.Sp2.Size = new System.Drawing.Size(1266, 672);
            this.Sp2.SplitterBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Sp2.SplitterDistance = 833;
            this.Sp2.SplitterWidth = 11;
            this.Sp2.TabIndex = 99;
            this.Sp2.TabStop = false;
            // 
            // PicMain
            // 
            this.PicMain.AllowDrop = true;
            this.PicMain.BackColor = System.Drawing.SystemColors.Control;
            this.PicMain.BaseDpi = 96F;
            this.PicMain.CheckerboardMode = ImageGlass.Base.PhotoBox.CheckerboardMode.Client;
            this.PicMain.CheckFPS = false;
            this.PicMain.ClientSelection = ((System.Drawing.RectangleF)(resources.GetObject("PicMain.ClientSelection")));
            this.PicMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicMain.EnableSelection = false;
            this.PicMain.InterpolationScaleDown = ImageGlass.Base.PhotoBox.ImageInterpolation.SampleLinear;
            this.PicMain.Location = new System.Drawing.Point(0, 0);
            this.PicMain.MessageBorderRadius = 6F;
            this.PicMain.Name = "PicMain";
            this.PicMain.NavButtonSize = new System.Drawing.SizeF(70F, 70F);
            this.PicMain.NavDisplay = ImageGlass.Base.PhotoBox.NavButtonDisplay.Both;
            this.PicMain.RequestUpdateFrame = false;
            this.PicMain.SelectionAspectRatio = new System.Drawing.SizeF(0F, 0F);
            this.PicMain.AccentColor = System.Drawing.Color.Black;
            this.PicMain.Size = new System.Drawing.Size(833, 672);
            this.PicMain.SourceSelection = ((System.Drawing.RectangleF)(resources.GetObject("PicMain.SourceSelection")));
            this.PicMain.TabIndex = 3;
            this.PicMain.OnZoomChanged += new ImageGlass.Views.DXCanvas.ZoomChangedEventHandler(this.PicMain_OnZoomChanged);
            this.PicMain.OnNavLeftClicked += new ImageGlass.Views.DXCanvas.NavLeftClickedEventHandler(this.PicMain_OnNavLeftClicked);
            this.PicMain.OnNavRightClicked += new ImageGlass.Views.DXCanvas.NavRightClickedEventHandler(this.PicMain_OnNavRightClicked);
            this.PicMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.PicMain_DragDrop);
            this.PicMain.DragOver += new System.Windows.Forms.DragEventHandler(this.PicMain_DragOver);
            this.PicMain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PicMain_KeyDown);
            this.PicMain.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PicMain_KeyUp);
            this.PicMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PicMain_MouseClick);
            this.PicMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PicMain_MouseDoubleClick);
            this.PicMain.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.PicMain_MouseWheel);
            // 
            // Gallery
            // 
            this.Gallery.CheckBoxAlignment = System.Drawing.ContentAlignment.TopLeft;
            this.Gallery.CheckBoxPadding = new System.Drawing.Size(6, 6);
            this.Gallery.Dock = System.Windows.Forms.DockStyle.Top;
            this.Gallery.EnableKeyNavigation = true;
            this.Gallery.EnableTransparent = true;
            this.Gallery.Location = new System.Drawing.Point(0, 0);
            this.Gallery.Margin = new System.Windows.Forms.Padding(0);
            this.Gallery.Name = "Gallery";
            this.Gallery.PersistentCacheDirectory = "";
            this.Gallery.PersistentCacheSize = ((long)(100));
            this.Gallery.RetryOnError = false;
            this.Gallery.Size = new System.Drawing.Size(1266, 25);
            this.Gallery.TabIndex = 0;
            this.Gallery.ThumbnailSize = new System.Drawing.Size(70, 70);
            this.Gallery.View = ImageGlass.Gallery.View.HorizontalStrip;
            this.Gallery.ItemClick += new ImageGlass.Gallery.ItemClickEventHandler(this.Gallery_ItemClick);
            this.Gallery.ItemTooltipShowing += new System.EventHandler<ImageGlass.Gallery.ItemTooltipShowingEventArgs>(this.Gallery_ItemTooltipShowing);
            // 
            // MnuContext
            // 
            this.MnuContext.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.MnuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemToPreserveTheSpaceToolStripMenuItem});
            this.MnuContext.Name = "MnuContext";
            this.MnuContext.Size = new System.Drawing.Size(448, 52);
            this.MnuContext.Opening += new System.ComponentModel.CancelEventHandler(this.MnuContext_Opening);
            // 
            // itemToPreserveTheSpaceToolStripMenuItem
            // 
            this.itemToPreserveTheSpaceToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.itemToPreserveTheSpaceToolStripMenuItem.Name = "itemToPreserveTheSpaceToolStripMenuItem";
            this.itemToPreserveTheSpaceToolStripMenuItem.Size = new System.Drawing.Size(447, 48);
            this.itemToPreserveTheSpaceToolStripMenuItem.Text = "item to preserve the space";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(188, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(188, 6);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(188, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(188, 6);
            // 
            // MnuSubMenu
            // 
            this.MnuSubMenu.ImageScalingSize = new System.Drawing.Size(22, 22);
            this.MnuSubMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemToPreserveSpaceToolStripMenuItem});
            this.MnuSubMenu.Name = "MnuContext";
            this.MnuSubMenu.Size = new System.Drawing.Size(397, 52);
            // 
            // itemToPreserveSpaceToolStripMenuItem
            // 
            this.itemToPreserveSpaceToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.itemToPreserveSpaceToolStripMenuItem.Name = "itemToPreserveSpaceToolStripMenuItem";
            this.itemToPreserveSpaceToolStripMenuItem.Size = new System.Drawing.Size(396, 48);
            this.itemToPreserveSpaceToolStripMenuItem.Text = "item to preserve space";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(240F, 240F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1266, 723);
            this.Controls.Add(this.Tb0);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmMain";
            this.RightToLeftLayout = true;
            this.Text = "ImageGlass";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmMain_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FrmMain_KeyUp);
            this.MnuMain.ResumeLayout(false);
            this.Tb0.ResumeLayout(false);
            this.Tb0.PerformLayout();
            this.Sp1.Panel1.ResumeLayout(false);
            this.Sp1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Sp1)).EndInit();
            this.Sp1.ResumeLayout(false);
            this.Sp2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Sp2)).EndInit();
            this.Sp2.ResumeLayout(false);
            this.MnuContext.ResumeLayout(false);
            this.MnuSubMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public ToolStripMenuItem MnuNavigation;
        public ToolStripMenuItem MnuZoom;
        public ToolStripMenuItem MnuImage;
        public ToolStripMenuItem MnuClipboard;
        private ToolStripSeparator toolStripMenuItem1;
        public ToolStripMenuItem MnuFrameless;
        public ToolStripMenuItem MnuFullScreen;
        public ToolStripMenuItem MnuSlideshow;
        private ToolStripSeparator toolStripMenuItem2;
        public ToolStripMenuItem MnuLayout;
        public ToolStripMenuItem MnuTools;
        private ToolStripSeparator toolStripMenuItem3;
        public ToolStripMenuItem MnuSettings;
        public ToolStripMenuItem MnuHelp;
        private ToolStripSeparator toolStripMenuItem4;
        public ToolStripMenuItem MnuExit;
        public ToolStripMenuItem MnuFile;
        private ToolStripSeparator toolStripMenuItem6;
        public ToolStripMenuItem MnuWindowFit;
        private ToolStripSeparator toolStripMenuItem7;
        private ToolStripSeparator toolStripMenuItem8;
        private ToolStripSeparator toolStripMenuItem9;
        public ToolStripMenuItem MnuOpenFile;
        public ToolStripMenuItem MnuPasteImage;
        public ToolStripMenuItem MnuNewWindow;
        public ToolStripMenuItem MnuSave;
        public ToolStripMenuItem MnuSaveAs;
        private ToolStripSeparator toolStripMenuItem10;
        public ToolStripMenuItem MnuOpenWith;
        public ToolStripMenuItem MnuEdit;
        public ToolStripMenuItem MnuPrint;
        private ToolStripSeparator toolStripMenuItem12;
        public ToolStripMenuItem MnuRefresh;
        public ToolStripMenuItem MnuReload;
        public ToolStripMenuItem MnuReloadImageList;
        public ToolStripMenuItem MnuViewNext;
        public ToolStripMenuItem MnuViewPrevious;
        private ToolStripSeparator toolStripMenuItem13;
        public ToolStripMenuItem MnuGoTo;
        public ToolStripMenuItem MnuGoToFirst;
        public ToolStripMenuItem MnuGoToLast;
        private ToolStripSeparator toolStripMenuItem14;
        public ToolStripMenuItem MnuViewNextFrame;
        public ToolStripMenuItem MnuViewPreviousFrame;
        public ToolStripMenuItem MnuViewFirstFrame;
        public ToolStripMenuItem MnuViewLastFrame;
        public ToolStripMenuItem MnuZoomIn;
        public ToolStripMenuItem MnuZoomOut;
        public ToolStripMenuItem MnuCustomZoom;
        public ToolStripMenuItem MnuActualSize;
        private ToolStripSeparator toolStripMenuItem15;
        public ToolStripMenuItem MnuAutoZoom;
        public ToolStripMenuItem MnuLockZoom;
        public ToolStripMenuItem MnuScaleToWidth;
        public ToolStripMenuItem MnuScaleToHeight;
        public ToolStripMenuItem MnuScaleToFit;
        public ToolStripMenuItem MnuScaleToFill;
        public ToolStripMenuItem MnuViewChannels;
        public ToolStripMenuItem MnuLoadingOrders;
        private ToolStripSeparator toolStripMenuItem16;
        public ToolStripMenuItem MnuRotateLeft;
        public ToolStripMenuItem MnuRotateRight;
        public ToolStripMenuItem MnuFlipHorizontal;
        public ToolStripMenuItem MnuFlipVertical;
        private ToolStripSeparator toolStripMenuItem17;
        public ToolStripMenuItem MnuRename;
        public ToolStripMenuItem MnuMoveToRecycleBin;
        public ToolStripMenuItem MnuDeleteFromHardDisk;
        private ToolStripSeparator toolStripMenuItem18;
        public ToolStripMenuItem MnuToggleImageAnimation;
        public ToolStripMenuItem MnuExtractFrames;
        public ToolStripMenuItem MnuSetDesktopBackground;
        public ToolStripMenuItem MnuSetLockScreen;
        public ToolStripMenuItem MnuOpenLocation;
        public ToolStripMenuItem MnuImageProperties;
        public ToolStripMenuItem MnuCopyImageData;
        public ToolStripMenuItem MnuCopy;
        public ToolStripMenuItem MnuCut;
        private ToolStripSeparator toolStripMenuItem19;
        private ToolStripSeparator toolStripMenuItem23;
        public ToolStripMenuItem MnuCopyPath;
        public ToolStripMenuItem MnuClearClipboard;
        public ToolStripMenuItem MnuStartSlideshow;
        public ToolStripMenuItem MnuCloseAllSlideshows;
        public ToolStripMenuItem MnuToggleToolbar;
        public ToolStripMenuItem MnuToggleThumbnails;
        public ToolStripMenuItem MnuToggleCheckerboard;
        private ToolStripSeparator toolStripMenuItem20;
        public ToolStripMenuItem MnuToggleTopMost;
        public ToolStripMenuItem MnuColorPicker;
        public ToolStripMenuItem MnuCropTool;
        public ToolStripMenuItem MnuPageNav;
        public ToolStripMenuItem MnuExifTool;
        public ToolStripMenuItem MnuAbout;
        public ToolStripMenuItem MnuCheckForUpdate;
        public ToolStripMenuItem MnuReportIssue;
        public ToolStripMenuItem MnuFirstLaunch;
        private ToolStripSeparator toolStripSeparator1;
        public ToolStripMenuItem itemToPreserveTheSpaceToolStripMenuItem;
        public ToolStripMenuItem itemToPreserveSpaceToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem22;
        public ToolStripMenuItem MnuUnsetDefaultPhotoViewer;
        public ToolStripMenuItem MnuSetDefaultPhotoViewer;
        public ToolStripMenuItem MnuShare;
        public ToolStripMenuItem MnuPanning;
        public ToolStripMenuItem MnuPanLeft;
        public ToolStripMenuItem MnuPanRight;
        public ToolStripMenuItem MnuPanUp;
        public ToolStripMenuItem MnuPanDown;
        private ToolStripSeparator toolStripSeparator3;
        public ToolStripMenuItem MnuPanToLeftSide;
        public ToolStripMenuItem MnuPanToRightSide;
        public ToolStripMenuItem MnuPanToTop;
        public ToolStripMenuItem MnuPanToBottom;
        public Views.DXCanvas PicMain;
        public UI.ModernToolbar Toolbar;
        public Gallery.ImageGallery Gallery;
        public UI.ModernMenu MnuMain;
        public UI.ModernMenu MnuContext;
        public UI.ModernMenu MnuSubMenu;
        public TableLayoutPanel Tb0;
        public UI.ModernSplitContainer Sp1;
        public UI.ModernSplitContainer Sp2;
        public ToolStripMenuItem MnuUnload;
    }
}