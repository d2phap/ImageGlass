/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2015-2018 DUONG DIEU PHAP
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
 
A form to customize the toolbar.
This file created by Kevin Routley (aka fire-eggs).
*/

using System;
using System.Collections.Generic;
using System.Text;
using ImageGlass.Properties;
using ImageGlass.Services.Configuration;
using System.Drawing;
using System.Windows.Forms;


// How to add a new toolbar button:
// 1.

namespace ImageGlass
{
    // ReSharper disable EmptyGeneralCatchClause
    // ReSharper disable once InconsistentNaming
    // ReSharper disable UnusedMember.Local

    public partial class frmCustToolbar : Form
    {
        private string _separatorText; // Text used in lists for separator

        public frmCustToolbar()
        {
            InitializeComponent();

            InitLanguagePack();
            BuildImageList();
            InitUsedList();
            InitAvailList();
            UpdateButtonState();
        }

        private void InitLanguagePack()
        {
            _separatorText = GlobalSetting.LangPack.Items["frmSetting.txtSeparator"];

        }

        // TODO consider fetching from frmMain field values instead???
        private ImageList _images;
        private void BuildImageList()
        {
            // The image order here must match that of the enum
            _images = new ImageList();
            _images.ImageSize = new Size(28,28);

            _images.Images.Add(Resources.back);
            _images.Images.Add(Resources.next);
            _images.Images.Add(Resources.leftrotate);
            _images.Images.Add(Resources.rightrotate);
            _images.Images.Add(Resources.zoomin);
            _images.Images.Add(Resources.zoomout);
            _images.Images.Add(Resources.scaletofit);
            _images.Images.Add(Resources.zoomlock);
            _images.Images.Add(Resources.scaletowidth);
            _images.Images.Add(Resources.scaletoheight);
            _images.Images.Add(Resources.autosizewindow);
            _images.Images.Add(Resources.open);
            _images.Images.Add(Resources.refresh);
            _images.Images.Add(Resources.gotoimage);
            _images.Images.Add(Resources.thumbnail);
            _images.Images.Add(Resources.background);
            _images.Images.Add(Resources.fullscreen);
            _images.Images.Add(Resources.slideshow);
            _images.Images.Add(Resources.convert);
            _images.Images.Add(Resources.printer);
            //_images.Images.Add(Resources.uploadfb);
            //_images.Images.Add(Resources.extension);
            //_images.Images.Add(Resources.settings);
            //_images.Images.Add(Resources.about);

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            // TODO these images should all move to resources
            var img = resources.GetObject("mnuMainRename.Image");
            if (img != null) // TODO failure to get an image means the toolbar button shouldn't be available
                _images.Images.Add((Image)img);
            img = resources.GetObject("mnuMainMoveToRecycleBin.Image");
            if (img != null) // TODO failure to get an image means the toolbar button shouldn't be available
                _images.Images.Add((Image)img);
            img = resources.GetObject("mnuMainEditImage.Image");
            if (img != null) // TODO failure to get an image means the toolbar button shouldn't be available
                _images.Images.Add((Image)img);
        }

        private List<ListViewItem> _masterUsedList;

        private void InitUsedList()
        {
            usedButtons.View = View.SmallIcon;
            usedButtons.SmallImageList = _images;
            usedButtons.FullRowSelect = true;
            usedButtons.MultiSelect = true;
            usedButtons.Sorting = SortOrder.None;

            string currentSet = GlobalSetting.ToolbarButtons;
            var enumList = TranslateFromConfig(currentSet);

            _masterUsedList = new List<ListViewItem>(enumList.Count);
            for (int i = 0; i < enumList.Count; i++)
            {
                ListViewItem lvi = new ListViewItem();

                if (enumList[i] == allBtns.Separator)
                {
                    lvi.Text = _separatorText;
                    lvi.ToolTipText = _separatorText;
                    lvi.Tag = allBtns.Separator;
                }
                else
                {
                    lvi.ImageIndex = (int)enumList[i];
                    var btnEnum = enumList[i];

                    // Here we fetch the localized string by the *name* of the enum
                    string name = string.Format("frmMain.{0}", btnEnum);
                    lvi.Text = GlobalSetting.LangPack.Items[name];
                    lvi.ToolTipText = GlobalSetting.LangPack.Items[name];
                    lvi.Tag = btnEnum;
                }

                _masterUsedList.Add(lvi);
            }

            usedButtons.Items.AddRange(_masterUsedList.ToArray());
        }

