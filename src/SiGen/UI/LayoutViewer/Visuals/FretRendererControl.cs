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
    public class FretRendererControl : Control
    {
        private ILayoutViewerContext ViewerContext { get; }
        private StringedInstrumentLayout? Layout => ViewerContext.Layout;
        public ThemeRenderSettings RenderSettings => ViewerContext.RenderSettings;

        private readonly Dictionary<(int Bass, int Treble), Geometry> _clipGeometryCache = new();

        public FretRendererControl(ILayoutViewerContext context)
        {
            ViewerContext = context;
            context.RenderSettingsChanged += (s, e) =>
            {
                InvalidateVisual();
            };
        }

        public override void Render(DrawingContext context)
        {
            if (Layout == null) return;
            _clipGeometryCache.Clear();

            var fretSegments = Layout.Elements.OfType<FretSegmentElement>();
            foreach (var segment in fretSegments)
            {
                if (segment.FretShape == null) continue;
                var adjustedShape = segment.FretShape.Extend(0.25);

                if (adjustedShape == null) continue;
                var clipGeom = GetFretClipGeom(segment);
                var fretColorBrush = segment.IsNut ? new SolidColorBrush(RenderSettings.NutColor) :
                    (segment.IsBridge ? new SolidColorBrush(RenderSettings.BridgeColor) :
                    new SolidColorBrush(RenderSettings.FretColor));

                //var fretColor = segment.IsNut ? RenderSettings.NutColor :
                //    (segment.IsBridge ? RenderSettings.BridgeColor : RenderSettings.FretColor);

                var fretThickness = segment.IsNut || segment.IsBridge ? 2 : SiGen.Measuring.Measure.Mm(2).ToPixels();
                var fretPen = new Pen(fretColorBrush, fretThickness);
                Geometry? fretGeometry = null;
                if (adjustedShape is LinearPath linearPath)
                {
                    fretGeometry = new LineGeometry
                    {
                        StartPoint = linearPath.Start.ToAvalonia(),
                        EndPoint = linearPath.End.ToAvalonia()
                    };
                    //context.DrawLine(fretColorBrush, linearPath.Start.ToAvalonia(), linearPath.End.ToAvalonia(), fretThickness, clipGeom);
                }
                else if (adjustedShape is PolyLinePath polyLine)
                {
                    fretGeometry = new PolylineGeometry()
                    {
                        Points = polyLine.Points.Select(p => p.ToAvalonia()).ToList()
                    };
                    //context.DrawGeometry(fretColorBrush, polyLine.Points.Select(x => x.ToAvalonia()), fretThickness, clipGeom);
                }

                if (fretGeometry != null)
                {
                    DrawingContext.PushedState? clipState = null;
                    if (clipGeom != null)
                        clipState = context.PushGeometryClip(clipGeom);
                    context.DrawGeometry(null, fretPen, fretGeometry);
                    clipState?.Dispose();
                }
            }
        }

        //todo: cache clip geometries by covered string range
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
    }
}
