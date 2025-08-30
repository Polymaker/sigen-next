using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Export
{
    public class BaseExportOptions
    {
        public LengthUnit Unit { get; set; } = LengthUnit.Cm;
        public bool ExportFrets { get; set; } = true;
        public LineExportOptions FretLineOptions { get; set; } = new LineExportOptions();
        public bool ExtendFrets { get; set; }
        public Measure? FretExtensionAmount { get; set; }
        public bool ExportStrings { get; set; }
        public LineExportOptions StringLineOptions { get; set; } = new LineExportOptions();
        public bool UseStringThickness { get; set; }
        public bool ExportFingerboard { get; set; }
        public LineExportOptions FingerboardLineOptions { get; set; } = new LineExportOptions();
        public bool ExportCenterLine { get; set; }
        public bool ExportMedians { get; set; }
        public LineExportOptions GuideLinesOptions { get; set; } = new LineExportOptions();
    }

    public class SvgExportOptions : BaseExportOptions
    {
        
    }

    public class DxfExportOptions : BaseExportOptions
    {
        
    }
}
