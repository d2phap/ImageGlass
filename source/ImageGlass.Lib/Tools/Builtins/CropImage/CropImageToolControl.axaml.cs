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
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

public partial class CropImageToolControl : PhControl, IToolControl
{
    // prevents dead-loop when updating NumericUpDown values from SelectionChanged
    private bool _isUpdatingSelectionUI;
    private Rect _lastSelectionArea;


    public static string TOOL_ID => "Tool_CropImage";
    public string ToolId => TOOL_ID;
    public string ToolName => Core.Lang[LangId.Menu_MnuCropTool];
    public ResxIconId? ToolIcon => ResxIconId.IconToolCropImage;
    public bool HasSettingsUI => true;
    public object? Settings { get; private set; } = new CropImageConfig();
    public CropImageConfig Options => (CropImageConfig)Settings!;
    public ViewerControl Viewer { get; set; } = null!;


    public CropImageToolControl()
    {
        InitializeComponent();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // enable selection on the viewer
        Viewer.EnableSelection = true;

        // subscribe to viewer selection changes
        Viewer.SelectionChanged += Viewer_SelectionChanged;

        // subscribe to button clicks
        PART_BtnReset.Click += PART_BtnReset_Click;
        PART_BtnSave.Click += PART_BtnSave_Click;
        PART_BtnSaveAs.Click += PART_BtnSaveAs_Click;
        PART_BtnCrop.Click += PART_BtnCrop_Click;
        PART_BtnCopy.Click += PART_BtnCopy_Click;

        // subscribe to aspect ratio change
        PART_CmdAspectRatio.SelectionChanged += PART_CmdAspectRatio_SelectionChanged;

        // subscribe to NumericUpDown value changes
        PART_NumX.ValueChanged += NumSelection_ValueChanged;
        PART_NumY.ValueChanged += NumSelection_ValueChanged;
        PART_NumWidth.ValueChanged += NumSelection_ValueChanged;
        PART_NumHeight.ValueChanged += NumSelection_ValueChanged;

        // subscribe to custom ratio value changes
        PART_NumRatioFrom.ValueChanged += NumRatio_ValueChanged;
        PART_NumRatioTo.ValueChanged += NumRatio_ValueChanged;

        // subscribe to photo loading to restore default selection on new photo
        Viewer.PhotoLoading += Viewer_PhotoLoading;

        // load initial values from settings
        _isUpdatingSelectionUI = true;
        PART_NumRatioFrom.Value = Options.AspectRatioValues[0];
        PART_NumRatioTo.Value = Options.AspectRatioValues[1];
        PART_CmdAspectRatio.SelectedIndex = (int)Options.AspectRatio;
        _isUpdatingSelectionUI = false;

        // apply aspect ratio and load default selection
        UpdateAspectRatioValues();
        UpdateCustomRatioVisibility();
        LoadDefaultSelection();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        // unsubscribe events
        Viewer.SelectionChanged -= Viewer_SelectionChanged;

        PART_BtnReset.Click -= PART_BtnReset_Click;
        PART_BtnSave.Click -= PART_BtnSave_Click;
        PART_BtnSaveAs.Click -= PART_BtnSaveAs_Click;
        PART_BtnCrop.Click -= PART_BtnCrop_Click;
        PART_BtnCopy.Click -= PART_BtnCopy_Click;

        PART_CmdAspectRatio.SelectionChanged -= PART_CmdAspectRatio_SelectionChanged;

        PART_NumX.ValueChanged -= NumSelection_ValueChanged;
        PART_NumY.ValueChanged -= NumSelection_ValueChanged;
        PART_NumWidth.ValueChanged -= NumSelection_ValueChanged;
        PART_NumHeight.ValueChanged -= NumSelection_ValueChanged;

        PART_NumRatioFrom.ValueChanged -= NumRatio_ValueChanged;
        PART_NumRatioTo.ValueChanged -= NumRatio_ValueChanged;

        Viewer.PhotoLoading -= Viewer_PhotoLoading;

        // reset selection and disable selection mode
        Viewer.SourceSelection = default;
        Viewer.EnableSelection = false;

        base.OnUnloaded(e);
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        PART_BtnReset.Text = Core.Lang[LangId.Tool_Crop_BtnReset];
        PART_BtnSave.Text = Core.Lang[LangId.Tool_Crop_BtnSave];
        PART_BtnSaveAs.Text = Core.Lang[LangId.Tool_Crop_BtnSaveAs];
        PART_BtnCrop.Text = Core.Lang[LangId.Tool_Crop_BtnCrop];
        PART_BtnCopy.Text = Core.Lang[LangId.Tool_Crop_BtnCopy];

        PART_CmdAspectRatio.Items[0] = Core.Lang[LangId.Tool_Crop_SelectionAspectRatio_FreeRatio];
        PART_CmdAspectRatio.Items[1] = Core.Lang[LangId.Tool_Crop_SelectionAspectRatio_Custom];
        PART_CmdAspectRatio.Items[2] = Core.Lang[LangId.Tool_Crop_SelectionAspectRatio_Original];
    }


