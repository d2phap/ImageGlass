/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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

namespace ImageGlass.Theme
{
    public class Theme
    {
        //Info
        public string name = string.Empty;                  
        public string version = string.Empty;               
        public string author = string.Empty;                
        public string email = string.Empty;                 
        public string website = string.Empty;               
        public string description = string.Empty;           
        public string type = string.Empty;                     //config file type
        public string compatibility = string.Empty;            //minimum version requirement
        public string preview = string.Empty;                  //preview photo file

        //main
        public string topbar = string.Empty;                   
        public int topbartransparent = 0;                      //v1.2 only
        public Color backcolor = Color.White;                  
        public string bottombar = string.Empty;                 //v3.2-
        public Color bottomBarColor = Color.FromArgb(234, 234, 242);    //v3.3+
        public Color statuscolor = Color.Black;                
        
        /// <summary>
        /// Toolbar Icon collection for the theme
        /// </summary>
        public ThemeIconCollection ToolbarIcons { get; set; }


        public Theme()
        {
            ToolbarIcons = new ThemeIconCollection();
        }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+)
        /// </summary>
        /// <param name="file"></param>
        public Theme(string file)
        {
            ToolbarIcons = new ThemeIconCollection();

            LoadTheme(file);
        }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+). 
        /// Return TRUE if successful, FALSE if the theme format is invalid
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool LoadTheme(string file)
        {
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
                return false;
            }

            //Get Scaling factor
            double scaleFactor = DPIScaling.GetDPIScaleFactor();
            int iconHeight = (int)((int)Constants.TOOLBAR_ICON_HEIGHT * scaleFactor);

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
            try { preview = n.GetAttribute("preview"); }
            catch (Exception ex) { };

            n = (XmlElement)nType.SelectNodes("main")[0]; //<main>
            try { topbar = n.GetAttribute("topbar"); }
            catch (Exception ex) { };
            try { topbartransparent = int.Parse(n.GetAttribute("topbartransparent")); }
            catch (Exception ex) { };
            try { backcolor = Color.FromArgb(int.Parse(n.GetAttribute("backcolor"))); }
            catch (Exception ex) { };
            try { bottomBarColor = Color.FromArgb(int.Parse(n.GetAttribute("bottombarcolor"))); }
            catch (Exception ex) { };
            try { statuscolor = Color.FromArgb(int.Parse(n.GetAttribute("statuscolor"))); }
            catch (Exception ex) { };

            n = (XmlElement)nType.SelectNodes("toolbar_icon")[0]; //<toolbar_icon>
            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("back"));
                ToolbarIcons.ViewPreviousImage = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("next"));
                ToolbarIcons.ViewNextImage = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("leftrotate"));
                ToolbarIcons.RotateLeft = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("rightrotate"));
                ToolbarIcons.RotateRight = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomin"));
                ToolbarIcons.ZoomIn = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomout"));
                ToolbarIcons.ZoomOut = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletofit"));
                ToolbarIcons.ActualSize = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("zoomlock"));
                ToolbarIcons.LockRatio = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletowidth"));
                ToolbarIcons.ScaleToWidth = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("scaletoheight"));
                ToolbarIcons.ScaleToHeight = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("autosizewindow"));
                ToolbarIcons.AdjustWindowSize = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("open"));
                ToolbarIcons.OpenFile = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("refresh"));
                ToolbarIcons.Refresh = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("gotoimage"));
                ToolbarIcons.GoToImage = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("thumbnail"));
                ToolbarIcons.ThumbnailBar = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("checkedbackground"));
                ToolbarIcons.CheckedBackground = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("fullscreen"));
                ToolbarIcons.FullScreen = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("slideshow"));
                ToolbarIcons.Slideshow = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("convert"));
                ToolbarIcons.Convert = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("print"));
                ToolbarIcons.Print = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("uploadfb"));
                ToolbarIcons.Sharing = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("extension"));
                ToolbarIcons.Plugins = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("settings"));
                ToolbarIcons.Settings = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("about"));
                ToolbarIcons.About = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            try
            {
                var iconFile = Path.Combine(dir, n.GetAttribute("menu"));
                ToolbarIcons.Menu = new ThemeIcon(iconFile, iconHeight, iconHeight);
            }
            catch (Exception ex) { };

            return true;
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
            n.SetAttribute("preview", preview);
            nType.AppendChild(n);

            n = doc.CreateElement("main");// <main>
            n.SetAttribute("topbar", topbar);
            n.SetAttribute("topbartransparent", "0");
            n.SetAttribute("bottombar", bottombar);
            n.SetAttribute("backcolor", backcolor.ToArgb().ToString());
            n.SetAttribute("statuscolor", statuscolor.ToArgb().ToString());
            nType.AppendChild(n);

            n = doc.CreateElement("toolbar_icon");// <toolbar_icon>
            n.SetAttribute("back", Path.GetFileName(ToolbarIcons.ViewPreviousImage.Filename));
            n.SetAttribute("next", Path.GetFileName(ToolbarIcons.ViewNextImage.Filename));
            n.SetAttribute("leftrotate", Path.GetFileName(ToolbarIcons.RotateLeft.Filename));
            n.SetAttribute("rightrotate", Path.GetFileName(ToolbarIcons.RotateRight.Filename));
            n.SetAttribute("zoomin", Path.GetFileName(ToolbarIcons.ZoomIn.Filename));
            n.SetAttribute("zoomout", Path.GetFileName(ToolbarIcons.ZoomOut.Filename));
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

        /// <summary>
        /// Apply the new theme
        /// </summary>
        /// <param name="themePath">Full path of *.igtheme</param>
        public void ApplyTheme(string themePath)
        {
            //Save theme path
            GlobalSetting.SetConfig("Theme", themePath);

            //Save background color
            try
            {
                Theme th = new Theme(themePath);
                GlobalSetting.SetConfig("BackgroundColor", th.backcolor.ToArgb().ToString());
            }
            catch
            {
                GlobalSetting.SetConfig("BackgroundColor", Color.White.ToArgb().ToString());
            }
        }


    }
}
