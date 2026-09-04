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
using Avalonia.Platform.Storage;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders.Licensing;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

/// <summary>
/// The Pro upgrade pitch: description, feature-comparison link and the buy / import / retrieve
/// actions. Shared by the Manage-license window and the Quick Setup wizard.
/// </summary>
public partial class UpgradeToProControl : PhControl
{
    /// <summary>
    /// Gets, sets the utm campaign tag applied to the website links.
    /// </summary>
    public string Campaign { get; set; } = "from_upgrade_dialog";


    /// <summary>
    /// Gets, sets the corner radius of the CTA panel. Zero (default) keeps the square, full-bleed
    /// dialog section; a host embedding the control can round it into a card.
    /// </summary>
    public CornerRadius CtaCornerRadius
    {
        get => GetValue(CtaCornerRadiusProperty);
        set => SetValue(CtaCornerRadiusProperty, value);
    }
    public static readonly StyledProperty<CornerRadius> CtaCornerRadiusProperty =
        AvaloniaProperty.Register<UpgradeToProControl, CornerRadius>(nameof(CtaCornerRadius));


    /// <summary>
    /// Gets, sets whether the rule above the CTA panel is shown.
    /// </summary>
    public bool IsDividerVisible
    {
        get => GetValue(IsDividerVisibleProperty);
        set => SetValue(IsDividerVisibleProperty, value);
    }
    public static readonly StyledProperty<bool> IsDividerVisibleProperty =
        AvaloniaProperty.Register<UpgradeToProControl, bool>(nameof(IsDividerVisible), true);


    /// <summary>
    /// Gets, sets whether a successful import offers to restart the app. Quick Setup turns this
    /// off: it restarts on Save, and restarting here would drop the steps already filled in.
    /// </summary>
    public bool PromptRestartAfterImport
    {
        get => GetValue(PromptRestartAfterImportProperty);
        set => SetValue(PromptRestartAfterImportProperty, value);
    }
    public static readonly StyledProperty<bool> PromptRestartAfterImportProperty =
        AvaloniaProperty.Register<UpgradeToProControl, bool>(nameof(PromptRestartAfterImport), true);


    /// <summary>
    /// Gets, sets a language pack to localize from instead of the app language. Quick Setup
    /// previews a pack this way, so the wizard text follows the picker before anything is saved.
    /// </summary>
    public Lang? PreviewLang
    {
        get; set
        {
            field = value;
            Localize();
        }
    }


    private Lang CurrentLang => PreviewLang ?? Core.Lang;

    // set once an import succeeded without a restart, so the button keeps naming that license
    private string? _importedLicenseId;


    public UpgradeToProControl()
    {
        InitializeComponent();

        Localize();

        PART_BtnViewFeatures.Click += (_, _) => OpenUrl("https://imageglass.org/pro");
        PART_BtnBuyOnline.Click += (_, _) => OpenUrl("https://imageglass.org/pricing");
        PART_BtnRetrieveEmail.Click += (_, _) => OpenUrl("https://imageglass.org/pro/retrieve");
        PART_BtnImportLicense.Click += async (_, _) => await ImportAndShowResultAsync();
    }



