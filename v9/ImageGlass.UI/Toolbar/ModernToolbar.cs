/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using ImageGlass.Base.WinApi;
using System.ComponentModel;

namespace ImageGlass.UI.Toolbar;

public enum ToolbarAlignment
{
    LEFT = 0,
    CENTER = 1,
}

public class ModernToolbar : ToolStrip
{
    private ToolStripItem mouseOverItem;
    private Point mouseOverPoint;
    private readonly System.Windows.Forms.Timer timer;
    private ToolTip _tooltip;
    public int ToolTipInterval = 4000;
    public string ToolTipText;

    /// <summary>
    /// Gets, sets value indicates that the tooltip direction is top or bottom
    /// </summary>
    public bool ToolTipShowUp { get; set; } = false;


    /// <summary>
    /// Gets, sets value indicates that the tooltip is shown
    /// </summary>
    public bool HideTooltips { get; set; } = false;

    private ToolbarAlignment _alignment;

    private ToolTip Tooltip
    {
        get
        {
            if (_tooltip == null)
            {
                _tooltip = new ToolTip();
                Tooltip.AutomaticDelay = 2000;
                Tooltip.InitialDelay = 2000;
            }
            return _tooltip;
        }
    }

    /// <summary>
    /// Gets, sets items alignment
    /// </summary>
    public ToolbarAlignment Alignment
    {
        get => _alignment;
        set
        {
            _alignment = value;

            UpdateAlignment();
        }
    }


