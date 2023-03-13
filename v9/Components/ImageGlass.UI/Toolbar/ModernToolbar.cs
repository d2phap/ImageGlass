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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using System.ComponentModel;

namespace ImageGlass.UI;


/// <summary>
/// Modern toolbar
/// </summary>
public class ModernToolbar : ToolStrip
{
    private ToolbarAlignment _alignment = ToolbarAlignment.Center;
    private int _iconHeight = Constants.TOOLBAR_ICON_HEIGHT;

    private readonly ToolTip _tooltip = new();
    private CancellationTokenSource _tooltipTokenSrc = new();
    private ToolStripItem? _hoveredItem = null;

    private ContextMenuStrip _mainMenu = new();
    private ToolStripButton _mainMenuButton => new()
    {
        Name = "Btn_MainMenu",
        DisplayStyle = ToolStripItemDisplayStyle.Image,
        TextImageRelation = TextImageRelation.ImageBeforeText,
        Text = "[Main menu]",
        ToolTipText = "[Main menu (Alf+F)]",

        // save icon name to load later
        Tag = new ToolbarItemTagModel()
        {
            Image = nameof(Theme.ToolbarIcons.MainMenu),
        },

        Alignment = ToolStripItemAlignment.Right,
        Overflow = ToolStripItemOverflow.Never,
    };


    #region Public properties

    /// <summary>
    /// Enable transparent background.
    /// </summary>
    public bool EnableTransparent { get; set; } = true;

    /// <summary>
    /// Show or hide main menu button of toolbar
    /// </summary>
    public bool ShowMainMenuButton
    {
        get => _mainMenuButton.Visible;
        set => _mainMenuButton.Visible = value;
    }

    /// <summary>
    /// Gets main menu button
    /// </summary>
    public ToolStripButton MainMenuButton => GetItem(_mainMenuButton.Name) ?? _mainMenuButton;

    /// <summary>
    /// Gets, sets main menu
    /// </summary>
    public ContextMenuStrip MainMenu
    {
        get => _mainMenu;
        set
        {
            _mainMenu.Opened -= MainMenu_Opened;
            _mainMenu.Closed -= MainMenu_Closed;

            _mainMenu = value;

            _mainMenu.Opened += MainMenu_Opened;
            _mainMenu.Closed += MainMenu_Closed;
        }
    }

    /// <summary>
    /// Tooltip display text
    /// </summary>
    public string ToolTipText { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets tooltip direction value
    /// </summary>
    public TooltipDirection ToolTipDirection { get; set; } = TooltipDirection.Bottom;

    /// <summary>
    /// Gets, sets value indicates that the tooltip is shown
    /// </summary>
    public bool HideTooltips { get; set; } = false;

    /// <summary>
    /// Gets default gap for sizing calculation
    /// </summary>
    public int DefaultGap => ImageScalingSize.Height / 4;

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

    /// <summary>
    /// Gets, sets theme
    /// </summary>
    public IgTheme? Theme { get; set; }

    /// <summary>
    /// Gets, sets icons height
    /// </summary>
    public int IconHeight
    {
        get => _iconHeight;
        set
        {
            _iconHeight = value;
            ImageScalingSize = new(_iconHeight, _iconHeight);
        }
    }

    /// <summary>
    /// Gets, sets value indicates that the toolstrip will autofocus on hover
    /// </summary>
    public bool AutoFocusOnHover { get; set; } = true;

    #endregion


    #region Protected methods

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (HideTooltips) return;

        var item = GetItemAt(e.Location);

        if (item == null)
        {
            HideItemTooltip();
            _hoveredItem = null;
        }
        else if (item != _hoveredItem)
        {
            _hoveredItem = item;
            ShowItemTooltip(_hoveredItem);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        HideItemTooltip();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        HideItemTooltip();
    }

    //protected override void OnMouseEnter(EventArgs e)
    //{
    //    if (AutoFocusOnHover && CanFocus && !Focused)
    //        Focus();

    //    base.OnMouseEnter(e);
    //}

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        HideItemTooltip();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            OverflowButton.DropDown.Opening -= OverflowDropDown_Opening;
            _tooltip.Dispose();
            _tooltipTokenSrc.Dispose();
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        UpdateAlignment();

        base.OnSizeChanged(e);

        UpdateAlignment();
    }

    protected override Padding DefaultPadding
    {
        get
        {
            return new Padding(DefaultGap, 0, DefaultGap, 0);
        }
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);

