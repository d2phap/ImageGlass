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
using Avalonia.Input;
using Avalonia.Threading;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using ImageMagick;
using System;
using System.Runtime.InteropServices;

namespace ImageGlass.Common.Windows;

public partial class AboutWindow : DialogWindow
{
    private const int HERO_INTRO_DELAY_SEC = 1;

    private IDisposable? _heroIntroTimer;


    private const string _creditContent = """
        ◍ Avalonia                                         MIT licence 
          https://github.com/AvaloniaUI/Avalonia
          Copyright (c) AvaloniaUI OÜ All Rights Reserved

        ◍ Magick.NET                                Apache-2.0 license
          https://github.com/dlemstra/Magick.NET
          Copyright (c) 2013-2026 Dirk Lemstra

        ◍ SkiaSharp                                        MIT licence
          https://github.com/mono/SkiaSharp
          Copyright (c) 2015-2016 Xamarin, Inc
          Copyright (c) 2017-2018 Microsoft Corporation

        ◍ Svg.Skia                                         MIT licence
          https://github.com/wieslawsoltes/Svg.Skia
          Copyright (c) 2020 Wiesław Šoltés

        ◍ NativeMemoryArray                                MIT licence
          https://github.com/Cysharp/NativeMemoryArray
          Copyright (c) 2021 Cysharp, Inc.

        ◍ NetCoreAudio                                     MIT licence
          https://github.com/mobiletechtracker/NetCoreAudio
          Copyright (c) 2020-2024 Fiodar Sazanavets
                                  (Scientific Programmer Ltd)

        ◍ CsWin32                                          MIT licence
          https://github.com/microsoft/CsWin32
          Copyright (c) Microsoft Corporation

        ◍ Fluent Emoji                                     MIT licence
          https://github.com/microsoft/fluentui-emoji
          Copyright (c) Microsoft Corporation

        ◍ D2Phap.EggShell                           Apache-2.0 license
          Copyright (c) 2024-2026 Dương Diệu Pháp

        ◍ D2Phap.FileWatcherEx                             MIT licence
          https://github.com/d2phap/FileWatcherEx
          Copyright (c) 2018-2026 Dương Diệu Pháp

        ◍ ImageGlass logo idea
          Nguyễn Quốc Tuấn
        """;


    public AboutWindow()
    {
        InitializeComponent();

        IsButton1Visible = true;
        IsButton2Visible = false;
        IsButton3Visible = false;

        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Button1;
        ShowInTaskbar = true;

        PART_LblCopyright.Text = $"Copyright © 2010-{DateTime.UtcNow.Year} Dương Diệu Pháp";
        PART_LblCreditContent.Text = _creditContent;

        SetupEditionChip();
        SetupHero();

        // PhButton.ApplyVariant clears Padding, so the link spacing cannot live in the markup
        PhButton[] linkButtons = [PART_BtnWebsite, PART_BtnGitHub, PART_BtnEula, PART_BtnPrivacy];
        foreach (var btn in linkButtons) btn.Padding = new Thickness(6, 2);

        PART_BtnWebsite.Click += (_, _) => OpenUrl("https://imageglass.org");
        PART_BtnGitHub.Click += (_, _) => OpenUrl("https://github.com/d2phap/ImageGlass");
        PART_BtnEula.Click += (_, _) => OpenUrl("https://imageglass.org/license");
        PART_BtnPrivacy.Click += (_, _) => OpenUrl("https://imageglass.org/privacy");
        PART_BtnDonate.Click += (_, _) => OpenUrl("https://imageglass.org/donate");

        PART_BtnCheckForUpdate.Click += async (_, _) =>
        {
            _ = await Core.API.RunApiAsync(API.IG_CheckForUpdate, "true");
        };
    }


    #region Override Methods

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        _heroIntroTimer?.Dispose();
        _heroIntroTimer = DispatcherTimer.RunOnce(PlayHeroBurst,
            TimeSpan.FromSeconds(HERO_INTRO_DELAY_SEC));
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // RunOnce is dispatcher-global, so a closed window would be held until it fires
        _heroIntroTimer?.Dispose();
        _heroIntroTimer = null;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Menu_MnuAbout];
        Button1Text = Core.Lang[LangId._Close];

        PART_LblSlogan.Text = Core.Lang[LangId._Slogan];
        PART_LblCredits.Text = Core.Lang[LangId._Credits];
        PART_BtnWebsite.Text = Core.Lang[LangId._Homepage];
        PART_BtnGitHub.Text = "GitHub";
        PART_BtnEula.Text = Core.Lang[LangId._License];
        PART_BtnPrivacy.Text = Core.Lang[LangId._Privacy];
        PART_BtnDonate.Text = "❤️ " + Core.Lang[LangId._Donate];
        PART_BtnCheckForUpdate.Text = Core.Lang[LangId._CheckForUpdate];

        // the edition name is not translatable, but the tooltip says what clicking it does
        ToolTip.SetTip(PART_BtnEdition, Core.Lang[Core.IsProEnabled
            ? LangId.Menu_MnuManageLicense
            : LangId.Menu_MnuUpgradeLicense]);

        UpdateVersionText();
    }

    #endregion // Override Methods



    #region Private Methods

    private void UpdateVersionText()
    {
        var magickVersion = MagickNET.Version;
        var dotnetVersion = RuntimeInformation.FrameworkDescription;

        PART_LblVersion.Text = $"""
            {Core.Lang[LangId._AboutVersion]} {Core.BuildInfo.FullVersion}
            {magickVersion}
            {dotnetVersion}
            """;
    }


    /// <summary>
    /// Wires up the hero decoration; a failure here must never block the About actions.
    /// </summary>
    private void SetupHero()
    {
        try
        {
            // the flying stars are a Pro flourish; Classic gets the floating logo alone
            PART_HeroStars.EnableStars = Core.IsProEnabled;
            PART_HeroStars.BobTarget = PART_Logo;
            PART_Hero.PointerPressed += (_, _) => PlayHeroBurst();
        }
        catch { }
    }


    private void PlayHeroBurst()
    {
        // an early click must not be restarted by the pending intro timer
        _heroIntroTimer?.Dispose();
        _heroIntroTimer = null;

        PART_HeroStars?.Play();
    }


    /// <summary>
    /// Fills in the edition chip: a filled accent pill for Pro, a plain one for Classic.
    /// </summary>
    private void SetupEditionChip()
    {
        var isPro = Core.IsProEnabled;

        // "Classic" and "Pro" are edition names, not translatable copy
        PART_BtnEdition.Text = isPro ? "Pro" : "Classic";
        PART_BtnEdition.Variant = isPro ? PhButtonVariant.Accent : PhButtonVariant.Outline;

        // applying the variant clears both, so the pill shape has to be restored after it
        PART_BtnEdition.Padding = new Thickness(14, 2);
        PART_BtnEdition.Cursor = new Cursor(StandardCursorType.Hand);

        // the accent variant fills the background but leaves the text colour alone
        if (isPro)
        {
            PART_BtnEdition[!PhButton.ForegroundProperty] = Resx.CreateBinding(ResxId.AccentButtonForeground);
        }

        // owned by this dialog, not the main window, so it stacks on top of the modal About
        PART_BtnEdition.Click += async (_, _) =>
        {
            try
            {
                _ = await new ManageLicenseWindow().ShowAsync(this);
            }
            catch
            {
                // an async void handler must never reach the unhandled-error dialog
            }
        };
    }


    private void OpenUrl(string url) => _ = BHelper.OpenUrlAsync(this, url, "from_about");


    #endregion // Private Methods

}
