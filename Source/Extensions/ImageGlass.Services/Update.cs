using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Diagnostics;
using ImageGlass.Library.Net;

namespace ImageGlass.Services
{
    public class Update
    {
        private InfoUpdate _info;
        private HotfixUpdate _hotfix;

        #region Properties
        /// <summary>
        /// Get / set information of info update
        /// </summary>
        public InfoUpdate Info
        {
            get { return _info; }
            set { _info = value; }
        }
        
        /// <summary>
        /// Get / set information of hotfix update
        /// </summary>
        public HotfixUpdate Hotfix
        {
            get { return _hotfix; }
            set { _hotfix = value; }
        }
        #endregion

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update(Uri link, string savedPath)
        {
            _info = new InfoUpdate();
            _hotfix = new HotfixUpdate();

            //Get information update pack
            GetUpdateConfig(link, savedPath);
        }

        /// <summary>
        /// Provides structure, method of Update
        /// </summary>
        public Update()
        {
            _info = new InfoUpdate();
            _hotfix = new HotfixUpdate();
        }


        /// <summary>
        /// Get update data from server
        /// </summary>
        /// <param name="link"></param>
        /// <param name="savedPath"></param>
        /// <returns></returns>
        private bool GetUpdateConfig(Uri link, string savedPath)
        {
            //Get config file
            try
            {
                if (File.Exists(savedPath)) { File.Delete(savedPath); }

                System.Net.WebClient w = new WebClient();
                w.DownloadFile(link, savedPath);
            }
            catch { return false; }

            //return FALSE if config file is not exist
            if (!File.Exists(savedPath)) { return false; }

            //Init
            LoadUpdateConfig(savedPath);

            return true;
        }

        /// <summary>
        /// Load update data from XML file
        /// </summary>
        /// <param name="filename"></param>
        public void LoadUpdateConfig(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            XmlElement nType = (XmlElement)root.SelectNodes("Update")[0]; //<Update>
            XmlElement n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

            //Get <Info> Attributes
            _info.NewVersion = new Version(n.GetAttribute("newVersion"));
            _info.VersionType = n.GetAttribute("versionType");
            _info.Level = n.GetAttribute("level");
            _info.Link = new Uri(n.GetAttribute("link"));
            _info.Size = n.GetAttribute("size");
            _info.Decription = n.GetAttribute("decription");

            //Get <Hotfix> Attributes
            n = (XmlElement)nType.SelectNodes("Hotfix")[0];//<Hotfix>
            _hotfix.Count = int.Parse(n.GetAttribute("count"));
            string sTempPath = n.GetAttribute("temp") + "\\";//{root}\a\b\c
            string rootPath = System.Windows.Forms.Application.StartupPath;
            sTempPath = sTempPath.Replace("{root}", rootPath);
            _hotfix.TempDir = sTempPath.Replace("\\\\", "\\");
            if (!Directory.Exists(_hotfix.TempDir)) Directory.CreateDirectory(_hotfix.TempDir);

            //Get Hotfix items
            for (int i = 0; i < _hotfix.Count; i++)
            {
                HotfixItemUpdate hf = new HotfixItemUpdate();
                XmlElement node = (XmlElement)n.SelectNodes("_" + (i + 1).ToString())[0];//<_1>
                hf.Level = node.GetAttribute("level");
                hf.NewVersion = new Version(node.GetAttribute("newVersion"));
                hf.Link = new Uri(node.GetAttribute("link"));
                hf.OriginalFile = node.GetAttribute("oriFile");
                hf.OriginalFile = hf.OriginalFile.Replace("{root}", rootPath);
                hf.DestinaionFile = node.GetAttribute("desFile");
                hf.DestinaionFile = hf.DestinaionFile.Replace("{root}", rootPath);

                _hotfix.HotfixItems.Add(hf);
            }
        }


        /// <summary>
        /// Load current ImageGlass.exe file and compare to the latest version.
        /// Equal is TRUE, else FALSE
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public bool CheckForUpdate(string exePath)
        {
            FileVersionInfo fv = FileVersionInfo.GetVersionInfo(exePath);

            //There is a new version
            if (this.Info.NewVersion.ToString().CompareTo(fv.FileVersion) != 0)
            {
                return true;
            }

            //default don't need to update
            return false;
        }

