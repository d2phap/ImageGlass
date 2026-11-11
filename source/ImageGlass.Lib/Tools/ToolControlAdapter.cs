/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using ImageGlass.UI.Viewer;

namespace ImageGlass.Tools;


/// <summary>
/// Wraps a <see cref="ToolControlFactory"/> as an <see cref="ITool"/> for registry compatibility.
/// The factory is invoked each time a hosted tool needs to be opened.
/// </summary>
public sealed class ToolControlAdapter : ITool
{
    private readonly ToolControlFactory _factory;

    public string ToolId { get; }
    public bool IsHosted => true;

    // Settings and Viewer are not used on the adapter itself;
    // they are set on the IToolControl instance created by the factory.
    public object? Settings => null;
    public ViewerControl Viewer { get; set; } = null!;

    public ToolControlAdapter(string toolId, ToolControlFactory factory)
    {
        ToolId = toolId;
        _factory = factory;
    }

    /// <summary>
    /// Creates a new hosted tool instance.
    /// </summary>
    public IToolControl CreateToolControl(ViewerControl viewer) => _factory(viewer);
}