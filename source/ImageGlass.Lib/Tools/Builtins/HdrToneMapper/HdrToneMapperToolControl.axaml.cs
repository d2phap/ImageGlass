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
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using System;
using System.Globalization;
using System.Text.Json;

namespace ImageGlass.Tools;

/// <summary>
/// Hosted tool to adjust the 5 <see cref="HdrToneMappingOptions"/> values live.
/// Edits <see cref="Core.HdrToneMappingConfig"/> (the source of truth the viewer reads),
/// persists via tool settings, and re-decodes the current HDR photo to show changes.
/// </summary>
public partial class HdrToneMapperToolControl : PhControl, IToolControl
{
    // prevents feedback loop when loading control values from config
    private bool _isUpdatingUI;

    // Mode ComboBox items follow this enum order (index maps to the value)
    private static readonly HdrToneMappingMode[] _modes = Enum.GetValues<HdrToneMappingMode>();

    // layout steps, widest first
    private static readonly (int Columns, Orientation RowOrientation)[] _layoutSteps =
    [
        (4, Orientation.Horizontal), // sliders 1x4 | buttons 1x2
        (2, Orientation.Horizontal), // sliders 2x2 | buttons 1x2
        (2, Orientation.Vertical),   // sliders 2x2 | buttons 2x1
        (1, Orientation.Vertical),   // sliders 4x1 | buttons 2x1
    ];


    public static string TOOL_ID => "Tool_HdrToneMapper";
    public string ToolId => TOOL_ID;
    public bool HasSettingsUI => false;
    public bool IsProPreview => !Core.IsProEnabled;
    public object? Settings => Core.HdrToneMappingConfig;
    public ViewerControl Viewer { get; set; } = null!;


    public HdrToneMapperToolControl()
    {
        InitializeComponent();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        PopulateModeItems();
        LoadConfigToUI();

        // retain the raw HDR frame so slider changes re-apply instantly (no disk re-decode)
        if (!IsProPreview) Viewer?.BeginLiveHdrToneMapping();

        PART_CmbMode.SelectionChanged += Mode_SelectionChanged;
        PART_SldExposure.ValueChanged += Slider_ValueChanged;
        PART_SldReferenceWhite.ValueChanged += Slider_ValueChanged;
        PART_SldHighlightCompression.ValueChanged += Slider_ValueChanged;
        PART_SldSaturation.ValueChanged += Slider_ValueChanged;
        PART_BtnReset.Click += PART_BtnReset_Click;

        if (Viewer is not null) Viewer.PhotoLoading += Viewer_PhotoLoading;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        PART_CmbMode.SelectionChanged -= Mode_SelectionChanged;
        PART_SldExposure.ValueChanged -= Slider_ValueChanged;
        PART_SldReferenceWhite.ValueChanged -= Slider_ValueChanged;
        PART_SldHighlightCompression.ValueChanged -= Slider_ValueChanged;
        PART_SldSaturation.ValueChanged -= Slider_ValueChanged;
        PART_BtnReset.Click -= PART_BtnReset_Click;

        if (Viewer is not null) Viewer.PhotoLoading -= Viewer_PhotoLoading;

        // stop retaining the raw HDR frame
        Viewer?.EndLiveHdrToneMapping();

        base.OnUnloaded(e);
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        // unconstrained first, so the steps key off natural content widths
        base.MeasureOverride(new Size(double.PositiveInfinity, availableSize.Height));
        ApplyResponsiveLayout(availableSize.Width);

        return base.MeasureOverride(availableSize);
    }


    private void Viewer_PhotoLoading(ViewerControl sender, PhotoLoadingEventArgs e)
    {
        UpdateControlsState();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        PART_BtnReset.Text = Core.Lang[LangId.Tool_Hdr_BtnReset];
        UpdateValueLabels();
    }


