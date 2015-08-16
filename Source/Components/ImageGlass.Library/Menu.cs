using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ImageGlass.Library
{
    public static class Menu
    {
        /// <summary>
        /// Clone ToolStripMenu item
        /// </summary>
        /// <param name="mnu">ToolStripMenuItem</param>
        /// <returns></returns>
        public static ToolStripMenuItem Clone(ToolStripMenuItem mnu)
        {
            ToolStripMenuItem m = new ToolStripMenuItem();

            //clone all events
            var eventsField = typeof(Component).GetField("events", BindingFlags.NonPublic | BindingFlags.Instance);
            var eventHandlerList = eventsField.GetValue(mnu);
            eventsField.SetValue(m, eventHandlerList);

            //clone all properties
            m.AccessibleName = mnu.AccessibleName;
            m.AccessibleRole = mnu.AccessibleRole;
            m.Alignment = mnu.Alignment;
            m.AllowDrop = mnu.AllowDrop;
            m.Anchor = mnu.Anchor;
            m.AutoSize = mnu.AutoSize;
            m.AutoToolTip = mnu.AutoToolTip;
            m.BackColor = Color.Transparent;
            m.BackgroundImage = mnu.BackgroundImage;
            m.BackgroundImageLayout = mnu.BackgroundImageLayout;
            m.Checked = mnu.Checked;
            m.CheckOnClick = mnu.CheckOnClick;
            m.CheckState = mnu.CheckState;
            m.DisplayStyle = mnu.DisplayStyle;
            m.Dock = mnu.Dock;
            m.DoubleClickEnabled = mnu.DoubleClickEnabled;
            m.Enabled = mnu.Enabled;
            m.Font = mnu.Font;
            m.ForeColor = mnu.ForeColor;
            m.Image = mnu.Image;
            m.ImageAlign = mnu.ImageAlign;
            m.ImageScaling = mnu.ImageScaling;
            m.ImageTransparentColor = mnu.ImageTransparentColor;
            m.Margin = mnu.Margin;
            m.MergeAction = mnu.MergeAction;
            m.MergeIndex = mnu.MergeIndex;
            m.Name = mnu.Name;
            m.Overflow = mnu.Overflow;
            m.Padding = mnu.Padding;
            m.RightToLeft = mnu.RightToLeft;

            m.ShortcutKeys = mnu.ShortcutKeys;
            m.ShowShortcutKeys = mnu.ShowShortcutKeys;
            m.Tag = mnu.Tag;
            m.Text = mnu.Text;
            m.TextAlign = mnu.TextAlign;
            m.TextDirection = mnu.TextDirection;
            m.TextImageRelation = mnu.TextImageRelation;
            m.ToolTipText = mnu.ToolTipText;

            m.Available = mnu.Available;

            if (!mnu.AutoSize)
            {
                m.Size = mnu.Size;
            }
            return m;
        }
    }
}
