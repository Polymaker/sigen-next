using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SiGen.Measuring;
using System;
using System.Globalization;

namespace SiGen.UI
{
    public class LayoutGridControl : Control
    {

        public static readonly StyledProperty<double> ZoomProperty =
        AvaloniaProperty.Register<LayoutGridControl, double>(nameof(Zoom), 1);

        public static readonly StyledProperty<IBrush> MillimeterGridBrushProperty =
       AvaloniaProperty.Register<LayoutGridControl, IBrush>(nameof(MillimeterGridBrush),
           new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)));

        public static readonly StyledProperty<IBrush> MinorGridBrushProperty =
            AvaloniaProperty.Register<LayoutGridControl, IBrush>(nameof(MinorGridBrush),
                new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));

        public static readonly StyledProperty<IBrush> MajorGridBrushProperty =
            AvaloniaProperty.Register<LayoutGridControl, IBrush>(nameof(MajorGridBrush),
                new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)));

        public static readonly StyledProperty<UnitMode> UnitModeProperty =
            AvaloniaProperty.Register<LayoutGridControl, UnitMode>(nameof(UnitMode), UnitMode.Metric);

        public static readonly StyledProperty<RectangleM?> LayoutBoundsProperty =
            AvaloniaProperty.Register<LayoutGridControl, RectangleM?>(nameof(LayoutBounds));

        private double GridSize => UnitMode == UnitMode.Metric ? 37.7952755906 : 96;

        private int MajorGridDivisions => UnitMode == UnitMode.Metric ? 5 : 6;


        public IBrush MillimeterGridBrush
        {
            get => GetValue(MillimeterGridBrushProperty);
            set => SetValue(MillimeterGridBrushProperty, value);
        }

        public IBrush MinorGridBrush
        {
            get => GetValue(MinorGridBrushProperty);
            set => SetValue(MinorGridBrushProperty, value);
        }

        public IBrush MajorGridBrush
        {
            get => GetValue(MajorGridBrushProperty);
            set => SetValue(MajorGridBrushProperty, value);
        }

        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public UnitMode UnitMode
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

        private Rect blueprintGridRect = new Rect();

        public override void Render(DrawingContext context)
        {
            //var renderBounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            //var backgroundBrush = new SolidColorBrush(Color.FromRgb(30, 50, 80)); // Blueprint blue
            //context.FillRectangle(backgroundBrush, renderBounds);

            if (blueprintGridRect.Height > 0)
            {
                

                bool showSubUnits = UnitMode == UnitMode.Metric ? Zoom >= 4 : true; // Show subunits (millimeters) at zoom level 4 and above
                var gridBrush = CreateGridBrush(showSubUnits);
                context.FillRectangle(gridBrush, blueprintGridRect);

                double majorPenSize = Math.Max(3 / Zoom, 0.45);
                var majorPen = new Pen(MajorGridBrush, majorPenSize);
                context.DrawLine(majorPen, new Point(centerLineOffsetX, blueprintGridRect.Top), new Point(centerLineOffsetX, blueprintGridRect.Bottom));
                context.DrawLine(majorPen, new Point(blueprintGridRect.Left, centerLineOffsetY), new Point(blueprintGridRect.Right, centerLineOffsetY));
            }


            base.Render(context);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == LayoutBoundsProperty)
            {
                if (LayoutBounds != null)
                    SetBluePrintBounds(LayoutBounds);
            }
        }


        private double majorGridOffsetX = 0;
        private double majorGridOffsetY = 0;
        private double centerLineOffsetX = 0;
        private double centerLineOffsetY = 0;

        public void SetBluePrintBounds(RectangleM bounds)
        {
            double scale = LayoutViewer.LayoutViewer.CmScaleFactor;
            int columnCount = (int)Math.Ceiling((double)(bounds.Top - bounds.Bottom).NormalizedValue * scale / GridSize) + 20;
            columnCount = (int)Math.Ceiling(Math.Floor(columnCount / (double)MajorGridDivisions) / 2d) * 2 * MajorGridDivisions;
            
            //columnCount = (int)Math.Ceiling(columnCount / 2d) * 2;
            blueprintGridRect = new Rect(0, 0, columnCount * GridSize, columnCount * GridSize);
            centerLineOffsetX = blueprintGridRect.Width / 2d;
            centerLineOffsetY = blueprintGridRect.Height / 2d;
            //majorGridOffsetX = GridSize / 2d;
            //majorGridOffsetY = GridSize / 2d;
            //double left = Math.Floor((double)bounds.Left.NormalizedValue * scale / GridSize) * GridSize;
            //double right = Math.Ceiling((double)bounds.Right.NormalizedValue * scale / GridSize) * GridSize;

            //double top = Math.Ceiling((double)bounds.Top.NormalizedValue * scale / GridSize) * GridSize;
            //double bottom = Math.Floor((double)bounds.Bottom.NormalizedValue * scale / GridSize) * GridSize;

            //majorGridOffsetX = Math.Abs(left) % (GridSize * MajorGridDivisions);
            //majorGridOffsetY = Math.Abs(bottom) % (GridSize * MajorGridDivisions);

            //centerLineOffsetX = left * -1d;
            //centerLineOffsetY = bottom * -1d;

            //blueprintGridRect = new Rect(0, 0, right - left, top - bottom);
            Height = blueprintGridRect.Height;
            Width = blueprintGridRect.Width;
            Canvas.SetLeft(this, blueprintGridRect.Width / -2d);
            Canvas.SetTop(this, blueprintGridRect.Height / -2d);
        }
        
        private DrawingBrush CreateGridBrush(bool showSubUnits)
        {
            var majorGridSize = GridSize * MajorGridDivisions; // Major lines every 5 squares (5cm)

            int subGridDivisions = UnitMode == UnitMode.Metric ? 10 : (Zoom > 4 ? 16 : (Zoom > 1 ? 8 : 4)); // 10mm or 16th of inch
            var subGridSize = GridSize / subGridDivisions; // Millimeter lines (1/10 of cm)

            var drawingGroup = new DrawingGroup();

            double mmPenSize = Math.Max(0.75 / Zoom, 0.1);
            double cmPenSize = Math.Max(1 / Zoom, 0.2);
            double majorPenSize = Math.Max(2 / Zoom, 0.3);
            // Create pens
            var mmPen = new Pen(MillimeterGridBrush, mmPenSize);
            var cmPen = new Pen(MinorGridBrush, cmPenSize);
            var majorPen = new Pen(MajorGridBrush, majorPenSize);

            double majorOffset = majorPenSize / 2d;

            var majorClipRect = new RectangleGeometry
            {
                Rect = new Rect(majorOffset, majorOffset, majorGridSize - (majorOffset *2d), majorGridSize - (majorOffset * 2d))
            };

            var minorLinesGroup = new DrawingGroup();
            minorLinesGroup.ClipGeometry = majorClipRect;

            // Draw millimeter grid lines if enabled
            if (showSubUnits)
            {
                for (double i = subGridSize; i < majorGridSize; i += subGridSize)
                {
                    // Skip positions where cm or major lines will be drawn
                    if (Math.Abs(i % GridSize) < 0.01 || Math.Abs(i % majorGridSize) < 0.01)
                        continue;

                    // Vertical mm lines
                    var verticalSubLine = new LineGeometry
                    {
                        StartPoint = new Point(i, 0),
                        EndPoint = new Point(i, majorGridSize)
                    };
                    minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = verticalSubLine, Pen = mmPen });

                    // Horizontal mm lines
                    var horizontalSubLine = new LineGeometry
                    {
                        StartPoint = new Point(0, i),
                        EndPoint = new Point(majorGridSize, i)
                    };
                    minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = horizontalSubLine, Pen = mmPen });
                }
            }


            // Draw major grid lines
            for (int i = 1; i < MajorGridDivisions; i++) // Skip 0 and last (major lines)
            {
                var offset = i * GridSize;

                // Vertical lines
                var verticalLine = new LineGeometry
                {
                    StartPoint = new Point(offset, 0),
                    EndPoint = new Point(offset, majorGridSize)
                };
                minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = verticalLine, Pen = cmPen });

                // Horizontal lines
                var horizontalLine = new LineGeometry
                {
                    StartPoint = new Point(0, offset),
                    EndPoint = new Point(majorGridSize, offset)
                };
                minorLinesGroup.Children.Add(new GeometryDrawing { Geometry = horizontalLine, Pen = cmPen });
            }
            var gridGeometry = new RectangleGeometry
            {
                Rect = new Rect(0, 0, majorGridSize, majorGridSize)
            };
            var majorSquare = new RectangleGeometry
            {
                Rect = new Rect(0, 0, majorGridSize, majorGridSize)
            };
            //drawingGroup.Children.Add(new GeometryDrawing { Geometry = gridGeometry, Brush = Brushes.Red });
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
                //Viewport = new Rect(0, 0, majorGridSize, majorGridSize),
                //ViewportUnits = BrushMappingMode.Absolute
            };
        }
    }
}
