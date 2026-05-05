using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Settings;
using System;

namespace SiGen.UI.LayoutViewer.Overlays
{
    public class MeasurementOverlayPresenter
    {
        private const double ZeroToleranceCm = 0.01; // 0.1mm — handles floating-point imprecision in snapped intersection points
        private const double LabelLineClearancePx = 8;
        private const double LabelOverlapPushPx = 14;
        private const double IndicatorRadiusPx = 6.5;
        private const string TextBoxClassDark = "measure-overlay-dark";
        private const string TextBoxClassLight = "measure-overlay-light";

        private readonly Color _mainColor = Color.FromArgb(230, 20, 20, 20);
        private readonly Color _xAxisColor = Color.FromArgb(240, 220, 65, 55);
        private readonly Color _yAxisColor = Color.FromArgb(240, 64, 126, 210);
        private readonly Color _indicatorColor = Color.FromArgb(255, 255, 120, 20);

        private Canvas? _overlayCanvas;
        private Measuring.UnitSystem _unitMode;

        private VectorD? _startPoint;
        private VectorD? _endPoint;
        private VectorD? _snapPreviewPoint;
        private bool _snapPreviewVisible;

        private Line _mainLine;
        private Line _xLine;
        private Line _yLine;
        private Line _livePreviewLine;
        private Ellipse _startMarker;
        private Ellipse _snapPreviewMarker;
        private TextBox _mainText;
        private TextBox _xText;
        private TextBox _yText;

        public bool HasStartPoint => _startPoint.HasValue;
        public bool HasCompletedMeasurement => _startPoint.HasValue && _endPoint.HasValue;
        public VectorD? StartPoint => _startPoint;
        private ILayoutViewerContext _layoutViewer = default!;

        public MeasurementOverlayPresenter(ILayoutViewerContext layoutViewer, Measuring.UnitSystem unitMode, LayoutViewerColorScheme theme)
        {
            _layoutViewer = layoutViewer;
            _unitMode = unitMode;

            _mainLine = CreateLine(3);
            _xLine = CreateLine(2.5);
            _yLine = CreateLine(2.5);
            _livePreviewLine = CreatePreviewLine(3);
            _startMarker = CreateIndicatorMarker();
            _snapPreviewMarker = CreateIndicatorMarker();

            _mainText = CreateTextBox();
            _xText = CreateTextBox();
            _yText = CreateTextBox();

            SetTextBoxContrastTheme(false);
            UpdateTheme(theme);
            HideAll();
        }

        public void SetTextBoxContrastTheme(bool isLightBackground)
        {
            var classToAdd = isLightBackground ? TextBoxClassLight : TextBoxClassDark;
            var classToRemove = isLightBackground ? TextBoxClassDark : TextBoxClassLight;

            ApplyTextBoxClass(_mainText, classToAdd, classToRemove);
            ApplyTextBoxClass(_xText, classToAdd, classToRemove);
            ApplyTextBoxClass(_yText, classToAdd, classToRemove);
        }

        public void Attach(Canvas overlayCanvas)
        {
            //if (_overlayCanvas == overlayCanvas)
            //    return;

            _overlayCanvas = overlayCanvas;
            if (!_overlayCanvas.Children.Contains(_mainLine)) _overlayCanvas.Children.Add(_mainLine);
            if (!_overlayCanvas.Children.Contains(_xLine)) _overlayCanvas.Children.Add(_xLine);
            if (!_overlayCanvas.Children.Contains(_yLine)) _overlayCanvas.Children.Add(_yLine);
            if (!_overlayCanvas.Children.Contains(_livePreviewLine)) _overlayCanvas.Children.Add(_livePreviewLine);
            if (!_overlayCanvas.Children.Contains(_startMarker)) _overlayCanvas.Children.Add(_startMarker);
            if (!_overlayCanvas.Children.Contains(_snapPreviewMarker)) _overlayCanvas.Children.Add(_snapPreviewMarker);
            if (!_overlayCanvas.Children.Contains(_mainText)) _overlayCanvas.Children.Add(_mainText);
            if (!_overlayCanvas.Children.Contains(_xText)) _overlayCanvas.Children.Add(_xText);
            if (!_overlayCanvas.Children.Contains(_yText)) _overlayCanvas.Children.Add(_yText);

            Reposition();
        }

        public void SetSnapPreview(VectorD? point, bool isVisible)
        {
            _snapPreviewPoint = point;
            _snapPreviewVisible = isVisible && point.HasValue;
            Reposition();
        }

        public void SetUnitMode(Measuring.UnitSystem unitMode)
        {
            _unitMode = unitMode;
            Reposition();
        }

        public void SetStartPoint(VectorD point)
        {
            _startPoint = point;
            _endPoint = null;
            Reposition();
        }

