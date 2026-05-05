using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SiGen.Measuring;
using SiGen.Utilities;
using System;
using System.Globalization;

namespace SiGen.UI.LayoutViewer
{
    public class LayoutGridControl : Control
    {
        public static readonly StyledProperty<double> ZoomProperty =
            AvaloniaProperty.Register<LayoutGridControl, double>(nameof(Zoom), 1);

        public static readonly StyledProperty<UnitSystem> UnitModeProperty =
            AvaloniaProperty.Register<LayoutGridControl, UnitSystem>(nameof(UnitMode), UnitSystem.Metric);

        public static readonly StyledProperty<RectangleM?> LayoutBoundsProperty =
            AvaloniaProperty.Register<LayoutGridControl, RectangleM?>(nameof(LayoutBounds));

        // Main grid interval (cm or inch)
        private double GridSize => UnitMode == UnitSystem.Metric ? MeasureUtils.CmToPixels : 96;
        // Number of main grid cells per major grid cell
        private int MajorGridDivisions => UnitMode == UnitSystem.Metric ? 5 : 6;

        protected LayoutViewerColorScheme ColorScheme { get; private set; } = new ();

        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public UnitSystem UnitMode
        {
            get => GetValue(UnitModeProperty);
            set => SetValue(UnitModeProperty, value);
        }

        public RectangleM? LayoutBounds
        {
            get => GetValue(LayoutBoundsProperty);
            set => SetValue(LayoutBoundsProperty, value);
        }

        static LayoutGridControl()
        {
            AffectsRender<LayoutGridControl>(UnitModeProperty, ZoomProperty, LayoutBoundsProperty);
        }

        public LayoutGridControl()
        {
            Focusable = true;
        }

        private Rect blueprintGridRect = new Rect();

