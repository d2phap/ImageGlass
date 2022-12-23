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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using System.Windows.Forms.VisualStyles;

namespace ImageGlass.Gallery;


#region DefaultRenderer
/// <summary>
/// The default renderer.
/// </summary>
public class DefaultRenderer : StyleRenderer
{
    /// <summary>
    /// Initializes a new instance of the DefaultRenderer class.
    /// </summary>
    public DefaultRenderer() { }
}
#endregion


#region SystemRenderer
/// <summary>
/// Displays the control in the current system theme.
/// This renderer cannot be themed.
/// </summary>
public class SystemRenderer : StyleRenderer
{
    // Check boxes
    private readonly VisualStyleRenderer? rCheckedNormal = null;
    private readonly VisualStyleRenderer? rUncheckedNormal = null;
    private readonly VisualStyleRenderer? rCheckedDisabled = null;
    private readonly VisualStyleRenderer? rUncheckedDisabled = null;

    // File icons
    private VisualStyleRenderer? rFileIcon = null;

    // Items
    private readonly VisualStyleRenderer? rItemNormal = null;
    private readonly VisualStyleRenderer? rItemHovered = null;
    private readonly VisualStyleRenderer? rItemPressed = null;
    private readonly VisualStyleRenderer? rItemSelected = null;
    private readonly VisualStyleRenderer? rItemHoveredSelected = null;
    private readonly VisualStyleRenderer? rItemSelectedHidden = null;
    private readonly VisualStyleRenderer? rItemDisabled = null;

    /// <summary>
    /// Gets whether visual styles are supported.
    /// </summary>
    public bool VisualStylesEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this renderer can apply custom colors.
    /// </summary>
    /// <value></value>
    public override bool CanApplyColors { get { return false; } }

    /// <summary>
    /// Initializes a new instance of the ThemeRenderer class.
    /// </summary>
    public SystemRenderer()
    {
        VisualStylesEnabled = Application.RenderWithVisualStyles;

        // Create renderers
        if (VisualStylesEnabled)
        {
            // See https://docs.microsoft.com/en-us/windows/win32/controls/parts-and-states
            // for part and state codes used below.

            // Check boxes
            rCheckedNormal = GetRenderer(VisualStyleElement.Button.CheckBox.CheckedNormal);
            rUncheckedNormal = GetRenderer(VisualStyleElement.Button.CheckBox.UncheckedNormal);
            rCheckedDisabled = GetRenderer(VisualStyleElement.Button.CheckBox.CheckedDisabled);
            rUncheckedDisabled = GetRenderer(VisualStyleElement.Button.CheckBox.UncheckedDisabled);

            // File icons
            rFileIcon = GetRenderer(VisualStyleElement.Button.PushButton.Normal);

            // Items
            rItemNormal = GetRenderer("Explorer::ListView", 1, 1);
            rItemHovered = GetRenderer("Explorer::ListView", 1, 2);
            rItemPressed = GetRenderer("Explorer::ListView", 1, 3);
            rItemSelected = GetRenderer("Explorer::ListView", 1, 3);
            rItemHoveredSelected = GetRenderer("Explorer::ListView", 1, 6);
            rItemSelectedHidden = GetRenderer("Explorer::ListView", 1, 5);
            rItemDisabled = GetRenderer("Explorer::ListView", 1, 4);
        }
    }

    /// <summary>
    /// Returns a renderer for the given element.
    /// </summary>
    private static VisualStyleRenderer? GetRenderer(VisualStyleElement e)
    {
        if (VisualStyleRenderer.IsElementDefined(e))
            return new VisualStyleRenderer(e);
        else
            return null;
    }

    /// <summary>
    /// Returns a renderer for the given element.
    /// </summary>
    private static VisualStyleRenderer? GetRenderer(string className, int part, int state)
    {
        var e = VisualStyleElement.CreateElement(className, part, state);
        if (VisualStyleRenderer.IsElementDefined(e))
            return new VisualStyleRenderer(e);
        else
            return null;
    }

