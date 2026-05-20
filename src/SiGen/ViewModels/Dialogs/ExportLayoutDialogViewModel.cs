using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SiGen.Export;
using SiGen.Layouts;
using SiGen.Layouts.Builders;
using SiGen.Measuring;
using SiGen.Services;
using SiGen.Services.InstrumentProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DrawingColor = System.Drawing.Color;

namespace SiGen.ViewModels.Dialogs
{
    public partial class ExportLayoutDialogViewModel : DialogViewModelBase
    {
        public override string Title => "Export Layout";

        private ILayoutDocument? _context;

        [ObservableProperty]
        private StringedInstrumentLayout? _layout;



        private readonly IDialogService dialogService;
        private readonly IPdfPrinterService? pdfPrinterService;

        public IAsyncRelayCommand ExportCommand { get; }
        public IAsyncRelayCommand PrintCommand { get; }

        #region General Settings

        [ObservableProperty]
        private LengthUnit _unit = LengthUnit.Cm;

        [ObservableProperty]
        private ExportTargetFormat _format = ExportTargetFormat.Svg;

        public bool IsExportingToPdf => Format == ExportTargetFormat.Pdf;

        [ObservableProperty]
        private bool _inkscapeCompatible = true;

        #endregion

        #region Frets

        [ObservableProperty]
        private bool _exportFrets = true;

        [ObservableProperty]
        private Color? _fretColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _fretLineThickness;

        [ObservableProperty]
        private bool _fretDashed;

        [ObservableProperty]
        private bool _extendFrets;

        [ObservableProperty]
        private Measure? _fretExtensionAmount;

        #endregion

        #region Strings

        [ObservableProperty]
        private bool _exportStrings;

        [ObservableProperty]
        private Color? _stringColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _stringLineThickness;

        [ObservableProperty]
        private bool _stringDashed;

        [ObservableProperty]
        private bool _useStringGauge;

        #endregion

        #region Fingerboard

        [ObservableProperty]
        private bool _exportFingerboard = true;

        [ObservableProperty]
        private Color? _fingerboardColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _fingerboardLineThickness;

        [ObservableProperty]
        private bool _fingerboardDashed;

        [ObservableProperty]
        private bool _exportFingerboardProjectionLines;

        [ObservableProperty]
        private Color? _fingerboardProjectionColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _fingerboardProjectionLineThickness;

        [ObservableProperty]
        private bool _fingerboardProjectionDashed = true;

        #endregion

        #region Center Line

        [ObservableProperty]
        private bool _exportCenterLine = true;

        [ObservableProperty]
        private Color? _centerLineColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _centerLineThickness;

        [ObservableProperty]
        private bool _centerLineDashed;

        #endregion

        #region Medians

        [ObservableProperty]
        private bool _exportMedians;

        [ObservableProperty]
        private Color? _mediansColor = Colors.Black;

        [ObservableProperty]
        private LineThickness? _mediansLineThickness;

        [ObservableProperty]
        private bool _mediansDashed = true;

        #endregion

        #region PDF Parameters

        public List<PdfPaperSize> AvailablePaperSizes => new() { PdfPaperSize.A4, PdfPaperSize.Letter, PdfPaperSize.Legal };
        public Array PageOrientations => Enum.GetValues(typeof(PdfPageOrientation));

        [ObservableProperty]
        private PdfPaperSize? selectedPaperSize;

        [ObservableProperty]
        private PdfPageOrientation selectedPageOrientation;

        [ObservableProperty]
        private Measure? _pdfMarginLeft;

        [ObservableProperty]
        private Measure? _pdfMarginTop;

        [ObservableProperty]
        private Measure? _pdfMarginRight;

        [ObservableProperty]
        private Measure? _pdfMarginBottom;

        public PdfPageMargin PdfContentMargin
        {
            get => new(
                PdfMarginLeft ?? Measure.Mm(10),
                PdfMarginTop ?? Measure.Mm(10),
                PdfMarginRight ?? Measure.Mm(10),
                PdfMarginBottom ?? Measure.Mm(10)
            );
        }

