using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Measuring
{
    public class RectangleM
    {
        public Measure X { get; set; }
        public Measure Y { get; set; }
        public Measure Width { get; set; }
        public Measure Height { get; set; }

        public PointM Location
        {
            get => new(X, Y);
            set { X = value.X; Y = value.Y; }
        }
        public PointM Size
        {
            get => new(Width, Height);
            set { Width = value.X; Height = value.Y; }
        }

        public Measure Left
        {
            get => X;
            set => X = value;
        }

        public Measure Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        public Measure Top
        {
            get => Y;
            set => Y = value;
        }

        public Measure Bottom
        {
            get => Y - Height;
            set => Height = Top - value;
        }

        public PointM Center => Location + new PointM(Width * 0.5m, Height * -0.5m);

        public RectangleM() 
        {
            X = Measure.Zero; Y = Measure.Zero;
            Width = Measure.Zero; Height = Measure.Zero;
        }

        public RectangleM(Measure x, Measure y, Measure width, Measure height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        #region Static Ctors

        public static RectangleM FromLTRB(Measure left, Measure top, Measure right, Measure bottom)
        {
            return new RectangleM(left, top, right - left, top - bottom);
        }

        public static RectangleM BoundingRectangle(IEnumerable<PointM> points)
        {
            Measure minX = Measure.Empty;
            Measure maxX = Measure.Empty;
            Measure minY = Measure.Empty;
            Measure maxY = Measure.Empty;

            foreach (var pt in points)
            {
                if (pt.IsEmpty)
                    continue;
                if (minX.IsEmpty || pt.X < minX)
                    minX = pt.X;
                if (minY.IsEmpty || pt.Y < minY)
                    minY = pt.Y;
                if (maxX.IsEmpty || pt.X > maxX)
                    maxX = pt.X;
                if (maxY.IsEmpty || pt.Y > maxY)
                    maxY = pt.Y;
            }

            return FromLTRB(minX, maxY, maxX, minY);
        }

        public static RectangleM BoundingRectangle(params PointM[] points)
        {
            return BoundingRectangle((IEnumerable<PointM>)points);
        }

        public static RectangleM Combine(RectangleM rect1, RectangleM rect2)
        {
            return FromLTRB(
                Measure.Min(rect1.Left, rect2.Left), 
                Measure.Max(rect1.Top, rect2.Top),
                Measure.Max(rect1.Right, rect2.Right),
                Measure.Min(rect1.Bottom, rect2.Bottom));
        }

        #endregion
    }
}