        public void SetEndPoint(VectorD point)
        {
            if (!_startPoint.HasValue)
            {
                _startPoint = point;
                _endPoint = null;
            }
            else
            {
                _endPoint = point;
            }
            Reposition();
        }

        public void Clear()
        {
            _startPoint = null;
            _endPoint = null;
            HideAll();
        }

        public void Reposition()
        {
            UpdateSnapPreviewMarker();

            if (!_startPoint.HasValue)
            {
                HideMeasurementVisuals();
                return;
            }

            var startScreen = _layoutViewer.VectorToScreen(_startPoint.Value);
            PositionIndicator(_startMarker, startScreen, true);

            if (!_endPoint.HasValue)
            {
                UpdateLivePreviewLine(startScreen);
                _mainLine.IsVisible = false;
                _xLine.IsVisible = false;
                _yLine.IsVisible = false;
                _mainText.IsVisible = false;
                _xText.IsVisible = false;
                _yText.IsVisible = false;
                return;
            }

            _livePreviewLine.IsVisible = false;
            _startMarker.IsVisible = false;

            var end = _endPoint.Value;
            var start = _startPoint.Value;
            var endScreen = _layoutViewer.VectorToScreen(end);
            var delta = end - start;

            _mainLine.StartPoint = startScreen;
            _mainLine.EndPoint = endScreen;
            _mainLine.IsVisible = true;

            _mainText.Text = FormatMeasure(VectorD.Distance(start, end));
            _mainText.IsVisible = true;

            var hasX = Math.Abs(delta.X) > ZeroToleranceCm;
            var hasY = Math.Abs(delta.Y) > ZeroToleranceCm;
            var showComponents = hasX && hasY; // only show axis breakdown when truly diagonal

            var elbow = new VectorD(end.X, start.Y);
            var elbowScreen = _layoutViewer.VectorToScreen(elbow);
            var triangleCenter = new Point(
                (startScreen.X + elbowScreen.X + endScreen.X) / 3d,
                (startScreen.Y + elbowScreen.Y + endScreen.Y) / 3d);

            _xLine.IsVisible = showComponents;
            _yLine.IsVisible = showComponents;
            _xText.IsVisible = showComponents;
            _yText.IsVisible = showComponents;

            if (showComponents)
            {
                _xLine.StartPoint = startScreen;
                _xLine.EndPoint = elbowScreen;
                _xText.Text = FormatMeasure(Math.Abs(delta.X));
            }

            if (showComponents)
            {
                _yLine.StartPoint = elbowScreen;
                _yLine.EndPoint = endScreen;
                _yText.Text = FormatMeasure(Math.Abs(delta.Y));
            }

            var mainRect = PositionLabelAwayFromTriangle(_mainText, startScreen, endScreen, triangleCenter, 0);

            Rect? xRect = null;
            Rect? yRect = null;

            if (showComponents)
                xRect = PositionLabelAwayFromTriangle(_xText, startScreen, elbowScreen, triangleCenter, 0);

            if (showComponents)
                yRect = PositionLabelAwayFromTriangle(_yText, elbowScreen, endScreen, triangleCenter, 0);

            if (showComponents && xRect.HasValue && xRect.Value.Intersects(mainRect))
                xRect = PositionLabelAwayFromTriangle(_xText, startScreen, elbowScreen, triangleCenter, LabelOverlapPushPx);

            if (showComponents && yRect.HasValue && (yRect.Value.Intersects(mainRect) || (xRect.HasValue && yRect.Value.Intersects(xRect.Value))))
                PositionLabelAwayFromTriangle(_yText, elbowScreen, endScreen, triangleCenter, LabelOverlapPushPx);
        }

        public void UpdateTheme(LayoutViewerColorScheme theme)
        {
            _mainLine.Stroke = new SolidColorBrush(_mainColor);
            _xLine.Stroke = new SolidColorBrush(_xAxisColor);
            _yLine.Stroke = new SolidColorBrush(_yAxisColor);
            _livePreviewLine.Stroke = new SolidColorBrush(Color.FromArgb(200, _mainColor.R, _mainColor.G, _mainColor.B));

            var indicatorStroke = new SolidColorBrush(_indicatorColor);
            var indicatorFill = new SolidColorBrush(Color.FromArgb(150, _indicatorColor.R, _indicatorColor.G, _indicatorColor.B));
            _startMarker.Stroke = indicatorStroke;
            _startMarker.Fill = indicatorFill;
            _snapPreviewMarker.Stroke = indicatorStroke;
            _snapPreviewMarker.Fill = indicatorFill;

            _mainText.BorderBrush = new SolidColorBrush(_mainColor);
            _xText.BorderBrush = new SolidColorBrush(_xAxisColor);
            _yText.BorderBrush = new SolidColorBrush(_yAxisColor);
        }

