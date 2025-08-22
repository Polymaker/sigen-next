using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.Settings;
using SiGen.Utilities;
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
    /// <summary>
    /// Visual element for rendering a single string in the layout viewer.
    /// Handles drawing the string, its highlight, and applies theme settings.
    /// </summary>
    public class StringVisualElement : VisualElementBase<StringElement>, INotifyZoomChanged
    {
        private Line? _mainStringLine;
        private Line? selectionHighlight;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringVisualElement"/> class.
        /// </summary>
        /// <param name="element">The string element to visualize.</param>
        /// <param name="themeRenderSettings">Theme settings for rendering.</param>
        public StringVisualElement(StringElement element, ThemeRenderSettings themeRenderSettings)
            : base(element, themeRenderSettings)
        {
            BuildTooltip();
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            Classes.Add("overOnce");
        }

        /// <summary>
        /// Generates the visuals for the string, including highlight and main line.
        /// </summary>
        protected override void GenerateVisuals()
        {
            Children.Clear();
            var stringClipGeometry = GetStringClip();
            var stringGauge = Element.GetGauge();
            double stringThicknessPx = !Measuring.Measure.IsNullOrEmpty(stringGauge) ? stringGauge.ToPixels() : 1;
            var perpendicularLine = Element.Path.GetEquation().GetPerpendicular(Element.Path.Start);
            var highlightStart = (Element.Path.Start - perpendicularLine.Vector * (stringThicknessPx / CmScaleFactor) * 0.5d).ToAvalonia();
            var highlightEnd = (Element.Path.Start + perpendicularLine.Vector * (stringThicknessPx / CmScaleFactor) * 0.5d).ToAvalonia();
            var extendedPath = (LinearPath)Element.Path.Extend(0.2)!;

            var stringGradientBrush = new LinearGradientBrush
            {
                SpreadMethod = GradientSpreadMethod.Repeat,
                StartPoint = new RelativePoint(highlightStart.X, highlightStart.Y, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(highlightEnd.X, highlightEnd.Y, RelativeUnit.Absolute),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.FromArgb(60,0,0,0), Offset = 0 },
                    new GradientStop { Color = Color.FromArgb(0,200,200,200), Offset = 0.25 },
                    new GradientStop { Color = Color.FromArgb(80,255,255,255), Offset = 0.5 },
                    new GradientStop { Color = Color.FromArgb(80,255,255,255), Offset = 0.6 },
                    new GradientStop { Color = Color.FromArgb(0,200,200,200), Offset = 0.75 },
                    new GradientStop { Color = Color.FromArgb(40,0,0,0), Offset = 1 },
                },
            };

            CreateHighlight(stringThicknessPx);

            if (stringThicknessPx < 5)
            {
                Children.Add(new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(0, 200, 200, 200)),
                    StrokeThickness = Math.Max(5, stringThicknessPx * 1.5),
                    StartPoint = Element.Path.Start.ToAvalonia(),
                    EndPoint = Element.Path.End.ToAvalonia(),
                    Clip = stringClipGeometry
                });
            }

            _mainStringLine = new Line
            {
                Stroke = new SolidColorBrush(ThemeRenderSettings.StringColor),
                StrokeThickness = stringThicknessPx,
                StartPoint = extendedPath.Start.ToAvalonia(),
                EndPoint = extendedPath.End.ToAvalonia(),
                Clip = stringClipGeometry,
            };
            Children.Add(_mainStringLine);

            CreateStringShadowEffect();

            Children.Add(new Line
            {
                Stroke = stringGradientBrush,
                StrokeThickness = stringThicknessPx,
                StartPoint = extendedPath.Start.ToAvalonia(),
                EndPoint = extendedPath.End.ToAvalonia(),
                Clip = stringClipGeometry
            });
        }


        private void BuildTooltip()
        {
            if (Element.GroupIndex.HasValue)
            {
                var textBlock = new TextBlock();
                textBlock.Inlines = new InlineCollection();
                textBlock.Inlines!.Add(new Run($"{Lang.Resources.StringGroupLabel} {Element.StringIndex + 1}"));
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run($"{Lang.Resources.StringLabel} {Element.GroupIndex.Value + 1}"));
                ToolTip.SetTip(this, textBlock);
            }
            else
                ToolTip.SetTip(this, $"{Lang.Resources.StringLabel} {Element.StringIndex + 1}");
        }

        public override void UpdateTheme(ThemeRenderSettings theme)
        {
            base.UpdateTheme(theme);
            if (_mainStringLine != null)
                _mainStringLine.Stroke = new SolidColorBrush(ThemeRenderSettings.StringColor);
        }

        /// <summary>
        /// Creates a highlight effect for the string.
        /// </summary>
        /// <param name="stringThicknessPx">Thickness of the string in pixels.</param>
        private void CreateHighlight(double stringThicknessPx)
        {
            selectionHighlight = new Line
            {
                Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = stringThicknessPx + 8,
                StrokeLineCap = PenLineCap.Round,
                StartPoint = Element.Path.Start.ToAvalonia(),
                EndPoint = Element.Path.End.ToAvalonia(),
                IsHitTestVisible = false,
            };

            selectionHighlight.Classes.Add("highlight");
            Children.Add(selectionHighlight);

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
            });
        }

        /// <summary>
        /// Gets the geometry used to clip the string rendering between nut and bridge.
        /// </summary>
        /// <returns>Geometry for clipping, or null if unavailable.</returns>
        private Geometry? GetStringClip()
        {
            if (Element.Layout == null)
                return null;

            var nutBridgeFrets = Element.Layout.Elements.OfType<FretSegmentElement>()
                .Where(x => x.ContainsString(Element.StringIndex) && (x.IsNut || x.IsBridge)).ToList();

            var pathGeometry = new PathGeometry();
            var ctx = pathGeometry.Open();
            ctx.BeginFigure(nutBridgeFrets[0].FretShape!.GetFirstPoint().ToAvalonia(), true);
            ctx.LineTo(nutBridgeFrets[0].FretShape!.GetLastPoint().ToAvalonia());
            ctx.LineTo(nutBridgeFrets[1].FretShape!.GetLastPoint().ToAvalonia());
            ctx.LineTo(nutBridgeFrets[1].FretShape!.GetFirstPoint().ToAvalonia());
            ctx.EndFigure(true);

            pathGeometry.Transform = new TranslateTransform(0.001, 0.001);
            return pathGeometry;
        }

        /// <summary>
        /// Converts a string's normal vector to a rotation angle in degrees.
        /// </summary>
        public static PreciseDouble GetRotationAngleFromNormal(VectorD normal)
        {
            var angleRadians = MathD.Atan2(normal.Y, normal.X);
            var angleDegrees = angleRadians * 180.0 / Math.PI;
            return angleDegrees;
        }

        private const double CmScaleFactor = 37.7952755906; // 1 cm in pixels at 96 DPI

 

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Trace.WriteLine($"Clicked on string {Element.StringIndex}");
        }

        /// <summary>
        /// Creates a brush for wound string rendering.
        /// </summary>
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
            return new DrawingBrush
            {
                Drawing = drawingGroup,
                TileMode = TileMode.Tile,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                SourceRect = new RelativeRect(new Rect(0, -1, patternSize, patternSize + 2), RelativeUnit.Relative),
                DestinationRect = new RelativeRect(new Rect(0, -1, patternSize, patternSize + 2), RelativeUnit.Relative),
            };
        }

        public void ZoomChanged(double newZoom)
        {
            if (selectionHighlight == null)
                return;
            var stringGauge = Element.GetGauge();
            double stringThicknessPx = !Measuring.Measure.IsNullOrEmpty(stringGauge) ? stringGauge.ToPixels() : 1;
            selectionHighlight.StrokeThickness = stringThicknessPx + 12 / newZoom;
        }

        public void BeginZoomChange()
        {
            if (_mainStringLine != null)
                _mainStringLine.Effect = null;
        }

        public void EndZoomChange()
        {
            CreateStringShadowEffect();
        }

        private void CreateStringShadowEffect()
        {
            if (_mainStringLine != null)
            {
                _mainStringLine.Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(100, 0, 0, 0),
                    BlurRadius = 2,
                    OffsetX = 4,
                };
            }
        }
    }
}
