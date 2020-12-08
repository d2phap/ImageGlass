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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ImageGlass.Library {
    public class Language {
        #region Properties
        /// <summary>
        /// Get, set code of language
        /// </summary>
        public string LangCode { get; set; }

        //Get, set name of language
        public string LangName { get; set; }

        //Get, set author
        public string Author { get; set; }

        /// <summary>
        /// Get, set description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Get, set language file path
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Get, set list of language string
        /// </summary>
        public LanguageItem<string, string> Items { get; set; }

        /// <summary>
        /// Gets, sets minimum version of ImageGlass that compatible with.
        /// </summary>
        public string MinVersion { get; set; }

        /// <summary>
        /// Gets, sets the value that indicates right-to-left layout style
        /// </summary>
        public RightToLeft IsRightToLeftLayout { get; set; }
        #endregion

        /// <summary>
        /// Set default values of Language
        /// </summary>
        public Language() {
            LangCode = "en-US";
            LangName = "Local name of the language";
            Author = "ImageGlass community";
            Description = "English name of language";
            MinVersion = "8.0.0.0";
            FileName = "";
            IsRightToLeftLayout = RightToLeft.No;

            Items = new LanguageItem<string, string>();
            InitDefaultLanguageDictionary();
        }

        /// <summary>
        /// Set values of Language
        /// </summary>
        /// <param name="fileName">*.igLang path</param>
        /// <param name="dirPath">The directory path contains language file (for relative filename)</param>
        public Language(string fileName, string dirPath = "") {
            Items = new LanguageItem<string, string>();
            InitDefaultLanguageDictionary();

            FileName = Path.Combine(dirPath, fileName);

            if (File.Exists(FileName)) {
                ReadLanguageFile();
            }
        }

        /// <summary>
        /// Read language strings from file (new format)
        /// </summary>
        public void ReadLanguageFile() {
            var doc = new XmlDocument();
            doc.Load(FileName);

            var root = doc.DocumentElement;// <ImageGlass>
            var nType = (XmlElement)root.SelectNodes("Language")[0]; //<Language>
            var n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

            //Get <Info> Attributes
            LangCode = n.GetAttribute("langCode");
            LangName = n.GetAttribute("langName");
            Author = n.GetAttribute("author");
            Description = n.GetAttribute("description");
            MinVersion = n.GetAttribute("minVersion");

            bool.TryParse(n.GetAttribute("isRightToLeftLayout"), out var _isRightToLeftLayout);
            IsRightToLeftLayout = _isRightToLeftLayout ? RightToLeft.Yes : RightToLeft.No; //v3.2

            //Get <Content> element
            var nContent = (XmlElement)nType.SelectNodes("Content")[0];//<Content>

            //Get all lang items
            var nLangList = nContent.SelectNodes("Item");//<Item>

            foreach (var item in nLangList) {
                var nItem = (XmlElement)item;
                var _key = nItem.GetAttribute("key");
                var _value = nItem.GetAttribute("value").Replace("\\n", "\n");

                try {
                    Items[_key] = _value;
                }
                catch { }
            }
        }

        /// <summary>
        /// Export all language strings to xml file
        /// </summary>
        /// <param name="filename"></param>
        public void ExportLanguageToXML(string filename) {
            var doc = new XmlDocument();
            var root = doc.CreateElement("ImageGlass");// <ImageGlass>
            var nType = doc.CreateElement("Language");// <Language>

            var nInfo = doc.CreateElement("Info");// <Info>
            nInfo.SetAttribute("langCode", LangCode);
            nInfo.SetAttribute("langName", LangName);
            nInfo.SetAttribute("author", Author);
            nInfo.SetAttribute("description", Description);
            nInfo.SetAttribute("minVersion", MinVersion);
            nInfo.SetAttribute("isRightToLeftLayout", IsRightToLeftLayout.ToString());
            nType.AppendChild(nInfo);// <Info />

            var nContent = doc.CreateElement("Content");// <Content>
            foreach (var item in Items) {
                var n = doc.CreateElement("Item"); // <Item>
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
        private void InitDefaultLanguageDictionary() {
            Items.Add("_IncompatibleConfigs", "Some settings are not compatible with your ImageGlass {0}. It's recommended to update them before continuing.\r\n\n- Click Yes to learn about the changes.\r\n- Click No to launch ImageGlass with default settings."); //v7.5

            Items.Add("_.ImageOrderBy._Name", "Name (default)"); //v8.0
            Items.Add("_.ImageOrderBy._Length", "Length"); //v8.0
            Items.Add("_.ImageOrderBy._CreationTime", "Creation time"); //v8.0
            Items.Add("_.ImageOrderBy._LastAccessTime", "Last access time"); //v8.0
            Items.Add("_.ImageOrderBy._LastWriteTime", "Last write time"); //v8.0
            Items.Add("_.ImageOrderBy._Extension", "Extension"); //v8.0
            Items.Add("_.ImageOrderBy._Random", "Random"); //v8.0

            Items.Add("_.ImageOrderType._Asc", "Ascending");  //v8.0
            Items.Add("_.ImageOrderType._Desc", "Descending");  //v8.0

            Items.Add("_.AfterOpeningEditAppAction._Nothing", "Nothing"); //v8.0
            Items.Add("_.AfterOpeningEditAppAction._Minimize", "Minimize"); //v8.0
            Items.Add("_.AfterOpeningEditAppAction._Close", "Close"); //v8.0

            #region frmMain

            #region Main menu

            #region File
            Items.Add("frmMain.mnuMainFile", "File"); //v7.0
            Items.Add("frmMain.mnuMainOpenFile", "Open file…"); //v3.0
            Items.Add("frmMain.mnuMainOpenImageData", "Open image data from clipboard"); //v3.0
            Items.Add("frmMain.mnuMainNewWindow", "Open new window"); //v7.0
            Items.Add("frmMain.mnuMainNewWindow._Error", "Cannot open new window because only one instance allowed"); //v7.0
            Items.Add("frmMain.mnuMainSaveAs", "Save image as…"); //v3.0
            Items.Add("frmMain.mnuMainRefresh", "Refresh"); //v3.0
            Items.Add("frmMain.mnuMainReloadImage", "Reload image"); //v5.5
            Items.Add("frmMain.mnuMainReloadImageList", "Reload image list"); //v7.0
            Items.Add("frmMain.mnuOpenWith", "Open with…"); //v7.6
            Items.Add("frmMain.mnuMainEditImage", "Edit image {0}…"); //v3.0, updated 4.0
            Items.Add("frmMain.mnuMainPrint", "Print…"); //v3.0
            #endregion

            #region Navigation
            Items.Add("frmMain.mnuMainNavigation", "Navigation"); //v3.0
            Items.Add("frmMain.mnuMainViewNext", "View next image"); //v3.0
            Items.Add("frmMain.mnuMainViewNext.Shortcut", "Right Arrow / PageDown"); //v6.0
            Items.Add("frmMain.mnuMainViewPrevious", "View previous image"); //v3.0
            Items.Add("frmMain.mnuMainViewPrevious.Shortcut", "Left Arrow / PageUp"); // V6.0

            Items.Add("frmMain.mnuMainGoto", "Go to…"); //v3.0
            Items.Add("frmMain.mnuMainGotoFirst", "Go to the first image"); //v3.0
            Items.Add("frmMain.mnuMainGotoLast", "Go to the last image"); //v3.0

            Items.Add("frmMain.mnuMainNextPage", "View next page"); //v7.5
            Items.Add("frmMain.mnuMainPrevPage", "View previous page"); //v7.5
            Items.Add("frmMain.mnuMainFirstPage", "View the first page"); //v7.5
            Items.Add("frmMain.mnuMainLastPage", "View the last page"); //v7.5
            #endregion

            #region Zoom
            Items.Add("frmMain.mnuMainZoom", "Zoom"); //v7.0
            Items.Add("frmMain.mnuMainZoomIn", "Zoom in"); //v3.0
            Items.Add("frmMain.mnuMainZoomOut", "Zoom out"); //v3.0
            Items.Add("frmMain.mnuMainScaleToFit", "Scale to fit"); //v3.5
            Items.Add("frmMain.mnuMainScaleToFill", "Scale to fill"); //v7.5
            Items.Add("frmMain.mnuMainActualSize", "Actual size"); //v3.0
            Items.Add("frmMain.mnuMainLockZoomRatio", "Lock zoom ratio"); //v3.0
            Items.Add("frmMain.mnuMainAutoZoom", "Auto zoom"); //v5.5
            Items.Add("frmMain.mnuMainScaleToWidth", "Scale to width"); //v3.0
            Items.Add("frmMain.mnuMainScaleToHeight", "Scale to height"); //v3.0
            #endregion

            #region Image
            Items.Add("frmMain.mnuMainImage", "Image"); //v7.0
            Items.Add("frmMain.mnuMainChannels", "View channels"); //v7.0
            Items.Add("frmMain.mnuMainChannels._All", "All"); //v7.0
            Items.Add("frmMain.mnuMainChannels._Red", "Red"); //v7.0
            Items.Add("frmMain.mnuMainChannels._Green", "Green"); //v7.0
            Items.Add("frmMain.mnuMainChannels._Blue", "Blue"); //v7.0
            Items.Add("frmMain.mnuMainChannels._Black", "Black"); //v7.0
            Items.Add("frmMain.mnuMainChannels._Alpha", "Alpha"); //v7.0

            Items.Add("frmMain.mnuLoadingOrder", "Loading order"); //v8.0

            Items.Add("frmMain.mnuMainRotateLeft", "Rotate left"); //v7.5
            Items.Add("frmMain.mnuMainRotateRight", "Rotate right"); //v7.5
            Items.Add("frmMain.mnuMainFlipHorz", "Flip Horizontal"); // V6.0
            Items.Add("frmMain.mnuMainFlipVert", "Flip Vertical"); // V6.0
            Items.Add("frmMain.mnuMainRename", "Rename image…"); //v3.0
            Items.Add("frmMain.mnuMainMoveToRecycleBin", "Move to recycle bin"); //v3.0
            Items.Add("frmMain.mnuMainDeleteFromHardDisk", "Delete from hard disk"); //v3.0
            Items.Add("frmMain.mnuMainExtractPages", "Extract image pages ({0})…"); //v7.5
            Items.Add("frmMain.mnuMainStartStopAnimating", "Start / Stop animating image"); //v3.0
            Items.Add("frmMain.mnuMainSetAsDesktop", "Set as Desktop background"); //v3.0
            Items.Add("frmMain.mnuMainSetAsLockImage", "Set as Lock screen image"); // V6.0
            Items.Add("frmMain.mnuMainImageLocation", "Open image location"); //v3.0
            Items.Add("frmMain.mnuMainImageProperties", "Image properties"); //v3.0
            #endregion

            #region Clipboard
            Items.Add("frmMain.mnuMainClipboard", "Clipboard"); //v3.0
            Items.Add("frmMain.mnuMainCopy", "Copy"); //v3.0
            Items.Add("frmMain.mnuMainCopyImageData", "Copy image data"); //v5.0
            Items.Add("frmMain.mnuMainCut", "Cut"); //v3.0
            Items.Add("frmMain.mnuMainCopyImagePath", "Copy image path"); //v3.0
            Items.Add("frmMain.mnuMainClearClipboard", "Clear clipboard"); //v3.0
            #endregion

            Items.Add("frmMain.mnuWindowFit", "Window fit"); //v7.5
            Items.Add("frmMain.mnuMainFullScreen", "Full screen"); //v3.0
            Items.Add("frmMain.mnuFrameless", "Frameless"); //v7.5

            #region Slideshow
            Items.Add("frmMain.mnuMainSlideShow", "Slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowStart", "Start slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowPause", "Pause / Resume slideshow"); //v3.0
            Items.Add("frmMain.mnuMainSlideShowExit", "Exit slideshow"); //v3.0
            #endregion

            Items.Add("frmMain.mnuMainShare", "Share…"); //v3.0

            #region Layout
            Items.Add("frmMain.mnuMainLayout", "Layout"); //v3.0
            Items.Add("frmMain.mnuMainToolbar", "Toolbar"); //v3.0
            Items.Add("frmMain.mnuMainThumbnailBar", "Thumbnail panel"); //v3.0
            Items.Add("frmMain.mnuMainCheckBackground", "Checkerboard background"); //v3.0, updated v5.0
            Items.Add("frmMain.mnuMainAlwaysOnTop", "Keep window always on top"); //v3.2
            #endregion

            #region Tools
            Items.Add("frmMain.mnuMainTools", "Tools"); //v3.0
            Items.Add("frmMain.mnuMainColorPicker", "Color picker"); //v5.0
            Items.Add("frmMain.mnuMainPageNav", "Page navigation"); // v7.5
            Items.Add("frmMain.mnuMainCrop", "Cropping"); // v7.6
            Items.Add("frmMain.mnuExifTool", "Exif tool"); // v8.0
            #endregion

            Items.Add("frmMain.mnuMainSettings", "Settings…"); //v3.0

            #region Help
            Items.Add("frmMain.mnuMainHelp", "Help"); //v7.0
            Items.Add("frmMain.mnuMainAbout", "About"); //v3.0
            Items.Add("frmMain.mnuMainFirstLaunch", "First-launch configurations…"); //v5.0
            Items.Add("frmMain.mnuMainCheckForUpdate._NoUpdate", "Check for update…"); //v5.0
            Items.Add("frmMain.mnuMainCheckForUpdate._NewVersion", "A new version is available!"); //v5.0
            Items.Add("frmMain.mnuMainReportIssue", "Report an issue…"); //v3.0

            Items.Add("frmMain.mnuMainExitApplication", "Exit ImageGlass"); //v7.0
            #endregion

            #endregion

            #region Form message texts
            Items.Add("frmMain.picMain._ErrorText", "ImageGlass cannot open this picture because the file appears to be damaged, corrupted or not supported.");// v2.0 beta, updated 4.0
            Items.Add("frmMain._ImageNotExist", "The viewing image doesn't exist.");// v4.5
            Items.Add("frmMain.btnMenu", "Menu (Hotkey: `)"); // v3.0

            Items.Add("frmMain._OpenFileDialog", "All supported files");
            Items.Add("frmMain._Files", "file(s)"); // v7.5
            Items.Add("frmMain._Pages", "pages"); // v7.5
            Items.Add("frmMain._ImageData", "Image data"); // v5.0
            Items.Add("frmMain._RenameDialogText", "Rename"); // v3.5
            Items.Add("frmMain._RenameDialog", "Enter new filename");
            Items.Add("frmMain._GotoDialogText", "Enter the image index to view it. Press ENTER");
            Items.Add("frmMain._DeleteDialogText", "Delete file '{0}' ?");
            Items.Add("frmMain._DeleteDialogTitle", "Confirm");

            Items.Add("frmMain._ExtractPageText", "Extracting image pages. Please select output folder.");
            Items.Add("frmMain._FullScreenMessage", "Press {0} to exit full screen mode.");// v2.0 beta, v6.0, v8.0
            Items.Add("frmMain._SlideshowMessage", "Press ESC to exit slideshow.\n Right click to open context menu."); // v2.0 beta
            Items.Add("frmMain._SlideshowMessagePause", "Slideshow is paused"); // v4.0
            Items.Add("frmMain._SlideshowMessageResume", "Slideshow is resumed"); // v4.0
            Items.Add("frmMain._CopyFileText", "Copied {0} file(s)"); // v2.0 final
            Items.Add("frmMain._CutFileText", "Cut {0} file(s)"); // v2.0 final
            Items.Add("frmMain._CopyImageData", "Image was copied to clipboard"); // v5.0
            Items.Add("frmMain._ClearClipboard", "Clipboard was cleared"); // v2.0 final
            Items.Add("frmMain._SaveChanges", "Saving change..."); // v2.0 final
            Items.Add("frmMain._SaveImage", "Image was saved to\r\n{0}"); // v5.0
            Items.Add("frmMain._SavingImage", "Saving image...\r\n{0}"); // v7.6
            Items.Add("frmMain._SaveImageError", "Unable to save image\r\n{0}."); // v5.0
            Items.Add("frmMain._Loading", "Loading..."); // v3.0
            Items.Add("frmMain._FirstItemOfList", "Reached the first image"); // v4.0
            Items.Add("frmMain._LastItemOfList", "Reached the last image"); // v4.0
            Items.Add("frmMain._CannotRotateAnimatedFile", "Modification for animated format is not supported"); // Added V5.0; Modified V6.0
            Items.Add("frmMain._SetLockImage_Error", "There was an error while setting lock screen image"); // v6.0
            Items.Add("frmMain._SetLockImage_Success", "Lock screen image was set successfully"); //v6.0
            Items.Add("frmMain._SetBackground_Error", "There was an error while setting desktop background"); // v6.0
            Items.Add("frmMain._SetBackground_Success", "Desktop background was set successfully"); // v6.0

            Items.Add("frmMain._PageExtractComplete", "Page extraction completed."); // v7.5
            Items.Add("frmMain._Frameless", "Hold SHIFT to move the window."); // v7.5
            Items.Add("frmMain._InvalidImageClipboardData", "Clipboard does not contain image data."); // v8.0
            #endregion

            #endregion

            #region frmAbout
            Items.Add("frmAbout.lblSlogant", "A lightweight, versatile image viewer"); //changed 4.0
            Items.Add("frmAbout.lblInfo", "Information");
            Items.Add("frmAbout.lblComponent", "Components");
            Items.Add("frmAbout.lblReferences", "References");
            Items.Add("frmAbout.lblVersion", "Version: {0}");
            Items.Add("frmAbout.lblInfoContact", "Contact");
            Items.Add("frmAbout.lblSoftwareUpdate", "Software updates");
            Items.Add("frmAbout.lnkCheckUpdate", "» Check for update…");
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
            Items.Add("frmSetting.lblEdit", "Edit"); //v6.0
            Items.Add("frmSetting.lblFileTypeAssoc", "File Type Associations"); //v2.0 final
            Items.Add("frmSetting.lblToolbar", "Toolbar"); //v5.0
            Items.Add("frmSetting.lblLanguage", "Language");
            Items.Add("frmSetting.lblTheme", "Theme"); //v5.0
            Items.Add("frmSetting.lblKeyboard", "Keyboard"); // v7.0
            #endregion

            #region TAB General
            #region Start up
            Items.Add("frmSetting.lblHeadStartup", "Start up"); //v4.0
            Items.Add("frmSetting.chkWelcomePicture", "Show welcome picture");
            Items.Add("frmSetting.chkLastSeenImage", "Open last seen image"); //v6.0
            Items.Add("frmSetting.chkShowToolBar", "Show toolbar when starting up"); //v4.0

            #endregion

            #region Configuration dir
            //Items.Add("frmSetting.lblHeadPortableMode", "Portable mode"); //v4.0, removed 5.5.x

            //Items.Add("frmSetting.chkPortableMode", "Enable Portable mode"); //remove v4.0
            //Items.Add("frmSetting.chkPortableMode._Enabled", "Portable mode is enabled"); //v4.5, removed 5.5.x
            //Items.Add("frmSetting.chkPortableMode._Disabled", "Portable mode is disabled on the installed folder:\r\n{0}"); //v4.5, removed 5.5.x

            Items.Add("frmSetting.lblHeadConfigDir", "Configuration directory"); // 5.5.x
            #endregion

            #region Viewer
            Items.Add("frmSetting.lblHeadViewer", "Viewer"); // v7.6
            Items.Add("frmSetting.chkShowScrollbar", "Display viewer scrollbars"); //v4.1
            Items.Add("frmSetting.chkShowNavButtons", "Display navigation arrow buttons"); //v6.0
            Items.Add("frmSetting.chkDisplayBasename", "Display basename of the viewing image on title bar"); //v5.0
            Items.Add("frmSetting.chkShowCheckerboardOnlyImage", "Display checkerboard only in the image region"); //v6.0
            Items.Add("frmSetting.chkUseTouchGesture", "Enable touch gesture support"); // v7.6
            Items.Add("frmSetting.lblBackGroundColor", "Background color");
            Items.Add("frmSetting.lnkResetBackgroundColor", "Reset"); // v4.0
            #endregion

            #region Others
            Items.Add("frmSetting.lblHeadOthers", "Others"); //v4.0
            Items.Add("frmSetting.chkAutoUpdate", "Check for update automatically");
            Items.Add("frmSetting.chkAllowMultiInstances", "Allow multiple instances of the program"); //v3.0
            Items.Add("frmSetting.chkESCToQuit", "Allow to press ESC to quit application"); //v2.0 final
            Items.Add("frmSetting.chkConfirmationDelete", "Display Delete confirmation dialog"); //v4.0
            Items.Add("frmSetting.chkCenterWindowFit", "Auto-center the window in Window Fit mode"); //v7.5
            Items.Add("frmSetting.chkShowToast", "Show toast message"); //v7.5

            #endregion
            #endregion

            #region TAB Image
            #region Image loading
            Items.Add("frmSetting.lblHeadImageLoading", "Image loading"); //v4.0
            Items.Add("frmSetting.chkFindChildFolder", "Find images in child folder");
            Items.Add("frmSetting.chkShowHiddenImages", "Show hidden images"); //v4.5
            Items.Add("frmSetting.chkLoopViewer", "Loop back viewer to the first image when reaching the end of the list"); //v4.0
            Items.Add("frmSetting.chkIsCenterImage", "Center image on viewer"); //v7.0
            // Items.Add("frmSetting.chkImageBoosterBack", "Turn on Image Booster when navigate back (need more ~20% RAM)"); //v2.0 final // removed 7.0

            Items.Add("frmSetting.lblImageLoadingOrder", "Image loading order");
            Items.Add("frmSetting.chkUseFileExplorerSortOrder", "Use Windows File Explorer sort order if possible"); //v7.0
            Items.Add("frmSetting.chkGroupByDirectory", "Group images by directory"); //v8.0
            Items.Add("frmSetting.lblImageBoosterCachedCount", "Number of images cached by ImageBooster (one direction)"); //v7.0
            #endregion

            #region Color Management
            Items.Add("frmSetting.lblColorManagement", "Color management"); //v6.0
            Items.Add("frmSetting.chkApplyColorProfile", "Apply also for images without embedded color profile"); //v6.0
            Items.Add("frmSetting.lblColorProfile", "Color profile:"); //v6.0
            Items.Add("frmSetting.lnkColorProfileBrowse", "Browse…"); //v6.0
            Items.Add("frmSetting.cmbColorProfile._None", "None"); //v6.0
            Items.Add("frmSetting.cmbColorProfile._CustomProfileFile", "Custom…"); //v6.0

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

            Items.Add("frmSetting.lblZoomLevels", "Zoom levels"); //v7.0
            Items.Add("frmSetting.txtZoomLevels._Error", "There was error updating Zoom levels. Error message:\r\n\n{0}"); //v7.0
            #endregion

            #region Thumbnail bar
            Items.Add("frmSetting.lblHeadThumbnailBar", "Thumbnail bar"); //v4.0
            Items.Add("frmSetting.chkThumbnailVertical", "Show thumbnails on right side");
            Items.Add("frmSetting.chkShowThumbnailScrollbar", "Show thumbnails scroll bar"); //v5.5
            //Items.Add("frmSetting.lblGeneral_MaxFileSize", "Maximum thumbnail file size (MB)"); //removed v5.0
            Items.Add("frmSetting.lblGeneral_ThumbnailSize", "Thumbnail dimension (pixel)"); // v3.0
            #endregion

            #region Slideshow
            Items.Add("frmSetting.lblHeadSlideshow", "Slideshow"); // v4.0
            Items.Add("frmSetting.chkLoopSlideshow", "Loop back slideshow to the first image when reaching the end of the list"); // v2.0 final
            Items.Add("frmSetting.chkShowSlideshowCountdown", "Show slideshow countdown"); // v7.5
            Items.Add("frmSetting.chkRandomSlideshowInterval", "Use random interval"); // v7.6
            Items.Add("frmSetting.lblSlideshowInterval", "Slideshow interval: {0}");
            Items.Add("frmSetting.lblSlideshowIntervalTo", "to"); // v7.6
            #endregion

            #endregion

            #region TAB Edit
            Items.Add("frmSetting.chkSaveOnRotate", "Save the viewing image after rotating"); //v4.5
            Items.Add("frmSetting.lblSelectAppForEdit", "Select application for image editing"); //v4.5
            Items.Add("frmSetting.lblAfterEditingApp", "After opening editing app:"); // v8.0
            Items.Add("frmSetting.lblImageQuality", "Image quality:"); // v8.0

            Items.Add("frmSetting.btnEditEditExt", "Edit…"); //v4.0
            Items.Add("frmSetting.btnEditResetExt", "Reset to default"); //v4.0
            Items.Add("frmSetting.btnEditEditAllExt", "Edit all extensions…"); //v4.1
            Items.Add("frmSetting._allExtensions", "all extensions"); //v4.1
            Items.Add("frmSetting.lvImageEditing.clnFileExtension", "File extension"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppName", "App name"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppPath", "App path"); //v4.0
            Items.Add("frmSetting.lvImageEditing.clnAppArguments", "App arguments"); //v4.0

            Items.Add("frmSetting.chkSaveModifyDate", "Preserve the image's modified date on save"); //v5.5, v8.0
            #endregion

            #region TAB File Associations
            Items.Add("frmSetting.lblSupportedExtension", "Supported formats: {0}"); // v3.0, updated v4.0
            Items.Add("frmSetting.lnkOpenFileAssoc", "Open File Type Associations"); // 4.0

            Items.Add("frmSetting.btnAddNewExt", "Add…"); // 4.0
            Items.Add("frmSetting.btnRegisterExt", "Set as Default photo viewer…"); // 4.0, updated v5.0
            Items.Add("frmSetting.btnUnregisterExt", "Unregister extensions"); // 8.0
            Items.Add("frmSetting.btnDeleteExt", "Delete"); // 4.0
            Items.Add("frmSetting.btnResetExt", "Reset to default"); // 4.0
            Items.Add("frmSetting._RegisterWebToApp_Error", "Unable to register Web-to-App linking"); // 7.0
            Items.Add("frmSetting._RegisterAppExtensions_Error", "Unable to register file extensions for ImageGlass app"); // 6.0
            Items.Add("frmSetting._RegisterAppExtensions_Success", "All file extensions are registered successfully! To set ImageGlass as Default photo viewer, please open Windows Settings > Default Apps, and manually select ImageGlass app under Photo Viewer section."); // 6.0

            Items.Add("frmSetting._UnregisterAppExtensions_Error", "Unable to delete registered file extensions of ImageGlass app"); // 8.0
            Items.Add("frmSetting._UnregisterAppExtensions_Success", "All file extensions are unregistered successfully!"); // 8.0
            #endregion

            #region TAB Toolbar
            Items.Add("frmSetting.lblToolbarPosition", "Toolbar position:"); // v5.5
            Items.Add("frmSetting.lblToolbarIconHeight", "Toolbar icon size:");
            Items.Add("frmSetting.cmbToolbarPosition._Top", "Top"); // v5.5
            Items.Add("frmSetting.cmbToolbarPosition._Bottom", "Bottom"); // v5.5

            // V5.0
            Items.Add("frmSetting._separator", "Separator"); // i.e. 'toolbar separator'
            Items.Add("frmSetting.lblToolbar._Tooltip", "Configure toolbar buttons"); // tooltip
            Items.Add("frmSetting.lblUsedBtns", "Current Buttons:");
            Items.Add("frmSetting.lblAvailBtns", "Available Buttons:");
            Items.Add("frmSetting.btnMoveDown._Tooltip", "Move selected button down"); // tooltip
            Items.Add("frmSetting.btnMoveLeft._Tooltip", "Remove selected button(s) from the toolbar"); // tooltip
            Items.Add("frmSetting.btnMoveRight._Tooltip", "Add selected button(s) to the toolbar"); // tooltip
            Items.Add("frmSetting.btnMoveUp._Tooltip", "Move selected button up"); // tooltip

            Items.Add("frmSetting.chkHorzCenterToolbarBtns", "Center toolbar buttons horizontally in window"); // V6.0
            Items.Add("frmSetting.chkHideTooltips", "Hide toolbar tooltips"); // v8.0
            #endregion

            #region TAB Tools
            Items.Add("frmSetting.chkColorUseRGBA", "Use RGB format with Alpha value"); //v5.0
            Items.Add("frmSetting.chkColorUseHEXA", "Use HEX format with Alpha value"); //v5.0
            Items.Add("frmSetting.chkColorUseHSLA", "Use HSL format with Alpha value"); //v5.0
            Items.Add("frmSetting.chkColorUseHSVA", "Use HSV format with Alpha value"); //v8.0
            Items.Add("frmSetting.lblDefaultColorCode", "Default color code format when copying"); //v5.0

            Items.Add("frmSetting.chkShowPageNavAuto", "Auto-show Page navigation tool for multi-page image"); //v7.5

            Items.Add("frmSetting.chkExifToolAlwaysOnTop", "Keep Exif tool always on top"); // v8.0
            Items.Add("frmSetting.lnkSelectExifTool", "Select Exif tool file"); // v8.0
            Items.Add("frmSetting.lnkSelectExifTool._NotFound", "The Exif tool does not exist or invalid: \n{0}"); // v8.0
            #endregion

            #region TAB Language
            Items.Add("frmSetting.lblLanguageText", "Installed languages");
            Items.Add("frmSetting.lnkRefresh", "> Refresh");
            Items.Add("frmSetting.lblLanguageWarning", "This language pack may be not compatible with {0}"); //v3.2

            Items.Add("frmSetting.lnkInstallLanguage", "> Install new language pack (*.iglang)…"); //v2.0 final
            Items.Add("frmSetting.lnkCreateNew", "> Create new language pack…");
            Items.Add("frmSetting.lnkEdit", "> Edit selected language pack…");
            Items.Add("frmSetting.lnkGetMoreLanguage", "> Get more language packs…");
            #endregion

            #region TAB Theme

            Items.Add("frmSetting.lblInstalledThemes", "Installed themes: {0}"); //v5.0
            Items.Add("frmSetting.lnkThemeDownload", "Download themes…"); //v5.0
            Items.Add("frmSetting.btnThemeRefresh", "Refresh"); //v5.0
            Items.Add("frmSetting.btnThemeInstall", "Install…"); //v5.0
            Items.Add("frmSetting.btnThemeUninstall", "Uninstall…"); //v5.0
            Items.Add("frmSetting.btnThemeSaveAs", "Save as…"); //v5.0
            Items.Add("frmSetting.btnThemeFolderOpen", "Open theme folder"); //v5.0
            Items.Add("frmSetting.btnThemeApply", "Apply theme"); //v5.0

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

            #region TAB Keyboard
            Items.Add("frmSetting.btnKeyReset", "Reset to default"); // v7.0
            Items.Add("frmSetting.lblKeysSpaceBack", "Space / Backspace"); // v7.0
            Items.Add("frmSetting.lblKeysPageUpDown", "PageUp / PageDown"); // v7.0
            Items.Add("frmSetting.lblKeysUpDown", "Up / Down arrows"); // v7.0
            Items.Add("frmSetting.lblKeysLeftRight", "Left / Right arrows"); // v7.0

            #region Actions Combo Values
            Items.Add("frmSetting.KeyActions._PrevNextImage", "Previous / Next Image"); // v7.0
            Items.Add("frmSetting.KeyActions._PanLeftRight", "Pan Left / Right"); // v7.0
            Items.Add("frmSetting.KeyActions._PanUpDown", "Pan Up / Down"); // v7.0
            Items.Add("frmSetting.KeyActions._ZoomInOut", "Zoom In / Out"); // v7.0
            Items.Add("frmSetting.KeyActions._PauseSlideshow", "Pause slideshow"); // v7.0
            Items.Add("frmSetting.KeyActions._DoNothing", "Do nothing"); // v7.0
            #endregion

            #endregion

            #endregion

            #region frmAddNewFormat
            Items.Add("frmAddNewFormat.lblFileExtension", "File extension"); // 4.0
            Items.Add("frmAddNewFormat.btnOK", "OK"); // 4.0
            Items.Add("frmAddNewFormat.btnClose", "Close"); // 4.0
            #endregion

            #region frmEditApp
            Items.Add("frmEditApp.lblFileExtension", "File extension"); // 4.0
            Items.Add("frmEditApp.lblAppName", "App name"); // 4.0
            Items.Add("frmEditApp.lblAppPath", "App path"); // 4.0
            Items.Add("frmEditApp.lblAppArguments", "App arguments"); // 4.0
            Items.Add("frmEditApp.btnReset", "Reset"); // 4.0
            Items.Add("frmEditApp.btnOK", "OK"); // 4.0
            Items.Add("frmEditApp.btnClose", "Close"); // 4.0
            Items.Add("frmEditApp.lblPreviewLabel", "Preview"); // 5.0
            #endregion

            #region frmFirstLaunch
            Items.Add("frmFirstLaunch._Text", "First-Launch Configurations"); //v5.0
            Items.Add("frmFirstLaunch._ConfirmCloseProcess", "ImageGlass needs to close all its processes to apply the new settings, do you want to continue?"); //v7.5
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

            #region frmCrop
            Items.Add("frmCrop.lblWidth", "Width:"); //v7.6
            Items.Add("frmCrop.lblHeight", "Height:"); //v7.6
            Items.Add("frmCrop.btnSave", "Save"); //v7.6
            Items.Add("frmCrop.btnSaveAs", "Save as…"); //v7.6
            Items.Add("frmCrop.btnCopy", "Copy"); //v7.6
            Items.Add("frmCrop.btnReset", "Reset"); //v8.0

            #endregion

            #region FrmExifTool
            Items.Add("FrmExifTool.clnProperty", "Property"); // v8.0
            Items.Add("FrmExifTool.clnValue", "Value"); // v8.0

            Items.Add("FrmExifTool.btnCopyValue", "Copy value"); // v8.0
            Items.Add("FrmExifTool.btnExport", "Export…"); // v8.0
            Items.Add("FrmExifTool.btnClose", "Close"); // v8.0

            #endregion
        }
    }
}
