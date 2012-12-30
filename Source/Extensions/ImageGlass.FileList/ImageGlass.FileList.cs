using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
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
