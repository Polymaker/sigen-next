using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Maths
{
    public static class MathD
    {

        public static double Max(double a, params double[] b)
        {
            double maxVal = a;
            for (int i = 0; i < b.Length; i++)
                maxVal = Math.Max(maxVal, b[i]);
            return maxVal;
        }

        public static double Min(double a, params double[] b)
        {
            double minVal = a;
            for (int i = 0; i < b.Length; i++)
                minVal = Math.Min(minVal, b[i]);
            return minVal;
        }

       

        // Clamping
        public static double Clamp(double x)
        {
            return x > 1d ? 1d : (x < 0 ? 0d : x);
        }

        public static double Clamp(double x, double min, double max)
        {
            return x > max ? max : (x < min ? min : x);
        }

        // Comparison
        public static bool EqualOrClose(this double n1, double n2)
        {
            return EqualOrClose(n1, n2, double.Epsilon);
        }

        public static bool EqualOrClose(this double n1, double n2, double tolerence)
        {
            return Math.Abs(n1 - n2) <= tolerence;
        }

        // Interpolation
        public static double Lerp(double start, double end, double t, bool clamp = true)
        {
            if (clamp)
                t = Max(0.0, Min(1.0, t));
            // Perform linear interpolation
            return start + (end - start) * t;
        }

        public static double InvLerp(double a, double b, double v)
        {
            return (v - a) / (b - a);
        }

        // Mapping
        public static double Map(double iMin, double iMax, double oMin, double oMax, double v, bool clamp = true)
        {
            double t = InvLerp(iMin, iMax, v);
            return Lerp(oMin, oMax, t, clamp);
        }
    }
}
