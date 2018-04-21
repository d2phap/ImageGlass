/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

using System.Xml;
using System.IO;
using System.Drawing;
using ImageGlass.Services.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using System.Text;

namespace ImageGlass.Theme
{
    public class Theme
    {
        private string _themeConfigFilePath = string.Empty;
        public string ThemeConfigFilePath { get => _themeConfigFilePath; }

        //Info
        public string name = string.Empty;                  
        public string version = string.Empty;               
        public string author = string.Empty;                
        public string email = string.Empty;                 
        public string website = string.Empty;               
        public string description = string.Empty;           
        public string type = string.Empty;                     //config file type
        public string compatibility = string.Empty;            //minimum version requirement

        //main
        public ThemeImage PreviewImage { get; set; } = new ThemeImage();
        public Color BackgroundColor { get; set; } = Color.White;

        public ThemeImage ToolbarBackgroundImage { get; set; } = new ThemeImage();
        public Color ToolbarBackgroundColor { get; set; } = Color.FromArgb(234, 234, 242);
        public ThemeImage ThumbnailBackgroundImage { get; set; } = new ThemeImage();
        public Color ThumbnailBackgroundColor { get; set; } = Color.FromArgb(234, 234, 242);
        public Color TextInfoColor { get; set; } = Color.Black;

        /// <summary>
        /// Toolbar Icon collection for the theme
        /// </summary>
        public ThemeIconCollection ToolbarIcons { get; set; } = new ThemeIconCollection();
        public bool IsThemeValid { get; set; } = true;

