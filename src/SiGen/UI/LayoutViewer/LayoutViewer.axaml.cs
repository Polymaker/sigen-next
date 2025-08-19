using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.UI.LayoutViewer.Overlays;
using SiGen.UI.LayoutViewer.Visuals;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiGen.UI.LayoutViewer;

public partial class LayoutViewer : UserControl
{
    public static readonly StyledProperty<LayoutOrientation> OrientationProperty =
        AvaloniaProperty.Register<LayoutViewer, LayoutOrientation>(nameof(Orientation), LayoutOrientation.HorizontalNutRight);

    public static readonly StyledProperty<StringedInstrumentLayout?> LayoutProperty =
        AvaloniaProperty.Register<LayoutViewer, StringedInstrumentLayout?>(nameof(Layout));

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
    private ScaleTransform _zoomTransform;
    private TranslateTransform _translateTransform;
    private RotateTransform _orientationTransform;

    private double zoomToFit = 1;

    public LayoutViewer()
    {
        InitializeComponent();

        _zoomTransform = new ScaleTransform(1, -1);
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
            Children = { _centerTransform, _zoomTransform, _orientationTransform, _translateTransform }
        };
        RenderCanvas.RenderTransform = transformGroup;
        OverlayCanvas.RenderTransform = new TransformGroup
        {
            Children = { _centerTransform, _translateTransform }
        };
       
        BorderContainer.AddHandler(Gestures.PinchEvent, Canvas_PinchGesture, handledEventsToo: true);

        
        //BlueprintRect.UnitMode = UnitMode.Imperial;
        Button1.Click += Button1_Click;
        Button2.Click += (s, e) =>
        {
            Orientation = (LayoutOrientation)(((int)Orientation + 1) % Enum.GetValues(typeof(LayoutOrientation)).Length);
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    private void Button1_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CreateLayoutVisualsAndOverlays();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LayoutProperty)
        {
            CreateLayoutVisualsAndOverlays();
            
            CalculateZoomToFit();

            if (isZoomToFit)
                SetZoom(zoomToFit);
            else if (Layout != null)
                ClampZoom();
        }
        else if (change.Property == OrientationProperty)
        {
            ResetZoomAndTranslation();
            //RepositionOverlays();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (!double.IsNaN(e.NewSize.Width) && !double.IsNaN(e.NewSize.Height))
        {
            _centerTransform.X = e.NewSize.Width / 2d;
            _centerTransform.Y = e.NewSize.Height / 2d;
        }

        CalculateZoomToFit();
        if (isZoomToFit)
            SetZoom(zoomToFit);
        else if (Layout != null)
            ClampZoom();
    }

    #region Zoom Handling

    private const double ZoomFactorStep = 1.1;
    private Point _lastMousePosition;
    private bool _isPanning;
    private bool isZoomToFit = true;
    private double minimumZoom = 0.5;
    private double maximumZoom = 2.0;

    private bool SetZoom(double amount)
    {
        double oldZoom = _zoomTransform.ScaleX;
        _zoomTransform.ScaleX = Math.Clamp(amount, minimumZoom, maximumZoom);
        _zoomTransform.ScaleY = Math.Clamp(amount, minimumZoom, maximumZoom) * -1d;
        if (Math.Abs(_zoomTransform.ScaleX - oldZoom) > 0.001)
        {
            OnZoomChanged();
            return true;
        }
        return false;
    }

    private bool AdjustZoom(double delta)
    {
        double oldZoom = _zoomTransform.ScaleX;
        _zoomTransform.ScaleX = Math.Clamp(oldZoom * delta, minimumZoom, maximumZoom);
        _zoomTransform.ScaleY = Math.Clamp(oldZoom * delta, minimumZoom, maximumZoom) * -1d;
        if (Math.Abs(_zoomTransform.ScaleX - oldZoom) > 0.001)
        {
            OnZoomChanged();
            return true;
        }
        return false;
    }

    public void ZoomAtCenter(double zoomDelta)
    {
        ZoomAtPoint(new Point(_centerTransform.X, _centerTransform.Y), zoomDelta);
    }

