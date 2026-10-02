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
using ImageGlass.Common.Localization;
using ImageGlass.UI.Viewer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ImageGlass.Common.Windows;

public partial class ViewerSettingsView : SettingsPageView
{
    // canonical in-text separator for the zoom-levels list
    private const string ZOOM_SEPARATOR = "; ";

    // default zoom levels as a percentage list (1.0 factor = 100%)
    private const string DEFAULT_ZOOM_LEVELS = "5; 10; 15; 20; 30; 40; 50; 60; 70; 80; 90; 100; "
        + "125; 150; 175; 200; 250; 300; 350; 400; 500; 600; 700; 800; 1000; 1200; 1500; 1800; "
        + "2100; 2500; 3000; 3500; 4500; 6000; 8000; 10000";


    public ViewerSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public ViewerSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Appearance
        BindToggle(PART_NavButtons, ConfigId.EnableNavigationButtons,
            LangId.Settings_EnableNavigationButtons, LangId.Settings_Appearance, true);
        BindToggle(PART_VectorRenderer, ConfigId.EnableVectorRenderer,
            LangId.Settings_EnableVectorRenderer, LangId.Settings_Appearance, true);
        BindEnumDropdown(PART_ScrollbarMode, ConfigId.ViewerScrollbarMode, ScrollbarMode.Never,
            LangId.Settings_ViewerScrollbarMode, LangId.Settings_Appearance);
        BindEnumDropdown(PART_CheckerboardMode, ConfigId.CheckerboardMode, CheckerboardType.None,
            LangId.Settings_CheckerboardMode, LangId.Settings_Appearance);

        // Panning (free & margin panning are Pro)
        BindToggle(PART_FreePan, ConfigId.EnableFreePan,
            LangId.Settings_EnableFreePan, LangId.Settings_Panning);
        ProGate(ConfigId.EnableFreePan, PART_FreePanProBadge, PART_FreePan);
        BindDoubleInput(PART_PanMargin, ConfigId.PanMargin,
            LangId.Settings_PanMargin, LangId.Settings_Panning);
        ProGate(ConfigId.PanMargin, PART_PanMarginProBadge, PART_PanMargin);
        BindSlider(PART_PanSpeed, ConfigId.PanSpeed,
            LangId.Settings_PanSpeed, LangId.Settings_Panning, 20d, PART_PanSpeedLabel);

        // Zooming
        BindEnumDropdown(PART_InterpolationScaleDown, ConfigId.ImageInterpolationScaleDown,
            ImageInterpolation.LinearMipmapNearest,
            LangId.Settings_ImageInterpolation_ScaleDown, LangId.Settings_Zooming);
        BindEnumDropdown(PART_InterpolationScaleUp, ConfigId.ImageInterpolationScaleUp,
            ImageInterpolation.Nearest,
            LangId.Settings_ImageInterpolation_ScaleUp, LangId.Settings_Zooming);
        BindSlider(PART_ZoomSpeed, ConfigId.ZoomSpeed,
            LangId.Settings_ZoomSpeed, LangId.Settings_Zooming, 0d, PART_ZoomSpeedLabel);

        BuildZoomLevels();
    }


    #region Zoom levels

    /// <summary>
    /// Binds the zoom-levels box (a semicolon-separated percentage list) plus the "use smooth zooming"
    /// toggle and the load-defaults link. Smooth zooming means no discrete levels: when it is on the
    /// stored list is empty and the box/link are disabled (matching the viewer's snap-vs-continuous rule).
    /// </summary>
    private void BuildZoomLevels()
    {
        // stored as factors (1.0 = 100%); empty list => smooth/continuous zoom
        var levels = VM.GetValue(ConfigId.ZoomLevels, Array.Empty<double>());

        PART_UseSmoothZooming.IsChecked = levels.Length == 0;
        PART_ZoomLevels.Text = FormatZoomLevels(levels);

        PART_UseSmoothZooming.IsCheckedChanged += (_, _) =>
        {
            UpdateZoomLevelsEnabled();
            StageZoomLevels(reformat: false);
        };

        // stage while typing; normalize the displayed text once editing ends (same as image-info tags)
        PART_ZoomLevels.TextChanged += (_, _) => StageZoomLevels(reformat: false);
        PART_ZoomLevels.LostFocus += (_, _) => StageZoomLevels(reformat: true);

        SetLocalizedText(PART_LoadDefaultZoomLevels, LangId.Settings_LoadDefaultZoomLevels);
        PART_LoadDefaultZoomLevels.Click += (_, _) => PART_ZoomLevels.Text = DEFAULT_ZOOM_LEVELS;

        UpdateZoomLevelsEnabled();

        RegisterSearchKey(PART_ZoomLevels, LangId.Settings_ZoomLevels,
            ConfigId.ZoomLevels, LangId.Settings_Zooming);
    }


    /// <summary>
    /// Enables the zoom-levels box and load-defaults link only when smooth zooming is off.
    /// </summary>
    private void UpdateZoomLevelsEnabled()
    {
        // an admin-locked zoom-levels setting disables all three controls (incl. the smooth-zoom
        // toggle) so the value cannot be changed through any of them
        if (Config.IsConfigLocked(ConfigId.ZoomLevels))
        {
            PART_UseSmoothZooming.IsEnabled = false;
            PART_ZoomLevels.IsEnabled = false;
            PART_LoadDefaultZoomLevels.IsEnabled = false;
            return;
        }

        var smooth = PART_UseSmoothZooming.IsChecked ?? false;
        PART_ZoomLevels.IsEnabled = !smooth;
        PART_LoadDefaultZoomLevels.IsEnabled = !smooth;
    }


    /// <summary>
    /// Stages the zoom levels. Smooth zooming stages an empty list; otherwise the box is parsed
    /// (tokens split on semicolons, commas, spaces or line breaks), keeping finite positive
    /// percentages as ascending, de-duplicated factors. When <paramref name="reformat"/> is set, the
    /// box is rewritten as a clean, semicolon-separated percentage list.
    /// </summary>
    private void StageZoomLevels(bool reformat)
    {
        if (PART_UseSmoothZooming.IsChecked ?? false)
        {
            VM.SetValue(ConfigId.ZoomLevels, Array.Empty<double>());
            return;
        }

        var tokens = (PART_ZoomLevels.Text ?? string.Empty)
            .Split([';', ',', ' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var seen = new HashSet<double>();
        var factors = new List<double>(tokens.Length);
        foreach (var token in tokens)
        {
            // values are percentages in the box; store them as zoom factors
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
                && double.IsFinite(percent) && percent > 0)
            {
                var factor = percent / 100d;
                if (seen.Add(factor)) factors.Add(factor);
            }
        }
        factors.Sort();

        VM.SetValue(ConfigId.ZoomLevels, factors.ToArray());

        if (reformat)
        {
            var normalized = FormatZoomLevels(factors);
            if (!string.Equals(PART_ZoomLevels.Text, normalized, StringComparison.Ordinal))
                PART_ZoomLevels.Text = normalized;
        }
    }


    /// <summary>
    /// Renders the stored factors as a semicolon-separated percentage list (e.g. <c>"50; 100; 200"</c>).
    /// </summary>
    private static string FormatZoomLevels(IEnumerable<double> factors)
        => string.Join(ZOOM_SEPARATOR,
            factors.Select(f => (f * 100d).ToString("0.##", CultureInfo.InvariantCulture)));

    #endregion // Zoom levels

}
