using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using SiGen.Layouts.Elements;
using SiGen.Layouts;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiGen.UI.LayoutViewer;

public partial class LayoutViewer : UserControl
{

    public static readonly StyledProperty<LayoutViewMode> ViewModeProperty =
        AvaloniaProperty.Register<LayoutViewer, LayoutViewMode>(nameof(ViewMode));

    public static readonly StyledProperty<LayoutOrientation> OrientationProperty =
        AvaloniaProperty.Register<LayoutViewer, LayoutOrientation>(nameof(Orientation), LayoutOrientation.HorizontalNutRight);

    public static readonly StyledProperty<StringedInstrumentLayout?> LayoutProperty =
        AvaloniaProperty.Register<LayoutViewer, StringedInstrumentLayout?>(nameof(Layout));

    public LayoutViewMode ViewMode
    {
        get => GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public LayoutOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public StringedInstrumentLayout? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    private TranslateTransform _centerTransform;
    private ScaleTransform _scaleTransform;
    private TranslateTransform _translateTransform;
    private RotateTransform _orientationTransform;
    private double zoomToFit = 1;

    public LayoutViewer()
    {
        InitializeComponent();

        _scaleTransform = new ScaleTransform(1, -1);
        _translateTransform = new TranslateTransform(0, 0);
        _centerTransform = new TranslateTransform(0, 0);
        _orientationTransform = new RotateTransform(Orientation switch
        {
            LayoutOrientation.HorizontalNutRight => 90,
            LayoutOrientation.HorizontalNutLeft => 270,
            _ => 0
        });

        _orientationTransform.Bind(
            RotateTransform.AngleProperty,
            this.GetObservable(OrientationProperty)
                .Select(o => o switch
                {
                    LayoutOrientation.HorizontalNutRight => 90.0,
                    LayoutOrientation.HorizontalNutLeft => 270.0,
                    _ => 0.0
                })
        );

        var transformGroup = new TransformGroup
        {
            Children = { _centerTransform, _scaleTransform, _orientationTransform, _translateTransform }
        };
        RenderCanvas.RenderTransform = transformGroup;
        OverlayCanvas.RenderTransform = new TransformGroup
        {
            Children = { _centerTransform, _translateTransform }
        };
       
        BorderContainer.AddHandler(Gestures.PinchEvent, Canvas_PinchGesture, handledEventsToo: true);
        //BlueprintRect.SetBluePrintSize(new Size(200, 200));

        
        //BlueprintRect.UnitMode = UnitMode.Imperial;
        Button1.Click += Button1_Click;
        Button2.Click += (s, e) =>
        {
            Orientation = (LayoutOrientation)(((int)Orientation + 1) % Enum.GetValues(typeof(LayoutOrientation)).Length);
        };
    }

    #region Transforms

    #endregion

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    private void Button1_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RebuildLayout();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LayoutProperty)
        {
            RebuildLayout();
            isZoomToFit = true;
            CalculateZoomToFit();
            _scaleTransform.ScaleX = zoomToFit;
            _scaleTransform.ScaleY = zoomToFit * -1;
        }
        else if (change.Property == OrientationProperty)
        {
            //_orientationTransform.Angle = Orientation switch
            //{
            //    LayoutOrientation.HorizontalNutRight => 90,
            //    LayoutOrientation.HorizontalNutLeft => 270,
            //    _ => 0
            //};
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        if (!double.IsNaN(e.NewSize.Width) && !double.IsNaN(e.NewSize.Height))
        {
            _centerTransform.X = e.NewSize.Width / 2d;
            _centerTransform.Y = e.NewSize.Height / 2d;
        }

        if (isZoomToFit)
        {
            CalculateZoomToFit();
            _scaleTransform.ScaleX = zoomToFit;
            _scaleTransform.ScaleY = zoomToFit * -1;
        }

        base.OnSizeChanged(e);
    }

    #region Canvas Panning/Zooming

    private const double ZoomFactorStep = 1.1;
    private Point _lastMousePosition;
    private bool _isPanning;
    private bool isZoomToFit = true;

    public void ZoomAtCenter(double scaleFactor)
    {
        _translateTransform.X -= _translateTransform.X * (scaleFactor - 1);
        _translateTransform.Y -= _translateTransform.Y * (scaleFactor - 1);

        _scaleTransform.ScaleX *= scaleFactor;
        _scaleTransform.ScaleY *= scaleFactor;
        BlueprintRect.Zoom = _scaleTransform.ScaleX;
        isZoomToFit = false;
    }

    public void ZoomAtPoint(Point origin, double scaleFactor)
    {
        var centeredPoint = new Point(origin.X - _centerTransform.X, origin.Y - _centerTransform.Y);

        _translateTransform.X -= (centeredPoint.X - _translateTransform.X) * (scaleFactor - 1);
        _translateTransform.Y -= (centeredPoint.Y - _translateTransform.Y) * (scaleFactor - 1);

        _scaleTransform.ScaleX *= scaleFactor;
        _scaleTransform.ScaleY *= scaleFactor;
        BlueprintRect.Zoom = _scaleTransform.ScaleX;
        isZoomToFit = false;
        OnTranslateRender();
    }

