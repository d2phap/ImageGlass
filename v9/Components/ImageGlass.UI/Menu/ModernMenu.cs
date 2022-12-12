/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using System.ComponentModel;

namespace ImageGlass.UI;


/// <summary>
/// Modern menu with theme supported
/// </summary>
public class ModernMenu : ContextMenuStrip
{
    private IgTheme _theme = new();


    #region Public properties

    /// <summary>
    /// Gets, sets the theme of menu
    /// </summary>
    public IgTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            Renderer = new ModernMenuRenderer(Theme);
        }
    }

    /// <summary>
    /// Gets all items excluding <c>ToolStripSeparator</c> items.
    /// </summary>
    public IEnumerable<ToolStripItem> ActualItems => MenuUtils.GetActualItems(Items);

    /// <summary>
    /// Checks if the menu is open.
    /// </summary>
    public bool IsOpen { get; private set; } = false;

    #endregion


    public ModernMenu(IContainer container) : base(container)
    {
    }


    #region Protected override

    protected override void OnHandleCreated(EventArgs e)
    {
        // apply Windows 11 Corner API
        WindowApi.SetRoundCorner(Handle);

        base.OnHandleCreated(e);
    }

    protected override void OnItemAdded(ToolStripItemEventArgs e)
    {
        base.OnItemAdded(e);

        // manually control the height of menu by disable image scaling
        e.Item.ImageScaling = ToolStripItemImageScaling.None;

        if (e.Item is ToolStripMenuItem item)
        {
            item.DropDown.Renderer = new ModernMenuRenderer(Theme);
        }
    }

    protected override void OnOpening(CancelEventArgs e)
    {
        base.OnOpening(e);

        if (!DesignMode)
        {
            FixGeneralIssues(toFixDpiSize: true, toFixDropdown: true);
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        IsOpen = true;
    }

    protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
    {
        base.OnClosed(e);
        IsOpen = false;
    }

    #endregion


    #region Public function

    /// <summary>
    /// Apply these fixes:
    /// <list type="bullet">
    ///   <item>Menu height when DPI changes</item>
    ///   <item>Windows 11 round border</item>
    ///   <item>Dropdown direction</item>
    /// </list>
    /// to the <see cref="ModernMenu"/> component.
    /// </summary>
    public void FixGeneralIssues(
        bool toFixDpiSize = false,
        bool toFixDropdown = false)
    {
        FixGeneralIssues(this, toFixDpiSize, toFixDropdown);
    }


    /// <summary>
    /// Apply these fixes:
    /// <list type="bullet">
    ///   <item>Menu height when DPI changes</item>
    ///   <item>Windows 11 round border</item>
    ///   <item>Dropdown direction</item>
    /// </list>
    /// to the provided menu component.
    /// </summary>
    /// <param name="items"></param>
    public void FixGeneralIssues(
        ToolStripDropDown menu,
        bool toFixDpiSize = false,
        bool toFixDropdown = false)
    {
        if (!toFixDpiSize && !toFixDropdown) return;

        var allItems = MenuUtils.GetActualItems(menu.Items);
        if (!allItems.Any()) return;

        // standard icon size
        var iconH = DpiApi.Transform(Constants.MENU_ICON_HEIGHT);
        var standardIcon = new Bitmap(iconH, iconH);

        foreach (ToolStripMenuItem item in allItems)
        {
            #region Fix menu height
            if (toFixDpiSize)
            {
                item.ImageScaling = ToolStripItemImageScaling.None;

                if (item.Image is not null)
                {
                    if (item.Image.Height != standardIcon.Height)
                    {
                        item.Image = new Bitmap(item.Image, standardIcon.Width, standardIcon.Height);
                    }
                }
                else
                {
                    item.Image = standardIcon;
                }
            }
            #endregion


            if (item.HasDropDownItems && toFixDropdown)
            {
                // apply corner
                WindowApi.SetRoundCorner(item.DropDown.Handle);

                // fix dropdown direction
                item.DropDownOpening -= Item_DropDownOpening;
                item.DropDownOpening += Item_DropDownOpening;

                // fix dropdown items
                FixGeneralIssues(item.DropDown, toFixDpiSize, toFixDropdown);
            }
        }

    }

    #endregion


    #region Private functions

    private void Item_DropDownOpening(object? sender, EventArgs e)
    {
        var mnuItem = sender as ToolStripMenuItem;
        if (mnuItem is null || !mnuItem.HasDropDownItems)
        {
            return; // not a dropdown item
        }


        #region Fix dropdown direction

        // get position of current menu item
        var pos = new Point(mnuItem.GetCurrentParent().Left, mnuItem.GetCurrentParent().Top);

        // Current bounds of the current monitor
        var currentScreen = Screen.FromPoint(pos);

        // Find the width of sub-menu
        var maxWidth = 0;
        foreach (var subItem in mnuItem.DropDownItems)
        {
            if (subItem is ToolStripMenuItem mnu)
            {
                maxWidth = Math.Max(mnu.Width, maxWidth);
            }
        }
        maxWidth += 10; // Add a little wiggle room

        var farRight = pos.X + Width + maxWidth;
        var farLeft = pos.X - maxWidth;

        // get left and right distance to compare
        var leftGap = farLeft - currentScreen.Bounds.Left;
        var rightGap = currentScreen.Bounds.Right - farRight;

        if (leftGap >= rightGap)
        {
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Left;
        }
        else
        {
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Right;
        }

        #endregion
    }

    #endregion


}