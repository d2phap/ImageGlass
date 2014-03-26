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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace ImageGlass.ThumbBar
{
    
    /// <summary>
    /// Class representing a sequence of thumbnails in a flow layout panel.
    /// </summary>
    public class ThumbnailsSequence
        : FlowLayoutPanel
    {

        public ThumbnailsSequence()
            : base()
        {
            this.DoubleBuffered = true;
            this.allowSetThumbnailBorder = true;
            this.pendingAction = false;

            this.VerticalScroll.SmallChange = 25;
            this.VerticalScroll.LargeChange = 50;            

            this.imageFiles = new List<ImageFile>();
        }

        #region Properties
        /// <summary>
        /// Sets the directory where images reside.
        /// </summary>
        public string CurrentDirectory
        {
            set
            { 
                this.currentDirectory = value;
            }
        }

        /// <summary>
        /// Gets, sets thumbnail size
        /// </summary>
        public int ThumbnailSize
        {
            set
            {
                GlobalData.ThumbnailSize = value;
                GlobalData.ReloadData();
            }
            get
            {
                return GlobalData.ThumbnailSize;
            }
        }
        #endregion


        #region Public method
        public void SetPendingAction()
        {
            this.pendingAction = true;
        }

        /// <summary>
        /// Removes the thumbnails from the container control.
        /// </summary>
        public void DisposeOfPreviousThumbnails()
        {
            this.currentThumbnail = null;

            foreach (ThumbnailBox thumbnail in this.Controls)
                thumbnail.Dispose();

            this.Controls.Clear();

            foreach (ImageFile anImageFile in imageFiles)
                anImageFile.Dispose();
        }

        /// <summary>
        /// Shows the thumbnails on the screen.
        /// </summary>
        public void ShowThumbnails(List<string> @imageFileList = null)
        {
            this.SuspendLayout();

            this.DisposeOfPreviousThumbnails();
            this.AutoScrollPosition = new Point(0, 0);

            if (imageFileList == null)
            {
                this.AddNewThumbnails();
            }
            else
            {
                this.AddNewThumbnails(imageFileList);
            }

            this.ResumeLayout(true);

            this.SetThumbnails();
        }

        /// <summary>
        /// Moves to the given thumbnail in the sequence.
        /// </summary>
        public void MoveToThumbnail(ThumbnailBox thumbnail)
        {
            if (thumbnail != null)
            {
                this.SuspendLayout();
                
                SetThumbnailBorder(BorderStyle.None);
                currentThumbnail = thumbnail;
                SetThumbnailBorder(BorderStyle.FixedSingle);
                ScrollControlIntoView(currentThumbnail);

                this.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Moves to the given thumbnail index in the sequence.
        /// </summary>
        /// <param name="index"></param>
        public void MoveToThumbnail(int index)
        {
            
        }

        /// <summary>
        /// Moves to the next thumbnail in the sequence, if any.
        /// </summary>
        public ThumbnailBox MoveToNextThumbnail()
        {
            MoveToThumbnail(this.NextThumbnail);

            if (this.NextThumbnail != null)
            {
                return this.NextThumbnail;
            }
            return this.CurrentThumbnail;
        }

        /// <summary>
        /// Moves to the previous thumbnail in the sequence, if any.
        /// </summary>
        public ThumbnailBox MoveToPreviousThumbnail()
        {
            MoveToThumbnail(this.PreviousThumbnail);

            if (this.PreviousThumbnail != null)
            {
                return this.PreviousThumbnail;
            }
            return this.CurrentThumbnail;
        }


        /// <summary>
        /// Gets or sets the currently selected thumbnail.
        /// </summary>
        public ThumbnailBox CurrentThumbnail
        {
            get { return currentThumbnail; }
            set { currentThumbnail = value; }
        }

        /// <summary>
        /// Gets the next thumbnail in the sequence.
        /// </summary>
        public ThumbnailBox NextThumbnail
        {
            get
            {
                if (currentThumbnail == null)
                {
                    if (this.Controls.Count > 0)
                        return (ThumbnailBox)this.Controls[0];
                    else
                        return null;
                }
                else
                {
                    if (currentThumbnail.ThumbnailIndex < this.Controls.Count - 1)
                        return (ThumbnailBox)this.Controls[currentThumbnail.ThumbnailIndex + 1];
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the previous thumbnail in the sequence.
        /// </summary>
        public ThumbnailBox PreviousThumbnail
        {
            get
            {
                if (currentThumbnail == null)
                {
                    if (this.Controls.Count > 0)
                        return (ThumbnailBox)this.Controls[this.Controls.Count - 1];
                    else
                        return null;
                }
                else
                {
                    if (currentThumbnail.ThumbnailIndex > 0)
                        return (ThumbnailBox)this.Controls[currentThumbnail.ThumbnailIndex - 1];
                    else
                        return null;
                }
            }
        }


        public void StopThumbnailsGeneration()
        {
            if (tasksDispatcher != null)
                tasksDispatcher.Stop();
        }
        #endregion


        #region Private

        private delegate ThumbnailBox CreateThumbnailBoxDelegate(int i);

        /// <summary>
        /// Display tooltip
        /// </summary>
        private ToolTip tip = new ToolTip();

        /// <summary>
        /// The directory where images reside.
        /// </summary>
        private string currentDirectory;

        /// <summary>
        /// The list of image files within the directory.
        /// </summary>
        private IList<ImageFile> imageFiles;

        private TasksDispatcher tasksDispatcher;

        /// <summary>
        /// The currently selected thumbnail.
        /// </summary>
        private ThumbnailBox currentThumbnail;

        /// <summary>
        /// Determines whether or not the navigation through the thumbnails should reset
        /// their border status when navigated to.
        /// </summary>
        private bool allowSetThumbnailBorder;

        private bool pendingAction;


        /// <summary>
        /// Adds the new thumbnails to the container control.
        /// </summary>
        private void AddNewThumbnails()
        {
            try
            {
                ImageFolder container = new ImageFolder(currentDirectory);
                imageFiles = container.ImagesList;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Adds the new thumbnails to the container control.
        /// </summary>
        /// <param name="imageFiles">Image list</param>
        private void AddNewThumbnails(List<string> imageFileList)
        {
            try
            {
                ImageFolder container = new ImageFolder(imageFileList);
                this.imageFiles = container.ImagesList;
            }
            catch
            {
            }
        }

        private void SetThumbnailBorder(BorderStyle borderStyle)
        {
            if (allowSetThumbnailBorder == true)
            {
                if (currentThumbnail != null)
                {
                    currentThumbnail.SetBorder(borderStyle);
                    currentThumbnail.Select();
                }
            }
        }

        private void GenerateThumbnail(object index)
        {
            ThumbnailBox tb;

            if (this.InvokeRequired)
                tb = (ThumbnailBox)this.Invoke(new CreateThumbnailBoxDelegate(GenerateThumbnailHelper), (int)index);
            else
                tb = GenerateThumbnailHelper((int)index);

            tb.ReadImageFromDiscThread();
        }

        // The delegate procedure we are assigning to our object
        public delegate void GeneratingHandler(object sender, GeneratingEventArgs e);
        public event GeneratingHandler OnGeneratingThumbnailItem;

        private ThumbnailBox GenerateThumbnailHelper(int i)
        {
            ThumbnailBox tb = new ThumbnailBox(this, i, imageFiles[i]);
            ////////////////////////////////////////////////////////////
            //Raise event
            GeneratingEventArgs e = new GeneratingEventArgs(tb);
            e.Index = i;
            OnGeneratingThumbnailItem(this, e);

            this.Controls.Add(tb);

            if (currentThumbnail == null)
                MoveToThumbnail(tb);

            return tb;
        }

        private void SetThumbnails()
        {
            tasksDispatcher = new TasksDispatcher(GenerateThumbnail, SetThumbnailImage, imageFiles.Count);
            tasksDispatcher.Start();
        }

        private void SetThumbnailImage(object index)
        {
            ThumbnailBox aThumbnailBox = (ThumbnailBox)this.Controls[(int)index];
            aThumbnailBox.SetThumbnailImageThread();
        }

        #endregion


        #region Protected

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if ((pendingAction) && (currentThumbnail != null))
            {
                ScrollControlIntoView(currentThumbnail);
                pendingAction = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((currentThumbnail != null) && (!currentThumbnail.Focused))
                currentThumbnail.Focus();
        }

        #endregion

    }

}
