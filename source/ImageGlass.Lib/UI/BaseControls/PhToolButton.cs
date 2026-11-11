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
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.ComponentModel;

namespace ImageGlass.UI;

public partial class PhToolButton : ToggleButton
{
    protected override Type StyleKeyOverride => typeof(ToggleButton);


    // events
    public event TEventHandler<ContextMenu, CancelEventArgs>? DropdownOpening;
    public event TEventHandler<ContextMenu, RoutedEventArgs>? DropdownOpened;
    public event TEventHandler<ContextMenu, CancelEventArgs>? DropdownClosing;
    public event TEventHandler<ContextMenu, RoutedEventArgs>? DropdownClosed;


    #region Public Properties

    /// <summary>
    /// Gets the DPI scale value.
    /// </summary>
    public double Dpi => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;


    /// <summary>
    /// Gets, sets the value indicates that the button is checked on click.
    /// </summary>
    public bool IsCheckOnClick
    {
        get => GetValue(IsCheckOnClickProperty);
        set => SetValue(IsCheckOnClickProperty, value);
    }
    public static readonly StyledProperty<bool> IsCheckOnClickProperty =
        AvaloniaProperty.Register<PhToolButton, bool>(nameof(IsCheckOnClick));


    /// <summary>
    /// Gets, sets the value indicates that the dropdown menu is open on click.
    /// </summary>
    public bool IsOpenDropdownMenuOnClick
    {
        get => GetValue(IsOpenDropdownMenuOnClickProperty);
        set => SetValue(IsOpenDropdownMenuOnClickProperty, value);
    }
    public static readonly StyledProperty<bool> IsOpenDropdownMenuOnClickProperty =
        AvaloniaProperty.Register<PhToolButton, bool>(nameof(IsOpenDropdownMenuOnClick));


    /// <summary>
    /// Gets, sets the dropdown menu of this button.
    /// </summary>
    public ContextMenu? DropdownMenu
    {
        get => GetValue(DropdownMenuProperty);
        set
        {
            DropdownMenu?.Opening -= DropdownMenu_Opening;
            DropdownMenu?.Opened -= DropdownMenu_Opened;
            DropdownMenu?.Closing -= DropdownMenu_Closing;
            DropdownMenu?.Closed -= DropdownMenu_Closed;

            SetValue(DropdownMenuProperty, value);
        }
    }
    public static readonly StyledProperty<ContextMenu?> DropdownMenuProperty =
        AvaloniaProperty.Register<PhToolButton, ContextMenu?>(nameof(DropdownMenu));


    #endregion // Public Properties



