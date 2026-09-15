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
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders.Licensing;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class ManageLicenseView : PhControl
{
    private const int HERO_INTRO_DELAY_SEC = 1;

    private IDisposable? _heroIntroTimer;


    public ManageLicenseView()
    {
        InitializeComponent();

        var isPro = Core.IsProEnabled;
        PART_UpgradeBody.IsVisible = !isPro;
        PART_ManageBody.IsVisible = isPro;

        UpdateHeading();
        if (isPro) FillLicenseInfo();
        SetupHero();

        PART_BtnPlan.Click += (_, _) => OpenUrl("https://imageglass.org/license");
        PART_BtnChangeLicense.Click += async (_, _) => await UpgradeToProControl.ImportLicenseAsync(this);
        PART_BtnUpgradePlan.Click += (_, _) => OpenUrl("https://imageglass.org/pricing");
        PART_BtnExportLicense.Click += async (_, _) => await ExportLicenseAsync();
    }


    #region Overrides

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // OnLoaded can fire again on a tree re-attach, so never leave a timer armed twice
        _heroIntroTimer?.Dispose();
        _heroIntroTimer = DispatcherTimer.RunOnce(PlayHeroBurst,
            TimeSpan.FromSeconds(HERO_INTRO_DELAY_SEC));
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        // RunOnce is dispatcher-global, so a closed dialog would be held until it fires
        _heroIntroTimer?.Dispose();
        _heroIntroTimer = null;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // the license values are not localized, but the "Perpetual" fallback and the source are
        if (Core.IsProEnabled) FillLicenseInfo();

        UpdateHeading();
    }

    #endregion // Overrides



    #region Methods

    /// <summary>
    /// An expired license names the expiry, so its owner is not greeted with a sales pitch.
    /// </summary>
    private void UpdateHeading()
    {
        var headingId = LangId.Menu_MnuUpgradeLicense;
        if (Core.IsProEnabled) headingId = LangId.Menu_MnuManageLicense;
        else if (Core.ExpiredLicense is not null) headingId = LangId.Menu_MnuUpgradeLicense_ExpiredTitle;

        PART_LblHeading.Text = Core.Lang[headingId];
    }


    private void FillLicenseInfo()
    {
        var lic = Core.AppLicense;
        var isStoreBuild = Core.StoreEntitlementProvider?.IsStoreEntitled == true;

        // every customer of a store shares one bundled license, so showing its owner and id as if
        // they belonged to this user would misattribute them
        PART_ValLicensedTo.Text = isStoreBuild ? string.Empty : lic?.CustomerName ?? string.Empty;
        PART_ValLicenseId.Text = isStoreBuild ? string.Empty : lic?.LicenseId ?? string.Empty;

        PART_BtnPlan.Text = lic?.Plan ?? string.Empty;
        PART_ValSeats.Text = (lic?.SeatCount ?? 1).ToString();
        PART_ValExpires.Text = string.IsNullOrEmpty(lic?.ExpiresAt)
            ? Core.Lang[LangId.Menu_MnuManageLicense_Perpetual]
            : FormatExpiry(lic);
        PART_ValSource.Text = isStoreBuild
            ? LicenseService.GetChannelDisplayName(Core.StoreEntitlementProvider?.ChannelId)
            : string.Empty;

        // the store grants the entitlement, so there is no file to swap and no plan to upgrade
        PART_BtnChangeLicense.IsVisible = !isStoreBuild;
        PART_BtnUpgradePlan.IsVisible = !isStoreBuild;

        var canExport = LicenseService.TryGetExportableLicense(out _, out _);
        PART_BtnExportLicense.IsVisible = canExport;
        PART_LblExportNote.IsVisible = canExport;

        // otherwise the divider floats above an empty action area
        var hasAnyAction = canExport || !isStoreBuild;
        PART_ManageDivider.IsVisible = hasAnyAction;

        HideEmptyLicenseRows();
    }


    // a store entitlement has no owner or id to show, so those rows would render as bare labels
    private void HideEmptyLicenseRows()
    {
        (PhTextBlock Label, SelectableTextBlock Value)[] rows =
        [
            (PART_LblLicensedTo, PART_ValLicensedTo),
            (PART_LblSeats, PART_ValSeats),
            (PART_LblExpires, PART_ValExpires),
            (PART_LblLicenseId, PART_ValLicenseId),
            (PART_LblSource, PART_ValSource),
        ];

        foreach (var (label, value) in rows)
        {
            var hasValue = !string.IsNullOrWhiteSpace(value.Text);
            label.IsVisible = hasValue;
            value.IsVisible = hasValue;
        }
    }


    /// <summary>
    /// Wires up the hero decoration; a failure here must never block the license actions.
    /// </summary>
    private void SetupHero()
    {
        try
        {
            PART_HeroStars.BobTarget = PART_Logo;
            PART_Header.PointerPressed += (_, _) => PlayHeroBurst();
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


    private void OpenUrl(string url)
    {
        var campaign = Core.IsProEnabled ? "from_manage_license" : "from_upgrade_dialog";
        _ = BHelper.OpenUrlAsync(this, url, campaign);
    }


    /// <summary>
    /// Save the bundled license so the user can import it on other installations.
    /// </summary>
    private async Task ExportLicenseAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var owner = topLevel as PhWindow;
        var title = Core.Lang[LangId.Menu_MnuManageLicense_ExportLicense];

        // the licensing UI must never raise the unhandled-error dialog, so nothing may escape here
        try
        {
            // read the live state: this view decided what to show back in its constructor
            var canExport = LicenseService.TryGetExportableLicense(out var sourcePath, out var lic);
            if (!canExport)
            {
                await UpgradeToProControl.ShowErrorAsync(owner, title, LangId.Menu_MnuManageLicense_ExportFailed);
                return;
            }

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = lic.LicenseId + LicenseService.LICENSE_FILE_EXTENSION,
                // so a name the user edits still gets the suffix the import picker filters on
                DefaultExtension = LicenseService.LICENSE_FILE_EXTENSION.TrimStart('.'),
                FileTypeChoices =
                [
                    new FilePickerFileType(Core.Lang[LangId.Menu_MnuManageLicense_LicenseFileType])
                    {
                        Patterns = [LicenseService.LICENSE_FILE_EXTENSION],
                    },
                ],
            });

            if (file is null) return;

            // copy the bytes as they are; re-serializing would change them and break the signature.
            // the scope closes the stream before the success message, so a failed flush is reported
            var bytes = await File.ReadAllBytesAsync(sourcePath);
            await using (var dest = await file.OpenWriteAsync())
            {
                await dest.WriteAsync(bytes);
            }

            await ModalWindow.ShowInfoAsync(owner, new ModalWindowOptions
            {
                Title = title,
                Heading = Core.Lang[LangId.Menu_MnuManageLicense_ExportSuccess],
                Description = file.TryGetLocalPath(),
            });
        }
        catch (Exception ex)
        {
            await UpgradeToProControl.ShowErrorAsync(owner, title,
                LangId.Menu_MnuManageLicense_ExportFailed, ex.Message, ex.ToString());
        }
    }


    // the expiry is instant-precise, so the time belongs on screen next to the date
    private static string FormatExpiry(LicenseInfo license)
    {
        var expiresAt = LicenseService.GetExpiryUtc(license);
        if (expiresAt is null) return license.ExpiresAt ?? string.Empty;

        return BHelper.FormatDateTime(expiresAt.Value.ToLocalTime().DateTime);
    }


    #endregion // Methods

}
