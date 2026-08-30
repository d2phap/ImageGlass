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
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Licensing;
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Windows;

public partial class UpdateWindow : ModalWindow
{
    private UpdateCheckResult? _result;
    private bool _isReadyToInstall;
    private bool _canDownloadAndInstall;
    private CancellationTokenSource? _cancelDownload;

    protected override int MIN_WIDTH => 550;
    protected override int MAX_WIDTH => 550;


    /// <summary>
    /// Whether the app runs from the Microsoft Store package.
    /// </summary>
    private static bool IsMsStoreBuild => string.Equals(Core.ShellProvider?.InstallChannelId,
        LicenseService.CHANNEL_MSSTORE, StringComparison.OrdinalIgnoreCase);


    /// <summary>
    /// Gets whether the user chose to skip this version.
    /// </summary>
    public bool IsSkipped { get; private set; }


    public UpdateWindow()
    {
        InitializeComponent();

        ShowInTaskbar = true;
        Title = Core.Lang[LangId._CheckForUpdate];
        Description = Core.Lang[LangId.Menu_MnuCheckForUpdate_CurrentVersion, Core.BuildInfo.Version];

        PART_BtnTitle.Click += (_, _) => OpenChangeLog();
        PART_BtnChangelog.Click += (_, _) => OpenChangeLog();
        PART_BtnSkipVersion.Click += (_, _) => SkipVersion();
    }



    #region Override Methods

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        PART_BtnSkipVersion.Text = Core.Lang[LangId.Menu_MnuCheckForUpdate_SkipVersion];
        PART_BtnChangelog.Text = Core.Lang[LangId.QuickSetup_SeeWhatNew];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        // installed directly, not via RunApiAsync: a locked API would swallow the click silently
        if (_isReadyToInstall)
        {
            _ = AppAPIProvider.IG_RestartToUpdateAsync(false);
            return;
        }

        // this build can install it itself, so fetch the package instead of opening a browser
        if (_canDownloadAndInstall)
        {
            _ = DownloadThenInstallAsync();
            return;
        }

        // the Store delivers updates for its own package, so go to the Store listing
        if (IsMsStoreBuild)
        {
            _ = BHelper.OpenUrlAsync(this, UpdateConstants.MsStoreProductUrl, "from_update_dialog");
            return;
        }

        // "Update" button opens the update URL, falling back to the changelog URL
        var release = _result?.Release;
        var url = !string.IsNullOrWhiteSpace(release?.UpdateUrl)
            ? release.UpdateUrl
            : release?.ChangelogUrl;
        if (!string.IsNullOrWhiteSpace(url))
        {
            _ = BHelper.OpenUrlAsync(this, url, "from_update_dialog");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        _cancelDownload?.Cancel();
    }

    #endregion // Override Methods



    #region Private Methods

    /// <summary>
    /// Remembers the release as skipped, then closes the window.
    /// </summary>
    private void SkipVersion()
    {
        var version = _result?.Release?.Version;
        if (!string.IsNullOrWhiteSpace(version))
        {
            Core.Config.UpdateSkippedVersion = version;
            IsSkipped = true;

            // nothing was staged with the OS, so skipping really does retract the update
            if (string.Equals(Core.Config.UpdatePendingVersion, version, StringComparison.OrdinalIgnoreCase))
            {
                UpdateProvider.DiscardPendingUpdate();
                _ = Core.Config.SaveAsync();
            }
        }

        DialogResult = DialogExitCode.Cancel;
        Close();
    }


    /// <summary>
    /// Opens release changelog url.
    /// </summary>
    private void OpenChangeLog()
    {
        var url = _result?.Release?.ChangelogUrl;
        if (!string.IsNullOrWhiteSpace(url))
        {
            _ = BHelper.OpenUrlAsync(this, url, "from_update_dialog");
        }
    }


    /// <summary>
    /// Fills and shows the latest-release card: title, version, published date, and release notes.
    /// </summary>
    private void ShowReleaseCard(UpdateReleaseInfo release)
    {
        var versionText = Core.Lang[LangId.Menu_MnuCheckForUpdate_LatestVersion, release.Version];

        // primary title (fall back to the version label when the release has no title)
        PART_BtnTitle.Text = !string.IsNullOrWhiteSpace(release.Title) ? release.Title : versionText;
        PART_LblVersion.Text = versionText;

        PART_LblPublishedDate.Text = Core.Lang[LangId.Menu_MnuCheckForUpdate_PublishedDate, release.PublishedDate];
        PART_LblPublishedDate.IsVisible = !string.IsNullOrWhiteSpace(release.PublishedDate);

        // the separator only earns its space when there are notes below it
        var hasNotes = !string.IsNullOrWhiteSpace(release.Description);
        PART_LblNotes.Text = release.Description;
        PART_NotesSeparator.IsVisible = hasNotes;
        PART_NotesScroller.IsVisible = hasNotes;

        PART_ReleaseCard.IsVisible = true;
    }


    /// <summary>
    /// Hides the release card and both footer links.
    /// </summary>
    private void HideResultContent()
    {
        PART_ReleaseCard.IsVisible = false;
        PART_BtnSkipVersion.IsVisible = false;
    }

