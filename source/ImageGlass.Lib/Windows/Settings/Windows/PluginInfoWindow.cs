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
using ImageGlass.Common.Types;
using ImageGlass.SDK.Plugins;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

/// <summary>
/// The action a <see cref="PluginInfoWindow"/> offers for a plugin, decided from its trust state.
/// </summary>
internal enum PluginInfoWindowMode
{
    /// <summary>
    /// Read-only metadata view ([OK]).
    /// </summary>
    View,

    /// <summary>
    /// Trust-and-enable consent prompt ([Trust and Enable] / [Cancel]); turns into <see cref="Configure"/> once enabled.
    /// </summary>
    Enable,

    /// <summary>
    /// Format picker for an already-trusted plugin ([Save] / [Cancel]).
    /// </summary>
    Configure,
}


/// <summary>
/// Modal window that displays a native plugin's manifest metadata and, depending on
/// <see cref="PluginInfoWindowMode"/>, offers to enable it or edit its formats.
/// </summary>
internal sealed class PluginInfoWindow : DialogWindow
{
    private readonly PluginInfoWindowView _view;
    private readonly PluginManifest _manifest;
    private readonly string _pluginDir;
    private readonly bool _showFormats;
    private readonly Func<Task<bool>>? _enableAsync;
    private readonly PhButton _deleteButton;
    private readonly string _pluginName;
    private PluginInfoWindowMode _mode;
    private bool _isEnabling;


    // fixed dialog width so it doesn't grow/shrink with the metadata text
    protected override int MIN_WIDTH => 500;
    protected override int MAX_WIDTH => 500;
    protected override Thickness ContentPadding => new(0);


    /// <summary>
    /// Whether the user clicked the footer "Delete" link (the caller runs the delete flow).
    /// </summary>
    public bool DeleteRequested { get; private set; }


    /// <summary>
    /// Extensions the user switched off for decoding.
    /// </summary>
    public IReadOnlyCollection<string> DisabledDecodeExtensions => _view.DisabledDecodeExtensions;


    /// <summary>
    /// Extensions the user switched off for encoding.
    /// </summary>
    public IReadOnlyCollection<string> DisabledEncodeExtensions => _view.DisabledEncodeExtensions;


    /// <summary>
    /// Whether the format choices differ from what the window opened with.
    /// </summary>
    public bool ChoicesChanged => _view.ChoicesChanged;


    /// <summary>
    /// Whether the plugin was enabled from this window, which stands even if the window is then cancelled.
    /// </summary>
    public bool PluginEnabled { get; private set; }


    /// <summary>
    /// Opens the window on <paramref name="manifest"/> (folder <paramref name="pluginDir"/>), titled
    /// with the plugin's name. <paramref name="mode"/> picks the footer and whether the format picker
    /// is editable: <see cref="PluginInfoWindowMode.Enable"/> is the consent prompt
    /// (<paramref name="hashChanged"/> adds a stronger warning),
    /// <see cref="PluginInfoWindowMode.Configure"/> allows edits, and
    /// <see cref="PluginInfoWindowMode.View"/> is read-only.
    /// </summary>
    /// <param name="enableAsync">Trusts and loads the plugin for [Trust and enable]; <c>true</c> if trust changed.</param>
    public PluginInfoWindow(PluginManifest manifest, string pluginDir,
        PluginInfoWindowMode mode = PluginInfoWindowMode.View, bool hashChanged = false,
        Func<Task<bool>>? enableAsync = null)
    {
        _mode = mode;
        _manifest = manifest;
        _pluginDir = pluginDir;
        _enableAsync = enableAsync;
        _pluginName = string.IsNullOrWhiteSpace(manifest.Name) ? manifest.Id : manifest.Name;

        // file formats are a codec concept; a future non-codec kind gets the info tab only
        _showFormats = manifest.Kind == IGPluginKind.Codec;

        if (mode is PluginInfoWindowMode.Enable or PluginInfoWindowMode.Configure)
        {
            IsButton1Visible = true;
            IsButton2Visible = true;
            IsButton3Visible = false;

            // Enable is a consent prompt, so Cancel stays the accented default; once the plugin is
            // trusted this is just a format editor, where Save is the expected action.
            var cta = mode == PluginInfoWindowMode.Configure ? DialogButton.Button1 : DialogButton.Button2;
            DefaultButton = cta;
            DefaultFocus = cta == DialogButton.Button1 ? DialogFocus.Button1 : DialogFocus.Button2;
        }
        else
        {
            IsButton1Visible = true;
            IsButton2Visible = false;
            IsButton3Visible = false;
            DefaultButton = DialogButton.Button1;
            DefaultFocus = DialogFocus.Button1;
        }

        _view = new PluginInfoWindowView();
        _view.LoadData(manifest, pluginDir,
            allowEdit: mode == PluginInfoWindowMode.Configure,
            showFormats: _showFormats);
        if (mode == PluginInfoWindowMode.Enable)
        {
            // the badge glyph is an outline, so it has to be stroked instead of filled
            Button1Icon = Resx.GetIcon(ResxIconId.IconVerify);
            _btn1.IconStrokeThickness = 1.5;
            _view.ShowConsentWarning(manifest, hashChanged);
        }
        DialogContent = _view;

        // footer-left link; closes the window signalling the caller to run the delete flow
        _deleteButton = NewFooterLink(() => DeleteRequested = true);
        DialogFooterLeftContent = _deleteButton;
    }


