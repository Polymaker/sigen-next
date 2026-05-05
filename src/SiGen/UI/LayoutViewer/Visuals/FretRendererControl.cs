using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts;
using SiGen.Layouts.Elements;
using SiGen.Paths;
using SiGen.Settings;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public class FretRendererControl : Control, ILayoutRenderable
    {
        private ILayoutViewerContext ViewerContext { get; }
        private StringedInstrumentLayout? Layout => ViewerContext.Layout;
        public LayoutViewerColorScheme RenderSettings => ViewerContext.ColorScheme;

        private readonly Dictionary<(int Bass, int Treble), Geometry> _clipGeometryCache = new();

        public FretRendererControl(ILayoutViewerContext context)
        {
            ViewerContext = context;
        }

        public override void Render(DrawingContext context)
        {
            if (Layout == null) return;
            _clipGeometryCache.Clear();

            var fretSegments = Layout.Elements.OfType<FretSegmentElement>();

            // Group segments by color/thickness and string range to minimize Draw calls and Pen creation
            var groups = Layout.Elements.OfType<FretSegmentElement>()
                .Where(s => s.FretShape != null)
                .GroupBy(s => new { s.IsNut, s.IsBridge, s.BassStringIndex, s.TrebleStringIndex });

            foreach (var group in groups)
            {
                // 1. Setup the Pen once per group
                var color = group.Key.IsNut ? RenderSettings.NutColor :
                            group.Key.IsBridge ? RenderSettings.BridgeColor :
                            RenderSettings.FretColor;

                var thickness = group.Key.IsNut || group.Key.IsBridge ? 2 : SiGen.Measuring.Measure.Mm(2).ToPixels();
                var fretPen = new Pen(new SolidColorBrush(color), thickness);
                var clipGeom = GetFretClipGeom(group.First());
                // 2. Create the StreamGeometry for this group
                var streamGeom = new StreamGeometry();
                using (var sContext = streamGeom.Open())
                {
                    foreach (var segment in group)
                    {
                        var adjustedShape = segment.FretShape?.TrimExtend(TrimExtendSide.Start | TrimExtendSide.End, 0.25);
                        if (adjustedShape == null) continue;

                        if (adjustedShape is LinearPath linear)
                        {
                            sContext.BeginFigure(linear.Start.ToAvalonia(), isFilled: false);
                            sContext.LineTo(linear.End.ToAvalonia());
                            sContext.EndFigure(isClosed: false);
                        }
                        else if (adjustedShape is PolyLinePath poly)
                        {
                            var points = poly.Points.Select(p => p.ToAvalonia()).ToList();
                            if (points.Count > 0)
                            {
                                sContext.BeginFigure(points[0], isFilled: false);
                                for (int i = 1; i < points.Count; i++)
                                    sContext.LineTo(points[i]);
                                sContext.EndFigure(isClosed: false);

                            }
                        }
                        else if (adjustedShape is BezierSplinePath bezierSpline)
                        {
                            var segments = bezierSpline.GetSegments();
                            if (segments.Count == 0) continue;

                            sContext.BeginFigure(segments[0].P0.ToAvalonia(), isFilled: false);
                            foreach (var seg in segments)
                                sContext.CubicBezierTo(seg.P1.ToAvalonia(), seg.P2.ToAvalonia(), seg.P3.ToAvalonia());
                            sContext.EndFigure(isClosed: false);
                        }
                    }
                }

                DrawingContext.PushedState? clipState = null;
                if (clipGeom != null)
                    clipState = context.PushGeometryClip(clipGeom);
                context.DrawGeometry(null, fretPen, streamGeom);
                clipState?.Dispose();
            }
        }

        private Geometry? GetFretClipGeom(FretSegmentElement element)
        {
            var key = (element.BassStringIndex, element.TrebleStringIndex);
            if (_clipGeometryCache.TryGetValue(key, out var cachedGeom))
                return cachedGeom;

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
            ctx.BeginFigure(bp2.ToAvalonia(), true);
            ctx.LineTo(bp1.ToAvalonia());
            ctx.LineTo(tp1.ToAvalonia());
            ctx.LineTo(tp2.ToAvalonia());
            ctx.EndFigure(true);

            pathGeometry.Transform = new TranslateTransform(0.001, 0.001); //required otherwise the clip does not work properly
            _clipGeometryCache[key] = pathGeometry;
            return pathGeometry;
        }

        public void UpdateColorScheme(LayoutViewerColorScheme theme)
        {
            InvalidateVisual();
        }
    }
}
