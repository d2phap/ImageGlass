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

This file created by Kevin Routley, aka fire-eggs.
*/
using ImageGlass.Services.Configuration;
using ImageGlass.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

/*
Regarding the fixed width of the listviews: to get a nice icon+text view,
I chose to use SmallIcon mode. Alas, to get a nice single-column display,
it is required to constrain the width of the listview so that it is 
narrower than most? many? of the icon+text width. Thus, the listviews cannot
be docked-to-fill and cannot resize wider when the form resizes.

Changing the Alignment or AutoArrange properties will likely mess this up as well.
*/

// TODO investigate using "List" mode for the ListViews, might solve the fixed width limitation.
// TODO consider cleaning up the multiple copies of the reflection code

/*
How to add a new toolbar button:
1. A SVG image in the theme is necessary.
2. A tooltip string in the language set is necessary.
3. Add a ToolStripButton field to frmMain. Note the field name (e.g. "btnRename").
   The button does NOT need to be added to the toolstrip, or created by the designer.
   It can be created and initialized in code, either by, or before, ConfigToolbar
   is invoked. The image, tooltip and Click event must all be specified!
4. Add a new enum to allBtns below, with the same name as the field name assigned in
   the step above (e.g. "btnRename"). The new enum goes BEFORE the MAX entry.

The new toolbar button will now be available, the user would use the toolbar config
tab to add it to their toolbar settings.
*/


namespace ImageGlass
{
    public partial class frmSetting : Form
    {
        private string _separatorText; // Text used in lists to represent separator bar
        private ImageList _images;
        private List<ListViewItem> _masterUsedList;

        // The out-of-the-box toolbar button set from V4.5
        private const string _defaultToolbarConfig = "0,1,s,2,3,s,4,5,6,7,8,9,10,s,11,12,13,14,s,15,16,17,18,19,s";

        // instance of frmMain, for reflection
        public frmMain MainInstance { get; internal set; }

        private void InitLanguagePackToolbar()
        {
            var lang = GlobalSetting.LangPack.Items;
            _separatorText = lang["frmSetting.txtSeparator"];
            lblToolbar.Text = lang["frmSetting.lblToolbar"];
            lblUsedBtns.Text = lang["frmSetting.lblUsedBtns"];
            lblAvailBtns.Text = lang["frmSetting.lblAvailBtns"];
            lblRestartForChange.Text = lang["frmSetting.lblRestartForChange"];

            tip1.SetToolTip(lblToolbar, lang["frmSetting.lblToolbarTT"]);
            tip1.SetToolTip(btnMoveUp, lang["frmSetting.btnMoveUpTT"]);
            tip1.SetToolTip(btnMoveDown, lang["frmSetting.btnMoveDownTT"]);
            tip1.SetToolTip(btnMoveLeft, lang["frmSetting.btnMoveLeftTT"]);
            tip1.SetToolTip(btnMoveRight, lang["frmSetting.btnMoveRightTT"]);
        }
        
        private void LoadTabToolbar()
        {
            Theme.RenderTheme th = new Theme.RenderTheme();
            th.ApplyTheme(availButtons);
            th.ApplyTheme(usedButtons);

            BuildImageList();
            InitUsedList();
            InitAvailList();
            UpdateButtonState();
        }

        private void ApplyToolbarChanges()
        {
            // User hasn't actually visited the toolbar tab, don't do anything!
            // (Discovered by clicking 'Save' w/o having visited the toolbar tab...
            if (usedButtons.Items.Count == 0 && availButtons.Items.Count == 0)
                return;

            // Save the current set of 'used' buttons to the comma-separated list of integers.

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (ListViewItem item in usedButtons.Items)
            {
                string val;
                if ((allBtns)item.Tag == allBtns.Separator)
                    val = "s";
                else
                    val = ((int)item.Tag).ToString();
                if (!first)
                    sb.Append(",");
                first = false;
                sb.Append(val);
            }

            GlobalSetting.ToolbarButtons = sb.ToString();
        }

