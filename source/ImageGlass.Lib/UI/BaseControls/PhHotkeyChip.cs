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
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI;


/// <summary>
/// Displays a single hotkey string styled as a physical keyboard keycap.
/// </summary>
public class PhHotkeyChip : PhControl
{
    // the darker base peeking below the top face gives the 3D keycap depth
    private const double KEYCAP_LIP = 2;
    private const double GLYPH_DIM_OPACITY = 0.4;
    private static readonly CornerRadius BASE_RADIUS = new(6);
    private static readonly CornerRadius FACE_RADIUS = new(4);

    private readonly Border _face;
    private readonly StackPanel _contentEl;
    private readonly TextBlock _label;
    private readonly PhToolButton _removeBtn;
    private readonly Path _removeIcon;
    private bool _showDelete;
    private PhHotkeyChipTone _tone;


    /// <summary>
    /// Raised when the delete button is clicked.
    /// </summary>
    public event EventHandler? Deleted;


    #region Public Properties

    /// <summary>
    /// Gets, sets the hotkey text shown on the keycap.
    /// </summary>
    public string Text
    {
        get => _label.Text ?? string.Empty;
        set => _label.Text = value;
    }


    /// <summary>
    /// Gets, sets whether the delete button is shown (hidden by default for a read-only keycap).
    /// </summary>
    public bool ShowDelete
    {
        get => _showDelete;
        set
        {
            if (_showDelete == value) return;
            _showDelete = value;
            UpdateDeleteVisibility();
        }
    }


    /// <summary>
    /// Gets, sets the color tone of the key text (normal, accent, or danger).
    /// </summary>
    public PhHotkeyChipTone Tone
    {
        get => _tone;
        set
        {
            if (_tone == value) return;
            _tone = value;
            ApplyTone();
        }
    }

    #endregion // Public Properties


    public PhHotkeyChip() : this(string.Empty)
    {
    }


    public PhHotkeyChip(string text, bool showDelete = false)
    {
        _label = new TextBlock
        {
            Text = text,
            FontFamily = new FontFamily(Const.FONT_CODE),
            FontSize = Const.FONT_SIZE_SMALL,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _removeIcon = new Path
        {
            Width = 10,
            Height = 10,
            Data = Resx.GetIcon(ResxIconId.IconClose),
            Stretch = Stretch.Uniform,
            Opacity = GLYPH_DIM_OPACITY,
        };
        _removeIcon[!Shape.FillProperty] = Resx.CreateBinding(ResxId.TextControlForeground);

        // a PhToolButton gives the square tool-button shape with the app's hover + press feedback
        _removeBtn = new PhToolButton
        {
            Padding = new Thickness(4),
            Content = _removeIcon,
            Cursor = new Cursor(StandardCursorType.Hand),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(_removeBtn, Core.Lang[LangId._Delete]);
        _removeBtn.Click += (_, _) => Deleted?.Invoke(this, EventArgs.Empty);

        // dim the glyph unless the delete button itself is hovered
        _removeBtn.PointerEntered += (_, _) => _removeIcon.Opacity = 1;
        _removeBtn.PointerExited += (_, _) => _removeIcon.Opacity = GLYPH_DIM_OPACITY;

        _contentEl = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _label },
        };

        // top face sits above a 1px outline + a thicker bottom lip of the darker base (the keycap edge)
        _face = new Border
        {
            CornerRadius = FACE_RADIUS,
            Margin = new Thickness(1, 1, 1, KEYCAP_LIP),
            Child = _contentEl,
        };
        _face[!Border.BackgroundProperty] = new DynamicResourceExtension("PhButtonBackground");

        var keycapBase = new Border
        {
            CornerRadius = BASE_RADIUS,
            Child = _face,
        };
        keycapBase[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.TextControlBorderBrush);

        Content = keycapBase;

        _showDelete = showDelete;
        UpdateDeleteVisibility();
        ApplyTone();
    }


    /// <summary>
    /// Applies the current <see cref="Tone"/> to the key text (color + weight), theme-aware.
    /// </summary>
    private void ApplyTone()
    {
        switch (_tone)
        {
            case PhHotkeyChipTone.Accent:
                _label.FontWeight = FontWeight.SemiBold;
                _label[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("PhAccentFill");
                break;

            case PhHotkeyChipTone.Danger:
                _label.FontWeight = FontWeight.SemiBold;
                _label[!TextBlock.ForegroundProperty] = Resx.CreateBinding(ResxId.IG_TextDangerBrush);
                break;

            default:
                _label.FontWeight = FontWeight.Normal;
                _label[!TextBlock.ForegroundProperty] = Resx.CreateBinding(ResxId.TextControlForeground);
                break;
        }
    }


    /// <summary>
    /// Adds/removes the delete button and adjusts the face padding to keep the keycap balanced.
    /// </summary>
    private void UpdateDeleteVisibility()
    {
        if (_showDelete)
        {
            if (!_contentEl.Children.Contains(_removeBtn)) _contentEl.Children.Add(_removeBtn);
            _face.Padding = new Thickness(6, 2, 4, 2);
        }
        else
        {
            _contentEl.Children.Remove(_removeBtn);
            _face.Padding = new Thickness(4, 2);
        }
    }

}



/// <summary>
/// Color tone of a <see cref="PhHotkeyChip"/>'s key text.
/// </summary>
public enum PhHotkeyChipTone
{
    Normal,
    Accent, // customized
    Danger, // conflicting
}
