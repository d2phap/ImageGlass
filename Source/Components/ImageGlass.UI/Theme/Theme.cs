/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
Project homepage: https://imageglass.org

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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ImageGlass.UI {
    public partial class Theme {

        /// <summary>
        /// Theme API version, to check compatibility
        /// </summary>
        public string CONFIG_VERSION { get; } = "8";

        /// <summary>
        /// Filename of theme configuration since v8.0
        /// </summary>
        public static string CONFIG_FILE { get; } = "igtheme.xml";

        /// <summary>
        /// Legacy filename of theme configuration
        /// </summary>
        private const string LEGACY_CONFIG_FILE = "config.xml";


        #region PUBLIC PROPERTIES

        /// <summary>
        /// Get the name of theme folder
        /// </summary>
        public string FolderName { get; internal set; }

        /// <summary>
        /// Get theme config file path (<see cref="CONFIG_FILE"/>)
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
        /// Config version of this theme to work with
        /// </summary>
        public string ConfigVersion { get; set; } = "";
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
        /// Menu background hover color
        /// </summary>
        public Color MenuBackgroundHoverColor { get; set; } = Color.FromArgb(35, 0, 0, 0);

        /// <summary>
        /// Menu text color
        /// </summary>
        public Color MenuTextColor { get; set; } = Color.Black;

        /// <summary>
        /// Menu text color on hover
        /// </summary>
        public Color MenuTextHoverColor { get; set; } = Color.Black;

        /// <summary>
        /// The multiplier which impacts the size of the navigation arrows.
        /// </summary>
        public double NavArrowMultiplier { get; set; } = 2.0;


        /// <summary>
        /// The accent color
        /// </summary>
        public Color AccentColor { get; set; } = Color.FromArgb(255, 0, 125, 208);

        /// <summary>
        /// The light accent color
        /// </summary>
        public Color AccentLightColor { get; set; } = Color.FromArgb(255, 0, 161, 225);

        /// <summary>
        /// The dark accent color
        /// </summary>
        public Color AccentDarkColor { get; set; } = Color.FromArgb(255, 0, 75, 150);

        /// <summary>
        /// The app logo
        /// </summary>
        public ThemeImage Logo { get; set; } = new ThemeImage();


        /// <summary>
        /// Show or hide logo on title bar of window
        /// </summary>
        public bool IsShowTitlebarLogo { get; set; } = true;

        #endregion


        #region <TOOLBAR_ICON> node

        /// <summary>
        /// Toolbar Icon collection for the theme
        /// </summary>
        public ThemeIconCollection ToolbarIcons { get; set; } = new ThemeIconCollection();
        #endregion


        #region Navigation arrows
        /// <summary>
        /// Gets, sets the navigation left arrow
        /// </summary>
        public Image NavArrowLeft { get; set; } = null;

        /// <summary>
        /// Gets, sets the navigation right arrow
        /// </summary>
        public Image NavArrowRight { get; set; } = null;

        #endregion


        #endregion


        /// <summary>
        /// Initiate theme object with configuration file (Version 1.5+)
        /// </summary>
        /// <param name="iconHeight">The height of toolbar icons</param>
        /// <param name="themeFolderPath">The absolute path of theme folder.</param>
        public Theme(int iconHeight = Constants.DEFAULT_TOOLBAR_ICON_HEIGHT, string themeFolderPath = "") => IsValid = LoadTheme(iconHeight, themeFolderPath);


        #region PUBLIC CLASS FUNCS

        /// <summary>
        /// Common logic to load an image from a theme config file.
        /// </summary>
        /// <param name="dir">path to folder containing theme files</param>
        /// <param name="n">XMLElement to pull theme filename attribute from</param>
        /// <param name="attribname">name of theme attribute</param>
        /// <param name="iconHeight">optional target height/width</param>
        /// <returns></returns>
        private static ThemeImage LoadThemeImage(string dir, XmlElement n, string attribname, int iconHeight = 0) {
            try {
                var attrib = n.GetAttribute(attribname);

                if (string.IsNullOrEmpty(attrib)) {
                    return new ThemeImage("");
                }

                var imgFile = Path.Combine(dir, attrib);

                if (iconHeight > 0) {
                    return new ThemeImage(imgFile, iconHeight);
                }

                return new ThemeImage(imgFile);
            }
            catch {
                return new ThemeImage("");
            }
        }

        /// <summary>
        /// Reload the image icons to adapt DPI changes
        /// </summary>
        /// <param name="iconHeight">The height of toolbar icons</param>
        public void ReloadIcons(int iconHeight) {
            ToolbarIcons.ViewPreviousImage.Refresh(iconHeight);
            ToolbarIcons.ViewNextImage.Refresh(iconHeight);
            ToolbarIcons.RotateLeft.Refresh(iconHeight);
            ToolbarIcons.RotateRight.Refresh(iconHeight);
            ToolbarIcons.FlipHorz.Refresh(iconHeight);
            ToolbarIcons.FlipVert.Refresh(iconHeight);
            ToolbarIcons.Delete.Refresh(iconHeight);
            ToolbarIcons.Edit.Refresh(iconHeight);
            ToolbarIcons.Crop.Refresh(iconHeight);
            ToolbarIcons.ColorPicker.Refresh(iconHeight);
            ToolbarIcons.ZoomIn.Refresh(iconHeight);
            ToolbarIcons.ZoomOut.Refresh(iconHeight);
            ToolbarIcons.ScaleToFit.Refresh(iconHeight);
            ToolbarIcons.ActualSize.Refresh(iconHeight);
            ToolbarIcons.LockRatio.Refresh(iconHeight);
            ToolbarIcons.AutoZoom.Refresh(iconHeight);
            ToolbarIcons.ScaleToWidth.Refresh(iconHeight);
            ToolbarIcons.ScaleToHeight.Refresh(iconHeight);
            ToolbarIcons.ScaleToFill.Refresh(iconHeight);
            ToolbarIcons.AdjustWindowSize.Refresh(iconHeight);
            ToolbarIcons.OpenFile.Refresh(iconHeight);
            ToolbarIcons.Refresh.Refresh(iconHeight);
            ToolbarIcons.GoToImage.Refresh(iconHeight);
            ToolbarIcons.ThumbnailBar.Refresh(iconHeight);
            ToolbarIcons.Checkerboard.Refresh(iconHeight);
            ToolbarIcons.FullScreen.Refresh(iconHeight);
            ToolbarIcons.Slideshow.Refresh(iconHeight);
            ToolbarIcons.Convert.Refresh(iconHeight);
            ToolbarIcons.Print.Refresh(iconHeight);
            ToolbarIcons.Settings.Refresh(iconHeight);
            ToolbarIcons.About.Refresh(iconHeight);
            ToolbarIcons.Menu.Refresh(iconHeight);
            ToolbarIcons.ViewFirstImage.Refresh(iconHeight);
            ToolbarIcons.ViewLastImage.Refresh(iconHeight);

            #region Navigation arrows (derived from toolbar)

            var arrowHeight = (int)(DPIScaling.Transform(Constants.DEFAULT_TOOLBAR_ICON_HEIGHT) * NavArrowMultiplier);

            var navArrowTemp = new ThemeImage(ToolbarIcons.ViewPreviousImage.Filename, arrowHeight);
            navArrowTemp.Refresh(arrowHeight);
            NavArrowLeft = navArrowTemp.Image;

            navArrowTemp = new ThemeImage(ToolbarIcons.ViewNextImage.Filename, arrowHeight);
            navArrowTemp.Refresh(arrowHeight);
            NavArrowRight = navArrowTemp.Image;

            #endregion
        }


        /// <summary>
        /// Check and process legacy configuration file
        /// </summary>
        /// <param name="themeFolderPath"></param>
        /// <param name="useDefaultIfNotFound">Returns default theme configuration file (<see cref="CONFIG_FILE"/>) if not found</param>
        /// <returns></returns>
        private static string ProcessLegacyTheme(string themeFolderPath, bool useDefaultIfNotFound = true) {
            var configFilePath = Path.Combine(themeFolderPath, CONFIG_FILE);

            if (!File.Exists(configFilePath)) {
                var legacyConfigFilePath = Path.Combine(themeFolderPath, LEGACY_CONFIG_FILE);

                if (File.Exists(legacyConfigFilePath)) {
                    configFilePath = legacyConfigFilePath;
                }
                else {
                    // use default theme
                    if (useDefaultIfNotFound) {
                        configFilePath = App.StartUpDir(Dir.Themes, Constants.DEFAULT_THEME, CONFIG_FILE);
                    }
                    else {
                        configFilePath = "";
                    }
                }
            }

            return configFilePath;
        }


        /// <summary>
        /// Read theme data from theme configuration file (Version 1.5+).
        /// Return TRUE if successful, FALSE if the theme format is invalid
        /// </summary>
        /// <param name="iconHeight">The height of toolbar icons</param>
        /// <param name="themeFolderPath">The absolute path of theme folder.</param>
        /// <returns></returns>
        public bool LoadTheme(int iconHeight, string themeFolderPath) {
            // check and process legacy config filename
            this.ConfigFilePath = ProcessLegacyTheme(themeFolderPath);
            this.FolderName = Path.GetFileName(themeFolderPath); // get folder name

            var dir = Path.GetDirectoryName(this.ConfigFilePath);
            var doc = new XmlDocument();
            doc.Load(this.ConfigFilePath);

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
            catch { }
            try { Version = n.GetAttribute("version"); }
            catch { }
            try { Author = n.GetAttribute("author"); }
            catch { }
            try { Email = n.GetAttribute("email"); }
            catch { }
            try { Website = n.GetAttribute("website"); }
            catch { }
            try { Description = n.GetAttribute("description"); }
            catch { }
            try { Type = n.GetAttribute("type"); }
            catch { }
            try {
                ConfigVersion = n.GetAttribute("configversion");
                ConfigVersion = string.IsNullOrWhiteSpace(ConfigVersion) ? CONFIG_VERSION : ConfigVersion;
            }
            catch { }
            #endregion


            #region Theme <main>
            PreviewImage = LoadThemeImage(dir, n, "preview", iconHeight);

            n = (XmlElement)nType.SelectNodes("main")[0]; //<main>

            ToolbarBackgroundImage = LoadThemeImage(dir, n, "topbar", iconHeight);

            var color = FetchColorAttribute(n, "topbarcolor");
            if (color != Color.Transparent) {
                ToolbarBackgroundColor = color;
            }

            ThumbnailBackgroundImage = LoadThemeImage(dir, n, "bottombar", iconHeight);

            color = FetchColorAttribute(n, "bottombarcolor");
            if (color != Color.Transparent) {
                ThumbnailBackgroundColor = color;
            }

            color = FetchColorAttribute(n, "backcolor");
            if (color != Color.Transparent) {
                BackgroundColor = color;
            }

            color = FetchColorAttribute(n, "statuscolor");
            if (color != Color.Transparent) {
                TextInfoColor = color;
            }

            color = FetchColorAttribute(n, "menubackgroundcolor");
            if (color != Color.Transparent) {
                MenuBackgroundColor = color;
            }

            color = FetchColorAttribute(n, "menubackgroundhovercolor");
            if (color != Color.Transparent) {
                MenuBackgroundHoverColor = color;
            }

            color = FetchColorAttribute(n, "menutextcolor");
            if (color != Color.Transparent) {
                MenuTextColor = color;
            }

            color = FetchColorAttribute(n, "menutexthovercolor");
            if (color != Color.Transparent) {
                MenuTextHoverColor = color;
            }

            // For 7.6: add ability to control the size of the navigation arrows
            // Minimum value is 1.0, default is 2.0.
            try {
                var colorString = n.GetAttribute("navarrowsize");
                if (!string.IsNullOrWhiteSpace(colorString)) {
                    if (!double.TryParse(colorString, out var val)) {
                        val = 2.0;
                    }

                    val = Math.Max(val, 1.0);

                    NavArrowMultiplier = val;
                }
            }
            catch { }

            // v8.0: Accent colors
            color = FetchColorAttribute(n, "accentcolor");
            if (color != Color.Transparent) {
                AccentColor = color;
            }

            color = FetchColorAttribute(n, "accentlightcolor");
            if (color != Color.Transparent) {
                AccentLightColor = color;
            }

            color = FetchColorAttribute(n, "accentdarkcolor");
            if (color != Color.Transparent) {
                AccentDarkColor = color;
            }

            // v8.0: Form icon
            Logo = LoadThemeImage(dir, n, "logo", 128);
            if (Logo.Image is null) {
                Logo.Image = Properties.Resources.DefaultLogo;
            }

            // v8.0: Show icon on title bar
            if (bool.TryParse(n.GetAttribute("isshowtitlebarlogo"), out var showLogo)) {
                IsShowTitlebarLogo = showLogo;
            }

            #endregion


            #region Theme <toolbar_icon>
            n = (XmlElement)nType.SelectNodes("toolbar_icon")[0]; //<toolbar_icon>

            ToolbarIcons.ViewPreviousImage = LoadThemeImage(dir, n, "back", iconHeight);
            ToolbarIcons.ViewNextImage = LoadThemeImage(dir, n, "next", iconHeight);
            ToolbarIcons.RotateLeft = LoadThemeImage(dir, n, "leftrotate", iconHeight);
            ToolbarIcons.RotateRight = LoadThemeImage(dir, n, "rightrotate", iconHeight);
            ToolbarIcons.FlipHorz = LoadThemeImage(dir, n, "fliphorz", iconHeight);
            ToolbarIcons.FlipVert = LoadThemeImage(dir, n, "flipvert", iconHeight);
            ToolbarIcons.Delete = LoadThemeImage(dir, n, "delete", iconHeight);
            ToolbarIcons.Edit = LoadThemeImage(dir, n, "edit", iconHeight);
            ToolbarIcons.Crop = LoadThemeImage(dir, n, "crop", iconHeight);
            ToolbarIcons.ColorPicker = LoadThemeImage(dir, n, "colorpicker", iconHeight);
            ToolbarIcons.ZoomIn = LoadThemeImage(dir, n, "zoomin", iconHeight);
            ToolbarIcons.ZoomOut = LoadThemeImage(dir, n, "zoomout", iconHeight);
            ToolbarIcons.ScaleToFit = LoadThemeImage(dir, n, "zoomtofit", iconHeight);
            ToolbarIcons.ActualSize = LoadThemeImage(dir, n, "scaletofit", iconHeight);
            ToolbarIcons.LockRatio = LoadThemeImage(dir, n, "zoomlock", iconHeight);
            ToolbarIcons.AutoZoom = LoadThemeImage(dir, n, "autozoom", iconHeight);
            ToolbarIcons.ScaleToWidth = LoadThemeImage(dir, n, "scaletowidth", iconHeight);
            ToolbarIcons.ScaleToHeight = LoadThemeImage(dir, n, "scaletoheight", iconHeight);
            ToolbarIcons.ScaleToFill = LoadThemeImage(dir, n, "scaletofill", iconHeight);
            ToolbarIcons.AdjustWindowSize = LoadThemeImage(dir, n, "autosizewindow", iconHeight);
            ToolbarIcons.OpenFile = LoadThemeImage(dir, n, "open", iconHeight);
            ToolbarIcons.Refresh = LoadThemeImage(dir, n, "refresh", iconHeight);
            ToolbarIcons.GoToImage = LoadThemeImage(dir, n, "gotoimage", iconHeight);
            ToolbarIcons.ThumbnailBar = LoadThemeImage(dir, n, "thumbnail", iconHeight);
            ToolbarIcons.Checkerboard = LoadThemeImage(dir, n, "checkerboard", iconHeight);
            ToolbarIcons.FullScreen = LoadThemeImage(dir, n, "fullscreen", iconHeight);
            ToolbarIcons.Slideshow = LoadThemeImage(dir, n, "slideshow", iconHeight);
            ToolbarIcons.Convert = LoadThemeImage(dir, n, "convert", iconHeight);
            ToolbarIcons.Print = LoadThemeImage(dir, n, "print", iconHeight);
            ToolbarIcons.Settings = LoadThemeImage(dir, n, "settings", iconHeight);
            ToolbarIcons.About = LoadThemeImage(dir, n, "about", iconHeight);
            ToolbarIcons.Menu = LoadThemeImage(dir, n, "menu", iconHeight);
            ToolbarIcons.ViewFirstImage = LoadThemeImage(dir, n, "gofirst", iconHeight);
            ToolbarIcons.ViewLastImage = LoadThemeImage(dir, n, "golast", iconHeight);
            #endregion


            #region Arrow cursors (derived from toolbar)

            var arrowHeight = (int)(DPIScaling.Transform(iconHeight) * NavArrowMultiplier);

            var navArrowTemp = new ThemeImage(ToolbarIcons.ViewPreviousImage.Filename, arrowHeight);
            navArrowTemp.Refresh(arrowHeight);
            NavArrowLeft = navArrowTemp.Image;

            NavArrowRight = new ThemeImage(ToolbarIcons.ViewNextImage.Filename, arrowHeight).Image;

            #endregion


            this.IsValid = true;
            return this.IsValid;


            // Fetch a color attribute value from the theme config file.
            // Returns: a Color value if valid; Color.Transparent if an error
            static Color FetchColorAttribute(XmlElement xmlElement, string attribute) {
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
            var doc = new XmlDocument();
            var root = doc.CreateElement("ImageGlass");//<ImageGlass>
            var nType = doc.CreateElement("Theme");//<Theme>

            var n = doc.CreateElement("Info");// <Info>
            n.SetAttribute("name", Name);
            n.SetAttribute("version", Version);
            n.SetAttribute("author", Author);
            n.SetAttribute("email", Email);
            n.SetAttribute("website", Website);
            n.SetAttribute("description", Description);
            n.SetAttribute("type", "ImageGlass Theme Configuration");
            n.SetAttribute("configversion", CONFIG_VERSION);
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
            n.SetAttribute("menubackgroundhovercolor", ConvertColorToHEX(this.MenuBackgroundHoverColor, true));
            n.SetAttribute("menutextcolor", ConvertColorToHEX(this.MenuTextColor, true));
            n.SetAttribute("menutexthovercolor", ConvertColorToHEX(this.MenuTextHoverColor, true));

            n.SetAttribute("accentcolor", ConvertColorToHEX(this.AccentColor, true));
            n.SetAttribute("accentlightcolor", ConvertColorToHEX(this.AccentLightColor, true));
            n.SetAttribute("accentdarkcolor", ConvertColorToHEX(this.AccentDarkColor, true));
            n.SetAttribute("logo", Path.GetFileName(Logo.Filename));
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
            n.SetAttribute("crop", Path.GetFileName(ToolbarIcons.Crop.Filename));
            n.SetAttribute("colorpicker", Path.GetFileName(ToolbarIcons.ColorPicker.Filename));
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

            // create temp directory of theme
            if (Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            doc.Save(Path.Combine(dir, Theme.CONFIG_FILE)); //save file
        }

        #endregion


        #region PRIVATE STATIC FUNCS
        private static ThemeInstallingResult _extractThemeResult = ThemeInstallingResult.UNKNOWN;

        private static ThemeInstallingResult ExtractTheme(string themePath, string dir) {
            _extractThemeResult = ThemeInstallingResult.UNKNOWN;

            try {
                using var z = new ZipFile(themePath, Encoding.UTF8);
                z.ExtractProgress += z_ExtractProgress;
                z.ZipError += z_ZipError;
                z.ExtractAll(dir, ExtractExistingFileAction.OverwriteSilently);
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
        /// Get all theme packs from default folder and user folder
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Theme>> GetAllThemePacksAsync() {
            return await Task.Run(GetAllThemePacks).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all theme packs from default folder and user folder
        /// </summary>
        /// <returns></returns>
        public static List<Theme> GetAllThemePacks() {
            var defaultThemeFolder = App.StartUpDir(Dir.Themes);
            var userThemeFolder = App.ConfigDir(PathType.Dir, Dir.Themes);

            // Create theme folder if not exist
            Directory.CreateDirectory(userThemeFolder);

            var userThemes = Directory.GetDirectories(userThemeFolder);
            var defaultThemes = Directory.GetDirectories(defaultThemeFolder);

            // merge and distinct all themes
            var allThemePaths = defaultThemes.ToList();
            allThemePaths.AddRange(userThemes);
            allThemePaths = allThemePaths.Distinct().ToList();

            var allThemes = new List<Theme>(allThemePaths.Count);

            foreach (var dir in allThemePaths) {
                var configFile = ProcessLegacyTheme(dir, false);

                if (File.Exists(configFile)) {
                    var th = new Theme(themeFolderPath: dir);

                    // invalid theme
                    if (!th.IsValid) {
                        continue;
                    }

                    allThemes.Add(th);
                }
            }

            return allThemes;
        }

        /// <summary>
        /// Install ImageGlass theme
        /// </summary>
        /// <param name="themePath">Full path of *.igtheme</param>
        /// <returns></returns>
        public static ThemeInstallingResult InstallTheme(string themePath) {
            if (!File.Exists(themePath)) {
                return ThemeInstallingResult.ERROR;
            }

            var themeFolder = App.ConfigDir(PathType.Dir, Dir.Themes);
            Directory.CreateDirectory(themeFolder);

            return ExtractTheme(themePath, themeFolder);
        }

        /// <summary>
        /// Uninstall ImageGlass theme pack
        /// </summary>
        /// <param name="themeFolderPath">The theme folder path</param>
        /// <returns></returns>
        public static ThemeUninstallingResult UninstallTheme(string themeFolderPath) {
            try {
                Directory.Delete(themeFolderPath, true);
            }
            catch (DirectoryNotFoundException) {
                return ThemeUninstallingResult.ERROR_THEME_NOT_FOUND;
            }
            catch {
                return ThemeUninstallingResult.ERROR;
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

            var th = new Theme(Constants.DEFAULT_TOOLBAR_ICON_HEIGHT, themeFolderPath);

            // updated theme config file
            th.SaveAsThemeConfigs(themeFolderPath);

            // if file exist, rename & backup
            if (File.Exists(outputThemeFile)) {
                File.Move(outputThemeFile, outputThemeFile + ".old");
            }

            try {
                using var z = new ZipFile(outputThemeFile, Encoding.UTF8);
                z.AddDirectory(themeFolderPath, th.Name);
                z.Save();
            }
            catch (Exception) {
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

            var black = Math.Min(1.0 - (c.R / 255.0), Math.Min(1.0 - (c.G / 255.0), 1.0 - (c.B / 255.0)));
            var cyan = (1.0 - (c.R / 255.0) - black) / (1.0 - black);
            var magenta = (1.0 - (c.G / 255.0) - black) / (1.0 - black);
            var yellow = (1.0 - (c.B / 255.0) - black) / (1.0 - black);

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
            var h = (float)Math.Round(c.GetHue());
            var s = (float)Math.Round(c.GetSaturation() * 100);
            var l = (float)Math.Round(c.GetBrightness() * 100);
            var a = (float)Math.Round(c.A / 255.0, 3);

            return new[] { h, s, l, a };
        }

        /// <summary>
        /// Convert Color to HSVA
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float[] ConvertColorToHSVA(Color c) {
            int max = Math.Max(c.R, Math.Max(c.G, c.B));
            int min = Math.Min(c.R, Math.Min(c.G, c.B));

            var hue = (float)Math.Round(c.GetHue());
            var saturation = (float)Math.Round(100 * ((max == 0) ? 0 : 1f - (1f * min / max)));
            var value = (float)Math.Round(max * 100f / 255);
            var alpha = (float)Math.Round(c.A / 255.0, 3);

            return new[] { hue, saturation, value, alpha };
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
            if (hex.IndexOf('#') != -1) {
                hex = hex.Replace("#", "");
            }

            var red = 0;
            var green = 0;
            var blue = 0;
            var alpha = 255;

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
            const float min = 0.001f;
            const float max = 1.999f;

            return ControlPaint.Light(color, min + (MinMax(factor, 0f, 1f) * (max - min)));
        }

        /// <summary>
        /// Makes the color darker by the given factor (0 = no change, 1 = black).
        /// </summary>
        /// <param name="color">The color to make darker.</param>
        /// <param name="factor">The factor to make the color darker (0 = no change, 1 = black).</param>
        /// <returns>The darker color.</returns>
        public static Color DarkenColor(Color color, float factor) {
            const float min = -0.5f;
            const float max = 1f;

            return ControlPaint.Dark(color, min + (MinMax(factor, 0f, 1f) * (max - min)));
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
