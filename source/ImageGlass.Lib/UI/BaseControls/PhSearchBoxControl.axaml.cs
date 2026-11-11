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
using Avalonia;
using Avalonia.Data;
using System;

namespace ImageGlass.UI;

/// <summary>
/// A search text box with an inline search icon and a clear (X) button.
/// </summary>
public partial class PhSearchBoxControl : PhControl
{

    #region Public properties

    /// <summary>
    /// Gets the inner text box (for focus, selection, popup placement, and event wiring).
    /// </summary>
    public PhTextBox TextBox => PART_TextBox;


    /// <summary>
    /// Gets whether the pointer is over the clear (X) button.
    /// </summary>
    public bool IsClearButtonPointerOver => PART_ClearButton.IsPointerOver;


    /// <summary>
    /// Gets, sets the search query text.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<PhSearchBoxControl, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);


    /// <summary>
    /// Gets, sets the placeholder text shown when the box is empty.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<PhSearchBoxControl, string?>(nameof(PlaceholderText));

    #endregion // Public properties


    /// <summary>
    /// Raised whenever the search text changes.
    /// </summary>
    public event EventHandler? TextChanged;


    public PhSearchBoxControl()
    {
        InitializeComponent();

        PART_TextBox.TextChanged += (_, _) => TextChanged?.Invoke(this, EventArgs.Empty);
        PART_ClearButton.Click += (_, _) => Clear();
    }


    #region Public methods

    /// <summary>
    /// Moves keyboard focus to the search box.
    /// </summary>
    public void FocusSearch() => PART_TextBox.Focus();


    /// <summary>
    /// Clears the text and re-focuses the search box.
    /// </summary>
    public void Clear()
    {
        PART_TextBox.Text = string.Empty;
        PART_TextBox.Focus();
    }

    #endregion // Public methods

}
