using SiGen.Layouts;
using SiGen.Measuring;
using SiGen.Paths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Export
{
    public abstract class BaseLayoutExporter<TOptions>
        where TOptions : BaseExportOptions
    {
        public TOptions Options { get; }

        public StringedInstrumentLayout Layout { get; }

        public BaseLayoutExporter(TOptions options, StringedInstrumentLayout layout)
        {
            Options = options;
            Layout = layout;
        }

        public void ExportLayout(string filePath)
        {
            if (Options.ExportCenterLine)
            {
                var bounds = Layout.Bounds!;
                var centerLine = new LinearPath(
                    new PointM(bounds.Left + bounds.Width / 2, bounds.Top).ToVector(),
                    new PointM(bounds.Left + bounds.Width / 2, bounds.Bottom).ToVector()
                );
                var centerLineOptions = new LineExportOptions()
                {
                    Color = Color.Blue,
                    LineThickness = new LineThickness(1, ThicknessUnit.Point),
                    Dashed = true
                };
                ExportElement(ElementType.GuideLine, centerLine, centerLineOptions);
            }

            if (Options.ExportFingerboard)
                ExportFingerboard();

            if (Options.ExportFrets)
                ExportFrets();

            if (Options.ExportStrings)
                ExportStrings();



            SaveToFile(filePath);
        }

        private void ExportFrets()
        {

            foreach (var fretElement in Layout.GetFretSegments())
            {
                if (fretElement.FretShape == null)
                    continue;

                var shape = fretElement.FretShape;
                if (!(fretElement.IsBridge || fretElement.IsNut) && Options.ExtendFrets && !Measure.IsNullOrEmpty(Options.FretExtensionAmount))
                {
                    shape = shape.Extend(Options.FretExtensionAmount.Value.NormalizedValue) ?? fretElement.FretShape;
                }

                if (shape != null)
                    ExportElement(ElementType.Fret, shape, Options.FretLineOptions);
            }
        }

        private void ExportFingerboard()
        {
            foreach (var edge in Layout.Elements.OfType<FingerboardEdgeElement>())
            {
                ExportElement(ElementType.Fingerboard, edge.Path, Options.FretLineOptions);
            }
        }

        private void ExportStrings()
        {
            var stringLineOptions = new LineExportOptions()
            {
                Color = Options.StringLineOptions.Color,
                LineThickness = Options.StringLineOptions.LineThickness,
                Dashed = Options.StringLineOptions.Dashed
            };

            foreach (var stringElem in Layout.GetStrings())
            {
                if (Options.UseStringThickness)
                {
                    var stringGauge = stringElem.GetGauge();
                    stringLineOptions.LineThickness = stringGauge.HasValue
                        ? new LineThickness((double)stringGauge.Value[LengthUnit.Mm], ThicknessUnit.Millimeter)
                        : Options.StringLineOptions.LineThickness;
                }
                ExportElement(ElementType.String, stringElem.Path, stringLineOptions);
            }
        }

        protected abstract void ExportElement(ElementType elementType, PathBase path, LineExportOptions lineOptions);

        protected abstract void SaveToFile(string filePath);
    }

    public enum ElementType
    {
        Fret,
        String,
        Fingerboard,
        //CenterLine,
        GuideLine
    }

    public enum ThicknessUnit
    {
        Pixel,
        Point,
        Millimeter,
    }

    public struct LineThickness
    {
        public double Value { get; set; }
        public ThicknessUnit Unit { get; set; }

        public LineThickness(double value, ThicknessUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public override string ToString() => $"{Value} {Unit}";
    }

    public class LineExportOptions
    {
        public LineThickness? LineThickness { get; set; }// = new LineThickness(0.1, ThicknessUnit.Millimeter);
        public Color? Color { get; set; } = System.Drawing.Color.Black;
        public bool Dashed { get; set; } = false;
    }
}
