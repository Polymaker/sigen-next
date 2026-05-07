using SiGen.Layouts.Elements;
using SiGen.Maths;
using SiGen.Paths;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiGen.Layouts.Snapping
{
    [Flags]
    public enum SnapLineType
    {
        None = 0,
        Fret = 1,
        String = 2,
        Fingerboard = 4,
        CenterLine = 8,
    }

    public enum SnapTargetType
    {
        None = 0,
        Intersection = 1,
        Line = 2,
    }

    public readonly record struct LayoutSnapResult(
        bool IsSnapped,
        VectorD Position,
        SnapTargetType TargetType,
        SnapLineType LineTypes,
        double Distance
    )
    {
        public static LayoutSnapResult None { get; } = new(false, VectorD.Empty, SnapTargetType.None, SnapLineType.None, double.PositiveInfinity);
    }

    public sealed class LayoutSnapLine
    {
        public SnapLineType Type { get; }
        public LayoutElement SourceElement { get; }
        public PathBase Path { get; }

        public LayoutSnapLine(SnapLineType type, LayoutElement sourceElement, PathBase path)
        {
            Type = type;
            SourceElement = sourceElement;
            Path = path;
        }
    }

    public sealed class LayoutIntersectionPoint
    {
        public VectorD Position { get; }
        public SnapLineType LineTypes { get; private set; }

        public LayoutIntersectionPoint(VectorD position, SnapLineType lineTypes)
        {
            Position = position;
            LineTypes = lineTypes;
        }

        internal void AddLineTypes(SnapLineType lineTypes)
        {
            LineTypes |= lineTypes;
        }
    }

    public sealed class LayoutSnapData
    {
        public static LayoutSnapData Empty { get; } = new LayoutSnapData(Array.Empty<LayoutSnapLine>(), Array.Empty<LayoutIntersectionPoint>());

        public IReadOnlyList<LayoutSnapLine> Lines { get; }
        public IReadOnlyList<LayoutIntersectionPoint> IntersectionPoints { get; }

        public LayoutSnapData(IReadOnlyList<LayoutSnapLine> lines, IReadOnlyList<LayoutIntersectionPoint> intersectionPoints)
        {
            Lines = lines;
            IntersectionPoints = intersectionPoints;
        }

        public LayoutSnapResult TrySnap(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            if (allowedLineTypes == SnapLineType.None || maxDistance < 0)
                return LayoutSnapResult.None;

            var bestIntersection = TrySnapToIntersection(cursorPosition, allowedLineTypes, maxDistance);
            if (bestIntersection.IsSnapped)
                return bestIntersection;

            return TrySnapToLine(cursorPosition, allowedLineTypes, maxDistance);
        }

        private LayoutSnapResult TrySnapToIntersection(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            var best = LayoutSnapResult.None;

            for (int i = 0; i < IntersectionPoints.Count; i++)
            {
                var intersection = IntersectionPoints[i];
                // Only snap to intersection if ALL line types that form it are allowed
                if ((intersection.LineTypes & allowedLineTypes) != intersection.LineTypes)
                    continue;

                var distance = VectorD.Distance(cursorPosition, intersection.Position);
                if (distance > maxDistance)
                    continue;

                if (!best.IsSnapped || distance < best.Distance)
                {
                    best = new LayoutSnapResult(true, intersection.Position, SnapTargetType.Intersection, intersection.LineTypes, distance);
                }
            }

            return best;
        }

        private LayoutSnapResult TrySnapToLine(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            var best = LayoutSnapResult.None;

            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if ((line.Type & allowedLineTypes) == SnapLineType.None)
                    continue;

                if (!line.Path.TrySnapToNearestPoint(cursorPosition, out var projectedPoint, out var distance))
                    continue;

                if (distance > maxDistance)
                    continue;

                if (!best.IsSnapped || distance < best.Distance)
                {
                    best = new LayoutSnapResult(true, projectedPoint, SnapTargetType.Line, line.Type, distance);
                }
            }

            return best;
        }
    }

    internal static class LayoutSnapDataBuilder
    {
        private const double IntersectionMergeTolerance = 0.0001;
        private const double SegmentIntersectionThreshold = 0.01;

        public static LayoutSnapData Build(StringedInstrumentLayout layout)
        {
            var lines = CreateLines(layout).ToList();
            var intersections = CreateIntersections(lines);
            return new LayoutSnapData(lines, intersections);
        }

        private static IEnumerable<LayoutSnapLine> CreateLines(StringedInstrumentLayout layout)
        {
            foreach (var fret in layout.Elements.OfType<FretSegmentElement>())
            {
                if (fret.FretShape != null)
                    yield return new LayoutSnapLine(SnapLineType.Fret, fret, fret.FretShape);
            }

            foreach (var @string in layout.Elements.OfType<StringElement>())
                yield return new LayoutSnapLine(SnapLineType.String, @string, @string.Path);

            foreach (var fingerboardEdge in layout.Elements.OfType<FingerboardEdgeElement>())
                yield return new LayoutSnapLine(SnapLineType.Fingerboard, fingerboardEdge, fingerboardEdge.Path);

            if (layout.Bounds != null)
            {
                const double centerLineMarginCm = 1.0;
                var top = layout.Bounds.Top.NormalizedValue;
                var bottom = layout.Bounds.Bottom.NormalizedValue;

                if (top < bottom)
                    (top, bottom) = (bottom, top);

                var centerLinePath = new LinearPath(
                    new VectorD(0, top + centerLineMarginCm),
                    new VectorD(0, bottom - centerLineMarginCm));

                yield return new LayoutSnapLine(SnapLineType.CenterLine, new GuideLineElement(centerLinePath), centerLinePath);
            }
        }

        private static List<LayoutIntersectionPoint> CreateIntersections(List<LayoutSnapLine> lines)
        {
            var intersections = new List<LayoutIntersectionPoint>();

            for (int i = 0; i < lines.Count; i++)
            {
                var lineA = lines[i];

                for (int j = i + 1; j < lines.Count; j++)
                {
                    var lineB = lines[j];
                    var lineTypes = lineA.Type | lineB.Type;
                    var points = PathOperations.GetIntersections(lineA.Path, lineB.Path, SegmentIntersectionThreshold);

                    for (int k = 0; k < points.Count; k++)
                        MergeIntersection(intersections, points[k], lineTypes);
                }
            }

            return intersections;
        }

        private static void MergeIntersection(List<LayoutIntersectionPoint> intersections, VectorD intersection, SnapLineType lineTypes)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (VectorD.Distance(intersections[i].Position, intersection) <= IntersectionMergeTolerance)
                {
                    intersections[i].AddLineTypes(lineTypes);
                    return;
                }
            }

            intersections.Add(new LayoutIntersectionPoint(intersection, lineTypes));
        }
    }
}
