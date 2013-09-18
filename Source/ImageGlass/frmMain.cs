/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ImageGlass.Core;
using ImageGlass.ThumbBar;
using ImageGlass.Library.Image;
using ImageGlass.Library.Comparer;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing.IconLib;
using System.Drawing.IconLib.ColorProcessing;
using System.Diagnostics;
using ImageGlass.Services.Configuration;

namespace ImageGlass
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            m_AddImageDelegate = new DelegateAddImage(this.AddImage);

            m_thumbBar = new ThumbnailController();
            m_thumbBar.OnStart += new ThumbnailControllerEventHandler(m_Controller_OnStart);
            m_thumbBar.OnAdd += new ThumbnailControllerEventHandler(m_Controller_OnAdd);
            m_thumbBar.OnEnd += new ThumbnailControllerEventHandler(m_Controller_OnEnd);
        }


        #region Local variable
        private Rectangle rect = Rectangle.Empty;               // Widnow size memory (kiosk mode)
        private Rectangle[] zoom = new Rectangle[10];           // The zoom level memory
        private int scrollIdent = 0;                            // Sticky keys scrolling filter
        private const int M_THUMBNAIL_SIZE = 40;                
        
        private double offX, offY, aspect;                      // Relative position reference
        private Point loc = Point.Empty;
        private Point center = Point.Empty;
        private bool mouseMoveAvailable = true;
        private Point chrome = Point.Empty;

        public event ThumbnailImageEventHandler OnImageSizeChanged;
        private ThumbnailController m_thumbBar;
        private delegate void DelegateAddImage(string imageFilename);
        private DelegateAddImage m_AddImageDelegate;
        #endregion


        #region Drag - drop
        private void frmMain_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
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
        /// Lấy tất cả các file ảnh trong thư mục
        /// </summary>
        /// <param name="path"></param>
 
        private void Prepare(string path)
        {            
            if (File.Exists(path) == false && Directory.Exists(path) == false)
                return;

            GlobalSetting.IsZooming = false;
            GlobalSetting.CurrentIndex = 0;//dat lai index cua hinh anh
            string initFile = "";

            if (File.Exists(path))
            {
                initFile = path;
                path = path.Substring(0, path.LastIndexOf("\\") + 1);//lay duong dan thu muc                
            }
            else if (Directory.Exists(path))
            {
                path = path.Replace("\\\\", "\\");                
            }

            GlobalSetting.CurrentPath = path;//gan lam thu muc hinh anh
            GlobalSetting.ImageFilenameList = new List<string>(); //danh sach ten tap tin hinh anh
            GetFiles(GlobalSetting.ImageFilenameList, path, -1);//loc lay danh sach hinh anh co trong thu muc
            GlobalSetting.ImageList.Dispose();
            GlobalSetting.ImageList = new ImgMan(path, GlobalSetting.ImageFilenameList.ToArray());//gan ds hinh anh

            //tim index cua hinh anh
            GlobalSetting.CurrentIndex = GlobalSetting.ImageFilenameList.IndexOf(Path.GetFileName(initFile));
            if (GlobalSetting.CurrentIndex == -1)//ảnh không thể hiển thị
            {
                GlobalSetting.IsImageError = true;
                picMain.Size = GlobalSetting.ImageList.ErrorImage().Size;
                picMain.Image = GlobalSetting.ImageList.ErrorImage();
                this.Text = "ImageGlass - " + initFile;
                lblZoomRatio.Text = ImageInfo.GetFileSize(initFile);

                return;
            }

            NextPic(0);

            //show thumbnail image--------------------------------------------------------------------------
            if (GlobalSetting.IsShowThumbnail)
            {
                LoadThumnailImage(true);
            }

            sysWatch.Path = GlobalSetting.CurrentPath;
            sysWatch.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Clear and reload all thumbnail image
        /// </summary>
        /// <param name="Setting.ImageFilenameList"></param>
        private void LoadThumnailImage(bool isNext)
        {
            //lay so luong thumbnail se hien thi
            int soluong = this.Width / (M_THUMBNAIL_SIZE + 13);

            //lay 1/2 so tam anh de tim index;
            //int half = soluong / 2;

            //danh sach vi tri anh se duoc load
            List<int> dsIndex = new List<int>();

            //vi tri cua tam anh cuoi cung dc load
            //int max = Setting.CurrentIndex + half;

            int iBegin = 0;
            int iEnd = 0;

            if (isNext)
            {
                iBegin = GlobalSetting.CurrentIndex;
                iEnd = GlobalSetting.CurrentIndex + soluong;
            }
            else
            {
                iBegin = GlobalSetting.CurrentIndex - soluong;
                iEnd = GlobalSetting.CurrentIndex + 1;
            }

            if (iBegin < 0)
            {
                iBegin = 0;
                iEnd = soluong;
            }
            else if (iEnd > GlobalSetting.ImageList.length)
            {
                iBegin = GlobalSetting.ImageList.length - soluong;
                iEnd = GlobalSetting.ImageList.length;
            }

            //tim ds cac index hop le
            for (int i = iBegin; i < iEnd; i++)
            {
                if (-1 < i && i < GlobalSetting.ImageList.length) //i = [0, lenght - 1]
                {
                    dsIndex.Add(i);
                }
            }

            //clear thumbnail items
            thumbBar.Controls.Clear();

            //item list will be loaded
            List<string> files = new List<string>();

            //ve thumbnail
            for (int i = 0; i < dsIndex.Count; i++)
            {
                Application.DoEvents();
                files.Add(GlobalSetting.ImageList.getPath(dsIndex[i]));
            }

            m_thumbBar.AddFile(files.ToArray());
            System.GC.Collect();
        }


        /// <summary>
        /// Lọc ra các tập tin định dạng hình ảnh từ thư mục
        /// </summary>
        /// <param name="dsFile"></param>
        /// <param name="path"></param>
        /// <param name="len"></param>
        void GetFiles(List<string> dsFile, string path, int len)
        {
            int a = dsFile.Count;
            if (len < 0)
            {
                len = path.Length;
            }

            //Lấy thứ tự sắp xếp ảnh
            GlobalSetting.LoadImageOrderConfig();

            //Sap xem thu tu hinh anh
            if (GlobalSetting.ImageOrderBy == ImageOrderBy.Length)
            {
                dsFile.AddRange(Directory.GetFiles(path).OrderBy(f => new FileInfo(f).Length));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.LastWriteTime)
            {
                dsFile.AddRange(Directory.GetFiles(path).OrderBy(f => new FileInfo(f).LastWriteTime));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.LastAccessTime)
            {
                dsFile.AddRange(Directory.GetFiles(path).OrderBy(f => new FileInfo(f).LastAccessTime));
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.Extension)
            {
                dsFile.AddRange(Directory.GetFiles(path).OrderBy(f => new FileInfo(f).Extension));                
            }
            else if (GlobalSetting.ImageOrderBy == ImageOrderBy.Random)
            {
                dsFile.AddRange(Directory.GetFiles(path).OrderBy(f => Guid.NewGuid()));
            }
            else
            {
                dsFile.AddRange(FileLogicalComparer.Sort(Directory.GetFiles(path)));
            }


            for (; a < dsFile.Count; a++)
            {
                //Lay ext cua file
                string ext = Path.GetExtension(dsFile[a]).ToLower();

                //Lọc lấy phần name của file
                dsFile[a] = dsFile[a].Substring(len);

                // loc lai danh sach cac file co ext ho tro
                if (!GlobalSetting.SupportedExtensions.Contains(ext))
                {
                    dsFile.RemoveAt(a);
                    a--;
                }
            }

            //neu tim kiem de quy
            if (GlobalSetting.IsRecursive)
            {
                string[] sub = System.IO.Directory.GetDirectories(path);
                foreach (string e in sub)
                {
                    GetFiles(dsFile, e, len);
                }
            }
        }

        /// <summary>
        /// Change to the dir'th picture from this one
        /// </summary>
        /// <param name="step">Steps, signed</param>
        void NextPic(int step)
        {
            if (GlobalSetting.ImageList.length < 1)
            {
                this.Text = "ImageGlass";
                lblZoomRatio.Text = string.Empty;
                picMain.Image = null;
                GlobalSetting.IsImageError = true;
                return;
            }

            GlobalSetting.IsZooming = false;
            GlobalSetting.CurrentIndex += step;
            if (GlobalSetting.ImageList.length == 0) return;
            if (GlobalSetting.CurrentIndex >= GlobalSetting.ImageList.length) GlobalSetting.CurrentIndex = 0;
            if (GlobalSetting.CurrentIndex < 0) GlobalSetting.CurrentIndex = GlobalSetting.ImageList.length - 1;


            picMain.Image = null;       
            this.Text = "ImageGlass - " +
                        (GlobalSetting.CurrentIndex + 1) + "/" + GlobalSetting.ImageList.length + " " + 
                        GlobalSetting.LangPack.Items["frmMain._Text"] + " - " +
                        GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
            Application.DoEvents();
            Image im = null;


            try
            {
                GlobalSetting.IsImageError = GlobalSetting.ImageList.imgError;

                //Kiem tra neu la icon thi lay Image lon nhat
                if (Path.GetExtension(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)).Replace(".", "").ToUpper() == "ICO")
                {
                    try
                    {
                        MultiIcon mIcon = new MultiIcon();
                        mIcon.Load(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex));

                        SingleIcon sIcon = mIcon[0];//Lay icon day tien
                        IconImage iImage = sIcon.OrderByDescending(ico => ico.Size.Width).ToList()[0];

                        im = iImage.Icon.ToBitmap();
                    }
                    catch
                    {
                        im = GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex);
                        GlobalSetting.IsImageError = GlobalSetting.ImageList.imgError;
                    }
                }
                else
                {
                    im = GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex);
                    GlobalSetting.IsImageError = GlobalSetting.ImageList.imgError;
                }

                if (im.Width <= sp0.Panel1.Width && im.Height <= sp0.Panel1.Height)
                {
                    Point p = new Point();
                    p.X = sp0.Panel1.Width / 2 - im.Width / 2;
                    p.Y = sp0.Panel1.Height / 2 - im.Height / 2;

                    picMain.Image = im;
                    picMain.Bounds = new Rectangle(p, picMain.Image.Size);
                }
                else
                {
                    Recenter(im);
                    picMain.Image = im;
                }

                //Get zoom ratio               
                lblZoomRatio.Text = Math.Round(GetZoomRatio(), 2).ToString() + "X";
                lblImageSize.Text = picMain.Image.Width + " x " + picMain.Image.Height + " px";
                lblImageFileSize.Text = ImageInfo.GetFileSize(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex));
                lblImageType.Text = Path.GetExtension(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)).Replace(".", "").ToUpper();
                lblImageDateCreate.Text = File.GetCreationTime(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)).ToString();

                if (GlobalSetting.IsHideToolBar)
                {
                    this.Text += " - " + lblZoomRatio.Text;
                    this.Text += " - " + lblImageSize.Text;
                    this.Text += " - " + lblImageFileSize.Text;
                    this.Text += " - " + lblImageType.Text;
                    this.Text += " - " + lblImageDateCreate.Text;
                }

                //giai phong bo nho
                if (GlobalSetting.CurrentIndex - 1 > -1 && GlobalSetting.CurrentIndex < GlobalSetting.ImageList.length)
                {
                    GlobalSetting.ImageList.unload(GlobalSetting.CurrentIndex - 1);
                }
            }
            catch//(Exception ex)
            {
                picMain.Image = null;
                
                Application.DoEvents();
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)))
                {
                    GlobalSetting.ImageList.unload(GlobalSetting.CurrentIndex);
                }
            }

            //select thumbnail item
            if (GlobalSetting.IsShowThumbnail)
            {
                int iFrom = 0;
                int iTo = 0;

                int i = 1;
                foreach (Control c in thumbBar.Controls)
                {
                    ImageViewer ti = (ImageViewer)c;

                    if (ti.IndexImage == GlobalSetting.CurrentIndex)
                    {
                        ti.IsActive = true;
                    }
                    else
                    {
                        ti.IsActive = false;
                    }
                    i++;
                }

                //lấy giá trị mút index cua thumbnail
                if (thumbBar.Controls.Count > 0)
                {
                    ImageViewer ti = (ImageViewer)thumbBar.Controls[0];
                    iFrom = ti.IndexImage;

                    ti = (ImageViewer)thumbBar.Controls[thumbBar.Controls.Count - 1];
                    iTo = ti.IndexImage;

                    if (GlobalSetting.CurrentIndex < iFrom)
                    {
                        LoadThumnailImage(false);
                    }
                    else if (GlobalSetting.CurrentIndex > iTo)
                    {
                        LoadThumnailImage(true);
                    }
                }                
            }//end thumbnail

            if (GlobalSetting.ZoomLockValue != 1 && !GlobalSetting.IsImageError)
            {
                setZoomOrigin();
                ZoomImage(GlobalSetting.ZoomLockValue);
            }

            System.GC.Collect();
        }
        #endregion


        #region Key event

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                NextPic(-1);
            }
            else if (keyData == Keys.Right)
            {
                NextPic(1);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            //this.Text = e.KeyValue.ToString();

            //===================================\\'
            //=  Thực hiện load file bằng cách  =\\'
            //=    dán dữ liệu từ Clipboard     =\\'
            //===================================\\'
            #region Ctrl + V
            if (e.KeyCode == Keys.V && e.Control && !e.Shift && !e.Alt)//Ctrl + V
            {
               //Kiem tra co file trong clipboard khong?-------------------------------------------------------------------------
                if (Clipboard.ContainsFileDropList())
                {
                    string[] sFile = (string[])Clipboard.GetData(System.Windows.Forms.DataFormats.FileDrop);
                    int fileCount = 0;

                    fileCount = sFile.Length;
                   
                    //neu co file thi load
                    Prepare(sFile[0]);
                }


                //Kiem tra co image trong clipboard khong?------------------------------------------------------------------------
                //CheckImageInClipboard: ;
                else if (Clipboard.ContainsImage())
                {
                    picMain.Image = Clipboard.GetImage();
                }
               

                //Kiem tra co duong dan (string) trong clipboard khong?-----------------------------------------------------------
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
                    GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
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

            // Sap hinh anh voi duong bien duoi-------------------------------------------------
            if (e.KeyCode == Keys.Down)
            {
                //Sap bien duoi
                if (e.Control && !e.Shift && !e.Alt)//Ctrl + Down
                {
                    ScrollSmooth(picMain.Left, picMain.Top + sp0.Panel1.Height); 
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    ScrollSmooth(picMain.Left, picMain.Top + sp0.Panel1.Height / 2);
                }
                validateBounds();
                return;
            }

            // Sap bien tren-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Up)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    ScrollSmooth(picMain.Left, picMain.Top - sp0.Panel1.Height);
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    ScrollSmooth(picMain.Left, picMain.Top - sp0.Panel1.Height / 2);
                }
                validateBounds();
                return;
            }

            // Sap bien phai-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Right)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    ScrollSmooth(picMain.Left + sp0.Panel1.Width, picMain.Top);
                    validateBounds();
                }
                else if (e.Shift && !e.Control && !e.Alt)
                {
                    ScrollSmooth(picMain.Left + sp0.Panel1.Width / 2, picMain.Top);
                    validateBounds();
                }
                return;

            }

            // Sap bien trai-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Left)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    ScrollSmooth(picMain.Left - sp0.Panel1.Width, picMain.Top);
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    ScrollSmooth(picMain.Left - sp0.Panel1.Width / 2, picMain.Top);
                }
                validateBounds();
                return;
            }


            
        }

        #endregion 


        #region Phuong thuc
        
        /// <summary>
        /// Add thumbnail item
        /// </summary>
        /// <param name="imageFilename"></param>
        private void AddImage(string imageFilename)
        {
            // thread safe
            if (this.InvokeRequired)
            {
                this.Invoke(m_AddImageDelegate, imageFilename);
            }
            else
            {
                //lấy kích thước tối đa của thumbnail
                GlobalSetting.LoadMaxThumbnailFileSizeConfig();

                int size = M_THUMBNAIL_SIZE;
                ImageViewer imageViewer = new ImageViewer();
                imageViewer.Dock = DockStyle.Bottom;

                //chi load thumbnail voi SIZE < Setting.ThumbnailMaxFileSize MB
                if (new FileInfo(imageFilename).Length > GlobalSetting.ThumbnailMaxFileSize * 1024 * 1024)
                {
                    imageViewer.Image = ImageGlass.Properties.Resources.imagetoolarge;
                    imageViewer.ImageLocation = imageFilename;
                }
                else
                {
                    imageViewer.LoadImage(imageFilename, M_THUMBNAIL_SIZE, M_THUMBNAIL_SIZE);
                }
                
                                
                imageViewer.Width = size;
                imageViewer.Height = size;
                imageViewer.IsThumbnail = false;
                imageViewer.BackColor = Color.Transparent;
                imageViewer.IndexImage = GlobalSetting.ImageFilenameList.IndexOf(Path.GetFileName(imageFilename));

                if (imageViewer.IndexImage == GlobalSetting.CurrentIndex)
                {
                    imageViewer.IsActive = true;
                }

                tip1.SetToolTip(imageViewer, imageFilename);

                imageViewer.MouseClick += thumbBarItem_MouseClick;
                this.OnImageSizeChanged += new ThumbnailImageEventHandler(imageViewer.ImageSizeChanged);

                this.thumbBar.Controls.Add(imageViewer);
            }
        }

        private void thumbBarItem_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ImageViewer iv = (ImageViewer)sender;

                //active control
                foreach (Control c in thumbBar.Controls)
                {
                    ImageViewer ti = (ImageViewer)c;
                    ti.IsActive = false;
                }
                iv.IsActive = true;

                GlobalSetting.CurrentIndex = iv.IndexImage;
                NextPic(0);
            }
        }

        /// <summary>
        /// Rename image
        /// </summary>
        /// <param name="oldFilename"></param>
        /// <param name="newname"></param>
        private void RenameImage()
        {
            //Lay ten file
            string oldFilename; 
            string newname;
            oldFilename = newname = GlobalSetting.ImageList.getName(GlobalSetting.CurrentIndex);

            //Lay ext
            string ext = newname.Substring(newname.LastIndexOf("."));
            newname = newname.Substring(0, newname.Length - ext.Length);

            //Hien input box
            string str = null;
            if (InputBox.ShowDiaLog("Rename", GlobalSetting.LangPack.Items["frmMain._RenameDialog"], 
                                    newname) == System.Windows.Forms.DialogResult.OK)
            {
                str = InputBox.Message;
            }
            if (str == null)
            {
                return;
            }

            newname = str + ext;
            //Neu ten giong nhau thi return;
            if (oldFilename == newname)
            {
                return;
            }

            try
            {
                //Doi ten tap tin
                ImageInfo.RenameFile(GlobalSetting.CurrentPath + oldFilename,
                                    GlobalSetting.CurrentPath + newname);

                //Cập nhật lại trạng thái tiêu đề
                this.Text = "ImageGlass - " +
                            (GlobalSetting.CurrentIndex + 1) + "/" + GlobalSetting.ImageList.length + " " +
                            GlobalSetting.LangPack.Items["frmMain._Text"] + " - " +
                            GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }


        /// <summary>
        /// Set zoom pivot by reading viewport location
        /// </summary>
        private void setZoomOrigin()
        {
            setZoomOrigin(getCursorPositionRelativeToWindowPositionThroughTheMathematicalImplementation());
        }


        /// <summary>
        /// Set zoom pivot based on mouseEvent
        /// </summary>
        /// <param name="pt">The clicked absolute coordinate of dsp</param>
        private void setZoomOrigin(Point pt)
        {
            loc = getCursorPositionRelativeToWindowPosition();
            center = loc;
            offX = pt.X / (picMain.Width * 1.0);
            offY = pt.Y / (picMain.Height * 1.0);
            try
            {
                aspect = picMain.Image.Width / (1.0 * picMain.Image.Height);
            }
            catch { }
        }

        /// <summary>
        /// Check whether existing viewport is within allowed bounds
        /// Please don't use this method unless you want flickering
        /// </summary>
        /// <returns>The closest valid viewport based on reg</returns>
        private Point validateBounds()
        {
            return validateBounds(picMain.Bounds, true);
        }

        /// <summary>
        /// Validate and (optionally) apply new viewport
        /// </summary>
        /// <param name="reg">The new viewport</param>
        /// <param name="exec">Apply viewport</param>
        /// <returns>The closest valid viewport based on reg</returns>
        private Point validateBounds(Rectangle reg, bool exec)
        {
            int x = reg.Left, y = reg.Top;
            if (GlobalSetting.IsLockWorkspaceEdges)
            {

                // Validate width
                if (reg.Width >= sp0.Panel1.Width)
                {
                    // Larger than screen
                    if (x > 0) x = 0;
                    if (sp0.Panel1.Width > reg.Width + x)
                        x = -(reg.Width - sp0.Panel1.Width);
                }
                else
                {
                    // Smaller than screen
                    if (x < 0) x = 0;
                    if (sp0.Panel1.Width < reg.Width + x)
                        x = sp0.Panel1.Width - reg.Width;
                }

                // Validate height
                if (reg.Height >= sp0.Panel1.Height)
                {
                    // Larger than screen
                    if (y > 0) y = 0;
                    if (sp0.Panel1.Height > reg.Height + y)
                        y = -(reg.Height - sp0.Panel1.Height);
                }
                else
                {
                    // Smaller than screen
                    if (y < 0) y = 0;
                    if (sp0.Panel1.Height < reg.Height + y)
                        y = sp0.Panel1.Height - reg.Height;
                }
            }
            //bool ret = (x == reg.Left && y == reg.Top);
            if (exec) picMain.Location = new Point(x, y);
            return new Point(x, y);
        }        

        /// <summary>
        /// Handles mouseMove event (pan/zoom)
        /// </summary>
        /// <param name="sender">Event origin (control)</param>
        /// <param name="e">Event parameters</param>
        private void picMain_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                //change cursor
                if (int.Parse(picMain.Tag.ToString()) == 1)
                {
                    Bitmap img = ImageGlass.Properties.Resources.Hand_Down;
                    Cursor c = new Cursor(img.GetHicon());
                    picMain.Cursor = c;
                }
                else
                {
                    Bitmap img = ImageGlass.Properties.Resources.Hand_Move;
                    Cursor c = new Cursor(img.GetHicon());
                    picMain.Cursor = c;
                }


                if (!mouseMoveAvailable) return;
                mouseMoveAvailable = false;

                if (e.Button == MouseButtons.Right && !GlobalSetting.IsImageError)
                {

                    // ZOOOOOOOOM
                    Point pos = loc;
                    loc = getCursorPositionRelativeToWindowPosition();

                    // Just to make things less complicated
                    int diffX = loc.X - pos.X;
                    int diffY = loc.Y - pos.Y;

                    // Pythagoras to find length
                    double diff = Math.Sqrt(
                        Math.Pow(1.0 * diffX, 2) +
                        Math.Pow(1.0 * diffY, 2));

                    // Retain most weighted sign
                    if (Math.Abs(diffX) > Math.Abs(diffY))
                        diff *= Math.Sign(diffX);
                    else diff *= Math.Sign(diffY);

                    ZoomImage(diff);

                }
                else if (e.Button == MouseButtons.Left)
                {

                    // Pan
                    Point pos = loc; loc = getCursorPositionRelativeToWindowPosition(); //Cursor.Position;

                    // Directly set location
                    //dsp.Left += Cursor.Position.X - pos.X;
                    //dsp.Top += Cursor.Position.Y - pos.Y;

                    // Validate not out of bounds
                    validateBounds(new Rectangle(
                        picMain.Left + (loc.X - pos.X),
                        picMain.Top + (loc.Y - pos.Y),
                        picMain.Width, picMain.Height), true);
                }
                mouseMoveAvailable = true;
            }
            catch { }
        }

        /// <summary>
        /// Feeling real ENTERPRISE
        /// </summary>
        /// <returns>The Cursor Position Relative To Window Position (Thus Not The Absolute Position Of The Cursor)</returns>
        Point getCursorPositionRelativeToWindowPosition()
        {
            Point ret = Cursor.Position;
            ret.X -= this.Left - chrome.X;
            ret.Y -= this.Top - chrome.Y;
            return ret;
        }

        /// <summary>
        /// Alternative vershun
        /// </summary>
        /// <returns>Mathematical variation of gCPRTWP; more suitable for zoomfix</returns>
        Point getCursorPositionRelativeToWindowPositionThroughTheMathematicalImplementation()
        {
            return new Point(
                Cursor.Position.X - picMain.Left - this.Left,
                Cursor.Position.Y - picMain.Top - this.Top);
        }

        /// <summary>
        /// Zooms the image lambda pizels
        /// </summary>
        /// <param name="lambda">Amount of pixels to append</param>
        void ZoomImage(double lambda)
        {
            GlobalSetting.IsZooming = true;

            // I'm too tired to figure out why this is
            // necessary right now, I'm just happy it works
            if (lambda < 0)
            {
                lambda /= 2.5;
            }
            else
            {
                lambda /= 1.5;
            }
                      

            // Slightly higher performance
            double w = picMain.Width, h = picMain.Height;

            // Calculate new size and offset
            double newW = Math.Max(25, w + w * 0.005 * lambda);
            double newH = newW / aspect;
            double newOffX = -(newW * offX) + center.X;
            double newOffY = -(newH * offY) + center.Y;

            // Apply the change
            if (newW * newH < 104857600)
            { //10240^2

                picMain.Width = (int)newW;
                picMain.Height = (int)newH;

                // Old pivot method; centered on screen
                picMain.Left -= (int)(picMain.Width - w) / 2;
                picMain.Top -= (int)(picMain.Height - h) / 2;

                // New pivot method; weighted on original rightclick coordinate
                //if ((int)newOffY < 0)
                //    picMain.Top = 0;
                //else
                //    picMain.Top = (int)newOffY;

                //if ((int)newOffX < 0)
                //    picMain.Left = 0;
                //else
                //    picMain.Left = (int)newOffX;


                //Get zoom ratio               
                lblZoomRatio.Text = Math.Round(GetZoomRatio(), 2).ToString() + "X";
                if (GlobalSetting.IsHideToolBar)
                {
                    this.Text = "ImageGlass - " +
                        (GlobalSetting.CurrentIndex + 1) + "/" + GlobalSetting.ImageList.length + " " + 
                        GlobalSetting.LangPack.Items["frmMain._Text"] + " - " +
                        GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
                    this.Text += " - " + lblZoomRatio.Text;
                    this.Text += " - " + lblImageSize.Text;
                    this.Text += " - " + lblImageFileSize.Text;
                    this.Text += " - " + lblImageType.Text;
                    this.Text += " - " + lblImageDateCreate.Text;
                }
                
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Get zoom ratio value
        /// </summary>
        /// <returns></returns>
        private double GetZoomRatio()
        {
            //Get zoom ratio
            double x = 0;
            if (picMain.Width < picMain.Image.Width)
            {
                x = -picMain.Image.Width * 1.0 / picMain.Width;
            }
            else
            {
                x = picMain.Width * 1.0 / picMain.Image.Width;
            }

            return x;
        }

        /// <summary>
        /// Set value of zoom lock
        /// </summary>
        private void SetZoomLockValue()
        {
            if (btnZoomLock.Checked && picMain.Image != null)
            {
                double x = 0;

                if (picMain.Width < picMain.Image.Width)
                {
                    x = -picMain.Image.Width * 1.0 / picMain.Width;
                }
                else
                {
                    x = picMain.Width * 1.0 / picMain.Image.Width;
                }

                GlobalSetting.ZoomLockValue = x * 140;
            }
            else
            {
                GlobalSetting.ZoomLockValue = 1;
            }
        }

        /// <summary>
        /// Smoothly scroll the image to given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ScrollSmooth(int x, int y)
        {

            // Filter repetitious scrolls
            scrollIdent++;
            int myIdent = scrollIdent;

            // DERP
            if (!GlobalSetting.IsSmoothPanning) ScrollInstant(x, y);

            // Make sure we're within bounds
            Point xy = validateBounds(new Rectangle(x, y, picMain.Width, picMain.Height), false);
            x = xy.X; y = xy.Y;         // expand validated position

            int xa = picMain.Left;          // read current ("from") pos
            int ya = picMain.Top;
            int xd = x - xa;            // difference between Setting.CurrentIndex and target
            int yd = y - ya;
            int xs = Math.Sign(xd);     // get direction
            int ys = Math.Sign(yd);
            xd *= xs;                   // filter direction from distance
            yd *= ys;
            Size sz = picMain.Size;
            int[] stepsize = { 200, 100, 50, 25, 12, 6, 3, 2, 1 };
            while (xd > 1 || yd > 1)
            {
                bool xb = false, yb = false;            // stepped this axis yet?
                foreach (int step in stepsize)
                {        // try all stepsizes
                    Point newpos = new Point(xa, ya);   // original position
                    if (!xb && step < xd)
                    {             // can & should step X axis?
                        xd -= step;                     // subtract steps from dist
                        xa += step * (1 * xs);          // add step to position
                        xb = true;                      // stepped X axis
                    }
                    if (!yb && step < yd)
                    {
                        yd -= step;
                        ya += step * (1 * ys);
                        yb = true;
                    }
                    /*if (step < dist) {
                        dist -= step;
                        from += step * (-1 * sign);
                        if (!validateBounds(new Rectangle(dsp.Left, from, dsp.Width, dsp.Height))) dist = 0;
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(50);
                        break;
                    }*/
                }
                if (myIdent != scrollIdent) break;
                validateBounds(new Rectangle(xa, ya, sz.Width, sz.Height), true);
                Application.DoEvents();
                System.Threading.Thread.Sleep(20);
            }
        }

        void ScrollInstant(int x, int y)
        {
            validateBounds(new Rectangle(x, y, picMain.Width, picMain.Height), true);
        }

        /// <summary>
        /// Apply and center a best-fit viewport
        /// </summary>
        void Recenter()
        {
            if (picMain.Image != null) Recenter(picMain.Image);
        }

        /// <summary>
        /// Apply and center a best-fit viewport based on supplied bitmap
        /// </summary>
        /// <param name="bm">Image to base calculations on</param>
        void Recenter(Image bm)
        {            
            int durrX, durrY;
            double derpX = bm.Width / (1.0 * sp0.Panel1.Width);
            double derpY = bm.Height / (1.0 * sp0.Panel1.Height);

            if (derpX > derpY)
            {
                durrX = sp0.Panel1.Width;
                durrY = (int)(bm.Height / derpX);
                picMain.Location = new Point(0, (sp0.Panel1.Height - durrY) / 2);
            }
            else
            {
                durrY = sp0.Panel1.Height;
                durrX = (int)(bm.Width / derpY);
                picMain.Location = new Point((sp0.Panel1.Width - durrX) / 2, 0);
            }

            picMain.Size = new Size(durrX, durrY);
                        
        }
        #endregion


        #region Configuration
        /// <summary>
        /// Áp dụng theme mặc định
        /// </summary>
        private void LoadThemeDefault()
        {
            // <main>
            toolMain.BackgroundImage = ImageGlass.Properties.Resources.topbar;
            sp0.Panel2.BackgroundImage = ImageGlass.Properties.Resources.bottombar;
            lblZoomRatio.ForeColor = Color.Black;
            lblImageDateCreate.ForeColor = Color.Black;
            lblImageFileSize.ForeColor = Color.Black;
            lblImageSize.ForeColor = Color.Black;
            lblImageType.ForeColor = Color.Black;

            sp0.BackColor = Color.White;

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
            btnCaro.Image = ImageGlass.Properties.Resources.caro1;
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
        /// Thay đổi theme
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
                    lblZoomRatio.ForeColor = t.statuscolor;
                    lblImageDateCreate.ForeColor = t.statuscolor;
                    lblImageFileSize.ForeColor = t.statuscolor;
                    lblImageSize.ForeColor = t.statuscolor;
                    lblImageType.ForeColor = t.statuscolor;
                }
                catch
                {
                    lblZoomRatio.ForeColor = Color.White;
                    lblImageDateCreate.ForeColor = Color.White;
                    lblImageFileSize.ForeColor = Color.White;
                    lblImageSize.ForeColor = Color.White;
                    lblImageType.ForeColor = Color.White;
                }


                try
                {
                    sp0.BackColor = t.backcolor;
                    GlobalSetting.BackgroundColor = t.backcolor;
                }
                catch
                {
                    sp0.BackColor = Color.White;
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
        /// Tải dữ liệu từ file Config
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

            //Windows Bound (Position + Size)------------------------------------------------                     "280,125,750,545").ToString());
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
            if (!(0 < i && i < 61)) i = 5;//gioi han thoi gian [1; 60] giay
            timSlideShow.Interval = 1000 * i;

            //Show Caro--------------------------------------------------------------------
            if (bool.Parse(GlobalSetting.GetConfig("Caro", "False").ToString()))
            {
                btnCaro.PerformClick();
            }

            //Khoá khung ảnh---------------------------------------------------------------
            GlobalSetting.IsLockWorkspaceEdges = bool.Parse(GlobalSetting.GetConfig("LockToEdge", "True"));
           
            //Tìm ảnh đệ quy----------------------------------------------------------------
            GlobalSetting.IsRecursive = bool.Parse(GlobalSetting.GetConfig("Recursive", "False"));
            
            //Lăn chuột mượt----------------------------------------------------------------
            GlobalSetting.IsSmoothPanning = bool.Parse(GlobalSetting.GetConfig("SmoothPanning", "False"));

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
            
            //Load ảnh mặc định------------------------------------------------------------
            string y = GlobalSetting.GetConfig("Welcome", "true");
            if (y.ToLower() == "true")
            {
                Prepare((Application.StartupPath + "\\").Replace("\\\\", "\\") + "default.png");
            }

            //Kích thước tối đa của file ảnh thu nhỏ----------------------------------------
            GlobalSetting.LoadMaxThumbnailFileSizeConfig();

            //Thứ tự sắp xếp hình ảnh------------------------------------------------------
            GlobalSetting.LoadImageOrderConfig();

            //Load theme--------------------------------------------------------------------
            LoadTheme();
            
            //Load background---------------------------------------------------------------
            z = GlobalSetting.GetConfig("BackgroundColor", "-1");
            GlobalSetting.BackgroundColor = Color.FromArgb(int.Parse(z));
            sp0.BackColor = GlobalSetting.BackgroundColor;

            //Load state of Toolbar---------------------------------------------------------
            if (bool.Parse(GlobalSetting.GetConfig("IsHideToolbar", "false")))//Dang An
            {
                //An
                spMain.Panel1Collapsed = true;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];
                GlobalSetting.IsHideToolBar = true;
            }
            else//Dang Hien
            {
                //Hien
                spMain.Panel1Collapsed = false;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
                GlobalSetting.IsHideToolBar = false;
            }
        }


        /// <summary>
        /// Lưu dữ liệu xuống file Config
        /// </summary>
        void SaveConfig()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                GlobalSetting.SetConfig("WindowsBound", GlobalSetting.RectToString(rect == Rectangle.Empty ?
                                                                    this.Bounds : rect));
            }            

            //Windows State-------------------------------------------------------------------
            GlobalSetting.SetConfig("WindowsState", this.WindowState.ToString());
            
            //Caro Style
            GlobalSetting.SetConfig("Caro", btnCaro.Checked.ToString());
            
            //Lan chuot muot------------------------------------------------------------------
            GlobalSetting.SetConfig("SmoothPanning", GlobalSetting.IsSmoothPanning.ToString());
        }

        #endregion


        #region Form event
        private void frmMain_Load(object sender, EventArgs e)
        {
            Application.DoEvents();
            LoadConfig();

            //Load image from param
            if (Program.args.Length > 0)
            {
                if (File.Exists(Program.args[0]))
                {
                    FileInfo f = new FileInfo(Program.args[0]);
                    Prepare(f.FullName);
                }
                else if (Directory.Exists(Program.args[0]))
                {
                    DirectoryInfo d = new DirectoryInfo(Program.args[0]);
                    Prepare(d.FullName);
                }
            }

            // Apply zooming with scroll wheel
            this.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
            sp0.SplitterDistance = sp0.Height - 71;

            System.GC.Collect();
        }

        /// <summary>
        /// Removes empty folders on exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {
            if (GlobalSetting.IsForcedActive)
            {
                sp0.BackColor = GlobalSetting.BackgroundColor;

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

        private void m_Controller_OnStart(object sender, ThumbnailControllerEventArgs e)
        {

        }

        private void m_Controller_OnAdd(object sender, ThumbnailControllerEventArgs e)
        {
            this.AddImage(e.ImageFilename);
            this.Invalidate();
        }

        private void m_Controller_OnEnd(object sender, ThumbnailControllerEventArgs e)
        {
            // thread safe
            if (this.InvokeRequired)
            {
                this.Invoke(new ThumbnailControllerEventHandler(m_Controller_OnEnd), sender, e);
            }
        }

        /// <summary>
        /// I wonder what this does
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            // Set chrome (non-userspace) padding
            chrome = new Point(
                (e.X + picMain.Left) - (Cursor.Position.X - this.Left),
                (e.Y + picMain.Top) - (Cursor.Position.Y - this.Top));

            setZoomOrigin(e.Location);
            picMain.Tag = 1;

            Bitmap img = ImageGlass.Properties.Resources.Hand_Down;
            Cursor c = new Cursor(img.GetHicon());
            picMain.Cursor = c;
        }

        /// <summary>
        /// Hurr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Resize(object sender, EventArgs e)
        {
            try
            {
                if (GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Width <= sp0.Panel1.Width && GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Height <= sp0.Panel1.Height)
                {
                    Point p = new Point();
                    p.X = sp0.Panel1.Width / 2 - GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Width / 2;
                    p.Y = sp0.Panel1.Height / 2 - GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Height / 2;

                    picMain.Image = GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex);
                    picMain.Bounds = new Rectangle(p, picMain.Image.Size);
                }
                else
                {
                    Recenter(); //NO DOCKING ALLOWED
                }
            }
            catch { }

        }

        /// <summary>
        /// Zoom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void picMain_MouseWheel(object sender, MouseEventArgs e)
        {
            if (GlobalSetting.IsImageError)
            {
                return;
            }
            setZoomOrigin();
            ZoomImage(e.Delta);

            //Set value of zoom lock
            SetZoomLockValue();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            NextPic(1);//xem anh ke tiep
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            NextPic(-1);//xem anh truoc do
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }
            Prepare(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex));//cap nhat lai thu muc anh
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }

            GlobalSetting.ImageList.filter.incRotate(1);
            NextPic(0);
        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }

            GlobalSetting.ImageList.filter.incRotate(-1);
            NextPic(0);
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
                    LoadThumnailImage(true);
                }
            }
        }

        private void btnActualSize_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }

            Point l = new Point();
            GlobalSetting.IsZooming = false;

            l.X = sp0.Panel1.Width / 2 - picMain.Width / 2;
            l.Y = sp0.Panel1.Height / 2 - picMain.Width / 2;

            picMain.Bounds = new Rectangle(l, picMain.Image.Size);

        }

        private void btnScaletoWidth_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }
            // Scale to Width
            double frac = sp0.Panel1.Width / (1.0 * picMain.Image.Width);
            int height = (int)(picMain.Image.Height * frac);
            picMain.Bounds = new Rectangle(Point.Empty, new Size(sp0.Panel1.Width, height));
        }

        private void btnScaletoHeight_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }
            // Scale to Height
            double frac = sp0.Panel1.Height / (1.0 * picMain.Image.Height);
            int width = (int)(picMain.Image.Width * frac);
            picMain.Bounds = new Rectangle(Point.Empty, new Size(width, sp0.Panel1.Height));
        }

        private void btnWindowAutosize_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
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

                if (-1 < n && n < GlobalSetting.ImageList.length)
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
                picMain.BackgroundImage = ImageGlass.Properties.Resources.caro;
            }
            else
            {
                picMain.BackgroundImage = null;
            }
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            if (!GlobalSetting.IsImageError)
            {
                setZoomOrigin();
                ZoomImage(14);

                //Set value of zoom lock
                SetZoomLockValue();
            }
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (!GlobalSetting.IsImageError)
            {
                setZoomOrigin();
                ZoomImage(-14);

                //Set value of zoom lock
                SetZoomLockValue();
            }
        }

        private void btnZoomLock_Click(object sender, EventArgs e)
        {
            if (btnZoomLock.Checked && picMain.Image != null)
            {
                SetZoomLockValue();
            }
            else
            {
                GlobalSetting.ZoomLockValue = 1;
                btnZoomLock.Checked = false;
            }
        }

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            NextPic(1);
        }

        private void btnSlideShow_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1)
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

                //An
                spMain.Panel1Collapsed = true;
                mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Show"];
            }
            //exit full screen
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                Application.DoEvents();
                this.Bounds = this.rect;
                this.rect = Rectangle.Empty;

                //Hien
                if (!GlobalSetting.IsHideToolBar)
                {
                    spMain.Panel1Collapsed = false;
                    mnuShowToolBar.Text = GlobalSetting.LangPack.Items["frmMain.mnuShowToolBar._Hide"];
                }
            }
        }

        private void btnPrintImage_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsImageError)
            {
                return;
            }

            Process p = new Process();
            p.StartInfo.FileName = GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
            p.StartInfo.Verb = "print";
            p.Start();
        }

        private void btnFacebook_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length > 0 && File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)))
            {
                if (LocalSetting.FFacebook.IsDisposed)
                {
                    LocalSetting.FFacebook = new frmFacebook();
                }

                LocalSetting.FFacebook.Filename = GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
                GlobalSetting.IsForcedActive = false;
                LocalSetting.FFacebook.Show();
                LocalSetting.FFacebook.Activate();
            }
        }

        private void picMain_MouseUp(object sender, MouseEventArgs e)
        {
            picMain.Tag = 0;
            Bitmap img = ImageGlass.Properties.Resources.Hand_Move;
            Cursor c = new Cursor(img.GetHicon());
            picMain.Cursor = c;
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
                System.Diagnostics.Process.Start("mspaint.exe", char.ConvertFromUtf32(34) + GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex) + char.ConvertFromUtf32(34));
            }
        }

        private void mnuProperties_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length > 0)
            {
                ImageInfo.DisplayFileProperties(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex),
                                                this.Handle);
            }
        }

        private void mnuImageLocation_Click(object sender, EventArgs e)
        {
            if (GlobalSetting.ImageList.length > 0)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + 
                    GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex) + "\"");
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show(
                                string.Format(GlobalSetting.LangPack.Items["frmMain._DeleteDialogText"], 
                                            GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)),
                                GlobalSetting.LangPack.Items["frmMain._DeleteDialogTitle"], 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg == DialogResult.Yes)
            {
                string f = GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
                try
                {
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
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show(
                                string.Format(GlobalSetting.LangPack.Items["frmMain._RecycleBinDialogText"], 
                                                GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)),
                                GlobalSetting.LangPack.Items["frmMain._RecycleBinDialogTitle"], 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg == DialogResult.Yes)
            {
                string f = GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);
                try
                {
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
                                        "\"" + GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex) + "\" " + //arg 1
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
                    ani.ExtractAllFrames(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex), 
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
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)) || GlobalSetting.IsImageError)
                {
                    return;
                }
            }
            catch { return; }

            Library.Image.ImageInfo.ConvertImage(Image.FromFile(GlobalSetting.CurrentPath + 
                                    GlobalSetting.ImageList.getName(GlobalSetting.CurrentIndex)), 
                                    GlobalSetting.ImageList.getName(GlobalSetting.CurrentIndex));
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

        private void picMain_Paint(object sender, PaintEventArgs e)
        {
            if (GlobalSetting.ImageList.length < 1 || GlobalSetting.IsZooming == false) return;

            Graphics g = e.Graphics;
            
            if(GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationValue.SmoothPixels)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            }
            else if (GlobalSetting.ZoomOptimizationMethod == ZoomOptimizationValue.ClearPixels)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
            else //auto
            {
                if (GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Width * GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).Height > picMain.Width * picMain.Height)
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                else
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }

            g.DrawImage(GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex), 0, 0, picMain.Width, picMain.Height);
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void mnuPopup_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                if (!File.Exists(GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex)) || 
                                 GlobalSetting.IsImageError)
                {
                    e.Cancel = true;
                    return;
                }
            }
            catch { e.Cancel = true; return; }

            try
            {
                int i = GlobalSetting.ImageList.get(GlobalSetting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
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
                Clipboard.SetText(GlobalSetting.CurrentPath + GlobalSetting.ImageList.getName(GlobalSetting.CurrentIndex));
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

        private void mnuUploadFacebook_Click(object sender, EventArgs e)
        {
            btnFacebook_Click(btnFacebook, e);
        }

        private void mnuStopSlideshow_Click(object sender, EventArgs e)
        {
            timSlideShow.Enabled = false;
            timSlideShow.Stop();
            mnuStartSlideshow.Enabled = true;
            mnuStopSlideshow.Enabled = false;
        }
        
        private void sp0_Panel1_MouseEnter(object sender, EventArgs e)
        {
            if (GlobalSetting.IsForcedActive)
            {
                picMain.Focus();
            }
        }

        private void picMain_MouseEnter(object sender, EventArgs e)
        {
            if (GlobalSetting.IsForcedActive)
            {
                picMain.Focus();
            }
        }

        private void sysWatch_Changed(object sender, FileSystemEventArgs e)
        {
            
        }

        //Neu file bi thay doi ten
        private void sysWatch_Renamed(object sender, RenamedEventArgs e)
        {
            string newName = e.Name;
            string oldName = e.OldName;

            //Get index of renamed image
            int imgIndex = GlobalSetting.ImageFilenameList.IndexOf(oldName);
            if (imgIndex > -1)
            {
                //Rename image list
                GlobalSetting.ImageList.setName(imgIndex, newName);
                GlobalSetting.ImageFilenameList[imgIndex] = newName;

                //Cap nhat lai tieu de
                this.Text = "ImageGlass - " +
                        (GlobalSetting.CurrentIndex + 1) + "/" + GlobalSetting.ImageList.length + " " +
                        GlobalSetting.LangPack.Items["frmMain._Text"] + " - " +
                        GlobalSetting.ImageList.getPath(GlobalSetting.CurrentIndex);

                try
                {
                    //Rename image in thumbnail bar
                    ImageViewer ti = (ImageViewer)thumbBar.Controls[imgIndex];
                    ti.ImageLocation = e.FullPath;
                    tip1.SetToolTip(ti, e.FullPath);
                }
                catch { }
            }
        }

        private void sysWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            //Get index of deleted image
            int imgIndex = GlobalSetting.ImageFilenameList.IndexOf(e.Name);

            if (imgIndex > -1)
            {
                //delete image list
                GlobalSetting.ImageList.remove(imgIndex);
                GlobalSetting.ImageFilenameList.RemoveAt(imgIndex);

                try
                {
                    //delete thumbnail list
                    thumbBar.Controls.RemoveAt(imgIndex);

                    //update index of thumbnail
                    for (int i = imgIndex; i < thumbBar.Controls.Count; i++)
                    {
                        ImageViewer ti = (ImageViewer)thumbBar.Controls[i];
                        ti.IndexImage = i;
                    }
                }
                catch { }

                NextPic(0);
            }
        }

        #endregion




    }
}