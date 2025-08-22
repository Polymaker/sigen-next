using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Settings; 
using SiGen.UI.LayoutViewer.Overlays;
using SiGen.UI.LayoutViewer.Visuals;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace SiGen.UI.LayoutViewer;

public partial class LayoutViewerControl : UserControl, ILayoutViewerContext
{
    public static readonly StyledProperty<LayoutOrientation> OrientationProperty =
        AvaloniaProperty.Register<LayoutViewerControl, LayoutOrientation>(nameof(Orientation), LayoutOrientation.HorizontalNutRight);

    public static readonly StyledProperty<double> ZoomProperty =
       AvaloniaProperty.Register<LayoutViewerControl, double>(nameof(Zoom), 1d, coerce: CoerceZoomValue);

    public static readonly StyledProperty<Point> TranslationProperty =
       AvaloniaProperty.Register<LayoutViewerControl, Point>(nameof(Translation), new Point(), coerce: CoerceTranslationValue);

    public static readonly StyledProperty<StringedInstrumentLayout?> LayoutProperty =
        AvaloniaProperty.Register<LayoutViewerControl, StringedInstrumentLayout?>(nameof(Layout));

    // Theme property
    private ThemeRenderSettings _renderSettings = ThemeRenderSettings.Blueprint;

    public ThemeRenderSettings RenderSettings
    {
        get => _renderSettings;
        set
        {
            if (_renderSettings != value)
            {
                _renderSettings = value;
                OnRenderSettingsChanged();
            }
        }
    }

    public event EventHandler? RenderSettingsChanged;

    public LayoutOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public Point Translation
    {
        get => GetValue(TranslationProperty);
        set => SetValue(TranslationProperty, value);
    }

    public StringedInstrumentLayout? Layout
    {
        get => GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    public static readonly StyledProperty<UnitMode> UnitModeProperty =
        AvaloniaProperty.Register<LayoutViewerControl, UnitMode>(nameof(UnitMode), UnitMode.Metric);

    public UnitMode UnitMode
    {
        get => GetValue(UnitModeProperty);
        set => SetValue(UnitModeProperty, value);
    }

    private TranslateTransform _centerTransform;
    private ScaleTransform _zoomTransform;
    private TranslateTransform _translateTransform;
    private RotateTransform _orientationTransform;

    
    public bool IsAssigningLayout { get; set; }

    public LayoutViewerControl()
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
        BorderContainer.AddHandler(Gestures.PinchEndedEvent, Canvas_PinchGestureEnded, handledEventsToo: true);


        Button1.Click += Button1_Click;
        Button2.Click += (s, e) =>
        {
            Orientation = (LayoutOrientation)(((int)Orientation + 1) % Enum.GetValues(typeof(LayoutOrientation)).Length);
        };
        Button3.Click += (s, e) =>
        {
            UnitMode = (UnitMode)(((int)UnitMode + 1) % 2);
        };
        OnRenderSettingsChanged();
        LayoutGrid.SetBluePrintBounds(new RectangleM(Measuring.Measure.Cm(-5), Measuring.Measure.Cm(-5), Measuring.Measure.Cm(10), Measuring.Measure.Cm(10)));

        zoomEndTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        zoomEndTimer.Tick += ZoomEndTimer_Tick;

        fretNumberOverlay = new FretNumberOverlayControl(this);
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UnitModeProperty)
        {
            LayoutGrid.UnitMode = UnitMode;
        }
        else if (change.Property == ZoomProperty)
        {
            CalculateTranslationBounds();

            double newZoom = change.GetNewValue<double>();

            _zoomTransform.ScaleX = newZoom;
            _zoomTransform.ScaleY = newZoom * -1d;

            if (Math.Abs(fitToViewZoom - newZoom) > 0.001 && Translation.X == 0 && Translation.Y == 0)
                isZoomToFit = false;

            OnZoomChanged();
        }
        else if (change.Property == TranslationProperty)
        {
            _translateTransform.X = change.GetNewValue<Point>().X;
            _translateTransform.Y = change.GetNewValue<Point>().Y;

            if (Math.Abs(fitToViewZoom - Zoom) > 0.001 && Translation.X == 0 && Translation.Y == 0)
                isZoomToFit = false;

            OnTranslationChanged();
        }

