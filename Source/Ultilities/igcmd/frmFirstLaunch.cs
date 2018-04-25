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
using ImageGlass;
using ImageGlass.Theme;

namespace igcmd
{
    public partial class frmFirstLaunch : Form
    {
        public frmFirstLaunch()
        {
            InitializeComponent();
        }


        private List<Theme> themeList = new List<Theme>();




        #region Events

        private void frmFirstLaunch_Load(object sender, EventArgs e)
        {
            //Load language list
            LoadLanguageList();

            //Select default layout
            cmbLayout.SelectedIndex = 0;

            //Load theme list
            LoadThemeList();
            

            //Don't run again
            GlobalSetting.SetConfig("IsRunFirstLaunchConfigurations", "False");
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

        #endregion



        #region Private Functions
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


        /// <summary>
        /// Launch ImageGlass app
        /// </summary>
        private void LaunchImageGlass()
        {
            var appExe = Path.Combine(GlobalSetting.StartUpDir, "ImageGlass.exe");

            Process p = new Process();
            p.StartInfo.FileName = Path.Combine(appExe);
            p.Start();
        }


        /// <summary>
        /// Load theme list
        /// </summary>
        private void LoadThemeList()
        {
            //add default theme
            var defaultTheme = new Theme(Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml"));
            themeList.Add(defaultTheme);
            cmbTheme.Items.Add(defaultTheme.Name);
            cmbTheme.SelectedIndex = 0;


            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\");

            //get the current theme
            var currentTheme = GlobalSetting.GetConfig("Theme", "Default");


            if (Directory.Exists(dir))
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string configFile = Path.Combine(d, "config.xml");

                    if (File.Exists(configFile))
                    {
                        Theme th = new Theme();

                        //invalid theme
                        if (!th.LoadTheme(configFile))
                        {
                            continue;
                        }

                        themeList.Add(th);
                        cmbTheme.Items.Add(th.Name);

                        if (currentTheme.ToLower().CompareTo(th.ThemeConfigFilePath.ToLower()) == 0)
                        {
                            cmbTheme.SelectedIndex = cmbTheme.Items.Count - 1;
                        }
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(dir);
            }

            
        }


        #endregion




    }
}
