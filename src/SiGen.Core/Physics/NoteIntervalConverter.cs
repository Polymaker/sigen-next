using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Physics
{
    public static class NoteIntervalConverter
    {
        public static double CentsToRatio(double cents)
        {
            return Math.Pow(2d, cents / 1200d);
        }

        public static double RatioToCents(double ratio)
        {
            return Math.Log(ratio, 2) * 1200d;
        }


    }
}
