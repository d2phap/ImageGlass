/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ImageGlass.Common.Actions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class MouseSettingsView : SettingsPageView
{
    // shared width for the wheel dropdowns and click buttons
    private const double ACTION_CONTROL_WIDTH = 250;

    private readonly Dictionary<MouseWheelEvent, MouseWheelAction> _wheelActions = [];
    private readonly Dictionary<MouseClickEvent, SingleAction> _clickActions = [];


    public MouseSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public MouseSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        BuildWheelActions();
        BuildClickActions();
    }


    #region Mouse wheel actions

    /// <summary>
    /// Loads the working copy of the wheel actions and wires the table + reset link.
    /// </summary>
    private void BuildWheelActions()
    {
        // seed from defaults (effective values), then overlay the stored config
        foreach (var (evt, action) in Config.DefaultMouseWheelActions) _wheelActions[evt] = action;
        foreach (var (evt, action) in VM.GetValue(ConfigId.MouseWheelActions,
            new Dictionary<MouseWheelEvent, MouseWheelAction>()))
        {
            _wheelActions[evt] = action;
        }

        SetLocalizedText(PART_ResetWheel, LangId._ResetToDefault);
        PART_ResetWheel.Click += (_, _) => ResetWheelActions();

        AddLangRefresher(RebuildWheelTable);

        RegisterSearchKey(PART_ResetWheel, LangId.Settings_MouseWheelAction,
            ConfigId.MouseWheelActions, LangId.Settings_MouseWheelAction);

        DisableIfLocked(ConfigId.MouseWheelActions, PART_WheelTable, PART_ResetWheel);
    }


    /// <summary>
    /// Stages the current working copy of wheel actions into the view model.
    /// </summary>
    private void StageWheelActions()
        => VM.SetValue(ConfigId.MouseWheelActions,
            new Dictionary<MouseWheelEvent, MouseWheelAction>(_wheelActions));


    /// <summary>
    /// Restores the default wheel actions and re-renders.
    /// </summary>
    private void ResetWheelActions()
    {
        _wheelActions.Clear();
        foreach (var (evt, action) in Config.DefaultMouseWheelActions) _wheelActions[evt] = action;

        StageWheelActions();
        RebuildWheelTable();
    }


    /// <summary>
    /// Rebuilds the wheel rows: each event shows its label above an action dropdown.
    /// </summary>
    private void RebuildWheelTable()
    {
        PART_WheelTable.Children.Clear();

        foreach (var evt in Enum.GetValues<MouseWheelEvent>())
        {
            var row = new StackPanel { Spacing = 5 };
            row.Children.Add(new PhTextBlock { Text = EnumLabel(nameof(MouseWheelEvent), evt) });
            row.Children.Add(BuildWheelCombo(evt));

            PART_WheelTable.Children.Add(row);
        }
    }


    /// <summary>
    /// Builds the action dropdown for a wheel event, bound to the working copy.
    /// </summary>
    private ComboBox BuildWheelCombo(MouseWheelEvent evt)
    {
        var combo = new ComboBox
        {
            Width = ACTION_CONTROL_WIDTH,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var current = _wheelActions.GetValueOrDefault(evt, MouseWheelAction.DoNothing);
        var selectedIndex = 0;

        var actions = Enum.GetValues<MouseWheelAction>();
        for (var i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            combo.Items.Add(new ComboBoxItem
            {
                Tag = action,
                Content = EnumLabel(nameof(MouseWheelAction), action),
            });
            if (action == current) selectedIndex = i;
        }
        combo.SelectedIndex = selectedIndex;

        // subscribe after the initial selection so loading doesn't stage
        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is ComboBoxItem { Tag: MouseWheelAction action })
            {
                _wheelActions[evt] = action;
                StageWheelActions();
            }
        };

        return combo;
    }

    #endregion // Mouse wheel actions



    #region Mouse click actions

    /// <summary>
    /// Loads the working copy of the click actions and wires the table + reset link.
    /// </summary>
    private void BuildClickActions()
    {
        // seed from defaults (effective values), then overlay the stored config
        foreach (var (evt, action) in Config.DefaultMouseClickActions) _clickActions[evt] = action;
        foreach (var (evt, action) in VM.GetValue(ConfigId.MouseClickActions,
            new Dictionary<MouseClickEvent, SingleAction>()))
        {
            _clickActions[evt] = action;
        }

        SetLocalizedText(PART_ResetClick, LangId._ResetToDefault);
        PART_ResetClick.Click += (_, _) => ResetClickActions();

        AddLangRefresher(RebuildClickTable);

        RegisterSearchKey(PART_ResetClick, LangId.Settings_MouseClickAction,
            ConfigId.MouseClickActions, LangId.Settings_MouseClickAction);

        DisableIfLocked(ConfigId.MouseClickActions, PART_ClickTable, PART_ResetClick);

        // custom mouse click actions are Pro; Classic can still open each one to view it
        ProGate(ConfigId.MouseClickActions, null, PART_ResetClick);
    }


    /// <summary>
    /// Stages the current working copy of click actions into the view model.
    /// </summary>
    private void StageClickActions()
        => VM.SetValue(ConfigId.MouseClickActions,
            new Dictionary<MouseClickEvent, SingleAction>(_clickActions));


    /// <summary>
    /// Restores the default click actions and re-renders.
    /// </summary>
    private void ResetClickActions()
    {
        _clickActions.Clear();
        foreach (var (evt, action) in Config.DefaultMouseClickActions) _clickActions[evt] = action;

        StageClickActions();
        RebuildClickTable();
    }


    /// <summary>
    /// Rebuilds the click rows: each event shows its label and Pro badge above its editor button.
    /// </summary>
    private void RebuildClickTable()
    {
        PART_ClickTable.Children.Clear();
        var isProGated = Config.IsConfigProGated(ConfigId.MouseClickActions);

        foreach (var evt in Enum.GetValues<MouseClickEvent>())
        {
            var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            header.Children.Add(new PhTextBlock
            {
                Text = EnumLabel(nameof(MouseClickEvent), evt),
                VerticalAlignment = VerticalAlignment.Center,
            });
            header.Children.Add(new PhProBadge { IsVisible = isProGated });

            var row = new StackPanel { Spacing = 5 };
            row.Children.Add(header);
            row.Children.Add(BuildClickButton(evt));

            PART_ClickTable.Children.Add(row);
        }
    }


    /// <summary>
    /// Builds the action button for a click event: shows the bound executable (or "Do nothing")
    /// and opens the editor when clicked.
    /// </summary>
    private PhButton BuildClickButton(MouseClickEvent evt)
    {
        var exe = _clickActions.GetValueOrDefault(evt)?.Executable?.Trim();
        var hasExe = !string.IsNullOrEmpty(exe);

        var btn = new PhButton
        {
            Width = ACTION_CONTROL_WIDTH,
            HorizontalAlignment = HorizontalAlignment.Left,
            // stretch the content so the label fills the width and can ellipsis-trim
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        // PhButton.Text centers and never trims, so use our own trimming label as the content
        var label = new TextBlock
        {
            Text = hasExe ? System.IO.Path.GetFileName(exe!) : Core.Lang[LangId.MouseWheelAction_DoNothing],
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        label[!TextBlock.ForegroundProperty] = btn[!Button.ForegroundProperty];
        btn.Content = label;

        if (hasExe) ToolTip.SetTip(btn, exe);

        btn.Click += async (_, _) => await EditClickActionAsync(evt);

        return btn;
    }


    /// <summary>
    /// Opens the editor for a click event (view-only for Classic) and stages the result; empty unbinds it.
    /// </summary>
    private async Task EditClickActionAsync(MouseClickEvent evt)
    {
        var existing = _clickActions.GetValueOrDefault(evt);
        var isReadOnly = Config.IsConfigProGated(ConfigId.MouseClickActions);
        var window = new MouseClickActionEditWindow(EnumLabel(nameof(MouseClickEvent), evt), existing, isReadOnly);

        if (await window.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) != DialogExitCode.OK) return;
        if (window.ResultAction is not { } result) return;

        if (string.IsNullOrEmpty(result.Executable)) _clickActions.Remove(evt);
        else _clickActions[evt] = result;

        StageClickActions();
        RebuildClickTable();
    }

    #endregion // Mouse click actions



    #region Helpers

    /// <summary>
    /// Gets the localized label of an enum value via the <c>{EnumType}_{Value}</c> key.
    /// </summary>
    private static string EnumLabel<TEnum>(string enumName, TEnum value) where TEnum : struct, Enum
        => Lang.GetKey($"{enumName}_{value}") is { } key ? Core.Lang[key] : value.ToString();

    #endregion // Helpers

}