        #region Initialize
        private void BuildImageList()
        {
            // Fetch all the toolbar images via reflection from the ToolStripButton
            // instances in the frmMain instance. This is why the enum name MUST
            // match the frmMain field name!

            if (_images != null)
                return;
            _images = new ImageList();
            _images.ColorDepth = ColorDepth.Depth32Bit; // max out image quality

            var iconHeight = ThemeImage.GetCorrectIconHeight();
            _images.ImageSize = new Size(iconHeight, iconHeight); // TODO empirically determined (can get from ImageGlass.Theme)

            Type mainType = typeof(frmMain);
            for (int i=0; i < (int)allBtns.MAX; i++)
            {
                var fieldName = ((allBtns)i).ToString();

                try
                {
                    var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                    ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;
                    _images.Images.Add(val.Image);
                }
                catch (Exception)
                {
                    // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                }
            }
        }

        private void InitUsedList()
        {
            // Build the list of "currently used" toolbar buttons

            usedButtons.View = View.Tile;
            usedButtons.LargeImageList = _images;
            //usedButtons.Width = 200;  // See comment at top of file

            usedButtons.Items.Clear();

            string currentSet = GlobalSetting.ToolbarButtons;
            var enumList = TranslateFromConfig(currentSet);

            _masterUsedList = new List<ListViewItem>(enumList.Count);
            for (int i = 0; i < enumList.Count; i++)
            {
                ListViewItem lvi;

                if (enumList[i] == allBtns.Separator)
                {
                    lvi = BuildSeparatorItem();
                }
                else
                {
                    lvi = BuildItem(enumList[i]);
                }

                _masterUsedList.Add(lvi);
            }

            usedButtons.Items.AddRange(_masterUsedList.ToArray());
        }


        

        private ListViewItem BuildItem(allBtns who)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.ImageIndex = (int)who;
            lvi.Tag = who;
            

            // Fetch the toolbar string via reflection from the ToolStripButton
            // instance in the frmMain instance. This is why the enum name MUST
            // match the frmMain field name!

            var fieldName = who.ToString();
            Type mainType = typeof(frmMain);

            try
            {
                var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;
                lvi.Text = lvi.ToolTipText = val.ToolTipText;
            }
            catch (Exception)
            {
                // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                return null;
            }

            return lvi;
        }

        private ListViewItem BuildSeparatorItem()
        {
            var lvi = new ListViewItem();
            lvi.Text = _separatorText;
            lvi.ToolTipText = _separatorText;
            lvi.Tag = allBtns.Separator;
            return lvi;
        }

        private void InitAvailList()
        {
            // Build the list of "not currently used" toolbar buttons

            availButtons.View = View.Tile;
            availButtons.LargeImageList = _images;
            //availButtons.Width = 200; // See comment at top of file

            availButtons.Items.Clear();

            // Build by adding each button NOT in the 'used' list
            string currentSet = GlobalSetting.ToolbarButtons;
            var enumList = TranslateFromConfig(currentSet);
            for (int i=0; i < (int)allBtns.MAX; i++)
            {
                if (!enumList.Contains((allBtns)i))
                {
                    availButtons.Items.Add(BuildItem((allBtns)i));
                }
            }

            // separator is always available
            availButtons.Items.Add(BuildSeparatorItem());
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

        #endregion

        #region Event Handlers
        private void usedButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void availButtons_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
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

            for (int i = availButtons.SelectedItems.Count - 1; i >= 0; i--)
            {
                var lvi = availButtons.SelectedItems[i];
                if ((allBtns)lvi.Tag != allBtns.Separator)
                    availButtons.Items.Remove(lvi);
            }
            availButtons.SelectedIndices.Clear();
            RebuildUsed(_masterUsedList.Count - 1);
            usedButtons.EnsureVisible(_masterUsedList.Count - 1);
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

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            MoveUsedEntry(+1);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            MoveUsedEntry(-1);
        }