        [ObservableProperty]
        private Measuring.Measure? _pdfOverlap;

        #endregion

        /// <summary>
        /// Gets the current export options based on the ViewModel state.
        /// This property automatically updates when any export option changes.
        /// </summary>
        public BaseExportOptions ExportOptions => CreateExportOptions();

        //designer constructor with default values
        public ExportLayoutDialogViewModel()
        {
            dialogService = new MockDialogService();
            var provider = new ElectricGuitarValuesProvider();
            var layoutConfig = provider.GetDefaultConfiguration();
            var result = LayoutBuilder.Build(layoutConfig);
            ExportCommand = new AsyncRelayCommand(ExportLayout);
            PrintCommand = new AsyncRelayCommand(PrintLayout);
            _layout = result.Layout;
            SelectedPageOrientation = PdfPageOrientation.Portrait;
            SelectedPaperSize = PdfPaperSize.Letter;
            PdfMarginLeft = Measure.Mm(10);
            PdfMarginTop = Measure.Mm(10);
            PdfMarginRight = Measure.Mm(10);
            PdfMarginBottom = Measure.Mm(10);
            PdfOverlap = Measure.Mm(10);
        }

        [ActivatorUtilitiesConstructor]
        public ExportLayoutDialogViewModel(ILayoutDocument? document, IDialogService dialogService, IPdfPrinterService pdfPrinterService)
        {
            _context = document;
            _layout = document?.Layout;
            this.dialogService = dialogService;
            this.pdfPrinterService = pdfPrinterService;

            ExportCommand = new AsyncRelayCommand(ExportLayout);
            PrintCommand = new AsyncRelayCommand(PrintLayout, () => pdfPrinterService.CanPrintPdf);
            SelectedPageOrientation = PdfPageOrientation.Portrait;
            SelectedPaperSize = PdfPaperSize.Letter;
            PdfMarginLeft = Measure.Mm(10);
            PdfMarginTop = Measure.Mm(10);
            PdfMarginRight = Measure.Mm(10);
            PdfMarginBottom = Measure.Mm(10);
            PdfOverlap = Measure.Mm(10);
            Resizable = true;
        }

        // Subscribe to all property changes to notify ExportOptions has changed
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Notify that PdfContentMargin has changed when any individual margin property changes
            if (e.PropertyName == nameof(PdfMarginLeft) || 
                e.PropertyName == nameof(PdfMarginTop) || 
                e.PropertyName == nameof(PdfMarginRight) || 
                e.PropertyName == nameof(PdfMarginBottom))
            {
                OnPropertyChanged(nameof(PdfContentMargin));
            }

            // Notify that ExportOptions has changed whenever any option property changes
            if (e.PropertyName != nameof(ExportOptions) && e.PropertyName != nameof(Layout) && e.PropertyName != nameof(Format))
            {
                OnPropertyChanged(nameof(ExportOptions));
            }

            if (e.PropertyName == nameof(Format))
            {
                OnPropertyChanged(nameof(IsExportingToPdf));
            }
        }


        #region Color Conversion Helpers

