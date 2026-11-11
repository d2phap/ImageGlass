/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Layout;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System.Threading.Tasks;

namespace ImageGlass.Windows;

public partial class QuickSetupWindow : DialogWindow
{
    protected override int MIN_WIDTH => 500;
    protected override int MAX_WIDTH => 500;

    private readonly QuickSetupView _view;
    private PhButton _btnSkip = null!;


    /// <summary>
    /// Whether the wizard should be forced on startup: the saved <see cref="Config.QuickSetupVersion"/>
    /// is behind the current <see cref="Const.QUICK_SETUP_VERSION"/> (new install, missing key, or a
    /// major-change bump). Set the config value >= that constant to bypass.
    /// </summary>
    public static bool ShouldShowAtStartup => Core.Config.QuickSetupVersion < Const.QUICK_SETUP_VERSION;


    /// <summary>
    /// Whether Save was confirmed and the app is restarting. The startup gate checks this to avoid
    /// showing the main window (which would flash before the process exits to relaunch).
    /// </summary>
    public bool IsRestarting { get; private set; }


    public QuickSetupWindow()
    {
        // may be shown before the main window on startup, so make it discoverable and centered
        ShowInTaskbar = true;
        WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen;

        IsButton1Visible = false; // "Back" (hidden on the first step)
        IsButton2Visible = true;  // "Next" / "Save"
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button2;
        DefaultFocus = DialogFocus.Button2;

        _view = new QuickSetupView();
        _view.PreviewLanguageChanged += (_, _) => RefreshFooter();
        DialogContent = _view;
        DialogFooterLeftContent = _btnSkip = BuildSkipButton();

        RefreshFooter();
    }



    #region Overrides

    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        RefreshFooter();
    }


    /// <summary>
    /// "Back" (footer button 1): go to the previous step.
    /// </summary>
    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        if (_view.CurrentStep <= 1) return;

        _view.ShowStep(_view.CurrentStep - 1);
        RefreshFooter();
    }


    /// <summary>
    /// "Next" / "Save" (footer button 2): advance, or submit on the last step.
    /// </summary>
    protected override void OnDialogCancelled(DialogEventArgs e)
    {
        if (_view.CurrentStep < _view.StepCount)
        {
            _view.ShowStep(_view.CurrentStep + 1);
            RefreshFooter();
            return;
        }

        _ = SaveAndCloseAsync();
    }

    #endregion // Overrides



    #region Methods

    /// <summary>
    /// Syncs footer button visibility and text (localized from the wizard's preview language).
    /// </summary>
    private void RefreshFooter()
    {
        var lang = _view.PreviewLang;
        var isLastStep = _view.CurrentStep >= _view.StepCount;

        IsButton1Visible = _view.CurrentStep > 1;

        Title = lang[LangId.QuickSetup_Title];
        Button1Text = lang[LangId._Back];
        Button2Text = lang[isLastStep ? LangId._Save : LangId._Next];
        if (_btnSkip is not null) _btnSkip.Text = lang[LangId.QuickSetup_SkipAndLaunch];
    }


    /// <summary>
    /// Builds the footer-left "Skip this and launch ImageGlass" link button.
    /// </summary>
    private PhButton BuildSkipButton()
    {
        var btn = new PhButton
        {
            Variant = PhButtonVariant.Link,
            VerticalAlignment = VerticalAlignment.Center,
        };
        btn.Click += (_, _) => SkipSetup();

        return btn;
    }


    /// <summary>
    /// Closes the wizard without saving. At startup the app then shows the main window; the
    /// startup gate persists the version so the wizard isn't shown again.
    /// </summary>
    private void SkipSetup()
    {
        DialogResult = DialogExitCode.Cancel;
        Close(DialogResult);
    }


    /// <summary>
    /// Resets all settings to built-in defaults, applies the wizard choices on top, saves, then
    /// restarts the app so every reset setting takes effect. If other app instances are running,
    /// first asks the user to confirm closing them (No = close without saving anything).
    /// </summary>
    private async Task SaveAndCloseAsync()
    {
        // other instances would overwrite the reset config on their own exit, so confirm + close them
        if (BHelper.HasOtherInstances())
        {
            var modal = await ModalWindow.ShowWarningAsync(this, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.QuickSetup_Title],
                Heading = Core.Lang[LangId.QuickSetup_ConfirmCloseProcess],
                Description = Core.Lang[LangId.QuickSetup_ConfirmCloseProcess_Description],
            }, ModalWindowButton.Yes_No);

            // "No": close without saving anything
            if (modal.ExitCode != DialogExitCode.OK)
            {
                DialogResult = DialogExitCode.Cancel;
                Close(DialogResult);
                return;
            }

            BHelper.CloseOtherInstances();
        }

        // reset everything to built-in defaults, then apply the wizard choices on top
        Core.Config.ResetToDefault();
        ApplySettings();

        // the restart below reloads the config from disk, so an unwritten file discards every choice
        if (!await Core.Config.SaveAsync())
        {
            await ModalWindow.ShowSettingsSaveErrorAsync(this, Core.Lang[LangId.QuickSetup_Title]);
            return;
        }

        // restart so the fresh instance loads the reset config (all settings applied);
        // suppress the wizard on relaunch so an admin-locked version can't loop
        IsRestarting = true;
        BHelper.RestartApp(suppressQuickSetup: true);
    }


    /// <summary>
    /// Applies the wizard selections to <see cref="Core.Config"/> (on top of the reset defaults).
    /// </summary>
    private void ApplySettings()
    {
        // mark this Quick Setup version as done so it isn't forced again on startup
        Core.Config.QuickSetupVersion = Const.QUICK_SETUP_VERSION;

        Core.Config.Language = _view.SelectedLanguageValue;

        Core.Config.ColorProfile = _view.IsProfessional
            ? nameof(ColorProfileOption.CurrentMonitorProfile)
            : nameof(ColorProfileOption.None);
        Core.Config.EnableHdrToneMapping = _view.IsProfessional;
        Core.Config.EnableExplorerSortOrder = _view.IsProfessional;
        Core.Config.EnableOnlyLoadRawPreview = !_view.IsProfessional;
    }

    #endregion // Methods

}
