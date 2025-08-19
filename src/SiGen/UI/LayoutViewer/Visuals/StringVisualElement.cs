using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public class StringVisualElement : VisualElementBase<StringElement>
    {

        public StringVisualElement(StringElement element) : base(element)
        {
            ToolTip.SetTip(this, $"{Lang.Resources.StringLabel} {element.StringIndex + 1}");
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            Classes.Add("overOnce");
        }

        protected override void GenerateVisuals()
        {
            Children.Clear();
            
            var stringClipGeom = GetStringClip();
            var stringGauge = Element.GetGauge();
            double stringThickness = !Measuring.Measure.IsNullOrEmpty(stringGauge) ? MeasureToPixels(stringGauge) : 1;
            

            var perpLine = Element.Path.GetEquation().GetPerpendicular(Element.Path.Start);
            var p1 = Convert(Element.Path.Start - perpLine.Vector * (stringThickness / CmScaleFactor) * 0.5d);
            var p2 = Convert(Element.Path.Start + perpLine.Vector * (stringThickness / CmScaleFactor) * 0.5d);

            var extendedPath = (LinearPath)Element.Path.Extend(0.2)!;

            var test2 = new LinearGradientBrush
            {
                SpreadMethod = GradientSpreadMethod.Repeat,
                StartPoint = new RelativePoint(p1.X, p1.Y, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(p2.X, p2.Y, RelativeUnit.Absolute),
                GradientStops = new GradientStops
                {
                    new GradientStop
                    {
                        Color = Color.FromArgb(60,0,0,0),
                        Offset = 0
                    },
                    new GradientStop
                    {
                        Color = Color.FromArgb(0,200,200,200),
                        Offset = 0.25
                    },
                    new GradientStop
                    {
                        Color = Color.FromArgb(100,255,255,255),
                        Offset = 0.5
                    },
                    new GradientStop
                    {
                        Color = Color.FromArgb(100,255,255,255),
                        Offset = 0.6
                    },
                    new GradientStop
                    {
                        Color = Color.FromArgb(0,200,200,200),
                        Offset = 0.75
                    },
                    new GradientStop
                    {
                        Color = Color.FromArgb(40,0,0,0),
                        Offset = 1
                    },
                },
                //Transform = new RotateTransform((double)GetRotationAngleFromNormal(Element.Path.Direction)),
                //TransformOrigin = new RelativePoint(0, stringThickness * 0.5, RelativeUnit.Relative)
            };

            //Children.Add(new Line
            //{
            //    Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
            //    StrokeThickness = stringThickness,
            //    StartPoint = Convert(extendedPath.Start) + new Point(4, 0),
            //    EndPoint = Convert(extendedPath.End) + new Point(4, 0),
            //    IsEnabled = false,
            //    IsHitTestVisible= false,
            //    Clip = stringClipGeom
            //});

            CreateHighlight(stringThickness);

            if (stringThickness < 5)
            {
                Children.Add(new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(0, 200, 200, 200)),
                    StrokeThickness = Math.Max(5, stringThickness * 1.5),
                    StartPoint = Convert(Element.Path.Start),
                    EndPoint = Convert(Element.Path.End),
                    Clip = stringClipGeom
                });
            }

            Children.Add(new Line
            {
                Stroke = new SolidColorBrush(Color.FromRgb(230,230,240)),
                StrokeThickness = stringThickness,
                StartPoint = Convert(extendedPath.Start),
                EndPoint = Convert(extendedPath.End),
                Clip = stringClipGeom,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(100, 0, 0, 0),
                    BlurRadius = 2,
                    OffsetX = 4
                }
            });
            Children.Add(new Line
            {
                Stroke = test2,
                StrokeThickness = stringThickness,
                StartPoint = Convert(extendedPath.Start),
                EndPoint = Convert(extendedPath.End),
                Clip = stringClipGeom
            });
        }

        private void CreateHighlight(double stringThickness)
        {
            var highlightGeom = new Line
            {
                Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = stringThickness + 8,
                StrokeLineCap = PenLineCap.Round,
                StartPoint = Convert(Element.Path.Start),
                EndPoint = Convert(Element.Path.End),
                IsHitTestVisible = false,
       
            };

            highlightGeom.Classes.Add("highlight");

            Children.Add(highlightGeom);

            double maxOpacity = 0.4;
            var fadeInAnim = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(150),
                FillMode = FillMode.Forward,
                Easing = new CubicEaseIn(),
            };
            fadeInAnim.Children.Add(new KeyFrame
            {
                Setters =
                {
                    new Setter(OpacityProperty, 0.0)
                },
                Cue = new Cue(0),
            });
            fadeInAnim.Children.Add(new KeyFrame
            {
                Setters =
                {
                    new Setter(OpacityProperty, maxOpacity)
                },
                Cue = new Cue(1),
            });
            var fadeOutAnim = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(1200),
                FillMode = FillMode.Forward,
                Easing = new CircularEaseOut(),
            };
            fadeOutAnim.Children.Add(new KeyFrame
            {
                Setters =
                {
                    new Setter(OpacityProperty, maxOpacity)
                },
                Cue = new Cue(0),
            });
            fadeOutAnim.Children.Add(new KeyFrame
            {
                Setters =
                {
                    new Setter(OpacityProperty, 0.0)
                },
                Cue = new Cue(1),
            });
            Styles.Add(new Style(x => x.OfType<StringVisualElement>().Not(y => y.Class(":pointerover")).Child().Class("highlight"))
            {
                Setters =
                {
                    new Setter(OpacityProperty, 0.0)
                }
            });
            Styles.Add(new Style(x => x.OfType<StringVisualElement>().Class("overOnce").Not(y => y.Class(":pointerover")).Child().Class("highlight"))
            {
                Animations =
                {
                    fadeOutAnim
                }
            });
            Styles.Add(new Style(x => x.OfType<StringVisualElement>().Class(":pointerover").Child().Class("highlight"))
            {
                Setters =
                {
                    new Setter(IsVisibleProperty, true)
                },
                Animations = {
                    fadeInAnim
                },
                //Setters =
                //{
                //    new Setter(Line.OpacityProperty, 0.5)
                //}
            });

            

        }

        private Geometry? GetStringClip()
        {
            if (Element.Layout == null)
                return null; 

            var nutBridgeFrets = Element.Layout.Elements.OfType<FretSegmentElement>()
                .Where(x => x.ContainsString(Element.StringIndex) && (x.IsNut || x.IsBridge)).ToList();

            var pathGeometry = new PathGeometry();
            var ctx = pathGeometry.Open();
            ctx.BeginFigure(Convert(nutBridgeFrets[0].FretShape!.GetFirstPoint()), true);
            ctx.LineTo(Convert(nutBridgeFrets[0].FretShape!.GetLastPoint()));
            ctx.LineTo(Convert(nutBridgeFrets[1].FretShape!.GetLastPoint()));
            ctx.LineTo(Convert(nutBridgeFrets[1].FretShape!.GetFirstPoint()));
            ctx.EndFigure(true);

            pathGeometry.Transform = new TranslateTransform(0.001, 0.001);
            return pathGeometry;
        }

        public static PreciseDouble GetRotationAngleFromNormal(VectorD normal)
        {
            // Convert to angle using atan2
            var angleRadians = MathD.Atan2(normal.Y, normal.X);

            // Convert radians to degrees
            var angleDegrees = angleRadians * 180.0 / Math.PI;

            return angleDegrees;
        }

        private const double CmScaleFactor = 37.7952755906; // 1 cm in pixels at 96 DPI
        private Point Convert(VectorD point)
        {
            return new Point((double)point.X * CmScaleFactor, (double)point.Y * CmScaleFactor);
        }

        private double MeasureToPixels(Measure measure)
        {
            return (double)measure.NormalizedValue * CmScaleFactor;
        }
        //public override void Render(DrawingContext context)
        //{
        //    var brush = CreateWoundStringBrush();// new SolidColorBrush(Color.FromRgb(30, 50, 80)); // Blueprint blue

        //    var renderBounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        //    context.FillRectangle(brush, renderBounds);
        //    base.Render(context);
        //}
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Trace.WriteLine($"Clicked on string {Element.StringIndex}");
        }
        private DrawingBrush CreateWoundStringBrush(double rotation = 0)
        {
            double patternSize = 10;
            int winds = 3;
            double thickness = 3.5;
            var drawingGroup = new DrawingGroup();

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)), thickness);
            pen.LineCap = PenLineCap.Round;
            var pen2 = new Pen(new SolidColorBrush(Color.FromArgb(50, 0,0,0)), thickness);
            pen2.LineCap = PenLineCap.Round;
            double startY = thickness / 2d;
            double endY = patternSize - thickness / 2d;
            double stride = patternSize / winds;
            for (int i = 0; i <= winds + 1; i++)
            {
                double posX = stride * -0.5 + stride * i;
                drawingGroup.Children.Add(new GeometryDrawing
                {
                    Pen = pen,
                    Geometry = new LineGeometry
                    {
                        StartPoint = new Point(posX, startY),
                        EndPoint = new Point(posX + stride /2, endY),
                    },
                });
                drawingGroup.Children.Add(new GeometryDrawing
                {
                    Pen = pen2,
                    Geometry = new LineGeometry
                    {
                        StartPoint = new Point(posX + thickness *0.80, startY),
                        EndPoint = new Point(posX + thickness * 0.80 + stride / 2, endY),
                    },
                });
            }

            //drawingGroup.Children.Add(new GeometryDrawing
            //{
            //    Pen = pen,
            //    Geometry = new LineGeometry
            //    {
            //        StartPoint = new Point(-2.5, 2.5),
            //        EndPoint = new Point(0, 7.5),
            //    },
            //});
            //drawingGroup.Children.Add(new GeometryDrawing
            //{
            //    Pen = pen,
            //    Geometry = new LineGeometry
            //    {
            //        StartPoint = new Point(2.5, 2.5),
            //        EndPoint = new Point(5, 7.5),
            //    },
            //});
            //drawingGroup.Children.Add(new GeometryDrawing
            //{
            //    Pen = pen,
            //    Geometry = new LineGeometry
            //    {
            //        StartPoint = new Point(7.5, 2.5),
            //        EndPoint = new Point(10, 7.5),
            //    },
            //});
            return new DrawingBrush
            {
                Drawing = drawingGroup,
                TileMode = TileMode.Tile,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                //Stretch = Stretch.Uniform,
                //TransformOrigin = new RelativePoint(patternSize / 2d, patternSize / 2d, RelativeUnit.Absolute),
                SourceRect = new RelativeRect(new Rect(0, -1, patternSize, patternSize + 2), RelativeUnit.Relative),
                DestinationRect = new RelativeRect(new Rect(0, -1, patternSize, patternSize + 2), RelativeUnit.Relative),
                
                //DestinationRect = new RelativeRect(new Rect(0, 0, majorGridSize, majorGridSize), RelativeUnit.Absolute),
                //Transform = new RotateTransform(rotation)
                //Viewport = new Rect(0, 0, majorGridSize, majorGridSize),
                //ViewportUnits = BrushMappingMode.Absolute
            };
        }
    }
}
