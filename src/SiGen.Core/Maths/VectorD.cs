using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiGen.Maths
{
    public struct VectorD : IEquatable<VectorD>, IFormattable
    {
        /// <summary>The X component of the vector.</summary>
        public PreciseDouble X;

        /// <summary>The Y component of the vector.</summary>
        public PreciseDouble Y;

        public VectorD(PreciseDouble value) : this(value, value)
        {
        }

        public VectorD(PreciseDouble x, PreciseDouble y)
        {
            X = x;
            Y = y;
        }


        /// <summary>Returns a vector whose 2 elements are equal to zero.</summary>
        /// <value>A vector whose two elements are equal to zero (that is, it returns the vector <c>(0,0)</c>.</value>
        public static VectorD Zero
        {
            get => default;
        }

        /// <summary>Gets a vector whose 2 elements are equal to one.</summary>
        /// <value>A vector whose two elements are equal to one (that is, it returns the vector <c>(1,1)</c>.</value>
        public static VectorD One
        {
            get => new VectorD(1.0m);
        }

        /// <summary>Gets the vector (1,0).</summary>
        /// <value>The vector <c>(1,0)</c>.</value>
        public static VectorD UnitX
        {
            get => new VectorD(1.0m, 0.0m);
        }

        /// <summary>Gets the vector (0,1).</summary>
        /// <value>The vector <c>(0,1)</c>.</value>
        public static VectorD UnitY
        {
            get => new VectorD(0.0m, 1.0m);
        }

        public static VectorD Empty
        {
            get => new VectorD(PreciseDouble.Empty, PreciseDouble.Empty);
        }

        public readonly bool IsEmpty => X.IsEmpty && Y.IsEmpty;

        public VectorD Normalized => Normalize(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator +(VectorD left, VectorD right)
        {
            return new VectorD(
                left.X + right.X,
                left.Y + right.Y
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator /(VectorD left, VectorD right)
        {
            return new VectorD(
                left.X / right.X,
                left.Y / right.Y
            );
        }

        /// <summary>Divides the specified vector by a specified scalar value.</summary>
        /// <param name="value1">The vector.</param>
        /// <param name="value2">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <remarks>The <see cref="System.Numerics.Vector2D.op_Division" /> method defines the division operation for <see cref="System.Numerics.Vector2D" /> objects.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator /(VectorD value1, PreciseDouble value2)
        {
            return value1 / new VectorD(value2);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(VectorD left, VectorD right)
        {
            return (left.X == right.X)
                && (left.Y == right.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(VectorD left, VectorD right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator *(VectorD left, VectorD right)
        {
            return new VectorD(
                left.X * right.X,
                left.Y * right.Y
            );
        }

        /// <summary>Multiplies the specified vector by the specified scalar value.</summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        /// <remarks>The <see cref="System.Numerics.Vector2D.op_Multiply" /> method defines the multiplication operation for <see cref="System.Numerics.Vector2D" /> objects.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator *(VectorD left, PreciseDouble right)
        {
            return left * new VectorD(right);
        }

        /// <summary>Multiplies the scalar value by the specified vector.</summary>
        /// <param name="left">The vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        /// <remarks>The <see cref="System.Numerics.Vector2D.op_Multiply" /> method defines the multiplication operation for <see cref="System.Numerics.Vector2D" /> objects.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator *(PreciseDouble left, VectorD right)
        {
            return right * left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator -(VectorD left, VectorD right)
        {
            return new VectorD(
                left.X - right.X,
                left.Y - right.Y
            );
        }

        /// <summary>Negates the specified vector.</summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>The negated vector.</returns>
        /// <remarks>The <see cref="System.Numerics.Vector2D.op_UnaryNegation" /> method defines the unary negation operation for <see cref="System.Numerics.Vector2D" /> objects.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD operator -(VectorD value)
        {
            return Zero - value;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Abs(VectorD value)
        {
            return new VectorD(
                MathD.Abs(value.X),
                MathD.Abs(value.Y)
            );
        }

        /// <summary>Adds two vectors together.</summary>
        /// <param name="left">The first vector to add.</param>
        /// <param name="right">The second vector to add.</param>
        /// <returns>The summed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Add(VectorD left, VectorD right)
        {
            return left + right;
        }

        /// <summary>Restricts a vector between a minimum and a maximum value.</summary>
        /// <param name="value1">The vector to restrict.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The restricted vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Clamp(VectorD value1, VectorD min, VectorD max)
        {
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.
            return Min(Max(value1, min), max);
        }

        /// <summary>Computes the Euclidean distance between the two given points.</summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PreciseDouble Distance(VectorD value1, VectorD value2)
        {
            PreciseDouble distanceSquared = DistanceSquared(value1, value2);
            return (PreciseDouble)Math.Sqrt((double)distanceSquared);
        }

        /// <summary>Returns the Euclidean distance squared between two specified points.</summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance squared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PreciseDouble DistanceSquared(VectorD value1, VectorD value2)
        {
            VectorD difference = value1 - value2;
            return Dot(difference, difference);
        }

        /// <summary>Divides the first vector by the second.</summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Divide(VectorD left, VectorD right)
        {
            return left / right;
        }

        /// <summary>Divides the specified vector by a specified scalar value.</summary>
        /// <param name="left">The vector.</param>
        /// <param name="divisor">The scalar value.</param>
        /// <returns>The vector that results from the division.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Divide(VectorD left, PreciseDouble divisor)
        {
            return left / divisor;
        }

        /// <summary>Returns the dot product of two vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PreciseDouble Dot(VectorD value1, VectorD value2)
        {
            return (value1.X * value2.X)
                 + (value1.Y * value2.Y);
        }

        /// <summary>Performs a linear interpolation between two vectors based on the given weighting.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <param name="amount">A value between 0 and 1 that indicates the weight of <paramref name="value2" />.</param>
        /// <returns>The interpolated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Lerp(VectorD value1, VectorD value2, PreciseDouble amount)
        {
            return (value1 * (1.0m - amount)) + (value2 * amount);
        }

        /// <summary>Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The maximized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Max(VectorD value1, VectorD value2)
        {
            return new VectorD(
                (value1.X > value2.X) ? value1.X : value2.X,
                (value1.Y > value2.Y) ? value1.Y : value2.Y
            );
        }

        /// <summary>Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.</summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The minimized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Min(VectorD value1, VectorD value2)
        {
            return new VectorD(
                (value1.X < value2.X) ? value1.X : value2.X,
                (value1.Y < value2.Y) ? value1.Y : value2.Y
            );
        }

        /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The element-wise product vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Multiply(VectorD left, VectorD right)
        {
            return left * right;
        }

        /// <summary>Multiplies a vector by a specified scalar.</summary>
        /// <param name="left">The vector to multiply.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Multiply(VectorD left, PreciseDouble right)
        {
            return left * right;
        }

        /// <summary>Multiplies a scalar value by a specified vector.</summary>
        /// <param name="left">The scaled value.</param>
        /// <param name="right">The vector.</param>
        /// <returns>The scaled vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Multiply(PreciseDouble left, VectorD right)
        {
            return left * right;
        }

        /// <summary>Negates a specified vector.</summary>
        /// <param name="value">The vector to negate.</param>
        /// <returns>The negated vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Negate(VectorD value)
        {
            return -value;
        }

        /// <summary>Returns a vector with the same direction as the specified vector, but with a length of one.</summary>
        /// <param name="value">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Normalize(VectorD value)
        {
            return value / value.Length();
        }

        /// <summary>Returns the reflection of a vector off a surface that has the specified normal.</summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal of the surface being reflected off.</param>
        /// <returns>The reflected vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Reflect(VectorD vector, VectorD normal)
        {
            PreciseDouble dot = Dot(vector, normal);
            return vector - (2m * dot * normal);
        }

        /// <summary>Returns a vector whose elements are the square root of each of a specified vector's elements.</summary>
        /// <param name="value">A vector.</param>
        /// <returns>The square root vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD SquareRoot(VectorD value)
        {
            return new VectorD(
                (PreciseDouble)Math.Sqrt((double)value.X),
                (PreciseDouble)Math.Sqrt((double)value.Y)
            );
        }

        /// <summary>Subtracts the second vector from the first.</summary>
        /// <param name="left">The first vector.</param>
        /// <param name="right">The second vector.</param>
        /// <returns>The difference vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD Subtract(VectorD left, VectorD right)
        {
            return left - right;
        }

       

        /// <summary>Returns a value that indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if the current instance and <paramref name="obj" /> are equal; otherwise, <see langword="false" />. If <paramref name="obj" /> is <see langword="null" />, the method returns <see langword="false" />.</returns>
        /// <remarks>The current instance and <paramref name="obj" /> are equal if <paramref name="obj" /> is a <see cref="System.Numerics.Vector2D" /> object and their <see cref="System.Numerics.Vector2D.X" /> and <see cref="System.Numerics.Vector2D.Y" /> elements are equal.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return (obj is VectorD other) && Equals(other);
        }

        /// <summary>Returns a value that indicates whether this instance and another vector are equal.</summary>
        /// <param name="other">The other vector.</param>
        /// <returns><see langword="true" /> if the two vectors are equal; otherwise, <see langword="false" />.</returns>
        /// <remarks>Two vectors are equal if their <see cref="System.Numerics.Vector2D.X" /> and <see cref="System.Numerics.Vector2D.Y" /> elements are equal.</remarks>
        public readonly bool Equals(VectorD other)
        {
            return this == other;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>The hash code.</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>Returns the length of the vector.</summary>
        /// <returns>The vector's length.</returns>
        /// <altmember cref="System.Numerics.Vector2D.LengthSquared"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly PreciseDouble Length()
        {
            PreciseDouble lengthSquared = LengthSquared();
            return (PreciseDouble)Math.Sqrt((double)lengthSquared);
        }

        /// <summary>Returns the length of the vector squared.</summary>
        /// <returns>The vector's length squared.</returns>
        /// <remarks>This operation offers better performance than a call to the <see cref="System.Numerics.Vector2D.Length" /> method.</remarks>
        /// <altmember cref="System.Numerics.Vector2D.Length"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly PreciseDouble LengthSquared()
        {
            return Dot(this, this);
        }

        /// <summary>Returns the string representation of the current instance using default formatting.</summary>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using the "G" (general) format string and the formatting conventions of the current thread culture. The "&lt;" and "&gt;" characters are used to begin and end the string, and the current culture's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements.</summary>
        /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using <paramref name="format" /> and the current culture's formatting conventions. The "&lt;" and "&gt;" characters are used to begin and end the string, and the current culture's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
        /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
        public readonly string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>Returns the string representation of the current instance using the specified format string to format individual elements and the specified format provider to define culture-specific formatting.</summary>
        /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
        /// <param name="formatProvider">A format provider that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the current instance.</returns>
        /// <remarks>This method returns a string in which each element of the vector is formatted using <paramref name="format" /> and <paramref name="formatProvider" />. The "&lt;" and "&gt;" characters are used to begin and end the string, and the format provider's <see cref="System.Globalization.NumberFormatInfo.NumberGroupSeparator" /> property followed by a space is used to separate each element.</remarks>
        /// <related type="Article" href="/dotnet/standard/base-types/custom-numeric-format-strings">Custom Numeric Format Strings</related>
        /// <related type="Article" href="/dotnet/standard/base-types/standard-numeric-format-strings">Standard Numeric Format Strings</related>
        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
            sb.Append('<');
            sb.Append(X.ToString(format, formatProvider));
            sb.Append(separator);
            sb.Append(' ');
            sb.Append(Y.ToString(format, formatProvider));
            sb.Append('>');
            return sb.ToString();
        }
    }

}