        public bool IsOwnedControl(object? control)
        {
            if (control is not Visual visual)
                return false;

            Visual? current = visual;
            while (current != null)
            {
                if (current == _mainText || current == _xText || current == _yText)
                    return true;
                current = current.GetVisualParent() as Visual;
            }

            return false;
        }

        private void HideAll()
        {
            HideMeasurementVisuals();
            _snapPreviewMarker.IsVisible = false;
        }

        private void HideMeasurementVisuals()
        {
            _mainLine.IsVisible = false;
            _xLine.IsVisible = false;
            _yLine.IsVisible = false;
            _livePreviewLine.IsVisible = false;
            _mainText.IsVisible = false;
            _xText.IsVisible = false;
            _yText.IsVisible = false;
            _startMarker.IsVisible = false;
        }

        private void UpdateSnapPreviewMarker()
        {
            if (!_snapPreviewVisible || !_snapPreviewPoint.HasValue)
            {
                _snapPreviewMarker.IsVisible = false;
                return;
            }

            PositionIndicator(_snapPreviewMarker, _layoutViewer.VectorToScreen(_snapPreviewPoint.Value), true);
        }

        private void PositionIndicator(Ellipse indicator, Point point, bool visible)
        {
            Canvas.SetLeft(indicator, point.X - IndicatorRadiusPx);
            Canvas.SetTop(indicator, point.Y - IndicatorRadiusPx);
            indicator.IsVisible = visible;
        }

        private void UpdateLivePreviewLine(Point startScreen)
        {
            if (!_snapPreviewVisible || !_snapPreviewPoint.HasValue)
            {
                _livePreviewLine.IsVisible = false;
                return;
            }

            var previewScreen = _layoutViewer.VectorToScreen(_snapPreviewPoint.Value);
            _livePreviewLine.StartPoint = startScreen;
            _livePreviewLine.EndPoint = previewScreen;
            _livePreviewLine.IsVisible = true;
        }

        private Rect PositionLabelAwayFromTriangle(TextBox label, Point lineStart, Point lineEnd, Point triangleCenter, double extraPush)
        {
            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = label.DesiredSize;
            var midpoint = MidPoint(lineStart, lineEnd);

            // Compute the perpendicular to the segment
            Vector segment = lineEnd - lineStart;
            Vector perp = segment.Length > 0.001
                ? new Vector(-segment.Y, segment.X) / segment.Length
                : new Vector(0, -1);

            // Choose the side that points away from triangle center
            Vector toCenter = triangleCenter - midpoint;
            if (Vector.Dot(perp, toCenter) > 0)
                perp = -perp;

            var halfProjection = Math.Abs(perp.X) * (size.Width * 0.5) + Math.Abs(perp.Y) * (size.Height * 0.5);
            var clearance = halfProjection + LabelLineClearancePx + extraPush;
            var center = midpoint + perp * clearance;

            var rect = new Rect(
                center.X - size.Width * 0.5,
                center.Y - size.Height * 0.5,
                size.Width,
                size.Height);

            Canvas.SetLeft(label, rect.X);
            Canvas.SetTop(label, rect.Y);
            return rect;
        }

        private static Point MidPoint(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) * 0.5, (p1.Y + p2.Y) * 0.5);
        }

        private string FormatMeasure(double normalizedCm)
        {
            var unit = _unitMode == Measuring.UnitSystem.Imperial ? LengthUnit.In : LengthUnit.Mm;
            return Measure.FromNormalizedValue(unit, normalizedCm).ToStringFormatted();
        }

        private static Line CreateLine(double thickness)
        {
            return new Line
            {
                StrokeThickness = thickness,
                IsVisible = false,
                IsHitTestVisible = false,
               
            };
        }

        private static Line CreatePreviewLine(double thickness)
        {
            return new Line
            {
                StrokeThickness = thickness,
                StrokeDashArray = new AvaloniaList<double> { 4, 3 },
                IsVisible = false,
                IsHitTestVisible = false
            };
        }

        private static Ellipse CreateIndicatorMarker()
        {
            return new Ellipse
            {
                Width = IndicatorRadiusPx * 2,
                Height = IndicatorRadiusPx * 2,
                StrokeThickness = 1.8,
                IsVisible = false,
                IsHitTestVisible = false
            };
        }

        private static TextBox CreateTextBox()
        {
            return new TextBox
            {
                IsReadOnly = true,
                MinHeight = 0,
                Padding = new Thickness(4, 2),
                FontSize = 13,
                BorderThickness = new Thickness(1.5),
                IsVisible = false
            };
        }

        private static void ApplyTextBoxClass(TextBox textBox, string classToAdd, string classToRemove)
        {
            textBox.Classes.Remove(classToRemove);
            if (!textBox.Classes.Contains(classToAdd))
                textBox.Classes.Add(classToAdd);
        }
    }
}

