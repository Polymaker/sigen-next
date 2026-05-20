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
        EndPoint = 3,
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
        public bool IsEndPoint { get; private set; }

        public LayoutIntersectionPoint(VectorD position, SnapLineType lineTypes)
        {
            Position = position;
            LineTypes = lineTypes;
        }

        internal void AddLineTypes(SnapLineType lineTypes)
        {
            LineTypes |= lineTypes;
        }

        internal void MarkAsEndPoint()
        {
            IsEndPoint = true;
        }
    }

    public sealed class LayoutEndPoint
    {
        public VectorD Position { get; }
        public SnapLineType LineType { get; }
        public LayoutElement SourceElement { get; }
        public bool IsStart { get; }

        public LayoutEndPoint(VectorD position, SnapLineType lineType, LayoutElement sourceElement, bool isStart)
        {
            Position = position;
            LineType = lineType;
            SourceElement = sourceElement;
            IsStart = isStart;
        }
    }

    public sealed class LayoutSnapData
    {
        public static LayoutSnapData Empty { get; } = new LayoutSnapData(Array.Empty<LayoutSnapLine>(), Array.Empty<LayoutIntersectionPoint>(), Array.Empty<LayoutEndPoint>());

        public IReadOnlyList<LayoutSnapLine> Lines { get; }
        public IReadOnlyList<LayoutIntersectionPoint> IntersectionPoints { get; }
        public IReadOnlyList<LayoutEndPoint> EndPoints { get; }

        public LayoutSnapData(IReadOnlyList<LayoutSnapLine> lines, IReadOnlyList<LayoutIntersectionPoint> intersectionPoints, IReadOnlyList<LayoutEndPoint> endPoints)
        {
            Lines = lines;
            IntersectionPoints = intersectionPoints;
            EndPoints = endPoints;
        }

        public LayoutSnapResult TrySnap(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            if (allowedLineTypes == SnapLineType.None || maxDistance < 0)
                return LayoutSnapResult.None;

            var bestIntersection = TrySnapToIntersection(cursorPosition, allowedLineTypes, maxDistance);
            if (bestIntersection.IsSnapped)
                return bestIntersection;

            var bestEndPoint = TrySnapToEndPoint(cursorPosition, allowedLineTypes, maxDistance);
            if (bestEndPoint.IsSnapped)
                return bestEndPoint;

            return TrySnapToLine(cursorPosition, allowedLineTypes, maxDistance);
        }

        private LayoutSnapResult TrySnapToIntersection(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            var best = LayoutSnapResult.None;

            for (int i = 0; i < IntersectionPoints.Count; i++)
            {
                var intersection = IntersectionPoints[i];

                // For endpoints: snap if ANY of the line types is allowed
                // For regular intersections: snap only if ALL line types are allowed
                bool isAllowed = intersection.IsEndPoint
                    ? (intersection.LineTypes & allowedLineTypes) != SnapLineType.None
                    : (intersection.LineTypes & allowedLineTypes) == intersection.LineTypes;

                if (!isAllowed)
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

        private LayoutSnapResult TrySnapToEndPoint(VectorD cursorPosition, SnapLineType allowedLineTypes, double maxDistance)
        {
            var best = LayoutSnapResult.None;

            for (int i = 0; i < EndPoints.Count; i++)
            {
                var endPoint = EndPoints[i];
                if ((endPoint.LineType & allowedLineTypes) == SnapLineType.None)
                    continue;

                var distance = VectorD.Distance(cursorPosition, endPoint.Position);
                if (distance > maxDistance)
                    continue;

                if (!best.IsSnapped || distance < best.Distance)
                {
                    best = new LayoutSnapResult(true, endPoint.Position, SnapTargetType.EndPoint, endPoint.LineType, distance);
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
        private const double SegmentIntersectionThreshold = 0.02;
        private const double EndPointMergeTolerance = 0.0001;

        public static LayoutSnapData Build(StringedInstrumentLayout layout)
        {
            var lines = CreateLines(layout).ToList();
            var intersections = CreateIntersections(lines);
            var endPoints = CreateEndPoints(lines, intersections);
            return new LayoutSnapData(lines, intersections, endPoints);
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

                yield return new LayoutSnapLine(SnapLineType.CenterLine, new GuideLineElement(GuideLineType.CenterLine, centerLinePath), centerLinePath);
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

        private static List<LayoutEndPoint> CreateEndPoints(List<LayoutSnapLine> lines, List<LayoutIntersectionPoint> intersections)
        {
            var endPoints = new List<LayoutEndPoint>();

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var path = line.Path;

                var startPoint = path.GetFirstPoint();
                var endPoint = path.GetLastPoint();

                // Mark intersections that are also endpoints, or add as standalone endpoint
                if (!MarkIntersectionAsEndPoint(startPoint, intersections))
                    endPoints.Add(new LayoutEndPoint(startPoint, line.Type, line.SourceElement, true));

                if (!MarkIntersectionAsEndPoint(endPoint, intersections))
                    endPoints.Add(new LayoutEndPoint(endPoint, line.Type, line.SourceElement, false));
            }

            return endPoints;
        }

        private static bool MarkIntersectionAsEndPoint(VectorD point, List<LayoutIntersectionPoint> intersections)
        {
            for (int i = 0; i < intersections.Count; i++)
            {
                if (VectorD.Distance(point, intersections[i].Position) <= EndPointMergeTolerance)
                {
                    intersections[i].MarkAsEndPoint();
                    return true;
                }
            }
            return false;
        }
    }
}
