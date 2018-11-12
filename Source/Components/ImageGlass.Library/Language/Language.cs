/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

using System.Windows.Forms;
using System.Xml;

namespace ImageGlass.Library
{
    public class Language
    {
        private string _langCode;
        private string _langName;
        private string _author;
        private string _description;
        private string _minVersion;
        private string _fileName;
        private RightToLeft _isRightToLeftLayout;
        //private Dictionary<string, string> _Items;

        private LanguageItem<string, string> _Items;

        #region Properties
        /// <summary>
        /// Get, set code of language
        /// </summary>
        public string LangCode
        {
            get { return _langCode; }
            set { _langCode = value; }
        }

        //Get, set name of language
        public string LangName
        {
            get { return _langName; }
            set { _langName = value; }
        }

        //Get, set author
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        /// <summary>
        /// Get, set description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Get, set language file path
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        /// <summary>
        /// Get, set list of language string
        /// </summary>
        public LanguageItem<string, string> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        /// <summary>
        /// Gets, sets minimum version of ImageGlass that compatible with.
        /// </summary>
        public string MinVersion
        {
            get { return _minVersion; }
            set { _minVersion = value; }
        }

        /// <summary>
        /// Gets, sets the value that indicates right-to-left layout style
        /// </summary>
        public RightToLeft IsRightToLeftLayout
        {
            get { return _isRightToLeftLayout; } 
            set { _isRightToLeftLayout = value; }
        }
        #endregion


        /// <summary>
        /// Set default values of Language
        /// </summary>
        public Language()
        {
            _langCode = "en-US";
            _langName = "Local name of the language";
            _author = "ImageGlass community";
            _description = "English name of language";
            _minVersion = "5.5.7.26";
            _fileName = "";
            _isRightToLeftLayout = RightToLeft.No;

            _Items = new LanguageItem<string, string>();
            InitDefaultLanguageDictionary();
        }



        /// <summary>
        /// Set values of Language
        /// </summary>
        /// <param name="fileName">*.igLang path</param>
        public Language(string fileName)
        {
            _Items = new LanguageItem<string, string>();
            InitDefaultLanguageDictionary();

            _fileName = fileName;
            ReadLanguageFile();
        }



        /// <summary>
        /// Read language strings from file (new format)
        /// </summary>
        public void ReadLanguageFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_fileName);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            XmlElement nType = (XmlElement)root.SelectNodes("Language")[0]; //<Language>
            XmlElement n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

            //Get <Info> Attributes
            LangCode = n.GetAttribute("langCode");
            LangName = n.GetAttribute("langName");
            Author = n.GetAttribute("author");
            Description = n.GetAttribute("description");
            MinVersion = n.GetAttribute("minVersion");

            bool _isRightToLeftLayout = false;
            bool.TryParse(n.GetAttribute("isRightToLeftLayout"), out _isRightToLeftLayout);
            IsRightToLeftLayout = _isRightToLeftLayout ? RightToLeft.Yes : RightToLeft.No; //v3.2

            //Get <Content> element
            XmlElement nContent = (XmlElement)nType.SelectNodes("Content")[0];//<Content>

            //Get all lang items
            XmlNodeList nLangList = nContent.SelectNodes("Item");//<Item>

