using SiGen.Layouts;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using Svg;
using Svg.Pathing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Export
{
    public class SvgLayoutExporter : BaseLayoutExporter<SvgExportOptions>
    {
        private SvgDocument Document;
        private PointM OriginOffset;
        private float ScaleFactor = 1f;

        public SvgLayoutExporter(SvgExportOptions options, StringedInstrumentLayout layout) : base(options, layout)
        {
            Document = new SvgDocument
            {
                X = new SvgUnit(0),
                Y = new SvgUnit(0),
                Width = GetDocumentUnit(layout.Bounds!.Width),
                Height = GetDocumentUnit(layout.Bounds!.Height)
            };
            Document.ViewBox = new SvgViewBox(0, 0, Document.Width, Document.Height);
            //Document.ViewBox = new SvgViewBox(0, 0, (float)layout.Bounds!.Width[Options.Unit], (float)layout.Bounds!.Height[Options.Unit]);
            ScaleFactor = Document.ViewBox.Height / (float)layout.Bounds.Height[Options.Unit];

            OriginOffset = new PointM(layout.Bounds.Location.X * -1, layout.Bounds.Location.Y * -1);
        }

        protected override void ExportElement(ElementType elementType, PathBase path, LineExportOptions lineOptions)
        {
            var svgElement = GetSvgElementForPath(path);
            if (svgElement != null)
            {

                //svgElement.CustomAttributes.Add("elementType", elementType.ToString());
                ApplyStyle(svgElement, lineOptions);
                Document.Children.Add(svgElement);
                return;
            }
        }

        private SvgMarkerElement? GetSvgElementForPath(PathBase path)
        {
            if (path is LinearPath linear)
            {
                var startPt = PointM.FromVector(linear.Start) + OriginOffset;
                var endPt = PointM.FromVector(linear.End) + OriginOffset;
                var line = new SvgLine()
                {
                    StartX = GetDocumentUnit(startPt.X),
                    StartY = GetDocumentUnit(startPt.Y * -1d),
                    EndX = GetDocumentUnit(endPt.X),
                    EndY = GetDocumentUnit(endPt.Y * -1d),
                };
                return line;
            }
            else if (path is PolyLinePath poly)
            {
                var svgPoly = new SvgPath();
                svgPoly.PathData = new SvgPathSegmentList();
                svgPoly.PathData.Add(new SvgMoveToSegment(true, ToPointF(poly.Points[0])));

                for (int i = 0; i < poly.Points.Count - 1; i++)
                {
                    svgPoly.PathData.Add(new SvgLineSegment(false, ToPointF(poly.Points[i + 1])));
                }
                return svgPoly;
            }
            //else if (path is BezierPath bezier)
            //{
            //    var svgPath = new SvgPath();
            //    var startPt = PointM.FromVector(bezier.Start);
            //    svgPath.PathData.Add(new SvgMoveToSegment(new SvgPoint(GetDocumentUnit(startPt.X), GetDocumentUnit(startPt.Y))));
            //    for (int i = 0; i <= 20; i++)
            //    {
            //        var t = i / 20d;
            //        var pt = PointM.FromVector(bezier.Interpolate(t));
            //        svgPath.PathData.Add(new SvgLineSegment(new SvgPoint(GetDocumentUnit(pt.X), GetDocumentUnit(pt.Y))));
            //    }
            //    return svgPath;
            //}

            return null;
        }

        private void ApplyStyle(SvgElement svgElement, LineExportOptions lineOptions)
        {
            if (lineOptions.Color.HasValue)
            {
                svgElement.Stroke = new SvgColourServer(lineOptions.Color.Value);
                svgElement.Fill = SvgPaintServer.None;
            }
            else
            {
                svgElement.Stroke = new SvgColourServer(System.Drawing.Color.Black);
                svgElement.Fill = SvgPaintServer.None;
            }
            if (lineOptions.LineThickness.HasValue)
            {
                svgElement.StrokeWidth = GetSvgThickness(lineOptions.LineThickness.Value);
            }
            else
            {
                svgElement.StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 1);
            }
            if (lineOptions.Dashed)
            {
                svgElement.StrokeDashArray = new SvgUnitCollection()
                {
                    new SvgUnit(SvgUnitType.Pixel, 4),
                    new SvgUnit(SvgUnitType.Pixel, 2)
                };
            }
        }

        protected override void SaveToFile(string filePath)
        {
            Document.Write(filePath);
        }

        protected override void SaveToStream(Stream stream)
        {
            Document.Write(stream);
        }

        private SvgUnit GetDocumentUnit(Measure value)
        {
            var svgUnitType = LengthUnitToSvg(Options.Unit);
            var svgValue = value[Options.Unit];

            if (Options.Unit == LengthUnit.Ft)
            {
                svgUnitType = SvgUnitType.Inch;
                svgValue *= 12;
            }

            return new SvgUnit(svgUnitType, (float)svgValue);
        }

        private SvgUnit GetRelativeUnit(Measure value)
        {
            return new SvgUnit((float)value[Options.Unit]);
        }

        private PointF ToPointF(VectorD vector)
        {
            return ToPointF(PointM.FromVector(vector));
        }

        private PointF ToPointF(PointM point)
        {
            point += OriginOffset;
            var posX = GetRelativeUnit(point.X);
            var posY = GetRelativeUnit(point.Y * -1d);

            return new PointF(posX.Value * ScaleFactor, posY.Value * ScaleFactor);
        }

        private SvgUnitType LengthUnitToSvg(LengthUnit unit)
        {
            if (unit == LengthUnit.Mm)
                return SvgUnitType.Millimeter;
            else if (unit == LengthUnit.Cm)
                return SvgUnitType.Centimeter;
            else if (unit == LengthUnit.In)
                return SvgUnitType.Inch;
            //else if (unit == UnitOfMeasure.Feets)
            //    return SvgUnitType.f;

            return SvgUnitType.User;
        }

        private SvgUnit GetSvgThickness(LineThickness thickness)
        {
            SvgUnitType svgUnit = thickness.Unit switch
            {
                ThicknessUnit.Millimeter => SvgUnitType.Millimeter,
                ThicknessUnit.Point => SvgUnitType.Point,
                ThicknessUnit.Pixel => SvgUnitType.Pixel,
                _ => SvgUnitType.Pixel
            };
           
            return new SvgUnit(svgUnit, (float)thickness.Value);
        }
    }
}
