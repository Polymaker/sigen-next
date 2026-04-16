using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SiGen.Data.Common;
using SiGen.Maths;
using SiGen.Measuring;
using System;
using System.ComponentModel;

namespace SiGen.UI.Controls
{
    public class GuitarStringRenderer : Control
    {
        private const double DPI = 96.0 * 1.5;
        private const double WRAP_RATIO = 0.4; // 40% of total gauge

        // Gradient stop positions for metallic brush
        private const double SHADOW_START = 0.0;
        private const double SHADOW_MID = 0.2;
        private const double SHADOW_END = 1.0;
        private const double BASE_MID = 0.6;

        private const double GUITAR_BALL_END_SIZE_IN = 0.157; // 0.157" diameter standard ball end size
        private const double GUITAR_BALL_END_HOLE_IN = 0.090; // 0.090" hole diameter for string core
        private const double BASS_BALL_END_SIZE_IN = 0.236; // 0.236" diameter for bass strings
        private const double BASS_BALL_END_HOLE_IN = 0.118; // 0.118" hole diameter for bass string core

        // Winding highlight gradient stops
        private const double WINDING_HIGHLIGHT_OPACITY = 0.5;
        private const double WINDING_HIGHLIGHT_CENTER = 0.5;
        private const double WINDING_HIGHLIGHT_FADE = 0.8;

        public static readonly StyledProperty<Measure?> GaugeProperty =
            AvaloniaProperty.Register<GuitarStringRenderer, Measure?>(nameof(Gauge));

        public static readonly StyledProperty<StringMaterialType?> MaterialProperty =
            AvaloniaProperty.Register<GuitarStringRenderer, StringMaterialType?>(nameof(Material));

        public static readonly StyledProperty<int> TrebleIndexProperty =
            AvaloniaProperty.Register<GuitarStringRenderer, int>(nameof(TrebleIndex), 0);

        public static readonly StyledProperty<bool> IsBassStringProperty =
            AvaloniaProperty.Register<GuitarStringRenderer, bool>(nameof(IsBassString));

        [TypeConverter(typeof(MeasureTypeConverter))]
        public Measure? Gauge
        {
            get => GetValue(GaugeProperty);
            set => SetValue(GaugeProperty, value);
        }

