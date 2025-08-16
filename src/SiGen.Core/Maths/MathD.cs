using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Maths
{
    public static class MathD
    {
        public static PreciseDouble Abs(PreciseDouble value)
        {
            if (value < 0)
                return value * -1m;
            return value;
        }

        public static PreciseDouble Max(PreciseDouble a, PreciseDouble b)
        {
            return a > b ? a : b;
        }

        public static PreciseDouble Min(PreciseDouble a, PreciseDouble b)
        {
            return a < b ? a : b;
        }

        public static PreciseDouble Max(PreciseDouble a, params PreciseDouble[] b)
        {
            PreciseDouble maxVal = a;
            for (int i = 0; i < b.Length; i++)
                maxVal = Max(a, b[i]);
            return maxVal;
        }

        public static PreciseDouble Min(PreciseDouble a, params PreciseDouble[] b)
        {
            PreciseDouble minVal = a;
            for (int i = 0; i < b.Length; i++)
                minVal = Min(a, b[i]);
            return minVal;
        }

        public static PreciseDouble Cos(PreciseDouble value)
        {
            return Math.Cos(value.DoubleValue);
        }

        public static PreciseDouble Sin(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Sin(pd.DoubleValue);
        }

        public static PreciseDouble Sqrt(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Sqrt(pd.DoubleValue);
        }

        public static PreciseDouble Acos(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Acos(pd.DoubleValue);
        }

        public static PreciseDouble Atan(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Atan(pd.DoubleValue);
        }

        public static PreciseDouble Asin(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Asin(pd.DoubleValue);
        }

        public static PreciseDouble Atan2(PreciseDouble y, PreciseDouble x)
        {
            return (PreciseDouble)Math.Atan2(y.DoubleValue, x.DoubleValue);
        }

        #region MyRegion

        public static PreciseDouble Pow(PreciseDouble x, PreciseDouble y)
        {
            return Math.Pow(x.DoubleValue, y.DoubleValue);
        }

        public static PreciseDouble Clamp(PreciseDouble x)
        {
            return x > 1d ? 1d : (x < 0 ? 0d : x);
        }

        public static PreciseDouble Clamp(PreciseDouble x, double min, double max)
        {
            return x > max ? max : (x < min ? min : x);
        }

        #endregion

        public static bool EqualOrClose(this PreciseDouble n1, PreciseDouble n2)
        {
            return EqualOrClose(n1, n2, double.Epsilon);
        }

        public static bool EqualOrClose(this PreciseDouble n1, PreciseDouble n2, PreciseDouble tolerence)
        {
            return Abs(n1 - n2) <= tolerence;
        }

        public static PreciseDouble Lerp(PreciseDouble start, PreciseDouble end, PreciseDouble t)
        {
            // Ensure t is clamped between 0 and 1
            t = Max(0.0, Min(1.0, t));

            // Perform linear interpolation
            return start + (end - start) * t;
        }

        public static PreciseDouble InvLerp(PreciseDouble a, PreciseDouble b, PreciseDouble v)
        {
            return (v - a) / (b - a);
        }

        public static PreciseDouble Map(PreciseDouble iMin, PreciseDouble iMax, PreciseDouble oMin, PreciseDouble oMax, PreciseDouble v)
        {
            PreciseDouble t = InvLerp(iMin, iMax, v);
            return Lerp(oMin, oMax, t);
        }

        
    }
}
