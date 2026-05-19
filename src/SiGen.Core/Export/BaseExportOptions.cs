using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SiGen.Export
{
    public class BaseExportOptions
    {
        public LengthUnit Unit { get; set; } = LengthUnit.Cm;

        public FretExportOptions Frets { get; set; } = new();
        public StringExportOptions Strings { get; set; } = new();
        public FingerboardExportOptions Fingerboard { get; set; } = new();
        public ElementExportOptions CenterLine { get; set; } = new();
        public ElementExportOptions Medians { get; set; } = new();

        #region Export flags accessors

        [JsonIgnore]
        public bool ExportFrets
        {
            get => Frets.Enabled;
            set => Frets.Enabled = value;
        }

        [JsonIgnore]
        public bool ExportStrings
        {
            get => Strings.Enabled;
            set => Strings.Enabled = value;
        }

        [JsonIgnore]
        public bool UseStringGauge
        {
            get => Strings.UseGauge;
            set => Strings.UseGauge = value;
        }

        [JsonIgnore]
        public bool ExportFingerboard
        {
            get => Fingerboard.Enabled;
            set => Fingerboard.Enabled = value;
        }

        [JsonIgnore]
        public bool ExportFingerboardProjectionLines
        {
            get => Fingerboard.ProjectionLines.Enabled;
            set => Fingerboard.ProjectionLines.Enabled = value;
        }

        [JsonIgnore]
        public bool ExportCenterLine
        {
            get => CenterLine.Enabled;
            set => CenterLine.Enabled = value;
        }

        [JsonIgnore]
        public bool ExportMedians
        {
            get => Medians.Enabled;
            set => Medians.Enabled = value;
        }

        #endregion
        
    }

    public class SvgExportOptions : BaseExportOptions
    {
        public bool InkscapeCompatible { get; set; } = true;
    }

    public class DxfExportOptions : BaseExportOptions
    {
        
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
        public System.Drawing.Color? Color { get; set; } = System.Drawing.Color.Black;
        public bool Dashed { get; set; } = false;
    }

    public class ElementExportOptions : LineExportOptions
    {
        public bool Enabled { get; set; }
    }

    public class FretExportOptions : ElementExportOptions
    {
        public bool Extend { get; set; }
        public Measure? ExtensionAmount { get; set; }
    }

    public class StringExportOptions : ElementExportOptions
    {
        public bool UseGauge { get; set; }
    }

    public class FingerboardExportOptions : ElementExportOptions
    {
        public ElementExportOptions ProjectionLines { get; set; } = new();
    }

    public enum ExportTargetFormat
    {
        Svg,
        Dxf,
        Pdf
    }
}
