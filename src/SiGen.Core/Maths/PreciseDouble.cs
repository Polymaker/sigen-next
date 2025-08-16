using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen.Maths
{
    public struct PreciseDouble : IComparable<PreciseDouble>
    {
        private readonly decimal? _decValue;
        private readonly double? _dblValue;
        public readonly decimal DecimalValue => _decValue ?? 0;

        public readonly double DoubleValue => _dblValue ?? (double)DecimalValue;

        public readonly bool IsEmpty => /*_decValue == null && */_dblValue != null && (double.IsNaN(_dblValue.Value) || _dblValue == double.NaN);

        public readonly bool IsSpecialValue => _dblValue != null;

        public static readonly PreciseDouble Empty = new PreciseDouble(null, double.NaN);

        private PreciseDouble(decimal? dec, double? dbl)
        {
            _decValue = dec;
            _dblValue = dbl;
        }

        public PreciseDouble()
        {
            _decValue = 0;
            _dblValue = null;
        }

        public PreciseDouble(decimal value)
        {
            _decValue = value;
            _dblValue = null;
        }

        public PreciseDouble(double value)
        {
            if (IsSpecialDoubleValue(value))
            {
                _dblValue = value;
                _decValue = null;
            }
            else
            {
                _dblValue = null;
                _decValue = (decimal)value;
            }
        }

        #region Equality operators

        public override bool Equals(object? obj)
        {
            if (obj is PreciseDouble pd)
            {
                if (IsSpecialValue || pd.IsSpecialValue)
                    return DoubleValue == pd.DoubleValue;
                return DecimalValue == pd.DecimalValue;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _decValue.GetHashCode();
        }

        public static bool operator ==(PreciseDouble left, PreciseDouble right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PreciseDouble left, PreciseDouble right)
        {
            return !(left == right);
        }

        #endregion

        #region Arithmetic operators

        public static PreciseDouble operator +(PreciseDouble left, PreciseDouble right)
        {
            if (left.IsEmpty)
                throw new ArgumentException("Cannot perform addition with empty value.", nameof(left));

            if (right.IsEmpty)
                throw new ArgumentException("Cannot perform addition with empty value.", nameof(right));

            return Add(left, right);
        }

        public static PreciseDouble operator -(PreciseDouble left, PreciseDouble right)
        {
            if (left.IsEmpty)
                throw new ArgumentException("Cannot perform substraction with empty value.", nameof(left));

            if (right.IsEmpty)
                throw new ArgumentException("Cannot perform substraction with empty value.", nameof(right));

            return Substract(left, right);
        }

        public static PreciseDouble operator -(PreciseDouble left)
        {
            if (left.IsEmpty)
                throw new ArgumentException("Cannot perform substraction with empty value.", nameof(left));

  
            return left * -1d;
        }

        public static PreciseDouble operator *(PreciseDouble left, PreciseDouble right)
        {
            if (left.IsEmpty)
                throw new ArgumentException("Cannot perform multiplication with empty value.", nameof(left));

            if (right.IsEmpty)
                throw new ArgumentException("Cannot perform multiplication with empty value.", nameof(right));

            return Multiply(left, right);
        }

        public static PreciseDouble operator /(PreciseDouble left, PreciseDouble right)
        {
            if (left.IsEmpty)
                throw new ArgumentException("Cannot perform division with empty value.", nameof(left));

            if (right.IsEmpty)
                throw new ArgumentException("Cannot perform division with empty value.", nameof(right));

            return Divide(left, right);
        }

        public static PreciseDouble Add(PreciseDouble a, PreciseDouble b)
        {
            try
            {
                return new PreciseDouble(a.DecimalValue + b.DecimalValue);
            }
            catch (OverflowException)
            {
                return new PreciseDouble(a.DoubleValue + b.DoubleValue);
            }
        }

        public static PreciseDouble Substract(PreciseDouble a, PreciseDouble b)
        {
            try
            {
                return new PreciseDouble(a.DecimalValue - b.DecimalValue);
            }
            catch (OverflowException)
            {
                return new PreciseDouble(a.DoubleValue - b.DoubleValue);
            }
        }

        public static PreciseDouble Multiply(PreciseDouble a, PreciseDouble b)
        {
            try
            {
                return new PreciseDouble(a.DecimalValue * b.DecimalValue);
            }
            catch (OverflowException)
            {
                return new PreciseDouble(a.DoubleValue * b.DoubleValue);
            }
        }

        public static PreciseDouble Divide(PreciseDouble a, PreciseDouble b)
        {
            try
            {
                return new PreciseDouble(a.DecimalValue / b.DecimalValue);
            }
            catch (OverflowException)
            {
                return new PreciseDouble(a.DoubleValue / b.DoubleValue);
            }
        }

        private static bool IsSpecialDoubleValue(double value)
            => double.IsInfinity(value) || double.IsNaN(value);

        #endregion

        #region Comparison operators

        public int CompareTo(PreciseDouble other)
        {
            return DoubleValue.CompareTo(other.DoubleValue);
        }

        public static bool operator >(PreciseDouble v1, PreciseDouble v2)
        {
            return v1.DoubleValue > v2.DoubleValue;
        }

        public static bool operator <(PreciseDouble v1, PreciseDouble v2)
        {
            return v1.DoubleValue < v2.DoubleValue;
        }

        public static bool operator >=(PreciseDouble v1, PreciseDouble v2)
        {
            return v1.DoubleValue >= v2.DoubleValue;
        }

        public static bool operator <=(PreciseDouble v1, PreciseDouble v2)
        {
            return v1.DoubleValue <= v2.DoubleValue;
        }

        #endregion

        #region Convertion operators

        public static implicit operator PreciseDouble(double value)
        {
            return new PreciseDouble(value);
        }

        public static implicit operator PreciseDouble(decimal value)
        {
            return new PreciseDouble(value);
        }

        public static implicit operator PreciseDouble(int value)
        {
            return new PreciseDouble((decimal)value);
        }

        public static explicit operator double(PreciseDouble value)
        {
            return value.DoubleValue;
        }

        public static explicit operator decimal(PreciseDouble value)
        {
            if (value.IsSpecialValue)
                throw new NotSupportedException($"Cannot cast a value of {value.DoubleValue} to decimal.");
            return value.DecimalValue;
        }

        #endregion

        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            return DoubleValue.ToString(format, formatProvider);
        }
    }
}