    #region Protected methods
    protected override void OnMouseMove(MouseEventArgs mea)
    {
        base.OnMouseMove(mea);

        if (HideTooltips) return;

        var newMouseOverItem = GetItemAt(mea.Location);
        if (mouseOverItem != newMouseOverItem ||
            (Math.Abs(mouseOverPoint.X - mea.X) > SystemInformation.MouseHoverSize.Width || (Math.Abs(mouseOverPoint.Y - mea.Y) > SystemInformation.MouseHoverSize.Height)))
        {
            mouseOverItem = newMouseOverItem;
            mouseOverPoint = mea.Location;
            Tooltip.Hide(this);
            timer.Stop();
            timer.Start();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        var newMouseOverItem = GetItemAt(e.Location);
        if (newMouseOverItem != null)
        {
            Tooltip.Hide(this);
        }
    }

    protected override void OnMouseUp(MouseEventArgs mea)
    {
        base.OnMouseUp(mea);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var newMouseOverItem = GetItemAt(mea.Location);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        timer.Stop();
        Tooltip.Hide(this);
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        timer.Stop();
        try
        {
            Point currentMouseOverPoint;
            if (ToolTipShowUp)
            {
                currentMouseOverPoint = PointToClient(new(MousePosition.X, MousePosition.Y - Cursor.Current.Size.Height + Cursor.Current.HotSpot.Y - Height / 2));
            }
            else
            {
                currentMouseOverPoint = PointToClient(new(MousePosition.X, MousePosition.Y + Cursor.Current.Size.Height - Cursor.Current.HotSpot.Y));
            }

            if (mouseOverItem == null)
            {
                if (!string.IsNullOrEmpty(ToolTipText))
                {
                    Tooltip.Show(ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                }
            }
            // TODO: revisit this; toolbar buttons like to disappear, if changed.
            else if (
                ((mouseOverItem is not ToolStripDropDownButton
                    && mouseOverItem is not ToolStripSplitButton)
                || (mouseOverItem is ToolStripDropDownButton
                    && !((ToolStripDropDownButton)mouseOverItem).DropDown.Visible)
                || (mouseOverItem is ToolStripSplitButton
                    && !((ToolStripSplitButton)mouseOverItem).DropDown.Visible))
                && !string.IsNullOrEmpty(mouseOverItem.ToolTipText)
                && Tooltip != null)
            {
                Tooltip.Show(mouseOverItem.ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
            }
        }
        catch { }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            OverflowButton.DropDown.Opening -= OverflowDropDown_Opening;
            timer.Dispose();
            Tooltip.Dispose();
        }
    }

    #endregion


    protected override void OnSizeChanged(EventArgs e)
    {
        UpdateAlignment();
        base.OnSizeChanged(e);

        UpdateAlignment();
    }


    public ModernToolbar() : base()
    {
        ShowItemToolTips = false;
        timer = new()
        {
            Enabled = false,
            Interval = 200 // KBR enforce long initial time SystemInformation.MouseHoverTime;
        };
        timer.Tick += Timer_Tick;

        // Apply Windows 11 corner API
        CornerApi.ApplyCorner(OverflowButton.DropDown.Handle);


        // Set default style for overflow button and dropdown
        OverflowButton.DropDown.AutoSize = false;
        OverflowButton.Margin = Constants.TOOLBAR_BTN_MARGIN;
        OverflowButton.Padding = new(
            OverflowButton.Height / 2,
            0,
            OverflowButton.Height / 2,
            0);

        OverflowButton.DropDown.Padding = new(
            Constants.TOOLBAR_BTN_MARGIN.Top,
            0,
            Constants.TOOLBAR_BTN_MARGIN.Bottom,
            0);

        // fix the size of overflow dropdown
        OverflowButton.DropDown.Opening += OverflowDropDown_Opening;
    }

    private void OverflowDropDown_Opening(object? sender, CancelEventArgs e)
    {
        UpdateOverflowDropdownSize();
    }



    /// <summary>
    /// Update overflow dropdown size
    /// </summary>
    public void UpdateOverflowDropdownSize()
    {
        var maxItemHeight = 0;
        var fullDropdownWidth = OverflowButton.DropDown.Padding.Left + OverflowButton.DropDown.Padding.Right;

        foreach (ToolStripItem item in Items)
        {
            if (!item.IsOnDropDown) continue;

            fullDropdownWidth += item.Width
                + item.Margin.Left
                + item.Margin.Right;

            maxItemHeight = Math.Max(maxItemHeight, item.Height + item.Margin.Top + item.Margin.Bottom);
        }

        var maxDropdownWidth = Screen.FromControl(this).WorkingArea.Width / 2;
        var dropdownWidth = Math.Min(fullDropdownWidth, maxDropdownWidth);
        var dropdownHeight = (int)(Math.Ceiling(fullDropdownWidth * 1f / dropdownWidth)
            * maxItemHeight
            + OverflowButton.DropDown.Padding.Top
            + OverflowButton.DropDown.Padding.Bottom);

        OverflowButton.DropDown.Width = dropdownWidth;
        OverflowButton.DropDown.Height = dropdownHeight;
    }


    /// <summary>
    /// Update the alignment if toolstrip items
    /// </summary>
    public void UpdateAlignment()
    {
        if (Items.Count < 1) return;

        // find the first left-aligned button
        ToolStripItem? firstBtn = null;
        foreach (ToolStripItem item in Items)
        {
            if (item.Alignment == ToolStripItemAlignment.Left)
            {
                firstBtn = item;
                break;
            }
        }

        if (firstBtn == null) return;


        var defaultMargin = new Padding(0, firstBtn.Margin.Top, firstBtn.Margin.Right, firstBtn.Margin.Bottom);

        // reset the alignment to left
        firstBtn.Margin = defaultMargin;

        if (Alignment == ToolbarAlignment.CENTER)
        {
            // get the correct content width, excluding the sticky right items
            var toolbarContentWidth = 0;
            foreach (ToolStripItem item in Items)
            {
                if (item.Alignment == ToolStripItemAlignment.Right)
                {
                    toolbarContentWidth += item.Width * 2;
                }
                else
                {
                    toolbarContentWidth += item.Width;
                }

                // reset margin
                item.Margin = defaultMargin;
            }


            // if the content cannot fit the toolbar size:
            //if (toolbarContentWidth > Width)
            if (OverflowButton.Visible)
            {
                // align left
                firstBtn.Margin = defaultMargin;
            }
            else
            {
                // the default margin (left alignment)
                var margin = defaultMargin;

                // get the gap of content width and toolbar width
                var gap = Math.Abs(Width - toolbarContentWidth);

                // update the left margin value
                margin.Left = gap / 2;

                // align the first item
                firstBtn.Margin = margin;
            }
        }
    }
}