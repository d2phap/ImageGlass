/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
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
using System.Drawing.IconLib;
using System.Diagnostics;
using ImageGlass.Services.Configuration;
using ImageGlass.Library;
using ImageGlass.ThumbBar.ImageHandling;
using ImageGlass.ThumbBar;
using System.Collections.Specialized;

namespace ImageGlass
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }


        #region Local variables
        private Rectangle rect = Rectangle.Empty; // Window size
        private string imageInfo = "";
        private delegate void DelegateAddImage(string imageFilename);
        private DelegateAddImage m_AddImageDelegate;
        #endregion


        #region Drag - drop
        private void picMain_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void picMain_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Prepare(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            }
            catch { }
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
        private void Prepare(string path)
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

            //Check wheather show thumbnail or not
            if (GlobalSetting.IsShowThumbnail)
            {
                //Load thumnbnail
                LoadThumnailImage();
            }

            //Watch all change of current path
            sysWatch.Path = Path.GetDirectoryName(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            sysWatch.EnableRaisingEvents = true;
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
                new Predicate<string>(delegate(String f)
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
        private void LoadThumnailImage()
        {
            //List of image position
            List<int> dsIndex = new List<int>();

            //Stop current thumbnail generation thread
            thumbBar.StopThumbnailsGeneration();

            //Remove all current thumbnails
            thumbBar.DisposeOfPreviousThumbnails();

            //Item list will be loaded
            List<string> files = new List<string>();

            //Build file list
            for (int i = 0; i < GlobalSetting.ImageList.Length; i++)
            {
                Application.DoEvents();
                files.Add(GlobalSetting.ImageList.GetFileName(i));
            }

            //Add generation event
            thumbBar.OnGeneratingThumbnailItem += thumbBar_OnGeneratingThumbnailItem;

            //Show thumbnail
            thumbBar.ShowThumbnails(files);
        }

        /// <summary>
        /// Change image
        /// </summary>
        /// <param name="step">Image step to change. Zero is reload the current image.</param>
        private void NextPic(int step)
        {
            picMain.Text = "";

            if (GlobalSetting.ImageList.Length < 1)
            {
                this.Text = "ImageGlass";
                lblInfo.Text = string.Empty;
                
                GlobalSetting.IsImageError = true;
                picMain.Image = null;

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
                //Check if the image is a icon or not
                if (Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToLower() == ".ico")
                {
                    try
                    {
                        MultiIcon mIcon = new MultiIcon();
                        mIcon.Load(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
                        
                        //Try to get the largest image of it
                        SingleIcon sIcon = mIcon[0];
                        IconImage iImage = sIcon.OrderByDescending(ico => ico.Size.Width).ToList()[0];

                        //Convert to bitmap
                        im = iImage.Icon.ToBitmap();
                    }
                    catch //If a invalid icon
                    {
                        im = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                        
                    }
                }
                else //If a normal image
                {
                    im = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex);
                }

                GlobalSetting.IsImageError = GlobalSetting.ImageList.IsErrorImage;

                //Show image
                picMain.Image = im;

                //Zoom condition
                if(btnZoomLock.Checked)
                {
                    picMain.Zoom = GlobalSetting.ZoomLockValue;
                }
                else
                {
                    //Reset zoom
                    picMain.ZoomToFit();
                }
                
                //Get image file information
                this.UpdateStatusBar();

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
                
                Application.DoEvents();
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
                }
            }

            if(GlobalSetting.IsImageError)
            {
                picMain.Text = GlobalSetting.LangPack.Items["frmMain.picMain._ErrorText"];
                picMain.Image = null; 
            }

            //Select thumbnail item
            if (GlobalSetting.IsShowThumbnail)
            {
                if (thumbBar.Controls.Count > 0)
                {
                    try
                    {
                        ThumbnailBox tb = (ThumbnailBox)thumbBar.Controls[GlobalSetting.CurrentIndex];
                        thumbBar.MoveToThumbnail(tb);
                    }
                    catch { }
                }
            }//end thumbnail

            //Collect system garbage
            System.GC.Collect();
        }

        /// <summary>
        /// Update image information on status bar
        /// </summary>
        private void UpdateStatusBar(bool @zoomOnly = false)
        {
            string fileinfo = "";

            if(GlobalSetting.ImageList.Length < 1)
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
                    fileinfo += File.GetCreationTime(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToString();
                    this.imageInfo = fileinfo;
                }
                catch { fileinfo = ""; }
            }
            else
            {
                fileinfo += picMain.Image.Width + " x " + picMain.Image.Height + " px  |  ";

                if (zoomOnly)
                {
                    fileinfo = picMain.Zoom.ToString() + "%  |  " + imageInfo;
                }
                else
                {
                    fileinfo += ImageInfo.GetFileSize(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)) + "\t  |  ";
                    fileinfo += Path.GetExtension(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).Replace(".", "").ToUpper() + "  |  ";
                    fileinfo += File.GetCreationTime(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)).ToString();

                    this.imageInfo = fileinfo;

                    fileinfo = picMain.Zoom.ToString() + "%  |  " + fileinfo;
                }
            }

            lblInfo.Text = fileinfo;

            //Check if Toolbar is hide or not
            if (GlobalSetting.IsHideToolBar)
            {
                //Move image information to Window title
                this.Text += "  |  " + fileinfo;
            }
        }
        #endregion


        #region Key event
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //this.Text = e.KeyValue.ToString();

            //===================================\\'
            //=       Do loading image by       =\\'
            //=  looking for data in Clipboard  =\\'
            //===================================\\'
            #region Ctrl + V
            if (e.KeyCode == Keys.V && e.Control && !e.Shift && !e.Alt)//Ctrl + V
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
                }

                //Is there a filename in clipboard?-----------------------------------------------
                //CheckPathInClipboard: ;
                else if (Clipboard.ContainsText())
                {
                    if (File.Exists(Clipboard.GetText()) || Directory.Exists(Clipboard.GetText()))
                    {
                        Prepare(Clipboard.GetText());
                    }
                }


            }
            #endregion

            // Copy file to clipboard-------------------------------------------------------
            #region Ctrl + C
            if (e.KeyValue == 67 && e.Control && !e.Shift && !e.Alt)
            {
                CopyFile();
            }
            #endregion

            // Rotation Counterclockwise----------------------------------------------------
            #region Ctrl + ,
            if (e.KeyValue == 188 && e.Control && !e.Shift && !e.Alt)//Ctrl + ,
            {
                btnRotateLeft_Click(null, null);
                return;
            }
            #endregion


            //Rotate Clockwise--------------------------------------------------------------
            #region Ctrl + .
            if (e.KeyValue == 190 && e.Control && !e.Shift && !e.Alt)//Ctrl + .
            {
                btnRotateRight_Click(null, null);
                return;
            }
            #endregion


            //Play slideshow----------------------------------------------------------------
            #region F11
            if (e.KeyValue == 122 && !e.Control && !e.Shift && !e.Alt)//F11
            {
                btnSlideShow_Click(null, null);
                return;
            }
            #endregion


            //Exit slideshow----------------------------------------------------------------
            #region ESC
            if (e.KeyValue == 27 && !e.Control && !e.Shift && !e.Alt)//ESC
            {
                if (GlobalSetting.IsPlaySlideShow)
                {
                    mnuExitSlideshow_Click(null, null);
                }
                return;
            }
            #endregion


            //Start / stop slideshow---------------------------------------------------------
            #region SPACE
            if (GlobalSetting.IsPlaySlideShow && e.KeyCode == Keys.Space && !e.Control && !e.Shift && !e.Alt)//SPACE
            {
                if (timSlideShow.Enabled)//stop
                {
                    mnuStopSlideshow_Click(null, null);
                }
                else//start
                {
                    mnuStartSlideshow_Click(null, null);
                }
                
                return;
            }
            #endregion


            // Help ------------------------------------------------------------------------
            #region F1
            if (e.KeyValue == 112 && !e.Control && !e.Shift && !e.Alt)//F1
            {
                btnHelp_Click(null, null);
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


            //Scale to Width------------------------------------------------------------------
            #region Ctrl + W
            if (e.KeyCode == Keys.W && e.Control && !e.Shift && !e.Alt)// Ctrl + W
            {
                btnScaletoWidth_Click(null, null);
                return;
            }
            #endregion


            //Scale to Height-----------------------------------------------------------------
            #region Ctrl + H
            if (e.KeyCode == Keys.H && e.Control && !e.Shift && !e.Alt)// Ctrl + H
            {
                btnScaletoHeight_Click(null, null);
                return;
            }
            #endregion


            //Auto size window----------------------------------------------------------------
            #region Ctrl + M
            if (e.KeyCode == Keys.M && e.Control && !e.Shift && !e.Alt)// Ctrl + M
            {
                btnWindowAutosize_Click(null, null);
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


            //Lock zoom ratio--- -------------------------------------------------------------
            #region Ctrl + R
            if (e.KeyData == Keys.R && e.Control && !e.Shift && !e.Alt)// Ctrl + R
            {
                btnZoomLock.PerformClick();
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


            //Open file----------------------------------------------------------------------
            #region Ctrl + O
            if (e.KeyValue == 79 && e.Control && !e.Shift && !e.Alt)// Ctrl + O
            {
                OpenFile();
                return;
            }
            #endregion


            //Convert file-------------------------------------------------------------------
            #region Ctrl + S
            if (e.KeyValue == 83 && e.Control && !e.Shift && !e.Alt)// Ctrl + S
            {
                btnConvert_Click(null, null);
                return;
            }
            #endregion


            //Refresh Image------------------------------------------------------------------
            #region F5
            if (e.KeyValue == 116 && !e.Control && !e.Shift && !e.Alt)//F5
            {
                btnRefresh_Click(null, null);
                return;
            }
            #endregion


            //Goto image---------------------------------------------------------------------
            #region Ctrl + G
            if (e.KeyValue == 71 && e.Control && !e.Shift && !e.Alt)//Ctrl + G
            {
                btnGoto_Click(null, null);
                return;
            }
            #endregion


            //Extract frames-----------------------------------------------------------------
            #region Ctrl + E
            if (e.KeyCode == Keys.E && e.Control && !e.Shift && !e.Alt)//Ctrl + E
            {
                try
                {
                    GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                    mnuExtractFrames_Click(null, null);
                }
                catch { }
                return;
            }
            #endregion


            //Detele to Recycle bin----------------------------------------------------------
            #region DELETE
            if (e.KeyCode == Keys.Delete && !e.Control && !e.Shift && !e.Alt)//Delete
            {
                mnuRecycleBin_Click(null, null);
                return;
            }
            #endregion


            //Detele From Hark disk----------------------------------------------------------
            #region SHIFT + DELETE
            if (e.KeyCode == Keys.Delete && !e.Control && e.Shift && !e.Alt)//Shift + Delete
            {
                mnuDelete_Click(null, null);
                return;
            }
            #endregion


            //Lock zoom ratio----------------------------------------------------------------
            #region Ctrl + L
            if (e.KeyCode == Keys.L && e.Control && !e.Shift && !e.Alt)//Ctrl + L
            {
                btnZoomLock.PerformClick();
                return;
            }
            #endregion


            //Image properties----------------------------------------------------------------
            #region Ctrl + I
            if (e.KeyCode == Keys.I && e.Control && !e.Shift && !e.Alt)//Ctrl + I
            {
                mnuProperties_Click(null, null);
                return;
            }
            #endregion


            //Show thumbnail-------------------------------------------------------------------
            #region Ctrl + T
            if (e.KeyCode == Keys.T && e.Control && !e.Shift && !e.Alt)// Ctrl + T
            {
                btnThumb.PerformClick();
                return;
            }
            #endregion


            //Caro background------------------------------------------------------------------
            #region Ctrl + B
            if (e.KeyCode == Keys.B && e.Control && !e.Shift && !e.Alt)// Ctrl + B
            {
                btnCaro.PerformClick();
                return;
            }
            #endregion


            //Print Image-----------------------------------------------------------------------
            #region Ctrl + P
            if (e.KeyCode == Keys.P && e.Control && !e.Shift && !e.Alt)// Ctrl + P
            {
                btnPrintImage_Click(null, null);
                return;
            }
            #endregion


            //Show / Hide Toolbar-----------------------------------------------------------------
            #region Ctrl + F1
            if (e.KeyCode == Keys.F1 && e.Control && !e.Shift && !e.Alt)// Ctrl + F1
            {
                mnuShowToolBar_Click(null, null);
                return;
            }
            #endregion


            //Upload image to Facebook------------------------------------------------------------
            #region Ctrl + U
            if (e.KeyCode == Keys.U && e.Control && !e.Shift && !e.Alt)// Ctrl + U
            {
                btnFacebook_Click(btnFacebook, e);
                return;
            }
            #endregion
            

            //Rename image----------------------------------------------------------------------
            #region F2
            if (e.KeyValue == 113 && !e.Control && !e.Shift && !e.Alt)//F2
            {
                RenameImage();
                return;
            }
            #endregion

            //Show Settings dialog--------------------------------------------------------------
            #region Ctrl + Shift + P
            if (e.KeyCode == Keys.P && e.Control && e.Shift && !e.Alt)// Ctrl + Shift + P
            {
                btnSetting_Click(null, null);
                return;
            }
            #endregion


            //Show Extension Manager dialog-----------------------------------------------------
            #region Ctrl + Shift + E
            if (e.KeyCode == Keys.E && e.Control && e.Shift && !e.Alt)// Ctrl + Shift + P
            {
                btnExtension_Click(null, null);
                return;
            }
            #endregion


            //Image location--------------------------------------------------------------------
            #region Ctrl + Shift + L
            if (e.KeyCode == Keys.L && e.Control && e.Shift && !e.Alt)//Ctrl + Shift + L
            {
                mnuImageLocation_Click(null, null);
                return;
            }
            #endregion


        }

        #endregion 


        #region Private functions

        /// <summary>
        /// Start optimization
        /// </summary>
        private void ZoomOptimization()
        {
            if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationValue.Auto)
            {
                if (picMain.Zoom == 150)
                {
                    picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                }
                else if (picMain.Zoom == 70)
                {
                    picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                }
            }
            else if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationValue.ClearPixels)
            {
                picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
            else if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationValue.SmoothPixels)
            {
                picMain.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            }
        }

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
        /// <param name="msg"></param>
        /// <param name="duration"></param>
        private void DisplayTextMessage(string msg, int duration)
        {
            System.Windows.Forms.Timer tmsg = new System.Windows.Forms.Timer();
            tmsg.Enabled = false;
            tmsg.Tick += tmsg_Tick;
            tmsg.Interval = duration; //display in xxx seconds

            picMain.TextBackColor = Color.Black;
            picMain.Font = new System.Drawing.Font(this.Font.FontFamily, 14);
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
            
            picMain.TextBackColor = Color.Transparent;
            picMain.Font = this.Font;
            picMain.ForeColor = Color.Black;
            picMain.Text = string.Empty;
        }

        /// <summary>
        /// Copy file to Clipboard
        /// </summary>
        private void CopyFileToClipBoard(string filename, ref StringCollection pathCollection)
        {
            pathCollection.Add(filename);
            Clipboard.SetFileDropList(pathCollection);
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

            StringCollection pathCollection = new StringCollection();
            
            CopyFileToClipBoard(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex), 
                ref pathCollection);
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
            sp0.Panel2.BackgroundImage = ImageGlass.Properties.Resources.bottombar;
            lblInfo.ForeColor = Color.Black;

            picMain.BackColor = Color.White;

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
            btnCaro.Image = ImageGlass.Properties.Resources.caro;
            btnFullScreen.Image = ImageGlass.Properties.Resources.fullscreen;
            btnSlideShow.Image = ImageGlass.Properties.Resources.slideshow;
            btnConvert.Image = ImageGlass.Properties.Resources.convert;
            btnPrintImage.Image = ImageGlass.Properties.Resources.printer;
            btnFacebook.Image = ImageGlass.Properties.Resources.uploadfb;
            btnExtension.Image = ImageGlass.Properties.Resources.extension;
            btnSetting.Image = ImageGlass.Properties.Resources.settings;
            btnHelp.Image = ImageGlass.Properties.Resources.about;
            btnFacebookLike.Image = ImageGlass.Properties.Resources.facebook;
            btnFollow.Image = ImageGlass.Properties.Resources.follow;
            btnReport.Image = ImageGlass.Properties.Resources.report;

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

                try { sp0.Panel2.BackgroundImage = Image.FromFile(dir + t.bottombar); }
                catch { sp0.Panel2.BackgroundImage = ImageGlass.Properties.Resources.bottombar; }

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

                try { btnCaro.Image = Image.FromFile(dir + t.caro); }
                catch { btnCaro.Image = ImageGlass.Properties.Resources.caro; }

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

                try { btnFacebookLike.Image = Image.FromFile(dir + t.like); }
                catch { btnFacebookLike.Image = ImageGlass.Properties.Resources.facebook; }

                try { btnFollow.Image = Image.FromFile(dir + t.dislike); }
                catch { btnFollow.Image = ImageGlass.Properties.Resources.follow; }

                try { btnReport.Image = Image.FromFile(dir + t.report); }
                catch { btnReport.Image = ImageGlass.Properties.Resources.report; }

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
            GlobalSetting.SetConfig("igVersion", Application.ProductVersion.ToString());

            //Load language pack-------------------------------------------------------------
            string s = GlobalSetting.GetConfig("Language", "English");
            if (s.ToLower().CompareTo("english") != 0 && File.Exists(s))
            {
                GlobalSetting.LangPack = new Library.Language(s);
            }

            //Windows Bound (Position + Size)------------------------------------------------
            Rectangle rc = GlobalSetting.StringToRect(GlobalSetting.GetConfig("WindowsBound", "280,125,750,545"));
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

            //Slideshow Interval-----------------------------------------------------------
            int i = int.Parse(GlobalSetting.GetConfig("Interval", "5"));
            if (!(0 < i && i < 61)) i = 5;//time limit [1; 60] seconds
            timSlideShow.Interval = 1000 * i;

            //Show checked bakcground-------------------------------------------------------
            if (bool.Parse(GlobalSetting.GetConfig("Caro", "False").ToString()))
            {
                btnCaro.PerformClick();
            }

            //Recursive loading--------------------------------------------------------------
            GlobalSetting.IsRecursive = bool.Parse(GlobalSetting.GetConfig("Recursive", "False"));
            
            //Get welcome screen------------------------------------------------------------
            GlobalSetting.IsWelcomePicture = bool.Parse(GlobalSetting.GetConfig("Welcome", "True"));

            //Zoom optimization method-------------------------------------------------------
            string z = GlobalSetting.GetConfig("ZoomOptimize", "auto");
            if (z.ToLower() == "smooth pixels")
            {
                GlobalSetting.ZoomOptimizationMethod = ZoomOptimizationValue.SmoothPixels;
            }
            else if (z.ToLower() == "clear pixels")
            {
                GlobalSetting.ZoomOptimizationMethod = ZoomOptimizationValue.ClearPixels;
            }
            else //auto
            {
                GlobalSetting.ZoomOptimizationMethod = ZoomOptimizationValue.Auto;
            }
            
            //Load default image------------------------------------------------------------
            string y = GlobalSetting.GetConfig("Welcome", "True");
            if (y.ToLower() == "true")
            {
                Prepare((Application.StartupPath + "\\").Replace("\\\\", "\\") + "default.png");
            }

            //Load is loop back slideshow---------------------------------------------------
            GlobalSetting.IsLoopBackSlideShow = bool.Parse(GlobalSetting.GetConfig("IsLoopBackSlideShow", "True"));

            //Load image order config------------------------------------------------------
            GlobalSetting.LoadImageOrderConfig();

            //Load theme--------------------------------------------------------------------
            LoadTheme();
            
            //Load background---------------------------------------------------------------
            z = GlobalSetting.GetConfig("BackgroundColor", "-1");
            GlobalSetting.BackgroundColor = Color.FromArgb(int.Parse(z));
            picMain.BackColor = GlobalSetting.BackgroundColor;

            //Load state of Toolbar---------------------------------------------------------
            if (bool.Parse(GlobalSetting.GetConfig("IsHideToolbar", "false")))//Invisible
            {
                //Hide
                spMain.Panel1Collapsed = true;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];
                GlobalSetting.IsHideToolBar = true;
            }
            else//Visible
            {
                //Show
                spMain.Panel1Collapsed = false;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
                GlobalSetting.IsHideToolBar = false;
            }
        }


        /// <summary>
        /// Save app configurations
        /// </summary>
        private void SaveConfig()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig("WindowsBound", GlobalSetting.RectToString(rect == Rectangle.Empty ?
                                                                    this.Bounds : rect));
            }            

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig("WindowsState", this.WindowState.ToString());
            
            //Checked background
            GlobalSetting.SetConfig("Caro", btnCaro.Checked.ToString());
            
        }

        #endregion


        #region Form events
        private void frmMain_Load(object sender, EventArgs e)
        {
            //Remove white line under tool strip
            toolMain.Renderer = new ImageGlass.Theme.ToolStripRenderer();

            LoadConfig();
            Application.DoEvents();

            //Load image from param
            if (Program.args.Length > 0)
            {
                string filename = "";
                Program.args.ToList().ForEach(i => filename += " " + i);

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

            sp0.SplitterDistance = sp0.Height - 71;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Stop Thumbnails Generation thread
            thumbBar.StopThumbnailsGeneration();

            SaveConfig();
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (GlobalSetting.IsForcedActive)
            {
                picMain.BackColor = GlobalSetting.BackgroundColor;

                //Change language
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
                btnCaro.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnCaro"];
                btnFullScreen.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFullScreen"];
                btnSlideShow.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnSlideShow"];
                btnConvert.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnConvert"];
                btnPrintImage.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnPrintImage"];
                btnFacebook.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFacebook"];
                btnExtension.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnExtension"];
                btnSetting.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnSetting"];
                btnHelp.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnHelp"];
                btnFacebookLike.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFacebookLike"];
                btnFollow.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnFollow"];
                btnReport.ToolTipText = GlobalSetting.LangPack.Items["frmMain.btnReport"];

                mnuStartSlideshow.Text = GlobalSetting.LangPack.Items["frmMain.mnuStartSlideshow"];
                mnuStopSlideshow.Text = GlobalSetting.LangPack.Items["frmMain.mnuStopSlideshow"];
                mnuExitSlideshow.Text = GlobalSetting.LangPack.Items["frmMain.mnuExitSlideshow"];
                mnuEditWithPaint.Text = GlobalSetting.LangPack.Items["frmMain.mnuEditWithPaint"];
                mnuExtractFrames.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuExtractFrames"], 0);
                mnuSetWallpaper.Text = GlobalSetting.LangPack.Items["frmMain.mnuSetWallpaper"];
                mnuMoveRecycle.Text = GlobalSetting.LangPack.Items["frmMain.mnuMoveRecycle"];
                mnuDelete.Text = GlobalSetting.LangPack.Items["frmMain.mnuDelete"];
                mnuRename.Text = GlobalSetting.LangPack.Items["frmMain.mnuRename"];
                mnuUploadFacebook.Text = GlobalSetting.LangPack.Items["frmMain.mnuUploadFacebook"];
                mnuCopyImagePath.Text = GlobalSetting.LangPack.Items["frmMain.mnuCopyImagePath"];
                mnuOpenLocation.Text = GlobalSetting.LangPack.Items["frmMain.mnuOpenLocation"];
                mnuImageProperties.Text = GlobalSetting.LangPack.Items["frmMain.mnuImageProperties"];
            }

            GlobalSetting.IsForcedActive = false;
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void thumbBar_OnGeneratingThumbnailItem(object sender, GeneratingEventArgs e)
        {
            try
            {
                e.ThumbnailItem.OnClickThumbnailItem -= ThumbnailItem_OnClickThumbnailItem;
            }
            catch { }

            e.ThumbnailItem.OnClickThumbnailItem += ThumbnailItem_OnClickThumbnailItem;
        }

        private void ThumbnailItem_OnClickThumbnailItem(object sender, ThumbnailItemClickEventArgs e)
        {
            GlobalSetting.CurrentIndex = e.Index;
            NextPic(0);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(1); 
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            NextPic(-1); //xem anh truoc do
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            NextPic(0);
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            Bitmap bmp = new Bitmap(picMain.Image);
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            picMain.Image = bmp;
        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            Bitmap bmp = new Bitmap(picMain.Image);
            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            picMain.Image = bmp;

            //bmp.Save(GlobalSetting.ImageFilenameList[GlobalSetting.CurrentIndex]);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void btnThumb_Click(object sender, EventArgs e)
        {
            GlobalSetting.IsShowThumbnail = !GlobalSetting.IsShowThumbnail;
            sp0.Panel2Collapsed = !GlobalSetting.IsShowThumbnail;

            if (GlobalSetting.IsShowThumbnail)
            {
                sp0.Panel2MinSize = 70;
                sp0.SplitterDistance = sp0.Height - 71;
                if (thumbBar.Controls.Count == 0)
                {
                    LoadThumnailImage();
                }
            }
        }

        private void btnActualSize_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ActualSize();
        }

        private void btnScaletoWidth_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            // Scale to Width
            double frac = sp0.Panel1.Width / (1.0 * picMain.Image.Width);
            picMain.Zoom = (int)(frac * 100);
        }

        private void btnScaletoHeight_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            // Scale to Height
            double frac = sp0.Panel1.Height / (1.0 * picMain.Image.Height);
            picMain.Zoom = (int)(frac * 100);
        }

        private void btnWindowAutosize_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            // Window adapt to image
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(Width += picMain.Image.Width - sp0.Panel1.Width,
                                Height += picMain.Image.Height - sp0.Panel1.Height);
            //Application.DoEvents();
            picMain.Bounds = new Rectangle(Point.Empty, picMain.Image.Size);
            this.Top = (screen.Height - this.Height) / 2 + screen.Top;
            this.Left = (screen.Width - this.Width) / 2 + screen.Left;
        }

        private void btnGoto_Click(object sender, EventArgs e)
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

        private void btnCaro_Click(object sender, EventArgs e)
        {
            if (btnCaro.Checked)
            {
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.Client;
            }
            else
            {
                picMain.GridDisplayMode = ImageBoxGridDisplayMode.None;
            }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomIn();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (picMain.Image == null)
            {
                return;
            }

            picMain.ZoomOut();
        }

        private void btnZoomLock_Click(object sender, EventArgs e)
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

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            NextPic(1);

            //stop playing slideshow at last image
            if (GlobalSetting.CurrentIndex == GlobalSetting.ImageList.Length - 1)
            {
                if (!GlobalSetting.IsLoopBackSlideShow)
                {
                    mnuStopSlideshow_Click(null, null);
                }
            }
        }

        private void btnSlideShow_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1)
            {
                return;
            }

            //full screen
            if (this.rect == Rectangle.Empty)
            {
                sp0.BackColor = Color.Black;
                btnFullScreen.PerformClick();

                timSlideShow.Start();
                timSlideShow.Enabled = true;

                GlobalSetting.IsPlaySlideShow = true;
                mnuStartSlideshow.Visible = true;
                mnuStopSlideshow.Visible = true;
                mnuExitSlideshow.Visible = true;
                mnuPhanCach.Visible = true;
                mnuStartSlideshow.Enabled = false;
                mnuStopSlideshow.Enabled = true;
            }
            else
            {
                btnFullScreen.PerformClick();
                btnSlideShow_Click(sender, e);
            }

            this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._SlideshowMessage"]
                    , 5000);
        }

        private void btnFullScreen_Click(object sender, EventArgs e)
        {
            //full screen
            if (this.rect == Rectangle.Empty)
            {
                this.rect = this.Bounds;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                Application.DoEvents();
                this.Bounds = Screen.FromControl(this).Bounds;

                //Hide
                spMain.Panel1Collapsed = true;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];

                this.DisplayTextMessage(GlobalSetting.LangPack.Items["frmMain._FullScreenMessage"]
                    , 5000);
            }
            //exit full screen
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                Application.DoEvents();
                this.Bounds = this.rect;
                this.rect = Rectangle.Empty;

                //Show
                if (!GlobalSetting.IsHideToolBar)
                {
                    spMain.Panel1Collapsed = false;
                    mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
                }
            }
        }

        private void btnPrintImage_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }

            Process p = new Process();
            p.StartInfo.FileName = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
            p.StartInfo.Verb = "print";
            p.Start();
        }

        private void btnFacebook_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0 && File.Exists(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)))
            {
                if (LocalSetting.FFacebook.IsDisposed)
                {
                    LocalSetting.FFacebook = new frmFacebook();
                }

                LocalSetting.FFacebook.Filename = GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex);
                GlobalSetting.IsForcedActive = false;
                LocalSetting.FFacebook.Show();
                LocalSetting.FFacebook.Activate();
            }
        }

        private void btnExtension_Click(object sender, EventArgs e)
        {
            if (LocalSetting.FExtension.IsDisposed)
            {
                LocalSetting.FExtension = new frmExtension();
            }
            GlobalSetting.IsForcedActive = false;
            LocalSetting.FExtension.Show();
            LocalSetting.FExtension.Activate();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (LocalSetting.FSetting.IsDisposed)
            {
                LocalSetting.FSetting = new frmSetting();
            }

            GlobalSetting.IsForcedActive = false;
            LocalSetting.FSetting.Show();
            LocalSetting.FSetting.Activate();
        }

        private void mnuShowToolBar_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.IsHideToolBar)//Dang An
            {
                //Hien
                spMain.Panel1Collapsed = false;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
                GlobalSetting.IsHideToolBar = false;
            }
            else//Dang Hien
            {
                //An
                spMain.Panel1Collapsed = true;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];
                GlobalSetting.IsHideToolBar = true;
            }
        }

        private void mnuEditWithPaint_Click(object sender, EventArgs e)
        {
            if (!GlobalSetting.IsImageError)
            {
                System.Diagnostics.Process.Start("mspaint.exe", char.ConvertFromUtf32(34) + GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + char.ConvertFromUtf32(34));
            }
        }

        private void mnuProperties_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                ImageInfo.DisplayFileProperties(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex),
                                                this.Handle);
            }
        }

        private void mnuImageLocation_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.Length > 0)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + 
                    GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + "\"");
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
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
                            thumbBar.Controls.RemoveAt(GlobalSetting.CurrentIndex);
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

        private void mnuRecycleBin_Click(object sender, EventArgs e)
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
                                string.Format(GlobalSetting.LangPack.Items["frmMain._RecycleBinDialogText"],
                                                GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex)),
                                GlobalSetting.LangPack.Items["frmMain._RecycleBinDialogTitle"],
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
                            thumbBar.Controls.RemoveAt(GlobalSetting.CurrentIndex);
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
        }

        private void mnuWallpaper_Click(object sender, EventArgs e)
        {
            if (!GlobalSetting.IsImageError)
            {
                Process p = new Process();
                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igtasks.exe";
                p.StartInfo.Arguments = "setwallpaper " + //name of param
                                        "\"" + GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex) + "\" " + //arg 1
                                        "\"" + "0" + "\" "; //arg 2
                
                p.Start();
            }
        }

        private void mnuExtractFrames_Click(object sender, EventArgs e)
        {
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

        private void btnHelp_Click(object sender, EventArgs e)
        {
            frmAbout f = new frmAbout();
            f.ShowDialog();
        }

        private void btnConvert_Click(object sender, EventArgs e)
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

        private void btnFacebookLike_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(char.ConvertFromUtf32(34) +
                                (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "igsocial");
            }
            catch { }
        }

        private void btnFollow_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(char.ConvertFromUtf32(34) +
                                (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "igfollow");
            }
            catch { }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/imageglass/issues/");
        }

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

            if(GlobalSetting.IsHideToolBar)
            {
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];
            }
            else
            {
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
            }

            try
            {
                int i = GlobalSetting.ImageList.GetImage(GlobalSetting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                mnuExtractFrames.Text = string.Format(GlobalSetting.LangPack.Items["frmMain.mnuExtractFrames"], i);
                mnuExtractFrames.Enabled = true;
            }
            catch
            {
                mnuExtractFrames.Enabled = false;
            }
        }

        private void mnuPopup_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            mnuExtractFrames.Enabled = false;
        }

        private void mnuRename_Click(object sender, EventArgs e)
        {
            RenameImage();
        }

        private void mnuExitSlideshow_Click(object sender, EventArgs e)
        {
            timSlideShow.Stop();
            timSlideShow.Enabled = false;

            sp0.BackColor = GlobalSetting.BackgroundColor;
            btnFullScreen.PerformClick();

            mnuStartSlideshow.Visible = false;
            mnuStopSlideshow.Visible = false;
            mnuExitSlideshow.Visible = false;
            mnuPhanCach.Visible = false;
        }

        private void mnuCopyImagePath_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(GlobalSetting.ImageList.GetFileName(GlobalSetting.CurrentIndex));
            }
            catch { }
        }

        private void mnuStartSlideshow_Click(object sender, EventArgs e)
        {
            timSlideShow.Enabled = true;
            timSlideShow.Start();
            mnuStartSlideshow.Enabled = false;
            mnuStopSlideshow.Enabled = true;
        }

        private void mnuStopSlideshow_Click(object sender, EventArgs e)
        {
            timSlideShow.Enabled = false;
            timSlideShow.Stop();
            mnuStartSlideshow.Enabled = true;
            mnuStopSlideshow.Enabled = false;
        }

        private void mnuUploadFacebook_Click(object sender, EventArgs e)
        {
            btnFacebook_Click(btnFacebook, e);
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
                    ThumbnailBox tb = (ThumbnailBox)thumbBar.Controls[imgIndex];
                    tb.ToolTip = e.FullPath;
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
                    thumbBar.Controls.RemoveAt(imgIndex);
                }
                catch { }

                NextPic(0);
            }
        }

        private void sysWatch_Changed(object sender, FileSystemEventArgs e)
        {
            GlobalSetting.ImageList.Unload(GlobalSetting.CurrentIndex);
            NextPic(0);
        }

        private void picMain_Zoomed(object sender, ImageBoxZoomEventArgs e)
        {
            //Zoom optimization
            ZoomOptimization();

            this.UpdateStatusBar(true);
        }
        #endregion

        

    }
}