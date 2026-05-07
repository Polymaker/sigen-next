using SiGen.Maths;
using System;
using System.Collections.Generic;

namespace SiGen.Paths
{
    public static class PathOperations
    {
        private const int DefaultCurveSamples = 64;
        private const double DefaultPointMergeEpsilon = 1e-7;

        public static IReadOnlyList<VectorD> GetIntersections(PathBase first, PathBase second, double threshold = 0d)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            var firstSegments = GetLinearizedSegments(first);
            var secondSegments = GetLinearizedSegments(second);

            if (firstSegments.Count == 0 || secondSegments.Count == 0)
                return Array.Empty<VectorD>();

            var intersections = new List<VectorD>();
            var mergeEpsilon = threshold > 0d ? threshold : DefaultPointMergeEpsilon;

            bool samePath = ReferenceEquals(first, second);

            for (int i = 0; i < firstSegments.Count; i++)
            {
                var segA = firstSegments[i];
                if (VectorD.DistanceSquared(segA.Start, segA.End) <= double.Epsilon)
                    continue;

                int startJ = samePath ? i + 1 : 0;
                for (int j = startJ; j < secondSegments.Count; j++)
                {
                    var segB = secondSegments[j];
                    if (VectorD.DistanceSquared(segB.Start, segB.End) <= double.Epsilon)
                        continue;

                    var lineA = new LinearPath(segA.Start, segA.End);
                    var lineB = new LinearPath(segB.Start, segB.End);
                    if (LinearPath.Intersects(lineA, lineB, out var intersection, false, threshold))
                        AddUnique(intersections, intersection, mergeEpsilon);
                }
            }

            return intersections;
        }

        public static bool TrySnapToNearestPoint(PathBase path, VectorD point, out VectorD snappedPoint, out double distance)
        {
            ArgumentNullException.ThrowIfNull(path);

            var segments = GetLinearizedSegments(path);
            if (segments.Count == 0)
            {
                snappedPoint = VectorD.Empty;
                distance = double.NaN;
                return false;
            }

            var bestPoint = VectorD.Empty;
            double bestDistanceSquared = double.MaxValue;

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                var candidate = GetClosestPointOnSegment(seg.Start, seg.End, point);
                var distSq = VectorD.DistanceSquared(point, candidate);
                if (distSq < bestDistanceSquared)
                {
                    bestDistanceSquared = distSq;
                    bestPoint = candidate;
                }
            }

            snappedPoint = bestPoint;
            distance = Math.Sqrt(bestDistanceSquared);
            return true;
        }

        private static VectorD GetClosestPointOnSegment(VectorD start, VectorD end, VectorD point)
        {
            var segment = end - start;
            var lenSq = segment.LengthSquared();
            if (lenSq <= double.Epsilon)
                return start;

            var t = VectorD.Dot(point - start, segment) / lenSq;
            t = Math.Max(0d, Math.Min(1d, t));
            return start + segment * t;
        }

        private static List<LinearizedSegment> GetLinearizedSegments(PathBase path)
        {
            var segments = new List<LinearizedSegment>();

            switch (path)
            {
                case LinearPath line:
                    segments.Add(new LinearizedSegment(line.Start, line.End));
                    break;

                case PolyLinePath poly:
                    for (int i = 0; i < poly.Points.Count - 1; i++)
                        segments.Add(new LinearizedSegment(poly.Points[i], poly.Points[i + 1]));
                    break;

                case BezierPath bezier:
                    AddSampledBezierSegments(segments, bezier.Interpolate, DefaultCurveSamples);
                    break;

                case BezierSplinePath spline:
                    foreach (var seg in spline.GetSegments())
                        AddSampledBezierSegments(segments, seg.Interpolate, DefaultCurveSamples);
                    break;
            }

            return segments;
        }

        private static void AddSampledBezierSegments(List<LinearizedSegment> target, Func<double, VectorD> interpolate, int samples)
        {
            VectorD prev = interpolate(0d);
            for (int i = 1; i <= samples; i++)
            {
                VectorD curr = interpolate(i / (double)samples);
                target.Add(new LinearizedSegment(prev, curr));
                prev = curr;
            }
        }

        private static void AddUnique(List<VectorD> points, VectorD point, double epsilon)
        {
            double epsilonSquared = epsilon * epsilon;
            for (int i = 0; i < points.Count; i++)
            {
                if (VectorD.DistanceSquared(points[i], point) <= epsilonSquared)
                    return;
            }

            points.Add(point);
        }

        private readonly struct LinearizedSegment
        {
            public readonly VectorD Start;
            public readonly VectorD End;

            public LinearizedSegment(VectorD start, VectorD end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