    private void Viewer_SelectionChanged(ViewerControl sender, ViewerSelectionChangedEventArgs e)
    {
        // track the latest selection for UseTheLastSelection mode.
        _lastSelectionArea = e.SourceSelection;

        UpdateSelectionUI(e.SourceSelection);
    }


    private void PART_CmdAspectRatio_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingSelectionUI) return;

        UpdateAspectRatioValues();
        LoadDefaultSelection();
    }


    private void NumRatio_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdatingSelectionUI) return;

        // update settings and viewer with new custom ratio values
        var ratioW = (int)(PART_NumRatioFrom.Value ?? 1);
        var ratioH = (int)(PART_NumRatioTo.Value ?? 1);

        Options.AspectRatioValues = [ratioW, ratioH];
        Viewer.SelectionAspectRatio = new Size(ratioW, ratioH);

        LoadDefaultSelection();
    }


    private void Viewer_PhotoLoading(ViewerControl sender, PhotoLoadingEventArgs e)
    {
        if (e.State != PhotoState.Loaded) return;

        // restore default selection when the new photo is fully loaded
        Dispatcher.UIThread.Post(() =>
        {
            UpdateAspectRatioValues();
            LoadDefaultSelection();
        });
    }


    private void NumSelection_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        // guard against dead-loop: viewer SelectionChanged -> update UI -> value changed -> update viewer
        if (_isUpdatingSelectionUI) return;

        LoadSelectionFromInputs();
    }


    private void NumericUpDown_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LoadSelectionFromInputs();
        }
    }


    private void PART_BtnReset_Click(object? sender, RoutedEventArgs e)
    {
        Viewer.SourceSelection = default;
        Viewer.Refresh(false);
    }


    private async void PART_BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        await Core.API!.RunApiAsync(API.IG_Save);

        if (Options.CloseAfterSaved)
        {
            await Core.API!.RunApiAsync(API.IG_CloseCurrentTool);
        }
    }


    private async void PART_BtnSaveAs_Click(object? sender, RoutedEventArgs e)
    {
        await Core.API!.RunApiAsync(API.IG_SaveAs);

        if (Options.CloseAfterSaved)
        {
            await Core.API!.RunApiAsync(API.IG_CloseCurrentTool);
        }
    }


    private async void PART_BtnCrop_Click(object? sender, RoutedEventArgs e)
    {
        var bitmap = Viewer.GetRenderedBitmap(true);
        var photo = new Photo(bitmap);

        await AppAPIProvider.LoadClipboardPhotoAsync(photo);
    }


    private async void PART_BtnCopy_Click(object? sender, RoutedEventArgs e)
    {
        await Core.API!.RunApiAsync(API.IG_CopyImagePixels);
    }

    #endregion // Control Events



    #region Control Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void LoadSettings(JsonElement? jsonEl)
    {
        var settings = jsonEl?.Deserialize(CropImageConfigJsonContext.Default.CropImageConfig);
        if (settings is not null)
        {
            Settings = settings;
        }

        _lastSelectionArea = Options.InitSelectedArea;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public JsonElement? SaveSettings()
    {
        if (Options.InitSelectionType == DefaultSelectionType.UseTheLastSelection)
        {
            Options.InitSelectedArea = Viewer.SourceSelection;
        }

        var jsonEl = JsonSerializer.SerializeToElement(Options, CropImageConfigJsonContext.Default.CropImageConfig);

        return jsonEl;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task ShowSettingsWindowAsync()
    {
        var window = new CropImageSettingsWindow(Options);
        var owner = TopLevel.GetTopLevel(this) as PhWindow;
        var result = await window.ShowAsync(owner);

        if (result == DialogExitCode.OK)
        {
            Settings = window.ResultConfig;

            // re-apply aspect ratio and reload default selection
            _isUpdatingSelectionUI = true;
            PART_CmdAspectRatio.SelectedIndex = (int)Options.AspectRatio;
            _isUpdatingSelectionUI = false;

            UpdateAspectRatioValues();
            LoadDefaultSelection();
        }
    }


    /// <summary>
    /// Updates the viewer's <see cref="ViewerControl.SelectionAspectRatio"/>
    /// based on the current aspect ratio selection.
    /// </summary>
    private void UpdateAspectRatioValues()
    {
        var ratio = (SelectionAspectRatio)PART_CmdAspectRatio.SelectedIndex;
        var ratioW = Options.AspectRatioValues[0];
        var ratioH = Options.AspectRatioValues[1];

        if (ratio == SelectionAspectRatio.Original)
        {
            var srcW = (int)Viewer.BitmapSize.Width;
            var srcH = (int)Viewer.BitmapSize.Height;

            if (srcW > 0 && srcH > 0)
            {
                var results = BHelper.SimplifyFractions(srcW, srcH);
                ratioW = results[0];
                ratioH = results[1];
            }
        }
        else if (ratio == SelectionAspectRatio.Custom)
        {
            ratioW = (int)(PART_NumRatioFrom.Value ?? 1);
            ratioH = (int)(PART_NumRatioTo.Value ?? 1);

            // default to the image's simplified ratio if no custom values set
            if (ratioW <= 0 || ratioH <= 0)
            {
                var srcW = (int)Viewer.BitmapSize.Width;
                var srcH = (int)Viewer.BitmapSize.Height;

                if (srcW > 0 && srcH > 0)
                {
                    var results = BHelper.SimplifyFractions(srcW, srcH);
                    ratioW = results[0];
                    ratioH = results[1];
                }
                else
                {
                    ratioW = 1;
                    ratioH = 1;
                }
            }
        }
        else if (CropImageConfig.AspectRatioValue.TryGetValue(ratio, out var value))
        {
            ratioW = value[0];
            ratioH = value[1];
        }

        // update the custom ratio UI
        _isUpdatingSelectionUI = true;
        PART_NumRatioFrom.Value = ratioW;
        PART_NumRatioTo.Value = ratioH;
        _isUpdatingSelectionUI = false;

        // save to settings
        Options.AspectRatio = ratio;
        Options.AspectRatioValues = [ratioW, ratioH];

        // update visibility of custom ratio controls
        UpdateCustomRatioVisibility();

        // apply to viewer
        if (ratio == SelectionAspectRatio.FreeRatio)
        {
            Viewer.SelectionAspectRatio = new Size();
        }
        else
        {
            Viewer.SelectionAspectRatio = new Size(ratioW, ratioH);
        }
    }


    /// <summary>
    /// Shows or hides the custom ratio NumericUpDown controls.
    /// They are visible for Custom and Original ratios, but only editable for Custom.
    /// </summary>
    private void UpdateCustomRatioVisibility()
    {
        var ratio = (SelectionAspectRatio)PART_CmdAspectRatio.SelectedIndex;

        var showCustomRatio = ratio is SelectionAspectRatio.Original
            or SelectionAspectRatio.Custom;
        PART_NumRatioFrom.IsVisible = showCustomRatio;
        PART_NumRatioTo.IsVisible = showCustomRatio;

        PART_NumRatioFrom.IsEnabled = ratio == SelectionAspectRatio.Custom;
        PART_NumRatioTo.IsEnabled = ratio == SelectionAspectRatio.Custom;
    }


    /// <summary>
    /// Loads the default selection area based on settings.
    /// </summary>
    private void LoadDefaultSelection()
    {
        var srcW = (int)Viewer.BitmapSize.Width;
        var srcH = (int)Viewer.BitmapSize.Height;

        if (srcW <= 0 || srcH <= 0) return;

        var useLastSelection = Options.InitSelectionType == DefaultSelectionType.UseTheLastSelection;

        var x = 0;
        var y = 0;
        var w = 0;
        var h = 0;

        if (useLastSelection)
        {
            x = (int)_lastSelectionArea.X;
            y = (int)_lastSelectionArea.Y;
            w = (int)_lastSelectionArea.Width;
            h = (int)_lastSelectionArea.Height;
        }
        else if (Options.InitSelectionType == DefaultSelectionType.CustomArea)
        {
            x = (int)Options.InitSelectedArea.X;
            y = (int)Options.InitSelectedArea.Y;
            w = (int)Options.InitSelectedArea.Width;
            h = (int)Options.InitSelectedArea.Height;
        }
        else
        {
            var selectPercent = Options.InitSelectionType switch
            {
                DefaultSelectionType.SelectNone => 0f,
                DefaultSelectionType.Select10Percent => 0.1f,
                DefaultSelectionType.Select20Percent => 0.2f,
                DefaultSelectionType.Select25Percent => 0.25f,
                DefaultSelectionType.Select30Percent => 0.3f,
                DefaultSelectionType.SelectOneThird => 1 / 3f,
                DefaultSelectionType.Select40Percent => 0.4f,
                DefaultSelectionType.Select50Percent => 0.5f,
                DefaultSelectionType.Select60Percent => 0.6f,
                DefaultSelectionType.SelectTwoThirds => 2 / 3f,
                DefaultSelectionType.Select70Percent => 0.7f,
                DefaultSelectionType.Select75Percent => 0.75f,
                DefaultSelectionType.Select80Percent => 0.8f,
                DefaultSelectionType.Select90Percent => 0.9f,
                DefaultSelectionType.SelectAll => 1f,
                _ => 0.5f,
            };

            w = (int)(srcW * selectPercent);
            h = (int)(srcH * selectPercent);
        }

        // update selection size according to the aspect ratio
        if (Options.AspectRatio != SelectionAspectRatio.FreeRatio
            && Options.AspectRatioValues[0] > 0
            && Options.AspectRatioValues[1] > 0)
        {
            var ratioSize = GetSizeWithAspectRatio(w, h);
            w = (int)ratioSize.Width;
            h = (int)ratioSize.Height;
        }

        // auto-center the selection (skip for UseTheLastSelection)
        if (Options.AutoCenterSelection && !useLastSelection)
        {
            x = srcW / 2 - w / 2;
            y = srcH / 2 - h / 2;
        }

        // validate bounds
        x = Math.Max(0, x);
        y = Math.Max(0, y);
        w = Math.Max(0, w);
        h = Math.Max(0, h);

        Viewer.SourceSelection = new Rect(x, y, w, h);
        Viewer.Refresh(false);
    }


    /// <summary>
    /// Computes a size that fits within the image bounds while preserving the current aspect ratio.
    /// </summary>
    private Size GetSizeWithAspectRatio(int width, int height)
    {
        var ratioW = Options.AspectRatioValues[0];
        var ratioH = Options.AspectRatioValues[1];
        if (ratioW <= 0 || ratioH <= 0) return new Size(width, height);

        var wRatio = 1.0 * ratioW / ratioH;
        var hRatio = 1.0 * ratioH / ratioW;

        var srcW = (int)Viewer.BitmapSize.Width;
        var srcH = (int)Viewer.BitmapSize.Height;

        var w = (double)width;
        var h = (double)height;

        // scale to the aspect ratio
        if (w > h)
        {
            w = h * wRatio;
        }
        else if (w < h)
        {
            h = w * hRatio;
        }

        // if new size is larger than source size
        if (w >= srcW || h >= srcH)
        {
            var srcWRatio = 1.0 * srcW / srcH;
            var srcHRatio = 1.0 * srcH / srcW;

            if (srcWRatio >= wRatio)
            {
                w = wRatio * srcH;
            }
            else if (srcHRatio >= hRatio)
            {
                h = hRatio * srcW;
            }
        }

        return new Size(w, h);
    }


    /// <summary>
    /// Updates the selection UI fields from the given selection rect.
    /// </summary>
    private void UpdateSelectionUI(Rect selection)
    {
        _isUpdatingSelectionUI = true;
        try
        {
            PART_NumX.Value = (decimal)selection.X;
            PART_NumY.Value = (decimal)selection.Y;
            PART_NumWidth.Value = (decimal)selection.Width;
            PART_NumHeight.Value = (decimal)selection.Height;

            UpdateActionButtonStates(selection.Width > 0 && selection.Height > 0);
        }
        finally
        {
            _isUpdatingSelectionUI = false;
        }
    }


    /// <summary>
    /// Enables or disables action buttons based on whether a selection exists.
    /// </summary>
    private void UpdateActionButtonStates(bool hasSelection)
    {
        PART_BtnSave.IsEnabled = hasSelection;
        PART_BtnSaveAs.IsEnabled = hasSelection;
        PART_BtnCrop.IsEnabled = hasSelection;
        PART_BtnCopy.IsEnabled = hasSelection;
    }


    /// <summary>
    /// Reads the current values from NumericUpDown controls and applies
    /// them to the viewer's <see cref="ViewerControl.SourceSelection"/>.
    /// </summary>
    private void LoadSelectionFromInputs()
    {
        var x = (double)(PART_NumX.Value ?? 0);
        var y = (double)(PART_NumY.Value ?? 0);
        var w = (double)(PART_NumWidth.Value ?? 0);
        var h = (double)(PART_NumHeight.Value ?? 0);

        // don't update selection with invalid values
        if (x < 0 || y < 0 || w < 0 || h < 0) return;

        var newRect = new Rect(x, y, w, h);

        if (newRect != Viewer.SourceSelection)
        {
            Viewer.SourceSelection = newRect;
            Viewer.Refresh(false);
        }
    }

    #endregion // Control Methods


}