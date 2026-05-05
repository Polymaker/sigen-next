using SiGen.Layouts;
using SiGen.Measuring;
using SiGen.Paths;
using SkiaSharp;

namespace SiGen.Export
{
    public class PdfLayoutExporter : BaseLayoutExporter<PdfExportOptions>
    {
        private const float PointsPerInch = 72f;
        private const float PointsPerCm = PointsPerInch / 2.54f;
        private static readonly Measure MarkerRadius = Measure.Mm(3);
        private static readonly Measure MarkerEdgeInset = Measure.Mm(8);

        public PdfLayoutExporter(PdfExportOptions options, StringedInstrumentLayout layout) : base(options, layout)
        {
        }

        public override void ExportLayout(ExportTarget target)
        {
            var plan = BuildPlan();
            using var document = CreatePdfDocument(target);

            foreach (var page in plan.Pages)
            {
                var pageWidth = ToPoints(plan.PageSize.Width);
                var pageHeight = ToPoints(plan.PageSize.Height);

                using var canvas = document.BeginPage(pageWidth, pageHeight);
                RenderPage(canvas, page, plan);
                document.EndPage();
            }

            document.Close();
        }

        public PdfLayoutPlan BuildPlan()
        {
            ValidateOptions();

            var bounds = Layout.Bounds ?? throw new InvalidOperationException("Layout bounds must be calculated before exporting to PDF.");
            var pageSize = GetPageSize();
            var printableArea = GetPrintableArea(pageSize);

            if (printableArea.Width <= Measure.Zero || printableArea.Height <= Measure.Zero)
                throw new InvalidOperationException("Printable area must be greater than zero.");

            var contentWidth = printableArea.Width / Options.OutputScale;
            var contentHeight = printableArea.Height / Options.OutputScale;

            if (Options.EnableTiling)
            {
                if (Options.HorizontalPageOverlap >= contentWidth)
                    throw new InvalidOperationException("Horizontal page overlap must be smaller than the printable width.");

                if (Options.VerticalPageOverlap >= contentHeight)
                    throw new InvalidOperationException("Vertical page overlap must be smaller than the printable height.");
            }

            var stepX = Options.EnableTiling ? contentWidth - Options.HorizontalPageOverlap : contentWidth;
            var stepY = Options.EnableTiling ? contentHeight - Options.VerticalPageOverlap : contentHeight;

            if (stepX <= Measure.Zero || stepY <= Measure.Zero)
                throw new InvalidOperationException("The effective printable step must be greater than zero.");

            int columnCount = Options.EnableTiling ? CalculatePageCount(bounds.Width, contentWidth, stepX) : 1;
            int rowCount = Options.EnableTiling ? CalculatePageCount(bounds.Height, contentHeight, stepY) : 1;

            var tiledWidth = contentWidth + stepX * (columnCount - 1);
            var tiledHeight = contentHeight + stepY * (rowCount - 1);

            var centeredStartX = bounds.Left - (tiledWidth - bounds.Width) / 2d;
            var centeredStartY = bounds.Top + (tiledHeight - bounds.Height) / 2d;

            var pages = new List<PdfLayoutPage>(rowCount * columnCount);
            int pageNumber = 1;

            for (int row = 0; row < rowCount; row++)
            {
                var pageTop = centeredStartY - stepY * row;
                for (int column = 0; column < columnCount; column++)
                {
                    var pageLeft = centeredStartX + stepX * column;
                    var layoutArea = RectangleM.FromLTRB(pageLeft, pageTop, pageLeft + contentWidth, pageTop - contentHeight);

                    pages.Add(new PdfLayoutPage(
                        pageNumber,
                        layoutArea,
                        printableArea,
                        CreateMetadata(pageNumber, rowCount * columnCount, layoutArea, bounds)));

                    pageNumber++;
                }
            }

            var markers = BuildMarkers(pages, rowCount, columnCount, stepX, stepY);

            return new PdfLayoutPlan(bounds, pageSize, printableArea, pages, markers, Options);
        }

