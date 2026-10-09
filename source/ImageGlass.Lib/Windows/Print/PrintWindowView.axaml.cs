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
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Printing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;


/// <summary>
/// The content of the Print window: the settings, the live preview, and the job it starts.
/// </summary>
public partial class PrintWindowView : PhControl
{
    private const int RENDER_DEBOUNCE_MS = 80;
    private const int PRINTER_TIMEOUT_MS = 10_000;
    private const int MANY_PAGES = 100;

    private readonly PrintSession _session = null!;
    private readonly PrintConfig _config = new();
    private readonly CancellationTokenSource _closing = new();
    private readonly List<LayoutOption> _layoutOptions = [];

    private PrinterInfo? _printer;
    private PrinterCapabilities _caps = PrintProviderBase.PdfCapabilities;
    private PrintLayout _layout = PrintLayouts.FullPage;
    private PrintDocumentLayout? _doc;
    private byte[]? _platformState;
    private string? _noPrintersText;
    private CancellationTokenSource? _capsCancel;
    private CancellationTokenSource? _printCancel;
    private int _pageIndex;
    private int _renderVersion;
    private bool _isUpdatingControls;
    private bool _isMoreVisible;
    private bool _isLoadingPrinters;
    private bool _refreshPrintersOnActivate;
    private WindowBase? _window;


    /// <summary>
    /// Occurs when what can be printed changes, or a job starts or ends.
    /// </summary>
    public event Action? StateChanged;


    /// <summary>
    /// Gets whether a job is running.
    /// </summary>
    public bool IsPrinting => _printCancel is not null;


    /// <summary>
    /// Gets whether there is something to print and somewhere to print it.
    /// </summary>
    public bool CanPrint => _doc is { Pages.Count: > 0 } && _printer is not null && !IsPrinting;


    /// <summary>
    /// Gets the destination chosen.
    /// </summary>
    public PrinterInfo? Printer => _printer;


    /// <summary>
    /// Gets the number of pages and prints, as the footer shows them.
    /// </summary>
    public string SummaryText => _doc is null
        ? string.Empty
        : Core.Lang[LangId.Print_Summary, _doc.Pages.Count, _doc.PrintCount];


    public PrintWindowView()
    {
        InitializeComponent();
    }


    public PrintWindowView(PrintSession session) : this()
    {
        _session = session;
        _config = PrintConfig.Load();
        _layout = PrintLayouts.Find(_config.LayoutId);

        InitializeControls();
        WireEvents();
        PART_PreviewPane.Background = new SolidColorBrush(PrintPreviewControl.GetDeskColor());
        PART_BtnAddPrinter.IsVisible = Core.PrintProvider.CanAddPrinter;

        _session.ImageReady += Session_ImageReady;
    }


    #region Lifecycle

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _window = TopLevel.GetTopLevel(this) as WindowBase;
        if (_window is not null) _window.Activated += Window_Activated;