    private void Mode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingUI) return;

        SaveUIToConfig();
    }


    private void Slider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isUpdatingUI) return;

        SaveUIToConfig();
    }


    private void PART_BtnReset_Click(object? sender, RoutedEventArgs e)
    {
        // reset the sliders to defaults but keep the current Mode
        var mode = Core.HdrToneMappingConfig.Mode;
        Core.HdrToneMappingConfig = new HdrToneMappingOptions { Mode = mode };

        LoadConfigToUI();
        ToolRegistry.SaveToolSettings(this);
        Viewer?.ReapplyHdrToneMapping();
    }

    #endregion // Control Events



    #region Control Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void LoadSettings(JsonElement? jsonEl)
    {
        var opts = jsonEl?.Deserialize(HdrToneMappingOptionsJsonContext.Default.HdrToneMappingOptions);
        if (opts is not null) Core.HdrToneMappingConfig = opts;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public JsonElement? SaveSettings()
    {
        return JsonSerializer.SerializeToElement(Core.HdrToneMappingConfig,
            HdrToneMappingOptionsJsonContext.Default.HdrToneMappingOptions);
    }


    /// <summary>
    /// Populates the Mode ComboBox from the <see cref="HdrToneMappingMode"/> enum (in enum order).
    /// </summary>
    private void PopulateModeItems()
    {
        if (PART_CmbMode.ItemCount > 0) return;

        foreach (var mode in _modes)
        {
            PART_CmbMode.Items.Add(new ComboBoxItem { Content = Enum.GetName(mode) });
        }
    }


    /// <summary>
    /// Applies the widest <see cref="_layoutSteps"/> entry that fits <paramref name="availableWidth"/>.
    /// Only valid right after an unconstrained measure.
    /// </summary>
    private void ApplyResponsiveLayout(double availableWidth)
    {
        // UniformGrid sizes every cell to the largest one
        var columnWidth = 0d;
        foreach (var column in PART_SlidersGrid.Children)
        {
            columnWidth = Math.Max(columnWidth, column.DesiredSize.Width);
        }

        // not measured yet; keep the current step
        if (columnWidth <= 0) return;

        var comboWidth = PART_CmbMode.DesiredSize.Width;
        var resetWidth = PART_BtnReset.DesiredSize.Width;
        var modeLabelWidth = PART_LblMode.DesiredSize.Width;
        var groupRowWidth = comboWidth + PART_ModeResetRow.Spacing + resetWidth;

        // group width = its widest line, label included
        var groupHorizontalWidth = Math.Max(modeLabelWidth, groupRowWidth);
        var groupVerticalWidth = Math.Max(modeLabelWidth, Math.Max(comboWidth, resetWidth));

        var contentWidth = availableWidth - Padding.Left - Padding.Right;
        var columns = _layoutSteps[^1].Columns;
        var rowOrientation = _layoutSteps[^1].RowOrientation;

        foreach (var step in _layoutSteps)
        {
            var groupWidth = step.RowOrientation == Orientation.Horizontal
                ? groupHorizontalWidth
                : groupVerticalWidth;
            var stepWidth = (step.Columns * columnWidth) + PART_RootPanel.Spacing + groupWidth;
            if (stepWidth > contentWidth) continue;

            columns = step.Columns;
            rowOrientation = step.RowOrientation;
            break;
        }

        if (PART_SlidersGrid.Columns != columns) PART_SlidersGrid.Columns = columns;
        if (PART_ModeResetRow.Orientation != rowOrientation) PART_ModeResetRow.Orientation = rowOrientation;
    }


    /// <summary>
    /// Loads the current <see cref="Core.HdrToneMappingConfig"/> values into the controls.
    /// </summary>
    private void LoadConfigToUI()
    {
        _isUpdatingUI = true;
        try
        {
            var cfg = Core.HdrToneMappingConfig;

            PART_CmbMode.SelectedIndex = Array.IndexOf(_modes, cfg.Mode);
            PART_SldExposure.Value = cfg.Exposure;
            PART_SldReferenceWhite.Value = cfg.ReferenceWhiteNits;
            PART_SldHighlightCompression.Value = cfg.HighlightCompression;
            PART_SldSaturation.Value = cfg.Saturation;

            UpdateValueLabels();
            UpdateControlsState();
        }
        finally
        {
            _isUpdatingUI = false;
        }
    }


    /// <summary>
    /// Disables the whole tool when the current photo cannot be tone-mapped, and the value sliders
    /// when Mode is <see cref="HdrToneMappingMode.None"/> (pass-through ignores them).
    /// In Classic every input is dead but the labels stay readable.
    /// </summary>
    private void UpdateControlsState()
    {
        var isProPreview = IsProPreview;

        // disabling the root panel cascades to every child, labels included
        PART_RootPanel.IsEnabled = isProPreview || (Viewer?.CanReapplyHdrToneMapping() ?? false);

        var idx = PART_CmbMode.SelectedIndex;
        var mode = idx >= 0 ? _modes[idx] : HdrToneMappingMode.BT2408;
        var enabled = !isProPreview && mode != HdrToneMappingMode.None;

        PART_CmbMode.IsEnabled = !isProPreview;
        PART_BtnReset.IsEnabled = !isProPreview;
        PART_SldExposure.IsEnabled = enabled;
        PART_SldReferenceWhite.IsEnabled = enabled;
        PART_SldHighlightCompression.IsEnabled = enabled;
        PART_SldSaturation.IsEnabled = enabled;
    }


    /// <summary>
    /// Applies the control values to <see cref="Core.HdrToneMappingConfig"/>, persists them, and
    /// requests a live re-tone-map. The viewer coalesces requests and runs the pass off-thread.
    /// </summary>
    private void SaveUIToConfig()
    {
        var cfg = Core.HdrToneMappingConfig;
        cfg.Mode = _modes[Math.Max(0, PART_CmbMode.SelectedIndex)];
        cfg.Exposure = PART_SldExposure.Value;
        cfg.ReferenceWhiteNits = PART_SldReferenceWhite.Value;
        cfg.HighlightCompression = PART_SldHighlightCompression.Value;
        cfg.Saturation = PART_SldSaturation.Value;

        // Mode 'None' turns HDR tone mapping off; any real mode turns it on
        Core.Config.EnableHdrToneMapping = cfg.Mode != HdrToneMappingMode.None;

        UpdateValueLabels();
        UpdateControlsState();
        ToolRegistry.SaveToolSettings(this);

        Viewer?.ReapplyHdrToneMapping();
    }


    /// <summary>
    /// Shows the live slider values in their labels (the lang strings carry a <c>{0}</c> placeholder).
    /// </summary>
    private void UpdateValueLabels()
    {
        PART_LblExposure.LangParams = PART_SldExposure.Value.ToString("0.0#", CultureInfo.InvariantCulture);
        PART_LblReferenceWhite.LangParams = PART_SldReferenceWhite.Value.ToString("0", CultureInfo.InvariantCulture);
        PART_LblHighlightCompression.LangParams = PART_SldHighlightCompression.Value.ToString("0.00", CultureInfo.InvariantCulture);
        PART_LblSaturation.LangParams = PART_SldSaturation.Value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    #endregion // Control Methods

}
