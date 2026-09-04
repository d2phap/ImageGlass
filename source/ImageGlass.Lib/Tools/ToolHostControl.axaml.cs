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
using Avalonia.Interactivity;
using Avalonia.Metadata;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.UI;
using System;

namespace ImageGlass.Tools;

public partial class ToolHostControl : PhControl
{

    #region Public Properties

    /// <summary>
    /// Gets, sets the current tool content hosted by this control.
    /// </summary>
    [Content]
    public IToolControl? Tool
    {
        get => GetValue(PluginContentProperty);
        set => SetValue(PluginContentProperty, value);
    }
    public static readonly StyledProperty<IToolControl?> PluginContentProperty =
        AvaloniaProperty.Register<ToolHostControl, IToolControl?>(nameof(Tool));



    /// <summary>
    /// Gets the tooltip text displayed for the close button.
    /// </summary>
    public string CloseButtonTooltipText
    {
        get => GetValue(CloseButtonTooltipTextProperty);
        private set => SetValue(CloseButtonTooltipTextProperty, value);
    }
    public static readonly StyledProperty<string> CloseButtonTooltipTextProperty =
        AvaloniaProperty.Register<ToolHostControl, string>(nameof(CloseButtonTooltipText));


    /// <summary>
    /// Gets the tooltip text displayed for the settings button.
    /// </summary>
    public string SettingsButtonTooltipText
    {
        get => GetValue(SettingsButtonTooltipTextProperty);
        private set => SetValue(SettingsButtonTooltipTextProperty, value);
    }
    public static readonly StyledProperty<string> SettingsButtonTooltipTextProperty =
        AvaloniaProperty.Register<ToolHostControl, string>(nameof(SettingsButtonTooltipText));


    /// <summary>
    /// Gets the value indicates if the tool contains settings.
    /// </summary>
    public bool HasSettings
    {
        get => GetValue(HasSettingsProperty);
        private set => SetValue(HasSettingsProperty, value);
    }
    public static readonly StyledProperty<bool> HasSettingsProperty =
        AvaloniaProperty.Register<ToolHostControl, bool>(nameof(HasSettings));


    /// <summary>
    /// Gets the value indicates if the hosted tool is a Pro feature shown as a read-only preview.
    /// </summary>
    public bool IsProPreview
    {
        get => GetValue(IsProPreviewProperty);
        private set => SetValue(IsProPreviewProperty, value);
    }
    public static readonly StyledProperty<bool> IsProPreviewProperty =
        AvaloniaProperty.Register<ToolHostControl, bool>(nameof(IsProPreview));


    #endregion // Public Properties



    public ToolHostControl()
    {
        InitializeComponent();
        IsContentVisible = false;
    }



    #region Control Events


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        CloseButtonTooltipText = Core.Lang[LangId._Close];
        SettingsButtonTooltipText = Core.Lang[LangId.Menu_MnuSettings];
    }


    private async void PART_BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        // Route through API so settings are saved before closing
        if (Tool is IToolControl tool)
        {
            _ = await Core.API.RunApiAsync(API.IG_CloseTool, tool.ToolId);
        }
    }


    private async void PART_BtnSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (Tool is not IToolControl tool) return;
        if (!tool.HasSettingsUI) return;

        await tool.ShowSettingsWindowAsync();
    }


    #endregion // Control Events



    #region Public Methods

    /// <summary>
    /// Opens the specified tool as the current hosted content.
    /// Settings are NOT loaded here — the caller (API layer) loads them before calling this.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public bool OpenTool(IToolControl? newTool)
    {
        if (newTool is null) return false;

        if (Tool is IToolControl tool)
        {
            throw new InvalidOperationException($"IGE: The current tool (ID = {tool.ToolId}) must be closed before opening another tool");
        }

        HasSettings = newTool.HasSettingsUI;
        IsProPreview = newTool.IsProPreview;

        // open the tool
        Tool = newTool;
        IsContentVisible = true;

        return true;
    }


    /// <summary>
    /// Closes the tool with the specified identifier if it is currently active.
    /// Settings are NOT saved here — the caller (API layer) saves them before calling this.
    /// </summary>
    public void CloseTool(string toolId)
    {
        if (Tool is not IToolControl tool) return;
        if (tool.ToolId != toolId) return;

        CloseCurrentTool();
    }


    /// <summary>
    /// Closes the currently active tool.
    /// Settings are NOT saved here — the caller (API layer) saves them before calling this.
    /// </summary>
    public void CloseCurrentTool()
    {
        if (Tool is not IToolControl) return;

        try
        {
            Tool = null;
            IsContentVisible = false;
        }
        catch { }
    }


    #endregion // Public Methods

}
