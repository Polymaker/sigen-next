using SiGen.Measuring;

namespace SiGen.Export
{
    public class PdfExportOptions : BaseExportOptions
    {
        public PdfPaperSize Paper { get; set; } = PdfPaperSize.Letter;
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;
        public PdfPageMargin ContentMargin { get; set; } = PdfPageMargin.Uniform(Measure.Mm(10));
        public bool EnableTiling { get; set; } = true;
        public Measure PageOverlap { get; set; } = Measure.Mm(10);
        //public Measure VerticalPageOverlap { get; set; } = Measure.Mm(10);
        public bool IncludeRegistrationMarks { get; set; } = true;
        //public bool IncludeCutGuides { get; set; } = true;
        //public bool IncludePageLabels { get; set; } = true;
        //public bool IncludeScaleCalibration { get; set; } = true;
        //public Measure ScaleCalibrationLength { get; set; } = Measure.Mm(50);
        public double OutputScale { get; set; } = 1d; //todo: maybe remove because why print at a different scale than 1:1?
    }

    public enum PdfPageOrientation
    {
        Portrait,
        Landscape,
    }

    public record PdfPaperSize(string Name, Measure Width, Measure Height)
    {
        // ISO A-series
        public static PdfPaperSize A0 => new("A0", Measure.Mm(841), Measure.Mm(1189));
        public static PdfPaperSize A1 => new("A1", Measure.Mm(594), Measure.Mm(841));
        public static PdfPaperSize A2 => new("A2", Measure.Mm(420), Measure.Mm(594));
        public static PdfPaperSize A3 => new("A3", Measure.Mm(297), Measure.Mm(420));
        public static PdfPaperSize A4 => new("A4", Measure.Mm(210), Measure.Mm(297));
        public static PdfPaperSize A5 => new("A5", Measure.Mm(148), Measure.Mm(210));
        // ISO B-series
        public static PdfPaperSize B4 => new("B4", Measure.Mm(250), Measure.Mm(353));
        public static PdfPaperSize B5 => new("B5", Measure.Mm(176), Measure.Mm(250));
        // North American
        public static PdfPaperSize Letter => new("Letter", Measure.In(8.5), Measure.In(11));
        public static PdfPaperSize Legal => new("Legal", Measure.In(8.5), Measure.In(14));
        public static PdfPaperSize Tabloid => new("Tabloid", Measure.In(11), Measure.In(17));

        public string DisplayDescription => $"{Name} ({Width} x {Height})";
    }

    public record PdfPageMargin(Measure Left, Measure Top, Measure Right, Measure Bottom)
    {
        public static PdfPageMargin Uniform(Measure value) => new(value, value, value, value);
        public Measure Vertical => Top + Bottom;
        public Measure Horizontal => Left + Bottom;
    }

    public record PdfLayoutPlan(
        RectangleM LayoutBounds,
        PdfPaperSize PageSize,
        RectangleM PrintableArea,
        SizeM TiledSize,
        IReadOnlyList<PdfLayoutPage> Pages,
        IReadOnlyList<PdfLayoutMarker> Markers,
        PdfExportOptions Options);

    public record PdfLayoutPage(
        int PageNumber,
        int Column,
        int Row,
        RectangleM LayoutArea,
        RectangleM PrintableArea,
        PdfLayoutPageMetadata Metadata);

    public record PdfLayoutPageMetadata(
        int PageNumber,
        int TotalPages,
        bool HasLeftOverlap,
        bool HasRightOverlap,
        bool HasTopOverlap,
        bool HasBottomOverlap);

    public record PdfLayoutMarker(
        PointM Position,
        PdfLayoutMarkerType Type,
        Measure Radius);

    public enum PdfLayoutMarkerType
    {
        Registration,
    }
}
