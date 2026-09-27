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
using ImageGlass.Common.ServiceProviders;
using System;
using System.Globalization;

namespace ImageGlass.Common.Windows;

public partial class SlideshowSettingsView : SettingsPageView
{
    /// <summary>
    /// Smallest interval the slideshow timer honors, in seconds.
    /// </summary>
    private const double MIN_INTERVAL = SlideshowProvider.MIN_INTERVAL_MS / 1000.0;


    public SlideshowSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public SlideshowSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Appearance
        BindToggle(PART_FullscreenSlideshow, ConfigId.EnableFullscreenSlideshow,
            LangId.Settings_EnableFullscreenSlideshow, LangId.Settings_Slideshow_Appearance, true);
        BindToggle(PART_SlideshowCountdown, ConfigId.EnableSlideshowCountdown,
            LangId.Settings_EnableSlideshowCountdown, LangId.Settings_Slideshow_Appearance, true);
        BindColorPicker(PART_BgColor, ConfigId.SlideshowBackgroundColor, "#000000",
            LangId.Settings_SlideshowBackgroundColor, LangId.Settings_Slideshow_Appearance);

        // Playback
        BindToggle(PART_LoopSlideshow, ConfigId.EnableLoopSlideshow,
            LangId.Settings_EnableLoopSlideshow, LangId.Settings_Slideshow_Playback, true);
        BindToggle(PART_RandomInterval, ConfigId.EnableSlideshowRandomInterval,
            LangId.Settings_EnableSlideshowRandomInterval, LangId.Settings_Slideshow_Playback);

        BindDoubleInput(PART_SlideshowInterval, ConfigId.SlideshowInterval,
            LangId.Settings_SlideshowInterval, LangId.Settings_Slideshow_Playback, 5d, MIN_INTERVAL);
        BindDoubleInput(PART_SlideshowIntervalTo, ConfigId.SlideshowIntervalTo,
            LangId.Settings_SlideshowInterval, LangId.Settings_Slideshow_Playback, 5d, MIN_INTERVAL);

        // the "to" interval + the heading range only apply when random interval is on
        PART_RandomInterval.IsCheckedChanged += (_, _) => UpdateIntervalUI();
        PART_SlideshowInterval.TextChanged += (_, _) => UpdateIntervalHeading();
        PART_SlideshowIntervalTo.TextChanged += (_, _) => UpdateIntervalHeading();
        AddLangRefresher(UpdateIntervalHeading); // refresh the localized prefix on language change
        UpdateIntervalUI();

        // Notification
        BindUIntInput(PART_NotifySound, ConfigId.SlideshowImagesToNotifySound,
            LangId.Settings_SlideshowImagesToNotifySound, LangId.Settings_Slideshow_Playback);

        BindTransitionEditor(PART_Transition, PART_TransitionDuration, PART_TransitionError, PART_OpenTransitionsFolder,
            ConfigId.SlideshowTransition, ConfigId.SlideshowTransitionDuration, 1000u, LangId.Settings_Slideshow_Playback);
    }


    /// <summary>
    /// Shows the "to" interval input (and the "from" label) only when random interval is enabled,
    /// then refreshes the heading. With random off there's a single value, so the "from" label is redundant.
    /// </summary>
    private void UpdateIntervalUI()
    {
        var isRandom = PART_RandomInterval.IsChecked ?? false;
        PART_IntervalToSection.IsVisible = isRandom;
        PART_IntervalFromLabel.IsVisible = isRandom;

        // with random off there's a single value, so let the "from" box fill the wider space
        PART_SlideshowInterval.Width = isRandom ? 150 : 200;
        UpdateIntervalHeading();
    }


    /// <summary>
    /// Updates the interval sub-group heading to show the live range, e.g.
    /// "Slideshow interval: 00:05.000 - 00:10.000" (the "to" part only when random is on).
    /// </summary>
    private void UpdateIntervalHeading()
    {
        var heading = $"{Core.Lang[LangId.Settings_SlideshowInterval]} {FormatInterval(PART_SlideshowInterval.Text)}";
        if (PART_RandomInterval.IsChecked ?? false)
        {
            heading += $" - {FormatInterval(PART_SlideshowIntervalTo.Text)}";
        }

        PART_IntervalHeading.Text = heading;
    }


    /// <summary>
    /// Formats a seconds value (as typed in the box) as <c>mm:ss.fff</c>.
    /// </summary>
    private static string FormatInterval(string? secondsText)
    {
        _ = double.TryParse(secondsText, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds);
        var clamped = Math.Max(MIN_INTERVAL, seconds);

        return TimeSpan.FromSeconds(clamped).ToString(@"mm\:ss\.fff", CultureInfo.InvariantCulture);
    }

}
