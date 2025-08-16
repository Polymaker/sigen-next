using SiGen.Layouts.Configuration;
using SiGen.Layouts.Data;
using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Measuring;
using SiGen.Paths;
using SiGen.Physics;
using System.Diagnostics;
using System.Numerics;

namespace SiGen.Layouts.Builders
{
    public class FretsBuilder : LayoutBuilderBase
    {
        public FretsBuilder(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration) : base(layout, configuration)
        {
        }

        public override void BuildLayoutCore()
        {
            if (!HasManualFretPositions())
                GenerateEqualTemperamentFrets();
            else
            {
                //todo
            }

            AdjustFingerboardEdges();

            if (Configuration.StringConfigurations.Any(x => (x.Frets?.StartingFret ?? 0) != 0))
            {
                foreach (var @string in Layout.Strings)
                    @string.GeneratePath();
            }
        }

        private void AdjustFingerboardEdges()
        {
            var fretElems = Layout.Elements.OfType<FretSegmentElement>()
                .Where(x => x.IsNut || x.IsBridge)
                .ToList();

            void AdjustEdge(FingerboardSide side)
            {
                var nutFretElem = fretElems.FirstOrDefault(x => x.HasFingerboardSide(side) && x.IsNut);
                var bridgeFretElem = fretElems.FirstOrDefault(x => x.HasFingerboardSide(side) && x.IsBridge);
                var edgeElem = Layout.GetFingerboardEdge(side);
                if (nutFretElem?.FretShape != null)
                {
                    edgeElem.Path.Start = side == FingerboardSide.Bass ?
                        nutFretElem.FretShape.GetFirstPoint() :
                        nutFretElem.FretShape.GetLastPoint();
                }
                if (bridgeFretElem?.FretShape != null)
                {
                    edgeElem.Path.End = side == FingerboardSide.Bass ?
                        bridgeFretElem.FretShape.GetFirstPoint() :
                        bridgeFretElem.FretShape.GetLastPoint();
                }
            }

            AdjustEdge(FingerboardSide.Bass);
            AdjustEdge(FingerboardSide.Treble);

            for (int i = 0; i < NumberOfStrings - 1; i++)
            {
                var median = Layout.GetStringMedian(i);
                var bassStr = Layout.GetStringElement(median.BassStringIndex);
                var trebStr = Layout.GetStringElement(median.TrebleStringIndex);
                median.Path.Start = (bassStr.NutPoint + trebStr.NutPoint).ToVector() / 2d;
            }
        }

        private bool HasManualFretPositions()
        {
            return Configuration.StringConfigurations.Any(x => x.Frets != null && x.Frets.Intervals?.Any() == true);
        }

        private void GenerateEqualTemperamentFrets()
        {
            var stringElems = Layout.Strings.ToList();

            var points = GenerateFretPoints();

            foreach (var fretGroup in points.GroupBy(x => x.FretIndex))
            {
                var fretSegments = CreateFretSegments(fretGroup.ToList(), 30, 10);
                foreach (var segment in fretSegments)
                {
                    var segmentPath = CreateSegmentPath(segment);
                    Layout.AddElement(new FretSegmentElement(fretGroup.Key, segment, segmentPath));
                }
            }
        }

        private List<FretPoint> GenerateFretPoints()
        {
            var points = new List<FretPoint>();

            int minimumFrets = Configuration.StringConfigurations.Min(x => x.Frets?.StartingFret ?? 0);
            int maximumFrets = Math.Max(Configuration.StringConfigurations.Max(x => x.Frets?.NumberOfFrets ?? 0), Configuration.NumberOfFrets ?? 0);

            if (maximumFrets == 0) return points;

            for (int i = 0; i < NumberOfStrings; i++)
            {
                int? numberOfFrets = GetStringConfig(i)?.Frets?.NumberOfFrets ?? Configuration.NumberOfFrets;
                if (numberOfFrets == null)
                    continue;

                int startingFret = GetStringConfig(i)?.Frets?.StartingFret ?? 0;

                var stringElem = Layout.Strings.First(x => x.StringIndex == i);

                for (int j = minimumFrets; j <= maximumFrets; j++)
                {
                    var interval = PitchInterval.From12TET(j, 0);
                    var fretRatio = 1d / interval.Ratio;
                    var fretPos = stringElem.Path.Interpolate(1d - fretRatio);
                    var fretPoint = new FretPoint(i, j, PointM.FromVector(fretPos), interval);

                    if (j == startingFret)
                    {
                        fretPoint.IsNut = true;
                        if (startingFret != 0)
                            stringElem.NutPoint = fretPoint.Position;
                    }

                    //the string does not really contain the fret
                    if (j < startingFret || j > numberOfFrets)
                        fretPoint.IsReference = true;

                    points.Add(fretPoint);

                }

                var bridgePoint = new FretPoint(i, 999, stringElem.BridgePoint, PitchInterval.FromCents(0));
                bridgePoint.IsBridge = true;
                points.Add(bridgePoint);
            }
            return points;
        }

