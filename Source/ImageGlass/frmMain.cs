/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2012 DUONG DIEU PHAP
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
using ImageGlass.Feature;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using ImageGlass.ThumbBar;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing.IconLib;
using System.Drawing.IconLib.ColorProcessing;

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
            o.Filter = "All supported files|" + Setting.SupportedExtensions;

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

            Setting.IsZooming = false;
            Setting.CurrentIndex = 0;//dat lai index cua hinh anh
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

            Setting.CurrentPath = path;//gan lam thu muc hinh anh
            Setting.ImageFilenameList = new List<string>(); //danh sach ten tap tin hinh anh
            GetFiles(Setting.ImageFilenameList, path, -1);//loc lay danh sach hinh anh co trong thu muc
            Setting.ImageList.Dispose();
            Setting.ImageList = new ImgMan(path, Setting.ImageFilenameList.ToArray());//gan ds hinh anh

            //tim index cua hinh anh
            Setting.CurrentIndex = Setting.ImageFilenameList.IndexOf(Path.GetFileName(initFile));
            if (Setting.CurrentIndex == -1)//ảnh không thể hiển thị
            {
                Setting.IsImageError = true;
                picMain.Size = Setting.ImageList.ErrorImage().Size;
                picMain.Image = Setting.ImageList.ErrorImage();
                this.Text = "ImageGlass - " + initFile;
                lblZoomRatio.Text = ImageGlass_ImageInfo.GetFileSize(initFile);

                return;
            }

            NextPic(0);

            //show thumbnail image--------------------------------------------------------------------------
            if (Setting.IsShowThumbnail)
            {
                LoadThumnailImage(true);
            }

            sysWatch.Path = Setting.CurrentPath;
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
                iBegin = Setting.CurrentIndex;
                iEnd = Setting.CurrentIndex + soluong;
            }
            else
            {
                iBegin = Setting.CurrentIndex - soluong;
                iEnd = Setting.CurrentIndex + 1;
            }

            if (iBegin < 0)
            {
                iBegin = 0;
                iEnd = soluong;
            }
            else if (iEnd > Setting.ImageList.length)
            {
                iBegin = Setting.ImageList.length - soluong;
                iEnd = Setting.ImageList.length;
            }

            //tim ds cac index hop le
            for (int i = iBegin; i < iEnd; i++)
            {
                if (-1 < i && i < Setting.ImageList.length) //i = [0, lenght - 1]
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
                files.Add(Setting.ImageList.getPath(dsIndex[i]));
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
            LoadImageOrderConfig();

            //Sap xem thu tu hinh anh
            if (Setting.ImageOrderBy == ImageOrderBy.Length)
            {
                dsFile.AddRange(System.IO.Directory.GetFiles(path).OrderBy(f => new FileInfo(f).Length));
            }
            else if (Setting.ImageOrderBy == ImageOrderBy.LastWriteTime)
            {
                dsFile.AddRange(System.IO.Directory.GetFiles(path).OrderBy(f => new FileInfo(f).LastWriteTime));
            }
            else if (Setting.ImageOrderBy == ImageOrderBy.LastAccessTime)
            {
                dsFile.AddRange(System.IO.Directory.GetFiles(path).OrderBy(f => new FileInfo(f).LastAccessTime));
            }
            else if (Setting.ImageOrderBy == ImageOrderBy.Extension)
            {
                dsFile.AddRange(System.IO.Directory.GetFiles(path).OrderBy(f => new FileInfo(f).Extension));
            }
            else 
            {
                dsFile.AddRange(System.IO.Directory.GetFiles(path).OrderBy(f => new FileInfo(f).Name));
            }


            for (; a < dsFile.Count; a++)
            {
                //Lay ext cua file
                string ext = Path.GetExtension(dsFile[a]).ToLower();

                //Lọc lấy phần name của file
                dsFile[a] = dsFile[a].Substring(len);

                // loc lai danh sach cac file co ext ho tro
                if (!Setting.SupportedExtensions.Contains(ext))
                {
                    dsFile.RemoveAt(a);
                    a--;
                }
            }

            //neu tim kiem de quy
            if (Setting.IsRecursive)
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
        /// <param name="dir">Steps, signed</param>
        void NextPic(int dir)
        {
            if (Setting.ImageList.length < 1)
            {
                this.Text = "ImageGlass";
                lblZoomRatio.Text = string.Empty;
                return;
            }

            Setting.IsZooming = false;
            Setting.CurrentIndex += dir;
            if (Setting.ImageList.length == 0) return;
            if (Setting.CurrentIndex >= Setting.ImageList.length) Setting.CurrentIndex = 0;
            if (Setting.CurrentIndex < 0) Setting.CurrentIndex = Setting.ImageList.length - 1;


            picMain.Image = null;       
            this.Text = "ImageGlass - " +
                        (Setting.CurrentIndex + 1) + "/" + Setting.ImageList.length + " file(s) - " +
                        Setting.ImageList.getPath(Setting.CurrentIndex);
            Application.DoEvents();
            Image im = null;


            try
            {
                Setting.IsImageError = Setting.ImageList.imgError;

                //Kiem tra neu la icon thi lay Image lon nhat
                if (Path.GetExtension(Setting.ImageList.getPath(Setting.CurrentIndex)).Replace(".", "").ToUpper() == "ICO")
                {
                    try
                    {
                        MultiIcon mIcon = new MultiIcon();
                        mIcon.Load(Setting.ImageList.getPath(Setting.CurrentIndex));

                        SingleIcon sIcon = mIcon[0];//Lay icon day tien
                        IconImage iImage = sIcon.OrderByDescending(ico => ico.Size.Width).ToList()[0];

                        im = iImage.Icon.ToBitmap();
                    }
                    catch
                    {
                        im = Setting.ImageList.get(Setting.CurrentIndex);
                        Setting.IsImageError = Setting.ImageList.imgError;
                    }
                }
                else
                {
                    im = Setting.ImageList.get(Setting.CurrentIndex);
                    Setting.IsImageError = Setting.ImageList.imgError;
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
                lblImageFileSize.Text = ImageGlass_ImageInfo.GetFileSize(Setting.ImageList.getPath(Setting.CurrentIndex));
                lblImageType.Text = Path.GetExtension(Setting.ImageList.getPath(Setting.CurrentIndex)).Replace(".", "").ToUpper();
                lblImageDateCreate.Text = File.GetCreationTime(Setting.ImageList.getPath(Setting.CurrentIndex)).ToString();

                //giai phong bo nho
                if (Setting.CurrentIndex - 1 > -1 && Setting.CurrentIndex < Setting.ImageList.length)
                {
                    Setting.ImageList.unload(Setting.CurrentIndex - 1);
                }
            }
            catch//(Exception ex)
            {
                picMain.Image = null;
                
                Application.DoEvents();
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
                {
                    Setting.ImageList.unload(Setting.CurrentIndex);
                }
            }

            //select thumbnail item
            if (Setting.IsShowThumbnail)
            {
                int iFrom = 0;
                int iTo = 0;

                int i = 1;
                foreach (Control c in thumbBar.Controls)
                {
                    ImageViewer ti = (ImageViewer)c;

                    if (ti.IndexImage == Setting.CurrentIndex)
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

                    if (Setting.CurrentIndex < iFrom)
                    {
                        LoadThumnailImage(false);
                    }
                    else if (Setting.CurrentIndex > iTo)
                    {
                        LoadThumnailImage(true);
                    }
                }                
            }//end thumbnail

            if (Setting.ZoomLockValue != 1 && !Setting.IsImageError)
            {
                setZoomOrigin();
                zoomImage(Setting.ZoomLockValue);
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
 


            // Rotation Counterclockwise----------------------------------------------------
            if (e.KeyValue == 188 && e.Control && !e.Shift && !e.Alt)//Ctrl + ,
            {
                btnRotateLeft_Click(null, null);
                return;
            }
            //Rotate Clockwise--------------------------------------------------------------
            if (e.KeyValue == 190 && e.Control && !e.Shift && !e.Alt)//Ctrl + .
            {
                btnRotateRight_Click(null, null);
                return;
            }
           
            //Play slideshow----------------------------------------------------------------
            if (e.KeyValue == 122 && !e.Control && !e.Shift && !e.Alt)//F11
            {
                btnSlideShow_Click(null, null);
                return;
            }

            //Exit slideshow----------------------------------------------------------------
            if (e.KeyValue == 27 && !e.Control && !e.Shift && !e.Alt)//ESC
            {
                if (Setting.IsPlaySlideShow)
                {
                    mnuExitSlideshow_Click(null, null);
                }
                return;
            }

            //Start / stop slideshow---------------------------------------------------------
            if (Setting.IsPlaySlideShow && e.KeyCode == Keys.Space && !e.Control && !e.Shift && !e.Alt)//SPACE
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

            // Help ------------------------------------------------------------------------
            if (e.KeyValue == 112 && !e.Control && !e.Shift && !e.Alt)//F1
            {
                btnHelp_Click(null, null);
                return;
            }

            //Previous Image----------------------------------------------------------------
            if ((e.KeyValue == 33 || e.KeyValue == 37) && 
                e.Control == false && e.Shift == false && e.Alt == false)//Left arrow / PageUp
            {
                NextPic(-1);
                return;
            }

            //Next Image---------------------------------------------------------------------
            if ((e.KeyValue == 34 || e.KeyValue == 39) && 
                e.Control == false && e.Shift == false && e.Alt == false)//Right arrow / Pagedown
            {
                NextPic(1);
                return;
            }

            //Zoom + ------------------------------------------------------------------------
            if (e.KeyValue == 187 && e.Control && !e.Shift && !e.Alt)// Ctrl + =
            {
                btnZoomIn_Click(null, null);
                return;
            }

            //Zoom - ------------------------------------------------------------------------
            if (e.KeyValue == 189 && e.Control && !e.Shift && !e.Alt)// Ctrl + -
            {
                btnZoomOut_Click(null, null);
                return;
            }

            //Scale to Width------------------------------------------------------------------
            if (e.KeyCode == Keys.W && e.Control && !e.Shift && !e.Alt)// Ctrl + W
            {
                btnScaletoWidth_Click(null, null);
                return;
            }

            //Scale to Height------------------------------------------------------------------
            if (e.KeyCode == Keys.H && e.Control && !e.Shift && !e.Alt)// Ctrl + W
            {
                btnScaletoHeight_Click(null, null);
                return;
            }

            //Auto size window------------------------------------------------------------------
            if (e.KeyCode == Keys.M && e.Control && !e.Shift && !e.Alt)// Ctrl + W
            {
                btnWindowAutosize_Click(null, null);
                return;
            }

            //Auto size image ----------------------------------------------------------------
            if (e.KeyValue == 48 && e.Control && !e.Shift && !e.Alt)// Ctrl + 0
            {
                btnScale11_Click(null, null);
                return;
            }

            //Full screen--------------------------------------------------------------------
            if (e.Alt && e.KeyCode == Keys.Enter && !e.Control && !e.Shift)//Alt + Enter
            {
                btnFullScreen.PerformClick();
                return;
            }

            //Open file----------------------------------------------------------------------
            if (e.KeyValue == 79 && e.Control && !e.Shift && !e.Alt)// Ctrl + O
            {
                OpenFile();
                return;
            }

            //Convert file----------------------------------------------------------------------
            if (e.KeyValue == 83 && e.Control && !e.Shift && !e.Alt)// Ctrl + S
            {
                btnConvert_Click(null, null);
                return;
            }

            //Refresh Image------------------------------------------------------------------
            if (e.KeyValue == 116)//F5
            {
                btnRefresh_Click(null, null);
                return;
            }

            //Goto image---------------------------------------------------------------------
            if (e.KeyValue == 71 && e.Control && !e.Shift && !e.Alt)//Ctrl + G
            {
                try
                {
                    int n = Convert.ToInt32(InputBox.Derp("Goto image ...", (Setting.CurrentIndex + 1).ToString())) - 1;
                    Setting.CurrentIndex = n;
                    NextPic(0);
                }
                catch { }
                return;
            }

            //Extract frames-----------------------------------------------------------------
            if (e.KeyCode == Keys.E && e.Control && !e.Shift && !e.Alt)//Ctrl + E
            {
                try
                {
                    Setting.ImageList.get(Setting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                    mnuExtractFrames_Click(null, null);
                }
                catch { }
                return;
            }

            //Detele to Recycle bin----------------------------------------------------------
            if (e.KeyCode == Keys.Delete && !e.Control && !e.Shift && !e.Alt)//Delete
            {
                mnuRecycleBin_Click(null, null);
                return;
            }

            //Detele From Hark disk----------------------------------------------------------
            if (e.KeyCode == Keys.Delete && !e.Control && e.Shift && !e.Alt)//Shift + Delete
            {
                mnuDelete_Click(null, null);
                return;
            }

            //Open image location------------------------------------------------------------
            if (e.KeyCode == Keys.L && e.Control && !e.Shift && !e.Alt)//Ctrl + L
            {
                mnuImageLocation_Click(null, null);
                return;
            }

            //Image properties----------------------------------------------------------------
            if (e.KeyCode == Keys.I && e.Control && !e.Shift && !e.Alt)//Ctrl + I
            {
                mnuProperties_Click(null, null);
                return;
            }

            //Show thumbnail-------------------------------------------------------------------
            if (e.KeyCode == Keys.T && e.Control && !e.Shift && !e.Alt)// Ctrl + T
            {
                btnThumb.PerformClick();
                return;
            }

            //Caro background------------------------------------------------------------------
            if (e.KeyCode == Keys.B && e.Control && !e.Shift && !e.Alt)// Ctrl + B
            {
                btnCaro.PerformClick();
                return;
            }

            //ImageGlass Settings---------------------------------------------------------------
            if (e.KeyCode == Keys.P && e.Control && !e.Shift && !e.Alt)// Ctrl + P
            {
                btnSetting_Click(null, null);
                return;
            }

            //Rename image-----------------------------------------------------------------------
            if (e.KeyValue == 113)//F2
            {
                RenameImage();
                return;
            }
           
            // Sap hinh anh voi duong bien duoi-----------------------------------------------------
            if (e.KeyCode == Keys.Down)
            {
                //Sap bien duoi
                if (e.Control && !e.Shift && !e.Alt)//Ctrl + Down
                {
                    scrollSmooth(picMain.Left, picMain.Top + sp0.Panel1.Height); //dsp.Top += span.Height;
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    scrollSmooth(picMain.Left, picMain.Top + sp0.Panel1.Height / 2); //dsp.Top += span.Height / 2;
                }
                validateBounds();
                return;
            }

            // Sap bien tren-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Up)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    scrollSmooth(picMain.Left, picMain.Top - sp0.Panel1.Height); //dsp.Top -= span.Height;
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    scrollSmooth(picMain.Left, picMain.Top - sp0.Panel1.Height / 2); //dsp.Top -= span.Height / 2;
                }
                validateBounds();
                return;
            }

            // Sap bien phai-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Right)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    scrollSmooth(picMain.Left + sp0.Panel1.Width, picMain.Top); //dsp.Left += span.Width;
                    validateBounds();
                }
                else if (e.Shift && !e.Control && !e.Alt)
                {
                    scrollSmooth(picMain.Left + sp0.Panel1.Width / 2, picMain.Top); //dsp.Left += span.Width / 2;
                    validateBounds();
                }
                return;

            }

            // Sap bien trai-----------------------------------------------------------------------
            if (e.KeyCode == Keys.Left)
            {
                if (e.Control && !e.Shift && !e.Alt)
                {
                    scrollSmooth(picMain.Left - sp0.Panel1.Width, picMain.Top); //dsp.Left -= span.Width;
                }
                if (e.Shift && !e.Control && !e.Alt)
                {
                    scrollSmooth(picMain.Left - sp0.Panel1.Width / 2, picMain.Top); //dsp.Left -= span.Width / 2;
                }
                validateBounds();
                return;
            }


            //if (e.KeyCode == Keys.B)
            //{
            //    picMain.BackgroundImage = null;
            //    if (e.Control) this.BackColor = Color.White;
            //    if (e.Shift) picMain.BackgroundImage = ImageGlass.Properties.Resources.caro;
            //    if (e.Alt) using (ColorDialog cd = new ColorDialog())
            //        {
            //            cd.ShowDialog();
            //            this.BackColor = cd.Color;
            //        }
            //    return;
            //}


            return;
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
                LoadMaxThumbnailFileSizeConfig();

                int size = M_THUMBNAIL_SIZE;
                ImageViewer imageViewer = new ImageViewer();
                imageViewer.Dock = DockStyle.Bottom;

                //chi load thumbnail voi SIZE < Setting.ThumbnailMaxFileSize MB
                if (new FileInfo(imageFilename).Length > Setting.ThumbnailMaxFileSize * 1024 * 1024)
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
                imageViewer.IndexImage = Setting.ImageFilenameList.IndexOf(Path.GetFileName(imageFilename));

                if (imageViewer.IndexImage == Setting.CurrentIndex)
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

                Setting.CurrentIndex = iv.IndexImage;
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
            string oldFilename; 
            string newname;
            oldFilename = newname = Setting.ImageList.getName(Setting.CurrentIndex);
            string ext = newname.Substring(newname.LastIndexOf("."));
            newname = newname.Substring(0, newname.Length - ext.Length);

            string str = InputBox.Derp("Enter new filename", newname);
            if (str == null)
            {
                return;
            }

            newname = str + ext;
            if (oldFilename == newname)
            {
                return;
            }

            Setting.ImageList.unload(Setting.CurrentIndex);
            picMain.Image = null;

            Application.DoEvents();
            ImageGlass_Image.RenameFile(Setting.CurrentPath + oldFilename, newname);
            NextPic(0);
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
            if (Setting.IsLockWorkspaceEdges)
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

                if (e.Button == MouseButtons.Right && !Setting.IsImageError)
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

                    zoomImage(diff);

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
        void zoomImage(double lambda)
        {
            Setting.IsZooming = true;

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

                Setting.ZoomLockValue = x * 140;
            }
            else
            {
                Setting.ZoomLockValue = 1;
            }
        }

        /// <summary>
        /// Smoothly scroll the image to given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void scrollSmooth(int x, int y)
        {

            // Filter repetitious scrolls
            scrollIdent++;
            int myIdent = scrollIdent;

            // DERP
            if (!Setting.IsSmoothPanning) scrollInstant(x, y);

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

        void scrollInstant(int x, int y)
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

        Rectangle StringToRect(string str)
        {
            string[] args = str.Split(',');
            int[] arg = new int[args.Length];
            for (int a = 0; a < arg.Length; a++)
            {
                arg[a] = Convert.ToInt32(args[a]);
            }
            return new Rectangle(arg[0], arg[1], arg[2], arg[3]);
        }

        string RectToString(Rectangle rc)
        {
            return rc.Left + "," + rc.Top + "," + rc.Width + "," + rc.Height;
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
            lblZoomRatio.ForeColor = Color.White;
            lblImageDateCreate.ForeColor = Color.White;
            lblImageFileSize.ForeColor = Color.White;
            lblImageSize.ForeColor = Color.White;
            lblImageType.ForeColor = Color.White;

            this.BackColor = Color.White;
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            Registry.SetValue(hkey, "BackColor", this.BackColor.ToArgb().ToString());

            // <toolbar_icon>
            btnBack.Image = ImageGlass.Properties.Resources.back;
            btnNext.Image = ImageGlass.Properties.Resources.next;
            btnRotateLeft.Image = ImageGlass.Properties.Resources.leftrotate;
            btnRotateRight.Image = ImageGlass.Properties.Resources.rightrotate;
            btnZoomIn.Image = ImageGlass.Properties.Resources.zoomin;
            btnZoomOut.Image = ImageGlass.Properties.Resources.zoomout;
            btnScale11.Image = ImageGlass.Properties.Resources.scaletofit;
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
            btnSetting.Image = ImageGlass.Properties.Resources.settings;
            btnHelp.Image = ImageGlass.Properties.Resources.about;
            btnLike.Image = ImageGlass.Properties.Resources.like;
            btnDisLike.Image = ImageGlass.Properties.Resources.dislike;
            btnReport.Image = ImageGlass.Properties.Resources.report;

            Registry.SetValue(hkey, "Theme", "default");
        }

        /// <summary>
        /// Thay đổi theme
        /// </summary>
        private void LoadTheme()
        {
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            string themeFile = Registry.GetValue(hkey, "Theme", "default").ToString();

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

                try { this.BackColor = t.backcolor; }
                catch { this.BackColor = Color.White; }
                Registry.SetValue(hkey, "BackColor", this.BackColor.ToArgb().ToString());

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

                try { btnScale11.Image = Image.FromFile(dir + t.scaletofit); }
                catch { btnScale11.Image = ImageGlass.Properties.Resources.scaletofit; }

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

                try { btnLike.Image = Image.FromFile(dir + t.like); }
                catch { btnLike.Image = ImageGlass.Properties.Resources.like; }

                try { btnDisLike.Image = Image.FromFile(dir + t.dislike); }
                catch { btnDisLike.Image = ImageGlass.Properties.Resources.dislike; }

                try { btnReport.Image = Image.FromFile(dir + t.report); }
                catch { btnReport.Image = ImageGlass.Properties.Resources.report; }

                Registry.SetValue(hkey, "Theme", themeFile);
            }
            else
            {
                LoadThemeDefault();
            }

        }

        /// <summary>
        /// Tải dữ liệu từ Registry
        /// </summary>
        private void LoadConfig()
        {
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            Registry.SetValue(hkey, "igVersion", Application.ProductVersion);

            //Windows Bound (Position + Size)------------------------------------------------
            Rectangle rc = StringToRect(Registry.GetValue(hkey, "WindowsBound",
                                        "280,125,750,545").ToString());
            this.Bounds = rc;
            
            //windows state--------------------------------------------------------------
            string s = Registry.GetValue(hkey, "WindowsState", "Normal").ToString();
            if (s == "Normal")
            {
                this.WindowState = FormWindowState.Normal;
            }
            else if (s == "Maximized")
            {
                this.WindowState = FormWindowState.Maximized;
            }

            //Slideshow Interval----------------------------------------------------------
            int i = int.Parse(Registry.GetValue(hkey, "Interval", "5").ToString());
            if (!(0 < i && i < 61)) i = 5;//gioi han thoi gian [1; 60] giay
            timSlideShow.Interval = 1000 * i;

            //Show Caro
            if (bool.Parse(Registry.GetValue(hkey, "Caro", "False").ToString()))
            {
                btnCaro.PerformClick();
            }

            //Khoá khung ảnh---------------------------------------------------------------
            Setting.IsLockWorkspaceEdges = bool.Parse(Registry.GetValue(hkey, "LockToEdge", "True").ToString());
           
            //Tìm ảnh đệ quy----------------------------------------------------------------
            Setting.IsRecursive = bool.Parse(Registry.GetValue(hkey, "Recursive", "False").ToString());
            
            //Lăn chuột mượt----------------------------------------------------------------
            Setting.IsSmoothPanning = bool.Parse(Registry.GetValue(hkey, "SmoothPanning", "False").ToString());

            //Zoom optimization method
            string z = Microsoft.Win32.Registry.GetValue(hkey, "ZoomOptimize", "auto").ToString();
            if (z.ToLower() == "smooth pixels")
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.SmoothPixels;
            }
            else if (z.ToLower() == "clear pixels")
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.ClearPixels;
            }
            else //auto
            {
                Setting.ZoomOptimizationMethod = ZoomOptimizationValue.Auto;
            }
            

            //Kích thước tối đa của file ảnh thu nhỏ----------------------------------------
            LoadMaxThumbnailFileSizeConfig();

            //Thứ tự sắp xếp hình ảnh------------------------------------------------------
            LoadImageOrderConfig();

            //Load theme--------------------------------------------------------------------
            LoadTheme();
                        
        }

        /// <summary>
        /// Lấy kích thước tối đa sẽ nạp file ảnh thu nhỏ
        /// </summary>
        private void LoadMaxThumbnailFileSizeConfig()
        {
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            string s = Registry.GetValue(hkey, "MaxThumbnailFileSize", "1").ToString();
            int i = 1;

            if (int.TryParse(s, out i))
            { }
            Setting.ThumbnailMaxFileSize = i;
        }

        /// <summary>
        /// Lấy dữ liệu thứ tự sắp xếp ảnh
        /// </summary>
        private void LoadImageOrderConfig()
        {
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            string s = Registry.GetValue(hkey, "ImageLoadingOrder", "0").ToString();
            int i = 0;

            if (int.TryParse(s, out i))
            {
                if (-1 < i && i < 6)
                { }
                else
                {
                    i = 0;
                }
            }
            if (i == 1)
            {
                Setting.ImageOrderBy = ImageOrderBy.Length;
            }
            else if (i == 2)
            {
                Setting.ImageOrderBy = ImageOrderBy.CreationTime;
            }
            else if (i == 3)
            {
                Setting.ImageOrderBy = ImageOrderBy.LastAccessTime;
            }
            else if (i == 4)
            {
                Setting.ImageOrderBy = ImageOrderBy.LastWriteTime;
            }
            else if (i == 5)
            {
                Setting.ImageOrderBy = ImageOrderBy.Extension;
            }
            else
            {
                Setting.ImageOrderBy = ImageOrderBy.Name;
            }
        }

        /// <summary>
        /// Lưu dữ liệu xuống Registry
        /// </summary>
        void SaveConfig()
        {
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";

            if (this.WindowState == FormWindowState.Normal)
            {
                //Windows Bound-------------------------------------------------------------------
                Registry.SetValue(hkey, "WindowsBound", RectToString(rect == Rectangle.Empty ?
                                                                    this.Bounds : rect));
            }            

            //Windows State-------------------------------------------------------------------
            Registry.SetValue(hkey, "WindowsState", this.WindowState);
            
            //Caro Style
            Registry.SetValue(hkey, "Caro", btnCaro.Checked.ToString());
            
            //Lan chuot muot------------------------------------------------------------------
            Registry.SetValue(hkey, "SmoothPanning", Setting.IsSmoothPanning);
            
        }

        #endregion


        #region Form event
        private void frmMain_Load(object sender, EventArgs e)
        {
            Application.DoEvents();
            LoadConfig();

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
                if (Setting.ImageList.get(Setting.CurrentIndex).Width <= sp0.Panel1.Width && Setting.ImageList.get(Setting.CurrentIndex).Height <= sp0.Panel1.Height)
                {
                    Point p = new Point();
                    p.X = sp0.Panel1.Width / 2 - Setting.ImageList.get(Setting.CurrentIndex).Width / 2;
                    p.Y = sp0.Panel1.Height / 2 - Setting.ImageList.get(Setting.CurrentIndex).Height / 2;

                    picMain.Image = Setting.ImageList.get(Setting.CurrentIndex);
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
            if (Setting.IsImageError)
            {
                return;
            }
            setZoomOrigin();
            zoomImage(e.Delta);

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
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }
            Prepare(Setting.ImageList.getPath(Setting.CurrentIndex));//cap nhat lai thu muc anh
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
            {
                return;
            }

            Setting.ImageList.filter.incRotate(1);
            NextPic(0);
        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
            {
                return;
            }

            Setting.ImageList.filter.incRotate(-1);
            NextPic(0);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void btnThumb_Click(object sender, EventArgs e)
        {
            Setting.IsShowThumbnail = !Setting.IsShowThumbnail;
            sp0.Panel2Collapsed = !Setting.IsShowThumbnail;

            if (Setting.IsShowThumbnail)
            {
                sp0.Panel2MinSize = 70;
                sp0.SplitterDistance = sp0.Height - 71;
                if (thumbBar.Controls.Count == 0)
                {
                    LoadThumnailImage(true);
                }
            }
        }

        private void btnScale11_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
            {
                return;
            }

            Point l = new Point();
            Setting.IsZooming = false;

            l.X = sp0.Panel1.Width / 2 - picMain.Width / 2;
            l.Y = sp0.Panel1.Height / 2 - picMain.Width / 2;

            picMain.Bounds = new Rectangle(l, picMain.Image.Size);

        }

        private void btnScaletoWidth_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
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
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
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
            if (Setting.ImageList.length < 1 || Setting.IsImageError)
            {
                return;
            }
            // In Soviet Russia, window adapt to image
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
            int n = Setting.CurrentIndex;
            string s = InputBox.Derp("Enter the image index to view it. Press [ENTER] ", "0");

            if (int.TryParse(s, out n))
            {
                n--;

                if (-1 < n && n < Setting.ImageList.length)
                {
                    Setting.CurrentIndex = n;
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
            if (!Setting.IsImageError)
            {
                setZoomOrigin();
                zoomImage(14);

                //Set value of zoom lock
                SetZoomLockValue();
            }

            this.Text = picMain.Width.ToString() + "--" + picMain.Image.Width.ToString();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            if (!Setting.IsImageError)
            {
                setZoomOrigin();
                zoomImage(-14);

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
                Setting.ZoomLockValue = 1;
                btnZoomLock.Checked = false;
            }
        }

        private void timSlideShow_Tick(object sender, EventArgs e)
        {
            NextPic(1);
        }

        private void btnSlideShow_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length < 1)
            {
                return;
            }

            //full screen
            if (this.rect == Rectangle.Empty)
            {
                this.rect = this.Bounds;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.BackColor = Color.Black;
                toolMain.Visible = false;
                Application.DoEvents();
                this.Bounds = Screen.FromControl(this).Bounds;

                timSlideShow.Start();
                timSlideShow.Enabled = true;

                Setting.IsPlaySlideShow = true;
                mnuStartSlideshow.Visible = true;
                mnuStopSlideshow.Visible = true;
                mnuExitSlideshow.Visible = true;
                mnuPhanCach.Visible = true;
                mnuStartSlideshow.Enabled = false;
                mnuStopSlideshow.Enabled = true;
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
            }
            //exit full screen
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                Application.DoEvents();
                this.Bounds = this.rect;
                this.rect = Rectangle.Empty;
            }
        }

        private void btnPrintImage_Click(object sender, EventArgs e)
        {

        }

        private void btnFacebook_Click(object sender, EventArgs e)
        {
            if (Setting.ImageList.length > 0 && File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
            {
                if (Setting.FFacebook.IsDisposed)
                {
                    Setting.FFacebook = new frmFacebook();
                }

                Setting.FFacebook.Filename = Setting.ImageList.getPath(Setting.CurrentIndex);
                Setting.IsForcedActive = false;
                Setting.FFacebook.Show();
                Setting.FFacebook.Activate();
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
            if (Setting.FExtension.IsDisposed)
            {
                Setting.FExtension = new frmExtension();
            }
            Setting.IsForcedActive = false;
            Setting.FExtension.Show();
            Setting.FExtension.Activate();
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (Setting.FSetting.IsDisposed)
            {
                Setting.FSetting = new frmSetting();
            }

            Setting.IsForcedActive = false;
            Setting.FSetting.Show();
            Setting.FSetting.Activate();
        }

        private void mnuEditWithPaint_Click(object sender, EventArgs e)
        {
            if (!Setting.IsImageError)
            {
                System.Diagnostics.Process.Start("mspaint.exe", char.ConvertFromUtf32(34) + Setting.ImageList.getPath(Setting.CurrentIndex) + char.ConvertFromUtf32(34));
            }
        }

        private void mnuProperties_Click(object sender, EventArgs e)
        {
            ImageGlass_ImageInfo.DisplayFileProperties(Setting.ImageList.getPath(Setting.CurrentIndex), this.Handle);
        }

        private void mnuImageLocation_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + Setting.ImageList.getPath(Setting.CurrentIndex) + "\"");
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show("Delete file '" + Setting.ImageList.getPath(Setting.CurrentIndex) + "' ?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg == DialogResult.Yes)
            {
                string f = Setting.ImageList.getPath(Setting.CurrentIndex);

                System.GC.Collect();
                Setting.ImageList.remove(Setting.CurrentIndex);
                picMain.Image = null;


                try
                {
                    ImageGlass_Image.DeleteFile(f, false);
                    NextPic(0);
                }
                catch { }
            }
        }

        private void mnuRecycleBin_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }

            DialogResult msg = MessageBox.Show("Send file '" + Setting.ImageList.getPath(Setting.CurrentIndex) + "' to recycle bin?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg == DialogResult.Yes)
            {
                string f = Setting.ImageList.getPath(Setting.CurrentIndex);

                System.GC.Collect();
                Setting.ImageList.remove(Setting.CurrentIndex);
                picMain.Image = null;

                try
                {
                    ImageGlass_Image.DeleteFile(f, true);
                    NextPic(0);
                }
                catch { }
            }
        }

        private void mnuWallpaper_Click(object sender, EventArgs e)
        {
            if (!Setting.IsImageError)
            {
                ImageGlass_Image.SetWallpaper(Setting.ImageList.getPath(Setting.CurrentIndex));
            }
        }

        private void mnuExtractFrames_Click(object sender, EventArgs e)
        {
            if (!Setting.IsImageError)
            {
                FolderBrowserDialog f = new FolderBrowserDialog();
                f.Description = "Select output folder";
                f.ShowNewFolderButton = true;
                DialogResult res = f.ShowDialog();

                if (res == DialogResult.OK && Directory.Exists(f.SelectedPath))
                {
                    ImageGlass_Animation.ExtractAllFrames(Setting.ImageList.getPath(Setting.CurrentIndex), f.SelectedPath);
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
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)) || Setting.IsImageError)
                {
                    return;
                }
            }
            catch { return; }
            ImageGlass_Image.ConvertImage(picMain.Image, Setting.ImageList.getName(Setting.CurrentIndex));
        }

        private void btnLike_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(char.ConvertFromUtf32(34) +
                                (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "iglike " + Application.ProductVersion);
            }
            catch { }
        }

        private void btnDisLike_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(char.ConvertFromUtf32(34) +
                                (Application.StartupPath + "\\").Replace("\\\\", "\\") + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "igdislike " + Application.ProductVersion);
            }
            catch { }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/imageglass/issues/");
        }

        private void picMain_Paint(object sender, PaintEventArgs e)
        {
            if (Setting.ImageList.length < 1 || Setting.IsZooming == false) return;

            Graphics g = e.Graphics;
            
            if(Setting.ZoomOptimizationMethod == ZoomOptimizationValue.SmoothPixels)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            }
            else if (Setting.ZoomOptimizationMethod == ZoomOptimizationValue.ClearPixels)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
            else //auto
            {
                if (Setting.ImageList.get(Setting.CurrentIndex).Width * Setting.ImageList.get(Setting.CurrentIndex).Height > picMain.Width * picMain.Height)
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                else
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }

            g.DrawImage(Setting.ImageList.get(Setting.CurrentIndex), 0, 0, picMain.Width, picMain.Height);
        }

        private void frmMain_ResizeEnd(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void mnuPopup_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)) || Setting.IsImageError)
                {
                    e.Cancel = true;
                    return;
                }
            }
            catch { e.Cancel = true; return; }

            try
            {
                int i = Setting.ImageList.get(Setting.CurrentIndex).GetFrameCount(System.Drawing.Imaging.FrameDimension.Time);
                mnuExtractFrames.Text = "&Extract image frames (" + i.ToString() + ")";
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

            this.BackColor = Color.White;
            toolMain.Visible = true;

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;

            Application.DoEvents();
            this.Bounds = this.rect;
            this.rect = Rectangle.Empty;

            mnuStartSlideshow.Visible = false;
            mnuStopSlideshow.Visible = false;
            mnuExitSlideshow.Visible = false;
            mnuPhanCach.Visible = false;
        }

        private void mnuCopyImagePath_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(Setting.CurrentPath + Setting.ImageList.getName(Setting.CurrentIndex));
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
            if (Setting.IsForcedActive)
            {
                picMain.Focus();
            }
        }

        private void picMain_MouseEnter(object sender, EventArgs e)
        {
            if (Setting.IsForcedActive)
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
            int imgIndex = Setting.ImageFilenameList.IndexOf(oldName);
            if (imgIndex > -1)
            {
                //Rename image list
                Setting.ImageList.setName(imgIndex, newName);
                Setting.ImageFilenameList[imgIndex] = newName;
                
                //Rename thumbnail list
                foreach (Control c in thumbBar.Controls)
                {
                    ImageViewer ti = (ImageViewer)c;
                    if (ti.IndexImage == imgIndex)
                    {
                        ti.ImageLocation = e.FullPath;
                        tip1.SetToolTip(ti, e.FullPath);
                    }
                    ti.Dispose();
                }
            }
        }

        private void sysWatch_Deleted(object sender, FileSystemEventArgs e)
        {
            //Get index of deleted image
            int imgIndex = Setting.ImageFilenameList.IndexOf(e.Name);

            if (imgIndex > -1)
            {
                //delete image list
                Setting.ImageList.remove(imgIndex);
                Setting.ImageFilenameList.RemoveAt(imgIndex);

                //delete thumbnail list
                for (int i = 0; i < thumbBar.Controls.Count; i++)
                {
                    ImageViewer ti = (ImageViewer)thumbBar.Controls[i];

                    if (ti.IndexImage == imgIndex)
                    {
                        thumbBar.Controls.RemoveAt(i);
                    }
                }
            }
        }

        private void sysWatch_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!File.Exists(Setting.ImageList.getPath(Setting.CurrentIndex)))
                {
                    return;
                }
            }
            catch { return; }
            Prepare(Setting.ImageList.getPath(Setting.CurrentIndex));//cap nhat lai thu muc anh
        }

        #endregion

        

        
        






    }
}