    static PhToolButton()
    {
        // create theme for tool button
        ThemeProperty.OverrideDefaultValue<PhToolButton>(new ControlTheme(typeof(ToggleButton))
        {
            Setters =
            {
                new Setter(BackgroundProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackground)),
                new Setter(BorderBrushProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackground)),
                new Setter(CornerRadiusProperty, Resx.CreateBinding(ResxId.ControlCornerRadius)),
                new Setter(TransitionsProperty, new Transitions
                {
                    new BrushTransition {
                        Property = BackgroundProperty,
                        Duration = TimeSpan.FromMilliseconds(50),
                    },
                    new BrushTransition {
                        Property = BorderBrushProperty,
                        Duration = TimeSpan.FromMilliseconds(50),
                    },
                    new TransformOperationsTransition {
                        Property = RenderTransformProperty,
                        Duration = TimeSpan.FromMilliseconds(50),
                    },
                }),
            },
            Children =
            {
                new Style(x => x.Nesting().Class(":checked")
                    .Not(y => y.Class(":disabled")))
                {
                    Setters = {
                        new Setter(BackgroundProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundChecked)),
                        new Setter(BorderBrushProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundChecked)),
                    },
                },
                new Style(x => x.Nesting().Class(":pressed")
                    .Not(y => y.Class(":disabled")))
                {
                    Setters = {
                        new Setter(BackgroundProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundPressed)),
                        new Setter(BorderBrushProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundPressed)),
                        new Setter(RenderTransformProperty, new ScaleTransform(0.95, 0.95)),
                        new Setter(TransitionsProperty, new Transitions
                        {
                            new BrushTransition {
                                Property = BackgroundProperty,
                                Duration = TimeSpan.FromMilliseconds(30),
                            },
                            new BrushTransition {
                                Property = BorderBrushProperty,
                                Duration = TimeSpan.FromMilliseconds(30),
                            },
                        }),
                    }
                },
                new Style(x => x.Nesting().Class(":pointerover")
                    .Not(y => y.Class(":pressed"))
                    .Not(y => y.Class(":disabled")))
                {
                    Setters = {
                        new Setter(BackgroundProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundHover)),
                        new Setter(BorderBrushProperty, Resx.CreateBinding(ResxId.IG_ToolButtonBackgroundHover)),
                    },
                },
            },
        });
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        OnIgLanguageChanged();
        Core.ThemeChanged += Core_ThemeChanged;
        Core.LanguageChanged += Core_LanguageChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        Core.ThemeChanged -= Core_ThemeChanged;
        Core.LanguageChanged -= Core_LanguageChanged;
    }


    protected override void OnClick()
    {
        if (IsOpenDropdownMenuOnClick)
        {
            OpenDropdownMenu();
        }

        base.OnClick();
    }


    protected override void Toggle()
    {
        // don't call Toggle() method if not allowed.
        if (!IsCheckOnClick) return;

        base.Toggle();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == DropdownMenuProperty)
        {
            DropdownMenu?.Opening -= DropdownMenu_Opening;
            DropdownMenu?.Opened -= DropdownMenu_Opened;
            DropdownMenu?.Closing -= DropdownMenu_Closing;
            DropdownMenu?.Closed -= DropdownMenu_Closed;

            DropdownMenu?.Opening += DropdownMenu_Opening;
            DropdownMenu?.Opened += DropdownMenu_Opened;
            DropdownMenu?.Closing += DropdownMenu_Closing;
            DropdownMenu?.Closed += DropdownMenu_Closed;
        }
    }


    private void DropdownMenu_Opening(object? sender, CancelEventArgs e)
    {
        OnIgDropdownMenuOpening(e);
    }


    private void DropdownMenu_Opened(object? sender, RoutedEventArgs e)
    {
        OnIgDropdownMenuOpened(e);
    }


    private void DropdownMenu_Closing(object? sender, CancelEventArgs e)
    {
        OnIgDropdownMenuClosing(e);
    }


    private void DropdownMenu_Closed(object? sender, RoutedEventArgs e)
    {
        OnIgDropdownMenuClosed(e);
    }


    private void Core_ThemeChanged(object? sender, ThemePackChangedEventArgs e)
    {
        OnIgThemeChanged(e);
    }


    private void Core_LanguageChanged(object? sender, EventArgs e)
    {
        OnIgLanguageChanged();
    }

    #endregion // Control Events



    #region Virtual Methods

    /// <summary>
    /// Occurs when the app theme is changed.
    /// </summary>
    protected virtual void OnIgThemeChanged(ThemePackChangedEventArgs e) { }


    /// <summary>
    /// Occurs when the app language is changed.
    /// </summary>
    protected virtual void OnIgLanguageChanged() { }


    /// <summary>
    /// Occurs when the value of the IsOpen property is changing from false to true.
    /// </summary>
    protected virtual void OnIgDropdownMenuOpening(CancelEventArgs e)
    {
        DropdownOpening?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the dropdown menu is opened.
    /// </summary>
    protected virtual void OnIgDropdownMenuOpened(RoutedEventArgs e)
    {
        DropdownOpened?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the value of the IsOpen property is changing from true to false.
    /// </summary>
    protected virtual void OnIgDropdownMenuClosing(CancelEventArgs e)
    {
        DropdownClosing?.Invoke(DropdownMenu!, e);
    }


    /// <summary>
    /// Occurs when the dropdown menu is closed.
    /// </summary>
    protected virtual void OnIgDropdownMenuClosed(RoutedEventArgs e)
    {
        DropdownClosed?.Invoke(DropdownMenu!, e);
    }

    #endregion // Virtual Methods



    #region Public Methods

    /// <summary>
    /// Opens dropdown menu of this button if any.
    /// </summary>
    public void OpenDropdownMenu(PlacementMode? placement = null)
    {
        if (DropdownMenu is null) return;

        DropdownMenu.Placement = placement ?? PlacementMode.BottomEdgeAlignedRight;
        DropdownMenu.Open(this);
    }

    #endregion // Public Methods


}
