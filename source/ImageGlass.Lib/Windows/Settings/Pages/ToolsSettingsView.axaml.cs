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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.Tools;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class ToolsSettingsView : SettingsPageView
{
    private const double NAME_MAX_WIDTH = 220;
    private const double HOTKEY_MAX_WIDTH = 200;
    private const string TOOLS_URL = "https://imageglass.org/tools";

    // working copy of the registered tools; staged into the VM on change
    private readonly List<ExternalTool> _tools = [];


    public ToolsSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public ToolsSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // copy the staged/config value so edits don't mutate the live config before commit
        _tools.AddRange(VM.GetValue(ConfigId.Tools, new ObservableCollection<ExternalTool>()));

        SetLocalizedText(PART_AddTool, LangId._Add);
        PART_AddTool.Click += async (_, _) => await AddOrEditToolAsync(null);

        BindLink(PART_GetMoreTools, LangId._GetMoreTools, TOOLS_URL,
            () => _ = BHelper.OpenUrlAsync(App.SettingsWindow, TOOLS_URL, "from_get_more_tools"));

        // rebuild on language change (also performs the initial render)
        AddLangRefresher(RebuildTable);

        RegisterSearchKey(PART_AddTool, LangId.Settings_Nav_Tools, ConfigId.Tools, LangId.Settings_Nav_Tools);

        DisableIfLocked(ConfigId.Tools, PART_AddTool, PART_Table);
    }


    /// <summary>
    /// Stages the current working copy of tools into the view model.
    /// </summary>
    private void StageTools() => VM.SetValue(ConfigId.Tools, new ObservableCollection<ExternalTool>(_tools));


    /// <summary>
    /// Opens <see cref="ToolEditWindow"/> to add a new tool (when <paramref name="existing"/> is null)
    /// or edit an existing one, then updates the working copy and re-renders.
    /// </summary>
    private async Task AddOrEditToolAsync(ExternalTool? existing)
    {
        var isIdLocked = existing is not null && PredefinedExternalTools.IsPredefined(existing.ToolId);
        var win = new ToolEditWindow(existing, CollectTakenIds(except: existing), isIdLocked);
        if (await win.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) != DialogExitCode.OK) return;
        if (win.ResultTool is not { } tool) return;

        var index = existing is not null ? _tools.IndexOf(existing) : -1;
        if (index >= 0) _tools[index] = tool;
        else _tools.Add(tool);

        StageTools();
        RebuildTable();
    }


    /// <summary>
    /// Opens the edit dialog for the tool with the given id; when missing from the working copy,
    /// opens the add dialog seeded from the pre-configured definition, else from the id alone.
    /// </summary>
    public Task EditToolAsync(string toolId)
    {
        if (string.IsNullOrEmpty(toolId)) return Task.CompletedTask;

        var existing = _tools.FirstOrDefault(t => string.Equals(t.ToolId, toolId, StringComparison.OrdinalIgnoreCase));
        var seed = existing ?? PredefinedExternalTools.Find(toolId) ?? new ExternalTool { ToolId = toolId };

        return AddOrEditToolAsync(seed);
    }


    /// <summary>
    /// Removes a tool from the working copy and re-renders. Pre-configured tools are not removable.
    /// </summary>
    private void DeleteTool(ExternalTool tool)
    {
        if (PredefinedExternalTools.IsPredefined(tool.ToolId)) return;
        if (!_tools.Remove(tool)) return;

        StageTools();
        RebuildTable();
    }


    /// <summary>
    /// Gets the tool ids already in use, excluding the given tool's own id (so editing doesn't clash with itself).
    /// </summary>
    private HashSet<string> CollectTakenIds(ExternalTool? except)
    {
        var set = new HashSet<string>(_tools.Select(t => t.ToolId), StringComparer.OrdinalIgnoreCase);
        if (except is not null) set.Remove(except.ToolId);
        return set;
    }


    /// <summary>
    /// Rebuilds the tools table from the working copy (header + one row per tool), toggling the empty state.
    /// </summary>
    private void RebuildTable()
    {
        PhTableColumn[] columns =
        [
            new() { Header = Core.Lang[LangId._Name] },
            new() { Header = Core.Lang[LangId._Executable], Star = true },
            new() { Header = string.Empty }, // integrated indicator icon
            new() { Header = Core.Lang[LangId._Hotkeys] },
        ];

        var rows = _tools.Select(tool =>
        {
            var actions = new List<PhTableAction>
            {
                new() { Icon = ResxIconId.IconEdit, Tooltip = Core.Lang[LangId._Edit], Click = () => _ = AddOrEditToolAsync(tool) },
            };

            // a pre-configured tool is editable but not removable
            if (!PredefinedExternalTools.IsPredefined(tool.ToolId))
            {
                actions.Add(new() { Icon = ResxIconId.IconClose, Tooltip = Core.Lang[LangId._Delete], Click = () => DeleteTool(tool) });
            }

            return new PhTableRow
            {
                Cells =
                [
                    NameCell(tool),
                    ExecutableCell(tool),
                    IntegratedCell(tool),
                    HotkeysCell(tool),
                ],
                Actions = actions,
            };
        }).ToList();

        PART_Table.EmptyText = Core.Lang[LangId._Empty];
        PART_Table.Build(columns, rows);
    }


    /// <summary>
    /// The hotkeys cell: one keycap chip per hotkey (capped width so multiple chips wrap),
    /// or an empty cell when the tool has none.
    /// </summary>
    private static Control HotkeysCell(ExternalTool tool)
    {
        if (tool.Hotkeys.Length == 0) return new Panel();

        var panel = new WrapPanel { MaxWidth = HOTKEY_MAX_WIDTH };
        foreach (var hk in tool.Hotkeys)
        {
            panel.Children.Add(new PhHotkeyChip(hk.KeyString)
            {
                Margin = new Thickness(0, 0, 6, 0),
            });
        }

        return PhTableControl.WrapCell(panel);
    }


    #region Table cell builders

    /// <summary>
    /// The name cell: a link button showing the tool name (capped); clicking it opens the edit dialog.
    /// </summary>
    private Control NameCell(ExternalTool tool)
    {
        var displayName = string.IsNullOrWhiteSpace(tool.ToolName) ? tool.ToolId : tool.ToolName;

        var btn = new PhButton
        {
            Variant = PhButtonVariant.Link,
            Text = displayName,
            MaxWidth = NAME_MAX_WIDTH,
            HorizontalAlignment = HorizontalAlignment.Left,
            ClipToBounds = true,
        };
        ToolTip.SetTip(btn, displayName);
        btn.Click += (_, _) => _ = AddOrEditToolAsync(tool);

        return PhTableControl.WrapCell(btn);
    }


    /// <summary>
    /// The executable cell: the executable path, with the arguments on a dimmed second line below it
    /// (both selectable and truncated); the arguments line is omitted when empty.
    /// </summary>
    private static Control ExecutableCell(ExternalTool tool)
    {
        var stack = new StackPanel { Spacing = 2 };
        stack.Children.Add(ExecutableLine(tool.Executable, secondary: false));

        if (!string.IsNullOrWhiteSpace(tool.Arguments))
        {
            stack.Children.Add(ExecutableLine(tool.Arguments, secondary: true));
        }

        return PhTableControl.WrapCell(stack);
    }


    /// <summary>
    /// One selectable, ellipsis-truncated line of the executable cell; <paramref name="secondary"/>
    /// shrinks and dims it (used for the arguments line).
    /// </summary>
    private static SelectableTextBlock ExecutableLine(string text, bool secondary)
    {
        var tb = new SelectableTextBlock
        {
            Text = text,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Top,
            IsTabStop = false,
        };
        if (!string.IsNullOrEmpty(text)) ToolTip.SetTip(tb, text);

        if (secondary)
        {
            tb.FontSize = Const.FONT_SIZE_SMALL;
            tb.Opacity = 0.7;
        }

        return tb;
    }


    /// <summary>
    /// The integrated-indicator cell: for integrated tools, the "integrated" glyph with an
    /// "Integrated" tooltip; otherwise an empty cell.
    /// </summary>
    private static Control IntegratedCell(ExternalTool tool)
    {
        if (!tool.IsIntegrated) return new Panel();

        var glyph = new Path
        {
            Data = Resx.GetIcon(ResxIconId.IconIntegrated),
            Width = Const.FONT_SIZE_BODY,
            Height = Const.FONT_SIZE_BODY,
            StrokeThickness = 1.5,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Stretch = Stretch.Uniform,
        };
        glyph[!Shape.StrokeProperty] = new DynamicResourceExtension("PhAccentFill");

        // transparent fill so the whole icon box reports hover for the tooltip
        var hit = new Border { Background = Brushes.Transparent, Child = glyph };
        ToolTip.SetTip(hit, Core.Lang[LangId.Settings_Tools_Integrated]);

        return PhTableControl.WrapCell(hit);
    }

    #endregion // Table cell builders

}
