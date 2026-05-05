using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.UI.LayoutViewer.Overlays;
using SiGen.UI.LayoutViewer.Visuals;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiGen.Layouts.Snapping;

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
    private LayoutViewerColorScheme _renderSettings = LayoutViewerColorScheme.Blueprint;

    public LayoutViewerColorScheme ColorScheme
    {
        get => _renderSettings;
        set
        {
            if (_renderSettings != value)
            {
                _renderSettings = value;
                OnColorSchemeChanged();
            }
        }
    }

    public event EventHandler? ColorSchemeChanged;

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

    public static readonly StyledProperty<Measuring.UnitSystem> UnitModeProperty =
        AvaloniaProperty.Register<LayoutViewerControl, Measuring.UnitSystem>(nameof(UnitMode), Measuring.UnitSystem.Metric);

    public static readonly StyledProperty<SnapLineType> ActiveSnapFiltersProperty =
        AvaloniaProperty.Register<LayoutViewerControl, SnapLineType>(nameof(ActiveSnapFilters), SnapLineType.Fret | SnapLineType.String | SnapLineType.Fingerboard | SnapLineType.CenterLine);

    public static readonly StyledProperty<bool> IsMeasureToolActiveProperty =
        AvaloniaProperty.Register<LayoutViewerControl, bool>(nameof(IsMeasureToolActive), false);

    public Measuring.UnitSystem UnitMode
    {
        get => GetValue(UnitModeProperty);
        set => SetValue(UnitModeProperty, value);
    }

    public SnapLineType ActiveSnapFilters
    {
        get => GetValue(ActiveSnapFiltersProperty);
        set => SetValue(ActiveSnapFiltersProperty, value);
    }

    public bool IsMeasureToolActive
    {
        get => GetValue(IsMeasureToolActiveProperty);
        set => SetValue(IsMeasureToolActiveProperty, value);
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


        fretNumberOverlay = new FretNumbersRenderer(this);
        measurementOverlay = new MeasurementOverlayPresenter(this, UnitMode, ColorScheme);

        OnColorSchemeChanged();
        LayoutGrid.SetBluePrintBounds(new RectangleM(Measuring.Measure.Cm(-5), Measuring.Measure.Cm(-5), Measuring.Measure.Cm(10), Measuring.Measure.Cm(10)));

        zoomEndTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        zoomEndTimer.Tick += ZoomEndTimer_Tick;
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == UnitModeProperty)
        {
            LayoutGrid.UnitMode = UnitMode;
            measurementOverlay.SetUnitMode(UnitMode);
        }
        else if (change.Property == ZoomProperty)
        {
            CalculateTranslationBounds();

            double newZoom = change.GetNewValue<double>();

            _zoomTransform.ScaleX = newZoom;
            _zoomTransform.ScaleY = newZoom * -1d;
            IsZoomToFit = Math.Abs(fitToViewZoom - newZoom) <= 0.001 && Translation.X == 0 && Translation.Y == 0;

            OnZoomChanged();
        }
        else if (change.Property == TranslationProperty)
        {
            _translateTransform.X = change.GetNewValue<Point>().X;
            _translateTransform.Y = change.GetNewValue<Point>().Y;
            IsZoomToFit = Math.Abs(fitToViewZoom - Zoom) <= 0.001 && Translation.X == 0 && Translation.Y == 0;

            OnTranslationChanged();
        }
        else if (change.Property == IsMeasureToolActiveProperty && !IsMeasureToolActive)
        {
            HideSnapPreview();
            measurementOverlay.Clear();
        }

        if (change.Property == LayoutProperty)
        {
            StopFling();

            if (!IsLoaded)
                return;

            CalculateTranslationBounds();

            CreateLayoutVisualsAndOverlays();
            HideSnapPreview();
            measurementOverlay.Clear();

            CalculateZoomToFit();

            if (IsZoomToFit)
            {
                IsZoomToFit = true;
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
            _orientationTransform.Angle = Orientation switch
            {
                LayoutOrientation.HorizontalNutRight => 90,
                LayoutOrientation.HorizontalNutLeft => 270,
                _ => 0
            };
            CalculateZoomToFit();
            ResetZoomAndTranslation();
            RepositionOverlays();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Layout != null)
        {
            CalculateTranslationBounds();
            CreateLayoutVisualsAndOverlays();
            CalculateZoomToFit();
            if (IsZoomToFit)
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

        if (IsZoomToFit)
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

    public bool IsZoomToFit { get; private set; } = true;
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
        IsZoomToFit = true;
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
                availableSize.Height / (layoutBounds.Height.NormalizedValue * MeasureUtils.CmToPixels),
                availableSize.Width / (layoutBounds.Width.NormalizedValue * MeasureUtils.CmToPixels)
            );
        }
        else
        {
            fitToViewZoom = (double)MathD.Min(
                availableSize.Height / (layoutBounds.Width.NormalizedValue * MeasureUtils.CmToPixels),
                availableSize.Width / (layoutBounds.Height.NormalizedValue * MeasureUtils.CmToPixels)
            );
        }

        minimumZoom = Math.Max(fitToViewZoom * 0.90, 0.15);
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
    }


    private void ZoomEndTimer_Tick(object? sender, EventArgs e)
    {
        zoomEndTimer.Stop();
        isZooming = false;

        OnZoomEnd();


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
    private bool _isPanning;

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

        Translation = ClampTranslation(Translation + delta);
    }

    protected void OnTranslationChanged()
    {
        measurementOverlay.Reposition();
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

    #region Mouse / Touch Handling

    private enum PanSource
    {
        Touch,
        LeftClick,
        MiddleClick,
        Spacebar
    }

    private PanSource? _currentPanSource = null;

    public void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(RenderCanvas);
        bool allowLeftMousePan = !(IsMeasureToolActive && point.Pointer.Type == PointerType.Mouse && point.Properties.IsLeftButtonPressed);
        bool isPanningButton = point.Pointer.Type != PointerType.Mouse ||
                               point.Properties.IsMiddleButtonPressed ||
                               (point.Properties.IsLeftButtonPressed && allowLeftMousePan);

        if (isPanningButton)
            StopFling();


        if (!_isPanning && isPanningButton && _currentPanSource == null)
        {
            if (point.Pointer.Type != PointerType.Mouse)
                _currentPanSource = PanSource.Touch;
            else
                _currentPanSource = point.Properties.IsLeftButtonPressed ? PanSource.LeftClick : PanSource.MiddleClick;

            _lastMousePosition = e.GetPosition(this);
            RenderCanvas.Cursor = new Cursor(StandardCursorType.SizeAll);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {

        if (e.Key == Key.Space && !_isPanning)
        {
            StopFling();
            //var mousePos = MouseDevice.Instance.GetPosition(this);
            _lastMousePosition = new Point();
            _currentPanSource = PanSource.Spacebar;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Space && _currentPanSource == PanSource.Spacebar)
        {

            _currentPanSource = null;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Arrow);
            e.Handled = true;

            if (_isPanning)
            {
                var velocity = CalculateFlingVelocity();
                _isPanning = false;
                if (velocity.Length > MinimumFlingVelocity)
                    StartFling(velocity);
            }

        }
        base.OnKeyUp(e);
    }

    public void Canvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_currentPanSource != null)
        {
            var currentPosition = e.GetPosition(this);

            if (!_isPanning)
            {
                _isPanning = true;
                _lastMousePosition = currentPosition;
                return;
            }

            AddPositionToFlingHistory(currentPosition);

            var delta = currentPosition - _lastMousePosition;
            Translation += delta;
            _lastMousePosition = currentPosition;
            return;
        }

        UpdateSnappingPreview(e.GetPosition(this), e.KeyModifiers);
    }

    private void UpdateSnappingPreview(Point pointerPosition, KeyModifiers keyModifiers)
    {
        if (!IsMeasureToolActive || Layout == null || ActiveSnapFilters == SnapLineType.None || Zoom <= 0)
        {
            HideSnapPreview();
            return;
        }

        var cursorInLayout = ScreenToLayout(pointerPosition);
        var maxDistanceCm = SnapMaxDistancePx / (MeasureUtils.CmToPixels * Zoom);

        //if (keyModifiers.HasFlag(KeyModifiers.Control) && measurementOverlay.HasStartPoint)
        //{
        //    cursorInLayout = SnapPointToAngleIncrement(measurementOverlay.StartPoint!.Value, cursorInLayout);
        //}

        var snapResult = Layout.SnapData.TrySnap(cursorInLayout, ActiveSnapFilters, maxDistanceCm);

        bool allowNonSnappedPoint = keyModifiers.HasFlag(KeyModifiers.Alt);

        VectorD previewPoint;
        if (snapResult.IsSnapped)
        {
            _currentSnapResult = snapResult;
            previewPoint = snapResult.Position;
        }
        else if (allowNonSnappedPoint)
        {
            _currentSnapResult = LayoutSnapResult.None;
            previewPoint = cursorInLayout;
        }
        else
        {
            HideSnapPreview();
            return;
        }

        measurementOverlay.SetSnapPreview(previewPoint, true);
    }

    private void HideSnapPreview()
    {
        _currentSnapResult = LayoutSnapResult.None;
        measurementOverlay.SetSnapPreview(null, false);
    }

    private VectorD ScreenToLayout(Point point)
    {
        var translated = point - new Point(_centerTransform.X + Translation.X, _centerTransform.Y + Translation.Y);
        var scale = MeasureUtils.CmToPixels * Zoom;
        if (scale <= 0)
            return VectorD.Empty;

        return Orientation switch
        {
            LayoutOrientation.HorizontalNutRight => new VectorD(translated.Y / scale, translated.X / scale),
            LayoutOrientation.HorizontalNutLeft => new VectorD(-translated.Y / scale, -translated.X / scale),
            _ => new VectorD(translated.X / scale, -translated.Y / scale),
        };
    }

    private bool MatchPanSource(PointerReleasedEventArgs eventArgs)
    {
        if (_currentPanSource == PanSource.Touch)
            return true;
        else if (_currentPanSource == PanSource.LeftClick)
            return eventArgs.InitialPressMouseButton == MouseButton.Left;
        else if (_currentPanSource == PanSource.MiddleClick)
            return eventArgs.InitialPressMouseButton == MouseButton.Middle;

        return false;
    }

    public void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (IsMeasureToolActive && e.InitialPressMouseButton == MouseButton.Left && _currentPanSource == null
            && !measurementOverlay.IsOwnedControl(e.Source))
        {
            HandleMeasureClick(e.GetPosition(this), e.KeyModifiers);
            return;
        }

        if (_isPanning && MatchPanSource(e))
        {
            _isPanning = false;
            _currentPanSource = null;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Arrow);

            var velocity = CalculateFlingVelocity();

            if (velocity.Length > MinimumFlingVelocity)
                StartFling(velocity);
        }
        else if (_currentPanSource != null && MatchPanSource(e))
        {
            _currentPanSource = null;
            RenderCanvas.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    public void Canvas_OnPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        StopFling();

        double factor = e.Delta.Y > 0 ? ZoomFactorStep : 1 / ZoomFactorStep;
        ZoomAtPoint(e.GetPosition(this), factor);
    }

    #region Pinch Handling

    private double lastPinchScale = 1.0;

    private void Canvas_PinchGesture(object? sender, PinchEventArgs e)
    {
        var center = e.ScaleOrigin;
        var scaleFactor = e.Scale;
        double scaleDelta = scaleFactor - lastPinchScale;

        if (Math.Abs(scaleDelta) < 0.01)
            return;

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

    #region Measure Tool

    private void HandleMeasureClick(Point pointerPosition, KeyModifiers keyModifiers)
    {
        if (Layout == null)
            return;

        bool allowNonSnappedPoint = keyModifiers.HasFlag(KeyModifiers.Alt);

        VectorD selectedPoint = _currentSnapResult.IsSnapped ? _currentSnapResult.Position : ScreenToLayout(pointerPosition);


        if (!_currentSnapResult.IsSnapped && !allowNonSnappedPoint)

        {
            if (measurementOverlay.HasCompletedMeasurement)
                measurementOverlay.Clear();
            return;
        }

        if (!measurementOverlay.HasStartPoint || measurementOverlay.HasCompletedMeasurement)
        {
            measurementOverlay.SetStartPoint(selectedPoint);
        }
        else
        {
            measurementOverlay.SetEndPoint(selectedPoint);
        }
    }

    private static VectorD SnapPointToAngleIncrement(VectorD origin, VectorD target)
    {
        var delta = target - origin;
        var length = delta.Length();
        if (length <= double.Epsilon)
            return target;

        const double angleStep = Math.PI / 12d;
        var angle = Math.Atan2(delta.Y, delta.X);
        var snappedAngle = Math.Round(angle / angleStep) * angleStep;

        return new VectorD(
            origin.X + Math.Cos(snappedAngle) * length,
            origin.Y + Math.Sin(snappedAngle) * length);
    }

    #endregion



    #endregion

    #region Visuals Elements

    private void CreateLayoutVisualsAndOverlays()
    {
        RenderCanvas.Children.Clear();
        //OverlayCanvas.Children.Clear();

        if (Layout == null) return;

        var layoutBounds = Layout.Bounds!;

        LayoutGrid.LayoutBounds = layoutBounds;
        RenderCanvas.Children.Add(LayoutGrid);

        GenerateOverlays();

        foreach (var median in Layout.Elements.OfType<GuideLineElement>())
            RenderCanvas.Children.Add(new GuideLineVisualElement(median, ColorScheme));

        foreach (var edge in Layout.Elements.OfType<FingerboardEdgeElement>())
            RenderCanvas.Children.Add(new FingerboardEdgeVisualElement(edge, ColorScheme));

        RenderCanvas.Children.Add(new FretRendererControl(this));

        foreach (var @string in Layout.Strings)
            RenderCanvas.Children.Add(new StringVisualElement(@string, ColorScheme));
    }

    #endregion

    #region Overlays

    private const double SnapMaxDistancePx = 10;

    private LayoutSnapResult _currentSnapResult = LayoutSnapResult.None;

    private MeasurementOverlayPresenter measurementOverlay;
    private FretNumbersRenderer fretNumberOverlay;

    Point ILayoutViewerContext.VectorToScreen(VectorD point)
    {
        if (Orientation == LayoutOrientation.HorizontalNutRight)
        {
            return new Point(point.Y * MeasureUtils.CmToPixels * Zoom, point.X * MeasureUtils.CmToPixels * Zoom);
        }

        if (Orientation == LayoutOrientation.HorizontalNutLeft)
        {
            return new Point(-point.Y * MeasureUtils.CmToPixels * Zoom, -point.X * MeasureUtils.CmToPixels * Zoom);
        }

        return new Point(point.X * MeasureUtils.CmToPixels * Zoom, -point.Y * MeasureUtils.CmToPixels * Zoom);
    }

    

    private void GenerateOverlays()
    {
        if (Layout == null) return;

        if (OverlayCanvas.Children.Count == 0)
        {
            OverlayCanvas.Children.Add(fretNumberOverlay);

            measurementOverlay.Attach(OverlayCanvas);
            measurementOverlay.Reposition();
        }
        else
        {
            fretNumberOverlay.InvalidateVisual();
            measurementOverlay.Reposition();
        }
    }

    private void RepositionOverlays()
    {
        if (Layout == null) return;

        fretNumberOverlay.InvalidateVisual();
        measurementOverlay.Reposition();
    }

    #endregion

    #region UI Handlers

    private void ResetViewButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ResetZoomAndTranslation();
    }

    private void SnapFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is SnapLineType flagToToggle)
        {
            // Toggle the flag
            if (ActiveSnapFilters.HasFlag(flagToToggle))
                ActiveSnapFilters &= ~flagToToggle;
            else
                ActiveSnapFilters |= flagToToggle;
        }
    }

    private void SnapMenuFlyout_Opening(object? sender, EventArgs e)
    {
        if (sender is MenuFlyout flyout)
        {
            foreach (var item in flyout.Items.OfType<MenuItem>())
            {
                if (item.Tag is SnapLineType flag)
                {
                    item.IsChecked = ActiveSnapFilters.HasFlag(flag);
                }
            }
        }
    }

    #endregion

    private void OnColorSchemeChanged()
    {
        ColorSchemeChanged?.Invoke(this, EventArgs.Empty);

        BorderContainer.Background = new SolidColorBrush(ColorScheme.BackgroundColor);
        LayoutGrid.UpdateTheme(ColorScheme);

        measurementOverlay.UpdateTheme(ColorScheme);
        fretNumberOverlay.InvalidateVisual();

        // Efficiently update theme for visuals and overlays
        foreach (var child in RenderCanvas.Children.OfType<ILayoutRenderable>())
        {
            child.UpdateColorScheme(ColorScheme);
        }

        //foreach (var overlay in OverlayCanvas.Children.OfType<ILayoutRenderable>())
        //{
        //    overlay.UpdateColorScheme(ColorScheme);
        //}

        double luminance = RelativeLuminance(ColorScheme.BackgroundColor);
        bool isLightBackground = luminance > 0.5;
        measurementOverlay.SetTextBoxContrastTheme(isLightBackground);

        if (isLightBackground)
        {
            ZoomPanelThemeVariant.RequestedThemeVariant = ThemeVariant.Light;
        }
        else
        {
            ZoomPanelThemeVariant.RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    private static double RelativeLuminance(Color c)
    {
        double Process(double s) =>
            s < 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);

        double dR = (double)c.R / 255,
            dG = (double)c.G / 255,
            dB = (double)c.B / 255;

        var r = Process(dR);
        var g = Process(dG);
        var b = Process(dB);

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }
}