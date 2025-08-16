using SiGen.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Measuring
{
    public class Measure : IComparable, IComparable<Measure>
    {
        private PreciseDouble? _CachedValue;
        private PreciseDouble _NormalizedValue;
        private LengthUnit _Unit;

        public LengthUnit Unit
        {
            get => _Unit;
            set
            {
                if (_Unit != value)
                {
                    _Unit = value;
                    _CachedValue = null;
                }
            }
        }

        public PreciseDouble NormalizedValue
        {
            get => _NormalizedValue;
            set
            {
                _NormalizedValue = value;
                _CachedValue = null;
            }
        }

        public PreciseDouble Value
        {
            get
            {
                if (IsEmpty)
                    return 0;

                if (_CachedValue.HasValue)
                    return _CachedValue.Value;

                _CachedValue = ConvertFromNormalizedValue(NormalizedValue, Unit);
                return _CachedValue.Value;
            }
            set
            {
                _NormalizedValue = ConvertToNormalizedValue(value, Unit);
                _CachedValue = value;
            }
        }

        public PreciseDouble this[LengthUnit unit]
        {
            get => ConvertFromNormalizedValue(_NormalizedValue, unit);
            set => _NormalizedValue = ConvertToNormalizedValue(value, unit);
        }

        public bool IsEmpty => _NormalizedValue.IsEmpty;

        public static Measure Zero => new(LengthUnit.Cm, 0);
        public static Measure Empty => new Measure();

        private Measure() 
        {
            _NormalizedValue = PreciseDouble.Empty;
        }

        public Measure(LengthUnit unit, PreciseDouble value)
        {
            _Unit = unit;
            _CachedValue = value;
            _NormalizedValue = ConvertToNormalizedValue(value, unit);
        }

        public static Measure FromNormalizedValue(LengthUnit unit, PreciseDouble value)
        {
            return new Measure() 
            { 
                _Unit = unit, 
                _NormalizedValue = value, 
                _CachedValue = ConvertFromNormalizedValue(value, unit) 
            };
        }

        public static Measure Cm(PreciseDouble value)
        {
            return new Measure(LengthUnit.Cm, value);
        }

        public static Measure Mm(PreciseDouble value)
        {
            return new Measure(LengthUnit.Mm, value);
        }

        public static Measure In(PreciseDouble value)
        {
            return new Measure(LengthUnit.In, value);
        }

        #region Equality operators

        public override bool Equals(object? obj)
        {
            if (obj is Measure measure)
            {
                if (IsEmpty || measure.IsEmpty)
                    return IsEmpty && measure.IsEmpty;
                return NormalizedValue == measure.NormalizedValue;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NormalizedValue);
        }

        public static bool operator ==(Measure? m1, Measure? m2)
        {
            if (m1 is null || m2 is null)
                return ReferenceEquals(m1, m2);
            return m1.Equals(m2);
        }

        public static bool operator !=(Measure? m1, Measure? m2)
        {
            return !(m1 == m2);
        }

        #endregion

        #region Comparison operators

        public static bool operator >(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                return false;
            return m1.NormalizedValue > m2.NormalizedValue;
        }

        public static bool operator <(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                return false;
            return m1.NormalizedValue < m2.NormalizedValue;
        }

        public static bool operator >=(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                return false;
            return m1.NormalizedValue >= m2.NormalizedValue;
        }

        public static bool operator <=(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                return false;
            return m1.NormalizedValue <= m2.NormalizedValue;
        }

        public int CompareTo(Measure? other)
        {
            if (other == null)
                return -1;

            if (this < other)
                return -1;
            else if (this > other)
                return 1;
            else if (this == other)
                return 0;
            if (!IsEmpty)
                return 1;
            if (!other.IsEmpty)
                return -1;
            return 0;
        }

        public int CompareTo(object? other)
        {
            if (other == null)
                return 1;
            if (other is not Measure)
                throw new ArgumentException("Invalid");
            return CompareTo((Measure)other);
        }

        #endregion

        #region Arithmetic operators

        public static Measure operator *(Measure m1, PreciseDouble value)
        {
            return new Measure(m1.Unit, m1.Value * value);
        }

        public static Measure operator *(PreciseDouble value, Measure m1)
        {
            return new Measure(m1.Unit, m1.Value * value);
        }

        public static Measure operator /(Measure m1, PreciseDouble value)
        {
            return new Measure(m1.Unit, m1.Value / value);
        }

        public static Measure operator /(PreciseDouble value, Measure m1)
        {
            return new Measure(m1.Unit, m1.Value / value);
        }

        public static Measure operator +(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                ThrowNaNOperationException();
            return FromNormalizedValue(m1.Unit, m1.NormalizedValue + m2.NormalizedValue);
        }

        public static Measure operator -(Measure m1, Measure m2)
        {
            if (m1.IsEmpty || m2.IsEmpty)
                ThrowNaNOperationException();
            return FromNormalizedValue(m1.Unit, m1.NormalizedValue - m2.NormalizedValue);
        }

        public static Measure operator -(Measure m1)
        {
            if (m1.IsEmpty)
                ThrowNaNOperationException();
            return FromNormalizedValue(m1.Unit, -m1.NormalizedValue);
        }

        private static void ThrowNaNOperationException()
        {
            throw new InvalidOperationException("You cannot perform any operation on an empty measure.");
        }

        #endregion

        public static PointM operator *(Vector2 vector, Measure m1)
        {
            return new PointM(m1 * (decimal)vector.X, m1 * (decimal)vector.Y);
        }

        public static PointM operator *(Measure m1, Vector2 vector)
        {
            return new PointM(m1 * (decimal)vector.X, m1 * (decimal)vector.Y);
        }


        #region Normalized convertion

        public static PreciseDouble ConvertFromNormalizedValue(PreciseDouble value, LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Cm => value,
                LengthUnit.Mm => value / 0.1m,
                LengthUnit.In => value / 2.54m,
                LengthUnit.Ft => value / (2.54m * 12m),
                _ => value
            };
        }

        public static PreciseDouble ConvertToNormalizedValue(PreciseDouble value, LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Cm => value,
                LengthUnit.Mm => value * 0.1m,
                LengthUnit.In => value * 2.54m,
                LengthUnit.Ft => value * (2.54m * 12m),
                _ => value
            };
        }



        #endregion


        #region Functions

        public static Measure Abs(Measure value)
        {
            return FromNormalizedValue(value.Unit, MathD.Abs(value.NormalizedValue));
        }

        public static Measure Max(Measure m1, params Measure[] values)
        {
            PreciseDouble maxVal = values.Max(x => x.NormalizedValue);
            maxVal = MathD.Max(maxVal, m1.NormalizedValue);
            return FromNormalizedValue(m1.Unit, maxVal);
        }

        public static Measure Min(Measure m1, params Measure[] values)
        {
            PreciseDouble minVal = values.Min(x => x.NormalizedValue);
            minVal = MathD.Min(minVal, m1.NormalizedValue);
            return FromNormalizedValue(m1.Unit, minVal);
        }

        #endregion

        public static bool IsNullOrEmpty([NotNullWhen(false)] Measure? measure)
        {
            return measure == null || measure.IsEmpty;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "{empty}";
            return $"{Value:0.####}{Unit}";
        }
        
        public string ToStringFormatted(CultureInfo? culture = null, bool useAbbreviation = false)
        {
            if (IsEmpty) return string.Empty;

            string unitText = Unit switch
            {
                LengthUnit.Mm => "mm",
                LengthUnit.Cm => "cm",
                LengthUnit.In => (useAbbreviation ? Localization.Texts.ResourceManager.GetString(nameof(Localization.Texts.InchAbbreviation), culture) ?? "in" : "\""),
                LengthUnit.Ft =>  (useAbbreviation ? Localization.Texts.ResourceManager.GetString(nameof(Localization.Texts.FeetAbbreviation), culture) ?? "ft" : "'"),
                _ => $"{Unit}" // Fallback for any other unit
            };

            return string.Format(culture, "{0:0.####}{1}", Value, unitText);
        }
    }

    public enum LengthUnit
    {
        Mm,
        Cm,
        In,
        Ft
    }

    public enum UnitMode
    {
        Metric,
        Imperial
    }
}