        private void InitAvailList()
        {
            availButtons.View = View.List;
            availButtons.SmallImageList = _images;
            availButtons.FullRowSelect = true;
            availButtons.MultiSelect = true;
            availButtons.Sorting = SortOrder.None;

            // TODO build by adding each button NOT in the 'used' list

            // each button NOT in the 'used' list
            ListViewItem lvi2 = new ListViewItem();
            lvi2.ImageIndex = (int) allBtns.btnRename;
            lvi2.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainRename"];
            lvi2.ToolTipText = lvi2.Text;
            lvi2.Tag = allBtns.btnRename;
            availButtons.Items.Add(lvi2);

            ListViewItem lvi3 = new ListViewItem();
            lvi3.ImageIndex = (int)allBtns.btnRecycleBin;
            lvi3.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"];
            lvi3.ToolTipText = lvi3.Text;
            lvi3.Tag = allBtns.btnRecycleBin;
            availButtons.Items.Add(lvi3);

            ListViewItem lvi4 = new ListViewItem();
            lvi4.ImageIndex = (int)allBtns.btnEditImage;
            lvi4.Text = GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"];
            lvi4.ToolTipText = lvi4.Text;
            lvi4.Tag = allBtns.btnEditImage;
            availButtons.Items.Add(lvi4);

            // separator is always available
            ListViewItem lvi = new ListViewItem();
            lvi.Text = _separatorText; 
            lvi.ToolTipText = _separatorText;
            lvi.Tag = allBtns.Separator;
            availButtons.Items.Add(lvi);
        }

        // ReSharper disable InconsistentNaming
        enum allBtns
        {
            Separator = -1,
            // NOTE: the names here MUST match the ToolTipText name in the LangPack,
            // and ideally the field name in frmMain as well.
            btnBack = 0,
            btnNext,
            btnRotateLeft,
            btnRotateRight,
            btnZoomIn,
            btnZoomOut,  // 5
            btnActualSize,
            btnZoomLock,
            btnScaletoWidth,
            btnScaletoHeight,
            btnWindowAutosize, // 10
            btnOpen,
            btnRefresh,
            btnGoto,
            btnThumb,
            btnCheckedBackground, // 15
            btnFullScreen,
            btnSlideShow,
            btnConvert,
            btnPrintImage,
            // NOTE: add new items here, must match order in _images.Images list
            btnRename, // 20
            btnRecycleBin,
            btnEditImage,
        }

        private void btnMoveLeft_Click(object sender, EventArgs e)
        {
            // 'Move' the selected entry in the RIGHT list to the bottom of the LEFT list
            // An exception is 'separator' which always remains available in the left list.

            for (int i = 0; i < usedButtons.SelectedItems.Count; i++)
            {
                var lvi = usedButtons.SelectedItems[i];
                if ((allBtns)lvi.Tag != allBtns.Separator)
                    availButtons.Items.Add(lvi.Clone() as ListViewItem);

                _masterUsedList.Remove(lvi);
            }
            RebuildUsed(-1);
        }

        private void btnMoveRight_Click(object sender, EventArgs e)
        {
            // 'Move' the selected entry in the LEFT list to the bottom of the RIGHT list
            // An exception is 'separator' which always remains available in the left list.

            for (int i = 0; i < availButtons.SelectedItems.Count; i++)
            {
                var lvi = availButtons.SelectedItems[i];
                _masterUsedList.Add(lvi.Clone() as ListViewItem);
            }

            for (int i = availButtons.SelectedItems.Count-1; i >=0; i--)
            {
                var lvi = availButtons.SelectedItems[i];
                if ((allBtns)lvi.Tag != allBtns.Separator)
                    availButtons.Items.Remove(lvi);
            }
            availButtons.SelectedIndices.Clear();
            RebuildUsed(_masterUsedList.Count - 1);
            usedButtons.EnsureVisible(_masterUsedList.Count-1);
        }

