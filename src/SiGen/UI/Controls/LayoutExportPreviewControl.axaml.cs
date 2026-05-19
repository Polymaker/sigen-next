using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.Extensions.Options;
using netDxf.Objects;
using SiGen.Export;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using DrawingColor = System.Drawing.Color;

namespace SiGen.UI.Controls;

public partial class LayoutExportPreviewControl : UserControl
{
    public static readonly StyledProperty<StringedInstrumentLayout?> LayoutProperty =
        AvaloniaProperty.Register<LayoutExportPreviewControl, StringedInstrumentLayout?>(nameof(Layout));

    public static readonly StyledProperty<BaseExportOptions?> ExportOptionsProperty =
        AvaloniaProperty.Register<LayoutExportPreviewControl, BaseExportOptions?>(nameof(ExportOptions));

    public StringedInstrumentLayout? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public BaseExportOptions? ExportOptions
    {
        get => GetValue(ExportOptionsProperty);
        set => SetValue(ExportOptionsProperty, value);
    }

    private TranslateTransform _centerTransform;
    private ScaleTransform _zoomTransform;
    private TranslateTransform _panTransform;
    private double _currentZoom = 1.0;
    private double _fitToViewZoom = 1.0;
    private const double ZoomStep = 1.2;
    private const double MinZoom = 0.1;
    private const double MaxZoom = 10.0;

    private bool _isPanning = false;
    private Point _lastPanPosition;

    public LayoutExportPreviewControl()
    {
        InitializeComponent();

        // Setup transforms: center, pan, then scale (with Y flip)
        _centerTransform = new TranslateTransform(0, 0);
        _panTransform = new TranslateTransform(0, 0);
        _zoomTransform = new ScaleTransform(1, -1); // Flip Y axis

        var transformGroup = new TransformGroup
        {
            Children = { _centerTransform, _panTransform, _zoomTransform }
        };
        PreviewCanvas.RenderTransform = transformGroup;

        // Wire up pointer events for panning on the container border
        ContainerBorder.PointerPressed += Container_PointerPressed;
        ContainerBorder.PointerMoved += Container_PointerMoved;
        ContainerBorder.PointerReleased += Container_PointerReleased;
        ContainerBorder.PointerCaptureLost += Container_PointerCaptureLost;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LayoutProperty || change.Property == ExportOptionsProperty)
        {
            RebuildPreview();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (!double.IsNaN(e.NewSize.Width) && !double.IsNaN(e.NewSize.Height))
        {
            _centerTransform.X = e.NewSize.Width / 2.0;
            _centerTransform.Y = e.NewSize.Height / 2.0;
        }

        CalculateZoomToFit();
        SetZoom(_fitToViewZoom);
        RebuildPreview();
    }

    private void CalculateZoomToFit()
    {
        if (Layout?.Bounds == null || Bounds.Width == 0 || Bounds.Height == 0)
        {
            _fitToViewZoom = 1.0;
            return;
        }

        var layoutBounds = Layout.Bounds;
        var availableSize = new Size(Bounds.Width - 20, Bounds.Height - 20); // 20px margin

        // Calculate zoom to fit both width and height
        var zoomX = availableSize.Width / (layoutBounds.Width.NormalizedValue * MeasureUtils.CmToPixels);
        var zoomY = availableSize.Height / (layoutBounds.Height.NormalizedValue * MeasureUtils.CmToPixels);

        _fitToViewZoom = Math.Min(zoomX, zoomY);
    }

    private void SetZoom(double zoom)
    {
        _currentZoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        _zoomTransform.ScaleX = _currentZoom;
        _zoomTransform.ScaleY = _currentZoom * -1.0; // Keep Y flipped
    }

    public void ZoomIn()
    {
        SetZoom(_currentZoom * ZoomStep);
    }

    public void ZoomOut()
    {
        SetZoom(_currentZoom / ZoomStep);
    }

    public void ResetView()
    {
        CalculateZoomToFit();
        SetZoom(_fitToViewZoom);
        _panTransform.X = 0;
        _panTransform.Y = 0;
    }