        private void MoveUsedEntry(int delta)
        {
            // Move an item in the 'used' list

            var currentIndex = usedButtons.SelectedItems[0].Index;

            // do not wrap around
            if (delta < 0)
            {
                if (currentIndex <= 0)
                    return;
            }
            else
            {
                if (currentIndex >= _masterUsedList.Count - 1)
                    return; 
            }

            var item = usedButtons.Items[currentIndex];

            _masterUsedList.RemoveAt(currentIndex);
            _masterUsedList.Insert(currentIndex + delta, item);
            RebuildUsed(currentIndex+delta);

            // Make sure the new position, plus some context, is visible after rebuild
            usedButtons.EnsureVisible(Math.Min(currentIndex + 2, _masterUsedList.Count - 1));
        }

        #endregion

        // All the supported toolbar buttons. NOTE: the names here MUST match the field 
        // name in frmMain! Reflection is used to fetch the image and string from the
        // frmMain field.
        //
        // The integer value of the enum is used for storing the config info.
        enum allBtns
        {
            Separator = -1,
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
            MAX // DO NOT ADD ANYTHING AFTER THIS
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

        public static void ConfigToolbar(ToolStrip toolMain, frmMain mainWin)
        {
            toolMain.Items.Clear();

            List<string> tbBtns = LoadToolbarConfig();
            Type mainType = typeof(frmMain);

            foreach (var tbBtn in tbBtns)
            {
                if (tbBtn == "_sep_")
                {
                    ToolStripSeparator sep = new ToolStripSeparator();
                    toolMain.Items.Add(sep);
                }
                else
                {
                    try
                    {
                        var info = mainType.GetField(tbBtn, BindingFlags.Instance | BindingFlags.NonPublic);
                        var val = info.GetValue(mainWin);
                        toolMain.Items.Add(val as ToolStripItem);
                    }
                    catch (Exception)
                    {
                        // GetField may fail if someone renames a toolbar button w/o updating the customize toolbar logic
                    }
                }
            }
        }

#if false

        // The following code is disabled as it was an early attempt to provide toolbar buttons for
        // rename, recycle, and edit. The menu images for those menu entries have been removed,
        // so this code cannot be used until SVG images are provided in the theme for those functions.

        private void MakeMenuButtons()
        {
            // These buttons were not part of the initial toolbar button set. Set up and initialize
            // as if they were created via the designer.

            ComponentResourceManager resources = new ComponentResourceManager(typeof(frmMain));

            string txt = GlobalSetting.LangPack.Items["frmMain.mnuMainMoveToRecycleBin"];
            btnRecycleBin = new ToolStripButton();
            MakeMenuButton(btnRecycleBin, "btnRecycleBin", txt);
            btnRecycleBin.Image = ((Image)(resources.GetObject("mnuMainMoveToRecycleBin.Image")));
            btnRecycleBin.Click += mnuMainMoveToRecycleBin_Click;

            txt = GlobalSetting.LangPack.Items["frmMain.mnuMainRename"];
            btnRename = new ToolStripButton();
            MakeMenuButton(btnRename, "btnRename", txt);
            btnRename.Image = ((Image)(resources.GetObject("mnuMainRename.Image")));
            btnRename.Click += mnuMainRename_Click;

            txt = GlobalSetting.LangPack.Items["frmMain.mnuMainEditImage"];
            btnEditImage = new ToolStripButton();
            MakeMenuButton(btnEditImage, "btnEditImage", txt);
            btnEditImage.Image = ((Image)(resources.GetObject("mnuMainEditImage.Image")));
            btnEditImage.Click += mnuMainEditImage_Click;
        }

        // NOTE: the field names here _must_ match the names in frmCustToolbar.cs/enum allBtns
        private ToolStripButton btnRecycleBin;
        private ToolStripButton btnRename;
        private ToolStripButton btnEditImage;

        private void MakeMenuButton(ToolStripButton btn, string name, string ttText)
        {
            btn.AutoSize = false;
            btn.BackColor = Color.Transparent;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btn.ImageScaling = ToolStripItemImageScaling.None;
            btn.ImageTransparentColor = Color.Magenta;
            btn.Margin = new Padding(3, 0, 0, 0);
            btn.Name = name;
            btn.Size = new Size(28, 28);
            btn.ToolTipText = ttText;
        }

#endif

    }
}