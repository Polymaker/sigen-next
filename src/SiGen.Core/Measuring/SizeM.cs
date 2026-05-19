using SiGen.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Measuring
{
    public struct SizeM
    {
        public Measure Width { get; set; }
        public Measure Height { get; set; }

        public bool IsEmpty => Width.IsEmpty && Height.IsEmpty;

        public SizeM(Measure width, Measure height)
        {
            Width = width;
            Height = height;
        }

        public SizeM(double width, double height, LengthUnit unit)
        {
            Width = new Measure(unit, width);
            Height = new Measure(unit, height);
        }

        public VectorD ToVector()
        {
            return new VectorD(Width.NormalizedValue, Height.NormalizedValue);
        }

        #region Arithmetic operators

        public static SizeM operator +(SizeM a, SizeM b)
        {
            return new SizeM(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeM operator -(SizeM a, SizeM b)
        {
            return new SizeM(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeM operator *(SizeM a, double b)
        {
            return new SizeM(a.Width * b, a.Height * b);
        }

        public static SizeM operator *(SizeM a, VectorD b)
        {
            return new SizeM(a.Width * b.X, a.Height * b.Y);
        }

        public static SizeM operator *(double a, SizeM b)
        {
            return new SizeM(b.Width * a, b.Height * a);
        }

        public static SizeM operator /(SizeM a, double b)
        {
            return new SizeM(a.Width / b, a.Height / b);
        }

        public static SizeM operator /(double a, SizeM b)
        {
            return new SizeM(b.Width / a, b.Height / a);
        }

        #endregion

        public static SizeM FromVector(VectorD vector, LengthUnit unit = LengthUnit.Cm)
        {
            return new SizeM(
                Measure.FromNormalizedValue(unit, vector.X),
                Measure.FromNormalizedValue(unit, vector.Y));
        }
    }
}
