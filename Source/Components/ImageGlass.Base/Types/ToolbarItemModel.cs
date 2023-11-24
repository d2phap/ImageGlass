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
using ImageGlass.Base.Actions;

namespace ImageGlass.Base;


/// <summary>
/// Toolbar item model
/// </summary>
public record ToolbarItemModel
{
    public ToolbarItemModelType Type { get; set; } = ToolbarItemModelType.Button;

    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public ToolStripItemDisplayStyle DisplayStyle { get; set; } = ToolStripItemDisplayStyle.Image;
    public string CheckableConfigBinding { get; set; } = string.Empty;
    public ToolStripItemAlignment Alignment { get; set; } = ToolStripItemAlignment.Left;

    public string Image { get; set; } = string.Empty;
    public SingleAction OnClick { get; set; } = new();
}

public enum ToolbarItemModelType
{
    Button,
    Separator,
}

public record ToolbarItemTagModel
{
    public SingleAction OnClick { get; set; } = new();
    public string Image { get; set; } = string.Empty;
    public string CheckableConfigBinding { get; set; } = string.Empty;
}


public enum ToolbarAddItemResult
{
    Success,
    ItemExists,
    InvalidModel,
    ThemeIsNull,
}
