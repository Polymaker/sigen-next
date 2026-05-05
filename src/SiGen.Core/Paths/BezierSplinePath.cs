using SiGen.Maths;
using System;
using System.Collections.Generic;

namespace SiGen.Paths
{
    /// <summary>
    /// Represents one anchor point in a Bézier spline, with its incoming and outgoing handles.
    /// </summary>
    public struct SplineControlPoint
    {
        /// <summary>The anchor position on the spline.</summary>
        public VectorD Anchor;

        /// <summary>Handle controlling the curve arriving at this anchor (ignored for the first point).</summary>
        public VectorD InHandle;

        /// <summary>Handle controlling the curve leaving this anchor (ignored for the last point).</summary>
        public VectorD OutHandle;

        public SplineControlPoint(VectorD anchor, VectorD inHandle, VectorD outHandle)
        {
            Anchor = anchor;
            InHandle = inHandle;
            OutHandle = outHandle;
        }

        /// <summary>Creates a control point where both handles are coincident with the anchor (sharp corner).</summary>
        public static SplineControlPoint Sharp(VectorD anchor)
            => new(anchor, anchor, anchor);

        /// <summary>Creates a control point where handles are placed symmetrically along a tangent direction.</summary>
        public static SplineControlPoint Smooth(VectorD anchor, VectorD tangent, double handleLength)
        {
            var offset = tangent.Normalized * handleLength;
            return new(anchor, anchor - offset, anchor + offset);
        }
    }

    /// <summary>
    /// A single cubic Bézier curve segment with four control points.
    /// </summary>
    public readonly struct BezierSegment
    {
        /// <summary>Start anchor.</summary>
        public readonly VectorD P0;

        /// <summary>Start out-handle (controls curve leaving P0).</summary>
        public readonly VectorD P1;

        /// <summary>End in-handle (controls curve arriving at P3).</summary>
        public readonly VectorD P2;

        /// <summary>End anchor.</summary>
        public readonly VectorD P3;

        public BezierSegment(VectorD p0, VectorD p1, VectorD p2, VectorD p3)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        /// <summary>Evaluates the point on the segment at parameter t ∈ [0, 1].</summary>
        public VectorD Interpolate(double t)
        {
            double omt = 1d - t;
            return Math.Pow(omt, 3d) * P0 +
                   3d * Math.Pow(omt, 2d) * t * P1 +
                   3d * omt * Math.Pow(t, 2d) * P2 +
                   Math.Pow(t, 3d) * P3;
        }

        /// <summary>Returns the normalized tangent direction at parameter t ∈ [0, 1].</summary>
        public VectorD GetTangent(double t)
        {
            double omt = 1d - t;
            var derivative = 3d * Math.Pow(omt, 2d) * (P1 - P0) +
                             6d * omt * t * (P2 - P1) +
                             3d * Math.Pow(t, 2d) * (P3 - P2);
            return derivative.Normalized;
        }

        /// <summary>Approximates the arc length of the segment by sampling.</summary>
        public double ApproximateLength(int samples = 50)
        {
            double length = 0;
            VectorD prev = P0;
            for (int i = 1; i <= samples; i++)
            {
                VectorD curr = Interpolate(i / (double)samples);
                length += VectorD.Distance(prev, curr);
                prev = curr;
            }
            return length;
        }

        /// <summary>
        /// Approximates the arc length from t=0 to the given t by sampling.
        /// </summary>
        private double ArcLengthTo(double t, int samples = 50)
        {
            double length = 0;
            VectorD prev = P0;
            for (int i = 1; i <= samples; i++)
            {
                VectorD curr = Interpolate(t * i / samples);
                length += VectorD.Distance(prev, curr);
                prev = curr;
            }
            return length;
        }

        /// <summary>
        /// Finds the parameter t ∈ [0, 1] at which the arc length from the start equals <paramref name="targetLength"/>.
        /// Returns 1 if targetLength exceeds the segment length.
        /// </summary>
        public double FindTForLength(double targetLength, int samples = 50)
        {
            if (targetLength <= 0)
                return 0;

            double totalLength = ApproximateLength(samples);
            if (targetLength >= totalLength)
                return 1;

            double lo = 0, hi = 1;
            for (int i = 0; i < 64; i++)
            {
                double mid = (lo + hi) * 0.5;
                double len = ArcLengthTo(mid, samples);
                if (Math.Abs(len - targetLength) < 1e-10)
                    break;
                if (len < targetLength)
                    lo = mid;
                else
                    hi = mid;
            }
            return (lo + hi) * 0.5;
        }

        /// <summary>
        /// Splits the segment at parameter t using De Casteljau's algorithm.
        /// Returns the left [0,t] and right [t,1] subsegments.
        /// </summary>
        public (BezierSegment left, BezierSegment right) SplitAt(double t)
        {
            var q0 = VectorD.Lerp(P0, P1, t);
            var q1 = VectorD.Lerp(P1, P2, t);
            var q2 = VectorD.Lerp(P2, P3, t);
            var r0 = VectorD.Lerp(q0, q1, t);
            var r1 = VectorD.Lerp(q1, q2, t);
            var s  = VectorD.Lerp(r0, r1, t);
            return (new BezierSegment(P0, q0, r0, s),
                    new BezierSegment(s, r1, q2, P3));
        }

        public bool Intersects(LinearPath line, out VectorD intersection, int sampleCount = 50)
        {
            VectorD prev = Interpolate(0d);
            for (int i = 1; i <= sampleCount; i++)
            {
                VectorD curr = Interpolate(i / (double)sampleCount);
                var seg = new LinearPath(prev, curr);
                if (LinearPath.Intersects(seg, line, out intersection))
                    return true;
                prev = curr;
            }
            intersection = VectorD.Empty;
            return false;
        }
    }