        public virtual void UpdateTheme(LayoutViewerColorScheme theme)
        {
            ColorScheme = theme;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (blueprintGridRect.Height > 0)
            {
                bool showSubUnits = UnitMode == UnitSystem.Metric ? Zoom >= 4 : true;
                var gridBrush = CreateGridBrush(showSubUnits);
                context.FillRectangle(gridBrush, blueprintGridRect);

                double majorPenSize = Math.Max(3 / Zoom, 0.45);
                var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(120, ColorScheme.GridColor.R, ColorScheme.GridColor.G, ColorScheme.GridColor.B)), majorPenSize);
                context.DrawLine(majorPen, new Point(centerLineOffsetX, blueprintGridRect.Top), new Point(centerLineOffsetX, blueprintGridRect.Bottom));
                context.DrawLine(majorPen, new Point(blueprintGridRect.Left, centerLineOffsetY), new Point(blueprintGridRect.Right, centerLineOffsetY));
            }
            base.Render(context);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == LayoutBoundsProperty && LayoutBounds != null)
            {
                SetBluePrintBounds(LayoutBounds);
            }
            else if (change.Property == UnitModeProperty && LayoutBounds != null)
            {
                SetBluePrintBounds(LayoutBounds);
            }
        }

        private double majorGridOffsetX = 0;
        private double majorGridOffsetY = 0;
        private double centerLineOffsetX = 0;
        private double centerLineOffsetY = 0;

        public void SetBluePrintBounds(RectangleM bounds)
        {
            double scale = MeasureUtils.CmToPixels;
            int padding = UnitMode == UnitSystem.Metric ? 20 : 8;
            int columnCount = (int)Math.Ceiling((double)(bounds.Top - bounds.Bottom).NormalizedValue * scale / GridSize) + padding;
            columnCount = (int)Math.Ceiling(Math.Floor(columnCount / (double)MajorGridDivisions) / 2d) * 2 * MajorGridDivisions;
            blueprintGridRect = new Rect(0, 0, columnCount * GridSize, columnCount * GridSize);
            centerLineOffsetX = blueprintGridRect.Width / 2d;
            centerLineOffsetY = blueprintGridRect.Height / 2d;
            Height = blueprintGridRect.Height;
            Width = blueprintGridRect.Width;
            Canvas.SetLeft(this, blueprintGridRect.Width / -2d);
            Canvas.SetTop(this, blueprintGridRect.Height / -2d);
        }
        
        private DrawingBrush CreateGridBrush(bool showSubUnits)
        {
            // Size of a major grid cell (group of main grid cells)
            var majorGridSize = GridSize * MajorGridDivisions;
            // Subdivision grid (mm or fractional inch)
            int subGridDivisions = UnitMode == UnitSystem.Metric ? 10 : (Zoom > 4 ? 16 : (Zoom > 1 ? 8 : 4));
            var subGridSize = GridSize / subGridDivisions;
            var drawingGroup = new DrawingGroup();
            double subPenSize = Math.Max(0.75 / Zoom, 0.1);
            double minorPenSize = Math.Max(1 / Zoom, 0.2);
            double majorPenSize = Math.Max(2 / Zoom, 0.3);
            // Hardcoded alpha values
            var subColor = Color.FromArgb(20, ColorScheme.GridColor.R, ColorScheme.GridColor.G, ColorScheme.GridColor.B);
            var minorColor = Color.FromArgb(40, ColorScheme.GridColor.R, ColorScheme.GridColor.G, ColorScheme.GridColor.B);
            var majorColor = Color.FromArgb(90, ColorScheme.MajorAxisColor.R, ColorScheme.MajorAxisColor.G, ColorScheme.MajorAxisColor.B);
            var subPen = new Pen(new SolidColorBrush(subColor), subPenSize);
            var minorPen = new Pen(new SolidColorBrush(minorColor), minorPenSize);
            var majorPen = new Pen(new SolidColorBrush(majorColor), majorPenSize);
            double majorOffset = majorPenSize / 2d;
            var majorClipRect = new RectangleGeometry
            {
                Rect = new Rect(majorOffset, majorOffset, majorGridSize - (majorOffset *2d), majorGridSize - (majorOffset * 2d))
            };
            var minorLinesGroup = new DrawingGroup();
            minorLinesGroup.ClipGeometry = majorClipRect;
            // Draw subdivision grid lines if enabled
            if (showSubUnits)
            {
                for (double i = subGridSize; i < majorGridSize; i += subGridSize)
                {
                    if (Math.Abs(i % GridSize) < 0.01 || Math.Abs(i % majorGridSize) < 0.01)
                        continue;
                    var verticalSubLine = new LineGeometry
                    {
                        StartPoint = new Point(i, 0),
                        EndPoint = new Point(i, majorGridSize)
                    };
                    minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = verticalSubLine, Pen = subPen });
                    var horizontalSubLine = new LineGeometry
                    {
                        StartPoint = new Point(0, i),
                        EndPoint = new Point(majorGridSize, i)
                    };
                    minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = horizontalSubLine, Pen = subPen });
                }
            }
            // Draw minor grid lines (main grid interval)
            for (int i = 1; i < MajorGridDivisions; i++)
            {
                var offset = i * GridSize;
                var verticalLine = new LineGeometry
                {
                    StartPoint = new Point(offset, 0),
                    EndPoint = new Point(offset, majorGridSize)
                };
                minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = verticalLine, Pen = minorPen });
                var horizontalLine = new LineGeometry
                {
                    StartPoint = new Point(0, offset),
                    EndPoint = new Point(majorGridSize, offset)
                };
                minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = horizontalLine, Pen = minorPen });
            }
            var gridGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, majorGridSize, majorGridSize)
            };
            var majorSquare = new RectangleGeometry
            {
                Rect = new Rect(0, 0, majorGridSize, majorGridSize)
            };
            drawingGroup.Children.Add(minorLinesGroup);
            drawingGroup.Children.Add(new GeometryDrawing { Geometry = majorSquare, Pen = majorPen });
            drawingGroup.ClipGeometry = gridGeometry;
            return new DrawingBrush
            {
                Drawing = drawingGroup,
                TileMode = TileMode.Tile,
                AlignmentX = AlignmentX.Left,
                SourceRect = new RelativeRect(new Rect(0, 0, majorGridSize, majorGridSize), RelativeUnit.Absolute),
                DestinationRect = new RelativeRect(new Rect(majorGridOffsetX, majorGridOffsetY, majorGridSize, majorGridSize), RelativeUnit.Absolute),
                AlignmentY = AlignmentY.Top,
            };
        }
    }
}
