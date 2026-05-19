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
        public double FretBreakAngleThreshold { get; set; } = 5; //todo: put this setting in the configuration
        public double FretSlantDistanceThreshold { get; set; } = 0.07; //todo: put this setting in the configuration

        public double MinimumSlantAngle { get; set; } = 20; // angle at which a fret is too slanted relative to the string and should be broken into a new segment

        public FretsBuilder(StringedInstrumentLayout layout, InstrumentLayoutConfiguration configuration) : base(layout, configuration)
        {
        }

        protected override void ExecuteFirstPass()
        {
            BuildFretSegments();

            AdjustFingerboardEdges();

            if (Configuration.StringConfigurations.Any(x => (x.Frets?.StartingFret ?? 0) != 0))
            {
                foreach (var @string in Layout.Strings)
                    @string.GeneratePath(); //regenerate string paths with updated nut points for strings with starting fret != 0
            }
        }

        private void BuildFretSegments()
        {
            bool useLinearMatching = Configuration.ScaleLength.Mode == ScaleLengthMode.Single && 
                Measure.IsNullOrEmpty(Configuration.ScaleLength.BassTrebleSkew);

            var points = GenerateFretPoints();
            var pointsByString = points
                .GroupBy(p => p.StringIndex)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Interval.Cents).ToList());

            var queue = new Queue<FretPoint>(points.OrderBy(p => p.StringIndex).ThenBy(p => p.Interval.Cents));
            var processed = new HashSet<FretPoint>();

            var segments = new List<FretSegment>();

            while (queue.Count > 0)
            {
                var seed = queue.Dequeue();
                if (processed.Contains(seed)/* || seed.IsReference*/) continue;

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
                    var segmentString = Layout.GetStringElement(currentPoint.StringIndex); //the string that the segment is on
                    var angleRelativeToString = Math.Abs(LinearPath.GetAngleBetweenLines(segmentString.Path, candidateSegmentLine));

                    //If the angle is less than MinimumSlantAngle, the segment is too slanted and a new segment is started.
                    if (angleRelativeToString < MinimumSlantAngle)
                        break;

                    //Layout.GetStringElement(currentPoint.StringIndex).Path.Intersects(candidateSegmentLine, out var inter1, true);

                    //if (currentSegment.Count >= 2)
                    //{
                    //    var lastLine = currentSegment.GetLineFromLastTwoPoints();
                    //    var angleRelativeToLastSegment = Math.Abs(LinearPath.GetAngleBetweenLines(lastLine, candidateSegmentLine));
                    //    //If the angle between consecutive segments exceeds FretBreakAngleThreshold, a new segment is started to avoid sharp bends.
                    //    if (angleRelativeToLastSegment > FretBreakAngleThreshold)
                    //        break;
                    //}

                    currentSegment.AddPoint(match);
                    if (currentSegment.Count > 2 && !ShouldFretBeStraight(currentSegment, FretSlantDistanceThreshold))
                    {
                        // if adding the new point creates a non-linear segment, break the segment here and start a new one from the match point
                        currentSegment.FretPoints.Remove(match);
                        break;
                    }

                    processed.Add(match);
                    currentPoint = match;
                }
            }

            //remove segments that are only reference points (I don't know if it's even possilbe)
            segments.RemoveAll(x => x.FretPoints.All(y => y.IsReference));

            //Split fret segments that have references points between two real points
            SplitSegments(segments);

            //Split segments that are partial nut segments (possible if a string has a starting fret <> 0)
            SplitNutBridgeSegments(segments);

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

            return candidatePoints
                .Where(p => !processed.Contains(p))
                .Where(p => Math.Abs(p.Position.ToVector().Y - currentPos.Y) <= FretSlantDistanceThreshold)
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
                var currentDirection = currentSegment.GetLineFromLastTwoPoints();
                if (nextString.Path.Intersects(currentDirection, out var expectedPosition2, true))
                {
                    var foundPoint =  candidatePoints
                       .Where(p => !processed.Contains(p))
                       .Where(p => VectorD.Distance(p.Position.ToVector(), expectedPosition2) <= FretSlantDistanceThreshold)
                       .MinBy(p => VectorD.Distance(p.Position.ToVector(), expectedPosition2));
                    if (foundPoint != null) return foundPoint;
                }
            }

            return candidatePoints
                    .Where(p => !processed.Contains(p))
                    .Where(p => VectorD.Distance(p.Position.ToVector(), expectedPosition1) <= FretSlantDistanceThreshold) 
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

        private List<FretPoint> GenerateFretPoints()
        {
            var points = new List<FretPoint>();

            int minimumFrets = Configuration.StringConfigurations.Min(x => x.Frets?.StartingFret ?? 0);
            int maximumFrets = Math.Max(Configuration.StringConfigurations.Max(x => x.Frets?.NumberOfFrets ?? 0), Configuration.NumberOfFrets ?? 0);

            //if (maximumFrets == 0) return points;

            for (int i = 0; i < NumberOfStrings; i++)
            {
                var stringConfig = GetStringConfig(i)!;

                bool hasStringFretConfig = stringConfig.Frets != null;

                FretConfigurationBase stringFretConfig = stringConfig.Frets ?? (FretConfigurationBase)Configuration.Frets;
                int? numberOfFrets = stringFretConfig?.NumberOfFrets ?? Configuration.NumberOfFrets;
                if (numberOfFrets == null)
                    continue;

                var temperament = stringFretConfig?.Temperament ?? Configuration.Temperament;
                int etSteps = stringFretConfig?.ETSteps ?? Configuration.Frets.ETSteps ?? 12;
                var intervalConfig = stringFretConfig?.Intervals ?? Configuration.Frets.Intervals;

                int startingFret = stringConfig.Frets?.StartingFret ?? 0;

                var stringElem = Layout.Strings.First(x => x.CourseIndex == i);

                var rootNote = stringConfig.GetPrimaryNote() ?? new NoteAndOctave(NoteName.C, 1);

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
                        var fretPoint = new FretPoint(i, PointM.FromVector(fretPos), interval);

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

                        stringPoints.Add(fretPoint);
                        lastFretIndex = j;
                    }
                }

                if (intervalConfig != null)
                {
                    var intervals = new List<double>();
                    
                    intervals.AddRange(intervalConfig);

                    if (temperament == Temperament.Custom && !intervals.Contains(0))
                        intervals.Insert(0, 0); // ensure there is a nut point if using custom temperament with custom intervals

                    for (int j = 0; j < intervals.Count; j++)
                    {
                        PitchInterval interval = PitchInterval.FromCents(intervals[j]);
                        var fretRatio = 1d / interval.Ratio;
                        var fretPos = stringElem.Path.Interpolate(1d - fretRatio);
                        var fretPoint = new FretPoint(i, PointM.FromVector(fretPos), interval);
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
                var bridgePoint = new FretPoint(i, 999, stringElem.BridgePoint, PitchInterval.FromCents(99999)) { IsBridge = true };
                stringPoints.Add(bridgePoint);

                int curFretIndex = 0;
                //int curFretNumber = 1;
                int curFretNumber = (stringConfig.Frets?.StartingFret ?? 0) + 1;
                stringPoints = [.. stringPoints.OrderBy(x => x.Interval.Cents)];
                foreach (var pt in stringPoints)
                {
                    pt.FretIndex = curFretIndex++;
                    if (!pt.IsReference && !(pt.IsBridge || pt.IsNut))
                        pt.FretNumber = curFretNumber++;
                }

                points.AddRange(stringPoints);
            }
            return points;
        }

        private static double GetCentsForFret(int fretIndex, NoteAndOctave rootNote, Temperament temperament, int etSteps)
        {
            return temperament switch
            {
                Temperament.Equal => rootNote.ToAbsoluteCents() + (fretIndex * (1200.0 / etSteps)),
                Temperament.Just or Temperament.Thidell or Temperament.Pythagorean =>
                    PitchInterval.FromNote(rootNote.Transpose(fretIndex), temperament).Cents,
                _ => throw new ArgumentOutOfRangeException()
            };
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

        private void SplitNutBridgeSegments(List<FretSegment> segments)
        {
            var mixedSegments = segments.Where(x => x.IsPartialNut() || x.IsPartialBridge()).ToList();
            if (mixedSegments.Count == 0) return;

            foreach (var segment in mixedSegments)
            {
                var splitSegments = SplitMixedSegment(segment);
                int index = segments.IndexOf(segment);
                segments.RemoveAt(index);
                segments.InsertRange(index, splitSegments);
            }
        }

        /// <summary>
        /// Splits a segment that contains a mix of nut, bridge and/or normal fret points into
        /// "pure" segments (each containing only one type). Cloned reference delimiter points
        /// are inserted at each split boundary.
        /// </summary>
        private static List<FretSegment> SplitMixedSegment(FretSegment segment)
        {
            static int PointType(FretPoint p) => p.IsNut ? 0 : p.IsBridge ? 2 : 1;

            var realPoints = segment.FretPoints.Where(x => !x.IsReference).ToList();

            // Group consecutive real points by type
            var groups = new List<List<FretPoint>>();
            var currentGroup = new List<FretPoint> { realPoints[0] };
            for (int i = 1; i < realPoints.Count; i++)
            {
                if (PointType(realPoints[i]) == PointType(currentGroup[0]))
                    currentGroup.Add(realPoints[i]);
                else
                {
                    groups.Add(currentGroup);
                    currentGroup = [realPoints[i]];
                }
            }
            groups.Add(currentGroup);

            var result = new List<FretSegment>(groups.Count);
            for (int g = 0; g < groups.Count; g++)
            {
                var newSegment = new FretSegment();

                // leading delimiter: clone the last point of the previous group
                if (g > 0)
                    newSegment.FretPoints.Add(groups[g - 1].Last().Clone());

                newSegment.FretPoints.AddRange(groups[g]);

                // trailing delimiter: clone the first point of the next group
                if (g < groups.Count - 1)
                    newSegment.FretPoints.Add(groups[g + 1].First().Clone());

                result.Add(newSegment);
            }

            return result;
        }

        /// <summary>
        /// Returns the boundary edge (fingerboard edge or string median) on the given side
        /// for the specified string index.
        /// </summary>
        private LinearPath GetFretBoundaryEdge(int stringIndex, FingerboardSide side)
        {
            if (side == FingerboardSide.Bass)
            {
                return stringIndex == 0
                    ? (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Bass).Path
                    : Layout.GetStringMedian(stringIndex - 1).Path;
            }
            else
            {
                return stringIndex == Configuration.NumberOfStrings - 1
                    ? (LinearPath)Layout.GetFingerboardEdge(FingerboardSide.Treble).Path
                    : Layout.GetStringMedian(stringIndex).Path;
            }
        }

        /// <summary>
        /// Builds a typed list of shape points for a fret segment.
        /// Each entry carries its position and the originating <see cref="FretPoint"/> (null for boundary intersections).
        /// </summary>
        private List<(VectorD Position, FretPoint? Source)> BuildFretShapePoints(FretSegment segment)
        {
            var result = new List<(VectorD, FretPoint?)>();

            var firstStringPt = segment.FretPoints.First(x => !x.IsReference);
            var lastStringPt  = segment.FretPoints.Last(x => !x.IsReference);

            var bassEdge   = GetFretBoundaryEdge(segment.FirstStringIndex, FingerboardSide.Bass);
            var trebleEdge = GetFretBoundaryEdge(segment.LastStringIndex,  FingerboardSide.Treble);

            // Bass boundary
            if (segment.FretPoints.Count == 1)
            {
                result.Add((bassEdge.SnapToLine(firstStringPt.Position.ToVector(), LinearPath.LineSnapDirection.Horizontal), null));
            }
            else
            {
                var fretLine = new LinearPath(segment.FretPoints[0].Position.ToVector(), segment.FretPoints[1].Position.ToVector());
                if (fretLine.Intersects(bassEdge, out var inter, true))
                    result.Add((inter, null));
            }

            foreach (var fp in segment.FretPoints.Where(x => !x.IsReference))
                result.Add((fp.Position.ToVector(), fp));

            // Treble boundary
            if (segment.FretPoints.Count == 1)
            {
                result.Add((trebleEdge.SnapToLine(lastStringPt.Position.ToVector(), LinearPath.LineSnapDirection.Horizontal), null));
            }
            else
            {
                var fretLine = new LinearPath(segment.FretPoints[^2].Position.ToVector(), segment.FretPoints[^1].Position.ToVector());
                if (fretLine.Intersects(trebleEdge, out var inter, true))
                    result.Add((inter, null));
            }

            return result;
        }

        private List<VectorD> GetFretShapePoints(FretSegment segment)
            => BuildFretShapePoints(segment).Select(p => p.Position).ToList();

        private PathBase CreateSegmentPath(FretSegment segment)
        {
            var shapeStyle = FretShapeStyle.Polyline;// Configuration.Frets.ShapeStyle;
            var tolerence = segment.IsBridge() ? 0.1 : 0.07; // allow more deviation for bridge segments since they often allow adjustment of the saddle
            if (segment.FretPoints.Count == 1 || ShouldFretBeStraight(segment, tolerence)) 
            {
                var vectorPoints = GetFretShapePoints(segment);
                return new LinearPath(vectorPoints.First(), vectorPoints.Last());
            }

            return shapeStyle switch
            {
                FretShapeStyle.NotchedSpline => CreateNotchedSplinePath(segment),
                FretShapeStyle.Polyline => new PolyLinePath(GetFretShapePoints(segment)),
                _ => CreateSmoothSplinePath(segment),
            };
        }

        public static bool ShouldFretBeStraight(FretSegment segment, double maxDeviationCm)
        {
            var fretPoints = segment.FretPoints.Where(x => !x.IsReference).ToList();
            if (fretPoints.Count <= 2)
                return true;

            var start = fretPoints.First().Position.ToVector();
            var end   = fretPoints.Last().Position.ToVector();
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var lengthSquared = dx * dx + dy * dy;

            if (lengthSquared == 0)
                return false;

            foreach (var pt in fretPoints)
            {
                var v = pt.Position.ToVector();
                var distance = Math.Abs((dy * (v.X - start.X) - dx * (v.Y - start.Y)) / Math.Sqrt(lengthSquared));
                if (distance > maxDeviationCm)
                    return false;
            }

            return true;
        }

        private BezierSplinePath CreateSmoothSplinePath(FretSegment segment)
        {
            const double handleRatio = 0.35;
            var points = BuildFretShapePoints(segment);
            var spline = new BezierSplinePath();

            for (int i = 0; i < points.Count; i++)
            {
                var anchor = points[i].Position;

                VectorD tangent;
                if (i == 0)
                    tangent = (points[1].Position - points[0].Position).Normalized;
                else if (i == points.Count - 1)
                    tangent = (points[^1].Position - points[^2].Position).Normalized;
                else
                    tangent = (points[i + 1].Position - points[i - 1].Position).Normalized;

                double inLength  = i > 0               ? VectorD.Distance(anchor, points[i - 1].Position) * handleRatio : 0;
                double outLength = i < points.Count - 1 ? VectorD.Distance(anchor, points[i + 1].Position) * handleRatio : 0;

                spline.AddPoint(new SplineControlPoint(
                    anchor,
                    anchor - tangent * inLength,
                    anchor + tangent * outLength));
            }

            return spline;
        }

        private BezierSplinePath CreateNotchedSplinePath(FretSegment segment)
        {
            const double handleRatio  = 0.35;
            const double notchHalfWidth = 0.05; // cm


            var spline = new BezierSplinePath();

            // Expand fret points into notch pairs; boundary points pass through unchanged.
            var anchors = new List<(VectorD Position, VectorD Perpendicular, bool IsNotch)>();

            var bassEdge = GetFretBoundaryEdge(segment.FirstStringIndex, FingerboardSide.Bass);
            var trebleEdge = GetFretBoundaryEdge(segment.LastStringIndex, FingerboardSide.Treble);

            var realPoints = segment.FretPoints.Where(p => !p.IsReference).ToList();

            VectorD GetStringPerp(int stringIndex)
            {
                var stringElem = Layout.GetStringElement(stringIndex);
                var dir = stringElem!.Path.Direction;

                return new VectorD(-dir.Y, dir.X).Normalized;
            }

            for (int i = 0; i < realPoints.Count; i++)
            {
                var perpNormal = GetStringPerp(realPoints[i].StringIndex);
                var perpLine = new LinearPath(realPoints[i].Position.ToVector(), realPoints[i].Position.ToVector() + perpNormal);

                if (i == 0)
                {
                    //add the bass boundary point
                    if (bassEdge.Intersects(perpLine, out var bassInter, true))
                        anchors.Add((bassInter, perpNormal, false));
                }

                var ptPos = realPoints[i].Position.ToVector();

                // Real fret point → two notch anchors straddling the string position
                anchors.Add((ptPos - perpNormal * notchHalfWidth, perpNormal, true));
                anchors.Add((ptPos + perpNormal * notchHalfWidth, perpNormal, true));

                if (i == realPoints.Count - 1)
                {
                    //add the treble boundary point 
                    if (trebleEdge.Intersects(perpLine, out var trebInter, true))
                        anchors.Add((trebInter, perpNormal, false));
                }
            }


            for (int i = 0; i < anchors.Count; i++)
            {
                var (anchor, perpendicular, isNotch) = anchors[i];

                double inLength  = i > 0                ? VectorD.Distance(anchor, anchors[i - 1].Position) * handleRatio : 0;
                double outLength = i < anchors.Count - 1 ? VectorD.Distance(anchor, anchors[i + 1].Position) * handleRatio : 0;

                if (isNotch)
                {
                    // Handles along the perpendicular enforce the flat notch section
                    spline.AddPoint(new SplineControlPoint(
                        anchor,
                        anchor - perpendicular * inLength,
                        anchor + perpendicular * outLength));
                }
                else
                {
                    // Boundary points: tangent-based handles (Catmull-Rom style)
                    VectorD prev = i > 0                ? anchors[i - 1].Position : anchor;
                    VectorD next = i < anchors.Count - 1 ? anchors[i + 1].Position : anchor;
                    var tangent = (next - prev).Normalized;

                    spline.AddPoint(new SplineControlPoint(
                        anchor,
                        anchor - tangent * inLength,
                        anchor + tangent * outLength));
                }
            }

            return spline;
        }
    }
}
