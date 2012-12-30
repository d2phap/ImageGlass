using System;
using System.Drawing;
using System.Threading;
using System.IO;

namespace ImageGlass.ThumbBar
{
    public class ThumbnailControllerEventArgs : EventArgs
    {
        public ThumbnailControllerEventArgs(string imageFilename)
        {
            this.ImageFilename = imageFilename;
        }

        public string ImageFilename;
    }

    public delegate void ThumbnailControllerEventHandler(object sender, ThumbnailControllerEventArgs e);

    public class ThumbnailController
    {
        private bool m_CancelScanning;
        static readonly object cancelScanningLock = new object();

        public bool CancelScanning
        {
            get
            {
                lock (cancelScanningLock)
                {
                    return m_CancelScanning;
                }
            }
            set
            {
                lock (cancelScanningLock)
                {
                    m_CancelScanning = value;
                }
            }
        }

        public event ThumbnailControllerEventHandler OnStart;
        public event ThumbnailControllerEventHandler OnAdd;
        public event ThumbnailControllerEventHandler OnEnd;

        public ThumbnailController()
        {
            
        }

        /// <summary>
        /// Add thumbnail items
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFile(string[] filePath)
        {
            CancelScanning = false;

            Thread thread = new Thread(new ParameterizedThreadStart(AddFile));
            thread.IsBackground = true;
            thread.Start(filePath);
        }

        public void AddFile(object filePath)
        {
            string[] path = (string[])filePath;

            if (this.OnStart != null)
            {
                this.OnStart(this, new ThumbnailControllerEventArgs(null));
            }

            this.AddFileIntern(path);

            if (this.OnEnd != null)
            {
                this.OnEnd(this, new ThumbnailControllerEventArgs(null));
            }

            CancelScanning = false;
        }

        private void AddFileIntern(string[] filePath)
        {
            if (CancelScanning) return;

            // not using AllDirectories
            string[] files = filePath;
            foreach (string file in files)
            {
                if (CancelScanning) break;

                Image img = null;

                try
                {
                    img = Image.FromFile(file);
                }
                catch
                {
                    // do nothing
                }

                if (img != null)
                {
                    this.OnAdd(this, new ThumbnailControllerEventArgs(file));

                    img.Dispose();
                }
            }
                        
        }

        /// <summary>
        /// Add thumbnail folder
        /// </summary>
        /// <param name="folderPath"></param>
        public void AddFolder(string folderPath)
        {
            CancelScanning = false;

            Thread thread = new Thread(new ParameterizedThreadStart(AddFolder));
            thread.IsBackground = true;
            thread.Start(folderPath);
        }

        private void AddFolder(object folderPath)
        {
            string path = (string)folderPath;

            if (this.OnStart != null)
            {
                this.OnStart(this, new ThumbnailControllerEventArgs(null));
            }

            this.AddFolderIntern(path);

            if (this.OnEnd != null)
            {
                this.OnEnd(this, new ThumbnailControllerEventArgs(null));
            }

            CancelScanning = false;
        }

        private void AddFolderIntern(string folderPath)
        {
            if (CancelScanning) return;

            // not using AllDirectories
            string[] files = Directory.GetFiles(folderPath);
            foreach(string file in files)
            {
                if (CancelScanning) break;

                Image img = null;

                try
                {
                    img = Image.FromFile(file);
                }
                catch
                {
                    // do nothing
                }

                if (img != null)
                {
                    this.OnAdd(this, new ThumbnailControllerEventArgs(file));

                    img.Dispose();
                }
            }
        }
    }
}
