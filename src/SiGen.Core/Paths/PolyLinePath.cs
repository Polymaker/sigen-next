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

        public override PathBase? Extend(PreciseDouble amount)
        {
            if (amount > 0)
            {
                var startLine = LineD.FromPoints(Points[0], Points[1]);
                var startDirection = (Points[0] - Points[1]).Normalized;
                var startPerp = startLine.GetPerpendicular(Points[0] + (startDirection * amount));

                if (!startLine.Intersects(startPerp, out var inter1))
                    return null;

                var endLine = LineD.FromPoints(Points[^2], Points[^1]);
                var endDirection = (Points[^1] - Points[^2]).Normalized;
                var endPerp = endLine.GetPerpendicular(Points[^1] + (endDirection * amount));

                if (!endLine.Intersects(endPerp, out var inter2))
                    return null;

            }

            return null;
        }
    }
}
