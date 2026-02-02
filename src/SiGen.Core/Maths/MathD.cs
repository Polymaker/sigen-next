using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Maths
{
    public static class MathD
    {
        // Basic math
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
                maxVal = Max(maxVal, b[i]);
            return maxVal;
        }

        public static PreciseDouble Min(PreciseDouble a, params PreciseDouble[] b)
        {
            PreciseDouble minVal = a;
            for (int i = 0; i < b.Length; i++)
                minVal = Min(minVal, b[i]);
            return minVal;
        }

        // Rounding
        public static PreciseDouble Round(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Round(pd.DoubleValue);
        }

        // Trigonometry
        public static PreciseDouble Cos(PreciseDouble pd)
        {
            if (pd.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Cos(pd.DoubleValue);
            return DecimalMath.Cos(pd.DecimalValue);
        }

        public static PreciseDouble Sin(PreciseDouble pd)
        {
            if (pd.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Sin(pd.DoubleValue);
            return DecimalMath.Sin(pd.DecimalValue);
        }

        public static PreciseDouble Sqrt(PreciseDouble pd)
        {
            if (pd.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Sqrt(pd.DoubleValue);
            return DecimalMath.Sqrt(pd.DecimalValue);
        }

        public static PreciseDouble Acos(PreciseDouble pd)
        {
            return (PreciseDouble)Math.Acos(pd.DoubleValue);
        }

        public static PreciseDouble Atan(PreciseDouble pd)
        {
            if (pd.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Atan(pd.DoubleValue);
            return DecimalMath.Atan(pd.DecimalValue);
            //return (PreciseDouble)Math.Atan(pd.DoubleValue);
        }

        public static PreciseDouble Asin(PreciseDouble pd)
        {
            if (pd.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Asin(pd.DoubleValue);
            return DecimalMath.Asin(pd.DecimalValue);
            //return (PreciseDouble)Math.Asin(pd.DoubleValue);
        }

        public static PreciseDouble Atan2(PreciseDouble y, PreciseDouble x)
        {
            if (x.IsSpecialValue || y.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return (PreciseDouble)Math.Atan2(y.DoubleValue, x.DoubleValue);
            return DecimalMath.Atan2(y.DecimalValue, x.DecimalValue);
        }

        public static PreciseDouble Pow(PreciseDouble x, PreciseDouble y)
        {
            if (x.IsSpecialValue || y.IsSpecialValue || PreciseDouble.DISABLE_DECIMALS)
                return Math.Pow(x.DoubleValue, y.DoubleValue);
            return DecimalMath.Power(x.DecimalValue, y.DecimalValue);
        }

        // Clamping
        public static PreciseDouble Clamp(PreciseDouble x)
        {
            return x > 1d ? 1d : (x < 0 ? 0d : x);
        }

        public static PreciseDouble Clamp(PreciseDouble x, double min, double max)
        {
            return x > max ? max : (x < min ? min : x);
        }

        // Comparison
        public static bool EqualOrClose(this PreciseDouble n1, PreciseDouble n2)
        {
            return EqualOrClose(n1, n2, double.Epsilon);
        }

        public static bool EqualOrClose(this PreciseDouble n1, PreciseDouble n2, PreciseDouble tolerence)
        {
            return Abs(n1 - n2) <= tolerence;
        }

        // Interpolation
        public static PreciseDouble Lerp(PreciseDouble start, PreciseDouble end, PreciseDouble t, bool clamp = true)
        {
            if (clamp)
                t = Max(0.0, Min(1.0, t));
            // Perform linear interpolation
            return start + (end - start) * t;
        }

        public static PreciseDouble InvLerp(PreciseDouble a, PreciseDouble b, PreciseDouble v)
        {
            return (v - a) / (b - a);
        }

        // Mapping
        public static PreciseDouble Map(PreciseDouble iMin, PreciseDouble iMax, PreciseDouble oMin, PreciseDouble oMax, PreciseDouble v, bool clamp = true)
        {
            PreciseDouble t = InvLerp(iMin, iMax, v);
            return Lerp(oMin, oMax, t, clamp);
        }
    }
}
