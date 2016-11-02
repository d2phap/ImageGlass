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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageGlass.FileList
{
    public partial class FileList : UserControl
    {
        public FileList()
        {
            InitializeComponent();

            ds = new List<FileListItem>();
            _title = string.Empty;
        }


        private List<FileListItem> ds;
        private string _title;


        #region Properties
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<FileListItem> Items
        {
            get { return ds; }
            set { ds = value; }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                //lblTitle.Text = _title;
            }
        }
        #endregion

        /// <summary>
        /// Add a FileListItem to list
        /// </summary>
        /// <param name="fi"></param>
        public void AddItems(FileListItem fi)
        {
            ds.Add(fi);
        }

        /// <summary>
        /// Add FileListItem from file path
        /// </summary>
        /// <param name="filePath"></param>
        public void AddItems(string filePath)
        {
            FileListItem fi = new FileListItem();
            FileVersionInfo f = FileVersionInfo.GetVersionInfo(filePath);

            fi.ImgAvatar = Icon.ExtractAssociatedIcon(filePath).ToBitmap();
            fi.Title = System.IO.Path.GetFileName(filePath);
            fi.Path = filePath;
            fi.CurrenVersion = f.FileVersion;

            ds.Add(fi);

        }

        /// <summary>
        /// Add FileListItems from file path array
        /// </summary>
        /// <param name="fileArray"></param>
        public void AddItems(string[] fileArray)
        {
            foreach (string f in fileArray)
            {
                FileListItem fi = new FileListItem();
                FileVersionInfo fv = FileVersionInfo.GetVersionInfo(f);

                fi.ImgAvatar = Icon.ExtractAssociatedIcon(f).ToBitmap();
                fi.Title = System.IO.Path.GetFileName(f);
                fi.Path = f;
                fi.CurrenVersion = fv.FileVersion;

                ds.Add(fi);
            }
        }


        public void ReLoadItems()
        {
            pan.Controls.Clear();

            Point p = new Point(0, 0);

            for (int i = 0; i < ds.Count; i++)
            {
                ds[i].Location = p;
                ds[i].Width = pan.Width - 20;
                ds[i].Height = 45;
                ds[i].Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top 
                                | System.Windows.Forms.AnchorStyles.Left) 
                                | System.Windows.Forms.AnchorStyles.Right)));

                pan.Controls.Add(ds[i]);
                p.Y = p.Y + ds[i].Height + 2;
            }
        }


    }
}
