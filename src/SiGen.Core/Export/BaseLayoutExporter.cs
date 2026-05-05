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
    public enum ExportTargetType
    {
        File, Stream
    }

    public class ExportTarget
    {
        public ExportTargetType Type { get; }
        public string? FilePath { get; }
        public Stream? Stream { get; }

        private ExportTarget(ExportTargetType type, string? filePath = null, Stream? stream = null)
        {
            Type = type;
            FilePath = filePath;
            Stream = stream;
        }

        public static ExportTarget ToFile(string filePath) => new ExportTarget(ExportTargetType.File, filePath: filePath);
        public static ExportTarget ToStream(Stream stream) => new ExportTarget(ExportTargetType.Stream, stream: stream);
    }

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
            ExportLayout(ExportTarget.ToFile(filePath));
        }

        public virtual void ExportLayout(ExportTarget target)
        {
            ExportElements();
            SaveToTarget(target);
        }

        protected virtual void SaveToTarget(ExportTarget target)
        {
            switch (target.Type)
            {
                case ExportTargetType.File:
                    SaveToFile(target.FilePath ?? throw new InvalidOperationException("File export target is missing a file path."));
                    break;

                case ExportTargetType.Stream:
                    SaveToStream(target.Stream ?? throw new InvalidOperationException("Stream export target is missing a stream."));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target));
            }
        }

        protected void ExportElements()
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
        }

        private void ExportFrets()
        {
            int lastStringIndex = Layout.NumberOfStrings - 1;
            foreach (var fretElement in Layout.GetFretSegments())
            {
                if (fretElement.FretShape == null)
                    continue;

                var shape = fretElement.FretShape;
                if (!(fretElement.IsBridge || fretElement.IsNut) && Options.ExtendFrets && !Measure.IsNullOrEmpty(Options.FretExtensionAmount))
                {
                    TrimExtendSide sides = TrimExtendSide.None;
                    if (fretElement.ContainsString(0)) sides |= TrimExtendSide.Start;
                    if (fretElement.ContainsString(lastStringIndex)) sides |= TrimExtendSide.End;
                    shape = shape.TrimExtend(sides, Options.FretExtensionAmount.Value.NormalizedValue) ?? fretElement.FretShape;
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

        protected virtual void SaveToStream(Stream stream)
        {
            throw new NotSupportedException($"{GetType().Name} does not support exporting to a stream.");
        }
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