        private void RebuildUsed(int toSelect)
        {
            // This is annoying. To show the desired appearance in the listview, we need
            // to use 'SmallIcons' mode [image + text on a single line]. 'SmallIcons' mode
            // will NOT repaint the listview after changes to the Items list!!! Thus, we
            // teardown and rebuild the listview here.
            
            usedButtons.BeginUpdate();
            usedButtons.SelectedIndices.Clear();
            usedButtons.Items.Clear();
            usedButtons.Items.AddRange(_masterUsedList.ToArray());
            if (toSelect >= 0)
                usedButtons.Items[toSelect].Selected = true;
            usedButtons.EndUpdate();
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            // Move an entry in the right list up one position.
            var currentIndex = usedButtons.SelectedItems[0].Index;
            if (currentIndex <= 0) 
                return; // do not wrap around

            var item = usedButtons.Items[currentIndex];

            _masterUsedList.RemoveAt(currentIndex);
            _masterUsedList.Insert(currentIndex-1, item);

            RebuildUsed(currentIndex - 1);

            // Make sure the new position, plus some context, is visible after rebuild
            usedButtons.EnsureVisible(Math.Min(currentIndex + 2, _masterUsedList.Count - 1));
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            // Move an entry in the right list down one position.
            var currentIndex = usedButtons.SelectedItems[0].Index;
            // do NOT wrap around to top
            if (currentIndex >= _masterUsedList.Count - 1) 
                return;

            var item = usedButtons.Items[currentIndex];

            _masterUsedList.RemoveAt(currentIndex);
            _masterUsedList.Insert(currentIndex + 1, item);

            RebuildUsed(currentIndex+1);

            // Make sure the new position, plus some context, is visible after rebuild
            usedButtons.EnsureVisible(Math.Max(currentIndex - 1, 0));
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            // 'Move right' active for at least one selected item in left list.
            // 'Move left' active for at least one selected item in left list.
            // 'Move up/down' active for ONLY one selected item in right list.

            btnMoveRight.Enabled = availButtons.SelectedItems.Count > 0;
            btnMoveLeft.Enabled = usedButtons.SelectedItems.Count > 0;
            btnMoveUp.Enabled = usedButtons.SelectedItems.Count == 1;
            btnMoveDown.Enabled = usedButtons.SelectedItems.Count == 1;
        }

        // The out-of-the-box toolbar button set from V4.5
        private const string _defaultToolbarConfig = "0,1,s,2,3,s,4,5,6,7,8,9,10,s,11,12,13,14,s,15,16,17,18,19,s";

        private static List<allBtns> TranslateFromConfig(string configval)
        {
            // The toolbar config string is stored as a comma-separated list of int values for convenience.
            // Here, we translate that string to a list of button enums.

            List<allBtns> outVal = new List<allBtns>();
            string[] splitvals = configval.Split(new[] { ',' });
            foreach (var splitval in splitvals)
            {
                if (splitval == "s")
                {
                    outVal.Add(allBtns.Separator);
                }
                else
                {
                    int numVal;
                    if (int.TryParse(splitval, out numVal))
                    {
                        try
                        {
                            allBtns enumVal = (allBtns)numVal;
                            outVal.Add(enumVal);
                        }
                        catch (Exception) // when the enumeration value doesn't exist, don't add it!
                        {
                        }
                    }
                }
            }
            return outVal;
        }

        public static List<string> LoadToolbarConfig()
        {
            // This method is used by the main form to actually initialize the toolbar.
            // The toolbar buttons setting is translated to a list of field NAMES from
            // the frmMain class. The need of a separator is indicated by the magic string
            // "_sep_".

            string savedVal = GlobalSetting.GetConfig("ToolbarButtons", _defaultToolbarConfig);
            GlobalSetting.ToolbarButtons = savedVal;

            var xlated = TranslateFromConfig(savedVal);

            List<string> frmMainFieldsByName = new List<string>();
            foreach (var btnEnum in xlated)
            {
                switch (btnEnum)
                {
                case allBtns.Separator:
                    frmMainFieldsByName.Add("_sep_");
                    break;
                default: 
                    // enum name *must* match the field name in frmMain AND the resource name, e.g. "frmMain.btnBack"
                    frmMainFieldsByName.Add(btnEnum.ToString());
                    break;
                }
            }
            return frmMainFieldsByName;
        }

        private void frmCustToolbar_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save the current set of 'used' buttons to the comma-separated list of integers.

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (ListViewItem item in usedButtons.Items)
            {
                string val;
                if ((allBtns) item.Tag == allBtns.Separator)
                    val = "s";
                else
                    val = ((int) item.Tag).ToString();
                if (!first)
                    sb.Append(",");
                first = false;
                sb.Append(val);
            }

            GlobalSetting.ToolbarButtons = sb.ToString();
        }
    }
}