    /// <summary>
    /// A piecewise cubic Bézier spline built from a list of <see cref="SplineControlPoint"/> values.
    /// Each adjacent pair of control points defines one <see cref="BezierSegment"/>.
    /// </summary>
    public class BezierSplinePath : PathBase
    {
        private readonly List<SplineControlPoint> _controlPoints = new();
        private List<BezierSegment>? _segmentCache;

        public IReadOnlyList<SplineControlPoint> ControlPoints => _controlPoints;

        public int SegmentCount => Math.Max(0, _controlPoints.Count - 1);

        public BezierSplinePath() { }

        public BezierSplinePath(IEnumerable<SplineControlPoint> controlPoints)
        {
            _controlPoints.AddRange(controlPoints);
        }

        /// <summary>
        /// Appends a control point to the end of the spline.
        /// </summary>
        public void AddPoint(SplineControlPoint point)
        {
            _controlPoints.Add(point);
            _segmentCache = null;
        }

        /// <summary>
        /// Appends an anchor point with explicit in/out handles.
        /// </summary>
        public void AddPoint(VectorD anchor, VectorD inHandle, VectorD outHandle)
            => AddPoint(new SplineControlPoint(anchor, inHandle, outHandle));

        /// <summary>
        /// Appends an anchor point where both handles are coincident (sharp corner / no curvature influence).
        /// </summary>
        public void AddPoint(VectorD anchor)
            => AddPoint(SplineControlPoint.Sharp(anchor));

        /// <summary>
        /// Inserts a control point at the given index.
        /// </summary>
        public void InsertPoint(int index, SplineControlPoint point)
        {
            _controlPoints.Insert(index, point);
            _segmentCache = null;
        }

        /// <summary>
        /// Removes the control point at the given index.
        /// </summary>
        public void RemovePoint(int index)
        {
            _controlPoints.RemoveAt(index);
            _segmentCache = null;
        }

        /// <summary>
        /// Replaces the control point at the given index.
        /// </summary>
        public void SetPoint(int index, SplineControlPoint point)
        {
            _controlPoints[index] = point;
            _segmentCache = null;
        }

