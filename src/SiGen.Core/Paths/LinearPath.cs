using SiGen.Maths;
using System;
using System.Numerics;

namespace SiGen.Paths
{
    public class LinearPath : PathBase
    {
        public VectorD Start { get; set; }
        public VectorD End { get; set; }
        public PreciseDouble Length => VectorD.Distance(Start, End);
        public VectorD Direction => (End - Start).Normalized;

        public VectorD Size => VectorD.Abs(End - Start);

        public LinearPath()
        {
            Start = VectorD.Zero;
            End = VectorD.Zero;
        }

        public LinearPath(VectorD start, VectorD end)
        {
            Start = start;
            End = end;
        }

        public override VectorD GetFirstPoint()
        {
            return Start;
        }

        public override VectorD GetLastPoint()
        {
            return End;
        }

        public override void FlipHorizontal()
        {
            Start = new VectorD(-Start.X, Start.Y);
            End = new VectorD(-End.X, End.Y);
        }

        public PathBase Clone() //todo: but in base class
        {
            return new LinearPath(Start, End);
        }

        public VectorD Interpolate(double t) => VectorD.Lerp(Start, End, t);

        public static bool GetIntersectionDistance(LinearPath line1, LinearPath line2, out VectorD uv)
        {
            uv = default;

            VectorD vector = line1.End - line1.Start;
            VectorD vector2 = line2.End - line2.Start;
            VectorD vector3 = line1.Start - line2.Start;

            PreciseDouble num = vector.X * vector2.Y - vector.Y * vector2.X;

            if (num == 0d)
                return false;

            uv.X = (vector3.Y * vector2.X - vector3.X * vector2.Y) / num;
            uv.Y = (vector.X * vector3.Y - vector.Y * vector3.X) / num;

            return true;
        }

        public static bool IsIntersectionValid(VectorD uv)
        {
            return uv.X >= 0 && uv.X <= 1 && uv.Y >= 0 && uv.Y <= 1;
        }

        /// <summary>
        /// Check if two segments intersects and outputs the point of intersection.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="intersection"></param>
        /// <param name="allowOutside">If set to True, the function will return true even if the intersection lays outside of the segments</param>
        /// <returns></returns>
        public static bool Intersects(LinearPath line1, LinearPath line2, out VectorD intersection, bool allowOutside = false)
        {
            intersection = default;

            if (GetIntersectionDistance(line1, line2, out VectorD uv))
            {
                intersection = line1.Start + line1.Direction * line1.Length * uv.X;

                return allowOutside || IsIntersectionValid(uv);
            }

            return false;
        }

        public bool Intersects(LinearPath line, out VectorD intersection, bool allowOutside = false)
        {
            return Intersects(this, line, out intersection, allowOutside);
        }

        public LineD GetEquation() => LineD.FromPoints(Start, End);

        public VectorD GetPointForX(PreciseDouble x)
        {
            return GetEquation().GetPointForX(x);
            //var line2 = new LinePath(new VectorD(x, 100), new VectorD(x, -100));
            //if (Intersects(this, line2, out var result, true))
            //    return result;
            //return VectorD.Empty;
        }

        public VectorD GetPointForY(PreciseDouble y)
        {
            return GetEquation().GetPointForY(y);
            //var line2 = new LinePath(new VectorD(x, 100), new VectorD(x, -100));
            //if (Intersects(this, line2, out var result, true))
            //    return result;
            //return VectorD.Empty;
        }

        public static PreciseDouble GetAngleBetweenLines(LinearPath l1, LinearPath l2)
        {
            return LineD.GetAngleBetweenLines(l1.GetEquation(), l2.GetEquation());
        }

        public override void Offset(VectorD offset)
        {
            Start += offset;
            End += offset;
        }

        #region Snapping

        public enum LineSnapDirection
        {
            Perpendicular,
            Horizontal,
            Vertical
        }

        public VectorD SnapToLine(VectorD pos, LineSnapDirection snapMode = LineSnapDirection.Perpendicular, bool allowOutside = true)
        {
            var equation = GetEquation();
            VectorD result;
            if (snapMode == LineSnapDirection.Horizontal)
            {
                var line2 = new LinearPath(new VectorD(-200, pos.Y), new VectorD(200, pos.Y));
                if (Intersects(this, line2, out result, allowOutside))
                    return result;
            }
            else if (snapMode == LineSnapDirection.Vertical)
            {
                var line2 = new LinearPath(new VectorD(pos.X, 200), new VectorD(pos.X, -200));
                if (Intersects(this, line2, out result, allowOutside))
                    return result;
                //result = equation.GetPointForX(pos.X);
            }
            else
            {
                var perp = LineD.GetPerpendicular(equation, pos);
                equation.Intersects(perp, out result);
            }

            //if (!result.IsEmpty && !infiniteLine)
            //{
            //    var v1 = result - Start;
            //    var v2 = result - End;

            //    if (!v1.Normalized.EqualOrClose(Direction, 0.0001))
            //        return snapMode == LineSnapDirection.Perpendicular ? P1.ToVector() : Vector.Empty;
            //    else if (v1.Length > Length.NormalizedValue)
            //        return snapMode == LineSnapDirection.Perpendicular ? P2.ToVector() : Vector.Empty;

            //    if (!v2.Normalized.EqualOrClose(Direction * -1, 0.0001))
            //        return snapMode == LineSnapDirection.Perpendicular ? P2.ToVector() : Vector.Empty;
            //    else if (v2.Length > Length.NormalizedValue)
            //        return snapMode == LineSnapDirection.Perpendicular ? P1.ToVector() : Vector.Empty;

            //}

            return result;
        }

        #endregion

        public override PathBase? Extend(PreciseDouble amount)
        {
            var dirVec = Direction;
            var start = Start + dirVec * amount * -1;
            var end = End + dirVec * amount;
            return new LinearPath(start, end);
        }
    }
}
