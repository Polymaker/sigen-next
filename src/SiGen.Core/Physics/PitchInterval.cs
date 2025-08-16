using SiGen.Measuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Physics
{
    public readonly struct PitchInterval : IComparable<PitchInterval>
    {
        public double Cents { get; }
        public double Ratio { get; } 

        public PitchInterval(double cents, double ratio)
        {
            Cents = cents;
            Ratio = ratio;
        }

        public (NoteName, int) ToNote()
        {
            int totalSemitones = (int)Math.Round(Cents / 100);
            int octave = totalSemitones / 12;
            NoteName note = (NoteName)(totalSemitones % 12);
            return (note, octave);
        }

        public static PitchInterval FromCents(double cents)
        {
            return new PitchInterval(cents, CentsToRatio(cents));
        }

        public static PitchInterval FromRatio(double ratio)
        {
            return new PitchInterval(RatioToCents(ratio), ratio);
        }

        public static PitchInterval FromRatio(Tuple<int, int> ratio)
        {
            return FromRatio(ratio.Item1 / (double)ratio.Item2);
        }

        public static PitchInterval From12TET(int note, int octave)
        {
            return FromCents((note + (octave * 12)) * 100);
        }

        public static PitchInterval FromNote(NoteName note, int octave)
        {
            return From12TET((int)note, octave);
        }

        #region Comparison operators

        public static bool operator >(PitchInterval a, PitchInterval m2)
        {
            return a.Cents > m2.Cents;
        }

        public static bool operator <(PitchInterval a, PitchInterval b)
        {
            return a.Cents < b.Cents;
        }

        public static bool operator >=(PitchInterval a, PitchInterval b)
        {
            return a.Cents >= b.Cents;
        }

        public static bool operator <=(PitchInterval a, PitchInterval b)
        {
            return a.Cents <= b.Cents;
        }

        public int CompareTo(PitchInterval other)
        {
            return Cents.CompareTo(other.Cents);
        }

        public override bool Equals(object? obj)
        {
            if (obj is  PitchInterval other)
                return Cents == other.Cents;
            return false;
        }

        public override int GetHashCode()
        {
            return Cents.GetHashCode();
        }

        public static bool operator ==(PitchInterval left, PitchInterval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PitchInterval left, PitchInterval right)
        {
            return !(left == right);
        }

        #endregion
    
        public static double RatioToCents(double ratio)
        {
            return 1200d * Math.Log(ratio, 2d);
        }

        public static double CentsToRatio(double cents)
        {
            return Math.Pow(2d, cents / 1200d);
        }
    }
}