        #region Drawing

        private SKCanvas? currentCanvas;
        private PdfPaperSize? currentPageSize;
        private RectangleM? currentPrintableArea;
        private RectangleM? currentLayoutArea;
        private PointM currentPageOffset = new(Measure.Zero, Measure.Zero);

        protected override void ExportElement(ElementType elementType, PathBase path, LineExportOptions lineOptions)
        {
            if (currentCanvas == null || currentPageSize == null || currentPrintableArea == null || currentLayoutArea == null)
                return;

            using var paint = CreatePaint(lineOptions);

            switch (path)
            {
                case LinearPath linearPath:
                    currentCanvas.DrawLine(ToPdfPoint(linearPath.Start), ToPdfPoint(linearPath.End), paint);
                    break;

                case PolyLinePath polyLinePath when polyLinePath.Points.Count >= 2:
                    using (var skPath = new SKPath())
                    {
                        skPath.MoveTo(ToPdfPoint(polyLinePath.Points[0]));
                        for (int i = 1; i < polyLinePath.Points.Count; i++)
                            skPath.LineTo(ToPdfPoint(polyLinePath.Points[i]));
                        currentCanvas.DrawPath(skPath, paint);
                    }
                    break;

                case BezierPath bezierPath:
                    using (var skPath = new SKPath())
                    {
                        skPath.MoveTo(ToPdfPoint(bezierPath.ControlPoints[0]));
                        skPath.CubicTo(
                            ToPdfPoint(bezierPath.ControlPoints[1]),
                            ToPdfPoint(bezierPath.ControlPoints[2]),
                            ToPdfPoint(bezierPath.ControlPoints[3]));
                        currentCanvas.DrawPath(skPath, paint);
                    }
                    break;

                case BezierSplinePath splinePath when splinePath.SegmentCount > 0:
                    using (var skPath = new SKPath())
                    {
                        skPath.MoveTo(ToPdfPoint(splinePath.GetFirstPoint()));
                        foreach (var segment in splinePath.GetSegments())
                        {
                            skPath.CubicTo(
                                ToPdfPoint(segment.P1),
                                ToPdfPoint(segment.P2),
                                ToPdfPoint(segment.P3));
                        }
                        currentCanvas.DrawPath(skPath, paint);
                    }
                    break;
            }
        }

        private void RenderPage(SKCanvas canvas, PdfLayoutPage page, PdfLayoutPlan plan)
        {
            currentCanvas = canvas;
            currentPageSize = plan.PageSize;
            currentPrintableArea = page.PrintableArea;
            currentLayoutArea = page.LayoutArea;
            currentPageOffset = new PointM(
                page.PrintableArea.Left - (page.LayoutArea.Left * Options.OutputScale),
                page.PrintableArea.Bottom - (page.LayoutArea.Bottom * Options.OutputScale));

            currentCanvas.Clear(SKColors.White);
            currentCanvas.Save();
            currentCanvas.ClipRect(GetPrintableClipRect(page.PrintableArea, plan.PageSize));

            ExportElements();

            if (Options.IncludeRegistrationMarks)
            {
                foreach (var marker in plan.Markers.Where(x => ContainsPoint(page.LayoutArea, x.Position)))
                    DrawMarker(marker);
            }

            currentCanvas.Restore();
            currentCanvas = null;
            currentPageSize = null;
            currentPrintableArea = null;
            currentLayoutArea = null;
        }

        private void DrawMarker(PdfLayoutMarker marker)
        {
            if (currentCanvas == null)
                return;

            var center = ToPdfPoint(marker.Position.ToVector());
            var radius = ToPoints(marker.Radius * Options.OutputScale);
            var crossInset = radius /*/ 2.5f*/;

            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 1f,
                IsAntialias = true
            };