    /// <summary>
    /// Creates a footer link that flags the requested action and closes the window.
    /// </summary>
    private PhButton NewFooterLink(Action flag)
    {
        var button = new PhButton
        {
            Variant = PhButtonVariant.Link,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        button.Click += (_, _) =>
        {
            flag();
            OnDialogCancelled(new DialogEventArgs(DialogAction.Cancel));
        };
        return button;
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // the plugin's own name titles the window; the enable prompt's heading lives in the banner,
        // and only the footer buttons differ per mode.
        Title = _pluginName;
        _deleteButton.Text = Core.Lang[LangId._Delete];

        UpdateButtonTexts();
    }


    protected override async void OnDialogSubmitted(DialogEventArgs e)
    {
        if (_mode != PluginInfoWindowMode.Enable || _enableAsync is null)
        {
            base.OnDialogSubmitted(e);
            return;
        }
        if (!e.CanProceed || _isEnabling) return;

        SetEnabling(true);
        try
        {
            PluginEnabled = await _enableAsync();
        }
        finally
        {
            SetEnabling(false);
        }

        // closed while enabling
        if (DialogResult != DialogExitCode.None) return;

        // an unloaded plugin has no formats to choose; the caller reports why it did not load
        if (!_showFormats || !Core.PluginRegistry.IsLoaded(_manifest.Id))
        {
            base.OnDialogSubmitted(e);
            return;
        }

        SwitchToConfigure();
    }


    protected override void OnDialogAborted()
    {
        // the caller reads PluginEnabled on close, so the enable has to finish first
        if (_isEnabling) return;

        base.OnDialogAborted();
    }


    /// <summary>
    /// Locks the footer while the plugin is being trusted and loaded.
    /// </summary>
    private void SetEnabling(bool isEnabling)
    {
        _isEnabling = isEnabling;
        _btn1.IsEnabled = !isEnabling;
        _btn2.IsEnabled = !isEnabling;
        _deleteButton.IsEnabled = !isEnabling;
    }


    /// <summary>
    /// Turns the enable prompt into the format picker of the now-loaded plugin.
    /// </summary>
    private void SwitchToConfigure()
    {
        _mode = PluginInfoWindowMode.Configure;

        _view.LoadData(_manifest, _pluginDir, allowEdit: true, showFormats: _showFormats);
        _view.ShowFormatsAfterEnable();

        // the accent moves from Cancel to Save, as in a window opened in Configure mode
        _btn2.IsDefault = false;
        _btn2.Variant = PhButtonVariant.Default;
        Button1Icon = null;
        DefaultButton = DialogButton.Button1;
        DefaultFocus = DialogFocus.Button1;
        SetDefaultButton(DefaultButton);
        SetDefaultFocus();

        UpdateButtonTexts();
    }


    /// <summary>
    /// Sets the footer button captions for the current mode.
    /// </summary>
    private void UpdateButtonTexts()
    {
        switch (_mode)
        {
            case PluginInfoWindowMode.Enable:
                Button1Text = Core.Lang[LangId.Settings_Plugins_TrustAndEnable];
                Button2Text = Core.Lang[LangId._Cancel];
                break;

            case PluginInfoWindowMode.Configure:
                Button1Text = Core.Lang[LangId._Save];
                Button2Text = Core.Lang[LangId._Cancel];
                break;

            default:
                Button1Text = Core.Lang[LangId._OK];
                break;
        }
    }

}
