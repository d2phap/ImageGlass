
using ImageGlass.Base.WinApi;

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
            timer.Dispose();
            Tooltip.Dispose();
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateAlignment();
    }

    #endregion



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
    }


    /// <summary>
    /// Update the alignment if toolstrip items
    /// </summary>
    public void UpdateAlignment()
    {
        if (Items.Count == 0)
        {
            return;
        }

        var firstBtn = Items[0];
        var defaultMargin = new Padding(3, firstBtn.Margin.Top, firstBtn.Margin.Right, firstBtn.Margin.Bottom);

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
                    toolbarContentWidth += (item.Width + 0);// item.Margin.Right);
                }

                // reset margin
                item.Margin = defaultMargin;
            }

            // if the content cannot fit the toolbar size:
            // (toolbarContentWidth > toolMain.Size.Width)
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