        public StringMaterialType? Material
        {
            get => GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public int TrebleIndex
        {
            get => GetValue(TrebleIndexProperty);
            set => SetValue(TrebleIndexProperty, value);
        }

        public bool IsBassString
        {
            get => GetValue(IsBassStringProperty);
            set => SetValue(IsBassStringProperty, value);
        }

        protected bool UseBassBallEnd => IsBassString || (Gauge.HasValue && Gauge.Value > Measuring.Measure.In(0.090));

        static GuitarStringRenderer()
        {
            AffectsRender<GuitarStringRenderer>(GaugeProperty);
            AffectsRender<GuitarStringRenderer>(MaterialProperty);
            AffectsRender<GuitarStringRenderer>(TrebleIndexProperty);
            AffectsRender<GuitarStringRenderer>(IsBassStringProperty);
        }

        protected double BallEndSize => (UseBassBallEnd ? BASS_BALL_END_SIZE_IN : GUITAR_BALL_END_SIZE_IN) * DPI;

        public GuitarStringRenderer()
        {
            //Effect = new DropShadowEffect
            //{
            //    Color = Color.FromArgb(100, 0, 0, 0),
            //    BlurRadius = 2,
            //    OffsetX = 2,
            //    OffsetY = 2
            //};
        }

        public override void Render(DrawingContext context)
        {
            if (Gauge.HasValue && Material.HasValue)
            {
                if (IsWoundString(Material.Value))
                {
                    RenderBallEnd(context);
                    //double offset = BallEndSize * (UseBassBallEnd ? 1.5 : 1.3);
                    RenderWoundString(context);
                }
                else
                {
                    RenderPlainString(context, BallEndSize * 1.4);
                    RenderBallEnd(context);
                }
            }

            base.Render(context);
        }


        private static bool IsWoundString(StringMaterialType material)
        {
            return material.ToString().Contains("Wound", StringComparison.InvariantCultureIgnoreCase);
        }

        private void RenderWoundString(DrawingContext context)
        {
            if (!Gauge.HasValue) return;


            double totalGaugePx = DPI * Gauge.Value[LengthUnit.In];
            double wrapThicknessPx = totalGaugePx * WRAP_RATIO;

            double circumference = Math.PI * totalGaugePx;
            double angleRadians = Math.Atan2(wrapThicknessPx, circumference);
            double angleDegrees = angleRadians * (180.0 / Math.PI);
            double spacingPx = wrapThicknessPx / Math.Cos(angleRadians);
            double drawnHeight = totalGaugePx - wrapThicknessPx;
            double startX = (BallEndSize / 2 * 1.75) + BallEndSize * 0.5;
            double availableWidth = Bounds.Width - startX;

            bool overwind = totalGaugePx <= BallEndSize * 0.4;
            bool needsTaper = totalGaugePx > BallEndSize * 0.45;

            double twistLength = Math.Max(0.15 * DPI, spacingPx * 5);
            double loopOverlap = totalGaugePx * 0.3;
            if (overwind || needsTaper)
                loopOverlap = overwind ? totalGaugePx * 0.45 : totalGaugePx * 0.1;

            twistLength += loopOverlap;
            startX -= loopOverlap;
            availableWidth += loopOverlap;
            double slantOffset = drawnHeight * Math.Tan(angleDegrees * (Math.PI / 180.0));
            StreamGeometry stringGeometry = CreateWoundStringGeometry(drawnHeight, wrapThicknessPx, spacingPx, slantOffset, twistLength, availableWidth);

            var metallicBrush = CreateMetallicBrush(Material ?? StringMaterialType.SteelPlain);
            var lineCap = /*Material == StringMaterialType.SteelFlatwound ? PenLineCap.Square :*/ PenLineCap.Round;

            var pen1 = new Pen(metallicBrush, wrapThicknessPx, lineCap: lineCap);
            var pen2 = new Pen(CreateWindingHighlightBrush(wrapThicknessPx, spacingPx, angleDegrees), wrapThicknessPx, lineCap: PenLineCap.Round);

            var clipGeom = new RectangleGeometry(new Rect(0, 0, Bounds.Width, Bounds.Height));
            var state = context.PushGeometryClip(clipGeom);
            using (context.PushTransform(Matrix.CreateTranslation(startX, Bounds.Height / 2.0)))
            {
                context.DrawGeometry(null, pen1, stringGeometry);
                context.DrawGeometry(null, pen2, stringGeometry);
            }
            state.Dispose();
        }

        private void RenderPlainString(DrawingContext context, double offsetX)
        {
            double totalGaugePx = DPI * Gauge!.Value[LengthUnit.In];
            var metallicBrush = CreateMetallicBrush(Material ?? StringMaterialType.SteelPlain);
            var rect = new Rect(offsetX, (Bounds.Height - totalGaugePx) / 2.0, Bounds.Width - offsetX, totalGaugePx);
            context.FillRectangle(metallicBrush, rect);
        }

        private void RenderBallEnd(DrawingContext context)
        {
            // 1. Create a 3D-effect Radial Brush
            var ballColor = GetBallEndColor(TrebleIndex);
            var colorBrush = new SolidColorBrush(ballColor);
            var highlightBrush = new RadialGradientBrush
            {
                Center = new RelativePoint(0.2, 0.2, RelativeUnit.Relative), // Light from top-left
                Opacity = 0.9,
                RadiusX = new RelativeScalar(0.8, RelativeUnit.Relative),
                RadiusY = new RelativeScalar(0.8, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Colors.White, 0.0),      // Specular highlight
                    new GradientStop(Color.Parse("#11FFFFFF"), 0.4), 
                    new GradientStop(Color.Parse("#44000000"), 1.0) // Edge shadow
                }
            };

            double ballEndDiameter = (UseBassBallEnd ? BASS_BALL_END_SIZE_IN : GUITAR_BALL_END_SIZE_IN) * DPI;
            double ballEndHole = (UseBassBallEnd ? BASS_BALL_END_HOLE_IN : GUITAR_BALL_END_HOLE_IN) * DPI;
            var ballGeom = CreateHollowBallEnd(ballEndDiameter, ballEndHole);
            using (context.PushTransform(Matrix.CreateTranslation(ballEndDiameter * 0.5, Bounds.Height / 2.0)))
            {
                // 1. Draw the wire loop and twist first (behind the ball)
                RenderWireLoopAndTwist(context, ballEndDiameter);

                context.DrawGeometry(colorBrush, null, ballGeom);
                context.DrawGeometry(highlightBrush, null, ballGeom);
            }
        }