    private void RebuildPreview()
    {
        PreviewCanvas.Children.Clear();
        if (Layout is null || ExportOptions is null) return;

        GenerateBackgroundPage();

        if (ExportOptions.ExportCenterLine)
        {
            var bounds = Layout.Bounds!;
            var centerLine = new LinearPath(
                new PointM(bounds.Left + bounds.Width / 2, bounds.Top).ToVector(),
                new PointM(bounds.Left + bounds.Width / 2, bounds.Bottom).ToVector()
            );
            GenerateShapesFromElements([centerLine], ExportOptions.CenterLine);
        }

        if (ExportOptions.ExportFingerboard)
        {
            var edgePaths = Layout.Elements.OfType<FingerboardEdgeElement>().Select(e => e.Path);
            GenerateShapesFromElements(edgePaths, ExportOptions.Fingerboard);

            if (ExportOptions.Fingerboard.ProjectionLines.Enabled)
            {
                var projectionPaths = Layout.Elements.OfType<GuideLineElement>().Where(x => x.Type == GuideLineType.FretboardProjection).Select(e => e.Path);
                GenerateShapesFromElements(projectionPaths, ExportOptions.Fingerboard.ProjectionLines);
            }
        }

        if (ExportOptions.ExportStrings)
        {
            foreach(var stringElem in Layout.Strings)
            {
                var shape = GetAvaloniaShape(stringElem.Path, ExportOptions.Strings);
                if (ExportOptions.Strings.UseGauge && shape is not null && stringElem.Configuration.IsGaugeDefined == true)
                {
                    shape.StrokeThickness = GetStrokeThickness(new LineThickness(stringElem.GetGauge()!.Value[LengthUnit.Mm], ThicknessUnit.Millimeter));
                }

                if (shape != null)
                    PreviewCanvas.Children.Add(shape);
            }
        }

        if (ExportOptions.ExportFrets)
        {
            int lastStringIndex = Layout.NumberOfStrings - 1;

            var fretPaths = Layout.Elements.OfType<FretSegmentElement>().Where(x => x.FretShape is not null).Select(e =>
            {
                if (ExportOptions.Frets.Extend && !(e.IsNut || e.IsBridge) && ExportOptions.Frets.ExtensionAmount.HasValue)
                {
                    TrimExtendSide sides = TrimExtendSide.None;
                    if (e.ContainsString(0)) sides |= TrimExtendSide.Start;
                    if (e.ContainsString(lastStringIndex)) sides |= TrimExtendSide.End;
                    return e.FretShape!.TrimExtend(sides, ExportOptions.Frets.ExtensionAmount.Value.NormalizedValue) ?? e.FretShape;
                }
                return e.FretShape!;
            });

            GenerateShapesFromElements(fretPaths, ExportOptions.Frets);
        }

        if (ExportOptions.ExportMedians)
        {
            var medianPaths = Layout.Elements.OfType<GuideLineElement>().Where(x => x.Type == GuideLineType.StringMedian).Select(e => e.Path);
            GenerateShapesFromElements(medianPaths, ExportOptions.Medians);
        }
        
    }

    private void GenerateBackgroundPage()
    {
        if (Layout?.Bounds == null)
        {
            return;
        }

        

        Rectangle AddRectangle(Rect rect, IBrush? fill, IBrush? stroke = null, double? strokeThickness = null)
        {
            var rectShape = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness ?? 0
            };
            PreviewCanvas.Children.Add(rectShape);
            Canvas.SetLeft(rectShape, rect.X);
            Canvas.SetTop(rectShape, rect.Top * -1);
            return rectShape;
        }