    /// <summary>
    /// Downloads the package with progress, then hands it to the installer.
    /// </summary>
    private async Task DownloadThenInstallAsync()
    {
        var release = _result?.Release;
        if (release is null) return;

        _cancelDownload?.Cancel();
        _cancelDownload = new CancellationTokenSource();
        var token = _cancelDownload.Token;

        // downloading state: determinate bar, only [Close] which cancels
        Heading = Core.Lang[LangId.Menu_MnuCheckForUpdate_Downloading];
        IsButton1Visible = false;
        Button2Text = Core.Lang[LangId._Cancel];
        IsProgressVisible = true;
        IsProgressIndeterminate = false;
        ProgressValue = 0;
        PART_BtnSkipVersion.IsVisible = false;

        var progress = new Progress<double>(percent => ProgressValue = percent);
        var isReady = await UpdateProvider.TryDownloadForInstallAsync(release, progress, token);

        if (token.IsCancellationRequested) return;

        if (!isReady)
        {
            // put the dialog back so the user can still reach the download page
            SetResultState(_result!);
            Note = Core.Lang[LangId.Menu_MnuCheckForUpdate_DownloadFailed];
            NoteStyle = InfoBarSeverity.Danger;
            _canDownloadAndInstall = false;
            return;
        }

        await AppAPIProvider.IG_RestartToUpdateAsync(false);
    }

    #endregion // Private Methods



    #region Public Methods

    /// <summary>
    /// Configures the window to show "Checking for update..." with an indeterminate progress bar.
    /// </summary>
    public void SetCheckingState()
    {
        _result = null;
        Heading = Core.Lang[LangId.Menu_MnuCheckForUpdate_Checking];
        Thumbnail = Resx.GetSvg(ResxSvgId.Cyclone);

        IsProgressVisible = true;
        IsProgressIndeterminate = true;

        IsButton1Visible = false;
        IsButton2Visible = true;
        IsButton3Visible = false;
        Button2Text = Core.Lang[LangId._Close];
        DefaultFocus = DialogFocus.Button2;

        HideResultContent();
    }


    /// <summary>
    /// Transitions the window to display the update check result.
    /// </summary>
    public void SetResultState(UpdateCheckResult result)
    {
        _result = result;

        IsProgressVisible = false;
        IsProgressIndeterminate = false;

        // shared defaults: a single [Close] button, no extra content
        Note = null;
        Thumbnail = null;
        _isReadyToInstall = false;
        _canDownloadAndInstall = false;
        HideResultContent();
        IsButton1Visible = false;
        IsButton3Visible = false;
        IsButton2Visible = true;
        Button2Text = Core.Lang[LangId._Close];
        DefaultButton = DialogButton.Button2;
        DefaultFocus = DialogFocus.Button2;

        var release = result.Release;

        if (result.Status == UpdateCheckStatus.UpdateAvailable && release is not null)
        {
            // a downloaded package installs from disk; otherwise the button opens the download page
            _isReadyToInstall = string.Equals(Core.Config.UpdatePendingVersion, release.Version,
                    StringComparison.OrdinalIgnoreCase)
                && UpdateProvider.CanApplyPendingUpdate
                && UpdateProvider.GetPendingPackagePath() is not null;

            _canDownloadAndInstall = !_isReadyToInstall
                && UpdateProvider.CanInstallUpdateInApp
                && UpdateProvider.ResolveArtifact(release) is not null;

            Heading = Core.Lang[_isReadyToInstall
                ? LangId.Menu_MnuCheckForUpdate_ReadyToInstall
                : LangId.Menu_MnuCheckForUpdate_NewVersion];
            Description = Core.Lang[LangId.Menu_MnuCheckForUpdate_CurrentVersion, Core.BuildInfo.Version];
            Thumbnail = Resx.GetSvg(ResxSvgId.StarStruck);
            ShowReleaseCard(release);

            // "Skip this version" link + [Update / Restart now] [Close]
            PART_BtnSkipVersion.IsVisible = true;
            Button1Text = Core.Lang[_isReadyToInstall ? LangId._RestartNow : LangId._Update];
            IsButton1Visible = true;
            DefaultButton = DialogButton.Button1;
            DefaultFocus = DialogFocus.Button1;
        }
        else if (result.Status == UpdateCheckStatus.CheckFailed)
        {
            Heading = Core.Lang[LangId.Menu_MnuCheckForUpdate_Failed];
            Description = result.ErrorMessage;
        }
        else
        {
            // NoUpdate: always show the latest release info when we have it
            Heading = Core.Lang[LangId.Menu_MnuCheckForUpdate_NoUpdate];
            Description = Core.Lang[LangId.Menu_MnuCheckForUpdate_CurrentVersion, Core.BuildInfo.Version];
            Thumbnail = Resx.GetSvg(ResxSvgId.SmilingFaceWithSmilingEyes);

            if (release is not null)
            {
                ShowReleaseCard(release);

                // offer the changelog even when already up-to-date
                PART_BtnChangelog.IsVisible = !string.IsNullOrWhiteSpace(release.ChangelogUrl);
            }
        }
    }

    #endregion // Public Methods


}