        private void RenderWireLoopAndTwist(DrawingContext context, double ballEndSize)
        {
            // The core wire diameter is usually small, roughly 0.012" - 0.016"
            double coreWireThickness = 0.014 * DPI;
            double stringSizePx = Gauge!.Value[LengthUnit.In] * DPI;
            bool isWound = IsWoundString(Material!.Value);
            if (isWound)
                coreWireThickness = Math.Min(stringSizePx * 0.4, UseBassBallEnd ? 0.03 * DPI : 0.02 * DPI);
            else
                coreWireThickness = stringSizePx;

            var settings = GetMaterialSettings(isWound ? StringMaterialType.SteelPlain : Material!.Value);
            var twistBrush = new SolidColorBrush(settings.baseColor);
            var wireBrush = CreateMetallicBrush(Material.Value);
            //wireBrush.SpreadMethod = GradientSpreadMethod.Repeat;
            var twistPen = new Pen(twistBrush, coreWireThickness, lineCap: PenLineCap.Round);

            double ballRadius = ballEndSize / 2.0;
            double twistLength = 0.15 * DPI; // Length of the twisted section
            double startX = ballRadius * 1.75; // Start from the center of the ball
            double endX = startX + twistLength;
            double twistSize = stringSizePx * 0.6;

            // 1. Draw the Loop (The wire wrapping around the ball)
            // We draw two lines from the top/bottom of the ball meeting at the twist start
            //wireBrush.Transform = new RotateTransform(35 * TrebleIndex);
            //wireBrush.TransformOrigin = new RelativePoint(new Point(0.5, 0.5), RelativeUnit.Relative);

            context.DrawLine(twistPen, new Point(coreWireThickness * 0.7, -ballRadius + coreWireThickness * 0.6), new Point(startX, 0));
            context.DrawLine(twistPen, new Point(coreWireThickness * 0.7, ballRadius - coreWireThickness * 0.6), new Point(startX, 0));
            
            int twistTurns = 3;
            double segmentWidth = twistLength / twistTurns;

            if (!isWound)// 2. Draw the Lock Twist (The braided look)
            {
                for (int i = 1; i < twistTurns; i += 2)
                {
                    double x = startX + (i * segmentWidth);
                    context.DrawLine(twistPen, new Point(x, -twistSize), new Point(x + segmentWidth, twistSize));
                }
            }

            //context.DrawLine(wirePen, new Point(startX, 0), new Point(startX + ballRadius * 0.5, 0));
            var rect = new Rect(startX, coreWireThickness * -0.5, endX - startX, coreWireThickness);
            context.FillRectangle(wireBrush, rect);

            if (!isWound) // 2. Draw the Lock Twist (The braided look)
            {
                for (int i = 0; i < twistTurns; i += 2)
                {
                    double x = startX + (i * segmentWidth);
                    context.DrawLine(twistPen, new Point(x, twistSize), new Point(x + segmentWidth, -twistSize));
                }
            }
            
        }

        public void DrawGradientLine(DrawingContext context, Pen pen, Point lineStart, Point lineEnd)
        {
            if (pen.Brush is not LinearGradientBrush lgb)
                return;

            // 1. Calculate the direction vector of the line
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.0001) return; // Avoid division by zero

            // 2. Calculate the unit normal vector (perpendicular to the line)
            // We swap X and Y and negate one to get the perpendicular direction
            double ux = -dy / length;
            double uy = dx / length;

            // 3. Determine the distance from the center to the edge
            double halfThickness = pen.Thickness / 2.0;

