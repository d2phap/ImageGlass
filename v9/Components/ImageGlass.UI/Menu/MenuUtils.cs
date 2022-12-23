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
using System.ComponentModel;
using System.Reflection;

namespace ImageGlass.UI;

public static class MenuUtils
{

    #region Public static methods

    /// <summary>
    /// This contains a counter to help make names unique
    /// </summary>
    private static int menuNameCounter = 0;

    #endregion


    #region Public static methods

    /// <summary>
    /// Clones the specified source tool strip menu item. 
    /// Thanks to: https://www.codeproject.com/Articles/43472/A-Pretty-Good-Menu-Cloner
    /// </summary>
    /// <param name="srcMenuItem">The source tool strip menu item.</param>
    /// <returns>A cloned version of the toolstrip menu item</returns>
    public static ToolStripMenuItem Clone(this ToolStripMenuItem srcMenuItem)
    {
        var menuItem = new ToolStripMenuItem();

        var propList = from p in typeof(ToolStripMenuItem).GetProperties()
                       let attributes = p.GetCustomAttributes(true)
                       let isBrowseable = (from a in attributes
                                           where a.GetType() == typeof(BrowsableAttribute)
                                           select !(a as BrowsableAttribute).Browsable)
                                                .FirstOrDefault()
                       where !isBrowseable
                            && p.CanRead
                            && p.CanWrite
                            && p.Name != "DropDown"
                       orderby p.Name
                       select p;

        // Copy over using reflections
        foreach (var pInfo in propList)
        {
            var propertyInfoValue = pInfo.GetValue(srcMenuItem, null);
            pInfo.SetValue(menuItem, propertyInfoValue, null);
        }

        // Create a new menu name
        menuItem.Name = srcMenuItem.Name + "-" + menuNameCounter++;

        // Process any other properties
        if (srcMenuItem.ImageIndex != -1)
        {
            menuItem.ImageIndex = srcMenuItem.ImageIndex;
        }

        if (!string.IsNullOrEmpty(srcMenuItem.ImageKey))
        {
            menuItem.ImageKey = srcMenuItem.ImageKey;
        }

        // We need to make this visible 
        menuItem.Visible = true;

        // Recursively clone the drop down list
        foreach (var item in srcMenuItem.DropDownItems)
        {
            ToolStripItem newItem;
            if (item is ToolStripMenuItem)
            {
                newItem = ((ToolStripMenuItem)item).Clone();
            }
            else if (item is ToolStripRadioButtonMenuItem)
            {
                newItem = ((ToolStripRadioButtonMenuItem)item).Clone();
            }
            else if (item is ToolStripSeparator)
            {
                newItem = new ToolStripSeparator();
            }
            else
            {
                throw new NotImplementedException($"Menu item is not " +
                    $"a {nameof(ToolStripMenuItem)} or " +
                    $"a {nameof(ToolStripRadioButtonMenuItem)} or " +
                    $"a {nameof(ToolStripSeparator)}.");
            }

            menuItem.DropDownItems.Add(newItem);
        }

        // The handler list starts empty because we created its parent via a new
        // So this is equivalen to a copy.
        menuItem.AddHandlers(srcMenuItem);

        return menuItem;
    }


    /// <summary>
    /// Adds the handlers from the source component to the destination component
    /// </summary>
    /// <typeparam name="T">An IComponent type</typeparam>
    /// <param name="destinationComponent">The destination component.</param>
    /// <param name="sourceComponent">The source component.</param>
    public static void AddHandlers<T>(this T destinationComponent, T sourceComponent) where T : IComponent
    {
        // If there are other handlers, they will not be erased
        var destEventHandlerList = destinationComponent.GetEventHandlerList();
        var sourceEventHandlerList = sourceComponent.GetEventHandlerList();

        destEventHandlerList.AddHandlers(sourceEventHandlerList);
    }


    /// <summary>
    /// Gets the event handler list from a component
    /// </summary>
    /// <param name="component">The source component.</param>
    /// <returns>The EventHanderList or null if none</returns>
    public static EventHandlerList GetEventHandlerList(this IComponent component)
    {
        var eventsInfo = component.GetType()
            .GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);

        return (EventHandlerList?)eventsInfo.GetValue(component, null);
    }


    /// <summary>
    /// Gets all items excluding <c>ToolStripSeparator</c> items. 
    /// </summary>
    /// <param name="coll"></param>
    /// <returns></returns>
    public static IEnumerable<ToolStripItem> GetActualItems(ToolStripItemCollection coll)
    {
        return coll
            .Cast<ToolStripItem>()
            .Where(item => item.GetType() != typeof(ToolStripSeparator));
    }

    #endregion

}
