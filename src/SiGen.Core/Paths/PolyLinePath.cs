using SiGen.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Paths
{
    public class PolyLinePath : PathBase
    {
        public List<VectorD> Points { get; set; }

        public PolyLinePath()
        {
            Points = new List<VectorD>();
        }

        public PolyLinePath(List<VectorD> points)
        {
            Points = points;
        }

        public void Add(VectorD point) { Points.Add(point); }

        public override VectorD GetFirstPoint()
        {
            return Points[0];
        }

        public override VectorD GetLastPoint()
        {
            return Points[^1];
        }

        public override void FlipHorizontal()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new VectorD(-Points[i].X, Points[i].Y);
            }
        }

        public override void Offset(VectorD offset)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] += offset;
            }
        }

        public override PathBase? Extend(double amount)
        {
            if (Points.Count < 2 || double.IsNaN(amount) || Math.Abs(amount) <= double.Epsilon)
                return new PolyLinePath([.. Points]);

            var newPoints = new List<VectorD>(Points);
            var absAmount = Math.Abs(amount);

            if (amount > 0)
            {
                // Extend at start
                var startDir = (Points[0] - Points[1]).Normalized;
                var newStart = Points[0] + startDir * absAmount;
                newPoints.Insert(0, newStart);

                // Extend at end
                var endDir = (Points[^1] - Points[^2]).Normalized;
                var newEnd = Points[^1] + endDir * absAmount;
                newPoints.Add(newEnd);
            }
            else if (amount < 0)
            {
                // Trim at start
                var trimAmount = absAmount;
                int startIdx = 0;
                while (startIdx < newPoints.Count - 1)
                {
                    var segLen = VectorD.Distance(newPoints[startIdx], newPoints[startIdx + 1]);
                    if (trimAmount < segLen)
                        break;
                    trimAmount -= segLen;
                    startIdx++;
                }
                if (startIdx < newPoints.Count - 1)
                {
                    var dir = (newPoints[startIdx + 1] - newPoints[startIdx]).Normalized;
                    newPoints[startIdx] = newPoints[startIdx] + dir * trimAmount;
                }
                newPoints = newPoints.Skip(startIdx).ToList();

                // Trim at end
                trimAmount = absAmount;
                int endIdx = newPoints.Count - 1;
                while (endIdx > 0)
                {
                    var segLen = VectorD.Distance(newPoints[endIdx], newPoints[endIdx - 1]);
                    if (trimAmount < segLen)
                        break;
                    trimAmount -= segLen;
                    endIdx--;
                }
                if (endIdx > 0)
                {
                    var dir = (newPoints[endIdx - 1] - newPoints[endIdx]).Normalized;
                    newPoints[endIdx] = newPoints[endIdx] + dir * trimAmount;
                }
                newPoints = newPoints.Take(endIdx + 1).ToList();
            }

            return newPoints.Count >= 2 ? new PolyLinePath(newPoints) : null;
        }

        public override PathBase? TrimExtend(TrimExtendSide side, double amount)
        {
            if (Points.Count < 2 || side == TrimExtendSide.None || Math.Abs(amount) <= double.Epsilon)
                return new PolyLinePath([.. Points]);

            var newPoints = new List<VectorD>(Points);

            if (side.HasFlag(TrimExtendSide.Start))
            {
                if (amount > 0)
                {
                    var dir = (newPoints[0] - newPoints[1]).Normalized;
                    newPoints.Insert(0, newPoints[0] + dir * amount);
                }
                else
                {
                    newPoints = TrimStart(newPoints, -amount);
                    if (newPoints == null)
                        return null;
                }
            }

            if (side.HasFlag(TrimExtendSide.End))
            {
                if (amount > 0)
                {
                    var dir = (newPoints[^1] - newPoints[^2]).Normalized;
                    newPoints.Add(newPoints[^1] + dir * amount);
                }
                else
                {
                    newPoints = TrimEnd(newPoints, -amount);
                    if (newPoints == null)
                        return null;
                }
            }

            return newPoints.Count >= 2 ? new PolyLinePath(newPoints) : null;
        }

        private static List<VectorD>? TrimStart(List<VectorD> points, double amount)
        {
            int i = 0;
            while (i < points.Count - 1)
            {
                double segLen = VectorD.Distance(points[i], points[i + 1]);
                if (amount < segLen)
                {
                    var dir = (points[i + 1] - points[i]).Normalized;
                    points[i] = points[i] + dir * amount;
                    return points.Skip(i).ToList();
                }
                amount -= segLen;
                i++;
            }
            return null;
        }

        private static List<VectorD>? TrimEnd(List<VectorD> points, double amount)
        {
            int i = points.Count - 1;
            while (i > 0)
            {
                double segLen = VectorD.Distance(points[i], points[i - 1]);
                if (amount < segLen)
                {
                    var dir = (points[i - 1] - points[i]).Normalized;
                    points[i] = points[i] + dir * amount;
                    return points.Take(i + 1).ToList();
                }
                amount -= segLen;
                i--;
            }
            return null;
        }

        public override bool Intersects(LinearPath line, out VectorD intersection)
        {
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var segment = new LinearPath(Points[i], Points[i + 1]);
                if (LinearPath.Intersects(segment, line, out intersection))
                    return true;
            }
            intersection = VectorD.Empty;
            return false;
        }
    }
}
