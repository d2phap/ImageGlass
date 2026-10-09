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
using Avalonia.Layout;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI.Windowing;
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

public partial class ImageResizerWindow : ModalWindow
{
    private const double MAX_SIZE = 20000;

    private SKBitmap _srcBmp;
    private Size _srcSize;
    private Size _outputSize;

    private bool _suppressEvents;
    private CancellationTokenSource _cancel = new();

    private RadioButton _radPixels = null!;
    private RadioButton _radPercentage = null!;
    private NumericUpDown _numWidth = null!;
    private NumericUpDown _numHeight = null!;
    private TextBlock _lblSizeUnit = null!;
    private CheckBox _chkKeepRatio = null!;
    private ComboBox _cmbResample = null!;
    private TextBlock _lblSize = null!;
    private TextBlock _lblResample = null!;
    private TextBlock _lblCurrentSize = null!;
    private TextBlock _lblCurrentSizeValue = null!;
    private TextBlock _lblNewSize = null!;
    private TextBlock _lblNewSizeValue = null!;



    /// <summary>
    /// Gets the output bitmap.
    /// </summary>
    public SKBitmap? OutputBitmap { get; private set; }



    public ImageResizerWindow(SKBitmap srcBmp)
    {
        _srcBmp = srcBmp;
        _outputSize = _srcSize = new(_srcBmp.Width, _srcBmp.Height);

        IsButton1Visible = true;
        IsButton2Visible = true;
        IsButton3Visible = false;
        DefaultButton = DialogButton.Button1;

        IsProgressVisible = false;
        IsProgressIndeterminate = true;
        ProgressValue = 0;

        ShowInTaskbar = true;
        ModalExtraContent = CreateResizerContentElement();
    }



    #region Override Methods

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // set default focus to pixels radio button
        _radPixels.Focus(NavigationMethod.Tab);

        _numWidth.Value = (decimal)_srcSize.Width;
        _numHeight.Value = (decimal)_srcSize.Height;
        _lblCurrentSizeValue.Text = $"{_srcSize.Width:N0}×{_srcSize.Height:N0} px";
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = Core.Lang[LangId.Menu_MnuResizeTool];
        Button1Text = Core.Lang[LangId._OK];
        Button2Text = Core.Lang[LangId._Cancel];

