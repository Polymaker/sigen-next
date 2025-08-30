using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using SiGen.Layouts;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;

namespace SiGen.Export
{
    public class DxfLayoutExporter : BaseLayoutExporter<DxfExportOptions>
    {
        private DxfDocument Document;

        public DxfLayoutExporter(DxfExportOptions options, StringedInstrumentLayout layout) : base(options, layout)
        {
            Document = new DxfDocument();
            
            switch (options.Unit)
            {
                case Measuring.LengthUnit.Mm:
                    Document.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Millimeters;
                    break;
                case Measuring.LengthUnit.Cm:
                    Document.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Centimeters;
                    break;
                case Measuring.LengthUnit.In:
                    Document.DrawingVariables.InsUnits = netDxf.Units.DrawingUnits.Inches;
                    break;
            }
        }

        protected override void SaveToFile(string filePath)
        {
            Document.Save(filePath);
        }

        protected override void ExportElement(ElementType elementType, PathBase path, LineExportOptions lineOptions)
        {
            var dxfEntity = GetEntityForPath(path);
            if (dxfEntity != null)
            {
                dxfEntity.Layer = GetElementLayer(elementType);
                ApplyStyle(dxfEntity, lineOptions);
                Document.Entities.Add(dxfEntity);
                return;
            }
        }

        private Layer GetElementLayer(ElementType elementType)
        {
            var layerName = elementType switch
            {
                ElementType.Fret => "Frets",
                ElementType.String => "Strings",
                ElementType.Fingerboard => "Fingerboard",
                ElementType.GuideLine => "GuideLines",
                _ => "Other"
            };
            var layer = Document.Layers.FirstOrDefault(l => l.Name == layerName);
            if (layer == null)
            {
                layer = new Layer(layerName);
                Document.Layers.Add(layer);
            }
            return layer;
        }

        private netDxf.Entities.EntityObject? GetEntityForPath(PathBase path)
        {
            if (path is LinearPath linePath)
            {
                return new Line(ConvertToVector2(linePath.GetFirstPoint()), ConvertToVector2(linePath.GetLastPoint()));
            }
            else if (path is PolyLinePath polyPath)
            {
                var splinePoints = new List<Vector2>();
                foreach (var pt in polyPath.Points)
                    splinePoints.Add(ConvertToVector2(pt));
                return new netDxf.Entities.Polyline2D(splinePoints);
            }
            else if (path is BezierPath bezierPath)
            {
                var splinePoints = new List<Vector3>();
                for (int i = 0; i <= 20; i++)
                {
                    var t = i / 20d;
                    splinePoints.Add(ConvertToVector3(bezierPath.Interpolate(t)));
                }
                return new netDxf.Entities.Spline(splinePoints);
            }
            return null;
        }

        private void ApplyStyle(netDxf.Entities.EntityObject entity, LineExportOptions lineOptions)
        {
            if (lineOptions.Color.HasValue)
                entity.Color = new netDxf.AciColor(lineOptions.Color.Value.R, lineOptions.Color.Value.G, lineOptions.Color.Value.B);

            if (lineOptions.LineThickness.HasValue)
            {
                var thickness = lineOptions.LineThickness.Value;
                double thicknessMm = thickness.Unit switch
                {
                    ThicknessUnit.Millimeter => thickness.Value,
                    ThicknessUnit.Pixel => thickness.Value * 0.264583, // 1 px ≈ 0.264583 mm (assuming 96 DPI)
                    ThicknessUnit.Point => thickness.Value * 0.352778, // 1 pt ≈ 0.352778 mm
                    _ => thickness.Value
                };
                entity.Lineweight = (netDxf.Lineweight)System.Math.Min((int)(thicknessMm * 100), 200); // Max 2.11mm
            }

            if (lineOptions.Dashed)
                entity.Linetype = Linetype.Dashed;
        }


        private Vector2 ConvertToVector2(VectorD point)
        {
            return PointToVector(PointM.FromVector(point));
        }

        private Vector2 PointToVector(PointM point)
        {
            return new Vector2((double)point.X[Options.Unit], (double)point.Y[Options.Unit]);
        }

        private Vector3 ConvertToVector3(VectorD point)
        {
            var vector = ConvertToVector2(point);
            return new Vector3(vector.X, vector.Y, 0);
        }
    }
}