        /// <summary>
        /// Returns the precomputed segments, rebuilding the cache if control points have changed.
        /// </summary>
        public IReadOnlyList<BezierSegment> GetSegments()
        {
            if (_segmentCache == null)
                RebuildSegments();
            return _segmentCache!;
        }

        private void RebuildSegments()
        {
            _segmentCache = new List<BezierSegment>(_controlPoints.Count - 1);
            for (int i = 0; i < _controlPoints.Count - 1; i++)
            {
                var a = _controlPoints[i];
                var b = _controlPoints[i + 1];
                _segmentCache.Add(new BezierSegment(a.Anchor, a.OutHandle, b.InHandle, b.Anchor));
            }
        }

        private (BezierSegment segment, double localT) GetSegmentAtT(double t)
        {
            var segments = GetSegments();
            t = MathD.Clamp(t);
            double scaled = t * segments.Count;
            int index = (int)scaled;
            if (index >= segments.Count)
                index = segments.Count - 1;
            return (segments[index], scaled - index);
        }

        /// <summary>
        /// Evaluates the point on the spline at parameter t ∈ [0, 1].
        /// t is distributed evenly across all segments.
        /// </summary>
        public VectorD Interpolate(double t)
        {
            if (SegmentCount == 0)
                return VectorD.Empty;
            var (seg, localT) = GetSegmentAtT(t);
            return seg.Interpolate(localT);
        }

        /// <summary>
        /// Returns the normalized tangent direction at parameter t ∈ [0, 1].
        /// </summary>
        public VectorD GetTangent(double t)
        {
            if (SegmentCount == 0)
                return VectorD.Zero;
            var (seg, localT) = GetSegmentAtT(t);
            return seg.GetTangent(localT);
        }

        public override VectorD GetFirstPoint()
        {
            if (_controlPoints.Count == 0)
                return VectorD.Empty;
            return _controlPoints[0].Anchor;
        }

        public override VectorD GetLastPoint()
        {
            if (_controlPoints.Count == 0)
                return VectorD.Empty;
            return _controlPoints[^1].Anchor;
        }

        public override void Offset(VectorD offset)
        {
            for (int i = 0; i < _controlPoints.Count; i++)
            {
                var cp = _controlPoints[i];
                _controlPoints[i] = new SplineControlPoint(
                    cp.Anchor + offset,
                    cp.InHandle + offset,
                    cp.OutHandle + offset);
            }
            _segmentCache = null;
        }

        public override void FlipHorizontal()
        {
            for (int i = 0; i < _controlPoints.Count; i++)
            {
                var cp = _controlPoints[i];
                _controlPoints[i] = new SplineControlPoint(
                    new VectorD(-cp.Anchor.X, cp.Anchor.Y),
                    new VectorD(-cp.InHandle.X, cp.InHandle.Y),
                    new VectorD(-cp.OutHandle.X, cp.OutHandle.Y));
            }
            _segmentCache = null;
        }

        public override bool Intersects(LinearPath line, out VectorD intersection)
        {
            foreach (var seg in GetSegments())
            {
                if (seg.Intersects(line, out intersection))
                    return true;
            }
            intersection = default;
            return false;
        }

