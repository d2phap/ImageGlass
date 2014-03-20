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
using ImageGlass.ThumbBar.ImageHandling;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.ThumbBar
{
    public partial class ThumbnailBox
        : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="thumbnailsSequence">The thumbnails sequence parent container</param>
        /// <param name="thumbnailIndex">The index of the thumbnail in the thumbnail sequence of the folder</param>
        /// <param name="imageFile">The image file whose thumbnail is to be displayed</param>
        public ThumbnailBox(ThumbnailsSequence thumbnailsSequence, int thumbnailIndex, ImageFile imageFile)
        {
            InitializeComponent();

            this.thumbnailsSequence = thumbnailsSequence;
            this.thumbnailIndex = thumbnailIndex;
            this.imageFile = imageFile;

            SetLoadingImageThumbnail();
        }

        /// <summary>
        /// Gets or sets the index of the thumbnail in the thumbnail sequence of the folder.
        /// </summary>
        public int ThumbnailIndex
        {
            get { return thumbnailIndex; }
            set { thumbnailIndex = value; }
        }

        /// <summary>
        /// Gets the image corresponding to the thumbnail.
        /// </summary>
        public ImageFile Image
        {
            get { return imageFile; }
        }

        /// <summary>
        /// Thread method that sets the thumbnail image to be displayed.
        /// </summary>
        public void SetThumbnailImageThread()
        {
            Image anImage = imageFile.Thumbnail;

            if (this.InvokeRequired)
                this.Invoke(new SetThumbnailImageThreadDelegate(SetThumbnailImageHelper), anImage);
            else
                SetThumbnailImageHelper(anImage);
        }

        /// <summary>
        /// Thread method that reads the image from disc.
        /// </summary>
        public void ReadImageFromDiscThread()
        {
            imageFile.GetImageFromFile();
        }

        /// <summary>
        /// Sets the border style of the control.
        /// </summary>
        /// <param name="borderStyle">The border style to be set</param>
        public void SetBorder(BorderStyle borderStyle)
        {
            this.BorderStyle = borderStyle;
        }


        #region Protected

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //panelThumbnailBox.Cursor = this.Cursor;
            //panelThumbnailBox.MouseClick += new MouseEventHandler(Controls_MouseClick);
            
            //foreach (Control aControl in panelThumbnailBox.Controls)
            //{
            //    aControl.Cursor = this.Cursor;
            //    aControl.MouseClick += new MouseEventHandler(Controls_MouseClick);
            //}

            pbThumbnail.MouseClick += new MouseEventHandler(Controls_MouseClick);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (thumbnailsSequence.CurrentThumbnail != null)
            {
                KeyDownEventArgs ev = new KeyDownEventArgs(e);
                
                if (e.KeyCode == Keys.Right)
                {
                    thumbnailsSequence.MoveToNextThumbnail();

                    ev.ThumbnailItem = thumbnailsSequence.CurrentThumbnail;
                    ev.Index = thumbnailsSequence.CurrentThumbnail.ThumbnailIndex;

                    OnSelectThumbnailItem(thumbnailsSequence.CurrentThumbnail, ev);
                }
                else if (e.KeyCode == Keys.Left)
                {
                    thumbnailsSequence.MoveToPreviousThumbnail();

                    ev.ThumbnailItem = thumbnailsSequence.CurrentThumbnail;
                    ev.Index = thumbnailsSequence.CurrentThumbnail.ThumbnailIndex;

                    OnSelectThumbnailItem(thumbnailsSequence.CurrentThumbnail, ev);
                }

            }
            //if (e.KeyCode == Keys.Enter)
            //{
            //    if (thumbnailsSequence.CurrentThumbnail != null)
            //        new ImageForm(thumbnailsSequence, thumbnailsSequence.CurrentThumbnail.Image).ShowDialog();
            //}
            //else if ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Right))
            //{
                
            //    thumbnailsSequence.MoveToNextThumbnail();
            //    MessageBox.Show(thumbnailIndex.ToString() + "_" + imageFile.ShortFileName);
            //}
            //else if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Left))
            //{
            //    thumbnailsSequence.MoveToPreviousThumbnail();
            //    MessageBox.Show(thumbnailIndex.ToString() + "_" + imageFile.ShortFileName);
            //}
        }

        //protected override void OnMouseClick(MouseEventArgs e)
        //{
        //    base.OnMouseClick(e);

        //    if (e.Button == MouseButtons.Left)
        //    {
        //        thumbnailsSequence.MoveToThumbnail(this);
        //        new ImageForm(thumbnailsSequence, imageFile).ShowDialog();
        //    }
        //}

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        #endregion


        #region Private

        private ToolTip tip = new ToolTip();

        /// <summary>
        /// The thumbnails sequence parent container.
        /// </summary>
        private ThumbnailsSequence thumbnailsSequence;

        /// <summary>
        /// The index of the thumbnail in the thumbnail sequence of the folder.
        /// </summary>
        private int thumbnailIndex;

        /// <summary>
        /// The image file whose thumbnail is to be displayed.
        /// </summary>
        private ImageFile imageFile;

        /// <summary>
        /// Helper delegate for the thread setting the image thumbnail.
        /// </summary>
        private delegate void SetThumbnailImageThreadDelegate(Image thumbnail);


        public delegate void ThumbnailSelectHandler(object sender, KeyDownEventArgs e);
        public event ThumbnailSelectHandler OnSelectThumbnailItem;

        // The delegate procedure we are assigning to our object
        public delegate void ThumbnailClickHandler(object sender, ThumbnailItemClickEventArgs e);
        public event ThumbnailClickHandler OnClickThumbnailItem;

        /// <summary>
        /// Creates a new form to display the full-size image corresponding to the thumbnail.
        /// </summary>
        private void Controls_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                thumbnailsSequence.MoveToThumbnail(this);

                //Raise event
                ThumbnailItemClickEventArgs ev = new ThumbnailItemClickEventArgs();
                ev.Index = this.ThumbnailIndex;
                ev.Filename = imageFile.LongFileName;
                OnClickThumbnailItem(this, ev);
            }
        }

        /// <summary>
        /// Show tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThumbnailBox_Load(object sender, EventArgs e)
        {
            tip.SetToolTip(pbThumbnail, this.imageFile.LongFileName);
        }

        /// <summary>
        /// Helper method for setting the thumbnail image.
        /// </summary>
        /// <param name="anImage">The thumbnail image to be set</param>
        private void SetThumbnailImageHelper(Image anImage)
        {
            pbThumbnail.Image = anImage;
        }


        /// <summary>
        /// Sets the image load logo as thumbnail.
        /// </summary>
        private void SetLoadingImageThumbnail()
        {
            pbThumbnail.Image = GlobalData.LoadingImageThumbnail;
        }

        #endregion


        

    }

}
