using SiGen.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Measuring
{
    public class PointM
    {
        public Measure X { get; set; }
        public Measure Y { get; set; }

        public bool IsEmpty => X.IsEmpty && Y.IsEmpty;

        public PointM(Measure x, Measure y)
        {
            X = x;
            Y = y;
        }

        public PointM(PreciseDouble x, PreciseDouble y, LengthUnit unit)
        {
            X = new Measure(unit, x);
            Y = new Measure(unit, y);
        }

        public VectorD ToVector()
        {
            return new VectorD(X.NormalizedValue, Y.NormalizedValue);
        }

        //public Vector2 GetNormalizedVector()
        //{
        //    var v = new Vector2((float)X.NormalizedValue, (float)Y.NormalizedValue );
        //    return Vector2.Normalize(v);
        //}

        #region Arithmetic operators

        public static PointM operator +(PointM a, PointM b)
        {
            return new PointM(a.X + b.X, a.Y + b.Y);
        }

        public static PointM operator -(PointM a, PointM b)
        {
            return new PointM(a.X - b.X, a.Y - b.Y);
        }

        public static PointM operator *(PointM a, decimal b)
        {
            return new PointM(a.X * b, a.Y * b);
        }

        public static PointM operator *(PointM a, VectorD b)
        {
            return new PointM(a.X * b.X, a.Y * b.Y);
        }

        public static PointM operator *(decimal a, PointM b)
        {
            return new PointM(b.X * a, b.Y * a);
        }

        public static PointM operator /(PointM a, decimal b)
        {
            return new PointM(a.X * b, a.Y * b);
        }

        public static PointM operator /(decimal a, PointM b)
        {
            return new PointM(b.X / a, b.Y / a);
        }

        #endregion
    
        public static PointM FromVector(VectorD vector, LengthUnit unit = LengthUnit.Cm)
        {
            return new PointM(
                Measure.FromNormalizedValue(unit, (decimal)vector.X),
                Measure.FromNormalizedValue(unit, (decimal)vector.Y));
        }
    }
}
