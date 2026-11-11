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
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System.Linq;

namespace ImageGlass.UI;

/// <summary>
/// A read-only preview of the command an action.
/// </summary>
public class PhCommandPreview : PhControl
{
    // sample path used to render the preview
    private string PreviewFakePath { get; } = BHelper.OS == OSType.Windows
        ? @"C:\sample\photo.webp"
        : "/sample/photo.webp";

    private readonly SelectableTextBlock _previewText;


    /// <summary>
    /// Gets, sets the executable (an <c>IG_</c> method, a menu item name, or a file path).
    /// </summary>
    public string? Executable
    {
        get => GetValue(ExecutableProperty);
        set => SetValue(ExecutableProperty, value);
    }
    public static readonly StyledProperty<string?> ExecutableProperty =
        AvaloniaProperty.Register<PhCommandPreview, string?>(nameof(Executable));


    /// <summary>
    /// Gets, sets the argument string (supports the <c>&lt;file&gt;</c> macro).
    /// </summary>
    public string? Argument
    {
        get => GetValue(ArgumentProperty);
        set => SetValue(ArgumentProperty, value);
    }
    public static readonly StyledProperty<string?> ArgumentProperty =
        AvaloniaProperty.Register<PhCommandPreview, string?>(nameof(Argument));


    public PhCommandPreview()
    {
        _previewText = new SelectableTextBlock
        {
            FontFamily = new FontFamily(Const.FONT_CODE),
            FontSize = Const.FONT_SIZE_SMALL,
            TextWrapping = TextWrapping.Wrap,
        };

        // a soft neutral fill (no border) so the preview reads as a distinct, code-styled block
        var box = new Border
        {
            Padding = new Thickness(10, 7),
            Child = _previewText,
        };
        box[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_BackgroundNeutralBrush);
        box[!Border.CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius);

        Content = new StackPanel
        {
            Spacing = 5,
            Children =
            {
                new PhTextBlock { LangKey = LangId._CommandPreview },
                box,
            },
        };

        UpdatePreview();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ExecutableProperty || e.Property == ArgumentProperty)
        {
            UpdatePreview();
        }
    }


    /// <summary>
    /// Re-renders the preview from the current executable + argument, expanding the file macro.
    /// </summary>
    private void UpdatePreview()
    {
        var (exe, args) = BHelper.BuildExeArgs(
            Executable ?? string.Empty, Argument ?? string.Empty, PreviewFakePath);

        // app-protocol executables (ending with ':') join without a space
        var join = exe.EndsWith(':') ? string.Empty : " ";
        _previewText.Text = string.Join(join, new[] { exe, args }.Where(s => !string.IsNullOrEmpty(s)));
    }
}