        private static DrawingColor? ToDrawingColor(Color? avaloniaColor)
        {
            if (!avaloniaColor.HasValue)
                return null;

            var c = avaloniaColor.Value;
            return DrawingColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        private static Color? ToAvaloniaColor(DrawingColor? drawingColor)
        {
            if (!drawingColor.HasValue)
                return null;

            var c = drawingColor.Value;
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        #endregion

        public async Task ExportLayout()
        {
            string getDefaultFileName()
            {
                
                string layoutName = "layout";

                if (!string.IsNullOrEmpty(_context?.Title))
                    layoutName = _context.Title;
                else if (!string.IsNullOrEmpty(_context?.FilePath))
                    layoutName = System.IO.Path.GetFileNameWithoutExtension(_context.FilePath);

                return Format switch
                {
                    ExportTargetFormat.Svg => $"{layoutName}.svg",
                    ExportTargetFormat.Dxf => $"{layoutName}.dxf",
                    ExportTargetFormat.Pdf => $"{layoutName}.pdf",
                    _ => layoutName
                };
            }

            IEnumerable<FileDialogFilter> getDialogFilters()
            {
                return Format switch
                {
                    ExportTargetFormat.Svg => [new FileDialogFilter("SVG Files", "svg")],
                    ExportTargetFormat.Dxf => [new FileDialogFilter("DXF Files", "dxf")],
                    ExportTargetFormat.Pdf => [new FileDialogFilter("PDF Files", "pdf")],
                    _ => Array.Empty<FileDialogFilter>()
                };
            }

            var selectedFilePath = await dialogService.ShowSaveFileDialogAsync("Export Layout", getDefaultFileName(), getDialogFilters());
            if (string.IsNullOrEmpty(selectedFilePath)) return;

            var options = CreateExportOptions();

            ILayoutExporter? exporter = Format switch { 
                ExportTargetFormat.Svg => new SvgLayoutExporter((SvgExportOptions)options, Layout!),
                ExportTargetFormat.Dxf => new DxfLayoutExporter((DxfExportOptions)options, Layout!),
                ExportTargetFormat.Pdf => new PdfLayoutExporter((PdfExportOptions)options, Layout!),
                _ => throw new InvalidOperationException("Unsupported export format")

            };

            exporter?.ExportLayout(ExportTarget.ToFile(selectedFilePath));

            CompleteDialog(true);
        }
        
        public async Task PrintLayout()
        {
            if (pdfPrinterService == null || !pdfPrinterService.CanPrintPdf)
                return;

            Format = ExportTargetFormat.Pdf;
            var options = CreateExportOptions();

            var exporter = new PdfLayoutExporter((PdfExportOptions)options, Layout!);
            string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"SiGen_Print_{Guid.NewGuid()}.pdf");

            try
            {
                exporter.ExportLayout(ExportTarget.ToFile(tempFilePath));
                await pdfPrinterService.PrintPdfAsync(tempFilePath);

                // Give the print system time to spool the file before deleting
                await Task.Delay(2000);
            }
            catch
            {
                // Handle errors silently for now
            }
            finally
            {
                // Clean up temp file
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                }
                catch { }
            }
        }

        /// <summary>
        /// Creates export options from the current ViewModel state
        /// </summary>
        public BaseExportOptions CreateExportOptions()
        {
            BaseExportOptions options = Format switch
            {
                ExportTargetFormat.Svg => new SvgExportOptions { InkscapeCompatible = InkscapeCompatible },
                ExportTargetFormat.Dxf => new DxfExportOptions(),
                ExportTargetFormat.Pdf => new PdfExportOptions()
                {
                    Paper = SelectedPaperSize ?? PdfPaperSize.Letter,
                    Orientation = SelectedPageOrientation,
                    ContentMargin = PdfContentMargin,
                    PageOverlap = PdfOverlap ?? Measure.Cm(1),
                },
                _ => throw new InvalidOperationException("Unsupported export format")
            };

            options.Unit = Unit;

            // Frets
            options.Frets.Enabled = ExportFrets;
            options.Frets.Color = ToDrawingColor(FretColor);
            options.Frets.LineThickness = FretLineThickness;
            options.Frets.Dashed = FretDashed;
            options.Frets.Extend = ExtendFrets;
            options.Frets.ExtensionAmount = FretExtensionAmount;

            // Strings
            options.Strings.Enabled = ExportStrings;
            options.Strings.Color = ToDrawingColor(StringColor);
            options.Strings.LineThickness = StringLineThickness;
            options.Strings.Dashed = StringDashed;
            options.Strings.UseGauge = UseStringGauge;

            // Fingerboard
            options.Fingerboard.Enabled = ExportFingerboard;
            options.Fingerboard.Color = ToDrawingColor(FingerboardColor);
            options.Fingerboard.LineThickness = FingerboardLineThickness;
            options.Fingerboard.Dashed = FingerboardDashed;

            options.Fingerboard.ProjectionLines.Enabled = ExportFingerboardProjectionLines;
            options.Fingerboard.ProjectionLines.Color = ToDrawingColor(FingerboardProjectionColor);
            options.Fingerboard.ProjectionLines.LineThickness = FingerboardProjectionLineThickness;
            options.Fingerboard.ProjectionLines.Dashed = FingerboardProjectionDashed;

            // Center Line
            options.CenterLine.Enabled = ExportCenterLine;
            options.CenterLine.Color = ToDrawingColor(CenterLineColor);
            options.CenterLine.LineThickness = CenterLineThickness;
            options.CenterLine.Dashed = CenterLineDashed;

            // Medians
            options.Medians.Enabled = ExportMedians;
            options.Medians.Color = ToDrawingColor(MediansColor);
            options.Medians.LineThickness = MediansLineThickness;
            options.Medians.Dashed = MediansDashed;

            return options;
        }

