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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

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

            tip1.SetToolTip(lblToolbar, lang["frmSetting.lblToolbarTT"]);
            tip1.SetToolTip(btnMoveUp, lang["frmSetting.btnMoveUpTT"]);
            tip1.SetToolTip(btnMoveDown, lang["frmSetting.btnMoveDownTT"]);
            tip1.SetToolTip(btnMoveLeft, lang["frmSetting.btnMoveLeftTT"]);
            tip1.SetToolTip(btnMoveRight, lang["frmSetting.btnMoveRightTT"]);
        }

        private void LoadTabToolbar()
        {
            BuildImageList();
            InitUsedList();
            InitAvailList();
            UpdateButtonState();
        }

        private void ApplyToolbarChanges()
        {
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
            _images.ImageSize = new Size(20, 20); // TODO empirically determined

            Type mainType = typeof(frmMain);
            for (int i=0; i < (int)allBtns.MAX; i++)
            {
                var fieldName = ((allBtns)i).ToString();

                var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                // TODO if info is null, something is out-of-whack; how to recover?
                ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;

                _images.Images.Add(val.Image);
            }
        }

        private void InitUsedList()
        {
            // Build the list of "currently used" toolbar buttons

            usedButtons.View = View.SmallIcon;
            usedButtons.SmallImageList = _images;
            usedButtons.FullRowSelect = true;
            usedButtons.MultiSelect = true;
            usedButtons.Sorting = SortOrder.None;

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

            var info = mainType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            // TODO if info is null, something is out-of-whack; how to recover?
            ToolStripButton val = info.GetValue(MainInstance) as ToolStripButton;

            lvi.Text = lvi.ToolTipText = val.ToolTipText;
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

            availButtons.View = View.List;
            availButtons.SmallImageList = _images;
            availButtons.FullRowSelect = true;
            availButtons.MultiSelect = true;
            availButtons.Sorting = SortOrder.None;

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

        enum allBtns
        {
            Separator = -1,
            // NOTE: the names here MUST match the ToolTipText name in the LangPack,
            // and the field name in frmMain as well. If the two don't match, code
            // needs to translate between.
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
            //btnRename, // 20
            //btnRecycleBin,
            //btnEditImage,
            MAX // DO NOT ADD ANYTHING AFTER THIS
        }

    }
}