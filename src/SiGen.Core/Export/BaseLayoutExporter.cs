using SiGen.Layouts;
using SiGen.Layouts.Elements;
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

    public interface ILayoutExporter
    {
        void ExportLayout(ExportTarget target);
    }

    public abstract class BaseLayoutExporter<TOptions> : ILayoutExporter
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
                ExportCenterLine();

            if (Options.ExportFingerboard)
                ExportFingerboard();

            if (Options.ExportFrets)
                ExportFrets();

            if (Options.ExportStrings)
                ExportStrings();

            if (Options.ExportMedians)
                ExportMedians();
        }

        private void ExportCenterLine()
        {
            var bounds = Layout.Bounds!;
            var centerLine = new LinearPath(
                new PointM(bounds.Left + bounds.Width / 2, bounds.Top).ToVector(),
                new PointM(bounds.Left + bounds.Width / 2, bounds.Bottom).ToVector()
            );

            ExportElement(ElementType.GuideLine, centerLine, Options.CenterLine);
        }

        private void ExportFrets()
        {
            int lastStringIndex = Layout.NumberOfStrings - 1;
            foreach (var fretElement in Layout.GetFretSegments())
            {
                if (fretElement.FretShape == null)
                    continue;

                var shape = fretElement.FretShape;
                if (!(fretElement.IsBridge || fretElement.IsNut) && Options.Frets.Extend && !Measure.IsNullOrEmpty(Options.Frets.ExtensionAmount))
                {
                    TrimExtendSide sides = TrimExtendSide.None;
                    if (fretElement.ContainsString(0)) sides |= TrimExtendSide.Start;
                    if (fretElement.ContainsString(lastStringIndex)) sides |= TrimExtendSide.End;
                    shape = shape.TrimExtend(sides, Options.Frets.ExtensionAmount.Value.NormalizedValue) ?? fretElement.FretShape;
                }

                if (shape != null)
                    ExportElement(ElementType.Fret, shape, Options.Frets);
            }
        }

        private void ExportFingerboard()
        {
            foreach (var edge in Layout.Elements.OfType<FingerboardEdgeElement>())
            {
                ExportElement(ElementType.Fingerboard, edge.Path, Options.Fingerboard);
            }

            if (Options.Fingerboard.ProjectionLines.Enabled)
            {
                foreach (var contLine in Layout.Elements.OfType<GuideLineElement>().Where(x => x.Type == GuideLineType.FretboardProjection))
                {
                    ExportElement(ElementType.Fingerboard, contLine.Path, Options.Fingerboard.ProjectionLines);
                }
            }
        }

        private void ExportStrings()
        {
            var stringLineOptions = new LineExportOptions()
            {
                Color = Options.Strings.Color,
                LineThickness = Options.Strings.LineThickness,
                Dashed = Options.Strings.Dashed
            };

            foreach (var stringElem in Layout.GetStrings())
            {
                if (Options.Strings.UseGauge)
                {
                    var stringGauge = stringElem.GetGauge();
                    stringLineOptions.LineThickness = stringGauge.HasValue
                        ? new LineThickness((double)stringGauge.Value[LengthUnit.Mm], ThicknessUnit.Millimeter)
                        : Options.Strings.LineThickness;
                }
                ExportElement(ElementType.String, stringElem.Path, stringLineOptions);
            }
        }

        private void ExportMedians()
        {
            foreach (var contLine in Layout.Elements.OfType<GuideLineElement>().Where(x => x.Type == GuideLineType.StringMedian))
            {
                ExportElement(ElementType.GuideLine, contLine.Path, Options.Medians);
            }
        }

        /// <summary>
        /// Exports a single element to the output format.
        /// </summary>
        /// <param name="elementType">The type of element being exported (for layer organization)</param>
        /// <param name="path">The geometric path to export</param>
        /// <param name="lineOptions">Styling options (color, thickness, dashes). Note: Enabled flag is already checked by the base exporter.</param>
        protected abstract void ExportElement(ElementType elementType, PathBase path, LineExportOptions lineOptions);

        protected abstract void SaveToFile(string filePath);

        protected virtual void SaveToStream(Stream stream)
        {
            throw new NotSupportedException($"{GetType().Name} does not support exporting to a stream.");
        }
    }

    //public static class LayoutExporterFactory
    //{
    //    public static BaseLayoutExporter<TOptions> CreateExporter<TOptions>(ExportTargetFormat targetFormat, TOptions options, StringedInstrumentLayout layout)
    //        where TOptions : BaseExportOptions
    //    {
    //        if (targetFormat == ExportTargetFormat.Svg)
    //        {
    //            if (options is not SvgExportOptions svgOptions)
    //                throw new ArgumentException($"Expected options of type {typeof(SvgExportOptions).Name} for SVG export, but got {options.GetType().Name}.");
    //            return new SvgLayoutExporter(svgOptions, layout);
    //        }
    //        // For now we only have one exporter implementation, but this factory allows us to easily add more in the future.
            
    //    }
    //}

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

    

}
