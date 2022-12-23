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
namespace ImageGlass.Gallery;


/// <summary>
/// Represents the details of a mouse hit test.
/// </summary>
public class HitInfo
{

    #region Properties
    /// <summary>
    /// Gets whether an item is under the hit point.
    /// </summary>
    public bool IsItemHit => ItemIndex != -1;

    /// <summary>
    /// Gets whether an item checkbox is under the hit point.
    /// </summary>
    public bool IsCheckBoxHit { get; private set; }

    /// <summary>
    /// Gets whether the file icon is under the hit point.
    /// </summary>
    public bool IsFileIconHit { get; private set; }

    /// <summary>
    /// Gets the index of the item under the hit point.
    /// </summary>
    public int ItemIndex { get; private set; }

    /// <summary>
    /// Gets whether the hit point is inside the item area.
    /// </summary>
    public bool IsInItemArea { get; private set; }

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the HitInfo class.
    /// </summary>
    /// <param name="itemIndex">Index of the item.</param>
    /// <param name="checkBoxHit">if set to true the mouse cursor is over a checkbox.</param>
    /// <param name="fileIconHit">if set to true the mouse cursor is over a file icon.</param>
    /// <param name="inItemArea">if set to true the mouse is in the item area.</param>
    private HitInfo(int itemIndex, bool checkBoxHit, bool fileIconHit, bool inItemArea)
    {
        ItemIndex = itemIndex;
        IsCheckBoxHit = checkBoxHit;
        IsFileIconHit = fileIconHit;

        IsInItemArea = inItemArea;
    }

    /// <summary>
    /// Initializes a new instance of the HitInfo class.
    /// </summary>
    /// <param name="itemIndex">Index of the item.</param>
    /// <param name="checkBoxHit">if set to true the mouse cursor is over a checkbox.</param>
    /// <param name="fileIconHit">if set to true the mouse cursor is over a file icon.</param>
    internal HitInfo(int itemIndex, bool checkBoxHit, bool fileIconHit) : this(itemIndex, checkBoxHit, fileIconHit, true) { }

    #endregion

}