            // 4. Pick a reference point (the midpoint of the line is safest)
            Point mid = new Point(
                (lineStart.X + lineEnd.X) / 2.0,
                (lineStart.Y + lineEnd.Y) / 2.0
            );

            // 5. Set StartPoint and EndPoint perpendicular to the line flow
            // StartPoint is on one edge, EndPoint is on the other
            lgb.StartPoint = new RelativePoint(
                mid.X + ux * halfThickness,
                mid.Y + uy * halfThickness,
                RelativeUnit.Absolute);

            lgb.EndPoint = new RelativePoint(
                mid.X - ux * halfThickness,
                mid.Y - uy * halfThickness,
                RelativeUnit.Absolute);

            context.DrawLine(pen, lineStart, lineEnd);
        }

        #region Geometries

        ///
        private StreamGeometry CreateWoundStringGeometry(double drawnHeight, double wireSize, double spacingPx, double slantOffset, double twistLength, double availableWidth)
        {
            StreamGeometry stringGeometry = new StreamGeometry();
            double stringGauge = drawnHeight + wireSize;

            bool overwind = stringGauge <= BallEndSize * 0.4;
            bool needsTaper = stringGauge > BallEndSize * 0.45;
            //bool overwind = stringGauge <= BallEndSize * 0.4;
            //bool needsTaper = stringGauge > (0.070 * DPI); // Threshold for thick bass strings

            double overwindSize = (double)MathD.Map(0.018 * DPI, BallEndSize * 0.4, wireSize, wireSize * 0.4, stringGauge);

            using (StreamGeometryContext geometryContext = stringGeometry.Open())
            {
                double currentX = 0;
                int numWraps = (int)Math.Ceiling(availableWidth / spacingPx);

                for (int i = 0; i < numWraps; i++)
                {
                    double topY = drawnHeight * -0.5;
                    double bottomY = drawnHeight * 0.5;
                    if (overwind && currentX <= twistLength)
                    {
                        topY -= overwindSize;
                        bottomY += overwindSize;
                    }
                    // 2. Logic for Taper (Heavy strings)
                    else if (needsTaper && currentX < twistLength)
                    {
                        // If we are before the taper, stay at "core" size
                        // If in the taper zone, interpolate to full height
                        double coreHeight = stringGauge * 0.4; // Simulate the core/inner wrap
                        double t = currentX / twistLength;
                        t = Math.Clamp(t, 0, 1); // Normalize 0.0 to 1.0

                        double currentHeight = coreHeight + (drawnHeight - coreHeight) * t;
                        topY = currentHeight * -0.5;
                        bottomY = currentHeight * 0.5;
                    }

                    geometryContext.BeginFigure(new Point(currentX, topY), false);
                    geometryContext.LineTo(new Point(currentX + slantOffset, bottomY));
                    geometryContext.EndFigure(false);

                    currentX += spacingPx;
                }
            }

            return stringGeometry;
        }

