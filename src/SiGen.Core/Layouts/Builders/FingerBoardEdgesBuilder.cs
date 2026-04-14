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

            //var points = new List<VectorD>();
            //int maxFrets = Configuration.GetMaxFrets();

            //Measure nutBassMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Nut, FingerboardSide.Bass);
            //Measure bridgeBassMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Bridge, FingerboardSide.Bass);

            //nutBassMargin += Configuration.StringConfigurations[0].GetHalfWidth(FingerboardSide.Bass, Configuration.Fingerboard.CompensateMarginsForStrings);
            //bridgeBassMargin += Configuration.StringConfigurations[0].GetHalfWidth(FingerboardSide.Bass, Configuration.Fingerboard.CompensateMarginsForStrings);
            //int lastBassStringIndex = -1;
            //for (int i = 0; i <= maxFrets; i++)
            //{
            //    var fretSeg = Layout.GetFretSegments().Where(x => x.ContainsFret(i) && (lastBassStringIndex < 0 || x.ContainsString(lastBassStringIndex)))
            //        .OrderBy(x => x.BassStringIndex).FirstOrDefault();
            //    if (fretSeg != null && (fretSeg.BassStringIndex > 0 || fretSeg.ContainsString(lastBassStringIndex)))
            //    {
            //        var fretPt = fretSeg.GetFretPoint(fretSeg.BassStringIndex)!;
            //        var pos = fretPt.Position.ToVector();
            //        var ratio = 1d / fretPt.Interval.Ratio;
            //        var adjustedMargin = MathD.Map(1, 0, nutBassMargin.NormalizedValue, bridgeBassMargin.NormalizedValue, ratio);
            //        pos.X -= adjustedMargin;
            //        //MathD.Map(0, 1, nutBassMargin)
            //        points.Add(pos);
            //        lastBassStringIndex = fretSeg.BassStringIndex;

            //        if (fretSeg.BassStringIndex == 0) break;
            //    }
            //}

            //if (points.Count >= 2 )
            //{
            //    var line = new PolyLinePath(points);
            //    Layout.AddElement(new FingerboardEdgeElement(line, null));
            //}
        }

        private void CreateFingerboardExtensionLine()
        {
            var points = new List<VectorD>();

            double extension = 0;
            if (Configuration.Fingerboard.ExtensionAfterLastFret.HasValue)
                extension = Configuration.Fingerboard.ExtensionAfterLastFret.Value.NormalizedValue;
            //VectorD nextMedianPoint = new VectorD(0, 0);

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringElem = Layout.GetStringElement(i);
                var lastFretSeg = Layout.GetFretSegments()
                    .OrderByDescending(x => x.FretIndex)
                    .FirstOrDefault(x => x.IsLastFretForString(i)/* !x.IsBridge && x.ContainsString(i)*/);

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