    public void Recenter()
    {
        isZoomToFit = true;
        _scaleTransform.ScaleX = zoomToFit;
        _scaleTransform.ScaleY = zoomToFit * -1;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
        BlueprintRect.Zoom = 1;
        OnTranslateRender();
    }

    protected void OnTranslateRender()
    {
        DebugText.Text = $"Translate: ({_translateTransform.X:0.##}, {_translateTransform.Y:0.##})";
    }

    #region Mouse Handling

    public void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(RenderCanvas);
        if (point.Pointer.Type != PointerType.Mouse || point.Properties.IsLeftButtonPressed)
        {
            _lastMousePosition = e.GetPosition(this);
            _isPanning = true;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Hand);
        }
    }

    public void Canvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPanning)
        {
            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _lastMousePosition;
            _translateTransform.X += delta.X;
            _translateTransform.Y += delta.Y;
            isZoomToFit = false;
            OnTranslateRender();
            _lastMousePosition = currentPosition;
        }
    }

    public void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    public void Canvas_OnPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        double factor = e.Delta.Y > 0 ? ZoomFactorStep : 1 / ZoomFactorStep;
        ZoomAtPoint(e.GetPosition(this), factor);
    }

    #endregion

    private void Canvas_PinchGesture(object? sender, PinchEventArgs e)
    {
        // e.Scale gives the scale delta since last event
        var center = e.ScaleOrigin;
        var scaleFactor = e.Scale;

        ZoomAtPoint(center, scaleFactor);
    }

    #endregion

    #region MyRegion

    internal const double CmScaleFactor = 37.7952755906; // 1 cm in pixels at 96 DPI

    private void CalculateZoomToFit()
    {
        if (Layout == null)
        {
            zoomToFit = 1;
            return;
        }

        var layoutBounds = Layout.Bounds!;
        var availableSize = new Size(Bounds.Width - 30, Bounds.Height - 30);
        if (Orientation == LayoutOrientation.Vertical)
        {
            zoomToFit = (double)MathD.Min(
                availableSize.Height / (layoutBounds.Height.NormalizedValue * CmScaleFactor),
                availableSize.Width / (layoutBounds.Width.NormalizedValue * CmScaleFactor)
            );
        }
        else
        {
            zoomToFit = (double)MathD.Min(
                availableSize.Height / (layoutBounds.Width.NormalizedValue * CmScaleFactor),
                availableSize.Width / (layoutBounds.Height.NormalizedValue * CmScaleFactor)
            );
        }
    }

    private void RebuildLayout()
    {
        RenderCanvas.Children.Clear();
        OverlayCanvas.Children.Clear();

        if (Layout == null) return;


        Layout.CalculateBounds();
        var layoutBounds = Layout.Bounds!;

      
        BlueprintRect.LayoutBounds = layoutBounds;
        RenderCanvas.Children.Add(BlueprintRect);

        //RenderCanvas.Children.Add(new Line
        //{
        //    Stroke = new SolidColorBrush(Color.FromArgb(120, 0, 255, 0)),
        //    StrokeThickness = 2,
        //    StartPoint = Convert(new PointM(Measuring.Measure.Zero, layoutBounds.Top).ToVector()),
        //    EndPoint = Convert(new PointM(Measuring.Measure.Zero, layoutBounds.Bottom).ToVector()),
        //});

        //var fretElem = Layout.Elements.OfType<FretSegmentElement>().FirstOrDefault(x => x.FretIndex == 1);
        //PlaceOverlayText("Fret 1", fretElem!.FretShape!.GetFirstPoint(), Colors.White, new Point(0, -15));

        foreach (var @string in Layout.Elements.OfType<StringMedianElement>())
        {
            RenderCanvas.Children.Add(new Line
            {
                Stroke = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)),
                StrokeThickness = 1,
                StartPoint = Convert(@string.Path.Start),
                EndPoint = Convert(@string.Path.End),
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 8, 4,2,4 }
            });
        }

        var fretElements = Layout.Elements.OfType<FretSegmentElement>().ToList();
        double fretThickness = MeasureToPixels(SiGen.Measuring.Measure.Mm(2));


        foreach (var fret in fretElements)
        {
            var adjustedShape = fret.FretShape?.Extend(0.25);
            if (adjustedShape == null)
                continue;


            var clipGeom = GetFretClipGeom(fret);

            //if (fret.FretIndex == 12)
            //{

            //    var fretPt = Convert(fret.FretShape!.GetFirstPoint());
            //    RenderCanvas.Children.Add(new Path
            //    {
            //        Data = clipGeom,
            //        Fill = Brushes.LightGray,
            //        //RenderTransform = new TranslateTransform(fretPt.X, fretPt.Y)
            //    });
            //    isFirst = false;
            //}
            var color = fret.Segment.IsNut() ? Brushes.Brown : Brushes.Silver;
            
            if (adjustedShape is LinearPath linearPath)
            {
                RenderCanvas.Children.Add(new Line
                {
                    Stroke = color,
                    StrokeThickness = fret.IsNut || fret.IsBridge ? 2 : fretThickness,
                    StartPoint = Convert(linearPath.Start),
                    EndPoint = Convert(linearPath.End),
                    Clip = clipGeom,

                });
            }
            else if (adjustedShape is PolyLinePath polyLine)
            {
                RenderCanvas.Children.Add(new Polyline
                {
                    Stroke = color,
                    StrokeThickness = fretThickness,
                    Points = polyLine.Points.Select(Convert).ToArray()
                });
            }
        }
        foreach (var edge in Layout.Elements.OfType<FingerboardSideElement>())
        {
            RenderCanvas.Children.Add(new Line
            {
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                StartPoint = Convert(edge.Path.Start),
                EndPoint = Convert(edge.Path.End)
            });
        }

        var stringsElements = Layout.Strings;
        foreach (var @string in stringsElements)
        {
            if (@string.Path is LinearPath linearPath)
            {
                //double stringThickness = @string.Configuration.Gauge != null ? MeasureToPixels(@string.Configuration.Gauge) : 6;
                RenderCanvas.Children.Add(new StringVisualElement(@string));
                //RenderCanvas.Children.Add(new Line
                //{
                //    Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                //    StrokeThickness = stringThickness,
                //    StartPoint = Convert(linearPath.Start) + new Point(4, 0),
                //    EndPoint = Convert(linearPath.End) + new Point(4, 0)
                //});
                //RenderCanvas.Children.Add(new Line
                //{
                //    Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 210)),
                //    StrokeThickness = stringThickness,
                //    StartPoint = Convert(linearPath.Start),
                //    EndPoint = Convert(linearPath.End)
                //});
                //RenderCanvas.Children.Add(new Line
                //{
                //    Stroke = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)),
                //    StrokeThickness = stringThickness * 0.4,
                //    StartPoint = Convert(linearPath.Start) + new Point(stringThickness * 0.1, 0),
                //    EndPoint = Convert(linearPath.End) + new Point(stringThickness * 0.1, 0),
                //    //Effect = new BlurEffect
                //    //{
                //    //    Radius = stringThickness * 0.25,
                //    //}
                //});
            }
        }
    }

    private void PlaceOverlayText(string text, VectorD position, Color color, Point offset = new Point())
    {
        var overlayText = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(color),
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
        overlayText.Measure(Size.Infinity);
        var screenPt = GetOverlayPoint(position);
        OverlayCanvas.Children.Add(overlayText);
        Canvas.SetLeft(overlayText, screenPt.X - (overlayText.DesiredSize.Width / 2d) + offset.X);
        Canvas.SetTop(overlayText, screenPt.Y - (overlayText.DesiredSize.Height / 2d) + offset.Y);
    }

    //todo: cache clip geometries by covered string range
    private Geometry? GetFretClipGeom(FretSegmentElement element)
    {
        var bassLine = element.GetEdgePath(Layouts.Data.FingerboardSide.Bass);
        var trebleLine = element.GetEdgePath(Layouts.Data.FingerboardSide.Treble);
        if (bassLine == null || trebleLine == null || element.FretShape == null)
            return null;
        
        var bp1 = bassLine.GetPointForY(Layout!.Bounds!.Top.NormalizedValue + 1);
        var bp2 = bassLine.GetPointForY(Layout!.Bounds!.Bottom.NormalizedValue - 1);
        var tp1 = trebleLine.GetPointForY(Layout!.Bounds!.Top.NormalizedValue + 1);
        var tp2 = trebleLine.GetPointForY(Layout!.Bounds!.Bottom.NormalizedValue - 1);

        var pathGeometry = new PathGeometry();
        var ctx = pathGeometry.Open();
        ctx.BeginFigure(Convert(bp2), true);
        ctx.LineTo(Convert(bp1));
        ctx.LineTo(Convert(tp1));
        ctx.LineTo(Convert(tp2));
        ctx.EndFigure(true);

        pathGeometry.Transform = new TranslateTransform(0.001, 0.001);
        return pathGeometry;
    }

    private Point Convert(VectorD point)
    {
        return new Point((double)point.X * CmScaleFactor, (double)point.Y * CmScaleFactor);
    }

    private double MeasureToPixels(Measure measure)
    {
        return (double)measure.NormalizedValue * CmScaleFactor;
    }

    #endregion

    #region Overlay

    private Point GetOverlayPoint(VectorD point)
    {
        var zoom = _scaleTransform.ScaleX;


        if (Orientation == LayoutOrientation.HorizontalNutRight)
        {
            return new Point((double)point.Y * CmScaleFactor * zoom, (double)point.X * CmScaleFactor * zoom);
        }

        if (Orientation == LayoutOrientation.HorizontalNutLeft)
        {
            return new Point(-(double)point.Y * CmScaleFactor * zoom, -(double)point.X * CmScaleFactor * zoom);
        }

        return new Point((double)point.X * CmScaleFactor * zoom, -(double)point.Y * CmScaleFactor * zoom);
    }

    private Point GetOverlayPoint(PointM point)
    {
        return GetOverlayPoint(point.ToVector());
    }

    #endregion
}