        foreach (ToolStripItem item in Items)
        {
            if (item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText
                && item.TextImageRelation == TextImageRelation.ImageBeforeText)
            {
                item.TextAlign = ContentAlignment.MiddleCenter;
                item.ImageAlign = ContentAlignment.MiddleRight;
            }
        }
    }

    protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
    {
        base.OnItemClicked(e);

        // filter out BtnMainMenu
        if (e.ClickedItem.Name == MainMenuButton.Name)
        {
            // on main menu button clicked
            ShowMainMenu();
        }
    }

    #endregion


    public ModernToolbar() : base()
    {
        ShowItemToolTips = false;
        Items.Insert(0, _mainMenuButton);

        // Apply Windows 11 corner API
        WindowApi.SetRoundCorner(OverflowButton.DropDown.Handle);
    }


    #region Private functions
    private void MainMenu_Opened(object? sender, EventArgs e)
    {
        MainMenuButton.Checked = true;
    }
    private void MainMenu_Closed(object? sender, ToolStripDropDownClosedEventArgs e)
    {
        MainMenuButton.Checked = false;
    }


    /// <summary>
    /// Updates overflow button and dropdown
    /// </summary>
    private void UpdateOverflow()
    {
        // overflow size
        OverflowButton.Margin = new(0, 0, DefaultGap, 0);
        OverflowButton.Padding = new(DefaultGap);

        // dropdown size
        OverflowButton.DropDown.AutoSize = false;
        OverflowButton.DropDown.Padding = new(DefaultGap, 0, DefaultGap, 0);

        // fix the size of overflow dropdown
        OverflowButton.DropDown.Opening -= OverflowDropDown_Opening;
        OverflowButton.DropDown.Opening += OverflowDropDown_Opening;

        if (Theme is not null)
        {
            OverflowButton.DropDown.BackColor = BackColor.WithAlpha(255);
            OverflowButton.DropDown.ForeColor = ForeColor;
        }
    }


    private void OverflowDropDown_Opening(object? sender, CancelEventArgs e)
    {
        UpdateOverflowDropdownSize();
    }


    /// <summary>
    /// Update overflow dropdown size
    /// </summary>
    private void UpdateOverflowDropdownSize()
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

    #endregion


    #region Public functions

    /// <summary>
    /// Shows main menu
    /// </summary>
    public void ShowMainMenu()
    {
        var x = MainMenuButton.Bounds.Left + MainMenuButton.Bounds.Width - MainMenu.Width;
        var y = Visible ? Height : 10;

        MainMenu.Show(this, x, y);
    }

    /// <summary>
    /// Hide item's tooltip
    /// </summary>
    public void HideItemTooltip()
    {
        _tooltipTokenSrc.Cancel();
        _tooltip.Hide(this);
    }

    /// <summary>
    /// Shows item tooltip
    /// </summary>
    /// <param name="item"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    public async void ShowItemTooltip(ToolStripItem? item, int duration = 4000, int delay = 400)
    {
        if (item is null || string.IsNullOrEmpty(item.ToolTipText))
            return;

        _tooltipTokenSrc?.Cancel();
        _tooltipTokenSrc = new();

        _tooltip.Hide(this);

        try
        {
            const int TOOLTIP_HEIGHT = 28;
            var tooltipPosY = 0;

            if (ToolTipDirection == TooltipDirection.Top)
            {
                tooltipPosY = item.Bounds.Top - item.Padding.Top - TOOLTIP_HEIGHT;
            }
            else if (ToolTipDirection == TooltipDirection.Bottom)
            {
                tooltipPosY = item.Bounds.Bottom + item.Padding.Bottom;
            }

            // delay
            await Task.Delay(delay, _tooltipTokenSrc.Token);

            // show tooltip
            _tooltip.Show(item.ToolTipText, this, item.Bounds.X, tooltipPosY);

            // duration
            await Task.Delay(duration, _tooltipTokenSrc.Token);
        }
        catch { }

        _tooltip.Hide(this);
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

        if (Alignment == ToolbarAlignment.Center)
        {
            // get the correct content width, excluding the sticky right items
            var toolbarContentWidth = ShowMainMenuButton ? MainMenuButton.Width : 0;
            foreach (ToolStripItem item in Items)
            {
                toolbarContentWidth += item.Width;

                // reset margin
                item.Margin = defaultMargin;
            }


            // if the content cannot fit the toolbar size:
            // if (toolbarContentWidth > Width)
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


    /// <summary>
    /// Update toolbar theme
    /// </summary>
    public void UpdateTheme(int? iconHeight = null)
    {
        if (iconHeight is not null)
        {
            IconHeight = iconHeight.Value;
        }

        if (Theme is null) return;

        // update toolbar theme
        BackColor = Theme.Colors.ToolbarBgColor;
        ForeColor = Theme.Colors.ToolbarTextColor;
        Renderer = new ModernToolbarRenderer(this);

        // Overflow button and Overflow dropdown
        UpdateOverflow();

        // Toolbar items
        foreach (var item in Items)
        {
            if (item.GetType() == typeof(ToolStripSeparator))
            {
                var tItem = item as ToolStripSeparator;
                if (tItem is null) continue;

                tItem.AutoSize = false;
                tItem.Height = IconHeight;
                tItem.Width = IconHeight / 2;
            }

            if (item.GetType() == typeof(ToolStripButton))
            {
                var tItem = item as ToolStripButton;
                if (tItem is null) continue;

                // update font and alignment
                tItem.ForeColor = Theme.Colors.ToolbarTextColor;
                tItem.Padding = new(DefaultGap);
                tItem.Margin = new(0, DefaultGap, DefaultGap / 2, DefaultGap);

                // update item from metadata
                var tagModel = tItem.Tag as ToolbarItemTagModel;
                tItem.Image = Theme.GetToolbarIcon(tagModel?.Image);
            }
        }

        // update items alignment
        UpdateAlignment();
    }


    /// <summary>
    /// Gets item by name
    /// </summary>
    /// <typeparam name="T">Type of ToolstripItem to convert</typeparam>
    /// <param name="name">Name of item</param>
    /// <returns></returns>
    public T? GetItem<T>(string name)
    {
        var item = Items[name];

        if (item is null || item.GetType() != typeof(T))
        {
            return default;
        }

        return (T)Convert.ChangeType(item, typeof(T));
    }


    /// <summary>
    /// Gets ToolStripButton by name
    /// </summary>
    /// <param name="name">Name of item</param>
    public ToolStripButton? GetItem(string name)
    {
        return GetItem<ToolStripButton>(name);
    }


    /// <summary>
    /// Adds new toolbar item.
    /// </summary>
    /// <param name="model">Item model</param>
    /// <param name="position">The location in the items list at which to insert the toolbar item</param>
    /// <param name="modifier">Modifier function to modify item properties</param>
    public ToolbarAddItemResult AddItem(ToolbarItemModel model,
        int? position = null,
        Action<ToolStripItem>? modifier = null)
    {
        position ??= Items.Count;

        // separator
        if (model.Type == ToolbarItemModelType.Separator)
        {
            var sItem = new ToolStripSeparator();
            modifier?.Invoke(sItem);

            Items.Insert(position.Value, sItem);
            return ToolbarAddItemResult.Success;
        }


        if (GetItem<ToolStripItem>(model.Id) is not null)
            return ToolbarAddItemResult.ItemExists;


        // button
        var bItem = new ToolStripButton()
        {
            Name = model.Id,
            DisplayStyle = model.DisplayStyle,
            Text = model.Text,
            ToolTipText = model.Text,
            Alignment = model.Alignment,

            TextImageRelation = TextImageRelation.ImageBeforeText,
            TextAlign = ContentAlignment.MiddleRight,

            // save metadata
            Tag = new ToolbarItemTagModel()
            {
                Image = model.Image,
                CheckableConfigBinding = model.CheckableConfigBinding,
                OnClick = model.OnClick,
            },

            Image = Theme?.GetToolbarIcon(model.Image),
        };

        modifier?.Invoke(bItem);
        Items.Insert(position.Value, bItem);

        return ToolbarAddItemResult.Success;
    }


    /// <summary>
    /// Adds list of toolbar items
    /// </summary>
    /// <param name="list">The list of item models</param>
    /// <param name="modifier">Modifier function to modify item properties</param>
    public void AddItems(IEnumerable<ToolbarItemModel> list,
        Action<ToolStripItem>? modifier = null)
    {
        foreach (var item in list)
        {
            _ = AddItem(item, null, modifier);
        }
    }


    /// <summary>
    /// Clears toolbar items, then adds <see cref="MainMenuButton"/>.
    /// </summary>
    public void ClearItems()
    {
        Items.Clear();
        Items.Insert(0, _mainMenuButton);
    }

    #endregion


}


/// <summary>
/// Toolbar items alignment.
/// </summary>
public enum ToolbarAlignment
{
    Left = 0,
    Center = 1,
}

/// <summary>
/// Tooltip direction of toolbar item.
/// </summary>
public enum TooltipDirection
{
    Top = 0,
    Bottom = 1,
}

