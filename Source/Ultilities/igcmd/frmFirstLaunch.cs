using ImageGlass.Services.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageGlass.Library;

namespace igcmd
{
    public partial class frmFirstLaunch : Form
    {
        public frmFirstLaunch()
        {
            InitializeComponent();
        }

        private void frmFirstLaunch_Load(object sender, EventArgs e)
        {
            //Load language list
            LoadLanguageList();

            //Select default layout
            cmbLayout.SelectedIndex = 0;

            //Select default theme
            cmbTheme.SelectedIndex = 0;

            //Don't run again
            GlobalSetting.SetConfig("IsRunFirstLaunchConfigurations", "False");
        }


        /// <summary>
        /// Load language list
        /// </summary>
        private void LoadLanguageList()
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("English");

            var langList = new List<Language>
            {
                new Language()
            };

            string langPath = Path.Combine(GlobalSetting.StartUpDir, "Languages");

            if (!Directory.Exists(langPath))
            {
                Directory.CreateDirectory(langPath);
            }
            else
            {
                foreach (string f in Directory.GetFiles(langPath))
                {
                    if (Path.GetExtension(f).ToLower() == ".iglang")
                    {
                        Language l = new Language(f);
                        langList.Add(l);

                        int iLang = cmbLanguage.Items.Add(l.LangName);
                        string curLang = GlobalSetting.LangPack.FileName;

                        //using current language pack
                        if (f.CompareTo(curLang) == 0)
                        {
                            cmbLanguage.SelectedIndex = iLang;
                        }
                    }
                }
            }

            if (cmbLanguage.SelectedIndex == -1)
            {
                cmbLanguage.SelectedIndex = 0;
            }
        }




        

        private void btnNextStep_Click(object sender, EventArgs e)
        {
            if (tab1.SelectedIndex == tab1.TabCount - 1)
            {
                LaunchImageGlass();
                this.Close();
            }

            tab1.SelectedIndex++;
            lblStepNumber.Text = $"Step {tab1.SelectedIndex + 1}/{tab1.TabCount}";


            if (tab1.SelectedIndex == tab1.TabCount - 1)
            {
                btnNextStep.Text = "Done!";
            }
        }

        private void lnkSkip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LaunchImageGlass();
            this.Close();
        }

        private void LaunchImageGlass()
        {
            var appExe = Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe");

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(appExe);
            p.Start();
        }

        private void btnSetDefaultApp_Click(object sender, EventArgs e)
        {
            // Update extensions to registry
            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igtasks.exe");
            p.StartInfo.Arguments = $"regassociations {GlobalSetting.AllImageFormats}";

            try
            {
                p.Start();
            }
            catch { }
        }
    }
}