        /// <summary>
        /// Check for the latest hotfixs
        /// </summary>
        /// <returns></returns>
        public List<HotfixItemUpdate> CheckHotfix()
        {
            List<HotfixItemUpdate> ds = new List<HotfixItemUpdate>();

            for (int i = 0; i < _hotfix.HotfixItems.Count; i++)
            {
                //If hotfix exists
                if (File.Exists(_hotfix.HotfixItems[i].OriginalFile))
                {
                    //Get hotfix version
                    FileVersionInfo fv = FileVersionInfo.GetVersionInfo(_hotfix.HotfixItems[i].OriginalFile);

                    //Compare version
                    if (_hotfix.HotfixItems[i].NewVersion.ToString().CompareTo(fv.FileVersion) != 0)
                    {
                        ds.Add(_hotfix.HotfixItems[i]); //add latest hostfix
                    }
                }
                else //if hotfix is not exist
                {
                    ds.Add(_hotfix.HotfixItems[i]); //add latest hostfix
                }
            }

            //Return hotfixs which need install
            return ds;
        }



        #region Installing Event
        public delegate void GetInstallingStatusEventHandler(object sender, InstallingStatusEventArgs e);
        public delegate void FinishInstallingEventHandler(object sender, EventArgs e);

        public event GetInstallingStatusEventHandler GetInstallingStatus;
        public event FinishInstallingEventHandler GetFinishedStatus;

        protected virtual void OnInstallingStatus(InstallingStatusEventArgs e)
        {
            if (GetInstallingStatus != null) GetInstallingStatus(this, e);
        }

        protected virtual void OnFinishedStatus(EventArgs e)
        {
            if (GetFinishedStatus != null) GetFinishedStatus(this, e);
        }
        #endregion

        double totalProg = 0;
        double curProg = 0;
        InstallingStatusEventArgs ev = new InstallingStatusEventArgs();

        /// <summary>
        /// Installs hotfix
        /// </summary>
        /// <param name="ds"></param>
        public void InstallHotfix(List<HotfixItemUpdate> ds)
        {
            //temp directory
            string temp = _hotfix.TempDir;
            totalProg = 0;
            curProg = 0;            
            ev.Description = "Preparing to install hotfixs...";
            OnInstallingStatus(ev);

            for (int i = 0; i < ds.Count; i++)
            {
                totalProg += 1000;
            }

            ev.TotalPro = totalProg + (ds.Count * 100);
            OnInstallingStatus(ev);

            //download hotfix
            for (int i = 0; i < ds.Count; i++)
            {
                System.Windows.Forms.Application.DoEvents();

                FileDownloader d = new FileDownloader();
                d.AmountDownloadedChanged += new FileDownloader.AmountDownloadedChangedEventHandler(d_AmountDownloadedChanged);

                ev.Description = "Downloading " + Path.GetFileName(ds[i].DestinaionFile) + " ...";
                OnInstallingStatus(ev);

                if (d.DownloadFileWithProgress(ds[i].Link.ToString(), temp + Path.GetFileName(ds[i].DestinaionFile)))
                {

                }
            }

            //Stop all processes of application
            foreach (Process p in Process.GetProcessesByName("ImageGlass.exe"))
            {
                ev.Description = "Stopping all ImageGlass components...";
                OnInstallingStatus(ev);

                //p.Kill();
            }

            //Install file
            for (int i = 0; i < ds.Count; i++)
            {
                System.Windows.Forms.Application.DoEvents();
                if (File.Exists(ds[i].OriginalFile))
                {
                    //Get file version
                    FileVersionInfo fv = FileVersionInfo.GetVersionInfo(ds[i].OriginalFile);
                    //Backup file with version
                    File.Move(ds[i].OriginalFile, ds[i].OriginalFile + "." + fv.FileVersion.ToString());
                }

                //Copy new file
                File.Copy(temp + Path.GetFileName(ds[i].DestinaionFile), ds[i].DestinaionFile);

                //Raise event on installing
                ev.CurProg += 100;
                ev.Description = "Installing " + Path.GetFileName(ds[i].DestinaionFile) + " ...";
                ev.ID = i;
                OnInstallingStatus(ev);
            }

            //Raise event OnGetFinishedStatus finished
            OnFinishedStatus(EventArgs.Empty);
        }

        void d_AmountDownloadedChanged(long iNewProgress)
        {
            curProg += iNewProgress;
            ev.CurProg = curProg;
            OnInstallingStatus(ev);
        }




    }


    public class InstallingStatusEventArgs : EventArgs
    {
        private int _id = 0;
        private Double _totalPro = 0;
        private double _curProg = 0;
        private string _description = string.Empty;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public Double TotalPro
        {
            get { return _totalPro; }
            set { _totalPro = value; }
        }
        

        public double CurProg
        {
            get { return _curProg; }
            set { _curProg = value; }
        }

        /// <summary>
        /// Current hotfix id being installed
        /// </summary>
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

    }


}