        /// <summary>
        /// Loads export options into the ViewModel
        /// </summary>
        public void LoadFromExportOptions(BaseExportOptions options)
        {
            Unit = options.Unit;

            if (options is SvgExportOptions svgOptions)
            {
                Format = ExportTargetFormat.Svg;
                InkscapeCompatible = svgOptions.InkscapeCompatible;
            }
            else if (options is DxfExportOptions dxfOptions)
            {
                Format = ExportTargetFormat.Dxf;
            }
            else if (options is PdfExportOptions pdfOptions)
            {
                Format = ExportTargetFormat.Pdf;
                SelectedPaperSize = pdfOptions.Paper;
                SelectedPageOrientation = pdfOptions.Orientation;
                PdfMarginLeft = pdfOptions.ContentMargin.Left;
                PdfMarginTop = pdfOptions.ContentMargin.Top;
                PdfMarginRight = pdfOptions.ContentMargin.Right;
                PdfMarginBottom = pdfOptions.ContentMargin.Bottom;
                PdfOverlap = pdfOptions.PageOverlap;
            }

            // Frets
            ExportFrets = options.Frets.Enabled;
            FretColor = ToAvaloniaColor(options.Frets.Color);
            FretLineThickness = options.Frets.LineThickness;
            FretDashed = options.Frets.Dashed;
            ExtendFrets = options.Frets.Extend;
            FretExtensionAmount = options.Frets.ExtensionAmount;

            // Strings
            ExportStrings = options.Strings.Enabled;
            StringColor = ToAvaloniaColor(options.Strings.Color);
            StringLineThickness = options.Strings.LineThickness;
            StringDashed = options.Strings.Dashed;
            UseStringGauge = options.Strings.UseGauge;

            // Fingerboard
            ExportFingerboard = options.Fingerboard.Enabled;
            FingerboardColor = ToAvaloniaColor(options.Fingerboard.Color);
            FingerboardLineThickness = options.Fingerboard.LineThickness;
            FingerboardDashed = options.Fingerboard.Dashed;

            ExportFingerboardProjectionLines = options.Fingerboard.ProjectionLines.Enabled;
            FingerboardProjectionColor = ToAvaloniaColor(options.Fingerboard.ProjectionLines.Color);
            FingerboardProjectionLineThickness = options.Fingerboard.ProjectionLines.LineThickness;
            FingerboardProjectionDashed = options.Fingerboard.ProjectionLines.Dashed;

            // Center Line
            ExportCenterLine = options.CenterLine.Enabled;
            CenterLineColor = ToAvaloniaColor(options.CenterLine.Color);
            CenterLineThickness = options.CenterLine.LineThickness;
            CenterLineDashed = options.CenterLine.Dashed;

            // Medians
            ExportMedians = options.Medians.Enabled;
            MediansColor = ToAvaloniaColor(options.Medians.Color);
            MediansLineThickness = options.Medians.LineThickness;
            MediansDashed = options.Medians.Dashed;
        }
    }
}
