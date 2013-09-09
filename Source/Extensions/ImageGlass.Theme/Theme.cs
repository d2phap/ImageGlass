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

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using ImageGlass.Services.Configuration;


namespace ImageGlass.Theme
{
    public class Theme
    {
        //Info
        public string name = string.Empty;                     //tên của theme
        public string version = string.Empty;                  //phiên bản của theme
        public string author = string.Empty;                   //tác giả
        public string email = string.Empty;                    //email
        public string website = string.Empty;                  //website
        public string description = string.Empty;              //miêu tả
        public string type = string.Empty;                     //loại tập tin thiết lập
        public string compatibility = string.Empty;            //yêu cầu tương thích với phiên bản thấp nhất
        public string preview = string.Empty;                  //ảnh thu nhỏ của theme

        //main
        public string topbar = string.Empty;                   //ảnh nền thanh công cụ
        public int topbartransparent = 0;                      //0 - không aero glass, 1 - có aero glass
        public Color backcolor = Color.White;                  //màu nền vùng xem ảnh
        public string bottombar = string.Empty;                //ảnh nền thanh thumbnail
        public Color statuscolor = Color.Black;                //màu chữ của thông tin hình ảnh

        //toolbar icon
        public string back = string.Empty;
        public string next = string.Empty;
        public string leftrotate = string.Empty;
        public string rightrotate = string.Empty;
        public string zoomin = string.Empty;
        public string zoomout = string.Empty;
        public string scaletofit = string.Empty;
        public string zoomlock = string.Empty;                  //v1.5+ only
        public string scaletowidth = string.Empty;
        public string scaletoheight = string.Empty;
        public string autosizewindow = string.Empty;
        public string open = string.Empty;
        public string refresh = string.Empty;
        public string gotoimage = string.Empty;
        public string thumbnail = string.Empty;
        public string caro = string.Empty;
        public string fullscreen = string.Empty;
        public string slideshow = string.Empty;
        public string convert = string.Empty;
        public string print = string.Empty;                     //v1.5+ only
        public string uploadfb = string.Empty;                  //v1.5+ only
        public string extension = string.Empty;                 //v1.5+ only
        public string settings = string.Empty;
        public string about = string.Empty;
        public string like = string.Empty;
        public string dislike = string.Empty;
        public string report = string.Empty;

