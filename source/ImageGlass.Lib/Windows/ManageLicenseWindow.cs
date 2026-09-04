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
using Avalonia.Layout;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders.Licensing;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;

namespace ImageGlass.Common.Windows;

public partial class ManageLicenseWindow : DialogWindow
{
    protected override int MIN_WIDTH => 500;
    protected override Thickness ContentPadding => new(0);

    private readonly PhButton _btnHelp;

    // captured once so the footer text and the close action can never disagree
    private readonly bool _isLicenseExpired = Core.ExpiredLicense is not null;


    public ManageLicenseWindow()
    {
        IsButton1Visible = true;
        IsButton2Visible = false;
        IsButton3Visible = false;

        // the only footer button, and it must stay neutral, never accent
        DefaultButton = DialogButton.None;
        DefaultFocus = DialogFocus.Button1;
        ShowInTaskbar = true;

        Title = "ImageGlass Pro";
        DialogContent = new ManageLicenseView();
        DialogFooterLeftContent = _btnHelp = CreateHelpButton();
    }


    /// <summary>
    /// The footer link to the support page, for anything the license actions cannot resolve.
    /// </summary>
    private PhButton CreateHelpButton()
    {
        var btn = new PhButton
        {
            Variant = PhButtonVariant.Link,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var campaign = Core.IsProEnabled ? "from_manage_license" : "from_upgrade_dialog";
        btn.Click += (_, _) => _ = BHelper.OpenUrlAsync(this, "https://imageglass.org/support", campaign);

        return btn;
    }


    /// <summary>
    /// Leaving an expired license behind settles on Classic, whichever way the window was closed.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (!_isLicenseExpired) return;

        // silent: this runs on the close path, where a dialog has no window left to own it
        _ = LicenseService.TryUninstallExpiredLicenses(out _);
        Core.ExpiredLicense = null;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Button1Text = Core.Lang[_isLicenseExpired
            ? LangId.Menu_MnuUpgradeLicense_SwitchToClassic
            : LangId._Close];
        _btnHelp.Text = Core.Lang[LangId._GetHelp];
    }

}