        public Theme()
        {

        }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+)
        /// </summary>
        /// <param name="file"></param>
        public Theme(string file)
        {
            this.IsThemeValid = LoadTheme(file);
        }


        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+). 
        /// Return TRUE if successful, FALSE if the theme format is invalid
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool LoadTheme(string file)
        {
            if (!File.Exists(file))
            {
                file = Path.Combine(GlobalSetting.StartUpDir, @"DefaultTheme\config.xml");
            }

            this._themeConfigFilePath = file;

            string dir = Path.GetDirectoryName(file);
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlElement root = doc.DocumentElement;
            XmlElement nType = null;
            XmlElement n = null;

            try
            {
                //Load theme version 1.5+ as default
                nType = (XmlElement)root.SelectNodes("Theme")[0]; //<Theme>
                n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>
            }
            catch
            {
                this.IsThemeValid = false;
            }

            //Get Scaling factor
            //double scaleFactor = DPIScaling.GetDPIScaleFactor();
            int iconHeight = ThemeImage.GetCorrectIconHeight(); //(int)((int)Constants.TOOLBAR_ICON_HEIGHT * scaleFactor);

            #region Theme <Info>
            try { name = n.GetAttribute("name"); }
            catch (Exception ex) { };
            try { version = n.GetAttribute("version"); }
            catch (Exception ex) { };
            try { author = n.GetAttribute("author"); }
            catch (Exception ex) { };
            try { email = n.GetAttribute("email"); }
            catch (Exception ex) { };
            try { website = n.GetAttribute("website"); }
            catch (Exception ex) { };
            try { description = n.GetAttribute("description"); }
            catch (Exception ex) { };
            try { type = n.GetAttribute("type"); }
            catch (Exception ex) { };
            try { compatibility = n.GetAttribute("compatibility"); }
            catch (Exception ex) { };
            #endregion

            #region Theme <main>
            try
            {
                var imgFile = Path.Combine(dir, n.GetAttribute("preview"));
                PreviewImage = new ThemeImage(imgFile);
            }
            catch (Exception ex) { };

            n = (XmlElement)nType.SelectNodes("main")[0]; //<main>

            try
            {
                var imgFile = Path.Combine(dir, n.GetAttribute("topbar"));
                ToolbarBackgroundImage = new ThemeImage(imgFile);
            }
            catch (Exception ex) { };

            try
            {
                ToolbarBackgroundColor = Color.FromArgb(int.Parse(n.GetAttribute("topbarcolor")));
            }
            catch (Exception ex) { };

            try
            {
                var imgFile = Path.Combine(dir, n.GetAttribute("bottombar"));
                ThumbnailBackgroundImage = new ThemeImage(imgFile);
            }
            catch (Exception ex) { };

            try
            {
                ThumbnailBackgroundColor = Color.FromArgb(int.Parse(n.GetAttribute("bottombarcolor")));
            }
            catch (Exception ex) { };

            try
            {
                BackgroundColor = Color.FromArgb(int.Parse(n.GetAttribute("backcolor")));
            }
            catch (Exception ex) { };

            try
            {
                TextInfoColor = Color.FromArgb(int.Parse(n.GetAttribute("statuscolor")));
            }
            catch (Exception ex) { };
            #endregion

            #region Theme <toolbar_icon>
            n = (XmlElement)nType.SelectNodes("toolbar_icon")[0]; //<toolbar_icon>
            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("back"));
                ToolbarIcons.ViewPreviousImage = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("next"));
                ToolbarIcons.ViewNextImage = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("leftrotate"));
                ToolbarIcons.RotateLeft = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("rightrotate"));
                ToolbarIcons.RotateRight = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomin"));
                ToolbarIcons.ZoomIn = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomout"));
                ToolbarIcons.ZoomOut = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomtofit"));
                ToolbarIcons.ZoomToFit = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletofit"));
                ToolbarIcons.ActualSize = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomlock"));
                ToolbarIcons.LockRatio = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletowidth"));
                ToolbarIcons.ScaleToWidth = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletoheight"));
                ToolbarIcons.ScaleToHeight = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("autosizewindow"));
                ToolbarIcons.AdjustWindowSize = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("open"));
                ToolbarIcons.OpenFile = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("refresh"));
                ToolbarIcons.Refresh = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("gotoimage"));
                ToolbarIcons.GoToImage = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("thumbnail"));
                ToolbarIcons.ThumbnailBar = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("checkedbackground"));
                ToolbarIcons.CheckedBackground = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("fullscreen"));
                ToolbarIcons.FullScreen = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("slideshow"));
                ToolbarIcons.Slideshow = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("convert"));
                ToolbarIcons.Convert = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("print"));
                ToolbarIcons.Print = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("uploadfb"));
                ToolbarIcons.Sharing = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("extension"));
                ToolbarIcons.Plugins = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("settings"));
                ToolbarIcons.Settings = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("about"));
                ToolbarIcons.About = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("menu"));
                ToolbarIcons.Menu = new ThemeImage(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };
            #endregion

            this.IsThemeValid = true;
            return this.IsThemeValid;
        }


        
        /// <summary>
        /// Save theme compatible with v1.5+
        /// </summary>
        /// <param name="dir"></param>
        public void SaveAsTheme(string dir)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");//<ImageGlass>
            XmlElement nType = doc.CreateElement("Theme");//<Theme>

            XmlElement n = doc.CreateElement("Info");// <Info>
            n.SetAttribute("name", name);
            n.SetAttribute("version", version);
            n.SetAttribute("author", author);
            n.SetAttribute("email", email);
            n.SetAttribute("website", website);
            n.SetAttribute("description", description);
            n.SetAttribute("type", "ImageGlass Theme Configuration");
            n.SetAttribute("compatibility", compatibility);
            n.SetAttribute("preview", Path.GetFileName(PreviewImage.Filename));
            nType.AppendChild(n);

            n = doc.CreateElement("main");// <main>
            n.SetAttribute("topbar", Path.GetFileName(ToolbarBackgroundImage.Filename));
            n.SetAttribute("topbarcolor", ToolbarBackgroundColor.ToArgb().ToString());
            n.SetAttribute("bottombar", Path.GetFileName(ThumbnailBackgroundImage.Filename));
            n.SetAttribute("bottombarcolor", ThumbnailBackgroundColor.ToArgb().ToString());
            n.SetAttribute("backcolor", BackgroundColor.ToArgb().ToString());
            n.SetAttribute("statuscolor", TextInfoColor.ToArgb().ToString());
            nType.AppendChild(n);

            n = doc.CreateElement("toolbar_icon");// <toolbar_icon>
            n.SetAttribute("back", Path.GetFileName(ToolbarIcons.ViewPreviousImage.Filename));
            n.SetAttribute("next", Path.GetFileName(ToolbarIcons.ViewNextImage.Filename));
            n.SetAttribute("leftrotate", Path.GetFileName(ToolbarIcons.RotateLeft.Filename));
            n.SetAttribute("rightrotate", Path.GetFileName(ToolbarIcons.RotateRight.Filename));
            n.SetAttribute("zoomin", Path.GetFileName(ToolbarIcons.ZoomIn.Filename));
            n.SetAttribute("zoomout", Path.GetFileName(ToolbarIcons.ZoomOut.Filename));
            n.SetAttribute("zoomtofit", Path.GetFileName(ToolbarIcons.ZoomToFit.Filename));
            n.SetAttribute("zoomlock", Path.GetFileName(ToolbarIcons.LockRatio.Filename));
            n.SetAttribute("scaletofit", Path.GetFileName(ToolbarIcons.ActualSize.Filename));
            n.SetAttribute("scaletowidth", Path.GetFileName(ToolbarIcons.ScaleToWidth.Filename));
            n.SetAttribute("scaletoheight", Path.GetFileName(ToolbarIcons.ScaleToHeight.Filename));
            n.SetAttribute("autosizewindow", Path.GetFileName(ToolbarIcons.AdjustWindowSize.Filename));
            n.SetAttribute("open", Path.GetFileName(ToolbarIcons.OpenFile.Filename));
            n.SetAttribute("refresh", Path.GetFileName(ToolbarIcons.Refresh.Filename));
            n.SetAttribute("gotoimage", Path.GetFileName(ToolbarIcons.GoToImage.Filename));
            n.SetAttribute("thumbnail", Path.GetFileName(ToolbarIcons.ThumbnailBar.Filename));
            n.SetAttribute("caro", Path.GetFileName(ToolbarIcons.CheckedBackground.Filename));
            n.SetAttribute("fullscreen", Path.GetFileName(ToolbarIcons.FullScreen.Filename));
            n.SetAttribute("slideshow", Path.GetFileName(ToolbarIcons.Slideshow.Filename));
            n.SetAttribute("convert", Path.GetFileName(ToolbarIcons.Convert.Filename));
            n.SetAttribute("print", Path.GetFileName(ToolbarIcons.Print.Filename));
            n.SetAttribute("uploadfb", Path.GetFileName(ToolbarIcons.Sharing.Filename));
            n.SetAttribute("extension", Path.GetFileName(ToolbarIcons.Plugins.Filename));
            n.SetAttribute("settings", Path.GetFileName(ToolbarIcons.Settings.Filename));
            n.SetAttribute("about", Path.GetFileName(ToolbarIcons.About.Filename));
            n.SetAttribute("menu", Path.GetFileName(ToolbarIcons.Menu.Filename));
            nType.AppendChild(n);

            root.AppendChild(nType);
            doc.AppendChild(root);

            //create temp directory of theme
            if (Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            doc.Save(Path.Combine(dir, "config.xml")); //save file
        }




        #region PRIVATE STATIC FUNCS
        private static ThemeInstallResult _extractThemeResult = ThemeInstallResult.UNKNOWN;

        private static ThemeInstallResult ExtractTheme(string themePath, string dir)
        {
            _extractThemeResult = ThemeInstallResult.UNKNOWN;

            try
            {
                using (ZipFile z = new ZipFile(themePath, Encoding.UTF8))
                {
                    z.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(z_ExtractProgress);
                    z.ZipError += new EventHandler<ZipErrorEventArgs>(z_ZipError);
                    z.ExtractAll(dir, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch
            {
                _extractThemeResult = ThemeInstallResult.ERROR;
            }

            while (_extractThemeResult == ThemeInstallResult.UNKNOWN)
            {
                Thread.Sleep(20);
            }

            return _extractThemeResult;
        }

        private static void z_ZipError(object sender, ZipErrorEventArgs e)
        {
            _extractThemeResult = ThemeInstallResult.ERROR;
        }

        private static void z_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EntriesExtracted == e.EntriesTotal)
            {
                _extractThemeResult = ThemeInstallResult.SUCCESS;
            }
        }
        #endregion




        #region PUBLIC STATIC FUNCS
        /// <summary>
        /// Apply the new theme and save configs
        /// </summary>
        /// <param name="themeConfigPath">Full path of config.xml</param>
        /// <returns>Return Theme object if success, else NULL</returns>
        public static Theme ApplyTheme(string themeConfigPath)
        {
            //Save background color
            try
            {
                Theme th = new Theme();

                if (th.LoadTheme(themeConfigPath))
                {
                    GlobalSetting.SetConfig("BackgroundColor", th.BackgroundColor.ToArgb().ToString(GlobalSetting.NumberFormat));

                    //Save theme path
                    GlobalSetting.SetConfig("Theme", themeConfigPath);

                    return th;
                }
            }
            catch { }

            return null;
        }


        /// <summary>
        /// Install ImageGlass theme
        /// </summary>
        /// <param name="themePath">Full path of *.igtheme</param>
        /// <returns></returns>
        public static ThemeInstallResult InstallTheme(string themePath)
        {
            if (!File.Exists(themePath))
            {
                return ThemeInstallResult.ERROR;
            }

            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass\Themes\");
            Directory.CreateDirectory(dir);

            return ExtractTheme(themePath, dir);
        }

        

        /// <summary>
        /// Uninstall ImageGlass theme pack
        /// </summary>
        /// <param name="themeConfigPath">Full path of config.xml</param>
        /// <returns></returns>
        public static ThemeUninstallResult UninstallTheme(string themeConfigPath)
        {
            if (File.Exists(themeConfigPath))
            {
                string dir = Path.GetDirectoryName(themeConfigPath);

                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    return ThemeUninstallResult.ERROR;
                }
            }
            else
            {
                return ThemeUninstallResult.ERROR_THEME_NOT_FOUND;
            }

            return ThemeUninstallResult.SUCCESS;
        }

        #endregion
    }
}
