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
using Avalonia.Media;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer.Transitions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Base class for every settings page view.
/// </summary>
public abstract class SettingsPageView : PhControl
{
    private const string TRANSITIONS_URL = "https://imageglass.org/effects";


    /// <summary>
    /// Gets the staging working-copy view model the page binds to.
    /// </summary>
    protected SettingsViewModel VM { get; private set; } = null!;

    /// <summary>
    /// Gets the nav id of the hosting page.
    /// </summary>
    protected SettingsNavId NavId { get; private set; }

    /// <summary>
    /// Gets the localized label key of the hosting page (used for search breadcrumbs).
    /// </summary>
    protected LangId? PageLabel { get; private set; }

    /// <summary>
    /// Re-applies localized text to controls that don't self-refresh (buttons, combo items)
    /// </summary>
    private readonly List<Action> _langRefreshers = [];

    /// <summary>
    /// Guards a programmatic color-picker resync from staging a phantom edit.
    /// </summary>
    private bool _suppressColorStaging;


    /// <summary>
    /// Wires the page to its working copy and builds the rows. Call from the derived
    /// <c>(vm, navId, pageLabel)</c> constructor right after <c>InitializeComponent()</c>.
    /// </summary>
    protected void Initialize(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel)
    {
        VM = vm;
        NavId = navId;
        PageLabel = pageLabel;
        Build();
    }


