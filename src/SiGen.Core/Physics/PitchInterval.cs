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

        public NoteAndOctave ToNote()
        {
            int totalSemitones = (int)Math.Round(Cents / 100);
            int octave = totalSemitones / 12;
            NoteName note = (NoteName)(totalSemitones % 12);
            return new(note, octave);
        }

        #region Static Ctors

        public static PitchInterval FromCents(double cents)
        {
            return new PitchInterval(cents, CentsToRatio(cents));
        }

        public static PitchInterval FromRatio(double ratio)
        {
            return new PitchInterval(RatioToCents(ratio), ratio);
        }

        //public static PitchInterval FromRatio(Tuple<int, int> ratio)
        //{
        //    return FromRatio(ratio.Item1 / (double)ratio.Item2);
        //}

        public static PitchInterval FromNote(NoteAndOctave note, Temperament temperament, int etSteps = 12)
        {
            double totalCents = note.Octave * 1200;
            if (temperament == Temperament.Equal || temperament == Temperament.Thidell)
            {
                double nthRoot = Math.Pow(2.0, 1.0 / etSteps);
                double stepsPerOctave = etSteps / 12.0; // how many ET steps map to a chromatic semitone
                totalCents += RatioToCents(Math.Pow(nthRoot, (int)note.Note));
                if (temperament == Temperament.Thidell)
                    totalCents += ThidellFormulaChromaticOffsets[((int)note.Note) % ThidellFormulaChromaticOffsets.Length];
            }
            else if (temperament == Temperament.Just)
                totalCents += RatioToCents(JustScaleRatios[((int)note.Note) % JustScaleRatios.Length]);
            return FromCents(totalCents + note.CentOffset);
        }

        public static PitchInterval FromNote(NoteAndOctave note)
        {
            return FromNote(note, Temperament.Equal);
        }

        #endregion

        #region Arithmetic operators

        public static PitchInterval operator +(PitchInterval a, PitchInterval b)
        {
            return FromCents(a.Cents + b.Cents);
        }

        public static PitchInterval operator -(PitchInterval a, PitchInterval b)
        {
            return FromCents(a.Cents - b.Cents);
        }

        #endregion

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

        public static double CalculateFrequency(PitchInterval referenceNote, double referenceFrequency, PitchInterval note)
        {
            return referenceFrequency * (note.Ratio / referenceNote.Ratio);
        }

        public static double CalculateFrequency(PitchInterval note)
        {
            return CalculateFrequency(FromNote(A4), A4HZ, note);
        }

        public const double A4HZ = 440d;
        public const double C4HZ = 261.63d;
        private static readonly NoteAndOctave A4 = new(NoteName.A, 4);
        public static readonly double TwelfthRoot = Math.Pow(2d, 1d / 12d);
        public static readonly double[] JustScaleRatios =
        [
            1,//C
            16d/15d,//C#
            9d/8d,//D
            6d/5d,//Eb
            5d/4d,//E
            4d/3d,//F
            7d/5d,//F#
            3d/2d,//G
            8d/5d,//Ab
            5d/3d,//A
            16d/9d,//Bb
            15d/8d,//B
            //2d/1d//C
        ];
        public static readonly double[] ThidellFormulaChromaticOffsets = [2, -4, 2, -4, -2, 0, -4, 4, -4, 0, -4, -1];
        public static readonly double[] DieWohltemperirteChromaticOffsets = [5.9, 1.4, 2, 0.6, -2, 7.8, -1.4, 3.9, 0.2, 0, 3.9, 0];
    }
}