        public Theme() { }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+)
        /// </summary>
        /// <param name="file"></param>
        public Theme(string file)
        {
            LoadTheme(file);
        }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+). 
        /// Return TRUE if sucessful, FALSE if the theme is older version
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool LoadTheme(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlElement root = doc.DocumentElement;
            XmlElement nType = null;
            XmlElement n = null;

            try
            {
                //Load theme version 1.5 as default
                nType = (XmlElement)root.SelectNodes("Theme")[0]; //<Theme>
                n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>
            }
            catch
            {
                LoadThemeOldVersion(file);
                return false;
            }

            try { name = n.GetAttribute("name"); }
            catch { };
            try { version = n.GetAttribute("version"); }
            catch { };
            try { author = n.GetAttribute("author"); }
            catch { };
            try { email = n.GetAttribute("email"); }
            catch { };
            try { website = n.GetAttribute("website"); }
            catch { };
            try { description = n.GetAttribute("description"); }
            catch { };
            try { type = n.GetAttribute("type"); }
            catch { };
            try { compatibility = n.GetAttribute("compatibility"); }
            catch { };
            try { preview = n.GetAttribute("preview"); }
            catch { };

            n = (XmlElement)nType.SelectNodes("main")[0]; //đọc thẻ <main>
            try { topbar = n.GetAttribute("topbar"); }
            catch { };
            try { topbartransparent = int.Parse(n.GetAttribute("topbartransparent")); }
            catch { };
            try { backcolor = Color.FromArgb(int.Parse(n.GetAttribute("backcolor"))); }
            catch { };
            try { bottombar = n.GetAttribute("bottombar"); }
            catch { };
            try { statuscolor = Color.FromArgb(int.Parse(n.GetAttribute("statuscolor"))); }
            catch { };

            n = (XmlElement)nType.SelectNodes("toolbar_icon")[0]; //đọc thẻ <toolbar_icon>
            try { back = n.GetAttribute("back"); }
            catch { };
            try { next = n.GetAttribute("next"); }
            catch { };
            try { leftrotate = n.GetAttribute("leftrotate"); }
            catch { };
            try { rightrotate = n.GetAttribute("rightrotate"); }
            catch { };
            try { zoomin = n.GetAttribute("zoomin"); }
            catch { };
            try { zoomout = n.GetAttribute("zoomout"); }
            catch { };
            try { scaletofit = n.GetAttribute("scaletofit"); }
            catch { };
            try { zoomlock = n.GetAttribute("zoomlock"); }
            catch { };
            try { scaletowidth = n.GetAttribute("scaletowidth"); }
            catch { };
            try { scaletoheight = n.GetAttribute("scaletoheight"); }
            catch { };
            try { autosizewindow = n.GetAttribute("autosizewindow"); }
            catch { };
            try { open = n.GetAttribute("open"); }
            catch { };
            try { refresh = n.GetAttribute("refresh"); }
            catch { };
            try { gotoimage = n.GetAttribute("gotoimage"); }
            catch { };
            try { thumbnail = n.GetAttribute("thumbnail"); }
            catch { };
            try { caro = n.GetAttribute("caro"); }
            catch { };
            try { fullscreen = n.GetAttribute("fullscreen"); }
            catch { };
            try { slideshow = n.GetAttribute("slideshow"); }
            catch { };
            try { convert = n.GetAttribute("convert"); }
            catch { };
            try { print = n.GetAttribute("print"); }
            catch { };
            try { uploadfb = n.GetAttribute("uploadfb"); }
            catch { };
            try { extension = n.GetAttribute("extension"); }
            catch { };
            try { settings = n.GetAttribute("settings"); }
            catch { };
            try { about = n.GetAttribute("about"); }
            catch { };
            try { like = n.GetAttribute("like"); }
            catch { };
            try { dislike = n.GetAttribute("dislike"); }
            catch { };
            try { report = n.GetAttribute("report"); }
            catch { };

            return true;
        }

        /// <summary>
        /// Read theme data from theme configuration file (Version 1.4)
        /// </summary>
        /// <param name="file"></param>
        public void LoadThemeOldVersion(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlElement root = doc.DocumentElement;//<data>
            XmlElement n = (XmlElement)root.SelectNodes("metadata")[0];//<metadata>

            try { name = n.GetAttribute("name"); }
            catch { };
            try { version = n.GetAttribute("version"); }
            catch { };
            try { author = n.GetAttribute("author"); }
            catch { };
            try { email = n.GetAttribute("email"); }
            catch { };
            try { website = n.GetAttribute("website"); }
            catch { };
            try { description = n.GetAttribute("description"); }
            catch { };
            try { type = n.GetAttribute("type"); }
            catch { };
            try { compatibility = n.GetAttribute("compatibility"); }
            catch { };
            try { preview = n.GetAttribute("preview"); }
            catch { };

            n = (XmlElement)root.SelectNodes("main")[0]; //đọc thẻ <main>
            try { topbar = n.GetAttribute("topbar"); }
            catch { };
            try { topbartransparent = int.Parse(n.GetAttribute("topbartransparent")); }
            catch { };
            try { backcolor = Color.FromArgb(int.Parse(n.GetAttribute("backcolor"))); }
            catch { };
            try { bottombar = n.GetAttribute("bottombar"); }
            catch { };
            try { statuscolor = Color.FromArgb(int.Parse(n.GetAttribute("statuscolor"))); }
            catch { };

            n = (XmlElement)root.SelectNodes("toolbar_icon")[0]; //đọc thẻ <toolbar_icon>
            try { back = n.GetAttribute("back"); }
            catch { };
            try { next = n.GetAttribute("next"); }
            catch { };
            try { leftrotate = n.GetAttribute("leftrotate"); }
            catch { };
            try { rightrotate = n.GetAttribute("rightrotate"); }
            catch { };
            try { zoomin = n.GetAttribute("zoomin"); }
            catch { };
            try { zoomout = n.GetAttribute("zoomout"); }
            catch { };
            try { scaletofit = n.GetAttribute("scaletofit"); }
            catch { };
            try { zoomlock = n.GetAttribute("zoomlock"); }
            catch { };
            try { scaletowidth = n.GetAttribute("scaletowidth"); }
            catch { };
            try { scaletoheight = n.GetAttribute("scaletoheight"); }
            catch { };
            try { autosizewindow = n.GetAttribute("autosizewindow"); }
            catch { };
            try { open = n.GetAttribute("open"); }
            catch { };
            try { refresh = n.GetAttribute("refresh"); }
            catch { };
            try { gotoimage = n.GetAttribute("gotoimage"); }
            catch { };
            try { thumbnail = n.GetAttribute("thumbnail"); }
            catch { };
            try { caro = n.GetAttribute("caro"); }
            catch { };
            try { fullscreen = n.GetAttribute("fullscreen"); }
            catch { };
            try { slideshow = n.GetAttribute("slideshow"); }
            catch { };
            try { convert = n.GetAttribute("convert"); }
            catch { };
            try { print = n.GetAttribute("print"); }
            catch { };
            try { uploadfb = n.GetAttribute("uploadfb"); }
            catch { };
            try { extension = n.GetAttribute("extension"); }
            catch { };
            try { settings = n.GetAttribute("settings"); }
            catch { };
            try { about = n.GetAttribute("about"); }
            catch { };
            try { like = n.GetAttribute("like"); }
            catch { };
            try { dislike = n.GetAttribute("dislike"); }
            catch { };
            try { report = n.GetAttribute("report"); }
            catch { };

        }

        /// <summary>
        /// Save theme compatible with v1.5
        /// </summary>
        /// <param name="dir"></param>
        public void SaveAsTheme(string dir)
        {
            dir = (dir + "\\").Replace("\\\\", "\\");
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");//<ImageGlass>
            XmlElement nType = doc.CreateElement("Theme");//<Theme>

            XmlElement n = doc.CreateElement("Info");// <Info>
            n.SetAttribute("name", this.name);
            n.SetAttribute("version", this.version);
            n.SetAttribute("author", this.author);
            n.SetAttribute("email", this.email);
            n.SetAttribute("website", this.website);
            n.SetAttribute("description", this.description);
            n.SetAttribute("type", "ImageGlass Theme Configuration");
            n.SetAttribute("compatibility", this.compatibility);
            n.SetAttribute("preview", this.preview);
            nType.AppendChild(n);

            n = doc.CreateElement("main");// <main>
            n.SetAttribute("topbar", this.topbar);
            n.SetAttribute("topbartransparent", "0");
            n.SetAttribute("bottombar", this.bottombar);
            n.SetAttribute("backcolor", this.backcolor.ToArgb().ToString());
            n.SetAttribute("statuscolor", this.statuscolor.ToArgb().ToString());
            nType.AppendChild(n);

            n = doc.CreateElement("toolbar_icon");// <toolbar_icon>
            n.SetAttribute("back", this.back);
            n.SetAttribute("next", this.next);
            n.SetAttribute("leftrotate", this.leftrotate);
            n.SetAttribute("rightrotate", this.rightrotate);
            n.SetAttribute("zoomin", this.zoomin);
            n.SetAttribute("zoomout", this.zoomout);
            n.SetAttribute("zoomlock", this.zoomlock);
            n.SetAttribute("scaletofit", this.scaletofit);
            n.SetAttribute("scaletowidth", this.scaletowidth);
            n.SetAttribute("scaletoheight", this.scaletoheight);
            n.SetAttribute("autosizewindow", this.autosizewindow);
            n.SetAttribute("open", this.open);
            n.SetAttribute("refresh", this.refresh);
            n.SetAttribute("gotoimage", this.gotoimage);
            n.SetAttribute("thumbnail", this.thumbnail);
            n.SetAttribute("caro", this.caro);
            n.SetAttribute("fullscreen", this.fullscreen);
            n.SetAttribute("slideshow", this.slideshow);
            n.SetAttribute("convert", this.convert);
            n.SetAttribute("print", this.print);
            n.SetAttribute("uploadfb", this.uploadfb);
            n.SetAttribute("extension", this.extension);
            n.SetAttribute("settings", this.settings);
            n.SetAttribute("about", this.about);
            n.SetAttribute("like", this.like);
            n.SetAttribute("dislike", this.dislike);
            n.SetAttribute("report", this.report);
            nType.AppendChild(n);

            root.AppendChild(nType);
            doc.AppendChild(root);

            //create temp directory of theme
            if (Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            doc.Save(dir + "config.xml"); //save file
        }

        /// <summary>
        /// Áp dụng giao diện mới
        /// </summary>
        /// <param name="themePath">Đường dẫn đầy đủ của *.igtheme</param>
        public void ApplyTheme(string themePath)
        {
            //Lưu đường dẫn theme
            GlobalSetting.SetConfig("Theme", themePath);

            //Lưu màu nền
            try
            {
                ImageGlass.Theme.Theme th = new ImageGlass.Theme.Theme(themePath);
                GlobalSetting.SetConfig("BackgroundColor", th.backcolor.ToArgb().ToString());
            }
            catch
            {
                GlobalSetting.SetConfig("BackgroundColor", Color.White.ToArgb().ToString());
            }
            
        }


    }
}