    /// <summary>
    /// Draws the checkbox icon for the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The <see cref="ImageGalleryItem"/> to draw.</param>
    /// <param name="bounds">The bounding rectangle of the checkbox in client coordinates.</param>
    public override void DrawCheckBox(Graphics g, ImageGalleryItem item, Rectangle bounds)
    {
        VisualStyleRenderer? renderer;
        if (item.Enabled)
        {
            if (item.Checked)
                renderer = rCheckedNormal;
            else
                renderer = rUncheckedNormal;
        }
        else
        {
            if (item.Checked)
                renderer = rCheckedDisabled;
            else
                renderer = rUncheckedDisabled;
        }

        if (VisualStylesEnabled && renderer != null)
            renderer.DrawBackground(g, bounds, bounds);
        else
            base.DrawCheckBox(g, item, bounds);
    }

    /// <summary>
    /// Draws the file icon for the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The <see cref="ImageGalleryItem"/> to draw.</param>
    /// <param name="bounds">The bounding rectangle of the file icon in client coordinates.</param>
    public override void DrawFileIcon(Graphics g, ImageGalleryItem item, Rectangle bounds)
    {
        var icon = item.GetCachedImage(CachedImageType.SmallIcon);

        if (icon != null && VisualStylesEnabled && rFileIcon != null)
            rFileIcon.DrawImage(g, bounds, icon);
        else
            base.DrawFileIcon(g, item, bounds);
    }
    

    /// <summary>
    /// [IG_CHANGE] Returns item size for the given view mode.
    /// </summary>
    /// <param name="view">The view mode for which the measurement should be made.</param>
    /// <returns>The item size.</returns>
    public override Size MeasureItem(View view)
    {
        Size sz = base.MeasureItem(view);

        //sz.Width += 6;
        //sz.Height += 6;
        int textHeight = ImageGalleryOwner.Font.Height;

        sz.Width += textHeight * 2 / 5;
        sz.Height -= textHeight / 2;

        return sz;
    }

    /// <summary>
    /// [IG_CHANGE] Draws the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The <see cref="ImageGalleryItem"/> to draw.</param>
    /// <param name="state">The current view state of item.</param>
    /// <param name="bounds">The bounding rectangle of item in client coordinates.</param>
    public override void DrawItem(Graphics g, ImageGalleryItem item, ItemState state, Rectangle bounds)
    {
        VisualStyleRenderer? rBack;

        if (!ImageGalleryOwner.Enabled)
        {
            rBack = rItemSelectedHidden;
        }

        if ((state & ItemState.Disabled) != ItemState.None)
        {
            rBack = rItemDisabled;
        }
        else if (!ImageGalleryOwner.Focused && ((state & ItemState.Selected) != ItemState.None))
        {
            rBack = rItemSelectedHidden;
        }
        else if (((state & ItemState.Selected) != ItemState.None) && ((state & ItemState.Hovered) != ItemState.None))
        {
            rBack = rItemHoveredSelected;
        }
        else if ((state & ItemState.Selected) != ItemState.None)
        {
            rBack = rItemSelected;
        }
        else if ((state & ItemState.Pressed) != ItemState.None)
        {
            rBack = rItemPressed;
        }
        else if ((state & ItemState.Hovered) != ItemState.None)
        {
            rBack = rItemHovered;
        }
        else
        {
            rBack = rItemNormal;
        }

        if (VisualStylesEnabled && rBack != null)
        {
            // Do not draw the background of normal items
            if (((state & ItemState.Hovered) != ItemState.None) || ((state & ItemState.Selected) != ItemState.None))
                rBack.DrawBackground(g, bounds, bounds);

            // Size itemPadding = new Size(7, 7);
            var itemPadding = new Size(5, 5);

            // Draw the image
            var img = item.GetCachedImage(CachedImageType.Thumbnail);
            if (img != null)
            {
                var pos = Utility.GetSizedImageBounds(img,
                    new Rectangle(bounds.Location + itemPadding,
                    new Size(bounds.Width - 2 * itemPadding.Width, bounds.Height - 2 * itemPadding.Width)));

                // Image background
                var imgback = pos;
                imgback.Inflate(3, 3);

                // Image
                g.DrawImage(img, pos);
            }

            // Focus rectangle
            if (ImageGalleryOwner.Focused && ((state & ItemState.Focused) != ItemState.None))
            {
                Rectangle focusBounds = bounds;
                focusBounds.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(g, focusBounds);
            }

        }
        else
        {
            base.DrawItem(g, item, state, bounds);
        }
    }
    
}
#endregion