        public override PathBase? TrimExtend(TrimExtendSide side, double amount)
        {
            if (SegmentCount == 0 || side == TrimExtendSide.None || Math.Abs(amount) <= double.Epsilon)
                return Clone();

            var newPoints = new List<SplineControlPoint>(_controlPoints);

            if (amount > 0)
            {
                // Extend: move the endpoint along the tangent, shift the adjacent handle by the same delta
                if (side.HasFlag(TrimExtendSide.Start))
                {
                    var first = newPoints[0];
                    var seg = GetSegments()[0];
                    var tangent = seg.GetTangent(0);
                    var delta = tangent * -amount;
                    newPoints[0] = new SplineControlPoint(
                        first.Anchor + delta,
                        first.InHandle + delta,
                        first.OutHandle + delta);
                }
                if (side.HasFlag(TrimExtendSide.End))
                {
                    var last = newPoints[^1];
                    var seg = GetSegments()[^1];
                    var tangent = seg.GetTangent(1);
                    var delta = tangent * amount;
                    newPoints[^1] = new SplineControlPoint(
                        last.Anchor + delta,
                        last.InHandle + delta,
                        last.OutHandle + delta);
                }
            }
            else
            {
                var trimAmount = -amount;

                if (side.HasFlag(TrimExtendSide.Start))
                {
                    newPoints = TrimStart(newPoints, trimAmount);
                    if (newPoints == null || newPoints.Count < 2)
                        return null;
                }
                if (side.HasFlag(TrimExtendSide.End))
                {
                    newPoints = TrimEnd(newPoints, trimAmount);
                    if (newPoints == null || newPoints.Count < 2)
                        return null;
                }
            }

            return new BezierSplinePath(newPoints);
        }

        private BezierSplinePath Clone() => new(_controlPoints);

        /// <summary>
        /// Trims <paramref name="amount"/> arc-length from the start of the spline control points.
        /// Consumes and discards whole segments until the remaining trim falls within a single segment,
        /// then splits that segment with De Casteljau and rebuilds the control point at the split.
        /// </summary>
        private static List<SplineControlPoint>? TrimStart(List<SplineControlPoint> points, double amount)
        {
            int i = 0;
            while (i < points.Count - 1)
            {
                var seg = new BezierSegment(
                    points[i].Anchor, points[i].OutHandle,
                    points[i + 1].InHandle, points[i + 1].Anchor);

                double segLen = seg.ApproximateLength();
                if (amount < segLen)
                {
                    double t = seg.FindTForLength(amount);
                    var (_, right) = seg.SplitAt(t);
                    // Rebuild the new first control point from the right subsegment
                    // right.P0 = new anchor, right.P1 = new OutHandle, right.P2 = InHandle of next point
                    var newFirst = new SplineControlPoint(right.P0, right.P0, right.P1);
                    var updated = new List<SplineControlPoint>(points.Count - i);
                    updated.Add(newFirst);
                    // Restore the InHandle of the next point from the split (right.P2)
                    var next = points[i + 1];
                    updated.Add(new SplineControlPoint(next.Anchor, right.P2, next.OutHandle));
                    updated.AddRange(points.Skip(i + 2));
                    return updated;
                }

                amount -= segLen;
                i++;
            }
            return null;
        }

        /// <summary>
        /// Trims <paramref name="amount"/> arc-length from the end of the spline control points.
        /// </summary>
        private static List<SplineControlPoint>? TrimEnd(List<SplineControlPoint> points, double amount)
        {
            int i = points.Count - 1;
            while (i > 0)
            {
                var seg = new BezierSegment(
                    points[i - 1].Anchor, points[i - 1].OutHandle,
                    points[i].InHandle, points[i].Anchor);

                double segLen = seg.ApproximateLength();
                if (amount < segLen)
                {
                    double t = seg.FindTForLength(segLen - amount);
                    var (left, _) = seg.SplitAt(t);
                    // left.P3 = new anchor, left.P2 = new InHandle, left.P1 = OutHandle of prev point
                    var newLast = new SplineControlPoint(left.P3, left.P2, left.P3);
                    var updated = new List<SplineControlPoint>(i + 1);
                    updated.AddRange(points.Take(i - 1));
                    // Restore the OutHandle of the previous point from the split (left.P1)
                    var prev = points[i - 1];
                    updated.Add(new SplineControlPoint(prev.Anchor, prev.InHandle, left.P1));
                    updated.Add(newLast);
                    return updated;
                }

                amount -= segLen;
                i--;
            }
            return null;
        }
    }
}
