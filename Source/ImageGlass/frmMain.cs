/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2015 DUONG DIEU PHAP
Project homepage: http://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ImageGlass.Core;
using ImageGlass.Library.Image;
using ImageGlass.Library.Comparer;
using System.IO;
using System.Diagnostics;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using System.Collections.Specialized;
using ImageGlass.Services.InstanceManagement;
using System.Drawing.Imaging;

namespace ImageGlass
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            mnuMain.Renderer = mnuPopup.Renderer = new Theme.ModernMenuRenderer();

            //Check and perform DPI Scaling
            LocalSetting.OldDPI = LocalSetting.CurrentDPI;
            LocalSetting.CurrentDPI = Theme.DPIScaling.CalculateCurrentDPI(this);
            Theme.DPIScaling.HandleDpiChanged(LocalSetting.OldDPI, LocalSetting.CurrentDPI, this);
        }


        #region Local variables
        private string _imageInfo = "";

        // window size value before resizing
        private Size _windowSize = new Size(600, 500);

        // determine if the image is zoomed
        private bool _isZoomed = false;

        //determine if toolbar is shown
        private bool _isShownToolbar = true;
        #endregion



        #region Drag - drop
        private void picMain_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void picMain_DragDrop(object sender, DragEventArgs e)
        {
            Prepare(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
        }
        #endregion



        #region Preparing image
        /// <summary>
        /// Open an image
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = GlobalSetting.LangPack.Items["frmMain._OpenFileDialog"] + "|" +
                        GlobalSetting.SupportedExtensions;

            if (o.ShowDialog() == DialogResult.OK && File.Exists(o.FileName))
            {
                Prepare(o.FileName);
            }
            o.Dispose();
        }

        /// <summary>
        /// Prepare to load image
        /// </summary>
        /// <param name="path">Path</param>
        public void Prepare(string path)
        {
            if (File.Exists(path) == false && Directory.Exists(path) == false)
                return;

            //Reset current index
            GlobalSetting.CurrentIndex = 0;
            string initFile = "";

            //Check path is file or directory?
            if (File.Exists(path))
            {
                initFile = path;
                path = path.Substring(0, path.LastIndexOf("\\") + 1);
            }
            else if (Directory.Exists(path))
            {
                path = path.Replace("\\\\", "\\");
            }

            //Set path as current image path
            //GlobalSetting.CurrentPath = path;

            //Declare a new list to store filename
            GlobalSetting.ImageFilenameList = new List<string>();

            //Get supported image extensions from path
            GlobalSetting.ImageFilenameList = LoadImageFilesFromDirectory(path);

            //Dispose all garbage
            GlobalSetting.ImageList.Dispose();

            //Set filename to image list
            GlobalSetting.ImageList = new ImgMan(GlobalSetting.ImageFilenameList.ToArray());

            //Find the index of current image
            GlobalSetting.CurrentIndex = GlobalSetting.ImageFilenameList.IndexOf(initFile);

            //Load thumnbnail
            LoadThumbnails();

            //Cannot find the index
            if (GlobalSetting.CurrentIndex == -1)
            {
                //Mark as Image Error
                GlobalSetting.IsImageError = true;
                this.Text = "ImageGlass - " + initFile;
                lblInfo.Text = ImageInfo.GetFileSize(initFile);
                picMain.Text = GlobalSetting.LangPack.Items["frmMain.picMain._ErrorText"];
                picMain.Image = null;

                //Exit function
                return;
            }

            //Start loading image
            NextPic(0);

            //Watch all change of current path
            sysWatch.Path = Path.GetDirectoryName(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            sysWatch.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Select current thumbnail
        /// </summary>
        private void SelectCurrentThumbnail()
        {
            if (thumbnailBar.Items.Count > 0)
            {
                thumbnailBar.ClearSelection();
                thumbnailBar.Items[GlobalSetting.CurrentIndex].Selected = true;
                thumbnailBar.Items[GlobalSetting.CurrentIndex].Focused = true;
                thumbnailBar.EnsureVisible(GlobalSetting.CurrentIndex);
            }
        }

        /// <summary>
        /// Sort and find all supported image from directory
        /// </summary>
        /// <param name="path">Image folder path</param>
        private List<string> LoadImageFilesFromDirectory(string path)
        {
            //Load image order from config
            GlobalSetting.LoadImageOrderConfig();

            var list = new List<string>();

            //Get files from dir
            var dsFile = DirectoryFinder.FindFiles(path,
                GlobalSetting.IsRecursive,
                new Predicate<string>(delegate (String f)
                {
                    Application.DoEvents();
                    if (GlobalSetting.SupportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                    {
                        return true;
                    }
                    return false;
                }));

            //Sort image file
            if (GlobalSetting.ImageOrderBy == ImageOrderBy.Name)
            {
                list.AddRange(FileLogicalComparer
                    .Sort(dsFile.ToArray()));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.Length)
            {
                list.AddRange(dsFile
                    .OrderBy(f => new FileInfo(f).Length));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.CreationTime)
            {
                list.AddRange(dsFile
                    .OrderBy(f => new FileInfo(f).CreationTimeUtc));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.Extension)
            {
                list.AddRange(dsFile
                    .OrderBy(f => new FileInfo(f).Extension));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.LastAccessTime)
            {
                list.AddRange(dsFile
                    .OrderBy(f => new FileInfo(f).LastAccessTime));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.LastWriteTime)
            {
                list.AddRange(dsFile
                    .OrderBy(f => new FileInfo(f).LastWriteTime));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.Random)
            {
                list.AddRange(dsFile
                    .OrderBy(f => Guid.NewGuid()));
            }

            return list;
        }

        /// <summary>
        /// Clear and reload all thumbnail image
        /// </summary>
        private void LoadThumbnails()
        {
            thumbnailBar.Items.Clear();

            for (int i = 0; i < GlobalSetting.ImageList.Length; i++)
            {
                ImageListView.ImageListViewItem lvi = new ImageListView.ImageListViewItem(GlobalSetting.ImageList.GetFileName(i));
                lvi.Tag = GlobalSetting.ImageFilenameList[i];

                thumbnailBar.Items.Add(lvi);
            }

        }

        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        private void NextPic(int step)
        {
            //Save previous image if it was modified
            if (File.Exists(LocalSetting.ImageModifiedPath))
            {
                this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SaveChanges"], 2000);

                Application.DoEvents();
                ImageSaveChange();
                return;
            }

            this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._Loading"], 5000);
            Application.DoEvents();

            picMain.Text = "";
            GlobalSetting.IsTempMemoryData = false;

            if (GlobalSetting.ImageList.Length < 1)
            {
                this.Text = "ImageGlass";
                lblInfo.Text = string.Empty;

                GlobalSetting.IsImageError = true;
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";

                return;
            }

            //Update current index
            GlobalSetting.CurrentIndex += step;

            //Check if current index is greater than upper limit
            if (GlobalSetting.CurrentIndex >= GlobalSetting.ImageList.Length) GlobalSetting.CurrentIndex = 0;

            //Check if current index is less than lower limit
            if (GlobalSetting.CurrentIndex < 0) GlobalSetting.CurrentIndex = GlobalSetting.ImageList.Length - 1;

            //The image data will load
            Image im = null;

            try
            {
                //Track image loading progress
                GlobalSetting.ImageList.OnFinishLoadingImage += ImageList_OnFinishLoadingImage;

                //Read imaeg data
                im = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);

                GlobalSetting.IsImageError = GlobalSetting.ImageList.IsErrorImage;

                //Show image
                picMain.Image = im;

                //refresh image
                mnuMainRefresh_Click(null, null);
                
                //Release unused images
                if (GlobalSetting.CurrentIndex - 2 >= 0)
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex - 2);
                }
                if (!GlobalSetting.IsImageBoosterBack && GlobalSetting.CurrentIndex - 1 >= 0)
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex - 1);
                }
            }
            catch
            {
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";

                Application.DoEvents();
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
                }
            }

            if (GlobalSetting.IsImageError)
            {
                picMain.Text = GlobalSetting.LangPack.Items["frmMain.picMain._ErrorText"];
                picMain.Image = null;
                LocalSetting.ImageModifiedPath = "";
            }

            //Select thumbnail item
            SelectCurrentThumbnail();

            //Collect system garbage
            System.GC.Collect();
        }

        private void ImageList_OnFinishLoadingImage(object sender, EventArgs e)
        {
            //clear text when finishing
            this.DisplayTextMessage("", 0);
        }



        /// <summary>
        /// Update image information on status bar
        /// </summary>
        private void UpdateStatusBar(bool @zoomOnly = false)
        {
            string fileinfo = "";

            if (GlobalSetting.ImageList.Length < 1)
            {
                this.Text = "ImageGlass";
                lblInfo.Text = fileinfo;
                return;
            }

            //Set the text of Window title
            this.Text = "ImageGlass - " +
                        (GlobalSetting.CurrentIndex + 1) + "/" + GlobalSetting.ImageList.Length + " " +
                        GlobalSetting.LangPack.Items["frmMain._Text"] + " - " +
                        GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            if (GlobalSetting.IsImageError)
            {
                try
                {
                    fileinfo = ImageInfo.GetFileSize(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) + "\t  |  ";
                    fileinfo += Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).Replace(".", "").ToUpper() + "  |  ";
                    fileinfo += File.GetCreationTime(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToString("yyyy/M/d HH:m:s");
                    this._imageInfo = fileinfo;
                }
                catch { fileinfo = ""; }
            }
            else
            {
                fileinfo += picMain.Image.Width + " x " + picMain.Image.Height + " px  |  ";

                if (zoomOnly)
                {
                    fileinfo = picMain.Zoom.ToString() + "%  |  " + _imageInfo;
                }
                else
                {
                    fileinfo += ImageInfo.GetFileSize(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) + "\t  |  ";
                    fileinfo += File.GetCreationTime(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToString("yyyy/M/d HH:m:s");

                    this._imageInfo = fileinfo;

                    fileinfo = picMain.Zoom.ToString() + "%  |  " + fileinfo;
                }
            }

            //Move image information to Window title
            this.Text += "  |  " + fileinfo;

        }
        #endregion



        #region Key event

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //this.Text = e.KeyValue.ToString();
            if (e.KeyValue == 192 && !e.Control && !e.Shift && !e.Alt) // `
            {
                mnuMain.Show(picMain, 0, picMain.Top);
            }

            // Rotation Counterclockwise----------------------------------------------------
            #region Ctrl + ,
            if (e.KeyValue == 188 && e.Control && !e.Shift && !e.Alt)//Ctrl + ,
            {
                mnuMainRotateCounterclockwise_Click(null, null);
                return;
            }
            #endregion


            //Rotate Clockwise--------------------------------------------------------------
            #region Ctrl + .
            if (e.KeyValue == 190 && e.Control && !e.Shift && !e.Alt)//Ctrl + .
            {
                mnuMainRotateClockwise_Click(null, null);
                return;
            }
            #endregion


            //ESC ultility------------------------------------------------------------------
            #region ESC
            if (e.KeyValue == 27 && !e.Control && !e.Shift && !e.Alt)//ESC
            {
                //exit slideshow
                if (GlobalSetting.IsPlaySlideShow)
                {
                    mnuMainSlideShowExit_Click(null, null);
                }
                //exit full screen
                else if (GlobalSetting.IsFullScreen)
                {
                    btnFullScreen.PerformClick();
                }
                //Quit ImageGlass
                else if (GlobalSetting.IsPressESCToQuit)
                {
                    Application.Exit();
                }
                return;
            }
            #endregion


            //Clear clipboard----------------------------------------------------------------
            #region CTRL + `
            if (e.KeyValue == 192 && e.Control && !e.Shift && !e.Alt)//CTRL + `
            {
                mnuMainClearClipboard_Click(null, null);
                return;
            }
            #endregion


            //Start / stop slideshow---------------------------------------------------------
            #region SPACE
            if (GlobalSetting.IsPlaySlideShow && e.KeyCode == Keys.Space && !e.Control && !e.Shift && !e.Alt)//SPACE
            {
                mnuMainSlideShowPause_Click(null, null);
                return;
            }
            #endregion
            

            //Previous Image----------------------------------------------------------------
            #region LEFT ARROW / PAGE UP
            if ((e.KeyValue == 33 || e.KeyValue == 37) &&
                !e.Control && !e.Shift && !e.Alt)//Left arrow / PageUp
            {
                NextPic(-1);
                return;
            }
            #endregion


            //Next Image---------------------------------------------------------------------
            #region RIGHT ARROW / PAGE DOWN
            if ((e.KeyValue == 34 || e.KeyValue == 39) &&
                !e.Control && !e.Shift && !e.Alt)//Right arrow / Pagedown
            {
                NextPic(1);
                return;
            }
            #endregion


            //Goto the first Image---------------------------------------------------------------
            #region HOME
            if ((e.KeyValue == 36 || e.KeyValue == 39) &&
                !e.Control && !e.Shift && !e.Alt)
            {
                mnuMainGotoFirst_Click(null, e);
                return;
            }
            #endregion

            //Goto the last Image---------------------------------------------------------------
            #region END
            if ((e.KeyValue == 35 || e.KeyValue == 39) &&
                !e.Control && !e.Shift && !e.Alt)
            {
                mnuMainGotoLast_Click(null, e);
                return;
            }
            #endregion


            //Zoom + ------------------------------------------------------------------------
            #region Ctrl + =
            if (e.KeyValue == 187 && e.Control && !e.Shift && !e.Alt)// Ctrl + =
            {
                btnZoomIn_Click(null, null);
                return;
            }
            #endregion


            //Zoom - ------------------------------------------------------------------------
            #region Ctrl + -
            if (e.KeyValue == 189 && e.Control && !e.Shift && !e.Alt)// Ctrl + -
            {
                btnZoomOut_Click(null, null);
                return;
            }
            #endregion
            

            //Actual size image -------------------------------------------------------------
            #region Ctrl + 0
            if (e.KeyValue == 48 && e.Control && !e.Shift && !e.Alt)// Ctrl + 0
            {
                btnActualSize_Click(null, null);
                return;
            }
            #endregion
            

            //Full screen--------------------------------------------------------------------
            #region ALT + ENTER
            if (e.Alt && e.KeyCode == Keys.Enter && !e.Control && !e.Shift)//Alt + Enter
            {
                btnFullScreen.PerformClick();
                return;
            }
            #endregion
            
        }

        #endregion



        #region Private functions
        
        /// <summary>
        /// Rename image
        /// </summary>
        /// <param name="oldFilename"></param>
        /// <param name="newname"></param>
        private void RenameImage()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            //Lay ten file
            string oldName;
            string newName;
            oldName = newName = Path.GetFileName(
                GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            string currentPath = (Path.GetDirectoryName(
                GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) + "\\")
                .Replace("\\\\", "\\");

            //Lay ext
            string ext = newName.Substring(newName.LastIndexOf("."));
            newName = newName.Substring(0, newName.Length - ext.Length);

            //Hien input box
            string str = null;
            if (InputBox.ShowDiaLog("Rename", GlobalSetting.LangPack.Items["frmMain._RenameDialog"],
                                    newName) == System.Windows.Forms.DialogResult.OK)
            {
                str = InputBox.Message;
            }
            if (str == null)
            {
                return;
            }

            newName = str + ext;
            //Neu ten giong nhau thi return;
            if (oldName == newName)
            {
                return;
            }

            try
            {
                //Doi ten tap tin
                ImageInfo.RenameFile(currentPath + oldName, currentPath + newName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Display a message on picture box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="duration">Duration (milisecond)</param>
        private void DisplayTextMessage(string msg, int duration)
        {
            if(duration == 0)
            {
                picMain.TextBackColor = Color.Transparent;
                picMain.Font = this.Font;
                picMain.ForeColor = Color.Black;
                picMain.Text = string.Empty;
                return;
            }

            System.Windows.Forms.Timer tmsg = new System.Windows.Forms.Timer();
            tmsg.Enabled = false;
            tmsg.Tick += tmsg_Tick;
            tmsg.Interval = duration; //display in xxx mili seconds

            picMain.TextBackColor = Color.Black;
            picMain.Font = new System.Drawing.Font(this.Font.FontFamily, 12);
            picMain.ForeColor = Color.White;
            picMain.Text = msg;

            //Start timer
            tmsg.Enabled = true;
            tmsg.Start();
        }

        private void tmsg_Tick(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer tmsg = (System.Windows.Forms.Timer)sender;
            tmsg.Stop();

            if(GlobalSetting.IsImageError)
            {
                return;
            }

            picMain.TextBackColor = Color.Transparent;
            picMain.Font = this.Font;
            picMain.ForeColor = Color.Black;
            picMain.Text = string.Empty;
        }

        private void CopyFile()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            GlobalSetting.StringClipboard = new StringCollection();
            GlobalSetting.StringClipboard.Add(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            Clipboard.SetFileDropList(GlobalSetting.StringClipboard);

            this.DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items["frmMain._CopyFileText"],
                GlobalSetting.StringClipboard.Count), 1000);
        }

        private void CopyMultiFile()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            //get filename
            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            //exit if duplicated filename
            if (GlobalSetting.StringClipboard.IndexOf(filename) != -1)
            {
                return;
            }

            //add filename to clipboard
            GlobalSetting.StringClipboard.Add(filename);
            Clipboard.SetFileDropList(GlobalSetting.StringClipboard);

            this.DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items["frmMain._CopyFileText"],
                GlobalSetting.StringClipboard.Count), 1000);
        }

        private void CutFile()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            GlobalSetting.StringClipboard = new StringCollection();
            GlobalSetting.StringClipboard.Add(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));

            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            DataObject data = new DataObject();
            data.SetFileDropList(GlobalSetting.StringClipboard);
            data.SetData("Preferred DropEffect", dropEffect);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);

            this.DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items["frmMain._CutFileText"],
                GlobalSetting.StringClipboard.Count), 1000);
        }

        private void CutMultiFile()
        {
            try
            {
                if (GlobalSetting.IsImageError || !File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            //get filename
            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            //exit if duplicated filename
            if (GlobalSetting.StringClipboard.IndexOf(filename) != -1)
            {
                return;
            }

            //add filename to clipboard
            GlobalSetting.StringClipboard.Add(filename);

            byte[] moveEffect = new byte[] { 2, 0, 0, 0 };
            MemoryStream dropEffect = new MemoryStream();
            dropEffect.Write(moveEffect, 0, moveEffect.Length);

            DataObject data = new DataObject();
            data.SetFileDropList(GlobalSetting.StringClipboard);
            data.SetData("Preferred DropEffect", dropEffect);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);

            this.DisplayTextMessage(
                string.Format(GlobalSetting.LangPack.Items["frmMain._CutFileText"],
                GlobalSetting.StringClipboard.Count), 1000);
        }

        /// <summary>
        /// Save all change of image
        /// </summary>
        private void ImageSaveChange()
        {
            try
            {
                Library.Image.ImageInfo.SaveImage(picMain.Image, LocalSetting.ImageModifiedPath);
            }
            catch { }

            LocalSetting.ImageModifiedPath = "";
        }
        #endregion



        #region Configurations
        /// <summary>
        /// Load default theme
        /// </summary>
        private void LoadThemeDefault()
        {
            // <main>
            toolMain.BackgroundImage = ImageGlass.Properties.Resources.topbar;
            thumbnailBar.BackgroundImage = ImageGlass.Properties.Resources.bottombar;
            lblInfo.ForeColor = Color.Black;

            picMain.BackColor = this.BackColor;

            // <toolbar_icon>
            btnBack.Image = ImageGlass.Properties.Resources.back;
            btnNext.Image = ImageGlass.Properties.Resources.next;
            btnRotateLeft.Image = ImageGlass.Properties.Resources.leftrotate;
            btnRotateRight.Image = ImageGlass.Properties.Resources.rightrotate;
            btnZoomIn.Image = ImageGlass.Properties.Resources.zoomin;
            btnZoomOut.Image = ImageGlass.Properties.Resources.zoomout;
            btnActualSize.Image = ImageGlass.Properties.Resources.scaletofit;
            btnZoomLock.Image = ImageGlass.Properties.Resources.zoomlock;
            btnScaletoWidth.Image = ImageGlass.Properties.Resources.scaletowidth;
            btnScaletoHeight.Image = ImageGlass.Properties.Resources.scaletoheight;
            btnWindowAutosize.Image = ImageGlass.Properties.Resources.autosizewindow;
            btnOpen.Image = ImageGlass.Properties.Resources.open;
            btnRefresh.Image = ImageGlass.Properties.Resources.refresh;
            btnGoto.Image = ImageGlass.Properties.Resources.gotoimage;
            btnThumb.Image = ImageGlass.Properties.Resources.thumbnail;
            btnCheckedBackground.Image = ImageGlass.Properties.Resources.background;
            btnFullScreen.Image = ImageGlass.Properties.Resources.fullscreen;
            btnSlideShow.Image = ImageGlass.Properties.Resources.slideshow;
            btnConvert.Image = ImageGlass.Properties.Resources.convert;
            btnPrintImage.Image = ImageGlass.Properties.Resources.printer;
            btnFacebook.Image = ImageGlass.Properties.Resources.uploadfb;
            btnExtension.Image = ImageGlass.Properties.Resources.extension;
            btnSetting.Image = ImageGlass.Properties.Resources.settings;
            btnHelp.Image = ImageGlass.Properties.Resources.about;
            btnMenu.Image = ImageGlass.Properties.Resources.menu;

            GlobalSetting.SetConfig("Theme", "default");
        }


        /// <summary>
        /// Apply changing theme
        /// </summary>
        private void LoadTheme()
        { 
            string themeFile = GlobalSetting.GetConfig("Theme", "default");

            if (File.Exists(themeFile))
            {
                Theme.Theme t = new Theme.Theme(themeFile);
                string dir = (Path.GetDirectoryName(themeFile) + "\\").Replace("\\\\", "\\");

                // <main>
                try { toolMain.BackgroundImage = Image.FromFile(dir + t.topbar); }
                catch { toolMain.BackgroundImage = ImageGlass.Properties.Resources.topbar; }

                try { thumbnailBar.BackgroundImage = Image.FromFile(dir + t.bottombar); }
                catch { thumbnailBar.BackgroundImage = ImageGlass.Properties.Resources.bottombar; }

                try
                {
                    lblInfo.ForeColor = t.statuscolor;
                }
                catch
                {
                    lblInfo.ForeColor = Color.White;
                }


                try
                {
                    picMain.BackColor = t.backcolor;
                    GlobalSetting.BackgroundColor = t.backcolor;
                }
                catch
                {
                    picMain.BackColor = Color.White;
                    GlobalSetting.BackgroundColor = Color.White;
                }


                // <toolbar_icon>
                try { btnBack.Image = Image.FromFile(dir + t.back); }
                catch { btnBack.Image = ImageGlass.Properties.Resources.back; }

                try { btnNext.Image = Image.FromFile(dir + t.next); }
                catch { btnNext.Image = ImageGlass.Properties.Resources.next; }

                try { btnRotateLeft.Image = Image.FromFile(dir + t.leftrotate); }
                catch { btnRotateLeft.Image = ImageGlass.Properties.Resources.leftrotate; }

                try { btnRotateRight.Image = Image.FromFile(dir + t.rightrotate); }
                catch { btnRotateRight.Image = ImageGlass.Properties.Resources.rightrotate; }

                try { btnZoomIn.Image = Image.FromFile(dir + t.zoomin); }
                catch { btnZoomIn.Image = ImageGlass.Properties.Resources.zoomin; }

                try { btnZoomOut.Image = Image.FromFile(dir + t.zoomout); }
                catch { btnZoomOut.Image = ImageGlass.Properties.Resources.zoomout; }

                try { btnActualSize.Image = Image.FromFile(dir + t.scaletofit); }
                catch { btnActualSize.Image = ImageGlass.Properties.Resources.scaletofit; }

                try { btnZoomLock.Image = Image.FromFile(dir + t.zoomlock); }
                catch { btnZoomLock.Image = ImageGlass.Properties.Resources.zoomlock; }

                try { btnScaletoWidth.Image = Image.FromFile(dir + t.scaletowidth); }
                catch { btnScaletoWidth.Image = ImageGlass.Properties.Resources.scaletowidth; }

                try { btnScaletoHeight.Image = Image.FromFile(dir + t.scaletoheight); }
                catch { btnScaletoHeight.Image = ImageGlass.Properties.Resources.scaletoheight; }

                try { btnWindowAutosize.Image = Image.FromFile(dir + t.autosizewindow); }
                catch { btnWindowAutosize.Image = ImageGlass.Properties.Resources.autosizewindow; }

                try { btnOpen.Image = Image.FromFile(dir + t.open); }
                catch { btnOpen.Image = ImageGlass.Properties.Resources.open; }

                try { btnRefresh.Image = Image.FromFile(dir + t.refresh); }
                catch { btnRefresh.Image = ImageGlass.Properties.Resources.refresh; }

                try { btnGoto.Image = Image.FromFile(dir + t.gotoimage); }
                catch { btnGoto.Image = ImageGlass.Properties.Resources.gotoimage; }

                try { btnThumb.Image = Image.FromFile(dir + t.thumbnail); }
                catch { btnThumb.Image = ImageGlass.Properties.Resources.thumbnail; }

                try { btnCheckedBackground.Image = Image.FromFile(dir + t.checkBackground); }
                catch { btnCheckedBackground.Image = ImageGlass.Properties.Resources.background; }

                try { btnFullScreen.Image = Image.FromFile(dir + t.fullscreen); }
                catch { btnFullScreen.Image = ImageGlass.Properties.Resources.fullscreen; }

                try { btnSlideShow.Image = Image.FromFile(dir + t.slideshow); }
                catch { btnSlideShow.Image = ImageGlass.Properties.Resources.slideshow; }

                try { btnConvert.Image = Image.FromFile(dir + t.convert); }
                catch { btnConvert.Image = ImageGlass.Properties.Resources.convert; }

                try { btnPrintImage.Image = Image.FromFile(dir + t.print); }
                catch { btnPrintImage.Image = ImageGlass.Properties.Resources.printer; }

                try { btnFacebook.Image = Image.FromFile(dir + t.uploadfb); }
                catch { btnFacebook.Image = ImageGlass.Properties.Resources.uploadfb; }

                try { btnExtension.Image = Image.FromFile(dir + t.extension); }
                catch { btnExtension.Image = ImageGlass.Properties.Resources.extension; }

                try { btnSetting.Image = Image.FromFile(dir + t.settings); }
                catch { btnSetting.Image = ImageGlass.Properties.Resources.settings; }

                try { btnHelp.Image = Image.FromFile(dir + t.about); }
                catch { btnHelp.Image = ImageGlass.Properties.Resources.about; }

                try { btnMenu.Image = Image.FromFile(dir + t.menu); }
                catch { btnMenu.Image = ImageGlass.Properties.Resources.menu; }

                GlobalSetting.SetConfig("Theme", themeFile);
            }
            else
            {
                LoadThemeDefault();
            }

        }


        /// <summary>
        /// Load app configurations
        /// </summary>
        private void LoadConfig()
        {
            //Load language pack-------------------------------------------------------------
            string s = GlobalSetting.GetConfig("Language", "English");
            if (s.ToLower().CompareTo("english") != 0 && File.Exists(s))
            {
                GlobalSetting.LangPack = new Library.Language(s);

                //force update language pack
                GlobalSetting.IsForcedActive = true;
                frmMain_Activated(null, null);
            }
            
            //Windows Bound (Position + Size)------------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig("WindowsBound", "280,125,850,550"));
            this.Bounds = rc;

            //windows state--------------------------------------------------------------
            s = GlobalSetting.GetConfig("WindowsState", "Normal");
            if (s == "Normal")
            {
                this.WindowState = FormWindowState.Normal;
            }
            else if (s == "Maximized")
            {
                this.WindowState = FormWindowState.Maximized;
            }

            //check current version for the first time running
            s = GlobalSetting.GetConfig("igVersion", Application.ProductVersion);
            if (s.CompareTo(Application.ProductVersion) == 0) //Old version
            {
                //Load Extra extensions
                GlobalSetting.SupportedExtraExtensions = GlobalSetting.GetConfig("ExtraExtensions", GlobalSetting.SupportedExtraExtensions);
            }

            //Load theme--------------------------------------------------------------------
            LoadTheme();
            Application.DoEvents();

            //Slideshow Interval-----------------------------------------------------------
            int i = int.Parse(GlobalSetting.GetConfig("Interval", "5"));
            if (!(0 < i && i < 61)) i = 5;//time limit [1; 60] seconds
            timSlideShow.Interval = 1000 * i;

            //Show checked bakcground-------------------------------------------------------
            GlobalSetting.IsShowCheckedBackground = bool.Parse(GlobalSetting.GetConfig("IsShowCheckedBackground", "False").ToString());
            GlobalSetting.IsShowCheckedBackground = !GlobalSetting.IsShowCheckedBackground;
            mnuMainCheckBackground_Click(null, EventArgs.Empty);
            

            //Recursive loading--------------------------------------------------------------
            GlobalSetting.IsRecursive = bool.Parse(GlobalSetting.GetConfig("Recursive", "False"));

            //Get welcome screen------------------------------------------------------------
            GlobalSetting.IsWelcomePicture = bool.Parse(GlobalSetting.GetConfig("Welcome", "True"));

            //Load default image------------------------------------------------------------
            string y = GlobalSetting.GetConfig("Welcome", "True");
            if (y.ToLower() == "true")
            {
                //Do not show welcome image if params exist.
                if(Environment.GetCommandLineArgs().Count() < 2)
                {
                    Prepare(GlobalSetting.StartUpDir + "default.png");
                }
                
            }

            //Load is loop back slideshow---------------------------------------------------
            GlobalSetting.IsLoopBackSlideShow = bool.Parse(GlobalSetting.GetConfig("IsLoopBackSlideShow", "True"));

            //Load IsPressESCToQuit---------------------------------------------------------
            GlobalSetting.IsPressESCToQuit = bool.Parse(GlobalSetting.GetConfig("IsPressESCToQuit", "True"));

            //Load image order config------------------------------------------------------
            GlobalSetting.LoadImageOrderConfig();

            //Load background---------------------------------------------------------------
            string z = GlobalSetting.GetConfig("BackgroundColor", "-1");
            GlobalSetting.BackgroundColor = Color.FromArgb(int.Parse(z));
            picMain.BackColor = GlobalSetting.BackgroundColor;

            //Load state of Toolbar---------------------------------------------------------
            GlobalSetting.IsShowToolBar = bool.Parse(GlobalSetting.GetConfig("IsShowToolBar", "True"));
            GlobalSetting.IsShowToolBar = !GlobalSetting.IsShowToolBar;
            mnuMainToolbar_Click(null, EventArgs.Empty);


            //Load Thumbnail dimension
            if (int.TryParse(GlobalSetting.GetConfig("ThumbnailDimension", "48"), out i))
            {
                GlobalSetting.ThumbnailDimension = i;
            }
            else
            {
                GlobalSetting.ThumbnailDimension = 48;
            }

            thumbnailBar.SetRenderer(new ImageListView.ImageListViewRenderers.ThemeRenderer());
            thumbnailBar.ThumbnailSize = new Size(GlobalSetting.ThumbnailDimension + GlobalSetting.ThumbnailDimension / 3, GlobalSetting.ThumbnailDimension);

            //Load state of Thumbnail---------------------------------------------------------
            GlobalSetting.IsShowThumbnail = bool.Parse(GlobalSetting.GetConfig("IsShowThumbnail", "False"));
            GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;
            mnuMainThumbnailBar_Click(null, EventArgs.Empty);
        }


        /// <summary>
        /// Save app configurations
        /// </summary>
        private void SaveConfig()
        {
            GlobalSetting.SetConfig("igVersion", Application.ProductVersion.ToString());

            if (this.WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig("WindowsBound", GlobalSetting.RectToString(this.Bounds));
            }

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig("WindowsState", this.WindowState.ToString());

            //Checked background
            GlobalSetting.SetConfig("IsShowCheckedBackground", GlobalSetting.IsShowCheckedBackground.ToString());

            //Tool bar state
            GlobalSetting.SetConfig("IsShowToolBar", GlobalSetting.IsShowToolBar.ToString());

            //Thumbnail panel
            GlobalSetting.SetConfig("IsShowThumbnail", GlobalSetting.IsShowThumbnail.ToString());

            //Save previous image if it was modified
            if (File.Exists(LocalSetting.ImageModifiedPath))
            {
                this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SaveChanges"], 1000);

                Application.DoEvents();
                ImageSaveChange();
            }
        }

        #endregion



        #region Form events
        protected override void WndProc(ref Message m)
        {
            //Check if the received message is WM_SHOWME
            if (m.Msg == NativeMethods.WM_SHOWME)
            {
                //Set frmMain of the first instance to TopMost
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }
                // get our current "TopMost" value (ours will always be false though)
                bool top = TopMost;
                // make our form jump to the top of everything
                TopMost = true;
                // set it back to whatever it was
                TopMost = top;
            }
            //This message is sent when the form is dragged to a different monitor i.e. when
            //the bigger part of its are is on the new monitor. 
            else if (m.Msg == Theme.DPIScaling.WM_DPICHANGED)
            {
                LocalSetting.OldDPI = LocalSetting.CurrentDPI;
                LocalSetting.CurrentDPI = Theme.DPIScaling.LOWORD((int)m.WParam);

                if (LocalSetting.OldDPI != LocalSetting.CurrentDPI)
                {
                    Theme.DPIScaling.HandleDpiChanged(LocalSetting.OldDPI, LocalSetting.CurrentDPI, this);

                    toolMain.Height = 33;
                }
            }
            base.WndProc(ref m);
        }
        

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Remove white line under tool strip
            toolMain.Renderer = new Theme.ToolStripRenderer();

            LoadConfig();
            Application.DoEvents();

            //Load image from param
            LoadFromParams(Environment.GetCommandLineArgs());

            sp1.SplitterDistance = sp1.Height - GlobalSetting.ThumbnailDimension - 41;
            sp1.SplitterWidth = 1;
        }

        public void LoadFromParams(string[] args)
        {
            //Load image from param
            if (args.Length >= 2)
            {
                string filename = "";
                filename = args[1];

                if (File.Exists(filename))
                {
                    FileInfo f = new FileInfo(filename);
                    Prepare(f.FullName);
                }
                else if (Directory.Exists(filename))
                {
                    DirectoryInfo d = new DirectoryInfo(filename);
                    Prepare(d.FullName);
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //clear temp files
            string temp_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                            "\\ImageGlass\\Temp\\";
            if (Directory.Exists(temp_dir))
            {
                Directory.Delete(temp_dir, true);
            }            

            SaveConfig();
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (GlobalSetting.IsForcedActive)
            {
                picMain.BackColor = GlobalSetting.BackgroundColor;

                //Toolbar
                btnBack.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnBack"];
                btnNext.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnNext"];
                btnRotateLeft.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRotateLeft"];
                btnRotateRight.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRotateRight"];
                btnZoomIn.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomIn"];
                btnZoomOut.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomOut"];
                btnActualSize.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnActualSize"];
                btnZoomLock.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnZoomLock"];
                btnScaletoWidth.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnScaletoWidth"];
                btnScaletoHeight.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnScaletoHeight"];
                btnWindowAutosize.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnWindowAutosize"];
                btnOpen.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnOpen"];
                btnRefresh.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnRefresh"];
                btnGoto.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnGoto"];
                btnThumb.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnThumb"];
                btnCheckedBackground.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnCaro"];
                btnFullScreen.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFullScreen"];
                btnSlideShow.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnSlideShow"];
                btnConvert.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnConvert"];
                btnPrintImage.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnPrintImage"];
                btnFacebook.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFacebook"];
                btnExtension.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnExtension"];
                btnSetting.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnSetting"];
                btnHelp.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnHelp"];
                btnMenu.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnMenu"];

                //Main menu
                mnuMainOpenFile.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainOpenFile"];
                mnuMainOpenImageData.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainOpenImageData"];
                mnuMainSaveAs.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSaveAs"];
                mnuMainRefresh.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRefresh"];
                mnuMainEditImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"];

                mnuMainNavigation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainNavigation"];
                mnuMainViewNext.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainViewNext"];
                mnuMainViewPrevious.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainViewPrevious"];
                mnuMainGoto.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGoto"];
                mnuMainGotoFirst.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGotoFirst"];
                mnuMainGotoLast.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainGotoLast"];

                mnuMainFullScreen.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainFullScreen"];

                mnuMainSlideShow.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShow"];
                mnuMainSlideShowStart.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowStart"];
                mnuMainSlideShowPause.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowPause"];
                mnuMainSlideShowExit.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSlideShowExit"];

                mnuMainPrint.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainPrint"];

                mnuMainManipulation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainManipulation"];
                mnuMainRotateCounterclockwise.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRotateCounterclockwise"];
                mnuMainRotateClockwise.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRotateClockwise"];
                mnuMainZoomIn.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainZoomIn"];
                mnuMainZoomOut.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainZoomOut"];
                mnuMainActualSize.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainActualSize"];
                mnuMainLockZoomRatio.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainLockZoomRatio"];
                mnuMainScaleToWidth.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainScaleToWidth"];
                mnuMainScaleToHeight.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainScaleToHeight"];
                mnuMainWindowAdaptImage.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainWindowAdaptImage"];
                mnuMainRename.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRename"];
                mnuMainMoveToRecycleBin.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"];
                mnuMainDeleteFromHardDisk.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainDeleteFromHardDisk"];
                mnuMainExtractFrames.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"];
                mnuMainStartStopAnimating.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainStartStopAnimating"];
                mnuMainSetAsDesktop.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSetAsDesktop"];
                mnuMainImageLocation.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainImageLocation"];
                mnuMainImageProperties.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainImageProperties"];

                mnuMainClipboard.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainClipboard"];
                mnuMainCopy.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopy"];
                mnuMainCopyMulti.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopyMulti"];
                mnuMainCut.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCut"];
                mnuMainCutMulti.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCutMulti"];
                mnuMainCopyImagePath.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCopyImagePath"];
                mnuMainClearClipboard.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainClearClipboard"];

                mnuMainShare.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainShare"];
                mnuMainShareFacebook.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainShareFacebook"];

                mnuMainLayout.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainLayout"];
                mnuMainToolbar.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainToolbar"];
                mnuMainThumbnailBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainThumbnailBar"];
                mnuMainCheckBackground.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainCheckBackground"];

                mnuMainTools.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainTools"];
                mnuMainExtensionManager.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainExtensionManager"];

                mnuMainSettings.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainSettings"];
                mnuMainAbout.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainAbout"];
                mnuMainReportIssue.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainReportIssue"];
            }

            GlobalSetting.IsForcedActive = false;
        }

        private void frmMain_ResizeBegin(object sender, EventArgs e)
        {
            this._windowSize = this.Size;
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Size != this._windowSize && !this._isZoomed)
            {
                mnuMainRefresh_Click(null, null);

                SaveConfig();
            }
            
        }

        private void thumbnailBar_ItemClick(object sender, ImageListView.ItemClickEventArgs e)
        {
            GlobalSetting.CurrentIndex = e.Item.Index;
            NextPic(0);
        }

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            NextPic(1);

            //stop playing slideshow at last image
            if (GlobalSetting.CurrentIndex == GlobalSetting.ImageList.Length - 1)
            {
                if (!GlobalSetting.IsLoopBackSlideShow)
                {
                    mnuMainSlideShowPause_Click(null, null);
                }
            }
        }

        private void sysWatch_Renamed(object sender, RenamedEventArgs e)
        {
            string newName = e.FullPath;
            string oldName = e.OldFullPath;

            //Get index of renamed image
            int imgIndex = GlobalSetting.ImageFilenameList.IndexOf(oldName);
            if (imgIndex > -1)
            {
                //Rename image list
                GlobalSetting.ImageList.SetFileName(imgIndex, newName);
                GlobalSetting.ImageFilenameList[imgIndex] = newName;

                //Cap nhat lai tieu de
                this.UpdateStatusBar();

                try
                {
                    //Rename image in thumbnail bar
                    thumbnailBar.Items[imgIndex].Text = e.Name;
                    thumbnailBar.Items[imgIndex].Tag = newName;
                }
                catch { }
            }
        }

        private void sysWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            //Get index of deleted image
            int imgIndex = GlobalSetting.ImageFilenameList.IndexOf(e.FullPath);

            if (imgIndex > -1)
            {
                //delete image list
                GlobalSetting.ImageList.Remove(imgIndex);
                GlobalSetting.ImageFilenameList.RemoveAt(imgIndex);

                try
                {
                    //delete thumbnail list
                    thumbnailBar.Items.RemoveAt(imgIndex);
                }
                catch { }

                NextPic(0);
            }
        }

        private void sysWatch_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
                NextPic(0);
            }
        }

        private void picMain_Zoomed(object sender, ImageBoxZoomEventArgs e)
        {
            this._isZoomed = true;
            this.UpdateStatusBar(true);
        }

        private void picMain_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle: //Refresh
                    mnuMainRefresh_Click(null, null);
                    break;

                case MouseButtons.XButton1: //Back
                    mnuMainViewPrevious_Click(null, null);
                    break;

                case MouseButtons.XButton2: //Next
                    mnuMainViewNext_Click(null, null);
                    break;

                default:
                    break;
            }

        }
        #endregion



        #region Toolbar Button
        private void btnNext_Click(object sender, EventArgs e)
        {
            mnuMainViewNext_Click(null, e);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            mnuMainViewPrevious_Click(null, e);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            mnuMainRefresh_Click(null, e);
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            mnuMainRotateClockwise_Click(null, e);
        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            mnuMainRotateCounterclockwise_Click(null, e);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            mnuMainOpenFile_Click(null, e);
        }

        private void btnThumb_Click(object sender, EventArgs e)
        {
            mnuMainThumbnailBar_Click(null, e);
        }

        private void btnActualSize_Click(object sender, EventArgs e)
        {
            mnuMainActualSize_Click(null, e);
        }

        private void btnScaletoWidth_Click(object sender, EventArgs e)
        {
            mnuMainScaleToWidth_Click(null, e);
        }

        private void btnScaletoHeight_Click(object sender, EventArgs e)
        {
            mnuMainScaleToHeight_Click(null, e);
        }

        private void btnWindowAutosize_Click(object sender, EventArgs e)
        {
            mnuMainWindowAdaptImage_Click(null, e);
        }

        private void btnGoto_Click(object sender, EventArgs e)
        {
            mnuMainGoto_Click(null, e);
        }

        private void btnCheckedBackground_Click(object sender, EventArgs e)
        {
            mnuMainCheckBackground_Click(null, e);
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            mnuMainZoomIn_Click(null, e);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            mnuMainZoomOut_Click(null, e);
        }

        private void btnZoomLock_Click(object sender, EventArgs e)
        {
            mnuMainLockZoomRatio_Click(null, e);
        }

        private void btnSlideShow_Click(object sender, EventArgs e)
        {
            mnuMainSlideShowStart_Click(null, null);
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            mnuMainFullScreen_Click(null, e);
        }

        private void btnPrintImage_Click(object sender, EventArgs e)
        {
            mnuMainPrint_Click(null, e);
        }

        private void btnFacebook_Click(object sender, EventArgs e)
        {
            mnuMainShareFacebook_Click(null, e);
        }

        private void btnExtension_Click(object sender, EventArgs e)
        {
            mnuMainExtensionManager_Click(null, e);
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            mnuMainSettings_Click(null, e);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            mnuMainAbout_Click(null, e);
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            mnuMainSaveAs_Click(null, e);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            mnuMainReportIssue_Click(null, e);
        }
        #endregion
        


        #region Popup Menu
        private void mnuPopup_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) ||
                                 GlobalSetting.IsImageError)
                {
                    e.Cancel = true;
                    return;
                }
            }
            catch { e.Cancel = true; return; }

            //clear current items
            mnuPopup.Items.Clear();

            if (GlobalSetting.IsPlaySlideShow)
            {
                mnuPopup.Items.Add(Library.Menu.Clone(mnuMainSlideShowPause));
                mnuPopup.Items.Add(Library.Menu.Clone(mnuMainSlideShowExit));
                mnuPopup.Items.Add(new ToolStripSeparator());//---------------
            }
            
            //toolbar menu
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainToolbar));
            mnuPopup.Items.Add(new ToolStripSeparator());//---------------

            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainEditImage));
            
            //check if image can animate (GIF)
            try
            {
                Image img = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                FrameDimension dim = new FrameDimension(img.FrameDimensionsList[0]);
                int frameCount = img.GetFrameCount(dim);

                if (frameCount > 1)
                {
                    var mi = Library.Menu.Clone(mnuMainExtractFrames);
                    mi.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"], frameCount);

                    mnuPopup.Items.Add(Library.Menu.Clone(mi));
                    mnuPopup.Items.Add(Library.Menu.Clone(mnuMainStartStopAnimating));
                }

            }
            catch { }

            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainSetAsDesktop));

            mnuPopup.Items.Add(new ToolStripSeparator());//------------
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainOpenImageData));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainCopy));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainCut));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainClearClipboard));

            mnuPopup.Items.Add(new ToolStripSeparator());//------------
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainRename));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainMoveToRecycleBin));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainDeleteFromHardDisk));

            mnuPopup.Items.Add(new ToolStripSeparator());//------------
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainShareFacebook));

            mnuPopup.Items.Add(new ToolStripSeparator());//------------
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainCopyImagePath));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainImageLocation));
            mnuPopup.Items.Add(Library.Menu.Clone(mnuMainImageProperties));

        }
        #endregion



        #region Main Menu (Main function)

        private void mnuMainOpenFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void mnuMainOpenImageData_Click(object sender, EventArgs e)
        {
            //Is there a file in clipboard ?--------------------------------------------------
            if (Clipboard.ContainsFileDropList())
            {
                string[] sFile = (string[])Clipboard.GetData(System.Windows.Forms.DataFormats.FileDrop);
                int fileCount = 0;

                fileCount = sFile.Length;

                //neu co file thi load
                Prepare(sFile[0]);
            }


            //Is there a image in clipboard ?-------------------------------------------------
            //CheckImageInClipboard: ;
            else if (Clipboard.ContainsImage())
            {
                picMain.Image = Clipboard.GetImage();
                GlobalSetting.IsTempMemoryData = true;
            }

            //Is there a filename in clipboard?-----------------------------------------------
            //CheckPathInClipboard: ;
            else if (Clipboard.ContainsText())
            {
                if (File.Exists(Clipboard.GetText()) || Directory.Exists(Clipboard.GetText()))
                {
                    Prepare(Clipboard.GetText());
                }
                //get image from Base64string 
                else
                {
                    try
                    {
                        // data:image/jpeg;base64,xxxxxxxx
                        string base64str = Clipboard.GetText().Substring(Clipboard.GetText().LastIndexOf(',') + 1);
                        var file_bytes = Convert.FromBase64String(base64str);
                        var file_stream = new MemoryStream(file_bytes);
                        var file_image = Image.FromStream(file_stream);

                        picMain.Image = file_image;
                        GlobalSetting.IsTempMemoryData = true;
                    }
                    catch { }
                }
            }
        }

        private void mnuMainSaveAs_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            if (filename == "")
            {
                filename = "untitled.png";
            }

            Library.Image.ImageInfo.ConvertImage(picMain.Image, filename);
        }

        private void mnuMainRefresh_Click(object sender, EventArgs e)
        {
            // Any scrolling from prior image would 'stick': reset here
            picMain.ScrollTo(0, 0, 0, 0);

            //Zoom condition
            if (btnZoomLock.Checked)
            {
                picMain.Zoom = GlobalSetting.ZoomLockValue;
            }
            else
            {
                //Reset zoom
                picMain.ZoomToFit();

                this._isZoomed = false;
            }

            //Get image file information
            this.UpdateStatusBar();
        }

        private void mnuMainEditImage_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.IsImageError)
            {
                return;
            }

            string filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.Verb = "edit";

            //show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();
            }
            catch (Exception)
            { }
        }

        private void mnuMainViewNext_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(1);
        }

        private void mnuMainViewPrevious_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(-1);
        }

        private void mnuMainGoto_Click(object sender, EventArgs e)
        {
            int n = GlobalSetting.CurrentIndex;
            string s = "0";
            if (InputBox.ShowDiaLog("Message", GlobalSetting.LangPack.Items["frmMain._GotoDialogText"],
                                    "0", true) == System.Windows.Forms.DialogResult.OK)
            {
                s = InputBox.Message;
            }

            if (int.TryParse(s, out n))
            {
                n--;

                if (-1 < n && n < GlobalSetting.ImageList.Length)
                {
                    GlobalSetting.CurrentIndex = n;
                    NextPic(0);
                }
            }
        }

        private void mnuMainGotoFirst_Click(object sender, EventArgs e)
        {
            GlobalSetting.CurrentIndex = 0;
            NextPic(0);
        }

        private void mnuMainGotoLast_Click(object sender, EventArgs e)
        {
            GlobalSetting.CurrentIndex = GlobalSetting.ImageList.Length - 1;
            NextPic(0);
        }

        private void mnuMainFullScreen_Click(object sender, EventArgs e)
        {
            //full screen
            if (!GlobalSetting.IsFullScreen)
            {
                SaveConfig();

                //save last state of toolbar
                this._isShownToolbar = GlobalSetting.IsShowToolBar;

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                GlobalSetting.IsFullScreen = true;
                Application.DoEvents();
                this.Bounds = Screen.FromControl(this).Bounds;

                //Hide
                toolMain.Visible = false;
                GlobalSetting.IsShowToolBar = false;
                mnuMainToolbar.Checked = false;

                this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._FullScreenMessage"]
                    , 5000);
            }
            //exit full screen
            else
            {
                //restore last state of toolbar
                GlobalSetting.IsShowToolBar = this._isShownToolbar;

                this.FormBorderStyle = FormBorderStyle.Sizable;

                //windows state
                string state_str = GlobalSetting.GetConfig("WindowsState", "Normal");
                if (state_str == "Normal")
                {
                    this.WindowState = FormWindowState.Normal;
                }
                else if (state_str == "Maximized")
                {
                    this.WindowState = FormWindowState.Maximized;
                }

                //Windows Bound (Position + Size)
                this.Bounds = GlobalSetting.StringToRect(GlobalSetting.GetConfig("WindowsBound", "280,125,750,545"));

                GlobalSetting.IsFullScreen = false;
                Application.DoEvents();

                if (GlobalSetting.IsShowToolBar)
                {
                    //Show toolbar
                    toolMain.Visible = true;
                    mnuMainToolbar.Checked = true;
                }
            }
        }
        

        private void mnuMainSlideShowStart_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            //not performing
            if (!GlobalSetting.IsPlaySlideShow)
            {
                //perform slideshow
                picMain.BackColor = Color.Black;
                btnFullScreen.PerformClick();

                timSlideShow.Start();
                timSlideShow.Enabled = true;

                GlobalSetting.IsPlaySlideShow = true;
            }
            //performing
            else
            {
                mnuMainSlideShowExit_Click(null, null);
            }

            this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SlideshowMessage"]
                    , 5000);
        }

        private void mnuMainSlideShowPause_Click(object sender, EventArgs e)
        {
            //performing
            if (timSlideShow.Enabled)
            {
                timSlideShow.Enabled = false;
                timSlideShow.Stop();
            }
            else
            {
                timSlideShow.Enabled = true;
                timSlideShow.Start();
            }

        }

        private void mnuMainSlideShowExit_Click(object sender, EventArgs e)
        {
            timSlideShow.Stop();
            timSlideShow.Enabled = false;
            GlobalSetting.IsPlaySlideShow = false;

            picMain.BackColor = GlobalSetting.BackgroundColor;
            btnFullScreen.PerformClick();
        }

        /// <summary>
        /// Save current loaded image to file and print it
        /// </summary>
        private string SaveTemporaryMemoryData()
        {
            //save temp file
            string temp_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                        "\\ImageGlass\\Temp\\";
            if (!Directory.Exists(temp_dir))
            {
                Directory.CreateDirectory(temp_dir);
            }

            string filename = temp_dir + "temp_" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".png";

            picMain.Image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

            return filename;
        }

        private void mnuMainPrint_Click(object sender, EventArgs e)
        {
            string filename = "";

            //save image from memory
            if (GlobalSetting.IsTempMemoryData)
            {
                filename = this.SaveTemporaryMemoryData();
            }
            //image error
            else if (GlobalSetting.ImageList.Length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }
            else
            {
                filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

                // check if file extension is NOT supported for native print
                // these extensions will not be printed by its associated app.
                if (GlobalSetting.SupportedExtraExtensions.Contains(Path.GetExtension(filename).ToLower()))
                {
                    filename = this.SaveTemporaryMemoryData();
                }
            }

            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.Verb = "print";

            //show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();
            }
            catch (Exception)
            { }

        }

        private void mnuMainRotateCounterclockwise_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null || picMain.CanAnimate)
            {
                return;
            }

            Bitmap bmp = new Bitmap(picMain.Image);
            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            picMain.Image = bmp;

            try
            {
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageFilenameList[GlobalSetting.CurrentIndex];
            }
            catch { }
        }

        private void mnuMainRotateClockwise_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null || picMain.CanAnimate)
            {
                return;
            }

            Bitmap bmp = new Bitmap(picMain.Image);
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            picMain.Image = bmp;

            try
            {
                LocalSetting.ImageModifiedPath = GlobalSetting.ImageFilenameList[GlobalSetting.CurrentIndex];
            }
            catch { }
        }

        private void mnuMainZoomIn_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomIn();
        }

        private void mnuMainZoomOut_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomOut();
        }

        private void mnuMainActualSize_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ActualSize();
        }

        private void mnuMainLockZoomRatio_Click(object sender, EventArgs e)
        {
            if (btnZoomLock.Checked)
            {
                GlobalSetting.ZoomLockValue = picMain.Zoom;
            }
            else
            {
                GlobalSetting.ZoomLockValue = 100;
                btnZoomLock.Checked = false;
            }
        }

        private void mnuMainScaleToWidth_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            // Scale to Width
            double frac = picMain.Width / (1.0 * picMain.Image.Width);
            picMain.Zoom = (int)(frac * 100);
        }

        private void mnuMainScaleToHeight_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            // Scale to Height
            double frac = picMain.Height / (1.0 * picMain.Image.Height);
            picMain.Zoom = (int)(frac * 100);
        }

        private void mnuMainWindowAdaptImage_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }
            
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            this.WindowState = FormWindowState.Normal;

            //if image size is bigger than screen
            if (picMain.Image.Width >= screen.Width || picMain.Height >= screen.Height)
            {
                this.Left = this.Top = 0;
                this.Width = screen.Width;
                this.Height = screen.Height;
            }
            else
            {
                this.Size = new Size(Width += picMain.Image.Width - picMain.Width,
                                Height += picMain.Image.Height - picMain.Height);

                picMain.Bounds = new Rectangle(Point.Empty, picMain.Image.Size);
                this.Top = (screen.Height - this.Height) / 2 + screen.Top;
                this.Left = (screen.Width - this.Width) / 2 + screen.Left;
            }
            
        }

        private void mnuMainRename_Click(object sender, EventArgs e)
        {
            RenameImage();
        }

        private void mnuMainMoveToRecycleBin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            string f = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            try
            {
                //in case of GIF file...
                string ext = Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToLower();
                if (ext == ".gif")
                {
                    try
                    {
                        //delete thumbnail list
                        thumbnailBar.Items.RemoveAt(GlobalSetting.CurrentIndex);
                    }
                    catch { }

                    //delete image list
                    GlobalSetting.ImageList.Remove(GlobalSetting.CurrentIndex);
                    GlobalSetting.ImageFilenameList.RemoveAt(GlobalSetting.CurrentIndex);

                    NextPic(0);
                }

                ImageInfo.DeleteFile(f, true);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void mnuMainDeleteFromHardDisk_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show(
                                string.Format(GlobalSetting.LangPack.Items["frmMain._DeleteDialogText"],
                                            GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)),
                                GlobalSetting.LangPack.Items["frmMain._DeleteDialogTitle"],
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg == DialogResult.Yes)
            {
                string f = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                try
                {
                    //Neu la anh GIF thi giai phong bo nho truoc khi xoa
                    string ext = Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToLower();
                    if (ext == ".gif")
                    {
                        try
                        {
                            //delete thumbnail list
                            thumbnailBar.Items.RemoveAt(GlobalSetting.CurrentIndex);
                        }
                        catch { }

                        //delete image list
                        GlobalSetting.ImageList.Remove(GlobalSetting.CurrentIndex);
                        GlobalSetting.ImageFilenameList.RemoveAt(GlobalSetting.CurrentIndex);

                        NextPic(0);
                    }

                    ImageInfo.DeleteFile(f);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mnuMainExtractFrames_Click(object sender, EventArgs e)
        {
            if (!(sender as ToolStripMenuItem).Enabled) // Shortcut keys still work even when menu is disabled!
                return;

            if (!GlobalSetting.IsImageError)
            {
                FolderBrowserDialog f = new FolderBrowserDialog();
                f.Description = GlobalSetting.LangPack.Items["frmMain._ExtractFrameText"];
                f.ShowNewFolderButton = true;
                DialogResult res = f.ShowDialog();

                if (res == DialogResult.OK && Directory.Exists(f.SelectedPath))
                {
                    Animation ani = new Animation();
                    ani.ExtractAllFrames(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex),
                                                f.SelectedPath);
                }

                f = null;
            }
        }

        // ReSharper disable once EmptyGeneralCatchClause
        private void mnuMainSetAsDesktop_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.IsImageError)
                return;

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igtasks.exe";
                p.StartInfo.Arguments = "setwallpaper " + //name of param
                                        "\"" + GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + "\" " + //arg 1
                                        "\"" + "0" + "\" "; //arg 2
                p.Start();
            }
            catch (Exception)
            { }
        }

        private void mnuMainImageLocation_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" +
                    GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + "\"");
            }
        }

        private void mnuMainImageProperties_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                ImageInfo.DisplayFileProperties(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex),
                                                this.Handle);
            }
        }

        private void mnuMainCopy_Click(object sender, EventArgs e)
        {
            CopyFile();
        }

        private void mnuMainCopyMulti_Click(object sender, EventArgs e)
        {
            CopyMultiFile();
        }

        private void mnuMainCut_Click(object sender, EventArgs e)
        {
            CutFile();
        }

        private void mnuMainCutMulti_Click(object sender, EventArgs e)
        {
            CutMultiFile();
        }

        private void mnuMainCopyImagePath_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            }
            catch { }
        }

        private void mnuMainClearClipboard_Click(object sender, EventArgs e)
        {
            //clear copied files in clipboard
            if (GlobalSetting.StringClipboard.Count > 0)
            {
                GlobalSetting.StringClipboard = new StringCollection();
                Clipboard.Clear();
                this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._ClearClipboard"], 1000);
            }
        }

        private void mnuMainShareFacebook_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0 && File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
            {
                if (LocalSetting.FFacebook.IsDisposed)
                {
                    LocalSetting.FFacebook = new frmFacebook();
                }

                //CHECK FILE EXTENSION BEFORE UPLOADING
                string filename = "";

                //save image from memory
                if (GlobalSetting.IsTempMemoryData)
                {
                    filename = this.SaveTemporaryMemoryData();
                }
                //image error
                else if (GlobalSetting.ImageList.Length < 1 || GlobalSetting.IsImageError)
                {
                    return;
                }
                else
                {
                    filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);

                    // check if file extension is NOT supported for native print
                    // these extensions will not be printed by its associated app.
                    if (GlobalSetting.SupportedExtraExtensions.Contains(Path.GetExtension(filename).ToLower()))
                    {
                        filename = this.SaveTemporaryMemoryData();
                    }
                }

                LocalSetting.FFacebook.Filename = filename;
                GlobalSetting.IsForcedActive = false;
                LocalSetting.FFacebook.Show();
                LocalSetting.FFacebook.Activate();
            }
        }

        private void mnuMainToolbar_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowToolBar = !GlobalSetting.IsShowToolBar;
            if (GlobalSetting.IsShowToolBar)
            {
                //Hien
                toolMain.Visible = true;
            }
            else
            {
                //An
                toolMain.Visible = false;
            }
            mnuMainToolbar.Checked = GlobalSetting.IsShowToolBar;
        }

        private void mnuMainThumbnailBar_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;
            sp1.Panel2Collapsed = !GlobalSetting.IsShowThumbnail;
            btnThumb.Checked = GlobalSetting.IsShowThumbnail;

            if (GlobalSetting.IsShowThumbnail)
            {
                //hien
                sp1.Panel2MinSize = GlobalSetting.ThumbnailDimension + 40;
                sp1.SplitterDistance = sp1.Height - GlobalSetting.ThumbnailDimension - 41;
            }

            mnuMainThumbnailBar.Checked = GlobalSetting.IsShowThumbnail;
            SelectCurrentThumbnail();
        }

        private void mnuMainCheckBackground_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowCheckedBackground = !GlobalSetting.IsShowCheckedBackground;
            btnCheckedBackground.Checked = GlobalSetting.IsShowCheckedBackground;

            if (btnCheckedBackground.Checked)
            {
                //show
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.Client;
            }
            else
            {
                //hide
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.None;
            }

            mnuMainCheckBackground.Checked = btnCheckedBackground.Checked;
        }

        private void mnuMainExtensionManager_Click(object sender, EventArgs e)
        {
            if (LocalSetting.FExtension.IsDisposed)
            {
                LocalSetting.FExtension = new frmExtension();
            }
            GlobalSetting.IsForcedActive = false;
            LocalSetting.FExtension.Show();
            LocalSetting.FExtension.Activate();
        }

        private void mnuMainSettings_Click(object sender, EventArgs e)
        {
            if (LocalSetting.FSetting.IsDisposed)
            {
                LocalSetting.FSetting = new frmSetting();
            }

            GlobalSetting.IsForcedActive = false;
            LocalSetting.FSetting.Show();
            LocalSetting.FSetting.Activate();
        }

        private void mnuMainAbout_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog();
        }

        private void mnuMainReportIssue_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/d2phap/ImageGlass/issues");
            }
            catch { }
        }

        private void mnuMainStartStopAnimating_Click(object sender, EventArgs e)
        {
            if (picMain.IsAnimating)
            {
                picMain.StopAnimating();
            }
            else
            {
                picMain.StartAnimating();
            }
        }

        private void mnuMain_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                mnuMainExtractFrames.Enabled = false;
                mnuMainStartStopAnimating.Enabled = false;

                Image img = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                FrameDimension dim = new FrameDimension(img.FrameDimensionsList[0]);
                int frameCount = img.GetFrameCount(dim);

                mnuMainExtractFrames.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuMainExtractFrames"], frameCount);

                if (frameCount > 1)
                {
                    mnuMainExtractFrames.Enabled = true;
                    mnuMainStartStopAnimating.Enabled = true;
                }

            }
            catch { }
        }


        #endregion

        
    }
}
