using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;

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


            //var vertLine = new LineD(0, bassEdge.Path.Start.Y);
            //if (trebEdge.Path.GetEquation().Intersects(vertLine, out var bassIntersection))
            //{
            //    var center = (bassEdge.Path.Start + bassIntersection) / 2d;
            //}
        }

        protected override void ExecuteSecondPass()
        {
            var points = new List<VectorD>();

            PreciseDouble extension = 0;
            if (Configuration.Fingerboard.ExtensionAfterLastFret.HasValue)
                extension = Configuration.Fingerboard.ExtensionAfterLastFret.Value.NormalizedValue;
            //VectorD nextMedianPoint = new VectorD(0, 0);

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringElem = Layout.GetStringElement(i);
                var lastFretSeg = Layout.GetFretSegments()
                    .OrderByDescending(x => x.FretIndex)
                    .FirstOrDefault(x => !x.IsBridge && x.ContainsString(i));

                if (lastFretSeg?.FretShape == null) continue;
                if (i == 0)
                {
                    var bassEdgePath = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Bass).Path;
                    var edgeStopPoint = lastFretSeg.FretShape.GetFirstPoint() + bassEdgePath.Direction * extension;
                    points.Add(edgeStopPoint);
                    Layout.Elements.Add(new GuideLineElement(new LinearPath(edgeStopPoint, bassEdgePath.End)));
                    bassEdgePath.End = edgeStopPoint;
                }
                else
                {
                    var prevMedian = Layout.GetStringMedian(i - 1);
                    if (lastFretSeg.FretShape.Extend(0.02)?.Intersects(prevMedian.Path, out var prevInter) == true)
                    {
                        //var maxPt = nextMedianPoint.Y < prevInter.Y ? nextMedianPoint : prevInter;
                        points.Add(prevInter + prevMedian.Path.Direction * extension);
                    }
                }

                if (lastFretSeg.FretShape.Intersects(stringElem.Path, out var inter))
                {
                    points.Add(inter + stringElem.Path.Direction * extension);
                }

                

                if (i == NumberOfStrings - 1)
                {
                    var trebEdgePath = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Treble).Path;
                    var edgeStopPoint = lastFretSeg.FretShape.GetLastPoint() + trebEdgePath.Direction * extension;
                    points.Add(edgeStopPoint);
                    Layout.Elements.Add(new GuideLineElement(new LinearPath(edgeStopPoint, trebEdgePath.End)));
                    trebEdgePath.End = edgeStopPoint;
                }
                else
                {
                    var stringMedian = Layout.GetStringMedian(i);
                    if (lastFretSeg.FretShape.Intersects(stringMedian.Path, out var inter2))
                    {
                        var targetPos = inter2 + stringMedian.Path.Direction * extension;
                        if (VectorD.Distance(points[^1], targetPos) > 0.01)
                            points.Add(targetPos);
                        //nextMedianPoint = inter2;
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
                PreciseDouble offset = 0;
                var stringWidth = Configuration.StringConfigurations[0].GetTotalWidth();
                if (Configuration.Fingerboard.CompensateMarginsForStrings && !Measure.IsNullOrEmpty(stringWidth))
                    offset = stringWidth.Value.NormalizedValue / 2d;
                startPt -= PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt -= PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }
            else
            {
                PreciseDouble offset = 0;
                var stringWidth = Configuration.StringConfigurations[^1].GetTotalWidth();
                if (Configuration.Fingerboard.CompensateMarginsForStrings && !Measure.IsNullOrEmpty(stringWidth))
                    offset = stringWidth.Value.NormalizedValue / 2d;
                startPt += PointM.FromVector(nutPerpLine.Vector * (nutMargin.NormalizedValue + offset));
                endPt += PointM.FromVector(bridgePerpLine.Vector * (bridgeMargin.NormalizedValue + offset));
            }

            var edgePath = new Paths.LinearPath(startPt.ToVector(), endPt.ToVector());
            return new FingerboardEdgeElement(edgePath, side);
        }
    }
}