        /// <summary>
        /// Segments a list of fret points into fret segments based on geometric thresholds.
        /// A new segment is started when the angle between the candidate fret segment and the string
        /// becomes too acute (below <paramref name="minFretStringAngle"/>), indicating excessive slant.
        /// Additionally, if the angle between consecutive segments exceeds <paramref name="fretBreakAngleThreshold"/>,
        /// a new segment is started to avoid sharp direction changes.
        /// </summary>
        /// <param name="fretPoints">The list of fret points for a single fret index.</param>
        /// <param name="minFretStringAngle">
        /// Minimum allowed angle (in degrees) between a fret segment and the string.
        /// If the angle is less than this value, the segment is considered too slanted and a new segment is started.
        /// </param>
        /// <param name="fretBreakAngleThreshold">
        /// Maximum allowed angle (in degrees) between consecutive fret segments.
        /// If exceeded, a new segment is started to avoid sharp bends.
        /// </param>
        /// <returns>A list of fret segments for rendering and layout.</returns>
        private List<FretSegment> CreateFretSegments(List<FretPoint> fretPoints, decimal minFretStringAngle, decimal fretBreakAngleThreshold)
        {
            var segments = new List<FretSegment>();

            FretPoint currentPoint = fretPoints.First();

            var currentSegment = new FretSegment();
            currentSegment.AddPoint(currentPoint);
            segments.Add(currentSegment);

            FretPoint? GetNextPoint()
            {
                return fretPoints.FirstOrDefault(x => x.StringIndex > currentPoint.StringIndex);
            }

            while (true)
            {
                var nextPoint = GetNextPoint();

                if (nextPoint == null) break;

                if (nextPoint.StringIndex - currentPoint.StringIndex > 1)
                {
                    currentSegment = new FretSegment();
                    currentSegment.AddPoint(nextPoint);

                    currentPoint = nextPoint;
                    segments.Add(currentSegment);
                    break;
                }

                //When evaluating a candidate segment, calculate the angle between the segment and the string.
                var candidateSegmentLine = new LinearPath(currentPoint.Position.ToVector(), nextPoint.Position.ToVector());
                var segmentString = Layout.GetStringElement(currentPoint.StringIndex); //the string that the segment is on
                var angleRelativeToString = MathD.Abs(LinearPath.GetAngleBetweenLines(segmentString.Path, candidateSegmentLine));

                //If the angle is less than minFretStringAngle, the segment is too slanted and a new segment is started.
                bool shouldBreak = angleRelativeToString < minFretStringAngle;

                if (currentSegment.Count >= 2)
                {
                    var lastLine = currentSegment.GetLineFromLastTwoPoints();
                    var angleRelativeToLastSegment = MathD.Abs(LinearPath.GetAngleBetweenLines(lastLine, candidateSegmentLine));
                    //If the angle between consecutive segments exceeds fretBreakAngleThreshold, a new segment is started to avoid sharp bends.
                    if (angleRelativeToLastSegment > fretBreakAngleThreshold)
                        shouldBreak = true;
                }

                if (!shouldBreak)
                {
                    currentSegment.AddPoint(nextPoint);
                }
                else
                {
                    currentSegment = new FretSegment();
                    currentSegment.AddPoint(nextPoint);
                    segments.Add(currentSegment);
                }

                currentPoint = nextPoint;
            }


            //remove segments that are only reference points (I don't know if it's even possilbe)
            segments.RemoveAll(x => x.FretPoints.All(y => y.IsReference));

            //Split fret segments that have references points between two real points
            SplitSegments(segments);

            //split segments that are partial nut segments (possible if a string has a starting fret > 0)
            SplitNutSegments(segments);
            return segments;
        }

        /// <summary>
        /// Split fret segments that have references points between two real points
        /// </summary>
        /// <param name="segments"></param>
        private void SplitSegments(List<FretSegment> segments)
        {
            var segmentsWithReferences = segments.Where(x => x.FretPoints.Any(y => y.IsReference)).ToList();
            if (segmentsWithReferences.Count == 0) return;

            for (int i = 0; i < segmentsWithReferences.Count; i++)
            {
                var segment = segmentsWithReferences[i];
                segment.TrimReferencePoints();
                var truePoints = segment.FretPoints.Where(x => !x.IsReference).ToList();

                for (int j = 0; j < truePoints.Count - 1; j++)
                {
                    if (truePoints[j + 1].StringIndex - truePoints[j].StringIndex > 1)
                    {
                        var newSegment = new FretSegment();
                        int pointIndex = segment.FretPoints.IndexOf(truePoints[j + 1]);
                        newSegment.FretPoints.AddRange(segment.FretPoints.Skip(pointIndex - 1));
                        newSegment.TrimReferencePoints();

                        segment.FretPoints.RemoveRange(pointIndex, segment.Count - pointIndex);
                        segment.TrimReferencePoints();
                        segments.Add(newSegment);

                        if (newSegment.FretPoints.Any(y => y.IsReference))
                            segmentsWithReferences.Add(newSegment);
                    }
                }
            }
        }

