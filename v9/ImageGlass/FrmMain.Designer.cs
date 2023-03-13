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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            Toolbar = new UI.ModernToolbar();
            MnuMain = new UI.ModernMenu(components);
            MnuFile = new ToolStripMenuItem();
            MnuOpenFile = new ToolStripMenuItem();
            MnuNewWindow = new ToolStripMenuItem();
            MnuSave = new ToolStripMenuItem();
            MnuSaveAs = new ToolStripMenuItem();
            toolStripMenuItem10 = new ToolStripSeparator();
            MnuOpenWith = new ToolStripMenuItem();
            MnuEdit = new ToolStripMenuItem();
            MnuPrint = new ToolStripMenuItem();
            MnuShare = new ToolStripMenuItem();
            toolStripMenuItem12 = new ToolStripSeparator();
            MnuRefresh = new ToolStripMenuItem();
            MnuReload = new ToolStripMenuItem();
            MnuReloadImageList = new ToolStripMenuItem();
            MnuUnload = new ToolStripMenuItem();
            MnuNavigation = new ToolStripMenuItem();
            MnuViewNext = new ToolStripMenuItem();
            MnuViewPrevious = new ToolStripMenuItem();
            toolStripMenuItem13 = new ToolStripSeparator();
            MnuGoTo = new ToolStripMenuItem();
            MnuGoToFirst = new ToolStripMenuItem();
            MnuGoToLast = new ToolStripMenuItem();
            toolStripMenuItem14 = new ToolStripSeparator();
            MnuViewNextFrame = new ToolStripMenuItem();
            MnuViewPreviousFrame = new ToolStripMenuItem();
            MnuViewFirstFrame = new ToolStripMenuItem();
            MnuViewLastFrame = new ToolStripMenuItem();
            MnuZoom = new ToolStripMenuItem();
            MnuZoomIn = new ToolStripMenuItem();
            MnuZoomOut = new ToolStripMenuItem();
            MnuCustomZoom = new ToolStripMenuItem();
            MnuActualSize = new ToolStripMenuItem();
            toolStripMenuItem15 = new ToolStripSeparator();
            MnuAutoZoom = new ToolStripMenuItem();
            MnuLockZoom = new ToolStripMenuItem();
            MnuScaleToWidth = new ToolStripMenuItem();
            MnuScaleToHeight = new ToolStripMenuItem();
            MnuScaleToFit = new ToolStripMenuItem();
            MnuScaleToFill = new ToolStripMenuItem();
            MnuPanning = new ToolStripMenuItem();
            MnuPanLeft = new ToolStripMenuItem();
            MnuPanRight = new ToolStripMenuItem();
            MnuPanUp = new ToolStripMenuItem();
            MnuPanDown = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            MnuPanToLeftSide = new ToolStripMenuItem();
            MnuPanToRightSide = new ToolStripMenuItem();
            MnuPanToTop = new ToolStripMenuItem();
            MnuPanToBottom = new ToolStripMenuItem();
            MnuImage = new ToolStripMenuItem();
            MnuViewChannels = new ToolStripMenuItem();
            MnuLoadingOrders = new ToolStripMenuItem();
            toolStripMenuItem16 = new ToolStripSeparator();
            MnuRotateLeft = new ToolStripMenuItem();
            MnuRotateRight = new ToolStripMenuItem();
            MnuFlipHorizontal = new ToolStripMenuItem();
            MnuFlipVertical = new ToolStripMenuItem();
            toolStripMenuItem17 = new ToolStripSeparator();
            MnuRename = new ToolStripMenuItem();
            MnuMoveToRecycleBin = new ToolStripMenuItem();
            MnuDeleteFromHardDisk = new ToolStripMenuItem();
            toolStripMenuItem18 = new ToolStripSeparator();
            MnuToggleImageAnimation = new ToolStripMenuItem();
            MnuExportFrames = new ToolStripMenuItem();
            MnuSetDesktopBackground = new ToolStripMenuItem();
            MnuSetLockScreen = new ToolStripMenuItem();
            MnuOpenLocation = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            MnuImageProperties = new ToolStripMenuItem();
            MnuClipboard = new ToolStripMenuItem();
            MnuPasteImage = new ToolStripMenuItem();
            toolStripMenuItem19 = new ToolStripSeparator();
            MnuCopyImageData = new ToolStripMenuItem();
            MnuCopyPath = new ToolStripMenuItem();
            MnuCopyFile = new ToolStripMenuItem();
            MnuCutFile = new ToolStripMenuItem();
            toolStripMenuItem23 = new ToolStripSeparator();
            MnuClearClipboard = new ToolStripMenuItem();
            toolStripMenuItem6 = new ToolStripSeparator();
            MnuWindowFit = new ToolStripMenuItem();
            MnuFrameless = new ToolStripMenuItem();
            MnuFullScreen = new ToolStripMenuItem();
            MnuSlideshow = new ToolStripMenuItem();
            MnuStartSlideshow = new ToolStripMenuItem();
            MnuCloseAllSlideshows = new ToolStripMenuItem();
            toolStripMenuItem7 = new ToolStripSeparator();
            MnuLayout = new ToolStripMenuItem();
            MnuToggleToolbar = new ToolStripMenuItem();
            MnuToggleThumbnails = new ToolStripMenuItem();
            MnuToggleCheckerboard = new ToolStripMenuItem();
            toolStripMenuItem20 = new ToolStripSeparator();
            MnuToggleTopMost = new ToolStripMenuItem();
            MnuTools = new ToolStripMenuItem();
            MnuColorPicker = new ToolStripMenuItem();
            MnuCropTool = new ToolStripMenuItem();
            MnuPageNav = new ToolStripMenuItem();
            toolStripMenuItem8 = new ToolStripSeparator();
            MnuSettings = new ToolStripMenuItem();
            MnuHelp = new ToolStripMenuItem();
            MnuAbout = new ToolStripMenuItem();
            MnuCheckForUpdate = new ToolStripMenuItem();
            MnuReportIssue = new ToolStripMenuItem();
            MnuFirstLaunch = new ToolStripMenuItem();
            toolStripMenuItem22 = new ToolStripSeparator();
            MnuSetDefaultPhotoViewer = new ToolStripMenuItem();
            MnuUnsetDefaultPhotoViewer = new ToolStripMenuItem();
            toolStripMenuItem9 = new ToolStripSeparator();
            MnuExit = new ToolStripMenuItem();
            PicMain = new Views.DXCanvas();
            Gallery = new Gallery.ImageGallery();
            MnuContext = new UI.ModernMenu(components);
            itemToPreserveTheSpaceToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            toolStripMenuItem2 = new ToolStripSeparator();
            toolStripMenuItem3 = new ToolStripSeparator();
            toolStripMenuItem4 = new ToolStripSeparator();
            MnuSubMenu = new UI.ModernMenu(components);
            itemToPreserveSpaceToolStripMenuItem = new ToolStripMenuItem();
            MnuMain.SuspendLayout();
            MnuContext.SuspendLayout();
            MnuSubMenu.SuspendLayout();
            SuspendLayout();
            // 
            // Toolbar
            // 
            Toolbar.Alignment = UI.ToolbarAlignment.Center;
            Toolbar.AutoFocusOnHover = true;
            Toolbar.BackColor = Color.FromArgb(192, 255, 255);
            Toolbar.EnableTransparent = true;
            Toolbar.GripMargin = new Padding(0);
            Toolbar.GripStyle = ToolStripGripStyle.Hidden;
            Toolbar.HideTooltips = false;
            Toolbar.IconHeight = 22;
            Toolbar.ImageScalingSize = new Size(30, 30);
            Toolbar.Location = new Point(0, 0);
            Toolbar.MainMenu = MnuMain;
            Toolbar.Name = "Toolbar";
            Toolbar.RenderMode = ToolStripRenderMode.System;
            Toolbar.ShowItemToolTips = false;
            Toolbar.ShowMainMenuButton = false;
            Toolbar.Size = new Size(1661, 25);
            Toolbar.TabIndex = 1;
            Toolbar.Theme = null;
            Toolbar.ToolTipDirection = UI.TooltipDirection.Bottom;
            Toolbar.ToolTipText = "";
            Toolbar.ItemClicked += Toolbar_ItemClicked;
            // 
            // MnuMain
            // 
            MnuMain.CurrentDpi = 96;
            MnuMain.ImageScalingSize = new Size(40, 40);
            MnuMain.Items.AddRange(new ToolStripItem[] { MnuFile, MnuNavigation, MnuZoom, MnuPanning, MnuImage, MnuClipboard, toolStripMenuItem6, MnuWindowFit, MnuFrameless, MnuFullScreen, MnuSlideshow, toolStripMenuItem7, MnuLayout, MnuTools, toolStripMenuItem8, MnuSettings, MnuHelp, toolStripMenuItem9, MnuExit });
            MnuMain.Name = "MnuContext";
            MnuMain.RenderMode = ToolStripRenderMode.System;
            MnuMain.Size = new Size(294, 808);
            MnuMain.Opening += MnuMain_Opening;
            // 
            // MnuFile
            // 
            MnuFile.DropDownItems.AddRange(new ToolStripItem[] { MnuOpenFile, MnuNewWindow, MnuSave, MnuSaveAs, toolStripMenuItem10, MnuOpenWith, MnuEdit, MnuPrint, MnuShare, toolStripMenuItem12, MnuRefresh, MnuReload, MnuReloadImageList, MnuUnload });
            MnuFile.ImageScaling = ToolStripItemImageScaling.None;
            MnuFile.Name = "MnuFile";
            MnuFile.Size = new Size(293, 52);
            MnuFile.Text = "[File]";
            // 
            // MnuOpenFile
            // 
            MnuOpenFile.Name = "MnuOpenFile";
            MnuOpenFile.Size = new Size(472, 54);
            MnuOpenFile.Text = "[Open file...]";
            MnuOpenFile.Click += MnuOpenFile_Click;
            // 
            // MnuNewWindow
            // 
            MnuNewWindow.Name = "MnuNewWindow";
            MnuNewWindow.Size = new Size(472, 54);
            MnuNewWindow.Text = "[Open new window]";
            MnuNewWindow.Click += MnuNewWindow_Click;
            // 
            // MnuSave
            // 
            MnuSave.Name = "MnuSave";
            MnuSave.Size = new Size(472, 54);
            MnuSave.Text = "[Save image]";
            MnuSave.Click += MnuSave_Click;
            // 
            // MnuSaveAs
            // 
            MnuSaveAs.Name = "MnuSaveAs";
            MnuSaveAs.Size = new Size(472, 54);
            MnuSaveAs.Text = "[Save image as...]";
            MnuSaveAs.Click += MnuSaveAs_Click;
            // 
            // toolStripMenuItem10
            // 
            toolStripMenuItem10.Name = "toolStripMenuItem10";
            toolStripMenuItem10.Size = new Size(469, 6);
            // 
            // MnuOpenWith
            // 
            MnuOpenWith.Name = "MnuOpenWith";
            MnuOpenWith.Size = new Size(472, 54);
            MnuOpenWith.Text = "[Open with...]";
            MnuOpenWith.Click += MnuOpenWith_Click;
            // 
            // MnuEdit
            // 
            MnuEdit.Name = "MnuEdit";
            MnuEdit.Size = new Size(472, 54);
            MnuEdit.Text = "[Edit image...]";
            MnuEdit.Click += MnuEdit_Click;
            // 
            // MnuPrint
            // 
            MnuPrint.Name = "MnuPrint";
            MnuPrint.Size = new Size(472, 54);
            MnuPrint.Text = "[Print...]";
            MnuPrint.Click += MnuPrint_Click;
            // 
            // MnuShare
            // 
            MnuShare.Name = "MnuShare";
            MnuShare.Size = new Size(472, 54);
            MnuShare.Text = "[Share...]";
            MnuShare.Click += MnuShare_Click;
            // 
            // toolStripMenuItem12
            // 
            toolStripMenuItem12.Name = "toolStripMenuItem12";
            toolStripMenuItem12.Size = new Size(469, 6);
            // 
            // MnuRefresh
            // 
            MnuRefresh.Name = "MnuRefresh";
            MnuRefresh.Size = new Size(472, 54);
            MnuRefresh.Text = "[Refresh]";
            MnuRefresh.Click += MnuRefresh_Click;
            // 
            // MnuReload
            // 
            MnuReload.Name = "MnuReload";
            MnuReload.Size = new Size(472, 54);
            MnuReload.Text = "[Reload image]";
            MnuReload.Click += MnuReload_Click;
            // 
            // MnuReloadImageList
            // 
            MnuReloadImageList.Name = "MnuReloadImageList";
            MnuReloadImageList.Size = new Size(472, 54);
            MnuReloadImageList.Text = "[Reload image list]";
            MnuReloadImageList.Click += MnuReloadImageList_Click;
            // 
            // MnuUnload
            // 
            MnuUnload.Name = "MnuUnload";
            MnuUnload.Size = new Size(472, 54);
            MnuUnload.Text = "[Unload image]";
            MnuUnload.Click += MnuUnload_Click;
            // 
            // MnuNavigation
            // 
            MnuNavigation.DropDownItems.AddRange(new ToolStripItem[] { MnuViewNext, MnuViewPrevious, toolStripMenuItem13, MnuGoTo, MnuGoToFirst, MnuGoToLast, toolStripMenuItem14, MnuViewNextFrame, MnuViewPreviousFrame, MnuViewFirstFrame, MnuViewLastFrame });
            MnuNavigation.Image = (Image)resources.GetObject("MnuNavigation.Image");
            MnuNavigation.ImageAlign = ContentAlignment.MiddleLeft;
            MnuNavigation.ImageScaling = ToolStripItemImageScaling.None;
            MnuNavigation.Name = "MnuNavigation";
            MnuNavigation.Size = new Size(293, 52);
            MnuNavigation.Text = "[Navigation]";
            // 
            // MnuViewNext
            // 
            MnuViewNext.Name = "MnuViewNext";
            MnuViewNext.Size = new Size(503, 54);
            MnuViewNext.Text = "[View next image]";
            MnuViewNext.Click += MnuViewNext_Click;
            // 
            // MnuViewPrevious
            // 
            MnuViewPrevious.Name = "MnuViewPrevious";
            MnuViewPrevious.Size = new Size(503, 54);
            MnuViewPrevious.Text = "[View previous image]";
            MnuViewPrevious.Click += MnuViewPrevious_Click;
            // 
            // toolStripMenuItem13
            // 
            toolStripMenuItem13.Name = "toolStripMenuItem13";
            toolStripMenuItem13.Size = new Size(500, 6);
            // 
            // MnuGoTo
            // 
            MnuGoTo.Name = "MnuGoTo";
            MnuGoTo.Size = new Size(503, 54);
            MnuGoTo.Text = "[Go to...]";
            MnuGoTo.Click += MnuGoTo_Click;
            // 
            // MnuGoToFirst
            // 
            MnuGoToFirst.Name = "MnuGoToFirst";
            MnuGoToFirst.Size = new Size(503, 54);
            MnuGoToFirst.Text = "[Go to the first image]";
            MnuGoToFirst.Click += MnuGoToFirst_Click;
            // 
            // MnuGoToLast
            // 
            MnuGoToLast.Name = "MnuGoToLast";
            MnuGoToLast.Size = new Size(503, 54);
            MnuGoToLast.Text = "[Go to the last image]";
            MnuGoToLast.Click += MnuGoToLast_Click;
            // 
            // toolStripMenuItem14
            // 
            toolStripMenuItem14.Name = "toolStripMenuItem14";
            toolStripMenuItem14.Size = new Size(500, 6);
            // 
            // MnuViewNextFrame
            // 
            MnuViewNextFrame.Name = "MnuViewNextFrame";
            MnuViewNextFrame.Size = new Size(503, 54);
            MnuViewNextFrame.Text = "[View next frame]";
            MnuViewNextFrame.Click += MnuViewNextFrame_Click;
            // 
            // MnuViewPreviousFrame
            // 
            MnuViewPreviousFrame.Name = "MnuViewPreviousFrame";
            MnuViewPreviousFrame.Size = new Size(503, 54);
            MnuViewPreviousFrame.Text = "[View previous frame]";
            MnuViewPreviousFrame.Click += MnuViewPreviousFrame_Click;
            // 
            // MnuViewFirstFrame
            // 
            MnuViewFirstFrame.Name = "MnuViewFirstFrame";
            MnuViewFirstFrame.Size = new Size(503, 54);
            MnuViewFirstFrame.Text = "[View the first frame]";
            MnuViewFirstFrame.Click += MnuViewFirstFrame_Click;
            // 
            // MnuViewLastFrame
            // 
            MnuViewLastFrame.Name = "MnuViewLastFrame";
            MnuViewLastFrame.Size = new Size(503, 54);
            MnuViewLastFrame.Text = "[View the last frame]";
            MnuViewLastFrame.Click += MnuViewLastFrame_Click;
            // 
            // MnuZoom
            // 
            MnuZoom.DropDownItems.AddRange(new ToolStripItem[] { MnuZoomIn, MnuZoomOut, MnuCustomZoom, MnuActualSize, toolStripMenuItem15, MnuAutoZoom, MnuLockZoom, MnuScaleToWidth, MnuScaleToHeight, MnuScaleToFit, MnuScaleToFill });
            MnuZoom.Image = (Image)resources.GetObject("MnuZoom.Image");
            MnuZoom.ImageAlign = ContentAlignment.MiddleLeft;
            MnuZoom.ImageScaling = ToolStripItemImageScaling.None;
            MnuZoom.Name = "MnuZoom";
            MnuZoom.Size = new Size(293, 52);
            MnuZoom.Text = "[Zoom]";
            // 
            // MnuZoomIn
            // 
            MnuZoomIn.Name = "MnuZoomIn";
            MnuZoomIn.Size = new Size(436, 54);
            MnuZoomIn.Text = "[Zoom in]";
            MnuZoomIn.Click += MnuZoomIn_Click;
            // 
            // MnuZoomOut
            // 
            MnuZoomOut.Name = "MnuZoomOut";
            MnuZoomOut.Size = new Size(436, 54);
            MnuZoomOut.Text = "[Zoom out]";
            MnuZoomOut.Click += MnuZoomOut_Click;
            // 
            // MnuCustomZoom
            // 
            MnuCustomZoom.Name = "MnuCustomZoom";
            MnuCustomZoom.Size = new Size(436, 54);
            MnuCustomZoom.Text = "[Custom zoom...]";
            MnuCustomZoom.Click += MnuCustomZoom_Click;
            // 
            // MnuActualSize
            // 
            MnuActualSize.Name = "MnuActualSize";
            MnuActualSize.Size = new Size(436, 54);
            MnuActualSize.Text = "[View actual size]";
            MnuActualSize.Click += MnuActualSize_Click;
            // 
            // toolStripMenuItem15
            // 
            toolStripMenuItem15.Name = "toolStripMenuItem15";
            toolStripMenuItem15.Size = new Size(433, 6);
            // 
            // MnuAutoZoom
            // 
            MnuAutoZoom.CheckOnClick = true;
            MnuAutoZoom.Name = "MnuAutoZoom";
            MnuAutoZoom.Size = new Size(436, 54);
            MnuAutoZoom.Tag = "";
            MnuAutoZoom.Text = "[Auto Zoom]";
            MnuAutoZoom.Click += MnuAutoZoom_Click;
            // 
            // MnuLockZoom
            // 
            MnuLockZoom.CheckOnClick = true;
            MnuLockZoom.Name = "MnuLockZoom";
            MnuLockZoom.Size = new Size(436, 54);
            MnuLockZoom.Tag = "";
            MnuLockZoom.Text = "[Lock zoom ratio]";
            MnuLockZoom.Click += MnuLockZoom_Click;
            // 
            // MnuScaleToWidth
            // 
            MnuScaleToWidth.CheckOnClick = true;
            MnuScaleToWidth.Name = "MnuScaleToWidth";
            MnuScaleToWidth.Size = new Size(436, 54);
            MnuScaleToWidth.Tag = "";
            MnuScaleToWidth.Text = "[Scale to width]";
            MnuScaleToWidth.Click += MnuScaleToWidth_Click;
            // 
            // MnuScaleToHeight
            // 
            MnuScaleToHeight.CheckOnClick = true;
            MnuScaleToHeight.Name = "MnuScaleToHeight";
            MnuScaleToHeight.Size = new Size(436, 54);
            MnuScaleToHeight.Tag = "";
            MnuScaleToHeight.Text = "[Scale to height]";
            MnuScaleToHeight.Click += MnuScaleToHeight_Click;
            // 
            // MnuScaleToFit
            // 
            MnuScaleToFit.CheckOnClick = true;
            MnuScaleToFit.Name = "MnuScaleToFit";
            MnuScaleToFit.Size = new Size(436, 54);
            MnuScaleToFit.Tag = "";
            MnuScaleToFit.Text = "[Scale to fit]";
            MnuScaleToFit.Click += MnuScaleToFit_Click;
            // 
            // MnuScaleToFill
            // 
            MnuScaleToFill.CheckOnClick = true;
            MnuScaleToFill.Name = "MnuScaleToFill";
            MnuScaleToFill.Size = new Size(436, 54);
            MnuScaleToFill.Tag = "";
            MnuScaleToFill.Text = "[Scale to fill]";
            MnuScaleToFill.Click += MnuScaleToFill_Click;
            // 
            // MnuPanning
            // 
            MnuPanning.DropDownItems.AddRange(new ToolStripItem[] { MnuPanLeft, MnuPanRight, MnuPanUp, MnuPanDown, toolStripSeparator3, MnuPanToLeftSide, MnuPanToRightSide, MnuPanToTop, MnuPanToBottom });
            MnuPanning.Image = (Image)resources.GetObject("MnuPanning.Image");
            MnuPanning.ImageAlign = ContentAlignment.MiddleLeft;
            MnuPanning.ImageScaling = ToolStripItemImageScaling.None;
            MnuPanning.Name = "MnuPanning";
            MnuPanning.Size = new Size(293, 52);
            MnuPanning.Text = "[Panning]";
            // 
            // MnuPanLeft
            // 
            MnuPanLeft.Name = "MnuPanLeft";
            MnuPanLeft.Size = new Size(593, 54);
            MnuPanLeft.Text = "[Pan image left]";
            MnuPanLeft.Click += MnuPanLeft_Click;
            // 
            // MnuPanRight
            // 
            MnuPanRight.Name = "MnuPanRight";
            MnuPanRight.Size = new Size(593, 54);
            MnuPanRight.Text = "[Pan image right]";
            MnuPanRight.Click += MnuPanRight_Click;
            // 
            // MnuPanUp
            // 
            MnuPanUp.Name = "MnuPanUp";
            MnuPanUp.Size = new Size(593, 54);
            MnuPanUp.Text = "[Pan image up]";
            MnuPanUp.Click += MnuPanUp_Click;
            // 
            // MnuPanDown
            // 
            MnuPanDown.Name = "MnuPanDown";
            MnuPanDown.Size = new Size(593, 54);
            MnuPanDown.Text = "[Pan image down]";
            MnuPanDown.Click += MnuPanDown_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(590, 6);
            // 
            // MnuPanToLeftSide
            // 
            MnuPanToLeftSide.Name = "MnuPanToLeftSide";
            MnuPanToLeftSide.Size = new Size(593, 54);
            MnuPanToLeftSide.Text = "[Pan image to the left side]";
            MnuPanToLeftSide.Click += MnuPanToLeftSide_Click;
            // 
            // MnuPanToRightSide
            // 
            MnuPanToRightSide.Name = "MnuPanToRightSide";
            MnuPanToRightSide.Size = new Size(593, 54);
            MnuPanToRightSide.Text = "[Pan image to the right side]";
            MnuPanToRightSide.Click += MnuPanToRightSide_Click;
            // 
            // MnuPanToTop
            // 
            MnuPanToTop.Name = "MnuPanToTop";
            MnuPanToTop.Size = new Size(593, 54);
            MnuPanToTop.Text = "[Pan image to the top]";
            MnuPanToTop.Click += MnuPanToTop_Click;
            // 
            // MnuPanToBottom
            // 
            MnuPanToBottom.Name = "MnuPanToBottom";
            MnuPanToBottom.Size = new Size(593, 54);
            MnuPanToBottom.Text = "[Pan image to the bottom]";
            MnuPanToBottom.Click += MnuPanToBottom_Click;
            // 
            // MnuImage
            // 
            MnuImage.DropDownItems.AddRange(new ToolStripItem[] { MnuViewChannels, MnuLoadingOrders, toolStripMenuItem16, MnuRotateLeft, MnuRotateRight, MnuFlipHorizontal, MnuFlipVertical, toolStripMenuItem17, MnuRename, MnuMoveToRecycleBin, MnuDeleteFromHardDisk, toolStripMenuItem18, MnuToggleImageAnimation, MnuExportFrames, MnuSetDesktopBackground, MnuSetLockScreen, MnuOpenLocation, toolStripSeparator1, MnuImageProperties });
            MnuImage.Image = (Image)resources.GetObject("MnuImage.Image");
            MnuImage.ImageAlign = ContentAlignment.MiddleLeft;
            MnuImage.ImageScaling = ToolStripItemImageScaling.None;
            MnuImage.Name = "MnuImage";
            MnuImage.Size = new Size(293, 52);
            MnuImage.Text = "[Image]";
            // 
            // MnuViewChannels
            // 
            MnuViewChannels.Name = "MnuViewChannels";
            MnuViewChannels.Size = new Size(614, 54);
            MnuViewChannels.Text = "[Channels]";
            // 
            // MnuLoadingOrders
            // 
            MnuLoadingOrders.Name = "MnuLoadingOrders";
            MnuLoadingOrders.Size = new Size(614, 54);
            MnuLoadingOrders.Text = "[Loading orders]";
            // 
            // toolStripMenuItem16
            // 
            toolStripMenuItem16.Name = "toolStripMenuItem16";
            toolStripMenuItem16.Size = new Size(611, 6);
            // 
            // MnuRotateLeft
            // 
            MnuRotateLeft.Name = "MnuRotateLeft";
            MnuRotateLeft.Size = new Size(614, 54);
            MnuRotateLeft.Text = "[Rotate counterclockwise]";
            MnuRotateLeft.Click += MnuRotateLeft_Click;
            // 
            // MnuRotateRight
            // 
            MnuRotateRight.Name = "MnuRotateRight";
            MnuRotateRight.Size = new Size(614, 54);
            MnuRotateRight.Text = "[Rotate Clockwise]";
            MnuRotateRight.Click += MnuRotateRight_Click;
            // 
            // MnuFlipHorizontal
            // 
            MnuFlipHorizontal.Name = "MnuFlipHorizontal";
            MnuFlipHorizontal.Size = new Size(614, 54);
            MnuFlipHorizontal.Text = "[Flip Horizontal]";
            MnuFlipHorizontal.Click += MnuFlipHorizontal_Click;
            // 
            // MnuFlipVertical
            // 
            MnuFlipVertical.Name = "MnuFlipVertical";
            MnuFlipVertical.Size = new Size(614, 54);
            MnuFlipVertical.Text = "[Flip Vertical]";
            MnuFlipVertical.Click += MnuFlipVertical_Click;
            // 
            // toolStripMenuItem17
            // 
            toolStripMenuItem17.Name = "toolStripMenuItem17";
            toolStripMenuItem17.Size = new Size(611, 6);
            // 
            // MnuRename
            // 
            MnuRename.Name = "MnuRename";
            MnuRename.Size = new Size(614, 54);
            MnuRename.Text = "[Rename image]";
            MnuRename.Click += MnuRename_Click;
            // 
            // MnuMoveToRecycleBin
            // 
            MnuMoveToRecycleBin.Name = "MnuMoveToRecycleBin";
            MnuMoveToRecycleBin.Size = new Size(614, 54);
            MnuMoveToRecycleBin.Text = "[Move to recycle bin]";
            MnuMoveToRecycleBin.Click += MnuMoveToRecycleBin_Click;
            // 
            // MnuDeleteFromHardDisk
            // 
            MnuDeleteFromHardDisk.Name = "MnuDeleteFromHardDisk";
            MnuDeleteFromHardDisk.Size = new Size(614, 54);
            MnuDeleteFromHardDisk.Text = "[Delete from hard disk]";
            MnuDeleteFromHardDisk.Click += MnuDeleteFromHardDisk_Click;
            // 
            // toolStripMenuItem18
            // 
            toolStripMenuItem18.Name = "toolStripMenuItem18";
            toolStripMenuItem18.Size = new Size(611, 6);
            // 
            // MnuToggleImageAnimation
            // 
            MnuToggleImageAnimation.Name = "MnuToggleImageAnimation";
            MnuToggleImageAnimation.Size = new Size(614, 54);
            MnuToggleImageAnimation.Text = "[Start / stop animating image]";
            MnuToggleImageAnimation.Click += MnuToggleImageAnimation_Click;
            // 
            // MnuExportFrames
            // 
            MnuExportFrames.Name = "MnuExportFrames";
            MnuExportFrames.Size = new Size(614, 54);
            MnuExportFrames.Text = "[Extract image frames]";
            MnuExportFrames.Click += MnuExportFrames_Click;
            // 
            // MnuSetDesktopBackground
            // 
            MnuSetDesktopBackground.Name = "MnuSetDesktopBackground";
            MnuSetDesktopBackground.Size = new Size(614, 54);
            MnuSetDesktopBackground.Text = "[Set as desktop background]";
            MnuSetDesktopBackground.Click += MnuSetDesktopBackground_Click;
            // 
            // MnuSetLockScreen
            // 
            MnuSetLockScreen.Name = "MnuSetLockScreen";
            MnuSetLockScreen.Size = new Size(614, 54);
            MnuSetLockScreen.Text = "[Set as Lock Screen image]";
            MnuSetLockScreen.Click += MnuSetLockScreen_Click;
            // 
            // MnuOpenLocation
            // 
            MnuOpenLocation.Name = "MnuOpenLocation";
            MnuOpenLocation.Size = new Size(614, 54);
            MnuOpenLocation.Text = "[Open image location]";
            MnuOpenLocation.Click += MnuOpenLocation_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(611, 6);
            // 
            // MnuImageProperties
            // 
            MnuImageProperties.Name = "MnuImageProperties";
            MnuImageProperties.Size = new Size(614, 54);
            MnuImageProperties.Text = "[Image properties]";
            MnuImageProperties.Click += MnuImageProperties_Click;
            // 
            // MnuClipboard
            // 
            MnuClipboard.DropDownItems.AddRange(new ToolStripItem[] { MnuPasteImage, toolStripMenuItem19, MnuCopyImageData, MnuCopyPath, MnuCopyFile, MnuCutFile, toolStripMenuItem23, MnuClearClipboard });
            MnuClipboard.Image = (Image)resources.GetObject("MnuClipboard.Image");
            MnuClipboard.ImageAlign = ContentAlignment.MiddleLeft;
            MnuClipboard.ImageScaling = ToolStripItemImageScaling.None;
            MnuClipboard.Name = "MnuClipboard";
            MnuClipboard.Size = new Size(293, 52);
            MnuClipboard.Text = "[Clipboard]";
            // 
            // MnuPasteImage
            // 
            MnuPasteImage.Name = "MnuPasteImage";
            MnuPasteImage.Size = new Size(468, 54);
            MnuPasteImage.Text = "[Paste image]";
            MnuPasteImage.Click += MnuPasteImage_Click;
            // 
            // toolStripMenuItem19
            // 
            toolStripMenuItem19.Name = "toolStripMenuItem19";
            toolStripMenuItem19.Size = new Size(465, 6);
            // 
            // MnuCopyImageData
            // 
            MnuCopyImageData.Name = "MnuCopyImageData";
            MnuCopyImageData.Size = new Size(468, 54);
            MnuCopyImageData.Text = "[Copy image pixels]";
            MnuCopyImageData.Click += MnuCopyImageData_Click;
            // 
            // MnuCopyPath
            // 
            MnuCopyPath.Name = "MnuCopyPath";
            MnuCopyPath.Size = new Size(468, 54);
            MnuCopyPath.Text = "[Copy image path]";
            MnuCopyPath.Click += MnuCopyPath_Click;
            // 
            // MnuCopyFile
            // 
            MnuCopyFile.Name = "MnuCopyFile";
            MnuCopyFile.Size = new Size(468, 54);
            MnuCopyFile.Text = "[Copy file]";
            MnuCopyFile.Click += MnuCopyFile_Click;
            // 
            // MnuCutFile
            // 
            MnuCutFile.Name = "MnuCutFile";
            MnuCutFile.Size = new Size(468, 54);
            MnuCutFile.Text = "[Cut file]";
            MnuCutFile.Click += MnuCutFile_Click;
            // 
            // toolStripMenuItem23
            // 
            toolStripMenuItem23.Name = "toolStripMenuItem23";
            toolStripMenuItem23.Size = new Size(465, 6);
            // 
            // MnuClearClipboard
            // 
            MnuClearClipboard.Name = "MnuClearClipboard";
            MnuClearClipboard.Size = new Size(468, 54);
            MnuClearClipboard.Text = "[Clear clipboard]";
            MnuClearClipboard.Click += MnuClearClipboard_Click;
            // 
            // toolStripMenuItem6
            // 
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new Size(290, 6);
            // 
            // MnuWindowFit
            // 
            MnuWindowFit.CheckOnClick = true;
            MnuWindowFit.ImageScaling = ToolStripItemImageScaling.None;
            MnuWindowFit.Name = "MnuWindowFit";
            MnuWindowFit.Size = new Size(293, 52);
            MnuWindowFit.Text = "[Window fit]";
            MnuWindowFit.Click += MnuWindowFit_Click;
            // 
            // MnuFrameless
            // 
            MnuFrameless.CheckOnClick = true;
            MnuFrameless.Image = (Image)resources.GetObject("MnuFrameless.Image");
            MnuFrameless.ImageAlign = ContentAlignment.MiddleLeft;
            MnuFrameless.ImageScaling = ToolStripItemImageScaling.None;
            MnuFrameless.Name = "MnuFrameless";
            MnuFrameless.Size = new Size(293, 52);
            MnuFrameless.Text = "[Frameless]";
            MnuFrameless.Click += MnuFrameless_Click;
            // 
            // MnuFullScreen
            // 
            MnuFullScreen.CheckOnClick = true;
            MnuFullScreen.Image = (Image)resources.GetObject("MnuFullScreen.Image");
            MnuFullScreen.ImageAlign = ContentAlignment.MiddleLeft;
            MnuFullScreen.ImageScaling = ToolStripItemImageScaling.None;
            MnuFullScreen.Name = "MnuFullScreen";
            MnuFullScreen.Size = new Size(293, 52);
            MnuFullScreen.Text = "[Full screen]";
            MnuFullScreen.Click += MnuFullScreen_Click;
            // 
            // MnuSlideshow
            // 
            MnuSlideshow.DropDownItems.AddRange(new ToolStripItem[] { MnuStartSlideshow, MnuCloseAllSlideshows });
            MnuSlideshow.Image = (Image)resources.GetObject("MnuSlideshow.Image");
            MnuSlideshow.ImageAlign = ContentAlignment.MiddleLeft;
            MnuSlideshow.ImageScaling = ToolStripItemImageScaling.None;
            MnuSlideshow.Name = "MnuSlideshow";
            MnuSlideshow.Size = new Size(293, 52);
            MnuSlideshow.Text = "Slideshow";
            // 
            // MnuStartSlideshow
            // 
            MnuStartSlideshow.Name = "MnuStartSlideshow";
            MnuStartSlideshow.Size = new Size(490, 54);
            MnuStartSlideshow.Text = "[Start new slideshow]";
            MnuStartSlideshow.Click += MnuStartSlideshow_Click;
            // 
            // MnuCloseAllSlideshows
            // 
            MnuCloseAllSlideshows.Name = "MnuCloseAllSlideshows";
            MnuCloseAllSlideshows.Size = new Size(490, 54);
            MnuCloseAllSlideshows.Text = "[Close all slideshows]";
            MnuCloseAllSlideshows.Click += MnuCloseAllSlideshows_Click;
            // 
            // toolStripMenuItem7
            // 
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new Size(290, 6);
            // 
            // MnuLayout
            // 
            MnuLayout.DropDownItems.AddRange(new ToolStripItem[] { MnuToggleToolbar, MnuToggleThumbnails, MnuToggleCheckerboard, toolStripMenuItem20, MnuToggleTopMost });
            MnuLayout.Image = (Image)resources.GetObject("MnuLayout.Image");
            MnuLayout.ImageAlign = ContentAlignment.MiddleLeft;
            MnuLayout.ImageScaling = ToolStripItemImageScaling.None;
            MnuLayout.Name = "MnuLayout";
            MnuLayout.Size = new Size(293, 52);
            MnuLayout.Text = "[Layout]";
            // 
            // MnuToggleToolbar
            // 
            MnuToggleToolbar.CheckOnClick = true;
            MnuToggleToolbar.Name = "MnuToggleToolbar";
            MnuToggleToolbar.Size = new Size(604, 54);
            MnuToggleToolbar.Text = "[Toolbar]";
            MnuToggleToolbar.Click += MnuToggleToolbar_Click;
            // 
            // MnuToggleThumbnails
            // 
            MnuToggleThumbnails.CheckOnClick = true;
            MnuToggleThumbnails.Name = "MnuToggleThumbnails";
            MnuToggleThumbnails.Size = new Size(604, 54);
            MnuToggleThumbnails.Text = "[Thumbnail panel]";
            MnuToggleThumbnails.Click += MnuToggleThumbnails_Click;
            // 
            // MnuToggleCheckerboard
            // 
            MnuToggleCheckerboard.CheckOnClick = true;
            MnuToggleCheckerboard.Name = "MnuToggleCheckerboard";
            MnuToggleCheckerboard.Size = new Size(604, 54);
            MnuToggleCheckerboard.Text = "[Checkerboard]";
            MnuToggleCheckerboard.Click += MnuToggleCheckerboard_Click;
            // 
            // toolStripMenuItem20
            // 
            toolStripMenuItem20.Name = "toolStripMenuItem20";
            toolStripMenuItem20.Size = new Size(601, 6);
            // 
            // MnuToggleTopMost
            // 
            MnuToggleTopMost.CheckOnClick = true;
            MnuToggleTopMost.Name = "MnuToggleTopMost";
            MnuToggleTopMost.Size = new Size(604, 54);
            MnuToggleTopMost.Text = "[Keep window always on top]";
            MnuToggleTopMost.Click += MnuToggleTopMost_Click;
            // 
            // MnuTools
            // 
            MnuTools.DropDownItems.AddRange(new ToolStripItem[] { MnuColorPicker, MnuCropTool, MnuPageNav });
            MnuTools.Image = (Image)resources.GetObject("MnuTools.Image");
            MnuTools.ImageAlign = ContentAlignment.MiddleLeft;
            MnuTools.ImageScaling = ToolStripItemImageScaling.None;
            MnuTools.Name = "MnuTools";
            MnuTools.Size = new Size(293, 52);
            MnuTools.Text = "[Tools]";
            // 
            // MnuColorPicker
            // 
            MnuColorPicker.CheckOnClick = true;
            MnuColorPicker.Name = "MnuColorPicker";
            MnuColorPicker.Size = new Size(434, 54);
            MnuColorPicker.Text = "[Color picker]";
            MnuColorPicker.Click += MnuColorPicker_Click;
            // 
            // MnuCropTool
            // 
            MnuCropTool.CheckOnClick = true;
            MnuCropTool.Name = "MnuCropTool";
            MnuCropTool.Size = new Size(434, 54);
            MnuCropTool.Text = "[Cropping]";
            MnuCropTool.Click += MnuCropTool_Click;
            // 
            // MnuPageNav
            // 
            MnuPageNav.CheckOnClick = true;
            MnuPageNav.Name = "MnuPageNav";
            MnuPageNav.Size = new Size(434, 54);
            MnuPageNav.Text = "[Page navigation]";
            MnuPageNav.Click += MnuPageNav_Click;
            // 
            // toolStripMenuItem8
            // 
            toolStripMenuItem8.Name = "toolStripMenuItem8";
            toolStripMenuItem8.Size = new Size(290, 6);
            // 
            // MnuSettings
            // 
            MnuSettings.Image = (Image)resources.GetObject("MnuSettings.Image");
            MnuSettings.ImageAlign = ContentAlignment.MiddleLeft;
            MnuSettings.ImageScaling = ToolStripItemImageScaling.None;
            MnuSettings.Name = "MnuSettings";
            MnuSettings.Size = new Size(293, 52);
            MnuSettings.Text = "[Settings]";
            MnuSettings.Click += MnuSettings_Click;
            // 
            // MnuHelp
            // 
            MnuHelp.DropDownItems.AddRange(new ToolStripItem[] { MnuAbout, MnuCheckForUpdate, MnuReportIssue, MnuFirstLaunch, toolStripMenuItem22, MnuSetDefaultPhotoViewer, MnuUnsetDefaultPhotoViewer });
            MnuHelp.Image = (Image)resources.GetObject("MnuHelp.Image");
            MnuHelp.ImageAlign = ContentAlignment.MiddleLeft;
            MnuHelp.ImageScaling = ToolStripItemImageScaling.None;
            MnuHelp.Name = "MnuHelp";
            MnuHelp.Size = new Size(293, 52);
            MnuHelp.Text = "[Help]";
            // 
            // MnuAbout
            // 
            MnuAbout.Name = "MnuAbout";
            MnuAbout.Size = new Size(595, 54);
            MnuAbout.Text = "[About]";
            MnuAbout.Click += MnuAbout_Click;
            // 
            // MnuCheckForUpdate
            // 
            MnuCheckForUpdate.Name = "MnuCheckForUpdate";
            MnuCheckForUpdate.Size = new Size(595, 54);
            MnuCheckForUpdate.Text = "[A new version is available]";
            MnuCheckForUpdate.Click += MnuCheckForUpdate_Click;
            // 
            // MnuReportIssue
            // 
            MnuReportIssue.Name = "MnuReportIssue";
            MnuReportIssue.Size = new Size(595, 54);
            MnuReportIssue.Text = "[Report an issue]";
            MnuReportIssue.Click += MnuReportIssue_Click;
            // 
            // MnuFirstLaunch
            // 
            MnuFirstLaunch.Name = "MnuFirstLaunch";
            MnuFirstLaunch.Size = new Size(595, 54);
            MnuFirstLaunch.Text = "[First-launch configurations]";
            MnuFirstLaunch.Click += MnuFirstLaunch_Click;
            // 
            // toolStripMenuItem22
            // 
            toolStripMenuItem22.Name = "toolStripMenuItem22";
            toolStripMenuItem22.Size = new Size(592, 6);
            // 
            // MnuSetDefaultPhotoViewer
            // 
            MnuSetDefaultPhotoViewer.Name = "MnuSetDefaultPhotoViewer";
            MnuSetDefaultPhotoViewer.Size = new Size(595, 54);
            MnuSetDefaultPhotoViewer.Text = "[Set as default photo viewer]";
            MnuSetDefaultPhotoViewer.Click += MnuSetDefaultPhotoViewer_Click;
            // 
            // MnuUnsetDefaultPhotoViewer
            // 
            MnuUnsetDefaultPhotoViewer.Name = "MnuUnsetDefaultPhotoViewer";
            MnuUnsetDefaultPhotoViewer.Size = new Size(595, 54);
            MnuUnsetDefaultPhotoViewer.Text = "[Unset default photo viewer]";
            MnuUnsetDefaultPhotoViewer.Click += MnuUnsetDefaultPhotoViewer_Click;
            // 
            // toolStripMenuItem9
            // 
            toolStripMenuItem9.Name = "toolStripMenuItem9";
            toolStripMenuItem9.Size = new Size(290, 6);
            // 
            // MnuExit
            // 
            MnuExit.Image = (Image)resources.GetObject("MnuExit.Image");
            MnuExit.ImageAlign = ContentAlignment.MiddleLeft;
            MnuExit.ImageScaling = ToolStripItemImageScaling.None;
            MnuExit.Name = "MnuExit";
            MnuExit.Size = new Size(293, 52);
            MnuExit.Text = "[Exit]";
            MnuExit.Click += MnuExit_Click;
            // 
            // PicMain
            // 
            PicMain.AccentColor = Color.Black;
            PicMain.AllowDrop = true;
            PicMain.BackColor = SystemColors.Control;
            PicMain.BaseDpi = 96F;
            PicMain.CheckerboardMode = Base.PhotoBox.CheckerboardMode.Client;
            PicMain.CheckFPS = false;
            PicMain.ClientSelection = (RectangleF)resources.GetObject("PicMain.ClientSelection");
            PicMain.DebugMode = false;
            PicMain.Dock = DockStyle.Fill;
            PicMain.EnableSelection = false;
            PicMain.InterpolationScaleDown = Base.PhotoBox.ImageInterpolation.SampleLinear;
            PicMain.Location = new Point(0, 0);
            PicMain.Margin = new Padding(0);
            PicMain.MessageBorderRadius = 6F;
            PicMain.Name = "PicMain";
            PicMain.NavButtonSize = new SizeF(70F, 70F);
            PicMain.NavDisplay = Base.PhotoBox.NavButtonDisplay.Both;
            PicMain.RequestUpdateFrame = false;
            PicMain.SelectionAspectRatio = new SizeF(0F, 0F);
            PicMain.Size = new Size(1661, 833);
            PicMain.SourceSelection = (RectangleF)resources.GetObject("PicMain.SourceSelection");
            PicMain.TabIndex = 2;
            PicMain.OnNavLeftClicked += PicMain_OnNavLeftClicked;
            PicMain.OnNavRightClicked += PicMain_OnNavRightClicked;
            PicMain.OnZoomChanged += PicMain_OnZoomChanged;
            PicMain.DragDrop += PicMain_DragDrop;
            PicMain.DragOver += PicMain_DragOver;
            PicMain.KeyDown += PicMain_KeyDown;
            PicMain.KeyUp += PicMain_KeyUp;
            PicMain.MouseClick += PicMain_MouseClick;
            PicMain.MouseDoubleClick += PicMain_MouseDoubleClick;
            PicMain.MouseWheel += PicMain_MouseWheel;
            // 
            // Gallery
            // 
            Gallery.BackColor = Color.FromArgb(255, 192, 192);
            Gallery.CheckBoxAlignment = ContentAlignment.TopLeft;
            Gallery.CheckBoxPadding = new Size(6, 6);
            Gallery.Dock = DockStyle.Bottom;
            Gallery.EnableKeyNavigation = true;
            Gallery.EnableTransparent = true;
            Gallery.Location = new Point(0, 833);
            Gallery.Margin = new Padding(0);
            Gallery.Name = "Gallery";
            Gallery.PersistentCacheDirectory = "";
            Gallery.PersistentCacheSize = 100L;
            Gallery.RetryOnError = false;
            Gallery.Size = new Size(1661, 200);
            Gallery.TabIndex = 0;
            Gallery.ThumbnailSize = new Size(70, 70);
            Gallery.View = ImageGlass.Gallery.View.HorizontalStrip;
            Gallery.ItemClick += Gallery_ItemClick;
            Gallery.ItemTooltipShowing += Gallery_ItemTooltipShowing;
            // 
            // MnuContext
            // 
            MnuContext.CurrentDpi = 96;
            MnuContext.ImageScalingSize = new Size(22, 22);
            MnuContext.Items.AddRange(new ToolStripItem[] { itemToPreserveTheSpaceToolStripMenuItem });
            MnuContext.Name = "MnuContext";
            MnuContext.Size = new Size(479, 56);
            MnuContext.Opening += MnuContext_Opening;
            // 
            // itemToPreserveTheSpaceToolStripMenuItem
            // 
            itemToPreserveTheSpaceToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            itemToPreserveTheSpaceToolStripMenuItem.Name = "itemToPreserveTheSpaceToolStripMenuItem";
            itemToPreserveTheSpaceToolStripMenuItem.Size = new Size(478, 52);
            itemToPreserveTheSpaceToolStripMenuItem.Text = "item to preserve the space";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(188, 6);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(188, 6);
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(188, 6);
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(188, 6);
            // 
            // MnuSubMenu
            // 
            MnuSubMenu.CurrentDpi = 96;
            MnuSubMenu.ImageScalingSize = new Size(22, 22);
            MnuSubMenu.Items.AddRange(new ToolStripItem[] { itemToPreserveSpaceToolStripMenuItem });
            MnuSubMenu.Name = "MnuContext";
            MnuSubMenu.Size = new Size(424, 56);
            // 
            // itemToPreserveSpaceToolStripMenuItem
            // 
            itemToPreserveSpaceToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            itemToPreserveSpaceToolStripMenuItem.Name = "itemToPreserveSpaceToolStripMenuItem";
            itemToPreserveSpaceToolStripMenuItem.Size = new Size(423, 52);
            itemToPreserveSpaceToolStripMenuItem.Text = "item to preserve space";
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(240F, 240F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(192, 192, 255);
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(1661, 1033);
            Controls.Add(Toolbar);
            Controls.Add(PicMain);
            Controls.Add(Gallery);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(2);
            Name = "FrmMain";
            RightToLeftLayout = true;
            Text = "ImageGlass";
            Load += FrmMain_Load;
            KeyDown += FrmMain_KeyDown;
            KeyUp += FrmMain_KeyUp;
            MnuMain.ResumeLayout(false);
            MnuContext.ResumeLayout(false);
            MnuSubMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        public ToolStripMenuItem MnuExportFrames;
        public ToolStripMenuItem MnuSetDesktopBackground;
        public ToolStripMenuItem MnuSetLockScreen;
        public ToolStripMenuItem MnuOpenLocation;
        public ToolStripMenuItem MnuImageProperties;
        public ToolStripMenuItem MnuCopyImageData;
        public ToolStripMenuItem MnuCopyFile;
        public ToolStripMenuItem MnuCutFile;
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
        public ToolStripMenuItem MnuUnload;
        private TableLayoutPanel TbLayout;
    }
}