        if (change.Property == LayoutProperty)
        {
            StopFling();

            if (!hasLoaded)
                return;

            CalculateTranslationBounds();

            CreateLayoutVisualsAndOverlays();
            
            CalculateZoomToFit();

            if (isZoomToFit)
            {
                isZoomToFit = true;
                IsAssigningLayout = false;
                Zoom = fitToViewZoom;
            }
            else if (Layout != null)
            {
                Zoom = ClampZoom(Zoom); // Ensure zoom is within bounds
            }
        }
        else if (change.Property == OrientationProperty)
        {
            CalculateZoomToFit();
            ResetZoomAndTranslation();
            RepositionOverlays();
        }
    }
    
    private bool hasLoaded = false;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        hasLoaded = true;

        if (Layout != null)
        {
            CalculateTranslationBounds();
            CreateLayoutVisualsAndOverlays();
            CalculateZoomToFit();
            if (isZoomToFit)
            {
                Zoom = fitToViewZoom;
            }
            else
            {
                Zoom = ClampZoom(Zoom); // Ensure zoom is within bounds
            }
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
        {
            Zoom = fitToViewZoom;
        }
        else if (Layout != null)
        {
            Zoom = ClampZoom(Zoom); // Ensure zoom is within bounds
        }
            
    }

    #region Zoom Handling

    private const double ZoomFactorStep = 1.1;
    private Point _lastMousePosition;
    private bool _isPanning;
    private bool isZoomToFit = true;
    private bool isZooming = false;
    private double minimumZoom = 0.5;
    private double maximumZoom = 2.0;
    private bool internalZoomChange = false;
    private double fitToViewZoom = 1;
    private DispatcherTimer zoomEndTimer;

    private static double CoerceZoomValue(AvaloniaObject sender, double value)
    {
        var viewer = sender as LayoutViewerControl;
        if (viewer == null)
            return value;

        if (viewer.internalZoomChange || viewer.IsAssigningLayout)
            return value;

        return Math.Clamp(value, viewer.minimumZoom, viewer.maximumZoom);
    }

    private bool AdjustZoom(double delta)
    {
        double oldZoom = Zoom;

        //_zoomTransform.ScaleX = Math.Clamp(oldZoom * delta, minimumZoom, maximumZoom);
        //_zoomTransform.ScaleY = Math.Clamp(oldZoom * delta, minimumZoom, maximumZoom) * -1d;

        internalZoomChange = true; // Prevents recursive calls to SetZoom from OnZoomChanged
        Zoom = Math.Clamp(oldZoom * delta, minimumZoom, maximumZoom); // Update the Zoom property
        internalZoomChange = false; // Reset after setting the Zoom property

        //if (Math.Abs(Zoom - oldZoom) > 0.001)
        //{
        //    OnZoomChanged();
        //    return true;
        //}
        return Math.Abs(Zoom - oldZoom) > 0.001;
    }

    public void ZoomAtCenter(double zoomDelta)
    {
        ZoomAtPoint(new Point(_centerTransform.X, _centerTransform.Y), zoomDelta);
    }

    public void ZoomAtPoint(Point origin, double zoomDelta)
    {
        var centeredPoint = new Point(origin.X - _centerTransform.X, origin.Y - _centerTransform.Y);

        var targetTrans = new Point(
            Translation.X - (centeredPoint.X - Translation.X) * (zoomDelta - 1),
            Translation.Y - (centeredPoint.Y - Translation.Y) * (zoomDelta - 1)
        );

        var transDelta = targetTrans - new Point(Translation.X, Translation.Y);

        if (AdjustZoom(zoomDelta))
        {
            if (!isZooming)
            {
                isZooming = true;
                OnZoomBegin();
            }
            zoomEndTimer.Stop();
            zoomEndTimer.Start();

            //isZoomToFit = false;
            Translation += transDelta; // Apply translation change after zooming
        }
    }

    public void ResetZoomAndTranslation()
    {
        isZoomToFit = true;
        Zoom = fitToViewZoom;
        Translation = new Point(0, 0);
    }

    private void CalculateZoomToFit()
    {
        if (Layout == null || Layout.Bounds == null || Bounds.Width == 0 || Bounds.Height == 0)
        {
            fitToViewZoom = 1;
            minimumZoom = 0.5;
            maximumZoom = 2;
            return;
        }

        var layoutBounds = Layout.Bounds!;
        var availableSize = new Size(Bounds.Width - 30, Bounds.Height - 30);
        if (Orientation == LayoutOrientation.Vertical)
        {
            fitToViewZoom = (double)MathD.Min(
                availableSize.Height / (layoutBounds.Height.NormalizedValue * CmScaleFactor),
                availableSize.Width / (layoutBounds.Width.NormalizedValue * CmScaleFactor)
            );
        }
        else
        {
            fitToViewZoom = (double)MathD.Min(
                availableSize.Height / (layoutBounds.Width.NormalizedValue * CmScaleFactor),
                availableSize.Width / (layoutBounds.Height.NormalizedValue * CmScaleFactor)
            );
        }

        minimumZoom = fitToViewZoom * 0.95;
        maximumZoom = 10;
    }


    private double ClampZoom(double value)
    {
        return Math.Clamp(value, minimumZoom, maximumZoom);
    }

    private void OnZoomBegin()
    {
        foreach (var visual in RenderCanvas.Children.OfType<INotifyZoomChanged>())
            visual.BeginZoomChange();
    }

    private void OnZoomEnd()
    {
        RepositionOverlays();
        foreach (var visual in RenderCanvas.Children.OfType<INotifyZoomChanged>())
            visual.EndZoomChange();
    }

    protected void OnZoomChanged()
    {
        LayoutGrid.Zoom = Zoom;
        //DebugText.Text = $"Zoom: {Zoom:0.##}";
        //if (!isZooming)
            RepositionOverlays();

        ZoomTextBox.Text = $"{Zoom:0%}";
    }

    
    private void ZoomEndTimer_Tick(object? sender, EventArgs e)
    {
        zoomEndTimer.Stop();
        isZooming = false;
      
        OnZoomEnd();
        
  
    }

    #endregion

    #region Mouse / Touch Handling

    public void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        StopFling();

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

            AddPositionToFlingHistory(currentPosition);

            var delta = currentPosition - _lastMousePosition;
            Translation += delta;
            _lastMousePosition = currentPosition;
        }
    }

    public void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Arrow);

            var velocity = CalculateFlingVelocity();

            if (velocity.Length > MinimumFlingVelocity)
                StartFling(velocity);
        }
    }

    public void Canvas_OnPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        StopFling();

        double factor = e.Delta.Y > 0 ? ZoomFactorStep : 1 / ZoomFactorStep;
        ZoomAtPoint(e.GetPosition(this), factor);
    }

    private double lastPinchScale = 1.0;

    private void Canvas_PinchGesture(object? sender, PinchEventArgs e)
    {
        // e.Scale gives the scale delta since last event
        var center = e.ScaleOrigin;
        var scaleFactor = e.Scale;
        double scaleDelta = scaleFactor - lastPinchScale;

        if (Math.Abs(scaleDelta) < 0.01)
            return; // Ignore very small scale changes

        Trace.WriteLine($"e.Scale = {e.Scale} delta = {scaleDelta}");
        bool isZoomingIn = scaleDelta > 0;
        scaleDelta = isZoomingIn ? (scaleDelta + 1d) : 1d / (Math.Abs(scaleDelta) + 1d);
        
       

        lastPinchScale = e.Scale;
        
        ZoomAtPoint(center, scaleDelta);
    }

    private void Canvas_PinchGestureEnded(object? sender, PinchEndedEventArgs e)
    {
        lastPinchScale = 1;
    }

    #endregion

    #region Translation Handling

    private Rect translationBounds = new Rect();
    private IDisposable? _flingTimer;
    private readonly List<(Point position, long timestamp)> _pointerHistory = new();
    private const int FlingHistoryMs = 100; // Only consider last 100ms for velocity
    private const double FlingFriction = 0.92; // Friction per frame
    private const double MinimumFlingVelocity = 20; // px/sec
    private const int FlingTimeStepMs = 16; // ~60fps
    private Vector _flingVelocity;

    private void CalculateTranslationBounds()
    {
        if (Layout?.Bounds == null)
        {
            translationBounds = new Rect();
            return;
        }

        var layoutBoundsPx = GetAdjustedLayoutBounds() * Zoom;
        var viewerSize = Bounds;

        double layoutWidth = layoutBoundsPx.Width;
        double layoutHeight = layoutBoundsPx.Height;
        double viewWidth = viewerSize.Width;
        double viewHeight = viewerSize.Height;

        double minX, maxX, minY, maxY;

        // X axis
        if (layoutWidth <= viewWidth)
        {
            double visibleView = viewWidth * 0.75;
            minX = Math.Min((viewWidth - layoutWidth * 0.8d) * -0.5d, (layoutWidth + viewWidth) * -0.5d + visibleView);
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
            double visibleView = viewHeight * 0.75d;
            minY = Math.Min((viewHeight - layoutHeight * 0.8d) * -0.5d, (layoutHeight + viewHeight) * -0.5d + visibleView);
            maxY = -minY;
        }
        else
        {
            double visibleView = viewHeight * 0.75d;
            minY = (layoutHeight + viewHeight) * -0.5d + visibleView;
            maxY = -minY;
        }

        translationBounds = new Rect(
            minX, minY,
            maxX - minX, maxY - minY
        );
    }

    /// <summary>
    /// Returns the layout bounds adjusted for the current orientation
    /// </summary>
    /// <returns></returns>
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

    private static Point CoerceTranslationValue(AvaloniaObject sender, Point value)
    {
        var viewer = sender as LayoutViewerControl;
        if (viewer == null)
            return value;

        //if (viewer.internalZoomChange)
        //    return value;

        return viewer.ClampTranslation(value);
    }

    private Point ClampTranslation(Point translation)
    {
        return new Point(
            Math.Clamp(translation.X, translationBounds.Left, translationBounds.Right),
            Math.Clamp(translation.Y, translationBounds.Top, translationBounds.Bottom)
        );
    }

    public void TranslateView(Point delta)
    {
        if (Layout?.Bounds == null)
            return;

        //var layoutBoundsPx = GetAdjustedLayoutBounds() * Zoom;
        //var viewerSize = Bounds;

        //double layoutWidth = layoutBoundsPx.Width;
        //double layoutHeight = layoutBoundsPx.Height;
        //double viewWidth = viewerSize.Width;
        //double viewHeight = viewerSize.Height;

        //double minX, maxX, minY, maxY;

        ////todo: ease the transition between the two modes

        //// X axis
        //if (layoutWidth <= viewWidth)
        //{
        //    minX = (viewWidth - layoutWidth * 0.8d) * -0.5d;
        //    maxX = -minX;
        //}
        //else
        //{
        //    double visibleView = viewWidth * 0.75;
        //    minX = (layoutWidth + viewWidth) * -0.5d + visibleView;
        //    maxX = -minX;
        //}

        //// Y axis
        //if (layoutHeight <= viewHeight)
        //{
        //    minY = (viewHeight - layoutHeight * 0.8d) * -0.5d;
        //    maxY = -minY;
        //}
        //else
        //{
        //    double visibleView = viewHeight * 0.75d;
        //    minY = (layoutHeight + viewHeight) * -0.5d + visibleView;
        //    maxY = -minY;
        //}

        //var newTranslation = new Point(
        //    Math.Clamp(_translateTransform.X + delta.X, translationBounds.Left, translationBounds.Right),
        //    Math.Clamp(_translateTransform.Y + delta.Y, translationBounds.Top, translationBounds.Bottom)
        //);

        Translation = ClampTranslation(Translation + delta);

        //if (newTranslation.X != _translateTransform.X || newTranslation.Y != _translateTransform.Y)
        //{
        //    _translateTransform.X = newTranslation.X;
        //    _translateTransform.Y = newTranslation.Y;
        //    OnTranslationChanged();
        //}
    }

    protected void OnTranslationChanged()
    {
        //DebugText.Text = $"Translate: ({_translateTransform.X:0.##}, {_translateTransform.Y:0.##})";
    }

    private void AddPositionToFlingHistory(Point position)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _pointerHistory.Add((position, now));
        // Remove old entries
        _pointerHistory.RemoveAll(p => now - p.timestamp > FlingHistoryMs);
    }

    public Vector CalculateFlingVelocity()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _pointerHistory.RemoveAll(p => now - p.timestamp > FlingHistoryMs);

        if (_pointerHistory.Count < 2)
            return default;

        var first = _pointerHistory.First();
        var last = _pointerHistory.Last();

        var dt = (last.timestamp - first.timestamp) / 1000.0; // seconds
        if (dt <= 0)
            return default;

        var dx = last.position.X - first.position.X;
        var dy = last.position.Y - first.position.Y;

        // Velocity in pixels per second
        return new Vector(dx / dt, dy / dt);
    }

    private void StartFling(Vector velocity)
    {
        _flingVelocity = velocity;
        _flingTimer?.Dispose();
        _flingTimer = DispatcherTimer.Run(() =>
        {
            // Apply velocity to translation
            Translation = ClampTranslation(Translation + _flingVelocity * (FlingTimeStepMs / 1000.0));

            // Apply friction
            _flingVelocity *= FlingFriction;

            // Stop if velocity is low
            if (_flingVelocity.Length < MinimumFlingVelocity)
            {
                _flingTimer?.Dispose();
                _flingTimer = null;
                return false;
            }
            return true;
        }, TimeSpan.FromMilliseconds(FlingTimeStepMs));
    }

    private void StopFling()
    {
        _flingTimer?.Dispose();
        _flingTimer = null;
    }

    #endregion

    #region Visuals Elements

    internal const double CmScaleFactor = 37.7952755906; // 1 cm in pixels at 96 DPI

    private void CreateLayoutVisualsAndOverlays()
    {
        RenderCanvas.Children.Clear();
        OverlayCanvas.Children.Clear();

        if (Layout == null) return;
        
        var layoutBounds = Layout.Bounds!;

        LayoutGrid.LayoutBounds = layoutBounds;
        RenderCanvas.Children.Add(LayoutGrid);

        GenerateOverlays();

        foreach (var median in Layout.Elements.OfType<StringMedianElement>())
            RenderCanvas.Children.Add(new StringMedianVisualElement(median, RenderSettings));

        RenderCanvas.Children.Add(new FretRendererControl(this));
        //foreach (var fretSegment in Layout.Elements.OfType<FretSegmentElement>())
        //    RenderCanvas.Children.Add(new FretSegmentVisual(fretSegment, RenderSettings)); 

        foreach (var edge in Layout.Elements.OfType<FingerboardSideElement>())
            RenderCanvas.Children.Add(new FingerboardSideVisualElement(edge, RenderSettings));

        foreach (var @string in Layout.Strings)
            RenderCanvas.Children.Add(new StringVisualElement(@string, RenderSettings));
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

    private void HideAllOverlays()
    {
        foreach (var overlay in OverlayCanvas.Children.OfType<LayoutOverlayBase>())
        {
            overlay.IsVisible = false;
        }
    }

    private void ShowAllOverlays()
    {
        foreach (var overlay in OverlayCanvas.Children.OfType<LayoutOverlayBase>())
        {
            overlay.IsVisible = true;
        }
    }

    Point ILayoutViewerContext.VectorToScreen(VectorD point)
    {
        if (Orientation == LayoutOrientation.HorizontalNutRight)
        {
            return new Point((double)point.Y * CmScaleFactor * Zoom, (double)point.X * CmScaleFactor * Zoom);
        }
        
        if (Orientation == LayoutOrientation.HorizontalNutLeft)
        {
            return new Point(-(double)point.Y * CmScaleFactor * Zoom, -(double)point.X * CmScaleFactor * Zoom);
        }

        return new Point((double)point.X * CmScaleFactor * Zoom, -(double)point.Y * CmScaleFactor * Zoom);
    }

    private FretNumberOverlayControl fretNumberOverlay;

    private void GenerateOverlays()
    {
        if (Layout == null) return;

        //var helper = new OverlayPositionHelper(Layout, Zoom, Orientation);
        OverlayCanvas.Children.Add(fretNumberOverlay);
        // Pass theme to overlays (will refactor overlays next)
        //FretNumberOverlay.CreateElements(helper, OverlayCanvas, RenderSettings);
    }

    private void RepositionOverlays()
    {
        if (Layout == null) return;

        fretNumberOverlay.InvalidateVisual();
        foreach (var visual in RenderCanvas.Children.OfType<INotifyZoomChanged>())
            visual.ZoomChanged(Zoom);

        var helper = new OverlayPositionHelper(Layout, Zoom, Orientation);
        foreach (var overlay in OverlayCanvas.Children.OfType<ILayoutOverlay>())
        {
            //((Control)overlay).IsVisible = true; // Ensure overlay is visible
            overlay.Reposition(helper);
        }
    }


    #endregion

    #region UI Handlers

    private void Button1_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CreateLayoutVisualsAndOverlays();
    }

    private void ResetViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResetZoomAndTranslation();
    }

    #endregion

    private void OnRenderSettingsChanged()
    {
        RenderSettingsChanged?.Invoke(this, EventArgs.Empty);

        BorderContainer.Background = new SolidColorBrush(RenderSettings.BackgroundColor);
        LayoutGrid.UpdateTheme(RenderSettings);

        // Efficiently update theme for visuals and overlays
        foreach (var child in RenderCanvas.Children)
        {
            if (child is VisualElementBase<Layouts.LayoutElement> visual)
                visual.UpdateTheme(RenderSettings);
        }
        foreach (var overlay in OverlayCanvas.Children.OfType<LayoutOverlayBase>())
        {
            overlay.UpdateTheme(RenderSettings);
        }
    }
}