        private void SplitNutSegments(List<FretSegment> segments)
        {
            var partialNutSegments = segments.Where(x => x.IsPartialNut()).ToList();
            if (partialNutSegments.Count == 0) return;

            for (int i = 0;  i < partialNutSegments.Count; i++)
            {
                var segment = partialNutSegments[i];

                bool isNut = false;
                int firstNutIndex = 0;

                for (int j = 0; j < segment.Count; j++)
                {
                    if (isNut && !segment.FretPoints[j].IsNut)
                    {
                        var newSegment = new FretSegment();
                        if (firstNutIndex > 0)
                            newSegment.AddPoint(segment.FretPoints[firstNutIndex - 1].Clone());
                        newSegment.FretPoints.AddRange(segment.FretPoints.Skip(firstNutIndex));
                        segment.FretPoints.RemoveRange(firstNutIndex, segment.Count - firstNutIndex);
                        if (newSegment.IsPartialNut())
                            partialNutSegments.Add(newSegment);
                        segments.Add(newSegment);
                        break;
                    }

                    if (segment.FretPoints[j].IsReference) continue;

                    if (segment.FretPoints[j].IsNut)
                    {
                        isNut = true;
                        firstNutIndex = j;
                    }
                }

                if (isNut)
                {
                    var newSegment = new FretSegment();
                    if (firstNutIndex > 0)
                        newSegment.AddPoint(segment.FretPoints[firstNutIndex - 1].Clone());
                    newSegment.FretPoints.AddRange(segment.FretPoints.Skip(firstNutIndex));
                    segment.FretPoints.RemoveRange(firstNutIndex, segment.Count - firstNutIndex);
                    segments.Add(newSegment);
                }
            }
        }

        private PathBase CreateSegmentPath(FretSegment segment)
        {
            var vectorPoints = new List<VectorD>();

            var firstPt = segment.FretPoints.First(x => !x.IsReference);
            var lastPt = segment.FretPoints.Last(x => !x.IsReference);
            LinearPath? bassSideEdge = null;
            LinearPath? trebSideEdge = null;

            if (firstPt.StringIndex == 0)
                bassSideEdge = Layout.GetFingerboardEdge(FingerboardSide.Bass).Path;
            else
                bassSideEdge = Layout.GetStringMedian(firstPt.StringIndex - 1).Path;


            if (lastPt.StringIndex == Configuration.NumberOfStrings - 1)
                trebSideEdge = Layout.GetFingerboardEdge(FingerboardSide.Treble).Path;
            else
                trebSideEdge = Layout.GetStringMedian(lastPt.StringIndex).Path;

            foreach (var fretPt in segment.FretPoints.Where(x => !x.IsReference))
                vectorPoints.Add(fretPt.Position.ToVector());

            if (segment.FretPoints.Count == 1)
            {
                vectorPoints.Insert(0, bassSideEdge.SnapToLine(firstPt.Position.ToVector()));
                vectorPoints.Add(trebSideEdge.SnapToLine(lastPt.Position.ToVector()));
            }
            else
            {
                var fretLine = new LinearPath(segment.FretPoints[0].Position.ToVector(), segment.FretPoints[1].Position.ToVector());
                if (fretLine.Intersects(bassSideEdge, out var inter, true))
                    vectorPoints.Insert(0, inter);

                fretLine = new LinearPath(segment.FretPoints[^2].Position.ToVector(), segment.FretPoints[^1].Position.ToVector());
                if (fretLine.Intersects(trebSideEdge, out inter, true))
                    vectorPoints.Add(inter);
            }

            if (ShouldFretBeStraight(vectorPoints, 5))
            {
                return new LinearPath(vectorPoints.First(), vectorPoints.Last());
            }

            return new PolyLinePath(vectorPoints);
        }

        public static bool ShouldFretBeStraight(List<VectorD> fretPositions, double maxDeviation)
        {
            if (fretPositions.Count <= 2)
                return true; // Any two points always form a line

            var start = fretPositions.First();
            var end = fretPositions.Last();
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            var lengthSquared = dx * dx + dy * dy;

            if (lengthSquared == 0)
                return false; // All points collapsed to a single location

            foreach (var point in fretPositions)
            {
                // Area-based perpendicular distance to the line (cross-product method)
                var distance = MathD.Abs((dy * (point.X - start.X) - dx * (point.Y - start.Y)) / MathD.Sqrt(lengthSquared));
                if (distance > maxDeviation)
                    return false;
            }

            return true;
        }
    }
}
