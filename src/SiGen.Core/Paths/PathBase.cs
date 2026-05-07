using SiGen.Maths;
using System.Collections.Generic;

namespace SiGen.Paths
{
    public abstract class PathBase
    {
        public abstract void Offset(VectorD offset);

        public void Offset(double x, double y)
            => Offset(new VectorD(x, y));

        public abstract VectorD GetFirstPoint();
        public abstract VectorD GetLastPoint();

        public virtual PathBase? Extend(double amount)
        {
            return null;
        }

        public virtual PathBase? TrimExtend(TrimExtendSide side, double amount) 
        { return null; }

        public virtual IReadOnlyList<VectorD> GetIntersections(PathBase other, double threshold = 0d)
        {
            return PathOperations.GetIntersections(this, other, threshold);
        }

        public virtual bool Intersects(PathBase other, out IReadOnlyList<VectorD> intersections, double threshold = 0d)
        {
            intersections = GetIntersections(other, threshold);
            return intersections.Count > 0;
        }

        public virtual bool TrySnapToNearestPoint(VectorD point, out VectorD snappedPoint, out double distance)
        {
            return PathOperations.TrySnapToNearestPoint(this, point, out snappedPoint, out distance);
        }

        public abstract bool Intersects(LinearPath line, out VectorD intersection);

        public abstract void FlipHorizontal();
    }

    [Flags]
    public enum TrimExtendSide
    {
        None = 0,
        Start,
        End
    }
}
