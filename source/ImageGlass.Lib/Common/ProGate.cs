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
using Avalonia.Data.Converters;

namespace ImageGlass.Common;


/// <summary>
/// Binding converters that fold <see cref="Core.IsProEnabled"/> into a config value, so a
/// control receives an already-gated value without referencing the license itself.
/// </summary>
public static class ProGate
{
    /// <summary>
    /// A bool that is true only when the source is true AND Pro is active.
    /// </summary>
    public static readonly IValueConverter Bool =
        new FuncValueConverter<bool, bool>(value => value && Core.IsProEnabled);

    /// <summary>
    /// The source double when Pro is active, otherwise 0.
    /// </summary>
    public static readonly IValueConverter DoubleOrZero =
        new FuncValueConverter<double, double>(value => Core.IsProEnabled ? value : 0d);
}