        _ = LoadPrintersAsync();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (_window is not null) _window.Activated -= Window_Activated;
        _window = null;
    }


    private void Window_Activated(object? sender, EventArgs e)
    {
        // back from the system's printer settings: a printer added there is listed and chosen
        if (_refreshPrintersOnActivate && !IsPrinting) _ = LoadPrintersAsync(true);
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        PART_PreviewPane.Background = new SolidColorBrush(PrintPreviewControl.GetDeskColor());
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        if (_session is null) return;

        _isUpdatingControls = true;
        FillEnumItems();
        FillQualityItems();
        _isUpdatingControls = false;

        PART_BtnAddPrinter.Text = Core.Lang[LangId.Print_BtnAddPrinter];
        PART_BtnMore.Text = Core.Lang[LangId.Print_LblMoreSettings];
        PART_BtnProperties.Text = Core.Lang[LangId.Print_BtnProperties];
        ToolTip.SetTip(PART_BtnPrevPage, Core.Lang[LangId.Print_PreviousPage]);
        ToolTip.SetTip(PART_BtnNextPage, Core.Lang[LangId.Print_NextPage]);
        PART_PageRangeHint.Text = Core.Lang[LangId.Print_RangeInvalid];

        Relayout();
    }


    /// <summary>
    /// Stops the preview and any job; the session stays its owner's to dispose.
    /// </summary>
    public void Close()
    {
        _session.ImageReady -= Session_ImageReady;
        _printCancel?.Cancel();
        _capsCancel?.Cancel();
        _closing.Cancel();
        PART_Preview.SetPage(null, default);
    }

    #endregion // Lifecycle


    #region Controls

    /// <summary>
    /// Fills the choices and sets them from the saved settings.
    /// </summary>
    private void InitializeControls()
    {
        _isUpdatingControls = true;

        FillEnumItems();
        BuildLayoutOptions();

        SelectTag(PART_Orientation, _config.Orientation);
        SelectTag(PART_Fit, _config.Fit);
        SelectTag(PART_Margins, _config.Margins);
        SelectTag(PART_Color, _config.ColorMode);
        SelectTag(PART_Duplex, _config.Duplex);
        PART_AutoRotate.IsChecked = _config.AutoRotate;
        PART_Captions.IsChecked = _config.ShowCaptions;
        PART_FillPage.IsChecked = _config.FillPage;
        PART_Collate.IsChecked = _config.Collate;

        // a document prints all its pages, an animation the frame on screen
        var state = _session.State;
        PART_PagesRow.IsVisible = _session.FrameCount > 1;
        SelectTag(PART_PageScope, _session.FrameCount > 1 && !state.IsAnimated ? PrintPageScope.All : PrintPageScope.Current);

        // the part to print: offered with a selection, or a view zoomed into the photo
        var whole = state.GetRegionRect(ViewerImageRegion.WholeImage);
        var visible = state.GetRegionRect(ViewerImageRegion.VisibleArea);
        var hasSelection = !state.GetRegionRect(ViewerImageRegion.Selection).IsEmpty;
        var hasVisibleArea = !visible.IsEmpty && (visible.Width < whole.Width || visible.Height < whole.Height);
        PART_RegionRow.IsVisible = hasSelection || hasVisibleArea;
        FillRegionItems(hasSelection, hasVisibleArea);

        // Save as PDF until the printers arrive, so the preview shows at once
        _printer = PrintProviderBase.SaveAsPdfPrinter;
        PART_Printer.Items.Add(new ComboBoxItem { Content = Core.Lang[LangId.Print_LoadingPrinters], IsEnabled = false });
        PART_Printer.SelectedIndex = 0;
        PART_Printer.IsEnabled = false;
        FillPaperItems();
        FillQualityItems();
        UpdateCapabilityRows();

        _isUpdatingControls = false;

        ApplyItems();
        Relayout();
    }


    private void WireEvents()
    {
        PART_Printer.SelectionChanged += async (_, _) =>
        {
            if (_isUpdatingControls) return;
            if (PART_Printer.SelectedItem is ComboBoxItem { Tag: PrinterInfo printer }) await SelectPrinterAsync(printer);
        };

        foreach (var box in new[] { PART_Paper, PART_Orientation, PART_Fit, PART_Margins, PART_Color, PART_Duplex, PART_Quality })
        {
            box.SelectionChanged += (_, _) => OnSettingChanged();
        }

        // a paper chosen in code is measured too, so this runs while the controls update
        PART_Paper.SelectionChanged += (_, _) => _ = MeasureSelectedPaperAsync();

        foreach (var check in new[] { PART_AutoRotate, PART_Captions, PART_FillPage, PART_Collate })
        {
            check.IsCheckedChanged += (_, _) => OnSettingChanged();
        }

        PART_Copies.ValueChanged += (_, _) => OnSettingChanged();
        PART_PrintsEach.ValueChanged += (_, _) => OnSettingChanged();

        PART_Region.SelectionChanged += (_, _) => OnItemsChanged();
        PART_PageScope.SelectionChanged += (_, _) => OnItemsChanged();
        PART_PageRange.TextChanged += (_, _) => OnItemsChanged();

        PART_BtnAddPrinter.Click += async (_, _) => await OpenAddPrinterSettingsAsync();
        PART_BtnMore.Click += (_, _) => ToggleMore();
        PART_BtnProperties.Click += async (_, _) => await ShowPropertiesAsync();
        PART_BtnPrevPage.Click += (_, _) => GoToPage(-1);
        PART_BtnNextPage.Click += (_, _) => GoToPage(1);
        PART_Preview.SizeChanged += (_, _) => RequestRender();
    }


    /// <summary>
    /// Fills the choices of every enum, keeping what is selected across a language change.
    /// </summary>
    private void FillEnumItems()
    {
        FillItems(PART_Orientation, Enum.GetValues<PrintOrientation>());
        FillItems(PART_Fit, Enum.GetValues<PrintFitMode>());
        FillItems(PART_Margins, Enum.GetValues<PrintMarginPreset>());
        FillItems(PART_Color, Enum.GetValues<PrintColorMode>());
        FillItems(PART_Duplex, Enum.GetValues<PrintDuplex>());
        FillItems(PART_PageScope, Enum.GetValues<PrintPageScope>(), _session.FrameCount);

        if (PART_Region.Items.Count > 0)
        {
            var selected = GetTag(PART_Region, PrintRegion.WholeImage);
            var values = PART_Region.Items.OfType<ComboBoxItem>().Select(i => (PrintRegion)i.Tag!).ToArray();
            FillItems(PART_Region, values);
            SelectTag(PART_Region, selected);
        }
    }


    private void FillRegionItems(bool hasSelection, bool hasVisibleArea)
    {
        var values = new List<PrintRegion> { PrintRegion.WholeImage };
        if (hasSelection) values.Add(PrintRegion.Selection);
        if (hasVisibleArea) values.Add(PrintRegion.VisibleArea);

        FillItems(PART_Region, values);
        SelectTag(PART_Region, PrintRegion.WholeImage);
    }


    /// <summary>
    /// Fills a combo box with enum values named by their language keys, keeping the selection.
    /// </summary>
    private static void FillItems<T>(ComboBox box, IEnumerable<T> values, params object?[] args) where T : struct, Enum
    {
        var selected = box.SelectedItem is ComboBoxItem { Tag: T current } ? current : (T?)null;
        box.Items.Clear();

        foreach (var value in values)
        {
            var text = Core.Lang[Lang.GetKey($"{typeof(T).Name}_{value}"), args];
            box.Items.Add(new ComboBoxItem { Content = text, Tag = value });
        }

        if (selected is { } s) SelectTag(box, s);
        else if (box.Items.Count > 0) box.SelectedIndex = 0;
    }


    private static T GetTag<T>(ComboBox box, T fallback)
    {
        return box.SelectedItem is ComboBoxItem { Tag: T value } ? value : fallback;
    }


    private static void SelectTag<T>(ComboBox box, T value) where T : notnull
    {
        foreach (var item in box.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is T tag && tag.Equals(value))
            {
                box.SelectedItem = item;
                return;
            }
        }
    }


    /// <summary>
    /// Builds a drawn tile for every layout of the user's region, plus a saved one from another region.
    /// </summary>
    private void BuildLayoutOptions()
    {
        var layouts = PrintLayouts.GetAll(PrintUnits.IsMetricRegion).ToList();
        if (!layouts.Any(i => i.Id == _layout.Id)) layouts.Add(_layout);

        foreach (var layout in layouts)
        {
            var tile = new PrintLayoutTile(layout);
            // the button does not bound its content's width, so the label carries its own
            var label = new TextBlock
            {
                MaxWidth = 88,
                FontSize = Const.FONT_SIZE_SMALL,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var button = new PhToolButton
            {
                Content = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center, Children = { tile, label } },
            };
            button.Classes.Add("layout");
            button.Click += (_, _) => SelectLayout(layout);

            PART_Layouts.Children.Add(button);
            _layoutOptions.Add(new LayoutOption(layout, button, tile, label));
        }
    }


    /// <summary>
    /// Redraws every layout tile for the paper and names it with its count on that paper.
    /// </summary>
    private void UpdateLayoutOptions(PaperInfo paper, PrintLayoutInput input)
    {
        foreach (var option in _layoutOptions)
        {
            var doc = PrintLayoutEngine.Paginate(input with { Layout = option.Layout, ItemCount = 1, FillPage = false });
            option.Tile.SetPage(doc.PageSizePt, doc.CellRectsPt);
            option.Button.IsChecked = option.Layout.Id == _layout.Id;

            var name = GetLayoutName(option.Layout);
            var hasCount = option.Layout.Kind is PrintLayoutKind.FixedSize or PrintLayoutKind.Wallet or PrintLayoutKind.Passport;
            option.Label.Text = hasCount ? $"{name} ({doc.CellsPerPage})" : name;
            ToolTip.SetTip(option.Button, option.Label.Text);
        }
    }


    private static string GetLayoutName(PrintLayout layout) => layout.Kind switch
    {
        PrintLayoutKind.Grid => Core.Lang[LangId.Print_Layout_Grid, layout.CellCount],
        PrintLayoutKind.Wallet => Core.Lang[LangId.Print_Layout_Wallet],
        PrintLayoutKind.Passport => Core.Lang[LangId.Print_Layout_Passport, layout.FixedName],
        PrintLayoutKind.ContactSheet => Core.Lang[LangId.Print_Layout_ContactSheet],
        PrintLayoutKind.FixedSize => layout.FixedName ?? layout.Id,
        _ => Core.Lang[LangId.Print_Layout_FullPage],
    };


    private void SelectLayout(PrintLayout layout)
    {
        _layout = layout;
        Relayout();
    }


    private void ToggleMore()
    {
        _isMoreVisible = !_isMoreVisible;
        PART_MorePanel.IsVisible = _isMoreVisible;
        PART_BtnMore.IconData = Resx.GetIcon(_isMoreVisible ? ResxIconId.IconChevronDown : ResxIconId.IconChevronRight);
    }

    #endregion // Controls


    #region Printers

    /// <summary>
    /// Lists the printers, a slow network printer bounded by a timeout; a refresh keeps the chosen one unless a printer was just added.
    /// </summary>
    private async Task LoadPrintersAsync(bool isRefresh = false)
    {
        if (_isLoadingPrinters) return;
        _isLoadingPrinters = true;

        IReadOnlyList<PrinterInfo> printers;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_closing.Token);
            timeout.CancelAfter(PRINTER_TIMEOUT_MS);
            printers = await Core.PrintProvider.GetPrintersAsync(timeout.Token);
        }
        catch (Exception ex) when (!_closing.IsCancellationRequested)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintWindowView)}.{nameof(LoadPrintersAsync)}: {ex.Message}");

            // a failed refresh keeps the list there is
            if (isRefresh) return;
            printers = [PrintProviderBase.SaveAsPdfPrinter];
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            _isLoadingPrinters = false;
        }

        if (_closing.IsCancellationRequested) return;

        // the chosen printer keeps its instance, so the requests in flight for it still match
        var listed = PART_Printer.Items.OfType<ComboBoxItem>().Select(i => i.Tag).OfType<PrinterInfo>().ToList();
        printers = printers.Select(i => _printer is { } chosen && i.Id == chosen.Id ? chosen : i).ToList();

        var added = isRefresh ? printers.FirstOrDefault(i => !i.IsVirtual && listed.All(k => k.Id != i.Id)) : null;
        var current = isRefresh ? printers.FirstOrDefault(i => ReferenceEquals(i, _printer)) : null;
        var choice = added
            ?? current
            ?? printers.FirstOrDefault(i => i.Id == _config.LastPrinterId)
            ?? printers.FirstOrDefault(i => i.IsDefault)
            ?? printers[0];

        _isUpdatingControls = true;
        PART_Printer.Items.Clear();
        foreach (var printer in printers)
        {
            PART_Printer.Items.Add(new ComboBoxItem { Content = printer.DisplayName, Tag = printer });
        }
        SelectTag(PART_Printer, choice);
        PART_Printer.IsEnabled = true;
        _isUpdatingControls = false;

        // only the app's own destination: say why, under it
        _noPrintersText = printers.All(i => i.IsVirtual)
            ? Core.PrintProvider.SystemPrintersUnavailableReason ?? Core.Lang[LangId.Print_NoPrinters]
            : null;

        // the same printer keeps its settings, the driver's included; only its state is read again
        if (ReferenceEquals(choice, current))
        {
            _ = ShowPrinterStateAsync(choice);
            return;
        }

        await SelectPrinterAsync(choice);
    }


    /// <summary>
    /// Opens the system's settings to add a printer; from then on, the printers are listed again whenever the window is activated.
    /// </summary>
    private async Task OpenAddPrinterSettingsAsync()
    {
        if (TopLevel.GetTopLevel(this) is not PhWindow owner) return;

        _refreshPrintersOnActivate = true;
        try
        {
            await Core.PrintProvider.OpenAddPrinterSettingsAsync(owner);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintWindowView)}.{nameof(OpenAddPrinterSettingsAsync)}: {ex.Message}");
        }
    }


    /// <summary>
    /// Switches to a printer: reads what it can do, refills the papers and resolutions, and keeps the preview in step.
    /// </summary>
    private async Task SelectPrinterAsync(PrinterInfo printer)
    {
        _printer = printer;
        _platformState = null;
        StateChanged?.Invoke();

        _capsCancel?.Cancel();
        var cancel = CancellationTokenSource.CreateLinkedTokenSource(_closing.Token);
        cancel.CancelAfter(PRINTER_TIMEOUT_MS);
        _capsCancel = cancel;

        PrinterCapabilities caps;
        try
        {
            caps = await Core.PrintProvider.GetCapabilitiesAsync(printer, cancel.Token);
        }
        catch (Exception) when (!ReferenceEquals(_capsCancel, cancel) || _closing.IsCancellationRequested)
        {
            // another printer was picked meanwhile, or the window closed
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintWindowView)}.{nameof(SelectPrinterAsync)}: {ex.Message}");
            ShowPrinterStatus(Core.Lang[LangId.Print_PrinterUnavailable], ResxId.IG_TextDangerBrush);
            caps = PrintProviderBase.PdfCapabilities;
        }

        if (!ReferenceEquals(_printer, printer)) return;

        _caps = caps;
        _isUpdatingControls = true;
        FillPaperItems();
        FillQualityItems();
        UpdateCapabilityRows();
        _isUpdatingControls = false;

        Relayout();
        _ = ShowPrinterStateAsync(printer);
    }


    /// <summary>
    /// Shows the state of the printer under its name, when the printer reports one.
    /// </summary>
    private async Task ShowPrinterStateAsync(PrinterInfo printer)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_closing.Token);
            timeout.CancelAfter(PRINTER_TIMEOUT_MS);
            var status = await Core.PrintProvider.GetStatusAsync(printer, timeout.Token);
            if (!ReferenceEquals(_printer, printer)) return;

            if (printer.IsVirtual || status.State == PrinterState.Unknown)
            {
                ShowPrinterStatus(printer.IsVirtual ? _noPrintersText ?? string.Empty : string.Empty, ResxId.IG_TextWarningBrush);
                return;
            }

            var text = Core.Lang[Lang.GetKey($"{nameof(PrinterState)}_{status.State}")];
            ShowPrinterStatus(string.IsNullOrWhiteSpace(status.Message) ? text : $"{text}: {status.Message}", status.State switch
            {
                PrinterState.Ready => ResxId.IG_TextSuccessBrush,
                PrinterState.Busy or PrinterState.Paused => ResxId.IG_TextWarningBrush,
                _ => ResxId.IG_TextDangerBrush,
            });
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            if (ReferenceEquals(_printer, printer)) PART_PrinterStatusRow.IsVisible = false;
        }
    }


    /// <summary>
    /// Shows a printer's state or a notice right of the Printer label, behind a dot in the given color.
    /// </summary>
    private void ShowPrinterStatus(string text, ResxId dotBrush)
    {
        PART_PrinterStatus.Text = text;
        PART_PrinterStatusDot[!Avalonia.Controls.Shapes.Shape.FillProperty] = Resx.CreateBinding(dotBrush);
        PART_PrinterStatusRow.IsVisible = !string.IsNullOrWhiteSpace(text);
    }


    /// <summary>
    /// Fills the papers of the printer: the one used last on it, else its default.
    /// </summary>
    private void FillPaperItems()
    {
        PART_Paper.Items.Clear();
        foreach (var paper in _caps.Papers)
        {
            PART_Paper.Items.Add(new ComboBoxItem { Content = paper.DisplayName, Tag = paper });
        }

        var lastId = _printer is not null && _config.PaperByPrinter.TryGetValue(_printer.Id, out var id) ? id : null;
        var choice = _caps.Papers.FirstOrDefault(i => i.Id == lastId)
            ?? _caps.Papers.FirstOrDefault(i => i.Id == _caps.DefaultPaperId)
            ?? _caps.Papers.FirstOrDefault();

        if (choice is not null) SelectTag(PART_Paper, choice);
    }


    /// <summary>
    /// Measures the printable area of the chosen paper when the printer only estimated it, then lays the pages out on it.
    /// </summary>
    private async Task MeasureSelectedPaperAsync()
    {
        if (_printer is not { } printer) return;
        if (PART_Paper.SelectedItem is not ComboBoxItem { Tag: PaperInfo { IsPrintableAreaMeasured: false } paper } item) return;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_closing.Token);
            timeout.CancelAfter(PRINTER_TIMEOUT_MS);

            var measured = await Core.PrintProvider.MeasurePaperAsync(printer, paper, timeout.Token);
            if (!ReferenceEquals(_printer, printer)) return;

            // the measured paper replaces its estimate for good; the layout follows only while it is still chosen
            item.Tag = measured;
            _caps = _caps with { Papers = _caps.Papers.Select(i => i.Id == measured.Id ? measured : i).ToList() };
            if (ReferenceEquals(PART_Paper.SelectedItem, item)) Relayout();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintWindowView)}.{nameof(MeasureSelectedPaperAsync)}: {ex.Message}");
        }
    }


    /// <summary>
    /// Fills the resolutions of the printer: the one used last on it, else its default.
    /// </summary>
    private void FillQualityItems()
    {
        var selected = GetTag(PART_Quality, 0);
        PART_Quality.Items.Clear();

        foreach (var dpi in _caps.ResolutionsDpi)
        {
            PART_Quality.Items.Add(new ComboBoxItem { Content = Core.Lang[LangId.Print_Dpi, dpi], Tag = dpi });
        }

        var lastDpi = _printer is not null && _config.DpiByPrinter.TryGetValue(_printer.Id, out var d) ? d : 0;
        var wanted = selected > 0 ? selected : lastDpi > 0 ? lastDpi : _caps.DefaultDpi;
        var choice = _caps.ResolutionsDpi.OrderBy(i => Math.Abs(i - wanted)).FirstOrDefault();

        if (choice > 0) SelectTag(PART_Quality, choice);
    }


    /// <summary>
    /// Shows only the rows the printer supports.
    /// </summary>
    private void UpdateCapabilityRows()
    {
        var printsOnPaper = _printer?.OutputFileExtension is null;

        PART_Copies.Maximum = Math.Max(1, _caps.MaxCopies);
        PART_Copies.IsEnabled = printsOnPaper;
        if (!printsOnPaper) PART_Copies.Value = 1;

        PART_ColorRow.IsVisible = _caps.SupportsColor;
        PART_QualityRow.IsVisible = _caps.ResolutionsDpi.Count > 1;
        PART_DuplexRow.IsVisible = _caps.SupportsDuplex && printsOnPaper;
        PART_BtnProperties.IsVisible = _caps.HasPropertiesDialog;
        UpdateCollateRow();
    }


    private void UpdateCollateRow()
    {
        PART_Collate.IsVisible = (PART_Copies.Value ?? 1) > 1 && _doc is { Pages.Count: > 1 };
    }


    /// <summary>
    /// Shows the printer's own settings dialog and takes its choices back.
    /// </summary>
    private async Task ShowPropertiesAsync()
    {
        if (TopLevel.GetTopLevel(this) is not PhWindow owner) return;

        var settings = BuildSettings();
        if (settings is null || !await Core.PrintProvider.ShowPropertiesDialogAsync(owner, settings)) return;

        _platformState = settings.PlatformState;
        _isUpdatingControls = true;

        // the driver's choices win
        var paper = _caps.Papers.FirstOrDefault(i => i.Id == settings.Paper.Id);
        if (paper is not null) SelectTag(PART_Paper, paper);
        SelectTag(PART_Orientation, settings.IsLandscape ? PrintOrientation.Landscape : PrintOrientation.Portrait);
        PART_Copies.Value = Math.Clamp(settings.Copies, 1, (int)PART_Copies.Maximum);
        PART_Collate.IsChecked = settings.Collate;
        SelectTag(PART_Color, settings.ColorMode);
        SelectTag(PART_Duplex, settings.Duplex);
        if (_caps.ResolutionsDpi.Contains(settings.Dpi)) SelectTag(PART_Quality, settings.Dpi);

        _isUpdatingControls = false;
        Relayout();
    }

    #endregion // Printers


    #region Layout & Preview

    private void OnSettingChanged()
    {
        if (_isUpdatingControls) return;
        Relayout();
    }


    private void OnItemsChanged()
    {
        if (_isUpdatingControls) return;

        ApplyItems();
        Relayout();
    }


    /// <summary>
    /// Sets the prints from the pages and the part to print.
    /// </summary>
    private void ApplyItems()
    {
        var region = GetTag(PART_Region, PrintRegion.WholeImage);
        var scope = GetTag(PART_PageScope, PrintPageScope.Current);

        IReadOnlyList<int> pages = scope switch
        {
            PrintPageScope.All => Enumerable.Range(0, _session.FrameCount).ToList(),
            PrintPageScope.Range => PrintPageRange.Parse(PART_PageRange.Text, _session.FrameCount) ?? [],
            _ => [_session.State.FrameIndex],
        };

        PART_PageRange.IsVisible = scope == PrintPageScope.Range;
        PART_PageRangeHint.IsVisible = scope == PrintPageScope.Range && pages.Count == 0;
        PART_PageRangeHint.Text = Core.Lang[LangId.Print_RangeInvalid];

        // a part of the image prints the page on screen only
        PART_PageScope.IsEnabled = region == PrintRegion.WholeImage;
        PART_PageRange.IsEnabled = region == PrintRegion.WholeImage;

        _session.SetItems(pages, region);
    }


    /// <summary>
    /// Lays the prints out again and redraws the preview.
    /// </summary>
    private void Relayout()
    {
        if (_isUpdatingControls) return;
        if (GetTag<PaperInfo?>(PART_Paper, null) is not { } paper) return;

        var input = new PrintLayoutInput
        {
            Layout = _layout,
            Paper = paper,
            ItemCount = _session.Count,
            FirstItemAspect = _session.GetAspect(0),
            Orientation = GetTag(PART_Orientation, PrintOrientation.Auto),
            Margins = GetTag(PART_Margins, PrintMarginPreset.Normal),
            PrintsPerItem = (int)(PART_PrintsEach.Value ?? 1),
            FillPage = PART_FillPage.IsChecked == true,
            ShowCaptions = PART_Captions.IsChecked == true,
        };

        _doc = PrintLayoutEngine.Paginate(input);
        _pageIndex = Math.Clamp(_pageIndex, 0, Math.Max(0, _doc.Pages.Count - 1));

        UpdateLayoutOptions(paper, input);
        PART_FillPage.IsVisible = _doc.CellsPerPage > 1;
        PART_Warning.Text = Core.Lang[LangId.Print_WarnPrintShrunk];
        PART_Warning.IsVisible = _doc.IsPrintShrunk;
        UpdateCollateRow();
        UpdatePageNav();

        StateChanged?.Invoke();
        RequestRender();
    }


    /// <summary>
    /// Moves to the next or previous page of the preview.
    /// </summary>
    public void GoToPage(int delta)
    {
        if (_doc is null) return;

        var page = Math.Clamp(_pageIndex + delta, 0, Math.Max(0, _doc.Pages.Count - 1));
        if (page == _pageIndex) return;

        _pageIndex = page;
        UpdatePageNav();
        RequestRender();
    }


    private void UpdatePageNav()
    {
        var count = _doc?.Pages.Count ?? 0;
        PART_PageText.Text = count == 0 ? string.Empty : Core.Lang[LangId.Print_PageOf, _pageIndex + 1, count];

        // a single page has nowhere to go
        PART_BtnPrevPage.IsVisible = count > 1;
        PART_BtnNextPage.IsVisible = count > 1;
        PART_BtnPrevPage.IsEnabled = _pageIndex > 0;
        PART_BtnNextPage.IsEnabled = _pageIndex < count - 1;
    }


    private void Session_ImageReady()
    {
        Dispatcher.UIThread.Post(RequestRender);
    }


    private void RequestRender()
    {
        _ = RenderPreviewAsync();
    }


    /// <summary>
    /// Renders the shown page at the size the preview draws it, off the UI thread; a newer request drops this one.
    /// </summary>
    private async Task RenderPreviewAsync()
    {
        var version = Interlocked.Increment(ref _renderVersion);
        await Task.Delay(RENDER_DEBOUNCE_MS);
        if (version != _renderVersion || _closing.IsCancellationRequested) return;

        var doc = _doc;
        if (doc is null || doc.Pages.Count == 0)
        {
            PART_Preview.SetPage(null, doc?.PageSizePt ?? new SKSize(595, 842));
            return;
        }

        // device pixels, so the preview is sharp at any scaling
        var paperRect = PART_Preview.GetPaperRect(doc.PageSizePt);
        var scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
        var w = (int)Math.Round(paperRect.Width * scaling);
        var h = (int)Math.Round(paperRect.Height * scaling);
        if (w < 8 || h < 8) return;

        var pageIndex = _pageIndex;
        var options = BuildRenderOptions() with
        {
            IsPreview = true,
            TargetDpi = (float)(w / PrintUnits.PtToIn(doc.PageSizePt.Width)),
        };
        var destProfile = Core.IsDestColorProfileSupported ? Core.DestColorProfile : null;

        SKImage? page = null;
        try
        {
            page = await Task.Run(() => RenderPage(doc, pageIndex, w, h, options, destProfile));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PrintWindowView)}.{nameof(RenderPreviewAsync)}: {ex.Message}");
        }

        using (page)
        {
            if (version != _renderVersion || page is null || _closing.IsCancellationRequested) return;
            PART_Preview.SetPage(SkiaCodec.ToWritableBitmap(page), doc.PageSizePt);
        }
    }


    /// <summary>
    /// Draws one page of the preview, then turns it into the display's colors as the viewer does.
    /// </summary>
    private SKImage? RenderPage(PrintDocumentLayout doc, int pageIndex, int w, int h, PrintRenderOptions options, SKColorSpace? destProfile)
    {
        var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        using var surface = SKSurface.Create(info);
        if (surface is null) return null;

        surface.Canvas.Scale(w / doc.PageSizePt.Width, h / doc.PageSizePt.Height);

        // a preview never waits: what is not decoded yet draws as a placeholder
        PrintPageRenderer.DrawPageAsync(surface.Canvas, doc, pageIndex, _session, options, CancellationToken.None)
            .GetAwaiter().GetResult();

        var snapshot = surface.Snapshot();
        if (destProfile is not null && SkiaCodec.TryApplyColorSpace(snapshot, destProfile, out var profiled))
        {
            snapshot.Dispose();
            return profiled;
        }

        return snapshot;
    }

    #endregion // Layout & Preview


    #region Print

    /// <summary>
    /// Prints, or writes the file of a destination that writes one; returns whether the job finished.
    /// </summary>
    public async Task<bool> PrintAsync(PhWindow owner)
    {
        if (!CanPrint || _doc is null || _printer is null) return false;

        // a long job asks first
        var copies = (int)(PART_Copies.Value ?? 1);
        var sheets = _doc.Pages.Count * copies;
        if (sheets > MANY_PAGES)
        {
            var answer = await ModalWindow.ShowWarningAsync(owner, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Print_Title],
                Description = Core.Lang[LangId.Print_WarnManyPages, sheets],
            }, ModalWindowButton.OK_Cancel);
            if (answer.ExitCode != DialogExitCode.OK) return false;
        }

        var settings = BuildSettings();
        if (settings is null) return false;

        // a destination that writes a file asks where
        if (_printer.OutputFileExtension is { } extension)
        {
            var path = await PickOutputFileAsync(extension);
            if (string.IsNullOrEmpty(path)) return false;
            settings.OutputPath = path;
        }

        var job = BuildJob(settings);
        var cancel = new CancellationTokenSource();
        _printCancel = cancel;
        SetPrinting(true);

        try
        {
            var progress = new Progress<PrintProgress>(ShowProgress);
            await Task.Run(() => Core.PrintProvider.PrintAsync(job, progress, cancel.Token), cancel.Token);

            SaveConfig();
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Print_Title],
                Heading = Core.Lang[LangId.Menu_MnuPrint_Error],
                Description = ex.Message,
                Details = BHelper.GetExceptionDetails(ex),
            });
            return false;
        }
        finally
        {
            _printCancel = null;
            cancel.Dispose();
            SetPrinting(false);
        }
    }


    /// <summary>
    /// Cancels the running job.
    /// </summary>
    public void CancelPrinting()
    {
        _printCancel?.Cancel();
    }


    /// <summary>
    /// Hands the job to the platform's own print dialog; returns whether it opened.
    /// </summary>
    public async Task<bool> ShowSystemDialogAsync(PhWindow owner)
    {
        var settings = BuildSettings();
        if (settings is null) return false;

        try
        {
            await Core.PrintProvider.ShowSystemDialogAsync(owner, BuildJob(settings), _closing.Token);
            SaveConfig();
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await ModalWindow.ShowErrorAsync(owner, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Print_Title],
                Heading = Core.Lang[LangId.Menu_MnuPrint_Error],
                Description = ex.Message,
                Details = BHelper.GetExceptionDetails(ex),
            });
            return false;
        }
    }


    /// <summary>
    /// Saves the choices, so the next Print window starts from them.
    /// </summary>
    public void SaveConfig()
    {
        if (_printer is not null)
        {
            _config.LastPrinterId = _printer.Id;
            if (GetTag<PaperInfo?>(PART_Paper, null) is { } paper) _config.PaperByPrinter[_printer.Id] = paper.Id;
            if (GetTag(PART_Quality, 0) is var dpi and > 0) _config.DpiByPrinter[_printer.Id] = dpi;
        }

        _config.LayoutId = _layout.Id;
        _config.Fit = GetTag(PART_Fit, PrintFitMode.Fit);
        _config.Orientation = GetTag(PART_Orientation, PrintOrientation.Auto);
        _config.Margins = GetTag(PART_Margins, PrintMarginPreset.Normal);
        _config.ColorMode = GetTag(PART_Color, PrintColorMode.Color);
        _config.Duplex = GetTag(PART_Duplex, PrintDuplex.None);
        _config.AutoRotate = PART_AutoRotate.IsChecked == true;
        _config.ShowCaptions = PART_Captions.IsChecked == true;
        _config.FillPage = PART_FillPage.IsChecked == true;
        _config.Collate = PART_Collate.IsChecked == true;
        _config.Save();
    }


    private PrintJobSettings? BuildSettings()
    {
        if (_printer is null || GetTag<PaperInfo?>(PART_Paper, null) is not { } paper) return null;

        return new PrintJobSettings
        {
            Printer = _printer,
            Paper = paper,
            IsLandscape = _doc?.IsLandscape ?? false,
            Copies = (int)(PART_Copies.Value ?? 1),
            Collate = PART_Collate.IsChecked == true,
            Duplex = PART_DuplexRow.IsVisible ? GetTag(PART_Duplex, PrintDuplex.None) : PrintDuplex.None,
            ColorMode = PART_ColorRow.IsVisible ? GetTag(PART_Color, PrintColorMode.Color) : PrintColorMode.Color,
            Dpi = GetTag(PART_Quality, 0) is var dpi and > 0 ? dpi : _caps.DefaultDpi,
            PlatformState = _platformState,
        };
    }


    private PrintJob BuildJob(PrintJobSettings settings)
    {
        return new PrintJob
        {
            Settings = settings,
            Title = _session.GetCaption(0),
            Layout = _doc!,
            Session = _session,
            Render = BuildRenderOptions(),
        };
    }


    private PrintRenderOptions BuildRenderOptions()
    {
        // the resolution the file declares, for actual size; a missing or odd value reads as the screen's 96
        var dpi = _session.State.Photo.Metadata?.DpiX ?? 0;

        return new PrintRenderOptions
        {
            Fit = GetTag(PART_Fit, PrintFitMode.Fit),
            AutoRotate = PART_AutoRotate.IsChecked == true,
            ColorMode = PART_ColorRow.IsVisible ? GetTag(PART_Color, PrintColorMode.Color) : PrintColorMode.Color,
            ImageDpi = dpi is >= 30 and <= 4800 ? (float)dpi : 96,
        };
    }


    /// <summary>
    /// Asks where to write the file of a destination that writes one, next to the photo by default.
    /// </summary>
    private async Task<string?> PickOutputFileAsync(string extension)
    {
        if (TopLevel.GetTopLevel(this) is not { } top) return null;

        var photoPath = _session.State.Photo.FilePath;
        var name = string.IsNullOrEmpty(photoPath) ? "ImageGlass" : Path.GetFileNameWithoutExtension(photoPath);
        var folder = string.IsNullOrEmpty(photoPath) ? null : await top.StorageProvider.TryGetFolderFromPathAsync(Path.GetDirectoryName(photoPath)!);

        var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Core.Lang[LangId.Print_Title],
            SuggestedFileName = name + extension,
            SuggestedStartLocation = folder,
            DefaultExtension = extension.TrimStart('.'),
            ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType(extension.TrimStart('.').ToUpperInvariant()) { Patterns = ["*" + extension] }],
        });

        return file?.TryGetLocalPath();
    }


    private void SetPrinting(bool isPrinting)
    {
        PART_Progress.IsVisible = isPrinting;
        PART_ProgressBar.Value = 0;
        PART_ProgressText.Text = string.Empty;
        SetSettingsEnabled(!isPrinting);
        StateChanged?.Invoke();
    }


    private void SetSettingsEnabled(bool isEnabled)
    {
        PART_Settings.IsEnabled = isEnabled;
        PART_BtnPrevPage.IsEnabled = isEnabled && _pageIndex > 0;
        PART_BtnNextPage.IsEnabled = isEnabled && _pageIndex < (_doc?.Pages.Count ?? 0) - 1;
    }


    private void ShowProgress(PrintProgress progress)
    {
        if (progress.PageCount <= 0) return;

        PART_ProgressBar.Value = progress.PageIndex / (double)progress.PageCount;
        PART_ProgressText.Text = Core.Lang[LangId.Print_Printing, Math.Min(progress.PageIndex + 1, progress.PageCount), progress.PageCount];
    }

    #endregion // Print


    private sealed record LayoutOption(PrintLayout Layout, PhToolButton Button, PrintLayoutTile Tile, TextBlock Label);
}