    #region Overrides

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        Localize();
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // the divider carried the gap above the CTA panel, so take it over when it's hidden
        if (e.Property == IsDividerVisibleProperty && PART_Cta is not null)
        {
            PART_Cta.Margin = IsDividerVisible ? default : new Thickness(0, 20, 0, 0);
        }
    }

    #endregion // Overrides



    #region Public methods

    /// <summary>
    /// Pick a license file, verify its signature and install it. When <paramref name="promptRestart"/>
    /// is set, also offers the restart that activates it. Returns the imported license, or
    /// <see langword="null"/> when nothing was installed.
    /// </summary>
    public static async Task<LicenseInfo?> ImportLicenseAsync(Visual source, bool promptRestart = true)
    {
        var topLevel = TopLevel.GetTopLevel(source);
        if (topLevel is null) return null;

        var owner = topLevel as PhWindow;
        var title = Core.Lang[Core.IsProEnabled
            ? LangId.Menu_MnuManageLicense
            : LangId.Menu_MnuUpgradeLicense];

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(Core.Lang[LangId.Menu_MnuManageLicense_LicenseFileType])
                {
                    Patterns = ["*" + LicenseService.LICENSE_FILE_EXTENSION],
                },
            ],
        });

        var path = (files.Count > 0 ? files[0] : null)?.TryGetLocalPath();
        if (string.IsNullOrEmpty(path)) return null;

        // the signature is the source of truth; reject anything that doesn't verify
        if (!LicenseService.TryVerify(path, out var lic, out var errorCode))
        {
            await ShowErrorAsync(owner, title, LangId.Menu_MnuManageLicense_ImportFailed, path, errorCode);
            return null;
        }

        // authentic but past its expiry instant, so it can't activate Pro
        if (!LicenseService.IsWithinValidity(lic))
        {
            await ShowErrorAsync(owner, title, LangId.Menu_MnuManageLicense_ImportExpired, path);
            return null;
        }

        // install into the config dir; the license it replaces is uninstalled
        var isInstalled = LicenseService.TryInstall(path, lic, out var installError);
        if (!isInstalled)
        {
            await ShowErrorAsync(owner, title, LangId.Menu_MnuManageLicense_ImportFailed,
                installError?.Message, installError?.ToString());
            return null;
        }

        // the caller restarts on its own terms (Quick Setup saves first)
        if (!promptRestart) return lic;

        // the license only loads at startup, so offer to restart now
        var result = await ModalWindow.ShowInfoAsync(owner, new ModalWindowOptions
        {
            Title = title,
            Heading = Core.Lang[LangId.Menu_MnuManageLicense_ImportSuccess],
        }, ModalWindowButton.Yes_No);

        if (result.ExitCode == DialogExitCode.OK) BHelper.RestartApp();

        return lic;
    }


    /// <summary>
    /// Shows a licensing error dialog. The licensing UI must never raise the unhandled-error dialog.
    /// </summary>
    public static async Task ShowErrorAsync(PhWindow? owner, string title, LangId messageKey,
        string? description = null, string? details = null)
    {
        await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
        {
            Title = title,
            Heading = Core.Lang[messageKey],
            Description = description,
            Details = details,
        });
    }

    #endregion // Public methods



    #region Methods

    /// <summary>
    /// Imports a license, then reports the outcome on the button when no restart was offered.
    /// </summary>
    private async Task ImportAndShowResultAsync()
    {
        var lic = await ImportLicenseAsync(this, PromptRestartAfterImport);
        if (lic is null || PromptRestartAfterImport) return;

        // nothing else to do here: the license is installed and activates on the next launch
        _importedLicenseId = lic.LicenseId;
        PART_LblImportLicense.Text = _importedLicenseId;
        PART_BtnImportLicense.IsEnabled = false;
    }


    /// <summary>
    /// Applies every string from <see cref="CurrentLang"/>.
    /// </summary>
    private void Localize()
    {
        var lang = CurrentLang;

        PART_LblDescription.Text = lang[LangId.Menu_MnuUpgradeLicense_Description];
        PART_BtnViewFeatures.Text = lang[LangId.Menu_MnuUpgradeLicense_ViewFeatures];
        PART_LblBuyOnline.Text = lang[LangId.Menu_MnuUpgradeLicense_BuyOnline];
        PART_LblRetrieveEmail.Text = lang[LangId.Menu_MnuUpgradeLicense_RetrieveFromEmail];

        // an imported license names itself on the button, so it must survive a language switch
        PART_LblImportLicense.Text = _importedLicenseId
            ?? lang[LangId.Menu_MnuUpgradeLicense_ImportLicense];

        // an authentic license the app cannot use: name the reason, lapsed first, over a pitch
        var expiredLic = Core.ExpiredLicense;
        var outOfScopeLic = Core.OutOfScopeLicense;
        var isExpired = expiredLic is not null;

        PART_LblExpired.IsVisible = isExpired;
        PART_LblOutOfScope.IsVisible = !isExpired && outOfScopeLic is not null;
        PART_LblDescription.IsVisible = !isExpired && outOfScopeLic is null;

        if (expiredLic is not null)
        {
            PART_LblExpired.Text = lang[LangId.Menu_MnuUpgradeLicense_Expired,
                expiredLic.Plan, FormatExpiry(expiredLic)];
        }
        if (outOfScopeLic is not null)
        {
            PART_LblOutOfScope.Text = lang[LangId.Menu_MnuUpgradeLicense_OutOfScope,
                outOfScopeLic.Plan, outOfScopeLic.VersionScope, LicenseScope.GetRunningAppMajorText()];
        }
    }


    /// <summary>
    /// Formats the expiry of a license in local time, falling back to the raw value.
    /// </summary>
    private static string FormatExpiry(LicenseInfo license)
    {
        var expiresAt = LicenseService.GetExpiryUtc(license);
        if (expiresAt is null) return license.ExpiresAt ?? string.Empty;

        return BHelper.FormatDateTime(expiresAt.Value.ToLocalTime().DateTime);
    }


    private void OpenUrl(string url) => _ = BHelper.OpenUrlAsync(this, url, Campaign);

    #endregion // Methods

}