        public static StreamGeometry CreateHollowBallEnd(double outerDiam, double innerDiam)
        {
            static void AddCircle(StreamGeometryContext context, double radius)
            {
                // Simple way to draw a circle in a StreamContext
                context.BeginFigure(new Point(0, -radius), isFilled: true);
                context.ArcTo(new Point(0, radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
                context.ArcTo(new Point(0, -radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
                context.EndFigure(isClosed: true);
            }

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.SetFillRule(FillRule.EvenOdd); // Use EvenOdd to create the "Donut" effect
                AddCircle(context, outerDiam / 2); // Draw Outer Circle
                AddCircle(context, innerDiam / 2); // Draw Inner Circle
            }
            return geometry;
        }

        #endregion

        public IBrush CreateMetallicBrush(StringMaterialType material)
        {
            var settings = GetMaterialSettings(material);

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(settings.shadow, SHADOW_START),
                    new GradientStop(settings.baseColor, SHADOW_MID),
                    new GradientStop(settings.highlight, settings.highlightPos),
                    new GradientStop(settings.baseColor, BASE_MID),
                    new GradientStop(settings.shadow, SHADOW_END)
                },
            };
        }

        public LinearGradientBrush CreateTestBrush()
        {

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Colors.Yellow, 0),
                    new GradientStop(Colors.Blue, 0.25),
                    new GradientStop(Colors.Red, 0.50),
                    new GradientStop(Colors.Green, 0.75),
                    new GradientStop(Colors.Pink, 1)
                },
                
            };
        }

        private static (Color baseColor, Color shadow, Color highlight, double highlightPos) GetMaterialSettings(StringMaterialType material)
        {
            return material switch
            {
                StringMaterialType.SteelPlain =>
                    (Color.Parse("#A8A8A8"), Color.Parse("#444444"), Colors.White, 0.3),

                //StringMaterialType.SteelWound =>
                //    (Color.Parse("#B0B0B0"), Color.Parse("#333333"), Colors.White, 0.35),

                //StringMaterialType.SteelFlatwound =>
                //    (Color.Parse("#8E8E8E"), Color.Parse("#2A2A2A"), Color.Parse("#D0D0D0"), 0.4),

                StringMaterialType.NickelWound =>
                    (Color.Parse("#9DA2A8"), Color.Parse("#303235"), Color.Parse("#E8EAED"), 0.35),

                StringMaterialType.BronzeWound =>
                    (Color.Parse("#CD7F32"), Color.Parse("#4A2E12"), Color.Parse("#FFD39B"), 0.35),

                //StringMaterialType.PhosphorBronzeWound =>
                //    (Color.Parse("#B87333"), Color.Parse("#3D1F0A"), Color.Parse("#FFB07C"), 0.35),

                StringMaterialType.NylonPlain =>
                    (Color.Parse("#E0E0E0"), Color.Parse("#A0A0A0"), Colors.White, 0.45), // Low contrast

                //StringMaterialType.GutPlain =>
                //    (Color.Parse("#D2B48C"), Color.Parse("#8B7355"), Color.Parse("#F5DEB3"), 0.5),

                StringMaterialType.SilverPlatedWound =>
                    (Color.Parse("#C0C0C0"), Color.Parse("#444444"), Colors.White, 0.3),

                _ => (Colors.Gray, Colors.Black, Colors.White, 0.5)
            };
        }

        public static Color GetBallEndColor(int stringIndex)
        {
            // D'Addario Color sequence from High E to Low E:
            // 1(E):Silver, 2(B):Purple, 3(G):Green, 4(D):Black, 5(A):Red, 6(E):Brass

            // Since your index 0 is the BASS (thickest), we define the colors 
            // in reverse order (thickest to thinnest).
            stringIndex = Math.Max(0, stringIndex); // Ensure non-negative index

            var colors = new[]
            {
                Color.Parse("#C0C0C0"), // E (Silver)
                Color.Parse("#800080"), // B (Purple)
                Color.Parse("#008000"), // G (Green)
                Color.Parse("#101010"), // D (Black)
                Color.Parse("#FF0000"), // A (Red)
                Color.Parse("#D4AF37"), // E (Brass/Gold)
                Color.Parse("#B87333"), // Low B (Copper/Bronze)
                Color.Parse("#F3F300"), // Low F# (Yellow)
                Color.Parse("#FF8899"), // Pink
            };

            // Use modulo to loop if the user creates a 9+ string instrument
            return colors[stringIndex % colors.Length];
        }

        public IBrush CreateWindingHighlightBrush(double wrapThicknessPx, double spacingPx, double angleDegrees)
        {
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(spacingPx * 0.5, 0, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(spacingPx * 0.5 + wrapThicknessPx, 0, RelativeUnit.Absolute),
                Opacity = WINDING_HIGHLIGHT_OPACITY,
                GradientStops = new GradientStops
                {
                    new GradientStop(Colors.Black, 0.0),
                    new GradientStop(Color.FromArgb(100, 255, 255, 255), WINDING_HIGHLIGHT_CENTER),
                    new GradientStop(Colors.Black, WINDING_HIGHLIGHT_FADE),
                },
                SpreadMethod = GradientSpreadMethod.Repeat,
                Transform = new RotateTransform(-angleDegrees),
            };
        }
    }
}