        _radPixels.Content = Core.Lang[LangId.Tool_Resizer_RadResizeByPixels];
        _radPercentage.Content = Core.Lang[LangId.Tool_Resizer_RadResizeByPercentage];
        _lblSize.Text = Core.Lang[LangId.Tool_Crop_LblSize];
        _chkKeepRatio.Content = Core.Lang[LangId.Tool_Resizer_ChkKeepRatio];
        _lblResample.Text = Core.Lang[LangId.Tool_Resizer_LblResample];
        _lblCurrentSize.Text = Core.Lang[LangId.Tool_Resizer_LblCurrentSize];
        _lblNewSize.Text = Core.Lang[LangId.Tool_Resizer_LblNewSize];
    }


    protected override void OnDialogSubmitted(DialogEventArgs e)
    {
        _ = RunAsync();
    }


    protected override void OnDialogCancelled(DialogEventArgs e)
    {
        _cancel?.Cancel();
        _cancel?.Dispose();

        base.OnDialogCancelled(e);
    }


    protected override void OnDialogAborted()
    {
        _cancel?.Cancel();
        _cancel?.Dispose();

        base.OnDialogAborted();
    }

    #endregion // Override Methods




    #region Private methods

    private Grid CreateResizerContentElement()
    {
        // Row 0: Radio buttons (Pixels, Percentage)
        _radPixels = new RadioButton
        {
            GroupName = "ResizeUnit",
            IsChecked = true,
        };
        _radPercentage = new RadioButton
        {
            GroupName = "ResizeUnit",
            IsTabStop = false,
        };
        _radPixels.IsCheckedChanged += RadResizeUnit_IsCheckedChanged;

        var radioPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
        };
        KeyboardNavigation.SetTabNavigation(radioPanel, KeyboardNavigationMode.Once);
        radioPanel.KeyDown += RadioPanel_KeyDown;
        radioPanel.Children.AddRange([_radPixels, _radPercentage]);


        // Row 1: Size inputs (Width, Height) + unit label
        _lblSize = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        _numWidth = new NumericUpDown
        {
            Minimum = 1,
            Value = 1,
            Increment = 1,
            FormatString = "N0",
            MinWidth = 100,
            ShowButtonSpinner = false,
            ParsingNumberStyle = System.Globalization.NumberStyles.Integer,
        };
        _numHeight = new NumericUpDown
        {
            Minimum = 1,
            Value = 1,
            Increment = 1,
            FormatString = "N0",
            MinWidth = 100,
            ShowButtonSpinner = false,
            ParsingNumberStyle = System.Globalization.NumberStyles.Integer,
        };

        _numWidth.ValueChanged += NumWidth_ValueChanged;
        _numHeight.ValueChanged += NumHeight_ValueChanged;
        var timeLabel = new TextBlock
        {
            Text = "×",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var sizeInputPanel = new Grid
        {
            ColumnDefinitions = new("*,Auto,*"),
            ColumnSpacing = 8,
        };
        sizeInputPanel.Children.AddRange([_numWidth, timeLabel, _numHeight]);
        Grid.SetColumn(_numWidth, 0);
        Grid.SetColumn(timeLabel, 1);
        Grid.SetColumn(_numHeight, 2);

        _lblSizeUnit = new TextBlock
        {
            Text = "px",
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 50,
        };


        // Row 2: Keep ratio checkbox
        _chkKeepRatio = new CheckBox
        {
            IsChecked = true,
        };


        // Row 3: Resample combobox
        _lblResample = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };

        _cmbResample = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        foreach (var method in Enum.GetValues<ImageResamplingMethod>())
        {
            _cmbResample.Items.Add(Enum.GetName(method));
        }
        _cmbResample.SelectedIndex = 0;


        // Row 4: Current size label
        _lblCurrentSize = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 8, 0, 0),
        };
        _lblCurrentSizeValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 8, 0, 0),
        };


        // Row 5: New size label
        _lblNewSize = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        _lblNewSizeValue = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
        };


        // Build the grid layout
        var gridEl = new Grid
        {
            ColumnDefinitions = new("Auto, *, Auto"),
            RowDefinitions = new("Auto, Auto, Auto, Auto, Auto, Auto"),
            ColumnSpacing = 12,
            RowSpacing = 8,
            MinWidth = 380,
        };

        // Row 0: radio buttons
        Grid.SetRow(radioPanel, 0);
        Grid.SetColumn(radioPanel, 1);

        // Row 1: size label + inputs + unit
        Grid.SetRow(_lblSize, 1);
        Grid.SetColumn(_lblSize, 0);
        Grid.SetRow(sizeInputPanel, 1);
        Grid.SetColumn(sizeInputPanel, 1);
        Grid.SetRow(_lblSizeUnit, 1);
        Grid.SetColumn(_lblSizeUnit, 2);

        // Row 2: keep ratio checkbox
        Grid.SetRow(_chkKeepRatio, 2);
        Grid.SetColumn(_chkKeepRatio, 1);

        // Row 3: resample label + combobox
        Grid.SetRow(_lblResample, 3);
        Grid.SetColumn(_lblResample, 0);
        Grid.SetRow(_cmbResample, 3);
        Grid.SetColumn(_cmbResample, 1);

        // Row 4: current size
        Grid.SetRow(_lblCurrentSize, 4);
        Grid.SetColumn(_lblCurrentSize, 0);
        Grid.SetRow(_lblCurrentSizeValue, 4);
        Grid.SetColumn(_lblCurrentSizeValue, 1);

        // Row 5: new size
        Grid.SetRow(_lblNewSize, 5);
        Grid.SetColumn(_lblNewSize, 0);
        Grid.SetRow(_lblNewSizeValue, 5);
        Grid.SetColumn(_lblNewSizeValue, 1);

        gridEl.Children.AddRange([
            radioPanel,
            _lblSize, sizeInputPanel, _lblSizeUnit,
            _chkKeepRatio,
            _lblResample, _cmbResample,
            _lblCurrentSize, _lblCurrentSizeValue,
            _lblNewSize, _lblNewSizeValue,
        ]);

        return gridEl;
    }


    private void RadioPanel_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Left or Key.Right)) return;

        var focused = FocusManager?.GetFocusedElement();
        if (focused is not RadioButton) return;

        var target = focused == _radPixels ? _radPercentage : _radPixels;
        target.IsChecked = true;
        target.Focus(NavigationMethod.Directional);
        e.Handled = true;
    }


    private void RadResizeUnit_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_suppressEvents) return;

        var usePixels = _radPixels.IsChecked == true;
        _lblSizeUnit.Text = usePixels ? "px" : "%";
        _radPixels.IsTabStop = usePixels;
        _radPercentage.IsTabStop = !usePixels;

        if (_srcSize.Width <= 0 || _srcSize.Height <= 0) return;

        _suppressEvents = true;
        try
        {
            var w = (double)(_numWidth.Value ?? 1);
            var h = (double)(_numHeight.Value ?? 1);

            if (usePixels)
            {
                // percentage -> pixels
                _numWidth.Value = (decimal)Math.Max(1, Math.Round(w / 100.0 * _srcSize.Width));
                _numHeight.Value = (decimal)Math.Max(1, Math.Round(h / 100.0 * _srcSize.Height));
            }
            else
            {
                // pixels -> percentage
                _numWidth.Value = (decimal)Math.Max(1, Math.Round(w / _srcSize.Width * 100.0, 2));
                _numHeight.Value = (decimal)Math.Max(1, Math.Round(h / _srcSize.Height * 100.0, 2));
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        UpdateOutputSize();
    }


    private void NumWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_suppressEvents) return;

        _suppressEvents = true;
        try
        {
            var usePixels = _radPixels.IsChecked == true;
            var w = (double)(_numWidth.Value ?? 1);

            // clamp to MAX_SIZE
            var maxW = usePixels
                ? MAX_SIZE
                : (_srcSize.Width > 0 ? MAX_SIZE / _srcSize.Width * 100.0 : w);
            if (w > maxW)
            {
                w = maxW;
                _numWidth.Value = (decimal)Math.Max(1, Math.Round(w, 2));
            }

            if (_chkKeepRatio.IsChecked == true
                && _srcSize.Width > 0 && _srcSize.Height > 0)
            {
                var ratio = _srcSize.Height / _srcSize.Width;
                var newH = Math.Max(1, Math.Round(w * ratio, 2));

                // clamp the proportional height too
                var maxH = usePixels
                    ? MAX_SIZE
                    : MAX_SIZE / _srcSize.Height * 100.0;
                if (newH > maxH)
                {
                    newH = maxH;
                    // recalculate width from clamped height
                    w = Math.Max(1, Math.Round(newH / ratio, 2));
                    _numWidth.Value = (decimal)w;
                }

                _numHeight.Value = (decimal)newH;
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        UpdateOutputSize();
    }


    private void NumHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_suppressEvents) return;

        _suppressEvents = true;
        try
        {
            var usePixels = _radPixels.IsChecked == true;
            var h = (double)(_numHeight.Value ?? 1);

            // clamp to MAX_SIZE
            var maxH = usePixels
                ? MAX_SIZE
                : (_srcSize.Height > 0 ? MAX_SIZE / _srcSize.Height * 100.0 : h);
            if (h > maxH)
            {
                h = maxH;
                _numHeight.Value = (decimal)Math.Max(1, Math.Round(h, 2));
            }

            if (_chkKeepRatio.IsChecked == true
                && _srcSize.Width > 0 && _srcSize.Height > 0)
            {
                var ratio = _srcSize.Width / _srcSize.Height;
                var newW = Math.Max(1, Math.Round(h * ratio, 2));

                // clamp the proportional width too
                var maxW = usePixels
                    ? MAX_SIZE
                    : MAX_SIZE / _srcSize.Width * 100.0;
                if (newW > maxW)
                {
                    newW = maxW;
                    // recalculate height from clamped width
                    h = Math.Max(1, Math.Round(newW / ratio, 2));
                    _numHeight.Value = (decimal)h;
                }

                _numWidth.Value = (decimal)newW;
            }
        }
        finally
        {
            _suppressEvents = false;
        }

        UpdateOutputSize();
    }


    private void UpdateOutputSize()
    {
        var w = (double)(_numWidth.Value ?? 1);
        var h = (double)(_numHeight.Value ?? 1);

        if (_radPixels.IsChecked == true)
        {
            _outputSize = new(w, h);
        }
        else if (_srcSize.Width > 0 && _srcSize.Height > 0)
        {
            var pxW = (int)Math.Round(w / 100.0 * _srcSize.Width);
            var pxH = (int)Math.Round(h / 100.0 * _srcSize.Height);
            _outputSize = new(pxW, pxH);
        }

        _lblNewSizeValue.Text = $"{_outputSize.Width:N0}×{_outputSize.Height:N0} px";
    }


    private async Task RunAsync()
    {
        _btn1.IsEnabled = false;
        IsButton1Visible = false;
        IsButton2Visible = true;
        Button2Text = Core.Lang[LangId._Cancel];

        // show progress bar
        IsProgressVisible = true;
        IsProgressIndeterminate = true;
        ProgressValue = 0;

        // disable UI
        _radPixels.IsEnabled = false;
        _radPercentage.IsEnabled = false;
        _numWidth.IsEnabled = false;
        _numHeight.IsEnabled = false;
        _chkKeepRatio.IsEnabled = false;
        _cmbResample.IsEnabled = false;


        _ = Task.Factory.StartNew(async () =>
        {
            // start resizing
            var resample = (ImageResamplingMethod)_cmbResample.SelectedIndex;
            OutputBitmap = await SkiaCodec.ResizeAsync(_srcBmp,
                (int)_outputSize.Width,
                (int)_outputSize.Height, resample, _cancel.Token);

            await Task.Delay(200, _cancel.Token); // make it feel slow for better UX


            // done
            Dispatcher.UIThread.Post(() =>
            {
                IsProgressIndeterminate = false;
                ProgressValue = 100;

                Button2Text = Core.Lang[LangId._Close];
                DialogResult = DialogExitCode.OK;
                Close(DialogResult);
            });
        }, _cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }


    #endregion // Private methods



}