    public void ZoomAtPoint(Point origin, double zoomDelta)
    {
        var centeredPoint = new Point(origin.X - _centerTransform.X, origin.Y - _centerTransform.Y);

        var targetTrans = new Point(
            _translateTransform.X - (centeredPoint.X - _translateTransform.X) * (zoomDelta - 1),
            _translateTransform.Y - (centeredPoint.Y - _translateTransform.Y) * (zoomDelta - 1)
        );

        var transDelta = targetTrans - new Point(_translateTransform.X, _translateTransform.Y);

        if (AdjustZoom(zoomDelta))
        {
            isZoomToFit = false;
            TranslateView(transDelta);
        }
    }

    public void ResetZoomAndTranslation()
    {
        isZoomToFit = true;
        bool zoomChanged = SetZoom(zoomToFit);
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
        OnTranslationChanged();
        if (!zoomChanged)
            OnZoomChanged();
    }

    private void CalculateZoomToFit()
    {
        if (Layout == null)
        {
            zoomToFit = 1;
            minimumZoom = 0.5;
            maximumZoom = 2;
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

        minimumZoom = zoomToFit * 0.95;
        maximumZoom = 10;
    }

    private void ClampZoom()
    {
        double oldZoom = _zoomTransform.ScaleX;
        _zoomTransform.ScaleX = Math.Clamp(_zoomTransform.ScaleX, minimumZoom, maximumZoom);
        _zoomTransform.ScaleY = Math.Clamp(Math.Abs(_zoomTransform.ScaleY), minimumZoom, maximumZoom) * -1d;
        if (Math.Abs(_zoomTransform.ScaleX - oldZoom) > 0.001)
            OnZoomChanged();
    }

    protected void OnZoomChanged()
    {
        BlueprintRect.Zoom = _zoomTransform.ScaleX;
        RepositionOverlays();
    }

    #endregion

    #region Mouse / Touch Handling

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
            TranslateView(delta);
            //_translateTransform.X += delta.X;
            //_translateTransform.Y += delta.Y;
            isZoomToFit = false;
            OnTranslationChanged();
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

    private void Canvas_PinchGesture(object? sender, PinchEventArgs e)
    {
        // e.Scale gives the scale delta since last event
        var center = e.ScaleOrigin;
        var scaleFactor = e.Scale;

        ZoomAtPoint(center, scaleFactor);
    }

    #endregion

    #region Translation

    protected Rect GetAdjustedLayoutBounds()
    {
        if (Layout?.Bounds == null)
            return new Rect();

        var layoutBoundsPx = Layout.Bounds.ToAvalonia();


        if (Orientation == LayoutOrientation.HorizontalNutRight)
        {
            return new Rect(layoutBoundsPx.Top, layoutBoundsPx.Left, layoutBoundsPx.Height, layoutBoundsPx.Width);
        }
        else if (Orientation == LayoutOrientation.HorizontalNutLeft)
        {
            return new Rect(layoutBoundsPx.Top * -1d, layoutBoundsPx.Right, layoutBoundsPx.Height, layoutBoundsPx.Width);
        }

        return layoutBoundsPx;
    }

    //private void SetTranslation(Point translation)
    //{
    //    if (Layout?.Bounds == null)
    //        return;

    //    double zoom = _zoomTransform.ScaleX;
    //    var layoutBoundsPx = GetAdjustedLayoutBounds() * zoom;
    //    double layoutWidth = layoutBoundsPx.Width;
    //    double layoutHeight = layoutBoundsPx.Height;
    //    double viewWidth = Bounds.Width;
    //    double viewHeight = Bounds.Height;
    //    double minX, maxX, minY, maxY;
    //    // X axis
    //    if (layoutWidth <= viewWidth)
    //    {
    //        minX = (viewWidth - layoutWidth) * -0.5d;
    //        maxX = -minX;
    //    }
    //    else
    //    {
    //        double visibleView = viewWidth * 0.75;
    //        minX = (layoutWidth + viewWidth) * -0.5d + visibleView;
    //        maxX = -minX;
    //    }
    //    // Y axis
    //    if (layoutHeight <= viewHeight)
    //    {
    //        minY = (viewHeight - layoutHeight) * -0.5d;
    //        maxY = -minY;
    //    }
    //    else
    //    {
    //        double visibleView = viewHeight * 0.75d;
    //        minY = (layoutHeight + viewHeight) * -0.5d + visibleView;
    //        maxY = -minY;
    //    }
    //    _translateTransform.X = Math.Clamp(translation.X, minX, maxX);
    //    _translateTransform.Y = Math.Clamp(translation.Y, minY, maxY);
    //}

    public void TranslateView(Point delta)
    {
        if (Layout?.Bounds == null)
            return;

        double zoom = _zoomTransform.ScaleX;
        var layoutBoundsPx = GetAdjustedLayoutBounds() * zoom;
        var viewerSize = Bounds;

        double layoutWidth = layoutBoundsPx.Width;
        double layoutHeight = layoutBoundsPx.Height;
        double viewWidth = viewerSize.Width;
        double viewHeight = viewerSize.Height;

        double minX, maxX, minY, maxY;

        //todo: ease the transition between the two modes

        // X axis
        if (layoutWidth <= viewWidth)
        {
            minX = (viewWidth - layoutWidth * 0.8d) * -0.5d;
            maxX = -minX;
        }
        else
        {
            double visibleView = viewWidth * 0.75;
            minX = (layoutWidth + viewWidth) * -0.5d + visibleView;
            maxX = -minX;
        }

        // Y axis
        if (layoutHeight <= viewHeight)
        {
            minY = (viewHeight - layoutHeight * 0.8d) * -0.5d;
            maxY = -minY;
        }
        else
        {
            double visibleView = viewHeight * 0.75d;
            minY = (layoutHeight + viewHeight) * -0.5d + visibleView;
            maxY = -minY;
        }

        var newTranslation = new Point(
            Math.Clamp(_translateTransform.X + delta.X, minX, maxX),
            Math.Clamp(_translateTransform.Y + delta.Y, minY, maxY)
        );

        if (newTranslation.X != _translateTransform.X || newTranslation.Y != _translateTransform.Y)
        {
            _translateTransform.X = newTranslation.X;
            _translateTransform.Y = newTranslation.Y;
            OnTranslationChanged();
        }
    }

    protected void OnTranslationChanged()
    {
        //DebugText.Text = $"Translate: ({_translateTransform.X:0.##}, {_translateTransform.Y:0.##})";
    }

    #endregion

    #region Visuals and Overlays

    internal const double CmScaleFactor = 37.7952755906; // 1 cm in pixels at 96 DPI

    private void CreateLayoutVisualsAndOverlays()
    {
        RenderCanvas.Children.Clear();
        OverlayCanvas.Children.Clear();

        if (Layout == null) return;


        Layout.CalculateBounds();
        var layoutBounds = Layout.Bounds!;

      
        BlueprintRect.LayoutBounds = layoutBounds;
        RenderCanvas.Children.Add(BlueprintRect);

        GenerateOverlays();
       
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

        foreach (var fretSegment in Layout.Elements.OfType<FretSegmentElement>())
            RenderCanvas.Children.Add(new FretSegmentVisual(fretSegment));

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

        foreach (var @string in Layout.Strings)
            RenderCanvas.Children.Add(new StringVisualElement(@string));
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

    #region Overlays

    private class OverlayPositionHelper : IOverlayPositionHelper
    {
        public double Zoom { get; }
        public LayoutOrientation ViewerOrientation { get; }

        public StringedInstrumentLayout Layout { get; }

        public OverlayPositionHelper(StringedInstrumentLayout layout, double zoom, LayoutOrientation viewerOrientation)
        {
            Layout = layout;
            Zoom = zoom;
            ViewerOrientation = viewerOrientation;
        }

        public Point VectorToScreen(VectorD point)
        {
            if (ViewerOrientation == LayoutOrientation.HorizontalNutRight)
            {
                return new Point((double)point.Y * CmScaleFactor * Zoom, (double)point.X * CmScaleFactor * Zoom);
            }

            if (ViewerOrientation == LayoutOrientation.HorizontalNutLeft)
            {
                return new Point(-(double)point.Y * CmScaleFactor * Zoom, -(double)point.X * CmScaleFactor * Zoom);
            }

            return new Point((double)point.X * CmScaleFactor * Zoom, -(double)point.Y * CmScaleFactor * Zoom);
        }
    }

    private void GenerateOverlays()
    {
        if (Layout == null) return;

        var helper = new OverlayPositionHelper(Layout, _zoomTransform.ScaleX, Orientation);

        FretNumberOverlay.CreateElements(helper, OverlayCanvas);

        //RepositionOverlays();
    }

    private void RepositionOverlays()
    {
        if (Layout == null) return;
        var helper = new OverlayPositionHelper(Layout, _zoomTransform.ScaleX, Orientation);
        foreach (var overlay in OverlayCanvas.Children.OfType<ILayoutOverlay>())
            overlay.Reposition(helper);
    }

    #endregion
}