            currentCanvas.DrawCircle(center, radius, paint);
            currentCanvas.DrawLine(center.X - crossInset, center.Y - crossInset, center.X + crossInset, center.Y + crossInset, paint);
            currentCanvas.DrawLine(center.X - crossInset, center.Y + crossInset, center.X + crossInset, center.Y - crossInset, paint);
        }

        private SKPaint CreatePaint(LineExportOptions lineOptions)
        {
            return new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(lineOptions.Color) ?? SKColors.Black,
                StrokeWidth = GetStrokeWidth(lineOptions.LineThickness),
                IsAntialias = true,
                PathEffect = lineOptions.Dashed ? SKPathEffect.CreateDash([ToPoints(Measure.Mm(3)), ToPoints(Measure.Mm(2))], 0) : null
            };
        }

        private SKPoint ToPdfPoint(SiGen.Maths.VectorD point)
        {
            if (currentPageSize == null)
                return SKPoint.Empty;

            var pageX = currentPageOffset.X + (Measure.FromNormalizedValue(LengthUnit.Cm, point.X) * Options.OutputScale);
            var pageY = currentPageOffset.Y + (Measure.FromNormalizedValue(LengthUnit.Cm, point.Y) * Options.OutputScale);

            return new SKPoint(
                ToPoints(pageX),
                ToPoints(currentPageSize.Height - pageY));
        }

        private SKRect GetPrintableClipRect(RectangleM printableArea, PdfPaperSize pageSize)
        {
            return new SKRect(
                ToPoints(printableArea.Left),
                ToPoints(pageSize.Height - printableArea.Top),
                ToPoints(printableArea.Right),
                ToPoints(pageSize.Height - printableArea.Bottom));
        }

        private float GetStrokeWidth(LineThickness? thickness)
        {
            if (!thickness.HasValue)
                return 1f;

            var value = thickness.Value;
            return value.Unit switch
            {
                ThicknessUnit.Millimeter => ToPoints(Measure.Mm(value.Value)),
                ThicknessUnit.Point => (float)value.Value,
                ThicknessUnit.Pixel => (float)value.Value,
                _ => 1f
            };
        }

        private static SKColor? ToSKColor(System.Drawing.Color? color)
        {
            if (color == null) return null;
            return new SKColor(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
        }

        #endregion

        protected override void SaveToFile(string filePath)
        {
            throw new NotSupportedException("Use ExportLayout(ExportTarget) for PDF export.");
        }

        protected override void SaveToStream(Stream stream)
        {
            throw new NotSupportedException("Use ExportLayout(ExportTarget) for PDF export.");
        }

        private static SKDocument CreatePdfDocument(ExportTarget target)
        {
            return target.Type switch
            {
                ExportTargetType.File => SKDocument.CreatePdf(target.FilePath ?? throw new InvalidOperationException("File export target is missing a file path.")),
                ExportTargetType.Stream => SKDocument.CreatePdf(target.Stream ?? throw new InvalidOperationException("Stream export target is missing a stream.")),
                _ => throw new ArgumentOutOfRangeException(nameof(target))
            };
        }

        private void ValidateOptions()
        {
            if (Layout.Bounds == null)
                throw new InvalidOperationException("Layout bounds must be calculated before exporting to PDF.");

            if (Options.Paper.Width <= Measure.Zero || Options.Paper.Height <= Measure.Zero)
                throw new InvalidOperationException("Paper size must be greater than zero.");

            if (Options.OutputScale <= 0)
                throw new InvalidOperationException("Output scale must be greater than zero.");

            if (Options.ContentMargin.Left < Measure.Zero || Options.ContentMargin.Top < Measure.Zero ||
                Options.ContentMargin.Right < Measure.Zero || Options.ContentMargin.Bottom < Measure.Zero)
            {
                throw new InvalidOperationException("Margins cannot be negative.");
            }

            if (Options.HorizontalPageOverlap < Measure.Zero || Options.VerticalPageOverlap < Measure.Zero)
                throw new InvalidOperationException("Page overlap cannot be negative.");
        }

        private PdfPaperSize GetPageSize()
        {
            var paper = Options.Paper;
            if (Options.Orientation == PdfPageOrientation.Portrait)
                return paper;

            return new PdfPaperSize(paper.Name, paper.Height, paper.Width);
        }

        private RectangleM GetPrintableArea(PdfPaperSize pageSize)
        {
            var left = Options.ContentMargin.Left;
            var top = pageSize.Height - Options.ContentMargin.Top;
            var width = pageSize.Width - Options.ContentMargin.Left - Options.ContentMargin.Right;
            var height = pageSize.Height - Options.ContentMargin.Top - Options.ContentMargin.Bottom;
            return new RectangleM(left, top, width, height);
        }

        private List<PdfLayoutMarker> BuildMarkers(List<PdfLayoutPage> pages, int rowCount, int columnCount, Measure stepX, Measure stepY)
        {
            var markers = new List<PdfLayoutMarker>();
            if (!Options.IncludeRegistrationMarks || pages.Count == 0)
                return markers;

            var verticalInset = Measure.Min(MarkerEdgeInset, pages[0].LayoutArea.Height / 4d);
            var horizontalInset = Measure.Min(MarkerEdgeInset, pages[0].LayoutArea.Width / 4d);

            for (int row = 0; row < rowCount; row++)
            {
                for (int column = 0; column < columnCount; column++)
                {
                    int index = row * columnCount + column;
                    var page = pages[index];

                    if (column < columnCount - 1 && Options.HorizontalPageOverlap > Measure.Zero)
                    {
                        var markerX = page.LayoutArea.Left + stepX + (Options.HorizontalPageOverlap / 2d);
                        markers.Add(new PdfLayoutMarker(
                            new PointM(markerX, page.LayoutArea.Top - verticalInset),
                            PdfLayoutMarkerType.Registration,
                            MarkerRadius));
                        markers.Add(new PdfLayoutMarker(
                            new PointM(markerX, page.LayoutArea.Bottom + verticalInset),
                            PdfLayoutMarkerType.Registration,
                            MarkerRadius));
                    }

                    if (row < rowCount - 1 && Options.VerticalPageOverlap > Measure.Zero)
                    {
                        var markerY = page.LayoutArea.Top - stepY - (Options.VerticalPageOverlap / 2d);
                        markers.Add(new PdfLayoutMarker(
                            new PointM(page.LayoutArea.Left + horizontalInset, markerY),
                            PdfLayoutMarkerType.Registration,
                            MarkerRadius));
                        markers.Add(new PdfLayoutMarker(
                            new PointM(page.LayoutArea.Right - horizontalInset, markerY),
                            PdfLayoutMarkerType.Registration,
                            MarkerRadius));
                    }
                }
            }

            return markers;
        }

        private static PdfLayoutPageMetadata CreateMetadata(int pageNumber, int totalPages, RectangleM layoutArea, RectangleM fullBounds)
        {
            return new PdfLayoutPageMetadata(
                pageNumber,
                totalPages,
                layoutArea.Left > fullBounds.Left,
                layoutArea.Right < fullBounds.Right,
                layoutArea.Top < fullBounds.Top,
                layoutArea.Bottom > fullBounds.Bottom);
        }

        private static bool ContainsPoint(RectangleM rectangle, PointM point)
        {
            return point.X >= rectangle.Left && point.X <= rectangle.Right &&
                   point.Y <= rectangle.Top && point.Y >= rectangle.Bottom;
        }

        private static int CalculatePageCount(Measure totalSize, Measure pageContentSize, Measure step)
        {
            if (totalSize <= pageContentSize)
                return 1;

            return (int)Math.Ceiling((totalSize - pageContentSize).NormalizedValue / step.NormalizedValue) + 1;
        }

        private static float ToPoints(Measure value)
        {
            return (float)(value[LengthUnit.Cm] * PointsPerCm);
        }
    }
}