            foreach (var item in nLangList)
            {
                XmlElement nItem = (XmlElement)item;
                string _key = nItem.GetAttribute("key");
                string _value = nItem.GetAttribute("value").Replace("\\n", "\n");

                try
                {
                    Items[_key] = _value;
                }
                catch { }
            }
        }



        /// <summary>
        /// Export all language strings to xml file
        /// </summary>
        /// <param name="filename"></param>
        public void ExportLanguageToXML(string filename)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");// <ImageGlass>
            XmlElement nType = doc.CreateElement("Language");// <Language>

            XmlElement nInfo = doc.CreateElement("Info");// <Info>
            nInfo.SetAttribute("langCode", LangCode);
            nInfo.SetAttribute("langName", LangName);
            nInfo.SetAttribute("author", Author);
            nInfo.SetAttribute("description", Description);
            nInfo.SetAttribute("minVersion", MinVersion);
            nInfo.SetAttribute("isRightToLeftLayout", IsRightToLeftLayout.ToString());
            nType.AppendChild(nInfo);// <Info />

            XmlElement nContent = doc.CreateElement("Content");// <Content>
            foreach (var item in Items)
            {
                XmlElement n = doc.CreateElement("Item"); // <Item>
                n.SetAttribute("key", item.Key);
                n.SetAttribute("value", item.Value);
                nContent.AppendChild(n);// <Item />
            }
            nType.AppendChild(nContent);

            root.AppendChild(nType);// </Content>
            doc.AppendChild(root);// </ImageGlass>

            doc.Save(filename);
        }



        /// <summary>
        /// This is default language of ImageGlass
        /// </summary>
        private void InitDefaultLanguageDictionary()
        {

            #region Common
            Items.Add("_.ImageFormatGroup.Default", "Default formats"); // 4.0
            Items.Add("_.ImageFormatGroup.Optional", "Optional formats"); // 4.0
            #endregion


            #region frmMain
            Items.Add("frmMain.picMain._ErrorText", "ImageGlass cannot open this picture because the file appears to be damaged, corrupted or not supported.");//v2.0 beta, updated 4.0
            Items.Add("frmMain._ImageNotExist", "The viewing image doesn't exist.");//4.5
            Items.Add("frmMain.picMain.ArchiveEmptyBad", "ImageGlass cannot open this archive: the file is invalid or contains no supported images."); // V6.0
            Items.Add("frmMain.picMain.ArchiveExtractFail", "ImageGlass cannot extract this archive: possible privilege or disk space issue."); // V6.0
            Items.Add("frmMain.picMain.ArchiveSupportMissing", "ImageGlass cannot extract this archive: support DLLs missing."); // V6.0


            #region Tool bar

            //Items.Add("frmMain.btnBack", "Go to previous image (Left arrow / PageUp)"); // removed V6.0
            //Items.Add("frmMain.btnNext", "Go to next image (Right arrow / PageDown)"); // removed V6.0

            Items.Add("frmMain.btnRotateLeft", "Rotate Counterclockwise (Ctrl + ,)");
            Items.Add("frmMain.btnRotateRight", "Rotate Clockwise (Ctrl + .)");

            Items.Add("frmMain.btnFlipHorz", "Flip Horizontal"); // Added V6.0
            Items.Add("frmMain.btnFlipVert", "Flip Vertical");   // Added V6.0

            Items.Add("frmMain.btnZoomIn", "Zoom in (Ctrl + =)");
            Items.Add("frmMain.btnZoomOut", "Zoom out (Ctrl + -)");
            //Items.Add("frmMain.btnZoomToFit", "Zoom to fit (Ctrl + /)"); //4.5, removed v5.5
            Items.Add("frmMain.btnAutoZoom", "Auto zoom (Ctrl + A)"); //5.5
            Items.Add("frmMain.btnScaleToFit", "Scale to fit (Ctrl + /)"); //5.5
            Items.Add("frmMain.btnActualSize", "Actual size (Ctrl + 0)");
            Items.Add("frmMain.btnZoomLock", "Lock zoom ratio (Ctrl + L)");
            Items.Add("frmMain.btnScaletoWidth", "Scale to Width (Ctrl + W)");
            Items.Add("frmMain.btnScaletoHeight", "Scale to Height (Ctrl + H)");
            Items.Add("frmMain.btnWindowAutosize", "Adjust window to actual image dimensions (Ctrl + M)"); //updated 4.0
            Items.Add("frmMain.btnOpen", "Open file (Ctrl + O)");
            Items.Add("frmMain.btnRefresh", "Refresh (F5)");
            Items.Add("frmMain.btnGoto", "Go to ... (Ctrl + G)");
            Items.Add("frmMain.btnThumb", "Show thumbnail (Ctrl + T)");
            //Items.Add("frmMain.btnCaro", "Show checked background (Ctrl + B)"); //removed v5.0
            Items.Add("frmMain.btnFullScreen", "Full screen (Alt + Enter)");
            Items.Add("frmMain.btnSlideShow", "Play slideshow (F11, ESC to exit)");
            Items.Add("frmMain.btnConvert", "Convert image (Ctrl + S)");
            Items.Add("frmMain.btnPrintImage", "Print image (Ctrl + P)");
            //Items.Add("frmMain.btnFacebook", "Upload to Facebook (Ctrl + U)"); //removed 4.5
            //Items.Add("frmMain.btnExtension", "Extension Manager (Ctrl + Shift + E)"); //removed 4.5
            //Items.Add("frmMain.btnSetting", "ImageGlass Settings (Ctrl + Shift + P)"); //removed 4.5
            //Items.Add("frmMain.btnHelp", "Help (F1)"); //removed 4.5

            Items.Add("frmMain.btnMenu", "Menu (Hotkey: `)"); //v3.0
            #endregion


            #region Main menu
            Items.Add("frmMain.mnuMainOpenFile", "Open file"); //v3.0
            Items.Add("frmMain.mnuMainOpenImageData", "Open image data from clipboard"); //v3.0
            Items.Add("frmMain.mnuMainSaveAs", "Save image as ..."); //v3.0
            Items.Add("frmMain.mnuMainRefresh", "Refresh"); //v3.0
            Items.Add("frmMain.mnuMainReloadImage", "Reload image"); //v5.5
            Items.Add("frmMain.mnuMainEditImage", "Edit image {0}"); //v3.0, updated 4.0

            Items.Add("frmMain.mnuMainNavigation", "Navigation"); //v3.0

            Items.Add("frmMain.mnuMainViewNext", "View next image"); //v3.0
            Items.Add("frmMain.mnuMainViewNext.Shortcut", "Right Arrow / PageDown"); //v6.0

            Items.Add("frmMain.mnuMainViewPrevious", "View previous image"); //v3.0
            Items.Add("frmMain.mnuMainViewPrevious.Shortcut", "Left Arrow / PageUp"); // V6.0

            Items.Add("frmMain.mnuMainGoto", "Go to ..."); //v3.0
            Items.Add("frmMain.mnuMainGotoFirst", "Go to the first image"); //v3.0
            Items.Add("frmMain.mnuMainGotoLast", "Go to the last image"); //v3.0

            Items.Add("frmMain.mnuMainFullScreen", "Full screen"); //v3.0

            Items.Add("frmMain.mnuMainSlideShow", "Slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowStart", "Start slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowPause", "Pause / Resume slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowExit", "Exit slideshow"); //v3.0

            Items.Add("frmMain.mnuMainPrint", "Print"); //v3.0

            Items.Add("frmMain.mnuMainManipulation", "Manipulation"); //v3.0
            Items.Add("frmMain.mnuMainRotateCounterclockwise", "Rotate counterclockwise"); //v3.0
            Items.Add("frmMain.mnuMainRotateClockwise", "Rotate clockwise"); //v3.0
            Items.Add("frmMain.mnuMainFlipHorz", "Flip Horizontal"); // V6.0
            Items.Add("frmMain.mnuMainFlipVert", "Flip Vertical"); // V6.0
            Items.Add("frmMain.mnuMainZoomIn", "Zoom in"); //v3.0
            Items.Add("frmMain.mnuMainZoomOut", "Zoom out"); //v3.0
            //Items.Add("frmMain.mnuMainZoomToFit", "Zoom to fit"); //v3.5, removed v5.5
            Items.Add("frmMain.mnuMainScaleToFit", "Zoom to fit"); //v3.5
            Items.Add("frmMain.mnuMainActualSize", "Actual size"); //v3.0
            Items.Add("frmMain.mnuMainLockZoomRatio", "Lock zoom ratio"); //v3.0
            Items.Add("frmMain.mnuMainAutoZoom", "Auto Zoom"); //v5.5
            Items.Add("frmMain.mnuMainScaleToWidth", "Scale to width"); //v3.0
            Items.Add("frmMain.mnuMainScaleToHeight", "Scale to height"); //v3.0
            Items.Add("frmMain.mnuMainWindowAdaptImage", "Adjust window to actual image dimensions"); //v3.0, updated 4.0
            Items.Add("frmMain.mnuMainRename", "Rename image"); //v3.0
            Items.Add("frmMain.mnuMainMoveToRecycleBin", "Move to recycle bin"); //v3.0
            Items.Add("frmMain.mnuMainDeleteFromHardDisk", "Delete from hard disk"); //v3.0
            Items.Add("frmMain.mnuMainExtractFrames", "Extract image frames ({0})"); //v3.0
            Items.Add("frmMain.mnuMainStartStopAnimating", "Start / Stop animating image"); //v3.0
            Items.Add("frmMain.mnuMainSetAsDesktop", "Set as desktop background"); //v3.0
            Items.Add("frmMain.mnuMainSetAsLockImage", "Set as Lock Screen image"); // V6.0

            Items.Add("frmMain.mnuMainImageLocation", "Open image location"); //v3.0
            Items.Add("frmMain.mnuMainImageProperties", "Image properties"); //v3.0

            Items.Add("frmMain.mnuMainClipboard", "Clipboard"); //v3.0
            Items.Add("frmMain.mnuMainCopy", "Copy"); //v3.0
            Items.Add("frmMain.mnuMainCopyImageData", "Copy image data"); //v5.0
            Items.Add("frmMain.mnuMainCut", "Cut"); //v3.0
            //Items.Add("frmMain.mnuMainCopyMulti", "Copy multiple files"); //v3.0, removed 5.0
            //Items.Add("frmMain.mnuMainCutMulti", "Cut multiple files"); //v3.0, removed 5.0
            Items.Add("frmMain.mnuMainCopyImagePath", "Copy image path"); //v3.0
            Items.Add("frmMain.mnuMainClearClipboard", "Clear clipboard"); //v3.0

            Items.Add("frmMain.mnuMainShare", "Share ..."); //v3.0
            //Items.Add("frmMain.mnuMainShareFacebook", "Upload to Facebook"); //v3.0, removed v5.0

            Items.Add("frmMain.mnuMainLayout", "Layout"); //v3.0
            Items.Add("frmMain.mnuMainToolbar", "Toolbar"); //v3.0
            Items.Add("frmMain.mnuMainThumbnailBar", "Thumbnail panel"); //v3.0
            Items.Add("frmMain.mnuMainCheckBackground", "Checkerboard background"); //v3.0, updated v5.0
            Items.Add("frmMain.mnuMainAlwaysOnTop", "Keep window always on top"); //v3.2

            Items.Add("frmMain.mnuMainTools", "Tools"); //v3.0
            //Items.Add("frmMain.mnuMainExtensionManager", "Extension manager"); //v3.0, removed v5.0
            Items.Add("frmMain.mnuMainColorPicker", "Color picker"); //v5.0

            Items.Add("frmMain.mnuMainSettings", "Settings"); //v3.0
            Items.Add("frmMain.mnuMainAbout", "About"); //v3.0

            Items.Add("frmMain.mnuMainFirstLaunch", "First-launch configurations"); //v5.0
            Items.Add("frmMain.mnuMainCheckForUpdate._NoUpdate", "Check for update"); //v5.0
            Items.Add("frmMain.mnuMainCheckForUpdate._NewVersion", "A new version is available!"); //v5.0
            //Items.Add("frmMain.mnuMainCheckForUpdate", "A new version is available"); //v4.5, removed 5.0
            Items.Add("frmMain.mnuMainReportIssue", "Report an issue"); //v3.0
            #endregion


            Items.Add("frmMain._OpenFileDialog", "All supported files");
            Items.Add("frmMain._ArchiveFormats", "All supported archive files"); // V6.0
            Items.Add("frmMain._Text", "file(s)");
            Items.Add("frmMain._ImageData", "Image Data"); //v5.0
            Items.Add("frmMain._RenameDialogText", "Rename"); //v3.5
            Items.Add("frmMain._RenameDialog", "Enter new filename");
            Items.Add("frmMain._GotoDialogText", "Enter the image index to view it. Press {ENTER}");
            Items.Add("frmMain._DeleteDialogText", "Delete file '{0}' ?");
            Items.Add("frmMain._DeleteDialogTitle", "Confirm");

            Items.Add("frmMain._ExtractFrameText", "Extracting image frames. Please select output folder.");
            Items.Add("frmMain._FullScreenMessage", "Press ALT + ENTER to exit full screen mode.\nOr CTRL + F1 to show menu.");//v2.0 beta
            Items.Add("frmMain._SlideshowMessage", "Press ESC to exit slideshow.\n Right click to open context menu.");//v2.0 beta
            Items.Add("frmMain._SlideshowMessagePause", "Slideshow is paused"); // v4.0
            Items.Add("frmMain._SlideshowMessageResume", "Slideshow is resumed"); // v4.0
            Items.Add("frmMain._CopyFileText", "Copied {0} file(s)"); //v2.0 final
            Items.Add("frmMain._CutFileText", "Cut {0} file(s)"); //v2.0 final
            Items.Add("frmMain._CopyImageData", "Image was copied to clipboard"); //v5.0
            Items.Add("frmMain._ClearClipboard", "Clipboard was cleared"); //v2.0 final
            Items.Add("frmMain._SaveChanges", "Saving change..."); //v2.0 final
            Items.Add("frmMain._SaveImage", "Image was saved to\r\n{0}"); //v5.0
            Items.Add("frmMain._SaveImageError", "Unable to save image\r\n{0}."); //v5.0
            Items.Add("frmMain._Loading", "Loading..."); //v3.0
            Items.Add("frmMain._FirstItemOfList", "Reached the first image"); //v4.0
            Items.Add("frmMain._LastItemOfList", "Reached the last image"); //v4.0
            Items.Add("frmMain._CannotRotateAnimatedFile", "Modification for animated format is not supported"); //Added V5.0; Modified V6.0
            #endregion


            #region frmAbout
            Items.Add("frmAbout.lblSlogant", "A lightweight, versatile image viewer"); //changed 4.0
            Items.Add("frmAbout.lblInfo", "Information");
            Items.Add("frmAbout.lblComponent", "Components");
            Items.Add("frmAbout.lblReferences", "References");
            Items.Add("frmAbout.lblVersion", "Version: {0}");
            Items.Add("frmAbout.lblInfoContact", "Contact");
            Items.Add("frmAbout.lblSoftwareUpdate", "Software updates");
            Items.Add("frmAbout.lnkCheckUpdate", "» Check for update");
            Items.Add("frmAbout._Text", "About");
            Items.Add("frmAbout._PortableText", "[Portable]"); //v4.0
            #endregion


            #region frmSetting
            Items.Add("frmSetting._Text", "Settings");

            Items.Add("frmSetting.btnSave", "Save"); //v4.1
            Items.Add("frmSetting.btnCancel", "Cancel"); //v4.1
            Items.Add("frmSetting.btnApply", "Apply"); //v4.1


            #region Tab names
            Items.Add("frmSetting.lblGeneral", "General");
            Items.Add("frmSetting.lblImage", "Image"); //v4.0            
            Items.Add("frmSetting.lblFileAssociations", "File Associations"); //v2.0 final
            Items.Add("frmSetting.lblToolbar", "Toolbar"); //v5.0
            Items.Add("frmSetting.lblColorPicker", "Color Picker"); //v5.0
            Items.Add("frmSetting.lblLanguage", "Language");
            Items.Add("frmSetting.lblTheme", "Theme"); //v5.0
            #endregion


            #region TAB General
            #region Start up
            Items.Add("frmSetting.lblHeadStartup", "Start up"); //v4.0
            Items.Add("frmSetting.chkWelcomePicture", "Show welcome picture");
            Items.Add("frmSetting.chkShowToolBar", "Show toolbar when starting up"); //v4.0

            #endregion


            #region Portable mode
            //Items.Add("frmSetting.lblHeadPortableMode", "Portable mode"); //v4.0, removed 5.5.x

            //Items.Add("frmSetting.chkPortableMode", "Enable Portable mode"); //remove v4.0
            //Items.Add("frmSetting.chkPortableMode._Enabled", "Portable mode is enabled"); //v4.5, removed 5.5.x
            //Items.Add("frmSetting.chkPortableMode._Disabled", "Portable mode is disabled on the installed folder:\r\n{0}"); //v4.5, removed 5.5.x

            Items.Add("frmSetting.lblHeadConfigDir", "Configuration directory"); // 5.5.x
            #endregion


            #region Others
            Items.Add("frmSetting.lblHeadOthers", "Others"); //v4.0
            Items.Add("frmSetting.chkAutoUpdate", "Check for update automatically");
            Items.Add("frmSetting.chkAllowMultiInstances", "Allow multiple instances of the program"); //v3.0
            Items.Add("frmSetting.chkESCToQuit", "Allow to press ESC to quit application"); //v2.0 final
            Items.Add("frmSetting.chkConfirmationDelete", "Display Delete confirmation dialog"); //v4.0
            Items.Add("frmSetting.chkShowScrollbar", "Display viewer scrollbars"); //v4.1
            Items.Add("frmSetting.chkDisplayBasename", "Display basename of the viewing image on title bar"); //v5.0
            Items.Add("frmSetting.lblBackGroundColor", "Background color");
            Items.Add("frmSetting.lnkResetBackgroundColor", "Reset"); // v4.0
            #endregion
            #endregion


            #region TAB Image
            #region Image loading
            Items.Add("frmSetting.lblHeadImageLoading", "Image loading"); //v4.0
            Items.Add("frmSetting.chkFindChildFolder", "Find images in child folder");
            Items.Add("frmSetting.chkShowHiddenImages", "Show hidden images"); //v4.5
            Items.Add("frmSetting.chkLoopViewer", "Loop back viewer to the first image when reaching the end of the list"); //v4.0
            Items.Add("frmSetting.chkImageBoosterBack", "Turn on Image Booster when navigate back (need more ~20% RAM)"); //v2.0 final
            Items.Add("frmSetting.lblImageLoadingOrder", "Image loading order");
            Items.Add("frmSetting.cmbImageOrder._Name", "Name (default)");
            Items.Add("frmSetting.cmbImageOrder._Length", "Length");
            Items.Add("frmSetting.cmbImageOrder._CreationTime", "Creation time");
            Items.Add("frmSetting.cmbImageOrder._LastAccessTime", "Last access time");
            Items.Add("frmSetting.cmbImageOrder._LastWriteTime", "Last write time");
            Items.Add("frmSetting.cmbImageOrder._Extension", "Extension");
            Items.Add("frmSetting.cmbImageOrder._Random", "Random");
            #endregion

            #region Mouse wheel actions
            Items.Add("frmSetting.lblHeadMouseWheelActions", "Mouse wheel actions");
            Items.Add("frmSetting.lblMouseWheel", "Mouse wheel");
            Items.Add("frmSetting.lblMouseWheelAlt", "Mouse wheel + Alt");
            Items.Add("frmSetting.lblMouseWheelCtrl", "Mouse wheel + Ctrl");
            Items.Add("frmSetting.lblMouseWheelShift", "Mouse wheel + Shift");
            Items.Add("frmSetting.cmbMouseWheel._DoNothing", "Do nothing");
            Items.Add("frmSetting.cmbMouseWheel._Zoom", "Zoom");
            Items.Add("frmSetting.cmbMouseWheel._ScrollVertically", "Scroll vertically");
            Items.Add("frmSetting.cmbMouseWheel._ScrollHorizontally", "Scroll horizontally");
            Items.Add("frmSetting.cmbMouseWheel._BrowseImages", "Previous/next image");
            #endregion

            #region Zooming
            Items.Add("frmSetting.lblHeadZooming", "Zooming"); //v4.0
            //Items.Add("frmSetting.chkMouseNavigation", "Use the mouse wheel to browse images, hold CTRL for zooming"); //+3.5
            Items.Add("frmSetting.lblGeneral_ZoomOptimization", "Zoom optimization"); //-3.0, +3.5
            Items.Add("frmSetting.cmbZoomOptimization._Auto", "Auto"); //-3.2, +3.5
            Items.Add("frmSetting.cmbZoomOptimization._SmoothPixels", "Smooth pixels"); //-3.2, +3.5
            Items.Add("frmSetting.cmbZoomOptimization._ClearPixels", "Clear pixels"); //-3.2, +3.5
            #endregion


            #region Thumbnail bar
            Items.Add("frmSetting.lblHeadThumbnailBar", "Thumbnail bar"); //v4.0
            Items.Add("frmSetting.chkThumbnailVertical", "Show thumbnails on right side");
            Items.Add("frmSetting.chkShowThumbnailScrollbar", "Show thumbnails scroll bar"); //v5.5
            //Items.Add("frmSetting.lblGeneral_MaxFileSize", "Maximum thumbnail file size (MB)"); //removed v5.0
            Items.Add("frmSetting.lblGeneral_ThumbnailSize", "Thumbnail dimension (pixel)"); // v3.0
            #endregion


            #region Slideshow
            Items.Add("frmSetting.lblHeadSlideshow", "Slideshow"); //v4.0
            Items.Add("frmSetting.chkLoopSlideshow", "Loop back slideshow to the first image when reaching the end of the list"); //v2.0 final
            Items.Add("frmSetting.lblSlideshowInterval", "Slide show interval: {0} seconds");
            #endregion


            #region Image editing
            Items.Add("frmSetting.lblHeadImageEditing", "Image editing"); //v4.0
            Items.Add("frmSetting.chkSaveOnRotate", "Save the viewing image after rotating"); //v4.5
            Items.Add("frmSetting.lblSelectAppForEdit", "Select application for image editing"); //v4.5
            Items.Add("frmSetting.btnEditEditExt", "Edit"); //v4.0
            Items.Add("frmSetting.btnEditResetExt", "Reset to default"); //v4.0
            Items.Add("frmSetting.btnEditEditAllExt", "Edit all extensions"); //v4.1
            Items.Add("frmSetting._allExtensions", "all extensions"); //v4.1
            Items.Add("frmSetting.lvImageEditing.clnFileExtension", "File extension"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppName", "App name"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppPath", "App path"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppArguments", "App arguments"); //v4.0

            Items.Add("frmSetting.chkSaveModifyDate", "Preserve the image's Modify Date on save"); //v5.5

            #endregion

            #endregion


            #region TAB File Associations
            Items.Add("frmSetting.lblExtensionsGroupDescription", "*Optional formats will not be automatically pre-loaded into memory."); // 4.0
            Items.Add("frmSetting.lblSupportedExtension", "Supported formats: {0}"); // v3.0, updated v4.0
            Items.Add("frmSetting.lnkOpenFileAssoc", "Open File Associations"); // 4.0

            Items.Add("frmSetting.btnAddNewExt", "Add"); // 4.0
            Items.Add("frmSetting.btnRegisterExt", "Set as Default photo viewer"); // 4.0, updated v5.0
            Items.Add("frmSetting.btnDeleteExt", "Delete"); // 4.0
            Items.Add("frmSetting.btnResetExt", "Reset to default"); // 4.0
            #endregion


            #region TAB Toolbar
            Items.Add("frmSetting.lblToolbarPosition", "Toolbar position:"); // v5.5
            Items.Add("frmSetting.cmbToolbarPosition._Top", "Top"); // v5.5
            Items.Add("frmSetting.cmbToolbarPosition._Bottom", "Bottom"); // v5.5

            // V5.0
            Items.Add("frmSetting.txtSeparator", "Separator"); // i.e. 'toolbar separator'
            Items.Add("frmSetting.lblToolbarTT", "Configure toolbar buttons"); // tooltip
            Items.Add("frmSetting.lblUsedBtns", "Current Buttons:");
            Items.Add("frmSetting.lblAvailBtns", "Available Buttons:");
            Items.Add("frmSetting.btnMoveDownTT", "Move selected button down"); // tooltip
            Items.Add("frmSetting.btnMoveLeftTT", "Remove selected button(s) from the toolbar"); // tooltip
            Items.Add("frmSetting.btnMoveRightTT", "Add selected button(s) to the toolbar"); // tooltip
            Items.Add("frmSetting.btnMoveUpTT", "Move selected button up"); // tooltip

            Items.Add("frmSetting.chkHorzCenterToolbarBtns", "Center Toolbar Buttons Horizontally in Window"); // V6.0
            #endregion


            #region TAB Color Picker
            Items.Add("frmSetting.lblColorCodeFormat", "Color code format"); //v5.0
            Items.Add("frmSetting.chkColorUseRGBA", "Use RGBA format"); //v5.0
            Items.Add("frmSetting.chkColorUseHEXA", "Use HEX with alpha format"); //v5.0
            Items.Add("frmSetting.chkColorUseHSLA", "Use HSLA format"); //v5.0
            Items.Add("frmSetting.lblDefaultColorCode", "Default color code format when copying"); //v5.0
            #endregion


            #region TAB Language
            Items.Add("frmSetting.lblLanguageText", "Installed languages");
            Items.Add("frmSetting.lnkRefresh", "> Refresh");
            Items.Add("frmSetting.lblLanguageWarning", "This language pack may be not compatible with {0}"); //v3.2

            Items.Add("frmSetting.lnkInstallLanguage", "> Install new language pack (*.iglang)"); //v2.0 final
            Items.Add("frmSetting.lnkCreateNew", "> Create new language pack");
            Items.Add("frmSetting.lnkEdit", "> Edit selected language pack");
            Items.Add("frmSetting.lnkGetMoreLanguage", "> Get more language packs");
            #endregion


            #region TAB Theme

            Items.Add("frmSetting.lblInstalledThemes", "Installed themes: {0}"); //v5.0
            Items.Add("frmSetting.lnkThemeDownload", "Download themes"); //v5.0
            Items.Add("frmSetting.btnThemeRefresh", "Refresh"); //v5.0
            Items.Add("frmSetting.btnThemeInstall", "Install"); //v5.0
            Items.Add("frmSetting.btnThemeUninstall", "Uninstall"); //v5.0
            Items.Add("frmSetting.btnThemeSaveAs", "Save As"); //v5.0
            Items.Add("frmSetting.btnThemeFolderOpen", "Open Theme Folder"); //v5.0
            Items.Add("frmSetting.btnThemeEdit._Edit", "Edit Selected Theme"); //v5.0
            Items.Add("frmSetting.btnThemeEdit._New", "Create New Theme"); //v5.0
            Items.Add("frmSetting.btnThemeApply", "Apply Theme"); //v5.0

            Items.Add("frmSetting.txtThemeInfo._Name", "Name"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Version", "Version"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Author", "Author"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Email", "Email"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Website", "Website"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Compatibility", "Compatibility"); //v5.0
            Items.Add("frmSetting.txtThemeInfo._Description", "Description"); //v5.0

            Items.Add("frmSetting.btnThemeInstall._Success", "Your theme was installed successfully!"); //v5.0
            Items.Add("frmSetting.btnThemeInstall._Error", "Unable to install your theme."); //v5.0
            Items.Add("frmSetting.btnThemeUninstall._Error", "Unable to uninstall the selected theme."); //v5.0
            Items.Add("frmSetting.btnThemeSaveAs._Success", "Your selected theme has been saved in {0}"); //v5.0
            Items.Add("frmSetting.btnThemeSaveAs._Error", "Unable to save your selected theme."); //v5.0
            Items.Add("frmSetting.btnThemeApply._Success", "The selected theme was applied successfully!"); //v5.0
            Items.Add("frmSetting.btnThemeApply._Error", "Unable to apply the selected theme."); //v5.0

            #endregion


            #endregion


            #region frmAddNewFormat
            Items.Add("frmAddNewFormat.lblFileExtension", "File extension"); // 4.0
            Items.Add("frmAddNewFormat.lblFormatGroup", "Format group"); // 4.0
            Items.Add("frmAddNewFormat.btnOK", "OK"); // 4.0
            Items.Add("frmAddNewFormat.btnClose", "Close"); // 4.0
            #endregion


            #region frmEditEditingAssocisation
            Items.Add("frmEditEditingAssocisation.lblFileExtension", "File extension"); // 4.0
            Items.Add("frmEditEditingAssocisation.lblAppName", "App name"); // 4.0
            Items.Add("frmEditEditingAssocisation.lblAppPath", "App path"); // 4.0
            Items.Add("frmEditEditingAssocisation.lblAppArguments", "App arguments"); // 4.0
            Items.Add("frmEditEditingAssocisation.btnReset", "Reset"); // 4.0
            Items.Add("frmEditEditingAssocisation.btnOK", "OK"); // 4.0
            Items.Add("frmEditEditingAssocisation.btnClose", "Close"); // 4.0
            Items.Add("frmEditEditingAssocisation.lblPreviewLabel", "Preview"); // 5.0
            #endregion


            #region frmFirstLaunch
            Items.Add("frmFirstLaunch._Text", "First-Launch Configurations"); //v5.0
            Items.Add("frmFirstLaunch.lblStepNumber", "Step {0}/{1}"); //v5.0
            Items.Add("frmFirstLaunch.btnNextStep", "Next"); //v5.0
            Items.Add("frmFirstLaunch.btnNextStep._Done", "Done!"); //v5.0
            Items.Add("frmFirstLaunch.lnkSkip", "Skip this and Launch ImageGlass"); //v5.0

            Items.Add("frmFirstLaunch.lblLanguage", "Select Language"); //v5.0
            Items.Add("frmFirstLaunch.lblLayout", "Select Layout"); //v5.0
            Items.Add("frmFirstLaunch.cmbLayout._Standard", "Standard"); //v5.0
            Items.Add("frmFirstLaunch.cmbLayout._Designer", "Designer"); //v5.0
            Items.Add("frmFirstLaunch.lblTheme", "Select Theme"); //v5.0
            Items.Add("frmFirstLaunch.lblDefaultApp", "Set ImageGlass as Default Photo Viewer?"); //v5.0
            Items.Add("frmFirstLaunch.btnSetDefaultApp", "Yes"); //v5.0
            #endregion


            #region REMOVED strings
            //this.Items.Add("frmMain.btnFacebookLike", "Find ImageGlass on the Internet"); //removed v2.0 final
            //this.Items.Add("frmMain.btnFollow", "Follow ImageGlass by email"); //removed v2.0 final
            //this.Items.Add("frmMain.btnReport", "Leave ImageGlass feedbacks"); //removed v3.0

            //this.Items.Add("frmMain.mnuStartSlideshow", "Start slideshow"); //remove 3.0
            //this.Items.Add("frmMain.mnuStopSlideshow", "Stop slideshow"); //remove 3.0
            //this.Items.Add("frmMain.mnuExitSlideshow", "Exit slideshow"); //remove 3.0
            //this.Items.Add("frmMain.mnuShowToolBar._Hide", "Hide toolbar"); //remove 3.0
            //this.Items.Add("frmMain.mnuShowToolBar._Show", "Show toolbar"); //remove 3.0
            ////this.Items.Add("frmMain.mnuEditWithPaint", "Edit with Paint"); //remove 3.0      
            //this.Items.Add("frmMain.mnuExtractFrames", "Extract image frames ({0})"); //remove 3.0
            //this.Items.Add("frmMain.mnuSetWallpaper", "Set as desktop background"); //remove 3.0

            //this.Items.Add("frmMain.mnuPasteImage", "Paste image data"); //v2.0, remove 3.0
            //this.Items.Add("frmMain.mnuCopy", "Copy"); //v2.0, remove 3.0
            //this.Items.Add("frmMain.mnuMultiCopy", "Copy multiple files"); //v2.0, remove 3.0
            //this.Items.Add("frmMain.mnuCut", "Cut"); //v2.0, remove 3.0
            //this.Items.Add("frmMain.mnuMultiCut", "Cut multiple files"); //v2.0, remove 3.0
            //this.Items.Add("frmMain.mnuClearClipboard", "Clear clipboard"); //v2.0, remove 3.0

            //this.Items.Add("frmMain.mnuMoveRecycle", "Move to recycle bin"); //remove 3.0
            //this.Items.Add("frmMain.mnuDelete", "Delete from hard disk"); //remove 3.0
            //this.Items.Add("frmMain.mnuRename", "Rename image"); //remove 3.0
            //this.Items.Add("frmMain.mnuUploadFacebook", "Upload to Facebook"); //remove 3.0
            //this.Items.Add("frmMain.mnuCopyImagePath", "Copy image path"); //remove 3.0
            //this.Items.Add("frmMain.mnuOpenLocation", "Open image location"); //remove 3.0
            //this.Items.Add("frmMain.mnuImageProperties", "Image properties"); //remove 3.0
            //this.Items.Add("frmMain._RecycleBinDialogText", "Send file '{0}' to recycle bin ?"); //removed v3.0
            //this.Items.Add("frmMain._RecycleBinDialogTitle", "Confirm"); //remove v3.0
            //this.Items.Add("frmExtension.Node0", "Get more extensions"); //removed v2.0 final
            //this.Items.Add("frmExtension.lnkGetMoreExt", "Get more extensions"); //removed v2.0 final
            //this.Items.Add("frmSetting.lblContextMenu", "Context menu"); //removed v2.0 final
            //this.Items.Add("frmSetting.chkLockWorkspace", "Lock to workspace edge"); //removed v2.0 beta
            //this.Items.Add("frmSetting._OpenWithImageGlass", "Open with ImageGlass"); //remove 3.0
            //this.Items.Add("frmSetting.lbl_ContextMenu_Description", "This feature helps you open an image quickly by context menu. 'Add default' button lets you add the context menu into all supported extensions of ImageGlass. If you want to customize your extensions, please modify them in 'Extensions' textbox, and then click 'Update' button. 'Remove all' button lets you remove all context menus related to ImageGlass.\n\nAdd shortcut 'Open with ImageGlass' to context menu."); //removed 2.0 final
            //this.Items.Add("frmSetting.lblExtensions", "Extensions:"); //remove 3.0
            //this.Items.Add("frmSetting.btnAddDefaultExtension", "Add default"); //2.0 final, remove 3.0
            //this.Items.Add("frmSetting.lblAddDefaultContextMenu", "Add default"); //removed 2.0 final
            //this.Items.Add("frmSetting.lblContextMenu", "Context menu:"); //2.0 final, remove 3.0
            //this.Items.Add("frmSetting.btnUpdateContextMenu", "Update"); //2.0 final, remove 3.0
            //this.Items.Add("frmSetting.btnRemoveAllContextMenu", "Remove all"); //2.0 final, remove 3.0
            //this.Items.Add("frmSetting.lblUpdateContextMenu", "Update"); //removed 2.0 final
            //this.Items.Add("frmSetting.lblRemoveAllContextMenu", "Remove all"); //removed 2.0 final
            //this.Items.Add("frmSetting.lblFileAssociationsMng", "File associations:"); //add 2.0 final, remove 3.0
            //this.Items.Add("frmSetting.btnSetAssociations", "Set associations"); //v2.0 final, remove 3.0
            //Items.Add("frmSetting.btnOpenFileAssociations", "Open File Associations"); //v2.0 final, -3.5


            #region frmFacebook
            //Items.Add("frmFacebook.lblMessage", "Message"); //removed v5.0
            //Items.Add("frmFacebook.btnUpload._Upload", "Upload"); //removed v5.0
            //Items.Add("frmFacebook.btnUpload._Cancel", "Cancel"); //removed v5.0
            //Items.Add("frmFacebook.btnUpload._ViewImage", "View image"); //removed v5.0
            //Items.Add("frmFacebook.btnClose", "Close"); //removed v5.0
            //Items.Add("frmFacebook._StatusBegin", "Click '{0}' to begin"); //removed v5.0
            //Items.Add("frmFacebook._StatusInvalid", "Invalid filename"); //removed v5.0
            //Items.Add("frmFacebook._StatusUploading", "Uploading..."); //removed v5.0
            //Items.Add("frmFacebook._StatusCancel", "Cancelled"); //removed v5.0
            //Items.Add("frmFacebook._StatusSuccessful", "Successful"); //removed v5.0
            //Items.Add("frmFaceBookLogin._Text", "Logging in to Facebook ..."); //removed v5.0
            #endregion


            #region frmExtension
            //Items.Add("frmExtension._Text", "Extension Manager"); //removed v5.0

            //Items.Add("frmExtension.btnGetMoreExt", "Get more extensions"); //v2.0 final, removed v5.0
            //Items.Add("frmExtension.btnRefreshAllExt", "Refresh"); //v2.0 final, removed v5.0
            //Items.Add("frmExtension.btnInstallExt", "Install"); //v2.0 final, removed v5.0
            //Items.Add("frmExtension.btnClose", "Close"); //removed v5.0
            #endregion


            #endregion



        }



    }
}
