using netDxf;
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

        protected override void ExecuteFirstPass()
        {

            //var points = GenerateFretPoints();

            BuildFretSegments();

            //if (!HasManualFretPositions())
            //{
            //    GenerateFrets();
            //}
            //else
            //{
            //    //todo
            //}

            AdjustFingerboardEdges();

            if (Configuration.StringConfigurations.Any(x => (x.Frets?.StartingFret ?? 0) != 0))
            {
                foreach (var @string in Layout.Strings)
                    @string.GeneratePath();
            }
        }

        private void BuildFretSegments()
        {
            //bool isMultiscale = Configuration.ScaleLength.Mode != ScaleLengthMode.Single;
            bool useLinearMatching = Configuration.ScaleLength.Mode == ScaleLengthMode.Single && Measure.IsNullOrEmpty(Configuration.ScaleLength.BassTrebleSkew);
            var points = GenerateFretPoints();
            var pointsByString = points
                .GroupBy(p => p.StringIndex)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Interval.Cents).ToList());

            var queue = new Queue<FretPoint>(points.OrderBy(p => p.StringIndex).ThenBy(p => p.Interval.Cents));
            var processed = new HashSet<FretPoint>();

            var segments = new List<FretSegment>();

            double fretBreakAngleThreshold = 10; //todo: put this setting in the configuration

            while (queue.Count > 0)
            {
                var seed = queue.Dequeue();
                if (processed.Contains(seed) || seed.IsReference) continue;

                var segmentPoints = new List<FretPoint> { seed };
                var currentPoint = seed;
                var currentSegment = new FretSegment();
                currentSegment.AddPoint(seed);
                processed.Add(seed);
                segments.Add(currentSegment);

                // chain across strings
                while (pointsByString.TryGetValue(currentPoint.StringIndex + 1, out var nextStringPoints))
                {
                    FretPoint? match = null;

                    if (useLinearMatching)
                        match = FindLinearMatch(currentPoint, nextStringPoints, processed);
                    else
                        match = FindFannedMatch(currentPoint, currentSegment, nextStringPoints, processed);

                    if (match == null) break;

                    var candidateSegmentLine = new LinearPath(currentPoint.Position.ToVector(), match.Position.ToVector());
                    if (currentSegment.Count >= 2)
                    {
                        var lastLine = currentSegment.GetLineFromLastTwoPoints();
                        var angleRelativeToLastSegment = Math.Abs(LinearPath.GetAngleBetweenLines(lastLine, candidateSegmentLine));
                        //If the angle between consecutive segments exceeds fretBreakAngleThreshold, a new segment is started to avoid sharp bends.
                        if (angleRelativeToLastSegment > fretBreakAngleThreshold)
                            break;
                    }

                    currentSegment.AddPoint(match);
                    processed.Add(match);
                    currentPoint = match;
                }
            }

            //remove segments that are only reference points (I don't know if it's even possilbe)
            segments.RemoveAll(x => x.FretPoints.All(y => y.IsReference));

            //Split fret segments that have references points between two real points
            SplitSegments(segments);

            //split segments that are partial nut segments (possible if a string has a starting fret > 0)
            SplitNutSegments(segments);

            int fretIndex = 0;
            foreach (var segment in segments)
            {
                var segmentPath = CreateSegmentPath(segment);
                Layout.AddElement(new FretSegmentElement(fretIndex++, segment, segmentPath));
            }
        }

        private FretPoint? FindLinearMatch(FretPoint current, List<FretPoint> candidatePoints, HashSet<FretPoint> processed)
        {
            var currentPos = current.Position.ToVector();
            double toleranceCm = 0.1; //todo: put this setting in the configuration

            return candidatePoints
                .Where(p => !processed.Contains(p))
                .Where(p => Math.Abs(p.Position.ToVector().Y - currentPos.Y) <= toleranceCm)
                .MinBy(p => Math.Abs(p.Position.ToVector().Y - currentPos.Y));
        }

        private FretPoint? FindFannedMatch(FretPoint currentPoint, FretSegment currentSegment, List<FretPoint> candidatePoints, HashSet<FretPoint> processed)
        {
            var currentString = Layout.GetStringElement(currentPoint.StringIndex);
            var nextString = Layout.GetStringElement(currentPoint.StringIndex + 1);
            if (nextString == null) return null;

            var expectedPosition1 = nextString.Path.Interpolate(1d - (1d / currentPoint.Interval.Ratio));


            if (currentSegment.Count >= 2)
            {
                if (currentPoint.FretIndex == 16)
                {

                }
                var currentDirection = currentSegment.GetLineFromLastTwoPoints();
                if (nextString.Path.Intersects(currentDirection, out var expectedPosition2, true))
                {
                    var foundPoint =  candidatePoints
                       .Where(p => !processed.Contains(p))
                       .Where(p => VectorD.Distance(p.Position.ToVector(), expectedPosition2) <= 0.2) //todo: put this setting in the configuration
                       .MinBy(p => VectorD.Distance(p.Position.ToVector(), expectedPosition2));
                    if (foundPoint != null) return foundPoint;
                }
            }

            return candidatePoints
                    .Where(p => !processed.Contains(p))
                    .Where(p => VectorD.Distance(p.Position.ToVector(), expectedPosition1) <= 0.2) //todo: put this setting in the configuration
                    .MinBy(p => VectorD.Distance(p.Position.ToVector(), expectedPosition1));
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
                var edgePath = edgeElem.Path as LinearPath;
                if (edgePath == null) return;

                if (nutFretElem?.FretShape != null)
                {
                    edgePath.Start = side == FingerboardSide.Bass ?
                        nutFretElem.FretShape.GetFirstPoint() :
                        nutFretElem.FretShape.GetLastPoint();
                }
                if (bridgeFretElem?.FretShape != null)
                {
                    edgePath.End = side == FingerboardSide.Bass ?
                        bridgeFretElem.FretShape.GetFirstPoint() :
                        bridgeFretElem.FretShape.GetLastPoint();
                }
            }

            AdjustEdge(FingerboardSide.Bass);
            AdjustEdge(FingerboardSide.Treble);

            //for (int i = 0; i < NumberOfStrings - 1; i++)
            //{
            //    var median = Layout.GetStringMedian(i);
            //    var bassStr = Layout.GetStringElement(median.BassStringIndex);
            //    var trebStr = Layout.GetStringElement(median.TrebleStringIndex);
            //    //median.Path.Start = (bassStr.NutPoint + trebStr.NutPoint).ToVector() / 2d;
            //}
        }

        private bool HasManualFretPositions()
        {
            return Configuration.Temperament == Temperament.Custom || 
                Configuration.StringConfigurations.Any(x => x.Frets != null && x.Frets.Temperament == Temperament.Custom);
        }

        private void GenerateFrets()
        {
            var stringElems = Layout.Strings.ToList();

            var points = GenerateFretPoints();

            foreach (var fretGroup in points.GroupBy(x => x.FretIndex))
            {
                var fretSegments = CreateFretSegments(fretGroup.ToList(), 25, 10);
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

            //if (maximumFrets == 0) return points;

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringConfig = GetStringConfig(i);

                bool hasStringFretConfig = stringConfig?.Frets != null;

                var stringFretConfig = stringConfig?.Frets ?? Configuration.Frets;
                int? numberOfFrets = stringFretConfig?.NumberOfFrets ?? Configuration.NumberOfFrets;
                if (numberOfFrets == null)
                    continue;

                var temperament = stringFretConfig?.Temperament ?? Configuration.Temperament;
                int etSteps = stringFretConfig?.ETSteps ?? Configuration.Frets.ETSteps ?? 12;

                int startingFret = stringConfig?.Frets?.StartingFret ?? 0;

                var stringElem = Layout.Strings.First(x => x.StringIndex == i);

                var rootNote = stringConfig?.GetPrimaryNote() ?? new NoteAndOctave(NoteName.C, 1);

                var rootInterval = temperament != Temperament.Custom ? 
                    PitchInterval.FromNote(rootNote, temperament) : 
                    PitchInterval.FromNote(rootNote, Temperament.Equal);

                double rootCents = rootInterval.Cents;

                int lastFretIndex = 0;

                var stringPoints = new List<FretPoint>();

                if (temperament != Temperament.Custom)
                {
                    for (int j = minimumFrets; j <= maximumFrets; j++)
                    {
                        var fretCents = GetCentsForFret(j, rootNote, temperament, etSteps);

                        /*var fretNote = rootNote.Transpose(j);

                        var fretInterval = j == 0 ? rootInterval : PitchInterval.FromNote(fretNote, temperament);
                        PitchInterval interval = fretInterval - rootInterval;*/
                        PitchInterval interval = PitchInterval.FromCents(fretCents - rootCents);
                        var fretRatio = 1d / interval.Ratio;
                        var fretPos = stringElem.Path.Interpolate(1d - fretRatio);
                        var fretPoint = new FretPoint(i, j, PointM.FromVector(fretPos), interval);

                        // mark nut points
                        if (j == startingFret)
                        {
                            fretPoint.IsNut = true;
                            if (startingFret != 0) // update the nut point for the string when starting fret > 0
                                stringElem.NutPoint = fretPoint.Position;
                        }

                        //the string does not really contain the fret
                        if (j < startingFret || j > numberOfFrets)
                            fretPoint.IsReference = true;

                        //if (j == (stringConfig?.Frets?.NumberOfFrets ?? Configuration.NumberOfFrets))
                        //    fretPoint.IsLastFret = true;

                        stringPoints.Add(fretPoint);
                        lastFretIndex = j;
                    }
                }

                if (stringFretConfig?.Intervals != null)
                {
                    var intervals = new List<double>();
                    
                    intervals.AddRange(stringFretConfig.Intervals);

                    if (temperament == Temperament.Custom && !intervals.Contains(0))
                        intervals.Insert(0, 0); // ensure there is a nut point if using custom temperament with custom intervals

                    for (int j = 0; j < intervals.Count; j++)
                    {
                        PitchInterval interval = PitchInterval.FromCents(intervals[j]);
                        var fretRatio = 1d / interval.Ratio;
                        var fretPos = stringElem.Path.Interpolate(1d - fretRatio);
                        var fretPoint = new FretPoint(i, lastFretIndex + j + 1, PointM.FromVector(fretPos), interval);
                        fretPoint.IsManualInterval = true;
                        fretPoint.IsNut = intervals[j] == 0;
                        //fretPoint.IsLastFret = j == intervals.Count - 1;
                        stringPoints.Add(fretPoint);
                    }
                }

                var lastPoint = stringPoints.OrderBy(x=>x.Interval.Cents).LastOrDefault(x => !x.IsReference);
                if (lastPoint != null)
                    lastPoint.IsLastFret = true;

                //add bridge point
                var bridgePoint = new FretPoint(i, 999, stringElem.BridgePoint, PitchInterval.FromCents(99999));
                bridgePoint.IsBridge = true;
                stringPoints.Add(bridgePoint);

                int index = 0;
                stringPoints = stringPoints.OrderBy(x => x.Interval.Cents).ToList();
                foreach (var pt in stringPoints)
                    pt.FretIndex = index++;

                points.AddRange(stringPoints);
            }
            return points;
        }

        private static double GetCentsForFret(int fretIndex, NoteAndOctave rootNote, Temperament temperament, int etSteps)
        {
            return temperament switch
            {
                Temperament.Equal => rootNote.ToAbsoluteCents() + (fretIndex * (1200.0 / etSteps)),
                Temperament.Just or Temperament.Thidell =>
                    PitchInterval.FromNote(rootNote.Transpose(fretIndex), temperament).Cents,
                _ => throw new ArgumentOutOfRangeException()
            };
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
        private List<FretSegment> CreateFretSegments(List<FretPoint> fretPoints, double minFretStringAngle, double fretBreakAngleThreshold)
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
                var angleRelativeToString = Math.Abs(LinearPath.GetAngleBetweenLines(segmentString.Path, candidateSegmentLine));

                //If the angle is less than minFretStringAngle, the segment is too slanted and a new segment is started.
                bool shouldBreak = angleRelativeToString < minFretStringAngle;

                if (currentSegment.Count >= 2)
                {
                    var lastLine = currentSegment.GetLineFromLastTwoPoints();
                    var angleRelativeToLastSegment = Math.Abs(LinearPath.GetAngleBetweenLines(lastLine, candidateSegmentLine));
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
                var currentSegment = partialNutSegments[i];

                
                int firstNutIndex = 0;

                bool isNut = false; // segment.FretPoints[0].IsNut;

                void createNutSegment(int firstIndex, int lastIndex)
                {
                    var nutSegment = new FretSegment();
                    if (firstIndex > 0)
                        nutSegment.FretPoints.Add(currentSegment.FretPoints[firstIndex - 1].Clone());
                    nutSegment.FretPoints.AddRange(currentSegment.FretPoints.Skip(firstIndex).Take(lastIndex - firstNutIndex + 1));
                    if (lastIndex + 1 < currentSegment.Count)
                        nutSegment.FretPoints.Add(currentSegment.FretPoints[lastIndex + 1].Clone());
                    segments.Add(nutSegment);
                }

                for (int j = 0; j < currentSegment.Count; j++)
                {
                    if (currentSegment.FretPoints[j].IsReference) continue;

                    if (!isNut && currentSegment.FretPoints[j].IsNut)
                    {
                        isNut = true;
                        firstNutIndex = j;
                        continue;
                    }

                    if (isNut && !currentSegment.FretPoints[j].IsNut)
                    {
                        isNut = false;
                        int lastNutIndex = j - 1;
                        createNutSegment(firstNutIndex, lastNutIndex);

                        if (j + 1 < currentSegment.Count)
                        {
                            var remainingSegment = new FretSegment();
                            remainingSegment.FretPoints.Add(currentSegment.FretPoints[j - 1].Clone());
                            remainingSegment.FretPoints.AddRange(currentSegment.FretPoints.Skip(j));
                            segments.Add(remainingSegment);
                            if (remainingSegment.IsPartialNut())
                                partialNutSegments.Insert(j + 1, remainingSegment);
                        }
                        currentSegment.FretPoints[firstNutIndex] = currentSegment.FretPoints[firstNutIndex].Clone();
                        currentSegment.FretPoints.RemoveRange(firstNutIndex + 1, currentSegment.Count - firstNutIndex - 1);

                        if (currentSegment.FretPoints.All(y => y.IsReference))
                            segments.Remove(currentSegment);

                        break;
                    }
                }

                if (isNut)
                {
                    createNutSegment(firstNutIndex, currentSegment.Count - 1);
                    currentSegment.FretPoints[firstNutIndex] = currentSegment.FretPoints[firstNutIndex].Clone();
                    currentSegment.FretPoints.RemoveRange(firstNutIndex + 1, currentSegment.Count - firstNutIndex - 1);
                    if (currentSegment.FretPoints.All(y => y.IsReference))
                        segments.Remove(currentSegment);
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
                bassSideEdge = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Bass).Path;
            else
            {
                bassSideEdge = Layout.GetStringMedian(firstPt.StringIndex - 1).Path;
                //todo: only if there is no other strings before
                //bassSideEdge = GetFingerboardEdgeLineFromString(firstPt.StringIndex, FingerboardSide.Bass);
            }

            if (lastPt.StringIndex == Configuration.NumberOfStrings - 1)
                trebSideEdge = (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Treble).Path;
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
                if (segment.FretPoints[0].FretIndex == 0)
                {
                    Trace.WriteLine($"Nut p0: {vectorPoints.First().X} p1: {vectorPoints.Last().X}");
                }
                return new LinearPath(vectorPoints.First(), vectorPoints.Last());
            }

            return new PolyLinePath(vectorPoints);
        }

        private LinearPath GetFingerboardEdgeLineFromString(int stringIndex, FingerboardSide side)
        {
            var stringElem = Layout.GetStringElement(stringIndex);
            var stringPath = (LinearPath)stringElem.Path.Clone();

            var nutMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Nut, side);
            var bridgeMargin = Configuration.Fingerboard.GetMargin(Data.FingerboardEnd.Bridge, side);
            nutMargin += Configuration.StringConfigurations[stringIndex].GetHalfWidth(side, Configuration.Fingerboard.CompensateMarginsForStrings);
            bridgeMargin += Configuration.StringConfigurations[stringIndex].GetHalfWidth(side, Configuration.Fingerboard.CompensateMarginsForStrings);

            var start = stringPath.Start;
            var end = stringPath.End;
            if (side == FingerboardSide.Bass)
            {
                start.X -= nutMargin.NormalizedValue;
                end.X -= bridgeMargin.NormalizedValue;
            }
            else
            {
                start.X += nutMargin.NormalizedValue;
                end.X += bridgeMargin.NormalizedValue;
            }

            stringPath.Start = start;
            stringPath.End = end;   

            return stringPath;
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
                var distance = Math.Abs((dy * (point.X - start.X) - dx * (point.Y - start.Y)) / Math.Sqrt(lengthSquared));
                if (distance > maxDeviation)
                    return false;
            }

            return true;
        }
    }
}