        if (ExportOptions is PdfExportOptions pdfOptions)
        {

            var layoutPlan = PdfLayoutExporter.BuildPlan(pdfOptions, Layout);
            
            var paperSize = new SizeM(layoutPlan.PageSize.Width, layoutPlan.PageSize.Height).ToAvalonia();


            var tiledSize = layoutPlan.TiledSize.ToAvalonia();
            
            var totalPrintBounds = new RectangleM(layoutPlan.TiledSize.Width * -0.5, layoutPlan.TiledSize.Height * 0.5, layoutPlan.TiledSize.Width, layoutPlan.TiledSize.Height);
            totalPrintBounds.Height += pdfOptions.ContentMargin.Vertical;
            totalPrintBounds.Top += pdfOptions.ContentMargin.Top;
            totalPrintBounds.Width += pdfOptions.ContentMargin.Horizontal;
            totalPrintBounds.Left -= pdfOptions.ContentMargin.Left;

            var bgRectangle = totalPrintBounds.ToAvalonia();

            var bgRect = AddRectangle(bgRectangle, Brushes.White);
            bgRect.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                Opacity = 0.5,
                OffsetY = -10,
            };
            var verticalOverlap = (pdfOptions.PageOverlap + pdfOptions.ContentMargin.Vertical).ToPixels();
            var horizontalOverlap = (pdfOptions.PageOverlap + pdfOptions.ContentMargin.Horizontal).ToPixels();
            foreach (var page in layoutPlan.Pages)
            {
                var posX = page.Column * (paperSize.Width - horizontalOverlap);
                var posY = page.Row * (paperSize.Height - verticalOverlap);


                var pageRect = new Rect(new Point(bgRectangle.Left + posX, bgRectangle.Top - posY), paperSize);

                AddRectangle(pageRect, null, Brushes.LightBlue, 1.5);

                //var pageNumberText = new TextBlock
                //{
                //    Text = $"Page {page.Metadata.PageNumber}",
                //    Foreground = Brushes.Gray,
                //    FontSize = 20
                //};
                //pageNumberText.RenderTransform = new ScaleTransform(1, -1); // Flip text back to normal
                //PreviewCanvas.Children.Add(pageNumberText);
                //Canvas.SetLeft(pageNumberText, pageRect.Left + 10);
                //Canvas.SetTop(pageNumberText, (pageRect.Top * -1) + 10);
            }
        }
        else
        {
            var bgRectangle = Layout.Bounds.Inflate(Measuring.Measure.Cm(1)).ToAvalonia();
            var bgRect = AddRectangle(bgRectangle, Brushes.White);
            bgRect.Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                Opacity = 0.5
            };
        }


    }

    private void GenerateShapesFromElements(IEnumerable<PathBase> paths, LineExportOptions lineOptions)
    {
        foreach (var path in paths)
        {
            var shape = GetAvaloniaShape(path, lineOptions);
            if (shape != null)
                PreviewCanvas.Children.Add(shape);
        }
    }

    private Shape? GetAvaloniaShape(PathBase pathBase, LineExportOptions lineOptions)
    {
        Shape? createdShape = null;

        if (pathBase is LinearPath linear)
        {
            createdShape = new Line
            {
                StartPoint = linear.Start.ToAvalonia(),
                EndPoint = linear.End.ToAvalonia(),
            };
        }
        else if (pathBase is PolyLinePath polyPath)
        {
            createdShape = new Polyline
            {
                Points = polyPath.Points.Select(x => x.ToAvalonia()).ToList()
            };
        }
        else if (pathBase is BezierSplinePath bezierSpline)
        {
            var segments = bezierSpline.GetSegments();
            if (segments.Count > 0)
            {
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    ctx.BeginFigure(segments[0].P0.ToAvalonia(), false);
                    foreach (var seg in segments)
                        ctx.CubicBezierTo(seg.P1.ToAvalonia(), seg.P2.ToAvalonia(), seg.P3.ToAvalonia());
                    ctx.EndFigure(false);
                }
                createdShape = new Path
                {
                    Data = geom
                };
            }
        }

        if (createdShape != null)
        {
            createdShape.Stroke = GetStrokeBrush(lineOptions.Color); 
            createdShape.StrokeThickness = GetStrokeThickness(lineOptions.LineThickness ?? new LineThickness(1, ThicknessUnit.Point));
            createdShape.StrokeDashArray = lineOptions.Dashed ? new AvaloniaList<double> { 8, 4 } : null;
        }
        return createdShape;

    }

    private double GetStrokeThickness(LineThickness lineThickness)
    {
        // Convert all thicknesses to device-independent pixels (96 DPI)
        return lineThickness.Unit switch
        {
            ThicknessUnit.Pixel => lineThickness.Value,
            ThicknessUnit.Point => lineThickness.Value * 96.0 / 72.0,  // 1 point = 1/72 inch, 1 pixel = 1/96 inch
            ThicknessUnit.Millimeter => lineThickness.Value * 96.0 / 25.4,  // 1 inch = 25.4mm, 96 pixels/inch
            _ => lineThickness.Value
        };
    }

    private static Color? ToAvaloniaColor(DrawingColor? drawingColor)
    {
        if (!drawingColor.HasValue)
            return null;

        var c = drawingColor.Value;
        return Color.FromArgb(c.A, c.R, c.G, c.B);
    }

    private static IBrush GetStrokeBrush(DrawingColor? drawingColor)
    {
        var avaloniaColor = ToAvaloniaColor(drawingColor);
        return avaloniaColor.HasValue ? new SolidColorBrush(avaloniaColor.Value) : Brushes.Transparent;
    }

    #region UI Event Handlers

    private void ZoomInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ZoomIn();
    }

    private void ZoomOutButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ZoomOut();
    }

    private void ResetViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResetView();
    }

    #endregion

    #region Panning

    private void Container_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(ContainerBorder);

        // Start panning on left or middle mouse button
        if (point.Properties.IsLeftButtonPressed || point.Properties.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _lastPanPosition = e.GetPosition(this);
            ContainerBorder.Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(ContainerBorder);
            e.Handled = true;
        }
    }

    private void Container_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPanning)
        {
            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _lastPanPosition;

            // Compensate for zoom level
            _panTransform.X += delta.X / _currentZoom;
            _panTransform.Y += delta.Y * -1.0 / _currentZoom; // Invert Y because of the flipped coordinate system

            _lastPanPosition = currentPosition;
            e.Handled = true;
        }
    }

    private void Container_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            ContainerBorder.Cursor = new Cursor(StandardCursorType.Arrow);
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void Container_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            ContainerBorder.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    #endregion
}