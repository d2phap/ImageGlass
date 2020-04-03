/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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

using ImageGlass.Base;
using Ionic.Zip;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace ImageGlass.UI {
    public class Theme {
        #region PUBLIC PROPERTIES

        /// <summary>
        /// Get the name of theme folder
        /// </summary>
        public string FolderName { get; internal set; }

        /// <summary>
        /// Get theme config file path (config.xml)
        /// </summary>
        public string ConfigFilePath { get; internal set; }

        /// <summary>
        /// Check if this theme is valid
        /// </summary>
        public bool IsValid { get; internal set; }

        #endregion


        #region THEME NODE PROPERTIES

        #region <INFO> node

        /// <summary>
        /// Theme name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Theme version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Author's information
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Author's email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Author's website
        /// </summary>
        public string Website { get; set; } = string.Empty;

        /// <summary>
        /// Theme file description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Theme Config file type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Minimum version of this theme work with
        /// </summary>
        public string Compatibility { get; set; } = string.Empty;
        #endregion


        #region <MAIN> node

        /// <summary>
        /// The preview image of the theme
        /// </summary>
        public ThemeImage PreviewImage { get; set; } = new ThemeImage();

        /// <summary>
        /// Theme background color
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.White;

        /// <summary>
        /// Toolbar background image
        /// </summary>
        public ThemeImage ToolbarBackgroundImage { get; set; } = new ThemeImage();

        /// <summary>
        /// Toolbar background color
        /// </summary>
        public Color ToolbarBackgroundColor { get; set; } = Color.FromArgb(234, 234, 242);

        /// <summary>
        /// Thumbnail bar background image
        /// </summary>
        public ThemeImage ThumbnailBackgroundImage { get; set; } = new ThemeImage();

        /// <summary>
        /// Thumbnail bar background color
        /// </summary>
        public Color ThumbnailBackgroundColor { get; set; } = Color.FromArgb(234, 234, 242);

        /// <summary>
        /// Text color
        /// </summary>
        public Color TextInfoColor { get; set; } = Color.Black;


        /// <summary>
        /// Menu background color
        /// </summary>
        public Color MenuBackgroundColor { get; set; } = Color.White;


        /// <summary>
        /// Menu text color
        /// </summary>
        public Color MenuTextColor { get; set; } = Color.Black;


        /// <summary>
        /// The multiplier which impacts the size of the navigation arrows.
        /// </summary>
        public double NavArrowMultiplier { get; set; } = 2.0;

        #endregion


        #region <TOOLBAR_ICON> node

        /// <summary>
        /// Toolbar Icon collection for the theme
        /// </summary>
        public ThemeIconCollection ToolbarIcons { get; set; } = new ThemeIconCollection();
        #endregion


        #region Arrow Cursors
        public Cursor PreviousArrowCursor { get; set; }

        public Cursor NextArrowCursor { get; set; }

        public Image PreviousNavArrow { get; set; }
        public Image NextNavArrow { get; set; }

        #endregion

        #endregion



        /// <summary>
        /// Initiate theme object with configuration file (Version 1.5+)
        /// </summary>
        /// <param name="themeFolderPath">The absolute path of theme folder.</param>
        public Theme(string themeFolderPath = "") {
            this.IsValid = LoadTheme(themeFolderPath);
        }



        #region PUBLIC CLASS FUNCS

        /// <summary>
        /// Common logic to load an image from a theme config file.
        /// </summary>
        /// <param name="dir">path to folder containing theme files</param>
        /// <param name="n">XMLElement to pull theme filename attribute from</param>
        /// <param name="attribname">name of theme attribute</param>
        /// <param name="iconHeight">optional target height/width</param>
        /// <returns></returns>
        private ThemeImage LoadThemeImage(string dir, XmlElement n, string attribname, int iconHeight = 0) {
            try {
                var attrib = n.GetAttribute(attribname);

                if (string.IsNullOrEmpty(attrib)) // KBR 20180827 avoid throwing exception
                    return new ThemeImage(""); // KBR 20180827 code in frmMain assumes not null

                var imgFile = Path.Combine(dir, attrib);


                if (iconHeight > 0) {
                    return new ThemeImage(imgFile, iconHeight);
                }

                return new ThemeImage(imgFile);
            }
            catch {
                return new ThemeImage(""); // KBR 20180827 code in frmMain assumes not null
            }
        }


        /// <summary>
        /// Reload the image icons to adapt DPI changes
        /// </summary>
        public void ReloadIcons() {
            ToolbarIcons.ViewPreviousImage.Refresh();
            ToolbarIcons.ViewNextImage.Refresh();
            ToolbarIcons.RotateLeft.Refresh();
            ToolbarIcons.RotateRight.Refresh();
            ToolbarIcons.FlipHorz.Refresh();
            ToolbarIcons.FlipVert.Refresh();
            ToolbarIcons.Delete.Refresh();
            ToolbarIcons.Edit.Refresh();
            ToolbarIcons.ZoomIn.Refresh();
            ToolbarIcons.ZoomOut.Refresh();
            ToolbarIcons.ScaleToFit.Refresh();
            ToolbarIcons.ActualSize.Refresh();
            ToolbarIcons.LockRatio.Refresh();
            ToolbarIcons.AutoZoom.Refresh();
            ToolbarIcons.ScaleToWidth.Refresh();
            ToolbarIcons.ScaleToHeight.Refresh();
            ToolbarIcons.ScaleToFill.Refresh();
            ToolbarIcons.AdjustWindowSize.Refresh();
            ToolbarIcons.OpenFile.Refresh();
            ToolbarIcons.Refresh.Refresh();
            ToolbarIcons.GoToImage.Refresh();
            ToolbarIcons.ThumbnailBar.Refresh();
            ToolbarIcons.Checkerboard.Refresh();
            ToolbarIcons.FullScreen.Refresh();
            ToolbarIcons.Slideshow.Refresh();
            ToolbarIcons.Convert.Refresh();
            ToolbarIcons.Print.Refresh();
            ToolbarIcons.Settings.Refresh();
            ToolbarIcons.About.Refresh();
            ToolbarIcons.Menu.Refresh();
            ToolbarIcons.ViewFirstImage.Refresh();
            ToolbarIcons.ViewLastImage.Refresh();


            #region Arrow cursors (derived from toolbar)

            var arrowHeight = (int)(DPIScaling.Transform(Constants.TOOLBAR_ICON_HEIGHT) * NavArrowMultiplier);

            var prevImage = new ThemeImage(ToolbarIcons.ViewPreviousImage.Filename, arrowHeight);
            var icon = prevImage.Image.GetHicon();
            PreviousArrowCursor = new Cursor(icon);
            PreviousNavArrow = prevImage.Image;

            var nextImage = new ThemeImage(ToolbarIcons.ViewNextImage.Filename, arrowHeight);
            icon = nextImage.Image.GetHicon();
            NextArrowCursor = new Cursor(icon);
            NextNavArrow = nextImage.Image;

            #endregion
        }


        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+). 
        /// Return TRUE if successful, FALSE if the theme format is invalid
        /// </summary>
        /// <param name="themeFolderPath">The absolute path of theme folder.</param>
        /// <param name="startUpDir">The absolute startup folder of ImageGlass</param>
        /// <returns></returns>
        public bool LoadTheme(string themeFolderPath) {
            var configFilePath = Path.Combine(themeFolderPath, "config.xml");

            if (!File.Exists(configFilePath)) {
                configFilePath = App.StartUpDir(Dir.DefaultTheme, "config.xml");
            }

            this.ConfigFilePath = configFilePath;
            this.FolderName = Path.GetFileName(themeFolderPath); // get folder name

            var dir = Path.GetDirectoryName(configFilePath);
            var doc = new XmlDocument();
            doc.Load(configFilePath);

            var root = doc.DocumentElement;
            XmlElement nType = null;
            XmlElement n = null;

            try {
                //Load theme version 1.5+ as default
                nType = (XmlElement)root.SelectNodes("Theme")[0]; //<Theme>
                n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>
            }
            catch {
                this.IsValid = false;
            }


            #region Theme <Info>
            try { Name = n.GetAttribute("name"); }
            catch { };
            try { Version = n.GetAttribute("version"); }
            catch { };
            try { Author = n.GetAttribute("author"); }
            catch { };
            try { Email = n.GetAttribute("email"); }
            catch { };
            try { Website = n.GetAttribute("website"); }
            catch { };
            try { Description = n.GetAttribute("description"); }
            catch { };
            try { Type = n.GetAttribute("type"); }
            catch { };
            try { Compatibility = n.GetAttribute("compatibility"); }
            catch { };
            #endregion


            #region Theme <main>
            PreviewImage = LoadThemeImage(dir, n, "preview");

            n = (XmlElement)nType.SelectNodes("main")[0]; //<main>

            ToolbarBackgroundImage = LoadThemeImage(dir, n, "topbar");

            var color = FetchColorAttribute(n, "topbarcolor");
            if (color != Color.Transparent)
                ToolbarBackgroundColor = color;

            ThumbnailBackgroundImage = LoadThemeImage(dir, n, "bottombar");

            color = FetchColorAttribute(n, "bottombarcolor");
            if (color != Color.Transparent)
                ThumbnailBackgroundColor = color;

            color = FetchColorAttribute(n, "backcolor");
            if (color != Color.Transparent)
                BackgroundColor = color;

            color = FetchColorAttribute(n, "statuscolor");
            if (color != Color.Transparent)
                TextInfoColor = color;

            color = FetchColorAttribute(n, "menubackgroundcolor");
            if (color != Color.Transparent)
                MenuBackgroundColor = color;

            color = FetchColorAttribute(n, "menutextcolor");
            if (color != Color.Transparent)
                MenuTextColor = color;


            // For 7.6: add ability to control the size of the navigation arrows
            // Minimum value is 1.0, default is 2.0.
            try {
                var colorString = n.GetAttribute("navarrowsize");
                if (!string.IsNullOrWhiteSpace(colorString)) {
                    if (!double.TryParse(colorString, out var val))
                        val = 2.0;
                    val = Math.Max(val, 1.0);

                    NavArrowMultiplier = val;
                }
            }
            catch (Exception ex) { };

            #endregion


            #region Theme <toolbar_icon>
            n = (XmlElement)nType.SelectNodes("toolbar_icon")[0]; //<toolbar_icon>

            ToolbarIcons.ViewPreviousImage = LoadThemeImage(dir, n, "back");
            ToolbarIcons.ViewNextImage = LoadThemeImage(dir, n, "next");
            ToolbarIcons.RotateLeft = LoadThemeImage(dir, n, "leftrotate");
            ToolbarIcons.RotateRight = LoadThemeImage(dir, n, "rightrotate");
            ToolbarIcons.FlipHorz = LoadThemeImage(dir, n, "fliphorz");
            ToolbarIcons.FlipVert = LoadThemeImage(dir, n, "flipvert");
            ToolbarIcons.Delete = LoadThemeImage(dir, n, "delete");
            ToolbarIcons.Edit = LoadThemeImage(dir, n, "edit");
            ToolbarIcons.ZoomIn = LoadThemeImage(dir, n, "zoomin");
            ToolbarIcons.ZoomOut = LoadThemeImage(dir, n, "zoomout");
            ToolbarIcons.ScaleToFit = LoadThemeImage(dir, n, "zoomtofit");
            ToolbarIcons.ActualSize = LoadThemeImage(dir, n, "scaletofit");
            ToolbarIcons.LockRatio = LoadThemeImage(dir, n, "zoomlock");
            ToolbarIcons.AutoZoom = LoadThemeImage(dir, n, "autozoom");
            ToolbarIcons.ScaleToWidth = LoadThemeImage(dir, n, "scaletowidth");
            ToolbarIcons.ScaleToHeight = LoadThemeImage(dir, n, "scaletoheight");
            ToolbarIcons.ScaleToFill = LoadThemeImage(dir, n, "scaletofill");
            ToolbarIcons.AdjustWindowSize = LoadThemeImage(dir, n, "autosizewindow");
            ToolbarIcons.OpenFile = LoadThemeImage(dir, n, "open");
            ToolbarIcons.Refresh = LoadThemeImage(dir, n, "refresh");
            ToolbarIcons.GoToImage = LoadThemeImage(dir, n, "gotoimage");
            ToolbarIcons.ThumbnailBar = LoadThemeImage(dir, n, "thumbnail");
            ToolbarIcons.Checkerboard = LoadThemeImage(dir, n, "checkerboard");
            ToolbarIcons.FullScreen = LoadThemeImage(dir, n, "fullscreen");
            ToolbarIcons.Slideshow = LoadThemeImage(dir, n, "slideshow");
            ToolbarIcons.Convert = LoadThemeImage(dir, n, "convert");
            ToolbarIcons.Print = LoadThemeImage(dir, n, "print");
            ToolbarIcons.Settings = LoadThemeImage(dir, n, "settings");
            ToolbarIcons.About = LoadThemeImage(dir, n, "about");
            ToolbarIcons.Menu = LoadThemeImage(dir, n, "menu");
            ToolbarIcons.ViewFirstImage = LoadThemeImage(dir, n, "gofirst");
            ToolbarIcons.ViewLastImage = LoadThemeImage(dir, n, "golast");
            #endregion


            #region Arrow cursors (derived from toolbar)

            var arrowHeight = (int)(DPIScaling.Transform(Constants.TOOLBAR_ICON_HEIGHT) * NavArrowMultiplier);
            var prevImage = LoadThemeImage(dir, n, "back", arrowHeight);
            var icon = prevImage.Image.GetHicon();
            PreviousArrowCursor = new Cursor(icon);

            var nextImage = LoadThemeImage(dir, n, "next", arrowHeight);
            icon = nextImage.Image.GetHicon();
            NextArrowCursor = new Cursor(icon);

            #endregion


            this.IsValid = true;
            return this.IsValid;


            //
            // Fetch a color attribute value from the theme config file.
            // Returns: a Color value if valid; Color.Transparent if an error
            //
            Color FetchColorAttribute(XmlElement xmlElement, string attribute) {
                try {
                    var colorString = xmlElement.GetAttribute(attribute);

                    if (IsValidHex(colorString)) {
                        return ConvertHexStringToColor(colorString, true);
                    }

                    return Color.FromArgb(255, Color.FromArgb(int.Parse(colorString)));
                }
                catch {
                    // ignored
                }

                return Color.Transparent;
            }
        }



        /// <summary>
        /// Save as the new theme config file, compatible with v5.0+
        /// </summary>
        /// <param name="dir"></param>
        public void SaveAsThemeConfigs(string dir) {
            Compatibility = "5.0";

            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");//<ImageGlass>
            XmlElement nType = doc.CreateElement("Theme");//<Theme>

            XmlElement n = doc.CreateElement("Info");// <Info>
            n.SetAttribute("name", Name);
            n.SetAttribute("version", Version);
            n.SetAttribute("author", Author);
            n.SetAttribute("email", Email);
            n.SetAttribute("website", Website);
            n.SetAttribute("description", Description);
            n.SetAttribute("type", "ImageGlass Theme Configuration");
            n.SetAttribute("compatibility", Compatibility);
            n.SetAttribute("preview", Path.GetFileName(PreviewImage.Filename));
            nType.AppendChild(n);

            n = doc.CreateElement("main");// <main>
            n.SetAttribute("topbar", Path.GetFileName(ToolbarBackgroundImage.Filename));
            n.SetAttribute("topbarcolor", ConvertColorToHEX(ToolbarBackgroundColor, true));
            n.SetAttribute("bottombar", Path.GetFileName(ThumbnailBackgroundImage.Filename));
            n.SetAttribute("bottombarcolor", ConvertColorToHEX(ThumbnailBackgroundColor, true));
            n.SetAttribute("backcolor", ConvertColorToHEX(BackgroundColor, true));
            n.SetAttribute("statuscolor", ConvertColorToHEX(TextInfoColor, true));
            n.SetAttribute("menubackgroundcolor", ConvertColorToHEX(this.MenuBackgroundColor, true));
            n.SetAttribute("menutextcolor", ConvertColorToHEX(this.MenuTextColor, true));
            nType.AppendChild(n);

            n = doc.CreateElement("toolbar_icon");// <toolbar_icon>
            n.SetAttribute("back", Path.GetFileName(ToolbarIcons.ViewPreviousImage.Filename));
            n.SetAttribute("next", Path.GetFileName(ToolbarIcons.ViewNextImage.Filename));
            n.SetAttribute("leftrotate", Path.GetFileName(ToolbarIcons.RotateLeft.Filename));
            n.SetAttribute("rightrotate", Path.GetFileName(ToolbarIcons.RotateRight.Filename));
            n.SetAttribute("fliphorz", Path.GetFileName(ToolbarIcons.FlipHorz.Filename));
            n.SetAttribute("flipvert", Path.GetFileName(ToolbarIcons.FlipVert.Filename));
            n.SetAttribute("delete", Path.GetFileName(ToolbarIcons.Delete.Filename));
            n.SetAttribute("edit", Path.GetFileName(ToolbarIcons.Edit.Filename));
            n.SetAttribute("zoomin", Path.GetFileName(ToolbarIcons.ZoomIn.Filename));
            n.SetAttribute("zoomout", Path.GetFileName(ToolbarIcons.ZoomOut.Filename));
            n.SetAttribute("zoomtofit", Path.GetFileName(ToolbarIcons.ScaleToFit.Filename));
            n.SetAttribute("zoomlock", Path.GetFileName(ToolbarIcons.LockRatio.Filename));
            n.SetAttribute("autozoom", Path.GetFileName(ToolbarIcons.AutoZoom.Filename));
            n.SetAttribute("scaletofit", Path.GetFileName(ToolbarIcons.ActualSize.Filename));
            n.SetAttribute("scaletowidth", Path.GetFileName(ToolbarIcons.ScaleToWidth.Filename));
            n.SetAttribute("scaletoheight", Path.GetFileName(ToolbarIcons.ScaleToHeight.Filename));
            n.SetAttribute("scaletofill", Path.GetFileName(ToolbarIcons.ScaleToFill.Filename));
            n.SetAttribute("autosizewindow", Path.GetFileName(ToolbarIcons.AdjustWindowSize.Filename));
            n.SetAttribute("open", Path.GetFileName(ToolbarIcons.OpenFile.Filename));
            n.SetAttribute("refresh", Path.GetFileName(ToolbarIcons.Refresh.Filename));
            n.SetAttribute("gotoimage", Path.GetFileName(ToolbarIcons.GoToImage.Filename));
            n.SetAttribute("thumbnail", Path.GetFileName(ToolbarIcons.ThumbnailBar.Filename));
            n.SetAttribute("checkerboard", Path.GetFileName(ToolbarIcons.Checkerboard.Filename));
            n.SetAttribute("fullscreen", Path.GetFileName(ToolbarIcons.FullScreen.Filename));
            n.SetAttribute("slideshow", Path.GetFileName(ToolbarIcons.Slideshow.Filename));
            n.SetAttribute("convert", Path.GetFileName(ToolbarIcons.Convert.Filename));
            n.SetAttribute("print", Path.GetFileName(ToolbarIcons.Print.Filename));
            n.SetAttribute("uploadfb", Path.GetFileName(ToolbarIcons.Sharing.Filename));
            n.SetAttribute("extension", Path.GetFileName(ToolbarIcons.Plugins.Filename));
            n.SetAttribute("settings", Path.GetFileName(ToolbarIcons.Settings.Filename));
            n.SetAttribute("about", Path.GetFileName(ToolbarIcons.About.Filename));
            n.SetAttribute("menu", Path.GetFileName(ToolbarIcons.Menu.Filename));

            n.SetAttribute("double-left-chevron", Path.GetFileName(ToolbarIcons.ViewFirstImage.Filename));
            n.SetAttribute("double-right-chevron", Path.GetFileName(ToolbarIcons.ViewLastImage.Filename));
            nType.AppendChild(n);

            root.AppendChild(nType);
            doc.AppendChild(root);

            //create temp directory of theme
            if (Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            doc.Save(Path.Combine(dir, "config.xml")); //save file
        }


        #endregion



        #region PRIVATE STATIC FUNCS
        private static ThemeInstallingResult _extractThemeResult = ThemeInstallingResult.UNKNOWN;

        private static ThemeInstallingResult ExtractTheme(string themePath, string dir) {
            _extractThemeResult = ThemeInstallingResult.UNKNOWN;

            try {
                using (ZipFile z = new ZipFile(themePath, Encoding.UTF8)) {
                    z.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(z_ExtractProgress);
                    z.ZipError += new EventHandler<ZipErrorEventArgs>(z_ZipError);
                    z.ExtractAll(dir, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch {
                _extractThemeResult = ThemeInstallingResult.ERROR;
            }

            while (_extractThemeResult == ThemeInstallingResult.UNKNOWN) {
                Thread.Sleep(20);
            }

            return _extractThemeResult;
        }

        private static void z_ZipError(object sender, ZipErrorEventArgs e) {
            _extractThemeResult = ThemeInstallingResult.ERROR;
        }

        private static void z_ExtractProgress(object sender, ExtractProgressEventArgs e) {
            if (e.EntriesExtracted == e.EntriesTotal) {
                _extractThemeResult = ThemeInstallingResult.SUCCESS;
            }
        }
        #endregion



        #region PUBLIC STATIC FUNCS

        /// <summary>
        /// Install ImageGlass theme
        /// </summary>
        /// <param name="themePath">Full path of *.igtheme</param>
        /// <returns></returns>
        public static ThemeInstallingResult InstallTheme(string themePath) {
            if (!File.Exists(themePath)) {
                return ThemeInstallingResult.ERROR;
            }

            string themeFolder = App.ConfigDir(PathType.Dir, Dir.Themes);
            Directory.CreateDirectory(themeFolder);

            return ExtractTheme(themePath, themeFolder);
        }


        /// <summary>
        /// Uninstall ImageGlass theme pack
        /// </summary>
        /// <param name="themeFolderName">The theme folder name</param>
        /// <returns></returns>
        public static ThemeUninstallingResult UninstallTheme(string themeFolderName) {
            string fullConfigPath = App.ConfigDir(PathType.Dir, Dir.Themes, themeFolderName, "config.xml");

            if (File.Exists(fullConfigPath)) {
                string dir = Path.GetDirectoryName(fullConfigPath);

                try {
                    Directory.Delete(dir, true);
                }
                catch {
                    return ThemeUninstallingResult.ERROR;
                }
            }
            else {
                return ThemeUninstallingResult.ERROR_THEME_NOT_FOUND;
            }

            return ThemeUninstallingResult.SUCCESS;
        }


        /// <summary>
        /// Pack the theme folder to *.igtheme file
        /// </summary>
        /// <param name="themeFolderPath">The absolute path of theme folder</param>
        /// <param name="outputThemeFile">Output *.igtheme file</param>
        /// <returns></returns>
        public static ThemePackingResult PackTheme(string themeFolderPath, string outputThemeFile) {
            if (!Directory.Exists(themeFolderPath)) {
                return ThemePackingResult.ERROR;
            }

            Theme th = new Theme(themeFolderPath);

            //updated theme config file
            th.SaveAsThemeConfigs(themeFolderPath);

            //if file exist, rename & backup
            if (File.Exists(outputThemeFile)) {
                File.Move(outputThemeFile, outputThemeFile + ".old");
            }

            try {
                using (ZipFile z = new ZipFile(outputThemeFile, Encoding.UTF8)) {
                    z.AddDirectory(themeFolderPath, th.Name);
                    z.Save();
                };
            }
            catch (Exception ex) {
                // restore backup file
                if (File.Exists(outputThemeFile + ".old")) {
                    File.Move(outputThemeFile + ".old", outputThemeFile);
                }

                return ThemePackingResult.ERROR;
            }

            if (File.Exists(outputThemeFile + ".old")) {
                File.Delete(outputThemeFile + ".old");
            }

            return ThemePackingResult.SUCCESS;
        }


        /// <summary>
        /// Invert the color to black or white color
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color InvertBlackAndWhiteColor(Color c) {
            if (c.GetBrightness() > 0.5) {
                return Color.Black;
            }

            return Color.White;
        }


        /// <summary>
        /// Convert Color to CMYK
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int[] ConvertColorToCMYK(Color c) {
            if (c.R == 0 && c.G == 0 && c.B == 0) {
                return new[] { 0, 0, 0, 1 };
            }

            double black = Math.Min(1.0 - c.R / 255.0, Math.Min(1.0 - c.G / 255.0, 1.0 - c.B / 255.0));
            double cyan = (1.0 - (c.R / 255.0) - black) / (1.0 - black);
            double magenta = (1.0 - (c.G / 255.0) - black) / (1.0 - black);
            double yellow = (1.0 - (c.B / 255.0) - black) / (1.0 - black);

            return new[] {
                (int) Math.Round(cyan*100),
                (int) Math.Round(magenta*100),
                (int) Math.Round(yellow*100),
                (int) Math.Round(black*100)
            };
        }


        /// <summary>
        /// Convert Color to HSLA
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float[] ConvertColorToHSLA(Color c) {
            float h = (float)Math.Round(c.GetHue());
            float s = (float)Math.Round(c.GetSaturation() * 100);
            float l = (float)Math.Round(c.GetBrightness() * 100);
            float a = (float)Math.Round(c.A / 255.0, 3);

            return new[] { h, s, l, a };
        }


        /// <summary>
        /// Convert Color to HEX (with alpha)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="skipAlpha"></param>
        /// <returns></returns>
        public static string ConvertColorToHEX(Color c, bool @skipAlpha = false) {
            if (skipAlpha) {
                return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
            }

            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
        }

        /// <summary>
        /// Convert Hex (with alpha) to Color
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color ConvertHexStringToColor(string hex, bool @skipAlpha = false) {
            //Remove # if present
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");

            int red = 0;
            int green = 0;
            int blue = 0;
            int alpha = 255;

            if (hex.Length == 8) {
                //#RRGGBBAA
                red = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                alpha = int.Parse(hex.Substring(6, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hex.Length == 6) {
                //#RRGGBB
                red = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hex.Length == 4) {
                //#RGBA
                red = int.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
                green = int.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
                blue = int.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
                alpha = int.Parse($"{hex[3]}{hex[3]}", NumberStyles.AllowHexSpecifier);
            }
            else if (hex.Length == 3) {
                //#RGB
                red = int.Parse($"{hex[0]}{hex[0]}", NumberStyles.AllowHexSpecifier);
                green = int.Parse($"{hex[1]}{hex[1]}", NumberStyles.AllowHexSpecifier);
                blue = int.Parse($"{hex[2]}{hex[2]}", NumberStyles.AllowHexSpecifier);
            }

            if (skipAlpha) {
                alpha = 255;
            }

            return Color.FromArgb(alpha, red, green, blue);
        }


        /// <summary>
        /// Validate if Hex string is a valid color
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static bool IsValidHex(string hex) {
            if (hex.StartsWith("#")) {
                return hex.Length == 9 || hex.Length == 7 || hex.Length == 5 || hex.Length == 4;
            }

            return false;
        }



        /// <summary>
        /// Makes the color lighter by the given factor (0 = no change, 1 = white).
        /// </summary>
        /// <param name="color">The color to make lighter.</param>
        /// <param name="factor">The factor to make the color lighter (0 = no change, 1 = white).</param>
        /// <returns>The lighter color.</returns>
        public static Color LightenColor(Color color, float factor) {
            float min = 0.001f;
            float max = 1.999f;

            return ControlPaint.Light(color, min + MinMax(factor, 0f, 1f) * (max - min));
        }


        /// <summary>
        /// Makes the color darker by the given factor (0 = no change, 1 = black).
        /// </summary>
        /// <param name="color">The color to make darker.</param>
        /// <param name="factor">The factor to make the color darker (0 = no change, 1 = black).</param>
        /// <returns>The darker color.</returns>
        public static Color DarkenColor(Color color, float factor) {
            float min = -0.5f;
            float max = 1f;

            return ControlPaint.Dark(color, min + MinMax(factor, 0f, 1f) * (max - min));
        }


        /// <summary>
        /// Lightness of the color between black (-1) and white (+1).
        /// </summary>
        /// <param name="color">The color to change the lightness.</param>
        /// <param name="factor">The factor (-1 = black ... +1 = white) to change the lightness.</param>
        /// <returns>The color with the changed lightness.</returns>
        public static Color LightnessColor(Color color, float factor) {
            factor = MinMax(factor, -1f, 1f);
            return factor < 0f ? DarkenColor(color, -factor) : LightenColor(color, factor);
        }



        private static float MinMax(float value, float min, float max) {
            return Math.Min(Math.Max(value, min), max);
        }


        #endregion
    }
}
