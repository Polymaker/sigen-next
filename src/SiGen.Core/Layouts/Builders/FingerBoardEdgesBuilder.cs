using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using System.Diagnostics;

namespace SiGen.Layouts.Builders
{
    public class FingerBoardEdgesBuilder : LayoutBuilderBase
    {
        public FingerBoardEdgesBuilder(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration) : base(layout, configuration)
        {
        }

        protected override void ExecuteFirstPass()
        {
            var bassEdge = CreateSideElement(FingerboardSide.Bass);
            var trebEdge = CreateSideElement(FingerboardSide.Treble);


            Layout.AddElement(bassEdge);
            Layout.AddElement(trebEdge);

            //if (bassEdge.Path is LinearPath && trebEdge.Path is LinearPath)
            //{
            //    var vertLine = new LineD(0, (bassEdge.Path as LinearPath)!.Start.Y);
            //    if ((trebEdge.Path as LinearPath)!.GetEquation().Intersects(vertLine, out var bassIntersection))
            //    {
            //        var center = ((bassEdge.Path as LinearPath)!.Start + bassIntersection) / 2d;
            //        Trace.WriteLine($"Nut center: {center}");
            //    }
            //}
           
        }

        protected override void ExecuteSecondPass()
        {
            CreateFingerboardExtensionLine();

        }

        private void CreateFingerboardExtensionLine()
        {
            double extension = 0;
            if (Configuration.Fingerboard.ExtensionAfterLastFret.HasValue)
                extension = Configuration.Fingerboard.ExtensionAfterLastFret.Value.NormalizedValue;

            var points = new List<VectorD>();

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringElem = Layout.GetStringElement(i);
                var lastFretSeg = Layout.GetFretSegments()
                    .OrderByDescending(x => x.FretIndex)
                    .FirstOrDefault(x => x.IsLastFretForString(i));

                if (lastFretSeg?.FretShape == null) continue;

                // --- Bass-side point of string i's last fret ---
                if (i == 0)
                {
                    var bassEdgePath = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Bass).Path;
                    var edgeStopPoint = lastFretSeg.FretShape.GetFirstPoint() + bassEdgePath.Direction * extension;
                    points.Add(edgeStopPoint);
                    Layout.Elements.Add(new GuideLineElement(GuideLineType.FretboardProjection,  path: new LinearPath(edgeStopPoint, bassEdgePath.End)));
                    bassEdgePath.End = edgeStopPoint;
                }
                else
                {
                    // Intersect THIS string's last fret with the median between string i-1 and i
                    var bassMedian = Layout.GetStringMedian(i - 1);
                    if (lastFretSeg.FretShape.Extend(0.02)?.Intersects(bassMedian.Path, out var bassInter) == true)
                        points.Add(bassInter + bassMedian.Path.Direction * extension);
                }

                // --- String midpoint of string i's last fret (the fret line itself) ---
                if (lastFretSeg.FretShape.Intersects(stringElem.Path, out var stringInter))
                    points.Add(stringInter + stringElem.Path.Direction * extension);

                // --- Treble-side point of string i's last fret ---
                if (i == NumberOfStrings - 1)
                {
                    var trebEdgePath = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Treble).Path;
                    var edgeStopPoint = lastFretSeg.FretShape.GetLastPoint() + trebEdgePath.Direction * extension;
                    points.Add(edgeStopPoint);
                    Layout.Elements.Add(new GuideLineElement(GuideLineType.FretboardProjection, path: new LinearPath(edgeStopPoint, trebEdgePath.End)));
                    trebEdgePath.End = edgeStopPoint;
                }
                else
                {
                    // Intersect THIS string's last fret with the median between string i and i+1
                    var trebMedian = Layout.GetStringMedian(i);
                    if (lastFretSeg.FretShape.Extend(0.02)?.Intersects(trebMedian.Path, out var trebInter) == true)
                    {
                        var targetPos = trebInter + trebMedian.Path.Direction * extension;
                        if (points.Count == 0 || VectorD.Distance(points[^1], targetPos) > 0.01)
                            points.Add(targetPos);
                    }
                }
            }

            if (points.Count >= 2 && extension > 0)
            {
                var line = new PolyLinePath(points);
                Layout.AddElement(new FingerboardEdgeElement(line, null));
            }
        }

        private FingerboardEdgeElement CreateSideElement(FingerboardSide side)
        {
            var @string = Layout.GetStringElement(side);

            var nutMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Nut, side);
            var bridgeMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Bridge, side);

            var startPt = @string.StartPoint;
            var endPt = @string.BridgePoint;

            var nutPerpLine = @string.Path.GetEquation().GetPerpendicular(@string.NutPoint.ToVector());
            var bridgePerpLine = @string.Path.GetEquation().GetPerpendicular(@string.BridgePoint.ToVector());

            if (side == FingerboardSide.Bass)
            {
                double offset = Configuration.StringConfigurations[0].GetHalfWidth(side, Configuration.Fingerboard.CompensateMarginsForStrings).NormalizedValue;
                startPt -= PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt -= PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }
            else
            {
                double offset = Configuration.StringConfigurations[^1].GetHalfWidth(side, Configuration.Fingerboard.CompensateMarginsForStrings).NormalizedValue;
                startPt += PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt += PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }

            int stringIndex = side == FingerboardSide.Bass ? 0 : NumberOfStrings - 1;
            var edgePath = new Paths.LinearPath(startPt.ToVector(), endPt.ToVector());
            return new FingerboardEdgeElement(edgePath, side, stringIndex);
        }
    }
}
