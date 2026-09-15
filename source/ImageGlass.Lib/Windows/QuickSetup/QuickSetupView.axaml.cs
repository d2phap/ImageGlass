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
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Windows;

public partial class QuickSetupView : PhControl
{
    private const string NEWS_URL = "https://imageglass.org/news";

    private readonly List<Control> _stepPanels;
    private readonly List<Border> _dots = [];

    private bool _isPopulatingLangs;
    private string _selectedLangValue = "English";
    private readonly Action _loadLangAction;

    // local language used to preview the wizard only; the app is untouched until Save
    private Lang _previewLang = Core.Lang;

    // registration scope (per-user vs per-machine), derived from where the app is installed
    private static DefaultAppScope DefaultViewerScope
        => Core.ShellProvider?.GetDefaultViewerScope() ?? DefaultAppScope.CurrentUser;


    /// <summary>
    /// Gets the language currently previewed in the wizard (not applied to the app until Save).
    /// </summary>
    public Lang PreviewLang => _previewLang;


    /// <summary>
    /// Raised when the previewed language changes, so the host window can re-localize its footer.
    /// </summary>
    public event EventHandler? PreviewLanguageChanged;


    /// <summary>
    /// Gets the total number of steps (language + profile, plus the optional default-viewer and
    /// upgrade-to-Pro steps).
    /// </summary>
    public int StepCount => _stepPanels.Count;


    /// <summary>
    /// Whether the default-photo-viewer step applies: Windows only, and only where the shell honors
    /// our associations (excludes the virtualized Store build).
    /// </summary>
    private static bool CanSetDefaultViewer => BHelper.OS == OSType.Windows
        && Core.ShellProvider?.IsDefaultViewerConfigurable != false;


    /// <summary>
    /// Whether the applications-menu step applies: only a build that installs nothing has an entry
    /// to add (currently the Linux AppImage).
    /// </summary>
    private static bool CanRegisterAppMenuEntry => Core.ShellProvider?.CanRegisterAppMenuEntry == true;


    /// <summary>
    /// Gets the current 1-based step index.
    /// </summary>
    public int CurrentStep { get; private set; } = 1;


    /// <summary>
    /// Gets the selected display-language config value (a pack file name, or "English").
    /// </summary>
    public string SelectedLanguageValue => _selectedLangValue;


    /// <summary>
    /// Gets whether the "Professional user" profile is selected.
    /// </summary>
    public bool IsProfessional { get; private set; }


    public QuickSetupView()
    {
        InitializeComponent();

        _loadLangAction = () => _ = LoadSelectedLanguageAsync();

        _stepPanels = [PART_Step1, PART_Step2];

        // the default-viewer step is Windows-only and unreachable from the Store build
        if (CanSetDefaultViewer) _stepPanels.Add(PART_Step3);
        else PART_Step3.IsVisible = false;

        // the applications-menu step is the AppImage counterpart of the default-viewer step
        if (CanRegisterAppMenuEntry) _stepPanels.Add(PART_StepAppMenu);
        else PART_StepAppMenu.IsVisible = false;

        // step 4 pitches Pro, so there is nothing to show once it is active
        if (!Core.IsProEnabled) _stepPanels.Add(PART_Step4);
        else PART_Step4.IsVisible = false;

        BuildStepDots();

        // step 1: language
        PART_LanguageList.SelectionChanged += Language_SelectionChanged;
        PART_SeeWhatNew.Click += async (_, _) => await BHelper.OpenUrlAsync(this, NEWS_URL, "from_quick_setup");

        // step 2: profile
        PART_BtnStandard.Click += (_, _) => SelectProfile(false);
        PART_BtnProfessional.Click += (_, _) => SelectProfile(true);
        SelectProfile(false);

        // step 3: default photo viewer (applied immediately, it is not a config setting)
        PART_BtnRegisterViewer.Click += async (_, _) => await AppAPIProvider.SetDefaultPhotoViewerAsync(
            true, TopLevel.GetTopLevel(this) as PhWindow, _previewLang);

        // applications menu (applied immediately, it is not a config setting)
        PART_BtnRegisterAppMenu.Click += async (_, _) => await RegisterAppMenuEntryAsync();

        LocalizeAll();
        SetStep(1);
        _ = LoadLanguagesAsync();
    }



