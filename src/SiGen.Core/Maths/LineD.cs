using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SiGen.Maths
{
    public struct LineD
    {
        private bool _IsVertical;
        private PreciseDouble _A;
        private PreciseDouble _B;
        private PreciseDouble _X;

        public PreciseDouble A { get { return _A; } }

        public PreciseDouble B { get { return _B; } }

        public PreciseDouble X { get { return _X; } }

        public bool IsVertical { get { return _IsVertical; } }

        public bool IsHorizontal { get { return !IsVertical && A == 0d; } }

        public VectorD Vector
        {
            get
            {
                if (IsVertical)
                    return new VectorD(0, 1);
                return new VectorD(1, A).Normalized;
            }
        }

        public LineD(PreciseDouble x)
        {
            _IsVertical = true;
            _X = x;
            _A = 0;
            _B = 0;
        }

        public LineD(PreciseDouble a, PreciseDouble b)
        {
            _IsVertical = false;
            _X = 0;
            _A = a;
            _B = b;
        }

        public static LineD FromPoints(VectorD p1, VectorD p2)
        {
            var left = p1.X < p2.X ? p1 : p2;
            var right = p1.X < p2.X ? p2 : p1;
            var dx = right.X - left.X;
            var dy = right.Y - left.Y;

            if (dx.EqualOrClose(0, 0.00001))
                return new LineD(p1.X);//vertical line

            var slope = dy / dx;

            if (double.IsInfinity(slope.DoubleValue))
                return new LineD(p1.X);//vertical line

            var b = left.Y + ((left.X * -1) * slope);

            if (slope.EqualOrClose(0, 0.00001))
                slope = 0;//horizontal line

            return new LineD(slope, b);
        }

        #region Functions

        public VectorD GetPointForX(PreciseDouble x)
        {
            if (IsVertical)
                return VectorD.Empty;
            return new VectorD(x, B + (x * A));
        }

        public VectorD GetPointForY(PreciseDouble y)
        {
            if (IsVertical)
                return new VectorD(X, y);
            else if (IsHorizontal)
                return VectorD.Empty;
            return new VectorD((y - B) / A, y);
        }

        public static bool AreParallel(LineD l1, LineD l2)
        {
            if (l1.IsVertical || l2.IsVertical)
                return l1.IsVertical && l2.IsVertical;

            return MathD.EqualOrClose(l1.A, l2.A);
        }

        public static bool ArePerpendicular(LineD l1, LineD l2)
        {
            if ((l1.IsHorizontal && l2.IsVertical) || (l2.IsHorizontal && l1.IsVertical))
                return true;
            return MathD.EqualOrClose((1d / l1.A) * -1, l2.A) || MathD.EqualOrClose((1d / l2.A) * -1, l1.A);
        }

        public static LineD GetPerpendicular(LineD line, VectorD pt)
        {
            if (line.IsVertical)
                return new LineD(0, pt.Y);
            if (line.IsHorizontal)
                return new LineD(pt.X);
            var newSlope = (1 / line.A) * -1;
            var b = pt.Y - (pt.X * newSlope);
            return new LineD(newSlope, b);
        }

        public LineD GetPerpendicular(VectorD pt)
        {
            return GetPerpendicular(this, pt);
        }

        public static bool LinesIntersect(LineD l1, LineD l2, out VectorD intersection)
        {
            intersection = VectorD.Empty;
            if (AreParallel(l1, l2))
                return false;
            if (l1.IsVertical)
            {
                intersection = l2.GetPointForX(l1.X);
                return true;
            }
            else if (l2.IsVertical)
            {
                intersection = l1.GetPointForX(l2.X);
                return true;
            }
            intersection = new VectorD((l2.B - l1.B) / (l1.A - l2.A), l1.A * (l2.B - l1.B) / (l1.A - l2.A) + l1.B);
            return true;
        }

        public bool Intersects(LineD line, out VectorD intersection)
        {
            return LinesIntersect(this, line, out intersection);
        }

        public bool Intersects(LineD line)
        {
            return LinesIntersect(this, line, out _);
        }

        public VectorD GetIntersection(LineD line)
        {
            VectorD intersection;
            Intersects(line, out intersection);
            return intersection;
        }

        public VectorD GetClosestPointOnLine(VectorD point)
        {
            var perp = GetPerpendicular(this, point);
            VectorD inter;
            if (Intersects(perp, out inter))
                return inter;
            return VectorD.Empty;
        }

        public static PreciseDouble GetAngleBetweenLines(LineD l1, LineD l2)
        {
            // Case 1: both lines vertical
            if (l1.IsVertical && l2.IsVertical)
                return 0m;

            // Case 2: one vertical, one not
            if (l1.IsVertical)
                return AngleFromVerticalToSlope(l2.A);

            if (l2.IsVertical)
                return AngleFromVerticalToSlope(l1.A);

            // General case
            PreciseDouble m1 = l1.A;
            PreciseDouble m2 = l2.A;

            PreciseDouble numerator = MathD.Abs(m2 - m1);
            PreciseDouble denominator = 1 + m1 * m2;

            // Prevent division by zero
            if (denominator == 0)
                return 90m;

            PreciseDouble angleRad = MathD.Atan(numerator / denominator);
            PreciseDouble angleDeg = angleRad * 180d / (double)Math.PI;

            return angleDeg;
        }

        private static PreciseDouble AngleFromVerticalToSlope(PreciseDouble slope)
        {
            if (slope == 0)
                return 90d;

            PreciseDouble angleRad = MathD.Atan(MathD.Abs(1d / slope));
            PreciseDouble angleDeg = angleRad * 180d / (double)Math.PI;

            return angleDeg;
        }

        #endregion
    }

}
