using Avalonia;
using SiGen.Maths;
using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Utilities
{
    public static class MeasureUtils
    {
        public static double CmToPixels = 37.7952755906; // 1 cm = 37.7952755906 pixels (96 DPI)

        public static Rect ToAvalonia(this RectangleM rectangle, double scale)
        {
            return new Rect(
                (double)rectangle.X.NormalizedValue * scale,
                (double)rectangle.Y.NormalizedValue * scale,
                (double)rectangle.Width.NormalizedValue * scale,
                (double)rectangle.Height.NormalizedValue * scale);
        }

        public static Rect ToAvalonia(this RectangleM rectangle)
        {
            return ToAvalonia(rectangle, CmToPixels);
        }

        public static Point ToAvalonia(this PointM point, double scale)
        {
            return new Point(
                (double)point.X.NormalizedValue * scale,
                (double)point.Y.NormalizedValue * scale);
        }

        public static Point ToAvalonia(this PointM point)
        {
            return ToAvalonia(point, CmToPixels);
        }

        public static Point ToAvalonia(this VectorD point)
        {
            return ToAvalonia(point, CmToPixels);
        }

        public static Point ToAvalonia(this VectorD point, double scale)
        {
            return new Point(
                (double)point.X * scale,
                (double)point.Y * scale);
        }

        public static double ToPixels(this Measure measure)
        {
            return (double)measure.NormalizedValue * CmToPixels;
        }
    }
}