    /// <summary>
    /// Creates and binds the page's setting rows, using the <c>Bind*</c> helpers below.
    /// </summary>
    protected abstract void Build();


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // PhTextBlock labels refresh themselves; controls registered below need a nudge
        foreach (var refresh in _langRefreshers) refresh();
    }



    #region Binding helpers

    /// <summary>
    /// Registers a callback that re-applies localized text on language change, and runs it once now.
    /// </summary>
    protected void AddLangRefresher(Action refresh)
    {
        _langRefreshers.Add(refresh);
        refresh();
    }


    /// <summary>
    /// Sets a button's text to a localized string and keeps it refreshed on language change.
    /// </summary>
    protected void SetLocalizedText(PhButton btn, LangId key)
        => AddLangRefresher(() => btn.Text = Core.Lang[key]);


    /// <summary>
    /// Disables the given composite/custom editor controls when <paramref name="id"/> is
    /// admin-locked. Use for editors that don't bind through a single <c>Bind*</c> helper
    /// (tables, pickers, list editors), which the registry's
    /// <see cref="SettingsRegistry.DisableLockedControls"/> pass cannot reach.
    /// </summary>
    protected static void DisableIfLocked(ConfigId id, params Control?[] targets)
    {
        if (!Config.IsConfigLocked(id)) return;

        foreach (var target in targets)
        {
            if (target is not null) target.IsEnabled = false;
        }
    }


    /// <summary>
    /// Pro-gates a setting: when Pro is not active, reveals the ✦ Pro badge (a <see cref="PhProBadge"/>
    /// sibling, which carries its own hover + tooltip) and disables the given input controls.
    /// </summary>
    protected void ProGate(ConfigId id, Control? badge, params Control?[] inputs)
    {
        if (!Config.IsConfigProGated(id)) return;

        if (badge is not null) badge.IsVisible = true;

        foreach (var input in inputs)
        {
            if (input is not null) input.IsEnabled = false;
        }
    }


    /// <summary>
    /// Registers a setting row into the shared search index.
    /// </summary>
    protected void RegisterSearchKey(Control target, LangId label, ConfigId? id, LangId? section)
    {
        VM.Registry.Register(new SettingItem
        {
            Id = id,
            Label = label,
            PageNavId = NavId,
            Page = PageLabel,
            Section = section,
            Target = target,
        });
    }


    /// <summary>
    /// Binds a checkbox to a boolean config id (staged on change).
    /// </summary>
    protected void BindToggle(CheckBox chk, ConfigId id, LangId label, LangId? section, bool defaultValue = false)
    {
        chk.IsChecked = VM.GetValue(id, defaultValue);
        chk.IsCheckedChanged += (_, _) => VM.SetValue(id, chk.IsChecked ?? false);

        RegisterSearchKey(chk, label, id, section);
    }


    /// <summary>
    /// Binds a text box to an integer config id (staged on valid change).
    /// </summary>
    protected void BindIntInput(PhTextBox box, ConfigId id, LangId label, LangId? section, int defaultValue = 0)
    {
        box.Text = VM.GetValue(id, defaultValue).ToString(CultureInfo.InvariantCulture);
        box.TextChanged += (_, _) =>
        {
            if (int.TryParse(box.Text, out var v)) VM.SetValue(id, v);
        };

        RegisterSearchKey(box, label, id, section);
    }


    /// <summary>
    /// Binds a text box to an unsigned-integer config id (staged on valid change).
    /// </summary>
    protected void BindUIntInput(PhTextBox box, ConfigId id, LangId label, LangId? section, uint defaultValue = 0)
    {
        box.Text = VM.GetValue(id, defaultValue).ToString(CultureInfo.InvariantCulture);
        box.TextChanged += (_, _) =>
        {
            if (uint.TryParse(box.Text, out var v)) VM.SetValue(id, v);
        };

        RegisterSearchKey(box, label, id, section);
    }


    /// <summary>
    /// Binds a text box to a double config id (staged on valid change).
    /// </summary>
    protected void BindDoubleInput(PhTextBox box, ConfigId id, LangId label, LangId? section,
        double defaultValue = 0, double minValue = double.NegativeInfinity)
    {
        var initial = Math.Max(minValue, VM.GetValue(id, defaultValue));
        box.Text = initial.ToString(CultureInfo.InvariantCulture);

        box.TextChanged += (_, _) =>
        {
            if (double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                VM.SetValue(id, Math.Max(minValue, v));
        };

        if (double.IsNegativeInfinity(minValue))
        {
            RegisterSearchKey(box, label, id, section);
            return;
        }

        // show the clamped value instead of leaving the box reading something that won't be used
        box.LostFocus += (_, _) =>
        {
            var isValid = double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v);
            if (isValid && v >= minValue) return;

            box.Text = minValue.ToString(CultureInfo.InvariantCulture);
        };

        RegisterSearchKey(box, label, id, section);
    }


    /// <summary>
    /// Binds a slider to a double config id (staged on change). The slider's <c>Minimum</c>/<c>Maximum</c>
    /// are expected to be set in XAML. When <paramref name="valueLabel"/> is supplied, its
    /// <c>{0}</c> placeholder receives the live value, formatted by <paramref name="format"/>
    /// (a whole number by default), so the label reads e.g. "Zoom speed: 200".
    /// </summary>
    protected void BindSlider(Slider slider, ConfigId id, LangId label, LangId? section,
        double defaultValue = 0, PhTextBlock? valueLabel = null, Func<double, string>? format = null)
    {
        format ??= v => v.ToString("0", CultureInfo.InvariantCulture);

        slider.Value = Math.Clamp(VM.GetValue(id, defaultValue), slider.Minimum, slider.Maximum);
        if (valueLabel is not null) valueLabel.LangParams = format(slider.Value);

        slider.ValueChanged += (_, _) =>
        {
            VM.SetValue(id, slider.Value);
            if (valueLabel is not null) valueLabel.LangParams = format(slider.Value);
        };

        RegisterSearchKey(slider, label, id, section);
    }


    /// <summary>
    /// Binds a slider to a <c>uint</c> config id (staged on change). Behaves like
    /// <see cref="BindSlider(Slider, ConfigId, LangId, LangId?, double, PhTextBlock?, Func{double, string})"/>
    /// but stages a rounded <c>uint</c> so the value round-trips through the integer config getter.
    /// </summary>
    protected void BindUIntSlider(Slider slider, ConfigId id, LangId label, LangId? section,
        uint defaultValue = 0, PhTextBlock? valueLabel = null)
    {
        var value = Math.Clamp((uint)VM.GetValue(id, defaultValue), (uint)slider.Minimum, (uint)slider.Maximum);
        slider.Value = value;
        if (valueLabel is not null) valueLabel.LangParams = value.ToString(CultureInfo.InvariantCulture);

        slider.ValueChanged += (_, _) =>
        {
            var v = (uint)Math.Round(slider.Value);
            VM.SetValue(id, v);
            if (valueLabel is not null) valueLabel.LangParams = v.ToString(CultureInfo.InvariantCulture);
        };

        RegisterSearchKey(slider, label, id, section);
    }


    /// <summary>
    /// Populates an enum dropdown with localized labels (from the <c>{EnumType}_{Value}</c>
    /// language key, falling back to the raw name) and binds the selection to a config id.
    /// </summary>
    protected void BindEnumDropdown<TEnum>(ComboBox combo, ConfigId id, TEnum defaultValue,
        LangId label, LangId? section) where TEnum : struct, Enum
    {
        var current = VM.GetValue(id, defaultValue);
        var selectedIndex = 0;

        var names = Enum.GetNames<TEnum>();
        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i];
            var value = Enum.Parse<TEnum>(name);
            var item = new ComboBoxItem { Tag = value };

            BindComboItemText(item, Lang.GetKey($"{typeof(TEnum).Name}_{name}"), name);
            combo.Items.Add(item);
            if (EqualityComparer<TEnum>.Default.Equals(value, current)) selectedIndex = i;
        }
        combo.SelectedIndex = selectedIndex;

        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is ComboBoxItem { Tag: TEnum value }) VM.SetValue(id, value);
        };

        RegisterSearchKey(combo, label, id, section);
    }


    /// <summary>
    /// Sets a combo item's display text to a localized string (falling back to
    /// <paramref name="fallback"/> when there's no key) and keeps it refreshed on language change.
    /// </summary>
    protected void BindComboItemText(ComboBoxItem item, LangId? key, string fallback)
        => AddLangRefresher(() => item.Content = key is { } k ? Core.Lang[k] : fallback);


    /// <summary>
    /// Configures a link-style button: localized text (kept refreshed) and a click action.
    /// </summary>
    protected void BindLink(PhButton btn, LangId label, Action onClick)
    {
        SetLocalizedText(btn, label);
        btn.Click += (_, _) => onClick();

        RegisterSearchKey(btn, label, null, null);
    }


    /// <summary>
    /// Configures a link-style button: localized text (kept refreshed), full-path tooltip, click action.
    /// </summary>
    protected void BindLink(PhButton btn, LangId label, string tooltip, Action onClick)
    {
        BindLink(btn, label, onClick);
        ToolTip.SetTip(btn, tooltip);
    }


    /// <summary>
    /// Binds a <see cref="PhColorPickerControl"/> to a hex-string config id (staged on change).
    /// The reset link restores <paramref name="defaultHex"/>.
    /// </summary>
    protected void BindColorPicker(PhColorPickerControl picker, ConfigId id, string defaultHex,
        LangId label, LangId? section)
    {
        picker.DefaultColor = BHelper.ColorFromHex(defaultHex);
        picker.SelectedColor = BHelper.ColorFromHex(VM.GetValue(id, defaultHex));

        // subscribe AFTER seeding the value, so opening the page (or a resync) doesn't stage a phantom change
        picker.ColorChanged += (_, _) =>
        {
            if (!_suppressColorStaging) VM.SetValue(id, picker.SelectedColor.ToHex());
        };
        AddLangRefresher(() => picker.Title = Core.Lang[label]);

        RegisterSearchKey(picker, label, id, section);
    }


    /// <summary>
    /// Re-seeds a color picker's selected color and reset target (from the current config and the
    /// given default) without staging a phantom edit. Use when the value changed outside the page,
    /// e.g. the background color now follows a newly applied theme.
    /// </summary>
    protected void ResyncColorPicker(PhColorPickerControl picker, ConfigId id, string defaultHex)
    {
        _suppressColorStaging = true;
        picker.DefaultColor = BHelper.ColorFromHex(defaultHex);
        picker.SelectedColor = BHelper.ColorFromHex(VM.GetValue(id, defaultHex));
        _suppressColorStaging = false;
    }


    /// <summary>
    /// Binds a transition effect dropdown (the effect files, re-read now) and its duration box.
    /// </summary>
    protected void BindTransitionEditor(ComboBox combo, PhTextBox durationBox, PhTextBlock errorText,
        PhButton openFolderBtn, PhButton getMoreBtn,
        ConfigId effectId, ConfigId durationId, uint defaultDuration, LangId section)
    {
        // picks up effect files added since the last time settings were opened
        TransitionEffects.Reload();

        var current = VM.GetValue(effectId, TransitionEffects.NONE);
        bool IsCurrent(string id) => id.Equals(current, StringComparison.OrdinalIgnoreCase);

        var effectOptions = TransitionEffects.All
            .Select(e => TransitionEffectOption.FromFile(e.Id, e.FilePath!))
            .ToList();

        // keep a saved id whose file is gone, rather than silently switching it to None
        var isKnown = IsCurrent(TransitionEffects.NONE)
            || IsCurrent(TransitionEffects.RANDOM)
            || effectOptions.Any(o => IsCurrent(o.Id));
        if (!isKnown) effectOptions.Add(TransitionEffectOption.FromId(current));

        var options = new List<TransitionEffectOption> { TransitionEffectOption.FromId(TransitionEffects.NONE) };

        // picking at random needs at least two effects to pick from
        if (TransitionEffects.All.Count >= 2 || IsCurrent(TransitionEffects.RANDOM))
        {
            options.Add(TransitionEffectOption.FromId(TransitionEffects.RANDOM));
        }

        if (effectOptions.Count > 0)
        {
            options.Add(TransitionEffectOption.CreateDivider());
            options.AddRange(effectOptions);
        }

        combo.ItemsSource = options;
        combo.SelectedItem = options.Find(o => IsCurrent(o.Id)) ?? options[0];

        // shows why the selected effect cannot play, and disables the duration of None
        void UpdateEffectState()
        {
            var id = (combo.SelectedItem as TransitionEffectOption)?.Id ?? TransitionEffects.NONE;
            var isNone = id.Equals(TransitionEffects.NONE, StringComparison.OrdinalIgnoreCase);
            var isRandom = id.Equals(TransitionEffects.RANDOM, StringComparison.OrdinalIgnoreCase);

            var effect = isNone || isRandom ? null : TransitionEffects.Find(id);
            var hasError = effect is not null && effect.GetEffect() is null;

            errorText.IsVisible = hasError;
            errorText.Text = hasError
                ? $"{Core.Lang[LangId.Settings_TransitionCompileError]} {Path.GetFileName(effect!.FilePath)}\n{effect.CompileError}"
                : string.Empty;
            durationBox.IsEnabled = !isNone && !Config.IsConfigLocked(durationId);
        }

        combo.SelectionChanged += (_, _) =>
        {
            if (combo.SelectedItem is TransitionEffectOption { IsDivider: false } option) VM.SetValue(effectId, option.Id);
            UpdateEffectState();
        };
        AddLangRefresher(() =>
        {
            options.ForEach(o => o.RefreshLang());
            UpdateEffectState();
        });
        RegisterSearchKey(combo, LangId.Settings_TransitionEffect, effectId, section);

        BindUIntInput(durationBox, durationId, LangId.Settings_TransitionDuration, section, defaultDuration);
        BindLink(openFolderBtn, LangId.Settings_OpenTransitionsFolder,
            () => BHelper.OpenFolderPath(BHelper.GetRealPlatformConfigDir(Dir.Transitions)));
        BindLink(getMoreBtn, LangId.Settings_GetMoreTransitionEffects, TRANSITIONS_URL,
            () => _ = BHelper.OpenUrlAsync(this, TRANSITIONS_URL, "from_setting_transitions"));
    }


    #endregion // Binding helpers

}