    #region Overrides

    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        UpdateStepDots();
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // stronger entrance animation on first open
        AnimateStepIn(_stepPanels[CurrentStep - 1], isFirstOpen: true);
    }

    #endregion // Overrides



    #region Public methods

    /// <summary>
    /// Shows the given 1-based step with a soft slide/fade transition.
    /// </summary>
    public void ShowStep(int step)
    {
        SetStep(step);
        AnimateStepIn(_stepPanels[CurrentStep - 1], isFirstOpen: false);
    }

    #endregion // Public methods



    #region Step display

    /// <summary>
    /// Sets the visible step and refreshes the progress header (no animation).
    /// </summary>
    private void SetStep(int step)
    {
        CurrentStep = Math.Clamp(step, 1, StepCount);

        for (var i = 0; i < _stepPanels.Count; i++)
        {
            _stepPanels[i].IsVisible = i == CurrentStep - 1;
        }

        UpdateStepInfo();
        UpdateStepDots();
    }


    /// <summary>
    /// Fades and slides a step panel into view. The first open uses a larger, longer motion;
    /// step navigation uses a softer one.
    /// </summary>
    private static void AnimateStepIn(Control panel, bool isFirstOpen)
    {
        var duration = TimeSpan.FromMilliseconds(isFirstOpen ? 420 : 260);
        var offset = isFirstOpen ? 26 : 12;
        var easing = new CubicEaseOut();

        // apply the start state instantly (transitions off), then animate to rest
        panel.Transitions = null;
        panel.Opacity = 0;
        panel.RenderTransform = TransformOperations.Parse($"translateY({offset}px)");

        panel.Transitions =
        [
            new DoubleTransition { Property = OpacityProperty, Duration = duration, Easing = easing },
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = duration, Easing = easing },
        ];

        Dispatcher.UIThread.Post(() =>
        {
            panel.Opacity = 1;
            panel.RenderTransform = TransformOperations.Parse("translateY(0px)");
        }, DispatcherPriority.Render);
    }

    #endregion // Step display



    #region Language

    private void Language_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isPopulatingLangs || PART_LanguageList.SelectedItem is not Lang lang) return;

        _selectedLangValue = ToConfigValue(lang);
        BHelper.Debounce(120, _loadLangAction);
    }


    /// <summary>
    /// Loads the selected language pack and previews it in the wizard only (the running app is
    /// not affected until Save).
    /// </summary>
    private async Task LoadSelectedLanguageAsync()
    {
        var path = Lang.ResolveFilePath(_selectedLangValue);
        var lang = new Lang(path);
        await lang.LoadAsync();

        Dispatcher.UIThread.Post(() =>
        {
            _previewLang = lang;
            LocalizeAll();
            PreviewLanguageChanged?.Invoke(this, EventArgs.Empty);
        });
    }


    /// <summary>
    /// Loads installed language packs and selects the current one.
    /// </summary>
    private async Task LoadLanguagesAsync()
    {
        var packs = await Lang.LoadAllLanguagePacksAsync();

        // built-in English first (so users can revert)
        List<Lang> langs = [new(string.Empty), .. packs];

        _isPopulatingLangs = true;
        PART_LanguageList.ItemsSource = langs;

        var current = Core.Config.Language;
        PART_LanguageList.SelectedItem = langs
            .FirstOrDefault(l => ToConfigValue(l).Equals(current, StringComparison.OrdinalIgnoreCase))
            ?? langs[0];

        _selectedLangValue = current;
        _isPopulatingLangs = false;
    }


    private static string ToConfigValue(Lang lang) => lang.IsBuiltIn ? "English" : lang.FileName;

    #endregion // Language



    #region Profile

    /// <summary>
    /// Selects a setting profile and refreshes the "will be applied" checklist.
    /// </summary>
    private void SelectProfile(bool professional)
    {
        IsProfessional = professional;

        PART_BtnStandard.IsChecked = !professional;
        PART_BtnProfessional.IsChecked = professional;

        PART_ChkColorManagement.IsChecked = professional;
        PART_ChkHdrToneMapping.IsChecked = professional;
        PART_ChkExplorerSortOrder.IsChecked = professional;
        PART_ChkRawThumbnail.IsChecked = !professional;
    }

    #endregion // Profile


    #region Applications menu

    /// <summary>
    /// Registers the app in the system applications menu, reusing the shared result dialog. The
    /// button latches off once it succeeds, and comes back on failure so the user can retry.
    /// </summary>
    private async Task RegisterAppMenuEntryAsync()
    {
        PART_BtnRegisterAppMenu.IsEnabled = false;

        var ok = await AppAPIProvider.RegisterAppMenuEntryAsync(true,
            TopLevel.GetTopLevel(this) as PhWindow, _previewLang);

        PART_BtnRegisterAppMenu.IsEnabled = !ok;
    }

    #endregion // Applications menu




    #region Header

    /// <summary>
    /// Creates one progress dot per step.
    /// </summary>
    private void BuildStepDots()
    {
        for (var i = 0; i < StepCount; i++)
        {
            var dot = new Border
            {
                Height = 8,
                CornerRadius = new CornerRadius(4),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            _dots.Add(dot);
            PART_StepDots.Children.Add(dot);
        }
    }


    /// <summary>
    /// Colors/sizes the dots to reflect the current step (active dot is a wider accent pill).
    /// </summary>
    private void UpdateStepDots()
    {
        var accent = Core.AccentColor.ToBrush();
        var inactive = Resx.Get<IBrush>(ResxId.IG_BorderNeutralBrush);

        for (var i = 0; i < _dots.Count; i++)
        {
            var isActive = i == CurrentStep - 1;
            _dots[i].Width = isActive ? 22 : 8;
            _dots[i].Background = isActive ? accent : inactive;
        }
    }


    private void UpdateStepInfo()
    {
        PART_StepInfo.Text = _previewLang[LangId.QuickSetup_StepInfo, CurrentStep, StepCount];
    }

    #endregion // Header



    #region Localization

    /// <summary>
    /// Localizes every wizard string from the local preview language.
    /// </summary>
    private void LocalizeAll()
    {
        var lang = _previewLang;

        PART_LblLanguage.Text = lang[LangId.QuickSetup_SelectLanguage];
        PART_SeeWhatNew.Text = lang[LangId.QuickSetup_SeeWhatNew];

        PART_LblSelectProfile.Text = lang[LangId.QuickSetup_SelectProfile];
        PART_LblStandard.Text = lang[LangId.QuickSetup_StandardUser];
        PART_LblProfessional.Text = lang[LangId.QuickSetup_ProfessionalUser];
        PART_LblApplied.Text = lang[LangId.QuickSetup_SettingsWillBeApplied];
        PART_LblColorManagement.Text = lang[LangId.Settings_ColorManagement];
        PART_LblHdrToneMapping.Text = lang[LangId.Settings_EnableHdrToneMapping];
        PART_LblExplorerSort.Text = lang[LangId.Settings_EnableExplorerSortOrder];
        PART_LblRawThumbnail.Text = lang[LangId.Settings_EnableOnlyLoadRawPreview];
        PART_LblProfileNote.Text = lang[LangId.QuickSetup_SettingProfileDescription];

        PART_LblDefaultViewer.Text = lang[LangId.Settings_DefaultPhotoViewer];
        PART_LblDefaultViewerDesc.Text = lang[LangId.QuickSetup_RegisterImageFormats];
        PART_LblDefaultViewerWarning.Text = lang[LangId.Settings_UnmanagedSettingReminder];
        PART_LblDefaultViewerScope.Text = lang[DefaultViewerScope == DefaultAppScope.LocalMachine
            ? LangId.Settings_DefaultPhotoViewer_ScopePerMachine
            : LangId.Settings_DefaultPhotoViewer_ScopePerUser];
        PART_BtnRegisterViewer.Text = lang[LangId._Register];

        PART_LblAppMenu.Text = lang[LangId.Settings_AppMenuEntry];
        PART_LblAppMenuDesc.Text = lang[LangId.Settings_AppMenuEntry_Description];
        PART_LblAppMenuWarning.Text = lang[LangId.Settings_UnmanagedSettingReminder];
        PART_BtnRegisterAppMenu.Text = lang[LangId._Register];

        PART_LblUpgradePro.Text = lang[LangId.Menu_MnuUpgradeLicense];
        PART_UpgradePro.PreviewLang = lang;

        UpdateStepInfo();
    }

    #endregion // Localization

}
