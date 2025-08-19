using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using SiGen.Layouts.Elements;
using SiGen.Paths;
using SiGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.UI.LayoutViewer.Visuals
{
    public class FretSegmentVisual : VisualElementBase<FretSegmentElement>
    {
        public FretSegmentVisual(FretSegmentElement element) : base(element)
        {
        }

        override protected void GenerateVisuals()
        {
            if (Element.FretShape == null) return;

            var adjustedShape = Element.FretShape?.Extend(0.25);
            if (adjustedShape == null) return;


            var clipGeom = GetFretClipGeom(Element);

            var color = Element.IsNut ? Brushes.Brown : Brushes.Silver;
            var fretThickness = Element.IsNut || Element.IsBridge ? 2 : SiGen.Measuring.Measure.Mm(2).ToPixels();
            if (adjustedShape is LinearPath linearPath)
            {
                Children.Add(new Line
                {
                    Stroke = color,
                    StrokeThickness = fretThickness,
                    StartPoint = linearPath.Start.ToAvalonia(),
                    EndPoint = linearPath.End.ToAvalonia(),
                    Clip = clipGeom,

                });
            }
            else if (adjustedShape is PolyLinePath polyLine)
            {
                Children.Add(new Polyline
                {
                    Stroke = color,
                    StrokeThickness = fretThickness,
                    Points = polyLine.Points.Select(x=>x.ToAvalonia()).ToArray()
                });
            }
        }

        //todo: cache clip geometries by covered string range
        private Geometry? GetFretClipGeom(FretSegmentElement element)
        {
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

            pathGeometry.Transform = new TranslateTransform(0.001, 0.001);
            return pathGeometry;
        }
    }

}
