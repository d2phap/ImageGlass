/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

    /// <summary>
    /// Gets, sets the current DPI which is manually update from parent.
    /// <para>
    /// <c>Note**:</c><br/>
    /// The <see cref="DeviceDpi"/> prop of the menu <c><b>is not updated</b></c> when DPI changed.<br/>
    /// See the issue at: <see href="https://github.com/dotnet/winforms/issues/4898"/>.
    /// </para>
    /// </summary>
    public int CurrentDpi { get; set; } = 96;


    /// <summary>
    /// Gets the initial DPI when the menu is first opended.
    /// <para>
    /// <c>Note**:</c><br/>
    /// The <see cref="DeviceDpi"/> prop of the menu <c><b>is not updated</b></c> when DPI changed.<br/>
    /// See the issue at: <see href="https://github.com/dotnet/winforms/issues/4898"/>.
    /// </para>
    /// </summary>
    public new int DeviceDpi { get => base.DeviceDpi; }


    #endregion


    public ModernMenu(IContainer container) : base(container)
    {
    }


    // Protected override
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
            FixGeneralIssues(this, Constants.MENU_ICON_HEIGHT);
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

    #endregion // Protected override


    // Public functions
    #region Public functions

    /// <summary>
    /// Apply these fixes:
    /// <list type="bullet">
    ///   <item>DPI scaling for menu height, font size</item>
    ///   <item>Windows 11 round border</item>
    ///   <item>Dropdown direction</item>
    /// </list>
    /// </summary>
    /// <param name="items"></param>
    public void FixGeneralIssues(
        ToolStripDropDown menu,
        float originalIconSize)
    {
        var dpiScale = CurrentDpi / 96f;
        var currentFontSize = SystemInformation.MenuFont.SizeInPoints;

        // standard menu height
        var fontSizeDiff = Math.Max(0, currentFontSize - 9 - dpiScale) * dpiScale;
        var iconH = (originalIconSize + fontSizeDiff) * dpiScale;


        for (int i = 0; i < menu.Items.Count; i++)
        {
            if (menu.Items[i] is not ToolStripMenuItem mnuItem) continue;

            // fix font size scaling
            mnuItem.Font = new Font(Font.FontFamily, currentFontSize);

            // Fix menu height
            #region Fix menu height

            mnuItem.ImageScaling = ToolStripItemImageScaling.None;

            if (mnuItem.Image is not null)
            {
                if (mnuItem.Image.Height != iconH)
                {
                    mnuItem.Image = new Bitmap(mnuItem.Image, (int)iconH, (int)iconH);
                }
            }
            else
            {
                mnuItem.Image = new Bitmap((int)iconH, (int)iconH);
            }

            #endregion


            if (mnuItem.HasDropDownItems)
            {
                // apply corner
                WindowApi.SetRoundCorner(mnuItem.DropDown.Handle);

                // fix dropdown direction
                mnuItem.DropDownOpening -= Item_DropDownOpening;
                mnuItem.DropDownOpening += Item_DropDownOpening;

                // fix dropdown items
                FixGeneralIssues(mnuItem.DropDown, Constants.MENU_ICON_HEIGHT * 0.9f);
            }
        }

    }


    /// <summary>
    /// Finds <see cref="ToolStripMenuItem"/> item by name.
    /// </summary>
    public ToolStripMenuItem? FindMenuItem(string menuName)
    {
        if (string.IsNullOrEmpty(menuName)) return null;


        ToolStripMenuItem? FindMenuRecursive(ToolStripMenuItem menuItem)
        {
            if (menuItem.Name == menuName) return menuItem;

            foreach (var child in menuItem.DropDownItems.OfType<ToolStripMenuItem>())
            {
                var item = FindMenuRecursive(child);
                if (item != null) return item;
            }

            return null;
        }


        foreach (ToolStripMenuItem mnuItem in ActualItems)
        {
            var item = FindMenuRecursive(mnuItem);
            if (item != null) return item;
        }

        return null;
    }

    #endregion // Public functions


    // Private functions
    #region Private functions

    private void Item_DropDownOpening(object? sender, EventArgs e)
    {
        var mnuItem = sender as ToolStripMenuItem;
        // not a dropdown item
        if (mnuItem is null || !mnuItem.HasDropDownItems) return;


        #region Fix dropdown direction

        // current bounds of the current monitor
        var workingArea = Screen.FromControl(this).WorkingArea;

        // get width of all submenu dropdowns
        var openingDropdownWidth = GetWidthOfAllOpeningDropdowns(mnuItem);

        // get the ideal right bound of the last submenu dropdown
        var lastSubmenuRight = Left + Width + openingDropdownWidth;

        // if cannot fit the last submenu into right side
        if (lastSubmenuRight > workingArea.Right)
        {
            // show it on the left
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Left;
        }
        else
        {
            mnuItem.DropDownDirection = ToolStripDropDownDirection.Right;
        }

        #endregion // Fix dropdown direction

    }


    /// <summary>
    /// Gets the total width of all opening submenu dropdowns
    /// </summary>
    private int GetWidthOfAllOpeningDropdowns(ToolStripMenuItem mnuItem)
    {
        if (mnuItem is not ToolStripMenuItem) return 0;
        var parent = mnuItem.OwnerItem as ToolStripMenuItem;

        return mnuItem.DropDown.Width + GetWidthOfAllOpeningDropdowns(parent);
    }

    #endregion // Private functions


}